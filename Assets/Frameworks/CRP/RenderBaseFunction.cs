using System;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Frameworks.CRP
{
	public static class RenderBaseFunction
	{
		static readonly ProfilingSampler createCameraRenderTarget = new ProfilingSampler("CreateCameraRenderTarget");

		static Mesh s_FullscreenMesh = null;

		public static Mesh fullscreenMesh
		{
			get
			{
				if (s_FullscreenMesh != null)
					return s_FullscreenMesh;

				float topV = 1.0f;
				float bottomV = 0.0f;

				s_FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
				s_FullscreenMesh.SetVertices(new List<Vector3>
				{
					new Vector3(-1.0f, -1.0f, 0.0f),
					new Vector3(-1.0f,  1.0f, 0.0f),
					new Vector3(1.0f, -1.0f, 0.0f),
					new Vector3(1.0f,  1.0f, 0.0f)
				});

				s_FullscreenMesh.SetUVs(0, new List<Vector2>
				{
					new Vector2(0.0f, bottomV),
					new Vector2(0.0f, topV),
					new Vector2(1.0f, bottomV),
					new Vector2(1.0f, topV)
				});

				s_FullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
				s_FullscreenMesh.UploadMeshData(true);
				return s_FullscreenMesh;
			}
		}

		static Material s_ErrorMaterial;
		static Material errorMaterial
		{
			get
			{
				if (s_ErrorMaterial == null)
				{
					// TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
					// This might be in a point that some resources required for the pipeline are not finished importing yet.
					// Proper fix is to add a fence on asset import.
					try
					{
						s_ErrorMaterial = new Material(Shader.Find("Hidden/Custom Render Pipeline/FallbackError"));
					}
					catch { }
				}

				return s_ErrorMaterial;
			}
		}

		// Caches render texture format support. SystemInfo.SupportsRenderTextureFormat and IsFormatSupported allocate memory due to boxing.
		static Dictionary<RenderTextureFormat, bool> s_RenderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();
		static Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>> s_GraphicsFormatSupport = new Dictionary<GraphicsFormat, Dictionary<FormatUsage, bool>>();

		public static void ClearSystemInfoCache()
		{
			s_RenderTextureFormatSupport.Clear();
			s_GraphicsFormatSupport.Clear();
		}

		public static void SetViewAndProjectionMatrices(CommandBuffer cmd, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
		{
			Matrix4x4 viewAndProjectionMatrix = projectionMatrix * viewMatrix;
			cmd.SetGlobalMatrix(ShaderPropertyID.viewMatrix, viewMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyID.projectionMatrix, projectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyID.viewAndProjectionMatrix, viewAndProjectionMatrix);

			Matrix4x4 inverseViewMatrix			= Matrix4x4.Inverse(viewMatrix);
			Matrix4x4 inverseProjectionMatrix	= Matrix4x4.Inverse(projectionMatrix);
			Matrix4x4 inverseViewProjection		= inverseViewMatrix * inverseProjectionMatrix;

			cmd.SetGlobalMatrix(ShaderPropertyID.inverseViewMatrix, inverseViewMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyID.inverseProjectionMatrix, inverseProjectionMatrix);
			cmd.SetGlobalMatrix(ShaderPropertyID.inverseViewAndProjectionMatrix, inverseViewProjection);
		}

		public static void Blit(CommandBuffer cmd,
								RenderTargetIdentifier source,
								RenderTargetIdentifier destination,
								Material material,
								int passIndex = 0,
								bool useDrawProcedural = false,
								RenderBufferLoadAction colorLoadAction = RenderBufferLoadAction.Load,
								RenderBufferStoreAction colorStoreAction = RenderBufferStoreAction.Store,
								RenderBufferLoadAction depthLoadAction = RenderBufferLoadAction.Load,
								RenderBufferStoreAction depthStoreAction = RenderBufferStoreAction.Store)
		{
			cmd.SetGlobalTexture(ShaderPropertyID.sourceTex, source);
			//RenderTargetIdentifier cameraTarget = (renderingData.cameraData.targetTexture != null) ? new RenderTargetIdentifier(renderingData.cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
			//cmd.SetRenderTarget(cameraTarget);

			if (useDrawProcedural)
			{
				Vector4 scaleBias = new Vector4(1, 1, 0, 0);
				Vector4 scaleBiasRt = new Vector4(1, 1, 0, 0);
				cmd.SetGlobalVector(ShaderPropertyID.scaleBias, scaleBias);
				cmd.SetGlobalVector(ShaderPropertyID.scaleBiasRt, scaleBiasRt);
				cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1),
					colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
				cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Quads, 4, 1, null);
			}
			else
			{
				cmd.SetRenderTarget(destination, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
				cmd.Blit(source, BuiltinRenderTextureType.CurrentActive, material, passIndex);
			}
		}

		public static bool SupportsGraphicsFormat(GraphicsFormat format, FormatUsage usage)
		{
			bool support = false;
			if (!s_GraphicsFormatSupport.TryGetValue(format, out var uses))
			{
				uses = new Dictionary<FormatUsage, bool>();
				support = SystemInfo.IsFormatSupported(format, usage);
				uses.Add(usage, support);
				s_GraphicsFormatSupport.Add(format, uses);
			}
			else
			{
				if (!uses.TryGetValue(usage, out support))
				{
					support = SystemInfo.IsFormatSupported(format, usage);
					uses.Add(usage, support);
				}
			}

			return support;
		}

		public static bool IsGLRenderer()
		{
			return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore
								   || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2
								   || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;
		}

		public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
		{
			if (!s_RenderTextureFormatSupport.TryGetValue(format, out var support))
			{
				support = SystemInfo.SupportsRenderTextureFormat(format);
				s_RenderTextureFormatSupport.Add(format, support);
			}

			return support;
		}

		public static void CreateCameraRenderTarget(ScriptableRenderContext context, ref RenderingData renderingData, ref RenderTextureDescriptor descriptor, bool createColor, bool createDepth)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, createCameraRenderTarget))
			{
				if (createColor)
				{
					bool useDepthRenderBuffer = renderingData.curCameraDepthIdentifier == BuiltinRenderTextureType.CameraTarget;
					var colorDescriptor = descriptor;
					colorDescriptor.useMipMap = false;
					colorDescriptor.autoGenerateMips = false;
					colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? 32 : 0;
					cmd.GetTemporaryRT(RenderingData.ColorAttenmentID, colorDescriptor, FilterMode.Bilinear);
					renderingData.curCameraColorIdentifier = new RenderTargetIdentifier(RenderingData.ColorAttenmentID);
				}

				if (createDepth)
				{
					var depthDescriptor = descriptor;
					depthDescriptor.useMipMap = false;
					depthDescriptor.autoGenerateMips = false;
#if ENABLE_VR && ENABLE_XR_MODULE
                    // XRTODO: Enabled this line for non-XR pass? URP copy depth pass is already capable of handling MSAA.
                    depthDescriptor.bindMS = depthDescriptor.msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
#endif
					depthDescriptor.colorFormat = RenderTextureFormat.Depth;
					depthDescriptor.depthBufferBits = 32;
					cmd.GetTemporaryRT(RenderingData.DepthAttenmentID, depthDescriptor, FilterMode.Point);
					renderingData.curCameraDepthIdentifier = new RenderTargetIdentifier(RenderingData.DepthAttenmentID);
				}
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public static void ReleaseCreatedCameraRenderTarget(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			context.ExecuteCommandBuffer(cmd);

			if (renderingData.curCameraColorIdentifier != renderingData.orignCameraColorIdentifier)
			{
				cmd.ReleaseTemporaryRT(RenderingData.ColorAttenmentID);
			}

			if (renderingData.curCameraDepthIdentifier != renderingData.orignCameraDepthIdentifier)
			{
				cmd.ReleaseTemporaryRT(RenderingData.DepthAttenmentID);
			}

			CommandBufferPool.Release(cmd);

			renderingData.curCameraColorIdentifier = renderingData.orignCameraColorIdentifier;
			renderingData.curCameraDepthIdentifier = renderingData.orignCameraDepthIdentifier;
		}

		public static void SetRenderTarget( CommandBuffer cmd, ref RenderTargetIdentifier colorTargetId, ref RenderTargetIdentifier depthTargetId, ClearFlag clearFlag, Color clearColor)
		{
			RenderBufferLoadAction colorLoadAction = (clearFlag & ClearFlag.Color) != ClearFlag.None ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
			RenderBufferLoadAction depthLoadAction = (clearFlag & ClearFlag.Depth) != ClearFlag.None ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

			cmd.SetRenderTarget(colorTargetId, colorLoadAction, RenderBufferStoreAction.Store, 
								depthTargetId, depthLoadAction, RenderBufferStoreAction.Store);

			if (clearFlag != ClearFlag.None)
				cmd.ClearRenderTarget((clearFlag & ClearFlag.Depth) != 0, (clearFlag & ClearFlag.Color) != 0, clearColor);
		}
	
		public static int GetMaxTileResolutionInAtlas(int resolution, int CascadesCount)
		{
			int rt = resolution;

			if (CascadesCount <= 1)
				return rt;

			if (CascadesCount <= 4)
				return rt / 2;

			if (CascadesCount <= 9)
				return rt / 3;

			if (CascadesCount <= 16)
				return rt / 4;

			return rt;
		}

		public static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
		{
			// Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
			// apply z reversal to projection matrix. We need to do it manually here.
			if (SystemInfo.usesReversedZBuffer)
			{
				proj.m20 = -proj.m20;
				proj.m21 = -proj.m21;
				proj.m22 = -proj.m22;
				proj.m23 = -proj.m23;
			}

			Matrix4x4 worldToShadow = proj * view;

			var textureScaleAndBias = Matrix4x4.identity;
			textureScaleAndBias.m00 = 0.5f;
			textureScaleAndBias.m11 = 0.5f;
			textureScaleAndBias.m22 = 0.5f;
			textureScaleAndBias.m03 = 0.5f;
			textureScaleAndBias.m23 = 0.5f;
			textureScaleAndBias.m13 = 0.5f;

			// Apply texture scale and offset to save a MAD in shader.
			return textureScaleAndBias * worldToShadow;
		}

		public static void ApplySliceTransform(ref ShadowSliceData shadowSliceData, int atlasWidth, int atlasHeight)
		{
			Matrix4x4 sliceTransform = Matrix4x4.identity;
			float oneOverAtlasWidth = 1.0f / atlasWidth;
			float oneOverAtlasHeight = 1.0f / atlasHeight;
			sliceTransform.m00 = shadowSliceData.resolution * oneOverAtlasWidth;
			sliceTransform.m11 = shadowSliceData.resolution * oneOverAtlasHeight;
			sliceTransform.m03 = shadowSliceData.offsetX * oneOverAtlasWidth;
			sliceTransform.m13 = shadowSliceData.offsetY * oneOverAtlasHeight;

			// Apply shadow slice scale and offset
			shadowSliceData.shadowTransform = sliceTransform * shadowSliceData.shadowTransform;
		}

		public static bool ExtractDirectionalLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, int cascadeIndex,
					int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out Vector4 cascadeSplitDistance, out ShadowSliceData shadowSliceData, out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix)
		{
			ShadowSplitData splitData;

			bool success = cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
				cascadeIndex, shadowData.shadowCascadesCount, shadowData.shadowCascadesSplit, shadowResolution, shadowNearPlane, out viewMatrix, out projMatrix,
				out splitData);

			cascadeSplitDistance = splitData.cullingSphere;
			shadowSliceData.offsetX = (cascadeIndex % 2) * shadowResolution;
			shadowSliceData.offsetY = (cascadeIndex / 2) * shadowResolution;
			shadowSliceData.resolution = shadowResolution;
			shadowSliceData.viewMatrix = viewMatrix;
			shadowSliceData.projectionMatrix = projMatrix;
			shadowSliceData.shadowTransform = GetShadowTransform(projMatrix, viewMatrix);

			// If we have shadow cascades baked into the atlas we bake cascade transform
			// in each shadow matrix to save shader ALU and L/S
			if (shadowData.shadowCascadesCount > 1)
				ApplySliceTransform(ref shadowSliceData, shadowmapWidth, shadowmapHeight);

			return success;
		}

		public static Vector4 GetShadowBias(ref VisibleLight shadowLight, int shadowLightIndex, ref ShadowData shadowData, Matrix4x4 lightProjectionMatrix, float shadowResolution)
		{
			//if (shadowLightIndex < 0 || shadowLightIndex >= shadowData.bias.Count)
			//{
			//	Debug.LogWarning(string.Format("{0} is not a valid light index.", shadowLightIndex));
			//	return Vector4.zero;
			//}

			Vector4 bias = new Vector4(shadowLight.light.shadowBias, shadowLight.light.shadowNormalBias, 0.0f, 0.0f);

			float frustumSize;
			if (shadowLight.lightType == LightType.Directional)
			{
				// Frustum size is guaranteed to be a cube as we wrap shadow frustum around a sphere
				frustumSize = 2.0f / lightProjectionMatrix.m00;
			}
			else if (shadowLight.lightType == LightType.Spot)
			{
				// For perspective projections, shadow texel size varies with depth
				// It will only work well if done in receiver side in the pixel shader. Currently UniversalRP
				// do bias on caster side in vertex shader. When we add shader quality tiers we can properly
				// handle this. For now, as a poor approximation we do a constant bias and compute the size of
				// the frustum as if it was orthogonal considering the size at mid point between near and far planes.
				// Depending on how big the light range is, it will be good enough with some tweaks in bias
				frustumSize = Mathf.Tan(shadowLight.spotAngle * 0.5f * Mathf.Deg2Rad) * shadowLight.range;
			}
			else
			{
				Debug.LogWarning("Only spot and directional shadow casters are supported in universal pipeline");
				frustumSize = 0.0f;
			}

			// depth and normal bias scale is in shadowmap texel size in world space
			float texelSize = frustumSize / shadowResolution;
			//float depthBias = -shadowData.bias[shadowLightIndex].x * texelSize;
			//float normalBias = -shadowData.bias[shadowLightIndex].y * texelSize;

			float depthBias = -bias.x * texelSize;
			float normalBias = -bias.y * texelSize;

			if (shadowData.isUseSoftShadowMap)
			{
				// TODO: depth and normal bias assume sample is no more than 1 texel away from shadowmap
				// This is not true with PCF. Ideally we need to do either
				// cone base bias (based on distance to center sample)
				// or receiver place bias based on derivatives.
				// For now we scale it by the PCF kernel size (5x5)
				const float kernelRadius = 2.5f;
				depthBias *= kernelRadius;
				normalBias *= kernelRadius;
			}

			return new Vector4(depthBias, normalBias, 0.0f, 0.0f);
		}

	}
}