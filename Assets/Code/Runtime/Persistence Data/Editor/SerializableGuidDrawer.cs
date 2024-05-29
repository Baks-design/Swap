#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SwapChains.Runtime.PersistenceData
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var value0 = property.FindPropertyRelative("Part1");
            var value1 = property.FindPropertyRelative("Part2");
            var value2 = property.FindPropertyRelative("Part3");
            var value3 = property.FindPropertyRelative("Part4");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (value0 is not null && value1 is not null && value2 is not null && value3 is not null)
            {
                EditorGUI.SelectableLabel(
                    position,
                    new StringBuilder()
                        .AppendFormat("{0:X8}", (uint)value0.intValue)
                        .AppendFormat("{0:X8}", (uint)value1.intValue)
                        .AppendFormat("{0:X8}", (uint)value2.intValue)
                        .AppendFormat("{0:X8}", (uint)value3.intValue)
                        .ToString()
                );
            }
            else
                EditorGUI.SelectableLabel(position, "GUID Not Initialized");

            EditorGUI.EndProperty();
        }
    }
}
#endif
