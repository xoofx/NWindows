using HelloDirect3D;
using NWindows;
using NWindows.Input;
using NWindows.Threading;

//Dispatcher.Current.EnableDebug = true;

var mainWindow = Window.Create(new()
{
    Title = "Hello Direct3D",
    StartPosition = WindowStartPosition.CenterScreen,
});

var helloTriangle = new HelloTriangle11(mainWindow);
helloTriangle.Initialize();

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
}
