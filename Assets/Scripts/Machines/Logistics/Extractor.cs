using UnityEngine;

public class Extractor : MonoBehaviour
{
    public LayerMask pipeLayer;
    public LayerMask reservoirLayer;
    public Fluid fluidToExtract;
    public Renderer[] fluidRenderer;
    public Material nullMaterial;
    private Pipe nextPipe;
    

    private void SetNeighbouringPipes()
    {
        Ray rayForward = new Ray(transform.position, transform.forward);
        
        if (Physics.Raycast(rayForward, out RaycastHit hit, 1f, pipeLayer))
        {
            if(hit.collider.gameObject.CompareTag("Pipe"))
            {
                nextPipe = hit.collider.gameObject.GetComponent<Pipe>();
            }
        }
    }

    private void Update()
    {
        SetNeighbouringPipes();
        if (nextPipe != null)
        {
            nextPipe.SetHeldFluid(fluidToExtract);
        }
        if (fluidToExtract != null)
        {
            SetAllRendererMaterial(fluidToExtract.material);
        }
        else
        {
            SetAllRendererMaterial(nullMaterial);
        }
        PumpFromReservoir();
    }

    private void PumpFromReservoir()
    {
        if (Physics.Raycast(transform.position, new Vector3(0f, -1f, 0f), out RaycastHit hit, 3f, reservoirLayer))
        {
            fluidToExtract = hit.collider.GetComponent<Reservoir>().populatedFluid;
        }
    }

    protected void SetAllRendererMaterial(Material mat)
    {
        foreach (Renderer renderer in fluidRenderer)
        {
            renderer.material = mat;
        }
    }
    
    
    public Fluid GetHeldFluid()
    {
        return fluidToExtract;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(transform.position, new Vector3(0f, -3f, 0f));
    }
}
