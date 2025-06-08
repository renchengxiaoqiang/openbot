using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Wpf.Extensions
{
    public static class SaveFileDialogEx
    {
        public static string GetSaveFilename(string title = null, string filter = null, string defaultFilename = null)
        {
            string fileName = null;
            var dlg = new SaveFileDialog();
            if (!string.IsNullOrEmpty(title))
            {
                dlg.Title = title;
            }
            if (!string.IsNullOrEmpty(filter))
            {
                dlg.Filter = filter;
            }
            if (!string.IsNullOrEmpty(defaultFilename))
            {
                dlg.FileName = defaultFilename;
            }
            bool? isShow = dlg.ShowDialog();
            if (isShow.HasValue && isShow.Value)
            {
                fileName = dlg.FileName.Trim();
            }
            return fileName;
        }
    }
}
