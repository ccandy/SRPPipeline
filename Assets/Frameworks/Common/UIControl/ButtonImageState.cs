using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frameworks.UIControl
{
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(UIControl))]
	public class ButtonImageState : MonoBehaviour
	{
		public UIControl mButton;

		public Image	mImage;

		void Awake()
		{
			if (mButton == null)
				mButton = GetComponent<UIControl>();

			if (mImage == null)
				mImage = GetComponent<Image>();

			if (mImage != null)
			{
				mButton.mOnMoveOn += OnMoveOn;
				mButton.mOnMoveOff += OnMoveOff;
				mButton.mOnPressDown += OnPress;
				mButton.mOnButtonUp += OnButtonUp;
			}

		}

		void Start()
		{
			switch (mButton.CurState)
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

		public Sprite mNormalSprite = null;
		public Sprite mPressSprite = null;
		public Sprite mMoveOnSprite = null;

		public Color mNormalColor = Color.white;
		public Color mPressColor = Color.white*0.75f;
		public Color mMoveOnColor = Color.white;

		public void OnMoveOn()
		{
			if (mButton.IsInPress)
			{
				if (mPressSprite != null)
					mImage.sprite = mPressSprite;
				mImage.color = mPressColor;
			}
			else
			{
				if (mMoveOnSprite != null)
					mImage.sprite = mMoveOnSprite;
				mImage.color = mMoveOnColor;
			}
			
		}

		public void OnMoveOff()
		{
			if (mNormalSprite != null)
				mImage.sprite = mNormalSprite;
			mImage.color = mNormalColor;
		}

		public void OnPress()
		{
			if (mPressSprite != null)
				mImage.sprite	= mPressSprite;
			mImage.color	= mPressColor;
		}

		public void OnButtonUp()
		{
			if (mButton.IsInEnter)
			{
				if (mMoveOnSprite != null)
					mImage.sprite = mMoveOnSprite;
				mImage.color = mMoveOnColor;
			}
			else
			{
				if (mNormalSprite != null)
					mImage.sprite = mNormalSprite;
				mImage.color = mNormalColor;
			}
		}
	}
}
