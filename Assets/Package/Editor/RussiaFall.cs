using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.VFX;
using UnityEditor.VFX.Block;
using UnityEngine;
using UnityEngine.VFX;
using Block = UnityEditor.VFX.Block;
using Operator = UnityEditor.VFX.Operator;

/// <summary>
/// RussiaFall Interface Class
/// Author: Syed Irfan
/// 
/// Class that interfaces with VFX Graph API to make graphs using Script
/// </summary>

public static class RussiaFall
{
    private static string packagePath = "Packages/com.github.syedirfan74.vfxmaker";
    private static string texturePath = packagePath + "/Textures/Star.png";
    private static string destinationPath = "Assets/VFXMaker";
    private static string assetsPath = "/Samples~";
    private static string subgraphPath = "/Subgraphs";

    public static void SpawnSubgraph(VisualEffectAsset vfx, string subgraphName)
    {
        string[] guids = AssetDatabase.FindAssets("t:VisualEffectSubgraphOperator Random Radial");

        if (guids.Length == 0)
        {
            Debug.LogError("Subgraph not found");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        Debug.Log(guids);
        Debug.Log(path);
        var subgraph = AssetDatabase.LoadAssetAtPath<VisualEffectSubgraphOperator>(path);






        //var subgraph = AssetDatabase.LoadAssetAtPath<VFXSubgraphOperator>(packagePath + "/Subgraphs/" + subgraphName + ".vfxoperator");
        ////"C:\Users\Syed Irfan\Y3Sem2\SPROJ\VFX-Maker\Assets\Package\Subgraphs\Random Radial.vfxoperator"

        //Debug.Log(packagePath + "/Subgraphs/" + subgraphName + ".vfxoperator");

        if (subgraph == null) {
            Debug.LogWarning("No asset detected.");
            return;
        }
        Debug.Log(subgraph.name);
    }


    //Creates new VisualEffectAsset in designated file path with specified name
    public static VisualEffectAsset CreateVFXAsset(string path, string newAssetName, VisualEffectAsset vfx)
    {
        Debug.Log("Creating VFX");

        //Valid File Path Check
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"Folder not found! {path}");
            return null;
        }

        string assetPath = $"{path}/{newAssetName}.vfx";

        //Creates and saves Asset
        var vfxAsset = VisualEffectAssetEditorUtility.CreateNewAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        vfx = vfxAsset;

        Debug.Log($"Created new VFX Graph at: {path}");

