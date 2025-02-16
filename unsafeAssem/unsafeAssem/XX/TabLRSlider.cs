using System;
using UnityEngine;

namespace XX
{
	public class TabLRSlider
	{
		public TabLRSlider(Designer _Ds, float _outw, float _tab_h, bool _use_scroll = false)
		{
			this.Ds = _Ds;
			this.tab_h = ((_tab_h <= 0f) ? 28f : _tab_h);
			this.use_scroll = _use_scroll;
			this.outw = ((_outw <= 0f) ? (this.Ds.use_w + _outw) : _outw);
		}

		public float between_bounds_w
		{
			get
			{
				return this.outw - 60f - this.Ds.item_margin_x_px * 2f;
			}
		}

		public FillBlock setL()
		{
			if (this.DsnLr == null)
			{
				this.DsnLr = new DsnDataImg
				{
					html = true,
					size = 14f,
					swidth = 30f,
					aligny = ALIGNY.MIDDLE,
					text_auto_condense = false,
					text_auto_wrap = false,
					Col = MTRX.ColTrnsp
				};
			}
			this.DsnLr.FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawInFIB);
			this.DsnLr.sheight = this.tab_h - (this.use_scroll ? 6.6000004f : 0f);
			this.DsnLr.Text((!this.alloc_ltab_rtab_input) ? (this.alloc_lr_arrow_input ? "<key la/>" : "<key rem/>") : "<key ltab/>");
			this.PLeft = this.Ds.addImg(this.DsnLr);
			return this.PLeft;
		}

		public FillBlock setR()
		{
			this.PRight = this.Ds.addP(this.DsnLr.Text((!this.alloc_ltab_rtab_input) ? (this.alloc_lr_arrow_input ? "<key ra/>" : "<key add/>") : "<key rtab/>"), false);
			return this.PRight;
		}

		public byte lr_input
		{
			get
			{
				return this.lr_input_;
			}
			set
			{
				if (this.lr_input == value)
				{
					return;
				}
				int num = (int)this.lr_input_;
				this.lr_input_ = value;
				if (num == 0 != (value == 0))
				{
					if (value > 0)
					{
						if (this.t_anm > -1024 && this.t_anm != 0)
						{
							this.fineAnm(15);
						}
						this.t_anm = -1025;
					}
					this.PLeft.gameObject.SetActive(value > 0);
					this.PRight.gameObject.SetActive(value > 0);
				}
				if (value > 0)
				{
					this.PLeft.text_alpha_multiple = (((value & 1) != 0) ? 1f : 0.6f);
					this.PRight.text_alpha_multiple = (((value & 2) != 0) ? 1f : 0.6f);
				}
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.PLeft.use_valotile;
			}
			set
			{
				this.PLeft.use_valotile = value;
				this.PRight.use_valotile = value;
			}
		}

		public void setVisible(bool visibility)
		{
			DesignerRowMem.DsnMem designerBlockMemory = this.Ds.getDesignerBlockMemory(this.PLeft);
			if (designerBlockMemory.active != visibility)
			{
				designerBlockMemory.active = visibility;
				this.t_anm = 0;
			}
			this.Ds.getDesignerBlockMemory(this.PRight).active = visibility;
		}

		private bool fnDrawInFIB(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			update_meshdrawer = false;
			return this.fineAnm(X.AF);
		}

		private bool fineAnm(int fcnt)
		{
			if (this.t_anm > -1024)
			{
				if (this.t_anm > 0)
				{
					this.t_anm = X.Mx(this.t_anm - fcnt, 0);
					this.PRight.transform.localScale = Vector2.one * X.NIL(1f, 2f, (float)this.t_anm, 15f);
				}
				else if (this.t_anm < 0)
				{
					this.t_anm = X.Mn(this.t_anm + fcnt, 0);
					this.PLeft.transform.localScale = Vector2.one * X.NIL(1f, 2f, (float)(-(float)this.t_anm), 15f);
				}
				return this.t_anm == 0;
			}
			return true;
		}

		public bool runLRInput(int move = -2, bool force = false)
		{
			int num = (int)(force ? 3 : this.lr_input);
			if (num == 0)
			{
				return false;
			}
			if (this.t_anm <= -1024)
			{
				this.t_anm++;
				if (this.t_anm > -1024)
				{
					this.t_anm = 0;
				}
			}
			else
			{
				if (move <= -2)
				{
					move = 0;
					if (((this.alloc_ltab_rtab_input && IN.isRTabPD()) || (this.alloc_add_rem_input && IN.isUiAddPD()) || (this.alloc_lr_arrow_input && IN.isR())) && (num & 2) != 0)
					{
						move++;
					}
					else if (((this.alloc_ltab_rtab_input && IN.isLTabPD()) || (this.alloc_add_rem_input && IN.isUiRemPD()) || (this.alloc_lr_arrow_input && IN.isL())) && (num & 1) != 0)
					{
						move--;
					}
				}
				if (move != 0)
				{
					if (this.FD_slided == null || this.FD_slided(move))
					{
						if (TX.valid(this.click_snd))
						{
							SND.Ui.play(this.click_snd, false);
						}
						if (this.t_anm != 0 && this.t_anm > 0 != move > 0)
						{
							this.fineAnm(15);
						}
						this.t_anm = move * 15;
						this.fineAnm(0);
						this.PLeft.redraw_flag = true;
					}
					return true;
				}
			}
			return false;
		}

		public string click_snd = "toggle_button_open";

		public bool alloc_add_rem_input;

		public bool alloc_lr_arrow_input;

		public bool alloc_ltab_rtab_input = true;

		private string lkey;

		private string rkey;

		private readonly Designer Ds;

		private FillImageBlock PLeft;

		private FillBlock PRight;

		public readonly float outw;

		public readonly float tab_h;

		private readonly bool use_scroll;

		public const float tab_lr_input_width = 30f;

		public const float tab_h_default = 28f;

		private byte lr_input_ = 3;

		private int t_anm;

		public const int ANM_MAXT = 15;

		private DsnDataImg DsnLr;

		public Func<int, bool> FD_slided;
	}
}
