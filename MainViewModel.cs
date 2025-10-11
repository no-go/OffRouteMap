using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using OffRouteMap.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace OffRouteMap
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _windowTitle;
        private double _guiZoomFactor;
        private string _statusLine;
        private string _selectedMap;

        private readonly ThemeService _themeService;
        private readonly MainWindow _mainWindow;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GuiZoomInCommand => new RelayCommand(GuiZoomIn);
        public ICommand GuiZoomOutCommand => new RelayCommand(GuiZoomOut);
        public ICommand ToggleLightCommand => new RelayCommand(ToggleLight);
        public ICommand BeforeClosingCommand => new RelayCommand(BeforeClosing);

        public ObservableCollection<string> Items { get; set; }

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

                // @todo not the elegant way

                if (_selectedMap == "Google")
                {
                    _mainWindow.gmapControl.MapProvider = GMapProviders.GoogleMap;
                }
                else if (_selectedMap == "Cycle")
                {
                    _mainWindow.gmapControl.MapProvider = GMapProviders.OpenCycleMap;
                }
                else if (_selectedMap == "BingHybrid")
                {
                    _mainWindow.gmapControl.MapProvider = GMapProviders.BingHybridMap;
                }
                else
                {
                    _mainWindow.gmapControl.MapProvider = OpenStreetMapProvider.Instance;
                }
            }
        }

        public MainViewModel (MainWindow mainWindow) {

            // @todo make this init more configurable

            WindowTitle = GetType().Namespace;
            this._mainWindow = mainWindow;
            this._themeService = new ThemeService(255, 180, 0);
            this._themeService.ApplyTheme(_mainWindow, Settings.Default.isDark);
            GuiZoomFactor = Settings.Default.guiSize;

            Items = new ObservableCollection<string>
            {
                "OSM",
                "Google",
                "Cycle",
                "BingHybrid"
            };
            SelectedMap = Settings.Default.lastMap;

            _mainWindow.gmapControl.OnPositionChanged += OnPositionChanged;

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
            this._themeService.ApplyTheme(_mainWindow, Settings.Default.isDark);
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
            _mainWindow.lblStatus.Content = $"Lat Lng: {formattedLat} {formattedLng}";
        }



    }
}
