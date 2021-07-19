using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Frameworks.Common;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Frameworks.CRP
{
	public class DeferredRenderPassBlockAsset : RenderPassBlockAsset
	{
		public bool IsReceiveEnv = true;

#if UNITY_EDITOR
		public static RenderPassBlockAsset Create()
		{
			var instance = CreateInstance<DeferredRenderPassBlockAsset>();
			return instance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
		internal class CreateRenderPassBlockAsset : EndNameEditAction
		{
			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				//Create asset
				AssetDatabase.CreateAsset(Create(), pathName);
			}
		}

		[MenuItem("Assets/Create/Rendering/Custom Render Pipeline/Deferred Render Pass Block Asset", priority = CoreUtils.assetCreateMenuPriority1)]
		static void DoCreateRenderPassBlockAsset()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateRenderPassBlockAsset>(),
				"DeferredRenderPassBlockAsset.asset", null, null);
		}
#endif

		public override RenderPassBlock CreatePassBlock()
		{
			var renderPassBlock = new DeferredRenderPassBlock();

			renderPassBlock.ClearPasses();
			renderPassBlock.InitPasses();

			for (int i = 0; i < CustomPassAssets.Count; ++i)
			{
				var asset = CustomPassAssets[i];
				if (asset == null)
				{
					Log.Print(LogLevel.Error, "CreatePassBlock Error index == {0} is null assets", i);
					continue;
				}
				renderPassBlock.AddPass(asset.CreatePass(renderPassBlock));
			}

			renderPassBlock.SortPasses();

			return renderPassBlock;
		}
	}
}