        return vfx;
    }

    //Generates an Empty VFX Graph with all Main Modules Diff
    private static VFXGraph GenerateEmptyTemplate(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum outputEnum, bool overrule, float gapAmount = 400)
    {
        if (vfx == null)
        {
            Debug.LogError("No VFX Asset Detected! Ensure Asset is Referenced!");
            return null;
        }

        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Resets Graph if Override is Active
        if (overrule) graph.RemoveAllChildren();
       
        //Creates Spawn Module
        SpawnModule(vfx, overrule);

        //Creates Init Module
        InitialiseModule(vfx, false);

        //Creates Update Module
        UpdateModule(vfx);

        //Creates Output Module
        OutputModule(vfx, outputEnum);

        return graph;
    }

    //Import Assets into User's Project
    [MenuItem("Window/VFXMaker/Import Assets")]
    public static void ImportAssets()
    {
        var source = packagePath + assetsPath;
        
        if (!Directory.Exists(source))
        {
            Debug.LogError("Path does not exist");
            return;
        }

        FileUtil.CopyFileOrDirectory(source, destinationPath);
        AssetDatabase.Refresh();
    }
    
    //PRESETS START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Creates a Basic Constant Emitter VFX Graph
    public static void GenerateConstantEmitter(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum outputEnum, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool overrule, bool addCEPs)
    {
        var graph = GenerateEmptyTemplate(vfx, outputEnum, overrule);

        if (graph == null) return;

        //Constant Rate Spawner
        ConstantModule(vfx, addCEPs);

        //Set Velocity Random per Component 
        VelocityModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Lifetime Random per Component
        LifetimeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Color
        ColorModule(vfx, VFXEnum.VFXContextTarget.Init, addCEPs);
    }

    //Creates a Basic Burst Emitter VFX Graph
    public static void GenerateBurstEmitter(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum outputEnum, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool overrule, bool addCEPs)
    {
        var graph = GenerateEmptyTemplate(vfx, outputEnum, overrule);

        if (graph == null) return;

        //Burst Spawner
        BurstModule(vfx, addCEPs, false);

        //Set Velocity Random per Component 
        VelocityModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Lifetime Random per Component
        LifetimeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Color
        ColorModule(vfx, VFXEnum.VFXContextTarget.Init, addCEPs);
    }

    //Creates a Basic Spiral Emitter VFX Graph
    public static void GenerateSpiralEmitter(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum outputEnum, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool overrule, bool addCEPs)
    {
        var graph = GenerateEmptyTemplate(vfx, outputEnum, overrule);

        if (graph == null) return;

        //Getting Contexts
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);
        var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);
        var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);

        //INIT START --------------------------------------------------------------------

        //Constant Spawner
        ConstantModule(vfx, addCEPs);

        //Set Velocity Random per Component 
        VelocityModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Lifetime Random per Component
        LifetimeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //INIT END --------------------------------------------------------------------

        //UPDATE START --------------------------------------------------------------------

        //Set Position Block, Add to Update
        var setPosition = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setPosition.SetSettingValue("attribute", VFXAttribute.Position.name);
        update.AddChild(setPosition);

        //Rotate 3D Operator, Link to Set Position
        var rotate3D = ScriptableObject.CreateInstance<Operator.Rotate3D>();
        rotate3D.GetInputSlot(1).value = (Position)(new Vector3(1, 0, 0));
        rotate3D.GetInputSlot(3).value = 0.1f;
        rotate3D.GetOutputSlot(0).Link(setPosition.GetInputSlot(0));

        //Get Position, Link to Rotate 3D
        var getPosition = ScriptableObject.CreateInstance<VFXAttributeParameter>();
        getPosition.SetSettingValue("attribute", VFXAttribute.Position.name);
        getPosition.GetOutputSlot(0).Link(rotate3D.GetInputSlot(0));
        
        //Add Rotate3D & getPosition to graph
        graph.AddChild(rotate3D);
        graph.AddChild(getPosition);


        //OUTPUT START --------------------------------------------------------------------
        var setColorOverLifeType = Type.GetType("UnityEditor.VFX.Block.AttributeFromCurve, Unity.VisualEffectGraph.Editor");
        if (setColorOverLifeType != null)
        {
            var setColorOverLife = ScriptableObject.CreateInstance(setColorOverLifeType) as VFXBlock;
            setColorOverLife.SetSettingValue("attribute", VFXAttribute.Color.name);
            output.AddChild(setColorOverLife);
        }
        else Debug.LogError("Failed to find SetColorOverLife block type. Unity version or assembly name may differ.");
    }

    //Creates a Basic Gravity Emitter VFX Graph
    public static void GenerateGravityEmitter(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum outputEnum, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool overrule, bool addCEPs)
    {
        var graph = GenerateEmptyTemplate(vfx, outputEnum, overrule);

        if (graph == null) return;

        //Constant Rate Spawner
        ConstantModule(vfx, addCEPs);

        //Set Velocity Random per Component 
        VelocityModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Set Lifetime Random per Component
        LifetimeModule(vfx, VFXEnum.VFXContextTarget.Init, randomSetting, compositionSetting, addCEPs);

        //Add Gravity
        GravityModule(vfx, addCEPs);

        //Set Color
        ColorModule(vfx, VFXEnum.VFXContextTarget.Init, addCEPs);
    }

    //PRESETS END ---------------------------------------------------------------------------------------------------------------------------------------------------------

    //CONTEXTS START ------------------------------------------------------------------------------------------------------------------------------------------------------
    
    //Adds VFX Spawn Context to the Current VFX Graph
    public static void SpawnModule(VisualEffectAsset vfx, bool overrule, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var numGraphs = GetNumGraphs(vfx);

        //Creates Spawn Module
        var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
        if (overrule) spawner.position = new Vector2(0, 0);
        else spawner.position = new Vector2(gapAmount * numGraphs, 0);
        graph.AddChild(spawner);
    }

    //Adds VFX Initialise Context to the Current VFX Graph
    public static void InitialiseModule(VisualEffectAsset vfx, bool strip, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Creates Initialize Module
        var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
        //init.SetSettingValue("capacity", 1024u);
        if (strip) init.SetSettingValue("dataType", VFXDataParticle.DataType.ParticleStrip);
        graph.AddChild(init);

        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);

        init.position = new Vector2(spawn.position.x, gapAmount);

        spawn.LinkTo(init);
    }

    //Adds VFX Update Context to the Current VFX Graph
    public static void UpdateModule(VisualEffectAsset vfx, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Creates Update Module
        var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
        graph.AddChild(update);

        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);

        update.position = new Vector2(init.position.x, gapAmount * 2);

        init.LinkTo(update);
    }

    //Adds VFX Output Context to the Current VFX Graph
    public static void OutputModule(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum VFXEnum, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

        //Creates Unlit Quad Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXPlanarPrimitiveOutput)
        {
            var output = ScriptableObject.CreateInstance<VFXPlanarPrimitiveOutput>();
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);

            var slot = output.inputSlots.FirstOrDefault(s => s.property.type == typeof(Texture2D) && s.property.name == "mainTexture");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            if (texture != null) slot.value = texture;
            else Debug.Log("Yea this aint working");

            if (slot == null) Debug.Log("slot also not working bud");
            else Debug.Log("nah this works");
        }

        //Creates Unlit Mesh Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXMeshOutput)
        {
            var output = ScriptableObject.CreateInstance<VFXMeshOutput>();
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);
        }

        //Creates Lit Quad Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXURPLitPlanarPrimitiveOutput)
        {
            var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitPlanarPrimitiveOutput, Unity.RenderPipelines.Universal.Editor");
            if (type == null)
            {
                Debug.LogError("Couldn't find URP Lit Output type!");
                return;
            }

            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);
        }

        //Creates Lit Mesh Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXURPLitMeshOutput)
        {
            var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitMeshOutput, Unity.RenderPipelines.Universal.Editor");
            if (type == null)
            {
                Debug.LogError("Couldn't find URP Lit Output type!");
                return;
            }

            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);
        }

        //Creates Shader Graph Quad Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXShaderGraphQuad)
        {
            var type = Type.GetType("UnityEditor.VFX.VFXComposedParticleOutput" + ", Unity.VisualEffectGraph.Editor");
            if (type == null)
            {
                Debug.LogError("Could not find Type!");
                return;
            }

            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            output.position = new Vector2(update.position.x, gapAmount * 3);
            output.SetSettingValue("m_Topology", new ParticleTopologyPlanarPrimitive(VFXPrimitiveType.Quad));
            update.LinkTo(output);
            graph.AddChild(output);
        }

        //Creates Shader Graph Mesh Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXShaderGraphMesh)
        {
            var type = Type.GetType("UnityEditor.VFX.VFXComposedParticleOutput" + ", Unity.VisualEffectGraph.Editor");
            if (type == null)
            {
                Debug.LogError("Could not find Type!");
                return;
            }

            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            output.position = new Vector2(update.position.x, gapAmount * 3);
            output.SetSettingValue("m_Topology", new ParticleTopologyMesh());
            update.LinkTo(output);
            graph.AddChild(output);
        }

        //Creates Quad Strip Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXQuadStripOutput)
        {
            var output = ScriptableObject.CreateInstance<VFXQuadStripOutput>();
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);

            var slot = output.inputSlots.FirstOrDefault(s => s.property.type == typeof(Texture2D) && s.property.name == "mainTexture");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            if (texture != null) slot.value = texture;
            else Debug.Log("Yea this aint working");

            if (slot == null) Debug.Log("slot also not working bud");
            else Debug.Log("nah this works");
        }

        //Creates Shader Graph Mesh Output Context
        if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXURPLitQuadStripOutput)
        {
            var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitQuadStripOutput, Unity.RenderPipelines.Universal.Editor");
            if (type == null)
            {
                Debug.LogError("Couldn't find URP Lit Output type!");
                return;
            }

            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            output.position = new Vector2(update.position.x, gapAmount * 3);
            update.LinkTo(output);
            graph.AddChild(output);
        }
    }

    //Adds Event Context to the Current VFX Graph
    public static void EventModule(VisualEffectAsset vfx, bool overrule, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var numGraphs = GetNumGraphs(vfx);

        //Creates Spawn Module
        var gpuEvent = ScriptableObject.CreateInstance<VFXBasicEvent>();
        if (overrule) gpuEvent.position = new Vector2(0, 0);
        else gpuEvent.position = new Vector2(gapAmount * numGraphs, 0);
        graph.AddChild(gpuEvent);
    }

    //Adds GPU Event Context to the Current VFX Graph
    public static void GPUEventModule(VisualEffectAsset vfx, bool overrule, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);
        var trigger = update.children.LastOrDefault(c => c.name.Contains("Trigger Event"));

        var numGraphs = GetNumGraphs(vfx);

        //Creates Spawn Module
        var gpuEvent = ScriptableObject.CreateInstance<VFXBasicGPUEvent>();

        if (trigger != null)
        {
            trigger.GetOutputSlot(0).Link(gpuEvent.GetInputSlot(0));
            gpuEvent.position = new Vector2(gapAmount * numGraphs, update.position.y);
        }
        else
        {
            if (overrule) gpuEvent.position = new Vector2(0, 0);
            else gpuEvent.position = new Vector2(gapAmount * numGraphs, 0);
        }

        graph.AddChild(gpuEvent);
    }

    //Adds Output Event Context to the Current VFX Graph
    public static void OutputEventModule(VisualEffectAsset vfx, bool overrule, float gapAmount = 400)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var numGraphs = GetNumGraphs(vfx);

        //Creates Spawn Module
        var gpuEvent = ScriptableObject.CreateInstance<VFXOutputEvent>();
        if (overrule) gpuEvent.position = new Vector2(0, 0);
        else gpuEvent.position = new Vector2(gapAmount * numGraphs, 0);
        graph.AddChild(gpuEvent);
    }

    //CONTEXTS END ------------------------------------------------------------------------------------------------------------------------------------------------------


    //SPAWN START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Adds Constant Spawner Node to Spawn Context
    public static void ConstantModule(VisualEffectAsset vfx, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);

        //Constant Rate Spawner
        var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
        constantRate.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(constantRate);

        if (addCEPs) AddFloatProperty(vfx, 32.0f);
    }

    //Adds Burst Spawner Node to Spawn Context
    public static void BurstModule(VisualEffectAsset vfx, bool addCEPs, bool periodic)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);

        //Constant Rate Spawner
        var burst = ScriptableObject.CreateInstance<VFXSpawnerBurst>();

        if (periodic) burst.SetSettingValue("repeat", VFXSpawnerBurst.RepeatMode.Periodic);

        burst.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(burst);

        if (addCEPs) AddFloatProperty(vfx, 32.0f);
    }

    //SPAWN END ------------------------------------------------------------------------------------------------------------------------------------------------------


    //INITIALIZE START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Adds Position to the current VFX Graph
    public static void PositionModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
       
        //Set Velocity 
        var setPosition = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setPosition.SetSettingValue("attribute", VFXAttribute.Position.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setPosition.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setPosition.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setPosition.SetSettingValue("Random", Block.RandomMode.Uniform);

        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setPosition.GetInputSlot(0).value = (Position)(new Vector3(1, 1.5f, 1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }
        else
        {
            setPosition.GetInputSlot(0).value = (Position)(new Vector3(-1, 0, -1));
            setPosition.GetInputSlot(1).value = (Position)(new Vector3(1, 1.5f, 1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(-1, 0, -1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setPosition.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setPosition.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setPosition.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setPosition.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setPosition);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setPosition);
    }

    //Adds Velocity to the current VFX Graph
    public static void VelocityModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        //Set Velocity 
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setVelocity.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setVelocity.SetSettingValue("Random", Block.RandomMode.Uniform);

        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(1, 1.5f, 1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }
        else
        {
            setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
            setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(-1, 0, -1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setVelocity.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setVelocity.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setVelocity.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setVelocity.Composition = AttributeCompositionMode.Add;
        
        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setVelocity);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setVelocity);
    }

    //Adds Lifetime to the current VFX Graph
    public static void LifetimeModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        //Set Velocity Random per Component 
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setLifetime.SetSettingValue("Random", Block.RandomMode.Uniform);

        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setLifetime.GetInputSlot(0).value = 10.0f;
            if (addCEPs) AddFloatProperty(vfx, 10.0f);
        }
        else
        {
            setLifetime.GetInputSlot(0).value = 1.0f;
            setLifetime.GetInputSlot(1).value = 10.0f;
            if (addCEPs) AddFloatProperty(vfx, 1.0f);
            if (addCEPs) AddFloatProperty(vfx, 10.0f);
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setLifetime.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setLifetime.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setLifetime.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setLifetime.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setLifetime);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setLifetime);
    }

    //Adds Angle to the current VFX Graph
    public static void AngleModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        //Set Velocity Random per Component 
        var setAngle = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setAngle.SetSettingValue("attribute", VFXAttribute.angle.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setAngle.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setAngle.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setAngle.SetSettingValue("Random", Block.RandomMode.Uniform);

        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setAngle.GetInputSlot(0).value = new Vector3(1, 1.5f, 1);
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }
        else
        {
            setAngle.GetInputSlot(0).value = new Vector3(-1, 0, -1);
            setAngle.GetInputSlot(1).value = new Vector3(1, 1.5f, 1);
            if (addCEPs) AddVector3Property(vfx, new Vector3(-1, 0, -1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setAngle.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setAngle.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setAngle.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setAngle.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setAngle);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setAngle);
    }

    public static void PositionShapeModule(VisualEffectAsset vfx, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);

        //Set Velocity 
        var positionShape = ScriptableObject.CreateInstance<Block.PositionShape>();

        ////Random Setting
        //if (randomSetting == VFXEnum.VFXRandomSetting.Off) setPosition.SetSettingValue("Random", Block.RandomMode.Off);
        //if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setPosition.SetSettingValue("Random", Block.RandomMode.PerComponent);
        //if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setPosition.SetSettingValue("Random", Block.RandomMode.Uniform);

        ////Random Input
        //if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        //{
        //    setPosition.GetInputSlot(0).value = (Position)(new Vector3(1, 1.5f, 1));
        //    if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        //}
        //else
        //{
        //    setPosition.GetInputSlot(0).value = (Position)(new Vector3(-1, 0, -1));
        //    setPosition.GetInputSlot(1).value = (Position)(new Vector3(1, 1.5f, 1));
        //    if (addCEPs) AddVector3Property(vfx, new Vector3(-1, 0, -1));
        //    if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1.5f, 1));
        //}

        ////Composition setting
        //if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setPosition.Composition = AttributeCompositionMode.Overwrite;
        //if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setPosition.Composition = AttributeCompositionMode.Blend;
        //if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setPosition.Composition = AttributeCompositionMode.Multiply;
        //if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setPosition.Composition = AttributeCompositionMode.Add;

        init.AddChild(positionShape);
    }

    //INITIALIZE END ------------------------------------------------------------------------------------------------------------------------------------------------------


    //UPDATE START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Adds Gravity to the current VFX Graph
    public static void GravityModule(VisualEffectAsset vfx, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

        //Add Gravity
        var gravity = ScriptableObject.CreateInstance<Gravity>();
        update.AddChild(gravity);

        if (addCEPs) AddVector3Property(vfx, new Vector3(0, -9.81f, 0));
    }

    //Adds Drag to the current VFX Graph
    public static void DragModule(VisualEffectAsset vfx, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

        //Add Gravity
        var drag = ScriptableObject.CreateInstance<Block.Drag>();
        update.AddChild(drag);

        if (addCEPs) AddFloatProperty(vfx, 0);
    }

    //Adds Trigger Event to the current VFX Graph
    public static void TriggerEventModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

        //Add Gravity
        var triggerEvent = ScriptableObject.CreateInstance<TriggerEvent>();
        triggerEvent.SetSettingValue("mode", TriggerEvent.Mode.OnDie);
        update.AddChild(triggerEvent);
    }

    //Adds Collision Shape to the current VFX Graph
    public static void CollisionShapeModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

        //Add Gravity
        var triggerEvent = ScriptableObject.CreateInstance<CollisionShape>();
        update.AddChild(triggerEvent);
    }

    //UPDATE END ------------------------------------------------------------------------------------------------------------------------------------------------------


    //OUTPUT START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Adds Size to the current VFX Graph
    public static void SizeModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        //Set Velocity Random per Component 
        var setSize = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setSize.SetSettingValue("attribute", VFXAttribute.Size.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setSize.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setSize.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setSize.SetSettingValue("Random", Block.RandomMode.Uniform);
        
        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setSize.GetInputSlot(0).value = 1.0f;
            if (addCEPs) AddFloatProperty(vfx, 1.0f);
        }
        else
        {
            setSize.GetInputSlot(0).value = 1.0f;
            setSize.GetInputSlot(1).value = 10.0f;
            if (addCEPs) AddFloatProperty(vfx, 1.0f);
            if (addCEPs) AddFloatProperty(vfx, 10.0f);
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setSize.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setSize.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setSize.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setSize.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setSize);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setSize);
    }

    //Adds Scale to the current VFX Graph
    public static void ScaleModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, VFXEnum.VFXRandomSetting randomSetting, VFXEnum.VFXCompositionSetting compositionSetting, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        //Set Velocity Random per Component 
        var setScale = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setScale.SetSettingValue("attribute", VFXAttribute.scale.name);

        //Random Setting
        if (randomSetting == VFXEnum.VFXRandomSetting.Off) setScale.SetSettingValue("Random", Block.RandomMode.Off);
        if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent) setScale.SetSettingValue("Random", Block.RandomMode.PerComponent);
        if (randomSetting == VFXEnum.VFXRandomSetting.Uniform) setScale.SetSettingValue("Random", Block.RandomMode.Uniform);

        //Random Input
        if (randomSetting == VFXEnum.VFXRandomSetting.Off)
        {
            setScale.GetInputSlot(0).value = new Vector3(1, 1, 1);
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1, 1));
        }
        else
        {
            setScale.GetInputSlot(0).value = new Vector3(1, 1, 1);
            setScale.GetInputSlot(1).value = new Vector3(10, 10, 10);
            if (addCEPs) AddVector3Property(vfx, new Vector3(1, 1, 1));
            if (addCEPs) AddVector3Property(vfx, new Vector3(10, 10, 10));
        }

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setScale.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setScale.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setScale.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setScale.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setScale);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setScale);
    }

    //Adds Color to current VFX Graph
    public static void ColorModule(VisualEffectAsset vfx, VFXEnum.VFXContextTarget target, bool addCEPs)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();

        var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setColor.SetSettingValue("attribute", VFXAttribute.Color.name);

        if (target == VFXEnum.VFXContextTarget.Init) contexts.LastOrDefault(c => c.contextType == VFXContextType.Init).AddChild(setColor);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setColor);

        if (addCEPs) AddColorProperty(vfx, new Color(1, 1, 1));
    }

    //Adds Orient to the current VFX Graph
    public static void OrientModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);

        //Orient 
        var orient = ScriptableObject.CreateInstance<Block.Orient>();
        orient.mode = Orient.Mode.FaceCameraPlane;

        output.AddChild(orient);
    }

    //OUTPUT START ------------------------------------------------------------------------------------------------------------------------------------------------------

    //Adds Scale to the current VFX Graph
    public static void OverLifeModule(VisualEffectAsset vfx, VFXEnum.VFXAttributes attribute, VFXEnum.VFXCompositionSetting compositionSetting, VFXEnum.VFXContextTarget target)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        
        //Set Velocity Random per Component 
        var setScale = ScriptableObject.CreateInstance<Block.AttributeFromCurve>();

        if (attribute == VFXEnum.VFXAttributes.position) setScale.SetSettingValue("attribute", VFXAttribute.Position.name);
        if (attribute == VFXEnum.VFXAttributes.velocity) setScale.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        if (attribute == VFXEnum.VFXAttributes.color) setScale.SetSettingValue("attribute", VFXAttribute.Color.name);
        if (attribute == VFXEnum.VFXAttributes.alpha) setScale.SetSettingValue("attribute", VFXAttribute.Alpha.name);
        if (attribute == VFXEnum.VFXAttributes.size) setScale.SetSettingValue("attribute", VFXAttribute.Size.name);
        if (attribute == VFXEnum.VFXAttributes.scale) setScale.SetSettingValue("attribute", VFXAttribute.scale.name);
        if (attribute == VFXEnum.VFXAttributes.angle) setScale.SetSettingValue("attribute", VFXAttribute.angle.name);

        //Composition setting
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Overwrite) setScale.Composition = AttributeCompositionMode.Overwrite;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Blend_NOTWORKING) setScale.Composition = AttributeCompositionMode.Blend;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Multiply) setScale.Composition = AttributeCompositionMode.Multiply;
        if (compositionSetting == VFXEnum.VFXCompositionSetting.Add) setScale.Composition = AttributeCompositionMode.Add;

        if (target == VFXEnum.VFXContextTarget.Update) contexts.LastOrDefault(c => c.contextType == VFXContextType.Update).AddChild(setScale);
        if (target == VFXEnum.VFXContextTarget.Output) contexts.LastOrDefault(c => c.contextType == VFXContextType.Output).AddChild(setScale);
    }

    
    //Add Exposed Float Property to the Graph
    public static void AddFloatProperty(VisualEffectAsset vfx, float defaultValue)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null)
        {
            Debug.LogError("Unable to access VFX Graph.");
            return;
        }

        // Create a new VFXParameter of type Float
        var param = ScriptableObject.CreateInstance<VFXParameter>();
        param.Init(typeof(float));

        // Set default value
        param.value = defaultValue;

        // Add to the graph
        graph.AddChild(param);

        // Save
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx));

        graph.Invalidate(VFXModel.InvalidationCause.kStructureChanged);
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    //Add Exposed Int Property to the Graph
    public static void AddIntProperty(VisualEffectAsset vfx, int defaultValue)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null)
        {
            Debug.LogError("Unable to access VFX Graph.");
            return;
        }

        // Create a new VFXParameter of type Float
        var param = ScriptableObject.CreateInstance<VFXParameter>();
        param.Init(typeof(int));

        // Set default value
        param.value = defaultValue;

        // Add to the graph
        graph.AddChild(param);

        // Save
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx));
    }

    //Add Exposed Color Property to the Graph
    public static void AddColorProperty(VisualEffectAsset vfx, Color defaultValue)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null)
        {
            Debug.LogError("Unable to access VFX Graph.");
            return;
        }

        // Create a new VFXParameter of type Float
        var param = ScriptableObject.CreateInstance<VFXParameter>();
        param.Init(typeof(Color));

        // Set default value
        param.value = defaultValue;

        // Add to the graph
        graph.AddChild(param);

        // Save
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx));
    }

    //Add Exposed Vector3 Property to the Graph
    public static void AddVector3Property(VisualEffectAsset vfx, Vector3 defaultValue)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null)
        {
            Debug.LogError("Unable to access VFX Graph.");
            return;
        }

        // Create a new VFXParameter of type Float
        var param = ScriptableObject.CreateInstance<VFXParameter>();
        param.Init(typeof(Vector3));

        // Set default value
        param.value = defaultValue;

        // Add to the graph
        graph.AddChild(param);

        // Save
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx));
    }
    //MODULES END


    //HELPER FUNCTIONS

    //Finds the number of Spawn Contexts there are within a VFX Asset
    public static int GetNumGraphs(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        return graph.children.OfType<VFXContext>().Count(c => c.contextType == VFXContextType.Spawner);
    }

    public static List<ExposedPropertyInfo> GetExposedProperties(VisualEffectAsset vfx)
    {
        var results = new List<ExposedPropertyInfo>();

        if (vfx == null) return results;

        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null) return results;

        //Gets Parameters
        var parameters = graph.children.OfType<VFXParameter>();

        foreach (var p in parameters)
        {
            // Only include exposed parameters
            // p.exposed is accessible for reading even if the type is internal
            try
            {
                if (!p.exposed) continue;
            }
            catch
            {
                // If for some Unity version p.exposed isn't accessible, fall back to continue.
                continue;
            }

            // Name: try public property, fallback to common backing field names
            string name = p.exposedName;

            // Type: try property "type" or backing field "m_Type"
            Type valueType = TryGetTypeMember(p, "type") ?? TryGetTypeMember(p, "m_Type");

            // Value: use reflection to get the value. Many VFXParameter implementations expose a "value" property.
            object value = TryGetValueMember(p);

            results.Add(new ExposedPropertyInfo(name, valueType, value, p));
        }

        return results;
    }

    public static void SetExposedValue(VisualEffectAsset asset, ExposedPropertyInfo info, object newValue)
    {
        if (info.InternalParameter == null)
            return;

        var param = info.InternalParameter;
        var t = param.GetType();

        // Assign to "value" property via reflection
        var pi = t.GetProperty("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pi != null && pi.CanWrite)
        {
            pi.SetValue(param, newValue);
        }
        else
        {
            // try fallback "m_Value" field
            var fi = t.GetField("m_Value", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
                fi.SetValue(param, newValue);
        }

        info.Value = newValue;

        // Reimport to apply changes
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
    }

    private static Type TryGetTypeMember(object obj, string memberName)
    {
        var t = obj.GetType();
        var pi = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pi != null && pi.PropertyType == typeof(Type))
        {
            try { return pi.GetValue(obj) as Type; } catch { }
        }

        var fi = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fi != null && fi.FieldType == typeof(Type))
        {
            try { return fi.GetValue(obj) as Type; } catch { }
        }

        // Some versions store a serialized string for the type or enum - try to handle that:
        if (pi != null && pi.PropertyType == typeof(string))
        {
            try
            {
                var typeName = pi.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(typeName))
                {
                    return Type.GetType(typeName) ?? AppDomain.CurrentDomain.GetAssemblies()
                               .Select(a => a.GetType(typeName)).FirstOrDefault(t2 => t2 != null);
                }
            }
            catch { }
        }

        return null;
    }

    private static object TryGetValueMember(object paramObj)
    {
        var t = paramObj.GetType();

        // common property "value"
        var pi = t.GetProperty("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pi != null && pi.CanRead)
        {
            try { return pi.GetValue(paramObj); } catch { }
        }

        // try field "value" or "m_Value"
        var fi = t.GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 ?? t.GetField("m_Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fi != null)
        {
            try { return fi.GetValue(paramObj); } catch { }
        }

        // fallback: try property "defaultValue" etc.
        var fallbackPi = t.GetProperty("defaultValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fallbackPi != null)
        {
            try { return fallbackPi.GetValue(paramObj); } catch { }
        }

        return null;
    }

    public static void ReadTypes(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);
        var settings = init.GetSettings(true, VFXSettingAttribute.VisibleFlags.None | VFXSettingAttribute.VisibleFlags.InInspector | VFXSettingAttribute.VisibleFlags.InGraph | VFXSettingAttribute.VisibleFlags.Default | VFXSettingAttribute.VisibleFlags.InGeneratedCodeComments).ToList();

        for (int i = 0; i < settings.Count; i++)
        {
            Debug.Log(settings[i].name + " | " + settings[i].value);
        }
    }
}









//public static void testingshit(VisualEffectAsset vfx)
//{
//    var graph = vfx.GetResource()?.GetOrCreateGraph();
//    var contexts = graph.children.OfType<VFXContext>();
//    var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);

//    var slot = output.inputSlots.FirstOrDefault(s => s.property.type == typeof(Texture2D) && s.property.name == "mainTexture");

//    EditorApplication.delayCall += () =>
//    {
//        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

//        if (texture != null) slot.value = texture;
//        else Debug.Log("Yea this aint working");

//        if (slot == null) Debug.Log("slot also not working bud");
//        else Debug.Log("nah this works");

//        graph.RecompileIfNeeded();
//        EditorUtility.SetDirty(vfx);
//    };
//}


//Adds VFX Output Context to the Current VFX Graph
//public static void Output2Module(VisualEffectAsset vfx, VFXEnum.VFXOutputEnum VFXEnum, Texture2D texture, float gapAmount = 400)
//{
//    var graph = vfx.GetResource()?.GetOrCreateGraph();
//    var contexts = graph.children.OfType<VFXContext>();
//    var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);

//    //Creates Unlit Quad Output Context
//    if (VFXEnum == global::VFXEnum.VFXOutputEnum.VFXPlanarPrimitiveOutput)
//    {
//        var output = ScriptableObject.CreateInstance<VFXPlanarPrimitiveOutput>();
//        output.position = new Vector2(update.position.x, gapAmount * 3);
//        update.LinkTo(output);
//        graph.AddChild(output);

//        var slot = output.inputSlots.FirstOrDefault(s => s.property.type == typeof(Texture2D) && s.property.name == "baseColorMap");
//        slot.value = texture;
//    }
//}





//param.exposed = true;                  // Make it exposed
//param.exposedName = propertyName;             // Property Name



//float kContextOffset = 400.0f;

////Getting Contexts
//var contexts = graph.children.OfType<VFXContext>();
//var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);
//var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);
//var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);
//var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);






//Create Color Operator with Random Generated Color
//var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
//constantRate.GetInputSlot(0).value = 32.0f;
//spawn.AddChild(constantRate);
//var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
//setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
//setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
//init.AddChild(setVelocity);
//var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
//setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setLifetime.GetInputSlot(0).value = 1f;
//setLifetime.GetInputSlot(1).value = 10f;
//init.AddChild(setLifetime);
//var gravity = ScriptableObject.CreateInstance<Gravity>();
//update.AddChild(gravity);
//var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
//output.AddChild(setColor);
//var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
//hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
//hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
//hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

//graph.AddChild(hsvColor);

//var constant = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
//constant.GetInputSlot(0).value = 100.0f;
//spawn.AddChild(constant);
//var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
//setVelocity.SetSettingValue("Random", Block.RandomMode.Off);
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(0, 1, 0));
//init.AddChild(setVelocity);
//var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
//setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
//setLifetime.GetInputSlot(0).value = 15f;
//init.AddChild(setLifetime);


