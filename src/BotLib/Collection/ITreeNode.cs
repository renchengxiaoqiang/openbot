using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotLib.Collection
{
    public interface ITreeNode<T> where T : ITreeNode<T>
	{
		string Key { get; set; }
		string ParentKey { get; set; }
		string PrevSiblingKey { get; set; }
		List<T> Children { get; set; }
	}
}
