using System;
using System.Collections.Generic;
using UnityEngine;

public enum CurveAnimType
{
	Once,
	Loop,
	Pingpong,
}

public class CurveAdapt : MonoBehaviour
{
	public float BeginValue = 0.0f;
	public float EndValue = 1.0f;

	public float PassTime = 0.0f;
	public float TotalTime = 1.0f;

	public CurveAnimType AnimType = CurveAnimType.Once;

	public bool IsPlayByTime = true;

	public bool IsUseUnScaleTime = false;

	int Reback = 1;

	public AnimationCurve Curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

	void OnEnable()
	{
		PassTime = 0.0f;
	}

	void OnDisable()
	{
		PassTime = 0.0f;
		DoUpdate(BeginValue);
	}

	public void UpdateData(float deltaTime)
	{
		PassTime += Reback * deltaTime;	

		DoUpdate(Mathf.Lerp(BeginValue, EndValue, Curve.Evaluate(PassTime / TotalTime)));

		if (PassTime >= TotalTime)
		{
			if (AnimType == CurveAnimType.Loop)
			{
				PassTime -= TotalTime;
				Reback = 1;
			}
			else if (AnimType == CurveAnimType.Pingpong)
			{
				PassTime = TotalTime - (PassTime - TotalTime);
				Reback = -1;
			}
		}
		else if (PassTime < 0.0f)
		{
			PassTime = -1.0f * PassTime;
			Reback = 1;
		}
	}

	void Update()
	{
		if (!IsPlayByTime)
		{
			return;
		}

		UpdateData(IsUseUnScaleTime ? Time.unscaledDeltaTime : Time.deltaTime);
	}

	public virtual void DoUpdate(float CurValue)
	{

	}
}

