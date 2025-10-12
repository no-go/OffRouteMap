using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using OffRouteMap.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace OffRouteMap
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _windowTitle;
        private double _guiZoomFactor;
        private string _statusLine;
        private string _selectedMap;
        private string _cacheRoot;

        private readonly MainWindow _mainWindow;
        private readonly ThemeService _themeService;
        private readonly FolderDialogService _folderDialogService;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand GuiZoomInCommand => new RelayCommand(GuiZoomIn);
        public ICommand GuiZoomOutCommand => new RelayCommand(GuiZoomOut);
        public ICommand ToggleLightCommand => new RelayCommand(ToggleLight);
        public ICommand BeforeClosingCommand => new RelayCommand(BeforeClosing);
        public ICommand SetCacheRootCommand => new RelayCommand(SetCacheRoot);

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
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            WindowTitle = GetType().Namespace;
            _mainWindow = mainWindow;
            _themeService = new ThemeService(255, 180, 0);
            _themeService.ApplyTheme(_mainWindow, Settings.Default.isDark);
            _folderDialogService = new FolderDialogService();
            _cacheRoot = Settings.Default.cacheRoot;
            GuiZoomFactor = Settings.Default.guiSize;

            Items = new ProviderCollection(new[]
            {
                new ProviderItem("OSM",        "OpenStreetMap", OpenStreetMapProvider.Instance),
                new ProviderItem("Google",     "Google Maps",   GMapProviders.GoogleMap),
                new ProviderItem("Cycle",      "Cycle Maps",    GMapProviders.OpenCycleMap),
                new ProviderItem("BingHybrid", "Bing Hybrid",   GMapProviders.BingHybridMap)
            });
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
            _mainWindow.lblStatus.Content = $"Lat Lng: {formattedLat} {formattedLng}";
        }



    }
}
