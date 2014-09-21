using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BPMM_App
{
    public class NoteControl : BaseControl
    {
        private TextBox textField;

        public NoteControl() : base(Category.NOTE)
        {
            textField = new BPMM_TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "A Note... ",
                AcceptsReturn = true
            };
            frame.Children.Add(textField);

            textField.TextChanged += TextField_TextChanged;
            frame.AddHandler(UIElement.DoubleTappedEvent, new DoubleTappedEventHandler(Frame_DoubleTapped), true);
        }

        public override void UpdateFontSize(double scale)
        {
            textField.FontSize = textField.FontSize*scale;
        }

        private void TextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textField.Text.Length == 0)
            {
                textField.PlaceholderText = "A Note... ";
            }
        }
        private void Frame_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            textField.IsEnabled = true;
        }

        public override JsonObject serialize()
        {
            var noteEntry = base.serialize();
            if (textField.Text.Length != 0)
            {
                noteEntry.Add("description", JsonValue.CreateStringValue(textField.Text));
            }
            return noteEntry;
        }

        public static NoteControl deserialize(JsonObject input)
        {
            var note = (NoteControl)BaseControl.deserialize(input);
            var description = input.GetNamedString("description", "");
            note.textField.Text = description;
            return note;
        }
    }
}
