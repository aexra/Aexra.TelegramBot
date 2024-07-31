namespace Aexra.TelegramBot.Core.Extensions;
public static class TypeExtensions
{
    public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
        if (att != null)
        {
            return att;
        }
        return default;
    }

    public static IEnumerable<System.Reflection.MethodInfo> GetMethodsWith<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
    {
        return type.GetMethods().Where(m => m.GetCustomAttributes(typeof(TAttribute), false).Length > 0);
    }
}
