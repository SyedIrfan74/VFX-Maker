using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// VFX Maker Editor
/// Author: Syed Irfan
/// 
/// Enables users to make VFX simply with button inputs
/// 
/// FUTURE PLANS:
/// Do Particle System Improvements
/// Link Shader Graph for VFX Graph
/// 
/// </summary>

public class VFXMaker : EditorWindow
{
    private VisualEffectAsset vfx;
    private string path;
    private string newAssetName;

    private bool overrule;
    private bool addCEPs;

    private VFXEnum.VFXOutputEnum outputEnum;
    private VFXEnum.VFXRandomSetting randomSetting;
    private VFXEnum.VFXCompositionSetting compositionSetting;
    private VFXEnum.VFXAttributes attribute;

    private List<ExposedPropertyInfo> CEPs = new List<ExposedPropertyInfo>();

    private Vector2 scroll;

    [MenuItem("Window/VFXMaker/Spawn Window")]
    static void OpenWindow()
    {
        GetWindow<VFXMaker>();
    }

    private void HandleVFXAssetChange(VisualEffectAsset newVFX)
    {
        vfx = newVFX;
        RefreshCEP();
    }

    private void DrawTitleBar()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Target VFX Asset", EditorStyles.whiteLargeLabel);

        //Check if field has been changed
        EditorGUI.BeginChangeCheck();
        var newVFX = (VisualEffectAsset)EditorGUILayout.ObjectField(vfx, typeof(VisualEffectAsset), true);
        if (EditorGUI.EndChangeCheck()) HandleVFXAssetChange(newVFX);

