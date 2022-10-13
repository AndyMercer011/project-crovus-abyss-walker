using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TabToggle))]
[CanEditMultipleObjects]
public class TabToggleEditor : ToggleEditor
{
    private SerializedProperty m_NameTextProperty;

    private SerializedProperty m_IconProperty;

    private SerializedProperty m_PageIconsProperty;

    private SerializedProperty m_DisableWhenOffProperty;

    private SerializedProperty m_ColorOnProperty;

    private SerializedProperty m_ColorOffProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_NameTextProperty = base.serializedObject.FindProperty("m_NameText");
        m_IconProperty = base.serializedObject.FindProperty("m_Icon");
        m_PageIconsProperty = base.serializedObject.FindProperty("m_PageIcons");
        m_DisableWhenOffProperty = base.serializedObject.FindProperty("m_DisableWhenOff");
        m_ColorOnProperty = base.serializedObject.FindProperty("m_ColorOn");
        m_ColorOffProperty = base.serializedObject.FindProperty("m_ColorOff");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        base.serializedObject.Update();

        EditorGUILayout.PropertyField(m_NameTextProperty);
        EditorGUILayout.PropertyField(m_IconProperty);
        EditorGUILayout.PropertyField(m_PageIconsProperty);
        EditorGUILayout.PropertyField(m_DisableWhenOffProperty);
        EditorGUILayout.PropertyField(m_ColorOnProperty);
        EditorGUILayout.PropertyField(m_ColorOffProperty);

        base.serializedObject.ApplyModifiedProperties();
    }
}
