using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ElementBehaviour))]
public class ElementBehaviourDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 路径是_elements.Array.data [x]
        var splitPath = property.propertyPath.Split('.');
        var isArrayElement = splitPath[splitPath.Length - 2] == "Array";
        if (isArrayElement && property.objectReferenceValue != null)
        {
            // 获取数组索引
            var arrayIndexStr = splitPath[splitPath.Length - 1].Replace("data[", "").Replace("]", "");
            var arrayIndex = int.Parse(arrayIndexStr);
            // 创建字符串_elements.Array.data [{0}]
            var formatSplitPath = splitPath;
            formatSplitPath[formatSplitPath.Length - 1] = "data[{0}]";
            var formatPath = string.Join(".", formatSplitPath);
            // 获取上一个元素和下一个元素
            var previousElementPath = string.Format(formatPath, arrayIndex - 1);
            var nextElementPath = string.Format(formatPath, arrayIndex + 1);
            var previousElement = property.serializedObject.FindProperty(previousElementPath);
            var nextElement = property.serializedObject.FindProperty(nextElementPath);
            var isLastElement = nextElement == null;
            // 如果有前一个元素，并且最后一个元素（刚添加的元素），以及前一个元素和引用一样，则删除引用
            if (arrayIndex >= 1 && isLastElement && previousElement.objectReferenceValue == property.objectReferenceValue)
            {
                property.objectReferenceValue = null;
            }

        }

        // 通常绘制属性
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            EditorGUI.PropertyField(position, property);
        }

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

}