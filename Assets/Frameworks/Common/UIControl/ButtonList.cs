using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.UIControl
{
	[System.Serializable]
	public class UIButtonRowData
	{
		public UIControl[] ColData;

		public UIControl this[int col]
		{

			get
			{
				if (col >= ColData.Length)
					return null;
				return ColData[col];
			}

			set
			{
				if (col >= ColData.Length)
					return;

				ColData[col] = value;
			}
		}
	}

	public class ButtonList : MonoBehaviour
	{
		public UIButtonRowData[] Table;

		public int startRow = 0;
		public int startCol = 0;

		public bool IsActiveInCreate = true;

		public UIControl GetButton(int row, int col)
		{
			if (row >= Table.Length)
				return null;

			return Table[row][col];
		}

		public void SetCurListToMainControl()
		{
			UIKeyActionManager uiManager = UIKeyActionManager.Create();
			uiManager.SetButtonList(this);
		}

		public void ClearMainControl()
		{
			UIKeyActionManager uiManager = UIKeyActionManager.Create();
			uiManager.SetButtonList(null);
		}

		public UIButtonRowData this[int row]
		{

			get
			{
				if (row >= Table.Length)
					return null;
				return Table[row];
			}

			set
			{
				if (row >= Table.Length)
					return;
				
				Table[row] = value;
			}
		}

		void Awake()
		{

		}

		void Start()
		{
			if (IsActiveInCreate)
			{
				SetCurListToMainControl();
			}
		}
	}
}
