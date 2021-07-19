using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Frameworks
{
	class EditorArray
	{

		public delegate T ElemOnGUICallback<T>(string title, T t);

		static Dictionary<string, SavedBool> ExtenderStates = new Dictionary<string, SavedBool>();

		static SavedBool GetSavedBool(string name, bool bDefault = false)
		{
			SavedBool b = null;
			if (!ExtenderStates.TryGetValue(name, out b))
			{
				b = new SavedBool(name, bDefault);
				ExtenderStates.Add(name, b);
			}

			return b;
		}


		public static void OnGUIList<T>( ref List<T> arr, ElemOnGUICallback<T> callback)
		{
			int size = arr == null ? 0 : arr.Count;

			EditorGUI.BeginChangeCheck();
			int newSize = EditorGUILayout.IntField("size", size);
			if (EditorGUI.EndChangeCheck())
			{
				if (newSize < 0)
					newSize = 0;

				if (arr == null && newSize != 0)
				{
					arr = new List<T>();
				}

				if (newSize > size)
				{
					if (size != 0)
					{
						var newArr = new List<T>(newSize);
						for (int i = 0; i < newSize; ++i)
						{
							int index = i >= arr.Count ? arr.Count - 1 : i;
							newArr.Add(arr[index]);
						}
						arr = newArr;
					}
					else
					{
						for (int i = 0; i < newSize; ++i)
						{
							arr.Add( default(T) );
						}
					}
				}
				else if (newSize < size)
				{
					arr.RemoveRange(newSize, size - newSize);
				}
			}

			if (callback != null)
			{
				for (int i = 0; i < arr.Count; ++i)
				{
					string title = string.Format("\tElement{0}", i);
					arr[i] = callback(title, arr[i]);
				}
			}
		}

		public static void OnGUIList( System.Collections.IList arr, Type type, string name)
		{
			EditorGUILayout.LabelField(name);
			int size = arr == null ? 0 : arr.Count;
			EditorGUI.BeginChangeCheck();
			int newSize = EditorGUILayout.IntField( string.Concat( "\t", name , "size"), size);
			if (EditorGUI.EndChangeCheck())
			{
				if (newSize < 0)
					newSize = 0;

				if (arr == null && newSize != 0)
				{
					arr = (System.Collections.IList)Activator.CreateInstance(type);
				}

				if (newSize > size)
				{
					if (size != 0)
					{
						//var newArr = Activator.CreateInstance(type);
						for (int i = 0; i < newSize - size; ++i)
						{
							//int index = i >= arr.Count ? arr.Count - 1 : i;
							ReflectTools.InvokeMethod(arr, "Add", arr[size-1]);
						}
						//arr = (System.Collections.IList)newArr;
					}
					else
					{
						Type[] genericArgTypes = type.GetGenericArguments();

						if (genericArgTypes.Length > 0)
						{
							for (int i = 0; i < newSize; ++i)
							{
								arr.Add( Activator.CreateInstance(genericArgTypes[0]) );
							}
						}

					}
				}
				else if (newSize < size)
				{
					ReflectTools.InvokeMethod(arr, "RemoveRange", newSize, size - newSize);
				}
			}

			var isExtend = GetSavedBool(type.FullName + "_" + name);

			isExtend.value = EditorGUILayout.Foldout( isExtend.value, name);
			if (isExtend.value)
			{
				for (int i = 0; arr != null && i < arr.Count; ++i)
				{
					string title = string.Concat("\t", name, "Element", i);
					EditorGUIExt.OnFieldGUILayout( arr[0], title);
				}
			}

		}

		public static void OnGUIReorderList<T>( ref List<T> arr, int drawLines, ReorderableList.ElementCallbackDelegate drawFunc) where T : new()
		{
			ReorderableList reorderableList = new ReorderableList( arr, typeof(T), true, true, true, true);

			reorderableList.elementHeight *= drawLines;

			reorderableList.onAddCallback = (l) => l.list.Add( new T() );
			reorderableList.drawElementCallback = drawFunc;

			reorderableList.DoLayoutList();
		}
	}
}
