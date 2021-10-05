using ICSharpCode.AvalonEdit;
using System;
using System.Windows;
using System.Windows.Interactivity;

namespace MailSender.Behaviours
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        //public static readonly DependencyProperty GiveMeTheTextProperty =
        //    DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(AvalonEditBehaviour),
        //    new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.None, PropertyChangedCallback));

        public static readonly DependencyProperty MyCaretOffSetProperty =
            DependencyProperty.Register("MyCaretOffSet", typeof(int), typeof(AvalonEditBehaviour),
            new PropertyMetadata(default));

        //public string GiveMeTheText
        //{
        //    get { return (string)GetValue(GiveMeTheTextProperty); }
        //    set { SetValue(GiveMeTheTextProperty, value); }
        //}

        public int MyCaretOffSet
        {
            get { return (int)GetValue(MyCaretOffSetProperty); }
            set { SetValue(MyCaretOffSetProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                //AssociatedObject.Initialized += AssociatedObjectOnTextChanged;
                //AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                //AssociatedObject.Initialized -= AssociatedObjectOnTextChanged;
                //AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }
        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            if (sender is TextEditor textEditor)
            {
                if (textEditor.Document != null)
                {
                    //GiveMeTheText = textEditor.Document.Text;
                    MyCaretOffSet = textEditor.CaretOffset;
                }
            }
        }
        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior.AssociatedObject != null)
            {
                TextEditor editor = behavior.AssociatedObject as TextEditor;
                if (editor.Document != null)
                {
                    var caretOffset = editor.CaretOffset;

                    editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();

                    int textLength = editor.Document.Text.Length;
                    //editor.CaretOffset = textLength;
                    editor.CaretOffset = textLength == 0 ? 0 : textLength;
                }
            }
        }
    }
}
