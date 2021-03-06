﻿#region Copyright

// ****************************************************************************
// <copyright file="BindingSyntaxEx.cs">
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvmToolkit.Binding.Attributes;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding.Extensions.Syntax
{
    /// <summary>
    ///     Represents the binding syntax extnensions.
    /// </summary>
    [BindingSyntaxExtensions]
    public static class BindingSyntaxEx
    {
        #region Fields

        /// <summary>
        ///     Gets the provide expression method name.
        /// </summary>
        public const string ProvideExpressionMethodName = "ProvideExpression";

        private const string ResourceMethodName = "Resource";
        private const string SelfMethodName = "Self";
        private const string SourceMethodName = "Source";
        private const string RootMethodName = "Root";
        private const string RelativeMethodName = "Relative";
        private static readonly MethodInfo GetEventMethod;
        private static readonly MethodInfo GetErrorsMethod;
        private static readonly MethodInfo ResourceMethodInfo;
        private static readonly MethodInfo ResourceMethodImplMethod;

        #endregion

        #region Constructors

        static BindingSyntaxEx()
        {
            GetEventMethod = typeof(BindingSyntaxEx).GetMethodEx("GetEvent",
                MemberFlags.NonPublic | MemberFlags.Static);
            GetErrorsMethod = typeof(BindingSyntaxEx).GetMethodEx("GetErrorsImpl",
                MemberFlags.NonPublic | MemberFlags.Static);
            ResourceMethodImplMethod = typeof(BindingSyntaxEx).GetMethodEx("ResourceMethodImpl",
                MemberFlags.NonPublic | MemberFlags.Static);
            ResourceMethodInfo = typeof(BindingSyntaxEx).GetMethodEx(ResourceMethodName,
                MemberFlags.Public | MemberFlags.Static);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the current data context.
        /// </summary>
        public static T DataContext<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets the current data context for the specified item.
        /// </summary>
        [BindingSyntaxMember]
        public static T DataContext<T>(this object item)
        {
            return (T)BindingServiceProvider.MemberProvider.GetMemberValue(item, AttachedMemberConstants.DataContext);
        }

        /// <summary>
        ///     Gets a relative element by type.
        /// </summary>
        public static T Relative<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets a relative element by type and level.
        /// </summary>
        public static T Relative<T>(uint level)
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gest an element by element id.
        /// </summary>
        public static T Element<T>(object elementId)
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets the self item.
        /// </summary>
        public static T Self<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets the root element.
        /// </summary>
        public static T Root<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets the binding source item.
        /// </summary>
        public static T Source<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets a resource object by name.
        /// </summary>
        public static T Resource<T>(string name)
        {
            return (T)BindingServiceProvider
                .ResourceResolver
                .ResolveObject(name, MugenMvvmToolkit.Models.DataContext.Empty, true)
                .Value;
        }

        /// <summary>
        ///     Invokes a resource method by name.
        /// </summary>
        public static T ResourceMethod<T>(string name, params object[] args)
        {
            return (T)ResourceMethodImpl(name, Empty.Array<Type>(), MugenMvvmToolkit.Models.DataContext.Empty, args);
        }

        /// <summary>
        ///     Gets a member value by name.
        /// </summary>
        [BindingSyntaxMember]
        public static T Member<T>(this object target, string member)
        {
            return (T)BindingServiceProvider.MemberProvider.GetMemberValue(target, member);
        }

        /// <summary>
        ///     Gets the current event args, if any.
        /// </summary>
        public static T EventArgs<T>()
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        /// <summary>
        ///     Gets the errors for the specified members.
        /// </summary>
        public static IEnumerable<object> GetErrors(params object[] args)
        {
            throw BindingExceptionManager.MethodNotSupportedBindingExpression();
        }

        private static Expression ProvideExpression(IBuilderSyntaxContext context)
        {
            var mExp = context.MethodExpression;
            var name = mExp.Method.Name;
            if (name == "EventArgs")
            {
                if (context.IsSameExpression())
                    return Expression.Convert(Expression.Call(GetEventMethod, context.ContextParameter), mExp.Method.ReturnType);
                return null;
            }

            if (name == "ResourceMethod")
            {
                if (!context.IsSameExpression())
                    return null;
                var typeArgsEx = Expression.NewArrayInit(typeof(Type), Expression.Constant(mExp.Method.ReturnType, typeof(Type)));
                return Expression.Convert(Expression.Call(ResourceMethodImplMethod, mExp.Arguments[0], typeArgsEx,
                            context.ContextParameter, mExp.Arguments[1]), context.Expression.Type);
            }

            if (name == "GetErrors")
            {
                if (!context.IsSameExpression())
                    return null;
                var id = Guid.NewGuid();
                var args = new List<Expression>();
                var members = new List<string>();
                var arrayExpression = mExp.Arguments[0] as NewArrayExpression;
                if (arrayExpression != null)
                {
                    for (int i = 0; i < arrayExpression.Expressions.Count; i++)
                    {
                        var constantExpression = arrayExpression.Expressions[i] as ConstantExpression;
                        if (constantExpression == null)
                            args.Add(arrayExpression.Expressions[i]);
                        else
                            members.Add((string)constantExpression.Value);
                    }
                }
                if (args.Count == 0)
                    args.Add(Expression.Call(ResourceMethodInfo.MakeGenericMethod(typeof(object)),
                        Expression.Constant(BindingServiceProvider.ResourceResolver.BindingSourceResourceName)));
                context.AddBuildCallback(syntax =>
                {
                    var behaviors = syntax.Builder.GetOrAddBehaviors();
                    if (!behaviors.Any(behavior => behavior is NotifyDataErrorsAggregatorBehavior))
                    {
                        behaviors.Clear();
                        behaviors.Add(new OneTimeBindingMode(false));
                    }
                    behaviors.Add(new NotifyDataErrorsAggregatorBehavior(id) { ErrorPaths = members.ToArray() });
                });
                var array = Expression.NewArrayInit(typeof(object), args.Select(e => ExpressionReflectionManager.ConvertIfNeed(e, typeof(object), false)));
                return Expression.Call(GetErrorsMethod, Expression.Constant(id), context.ContextParameter, array);
            }

            Expression lastExpression;
            string path = string.Empty;
            if (!context.IsSameExpression() &&
                !BindingExtensions.TryGetMemberPath(context.Expression, ".", false, out lastExpression, out path) &&
                lastExpression != mExp)
                return null;

            if (name == AttachedMemberConstants.DataContext && mExp.Arguments.Count == 0)
                return context.GetOrAddParameterExpression(string.Empty, path, context.Expression,
                    BindingExtensions.CreteBindingSourceFromContextDel);

            if (name == SelfMethodName || name == RootMethodName || name == ResourceMethodName || name == SourceMethodName)
            {
                string resourceName;
                switch (name)
                {
                    case SelfMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.SelfResourceName;
                        break;
                    case RootMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.RootElementResourceName;
                        break;
                    case SourceMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.BindingSourceResourceName;
                        break;
                    default:
                        var exp = mExp.Arguments[0];
                        Should.BeOfType<ConstantExpression>(exp, "arg");
                        resourceName = (string)((ConstantExpression)exp).Value;
                        break;
                }
                return context.GetOrAddParameterExpression("res:" + resourceName, path, context.Expression,
                    (dataContext, s) =>
                    {
                        var value = BindingServiceProvider
                            .ResourceResolver
                            .ResolveObject(resourceName, dataContext, true);
                        return BindingExtensions.CreateBindingSourceExplicit(dataContext, s, value);
                    });
            }

            if (name == RelativeMethodName || name == "Element")
            {
                var firstArg = mExp.Arguments.Count == 0 ? 1u : ((ConstantExpression)mExp.Arguments[0]).Value;
                var node = name == RelativeMethodName
                    ? RelativeSourceExpressionNode
                        .CreateRelativeSource(mExp.Method.ReturnType.AssemblyQualifiedName, (uint)firstArg, null)
                    : RelativeSourceExpressionNode.CreateElementSource(firstArg.ToString(), null);
                return context
                    .GetOrAddParameterExpression(name + mExp.Method.ReturnType.FullName, path, context.Expression,
                        (dataContext, s) => RelativeSourceBehavior.GetBindingSource(node, dataContext.GetData(BindingBuilderConstants.Target), s));
            }
            return null;
        }

        private static bool IsSameExpression(this IBuilderSyntaxContext context)
        {
            return context.MethodExpression == context.Expression;
        }

        private static object GetEvent(IDataContext context)
        {
            return context.GetData(BindingConstants.CurrentEventArgs);
        }

        private static object ResourceMethodImpl(string name, IList<Type> typeArgs, IDataContext context, params object[] args)
        {
            return BindingServiceProvider
                .ResourceResolver
                .ResolveMethod(name, context, true)
                .Invoke(typeArgs, args, context);
        }

        internal static IEnumerable<object> GetErrorsImpl(Guid id, IDataContext context, object[] args)
        {
            var binding = context.GetData(BindingConstants.Binding);
            if (binding == null)
                return Empty.Array<object>();
            foreach (var behavior in binding.Behaviors)
            {
                if (behavior.Id == id)
                    return ((NotifyDataErrorsAggregatorBehavior)behavior).Errors;
            }
            return Empty.Array<object>();
        }

        #endregion
    }
}