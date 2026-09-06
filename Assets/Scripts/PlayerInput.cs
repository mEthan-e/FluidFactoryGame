using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float keyboardPanSpeed = 5f;
    
    private CinemachineFollow cinemachineFollow;
    private Vector3 startingFollowOffset;
    Vector3 targetPos;


    private void Awake()
    {
        if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("CinemachineCamera unassigned!");
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
    }
    
    private void Update()
    {
        DoPanning();
        DoZoom();
    }

    private void DoZoom()
    {
        targetPos = cameraTarget.position;
        Vector2 scrollValue = Mouse.current.scroll.ReadValue();
        
        if (scrollValue.y >= 0f)
        {
            cameraTarget.position = new Vector3(cameraTarget.position.x, cameraTarget.position.y - 2f, cameraTarget.position.z + 2f);
        }
        
        if (scrollValue.y <= 0f)
        {
            cameraTarget.position = new Vector3(cameraTarget.position.x, cameraTarget.position.y + 2f, cameraTarget.position.z - 2f);
        }
        
    }

    private void DoPanning()
    {
        Vector2 moveAmout = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
        {
            moveAmout.y += keyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            moveAmout.x -= keyboardPanSpeed;
        }
        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
        {
            moveAmout.y -= keyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            moveAmout.x += keyboardPanSpeed;
        }

        moveAmout *= Time.deltaTime;
        cameraTarget.position += new Vector3(moveAmout.x, 0, moveAmout.y);
    }
}
