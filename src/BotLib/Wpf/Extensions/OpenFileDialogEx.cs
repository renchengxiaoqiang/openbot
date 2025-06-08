using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Wpf.Extensions
{
    public static class OpenFileDialogEx
    {
        public static string GetOpenFileName(string title = null, string filter = null, string oldFileName = null)
        {
            string fileName = null;
            var dlg= new OpenFileDialog();
            if (!string.IsNullOrEmpty(filter))
            {
                dlg.Filter = filter;
            }
            if (!string.IsNullOrEmpty(title))
            {
                dlg.Title = title;
            }
            if (!string.IsNullOrEmpty(oldFileName))
            {
                dlg.FileName = oldFileName;
            }
            try
            {
                bool? isShow = dlg.ShowDialog();
                if (isShow.HasValue && isShow.Value)
                {
                    fileName = dlg.FileName;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return fileName;
        }
    }
}
