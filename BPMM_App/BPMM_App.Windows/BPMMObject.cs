using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMM_App
{
    public class BPMM_Object
    {
        public enum Type
        {
            VISION, GOAL, OBJECTIVE, MISSION, STRATEGY, TACTIC, BUSINESS_POLICY, BUSINESS_RULE, INFLUENCER, ASSESSMENT
        }

        String author;
        DateTime creationDate;
        public String title;
        public String description;
        List<String> references;
    }

    // Ends

    public class Vision : BPMM_Object
    {
        List<Goal> goals;
        List<Mission> missions;
        List<Assessment> assessments;
    }

    public class Goal : BPMM_Object
    {
        List<Goal> includedGoals;
        List<Objective> objectives;
        List<Assessment> assessments;
        List<Strategy> strategies;
    }

    public class Objective : BPMM_Object
    {
        List<Objective> includedObjectives;
        String metric;
        String metricTimeFrame;

        List<Assessment> assessments;
        List<Tactic> tactics;
    }

    // Means
    public class Mission : BPMM_Object
    {   
        List<Strategy> strategies;
        List<Assessment> assessments;
    }

    public class Strategy : BPMM_Object
    {
        List<Strategy> includedStrategies;
        List<Tactic> tactics;
        List<Assessment> assessments;
    }

    public class Tactic : BPMM_Object
    {
        List<Tactic> includedTactics;
        List<BusinessRule> enforcedRules;
        List<Assessment> assessments;
    }

    public class BusinessPolicy : BPMM_Object
    {
        List<BusinessPolicy> includedPolicies;
        List<Assessment> motivation;
    }

    public class BusinessRule : BPMM_Object
    {
        enum EnforcementLevel { STRICTLY, DEFERRED, PRE_AUTH_OVERRIDE, POST_JUST_OVERRIDE, EXPLAINED_OVERRIDE, GUIDELINE };

        BusinessPolicy policy;
        EnforcementLevel level;
    }

    // Influencers
    public class Influencer : BPMM_Object
    {
        static List<String> externalInfluencers;
        static List<String> internalInfluencers;
        
        String source;
        String type;
        List<Assessment> assessments;
    }

    public class Assessment : BPMM_Object
    {
        enum SWOT { STRENGTH, WEAKNESS, OPPORTUNITY, THREAT };

        SWOT type;
        String potentialReward;
        String risk;
        List<Assessment> usedAssessments;
    }
}
