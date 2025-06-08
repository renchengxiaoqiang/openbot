using System;
using BotLib.Db.Sqlite;

namespace DbEntity
{
	public class TreeCatalog : TreeNode
	{
		[NotNull]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				base.SetValue<string>(ref this._name, value);
			}
		}

		public static string GetNamePropertyName()
		{
			return "Name";
		}

		private string _name;
	}
}
