#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpgradePanelController))]
public class UpgradePanelControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpgradePanelController controller = (UpgradePanelController)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Preview Upgrade Items", GUILayout.Height(30)))
        {
            controller.SendMessage("PreviewUpgradeItemsInEditor");
        }

        if (GUILayout.Button("Clear Preview Items", GUILayout.Height(30)))
        {
            controller.SendMessage("ClearPreviewItems");
        }
    }
}
#endif