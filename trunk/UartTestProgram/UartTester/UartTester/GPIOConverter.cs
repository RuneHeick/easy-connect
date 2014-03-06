using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UartTester
{
    public class CheckBoxFlagsBehaviour
    {
        private static bool isValueChanging;

        public static string GetMask(DependencyObject obj)
        {
            return (string)obj.GetValue(MaskProperty);
        }

        public static void SetMask(DependencyObject obj, string value)
        {
            obj.SetValue(MaskProperty, value);
        }

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached("Mask", typeof(string),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(null));

        public static byte GetValue(DependencyObject obj)
        {
            return (byte)obj.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject obj, Byte value)
        {
            obj.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached("Value", typeof(byte),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(default(byte), ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            isValueChanging = true;
            byte mask = Convert.ToByte(GetMask(d));
            byte value = Convert.ToByte(e.NewValue);

            BindingExpression exp = BindingOperations.GetBindingExpression(d, IsCheckedProperty);
            PropertyInfo pi = exp.DataItem.GetType().GetProperty(exp.ParentBinding.Path.Path);
            pi.SetValue(exp.DataItem, (value & mask) != 0, null);

            ((CheckBox)d).IsChecked = (value & mask) != 0;
            isValueChanging = false;
        }

        public static bool? GetIsChecked(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsCheckedProperty);
        }

        public static void SetIsChecked(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.RegisterAttached("IsChecked", typeof(bool?),
            typeof(CheckBoxFlagsBehaviour), new UIPropertyMetadata(false, IsCheckedChanged));

        private static void IsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (isValueChanging) return;

            bool? isChecked = (bool?)e.NewValue;
            if (isChecked != null)
            {
                byte mask = Convert.ToByte(GetMask(d));
                byte value = Convert.ToByte(GetValue(d));

                if (isChecked.Value)
                {
                    if ((value & mask) == 0)
                    {
                        value = (byte)(value + mask);
                    }
                }
                else
                {
                    if ((value & mask) != 0)
                    {
                        value = (byte)(value - mask);
                    }
                }

                BindingExpression exp = BindingOperations.GetBindingExpression(d, ValueProperty);
                PropertyInfo pi = exp.DataItem.GetType().GetProperty(exp.ParentBinding.Path.Path);
                pi.SetValue(exp.DataItem, value, null);
            }
        }
    }  
}
