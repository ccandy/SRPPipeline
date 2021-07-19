using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks
{
	public class ImageStreamCombinEditor : EditorWindowBase<ImageStreamCombinEditor>
	{
		EditorFilePath RedFilePath	 = new EditorFilePath();
		EditorFilePath GreenFilePath = new EditorFilePath();
		EditorFilePath BlueFilePath	 = new EditorFilePath();
		EditorFilePath AlphaFilePath = new EditorFilePath();

		EditorFilePath ImageFilePath = new EditorFilePath();

		Texture2D RedTex;
		Texture2D GreenTex;
		Texture2D BlueTex;
		Texture2D AlphaTex;

		Vector4 _RTex_DotMask = new Vector4(1.0f,0.0f,0.0f,0.0f);
		Vector4 _GTex_DotMask = new Vector4(0.0f,1.0f,0.0f,0.0f);
		Vector4 _BTex_DotMask = new Vector4(0.0f,0.0f,1.0f,0.0f);
		Vector4 _ATex_DotMask = new Vector4(0.0f,0.0f,0.0f,1.0f);

		Vector2Int Resolution;

		bool IsRLinearToGamma = false;
		bool IsGLinearToGamma = false;
		bool IsBLinearToGamma = false;
		bool IsALinearToGamma = false;


		Texture2D ImageTex;

		bool isSplitStream = false;

		[MenuItem("Tools/ImageStreamCombinEditor", false, 10)]
		static void OpenEditor()
		{
			Open("贴图通道合并工具", new Vector2(600, 500));
		}

		void OnEnable()
		{
			
		}

		void OnCombineUI()
		{
			Resolution = EditorGUILayout.Vector2IntField( "分辨率:", Resolution);
			if (Resolution.x == 0)
				Resolution.x = 1;

			if (Resolution.y == 0 )
				Resolution.y = 1;

			RedTex				= (Texture2D)EditorGUILayout.ObjectField("R:", RedTex,		typeof(Texture2D), true);
			_RTex_DotMask		= EditorGUILayout.Vector4Field("_RTex_DotMask", _RTex_DotMask);
			IsRLinearToGamma	= EditorGUILayout.Toggle("是否将通道R进行线性转Gamma:", IsRLinearToGamma);

			GreenTex			= (Texture2D)EditorGUILayout.ObjectField("G:", GreenTex,	typeof(Texture2D), true);
			_GTex_DotMask		= EditorGUILayout.Vector4Field("_GTex_DotMask", _GTex_DotMask);
			IsGLinearToGamma	= EditorGUILayout.Toggle("是否将通道G进行线性转Gamma:", IsGLinearToGamma);

			BlueTex				= (Texture2D)EditorGUILayout.ObjectField("B:", BlueTex,		typeof(Texture2D), true);
			_BTex_DotMask		= EditorGUILayout.Vector4Field("_BTex_DotMask", _BTex_DotMask);
			IsBLinearToGamma	= EditorGUILayout.Toggle("是否将通道B进行线性转Gamma:", IsBLinearToGamma);

			AlphaTex			= (Texture2D)EditorGUILayout.ObjectField("A:", AlphaTex,	typeof(Texture2D), true);
			_ATex_DotMask		= EditorGUILayout.Vector4Field("_ATex_DotMask", _ATex_DotMask);
			IsALinearToGamma	= EditorGUILayout.Toggle("是否将通道A进行线性转Gamma:", IsALinearToGamma);

			ImageFilePath.OnGUI("输出贴图路径:", "png", "...", ImageFilePath.absotionPath, true, "image");

			if (GUILayout.Button("合并"))
			{
				DoCombine();
			}
		}

		void DoCombine()
		{
			EditorGPUPass pass = new EditorGPUPass();

			pass.DrawingMesh = EditorGPUPass.CreateScreenAlignedQuadMesh();
			pass.bindMat = new Material(Shader.Find("Hidden/CombineTexture"));

			pass.bindMat.SetTexture("_RTex", RedTex);
			pass.bindMat.SetTexture("_GTex", GreenTex);
			pass.bindMat.SetTexture("_BTex", BlueTex);
			pass.bindMat.SetTexture("_ATex", AlphaTex);

			pass.bindMat.SetVector("_RTex_DotMask", _RTex_DotMask);
			pass.bindMat.SetVector("_GTex_DotMask", _GTex_DotMask);
			pass.bindMat.SetVector("_BTex_DotMask", _BTex_DotMask);
			pass.bindMat.SetVector("_ATex_DotMask", _ATex_DotMask);


			RenderTexture renderTexture = RenderTexture.GetTemporary(Resolution.x, Resolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

			pass.RenderingOnce(renderTexture, (texture) => 
			{
				Texture2D tex2D = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
				tex2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
				tex2D.Apply();

				System.IO.File.WriteAllBytes( ImageFilePath.absotionPath, tex2D.EncodeToPNG());
			});

			renderTexture.Release();

			AssetDatabase.Refresh();
		}

		void OnSplitUI()
		{
			if (RedFilePath.OnGUI("R通道:", "png", "...", RedFilePath.absotionPath, true, "red"))
			{

			}

			if (GreenFilePath.OnGUI("G通道:", "png", "...", GreenFilePath.absotionPath, true, "green"))
			{

			}

			if (BlueFilePath.OnGUI("B通道:", "png", "...", BlueFilePath.absotionPath, true, "blue"))
			{

			}

			if (AlphaFilePath.OnGUI("A通道:", "png", "...", AlphaFilePath.absotionPath, true, "alpha"))
			{

			}
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();

			isSplitStream = EditorGUILayout.Toggle("是否拆分贴图通道:", isSplitStream);

			if (isSplitStream)
			{
				OnSplitUI();
			}
			else
			{
				OnCombineUI();
			}
			
			EditorGUILayout.EndVertical();
		}
	}
}
