using UnityEditor;
using UnityEditor.VFX;

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