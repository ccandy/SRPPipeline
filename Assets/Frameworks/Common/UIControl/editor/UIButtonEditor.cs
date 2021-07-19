using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks.UIControl
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIControl))]
	class UIButtonEditor : Editor
	{
		SerializedProperty mOnClickProperty;

		protected virtual void OnEnable()
		{
			mOnClickProperty = serializedObject.FindProperty("mOnClick");
		}

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.PropertyField(mOnClickProperty);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
