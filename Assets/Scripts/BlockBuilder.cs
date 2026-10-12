
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
    public float offset = 1f;

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

    [Header("Obstacle Detection")]
    public LayerMask obstacleMask;
    public Vector3 placementCheckSize = new Vector3(0.9f, 0.9f, 0.9f);
    public Vector3 placementCheckOffset = new Vector3(0f, 0.45f, 0f);

    [Header("Sound")]
    public AudioSource placeSound;

    private Dictionary<Vector2Int, GameObject> placedBlocks =
        new Dictionary<Vector2Int, GameObject>();

    private GameObject previewBlock;

    private int rotationIndex = 0;
    private bool isMirrored = false;

    private GameObject lastPreviewPrefab;

    private MaterialPropertyBlock materialPropertyBlock;

    private Color lastPreviewColor;
    private bool hasPreviewColor = false;

    // Original entry and exit transforms
    private Vector3 originalEntryPosition;
    private Vector3 originalExitPosition;
    private Quaternion originalEntryRotation;
    private Quaternion originalExitRotation;

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

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 hitPos = hit.point;

            int gx = Mathf.RoundToInt(hitPos.x / gridSize);
            int gz = Mathf.RoundToInt(hitPos.z / gridSize);

            float groundY = hit.collider.bounds.max.y + offset;

            Vector3 previewPos = new Vector3(
                gx * gridSize,
                groundY,
                gz * gridSize
            );

            previewBlock.SetActive(true);
            previewBlock.transform.position = previewPos;

            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);
            ApplyPreviewEntryExit();

            if (IsPlacementValid(previewPos))
                SetPreviewColor(Color.green);
            else
                SetPreviewColor(Color.red);
        }
        else
        {
            previewBlock.SetActive(false);
        }
    }

    bool IsPlacementValid(Vector3 position)
    {
        int gx = Mathf.RoundToInt(position.x / gridSize);
        int gz = Mathf.RoundToInt(position.z / gridSize);

        Vector2Int gridPos = new Vector2Int(gx, gz);

        // Prevent placement on existing blocks
        if (placedBlocks.ContainsKey(gridPos))
            return false;

        // Rotate the offset with the preview
        Quaternion rotation = previewBlock.transform.rotation;

        Vector3 checkCenter =
            position + rotation * placementCheckOffset;

        // Check for overlapping obstacle colliders
        bool hasObstacle = Physics.CheckBox(
            checkCenter,
            placementCheckSize * 0.5f,
            rotation,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !hasObstacle;
    }

    void SetPreviewColor(Color c)
    {
        if (hasPreviewColor && c == lastPreviewColor)
            return;

        lastPreviewColor = c;
        hasPreviewColor = true;

        Renderer[] renderers =
            previewBlock.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetColor("_BaseColor", c);

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
            ApplyPreviewEntryExit();
        }
    }

    void ToggleMirror()
    {
        isMirrored = !isMirrored;

        if (previewBlock != null)
        {
            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);
            ApplyPreviewEntryExit();
        }
    }

    void ApplyRotation(Transform t)
    {
        float baseY = rotationIndex * 90f;

        t.localRotation = Quaternion.Euler(0f, baseY, 0f);
    }

    void ApplyModelMirror(Transform root)
    {
        if (root.childCount == 0)
            return;

        Transform model = root.GetChild(0);

        Vector3 s = model.localScale;

        s.z = Mathf.Abs(s.z) * (isMirrored ? -1 : 1);

        model.localScale = s;
    }

    void CacheEntryExit()
    {
        Transform entry =
            FindChildRecursive(previewBlock.transform, entryPointName);

        Transform exit =
            FindChildRecursive(previewBlock.transform, exitPointName);

        if (entry == null || exit == null)
            return;

        originalEntryPosition = entry.localPosition;
        originalExitPosition = exit.localPosition;

        originalEntryRotation = entry.localRotation;
        originalExitRotation = exit.localRotation;
    }

    void ApplyPreviewEntryExit()
    {
        if (previewBlock == null)
            return;

        Transform entry =
            FindChildRecursive(previewBlock.transform, entryPointName);

        Transform exit =
            FindChildRecursive(previewBlock.transform, exitPointName);

        if (entry == null || exit == null)
            return;

        if (isMirrored)
        {
            entry.localPosition = originalExitPosition;
            exit.localPosition = originalEntryPosition;

            entry.localRotation = originalExitRotation;
            exit.localRotation = originalEntryRotation;
        }
        else
        {
            entry.localPosition = originalEntryPosition;
            exit.localPosition = originalExitPosition;

            entry.localRotation = originalEntryRotation;
            exit.localRotation = originalExitRotation;
        }
    }

    void SwapEntryExit(Transform root)
    {
        Transform entry =
            FindChildRecursive(root, entryPointName);

        Transform exit =
            FindChildRecursive(root, exitPointName);

        if (entry == null || exit == null)
            return;

        Vector3 tempPos = entry.localPosition;

        entry.localPosition = exit.localPosition;
        exit.localPosition = tempPos;

        Quaternion tempRot = entry.localRotation;

        entry.localRotation = exit.localRotation;
        exit.localRotation = tempRot;
    }

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
        if (previewBlock == null || !previewBlock.activeSelf)
            return;

        Vector3 pos = previewBlock.transform.position;

        int gx = Mathf.RoundToInt(pos.x / gridSize);
        int gz = Mathf.RoundToInt(pos.z / gridSize);

        Vector2Int gridPos = new Vector2Int(gx, gz);

        // Prevent placement inside obstacles or existing blocks
        if (!IsPlacementValid(pos))
            return;

        if (blockPrefab == null)
            return;

        GameObject block = Instantiate(
            blockPrefab,
            pos,
            Quaternion.identity
        );

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

        // Reset mirroring after placement
        if (isMirrored)
            ToggleMirror();
    }

    void TryDeleteBlock()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, blockMask))
        {
            // Find the registered root block, even when
            // the ray hits a collider on a child object.
            GameObject block = null;
            Vector2Int gridPos = default;

            foreach (var pair in placedBlocks)
            {
                if (pair.Value == null)
                    continue;

                Transform root = pair.Value.transform;

                if (hit.collider.transform == root ||
                    hit.collider.transform.IsChildOf(root))
                {
                    block = pair.Value;
                    gridPos = pair.Key;
                    break;
                }
            }

            if (block != null)
            {
                placedBlocks.Remove(gridPos);
                Destroy(block);
            }
        }
    }

    void RebuildPreview()
    {
        if (previewBlock != null)
            Destroy(previewBlock);

        previewBlock = null;

        if (previewPrefab != null)
        {
            previewBlock = Instantiate(previewPrefab);

            previewBlock.SetActive(false);

            CacheEntryExit();

            ApplyRotation(previewBlock.transform);
            ApplyModelMirror(previewBlock.transform);
            ApplyPreviewEntryExit();

            hasPreviewColor = false;
        }
    }

    // Visualise obstacle detection in the Scene view
    void OnDrawGizmosSelected()
    {
        if (previewBlock == null || !previewBlock.activeSelf)
            return;

        Quaternion rotation = previewBlock.transform.rotation;

        Vector3 checkCenter =
            previewBlock.transform.position +
            rotation * placementCheckOffset;

        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            checkCenter,
            rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, placementCheckSize);

        Gizmos.matrix = oldMatrix;
    }
}
