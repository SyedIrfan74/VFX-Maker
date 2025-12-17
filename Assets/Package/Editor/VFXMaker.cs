using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Graphs;
using UnityEditor.VersionControl;
using UnityEditor.VFX;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

/// <summary>
/// VFX Maker Editor
/// Author: Syed Irfan
/// 
/// Enables users to make VFX simply with button inputs
/// 
/// 
/// TO DO:
/// 1. Auto Link Nodes together when spawning individually
/// 2. Add more behaviour modules
/// 3. 
/// 4. 
/// 5. 
/// 6. Extract commonly used code fragments
/// 
/// 
/// 
/// FUTURE PLANS:
/// Do Particle System Improvements
/// Link Shader Graph for VFX Graph
/// 
/// </summary>

public class VFXMaker : EditorWindow
{
    [System.Serializable]
    public enum VFXOutputEnum
    {
        None,
        VFXURPLitMeshOutput,
        VFXURPLitPlanarPrimitiveOutput
    }

    [System.Serializable]
    public enum VFXRandomSetting
    {
        None,
        Off,
        PerComponent,
        Uniform,
    }

    [System.Serializable]
    public enum VFXCompositionSetting
    {
        None,
        Overwrite,
        Add,
        Multiply,
        Blend,
    }

    private VFXOutputEnum outputEnum;
    private VFXRandomSetting randomSetting;
    private VFXCompositionSetting compositionSetting;
    private bool overrule;

    private VisualEffectAsset vfx;
    private string path;
    private string newAssetName;

    private string selectedName;

