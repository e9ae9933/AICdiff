using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinPopper : ButtonSkin
	{
		public ButtonSkinPopper(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdEf = base.makeMesh(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			this.Md.InitSubMeshContainer(0);
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
			this.ValotEf = IN.GetOrAdd<ValotileRenderer>(this.MMRD.GetGob(this.MdEf));
			this.ValotEf.Init(this.MdEf, base.getMeshRenderer(this.MdEf), true);
			this.w = ((_w > 0f) ? _w : 130f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 130f) * 0.015625f;
			this.fine_continue_flags = 1U;
			if (ButtonSkinPopper.PtcForward == null)
			{
				ButtonSkinPopper.PtcForward = new EfParticleOnce("ui_popper_click", EFCON_TYPE.UI);
			}
		}

		public override bool use_valotile
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		public void setMesh(PxlFrame _PF)
		{
			if (this.PF == _PF || _PF == null)
			{
				return;
			}
			this.PF = _PF;
			if (this.PF != null)
			{
				this.Md.chooseSubMesh(2, false, true);
				this.Md.setMaterial(MTRX.getMI(this.PF).getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
				this.Md.chooseSubMesh(0, false, false);
			}
			this.fine_flag = true;
		}

		public void initPopping(bool forward)
		{
			this.pop_t = IN.totalframe;
			this.pop_forward = forward;
			this.fine_flag = true;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f * 0.5f;
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 0f;
			int num5 = 0;
			int num6 = (this.pop_forward ? 24 : 24);
			Color32 color = this.BaseCol;
			Color32 color2 = this.IconCol;
			this.Md.chooseSubMesh(0, false, false);
			if (this.pop_t != 0)
			{
				num5 = IN.totalframe - this.pop_t;
				if (this.pop_forward)
				{
					float num7 = 1f - X.ZSIN((float)num5, (float)num6);
					num2 += num7 * 0.5f;
					num3 += num7 * 0.32f;
					color = this.Md.ColGrd.Set(color).blend(this.HilightCol, num7).C;
					color2 = this.Md.ColGrd.Set(color2).blend(this.HilightIconCol, num7).C;
					num4 = (X.COSI((float)num5, 7.4f) + 0.5f * X.COSI((float)(num5 + 2), 18.143f)) * num7 * 0.014f * 3.1415927f;
				}
				else
				{
					float num8 = X.Sin0(0.125f + X.ZSIN((float)num5, (float)num6) * 0.375f);
					num2 -= num8 * 0.25f;
					num3 -= num8 * 0.14f;
					color = this.Md.ColGrd.Set(color).blend(this.BlurCol, num8).C;
					color2 = this.Md.ColGrd.Set(color2).blend(this.BlurIconCol, num8).C;
				}
			}
			if (!base.isLocked())
			{
				if (this.isPushDown())
				{
					color = this.BlurCol;
					color2 = this.BlurIconCol;
				}
				else if (base.isHoveredOrPushOut())
				{
					num2 *= 1.125f;
					num3 *= 1.15f;
					float num9 = 0.5f + 0.5f * X.COSIT(40f);
					color = this.Md.ColGrd.Set(color).blend(this.HilightCol, num9).mulA(0.75f + 0.25f * num9)
						.C;
					color2 = this.Md.ColGrd.Set(color2).blend(this.HilightIconCol, num9).mulA(0.75f + 0.25f * num9)
						.C;
				}
			}
			float drawalpha = this.drawalpha;
			this.Md.Col = C32.MulA(color, drawalpha);
			this.MdEf.clearSimple();
			if (this.pop_t != 0 && this.pop_forward && ButtonSkinPopper.PtcForward != null)
			{
				ButtonSkinPopper.PtcForward.drawTo(this.MdEf, 0f, 0f, num * 0.75f, num6, (float)num5, 0f);
			}
			this.Md.Col = C32.MulA(color, drawalpha);
			this.Md.Circle(0f, 0f, num * num2, 0f, false, 0f, 0f);
			if (this.PF != null)
			{
				num3 *= this.base_ico_scale;
				this.Md.chooseSubMesh(2, false, true);
				this.Md.Col = C32.MulA(color2, drawalpha);
				this.Md.RotaPF(this.iconx_, this.icony_, num3, num3, num4, this.PF, false, false, false, uint.MaxValue, false, 0);
				this.drawn_ico_scale = num3;
				this.Md.chooseSubMesh(0, false, false);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdEf.updateForMeshRenderer(false);
			base.Fine();
			if (this.pop_t != 0)
			{
				if (num5 >= num6)
				{
					this.pop_t = 0;
				}
				else
				{
					this.fine_flag = true;
				}
			}
			return this;
		}

		protected virtual float drawalpha
		{
			get
			{
				return this.alpha_;
			}
		}

		public float iconx
		{
			get
			{
				return this.iconx_;
			}
			set
			{
				if (this.iconx == value)
				{
					return;
				}
				this.iconx_ = value;
				this.fine_flag = true;
			}
		}

		public float icony
		{
			get
			{
				return this.icony_;
			}
			set
			{
				if (this.icony == value)
				{
					return;
				}
				this.icony_ = value;
				this.fine_flag = true;
			}
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			this.setMesh(MTRX.getPF(str));
			return this;
		}

		public const float DEFAULT_W = 130f;

		private MeshDrawer Md;

		private MeshDrawer MdEf;

		private ValotileRenderer ValotEf;

		public PxlFrame PF;

		public Color32 BaseCol = C32.d2c(uint.MaxValue);

		public Color32 IconCol = C32.d2c(4282006074U);

		public Color32 HilightCol = C32.d2c(4294963401U);

		public Color32 HilightIconCol = C32.d2c(4278190080U);

		public Color32 BlurCol = C32.d2c(2868903935U);

		public Color32 BlurIconCol = C32.d2c(1996488704U);

		public float base_ico_scale = 0.375f;

		public float drawn_ico_scale = 0.375f;

		private static EfParticleOnce PtcForward;

		private float iconx_;

		private float icony_;

		private int pop_t;

		private bool pop_forward;

		private const int ANIM_MAXT_F = 24;

		private const int ANIM_MAXT_B = 24;
	}
}
