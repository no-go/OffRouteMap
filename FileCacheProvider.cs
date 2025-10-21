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
        readonly long maxCacheBytes;
        readonly ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
        readonly ConcurrentQueue<string> cleanupQueue = new ConcurrentQueue<string>();
        readonly Thread cleanupThread;
        bool disposed;

        public FileCacheProvider (string rootPath, long maxCacheBytes = 0)
        {
            root = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            this.maxCacheBytes = maxCacheBytes;
            Directory.CreateDirectory(root);

            if (maxCacheBytes > 0)
            {
                cleanupThread = new Thread(BackgroundCleanup) { IsBackground = true };
                cleanupThread.Start();
            }
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
                //string folder = (type == 0) ? "" : GMapProviders.TryGetProvider(type).Name;
                //var baseDir = string.IsNullOrEmpty(folder) ? root : Path.Combine(root, folder);
                Directory.CreateDirectory(baseDir);

                // @todo .png always?!
                var path = Path.Combine(baseDir, zoom.ToString(), x.ToString(), y + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                rw.EnterWriteLock();
                try
                {
                    File.WriteAllBytes(path, tile);
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                }
                finally { rw.ExitWriteLock(); }

                if (maxCacheBytes > 0) cleanupQueue.Enqueue(path);

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

                //string folder = (type == 0) ? "" : GMapProviders.TryGetProvider(type).Name;
                //var baseDir = string.IsNullOrEmpty(folder) ? root : Path.Combine(root, folder);

                // @todo .png always?!
                var path = Path.Combine(baseDir, zoom.ToString(), x.ToString(), y + ".png");

                if (!File.Exists(path)) return null;

                rw.EnterReadLock();
                byte[] bytes;
                try
                {
                    bytes = File.ReadAllBytes(path);
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                }
                finally { rw.ExitReadLock(); }

                MemoryStream stm = new MemoryStream(bytes, 0, bytes.Length, false, true);

                var img = GMapImageProxy.Instance.FromStream(stm);
                if (img != null)
                {
                    img.Data = stm;
                }

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
            int deleted = 0;
            try
            {
                string baseDir = root;
                //if (type.HasValue && type.Value != 0)
                //    baseDir = Path.Combine(root, GMapProviders.TryGetProvider((int)type).Name);

                if (!Directory.Exists(baseDir)) return 0;

                rw.EnterWriteLock();
                try
                {
                    var files = Directory.EnumerateFiles(baseDir, "*.png", SearchOption.AllDirectories)
                        .Where(f => File.GetLastWriteTimeUtc(f) < date)
                        .ToList();

                    foreach (var f in files)
                    {
                        try
                        {
                            File.Delete(f);
                            deleted++;
                        }
                        catch { }
                    }
                }
                finally { rw.ExitWriteLock(); }
            }
            catch { }
            return deleted;
        }

        /// <summary>
        /// Background cleanup: enforce maxCacheBytes using LRU
        /// </summary>
        void BackgroundCleanup ()
        {
            while (!disposed)
            {
                try
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    if (cleanupQueue.IsEmpty && GetCacheSizeBytes() <= maxCacheBytes) continue;

                    rw.EnterWriteLock();
                    try
                    {
                        var files = Directory.EnumerateFiles(root, "*.png", SearchOption.AllDirectories)
                            .Select(p => new FileInfo(p))
                            .OrderBy(fi => fi.LastWriteTimeUtc)
                            .ToList();

                        long total = files.Sum(f => f.Length);
                        foreach (var fi in files)
                        {
                            if (total <= maxCacheBytes) break;
                            try
                            {
                                total -= fi.Length;
                                fi.Delete();
                            }
                            catch { }
                        }
                    }
                    finally { rw.ExitWriteLock(); }
                }
                catch { Thread.Sleep(1000); }
            }
        }

        long GetCacheSizeBytes ()
        {
            try
            {
                rw.EnterReadLock();
                try
                {
                    if (!Directory.Exists(root)) return 0;
                    return Directory.EnumerateFiles(root, "*.png", SearchOption.AllDirectories)
                        .Sum(f => new FileInfo(f).Length);
                }
                finally { rw.ExitReadLock(); }
            }
            catch { return 0; }
        }

        public void Dispose ()
        {
            disposed = true;
            try { cleanupThread?.Join(500); } catch { }
            rw?.Dispose();
        }
    }
}
