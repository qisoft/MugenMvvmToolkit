﻿#region Copyright

// ****************************************************************************
// <copyright file="EmptyPathObserver.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that uses the empty path member to observe.
    /// </summary>
    public sealed class EmptyPathObserver : ObserverBase
    {
        #region Nested types

        private sealed class EmptyBindingPathMembers : DefaultListener, IBindingPathMembers
        {
            #region Constructors

            public EmptyBindingPathMembers(WeakReference @ref)
                : base(@ref)
            {
            }

            #endregion

            #region Implementation of IBindingPathMembers

            public IBindingPath Path
            {
                get { return BindingPath.Empty; }
            }

            public bool AllMembersAvailable
            {
                get { return Ref.Target != null; }
            }

            public IList<IBindingMemberInfo> Members
            {
                get { return new[] { LastMember }; }
            }

            public IBindingMemberInfo LastMember
            {
                get { return BindingMemberInfo.Empty; }
            }

            public object Source
            {
                get
                {
                    var observer = (ObserverBase)Ref.Target;
                    if (observer == null)
                        return null;
                    return observer.GetActualSource();
                }
            }

            public object PenultimateValue
            {
                get { return Source; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IBindingPathMembers _members;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmptyPathObserver" /> class.
        /// </summary>
        public EmptyPathObserver([NotNull] object source, [NotNull] IBindingPath path)
            : base(source, path)
        {
            Should.BeSupported(path.IsEmpty, "The EmptyPathObserver supports only empty path members.");
            _members = new EmptyBindingPathMembers(ServiceProvider.WeakReferenceFactory(this, true));
            Initialize((IEventListener)_members);
        }

        #endregion

        #region Overrides of ObserverBase

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected override void UpdateInternal()
        {
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        protected override IBindingPathMembers GetPathMembersInternal()
        {
            return _members;
        }

        #endregion
    }
}