using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using BotLib.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.Goods
{
	public partial class CtlSkus : UserControl
	{
		public CtlSkus()
		{
			InitializeComponent();
		}

		public void Init(List<ZnkfItemSku> skus)
		{
			if (skus.xIsNullOrEmpty())
			{
				Visibility = Visibility.Collapsed;
			}
			else
			{
				Visibility = Visibility.Visible;
                RemoveGridRow();
				AddGridRow(skus.Count);
				InitUI(skus);
			}
		}
        private void InitUI(List<ZnkfItemSku> skus)
		{
            for (int i = 0; i < skus.Count; i++)
			{
                ZnkfItemSku skuItem = skus[i];
				var txtProp = new TextBlock();
				txtProp.Padding = new Thickness(6.0, 3.0, 3.0, 3.0);
				txtProp.Text = skuItem.propsName;
				Grid.SetRow(txtProp, i + 1);
				txtProp.MouseDown += txtBlock_MouseDown;
				gridMain.Children.Add(txtProp);
                var txtOuterId = new TextBlock();
                txtOuterId.Padding = new Thickness(6.0, 3.0, 3.0, 3.0);
                txtOuterId.Text = skuItem.skuId.ToString();
                Grid.SetRow(txtOuterId, i + 1);
                Grid.SetColumn(txtOuterId, 1);
                txtOuterId.MouseDown += txtBlock_MouseDown;
                gridMain.Children.Add(txtOuterId);
				var txtPrice = new TextBlock();
				txtPrice.Padding = new Thickness(6.0, 3.0, 3.0, 3.0);
				txtPrice.Text = skuItem.price;
				Grid.SetRow(txtPrice, i + 1);
				Grid.SetColumn(txtPrice, 2);
				txtPrice.MouseDown += txtBlock_MouseDown;
				gridMain.Children.Add(txtPrice);
			}
		}

		private void txtBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var uielement = sender as UIElement;
			if (uielement != null)
			{
				int row = Grid.GetRow(uielement);
				if (row > 0)
				{
					Grid.SetRow(recSelectBack, row);
				}
			}
		}

		private void AddGridRow(int skuNum)
		{
			for (int i = 0; i < skuNum; i++)
			{
				var rowDef = new RowDefinition();
				rowDef.Height = new GridLength(18.0);
				gridMain.RowDefinitions.Add(rowDef);
			}
		}

        private void RemoveGridRow()
		{
			if (gridMain.RowDefinitions.Count > 1)
			{
				gridMain.RowDefinitions.RemoveRange(1, gridMain.RowDefinitions.Count - 1);
			}
			var uieles = new List<UIElement>();
            foreach (UIElement element in gridMain.Children)
			{
                if (Grid.GetRow(element) > 0)
				{
                    uieles.Add(element);
				}
			}
			foreach (UIElement element in uieles)
			{
				gridMain.Children.Remove(element);
			}
		}
	}
}
