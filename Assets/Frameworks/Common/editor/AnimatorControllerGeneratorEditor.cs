using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Frameworks
{
	public class AnimatorControllerGeneratorEditor : EditorWindowBase<AnimatorControllerGeneratorEditor>
	{
		static List<Motion> motionClips = new List<Motion>();

		ModelImporter mainModelImport				= null;

		bool		  IsCreateAvatarMask			= true;
		Avatar		  mainAvatar					= null;

		EditorFilePath maskPath						= new EditorFilePath();
		EditorFilePath controllerPath				= new EditorFilePath();

		[MenuItem("Assets/Animator/AvatarMaskGenerator", false, 10)]
		static void GeneratorAvatarMask()
		{
			//string path = AssetDatabase.GetAssetPath(Selection.activeObject);

			//GUIUtility.systemCopyBuffer = path;

			var obj = Selection.activeObject;

			string path = AssetDatabase.GetAssetPath(obj);

			ModelImporter modelImporter  = AssetImporter.GetAtPath(path) as ModelImporter;

			if (modelImporter == null)
				return;

			string maskPath = path.Substring(0, path.LastIndexOf('.')) + "_mask.mask";

			AnimatiorControllerGenerator.GeneratorAvatarMask(modelImporter, maskPath);
			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/Animator/AnimatorControllerGenerator", false, 10)]
		static void GeneratorAnimationController()
		{
			//string path = AssetDatabase.GetAssetPath(Selection.activeObject);

			//GUIUtility.systemCopyBuffer = path;

			var objects = Selection.objects;

			motionClips.Clear();

			ModelImporter mainModelImport	= null;
			Avatar		  mainAvatar		= null;

			string		  controllerPath	= "Assets/_controller.controller";
			string		  maskPath			= "Assets/_mask.mask";

			for (int i = 0; i < objects.Length; ++i)
			{
				string path = AssetDatabase.GetAssetPath(objects[i]);

				bool isMainAvatar = false;
				var modelImport = AssetImporter.GetAtPath(path) as ModelImporter;

				if (modelImport != null)
				{
					if (modelImport.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel)
					{
						mainModelImport = modelImport;
						isMainAvatar = true;

						controllerPath = path.Substring(0, path.LastIndexOf('.')) + "_controller.controller";
						maskPath	   = path.Substring(0, path.LastIndexOf('.')) + "_mask.mask";
					}
				}

				var assets = AssetDatabase.LoadAllAssetsAtPath(path);

				if (assets.Length == 0)
					continue;

				for (int j = 0; j < assets.Length; ++j)
				{
					if (assets[j] is Motion)
					{
						var clip = assets[j] as Motion;

						if (clip.name.StartsWith("__preview"))
							continue;

						motionClips.Add(clip);
					}
					else if (assets[j] is Avatar && isMainAvatar)
					{
						mainAvatar = assets[j] as Avatar;
					}
				}
			}

			if (motionClips.Count == 0)
				return;

			Open("AnimationControllerGenerator", new Vector2(600, 300));

			Instance.mainModelImport	= mainModelImport;
			Instance.mainAvatar			= mainAvatar;
			Instance.controllerPath.SettingFromAssetPath(controllerPath);
			Instance.maskPath.SettingFromAssetPath(maskPath);			
		}

		private Vector2 m_ScrollPosition;

		void OnGUI()
		{
			if (motionClips.Count == 0)
			{
				Close();
				return;
			}

			IsCreateAvatarMask = GUILayout.Toggle( IsCreateAvatarMask, "Is Create Avatar Mask:");

			if (IsCreateAvatarMask)
			{
				if (mainModelImport != null)
				{
					EditorGUILayout.ObjectField("Avatar Mask From Avatar:", mainAvatar, typeof(Avatar), true);
					maskPath.OnGUI("Avatar Mask Path", "mask", "...", "", true, "avatarMask");
				}
			}

			GUILayout.Label("Import Motion List:");
			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

			for (int i = 0; i < motionClips.Count; ++i)
			{
				var motion = motionClips[i];

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.ObjectField(motion.name, motion, typeof(Motion), true);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			controllerPath.OnGUI("Controller Path", "controller", "...", "", true, "animatiorController");

			if (GUILayout.Button("Create") && !string.IsNullOrEmpty(controllerPath.absotionPath) && controllerPath.isAssetPath)
			{
				AvatarMask skillAvatarMask = null;

				if (IsCreateAvatarMask && maskPath.isAssetPath)
				{
					skillAvatarMask = AnimatiorControllerGenerator.GeneratorAvatarMask(mainModelImport, maskPath.assetPath);
					AssetDatabase.Refresh();
				}

				AnimatiorControllerGenerator.GeneratorAnimatorController(motionClips, controllerPath.assetPath, skillAvatarMask);
				AssetDatabase.Refresh();
			}
		}
	}
}
