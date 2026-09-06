using UnityEngine;

[CreateAssetMenu(menuName = "Data/Milestone")]
public class Milestone : ScriptableObject
{
    public Goal[] subGoals;
    public string[] rewardsToEnable;
    public Milestone[] requiredMilestones;
    public bool completed = false;

    public void CheckCompletion()
    {
        foreach (Goal goal in subGoals)
        {
            if (!goal.completed)
            {
                completed = false;
                return;
            }
        }
        
        completed = true;
    }
}
