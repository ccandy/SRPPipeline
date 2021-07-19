using Frameworks.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Asset
{
	public class BundleData : OrderLessObj
	{
		public enum State
		{
			Unload,
			Loading,
			Loaded,
			Unloading,
		}

		public int GetRef
		{
			get
			{
				return RefCount;
			}
		}

		int RefCount = 0;

		public State CurState = State.Unload;

		/// <summary>
		/// Using for GC all useless bundle.
		/// </summary>
		public bool IsUseless = false;

		/// <summary>
		/// bundle Path, begin with asset/...
		/// </summary>
		public string BundleKey = "";

		public string AbsPath	 = "";

		//public int ReadingCount  = 0;

		public int LoadingPercent = 0;

		public AssetBundle Bundle
		{
			get
			{
				return m_Bundle;
			}
		}

		public bool IsCanClearNow()
		{
			return m_Bundle == null
				|| RefCount <= 0
				|| CanClearTime == -1
				|| TimeTools.TimeGetTime() - CanClearTime > 0;
		}

		public bool IsCanRead
		{
			get
			{
				if (CurState < State.Loaded)
					return false;

				for (int i = 0; i < Dependencies.Count; ++i)
				{
					if (Dependencies[i].CurState < State.Loaded)
						return false;
				}

				return true;
			}
		}

		public bool IsLoading
		{
			get
			{
				if (CurState == State.Loading)
					return true;

				for (int i = 0; i < Dependencies.Count; ++i)
				{
					if (Dependencies[i].CurState == State.Loading)
						return true;
				}

				return false;
			}
		}

		public void OnFinishLoading(AssetBundle bundle)
		{
			m_Bundle = bundle;
			IsUseless = false;
			ResetCanClearTime();
			CurState = State.Loaded;
		}

		public void AddRef()
		{
			++RefCount;

			for (int i = 0; i < Dependencies.Count; ++i)
			{
				Dependencies[i].AddRef();
			}
		}

		public void ReduceRef()
		{
			--RefCount;

			if (RefCount < 0)
			{
				Log.Print(LogLevel.Error, "{0} Reduce Ref Error! RefCount == {1}", BundleKey, RefCount);
			}

			for (int i = 0; i < Dependencies.Count; ++i)
			{
				Dependencies[i].ReduceRef();
			}
		}

		void ResetCanClearTime()
		{
			CanClearTime = m_Bundle == null ? -1 : TimeTools.TimeGetTime() + AssetManager.BundleCanClearTimeOffsetMS;
		}

		AssetBundle m_Bundle = null;

		long CanClearTime = -1;

		public List<BundleData> Dependencies = new List<BundleData>();

		public static BundleData Create(string bundleKey)
		{
			BundleData bundleData		  = new BundleData();
			bundleData.BundleKey		  = bundleKey;
			bundleData.AbsPath			  = AssetManager.BundlePathToAbsPath(bundleKey);
			bundleData.Dependencies.Clear();
			return bundleData;
		}

		public bool Unload(bool isForce = true)
		{
			if (!IsCanClearNow() || !isForce)
			{
				CurState = State.Unload;
				return false;
			}

#if RESTORE_ASSET_IN_TABLE
			loadedAsset.Clear();
#endif
			LoadingPercent = 0;
			if (Bundle != null)
			{
				m_Bundle.Unload(false);
				m_Bundle = null;
			}

			CurState = State.Unload;

			return true;
		}

#if RESTORE_ASSET_IN_TABLE
		Dictionary< string, Dictionary<Type, UnityEngine.Object> > loadedAsset = new Dictionary<string, Dictionary<Type, UnityEngine.Object>>();
#endif

#if RESTORE_ASSET_IN_TABLE
		public T TryGetAsset<T>(string assetName) where T : UnityEngine.Object
		{
			if (Bundle == null)
				return null;

			ResetCanClearTime();

			Dictionary<Type, UnityEngine.Object> typeTable = null;
			if (!loadedAsset.TryGetValue(assetName, out typeTable))
			{
				typeTable = new Dictionary<Type, UnityEngine.Object>();
				loadedAsset.Add(assetName, typeTable);
			}

			UnityEngine.Object rt = null;

			Type t = typeof(T);
			typeTable.TryGetValue(t, out rt);
			return rt as T;
		}

		public void InsertLoadedAsset<T>(string assetName, T obj) where T : UnityEngine.Object
		{
			InsertLoadedAsset(assetName, obj, typeof(T));
		}

		public void InsertLoadedAsset(string assetName, UnityEngine.Object obj, Type t)
		{
			Dictionary<Type, UnityEngine.Object> typeTable = null;
			if (!loadedAsset.TryGetValue(assetName, out typeTable))
			{
				typeTable = new Dictionary<Type, UnityEngine.Object>();
				loadedAsset.Add(assetName, typeTable);
			}

			typeTable.Add( t, obj);
		}
#endif

		public UnityEngine.Object GetAsset(string assetName, Type loadType, bool isTryToLoad)
		{			
			if (Bundle == null)
				return null;

			ResetCanClearTime();

#if RESTORE_ASSET_IN_TABLE
			Dictionary<Type, UnityEngine.Object> typeTable = null;
			if (!loadedAsset.TryGetValue(assetName, out typeTable))
			{
				typeTable = new Dictionary<Type, UnityEngine.Object>();
				loadedAsset.Add(assetName, typeTable);
			}

			UnityEngine.Object rt = null;

			if (typeTable.TryGetValue( loadType, out rt))
			{
				return rt;
			}

			if (isTryToLoad)
			{
				rt = Bundle.LoadAsset(assetName, loadType);
				if (rt != null)
					typeTable.Add( loadType, rt);
			}
			else
				return null;

			return rt;
#else
			if (isTryToLoad)
				return Bundle.LoadAsset(assetName, loadType);
			else
				return null;
#endif
		}

		int orderlessIndex;
		public int CurOrderlessIndex()
		{
			return orderlessIndex;
		}

		public void SetOrderlessIndex(int index)
		{
			orderlessIndex = index;
		}
	}

	public class BundleLoadingRequest : OrderLessObj
	{
		public BundleData							MainBundleData	 = null;
		public AssetManager.OnLoadingCallbackFunc	LoadingCallback  = null;
		public AssetManager.OnLoadedCallbackFunc	OnLoadedCallback = null;
		public bool IsDirectlyBundle				= false;

		// return percent of process. 100 to finish
		public int UpdateProcess()
		{
			if (Request == null)
			{
				MainBundleData.LoadingPercent = 100;
				return MainBundleData.LoadingPercent;
			}

			if (Request.isDone)
			{
				MainBundleData.OnFinishLoading(Request.assetBundle);
				MainBundleData.LoadingPercent = 100;
				LoadingCallback?.Invoke(MainBundleData.LoadingPercent);
				OnLoadedCallback?.Invoke(MainBundleData);
			}

			MainBundleData.LoadingPercent = (int)(Request.progress * 100);
			LoadingCallback?.Invoke(MainBundleData.LoadingPercent);

			return MainBundleData.LoadingPercent;
		}

		public AssetBundleCreateRequest Request = null;

		int orderlessIndex;
		public int CurOrderlessIndex()
		{
			return orderlessIndex;
		}

		public void SetOrderlessIndex(int index)
		{
			orderlessIndex = index;
		}
	}

	public class AssetManager : SingletonMonoBehaviour<AssetManager>
	{
		public const int BundleCanClearTimeOffsetMS = 1000;

		public static long BundleCostMaxTimeMs = 5;

		public static long AssetCostMaxTimeMs  = 5;

		public static long BundleUnloadMaxTimeMs = 3;
		/// <summary>
		/// 是否使用 assetBundle 资源，在编辑模式下，建议该值改为 false
		/// </summary>
#if UNITY_EDITOR
#if TEST_RESOURCE
		public static readonly bool USE_ASSET_BUNDLE = true;
#else
		public static readonly bool USE_ASSET_BUNDLE = false;
	#endif
#else
		public static readonly bool USE_ASSET_BUNDLE = true;
#endif
		OrderlessPool<AssetLoadingRequest>		m_LoadingAssetTable		= new OrderlessPool<AssetLoadingRequest>();

		OrderlessPool<BundleLoadingRequest>		m_LoadingBundleTable	= new OrderlessPool<BundleLoadingRequest>();

		OrderlessPool<BundleData>				m_LoadedBundleTable		= new OrderlessPool<BundleData>();
		OrderlessPool<BundleData>				m_UnloadingBundleTable	= new OrderlessPool<BundleData>();

		Dictionary<string, BundleData>			m_DirectlyBundleTable	= new Dictionary<string, BundleData>();

		Dictionary<string, BundleData>			m_GlobeBundleTable		= new Dictionary<string, BundleData>();

		bool m_IsLoading = false;

		int checkBundleIndex = 0;

		/// <summary>
		/// Globe Asset Bundle Manifest. Using it to get the bundles dependent bundle.
		/// </summary>
		AssetBundleManifest mGlobeAssetBundleManifest = null;

		public AssetBundleManifest GlobeAssetBundleManifest
		{
			get
			{
				return mGlobeAssetBundleManifest;
			}
		}

		public static string ResBasePath = "";

		public static string PlatformPath = "";
		public static string GlobeManifestName = "";

		public static string ResRealPath = "";

		public static string HotPatchingBasePath = "";

		public static string GlobeManifestPath = "";

		public static string HotPatchingGlobeManifestPath = "";
		bool IsHotPatchingRes = false;

		public void InitDefault()
		{
			Instance.InitResourcePath();
			Instance.LoadBaseManifest();
			Instance.InitGlobeBundleTable();
		}

		public void InitResourcePath(string basePath = null)
		{

			if (string.IsNullOrEmpty(basePath))
			{
				basePath = Application.streamingAssetsPath;
			}

			ResBasePath = basePath;

#if UNITY_STANDALONE_WIN
			PlatformPath = "Assetbundles/StandaloneWindows";
			GlobeManifestName = "StandaloneWindows";
#elif UNITY_ANDROID
			PlatformPath = "Assetbundles/Android";
			GlobeManifestName = "Android";
#elif UNITY_IPHONE || UNITY_STANDALONE_OSX
			PlatformPath = "Assetbundles/Ios";
			GlobeManifestName = "Ios";
#endif
			ResRealPath = string.Format("{0}/{1}", ResBasePath, PlatformPath);

			GlobeManifestPath = string.Format("{0}/{1}", ResRealPath, GlobeManifestName);

			HotPatchingBasePath = string.Format("{0}/{1}", HotPatchingBasePath, PlatformPath);

			HotPatchingGlobeManifestPath = string.Format("{0}/{1}", HotPatchingBasePath, GlobeManifestName);
		}

		public void BuildDependBundleList(string bundlePath, ref List<string> includeDependBundleList)
		{
			bundlePath = ToBundlePath(bundlePath);
		}

		public static string AssetPathToAbsotionPath(string assetPath)
		{
			const string assetPathStartStr = "Assets/";
			if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(assetPathStartStr))
				return "";

			return string.Format("{0}/{1}", Application.dataPath, assetPath.Substring(assetPathStartStr.Length, assetPath.Length - assetPathStartStr.Length));
		}

		public static string AbsotionPathToAssetPath(string absPath)
		{
			if (string.IsNullOrEmpty(absPath) || !absPath.StartsWith(Application.dataPath))
				return "";

			return string.Format("Assets{0}", absPath.Substring(Application.dataPath.Length, absPath.Length - Application.dataPath.Length));
		}

		public static string AssetPathToBundlePath(string assetPath)
		{
			var lastLineIndex = assetPath.LastIndexOf('/');

			if (lastLineIndex == -1)
				return null;

			var bundlePath = assetPath.Substring(0, lastLineIndex).ToLower().Replace("\\", "/");

			return bundlePath;
		}

		public static string ToBundlePath(string bundlePath)
		{
			string rt = bundlePath.ToLower().Replace("\\", "/");
			return rt;
		}

		public static string BundlePathToAbsPath(string bundlePath)
		{
			string rt = string.Format("{0}/{1}.bundle", HotPatchingBasePath, bundlePath);

			if (!System.IO.File.Exists(rt))
			{
				rt = string.Format("{0}/{1}.bundle", ResRealPath, bundlePath);
			}

			return rt;
		}

		public bool LoadBaseManifest()
		{
#if UNITY_EDITOR
			if (!USE_ASSET_BUNDLE)
				return true;
#endif

			AssetBundle Bundle = null;

			if (System.IO.File.Exists(HotPatchingGlobeManifestPath))
			{
				Bundle = AssetBundle.LoadFromFile(HotPatchingGlobeManifestPath);
				IsHotPatchingRes = Bundle != null;
			}

			if (Bundle == null)
			{
				Bundle = AssetBundle.LoadFromFile(GlobeManifestPath);
			}

			if (Bundle == null)
				return false;

			mGlobeAssetBundleManifest = Bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			return mGlobeAssetBundleManifest == null;
		}

		public void InitGlobeBundleTable()
		{
			m_LoadedBundleTable.Clear();
			m_DirectlyBundleTable.Clear();
			m_GlobeBundleTable.Clear();

			for(int i = 0; i < m_UnloadingBundleTable.Count; ++i)
			{
				m_UnloadingBundleTable[i].Unload();
			}
			m_UnloadingBundleTable.Clear();

			Resources.UnloadUnusedAssets();

			if (mGlobeAssetBundleManifest == null)
				return;

			var globeBundlePathes = mGlobeAssetBundleManifest.GetAllAssetBundles();
			string bundleKey = null;
			for (int i = 0; i < globeBundlePathes.Length; ++i)
			{
				bundleKey = globeBundlePathes[i].Substring(0, globeBundlePathes[i].Length - 7/*".bundle".Length*/ );
				var bundleData = BundleData.Create(bundleKey);
				m_GlobeBundleTable.Add(bundleKey, bundleData);
			}

			BundleData dependencyBundleData = null;

			// Build Bunldle dependencies
			foreach ( var bundleData in m_GlobeBundleTable.Values)
			{
				string[] dependencies = mGlobeAssetBundleManifest.GetAllDependencies(bundleData.BundleKey + ".bundle");
				for (int i = 0; i < dependencies.Length; ++i)
				{
					dependencies[i] = dependencies[i].Substring(0, dependencies[i].Length - 7/*".bundle".Length*/ );
				}

				bundleData.Dependencies.Clear();
				for (int i = 0; i < dependencies.Length; ++i)
				{
					var dependenciesBundleKey = dependencies[i];
					
					if (!m_GlobeBundleTable.TryGetValue(dependenciesBundleKey, out dependencyBundleData))
					{
						Log.Print(LogLevel.Error, "InitGlobeBundleTable Error: [{0}] can't find bundle data.", dependenciesBundleKey);
					}

					bundleData.Dependencies.Add(dependencyBundleData);
				}
			}

		}

		public BundleData GetBundleData(string bundleKey)
		{
			BundleData bundleData = null;
			Instance.m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData);
			return bundleData;
		}

		public void LoadAsset( AssetRef assetRef, OnLoadedAssetCallbackFunc onloadedFunc)
		{
			if (string.IsNullOrEmpty(assetRef.Path))
				return;

#if UNITY_EDITOR
			if (!USE_ASSET_BUNDLE)
			{
				assetRef.OnLoaded(UnityEditor.AssetDatabase.LoadAssetAtPath( assetRef.Path, assetRef.LoadType), null);
				onloadedFunc?.Invoke(assetRef, null);
				return;
			}
#endif

			//T rt = null;

			var bundleKey = AssetPathToBundlePath(assetRef.Path);

			if (bundleKey == null)
				return;

			BundleData bundleData = null;
			if (!m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData))
			{
				Log.Print(LogLevel.Error, "Loading Asset: {0} Failed! Bundle File:{1} is not exsited!", assetRef.Path, bundleKey);
				return;
			}

			if (bundleData.IsLoading)
			{
				Log.Print(LogLevel.Warning, "Loading Asset: {0} Immediate Mode Failed! dependices bundle is loading in sync way.", assetRef.Path);
				LoadAssetAsync( assetRef, onloadedFunc);
				return;
			}

			LoadBundleImmediate(bundleData);

			assetRef.OnLoaded( bundleData.GetAsset( assetRef.Path, assetRef.LoadType, true), bundleData);

			onloadedFunc?.Invoke(assetRef, bundleData);
		}

		public void LoadAssetAsync(AssetRef assetRef, OnLoadedAssetCallbackFunc onloadedFunc)
		{ 
			if (string.IsNullOrEmpty(assetRef.Path))
				return;

#if UNITY_EDITOR
			if (!USE_ASSET_BUNDLE)
			{
				assetRef.OnLoaded(UnityEditor.AssetDatabase.LoadAssetAtPath(assetRef.Path, assetRef.LoadType), null);
				onloadedFunc?.Invoke(assetRef, null);
				return;
			}
#endif

			var bundleKey = AssetPathToBundlePath(assetRef.Path);

			if (bundleKey == null)
				return;

			BundleData bundleData = null;
			if (!m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData))
			{
				Log.Print(LogLevel.Error, "Loading Asset: {0} Failed! Bundle File:{1} is not exsited!", assetRef.Path, bundleKey);
				return;
			}

			CreateAssetLoadingRequest( assetRef, bundleData, onloadedFunc);
		}

		public bool CreateAssetLoadingRequest(AssetRef assetRef, BundleData bundleData, OnLoadedAssetCallbackFunc onloadedFunc)
		{
			if (assetRef == null || string.IsNullOrEmpty(assetRef.Path))
				return false;

			bundleData.AddRef();

			var assetObj = bundleData.GetAsset(assetRef.Path, assetRef.LoadType, false);
			if (assetObj != null)
			{
				assetRef.OnLoaded(assetObj, bundleData);
				onloadedFunc(assetRef, bundleData);
				return true;
			}

			AssetLoadingRequest assetLoadingRequest		= new AssetLoadingRequest();
			assetLoadingRequest.assetRef				= assetRef;
			assetLoadingRequest.MainBundleData			= bundleData;
			assetLoadingRequest.OnLoadedCallback		= onloadedFunc;

			CreateBundleLoadingRequest(bundleData);

			m_LoadingAssetTable.Insert(assetLoadingRequest);

			return true;
		}

		public bool CreateBundleLoadingRequest(BundleData bundleData, bool isDirectlyBundle = false, OnLoadedCallbackFunc loadedFunc = null, OnLoadingCallbackFunc loadingFunc = null)
		{
			if (bundleData == null)
				return false;

			if (bundleData.CurState == BundleData.State.Loading)
				return true;

			if (bundleData.CurState == BundleData.State.Unloading)
			{
				ResetLoadedState(bundleData);
				return true;
			}

			if (bundleData.IsCanRead)
			{
				loadingFunc?.Invoke(100);
				loadedFunc?.Invoke(bundleData);

				if (isDirectlyBundle)
				{
					BundleData existBundleData = null;
					if (!m_DirectlyBundleTable.TryGetValue(bundleData.BundleKey, out existBundleData))
					{
						m_DirectlyBundleTable.Add(bundleData.BundleKey, bundleData);
					}
				}

				return true;
			}

			for (int i = 0; i < bundleData.Dependencies.Count; ++i)
			{
				var depend = bundleData.Dependencies[i];
				if (depend == null)
					continue;

				CreateBundleLoadingRequest(depend, false);
			}

			if (bundleData.CurState == BundleData.State.Unload)
			{
				bundleData.CurState = BundleData.State.Loading;

				BundleLoadingRequest bundleLoadingRequest = new BundleLoadingRequest();
				bundleLoadingRequest.MainBundleData = bundleData;
				bundleLoadingRequest.IsDirectlyBundle = isDirectlyBundle;

				bundleLoadingRequest.LoadingCallback = loadingFunc;
				bundleLoadingRequest.OnLoadedCallback = loadedFunc;

				bundleLoadingRequest.Request = AssetBundle.LoadFromFileAsync(bundleLoadingRequest.MainBundleData.AbsPath);

				m_LoadingBundleTable.Insert(bundleLoadingRequest);
			}

			return true;
		}

		public static bool CheckBundleIsLoad(string bundleKey)
		{
			if (Instance == null)
				return false;

			string key = ToBundlePath(bundleKey);

			BundleData bundleData = null;

			if (!Instance.m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData))
				return false;

			return bundleData != null && bundleData.IsCanRead;
		}

		//public static void BuildincludedDependentBundleTable(ref List<BundleData> includedDependentPreloadBundle, BundleData bundleData)
		//{
		//	if (Instance == null)
		//	{
		//		Debug.LogError("BuildLoadingQueue Failed! AssetManager is not Instantiated!");
		//		return;
		//	}

		//	if ( includedDependentPreloadBundle.Contains(bundleData) )
		//		return;

		//	string bundlePath = string.Format("{0}.bundle", bundleData.BundlePath);

		//	string[] dependencies = Instance.mGlobeAssetBundleManifest.GetAllDependencies(bundlePath);

		//	for (int i = 0; i < dependencies.Length; ++i)
		//	{
		//		string dependenciesPath = dependencies[i].Substring(0, dependencies[i].Length - 7/*".bundle".Length*/ );

		//		if (AssetManager.CheckBundleIsLoad(dependenciesPath))
		//			continue;

		//		if (includedDependentPreloadBundle.Contains(dependenciesPath))
		//			continue;

		//		includedDependentPreloadBundle.Add(dependenciesPath);
		//	}

		//	includedDependentPreloadBundle.Add(path);
		//}

		public delegate void OnLoadingCallbackFunc(int percent);

		public delegate void OnLoadedCallbackFunc(BundleData data);

		public delegate void OnLoadedAssetCallbackFunc(AssetRef assetRef, BundleData bundleData);

		public bool LoadBundleImmediate(string bundleKey)
		{
			BundleData bundleData = null;

			if (!Instance.m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData))
				return false;

			return LoadBundleImmediate(bundleData);
		}

		void ResetLoadedState(BundleData bundleData)
		{
			if (bundleData == null)
				return;

			bundleData.CurState = BundleData.State.Loaded;
			m_UnloadingBundleTable.Remove(bundleData);
			m_LoadedBundleTable.Insert(bundleData);

			for (int i = 0; i < bundleData.Dependencies.Count; ++i)
			{
				var depend = bundleData.Dependencies[i];
				if (depend.CurState == BundleData.State.Unloading)
				{
					m_UnloadingBundleTable.Remove(depend);
					m_LoadedBundleTable.Insert(depend);
				}
				depend.CurState = BundleData.State.Loaded;
			}
		}

		public bool LoadBundleImmediate(BundleData bundleData, bool isDirectlyBundle = false)
		{
			if (bundleData == null)
			{
				return false;
			}

			if (bundleData.IsCanRead)
			{
				ResetLoadedState(bundleData);
				return true;
			}

			for (int i = 0; i < bundleData.Dependencies.Count; ++i)
			{
				var depend = bundleData.Dependencies[i];

				switch(depend.CurState)
				{
					case BundleData.State.Unloading:
						{
							m_UnloadingBundleTable.Remove(depend);
							m_LoadedBundleTable.Insert(depend);
						}
						break;
					case BundleData.State.Unload:
						{
							var dependentBundle = AssetBundle.LoadFromFile(depend.AbsPath);
							depend.OnFinishLoading(dependentBundle);
							m_LoadedBundleTable.Insert(depend);
						}
						break;
					case BundleData.State.Loading:
						{
							Log.Print(LogLevel.Error, "LoadBundleImmediate can not accept loading bundle data: {0}!", bundleData.BundleKey);
							return false;
						}
					default:
						break;
				}

				depend.CurState = BundleData.State.Loaded;
			}

			switch (bundleData.CurState)
			{
				case BundleData.State.Unloading:
					{
						m_UnloadingBundleTable.Remove(bundleData);
						m_LoadedBundleTable.Insert(bundleData);
					}
					break;
				case BundleData.State.Unload:
					{
						var mainBundle = AssetBundle.LoadFromFile(bundleData.AbsPath);
						bundleData.OnFinishLoading(mainBundle);
						m_LoadedBundleTable.Insert(bundleData);
					}
					break;
				case BundleData.State.Loading:
					{
						Log.Print(LogLevel.Error, "LoadBundleImmediate can not accept loading bundle data: {0}!", bundleData.BundleKey);
						return false;
					}
				default:
					break;
			}

			bundleData.CurState = BundleData.State.Loaded;

			if (isDirectlyBundle)
			{
				BundleData tempBundleData = null;
				if (!m_DirectlyBundleTable.TryGetValue(bundleData.BundleKey, out tempBundleData))
				{
					m_DirectlyBundleTable.Add(bundleData.BundleKey, bundleData);
				}
			}

			return true;
		}

		public bool LoadBundle(string bundleKey, bool isDirectlyBundle = false, OnLoadedCallbackFunc loadedFunc = null, OnLoadingCallbackFunc loadingFunc = null)
		{
			BundleData bundleData = null;

			if (!Instance.m_GlobeBundleTable.TryGetValue(bundleKey, out bundleData))
				return false;

			LoadBundle(bundleData, isDirectlyBundle, loadedFunc, loadingFunc);
			return true;
		}

		public void LoadBundle(BundleData bundleData, bool isDirectlyBundle = false, OnLoadedCallbackFunc loadedFunc = null, OnLoadingCallbackFunc loadingFunc = null)
		{
#if UNITY_EDITOR
			if (!USE_ASSET_BUNDLE)
			{
				loadingFunc?.Invoke(100);
				loadedFunc?.Invoke(null);
			}
#endif
			if (bundleData == null)
			{
				return;
			}

			if (bundleData.IsCanRead)
			{
				ResetLoadedState(bundleData);
				return;
			}

			CreateBundleLoadingRequest(bundleData, isDirectlyBundle, loadedFunc, loadingFunc);
		}

