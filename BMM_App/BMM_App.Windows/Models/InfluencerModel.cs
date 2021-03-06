﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace BMM_App
{
    class InfluencerModel : BMM
    {
        public ComboBox influencerCombo;
        private ObservableCollection<String> influencerTypes;
        public static List<string> externalInfluencers =
        new List<String> { "Competitor", "Customer", "Environment", "Partner", "Regulation", "Supplier", "Technology" };
        public static List<string> internalInfluencers =
            new List<String> { "Assumption", "Corporate Value, explicit", "Corporate Value, implicit", "Habit",
                                            "Infrastructure", "Issue", "Management Prerogative", "Resource" };
        public const String externalSep = "--- External: ---";
        public const String internalSep = "--- Internal: ---";

        public InfluencerModel(string author)
            : base(Category.INFLUENCER, author)
        {
            List<String> types = new List<String>();
            types.Add("Influence Type");
            types.Add(externalSep);
            types.AddRange(externalInfluencers);
            types.Add(internalSep);
            types.AddRange(internalInfluencers);
            InfluencerTypes = new ObservableCollection<String>(types);

            influencerCombo = new ComboBox();
            influencerCombo.DataContext = this;
            var influencerBinding = new Binding() { Source = InfluencerTypes };
            var defaultBinding = new Binding() { Source = DefaultInfluencer };
            influencerCombo.SetBinding(ComboBox.ItemsSourceProperty, influencerBinding);
            influencerCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            influencerCombo.SelectionChanged += influencerCombo_SelectionChanged;
            frame.RowDefinitions.Clear();
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(36, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            Grid.SetRow(headerBorder, 0);
            Grid.SetRow(influencerCombo, 1);
            Grid.SetRow(descriptionBorder, 2);
            Grid.SetRow(stateCombo, 3);
            frame.Children.Add(influencerCombo);
        }

        public override void UpdateFontSize(double scale)
        {
            influencerCombo.FontSize = influencerCombo.FontSize * scale;
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

        public static new InfluencerModel deserialize(JsonObject input)
        {
            var model = (InfluencerModel)BMM.deserialize(input);
            var influencer = input.GetNamedNumber("influencer", -1);
            if (influencer != -1)
            {
                model.influencerCombo.SelectedIndex = (int)influencer;
            }

            return model;
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
