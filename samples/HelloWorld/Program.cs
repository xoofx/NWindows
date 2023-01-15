using System.Drawing;
using NWindows;
using NWindows.Input;
using NWindows.Threading;

var window = Window.Create(new()
{
    Title = "Hello World",
    BackgroundColor = Color.DarkSlateGray,
    StartPosition = WindowStartPosition.CenterScreen,
});

Console.WriteLine("Press escape to close the Window");

window.Events.Keyboard += (_, evt) =>
{
    if (evt.Key == Key.Escape)
    {
        window.Close();
        evt.Handled = true;
    }
};

Dispatcher.Current.Run();