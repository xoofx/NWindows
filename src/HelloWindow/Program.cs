using System.Drawing;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

Dispatcher.Current.EnableDebug = true;
//Dispatcher.Current.DebugOutput = Console.Out.WriteLine;

var window = Window.Create(new ()
{
    Title = "Hello World",
    BackgroundColor = Color.DarkSlateGray,
    StartPosition = WindowStartPosition.CenterScreen,
    DragDrop = true
});

//var windowChild = Window.Create(new()
//{
//    Kind = WindowKind.Child,
//    BackgroundColor = Color.Red,
//    Parent = window,
//});


window.Events.All += EventsOnAll;

Point deltaMousePosition = default;

//windowChild.Events.All += EventsOnAllChild;

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



//void EventsOnAllChild(Window w, WindowEvent evt)
//{
//    Console.WriteLine($"Child {evt}");

//    if (evt is MouseEvent mouseEvt)
//    {
//        var mousePositionOnScreen = Mouse.Position;

//        if (mouseEvt.Button == MouseButton.Left)
//        {
//            //window.Title = $"{window.Kind} Pressed: {_eventId}";
//            deltaMousePosition = new Point(windowChild.Position.X - mousePositionOnScreen.X, windowChild.Position.Y - mousePositionOnScreen.Y);
//        }
//        if ((mouseEvt.Pressed & MouseButtonFlags.Left) != 0)
//        {
//            windowChild.Position = new Point(mousePositionOnScreen.X + deltaMousePosition.X, mousePositionOnScreen.Y + deltaMousePosition.Y);
//        }
//    }

//}