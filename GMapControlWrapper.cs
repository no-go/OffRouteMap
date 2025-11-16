using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows.Input;
namespace RouteEditorCS
{

    public class GMapControlWrapper : IGMapControl
    {
        private readonly GMapControl _control;

        public GMapControlWrapper (GMapControl control)
        {
            _control = control;
        }

        public double Zoom
        {
            get => _control.Zoom;
            set => _control.Zoom = value;
        }

        public bool ShowCenter
        {
            get => _control.ShowCenter;
            set => _control.ShowCenter = value;
        }

        public bool CanDragMap
        {
            get => _control.CanDragMap;
            set => _control.CanDragMap = value;
        }

        public MouseButton DragButton
        {
            get => _control.DragButton;
            set => _control.DragButton = value;
        }

        public PointLatLng Position
        {
            get => _control.Position;
            set => _control.Position = value;
        }

        public GMapProvider MapProvider
        {
            get => _control.MapProvider;
            set => _control.MapProvider = value;
        }

        public ObservableCollection<GMapMarker> Markers => _control.Markers;


        PureImageCache IGMapControl.PrimaryCache
        { 
            get => _control.Manager.PrimaryCache;
            set => _control.Manager.PrimaryCache = value;
        }

        AccessMode IGMapControl.CacheMode
        {
            get => _control.Manager.Mode;
            set => _control.Manager.Mode = value;
        }

        public PointLatLng FromLocalToLatLng (int x, int y) => _control.FromLocalToLatLng(x, y);

    }

}
