using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace BPMM_App
{
    public sealed partial class WarningItem : UserControl, INotifyPropertyChanged
    {
        public BPMMControl control;
        private string warningShort;
        private string warningLong;
        public event PropertyChangedEventHandler PropertyChanged;

        public static Dictionary<string, KeyValuePair<string, string>> catalogue = new Dictionary<string, KeyValuePair<string, string>>
            {
                { "MLINK_1", new KeyValuePair<string, string>( "Missing Link to an Influencer", "Assessments are not meant to assess anything else than an Influencer, therefore they should always be attached to at least one Influencer.") },
                { "MLINK_2", new KeyValuePair<string, string>( "Missing Link to any Ends or Means", "Assessments are meant to evaluate the impact of Influencers on Ends or Means. An Assessment without relationship is useless for any business process motivation.") },
                { "MLINK_3", new KeyValuePair<string, string>( "", "") }
            };

        public WarningItem(BPMMControl control, string text)
        {
            this.InitializeComponent();
            DataContext = this;
            this.control = control;
            var entry = new KeyValuePair<string, string>();
            catalogue.TryGetValue(text, out entry);
            Warning_Short = String.Format("Model {0} \"{1}\": {2}.", control.id, control.Title, entry.Key);
            Warning_Long = entry.Value;
        }

        public string Warning_Short
        {
            get { return warningShort; }
            set { warningShort = value; OnPropertyChanged("Warning_Short"); }
        }

        public string Warning_Long
        {
            get { return warningLong; }
            set { warningLong = value; OnPropertyChanged("Warning_Long"); }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
