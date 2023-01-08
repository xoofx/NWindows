using System.Drawing;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

//Dispatcher.Current.EnableTrace = true;
//Dispatcher.Current.TraceOutput = Console.Out.WriteLine;

var window = Window.Create(new WindowCreateOptions()
{
    Title = "Hello World",
    BackgroundColor = Color.DarkSlateGray,
    StartPosition = WindowStartPosition.CenterScreen
});

window.Events.All += EventsOnAll;

Dispatcher.Current.Run();

static void EventsOnAll(Window window, WindowEvent evt)
{
    Console.WriteLine(evt);
    if (evt is MouseEvent mouseEvent)
    {
        Console.WriteLine($"Mouse Position: {Mouse.Position} LeftButton: {Mouse.LeftButton}");
        if ((mouseEvent.Pressed & MouseButtonFlags.Left) != 0)
        {
            Mouse.SetCursor(Cursor.Hand);
        }
        else
        {
            Mouse.SetCursor(Cursor.Arrow);
        }
    }
}