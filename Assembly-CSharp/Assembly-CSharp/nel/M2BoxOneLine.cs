using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2BoxOneLine : OneLineBox
	{
		protected override void Awake()
		{
			base.Awake();
			this.html_mode = true;
			this.deactivating_set_sx_shift = 0f;
			this.deactivating_set_sy_shift = -30f;
		}

		public M2BoxOneLine Init(M2DBase _M2D, bool set_gob_active = false)
		{
			this.M2D = _M2D;
			base.gameObject.layer = IN.gui_layer;
			IN.setZ(base.transform, 20f);
			base.gameObject.SetActive(false);
			this.ColHilight = C32.d2c(4290689711U);
			base.position_max_time(18, -1).appear_time(18).hideTime(26)
				.col(3707764736U)
				.TxCol(uint.MaxValue);
			base.make(" ");
			if (this.M2D.curMap != null)
			{
				this.MapPosAndShift_.x = (float)this.M2D.curMap.clms * 0.5f;
				this.MapPosAndShift_.y = (float)this.M2D.curMap.rows * 0.5f;
			}
			this.M2D.addValotAddition(this);
			base.gameObject.SetActive(set_gob_active);
			return this;
		}

		public override void destruct()
		{
			base.destruct();
			if (this.M2D != null)
			{
				try
				{
					this.M2D.remValotAddition(this);
				}
				catch
				{
				}
			}
		}

		public override MsgBox make(string _tex, bool refining)
		{
			this.activate();
			base.make(_tex, refining);
			this.fineCheckTargetPos(refining);
			return this;
		}

		public void fineCheckTargetPos(bool out_pos = false)
		{
			if (!base.visible)
			{
				return;
			}
			this.need_fine_position = false;
			M2BoxOneLine.fineBoxPosOnMap(this, this.M2D, this.MapPosAndShift_, out_pos, this.no_clip_map_pos, 0f, 0f);
		}

		protected override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			if (this.need_fine_position || (this.MapPosAndShift_.z != 0f && X.D))
			{
				this.fineCheckTargetPos(false);
			}
			return true;
		}

		public M2BoxOneLine setPos(float mapx, float mapy, float t_shift_px = -1000f, float b_shift_px = -1000f)
		{
			this.MapPosAndShift = new Vector4(mapx, mapy, (t_shift_px == -1000f) ? this.MapPosAndShift.z : t_shift_px, (b_shift_px == -1000f) ? this.MapPosAndShift.w : b_shift_px);
			return this;
		}

		public Vector4 MapPosAndShift
		{
			get
			{
				return this.MapPosAndShift_;
			}
			set
			{
				if (this.MapPosAndShift_.Equals(value))
				{
					return;
				}
				this.MapPosAndShift_ = value;
				this.need_fine_position = true;
				if (this.post >= 0)
				{
					this.post = 0;
				}
			}
		}

		public static void fineBoxPosOnMap(IDesignerPosSetableBlock Box, M2DBase M2D, Vector4 Pos, bool reset_time = false, bool no_clip_screen = false, float add_x_px = 0f, float add_y_px = 0f)
		{
			Map2d curMap = M2D.curMap;
			if (curMap == null)
			{
				return;
			}
			float num = IN.w - X.Abs(M2D.ui_shift_x);
			float h = IN.h;
			float num2 = M2D.ux2effectScreenx(curMap.map2ux(Pos.x));
			float num3 = M2D.uy2effectScreeny(curMap.map2uy(Pos.y));
			float num4 = M2D.ux2effectScreenx(curMap.pixel2ux(M2D.Cam.x));
			float num5 = M2D.uy2effectScreeny(curMap.pixel2uy(M2D.Cam.y));
			float scale = M2D.Cam.getScale(true);
			num2 = (num2 - num4) * scale * 64f;
			num3 = (num3 - num5) * scale * 64f;
			bool flag = num3 < IN.h * 0.35f;
			if (flag)
			{
				num3 += Pos.z;
			}
			else
			{
				num3 += Pos.w;
			}
			if (!no_clip_screen)
			{
				float num6 = Box.get_swidth_px() / 2f + 8f;
				float num7 = (Box.get_sheight_px() + 20f) * 0.5f;
				num2 = X.MMX2(-num * 0.5f + num6, num2, num * 0.5f - num6);
				num3 = X.MMX2(-h * 0.5f + num7, num3, h * 0.5f - num7);
			}
			num2 += M2D.ui_shift_x + add_x_px;
			num3 += add_y_px;
			Box.posSetA(num2, num3 - (float)(30 * X.MPF(flag)), num2, num3, !reset_time);
		}

		private M2DBase M2D;

		private Vector4 MapPosAndShift_;

		public bool need_fine_position;

		public bool no_clip_map_pos;
	}
}
