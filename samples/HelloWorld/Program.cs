using System.Drawing;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

var mainWindow = Window.Create(new()
{
    Title = "Hello World",
    BackgroundColor = GetCurrentThemeColor(),
    StartPosition = WindowStartPosition.CenterScreen,
});

Console.WriteLine("Press escape to close the Window");

mainWindow.Events.Frame += (window, evt) =>
{
    if (evt.FrameKind == FrameEventKind.ThemeChanged)
    {
        // Update the background color if the theme changed
        window.BackgroundColor = GetCurrentThemeColor();
    }
};

mainWindow.Events.Keyboard += (_, evt) =>
{
    if (evt.Key == Key.Escape)
    {
        mainWindow.Close();
        evt.Handled = true;
    }
};

Dispatcher.Current.Run();

static Color GetCurrentThemeColor() => WindowSettings.Theme == WindowTheme.Light ? Color.FromArgb(245, 245, 245)  : Color.FromArgb(30, 30, 30);
