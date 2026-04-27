using UnityEngine;

public class PoolAbleObject : MonoBehaviour
{
    public ObjectPool _parent;

    public virtual void OnDisable() {
        _parent.ReturnObjectToPool(this);
    }
}
