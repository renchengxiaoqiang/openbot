using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class CsvFileHelper
    {
        public static List<List<string>> ReadCsvFile(string fn, int maxlines = -1)
        {
            return ReadCsvFile(fn, Encoding.GetEncoding("gb2312"), maxlines);
        }

        public static List<List<string>> ReadCsvFile(string fn, Encoding enc, int maxlines = -1)
        {
            List<List<string>> lines = new List<List<string>>();
            int nline = 0;
            StreamReaderEx.OpenSteamForRead(fn, enc, sr=>
            {
                while (!sr.EndOfStream && (maxlines <= 0 || nline < maxlines))
                {
                    nline++;
                    var line = GetLine(sr);
                    bool hasNullVal = true;
                    foreach (var fieldVal in line)
                    {
                        if (!string.IsNullOrEmpty(fieldVal))
                        {
                            hasNullVal = false;
                            break;
                        }
                    }
                    if (!hasNullVal)
                    {
                        lines.Add(line);
                    }
                }
            });
            return lines;
        }

        private static List<string> GetLine(StreamReader sr)
        {
            var line = new List<string>();
            var val = sr.ReadLine();
            while (!string.IsNullOrEmpty(val))
            {
                string oneField = GetOneField(ref val, sr);
                line.Add(oneField);
            }
            return line;
        }

        private static string GetOneField(ref string s, StreamReader sr)
        {
            int n = CountContinuousQuotes(s, 0);
            if (IsStartWithQuote(s, n))
                return GetQuoteField(ref s, n, sr);

            var rtVal = string.Empty;
            int len = s.IndexOf(',');
            if (len < 0)
            {
                len = s.Length;
            }
            rtVal = ((len == 0) ? "" : s.Substring(0, len));
            s = ((len >= s.Length - 1) ? "" : s.Substring(len + 1));

            return rtVal;
        }

        private static int CountContinuousQuotes(string s, int startIdx = 0)
        {
            int cnt = 0;
            for (var i = startIdx; i < s.Length; i++)
            {
                if (s[i] != '"')
                {
                    break;
                }
                cnt++;
            }
            return cnt;
        }

        private static bool IsStartWithQuote(string s, int n)
        {
            return n > 0 && (n % 2 == 1 || (s.Length > n && s[n] == ',') || s.Length == n);
        }

        private static string GetQuoteField(ref string s, int n, StreamReader sr)
        {
            var fieldVal = string.Empty;
            if (n % 2 == 0)
            {
                Util.Assert(s[n] == ',');
                fieldVal = ((n == 2) ? "" : s.Substring(1, n - 2));
                s = ((s.Length > n + 1) ? s.Substring(n + 1) : "");
            }
            else
            {
                int rightQuote = GetRightQuote(ref s, n, sr);
                Util.Assert(rightQuote > n);
                fieldVal = s.Substring(1, rightQuote - 1);
                s = ((s.Length > rightQuote + 2) ? s.Substring(rightQuote + 2) : "");
            }
            if (fieldVal.Length > 0 && fieldVal[0] == '\t')
            {
                fieldVal = fieldVal.Substring(1);
            }
            return fieldVal.Replace("\"\"", "\"");
        }

        private static int GetRightQuote(ref string s, int startIdx, StreamReader sr)
        {
            int nextQuote = GetNextQuote(ref s, startIdx, sr);
            Util.Assert(IsRightQuote(s, nextQuote));
            return nextQuote;
        }

        private static bool IsRightQuote(string s, int idx)
        {
            return s.Length <= idx + 1 || s[idx + 1] == ',';
        }

        private static int GetNextQuote(ref string s, int sidx, StreamReader sr)
        {
            int i = -1;
            while (i < 0)
            {
                i = s.IndexOf('"', sidx);
                if (i < 0)
                {
                    int length = s.Length;
                    s = s + "\r\n" + sr.ReadLine();
                    sidx = length + 2;
                }
                else
                {
                    int cnt = CountContinuousQuotes(s, i);
                    if (cnt % 2 == 1)
                    {
                        i += cnt - 1;
                    }
                    else
                    {
                        sidx = i + cnt;
                        i = -1;
                    }
                }
            }
            return i;
        }

        public static void WriteCsvFile(List<List<string>> lines, string fn, HashSet<int> dontEncodeColumns = null)
        {
            WriteCsvFile(lines, fn, Encoding.GetEncoding("gb2312"), dontEncodeColumns);
        }

        private static void WriteCsvFile(List<List<string>> lines, string fn, Encoding enc, HashSet<int> dontEncodeColumns)
        {
            using (var writer = new StreamWriter(fn, false, enc))
            {
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    sb.Clear();
                    for (int i = 0; i < line.Count; i++)
                    {
                        var text = line[i] ?? "";
                        if (dontEncodeColumns == null || !dontEncodeColumns.Contains(i))
                        {
                            sb.Append(string.Format("\"\t{0}\"", text.Replace("\"", "\"\"")));
                        }
                        else
                        {
                            sb.Append(text);
                        }
                        if (i < line.Count - 1)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            sb.Append("\r\n");
                        }
                    }
                    writer.Write(sb.ToString());
                }
            }
        }
    }
}
