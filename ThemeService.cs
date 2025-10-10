using ControlzEx.Theming;
using System.Windows;
using System.Windows.Media;

namespace OffRouteMap
{
    class ThemeService
    {
        private Color _color;

        public ThemeService (byte red, byte green, byte blue)
        {
            _color = Color.FromRgb(red, green, blue);
        }

        public void ApplyTheme (FrameworkElement frameworkElement, Boolean isDark)
        {
            ThemeManager.Current.ChangeTheme(
                frameworkElement,
                RuntimeThemeGenerator.Current.GenerateRuntimeTheme(
                    isDark ? "Dark" : "Light",
                    _color
                )
            );
        }
    }
}
