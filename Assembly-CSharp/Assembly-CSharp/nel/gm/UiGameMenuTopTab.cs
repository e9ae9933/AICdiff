using System;
using XX;

namespace nel.gm
{
	internal sealed class UiGameMenuTopTab : UiBoxDesignerFamily
	{
		internal void initializeGM(UiGameMenu _GM)
		{
			this.GM = _GM;
			this.bounds_h = this.GM.bounds_h;
			this.bounds_w = this.GM.right_w;
		}

		internal float cur_item_height
		{
			get
			{
				return this.cur_row_height + this.margin_h;
			}
		}

		internal float activateState(UiGMC AppearC, bool is_top, float right_cx, float right_cy)
		{
			int num = (int)(is_top ? AppearC.subarea_top_rows : AppearC.subarea_btm_rows);
			int num2 = (int)(is_top ? AppearC.subarea_top_clms : AppearC.subarea_btm_clms);
			int num3 = num * num2;
			float num4 = (is_top ? AppearC.subarea_top_row_height : AppearC.subarea_btm_row_height);
			this.cur_row_height = this.row_h * num4 + this.margin_h * (num4 - 1f);
			if (num3 == 0)
			{
				if (this.isActive())
				{
					this.deactivate(false);
				}
				return 0f;
			}
			this.active = true;
			base.gameObject.SetActive(true);
			float num5 = (this.bounds_w - (float)(num2 - 1) * this.margin_w) / (float)num2;
			float num6 = this.cur_row_height;
			float num7 = 0f;
			float num8 = 0f;
			float cur_item_height = this.cur_item_height;
			float num9 = right_cy + this.bounds_h / 2f * (float)X.MPF(is_top);
			for (int i = 0; i < num3; i++)
			{
				UiBoxDesigner uiBoxDesigner;
				if (i >= this.ADs.Count)
				{
					uiBoxDesigner = base.Create("toptab_" + i.ToString(), 0f, 0f, 100f, 40f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
					uiBoxDesigner.stencil_ref = (uiBoxDesigner.box_stencil_ref_mask = 180 + i);
					uiBoxDesigner.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
					uiBoxDesigner.bkg_scale(false, false, false);
					uiBoxDesigner.item_margin_x_px = 0f;
					uiBoxDesigner.item_margin_y_px = 4f;
				}
				else
				{
					uiBoxDesigner = this.ADs[i];
				}
				UiGameMenuTopTab.BoxSpeedSet(uiBoxDesigner.getBox(), true);
				uiBoxDesigner.getBox().scaling_alpha_set(false, false);
				float num10 = right_cx - this.bounds_w / 2f + (0.5f + num7) * num5 + num7 * this.margin_w;
				float num11 = num9 + (is_top ? (-(0.5f + num8) * num6 - num8 * this.margin_h) : ((0.5f + ((float)(num - 1) - num8)) * num6 + ((float)(num - 1) - num8) * this.margin_h));
				uiBoxDesigner.WHanim(num5, num6, true, this.pre_count == 0);
				uiBoxDesigner.activate();
				if (this.pre_count == 0)
				{
					uiBoxDesigner.positionD(num10, num11, 3, 24f);
				}
				else if (this.pre_count >= num3)
				{
					uiBoxDesigner.position(num10, num11, -1000f, -1000f, false);
				}
				else if (i >= this.pre_count)
				{
					uiBoxDesigner.position(right_cx + this.bounds_w / 2f, num11, num10, num11, false);
				}
				else
				{
					uiBoxDesigner.position(num10, num11, -1000f, -1000f, false);
				}
				if ((num7 += 1f) >= (float)num2)
				{
					num7 = 0f;
					num8 += 1f;
				}
			}
			for (int j = num3; j < this.pre_count; j++)
			{
				UiBoxDesigner uiBoxDesigner2 = this.ADs[j];
				uiBoxDesigner2.wh_animZero(true, false);
				uiBoxDesigner2.position(right_cx + this.bounds_w / 2f, uiBoxDesigner2.getBox().get_deperture_y(), -1000f, -1000f, true);
				uiBoxDesigner2.deactivate();
			}
			this.pre_count = num3;
			return (float)num * this.cur_item_height;
		}

		internal static void BoxSpeedSet(UiBox Bx, bool flag = true)
		{
			Bx.resize_speed_when_active = (flag ? 0.5f : 1f);
			Bx.wh_animate_cos = false;
			int num = (int)(Bx.resize_speed_when_active * 20f);
			Bx.position_max_time(num, flag ? num : 18);
			Bx.appear_time(num);
			Bx.hideTime(flag ? num : 18);
			if (flag)
			{
				Bx.pos_animate_sin_when_active = true;
			}
		}

		internal UiBoxDesigner GetDesigner(int i)
		{
			if (!X.BTW(0f, (float)i, (float)this.pre_count))
			{
				return null;
			}
			return this.ADs[i];
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			for (int i = 0; i < this.pre_count; i++)
			{
				UiBoxDesigner uiBoxDesigner = this.ADs[i];
				uiBoxDesigner.Clear();
				uiBoxDesigner.wh_animZero(true, true);
				uiBoxDesigner.positionD(uiBoxDesigner.getBox().get_deperture_x(), uiBoxDesigner.getBox().get_deperture_y(), 3, 24f);
			}
			this.pre_count = 0;
			this.pre_categ = CATEG._NOUSE;
			base.deactivate(immediate);
			return this;
		}

		internal const float BXR_Y = 45f;

		internal float bounds_w;

		internal float bounds_h;

		private float row_h = 32f;

		private float margin_h = 6f;

		private float margin_w = 4f;

		private float cur_row_height = 32f;

		internal int pre_count;

		private CATEG pre_categ;

		private UiGameMenu GM;
	}
}
