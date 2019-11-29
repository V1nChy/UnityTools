
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//自定义Tset脚本
[CustomEditor(typeof(ParticleEffectScript))] 
public class MyParticleEffectUI : Editor {

    private string[] m_Label = new string[20];

    private Rect _windowSize = new Rect(8, 24, 300, 100);

    void OnSceneGUI()
    {
        ParticleEffectScript particleEffectScript = (ParticleEffectScript)target;

        m_Label[0] = GetParticleEffectData.GetGetRuntimeMemorySizeStr(particleEffectScript.gameObject);

        if (EditorApplication.isPlaying)
        {
            m_Label[1] = GetParticleEffectData.GetOnlyParticleEffecDrawCallStr();
            m_Label[2] = GetParticleEffectData.GetParticleCountStr(particleEffectScript);
            m_Label[3] = GetParticleEffectData.GetPixDrawAverageStr(particleEffectScript);
            m_Label[4] = GetParticleEffectData.GetPixActualDrawAverageStr(particleEffectScript);
            m_Label[5] = GetParticleEffectData.GetPixRateStr(particleEffectScript);
        }

        GUILayout.Window(1, _windowSize, ShowUI, "Params Window");
    }

    void ShowUI(int id)
    {
        for (int i = 0; i < m_Label.Length; i++)
        {
            if (!string.IsNullOrEmpty(m_Label[i]))
            {
                EditorGUILayout.LabelField(m_Label[i]);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ParticleEffectScript particleEffectScript = (ParticleEffectScript)target;

        string autoCullingTips = GetParticleEffectData.GetCullingSupportedString(particleEffectScript.gameObject);
        if (!string.IsNullOrEmpty(autoCullingTips))
        {
            GUILayout.Label("ParticleSystem以下选项会导致无法自动裁剪：", EditorStyles.whiteLargeLabel);
            GUILayout.Label(autoCullingTips);
        }
    }
}
