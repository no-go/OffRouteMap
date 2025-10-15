using OffRouteMap;
using NUnit.Framework;

namespace OffRouteMap.Tests
{
    [Apartment(ApartmentState.STA)]
    public class MainViewModelTests
    {
        private MainViewModel _vm;

        [SetUp]
        public void Setup ()
        {
            _vm = new MainViewModel(new MainWindow());
        }

        [Test]
        public void WindowTitle_SetNonNullValue_SetsFieldAndRaisesPropertyChanged ()
        {
            string? raisedName = null;
            _vm.PropertyChanged += (s, e) => raisedName = e.PropertyName;

            _vm.WindowTitle = "New title";

            Assert.That(_vm.WindowTitle, Is.EqualTo("New title"));
            Assert.That(raisedName, Is.EqualTo(nameof(_vm.WindowTitle)));
        }

        [Test]
        public void WindowTitle_SetNullValue_DoesNotChangeFieldOrRaisePropertyChanged ()
        {
            _vm.WindowTitle = "Initial";
            bool eventRaised = false;
            _vm.PropertyChanged += (s, e) => eventRaised = true;

            _vm.WindowTitle = null;

            Assert.That(_vm.WindowTitle, Is.EqualTo("Initial"));
            Assert.That(eventRaised, Is.EqualTo(false));
        }
    }
}