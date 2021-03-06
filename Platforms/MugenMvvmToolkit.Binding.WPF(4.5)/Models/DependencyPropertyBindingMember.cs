﻿#region Copyright

// ****************************************************************************
// <copyright file="DependencyPropertyBindingMember.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
#if NETFX_CORE || WINDOWSCOMMON
using BindingEx = Windows.UI.Xaml.Data.Binding;
#else
using BindingEx = System.Windows.Data.Binding;
#endif


namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the binding member for a dependency property.
    /// </summary>
    public sealed class DependencyPropertyBindingMember : IBindingMemberInfo
    {
        #region Nested types

        //BUG: WP doesn't update DataContextProperty on change in parent.
#if WINDOWS_PHONE
        private static class DataContextChangedHelper
        {
        #region Fields

            public static readonly DependencyProperty InternalDataContextProperty = DependencyProperty.RegisterAttached(
                "InternalDataContext", typeof(object), typeof(DataContextChangedHelper),
                new PropertyMetadata(null, OnDataContextChanged));

            public static readonly DependencyProperty DataContextListenerProperty = DependencyProperty.RegisterAttached(
                "DataContextListener", typeof(EventListenerList), typeof(DataContextChangedHelper), new PropertyMetadata(default(EventListenerList)));

        #endregion

        #region Methods

            private static void OnDataContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                var value = (EventListenerList)sender.GetValue(DataContextListenerProperty);
                if (value != null)
                    value.Raise(sender, e);
            }

            public static IDisposable Listen(FrameworkElement control, IEventListener listener)
            {
                var value = (EventListenerList)control.GetValue(DataContextListenerProperty);
                if (value == null)
                {
                    value = new EventListenerList();
                    control.SetValue(DataContextListenerProperty, value);
                    control.SetBinding(InternalDataContextProperty, new BindingEx
                    {
                        ValidatesOnDataErrors = false,
                        NotifyOnValidationError = false,
                        ValidatesOnNotifyDataErrors = false,
                        ValidatesOnExceptions = false
                    });
                }
                return value.AddWithUnsubscriber(listener);
            }

        #endregion
        }
#endif

        public sealed class DependencyPropertyListener : DependencyObject, IDisposable
        {
            #region Fields

            public static readonly DependencyProperty ValueProperty = DependencyProperty
                .Register("Value", typeof(object), typeof(DependencyPropertyListener), new PropertyMetadata(null, OnValueChanged));

#if !NETFX_CORE && !WINDOWSCOMMON
            private static readonly Action<DependencyPropertyListener> DisposeDelegate = DisposeInternal;
#endif
            private WeakEventListenerWrapper _listener;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="DependencyPropertyListener" /> class.
            /// </summary>
            public DependencyPropertyListener(object source, string propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new BindingEx
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
#if !NETFX_CORE
#if !WINDOWS_PHONE
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#endif

#if !WINDOWSCOMMON
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,

#endif

#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif

#endif
                    });
            }
#if !NETFX_CORE && !WINDOWSCOMMON && !WINDOWS_PHONE
            /// <summary>
            ///     Initializes a new instance of the <see cref="DependencyPropertyListener" /> class.
            /// </summary>
            public DependencyPropertyListener(object source, DependencyProperty propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new BindingEx
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,
#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif
                    });
            }
#endif

            #endregion

            #region Properties

            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            #endregion

            #region Methods

            private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                var listener = (DependencyPropertyListener)sender;
                if (!listener._listener.EventListener.TryHandle(sender, EventArgs.Empty))
                    DisposeInternal(listener);
            }

            private static void DisposeInternal(DependencyPropertyListener listener)
            {
                try
                {
                    if (listener._listener.IsEmpty)
                        return;
                    listener.ClearValue(ValueProperty);
                    listener._listener = WeakEventListenerWrapper.Empty;
                }
                catch (InvalidOperationException e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }

            #endregion

            #region Implementation of IDisposable

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
#if NETFX_CORE || WINDOWSCOMMON
                if (Dispatcher.HasThreadAccess)
                    DisposeInternal(this);
                else
                    Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => DisposeInternal(this));
#else
                if (Dispatcher.CheckAccess())
                    DisposeInternal(this);
                else
                    Dispatcher.BeginInvoke(DisposeDelegate, this);
