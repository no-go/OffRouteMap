using GMap.NET;
using GMap.NET.MapProviders;
using MahApps.Metro.Controls;

namespace OffRouteMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);

            gmapControl.MapProvider = OpenStreetMapProvider.Instance;
            gmapControl.Position = new PointLatLng(52.5200, 13.4050); // Berlin (Latitude, Longitude)
            gmapControl.Zoom = 10;
        }

    }
}