﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace MailSender.Behaviours
{
    public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            MouseWheelEventArgs clonedEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent
            };

            AssociatedObject.RaiseEvent(clonedEvent);
        }
    }
}
