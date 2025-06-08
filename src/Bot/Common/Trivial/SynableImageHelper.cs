using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Bot.Asset;
using DbEntity;

namespace Bot.Common.Trivial
{
    public class SynableImageHelper
    {
        private TransferFileTypeEnum _fileType;
        private readonly string _imageDir;
        private static HashSet<string> validImgExtensions;

        static SynableImageHelper()
        {
            validImgExtensions = new HashSet<string>
			{
				".jpg",
				".png",
				".gif",
				".jpeg",
				".tif",
				".tiff"
			};
        }
        public SynableImageHelper(TransferFileTypeEnum transferFileType)
        {
            _fileType = transferFileType;
            _imageDir = PathEx.GetSubDirOfData(transferFileType.ToString());
        }

        public bool CopyTo(string src, string dest)
        {
            bool rt = false;
            if (!string.IsNullOrEmpty(src))
            {
                string srcPath = GetImagePath(src);
                if (File.Exists(srcPath))
                {
                    var destFileName = Path.Combine(dest, src);
                    File.Copy(srcPath, destFileName);
                    rt = true;
                }
            }
            return rt;
        }

        public string GetImagePath(string fn)
        {
            return _imageDir + fn;
        }

        public async void UseImage(string fn, Action<BitmapImage> cb)
        {
            if (!string.IsNullOrEmpty(fn))
            {
                var imagePath = GetImagePath(fn);
                BitmapImage img = BitmapImageEx.CreateFromFile(imagePath, 3);
                if (img == null)
                {
                    img = BitmapImageEx.CreateFromFile(imagePath, 3);
                    if (img == null)
                    {
                        img = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgCantFindImage);
                    }
                }
                cb(img);
                img = null;
            }
        }

        public BitmapImage UseImage(string fn)
        {
            BitmapImage img = null;
            if (!string.IsNullOrEmpty(fn))
            {
                var imagePath = GetImagePath(fn);
                img = BitmapImageEx.CreateFromFile(imagePath, 3);
                if (img == null)
                {
                    img = BitmapImageEx.CreateFromFile(imagePath, 3);
                    if (img == null)
                    {
                        img = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgCantFindImage);
                    }
                }
            }
            return img;
        }

        public bool CopyImageIntoCache(string fn)
        {
            var rt = false;
            try
            {
                var fileName = Path.GetFileName(fn);
                if (!ExistsFile(fileName, fn))
                {
                    string fullName = GetImagePath(fileName);
                    if (File.Exists(fullName))
                    {
                        FileEx.DeleteWithoutException(fullName);
                    }
                    File.Copy(fn, fullName);
                }
                else
                {
                    rt = true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                rt = false;
            }
            return rt;
        }

        private bool ExistsFile(string fn1, string fn2)
        {
            bool rt = false;
            var img1Path = GetImagePath(fn1);
            if (img1Path != null && File.Exists(img1Path) && FileEx.IsTwoFileEqual(img1Path, fn2))
            {
                rt = true;
            }
            return rt;
        }

        public async void UseImage(string fn, System.Windows.Controls.Image imageControl)
        {
            if (!string.IsNullOrEmpty(fn))
            {
                var fullName = GetImagePath(fn);
                BitmapImage img = BitmapImageEx.CreateFromFile(fullName, 3);
                if (img == null)
                {
                    img = BitmapImageEx.CreateFromFile(fullName, 3);
                }
                imageControl.Source = img;
                img = null;
            }
        }

        public void DeleteImage(string fn)
        {
            if (!string.IsNullOrEmpty(fn))
            {
                var fullName = GetImagePath(fn);
                FileEx.DeleteWithoutException(fullName);
                //BotSvrApi.Delete(fn, _fileType);
            }
        }

        public string AddNewImage(string fn, string parnFnOld)
        {
            if (parnFnOld != null)
            {
                DeleteImage(parnFnOld);
            }
            return AddNewImage(fn);
        }

        public string AddNewImage(string fullFn)
        {
            string imagePath = null;
            string extension = Path.GetExtension(fullFn);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.ToLower();
            }
            Util.Assert(validImgExtensions.Contains(extension));
            for (int i = 0; i < 2; i++)
            {
                imagePath = StringEx.xGenGuidB32Str() + extension;
                var fullName = GetImagePath(imagePath);
                File.Copy(fullFn, fullName);
                FileEx.DeleteWithoutException(fullName);
                imagePath = null;
            }
            return imagePath;
        }

    }

}
