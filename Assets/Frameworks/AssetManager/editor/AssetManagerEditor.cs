using UnityEditor;
using UnityEngine;
using System.IO;
using Frameworks;

namespace Frameworks.Asset
{
	[CustomEditor(typeof(AssetManager))]
	public class AssetManagerEditor : Editor
	{
		bool IsShowResource = false;

		bool IsShowAllResource = false;

		bool IsShowDirectResource = false;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();

			AssetManager mgr = target as AssetManager;

			if (mgr == null)
				return;

			IsShowResource = EditorGUILayout.Foldout(IsShowResource, "资源列表");

			if (IsShowResource)
			{
				Pool<BundleData> m_GlobeBundleTable		= ReflectTools.GetPrivateField<Pool<BundleData>>(mgr, "m_GlobeBundleTable");
				Pool<BundleData> m_DirectlyBundleTable	= ReflectTools.GetPrivateField<Pool<BundleData>>(mgr, "m_DirectlyBundleTable");


				IsShowAllResource = EditorGUILayout.Foldout(IsShowAllResource, string.Format("全局资源列表({0}个)", m_GlobeBundleTable.lookForTable.Count));

				if (IsShowAllResource)
				{
					var totalPool = m_GlobeBundleTable.pool;

					for (int i = 0; i < totalPool.Count; ++i)
					{
						var bundleData = totalPool[i];
						if (bundleData == null)
							continue;

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(string.Format("    Url: {0}", bundleData.BundleKey));
						EditorGUILayout.EndHorizontal();
					}
				}

				IsShowDirectResource = EditorGUILayout.Foldout(IsShowDirectResource, string.Format("直接资源列表({0}个)", m_DirectlyBundleTable.lookForTable.Count));

				if (IsShowDirectResource)
				{
					var totalPool = m_DirectlyBundleTable.pool;

					for (int i = 0; i < totalPool.Count; ++i)
					{
						var bundleData = totalPool[i];
						if (bundleData == null)
							continue;

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(string.Format("    Url: {0}", bundleData.BundleKey));
						EditorGUILayout.EndHorizontal();

					}
				}
			}
		}
	}
}
