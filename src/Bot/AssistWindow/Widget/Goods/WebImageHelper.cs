using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Bot.Asset;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.Widget.Right.Goods
{
    public class WebImageHelper
    {
        private static BitmapImage _imgLoading;
        private static object _synobj;
        static WebImageHelper()
        {
            _synobj = new object();
        }

        public static async void GetImageFromUrl(string url, Image image, bool useCache = false)
        {
            var img = useCache ? null : ReadImageFromCachedFile(url);
            if (img == null)
            {
                image.Source = ImgLoading;
                await Task.Factory.StartNew(() =>
                {
                    DownImageAndSave(url);
                }, TaskCreationOptions.LongRunning);
                img = ReadImageFromCachedFile(url);
            }
            image.Source = img;
        }

        public static BitmapImage ImgLoading
        {
            get
            {
                if (_imgLoading == null)
                {
                    try
                    {
                        _imgLoading = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgLoading);
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                }
                return _imgLoading;
            }
        }

        private static string GetImageFileName(string url)
        {
            string subDirOfData = PathEx.GetSubDirOfData("imgcache");
            if (!Directory.Exists(subDirOfData))
            {
                Directory.CreateDirectory(subDirOfData);
            }
            return Path.Combine(subDirOfData, url.GetHashCode().ToString());
        }

        private static void DownImageAndSave(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string text = GetImageFileName(url);
                FileEx.DeleteWithoutException(text);
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(url, text);
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                        Log.Info("无法下载图片，url=" + url);
                    }
                }
                try
                {
                    BitmapImageEx.ResizeImageAndSave(text, 300);
                }
                catch (Exception e2)
                {
                    Log.Exception(e2);
                }
            }
        }

        private static BitmapImage ReadImageFromCachedFile(string url)
        {
            BitmapImage img = null;
            lock (_synobj)
            {
                try
                {
                    string filename = GetImageFileName(url);
                    img = BitmapImageEx.CreateFromFile(filename, 3);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
            return img;
        }

    }
}
