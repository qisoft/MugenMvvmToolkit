#region Copyright

// ****************************************************************************
// <copyright file="PlatformBootstrapperService.cs">
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    internal sealed class PlatformBootstrapperService : XamarinFormsBootstrapperBase.IPlatformService
    {
        #region Constructors

        static PlatformBootstrapperService()
        {
            CompiledExpressionInvoker.SupportCoalesceExpression = false;
        }

        #endregion

        #region Implementation of IPlatformService

        public Func<IBindingMemberInfo, Type, object, object> ValueConverter
        {
            get { return BindingReflectionExtensions.Convert; }
        }

        public PlatformInfo GetPlatformInfo()
        {
#if WINDOWS_PHONE
            return new PlatformInfo(PlatformType.WinPhone, Environment.OSVersion.Version);
#elif TOUCH
            Version result;
            Version.TryParse(UIKit.UIDevice.CurrentDevice.SystemVersion, out result);
            return new PlatformInfo(PlatformType.iOS, result);
#else
            Version result;
            Version.TryParse(Android.OS.Build.VERSION.Release, out result);
            return new PlatformInfo(PlatformType.Android, result);
#endif

        }

        public ICollection<Assembly> GetAssemblies()
        {
#if WINDOWS_PHONE
            var listAssembly = new HashSet<Assembly>();
            foreach (var part in System.Windows.Deployment.Current.Parts)
            {
                string assemblyName = part.Source.Replace(".dll", string.Empty);
                if (assemblyName.Contains("/"))
                    continue;
                try
                {
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (assembly.IsToolkitAssembly())
                        listAssembly.Add(assembly);
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return listAssembly;
#else
            return new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies());
#endif
        }

        #endregion
    }
}