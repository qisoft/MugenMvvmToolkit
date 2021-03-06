﻿#region Copyright

// ****************************************************************************
// <copyright file="IHasOperationResult.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the model that has operation result.
    /// </summary>
    public interface IHasOperationResult
    {
        /// <summary>
        ///     Gets or sets the operation result value.
        /// </summary>
        bool? OperationResult { get; set; }
    }
}