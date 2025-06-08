using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BotLib.Wpf.Controls
{
    public class CtlHighlightableTextBlock : TextBlock
    {
        public Brush HightlightForeground { get; set; }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;
                    HighlightKeyText();
                }
            }
        }

        public new Brush Foreground
        {
            get
            {
                return base.Foreground;
            }
            set
            {
                if (value != base.Foreground)
                {
                    base.Foreground = value;
                    HighlightKeyText();
                }
            }
        }

        public string[] HighlightKeys
        {
            get
            {
                return _hightlightKeys;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                string[] newArr = DistinctAndSortByLengthDesc(value);
                if (!IsEqual(newArr, _hightlightKeys))
                {
                    _hightlightKeys = newArr;
                    HighlightKeyText();
                }
            }
        }

        private string[] _hightlightKeys;

        public CtlHighlightableTextBlock()
        {
            InitPropertiesDefaultValue();
        }

        private void InitPropertiesDefaultValue()
        {
            HightlightForeground = Brushes.Red;
        }

        private string[] DistinctAndSortByLengthDesc(string[] arr)
        {
            List<string> lst = new List<string>();
            foreach (string text in arr)
            {
                if (!lst.Contains(text.Trim()))
                {
                    lst.Add(text.Trim());
                }
            }
            lst = lst.OrderByDescending(k => k.Length).ToList();
            return lst.ToArray();
        }

        private bool IsEqual(string[] arr1, string[] arr2)
        {
            if (arr1 == null && arr2 == null) return true;
            if (arr1 == null && arr2 != null) return false;
            if (arr1 != null && arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void HighlightKeyText()
        {
            if (!string.IsNullOrEmpty(Text) && HighlightKeys != null && HighlightKeys.Length > 0)
            {
                Dictionary<string, bool> dictionary;
                List<string> list = Split(Text, HighlightKeys, out dictionary);
                Inlines.Clear();
                foreach (string text in list)
                {
                    Run run = new Run(text);
                    run.Foreground = (dictionary.ContainsKey(text) ? HightlightForeground : Foreground);
                    base.Inlines.Add(run);
                }
            }
        }

        private List<string> Split(string text, string[] hkeys, out Dictionary<string, bool> keydict)
        {
            keydict = new Dictionary<string, bool>();
            List<string> list = new List<string>();
            foreach (string key in hkeys)
            {
                keydict[key] = true;
            }
            int idx = 0;
            while (text.Length > idx)
            {
                bool startWithHkey = false;
                foreach (string hkey in hkeys)
                {
                    if (text.IndexOf(hkey, idx) == idx)
                    {
                        if (idx != 0)
                        {
                            list.Add(text.Substring(0, idx));
                        }
                        list.Add(hkey);
                        text = text.Substring(idx + hkey.Length);
                        idx = 0;
                        startWithHkey = true;
                        break;
                    }
                }
                if (!startWithHkey)
                {
                    idx++;
                }
            }
            if (text.Length > 0)
            {
                list.Add(text);
            }
            return list;
        }

        private List<string> SplitKeyAndSortByLenghDescendant(string highlightText)
        {
            var splitArr = highlightText.Split(new char[]{' ','\u3000'}, StringSplitOptions.RemoveEmptyEntries);
            var keys = new List<string>();
            foreach (string item in splitArr)
            {
                if (!keys.Contains(item))
                {
                    keys.Add(item);
                }
            }
            keys = keys.OrderByDescending(k => k.Length).ToList();
            return keys;
        }

    }

}
