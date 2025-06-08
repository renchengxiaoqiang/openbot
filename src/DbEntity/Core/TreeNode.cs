using System;

namespace DbEntity
{
	public class TreeNode : EntityBase
    {
        private string _nextId;
        private string _parentId;
        private string _prevId;

		public string NextId
		{
			get
			{
				return this._nextId;
			}
			set
			{
				base.SetValue<string>(ref this._nextId, value);
			}
		}

		public string ParentId
		{
			get
			{
				return this._parentId;
			}
			set
			{
				base.SetValue<string>(ref this._parentId, value);
			}
		}

		public string PrevId
		{
			get
			{
				return this._prevId;
			}
			set
			{
				base.SetValue<string>(ref this._prevId, value);
			}
		}

		public void CopyTreeNodeDataFrom(TreeNode bak)
		{
			base.DbAccount = bak.DbAccount;
			base.EntityId = bak.EntityId;
			base.IsDeleted = bak.IsDeleted;
			base.ModifyTick = bak.ModifyTick;
			this.NextId = bak.NextId;
			this.ParentId = bak.ParentId;
			this.PrevId = bak.PrevId;
		}
	}
}
