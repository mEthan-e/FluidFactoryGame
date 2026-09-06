using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class RewardsManager : MonoBehaviour
{
    private Dictionary<string, RewardIdentification> register = new();

    public void Register(RewardIdentification reward)
    {
        register.Add(reward.id, reward);
    }

    public RewardIdentification Get(string id)
    {
        return register[id];
    }

    private void Update()
    {
        Debug.Log(register["tunnelExit"]);
    }
}
