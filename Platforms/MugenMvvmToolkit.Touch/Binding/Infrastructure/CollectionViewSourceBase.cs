#region Copyright

// ****************************************************************************
// <copyright file="CollectionViewSourceBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using UIKit;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class CollectionViewSourceBase : UICollectionViewSource
    {
        #region Fields

        internal const int InitializingStateMask = 1;
        private const int InitializedStateMask = 2;
        private static Func<UICollectionView, IDataContext, CollectionViewSourceBase> _factory;

        private readonly WeakReference _collectionView;
        private readonly IBindingMemberInfo _itemTemplateMember;

        private UICollectionViewCell _lastCreatedCell;
        private NSIndexPath _lastCreatedCellPath;
        private object _selectedItem;

        #endregion

        #region Constructors

        static CollectionViewSourceBase()
        {
            _factory = (o, context) => new ItemsSourceCollectionViewSource(o);
        }

        protected CollectionViewSourceBase(IntPtr handle)
            : base(handle)
        {
        }

        protected CollectionViewSourceBase([NotNull] UICollectionView collectionView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(collectionView, "collectionView");
            _collectionView = PlatformExtensions.CreateWeakReference(collectionView, true);
            _itemTemplateMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(collectionView.GetType(), itemTemplate, false, false);
            var controllerView = collectionView.FindParent<IViewControllerView>();
            if (controllerView != null && !(controllerView is IMvvmNavigationController))
                controllerView.Mediator.DisposeHandler += ControllerOnDispose;

            UseAnimations = PlatformDataBindingModule
                .CollectionViewUseAnimationsMember
                .GetValue(collectionView, null)
                .GetValueOrDefault(true);
            ScrollPosition = PlatformDataBindingModule
                .CollectionViewScrollPositionMember
                .GetValue(collectionView, null)
                .GetValueOrDefault(UICollectionViewScrollPosition.Top);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the factory that allows to create items source adapter.
        /// </summary>
        [NotNull]
        public static Func<UICollectionView, IDataContext, CollectionViewSourceBase> Factory
        {
            get { return _factory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _factory = value;
            }
        }

        public bool UseAnimations { get; set; }

        public UICollectionViewScrollPosition ScrollPosition { get; set; }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                var collectionView = CollectionView;
                if (collectionView != null)
                    SetSelectedItem(collectionView, value, true);
            }
        }

        [CanBeNull]
        protected UICollectionView CollectionView
        {
            get { return (UICollectionView)_collectionView.Target; }
        }

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            var collectionView = CollectionView;
            if (collectionView != null)
                collectionView.ReloadData();
        }

        public virtual bool UpdateSelectedBindValue(UICollectionViewCell cell, bool selected)
        {
            var collectionView = CollectionView;
            if (collectionView == null || collectionView.AllowsMultipleSelection)
                return selected;

            if (!collectionView.AllowsSelection)
                return false;
            if (HasMask(cell, InitializingStateMask))
            {
                if (Equals(ViewManager.GetDataContext(cell), SelectedItem))
                    return true;
                return selected && SelectedItem == null;
            }
            return selected;
        }

        public virtual void OnCellSelectionChanged(UICollectionViewCell cell, bool selected, bool setFromBinding)
        {
            var collectionView = CollectionView;
            if (!setFromBinding || collectionView == null)
                return;
            UpdateSelectedItemInternal(collectionView, ViewManager.GetDataContext(cell), selected);
            var path = IndexPathForCell(collectionView, cell);
            if (path == null)
                return;
            if (selected)
                collectionView.SelectItem(path, false, UICollectionViewScrollPosition.None);
            else
                collectionView.DeselectItem(path, false);
        }

        protected abstract object GetItemAt(NSIndexPath indexPath);

        protected abstract void SetSelectedCellByItem(UICollectionView collectionView, object selectedItem);

        protected virtual void ControllerOnDispose(object sender, EventArgs eventArgs)
        {
            ((IViewControllerView)sender).Mediator.DisposeHandler -= ControllerOnDispose;
            Dispose();
        }

        protected void ClearSelection(UICollectionView collectionView)
        {
            var indexPaths = collectionView.GetIndexPathsForSelectedItems();
            if (indexPaths != null)
            {
                foreach (NSIndexPath indexPath in indexPaths)
                    collectionView.DeselectItem(indexPath, UseAnimations);
            }
            SetSelectedItem(collectionView, null, false);
        }

        internal static bool HasMask(UICollectionViewCell cell, int mask)
        {
            return (cell.Tag & mask) == mask;
        }

        internal UICollectionViewCell CellForItem(UICollectionView view, NSIndexPath path)
        {
            if (path == null)
                return null;
            UICollectionViewCell cell = view.CellForItem(path);
            if (cell != null)
                return cell;
            if (_lastCreatedCellPath != null && path.Equals(_lastCreatedCellPath))
                return _lastCreatedCell;
            return null;
        }

        internal NSIndexPath IndexPathForCell(UICollectionView collectionView, UICollectionViewCell cell)
        {
            if (ReferenceEquals(cell, _lastCreatedCell))
                return _lastCreatedCellPath;
            return collectionView.IndexPathForCell(cell);
        }

        private void SetSelectedItem(UICollectionView collectionView, object value, bool sourceUpdate)
        {
            if (Equals(_selectedItem, value))
                return;
            _selectedItem = value;
            if (sourceUpdate)
                SetSelectedCellByItem(collectionView, value);
            PlatformDataBindingModule.CollectionViewSelectedItemChangedEvent.Raise(collectionView, EventArgs.Empty);
        }

        private void UpdateSelectedItemInternal(UICollectionView collectionView, object item, bool selected)
        {
            if (selected)
            {
                if (!collectionView.AllowsMultipleSelection || SelectedItem == null)
                    SetSelectedItem(collectionView, item, false);
            }
            else if (Equals(item, SelectedItem))
                SetSelectedItem(collectionView, null, false);
        }

        #endregion

        #region Overrides of UICollectionViewSource

        public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = null;
            var callback = cell as IHasDisplayCallback;
            if (callback != null)
                callback.DisplayingEnded();
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var selector = _itemTemplateMember.TryGetValue<ICollectionCellTemplateSelector>(collectionView);
            if (selector == null)
                throw new NotSupportedException("The ItemTemplate is null to create UICollectionViewCell use the ItemTemplate with ICollectionCellTemplateSelector value.");
            object item = GetItemAt(indexPath);
            NSString identifier = selector.GetIdentifier(item, collectionView);
            var cell = (UICollectionViewCell)collectionView.DequeueReusableCell(identifier, indexPath);

            _lastCreatedCell = cell;
            _lastCreatedCellPath = indexPath;


            if (Equals(item, _selectedItem) && !cell.Selected)
                collectionView.SelectItem(indexPath, false, UICollectionViewScrollPosition.None);
            cell.Tag |= InitializingStateMask;
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = item;
            if (!HasMask(cell, InitializedStateMask))
            {
                cell.Tag |= InitializedStateMask;
                ParentObserver.GetOrAdd(cell).Parent = collectionView;
                selector.InitializeTemplate(collectionView, cell);
            }
            cell.Tag &= ~InitializingStateMask;
            var initializableItem = cell as IHasDisplayCallback;
            if (initializableItem != null)
                initializableItem.WillDisplay();
            return cell;
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(collectionView, GetItemAt(indexPath), false);
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(collectionView, GetItemAt(indexPath), true);
        }

        public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UICollectionViewCell cell = CellForItem(collectionView, indexPath);
            if (cell != null)
                PlatformDataBindingModule.CollectionViewCellHighlightedMember.Raise(cell, EventArgs.Empty);
        }

        public override void ItemUnhighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UICollectionViewCell cell = CellForItem(collectionView, indexPath);
            if (cell != null)
                PlatformDataBindingModule.CollectionViewCellHighlightedMember.Raise(cell, EventArgs.Empty);
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override bool ShouldDeselectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldDeselectMember
                .TryGetValue(CellForItem(collectionView, indexPath))
                .GetValueOrDefault(true);
        }

        public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldHighlightMember
                .TryGetValue(CellForItem(collectionView, indexPath))
                .GetValueOrDefault(true);
        }

        public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldSelectMember
                .TryGetValue(CellForItem(collectionView, indexPath))
                .GetValueOrDefault(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lastCreatedCell = null;
                _lastCreatedCellPath = null;
                _selectedItem = null;

                var collectionView = CollectionView;
                if (collectionView.IsAlive())
                {
                    if (ReferenceEquals(collectionView.Source, this))
                        collectionView.Source = null;
                    var controllerView = collectionView.FindParent<IViewControllerView>();
                    if (controllerView != null)
                        controllerView.Mediator.DisposeHandler -= ControllerOnDispose;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}