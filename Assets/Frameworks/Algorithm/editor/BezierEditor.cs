using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Frameworks.Algorithm;

[CustomEditor(typeof(Bezier))]
public class BezierEditor : Editor
{
	GUIStyle m_Style = new GUIStyle();

	static Color NameColor = Color.white;

	public int Speractor = 360;

	Vector3 m_TempPos = Vector3.zero;

	Bezier m_Bezier;
	void OnEnable()
	{
		m_Bezier = target as Bezier;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		NameColor = EditorGUILayout.ColorField("NameColor", NameColor);

		Speractor = EditorGUILayout.IntField("贝塞尔曲线长度段数(段数越高精确率越高)", Speractor);

		if (GUILayout.Button("Cal Bezier Length"))
		{
			m_Bezier.LineLength = m_Bezier.CalLength(Speractor);
		}
	}

	public virtual void OnSceneGUI()
	{
		if (m_Bezier == null)
			return;

		Vector3 controlPosition = m_Bezier.ControlPosition0;

		if (OnSceneGUIPosition(ref controlPosition, "Control Position"))
		{
			m_Bezier.ControlPosition0 = controlPosition;
			EditorUtility.SetDirty(target);
		}

		if (m_Bezier.curveType == Bezier.CurveType.TwoControl)
		{
			controlPosition = m_Bezier.ControlPosition1;

			if (OnSceneGUIPosition(ref controlPosition, "Control Position1"))
			{
				m_Bezier.ControlPosition1 = controlPosition;
				EditorUtility.SetDirty(target);
			}
		}

		controlPosition = m_Bezier.Begin;

		if (OnSceneGUIPosition(ref controlPosition, "Begin"))
		{
			m_Bezier.Begin = controlPosition;
			EditorUtility.SetDirty(target);
		}

		controlPosition = m_Bezier.End;

		if (OnSceneGUIPosition(ref controlPosition, "End"))
		{
			m_Bezier.End = controlPosition;
			EditorUtility.SetDirty(target);
		}
	}

	public bool OnSceneGUIPosition(ref Vector3 position, string name)
	{
		m_Style.fontSize = 15;
		m_Style.fontStyle = FontStyle.Bold;
		m_Style.richText = true;

		string nameFormat = string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", (byte)(NameColor.r*255), (byte)(NameColor.g * 255), (byte)(NameColor.b * 255), (byte)(NameColor.a * 255), name);

		Handles.Label(position, nameFormat, m_Style);

		m_TempPos = Handles.PositionHandle(position, Quaternion.identity);

		if (Vector3.Distance(position, m_TempPos) > float.Epsilon)
		{
			position = m_TempPos;
			return true;
		}

		return false;
	}
}