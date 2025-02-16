using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiBoxMoney : UiBoxDesigner
	{
		public override void destruct()
		{
			base.destruct();
			CoinStorage.FD_MoneyChanged -= this.FD_MoneyChanged;
			if (this.M2D != null)
			{
				this.M2D.remValotAddition(this);
			}
		}

		public void Start()
		{
			this.started = true;
			this.M2D = M2DBase.Instance as NelM2DBase;
			if (this.M2D != null)
			{
				this.M2D.addValotAddition(this);
			}
			if (this.Fb == null)
			{
				this.WH(200f, 44f);
				this.margin_in_lr = 30f;
				base.item_margin_x_px = 0f;
				this.margin_in_tb = 8f;
				this.Bx.frametype = UiBox.FRAMETYPE.ONELINE;
				base.positionD((float)CAim._XD(this.set_aim, 1) * (IN.wh - this.w * 0.5f + this.h * 0.5f + 4f), IN.hh * 0.05f + (float)CAim._YD(this.set_aim, 1) * IN.hh * 0.83f, (CAim._XD(this.set_aim, 1) > 0) ? 0 : 2, this.w + 30f);
				this.init();
				this.FD_MoneyChanged = new CoinStorage.FnMoneyChanged(this.MoneyChanged);
				this.coin_cache = CoinStorage.getCount(this.ctype);
				this.coin_anim_val = 0f;
				CoinStorage.FD_MoneyChanged += this.FD_MoneyChanged;
				float num = this.h - this.margin_in_tb * 2f;
				base.addP(new DsnDataP("", false)
				{
					text = CoinStorage.getIconHtml(this.ctype, 30),
					html = true,
					sheight = num,
					swidth = 30f,
					text_margin_x = 0f,
					text_margin_y = 0f
				}, false);
				this.Fb = base.addP(new DsnDataP("", false)
				{
					text = this.coin_cache.ToString(),
					text_margin_x = 8f,
					text_margin_y = 0f,
					swidth = base.use_w - 10f,
					sheight = num,
					size = 20f,
					TxCol = C32.d2c(4283780170U),
					alignx = ALIGN.RIGHT
				}, false);
			}
		}

		public override Designer activate()
		{
			if (this.Fb == null)
			{
				this.Start();
			}
			base.activate();
			this.t_anim_pre = 0;
			this.quitEnough();
			this.coin_cache = CoinStorage.getCount(this.ctype);
			this.need_fine_text_content = true;
			return this;
		}

		public void initEnough()
		{
			this.t_notenough = 0f;
		}

		public void quitEnough()
		{
			if (this.t_notenough >= 0f)
			{
				this.t_notenough = -1f;
				this.Fb.TxCol = C32.d2c(4283780170U);
				this.Fb.margin_x = 8f;
				this.Fb.fineTxTransform();
			}
		}

		public override bool run(float fcnt)
		{
			if (!this.started)
			{
				this.Start();
			}
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.t_anim_pre != 0 && IN.totalframe >= this.t_anim_pre + 2)
			{
				SND.Ui.play("adding_coin", false);
				this.t_anim_pre = IN.totalframe;
				this.coin_cache = X.VALWALK(this.coin_cache, CoinStorage.getCount(this.ctype), this.coin_anim_val);
				if (this.coin_cache == CoinStorage.getCount(this.ctype))
				{
					this.t_anim_pre = 0;
				}
				this.need_fine_text_content = true;
			}
			if (this.t_notenough >= 0f)
			{
				this.t_notenough += fcnt;
				this.need_fine_text_content = true;
				if (this.t_notenough >= 35f)
				{
					this.quitEnough();
				}
			}
			if (this.need_fine_text_content)
			{
				this.need_fine_text_content = false;
				using (STB stb = TX.PopBld(null, 0))
				{
					if (this.t_notenough >= 0f)
					{
						float num = 1f - X.ZPOW(this.t_notenough - 8f, 27f);
						this.Fb.margin_x = 8f + (float)X.IntR(X.SINI(this.t_notenough + 4f, 14f) * num * 12.4f);
						this.Fb.fineTxTransform();
						Color32 c = MTRX.colb.Set(4283780170U).blend(4294901760U, num).mulA(base.alpha)
							.C;
						this.Fb.TxCol = c;
					}
					stb.Add(X.IntR(this.coin_cache));
					this.Fb.Txt(stb);
				}
			}
			return true;
		}

		private void MoneyChanged(CoinStorage.CTYPE ctype, int added)
		{
			if (this.Fb == null)
			{
				this.Start();
			}
			if (ctype == CoinStorage.CTYPE.GOLD)
			{
				this.t_anim_pre = IN.totalframe - 2;
				this.coin_anim_val = (float)X.Mx(1, X.IntC((float)X.Abs(added) / 15f));
			}
		}

		public void reduceCount(int c)
		{
			CoinStorage.reduceCount(c, this.ctype);
		}

		private CoinStorage.FnMoneyChanged FD_MoneyChanged;

		private CoinStorage.CTYPE ctype;

		private FillBlock Fb;

		private float coin_cache;

		private float coin_anim_val;

		private int t_anim_pre;

		private float t_notenough = -1f;

		private bool need_fine_text_content;

		private const float MAXT_NOTENOUGH = 35f;

		private const float DEFAULT_TEXT_MARGIN = 8f;

		public AIM set_aim = AIM.TR;

		private bool started;

		private NelM2DBase M2D;
	}
}
