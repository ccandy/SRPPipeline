using Frameworks.Common;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Frameworks.UIControl
{
	public class UIKeyActionManager : SingletonMonoBehaviour<UIKeyActionManager>
	{
		ButtonList mRefButtonList = null;

		public int selectRow = 0;
		public int selectCol = 0;

		public bool isUseBtnDownOnly = false;

		const string Vertical = "Vertical";
		const string Horizontal = "Horizontal";

		float m_CurVertical 	= 0.0f;
     	float m_CurHorizontal	= 0.0f;

		float m_LastVertical 	= 0.0f;
     	float m_LastHorizontal 	= 0.0f;

		public void SetButtonList(ButtonList list)
		{
			if (mRefButtonList == list)
				return;

			mRefButtonList = list;
			if (mRefButtonList == null)
				return;

			selectRow = mRefButtonList.startRow;
			selectCol = mRefButtonList.startCol;

			for ( int row = 0; row < mRefButtonList.Table.Length; ++row)
			{
				UIButtonRowData rowData = mRefButtonList.Table[row];
				if (rowData == null)
					continue;

				for (int col = 0; col < rowData.ColData.Length; ++col)
				{
					rowData.ColData[col].CurState = UIControl.KeyState.Normal;
				}
			}

			UIControl btn = mRefButtonList.GetButton(selectRow, selectCol);

			if (btn != null)
				btn.CurState = UIControl.KeyState.MoveOn;
		}

		public static bool IsEnterDown()
		{
			return Input.GetKeyDown(KeyCode.Joystick1Button0)
				|| Input.GetKeyDown(KeyCode.Joystick1Button1)
				|| Input.GetKeyDown(KeyCode.Joystick2Button0)
				|| Input.GetKeyDown(KeyCode.Joystick2Button1)
				|| Input.GetKeyDown(KeyCode.KeypadEnter)
				|| Input.GetKeyDown((KeyCode)10)
				|| Input.GetKeyDown(KeyCode.Return);
		}

		public static bool IsEnterUp()
		{
			return Input.GetKeyUp(KeyCode.Joystick1Button0)
				|| Input.GetKeyUp(KeyCode.Joystick1Button1)
				|| Input.GetKeyUp(KeyCode.Joystick2Button0)
				|| Input.GetKeyUp(KeyCode.Joystick2Button1)
				|| Input.GetKeyUp(KeyCode.KeypadEnter)
				|| Input.GetKeyUp((KeyCode)10)
				|| Input.GetKeyUp(KeyCode.Return);
		}

		void KeyboardUpdate()
		{
			if (mRefButtonList == null)
				return;

			if (mRefButtonList.Table.Length <= 0)
				return;

			int newSelectRow = selectRow;
			int newSelectCol = selectCol;

			bool IsIncreaseingRow = true;
			bool IsIncreaseingCol = true;

			if (Mathf.Abs(m_CurVertical - m_LastVertical) > float.Epsilon)
			{
				if (m_CurVertical > 0f)
				{
					--newSelectRow;
					IsIncreaseingRow = false;
				}
				else if (m_CurVertical < 0f)
				{
					++newSelectRow;
				}

				newSelectRow = (newSelectRow + mRefButtonList.Table.Length) % mRefButtonList.Table.Length;
			}

			if (mRefButtonList[newSelectRow] == null)
				return;

			if ( Mathf.Abs(m_CurHorizontal - m_LastHorizontal) > float.Epsilon)
			{
				if (m_CurHorizontal > 0f)
				{
					IsIncreaseingCol = false;
					--newSelectCol;
				}
				else if (m_CurHorizontal < 0f)
				{
					++newSelectCol;
				}

				newSelectCol = (newSelectCol + mRefButtonList[newSelectRow].ColData.Length) % mRefButtonList[newSelectRow].ColData.Length;
			}

			UIControl btn = mRefButtonList.GetButton(newSelectRow, newSelectCol);

			int GetNextRowCount = 0;
			int GetNextColCount = 0;

			// When the select control is unactive, find the new control.
			while (btn == null || !btn.gameObject.activeSelf)
			{
				if (IsIncreaseingCol)
					newSelectCol = (newSelectCol + 1 + mRefButtonList[newSelectRow].ColData.Length) % mRefButtonList[newSelectRow].ColData.Length;
				else
					newSelectCol = (newSelectCol - 1 + mRefButtonList[newSelectRow].ColData.Length) % mRefButtonList[newSelectRow].ColData.Length;

				++GetNextColCount;
				if (GetNextColCount >= mRefButtonList[newSelectRow].ColData.Length)
				{
					GetNextColCount = 0;
					++GetNextRowCount;
					if (GetNextRowCount >= mRefButtonList.Table.Length)
						return;
					
					if (IsIncreaseingRow)
						newSelectRow = (newSelectRow + 1 + mRefButtonList.Table.Length) % mRefButtonList.Table.Length;
					else
						newSelectRow = (newSelectRow - 1 + mRefButtonList.Table.Length) % mRefButtonList.Table.Length;
				}

				btn = mRefButtonList.GetButton(newSelectRow, newSelectCol);
			}
			

			if (newSelectRow != selectRow || newSelectCol != selectCol)
			{
				btn = mRefButtonList.GetButton(selectRow, selectCol);
				if (btn != null)
				{
					btn.CurState = UIControl.KeyState.Normal;
				}

				selectRow = newSelectRow;
				selectCol = newSelectCol;

				btn = mRefButtonList.GetButton(selectRow, selectCol);
				if (btn != null)
				{
					btn.CurState = UIControl.KeyState.MoveOn;
				}
			}

			btn = mRefButtonList.GetButton(selectRow, selectCol);
			if (btn == null)
			{
				return;
			}

			if (isUseBtnDownOnly)
			{
				if (IsEnterDown())
				{
					btn.CurState = UIControl.KeyState.Press;
					btn.DoClick();
					btn.CurState = UIControl.KeyState.MoveOn;
				}
			}
			else
			{
				if (IsEnterDown())
				{
					btn.CurState = UIControl.KeyState.Press;
				}
				else if (IsEnterUp())
				{
					btn.DoClick();
					btn.CurState = UIControl.KeyState.MoveOn;
				}
			}

		}

		void Update()
		{
			m_CurVertical 		= Input.GetAxisRaw(Vertical);
			m_CurHorizontal 	= Input.GetAxisRaw(Horizontal);

			KeyboardUpdate();

			m_LastVertical 		= m_CurVertical;
			m_LastHorizontal	= m_CurHorizontal;
		}

		void OnDestroy()
		{ 
		}
	}
}
