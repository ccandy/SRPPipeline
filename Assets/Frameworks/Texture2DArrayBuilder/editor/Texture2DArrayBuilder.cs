using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Frameworks.Common
{
	//public class Texture2DArrayBuilder : EditorWindowBase<Texture2DArrayBuilder>
	//{
	//	static List<Texture2D> textureInArray = new List<Texture2D>();

	//	ReorderableList m_TextureReorderableList;
	//	List<Texture2D> BindReorderableTextureList = null;

	//	EditorFilePath ArraySavePath  = new EditorFilePath();

	//	bool			IsGPUConvert  = false;

	//	bool			IsSRGBConvert = false;
	//	bool			IsMipmap	  = false;

	//	static bool		IsInited		= false;

	//	private Vector2 scrollView = Vector2.zero;

	//	[MenuItem("Assets/UnityResource/Texture2DArray", false, 10)]
	//	static void GeneratorTexture2DArray()
	//	{
	//		var objects = Selection.objects;

	//		textureInArray.Clear();

	//		for (int i = 0; i < objects.Length; ++i)
	//		{
	//			//string path = AssetDatabase.GetAssetPath(objects[i]);
	//			if (objects[i] is Texture2D)
	//			{
	//				textureInArray.Add((Texture2D)objects[i]);
	//			}
	//		}

	//		IsInited = false;

	//		Open("Texture2DArrayBuilder", new Vector2(800, 800));

	//		if (Instance != null)
	//		{
	//			Instance.Init();
	//		}
	//	}

	//	void BuildTextureReorderableList(List<Texture2D> curTextures)
	//	{
	//		BindReorderableTextureList = curTextures;
	//		m_TextureReorderableList = new ReorderableList(BindReorderableTextureList, typeof(Texture2D), true, true, true, true);

	//		m_TextureReorderableList.drawHeaderCallback = (Rect rect) =>
	//		{
	//			EditorGUI.LabelField(rect, "Textures");
	//		};

	//		m_TextureReorderableList.elementHeight *= 2;

	//		m_TextureReorderableList.onAddCallback		 = AddTexture;
	//		m_TextureReorderableList.drawElementCallback = DrawTextureArrayElement;
	//	}

	//	void AddTexture(ReorderableList list)
	//	{
	//		BindReorderableTextureList.Add(null);
	//	}

	//	void DrawTextureArrayElement(Rect rect, int index, bool selected, bool focused)
	//	{
	//		if (BindReorderableTextureList == null)
	//			return;

	//		BindReorderableTextureList[index] = (Texture2D)EditorGUI.ObjectField(rect, "texture", BindReorderableTextureList[index], typeof(Texture2D), false);
	//	}

	//	void Init()
	//	{
	//		if (IsInited)
	//			return;

	//		IsInited = true;

	//		BuildTextureReorderableList(textureInArray);

	//		if (BindReorderableTextureList.Count < 1)
	//			return;

	//		string textureAssetPath = AssetDatabase.GetAssetPath(BindReorderableTextureList[0]);

	//		if (string.IsNullOrEmpty(textureAssetPath))
	//			return;

	//		var importer = UnityEditor.AssetImporter.GetAtPath(textureAssetPath) as UnityEditor.TextureImporter;
	//		if (importer == null)
	//			return;

	//		IsSRGBConvert = importer.sRGBTexture;
	//		IsGPUConvert = IsSRGBConvert;
	//		IsMipmap = importer.mipmapEnabled;

	//		AutoSelectMipmap();
	//	}

	//	void AutoSelectMipmap()
	//	{
	//		if (BindReorderableTextureList.Count < 1)
	//			return;

	//		string textureAssetPath = AssetDatabase.GetAssetPath(BindReorderableTextureList[0]);

	//		if (string.IsNullOrEmpty(textureAssetPath))
	//			return;

	//		var importer = UnityEditor.AssetImporter.GetAtPath(textureAssetPath) as UnityEditor.TextureImporter;
	//		if (importer == null)
	//			return;

	//		IsMipmap = importer.mipmapEnabled;
	//	}

	//	void OnEnable()
	//	{
	//		Init();
	//	}

	//	void OnGUI()
	//	{
	//		ArraySavePath.OnGUI("存储路径:", "asset", "选择存储路径", ArraySavePath.absotionPath, true);

	//		IsGPUConvert	= EditorGUILayout.Toggle("是否使用GPU转换", IsGPUConvert);

	//		IsSRGBConvert &= IsGPUConvert;

	//		EditorGUI.BeginChangeCheck();
	//		IsSRGBConvert	= EditorGUILayout.Toggle("是否SRGB格式", IsSRGBConvert);
	//		if (EditorGUI.EndChangeCheck())
	//		{
	//			IsGPUConvert |= IsSRGBConvert;
	//			AutoSelectMipmap();
	//		}

	//		if (IsGPUConvert)
	//		{
	//			IsMipmap	= EditorGUILayout.Toggle("是否生成Mipmap", IsMipmap);
	//		}

	//		if (GUILayout.Button("打包Array"))
	//		{
	//			Create();
	//		}

	//		scrollView = EditorGUILayout.BeginScrollView(scrollView);

	//		//int newSelectIndex = -1;
	//		if (m_TextureReorderableList != null)
	//		{
	//			m_TextureReorderableList.DoLayoutList();

	//			//newSelectIndex = m_TextureReorderableList.index;
	//		}
			
	//		EditorGUILayout.EndScrollView();
	//	}

	//	bool IsCheckCanPackageArray()
	//	{
	//		if (BindReorderableTextureList == null || BindReorderableTextureList.Count == 0)
	//			return false;

	//		if (BindReorderableTextureList[0] == null)
	//			return false;

	//		int width  = BindReorderableTextureList[0].width;
	//		int height = BindReorderableTextureList[0].height;

	//		bool isCanPackage = true;

	//		for (int i = 1; i < BindReorderableTextureList.Count; ++i)
	//		{
	//			var texture = BindReorderableTextureList[i];

	//			if (texture == null)
	//				return false;

	//			if (texture.width != width
	//				|| texture.height != height)
	//			{
	//				Debug.LogErrorFormat("texture index({0}): resolution is not equal the first texture", i);
	//				isCanPackage &= false;
	//			}
	//		}

	//		if (!ArraySavePath.isAssetPath)
	//		{
	//			Debug.LogErrorFormat("ArraySavePath({0}) is not asset path.", ArraySavePath);
	//			isCanPackage &= false;
	//		}

	//		return isCanPackage;
	//	}

	//	void Create()
	//	{
	//		if (!IsCheckCanPackageArray())
	//			return;

	//		Texture2DArray texture2DArray = null;

	//		if (IsGPUConvert)
	//		{
	//			texture2DArray = new Texture2DArray(BindReorderableTextureList[0].width, BindReorderableTextureList[0].height, BindReorderableTextureList.Count, TextureFormat.ARGB32, IsMipmap);
	//		}
	//		else
	//		{
	//			texture2DArray = new Texture2DArray(BindReorderableTextureList[0].width, BindReorderableTextureList[0].height, BindReorderableTextureList.Count, BindReorderableTextureList[0].format, IsMipmap);
	//		}

	//		RenderTexture tempRenderTexture = null;
	//		EditorGPUPass SettingAlbedoPass = new EditorGPUPass();
	//		SettingAlbedoPass.CreateMaterialFromShaderPath("Hidden/sRGBCopy");
	//		Texture2D tempTex2D = null;

	//		if (IsGPUConvert)
	//		{
	//			/*tempRenderTexture = RenderTexture.GetTemporary(BindReorderableTextureList[0].width, BindReorderableTextureList[0].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 0, RenderTextureMemoryless.None, VRTextureUsage.None, false);*/
	//			tempRenderTexture = RenderTexture.GetTemporary(BindReorderableTextureList[0].width, BindReorderableTextureList[0].height, 0, RenderTextureFormat.ARGB32);
	//			tempRenderTexture.useMipMap = IsMipmap;
	//			tempTex2D = new Texture2D(BindReorderableTextureList[0].width, BindReorderableTextureList[0].height, TextureFormat.ARGB32, IsMipmap);
	//		}

	//		if (IsGPUConvert)
	//		{
	//			for (int i = 0; i < BindReorderableTextureList.Count; ++i)
	//			{
	//				// 直接copytexture 线性空间转换的处理没有处理到 只能用Pass暴力一波处理
	//				SettingAlbedoPass.bindMat.SetTexture("_MainTex", BindReorderableTextureList[i]);
	//				SettingAlbedoPass.bindMat.SetFloat("_ConvertGamma", IsSRGBConvert ? 0.0f : 1.0f);
	//				SettingAlbedoPass.Rendering(tempRenderTexture);
	//				tempTex2D.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
	//				tempTex2D.Apply();
	//				Graphics.CopyTexture(tempTex2D, 0, texture2DArray, i);
	//			}
	//			Graphics.SetRenderTarget(null);
	//		}
	//		else
	//		{
	//			for (int i = 0; i < BindReorderableTextureList.Count; ++i)
	//			{
	//				Graphics.CopyTexture(BindReorderableTextureList[i], 0, texture2DArray, i);
	//			}
	//		}

	//		AssetDatabase.CreateAsset( texture2DArray, ArraySavePath.assetPath);
	//		AssetDatabase.Refresh();

	//		if (tempRenderTexture != null)
	//			tempRenderTexture.Release();

	//		if (tempTex2D != null)
	//			DestroyImmediate(tempTex2D);
	//	}
	//}

	//public class NormalRoughnessAOBuilder : EditorWindowBase<NormalRoughnessAOBuilder>
	//{
	//	class NoramlMaskGroup
	//	{
	//		public Texture2D normal;
	//		public Texture2D roughness;
	//		public Texture2D ao;
	//		public string genSingleFileName;
	//	}

	//	bool IsMipmap = false;

	//	EditorFolderPath SaveFolderPath = new EditorFolderPath();

	//	EditorFilePath SaveArrayPath = new EditorFilePath();

	//	bool IsSaveAsArray;

	//	Vector2 scrollView = Vector2.zero;

	//	static bool IsInited = false;

	//	static List<NoramlMaskGroup> textureInArray = new List<NoramlMaskGroup>();

	//	ReorderableList m_TextureReorderableList;

	//	readonly static char[] sDirectorySplits = new char[] { '/', '\\' };

	//	[MenuItem("Assets/UnityResource/NormalRoughnessAOArray", false, 10)]
	//	static void GeneratorTexture2DArray()
	//	{
	//		var objects = Selection.objects;

	//		textureInArray.Clear();

	//		for (int i = 0; i < objects.Length; ++i)
	//		{
	//			string path = AssetDatabase.GetAssetPath(objects[i]);
	//			var importer = AssetImporter.GetAtPath(path) as TextureImporter;
	//			if (importer == null)
	//				continue;

	//			if (importer.textureType != TextureImporterType.NormalMap)
	//				continue;

	//			NoramlMaskGroup group = new NoramlMaskGroup();
	//			group.normal = objects[i] as Texture2D;

	//			int extHeadIndex = path.LastIndexOf(".");

	//			string fileHeadPart = path.Substring(0, extHeadIndex);

	//			int lastUnderLineIndex = fileHeadPart.LastIndexOf('_');
	//			if (lastUnderLineIndex != -1)
	//			{
	//				int lastDirectorySplit = fileHeadPart.LastIndexOfAny(sDirectorySplits);

	//				if (lastDirectorySplit < lastUnderLineIndex)
	//				{
	//					fileHeadPart = fileHeadPart.Substring(0, lastUnderLineIndex);
	//				}
	//			}

	//			string roughnessPath = string.Format("{0}_roughness{1}", fileHeadPart, path.Substring(extHeadIndex, path.Length - extHeadIndex));
	//			string aoPath		 = string.Format( "{0}_ao{1}", fileHeadPart,  path.Substring(extHeadIndex, path.Length - extHeadIndex));

	//			group.genSingleFileName = string.Format("{0}_NormalRoughnessAO{1}", System.IO.Path.GetFileName(fileHeadPart), path.Substring(extHeadIndex, path.Length - extHeadIndex));

	//			if ( System.IO.File.Exists(EditorFilePath.AssetPathToAbsotionPath(roughnessPath)) )
	//			{
	//				group.roughness = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);
	//			}

	//			if (System.IO.File.Exists(EditorFilePath.AssetPathToAbsotionPath(aoPath)))
	//			{
	//				group.ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
	//			}

	//			textureInArray.Add(group);
	//		}

	//		IsInited = false;

	//		Open("Texture2DArrayBuilder", new Vector2(800, 800));

	//		if (Instance != null)
	//		{
	//			Instance.Init();
	//		}
	//	}

	//	void Init()
	//	{
	//		if (IsInited)
	//			return;

	//		IsInited = true;

	//		BuildTextureReorderableList();

	//		if (textureInArray.Count < 1)
	//			return;

	//		AutoSelectMipmap();
	//	}

	//	void AutoSelectMipmap()
	//	{
	//		if (textureInArray.Count < 1)
	//			return;

	//		string textureAssetPath = AssetDatabase.GetAssetPath(textureInArray[0].normal);

	//		if (string.IsNullOrEmpty(textureAssetPath))
	//			return;

	//		var importer = UnityEditor.AssetImporter.GetAtPath(textureAssetPath) as UnityEditor.TextureImporter;
	//		if (importer == null)
	//			return;

	//		IsMipmap = importer.mipmapEnabled;
	//	}

	//	void BuildTextureReorderableList()
	//	{
	//		m_TextureReorderableList = new ReorderableList(textureInArray, typeof(NoramlMaskGroup), true, true, true, true);

	//		m_TextureReorderableList.drawHeaderCallback = (Rect rect) =>
	//		{
	//			EditorGUI.LabelField(rect, "Textures");
	//		};

	//		m_TextureReorderableList.elementHeight *= 6;

	//		m_TextureReorderableList.onAddCallback			= AddTexture;
	//		m_TextureReorderableList.drawElementCallback	= DrawTextureArrayElement;
	//	}

	//	void AddTexture(ReorderableList list)
	//	{
	//		textureInArray.Add(new NoramlMaskGroup());
	//	}

	//	void DrawTextureArrayElement(Rect rect, int index, bool selected, bool focused)
	//	{
	//		Vector2 offset = new Vector2(0,rect.height / 3.0f);
	//		Rect curRt   = new Rect(rect.min.x, rect.min.y, rect.width, offset.y);
			
	//		textureInArray[index].normal = (Texture2D)EditorGUI.ObjectField(curRt, "normal", textureInArray[index].normal, typeof(Texture2D), false);

	//		curRt.position += offset;
	//		textureInArray[index].roughness = (Texture2D)EditorGUI.ObjectField(curRt, "roughness", textureInArray[index].roughness, typeof(Texture2D), false);

	//		curRt.position += offset;
	//		textureInArray[index].ao = (Texture2D)EditorGUI.ObjectField(curRt, "ao", textureInArray[index].ao, typeof(Texture2D), false);
	//	}

	//	void OnEnable()
	//	{
	//		Init();
	//	}

	//	void OnGUI()
	//	{

	//		IsSaveAsArray	= EditorGUILayout.Toggle("是否生成TextureArray对象", IsSaveAsArray);
	//		IsMipmap		= EditorGUILayout.Toggle("是否生成Mipmap", IsMipmap);

	//		if (IsSaveAsArray)
	//			SaveArrayPath.OnGUI("存储路径:", "asset", "选择存储路径", SaveArrayPath.absotionPath, true);
	//		else
	//			SaveFolderPath.OnGUI("存储路径:");

	//		if (IsSaveAsArray ? GUILayout.Button("打包Array") : GUILayout.Button("批次生成(RG:法线_B:粗糙度_A:AO)贴图"))
	//		{
	//			if (IsSaveAsArray)
	//				CreateArray();
	//			else
	//				CreateTextures();
	//		}

	//		scrollView = EditorGUILayout.BeginScrollView(scrollView);
	//		//int newSelectIndex = -1;
	//		if (m_TextureReorderableList != null)
	//		{
	//			m_TextureReorderableList.DoLayoutList();

	//			//newSelectIndex = m_TextureReorderableList.index;
	//		}

			

	//		EditorGUILayout.EndScrollView();
	//	}

	//	bool IsCheckCanPackageArray()
	//	{
	//		if (textureInArray == null || textureInArray.Count == 0)
	//			return false;

	//		if (textureInArray[0] == null)
	//			return false;

	//		int width = textureInArray[0].normal.width;
	//		int height = textureInArray[0].normal.height;

	//		bool isCanPackage = true;

	//		for (int i = 1; i < textureInArray.Count; ++i)
	//		{
	//			var group = textureInArray[i];

	//			if (group == null)
	//				return false;

	//			if (group.normal.width != width
	//				|| group.normal.height != height)
	//			{
	//				Debug.LogErrorFormat("texture index({0}): resolution is not equal the first texture", i);
	//				isCanPackage &= false;
	//			}
	//		}

	//		if (!SaveArrayPath.isAssetPath)
	//		{
	//			Debug.LogErrorFormat("ArraySavePath({0}) is not asset path.", SaveArrayPath);
	//			isCanPackage &= false;
	//		}

	//		return isCanPackage;
	//	}

	//	void CreateArray()
	//	{
	//		if (!IsCheckCanPackageArray())
	//			return;

	//		Texture2DArray texture2DArray = null;

	//		texture2DArray = new Texture2DArray(textureInArray[0].normal.width, textureInArray[0].normal.height, textureInArray.Count, TextureFormat.ARGB32, IsMipmap);

	//		Texture2D tempTex2D = null;

	//		for (int i = 0; i < textureInArray.Count; ++i)
	//		{
	//			var group = textureInArray[i];
	//			TextureConvertLogic.ConvertUnityNormalToNormalRG(group.normal, group.roughness, group.ao, IsMipmap, ref tempTex2D);

	//			Graphics.CopyTexture(tempTex2D, 0, texture2DArray, i);
	//		}

	//		AssetDatabase.CreateAsset(texture2DArray, SaveArrayPath.assetPath);
	//		AssetDatabase.Refresh();

	//		if (tempTex2D != null)
	//			DestroyImmediate(tempTex2D);
	//	}

	//	void CreateTextures()
	//	{
	//		Texture2D tempTex2D = null;

	//		for (int i = 0; i < textureInArray.Count; ++i)
	//		{
	//			var group = textureInArray[i];
	//			if (string.IsNullOrEmpty(group.genSingleFileName))
	//			{
	//				continue;
	//			}

	//			TextureConvertLogic.ConvertUnityNormalToNormalRG(group.normal, group.roughness, group.ao, IsMipmap, ref tempTex2D);

	//			string savePath = string.Format("{0}/{1}", SaveFolderPath.absotionPath, group.genSingleFileName);

	//			savePath = savePath.Substring(0, savePath.LastIndexOf('.')) + ".tga";
	//			System.IO.File.WriteAllBytes(savePath, tempTex2D.EncodeToTGA());
	//		}

	//		AssetDatabase.Refresh();
	//	}
	//}

	public class NormalAOBuilder : EditorWindowBase<NormalAOBuilder>
	{
		class NoramlMaskGroup
		{
			public Texture2D normal;
			//public Texture2D roughness;
			public Texture2D ao;
			public string genSingleFileName;
		}

		bool IsMipmap = false;

		EditorFolderPath SaveFolderPath = new EditorFolderPath();

		EditorFilePath SaveArrayPath = new EditorFilePath();

		Vector2 scrollView = Vector2.zero;

		static bool IsInited = false;

		static List<NoramlMaskGroup> textureInArray = new List<NoramlMaskGroup>();

		ReorderableList m_TextureReorderableList;

		readonly static char[] sDirectorySplits = new char[] { '/', '\\' };

		[MenuItem("Assets/UnityResource/NormalAOPathcher", false, 10)]
		static void GeneratorTexture2DArray()
		{
			var objects = Selection.objects;

			textureInArray.Clear();

			for (int i = 0; i < objects.Length; ++i)
			{
				string path = AssetDatabase.GetAssetPath(objects[i]);
				var importer = AssetImporter.GetAtPath(path) as TextureImporter;
				if (importer == null)
					continue;

				if (importer.textureType != TextureImporterType.NormalMap)
					continue;

				NoramlMaskGroup group = new NoramlMaskGroup();
				group.normal = objects[i] as Texture2D;

				int extHeadIndex = path.LastIndexOf(".");

				string fileHeadPart = path.Substring(0, extHeadIndex);

				int lastUnderLineIndex = fileHeadPart.LastIndexOf('_');
				if (lastUnderLineIndex != -1)
				{
					int lastDirectorySplit = fileHeadPart.LastIndexOfAny(sDirectorySplits);

					if (lastDirectorySplit < lastUnderLineIndex)
					{
						fileHeadPart = fileHeadPart.Substring(0, lastUnderLineIndex);
					}
				}

				string aoPath = string.Format("{0}_ao{1}", fileHeadPart, path.Substring(extHeadIndex, path.Length - extHeadIndex));

				group.genSingleFileName = string.Format("{0}_NormalAO{1}", System.IO.Path.GetFileName(fileHeadPart), path.Substring(extHeadIndex, path.Length - extHeadIndex));

				if (System.IO.File.Exists(EditorFilePath.AssetPathToAbsotionPath(aoPath)))
				{
					group.ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
				}

				textureInArray.Add(group);
			}

			IsInited = false;

			Open("Texture2DArrayBuilder", new Vector2(800, 800));

			if (Instance != null)
			{
				Instance.Init();
			}
		}

		void Init()
		{
			if (IsInited)
				return;

			IsInited = true;

			BuildTextureReorderableList();

			if (textureInArray.Count < 1)
				return;

			AutoSelectMipmap();
		}

		void AutoSelectMipmap()
		{
			if (textureInArray.Count < 1)
				return;

			string textureAssetPath = AssetDatabase.GetAssetPath(textureInArray[0].normal);

			if (string.IsNullOrEmpty(textureAssetPath))
				return;

			var importer = UnityEditor.AssetImporter.GetAtPath(textureAssetPath) as UnityEditor.TextureImporter;
			if (importer == null)
				return;

			IsMipmap = importer.mipmapEnabled;
		}

		void BuildTextureReorderableList()
		{
			m_TextureReorderableList = new ReorderableList(textureInArray, typeof(NoramlMaskGroup), true, true, true, true);

			m_TextureReorderableList.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField(rect, "Textures");
			};

			m_TextureReorderableList.elementHeight *= 6;

			m_TextureReorderableList.onAddCallback = AddTexture;
			m_TextureReorderableList.drawElementCallback = DrawTextureArrayElement;
		}

		void AddTexture(ReorderableList list)
		{
			textureInArray.Add(new NoramlMaskGroup());
		}

		void DrawTextureArrayElement(Rect rect, int index, bool selected, bool focused)
		{
			Vector2 offset = new Vector2(0, rect.height / 3.0f);
			Rect curRt = new Rect(rect.min.x, rect.min.y, rect.width, offset.y);

			textureInArray[index].normal = (Texture2D)EditorGUI.ObjectField(curRt, "normal", textureInArray[index].normal, typeof(Texture2D), false);

			curRt.position += offset;
			textureInArray[index].ao = (Texture2D)EditorGUI.ObjectField(curRt, "ao", textureInArray[index].ao, typeof(Texture2D), false);
		}

		void OnEnable()
		{
			Init();
		}

		void OnGUI()
		{
			IsMipmap = EditorGUILayout.Toggle("是否生成Mipmap", IsMipmap);

			SaveFolderPath.OnGUI("存储路径:");

			if (GUILayout.Button("批次生成(RG:法线_B:AO)贴图"))
			{
				CreateTextures();
			}

			scrollView = EditorGUILayout.BeginScrollView(scrollView);
			//int newSelectIndex = -1;
			if (m_TextureReorderableList != null)
			{
				m_TextureReorderableList.DoLayoutList();

				//newSelectIndex = m_TextureReorderableList.index;
			}



			EditorGUILayout.EndScrollView();
		}

		bool IsCheckCanPackageArray()
		{
			if (textureInArray == null || textureInArray.Count == 0)
				return false;

			if (textureInArray[0] == null)
				return false;

			int width = textureInArray[0].normal.width;
			int height = textureInArray[0].normal.height;

			bool isCanPackage = true;

			for (int i = 1; i < textureInArray.Count; ++i)
			{
				var group = textureInArray[i];

				if (group == null)
					return false;

				if (group.normal.width != width
					|| group.normal.height != height)
				{
					Debug.LogErrorFormat("texture index({0}): resolution is not equal the first texture", i);
					isCanPackage &= false;
				}
			}

			if (!SaveArrayPath.isAssetPath)
			{
				Debug.LogErrorFormat("ArraySavePath({0}) is not asset path.", SaveArrayPath);
				isCanPackage &= false;
			}

			return isCanPackage;
		}

		void CreateTextures()
		{
			Texture2D tempTex2D = null;

			for (int i = 0; i < textureInArray.Count; ++i)
			{
				var group = textureInArray[i];
				if (string.IsNullOrEmpty(group.genSingleFileName))
				{
					string path = AssetDatabase.GetAssetPath(group.normal);
					if (string.IsNullOrEmpty(path))
					{
						group.genSingleFileName = "normal" + i;
					}
					else
					{
						group.genSingleFileName = System.IO.Path.GetFileName(path);
					}
				}

				TextureConvertLogic.ConvertUnityNormalToNormalRG(group.normal, group.ao, Texture2D.whiteTexture, IsMipmap, ref tempTex2D);

				string savePath = string.Format("{0}/{1}", SaveFolderPath.absotionPath, group.genSingleFileName);

				savePath = savePath.Substring(0, savePath.LastIndexOf('.')) + ".png";
				System.IO.File.WriteAllBytes(savePath, tempTex2D.EncodeToPNG());
			}

			AssetDatabase.Refresh();
		}
	}
}
