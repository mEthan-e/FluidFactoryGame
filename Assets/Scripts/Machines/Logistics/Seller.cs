using UnityEngine;
using System.Collections.Generic;

public class Seller : MonoBehaviour
{
    public Renderer[] fluidRenderer;
    public Material nullMat;
    public LayerMask pipeLayer;
    public Stats globalStats;
    public float throughput = 1f;
    
    private Pipe previousPipe;
    private bool isSelling;
    private bool isEmpty;
    
    private void SetNeighbouringPipes()
    {
        Ray rayBackward = new Ray(transform.position, transform.forward);
        
        if (Physics.Raycast(rayBackward, out RaycastHit hitBack, 1f, pipeLayer))
        {
            if(hitBack.collider.gameObject.CompareTag("Pipe"))
            {
                previousPipe = hitBack.collider.gameObject.GetComponent<Pipe>();
                if (previousPipe != null)
                {
                    throughput = previousPipe.throughput;
                }
            }
        }

    }

    private void Update()
    {
        if (previousPipe != null)
        {
            isEmpty = (previousPipe.GetHeldFluid() == null);
            if (isEmpty)
            {
                SetAllRendererMaterial(nullMat);
            }
            else
            {
                SetAllRendererMaterial(previousPipe.GetHeldFluid().material);
            }
        }
        
        if (globalStats == null)
        {
            globalStats = FindAnyObjectByType<Stats>();
        }
        SetNeighbouringPipes();
        // store the fluid in global stats by litre, sell later or use for milestone, whichever is relevant. Maybe by frame (deltaTime * throughput or smth)
        // globalStats.heldFluids (access the dict to add to the amount of fluid per frame, if the fluid is not currently referenced then add it.)
        if (previousPipe == null) {return;}
        if (previousPipe.GetHeldFluid() == null) {return;}

        if (globalStats.heldFluids.ContainsKey(previousPipe.GetHeldFluid().identifier))
        {
            globalStats.heldFluids[previousPipe.GetHeldFluid().identifier] += (throughput * Time.deltaTime);
        }
        else
        {
            globalStats.heldFluids.Add(previousPipe.GetHeldFluid().identifier, 0f);
        }
    }

    protected void SetAllRendererMaterial(Material mat)
    {
        foreach (Renderer renderer in fluidRenderer)
        {
            renderer.material = mat;
        }
    }    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 1);
    }
}
