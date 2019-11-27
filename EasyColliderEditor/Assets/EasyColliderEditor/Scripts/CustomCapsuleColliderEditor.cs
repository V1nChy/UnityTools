using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CapsuleCollider))]
public class CustomCapsuleColliderEditor : DecoratorEditor
{
    private bool m_IsEdit = false;
    public CustomCapsuleColliderEditor() : base("CapsuleColliderEditor") { }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    public void SetEditMode(bool isEdit)
    {
        m_IsEdit = isEdit;
        Bounds bounds = (this.target as Collider).bounds;
        if (m_IsEdit)
            EditMode.ChangeEditMode(EditMode.SceneViewEditMode.Collider, bounds, EditorInstance);
        else
            EditMode.ChangeEditMode(EditMode.SceneViewEditMode.None, bounds, EditorInstance);
    }
}
