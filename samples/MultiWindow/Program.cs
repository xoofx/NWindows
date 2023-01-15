using System.Drawing;
using NWindows;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;

int popupCount = 0;
var popupEventHub = new WindowEventHub();
popupEventHub.All += PopupEventHandler;

var mainEventHub = new WindowEventHub();
mainEventHub.All += MainEventHandler;

var mainWindow = Window.Create(new()
{
    Title = "Multi Window",
    BackgroundColor = Color.DarkSlateGray,
    StartPosition = WindowStartPosition.CenterScreen,
    Events = mainEventHub,
});

Console.WriteLine("----------------------------------------------------");
Console.WriteLine("Multi Window Sample");
Console.WriteLine("----------------------------------------------------");
Console.WriteLine();
Console.WriteLine("On the mainWindow:");
Console.WriteLine("- Press <escape> to exit from the application.");
Console.WriteLine("- Press <space> to create a popup window.");
Console.WriteLine();
Console.WriteLine("On the popup Window:");
Console.WriteLine("- Press <escape> to close the popup window.");
Console.WriteLine("- Press <D> to add or remove decorations around the popup window.");
Console.WriteLine("- Press <R> to allow or disable resize of the window. (When the window is not resizeable, it becomes red)");
Console.WriteLine("- Press <M> to enable or disable the popup window as model.");
Console.WriteLine("- Change <mouse wheel> to change the opacity of the window.");

Console.WriteLine();
Console.WriteLine("----------------------------------------------------");
Console.WriteLine("Events");
Console.WriteLine("----------------------------------------------------");

Dispatcher.Current.Run();

void MainEventHandler(Window window, WindowEvent evt)
{
    if (evt is KeyboardEvent keyboardEvent && keyboardEvent.IsDown)
    {
        bool handled = false;
        if (keyboardEvent.Key == Key.Space)
        {
            popupCount++;
            var popup = Window.Create(new()
            {
                Title = $"Popup Window {popupCount}",
                Kind = WindowKind.Popup,
                Parent = window,
                BackgroundColor = Color.DarkSlateBlue,
                StartPosition = WindowStartPosition.CenterParent,
                Size = new Size(640, 480),
                Events = popupEventHub,
            });
            handled = true;
        }
        else if (keyboardEvent.Key == Key.Escape)
        {
            window.Close();
            handled = true;
        }
        keyboardEvent.Handled = handled; 
    }
}

static void PopupEventHandler(Window window, WindowEvent evt)
{
    if (evt is KeyboardEvent keyboardEvent && keyboardEvent.IsDown)
    {
        var handled = false;
        if (keyboardEvent.Key == Key.D)
        {
            window.HasDecorations = !window.HasDecorations;
            Console.WriteLine($"Popup Window \"{window.Title}\" HasDecorations: {window.HasDecorations}");
            handled = true;
        }
        else if (keyboardEvent.Key == Key.R)
        {
            window.Resizeable = !window.Resizeable;
            Console.WriteLine($"Popup Window \"{window.Title}\" Resizeable: {window.Resizeable}");
            window.BackgroundColor = window.Resizeable ? Color.DarkSlateBlue : Color.DarkRed;
            handled = true;
        }
        else if (keyboardEvent.Key == Key.M)
        {
            window.Modal = !window.Modal;
            Console.WriteLine($"Popup Window \"{window.Title}\" Modal: {window.Modal}");
            handled = true;
        }
        else if (keyboardEvent.Key == Key.Escape)
        {
            window.Close();
            handled = true;
        }
        keyboardEvent.Handled = handled;
    }
    else if (evt is MouseEvent mouseEvt)
    {
        if ((mouseEvt.WheelDelta.Y != 0))
        {
            window.Opacity += 0.1f * mouseEvt.WheelDelta.Y;
        }
    }
    else if (evt is CloseEvent)
    {
        Console.WriteLine($"Window \"{window.Title}\" is going to be closed");
    }
}