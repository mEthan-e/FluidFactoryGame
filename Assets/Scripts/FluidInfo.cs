using UnityEngine;
using TMPro;

public class FluidInfo : MonoBehaviour
{
    public Stats globalStats;
    public string relatedFluidId;
    public TMP_Text amountDisplay;

    private void Update()
    {
        if (globalStats.heldFluids.TryGetValue(relatedFluidId, out float amount))
        {
            amountDisplay.text = (Mathf.Round(amount * 100f) / 100f).ToString() + "L";
        }
        
    }
}
