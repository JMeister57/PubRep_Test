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
        public Codes code;
        private string warningShort;
        private string warningLong;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public enum Codes
        {   // missing associations
            M_ASSESSMENT_INFLUENCER, M_ASSESSMENT_ENDS_MEANS, M_INFLUENCER_ASSESSMENT, M_POLICY_RULE, M_DIRECTIVE_ACTION,
            M_STRATEGY_TACTIC, M_ACTION_RESULT, M_GOAL_VISION, M_GOAL_OBJECTIVE, M_MISSION_STRATEGY,
            // wrong associations
            W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE, W_DIRECTIVE_INFLUENCER_NOT_REGULATION, W_DIRECTIVE_VISION_MISSION,
            W_VISION_NOT_MISSION_GOAL_ASSESSMENT
        };
        public static Dictionary<Codes, KeyValuePair<string, string>> catalogue = new Dictionary<Codes, KeyValuePair<string, string>>
            {
                { Codes.M_ASSESSMENT_INFLUENCER, new KeyValuePair<string, string>( "Missing Link to an Influencer", "Assessments are not meant to assess anything else than an Influencer, therefore they should always be attached to at least one Influencer.") },
                { Codes.M_ASSESSMENT_ENDS_MEANS, new KeyValuePair<string, string>( "Missing Link to any Ends or Means", "Assessments are meant to evaluate the impact of Influencers on Ends or Means. An Assessment without relationship is useless for any business process motivation.") },
                { Codes.M_INFLUENCER_ASSESSMENT, new KeyValuePair<string, string>( "m_influencer_assessment", "") },
                { Codes.M_POLICY_RULE, new KeyValuePair<string, string>( "m_policy_rule", "") },
                { Codes.M_DIRECTIVE_ACTION, new KeyValuePair<string, string>( "m_directive_action", "") },
                { Codes.M_STRATEGY_TACTIC, new KeyValuePair<string, string>( "m_strategy_tactic", "") },
                { Codes.M_ACTION_RESULT, new KeyValuePair<string, string>( "m_action_result", "") },
                { Codes.M_GOAL_VISION, new KeyValuePair<string, string>( "m_goal_vision", "") },
                { Codes.M_GOAL_OBJECTIVE, new KeyValuePair<string, string>( "m_goal_objective", "") },
                { Codes.M_MISSION_STRATEGY, new KeyValuePair<string, string>( "m_mission_strategy", "") },
                { Codes.W_INFLUENCER_NOT_ASSESSMENT_DIRECTIVE, new KeyValuePair<string, string>( "w_influencer_not_assessment_directive", "") },
                { Codes.W_DIRECTIVE_INFLUENCER_NOT_REGULATION, new KeyValuePair<string, string>( "w_directive_influencer_not_regulation", "") },
                { Codes.W_DIRECTIVE_VISION_MISSION, new KeyValuePair<string, string>( "w_directive_vision_mission", "") },
                { Codes.W_VISION_NOT_MISSION_GOAL_ASSESSMENT, new KeyValuePair<string, string>( "w_vision_not_mission_goal_assessment", "") },
            };

        public WarningItem(BPMMControl control, Codes code)
        {
            this.InitializeComponent();
            DataContext = this;
            this.control = control;
            this.code = code;
            var entry = new KeyValuePair<string, string>();
            catalogue.TryGetValue(code, out entry);
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
