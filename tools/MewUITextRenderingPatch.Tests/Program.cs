using System.Runtime.CompilerServices;
using Aprillz.MewUI.Rendering.Direct2D;

// No window or native graphics initialization: exercise the actual patched selector with sentinel
// pointers. UnsafeAccessor is resolved statically, so the same checks can run in a NativeAOT binary.
var factory = Access.Create();
Access.Original(factory) = 22;
Access.LowDpi(factory) = 11;

Check(1, 11);
Check(1.25, 22);
Check(1.5, 22);
Check(2, 22);
Check(1, 11); // Moving back to a 100% screen must select the low-DPI params again.
Check(double.NaN, 22);
Access.LowDpi(factory) = 0;
Check(1, 22); // Failure to create system params preserves the original rendering path.
Access.Original(factory) = 0;
factory.Dispose(); // An uninitialized factory must release zero handles safely, including on shutdown.
factory.Dispose();
Console.WriteLine("7 Windows text rendering checks and repeated disposal passed.");

void Check(double dpi, nint expected)
{
    var actual = Access.Select(factory, dpi);
    if (actual != expected) throw new InvalidOperationException($"DPI scale {dpi}: expected {expected}, got {actual}");
}

static class Access
{
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    internal static extern Direct2DGraphicsFactory Create();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_textRenderingParams")]
    internal static extern ref nint Original(Direct2DGraphicsFactory factory);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_islandsLowDpiTextRenderingParams")]
    internal static extern ref nint LowDpi(Direct2DGraphicsFactory factory);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IslandsSelectTextRenderingParams")]
    internal static extern nint Select(Direct2DGraphicsFactory factory, double dpiScale);
}
