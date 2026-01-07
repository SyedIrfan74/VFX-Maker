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

    [System.Serializable]
    public enum VFXAttributes
    {
        position,
        velocity,
        color,
        alpha,
        size,
        scale,
        angle

        //oldPosition,
        //direction,
        //lifetime, 
        //age,
        //texIndex,
        //meshIndex,
        //axisX,
        //axisY,
        //axisZ,
        //alive,
        //mass,
        //targetpostiion,
        //angularVelocity,
        //pivot,
    }
}
