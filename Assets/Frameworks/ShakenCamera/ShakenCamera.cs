using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Frameworks
{
	[Serializable]
	public class ShakenParams
	{
		/// <summary>
		/// The default position influence of all shakes created by this shaker.
		/// </summary>
		public Vector3 PosInfluence = Vector3.one * 0.15f;

		/// <summary>
		/// The default rotation influence of all shakes created by this shaker.
		/// </summary>
		public Vector3 RotInfluence = Vector3.one;

		/// <summary>
		/// The intensity of the shake. It is recommended that you use ScaleMagnitude to alter the magnitude of a shake.
		/// </summary>
		public float Magnitude = 1;

		/// <summary>
		/// Roughness of the shake. It is recommended that you use ScaleRoughness to alter the roughness of a shake.
		/// </summary>
		public float Roughness = 1;

		public void ZeroParams()
		{
			PosInfluence	= Vector3.zero;
			RotInfluence	= Vector3.zero;
			Magnitude		= 0;
			Roughness		= 0;
		}

		public void BlendParams(ShakenParams param)
		{
			if (PosInfluence.x < param.PosInfluence.x)
				PosInfluence.x	= param.PosInfluence.x;

			if (PosInfluence.y < param.PosInfluence.y)
				PosInfluence.y	= param.PosInfluence.y;

			if (PosInfluence.z < param.PosInfluence.z)
				PosInfluence.z	= param.PosInfluence.z;
			
			if (Magnitude < param.Magnitude)
				Magnitude		= param.Magnitude;

			if (Roughness < param.Roughness)
				Roughness		= param.Roughness;
		}
	}

	public class ShakenCamera : MonoBehaviour
	{
		ShakenParams m_ShakenParams = new ShakenParams();

		float roughMod = 1, magnMod = 1;
		bool sustain;
		float tick = 0;
		Vector3 amt;

		public int CameraIndex = 0;

		List<ShakenInstance> shakenInstances = new List<ShakenInstance>();

		static Dictionary<int, ShakenCamera> ms_ShakenCameraTable = new Dictionary<int, ShakenCamera>();

		public static void RegShakenInstance(int Index, ShakenInstance instance)
		{
			if (instance == null)
			{
				Debug.LogErrorFormat("RegShakenInstance to Index:({0}) error! instance == null.", Index);
				return;
			}

			ShakenCamera camera = null;
			if (!ms_ShakenCameraTable.TryGetValue(Index, out camera))
			{
				Debug.LogWarningFormat("RegShakenInstance to Index:({0}) error! camera not found.", Index);
			}

			if (camera.shakenInstances.Contains(instance))
				return;

			camera.shakenInstances.Add(instance);
		}

		public static void UnregShakenInstance(int Index, ShakenInstance instance)
		{
			if (instance == null)
			{
				Debug.LogErrorFormat("UnregShakenInstance to Index:({0}) error! instance == null.", Index);
				return;
			}

			ShakenCamera camera = null;
			if (!ms_ShakenCameraTable.TryGetValue(Index, out camera))
			{
				Debug.LogWarningFormat("UnregShakenInstance to Index:({0}) error! camera not found.", Index);
				return;
			}

			camera.shakenInstances.Remove(instance);
		}

		public static Vector3 MultiplyVectors(Vector3 v, Vector3 w)
		{
			v.x *= w.x;
			v.y *= w.y;
			v.z *= w.z;

			return v;
		}

		void Awake()
		{
			if (ms_ShakenCameraTable.ContainsKey(CameraIndex))
			{
				Debug.LogWarningFormat("ShakenCamera Index {0} is existed!", CameraIndex);

				while (ms_ShakenCameraTable.ContainsKey(CameraIndex))
				{
					++CameraIndex;
				}

				Debug.LogWarningFormat("ShakenCamera Index reset to {0}!", CameraIndex);
			}

			ms_ShakenCameraTable.Add(CameraIndex, this);
		}

		public Vector3 UpdateShake()
		{
			amt.x = Mathf.PerlinNoise(tick, 0) - 0.5f;
			amt.y = Mathf.PerlinNoise(0, tick) - 0.5f;
			amt.z = Mathf.PerlinNoise(tick, tick) - 0.5f;

			tick += Time.deltaTime * m_ShakenParams.Roughness * roughMod;

			return amt * m_ShakenParams.Magnitude * magnMod;
		}

		void Update()
		{
			if (shakenInstances.Count == 0)
				return;

			m_ShakenParams.ZeroParams();

			float deltaTime = Time.deltaTime;

			for (int i = 0; i < shakenInstances.Count; ++i)
			{
				var param = shakenInstances[i].OnUpdateInstance(deltaTime);

				m_ShakenParams.BlendParams(param);

				if (shakenInstances[i].IsOverLife())
				{
					shakenInstances.RemoveAt(i);
					--i;
				}
			}

			if (shakenInstances.Count == 0)
			{
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
			else
			{
				transform.localPosition		= MultiplyVectors(UpdateShake(), m_ShakenParams.PosInfluence);
				transform.localEulerAngles	= MultiplyVectors(UpdateShake(), m_ShakenParams.RotInfluence);
			}
		}

		private void OnDestroy()
		{
			ms_ShakenCameraTable.Remove(CameraIndex);
		}
	}
}