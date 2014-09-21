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

        public BaseControl source;
        public BaseControl target;

        public event EventHandler DeleteEvent;

        public AssociationControl(BaseControl sourceControl, Point source, Point target)
        {
            InitializeComponent();

            Points.Add(source);
            Points.Add(target);
            Description = "[relation]";

            updateBoxPosition(source, target);
            this.source = sourceControl;
            DataContext = this;
        }

        public void UpdateFontSize(double scale)
        {
            descriptionBox.FontSize = descriptionBox.FontSize * scale;
            updateBoxPosition(Points[0], Points[1]);
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

        #region validation
        public static List<WarningItem> validateAssessment(BPMMControl control, List<AssociationControl> links)
        {
            var warnings = new List<WarningItem>();
            bool m_assessment_influencer = true;
            bool m_assessment_ends_means = true;
            foreach (var link in links)
            {
                if (m_assessment_influencer && link.contains(Category.INFLUENCER))
                {
                    m_assessment_influencer = false;
                }
                if (m_assessment_ends_means && link.contains(Category.VISION, Category.GOAL, Category.OBJECTIVE, Category.MISSION, Category.STRATEGY, Category.TACTIC))
                {
                    m_assessment_ends_means = false;
                }
            }
            if (m_assessment_influencer)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_ASSESSMENT_INFLUENCER));
            }
            if (m_assessment_ends_means)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS));
            }
            return warnings;
        }

        public static List<WarningItem> validateInfluencer(BPMMControl control, List<AssociationControl> links)
        {
            var warnings = new List<WarningItem>();
            bool m_influencer_assessment = true;
            bool w_influencer_ass_dir = false;
            foreach (var link in links)
            {
                if (m_influencer_assessment && link.contains(Category.ASSESSMENT))
                {
                    m_influencer_assessment = false;
                }
                if (w_influencer_ass_dir == false && link.misses(Category.ASSESSMENT, Category.BUSINESS_POLICY, Category.BUSINESS_RULE))
                {
                    w_influencer_ass_dir = true;
                }
            }
            if (m_influencer_assessment)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_INFLUENCER_ASSESSMENT));
            }
            if (w_influencer_ass_dir)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE));
            }
            return warnings;
        }

        public static List<WarningItem> validateBusinessRule(BPMMControl control, List<AssociationControl> links)
        {
            var warnings = new List<WarningItem>();
            bool m_dir_action = true;
            bool w_rule_vision_mission = false;
            foreach (var link in links)
            {
                if (m_dir_action && link.contains(Category.STRATEGY, Category.TACTIC))
                {
                    m_dir_action = false;
                }
                if (w_rule_vision_mission == false && link.contains(Category.VISION, Category.MISSION))
                {
                    w_rule_vision_mission = true;
                }
            }
            if (m_dir_action)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_DIRECTIVE_ACTION));
            }
            if (w_rule_vision_mission)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION));
            }
            return warnings;
        }

        public static List<WarningItem> validateBusinessPolicy(BPMMControl control, List<AssociationControl> links)
        {
            var warnings = new List<WarningItem>();
            bool m_policy_rule = true;
            bool m_dir_action = true;
            bool w_policy_vision_mission = false;
            foreach (var link in links)
            {
                if (m_policy_rule && link.contains(Category.BUSINESS_RULE))
                {
                    m_policy_rule = false;
                }
                if (m_dir_action && link.contains(Category.STRATEGY, Category.TACTIC))
                {
                    m_dir_action = false;
                }
                if (w_policy_vision_mission == false && link.contains(Category.VISION, Category.MISSION))
                {
                    w_policy_vision_mission = true;
                }
            }
            if (m_policy_rule)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_POLICY_RULE));
            }
            if (m_dir_action)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.M_DIRECTIVE_ACTION));
            }
            if (w_policy_vision_mission)
            {
                warnings.Add(new WarningItem(control, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION));
            }
            return warnings;
        }



        public bool contains(params Category[] types)
        {
            if (source is BPMMControl)
            {
                foreach (var type in types)
                {
                    if (((BPMMControl)source).category == type)
                    {
                        return true;
                    }
                }
            }
            if (target is BPMMControl)
            {
                foreach (var type in types)
                {
                    if (((BPMMControl)target).category == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool misses(params Category[] types)
        {
            if (source is BPMMControl)
            {
                foreach (var type in types)
                {
                    if (((BPMMControl)source).category == type)
                    {
                        return false;
                    }
                }
            }
            if (target is BPMMControl)
            {
                foreach (var type in types)
                {
                    if (((BPMMControl)target).category == type)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion
        public void updateEndPoint(BaseControl targetControl, Point p)
        {
            if (targetControl != null)
            {
                target = targetControl;
            }
            points[1] = p;
            updateBoxPosition(points[0], p);
        }

        public void targetMoved(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            points[1] = new Point(Canvas.GetLeft((UIElement)sender), Canvas.GetTop((UIElement)sender));
            updateBoxPosition(points[0], points[1]);
        }

        public void sourceMoved(object sender, ManipulationDeltaRoutedEventArgs e)
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
            associationEntry.Add("source", JsonValue.CreateNumberValue(source.id));
            associationEntry.Add("target", JsonValue.CreateNumberValue(target.id));
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
            association.target = target;

            association.Description = input.GetNamedString("relation", "");

            for (int i = 0; i < points.Count; ++i)
            {
                association.Points[i] = points[i];
            }
            return association;
        }
    }
}
