using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunRise_SunDown.Helpers
{
    public abstract class AnimatedValueBase : UIElement 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class AnimatedDoubleValueTracker : AnimatedValueBase
    {
        private double _value_cache; // this stores the current animated value!
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                if (_value_cache == value)
                    return;
                _value_cache = value;
                SetValue(ValueProperty, value);
                FirePropertyChanged("Value");
            }
        }

        public static readonly DependencyProperty ValueProperty;

        static AnimatedDoubleValueTracker()
        {
            ValueProperty = DependencyProperty.Register("Value", typeof(double), 
                typeof(AnimatedDoubleValueTracker), new UIPropertyMetadata(0.0, ValuePropertyChanged));
        }

        private static void ValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var self = (AnimatedDoubleValueTracker)sender;
            var value = (double)e.NewValue;
            self.UpdateValue(value);
        }

        private void UpdateValue(double value)
        {
            _value_cache = value;
            FirePropertyChanged("Value");
        }
    }
}
