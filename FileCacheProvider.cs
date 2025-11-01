using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.Concurrent;
using System.IO;

// thanks to GPT5-mini for greate basic help here!

namespace OffRouteMap
{

    public class FileCacheProvider : PureImageCache, IDisposable
    {
        readonly string root;
        readonly ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
        readonly Thread cleanupThread;
        bool disposed;

        public FileCacheProvider (string rootPath)
        {
            root = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            Directory.CreateDirectory(root);
        }

        /// <summary>
        /// Store map tile as file.
        /// </summary>
        /// <param name="tile">raw image bytes (png/jpg)</param>
        /// <param name="type">provider-specific type (can be ignored or used to subfolder)</param>
        /// <param name="pos"></param>
        /// <param name="zoom"></param>
        /// <returns>false on error</returns>
        public bool PutImageToCache (byte[] tile, int type, GPoint pos, int zoom)
        {
            try
            {
                long x = pos.X;
                long y = pos.Y;

                string baseDir = root;
                Directory.CreateDirectory(baseDir);

                // @todo .png always?!
                var path = Path.Combine(baseDir, zoom.ToString(), x.ToString(), y + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                rw.EnterWriteLock();
                try
                {
                    File.WriteAllBytes(path, tile);
                }
                finally { rw.ExitWriteLock(); }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load map tile from file cache.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <param name="zoom"></param>
        /// <returns>tile as pureImage or null on fail</returns>
        public PureImage GetImageFromCache (int type, GPoint pos, int zoom)
        {
            try
            {
                long x = pos.X;
                long y = pos.Y;

                string baseDir = root;

                // @todo .png always?!
                var path = Path.Combine(baseDir, zoom.ToString(), x.ToString(), y + ".png");

                if (!File.Exists(path)) return null;

                rw.EnterReadLock();
                byte[] bytes;
                try
                {
                    bytes = File.ReadAllBytes(path);
                }
                finally { rw.ExitReadLock(); }


                using var stm = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);

                var img = GMapImageProxy.Instance.FromStream(stm);

                if (img != null) img.Data = stm;

                return img;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// delete files older than date; if type != null, limit to that subfolder
        /// returns number of deleted tiles
        /// </summary>
        public int DeleteOlderThan (DateTime date, int? type)
        {
            return 0;
        }

        public void Dispose ()
        {
            disposed = true;
            try { cleanupThread?.Join(500); } catch { }
            rw?.Dispose();
        }
    }
}
