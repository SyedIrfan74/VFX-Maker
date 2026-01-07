using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VFX;
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

    private VFXEnum.VFXOutputEnum outputEnum;
    private VFXEnum.VFXRandomSetting randomSetting;
    private VFXEnum.VFXCompositionSetting compositionSetting;
    private VFXEnum.VFXAttributes attribute;

    private List<ExposedPropertyInfo> CEPs = new List<ExposedPropertyInfo>();
    private string newName;

    private Vector2 scroll;

    private Texture2D haha = null;
    private string texturePath = "Packages/com.github.syedirfan74.vfxmaker/Textures";
    private string textureName = "Star 5";


    [MenuItem("Window/VFXMaker")]
    static void OpenWindow()
    {
        GetWindow<VFXMaker>();
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
        if (newName == null) return;

        //p.exposedName = newName;
        newName = null;
        Repaint();
    }

    private void HandleVFXAssetChange(VisualEffectAsset newVFX)
    {
        vfx = newVFX;
        RefreshCEP();
        newName = null; 

        if (haha == null) haha = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath + "/" + textureName);

        Debug.Log(haha);
    }

    private void OnGUI()
    {
        RefreshCEP();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Target VFX Asset", EditorStyles.whiteLargeLabel);
        
        //Check if field has been changed
        EditorGUI.BeginChangeCheck();
        var newVFX = (VisualEffectAsset)EditorGUILayout.ObjectField(vfx, typeof(VisualEffectAsset), true);
        if (EditorGUI.EndChangeCheck()) HandleVFXAssetChange(newVFX);
        EditorGUILayout.EndVertical();

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
            scroll = EditorGUILayout.BeginScrollView(scroll);

            //PRESETS START ------------------------------------------------------------------------------------------------------------
            EditorGUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Presets", EditorStyles.whiteLargeLabel);

            EditorGUI.BeginChangeCheck();
            overrule = EditorGUILayout.Toggle("Override", overrule);
            EditorGUI.EndChangeCheck();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Constant Emitter")) RussiaFall.GenerateConstantEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule);
            if (GUILayout.Button("Burst Emitter")) RussiaFall.GenerateBurstEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule);
            if (GUILayout.Button("Spiral Emitter")) RussiaFall.GenerateSpiralEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule);
            if (GUILayout.Button("Gravity Emitter")) RussiaFall.GenerateGravityEmitter(vfx, outputEnum, randomSetting, compositionSetting, overrule);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //PRESETS END ------------------------------------------------------------------------------------------------------------

            EditorGUILayout.Separator();

            //MODULES START ------------------------------------------------------------------------------------------------------------

            //Settings Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Modules", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            randomSetting = (VFXEnum.VFXRandomSetting)EditorGUILayout.EnumPopup(randomSetting);
            compositionSetting = (VFXEnum.VFXCompositionSetting)EditorGUILayout.EnumPopup(compositionSetting);
            outputEnum = (VFXEnum.VFXOutputEnum)EditorGUILayout.EnumPopup(outputEnum);
            attribute = (VFXEnum.VFXAttributes)EditorGUILayout.EnumPopup(attribute);
            EditorGUI.EndChangeCheck();
            GUILayout.EndHorizontal();
            //Settings End

            //Contexts Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Contexts", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn Context")) RussiaFall.SpawnModule(vfx, overrule);
            if (GUILayout.Button("Initialise Context")) RussiaFall.InitialiseModule(vfx);
            if (GUILayout.Button("Update Context")) RussiaFall.UpdateModule(vfx);
            if (GUILayout.Button("Output Context")) RussiaFall.OutputModule(vfx, outputEnum, 400);
            if (GUILayout.Button("Output 2 Context")) RussiaFall.Output2Module(vfx, outputEnum, haha);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //Contexts End

            //Spawn / Init Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Spawn / Init", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Constant Spawn Rate")) RussiaFall.ConstantModule(vfx);
            if (GUILayout.Button("Burst Spawn")) RussiaFall.BurstModule(vfx);
            if (GUILayout.Button("Velocity")) RussiaFall.VelocityModule(vfx, randomSetting, compositionSetting);
            if (GUILayout.Button("Lifetime")) RussiaFall.LifetimeModule(vfx, randomSetting, compositionSetting);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //Spawn / Init End

            //Update Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Update", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gravity")) RussiaFall.GravityModule(vfx);
            if (GUILayout.Button("Trigger Event On Die")) RussiaFall.TriggerEventModule(vfx);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //Update End

            //Output Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Output", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Size")) RussiaFall.SizeModule(vfx, randomSetting, compositionSetting);
            if (GUILayout.Button("Scale")) RussiaFall.ScaleModule(vfx, randomSetting, compositionSetting);
            if (GUILayout.Button("Over Life")) RussiaFall.OverLifeModule(vfx, attribute, compositionSetting);
            if (GUILayout.Button("Orient")) RussiaFall.OrientModule(vfx);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //Output End

            //CEP Start
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("CEP", EditorStyles.whiteLargeLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Exposed Float")) RussiaFall.AddFloatProperty(vfx, 100.0f);
            if (GUILayout.Button("Exposed Int")) RussiaFall.AddIntProperty(vfx, 100);
            if (GUILayout.Button("Exposed Color")) RussiaFall.AddColorProperty(vfx, new Color(1, 1, 1));
            if (GUILayout.Button("Exposed Vector3")) RussiaFall.AddVector3Property(vfx, new Vector3(1, 1, 1));
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //CEP End

            EditorGUILayout.EndVertical();
            //MODULES END ------------------------------------------------------------------------------------------------------------

            EditorGUILayout.Separator();

            //CEP START ------------------------------------------------------------------------------------------------------------
            EditorGUILayout.LabelField("Enter New Name for CEP:");
            newName = EditorGUILayout.TextField(newName);
            //CEP END ------------------------------------------------------------------------------------------------------------

            EditorGUILayout.Separator();

            //VALUES START ------------------------------------------------------------------------------------------------------------
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
            //VALUES END ------------------------------------------------------------------------------------------------------------
            EditorGUILayout.EndScrollView();
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

    private void RefreshCEP()
    {
        CEPs.Clear();
        if (vfx != null) CEPs = RussiaFall.GetExposedProperties(vfx);
    }
}










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