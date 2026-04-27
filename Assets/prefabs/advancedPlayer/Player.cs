using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Miscellaneous")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private cutAndParry cutAndParry;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private cameraLean cameraLean;

    [Header("Input")]
    [SerializeField] private bool toggleCrouchSlide = false;
    [SerializeField] private bool toggleRun = false;

    private IA_AdvancedPlayer _inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new IA_AdvancedPlayer();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.getCameraTarget());
        cameraSpring.Initialize();
        cameraLean.Initialize();
        cutAndParry.Initialize();
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        var input = _inputActions.player;
        var deltaTime = Time.deltaTime;

        // get camera input and updates its rotation
        var cameraInput = new CameraInput { lookVec = input.look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        // get character input and update it
        var characterInput = new CharacterInput
        {
            // movement input
            Rotation = playerCamera.transform.rotation,
            Move = input.WASD.ReadValue<Vector2>(),
            Jump = input.jump.WasPressedThisFrame(),
            JumpSustain = input.jump.IsPressed(),
            Crouch = toggleCrouchSlide ? (input.crouch_slide.WasPressedThisFrame() ? CrouchInput.toggle : CrouchInput.None) : (input.crouch_slide.IsPressed() ? CrouchInput.Crouch : CrouchInput.Uncrouch),
            Run = toggleRun ? (input.run.WasPressedThisFrame() ? RunInput.toggle : RunInput.None) : (input.run.IsPressed() ? RunInput.Run : RunInput.Unrun),
            Dash = input.dash.WasPressedThisFrame(),

        };

        var weaponInput = new weaponInput
        {
            // Gameplay input
            Cut = input.cut.WasPressedThisFrame(),
            Parry = input.parry.WasPerformedThisFrame(),
            Sheath = input.Sheath.WasPerformedThisFrame()
        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

        cutAndParry.UpdateWeaponInput(weaponInput);

#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
#endif
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.getCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateSpring(deltaTime, state.Stance is Stance.sliding, state.Acceleration, cameraTarget.up);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.setPosition(position);
    }

    public Vector3 GetPos() => playerCharacter.transform.position;

}
 