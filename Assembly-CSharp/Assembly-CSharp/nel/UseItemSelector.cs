using System;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UseItemSelector : ActiveSelector
	{
		public UseItemSelector(NelItemManager _IMNG)
			: base("USEL", "tool_gradation")
		{
			this.ACell = new UseItemSelector.ItCell[8];
			for (int i = 0; i < 8; i++)
			{
				this.ACell[i] = new UseItemSelector.ItCell();
			}
			this.IMNG = _IMNG;
			this.CenterPos = new Vector3(0f, 0.78125f, -4.5f);
		}

		public void newGame()
		{
			this.exist_item_data = false;
			for (int i = 0; i < 8; i++)
			{
				this.ACell[i].Clear();
			}
			this.cur_select = -2;
			this.deactivate();
			this.submit_select = -1;
			this.submit_t = 0;
		}

		private void fineInfo()
		{
			this.exist_item_data = false;
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				itCell.cacheClear();
				if (itCell.Itm == null)
				{
					itCell.Info = null;
				}
				else
				{
					itCell.Info = this.IMNG.getInventory().getInfo(itCell.Itm);
					this.exist_item_data = true;
				}
			}
		}

		public void fineCurrentUseable()
		{
			PR pr = this.IMNG.Mp.getKeyPr() as PR;
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				this.fineCurrentUseable(pr, itCell);
			}
		}

		private void fineCurrentUseable(PR Pr, UseItemSelector.ItCell Cl)
		{
			if (Cl.Itm == null || Cl.Info == null || Cl.getCountTotal() == 0)
			{
				Cl.current_useable = UseItemSelector.ItCell.USE_TYPE.NO_ITEM;
				return;
			}
			ItemStorage inventory = this.IMNG.getInventory();
			if (Pr != null && Pr.canUseItem(Cl.Itm, inventory, true) != "")
			{
				Cl.current_useable = UseItemSelector.ItCell.USE_TYPE.NOT_SUITABLE;
				return;
			}
			Cl.current_useable = ((Pr != null && Cl.Itm.Use(Pr, inventory, Cl.getGrade(), null) > 0) ? ((Cl.getCount() == 0) ? UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE : UseItemSelector.ItCell.USE_TYPE.USEABLE) : UseItemSelector.ItCell.USE_TYPE.NOT_USEABLE);
		}

		private void drawCell(UseItemSelector.ItCell Cl, float x, float y, float iconscale, float w, float alpha, MeshDrawer Md, MeshDrawer MdIco, MeshDrawer MdStripe, bool current_useable0, bool selecting, bool submit_select)
		{
			float num = 0f;
			PR pr = this.IMNG.Mp.getKeyPr() as PR;
			bool flag = pr != null && pr.Ser.getLevel(SER.CONFUSE) >= 2;
			if (selecting)
			{
				num = X.COSIT(24f);
			}
			bool flag2 = current_useable0 || Cl.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE || Cl.current_useable == UseItemSelector.ItCell.USE_TYPE.USEABLE;
			if (Cl.Itm == null)
			{
				Md.Col = (selecting ? C32.MulA(uint.MaxValue, alpha) : C32.MulA(4289506476U, alpha));
			}
			else
			{
				Md.ColGrd.Set(uint.MaxValue);
				MdIco.ColGrd.Set(flag ? C32.d2c(4288848546U) : Cl.Itm.getColor(this.IMNG.getInventory()));
				Md.ColGrd.Set(uint.MaxValue);
				if (!submit_select && !flag2)
				{
					Md.Col = C32.d2c(4285359718U);
					MdIco.ColGrd.blend(4284439387U, 0.8f);
					Md.ColGrd.blend(4284439387U, 0.8f);
				}
				else
				{
					Md.Col = C32.d2c(4279900698U);
				}
				if (!submit_select)
				{
					Md.Col = C32.MulA(Md.Col, alpha * 0.7f);
					Md.KadoPolyRect(x, y, w, w, w * 0.12f, 1, 0f, false, 0f, 0f, false);
				}
				Color32 c = MdIco.ColGrd.mulA(alpha).C;
				Md.ColGrd.mulA(alpha);
				MdIco.Col = c;
				int num2 = (flag ? 36 : Cl.Itm.getIcon(this.IMNG.getInventory(), null));
				if (X.BTW(0f, (float)num2, (float)MTR.AItemIcon.Length))
				{
					if (submit_select)
					{
						MdIco.Col = C32.MulA(4278190080U, alpha);
					}
					else if (selecting)
					{
						MdIco.Col = C32.MulA(MdIco.Col, 0.78f + 0.22f * num);
					}
					if (!submit_select && !current_useable0 && Cl.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE)
					{
						MdIco.Col = C32.MulA(MdIco.Col, 0.88f);
					}
					MdIco.RotaPF(x - w * 0.12f, y + w * 0.12f, iconscale, iconscale, 0f, MTR.AItemIcon[num2], false, false, false, uint.MaxValue, false, 0);
				}
				int grade = Cl.getGrade();
				float num3 = iconscale * 0.5f;
				MdIco.Col = Md.ColGrd.C;
				if (!current_useable0 && Cl.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE)
				{
					MdIco.Col = C32.MulA(MdIco.Col, 0.7f);
				}
				MdIco.RotaPF(x, y - w * 0.3f, num3, num3, 0f, MTR.AItemGradeStars[15 + grade], false, false, false, uint.MaxValue, false, 0);
				STB stb = TX.PopBld(null, 0);
				stb.Add("x");
				if (Cl.Info != null)
				{
					if (flag)
					{
						stb.Add("??");
					}
					else
					{
						stb.Add(Cl.getCount());
					}
					stb.Add('/');
					if (flag)
					{
						stb.Add("??");
					}
					else
					{
						stb.Add(Cl.getCountTotal());
					}
					if (Cl.sort >= UseItemSelector.SORT.ASCEND)
					{
						MdIco.RotaPF(x + 40f * num3, y - w * 0.3f - 5f * num3, num3, num3, (Cl.sort == UseItemSelector.SORT.ASCEND) ? 3.1415927f : 0f, MTRX.getPF("small_b_arrow"), false, false, false, uint.MaxValue, false, 0);
					}
				}
				MTRX.ChrL.marginw--;
				MTRX.ChrL.DrawScaleStringTo(MdIco, stb, x + w * 0.16f, y + w * 0.12f - 8f * num3, num3, num3, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
				MTRX.ChrL.marginw++;
				TX.ReleaseBld(stb);
				Md.Col = (submit_select ? Md.ColGrd.blend(c, 0.8f).C : Md.ColGrd.blend(c, (Cl.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE) ? 0.25f : 0.5f).C);
			}
			Md.KadoPolyRect(x, y, w, w, w * 0.12f, 1, (float)(submit_select ? 0 : 2), false, 0f, 0f, false);
			if (MdStripe != null && selecting && !submit_select)
			{
				float num4 = w + 10f + num * 5.5f;
				Md.Col = C32.MulA(Md.Col, 0.85f + 0.15f * num);
				Md.KadoPolyRect(x, y, num4, num4, num4 * 0.12f, 1, 3f, false, 0f, 0f, false);
				MdStripe.Col = C32.MulA(Md.Col, 0.25f);
				MdStripe.uvRectN(X.Cos(-0.7853982f), X.Sin(-0.7853982f));
				MdStripe.allocUv2(8, false).Uv2((float)(flag2 ? 10 : 30), flag2 ? 0.5f : 0.02f, false);
				MdStripe.KadoPolyRect(x, y, w - 8f, w - 8f, w * 0.12f, 1, 0f, false, 0f, 0f, false);
				MdStripe.allocUv2(0, true);
			}
		}

		public void drawMesh(float x, float y, float iconscale, float tz, MeshDrawer Md, MeshDrawer MdIco, MeshDrawer MdStripe, float w, int selecting, int submit_select = -2)
		{
			if (submit_select == -2)
			{
				submit_select = this.submit_select;
			}
			float num = X.Abs(tz);
			float num2 = (w * 0.0625f + w) * (0.25f + 0.75f * X.ZSIN((tz < 0f) ? 1f : num));
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				this.drawCell(itCell, x + (float)CAim._XD(i, 1) * num2, y + (float)CAim._YD(i, 1) * num2, iconscale, w, num, Md, MdIco, MdStripe, tz < 0f, i == selecting, i == submit_select);
			}
		}

		protected override bool drawEd(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!base.drawEd(Ef, Ed))
			{
				return false;
			}
			float num = 1f / this.IMNG.M2D.Cam.getScale(true);
			Vector3 vector = this.IMNG.M2D.Cam.PosMainTransform + this.CenterPos * num;
			MeshDrawer mesh = Ef.GetMesh("useitemsel", MTRX.MtrMeshNormal, false);
			MeshDrawer meshImg = Ef.GetMeshImg("useitemsel", MTRX.MIicon, BLEND.NORMAL, false);
			mesh.base_z -= 1f;
			meshImg.base_z -= 1.01f;
			mesh.base_x = (meshImg.base_x = vector.x);
			mesh.base_y = (meshImg.base_y = vector.y);
			if (this.ed_quiting && this.PeSlow == null)
			{
				float num2 = X.ZLINE(Ed.t, 40f);
				if (num2 >= 1f || this.submit_select < 0 || (this.EdDraw != null && Ed != this.EdDraw))
				{
					return false;
				}
				MeshDrawer meshImg2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.MUL, false);
				meshImg2.base_x = vector.x;
				meshImg2.base_y = vector.y;
				mesh.Col = mesh.ColGrd.White().mulA(X.ZLINE(2f - 2f * num2)).C;
				float num3 = 170f * num;
				meshImg2.Col = meshImg2.ColGrd.Set(4282006084U).mulA((0.85f + X.COSIT(24f) * 0.1f) * (1f - num2)).C;
				meshImg2.initForImg(MTRX.EffBlurCircle245, 0);
				float num4 = num3 * (float)CAim._XD(this.submit_select, 1);
				float num5 = num3 * (float)CAim._YD(this.submit_select, 1);
				meshImg2.Rect(num4, num5, 160f * num * 2.5f, 160f * num * 2.5f, false);
				this.drawCell(this.ACell[this.submit_select], num4, num5, 2f * num, 160f * num, 1f - num2, mesh, meshImg, null, true, false, X.ANMT(2, 4f) == 0);
				meshImg.initForImg(MTRX.EffCircle128, 0);
			}
			else
			{
				float num6 = X.ZLINE(Ed.t, 12f);
				MeshDrawer mesh2 = Ef.GetMesh("useitemsel", MTRX.MtrMeshStriped, false);
				mesh2.base_z -= 1.02f;
				mesh2.base_x = vector.x;
				mesh2.base_y = vector.y;
				this.drawMesh(0f, 0f, 2f * num, num6, mesh, meshImg, mesh2, 160f * num, this.cur_select, (this.select_time > 0 && IN.totalframe - this.select_time > 10) ? (-1) : this.submit_select);
			}
			if (this.select_time > 0 && this.submit_select >= 0)
			{
				float num7 = 170f * num;
				float num8 = num7 * (float)CAim._XD(this.submit_select, 1);
				float num9 = num7 * (float)CAim._YD(this.submit_select, 1);
				this.PtO.drawTo(meshImg, meshImg.base_x * 64f + num8, meshImg.base_y * 64f + num9, 0f, 0, (float)(IN.totalframe - this.select_time), 0f);
			}
			return true;
		}

		private void selectInit()
		{
			this.submit_select = -1;
			this.naname_lock = 0;
			this.deactivateEffect(true);
			this.fineInfo();
			if (this.IMNG.Mp.getKeyPr() as PR != null)
			{
				this.fineCurrentUseable();
				if (this.exist_item_data)
				{
					base.selectInit(this.IMNG.Mp.setEDT("magic_selector", this.FD_drawEd, 0f), 20f, -1f, 0.0625f);
					this.ed_quiting = false;
					this.select_time = 0;
					this.cur_select = -1;
					if (this.TxDetail == null)
					{
						this.TxDetail = NEL.CreateBottomRightText("USel-detail", 0f);
						this.TxKD = NEL.CreateBottomRightText("USel-KD", -0.01f);
						this.TxKD.Align(ALIGN.CENTER);
						this.TxDetail.Align(ALIGN.CENTER).LetterSpacing(0.95f).LineSpacing(0.95f);
						this.TxDetail.transform.SetParent(UIBase.Instance.transform, false);
						this.TxKD.transform.SetParent(this.TxDetail.transform, false);
						FillImageBlock.FnDrawInFIB fnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.FbDrawLR);
						for (int i = 0; i < 2; i++)
						{
							FillImageBlock fillImageBlock = IN.CreateGob(this.TxKD.gameObject, "-Fb_grade_" + i.ToString()).AddComponent<FillImageBlock>();
							if (i == 0)
							{
								this.FbGradeL = fillImageBlock;
							}
							else
							{
								this.FbGradeR = fillImageBlock;
							}
							IN.setZ(fillImageBlock.transform, -0.01f);
							fillImageBlock.alignx = ALIGN.CENTER;
							fillImageBlock.letterSpacing = this.TxDetail.letter_spacing;
							fillImageBlock.lineSpacing = 1.4f;
							fillImageBlock.TxCol = this.TxDetail.TextColor;
							fillImageBlock.TxBorderCol = this.TxDetail.BorderColor;
							fillImageBlock.size = 16f;
							fillImageBlock.FnDrawFIB = fnDrawInFIB;
							fillImageBlock.StartFb(" ", null, true);
							M2DBase.Instance.addValotAddition(fillImageBlock);
							float num = 165f;
							IN.PosP2(fillImageBlock.transform, (float)X.MPF(i == 1) * num * 0.77f, num * 0.05f);
						}
					}
					this.TxDetail.gameObject.SetActive(true);
					this.TxKD.gameObject.SetActive(false);
					this.FbGradeL.gameObject.SetActive(true);
					this.FbGradeR.gameObject.SetActive(true);
					this.FbGradeL.alpha = 0f;
					this.FbGradeR.alpha = 0f;
				}
			}
		}

		public void selectQuit(bool can_use_item)
		{
			if (this.cur_select < 0 || !can_use_item)
			{
				this.deactivate();
				return;
			}
			PR pr = this.IMNG.Mp.getKeyPr() as PR;
			bool flag;
			if (CFG.item_usel_type == CFG.USEL_TYPE.D_RELEASE && this.useCurrentCursor(out flag))
			{
				this.cur_select = -2;
				if (this.EdDraw != null)
				{
					this.ed_quiting = true;
					this.EdDraw.t = 0f;
				}
				if (this.PeDarken != null)
				{
					this.deactivateEffect(false);
				}
			}
			else
			{
				this.deactivate();
			}
			if (pr != null)
			{
				pr.jump_hold_lock = true;
			}
		}

		public bool useCurrentCursor(out bool deactivate_flag)
		{
			deactivate_flag = false;
			if (this.cur_select >= 0)
			{
				UseItemSelector.ItCell itCell = this.ACell[this.cur_select];
				PR pr = this.IMNG.Mp.getKeyPr() as PR;
				if (itCell.Itm == null || itCell.Info == null || pr == null || !itCell.Itm.useable)
				{
					return false;
				}
				int grade = itCell.getGrade();
				ItemStorage inventory = this.IMNG.getInventory();
				if (itCell.Info.getCount(grade) <= 0 || pr.canUseItem(itCell.Itm, inventory, true) != "")
				{
					return false;
				}
				int num = itCell.Itm.Use(pr, this.IMNG.getInventory(), grade, pr);
				if (num > 0)
				{
					this.submit_select = this.cur_select;
					this.IMNG.getInventory().Reduce(itCell.Itm, 1, grade, true);
					SND.Ui.play("value_assign", false);
					PostEffect.IT.addTimeFixedEffect(pr.TeCon.setColorBlink(3f, 60f, 0.7f, 13434832, 0), 1f);
					pr.PtcVar("maxt", 25.0).PtcVar("saf", -10.0);
					if (!itCell.Itm.is_bomb)
					{
						pr.PtcHld.PtcSTTimeFixed("general_circle_t", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
					}
					UIPicture.Instance.useItem(itCell.Itm, null);
					UILog.Instance.AddConsumeItem(itCell.Itm, grade, itCell.getCountTotal());
					this.select_time = IN.totalframe;
					if (num == 2)
					{
						deactivate_flag = true;
					}
					return true;
				}
			}
			return false;
		}

		public bool run(bool input_on, bool can_use_item_on_quit)
		{
			bool flag = false;
			if (this.cur_select < -1)
			{
				if (!input_on)
				{
					return false;
				}
				this.selectInit();
				flag = true;
			}
			base.runPE();
			bool flag2 = false;
			if (this.isSelecting())
			{
				if (!input_on)
				{
					this.selectQuit(can_use_item_on_quit);
					return false;
				}
				flag2 = true;
				UIStatus.showHold(10, true);
				flag = this.fineCurs(true) || flag;
				if ((CFG.item_usel_type == CFG.USEL_TYPE.D_TAP_Z || CFG.item_usel_type == CFG.USEL_TYPE.Z_PUSH) && IN.isSubmitPD(1))
				{
					PR pr = this.IMNG.Mp.getKeyPr() as PR;
					PR.PunchDecline(20, false);
					pr.jump_hold_lock = true;
					IN.clearSubmitPushDown(true);
					bool flag3;
					if (this.useCurrentCursor(out flag3))
					{
						flag = true;
						this.fineInfo();
						this.fineCurrentUseable();
						if (flag3)
						{
							this.selectQuit(can_use_item_on_quit);
							return false;
						}
						if (this.ACell[this.cur_select].slideGrade(0, true))
						{
							flag = true;
							this.fineCurrentUseable(pr, this.ACell[this.cur_select]);
						}
					}
				}
				if (this.cur_select >= 0 && ((IN.isLTabPD() && this.ACell[this.cur_select].slideGrade(-1, true)) || (IN.isRTabPD() && this.ACell[this.cur_select].slideGrade(1, true))))
				{
					flag = true;
					base.playSnd("tool_laysel");
					this.fineCurrentUseable(this.IMNG.Mp.getKeyPr() as PR, this.ACell[this.cur_select]);
				}
			}
			if (flag && this.TxKD != null)
			{
				if (this.cur_select < 0)
				{
					this.TxKD.gameObject.SetActive(false);
					this.FbGradeL.alpha = 0f;
					this.FbGradeR.alpha = 0f;
				}
				else
				{
					UseItemSelector.ItCell itCell = this.ACell[this.cur_select];
					using (STB stb = TX.PopBld(null, 0))
					{
						if (itCell.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_CURRENT_GRADE)
						{
							stb.Add(" <img mesh=\"alert\" /> ");
						}
						else if (itCell.current_useable == UseItemSelector.ItCell.USE_TYPE.NOT_SUITABLE)
						{
							stb.AddTxA("KD_ItemSel_use_disable", false);
						}
						else if (itCell.current_useable == UseItemSelector.ItCell.USE_TYPE.NOT_USEABLE)
						{
							stb.AddTxA("KD_ItemSel_use_disable", false);
						}
						else if (itCell.current_useable == UseItemSelector.ItCell.USE_TYPE.NO_ITEM)
						{
							stb.AddTxA("KD_ItemSel_no_item", false);
						}
						else
						{
							stb.AddTxA((CFG.item_usel_type == CFG.USEL_TYPE.D_RELEASE) ? "KD_ItemSel_use_release" : "KD_ItemSel_use_submit", false);
						}
						this.TxKD.Txt(stb);
					}
					this.TxKD.gameObject.SetActive(true);
					float num = 165f;
					IN.PosP2(this.TxKD.transform, num * (float)CAim._XD(this.cur_select, 1), num * (float)CAim._YD(this.cur_select, 1) - 30f);
					int num2;
					if (itCell.slideGrade(-1, out num2, false))
					{
						this.FbGradeL.alpha = 1f;
						using (STB stb2 = TX.PopBld(null, 0))
						{
							stb2.AddTxA("KD_ItemSel_grade_change", false).TxRpl("<img mesh=\"nel_item_grade.", (float)num2, "\" tx_color />").TxRpl(itCell.getCount())
								.TxRpl("<key ltab />");
							this.FbGradeL.Txt(stb2);
						}
						this.FbGradeL.text_alpha = 0f;
						this.FbGradeL.redraw_flag = true;
						this.FbGradeL.alpha = 1f;
					}
					else
					{
						this.FbGradeL.alpha = 0f;
					}
					if (itCell.slideGrade(1, out num2, false))
					{
						this.FbGradeR.alpha = 1f;
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb3.AddTxA("KD_ItemSel_grade_change", false).TxRpl("<img mesh=\"nel_item_grade.", (float)num2, "\" tx_color />").TxRpl(itCell.getCount())
								.TxRpl("<key rtab />");
							this.FbGradeR.Txt(stb3);
						}
						this.FbGradeR.text_alpha = 0f;
						this.FbGradeR.redraw_flag = true;
						this.FbGradeR.alpha = 1f;
					}
					else
					{
						this.FbGradeR.alpha = 0f;
					}
				}
				Vector3 centerPos = this.CenterPos;
				centerPos.x += this.IMNG.M2D.ui_shift_x * 0.015625f;
				this.TxDetail.transform.position = centerPos;
				this.fineCenterString(this.TxDetail, false);
			}
			return flag2;
		}

		public bool fineCurs(bool no_deactivate = false)
		{
			if (this.cur_select < -1)
			{
				return false;
			}
			if (this.naname_lock > 0)
			{
				this.naname_lock--;
			}
			UseItemSelector.SKEY skey = (UseItemSelector.SKEY)0;
			if (this.isUiActive())
			{
				skey |= (IN.isLO(0) ? UseItemSelector.SKEY.LO : ((UseItemSelector.SKEY)0)) | (IN.isTO(0) ? UseItemSelector.SKEY.TO : ((UseItemSelector.SKEY)0)) | (IN.isRO(0) ? UseItemSelector.SKEY.RO : ((UseItemSelector.SKEY)0)) | (IN.isBO(0) ? UseItemSelector.SKEY.BO : ((UseItemSelector.SKEY)0)) | (IN.isLP(1) ? UseItemSelector.SKEY.LP : ((UseItemSelector.SKEY)0)) | (IN.isTP(1) ? UseItemSelector.SKEY.TP : ((UseItemSelector.SKEY)0)) | (IN.isRP(1) ? UseItemSelector.SKEY.RP : ((UseItemSelector.SKEY)0)) | (IN.isBP(1) ? UseItemSelector.SKEY.BP : ((UseItemSelector.SKEY)0)) | ((!IN.isMenuO(0) && (IN.isMovingPD() || IN.isAtkPD(1) || IN.isSubmitPD(1) || IN.isMagicPD(1) || IN.isUiSortPD())) ? UseItemSelector.SKEY.ActP : ((UseItemSelector.SKEY)0));
			}
			else
			{
				M2MoverPr keyPr = this.IMNG.Mp.getKeyPr();
				if (keyPr == null)
				{
					return false;
				}
				skey |= (keyPr.isLO(0) ? UseItemSelector.SKEY.LO : ((UseItemSelector.SKEY)0)) | (keyPr.isTO(0) ? UseItemSelector.SKEY.TO : ((UseItemSelector.SKEY)0)) | (keyPr.isRO(0) ? UseItemSelector.SKEY.RO : ((UseItemSelector.SKEY)0)) | (keyPr.isBO(0) ? UseItemSelector.SKEY.BO : ((UseItemSelector.SKEY)0)) | (keyPr.isLP(1) ? UseItemSelector.SKEY.LP : ((UseItemSelector.SKEY)0)) | (keyPr.isTP(1) ? UseItemSelector.SKEY.TP : ((UseItemSelector.SKEY)0)) | (keyPr.isRP(1) ? UseItemSelector.SKEY.RP : ((UseItemSelector.SKEY)0)) | (keyPr.isBP() ? UseItemSelector.SKEY.BP : ((UseItemSelector.SKEY)0)) | (keyPr.isActionPD() ? UseItemSelector.SKEY.ActP : ((UseItemSelector.SKEY)0));
			}
			if (this.manual_select)
			{
				if ((skey & UseItemSelector.SKEY.ActP) == (UseItemSelector.SKEY)0)
				{
					return false;
				}
				this.manual_select = false;
			}
			bool flag = (skey & UseItemSelector.SKEY.LO) > (UseItemSelector.SKEY)0;
			bool flag2 = (skey & UseItemSelector.SKEY.RO) > (UseItemSelector.SKEY)0;
			int num;
			if (flag && flag2)
			{
				if ((skey & UseItemSelector.SKEY.LP) != (UseItemSelector.SKEY)0)
				{
					num = -1;
				}
				else if ((skey & UseItemSelector.SKEY.RP) != (UseItemSelector.SKEY)0)
				{
					num = 1;
				}
				else
				{
					num = ((this.cur_select >= 0) ? CAim._XD(this.cur_select, 1) : 0);
				}
			}
			else
			{
				num = (flag ? (-1) : (flag2 ? 1 : 0));
			}
			flag = (skey & UseItemSelector.SKEY.TO) > (UseItemSelector.SKEY)0;
			flag2 = (skey & UseItemSelector.SKEY.BO) > (UseItemSelector.SKEY)0;
			int num2;
			if (flag && flag2)
			{
				if ((skey & UseItemSelector.SKEY.TP) != (UseItemSelector.SKEY)0)
				{
					num2 = 1;
				}
				else if ((skey & UseItemSelector.SKEY.BP) != (UseItemSelector.SKEY)0)
				{
					num2 = -1;
				}
				else
				{
					num2 = ((this.cur_select >= 0) ? CAim._YD(this.cur_select, 1) : 0);
				}
			}
			else
			{
				num2 = (flag ? 1 : (flag2 ? (-1) : 0));
			}
			int num3 = (int)((num == 0 && num2 == 0) ? ((AIM)4294967295U) : CAim.get_aim2(0f, 0f, (float)num, (float)num2, false));
			bool flag3 = false;
			if (CAim.is_naname((AIM)this.cur_select) && this.naname_lock > 0)
			{
				if (num3 < 0)
				{
					this.naname_lock = 0;
				}
				else if (!CAim.is_naname((AIM)num3) && CAim.nanameMarging((AIM)this.cur_select, (AIM)num3))
				{
					flag3 = true;
					num3 = this.cur_select;
				}
			}
			if (!no_deactivate && num3 >= 0 && this.ACell[num3].Itm == null)
			{
				num3 = this.cur_select;
			}
			if (num3 >= 0 && CAim.is_naname((AIM)num3))
			{
				if (!flag3)
				{
					this.naname_lock = 10;
				}
			}
			else
			{
				this.naname_lock = 0;
			}
			if (this.cur_select == num3)
			{
				return false;
			}
			if (num3 < 0 && this.cur_select >= 0 && no_deactivate)
			{
				return false;
			}
			base.playSnd((num3 >= 0) ? "tool_laysel" : "tool_drag_quit");
			this.cur_select = num3;
			return true;
		}

		public override void deactivateEffect(bool clear_efdraw = true)
		{
			base.deactivateEffect(clear_efdraw);
			if (this.TxDetail != null)
			{
				this.TxDetail.gameObject.SetActive(false);
				this.TxKD.gameObject.SetActive(false);
			}
		}

		public override void deactivate()
		{
			base.deactivate();
			this.cur_select = -2;
			this.manual_select = false;
			if (this.BtnSel != null)
			{
				this.BtnSel.get_Skin().fine_flag = true;
				this.submit_select = -1;
				this.submit_t = 0;
			}
			this.BtnSel = null;
			this.UiItmTarget = null;
		}

		private void fineCenterString(TextRenderer Tx, bool for_ui = false)
		{
			PR pr = this.IMNG.Mp.getKeyPr() as PR;
			Tx.effect_confusion = pr != null && pr.Ser.getLevel(SER.CONFUSE) >= 2;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.cur_select <= -1)
				{
					stb.AddTxA("KD_ItemSel_noselect", false);
				}
				else
				{
					UseItemSelector.ItCell itCell = this.ACell[this.cur_select];
					if (for_ui)
					{
						if (!this.ui_show_item_detail)
						{
							if (itCell.Itm != this.UiItmTarget)
							{
								stb.AddTxA("KD_ItemSel_assign0", false);
							}
							else
							{
								stb.AddTxA("KD_ItemSel_assign1", false).Add("\n").AddTxA("ItemSel_sort_now", false);
								if (itCell.sort < UseItemSelector.SORT.ASCEND)
								{
									stb.Add("<img mesh=\"nel_item_grade.").Add((int)itCell.sort).Add("\" />");
								}
								else
								{
									stb.AddTxA((itCell.sort == UseItemSelector.SORT.ASCEND) ? "ItemSel_sort_ascend" : "ItemSel_sort_descend", false);
								}
							}
						}
						else
						{
							this.getDetailForItm(stb, itCell.Itm, itCell.Info, itCell.getGrade());
						}
						if (itCell.Itm != null)
						{
							stb.Add("\n\n").AddTxA(this.ui_show_item_detail ? "KD_ItemSel_item_revert" : "KD_ItemSel_item_info", false);
						}
					}
					else
					{
						this.getDetailForItm(stb, itCell.Itm, itCell.Info, itCell.getGrade());
					}
				}
				Tx.Txt(stb);
			}
		}

		private void getDetailForItm(STB Stb, NelItem Itm, ItemStorage.ObtainInfo Info, int grade)
		{
			if (Info == null)
			{
				return;
			}
			Itm.getLocalizedName(Stb, grade);
			Stb.Add("\n\n");
			Itm.getDetail(Stb, null, grade, Info, true, false, false);
		}

		private bool FbDrawLR(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (alpha == 0f || FI.textIs(""))
			{
				return FI.alpha == 0f;
			}
			Md.Col = Md.ColGrd.Black().mulA(alpha * FI.text_alpha * 0.8f).C;
			Md.ColGrd.mulA(0f);
			Md.KadomaruRect(0f, 0f, FI.get_swidth_px() + 90f, FI.get_sheight_px() + 40f, 50f, 0f, false, 1f, 0f, false);
			update_meshdrawer = true;
			return FI.text_alpha >= 1f;
		}

		public void initUi(UiBoxDesigner Bx, NelItem Itm, bool recreate_btn)
		{
			this.fineInfo();
			this.submit_select = -1;
			this.naname_lock = 0;
			this.ui_show_item_detail = false;
			if (recreate_btn)
			{
				Bx.addP(new DsnDataP("", false)
				{
					text = TX.Get("ItemSel_assign", ""),
					swidth = Bx.use_w,
					sheight = 40f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					size = 16f,
					html = true,
					TxCol = MTRX.ColWhite
				}, false);
				Bx.Br();
				this.BtnSel = Bx.addButtonT<aBtnUItemSel>(new DsnDataButton
				{
					name = "usel_btn",
					w = Bx.use_w,
					h = Bx.use_h,
					skin = "normal"
				});
			}
			else
			{
				this.BtnSel = Bx.getBtn("usel_btn") as aBtnUItemSel;
			}
			this.BtnSel.Select(true);
			this.fineCenterString(this.BtnSel.TxCenter, true);
			this.UiItmTarget = Itm;
			SND.Ui.play(this.select_init_snd_cue, false);
			this.cur_select = -1;
		}

		public bool runUi()
		{
			if (this.BtnSel == null || this.UiItmTarget == null)
			{
				return false;
			}
			if (IN.isCancel() || IN.isMenuPD(1) || IN.isItmU(1))
			{
				SND.Ui.play("cancel", false);
				return false;
			}
			bool flag = this.fineCurs(true);
			if (this.cur_select >= 0 && IN.isUiRemPD() && this.ACell[this.cur_select].Itm != null)
			{
				SND.Ui.play("reset_var", false);
				this.ACell[this.cur_select].Clear();
				this.submit_t = 0;
				this.submit_select = this.cur_select;
			}
			if (this.submit_select >= 0)
			{
				if (this.submit_t == 0)
				{
					flag = true;
				}
				int num = this.submit_t + 1;
				this.submit_t = num;
				if (num >= 8)
				{
					this.BtnSel.get_Skin().fine_flag = true;
					this.submit_t = 0;
					this.submit_select = -1;
				}
			}
			if (IN.isUiAddPD())
			{
				SND.Ui.play("tool_drag_init", false);
				this.ui_show_item_detail = !this.ui_show_item_detail;
				flag = true;
			}
			if (flag)
			{
				this.BtnSel.fineCursor();
				this.fineCenterString(this.BtnSel.TxCenter, true);
			}
			return true;
		}

		public bool uiClicked()
		{
			if (this.cur_select < 0 || this.BtnSel == null || this.UiItmTarget == null)
			{
				return false;
			}
			ItemStorage.ObtainInfo info = this.IMNG.getInventory().getInfo(this.UiItmTarget);
			if (info == null || info.total <= 0)
			{
				return false;
			}
			UseItemSelector.ItCell itCell = this.ACell[this.cur_select];
			int num;
			if (itCell.Itm == this.UiItmTarget)
			{
				num = (int)(itCell.sort + 1);
				if (num > 6)
				{
					num = 0;
				}
			}
			else
			{
				num = (int)((itCell.Itm == null) ? UseItemSelector.SORT.GRADE0 : itCell.sort);
			}
			int num2 = num;
			while (num2 < 5 && info.getCount(num2) <= 0)
			{
				num2++;
			}
			this.exist_item_data = true;
			itCell.sort = (UseItemSelector.SORT)num2;
			itCell.Itm = this.UiItmTarget;
			itCell.Info = info;
			itCell.cacheClear();
			this.submit_select = this.cur_select;
			this.submit_t = 0;
			SND.Ui.play("tool_eraser_init", false);
			if (itCell.Itm != null)
			{
				itCell.Itm.prepareResource(M2DBase.Instance);
			}
			return true;
		}

		public void focusManual(int _aim)
		{
			if (this.cur_select != _aim)
			{
				this.manual_select = true;
				this.cur_select = _aim;
				this.fineCenterString(this.BtnSel.TxCenter, true);
			}
		}

		public bool isUiActive()
		{
			return this.BtnSel != null;
		}

		public bool isSelecting()
		{
			return this.EdDraw != null && this.cur_select >= -1;
		}

		public int getSelectedIndex()
		{
			return this.cur_select;
		}

		public void prepareResource(M2DBase M2D)
		{
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				if (itCell.Itm != null)
				{
					itCell.Itm.prepareResource(M2D);
				}
			}
		}

		public void readBinaryFrom(ByteReader Ba)
		{
			Ba.readByte();
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				string text = Ba.readPascalString("utf-8", false);
				if (TX.noe(text))
				{
					itCell.Clear();
				}
				else
				{
					int num = Ba.readByte();
					NelItem byId = NelItem.GetById(text, false);
					if (byId != null)
					{
						itCell.Itm = byId;
						itCell.sort = (UseItemSelector.SORT)num;
						this.exist_item_data = true;
					}
					else
					{
						itCell.Clear();
					}
				}
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(0);
			for (int i = 0; i < 8; i++)
			{
				UseItemSelector.ItCell itCell = this.ACell[i];
				if (itCell.Itm == null)
				{
					Ba.writeByte(0);
				}
				else
				{
					Ba.writePascalString(itCell.Itm.key, "utf-8");
					Ba.writeByte((int)itCell.sort);
				}
			}
		}

		public const float DEF_WH = 160f;

		public const float MARGIN_RATE = 0.0625f;

		private const int MAX_CELL = 8;

		private UseItemSelector.ItCell[] ACell;

		public readonly NelItemManager IMNG;

		private bool exist_item_data;

		private int cur_select = -2;

		private bool manual_select;

		private int submit_select = -1;

		private int submit_t;

		private int naname_lock;

		private EfParticleOnce PtO = new EfParticleOnce("general_white_circle_l", EFCON_TYPE.FIXED);

		private TextRenderer TxDetail;

		private TextRenderer TxKD;

		private FillImageBlock FbGradeL;

		private FillImageBlock FbGradeR;

		private bool ed_quiting;

		private int select_time;

		private Vector3 CenterPos;

		private aBtnUItemSel BtnSel;

		private NelItem UiItmTarget;

		private bool ui_show_item_detail;

		public class ItCell
		{
			public void Clear()
			{
				this.Itm = null;
				this.Info = null;
				this.sort = UseItemSelector.SORT.GRADE0;
				this.t_grade = byte.MaxValue;
				this.current_useable = UseItemSelector.ItCell.USE_TYPE.NO_ITEM;
			}

			public void cacheClear()
			{
				if (this.Itm == null)
				{
					return;
				}
				if (this.sort < UseItemSelector.SORT.ASCEND)
				{
					this.t_grade = (byte)this.sort;
					return;
				}
				this.t_grade = byte.MaxValue;
				if (this.Info != null)
				{
					for (int i = 0; i < 5; i++)
					{
						int num = ((this.sort == UseItemSelector.SORT.ASCEND) ? i : (4 - i));
						if (this.Info.getCount(num) > 0)
						{
							this.t_grade = (byte)num;
							break;
						}
					}
				}
				if (this.t_grade == 255)
				{
					this.t_grade = ((this.sort == UseItemSelector.SORT.ASCEND) ? 0 : 4);
				}
			}

			public bool slideGrade(int dir, bool execute = true)
			{
				int num;
				return this.slideGrade(dir, out num, execute);
			}

			public bool slideGrade(int dir, out int next_grade, bool execute = true)
			{
				next_grade = (int)this.t_grade;
				if (this.Itm == null || this.Info == null || this.t_grade == 255)
				{
					return false;
				}
				int num = ((dir == 0) ? 3 : ((dir < 0) ? 1 : 2));
				int i = ((dir == 0) ? 0 : 1);
				while (i <= 10)
				{
					int num2 = (int)this.t_grade;
					if (i <= 0)
					{
						goto IL_00C9;
					}
					if (num == 1)
					{
						num2 = (int)this.t_grade - i;
						if (!X.BTW(0f, (float)num2, 5f))
						{
							return false;
						}
						goto IL_00C9;
					}
					else if (num == 2)
					{
						num2 = (int)this.t_grade + i;
						if (!X.BTW(0f, (float)num2, 5f))
						{
							return false;
						}
						goto IL_00C9;
					}
					else
					{
						num2 = (int)this.t_grade + (i - 1) / 2 * X.MPF(i % 2 == 1 == (this.sort == UseItemSelector.SORT.ASCEND));
						if (X.BTW(0f, (float)num2, 5f))
						{
							goto IL_00C9;
						}
					}
					IL_00E8:
					i++;
					continue;
					IL_00C9:
					if (this.Info.getCount(num2) > 0)
					{
						next_grade = num2;
						if (execute)
						{
							this.t_grade = (byte)num2;
						}
						return true;
					}
					goto IL_00E8;
				}
				return false;
			}

			public int getGrade()
			{
				if (this.t_grade == 255)
				{
					this.cacheClear();
				}
				return (int)this.t_grade;
			}

			public int getCountTotal()
			{
				if (this.Info != null)
				{
					return this.Info.total;
				}
				return 0;
			}

			public int getCount()
			{
				if (this.Itm == null || this.Info == null)
				{
					return 0;
				}
				if (this.t_grade == 255)
				{
					this.cacheClear();
				}
				return this.Info.getCount((int)this.t_grade);
			}

			public NelItem Itm;

			public ItemStorage.ObtainInfo Info;

			public UseItemSelector.SORT sort;

			private byte t_grade;

			public UseItemSelector.ItCell.USE_TYPE current_useable;

			public enum USE_TYPE
			{
				USEABLE,
				NO_CURRENT_GRADE,
				NOT_USEABLE,
				NO_ITEM,
				NOT_SUITABLE
			}
		}

		public enum SORT : byte
		{
			GRADE0,
			GRADE1,
			GRADE2,
			GRADE3,
			GRADE4,
			ASCEND,
			DESCEND
		}

		private enum SKEY
		{
			LO = 1,
			TO,
			RO = 4,
			BO = 8,
			LP = 16,
			TP = 32,
			RP = 64,
			BP = 128,
			ActP = 256
		}
	}
}
