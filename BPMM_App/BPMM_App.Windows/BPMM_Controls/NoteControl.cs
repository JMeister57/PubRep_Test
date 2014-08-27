using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public NoteControl() : base()
        {
            TextBox textField = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "A Note... ",
                AcceptsReturn = true
            };
            setContent(textField);
        }

        public override bool linkableWith(BaseControl target)
        {
            return true;
        }
        public override bool LinkWith(BaseControl target)
        {
            return true;
        }
    }
}
