using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestParticleEffe{

    private static bool m_Flag = false;

    [InitializeOnLoadMethod]
    static void Init()
    {
        if (EditorSceneManager.GetActiveScene().name == "TestEffect" && !m_Flag)
        {
            m_Flag = true;
            SceneView.onSceneGUIDelegate += OnScene;
            EditorApplication.hierarchyWindowItemOnGUI += Onhierarchy;
        }
        else if (EditorSceneManager.GetActiveScene().name != "TestEffect" && m_Flag)
        {
            m_Flag = false;
            SceneView.onSceneGUIDelegate -= OnScene;
            EditorApplication.hierarchyWindowItemOnGUI -= Onhierarchy;
        }


        EditorSceneManager.sceneOpened += (Scene scene, OpenSceneMode mode) =>
        {
            if (scene.name == "TestEffect" && !m_Flag)
            {
                m_Flag = true;
                SceneView.onSceneGUIDelegate += OnScene;
                EditorApplication.hierarchyWindowItemOnGUI += Onhierarchy;
            }
            else if (scene.name != "TestEffect" && m_Flag)
            {
                m_Flag = false;
                SceneView.onSceneGUIDelegate -= OnScene;
                EditorApplication.hierarchyWindowItemOnGUI -= Onhierarchy;
            }
        };
    }

    private static void OnScene(SceneView sceneView)
    {
        OnChange();
    }

    private static void Onhierarchy(int instanceID, Rect selectionRect)
    {
        OnChange();
    }

    private static void OnChange()
    {
        Event e = Event.current;
        if(e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if(e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                EditorApplication.delayCall = () => { Test(); };
            }
        }
    }

    [MenuItem("GameObject/特效/测试", false, 11)]
    static void Test()
    {
        if (Selection.activeGameObject == null)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != "TestEffect")
        {
            EditorUtility.DisplayDialog("提示", "请先打开TestEffect场景！", "确定");
            return;
        }

        GameObject go = Selection.activeGameObject;
        List<ParticleSystemRenderer> particleSystemRenderer = GetParticleEffectData.GetComponentByType<ParticleSystemRenderer>(go);

        if (particleSystemRenderer.Count == 0)
        {
            Debug.LogError("不是特效无法测试！");
            return;
        }

        List<ParticleEffectScript> particleEffectScript = GetParticleEffectData.GetComponentByType<ParticleEffectScript>(go);

        if (particleEffectScript.Count == 0)
        {
            go.AddComponent<ParticleEffectScript>();
        }

        EditorApplication.isPlaying = true;
    }
}
