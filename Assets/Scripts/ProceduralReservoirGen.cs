using UnityEngine;

public class ProceduralReservoirGen : MonoBehaviour
{
    public GameObject reservoirPrefab;
    public int reservoirCount;
    public Vector3 planeSize;

    void Start()
    {
        for (int i = 0; i < reservoirCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range((0 - planeSize.x) / 2, (0 - planeSize.x) / 2), 0, Random.Range((0 - planeSize.z) / 2, (0 - planeSize.z) / 2));
            reservoirPrefab.Instantiate(pos);
        }
    }
}
