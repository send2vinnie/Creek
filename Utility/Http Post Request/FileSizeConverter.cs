using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Utility
{
    public class FileSizeConverter
    {

        private static string DECIMAL_FORMATTER = "{0:0.##}";

        /**
         * A quantity in which to express a file size.
         * 
         * @author erickok
         *
         */
        public enum SizeUnit
        {
            B,
            KB,
            MB,
            GB
        }

        private static int INC_SIZE = 1024;

        // Returns a file size given in bytes to a different unit, as a formatted string
        public static string GetSize(long from, SizeUnit to)
        {
            string outstr;
            switch (to)
            {
                case SizeUnit.B:
                    outstr = from.ToString();
                    break;
                case SizeUnit.KB:
                    outstr = String.Format(DECIMAL_FORMATTER, ((double)from) / 1024);
                    break;
                case SizeUnit.MB:
                    outstr = String.Format(DECIMAL_FORMATTER, ((double)from) / 1024 / 1024);
                    break;
                default:
                    outstr = String.Format(DECIMAL_FORMATTER, ((double)from) / 1024 / 1024 / 1024);
                    break;
            }

            return (outstr + " " + to.ToString());
        }

        // Returns a file size in bytes in a nice readable formatted string
        public static string GetSize(long from)
        {
            return GetSize(from, true);
        }

        // Returns a file size in bytes in a nice readable formatted string
        public static string GetSize(long from, bool withUnit)
        {
            if (from < INC_SIZE)
            {
                return from.ToString() + (withUnit ? SizeUnit.B.ToString() : "");
            }
            else if (from < (INC_SIZE * INC_SIZE))
            {
                return string.Format(DECIMAL_FORMATTER, ((double)from) / INC_SIZE) + (withUnit ? SizeUnit.KB.ToString() : "");
            }
            else if (from < (INC_SIZE * INC_SIZE * INC_SIZE))
            {
                return string.Format(DECIMAL_FORMATTER, ((double)from) / INC_SIZE / INC_SIZE) + (withUnit ? SizeUnit.MB.ToString() : "");
            }
            else
            {
                return string.Format(DECIMAL_FORMATTER, ((double)from) / INC_SIZE / INC_SIZE / INC_SIZE) + (withUnit ? SizeUnit.GB.ToString() : "");
            }
        }

    }
}
