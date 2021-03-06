﻿#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Navigation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#endif

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the application state manager.
    /// </summary>
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Nested types

        [DataContract]
        internal protected sealed class LazySerializableContainer
        {
            #region Fields

            [IgnoreDataMember]
            private readonly ISerializer _serializer;

            [IgnoreDataMember]
            private byte[] _bytes;
            [IgnoreDataMember]
            private IDataContext _context;

            #endregion

            #region Constructors

            public LazySerializableContainer(ISerializer serializer, IDataContext context, IViewModel viewModel)
            {
                _serializer = serializer;
                _context = context;
            }

            #endregion

            #region Properties

            [DataMember]
            internal byte[] Bytes
            {
                get
                {
                    if (_bytes == null && _context.Count != 0)
                        _bytes = _serializer.Serialize(_context).ToArray();
                    return _bytes;
                }
                set { _bytes = value; }
            }

            #endregion

            #region Methods

            public IDataContext GetContext(ISerializer serializer)
            {
                if (_context == null)
                {
                    if (_bytes == null)
                        return DataContext.Empty;
                    using (var ms = new MemoryStream(_bytes))
                        _context = (IDataContext)serializer.Deserialize(ms);
                }
                return _context;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string VmStateKey = "@`vmstate";
        private const string VmTypeKey = "@`vmtype";

        private static readonly Type[] KnownTypesStatic;

        private readonly ISerializer _serializer;
        private readonly IViewManager _viewManager;
        private readonly IViewModelPresenter _viewModelPresenter;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static ApplicationStateManager()
        {
            KnownTypesStatic = new[] { typeof(LazySerializableContainer) };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateManager" /> class.
        /// </summary>
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, "serializer");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(viewModelPresenter, "viewModelPresenter");
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="ISerializer" />.
        /// </summary>
        protected ISerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" />.
        /// </summary>
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" />.
        /// </summary>
        protected IViewModelPresenter ViewModelPresenter
        {
            get { return _viewModelPresenter; }
        }

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        ///     Gets the collection of known types.
        /// </summary>
        public virtual IList<Type> KnownTypes
        {
            get { return KnownTypesStatic; }
        }

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        public void OnSaveState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            var viewModel = element.DataContext as IViewModel;
            if (viewModel != null)
            {
                state[VmTypeKey] = viewModel.GetType().AssemblyQualifiedName;
                PreserveViewModel(viewModel, element, state, args, context ?? DataContext.Empty);
            }
        }

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        public void OnLoadState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            object value;
            if (!state.TryGetValue(VmTypeKey, out value))
                return;
            state.Remove(VmTypeKey);
            object dataContext = element.DataContext;
            Type vmType = Type.GetType(value as string, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return;

            if (context == null)
                context = DataContext.Empty;
            var viewModelState = RestoreViewModelState(element, state, args, context);
            //The navigation is already handled.
            var eventArgs = args as NavigationEventArgs;
            if (eventArgs != null && eventArgs.GetHandled())
            {
                eventArgs.SetHandled(false);
                PlatformExtensions.SetViewModelState(eventArgs.Content, viewModelState);
            }
            else
                RestoreViewModel(vmType, viewModelState, element, state, args, context);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Restores the view model state.
        /// </summary>
        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state,
             [NotNull] object args, [NotNull] IDataContext context)
        {
            object value;
            if (state.TryGetValue(VmStateKey, out value))
                return ((LazySerializableContainer)value).GetContext(_serializer);
            return DataContext.Empty;
        }

        /// <summary>
        ///     Restores the view model.
        /// </summary>
        protected virtual void RestoreViewModel([NotNull] Type viewModelType, [NotNull] IDataContext viewModelState, [NotNull] FrameworkElement element,
            [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            context = context.ToNonReadOnly();
            context.AddOrUpdate(InitializationConstants.ViewModelType, viewModelType);

#if WINDOWSCOMMON
            context.Add(DynamicViewModelWindowPresenter.RestoredViewConstant, element);
            context.Add(DynamicViewModelWindowPresenter.IsOpenViewConstant, true);
#endif
            IViewModel viewModel = _viewModelProvider.RestoreViewModel(viewModelState, context, false);
            _viewManager.InitializeViewAsync(viewModel, element, context).WithTaskExceptionHandler(this);
            _viewModelPresenter.Restore(viewModel, context);
        }

        /// <summary>
        ///     Preserves the view model.
        /// </summary>
        protected virtual void PreserveViewModel([NotNull] IViewModel viewModel, [NotNull] FrameworkElement element,
             [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            state[VmStateKey] = new LazySerializableContainer(_serializer, _viewModelProvider.PreserveViewModel(viewModel, context), viewModel);
        }

        #endregion
    }
}