using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
public class SampleSceneEditWindow
{
    private static Rect _windowSize = new Rect(8, 24, 300, 100);
    private static bool _enabled = false;
    private static Color matColor;
    // 加载脚本时调用
    [InitializeOnLoadMethod]
    private static void SampleSceneEdit()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }
    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!GetIsTargetScene())
        {
            return;
        }
        GUILayout.Window(1, _windowSize, DrawWindow, "Sample Window");
    }
    public static void DrawWindow(int id)
    {
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.richText = true;
        EditorGUILayout.LabelField("<color=white>Sample</color>", textStyle);
    }
    private static bool GetIsTargetScene()
    {
        return SceneManager.GetActiveScene().name.Contains("Sample_");
    }
}