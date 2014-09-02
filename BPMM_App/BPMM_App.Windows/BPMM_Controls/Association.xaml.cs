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
using Windows.Data.Json;

namespace BPMM_App
{
    public partial class AssociationControl : UserControl, INotifyPropertyChanged
    {
        public PointCollection points = new PointCollection();
        public String description;
        public event PropertyChangedEventHandler PropertyChanged;

        public int sourceId;
        public int targetId;

        public event EventHandler DeleteEvent;

        public AssociationControl(BaseControl sourceControl, Point source, Point target)
        {
            InitializeComponent();

            Points.Add(source);
            Points.Add(target);
            Description = "[relation]";

            updateBoxPosition(source, target);
            sourceId = sourceControl.id;
            DataContext = this;
        }

        # region getters/setters
        public PointCollection Points
        {
            get { return points; }
        }

        public String Description
        {
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
            get { return description; }
        }
        
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        # endregion

        public void updateEndPoint(BaseControl targetControl, Point p)
        {
            if (targetControl != null)
            {
                targetId = targetControl.id;
            }
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

        public JsonObject serialize()
        {
            var associationEntry = new JsonObject();
            associationEntry.Add("relation", JsonValue.CreateStringValue(Description));
            associationEntry.Add("source", JsonValue.CreateNumberValue(sourceId));
            associationEntry.Add("target", JsonValue.CreateNumberValue(targetId));
            JsonArray pointsEntry = new JsonArray();
            int i = 0;
            foreach (var point in Points)
            {
                var pointEntry = new JsonObject();
                pointEntry.Add("index", JsonValue.CreateNumberValue(i++));
                pointEntry.Add("x", JsonValue.CreateNumberValue(point.X));
                pointEntry.Add("y", JsonValue.CreateNumberValue(point.Y));
                pointsEntry.Add(pointEntry);
            }
            associationEntry.Add("points", pointsEntry);
            return associationEntry;
        }

        public static AssociationControl deserialize(JsonObject input)
        {
            var sourceId = (int)input.GetNamedNumber("source", -1);
            var targetId = (int)input.GetNamedNumber("target", -1);
            if (sourceId == -1 || targetId == -1)
            {
                return null;
            }
            var source = MainPage.getControl(sourceId);
            var target = MainPage.getControl(targetId);
            if (source == null || target == null)
            {
                return null;
            }
            var pointsArray = input.GetNamedArray("points", null);
            if (pointsArray == null)
            {
                return null;
            }
            List<Point> points = new List<Point>();
            pointsArray.OrderBy(x => x.GetObject().GetNamedNumber("index"));
            foreach (var entry in pointsArray)
            {
                var pointEntry = entry.GetObject();
                var index = pointEntry.GetNamedNumber("index", -1);
                if (index == -1)
                {
                    return null;
                }
                var x = pointEntry.GetNamedNumber("x", -1);
                var y = pointEntry.GetNamedNumber("y", -1);
                if (x == -1 || y == -1)
                {
                    return null;
                }
                points.Add(new Point(x, y));
            }
            var association = new AssociationControl(source, points[0], points[points.Count - 1]);
            association.targetId = target.id;

            association.Description = input.GetNamedString("relation", "");

            for (int i = 0; i < points.Count; ++i)
            {
                association.Points[i] = points[i];
            }
            return association;
        }
    }
}
