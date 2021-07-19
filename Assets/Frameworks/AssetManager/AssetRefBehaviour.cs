using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Asset
{
	public class AssetRef : IDisposable
	{ 
		public AssetRef( string path, Type loadType )
		{
			m_Path = path;
			m_AssetObject = null;
			m_LoadType = loadType;
		}

		public AssetRef(Type loadType)
		{
			m_Path = "";
			m_AssetObject = null;
			m_LoadType = loadType;
		}

		public string Path
		{
			set
			{
				if (value.CompareTo(m_Path) == 0)
					return;

				if (m_AssetObject != null)
				{
					Dispose(true);
				}
				m_Path = value;
			}
			get
			{
				return m_Path;
			}
		}

		string m_Path = null;

		public Type LoadType
		{
			set
			{
				if (m_LoadType == value)
					return;

				if (m_AssetObject != null)
				{
					Dispose(true);
				}
				m_LoadType = value;
			}
			get
			{
				return m_LoadType;
			}
		}
		Type m_LoadType = null;

		public UnityEngine.Object AssetObject
		{
			get
			{
				return m_AssetObject;
			}
		}

		UnityEngine.Object m_AssetObject;

		public void DoLoadAsset(AssetManager.OnLoadedAssetCallbackFunc loadedCallback = null)
		{
			AssetManager.Instance.LoadAsset( this, loadedCallback);
		}

		public void DoLoadAssetAsync(AssetManager.OnLoadedAssetCallbackFunc loadedCallback = null)
		{
			AssetManager.Instance.LoadAssetAsync(this, loadedCallback);
		}

		public virtual void OnLoaded( UnityEngine.Object obj, BundleData bundle)
		{
			RequireBundle = bundle;
			m_AssetObject = obj;
			disposedValue = false;
		}

		#region IDisposable Support
		private bool disposedValue = true;
		BundleData RequireBundle = null;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (RequireBundle != null)
				{
					RequireBundle.ReduceRef();
					RequireBundle = null;
				}

				m_AssetObject = null;
				disposedValue = true;
			}
		}

		 ~AssetRef()
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