//float kContextOffset = 400.0f;

////Getting Contexts
//var contexts = graph.children.OfType<VFXContext>();
//var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);
//var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);
//var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);
//var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);



//Create Color Operator with Random Generated Color
//var burst = ScriptableObject.CreateInstance<VFXSpawnerBurst>();
//burst.GetInputSlot(0).value = 32.0f;
//spawn.AddChild(burst);
//var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
//setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
//setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
//init.AddChild(setVelocity);
//var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
//setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setLifetime.GetInputSlot(0).value = 1f;
//setLifetime.GetInputSlot(1).value = 10f;
//init.AddChild(setLifetime);
//var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
//output.AddChild(setColor);
//var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
//hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
//hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
//hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

//graph.AddChild(hsvColor);






//float kContextOffset = 400.0f;

//Getting Contexts
//var contexts = graph.children.OfType<VFXContext>();
//var spawn = contexts.LastOrDefault(c => c.contextType == VFXContextType.Spawner);
//var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);
//var update = contexts.LastOrDefault(c => c.contextType == VFXContextType.Update);
//var output = contexts.LastOrDefault(c => c.contextType == VFXContextType.Output);

//Create Color Operator with Random Generated Color
//var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
//constantRate.GetInputSlot(0).value = 32.0f;
//spawn.AddChild(constantRate);
//var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
//setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
//setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
//init.AddChild(setVelocity);
//var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
//setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setLifetime.GetInputSlot(0).value = 1f;
//setLifetime.GetInputSlot(1).value = 10f;
//init.AddChild(setLifetime);
//var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
//output.AddChild(setColor);

