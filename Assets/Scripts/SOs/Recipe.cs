using UnityEngine;

[CreateAssetMenu(menuName = "Data/Recipe")]
public class Recipe : ScriptableObject
{
    public string identifier;
    public string requiredMachineIdentifier;
    public Fluid[] ingredients;
    public Fluid[] products;
}
