using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FunctionZero.TreeListItemsSourceZero
{
    public class TreeItemsSourceManager<T> : TreeNodeContainer<T>
    {
        /// <summary>
        /// This is what the GridView binds to, via 'TreeNodeChildren ReadOnlyObservableCollection<TreeNodeContainer<T>>'.
        /// </summary>
        private readonly ObservableCollection<TreeNodeContainer<T>> _itemsSource;
        private bool _isTreeRootShown;

        public bool IsTreeRootShown
        {
            get => _isTreeRootShown;
            set
            {
                if (value != _isTreeRootShown)
                {
                    _isTreeRootShown = value;
                    OnPropertyChanged();
                }
            }
        }

        internal Func<T, IEnumerable> GetChildren { get; }
        internal Func<T, bool> GetCanHaveChildren { get; }

        public delegate void TreeGridNodeEventHandler(object sender, TreeNodeContainerEventArgs e);

        public event TreeGridNodeEventHandler NodeChanged;

        public ReadOnlyObservableCollection<TreeNodeContainer<T>> TreeNodeChildren { get; }

        public TreeItemsSourceManager(bool isTreeRootShown, T data, Func<T, bool> getCanHaveChildren, Func<T, IEnumerable> getChildren) : base(null, data)
        {
            _itemsSource = new ObservableCollection<TreeNodeContainer<T>>();
            TreeNodeChildren = new ReadOnlyObservableCollection<TreeNodeContainer<T>>(_itemsSource);

            IsTreeRootShown = isTreeRootShown;
            if (IsTreeRootShown == false)
                OnPropertyChanged(nameof(IsTreeRootShown));
            GetChildren = getChildren;
            GetCanHaveChildren = getCanHaveChildren;

            this.IsVisible = true;
        }

        // TODO: If action is NodeCollapsed, set a bool in eventargs to signal to caller (watch out for other subscribers) the child TreeGridNodes should be deallocated.
        // TODO: And / or have a strategy e.g. recycle 1000 containers, automatically recycle collapsed nodes by oldest first.
        // TODO: This is the connection between UI-databound TreeGridNodes (and their Data) and the app., so other cool stuff might go here.
        // TODO: UPDATE: Be aware, the DataGrid recycles DataGridRow objects by applying a new DataContext.
        private void OnNodeChanged(TreeNodeContainerEventArgs e)
        {
            NodeChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Something has happened to this TreeGridNode<T>
        /// </summary>
        /// <param name="action">What happened</param>
        internal void ChangeNode(TreeNodeContainer<T> node, NodeAction action)
        {
            // action applies to 'node'.
            switch (action)
            {
                case NodeAction.Added:
                    if (node.IsExpanded != false)
                        throw new InvalidOperationException("Attempt to add aTreeGridNode where IsExpanded == true. If that's deliberate, ask, and the developer will supoport it.");

                    if (node.Parent.IsVisible && node.Parent.IsExpanded)
                        node.IsVisible = true;

                    if (node.IsVisible)
                        // If the newly added node is expanded, it will already have children. Add them to the ItemsSource.
                        if (node.IsExpanded == true)
                            foreach (var child in node.Children)
                                child.IsVisible = true;

                    break;
                case NodeAction.Removed:
                    // If the newly removed node had children, remove them from the ItemsSource. TEST! THINK! Will the Children already be dealt with?
                    // If not (they're not) then ought they be dealt with *before* removing 'this'.
                    // Memory leak here? If we recycle containers, we need to reclaim the child containers, otherwise the GC will claim them (confirm there are no other references to them)
                    if (node.IsExpanded == true)
                        foreach (var child in node.Children)
                            child.IsVisible = false;

                    node.IsVisible = false;
                    break;

                case NodeAction.IsExpandedChanged:
                    if (node.IsExpanded == true)
                    {
                        if (node._hasMadeChildren == false)
                            node.MakeChildContainers();

                        foreach (var child in node.Children)
                            child.IsVisible = true;
                    }
                    else
                    {
                        Debug.Assert(node._hasMadeChildren == true);

                        foreach (var child in node.Children)
                            child.IsVisible = false;
                    }
                    break;
                case NodeAction.IsVisibleChanged:
                    node.UpdateShowChevron();

                    if (node.IsVisible == true)
                    {
                        // Will do nothing if node is root node.
                        // Root node is handled separately.
                        Insert(node);

                        if (node.IsExpanded)
                        {
                            foreach (var child in node.Children)
                                child.IsVisible = true;
                        }
                    }
                    else
                    {
                        foreach (var child in node.Children)
                            child.IsVisible = false;
                        _itemsSource.Remove(node);
                    }
                    break;
            }
            OnNodeChanged(new TreeNodeContainerEventArgs(node, action));
        }

        // TODO: Consider some sort of sort-provider.
        // TODO: This can be substantially optimised because it will often be called in batches with items from the same parent.
        private int Insert(TreeNodeContainer<T> item)
        {
            // Root node is handled separately.
            if (item == this)
                return -1;

            // IndexOf returns -1 if not found.
            int insertIndex = _itemsSource.IndexOf(item.Parent);
            insertIndex += GetInsertOffset(item.Parent);
            _itemsSource.Insert(insertIndex, item);
            return insertIndex;
        }

        private int GetInsertOffset(TreeNodeContainer<T> item)
        {
            int offset = 0;

            foreach (var child in item.Children)
                if (child.IsVisible)
                    offset += GetInsertOffset(child) + 1;

            return offset;
        }

        protected override async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsTreeRootShown))
            {
                if (IsTreeRootShown)
                    _itemsSource.Insert(0, this);
                else
                    _itemsSource.Remove(this);

                foreach (TreeNodeContainer<T> node in _itemsSource)
                    node.UpdateIndent();
            }
        }
    }
}