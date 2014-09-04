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
using System.Collections.ObjectModel;
using Windows.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Data.Json;
using Windows.UI;

namespace BPMM_App
{
    public enum Model
    {
        VISION, GOAL, OBJECTIVE, MISSION, STRATEGY, TACTIC, BUSINESS_POLICY, BUSINESS_RULE, INFLUENCER, ASSESSMENT
    }

    public class BPMMControl : BaseControl, INotifyPropertyChanged
    {
        public static ObservableCollection<string> states = new ObservableCollection<string> { "created", "approved", "denied", "duplicate" };

        public Model type;
        string author;
        DateTime creationDate;
        public string title;
        public string description;
        List<string> references;
        string state;

        public ObservableCollection<WarningItem> warnings = new ObservableCollection<WarningItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ObservableCollection<WarningItem>> WarningsAddedEvent;
        public event EventHandler<ObservableCollection<WarningItem>> WarningsRemovedEvent;

        protected TextBox headerBox;
        protected TextBox descriptionBox;
        protected ComboBox stateCombo;
        

        public BPMMControl(Model newType) : base()
        {
            author = "Tieni";
            creationDate = DateTime.Now;
            States = states;
            state = States[0];
            type = newType;
            DataContext = this;

            headerBox = new TextBox() { AcceptsReturn = true, Margin = new Thickness(0, 0, 0, 2) };
            headerBox.DataContext = this;
            var headerBinding = new Binding() { Path = new PropertyPath("Title"), Mode = BindingMode.TwoWay };
            headerBox.SetBinding(TextBox.TextProperty, headerBinding);

            descriptionBox = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "description",
            };
            descriptionBox.DataContext = this;
            var descriptionBinding = new Binding() { Path = new PropertyPath("Description"), Mode = BindingMode.TwoWay };
            descriptionBox.SetBinding(TextBox.TextProperty, descriptionBinding);

            stateCombo = new ComboBox();
            Binding statesBinding = new Binding() { Source = States };
            Binding defaultBinding = new Binding() { Source = DefaultState };
            stateCombo.SetBinding(ComboBox.ItemsSourceProperty, statesBinding);
            stateCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);

            contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(6, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
            Grid.SetRow(headerBox, 0);
            Grid.SetRow(descriptionBox, 1);
            Grid.SetRow(stateCombo, 2);
            contentGrid.Children.Add(headerBox);
            contentGrid.Children.Add(descriptionBox);
            contentGrid.Children.Add(stateCombo);

            setContent(contentGrid);
            
            switch (type)
            { // type dependent configuration
                case Model.VISION:
                    Title = "Vision";
                    break;
                case Model.GOAL:
                    Title = "Goal";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_GOAL_OBJECTIVE));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_GOAL_VISION));
                    break;
                case Model.OBJECTIVE:
                    Title = "Objective";
                    break;
                case Model.MISSION:
                    Title = "Mission";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_MISSION_STRATEGY));
                    break;
                case Model.STRATEGY:
                    Title = "Strategy";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_STRATEGY_TACTIC));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT));
                    break;
                case Model.TACTIC:
                    Title = "Tactic";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ACTION_RESULT));
                    break;
                case Model.BUSINESS_POLICY:
                    Title = "Business Policy";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_POLICY_RULE));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION));
                    break;
                case Model.BUSINESS_RULE:
                    Title = "Business Rule";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_DIRECTIVE_ACTION));
                    break;
                case Model.INFLUENCER:
                    Title = "Influencer";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_INFLUENCER_ASSESSMENT));
                    break;
                case Model.ASSESSMENT:
                    Title = "Assessment";
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS));
                    warnings.Add(new WarningItem(this, WarningItem.Codes.M_ASSESSMENT_INFLUENCER));
                    break;
            }
            if (WarningsAddedEvent != null)
            {
                WarningsAddedEvent(this, warnings);
            }
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

        public string DefaultState
        {
            get { return states[0]; }
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

        #region validation

        public void validateNewLink(Model linkedType)
        {
            var added = new ObservableCollection<WarningItem>();
            var removed = new ObservableCollection<WarningItem>();
            switch (type)
            { // check if an incorrect association is added or if a missing association is restored.
                case Model.VISION:
                    if (linkedType != Model.MISSION && linkedType != Model.GOAL && linkedType != Model.ASSESSMENT)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Model.GOAL:
                    if (linkedType == Model.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_GOAL_OBJECTIVE);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Model.VISION)
                    {
                        var item = getWarning(WarningItem.Codes.M_GOAL_VISION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Model.OBJECTIVE:
                    break;
                case Model.MISSION:
                    if (linkedType == Model.STRATEGY)
                    {
                        var item = getWarning(WarningItem.Codes.M_MISSION_STRATEGY);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Model.STRATEGY:
                    if (linkedType == Model.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_STRATEGY_TACTIC);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Model.GOAL || linkedType == Model.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_ACTION_RESULT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Model.TACTIC:
                    if (linkedType == Model.GOAL || linkedType == Model.OBJECTIVE)
                    {
                        var item = getWarning(WarningItem.Codes.M_ACTION_RESULT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    break;
                case Model.BUSINESS_POLICY:
                    if (linkedType == Model.BUSINESS_RULE)
                    {
                        var item = getWarning(WarningItem.Codes.M_POLICY_RULE);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Model.STRATEGY || linkedType == Model.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_DIRECTIVE_ACTION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Model.VISION || linkedType == Model.MISSION)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Model.BUSINESS_RULE:
                    if (linkedType == Model.STRATEGY || linkedType == Model.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_DIRECTIVE_ACTION);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    if (linkedType == Model.VISION || linkedType == Model.MISSION)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_DIRECTIVE_VISION_MISSION);
                        warnings.Add(item);
                        added.Add(item);
                    }
                    break;
                case Model.INFLUENCER:
                    if (linkedType == Model.ASSESSMENT)
                    {
                        var item = getWarning(WarningItem.Codes.M_INFLUENCER_ASSESSMENT);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    else if (linkedType != Model.BUSINESS_POLICY && linkedType != Model.BUSINESS_RULE)
                    {
                        var item = new WarningItem(this, WarningItem.Codes.W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE);
                        warnings.Add(item);
                        added.Add(item);

                    }
                    break;
                case Model.ASSESSMENT:
                    if (linkedType == Model.VISION || linkedType == Model.GOAL || linkedType == Model.OBJECTIVE
                        || linkedType == Model.MISSION || linkedType == Model.STRATEGY || linkedType == Model.TACTIC)
                    {
                        var item = getWarning(WarningItem.Codes.M_ASSESSMENT_ENDS_MEANS);
                        warnings.Remove(item);
                        removed.Add(item);
                    }
                    else if (linkedType == Model.INFLUENCER)
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
        public void validateRemovedLink(Model linkedType, List<AssociationControl> remainingLinks)
        {
            var removed = new ObservableCollection<WarningItem>();
            var added = new ObservableCollection<WarningItem>();
            switch (type)
            { // check if an incorrect association has been removed or if a needed one is destroyed.
                case Model.VISION:
                    if(getWarning(WarningItem.Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT) != null) {
                        bool removeWarning = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if(src.type != Model.MISSION && src.type != Model.GOAL && src.type != Model.ASSESSMENT
                                && trgt.type != Model.MISSION && trgt.type != Model.GOAL && trgt.type != Model.ASSESSMENT)
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
                case Model.GOAL:
                    if (linkedType == Model.OBJECTIVE || linkedType == Model.VISION)
                    {
                        bool warnObjective = true;
                        bool warnVision = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.OBJECTIVE || trgt.type == Model.OBJECTIVE)
                            {
                                warnObjective = false;
                            }
                            else if (src.type == Model.VISION || trgt.type == Model.VISION)
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
                case Model.OBJECTIVE:
                    break;
                case Model.MISSION:
                    if (linkedType == Model.STRATEGY)
                    {
                        bool warn = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.STRATEGY || trgt.type == Model.STRATEGY)
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
                case Model.STRATEGY:
                    if (linkedType == Model.TACTIC || linkedType == Model.GOAL || linkedType == Model.OBJECTIVE)
                    {
                        bool warnTactic = true;
                        bool warnResult = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.TACTIC || trgt.type == Model.TACTIC)
                            {
                                warnTactic = false;
                            }
                            else if (src.type == Model.GOAL || trgt.type == Model.GOAL || src.type == Model.OBJECTIVE || trgt.type == Model.OBJECTIVE)
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
                case Model.TACTIC:
                    if (linkedType == Model.GOAL || linkedType == Model.OBJECTIVE)
                    {
                        bool warnResult = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.GOAL || trgt.type == Model.GOAL || src.type == Model.OBJECTIVE || trgt.type == Model.OBJECTIVE)
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
                case Model.BUSINESS_POLICY:
                    if (linkedType == Model.BUSINESS_RULE || linkedType == Model.STRATEGY || linkedType == Model.TACTIC
                        || linkedType == Model.VISION || linkedType == Model.MISSION)
                    {
                        bool warnRule = true;
                        bool warnAction = true;
                        bool removeVisionMission = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.BUSINESS_RULE || trgt.type == Model.BUSINESS_RULE)
                            {
                                warnRule = false;
                            }
                            else if (src.type == Model.STRATEGY || trgt.type == Model.STRATEGY || src.type == Model.TACTIC || trgt.type == Model.TACTIC)
                            {
                                warnAction = false;
                            }
                            else if (src.type == Model.VISION || trgt.type == Model.VISION || src.type == Model.MISSION || trgt.type == Model.MISSION)
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
                case Model.BUSINESS_RULE:
                    if (linkedType == Model.STRATEGY || linkedType == Model.TACTIC || linkedType == Model.VISION || linkedType == Model.MISSION)
                    {
                        bool warnAction = true;
                        bool removeVisionMission = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.STRATEGY || trgt.type == Model.STRATEGY || src.type == Model.TACTIC || trgt.type == Model.TACTIC)
                            {
                                warnAction = false;
                            }
                            else if (src.type == Model.VISION || trgt.type == Model.VISION || src.type == Model.MISSION || trgt.type == Model.MISSION)
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
                case Model.INFLUENCER:
                    if (linkedType == Model.ASSESSMENT || linkedType == Model.BUSINESS_POLICY || linkedType == Model.BUSINESS_RULE)
                    {
                        bool warnAssessment = true;
                        bool removeAssDir = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (src.type == Model.ASSESSMENT || trgt.type == Model.ASSESSMENT)
                            {
                                removeAssDir = false;
                                warnAssessment = false;
                            }
                            else if (src.type != Model.BUSINESS_POLICY || trgt.type != Model.BUSINESS_POLICY
                                || src.type != Model.BUSINESS_RULE || trgt.type != Model.BUSINESS_RULE)
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
                case Model.ASSESSMENT:
                    if (linkedType == Model.VISION || linkedType == Model.GOAL || linkedType == Model.OBJECTIVE
                        || linkedType == Model.MISSION || linkedType == Model.STRATEGY || linkedType == Model.TACTIC || linkedType == Model.INFLUENCER)
                    {
                        bool warnEndsMeans = true;
                        bool warnInfluencer = true;
                        foreach (var link in remainingLinks)
                        {
                            var src = (BPMMControl)link.source;
                            var trgt = (BPMMControl)link.target;
                            if (linkedType == Model.VISION || linkedType == Model.GOAL || linkedType == Model.OBJECTIVE
                                || linkedType == Model.MISSION || linkedType == Model.STRATEGY || linkedType == Model.TACTIC)
                            {
                                warnEndsMeans = false;
                            }
                            else if (src.type == Model.INFLUENCER || trgt.type == Model.INFLUENCER)
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
            controlEntry.Add("type", JsonValue.CreateNumberValue((int)type));
            controlEntry.Add("state", JsonValue.CreateNumberValue(stateCombo.SelectedIndex));
            return controlEntry;
        }

        public static BPMMControl deserialize(JsonObject input)
        {
            var value = input.GetNamedNumber("type", -1);
            if (value == -1)
            {
                return null;
            }
            Model newType;
            try
            {
                newType = (Model)value;
            }
            catch (InvalidCastException)
            {
                return null;
            }
            var control =
                (newType == Model.BUSINESS_RULE) ? (BaseControl)new BusinessRuleControl(newType) :
                (newType == Model.INFLUENCER) ? (BaseControl)new InfluencerControl(newType) :
                (newType == Model.ASSESSMENT) ? (BaseControl)new AssessmentControl(newType) : 
                new BPMMControl(newType);
            BaseControl.deserialize(ref control, input);
            var title = input.GetNamedString("title", "");
            if (title.Length > 0)
            {
                ((BPMMControl)control).Title = title;
            }
            var description = input.GetNamedString("description", "");
            if (description.Length > 0)
            {
                ((BPMMControl)control).Description = description;
            }
            var state = input.GetNamedNumber("state", -1);
            if (state != -1)
            {
                ((BPMMControl)control).stateCombo.SelectedIndex = (int)state;
            }
            return (BPMMControl)control;
        }     
    }
}