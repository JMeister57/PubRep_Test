using BMM_App.Common;
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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Globalization;
using System.Collections.Specialized;

namespace BMM_App
{
    public class Link : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<Point> points = new ObservableCollection<Point>();
        public String description;
        public event PropertyChangedEventHandler PropertyChanged;

        public BaseModel sourceModel;
        public BaseModel targetModel;

        private Polyline line;
        private Border descriptionBorder;
        private TextBox descriptionBox;
        private TextBlock descriptionBlock;
        private List<Border> circles = new List<Border>();
        private Canvas container;

        private int newPoint;
        private bool movingPoint = false;
        private bool showCircles = false;

        public event EventHandler DeleteEvent;

        public Link(BaseModel sourceModel, Point source, Point target)
        {
            descriptionBox = new TextBox() { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            var binding = new Binding() { Path = new PropertyPath("Description"), Mode = BindingMode.TwoWay };
            descriptionBox.SetBinding(TextBox.TextProperty, binding);
            descriptionBox.KeyUp += description_KeyUp;
            descriptionBlock = new TextBlock() { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            descriptionBlock.SetBinding(TextBlock.TextProperty, binding);
            descriptionBlock.DoubleTapped += description_DoubleTapped;
            descriptionBorder = new Border() { Child = descriptionBlock };

            line = new Polyline() { Stroke = new SolidColorBrush(Colors.White), ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY };
            points.CollectionChanged += points_CollectionChanged;
            var lineBinding = new Binding() { Path = new PropertyPath("Points"), Converter = new PointCollectionConverter() };
            line.SetBinding(Polyline.PointsProperty, lineBinding);
            line.Tapped += line_Tapped;
            line.ManipulationStarted += line_ManipulationStarted;
            line.ManipulationDelta += line_ManipulationDelta;
            line.LostFocus += line_LostFocus;

            AddHandler(PointerPressedEvent, new PointerEventHandler(model_PointerPressed), true);
            PointerMoved += model_PointerMoved;
            PointerReleased += model_PointerReleased;
            
            container = new Canvas();
            container.Children.Add(descriptionBorder);
            container.Children.Add(line);
            Content = container;

            Points.Add(source);
            Points.Add(target);
            Description = descriptionBox.PlaceholderText = "[relation]";

            this.sourceModel = sourceModel;
            DataContext = this;
        }

        private Border newCircle(Point p)
        {
            var circle = new Border()
            {
                CornerRadius = new CornerRadius(30),
                Width = 10, Height = 10,
                BorderBrush = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(2),
            };
            Canvas.SetLeft(circle, p.X - 5);
            Canvas.SetTop(circle, p.Y - 5);
            return circle;
        }

        public void UpdateFontSize(double scale)
        {
            descriptionBox.FontSize = descriptionBox.FontSize * scale;
            updateDescriptionPos();
        }

        # region getters/setters
        public ObservableCollection<Point> Points
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

        public bool ShowCircles
        {
            get { return showCircles; }
            set
            {
                showCircles = value;
                OnPropertyChanged("ShowCircles");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        # endregion

        public void updateStartPoint(BaseModel sourceModel, Point p)
        {
            if (sourceModel != null)
            {
                this.sourceModel = sourceModel;
            }
            Points[0] = p;
        }

        public void updateEndPoint(BaseModel targetModel, Point p)
        {
            if (targetModel != null)
            {
                this.targetModel = targetModel;
            }
            Points[Points.Count - 1] = p;
        }

        private void description_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Description = (Description == descriptionBox.PlaceholderText) ? "" : Description;
            descriptionBorder.Child = descriptionBox;
            descriptionBox.Focus(FocusState.Programmatic);
        }

        private void description_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                descriptionBorder.Child = descriptionBlock;
                var currWidth = descriptionBlock.ActualWidth;
                descriptionBlock.Text = (descriptionBox.Text.Length > 0) ? descriptionBox.Text : descriptionBox.PlaceholderText;
                descriptionBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (descriptionBlock.DesiredSize.Width > currWidth)
                {
                    descriptionBlock.Width = descriptionBlock.ActualWidth + descriptionBlock.DesiredSize.Width - currWidth;
                }
            }
        }

        private void line_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowCircles = true;
        }

