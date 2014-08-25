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

namespace BPMM_App
{
    public partial class BPMMControl : UserControl
    {
        private const int MIN_SIZE = 100;

        public BPMM_Object.Type type;
        public BPMM_ViewModel viewModel;
        private bool resizing;

        public event PointerEventHandler AssociationEvent;
        public event ManipulationDeltaEventHandler MovedEvent;
        public BPMMControl()
        {
            this.InitializeComponent();
        }
        
        public BPMMControl(BPMM_Object obj)
        {
            this.InitializeComponent();
            resizing = false;
            viewModel = new BPMM_ViewModel(obj);
            DataContext = this.viewModel;
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

        // drag
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (resizing)
            {
                return;
            }
            if (e.IsInertial) // disable inertia, we don't want any flicking
            {
                e.Complete();
                e.Handled = true;
            }

            var ct = (CompositeTransform)container.RenderTransform;
            ct.TranslateX += e.Delta.Translation.X; 
            ct.TranslateY += e.Delta.Translation.Y;
            if (MovedEvent != null)
            {
                MovedEvent(this, e);
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

            public event PropertyChangedEventHandler PropertyChanged;

            public BPMM_ViewModel(BPMM_Object obj)
            {
                linkedObject = obj;
                States = new ObservableCollection<String>(BPMM_Object.States);
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
