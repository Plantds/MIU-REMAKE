using UnityEngine;
using KinematicCharacterController;

public enum CrouchInput
{
    None, toggle, Crouch, Uncrouch
}
public enum RunInput
{
    None, toggle, Run, Unrun
}

public enum Stance
{
    Stand, Crouch, sliding, Running, Wallruning, Dashing
}

public struct CharacterStance
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
    public RunInput Run;
    public bool Dash;

}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [Header("Miscellaneous")]
    [SerializeField] private float gravity = -90.0f;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform root;

    private Collider[] _uncrouchOverlapResults;

    [Space]
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 15.0f;
    [SerializeField] private float walkResponse = 25.0f;

    [Header("Running")]
    [SerializeField] private float runSpeed = 30.0f;
    [SerializeField] private float runResponse = 30.0f;


    [Header("Jump")]
    [SerializeField] private float jumpSpeed = 20.0f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 7.0f;
    [SerializeField] private float crouchResponse = 20.0f;
    [SerializeField] private float crocuhHight = 0.5f;
    [SerializeField] private float standingHight = 1.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float crouhCameraHeightTarget = 0.7f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float standingCameraHeightTarget = 0.9f;
    [SerializeField] private float crouchHeightResponse = 15.0f;
    [SerializeField] private float slideFriction = 0.8f;

    [Header("Sliding")]
    [SerializeField] private float slideStartSpeed = 25.0f;
    [SerializeField] private float slideEndSpeed = 15.0f;
    [SerializeField] private float slideSteerAcceleration = 5.0f;
    [SerializeField] private float slideGravity = -90.0f;
    [SerializeField] private float slideStartFreshold = 800.0f;

    [Header("Wallrunning")]
    [SerializeField] private float wallrunStartSpeed = 30.0f;
    [SerializeField] private float wallrunEndSpeed = 15.0f;
    [SerializeField] private float wallrunFrechold = 800.0f;
    [SerializeField] private float wallStartDistance = 1.5f;
    [SerializeField] private float wallrunResponse = 30.0f;
    [SerializeField] private float wallrunColdownTime = 0.3f;
    [SerializeField] private LayerMask wall;

    [Header("Dashing")]
    [SerializeField] private float groundDashSpeed = 45.0f;
    [SerializeField] private float airDashSpeed = 45.0f;
    [SerializeField] private float dashRecoveryTime = 3.0f;
    [SerializeField] private float dashGravityTime = 0.3f;
    [SerializeField] private int maxDashes = 2;
    private float dashRecoveryTimer = 0.0f;
    private float dashGravityTimer = 0.0f;
    private int currentDashesLeft = 0;

    [Header("air")]
    [SerializeField] private float airSpeed = 15.0f;
    [SerializeField] private float airAcceleration = 70.0f;


    private CharacterStance _state;
    private CharacterStance _lastState;
    private CharacterStance _tempState;

    private Quaternion _requestedRotation;
    private Vector3 _requestedmovement;
    
    private bool _requestedJump;
    private bool _requestedSustainJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _ungrounedDueToJump;
    private bool _requestedRun;
    private bool _requestedDash;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;

    private float _wallrunColdownTimer = 0.0f;
    private bool _isGettingMovementInput;
    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;
        _uncrouchOverlapResults = new Collider[8];

        currentDashesLeft = maxDashes;
        dashRecoveryTimer = dashRecoveryTime;
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        _requestedmovement = new Vector3(input.Move.x, 0.0f, input.Move.y);
        _requestedmovement = Vector3.ClampMagnitude(_requestedmovement, 1.0f);
        _requestedmovement = input.Rotation * _requestedmovement;

        _isGettingMovementInput = input.Move.magnitude > 0.0f;

        bool wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequestingJump) _timeSinceJumpRequest = 0.0f;

        _requestedSustainJump = input.Jump;

        if (input.Dash) _requestedDash = true;

        if (currentDashesLeft < maxDashes) dashRecoveryTimer -= Time.deltaTime;
        if (dashRecoveryTimer < 0.0f)
        {
            currentDashesLeft++;
            dashRecoveryTimer = dashRecoveryTime;
        }
        dashGravityTimer -= Time.deltaTime;
        if (_requestedDash && currentDashesLeft > 0) dashGravityTimer = dashGravityTime;

        if (_state.Stance is not Stance.Wallruning) _wallrunColdownTimer -= Time.deltaTime;

        bool wasReqestingCrouch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,

            CrouchInput.Crouch => true,
            CrouchInput.Uncrouch => false,
            _ => _requestedCrouch
        };

        _requestedRun = input.Run switch
        {
            RunInput.toggle => !_requestedRun,
            RunInput.None => _requestedRun,

            RunInput.Run => true,
            RunInput.Unrun => false,
            _ => _requestedRun
        };

        if (_requestedCrouch && !wasReqestingCrouch) _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasReqestingCrouch) _requestedCrouchInAir = false; 
    }
    public void UpdateBody(float deltaTime)
    {
        float currentHeight = motor.Capsule.height;
        float normalizedHeight = currentHeight / standingHight;

        float cameraTargetHeight = currentHeight * (_state.Stance is Stance.Stand ? standingCameraHeightTarget : crouhCameraHeightTarget);
        Vector3 rootTragetScale = new Vector3(1.0f, normalizedHeight, 1.0f);

        cameraTarget.localPosition = Vector3.Lerp(
            a: cameraTarget.localPosition,
            b: new Vector3(0.0f, cameraTargetHeight, 0.0f),
            t: 1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime));

        root.localScale = Vector3.Lerp(
            a: root.localScale,
            b: rootTragetScale,
            t: 1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime));
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        if (_requestedRun && _state.Stance is Stance.Stand && !_requestedCrouch)
        {
            _state.Stance = Stance.Running;
        }

        if (_requestedCrouch && (_state.Stance is Stance.Stand || _state.Stance is Stance.Running))
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crocuhHight,
                yOffset: crocuhHight * 0.5f
            );

            root.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _state.Stance is not Stance.Stand && _state.Stance is not Stance.Running && _state.Stance is not Stance.Wallruning)
        {
            motor.SetCapsuleDimensions(
               radius: motor.Capsule.radius,
               height: standingHight,
               yOffset: standingHight * 0.5f
           );

            if (motor.CharacterOverlap(motor.TransientPosition, motor.TransientRotation, _uncrouchOverlapResults, motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                _requestedCrouch = true;
                motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crocuhHight,
                yOffset: crocuhHight * 0.5f
                );
            }
            else
            {
                root.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
                _state.Stance = Stance.Stand;
            }
        }

        if (!_requestedRun && _state.Stance is Stance.Running)
        {
            _state.Stance = Stance.Stand;
        }

        _state.Velocity = motor.Velocity;
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _lastState = _tempState;
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        _state.Acceleration = Vector3.ProjectOnPlane(_state.Acceleration, hitNormal);
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.sliding) _state.Stance = Stance.Crouch;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;
        Vector3 groundedMovement = motor.GetDirectionTangentToSurface(direction: _requestedmovement, surfaceNormal: motor.GroundingStatus.GroundNormal) * _requestedmovement.magnitude;
        bool moving = groundedMovement.sqrMagnitude > 0;
        if(_state.Stance is Stance.Wallruning) _timeSinceUngrounded = 0.0f;
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0.0f;
            _ungrounedDueToJump = false;


            bool running = _state.Stance is Stance.Running;
            var shouldSlideStart = currentVelocity.magnitude > slideStartFreshold;
            var crouching = _state.Stance is Stance.Crouch;
            var wasStanding = _lastState.Stance is Stance.Stand;
            var wasRuning = _lastState.Stance is Stance.Running;
            var wasInAir = !_lastState.Grounded;
            // start slide
            if (moving && crouching && shouldSlideStart && (wasStanding || wasInAir || wasRuning))
            {
                _state.Stance = Stance.sliding;

                if (wasInAir)
                {
                    Vector3 groundNormal = motor.GroundingStatus.GroundNormal;
                    Vector3 tangent = Vector3.ProjectOnPlane(currentVelocity, groundNormal);

                    // Keep tangential (momentum along slope), discard into-ground velocity
                    if (tangent.sqrMagnitude > 0.001f)
                        currentVelocity = tangent.normalized * currentVelocity.magnitude;
                }

                float effectiveSlideStartSpeed = slideStartSpeed;
                if (!_lastState.Grounded && !_requestedCrouchInAir)
                {
                    effectiveSlideStartSpeed = 0.0f;
                    _requestedCrouchInAir = false;
                }
                float slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                currentVelocity = motor.GetDirectionTangentToSurface(
                    direction: currentVelocity,
                    surfaceNormal: motor.GroundingStatus.GroundNormal) * slideSpeed;

            }

            if (_state.Stance is Stance.Stand or Stance.Crouch or Stance.Running)
            {
                float speed = _state.Stance switch
                {
                    Stance.Stand => walkSpeed,
                    Stance.Crouch => crouchSpeed,
                    Stance.Running => runSpeed,
                    _ => walkSpeed
                };
                float response = _state.Stance switch
                {
                    Stance.Stand => walkResponse,
                    Stance.Crouch => crouchResponse,
                    Stance.Running => runResponse,
                    _ => walkResponse
                };

                if (!_requestedDash)
                {
                    Vector3 tragetVelocity = groundedMovement * speed;
                    Vector3 moveVelocity = Vector3.Lerp(
                        a: currentVelocity,
                        b: tragetVelocity,
                        t: 1.0f - Mathf.Exp(-response * deltaTime)
                    );
                    _state.Acceleration = moveVelocity - currentVelocity;
                    currentVelocity = moveVelocity;
                }
            }
            else
            {
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                Vector3 forceOnSlope = Vector3.ProjectOnPlane(
                    vector: -motor.CharacterUp,
                    planeNormal: motor.GroundingStatus.GroundNormal
                ) * slideGravity;

                currentVelocity -= forceOnSlope * deltaTime;

                float currentSpeed = currentVelocity.magnitude;
                Vector3 targetVelocity = groundedMovement * currentVelocity.magnitude;
                Vector3 steerVelocity = currentVelocity;
                Vector3 steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;
                steerVelocity += steerForce;
                steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                currentVelocity = steerVelocity;

                if (currentVelocity.magnitude < slideEndSpeed) _state.Stance = Stance.Crouch;
            }
        }
        else if (_state.Stance is not Stance.Wallruning)
        { // air 
            _timeSinceUngrounded += deltaTime;
            if (_requestedmovement.sqrMagnitude > 0.0f)
            {
                Vector3 planarMovement = Vector3.ProjectOnPlane(
                    vector: _requestedmovement,
                    planeNormal: motor.CharacterUp) * _requestedmovement.magnitude;

                Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp);

                Vector3 movementForce = planarMovement * airAcceleration * deltaTime;

                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0.0f)
                {
                    Vector3 constrainedMovementForce = Vector3.ProjectOnPlane(
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                if (motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0.0f)
                    {
                        Vector3 obstuctionNormal = Vector3.Cross(
                            motor.CharacterUp,
                            Vector3.Cross(
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )
                        ).normalized;

                        movementForce = Vector3.ProjectOnPlane(movementForce, obstuctionNormal);
                    }
                }

                currentVelocity += movementForce;
            }

            float effectiveGravity = gravity;

            float verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSustainJump && verticalSpeed > 0.0f) effectiveGravity *= jumpSustainGravity;
            if (dashGravityTimer > 0.0f) effectiveGravity = 0.0f;
            if (_state.Stance is Stance.Wallruning) effectiveGravity = 0.0f;

            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;

        }

        RaycastHit rightHit;
        RaycastHit leftHit;
        bool rightWall = Physics.Raycast(transform.position, motor.CharacterRight, out rightHit, wallStartDistance, wall);
        bool leftWall = Physics.Raycast(transform.position, -motor.CharacterRight, out leftHit, wallStartDistance, wall);


        if (_requestedJump)
        {
            if (_state.Stance is not Stance.Wallruning)
            {
                bool grounded = motor.GroundingStatus.IsStableOnGround;
                bool canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungrounedDueToJump;
                if (grounded || canCoyoteJump)
                {
                    _requestedJump = false;
                    _requestedCrouch = false;
                    _requestedCrouchInAir = false;

                    motor.ForceUnground(time: 0.0f);
                    _ungrounedDueToJump = true;

                    float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                    float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                    currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                }
            }
            else if (_state.Stance is Stance.Wallruning)
            {
                bool canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungrounedDueToJump;
                if (canCoyoteJump)
                {
                    _wallrunColdownTimer = wallrunColdownTime;
                    _requestedJump = false;

                    _ungrounedDueToJump = true;

                    Vector3 wallNormal = rightWall ? rightHit.normal : leftHit.normal;
                    Vector3 combinedDir = motor.CharacterUp + (wallNormal * 0.3f);

                    currentVelocity += combinedDir * jumpSpeed;
                }
            }
            else
            {
                bool canJumpLater = _timeSinceJumpRequest < coyoteTime;
                _requestedJump = canJumpLater;
                _timeSinceJumpRequest += deltaTime;
            }
        }

        if (_requestedDash && currentDashesLeft > 0)
        {
            if (_state.Stance is Stance.Wallruning)
            {
                _state.Stance = Stance.Dashing;
                _wallrunColdownTimer = wallrunColdownTime;
                Vector3 wallNormal = rightWall ? rightHit.normal : leftHit.normal; // normals can at time point towards the wall fuck

                Vector3 comebinedDir = (wallNormal * 0.3f) + motor.CharacterForward.normalized;

                currentVelocity = new Vector3(0.0f, currentVelocity.y, currentVelocity.z);

                currentVelocity += comebinedDir * airDashSpeed;
            }
            else
            {
                _state.Stance = Stance.Dashing;
                Vector3 groundedForward = motor.GetDirectionTangentToSurface(direction: motor.CharacterForward, surfaceNormal: motor.GroundingStatus.GroundNormal);
                motor.ForceUnground(time: 0.0f);

                float effectiveDashSpeed = motor.GroundingStatus.IsStableOnGround ? groundDashSpeed : airDashSpeed;

                if (groundedMovement.magnitude > 0.0f) currentVelocity += groundedMovement * effectiveDashSpeed;
                else currentVelocity += groundedForward * effectiveDashSpeed;
            }

            currentDashesLeft--;
            _requestedDash = false;
        }
        if (currentDashesLeft == 0) _requestedDash = false;

        // wallruning
        if (!motor.GroundingStatus.IsStableOnGround && _wallrunColdownTimer < 0.0f && _state.Stance is not Stance.Dashing && !_requestedJump && _isGettingMovementInput && (currentVelocity.sqrMagnitude > wallrunFrechold || _state.Stance is Stance.Running) && (rightWall || leftWall))
        {
            {// start wallrun
                _state.Stance = Stance.Wallruning;
                _ungrounedDueToJump = false;
                currentVelocity = new Vector3(currentVelocity.x, 0.0f, currentVelocity.z);
            }
            {// during wallrun
                Vector3 wallNormal = rightWall ? rightHit.normal : leftHit.normal;
                Vector3 wallForward = Vector3.Cross(wallNormal, motor.CharacterUp);

                float chosenStartSpeed = Mathf.Max(wallrunStartSpeed, currentVelocity.magnitude);
                float effectiveWallrunSpeed = Mathf.Lerp(
                    a: chosenStartSpeed,
                    b: wallrunEndSpeed,
                    t: 1.0f - Mathf.Exp(-walkResponse * deltaTime));

                Vector3 effectiveForward = (motor.CharacterForward - wallForward).magnitude > (motor.CharacterForward - -wallForward).magnitude ? -wallForward : wallForward;

                Vector3 tragetVelocity = (effectiveForward + groundedMovement).normalized * effectiveWallrunSpeed;
                Vector3 moveVelocity = Vector3.Lerp( // not learping shit huh
                    a: currentVelocity,
                    b: tragetVelocity,
                    t: 1.0f - Mathf.Exp(-wallrunResponse * deltaTime)
                );
                _state.Acceleration = moveVelocity - currentVelocity;
                currentVelocity = moveVelocity;
            }
        }
        else if (_state.Stance is Stance.Wallruning && !rightWall && !leftWall || _state.Stance is Stance.Wallruning && !_isGettingMovementInput)
        {
            _wallrunColdownTimer = wallrunColdownTime;
            _state.Stance = Stance.Stand;
        }
        
    }

    public Transform getCameraTarget() => cameraTarget;

    public CharacterStance GetState() => _state;
    public CharacterStance GetLastState() => _lastState;


    public void setPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity) motor.BaseVelocity = Vector3.zero;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.normal.textColor = Color.lightGray;
        GUI.Box(new Rect(5, 5, 200, 200), "PLAYER INFO:", GUI.skin.window);
        GUI.Label(new Rect(10, 20, 750, 40), "grounded: " + _state.Grounded, style);
        GUI.Label(new Rect(10, 35, 750, 40), "currStance: " + _state.Stance, style);
        GUI.Label(new Rect(10, 50, 750, 40), "lastStance: " + _lastState.Stance, style);
        GUI.Label(new Rect(10, 65, 750, 40), "velocity: " + _state.Velocity, style);
        GUI.Label(new Rect(10, 80, 750, 40), "velocity.sqrMagnitude: " + _state.Velocity.sqrMagnitude, style);
        GUI.Label(new Rect(10, 95, 750, 40), "acceleration: " + _state.Acceleration, style);
        GUI.Label(new Rect(10, 110, 750, 40), "#####################");
        GUI.Label(new Rect(10, 125, 750, 40), "dashesLeft: " + currentDashesLeft, style);
        GUI.Label(new Rect(10, 140, 750, 40), "dashRecoveryTimer: " + dashRecoveryTimer, style);
        GUI.Label(new Rect(10, 155, 750, 40), "wallrunColdownTimer: " + _wallrunColdownTimer, style);
        GUI.Label(new Rect(10, 170, 750, 40), "isGettingMovementInput: " + _isGettingMovementInput, style);
    }
}