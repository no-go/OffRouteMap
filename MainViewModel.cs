using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using OffRouteMap.Properties;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace OffRouteMap
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _windowTitle;
        private double _guiZoomFactor;
        private string _statusLine;
        private string _selectedMap;
        private string _cacheRoot;

        private PointLatLng _mouseDownPos;
        private List<PointLatLng> _routePoints;
        private GMapRoute _route;

        private readonly GMapControl _gmapControl;
        
        private readonly FolderDialogService _folderDialogService;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GuiZoomInCommand => new RelayCommand(GuiZoomIn);
        public ICommand GuiZoomOutCommand => new RelayCommand(GuiZoomOut);
        public ICommand BeforeClosingCommand => new RelayCommand(BeforeClosing);
        public ICommand SetCacheRootCommand => new RelayCommand(SetCacheRoot);
        public ICommand RemoveRouteCommand => new RelayCommand(RemoveRoute);
        public ICommand LoadRouteCommand => new RelayCommand(LoadRoute);
        public ICommand SaveRouteCommand => new RelayCommand(SaveRoute);

        public ProviderCollection Items { get; }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (value != null)
                {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public string StatusLine
        {
            get => _statusLine;
            set
            {
                if (value != null)
                {
                    _statusLine = value;
                    OnPropertyChanged(nameof(StatusLine));
                }
            }
        }

        public double GuiZoomFactor
        {
            get { return _guiZoomFactor; }
            set
            {
                if (_guiZoomFactor != value)
                {
                    _guiZoomFactor = value;
                    OnPropertyChanged(nameof(GuiZoomFactor));
                }
            }
        }
        public string SelectedMap
        {
            get => _selectedMap;
            set
            {
                _selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap));
                HandleMapChanges();
            }
        }

        public MainViewModel (GMapControl gmapControl) {

            // @todo make this init more configurable

            // for testing
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            WindowTitle = GetType().Namespace;
            _gmapControl = gmapControl;

            _folderDialogService = new FolderDialogService();
            _cacheRoot = Settings.Default.cacheRoot;
            GuiZoomFactor = Settings.Default.guiSize;

            Items = new ProviderCollection(new[]
            {
                new ProviderItem("OSM",          "OpenStreetMap", OpenStreetMapProvider.Instance),
                new ProviderItem("googlemaps",   "Google Maps",   GMapProviders.GoogleMap),
                new ProviderItem("opencyclemap", "Cycle Maps",    GMapProviders.OpenCycleMap),
                new ProviderItem("BingHybrid",   "Bing Hybrid",   GMapProviders.BingHybridMap)
            });
            SelectedMap = Settings.Default.lastMap;
            _gmapControl.Zoom = Settings.Default.lastZoom;
            _gmapControl.ShowCenter = false;
            _gmapControl.CanDragMap = true;
            _gmapControl.DragButton = MouseButton.Left;

            _gmapControl.Position = new PointLatLng(
                Settings.Default.lastLatitude,
                Settings.Default.lastLongitude
            );

            _gmapControl.MouseDoubleClick += OnMouseDoubleDownClick;
            _gmapControl.MouseRightButtonDown += OnMouseRightClick;
            _gmapControl.MouseMove += OnMouseMove;
            OnPositionChanged(_gmapControl.Position);
        }

        protected void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GuiZoomIn ()
        {
            GuiZoomFactor *= 1.1;
            Settings.Default.guiSize = _guiZoomFactor;
            Settings.Default.Save();
        }

        public void GuiZoomOut ()
        {
            GuiZoomFactor /= 1.1;
            Settings.Default.guiSize = _guiZoomFactor;
            Settings.Default.Save();
        }

        public void SetCacheRoot ()
        {
            var path = _folderDialogService.ShowSelectFolderDialog(
                Strings.FolderDialog_Title,
                _cacheRoot
            );
            if (path != null && path != _cacheRoot)
            {
                _cacheRoot = path;
                Settings.Default.cacheRoot = _cacheRoot;
                Settings.Default.Save();
                HandleMapChanges();
            }
        }

        private void HandleMapChanges ()
        {
            if (_cacheRoot == "")
            {
                _cacheRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Maps"
                );
            }
            string cachePath = Path.Combine(_cacheRoot, _selectedMap);
            _gmapControl.Manager.PrimaryCache = new FileCacheProvider(cachePath);
            _gmapControl.Manager.Mode = AccessMode.ServerAndCache;

            if (Items.TryGet(_selectedMap, out var item))
            {
                _gmapControl.MapProvider = item.Provider;
            }
        }

        private void BeforeClosing ()
        {
            Settings.Default.lastLatitude = _gmapControl.Position.Lat;
            Settings.Default.lastLongitude = _gmapControl.Position.Lng;
            Settings.Default.lastZoom = (int)_gmapControl.Zoom;
            Settings.Default.lastMap = _selectedMap;
            Settings.Default.Save();
        }

        private void OnPositionChanged (PointLatLng point)
        {
            string formattedLat = point.Lat.ToString("F6", CultureInfo.InvariantCulture);
            string formattedLng = point.Lng.ToString("F6", CultureInfo.InvariantCulture);

            double distance = RouteLengthKm();
            if (distance > 0)
            {
                string formattedDist = distance.ToString("F4", CultureInfo.InvariantCulture);
                StatusLine = $"Lat Lng Route: {formattedLat} {formattedLng} {formattedDist} km";
            }
            else
            {
                StatusLine = $"Lat Lng: {formattedLat} {formattedLng}";
            }
        }

        private void RemoveRoute()
        {
            if (_route != null)
            {
                _gmapControl.Markers.Remove(_route);
                _route = null;
            }
            _routePoints = new List<PointLatLng>();
            StatusLine = "";
        }

        private void SaveRoute ()
        {
            if (_routePoints == null || _routePoints.Count == 0) return;

            var dlg = new VistaSaveFileDialog
            {
                Title = Strings.SaveDialog_Title,
                Filter = Strings.Dialog_Filetype + " (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = "route.txt",
                OverwritePrompt = true
            };

            bool? result = dlg.ShowDialog();
            if (result != true) return;

            var culture = CultureInfo.InvariantCulture;

            var sb = new StringBuilder();
            double distance = 0.0;
            for (int i = 0; i < _routePoints.Count; i++)
            {
                double lat = _routePoints[i].Lat;
                double lng = _routePoints[i].Lng;
                if (i > 0)
                {
                    distance += DistanceKm(_routePoints[i - 1], _routePoints[i]);
                }

                sb.AppendLine(string.Format(culture, "{0:F6},{1:F6},{2:F4}km", lat, lng, distance));
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
        }

        private void LoadRoute ()
        {
            var dialog = new VistaOpenFileDialog
            {
                Title = Strings.LoadDialog_Title,
                Filter = Strings.Dialog_Filetype + " (*.txt)|*.txt",
                DefaultExt = "txt",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var culture = CultureInfo.InvariantCulture;

                if (_route != null)
                {
                    _gmapControl.Markers.Remove(_route);
                }
                _routePoints = new List<PointLatLng>();

                foreach (var line in File.ReadLines(dialog.FileName))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        continue;
                    }
                    if (
                        double.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, culture, out double v1) &&
                        double.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, culture, out double v2)
                    )
                    {
                        _routePoints.Add(new PointLatLng(v1, v2));
                    }
                    // @todo error handling like a log or StatusLine
                }
                if (_routePoints.Count > 1)
                {
                    _gmapControl.Position = _routePoints.Last();
                    ShowRoute();
                }
            }
        }

        private void ShowRoute ()
        {
            if (_routePoints == null)
            {
                _routePoints = new List<PointLatLng>();
            }

            if (_route != null)
            {
                _gmapControl.Markers.Remove(_route);
            }

            if (_routePoints.Count > 0)
            {
                _route = new GMapRoute(_routePoints);
                _route.Shape = new System.Windows.Shapes.Path()
                {
                    Stroke = new SolidColorBrush(Colors.Blue),
                    StrokeThickness = 2
                };

                _gmapControl.Markers.Add(_route);
            }
        }

        private double DistanceKm (PointLatLng p1, PointLatLng p2)
        {
            const double R = 6371.0; // Erdradius in km
            double lat1 = DegreesToRadians(p1.Lat);
            double lat2 = DegreesToRadians(p2.Lat);
            double dLat = DegreesToRadians(p2.Lat - p1.Lat);
            double dLon = DegreesToRadians(p2.Lng - p1.Lng);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double DegreesToRadians (double deg)
        {
            return deg * (Math.PI / 180.0);
        }

        private double RouteLengthKm ()
        {
            if (_routePoints == null || _routePoints.Count < 2) return 0.0;
            double total = 0.0;
            for (int i = 0; i < _routePoints.Count - 1; i++)
            {
                total += DistanceKm(_routePoints[i], _routePoints[i + 1]);
            }
            return total;
        }

        private void GetMouseLocation (System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition(_gmapControl);
            _mouseDownPos = _gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
        }

        private void OnMouseMove (object sender, System.Windows.Input.MouseEventArgs e)
        {
            GetMouseLocation(e);
            OnPositionChanged(_mouseDownPos);
        }

        private void OnMouseDoubleDownClick (object sender, MouseButtonEventArgs e)
        {
            GetMouseLocation(e);
            if (_routePoints == null)
            {
                _routePoints = new List<PointLatLng>();
            }
            _routePoints.Add(_mouseDownPos);
            OnPositionChanged(_mouseDownPos);
            ShowRoute();
        }

        private void OnMouseRightClick (object sender, MouseButtonEventArgs e)
        {
            GetMouseLocation(e);
            if ((_routePoints != null) && (_routePoints.Count > 0))
            {
                _routePoints.RemoveAt(_routePoints.Count - 1);
                OnPositionChanged(_mouseDownPos);
                ShowRoute();
            }
        }

    }
}
