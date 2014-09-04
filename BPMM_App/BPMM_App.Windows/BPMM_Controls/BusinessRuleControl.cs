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

namespace BPMM_App
{
    public class BusinessRuleControl : BPMMControl
    {
        private ComboBox enforcementCombo;
        private static ObservableCollection<string> enforcementLevels =
            new ObservableCollection<string> { "<Enforcement Lvl>", "Strictly", "Deferred", "Pre-auth Override", "Post-justified Override", "Explained Override", "Guideline" };
        private Model newType;

        public BusinessRuleControl(Model type) : base(type)
        {
            enforcementCombo = new ComboBox();
            enforcementCombo.DataContext = this;
            var levelBinding = new Binding() { Source = EnforcementLevels };
            var defaultBinding = new Binding() { Source = DefaultLevel };
            enforcementCombo.SetBinding(ComboBox.ItemsSourceProperty, levelBinding);
            enforcementCombo.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            contentGrid.RowDefinitions.Clear();
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.8, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5.3, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            Grid.SetRow(headerBox, 0);
            Grid.SetRow(enforcementCombo, 1);
            Grid.SetRow(descriptionBox, 2);
            Grid.SetRow(stateCombo, 3);
            contentGrid.Children.Add(enforcementCombo);
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

        public static new BusinessRuleControl deserialize(JsonObject input)
        {
            var control = (BusinessRuleControl)BPMMControl.deserialize(input);
            var level = input.GetNamedNumber("enforcement-level", -1);
            if (level != -1)
            {
                control.enforcementCombo.SelectedIndex = (int)level;
            }
            return control;
        }
    }
}
