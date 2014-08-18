using BPMM_App.Common;
using System;
using System.Collections.Generic;
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
using Windows.Storage;

namespace BPMM_App
{

    public sealed partial class MainPage : Page
    {      
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // restore session data
            if (e.PageState != null && e.PageState.ContainsKey("greetingOutputText"))
            {

            }
            // restore app data
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        #region NavigationHelper-Registrierung
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            object item;
            if (e.Data.Properties.TryGetValue("Item", out item))
            {   
                BPMM_Object.Type type = (BPMM_Object.Type)item;
                BPMM_Object obj;
                String title;
                switch (type)
                {
                    case BPMM_Object.Type.VISION:
                        obj = (BPMM_Object) new Vision();
                        title = "Vision";
                        break;
                    case BPMM_Object.Type.GOAL:
                        obj = (BPMM_Object) new Goal();
                        title = "Goal";
                        break;
                    case BPMM_Object.Type.OBJECTIVE:
                        obj = (BPMM_Object) new Objective();
                        title = "Objective";
                        break;
                    case BPMM_Object.Type.MISSION:
                        obj = (BPMM_Object) new Mission();
                        title = "Mission";
                        break;
                    case BPMM_Object.Type.STRATEGY:
                        obj = (BPMM_Object) new Strategy();
                        title = "Strategy";
                        break;
                    case BPMM_Object.Type.TACTIC:
                        obj = (BPMM_Object) new Tactic();
                        title = "Tactic";
                        break;
                    case BPMM_Object.Type.BUSINESS_POLICY:
                        obj = (BPMM_Object) new BusinessPolicy();
                        title = "Policy";
                        break;
                    case BPMM_Object.Type.BUSINESS_RULE:
                        obj = (BPMM_Object) new BusinessRule();
                        title = "Rule";
                        break;
                    case BPMM_Object.Type.INFLUENCER:
                        obj = (BPMM_Object) new Influencer();
                        title = "Influencer";
                        break;
                    case BPMM_Object.Type.ASSESSMENT:
                        obj = (BPMM_Object) new Assessment();
                        title = "Assessment";
                        break;
                    default:
                        return;
                }
                BPMMControl control = new BPMMControl(obj);
                control.type = type;
                control.viewModel.Title = title;
                Point pos = e.GetPosition(workspace);
                Canvas.SetLeft(control, pos.X);
                Canvas.SetTop(control, pos.Y);
                workspace.Children.Add(control);                
            }
        }

        private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items[0].Equals(visionIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.VISION); }
            else if (e.Items[0].Equals(goalIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.GOAL); }
            else if (e.Items[0].Equals(objectiveIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.OBJECTIVE); }
            else if (e.Items[0].Equals(missionIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.MISSION); }
            else if (e.Items[0].Equals(strategyIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.STRATEGY); }
            else if (e.Items[0].Equals(tacticIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.TACTIC); }
            else if (e.Items[0].Equals(policyIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.BUSINESS_POLICY); }
            else if (e.Items[0].Equals(ruleIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.BUSINESS_RULE); }
            else if (e.Items[0].Equals(influencerIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.INFLUENCER); }
            else if (e.Items[0].Equals(assessmentIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.ASSESSMENT); }
        }
    }
}
