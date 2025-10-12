using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using OffRouteMap.Properties;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using static GMap.NET.Entity.OpenStreetMapRouteEntity;

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

        private readonly MainWindow _mainWindow;
        private readonly ThemeService _themeService;
        private readonly FolderDialogService _folderDialogService;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GuiZoomInCommand => new RelayCommand(GuiZoomIn);
        public ICommand GuiZoomOutCommand => new RelayCommand(GuiZoomOut);
        public ICommand ToggleLightCommand => new RelayCommand(ToggleLight);
        public ICommand BeforeClosingCommand => new RelayCommand(BeforeClosing);
        public ICommand SetCacheRootCommand => new RelayCommand(SetCacheRoot);
        public ICommand RemoveRouteCommand => new RelayCommand(RemoveRoute);
        public ICommand LoadRouteCommand => new RelayCommand(LoadRoute);

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

        public MainViewModel (MainWindow mainWindow) {

            // @todo make this init more configurable

            // for testing
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            WindowTitle = GetType().Namespace;
            _mainWindow = mainWindow;
            _themeService = new ThemeService(140, 220, 178);
            _themeService.ApplyTheme(_mainWindow, Settings.Default.isDark);
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

            _mainWindow.gmapControl.MouseDown += OnMouseDownClick;
            _mainWindow.gmapControl.MouseDoubleClick += OnMouseDoubleDownClick;

            _mainWindow.gmapControl.Position = new PointLatLng(
                Settings.Default.lastLatitude,
                Settings.Default.lastLongitude
            );
            _mainWindow.gmapControl.Zoom = Settings.Default.lastZoom;
            OnPositionChanged(_mainWindow.gmapControl.Position);
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

        public void ToggleLight ()
        {
            Settings.Default.isDark = !Settings.Default.isDark;
            Settings.Default.Save();
            _themeService.ApplyTheme(_mainWindow, Settings.Default.isDark);
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
            _mainWindow.gmapControl.Manager.PrimaryCache = new FileCacheProvider(cachePath);
            _mainWindow.gmapControl.Manager.Mode = AccessMode.ServerAndCache;

            if (Items.TryGet(_selectedMap, out var item))
            {
                _mainWindow.gmapControl.MapProvider = item.Provider;
            }
        }

        private void BeforeClosing ()
        {
            Settings.Default.lastLatitude = _mainWindow.gmapControl.Position.Lat;
            Settings.Default.lastLongitude = _mainWindow.gmapControl.Position.Lng;
            Settings.Default.lastZoom = (int)_mainWindow.gmapControl.Zoom;
            Settings.Default.lastMap = _selectedMap;
            Settings.Default.Save();
        }

        private void OnPositionChanged (PointLatLng point)
        {
            string formattedLat = point.Lat.ToString("F10", CultureInfo.InvariantCulture);
            string formattedLng = point.Lng.ToString("F10", CultureInfo.InvariantCulture);

            double distance = RouteLengthKm();
            if (distance > 0)
            {
                string formattedDist = distance.ToString("F5", CultureInfo.InvariantCulture);
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
                _mainWindow.gmapControl.Markers.Remove(_route);
                _route = null;
            }
            _routePoints = new List<PointLatLng>();
            StatusLine = "";
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
                string selected = dialog.FileName;

                if (_route != null)
                {
                    _mainWindow.gmapControl.Markers.Remove(_route);
                }
                _routePoints = new List<PointLatLng>();

                foreach (var line in File.ReadLines(selected))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
                }
                if (_routePoints.Count > 1)
                {
                    ShowRoute();
                    OnPositionChanged(_routePoints.Last());
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
                _mainWindow.gmapControl.Markers.Remove(_route);
            }

            if (_routePoints.Count > 0)
            {
                _route = new GMapRoute(_routePoints);
                _route.Shape = new System.Windows.Shapes.Path()
                {
                    Stroke = new SolidColorBrush(Colors.Blue),
                    StrokeThickness = 2
                };

                _mainWindow.gmapControl.Markers.Add(_route);
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

        private void OnMouseDownClick (object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(_mainWindow.gmapControl);
            _mouseDownPos = _mainWindow.gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
            OnPositionChanged(_mouseDownPos);
        }

        private void OnMouseDoubleDownClick (object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(_mainWindow.gmapControl);
            _mouseDownPos = _mainWindow.gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
            if (_routePoints == null)
            {
                _routePoints = new List<PointLatLng>();
            }
            _routePoints.Add(_mouseDownPos);
            OnPositionChanged(_mouseDownPos);
            ShowRoute();
        }
    }
}
