using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)]
    public float maxDistance = 500f;
    public Directional directional = new Directional
    {
        atlasSize = MapSize._2048
    };
}

[System.Serializable]
public struct Directional
{
    public MapSize atlasSize;
}

public enum MapSize
{
    _256 = 256,
    _512 = 512,
    _1024 = 1024,
    _2048 = 2048,
    _4096 = 4096,
    _8192 = 8192,
}