using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
   public interface ITreeDb<T> where T : ITreeNode<T>
	{
		void SaveNode(T node);

		void DeleteNode(T node);

		T ReadNode(string key);

		List<T> ReadChildren(string parentKey);
	}
}
