using UnityEngine;

public class Reservoir : MonoBehaviour
{
    public ParticleSystem reservoirParticles;
    public Fluid populatedFluid;
    [Tooltip("This is for calculating throughput with a max clamp of the pipes throughput (UNIMPLEMENTED, TODO)")] // do this
    public float fluidDensity; // unused

    void Start()
    {
        ParticleSystemRenderer psRenderer = reservoirParticles.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = populatedFluid.material;
        psRenderer.trailMaterial = populatedFluid.material;
    }
}
