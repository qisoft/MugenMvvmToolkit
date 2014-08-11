﻿#region Copyright
// ****************************************************************************
// <copyright file="DataBinding.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Binding.Core
{
    /// <summary>
    ///     Provides high-level access to the definition of a binding, which connects the properties of binding target objects
    ///     and any data source
    /// </summary>
    public class DataBinding : IDataBinding, IDataContext
    {
        #region Nested types

        [DebuggerDisplay("Count = {Count}")]
        private sealed class BehaviorCollection : ICollection<IBindingBehavior>
        {
            #region Fields

            private IBindingBehavior[] _items;
            private int _size;

            #endregion

            #region Fields

            private readonly DataBinding _dataBinding;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="BehaviorCollection" /> class that is empty.
            /// </summary>
            public BehaviorCollection(DataBinding dataBinding)
            {
                _items = EmptyValue<IBindingBehavior>.ArrayInstance;
                _dataBinding = dataBinding;
            }

            #endregion

            #region Properties

            private int Capacity
            {
                set
                {
                    if (value < _size)
                        throw ExceptionManager.CapacityLessThanCollection("Capacity");
                    if (value == _items.Length)
                        return;
                    if (value > 0)
                    {
                        var objArray = new IBindingBehavior[value];
                        if (_size > 0)
                            Array.Copy(_items, 0, objArray, 0, _size);
                        _items = objArray;
                    }
                    else
                        _items = EmptyValue<IBindingBehavior>.ArrayInstance;
                }
            }

            #endregion

            #region Implementation of IEnumerable

            public IEnumerator<IBindingBehavior> GetEnumerator()
            {
                return _items
                    .OfType<IBindingBehavior>()
                    .Take(_size)
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IBindingBehavior item)
            {
                CheckBehavior(item);
                if (!item.Attach(_dataBinding))
                    return;
                if (_size == _items.Length)
                    EnsureCapacity(_size + 1);
                _items[_size++] = item;
                _dataBinding.OnBehaviorAdded(item);
            }

            public void Clear()
            {
                while (_size != 0)
                    Remove(_items[0]);
            }

            public bool Contains(IBindingBehavior item)
            {
                Should.NotBeNull(item, "item");
                return IndexOf(item) >= 0;
            }

            public void CopyTo(IBindingBehavior[] array, int arrayIndex)
            {
                Array.Copy(_items, 0, array, arrayIndex, _size);
            }

            public bool Remove(IBindingBehavior item)
            {
                Should.NotBeNull(item, "item");
                int index = IndexOf(item);
                if (index < 0)
                    return false;
                IBindingBehavior behavior = _items[index];
                --_size;
                if (index < _size)
                    Array.Copy(_items, index + 1, _items, index, _size - index);
                _items[_size] = null;
                behavior.Detach(_dataBinding);
                _dataBinding.OnBehaviorRemoved(behavior);
                return true;
            }

            public int Count
            {
                get { return _size; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            #endregion

            #region Methods

            private void CheckBehavior(IBindingBehavior newBehavior)
            {
                Should.NotBeNull(newBehavior, "newBehavior");
                if (_size == 0)
                    return;
                for (int index = 0; index < _size; index++)
                {
                    if (_items[index].Id == newBehavior.Id)
                        throw BindingExceptionManager.DuplicateBehavior(_items[index], newBehavior);
                }
            }

            private void EnsureCapacity(int min)
            {
                if (_items.Length < min)
                    Capacity = _items.Length == 0 ? 2 : _items.Length * 2;
            }

            private int IndexOf(IBindingBehavior item)
            {
                return Array.IndexOf(_items, item, 0, _size);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly BehaviorCollection _behaviors;
        private readonly IBindingSourceAccessor _sourceAccessor;
        private readonly ISingleBindingSourceAccessor _targetAccessor;
        private bool _isDisposed;
        private IDataContext _lazyContext;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataBinding" /> class.
        /// </summary>
        public DataBinding([NotNull] ISingleBindingSourceAccessor target, [NotNull] IBindingSourceAccessor source)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(source, "source");
            _targetAccessor = target;
            _sourceAccessor = source;
            _behaviors = new BehaviorCollection(this);
        }

        #endregion

        #region Properties

        internal bool IsAssociated { get; set; }

        #endregion

        #region Implementation of IBinding

        /// <summary>
        ///     Gets the current <see cref="IDataContext" />.
        /// </summary>
        public IDataContext Context
        {
            get { return this; }
        }

        /// <summary>
        ///     Gets the binding target accessor.
        /// </summary>
        public ISingleBindingSourceAccessor TargetAccessor
        {
            get { return _targetAccessor; }
        }

        /// <summary>
        ///     Gets the binding source accessor.
        /// </summary>
        public IBindingSourceAccessor SourceAccessor
        {
            get { return _sourceAccessor; }
        }

        /// <summary>
        ///     Gets the binding behaviors.
        /// </summary>
        public ICollection<IBindingBehavior> Behaviors
        {
            get { return _behaviors; }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        ///     Sends the current value back to the source.
        /// </summary>
        public virtual void UpdateSource()
        {
            try
            {
                if (_sourceAccessor.SetValue(_targetAccessor, this, true))
                    RaiseBindingUpdated(BindingAction.UpdateSource);
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, BindingAction.UpdateSource, exception),
                    exception, BindingAction.UpdateSource);
            }
        }

        /// <summary>
        ///     Forces a data transfer from source to target.
        /// </summary>
        public virtual void UpdateTarget()
        {
            try
            {
                if (_targetAccessor.SetValue(_sourceAccessor, this, true))
                    RaiseBindingUpdated(BindingAction.UpdateTarget);
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, BindingAction.UpdateTarget, exception), exception,
                    BindingAction.UpdateTarget);
            }
        }

        /// <summary>
        ///     Validates the current binding and raises the BindingException event if needed.
        /// </summary>
        public virtual bool Validate()
        {
            var action = BindingAction.UpdateTarget;
            try
            {
                bool isValid = _targetAccessor.Source.Validate(true);
                action = BindingAction.UpdateSource;

                var singleSourceAccessor = _sourceAccessor as ISingleBindingSourceAccessor;
                if (singleSourceAccessor != null)
                {
                    if (isValid && !singleSourceAccessor.Source.Validate(true))
                        isValid = false;
                }
                else
                {
                    for (int index = 0; index < _sourceAccessor.Sources.Count; index++)
                    {
                        if (isValid && !_sourceAccessor.Sources[index].Validate(true))
                            isValid = false;
                    }
                }
                return isValid;
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, action, exception), exception,
                    BindingAction.UpdateTarget);
                return false;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            lock (_behaviors)
            {
                if (_isDisposed)
                    return;
                try
                {
                    OnDispose();
                    var disposed = Disposed;
                    if (disposed != null)
                    {
                        disposed(this, EventArgs.Empty);
                        Disposed = null;
                    }
                }
                finally
                {
                    _isDisposed = true;
                }
            }
        }

        /// <summary>
        ///     Occurs when the binding updates the values.
        /// </summary>
        public event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;

        /// <summary>
        ///     Occurs when an exception is not caught.
        /// </summary>
        public event EventHandler<IDataBinding, BindingExceptionEventArgs> BindingException;

        /// <summary>
        ///     Occurs when the object is disposed by a call to the Dispose method.
        /// </summary>
        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when behavior added.
        /// </summary>
        protected virtual void OnBehaviorAdded([NotNull] IBindingBehavior behavior)
        {
        }

        /// <summary>
        ///     Occurs when behavior removed.
        /// </summary>
        protected virtual void OnBehaviorRemoved([NotNull] IBindingBehavior behavior)
        {
        }


        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected virtual void OnDispose()
        {
            BindingUpdated = null;
            BindingException = null;
            _behaviors.Clear();
            _sourceAccessor.Dispose();
            _targetAccessor.Dispose();
        }

        /// <summary>
        ///     Raises the <see cref="BindingException" /> event.
        /// </summary>
        protected void RaiseBindingException(Exception exception, Exception originalException, BindingAction action)
        {
            Tracer.Error(exception.Message);
            var handler = BindingException;
            if (handler != null) handler(this, new BindingExceptionEventArgs(action, exception, originalException));
        }

        /// <summary>
        ///     Raises the <see cref="BindingUpdated" /> event.
        /// </summary>
        protected void RaiseBindingUpdated(BindingAction action)
        {
            var handler = BindingUpdated;
            if (handler != null) handler(this, new BindingEventArgs(action));
        }

        #endregion

        #region Implementation of IDataContext

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="IDataContext" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="IDataContext" />.
        /// </returns>
        int IDataContext.Count
        {
            get
            {
                if (_lazyContext == null)
                    return 1;
                return _lazyContext.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IDataContext" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="IDataContext" /> is read-only; otherwise, false.
        /// </returns>
        bool IDataContext.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        void IDataContext.Add<T>(DataConstant<T> dataConstant, T value)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    _lazyContext = new DataContext
                    {
                        {BindingConstants.Binding, this}
                    };
            }
            _lazyContext.Add(dataConstant, value);
        }

        /// <summary>
        ///     Adds the data constant value or update existing.
        /// </summary>
        void IDataContext.AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    _lazyContext = new DataContext
                    {
                        {BindingConstants.Binding, this}
                    };
            }
            _lazyContext.AddOrUpdate(dataConstant, value);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        T IDataContext.GetData<T>(DataConstant<T> dataConstant)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                {
                    if (BindingConstants.Binding.Equals(dataConstant))
                        return (T)(object)this;
                    return default(T);
                }
            }
            return _lazyContext.GetData(dataConstant);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        bool IDataContext.TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                {
                    if (BindingConstants.Binding.Equals(dataConstant))
                    {
                        data = (T)(object)this;
                        return true;
                    }
                    data = default(T);
                    return false;
                }
            }
            return _lazyContext.TryGetData(dataConstant, out data);
        }

        /// <summary>
        ///     Determines whether the <see cref="IDataContext" /> contains the specified key.
        /// </summary>
        bool IDataContext.Contains(DataConstant dataConstant)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    return BindingConstants.Binding.Constant.Equals(dataConstant);
            }
            return _lazyContext.Contains(dataConstant);
        }

        /// <summary>
        ///     Removes the data constant value.
        /// </summary>
        bool IDataContext.Remove(DataConstant dataConstant)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    return false;
            }
            return _lazyContext.Remove(dataConstant);
        }

        /// <summary>
        ///     Updates the current context.
        /// </summary>
        void IDataContext.Update(IDataContext context)
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    _lazyContext = new DataContext
                    {
                        {BindingConstants.Binding, this}
                    };
            }
            _lazyContext.Update(context);
        }

        /// <summary>
        /// Removes all values from current context.
        /// </summary>
        void IDataContext.Clear()
        {
            lock (_behaviors)
            {
                if (_lazyContext != null)
                    _lazyContext.Clear();
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="IList{DataConstantValue}" /> from current context.
        /// </summary>
        IList<DataConstantValue> IDataContext.ToList()
        {
            lock (_behaviors)
            {
                if (_lazyContext == null)
                    return new List<DataConstantValue> { BindingConstants.Binding.ToValue(this) };
            }
            return _lazyContext.ToList();
        }

        #endregion
    }
}