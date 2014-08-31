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

namespace BPMM_App
{
    public class BPMMControl : BaseControl
    {
        public BPMM_ViewModel viewModel;

        private ComboBox stateCombo;
        private ComboBox influencerCombo;

        public BPMMControl(BPMM_Object obj) : base()
        {
            viewModel = new BPMM_ViewModel(obj);
            DataContext = viewModel;


            var headerBox = new TextBox() { AcceptsReturn = true, Margin = new Thickness(0, 0, 0, 2) };
            headerBox.DataContext = viewModel;
            var headerBinding = new Binding() { Path = new PropertyPath("Title"), Mode = BindingMode.TwoWay };
            headerBox.SetBinding(TextBox.TextProperty, headerBinding);

            var descriptionBox = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "description",
            };
            descriptionBox.DataContext = viewModel;
            var descriptionBinding = new Binding() { Path = new PropertyPath("Description"), Mode = BindingMode.TwoWay };
            descriptionBox.SetBinding(TextBox.TextProperty, descriptionBinding);

            stateCombo = new ComboBox();
            Binding statesBinding = new Binding() { Source = viewModel.States };
            Binding defaultBinding = new Binding() { Source = viewModel.DefaultState };
            stateCombo.SetBinding(ComboBox.ItemsSourceProperty, statesBinding);
            stateCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);

            Grid contentGrid = new Grid();

