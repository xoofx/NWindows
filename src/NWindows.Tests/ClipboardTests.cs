using NUnit.Framework;

namespace NWindows.Tests;

public class ClipboardTests
{
    [Test]
    public void TestUnicodeText()
    {
        var text = "Hello World from ★ NWindows ★";
        Clipboard.SetData(DataFormats.UnicodeText, text);
        var textFromClipboard = Clipboard.GetData(DataFormats.UnicodeText);
        Assert.AreEqual(text, textFromClipboard);

        var formats = Clipboard.GetDataFormats();
        Assert.True(formats.Count > 0, "Expecting at least one format in the clipboard");

        CollectionAssert.Contains(formats, DataFormats.UnicodeText);
    }

    [Test]
    public void TestText()
    {
        var text = "Hello World from NWindows 2";
        Clipboard.SetData(DataFormats.Text, text);
        var textFromClipboard = Clipboard.GetData(DataFormats.Text);
        Assert.AreEqual(text, textFromClipboard);
    }

    [Test]
    public void TestHtml()
    {
        var text = "<p>Hello World from NWindows</p>";
        Clipboard.SetData(DataFormats.Html, text);
        var textFromClipboard = Clipboard.GetData(DataFormats.Html);
        Assert.AreEqual(text, textFromClipboard);
    }

    [Test]
    public void TestFileTransferList()
    {
        var list = new FileTransferList()
        {
            "/file1.txt",
            "/dir1/file2.txt",
            "/dir2/file3.txt",
        };
        Clipboard.SetData(DataFormats.File, list);
        var fileFromClipboard = Clipboard.GetData(DataFormats.File);
        Assert.AreEqual(list, fileFromClipboard);
    }
    
    [Test]
    public void TestPng()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        Clipboard.SetData(DataFormats.Png, buffer);
        var dataFromClipboard = Clipboard.GetData(DataFormats.Png);
        Assert.AreEqual(buffer, dataFromClipboard);
    }

    [Test]
    public void TestRichText()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        Clipboard.SetData(DataFormats.RichText, buffer);
        var dataFromClipboard = Clipboard.GetData(DataFormats.RichText);
        Assert.AreEqual(buffer, dataFromClipboard);
    }

    [Test]
    public void TestCustomFormat()
    {
        {
            var format = Clipboard.Register<byte[]>("NWindows-MyTestCustomFormat-bytes");
            var buffer = new byte[] { 1, 2, 3, 4 };
            Clipboard.SetData(format, buffer);
            var dataFromClipboard = Clipboard.GetData(format);
            Assert.AreEqual(buffer, dataFromClipboard);
        }
        {

            var format = Clipboard.Register<string>("NWindows-MyTestCustomFormat-string");
            var buffer = $"Hello from {format.Mime}";
            Clipboard.SetData(format, buffer);
            var dataFromClipboard = Clipboard.GetData(format);
            Assert.AreEqual(buffer, dataFromClipboard);

            Clipboard.Clear();
            dataFromClipboard = Clipboard.GetData(format);
            Assert.Null(dataFromClipboard);
        }
    }

}