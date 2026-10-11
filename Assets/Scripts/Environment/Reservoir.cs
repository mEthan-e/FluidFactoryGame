using UnityEngine;

public class Reservoir : MonoBehaviour
{
    public ParticleSystem[] reservoirParticles;
    public LineRenderer[] beacons;
    public Fluid populatedFluid;
    [Tooltip("This is for calculating throughput with a max clamp of the pipes throughput (UNIMPLEMENTED, TODO)")] // do this
    public float fluidDensity; // unused

    void Start()
    {
        foreach (ParticleSystem ps in reservoirParticles)
        {
            ParticleSystemRenderer psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = populatedFluid.material;
            psRenderer.trailMaterial = populatedFluid.material;
        }

        foreach (LineRenderer line in beacons)
        {
            line.startColor = populatedFluid.color;
            line.endColor = populatedFluid.color;
        }

    }
}