    private List<ExposedPropertyInfo> props = new List<ExposedPropertyInfo>();
    private Vector2 scroll;

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
        if (vfx != null) numGraphs = RussiaFall.GetNumGraphs(vfx);
        props.Clear();
        Refresh();
        selectedName = null; 
    }

    private void OnEnable()
    {
        InspectorWatcher.OnParameterSelected += OnParamSelected;
    }
    private void OnDisable()
    {
        InspectorWatcher.OnParameterSelected -= OnParamSelected;
    }

    void OnParamSelected(VFXParameter p)
    {
        selectedName = p.exposedName;
        p.exposedName = "hah";
        Repaint();
    }

    private void OnGUI()
    {
        if (VFXReloadGuard.NeedsRefresh)
        {
            return;
        }

        Refresh();

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

        //If an Asset is detected
        if (vfx != null)
        {
            //PRESETS START ------------------------------------------------------------------------------------------------------------
            GUILayout.BeginHorizontal();
            GUILayout.Label("Presets", EditorStyles.whiteLargeLabel);

            EditorGUI.BeginChangeCheck();
            overrule = EditorGUILayout.Toggle("Override", overrule);
            EditorGUI.EndChangeCheck();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Constant Emitter")) RussiaFall.GenerateConstantEmitter(vfx, overrule);
            if (GUILayout.Button("Burst Emitter")) RussiaFall.GenerateBurstEmitter(vfx);
            if (GUILayout.Button("Spiral Emitter")) RussiaFall.GenerateSpiralEmitter(vfx);
            if (GUILayout.Button("Gravity Emitter")) RussiaFall.GenerateGravityEmitter(vfx);
            GUILayout.EndHorizontal();
            //PRESETS END ------------------------------------------------------------------------------------------------------------

            EditorGUILayout.Separator();

            //MODULES START ------------------------------------------------------------------------------------------------------------
            
            //Settings Start
            GUILayout.BeginHorizontal();
            GUILayout.Label("Modules", EditorStyles.whiteLargeLabel);
            EditorGUI.BeginChangeCheck();
            randomSetting = (VFXRandomSetting)EditorGUILayout.EnumPopup(randomSetting);
            compositionSetting = (VFXCompositionSetting)EditorGUILayout.EnumPopup(compositionSetting);
            outputEnum = (VFXOutputEnum)EditorGUILayout.EnumPopup(outputEnum);
            EditorGUI.EndChangeCheck();
            GUILayout.EndHorizontal();
            //Settings End

            //Contexts Start
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn Context")) RussiaFall.SpawnModule(vfx);
            if (GUILayout.Button("Initialise Context")) RussiaFall.InitialiseModule(vfx);
            if (GUILayout.Button("Update Context")) RussiaFall.UpdateModule(vfx);
            if (GUILayout.Button("Output Context")) RussiaFall.OutputModule(vfx, outputEnum);
            GUILayout.EndHorizontal();
            //Contexts End

            //Spawn / Init Start
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Constant Spawn Rate")) RussiaFall.ConstantModule(vfx);
            if (GUILayout.Button("Burst Spawn")) RussiaFall.BurstModule(vfx);
            if (GUILayout.Button("Velocity")) RussiaFall.VelocityModule(vfx, randomSetting);
            if (GUILayout.Button("Lifetime")) RussiaFall.LifetimeModule1(vfx, randomSetting, compositionSetting);
            GUILayout.EndHorizontal();
            //Spawn / Init End

            //Update Start
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gravity")) RussiaFall.GravityModule(vfx);
            GUILayout.EndHorizontal();
            //Update End

            //Output Start
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Size")) RussiaFall.SizeModule(vfx, randomSetting);
            if (GUILayout.Button("Orient")) RussiaFall.OrientModule(vfx);
            GUILayout.EndHorizontal();
            //Output End

            //CEP Start
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Exposed Float")) RussiaFall.AddFloatProperty(vfx, "CEPFloat", 100);
            GUILayout.EndHorizontal();
            //CEP End

            //MODULES END ------------------------------------------------------------------------------------------------------------


            //CEP START ------------------------------------------------------------------------------------------------------------
            EditorGUILayout.Separator();

            if (selectedName != null)
            {
                GUILayout.Label("Selected Property:");
                GUILayout.Label(selectedName);

                EditorGUI.BeginChangeCheck();
                if (EditorGUI.EndChangeCheck())
                {
                    selectedName = GUILayout.TextField(selectedName);
                    //RussiaFall.SetExposedName();
                }
            }
            //CEP END ------------------------------------------------------------------------------------------------------------

            //VALUES START ------------------------------------------------------------------------------------------------------------
            EditorGUILayout.LabelField($"Found {props.Count} exposed property(ies):", EditorStyles.boldLabel);

            if (props != null)
            {
                foreach (var p in props)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(p.Name, EditorStyles.boldLabel);

                    object newValue = DrawEditableValueField(p);

                    //If value changed => write back to VFX asset
                    if (newValue != null && !Equals(newValue, p.Value))
                    {
                        RussiaFall.SetExposedValue(vfx, p, newValue);
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            //VALUES END ------------------------------------------------------------------------------------------------------------
        }

        EditorGUILayout.Separator();
    }

    object DrawEditableValueField(ExposedPropertyInfo p)
    {
        var t = p.ValueType;

        if (t == typeof(float))
            return EditorGUILayout.FloatField(p.Name, (float)p.Value);

        if (t == typeof(int))
            return EditorGUILayout.IntField(p.Name, (int)p.Value);

        if (t == typeof(Vector2))
            return EditorGUILayout.Vector2Field(p.Name, (Vector2)p.Value);

        if (t == typeof(Vector3))
            return EditorGUILayout.Vector3Field(p.Name, (Vector3)p.Value);

        if (t == typeof(Vector4))
        {
            Vector4 v4 = (Vector4)p.Value;
            Vector3 v3 = new Vector3(v4.x, v4.y, v4.z);
            float w = v4.w;

            v3 = EditorGUILayout.Vector3Field("XYZ", v3);
            w = EditorGUILayout.FloatField("W", w);

            return new Vector4(v3.x, v3.y, v3.z, w);
        }

        if (t == typeof(Color))
            return EditorGUILayout.ColorField(p.Name, (Color)p.Value);

        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            return EditorGUILayout.ObjectField(p.Name, (UnityEngine.Object)p.Value, t, false);

        if (t == typeof(Gradient))
        {
            return EditorGUILayout.GradientField(p.Name, (Gradient)p.Value);
        }

        if (t == typeof(bool))
        {
            return EditorGUILayout.Toggle(p.Name, (bool)p.Value);
        }

        // Default fallback
        EditorGUILayout.LabelField($"Unsupported type: {t.Name}");
        return null;
    }

    private void Refresh()
    {
        props.Clear();
        if (vfx != null)
            props = RussiaFall.GetExposedProperties(vfx);
    }
}


public static class VFXReloadGuard
{
    public static bool NeedsRefresh { get; private set; }

    [DidReloadScripts]
    private static void OnReload()
    {
        NeedsRefresh = true;
        EditorApplication.update += WaitOneFrame;
    }

    private static void WaitOneFrame()
    {
        NeedsRefresh = false;
        EditorApplication.update -= WaitOneFrame;
    }
}








//if (GUILayout.Button("Constant Spawn Rate")) RussiaFall.ConstantModule(vfx);
//if (GUILayout.Button("Burst Spawn")) RussiaFall.BurstModule(vfx);
//if (GUILayout.Button("Gravity")) RussiaFall.GravityModule(vfx);
//if (GUILayout.Button("Lifetime")) RussiaFall.LifetimeModule(vfx, randomSetting);

//if (GUILayout.Button("Refresh"))
//    Refresh();

//scroll = EditorGUILayout.BeginScrollView(scroll);
//foreach (var p in props)
//{
//    EditorGUILayout.BeginVertical("box");
//    EditorGUILayout.LabelField("Name", p.Name);
//    EditorGUILayout.LabelField("Type", p.ValueType != null ? p.ValueType.FullName : "<unknown>");
//    // Show value nicely depending on type:
//    if (p.ValueType == typeof(float) || (p.ValueType == null && p.Value is float))
//        EditorGUILayout.LabelField("Value", p.Value.ToString());
//    else if (p.ValueType == typeof(int) || (p.ValueType == null && p.Value is int))
//        EditorGUILayout.LabelField("Value", p.Value.ToString());
//    else if (p.ValueType == typeof(Vector3) || (p.ValueType == null && p.Value is Vector3))
//        EditorGUILayout.Vector3Field("Value", (Vector3)(p.Value ?? Vector3.zero));
//    else if (p.ValueType == typeof(Color) || (p.ValueType == null && p.Value is Color))
//        EditorGUILayout.ColorField("Value", (Color)(p.Value ?? Color.white));
//    else if (p.ValueType == typeof(Gradient) || (p.ValueType == null && p.Value is Gradient))
//        EditorGUILayout.LabelField("Value", "<Gradient>");
//    else if (p.ValueType != null && typeof(UnityEngine.Object).IsAssignableFrom(p.ValueType))
//        EditorGUILayout.ObjectField("Value", p.Value as UnityEngine.Object, p.ValueType, false);
//    else
//        EditorGUILayout.LabelField("Value", p.Value?.ToString() ?? "<null>");

//    EditorGUILayout.EndVertical();
//}
//EditorGUILayout.EndScrollView();


//scroll = EditorGUILayout.BeginScrollView(scroll);
//EditorGUILayout.EndScrollView();




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