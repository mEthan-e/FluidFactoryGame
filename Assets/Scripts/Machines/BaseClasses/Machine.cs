using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public abstract class Machine : Pipe
{
    public Recipe[] recipies;
    public float proccessTime = 2f;
    protected List<Fluid> heldFluids = new List<Fluid>();
    protected string identifier;
    private float timer;
    // Update already calls SetNeighbouringPipes and ManageThroughput
    
    public Recipe GetPossibleRecipe()
    {
        foreach (Recipe recipie in recipies)
        {
            bool skipIter = false;
            if (recipie.requiredMachineIdentifier != identifier) {continue;}
            if (heldFluids.Count <= 0) {return null;}
            if (heldFluids.Count == 1 && recipie.ingredients.Contains(heldFluids[0]) && recipie.ingredients.Length == 1)
            {
                return recipie;
            }
            else if (recipie.ingredients.Length == heldFluids.Count)
            {
                // check all held fluids against all ingredients to figure out if it's possible
                int i = 0;
                foreach (Fluid ingredient in recipie.ingredients)
                {
                    
                    if (!recipie.ingredients.Contains(heldFluids[i]))
                    {
                        skipIter = true;
                        break;
                    }
                    i++;
                }
                if(skipIter)
                    continue;
                return recipie;
            }
        }
        return null;
    }

    protected override void ManageThroughput()
    {

        timer += Time.deltaTime;

        if (heldFluids.Count > 0)
            heldFluid = heldFluids[0];
        
        // PREVIOUS PIPES MOVE FLUID IN
        if (previousPipes.Count <= 0)
        {
            heldFluids.Clear();
        }
        
        if (heldFluids.Count <= 0)
        {
            SetAllRendererMaterial(nullMat);
        }
        else
        {
            SetAllRendererMaterial(heldFluids[0].material);
        }
        // DO MACHINE PROCCESS
        if (timer >= proccessTime)
        {
            timer -= proccessTime;

            Recipe recipe = GetPossibleRecipe();

            if (recipe != null)
                ProccessFluids(recipe);
        }

        // MOVE FLUIDS OUT
        if (nextPipes.Count <= 0) {return;}
        if (heldFluids.Count < 1) 
        {
            nextPipes[0].SetHeldFluid(null);
            return;
        }
        if (nextPipes.Count == 1)
        {
            nextPipes[0].SetHeldFluid(heldFluids[0]);
            nextPipes[0].throughput = 1 / proccessTime;
        }
        else if (nextPipes.Count > 1)
        {
            // TODO: DISTRIBUTE ONE FLUID ONE WAY, THE OTHER FLUID TO THE OTHER WAY
            // foreach pipe in nextPipes we assign the next fluid 
            var i = 0;
            foreach (Pipe pipe in nextPipes)
            {
                pipe.SetHeldFluid(heldFluids[i]);
                i++;
            }
        }
    }

    public override void SetHeldFluid(Fluid fluidToSet)
    {
        if (heldFluids.Contains(fluidToSet)) {return;}
        if (fluidToSet == null) {return;}
        heldFluids.Add(fluidToSet);
    }

    protected abstract void ProccessFluids(Recipe recipe); // OVERRIDE THIS ON CHILDREN CLASSES
}
