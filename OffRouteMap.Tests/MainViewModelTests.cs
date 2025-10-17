using GMap.NET.WindowsPresentation;
using Xunit;

namespace OffRouteMap.Tests
{
    public class MainViewModelTests
    {
        public static bool WindowTitle_PropertyChanged_Called;
        public static bool WindowTitle_PropertyChanged_null_Called;
        
        private MainViewModel _vm;

        public MainViewModelTests ()
        {
            _vm = new MainViewModel(new GMapControl());
        }

        [Fact]
        public void WindowTitle_PropertyChanged ()
        {
            WindowTitle_PropertyChanged_Called = true;

            string? raisedName = null;
            _vm.PropertyChanged += (s, e) => raisedName = e.PropertyName;

            _vm.WindowTitle = "New title";

            Assert.Equal("New title", _vm.WindowTitle);
            Assert.Equal(nameof(_vm.WindowTitle), raisedName);
        }

        [Fact]
        public void WindowTitle_SetNullValue_PropertyChanged_null ()
        {
            WindowTitle_PropertyChanged_null_Called = true;

            _vm.WindowTitle = "Initial";
            bool eventRaised = false;
            _vm.PropertyChanged += (s, e) => eventRaised = true;

            _vm.WindowTitle = null;

            Assert.Equal("Initial", _vm.WindowTitle);
            Assert.False(eventRaised);
        }
    }
}