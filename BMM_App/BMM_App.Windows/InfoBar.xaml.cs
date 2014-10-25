using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace BMM_App
{
    public sealed partial class InfoBar : UserControl, INotifyPropertyChanged
    {
        private double contentHeight = 100;

        private ObservableCollection<WarningItem> warnings = new ObservableCollection<WarningItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public InfoBar()
        {
            this.InitializeComponent();
            warnings_listview.DataContext = this;
        }

        public ObservableCollection<WarningItem> Warnings
        {
            get { return warnings; }
            set { warnings = value; OnPropertyChanged("Warnings"); }
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void BMMTapped(object sender, TappedRoutedEventArgs e)
        {
            var bmm = sender as BMM;
            DataContext = bmm;
            var nameBinding = new Binding() { Path = new PropertyPath("Title"), Mode = BindingMode.TwoWay };
            nameBox.SetBinding(TextBox.TextProperty, nameBinding);
            var descriptionBinding = new Binding() { Path = new PropertyPath("Description"), Mode = BindingMode.TwoWay };
            descriptionBox.SetBinding(TextBox.TextProperty, descriptionBinding);
            Binding statesBinding = new Binding() { Source = bmm.States, Mode = BindingMode.TwoWay };
            stateCombo.SetBinding(ComboBox.ItemsSourceProperty, statesBinding);
            Binding selectedStateBinding = new Binding() { Path = new PropertyPath("State"), Mode = BindingMode.TwoWay };
            stateCombo.SetBinding(ComboBox.SelectedItemProperty, selectedStateBinding);
            var referencesBinding = new Binding() { Path = new PropertyPath("References"), Mode = BindingMode.TwoWay };
            referencesBox.SetBinding(TextBox.TextProperty, referencesBinding);
            var authorBinding = new Binding() { Path = new PropertyPath("Author")};
            authorBlock.SetBinding(TextBlock.TextProperty, authorBinding);
            var dateBinding = new Binding() { Path = new PropertyPath("CreationDate")};
            dateBlock.SetBinding(TextBlock.TextProperty, dateBinding);
        }

        public void AddWarnings(object sender, ObservableCollection<WarningItem> added)
        {
            foreach (var item in added)
            {
                if (Warnings.Contains(item) == false)
                { // TODO: filter out duplicate warnings at validation time
                    Warnings.Add(item);
                }
            }
        }

        public void RemoveWarnings(object sender, ObservableCollection<WarningItem> removed)
        {
            foreach (var item in removed)
            {
                if (Warnings.Contains(item))
                {
                    Warnings.Remove(item);
                }
            }
        }

        private void InfoBar_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var contentRow = frame.RowDefinitions[1];
            var newHeight = Math.Min(Math.Max(contentRow.ActualHeight - e.Delta.Translation.Y, 0), 800);
            contentRow.Height = new GridLength(newHeight, GridUnitType.Pixel);
            contentHeight = newHeight;
        }

        private void minimize_button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (frame.RowDefinitions[1].ActualHeight != 0)
            {
                frame.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                frame.RowDefinitions[1].Height = new GridLength(contentHeight, GridUnitType.Pixel);
            }
        }

        // warnings tab
        private void warnings_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selected = (WarningItem)e.AddedItems[0];
                if (selected != null)
                {
                    selected.model.HighLight();
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                var released = (WarningItem)e.RemovedItems[0];
                if (released != null)
                {
                    released.model.LowLight();
                }
            }
        }

        private void InfoBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender == warnings_tab)
            {
                warnings_listview.Visibility = Visibility.Visible;
                details_listview.Visibility = Visibility.Collapsed;
            }
            else if(sender == details_tab)
            {
                warnings_listview.Visibility = Visibility.Collapsed;
                details_listview.Visibility = Visibility.Visible;
            }
        }
    }
}
