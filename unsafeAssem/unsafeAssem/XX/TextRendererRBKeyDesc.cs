using System;
using UnityEngine;

namespace XX
{
	public class TextRendererRBKeyDesc : TextRenderer, IRunAndDestroy
	{
		protected override void Awake()
		{
			base.Awake();
			base.BorderCol(MTRX.ColBlack);
			base.gameObject.layer = IN.LAY(IN.gui_layer_name);
			this.Alpha(1f);
			base.LineSpacing(1.4f);
			base.html_mode = true;
			this.fineCenterTextOption();
			this.Reposit();
			this.MainCol = this.Style.MyCol;
		}

		private void fineCenterTextOption()
		{
			base.Align(this.top_center_ ? ALIGN.LEFT : ALIGN.RIGHT);
			base.AlignY(this.top_center_ ? ALIGNY.MIDDLE : ALIGNY.BOTTOM);
		}

		public virtual bool run(float fcnt)
		{
			if (!this.redraw_flag && X.D && this.shake_t > 0)
			{
				this.shake_t -= X.AF;
				this.need_reposit = true;
				float num = X.ZSIN((float)this.shake_t, 50f);
				base.Col(MTRX.cola.Set(this.MainCol).blend(4294921806U, num).C);
			}
			if (this.need_reposit)
			{
				this.Reposit();
			}
			return true;
		}

		public void Reposit()
		{
			this.need_reposit = false;
			this.fineCenterTextOption();
			float num = 0f;
			if (this.shake_t > 0)
			{
				num = X.ZSIN((float)this.shake_t, 50f) * X.COSI((float)(50 - this.shake_t), 13f) * 21f;
			}
			if (this.top_center)
			{
				IN.PosP2(base.transform, num + this.ui_shift_x - base.get_swidth_px() * 0.5f, this.shift_pixel_y_ + IN.h * 0.275f);
				return;
			}
			IN.PosP2(base.transform, num + (IN.wh - 20f) + ((this.ui_shift_x < 0f) ? (this.ui_shift_x * 2f) : 0f), this.shift_pixel_y_ - IN.hh + 10f);
		}

		public bool top_center
		{
			get
			{
				return this.top_center_;
			}
			set
			{
				if (this.top_center == value)
				{
					return;
				}
				this.top_center_ = value;
				this.fineCenterTextOption();
				this.need_reposit = true;
			}
		}

		public float ui_shift_x
		{
			get
			{
				return this.ui_shift_x_;
			}
			set
			{
				if (this.ui_shift_x == value)
				{
					return;
				}
				float num = this.ui_shift_x_;
				this.ui_shift_x_ = value;
				if (this.top_center_ || this.ui_shift_x_ <= 0f || num <= 0f)
				{
					this.need_reposit = true;
				}
			}
		}

		public float shift_pixel_y
		{
			get
			{
				return this.shift_pixel_y_;
			}
			set
			{
				if (this.shift_pixel_y == value)
				{
					return;
				}
				this.shift_pixel_y_ = value;
				this.need_reposit = true;
			}
		}

		public override TextRenderer Col(Color32 C)
		{
			base.Col(C);
			this.MainCol = C;
			return this;
		}

		public void Shake()
		{
			this.shake_t = 50;
		}

		public void stopShake()
		{
			if (this.shake_t > 0)
			{
				this.shake_t = X.Mn(this.shake_t, 1);
			}
		}

		public bool isShaking()
		{
			return this.shake_t > 0;
		}

		public override string ToString()
		{
			return "TxRBDesc";
		}

		public override void OnEnable()
		{
			base.OnEnable();
			this.runner_assigned = true;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			this.runner_assigned = false;
		}

		public void destruct()
		{
			this.OnDestroy();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.OnDisable();
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					IN.addRunner(this);
					return;
				}
				IN.remRunner(this);
			}
		}

		private int shake_t;

		private Color32 MainCol;

		public bool need_reposit;

		private bool top_center_;

		private float ui_shift_x_;

		public const float SIZE_DEFAULT = 14f;

		private float shift_pixel_y_;

		protected bool runner_assigned_;
	}
}
