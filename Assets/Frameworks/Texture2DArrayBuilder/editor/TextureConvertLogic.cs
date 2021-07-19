using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Frameworks
{
	public class TextureConvertLogic
	{
		public static void ConvertUnityNormalToNormalRG(Texture2D normal, Texture2D Roughness, Texture2D AO, bool isMipmap, ref Texture2D outputTexture)
		{
			EditorGPUPass convertPass = new EditorGPUPass();
			convertPass.CreateMaterialFromShaderPath("Hidden/NormalRoughnessAOConvert");
			convertPass.DrawingMesh = EditorGPUPass.CreateScreenAlignedQuadMesh();

			if (normal == null)
			{
				normal = Texture2D.normalTexture;
			}

			if (Roughness == null)
			{
				Roughness = Texture2D.whiteTexture;
			}

			if (AO == null)
			{
				AO = Texture2D.whiteTexture;
			}

			Vector2Int resolution = new Vector2Int(normal.width, normal.height);

			var tempRenderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32);

			tempRenderTexture.autoGenerateMips = isMipmap;

			if (outputTexture == null
				|| outputTexture.width != resolution.x
				|| outputTexture.height != resolution.y)
			{
				if (outputTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(outputTexture);
				}

				outputTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.ARGB32, isMipmap);
			}

			convertPass.bindMat.SetTexture("_NormalTex", normal);
			convertPass.bindMat.SetTexture("_RoughnessTex", Roughness);
			convertPass.bindMat.SetTexture("_AOTex", AO);

			convertPass.Rendering(tempRenderTexture);

			outputTexture.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
			outputTexture.Apply();

			Graphics.SetRenderTarget(null);

			tempRenderTexture.Release();

		}

		public static void ConvertUnityNormalToNormalRG(Texture2D normal, Texture2D AO, bool isMipmap, ref Texture2D outputTexture)
		{
			EditorGPUPass convertPass = new EditorGPUPass();
			convertPass.CreateMaterialFromShaderPath("Hidden/NormalAOConvert");
			convertPass.DrawingMesh = EditorGPUPass.CreateScreenAlignedQuadMesh();

			if (normal == null)
			{
				normal = Texture2D.normalTexture;
			}

			if (AO == null)
			{
				AO = Texture2D.whiteTexture;
			}

			Vector2Int resolution = new Vector2Int(normal.width, normal.height);

			var tempRenderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32);

			tempRenderTexture.autoGenerateMips = isMipmap;

			if (outputTexture == null
				|| outputTexture.width != resolution.x
				|| outputTexture.height != resolution.y)
			{
				if (outputTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(outputTexture);
				}

				outputTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.ARGB32, isMipmap);
			}

			convertPass.bindMat.SetTexture("_NormalTex", normal);
			convertPass.bindMat.SetTexture("_AOTex", AO);

			convertPass.Rendering(tempRenderTexture);

			outputTexture.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
			outputTexture.Apply();

			Graphics.SetRenderTarget(null);

			tempRenderTexture.Release();

		}


		public static void ConvertTextureArrayToTextures( Texture2DArray texture2DArray, ref List<Texture2D> textures, bool isMipmap)
		{
			if (texture2DArray == null)
				return;

			if (textures == null)
			{
				textures = new List<Texture2D>(texture2DArray.depth);
			}

			textures.Clear();
			Resources.UnloadUnusedAssets();

			for (int i = 0; i < texture2DArray.depth; ++i)
			{
				var texture = new Texture2D(texture2DArray.width, texture2DArray.height, texture2DArray.format, isMipmap);
				Graphics.CopyTexture(texture2DArray, i, texture, 0);
			}
		}

		public static void ConvertTexturesToTextureArray(ref Texture2DArray texture2DArray, List<Texture2D> textures, bool isMipmap)
		{
			if (textures == null)
			{
				return;
			}

			if (texture2DArray == null)
			{
			}
			
			textures.Clear();
			Resources.UnloadUnusedAssets();

			for (int i = 0; i < texture2DArray.depth; ++i)
			{
				var texture = new Texture2D(texture2DArray.width, texture2DArray.height, texture2DArray.format, isMipmap);
				Graphics.CopyTexture(texture2DArray, i, texture, 0);
			}
		}
	}
}
