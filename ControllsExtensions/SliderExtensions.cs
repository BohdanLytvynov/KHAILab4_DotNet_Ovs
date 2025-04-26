using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SunRise_SunDown.ControllsExtensions
{
    internal static class SliderExtensions
    {
        #region Dependency Properties

        public static string GetSliderName(DependencyObject obj)
        {
            return (string)obj.GetValue(SliderNameProperty);
        }

        public static void SetSliderName(DependencyObject obj, string value)
        {
            obj.SetValue(SliderNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for SliderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderNameProperty;

        #endregion

        static SliderExtensions() 
        {
            SliderNameProperty =
            DependencyProperty.RegisterAttached("SliderName", typeof(string), typeof(SliderExtensions), new PropertyMetadata("Enter Slider Name"));
        }  
    }
}
