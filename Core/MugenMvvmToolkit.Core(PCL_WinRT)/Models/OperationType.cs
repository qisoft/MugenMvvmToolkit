﻿#region Copyright
// ****************************************************************************
// <copyright file="OperationType.cs">
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
namespace MugenMvvmToolkit.Models
{
    public class OperationType : StringConstantBase<OperationType>
    {
        #region Fields

        public static readonly OperationType TabNavigation;

        public static readonly OperationType WindowNavigation;

        public static readonly OperationType Navigation;

        #endregion

        #region Constructors

        static OperationType()
        {
            TabNavigation = new OperationType("Tab");
            WindowNavigation = new OperationType("Window");
            Navigation = new OperationType("Navigation");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationType" /> class.
        /// </summary>
        public OperationType(string id)
            : base(id)
        {
        }

        #endregion
    }
}