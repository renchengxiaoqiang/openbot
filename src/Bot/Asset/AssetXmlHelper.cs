using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bot.Asset
{
    public class AssetXmlHelper
    {
        public static string GetAsset(string assetName)
        {
            string packUri = "pack://application:,,,/Asset/" + assetName;
            string rt;
            using (var stream = Application.GetResourceStream(new Uri(packUri)).Stream)
            {
                using (var sr = new StreamReader(stream))
                {
                    rt = sr.ReadToEnd();
                }
            }
            return rt;
        }

        public static Stream GetAssetStream(string assetName)
        {
            string packUri = "pack://application:,,,/Asset/" + assetName;
            return Application.GetResourceStream(new Uri(packUri)).Stream;
        }
    }
}