//		public void LoadBundle(List<string> bundlePathes, bool isDirectlyBundle = false, OnLoadedCallbackFunc loadedFunc = null, OnLoadingCallbackFunc loadingFunc = null)
//		{
//#if UNITY_EDITOR
//			if (!USE_ASSET_BUNDLE)
//			{
//				if (loadingFunc != null)
//					loadingFunc(100);

//				if (loadedFunc != null)
//					loadedFunc(null);
//			}
//#endif
//		}

		public void ClearDirectlyBundleList()
		{
			m_DirectlyBundleTable.Clear();
		}

		public static void RemoveUselessBundle()
		{
			if (Instance == null)
				return;

			var totalBundleList = Instance.m_GlobeBundleTable;

			var directlyBundleList = Instance.m_DirectlyBundleTable;

			foreach (var bundleData in Instance.m_GlobeBundleTable.Values)
			{
				if (bundleData == null)
					continue;

				bundleData.IsUseless = true;
			}

			foreach (var bundleData in Instance.m_DirectlyBundleTable.Values)
			{
				if (bundleData == null)
					continue;

				bundleData.IsUseless = false;

				var dependentList = bundleData.Dependencies;

				for (int j = 0; j < dependentList.Count; ++j)
				{
					dependentList[j].IsUseless = false;
				}
			}

			foreach (var bundleData in Instance.m_GlobeBundleTable.Values)
			{
				if (bundleData == null)
					continue;

				if (bundleData.IsUseless)
				{
					bundleData.Unload();
				}
			}

			Resources.UnloadUnusedAssets();

			GC.Collect();
		}


		void Update()
		{
			OnLoadingBundleProcess();
			OnLoadingAssetProcess();
			OnUnLoadingUselessBundle();
		}

		void OnLoadingBundleProcess()
		{
			var beginTime = TimeTools.BeginTime();

			for (int i = 0; i < m_LoadingBundleTable.Count; ++i)
			{
				var bundleRequest = m_LoadingBundleTable[i];

				if (m_LoadingBundleTable[i].UpdateProcess() == 100)
				{
					m_LoadingBundleTable.Remove(bundleRequest);
					--i;
				}

				if (TimeTools.PassTime(beginTime) > BundleCostMaxTimeMs)
					return;
			}
		}

		void OnLoadingAssetProcess()
		{
			var beginTime = TimeTools.BeginTime();

			for (int i = 0; i < m_LoadingAssetTable.Count; ++i)
			{
				var assetRequest = m_LoadingAssetTable[i];

				if (m_LoadingAssetTable[i].UpdateProcess() == 100)
				{
					m_LoadingAssetTable.Remove(assetRequest);
					--i;
				}

				if (TimeTools.PassTime(beginTime) > AssetCostMaxTimeMs)
					return;
			}
		}
	
		void OnUnLoadingUselessBundle()
		{
			if (m_LoadedBundleTable.Count == 0)
				return;

			checkBundleIndex %= m_LoadedBundleTable.Count;

			var beginTime = TimeTools.BeginTime();

			for ( int i = 0; i < m_LoadedBundleTable.Count; ++i )
			{
				if (TimeTools.PassTime(beginTime) > BundleUnloadMaxTimeMs)
					return;

				int index = (i + checkBundleIndex) % m_LoadedBundleTable.Count;

				var bundle = m_LoadedBundleTable[index];

				if (bundle == null)
					continue;

				if (bundle.CurState != BundleData.State.Loaded)
					continue;

				if (bundle.GetRef > 0)
					continue;

				m_UnloadingBundleTable.Insert(bundle);
			}
		}
	}
}
