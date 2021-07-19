using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.CRP
{
	public static class CameraExtensions
	{
		/// <summary>
		/// Universal Render Pipeline exposes additional rendering data in a separate component.
		/// This method returns the additional data component for the given camera or create one if it doesn't exists yet.
		/// </summary>
		/// <param name="camera"></param>
		/// <returns>The <c>UniversalAdditinalCameraData</c> for this camera.</returns>
		/// <see cref="UniversalAdditionalCameraData"/>
		public static PipelineAdditionalCameraData GetPipelineAdditionalCameraData(this Camera camera)
		{
			var gameObject = camera.gameObject;
			bool componentExists = gameObject.TryGetComponent<PipelineAdditionalCameraData>(out var cameraData);
			if (!componentExists)
			{
				cameraData = gameObject.AddComponent<PipelineAdditionalCameraData>();
			}

			return cameraData;
		}
	}

	[RequireComponent(typeof(Camera))]
	public class PipelineAdditionalCameraData : MonoBehaviour
	{
		public bool IsOverlayCamera = false;

		/// <summary>
		/// This params only effect when is overlay camera.
		/// </summary>
		public bool IsOverlayClearDepth	= true;

		public List<Camera> StackCameras = new List<Camera>();

		public int RenderPassBlockIndex = 0; 
	}
}
