using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

//using Frameworks.Common;

namespace Frameworks.UIControl
{
	public class UIControl : UnityEngine.EventSystems.EventTrigger
	{
		public static List<UIControl> ButtonTable = new List<UIControl>();

		public enum KeyState
		{
			Normal,
			MoveOn,
			Press,
		};

		public KeyState CurState
		{
			set
			{
				if (mState == value)
					return;

				switch (value)
				{
					case KeyState.MoveOn:
						{
							if (mOnMoveOn != null)
								mOnMoveOn();
						}
						break;
					case KeyState.Press:
						{
							if (mOnPressDown != null)
								mOnPressDown();
						}
						break;
					case KeyState.Normal:
						{
							if (mOnMoveOff != null)
								mOnMoveOff();
						}
						break;
				}

				mState = value;
			}

			get
			{
				return mState;
			}
		}

		// This just use for quick get the position from button list in UIManager mix mode.
		public int RowInList = -1;
		public int ColInList = -1;

		int mDepth = 0;

		public int depth
		{
			get
			{
				return mDepth;
			}
		}

		KeyState mState = KeyState.Normal;
		
		public delegate void ButtonEventFunc();

		public ButtonEventFunc mOnMoveOn = null;
		public ButtonEventFunc mOnMoveOff = null;
		public ButtonEventFunc mOnPressDown = null;
		public ButtonEventFunc mOnButtonUp = null;

		[FormerlySerializedAs("onClick")]
		public UnityEvent mOnClick = new UnityEvent();

		public bool IsInEnter
		{
			get
			{
				return mIsInEnter;
			}
		}
		public bool IsInPress
		{
			get
			{
				return mIsInPress;
			}
		}

		bool mIsInEnter = false;
		bool mIsInPress = false;

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			//Log.Print(LogLevel.Info, "Do Click");
			DoClick();
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (!gameObject.activeSelf)
				return;

			//Log.Print(LogLevel.Info, "OnPointerDown");
			mIsInPress = true;
			if (mOnPressDown != null)
				mOnPressDown();
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (!gameObject.activeSelf)
				return;

			//Log.Print(LogLevel.Info, "OnPointerEnter");
			mIsInEnter = true;
			if (mOnMoveOn != null) 
				mOnMoveOn();
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!gameObject.activeSelf)
				return;

			//Log.Print(LogLevel.Info, "OnPointerExit");
			mIsInEnter = false;
			if (mOnMoveOff != null) 
				mOnMoveOff();
		}
		public override void OnPointerUp(PointerEventData eventData)
		{
			if (!gameObject.activeSelf)
				return;

			//Log.Print(LogLevel.Info, "OnPointerUp");
			mIsInPress = false;
			if (mOnButtonUp != null)
				mOnButtonUp();				
		}

// 		public override void OnSelect(BaseEventData eventData)
// 		{
// 			Log.Print(LogLevel.Info, "OnSelect");
// 			if (mOnPressDown != null) 
// 				mOnPressDown();
// 		}

		public void DoClick()
		{
			if (mOnClick == null)
				return;

			mOnClick.Invoke();
		}

		void Awake()
		{
			ButtonTable.Add(this);
			 
			MaskableGraphic grap = GetComponent<MaskableGraphic>();
			if (grap != null)
			{
				mDepth = grap.depth;
			}
		}

		void OnDestroy()
		{
			ButtonTable.Remove(this);
		}
	}
}
