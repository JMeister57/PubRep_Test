using BPMM_App.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;

namespace BPMM_App
{
    public partial class BPMMControl : UserControl
    {

        private const int MIN_SIZE = 100;

        public BPMM_Object.Type type;
        private bool resizing;
        public BPMM_ViewModel viewModel;
        

        public BPMMControl(BPMM_Object obj)
        {
            this.InitializeComponent();
            resizing = false;
            viewModel = new BPMM_ViewModel(obj);
            DataContext = this.viewModel;
        }

        public BPMMControl(BPMMControl c)
        {
            InitializeComponent();
            this.bpmmObject.Height = c.bpmmObject.Height;
            this.bpmmObject.Width = c.bpmmObject.Height;
        }

        // drag
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (resizing)
            {
                return;
            }
            if (e.IsInertial) // disable inertia, we don't want any flicking
            {
                e.Complete();
                e.Handled = true;
            }

            var ct = (CompositeTransform)bpmmObject.RenderTransform;
            ct.TranslateX += e.Delta.Translation.X; 
            ct.TranslateY += e.Delta.Translation.Y;
        }

        #region resize
        private void ThumbTopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width -= xChange;
            bpmmObject.Height -= yChange;
            Canvas.SetLeft(bpmmObject, Canvas.GetLeft(bpmmObject) + xChange);
            Canvas.SetTop(bpmmObject, Canvas.GetTop(bpmmObject) + yChange);
        }

        private void ThumbTopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width += xChange;
            bpmmObject.Height -= yChange;
            Canvas.SetTop(bpmmObject, Canvas.GetTop(bpmmObject) + yChange);
        }

        private void ThumbBottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width -= xChange;
            bpmmObject.Height += yChange;
            Canvas.SetLeft(bpmmObject, Canvas.GetLeft(bpmmObject) + xChange);
        }

        private void ThumbBottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width += xChange;
            bpmmObject.Height += yChange;
        }

        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            resizing = false;
        }
        # endregion
    }

    public class BPMM_ViewModel : INotifyPropertyChanged
    {
        public BPMM_Object linkedObject;
        public event PropertyChangedEventHandler PropertyChanged;

        public BPMM_ViewModel(BPMM_Object obj)
        {
            linkedObject = obj;
        }

        # region getters/setters
        public string Title
        {
            get { return linkedObject.title; }
            set
            {
                linkedObject.title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Description
        {
            get { return linkedObject.description; }
            set
            {
                linkedObject.description = value;
                OnPropertyChanged("Description");
            }
        }
        # endregion

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
