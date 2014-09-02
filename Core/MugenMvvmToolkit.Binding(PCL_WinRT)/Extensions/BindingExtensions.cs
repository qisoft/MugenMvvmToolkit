﻿#region Copyright
// ****************************************************************************
// <copyright file="BindingExtensions.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the binding extensions.
    /// </summary>
    public static class BindingExtensions
    {
        #region Nested types

        private sealed class BindingContextWrapper : IBindingContext, IEventListener
        {
            #region Fields

            private readonly IBindingContext _innerContext;
            private IBindingContext _parentContext;
            //NOTE to keep observer reference.
            // ReSharper disable once NotAccessedField.Local
            private IDisposable _parentListener;

            #endregion

            #region Constructors

            public BindingContextWrapper(object target)
            {
                var parentMember = BindingServiceProvider.VisualTreeManager.GetParentMember(target.GetType());
                if (parentMember != null)
                    _parentListener = parentMember.TryObserve(target, this);
                Update(target);
                _innerContext = BindingServiceProvider.ContextManager.GetBindingContext(target);
            }

            #endregion

            #region Implementation of interfaces

            public object Source
            {
                get { return _innerContext; }
            }

            public object Value
            {
                get
                {
                    if (_parentContext == null)
                        return null;
                    return _parentContext.Value;
                }
                set { _innerContext.Value = value; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public void Handle(object sender, object message)
            {
                if (!(sender is IBindingContext) && _innerContext != null)
                {
                    lock (this)
                        Update(_innerContext.Source);
                }
                RaiseValueChanged();
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private void Update(object source)
            {
                if (_parentContext != null)
                {
                    var src = _parentContext.Source;
                    if (src != null)
                        WeakEventManager.GetBindingContextListener(src).Remove(this);
                }
                if (source == null)
                    _parentContext = null;
                else
                {
                    _parentContext = GetParentBindingContext(source);
                    if (_parentContext != null)
                    {
                        var src = _parentContext.Source;
                        if (src != null)
                            WeakEventManager.GetBindingContextListener(src).Add(this);
                    }
                }
            }

            private static IBindingContext GetParentBindingContext(object target)
            {
                object parent = BindingServiceProvider.VisualTreeManager.FindParent(target);
                if (parent == null)
                    return null;
                return BindingServiceProvider.ContextManager.GetBindingContext(parent);
            }

            private void RaiseValueChanged()
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class WeakEventListenerWrapper : IEventListener
        {
            #region Fields

            private WeakReference _listenerRef;

            #endregion

            #region Constructors

            public WeakEventListenerWrapper(IEventListener listener)
            {
                _listenerRef = ServiceProvider.WeakReferenceFactory(listener, true);
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsWeak
            {
                get { return true; }
            }

            public void Handle(object sender, object message)
            {
                var reference = _listenerRef;
                if (reference == null)
                    return;
                var listener = (IEventListener)reference.Target;
                if (listener == null)
                    _listenerRef = null;
                else
                    listener.Handle(sender, message);
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets the array with single null value.
        /// </summary>
        public static readonly object[] NullValue;

        /// <summary>
        /// Gets the attached parent member.
        /// </summary>
        public readonly static IAttachedBindingMemberInfo<object, object> AttachedParentMember;

        private static readonly Func<string, string, string> MergePathDelegate;

        #endregion

        #region Constructors

        static BindingExtensions()
        {
            AttachedParentMember = AttachedBindingMember.CreateAutoProperty<object, object>("#" + AttachedMemberConstants.Parent);
            NullValue = new object[] { null };
            MergePathDelegate = MergePath;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to subscribe to the event.        
        /// </summary>
        public static IDisposable TrySubscribe([NotNull] this IWeakEventManager eventManager, [NotNull] object target, string eventName, IEventListener listener,
            IDataContext context = null)
        {
            Should.NotBeNull(eventManager, "eventManager");
            Should.NotBeNull(target, "target");
            var @event = target.GetType().GetEventEx(eventName, MemberFlags.Instance | MemberFlags.Public);
            if (@event == null)
                throw BindingExceptionManager.MissingEvent(target, eventName);
            return eventManager.TrySubscribe(target, @event, listener, context);
        }

        /// <summary>
        /// Converts the specified <see cref="IEventListener"/> to a weak <see cref="IEventListener"/> if listener is not weak.
        /// </summary>
        public static IEventListener ToWeakEventListener(this IEventListener listener)
        {
            if (listener.IsWeak)
                return listener;
            return new WeakEventListenerWrapper(listener);
        }

        /// <summary>
        /// Converts the specified <see cref="IEventListener"/> to a <see cref="WeakReference"/> if listener is not weak.
        /// </summary>
        public static object ToWeakItem(this IEventListener target)
        {
            if (target.IsWeak)
                return target;
            return ToolkitExtensions.GetWeakReference(target);
        }

        /// <summary>
        /// Gets the <see cref="IEventListener"/> from an object that was obtained from the <see cref="ToWeakItem"/> method.
        /// </summary>
        public static IEventListener GetEventListenerFromWeakItem(object target)
        {
            if (target == null)
                return null;
            var listener = target as IEventListener;
            if (listener == null)
                return (IEventListener)((WeakReference)target).Target;
            return listener;
        }

        /// <summary>
        ///     Creates an instance of <see cref="IBindingBuilder" />.
        /// </summary>
        [NotNull]
        public static IBindingBuilder CreateBuilderFromString(this IBindingProvider bindingProvider, [NotNull] object target,
             [NotNull] string targetPath, [CanBeNull] string expression, object source = null)
        {
            IList<object> sources = null;
            if (source != null)
                sources = new[] { source };
            return bindingProvider.CreateBuildersFromString(target, targetPath + " " + expression + ";", sources)[0];
        }

        /// <summary>
        ///     Creates a series of instances of <see cref="IDataBinding" />.
        /// </summary>
        [NotNull]
        public static IDataBinding CreateBindingFromString(this IBindingProvider bindingProvider, [NotNull] object target,
             [NotNull] string targetPath, [CanBeNull] string expression, object source = null)
        {
            IList<object> sources = null;
            if (source != null)
                sources = new[] { source };
            return bindingProvider.CreateBindingsFromString(target, targetPath + " " + expression + ";", sources)[0];
        }

        /// <summary>
        /// Tries to add the specified converter 
        /// </summary>
        public static bool TryAddConverter([CanBeNull]this IBindingResourceResolver resourceResolver, [NotNull] string name, [NotNull] IBindingValueConverter converter)
        {
            if (resourceResolver == null)
                return false;
            if (resourceResolver.ResolveConverter(name, DataContext.Empty, false) != null)
                return false;
            resourceResolver.AddConverter(name, converter, true);
            return true;
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull]this IBindingMemberProvider memberProvider, [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
        {
            memberProvider.Register(member.Path, member, rewrite);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        public static void Register<TTarget, TType>([NotNull]this IBindingMemberProvider memberProvider, string path, [NotNull] IAttachedBindingMemberInfo<TTarget, TType> member, bool rewrite = true)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            memberProvider.Register(typeof(TTarget), path, member, rewrite);
        }

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        public static IBindingContext GetBindingContext([NotNull] this IBindingContextManager contextManager, [NotNull] object target, [NotNull] string targetPath)
        {
            Should.NotBeNull(contextManager, "contextManager");
            if (targetPath == AttachedMemberConstants.DataContext)
                return new BindingContextWrapper(target);
            return contextManager.GetBindingContext(target);
        }

        /// <summary>
        ///     Gets the value that indicates that type has the member.
        /// </summary>
        public static bool HasMember([NotNull] this IBindingMemberProvider memberProvider, Type sourceType, string path, bool ignoreAttachedMembers)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            return memberProvider.GetBindingMember(sourceType, path, ignoreAttachedMembers, false) != null;
        }

        /// <summary>
        ///     Creates an binding set.
        /// </summary>
        public static BindingSet<TTarget, TSource> CreateBindingSet<TTarget, TSource>([NotNull] this TTarget target,
            IBindingProvider bindingProvider = null) where TTarget : class, IView
        {
            Should.NotBeNull(target, "target");
            return new BindingSet<TTarget, TSource>(target, bindingProvider);
        }

        /// <summary>
        ///     Gets the member names from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <param name="separator">The specified separator.</param>
        /// <returns>An instance of string.</returns>
        [Pure]
        public static string GetMemberPath([NotNull] LambdaExpression expression, string separator = ".")
        {
            var ret = new StringBuilder();
            Expression current = expression.Body;
            while (current.NodeType != ExpressionType.Parameter)
            {
                // This happens when a value type gets boxed
                if (current.NodeType == ExpressionType.Convert || current.NodeType == ExpressionType.ConvertChecked)
                {
                    var ue = (UnaryExpression)current;
                    current = ue.Operand;
                    continue;
                }

                string memberName;
                var methodCallExpression = current as MethodCallExpression;
                if (methodCallExpression != null)
                {
                    if (methodCallExpression.Method.Name != "get_Item")
                        throw BindingExceptionManager.InvalidBindingMemberExpression();
                    string s = string.Join(",", methodCallExpression.Arguments.Cast<ConstantExpression>().Select(e => e.Value));
                    memberName = "[" + s + "]";
                    current = methodCallExpression.Object;
                }
                else if (current.NodeType != ExpressionType.MemberAccess)
                    throw BindingExceptionManager.InvalidBindingMemberExpression();
                else
                {
                    var me = (MemberExpression)current;
                    memberName = me.Member.Name;
                    current = me.Expression;
                }

                if (ret.Length != 0)
                    ret.Insert(0, separator);
                ret.Insert(0, memberName);
                if (current == null)
                    break;
            }
            return ret.ToString();
        }

        /// <summary>
        ///     Gets the value of binding member.
        /// </summary>
        public static object GetValue([CanBeNull]this BindingMemberValue memberValue, object[] args)
        {
            if (memberValue == null)
                return BindingConstants.UnsetValue;
            object source = memberValue.MemberSource.Target;
            if (source == null)
                return BindingConstants.UnsetValue;
            try
            {
                return memberValue.Member.GetValue(source, args);
            }
            catch (Exception exception)
            {
                Tracer.Error(exception.Flatten(false));
                return BindingConstants.UnsetValue;
            }
        }

        /// <summary>
        ///     Tries to set value.
        /// </summary>
        public static bool TrySetValue<TResult>([CanBeNull]this BindingMemberValue memberValue, object[] args, out TResult result)
        {
            result = default(TResult);
            if (memberValue == null)
                return false;
            object source = memberValue.MemberSource.Target;
            if (memberValue.MemberSource.Target == null)
                return false;
            try
            {
                var value = memberValue.Member.SetValue(source, args);
                if (value is TResult)
                    result = (TResult)value;
            }
            catch (Exception exception)
            {
                Tracer.Error(exception.Flatten(false));
                return false;
            }
            return true;
        }

        public static object GetValueFromPath(object src, string strPath, int firstMemberIndex = 0)
        {
            IBindingPath path = BindingPath.Create(strPath);
            for (int index = firstMemberIndex; index < path.Parts.Count; index++)
            {
                var item = path.Parts[index];
                if (src == null)
                    return null;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(src.GetType(), item, false, true);
                src = member.GetValue(src, null);
            }
            return src;
        }

        public static string MergePath([CanBeNull]string left, [CanBeNull] string right)
        {
            if (string.IsNullOrEmpty(right))
                return left;
            if (string.IsNullOrEmpty(left))
                left = right;
            else
            {
                if (right.StartsWith("["))
                    left += right;
                else
                    left += "." + right;
            }
            return left;
        }

        [NotNull]
        public static string MergePath([CanBeNull] IList<string> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;
            return items.Aggregate(MergePathDelegate);
        }

        [CanBeNull]
        public static IBindingMemberInfo TryFindMemberChangeEvent([NotNull] IBindingMemberProvider memberProvider,
            [NotNull] Type type, [NotNull] string memberName)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            var member = memberProvider.GetBindingMember(type, memberName + "Changed", false, false);
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = memberProvider.GetBindingMember(type, memberName + "Change", false, false);

            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        public static bool PropertyNameEqual([CanBeNull] this DataErrorsChangedEventArgs args,
            [NotNull] IBindingSourceAccessor accessor)
        {
            if (args == null)
                return false;
            var singleAccessor = accessor as ISingleBindingSourceAccessor;
            if (singleAccessor != null)
                return ToolkitExtensions.PropertyNameEqual(args.PropertyName, singleAccessor.Source.Path.Parts.LastOrDefault(), true);
            for (int i = 0; i < accessor.Sources.Count; i++)
            {
                if (ToolkitExtensions.PropertyNameEqual(args.PropertyName, accessor.Sources[i].Path.Parts.LastOrDefault(), true))
                    return true;
            }
            return false;
        }

        internal static void CheckDuplicateLambdaParameter(ICollection<string> parameters)
        {
            if (parameters.Count == 0)
                return;
            var strings = new HashSet<string>();
            foreach (string parameter in parameters)
            {
                if (strings.Contains(parameter))
                    throw BindingExceptionManager.DuplicateLambdaParameter(parameter);
                strings.Add(parameter);
            }
        }

        public static Exception DuplicateLambdaParameter(string parameterName)
        {
            return BindingExceptionManager.DuplicateLambdaParameter(parameterName);
        }

        internal static string TryGetMemberName(this IExpressionNode target, bool allowIndexer, bool allowDynamicMember, IList<IExpressionNode> nodes = null, List<string> members = null)
        {
            if (target == null)
                return null;
            if (members == null)
                members = new List<string>();
            while (target != null)
            {
                if (nodes != null)
                    nodes.Insert(0, target);
                var expressionNode = target as IMemberExpressionNode;
                if (expressionNode == null)
                {
                    if (target is ResourceExpressionNode)
                    {
                        if (!allowDynamicMember || members.Count == 0)
                            return null;
                        break;
                    }

                    if (!allowIndexer)
                        return null;
                    var indexExpressionNode = target as IIndexExpressionNode;
                    if (indexExpressionNode == null ||
                        indexExpressionNode.Arguments.Any(arg => arg.NodeType != ExpressionNodeType.Constant))
                        return null;
                    IEnumerable<string> args = indexExpressionNode
                        .Arguments
                        .Cast<IConstantExpressionNode>()
                        .Select(node => node.Value == null ? "null" : node.Value.ToString());
                    members.Insert(0, string.Format("[{0}]", string.Join(",", args)).Trim());
                    target = indexExpressionNode.Object;
                }
                else
                {
                    string memberName = expressionNode.Member.Trim();
                    members.Insert(0, memberName);
                    target = expressionNode.Target;
                }
            }
            return string.Join(".", members);
        }

        internal static bool IsUnsetValueOrDoNothing(this object obj)
        {
            if (obj is DataConstant)
                return obj.IsDoNothing() || obj.IsUnsetValue();
            return false;
        }

        internal static bool IsUnsetValue(this object obj)
        {
            return ReferenceEquals(obj, BindingConstants.UnsetValue);
        }

        internal static bool IsDoNothing(this object obj)
        {
            return ReferenceEquals(obj, BindingConstants.DoNothing);
        }

        internal static T GetValueOrDefault<T>(this Func<IDataContext, T> getValue, IDataContext context, T value = default(T))
        {
            if (getValue == null)
                return value;
            return getValue(context);
        }

        #endregion
    }
}