using UnityEngine;
using System.Collections;
using UnityEditor;

[System.Serializable]
public class EasyColliderPreferences : ScriptableObject
{
    public static string PreferencesPath = "Assets/EasyColliderEditor/EasyColliderPreferences.asset";

    //Collider高亮显示
    [SerializeField] private Color _hoverColliderColour;
    public Color HoverColliderColour { get { return _hoverColliderColour; } set { _hoverColliderColour = value; EditorUtility.SetDirty(this); } }

    //Collider选中显示
    [SerializeField] private Color _selectedColliderColour;
    public Color SelectedColliderColour { get { return _selectedColliderColour; } set { _selectedColliderColour = value; } }

    //选中Collider按键
    [SerializeField] private KeyCode _colliderSelectKeyCode;
    public KeyCode ColliderSelectKeyCode { get { return _colliderSelectKeyCode; } set { _colliderSelectKeyCode = value; } }

    //选中顶点按键
    [SerializeField] private KeyCode _vertSelectKeyCode;
    public KeyCode VertSelectKeyCode {get {return _vertSelectKeyCode;} set { _vertSelectKeyCode = value; } }

    //显示顶点的大小
    [SerializeField] private float _displayVerticesScaling;
    public float DisplayVerticesScaling { get { return _displayVerticesScaling;} set { _displayVerticesScaling = value;} }

    //普通状态下顶点颜色
    [SerializeField] private Color _displayVerticesColour;
    public Color DisplayVerticesColour { get { return _displayVerticesColour; } set { _displayVerticesColour = value; } }

    //高亮顶点颜色
    [SerializeField] private float _hoverVertScaling;
    public float HoverVertScaling { get { return _hoverVertScaling; } set { _hoverVertScaling = value; } }

    //高亮顶点大小
    [SerializeField]  private Color _hoverVertColour;
    public Color HoverVertColour { get { return _hoverVertColour; } set { _hoverVertColour = value; EditorUtility.SetDirty(this); } }

    //选中顶点大小
    [SerializeField] private float _selectedVertScaling;
    public float SelectedVertScaling { get { return _selectedVertScaling; } set { _selectedVertScaling = value; } }

    //选中顶点颜色
    [SerializeField] private Color _selectedVertCol;
    public Color SelectedVertexColour { get { return _selectedVertCol; } set { _selectedVertCol = value; } }

    //已被选中顶点的高亮大小
    [SerializeField] private float _overlapSelectedVertScale;
    public float OverlapSelectedVertScale { get { return _overlapSelectedVertScale; } set { _overlapSelectedVertScale = value;} }

    //已被选中顶点的高亮颜色
    [SerializeField] private Color _overlapSelectedVertColour;
    public Color OverlapSelectedVertColour { get { return _overlapSelectedVertColour;} set {_overlapSelectedVertColour = value;} }

    void OnDisable()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    void OnDestroy()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    
    public void SetDefaultValues()
    {
        DisplayVerticesScaling = 0.1F;
        DisplayVerticesColour = Color.blue;

        HoverVertScaling = 0.15F;
        HoverVertColour = Color.cyan;

        SelectedVertScaling = 0.2F;
        SelectedVertexColour = Color.green;

        OverlapSelectedVertScale = 0.175F;
        OverlapSelectedVertColour = Color.red;
        VertSelectKeyCode = KeyCode.Keypad0;

        SelectedColliderColour = Color.red;
        HoverColliderColour = Color.black;
        _colliderSelectKeyCode = KeyCode.Keypad1;
    }

}