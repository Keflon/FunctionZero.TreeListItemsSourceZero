using System;

namespace FunctionZero.TreeListItemsSourceZero
{
    public class TreeNodeContainerEventArgs<T> : EventArgs
    {
        public NodeAction Action { get; }
        public TreeNodeContainer<T> Node { get; }


        public TreeNodeContainerEventArgs(TreeNodeContainer<T> node, NodeAction action)
        {
            Action = action;
            Node = node;
        }
    }

    public enum NodeAction
    {
        Added,
        Removed,
        IsExpandedChanged,
        IsVisibleChanged
        // TODO: RefreshRequested etc.
    }
}