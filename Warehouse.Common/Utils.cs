using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Common
{
    public static class Utils
    {
        public static DateTime ServerNow
        {
            get
            {
                DateTime date1 = DateTime.UtcNow;

                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

                DateTime date2 = TimeZoneInfo.ConvertTime(date1, tz);
                // return DateTime.Now.AddHours(11);
                return date2;
            }
        }
        public static string API_PATH = "http://localhost:40008/";


        public enum DirectionType
        {
            In,
            Out
        }
    }

    public static class Roles
    {
        public static string DeveloperRole = "Developer";
        public static string AdminRole = "Admin";
        public static string NormalUserRole = "NormalUser";
    }

    public enum STATUS
    {
        InWork=0, InArchive=1
    }

    public enum DeleteResourceResult
    {
        NotFound,
        HasDependencies,
        Success
    }

}
