using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Cosmobot
{
    public static class SerializationUtils
    {
        public static CultureInfo CultureInfo { get; } = CultureInfo.InvariantCulture;
        public static NumberFormatInfo NumberFormatInfo { get; } = new NumberFormatInfo();
        
        public static bool TryParse(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, NumberFormatInfo, out result);
        }
        
        public static bool TryParse(string value, out float result)
        {
            return float.TryParse(value, NumberStyles.Float, NumberFormatInfo, out result);
        }
        
        public static bool TryParse(string value, out bool result)
        {
            return bool.TryParse(value, out result);
        }

        /// <param name="value"></param>
        /// <returns>
        /// value toString() for <see cref="System.Globalization.CultureInfo.InvariantCulture"/>
        /// (and bool is lower case) or null if <paramref name="value"/> is null
        /// </returns>
        [CanBeNull]
        public static string ToString([CanBeNull] object value)
        {
            if (value is null) return null;
            if (value is bool)
            {
                var b = (bool)Convert.ChangeType(value, typeof(bool));
                return b ? "true" : "false"; // i hate C#
            }
            return Convert.ToString(value, CultureInfo);
        }
    }
}