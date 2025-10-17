using MahApps.Metro.Controls;
using OffRouteMap.Properties;

namespace OffRouteMap
{

    public partial class MainWindow : MetroWindow
    {
        private readonly ThemeService _themeService;

        public MainWindow()
        {
            InitializeComponent();

            btnCacheRoot.ToolTip = Strings.FolderDialog_Title;
            btnDelete.ToolTip = Strings.DelCommand_Hint;
            btnLoad.ToolTip = Strings.LoadDialog_Title;
            btnSave.ToolTip = Strings.SaveDialog_Title;

            _themeService = new ThemeService(140, 220, 178);
            _themeService.ApplyTheme(this, Settings.Default.isDark);

            DataContext = new MainViewModel(gmapControl);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
    }
}