using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class DesignerHr : MonoBehaviour, IDesignerBlock, IPauseable, IAlphaSetable
	{
		public void Start()
		{
			if (this.Md == null)
			{
				Material material;
				if (this.stencil_ref >= 0)
				{
					material = MTRX.getMtr(BLEND.NORMALST, this.stencil_ref);
				}
				else
				{
					material = MTRX.MtrMeshNormal;
				}
				this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, material, 0f, -1, null, false, false);
				this.redraw();
			}
		}

		public void redraw()
		{
			if (this.Md == null)
			{
				this.Start();
				return;
			}
			if (this.width_px > 0f)
			{
				this.Md.Col = this.Col;
				this.Md.Col.a = (byte)((float)this.Md.Col.a * this.alpha_);
				float num = this.width_px * this.draw_width_rate;
				if (this.vertical)
				{
					float num2 = -this.get_swidth_px() * 0.5f + this.margin_t;
					if (this.dashed > 0)
					{
						this.Md.LineDashed(num2 + this.height_px * 0.5f, -num * 0.5f, num2 + this.height_px * 0.5f, num * 0.5f, 0f, this.dashed, this.height_px, false, 0.66f);
					}
					else
					{
						this.Md.RectBL(num2, -num * 0.5f, this.height_px, num, false);
					}
				}
				else
				{
					float num3 = -this.get_sheight_px() * 0.5f + this.margin_t;
					if (this.dashed > 0)
					{
						this.Md.LineDashed(-num * 0.5f, num3 + this.height_px * 0.5f, num * 0.5f, num3 + this.height_px * 0.5f, 0f, this.dashed, this.height_px, false, 0.66f);
					}
					else
					{
						this.Md.RectBL(-num * 0.5f, num3, num, this.height_px, false);
					}
				}
			}
			this.Md.updateForMeshRenderer(false);
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ == value)
				{
					return;
				}
				this.alpha_ = value;
				this.redraw();
			}
		}

		public void OnDestroy()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
		}

		public void setAlpha(float value)
		{
			this.alpha = value;
		}

		public float get_swidth_px()
		{
			if (!this.vertical)
			{
				return this.width_px;
			}
			return this.height_px + this.margin_t + this.margin_b;
		}

		public float get_sheight_px()
		{
			if (!this.vertical)
			{
				return this.height_px + this.margin_t + this.margin_b;
			}
			return this.width_px;
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void AddSelectableItems(List<aBtn> A, bool only_front)
		{
		}

		public void Pause()
		{
		}

		public void Resume()
		{
		}

		public Color32 Col;

		public float width_px = 140f;

		public float draw_width_rate = 0.75f;

		public float height_px = 1f;

		public float margin_t = 18f;

		public float margin_b = 26f;

		public bool vertical;

		public int dashed;

		public int stencil_ref = -1;

		private MeshDrawer Md;

		private float alpha_ = 1f;
	}
}
