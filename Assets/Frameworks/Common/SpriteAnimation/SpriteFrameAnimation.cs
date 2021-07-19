using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Frameworks.Common
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteFrameAnimation : CurveAdapt
	{
		SpriteRenderer mRender;

		public UnityEngine.U2D.SpriteAtlas atlas = null;

		public string[] FramesName;

		Sprite[] sprite;

		int CurKey = 0;

		void Awake()
		{
			mRender = GetComponent<SpriteRenderer>();

			sprite = new Sprite[FramesName.Length];
			for (int i = 0; i < FramesName.Length; ++i)
			{
				sprite[i] = atlas.GetSprite(FramesName[i]);
			}
		}

		public override void DoUpdate(float CurValue)
		{
			if (FramesName == null || FramesName.Length == 0)
				return;

			CurKey = Mathf.FloorToInt(CurValue * FramesName.Length);

			if (CurKey < 0 || CurKey >= FramesName.Length)
			{
				return;
			}
				

			mRender.sprite = sprite[CurKey];
		}
	}
}
