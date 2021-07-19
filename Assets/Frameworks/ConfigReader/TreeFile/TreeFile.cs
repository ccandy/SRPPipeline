using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Tree File Template:
/// Bool
/// Int
/// Float
/// String
/// 
/// Vector2
/// Vector3
/// Vector4
/// Vector2Int
/// Vector3Int
/// Curve
/// 
/// Array<V>
/// Map<K,V>
/// 
/// AssetRef<GameObject> 
/// 
/// eg:
/// 
/// Tree TData
/// {
///		bool						bData,
///		int							iData,
///		float						fData,
///		string						strData,
///		AssetRef<GameObject>		pathData,
///		Vector2						v2Data,
///		Vector3						v3Data,
///		Vector4						v4Data,
///		Vector2Int					v2iData,
///		Vector3Int					v3iData,
///		Curve						curveData,
/// 
///		Array<Vector3>				arrayData,
///		Map<String,TData>			mapData,
/// }
/// 
/// </summary>

public partial class TreeData
{
	public virtual void LoadFromJson(JToken jObject)
	{

	}

	public virtual JObject SaveToJson()
	{
		return new JObject();
	}

	public virtual string ClassType()
	{
		return "TreeData";
	}

	public virtual void LoadAssetRef()
	{

	}

	public virtual void LoadAssetRefAsync()
	{

	}

	public virtual void DisposeAssetRefs()
	{

	}
	/// <summary>
	/// 
	/// </summary>
	/// <returns>x: cur loaded, y total asset count.</returns>
	//public virtual Vector2Int GetAssetLoadedProcess()
	//{
	//	return Vector2Int.zero;
	//}

	//public virtual int GetAssetRefCount()
	//{
	//	return 0;
	//}

#if UNITY_EDITOR
	public virtual void OnGUI()
	{
		//UnityEditor.EditorGUILayout.BeginVertical();
		//UnityEditor.EditorGUILayout.EndVertical();
	}
#endif
}
