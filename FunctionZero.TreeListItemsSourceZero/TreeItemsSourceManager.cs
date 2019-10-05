using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        internal Predicate<T> _filterPredicate;

        internal Comparison<T> _sortComparison;

        public void SetFilterPredicate(Predicate<T> predicate)
        {
            _filterPredicate = predicate;
            FilterNode(this);
        }

        public void SetSortComparison(Comparison<T> sortComparison)
        {
            _sortComparison = sortComparison;
            if (this.IsExpanded)
            {
                this.IsExpanded = false;
                this.IsExpanded = true;
            }
        }


        // The below is DONE and just needs more test.
        //private void SetVisibleByPredicate(TreeNodeContainer<T> node)
        //{
        //    // TODO: Work directly on node.Data, so all available data is recursed.
        //    // TODO: This allows the data to e.g. correctly adjust folder sizes based on visible children.
        //    // TODO: But, that means all datanodes are 'pulled' for their children, meaning we probably don't want to do that.
        //    // TODO: Which means the filtering ought to be done outside of the TreeItemsSourceManager.
        //    // TODO: Instead, let the data decide for itself what is visible, based on any criteria. Then,
        //    // TODO: ask the TreeGridItemsSourceManager to update TreeNodeContainer visibility based on a predicate
        //    // TODO: something like this: TreeItemsSourceManager.FilterVisibility((o) => o.IsFilterVisible);
        //    // TODO: BOOM! :)
        //    node.IsVisibleByPredicate = _filterPredicate(node.Data);
        //    foreach (var child in node.Children)
        //    {
        //        SetVisibleByPredicate(child);
        //    }
        //}

        private void FilterNode(TreeNodeContainer<T> node)
        {
            //node.UpdateIsVisible();
            foreach (var child in node.Children)
            {
                child.UpdateIsVisible();
                if (child.IsVisible)
                    FilterNode(child);
            }
        }
        private void FilterNode2_Test_It(TreeNodeContainer<T> node)
        {
            if (node.UpdateIsVisible())
                foreach (var child in node.Children)
                    FilterNode(child);
        }

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
            _itemsSource.CollectionChanged += _itemsSource_CollectionChanged;
            TreeNodeChildren = new ReadOnlyObservableCollection<TreeNodeContainer<T>>(_itemsSource);

            IsTreeRootShown = isTreeRootShown;
            if (IsTreeRootShown == false)
                OnPropertyChanged(nameof(IsTreeRootShown));
            GetChildren = getChildren;
            GetCanHaveChildren = getCanHaveChildren;

            SetFilterPredicate((o) => true);

            this.UpdateIsVisible();
        }

        private void _itemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((TreeNodeContainer<T>)e.NewItems[0])._isInTree = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ((TreeNodeContainer<T>)e.OldItems[0])._isInTree = false;
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
            }
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

                    node.UpdateIsVisible();

                    if (node.IsVisible)
                        // If the newly added node is expanded, it will already have children. Add them to the ItemsSource.
                        if (node.IsExpanded == true)
                            foreach (var child in node.Children)
                                child.UpdateIsVisible();

                    break;
                case NodeAction.Removed:
                    if (this.Parent != null)
                        throw new InvalidOperationException("ERROR9");

                    // The node has already been removed; now we remove any children, by calling UpdateIsVisible when node.Parent is null
                    // TODO: Confirm it is safe to remove children *after* their parent
                    // TODO: Confirm they do in fact go.
                    // TODO: Look for memory leaks.
                    node.UpdateIsVisible();
                    break;

                case NodeAction.IsExpandedChanged:
                    if (node.IsExpanded == true)
                    {
                        if (node._hasMadeChildren == false)
                            node.MakeChildContainers();
                    }
                    else
                    {
                        Debug.Assert(node._hasMadeChildren == true);
                    }
                    foreach (var child in node.Children)
                        child.UpdateIsVisible();

                    break;
                case NodeAction.IsVisibleChanged:
                    node.UpdateShowChevron();

                    if (node.IsVisible == true)
                    {
                        // Will do nothing if node is root node.
                        // Root node is handled separately.
                        Insert(node);
                        node.Parent.VisibleChildrenCount++;
                    }
                    else
                    {
                        node.Parent.VisibleChildrenCount--;
                        _itemsSource.Remove(node);
                    }

                    foreach (var child in node.Children)
                        child.UpdateIsVisible();
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
            //int insertIndex = _itemsSource.IndexOf(item.Parent);
            //insertIndex += GetInsertOffset(item.Parent, item);

            int insertIndex = GetInsertIndex(item);

            _itemsSource.Insert(insertIndex, item);
            return insertIndex;
        }



        private int GetInsertIndex(TreeNodeContainer<T> item)
        {
            var parent = item.Parent;

            var insertIndex = _itemsSource.IndexOf(item.Parent)+1;

            var insertNestLevel = parent.NestLevel + 1;
            // Look for direct children of parent already in _itemsSource;

            while(insertIndex < _itemsSource.Count)
            {
                var candidate = _itemsSource[insertIndex];
                
                if (candidate.NestLevel < insertNestLevel)
                    return insertIndex;

                if(candidate.NestLevel == insertNestLevel)
                    if (_sortComparison?.Invoke(item.Data, candidate.Data) < 0)
                        return insertIndex;

                insertIndex++;
            }
            return insertIndex;
        }

        private int GetInsertOffset(TreeNodeContainer<T> parent, TreeNodeContainer<T> item)
        {
            int offset = 1;

            foreach (var child in parent.Children)
            {
                if (child._isInTree)
                {
                    if ((item != null) && (_sortComparison!=null))
                    {
                        if (_sortComparison?.Invoke(item.Data, child.Data) < 0)
                            return _itemsSource.IndexOf(child)- _itemsSource.IndexOf(parent);
                        if (item.CanHaveChildren)
                            throw new InvalidOperationException();
                    }
                    offset += GetInsertOffset(child, null);// + 1;
                }

            }
            return offset;
        }


        public override bool IsVisible => true;

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