using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace BMM_App
{
    public enum TourStep { None, V1, V2, V3, G1, G2, O1, O2, S1, S2, T1, T2, I1, I2, A1, A2, A3, A4, P1, P2, R1, R2, R3, R4, R5 }

    public sealed partial class MainPage
    {
        private TourStep step = TourStep.None;
        private Storyboard storyboard;

        private string guideText;
        private SolidColorBrush highlightedBrush;
        private BMM Vision;
        private BMM Goal;
        private BMM Objective;
        private BMM Strategy;
        private BMM Tactic;
        private BMM BusinessPolicy;
        private BusinessRuleModel BusinessRule;
        private InfluencerModel Influencer;
        private AssessmentModel Assessment;

        private async void startTour(object sender, PointerRoutedEventArgs e)
        {
            if (await clearWorkspace())
            {
                MessageDialog greetings =
                    new MessageDialog("Step by step we will create a business motivation model for a fictional cancer research institute."
                        + " You can cancel the tour at any time by tapping the 'End Tour' symbol. Have fun!",
                        String.Format("Welcome to our guided tour, {0}!", user));
                greetings.Commands.Add(new UICommand("Start Tour"));
                greetings.Commands.Add(new UICommand("Cancel"));
                var result = await greetings.ShowAsync();
                if (result.Label == "Start Tour")
                {
                    tourIcon.Symbol = Symbol.Clear;
                    ToolTipService.SetToolTip(tourIcon, "End Tour");
                    tourBorder.PointerReleased -= startTour;
                    tourBorder.PointerReleased += endTour;
                    storyboard = new Storyboard();
                    performStep(TourStep.V1);
                }
            }
        }

        private async void performStep(TourStep nextStep)
        {
            step = nextStep;
            switch (nextStep)
            {
                case TourStep.None:
                    storyboard.Stop();
                    if (storyboard.Children.Count == 1)
                    {
                        storyboard.Children.RemoveAt(0); 
                    }
                    highlightedBrush.Color = Colors.Transparent;
                    guideBubble.Visibility = Visibility.Collapsed;
                    Guide = "";
                    step = TourStep.None;
                    
                    tourIcon.Symbol = Symbol.Help;
                    ToolTipService.SetToolTip(tourIcon, "Start Tour");
                    tourBorder.PointerReleased -= endTour;
                    tourBorder.PointerReleased += startTour;
                    break;
                case TourStep.V1:
                    visionIcon.Background = highlightBrush();
                    Guide = "Business Motivation Models need Ends to justify any planned Means. The highest End is an organization's Vision."
                        + "So, let's start by formulating a Vision for our cancer research institute.\nSimply drag a Vision element onto the workspace.";
                    guideBubble.Visibility = Visibility.Visible;
                    break;
                case TourStep.V2:
                    Vision = (BMM)workspace.Children.Last();
                    Vision.headerBorder.Background = highlightBrush();
                    Vision.headerBox.KeyUp += Tour_TextBoxEdited;
                    Guide = "Good job! Next, we should give our Vision a descriptive name. Double-tap its header and enter e.g.:\"Cancer Research\". To finish editing, press [Enter].";
                    break;
                case TourStep.V3:
                    Vision.headerBox.KeyUp -= Tour_TextBoxEdited;
                    Vision.descriptionBox.KeyUp += Tour_TextBoxEdited;
                    Vision.descriptionBorder.Background = highlightBrush();
                    Guide = "Next, we formulate the Vision itself. Double-tap its description and enter e.g.: "
                        + "\"The ultimate defeat of cancer and further education of young academics with exclusively non-profit activities\". Again, finish by pressing [Enter].";
                    break;
                case TourStep.G1:
                    Vision.descriptionBox.KeyUp -= Tour_TextBoxEdited;
                    goalIcon.Background = highlightBrush();
                    Guide = "Excellent. This Vision describes the final purpose of our cancer reserach institute. In order to reach it we need to formulate smaller Goals."
                        + "\nSo let's place a Goal on our workspace, next.";
                    break;
                case TourStep.G2:
                    Goal = (BMM)workspace.Children.Last();
                    Goal.Title = "Attractive Employer";
                    Goal.Description = "Become an attractive employer for young academics, especially for PhD students.";
                    Goal.anchor.Background = highlightBrush();
                    Vision.LinkEndEvent += Tour_linkEstablished;
                    Guide = "To save you some time I already formulated this Goal. Note how it describes exactly one of many possible targets for the Vision: "
                        + "To educate young academics the institute needs to attract many of them."
                        + "\nPlease link the Goal with the Vision by dragging from the Goal's link achor and releasing somewhere on the Vision.";
                    break;
                case TourStep.O1:
                    Vision.LinkEndEvent -= Tour_linkEstablished;
                    objectiveIcon.Background = highlightBrush();
                    Guide = "The next step is to state concrete and realizable Objectives to achieve our Goal. Please add an Objective to the workspace.";
                    break;
                case TourStep.O2:
                    Objective = (BMM)workspace.Children.Last();
                    Objective.anchor.Background = highlightBrush();
                    Objective.Title = "Known by PhDs";
                    Objective.Description = "Within the next two years become familiar to at least 90% of the PhD students in the field of Medicine and Biology.";
                    Goal.LinkEndEvent += Tour_linkEstablished;
                    Guide = "Again, we skip the typing work. Pay attention to the difference between Goals and Objectives."
                        + " While Goals represent single inconcrete targets for the Vision, Objectives are meant to be precise enough to derive Business Processes from them."
                        + "\nSince this Objective helps realizing our \"Attractive Employer\" Goal, let's link the two.";
                    break;
                case TourStep.S1:
                    Goal.LinkEndEvent -= Tour_linkEstablished;
                    strategyIcon.Background = highlightBrush();
                    Guide = "We could add more Goals and Objectives, for example the Goal to become nation-wide leading institute in cancer research by year X "
                        + "or the Objective to become well known by undergraduates as well, you get the idea.\nTo keep it simple, we finish Ends and continue with Means instead."
                        + "\nPlease drag a Strategy to the workspace.";
                    break;
                case TourStep.S2:
                    Strategy = (BMM)workspace.Children.Last();
                    Strategy.Title = "Excellent Working Conditions";
                    Strategy.Description = "Establish excellent working conditions for young academics, for PhD students in particular.";
                    Strategy.anchor.Background = highlightBrush();
                    Goal.LinkEndEvent += Tour_linkEstablished; 
                    Guide = "Developing good working conditions is one possible strategy to attain the Goal \"Attractive Employer\"."
                        + " Another strategy could be to offer exceptionally high salary.\nBut let's link this one with our Goal.";
                    break;
                case TourStep.T1:
                    Goal.LinkEndEvent -= Tour_linkEstablished;
                    tacticIcon.Background = highlightBrush();
                    Guide = "Next, just like with the pair of Goal and Objective a Strategy sometimes needs concrete Tactics that implement it."
                        + " Organizations sometimes choose to not make this distinction, but for the sake of completeness,\nplease add a Tactic to your workspace.";
                    break;
                case TourStep.T2:
                    Tactic = (BMM)workspace.Children.Last();
                    Tactic.Title = "Annual Festival";
                    Tactic.Description = "Celebrate annual festivals to which employees can also contribute.";
                    Tactic.anchor.Background = highlightBrush();
                    Strategy.LinkEndEvent += Tour_linkEstablished;
                    Guide = "By celebrating annual festivals the Strategy \"Excellent Working Conditions\" is implemented.\nA link should visualize this relationship.";
                    break;
                case TourStep.I1:
                    Strategy.LinkEndEvent -= Tour_linkEstablished;
                    influencerIcon.Background = highlightBrush();
                    Guide = "Well done! So far we have defined some Ends and corresponding Means for our organization."
                        + "\nThe next important concept are Influencers.\nDrag an Influencer onto the workspace.";
                    break;
                case TourStep.I2:
                    Influencer = (InfluencerModel)workspace.Children.Last();
                    Influencer.Title = "Survey on Research Money";
                    Influencer.Description = "According to a survey of institute X, 35% of the society believe that taxpayer's money is not used effectively in research institutes.";
                    Influencer.influencerCombo.Background = highlightBrush();
                    Influencer.influencerCombo.SelectionChanged += Tour_ComboSelectionChanged;
                    Guide = "Influencers represent changes of the environment that might affect the organization's activities and plans."
                        + " Note that Influencers are neutral. Their actual impact will be dealt with later.\nIt is possible to categorize Influencers."
                        + " Select \"Environment\" for this one.";
                    break;
                case TourStep.A1:
                    Influencer.influencerCombo.SelectionChanged -= Tour_ComboSelectionChanged;
                    assessmentIcon.Background = highlightBrush();
                    Guide = "I have mentioned  that Influencers are formulated in a neutral way. In fact, a motivation should never be directly derived from an Influencer."
                        + " Instead, in order to evaluate its impact on the organization, an Assessment can be made.\nPlease add an Assessment to the workspace.";
                    break;
                case TourStep.A2:
                    Assessment = (AssessmentModel)workspace.Children.Last();
                    Assessment.Title = "Festivals Reputation";
                    Assessment.Description = "The annual festivals of the institute might lead to negative public perception.";
                    Assessment.anchor.Background = highlightBrush();
                    Guide = "This Assessment addresses the Influencer's impact on the Tactic \"Annual Festival\"."
                        + " The Influencer might also affect other parts of the organizational plan, for each such impact, a new Assessment should be created."
                        + "\n Link this one with its Influencer and with mentioned Tactic.";
                    Influencer.LinkEndEvent += Tour_linkEstablished;
                    Tactic.LinkEndEvent += Tour_linkEstablished;
                    break;
                case TourStep.A3:
                    foreach (var link in findLinks(Influencer))
                    {
                        if (link.sourceModel == Assessment || link.targetModel == Assessment)
                        {
                            Influencer.LinkEndEvent -= Tour_linkEstablished;
                            return;
                        }
                    }
                    Tactic.LinkEndEvent -= Tour_linkEstablished;
                    break;
                case TourStep.A4:
                    Influencer.LinkEndEvent -= Tour_linkEstablished;
                    Tactic.LinkEndEvent -= Tour_linkEstablished;
                    Assessment.swotControl.Background = highlightBrush();
                    Assessment.swotControl.SelectionChanged += Tour_ComboSelectionChanged;
                    Guide = "Note that an impact can be of various nature. It can be an internal Strength or Weakness or an external Threat or Opportunity."
                        + "\nClassify this one as a Threat.";
                    break;
                case TourStep.P1:
                    Assessment.swotControl.SelectionChanged -= Tour_ComboSelectionChanged;
                    policyIcon.Background = highlightBrush();
                    Guide = "After an Assessment has been made it should not go without consequences."
                        + " Let's create a Business Policy to counter the Threat to our cancer research institute.\nDrag a Business Policy onto the workspace.";
                    break;
                case TourStep.P2:
                    BusinessPolicy = (BMM)workspace.Children.Last();
                    BusinessPolicy.Title = "Thrifty Budget";
                    BusinessPolicy.Description = "The institute shall use its budget task-oriented and thrifty.";
                    BusinessPolicy.anchor.Background = highlightBrush();
                    Tactic.LinkEndEvent += Tour_linkEstablished;
                    Guide = "This Business Policy is motivated by the newly made Assessment, but it also supports the non-profit part of the institute's Vision."
                        + " Since it restricts the Tactic \"Annual Festival\", let's link the two.";
                    break;
                case TourStep.R1:
                    Tactic.LinkEndEvent -= Tour_linkEstablished;
                    ruleIcon.Background = highlightBrush();
                    Guide = "We are almost done! The last thing I'd like to demonstrate is the Business Rule."
                        + "\nYou might have noticed, that the Business Policy is not very precise as a restriction. It is still arguable which investions are necessarys, "
                        + "so that the Policy is not directly enforceable.\nAdd a Business Rule to the workspace to see an example of an enforceable Directive.";
                    break;
                case TourStep.R2:
                    BusinessRule = (BusinessRuleModel)workspace.Children.Last();
                    BusinessRule.Title = "Student Salary";
                    BusinessRule.Description = "Undergraduates and students with Master's degree shall receive an ordinary research assistant salary at most.";
                    BusinessRule.enforcementCombo.Background = highlightBrush();
                    BusinessRule.enforcementCombo.SelectionChanged += Tour_ComboSelectionChanged;
                    Guide = "This rule is directly enforceable since it is objectively possible to determine if the rule is broken or not."
                        + "Additionally you can classify the Rule's enforcement level. I would choose \"Strictly\" for this one, what do you say?";
                    break;
                case TourStep.R3:
                    BusinessRule.enforcementCombo.SelectionChanged -= Tour_ComboSelectionChanged;
                    BusinessRule.anchor.Background = highlightBrush();
                    BusinessPolicy.LinkEndEvent += Tour_linkEstablished;
                    Goal.LinkEndEvent += Tour_linkEstablished;
                    Guide = "Lastly, since our Business Rule reduces the budget for employees it also supports the Policy \"Thrifty Budget\" and restricts the Goal"
                        + " \"Attractive Employer\". We should draw links to the two.";
                    break;
                case TourStep.R4:
                    foreach (var link in findLinks(BusinessPolicy))
                    {
                        if (link.sourceModel == BusinessRule || link.targetModel == BusinessRule)
                        {
                            BusinessPolicy.LinkEndEvent -= Tour_linkEstablished;
                            return;
                        }
                    }
                    Goal.LinkEndEvent -= Tour_linkEstablished;
                    break;
                case TourStep.R5:
                    BusinessPolicy.LinkEndEvent -= Tour_linkEstablished;
                    Goal.LinkEndEvent -= Tour_linkEstablished;
                    storyboard.Stop();
                    storyboard.Children.RemoveAt(0);
                    highlightedBrush.Color = Colors.Transparent;
                    guideBubble.Visibility = Visibility.Collapsed;
                    Guide = "";
                    step = TourStep.None;
                    
                    tourIcon.Symbol = Symbol.Help;
                    ToolTipService.SetToolTip(tourIcon, "Start Tour");
                    tourBorder.PointerReleased -= endTour;
                    tourBorder.PointerReleased += startTour;

                    MessageDialog tourEnd =
                    new MessageDialog("You have learnt the most important concepts of the Business Motivation Model. For further reading explore our documentation. "
                        + "Otherwise, you can go on and create your own diagram now. Tap the trashcan to clear the workspace. I wish you and your organization much success!"
                        , "Congratulations!");
                    await tourEnd.ShowAsync();
                    break;
                }
        }

        private SolidColorBrush highlightBrush()
        {
            storyboard.Stop();
            if (storyboard.Children.Count > 0)
            {
                storyboard.Children.RemoveAt(0);
                highlightedBrush.Color = Colors.Transparent;
            }
            highlightedBrush = new SolidColorBrush(Colors.LightGoldenrodYellow);

            var colorBreath = new ColorAnimation()
            {
                From = Colors.LightYellow,
                To = Colors.DarkOrange,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            storyboard.Children.Add(colorBreath);
            Storyboard.SetTargetProperty(colorBreath, "Color");
            Storyboard.SetTarget(colorBreath, highlightedBrush);
            storyboard.Begin();
            return highlightedBrush;
        }

        private void Tour_TextBoxEdited(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                performStep(step + 1);
            }
        }

        private void Tour_linkEstablished(object sender, EventArgs e)
        {
            if (step == TourStep.G2 && currentLine.sourceModel == Goal
                || step == TourStep.O2 && currentLine.sourceModel == Objective
                || step == TourStep.S2 && currentLine.sourceModel == Strategy
                || step == TourStep.T2 && currentLine.sourceModel == Tactic
                || step == TourStep.A2 && currentLine.sourceModel == Assessment
                || step == TourStep.A3 && currentLine.sourceModel == Assessment
                || step == TourStep.P2 && currentLine.sourceModel == BusinessPolicy
                || step == TourStep.R3 && currentLine.sourceModel == BusinessRule
                || step == TourStep.R4 && currentLine.sourceModel == BusinessRule)
            if (true)
            {
                currentLine = null;
                performStep(step + 1);
            }
        }

        private void Tour_ComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            performStep(step + 1);
        }

        private void endTour(object sender, PointerRoutedEventArgs e)
        {
            performStep(TourStep.None);
        }

        public string Guide
        {
            get { return guideText; }
            set { guideText = value; OnPropertyChanged("Guide"); }
        }
    }
}