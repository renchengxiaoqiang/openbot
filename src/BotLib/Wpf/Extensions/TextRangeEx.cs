using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace BotLib.Wpf.Extensions
{
    public static class TextRangeEx
    {
        public static string GetRichContentXaml(this TextRange range)
        {
            return range.GetRichContent(DataFormats.Xaml);
        }

        public static string GetRichContent(this TextRange rt, string format)
        {
            string content = "";
            var memoryStream = new MemoryStream();
            rt.Save(memoryStream, format);
            memoryStream.Position = 0L;
            using (var streamReader = new StreamReader(memoryStream))
            {
                content = streamReader.ReadToEnd();
            }
            return content;
        }

        public static object GetPropertyValueEx(this TextRange range, DependencyProperty prop)
        {
            object propertyValue = range.GetPropertyValue(prop);
            if (propertyValue == DependencyProperty.UnsetValue)
            {
                TextRange textRange = new TextRange(range.Start, range.Start);
                propertyValue = textRange.GetPropertyValue(prop);
            }
            return propertyValue;
        }
    }
}
