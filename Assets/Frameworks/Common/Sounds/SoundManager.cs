using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common
{
	class SoundManager : MonoBehaviour
	{
		static SoundManager msSoundManager;

		public UnityEngine.Object soundObjPrefab = null;

		public static SoundManager instance
		{
			get
			{
				return msSoundManager;
			}
		}

		public static SoundManager Create()
		{
			GameObject obj = new GameObject("SoundManager");

			obj.AddComponent<SoundManager>();

			return SoundManager.instance;
		}

		public static SoundObj PlaySound(AudioClip clip, Vector3 position)
		{
			if (SoundManager.instance == null)
			{
				SoundManager.Create();
			}

			SoundObj soundObj =  SoundManager.instance.GetPool().DoCreateOrReuseObj();

			soundObj.transform.position = position;

			soundObj.source.clip = clip;
			soundObj.source.Play();

			return soundObj;
		}

		public static void PlaySound(AudioClip clip, GameObject obj)
		{
			if (SoundManager.instance == null)
			{
				SoundManager.Create();
			}

			SoundObj soundObj = SoundManager.instance.GetPool().DoCreateOrReuseObj();

			soundObj.followingObj = obj;

			soundObj.source.clip = clip;
			soundObj.source.Play();
		}


		RecyclePool<SoundObj> mSoundObjPool;

		void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			mSoundObjPool = new RecyclePool<SoundObj>();

			if (soundObjPrefab == null)
			{
				mSoundObjPool.SetNewObjFunc(NewSoundObj);
			}
			else
			{
				mSoundObjPool.SetNewObjFunc(CloneSoundObj);
			}

			mSoundObjPool.SetOnResetObjFunc(OnResetObj);

			msSoundManager = this;
		}

		public RecyclePool<SoundObj> GetPool()
		{
			return mSoundObjPool;
		}

		SoundObj NewSoundObj()
		{
			GameObject obj = new GameObject("SoundObj");

			return obj.AddComponent<SoundObj>();
		}

		SoundObj CloneSoundObj()
		{
			GameObject obj = GameObject.Instantiate(soundObjPrefab) as GameObject;

			return obj.GetComponent<SoundObj>();
		}

		void OnResetObj(SoundObj obj)
		{
			obj.followingObj = null;
		}


	}
}