        EditorGUILayout.EndVertical();
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Settings", EditorStyles.whiteLargeLabel);

        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        overrule = EditorGUILayout.Toggle("Override", overrule);
        addCEPs = EditorGUILayout.Toggle("Auto Add CEPs", addCEPs);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        randomSetting = (VFXEnum.VFXRandomSetting)EditorGUILayout.EnumPopup(randomSetting);
        compositionSetting = (VFXEnum.VFXCompositionSetting)EditorGUILayout.EnumPopup(compositionSetting);
        outputEnum = (VFXEnum.VFXOutputEnum)EditorGUILayout.EnumPopup(outputEnum);
        attribute = (VFXEnum.VFXAttributes)EditorGUILayout.EnumPopup(attribute);
        EditorGUI.EndChangeCheck();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawPresets()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Presets", EditorStyles.whiteLargeLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Constant Emitter")) RussiaFall.GenerateConstantEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule, addCEPs);
        if (GUILayout.Button("Burst Emitter")) RussiaFall.GenerateBurstEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule, addCEPs);
        if (GUILayout.Button("Spiral Emitter")) RussiaFall.GenerateSpiralEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule, addCEPs);
        if (GUILayout.Button("Gravity Emitter")) RussiaFall.GenerateGravityEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule, addCEPs);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawContexts()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Contexts", EditorStyles.whiteLargeLabel);

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn")) RussiaFall.SpawnModule(vfx, overrule);
        if (GUILayout.Button("Initialise Particle")) RussiaFall.InitialiseModule(vfx, false);
        if (GUILayout.Button("Update")) RussiaFall.UpdateModule(vfx);
        if (GUILayout.Button("Output")) RussiaFall.OutputModule(vfx, outputEnum, 400);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Event")) RussiaFall.EventModule(vfx, overrule);
        if (GUILayout.Button("Initialise Strip")) RussiaFall.InitialiseModule(vfx, true);
        if (GUILayout.Button("GPU Event")) RussiaFall.GPUEventModule(vfx, overrule);
        if (GUILayout.Button("Output Event")) RussiaFall.OutputEventModule(vfx, overrule);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawSpawn()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Spawn", EditorStyles.whiteLargeLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Constant Spawn Rate")) RussiaFall.ConstantModule(vfx, addCEPs);
        if (GUILayout.Button("Single Burst Spawn")) RussiaFall.BurstModule(vfx, addCEPs, false);
        if (GUILayout.Button("Periodic Burst Spawn")) RussiaFall.BurstModule(vfx, addCEPs, true);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawInit()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Initialize", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Position")) RussiaFall.PositionModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Velocity")) RussiaFall.VelocityModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Lifetime")) RussiaFall.LifetimeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Angle")) RussiaFall.AngleModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Size")) RussiaFall.SizeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Scale")) RussiaFall.ScaleModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Color")) RussiaFall.ColorModule(vfx, VFXEnum.VFXContextTarget.Init, addCEPs);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Position Shape")) RussiaFall.PositionShapeModule(vfx, randomSetting, compositionSetting, addCEPs);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawUpdate()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Update", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Collision Shape")) RussiaFall.CollisionShapeModule(vfx);
        if (GUILayout.Button("Trigger Event")) RussiaFall.TriggerEventModule(vfx);
        if (GUILayout.Button("Over Life: Update")) RussiaFall.OverLifeModule(vfx, attribute, compositionSetting, VFXEnum.VFXContextTarget.Update);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gravity")) RussiaFall.GravityModule(vfx, addCEPs);
        if (GUILayout.Button("Drag")) RussiaFall.DragModule(vfx, addCEPs);
        if (GUILayout.Button("Subgraph")) RussiaFall.SpawnSubgraph(vfx, "Spherical Orbit");
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawOutput()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Output", EditorStyles.whiteLargeLabel);

        EditorGUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Position")) RussiaFall.PositionModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Velocity")) RussiaFall.VelocityModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Lifetime")) RussiaFall.LifetimeModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Angle")) RussiaFall.AngleModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Size")) RussiaFall.SizeModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Scale")) RussiaFall.ScaleModule(vfx, VFXEnum.VFXContextTarget.Output, randomSetting, compositionSetting, addCEPs);
        if (GUILayout.Button("Color")) RussiaFall.ColorModule(vfx, VFXEnum.VFXContextTarget.Output, addCEPs);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Orient")) RussiaFall.OrientModule(vfx);
        if (GUILayout.Button("Over Life: Output")) RussiaFall.OverLifeModule(vfx, attribute, compositionSetting, VFXEnum.VFXContextTarget.Output);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
    }

    private void DrawCEP()
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Custom Exposed Properties", EditorStyles.whiteLargeLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Exposed Float")) RussiaFall.AddFloatProperty(vfx, 100.0f);
        if (GUILayout.Button("Exposed Int")) RussiaFall.AddIntProperty(vfx, 100);
        if (GUILayout.Button("Exposed Color")) RussiaFall.AddColorProperty(vfx, new Color(1, 1, 1));
        if (GUILayout.Button("Exposed Vector3")) RussiaFall.AddVector3Property(vfx, new Vector3(1, 1, 1));
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
    }

    private void DrawValues()
    {
        EditorGUILayout.LabelField($"Found {CEPs.Count} exposed property(ies):", EditorStyles.boldLabel);

        if (CEPs == null) return;

        foreach (var p in CEPs)
        {
            EditorGUILayout.BeginVertical("box");
            object newValue = DrawEditableValueField(p);

            //If value changed => write back to VFX asset
            if (newValue != null && !Equals(newValue, p.Value))
            {
                RussiaFall.SetExposedValue(vfx, p, newValue);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void OnGUI()
    {
        RefreshCEP();

        DrawTitleBar();

        //If no asset is present
        if (vfx == null)
        {
            path = EditorGUILayout.TextField("Asset File Path: ", path);
            newAssetName = EditorGUILayout.TextField("New Asset Name: ", newAssetName);

            if (GUILayout.Button("Create VFX Asset")) vfx = RussiaFall.CreateVFXAsset(path, newAssetName, vfx);
            if (GUILayout.Button("Import Assets")) RussiaFall.ImportAssets();
        }

        EditorGUILayout.Separator();

        //If an Asset is detected
        if (vfx != null)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawSettings();

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Modules", EditorStyles.whiteLargeLabel);

            DrawPresets();

            DrawContexts();

            DrawSpawn();

            DrawInit();

            DrawUpdate();

            DrawOutput();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            DrawCEP();

            DrawValues();

            EditorGUILayout.EndScrollView();
        }
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

    private void RefreshCEP()
    {
        CEPs.Clear();
        if (vfx != null) CEPs = RussiaFall.GetExposedProperties(vfx);
    }
}



//private void OnEnable()
//{
//    InspectorWatcher.OnParameterSelected += OnParamSelected;
//}
//private void OnDisable()
//{
//    InspectorWatcher.OnParameterSelected -= OnParamSelected;
//}
//void OnParamSelected(VFXParameter p)
//{
//    Repaint();
//}


//EditorGUILayout.Separator();

//CEP START ------------------------------------------------------------------------------------------------------------
//EditorGUILayout.LabelField("Enter New Name for CEP:");
//newName = EditorGUILayout.TextField(newName);
//CEP END ------------------------------------------------------------------------------------------------------------


//private Texture2D haha = null;
//private string texturePath = "Packages/com.github.syedirfan74.vfxmaker/Textures/Star.png";
//[MenuItem("Tools/Test Load Package Texture")]
//static void TestLoad()
//{
//    string path = "Packages/com.github.syedirfan74.vfxmaker/Textures/Star.png";

//    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

//    Debug.Log(tex != null
//        ? "Loaded texture: " + tex.name
//        : "FAILED to load texture at: " + path);
//}

//haha = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

//Debug.Log(haha);
//if (GUILayout.Button("Output 2 Context")) RussiaFall.Output2Module(vfx, outputEnum, haha);



//if (GUILayout.Button("testingshit")) RussiaFall.testingshit(vfx);


//enum Tab
//{
//    Overview,
//    Properties,
//    Debug
//}

//Tab currentTab;
//currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new[] { "Overview", "Properties", "Debug"});

//GUILayout.Space(10);

//var testHAHA = (Texture2D)EditorGUILayout.ObjectField(haha, typeof(Texture2D), true);




//if (VFXReloadGuard.NeedsRefresh)
//{
//    return;
//}
//public static class VFXReloadGuard
//{
//    public static bool NeedsRefresh { get; private set; }

//    [DidReloadScripts]
//    private static void OnReload()
//    {
//        NeedsRefresh = true;
//        EditorApplication.update += WaitOneFrame;
//    }

//    private static void WaitOneFrame()
//    {
//        NeedsRefresh = false;
//        EditorApplication.update -= WaitOneFrame;
//    }
//}








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