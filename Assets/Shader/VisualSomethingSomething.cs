using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/VisualSomethingSomething")]
public sealed class VisualSomethingSomething : IPostProcessComponent
{
    

    public bool IsActive()
    {
        throw new NotImplementedException();
    }
}
