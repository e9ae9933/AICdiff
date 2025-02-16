using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class EvSmallWin : EvDrawer
	{
		public EvSmallWin(EvDrawerContainer _DC, string _key, EvDrawerContainer.LAYER _layer)
			: base(_DC, _key, _layer)
		{
		}

		public override void clearValues(bool clear_position = false)
		{
			base.clearValues(clear_position);
			if (this.Bx != null)
			{
				this.Bx.visible = false;
			}
		}

		protected override EvDrawer activate(uint view_type = 0U, bool refine_mesh = false)
		{
			base.activate(view_type, refine_mesh);
			if (this.Bx)
			{
				this.Bx.visible = true;
				this.Bx.activate();
			}
			return this;
		}

		public override EvDrawer deactivate()
		{
			if (this.Bx)
			{
				this.Bx.deactivate();
			}
			base.deactivate();
			this.mt_max = EvSmallWin.MT_HIDE;
			base.movetype = "ZSIN2";
			return this;
		}

		public bool setGrpAndText(string gkey, string msg = null, string view_key = "", string cap_msg = null, float _scale = -1f, float bmsize_x = -1000f, float bmsize_y = -1000f)
		{
			bool flag = false;
			bool flag2 = false;
			if (this.Bx == null || !this.Bx.visible)
			{
				flag = true;
				SND.Ui.play("paper", false);
			}
			if (msg == null)
			{
				if (this.Bx != null && this.Bx.isActive())
				{
					msg = this.Bx.getText();
				}
				else
				{
					msg = "";
				}
			}
			else if (msg != "")
			{
				X.de("swin msg 未実装", null);
			}
			if (_scale < 0f)
			{
				_scale = (this.Bx ? EvSmallWin.img_scale : EvSmallWin.DEF_SCALE);
			}
			EvSmallWin.img_scale = _scale;
			if (this.Bx != null)
			{
				flag2 = this.Bx.isActive();
				this.Bx.visible = true;
			}
			PxlFrame pxlFrame = null;
			if (gkey != "")
			{
				EvImg pic = EV.Pics.getPic(gkey, true, true);
				if (pic == null)
				{
					X.de("画像が見つかりません:" + gkey, null);
				}
				else
				{
					pxlFrame = pic.PF;
				}
			}
			this.activate(base.stringToViewType(view_key + "N"), false);
			if (flag)
			{
				if (this.Bx == null)
				{
					this.Bx = IN.CreateGob(this.MMRD.gameObject, "-swin_box").AddComponent<SmallWinBox>();
					this.Bx.gradation(new Color32[]
					{
						C32.d2c(EvSmallWin.boxcol_top),
						C32.d2c(EvSmallWin.boxcol_btm)
					}, null);
					this.Bx.use_valotile = this.use_valotile_;
				}
				this.Bx.Init(pxlFrame, 300);
				base.initPosition("C", "C+40", "C", "C-20", 0f);
				this.t = 0f;
				base.movetype = "ZSIN3";
				this.mt_max = EvSmallWin.MT_SHOW;
				this.Bx.pic_margin = 15;
				this.Bx.bm_size(bmsize_x, bmsize_y).bm_scale(EvSmallWin.img_scale * this.pz, true).bkg_scale(true, true, false)
					.appear_time(EvSmallWin.MT_SHOW)
					.TxSize((float)EvSmallWin.MSG_FONTSIZE);
				this.Bx.make(msg);
			}
			else
			{
				this.Bx.replaceGrp(pxlFrame).bm_scale(EvSmallWin.img_scale * this.pz, false).make(msg);
			}
			if (flag2)
			{
				this.t = (float)this.mt_max;
				this.x = this.dx_real;
				this.y = this.dy_real;
			}
			this.redrawMesh(0f);
			return true;
		}

		public override void repositMesh(float z = -1000f)
		{
			base.repositMesh(z);
			if (this.Bx != null)
			{
				if (z > -1000f)
				{
					base.repositMesh(this.Bx.transform, z, 0f, -1, 0f, 0f);
					return;
				}
				this.Bx.SetXyPx(this.x, this.y);
				this.Bx.bm_scale(EvSmallWin.img_scale * this.pz, false);
				this.Bx.spMove(this.px, this.py, this.prot, false);
			}
		}

		public EvSmallWin prepareShadow(float _scale = -1f)
		{
			if (this.Bx == null)
			{
				return this;
			}
			this.Bx.prepareShadow(MTRX.getPF("lightball0"), _scale);
			return this;
		}

		public override EvDrawer fine(bool fine_alpha = true, bool fine_move = true, bool fine_movea = true)
		{
			base.fine(fine_alpha, fine_move, fine_movea);
			if (this.Bx == null)
			{
				return this;
			}
			if (this.t >= 0f)
			{
				this.Bx.SetXyPx(this.x, this.y);
			}
			this.redrawMesh(0f);
			if (this.t < 0f && this.Bx != null)
			{
				this.Bx.visible = false;
			}
			return this;
		}

		public override void redrawMesh(float fcnt)
		{
			if (this.Bx == null)
			{
				return;
			}
			float drawAlpha = base.getDrawAlpha();
			this.Bx.bm_alpha(drawAlpha).bm_scale(EvSmallWin.img_scale * this.pz, false);
		}

		public override void setDrawnAlpha(float alp1 = -1f)
		{
			if (alp1 < 0f)
			{
				alp1 = base.getDrawAlpha();
			}
			base.setDrawnAlpha(alp1);
			this.Bx.bm_alpha(alp1);
		}

		public override bool runDraw(float fcnt, bool deleting = false)
		{
			bool flag = base.runDraw(fcnt, deleting);
			if (this.Bx != null && ((!this.Bx.visible || !flag) && deleting))
			{
				this.Bx.visible = false;
				return false;
			}
			return flag;
		}

		public override EvDrawer release()
		{
			if (this.Bx)
			{
				this.Bx.visible = false;
			}
			return base.release();
		}

		public EvDrawer destruct()
		{
			this.release();
			if (this.Bx)
			{
				this.Bx.destruct();
				this.Bx = null;
			}
			return null;
		}

		public override int id_in_layer
		{
			get
			{
				return X.NmI(TX.slice(this.key, 2), -1, true, false);
			}
			set
			{
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				this.use_valotile_ = value;
				if (this.Bx != null)
				{
					this.Bx.use_valotile = value;
				}
			}
		}

		private SmallWinBox Bx;

		private static readonly int MSG_FONTSIZE = 16;

		private static readonly int MT_SHOW = 48;

		private static readonly float DEF_SCALE = 1f;

		private static readonly int MT_HIDE = 40;

		public static uint boxcol_top = 2853332515U;

		public static uint boxcol_btm = 2861102961U;

		private static float img_scale = 1f;

		private bool use_valotile_;
	}
}
