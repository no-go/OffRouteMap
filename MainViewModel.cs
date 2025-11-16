using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using RouteEditorCS.Properties;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace RouteEditorCS
{
    /// <summary>
    /// MainViewModel implements the UI functions joins them with data and files. 
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {

        private string _windowTitle;
        private double _guiZoomFactor;
        private string _statusLine;
        private string _selectedMap;

        private PointLatLng _mouseDownPos;
        private List<PointLatLng> _routePoints;
        private GMapRoute _route;

        private readonly IGMapControl _gmapControl;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GuiZoomInCommand => new RelayCommand(GuiZoomIn);
        public ICommand GuiZoomOutCommand => new RelayCommand(GuiZoomOut);
        public ICommand BeforeClosingCommand => new RelayCommand(BeforeClosing);
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
        protected void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The Model Constructor needs the map control object.
        /// </summary>
        /// <param name="gmapControl">most functions of the UI are focused on the MapControl</param>
        public MainViewModel (IGMapControl gmapControl) {

            // @todo make this init more configurable

            // for testing
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            WindowTitle = GetType().Namespace;
            _gmapControl = gmapControl;

            GuiZoomFactor = Settings.Default.guiSize;

            Items = new ProviderCollection(new[]
            {
                new ProviderItem(
                    "OSM",
                    "OpenStreetMap",
                    OpenStreetMapProvider.Instance
                ),
                new ProviderItem(
                    "googlemaps",
                    "Google Maps",
                    GMapProviders.GoogleMap
                ),
                new ProviderItem(
                    "opencyclemap",
                    "Cycle Maps",
                    GMapProviders.OpenCycleMap
                ),
                new ProviderItem(
                    "BingHybrid",
                    "Bing Hybrid",
                    GMapProviders.BingHybridMap
                )
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

            OnPositionChanged(_gmapControl.Position);
        }

        /// <summary>
        /// UI Button function to make the UI bigger.
        /// </summary>
        public void GuiZoomIn ()
        {
            GuiZoomFactor *= 1.1;
            Settings.Default.guiSize = _guiZoomFactor;
            Settings.Default.Save();
        }

        /// <summary>
        /// UI Button function to make the UI smaller.
        /// </summary>
        public void GuiZoomOut ()
        {
            GuiZoomFactor /= 1.1;
            Settings.Default.guiSize = _guiZoomFactor;
            Settings.Default.Save();
        }

        /// <summary>
        /// This method sets map provider and cache based on the _selectedMap and settings.
        /// </summary>
        private void HandleMapChanges ()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                _gmapControl.CacheMode = AccessMode.ServerAndCache;
            }
            else
            {
                _gmapControl.CacheMode = AccessMode.CacheOnly;
            }

            if (Items.TryGet(_selectedMap, out var item))
            {
                _gmapControl.MapProvider = item.Provider;
            }
        }

        /// <summary>
        /// Store position, map zoom and map provider before closing the application.
        /// This method is called by the UI in the OnClose Event.
        /// </summary>
        private void BeforeClosing ()
        {
            Settings.Default.lastLatitude = _gmapControl.Position.Lat;
            Settings.Default.lastLongitude = _gmapControl.Position.Lng;
            Settings.Default.lastZoom = (int)_gmapControl.Zoom;
            Settings.Default.lastMap = _selectedMap;
            Settings.Default.Save();
        }

        /// <summary>
        /// This method refreshs the statusline with coordinates and route length.
        /// It is called by mouse events, which occurs on the mapControl object.
        /// </summary>
        /// <param name="point">the latitude and longitude calculated by the UI event.</param>
        private void OnPositionChanged (PointLatLng point)
        {
            string formattedLat = point.Lat.ToString("F6", CultureInfo.InvariantCulture);
            string formattedLng = point.Lng.ToString("F6", CultureInfo.InvariantCulture);
            string zoom = _gmapControl.Zoom.ToString("F1", CultureInfo.InvariantCulture);

            double distance = RouteLengthKm();
            if (distance > 0)
            {
                string formattedDist = distance.ToString("F4", CultureInfo.InvariantCulture);
                StatusLine = $"Lat Lng [Zoom] Route: {formattedLat} {formattedLng} [{zoom}] {formattedDist} km";
            }
            else
            {
                StatusLine = $"Lat Lng [Zoom]: {formattedLat} {formattedLng} [{zoom}]";
            }
        }

        /// <summary>
        /// UI Button function to delete the route from the map.
        /// </summary>
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

        /// <summary>
        /// UI Button function to open a save dialog.
        /// </summary>
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

            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// UI Button function to open a file-open dialog to load a route.
        /// </summary>
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

                foreach (var line in System.IO.File.ReadLines(dialog.FileName))
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

        /// <summary>
        /// This method is called on load route or if a new point is added to the route or a point is removed.
        /// </summary>
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

        /// <summary>
        /// Primitive methode to calculate the distance between to positions on the earth.
        /// </summary>
        /// <param name="p1">position 1</param>
        /// <param name="p2">position 2</param>
        /// <returns></returns>
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

        /// <summary>
        /// converter used by DistanceKm() to get positions in radians and not degrees
        /// </summary>
        /// <param name="deg">angle by degrees</param>
        /// <returns>the angle in radians</returns>
        private double DegreesToRadians (double deg)
        {
            return deg * (Math.PI / 180.0);
        }

        /// <summary>
        /// method loops through _routePoints and calculate + sum the route distance.
        /// </summary>
        /// <returns>the complete distance of the route stored in _routePoints</returns>
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

        /// <summary>
        /// Used by UI events to store the map positon on earth in _mouseDownPos
        /// </summary>
        /// <param name="point"></param>
        public void UpdateMousePositionFrom (System.Windows.Point point)
        {
            _mouseDownPos = _gmapControl.FromLocalToLatLng((int)point.X, (int)point.Y);
            OnPositionChanged(_mouseDownPos);
        }

        /// <summary>
        /// Used by UI event on mapControl object and add a point in _routePoints
        /// </summary>
        public void AddRoutePoint ()
        {
            // to modern for mono csc
            //_routePoints ??= new List<PointLatLng>();
            if (_routePoints == null) _routePoints = new List<PointLatLng>();
            _routePoints.Add(_mouseDownPos);
            ShowRoute();
        }

        /// <summary>
        /// Used by UI event on mapControl object and remove a point in _routePoints
        /// </summary>
        public void RemoveLastRoutePoint ()
        {
            if (_routePoints != null && _routePoints.Count > 0)
            {
                _routePoints.RemoveAt(_routePoints.Count - 1);
                ShowRoute();
            }
        }

    }
}
