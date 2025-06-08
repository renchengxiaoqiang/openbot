using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BotLib.Wpf.Extensions
{
    public static class ComboBoxEx
    {
        public static string xGetSelectedItemText(this ComboBox combo)
        {
            if (combo == null || combo.SelectedItem == null) return null;
            var it = combo.SelectedItem as ComboBoxItem;
            return it.Content.ToString();
        }
    }
}
