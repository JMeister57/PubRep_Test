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
using Windows.UI.Xaml.Shapes;

namespace BPMM_App
{
    public partial class InfluencerControl : UserControl
    {
        private const int MIN_SIZE = 100;

        public Influencer_ViewModel viewModel;
        private bool resizing;

        public event PointerEventHandler AssociationEvent;

        public InfluencerControl()
        {
            this.InitializeComponent();
        }
        public InfluencerControl(Influencer obj)
        {
            this.InitializeComponent();
            resizing = false;
            viewModel = new Influencer_ViewModel(obj);
            DataContext = this.viewModel;
        }

        #region linking
        public bool linkableWith(BPMMControl control)
        {
            return control.type == BPMM_Object.Type.ASSESSMENT;
        }

        public bool linkWith(BPMMControl control)
        {
            viewModel.linkedInfluencer.linkWith(control.viewModel.linkedObject);
            return true;
        }
        #endregion


        private void typeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == Influencer_ViewModel.internalSep
                || e.AddedItems[0].ToString() == Influencer_ViewModel.externalSep)
            {
                if (e.RemovedItems.Count == 0)
                {
                    typeCombo.SelectedIndex = -1;
                    return;
                }
                typeCombo.SelectedItem = e.RemovedItems[0];
            }
        }

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
        public class Influencer_ViewModel : INotifyPropertyChanged
        {
            public Influencer linkedInfluencer;
            private ObservableCollection<String> states;
            private ObservableCollection<String> influencerTypes;
            public event PropertyChangedEventHandler PropertyChanged;
            public const String externalSep = "--- External: ---";
            public const String internalSep = "--- Internal: ---";

            public Influencer_ViewModel(Influencer obj)
            {
                linkedInfluencer = obj;
                States = new ObservableCollection<String>(BPMM_Object.States);
                List<String> types = new List<String>();
                types.Add(externalSep);
                types.AddRange(Influencer.externalInfluencers);
                types.Add(internalSep);
                types.AddRange(Influencer.internalInfluencers);
                Types = new ObservableCollection<String>(types);
            }

            # region getters/setters
            public string Title
            {
                get { return linkedInfluencer.title; }
                set
                {
                    linkedInfluencer.title = value;
                    OnPropertyChanged("Title");
                }
            }

            public ObservableCollection<String> Types
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
                get { return linkedInfluencer.description; }
                set
                {
                    linkedInfluencer.description = value;
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
