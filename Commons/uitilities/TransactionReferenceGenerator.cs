using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.uitilities
{
    public static class TransactionReferenceGenerator
    {
        public const string prefix = "iv";
    
        public static string Reference()
        {
            string randomNumber = OtpUtility.GenerateRandomCode(6,false);
            string day = DateTime.Now.Day.ToString();
            string month = DateTime.Now.Month.ToString();       
            string yearCode = YearCode();
            string reference = prefix + randomNumber + day + month + yearCode;
            return reference;
        }
        public static string YearCode()
        {
            int year = DateTime.Now.Year;
            bool result = YearMap.TryGetValue(year, out var code);
            if (result)
                return code;
            throw new KeyNotFoundException($"no code found for {year}");
        }
        private static readonly Dictionary<int, string> YearMap = new()
        {
                {2026, "bbun"},
                {2027, "uvet"},
                {2028, "gnfk"},
                {2029, "fadt"},
                {2030, "fxwj"},
                {2031, "cnjs"},
                {2032, "pqtt"},
                {2033, "kgrf"},
                {2034, "nmgu"},
                {2035, "amlm"},
                {2036, "slhh"},
                {2037, "qhjw"},
                {2038, "ewqq"},
                {2039, "lman"},
                {2040, "sbye"},
                {2041, "ckia"},
                {2042, "vaws"},
                {2043, "tjts"},
                {2044, "ljqq"},
                {2045, "gbbh"},
                {2046, "gwoo"},
                {2047, "pbta"},
                {2048, "lsby"},
                {2049, "ohkh"},
                {2050, "cgmq"},
                {2051, "lqcv"},
                {2052, "cyeb"},
                {2053, "wjhq"},
                {2054, "zqyf"},
                {2055, "vukt"},
                {2056, "gwhb"},
                {2057, "huee"},
                {2058, "zjoa"},
                {2059, "plwp"},
                {2060, "utlr"},
                {2061, "xsec"},
                {2062, "llyx"},
                {2063, "ieli"},
                {2064, "vsjo"},
                {2065, "jani"},
                {2066, "uacm"},
                {2067, "vhan"},
                {2068, "uikc"},
                {2069, "jtwo"},
                {2070, "rfpw"},
                {2071, "wgvu"},
                {2072, "xfex"},
                {2073, "chgt"},
                {2074, "gdxx"},
                {2075, "qrge"},
                {2076, "kkda"},
        };
    }
}
