using UnityEngine;

public class cameraLean : MonoBehaviour
{
    [Header("Damping")]
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decaykDamping = 0.3f;
    [SerializeField] private float walkStrength = 0.075f;
    [SerializeField] private float slideStrength = 0.2f;
    [SerializeField] private float strengthResponse = 5f;



    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVel;
    private float _smoothStrength;

    public void Initialize()
    {
        _smoothStrength = walkStrength;
    }

    public void UpdateSpring(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decaykDamping;

        _dampedAcceleration = Vector3.SmoothDamp(
            current: _dampedAcceleration,
            target: planarAcceleration,
            currentVelocity: ref _dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up);

        transform.localRotation = Quaternion.identity;

        var targetStrength = sliding ? slideStrength : walkStrength;

        _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1.0f - Mathf.Exp(-strengthResponse * deltaTime));

        transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
    }
}

