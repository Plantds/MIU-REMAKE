using System;
using Unity.Mathematics;
using UnityEngine;

public struct CameraInput
{
    public Vector2 lookVec;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitvity = 0.1f;
    private Vector3 _eulerAngles;
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;

        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles += new Vector3(-input.lookVec.y, input.lookVec.x) * sensitvity;
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89.0f, 89.0f);
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
