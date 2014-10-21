using System;
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
    public class BusinessRuleModel : BMM
    {
        public ComboBox enforcementCombo;
        private static ObservableCollection<string> enforcementLevels =
            new ObservableCollection<string> { "<Enforcement Lvl>", "Strictly", "Deferred", "Pre-auth Override", "Post-justified Override", "Explained Override", "Guideline" };

        public BusinessRuleModel()
            : base(Category.BUSINESS_RULE)
        {
            enforcementCombo = new ComboBox();
            enforcementCombo.DataContext = this;
            var levelBinding = new Binding() { Source = EnforcementLevels };
            var defaultBinding = new Binding() { Source = DefaultLevel };
            enforcementCombo.SetBinding(ComboBox.ItemsSourceProperty, levelBinding);
            enforcementCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            frame.RowDefinitions.Clear();
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(36, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            Grid.SetRow(headerBorder, 0);
            Grid.SetRow(enforcementCombo, 1);
            Grid.SetRow(descriptionBorder, 2);
            Grid.SetRow(stateCombo, 3);
            frame.Children.Add(enforcementCombo);
        }

        public override void UpdateFontSize(double scale)
        {
            enforcementCombo.FontSize = enforcementCombo.FontSize * scale;
        }

        public ObservableCollection<string> EnforcementLevels
        {
            get { return enforcementLevels; }
            set 
            {
                enforcementLevels = value;
                OnPropertyChanged("EnforcementLevels");
            }
        }

        public string DefaultLevel
        {
            get { return enforcementLevels[0]; }
        }

        public override JsonObject serialize()
        {
            var controlEntry = base.serialize();
            controlEntry.Add("enforcement-level", JsonValue.CreateNumberValue(enforcementCombo.SelectedIndex));
            return controlEntry;
        }

        public static new BusinessRuleModel deserialize(JsonObject input)
        {
            var model = (BusinessRuleModel)BMM.deserialize(input);
            var level = input.GetNamedNumber("enforcement-level", -1);
            if (level != -1)
            {
                model.enforcementCombo.SelectedIndex = (int)level;
            }
            return model;
        }
    }
}
