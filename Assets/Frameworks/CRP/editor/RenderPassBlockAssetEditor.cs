using System;
using System.Collections.Generic;
using UnityEditor;
using Frameworks;
using UnityEngine;
using UnityEditorInternal;
using System.Text.RegularExpressions;

namespace Frameworks.CRP
{
	[CustomEditor(typeof(RenderPassBlockAsset), true)]
	public class RenderPassBlockAssetEditor : Editor
	{
		internal class Styles
		{
			public static GUIContent passHeaderText = EditorGUIUtility.TrTextContent("Custom Pass List", "Lists all the passes available to this Pass Block Asset.");
		}

		SerializedProperty m_RenderPassAssetsProp;
		ReorderableList m_RenderPassBlockAssetsList;

		void OnEnable()
		{
			m_RenderPassAssetsProp = serializedObject.FindProperty("CustomPassAssets");
			m_RenderPassBlockAssetsList = new ReorderableList(serializedObject, m_RenderPassAssetsProp, true, true, true, true);
			DrawPassListLayout(m_RenderPassBlockAssetsList, m_RenderPassAssetsProp);
		}

		string GetMenuNameFromType(Type type)
		{
			var path = type.Name;

			// Inserts blank space in between camel case strings
			return Regex.Replace(Regex.Replace(path, "([a-z])([A-Z])", "$1 $2", RegexOptions.Compiled),
				"([A-Z])([A-Z][a-z])", "$1 $2", RegexOptions.Compiled);
		}

		private void AddComponent(object type)
		{
			serializedObject.Update();

			ScriptableObject component = CreateInstance((string)type);
			component.name = $"New{(string)type}";
			Undo.RegisterCreatedObjectUndo(component, "Add Renderer Feature");

			// Store this new effect as a sub-asset so we can reference it safely afterwards
			// Only when we're not dealing with an instantiated asset
			if (EditorUtility.IsPersistent(target))
			{
				AssetDatabase.AddObjectToAsset(component, target);
			}
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier(component, out var guid, out long localId);

			// Grow the list first, then add - that's how serialized lists work in Unity
			m_RenderPassAssetsProp.arraySize++;
			SerializedProperty componentProp = m_RenderPassAssetsProp.GetArrayElementAtIndex(m_RenderPassAssetsProp.arraySize - 1);
			componentProp.objectReferenceValue = component;

			serializedObject.ApplyModifiedProperties();

			// Force save / refresh
			if (EditorUtility.IsPersistent(target))
			{
				EditorUtility.SetDirty(target);
			}
			serializedObject.ApplyModifiedProperties();
		}

		void DrawPassListLayout(ReorderableList list, SerializedProperty prop)
		{
			list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				rect.y += 2;
				Rect indexRect = new Rect(rect.x, rect.y, 14, EditorGUIUtility.singleLineHeight);
				EditorGUI.LabelField(indexRect, index.ToString());
				Rect objRect = new Rect(rect.x + indexRect.width, rect.y, rect.width - 134, EditorGUIUtility.singleLineHeight);

				EditorGUI.BeginChangeCheck();
				EditorGUI.ObjectField(objRect, prop.GetArrayElementAtIndex(index), GUIContent.none);
				if (EditorGUI.EndChangeCheck())
					EditorUtility.SetDirty(target);

				Rect selectRect = new Rect(rect.x + rect.width - 24, rect.y, 24, EditorGUIUtility.singleLineHeight);

				CustomRenderPipelineAsset asset = target as CustomRenderPipelineAsset;

				// If object selector chose an object, assign it to the correct ScriptableRendererData slot.
				if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == index)
				{
					prop.GetArrayElementAtIndex(index).objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
				}
			};

			list.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField(rect, Styles.passHeaderText);
			};

			list.onAddCallback = li =>
			{
				GenericMenu menu = new GenericMenu();
				TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<RenderPassAsset>();
				foreach (Type type in types)
				{
					string path = GetMenuNameFromType(type);
					menu.AddItem(new GUIContent(path), false, AddComponent, type.Name);
				}
				menu.ShowAsContext();	
			};

			list.onCanRemoveCallback = li => { return li.count > 0; };

			list.onRemoveCallback = li =>
			{
				// set to null
				var prop = list.serializedProperty.GetArrayElementAtIndex(li.index);
				DestroyImmediate(prop.objectReferenceValue, true);
				list.serializedProperty.DeleteArrayElementAtIndex(li.index);
				list.serializedProperty.DeleteArrayElementAtIndex(li.index);
				EditorUtility.SetDirty(target);
			};

			list.onReorderCallbackWithDetails += (reorderableList, index, newIndex) =>
			{

			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			m_RenderPassBlockAssetsList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}
