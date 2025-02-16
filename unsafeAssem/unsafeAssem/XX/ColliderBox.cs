using System;
using UnityEngine;

namespace XX
{
	public class ColliderBox : MonoBehaviour, IClickable
	{
		public void Init(GameObject Gob, MeshDrawer _Md, float w, float h, float radius = 18f)
		{
			this.swidth = w;
			this.sheight = h;
			this.Md = _Md;
			this.shift_x = this.Md.base_x;
			this.shift_y = this.Md.base_y;
			this.Md.clearSimple();
			this.Md.KadomaruRect(0f, 0f, w, h, 18f, 0f, false, 0f, 0f, false).updateForMeshRenderer(true);
		}

		public Color32 Col
		{
			get
			{
				if (this.Md == null)
				{
					return MTRX.ColTrnsp;
				}
				return this.Md.Col;
			}
			set
			{
				if (this.Md == null)
				{
					return;
				}
				this.Md.Col = value;
				Color32[] colorArray = this.Md.getColorArray();
				int vertexMax = this.Md.getVertexMax();
				for (int i = 0; i < vertexMax; i++)
				{
					colorArray[i] = value;
				}
				this.Md.updateForMeshRenderer(true);
			}
		}

		public bool getClickable(Vector2 PosU, out IClickable Res)
		{
			Res = null;
			if (CLICK.getClickableRectSimple(PosU - new Vector2(this.shift_x, this.shift_y), base.transform, this.swidth * 0.015625f, this.sheight * 0.015625f))
			{
				Res = this;
				return true;
			}
			return false;
		}

		private void OnEnable()
		{
			IN.Click.addClickable(this);
		}

		private void OnDisable()
		{
			IN.Click.remClickable(this, false);
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void OnPointerEnter()
		{
		}

		public void OnPointerExit()
		{
		}

		public bool OnPointerDown()
		{
			return false;
		}

		public void OnPointerUp(bool clicking)
		{
		}

		public float getFarLength()
		{
			return base.transform.position.z + 0.01f;
		}

		private float swidth;

		private float sheight;

		private float shift_x;

		private float shift_y;

		private MeshDrawer Md;
	}
}
