using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class SimpleTree<T>
	{
		public SimpleTreeNode<T> Root = new SimpleTreeNode<T>(default(T));

		public void TraverseTreeNode(Action<SimpleTreeNode<T>> act, bool includeRoot = false)
		{
			if (includeRoot)
			{
				act(this.Root);
			}
			this.TraverseChildren(this.Root.Children, act);
		}

		private void TraverseChildren(List<SimpleTreeNode<T>> children, Action<SimpleTreeNode<T>> act)
		{
			foreach (SimpleTreeNode<T> simpleTreeNode in children)
			{
				act(simpleTreeNode);
				if (simpleTreeNode.Children.Count > 0)
				{
					this.TraverseChildren(simpleTreeNode.Children, act);
				}
			}
		}

	}
}
