using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Frameworks
{
	public class AnimatiorControllerGenerator
	{
		const string _sUpporSpineBoneName = "Bip001 Spine1";
		const string _sIdleAnimationName = "Idle";

		public static AvatarMask GeneratorAvatarMask(ModelImporter modelImporter, string avatarMaskAssetPath, string spineBoneName = _sUpporSpineBoneName)
		{
			if (modelImporter == null)
				return null;

			if (string.IsNullOrEmpty(avatarMaskAssetPath))
				return null;

			AvatarMask avatarMask = new AvatarMask();

			var refTransformsPath = modelImporter.transformPaths;
			
			avatarMask.transformCount = refTransformsPath.Length;

			for (int i = 0; i < refTransformsPath.Length; i++)
			{
				avatarMask.SetTransformPath(i, refTransformsPath[i]);
				avatarMask.SetTransformActive(i, refTransformsPath[i].Contains(spineBoneName));
			}

			AssetDatabase.CreateAsset(avatarMask, avatarMaskAssetPath);

			return avatarMask;
		}

		public static void GeneratorAnimatorController(List<Motion> motionClips, string controllerAssetPath, AvatarMask skillLayerMask)
		{
			if (string.IsNullOrEmpty(controllerAssetPath))
				return;

			if (motionClips == null || motionClips.Count == 0)
				return;

			//var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerAssetPath);


			AnimatorController controller = new AnimatorController();
			controller.name = System.IO.Path.GetFileName(controllerAssetPath);

			ReflectTools.InvokeMethod(controller, "set_pushUndo", false);

			controller.AddLayer("Base Layer");

			var baseLayer = controller.layers[0];
			var skillLayer = new AnimatorControllerLayer();

			skillLayer.name = controller.MakeUniqueLayerName("SkillLayer");
			skillLayer.stateMachine = new AnimatorStateMachine();
			skillLayer.stateMachine.name = skillLayer.name;
			skillLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
			
			var sm = baseLayer.stateMachine;

			List<Object> storeObjs = new List<Object>();

			AnimatorState state = null;
			for (int i = 0; i < motionClips.Count; ++i)
			{
				var clip = motionClips[i];
				state = sm.AddState(clip.name);

				if (clip.name.CompareTo(_sIdleAnimationName) == 0)
				{
					sm.defaultState = state;
				}

				state.motion = clip;

				storeObjs.Add(state);
			}

			sm = skillLayer.stateMachine;

			skillLayer.avatarMask	 = skillLayerMask;
			skillLayer.defaultWeight = 1.0f;

			state = sm.AddState("empty");
			sm.defaultState = state;
			storeObjs.Add(state);

			for (int i = 0; i < motionClips.Count; ++i)
			{
				var clip = motionClips[i];

				if (!clip.name.StartsWith("skill"))
					continue;
				state = sm.AddState(clip.name);
				state.motion = clip;

				storeObjs.Add(state);
			}

			controller.AddLayer(skillLayer);

			ReflectTools.InvokeMethod(controller, "set_pushUndo", true);
			//ReflectTools.SetObjectField(controller, "pushUndo", true);

			AssetDatabase.CreateAsset(controller, controllerAssetPath);

			string controllerPath = AssetDatabase.GetAssetPath(controller);

			if (controllerPath != "")
			{
				for (int i = 0; i < storeObjs.Count; ++i)
				{
					AssetDatabase.AddObjectToAsset(storeObjs[i], controllerPath);
				}

				AssetDatabase.AddObjectToAsset(baseLayer.stateMachine,  controllerPath);
				AssetDatabase.AddObjectToAsset(skillLayer.stateMachine, controllerPath);
			}
		}
	}
}
