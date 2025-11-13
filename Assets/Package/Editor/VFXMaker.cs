using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.VFX;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// VFX Maker Editor
/// Author: Syed Irfan
/// 
/// Enables users to make VFX simply with button inputs
/// 
/// 
/// TO DO:
/// 1. Make Preset Emitters (Constant Rate, Burst, Spiral, Gravity)
/// 2. Extract commonly used code fragments in 1
/// 3. Enable Users to make Custom Exposed Properties (CEPs)
/// 4. Enable Users to link CEPs to nodes
/// 5. Show all CEPs in Window Editor
/// 6. 
/// 
/// INTERMITTENT PLANS
/// Find out what people like and dont like about Particle System and VFXGraph
/// Keep the likes and make the nots better
/// 
/// FUTURE PLANS:
/// Do Particle System Improvements
/// Link Shader Graph for VFX Graph
/// 
/// </summary>

public class VFXMaker : EditorWindow
{
    [System.Serializable]
    public enum VFXEnumTest
    {
        NONE,
        VFXURPLitMeshOutput,
        VFXURPLitPlanarPrimitiveOutput
    }

    private VFXEnumTest testing;

    private VisualEffectAsset vfx;
    private string path;
    private string newAssetName;

    private int numGraphs;
    private float graphGap = 400.0f;

    [MenuItem("Window/VFXMaker")]
    static void OpenWindow()
    {
        GetWindow<VFXMaker>();
    }
    private void HandleVFXAssetChange(VisualEffectAsset newVFX)
    {
        vfx = newVFX;
        numGraphs = RussiaFall.GetNumGraphs(vfx);
    }

    private void OnGUI()
    {
        GUILayout.Label("Target VFX Asset", EditorStyles.whiteLargeLabel); 

        //Check if field has been changed
        EditorGUI.BeginChangeCheck();
        var newVFX = (VisualEffectAsset)EditorGUILayout.ObjectField(vfx, typeof(VisualEffectAsset), true);
        if (EditorGUI.EndChangeCheck()) HandleVFXAssetChange(newVFX);

        //If no asset is present
        if (vfx == null)
        {
            path = EditorGUILayout.TextField("Asset File Path: ", path);
            newAssetName = EditorGUILayout.TextField("New Asset Name: ", newAssetName);

            if (GUILayout.Button("Create VFX Asset")) vfx = RussiaFall.CreateVFXAsset(path, newAssetName, vfx);
        }

        EditorGUILayout.Separator();

        if (vfx != null)
        {
            GUILayout.Label("Presets", EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Constant Emitter")) RussiaFall.GenerateConstantEmitter(vfx);
            if (GUILayout.Button("Burst Emitter")) RussiaFall.GenerateBurstEmitter(vfx);
            if (GUILayout.Button("Spiral Emitter")) RussiaFall.GenerateSpiralEmitter(vfx);
            if (GUILayout.Button("Gravity Emitter")) RussiaFall.GenerateGravityEmitter(vfx);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();


        EditorGUILayout.EnumFlagsField(testing);
    }
}













//private GUILayoutOption[] titles = { };
//private bool showPresets;
//private bool overrideGraph;

//EditorStyles.largeLabel, EditorStyles.boldLabel
//GUILayout.Label("Presets");

//GUILayout.BeginHorizontal();

//if (GUILayout.Button("Constant Emitter")) RussiaFall.GenerateConstantEmitter(vfx);
//if (GUILayout.Button("Burst Emitter")) RussiaFall.GenerateBurstEmitter(vfx);
//if (GUILayout.Button("Spiral Emitter")) RussiaFall.GenerateSpiralEmitter(vfx);
//if (GUILayout.Button("Gravity Emitter")) RussiaFall.GenerateGravityEmitter(vfx);

//GUILayout.EndHorizontal();



//if (vfx != null)
//{
//    GUILayout.BeginHorizontal();
//    showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true, EditorStyles.boldLabel);

//    EditorGUI.BeginChangeCheck();
//    var newOverrideGraph = EditorGUILayout.Toggle("Override Graph", overrideGraph, GUILayout.ExpandWidth(true));
//    if (EditorGUI.EndChangeCheck()) overrideGraph = newOverrideGraph;
//    GUILayout.EndHorizontal();

//    //EditorGUILayout.Space();

//    if (showPresets)
//    {

//    }
//}