//var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
//hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
//hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
//hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

//graph.AddChild(hsvColor);


//if (outputEnum == VFXEnum.VFXOutputEnum.None)
//{
//    Debug.LogError("Select Output Context!");
//    return null;
//}


//Adds Lifetime to the current VFX Graph
//public static void LifetimeModule(VisualEffectAsset vfx, VFXEnum.VFXRandomSetting randomSetting)
//{
//    var graph = vfx.GetResource()?.GetOrCreateGraph();
//    var contexts = graph.children.OfType<VFXContext>();
//    var init = contexts.LastOrDefault(c => c.contextType == VFXContextType.Init);

//    //Set Velocity Random per Component 
//    var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
//    setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);

//    //Random Setting
//    if (randomSetting == VFXEnum.VFXRandomSetting.Off) {
//        setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
//    }
//    if (randomSetting == VFXEnum.VFXRandomSetting.PerComponent)
//    {
//        setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
//    }
//    if (randomSetting == VFXEnum.VFXRandomSetting.Uniform)
//    {
//        setLifetime.SetSettingValue("Random", Block.RandomMode.Uniform);
//    }

//    //Random Input
//    if (randomSetting == VFXEnum.VFXRandomSetting.Off)
//    {
//        setLifetime.GetInputSlot(0).value = 10.0f;
//    }
//    else
//    {
//        setLifetime.GetInputSlot(0).value = 1.0f;
//        setLifetime.GetInputSlot(1).value = 10.0f;
//    }

