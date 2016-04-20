using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Utility
{
    internal class Check
    {
        public static void IsNullOrEmpty(string sToCheck, string sName)
        {
            if (sToCheck == null || sToCheck.Length == 0)
                throw new ArgumentException(AppResource.InputParameterRequired, sName);
        }

        public static void IsNull(object oToCheck, string sName)
        {
            if (oToCheck == null)
                throw new ArgumentException(AppResource.InputParameterRequired, sName);
        }

        public static void IsPositive(long lToCheck, string sName)
        {
            if (lToCheck < 0)
                throw new ArgumentException(AppResource.InputParameterSouldBePositive, sName);
        }
    }
}
