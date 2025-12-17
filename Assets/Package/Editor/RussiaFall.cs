using System;
using System.Collections.Generic;
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

    //Generates an Empty VFX Graph with all Main Modules
    private static VFXGraph GenerateEmptyTemplate(VisualEffectAsset vfx, float gapAmount)
    {
        if (vfx == null)
        {
            Debug.LogError("No VFX Asset Detected! Ensure Asset is Referenced!");
            return null;
        }

        //Resets Graph
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        graph.RemoveAllChildren();

        //Creates Spawn Module
        var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
        spawner.label = UnityEngine.Random.Range(0, 10000).ToString();
        spawner.position = new Vector2(0, 0);

        //Creates Init Module
        var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
        init.SetSettingValue("capacity", 1024u);
        init.label = UnityEngine.Random.Range(0, 10000).ToString();
        init.position = new Vector2(0, gapAmount);

        //Creates Update Module
        var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
        update.label = UnityEngine.Random.Range(0, 10000).ToString();
        update.position = new Vector2(0, gapAmount * 2);

        //Creates Output Module
        var output = ScriptableObject.CreateInstance<VFXPlanarPrimitiveOutput>();
        output.label = UnityEngine.Random.Range(0, 10000).ToString();
        output.position = new Vector2(0, gapAmount * 3);

        spawner.LinkTo(init);
        init.LinkTo(update);
        update.LinkTo(output);

        graph.AddChild(spawner);
        graph.AddChild(init);
        graph.AddChild(update);
        graph.AddChild(output);

        return graph;
    }

    //Generates an Empty VFX Graph with all Main Modules Diff
    private static VFXGraph GenerateEmptyTemplateDiff(VisualEffectAsset vfx, float gapAmount)
    {
        if (vfx == null)
        {
            Debug.LogError("No VFX Asset Detected! Ensure Asset is Referenced!");
            return null;
        }

        //Resets Graph
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        graph.RemoveAllChildren();

        //Creates Spawn Module
        var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
        spawner.label = UnityEngine.Random.Range(0, 10000).ToString();
        spawner.position = new Vector2(0, 0);

        //Creates Init Module
        var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
        init.SetSettingValue("capacity", 1024u);
        init.label = UnityEngine.Random.Range(0, 10000).ToString();
        init.position = new Vector2(0, gapAmount);

        //Creates Update Module
        var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
        update.label = UnityEngine.Random.Range(0, 10000).ToString();
        update.position = new Vector2(0, gapAmount * 2);

        //Creates Output Module
        //var output = ScriptableObject.CreateInstance<VFXMeshOutput>();
        

        var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitMeshOutput, Unity.RenderPipelines.Universal.Editor");
        //var type = Type.GetType("UnityEditor.VFX.URP.VFXURPLitPlanarPrimitiveOutput, Unity.RenderPipelines.Universal.Editor");
        if (type != null)
        {
            var output = ScriptableObject.CreateInstance(type) as VFXContext;
            Debug.Log("Created URP Lit Output: " + output);

            output.label = UnityEngine.Random.Range(0, 10000).ToString();
            output.position = new Vector2(0, gapAmount * 3);

            spawner.LinkTo(init);
            init.LinkTo(update);
            update.LinkTo(output);

            graph.AddChild(spawner);
            graph.AddChild(init);
            graph.AddChild(update);
            graph.AddChild(output);

            return graph;
        }
        else
        {
            Debug.LogError("Couldn't find URP Lit Output type!");

            return null;
        }
    }
    
    //PRESETS START
    //Creates a Basic Emitter VFX Graph
    public static void GenerateConstantEmitter(VisualEffectAsset vfx, bool overrule)
    {
        var graph = GenerateEmptyTemplate(vfx, 400);

        if (graph == null) return;

        float kContextOffset = 400.0f;

        //Getting Contexts
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //Constant Rate Spawner
        var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
        constantRate.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(constantRate);

        //Set Velocity Random per Component 
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
        setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
        init.AddChild(setVelocity);

        //Set Lifetime Random per Component
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
        setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setLifetime.GetInputSlot(0).value = 1f;
        setLifetime.GetInputSlot(1).value = 10f;
        init.AddChild(setLifetime);

        //Set Color
        var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
        output.AddChild(setColor);

        //Create Color Operator with Random Generated Color
        var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
        hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
        hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

        graph.AddChild(hsvColor);
    }
    public static void GenerateBurstEmitter(VisualEffectAsset vfx)
    {
        var graph = GenerateEmptyTemplate(vfx, 400);

        if (graph == null) return;

        float kContextOffset = 400.0f;

        //Getting Contexts
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //Burst Spawner
        var burst = ScriptableObject.CreateInstance<VFXSpawnerBurst>();
        burst.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(burst);

        //Set Velocity Random per Component 
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
        setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
        init.AddChild(setVelocity);

        //Set Lifetime Random per Component
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
        setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setLifetime.GetInputSlot(0).value = 1f;
        setLifetime.GetInputSlot(1).value = 10f;
        init.AddChild(setLifetime);

        //Set Color
        var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
        output.AddChild(setColor);

        //Create Color Operator with Random Generated Color
        var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
        hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
        hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

        graph.AddChild(hsvColor);
    }
    public static void GenerateSpiralEmitter(VisualEffectAsset vfx)
    {
        var graph = GenerateEmptyTemplateDiff(vfx, 400);

        if (graph == null) return;

        //float kContextOffset = 400.0f;

        //Getting Contexts
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //INIT START --------------------------------------------------------------------
        //Constant Spawner
        var constant = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
        constant.GetInputSlot(0).value = 100.0f;
        spawn.AddChild(constant);

        //Set Velocity  
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        setVelocity.SetSettingValue("Random", Block.RandomMode.Off);
        setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(0, 1, 0));
        init.AddChild(setVelocity);

        //Set Lifetime 
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
        setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
        setLifetime.GetInputSlot(0).value = 15f;
        init.AddChild(setLifetime);
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
    public static void GenerateGravityEmitter(VisualEffectAsset vfx)
    {
        var graph = GenerateEmptyTemplate(vfx, 400);

        if (graph == null) return;

        float kContextOffset = 400.0f;

        //Getting Contexts
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //Constant Rate Spawner
        var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
        constantRate.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(constantRate);

        //Set Velocity Random per Component 
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
        setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
        init.AddChild(setVelocity);

        //Set Lifetime Random per Component
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);
        setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        setLifetime.GetInputSlot(0).value = 1f;
        setLifetime.GetInputSlot(1).value = 10f;
        init.AddChild(setLifetime);

        //Add Gravity
        var gravity = ScriptableObject.CreateInstance<Gravity>();
        update.AddChild(gravity);

        //Set Color
        var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
        output.AddChild(setColor);

        //Create Color Operator with Random Generated Color
        var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
        hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
        hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

        graph.AddChild(hsvColor);
    }
    //PRESETS END

    //MODULES START
    //Adds VFX Spawn Context to the Current VFX Graph
    public static void SpawnModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Creates Spawn Module
        var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
        graph.AddChild(spawner);
    }

    //Adds VFX Initialise Context to the Current VFX Graph
    public static void InitialiseModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Creates Initialize Module
        var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
        graph.AddChild(init);

        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);

        spawn.LinkTo(init);
    }

    //Adds VFX Update Context to the Current VFX Graph
    public static void UpdateModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();

        //Creates Update Module
        var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
        graph.AddChild(update);

        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);

        init.LinkTo(update);
    }

    //Adds VFX Output Context to the Current VFX Graph
    public static void OutputModule(VisualEffectAsset vfx, VFXMaker.VFXOutputEnum VFXEnum)
    {
        //if (VFXEnum == VFXMaker.VFXOutputEnum.NONE)
        //{
        //    Debug.LogError("Please Select Output Type!");
        //    return;
        //}

        var graph = vfx.GetResource()?.GetOrCreateGraph();

        var type = Type.GetType("UnityEditor.VFX.URP." + VFXEnum.ToString() +  ", Unity.RenderPipelines.Universal.Editor");
        if (type == null)
        {
            Debug.LogError("Could not find Type!");
            return;
        }

        var output = ScriptableObject.CreateInstance(type) as VFXContext;
        graph.AddChild(output);

        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);

        update.LinkTo(output);
    }

    //Adds Constant Spawner Node to Spawn Context
    public static void ConstantModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);

        //Constant Rate Spawner
        var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
        constantRate.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(constantRate);
    }

    //Adds Burst Spawner Node to Spawn Context
    public static void BurstModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var spawn = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Spawner);

        //Constant Rate Spawner
        var burst = ScriptableObject.CreateInstance<VFXSpawnerBurst>();
        burst.GetInputSlot(0).value = 32.0f;
        spawn.AddChild(burst);
    }

    //Adds gravity to the current VFX Graph
    public static void GravityModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var update = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Update);

        //Add Gravity
        var gravity = ScriptableObject.CreateInstance<Gravity>();
        update.AddChild(gravity);
    }

    //Adds Lifetime to the current VFX Graph
    public static void LifetimeModule(VisualEffectAsset vfx, VFXMaker.VFXRandomSetting randomSetting)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);

        //Set Velocity Random per Component 
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);

        //Random Setting
        if (randomSetting == VFXMaker.VFXRandomSetting.Off) {
            setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.PerComponent)
        {
            setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.Uniform)
        {
            setLifetime.SetSettingValue("Random", Block.RandomMode.Uniform);
        }

        //Random Input
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setLifetime.GetInputSlot(0).value = 10.0f;
        }
        else
        {
            setLifetime.GetInputSlot(0).value = 1.0f;
            setLifetime.GetInputSlot(1).value = 10.0f;
        }

        init.AddChild(setLifetime);
    }

    //Adds Lifetime to the current VFX Graph
    public static void LifetimeModule1(VisualEffectAsset vfx, VFXMaker.VFXRandomSetting randomSetting, VFXMaker.VFXCompositionSetting compositionSetting)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);

        //Set Velocity Random per Component 
        var setLifetime = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setLifetime.SetSettingValue("attribute", VFXAttribute.Lifetime.name);

        if (compositionSetting == VFXMaker.VFXCompositionSetting.Overwrite)
        {
            setLifetime.Composition = AttributeCompositionMode.Overwrite;
        }
        if (compositionSetting == VFXMaker.VFXCompositionSetting.Blend)
        {
            setLifetime.Composition = AttributeCompositionMode.Blend;
        }
        if (compositionSetting == VFXMaker.VFXCompositionSetting.Multiply)
        {
            setLifetime.Composition = AttributeCompositionMode.Multiply;
        }
        if (compositionSetting == VFXMaker.VFXCompositionSetting.Add)
        {
            setLifetime.Composition = AttributeCompositionMode.Add;
        }

        //Random Setting
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setLifetime.SetSettingValue("Random", Block.RandomMode.Off);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.PerComponent)
        {
            setLifetime.SetSettingValue("Random", Block.RandomMode.PerComponent);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.Uniform)
        {
            setLifetime.SetSettingValue("Random", Block.RandomMode.Uniform);
        }

        //Random Input
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setLifetime.GetInputSlot(0).value = 10.0f;
        }
        else
        {
            setLifetime.GetInputSlot(0).value = 1.0f;
            setLifetime.GetInputSlot(1).value = 10.0f;
        }

        init.AddChild(setLifetime);
    }

    //Adds Velocity to the current VFX Graph
    public static void VelocityModule(VisualEffectAsset vfx, VFXMaker.VFXRandomSetting randomSetting)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var init = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Init);

        //Set Velocity Random per Component 
        var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
        //setVelocity.SetSettingValue("composition", VFXComposition);

        //Random Setting
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setVelocity.SetSettingValue("Random", Block.RandomMode.Off);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.PerComponent)
        {
            setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.Uniform)
        {
            setVelocity.SetSettingValue("Random", Block.RandomMode.Uniform);
        }

        //Random Input
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(1, 1.5f, 1));
        }
        else
        {
            setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
            setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
        }

        init.AddChild(setVelocity);
    }

    //Adds Velocity to the current VFX Graph
    public static void SizeModule(VisualEffectAsset vfx, VFXMaker.VFXRandomSetting randomSetting)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //Set Velocity Random per Component 
        var setSize = ScriptableObject.CreateInstance<Block.SetAttribute>();
        setSize.SetSettingValue("attribute", VFXAttribute.Size.name);

        //Random Setting
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setSize.SetSettingValue("Random", Block.RandomMode.Off);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.PerComponent)
        {
            setSize.SetSettingValue("Random", Block.RandomMode.PerComponent);
        }
        if (randomSetting == VFXMaker.VFXRandomSetting.Uniform)
        {
            setSize.SetSettingValue("Random", Block.RandomMode.Uniform);
        }

        //Random Input
        if (randomSetting == VFXMaker.VFXRandomSetting.Off)
        {
            setSize.GetInputSlot(0).value = 1.0f;
        }
        else
        {
            setSize.GetInputSlot(0).value = 1.0f;
            setSize.GetInputSlot(1).value = 10.0f;
        }

        output.AddChild(setSize);
    }

    //Adds Orient to the current VFX Graph
    public static void OrientModule(VisualEffectAsset vfx)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        var contexts = graph.children.OfType<VFXContext>();
        var output = contexts.FirstOrDefault(c => c.contextType == VFXContextType.Output);

        //Set Velocity Random per Component 
        var orient = ScriptableObject.CreateInstance<Block.Orient>();
        orient.mode = Orient.Mode.FaceCameraPlane;

        output.AddChild(orient);
    }

    //Add Exposed Float Property to the Graph
    public static void AddFloatProperty(VisualEffectAsset vfx, string propertyName, float defaultValue)
    {
        var graph = vfx.GetResource()?.GetOrCreateGraph();
        if (graph == null)
        {
            Debug.LogError("Unable to access VFX Graph.");
            return;
        }

        // Create a new VFXParameter of type Float
        var param = ScriptableObject.CreateInstance<VFXParameter>();
        param.Init(typeof(float));             // Parameter Type
        //param.exposed = true;                  // Make it exposed
        //param.exposedName = propertyName;             // Property Name

        // Set default value
        param.value = defaultValue;

        // Add to the graph
        graph.AddChild(param);

        // Save
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx));
        //Debug.Log($"Created exposed float: {propertyName}");
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

    public static void SetExposedName(VisualEffectAsset asset, ExposedPropertyInfo info, object newValue)
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

    private static string TryGetStringMember(object obj, string memberName)
    {
        var t = obj.GetType();
        var pi = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //Debug.Log(pi);
        if (pi != null)
        {
            try { var v = pi.GetValue(obj); return v?.ToString(); } catch { }
        }
        var fi = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fi != null)
        {
            try { var v = fi.GetValue(obj); return v?.ToString(); } catch { }
        }
        return null;
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


    private static void Generate(VisualEffectAsset vfx)
    {
        if (vfx != null)
        {
            Debug.Log("REGENERATE VFX");
            float kContextOffset = 400.0f;

            var graph = vfx.GetResource()?.GetOrCreateGraph();
            graph.RemoveAllChildren();

            var spawner = ScriptableObject.CreateInstance<VFXBasicSpawner>();
            spawner.label = UnityEngine.Random.Range(0, 10000).ToString();
            spawner.position = new Vector2(0, 0);

            var constantRate = ScriptableObject.CreateInstance<VFXSpawnerConstantRate>();
            constantRate.GetInputSlot(0).value = 32.0f;
            spawner.AddChild(constantRate);

            var init = ScriptableObject.CreateInstance<VFXBasicInitialize>();
            init.SetSettingValue("capacity", 1024u);
            init.label = UnityEngine.Random.Range(0, 10000).ToString();
            init.position = new Vector2(0, kContextOffset);

            var setVelocity = ScriptableObject.CreateInstance<Block.SetAttribute>();
            setVelocity.SetSettingValue("attribute", VFXAttribute.Velocity.name);
            setVelocity.SetSettingValue("Random", Block.RandomMode.PerComponent);
            setVelocity.GetInputSlot(0).value = (Vector)(new Vector3(-1, 0, -1));
            setVelocity.GetInputSlot(1).value = (Vector)(new Vector3(1, 1.5f, 1));
            init.AddChild(setVelocity);

            var update = ScriptableObject.CreateInstance<VFXBasicUpdate>();
            update.label = UnityEngine.Random.Range(0, 10000).ToString();
            update.position = new Vector2(0, kContextOffset * 2);

            var gravity = ScriptableObject.CreateInstance<Block.Gravity>();
            gravity.GetInputSlot(0).value = (Vector)(new Vector3(0, -1, 0));
            update.AddChild(gravity);

            //var collider = ScriptableObject.CreateInstance<CollisionPlane>();
            //collider.GetInputSlot(1).value = UnityEngine.Random.Range(0.25f, 0.75f); // bounce
            //collider.GetInputSlot(2).value = UnityEngine.Random.Range(0.05f, 0.25f); // friction
            //update.AddChild(collider);

            var output = ScriptableObject.CreateInstance<VFXPlanarPrimitiveOutput>();
            output.label = UnityEngine.Random.Range(0, 10000).ToString();
            output.position = new Vector2(0, kContextOffset * 3);

            var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
            setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
            output.AddChild(setColor);

            var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
            hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
            hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
            hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

            spawner.LinkTo(init);
            init.LinkTo(update);
            update.LinkTo(output);

            graph.AddChild(spawner);
            graph.AddChild(init);
            graph.AddChild(update);
            graph.AddChild(output);
            graph.AddChild(hsvColor);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(vfx.GetResource()));
        }
    }
}
[Serializable]
public class ExposedPropertyInfo
{
    public string Name;
    public Type ValueType;
    public object Value;
    public object InternalParameter; 

    public ExposedPropertyInfo(string name, Type valueType, object value, object internalRef)
    {
        Name = name;
        ValueType = valueType;
        Value = value;
        InternalParameter = internalRef;
    }
}



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