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

namespace BPMM_App
{
    public partial class BPMMControl : UserControl
    {
        private const int MIN_SIZE = 100;
        
        protected bool isDragging;
        private PointerPoint offset;

        ComboBox influencerCombo;
        public BPMM_Object.Type type;
        public BPMM_ViewModel viewModel;
        private bool resizing;

        public event PointerEventHandler AssociationEvent;
        public event PointerEventHandler MovedEvent;
        public BPMMControl()
        {
            this.InitializeComponent();
        }
        
        public BPMMControl(BPMM_Object obj)
        {
            this.InitializeComponent();
            resizing = false;
            viewModel = new BPMM_ViewModel(obj);
            DataContext = viewModel;
            
            if (obj is Influencer)
            { // add row for the influence type combobox
                influencerCombo = new ComboBox();
                influencerCombo.DataContext = viewModel;
                Binding items = new Binding() { Source = viewModel.InfluencerTypes };
                BindingOperations.SetBinding(influencerCombo, ComboBox.ItemsSourceProperty, items);
                influencerCombo.SelectionChanged += influencerCombo_SelectionChanged;
                influencerCombo.Margin = new Thickness(10, 0, 10, 2);

                bpmmObject.Height = 240;
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1.4, GridUnitType.Star);
                bpmmObject.RowDefinitions.Insert(2, row);
                Grid.SetRow(header, 1);
                Grid.SetRow(influencerCombo, 2);
                Grid.SetRow(description, 3);
                Grid.SetRow(stateCombo, 4);
                Grid.SetRow(ThumbBottomLeft, 5);
                Grid.SetRow(ThumbBottomRight, 5);
                bpmmObject.Children.Add(influencerCombo);
                bpmmObject.RowDefinitions[1].Height = new GridLength(1.8, GridUnitType.Star);
                bpmmObject.RowDefinitions[3].Height = new GridLength(5.3, GridUnitType.Star);
                bpmmObject.RowDefinitions[4].Height = new GridLength(1.4, GridUnitType.Star);
            }
        }

        #region linking
        public bool linkableWith(BPMMControl control)
        {
            switch (type)
            {
                case BPMM_Object.Type.VISION:
                    return control.type == BPMM_Object.Type.GOAL || control.type == BPMM_Object.Type.MISSION
                        || control.type == BPMM_Object.Type.INFLUENCER;
                case BPMM_Object.Type.GOAL:
                    return control.type == BPMM_Object.Type.MISSION || control.type == BPMM_Object.Type.OBJECTIVE
                        || control.type == BPMM_Object.Type.GOAL || control.type == BPMM_Object.Type.ASSESSMENT
                        || control.type == BPMM_Object.Type.STRATEGY;
                case BPMM_Object.Type.OBJECTIVE:
                    return control.type == BPMM_Object.Type.GOAL || control.type == BPMM_Object.Type.OBJECTIVE
                        || control.type == BPMM_Object.Type.ASSESSMENT || control.type == BPMM_Object.Type.TACTIC;
                case BPMM_Object.Type.MISSION:
                    return control.type == BPMM_Object.Type.STRATEGY || control.type == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.STRATEGY:
                    return control.type == BPMM_Object.Type.MISSION || control.type == BPMM_Object.Type.TACTIC
                        || control.type == BPMM_Object.Type.STRATEGY || control.type == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.TACTIC:
                    return control.type == BPMM_Object.Type.STRATEGY || control.type == BPMM_Object.Type.TACTIC
                        || control.type == BPMM_Object.Type.BUSINESS_RULE || control.type == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.BUSINESS_POLICY:
                    return control.type == BPMM_Object.Type.BUSINESS_POLICY || control.type == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.BUSINESS_RULE:
                    return control.type == BPMM_Object.Type.TACTIC || control.type == BPMM_Object.Type.BUSINESS_POLICY;
                case BPMM_Object.Type.INFLUENCER:
                    return control.type == BPMM_Object.Type.ASSESSMENT;
                case BPMM_Object.Type.ASSESSMENT:
                    return true;
                default:
                    return false;
            }
        }

        public bool linkWith(BPMMControl control)
        {
            switch (type)
            {
                case BPMM_Object.Type.VISION:
                    ((Vision)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.GOAL:
                    ((Goal)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.OBJECTIVE:
                    ((Objective)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.MISSION:
                    ((Mission)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.STRATEGY:
                    ((Strategy)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.TACTIC:
                    ((Tactic)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.BUSINESS_POLICY:
                    ((BusinessPolicy)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.BUSINESS_RULE:
                    ((BusinessRule)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.INFLUENCER:
                    ((Influencer)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
                    return true;
                case BPMM_Object.Type.ASSESSMENT:
                    ((Assessment)viewModel.linkedObject).linkWith(control.viewModel.linkedObject);
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
                    influencerCombo.SelectedIndex = -1;
                    return;
                }
                influencerCombo.SelectedItem = e.RemovedItems[0];
            }
        }

        // drag
        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isDragging = true;
            offset = e.GetCurrentPoint(this);
            bpmmObject.CapturePointer(e.Pointer);
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
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                bpmmObject.ReleasePointerCapture(e.Pointer);
            }
        }

        #region resize
        private void ThumbTopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width -= xChange;
            bpmmObject.Height -= yChange;
            Canvas.SetLeft(bpmmObject, Canvas.GetLeft(bpmmObject) + xChange);
            Canvas.SetTop(bpmmObject, Canvas.GetTop(bpmmObject) + yChange);
        }

        private void ThumbTopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height - e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width += xChange;
            bpmmObject.Height -= yChange;
            Canvas.SetTop(bpmmObject, Canvas.GetTop(bpmmObject) + yChange);
        }

        private void ThumbBottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width - e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width -= xChange;
            bpmmObject.Height += yChange;
            Canvas.SetLeft(bpmmObject, Canvas.GetLeft(bpmmObject) + xChange);
        }

        private void ThumbBottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            resizing = true;
            double xChange = bpmmObject.Width + e.HorizontalChange > MIN_SIZE ? e.HorizontalChange : 0;
            double yChange = bpmmObject.Height + e.VerticalChange > MIN_SIZE ? e.VerticalChange : 0;
            bpmmObject.Width += xChange;
            bpmmObject.Height += yChange;
        }


        private void anchor_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (AssociationEvent != null)
            {
                AssociationEvent(this, e);
            }
        }

        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            resizing = false;
        }
        # endregion

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
