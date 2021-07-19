using UnityEngine;

namespace Frameworks.Common
{
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T mInstance = null;

		public static T Instance
		{
			get
			{
				return mInstance;
			}
		}

		public static T Create()
		{
			if (mInstance != null)
				return mInstance;

			GameObject obj = new GameObject(typeof(T).ToString());
			mInstance = obj.AddComponent<T>();
			return mInstance;
		}

		protected virtual void Awake()
		{
			if (mInstance != null)
			{
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);
		}
	}
}
