using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Frameworks.CRP
{
	[System.Serializable]
	public struct SHValue
	{
		// 球谐数据
		public Vector4[] SHParams;

		public SHValue(float brightness)
		{
			SHParams = new Vector4[9];
			for (int i = 0; i < 9; i++)
			{
				SHParams[i] = new Vector4(brightness, brightness, brightness, brightness);
			}
		}

		//public SHValue(SphericalHarmonicsL2 daySH, SphericalHarmonicsL2 nightSH)
		//{
		//	var sh1 = new Vector4(nightSH[0, 0], nightSH[1, 0], nightSH[2, 0], daySH[0, 0] * 0.30f + daySH[1, 0] * 0.59f + daySH[2, 0] * 0.11f);
		//	var sh2 = new Vector4(nightSH[0, 1], nightSH[1, 1], nightSH[2, 1], daySH[0, 1] * 0.30f + daySH[1, 1] * 0.59f + daySH[2, 1] * 0.11f);
		//	var sh3 = new Vector4(nightSH[0, 2], nightSH[1, 2], nightSH[2, 2], daySH[0, 2] * 0.30f + daySH[1, 2] * 0.59f + daySH[2, 2] * 0.11f);
		//	var sh4 = new Vector4(nightSH[0, 3], nightSH[1, 3], nightSH[2, 3], daySH[0, 3] * 0.30f + daySH[1, 3] * 0.59f + daySH[2, 3] * 0.11f);
		//	var sh5 = new Vector4(nightSH[0, 4], nightSH[1, 4], nightSH[2, 4], daySH[0, 4] * 0.30f + daySH[1, 4] * 0.59f + daySH[2, 4] * 0.11f);
		//	var sh6 = new Vector4(nightSH[0, 5], nightSH[1, 5], nightSH[2, 5], daySH[0, 5] * 0.30f + daySH[1, 5] * 0.59f + daySH[2, 5] * 0.11f);
		//	var sh7 = new Vector4(nightSH[0, 6], nightSH[1, 6], nightSH[2, 6], daySH[0, 6] * 0.30f + daySH[1, 6] * 0.59f + daySH[2, 6] * 0.11f);
		//	var sh8 = new Vector4(nightSH[0, 7], nightSH[1, 7], nightSH[2, 7], daySH[0, 7] * 0.30f + daySH[1, 7] * 0.59f + daySH[2, 7] * 0.11f);
		//	var sh9 = new Vector4(nightSH[0, 8], nightSH[1, 8], nightSH[2, 8], daySH[0, 8] * 0.30f + daySH[1, 8] * 0.59f + daySH[2, 8] * 0.11f);

		//	var shAr = new Vector4(sh4.x, sh2.x, sh3.x, sh1.x - sh7.x);
		//	var shAg = new Vector4(sh4.y, sh2.y, sh3.y, sh1.y - sh7.y);
		//	var shAb = new Vector4(sh4.z, sh2.z, sh3.z, sh1.z - sh7.z);
		//	var shAa = new Vector4(sh4.w, sh2.w, sh3.w, sh1.w - sh7.w);
		//	var shBr = new Vector4(sh5.x, sh6.x, 3.0f * sh7.x, sh8.x);
		//	var shBg = new Vector4(sh5.y, sh6.y, 3.0f * sh7.y, sh8.y);
		//	var shBb = new Vector4(sh5.z, sh6.z, 3.0f * sh7.z, sh8.z);
		//	var shBa = new Vector4(sh5.w, sh6.w, 3.0f * sh7.w, sh8.w);
		//	var shC = sh9;

		//	SHParams = new Vector4[9]
		//	{
		//			shAr, shAg, shAb, shAa,
		//			shBr, shBg, shBb, shBa,
		//			shC
		//	};
		//}

		public SHValue(SHValue val1, SHValue val2, SHValue val3, Vector3 weights)
		{
			SHParams = new Vector4[9];
			for (int i = 0; i < 9; i++)
			{
				SHParams[i] = val1.SHParams[i] * weights.x + val2.SHParams[i] * weights.y + val3.SHParams[i] * weights.z;
			}
		}
	}

	public class LightSHBuilder
	{
		public static readonly float[] SHCoefficients =
			{
				0.28209479177387814347403972578039f,    // L0 M0          1 / (2*Sqrt(Pi))
				0.48860251190291992158638462283835f,    // L1 M-1   Sqrt(3) / (2*Sqrt(Pi))
				0.48860251190291992158638462283835f,    // L1 M0    Sqrt(3) / (2*Sqrt(Pi))
				0.48860251190291992158638462283835f,    // L1 M1    Sqrt(3) / (2*Sqrt(Pi))
				1.0925484305920790705433857058027f,     // L2 M-2  Sqrt(15) / (2*Sqrt(Pi))
				1.0925484305920790705433857058027f,     // L2 M-1  Sqrt(15) / (2*Sqrt(Pi))
				0.31539156525252000603089369029571f,    // L2 M0   Sqrtf(5) / (4*sqrt(Pi))
				1.0925484305920790705433857058027f,     // L2 M1   Sqrt(15) / (2*Sqrt(Pi))
				0.54627421529603953527169285290135f,    // L2 M2   Sqrt(15) / (4*Sqrt(Pi))
			};

		static Vector3 MapXYSToDir(float posX, float posY, float width, float height, int faceID, ref float weight)
		{
			var dir = Vector3.zero;
			var u = (posX + 0.5f) * 2.0f / width - 1.0f;
			var v = (posY + 0.5f) * 2.0f / height - 1.0f;
			switch (faceID)
			{
				case 0:
					dir = new Vector3(1.0f, -v, -u);
					break;
				case 1:
					dir = new Vector3(-1.0f, -v, u);
					break;
				case 2:
					dir = new Vector3(u, 1.0f, v);
					break;
				case 3:
					dir = new Vector3(u, -1.0f, -v);
					break;
				case 4:
					dir = new Vector3(u, -v, 1.0f);
					break;
				case 5:
					dir = new Vector3(-u, -v, -1.0f);
					break;
			}
			var mag = dir.magnitude;
			dir = dir / mag;
			weight = 4.0f / (mag * mag * mag);
			return dir;
		}

		static Vector3 MapXYSToDir(float posX, float posY, float width, float height, ref float weight)
		{
			var dir = Vector3.zero;

			float u = (posX + 0.5f) / width;
			float v = (posY + 0.5f) / height;

			float horAngle = (u - 0.5f) * Mathf.PI * 2.0f;
			float vecAngle = (0.5f - v) * Mathf.PI;

			dir = Quaternion.Euler(vecAngle * Mathf.Rad2Deg, horAngle * Mathf.Rad2Deg, 0.0f) * Vector3.forward;

			var mag = dir.magnitude;
			//var mag = Mathf.Sqrt( 1.0f / ( Mathf.Sin(vecAngle) * Mathf.Sin(vecAngle) + 1.0f / (Mathf.Sin(horAngle) * Mathf.Sin(horAngle)) ) );//dir.magnitude;

			dir /= mag;

			weight = 4.0f / (mag * mag * mag);

			//weight = 1.0f / (mag * mag * mag);

			return dir;
		}

		static Vector3[] DirToSH9Col(Vector3 dir, Color col, float weight)
		{
			var sh = new float[9];
			var dirX = dir.x;
			var dirY = dir.y;
			var dirZ = dir.z;
			sh[0] = SHCoefficients[0];
			sh[1] = SHCoefficients[1] * dirY;
			sh[2] = SHCoefficients[2] * dirZ;
			sh[3] = SHCoefficients[3] * dirX;
			sh[4] = SHCoefficients[4] * dirX * dirY;
			sh[5] = SHCoefficients[5] * dirY * dirZ;
			sh[6] = SHCoefficients[6] * (3.0f * dirZ * dirZ - 1.0f);
			sh[7] = SHCoefficients[7] * dirX * dirZ;
			sh[8] = SHCoefficients[8] * (dirX * dirX - dirY * dirY);

			var colVec = new Vector3(col.r, col.g, col.b);
			Vector3[] shCol = new Vector3[9];
			for (int i = 0; i < 9; ++i)
			{
				shCol[i] = colVec * sh[i] * weight;
			}

			return shCol;
		}

		public static Vector3 GetStandardParams(float[] values, int index)
		{
			if (index < 0 || index > 8) return Vector3.zero;
			return new Vector3(values[index * 3], values[index * 3 + 1], values[index * 3 + 2]);
		}

		static void SetStandardParams(float[] values, Vector3 value, int index)
		{
			if (index < 0 || index > 8) return;
			values[index * 3] = value.x;
			values[index * 3 + 1] = value.y;
			values[index * 3 + 2] = value.z;
		}

		// 增量SH标准参
		static void AddStandardParam(float[] values, Vector3 value, int index)
		{
			if (index < 0 || index > 8)
				return;
			values[index * 3] += value.x;
			values[index * 3 + 1] += value.y;
			values[index * 3 + 2] += value.z;
		}

		static void ScaleValues(float[] values, float scale)
		{
			for (int i = 0; i < 27; i++)
			{
				values[i] *= scale;
			}
		}

		public static Color GammaToLinearSpace(Color sRGB)
		{
			Color rt = new Color();
			// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
			rt.r = sRGB.r * (sRGB.r * (sRGB.r * 0.305306011f + 0.682171111f) + 0.012522878f);
			rt.g = sRGB.g * (sRGB.g * (sRGB.g * 0.305306011f + 0.682171111f) + 0.012522878f);
			rt.b = sRGB.b * (sRGB.b * (sRGB.b * 0.305306011f + 0.682171111f) + 0.012522878f);
			return rt;
		}


		public static float[] CalculateCube(Cubemap cube, bool hdr2ldr, float blur = 1.0f)
		{
			// 保护
			if (cube == null)
				return null;

			var values = new float[27];

#if UNITY_EDITOR
			var assetReadable = false;
			var importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(cube)) as UnityEditor.TextureImporter;
			if (importer != null)
			{
				assetReadable = importer.isReadable;
				if (!assetReadable) importer.isReadable = true;
				importer.SaveAndReimport();
			}
#endif

			// 分析Mipmap尺寸
			var mipMax = cube.mipmapCount - 2;
			var mip = Mathf.FloorToInt(Mathf.Lerp(0, mipMax, blur));
			var width = (int)Mathf.Pow(2.0f, 1.0f + (mipMax - mip));
			var height = width;
			Debug.Log(string.Concat("mip:", mip));
			Debug.Log(string.Concat("resolution:", height));
			// 采样球谐参数
			var weightSum = 0.0f;
			for (int face = 0; face < 6; ++face)
			{
				var cols = cube.GetPixels((CubemapFace)face, mip);
				for (int y = 0; y < height; ++y)
				{
					for (int x = 0; x < width; ++x)
					{
						// 计算方向和权重
						var weight = 0.0f;
						var dir = MapXYSToDir(x, y, width, height, face, ref weight);
						weightSum += weight;
						// 计算球谐参数
						var index = y * height + x;
						var col = cols[index];
						// 对HDR2LDR编码支持
						if (hdr2ldr)
						{
							col = new Color(col.r * col.a * 5.0f, col.g * col.a * 5.0f, col.b * col.a * 5.0f, 0.0f);
						}
						else
						{
							//col = GammaToLinearSpace(col);
						}

						var dsh = DirToSH9Col(dir, col, weight);
						for (int i = 0; i < 9; i++)
						{
							AddStandardParam(values, dsh[i], i);
						}
					}
				}
			}
			ScaleValues(values, (4.0f * 3.14159f) / weightSum);
			for (int i = 0; i < values.Length / 3; ++i)
			{
				values[3 * i] *= SHCoefficients[i];
				values[3 * i + 1] *= SHCoefficients[i];
				values[3 * i + 2] *= SHCoefficients[i];
			}

#if UNITY_EDITOR
			if (importer != null)
			{
				if (!assetReadable) importer.isReadable = false;
				importer.SaveAndReimport();
			}
#endif

			return values;
		}

		public static Vector4[] GetUnityParams(float[] values)
		{
			var shParams = new Vector4[7];

			shParams[0] = new Vector4(values[9], values[3], values[6], values[0] - values[18]);
			shParams[1] = new Vector4(values[10], values[4], values[7], values[1] - values[19]);
			shParams[2] = new Vector4(values[11], values[5], values[8], values[2] - values[20]);
			shParams[3] = new Vector4(values[12], values[15], 3.0f * values[18], values[21]);
			shParams[4] = new Vector4(values[13], values[16], 3.0f * values[19], values[22]);
			shParams[5] = new Vector4(values[14], values[17], 3.0f * values[20], values[23]);
			shParams[6] = new Vector4(values[24], values[25], values[26], 1.0f);

			return shParams;
		}

		public static void BuildSH(ref SHValue SH9Value, Cubemap cube, bool hdr2ldr, float blur = 1.0f)
		{
			var float27Array = CalculateCube(cube, hdr2ldr, blur);

			SH9Value.SHParams = GetUnityParams(float27Array);
		}
	}
}