#endif
            }

            #endregion
        }

        #endregion

        #region Fields

        internal static readonly Func<object, bool> IsNamedObjectFunc;

        private readonly bool _canWrite;
        private readonly IBindingMemberInfo _changePropertyMember;
        private readonly DependencyProperty _dependencyProperty;
        private readonly MemberInfo _member;
        private readonly string _path;
        private readonly Type _type;

        #endregion

        #region Constructors

        static DependencyPropertyBindingMember()
        {
            try
            {
                var type = DependencyProperty.UnsetValue.GetType();
                var methodInfo = typeof(DependencyPropertyBindingMember).GetMethodEx("Is", MemberFlags.Static | MemberFlags.Public);
                if (methodInfo == null || !type.FullName.Equals("MS.Internal.NamedObject", StringComparison.Ordinal))
                    IsNamedObjectFunc = o => false;
                else
                    IsNamedObjectFunc = (Func<object, bool>)ServiceProvider.ReflectionManager.TryCreateDelegate(typeof(Func<object, bool>), null, methodInfo.MakeGenericMethod(type));
            }
            catch (Exception e)
            {
                IsNamedObjectFunc = o => false;
                Tracer.Error(e.Flatten(true));
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DependencyPropertyBindingMember" /> class.
        /// </summary>
        public DependencyPropertyBindingMember([NotNull] DependencyProperty dependencyProperty, [NotNull] string path,
            [NotNull] Type type, bool readOnly, [CanBeNull] MemberInfo member, [CanBeNull] IBindingMemberInfo changePropertyMember)
        {
            Should.NotBeNull(dependencyProperty, "dependencyProperty");
            Should.NotBeNullOrEmpty(path, "path");
            Should.NotBeNull(type, "type");
            _dependencyProperty = dependencyProperty;
            _path = path;
#if WPF
            _type = dependencyProperty.PropertyType;
            _canWrite = !dependencyProperty.ReadOnly;
#else
            _type = type;
            _canWrite = !readOnly;
#endif
            _member = member;
            _changePropertyMember = changePropertyMember;
        }

        #endregion

        #region Methods

        public static bool Is<T>(object item)
        {
            return item is T;
        }

        #endregion

        #region Implementation of IBindingMemberInfo

        /// <summary>
        ///     Gets the path of member.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Gets the type of member.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the underlying member, if any.
        /// </summary>
        public MemberInfo Member
        {
            get { return _member; }
        }

        /// <summary>
        ///     Gets the member type.
        /// </summary>
        public BindingMemberType MemberType
        {
            get { return BindingMemberType.DependencyProperty; }
        }

        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        public bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        public bool CanWrite
        {
            get { return _canWrite; }
        }

        /// <summary>
        ///     Returns the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be returned.</param>
        /// <param name="args">Optional values for methods. This value should be null for method members.</param>
        /// <returns>The member value of the specified object.</returns>
        public object GetValue(object source, object[] args)
        {
            object value = ((DependencyObject)source).GetValue(_dependencyProperty);
            if (value != null && (ReferenceEquals(value, DependencyProperty.UnsetValue) || IsNamedObjectFunc(value)))
                return BindingConstants.UnsetValue;
            return value;
        }

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="args">Optional values for methods. This value should be null for method members.</param>
        public object SetValue(object source, object[] args)
        {
            object value = args[0];
            if (ReferenceEquals(value, BindingConstants.UnsetValue))
                value = DependencyProperty.UnsetValue;
            ((DependencyObject)source).SetValue(_dependencyProperty, value);
            return null;
        }

        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        public IDisposable TryObserve(object source, IEventListener listener)
        {
#if WINDOWS_PHONE
            var frameworkElement = source as FrameworkElement;
            if (frameworkElement != null && _path == AttachedMemberConstants.DataContext)
                return DataContextChangedHelper.Listen(frameworkElement, listener);
#endif
            if (_changePropertyMember == null)
#if NETFX_CORE || WINDOWSCOMMON || WINDOWS_PHONE
                return new DependencyPropertyListener(source, _path, listener);
#else
                return new DependencyPropertyListener(source, _dependencyProperty, listener);
#endif
            return _changePropertyMember.SetValue(source, new object[] { listener }) as IDisposable;
        }

        #endregion
    }
}