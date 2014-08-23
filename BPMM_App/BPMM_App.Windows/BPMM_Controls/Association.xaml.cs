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

namespace BPMM_App
{
    public partial class AssociationControl : UserControl
    {
        public PointCollection points = new PointCollection();
        public String description;

        public Line_ViewModel viewModel;
        public AssociationControl(Point source, Point target)
        {
            this.InitializeComponent();
            viewModel = new Line_ViewModel(this, source, target);
            updateBoxPosition(source, target);
            DataContext = viewModel;
        }

        public void updateEndPoint(Point p)
        {
            points[1] = p;
            updateBoxPosition(points[0], p);
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
    }
}
