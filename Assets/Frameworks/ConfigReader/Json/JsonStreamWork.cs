using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks
{
	public class JsonStreamWork
	{
		public static Vector2 JsonToVector2(JToken json)
		{
			Vector2 v = Vector2.zero;

			if (json == null)
				return v;

			v.x = json["X"]?.ToObject<float>() ?? 0.0f;
			v.y = json["Y"]?.ToObject<float>() ?? 0.0f;

			return v;
		}

		public static Vector3 JsonToVector3(JToken json)
		{
			Vector3 v = Vector3.zero;

			if (json == null)
				return v;

			v.x = json["X"]?.ToObject<float>() ?? 0.0f;
			v.y = json["Y"]?.ToObject<float>() ?? 0.0f;
			v.z = json["Z"]?.ToObject<float>() ?? 0.0f;

			return v;
		}

		public static Vector4 JsonToVector4(JToken json)
		{
			Vector4 v = Vector4.zero;

			if (json == null)
				return v;

			v.x = json["X"]?.ToObject<float>() ?? 0.0f;
			v.y = json["Y"]?.ToObject<float>() ?? 0.0f;
			v.z = json["Z"]?.ToObject<float>() ?? 0.0f;
			v.w = json["W"]?.ToObject<float>() ?? 0.0f;

			return v;
		}

		public static Vector2Int JsonToVector2Int(JToken json)
		{
			Vector2Int v = Vector2Int.zero;

			if (json == null)
				return v;

			v.x = json["X"]?.ToObject<int>() ?? 0;
			v.y = json["Y"]?.ToObject<int>() ?? 0;

			return v;
		}

		public static Vector3Int JsonToVector3Int(JToken json)
		{
			Vector3Int v = Vector3Int.zero;

			if (json == null)
				return v;

			v.x = json["X"]?.ToObject<int>() ?? 0;
			v.y = json["Y"]?.ToObject<int>() ?? 0;
			v.z = json["Z"]?.ToObject<int>() ?? 0;

			return v;
		}

		public static AnimationCurve JsonToCurve(JToken json)
		{
			AnimationCurve curve = new AnimationCurve();

			if (json == null)
				return curve;

			var keyFrames = json["KeyFrames"];
			if (keyFrames != null)
			{
				foreach (var node in keyFrames)
				{
					Keyframe keyframe = new Keyframe();

					keyframe.time = node["time"]?.ToObject<float>() ?? 0.0f;
					keyframe.value = node["value"]?.ToObject<float>() ?? 0.0f;

					keyframe.inTangent = node["inTangent"]?.ToObject<float>() ?? 0.0f;
					keyframe.outTangent = node["outTangent"]?.ToObject<float>() ?? 0.0f;

					keyframe.tangentMode = node["tangentMode"]?.ToObject<int>() ?? 0;

					keyframe.inWeight = node["inWeight"]?.ToObject<float>() ?? 0.0f;
					keyframe.outWeight = node["outWeight"]?.ToObject<float>() ?? 0.0f;

					keyframe.weightedMode = (WeightedMode)(node["weightedMode"]?.ToObject<int>() ?? 0);

					curve.AddKey(keyframe);
				}
			}

			curve.preWrapMode = (WrapMode)(json["preWrapMode"]?.ToObject<int>() ?? 0);

			curve.postWrapMode = (WrapMode)(json["postWrapMode"]?.ToObject<int>() ?? 0);

			return curve;
		}

		public static JObject Vector2ToJson(Vector2 v)
		{
			JObject json = new JObject();

			json.Add("type", "Vector2");
			json.Add("X", v.x);
			json.Add("Y", v.y);

			return json;
		}

		public static JObject Vector3ToJson(Vector3 v)
		{
			JObject json = new JObject();

			json.Add("type", "Vector3");
			json.Add("X", v.x);
			json.Add("Y", v.y);
			json.Add("Z", v.z);

			return json;
		}

		public static JObject Vector4ToJson(Vector4 v)
		{
			JObject json = new JObject();

			json.Add("type", "Vector4");
			json.Add("X", v.x);
			json.Add("Y", v.y);
			json.Add("Z", v.z);
			json.Add("W", v.w);

			return json;
		}

		public static JObject Vector2IntToJson(Vector2Int v)
		{
			JObject json = new JObject();

			json.Add("type", "Vector2Int");
			json.Add("X", v.x);
			json.Add("Y", v.y);

			return json;
		}

		public static JObject Vector3IntToJson(Vector3Int v)
		{
			JObject json = new JObject();

			json.Add("type", "Vector3Int");
			json.Add("X", v.x);
			json.Add("Y", v.y);
			json.Add("Z", v.z);

			return json;
		}

		public static JObject CurveToJson(AnimationCurve curve)
		{
			JObject json = new JObject();

			json.Add("type", "AnimationCurve");

			if (curve == null)
				return json;

			JArray keyFrames = new JArray();
			for (int i = 0; i < curve.keys.Length; ++i)
			{
				Keyframe keyframe = curve.keys[i];

				JObject node = new JObject();

				node.Add("time", keyframe.time);
				node.Add("value", keyframe.value);

				node.Add("inTangent", keyframe.inTangent);
				node.Add("outTangent", keyframe.outTangent);

				node.Add("tangentMode", keyframe.tangentMode);

				node.Add("inWeight", keyframe.inWeight);
				node.Add("outWeight", keyframe.outWeight);

				node.Add("weightedMode", (int)keyframe.weightedMode);

				keyFrames.Add(node);
			}

			json.Add("KeyFrames", keyFrames);
			json.Add("preWrapMode", (int)curve.preWrapMode);
			json.Add("postWrapMode", (int)curve.postWrapMode);

			return json;
		}
	}
}
