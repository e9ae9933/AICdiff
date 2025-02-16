using System;

namespace XX
{
	public class ScrollAppend
	{
		public ScrollAppend(int _stencil_ref, float _out_w, float _out_h, float _in_margin_w = 4f, float _in_margin_h = 6f, int _lr_slide_row = 0)
		{
			this.stencil_ref = _stencil_ref;
			this.out_w_ = _out_w;
			this.out_h_ = _out_h;
			this.in_margin_w = _in_margin_w;
			this.in_margin_h = _in_margin_h;
			this.lr_slide_row = _lr_slide_row;
		}

		public ScrollAppend(ScrollAppend Base)
			: this(Base.stencil_ref, Base.out_w_, Base.out_h_, Base.in_margin_w, Base.in_margin_h, Base.lr_slide_row)
		{
		}

		public virtual float out_w
		{
			get
			{
				return this.out_w_;
			}
			set
			{
				if (this.out_w == value)
				{
					return;
				}
				this.out_w_ = value;
			}
		}

		public virtual float out_h
		{
			get
			{
				return this.out_h_;
			}
			set
			{
				if (this.out_h == value)
				{
					return;
				}
				this.out_h_ = value;
			}
		}

		protected float out_w_;

		protected float out_h_;

		public float in_margin_w;

		public float in_margin_h;

		public int stencil_ref;

		public int lr_slide_row;
	}
}
