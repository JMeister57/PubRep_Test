using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
            textField = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "A Note... ",
                AcceptsReturn = true
            };
            setContent(textField);
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