        private void line_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowCircles = false;
        }

        private void line_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            newPoint = insert(e.Position);
            ShowCircles = true;
        }

        private void line_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Points[newPoint] = e.Position;
            ShowCircles = true;
        }

        private void model_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (movingPoint)
            {
                CapturePointer(e.Pointer);
            }
        }
        private void model_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (movingPoint)
            {
                Points[newPoint] = e.GetCurrentPoint((UIElement)Parent).Position;
            }
        }

        private void model_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (movingPoint)
            {
                if (newPoint == 0 || newPoint == Points.Count - 1)
                { // snap endpoints back to model edge
                    var model = (newPoint == 0) ? sourceModel : targetModel;
                    var p = new Point();
                    p.X =
                        (Points[newPoint].X < Canvas.GetLeft(model)) ? Canvas.GetLeft(model) :
                        (Points[newPoint].X > Canvas.GetLeft(model) + model.GetWidth()) ? Canvas.GetLeft(model) + model.GetWidth() :
                        Points[newPoint].X;
                    p.Y =
                        (Points[newPoint].Y < Canvas.GetTop(model)) ? Canvas.GetTop(model) :
                        (Points[newPoint].Y > Canvas.GetTop(model) + model.GetHeight()) ? Canvas.GetTop(model) + model.GetHeight() :
                        Points[newPoint].Y;
                    Points[newPoint] = p;
                }
                movingPoint = false;
                ReleasePointerCapture(e.Pointer);
            }
        }

        private int insert(Point p)
        {
            for (int i = 0; i < Points.Count - 1; ++i)
            {
                var lowerX = Math.Min(Points[i].X, Points[i + 1].X);
                var upperX = Math.Max(Points[i].X, Points[i + 1].X);
                var lowerY = Math.Min(Points[i].Y, Points[i + 1].Y);
                var upperY = Math.Max(Points[i].Y, Points[i + 1].Y);
                if (lowerX <= p.X && p.X <= upperX && lowerY <= p.Y && p.Y <= upperY)
                {
                    Points.Insert(i + 1, p);
                    return i + 1;
                }
            }
            return -1;
        }

        private void points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var circle in circles)
            {
                container.Children.Remove(circle);
            }
            circles.Clear();
            foreach (var p in Points)
            {
                var circle = newCircle(p);
                circle.PointerPressed += delegate(object elem, PointerRoutedEventArgs args)
                {
                    newPoint = circles.IndexOf((Border)elem);
                    movingPoint = true;
                };
                circle.SetBinding(Border.VisibilityProperty, new Binding() { Path = new PropertyPath("ShowCircles"), Converter = new BooleanToVisibilityConverter() });
                circles.Add(circle);
                container.Children.Add(circle);
            }
            updateDescriptionPos();
            OnPropertyChanged("Points");
        }

        public void targetMoved(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            updateEndPoint(null, new Point(Points.Last().X + e.Delta.Translation.X, Points.Last().Y + e.Delta.Translation.Y));
        }

        public void sourceMoved(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            updateStartPoint(null, new Point(Points.First().X + e.Delta.Translation.X, Points.First().Y + e.Delta.Translation.Y));
        }

        private void updateDescriptionPos()
        {
            if (Points.Count < 2)
            {
                return;
            }
            if (Double.IsNaN(descriptionBorder.Width))
            {
                // happens on initial Link creation
                descriptionBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                descriptionBorder.Arrange(new Rect(0, 0, descriptionBorder.DesiredSize.Width, descriptionBorder.DesiredSize.Height));
            }
            // place description between the two points at middle of the line (for uneven # points, left to the middle)
            Point p = Points[Points.Count/2 - 1];
            Point q = Points[Points.Count / 2];
            Point boxShift = new Point();
            if (q.X > p.X)
            {
                boxShift.Y = descriptionBorder.MinHeight;
                if (q.Y < p.Y)
                { // positive slope
                    boxShift.X = descriptionBorder.ActualWidth;
                }
            }
            else
            {
                boxShift.Y = descriptionBorder.MinHeight;
                if (p.Y < q.Y)
                { // positive slope
                    boxShift.X = descriptionBorder.ActualWidth;
                }
            }
            double xOffset = Math.Min(p.X, q.X) + (Math.Max(p.X, q.X) - Math.Min(p.X, q.X)) / 2 - boxShift.X;
            double yOffset = Math.Min(p.Y, q.Y) + (Math.Max(p.Y, q.Y) - Math.Min(p.Y, q.Y)) / 2 - boxShift.Y;
            Canvas.SetLeft(descriptionBorder, xOffset);
            Canvas.SetTop(descriptionBorder, yOffset);
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

        private async void UserControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Delete Link"));

            GeneralTransform transform = descriptionBox.TransformToVisual(null);
            Point pointTransformed = transform.TransformPoint(new Point(0, 0));
            Rect rect = new Rect(pointTransformed.X, pointTransformed.Y, ActualWidth, ActualHeight);
            var response = await menu.ShowForSelectionAsync(rect);
            if (response != null && response.Label == "Delete Link")
            {
                if (DeleteEvent != null)
                {
                    DeleteEvent(this, EventArgs.Empty);
                }
            }
        }

        public JsonObject serialize()
        {
            var linkEntry = new JsonObject();
            linkEntry.Add("relation", JsonValue.CreateStringValue(Description));
            linkEntry.Add("source", JsonValue.CreateNumberValue(sourceModel.id));
            linkEntry.Add("target", JsonValue.CreateNumberValue(targetModel.id));
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
            linkEntry.Add("points", pointsEntry);
            return linkEntry;
        }

        public static Link deserialize(JsonObject input)
        {
            var sourceId = (int)input.GetNamedNumber("source", -1);
            var targetId = (int)input.GetNamedNumber("target", -1);
            if (sourceId == -1 || targetId == -1)
            {
                return null;
            }
            var source = MainPage.getModel(sourceId);
            var target = MainPage.getModel(targetId);
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
            var link = new Link(source, points[0], points[points.Count - 1]);
            link.targetModel = target;

            link.Description = input.GetNamedString("relation", "");

            for (int i = 0; i < points.Count; ++i)
            {
                link.Points[i] = points[i];
            }
            return link;
        }
    }

    public class PointCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var points = value as ObservableCollection<Point>;
            if (points == null)
            {
                return null;
            }
            var collection = new PointCollection();
            foreach (var point in points)
            {
               // Debug.WriteLine("point {0}, {1}", point.X, point.Y);
                collection.Add(point);
            }

            return collection;
        }
        
        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = System.Convert.ToBoolean(value);
            return (val) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}