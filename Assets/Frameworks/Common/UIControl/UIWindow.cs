using UnityEngine;
using System.Collections.Generic;
using Frameworks.Asset;
using Frameworks.Common;

namespace Frameworks.UIControl
{
	[RequireComponent(typeof(CanvasGroup))]
	public class UIWindow : MonoBehaviour
	{
		public delegate void OnWindowCreated(UIWindow window);

		static Dictionary<string, UIWindow> WindowList = new Dictionary<string, UIWindow>();

		public static UIWindow GetWindow(string prefabPath)
		{
			UIWindow window = null;
			WindowList.TryGetValue(prefabPath, out window);
			return window;
		}

		public static void Create( string prefabPath, Transform parentNode = null, bool isResetPosition = true, OnWindowCreated onCreatedCallback = null)
		{
			var window = GetWindow(prefabPath);
			if (window != null)
			{
				Log.Print(LogLevel.Warning, "{0} is already exist!", prefabPath);
				return;
			}

			AssetManager.Instance.LoadAsset( new AssetRef(prefabPath, typeof(GameObject)), (assetRef, bundleData) =>
			{
				GameObject prefab = assetRef.AssetObject as GameObject;

				if (prefab == null)
				{
					Debug.LogErrorFormat("UIWindow.Create Error! {0} loaded failed!", prefabPath);
					return;
				}

				GameObject obj = Instantiate<GameObject>(prefab);

				UIWindow window = obj.GetComponent<UIWindow>();

				if (window == null)
				{
					Debug.LogErrorFormat("UIWindow.Create Error! {0} do not have UIWindow component! destroy it!", prefabPath);
					Destroy(obj);
					return;
				}

				if (parentNode != null)
				{
					window.transform.SetParent(parentNode, !isResetPosition);
				}

				if (isResetPosition)
				{
					window.transform.localPosition = Vector3.zero;
				}

				WindowList.Add(prefabPath, window);

				window.OnCreate(prefabPath);

				onCreatedCallback?.Invoke(window);
			});
			
		}

		public static void CloseWindow(string prefabPath)
		{
			var window = GetWindow(prefabPath);
			if (window == null)
				return;

			Destroy(window.gameObject);

			WindowList.Remove(prefabPath);
		}

		public string PrefabPath = "";

		[Header("UI Bind Object")]
		public ButtonList RootBtnList = null;

		protected static List <ButtonList> mButtonListStack = new List<ButtonList>();

		public virtual void InitButtonList()
		{
			//mButtonListStack.Clear();

			if (RootBtnList != null)
			{
				UIKeyActionManager.Instance.SetButtonList(RootBtnList);
				mButtonListStack.Add(RootBtnList);
			}
		}

		protected virtual void OnCreate(string prefabPath)
		{
			PrefabPath = prefabPath;
			InitButtonList();
		}

		protected virtual void OnDestroy()
		{
			if (RootBtnList != null && mButtonListStack.Count > 0)
			{
				mButtonListStack.Remove(RootBtnList);
			}
		}
	}
}
