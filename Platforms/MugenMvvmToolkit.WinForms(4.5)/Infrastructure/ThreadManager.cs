﻿#region Copyright

// ****************************************************************************
// <copyright file="ThreadManager.cs">
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ThreadManager : IThreadManager
    {
        #region Fields

        private readonly SynchronizationContext _synchronizationContext;
        private int? _mainThreadId;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadManager" /> class.
        /// </summary>
        public ThreadManager([NotNull] SynchronizationContext synchronizationContext)
        {
            Should.NotBeNull(synchronizationContext, "synchronizationContext");
            _synchronizationContext = synchronizationContext;
            synchronizationContext.Post(state => ((ThreadManager)state)._mainThreadId = ManagedThreadId, this);
        }

        #endregion

        #region Properties

        private static int ManagedThreadId
        {
            get
            {
#if XAMARIN_FORMS
                return Environment.CurrentManagedThreadId;
#else
                return Thread.CurrentThread.ManagedThreadId;
#endif
            }
        }

        #endregion

        #region Implementation of IThreadManager

        /// <summary>
        ///     Determines whether the calling thread is the UI thread.
        /// </summary>
        /// <returns><c>true</c> if the calling thread is the UI thread; otherwise, <c>false</c>.</returns>
        public bool IsUiThread
        {
            get
            {
                if (_mainThreadId == null)
                    return SynchronizationContext.Current == _synchronizationContext;
                return _mainThreadId.Value == ManagedThreadId;
            }
        }

        /// <summary>
        ///     Invokes an action on the UI thread synchronous using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (IsUiThread)
                action();
            else
                _synchronizationContext.Send(state => ((Action)state).Invoke(), action);
        }

        /// <summary>
        ///     Invokes an action on the UI thread using the UiDispatcher.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (priority != OperationPriority.Low && IsUiThread)
                action();
            else
                _synchronizationContext.Post(state => ((Action)state).Invoke(), action);
        }

        /// <summary>
        ///     Invokes an action asynchronous.
        /// </summary>
        /// <param name="action">
        ///     The specified <see cref="System.Action" />.
        /// </param>
        /// <param name="priority">The specified <see cref="OperationPriority" /> to invoke the action.</param>
        /// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            Task.Factory.StartNew(action, cancellationToken);
        }

        #endregion
    }
}