using System;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm
{
	public class UiMgmTimer : IRunAndDestroy, IEventWaitListener, IEventListener, IPauseable
	{
		public Map2d Mp
		{
			get
			{
				if (this.M2D == null)
				{
					return null;
				}
				return this.M2D.curMap;
			}
		}

		public float box_w
		{
			get
			{
				return this.box_w_;
			}
			set
			{
				if (this.box_w_ != value)
				{
					this.box_w_ = value;
					this.fine_pos_bits = this.FPOS__ALL;
					this.draw_main_bits = 15U;
					this.need_main_repos = true;
				}
			}
		}

		private float box_wh
		{
			get
			{
				return this.box_w * 0.5f;
			}
		}

		public float bary
		{
			get
			{
				return this.box_h * 0.5f - 33f;
			}
		}

		public float score_top_y
		{
			get
			{
				return this.bary - 5f;
			}
		}

		private float barx
		{
			get
			{
				return (float)((int)(-this.bar_w * 0.5f));
			}
		}

		private float chr_y
		{
			get
			{
				return this.bary - 2f;
			}
		}

		private float box_margin
		{
			get
			{
				return 58f;
			}
		}

		private float bar_w
		{
			get
			{
				return this.box_w - this.box_margin;
			}
		}

		public UiMgmTimer(NelM2DBase _M2D, string gob_name, int score_capacity = 0)
		{
			this.M2D = _M2D;
			this.Gob = IN.CreateGobGUI(null, gob_name);
			this.Gob.AddComponent<UiMgmTimerBehaviour>().Timer = this;
			this.MdB = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MtrMeshNormal, 0f, -1, null, true, false);
			this.MdB.chooseSubMesh(1, false, false);
			this.MdB.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMALP3, -1), false);
			this.AMa = new MdArranger[4];
			for (int i = 3; i >= 0; i--)
			{
				this.AMa[i] = new MdArranger(this.MdB);
			}
			if (score_capacity > 0)
			{
				this.AScore = new UiMgmTimer.ScoreCounter[score_capacity];
			}
			MeshRenderer component = this.Gob.GetComponent<MeshRenderer>();
			this.MdB.connectRendererToTriMulti(component);
			this.Gob.SetActive(false);
			IN.setZAbs(this.Gob.transform, -4.8500004f);
			if (this.M2D != null)
			{
				this.M2D.loadMaterialSnd("minigame");
				return;
			}
			SND.loadSheets("minigame", "MGMTIMER");
		}

		public void activate(int totalcount, int countdown_sec)
		{
			if (this.count == -1000)
			{
				IN.addRunner(this);
				this.MdB.clear(false, false);
				this.draw_main_bits = 0U;
			}
			if (this.Ef == null && this.attach_effect && this.M2D != null)
			{
				this.Ef = this.Mp.getEffectTop().setEffectWithSpecificFn("MGTimer", 0f, 0f, this.Mp.floort, -1000, 0, new FnEffectRun(this.fnDrawEffect));
				this.M2D.AssignPauseableP(this);
			}
			if (!this.attach_effect && this.MdTopCD == null)
			{
				GameObject gameObject = (this.GobTopCD = IN.CreateGob(this.Gob, "-TopCD"));
				IN.setZ(gameObject.transform, -0.05f);
				this.MdTopCD = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MIicon.getMtr(BLEND.MUL, -1), 0f, -1, null, true, true);
				this.MdTopCD.chooseSubMesh(1, false, false);
				this.MdTopCD.setMaterial(MTRX.MtrMeshNormal, false);
				this.MdTopCD.chooseSubMesh(2, false, false);
				this.MdTopCD.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMALP3, -1), false);
				this.MdTopCD.connectRendererToTriMulti(gameObject.GetComponent<MeshRenderer>());
			}
			if (this.MdTopCD != null)
			{
				this.MdTopCD.clear(false, false);
			}
			this.pausing = false;
			this.count = totalcount;
			this.move_t = -30f;
			this.t = (float)(-(float)countdown_sec * 60);
			if (this.Ef != null)
			{
				this.Ef.y = 0f;
				this.Ef.z = this.Mp.floort;
			}
			if (countdown_sec <= 0)
			{
				this.initMainCount();
			}
			else
			{
				this.main_time = -1000;
			}
			if (this.M2D != null)
			{
				this.M2D.MapTitle.deactivate(false, true);
			}
		}

		public void deactivate(bool immediate_destruct = false)
		{
			if (this.M2D != null)
			{
				this.M2D.DeassignPauseable(this);
			}
			else
			{
				SND.unloadSheets("minigame", "MGMTIMER");
			}
			if (immediate_destruct)
			{
				this.count = -1000;
				IN.remRunner(this);
				this.Gob.SetActive(false);
			}
			this.temp_visible = false;
			if (this.Ef != null)
			{
				this.Ef.destruct();
				this.Ef = null;
			}
			if (this.MdTopCD != null)
			{
				this.MdTopCD.destruct();
				this.MdTopCD = null;
				IN.DestroyE(this.GobTopCD);
			}
			this.main_time = -1001;
			if (this.FnDone != null)
			{
				this.FnDone(this, true);
				this.FnDone = null;
			}
			EV.remListener(this);
		}

		public void destruct()
		{
			this.deactivate(true);
			EV.remListener(this);
			IN.DestroyE(this.Gob);
		}

		public void Pause()
		{
			this.pausing = true;
		}

		public void Resume()
		{
			this.pausing = false;
		}

		public void initMainCount()
		{
			this.t = 0f;
			this.move_t = 0f;
			this.main_time = this.count;
			if (this.Ef != null)
			{
				this.Ef.z = this.Mp.floort;
				this.Ef.y = 1f;
				this.Ef.af = 0f;
				this.Ef.f0 = (int)this.Mp.floort;
			}
			this.Gob.SetActive(true);
			this.MdB.clear(false, false);
			this.need_main_repos = true;
			this.draw_main_bits = 15U;
			this.fine_pos_bits |= this.FPOS_SCORE_CHECK;
		}

		public void countFinished()
		{
			if (this.main_time != 0)
			{
				this.main_time = 0;
				if (this.FnDone != null)
				{
					this.FnDone(this, false);
					if (this.main_time <= 0)
					{
						this.FnDone = null;
					}
				}
			}
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			if (this.main_time >= -1000)
			{
				if (this.t < 0f)
				{
					int num = -X.IntC(-this.t / 60f);
					if (this.main_time != num)
					{
						this.main_time = num;
						if (this.Ef != null)
						{
							this.Ef.af = 0f;
						}
						SND.Ui.play("minigame_count", false);
					}
					this.draw_mdtopcd = true;
					if (!this.pausing)
					{
						this.t += fcnt;
					}
					if (this.t >= 0f)
					{
						SND.Ui.play("minigame_init", false);
						this.initMainCount();
					}
				}
				else if (this.main_time > 0)
				{
					if (!this.pausing)
					{
						this.t += fcnt * ((this.attach_effect && this.M2D != null && this.M2D.pre_map_active) ? Map2d.TS : 1f);
					}
					else
					{
						this.fine_pos_bits |= this.FPOS_PAUSE_BLINK;
					}
					this.draw_main_bits |= 2U;
					if (this.t >= 60f)
					{
						this.draw_main_bits |= 4U;
						this.t -= 60f;
						this.digit_prog_totalframe = IN.totalframe;
						int num2 = this.main_time;
						int num3 = X.keta_count(this.main_time);
						this.main_time--;
						if (this.main_time <= 0)
						{
							this.main_time = 1;
							this.countFinished();
						}
						else if (this.main_time <= 5)
						{
							SND.Ui.play("minigame_count_ingame", false);
						}
						if (num3 > X.keta_count(this.main_time))
						{
							this.draw_main_bits |= 8U;
						}
					}
					if (this.main_time <= 20)
					{
						this.fine_pos_bits |= this.FPOS_KADOMARU;
					}
					if (this.main_time <= 5)
					{
						this.draw_mdtopcd = true;
					}
				}
			}
			else
			{
				if (this.t < 0f)
				{
					return false;
				}
				this.fine_pos_bits |= this.FPOS_KADOMARU;
				this.t += fcnt;
				if (this.t <= 100f)
				{
					this.need_main_repos = true;
				}
			}
			if (X.D)
			{
				if (this.move_t >= 0f)
				{
					if (this.move_t < 30f)
					{
						this.move_t += fcnt;
						this.need_main_repos = true;
					}
				}
				else
				{
					if (this.move_t > -30f)
					{
						this.move_t -= fcnt;
						this.need_main_repos = true;
					}
					if (this.move_t <= -30f && this.main_time <= -1001)
					{
						this.count = -1000;
						this.Gob.SetActive(false);
						return false;
					}
				}
				bool flag2 = false;
				if ((this.fine_pos_bits & this.FPOS_SCORE_CHECK) != 0U)
				{
					this.fine_pos_bits &= ~this.FPOS_SCORE_CHECK;
					if (this.AScore != null)
					{
						bool flag3 = false;
						for (int i = this.AScore.Length - 1; i >= 0; i--)
						{
							UiMgmTimer.ScoreCounter scoreCounter = this.AScore[i];
							bool flag4 = false;
							if (scoreCounter.score_type == UiMgmTimer.SCORE_TYPE.TEXT_SUFFIX)
							{
								if (scoreCounter.Tx == null)
								{
									TextRenderer textRenderer = (scoreCounter.Tx = IN.CreateGob(this.Gob, "-Tx" + i.ToString()).AddComponent<TextRenderer>());
									textRenderer.Size(12f).Col(MTRX.ColWhite).Align(ALIGN.RIGHT)
										.AlignY(ALIGNY.MIDDLE);
									textRenderer.use_valotile = true;
									flag4 = true;
								}
								IN.PosP2(scoreCounter.Tx.transform, this.bar_w * 0.5f - 4f, this.score_top_y - 20f * ((float)i + 0.5f));
							}
							if (scoreCounter.cushion_animating)
							{
								if (scoreCounter.cushion_d == 0f)
								{
									scoreCounter.fineCushionSpeed();
								}
								scoreCounter.cushion += scoreCounter.cushion_d;
								if (scoreCounter.cushion < scoreCounter.value != scoreCounter.cushion_d > 0f)
								{
									scoreCounter.cushion = scoreCounter.value;
									scoreCounter.cushion_d = 0f;
								}
								else
								{
									flag3 = (flag2 = true);
								}
								this.draw_main_bits |= 8U;
								flag4 = true;
							}
							if (scoreCounter.score_type == UiMgmTimer.SCORE_TYPE.TEXT_SUFFIX && flag4)
							{
								using (STB stb = TX.PopBld(null, 0))
								{
									stb.AddTxA(scoreCounter.score_suffix, false);
									stb.TxRpl(scoreCounter.cushion);
									scoreCounter.Tx.Txt(stb);
									this.box_w = X.Mx(scoreCounter.Tx.get_swidth_px() + this.box_margin + 40f, this.box_w_);
								}
							}
							this.AScore[i] = scoreCounter;
						}
						if (flag3)
						{
							SND.Ui.play("mg_score_add_counter", false);
						}
					}
				}
				if (this.need_main_repos)
				{
					this.repositMain();
					this.need_main_repos = false;
				}
				if (this.draw_main_bits > 0U)
				{
					this.drawMain();
					flag = true;
				}
				if (this.draw_mdtopcd && this.MdTopCD != null)
				{
					this.MdTopCD.clear(false, false);
					this.draw_mdtopcd = false;
					float num4;
					this.drawCountDown(out num4, this.MdTopCD, this.MdTopCD, this.MdTopCD, MTRX.ChrLb, X.Abs(this.main_time), this.t, 3, 0.75f);
					this.MdTopCD.updateForMeshRenderer(false);
				}
				if (this.fine_pos_bits != 0U)
				{
					uint num5 = this.fine_pos_bits;
					this.fine_pos_bits = 0U;
					if ((num5 & this.FPOS_KADOMARU) != 0U)
					{
						flag = true;
						this.fineKadomaruColor();
					}
					if ((num5 & this.FPOS_BAR) != 0U)
					{
						flag = true;
						this.fineBarWidth();
					}
					if ((num5 & this.FPOS_PAUSE_BLINK) != 0U)
					{
						flag = true;
						this.finePauseBlinkChr();
					}
				}
				if (flag2)
				{
					this.fine_pos_bits |= this.FPOS_SCORE_CHECK;
				}
				if (flag)
				{
					this.MdB.chooseSubMesh(1, false, false);
					this.AMa[3].revertVerAndTriIndexSaved(true);
					this.MdB.updateForMeshRenderer(true);
				}
			}
			return true;
		}

		private MeshDrawer GetMesh(EffectItem Ef, MImage MI, BLEND blnd = BLEND.NORMAL, float z_shift = -0.01f)
		{
			MeshDrawer meshImg = Ef.GetMeshImg("mg_timer", MTRX.MIicon, blnd, false);
			if (meshImg.getTriMax() == 0)
			{
				meshImg.base_z += -2.01f + z_shift;
			}
			Vector3 posMainTransform = this.M2D.Cam.PosMainTransform;
			meshImg.base_x = posMainTransform.x;
			meshImg.base_y = posMainTransform.y + 0.23f;
			return meshImg;
		}

		private MeshDrawer GetMesh(EffectItem Ef, BLEND blnd = BLEND.NORMAL, float z_shift = 0f)
		{
			MeshDrawer mesh = Ef.GetMesh("mg_timer", uint.MaxValue, blnd, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z += -2f + z_shift;
			}
			Vector3 posMainTransform = this.M2D.Cam.PosMainTransform;
			mesh.base_x = posMainTransform.x;
			mesh.base_y = posMainTransform.y + 0.23f;
			return mesh;
		}

		public bool fnDrawEffect(EffectItem E)
		{
			MeshDrawer mesh = this.GetMesh(E, BLEND.NORMAL, 0f);
			if (this.t < 0f)
			{
				if (E != this.Ef)
				{
					return false;
				}
				MeshDrawer mesh2 = this.GetMesh(E, MTRX.MIicon, BLEND.NORMAL, -0.01f);
				this.drawCountDown(mesh, mesh2, MTRX.ChrL, -this.main_time, E.af, 4, 1f);
			}
			else
			{
				if (this.main_time > 0)
				{
					if (this.main_time <= 5)
					{
						MeshDrawer mesh3 = this.GetMesh(E, MTRX.MIicon, BLEND.NORMALP3, -0.01f);
						MeshDrawer mesh4 = this.GetMesh(E, MTRX.MIicon, BLEND.MUL, 0.02f);
						float num;
						this.drawCountDown(out num, mesh4, mesh, mesh3, MTRX.ChrLb, this.main_time, this.t, 3, 0.75f);
					}
					else if (E.y == 1f)
					{
						float num2 = this.Mp.floort - (float)E.f0;
						if (num2 < 150f)
						{
							MeshDrawer mesh5 = this.GetMesh(E, MTRX.MIicon, BLEND.NORMALP3, -0.01f);
							float num3 = X.ZLINE(num2, 150f);
							float num4 = 1f - X.ZLINE(num3 - 0.875f, 0.125f);
							mesh.Col = mesh.ColGrd.White().mulA(0.7f * num4).C;
							float num5 = 200f * (1f + 2f * (1f - X.ZLINE(num2, 16f)) - 0.125f * (1f - X.ZSIN(num2 - 15f, 20f)));
							mesh.Scale(1f, X.NIL(1f, 0.8f, num3, 0.2f), false);
							mesh.Star(0f, 0f, num5, E.af / 80f * 3.1415927f, 12, 0.44f, 0f, false, 0f, 0f);
							float num6 = (float)((num2 < 6f) ? 6 : 4) * (2f - 1.125f * X.ZLINE(num2, 6f) + 0.125f * X.ZSIN(num2 - 5f, 16f));
							mesh5.allocUv23(16, false);
							float num7 = X.COSI(E.af, 18f);
							mesh5.Uv23(mesh5.ColGrd.Black().Scr(MTRX.ColWhite, 0.6f + 0.4f * num7).C, false);
							mesh5.Col = C32.WMulA(num4 * (0.8f + 0.2f * num7));
							using (STB stb = TX.PopBld("GO!", 0))
							{
								MTRX.ChrL.DrawScaleStringTo(mesh5, stb, 0f, 0f, num6, num6, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
							}
							mesh5.allocUv23(0, true);
						}
						else
						{
							E.y = 2f;
						}
					}
				}
				float num8 = this.move_t;
			}
			return true;
		}

		private float drawCountDown(MeshDrawer Md, MeshDrawer MdC, BMListChars Chr, int count_str, float af60, int char_scl = 3, float circle_radius_ratio = 1f)
		{
			float num;
			return this.drawCountDown(out num, null, Md, MdC, Chr, count_str, af60, char_scl, circle_radius_ratio);
		}

		private float drawCountDown(out float tz_in, MeshDrawer MdM, MeshDrawer Md, MeshDrawer MdC, BMListChars Chr, int count_str, float af60, int char_scl = 3, float circle_radius_ratio = 1f)
		{
			float num = 1f - X.ZLINE(af60 - 15f, 45f) * 0.875f;
			float num2 = X.ZSIN2(af60, 10f) * 1.1f - X.ZCOS(af60 - 10f, 24f) * 0.1f;
			if (MdC == Md)
			{
				Md.chooseSubMesh(2, false, false);
			}
			if (Chr == MTRX.ChrLb)
			{
				MdC.allocUv23(4, false);
				MdC.Uv23(MdC.ColGrd.Black().C, false);
			}
			MdC.Col = C32.WMulA(num);
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(count_str);
				Chr.DrawScaleStringTo(MdC, stb, 0f, 0f, 3f, 3f * num2, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
			}
			if (Chr == MTRX.ChrLb)
			{
				MdC.allocUv23(0, true);
			}
			if (MdC == Md)
			{
				Md.chooseSubMesh(1, false, false);
			}
			float num3 = X.ZLINE(af60, 30f);
			tz_in = X.ZSIN2(af60, 20f);
			float num4 = X.ZLINE(af60 - 30f, 30f);
			float num5 = 1f - num4;
			int vertexMax = Md.getVertexMax();
			float num6 = 6.2831855f * num3;
			float num7 = X.NI(70, 100, X.ZLINE(af60, 65f));
			Md.Col = MTRX.ColWhite;
			Md.Arc2(0f, 0f, num7, num7, 1.5707964f + num6 - X.Mn(num6, num5 * 3.1415927f * 1.85f), 1.5707964f + num6, 3f, 0f, 0f);
			int vertexMax2 = Md.getVertexMax();
			Color32[] colorArray = Md.getColorArray();
			for (int i = vertexMax; i < vertexMax2; i++)
			{
				float num8 = X.Scr(X.ZLINE((float)(i - vertexMax), (float)(vertexMax2 - vertexMax)), 1f - num3) * (1f - num4 * 0.75f);
				colorArray[i] = Md.ColGrd.Set(colorArray[i]).mulA(num8).C;
			}
			if (MdM != null)
			{
				if (MdM == Md)
				{
					Md.chooseSubMesh(0, false, false);
				}
				MdM.Col = MdM.ColGrd.Black().mulA(0.4f * num * tz_in).C;
				float num9 = X.NI(170, 140, num);
				MdM.initForImg(MTRX.EffBlurCircle245, 0);
				MdM.Rect(0f, 0f, num9, num9, false);
			}
			return num;
		}

		private void repositMain()
		{
			float num;
			if (this.move_t >= 0f)
			{
				num = X.ZSIN(this.move_t, 30f);
			}
			else
			{
				num = X.ZLINE(30f + this.move_t, 30f);
			}
			float num2 = IN.w * 0.5f + X.NI(this.box_wh, -(this.box_wh - 20f), num);
			float num3 = IN.hh - this.box_h * 0.5f - 18f;
			IN.PosP2(this.Gob.transform, num2, num3);
			if (this.MdTopCD != null)
			{
				IN.PosP2(this.GobTopCD.transform, -num2, -num3);
			}
		}

		private void drawMain()
		{
			uint num = this.draw_main_bits;
			bool flag = false;
			if (num == 15U)
			{
				this.MdB.clear(false, false);
				flag = true;
			}
			if (this.main_time <= -1001)
			{
				return;
			}
			this.draw_main_bits = 0U;
			this.MdB.chooseSubMesh(0, false, false);
			this.MdB.Identity();
			if ((num & 1U) != 0U)
			{
				if (flag)
				{
					this.AMa[0].Set(true);
				}
				this.AMa[0].revertVerAndTriIndexFirstSaved(true);
				float w = IN.w;
				X.Abs((this.M2D != null) ? (this.M2D.ui_shift_x * 2f) : 0f);
				this.fine_pos_bits |= this.FPOS_KADOMARU;
				this.MdB.ColGrd.Black();
				this.MdB.Col = this.MdB.ColGrd.mulA(0.7f).C;
				this.MdB.KadomaruRect(0f, 0f, this.box_w, this.box_h, 20f, 0f, false, 0f, 0f, false);
				this.AMa[0].Set(false);
			}
			else
			{
				this.AMa[0].revertVerAndTriIndexSaved(true);
			}
			if ((num & 2U) != 0U)
			{
				this.fine_pos_bits |= this.FPOS_BAR;
				if ((num & 1U) > 0U || flag)
				{
					this.AMa[1].Set(true);
				}
				this.AMa[1].revertVerAndTriIndexFirstSaved(true);
				this.MdB.Col = this.MdB.ColGrd.Set(4284703587U).mulA(1f).C;
				this.MdB.RectBL(this.barx, this.bary, this.bar_w * 0.5f, 1f, false);
				this.MdB.RectBL(this.barx + this.bar_w * 0.5f, this.bary, this.bar_w * 0.5f, 1f, false);
				this.AMa[1].Set(false);
			}
			else
			{
				this.AMa[1].revertVerAndTriIndexSaved(true);
			}
			this.MdB.chooseSubMesh(1, false, false);
			if ((num & 4U) != 0U)
			{
				if (flag)
				{
					this.AMa[2].Set(true);
				}
				this.AMa[2].revertVerAndTriIndexFirstSaved(true);
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add(this.main_time);
					float num2 = X.ZSIN((float)(IN.totalframe - this.digit_prog_totalframe), 30f);
					float num3 = 1.25f;
					if (num2 < 1f)
					{
						this.draw_main_bits |= 4U;
					}
					if (this.main_time <= 5)
					{
						num3 = 1.45f;
						this.MdB.ColGrd.Set(4294901760U);
					}
					else if (this.main_time <= 10)
					{
						this.MdB.ColGrd.Set(4294967040U);
					}
					else
					{
						this.MdB.ColGrd.White();
					}
					float num4 = X.NI(num3, 1f, num2);
					this.MdB.allocUv23(stb.Length * 4, false);
					this.MdB.Col = this.MdB.ColGrd.C;
					this.MdB.Uv23(this.MdB.ColGrd.Black().Scr(MTRX.ColWhite, 0.25f * (1f - num2)).setA(0f)
						.C, false);
					MTRX.ChrL.DrawScaleStringTo(this.MdB, stb, 0f, this.chr_y, 2f, num4 * 2f, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
					this.MdB.allocUv23(0, true);
				}
				this.AMa[2].Set(false);
			}
			else
			{
				this.AMa[2].revertVerAndTriIndexSaved(true);
			}
			if ((num & 8U) != 0U)
			{
				if ((num & 4U) > 0U || flag)
				{
					this.AMa[3].Set(true);
				}
				this.fine_pos_bits |= this.FPOS_PAUSE_BLINK;
				this.AMa[3].revertVerAndTriIndexFirstSaved(true);
				if (this.AScore != null)
				{
					float num5 = -1f;
					float num6 = -1f;
					using (STB stb2 = TX.PopBld(null, 0))
					{
						using (STB stb3 = TX.PopBld(null, 0))
						{
							int num7 = this.AScore.Length;
							for (int i = 0; i < num7; i++)
							{
								UiMgmTimer.ScoreCounter scoreCounter = this.AScore[i];
								float num8 = this.score_top_y - 20f * (float)(i + 1);
								PxlFrame pf = MTRX.getPF(scoreCounter.icon_pf_key);
								if (pf != null || scoreCounter.score_type == UiMgmTimer.SCORE_TYPE.ICON_SUFFIX)
								{
									stb2.Clear();
									stb3.Clear();
									stb2.Add(X.IntR(scoreCounter.cushion));
									stb3.Add(scoreCounter.score_suffix);
									float num9 = 1f;
									if (scoreCounter.cushion_animating)
									{
										if (num5 == -1f)
										{
											num5 = 0.66f + X.COSIT(5.4f) * 0.33f;
											num6 = 1.5f + X.COSIT(2.48f) * 0.25f;
										}
										this.MdB.Col = this.MdB.ColGrd.White().blend(4294966715U, num5).C;
										this.MdB.ColGrd.Black().blend(4294966715U, num5 * 0.25f);
										num9 = num6;
									}
									else
									{
										this.MdB.Col = MTRX.ColWhite;
										this.MdB.ColGrd.Black();
									}
									if (pf != null)
									{
										this.MdB.RotaPF(this.barx + 4f, num8 + 10f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
									}
									if (scoreCounter.score_type == UiMgmTimer.SCORE_TYPE.ICON_SUFFIX)
									{
										this.MdB.allocUv23(stb2.Length * 4, false);
										float num10 = this.barx + this.bar_w - 12f;
										if (TX.valid(scoreCounter.score_suffix))
										{
											this.MdB.Uv23(MTRX.ColTrnsp, false);
											float num11 = MTRX.ChrM.DrawBorderedScaleStringTo(this.MdB, stb3, num10, num8 + 3f, 1f, num9, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
											num10 -= num11 + 2f;
											this.MdB.allocUv23(0, true);
										}
										this.MdB.Uv23(this.MdB.ColGrd.C, false);
										MTRX.ChrL.DrawScaleStringTo(this.MdB, stb2, num10, num8 + 4f, 1f, num9, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
										this.MdB.allocUv23(0, true);
									}
								}
							}
						}
					}
					this.MdB.allocUv23(0, true);
				}
				this.AMa[3].Set(false);
				return;
			}
			this.AMa[3].revertVerAndTriIndexSaved(true);
		}

		private void fineKadomaruColor()
		{
			this.MdB.chooseSubMesh(0, false, false);
			if (this.main_time == -1001 || this.main_time <= 20)
			{
				Color32 c = this.MdB.ColGrd.Black().blend(4291234828U, 0.5f - 0.5f * X.COSIT(120f)).setA1(0.7f)
					.C;
				this.AMa[0].setColAll(c, false);
			}
		}

		private void fineBarWidth()
		{
			this.MdB.chooseSubMesh(0, false, false);
			float num = (float)(this.count * 60);
			float num2 = 1f / num;
			float num3 = ((this.main_time <= 0) ? 1f : ((float)(this.main_time * 60) - this.t));
			float num4 = (float)((int)(this.bar_w * num3));
			this.AMa[1].revertVerAndTriIndexFirstSaved(false);
			int vertexMax = this.MdB.getVertexMax();
			Vector3[] vertexArray = this.MdB.getVertexArray();
			Color32[] colorArray = this.MdB.getColorArray();
			float num5 = (this.barx + num4) * 0.015625f;
			for (int i = vertexMax + 2; i < vertexMax + 6; i++)
			{
				Vector3 vector = vertexArray[i];
				vector.x = num5;
				vertexArray[i] = vector;
			}
			Color32 color;
			if (num3 < 0.5f)
			{
				color = this.MdB.ColGrd.White().blend(4294967040U, X.ZLINE(num3 - 0.25f, 0.25f)).C;
			}
			else
			{
				color = this.MdB.ColGrd.Set(4294967040U).blend(4294901760U, X.ZPOW(num3 - 0.6f, 0.39999998f)).C;
			}
			for (int j = vertexMax + 4; j < vertexMax + 8; j++)
			{
				colorArray[j] = color;
			}
			this.AMa[1].revertVerAndTriIndexSaved(false);
		}

		private void finePauseBlinkChr()
		{
			this.AMa[2].setAlpha1((!this.pausing) ? 1f : ((X.ANMT(2, 30f) == 1) ? 0.8f : 0.5f), false);
		}

		public void resetScoreEntry(string icon_pf_key, float first_value = 0f, string _score_suffix = "", UiMgmTimer.SCORE_TYPE type = UiMgmTimer.SCORE_TYPE.ICON_SUFFIX)
		{
			bool flag = false;
			this.fine_pos_bits |= this.FPOS_SCORE_CHECK;
			UiMgmTimer.ScoreCounter scoreCounter = new UiMgmTimer.ScoreCounter(type, icon_pf_key, first_value, _score_suffix);
			if (this.AScore != null)
			{
				int num = this.AScore.Length;
				for (int i = 0; i < num; i++)
				{
					UiMgmTimer.ScoreCounter scoreCounter2 = this.AScore[i];
					if (scoreCounter2.Equals(scoreCounter))
					{
						scoreCounter.Tx = scoreCounter2.Tx;
						this.AScore[i] = scoreCounter;
						flag = true;
						break;
					}
					if (!scoreCounter2.valid)
					{
						this.AScore[i] = scoreCounter;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				if (this.AScore == null)
				{
					this.AScore = new UiMgmTimer.ScoreCounter[] { scoreCounter };
				}
				else
				{
					X.push<UiMgmTimer.ScoreCounter>(ref this.AScore, new UiMgmTimer.ScoreCounter(type, icon_pf_key, first_value, _score_suffix), -1);
				}
			}
			this.box_h = 40f + 20f * (float)this.AScore.Length;
		}

		public void addScore(string key, float v)
		{
			if (v == 0f)
			{
				return;
			}
			if (this.AScore != null)
			{
				int num = this.AScore.Length;
				for (int i = 0; i < num; i++)
				{
					UiMgmTimer.ScoreCounter scoreCounter = this.AScore[i];
					if (scoreCounter.icon_pf_key == key)
					{
						bool cushion_animating = scoreCounter.cushion_animating;
						scoreCounter.value += v;
						if (cushion_animating)
						{
							scoreCounter.fineCushionSpeed();
						}
						this.AScore[i] = scoreCounter;
						this.fine_pos_bits |= this.FPOS_SCORE_CHECK;
					}
				}
			}
		}

		public bool getScore(string key, out float v)
		{
			v = 0f;
			if (this.AScore == null)
			{
				return false;
			}
			int num = this.AScore.Length;
			for (int i = 0; i < num; i++)
			{
				UiMgmTimer.ScoreCounter scoreCounter = this.AScore[i];
				if (scoreCounter.icon_pf_key == key)
				{
					v = scoreCounter.value;
					return true;
				}
			}
			return false;
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			return false;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				this.deactivate(false);
				EV.remListener(this);
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public void gotoEventLabelWhenFinished(string label, bool force_set_going = false)
		{
			this.FnDone = delegate(UiMgmTimer _Ui, bool aborted)
			{
				if (!aborted && EV.isActive(false))
				{
					EV.getCurrentEvent().jumpTo(label);
					if (force_set_going)
					{
						EV.forceWriteStateToGoing();
					}
				}
			};
		}

		public bool temp_visible
		{
			get
			{
				return this.move_t >= 0f;
			}
			set
			{
				if (value == this.temp_visible)
				{
					return;
				}
				if (value)
				{
					this.move_t = X.Mx(0f, 30f + this.move_t);
				}
				else
				{
					this.move_t = X.Mn(-1f, -30f + this.move_t);
				}
				if (this.GobTopCD != null)
				{
					this.GobTopCD.SetActive(value);
				}
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			return this.count != -1000 && this.main_time >= -1000 && this.t < 0f;
		}

		public bool isMainGame()
		{
			return this.count != -1000 && this.t >= 0f;
		}

		public bool isGameOver()
		{
			return this.main_time == 0 || this.count == -1000;
		}

		public override string ToString()
		{
			return "MgmTimer";
		}

		public Transform transform
		{
			get
			{
				return this.Gob.transform;
			}
		}

		public readonly NelM2DBase M2D;

		public bool attach_effect = true;

		public const int C_DEACTIVATE = -1000;

		public int count = -1000;

		public const float CUSHION_MAXT = 60f;

		private EffectItem Ef;

		private int time;

		private readonly MeshDrawer MdB;

		public readonly GameObject Gob;

		private MeshDrawer MdTopCD;

		private GameObject GobTopCD;

		private const int MID_BASE = 0;

		private const int MID_CHR = 1;

		private const float KADOMARU_ALPHA = 0.7f;

		private const float box_h0 = 40f;

		private const float H_SCORE_ROW = 20f;

		private float box_h = 40f;

		private float box_w_ = 160f;

		private const float box_rd = 20f;

		private bool need_main_repos;

		private bool pausing;

		private int digit_prog_totalframe;

		private float move_t;

		private const int MOVE_MAXT = 30;

		private const int kadomaru_red_sec = 20;

		private float t;

		private int main_time = -1001;

		private uint draw_main_bits;

		private bool draw_mdtopcd;

		private const int DRAWMAIN_KADOMARU = 1;

		private const int DRAWMAIN_BAR = 2;

		private const int DRAWMAIN_COUNT = 4;

		private const int DRAWMAIN_SCORE = 8;

		private const int DRAWMAIN_END = 16;

		private const int MA_KADOMARU = 0;

		private const int MA_BAR = 1;

		private const int MA_COUNT = 2;

		private const int MA_SCORE = 3;

		private const int MA_END = 4;

		private const int DRAWMAIN_WHOLE = 15;

		private uint fine_pos_bits;

		private uint FPOS_KADOMARU = 1U;

		private uint FPOS_BAR = 2U;

		private uint FPOS_SCORE_CHECK = 4U;

		private uint FPOS_PAUSE_BLINK = 8U;

		private uint FPOS__ALL = 15U;

		private MdArranger[] AMa;

		private UiMgmTimer.ScoreCounter[] AScore;

		public UiMgmTimer.FnTimerDone FnDone;

		private const float FADE_T = 30f;

		private const string md_act_key = "mg_timer";

		public delegate void FnTimerDone(UiMgmTimer Timer, bool aborted = false);

		public enum SCORE_TYPE
		{
			OFFLINE,
			ICON_SUFFIX,
			TEXT_SUFFIX
		}

		public struct ScoreCounter
		{
			public ScoreCounter(UiMgmTimer.SCORE_TYPE _score_type, string _icon_pf_key, float _value, string _score_suffix = "")
			{
				this.score_type = _score_type;
				this.icon_pf_key = _icon_pf_key;
				this.score_suffix = _score_suffix;
				this.cushion = _value;
				this.value = _value;
				this.cushion_d = 0f;
				this.Tx = null;
			}

			public bool cushion_animating
			{
				get
				{
					return this.cushion != this.value;
				}
			}

			public void fineCushionSpeed()
			{
				if (this.value == this.cushion)
				{
					this.cushion_d = 0f;
					return;
				}
				this.cushion_d = X.absMx((this.value - this.cushion) / 60f, 1f);
			}

			public bool Equals(UiMgmTimer.ScoreCounter S)
			{
				return S.icon_pf_key == this.icon_pf_key && S.score_type == this.score_type;
			}

			public bool valid
			{
				get
				{
					return this.score_type > UiMgmTimer.SCORE_TYPE.OFFLINE;
				}
			}

			public string icon_pf_key;

			public string score_suffix;

			public float value;

			public float cushion;

			public float cushion_d;

			public TextRenderer Tx;

			public UiMgmTimer.SCORE_TYPE score_type;
		}
	}
}
