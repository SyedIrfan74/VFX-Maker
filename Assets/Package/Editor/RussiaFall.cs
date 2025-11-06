using UnityEditor;
using UnityEditor.VFX;
using UnityEngine;
using UnityEngine.VFX;

public static class RussiaFall
{
    //protected override void Init()
    //{
    //    base.Init();
    //}

    public static void CreateVFXAsset(string path, string newAssetName, VisualEffectAsset vfx)
    {
        Debug.Log("Creating VFX");

        //Valid File Path Check
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"Folder not found! {path}");
            return;
        }

        string assetPath = $"{path}/{newAssetName}.vfx";

        //Creates and saves Asset
        var vfxAsset = VisualEffectAssetEditorUtility.CreateNewAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        vfx = vfxAsset;

        Debug.Log($"Created new VFX Graph at: {path}");
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
}
