using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frameworks.UIControl
{
	[RequireComponent(typeof(RawImage))]
	[RequireComponent(typeof(UIControl))]
	public class ButtonRawImageState : MonoBehaviour
	{
		UIControl mButton;

		RawImage mRawImage = null;

		void Awake()
		{
			mButton = GetComponent<UIControl>();

			mRawImage = GetComponent<RawImage>();
			if (mRawImage != null)
			{
				mButton.mOnMoveOn		+= OnMoveOn;
				mButton.mOnMoveOff		+= OnMoveOff;
				mButton.mOnPressDown	+= OnPress;
				mButton.mOnButtonUp		+= OnButtonUp;
			}
		}

		void Start()
		{ 
			switch(mButton.CurState)
			{
				case UIControl.KeyState.Normal:
					OnMoveOff();
					break;
				case UIControl.KeyState.MoveOn:
					OnMoveOn();
					break;
				case UIControl.KeyState.Press:
					OnPress();
					break;
			}
		}

		public Texture mNormalTexture = null;
		public Texture mPressTexture = null;
		public Texture mMoveOnTexture = null;

		public Color mNormalColor = Color.white;
		public Color mPressColor = Color.white*0.75f;
		public Color mMoveOnColor = Color.white;

		public void OnMoveOn()
		{
			if (mMoveOnTexture != null)
				mRawImage.texture = mMoveOnTexture;
			mRawImage.color = mMoveOnColor;
		}

		public void OnMoveOff()
		{
			if (mNormalTexture != null)
				mRawImage.texture = mNormalTexture;
			mRawImage.color = mNormalColor;
		}

		public void OnPress()
		{
			if (mPressTexture != null)
				mRawImage.texture = mPressTexture;
			mRawImage.color = mPressColor;
		}

		public void OnButtonUp()
		{

		}
	}
}
