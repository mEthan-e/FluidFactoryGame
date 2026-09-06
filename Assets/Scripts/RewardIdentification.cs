using UnityEngine;
using UnityEngine.Splines.Interpolators;
using UnityEngine.UIElements;

public class RewardIdentification : MonoBehaviour
{
    public string id;
    public RewardsManager registry;

    private void Start()
    {
        registry.Register(this);
    }

    public virtual void EnableReward()
    {
        gameObject.SetActive(true);
    } 
}
