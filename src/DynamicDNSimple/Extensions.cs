using System;

namespace DynamicDNSimple
{
    static class Extensions
    {
        public static T Dump<T>(this T obj, string title = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                Console.WriteLine(obj);

            else Console.WriteLine("{0}: {1}", title, obj);
            return obj;
        }
    }
}