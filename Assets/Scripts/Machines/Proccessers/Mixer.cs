using UnityEngine;

public class Mixer : Machine
{
    private void Start()
    {
        identifier = "mixer";
    }
    protected override void ProccessFluids(Recipe recipe)
    {
        heldFluids.Insert(0 ,recipe.products[0]);
    }
}
