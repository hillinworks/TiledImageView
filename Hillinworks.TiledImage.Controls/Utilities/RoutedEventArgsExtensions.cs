using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Windows
{
    public static class RoutedEventArgsExtensions
    {
        private static MethodInfo MemberwiseCloneMethod { get; }
            = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        private static PropertyInfo InvokingHandlerProperty { get; }
            = typeof(RoutedEventArgs).GetProperty("InvokingHandler", BindingFlags.NonPublic | BindingFlags.Instance);

        public static T Clone<T>(this T e)
            where T : RoutedEventArgs
        {
            var clonedEvent = (T)MemberwiseCloneMethod.Invoke(e, null);
            return clonedEvent;
        }

        public static void SetInvokingHandler(this RoutedEventArgs e, bool invokingHandler)
        {
            InvokingHandlerProperty.SetValue(e, invokingHandler);
        }


    }
}