using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MulNodeTrail))]
public class MulNodeTrailEditor : Editor
{
	GUIStyle m_Style = new GUIStyle();

	static Color NameColor = Color.white;

	Vector3 m_TempPos = Vector3.zero;

	MulNodeTrail m_Trail;
	void OnEnable()
	{
		m_Trail = target as MulNodeTrail;
	}


}
