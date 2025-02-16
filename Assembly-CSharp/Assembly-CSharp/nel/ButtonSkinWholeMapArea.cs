using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinWholeMapArea : ButtonSkin, IRunAndDestroy
	{
		public event ButtonSkinWholeMapArea.FnSwitchWA FD_FnSwitch = delegate(bool _to_wa)
		{
		};

		public bool is_wa
		{
			get
			{
				return this.t_detail < 0f;
			}
		}

		public bool is_detail
		{
			get
			{
				return this.t_detail >= 0f;
			}
		}

		public bool is_detail_swithing
		{
			get
			{
				return X.BTWS(-30f, this.t_detail, 30f);
			}
		}

		public bool is_wa_fade_animation
		{
			get
			{
				return this.t_wa_fading != -1f;
			}
		}

		public ButtonSkinWholeMapArea(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			int num = 241;
			this.MdGrid = base.makeMesh(MTRX.getMtr(BLEND.MASK, num));
			this.MdMapFill = base.makeMesh(MTRX.getMtr(num));
			this.MdMapMask = base.makeMesh(MTRX.getMtr(num + 512));
			this.MdMapMask.Col = C32.d2c(0U);
			this.MdMapLine = base.makeMesh(MTRX.getMtr(num));
			this.MdMapWA = base.makeMesh(MTR.MIiconL.getMtr(BLEND.NORMALST, num));
			this.MdMapWA_Fading = base.makeMesh(MTR.MIiconL.getMtr(MTR.getShd("Hachan/ShaderGDTSTPencilDraw"), num));
			this.MdMapIco = base.makeMesh(MTRX.MIicon.getMtr(num - 1 + 256));
			this.MdIco = base.makeMesh(this.MdMapIco.getMaterial());
			this.MdFrame = base.makeMesh(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			this.DrawBounds = new DRect("_");
			this.w = ((_w > 0f) ? _w : 540f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 480f) * 0.015625f;
			this.curs_level_x = (this.w - (this.inner_margin * 2f + 27f) * 0.015625f) / this.w;
			this.curs_level_y = -(this.h - (this.inner_margin * 2f + 27f) * 0.015625f) / this.h;
			if (ButtonSkinWholeMapArea.Col == null)
			{
				ButtonSkinWholeMapArea.Col = new C32();
			}
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.fine_continue_flags |= 31U;
			IN.addRunner(this);
		}

		public override ButtonSkin WHPx(float _wpx, float _hpx)
		{
			base.WHPx(_wpx, _hpx);
			this.fine_all = (this.redraw_frame = true);
			this.fineTextPos();
			return this;
		}

		public override void destruct()
		{
			IN.remRunner(this);
			UiBox.FlgLockFocus.Rem("WHOLEMAP");
			this.DragFst.z = 0f;
			base.destruct();
		}

		public void fineWaFading()
		{
			if (this.is_wa)
			{
				if (this.t_wa_fading >= 0f)
				{
					this.t_wa_fading = -1f;
					WAManager.touchFinalizeAll();
				}
				this.switchWA(false, false, false);
			}
		}

		public bool switchWA(bool switch_to_focus_wm = false, bool focus_wm_animating = false, bool play_sound = true)
		{
			if (this.WM == null)
			{
				return false;
			}
			this.quitDragging();
			float num = this.mappos_x;
			float num2 = this.mappos_y;
			float num3 = this.cell_size;
			this.fine_all = true;
			if (this.t_detail >= 0f)
			{
				if (this.AWaAppeared == null)
				{
					this.AWaAppeared = new List<DRect>(4);
				}
				if (!WAManager.initializePositionAtSkin(out this.mappos_x, out this.mappos_y, this.WM.text_key, this.DrawBounds, this.AWaAppeared))
				{
					return false;
				}
				this.curspos_x = this.mappos_x;
				this.curspos_y = this.mappos_y;
				this.animate_time = 0;
				int count = this.AWaAppeared.Count;
				for (int i = 0; i < count; i++)
				{
					TextRenderer textRenderer;
					if (i + 2 >= this.ATxWA.Count)
					{
						textRenderer = this.initTxWARenderer(IN.CreateGob(this.B.gameObject, "-TxWA-" + i.ToString()).AddComponent<TextRenderer>(), true);
						this.ATxWA.Add(textRenderer);
					}
					else
					{
						textRenderer = this.ATxWA[i + 2];
					}
					textRenderer.gameObject.SetActive(true);
					textRenderer.use_valotile = true;
					textRenderer.max_swidth_px = X.Mx(this.AWaAppeared[i].width * 0.55f, 96f);
					textRenderer.auto_wrap = true;
					textRenderer.Txt(TX.Get("Area_" + this.AWaAppeared[i].key, ""));
					textRenderer.alpha = 0f;
				}
				for (int j = this.ATxWA.Count - 1; j >= count + 2; j--)
				{
					this.ATxWA[j].gameObject.SetActive(false);
				}
				this.FocusWM = this.WM;
				this.fast_travel_active = false;
				this.cell_size = 0.6f;
				this.t_detail = -1f;
				this.t_wa_fading = -10f;
				this.fineCurrentQuestDepert(true);
				this.TxLT.BorderCol(MTRX.ColTrnsp);
				if (play_sound)
				{
					SND.Ui.play("map_wa_zoomout", false);
				}
			}
			else
			{
				if (this.t_wa_fading < -10f)
				{
					return false;
				}
				if (switch_to_focus_wm)
				{
					if (this.FocusWM == null)
					{
						return false;
					}
					this.WM = this.FocusWM;
				}
				else
				{
					this.mappos_x = this.curspos_x;
					this.mappos_y = this.curspos_y;
					focus_wm_animating = true;
				}
				float num4;
				float num5;
				if (focus_wm_animating && WAManager.initializePositionAtSkin(out num4, out num5, this.WM.text_key, null, null))
				{
					this.SetCenter(num4, num5, true, true);
				}
				else
				{
					this.animate_time = 0;
				}
				num = this.curspos_x;
				num2 = this.curspos_y;
				if (this.t_wa_fading >= 0f)
				{
					WAManager.touchFinalizeAll();
				}
				this.t_wa_fading = -1f;
				this.fast_travel_active = false;
				this.cell_size = 20f;
				float num6 = 0f;
				float num7 = 0f;
				this.t_detail = 0f;
				if (this.WM != this.M2D.WM.CurWM || !this.WM.fixPlayerPosOnWM(this.M2D.getPrNoel(), ref num6, ref num7))
				{
					num6 = this.WM.Bounds.cx;
					num7 = this.WM.Bounds.cy;
				}
				this.setWholeMapTarget(this.WM, num6, num7);
				this.topright_t = IN.totalframe;
				for (int k = this.ATx.Count - 1; k >= 0; k--)
				{
					this.ATx[k].alpha = 0f;
				}
				this.TxLT.BorderCol(4291150012U);
				if (play_sound)
				{
					SND.Ui.play("map_wa_zoomin", false);
				}
			}
			if (this.FD_FnSwitch != null)
			{
				this.FD_FnSwitch(this.t_detail < 0f);
			}
			this.need_fine_lt_text = (this.need_fine_wmi_cur = true);
			this.WmiCurPos_ = null;
			this.FastTravelFocused = default(WMIconPosition);
			this.dtanim_sx = num;
			this.dtanim_sy = num2;
			this.dtanim_cellsize = num3;
			this.t_zoomin = 30;
			return true;
		}

		public bool need_fine_quest_lt_text
		{
			set
			{
				if (this.see_quest_depert && !this.fast_travel_active_)
				{
					this.need_fine_lt_text = true;
				}
			}
		}

		public void SetCenterDefault(bool animate = true, bool set_curs = true)
		{
			if (!this.is_wa)
			{
				float num = 0f;
				float num2 = 0f;
				if (this.WM != this.M2D.WM.CurWM || !this.WM.fixPlayerPosOnWM(this.M2D.getPrNoel(), ref num, ref num2))
				{
					num = this.WM.Bounds.cx;
					num2 = this.WM.Bounds.cy;
				}
				this.SetCenter(num, num2, animate, set_curs);
				return;
			}
			float num3;
			float num4;
			if (!WAManager.initializePositionAtSkin(out num3, out num4, this.M2D.WM.CurWM.text_key, null, null))
			{
				return;
			}
			this.SetCenter(num3, num4, animate, set_curs);
		}

		public void fineCurrentQuestDepert()
		{
			this.fineCurrentQuestDepert(this.is_wa);
		}

		private void fineCurrentQuestDepert(bool is_wa)
		{
			if (is_wa)
			{
				if (this.see_quest_depert)
				{
					if (this.OQuestDepWA == null)
					{
						this.OQuestDepWA = new BDic<string, QuestTracker.QuestDepertureOnWa>();
					}
					this.OQuestDepWA.Clear();
					this.M2D.QUEST.listupDepertureWa(this.OQuestDepWA);
					return;
				}
				this.OQuestDepWA = null;
				return;
			}
			else
			{
				if (this.see_quest_depert)
				{
					if (this.AQuestDep == null)
					{
						this.AQuestDep = new List<QuestTracker.QuestDepertureOnMap>(4);
					}
					this.AQuestDep.Clear();
					this.M2D.QUEST.listupDepertureInCurrentWm(this.AQuestDep, this.WM.text_key);
					return;
				}
				this.AQuestDep = null;
				return;
			}
		}

		public void setWholeMapTarget(WholeMapItem _WM, float cen_x, float cen_y)
		{
			this.WM = _WM;
			this.t_zoomin = 30;
			this.DrawBounds.Set(0f, 0f, 0f, 0f);
			this.SetCenter(cen_x, cen_y, false, true);
			this.fine_all = (this.redraw_frame = true);
			if (this.fast_travel_active_)
			{
				if (this.WM == this.M2D.WM.CurWM)
				{
					if (this.ABenchList == null)
					{
						this.ABenchList = this.WM.getNoticedIconList(WMIcon.TYPE.BENCH);
					}
				}
				else
				{
					this.ABenchList = null;
				}
			}
			else
			{
				this.ABenchList = null;
			}
			this.fineCurrentQuestDepert(false);
			if (this.ATx == null)
			{
				this.ATx = new List<TextRenderer>(2);
				this.ATxWA = new List<TextRenderer>(2);
				int num = this.ATx.Capacity;
				for (int i = 0; i < num; i++)
				{
					TextRenderer textRenderer = IN.CreateGob(this.Gob, "-textTR-" + i.ToString()).AddComponent<TextRenderer>();
					this.ATx.Add(textRenderer);
					textRenderer.Bold(true).Align(ALIGN.RIGHT).AlignY(ALIGNY.TOP);
					textRenderer.use_valotile = true;
					textRenderer.monospace = true;
				}
				num = 2;
				for (int j = 0; j < num; j++)
				{
					TextRenderer textRenderer2 = IN.CreateGob(this.Gob, "-textTWA-" + j.ToString()).AddComponent<TextRenderer>();
					this.ATxWA.Add(textRenderer2);
					textRenderer2.use_valotile = true;
					textRenderer2.Bold(true).Align((j == 1) ? ALIGN.LEFT : ALIGN.RIGHT).AlignY(ALIGNY.MIDDLE);
				}
				this.TxT = IN.CreateGob(this.Gob, "-textT").AddComponent<TextRenderer>();
				this.TxT.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
				this.TxT.Col(4283780170U);
				this.TxT.use_valotile = true;
				this.TxLT = IN.CreateGob(this.Gob, "-textLT").AddComponent<TextRenderer>();
				this.TxLT.auto_wrap = true;
				this.TxLT.alignx = ALIGN.LEFT;
				this.TxLT.aligny = ALIGNY.TOP;
				this.TxLT.max_swidth_px = this.w * 64f * 0.7f - 20f;
				this.TxLT.html_mode = true;
				this.TxLT.BorderCol(4291150012U).Size(14f);
				this.TxLT.use_valotile = true;
				this.TxLT.auto_wrap = true;
				if (!this.show_topleft_text)
				{
					this.TxLT.gameObject.SetActive(false);
				}
				IN.setZ(this.TxLT.transform, -0.04f);
				for (int k = this.ATx.Count - 1; k >= 0; k--)
				{
					TextRenderer textRenderer3 = this.ATx[k];
					if (this.B.Container != null)
					{
						textRenderer3.StencilRef(this.B.Container.stencil_ref);
					}
					textRenderer3.html_mode = true;
					textRenderer3.BorderCol(4291150012U).Size(14f);
					textRenderer3.auto_condense = true;
					textRenderer3.max_swidth_px = this.w * 64f * 0.3f;
					textRenderer3.Col(4280557596U).LetterSpacing(0.88f);
				}
				for (int l = this.ATxWA.Count - 1; l >= 0; l--)
				{
					TextRenderer textRenderer4 = this.initTxWARenderer(this.ATxWA[l], false);
					textRenderer4.Txt((l == 0) ? TX.Get("KD_map_switch_wa", "") : TX.Get("KD_map_switch_detail", ""));
					if (l == 1)
					{
						textRenderer4.alpha = 0f;
					}
				}
				if (this.B.Container != null)
				{
					this.TxT.StencilRef(this.B.Container.stencil_ref);
				}
				this.TxT.Size(16f);
				this.TxT.html_mode = true;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(this.WM.localized_name);
				if ((this.WM.item_revealed & WholeMapItem.REVEALED.MAP) != WholeMapItem.REVEALED.NONE)
				{
					stb.Add("<img mesh=\"itemrow_category.", 35, "\" tx_color />");
				}
				this.TxT.Txt(stb);
			}
			IN.PosP2(this.TxT.transform, 0f, (this.h * 64f - 40f) * 0.5f);
			this.fineTextPos();
			this.fineTopRightText();
		}

		public void fineTextPos()
		{
			for (int i = this.ATx.Count - 1; i >= 0; i--)
			{
				IN.Pos(this.ATx[i].transform, this.w / 2f - 0.71875f, this.h / 2f - (float)(66 - 18 * i) * 0.015625f, -0.04f);
			}
			for (int j = this.ATxWA.Count - 1; j >= 0; j--)
			{
				IN.Pos(this.ATxWA[j].transform, (this.w / 2f - 0.25f) * (float)X.MPF(j == 0), this.h / 2f + 0.03125f, -0.05f);
			}
		}

		public void setWholeMapTarget(ButtonSkinWholeMapArea.PosMemory Mem)
		{
			if (Mem.WM == null)
			{
				return;
			}
			if (this.is_wa)
			{
				this.FocusWM = Mem.WM;
				this.switchWA(true, true, true);
				this.SetCenter(Mem.posx, Mem.posy, false, true);
				return;
			}
			this.setWholeMapTarget(Mem.WM, Mem.posx, Mem.posy);
		}

		public TextRenderer initTxWARenderer(TextRenderer _Tx, bool alignment = false)
		{
			_Tx.html_mode = true;
			_Tx.BorderCol(uint.MaxValue).Col(4283780170U).Size(14f);
			if (alignment)
			{
				_Tx.alignx = ALIGN.CENTER;
				_Tx.aligny = ALIGNY.BOTTOM;
				_Tx.StencilRef(241);
				IN.setZ(_Tx.transform, -0.05f);
			}
			else if (this.B.Container != null)
			{
				_Tx.StencilRef(this.B.Container.stencil_ref);
			}
			return _Tx;
		}

		public bool fast_travel_active
		{
			get
			{
				return this.fast_travel_active_;
			}
			set
			{
				if (this.fast_travel_active_ == value)
				{
					return;
				}
				this.fast_travel_active_ = value;
				this.FastTravelFocused = default(WMIconPosition);
				this.stop_t = 20f;
				this.need_fine_lt_text = true;
				if (this.WM == null)
				{
					return;
				}
				this.topright_t = IN.totalframe;
				this.fineTopRightText();
				for (int i = this.ATx.Count - 1; i >= 0; i--)
				{
					this.ATx[i].alpha = 0f;
				}
				this.fine_flag = (this.redraw_marker = true);
				if (!this.fast_travel_active_)
				{
					this.curspos_x = (float)((int)this.curspos_x) + 0.5f;
					this.curspos_y = (float)((int)this.curspos_y) + 0.5f;
					return;
				}
				if (this.ABenchList == null && this.WM == this.M2D.WM.CurWM)
				{
					this.ABenchList = this.WM.getNoticedIconList(WMIcon.TYPE.BENCH);
				}
			}
		}

		private void fineTopRightText()
		{
			if (this.ATx == null || this.WM == null)
			{
				return;
			}
			for (int i = this.ATx.Count - 1; i >= 0; i--)
			{
				this.ATx[i].enabled = true;
			}
			float num = this.WM.getCompletionRatio();
			this.ATx[0].Txt("<img mesh=\"wmap_foot\" width=\"40\" />" + X.spr_percentage(num, 1, "%"));
			this.ATx[0].BorderCol((num < 1f) ? 4291150012U : 4294965705U);
			num = this.WM.getCompletionTreasureRatio();
			if (num >= 0f)
			{
				this.ATx[1].Txt("<img mesh=\"wmap_treasure\" width=\"40\" />" + X.spr_percentage(num, 1, "%"));
			}
			else
			{
				this.ATx[1].Txt("<img mesh=\"wmap_treasure\" width=\"40\" /> --- ");
			}
			this.ATx[1].BorderCol((num < 1f) ? 4291150012U : 4294965705U);
		}

		public WholeMapItem.WMItem WmiCurPos
		{
			get
			{
				if (this.need_fine_wmi_cur)
				{
					this.WmiCurPos_ = null;
					if (this.is_detail)
					{
						this.need_fine_wmi_cur = false;
						this.WmiCurPos_ = this.WM.getAppeardWmi((int)this.curspos_x, (int)this.curspos_y);
					}
				}
				return this.WmiCurPos_;
			}
		}

		private void fineTopLeftText()
		{
			if (this.ATx == null)
			{
				return;
			}
			this.need_fine_lt_text = false;
			this.AColQuestHeadColor.Clear();
			this.AQuestFocus.Clear();
			if (this.is_detail && this.fast_travel_active_)
			{
				this.TxLT.Col(C32.d2c(4279966491U));
				this.TxLT.Txt(TX.Get("GameMenu_map_fasttravel", ""));
			}
			else if (this.see_quest_depert)
			{
				bool flag = true;
				this.TxLT.Col(4280557596U);
				if (this.is_detail)
				{
					if (this.AQuestDep != null && this.AQuestDep.Count != 0)
					{
						flag = false;
						WholeMapItem.WMItem wmiCurPos = this.WmiCurPos;
						for (int i = this.AQuestDep.Count - 1; i >= 0; i--)
						{
							QuestTracker.QuestDepertureOnMap questDepertureOnMap = this.AQuestDep[i];
							if ((wmiCurPos != null) ? wmiCurPos.Rc.isin(questDepertureOnMap.x, questDepertureOnMap.y, 0f) : (X.LENGTHXYN(questDepertureOnMap.x, questDepertureOnMap.y * 0.5f, this.curspos_x, this.curspos_y * 0.5f) < 1.3f))
							{
								if (this.AColQuestHeadColor.Count == 0)
								{
									this.AColQuestHeadColor.AddRange(questDepertureOnMap.ACol);
								}
								else
								{
									QuestTracker.QuestMapInfo.pushIdentical(this.AColQuestHeadColor, questDepertureOnMap.ACol);
								}
								this.AQuestFocus.AddRange(questDepertureOnMap.AProg);
							}
						}
						if (this.AQuestFocus.Count > 1)
						{
							this.AQuestFocus.Sort((QuestTracker.QuestProgress A, QuestTracker.QuestProgress B) => QuestTracker.sortProgInWM(A, B));
							this.TxLT.Txt(TX.GetA("quest_selecting_multi", this.AQuestFocus[0].current_description, this.AQuestFocus.Count.ToString()));
						}
						else if (this.AQuestFocus.Count == 1)
						{
							this.TxLT.Txt(this.AQuestFocus[0].current_description);
						}
						else
						{
							flag = true;
						}
					}
				}
				else
				{
					flag = false;
					if (this.OQuestDepWA != null && this.FocusWM != null)
					{
						foreach (KeyValuePair<string, QuestTracker.QuestDepertureOnWa> keyValuePair in this.OQuestDepWA)
						{
							QuestTracker.QuestDepertureOnWa value = keyValuePair.Value;
							if (value.Record.key == this.FocusWM.text_key)
							{
								int count = value.AProg.Count;
								for (int j = 0; j < count; j++)
								{
									QuestTracker.QuestProgress questProgress = value.AProg[j];
									QuestTracker.QuestMapInfo.pushIdentical(this.AColQuestHeadColor, this.M2D, questProgress);
									this.AQuestFocus.Add(questProgress);
								}
							}
						}
					}
					if (this.AQuestFocus.Count > 1)
					{
						this.AQuestFocus.Sort((QuestTracker.QuestProgress A, QuestTracker.QuestProgress B) => QuestTracker.sortProgInWM(A, B));
						this.TxLT.Txt(TX.GetA("quest_selecting_multi", this.AQuestFocus[0].current_description, this.AQuestFocus.Count.ToString()));
					}
					else if (this.AQuestFocus.Count == 1)
					{
						this.TxLT.Txt(this.AQuestFocus[0].current_description);
					}
					else
					{
						flag = true;
					}
				}
				if (flag)
				{
					QuestTracker.QuestProgress headQuest = this.M2D.QUEST.getHeadQuest();
					if (headQuest == null)
					{
						this.TxLT.text_content = "";
					}
					else
					{
						if (headQuest.CurrentDepert.isActiveMap())
						{
							this.AColQuestHeadColor.Add(new QuestTracker.QuestMapInfo(headQuest, false));
						}
						this.TxLT.Txt(headQuest.current_description);
					}
				}
				else
				{
					for (int k = this.AColQuestHeadColor.Count - 1; k >= 0; k--)
					{
						QuestTracker.QuestMapInfo questMapInfo = this.AColQuestHeadColor[k];
						questMapInfo.semitransp = false;
						this.AColQuestHeadColor[k] = questMapInfo;
					}
				}
			}
			this.TxLT.HeadXShift((this.AColQuestHeadColor.Count > 0) ? 24f : 0f);
		}

		public void SetCenter(float _x, float _y, bool animate = false, bool set_curs = true)
		{
			if (animate)
			{
				if (this.is_wa)
				{
					this.mappos_x = this.curspos_x;
					this.mappos_y = this.curspos_y;
				}
				this.animate_len = X.LENGTHXY(_x, _y, this.mappos_x, this.mappos_y);
				this.animate_agR = X.GAR(_x, -_y, this.mappos_x, -this.mappos_y);
				this.animate_time = IN.totalframe;
			}
			this.mappos_x = _x;
			this.mappos_y = _y;
			if (this.is_wa)
			{
				this.curspos_x = _x;
				this.curspos_y = _y;
				this.need_fine_quest_lt_text = true;
			}
			else if (set_curs)
			{
				this.curspos_x = (float)((int)_x) + 0.5f;
				this.curspos_y = (float)((int)_y) + 0.5f;
				this.FastTravelFocused = default(WMIconPosition);
				this.stop_t = 20f;
				this.need_fine_quest_lt_text = (this.need_fine_wmi_cur = true);
			}
			this.redraw_map = (this.redraw_rect = (this.redraw_marker = (this.redraw_grid = (this.redraw_wa = true))));
			this.fine_flag = true;
		}

		public void clipCursInBounds(ref float _curspos_x, ref float _curspos_y, bool fix_pos = true)
		{
			int num = X.IntC((this.w * 64f - this.inner_margin * 2f) / this.cell_size) / 2 - 2;
			int num2 = X.IntC((this.h * 64f - this.inner_margin * 2f) / this.cell_size) / 2 - 2;
			_curspos_x = X.MMX(this.DrawBounds.x - (float)num + 0.5f, _curspos_x, this.DrawBounds.right + (float)num - 0.5f);
			_curspos_y = X.MMX(this.DrawBounds.y - (float)num2 + 0.5f, _curspos_y, this.DrawBounds.bottom + (float)num2 - 0.5f);
			if (fix_pos && this.is_detail)
			{
				_curspos_x = (float)((int)_curspos_x) + 0.5f;
				_curspos_y = (float)((int)_curspos_y) + 0.5f;
			}
			this.need_fine_quest_lt_text = (this.need_fine_wmi_cur = true);
		}

		public bool smooth_walk
		{
			get
			{
				return this.fast_travel_active_ || this.is_wa;
			}
		}

		public bool walkCurs(float dx, float dy, bool immediate_focus = false)
		{
			bool flag = false;
			if (this.DrawBounds.isEmpty())
			{
				return false;
			}
			bool flag2 = dx == 0f && dy == 0f;
			if (!flag2)
			{
				float num = this.curspos_x + dx;
				float num2 = this.curspos_y + dy;
				this.clipCursInBounds(ref num, ref num2, !this.smooth_walk);
				dx = num - this.curspos_x;
				dy = num2 - this.curspos_y;
			}
			bool flag3 = false;
			if (this.is_wa)
			{
				dx *= 4.5f / this.cell_size;
				dy *= 4.5f / this.cell_size;
			}
			else if (this.smooth_walk)
			{
				dx *= 0.19f;
				dy *= 0.19f;
			}
			else
			{
				flag3 = true;
			}
			if (dx == 0f && dy == 0f)
			{
				if (flag2 && this.fast_travel_active_ && this.ABenchList != null && this.stop_t != 100f)
				{
					if (!immediate_focus)
					{
						float num3 = this.stop_t + 1f;
						this.stop_t = num3;
						if (num3 < 15f)
						{
							goto IL_0105;
						}
					}
					int count = this.ABenchList.Count;
					WMIconPosition wmiconPosition = this.FastTravelFocused;
					if (!wmiconPosition.valid)
					{
						float num4 = 0f;
						for (int i = 0; i < count; i++)
						{
							WMIconPosition wmiconPosition2 = this.ABenchList[i];
							float num5 = X.LENGTHXY2(this.curspos_x, this.curspos_y, wmiconPosition2.wmx, wmiconPosition2.wmy);
							if (num5 <= 4f && (!wmiconPosition.valid || num5 < num4))
							{
								wmiconPosition = wmiconPosition2;
								num4 = num5;
							}
						}
						if (!wmiconPosition.valid)
						{
							this.stop_t = 100f;
							return false;
						}
						flag = true;
					}
					this.FastTravelFocused = wmiconPosition;
					if (this.stop_t == 18f)
					{
						flag3 = true;
					}
					float num6 = X.LENGTHXY2(this.curspos_x, this.curspos_y, wmiconPosition.wmx, wmiconPosition.wmy);
					if (num6 <= 0.0025000002f)
					{
						dx = wmiconPosition.wmx - this.curspos_x;
						dy = wmiconPosition.wmy - this.curspos_y;
						this.stop_t = -200f;
						goto IL_029F;
					}
					float num7 = X.GAR2(this.curspos_x, -this.curspos_y, wmiconPosition.wmx, -wmiconPosition.wmy);
					num6 = 0.08f * Mathf.Sqrt(num6);
					dx = num6 * X.Cos(num7);
					dy = -num6 * X.Sin(num7);
					goto IL_029F;
				}
				IL_0105:
				return this.stop_t == 1f;
			}
			if (!this.fast_travel_active_)
			{
				flag = true;
			}
			this.stop_t = 0f;
			if (this.FastTravelFocused.valid)
			{
				this.FastTravelFocused = default(WMIconPosition);
				flag = true;
			}
			IL_029F:
			this.curspos_x += dx;
			this.curspos_y += dy;
			this.need_fine_quest_lt_text = (this.need_fine_wmi_cur = true);
			this.redraw_wa = true;
			this.clipCenterPosition(this.cell_size, dx < 0f, dy < 0f, dx > 0f, dy > 0f, false, true);
			if (flag3)
			{
				SND.Ui.play("cursor_gear", false);
			}
			return flag;
		}

		private bool clipCenterPosition(float cell_size, bool to_left, bool to_top, bool to_right, bool to_bottom, bool animate = true, bool execute_set_center = true)
		{
			if (this.is_wa)
			{
				this.mappos_x = this.curspos_x;
				this.mappos_y = this.curspos_y;
				return true;
			}
			float num = (float)X.IntC((this.w * 64f - this.inner_margin * 2f) / cell_size / 3.3f);
			float num2 = (float)X.IntC((this.h * 64f - this.inner_margin * 2f) / cell_size / 3.3f);
			bool flag = false;
			if (to_left && this.curspos_x + num < this.mappos_x)
			{
				flag = true;
				this.mappos_x = this.curspos_x + num;
			}
			else if (to_right && this.curspos_x - num > this.mappos_x)
			{
				flag = true;
				this.mappos_x = this.curspos_x - num;
			}
			if (to_top && this.curspos_y + num2 < this.mappos_y)
			{
				flag = true;
				this.mappos_y = this.curspos_y + num2;
			}
			else if (to_bottom && this.curspos_y - num2 > this.mappos_y)
			{
				flag = true;
				this.mappos_y = this.curspos_y - num2;
			}
			if (flag && execute_set_center)
			{
				this.SetCenter(this.mappos_x, this.mappos_y, animate, false);
			}
			return flag;
		}

		public void changeZoomInOut()
		{
			SND.Ui.play((this.t_zoomin < 0) ? "tool_submap" : "tool_eraser", false);
			this.t_zoomin = ((this.t_zoomin < 0) ? 1 : (-1));
			this.clipCenterPosition((this.t_zoomin >= 0) ? this.cell_size : (this.cell_size * 2f), true, true, true, true, true, true);
			this.fine_flag = (this.scale_changed = true);
		}

		public void reveal(WmDeperture Depert, bool no_first_delay = false, bool immediate = false)
		{
			this.t_qreveal = (no_first_delay ? 18f : 0f);
			this.DepertQReveal = Depert;
			if (this.is_detail && this.WM != null)
			{
				if (this.WM.text_key != Depert.wm_key)
				{
					WAManager.WARecord wa = WAManager.GetWa(Depert.wm_key, false);
					WholeMapItem byTextKey = this.M2D.WM.GetByTextKey(Depert.wm_key);
					if (wa != null && !wa.isActivated())
					{
						using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
						{
							if (Depert.getPosCache(byTextKey).getPos(this.M2D, this.WM, blist) && blist.Count > 0)
							{
								this.revealToPos(blist[0]);
								return;
							}
						}
					}
					if (immediate)
					{
						this.setWholeMapTarget(byTextKey, byTextKey.Bounds.cx, byTextKey.Bounds.cy);
						this.t_qreveal = -1f;
						this.DepertQReveal.wm_key = null;
						return;
					}
				}
				if (this.WM.text_key == Depert.wm_key)
				{
					this.DepertQReveal.wm_key = null;
				}
			}
		}

		private void revealToPos(MapPosition Pos)
		{
			this.DepertQReveal.wm_key = null;
			this.SetCenter(Pos.x, Pos.y, true, true);
			SND.Ui.play("cursor_gear_reset", false);
			this.t_qreveal = -1f;
		}

		public List<QuestTracker.QuestProgress> getFocusdQuestArray()
		{
			return this.AQuestFocus;
		}

		public bool hasFocusedQuest()
		{
			return this.AQuestFocus != null && this.AQuestFocus.Count > 0;
		}

		public ButtonSkinWholeMapArea.PosMemory getPositionMemory()
		{
			return new ButtonSkinWholeMapArea.PosMemory(this.WM, this.curspos_x, this.curspos_y, this.is_wa);
		}

		public void dragInit()
		{
			if (this.DragFst.z == 0f && this.B.clickable)
			{
				UiBox.FlgLockFocus.Add("WHOLEMAP");
				Vector2 mousePos = IN.getMousePos(null);
				this.DragFst.Set(mousePos.x, mousePos.y, 1f);
				if (this.is_wa)
				{
					this.PosFst.Set(this.curspos_x, this.curspos_y);
				}
				else
				{
					this.PosFst.Set(this.mappos_x, this.mappos_y);
				}
				SND.Ui.play("tool_drag_init", false);
			}
		}

		public bool run(float fcnt)
		{
			if (this.isDragging())
			{
				if (!IN.isMouseOn() || this.B == null || !this.B.clickable)
				{
					bool flag = false;
					bool flag2 = false;
					if (this.DragFst.z < 13f)
					{
						flag = true;
						Vector3 vector = base.global2local(this.DragFst);
						float num;
						float num2;
						if (this.is_wa)
						{
							num = this.curspos_x + vector.x * 64f / this.cell_size;
							num2 = this.curspos_y - vector.y * 64f / this.cell_size;
							flag2 = X.LENGTHXYS(this.PosFst.x, this.PosFst.y, num, num2) < 30f;
						}
						else
						{
							num = (float)((int)(this.mappos_x + vector.x * 64f / this.cell_size)) + 0.5f;
							num2 = (float)((int)(this.mappos_y - vector.y * 64f / this.cell_size)) + 0.5f;
							flag2 = X.LENGTHXYS(num, num2, this.curspos_x, this.curspos_y) < 0.8f;
						}
						this.clipCursInBounds(ref num, ref num2, true);
						this.SetCenter(num, num2, true, true);
						SND.Ui.play("cursor_gear_reset", false);
					}
					this.DragFst.z = 0f;
					SND.Ui.play("tool_drag_quit", false);
					UiBox.FlgLockFocus.Rem("WHOLEMAP");
					if (this.FnQuitDragging != null)
					{
						this.FnQuitDragging(false, flag, flag2);
					}
				}
				else
				{
					Vector2 mousePos = IN.getMousePos(null);
					this.DragFst.z = this.DragFst.z + 1f;
					Vector2 vector2 = mousePos - this.DragFst;
					float num3 = this.PosFst.x - vector2.x * 64f / this.cell_size;
					float num4 = this.PosFst.y + vector2.y * 64f / this.cell_size;
					this.clipCursInBounds(ref num3, ref num4, false);
					this.SetCenter(num3, num4, false, false);
				}
			}
			if (this.t_qreveal >= 0f && (!this.is_detail || (this.WM != null && this.WM.prepareAllVisitted(false))))
			{
				if (this.t_qreveal < 18f)
				{
					this.t_qreveal += 1f;
				}
				else if (this.DepertQReveal.wm_key != null)
				{
					if (!this.is_wa_fade_animation && !this.is_detail_swithing)
					{
						if (this.is_detail)
						{
							this.switchWA(false, false, true);
						}
						else
						{
							this.t_qreveal += 1f;
							if (this.t_qreveal >= 23f)
							{
								if (this.M2D.WA.isActivated(this.DepertQReveal.wm_key))
								{
									this.FocusWM = this.M2D.WM.GetByTextKey(this.DepertQReveal.wm_key);
									this.switchWA(this.FocusWM != null, true, true);
									this.t_qreveal = 5f;
									this.DepertQReveal.wm_key = null;
								}
								else
								{
									this.FocusWM = null;
									WAManager.WARecord wa = WAManager.GetWa(this.DepertQReveal.wm_key, false);
									if (wa != null)
									{
										Vector2 drawCenter = wa.getDrawCenter();
										this.SetCenter(drawCenter.x, drawCenter.y, true, true);
									}
									this.t_qreveal = -1f;
								}
							}
						}
					}
				}
				else if (this.is_wa)
				{
					this.switchWA(false, true, true);
				}
				else if (!this.is_detail_swithing)
				{
					if (this.DepertQReveal.map_key != null)
					{
						using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
						{
							if (this.DepertQReveal.getPosCache(this.WM).getPos(this.M2D, this.WM, blist))
							{
								this.revealToPos(blist[0]);
							}
							goto IL_0411;
						}
					}
					this.t_qreveal = -1f;
				}
			}
			IL_0411:
			if (this.need_fine_lt_text)
			{
				this.fineTopLeftText();
			}
			if (X.D)
			{
				if (this.t_detail >= 0f)
				{
					if (this.t_detail < 30f)
					{
						this.t_detail += (float)X.AF;
						this.fine_all = (this.redraw_frame = true);
						this.fineTextAlpha();
					}
					else if (this.t_zoomin >= 0)
					{
						if (this.t_zoomin <= 30)
						{
							this.t_zoomin += X.AF;
							this.cell_size = X.NIL(40f, 20f, X.ZSIN2((float)this.t_zoomin, 30f), 1f);
							this.fine_all = true;
						}
					}
					else if (this.t_zoomin > -30)
					{
						this.t_zoomin -= X.AF;
						this.cell_size = X.NIL(20f, 40f, X.ZSIN2((float)(-(float)this.t_zoomin), 30f), 1f);
						this.fine_all = true;
					}
					if (this.topright_t != -1)
					{
						float num5 = X.ZSIN((float)(IN.totalframe - this.topright_t), 22f);
						for (int i = this.ATx.Count - 1; i >= 0; i--)
						{
							this.ATx[i].Alpha(num5 * this.alpha);
						}
						this.TxLT.Alpha(num5 * this.alpha * (float)(this.show_topleft_text ? 0 : 1));
						if (num5 >= 1f)
						{
							this.topright_t = -1;
						}
					}
				}
				else
				{
					if (this.t_detail > -30f)
					{
						this.t_detail -= (float)X.AF;
						this.fine_all = (this.redraw_frame = true);
						this.fineTextAlpha();
					}
					else
					{
						if (this.t_wa_fading >= 0f)
						{
							if (this.t_wa_fading >= 1000f)
							{
								this.t_wa_fading -= 1000f;
								if (!this.is_zoomin)
								{
									this.changeZoomInOut();
								}
							}
							this.redraw_wa = true;
							bool flag3 = this.t_wa_fading >= 10f;
							this.t_wa_fading += (float)X.AF;
							if (!flag3 && this.t_wa_fading >= 10f)
							{
								SND.Ui.play("pencil_running", false);
							}
						}
						if (this.t_zoomin >= 0)
						{
							if (this.t_zoomin <= 30)
							{
								this.t_zoomin += X.AF;
								this.cell_size = X.NIL(1f, 0.6f, X.ZSIN2((float)this.t_zoomin, 30f), 1f);
								this.fine_all = true;
							}
						}
						else if (this.t_zoomin > -30)
						{
							this.t_zoomin -= X.AF;
							this.cell_size = X.NIL(0.6f, 1f, X.ZSIN2((float)(-(float)this.t_zoomin), 30f), 1f);
							this.fine_all = true;
						}
					}
					if (this.t_wa_fading < -1f)
					{
						this.t_wa_fading = X.Mn(this.t_wa_fading + (float)X.AF, -1f);
					}
				}
			}
			return true;
		}

		public bool fine_all
		{
			set
			{
				if (value)
				{
					this.redraw_map = (this.redraw_marker = (this.redraw_rect = (this.redraw_grid = (this.redraw_wa = (this.fine_flag = true)))));
				}
			}
		}

		public bool fine_wm_icons
		{
			set
			{
				if (value)
				{
					this.redraw_map = (this.redraw_marker = (this.fine_flag = true));
				}
			}
		}

		public bool hasWAFocus()
		{
			return this.FocusWM != null;
		}

		public void quitDragging()
		{
			try
			{
				if (this.DragFst.z > 0f)
				{
					UiBox.FlgLockFocus.Rem("WHOLEMAP");
					this.DragFst.z = 0f;
					if (this.FnQuitDragging != null)
					{
						this.FnQuitDragging(true, false, false);
					}
				}
			}
			catch
			{
			}
		}

		public bool isDragging()
		{
			return this.DragFst.z > 0f;
		}

		public bool can_handle
		{
			get
			{
				if (this.is_wa)
				{
					if (this.is_wa_fade_animation)
					{
						return false;
					}
				}
				else if (this.WM != null && !this.WM.prepareAllVisitted(true))
				{
					return false;
				}
				return this.t_qreveal < 0f;
			}
		}

		public bool is_cancelable
		{
			get
			{
				return this.can_handle && this.t_qreveal < 0f;
			}
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float w = this.w;
			float h = this.h;
			bool flag = false;
			bool flag2 = false;
			float num = 0f;
			float num2 = 0f;
			if (this.animate_time > 0)
			{
				int num3 = IN.totalframe - this.animate_time;
				if (num3 >= 30)
				{
					this.animate_time = 0;
				}
				else
				{
					flag = true;
					float num4 = X.ZPOW3((float)(30 - num3), 30f);
					this.redraw_map = (this.redraw_rect = (this.redraw_marker = (this.redraw_grid = (this.redraw_wa = true))));
					num += num4 * this.animate_len * X.Cos(this.animate_agR);
					num2 -= num4 * this.animate_len * X.Sin(this.animate_agR);
				}
			}
			if (this.redraw_grid)
			{
				this.MdGrid.clear(false, false);
			}
			if (this.t_detail >= 0f)
			{
				float num5 = X.ZLINE(this.t_detail, 30f);
				float num6 = num;
				float num7 = num2;
				if (this.t_detail < 30f)
				{
					num6 *= 0.125f;
					num7 *= 0.125f;
				}
				this.drawWM(this.mappos_x + num6, this.mappos_y + num7, this.cell_size * (0.125f + 0.875f * num5), this.alpha_ * num5, num5, ref flag, ref flag2);
			}
			else if (this.t_detail > -30f)
			{
				float num8 = X.ZLINE(30f + this.t_detail, 30f);
				this.drawWM(this.dtanim_sx, this.dtanim_sy, this.dtanim_cellsize * (0.125f + 0.875f * num8), this.alpha_ * num8, num8, ref flag, ref flag2);
			}
			else
			{
				this.MdIco.clear(false, false);
				this.marker_ico_ver_i = (this.marker_ico_tri_i = 0);
			}
			if (this.t_detail < 0f)
			{
				float num9 = X.ZLINE(-this.t_detail, 30f);
				this.drawWA(this.curspos_x + num, this.curspos_y + num2, this.cell_size * (2.5f - 1.5f * num9), this.alpha_ * num9, ref flag, ref flag2);
			}
			else if (this.t_detail < 30f)
			{
				float num10 = X.ZLINE(30f - this.t_detail, 30f);
				this.drawWA(this.dtanim_sx + num, this.dtanim_sy + num2, this.dtanim_cellsize * (2.5f - 1.5f * num10), this.alpha_ * num10, ref flag, ref flag2);
			}
			if (flag2)
			{
				this.MdMapIco.updateForMeshRenderer(true);
			}
			if (this.TxLT != null && this.show_topleft_text)
			{
				int count = this.AColQuestHeadColor.Count;
				Vector3 vector = this.TxLT.transform.localPosition * 64f;
				vector.x += 8f;
				vector.y -= 18f;
				if (this.AColQuestHeadColor.Count > 0)
				{
					if (this.AQuestFocus.Count == 0)
					{
						QuestTracker.QuestMapInfo questMapInfo = this.AColQuestHeadColor[0];
						this.MdIco.Col = questMapInfo.C;
						NEL.drawQuestDepertPin(this.MdIco, vector.x, vector.y, this.alpha_ * this.icon_base_alpha, 1f, 0f, questMapInfo.tracking ? 0.3f : 0f);
						if (questMapInfo.tracking)
						{
							flag = true;
						}
					}
					else
					{
						NEL.drawQuestDepertPinAnimSwitching(this.MdIco, IN.totalframe, this.AColQuestHeadColor, vector.x, vector.y - 4f, this.alpha_ * this.icon_base_alpha, 1f, 0f, 0.4f, 0.3f, false);
						flag = true;
					}
				}
			}
			this.MdIco.updateForMeshRenderer(true);
			if (this.redraw_frame)
			{
				this.redraw_frame = false;
				this.MdFrame.updateForMeshRenderer(false);
			}
			if (this.redraw_grid)
			{
				this.redraw_grid = false;
				this.MdGrid.updateForMeshRenderer(false);
			}
			base.Fine();
			if (flag)
			{
				this.fine_flag = true;
			}
			return this;
		}

		public Vector2 getCursorMapPos()
		{
			return new Vector2(this.curspos_x, this.curspos_y);
		}

		public Vector2 Map2Upos(Vector2 MapPos, bool fined_cell_size = false)
		{
			MapPos.Set(MapPos.x - this.mappos_x, MapPos.y - this.mappos_y);
			float num = (fined_cell_size ? (this.is_zoomin ? 40f : 20f) : this.cell_size);
			MapPos *= num * 0.015625f;
			MapPos.y *= -1f;
			return MapPos;
		}

		public bool is_zoomin
		{
			get
			{
				return this.t_zoomin < 0;
			}
		}

		public WholeMapItem getWholeMapTarget()
		{
			return this.WM;
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha != value)
				{
					base.alpha = value;
					this.fine_all = (this.redraw_frame = true);
					if (this.MaMapFill != null)
					{
						this.MaMapFill.setAlpha1(this.alpha_, false).updateForMeshRenderer(true);
						this.MaMapLine.setAlpha1(this.alpha_, false).updateForMeshRenderer(true);
					}
					if (this.MaMapIco != null)
					{
						this.MaMapIco.setAlpha1(this.alpha_, false).updateForMeshRenderer(true);
					}
					this.fineTextAlpha();
				}
			}
		}

		private void fineTextAlpha()
		{
			if (this.ATx != null)
			{
				float num = this.alpha_ * ((this.t_detail >= 0f) ? X.ZLINE(this.t_detail, 30f) : X.ZLINE(30f + this.t_detail, 30f));
				for (int i = this.ATx.Count - 1; i >= 0; i--)
				{
					this.ATx[i].Alpha(num);
				}
				this.TxT.Alpha(num);
				this.ATxWA[0].Alpha(num);
			}
			this.TxLT.Alpha(this.alpha_ * (float)(this.show_topleft_text ? 1 : 0));
			if (this.ATxWA != null)
			{
				float num2 = this.alpha_ * ((this.t_detail >= 0f) ? X.ZLINE(30f - this.t_detail, 30f) : X.ZLINE(-this.t_detail, 30f));
				for (int j = this.ATxWA.Count - 1; j >= 1; j--)
				{
					this.ATxWA[j].Alpha(num2);
				}
			}
		}

		public void topRightTxVisible(bool visible)
		{
			if (this.ATx != null)
			{
				for (int i = this.ATx.Count - 1; i >= 0; i--)
				{
					this.ATx[i].gameObject.SetActive(visible);
				}
			}
		}

		public void setWaSwitchKDVisible(bool visible)
		{
			if (this.ATxWA != null)
			{
				this.ATxWA[0].gameObject.SetActive(visible);
				this.ATxWA[1].gameObject.SetActive(visible);
			}
		}

		private void drawWM(float mappos_x, float mappos_y, float cell_size, float alpha_, float tz_detail, ref bool re_fine, ref bool update_mdmapico)
		{
			float num = this.w * 64f - 30f;
			float num2 = this.h * 64f - 40f;
			if (this.redraw_frame && this.TxT != null)
			{
				this.MdFrame.Col = C32.MulA(4283780170U, alpha_);
				NEL.CuteFrame(this.MdFrame, (1f - tz_detail) * 60f, 0f, num, num2, this.TxT.get_swidth_px() + 40f);
			}
			float num3 = num - this.inner_margin * 2f;
			float num4 = num2 - this.inner_margin * 2f;
			float num5 = num3 / 2f;
			float num6 = num4 / 2f;
			if (this.redraw_grid)
			{
				this.MdGrid.Col = C32.MulA(2004976000U, alpha_);
				float num7 = X.NI(this.w * 64f, num3, tz_detail);
				float num8 = X.NI(this.h * 64f, num4, tz_detail);
				this.MdGrid.Rect(0f, 0f, num7, num8, false);
				if (this.TxLT != null)
				{
					IN.PosP2(this.TxLT.transform, -X.NI(num7, num3, 0.5f) / 2f + 12f, X.NI(num8, num4, 0.5f) / 2f - 14f);
				}
			}
			if (this.WM != null && this.WM.prepareAllVisitted(false))
			{
				bool flag = false;
				if (this.redraw_map)
				{
					this.WM.drawTo(this.MdMapFill.clear(false, false), this.MdMapLine.clear(false, false), this.MdMapMask.clear(false, false), this.MdMapIco.clear(false, false), mappos_x, mappos_y, num, num2, cell_size, alpha_, alpha_ * this.icon_base_alpha, this.DrawBounds.isEmpty() ? this.DrawBounds : null);
					this.mapcontent_ver_i = this.MdMapFill.getVertexMax();
					this.mapcontent_tri_i = this.MdMapFill.getTriMax();
					if (this.redraw_marker)
					{
						this.redraw_marker = this.WM.drawMarker(this.MdIco.clear(false, false), this.MdMapFill, mappos_x, mappos_y, num, num2, cell_size, alpha_);
						this.marker_ico_ver_i = this.MdIco.getVertexMax();
						this.marker_ico_tri_i = this.MdIco.getTriMax();
					}
					else
					{
						this.MdIco.revertVerAndTriIndex(this.marker_ico_ver_i, this.marker_ico_tri_i, false);
					}
					if (this.MaMapFill == null)
					{
						this.MaMapFill = new MdArranger(this.MdMapFill);
						this.MaMapLine = new MdArranger(this.MdMapLine);
						this.MaMapIco = new MdArranger(this.MdMapIco);
					}
					this.MaMapFill.SetWhole(true);
					this.MaMapLine.SetWhole(true);
					this.MaMapIco.SetWhole(true);
					if (alpha_ < 1f)
					{
						this.MaMapFill.setAlpha1(alpha_, false);
						this.MaMapLine.setAlpha1(alpha_, false);
						this.MaMapIco.setAlpha1(alpha_, false);
					}
					flag = true;
					this.MdMapLine.updateForMeshRenderer(true);
					this.MdMapMask.updateForMeshRenderer(true);
					update_mdmapico = true;
				}
				else if (this.redraw_marker)
				{
					this.MdMapFill.revertVerAndTriIndex(this.mapcontent_ver_i, this.mapcontent_tri_i, false);
					this.redraw_marker = this.WM.drawMarker(this.MdIco.clear(false, false), this.MdMapFill, mappos_x, mappos_y, num, num2, cell_size, alpha_);
					this.marker_ico_ver_i = this.MdIco.getVertexMax();
					this.marker_ico_tri_i = this.MdIco.getTriMax();
					if (this.MaMapFill != null)
					{
						this.MaMapFill.SetWhole(true);
					}
					flag = true;
					re_fine = this.redraw_marker | re_fine;
				}
				else
				{
					this.MdIco.revertVerAndTriIndex(this.marker_ico_ver_i, this.marker_ico_tri_i, false);
				}
				if (this.ABenchList != null && this.fast_travel_active_)
				{
					if (!flag)
					{
						this.MdMapFill.revertVerAndTriIndex(this.mapcontent_ver_i, this.mapcontent_tri_i, false);
						flag = true;
					}
					float num9 = X.ANMPT(100, 1f) * 1.7f;
					if (num9 < 1f)
					{
						num9 = X.ZSIN(num9);
						Color32 c = this.MdMapFill.ColGrd.Set(2717908991U).mulA(1f - num9).C;
						this.MdMapFill.ColGrd.setA(0f);
						this.MdMapFill.Identity();
						int count = this.ABenchList.Count;
						float num10 = cell_size * (0.5f + 0.5f * num9) * 2.8f;
						float num11 = (float)X.IntC(num / cell_size);
						float num12 = (float)X.IntC(num2 / cell_size);
						float num13 = mappos_x - num11 / 2f;
						float num14 = mappos_x + num11 / 2f;
						float num15 = mappos_y - num12 / 2f;
						float num16 = mappos_y + num12 / 2f;
						for (int i = 0; i < count; i++)
						{
							WMIconPosition wmiconPosition = this.ABenchList[i];
							if (X.BTW(num13 - 3f, wmiconPosition.wmx, num14 + 3f) && X.BTW(num15 - 3f, wmiconPosition.wmy, num16 + 3f))
							{
								float num17 = this.WM.map2meshx(wmiconPosition.wmx, mappos_x, cell_size);
								float num18 = this.WM.map2meshy(wmiconPosition.wmy, mappos_y, cell_size);
								this.MdMapFill.Col = c;
								this.MdMapFill.BlurPoly2(num17, num18, num10, 0f, 12, 0f, num10 * 0.3f, null, this.MdMapFill.ColGrd);
							}
						}
					}
				}
				if (this.AQuestDep != null)
				{
					this.M2D.QUEST.getHeadQuest();
					int count2 = this.AQuestDep.Count;
					for (int j = 0; j < count2; j++)
					{
						QuestTracker.QuestDepertureOnMap questDepertureOnMap = this.AQuestDep[j];
						float num19 = this.WM.map2meshx(questDepertureOnMap.x, mappos_x, cell_size);
						float num20 = this.WM.map2meshy(questDepertureOnMap.y, mappos_y, cell_size);
						NEL.drawQuestDepertPinAnimSwitching(this.MdIco, IN.totalframe, questDepertureOnMap.ACol, num19, num20, alpha_ * this.icon_base_alpha, 1f, 0.5f, 1f, 1f, this.fast_travel_active_);
					}
				}
				float num21 = X.Abs(X.COSIT(140f));
				if (this.WM == this.M2D.WM.CurWM)
				{
					float num22 = (0.2f + 0.8f * num21) * alpha_;
					PR pr = UIPicture.getPr();
					if (pr != null)
					{
						if (this.pr_pos_x == -1000f)
						{
							this.WM.fixPlayerPosOnWM(pr, ref this.pr_pos_x, ref this.pr_pos_y);
						}
						if (this.pr_pos_x != -1000f)
						{
							this.MdIco.Col = ButtonSkinWholeMapArea.Col.White().setA1(num22).C;
							this.MdIco.RotaPF(this.WM.map2meshx(this.pr_pos_x, mappos_x, cell_size), this.WM.map2meshy(this.pr_pos_y, mappos_y, cell_size), 1f, 1f, 0f, MTRX.getPF(pr.is_alive ? "IconNoel0" : "IconNoel1"), false, false, false, uint.MaxValue, false, 0);
						}
					}
				}
				this.FD_FnDrawIcon(this, this.MdIco, (0.4f + 0.6f * (1f - num21)) * alpha_, mappos_x, mappos_y, cell_size);
				if (base.isChecked())
				{
					float num23 = cell_size;
					NEL.POINT_CURS point_CURS = NEL.POINT_CURS.LTRB;
					if (this.fast_travel_active_)
					{
						point_CURS = NEL.POINT_CURS.SUN_DISABLE;
						if (this.FastTravelFocused.valid)
						{
							point_CURS = NEL.POINT_CURS.SUN;
							num23 *= 1.25f;
						}
					}
					NEL.drawPointCurs(this.MdIco, this.WM.map2meshx(this.curspos_x, mappos_x, cell_size), this.WM.map2meshy(this.curspos_y, mappos_y, cell_size), num23, alpha_, point_CURS);
				}
				if (flag)
				{
					this.MdMapFill.updateForMeshRenderer(true);
				}
				this.redraw_map = (this.redraw_rect = false);
			}
			else
			{
				if (this.MaMapFill != null)
				{
					this.MaMapFill.clear(this.MdMapFill.clear(false, false));
					this.MaMapLine.clear(this.MdMapLine.clear(false, false));
					this.MdMapMask.clear(false, false);
					this.MaMapIco.clear(this.MdMapIco);
				}
				this.MdMapIco.clear(false, false);
				this.MdMapIco.Col = C32.MulA(4284240206U, alpha_);
				this.MdMapIco.RotaPF(0f, 0f, true, 2f, 2f, 0f, MTRX.SqLoadingS.getFrame(X.ANM(IN.totalframe, MTRX.SqLoadingS.countFrames(), 4f)), false, true, false, uint.MaxValue, 0);
				update_mdmapico = true;
				this.redraw_map = true;
				this.redraw_rect = true;
				this.redraw_marker = true;
				re_fine = true;
			}
			if (this.redraw_grid && this.WM != null && cell_size > 5f)
			{
				float num24 = alpha_ * X.ZLINE(cell_size - 5f, -4f);
				float num25 = (float)X.IntR(this.WM.map2meshx((float)((int)mappos_x), mappos_x, cell_size));
				float num26 = (float)X.IntR(this.WM.map2meshy((float)((int)mappos_y), mappos_y, cell_size));
				int num27 = X.IntC(num3 / cell_size) + 4;
				int num28 = num27 / 2;
				int num29 = X.IntC(num4 / cell_size) + 4;
				int num30 = num29 / 2;
				this.MdGrid.Col = C32.MulA(4284240206U, num24 * 0.4f);
				float num31 = num25 - (float)num28 * cell_size;
				float num32 = num26 - (float)num30 * cell_size;
				for (int k = 0; k < num29; k++)
				{
					if (X.BTW(-num6, num32, num6))
					{
						this.MdGrid.Line(-num5, num32, num5, num32, 1f, false, 0f, 0f);
					}
					num32 += cell_size;
				}
				num32 = -num26 - (float)num30 * cell_size;
				for (int l = 0; l < num27; l++)
				{
					if (X.BTW(-num5, num31, num5))
					{
						this.MdGrid.Line(num31, -num6, num31, num6, 1f, false, 0f, 0f);
					}
					num31 += cell_size;
				}
			}
		}

		private void drawWA(float mappos_x, float mappos_y, float cell_size, float alpha_, ref bool re_fine, ref bool update_mdmapico)
		{
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			float num3 = num - this.inner_margin * 2f;
			float num4 = num2 - this.inner_margin * 2f;
			bool flag = this.t_detail <= -30f;
			if (this.redraw_grid && flag)
			{
				this.MdGrid.Col = MTRX.ColTrnsp;
				this.MdGrid.Rect(0f, 0f, num, num2, false);
				if (this.TxLT != null)
				{
					IN.PosP2(this.TxLT.transform, -X.NI(num, num3, 0.5f) / 2f + 12f, X.NI(num2, num4, 0.5f) / 2f - 14f);
				}
			}
			if (this.MaMapIco == null)
			{
				this.MaMapIco = new MdArranger(this.MdMapIco);
			}
			if (this.redraw_wa)
			{
				if (flag)
				{
					this.MaMapIco.clear(this.MdMapIco.clear(false, false));
				}
				this.MaMapIco.Set(true);
				WholeMapItem wholeMapItem = null;
				int num5 = WAManager.drawWA(this.MdMapWA.clear(false, false), this.MdMapWA_Fading.clear(false, false), this.MdMapIco, mappos_x, mappos_y, num, num2, cell_size, alpha_, X.ZLINE((this.t_wa_fading >= 0f) ? (this.t_wa_fading % 1000f - 10f) : 0f, 54f), this.ATxWA, 2, ref wholeMapItem, out this.redraw_wa);
				if (this.FocusWM != wholeMapItem)
				{
					this.need_fine_lt_text = true;
					this.FocusWM = wholeMapItem;
				}
				if (num5 > 0)
				{
					if (this.t_wa_fading < 0f)
					{
						this.t_wa_fading = (float)((num5 == 2) ? 1000 : 0);
					}
				}
				else if (this.t_wa_fading >= 0f)
				{
					this.t_wa_fading = -1f;
				}
				this.MaMapIco.Set(false);
				this.MdMapWA.updateForMeshRenderer(false);
				this.MdMapWA_Fading.updateForMeshRenderer(false);
			}
			WAManager.fineMaIconAlpha(this.MdMapIco, this.MaMapIco, alpha_);
			if (this.see_quest_depert && this.OQuestDepWA != null)
			{
				this.MaMapIco.revertVerAndTriIndexSaved(false);
				foreach (KeyValuePair<string, QuestTracker.QuestDepertureOnWa> keyValuePair in this.OQuestDepWA)
				{
					QuestTracker.QuestDepertureOnWa value = keyValuePair.Value;
					NEL.drawQuestDepertPinAnimSwitching(this.MdIco, IN.totalframe, value.ACol, (value.x - mappos_x) * cell_size, -(value.y - mappos_y) * cell_size, alpha_, 1f, 0.5f, 1f, 1f, false);
				}
			}
			update_mdmapico = true;
		}

		public float icon_base_alpha = 1f;

		public bool show_topleft_text = true;

		public float inner_margin = 20f;

		protected MeshDrawer MdGrid;

		protected MeshDrawer MdMapFill;

		protected MeshDrawer MdMapLine;

		protected MeshDrawer MdMapMask;

		protected MeshDrawer MdMapIco;

		protected MeshDrawer MdMapWA;

		protected MeshDrawer MdMapWA_Fading;

		protected MdArranger MaMapFill;

		protected MdArranger MaMapLine;

		protected MdArranger MaMapIco;

		protected MeshDrawer MdIco;

		protected MeshDrawer MdFrame;

		public static C32 Col;

		private NelM2DBase M2D;

		private bool redraw_frame;

		private bool redraw_map = true;

		private bool redraw_wa = true;

		private bool redraw_grid = true;

		private bool redraw_rect = true;

		public bool redraw_marker = true;

		private float cell_size = 20f;

		public const float CELL_SIZE_DEF = 20f;

		public const float CELL_SIZE_LARGE = 40f;

		private const float CELL_SIZE_WA_DEF = 0.6f;

		private const float CELL_SIZE_WA_SCALED = 1f;

		public const float T_QRV_FIRST_DELAY = 18f;

		private float mappos_x;

		private float mappos_y;

		private float curspos_x;

		private float curspos_y;

		public bool scale_changed;

		private WholeMapItem.WMItem WmiCurPos_;

		private bool need_fine_lt_text;

		private bool need_fine_wmi_cur;

		private float pr_pos_x = -1000f;

		private float pr_pos_y = -1f;

		private const uint grid_color = 4284240206U;

		private const uint text_border_col = 4291150012U;

		public const float BOX_SIZE_W = 540f;

		public const float BOX_SIZE_H = 480f;

		private DRect DrawBounds;

		private WholeMapItem FocusWM;

		private float animate_agR;

		private int animate_time;

		private float animate_len;

		public const int ANIMATE_MAXT = 30;

		private const float height_margin = 40f;

		private int mapcontent_ver_i;

		private int mapcontent_tri_i;

		private int marker_ico_ver_i;

		private int marker_ico_tri_i;

		private List<TextRenderer> ATx;

		private List<TextRenderer> ATxWA;

		private const int TXWA_RESERVED = 2;

		private TextRenderer TxT;

		private TextRenderer TxLT;

		private WholeMapItem WM;

		private int topright_t = -1;

		public const int TOPRIGHT_TX_MAXT = 22;

		private bool fast_travel_active_;

		public bool see_quest_depert = true;

		private float stop_t;

		private List<WMIconPosition> ABenchList;

		public ButtonSkinWholeMapArea.FnDrawIcon FD_FnDrawIcon = delegate(ButtonSkinWholeMapArea _WmSkin, MeshDrawer _Md, float _blink_alpha, float mappos_x, float mappos_y, float cell_size)
		{
		};

		private List<QuestTracker.QuestDepertureOnMap> AQuestDep;

		private BDic<string, QuestTracker.QuestDepertureOnWa> OQuestDepWA;

		public WMIconPosition FastTravelFocused;

		private List<DRect> AWaAppeared;

		public Action<bool, bool, bool> FnQuitDragging;

		public const int ZOOM_MAXT = 30;

		private int t_zoomin;

		public const int DETAIL_MAXT = 30;

		private float t_detail = 30f;

		private float dtanim_sx;

		private float dtanim_sy;

		private float dtanim_cellsize;

		private float t_wa_fading = -1f;

		private WmDeperture DepertQReveal;

		private float t_qreveal = -1f;

		private List<QuestTracker.QuestMapInfo> AColQuestHeadColor = new List<QuestTracker.QuestMapInfo>();

		private List<QuestTracker.QuestProgress> AQuestFocus = new List<QuestTracker.QuestProgress>();

		private const float quest_head_color_margin = 14f;

		public const float WAFADE_MAXT = 64f;

		private Vector3 DragFst;

		private Vector2 PosFst;

		public delegate void FnDrawIcon(ButtonSkinWholeMapArea WmSkin, MeshDrawer MdIco, float blink_alpha, float mappos_x, float mappos_y, float cell_size);

		public delegate void FnSwitchWA(bool to_wa);

		public struct PosMemory
		{
			public PosMemory(WholeMapItem _WM, float _posx, float _posy, bool _is_wa)
			{
				this.WM = _WM;
				this.posx = _posx;
				this.posy = _posy;
				this.is_wa = _is_wa;
			}

			public float posx;

			public float posy;

			public WholeMapItem WM;

			public bool is_wa;
		}
	}
}
