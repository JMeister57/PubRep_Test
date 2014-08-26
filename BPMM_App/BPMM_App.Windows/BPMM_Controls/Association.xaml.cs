using BPMM_App.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Diagnostics;
using Windows.System;
using Windows.UI.Popups;

namespace BPMM_App
{
    public partial class AssociationControl : UserControl
    {
        public PointCollection points = new PointCollection();
        public String description;

        private BPMMControl sourceControl;
        private BPMMControl targetControl;

        public Line_ViewModel viewModel;

        public event EventHandler DeleteEvent;

        public AssociationControl(BPMMControl sourceControl, Point source, Point target)
        {
            this.InitializeComponent();
            viewModel = new Line_ViewModel(this, source, target);
            updateBoxPosition(source, target);
            this.sourceControl = sourceControl;
            DataContext = viewModel;
        }

        public void updateEndPoint(BPMMControl targetControl, Point p)
        {
            this.targetControl = targetControl;
            points[1] = p;
            updateBoxPosition(points[0], p);
        }

        public void targetMoved(object sender, PointerRoutedEventArgs e)
        {
            points[1] = new Point(Canvas.GetLeft((UIElement)sender), Canvas.GetTop((UIElement)sender));
            updateBoxPosition(points[0], points[1]);
        }

        public void sourceMoved(object sender, PointerRoutedEventArgs e)
        {
            points[0] = new Point(Canvas.GetLeft((UIElement)sender), Canvas.GetTop((UIElement)sender));
            updateBoxPosition(points[0], points[1]);
        }

        private void updateBoxPosition(Point source, Point target)
        {
            if (Double.IsNaN(descriptionBox.Width)) {
                // happens on initial AssociationControl creation
                descriptionBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                descriptionBox.Arrange(new Rect(0, 0, descriptionBox.DesiredSize.Width, descriptionBox.DesiredSize.Height));
            }

            Point boxShift = new Point();
            if (target.X > source.X)
            {
                boxShift.Y = descriptionBox.MinHeight;
                if (target.Y < source.Y)
                { // positive slope
                    boxShift.X = descriptionBox.ActualWidth;
                }
            }
            else
            {
                boxShift.Y = descriptionBox.MinHeight;
                if (source.Y < target.Y)
                { // positive slope
                    boxShift.X = descriptionBox.ActualWidth;
                }
            }
            double xOffset = Math.Min(source.X, target.X) + (Math.Max(source.X, target.X) - Math.Min(source.X, target.X)) / 2 - boxShift.X;
            double yOffset = Math.Min(source.Y, target.Y) + (Math.Max(source.Y, target.Y) - Math.Min(source.Y, target.Y)) / 2 - boxShift.Y;
            Canvas.SetLeft(descriptionBox, xOffset);
            Canvas.SetTop(descriptionBox, yOffset);
        }

        public void Delete(object sender, EventArgs e)
        {
            if (DeleteEvent != null)
            {
                DeleteEvent(this, e);
            }
        }

        public class Line_ViewModel : INotifyPropertyChanged
        {
            private AssociationControl parent;

            public event PropertyChangedEventHandler PropertyChanged;

            public Line_ViewModel(AssociationControl parent, Point source, Point target)
            {
                this.parent = parent;
                Points.Add(source);
                Points.Add(target);
                Description = "[relation]";
            }

            # region getters/setters
            public PointCollection Points
            {
                get { return parent.points; }
            }

            public String Description
            {
                set { parent.description = value; }
                get { return parent.description; }
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

        private void UserControl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Delete:
                    Delete(sender, EventArgs.Empty);
                    return;
            }
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Focus(FocusState.Pointer);
        }

        private async void UserControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Delete Association"));

            GeneralTransform transform = descriptionBox.TransformToVisual(null);
            Point pointTransformed = transform.TransformPoint(new Point(0, 0));
            Debug.WriteLine("point: {0}, {1}, ActualSize: {2}, {3}", pointTransformed.X, pointTransformed.Y, ActualWidth, ActualHeight);
            Rect rect = new Rect(pointTransformed.X, pointTransformed.Y, ActualWidth, ActualHeight);
            var response = await menu.ShowForSelectionAsync(rect);
            if (response != null && response.Label == "Delete Association")
            {
                if (DeleteEvent != null)
                {
                    DeleteEvent(this, EventArgs.Empty);
                }

            }
        }
    }
}
