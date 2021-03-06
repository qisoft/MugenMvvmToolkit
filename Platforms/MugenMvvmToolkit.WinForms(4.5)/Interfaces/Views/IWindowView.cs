﻿#region Copyright

// ****************************************************************************
// <copyright file="IWindowView.cs">
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

using System.ComponentModel;
using System.Windows.Forms;

namespace MugenMvvmToolkit.Interfaces.Views
{
    /// <summary>
    ///     Represent the base interface for a window view.
    /// </summary>
    public interface IWindowView : IView
    {
        /// <summary>
        ///     Shows window.
        /// </summary>
        void Show();

        /// <summary>
        ///     Shows window as dialog.
        /// </summary>
        /// <returns></returns>
        DialogResult ShowDialog();

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        void Close();

        /// <summary>
        ///     Occurred on closing window.
        /// </summary>
        event CancelEventHandler Closing;
    }
}