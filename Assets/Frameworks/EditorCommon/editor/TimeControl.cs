using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks
{
	public static class TimeControl
	{
		static readonly int s_TimeRulerHash		= "TimeRuler".GetHashCode();
		static readonly int s_TimeTrackerHash	= "TimeTracker".GetHashCode();

		static int lastTrackerBtnType = 0; //0: Unselect, 1: begin, 2: end

		public static float trackerHeight = 35.0f;

		public static bool Ruler( float maxTime, ref float cur, int FPS = 30)
		{
			float height = trackerHeight;

			var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
			int controlID = GUIUtility.GetControlID(s_TimeRulerHash, FocusType.Keyboard, rect);

			float percent = cur / maxTime;

			bool isChange = false;
			switch (Event.current.type)
			{
				case EventType.Repaint:
					{
						var s_HandleWireMaterial = ReflectTools.GetStaticField<Material>(typeof(HandleUtility), "s_HandleWireMaterial");

						if (s_HandleWireMaterial != null)
						{
							//ReflectTools.InvokeStaticMethod(typeof(HandleUtility), "ApplyWireMaterial");
							s_HandleWireMaterial.SetPass(0);
						}

						#region Background
						GL.Begin(GL.QUADS);
						GL.Color(Color.black);
						GL.Vertex(rect.min);
						GL.Vertex(new Vector2(rect.xMax, rect.yMin));
						GL.Vertex(rect.max);
						GL.Vertex(new Vector2(rect.xMin, rect.yMax));
						GL.End();
						#endregion

						GL.Color(Color.white);
						GL.Begin(GL.LINES);

						GL.Vertex3(rect.x, rect.y, 0f);
						GL.Vertex3(rect.x + rect.width, rect.y, 0f);
						GL.Vertex3(rect.x, rect.y + rect.height, 0f);
						GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0f);

						float totalSteps = maxTime * FPS;

						for (int i = 0; i <= totalSteps; i++)
						{
							if ((i % 10) == 0)
							{
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y, 0f);
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y + 15f, 0f);
							}
							else if ((i % 5) == 0)
							{
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y, 0f);
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y + 10f, 0f);
							}
							else
							{
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y, 0f);
								GL.Vertex3(rect.x + ((rect.width * i) / totalSteps), rect.y + 5f, 0f);
							}
						}

						GL.Color(Color.red);

						GL.Vertex3(rect.x + (rect.width * percent), rect.y, 0f);
						GL.Vertex3(rect.x + (rect.width * percent), rect.y + rect.height, 0f);

						GL.End();
					}
					break;
				case EventType.MouseDown:
					if (rect.Contains(Event.current.mousePosition))
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();

						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						percent = (dragPosition - rect.x) / rect.width;
						Event.current.Use();

						cur = percent * maxTime;
						isChange = true;
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						percent = (dragPosition - rect.x) / rect.width;
						Event.current.Use();

						cur = percent * maxTime;
						isChange = true;
					}
					break;
			}
			
			return isChange;
		}

		public static void Tracker( float maxTime, ref float begin, ref float end, int FPS = 30)
		{
			float height = trackerHeight;
			var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);

			int controlID = GUIUtility.GetControlID(s_TimeTrackerHash, FocusType.Keyboard, rect);

			float beginPer	= begin / maxTime;
			float endPer	= end   / maxTime;

			float btnWidth  = 3.0f;

			var beginRect	= new Rect( new Vector2(rect.x + beginPer * rect.width - btnWidth * 0.5f, rect.y) , new Vector2(btnWidth, height));

			var endRect		= new Rect( new Vector2(rect.x + endPer * rect.width - btnWidth *0.5f, rect.y), new Vector2(btnWidth, height));

			Color bgColor = Color.white * 0.15f;

			Color trackColor = Color.yellow * 0.75f;

			Color beginBtnColor = Color.green;
			Color endBtnColor	= Color.red;

			switch (Event.current.type)
			{
				case EventType.Repaint:
					{
						var s_HandleWireMaterial = ReflectTools.GetStaticField<Material>(typeof(HandleUtility), "s_HandleWireMaterial");

						if (s_HandleWireMaterial != null)
						{
							s_HandleWireMaterial.SetPass(0);
						}

						#region Background
						GL.Begin(GL.QUADS);
						GL.Color(bgColor);
						GL.Vertex(rect.min);
						GL.Vertex(new Vector2(rect.xMax, rect.yMin));
						GL.Vertex(rect.max);
						GL.Vertex(new Vector2(rect.xMin, rect.yMax));
						GL.End();
						#endregion

						#region Track
						GL.Begin(GL.QUADS);
						GL.Color(trackColor);
						GL.Vertex(new Vector2(rect.xMin + rect.width * beginPer, rect.yMin));
						GL.Vertex(new Vector2(rect.xMin + rect.width * endPer, rect.yMin));
						GL.Vertex(new Vector2(rect.xMin + rect.width * endPer, rect.yMax));
						GL.Vertex(new Vector2(rect.xMin + rect.width * beginPer, rect.yMax));
						GL.End();
						#endregion

						GL.Begin(GL.QUADS);
						GL.Color(beginBtnColor);
						GL.Vertex(beginRect.min);
						GL.Vertex(new Vector2(beginRect.xMax, beginRect.yMin));
						GL.Vertex(beginRect.max);
						GL.Vertex(new Vector2(beginRect.xMin, beginRect.yMax));

						GL.Color(endBtnColor);
						GL.Vertex(endRect.min);
						GL.Vertex(new Vector2(endRect.xMax, endRect.yMin));
						GL.Vertex(endRect.max);
						GL.Vertex(new Vector2(endRect.xMin, endRect.yMax));
						GL.End();
					}
					break;
				case EventType.MouseDown:
					if (rect.Contains(Event.current.mousePosition))
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();

						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						float percent = (dragPosition - rect.x) / rect.width;

						if (percent > endPer)
						{		
							lastTrackerBtnType = 2;
						}
						else if (percent < beginPer)
						{
							lastTrackerBtnType = 1;
						}
						else
						{
							lastTrackerBtnType = Mathf.Abs(percent - beginPer) > Mathf.Abs(percent - endPer) ? 2 : 1;
						}

						if (lastTrackerBtnType == 1)
						{
							beginPer = percent;
							begin = beginPer * maxTime;
						}
						else if(lastTrackerBtnType == 2)
						{
							endPer = percent;
							end = endPer * maxTime;
						}
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						lastTrackerBtnType = 0;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						float percent = (dragPosition - rect.x) / rect.width;

						if (lastTrackerBtnType == 1)
						{
							beginPer = percent;
							begin = beginPer * maxTime;
						}
						else if (lastTrackerBtnType == 2)
						{
							endPer = percent;
							end = endPer * maxTime;
						}

						if (begin > end)
						{
							float temp = begin;
							begin = end;
							end = temp;
						}
					}
					break;
			}
		}

		public static float Tracker( float maxTime, float step, int FPS = 30)
		{
			float height = trackerHeight;

			var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);

			int controlID = GUIUtility.GetControlID(s_TimeTrackerHash, FocusType.Keyboard, rect);

			float stepPer = step / maxTime;

			float btnWidth = 3.0f;

			var stepRect = new Rect(new Vector2(rect.x + stepPer * rect.width - btnWidth * 0.5f, rect.y), new Vector2(btnWidth, height));

			Color bgColor = Color.white * 0.15f;

			Color stepBtnColor = Color.yellow * 0.75f;

			switch (Event.current.type)
			{
				case EventType.Repaint:
					{
						var s_HandleWireMaterial = ReflectTools.GetStaticField<Material>(typeof(HandleUtility), "s_HandleWireMaterial");

						if (s_HandleWireMaterial != null)
							s_HandleWireMaterial.SetPass(0);

						#region Background
						GL.Begin(GL.QUADS);
						GL.Color(bgColor);
						GL.Vertex(rect.min);
						GL.Vertex(new Vector2(rect.xMax, rect.yMin));
						GL.Vertex(rect.max);
						GL.Vertex(new Vector2(rect.xMin, rect.yMax));
						GL.End();
						#endregion

						GL.Begin(GL.QUADS);
						GL.Color(stepBtnColor);
						GL.Vertex(stepRect.min);
						GL.Vertex(new Vector2(stepRect.xMax, stepRect.yMin));
						GL.Vertex(stepRect.max);
						GL.Vertex(new Vector2(stepRect.xMin, stepRect.yMax));
						GL.End();
					}
					break;
				case EventType.MouseDown:
					if (rect.Contains(Event.current.mousePosition))
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();

						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						float percent = (dragPosition - rect.x) / rect.width;

						stepPer = percent;
						step = stepPer * maxTime;
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						lastTrackerBtnType = 0;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						float dragPosition = Mathf.Clamp(Event.current.mousePosition.x, rect.x, rect.x + rect.width);
						float percent = (dragPosition - rect.x) / rect.width;

						stepPer = percent;
						step = stepPer * maxTime;
					}
					break;
			}

			return step;
		}
	}
}