//    init.AddChild(setLifetime);
//}

//var contexts = graph.children.OfType<VFXContext>();   
//var spawner = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);
//var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);
//var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);

//var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
//spawner.position = new Vector2(0, 0);
//var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
//init.SetSettingValue("capacity", 1024u);
//init.position = new Vector2(0, gapAmount);
//var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
//update.position = new Vector2(0, gapAmount * 2);
//if (outputEnum == VFXEnum.VFXOutputEnum.VFXPlanarPrimitiveOutput)
//{
//    var output = ScriptableObject.CreateInstance<VFXPlanarPrimitiveOutput>();
//    output.position = new Vector2(0, gapAmount * 3);
//    update.LinkTo(output);
//    graph.AddChild(output);
//}

//if (outputEnum == VFXEnum.VFXOutputEnum.VFXMeshOutput)
//{
//    var output = ScriptableObject.CreateInstance<VFXMeshOutput>();
//    output.position = new Vector2(0, gapAmount * 3);
//    update.LinkTo(output);
//    graph.AddChild(output);
//}

//if (outputEnum == VFXEnum.VFXOutputEnum.VFXURPLitMeshOutput)
//{
//    var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitMeshOutput, Unity.RenderPipelines.Universal.Editor");
//    if (type != null)
//    {
//        var output = ScriptableObject.CreateInstance(type) as VFXContext;
//        Debug.Log("Created URP Lit Output: " + output);

