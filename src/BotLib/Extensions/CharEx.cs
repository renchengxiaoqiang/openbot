using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class CharEx
    {
        static List<char> numbers;
        static List<char> chineseNumbers;
        static CharEx(){
            numbers = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            chineseNumbers = new List<char> { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖' };
        }

        public static bool xIsChinese(this char ch)
        {
            return ch >= '一' && ch <= '龥';
        }

        public static bool xIsNotEscapedCharacter(this string s, int idx, char ch, char escape = '\\')
        {
            return s[idx] == ch && s.xNotEscaped(idx, escape);
        }

        public static bool xIsEndWithLeftBrace(this string s)
        {
            return s.Length > 0 && s[s.Length - 1].xIsLeftBrace();
        }

        public static bool xIsLeftBrace(this char c)
        {
            return c == '{' || c == '｛';
        }

        public static bool xIsRightBrace(this char c)
        {
            return c == '}' || c == '｝';
        }

        public static bool xIsBackSlash(this char c)
        {
            return c == '\\' || c == '＼';
        }

        public static bool xIsSpace(this char c)
        {
            return c == ' ' || c == '\u3000';
        }

        public static bool xIsStartWithRightBrace(this string s)
        {
            return s.Length > 0 && s[0].xIsRightBrace();
        }

        public static char ChineseCharToArabChar(this char c)
        {
            var idx = chineseNumbers.ToList().IndexOf(c);
            if (idx == -1) return c;
            if (idx > 9) idx -= 9;
            return numbers[idx];
        }
    }
}
