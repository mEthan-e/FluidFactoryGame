using UnityEngine;
using UnityEngine.UI;

public class ButtonRewardId : RewardIdentification
{
    public Button button;
    
    private void Start()
    {
        registry.Register(this);
        button.interactable = false;
    }

    public override void EnableReward()
    {
        button.interactable = true;
    }
}
