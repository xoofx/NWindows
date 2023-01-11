using System.Drawing;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

//var pngData = Clipboard.GetData(DataFormats.Png);

var files = Clipboard.GetData(DataFormats.File);
if (files != null)
{
    Console.WriteLine($"Requesting files {files.PreferredDataTransferEffects}");
    Clipboard.Notify(DataTransferResult.Copy);
}

var formats = Clipboard.GetDataFormats();

foreach (var format in formats)
{
    Console.WriteLine(format);
}

Clipboard.SetData(DataFormats.File, new FileTransferList() { "C:\\code\\InsideClipboard\\readme.txt"});
var list = Clipboard.GetData(DataFormats.File);

Clipboard.SetData(DataFormats.UnicodeText, "Hello World 1");
var text1 = Clipboard.GetData(DataFormats.UnicodeText);

Clipboard.SetData(DataFormats.Text, "Hello World 2");
var text2 = Clipboard.GetData(DataFormats.Text);

Clipboard.SetData(new ClipboardData()
{
    {DataFormats.Html, "<p>Hello World</p>"},
    {DataFormats.UnicodeText, "Hello World"}
});
var html = Clipboard.GetData(DataFormats.Html);

var customFormat = Clipboard.Register<string>("NWindows.HelloWorld.Clipboard");
Clipboard.SetData(customFormat, "Hello World From Custom Format");
var test3 = Clipboard.GetData(customFormat);


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
    else if (evt is FrameEvent frameEvent)
    {
        if (frameEvent.FrameKind == FrameEventKind.ClipboardChanged)
        {
            var formats = Clipboard.GetDataFormats();
            Console.WriteLine(string.Join(",", formats));

            foreach (var format in formats)
            {
                if (format.IsSupported && format is DataFormat<string> stringFormat)
                {
                    var text = Clipboard.GetData(stringFormat);
                    Console.WriteLine(text);
                }
            }
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