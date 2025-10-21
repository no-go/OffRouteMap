using GMap.NET.WindowsPresentation;
using MahApps.Metro.Controls;
using OffRouteMap.Properties;
using System.Windows.Input;

namespace OffRouteMap
{

    public partial class MainWindow : MetroWindow
    {
        private readonly ThemeService _themeService;
        private readonly MainViewModel _viewModel;

        public MainWindow ()
        {
            InitializeComponent();

            // A primitve way for localisations.
            btnCacheRoot.ToolTip = Strings.FolderDialog_Title;
            btnDelete.ToolTip = Strings.DelCommand_Hint;
            btnLoad.ToolTip = Strings.LoadDialog_Title;
            btnSave.ToolTip = Strings.SaveDialog_Title;

            // set the UI theme
            _themeService = new ThemeService(140, 220, 178);
            _themeService.ApplyTheme(this, Settings.Default.isDark);

            gmapControl.MouseMove += GMapControl_MouseMove;
            gmapControl.MouseDoubleClick += GMapControl_MouseDoubleClick;
            gmapControl.MouseRightButtonDown += GMapControl_MouseRightButtonDown;

            var gmapWrapper = new GMapControlWrapper(gmapControl);
            _viewModel = new MainViewModel(gmapWrapper);
            DataContext = _viewModel;
        }

        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = (MainViewModel)this.DataContext;

            if (viewModel.BeforeClosingCommand.CanExecute(null))
            {
                viewModel.BeforeClosingCommand.Execute(null);
            }

            // @todo some newbie trail and error here :-S
            e.Cancel = false;
            //Application.Current.Shutdown();
            Environment.Exit(0);
            //this.Close();
        }

        /// <summary>
        /// The event of mouse movement on the map forward to the DataModel.
        /// </summary>
        /// <param name="sender">sending ui element</param>
        /// <param name="e">mouse event. we need its position here</param>
        private void GMapControl_MouseMove (object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(gmapControl);
            _viewModel.UpdateMousePositionFrom(point);
        }

        /// <summary>
        /// The event of mouse double click on the map forward to the DataModel.
        /// </summary>
        /// <param name="sender">sending ui element</param>
        /// <param name="e">mouse event. we need its position here</param>
        private void GMapControl_MouseDoubleClick (object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(gmapControl);
            _viewModel.UpdateMousePositionFrom(point);
            _viewModel.AddRoutePoint();
        }

        /// <summary>
        /// The event of mouse click on the map forward to the DataModel.
        /// </summary>
        /// <param name="sender">sending ui element</param>
        /// <param name="e">mouse event. we need its position here</param>
        private void GMapControl_MouseRightButtonDown (object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(gmapControl);
            _viewModel.UpdateMousePositionFrom(point);
            _viewModel.RemoveLastRoutePoint();
        }
    }
}