using UnityEngine;
using EasyButtons;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    [Header("Mats")]
    public Renderer[] fluidRenderer;
    public Material nullMat;
    public int matIndex = 0;
    [Header("VFX")]
    public ParticleSystem spewParticles;

    [Header("Raycasts")]
    public LayerMask pipeLayer;
    public string inputPipeTag = "Pipe";
    public string outputPipeTag = "Pipe";
    public float inputRayDistance = 1f;
    public float outRayDistance = 1f;
    [Header("Throughput")]
    public GameObject[] entryPoints;
    public GameObject[] exitPoints;
    [Tooltip("Measured in L/s")]
    public float throughput = 1f;

    protected List<Pipe> nextPipes = new List<Pipe>();
    protected List<Pipe> previousPipes = new List<Pipe>();
    protected Fluid heldFluid;
    protected bool isEmpty = true;
    protected ParticleSystemRenderer spewRenderer;

    private Material currentMaterial;

    [Button]
    public void LogNeighbours()
    {
        Debug.Log(previousPipes[0] + "  " + nextPipes[0]);
    }
    
    private void Update()
    {
        SetNeighbouringPipes(entryPoints, exitPoints);
        ManageThroughput();
        if (nextPipes.Count <= 0 && previousPipes.Count >= 1 && heldFluid != null)
        {
            spewRenderer.material = heldFluid.material;
            if (!spewParticles.isPlaying)
                spewParticles.Play();

        }
        else
            spewParticles.Stop();
    }

    private void Awake()
    {
        spewRenderer = spewParticles.GetComponent<ParticleSystemRenderer>();
    }

    protected virtual void ManageThroughput()
    {
        foreach (Pipe pipe in nextPipes)
        {
            if ((pipe is Pipe || pipe is Machine) && pipe.GetPreviousPipe().Count > 0)
            {
                if (pipe != null && pipe.GetPreviousPipe().Contains(this))
                {
                    pipe.SetHeldFluid(heldFluid);
                }
                if (pipe.GetPreviousPipe().Count > 1) 
                {
                    float nextPipeThrough = 0;
                    foreach (Pipe prevPipe in pipe.GetPreviousPipe())
                    {
                        nextPipeThrough += prevPipe.throughput;
                    }
                    
                    pipe.throughput = nextPipeThrough;
                    continue;
                }
                pipe.throughput = throughput / nextPipes.Count;
            }
        }

        
        isEmpty = heldFluid == null;
        if (isEmpty)
        {
            SetAllRendererMaterial(nullMat);
        }
        else
        {
            SetAllRendererMaterial(heldFluid.material);     
        }
        if (previousPipes.Count == 0)
        {
            heldFluid = null;            
        }

        if (previousPipes.Count <= 1) {return;}
        
        Fluid prevFluidRef = null;
        // LOOPS THROUGH ALL PREV PIPES TO MAKE SURE THEY'RE ALL THE SAME FLUID
        
        foreach (Pipe pipe in previousPipes)
        {
            if (prevFluidRef == null) {prevFluidRef = pipe.GetHeldFluid();}
            
            if (pipe.GetHeldFluid() == prevFluidRef)
            {
                continue;
            }
            else
            {
                heldFluid = null;
                break;
            }
        }
    }

    // SETS THE TWO NEIGHBOURING PIPES
    protected virtual void SetNeighbouringPipes(GameObject[] inputs, GameObject[] outputs)
    {
        nextPipes.Clear();
        previousPipes.Clear();
        foreach (GameObject input in inputs)
        {

            Ray rayForward = new Ray(input.transform.position, input.transform.forward);
            
            if (Physics.Raycast(rayForward, out RaycastHit hit, inputRayDistance, pipeLayer))
            {
                if(hit.collider.gameObject.CompareTag(inputPipeTag) || hit.collider.gameObject.CompareTag("Tunnel"))
                {
                    previousPipes.Add(hit.collider.gameObject.GetComponent<Pipe>());
                }
            }
            
        }
        foreach (GameObject output in outputs)
        {
            Ray rayBackward = new Ray(output.transform.position, output.transform.forward);
            
            if (Physics.Raycast(rayBackward, out RaycastHit hitBack, outRayDistance, pipeLayer))
            {
                if(hitBack.collider.gameObject.CompareTag(outputPipeTag) || hitBack.collider.gameObject.CompareTag("Tunnel"))
                {
                    nextPipes.Add(hitBack.collider.gameObject.GetComponent<Pipe>());
                }
            }        
        }
    }

    protected void SetAllRendererMaterial(Material mat)
    {
        if (currentMaterial == mat)
            return;

        currentMaterial = mat;

        foreach (Renderer renderer in fluidRenderer)
        {
            Material[] mats = renderer.materials;
            mats[matIndex] = mat;
            renderer.materials = mats;
        }
    }

    // ------------------------------------------------------------------- PUBLIC GET FUNCTIONS -------------------------------------------------------------------

    public Fluid GetHeldFluid()
    {
        return heldFluid;
    }

    public List<Pipe> GetNextPipe()
    {
        return nextPipes;
    }

    public List<Pipe> GetPreviousPipe()
    {
        return previousPipes;
    }

    public bool GetPipeState()
    {
        return isEmpty;
    }

    // ------------------------------------------------------------------- PUBLIC SET FUNCTIONS -------------------------------------------------------------------

    public virtual void SetHeldFluid(Fluid fluidToSet)
    {
        heldFluid = fluidToSet;
    }

    // ------------------------------------------------------------------- GIZMOS -------------------------------------------------------------------

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (GameObject point in exitPoints)
        {
            Gizmos.DrawRay(point.transform.position,
            point.transform.forward * 
            outRayDistance);
        }
        Gizmos.color = Color.red;
        foreach (GameObject point in entryPoints)
        {
            Gizmos.DrawRay(point.transform.position, 
            point.transform.forward * 
            inputRayDistance);
        }
        Gizmos.color = Color.yellow;
        
        if (!isEmpty)
            Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}