
using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExcludeFromPreset]
public abstract class RoXamiRenderFeature : ScriptableObject, IDisposable
{
    protected abstract void Create();
    
    public abstract void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData);

    public void OnEnable()
    {
        Create();
    }

    public void OnValidate()
    {
        Create();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}