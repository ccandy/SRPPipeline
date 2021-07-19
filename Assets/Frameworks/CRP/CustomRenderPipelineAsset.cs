using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Frameworks.CRP
{
	public enum ShadowType
	{
		Disable,
		HardShadow,
		SoftShadow,
	}

	public enum MSAAQuality
	{
		Disable = 1,
		x2 = 2,
		x4 = 4,
		x8 = 8
	}

	public enum EnvType
	{
		NONE,
		GLOBAL_CUBE,
	}

	public enum DefaultMaterialType
	{
		Standard,
		Particle,
		Terrain,
		Sprite,
		UnityBuiltinDefault
	}

	[ExcludeFromPreset]
	public class CustomRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver
	{
		#region Base Properties
		public bool SupportsHDR					= true;
		public bool UseMSAA						= false;

		public bool UseSRPBatcher				= true;
		public bool SupportsDynamicBatching		= true;
		public bool MixedLightingSupported		= true;

		public MSAAQuality MSAASampleCount		= MSAAQuality.Disable;
		public float		RenderScale			= 1.0f;

		#endregion

		#region Lighting Properties
		public EnvType EnvironmentType = EnvType.NONE;

		[HideInInspector]
		public Cubemap GlobalCubeMap;

		[HideInInspector]
		public SHValue CubeSH9;

		#endregion

		#region Shadow Properties
		public ShadowType shadowType						= ShadowType.SoftShadow;
		public int ShadowMapResolution						= 2048;
		// Shadows Settings
		public float	ShadowDistance						= 50.0f;
		public int		ShadowCascadeCount					= 1;
		public Vector4	CascadeSplits						= Vector4.zero;
		public static readonly float   DefaultCascade2Split	= 0.25f;
		public static readonly Vector2 DefaultCascade3Split = new Vector2(0.1f, 0.3f);
		public static readonly Vector3 DefaultCascade4Split = new Vector3(0.067f, 0.2f, 0.467f);
		public float	ShadowDepthBias						= 1.0f;
		public float	ShadowNormalBias					= 1.0f;


		public const int _ShadowCascadeMinCount = 1;
		public const int _ShadowCascadeMaxCount = 4; // F**k Unity Rendering System. lock the cascade everywhere!!

		#endregion

		#region Post Process Properties


		#endregion

		Shader m_defaultShader;

		Material GetMaterial(DefaultMaterialType materialType)
		{
#if UNITY_EDITOR
			return null;
			//var material = scriptableRendererData.GetDefaultMaterial(materialType);
			//if (material != null)
			//	return material;

			//switch (materialType)
			//{
			//	case DefaultMaterialType.Standard:
			//		return editorResources.materials.lit;

			//	case DefaultMaterialType.Particle:
			//		return editorResources.materials.particleLit;

			//	case DefaultMaterialType.Terrain:
			//		return editorResources.materials.terrainLit;

			//	// Unity Builtin Default
			//	default:
			//		return null;
			//}
#else
            return null;
#endif
		}

		public override Material defaultMaterial
		{
			get { return GetMaterial(DefaultMaterialType.Standard); }
		}

		public override Material defaultParticleMaterial
		{
			get { return GetMaterial(DefaultMaterialType.Particle); }
		}

		public override Material defaultLineMaterial
		{
			get { return GetMaterial(DefaultMaterialType.Particle); }
		}

		public override Material defaultTerrainMaterial
		{
			get { return GetMaterial(DefaultMaterialType.Terrain); }
		}

		public override Material defaultUIMaterial
		{
			get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
		}

		public override Material defaultUIOverdrawMaterial
		{
			get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
		}

		public override Material defaultUIETC1SupportedMaterial
		{
			get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
		}

		public override Material default2DMaterial
		{
			get { return GetMaterial(DefaultMaterialType.Sprite); }
		}

		public override Shader defaultShader
		{
			get
			{
				if (m_defaultShader == null)
					m_defaultShader = Shader.Find("CustomRenderPipeline/DeferredLit");
				return m_defaultShader;
			}
		}

		public List<RenderPassBlockAsset> renderPassBlockAssets = new List<RenderPassBlockAsset>(1);

#if UNITY_EDITOR
		public static CustomRenderPipelineAsset Create()
		{
			// Create Custom RP Asset
			var instance = CreateInstance<CustomRenderPipelineAsset>();
			return instance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
		internal class CreateCustomRenderPipelineAsset : EndNameEditAction
		{
			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				//Create asset
				AssetDatabase.CreateAsset( Create(), pathName);
			}
		}

		[MenuItem("Assets/Create/Rendering/Custom Render Pipeline/Pipeline Asset", priority = CoreUtils.assetCreateMenuPriority1)]
		static void CreateUniversalPipeline()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateCustomRenderPipelineAsset>(),
				"CustomRenderPipelineAsset.asset", null, null);
		}
#endif

		protected override RenderPipeline CreatePipeline()
		{
			return new CustomRenderPipeline(this);
		}

		protected override void OnValidate()
		{
			// This will call RenderPipelineManager.CleanupRenderPipeline that in turn disposes the render pipeline instance and
			// assign pipeline asset reference to null
			base.OnValidate();
		}

		public void OnAfterDeserialize()
		{

		}

		public void OnBeforeSerialize()
		{
		}
	}
}
