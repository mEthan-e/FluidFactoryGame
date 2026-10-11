using UnityEngine;
using System.Collections.Generic;
using EasyButtons;
using System.Collections;
using Steamworks;

public class MilestoneManager : MonoBehaviour
{
    // This script should instantiate the goal panels. Also manages milestone rewards and completion

    public GameObject goalPanelPrefab;
    public GameObject panelParent;
    public RewardsManager rewardsManager;
    public Milestone[] milestones;
    public Vector3 initialPos;
    public float offset = 80f;

    
    private Milestone activeMilestone;
    private Dictionary<Goal, GameObject> activePanels = new Dictionary<Goal, GameObject>();
    
    private int activeGoalCount = 0;
    private int activeMilestoneIndex = 0;

    private void Update()
    {
        if (activeMilestone == null) {return;}
        activeMilestone.CheckCompletion();
        foreach (Goal goal in activeMilestone.subGoals)
        {
            if (!goal.completed) {continue;}
            if (activePanels.TryGetValue(goal, out GameObject panel))
            {
                activePanels.Remove(goal);
                activeGoalCount -= 1;
                StartCoroutine(panel.GetComponent<GoalInfo>().Exit());
            }
        }
        
        if (activeMilestone.completed)
        {
            // Start the next milestone according to the milestone requirements.
            EnableRewards(activeMilestone);
            StartNextMilestone();
        }

    }
    
    private void AddGoal(Goal goal)
    {
        // Add the goal UI and assign the goal.
        GameObject panel = Instantiate(goalPanelPrefab);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.localPosition = new Vector3(initialPos.x, initialPos.y - (offset * activeGoalCount), 0f);
        panel.GetComponent<GoalInfo>().targetY = initialPos.y - (offset * activeGoalCount);
        panel.GetComponent<GoalInfo>().goal = goal;
        panel.transform.SetParent(panelParent.transform);
        panel.transform.localScale = new Vector3(1, 1, 1);
        activePanels.Add(goal, panel);
        activeGoalCount++;
    }

    private void EnableRewards(Milestone milestone)
    {
        foreach (string id in milestone.rewardsToEnable)
        {
            RewardIdentification reward = rewardsManager.Get(id);
            reward.EnableReward();
        }
    }

    [Button]
    private void TestMilestone()
    {
        StartNextMilestone();
    }
    
    private Milestone GetNextMilestone()
    {
        bool locked = false;
        foreach (Milestone milestone in milestones)
        {
            if (milestone.completed) {continue;}
            foreach (Milestone reqMilestone in milestone.requiredMilestones)
            {
                if (!reqMilestone.completed) {locked = true; break;}
            }
            if (locked) {continue;}
            return milestone;
        }
        return null;
    }
    
    private void StartNextMilestone()
    {
        Milestone nextMilestone = GetNextMilestone();

        if (nextMilestone == null) {return;}
        activeMilestone = nextMilestone;

        StartCoroutine(AddGoalsFancy(activeMilestone.subGoals));
    }

    private IEnumerator AddGoalsFancy(Goal[] goals)
    {
        foreach (Goal goal in goals)
        {
            AddGoal(goal);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
