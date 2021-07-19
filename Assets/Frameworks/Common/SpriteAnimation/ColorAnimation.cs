using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Frameworks.Common
{
	public class ColorAnimation : CurveAdapt
	{
		public enum Type
		{
			ColorOnly,
			ColorInImage,
			ColorInSpriteRender,
			ColorInMeshRender,

		}

		public Type type = Type.ColorOnly;

		public Color startColor = Color.white;
		public Color endColor	= Color.white;

		public Color lerpColor;

		public bool IsChangeChildren = true;

		Image[] images;
		SpriteRenderer[] spriteRenderer;
		MeshRenderer[] meshRenderer;

		void Awake()
		{
			UpdateActiveObjList();
		}

		public void UpdateColorToObjs()
		{
			int i = 0;
			switch (type)
			{
				case Type.ColorInImage:
					{
						if (images != null)
						{
							for (; i < images.Length; ++i)
							{
								images[i].color = lerpColor;
							}
						}
						
					}
					break;
				case Type.ColorInSpriteRender:
					{
						if (spriteRenderer != null)
						{
							for (; i < spriteRenderer.Length; ++i)
							{
								spriteRenderer[i].color = lerpColor;
							}
						}
					}
					break;
				case Type.ColorInMeshRender:
					{
						if (meshRenderer != null)
						{
							for (; i < meshRenderer.Length; ++i)
							{
								meshRenderer[i].material.color = lerpColor;
							}
						}
					}
					break;
			}
		}

		public void UpdateActiveObjList()
		{
			switch (type)
			{
				case Type.ColorInImage:
					UpdateImageList();
					break;
				case Type.ColorInSpriteRender:
					UpdateSpriteRendererList();
					break;
				case Type.ColorInMeshRender:
					UpdateMeshRendererList();
					break;
			}
		}

		public void UpdateImageList()
		{
			if (IsChangeChildren)
				images = transform.GetComponentsInChildren<Image>();
			else
			{
				Image img = transform.GetComponent<Image>();

				if (img != null)
				{
					images = new Image[] { img };
				}
			}
		}

		public void UpdateSpriteRendererList()
		{
			if (IsChangeChildren)
				spriteRenderer = transform.GetComponentsInChildren<SpriteRenderer>();
			else
			{
				SpriteRenderer render = transform.GetComponent<SpriteRenderer>();

				if (render != null)
				{
					spriteRenderer = new SpriteRenderer[] { render };
				}
			}
		}

		public void UpdateMeshRendererList()
		{
			if (IsChangeChildren)
				meshRenderer = transform.GetComponentsInChildren<MeshRenderer>();
			else
			{
				MeshRenderer render = transform.GetComponent<MeshRenderer>();

				if (render != null)
				{
					meshRenderer = new MeshRenderer[] { render };
				}	
			}
		}

		public override void DoUpdate(float CurValue)
		{
			lerpColor = Color.Lerp(startColor, endColor, CurValue);

			UpdateColorToObjs();
		}
	}
}
