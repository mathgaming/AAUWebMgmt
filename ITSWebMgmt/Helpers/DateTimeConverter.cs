using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;

namespace ITSWebMgmt.Helpers
{
    public class DateTimeConverter
    {
        public static string Convert(DateTime? date)
        {
            if (date != null)
            {
                return ((DateTime)date).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "Date not found";
        }

        public static string Convert(long rawDate)
        {
            DateTime date = DateTime.FromFileTime(rawDate);
            return Convert(date);
        }

        public static string Convert(string CIM_DATETIME)
        {
            DateTime date = ManagementDateTimeConverter.ToDateTime(CIM_DATETIME);
            return Convert(date);
        }

        public static string Convert(object adsLargeInteger)
        {
            IADsLargeInteger largeInt = (IADsLargeInteger)adsLargeInteger;
            long datelong = (((long)largeInt.HighPart) << 32) + largeInt.LowPart;
            if (datelong == 9223372032559808511 || datelong > DateTime.MaxValue.ToFileTime())
            {
                return Convert(DateTime.MaxValue);
            }

            DateTime date = DateTime.FromFileTime(datelong);
            return Convert(date);
        }
    }

    [ComImport, Guid("9068270b-0939-11d1-8be1-00c04fd8d503"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IADsLargeInteger
    {
        long HighPart
        {
            [SuppressUnmanagedCodeSecurity]
            get; [SuppressUnmanagedCodeSecurity]
            set;
        }

        long LowPart
        {
            [SuppressUnmanagedCodeSecurity]
            get; [SuppressUnmanagedCodeSecurity]
            set;
        }
    }
}