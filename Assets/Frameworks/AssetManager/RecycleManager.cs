using System;
using System.Collections.Generic;
using Frameworks.Common;
using UnityEngine;

namespace Frameworks.Asset
{
	public interface IRecycleObjLoaded
	{
		void OnAssetLoad(RecycleAsset asset, string path);
	}

	public class RecycleAssetManager : SingletonMonoBehaviour<RecycleAssetManager>
	{
		Dictionary<string, Stack<RecycleAsset> > AssetRecyclePool = new Dictionary<string, Stack<RecycleAsset>>();

		const int RecyclePoolMaxSize = 20;

		public void Clear()
		{
			foreach ( var pool in AssetRecyclePool.Values)
			{
				for (; pool.Count > 0;)
				{
					var asset = pool.Pop();

					if (asset == null)
						continue;

					Destroy(asset.gameObject);
				}
			}

			AssetRecyclePool.Clear();
		}

		public delegate void OnNewObjectLoaded(RecycleAsset asset, string path);

		public virtual void NewObj( string assetPath, IRecycleObjLoaded objLoaded = null)
		{
			if (AssetManager.Instance == null)
			{
				objLoaded?.OnAssetLoad(null, assetPath);
				return;
			}

			#region Get Data From Recycle Pool

			Stack<RecycleAsset> recycledObjs = null;

			AssetRecyclePool.TryGetValue(assetPath, out recycledObjs);

			if (recycledObjs != null && recycledObjs.Count > 0)
			{
				var recycleOne = recycledObjs.Pop();

				recycleOne.OnReset();

#if UNACTIVE_RECYCLE_OBJ
				recycleOne.gameObject.SetActive(true);
#else
#endif

				objLoaded?.OnAssetLoad(recycleOne, assetPath);
			}
			#endregion

			AssetRef assetRef = new AssetRef(assetPath, typeof(GameObject));

			AssetManager.Instance.LoadAsset( assetRef, (asset, bundleData) =>
			{
				GameObject prefab = asset.AssetObject as GameObject;

				if (prefab == null)
				{
					objLoaded?.OnAssetLoad(null, assetPath);
					return;
				}

				GameObject obj = GameObject.Instantiate(prefab);

				var newOne = obj.GetComponent<RecycleAsset>();
				if (newOne == null)
				{
					Log.Print(LogLevel.Warning, "{0} do not have Recycle Asset Behaviour. Auto Add.", assetRef.Path);
					newOne = obj.AddComponent<RecycleAsset>();
				}

				newOne.AssetPath = assetPath;

				objLoaded?.OnAssetLoad(newOne, assetRef.Path);
			});
		}

		public virtual void NewObj (AssetRef assetRef, IRecycleObjLoaded objLoaded = null)
		{
			if (assetRef == null)
				return;

			if (AssetManager.Instance == null)
			{
				objLoaded?.OnAssetLoad(null, assetRef.Path);
				return;
			}

			#region Get Data From Recycle Pool

			Stack<RecycleAsset> recycledObjs = null;

			AssetRecyclePool.TryGetValue(assetRef.Path, out recycledObjs);

			if (recycledObjs != null && recycledObjs.Count > 0)
			{
				var recycleOne = recycledObjs.Pop();

				recycleOne.OnReset();

#if UNACTIVE_RECYCLE_OBJ
				recycleOne.gameObject.SetActive(true);
#else
#endif

				objLoaded?.OnAssetLoad(recycleOne, assetRef.Path);
				return;
			}
			#endregion

			AssetManager.Instance.LoadAssetAsync(assetRef, (asset, bundleData) =>
			{
				GameObject prefab = asset.AssetObject as GameObject;

				if (prefab == null)
				{
					objLoaded?.OnAssetLoad(null, assetRef.Path);
					return;
				}

				GameObject obj = GameObject.Instantiate(prefab);

				var newOne = obj.GetComponent<RecycleAsset>();
				if (newOne == null)
				{
					Log.Print(LogLevel.Warning, "{0} do not have Recycle Asset Behaviour. Auto Add.", assetRef.Path);
					newOne = obj.AddComponent<RecycleAsset>();
				}

				newOne.AssetPath = assetRef.Path;

				objLoaded?.OnAssetLoad(newOne, assetRef.Path);
			});
		}

		public virtual void RecycleObj(RecycleAsset recycleAsset)
		{
			if (recycleAsset == null)
				return;

			if (string.IsNullOrEmpty(recycleAsset.AssetPath))
			{
				Destroy(recycleAsset.gameObject);
				return;
			}

#if UNACTIVE_RECYCLE_OBJ
			recycleAsset.gameObject.SetActive(false);
#else
#endif
			recycleAsset.OnRecycle();

			Stack<RecycleAsset> recycledObjs = null;

			AssetRecyclePool.TryGetValue(recycleAsset.AssetPath, out recycledObjs);
			if (recycledObjs == null)
			{
				recycledObjs = new Stack<RecycleAsset>();
				AssetRecyclePool.Add(recycleAsset.AssetPath, recycledObjs);
			}

			if (recycledObjs.Count < RecyclePoolMaxSize)
				recycledObjs.Push(recycleAsset);
			else
				Destroy(recycleAsset.gameObject);
		}
	}
}
