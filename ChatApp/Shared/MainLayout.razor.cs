using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ChatApp.Shared;

/// <summary>
/// The main layout component that handles theme and navigation drawer toggles.
/// </summary>
public partial class MainLayout : LayoutComponentBase
{
    /// <summary>
    /// Whether the navigation drawer is open.
    /// </summary>
    private bool _drawerOpen = false;

    /// <summary>
    /// Whether dark mode is active.
    /// </summary>
    private bool _isDarkMode = false;

    /// <summary>
    /// Gets the theme icon based on the current mode.
    /// </summary>
    private string ThemeIcon => _isDarkMode
        ? Icons.Material.Filled.LightMode
        : Icons.Material.Filled.ModeNight;

    /// <summary>
    /// Custom theme for the app.
    /// </summary>
    private MudTheme _myCustomTheme = new();

    /// <summary>
    /// Toggles the navigation drawer.
    /// </summary>
    private void ToggleNavMenu()
    {
        _drawerOpen = !_drawerOpen;
    }

    /// <summary>
    /// Switches between dark and light mode.
    /// </summary>
    private void ToggleDarkMode()
    {
        _isDarkMode = !_isDarkMode;
    }
}
