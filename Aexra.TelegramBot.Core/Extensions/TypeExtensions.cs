namespace Aexra.TelegramBot.Core.Extensions;

/// <summary>
/// Extensions for class Type
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns an object's TAttribute if exists
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
        if (att != null)
        {
            return att;
        }
        return default;
    }

    /// <summary>
    /// Return MethodInfo of all methods in Type that has TAttribute attribute
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="type"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static IEnumerable<System.Reflection.MethodInfo> GetMethodsWith<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
    {
        return type.GetMethods().Where(m => m.GetCustomAttributes(typeof(TAttribute), false).Length > 0);
    }
}
