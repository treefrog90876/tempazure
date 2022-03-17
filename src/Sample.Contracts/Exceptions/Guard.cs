using System;

namespace Sample.Exceptions
{
    public static class Guard
    {
        public static T ThrowIfNull<T>(T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        public static string ThrowIfNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }
    }
}
