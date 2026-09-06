using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockBuilder : MonoBehaviour
{
    [Header("Block Settings")]
    public GameObject blockPrefab;
    public GameObject previewPrefab;
    public float gridSize = 1f;
    public float layerY = 0f;

    [Header("Controls")]
    public KeyCode placeKey = KeyCode.Mouse0;
    public KeyCode deleteKey = KeyCode.Mouse1;
    public KeyCode rotateKey = KeyCode.R;
    public KeyCode mirrorKey = KeyCode.F;

    [Header("Entry/Exit Names")]
    public string entryPointName = "EntryPoint";
    public string exitPointName = "ExitPoint";

    [Header("Raycast")]
    public LayerMask groundMask;
    public LayerMask blockMask;

    [Header("Sound")]
    public AudioSource placeSound;

    private Dictionary<Vector2Int, GameObject> placedBlocks =
        new Dictionary<Vector2Int, GameObject>();

    private GameObject previewBlock;
    private int rotationIndex = 0;
    private bool isMirrored = false;

    private GameObject lastPreviewPrefab;

    // Material property block used for preview colours
    private MaterialPropertyBlock materialPropertyBlock;

    // Prevents changing the preview colour every frame
    private Color lastPreviewColor;
    private bool hasPreviewColor = false;

    void Awake()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        lastPreviewPrefab = previewPrefab;
        RebuildPreview();
    }

    void Update()
    {
        if (previewPrefab != lastPreviewPrefab)
        {
            lastPreviewPrefab = previewPrefab;
            RebuildPreview();
        }

        HandlePreview();
        HandleInput();
    }

    void HandleInput()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(placeKey))
            TryPlaceBlock();

        if (Input.GetKeyDown(deleteKey))
            TryDeleteBlock();

        if (Input.GetKeyDown(rotateKey))
            RotatePreview();

        if (Input.GetKeyDown(mirrorKey))
            ToggleMirror();
    }

    void HandlePreview()
    {
        if (previewBlock == null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundMask))
        {
            Vector3 hitPos = hit.point;

            int gx = Mathf.RoundToInt(hitPos.x / gridSize);
            int gz = Mathf.RoundToInt(hitPos.z / gridSize);

            Vector3 previewPos =
                new Vector3(gx * gridSize, layerY, gz * gridSize);

            previewBlock.SetActive(true);
            previewBlock.transform.position = previewPos;

            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);

            if (isMirrored)
                SwapEntryExit(previewBlock.transform);

            // Determine preview colour
            if (placedBlocks.ContainsKey(new Vector2Int(gx, gz)))
                SetPreviewColor(Color.red);
            else
                SetPreviewColor(Color.green);
        }
        else
        {
            previewBlock.SetActive(false);
        }
    }

    void SetPreviewColor(Color c)
    {
        // Don't update the renderers if the colour hasn't changed
        if (hasPreviewColor && c == lastPreviewColor)
            return;

        lastPreviewColor = c;
        hasPreviewColor = true;

        Renderer[] renderers =
            previewBlock.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(materialPropertyBlock);

            // URP / modern Unity shaders
            materialPropertyBlock.SetColor("_BaseColor", c);

            // Apply the property block without creating a material instance
            r.SetPropertyBlock(materialPropertyBlock);
        }
    }

    void RotatePreview()
    {
        rotationIndex = (rotationIndex + 1) % 4;

        if (previewBlock != null)
        {
            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);

            if (isMirrored)
                SwapEntryExit(previewBlock.transform);
        }
    }

    void ToggleMirror()
    {
        isMirrored = !isMirrored;

        if (previewBlock != null)
        {
            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);

            if (isMirrored)
                SwapEntryExit(previewBlock.transform);
        }
    }

    // BLOCK ROTATION
    void ApplyRotation(Transform t)
    {
        float baseY = rotationIndex * 90f;
        t.localRotation = Quaternion.Euler(0f, baseY, 0f);
    }

    // BLOCK MIRRORING
    void ApplyModelMirror(Transform root)
    {
        if (root.childCount == 0)
            return;

        Transform model = root.GetChild(0);

        Vector3 s = model.localScale;
        s.z = Mathf.Abs(s.z) * (isMirrored ? -1 : 1);
        model.localScale = s;
    }

    // SWAP ENTRY/EXIT
    void SwapEntryExit(Transform root)
    {
        Transform entry =
            FindChildRecursive(root, entryPointName);

        Transform exit =
            FindChildRecursive(root, exitPointName);

        if (entry == null || exit == null)
            return;

        // Swap positions
        Vector3 tempPos = entry.localPosition;
        entry.localPosition = exit.localPosition;
        exit.localPosition = tempPos;

        // Swap rotations
        Quaternion tempRot = entry.localRotation;
        entry.localRotation = exit.localRotation;
        exit.localRotation = tempRot;
    }

    // Recursive child finder
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in
            parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    void TryPlaceBlock()
    {
        if (previewBlock == null ||
            !previewBlock.activeSelf)
            return;

        Vector3 pos = previewBlock.transform.position;

        int gx = Mathf.RoundToInt(pos.x / gridSize);
        int gz = Mathf.RoundToInt(pos.z / gridSize);

        Vector2Int gridPos = new Vector2Int(gx, gz);

        if (placedBlocks.ContainsKey(gridPos))
            return;

        GameObject block =
            Instantiate(blockPrefab, pos, Quaternion.identity);

        ApplyRotation(block.transform);
        ApplyModelMirror(block.transform);

        if (isMirrored)
            SwapEntryExit(block.transform);

        placedBlocks.Add(gridPos, block);

        if (placeSound != null && !placeSound.isPlaying)
        {
            placeSound.pitch = Random.Range(0.90f, 1.20f);
            placeSound.Play();
        }

        // Remove the mirror from the next block
        if (isMirrored)
        {
            ToggleMirror();
        }
    }

    void TryDeleteBlock()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray =
            cam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, blockMask))
        {
            GameObject block = hit.collider.gameObject;

            Vector3 pos = block.transform.position;

            int gx = Mathf.RoundToInt(pos.x / gridSize);
            int gz = Mathf.RoundToInt(pos.z / gridSize);

            Vector2Int gridPos =
                new Vector2Int(gx, gz);

            if (placedBlocks.ContainsKey(gridPos))
            {
                Destroy(block);
                placedBlocks.Remove(gridPos);
            }
        }
    }

    void RebuildPreview()
    {
        if (previewBlock != null)
            Destroy(previewBlock);

        if (previewPrefab != null)
        {
            previewBlock = Instantiate(previewPrefab);
            previewBlock.SetActive(false);

            // Force the next colour update
            hasPreviewColor = false;
        }
    }
}