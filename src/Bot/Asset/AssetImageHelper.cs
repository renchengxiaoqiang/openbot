using BotLib.Collection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bot.Asset
{
    public class AssetImageHelper
    {
        private static Cache<AssetImageEnum, BitmapImage> _wpfCache;

        private static Cache<AssetImageEnum, Bitmap> _winformCache;
        static AssetImageHelper()
        {
            _wpfCache = new Cache<AssetImageEnum, BitmapImage>(0, 0, null);
            _winformCache = new Cache<AssetImageEnum, Bitmap>(0, 0, null);
        }

        public static BitmapImage GetImageFromWpfCache(AssetImageEnum assetImage)
        {
            return _wpfCache.GetValue(assetImage, () => Application.Current.FindResource(assetImage.ToString()) as BitmapImage, true, null);
        }

        public static Bitmap GetImageFromWinFormCache(AssetImageEnum assetImage)
        {
            return _winformCache.GetValue(assetImage, () => GetImageFromAppResource(GetImageFromWpfCache(assetImage)), true, null);
        }

        private static Bitmap GetImageFromAppResource(ImageSource imageSrc)
        {
            Bitmap image;
            if (imageSrc == null)
            {
                image = null;
            }
            else
            {
                Uri uriResource = new Uri(imageSrc.ToString());
                image = new Bitmap(Application.GetResourceStream(uriResource).Stream);
            }
            return image;
        }

    }
}
