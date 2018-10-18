using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BrownianMotion_WPF
{

    
        
    public class MouseDrag
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(MouseDrag),
                new UIPropertyMetadata(CommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof(object),
                typeof(MouseDrag),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is Thumb control)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    control.DragDelta += OnMouseDrag;
                    //control.MouseDoubleClick += OnMouseDrag;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.DragDelta -= OnMouseDrag;
                }
            }
        }

        private static void OnMouseDrag(object sender, RoutedEventArgs e)
        {
            Control control = sender as Thumb;
            ICommand command = (ICommand)control.GetValue(CommandProperty);
            var parameter = new MouseDragArgs(){e = e, sender = sender};
            
            object commandParameter = parameter;
            command.Execute(commandParameter);
        }
    }
}
