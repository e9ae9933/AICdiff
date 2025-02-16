using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class UiGQPointBox : UiBoxDesigner
	{
		public UiGQPointBox createUi(NelM2DBase _M2D, bool create_money = false)
		{
			if (this.M2D != null)
			{
				return this;
			}
			this.M2D = _M2D;
			int current_grank = this.M2D.GUILD.current_grank;
			this.gp_bar = this.M2D.GUILD.gq_point;
			base.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.margin_in_lr = 16f;
			this.margin_in_tb = 22f;
			base.item_margin_x_px = 10f;
			base.item_margin_y_px = 20f;
			this.init();
			float use_w = base.use_w;
			this.FbGLevel = base.addImg(new DsnDataImg
			{
				name = "guildrank",
				text = TX.Get("Guild_rank", ""),
				TxCol = NEL.ColText,
				swidth = use_w,
				sheight = 62f,
				text_margin_x = 10f,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				MI = MTRX.MIicon,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawGuildRank)
			});
			this.FbGLevel.replaceMaterialManual(MTRX.MIicon.getMtr(BLEND.NORMALP3, base.box_stencil_ref_mask));
			this.FbGp = base.Br().addImg(new DsnDataImg
			{
				name = "gp",
				text = " ",
				swidth = use_w,
				sheight = X.Mn(80f, base.use_h - (float)(create_money ? 70 : 0)),
				TxCol = NEL.ColText,
				text_margin_x = 10f,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				html = true,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawGuildPoint)
			});
			if (create_money)
			{
				float num = X.Mn((base.use_h - base.item_margin_y_px) * 0.5f, 16f);
				this.FbReceivable = base.Br().addP(new DsnDataP("", false)
				{
					name = "gq_receivable",
					text = " ",
					swidth = use_w,
					TxCol = NEL.ColText,
					sheight = num,
					text_margin_x = 10f,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.MIDDLE,
					html = true
				}, false);
				this.FbMoney = base.Br().addP(new DsnDataP("", false)
				{
					name = "money",
					text = " ",
					swidth = use_w,
					TxCol = NEL.ColText,
					sheight = num,
					text_margin_x = 10f,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.MIDDLE,
					html = true
				}, false);
				this.fineMoneyString();
				this.fineReceivableString();
			}
			this.fineGuildPointString();
			return this;
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.isAnimating())
			{
				this.runAnimating(fcnt);
			}
			return true;
		}

		private bool fnDrawGuildRank(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			bool flag = false;
			Md.allocUv23(4, false);
			if (this.t_levelup > 0f)
			{
				float num = 1f - X.ZLINE(this.t_levelup, 40f);
				Md.Uv23(Md.ColGrd.White().multiply(num, false).C, false);
				if (num > 0f)
				{
					flag = true;
				}
			}
			else
			{
				Md.Uv23(C32.d2c(0U), false);
			}
			Md.RotaPF(0f, -14f, 2f, 2f, 0f, MTR.AItemGradeStars[20 + this.M2D.GUILD.current_grank], false, false, false, uint.MaxValue, false, 0);
			Md.allocUv23(0, true);
			update_meshdrawer = true;
			return !flag;
		}

		public void fineGuildPointString()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.isAnimating())
				{
					stb.AddTxA("Guild_current_gp", false).TxRpl((int)this.gp_anim);
				}
				else
				{
					stb.AddTxA("Guild_current_gp", false).TxRpl(this.M2D.GUILD.gq_point);
					if (this.gp_obtained + this.gp_obtaining > 0)
					{
						stb.AddTxA("Guild_progress_gp", false).TxRpl(this.gp_obtained + this.gp_obtaining + this.M2D.GUILD.gq_point);
					}
				}
				this.FbGp.Txt(stb);
				this.FbGp.redraw_flag = true;
			}
		}

		public void fineMoneyString()
		{
			if (this.FbMoney != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Clear().AddTxA("Guild_total_reward_money", false).TxRpl(this.isAnimating() ? ((int)this.money_anim) : (this.money_obtained + this.money_obtaining_));
					this.FbMoney.Txt(stb);
				}
			}
		}

		public void fineReceivableString()
		{
			if (this.FbReceivable != null && this.M2D != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Clear().AddTxA("Guild_detail_receivable_quest", false).TxRpl(this.gq_receivable_)
						.TxRpl(this.M2D.GUILD.getRecievableGQ(this.M2D.GUILD.current_grank));
					this.FbReceivable.Txt(stb);
				}
				this.FbGLevel.redraw_flag = true;
			}
		}

		private bool fnDrawGuildPoint(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (this.FbGp == null)
			{
				return false;
			}
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTRX.MIicon.getMtr(FI.stencil_ref), false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			Md.chooseSubMesh(0, false, false);
			float num = X.NI(-28f, FI.get_sheight_px() * 0.5f - 45f, 0.5f);
			float num2 = FI.get_swidth_px() - 30f;
			float num3 = 44f;
			int num4 = (int)(num2 - 48f);
			float num5 = 16f;
			bool flag = false;
			Md.Col = C32.MulA(4279966491U, alpha * 0.7f);
			Md.KadomaruRect(0f, num, num2, num3, 40f, 0f, false, 0f, 0f, false);
			Md.Col = Md.ColGrd.Set(4292539256U).mulA(alpha).C;
			float num6;
			float num7;
			float num8;
			if (this.isAnimating())
			{
				num6 = this.gp_anim;
				num7 = 0f;
				num8 = 0f;
			}
			else
			{
				num6 = (float)this.gp_bar;
				num7 = (float)this.gp_obtained;
				num8 = (float)this.gp_obtaining;
			}
			int current_grank = this.M2D.GUILD.current_grank;
			int num9 = this.M2D.GUILD.grank_start_point(current_grank);
			int num10 = this.M2D.GUILD.grank_start_point(current_grank + 1);
			if (num9 == num10)
			{
				Md.Rect(0f, num, (float)(num4 + 6), num5 + 6f, false);
			}
			else
			{
				Color32 col = Md.Col;
				float num11 = num6 - (float)num9;
				float num12 = (float)(-(float)num4) * 0.5f;
				float num13 = num - num5 * 0.5f;
				num4++;
				int num14 = num4;
				if (num11 > 0f)
				{
					int num15 = (int)((float)num4 * num11 / (float)(num10 - num9));
					Md.RectBL(num12, num13, (float)num15, num5, false);
					num12 += (float)num15;
					num14 -= num15;
				}
				if (num7 > 0f && num14 > 0)
				{
					Md.Col = Md.ColGrd.Set(4286114291U).mulA(alpha).C;
					int num16 = (int)X.Mn((float)num14, (float)num4 * num7 / (float)(num10 - num9));
					Md.RectBL(num12, num13, (float)num16, num5, false);
					num12 += (float)num16;
					num14 -= num16;
				}
				if (num8 > 0f && num14 > 0)
				{
					float num17 = 0.125f + 0.6f * X.Abs(X.COSIT(220f));
					Md.Col = Md.ColGrd.Set(4286114291U).mulA(alpha * num17).C;
					int num18 = (int)X.Mn((float)num14, (float)num4 * num8 / (float)(num10 - num9));
					Md.RectBL(num12, num13, (float)num18, num5, false);
					num12 += (float)num18;
					num14 -= num18;
					flag = true;
				}
				Md.Col = col;
				Md.Box(0f, num, (float)(num4 - 1 + 6), num5 + 6f, 3f, false);
			}
			if (this.t_levelup >= 0f)
			{
				flag = true;
				Md.chooseSubMesh(1, false, false);
				float num19 = X.ZLINE(this.t_levelup, 80f);
				if (num19 >= 1f)
				{
					this.t_levelup = -1f;
				}
				else
				{
					bool flag2 = this.gp_obtaining >= 0;
					float num20 = 20f - 14f * (1f - X.ZSIN(this.t_levelup, 30f));
					float num21 = X.ZLINE(this.t_levelup, 22f);
					X.ZLINE(1f - num19, 0.35f);
					Md.Col = ((X.ANMPT(24, 2f) < 1f) ? C32.MulA(flag2 ? 4282077951U : 4290781724U, alpha) : C32.MulA(flag2 ? 4282116351U : 4288038981U, alpha));
					using (STB stb = TX.PopBld(flag2 ? "RANK UP!" : "RANK DOWN...", 0))
					{
						MTRX.ChrL.DrawScaleStringTo(Md, stb, 0f, num20, 1f, 1f, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
					}
					Md.chooseSubMesh(0, false, false);
					if (num21 < 1f)
					{
						Md.Col = Md.ColGrd.White().mulA(1f - num21).C;
						Md.Rect(0f, num, (float)(num4 + 6), num5 + 6f, false);
					}
				}
			}
			return !flag;
		}

		public void initAnimation(List<GuildManager.GQEntry> AGq, bool success, float delay = 0f)
		{
			this.activate();
			SND.loadSheets("minigame", "_GUILD");
			this.money_obtaining_ = 0;
			this.gp_obtaining = (this.gp_obtained = 0);
			int count = AGq.Count;
			for (int i = 0; i < count; i++)
			{
				GuildManager.GQEntry gqentry = AGq[i];
				this.gp_obtaining += (success ? gqentry.reward_gp : (-gqentry.lost_gp));
			}
			if (success)
			{
				COOK.CurAchive.Add(ACHIVE.MENT.city_gq_rank, this.gp_obtaining);
			}
			else
			{
				COOK.CurAchive.Add(ACHIVE.MENT.city_gq_rank_reduce, X.Abs(this.gp_obtaining));
			}
			this.money_obtaining_ = 0;
			this.t_anm = -delay;
			this.t_snd = 0f;
			this.t_levelup = -1f;
			this.rank_sound_played_bits = 0U;
			if (success && this.M2D.GUILD.current_grank >= 4)
			{
				this.rank_sound_played_bits |= 32U;
			}
			this.money_anim = (float)this.money_obtaining_;
			this.gp_anim = (float)this.gp_bar;
			this.fineMoneyString();
			this.fineGuildPointString();
			this.fineReceivableString();
		}

		public void runAnimating(float fcnt)
		{
			if (IN.isMenuO(0))
			{
				fcnt *= 3f;
			}
			else if (IN.isCancelOn(0))
			{
				fcnt *= 2f;
			}
			if (IN.isSubmitOn(0))
			{
				fcnt *= 2f;
			}
			if (this.t_levelup >= 0f)
			{
				this.t_levelup += fcnt;
			}
			this.t_anm += fcnt;
			if (this.t_snd > 0f)
			{
				this.t_snd -= fcnt;
			}
			bool flag = this.gp_obtaining > 0;
			if (this.t_anm < 0f)
			{
				return;
			}
			if (this.t_anm < 1000f)
			{
				float num = 60f + (float)this.gp_obtaining * 2.5f;
				this.gp_anim = X.MMX(0f, (float)this.gp_bar + (float)this.gp_obtaining * X.ZLINE(this.t_anm, num), 150f);
				if (this.t_anm <= num + 20f)
				{
					this.fineGuildPointString();
					this.FbGp.redraw_flag = true;
				}
				if (this.t_anm <= num && this.t_snd <= 0f)
				{
					this.t_snd = 3f;
					SND.Ui.play("mg_score_add_counter", false);
				}
				if (flag)
				{
					for (int i = 0; i < 3; i++)
					{
						int num2 = this.M2D.GUILD.current_grank;
						int num3 = this.M2D.GUILD.grank_start_point(num2 + 1);
						if (this.gp_anim < (float)num3 || (this.rank_sound_played_bits & (1U << num2 + 1)) != 0U)
						{
							break;
						}
						this.rank_sound_played_bits |= 1U << num2 + 1;
						if (this.Con != null)
						{
							this.Con.levelupcount++;
						}
						this.FbGp.redraw_flag = true;
						this.M2D.GUILD.gq_point = X.IntR(this.gp_anim);
						this.fineReceivableString();
						SND.Ui.play("mg_bell_great", false);
						this.t_levelup = 0f;
					}
				}
				else
				{
					for (int j = 0; j < 3; j++)
					{
						int num2 = this.M2D.GUILD.current_grank;
						int num4 = this.M2D.GUILD.grank_start_point(num2);
						if (this.gp_anim >= (float)num4 || (this.rank_sound_played_bits & (1U << num2)) != 0U)
						{
							break;
						}
						this.rank_sound_played_bits |= 1U << num2;
						if (this.Con != null)
						{
							this.Con.levelupcount--;
						}
						this.FbGp.redraw_flag = true;
						this.M2D.GUILD.gq_point = (int)this.gp_anim;
						this.fineReceivableString();
						SND.Ui.play("guildq_leveldown", false);
						this.t_levelup = 0f;
					}
				}
				if (this.t_anm >= num + 100f)
				{
					this.gp_anim = (float)X.Mx(0, this.gp_bar + this.gp_obtaining);
					this.gp_obtaining = 0;
					this.M2D.GUILD.gq_point = X.IntR(this.gp_anim);
					this.t_anm = 1000f;
					this.t_snd = 0f;
					return;
				}
			}
			else
			{
				float num5 = this.t_anm - 1000f;
				float num6;
				if (this.money_obtaining_ == 0)
				{
					num6 = -200f;
				}
				else
				{
					num6 = 30f + (float)this.money_obtaining_ * 0.33f;
					int num7 = (int)this.money_anim;
					this.money_anim = (float)this.money_obtaining_ * (1f - X.ZLINE(num5, num6));
					int num8 = (int)this.money_anim;
					if (num8 < num7)
					{
						CoinStorage.addCount(num7 - num8, CoinStorage.CTYPE.GOLD, true);
						this.fineMoneyString();
					}
				}
				if (num5 >= num6 + 120f)
				{
					this.gp_bar = X.IntR(this.gp_anim);
					this.money_obtaining_ = 0;
					this.gp_obtaining = 0;
					this.t_anm = -1000f;
					this.fineGuildPointString();
					this.fineMoneyString();
					if (this.FD_fnAnimateFinished != null)
					{
						this.FD_fnAnimateFinished();
					}
				}
			}
		}

		public int money_obtaining
		{
			get
			{
				return this.money_obtaining_;
			}
			set
			{
				this.money_obtaining_ = value;
				this.fineMoneyString();
			}
		}

		public int gq_receivable
		{
			get
			{
				return this.gq_receivable_;
			}
			set
			{
				if (this.gq_receivable == value)
				{
					return;
				}
				this.gq_receivable_ = value;
				this.fineReceivableString();
			}
		}

		public bool isAnimating()
		{
			return this.t_anm > -1000f;
		}

		public UiGQManageBox Con;

		private NelM2DBase M2D;

		private FillImageBlock FbGLevel;

		private FillImageBlock FbGp;

		private FillBlock FbMoney;

		private FillBlock FbReceivable;

		private int gq_receivable_;

		public int gp_obtained;

		public int gp_obtaining;

		public int money_obtained;

		private int money_obtaining_;

		private int gp_bar;

		public float gp_anim;

		public float money_anim;

		private uint rank_sound_played_bits;

		public float t_anm = -1000f;

		public float t_snd;

		public float t_levelup = -1f;

		public Action FD_fnAnimateFinished;
	}
}
