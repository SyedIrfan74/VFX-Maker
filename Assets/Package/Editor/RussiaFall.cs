using System.Linq;
using UnityEditor;
using UnityEditor.VFX;
using UnityEngine;
using UnityEngine.VFX;
using Block = UnityEditor.VFX.Block;
using Operator = UnityEditor.VFX.Operator;

public static class RussiaFall
{
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
    //Creates a Basic Emitter VFX Graph
    public static void GenerateConstantEmitter(VisualEffectAsset vfx)
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
        var graph = GenerateEmptyTemplate(vfx, 400);

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
        rotate3D.GetOutputSlot(0).Link(setPosition.GetInputSlot(0));

        //Get Position, Link to Rotate 3D
        var getPosition = ScriptableObject.CreateInstance<VFXAttributeParameter>();
        getPosition.SetSettingValue("attribute", VFXAttribute.Position.name);
        getPosition.GetOutputSlot(0).Link(rotate3D.GetInputSlot(0));
        
        //Add Rotate3D & getPosition to graph
        graph.AddChild(rotate3D);
        graph.AddChild(getPosition);


        //OUTPUT START --------------------------------------------------------------------
        //Set Color
        var setColor = ScriptableObject.CreateInstance<Block.SetAttribute>();
        //setColor.SetSettingValue("attribute", VFXAttribute.Color.name);
        //setColor.SetSettingValue("");
        output.AddChild(setColor);

        //Create Color Operator with Random Generated Color
        //var hsvColor = ScriptableObject.CreateInstance<Operator.HSVtoRGB>();
        //hsvColor.position = new Vector2(-kContextOffset, kContextOffset * 4.0f);
        //hsvColor.GetInputSlot(0).value = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        //hsvColor.GetOutputSlot(0).Link(setColor.GetInputSlot(0));

        //graph.AddChild(hsvColor);


    }
    public static void GenerateGravityEmitter()
    {
        var block = ScriptableObject.CreateInstance<Block.SetAttribute>();
        var settings = block.GetSettings(true);
        foreach (var s in settings)
        {
            Debug.Log($"Name: {s.name}, Value: {s.value}");
        }
    }
}



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