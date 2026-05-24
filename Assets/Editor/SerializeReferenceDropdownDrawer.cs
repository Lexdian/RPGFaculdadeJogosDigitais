using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SerializeReferenceDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 1. Define a região apenas para a primeira linha (onde fica o Dropdown)
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // 2. Descobre os tipos que herdam da classe abstrata do campo
        var type = fieldInfo.FieldType;
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToArray();

        string[] typeNames = types.Select(t => t.Name).Prepend("Null").ToArray();

        // 3. Identifica o tipo atual do objeto
        int currentIndex = 0;
        if (property.managedReferenceValue != null)
        {
            string currentTypeName = property.managedReferenceValue.GetType().Name;
            currentIndex = Array.IndexOf(typeNames, currentTypeName);
            if (currentIndex == -1) currentIndex = 0; // Fallback caso mude de nome
        }

        // 4. Desenha o Dropdown no Inspector
        int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, typeNames);

        if (newIndex != currentIndex)
        {
            if (newIndex == 0) property.managedReferenceValue = null;
            else property.managedReferenceValue = Activator.CreateInstance(types[newIndex - 1]);

            property.serializedObject.ApplyModifiedProperties();
        }

        // 5. DESENHA OS CAMPOS INTERNOS (O que estava faltando!)
        if (property.managedReferenceValue != null)
        {
            // Move a posição para baixo para não desenhar por cima do dropdown
            Rect childRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height);

            // Desenha as propriedades internas da classe filha de forma recursiva
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(childRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    // 6. DIZ AO UNITY A ALTURA REAL DO CAMPO (Crucial para não encavalar o layout)
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;

        if (property.managedReferenceValue != null)
        {
            // Adiciona a altura de todos os campos internos do objeto selecionado
            totalHeight += EditorGUI.GetPropertyHeight(property, true) + 2;
        }

        return totalHeight;
    }
}