using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class CsvConvertor
    {
        private static string Encode(string s)
        {
            s = s ?? "";
            s = s.Replace("\\", "\\\\");
            s = s.Replace(",", "\\,");
            return s;
        }

        private static string Decode(string s)
        {
            s = s.Replace("\\\\", "\\");
            s = s.Replace("\\,", ",");
            return s;
        }

        public static string ToCsvString(List<string> slist)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < slist.Count; i++)
            {
                sb.Append(CsvConvertor.Encode(slist[i]) + ",");
            }
            var txt = sb.ToString();
            if (txt.Length > 0)
            {
                txt = txt.Substring(0, txt.Length - 1);
            }
            return txt;
        }

        public static string ToCsvString(List<long> slist)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < slist.Count; i++)
            {
                sb.Append(slist[i].ToString() + ",");
            }
            var txt = sb.ToString();
            if (txt.Length > 0)
            {
                txt = txt.Substring(0, txt.Length - 1);
            }
            return txt;
        }

        public static List<string> SplitCsvString(string csv)
        {
            return CsvConvertor.SplitCsvString(csv, StringSplitOptions.None);
        }

        public static List<string> SplitCsvString(string csv, StringSplitOptions opt)
        {
            var txts = csv.Split(new char[]{','}, opt);
            var ds = new List<string>();
            foreach (var txt in txts)
            {
                ds.Add(txt.Replace("\\\\", "\\"));
            }
            return ds;
        }

        public static string ToCsvB64String(List<string> slist)
        {
            var s = CsvConvertor.ToCsvString(slist);
            var bytes = Encoding.Unicode.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static List<string> SplitCsvB64String(string b64csv)
        {
            var bytes = Convert.FromBase64String(b64csv);
            var s = Encoding.Unicode.GetString(bytes);
            return CsvConvertor.SplitCsvString(s);
        }
    }
}