            if (obj is Influencer)
            {
                influencerCombo = new ComboBox() { Margin = new Thickness(0, 0, 0, 2) };
                var influencerBinding = new Binding() { Source = viewModel.InfluencerTypes };
                influencerCombo.SetBinding(ComboBox.ItemsSourceProperty, influencerBinding);
                influencerCombo.SelectionChanged += influencerCombo_SelectionChanged;

                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.8, GridUnitType.Star) });
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5.3, GridUnitType.Star) });
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
                Grid.SetRow(headerBox, 0);
                Grid.SetRow(influencerCombo, 1);
                Grid.SetRow(descriptionBox, 2);
                Grid.SetRow(stateCombo, 3);
                contentGrid.Children.Add(headerBox);
                contentGrid.Children.Add(influencerCombo);
                contentGrid.Children.Add(descriptionBox);
                contentGrid.Children.Add(stateCombo);
            }
            else
            {
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(6, GridUnitType.Star) });
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });
                Grid.SetRow(headerBox, 0);
                Grid.SetRow(descriptionBox, 1);
                Grid.SetRow(stateCombo, 2);
                contentGrid.Children.Add(headerBox);
                contentGrid.Children.Add(descriptionBox);
                contentGrid.Children.Add(stateCombo);
            }

            setContent(contentGrid);
        }

        #region linking
        public override bool linkableWith(BaseControl target)
        {
            if (target is BPMMControl == false)
            {
                return false;
            }
            var targetType = ((BPMMControl)target).type();
            switch (type())
            {
                case BPMM_Object.Type.VISION:
                    return targetType == BPMM_Object.Type.GOAL || targetType == BPMM_Object.Type.MISSION
                        || targetType == BPMM_Object.Type.INFLUENCER;
                case BPMM_Object.Type.GOAL:
                    return targetType == BPMM_Object.Type.MISSION || targetType == BPMM_Object.Type.OBJECTIVE
                        || targetType == BPMM_Object.Type.GOAL || targetType == BPMM_Object.Type.ASSESSMENT
                        || targetType == BPMM_Object.Type.STRATEGY;
                case BPMM_Object.Type.OBJECTIVE:
                    return targetType == BPMM_Object.Type.GOAL || targetType == BPMM_Object.Type.OBJECTIVE
                        || targetType == BPMM_Object.Type.ASSESSMENT || targetType == BPMM_Object.Type.TACTIC;
                case BPMM_Object.Type.MISSION:
                    return targetType == BPMM_Object.Type.STRATEGY || targetType == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.STRATEGY:
                    return targetType == BPMM_Object.Type.MISSION || targetType == BPMM_Object.Type.TACTIC
                        || targetType == BPMM_Object.Type.STRATEGY || targetType == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.TACTIC:
                    return targetType == BPMM_Object.Type.STRATEGY || targetType == BPMM_Object.Type.TACTIC
                        || targetType == BPMM_Object.Type.BUSINESS_RULE || targetType == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.BUSINESS_POLICY:
                    return targetType == BPMM_Object.Type.BUSINESS_POLICY || targetType == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.BUSINESS_RULE:
                    return targetType == BPMM_Object.Type.TACTIC || targetType == BPMM_Object.Type.BUSINESS_POLICY;
                case BPMM_Object.Type.INFLUENCER:
                    return targetType == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.ASSESSMENT:
                    return true;
                default:
                    return false;
            }
        }

        public override bool LinkWith(BaseControl target)
        {
            if (target is BPMMControl == false)
            {
                return false;
            }
            var linkedObject = ((BPMMControl)target).viewModel.linkedObject;
            switch (type())
            {
                case BPMM_Object.Type.VISION:
                    ((Vision)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.GOAL:
                    ((Goal)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.OBJECTIVE:
                    ((Objective)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.MISSION:
                    ((Mission)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.STRATEGY:
                    ((Strategy)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.TACTIC:
                    ((Tactic)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.BUSINESS_POLICY:
                    ((BusinessPolicy)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.BUSINESS_RULE:
                    ((BusinessRule)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.INFLUENCER:
                    ((Influencer)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                case BPMM_Object.Type.ASSESSMENT:
                    ((Assessment)viewModel.linkedObject).linkWith(linkedObject);
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        private void influencerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == BPMM_ViewModel.internalSep
                || e.AddedItems[0].ToString() == BPMM_ViewModel.externalSep)
            {
                if (e.RemovedItems.Count == 0)
                {
                    ((ComboBox)sender).SelectedIndex = -1;
                    return;
                }
                ((ComboBox)sender).SelectedItem = e.RemovedItems[0];
            }
        }

        public override JsonObject serialize()
        {
            var controlEntry = base.serialize();
            controlEntry.Add("title", JsonValue.CreateStringValue(viewModel.Title));
            if (viewModel.Description != null)
            {
                controlEntry.Add("description", JsonValue.CreateStringValue(viewModel.Description));
            }
            controlEntry.Add("type", JsonValue.CreateNumberValue((int)type()));
            controlEntry.Add("state", JsonValue.CreateNumberValue(stateCombo.SelectedIndex));
            if (viewModel.linkedObject is Influencer)
            {
                controlEntry.Add("influencer", JsonValue.CreateNumberValue(influencerCombo.SelectedIndex));
            }
            return controlEntry;
        }

        public static BPMMControl deserialize(JsonObject input)
        {
            try
            {
                var value = input.GetNamedNumber("type", -1);
                if (value == -1)
                {
                    return null;
                }
                var type = (BPMM_Object.Type)value;
                BPMM_Object obj =
                (type == BPMM_Object.Type.VISION) ? (BPMM_Object)new Vision() :
                (type == BPMM_Object.Type.GOAL) ? (BPMM_Object)new Goal() :
                (type == BPMM_Object.Type.OBJECTIVE) ? (BPMM_Object)new Objective() :
                (type == BPMM_Object.Type.MISSION) ? (BPMM_Object)new Mission() :
                (type == BPMM_Object.Type.STRATEGY) ? (BPMM_Object)new Strategy() :
                (type == BPMM_Object.Type.TACTIC) ? (BPMM_Object)new Tactic() :
                (type == BPMM_Object.Type.BUSINESS_POLICY) ? (BPMM_Object)new BusinessPolicy() :
                (type == BPMM_Object.Type.BUSINESS_RULE) ? (BPMM_Object)new BusinessRule() :
                (type == BPMM_Object.Type.INFLUENCER) ? (BPMM_Object)new Influencer() :
                (BPMM_Object)new Assessment();

                var control = new BPMMControl(obj);
                var title = input.GetNamedString("title", "");
                if (title.Length > 0)
                {
                    control.viewModel.Title = title;
                }
                var description = input.GetNamedString("description", "");
                if (description.Length > 0)
                {
                    control.viewModel.Description = description;
                }
                var state = input.GetNamedNumber("state", -1);
                if (state != -1)
                {
                    control.stateCombo.SelectedIndex = (int)state;
                }
                var influencer = input.GetNamedNumber("influencer", -1);
                if (influencer != -1)
                {
                    control.influencerCombo.SelectedIndex = (int)influencer;
                }
                Canvas.SetLeft(control, input.GetNamedNumber("x", 0));
                Canvas.SetTop(control, input.GetNamedNumber("y", 0));
                return control;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        public BPMM_Object.Type type()
        {
            if (viewModel.linkedObject is Vision)
            {
                return BPMM_Object.Type.VISION;
            }
            else if (viewModel.linkedObject is Goal)
            {
                return BPMM_Object.Type.GOAL;
            }
            else if (viewModel.linkedObject is Objective)
            {
                return BPMM_Object.Type.OBJECTIVE;
            }
            if (viewModel.linkedObject is Mission)
            {
                return BPMM_Object.Type.MISSION;
            }
            if (viewModel.linkedObject is Strategy)
            {
                return BPMM_Object.Type.STRATEGY;
            }
            if (viewModel.linkedObject is Tactic)
            {
                return BPMM_Object.Type.TACTIC;
            }
            if (viewModel.linkedObject is BusinessPolicy)
            {
                return BPMM_Object.Type.BUSINESS_POLICY;
            }
            if (viewModel.linkedObject is BusinessRule)
            {
                return BPMM_Object.Type.BUSINESS_RULE;
            }
            if (viewModel.linkedObject is Influencer)
            {
                return BPMM_Object.Type.INFLUENCER;
            }
            else
            {
                return BPMM_Object.Type.ASSESSMENT;
            }
        }
        #region viewmodel
        public class BPMM_ViewModel : INotifyPropertyChanged
        {
            public BPMM_Object linkedObject;
            private ObservableCollection<String> states;
            private ObservableCollection<String> influencerTypes;
            public const String externalSep = "--- External: ---";
            public const String internalSep = "--- Internal: ---";

            public event PropertyChangedEventHandler PropertyChanged;

            public BPMM_ViewModel(BPMM_Object obj)
            {
                linkedObject = obj;
                States = new ObservableCollection<String>(BPMM_Object.States);

                if (obj is Influencer)
                {
                    List<String> types = new List<String>();
                    types.Add(externalSep);
                    types.AddRange(Influencer.externalInfluencers);
                    types.Add(internalSep);
                    types.AddRange(Influencer.internalInfluencers);
                    InfluencerTypes = new ObservableCollection<String>(types);
                }
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

            public ObservableCollection<String> InfluencerTypes
            {
                get { return influencerTypes; }
                set
                {
                    influencerTypes = value;
                    OnPropertyChanged("InfluencerTypes");
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

            public ObservableCollection<String> States
            {
                get { return states; }
                set
                {
                    states = value;
                    OnPropertyChanged("States");
                }
            }
            public String DefaultState
            {
                get { return states[0]; }
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
        #endregion
    }
}