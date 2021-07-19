using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundObj : MonoBehaviour
	{
		AudioSource mAudioSource;

		public AudioSource source
		{
			get
			{
				return mAudioSource;
			}
		}

		public GameObject followingObj = null;

		void Awake()
		{
			mAudioSource = GetComponent<AudioSource>();
		}

		void Update()
		{
			if (followingObj != null)
			{
				transform.position = followingObj.transform.position;
			}

			if (mAudioSource.isPlaying)
			{
				return;
			}

			if (SoundManager.instance == null)
			{
				return;
			}

			SoundManager.instance.GetPool().DoRecycle(this);
		}
	}
}