//        output.position = new Vector2(0, gapAmount * 3);
//        update.LinkTo(output);
//        graph.AddChild(output);
//    }
//    else
//    {
//        Debug.LogError("Couldn't find URP Lit Output type!");
//        return null;
//    }
//}

//if (outputEnum == VFXEnum.VFXOutputEnum.VFXURPLitPlanarPrimitiveOutput)
//{
//    var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitPlanarPrimitiveOutput, Unity.RenderPipelines.Universal.Editor");
//    if (type != null)
//    {
//        var output = ScriptableObject.CreateInstance(type) as VFXContext;
//        Debug.Log("Created URP Lit Output: " + output);

//        output.position = new Vector2(0, gapAmount * 3);
//        update.LinkTo(output);
//        graph.AddChild(output);
//    }
//    else
//    {
//        Debug.LogError("Couldn't find URP Lit Output type!");
//        return null;
//    }
//}

//if (outputEnum == VFXEnum.VFXOutputEnum.VFXShaderGraphMesh)
//{
//    var type = Type.GetType("UnityEditor.VFX.VFXComposedParticleOutput" + ", Unity.VisualEffectGraph.Editor");
//    if (type == null)
//    {
//        Debug.LogError("Could not find Type!");
//        return null;
//    }

//    var output = ScriptableObject.CreateInstance(type) as VFXContext;
//    output.SetSettingValue("m_Topology", new ParticleTopologyMesh());
//    update.LinkTo(output);
//    graph.AddChild(output);
//}

