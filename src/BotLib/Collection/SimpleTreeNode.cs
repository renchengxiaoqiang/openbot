using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class SimpleTreeNode<T>
    {
        public T Data;
        public object Tag;
        public List<SimpleTreeNode<T>> Children;

        public SimpleTreeNode(T data)
		{
			Data = data;
			Children = new List<SimpleTreeNode<T>>();
		}

    }
}
