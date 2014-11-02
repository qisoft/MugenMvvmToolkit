#region Copyright
// ****************************************************************************
// <copyright file="BooleanToCheckmarkAccessoryConverter.cs">
// Copyright � Vyacheslav Volkov 2012-2014
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
using System.Globalization;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Converters;

namespace MugenMvvmToolkit.Converters
{
    public class BooleanToCheckmarkAccessoryConverter : ValueConverterBase<bool?, UITableViewCellAccessory>
    {
        #region Overrides of ValueConverterBase<bool,UITableViewCellAccessory>

        protected override UITableViewCellAccessory Convert(bool? value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value.GetValueOrDefault() ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
        }

        protected override bool? ConvertBack(UITableViewCellAccessory value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value == UITableViewCellAccessory.Checkmark;
        }

        #endregion
    }
}