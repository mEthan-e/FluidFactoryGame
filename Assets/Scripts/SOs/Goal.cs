using UnityEngine;

[CreateAssetMenu(menuName = "Data/Goal")]
public class Goal : ScriptableObject
{
    public string goalId;
    [TextArea]
    public string description;
    public Fluid reqFluid;
    public float reqAmount;
    public bool completed;
}
