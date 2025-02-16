using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UISpecialGage
	{
		public UISpecialGage(MeshDrawer _Md, GameObject _Gob, PxlFrame _TitleMesh, float _maxv, float _defaultv, float base_z = -0.25f)
		{
			this.Md = _Md;
			this.Gob = _Gob;
			this.PFTitle = _TitleMesh;
			if (this.PFTitle == null)
			{
				X.de("UISpecialGage::ctr -" + this.Gob.name + " PFTitle がnull", null);
			}
			this.maxv = _maxv;
			this.value_ = _defaultv;
			this.Md.base_z = base_z;
			this.Md.chooseSubMesh(0, true, false).setMaterial(MTRX.getMtr(BLEND.MUL, -1), false);
			this.Md.chooseSubMesh(1, true, false).setMaterial(MTRX.getMtr(BLEND.ADD, -1), false);
			this.Md.chooseSubMesh(2, true, false).setMaterial(MTRX.MIicon.getMtr(BLEND.ADD, -1), false);
			this.Md.chooseSubMesh(3, true, false).setMaterial(MTRX.getMtr(BLEND.ADD, -1), false);
			this.Md.connectRendererToTriMulti(this.Gob.GetComponent<MeshRenderer>());
		}

		public bool run(int fcnt)
		{
			if (this.t_timeout >= 22)
			{
				return false;
			}
			if (this.value_ == this.defaultv)
			{
				this.t_timeout += fcnt;
				if (this.t_timeout >= 22)
				{
					this.t = 0;
					this.Gob.SetActive(false);
					return false;
				}
				if (this.t_timeout > 0)
				{
					this.need_fine_ |= 9;
				}
			}
			if (this.t < 22)
			{
				if (this.t <= 0 && !this.Gob.activeSelf)
				{
					this.Gob.SetActive(true);
				}
				if (this.alpha > 0f)
				{
					this.t = X.Mn(22, this.t + fcnt);
					this.need_fine_ |= 9;
				}
			}
			if (this.vib_level_ > 0f && X.D)
			{
				float num = 0f;
				num += 0.75f * X.ZLINE(1f - X.Abs(X.COSIT(X.NI(70, 43, this.vib_level_))) - 0.25f, 0.75f);
				num += 0.75f * X.ZLINE(1f - X.Abs(X.COSIT(X.NI(87, 65, this.vib_level_))) - 0.25f, 0.75f);
				num = X.ZLINE(num) * (0.4f + 0.6f * X.ZSINV(this.vib_level_));
				if (this.pre_vib_level != num)
				{
					this.pre_vib_level = num;
					this.need_fine_ |= 1;
				}
				this.need_fine_ |= 8;
			}
			if ((this.need_fine_ & 8) != 0)
			{
				this.need_fine_ -= 8;
				this.finePosition((float)fcnt);
			}
			if ((this.need_fine_ & 7) != 0)
			{
				this.fineMesh();
			}
			return true;
		}

		public float value
		{
			get
			{
				return this.value_;
			}
			set
			{
				if (this.value_ == value)
				{
					return;
				}
				this.value_ = value;
				this.need_fine_ |= 5;
				this.fineActivation(false);
			}
		}

		public float vib_level
		{
			get
			{
				return this.vib_level_;
			}
			set
			{
				if (this.vib_level_ == value)
				{
					return;
				}
				this.vib_level_ = value;
				if (this.vib_draw_width > 0f)
				{
					this.need_fine_ |= 5;
				}
				this.fineActivation(false);
			}
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				value = X.MMX(0f, value, 1f);
				if (this.alpha_ == value)
				{
					return;
				}
				this.alpha_ = value;
				if (this.alpha <= 0f)
				{
					this.t = 0;
					this.t_timeout = 21;
					return;
				}
				this.need_fine_ |= 15;
				if (this.t_timeout >= 21)
				{
					this.fineActivation(false);
				}
			}
		}

		public void fineActivation(bool force = false)
		{
			if (this.value_ != this.defaultv || force)
			{
				if (this.t <= 0)
				{
					this.t = 0;
					this.need_fine_ |= 2;
				}
				this.t_timeout = -this.T_TIMEOUT;
			}
		}

		public void showForce(int alloc_t = -1)
		{
			if (alloc_t < 0)
			{
				alloc_t = this.T_TIMEOUT;
			}
			bool flag = this.isActive();
			this.fineActivation(true);
			if (!flag)
			{
				this.t_timeout = X.MMX(-alloc_t, this.t_timeout, 0);
				return;
			}
			if (this.t_timeout >= 0)
			{
				this.need_fine_ |= 9;
			}
			this.t_timeout = X.Mn(X.Mn(this.t_timeout, -alloc_t), 0);
		}

		public bool need_fine
		{
			get
			{
				return (this.need_fine_ & 6) > 0;
			}
			set
			{
				if (value)
				{
					this.need_fine_ |= 2;
				}
			}
		}

		private void fineMesh()
		{
			float num = this.dheight * 0.5f + 2f;
			float num2 = X.Mn(X.ZLINE((float)this.t, 11f) * 0.875f + 0.125f, 1f - X.ZLINE((float)this.t_timeout, 22f));
			Color32 color = this.Md.ColGrd.Set(this.tcol).C;
			if (this.tcol != this.tcol_pinch)
			{
				color = this.Md.ColGrd.blend(this.tcol_pinch, X.ZLINE(X.Abs(this.value_ - this.defaultv), this.maxv * 0.7f)).C;
			}
			if (this.pre_vib_level > 0f)
			{
				color = this.Md.ColGrd.blend3(this.Md.ColGrd.rgba, this.tcol, this.tcol_vibrate, this.pre_vib_level).C;
			}
			if ((this.need_fine_ & 6) != 0)
			{
				float num3 = (float)X.IntC((this.dwidth - this.vib_draw_width * 2f) * this.value_ / this.maxv + ((this.vib_draw_width == 0f) ? 0f : ((1f - X.ZLINE(this.vib_level_ - this.vib_draw_thresh_level, 1f - this.vib_draw_thresh_level)) * this.vib_draw_width * 2f))) * 0.5f;
				if ((this.need_fine_ & 2) != 0)
				{
					this.Md.clear(false, false);
					this.Md.chooseSubMesh(0, true, false);
					this.Md.Col = this.Md.ColGrd.Set(this.mulcol).mulA(1f).C;
					this.Md.ColGrd.mulA(0f);
					this.Md.KadomaruRect(0f, 0f, this.dwidth + 80f, this.dheight + 80f, 40f, 0f, false, 1f, 0f, false);
					this.Md.KadomaruRect(0f, 0f, this.dwidth, 40f, 20f, 0f, false, 1f, 0f, false);
					this.Md.finalizeSubMeshArranger(true);
					this.Md.chooseSubMesh(1, true, false);
					this.Md.Col = this.Md.ColGrd.Set(color).mulA(this.alpha_ * num2 * 0.25f).C;
					this.Md.ColGrd.mulA(0f);
					this.Md.KadomaruRect(0f, 0f, num3 * 2f + 4f + 80f, 80f, 40f, 0f, false, 1f, 0f, false);
					this.Md.Circle(-this.dwidth * 0.5f, 0f, 40f, 0f, false, 1f, 0f);
					this.Md.Circle(this.dwidth * 0.5f, 0f, 40f, 0f, false, 1f, 0f);
					this.Md.KadomaruRect(0f, num, (float)(this.PFTitle.pPose.width + 30) + 80f, 100f, 40f, 0f, false, 1f, 0f, false);
					this.Md.finalizeSubMeshArranger(true);
					this.Md.chooseSubMesh(2, true, false);
					this.Md.Col = this.Md.ColGrd.Set(color).mulA(this.alpha_ * num2 * 0.25f).C;
					this.Md.RotaPF(0f, num, 1f, 1f, 0f, this.PFTitle, false, false, false, uint.MaxValue, false, 0);
					this.Md.finalizeSubMeshArranger(true);
					this.Md.chooseSubMesh(3, true, false);
				}
				else
				{
					this.Md.Col = this.Md.ColGrd.Set(color).C;
					this.Md.ColGrd.mulA(0f);
					this.Md.chooseSubMesh(1, false, false).reverVerAndTriIndexFromCSM();
					this.Md.KadomaruRect(0f, 0f, num3 * 2f + 4f + 80f, 80f, 40f, 0f, false, 1f, 0f, false);
					this.Md.chooseSubMesh(3, false, false).reverVerAndTriIndexFromCSM();
				}
				this.Md.Daia(-num3, 0f, 3.5f, 3.5f, false);
				this.Md.Daia(num3, 0f, 3.5f, 3.5f, false);
				this.Md.Line(-num3, 0f, num3, 0f, 1f, false, 0f, 0f);
				if ((this.need_fine_ & 2) != 0)
				{
					this.need_fine_ &= 8;
					if (this.vib_draw_width != 0f)
					{
						this.Md.Line(-this.vib_draw_width, -8f, -this.vib_draw_width, 8f, 1f, false, 0f, 0f);
						this.Md.Line(this.vib_draw_width, -8f, this.vib_draw_width, 8f, 1f, false, 0f, 0f);
						this.Md.Line(-this.vib_draw_width * 0.875f, -5f, -this.vib_draw_width * 0.875f, 5f, 0.5f, false, 0f, 0f);
						this.Md.Line(this.vib_draw_width * 0.875f, -5f, this.vib_draw_width * 0.875f, 5f, 0.5f, false, 0f, 0f);
					}
					this.Md.Line(-this.dwidth * 0.5f - 4f, -6f, -this.dwidth * 0.5f - 4f, 6f, 1f, false, 0f, 0f);
					this.Md.Line(this.dwidth * 0.5f + 4f, -6f, this.dwidth * 0.5f + 4f, 6f, 1f, false, 0f, 0f);
					this.Md.Line(0f, -4.5f, 0f, 4.5f, 1f, false, 0f, 0f);
					this.Md.finalizeSubMeshArranger(true);
				}
				else
				{
					this.need_fine_ = (this.need_fine_ & 8) | 1;
					this.Md.finalizeSubMeshArranger(false);
				}
			}
			if ((this.need_fine_ & 1) != 0)
			{
				Color32 color2 = this.Md.ColGrd.Set(this.mulcol).mulA(this.alpha_ * num2).C;
				this.Md.chooseSubMeshArranger(0).setColAll(color2, true);
				color2 = this.Md.ColGrd.Set(color).mulA(this.alpha_ * num2 * 0.25f).C;
				this.Md.chooseSubMeshArranger(1).setColAll(color2, true);
				color2 = this.Md.ColGrd.Set(color).mulA(this.alpha_ * num2).C;
				this.Md.chooseSubMeshArranger(3).setColAll(color2, true);
				this.Md.chooseSubMeshArranger(2).setColAll(color2, true);
				this.need_fine_ &= 14;
			}
			this.Md.updateForMeshRenderer(true);
		}

		public void finePosition(float fcnt)
		{
			float num = (X.ZSIN((float)this.t, 22f) * 0.5f + 0.5f) * (1f - X.ZLINE((float)this.t_timeout, 22f));
			if (this.vib_x > 0f || this.vib_level_ > 0f)
			{
				this.vib_x = X.VALWALK(this.vib_x, (2f + 11f * X.ZSINV(this.vib_level_ - 0.4f, 0.6f)) * X.COSIT(X.NI(47.4f, 11.3f, this.vib_level_) + (5f + 20f * X.ZSIN2(this.vib_level_, 0.5f)) * X.COSIT(X.NI(110f, 67f, this.vib_level_))), fcnt * X.NI(2.2f, 5.4f, this.vib_level_));
			}
			IN.Pos2(this.Gob.transform, X.NI(this.slidein_x, 0f, num) + this.vib_x * 0.015625f, X.NI(this.slidein_y, 0f, num));
		}

		public bool isActive()
		{
			return this.t_timeout < 22;
		}

		public int rest_time
		{
			get
			{
				return X.Mx(-this.t_timeout, 0);
			}
		}

		public uint tcol = uint.MaxValue;

		public uint tcol_pinch = uint.MaxValue;

		public uint tcol_vibrate = uint.MaxValue;

		public uint mulcol = 2852126720U;

		private GameObject Gob;

		private MeshDrawer Md;

		private const int CSM_BACK = 0;

		private const int CSM_BLUR = 1;

		private const int CSM_CHR = 2;

		private const int CSM_TCOL = 3;

		private float maxv = 100f;

		private float value_ = 100f;

		public float defaultv = 100f;

		public float dwidth = 160f;

		public float dheight = 20f;

		private byte need_fine_ = 15;

		private const int T_FADE = 22;

		public int T_TIMEOUT = 40;

		public int t;

		public int t_timeout = 22;

		public float slidein_x;

		public float slidein_y = -0.9375f;

		private float vib_level_;

		public float vib_draw_width;

		public float vib_draw_thresh_level;

		private float vib_x;

		private float pre_vib_level;

		private const float blur_alp_lv = 0.25f;

		private float alpha_ = 1f;

		private PxlFrame PFTitle;
	}
}
