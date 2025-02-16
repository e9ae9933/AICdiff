using System;
using System.Collections.Generic;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class UiQuestTrackerRow : IRunAndDestroy, INelMSG
	{
		public UiQuestTrackerRow(UiQuestTracker _Con, int index)
		{
			this.Con = _Con;
			this.Gob = IN.CreateGob(this.Con.Gob, "-QTRow-" + index.ToString());
			GameObject gameObject = IN.CreateGob(this.Gob, "-MdT");
			IN.setZ(gameObject.transform, -0.125f);
			this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.Mrd = this.Gob.GetComponent<MeshRenderer>();
			this.MdT = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.MrdT = gameObject.GetComponent<MeshRenderer>();
			this.Valot = this.Gob.AddComponent<ValotileRenderer>();
			this.Valot.InitUI(this.Md, this.Mrd);
			this.Valot.enabled = false;
			this.ValotT = gameObject.AddComponent<ValotileRenderer>();
			this.ValotT.InitUI(this.MdT, this.MrdT);
			this.ValotT.enabled = false;
			this.Ma = new MdArranger(this.Md);
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(this.Mrd);
			this.Tx = IN.CreateGob(this.Mrd.gameObject, "-Tx").AddComponent<NelEvTextRenderer>();
			this.TxB = IN.CreateGob(this.Mrd.gameObject, "-TxB").AddComponent<TextRenderer>();
			this.Tx.html_mode = (this.TxB.html_mode = true);
			this.Tx.max_swidth_px = UiQuestTrackerRow.rw - 60f;
			this.Tx.aligny = ALIGNY.MIDDLE;
			this.TxB.max_swidth_px = UiQuestTrackerRow.rw - 40f - 68f;
			this.TxB.alignx = ALIGN.CENTER;
			this.Tx.auto_wrap = (this.TxB.auto_wrap = true);
			this.Tx.auto_condense = (this.TxB.auto_condense = true);
			IN.setZ(this.Tx.transform, -0.05f);
			IN.setZ(this.TxB.transform, -0.05f);
			this.Tx.MakeValot(null, null);
			this.TxB.MakeValot(null, null);
			this.Tx.Aline_ver_pos = new List<int>(4);
			this.Tx.initNelMsg(this);
			this.Tx.Size(16f).Col(MTRX.ColWhite).BorderCol(MTRX.ColBlack);
			this.TxB.Col(MTRX.ColWhite).BorderCol(MTRX.ColBlack);
			if (UiQuestTrackerRow.Uni == null)
			{
				UiQuestTrackerRow.Uni = new UniBrightDrawer();
				UiQuestTrackerRow.Uni.count = 5;
				UiQuestTrackerRow.Uni.min_r = 0.859375f;
				UiQuestTrackerRow.Uni.max_r = 1.09375f;
			}
			this.use_valotile = true;
		}

		public void createCollectItemField()
		{
			if (this.TxClc != null)
			{
				return;
			}
			this.TxClc = IN.CreateGob(this.Mrd.gameObject, "-TxClc").AddComponent<TextRenderer>();
			this.TxClcCnt = IN.CreateGob(this.Mrd.gameObject, "-TxClcCnt").AddComponent<TextRenderer>();
			this.TxClc.auto_condense = true;
			this.TxClc.line_spacing = (this.TxClcCnt.line_spacing = 0.9f);
			this.TxClc.Size(13f).Col(MTRX.ColWhite).Align(ALIGN.LEFT)
				.BorderCol(MTRX.ColBlack);
			this.TxClc.max_swidth_px = UiQuestTrackerRow.rw - 76f;
			this.TxClcCnt.Size(13f).Col(MTRX.ColWhite).Align(ALIGN.RIGHT)
				.BorderCol(MTRX.ColBlack);
			this.TxClc.MakeValot(null, null);
			this.TxClcCnt.MakeValot(null, null);
			this.TxClc.use_valotile = (this.TxClcCnt.use_valotile = this.Tx.use_valotile);
			IN.setZ(this.TxClc.transform, -0.05f);
			IN.setZ(this.TxClcCnt.transform, -0.05f);
		}

		public void fineFonts()
		{
			if (this.Tx != null)
			{
				this.Tx.TargetFont = (this.TxB.TargetFont = TX.getDefaultFont());
			}
			if (this.TxClc != null)
			{
				this.TxClc.TargetFont = (this.TxClcCnt.TargetFont = TX.getDefaultFont());
			}
		}

		public void initialize(ref UiQuestTracker.TrackTask Trk)
		{
			this.Md.clear(false, false);
			this.MdT.clear(false, false);
			this.enabled = true;
			Trk.TargetRow = this;
			this.Prog = Trk.Prog;
			this.Task = Trk;
			this.FirstDepert = this.Prog.getDepert(Trk.sphase);
			this.t = (this.t_state = 0f);
			this.state = UiQuestTrackerRow.STATE.PREPARING;
			this.Tx.text_content = "";
			this.tx_y_level = 0f;
			this.base_text_pos = 0f;
			this.Tx.alpha = 1f;
			this.TxB.text_content = "";
			if (this.TxClc != null)
			{
				this.TxClc.text_content = "";
				this.TxClc.alpha = 0f;
				this.TxClcCnt.text_content = "";
				this.TxClcCnt.alpha = 0f;
			}
			this.TxB.alpha = 0f;
			FontStorage fontStorage = MTRX.OFontStorage[TX.getDefaultFont()];
			this.TxB.Size(fontStorage.defaultRendererSize * (X.ENG_MODE ? 0.8f : 1f));
			this.need_fine_text_pos = true;
			this.sound_bits = 0U;
			this.reserveText(this.Task.sphase, this.Task.type != QuestTracker.INVISIBLE.START, this.Task.type == QuestTracker.INVISIBLE.START);
			float num = 0f;
			this.fineRectHeight(ref num, true);
			this.need_fine_pos = true;
		}

		public bool enabled
		{
			get
			{
				return this.Gob.activeSelf;
			}
			set
			{
				this.Gob.SetActive(value);
				this.Valot.enabled = (this.ValotT.enabled = value && this.Con.use_valotile);
				this.Tx.use_valotile = value && this.Con.use_valotile;
				this.TxB.use_valotile = value && this.Con.use_valotile;
				if (this.TxClc != null)
				{
					this.TxClc.use_valotile = value && this.Con.use_valotile;
					this.TxClcCnt.use_valotile = value && this.Con.use_valotile;
				}
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.Valot.enabled;
			}
			set
			{
				Behaviour valot = this.Valot;
				this.ValotT.enabled = value;
				valot.enabled = value;
				this.Tx.use_valotile = value;
				this.TxB.use_valotile = value;
				if (this.TxClc != null)
				{
					this.TxClc.use_valotile = value;
					this.TxClcCnt.use_valotile = value;
				}
			}
		}

		public void fineRectHeight(ref float max_y_move, bool immediate = false)
		{
			if (!immediate && this.dep_rh == this.rh_)
			{
				return;
			}
			float rh = this.rh;
			if (immediate)
			{
				this.rh = this.dep_rh;
			}
			else
			{
				this.rh = X.VALWALK(this.rh, this.dep_rh, 3f);
			}
			this.need_fine_pos = (this.need_fine_base_mesh = true);
			max_y_move = X.Mx(X.Abs(rh - this.rh), max_y_move);
		}

		public void fineDepRectHeight()
		{
			this.dep_rh = this.Tx.get_sheight_px() + 96f + X.Abs(this.base_text_pos) * 0.6f;
		}

		public void reserveText(int phase, bool immediate_show = false, bool consider_cascade = true)
		{
			STB stb = TX.PopBld(null, 0);
			string text = null;
			while (phase < this.Prog.Q.end_phase)
			{
				if (text != null)
				{
					stb.Add(text);
				}
				stb.Add(this.Prog.Q.getDescription(phase));
				if (!consider_cascade || !this.Prog.Q.cascade_show_phase(phase))
				{
					break;
				}
				if (text == null)
				{
					text = TX.Get("quest_dot", "");
					stb.Insert(0, text);
				}
				phase++;
				stb.Ret("\n");
			}
			this.Tx.forceProgressNextStack(stb);
			TX.ReleaseBld(stb);
			this.fineDepRectHeight();
			if (immediate_show)
			{
				this.Tx.showImmediate(false, false);
			}
		}

		public bool updateTask(UiQuestTracker.TrackTask _Task)
		{
			if (this.Task.type != _Task.type && this.state != UiQuestTrackerRow.STATE.PREPARING)
			{
				this.Con.deactivateRow(this, false);
				return false;
			}
			int sphase = this.Task.sphase;
			this.Task = _Task;
			this.Task.sphase = X.Mx(sphase, this.Task.sphase);
			return true;
		}

		private void changeState(UiQuestTrackerRow.STATE _state)
		{
			if (this.t >= 0f)
			{
				this.state = _state;
				this.t_state = 0f;
				if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE)
				{
					SND.Ui.play("quest_track_quit", false);
				}
				if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE_FINISHED)
				{
					SND.Ui.play("quest_track_completed", false);
				}
				this.fineDepRectHeight();
			}
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			UiQuestTrackerRow.STATE state = this.state;
			bool flag2 = this.Con.CanProgress(true);
			bool flag3 = true;
			if (this.t >= 0f)
			{
				if (this.t == 0f && !flag2)
				{
					return true;
				}
				if (this.t < 160f)
				{
					this.need_fine_base_mesh = true;
				}
				this.Con.progressLock();
				if (this.t < 50f)
				{
					flag3 = false;
					if (!flag2)
					{
						this.t = 1f;
					}
					else
					{
						this.t += fcnt;
					}
				}
				else
				{
					this.t += fcnt;
				}
			}
			else
			{
				if (this.t <= -28f)
				{
					this.Md.clear(false, false);
					this.Con.deactivateRow(this, false);
					this.enabled = false;
					this.Con.need_fine_item_pos = true;
					return false;
				}
				if (this.t == -1f)
				{
					this.need_fine_base_mesh = true;
				}
				this.need_fine_pos = true;
				this.t -= fcnt;
				flag = true;
				this.fineSlideState();
			}
			if (flag3)
			{
				if (flag2)
				{
					this.Tx.run((float)((int)fcnt), false);
				}
				flag = true;
				if (this.state == UiQuestTrackerRow.STATE.PREPARING)
				{
					if (this.Task.type == QuestTracker.INVISIBLE.COLLECT)
					{
						if (this.t_state < 18f)
						{
							this.tx_y_level = -1f + X.ZLINE(this.t_state, 18f);
						}
						else
						{
							this.tx_y_level = 0f;
							if (flag2)
							{
								bool flag4;
								if (this.Prog.Q.getCollectTarget(this.Task.sphase, out flag4) != null)
								{
									this.createCollectItemField();
									this.changeState(UiQuestTrackerRow.STATE.CHECK_COLLECT);
								}
								else
								{
									this.changeState(UiQuestTrackerRow.STATE.UPDATE_CHECK);
								}
							}
						}
					}
					else if (this.t_state < 30f)
					{
						this.tx_y_level = -1f + X.ZLINE(this.t_state, 24f);
					}
					else
					{
						this.tx_y_level = 0f;
						flag = this.all_char_shown;
						if (this.t_state >= (float)((this.Task.type == QuestTracker.INVISIBLE.START) ? 70 : 30) && flag2)
						{
							this.changeState(UiQuestTrackerRow.STATE.UPDATE_CHECK);
						}
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.UPDATE_CHECK)
				{
					if (this.t_state >= 0f)
					{
						flag = false;
						while (this.Task.sphase < this.Task.dphase)
						{
							if (this.Task.Q.getDescription(this.Task.sphase) != this.Task.Q.getDescription(this.Task.sphase + 1))
							{
								this.changeState((this.Prog.Q.smooth_to_next_phase(this.Task.sphase) && this.Task.dphase < this.Task.Q.end_phase) ? UiQuestTrackerRow.STATE.GO_NEXT_SMOOTH : UiQuestTrackerRow.STATE.DRAW_LINE);
								break;
							}
							this.Task.sphase = this.Task.sphase + 1;
						}
						if (this.Task.sphase == this.Task.dphase)
						{
							flag = flag2;
							if (this.Task.Q.getDepert(this.Task.dphase).isActiveMap())
							{
								if (this.t_state >= (float)((this.Task.type == QuestTracker.INVISIBLE.START) ? 60 : 20) && flag2)
								{
									this.changeState((this.Task.type == QuestTracker.INVISIBLE.POS) ? UiQuestTrackerRow.STATE.CHECK_CURRENT_POS : UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE);
								}
							}
							else if (this.t_state >= 110f && flag2)
							{
								this.Con.deactivateRow(this, false);
							}
						}
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE)
				{
					this.need_fine_after_mesh = true;
				}
				else if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE_AFTER)
				{
					if (!flag2)
					{
						flag = false;
					}
					else
					{
						if (this.t_state == 0f)
						{
							int num = this.Task.sphase + 1;
							this.Task.sphase = num;
							if (num >= this.Task.Q.end_phase)
							{
								this.changeState(UiQuestTrackerRow.STATE.DRAW_LINE_FINISHED);
							}
						}
						if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE_AFTER)
						{
							this.tx_y_level = X.ZSIN(this.t_state, 40f);
							if (this.tx_y_level >= 1f)
							{
								this.changeState(UiQuestTrackerRow.STATE.PREPARING);
								this.reserveText(this.Task.sphase, false, true);
							}
						}
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.GO_NEXT_SMOOTH)
				{
					if (!flag2)
					{
						flag = false;
					}
					else
					{
						this.tx_y_level = X.ZSIN(this.t_state, 40f);
						if (this.tx_y_level >= 1f)
						{
							this.Task.sphase = X.Mn(this.Task.dphase, this.Task.sphase + 1);
							this.changeState(UiQuestTrackerRow.STATE.PREPARING);
							this.reserveText(this.Task.sphase, false, true);
						}
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.CHECK_COLLECT)
				{
					flag = flag2 || this.t_state < 170f;
					if (this.t_state == 0f)
					{
						bool flag5;
						NelItemEntry[] collectTarget = this.Prog.Q.getCollectTarget(this.Task.sphase, out flag5);
						if (collectTarget != null)
						{
							ushort[] collectedCount = this.Prog.getCollectedCount(this.Task.sphase);
							int num2 = collectTarget.Length;
							using (STB stb = TX.PopBld(null, 0))
							{
								using (STB stb2 = TX.PopBld(null, 0))
								{
									for (int i = 0; i < num2; i++)
									{
										NelItemEntry nelItemEntry = collectTarget[i];
										int num3 = (int)((collectedCount != null && X.BTW(0f, (float)i, (float)collectedCount.Length)) ? collectedCount[i] : 0);
										if (i > 0)
										{
											stb.Add("\n");
											stb2.Add("\n");
										}
										stb.Add(nelItemEntry.getLocalizedName(1, -1, false));
										stb2.Add("", num3, "/").Add(nelItemEntry.count);
									}
									this.TxClc.Txt(stb);
									this.TxClcCnt.Txt(stb2);
								}
							}
						}
					}
					if (this.t_state >= 145.20001f && flag2)
					{
						this.Con.deactivateRow(this, false);
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.CHECK_CURRENT_POS)
				{
					flag = flag2 || this.t_state < 130f;
					if (this.t_state == 0f)
					{
						if (this.Prog.tracking)
						{
							SND.Ui.play("quest_tracking", false);
						}
						this.TxB.text_content = TX.Get("quest_position_arrived", "");
					}
					if (this.t_state >= 220f && flag2)
					{
						this.Con.deactivateRow(this, false);
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE || this.state == UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE_POSUPDATED)
				{
					flag = flag2 || this.t_state < 130f;
					if (this.t_state == 0f)
					{
						if (this.Prog.tracking)
						{
							SND.Ui.play("quest_tracking", false);
						}
						if (!this.FirstDepert.isEqual(this.Task.Q.getDepert(this.Task.dphase)))
						{
							this.TxB.text_content = TX.Get("quest_position_updated", "");
							this.state = UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE_POSUPDATED;
						}
						else
						{
							this.TxB.text_content = TX.Get("quest_position_notice", "");
							if (this.Task.dphase == 0)
							{
								this.state = UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE_POSUPDATED;
							}
						}
					}
					if (this.t_state >= 220f && flag2)
					{
						this.Con.deactivateRow(this, false);
					}
				}
				else if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE_FINISHED)
				{
					flag = flag2 || this.t_state < 160f;
					if (this.t_state == 0f)
					{
						this.TxB.text_content = TX.Get("quest_completed", "");
					}
					if (this.t_state >= 220f && flag2)
					{
						this.Con.deactivateRow(this, false);
					}
				}
				this.fineSlideState();
			}
			if (this.state == state && flag && this.t >= 0f)
			{
				if (this.t_state < 0f)
				{
					this.t_state = X.Mn(0f, this.t_state + fcnt);
				}
				else
				{
					this.t_state += fcnt;
				}
			}
			float num4 = 4f;
			this.fineRectHeight(ref num4, false);
			if (this.need_fine_base_mesh)
			{
				this.makeBgMesh();
			}
			if (this.need_fine_bottom_mesh)
			{
				this.makeMeshBottom();
			}
			if (this.need_fine_text_pos)
			{
				this.fineTextPosition();
			}
			if (this.need_update_mesh)
			{
				this.Md.updateForMeshRenderer(true);
				this.need_update_mesh = false;
			}
			if (this.need_fine_after_mesh)
			{
				this.makeMeshAfter();
				this.MdT.updateForMeshRenderer(true);
			}
			if (this.need_fine_pos || this.Con.need_fine_item_pos)
			{
				this.need_fine_pos = false;
				float itemY = this.Con.getItemY(this, (this.t >= 0f) ? 1f : (1f - X.ZLINE(-this.t, 28f)));
				Vector2 vector = this.Mrd.transform.localPosition;
				if (this.t >= 0f && this.t <= fcnt + 2f)
				{
					vector.y = itemY * 0.015625f;
				}
				else
				{
					vector.y = X.VALWALK(vector.y, itemY * 0.015625f, num4 * 0.015625f);
					if (X.Abs(itemY - vector.y * 64f) >= 1.5f)
					{
						this.need_fine_pos = true;
					}
				}
				if (this.t >= 0f)
				{
					vector.x = 0f;
				}
				else
				{
					vector.x = X.ZPOW(-this.t, 28f) * 0.015625f * 510f * (float)X.MPF(this.position_is_right);
				}
				this.Mrd.transform.localPosition = vector;
			}
			return true;
		}

		public void makeBgMesh()
		{
			this.need_fine_base_mesh = false;
			this.need_update_mesh = true;
			this.Md.clear(false, false);
			this.Ma.Set(true);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.base_x = (this.Md.base_y = 0f);
			if (this.t == 0f)
			{
				return;
			}
			this.Md.Col = C32.MulA(MTRX.ColBlack, 0.6f);
			float num = (float)(this.position_is_right ? (-1) : 1);
			float num3;
			float num4;
			float num5;
			for (int i = 0; i < 2; i++)
			{
				float num2 = ((this.t >= 0f) ? ((i == 0) ? (this.t - 2f) : (this.t - 24f)) : (1f - X.ZLINE(-this.t - (float)(2 * i), 18f)));
				if (num2 > 0f)
				{
					if (num2 >= 2f && (this.sound_bits & (1U << i)) == 0U)
					{
						SND.Ui.play("quest_popup_0", false);
						this.sound_bits |= 1U << i;
					}
					num3 = ((this.t >= 0f) ? X.ZPOP(num2, 44f, 0.125f) : num2);
					num4 = (-UiQuestTrackerRow.rwh + (float)((i == 0) ? 2 : (-8))) * num;
					num5 = this.rh * 0.5f + (float)((i == 0) ? 32 : 6);
					this.Md.Circle(num4, num5, (float)(10 + 5 * i) * num3, 0f, false, 0f, 0f);
				}
			}
			float num6;
			if (this.t >= 0f)
			{
				float num2 = this.t - 52f;
				if (num2 <= 0f)
				{
					return;
				}
				if (num2 >= 2f && (this.sound_bits & 4U) == 0U)
				{
					SND.Ui.play("quest_popup_1", false);
					this.sound_bits |= 4U;
				}
				num3 = X.ZBOUNCE(num2, 44f);
				num6 = X.ZSIN2(num2, 35f);
			}
			else
			{
				num3 = 1f;
				num6 = 1f;
			}
			num4 = -44f * (1f - num6);
			num5 = -22f * (1f - num6);
			float num7 = num3 * UiQuestTrackerRow.rw;
			float num8 = num3 * this.rh;
			this.Md.KadomaruRect((num4 - UiQuestTrackerRow.rw * 0.5f + num7 * 0.5f) * num, num5 + this.rh * 0.5f - num8 * 0.5f, num7, num8, 32f, 0f, false, 0f, 0f, false);
			this.Ma.Set(false);
			this.need_fine_bottom_mesh = true;
		}

		private static float VerticeX(Vector3[] AVer, int si)
		{
			if (AVer.Length == 0)
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < 2; i++)
			{
				float x = AVer[X.MMX(0, si + i, AVer.Length - 1)].x;
				if (i == 0)
				{
					num = x;
				}
				if (i == 1)
				{
					num2 = x;
				}
			}
			return (num + num2) * 0.5f;
		}

		public void makeMeshAfter()
		{
			this.need_fine_after_mesh = false;
			MeshDrawer meshDrawer = this.MdT.clear(false, false);
			if (this.isDrawLineState())
			{
				Vector3 localPosition = this.Tx.transform.localPosition;
				meshDrawer.base_x = localPosition.x;
				meshDrawer.base_y = localPosition.y;
				meshDrawer.Col = C32.MulA(MTRX.ColWhite, X.ZSIN2(this.Tx.alpha));
				List<int> aline_ver_pos = this.Tx.Aline_ver_pos;
				int count = aline_ver_pos.Count;
				if (count >= 2)
				{
					Vector3[] vertexArray = this.Tx.getMeshDrawer().getVertexArray();
					int num = aline_ver_pos[0];
					float num2 = UiQuestTrackerRow.VerticeX(vertexArray, num);
					float num3 = this.Tx.get_sheight_px() * 0.5f - this.Tx.get_line_sheight_px(false) * 0.5f;
					int num4 = count - 1;
					float num5 = ((this.state == UiQuestTrackerRow.STATE.DRAW_LINE) ? this.t_state : ((float)num4 * 18f * 2f));
					for (int i = 1; i < count; i++)
					{
						num = aline_ver_pos[i];
						float num6 = UiQuestTrackerRow.VerticeX(vertexArray, num - 2);
						float num7 = num5 / 18f;
						if (num7 <= 0f)
						{
							num5 = -1f;
							break;
						}
						if (num2 != num6)
						{
							meshDrawer.Line(num2 * 64f, num3, X.NIL(num2, num6, num7, 1f) * 64f, num3, 2f, false, 0f, 0f);
							num5 -= 18f;
						}
						if (i < count - 1)
						{
							num2 = UiQuestTrackerRow.VerticeX(vertexArray, num);
						}
						num3 -= this.Tx.get_line_sheight_px(false);
					}
					if (num5 >= 0f && this.state == UiQuestTrackerRow.STATE.DRAW_LINE)
					{
						this.changeState(UiQuestTrackerRow.STATE.DRAW_LINE_AFTER);
					}
				}
			}
		}

		public void makeMeshBottom()
		{
			this.need_fine_bottom_mesh = false;
			this.need_update_mesh = true;
			this.Md.chooseSubMesh(1, false, true);
			this.Md.chooseSubMesh(0, false, false);
			this.Ma.revertVerAndTriIndexSaved(false);
			float num = -UiQuestTrackerRow.rw * 0.5f + 20f + 34f;
			float num2 = -this.rh * 0.5f + 36f - 8f;
			float num3 = X.ZLINE(this.t_state, 40f);
			float num4 = 1f - num3;
			if (this.isTextSlideState() && this.state != UiQuestTrackerRow.STATE.CHECK_COLLECT)
			{
				if (this.state == UiQuestTrackerRow.STATE.DRAW_LINE_FINISHED)
				{
					float num5 = X.ZPOW(this.t_state, 28f);
					float num6 = 1f + (1f - num5);
					this.Md.Col = this.Md.ColGrd.Set(4282253124U).mulA(num3 * num5).C;
					this.Md.CheckMark(num, num2, num6 * 25f, 5f * num6, false);
					UiQuestTrackerRow.Uni.Col(this.Md.ColGrd.C, 0.3f);
					UiQuestTrackerRow.Uni.drawTo(this.Md, num, num2, (float)IN.totalframe, false);
					if (num5 >= 1f)
					{
						num5 = X.ZLINE(this.t_state - 28f, 50f);
						if (num5 < 1f)
						{
							this.Md.chooseSubMesh(1, false, false);
							this.Md.Col = this.Md.ColGrd.Set(4282253124U).mulA(num3 * (1f - num5)).C;
							this.Md.initForImg((num5 < 0.08f) ? MTRX.EffCircle128 : MTRX.EffRippleCircle245, 0);
							float num7 = 140f * (0.25f + 0.75f * X.ZSIN2(num5));
							this.Md.Rect(num, num2, num7, num7, false);
						}
					}
					this.need_fine_bottom_mesh = true;
					return;
				}
				num2 -= 16f;
				this.Md.chooseSubMesh(1, false, false);
				this.Md.Col = this.Prog.Q.PinColor;
				if (this.state == UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE_POSUPDATED)
				{
					NEL.drawQuestDepertPinAnim(this.Md, IN.totalframe, num, num2, num3, 1f + 0.33f * num4, 0.5f, 0.25f, this.Prog.tracking ? 0.75f : 0f);
					this.need_fine_bottom_mesh = true;
				}
				else
				{
					NEL.drawQuestDepertPin(this.Md, num, num2, num3, 1f + 0.33f * num4, 0.33f, this.Prog.tracking ? 0.75f : 0f);
				}
				if (this.Prog.tracking)
				{
					this.need_fine_bottom_mesh = true;
				}
			}
		}

		public void fineTextPosition()
		{
			this.need_fine_text_pos = false;
			float num = this.base_text_pos + 38f * this.tx_y_level_;
			IN.PosP2(this.Tx.transform, -UiQuestTrackerRow.rw * 0.5f + 30f, num);
			if (this.isDrawLineState())
			{
				this.need_fine_after_mesh = true;
				this.need_fine_bottom_mesh = true;
			}
		}

		public void fineSlideState()
		{
			if (this.isTextSlideState())
			{
				float num = 20f;
				this.TxB.alpha = X.ZLINE(this.t_state - 20f, 30f);
				if (this.state == UiQuestTrackerRow.STATE.CHECK_COLLECT)
				{
					num = X.Mx(num, this.TxClc.get_sheight_px() + 10f);
					this.TxClc.alpha = (this.TxClcCnt.alpha = X.ZLINE(this.t_state, 15f));
				}
				this.base_text_pos = num * X.ZLINE(this.t_state, 60f);
				if (this.state == UiQuestTrackerRow.STATE.CHECK_COLLECT)
				{
					float num2 = this.Tx.transform.localPosition.y * 64f - this.Tx.get_sheight_px() * 0.5f - 8f - this.TxClc.get_sheight_px() * 0.5f;
					IN.PosP2(this.TxClc.transform, -UiQuestTrackerRow.rwh + 38f, num2);
					IN.PosP2(this.TxClcCnt.transform, UiQuestTrackerRow.rwh - 20f, num2);
				}
				if (this.t_state < 60f || (this.state != UiQuestTrackerRow.STATE.CHECK_CURRENT_PHASE && this.state != UiQuestTrackerRow.STATE.CHECK_CURRENT_POS))
				{
					this.need_fine_after_mesh = (this.need_fine_bottom_mesh = true);
				}
			}
		}

		public bool isTextSlideState()
		{
			UiQuestTrackerRow.STATE state = this.state;
			return state - UiQuestTrackerRow.STATE.DRAW_LINE_FINISHED <= 4;
		}

		public bool isDrawLineState()
		{
			UiQuestTrackerRow.STATE state = this.state;
			return state - UiQuestTrackerRow.STATE.DRAW_LINE <= 2;
		}

		public float rh
		{
			get
			{
				return this.rh_;
			}
			set
			{
				if (this.rh == value)
				{
					return;
				}
				this.rh_ = value;
				IN.PosP2(this.TxB.transform, 34f, -this.rh * 0.5f + 36f);
				this.need_fine_pos = true;
			}
		}

		public float tx_y_level
		{
			get
			{
				return this.tx_y_level_;
			}
			set
			{
				if (this.tx_y_level == value)
				{
					return;
				}
				this.tx_y_level_ = value;
				if (this.t >= 0f)
				{
					if (value >= 0f)
					{
						this.Tx.alpha = X.ZLINE(1f - value);
					}
					else
					{
						this.Tx.alpha = X.ZLINE(1f + value, 0.25f);
					}
				}
				this.need_fine_text_pos = true;
			}
		}

		public float base_text_pos
		{
			get
			{
				return this.base_text_pos_;
			}
			set
			{
				if (this.base_text_pos == value)
				{
					return;
				}
				this.base_text_pos_ = value;
				this.fineDepRectHeight();
				this.need_fine_text_pos = true;
			}
		}

		public void deactivate(bool immediate = false)
		{
			if (immediate)
			{
				this.Mrd.gameObject.SetActive(false);
				if (this.t > -28f)
				{
					this.t = -28f;
					return;
				}
			}
			else if (this.t >= 0f)
			{
				this.t = -1f;
			}
		}

		public void progressReserved()
		{
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public void executeRestMsgCmd()
		{
		}

		public bool all_char_shown { get; set; }

		public string talker_snd
		{
			get
			{
				return null;
			}
		}

		public void setHkdsTypeToDefault(bool reserve = false)
		{
		}

		public void setHkdsType(NelMSG.HKDSTYPE _type)
		{
		}

		public EMOT default_emot
		{
			get
			{
				return EMOT.FADEIN;
			}
		}

		public uint default_col
		{
			get
			{
				return uint.MaxValue;
			}
		}

		public bool is_temporary_hiding
		{
			get
			{
				return this.Con.isShowing(false);
			}
		}

		public void FixTextContent(STB Stb)
		{
		}

		public void TextRendererUpdated()
		{
		}

		public bool position_is_right
		{
			get
			{
				return UiQuestTracker.position_is_right;
			}
		}

		public void destruct()
		{
			this.Md.destruct();
			this.MdT.destruct();
		}

		private UiQuestTracker Con;

		public QuestTracker.QuestProgress Prog;

		private UiQuestTracker.TrackTask Task;

		private float t;

		private float t_state;

		private UiQuestTrackerRow.STATE state;

		public bool need_fine_pos;

		private bool need_fine_base_mesh;

		private bool need_fine_text_pos;

		private bool need_fine_after_mesh;

		private bool need_fine_bottom_mesh;

		private uint sound_bits;

		private bool need_update_mesh;

		private float tx_y_level_;

		private float base_text_pos_;

		private QuestTracker.QuestDeperture FirstDepert;

		private MeshDrawer Md;

		private MeshDrawer MdT;

		private GameObject Gob;

		private ValotileRenderer Valot;

		private ValotileRenderer ValotT;

		private MdArranger Ma;

		private MeshRenderer Mrd;

		private MeshRenderer MrdT;

		private NelEvTextRenderer Tx;

		private TextRenderer TxB;

		private TextRenderer TxClc;

		private TextRenderer TxClcCnt;

		public static readonly float rw = 296f;

		public static readonly float rwh = UiQuestTrackerRow.rw * 0.5f;

		private float rh_;

		private float dep_rh;

		public const float margin_x = 30f;

		public const float margin_x_b = 20f;

		public const float margin_y = 48f;

		public const float min_h = 50f;

		public const float txb_shift_x = 34f;

		private const float MAXT = 28f;

		private const float MAXT_SLIDE_TEXT = 40f;

		private const float MAXT_DEACTIVATE = 220f;

		private const float T_POPUP_0 = 2f;

		private const float T_POPUP_1 = 24f;

		private const float T_POPUP_2 = 52f;

		private static UniBrightDrawer Uni;

		private const float mbottom_y = 0.75f;

		private enum STATE
		{
			PREPARING,
			UPDATE_CHECK,
			GO_NEXT_SMOOTH,
			DRAW_LINE,
			DRAW_LINE_AFTER,
			DRAW_LINE_FINISHED,
			CHECK_CURRENT_PHASE,
			CHECK_CURRENT_PHASE_POSUPDATED,
			CHECK_CURRENT_POS,
			CHECK_COLLECT
		}
	}
}
