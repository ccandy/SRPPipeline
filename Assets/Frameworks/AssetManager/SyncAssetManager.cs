using System;
using System.Collections.Generic;
using Frameworks.Common;
using UnityEngine;

namespace Frameworks.Asset
{
	public class SyncAssetManager : SingletonMonoBehaviour<SyncAssetManager>
	{
		Dictionary<string, Stack<RecycleActor> > AssetObjectPool = new Dictionary< string, Stack<RecycleActor> >();

		public delegate void OnFinishLoadAsset(RecycleActor actor);

		public void RecycleAssetObject(  )
		{

		}

		public RecycleActor LoadAsset( string assetPath , OnFinishLoadAsset finishLoadedFunc, bool isDirectlyBundle = false)
		{
			if (string.IsNullOrEmpty(assetPath))
				return null;

			Stack<RecycleActor> stack = null;
			if ( !AssetObjectPool.TryGetValue( assetPath, out stack) )
			{
				stack = new Stack<RecycleActor>();
				AssetObjectPool.Add(assetPath, stack);
			}

			RecycleActor actor = null;
			if (stack.Count != 0)
			{
				actor = stack.Pop();
				finishLoadedFunc?.Invoke(actor);
				return actor;
			}

			actor = new RecycleActor(assetPath);
			actor.OnReset();

			return actor;

		}
	}
}
