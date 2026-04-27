using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Surface", fileName = "Surface")]
public class Surface : ScriptableObject
{
    [Serializable]
    public class SurfaceImpactTypeEffect
    {
        public ImpactType _impactType;
        public SurfaceEffect _surfaceEffect;
    }
    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();
}
