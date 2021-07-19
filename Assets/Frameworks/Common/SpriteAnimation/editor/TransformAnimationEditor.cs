// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// 
// namespace Frameworks.Common
// {
// 	[CanEditMultipleObjects]
// 	[CustomEditor(typeof(TransformAnimation))]
// 	public class TransformAnimationEditor : Editor
// 	{
// 		SerializedProperty startPositionProperty;
// 		SerializedProperty startRotationProperty;
// 		SerializedProperty startScaleProperty;
// 
// 		SerializedProperty endPositionProperty;
// 		SerializedProperty endRotationProperty;
// 		SerializedProperty endScaleProperty;
// 
// 		public override void OnInspectorGUI()
// 		{
// 			base.OnInspectorGUI();
// 
// 			serializedObject.Update();
// 
// 			startPositionProperty = serializedObject.FindProperty("startPosition");
// 			startRotationProperty = serializedObject.FindProperty("startRotation");
// 			startScaleProperty	  = serializedObject.FindProperty("startScale");
// 
// 			endPositionProperty = serializedObject.FindProperty("endPosition");
// 			endRotationProperty = serializedObject.FindProperty("endRotation");
// 			endScaleProperty	= serializedObject.FindProperty("endScale");
// 
// 			if (startPositionProperty == null)
// 				return;
// 			if ( startRotationProperty == null)
// 				return;
// 			if ( startScaleProperty	   == null)
// 				return;
// 			if ( endPositionProperty == null)
// 				return;
// 			if ( endRotationProperty == null)
// 				return;
// 			if ( endScaleProperty	 == null )
// 				return;
// 
// 
// 			EditorGUILayout.PropertyField(startPositionProperty);
// 
// 			Vector3 startEulerAngle = EditorGUILayout.Vector3Field("startRotation", startRotationProperty.quaternionValue.eulerAngles);
// 
// 			startRotationProperty.quaternionValue = Quaternion.Euler(startEulerAngle);
// 
// 			EditorGUILayout.PropertyField(startScaleProperty);
// 
// 
// 
// 			EditorGUILayout.PropertyField(endPositionProperty);
// 
// 			Vector3 endEulerAngle = EditorGUILayout.Vector3Field("endRotation", endRotationProperty.quaternionValue.eulerAngles);
// 
// 			endRotationProperty.quaternionValue = Quaternion.Euler(endEulerAngle);
// 
// 			EditorGUILayout.PropertyField(endScaleProperty);
// 
// 
// 			serializedObject.ApplyModifiedProperties();
// 
// 
// 		}
// 	}
// }
