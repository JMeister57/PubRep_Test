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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Popups;

namespace BPMM_App
{

    public sealed partial class MainPage : Page
    {      
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        bool associating;
        private AssociationControl currentLine;
        private BPMMControl sourceControl;
        
        private bool selecting;
        private Point selectionStartPoint;
        private Rectangle selectionBox;

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
            associating = false;
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
                        obj = (BPMM_Object)new Influencer();
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
                control.AssociationEvent += OnAssociationStart;
                control.PointerReleased += OnAssociationRequest;
                control.DeleteEvent += DeleteControl;
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
            else if (e.Items[0].Equals(association)) { e.Data.Properties.Add("Item", "Association"); }
        }
        #region association drawing
        public void OnAssociationStart(object sender, PointerRoutedEventArgs e)
        {
            sourceControl = (BPMMControl)sender;
            Point p = new Point(Canvas.GetLeft(sourceControl), Canvas.GetTop(sourceControl));
            currentLine = new AssociationControl(sourceControl, p, e.GetCurrentPoint((UIElement)sender).Position);
            sourceControl.MovedEvent += currentLine.sourceMoved;
            sourceControl.DeleteEvent += currentLine.Delete;
            currentLine.DeleteEvent += DeleteAssociation;
            workspace.Children.Add(currentLine);
            associating = true;
        }

        public void OnAssociationRequest(object sender, PointerRoutedEventArgs e)
        {
            if (currentLine == null)
            { // case: simple clickon control
                return;
            }

            BPMMControl target = (BPMMControl)sender;
            Point p = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));

            if (currentLine.viewModel.Points[0].Equals(p))
            { // case: association to itself
                Debug.WriteLine("Cannot pull association to itself.");
                workspace.Children.Remove(currentLine);
            }

            else if ((sourceControl).linkableWith(target))
            { // case: allowed association
                sourceControl.linkWith(target);
                currentLine.updateEndPoint(target, p);
                target.MovedEvent += currentLine.targetMoved;
                target.DeleteEvent += currentLine.Delete;
                sourceControl = null;
            }
            else
            { // case: misfitting BPMM objects
                Debug.WriteLine("these two objects cannot be linked");
                workspace.Children.Remove(currentLine);
            }
            sourceControl = null;
            currentLine = null;
            associating = false;
        }
        #endregion

        private void workspace_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            selecting = true;
            selectionStartPoint = e.GetCurrentPoint(workspace).Position;
            selectionBox = new Rectangle()
            {
                Width = Math.Abs(selectionStartPoint.X - selectionStartPoint.X),
                Height = Math.Abs(selectionStartPoint.Y - selectionStartPoint.Y),
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Blue) { Opacity = 1 },
                StrokeThickness = 4,
                Opacity = 0.2
            };
            Canvas.SetLeft(selectionBox, selectionStartPoint.X);
            Canvas.SetTop(selectionBox, selectionStartPoint.Y);
            workspace.Children.Add(selectionBox);
            workspace.CapturePointer(e.Pointer);
        }

        private void workspace_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (associating)
            {
                Point target = e.GetCurrentPoint((UIElement)sender).Position;
                target.X -= 5;
                target.Y -= 5;
                currentLine.updateEndPoint(null, target);
            }
            else if (selecting)
            {
                Point currPoint = e.GetCurrentPoint(workspace).Position;
                currPoint.X = (currPoint.X > workspace.Width) ? workspace.Width
                                                                : (currPoint.X < 0) ? 0 : currPoint.X;
                currPoint.Y = (currPoint.Y > workspace.Height) ? workspace.Height
                                                                : (currPoint.Y < 0) ? 0 : currPoint.Y;
                if (currPoint.X < selectionStartPoint.X)
                {
                    Canvas.SetLeft(selectionBox, currPoint.X);
                }
                if (currPoint.Y < selectionStartPoint.Y)
                {
                    Canvas.SetTop(selectionBox, currPoint.Y);
                }
                selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
                selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
            }
        }

        private void workspace_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (associating)
            {
                associating = false;
                workspace.Children.Remove(currentLine);
                currentLine = null;
            }

            else if (selecting)
            {
                Point currPoint = e.GetCurrentPoint(workspace).Position;
                selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
                selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
                workspace.Children.Remove(selectionBox);
                selecting = false;
                workspace.ReleasePointerCapture(e.Pointer);
            }
        }
        
        public void DeleteControl(object sender, EventArgs e)
        {
            workspace.Children.Remove((BPMMControl)sender);
        }

        public void DeleteAssociation(object sender, EventArgs e)
        {
            workspace.Children.Remove((AssociationControl)sender);
        }
    }
}
