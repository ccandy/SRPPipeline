using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Frameworks
{
	public class ShakenInstance : MonoBehaviour
	{
		public int				AffectCameraIndex = 0;

		public AnimationCurve	MagnitudeCurve	= new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
		public AnimationCurve	RoughnessCurve	= new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

		public ShakenParams		Params			= new ShakenParams();

		public float			TotalTime		= 1.0f;

		public float			DelayTime		= 0.1f;

		float passTime							= 0.0f;
		ShakenParams curParams					= new ShakenParams();

		void OnEnable()
		{
			OnInit();
		}

		void OnDisable()
		{
			ShakenCamera.UnregShakenInstance(AffectCameraIndex, this);
		}

		void OnInit()
		{
			ShakenCamera.RegShakenInstance(AffectCameraIndex, this);
			passTime = 0.0f;
		}

		public ShakenParams OnUpdateInstance(float deltaTime)
		{
			float normalPassTime = 1.0f;
			
			passTime += deltaTime;

			if (passTime < DelayTime)
			{
				curParams.Magnitude = 0;
				curParams.Roughness = 0;
				return curParams;
			}

			if (TotalTime > 0.0f)
			{
				normalPassTime = (passTime - DelayTime)/ TotalTime;
			}

			curParams.Magnitude = Params.Magnitude * MagnitudeCurve.Evaluate(normalPassTime);
			curParams.Roughness = Params.Roughness * RoughnessCurve.Evaluate(normalPassTime);

			return curParams;
		}

		public bool IsOverLife()
		{
			return passTime >= TotalTime;
		}
	}
}