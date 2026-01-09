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
        VFXShaderGraphQuad,
        VFXQuadStripOutput
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
        Blend_NOTWORKING,
    }

    [System.Serializable]
    public enum VFXContextTarget
    {
        Spawn,
        Init,
        Update,
        Output
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
