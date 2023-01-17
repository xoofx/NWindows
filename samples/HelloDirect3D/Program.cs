using System.Drawing;
using HelloDirect3D;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

//Dispatcher.Current.EnableDebug = true;

var mainWindow = Window.Create(new()
{
    Title = "Hello Direct3D",
    StartPosition = WindowStartPosition.CenterScreen,
    BackgroundColor = GetCurrentThemeColor(),
    EnableComposition = true, // For smooth rendering/resizing with Direct3D/DXGI
});

mainWindow.Events.Frame += (window, evt) =>
{
    if (evt.FrameKind == FrameEventKind.ThemeChanged)
    {
        // Update the background color if the theme changed
        window.BackgroundColor = GetCurrentThemeColor();
    }
};

var helloTriangle = new HelloTriangle11(mainWindow)
{
    // We can limit our rendering to full HD
    // MaximumSize = new Size(1920, 1080)
};
helloTriangle.Initialize();

mainWindow.Events.All += (window, evt) =>
{
    //Console.WriteLine(evt);
};

mainWindow.Events.Keyboard += (_, evt) =>
{
    if (evt.IsDown)
    {
        if (evt.Key == Key.Escape)
        {
            mainWindow.Close();
            evt.Handled = true;
        }
        else if (evt.Key == Key.Enter && (evt.Modifiers & ModifierKeys.Alt) != 0)
        {
            mainWindow.State = mainWindow.State == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            evt.Handled = true;
        }
    }
};

Dispatcher.Current.Events.Idle += DispatcherIdle;
Dispatcher.Current.Run();

helloTriangle.Dispose();

void DispatcherIdle(Dispatcher dispatcher, NWindows.Threading.Events.IdleEvent evt)
{
    helloTriangle.Draw();
    evt.SkipWaitForNextMessage = true;
    evt.Handled = true;
}

static Color GetCurrentThemeColor() => WindowSettings.Theme == WindowTheme.Light ? Color.FromArgb(245, 245, 245) : Color.FromArgb(30, 30, 30);
