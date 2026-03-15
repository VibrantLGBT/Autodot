using System.Runtime.InteropServices;

public static class MessageBox
{
#if !LINUX
    [DllImport("user32.dll")]
    private static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);
#else
    private static int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType)
    {
        return (int)MessageBoxResult.Ok;
    }
#endif

    public static MessageBoxResult Show(string text) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, "\0", (uint)MessageBoxButtons.Ok);

    public static MessageBoxResult Show(string text, string caption) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)MessageBoxButtons.Ok);

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)buttons);

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxDefaultButton button) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)button));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxDefaultButton button, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)button) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)modal));
}

public enum MessageBoxButtons
{
    Ok = 0x00000000,
    OkCancel = 0x00000001,
    AbortRetryIgnore = 0x00000002,
    YesNoCancel = 0x00000003,
    YesNo = 0x00000004,
    RetryCancel = 0x00000005,
    CancelTryIgnore = 0x00000006,
    Help = 0x00004000,
}

public enum MessageBoxResult
{
    Ok = 1,
    Cancel,
    Abort,
    Retry,
    Ignore,
    Yes,
    No,
    TryAgain = 10,
    Continue
}
public enum MessageBoxDefaultButton : uint
{
    Button1 = 0x00000000,
    Button2 = 0x00000100,
    Button3 = 0x00000200,
    Button4 = 0x00000300,
}
public enum MessageBoxModal : uint
{
    Application = 0x00000000,
    System = 0x00001000,
    Task = 0x00002000
}
public enum MessageBoxIcon : uint
{
    Error = 0x00000010,
    Question = 0x00000020,
    Warning = 0x00000030,
    Information = 0x00000040
}