//if (outputEnum == VFXEnum.VFXOutputEnum.VFXShaderGraphQuad)
//{
//    var type = Type.GetType("UnityEditor.VFX.VFXComposedParticleOutput" + ", Unity.VisualEffectGraph.Editor");
//    if (type == null)
//    {
//        Debug.LogError("Could not find Type!");
//        return null;
//    }

//    var output = ScriptableObject.CreateInstance(type) as VFXContext;
//    output.SetSettingValue("m_Topology", new ParticleTopologyPlanarPrimitive(VFXPrimitiveType.Quad));
//    update.LinkTo(output);
//    graph.AddChild(output);
//}







//Generates an Empty VFX Graph with all Main Modules
//private static VFXGraph GenerateEmptyTemplate(VisualEffectAsset vfx, float gapAmount)
//{
//    if (vfx == null)
//    {
//        Debug.LogError("No VFX Asset Detected! Ensure Asset is Referenced!");
//        return null;
//    }

//    //Resets Graph
//    var graph = vfx.GetResource()?.GetOrCreateGraph();
//    graph.RemoveAllChildren();

//    //Creates Spawn Module
//    var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
//    spawner.label = UnityEngine.Random.Range(0, 10000).ToString();
//    spawner.position = new Vector2(0, 0);

//    //Creates Init Module
//    var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
//    init.SetSettingValue("capacity", 1024u);
//    init.label = UnityEngine.Random.Range(0, 10000).ToString();
//    init.position = new Vector2(0, gapAmount);

