#region Copyright

// ****************************************************************************
// <copyright file="PlatformWrapperRegistrationModule.cs">
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
using System.Windows;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Modules
{
    public class PlatformWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        internal sealed class WindowViewWrapper : IWindowView, IViewWrapper
        {
            #region Fields

            private readonly Window _window;

            #endregion

            #region Constructors

            public WindowViewWrapper(Window window)
            {
                Should.NotBeNull(window, "window");
                _window = window;
            }

            #endregion

            #region Implementation of IWindowView

            public object View
            {
                get { return _window; }
            }

            public void Show()
            {
                _window.Show();
            }

            public bool? ShowDialog()
            {
                return _window.ShowDialog();
            }

            public void Close()
            {
                _window.Close();
            }

            event CancelEventHandler IWindowView.Closing
            {
                add { _window.Closing += value; }
                remove { _window.Closing -= value; }
            }

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        /// <summary>
        ///     Registers the wrappers using <see cref="WrapperManager" /> class.
        /// </summary>
        protected override void RegisterWrappers(WrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, WindowViewWrapper>(
                (type, context) => typeof (Window).IsAssignableFrom(type),
                (o, context) => new WindowViewWrapper((Window) o));
        }

        #endregion
    }
}