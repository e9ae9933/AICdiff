using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UiGQCard : UiBoxDesigner
	{
		public static float defaulth
		{
			get
			{
				return IN.h * 0.34f;
			}
		}

		public static float defaultw
		{
			get
			{
				return IN.w - 400f;
			}
		}

		public float top_left_w
		{
			get
			{
				return UiGQCard.defaultw * 0.6f;
			}
		}

		public UiGQCard.STATE state { get; private set; }

		public GuildManager.GQEntry SelectedGQ { get; private set; }

		private void createUi()
		{
			if (this.M2D == null)
			{
				this.M2D = M2DBase.Instance as NelM2DBase;
			}
			this.margin_in_lr = 34f;
			base.item_margin_x_px = 2f;
			this.margin_in_tb = 8f;
			this.init();
			float use_h = base.use_h;
			Designer designer = this.addTab("T_L", this.top_left_w, use_h, this.top_left_w, use_h, false);
			designer.Smallest();
			designer.margin_in_tb = 12f;
			designer.init();
			this.FbName = base.addP(new DsnDataP("", false)
			{
				name = "_gq_name",
				text = "  ",
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w - 80f,
				size = 16f,
				html = true,
				text_auto_wrap = false
			}, false);
			this.FbRank = base.addP(new DsnDataP("", false)
			{
				name = "item_rank",
				text = "  ",
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w,
				size = 14f,
				html = true,
				text_auto_wrap = false
			}, false);
			base.Br();
			DsnDataHr dsnDataHr = new DsnDataHr
			{
				draw_width_rate = 1f,
				margin_t = 4f,
				margin_b = 5f,
				Col = C32.d2c(4283780170U)
			};
			base.addHr(dsnDataHr);
			this.FbDetail = base.addP(new DsnDataP("", false)
			{
				name = "gq_detail",
				text = "  ",
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w,
				sheight = designer.use_h - 28f,
				text_margin_y = 14f,
				text_margin_x = 20f,
				lineSpacing = 1.28f,
				size = 14f,
				html = true,
				text_auto_condense = true,
				text_auto_wrap = TX.isEnglishLang()
			}, false);
			base.Br();
			base.addP(new DsnDataP("", false)
			{
				text = TX.Get("Guild_reward", "") + ": ",
				TxCol = C32.d2c(4283780170U),
				sheight = 28f,
				text_margin_x = 20f,
				text_margin_y = 10f,
				size = 14f
			}, false);
			aBtnNel aBtnNel = this.addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "gq_reel",
				skin = "reelinfo",
				w = X.Mn(designer.use_w, 240f),
				h = 28f,
				unselectable = 2
			});
			this.FbMoney = base.addP(new DsnDataP("", false)
			{
				name = "gq_money",
				text = "  ",
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w - 15f,
				text_margin_y = 10f,
				text_margin_x = 2f,
				size = 14f,
				html = true,
				text_auto_condense = true
			}, false);
			this.SkReel = aBtnNel.get_Skin() as ButtonSkinNelReelInfo;
			this.SkReel.fix_text_size = 14f;
			base.endTab(true);
			base.addHr(new DsnDataHr
			{
				margin_t = 14f,
				margin_b = 14f,
				vertical = true,
				swidth = designer.get_sheight_px(),
				line_height = 1f,
				draw_width_rate = 0.96f,
				Col = C32.d2c(4283780170U)
			});
			float use_w = base.use_w;
			Designer designer2 = this.addTab("T_R", use_w, use_h, use_w, use_h, false);
			designer2.Smallest().init();
			this.FbDesc = base.addImg(new DsnDataImg
			{
				name = "_gq_desc",
				text = "  ",
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				TxCol = C32.d2c(4283780170U),
				swidth = designer2.use_w,
				sheight = designer2.use_h,
				text_margin_x = 18f,
				text_margin_y = 32f,
				size = 14f,
				html = true,
				text_auto_condense = true,
				text_auto_wrap = TX.isEnglishLang(),
				MI = MTR.MIiconL,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawBlankBox)
			});
			base.endTab(true);
		}

		public void changeState(UiGQCard.STATE _state, bool immediate = false)
		{
			this.state = _state;
			this.t_state = (float)(immediate ? (-1) : 0);
			this.FbDesc.redraw_flag = true;
			switch (this.state)
			{
			case UiGQCard.STATE.RECEIVED:
				if (!immediate)
				{
					SND.Ui.play("pencil_running", false);
					return;
				}
				break;
			case UiGQCard.STATE.SUCCESS:
				if (!immediate)
				{
					SND.Ui.play("guildq_success", false);
					if (this.Efp0 == null)
					{
						this.Efp0 = new EfParticleOnce(null, EFCON_TYPE.FIXED);
					}
					if (this.Efp1 == null)
					{
						this.Efp1 = new EfParticleOnce(null, EFCON_TYPE.FIXED);
					}
					this.Efp0.key = "ui_guildq_success_smoke";
					this.Efp1.key = "ui_guildq_success_unikira";
					return;
				}
				break;
			case UiGQCard.STATE.FAILED:
				if (!immediate)
				{
					SND.Ui.play("guildq_failed", false);
				}
				break;
			default:
				return;
			}
		}

		public void Set(GuildManager.GQEntry Gq, UiGQCard.STATE _state = UiGQCard.STATE.AUTO)
		{
			if (Gq == this.SelectedGQ)
			{
				return;
			}
			this.SelectedGQ = Gq;
			if (this.FbName == null)
			{
				this.createUi();
			}
			if (Gq == null)
			{
				this.FbName.text_content = "";
				this.FbRank.text_content = "";
				this.FbDetail.text_content = "";
				this.FbDesc.text_content = "";
				this.SkReel.getBtn().gameObject.SetActive(false);
				return;
			}
			this.ran = Gq.ran0;
			using (STB stb = TX.PopBld(null, 0))
			{
				NelItem.getGradeMeshTxTo(stb, Gq.grade, 4, 34);
				this.FbRank.Txt(stb);
				Gq.ItemEntry.FnGetName(stb.Clear(), Gq.ItemEntry, Gq.grade);
				this.FbName.Txt(stb);
				Gq.ItemEntry.FnGetDesc(stb.Clear(), Gq.ItemEntry, Gq.grade);
				this.FbDesc.Txt(stb);
				Gq.ItemEntry.FnGetDetail(stb.Clear(), Gq.ItemEntry, Gq.grade);
				this.FbDetail.Txt(stb);
				stb.Clear().AddTxA("Guild_detail_obtain_money", false).TxRpl(Gq.reward_money);
				this.FbMoney.Txt(stb);
			}
			if (Gq.RewardIR != null)
			{
				this.SkReel.getBtn().gameObject.SetActive(true);
				this.SkReel.initIR(Gq.RewardIR);
			}
			else if (Gq.reward_etype > ReelExecuter.ETYPE.ITEMKIND)
			{
				this.SkReel.getBtn().gameObject.SetActive(true);
				this.SkReel.initReel(Gq.reward_etype);
			}
			else
			{
				this.SkReel.getBtn().gameObject.SetActive(false);
			}
			if (_state == UiGQCard.STATE.AUTO)
			{
				_state = ((this.M2D.IMNG.getInventoryPrecious().getCount(Gq.ItemEntry, -1) > 0) ? UiGQCard.STATE.RECEIVED : UiGQCard.STATE.NONE);
			}
			this.changeState(_state, true);
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.t_state >= 0f)
			{
				this.t_state += fcnt;
			}
			if (this.t_fb >= 0f)
			{
				this.t_fb += fcnt;
				this.repositTip();
			}
			return true;
		}

		public bool fnDrawBlankBox(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			bool flag = false;
			if (this.FbDesc == null)
			{
				return false;
			}
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTR.MIiconL.getMtr(MTR.getShd("Hachan/ShaderGDTSTPencilDraw"), FI.stencil_ref), false);
				Md.chooseSubMesh(2, false, false);
				Md.setMaterial(MTRX.getMtr(FI.stencil_ref), false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			Md.base_x = (Md.base_y = 0f);
			Md.chooseSubMesh(0, false, false);
			float num = FI.get_swidth_px() * 0.5f - 92f;
			float num2 = -FI.get_sheight_px() * 0.5f + 55f;
			Md.Identity().TranslateP(num, num2, false);
			Md.Col = C32.MulA(4283780170U, alpha * 0.4f);
			Md.RotaPF(0f, 0f, 1f, 1f, 0f, MTRX.getPF("guildquest_blank"), false, false, false, uint.MaxValue, false, 0);
			Md.Rotate(0.05235988f, true);
			if (this.SelectedGQ == null)
			{
				return true;
			}
			if (this.state == UiGQCard.STATE.RECEIVED_TEMP || this.state == UiGQCard.STATE.RECEIVED || this.state == UiGQCard.STATE.SUCCESS || this.state == UiGQCard.STATE.FAILED)
			{
				bool flag2 = this.state == UiGQCard.STATE.RECEIVED_TEMP;
				flag = true;
				float num3 = 0f;
				float num4 = X.NI(1f, 1.15f, X.RAN(this.ran, 664)) * 0.6f;
				float num5 = X.NI(-10, 10, X.RAN(this.ran, 315)) - 40f;
				float num6 = X.NI(-20, 20, X.RAN(this.ran, 4331)) - 6f;
				float num7 = X.NI(-1, 1, X.RAN(this.ran, 2139)) * 0.08f * 3.1415927f;
				if (flag2)
				{
					num3 = ((this.t_state < 0f) ? 1f : X.ZLINE(this.t_state, 20f)) * (0.1f + 0.7f * X.Abs(X.COSI((float)IN.totalframe, 140f)));
					Md.Col = C32.MulA(4283780170U, alpha * num3 * 0.7f);
					flag = true;
				}
				else if (this.state == UiGQCard.STATE.RECEIVED)
				{
					num3 = ((this.t_state < 0f) ? 1f : X.ZLINE(this.t_state, 40f));
					Md.chooseSubMesh(1, false, false);
					Md.Col = C32.MulA(4283780170U, alpha * X.ZLINE(num3, 0.2f));
					Md.allocUv23(4, false);
					flag = this.t_state >= 0f && this.t_state <= 40f;
				}
				else
				{
					Md.Col = C32.MulA(4283780170U, alpha);
				}
				Md.RotaPF(num5, num6, num4, num4, num7, MTRX.getPF("guildquest_sign"), false, false, false, uint.MaxValue, false, 0);
				if (this.state == UiGQCard.STATE.RECEIVED)
				{
					Md.Uv2(0f, 1f, false).Uv2(0f, 0f, false).Uv2(1f, 0f, false)
						.Uv2(1f, 1f, false);
					Md.Uv3(num3, 0f, false).allocUv3(0, true);
				}
				Md.chooseSubMesh(0, false, false);
				if (this.state == UiGQCard.STATE.FAILED || this.state == UiGQCard.STATE.SUCCESS)
				{
					num += X.NI(-10, 10, X.RAN(this.ran, 315)) + 40f;
					num2 += X.NI(-11, 11, X.RAN(this.ran, 4331)) + 6f;
					num4 = X.NI(1f, 1.15f, X.RAN(this.ran, 784)) * 0.36f;
					num7 = X.NI(-1, 1, X.RAN(this.ran, 9245)) * 0.13f * 3.1415927f;
					if (this.state == UiGQCard.STATE.SUCCESS)
					{
						if (this.t_state >= 0f)
						{
							num4 *= 1f + 0.04f * (X.ZSIN(this.t_state, 14f) * (1f - X.ZCOS(this.t_state - 18f, 30f)));
						}
						Md.Identity();
						Md.base_px_x = num;
						Md.base_px_y = num2;
						Md.Col = C32.MulA(4281449800U, alpha);
						Md.RotaPF(0f, 0f, num4, num4, num7, MTRX.getPF("guildquest_stamp"), false, false, false, uint.MaxValue, false, 0);
						if (this.t_state >= 0f)
						{
							this.Efp0.drawTo(Md, num, num2, 60f, 50, this.t_state, 0f);
							Md.chooseSubMesh(2, false, false);
							this.Efp1.drawTo(Md, num, num2, 60f, 50, this.t_state - 15f, 0f);
						}
					}
					if (this.state == UiGQCard.STATE.FAILED)
					{
						if (this.t_state >= 30f && this.t_state < 200f)
						{
							this.t_state = 200f;
							SND.Ui.play("guildq_failed", false);
						}
						PxlImage img = MTRX.getPF("guildquest_stroke").getLayer(0).Img;
						Rect rectIUv = img.RectIUv;
						Md.Col = C32.MulA(4290781724U, alpha);
						Md.initForImg(img, 0);
						float num8 = X.NI(90, 102, X.RAN(this.ran, 1141));
						for (int i = 0; i < 2; i++)
						{
							num3 = X.ZSIN2(this.t_state - (float)(200 * i), 24f);
							if (num3 <= 0f)
							{
								break;
							}
							float num9 = num7 - 0.7853982f - 1.5707964f * (float)i;
							Md.Identity().Rotate(num9, false).TranslateP(num, num2, false);
							Md.RectBL(-num8 * 0.5f, -4f, num8 * num3, 8f, false);
							Md.InputImageUv(0f, 0f, num3, 1f);
						}
					}
				}
			}
			update_meshdrawer = true;
			return !flag;
		}

		public override Designer activate()
		{
			base.activate();
			this.t_fb = -1f;
			if (this.AFbChip != null)
			{
				for (int i = this.AFbChip.Count - 1; i >= 0; i--)
				{
					this.AFbChip[i].gameObject.SetActive(false);
				}
			}
			if (this.SkReel != null)
			{
				this.SkReel.getBtn().hide();
			}
			return this;
		}

		public override Designer deactivate()
		{
			base.deactivate();
			return this;
		}

		public void setRenzokuTip(int money)
		{
			if (this.AFbChip == null)
			{
				this.AFbChip = new List<FillBlock>(1);
			}
			int count = this.AFbChip.Count;
			FillBlock fillBlock = null;
			for (int i = 0; i < count; i++)
			{
				FillBlock fillBlock2 = this.AFbChip[i];
				if (!fillBlock2.gameObject.activeSelf)
				{
					fillBlock = fillBlock2;
					fillBlock2.gameObject.SetActive(true);
					break;
				}
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Guild_reward_multi_quest", false).Add(" ");
				CoinStorage.getIconHtml(stb, CoinStorage.CTYPE.GOLD, 30);
				stb.Add("+" + money.ToString());
				if (fillBlock == null)
				{
					fillBlock = IN.CreateGobGUI(base.gameObject, "-Tip" + count.ToString()).AddComponent<FillBlock>();
					this.AFbChip.Add(fillBlock);
					fillBlock.radius = 100f;
					fillBlock.TxCol = MTRX.ColWhite;
					fillBlock.Col = C32.MulA(4278190080U, 0.7f);
					fillBlock.margin_x = 22f;
					fillBlock.margin_y = 10f;
					fillBlock.size = 16f;
					fillBlock.StartFb(null, stb, true);
				}
				else
				{
					fillBlock.Txt(stb);
				}
				fillBlock.alpha = 0f;
			}
			if (this.t_fb < 0f)
			{
				this.t_fb = 0f;
			}
			this.repositTip();
		}

		public void repositTip()
		{
			if (this.t_fb < 0f || this.AFbChip == null)
			{
				return;
			}
			int count = this.AFbChip.Count;
			Vector3 position = this.FbMoney.transform.position;
			for (int i = 0; i < count; i++)
			{
				FillBlock fillBlock = this.AFbChip[i];
				float num = this.t_fb - (float)(i * 12);
				if (num <= 0f)
				{
					fillBlock.setAlpha(0f);
					return;
				}
				float num2 = X.ZSIN(num, 20f);
				if (fillBlock.alpha == 0f)
				{
					SND.Ui.play("guild_tip_added", false);
				}
				fillBlock.setAlpha(num2);
				if (num < 40f)
				{
					Vector3 vector = position;
					vector.x -= 0.21875f;
					vector.y += ((float)(i * 20) + (20f + 12f * num2)) * 0.015625f;
					fillBlock.transform.position = vector;
				}
				fillBlock.text_alpha_multiple = 0.5f + 0.5f * X.Abs(X.COSI((float)(IN.totalframe + i * 13), 50f));
			}
		}

		public NelM2DBase M2D;

		private FillBlock FbName;

		private FillBlock FbRank;

		private FillBlock FbDetail;

		private FillBlock FbMoney;

		private FillImageBlock FbDesc;

		private List<FillBlock> AFbChip;

		private uint ran;

		private float t_state = -1f;

		private float t_fb = -1f;

		private ButtonSkinNelReelInfo SkReel;

		private EfParticleOnce Efp0;

		private EfParticleOnce Efp1;

		public enum STATE
		{
			AUTO,
			NONE,
			RECEIVED_TEMP,
			RECEIVED,
			SUCCESS,
			FAILED
		}
	}
}
