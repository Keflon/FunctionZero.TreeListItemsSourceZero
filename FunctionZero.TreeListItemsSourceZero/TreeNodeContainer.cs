using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FunctionZero.TreeZero;

namespace FunctionZero.TreeListItemsSourceZero
{
    public class TreeNodeContainer<T> : Node<TreeNodeContainer<T>>
    {
        private bool _isExpanded;
        //private bool _isVisible;
        private IEnumerable _dataChildren;
        private bool _showChevron;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }
        //public bool IsVisible
        //{
        //    get => _isVisible;
        //    internal set
        //    {
        //        if (_isVisible != value)
        //        {
        //            _isVisible = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        public virtual bool IsVisible
        {
            get
            {
                if (Parent == null)
                    return false;

                return Parent.IsVisible && Parent.IsExpanded;
            }
        }
        private bool _oldIsVisible;
        public void UpdateIsVisible()
        {
            var newIsVisible = IsVisible;
            if (_oldIsVisible != newIsVisible)
            {
                _oldIsVisible = newIsVisible;
                this.OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool ShowChevron
        {
            get => _showChevron;
            internal set
            {
                if (_showChevron != value)
                {
                    _showChevron = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UpdateShowChevron()
        {
            if (IsVisible == false)
                ShowChevron = false;
            else if (CanHaveChildren == false)
                ShowChevron = false;
            else if (_hasMadeChildren == false)
                ShowChevron = true;
            else if (Children.Count == 0)
                ShowChevron = false;
            else
                ShowChevron = true;

            return ShowChevron;
        }

        public int Indent { get { return NestLevel - (Manager.IsTreeRootShown ? 0 : 1); } }
        public bool CanHaveChildren => Manager.GetCanHaveChildren(this.Data);

        internal bool _hasMadeChildren = false;

        public TreeItemsSourceManager<T> Manager { get; }
        public T Data { get; }
        internal TreeNodeContainer(TreeItemsSourceManager<T> manager, T data)
        {
            Data = data;
            // HACK: Is it???
            if (manager == null)
            {
                Manager = (TreeItemsSourceManager<T>)this;
            }
            else
            {
                Manager = manager;
            }
            _oldIsVisible = IsVisible;
            Children.CollectionChanged += Children_CollectionChanged;
        }

        // TODO: _hasMadeChildren rename to something like _hasChildContainers.
        // TODO: Refactor Children, dataChildren
        // TODO: Refactor TreeGridNode<T>. It's a hierarchy node, or TreeNode. Nothing to do with a grid.
        private void _dataChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (_hasMadeChildren)
                    {
                        if (_observableDataChildren.Count == 1)
                            ShowChevron = true;

                        if (e.NewItems.Count != 1)
                            throw new InvalidOperationException("Attempt to add more than one item to the TreeGridNode data collection at a time.");
                        T newNode = (T)e.NewItems[0];

                        Children.Add(new TreeNodeContainer<T>(Manager, (T)newNode));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (_hasMadeChildren)
                    {
                        if (_observableDataChildren.Count == 0)
                            ShowChevron = false;
                        // TODO: Match node to container and remove the container.
                        // TODO: Use a map.
                        if (e.OldItems.Count != 1)
                            throw new InvalidOperationException("Attempt to remove more than one item from the TreeGridNode data collection at a time.");
                        var oldNode = e.OldItems[0];
                        Children.Remove(ContainerForItem((T)oldNode));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException(e.Action.ToString());
            }
        }

        public TreeNodeContainer<T> ContainerForItem(T dataNode)
        {
            // TODO: Replace this makeshift implementation.
            foreach (TreeNodeContainer<T> item in Children)
            {
                if (EqualityComparer<T>.Default.Equals(item.Data, dataNode))
                    //if (item.Data == dataNode)
                    return item;
            }
            return null;
        }

        internal void MakeChildContainers()
        {
            if (_hasMadeChildren)
                throw new InvalidOperationException("You cannot call MakeChildContainers twice!");
#if THESE_ARE_EQUIVALENT
            foreach (var item in _dataChildren)
            {
                var child = new TreeNodeContainer<T>(Manager, (T)item);
                child.ShowChevron = child.CanHaveChildren;
                Children.Add(child);
            }
#else
            foreach (T item in _dataChildren)
            {
                var child = new TreeNodeContainer<T>(Manager, item);
                child.Parent = this;
            }
#endif
            _hasMadeChildren = true;
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count != 1)
                        throw new InvalidOperationException("Attempt to add more than one item to the TreeGridNode collection at a time.");
                    var newNode = (TreeNodeContainer<T>)e.NewItems[0];
                    Manager.ChangeNode(newNode, NodeAction.Added);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 1)
                        throw new InvalidOperationException("Attempt to add remove than one item from the TreeGridNode collection at a time.");
                    var oldNode = (TreeNodeContainer<T>)e.OldItems[0];
                    Manager.ChangeNode(oldNode, NodeAction.Removed);
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException(e.Action.ToString());
            }
        }

        private ObservableCollection<T> _observableDataChildren;
        internal bool _isInTree;

        protected override async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsExpanded))
            {
                if (_dataChildren == null)
                    GetDataChildren();

                Manager.ChangeNode(this, NodeAction.IsExpandedChanged);
            }
            else if (propertyName == nameof(IsVisible))
            {
                if (_dataChildren == null && IsExpanded == true)
                {
                    if(IsVisible == false)
                    {
                        Debug.WriteLine("ERROR");
                    }
                    GetDataChildren();
                }

                Manager.ChangeNode(this, NodeAction.IsVisibleChanged);
            }

            void GetDataChildren()
            {
                _dataChildren = Manager.GetChildren(Data);

                if (_dataChildren is ObservableCollection<T> observableDataChildren)
                {
                    _observableDataChildren = observableDataChildren;
                    observableDataChildren.CollectionChanged += _dataChildren_CollectionChanged;
                }
            }
        }

        internal void UpdateIndent()
        {
            OnPropertyChanged(nameof(Indent));
        }
    }
}
