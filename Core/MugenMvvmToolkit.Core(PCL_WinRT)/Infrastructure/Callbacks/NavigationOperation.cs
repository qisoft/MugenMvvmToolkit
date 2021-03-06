﻿using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    /// <summary>
    ///     Represents the navigation operation.
    /// </summary>
    public class NavigationOperation : AsyncOperation<bool>, INavigationOperation
    {
        #region Fields

        private readonly Task _task;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationOperation" /> class.
        /// </summary>
        public NavigationOperation()
            : this(Empty.Task)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationOperation" /> class.
        /// </summary>
        public NavigationOperation(Task task)
        {
            Should.NotBeNull(task, "task");
            _task = task;
        }

        #endregion

        #region Implementation of INavigationOperation

        /// <summary>
        ///     Gets the navigation task, this task will be completed when navigation will be completed.
        /// </summary>
        public Task NavigationCompletedTask
        {
            get { return _task; }
        }

        #endregion
    }
}