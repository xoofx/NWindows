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


//Dispatcher.Current.EnableDebug = true;
//Dispatcher.Current.DebugOutput = Console.Out.WriteLine;
var icon = Icon.GetApplicationIcon();

var centerX = icon.Width / 2;
var centerY = icon.Height / 2;
var rX = (icon.Width - 1) / 2;
var rY = (icon.Height - 1) / 2;
var minR = Math.Min(rX, rY);
int stripeCount = minR / 3;
for (int i = 0; i < stripeCount; i++)
{
    var r = Math.Min(rX, rY) * i / (stripeCount - 1);
    for (float j = 0.0f; j < 2 * MathF.PI; j += MathF.PI / 360)
    {
        icon.PixelAt((int)(r * MathF.Cos(j) + centerX), (int)(r * MathF.Sin(j) + centerY)) = Color.Red;
    }
}

var window = Window.Create(new ()
{
    Title = "Hello World",
    BackgroundColor = Color.DarkSlateGray,
    //StartPosition = WindowStartPosition.CenterScreen,
    DragDrop = true,
    Icon = icon,
    //Decorations = false,
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
    if (evt is not MouseEvent || (evt is MouseEvent mouseEvent && (mouseEvent.Button != MouseButton.None || mouseEvent.Pressed != MouseButtonFlags.None)))
    {
        Console.WriteLine(evt);
    }

    if (evt is MouseEvent mouseEvt)
    {
        //Console.WriteLine($"Mouse Position: {Mouse.Position} LeftButton: {Mouse.LeftButton}");
        if ((mouseEvt.Pressed & MouseButtonFlags.Left) != 0)
        {
            Mouse.SetCursor(Cursor.Hand);
        }
        else
        {
            Mouse.SetCursor(Cursor.Arrow);
        }

        if ((mouseEvt.Pressed & MouseButtonFlags.Right) != 0)
        {
            Console.WriteLine($"Client (Dpi): {window.ClientSize} Client: {window.Dpi.LogicalToPixel(window.ClientSize)} Window (Dpi): {window.Size} Window: {window.Dpi.LogicalToPixel(window.Size)}");
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
    else if (evt is KeyboardEvent keyboardEvent)
    {
        if (keyboardEvent.IsDown)
        {
            if (keyboardEvent.Key == Key.E)
            {
                window.State = new WindowState.ExclusiveFullScreen(new(1920, 1080, 32, 60, DisplayOrientation.Default));
            }
            else if (keyboardEvent.Key == Key.F)
            {
                window.State = WindowState.FullScreen;
            }
            else if (keyboardEvent.Key == Key.N)
            {
                window.State = WindowState.Normal;
            }
            else if (keyboardEvent.Key == Key.P)
            {
                window.ClientSize = new SizeF(640, 320);
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