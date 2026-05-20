using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections;

[CustomPropertyDrawer(typeof(EnemyGroup))]
public class EnemyGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. Tenta pegar o objeto real de forma segura
        object targetObject = GetTargetObjectOfProperty(property);

        if (targetObject is EnemyGroup group)
        {
            string errorMessage;
            // 2. Se não for válido, desenha o fundo vermelho
            if (!group.IsValid(out errorMessage))
            {
                // Ajusta a altura para cobrir o campo expandido se necessário
                Rect bgRect = position;
                EditorGUI.DrawRect(bgRect, new Color(0.6f, 0.1f, 0.1f, 0.25f));
                label.text = "⚠️ " + label.text + " (Erro de Peso!)";
            }
        }

        // 3. Desenha a propriedade original
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    // MÉTODO SEGURO PARA PEGAR O OBJETO DENTRO DE ARRAYS
    private object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        string[] elements = path.Split('.');

        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    private object GetValue_Imp(object source, string name)
    {
        if (source == null) return null;
        Type type = source.GetType();
        FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null) return null;
        return f.GetValue(source);
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
}