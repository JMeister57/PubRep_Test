using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace BPMM_App
{
    class InfluencerControl : BPMMControl
    {
        private ComboBox influencerCombo;
        private ObservableCollection<String> influencerTypes; 
        public const String externalSep = "--- External: ---";
        public const String internalSep = "--- Internal: ---";

        public InfluencerControl(Influencer obj) : base(obj)
        {
            List<String> types = new List<String>();
            types.Add("Influence Type");
            types.Add(externalSep);
            types.AddRange(Influencer.externalInfluencers);
            types.Add(internalSep);
            types.AddRange(Influencer.internalInfluencers);
            InfluencerTypes = new ObservableCollection<String>(types);

            influencerCombo = new ComboBox();
            influencerCombo.DataContext = this;
            var influencerBinding = new Binding() { Source = InfluencerTypes };
            var defaultBinding = new Binding() { Source = DefaultInfluencer };
            influencerCombo.SetBinding(ComboBox.ItemsSourceProperty, influencerBinding);
            influencerCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            influencerCombo.SelectionChanged += influencerCombo_SelectionChanged;
            contentGrid.RowDefinitions.Clear();
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.8, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5.3, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            Grid.SetRow(headerBox, 0);
            Grid.SetRow(influencerCombo, 1);
            Grid.SetRow(descriptionBox, 2);
            Grid.SetRow(stateCombo, 3);
            contentGrid.Children.Add(influencerCombo);
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

        public string DefaultInfluencer
        {
            get { return influencerTypes[0]; }
        }

        public override JsonObject serialize()
        {
            var controlEntry = base.serialize();
            controlEntry.Add("influencer", JsonValue.CreateNumberValue(influencerCombo.SelectedIndex));
            return controlEntry;
        }

        public static new InfluencerControl deserialize(JsonObject input)
        {
            var control = (InfluencerControl)BPMMControl.deserialize(input);
            var influencer = input.GetNamedNumber("influencer", -1);
            if (influencer != -1)
            {
                control.influencerCombo.SelectedIndex = (int)influencer;
            }
            return control;
        }

        private void influencerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == internalSep
                || e.AddedItems[0].ToString() == externalSep)
            {
                if (e.RemovedItems.Count == 0)
                {
                    ((ComboBox)sender).SelectedIndex = -1;
                    return;
                }
                ((ComboBox)sender).SelectedItem = e.RemovedItems[0];
            }
        }
    }
}
