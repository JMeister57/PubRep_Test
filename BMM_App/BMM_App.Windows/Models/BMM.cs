using BMM_App.Common;
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
using System.Collections.ObjectModel;
using Windows.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Data.Json;
using Windows.UI;

namespace BMM_App
{
    public class BMM : BaseModel, INotifyPropertyChanged
    {
        public static ObservableCollection<string> states = new ObservableCollection<string> { "created", "approved", "denied", "duplicate" };

        public string title;
        public string description;
        public string references;
        public object state;

        public ObservableCollection<WarningItem> warnings = new ObservableCollection<WarningItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event TappedEventHandler BMMTappedEvent;
        public event EventHandler<ObservableCollection<WarningItem>> WarningsAddedEvent;
        public event EventHandler<ObservableCollection<WarningItem>> WarningsRemovedEvent;

        public BMM_TextBox headerBox;
        protected TextBlock headerBlock;
        public Border headerBorder;
        public BMM_TextBox descriptionBox;
        protected TextBlock descriptionBlock;
        public Border descriptionBorder;
        public ComboBox stateCombo;

        public BMM(Category category, string author)
            : base(category, author)
        {
            States = states;
            state = States[0];
            this.category = category;
            DataContext = this;

            AddHandler(UIElement.TappedEvent, new TappedEventHandler(delegate(object elem, TappedRoutedEventArgs args)
            {
                if (BMMTappedEvent != null)
                {
                    BMMTappedEvent(this, args);
                }
            }), true);

            headerBox = new BMM_TextBox() { Margin = new Thickness(0, 0, 0, 2)};
            headerBox.DataContext = this;
            var headerBinding = new Binding() { Path = new PropertyPath("Title"), Mode = BindingMode.TwoWay };
            headerBox.SetBinding(TextBox.TextProperty, headerBinding);
            headerBox.KeyUp += headerBox_KeyUp;
            headerBox.LostFocus += headerBox_LostFocus;
            headerBlock = new TextBlock() { Margin = new Thickness(0, 0, 0, 2), VerticalAlignment = VerticalAlignment.Center, FontSize = 18};
            headerBlock.SetBinding(TextBlock.TextProperty, headerBinding);
            headerBlock.DoubleTapped += headerBlock_DoubleTapped;
            headerBorder = new Border() { Child = headerBlock };

            descriptionBox = new BMM_TextBox() { TextWrapping = TextWrapping.Wrap };
            descriptionBox.DataContext = this;
            var descriptionBinding = new Binding() { Path = new PropertyPath("Description"), Mode = BindingMode.TwoWay };
            descriptionBox.SetBinding(TextBox.TextProperty, descriptionBinding);
            descriptionBox.KeyUp += descriptionBox_KeyUp;
            descriptionBlock = new TextBlock() { TextWrapping = TextWrapping.Wrap, FontSize = 13};
            descriptionBlock.DoubleTapped += descriptionBlock_DoubleTapped;
            descriptionBlock.SetBinding(TextBlock.TextProperty, descriptionBinding);
            descriptionBorder = new Border() { Child = descriptionBlock };

            stateCombo = new ComboBox();
            Binding statesBinding = new Binding() { Source = States, Mode = BindingMode.TwoWay };
            State = States[0];
            Binding stateBinding = new Binding() { Path = new PropertyPath("State"), Mode = BindingMode.TwoWay };
            stateCombo.SetBinding(ComboBox.ItemsSourceProperty, statesBinding);
            stateCombo.SetBinding(ComboBox.SelectedItemProperty, stateBinding);

            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40, GridUnitType.Pixel) });
            Grid.SetRow(headerBorder, 0);
            Grid.SetRow(descriptionBorder, 1);
            Grid.SetRow(stateCombo, 2);
            frame.Children.Add(headerBorder);
            frame.Children.Add(descriptionBorder);
            frame.Children.Add(stateCombo);
            
            switch (category)
            { // type dependent configuration
                case Category.VISION:
                    frame.Background = new SolidColorBrush(Colors.LightGreen);
                    Title = headerBox.PlaceholderText = "Vision";
                    Description = descriptionBox.PlaceholderText = "Describes the future state of the enterprise, without regard to how it is to be achieved.";
                    break;
                case Category.GOAL:
                    frame.Background = new SolidColorBrush(Colors.LightGreen);
                    Title = headerBox.PlaceholderText = "Goal";
                    Description = descriptionBox.PlaceholderText = "Indicates what must be satisfied on a continuing basis to effectively attain the Vision.";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_GOAL_OBJECTIVE));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_GOAL_VISION));
                    break;
                case Category.OBJECTIVE:
                    frame.Background = new SolidColorBrush(Colors.LightGreen);
                    Title = headerBox.PlaceholderText = "Objective";
                    Description = descriptionBox.PlaceholderText = "An attainable, time-restricted, and measurable target to achieve enterprise Goals.";
                    break;
                case Category.MISSION:
                    frame.Background = new SolidColorBrush(Colors.LightYellow);
                    Title = headerBox.PlaceholderText = "Mission";
                    Description = descriptionBox.PlaceholderText = "Indicates the ongoing operational activity of the enterprise (the day-to-day activities).";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_MISSION_STRATEGY));
                    break;
                case Category.STRATEGY:
                    frame.Background = new SolidColorBrush(Colors.LightYellow);
                    Title = headerBox.PlaceholderText = "Strategy";
                    Description = descriptionBox.PlaceholderText = "A component of the plan for the Mission: The essential Course of Action to achieve Ends (especially Goals).";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_STRATEGY_TACTIC));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT));
                    break;
                case Category.TACTIC:
                    frame.Background = new SolidColorBrush(Colors.LightYellow);
                    Title = headerBox.PlaceholderText = "Tactic";
                    Description = descriptionBox.PlaceholderText = "Represents part of the detailing of Strategies: Implements Strategies.";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT));
                    break;
                case Category.BUSINESS_POLICY:
                    frame.Background = new SolidColorBrush(Colors.LightBlue);
                    Title = headerBox.PlaceholderText = "Business Policy";
                    Description = descriptionBox.PlaceholderText = "Non-enforceable Directive to govern or guide the enterprise.\nProvides the basis for Business Rules.";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_POLICY_RULE));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION));
                    break;
                case Category.BUSINESS_RULE:
                    frame.Background = new SolidColorBrush(Colors.LightBlue);
                    Title = headerBox.PlaceholderText = "Business Rule";
                    Description = descriptionBox.PlaceholderText = "Enforceable atomic Directive that is often derived from Business Policies.";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION));
                    break;
                case Category.INFLUENCER:
                    frame.Background = new SolidColorBrush(Colors.Plum);
                    Title = headerBox.PlaceholderText = "Influencer";
                    Description = descriptionBox.PlaceholderText = "External or internal neutral impact on enterprise’s employment of Means or achievement of Ends.";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_INFLUENCER_ASSESSMENT));
                    break;
                case Category.ASSESSMENT:
                    frame.Background = new SolidColorBrush(Colors.Plum);
                    Title = headerBox.PlaceholderText = "Assessment";
                    Description = descriptionBox.PlaceholderText = "A Judgment of an Influencer's impact on the achievement of Ends and/or Means";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_INFLUENCER));
                    break;
            }
            if (WarningsAddedEvent != null)
            {
                WarningsAddedEvent(this, warnings);
            }
        }

        public override void UpdateFontSize(double scale)
        {
            headerBox.FontSize = headerBox.FontSize * scale;
            descriptionBox.FontSize = descriptionBox.FontSize * scale;
            stateCombo.FontSize = stateCombo.FontSize * scale;
        }

        #region getters/setters
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        public ObservableCollection<String> States
        {
            get { return states; }
            set
            {
                states = value;
                OnPropertyChanged("States");
            }
        }

        public string References
        {
            get { return references; }
            set
            {
                references = value;
                OnPropertyChanged("References");
            }
        }

        public object State
        {
            get { return state; }
            set { state = value; OnPropertyChanged("State"); }
        }

        public ObservableCollection<WarningItem> getWarnings()
        {
            return warnings;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private void headerBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && headerBox.shiftPressed == false)
            {
                headerLostFocus();
            }
        }

        private void headerBox_LostFocus(object sender, RoutedEventArgs e)
        {
            headerLostFocus();
        }

        private void headerLostFocus()
        {
            if (headerBorder.Child == headerBox)
            {
                headerBorder.Child = headerBlock;
                var currWidth = headerBlock.ActualWidth;
                headerBlock.Text = (headerBox.Text.Length > 0)? headerBox.Text : headerBox.PlaceholderText;
                headerBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (headerBlock.DesiredSize.Width > currWidth)
                {
                    Width = ActualWidth + headerBlock.DesiredSize.Width - currWidth;
                }
            }
        }

        private void headerBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            descriptionLostFocus();
            Title = (Title == headerBox.PlaceholderText) ? "" : Title;
            headerBorder.Child = headerBox;
            headerBox.Focus(FocusState.Programmatic);
        }

        protected void descriptionBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && descriptionBox.shiftPressed == false)
            {
                descriptionLostFocus();
            }
        }

        private void descriptionBox_LostFocus()
        {
            descriptionLostFocus();
        }

        private void descriptionLostFocus()
        {
            if (descriptionBorder.Child == descriptionBox)
            {
                descriptionBorder.Child = descriptionBlock;
                var currHeight = descriptionBlock.ActualHeight;
                descriptionBlock.Text = (descriptionBox.Text.Length > 0)? descriptionBox.Text : descriptionBox.PlaceholderText;
                descriptionBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if(descriptionBlock.DesiredSize.Height > currHeight) {
                    Height = ActualHeight + descriptionBlock.DesiredSize.Height - currHeight;
                }
            }
        }

        private void descriptionBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            headerLostFocus();
            Description = (Description == descriptionBox.PlaceholderText) ? "" : Description;
            descriptionBorder.Child = descriptionBox;
            descriptionBox.Focus(FocusState.Programmatic);
        }

        #region validation

        public void validateNewLink(Category linkedType)
        {
            var added = new ObservableCollection<WarningItem>();
            var removed = new ObservableCollection<WarningItem>();
            switch (category)
            { // check if an incorrect link is added or if a missing link is restored.
                case Category.VISION:
                    if (linkedType != Category.MISSION && linkedType != Category.GOAL && linkedType != Category.ASSESSMENT)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Category.GOAL:
                    if (linkedType == Category.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_GOAL_OBJECTIVE);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Category.VISION)
                    {
                        var item = getWarning(WarningItem.Codes.M_GOAL_VISION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Category.OBJECTIVE:
                    break;
                case Category.MISSION:
                    if (linkedType == Category.STRATEGY)
                    {
                        var item = getWarning(WarningItem.Codes.M_MISSION_STRATEGY);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Category.STRATEGY:
                    if (linkedType == Category.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_STRATEGY_TACTIC);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Category.GOAL || linkedType == Category.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_ACTION_RESULT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Category.TACTIC:
                    if (linkedType == Category.GOAL || linkedType == Category.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_ACTION_RESULT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Category.BUSINESS_POLICY:
                    if (linkedType == Category.BUSINESS_RULE)
                    {
                        var item = getWarning(WarningItem.Codes.M_POLICY_RULE);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Category.STRATEGY || linkedType == Category.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_DIRECTIVE_ACTION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Category.VISION || linkedType == Category.MISSION)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Category.BUSINESS_RULE:
                    if (linkedType == Category.STRATEGY || linkedType == Category.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_DIRECTIVE_ACTION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Category.VISION || linkedType == Category.MISSION)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Category.INFLUENCER:
                    if (linkedType == Category.ASSESSMENT)
                    {
                        var item = getWarning(WarningItem.Codes.M_INFLUENCER_ASSESSMENT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    else if (linkedType != Category.BUSINESS_POLICY && linkedType != Category.BUSINESS_RULE)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE);
                        warnings.Add(item);
                        added.Add(item);

                    }
                    break;
                case Category.ASSESSMENT:
                    if (linkedType == Category.VISION || linkedType == Category.GOAL || linkedType == Category.OBJECTIVE
                        || linkedType == Category.MISSION || linkedType == Category.STRATEGY || linkedType == Category.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    else if (linkedType == Category.INFLUENCER)
                    {
                        var item = getWarning(WarningItem.Codes.M_ASSESSMENT_INFLUENCER);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
            }
            if (WarningsAddedEvent != null && added.Count > 0)
            {
                WarningsAddedEvent(this, added);
            }
            if (WarningsRemovedEvent != null && removed.Count > 0)
            {
                WarningsRemovedEvent(this, removed);
            }
        }
        public void validateRemovedLink(Category linkedType, List<Link> remainingLinks)
        {
            var removed = new ObservableCollection<WarningItem>();
            var added = new ObservableCollection<WarningItem>();
            switch (category)
            { // check if an incorrect link has been removed or if a needed one is destroyed.
                case Category.VISION:
                    if (getWarning(WarningItem.Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT) != null)
                    {
                        bool removeWarning = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category != Category.MISSION && src.category != Category.GOAL && src.category != Category.ASSESSMENT
                                && trgt.category != Category.MISSION && trgt.category != Category.GOAL && trgt.category != Category.ASSESSMENT)
                            {
                                removeWarning = false;
                                break;
                            }
                        }
                        if (removeWarning)
                        {
                            var item = getWarning(WarningItem.Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT);
                            warnings.Remove(item);
                            removed.Add(item);
                        }
                    }
                    break;
                case Category.GOAL:
                    if (linkedType == Category.OBJECTIVE || linkedType == Category.VISION)
                    {
                        bool warnObjective = true;
                        bool warnVision = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.OBJECTIVE || trgt.category == Category.OBJECTIVE)
                            {
                                warnObjective = false;
                            }
                            else if (src.category == Category.VISION || trgt.category == Category.VISION)
                            {
                                warnVision = false;
                            }
                        }
                        if (warnObjective)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_GOAL_OBJECTIVE);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (warnVision)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_GOAL_VISION);
                            warnings.Add(item);
                            added.Add(item);
                        }
                    }
                    break;
                case Category.OBJECTIVE:
                    break;
                case Category.MISSION:
                    if (linkedType == Category.STRATEGY)
                    {
                        bool warn = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.STRATEGY || trgt.category == Category.STRATEGY)
                            {
                                warn = false;
                            }
                        }
                        if (warn)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_MISSION_STRATEGY);
                            warnings.Add(item);
                            added.Add(item);
                        }
                    }
                    break;
                case Category.STRATEGY:
                    if (linkedType == Category.TACTIC || linkedType == Category.GOAL || linkedType == Category.OBJECTIVE)
                    {
                        bool warnTactic = true;
                        bool warnResult = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.TACTIC || trgt.category == Category.TACTIC)
                            {
                                warnTactic = false;
                            }
                            else if (src.category == Category.GOAL || trgt.category == Category.GOAL || src.category == Category.OBJECTIVE || trgt.category == Category.OBJECTIVE)
                            {
                                warnResult = false;
                            }
                        }
                        if (warnTactic)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_STRATEGY_TACTIC);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (warnResult)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT);
                            warnings.Add(item);
                            added.Add(item);
                        }

                    }
                    break;
                case Category.TACTIC:
                    if (linkedType == Category.GOAL || linkedType == Category.OBJECTIVE)
                    {
                        bool warnResult = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.GOAL || trgt.category == Category.GOAL || src.category == Category.OBJECTIVE || trgt.category == Category.OBJECTIVE)
                            {
                                warnResult = false;
                            }
                        }
                        if (warnResult)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT);
                            warnings.Add(item);
                            added.Add(item);
                        }
                    }
                    break;
                case Category.BUSINESS_POLICY:
                    if (linkedType == Category.BUSINESS_RULE || linkedType == Category.STRATEGY || linkedType == Category.TACTIC
                        || linkedType == Category.VISION || linkedType == Category.MISSION)
                    {
                        bool warnRule = true;
                        bool warnAction = true;
                        bool removeVisionMission = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.BUSINESS_RULE || trgt.category == Category.BUSINESS_RULE)
                            {
                                warnRule = false;
                            }
                            else if (src.category == Category.STRATEGY || trgt.category == Category.STRATEGY || src.category == Category.TACTIC || trgt.category == Category.TACTIC)
                            {
                                warnAction = false;
                            }
                            else if (src.category == Category.VISION || trgt.category == Category.VISION || src.category == Category.MISSION || trgt.category == Category.MISSION)
                            {
                                removeVisionMission = false;
                            }
                        }
                        if (warnRule)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_POLICY_RULE);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (warnAction)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (removeVisionMission)
                        {
                            var item = getWarning(WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                            warnings.Remove(item);
                            removed.Add(item);
                        }
                    }
                    break;
                case Category.BUSINESS_RULE:
                    if (linkedType == Category.STRATEGY || linkedType == Category.TACTIC || linkedType == Category.VISION || linkedType == Category.MISSION)
                    {
                        bool warnAction = true;
                        bool removeVisionMission = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.STRATEGY || trgt.category == Category.STRATEGY || src.category == Category.TACTIC || trgt.category == Category.TACTIC)
                            {
                                warnAction = false;
                            }
                            else if (src.category == Category.VISION || trgt.category == Category.VISION || src.category == Category.MISSION || trgt.category == Category.MISSION)
                            {
                                removeVisionMission = false;
                            }
                        }
                        if (warnAction)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (removeVisionMission)
                        {
                            var item = getWarning(WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                            warnings.Remove(item);
                            removed.Add(item);
                        }
                    }
                    break;
                case Category.INFLUENCER:
                    if (linkedType == Category.ASSESSMENT || linkedType == Category.BUSINESS_POLICY || linkedType == Category.BUSINESS_RULE)
                    {
                        bool warnAssessment = true;
                        bool removeAssDir = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (src.category == Category.ASSESSMENT || trgt.category == Category.ASSESSMENT)
                            {
                                removeAssDir = false;
                                warnAssessment = false;
                            }
                            else if (src.category != Category.BUSINESS_POLICY || trgt.category != Category.BUSINESS_POLICY
                                || src.category != Category.BUSINESS_RULE || trgt.category != Category.BUSINESS_RULE)
                            {
                                removeAssDir = false;
                            }
                        }
                        if (warnAssessment)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_INFLUENCER_ASSESSMENT);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (removeAssDir)
                        {
                            var item = getWarning(WarningItem.Codes.W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE);
                            warnings.Remove(item);
                            removed.Add(item);
                        }
                    }
                    break;
                case Category.ASSESSMENT:
                    if (linkedType == Category.VISION || linkedType == Category.GOAL || linkedType == Category.OBJECTIVE
                        || linkedType == Category.MISSION || linkedType == Category.STRATEGY || linkedType == Category.TACTIC || linkedType == Category.INFLUENCER)
                    {
                        bool warnEndsMeans = true;
                        bool warnInfluencer = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BMM)link.sourceModel;
                            var trgt = (BMM)link.targetModel;
                            if (linkedType == Category.VISION || linkedType == Category.GOAL || linkedType == Category.OBJECTIVE
                                || linkedType == Category.MISSION || linkedType == Category.STRATEGY || linkedType == Category.TACTIC)
                            {
                                warnEndsMeans = false;
                            }
                            else if (src.category == Category.INFLUENCER || trgt.category == Category.INFLUENCER)
                            {
                                warnInfluencer = false;
                            }
                        }
                        if (warnEndsMeans)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS);
                            warnings.Add(item);
                            added.Add(item);
                        }
                        if (warnInfluencer)
                        {
                            var item = new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_INFLUENCER);
                            warnings.Add(item);
                            added.Add(item);
                        }
                    }
                    break;
            }
            if (WarningsAddedEvent != null && added.Count > 0)
            {
                WarningsAddedEvent(this, added);
            }
            if (WarningsRemovedEvent != null && removed.Count > 0)
            {
                WarningsRemovedEvent(this, removed);
            }
        }

        public WarningItem getWarning(WarningItem.Codes code)
        {
            foreach (var warning in warnings)
            {
                if (warning.code == code)
                {
                    return warning;
                }
            }
            return null;
        }
        #endregion
        public void HighLight()
        {
            frame.Background = new SolidColorBrush(Colors.Gold);
        }

        public void LowLight()
        {
            frame.Background = new SolidColorBrush(Colors.LightBlue);
        }
        public override JsonObject serialize()
        {
            var controlEntry = base.serialize();
            controlEntry.Add("title", JsonValue.CreateStringValue(Title));
            if (Description != null)
            {
                controlEntry.Add("description", JsonValue.CreateStringValue(Description));
            }
            controlEntry.Add("state", JsonValue.CreateNumberValue(stateCombo.SelectedIndex));
            return controlEntry;
        }

        public static BMM deserialize(JsonObject input)
        {
            var control = BaseModel.deserialize(input);
            var title = input.GetNamedString("title", "");
            if (title.Length > 0)
            {
                ((BMM)control).Title = title;
            }
            var description = input.GetNamedString("description", "");
            if (description.Length > 0)
            {
                ((BMM)control).Description = description;
            }
            var state = input.GetNamedNumber("state", -1);
            if (state != -1)
            {
                ((BMM)control).stateCombo.SelectedIndex = (int)state;
            }
            return (BMM)control;
        }     
    }
}