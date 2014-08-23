using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMM_App
{
    public abstract class BPMM_Object : Object
    {
        public enum Type
        {
            VISION, GOAL, OBJECTIVE, MISSION, STRATEGY, TACTIC, BUSINESS_POLICY, BUSINESS_RULE, INFLUENCER, ASSESSMENT
        }

        public static List<String> State = new List<String>(new String[] { "created", "approved", "denied", "duplicate" });
        String author;
        DateTime creationDate;
        public String title;
        public String description;
        List<String> references;

        public BPMM_Object()
        {
            author = "Tieni";
            creationDate = DateTime.Now;
        }

        public abstract bool linkWith(BPMM_Object targetObj);
    }

    // Ends

    public class Vision : BPMM_Object
    {
        List<Goal> goals;
        List<Mission> missions;
        List<Assessment> assessments;

        public Vision()
            : base()
        {
            goals = new List<Goal>();
            missions = new List<Mission>();
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Goal)
            {
                goals.Add((Goal)targetObj);
            }
            else if (targetObj is Mission) 
            {
                missions.Add((Mission)targetObj);
            }
            else if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Goal : BPMM_Object
    {
        List<Goal> includedGoals;
        List<Objective> objectives;
        List<Assessment> assessments;
        List<Strategy> strategies;

        public Goal()
            : base()
        {
            includedGoals = new List<Goal>();
            objectives = new List<Objective>();
            assessments = new List<Assessment>();
            strategies = new List<Strategy>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Goal)
            {
                includedGoals.Add((Goal)targetObj);
            }
            else if (targetObj is Objective)
            {
                objectives.Add((Objective)targetObj);
            }
            else if (targetObj is Strategy)
            {
                strategies.Add((Strategy)targetObj);
            }
            else if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Objective : BPMM_Object
    {
        String metric;
        String metricTimeFrame;
        
        List<Objective> includedObjectives;
        List<Tactic> tactics;
        List<Assessment> assessments;

        public Objective()
            : base()
        {
            includedObjectives = new List<Objective>();
            tactics = new List<Tactic>();
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Objective)
            {
                includedObjectives.Add((Objective)targetObj);
            }
            else if(targetObj is Tactic)
            {
                tactics.Add((Tactic)targetObj);
            }
            else if(targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    // Means
    public class Mission : BPMM_Object
    {   
        List<Strategy> strategies;
        List<Assessment> assessments;

        public Mission()
            : base()
        {
            strategies = new List<Strategy>();
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Strategy)
            {
                strategies.Add((Strategy)targetObj);
            }
            else if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Strategy : BPMM_Object
    {
        List<Strategy> includedStrategies;
        List<Tactic> tactics;
        List<Assessment> assessments;

        public Strategy()
            : base()
        {
            includedStrategies = new List<Strategy>();
            tactics = new List<Tactic>();
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Strategy)
            {
                includedStrategies.Add((Strategy)targetObj);
            }
            else if (targetObj is Tactic)
            {
                tactics.Add((Tactic)targetObj);
            }
            else if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Tactic : BPMM_Object
    {
        List<Tactic> includedTactics;
        List<BusinessRule> enforcedRules;
        List<Assessment> assessments;

        public Tactic()
            : base()
        {
            includedTactics = new List<Tactic>();
            enforcedRules = new List<BusinessRule>();
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Tactic)
            {
                includedTactics.Add((Tactic)targetObj);
            }
            else if (targetObj is BusinessRule)
            {
                enforcedRules.Add((BusinessRule)targetObj);
            }
            else if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class BusinessPolicy : BPMM_Object
    {
        List<BusinessPolicy> includedPolicies;
        List<Assessment> motivation;

        public BusinessPolicy()
            : base()
        {
            includedPolicies = new List<BusinessPolicy>();
            motivation = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is BusinessPolicy)
            {
                includedPolicies.Add((BusinessPolicy)targetObj);
            }
            else if (targetObj is Assessment)
            {
                motivation.Add((Assessment)targetObj);
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class BusinessRule : BPMM_Object
    {
        enum EnforcementLevel { STRICTLY, DEFERRED, PRE_AUTH_OVERRIDE, POST_JUST_OVERRIDE, EXPLAINED_OVERRIDE, GUIDELINE };

        BusinessPolicy policy;
        EnforcementLevel level;

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is BusinessPolicy)
            {
                policy = (BusinessPolicy)targetObj;
                return true;
            }
            return false;
        }
    }

    // Influencers
    public class Influencer : BPMM_Object
    {
        static List<String> externalInfluencers =
            new List<String>(new String[] { "Competitor", "Customer", "Environment", "Partner", "Regulation", "Supplier", "Technology" });
        static List<String> internalInfluencers =
            new List<String>(new String[] { "Assumption", "Corporate Value, explicit", "Corporate Value, implicit", "Habit",
                                            "Infrastructure", "Issue", "Management Prerogative", "Resource" });
        
        String source;
        String type;
        List<Assessment> assessments;

        public Influencer()
            : base()
        {
            assessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Assessment)
            {
                assessments.Add((Assessment)targetObj);
                return true;
            }
            return false;
        }
    }

    public class Assessment : BPMM_Object
    {
        enum SWOT { STRENGTH, WEAKNESS, OPPORTUNITY, THREAT };

        SWOT type;
        String potentialReward;
        String risk;
        List<Assessment> usedAssessments;

        public Assessment()
            : base()
        {
            usedAssessments = new List<Assessment>();
        }

        public override bool linkWith(BPMM_Object targetObj)
        {
            if (targetObj is Assessment)
            {
                usedAssessments.Add((Assessment)targetObj);
                return true;
            }
            return false;
        }
    }
}
