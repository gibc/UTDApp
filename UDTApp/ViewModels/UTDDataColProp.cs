using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using UDTApp.SchemaModels;

namespace UDTApp.ViewModels
{
    public class UTDDataColProp : DependencyObject
    {
        public static readonly DependencyProperty
            UDTDataColProperty =
            DependencyProperty.RegisterAttached(
                  "UDTDataCol", typeof(ObservableCollection<UDTData>), typeof(UTDDataColProp),
            new PropertyMetadata(default(ObservableCollection<UDTData>)));

        public static ObservableCollection<UDTData> GetDataCol(
            DependencyObject d)
        {
            return (ObservableCollection<UDTData>)d.GetValue(UDTDataColProperty);
        }
        public static void SetDataCol(
            DependencyObject d, ObservableCollection<UDTData> value)
        {
            d.SetValue(UDTDataColProperty, value);
        }
    }

    public class Ex : DependencyObject
    {
        public static readonly DependencyProperty
            SecurityIdProperty =
            DependencyProperty.RegisterAttached(
                  "SecurityId", typeof(ObservableCollection<UDTBase>), typeof(Ex),
            new PropertyMetadata(default(ObservableCollection<UDTBase>)));

        public static ObservableCollection<UDTBase> GetSecurityId(
            DependencyObject d)
        {
            return (ObservableCollection<UDTBase>)d.GetValue(SecurityIdProperty);
        }
        public static void SetSecurityId(
            DependencyObject d, ObservableCollection<UDTBase> value)
        {
            d.SetValue(SecurityIdProperty, value);
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            TextBox tb = d as TextBox;
            if ((bool)e.NewValue)
            {
                tb.SelectAll();
                //uie.RaiseEvent()
                uie.Focus(); // Don't care about false values.
  
                bool var = tb.IsKeyboardFocused;
              
            }
        }
    }

    public class AttachedAdorner
    {
       public static readonly DependencyProperty AdornerProperty = 
          DependencyProperty.RegisterAttached(
             "Adorner", typeof (Type), typeof (AttachedAdorner),
             new FrameworkPropertyMetadata(default(Type),
                 FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, 
                 PropertyChangedCallback));
 
       private static void PropertyChangedCallback(
          DependencyObject dependencyObject, 
          DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
       {
          var frameworkElement = dependencyObject as FrameworkElement;
          if (frameworkElement != null)
          {
              //frameworkElement.Loaded += Loaded;
              //Loaded(dependencyObject, dependencyPropertyChangedEventArgs)
              var adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
              if (dependencyPropertyChangedEventArgs.NewValue != dependencyPropertyChangedEventArgs.OldValue && 
                  adornerLayer != null)
              {
                    if(dependencyPropertyChangedEventArgs.NewValue == null)
                    {
                        if (adornerLayer.GetAdorners(frameworkElement) == null) return;
                        List<Adorner> adorners = adornerLayer.GetAdorners(frameworkElement).ToList<Adorner>();
                        foreach (Adorner adron in adorners)
                        { 
                            if (adron.GetType() == typeof(NoteAdorner))
                            { 
                                adornerLayer.Remove(adron);
                                return;
                            }
                        }
                    }
                    else
                    { 
                        var adorner = Activator.CreateInstance(
                            GetAdorner(frameworkElement),
                            frameworkElement) as Adorner;

                        //var adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
                        if (adornerLayer != null)
                        {
                            adornerLayer.Add(adorner); 
                        }
                    }
              }
          }
       }
 
       private static void Loaded(object sender, RoutedEventArgs e)
       {
          var frameworkElement = sender as FrameworkElement;
          if (frameworkElement != null)
          {
             var adorner = Activator.CreateInstance(
                GetAdorner(frameworkElement), 
                frameworkElement) as Adorner;
             if(adorner != null)
             {
                var adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
                adornerLayer.Add(adorner);
             }
          }
       }
 
       public static void SetAdorner(DependencyObject element, Type value)
       {
          element.SetValue(AdornerProperty, value);
       }
 
       public static Type GetAdorner(DependencyObject element)
       {
          return (Type)element.GetValue(AdornerProperty);
       }
    }

    public class NoteAdorner : Adorner
    {
        public NoteAdorner(UIElement adornedElement) :
            base(adornedElement)
        {
            Opacity = 0.2;
 
        }

        protected override void OnRender(DrawingContext drawingContext)
        {

            var adornedElementRect = new Rect(AdornedElement.RenderSize);

            var right = adornedElementRect.Right;
            var left = adornedElementRect.Left;
            var top = adornedElementRect.Top;
            var bottom = adornedElementRect.Bottom;

            var segments = new[]
               {
                  new LineSegment(new Point(left, top), true), 
                  new LineSegment(new Point(right, top), true), 
                  new LineSegment(new Point(right, bottom), true),
                  new LineSegment(new Point(left, bottom), true),
                  new LineSegment(new Point(left, top), true), 
               };

            var figure = new PathFigure(new Point(left, top), segments, true);
            var geometry = new PathGeometry(new[] { figure });
            drawingContext.DrawGeometry(Brushes.Gray, null, geometry);
        }
    }
}
