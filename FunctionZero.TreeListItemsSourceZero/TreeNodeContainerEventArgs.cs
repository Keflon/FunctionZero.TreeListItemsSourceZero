using System;

namespace FunctionZero.TreeListItemsSourceZero
{
    public class TreeNodeContainerEventArgs : EventArgs
    {
        public NodeAction Action { get; }
        public object Node { get; }


        public TreeNodeContainerEventArgs(object node, NodeAction action)
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