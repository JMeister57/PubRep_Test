using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.System;
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
    public enum Category
    {
        VISION, GOAL, OBJECTIVE, MISSION, STRATEGY, TACTIC, BUSINESS_POLICY, BUSINESS_RULE, INFLUENCER, ASSESSMENT, NOTE
    }
    public abstract class BaseControl : UserControl
    {
        private const int MIN_SIZE = 100;
        private static int max_id = 0;

        public int id;
        public Category category;

        protected Grid frame;
        private Grid container;
        public Border anchor;

        private Point pointerPressPos;
        public event ManipulationDeltaEventHandler MovedEvent;
        public event ManipulationCompletedEventHandler MoveEndEvent;
        public event PointerEventHandler LinkStartEvent;
        public event EventHandler LinkEndEvent;
        public event EventHandler DeleteEvent;

        public BaseControl(Category category)
        {
            id = ++max_id;
            this.category = category;
            AddHandler(UIElement.RightTappedEvent, new RightTappedEventHandler(BaseControl_RightTapped), false);

            frame = new Grid() { Width = 200, Height = 200, Background = new SolidColorBrush(Colors.Linen)};
            frame.ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            frame.AddHandler(UIElement.ManipulationDeltaEvent, new ManipulationDeltaEventHandler(BaseControl_ManipulationDelta), false);
            frame.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BaseControl_PointerPressed), false);
            frame.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BaseControl_PointerReleased), false);
            frame.AddHandler(UIElement.ManipulationCompletedEvent, new ManipulationCompletedEventHandler(BaseControl_ManipulationComplete), false);

            anchor = new Border() { Opacity = 0.4 };
            var linkIcon = new SymbolIcon(Symbol.Go) { Foreground = new SolidColorBrush(Colors.White) };
            linkIcon.PointerPressed += anchor_PointerPressed;
            anchor.Child = linkIcon;
            container = new Grid();
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

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);
            Size desiredSize = new Size();
            container.Measure(availableSize);
            desiredSize = container.DesiredSize;
            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            frame.Width = finalSize.Width * 0.90;
            frame.Height = finalSize.Height * 0.90;
            frame.Arrange(new Rect(0, 0, finalSize.Width * 0.90, finalSize.Height * 0.90));
            anchor.Width = finalSize.Width * 0.1;
            anchor.Height = finalSize.Width * 0.1;
            anchor.Arrange(new Rect(frame.ActualWidth, 0, finalSize.Width * 0.1, finalSize.Width * 0.1));
            container.Width = finalSize.Width;
            container.Height = finalSize.Height;
            container.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return size;
        }

        public double GetWidth()
        {
            return frame.ActualWidth;
        }

        public double GetHeight()
        {
            return frame.ActualHeight;
        }

        public abstract void UpdateFontSize(double scale);

        public virtual JsonObject serialize()
        {
            var controlEntry = new JsonObject();
            controlEntry.Add("category", JsonValue.CreateNumberValue((int)category));
            controlEntry.Add("x", JsonValue.CreateNumberValue(Canvas.GetLeft(this)));
            controlEntry.Add("y", JsonValue.CreateNumberValue(Canvas.GetTop(this)));
            controlEntry.Add("width", JsonValue.CreateNumberValue(ActualWidth));
            controlEntry.Add("height", JsonValue.CreateNumberValue(ActualHeight));
            return controlEntry;
        }

        public static BaseControl deserialize(JsonObject input)
        {
            var value = input.GetNamedNumber("category", -1);
            if (value == -1)
            {
                return null;
            }
            try
            {
                Category newType = (Category)value;
                var control =
                    (newType == Category.NOTE) ? (BaseControl)new NoteControl() :
                    (newType == Category.BUSINESS_RULE) ? (BaseControl)new BusinessRuleControl() :
                    (newType == Category.INFLUENCER) ? (BaseControl)new InfluencerControl() :
                    (newType == Category.ASSESSMENT) ? (BaseControl)new AssessmentControl() :
                    new BPMMControl(newType);
                Canvas.SetLeft(control, input.GetNamedNumber("x", 0));
                Canvas.SetTop(control, input.GetNamedNumber("y", 0));
                return control;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        public static void resetIds() {
            max_id = 0;
        }

        private void BaseControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointerPressPos = e.GetCurrentPoint((UIElement)Parent).Position;
        }
        private async void BaseControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.GetPosition((UIElement)Parent).X - pointerPressPos.X > 1 || e.GetPosition((UIElement)Parent).Y - pointerPressPos.Y > 1)
            {
                return;
            }
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

        private void BaseControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Scale != 1)
            { // Scaling
                Resize(e.Delta.Scale);
                e.Handled = true;
                return;
            }
            // Translation
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.Delta.Translation.Y);
            if (MovedEvent != null)
            {
                MovedEvent(this, e);
            }
            e.Handled = true;
        }

        private void BaseControl_ManipulationComplete(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X != 0 || e.Cumulative.Translation.Y != 0)
            {
                if (MoveEndEvent != null)
                {
                    MoveEndEvent(this, e);
                }
            }
        }

        public void Resize(double scale)
        {
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Width = ActualWidth * scale;
            Height = ActualHeight * scale;
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }

        private void BaseControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (LinkEndEvent != null)
            {
                LinkEndEvent(this, EventArgs.Empty);
            }
            e.Handled = true;
        }

        private void anchor_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            anchor.Opacity = 1;
            if (LinkStartEvent != null)
            {
                LinkStartEvent(this, e);
                e.Handled = true;
            }
        }
    }

    public class BPMM_TextBox : TextBox
    {
        public bool shiftPressed;

        public BPMM_TextBox()
            : base()
        {
            shiftPressed = false;
            KeyDown += tb_keyDown;
            KeyUp += tb_keyUp;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            shiftPressed = false;
        }

        private void tb_keyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Shift)
            {
                shiftPressed = false;
            }
            else if (e.Key == VirtualKey.Enter)
            {
                if (shiftPressed)
                {
                    Text += "\n";
                    Select(Text.Length, 0);
                }
            }
        }

        private void tb_keyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Shift)
            {
                shiftPressed = true;
            }
        }
    }
}
