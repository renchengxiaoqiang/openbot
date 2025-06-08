using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BotLib.Wpf.Extensions
{
    public static class BitmapImageEx
    {
        public static BitmapImage CreateFromFile(string filename, int maxTry = 3)
        {
            BitmapImage image = null;
            if (maxTry < 1)
            {
                maxTry = 1;
            }
            if (maxTry > 5)
            {
                maxTry = 5;
            }
            if (File.Exists(filename))
            {
                for (int i = 0; i < maxTry; i++)
                {
                    try
                    {
                        image = BitmapImageEx.LoadImage(filename) as BitmapImage;
                        if ( image != null)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("BitmapImageEx.CreateFromFile:" + ex.Message);
                        Log.Exception(ex);
                        Thread.Sleep(200);
                    }
                }
            }
            return image;
        }

        public static bool TryCreateFromFile(string filename, out BitmapImage img)
        {
            img = null;
            bool rt = false;
            try
            {
                img = CreateFromFile(filename, 3);
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        public static ImageSource LoadImage(string fn)
        {
            ImageSource src = null;
            try
            {
                byte[] imageBytes = LoadImageData(fn);
                if (imageBytes != null)
                {
                    src = CreateImage(imageBytes, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("BitmapImageEx.LoadImage,fn={0},err={1}", fn, ex.Message));
            }
            return src;
        }

        public static bool ResizeImageAndSave(string fn, int edgeMax)
        {
            bool rt = false;
            try
            {
                byte[] imageBytes = BitmapImageEx.LoadImageData(fn);
                if (imageBytes != null)
                {
                    ImageSource imageSource = BitmapImageEx.CreateImage(imageBytes, 0, 0);
                    if (imageSource.Width >= imageSource.Height)
                    {
                        imageSource = BitmapImageEx.CreateImage(imageBytes, edgeMax, 0);
                    }
                    else
                    {
                        imageSource = BitmapImageEx.CreateImage(imageBytes, 0, edgeMax);
                    }
                    imageBytes = BitmapImageEx.GetEncodedImageData(imageSource, ".png");
                    BitmapImageEx.SaveImageData(imageBytes, fn);
                    rt = true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private static byte[] LoadImageData(string filePath)
        {
            byte[] imageBytes = null;
            if (File.Exists(filePath))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (LoadImageDataInner(filePath, out imageBytes))
                    {
                        break;
                    }
                    Thread.Sleep(300);
                }
            }
            return imageBytes;
        }


        private static bool LoadImageDataInner(string filePath, out byte[] imageBytes)
        {
            bool rt = false;
            imageBytes = null;
            if (Monitor.TryEnter(filePath, 1000))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(fileStream))
                        {
                            imageBytes = binaryReader.ReadBytes((int)fileStream.Length);
                            binaryReader.Close();
                        }
                        fileStream.Close();
                    }
                    rt = true;
                }
                catch (Exception ex)
                {
                    Log.Error("BitmapImageEx.LoadImageDataInner" + ex.Message);
                }
                finally
                {
                    Monitor.Exit(filePath);
                }
            }
            else
            {
                Log.Error("获取锁超时");
            }
            return rt;
        }

        private static void SaveImageData(byte[] imageData, string filePath)
        {
            for (int i = 0; i < 10; i++)
            {
                if ( BitmapImageEx.SaveImageDataInner(imageData, filePath))
                {
                    break;
                }
                Thread.Sleep(300);
            }
        }

        private static bool SaveImageDataInner(byte[] imageData, string filePath)
        {
            bool rt = false;
            if (Monitor.TryEnter(filePath, 10000))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                        {
                            binaryWriter.Write(imageData);
                            binaryWriter.Close();
                        }
                        fileStream.Close();
                    }
                    rt = true;
                }
                catch (Exception ex)
                {
                    Log.Error("BitmapImageEx.SaveImageDataInner" + ex.Message);
                }
                finally
                {
                    Monitor.Exit(filePath);
                }
            }
            else
            {
                Log.Error("获取锁超时");
            }
            return rt;
        }

        private static BitmapSource CreateImage(byte[] imageData,
            int decodePixelWidth, int decodePixelHeight)
        {
            if (imageData == null) return null;

            var rtimg = new BitmapImage();
            rtimg.BeginInit();
            if (decodePixelWidth > 0)
            {
                rtimg.DecodePixelWidth = decodePixelWidth;
            }
            if (decodePixelHeight > 0)
            {
                rtimg.DecodePixelHeight = decodePixelHeight;
            }
            rtimg.StreamSource = new MemoryStream(imageData);
            rtimg.CreateOptions = BitmapCreateOptions.None;
            rtimg.CacheOption = BitmapCacheOption.Default;
            rtimg.EndInit();
            return rtimg;
        }

        private static byte[] GetEncodedImageData(ImageSource image, string preferredFormat)
        {
            byte[] rtds = null;
            BitmapEncoder encoder = null;
            switch (preferredFormat.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;

                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;

                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;

                case ".tif":
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;

                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;

                case ".wmp":
                    encoder = new WmpBitmapEncoder();
                    break;
            }

            if (image is BitmapSource)
            {
                var stream = new MemoryStream();
                encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                rtds = new byte[stream.Length];
                var br = new BinaryReader(stream);
                br.Read(rtds, 0, (int)stream.Length);
                br.Close();
                stream.Close();
            }
            return rtds;
        }
    }
}
