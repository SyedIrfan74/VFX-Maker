using UnityEngine;

public class VFXEnum
{
    [System.Serializable]
    public enum VFXOutputEnum
    {
        VFXPlanarPrimitiveOutput,
        VFXMeshOutput,
        VFXURPLitMeshOutput,
        VFXURPLitPlanarPrimitiveOutput,
        VFXShaderGraphMesh,
        VFXShaderGraphQuad
    }

    [System.Serializable]
    public enum VFXRandomSetting
    {
        Off,
        PerComponent,
        Uniform,
    }

    [System.Serializable]
    public enum VFXCompositionSetting
    {
        Overwrite,
        Add,
        Multiply,
        Blend,
    }
}
