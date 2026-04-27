using UnityEngine;

public class Cover : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(transform.position, Vector3.one*0.3f);        
    }
}
