using System.Collections.Generic;
using UnityEngine;

public class ProceduralIslandGen : MonoBehaviour
{
    [Header("Perlin")]
    public int height = 256;
    public int width = 256;
    public float scale = 20;
    public float offsetX = 0f;
    public float offsetY = 0f;

    [Header("Non Noise Based Generation")]
    public int mapBottomLeft = -500;
    public int mapTopRight = 500;

    public float lowerHeightBound = -5f;
    public float upperHeightBound = 5f;

    public int islandCount = 500;

    public GameObject[] islandPrefabs;

    [Header("Island Spacing")]
    public float minimumIslandDistance = 20f;

    private List<Vector3> islandPositions =
        new List<Vector3>();

    void Start()
    {
        GenerateMap();

        //offsetX = Random.Range(0, 1000);
        //offsetY = Random.Range(0, 1000);
    }

    void Update()
    {
        //Renderer renderer = GetComponent<Renderer>();
        //renderer.material.mainTexture = GenerateTexture();
    }

    void GenerateMap()
    {
        int attempts = 0;

        // Prevent the generator from trying forever
        int maxAttempts = islandCount * 20;

        while (
            islandPositions.Count < islandCount &&
            attempts < maxAttempts
        )
        {
            attempts++;

            // Pick a random island
            GameObject islandToSpawn =
                islandPrefabs[
                    Random.Range(
                        0,
                        islandPrefabs.Length
                    )
                ];

            // Pick a random position
            float posX =
                Random.Range(
                    mapBottomLeft,
                    mapTopRight
                );

            float posY =
                Random.Range(
                    lowerHeightBound,
                    upperHeightBound
                );

            float posZ =
                Random.Range(
                    mapBottomLeft,
                    mapTopRight
                );

            Vector3 position =
                new Vector3(
                    posX,
                    posY,
                    posZ
                );

            // Check if this position is far
            // enough away from existing islands
            if (!IsPositionValid(position))
                continue;

            // Spawn the island
            Instantiate(
                islandToSpawn,
                position,
                islandToSpawn.transform.rotation,
                transform
            );

            // Remember the position
            islandPositions.Add(position);
        }

        Debug.Log(
            "Generated " +
            islandPositions.Count +
            " islands after " +
            attempts +
            " attempts."
        );
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 existingPosition
                 in islandPositions)
        {
            float distance =
                Vector3.Distance(
                    position,
                    existingPosition
                );

            if (distance < minimumIslandDistance)
            {
                return false;
            }
        }

        return true;
    }

    // NOISE GENERATION, USE AT WILL
    Texture2D GenerateTexture()
    {
        Texture2D texture =
            new Texture2D(
                width,
                height
            );

        // GENERATE PERLIN NOISE
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color colour =
                    CalculateColour(x, y);

                texture.SetPixel(
                    x,
                    y,
                    colour
                );
            }
        }

        texture.Apply();

        return texture;
    }

    Color CalculateColour(int x, int y)
    {
        float xCoord =
            (float)x / width *
            scale +
            offsetX;

        float yCoord =
            (float)y / height *
            scale +
            offsetY;

        float sample =
            Mathf.PerlinNoise(
                xCoord,
                yCoord
            );

        return new Color(
            sample,
            sample,
            sample
        );
    }
}