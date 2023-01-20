# NWindows User Guide

This is a very basic user guide to give an overview of the architecture and how to interact with the library.

- [Getting Started](#getting-started)
- [Main classes](#main-classes)
- [Events](#events)
- [Advanced](#advanced)

## Getting Started

The main entry point to create a `Window` is the `Window.Create` method:

```c#
using NWindows;
using NWindows.Threading;
using System.Drawing;

var mainWindow = Window.Create(new()
{
    Title = "Hello World",
    StartPosition = WindowStartPosition.CenterScreen,
    BackgroundColor = WindowSettings.Theme == WindowTheme.Light
        ? Color.FromArgb(245, 245, 245)
        : Color.FromArgb(30, 30, 30)
});

Dispatcher.Current.Run();
```

Will create the following window on Windows:

![](../img/NWindows-HelloWorld.png)

You will find more examples in the [sample folder](../samples/readme.md).

## Main classes

There is a couple of API that you will likely need to use:

- The `Dispatcher` class is the class that runs the message loop for the thread via `Dispatcher.Current.Run();`
- The `Keyboard`, `Mouse`, `Cursor`, `Clipboard` classes give access to input methods and interaction with the clipboard.
- Both the dispatcher and the window publish their events via an `EventHub` class. See next section

## Events

NWindows is **modeling events by using modern C# with record**. It simplifies a lot the declaration of these events and it helps to have instant `ToString()` representation with their properties.

The `Dispatcher` class provides adding event listeners via the property `Dispatcher.Current.Events` to listen for:
- All the events below
- Application Idle events.
- Shutdown started/finished.
- Unhandled exceptions.

The `Window` class provides adding event listeners via the property `Window.Events` to listen for [Window events](https://github.com/xoofx/NWindows/tree/main/src/NWindows/Events): 
- All events.
- Keyboard events.
- Text events (translated character typed).
- Mouse events.
- Frame events (closed, destroyed, size changed...etc.).
- Paint event.
- Close event.
- HitTest events (for border-less Windows).
- Clipboard events.
- System events (screen changes...).

For example, the following code is listening for frame events on a specific Window when the OS theme is changed:

```c#
mainWindow.Events.Frame += (window, evt) =>
{
    // Update the background color if the theme changed
    if (evt.ChangeKind == FrameChangeKind.ThemeChanged)
    {
        window.BackgroundColor = GetCurrentThemeColor();
    }
};
```

Or the following code is closing the Window when the `<Escape>` key is pressed:

```c#
mainWindow.Events.Keyboard += (_, evt) =>
{
    if (evt.Key == Key.Escape)
    {
        mainWindow.Close();
        evt.Handled = true;
    }
};
```

> NOTE: In order to avoid allocations, events objects are cached across calls, so you should not cache them but copy their data during the call to the event handler!

## Advanced

A `Window` object provides the `Handle` property which returns the native handle for the associated OS dependent object.

Depending on the platforms, the `Handle` points to:

- On Windows: it is the `HWND` of the Window.
