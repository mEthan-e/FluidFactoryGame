using UnityEngine;

public class ChangePrefab : MonoBehaviour
{
    public BlockBuilder blockBuilder;
    public GameObject objectToSwitch;
    public GameObject previewToSwitch;
    
    public void ChangeBuilderPrefab()
    {
        blockBuilder.blockPrefab = objectToSwitch;
        blockBuilder.previewPrefab = previewToSwitch;
    }
}
