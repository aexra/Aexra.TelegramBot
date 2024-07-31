namespace Aexra.TelegramBot.Core.Helpers;
public static class TypeHelper
{
    public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit) where TAttribute : Attribute
    {
        return from a in AppDomain.CurrentDomain.GetAssemblies()
               from t in a.GetTypes()
               where t.IsDefined(typeof(TAttribute), inherit)
               select t;
    }
}
