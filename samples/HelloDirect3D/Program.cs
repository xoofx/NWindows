using HelloDirect3D;
using NWindows;
using NWindows.Input;
using NWindows.Threading;

var window = Window.Create(new()
{
    Title = "Hello Direct3D",
    StartPosition = WindowStartPosition.CenterScreen,
});

window.Events.Keyboard += (window1, evt) =>
{
    if (evt.Key == Key.Escape)
    {
        window.Close();
        evt.Handled = true;
    }
};

var helloTriangle = new HelloTriangle11(window);
helloTriangle.Initialize();

Dispatcher.Current.Events.Idle += DispatcherIdle;

Console.WriteLine("Press escape to close the Window");
Dispatcher.Current.Run();

helloTriangle.Dispose();

void DispatcherIdle(Dispatcher dispatcher, NWindows.Threading.Events.IdleEvent evt)
{
    helloTriangle.Draw();
    evt.SkipWaitForNextMessage = true;
}
