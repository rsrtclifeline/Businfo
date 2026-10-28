using Businfo;
using System.Windows;
using System.Windows.Input;

namespace Businfo;
public static class WindowOpener
{
    public static readonly DependencyProperty WindowTypeProperty =
        DependencyProperty.RegisterAttached(
            "WindowType",
            typeof(Type),
            typeof(WindowOpener),
            new PropertyMetadata(null, OnWindowTypeChanged));

    public static void SetWindowType(UIElement element, Type value)
        => element.SetValue(WindowTypeProperty, value);

    public static Type GetWindowType(UIElement element)
        => (Type)element.GetValue(WindowTypeProperty);

    private static void OnWindowTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && e.NewValue is Type windowType)
        {
            element.MouseDown += (s, args) =>
            {
                if (args.ChangedButton == MouseButton.Left)
                {
                    // ShowSingleInstance call
                    var method = typeof(WindowHelper)
                        .GetMethod("ShowSingleInstance")
                        ?.MakeGenericMethod(windowType);
                    method?.Invoke(null, null);
                }
            };
        }
    }
}
