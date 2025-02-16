using System;

namespace XX
{
	public abstract class ButtonSkinDesc : ButtonSkin
	{
		public ButtonSkinDesc(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
		}

		public string getDesc()
		{
			if (this.desc_ == null)
			{
				this.desc_ = X.T2K(this.title, "");
			}
			return this.desc_;
		}

		public bool use_desc
		{
			get
			{
				return this.B.isHovered() && !base.isPushed();
			}
		}

		public static aBtn setDesc(aBtn B, string s)
		{
			ButtonSkinDesc buttonSkinDesc = B.get_Skin() as ButtonSkinDesc;
			if (buttonSkinDesc != null)
			{
				buttonSkinDesc.desc = s;
			}
			return B;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			string text = "";
			if (this.use_desc)
			{
				text = this.getDesc();
			}
			if (text != "")
			{
				ButtonSkinDesc.DescObj = this;
				bool flag = false;
				if (ButtonSkinDesc.Fl == null)
				{
					ButtonSkinDesc.Fl = IN.CreateGob(IN._stage, "-FL").AddComponent<FillBlock>();
					flag = true;
					ButtonSkinDesc.Fl.margin_x = 28f;
					ButtonSkinDesc.Fl.radius = 100f;
					ButtonSkinDesc.Fl.margin_y = 4f;
					ButtonSkinDesc.Fl.size = 14f;
					ButtonSkinDesc.Fl.TxCol = MTRX.ColWhite;
					ButtonSkinDesc.Fl.Col = MTRX.ColMenu;
					ButtonSkinDesc.Fl.StartFb(text, null, true);
				}
				else
				{
					ButtonSkinDesc.Fl.text_content = text;
				}
				ButtonSkinDesc.Fl.alpha = this.alpha_;
				ButtonSkinDesc.Fl.enabled = this.MyMeshRenderer.enabled;
				if (flag)
				{
					ButtonSkinDesc.Fl.Start();
					IN.setZAbs(ButtonSkinDesc.Fl.transform, -9.4f);
				}
				IN.Pos2GoSide(ButtonSkinDesc.Fl, this.desc_aim_bit, 8f, 5f, this.B);
				if (this.use_desc_position_check)
				{
					IN.Pos2SetNotGoBeyondAbs(ButtonSkinDesc.Fl, 1f, 4f);
				}
				ButtonSkinDesc.DescObj = this;
			}
			else if (this.isDescShowing())
			{
				ButtonSkinDesc.Fl.enabled = false;
			}
			return base.Fine();
		}

		public override void destruct()
		{
			if (this.isDescShowing())
			{
				ButtonSkinDesc.Fl.enabled = false;
				ButtonSkinDesc.DescObj = null;
			}
		}

		public override void bindChanged(bool f)
		{
			base.bindChanged(f);
			if (this.isDescShowing())
			{
				ButtonSkinDesc.Fl.enabled = f;
			}
		}

		public bool isDescShowing()
		{
			return ButtonSkinDesc.Fl != null && ButtonSkinDesc.DescObj == this;
		}

		public string desc
		{
			get
			{
				return this.desc_;
			}
			set
			{
				if (value != this.desc_)
				{
					this.desc_ = value;
					if (this.isDescShowing())
					{
						string desc = this.getDesc();
						if (desc != null)
						{
							ButtonSkinDesc.Fl.text_content = desc;
						}
					}
				}
			}
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					if (this.isDescShowing())
					{
						ButtonSkinDesc.Fl.alpha = this.alpha_;
					}
					base.alpha = value;
				}
			}
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
			if (this.isDescShowing())
			{
				ButtonSkinDesc.Fl.enabled = f;
			}
		}

		protected static FillBlock Fl;

		private string desc_;

		public int desc_aim_bit = 8;

		public bool use_desc_position_check;

		public static ButtonSkinDesc DescObj;
	}
}
