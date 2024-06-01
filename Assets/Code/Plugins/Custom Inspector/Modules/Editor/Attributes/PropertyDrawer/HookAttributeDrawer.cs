using CustomInspector.Extensions;
using CustomInspector.Helpers.Editor;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(HookAttribute))]
    public class HookAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropInfo info = cache.GetInfo(property, attribute, fieldInfo);

            if (info.ErrorMessage != null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, info.ErrorMessage, MessageType.Error);
                return;
            }


            object oldValue = property.GetValue();

            EditorGUI.BeginChangeCheck();
            DrawProperties.PropertyField(position, label, property);
            if (EditorGUI.EndChangeCheck())
            {
                object newValue = property.GetValue();

                HookAttribute a = (HookAttribute)attribute;
                if (a.useHookOnly)
                {
                    //Revert change on property
                    property.SetValue(oldValue);
                }
                //property to instantiation
                property.serializedObject.ApplyModifiedProperties();
                //method on instantiation
                info.hookMethod(property, oldValue, newValue);
                //instantiation to property
                property.serializedObject.ApplyModifiedFields(true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropInfo info = cache.GetInfo(property, attribute, fieldInfo);

            if (info.ErrorMessage != null)
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            else
                return DrawProperties.GetPropertyHeight(label, property);
        }

        static readonly PropInfoCache<PropInfo> cache = new();

        class PropInfo : ICachedPropInfo
        {
            public string ErrorMessage { get; private set; }
            public bool MethodHasParameters { get; private set; }
            /// <summary> A method that executes on with property, oldValue & newValue </summary>
            public Action<SerializedProperty, object, object> hookMethod { get; private set; }

            public PropInfo() { }
            public void Initialize(SerializedProperty property, PropertyAttribute attr, FieldInfo fieldInfo)
            {
                DirtyValue owner = DirtyValue.GetOwner(property);
                HookAttribute attribute = (HookAttribute)attr;
                Type propertyType = fieldInfo.FieldType;

                InvokableMethod method;
                try
                {
                    try
                    {
                        method = property.GetMethodOnOwner(attribute.methodPath);
                        MethodHasParameters = false;
                        ErrorMessage = null;
                    }
                    catch
                    {
                        method = property.GetMethodOnOwner(attribute.methodPath, new Type[] { propertyType, propertyType });
                        MethodHasParameters = true;
                        ErrorMessage = null;
                    }
                }
                catch (MissingMethodException e)
                {
                    ErrorMessage = e.Message + " or without parameters";
                    return;
                }
                catch (Exception e)
                {
                    ErrorMessage = e.Message;
                    return;
                }

                if (!MethodHasParameters && attribute.useHookOnly)
                {
                    ErrorMessage = $"HookAttribute: New inputs are not applied, because you set 'useHookOnly', " +
                            $"but your method on '{attribute.methodPath}' did not define the parameters {propertyType} oldValue, {propertyType} newValue";
                    return;
                }

                Func<bool> ifExecute = attribute.target switch
                {
                    ExecutionTarget.Always => () => true,
                    ExecutionTarget.IsPlaying => () => Application.isPlaying,
                    ExecutionTarget.IsNotPlaying => () => !Application.isPlaying,
                    _ => throw new NotImplementedException(attribute.target.ToString()),
                };


                if (MethodHasParameters)
                {
                    hookMethod = (p, o, n) =>
                    {
                        if (ifExecute())
                            p.GetMethodOnOwner(attribute.methodPath, new Type[] { propertyType, propertyType }).Invoke(o, n);
                    };
                }
                else
                {
                    hookMethod = (p, o, n) =>
                    {
                        if (ifExecute())
                            p.GetMethodOnOwner(attribute.methodPath).Invoke();
                    };
                }
            }
        }
    }
}
