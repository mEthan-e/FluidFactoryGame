using UnityEngine;

[CreateAssetMenu(menuName = "Data/Fluid")]
public class Fluid : ScriptableObject
{
    public string identifier;
    [Tooltip("Per Litre")]
    public float sellValue;
    public Material material;
    public Color color;
}
