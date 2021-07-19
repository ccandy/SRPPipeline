using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Frameworks
{
	public class EditorDictionary
	{
		public delegate void ElemOnGUICallback<K, V>(string title, K key, V val);

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


		public class AddElementWizard : ScriptableWizard
		{
			public System.Object bindTable = null;

			public Type KeyType;

			public System.Object newKey;

			public Type ValType;

			public System.Object newVal;

			//public AddElementWizard()
			//{

			//}

			public void OnWizardUpdate()
			{
				
			}

			protected override bool DrawWizardGUI()
			{
				EditorGUIExt.OnFieldGUILayout(ref newKey, KeyType, "new key");

				if (newKey == null)
				{
					newKey = Activator.CreateInstance(KeyType);
				}

				if (newVal == null)
				{
					newVal = Activator.CreateInstance(ValType);
				}

				EditorGUIExt.OnFieldGUILayout(ref newVal, ValType, "new Val");

				return base.DrawWizardGUI();
			}

			public void OnWizardCreate()
			{
				if (bindTable == null || newKey == null || newVal == null)
					return;

				if (!(bool)ReflectTools.InvokeMethod(bindTable, "ContainsKey", newKey) )
				{
					ReflectTools.InvokeMethod(bindTable, "Add", newKey, newVal);
				}
			}
		}

		public static void OnGUIDictionary<K, V>(ref Dictionary<K, V> table/*, ElemOnGUICallback<K, V> callback*/)
		{
			int size = table == null ? 0 : table.Count;

			bool isClickDel = false;
			K delKey = default(K);

			string extendName = string.Concat(typeof(K).FullName, "_", typeof(V).FullName);

			//if (callback != null)
			//{
			var isExtend = GetSavedBool(extendName);

			isExtend.value = EditorGUILayout.Foldout(isExtend.value, extendName);
			if (isExtend.value)
			{

				int i = 0;
				foreach (var p in table)
				{
					string title = string.Format("\tElement{0}", i);
					//callback(title, p.Key, p.Value);

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(title);
					if (GUILayout.Button("-"))
					{
						isClickDel = true;
						delKey = p.Key;
					}
					EditorGUILayout.EndHorizontal();
					EditorGUIExt.OnFieldGUILayout(p.Key, "\tkey");
					EditorGUIExt.OnFieldGUILayout(p.Value, "\tvalue");

					++i;
				}
			}

			if (isClickDel)
			{
				table.Remove(delKey);
			}
			//}
			if (GUILayout.Button("+"))
			{
				AddElementWizard wizard = ScriptableWizard.DisplayWizard<AddElementWizard>("Add");

				//AddElementWizard<K, V> wizard = new AddElementWizard<K, V>();
				wizard.bindTable = table;
				wizard.KeyType = typeof(K);
				wizard.newKey = default(K);
				wizard.ValType = typeof(V);
				wizard.newVal = default(V);
				wizard.OnWizardUpdate();
				//wizard.Show();

			}
		}

		// TO DO : Tuple?? 
		public static void OnGUIDictionaryReorderList<K, V>(ref Dictionary<K, V> table, int drawLines, ReorderableList.AddCallbackDelegate addCallback, ReorderableList.ElementCallbackDelegate drawFunc)
		{
			int size = table == null ? 0 : table.Count;

			List<Tuple<K, V>> tempList = new List<Tuple<K, V>>();

			foreach( var p in table)
			{
				tempList.Add(new Tuple<K, V>(p.Key, p.Value));
			}

			ReorderableList reorderableList = new ReorderableList(tempList, typeof(Tuple<K, V>), true, true, true, true);

			reorderableList.elementHeight *= drawLines;

			reorderableList.onAddCallback = addCallback;
			reorderableList.drawElementCallback = drawFunc;

			EditorGUI.BeginChangeCheck();
			int newSize = EditorGUILayout.IntField("size", size);
			reorderableList.DoLayoutList();
			if (EditorGUI.EndChangeCheck())
			{
				if (newSize < 0)
					newSize = 0;

				if (table == null && newSize != 0)
				{
					table = new Dictionary<K, V>();
				}

				table.Clear();

				for (int i = 0; i < tempList.Count; ++i)
				{
					var elem = tempList[i];
					table.Add(elem.Item1, elem.Item2);
				}
			}
		}
	
	
	}
}