//    //Creates Update Module
//    var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
//    update.label = UnityEngine.Random.Range(0, 10000).ToString();
//    update.position = new Vector2(0, gapAmount * 2);

//    //Creates Output Module
//    var output = ScriptableObject.CreateInstance<VFXMeshOutput>();
//    output.label = UnityEngine.Random.Range(0, 10000).ToString();
//    output.position = new Vector2(0, gapAmount * 3);

//    spawner.LinkTo(init);
//    init.LinkTo(update);
//    update.LinkTo(output);

//    graph.AddChild(spawner);
//    graph.AddChild(init);
//    graph.AddChild(update);
//    graph.AddChild(output);

//    return graph;
//}


////Random Setting
//if (randomSetting == VFXMaker.VFXRandomSetting.Off)
//{
//    setSize.SetSettingValue("Random", Block.RandomMode.Off);
//}
//if (randomSetting == VFXMaker.VFXRandomSetting.PerComponent)
//{
//    setSize.SetSettingValue("Random", Block.RandomMode.PerComponent);
//}
//if (randomSetting == VFXMaker.VFXRandomSetting.Uniform)
//{
//    setSize.SetSettingValue("Random", Block.RandomMode.Uniform);
//}

////Random Input
//if (randomSetting == VFXMaker.VFXRandomSetting.Off)
//{
//    setSize.GetInputSlot(0).value = 1.0f;
//}
//else
//{
//    setSize.GetInputSlot(0).value = 1.0f;
//    setSize.GetInputSlot(1).value = 10.0f;
//}




//Set Velocity Random per Component 
//var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
//setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
//setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
//init.AddChild(setVelocity);
////Add Gravity
//var gravity = ScriptableObject.CreateInstance<Gravity>();
//update.AddChild(gravity);


//public ExposedPropertyInfo() { }


//Debug.Log(valueType);

//TryGetStringMember(p, "name") ?? TryGetStringMember(p, "m_Name") ?? "<unknown>";

//Debug.Log(name);

//public class VFXURPLitOutputWrapper : UnityEditor.VFX.URP.VFXURPLitOutput { }


//Link getPosition -> rotate3D
//rotate3D.GetInputSlot(0).AddChild(getPosition.GetOutputSlot(0));


//var rotate3D = ScriptableObject.CreateInstance<Operator.Rotate3D>();
//var getPosition = ScriptableObject.CreateInstance<VFXAttributeParameter>();
//getPosition.SetSettingValue("attribute", VFXAttribute.Position.name);
//rotate3D.GetInputSlot(0).AddChild(getPosition.GetOutputSlot(0));
//getPosition.GetOutputSlot(0).Attach(rotate3D);

//setPosition.SetSettingValue("random", Block.RandomMode.Off);
//setLifetime.GetInputSlot(1).value = 10f;
//setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
//setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));



//Create Color Operator with Random Generated Color
//var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
//hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
//hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
//hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

//graph.AddChild(hsvColor);



//Set Color
//var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
//setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
//output.AddChild(setColor);



//string enumString = enumTest.ToString();
//VFXMaker.VFXEnumTest enumTest

//Gets All Exposed Params from a VisualEffectAsset
//public static VFXParameter[] GetExposedParameters(VisualEffectAsset asset)
//{
//    if (asset == null)
//        return new VFXParameter[0];

//    var graph = asset.GetResource()?.GetOrCreateGraph();
//    if (graph == null)
//        return new VFXParameter[0];

//    // Parameters are children of the graph
//    var parameters = graph.children
//        .OfType<VFXParameter>()
//        .Where(p => p.exposed);   // filter only exposed ones
//        //.ToArray();

//    return (VFXParameter[])parameters;
//}