using UnityEditor;
using UnityEditor.VFX;
using UnityEngine;

//public class inspectorWatcher : MonoBehaviour
//{
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}



[InitializeOnLoad]
static class InspectorWatcher
{
    public static System.Action<VFXParameter> OnParameterSelected;

    static InspectorWatcher()
    {
        Selection.selectionChanged += UpdateSelection;
    }

    static void UpdateSelection()
    {
        var obj = Selection.activeObject;

        if (obj != null && obj.GetType().Name == "VFXParameter")
        {
            // Cast safely (internal type)
            var param = obj as VFXParameter;
            OnParameterSelected?.Invoke(param);
        }
    }
}