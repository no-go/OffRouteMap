using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OffRouteMap
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _windowTitle;
        private double _zoomFactor;
        private readonly ThemeService _themeService;
        private readonly FrameworkElement _frameworkElement;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ZoomInCommand => new RelayCommand(ZoomIn);
        public ICommand ZoomOutCommand => new RelayCommand(ZoomOut);
        public ICommand ToggleLightCommand => new RelayCommand(ToggleLight);


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

        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (_zoomFactor != value)
                {
                    _zoomFactor = value;
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }

        public MainViewModel (FrameworkElement frameworkElement) {
            WindowTitle = GetType().Namespace;
            this._frameworkElement = frameworkElement;
            this._themeService = new ThemeService(255, 180, 0);
            this._themeService.ApplyTheme(_frameworkElement, Properties.Settings.Default.isDark);
            ZoomFactor = Properties.Settings.Default.guiSize;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ZoomIn ()
        {
            ZoomFactor *= 1.1;
            Properties.Settings.Default.guiSize = _zoomFactor;
            Properties.Settings.Default.Save();
        }

        public void ZoomOut ()
        {
            ZoomFactor /= 1.1;
            Properties.Settings.Default.guiSize = _zoomFactor;
            Properties.Settings.Default.Save();
        }

        public void ToggleLight()
        {
            Properties.Settings.Default.isDark = !Properties.Settings.Default.isDark;
            Properties.Settings.Default.Save();
            this._themeService.ApplyTheme(_frameworkElement, Properties.Settings.Default.isDark);
        }
    }
}
