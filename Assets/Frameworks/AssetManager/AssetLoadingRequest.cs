using System;
using System.Collections.Generic;
using System.Linq;
using Frameworks.Common;

using System.Collections;

using UnityEngine;

namespace Frameworks.Asset
{
	public class AssetLoadingRequest : OrderLessObj
	{
		public AssetRef assetRef;

		public BundleData MainBundleData;

		public AssetBundleRequest Request = null;

		public AssetManager.OnLoadedAssetCallbackFunc OnLoadedCallback = null;

		int orderlessIndex;
		public int CurOrderlessIndex()
		{
			return orderlessIndex;
		}

		public void SetOrderlessIndex(int index)
		{
			orderlessIndex = index;
		}

		public int UpdateProcess()
		{
			if (MainBundleData == null)
			{
				return 100;
			}

			if (!MainBundleData.IsCanRead)
			{
				AssetManager.Instance.LoadBundle(MainBundleData);
				return 0;
			}

			if (Request == null)
			{
				var asset = MainBundleData.GetAsset(assetRef.Path, assetRef.LoadType, false);
				if (asset != null)
				{
					assetRef.OnLoaded( asset, MainBundleData );
					OnLoadedCallback?.Invoke(assetRef, MainBundleData);
					return 100;
				}

				Request = MainBundleData.Bundle.LoadAssetAsync(assetRef.Path, assetRef.LoadType);
			}

			if (Request.isDone)
			{
#if RESTORE_ASSET_IN_TABLE
				MainBundleData.InsertLoadedAsset(assetRef.Path, Request.asset, assetRef.LoadType);
#endif
				assetRef.OnLoaded(Request.asset, MainBundleData);
				OnLoadedCallback?.Invoke(assetRef, MainBundleData);
			}

			int per = (int)(Request.progress * 100);

			return per;
		}
	}
}
