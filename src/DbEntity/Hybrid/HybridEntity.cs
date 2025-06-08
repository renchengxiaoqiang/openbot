using System;
using BotLib.Db.Sqlite;

namespace DbEntity
{
	public class HybridEntity : EntityBase
	{
		private string _key;
		private string _value;
        
        [NotNull]
		public string Key
		{
			get
			{
				return this._key;
			}
			set
			{
				base.SetValue<string>(ref this._key, value);
			}
		}

		public string Value
		{
			get
			{
				return this._value;
			}
			set
			{
				base.SetValue<string>(ref this._value, value);
			}
		}
	}
}
