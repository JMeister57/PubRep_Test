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
    class AssessmentControl : BPMMControl
    {
        private ComboBox swotControl;
        private static ObservableCollection<string> swot =
            new ObservableCollection<string> { "<SWOT>", "Strength", "Weakness", "Opportunity", "Threat" };

        public AssessmentControl()
            : base(Category.ASSESSMENT)
        {
            swotControl = new ComboBox();
            swotControl.DataContext = this;
            var levelBinding = new Binding() { Source = SWOT };
            var defaultBinding = new Binding() { Source = DefaultSWOT };
            swotControl.SetBinding(ComboBox.ItemsSourceProperty, levelBinding);
            swotControl.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            frame.RowDefinitions.Clear();
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.8, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5.3, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.4, GridUnitType.Star) });
            Grid.SetRow(headerBox, 0);
            Grid.SetRow(swotControl, 1);
            Grid.SetRow(descriptionBox, 2);
            Grid.SetRow(stateCombo, 3);
        }

        public override void UpdateFontSize(double scale)
        {
            swotControl.FontSize = swotControl.FontSize * scale;
        }

        public ObservableCollection<string> SWOT
        {
            get { return swot; }
            set 
            {
                swot = value;
                OnPropertyChanged("SWOT");
            }
        }

        public string DefaultSWOT
        {
            get { return swot[0]; }
        }

        public override JsonObject serialize()
        {
            var controlEntry = base.serialize();
            controlEntry.Add("swot", JsonValue.CreateNumberValue(swotControl.SelectedIndex));
            return controlEntry;
        }

        public static new AssessmentControl deserialize(JsonObject input)
        {
            var control = (AssessmentControl)BPMMControl.deserialize(input);
            var swot = input.GetNamedNumber("swot", -1);
            if (swot != -1)
            {
                control.swotControl.SelectedIndex = (int)swot;
            }
            return control;
        }
    }
}
