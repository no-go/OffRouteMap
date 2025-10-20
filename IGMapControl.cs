using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OffRouteMap
{
    public interface IGMapControl
    {
        double Zoom { get; set; }
        bool ShowCenter { get; set; }
        bool CanDragMap { get; set; }
        MouseButton DragButton { get; set; }
        PointLatLng Position { get; set; }

        GMapProvider MapProvider { get; set; }
        ObservableCollection<GMapMarker> Markers { get; }

        PureImageCache PrimaryCache { get; set; }
        AccessMode CacheMode { get; set; }

        PointLatLng FromLocalToLatLng (int x, int y);
    }
}
