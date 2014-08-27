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

namespace BPMM_App
{
    public class BPMMControl : BaseControl
    {
        public BPMM_Object.Type type;
        public BPMM_ViewModel viewModel;

        public BPMMControl(BPMM_Object obj)
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

            var stateCombo = new ComboBox();
            Binding statesBinding = new Binding() { Source = viewModel.States };
            Binding defaultBinding = new Binding() { Source = viewModel.DefaultState };
            stateCombo.SetBinding(ComboBox.ItemsSourceProperty, statesBinding);
            stateCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);

            Grid contentGrid = new Grid();

            if (obj is Influencer)
            {
                ComboBox influencerCombo = new ComboBox() { Margin = new Thickness(0, 0, 0, 2) };
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
            var targetType = ((BPMMControl)target).type;
            switch (type)
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
            switch (type)
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