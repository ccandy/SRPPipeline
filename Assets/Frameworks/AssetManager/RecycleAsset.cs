using Frameworks.Asset;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common
{
	public class RecycleAsset : MonoBehaviour
	{
		public string AssetPath = string.Empty;

		public virtual void OnReset()
		{

		}

		public virtual void OnRecycle()
		{
			transform.position = new Vector3(10000.0f, 10000.0f, 10000.0f);
		}

		protected virtual void OnDestroy()
		{
			
		}
	}

	public partial class RecycleActor : IDisposable
	{
		AssetRef PrefabAsset = null;

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		public RecycleActor(AssetRef prefabAsset)
		{
			PrefabAsset = new AssetRef(prefabAsset.Path, typeof(GameObject));
		}

		public RecycleActor(string  prefabAssetPath)
		{
			PrefabAsset = new AssetRef(prefabAssetPath, typeof(GameObject));
		}

		public virtual void OnReset()
		{
			if (PrefabAsset == null)
				return;

			if (PrefabAsset.AssetObject != null)
			{
				OnAssetLoaded(PrefabAsset, null);
			}
			else
			{
				PrefabAsset.DoLoadAssetAsync(OnAssetLoaded);
			}
		}

		public virtual void OnRecycle()
		{

		}

		protected virtual void OnAssetLoaded(AssetRef assetRef, BundleData bundleData)
		{

		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (PrefabAsset != null)
				{
					PrefabAsset.Dispose();
				}
				disposedValue = true;
			}
		}

		~RecycleActor() 
		{
		   Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			//GC.SuppressFinalize(true); 
		}
		#endregion

	}
}
