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
    public class BPMMControl : BaseControl, INotifyPropertyChanged
    {
        public enum Type
        {
            VISION, GOAL, OBJECTIVE, MISSION, STRATEGY, TACTIC, BUSINESS_POLICY, BUSINESS_RULE, INFLUENCER, ASSESSMENT
        }
        public static ObservableCollection<string> states = new ObservableCollection<string> { "created", "approved", "denied", "duplicate" };

        public Type type;
        string author;
        DateTime creationDate;
        public string title;
        public string description;
        List<string> references;
        string state;

        public event PropertyChangedEventHandler PropertyChanged;

        protected Grid contentGrid;
        protected TextBox headerBox;
        protected TextBox descriptionBox;
        protected ComboBox stateCombo;
        

        public BPMMControl(Type newType) : base()
        {
            author = "Tieni";
            creationDate = DateTime.Now;
            States = states;
            state = States[0];
            type = newType;
            title =
                (type == BPMMControl.Type.VISION) ? "Vision" :
                (type == BPMMControl.Type.GOAL) ? "Goal" :
                (type == BPMMControl.Type.OBJECTIVE) ? "Objective" : 
                (type == BPMMControl.Type.MISSION) ? "Mission" :
                (type == BPMMControl.Type.STRATEGY) ? "Strategy" :
                (type == BPMMControl.Type.TACTIC) ? "Tactic" :
                (type == BPMMControl.Type.BUSINESS_POLICY) ? "Business Policy" :
                (type == BPMMControl.Type.BUSINESS_RULE) ? "BUsiness Rule" :
                (type == BPMMControl.Type.INFLUENCER) ? "Influencer" :
                "Assessment";
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

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

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
            Type newType;
            try
            {
                newType = (Type)value;
            }
            catch (InvalidCastException)
            {
                return null;
            }
            var control =
                (newType == Type.BUSINESS_RULE) ? (BaseControl)new BusinessRuleControl(newType) :
                (newType == Type.INFLUENCER) ? (BaseControl)new InfluencerControl(newType) :
                (newType == Type.ASSESSMENT) ? (BaseControl)new AssessmentControl(newType) : 
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