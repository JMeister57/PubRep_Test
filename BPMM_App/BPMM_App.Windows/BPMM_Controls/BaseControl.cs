using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace BPMM_App
{
    public abstract class BaseControl : UserControl
    {
        private const int MIN_SIZE = 100;

        protected Grid frame;
        Grid contentGrid;
        protected bool isDragging;
        private PointerPoint offset;

        public event PointerEventHandler MovedEvent;
        public event PointerEventHandler AssociationEvent;
        public event EventHandler DeleteEvent;

        public BaseControl()
        {
            RightTapped += BaseControl_RightTapped;

            frame = new Grid() { Width = 200, Height = 200, Background = new SolidColorBrush(Colors.LightBlue) };
            frame.PointerPressed += UserControl_PointerPressed;
            frame.PointerMoved += UserControl_PointerMoved;
            frame.PointerReleased += UserControl_PointerReleased;
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });
            frame.RowDefinitions.Add(new RowDefinition());
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });

            Thumb topLeftThumb = new Thumb()
            {
                Height = 10, Width = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Colors.White)
            };
            Thumb topRightThumb = new Thumb()
            {
                Height = 10, Width = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Colors.White)
            };
            Thumb bottomLeftThumb = new Thumb()
            {
                Height = 10, Width = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Colors.White)
            };
            Thumb bottomRightThumb = new Thumb()
            {
                Height = 10,
                Width = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Colors.White)
            };
            topLeftThumb.DragDelta += ThumbTopLeft_DragDelta;
            topRightThumb.DragDelta += ThumbTopRight_DragDelta;
            bottomLeftThumb.DragDelta += ThumbBottomLeft_DragDelta;
            bottomRightThumb.DragDelta += ThumbBottomRight_DragDelta;

            contentGrid = new Grid();
            Grid.SetRow(topLeftThumb, 0);
            Grid.SetRow(topRightThumb, 0);
            Grid.SetRow(contentGrid, 1);
            Grid.SetRow(bottomLeftThumb, 2);
            Grid.SetRow(bottomRightThumb, 2);
            frame.Children.Add(topLeftThumb);
            frame.Children.Add(topRightThumb);
            frame.Children.Add(bottomLeftThumb);
            frame.Children.Add(bottomRightThumb);
            frame.Children.Add(contentGrid);

            Rectangle anchor = new Rectangle()
            {
                Height = 15,
                Width = 15,
                VerticalAlignment = VerticalAlignment.Top,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.White)
            };
            anchor.PointerPressed += anchor_PointerPressed;

            Grid container = new Grid();
            container.ColumnDefinitions.Add(new ColumnDefinition());
            container.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(frame, 0);
            Grid.SetColumn(anchor, 1);
            container.Children.Add(frame);
            container.Children.Add(anchor);

            Canvas canvas = new Canvas();
            canvas.Children.Add(container);
            Content = canvas;
        }

        public abstract bool linkableWith(BaseControl target);
        public abstract bool LinkWith(BaseControl target);

        protected void setContent(FrameworkElement element)
        {
            Grid.SetRow(element, 1);
            element.Margin = new Thickness(10, 0, 10, 0);
            frame.Children.Add(element);
        }

        private async void BaseControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Delete BPMM Object"));

            var response = await menu.ShowForSelectionAsync(MenuPos());
            if (response != null && response.Label == "Delete BPMM Object")
            {
                MessageDialog affirmationPopup = new MessageDialog("", string.Format("Really Delete this?"));
                affirmationPopup.Commands.Add(new UICommand("Ok"));
                affirmationPopup.Commands.Add(new UICommand("Cancel"));
                var response2 = await affirmationPopup.ShowAsync();
                if (response2 != null && response2.Label == "Ok")
                {
                    if (DeleteEvent != null)
                    {
                        DeleteEvent(this, EventArgs.Empty);
                    }
                }
            }
        }

        private Rect MenuPos()
        {
            GeneralTransform transform = TransformToVisual(null);
            Point pointTransformed = transform.TransformPoint(new Point(0, 0));
            return new Rect(pointTransformed.X, pointTransformed.Y, ActualWidth, ActualHeight);
        }

        #region dragging
        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isDragging = true;
            offset = e.GetCurrentPoint(this);
            frame.CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isDragging)
            {
                PointerPoint currPos = e.GetCurrentPoint(Parent as UIElement);
                double prevPosX = Canvas.GetLeft(this);
                double prevPosY = Canvas.GetTop(this);

                double newPosX = currPos.Position.X - offset.Position.X;
                double newPosY = currPos.Position.Y - offset.Position.Y;
                Canvas.SetLeft(this, newPosX);
                Canvas.SetTop(this, newPosY);
                if (MovedEvent != null)
                {
                    MovedEvent(this, e);
                }
            }
            e.Handled = true;
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                frame.ReleasePointerCapture(e.Pointer);
            }
            e.Handled = true;
        }
        #endregion

        #region resize
        private void ThumbTopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double xChange = frame.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = frame.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            frame.Width -= xChange;
            frame.Height -= yChange;
            Canvas.SetLeft(frame, Canvas.GetLeft(frame) + xChange);
            Canvas.SetTop(frame, Canvas.GetTop(frame) + yChange);
        }

        private void ThumbTopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double xChange = frame.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = frame.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            frame.Width += xChange;
            frame.Height -= yChange;
            Canvas.SetTop(frame, Canvas.GetTop(frame) + yChange);
        }

        private void ThumbBottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double xChange = frame.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = frame.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            frame.Width -= xChange;
            frame.Height += yChange;
            Canvas.SetLeft(frame, Canvas.GetLeft(frame) + xChange);
        }

        private void ThumbBottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double xChange = frame.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = frame.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            frame.Width += xChange;
            frame.Height += yChange;
        }
        #endregion

        private void anchor_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (AssociationEvent != null)
            {
                AssociationEvent(this, e);
                e.Handled = true;
            }
        }
    }
}
