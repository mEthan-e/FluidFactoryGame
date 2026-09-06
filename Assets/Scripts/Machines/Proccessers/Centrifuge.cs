using UnityEngine;

public class Centrifuge : Machine
{
    private void Start()
    {
        identifier = "centrifuge";
    }
    
    protected override void ProccessFluids(Recipe recipe)
    {
        foreach (Fluid product in recipe.products)
        {
            heldFluids.Insert(0, product);
        }
    } 
}
