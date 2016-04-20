using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace System.Net
{
    public static class Extensions
    {
        public static string Join(this Dictionary<string, string> source, string keyValuePairSeparator, string keyValueSeparator)
        {
            return Join(source, keyValuePairSeparator, keyValueSeparator, false);
        }

        public static string Join(this Dictionary<string, string> source, string keyValuePairSeparator, string keyValueSeparator, bool urlEncode)
        {
            var builder = new StringBuilder();

            int count = 0;

            foreach (var parameter in source)
            {
                count++;
                builder.AppendFormat("{0}{1}{2}", parameter.Key, keyValueSeparator, urlEncode ? Uri.EscapeDataString(parameter.Value) : parameter.Value);
                if (count != source.Keys.Count)
                    builder.Append(keyValuePairSeparator);
            }

            return builder.ToString();
        }
    }
}
