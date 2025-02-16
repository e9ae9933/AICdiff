using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class aBtnBoxEar : aBtnNel
	{
		public static aBtnBoxEar Create(UiBoxDesigner _Target, float wh, AIM _pop_a, float pivot)
		{
			aBtnBoxEar aBtnBoxEar = IN.CreateGob(_Target.gameObject, "-ear").AddComponent<aBtnBoxEar>();
			aBtnBoxEar.h = wh;
			aBtnBoxEar.w = wh;
			aBtnBoxEar.initTarget(_Target, _pop_a, pivot);
			return aBtnBoxEar;
		}

		protected override void Awake()
		{
			base.Awake();
			this.w = (this.h = 40f);
			this.t_margin = -this.hide_margin_t;
			base.addHoverFn(new FnBtnBindings(this.fnHoverEar));
			base.addOutFn(new FnBtnBindings(this.fnOutEar));
			base.unselectable(true);
		}

		public void initTarget(UiBoxDesigner _Target, AIM _pop_a, float pivot)
		{
			if (this.Skin == null)
			{
				this.StartBtn();
			}
			this.Target = _Target;
			base.transform.SetParent(this.Target.transform, false);
			this.t_show = -this.ANIM_T;
			this.Target.WHanim(this.Target.w, this.Target.h, false, false);
			this.Target.Focusable(false, false, null);
			this.Target.hide();
			this.pop_a = _pop_a;
			Vector3 zero = Vector3.zero;
			switch (this.pop_a)
			{
			case AIM.L:
			case AIM.R:
			{
				float num = this.Target.get_sheight_px() - this.Target.radius * 2f - this.w;
				zero.y = num * pivot;
				zero.x = -(this.Target.get_swidth_px() * 0.5f + this.w * 0.5f) * (float)CAim._YD(this.pop_a, 1);
				float num2 = this.scwh - (this.Target.get_swidth_px() * 0.5f - this.Target.radius);
				num = this.Target.get_swidth_px() * 0.5f + this.w;
				this.RcMouseIn.Set(num2 * (float)CAim._XD(this.pop_a, 1) - num, this.Target.transform.localPosition.y * 64f - this.Target.get_sheight_px() * 0.5f, num * 2f, this.Target.get_sheight_px());
				break;
			}
			case AIM.T:
			case AIM.B:
			{
				float num = this.Target.get_swidth_px() - this.Target.radius * 2f - this.w;
				zero.x = num * pivot;
				zero.y = -(this.Target.get_sheight_px() * 0.5f + this.w * 0.5f) * (float)CAim._YD(this.pop_a, 1);
				float num2 = this.schh - (this.Target.get_sheight_px() * 0.5f - this.Target.radius);
				num = this.Target.get_sheight_px() * 0.5f + this.w;
				this.RcMouseIn.Set(this.Target.transform.localPosition.x * 64f - this.Target.get_swidth_px() * 0.5f, num2 * (float)CAim._YD(this.pop_a, 1) - num, this.Target.get_swidth_px(), num * 2f);
				break;
			}
			}
			IN.PosP2(base.transform, zero.x, zero.y);
			this.finePositionExecute(0f);
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			this.Skin = (this.ErSkin = new ButtonSkinBoxEar(this, this.w, this.w));
			return this.Skin;
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt) || this.Target == null)
			{
				return false;
			}
			if (!base.isActive())
			{
				return true;
			}
			if (IN.use_mouse)
			{
				Vector2 vector = IN.getMousePos(IN.getGUICamera()) * 64f;
				bool flag = X.BTW(this.RcMouseIn.x, vector.x, this.RcMouseIn.xMax) && X.BTW(this.RcMouseIn.y, vector.y, this.RcMouseIn.yMax);
				if (!this.mouse_in)
				{
					if (flag)
					{
						this.mouse_in = true;
						if (this.box_shown && this.t_margin < 0f)
						{
							this.fnHoverEar(null);
						}
					}
				}
				else if (!flag)
				{
					this.mouse_in = false;
					if (this.box_shown && this.t_margin >= 0f)
					{
						this.fnOutEar(null);
					}
				}
			}
			if (X.D)
			{
				if (this.t_margin >= 0f)
				{
					if (this.t_margin < this.show_margin_t)
					{
						this.t_margin += (float)X.AF;
						this.fineColor();
						if (this.t_margin >= this.show_margin_t)
						{
							this.activate();
						}
					}
				}
				else if (this.t_margin > -this.hide_margin_t)
				{
					this.t_margin -= (float)X.AF;
					this.fineColor();
					if (this.t_margin <= -this.hide_margin_t)
					{
						this.deactivate();
					}
				}
			}
			if (IN.isMenuPD(1))
			{
				if (!this.box_shown)
				{
					this.activate();
				}
				else
				{
					this.deactivate();
				}
			}
			if (X.D)
			{
				if (this.t_show >= 0f)
				{
					if (this.t_show < this.ANIM_T)
					{
						this.t_show += (float)X.AF;
						this.finePositionExecute();
					}
				}
				else if (this.t_show > -this.ANIM_T)
				{
					this.t_show -= (float)X.AF;
					this.finePositionExecute();
				}
			}
			return true;
		}

		public void finePosition()
		{
			if (this.t_show >= 0f)
			{
				this.t_show = X.Mn(this.t_show, this.ANIM_T - 1f);
				return;
			}
			this.t_show = X.Mx(this.t_show, -this.ANIM_T + 1f);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.fineColor();
			this.finePositionExecute();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			base.runner_assigned = false;
		}

		public void fineColor()
		{
			if (this.t_margin >= 0f)
			{
				this.fineColor(X.ZSIN(this.t_margin, this.show_margin_t));
				return;
			}
			this.fineColor(X.ZSINV(this.hide_margin_t + this.t_margin, this.hide_margin_t));
		}

		public void finePositionExecute()
		{
			if (this.t_show >= 0f)
			{
				this.finePositionExecute(X.ZSIN2(this.t_show, this.ANIM_T));
				return;
			}
			this.finePositionExecute(1f - X.ZSIN(-this.t_show, this.ANIM_T));
		}

		private bool fnHoverEar(aBtn B)
		{
			this.t_margin = ((this.t_margin < 0f) ? X.MMX(0f, this.show_margin_t + this.t_margin / this.hide_margin_t * this.show_margin_t, this.show_margin_t - 1f) : this.t_margin);
			return true;
		}

		private bool fnOutEar(aBtn B)
		{
			if (!this.box_shown || B == null)
			{
				this.t_margin = ((this.t_margin > 0f) ? (-X.MMX(1f, this.hide_margin_t - this.t_margin / this.show_margin_t * this.hide_margin_t, this.hide_margin_t - 1f)) : this.t_margin);
			}
			return true;
		}

		public void activate()
		{
			if (this.t_show < 0f && this.Target != null)
			{
				float num = this.t_show;
				this.t_show = 0f;
				if (this.FnActvChanged != null && !this.FnActvChanged(this, true))
				{
					this.t_show = num;
					return;
				}
				SND.Ui.play(this.snd_open, false);
				this.Target.bind();
				if (this.t_margin < 0f)
				{
					this.t_margin = 0f;
				}
			}
		}

		public void deactivate()
		{
			if (this.t_show >= 0f && this.Target != null)
			{
				if (this.FnActvChanged != null)
				{
					this.FnActvChanged(this, false);
				}
				SND.Ui.play(this.snd_close, false);
				this.t_show = -1f;
				this.Target.hide();
				if (this.t_margin >= 0f)
				{
					this.t_margin = -1f;
				}
			}
		}

		public void fineColor(float tz)
		{
			if (this.Target != null)
			{
				this.Target.getBox().fineFocusColor(tz, this.box_alpha);
				this.C = this.Target.getBox().getTopColor();
			}
			base.fine_flag = true;
		}

		public void finePositionExecute(float tz)
		{
			if (this.Target == null)
			{
				return;
			}
			Vector3 vector = this.Target.transform.localPosition * 64f;
			switch (this.pop_a)
			{
			case AIM.L:
			{
				float num = this.Target.get_swidth_px();
				vector.x = -this.scwh - num * 0.5f - 1f + tz * (num - this.Target.radius + 1f);
				break;
			}
			case AIM.T:
			{
				float num = this.Target.get_sheight_px();
				vector.y = this.schh + num * 0.5f + 1f - tz * (num - this.Target.radius + 1f);
				break;
			}
			case AIM.R:
			{
				float num = this.Target.get_swidth_px();
				vector.x = this.scwh + num * 0.5f + 1f - tz * (num - this.Target.radius + 1f);
				break;
			}
			case AIM.B:
			{
				float num = this.Target.get_sheight_px();
				vector.y = -this.schh - num * 0.5f - 1f + tz * (num - this.Target.radius + 1f);
				break;
			}
			}
			this.Target.transform.localPosition = vector * 0.015625f;
			this.Target.getBox().position_max_time(1, 1);
			this.Target.posSetA(-1000f, -1000f, vector.x, vector.y, true);
		}

		public float scw
		{
			get
			{
				if (this.scw_ != -1000f)
				{
					return this.scw_;
				}
				return IN.w;
			}
			set
			{
				this.scw_ = value;
				this.finePosition();
			}
		}

		public float sch
		{
			get
			{
				if (this.sch_ != -1000f)
				{
					return this.sch_;
				}
				return IN.h;
			}
			set
			{
				this.sch_ = value;
				this.finePosition();
			}
		}

		public float scwh
		{
			get
			{
				return this.scw * 0.5f;
			}
		}

		public float schh
		{
			get
			{
				return this.sch * 0.5f;
			}
		}

		public void setPF(PxlFrame PF)
		{
			if (this.ErSkin != null)
			{
				this.ErSkin.setMesh(PF);
			}
		}

		public void setPF(string pf_key)
		{
			this.setPF(MTRX.getPF(pf_key));
		}

		public float pict_alpha
		{
			get
			{
				return 0.33f + ((this.t_margin >= 0f) ? X.ZSIN(this.t_margin, this.show_margin_t) : X.ZLINE(this.hide_margin_t + this.t_margin, this.hide_margin_t)) * 0.4f;
			}
		}

		public bool box_shown
		{
			get
			{
				return this.t_show >= 0f;
			}
		}

		protected override void OnDestroy()
		{
			this.Target = null;
			base.OnDestroy();
		}

		private UiBoxDesigner Target;

		private AIM pop_a;

		private ButtonSkinBoxEar ErSkin;

		public Color32 C = MTRX.ColTrnsp;

		public float ANIM_T = 20f;

		public float show_margin_t = 15f;

		public float hide_margin_t = 40f;

		public float t_margin;

		public float t_show;

		private Rect RcMouseIn;

		private bool mouse_in;

		public float box_alpha = 0.6f;

		public string snd_open = "cursor_gear_reset";

		public string snd_close = "";

		public Func<aBtnBoxEar, bool, bool> FnActvChanged;

		private float scw_ = -1000f;

		private float sch_ = -1000f;
	}
}
