using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace BMM_App
{
    class AssessmentModel : BMM
    {
        public ComboBox swotControl;
        private static ObservableCollection<string> swot =
            new ObservableCollection<string> { "<SWOT>", "Strength", "Weakness", "Opportunity", "Threat" };

        public AssessmentModel(string author)
            : base(Category.ASSESSMENT, author)
        {
            swotControl = new ComboBox();
            swotControl.DataContext = this;
            var levelBinding = new Binding() { Source = SWOT };
            var defaultBinding = new Binding() { Source = DefaultSWOT };
            swotControl.SetBinding(ComboBox.ItemsSourceProperty, levelBinding);
            swotControl.SetBinding(ComboBox.SelectedItemProperty, defaultBinding);
            frame.RowDefinitions.Clear();
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(36, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            frame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(28, GridUnitType.Pixel) });
            Grid.SetRow(headerBorder, 0);
            Grid.SetRow(swotControl, 1);
            Grid.SetRow(descriptionBorder, 2);
            Grid.SetRow(stateCombo, 3);
            frame.Children.Add(swotControl);
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

        public static new AssessmentModel deserialize(JsonObject input)
        {
            var model = (AssessmentModel)BMM.deserialize(input);
            var swot = input.GetNamedNumber("swot", -1);
            if (swot != -1)
            {
                model.swotControl.SelectedIndex = (int)swot;
            }
            return model;
        }
    }
}
