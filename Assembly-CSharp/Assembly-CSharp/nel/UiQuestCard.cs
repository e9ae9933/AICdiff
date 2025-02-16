using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UiQuestCard : Designer
	{
		public UiQuestCard initQ(int stencil_ref, QuestTracker.QuestProgress _Prog, FnBtnBindings fnClickBtn)
		{
			if (this.FD_fnDrawP == null)
			{
				this.FD_fnDrawP = new FillImageBlock.FnDrawInFIB(this.fnDrawP);
				this.FD_fnBtnHover = new FnBtnBindings(this.fnBtnHover);
				this.FD_fnDrawPCollectTarget = new FillImageBlock.FnDrawInFIB(this.fnDrawPCollectTarget);
				this.FD_fnDrawTreasure = new FillImageBlock.FnDrawInFIB(this.fnDrawTreasure);
			}
			this.Prog = _Prog;
			this.treasure_image_load_rest = 0;
			base.Small();
			this.radius = (float)(this.small_mode ? 18 : 34);
			base.bgcol = C32.d2c(this.Prog.aborted ? 4289043355U : 4291806147U);
			this.use_button_connection = false;
			this.selectable_loop = 1;
			float num = 1f;
			this.RevealTarget = null;
			this.Clear();
			if (!this.small_mode && this.TxClient == null)
			{
				this.TxClient = IN.CreateGob(base.gameObject, "-Client").AddComponent<TextRenderer>();
				this.TxClient.Size(14f).Col(4283780170U).LetterSpacing(0.95f);
				this.TxClient.html_mode = true;
				this.TxClient.alignx = ALIGN.RIGHT;
			}
			string text;
			uint num2;
			this.Prog.Q.getClientData(out text, out this.PFClient, out num2);
			float num3 = 25f;
			if (!this.small_mode)
			{
				this.TxClient.gameObject.SetActive(true);
				this.TxClient.BorderCol(C32.MulA(num2, 0.7f));
				this.TxClient.text_content = text;
				this.repositTxClient();
				this.TxClient.stencil_ref = stencil_ref;
				num3 = this.TxClient.get_swidth_px() + 30f;
			}
			else if (this.TxClient != null)
			{
				this.TxClient.gameObject.SetActive(false);
				this.TxClient.stencil_ref = stencil_ref;
			}
			this.no_write_mask = true;
			this.stencil_ref = stencil_ref;
			base.alignx = ALIGN.LEFT;
			if (!this.small_mode)
			{
				this.margin_in_lr = 18f;
				this.margin_in_tb = 18f;
				base.item_margin_x_px = 12f;
				base.item_margin_y_px = 6f;
			}
			else
			{
				num = 0.8f;
				this.margin_in_lr = 10f;
				this.margin_in_tb = 10f;
				base.item_margin_x_px = 6f;
				base.item_margin_y_px = 3f;
			}
			this.init();
			if (base.Area == null)
			{
				base.makeAreaButton("transparent");
				base.Area.addHoverFn(this.FD_fnBtnHover);
			}
			base.Area.clearNavi(15U, false);
			int num4 = X.Mn(this.Prog.phase, this.Prog.Q.end_phase - 1);
			Color32 color = C32.MulA(4283780170U, (this.Prog.finished || this.Prog.aborted) ? 0.66f : 1f);
			DsnDataButton dsnDataButton = null;
			FnBtnBindings fnBtnBindings = null;
			int i = 0;
			while (i <= num4)
			{
				if (this.Prog.Q.cascade_show_phase(i) && num4 < this.Prog.Q.end_phase - 1)
				{
					num4++;
				}
				bool flag = i < this.Prog.phase;
				Color32 color2;
				if (flag)
				{
					color2 = C32.d2c(4288252036U);
				}
				else
				{
					color2 = C32.d2c(4283780170U);
				}
				bool flag2 = true;
				bool flag3;
				NelItemEntry[] collectTarget = this.Prog.Q.getCollectTarget(i, out flag3);
				QuestTracker.SummonerEntry[] summonerEntryTarget = this.Prog.Q.getSummonerEntryTarget(i);
				EvImg evImg = ((!this.small_mode || !flag) ? this.Prog.Q.getTreasureImg(i) : null);
				if (i < num4 && this.Prog.Q.getDescription(i + 1) == this.Prog.Q.getDescription(i))
				{
					flag2 = false;
				}
				if (i < num4 && (this.Prog.Q.auto_hide_bits & (1U << i)) != 0U)
				{
					for (int j = i + 1; j <= num4; j++)
					{
						if (TX.valid(this.Prog.Q.getDescription(i + 1)))
						{
							flag2 = false;
						}
					}
				}
				if (flag2)
				{
					STB stb = TX.PopBld(null, 0);
					stb += (flag ? "<img mesh=\"mini_checkbox_checked\" color=\"0x" : "<img mesh=\"mini_checkbox\" color=\"0x");
					stb += color;
					stb.Add("\"/> ", this.Prog.Q.getDescription(i));
					this.RevealTarget = base.addImg(new DsnDataImg
					{
						name = i.ToString(),
						Stb = stb,
						html = true,
						TxCol = color2,
						alignx = ALIGN.LEFT,
						FnDrawInFIB = (flag ? this.FD_fnDrawP : null),
						swidth = base.use_w - num3,
						size = num * 16f
					});
					num3 = 0f;
					TX.ReleaseBld(stb);
					base.Br();
				}
				if (evImg != null)
				{
					if (num3 > 0f)
					{
						base.addHr(new DsnDataHr().H(20f));
						num3 = 0f;
					}
					base.XSh(36f);
					EV.getLoaderForPxl(evImg.PF.pChar).preparePxlChar(true, true);
					this.treasure_image_load_rest++;
					LoadTicketManager.AddTicket("UiQuestCard", "loadTreasure", delegate(LoadTicketManager.LoadTicket Tk)
					{
						UiQuestCard uiQuestCard = Tk.Target as UiQuestCard;
						UiQuestCard uiQuestCard2 = uiQuestCard;
						int num11 = uiQuestCard2.treasure_image_load_rest - 1;
						uiQuestCard2.treasure_image_load_rest = num11;
						if (num11 == 0)
						{
							uiQuestCard.redrawAllFillImageBlock();
						}
						return false;
					}, this, 200);
					float num5 = ((i < this.Prog.phase) ? 0.3f : 1f);
					float num6 = (float)evImg.PF.pSq.width * num5;
					float num7 = (float)evImg.PF.pSq.height * num5;
					if (num6 > base.use_w - 10f)
					{
						num5 *= (base.use_w - 10f) / num6;
						num6 = (float)((int)(num6 * num5 + 10f));
						num7 = (float)((int)(num6 * num5 + 10f));
					}
					this.RevealTarget = base.addImg(new DsnDataImg
					{
						name = "treasure_" + i.ToString(),
						html = true,
						FnDrawInFIB = this.FD_fnDrawTreasure,
						swidth = num6,
						sheight = num7
					});
					base.Br();
					num3 = 0f;
				}
				if (collectTarget != null)
				{
					ushort[] collectedCount = this.Prog.getCollectedCount(i);
					int num8 = collectTarget.Length;
					using (STB stb2 = TX.PopBld(null, 0))
					{
						for (int k = 0; k < num8; k++)
						{
							NelItemEntry nelItemEntry = collectTarget[k];
							int num9 = (int)((collectedCount != null && X.BTW(0f, (float)k, (float)collectedCount.Length)) ? collectedCount[k] : 0);
							bool flag4 = num9 >= nelItemEntry.count;
							string text2 = num9.ToString() + " / " + nelItemEntry.count.ToString();
							nelItemEntry.getLocalizedName(stb2.Clear(), 0, 2, false);
							this.createCollectTargetRow(i, k, flag4, color2, num3, num, stb2, text2);
						}
						goto IL_07C1;
					}
					goto IL_0748;
				}
				goto IL_0748;
				IL_07C1:
				i++;
				continue;
				IL_0748:
				if (summonerEntryTarget != null)
				{
					int num10 = summonerEntryTarget.Length;
					using (STB stb3 = TX.PopBld(null, 0))
					{
						for (int l = 0; l < num10; l++)
						{
							QuestTracker.SummonerEntry summonerEntry = summonerEntryTarget[l];
							bool flag5 = this.Prog.collectionFinished(i);
							stb3.SetTxA("Summoner_" + summonerEntry.summoner_key, false);
							this.createCollectTargetRow(i, l, flag5, color2, num3, num, stb3, null);
						}
					}
					goto IL_07C1;
				}
				goto IL_07C1;
			}
			if (!this.Prog.finished)
			{
				base.alignx = ALIGN.RIGHT;
				bool flag6 = X.ENG_MODE && this.small_mode;
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					if (!this.small_mode)
					{
						blist.Add("quest_tracking");
					}
					object obj;
					if (this.QM.M2D.IMNG.has_recipe_collection && this.Prog.Q.getFieldGuideTarget(num4, out obj))
					{
						blist.Add("&&KD_go_to_def_in_catalog");
					}
					if (this.Prog.CurrentDepert.isActiveMap())
					{
						blist.Add("reveal_map");
					}
					if (this.FD_QuestBtnDefine != null)
					{
						this.FD_QuestBtnDefine(this, blist);
					}
					int count = blist.Count;
					aBtn aBtn = null;
					aBtn aBtn2 = null;
					for (int m = 0; m < count; m++)
					{
						string text3 = blist[m];
						if (dsnDataButton == null)
						{
							dsnDataButton = new DsnDataButton();
						}
						dsnDataButton.def = false;
						dsnDataButton.title = text3;
						dsnDataButton.skin = "row_center";
						dsnDataButton.click_snd = null;
						dsnDataButton.skin_title = null;
						dsnDataButton.w = 180f * (X.ENG_MODE ? 1.6f : 1f);
						dsnDataButton.h = (float)(this.small_mode ? 22 : 30);
						dsnDataButton.fnClick = fnClickBtn;
						dsnDataButton.fnHover = this.FD_fnBtnHover;
						FnBtnBindings fnBtnBindings2 = null;
						if (text3 != null)
						{
							if (!(text3 == "reveal_map"))
							{
								if (text3 == "quest_tracking")
								{
									dsnDataButton.skin_title = "<img mesh=\"quest_tracking\" width=\"22\" />";
									dsnDataButton.skin = "row";
									dsnDataButton.w = (float)(this.small_mode ? 30 : 45);
									dsnDataButton.click_snd = "";
									if (fnBtnBindings == null)
									{
										fnBtnBindings = new FnBtnBindings(this.fnClickTrackingStart);
									}
									fnBtnBindings2 = fnBtnBindings;
									dsnDataButton.def = this.Prog.tracking;
								}
							}
							else
							{
								dsnDataButton.skin_title = "&&button_quest_reveal_on_map";
								dsnDataButton.skin = "row_quest_pin";
							}
						}
						dsnDataButton.w = X.Mn(this.w - this.margin_in_lr * 2f, dsnDataButton.w);
						UiQuestCard.aBtnNelQuestCard aBtnNelQuestCard = this.addButtonT<UiQuestCard.aBtnNelQuestCard>(dsnDataButton);
						aBtnNelQuestCard.DsCard = this;
						ButtonSkinNelQuestPin buttonSkinNelQuestPin = aBtnNelQuestCard.get_Skin() as ButtonSkinNelQuestPin;
						if (buttonSkinNelQuestPin != null)
						{
							buttonSkinNelQuestPin.Q = this.Prog.Q;
						}
						if (fnBtnBindings2 != null)
						{
							aBtnNelQuestCard.addClickFn(fnBtnBindings2);
						}
						if (aBtn2 != null)
						{
							if (flag6)
							{
								aBtn2.setNaviB(aBtnNelQuestCard, true, false);
							}
							else
							{
								aBtn2.setNaviR(aBtnNelQuestCard, true, false);
							}
						}
						if (flag6)
						{
							base.Br();
						}
						if (aBtn == null)
						{
							aBtn = aBtnNelQuestCard;
						}
						aBtn2 = aBtnNelQuestCard;
					}
					if (aBtn != null)
					{
						if (flag6)
						{
							aBtn.setNaviT(aBtn2, true, false);
						}
						else
						{
							aBtn.setNaviL(aBtn2, true, false);
						}
					}
				}
			}
			return this;
		}

		private void redrawAllFillImageBlock()
		{
			foreach (KeyValuePair<IDesignerBlock, DesignerRowMem.DsnMem> keyValuePair in this.OBlockMem)
			{
				if (keyValuePair.Value.Blk is FillImageBlock)
				{
					(keyValuePair.Value.Blk as FillImageBlock).redraw_flag = true;
				}
			}
		}

		private void createCollectTargetRow(int phase, int itmi, bool already_collected, Color32 __TxCol, float clip_w, float fontscl, STB StbMain, string countstr)
		{
			if (already_collected)
			{
				__TxCol = C32.d2c(4288252036U);
			}
			base.XSh((float)(this.small_mode ? 30 : 68)).addImg(new DsnDataImg
			{
				name = "collect_target_" + phase.ToString() + "_" + itmi.ToString(),
				Stb = StbMain,
				html = true,
				alignx = ALIGN.LEFT,
				sheight = (float)(this.small_mode ? 18 : 27),
				TxCol = __TxCol,
				FnDrawInFIB = this.FD_fnDrawPCollectTarget,
				swidth = base.use_w - (float)(TX.valid(countstr) ? 90 : 14) - clip_w,
				text_auto_condense = true,
				size = fontscl * 14f
			});
			clip_w = 0f;
			if (TX.valid(countstr))
			{
				base.addP(new DsnDataP("", false)
				{
					text = countstr,
					TxCol = __TxCol,
					swidth = base.use_w - 8f,
					sheight = (float)(this.small_mode ? 18 : 27),
					size = fontscl * 14f,
					text_auto_condense = true
				}, false);
			}
			base.Br();
		}

		public void repositTxClient()
		{
			if (this.TxClient != null)
			{
				IN.PosP(this.TxClient.transform, this.w * 0.5f - 25f, this.h * 0.5f - 16f, -0.05f);
				base.need_redraw_background = true;
			}
		}

		public override Designer WH(float _wpx = 0f, float _hpx = 0f)
		{
			base.WH(_wpx, _hpx);
			this.repositTxClient();
			return this;
		}

		public void relinkNavi(BList<aBtn> APre, bool navi_clearing = true)
		{
			using (BList<aBtn> blist = ListBuffer<aBtn>.Pop(0))
			{
				using (BList<aBtn> blist2 = ListBuffer<aBtn>.Pop(0))
				{
					if (this.BCon.Length <= 1)
					{
						base.Area.gameObject.SetActive(true);
						base.Area.bind();
						base.Area.setNaviL(base.Area, true, true).setNaviR(base.Area, true, true);
						blist.Add(base.Area);
						blist2.Add(base.Area);
					}
					else
					{
						base.Area.hide();
						base.Area.clearNavi(10U, false);
						base.Area.gameObject.SetActive(false);
						if (this.small_mode && X.ENG_MODE)
						{
							blist.Add(this.BCon.Get(this.BCon.Length - 1));
							blist2.Add(this.BCon.Get(1));
						}
						else
						{
							int length = this.BCon.Length;
							for (int i = 1; i < length; i++)
							{
								aBtn aBtn = this.BCon.Get(i);
								blist.Add(aBtn);
								blist2.Add(aBtn);
							}
						}
					}
					if (navi_clearing)
					{
						for (int j = blist2.Count - 1; j >= 0; j--)
						{
							blist2[j].clearNavi(2U, false);
						}
					}
					if (APre.Count > 0)
					{
						if (navi_clearing)
						{
							for (int k = APre.Count - 1; k >= 0; k--)
							{
								APre[k].clearNavi(8U, false);
							}
						}
						int num = APre.Count - 1;
						int num2 = blist2.Count - 1;
						if (num > num2)
						{
							for (int l = 0; l <= num; l++)
							{
								APre[l].setNaviB(blist2[X.IntR((float)l / (float)num * (float)num2)], true, false);
							}
						}
						else
						{
							for (int m = 0; m <= num2; m++)
							{
								blist2[m].setNaviT(APre[(num == num2) ? m : X.IntR((float)m / (float)num2 * (float)num)], true, false);
							}
						}
					}
				}
				APre.Clear();
				APre.AddRange(blist);
			}
		}

		public void clearNaviTB()
		{
			this.BCon.clearNaviAll(10U, false);
		}

		public static void relinkNaviAll(Designer Ds, List<aBtn> AUpper = null)
		{
			using (BList<DesignerRowMem.DsnMem> blist = ListBuffer<DesignerRowMem.DsnMem>.Pop(0))
			{
				Ds.getRowManager().copyMems(blist, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is UiQuestCard, null);
				using (BList<aBtn> blist2 = ListBuffer<aBtn>.Pop(0))
				{
					int count = blist.Count;
					if (AUpper != null)
					{
						blist2.AddRange(AUpper);
					}
					for (int i = 0; i < count; i++)
					{
						(blist[i].Blk as UiQuestCard).relinkNavi(blist2, true);
					}
					if (AUpper != null && AUpper.Count > 0 && count > 0 && blist2.Count > 0)
					{
						int num = blist2.Count - 1;
						int num2 = AUpper.Count - 1;
						if (num > num2)
						{
							for (int j = 0; j <= num; j++)
							{
								blist2[j].setNaviB(AUpper[X.IntR((float)j / (float)num * (float)num2)], true, false);
							}
						}
						else
						{
							for (int k = 0; k <= num2; k++)
							{
								AUpper[k].setNaviT(blist2[(num == num2) ? k : X.IntR((float)k / (float)num2 * (float)num)], true, false);
							}
						}
					}
					else if (count >= 1)
					{
						(blist[0].Blk as UiQuestCard).relinkNavi(blist2, true);
					}
				}
			}
		}

		private bool fnBtnHover(aBtn B)
		{
			if (this.QM != null)
			{
				this.QM.UiDefaultFocusChange(this.Prog);
			}
			if (this.fnQBtnTouched != null)
			{
				this.fnQBtnTouched(B, this);
			}
			if (this.BelongDesigner != null && this.BelongDesigner.use_scroll)
			{
				ScrollBox scrollBox = this.BelongDesigner.getScrollBox();
				if (scrollBox.get_sheight_px() < base.Area.get_sheight_px() + 8f && this.RevealTarget != null)
				{
					scrollBox.reveal(this.RevealTarget, true, REVEALTYPE.IF_HIDING);
				}
				else
				{
					scrollBox.reveal(base.Area, true, REVEALTYPE.IF_HIDING);
				}
			}
			return this;
		}

		private bool fnClickTrackingStart(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			if (B.isChecked())
			{
				this.QM.setFocusedQuest(this.Prog);
				SND.Ui.play("quest_tracking", false);
			}
			else
			{
				this.Prog.tracking = false;
				SND.Ui.play("talk_progress", false);
			}
			this.QM.need_fine_head_quest = true;
			base.need_redraw_background = true;
			return true;
		}

		private bool fnDrawP(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (FI.get_text_swidth_px(false) < 4f)
			{
				return false;
			}
			Md.Col = C32.MulA(4288252036U, alpha);
			update_meshdrawer = true;
			float num = (FI.get_swidth_px() - 18f) * 0.5f;
			float num2 = (FI.get_sheight_px() - 8f) * 0.5f;
			float num3 = FI.get_text_swidth_px(false) + 4f;
			Md.Line(-num, num2, -num + num3, -num2, 2f, false, 0f, 0f);
			return true;
		}

		private bool fnDrawTreasure(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			string name = base.getName(FI);
			if (TX.noe(name) || !REG.match(name, UiQuestCard.RegTreasureFibName))
			{
				return false;
			}
			int num = X.NmI(REG.R1, 0, false, false);
			EvImg treasureImg = this.Prog.Q.getTreasureImg(num);
			if (treasureImg == null)
			{
				return false;
			}
			EV.getLoaderForPxl(treasureImg.PF.pChar);
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(0, false, false);
				Md.chooseSubMesh(1, false, false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			Md.chooseSubMesh(0, false, true);
			float num2 = FI.get_swidth_px();
			float num3 = FI.get_sheight_px();
			Md.Col = Md.ColGrd.Black().mulA(alpha * 0.3f).C;
			Md.Box(0f, 0f, num2 + 2f, num3 + 2f, 2f, false);
			Md.Col = Md.ColGrd.White().mulA(alpha * 0.75f).C;
			Md.Box(0f, 0f, num2, num3, 5f, false);
			Md.Col = Md.ColGrd.Set(4284703587U).mulA(alpha).C;
			num2 -= 10f;
			num3 -= 10f;
			Md.Box(0f, 0f, num2, num3, 10f, false);
			Md.chooseSubMesh(1, false, true);
			if (FI.text_alpha > 0f && this.treasure_image_load_rest == 0)
			{
				Md.Col = Md.ColGrd.White().mulA(alpha).C;
				Material mtr = MTRX.getMI(treasureImg.PF.pChar, false).getMtr(FI.stencil_ref);
				if (mtr != Md.getMaterial())
				{
					Md.setMaterial(mtr, false);
				}
				float width = treasureImg.PF.width;
				float num4 = 1f;
				if (width > num2)
				{
					num4 *= num2 / width;
				}
				Md.RotaPF(0f, 0f, num4, num4, 0f, treasureImg.PF, false, false, false, uint.MaxValue, false, 0);
			}
			Md.chooseSubMesh(0, false, false);
			if (this.Prog.phase > num)
			{
				Md.Col = Md.ColGrd.Set(4283780170U).mulA(alpha).C;
				Md.CheckMark(num2 * 0.5f + 26f, 0f, 24f, 4f, false);
			}
			update_meshdrawer = true;
			return FI.text_alpha == 1f;
		}

		private bool fnDrawPCollectTarget(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			string name = base.getName(FI);
			if (name == null)
			{
				return false;
			}
			if (REG.match(name, UiQuestCard.RegCollectFibName))
			{
				int num = X.NmI(REG.R1, 0, false, false);
				int num2 = X.NmI(REG.R2, 0, false, false);
				bool flag;
				NelItemEntry[] collectTarget = this.Prog.Q.getCollectTarget(num, out flag);
				if (collectTarget != null)
				{
					NelItemEntry nelItemEntry = collectTarget[num2];
					float num3 = -16f - FI.get_swidth_px() * 0.5f;
					if (!this.small_mode)
					{
						if (!Md.hasMultipleTriangle())
						{
							Md.chooseSubMesh(1, false, false);
							Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, FI.stencil_ref), false);
							Md.connectRendererToTriMulti(FI.getMeshRenderer());
						}
						nelItemEntry.Data.drawIconTo(Md, null, 0, 1, num3, 0f, 1f, alpha, null);
						num3 -= 30f;
					}
					ushort[] collectedCount = this.Prog.getCollectedCount(num);
					bool flag2 = (int)((collectedCount != null && X.BTW(0f, (float)num2, (float)collectedCount.Length)) ? collectedCount[num2] : 0) >= nelItemEntry.count;
					if (Md.hasMultipleTriangle())
					{
						Md.chooseSubMesh(0, false, false);
					}
					if (flag2)
					{
						Md.Col = C32.MulA(this.Prog.finished ? 4288252036U : 4283780170U, alpha);
						Md.CheckMark(num3, 0f, (float)(this.small_mode ? 13 : 18), 3f, false);
					}
					update_meshdrawer = true;
				}
				else
				{
					QuestTracker.SummonerEntry[] summonerEntryTarget = this.Prog.Q.getSummonerEntryTarget(num);
					if (summonerEntryTarget != null)
					{
						Md.Col = C32.MulA(this.Prog.finished ? 4288252036U : 4283780170U, alpha);
						float num4 = -16f - FI.get_swidth_px() * 0.5f + (float)(this.small_mode ? 14 : 0);
						float num5 = (float)(this.small_mode ? 12 : 20);
						Md.Box(num4, 0f, num5, num5, 2f, false);
						if (this.Prog.summonerAlreadyDefeated(num, summonerEntryTarget[num2].summoner_key))
						{
							Md.CheckMark(num4, 0f, num5 - 2f, 3f, false);
						}
					}
				}
			}
			return true;
		}

		public bool Is(QuestTracker.Quest Q)
		{
			return this.Prog != null && this.Prog.Q == Q;
		}

		public QuestTracker.Quest getQuest()
		{
			if (this.Prog == null)
			{
				return null;
			}
			return this.Prog.Q;
		}

		public QuestTracker.QuestDeperture getCurrentDepert()
		{
			return this.Prog.CurrentDepert;
		}

		public bool hasButton(aBtn B)
		{
			return B == base.Area || this.BCon.getIndex(B) >= 0;
		}

		public aBtn SelectFirstButton(string b_key = null)
		{
			aBtn aBtn;
			if (b_key != null)
			{
				if (b_key == "designer_area" && this.BCon.Length <= 1)
				{
					base.Area.Select(true);
					return base.Area;
				}
				aBtn = this.BCon.Get(b_key);
				if (aBtn != null)
				{
					aBtn.Select(true);
					return aBtn;
				}
			}
			if (this.BCon.Length <= 1)
			{
				base.Area.Select(true);
				return base.Area;
			}
			aBtn = this.BCon.Get(1);
			aBtn.Select(true);
			return aBtn;
		}

		protected override void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			if (this.MdKadomaru == null)
			{
				return;
			}
			if (this.Prog != null)
			{
				if (this.PFClient != null && !this.MdKadomaru.hasMultipleTriangle())
				{
					this.MdKadomaru.chooseSubMesh(1, false, false);
					this.MdKadomaru.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, this.stencil_ref), false);
					this.MdKadomaru.connectRendererToTriMulti(this.MrdKadomaru);
				}
				if (this.PFClient != null)
				{
					this.MdKadomaru.chooseSubMesh(0, false, true);
				}
				base.kadomaruRedraw(_t, false);
				if (this.Prog.aborted)
				{
					this.MdKadomaru.Col = C32.MulA(4284570202U, this.alpha_);
					float num = this.w * 0.5f - 20f;
					float num2 = this.h * 0.5f - 15f;
					this.MdKadomaru.Line(num, num2, -num, -num2, 8f, false, 0f, 0f);
					this.MdKadomaru.Line(num, -num2, -num, num2, 8f, false, 0f, 0f);
				}
				this.MdKadomaru.Col = C32.MulA(this.Prog.new_icon ? 4294926244U : 4286477940U, this.alpha_);
				float num3 = -this.w * 0.5f + (float)(this.small_mode ? 8 : 12);
				float num4 = 0f;
				this.MdKadomaru.Poly(num3, num4, (float)(this.small_mode ? 3 : 5), 0f, 15, 0f, false, 0f, 0f);
				if (this.Prog.tracking)
				{
					base.need_redraw_background = true;
					this.MdKadomaru.Col = C32.MulA(this.Prog.Q.PinColor, this.alpha_);
					NEL.drawQuestTrackingCircle(this.MdKadomaru, num3, num4, 0.75f);
				}
				if (this.PFClient != null)
				{
					this.MdKadomaru.chooseSubMesh(1, false, true);
					this.MdKadomaru.Col = C32.MulA(MTRX.ColWhite, this.alpha_);
					if (this.small_mode)
					{
						this.MdKadomaru.RotaPF(this.w * 0.5f - 18f, this.h * 0.5f - 18f, 1f, 1f, 0f, this.PFClient, false, false, false, uint.MaxValue, false, 0);
					}
					else if (this.TxClient != null)
					{
						this.MdKadomaru.RotaPF(this.w * 0.5f - 40f - this.TxClient.get_swidth_px(), this.h * 0.5f - 20f, 1f, 1f, 0f, this.PFClient, false, false, false, uint.MaxValue, false, 0);
					}
					this.MdKadomaru.chooseSubMesh(0, false, false);
				}
			}
			if (update_mesh)
			{
				this.MdKadomaru.updateForMeshRenderer(false);
			}
		}

		public static UiQuestCard getTabForButton(Designer Ds, aBtn B)
		{
			using (BList<DesignerRowMem.DsnMem> blist = ListBuffer<DesignerRowMem.DsnMem>.Pop(0))
			{
				Ds.getRowManager().copyMems(blist, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is UiQuestCard, null);
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					UiQuestCard uiQuestCard = blist[i].Blk as UiQuestCard;
					if (!(uiQuestCard == null) && uiQuestCard.hasButton(B))
					{
						return uiQuestCard;
					}
				}
			}
			return null;
		}

		public static UiQuestCard getTabForQuest(Designer Ds, QuestTracker.Quest Q)
		{
			using (BList<DesignerRowMem.DsnMem> blist = ListBuffer<DesignerRowMem.DsnMem>.Pop(0))
			{
				Ds.getRowManager().copyMems(blist, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is UiQuestCard, null);
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					UiQuestCard uiQuestCard = blist[i].Blk as UiQuestCard;
					if (!(uiQuestCard == null) && uiQuestCard.Is(Q))
					{
						return uiQuestCard;
					}
				}
			}
			return null;
		}

		public QuestTracker QM;

		public QuestTracker.QuestProgress Prog;

		public UiQuestCard.FnQuestBtnTouched fnQBtnTouched;

		private FillImageBlock.FnDrawInFIB FD_fnDrawP;

		private FillImageBlock.FnDrawInFIB FD_fnDrawTreasure;

		private FillImageBlock.FnDrawInFIB FD_fnDrawPCollectTarget;

		private FnBtnBindings FD_fnBtnHover;

		public UiQuestCard.FnQuestBtnDefine FD_QuestBtnDefine;

		public bool small_mode;

		public Designer BelongDesigner;

		private const float img_margin = 10f;

		public IDesignerBlock RevealTarget;

		private static readonly Regex RegCollectFibName = new Regex("^collect_target_(\\d+)_(\\d+)");

		private static readonly Regex RegTreasureFibName = new Regex("^treasure_(\\d+)");

		public TextRenderer TxClient;

		private PxlFrame PFClient;

		private int treasure_image_load_rest;

		public class aBtnNelQuestCard : aBtnNel
		{
			public UiQuestCard DsCard;
		}

		public delegate void FnQuestBtnDefine(UiQuestCard Tab, List<string> Acmd_list);

		public delegate void FnQuestBtnTouched(aBtn B, UiQuestCard Tabx);
	}
}
