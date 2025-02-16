using System;
using XX;

namespace nel
{
	public class UiBoxDesignerHelp : UiBoxDesigner
	{
		protected override void Awake()
		{
			base.Awake();
			this.margin_in_lr = 58f;
			this.margin_in_tb = 10f;
			base.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			base.item_margin_x_px = (base.item_margin_y_px = 0f);
		}

		public UiBoxDesignerHelp InitDH(FnBtnBindings FnClick = null)
		{
			this.BtnUnd = IN.CreateGob(base.gameObject, "-Und").AddComponent<aBtnNel>();
			this.BtnUnd.unselectable(true);
			this.BtnUnd.w = 200f;
			this.BtnUnd.h = 70f;
			ButtonSkinNelUi buttonSkinNelUi = this.BtnUnd.initializeSkin("row_center", "") as ButtonSkinNelUi;
			buttonSkinNelUi.fix_text_size = 18f;
			buttonSkinNelUi.setTitle(TX.Get("Button_Help", ""));
			buttonSkinNelUi.addIcon(MTRX.getPF("help_hatena"), -1);
			buttonSkinNelUi.icon_col_normal = 4283780170U;
			if (FnClick != null)
			{
				this.BtnUnd.addClickFn(FnClick);
			}
			this.BtnUnd.bind();
			this.WH(this.undetail_w, this.undetail_h);
			this.posSetDefault();
			if (!base.initted)
			{
				this.init();
			}
			return this;
		}

		public void posSetDefault()
		{
			base.posSetDA(this.undetail_x, this.undetail_y, 0, 240f, true);
		}

		public void activateDetail()
		{
			if (this.t_detail < 0f)
			{
				this.t_detail = 0f;
				base.posSetA(this.undetail_x, this.undetail_y, this.detail_x, this.detail_y, false);
				base.getBox().frametype = UiBox.FRAMETYPE.MAIN;
				base.WHanim(this.detail_fb_w + this.margin_in_lr * 2f, this.detail_fb_h + ((this.detail_title_tx_key != null) ? this.detail_title_fb_h : 0f) + this.margin_in_tb * 2f, true, true);
				if (this.FbDetail != null)
				{
					this.FbDetail.gameObject.SetActive(true);
					if (this.FbDetailTitle != null)
					{
						this.FbDetailTitle.gameObject.SetActive(true);
					}
				}
				else
				{
					this.init();
					if (this.detail_title_tx_key != null)
					{
						this.FbDetailTitle = base.addP(new DsnDataP("", false)
						{
							text = TX.Get(this.detail_title_tx_key, ""),
							swidth = this.detail_fb_w,
							sheight = this.detail_title_fb_h,
							text_margin_x = this.detail_fb_margin_lr,
							text_margin_y = 3f,
							TxCol = C32.d2c(4283780170U),
							size = 26f,
							html = true
						}, false);
					}
					this.FbDetail = base.addP(new DsnDataP("", false)
					{
						text = TX.Get(this.detail_tx_key, ""),
						swidth = this.detail_fb_w,
						sheight = this.detail_fb_h,
						text_margin_x = this.detail_fb_margin_lr,
						text_margin_y = this.detail_fb_margin_tb,
						TxCol = C32.d2c(4283780170U),
						size = 20f,
						html = true
					}, false);
				}
				this.BtnUnd.hide();
				this.BtnUnd.gameObject.SetActive(false);
			}
		}

		public void deactivateDetail()
		{
			if (this.t_detail >= 0f)
			{
				this.t_detail = -1f;
				base.position(-1000f, -1000f, this.undetail_x, this.undetail_y, false);
				base.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				base.WHanim(this.undetail_w, this.undetail_h, true, true);
				if (this.FbDetail != null)
				{
					this.FbDetail.gameObject.SetActive(false);
				}
				if (this.FbDetailTitle != null)
				{
					this.FbDetailTitle.gameObject.SetActive(false);
				}
				this.BtnUnd.bind();
				this.BtnUnd.gameObject.SetActive(true);
			}
		}

		public bool isShowingDetail()
		{
			return this.t_detail >= 0f;
		}

		public float undetail_w
		{
			get
			{
				return this.BtnUnd.w + this.margin_in_lr * 2f;
			}
		}

		public float undetail_h
		{
			get
			{
				return this.BtnUnd.h + this.margin_in_tb * 2f;
			}
		}

		public float undetail_x
		{
			get
			{
				return IN.wh - (this.undetail_w * 0.5f - this.topbtn_clip_x) + this.undetail_shiftx;
			}
		}

		public void executeClickDetailBtn()
		{
			if (!this.isShowingDetail())
			{
				this.BtnUnd.ExecuteOnClick();
			}
		}

		public float t_detail = -1f;

		private const float MAXT_FIRST_SHOW = 30f;

		private const float MAXT_FADE = 30f;

		public float topbtn_clip_x = 26f;

		public float undetail_y = -IN.hh * 0.6f;

		public float undetail_shiftx;

		public float detail_x;

		public float detail_y;

		public string detail_tx_key;

		public string detail_title_tx_key;

		public float detail_fb_w = IN.w * 0.7f;

		public float detail_fb_h = IN.h * 0.45f;

		public float detail_title_fb_h = IN.h * 0.08f;

		public float detail_fb_margin_lr;

		public float detail_fb_margin_tb;

		private aBtnNel BtnUnd;

		private FillBlock FbDetail;

		private FillBlock FbDetailTitle;
	}
}
