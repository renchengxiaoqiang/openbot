using System;
using System.Collections.Generic;
using BotLib.Db.Sqlite;

namespace DbEntity
{
	public class EntityBase
    {
        private string _entityId;
        private long _modifyTick;
        private string _dbAccount;
        private bool _isDeleted;
        private string _tag;
        private bool _isReadOnly = false;

		[PrimaryKey]
		public string EntityId
		{
			get
			{
				return this._entityId;
			}
			set
			{
				this.SetValue<string>(ref this._entityId, value);
			}
		}

		[Indexed]
		[NotNull]
		public long ModifyTick
		{
			get
			{
				return this._modifyTick;
			}
			set
			{
				this.SetValue<long>(ref this._modifyTick, value);
			}
		}

		[Indexed]
		[NotNull]
		public string DbAccount
		{
			get
			{
				return this._dbAccount;
			}
			set
			{
				this.SetValue<string>(ref this._dbAccount, value);
			}
		}

		public bool IsDeleted
		{
			get
			{
				return this._isDeleted;
			}
			set
			{
				this.SetValue<bool>(ref this._isDeleted, value);
			}
		}

		public string Tag
		{
			get
			{
				return this._tag;
			}
			set
			{
				this.SetValue<string>(ref this._tag, value);
			}
		}

		public EntityBase Clone(bool isreadonly = false)
		{
			EntityBase EntityBase = base.MemberwiseClone() as EntityBase;
			EntityBase.SetReadOnly(isreadonly);
			return EntityBase;
		}

		public ReturnType Clone<ReturnType>(bool isreadonly = false) where ReturnType : EntityBase
		{
			ReturnType returnType = (ReturnType)((object)base.MemberwiseClone());
			returnType.SetReadOnly(isreadonly);
			return returnType;
		}

		public void SetReadOnly(bool v)
		{
			this._isReadOnly = v;
		}

		public bool IsReadOnly()
		{
			return this._isReadOnly;
		}

		public void SetValue<T>(ref T field, T value)
		{
			bool isReadOnly = this._isReadOnly;
			if (isReadOnly)
			{
				throw new InvalidOperationException("不能修改只读对象");
			}
			field = value;
		}

	}
}
