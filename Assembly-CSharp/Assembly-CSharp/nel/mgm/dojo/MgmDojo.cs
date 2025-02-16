using System;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class MgmDojo : IEventListener, IEventWaitListener, IRunAndDestroy
	{
		public static int EvtCacheReadS(EvReader ER, string cmd, CsvReader rER)
		{
			if (rER._1 == "CREATE")
			{
				if (M2DBase.Instance != null)
				{
					M2DBase.Instance.loadMaterialPxl("mg_dojo", "EvImg/mg_dojo.pxls", false, true, false);
				}
				else
				{
					MTRX.loadMtiPxc("mg_dojo", "EvImg/mg_dojo.pxls", "MgmDojo", false, true, false);
				}
			}
			return 0;
		}

		public static bool EvtReadS(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string _ = rER._1;
			if (_ != null && _ == "CREATE")
			{
				new MgmDojo();
				return true;
			}
			return false;
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd == "MG_DOJO")
			{
				string _ = rER._1;
				if (_ != null)
				{
					uint num = <PrivateImplementationDetails>.ComputeStringHash(_);
					if (num <= 1623056740U)
					{
						if (num <= 1061728185U)
						{
							if (num <= 541268853U)
							{
								if (num != 159804867U)
								{
									if (num == 541268853U)
									{
										if (_ == "LEARN")
										{
											if (this.GM.isActive())
											{
												this.learnSkill();
											}
											return true;
										}
									}
								}
								else if (_ == "DEBUG_SET")
								{
									return true;
								}
							}
							else if (num != 886205846U)
							{
								if (num == 1061728185U)
								{
									if (_ == "CURE_CRACK")
									{
										if (this.stt == MgmDojo.STATE.GAME)
										{
											this.GM.LifePr.cureCrack(true);
										}
										return true;
									}
								}
							}
							else if (_ == "TUTO_SHOW_ENEMY_RPC")
							{
								this.GM.tutoShowEnemyRpc();
								return true;
							}
						}
						else if (num <= 1134567034U)
						{
							if (num != 1066503673U)
							{
								if (num == 1134567034U)
								{
									if (_ == "MENU")
									{
										this.changeState(MgmDojo.STATE.MENU);
										this.waitstt = MgmDojo.EWAIT.MENU;
										EV.initWaitFn(this, 0);
										return true;
									}
								}
							}
							else if (_ == "DIGEST_PR_CRACK")
							{
								this.GM.digestPrCrack();
								return true;
							}
						}
						else if (num != 1311330316U)
						{
							if (num == 1623056740U)
							{
								if (_ == "G_PROGRESS")
								{
									EV.getMessageContainer().setHandle(false);
									this.GM.gameProgressFromEvent();
									return true;
								}
							}
						}
						else if (_ == "CLOSEG")
						{
							if (this.GM.isActive())
							{
								this.GM.deactivate();
							}
							return true;
						}
					}
					else if (num <= 2489361939U)
					{
						if (num <= 2409015043U)
						{
							if (num != 1985538524U)
							{
								if (num == 2409015043U)
								{
									if (_ == "CLOSE")
									{
										this.EvtClose(true);
										return true;
									}
								}
							}
							else if (_ == "INITG")
							{
								this.changeState(MgmDojo.STATE.GAME);
								this.GM.initG(rER._2);
								this.UiCon.initG(rER._2);
								return true;
							}
						}
						else if (num != 2445859592U)
						{
							if (num == 2489361939U)
							{
								if (_ == "CLOSE_CUTIN")
								{
									this.GM.Cutin.deactivate();
									return true;
								}
							}
						}
						else if (_ == "RESHOW_FIG")
						{
							this.GM.finePrFigureImage();
							if (this.GM.isActive())
							{
								for (int i = 0; i < 2; i++)
								{
									this.APr[i].closeHiding();
								}
							}
							return true;
						}
					}
					else if (num <= 3572553947U)
					{
						if (num != 3074754201U)
						{
							if (num == 3572553947U)
							{
								if (_ == "BACK_FROM_LOSE_STATE")
								{
									if (this.stt == MgmDojo.STATE.GAME && !this.GM.is_tuto)
									{
										this.GM.backFromLoseState();
									}
									return true;
								}
							}
						}
						else if (_ == "RESHOW_RPC")
						{
							if (this.GM.isActive() && !this.GM.is_tuto)
							{
								this.GM.reshowRpc();
							}
							return true;
						}
					}
					else if (num != 3580723842U)
					{
						if (num == 3752427093U)
						{
							if (_ == "HIDE_RPC")
							{
								if (this.GM.isActive())
								{
									for (int j = 0; j < 2; j++)
									{
										this.GM.ARpc[j].deactivate();
									}
								}
								return true;
							}
						}
					}
					else if (_ == "WAIT_FOR_GAME_BGM")
					{
						if (this.stt == MgmDojo.STATE.GAME)
						{
							this.initWait(MgmDojo.EWAIT.GAME_BGM_INIT);
						}
						return true;
					}
				}
				return true;
			}
			return false;
		}

		public MgmDojo()
		{
			MTR.prepareShaderInstance();
			this.NM2D = M2DBase.Instance as NelM2DBase;
			this.Pxc = PxlsLoader.getPxlCharacter("mg_dojo");
			this.waitstt = MgmDojo.EWAIT.LOAD;
			EV.addListener(this);
			EV.addWaitListener("MG_DOJO", this);
			IN.addRunner(this);
			BGM.addUseBeatFlag("MG_DOJO");
			this.FD_BeatCheck = new BGM.FnBeatAttack(this.BeatCheck);
			BGM.EvBeatCallBack += this.FD_BeatCheck;
			this.Gob = IN.CreateGobGUI(null, "mg_dojo");
			this.MMRD = this.Gob.AddComponent<MultiMeshRenderer>();
			IN.setZ(this.Gob.transform, -3.4f);
			this.MdBg = this.MMRD.Make(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			this.MdFig = this.MMRD.Make(MTRX.MtrMeshNormal);
			IN.setZAbs(this.MMRD.GetGob(this.MdFig).transform, -3.55f);
			this.APr = new DjFigure[]
			{
				new DjFigure(this, false),
				new DjFigure(this, true)
			};
			this.Indg = new DjIndicator();
			this.UiCon = this.MMRD.gameObject.AddComponent<DjUI>();
			this.UiCon.DJ = this;
			this.GM = new DjGM(this);
			IN.setZAbs(EV.TutoBox.getTransform(), -3.59f);
			BGM.fadeout(0f, 140f, false);
			SND.loadSheets("ev_grazia_dojo", "MG_DOJO");
			SND.loadSheets("ev_city", "MG_DOJO");
			this.Indg.prepareSound(this, "BGM_dojogame0");
			BGM.load("BGM_dojogame0", "BGM_dojogame0", false);
			EV.initWaitFn(this, 0);
		}

		private void prepareMdMaterial()
		{
			this.MI = MTRX.getMI(this.Pxc, false);
			Material mtr = this.MI.getMtr(BLEND.NORMAL, -1);
			this.MMRD.setMaterial(this.MdFig, mtr, false);
			this.Indg.prepareMaterial(this);
		}

		public void destruct()
		{
			EV.remListener(this);
			EV.remWaitListener(this);
			BGM.EvBeatCallBack -= this.FD_BeatCheck;
			BGM.addUseBeatFlag("MG_DOJO");
			IN.DestroyE(this.MMRD.gameObject);
			this.Indg.destruct();
			this.GM.destruct();
			this.UiCon.destruct();
			IN.DestroyE(this.UiCon.gameObject);
			if (this.NM2D != null)
			{
				this.NM2D.FlagValotStabilize.Rem("MG_DOJO");
			}
			for (int i = 0; i < 2; i++)
			{
				this.APr[i].destruct();
			}
			for (int j = 0; j < 3; j++)
			{
				SND.unloadSheets("BGM_dojogame" + j.ToString(), "MG_DOJO");
			}
			SND.unloadSheets("ev_grazia_dojo", "MG_DOJO");
			SND.unloadSheets("ev_city", "MG_DOJO");
		}

		public bool run(float fcnt)
		{
			fcnt *= IN.deltaFrame;
			bool flag = false;
			switch (this.stt)
			{
			case MgmDojo.STATE.LOAD_PXC:
				flag = true;
				this.GM.HK.loadProgress();
				if (this.Pxc.isLoadCompleted())
				{
					this.prepareMdMaterial();
					this.changeState(MgmDojo.STATE.INTRO);
					for (int i = 0; i < 2; i++)
					{
						this.APr[i].prepareBlur(this.MdFig);
					}
					this.MdFig.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdFig));
					if (this.NM2D != null)
					{
						this.NM2D.FlagValotStabilize.Add("MG_DOJO");
					}
				}
				break;
			case MgmDojo.STATE.DEACTIVATING:
				this.need_redraw = true;
				this.need_redraw_bg = true;
				if (this.t_state >= 40f)
				{
					this.destruct();
					return false;
				}
				break;
			case MgmDojo.STATE.INTRO:
				this.need_redraw_bg = true;
				this.need_redraw = true;
				if (this.GM.HK.loadProgress() && this.t_state >= 150f)
				{
					this.changeState(MgmDojo.STATE.PREPARE);
					this.initBgm(0, false, true);
				}
				break;
			case MgmDojo.STATE.GAME:
				if (!this.GM.isActive())
				{
					this.changeState(MgmDojo.STATE.PREPARE);
				}
				break;
			}
			this.t_state += fcnt;
			if (this.tz_beat > 0f)
			{
				this.tz_beat = X.Mx(0f, this.tz_beat - X.Abs(this.bpm_r * 2.5f));
				this.need_redraw = true;
			}
			this.GM.run(fcnt);
			if (X.D)
			{
				for (int j = 0; j < 2; j++)
				{
					this.need_redraw = this.APr[j].run(fcnt * (float)X.AF) || this.need_redraw;
				}
				this.need_redraw_bg = this.Indg.run(this.bpm_r > 0f) || this.need_redraw_bg;
				if (!flag)
				{
					if (this.need_redraw_bg)
					{
						this.need_redraw_bg = false;
						this.redrawBg();
					}
					if (this.need_redraw)
					{
						this.need_redraw = false;
						this.redraw();
					}
				}
			}
			return true;
		}

		private void changeState(MgmDojo.STATE _stt)
		{
			IN.clearPushDown(true);
			MgmDojo.STATE state = this.stt;
			if (state != MgmDojo.STATE.MENU)
			{
				if (state == MgmDojo.STATE.GAME)
				{
					if (this.GM.isActive())
					{
						this.GM.deactivate();
					}
				}
			}
			else
			{
				this.UiCon.deactivate(false);
			}
			this.stt = _stt;
			this.t_state = 0f;
			MgmDojo.STATE state2 = this.stt;
			if (state2 != MgmDojo.STATE.DEACTIVATING)
			{
				if (state2 == MgmDojo.STATE.MENU)
				{
					this.UiCon.activateMenu();
					return;
				}
			}
			else
			{
				EV.TutoBox.local_z = EV.TutoBox.local_z;
			}
		}

		internal void initWait(MgmDojo.EWAIT _waitstt)
		{
			this.waitstt = _waitstt;
			EV.initWaitFn(this, 0);
		}

		public bool EvtWait(bool is_first = false)
		{
			if (is_first)
			{
				return true;
			}
			switch (this.waitstt)
			{
			case MgmDojo.EWAIT.LOAD:
				if (this.stt <= MgmDojo.STATE.INTRO)
				{
					return true;
				}
				break;
			case MgmDojo.EWAIT.MENU:
				if (this.stt == MgmDojo.STATE.MENU && this.UiCon.result == null)
				{
					return true;
				}
				if (this.UiCon.result != null)
				{
					EV.getVariableContainer().define("_result", this.UiCon.result, true);
				}
				break;
			case MgmDojo.EWAIT.GAME_PLAYING:
				if (this.stt == MgmDojo.STATE.GAME && this.GM.waitHold(this.waitstt))
				{
					return true;
				}
				EV.getMessageContainer().setHandle(true);
				break;
			case MgmDojo.EWAIT.GAME_BGM_INIT:
				if (this.stt == MgmDojo.STATE.GAME && this.GM.waitHold(this.waitstt))
				{
					return true;
				}
				break;
			case MgmDojo.EWAIT.LEARN:
				if (this.NM2D != null)
				{
					ItemDescBox descBox = this.NM2D.IMNG.get_DescBox();
					descBox.lock_input_focus = false;
					if (descBox.EvtWait(is_first))
					{
						return true;
					}
				}
				break;
			}
			this.waitstt = MgmDojo.EWAIT.NONE;
			return false;
		}

		public void initBgm(int id, bool load_bgm = true, bool force = true)
		{
			if (!force && id == this.cur_bgm_id)
			{
				return;
			}
			if (id < 0)
			{
				this.cur_bgm_id = id;
				if (load_bgm)
				{
					this.Indg.deattachSound();
				}
				this.bpm_r = -X.Abs(this.bpm_r);
				BGM.fadeout(0f, 30f, false);
				return;
			}
			string text = ((id == 0) ? "BGM_dojogame0" : ((id == 1) ? "BGM_dojogame1" : "BGM_dojogame2"));
			float bpm = BgmKind.GetFromSheet(text).bpm;
			this.bpm_r = bpm / 3600f;
			if (load_bgm)
			{
				this.Indg.prepareSound(this, text);
				SND.loadSheets(text, "MG_DOJO");
				BGM.load(text, null, false);
			}
			BGM.replace(0f, 0f, false, true);
			this.cur_bgm_id = id;
		}

		public void initBgmBlock(bool block_b)
		{
			if (this.cur_bgm_id >= 0)
			{
				BGM.setOverrideKey(block_b ? "toB" : "_", false);
			}
		}

		public void BeatCheck(int count)
		{
			if (this.bpm_r <= 0f)
			{
				return;
			}
			this.tz_beat = 1f;
			this.GM.BeatAttacked(false);
		}

		private void redrawBg()
		{
			float num = 1f;
			this.MdBg.clear(false, false);
			if (this.stt == MgmDojo.STATE.INTRO)
			{
				num *= X.ZLINE(this.t_state - 30f, 90f);
				float num2 = 1f - X.ZLINE(this.t_state, 120f);
				float num3 = X.NI(1f, 1.5f, X.ZPOW(num2));
				this.MdBg.Scale(num3, num3, false);
			}
			if (this.stt == MgmDojo.STATE.DEACTIVATING)
			{
				num *= 1f - X.ZLINE(this.t_state, 40f);
			}
			if (num > 0f)
			{
				this.Indg.drawTo(this.MdBg, num);
			}
			this.MdBg.updateForMeshRenderer(false);
		}

		private void redraw()
		{
			this.MdFig.clear(false, false);
			this.MdFig.Identity();
			float num = 0f;
			float num2 = 1f;
			if (this.stt == MgmDojo.STATE.INTRO)
			{
				num = 1f - X.ZLINE(this.t_state, 120f);
				float num3 = X.NI(1f, 2.5f, X.ZPOW(num));
				this.MdFig.Scale(num3, num3, false);
			}
			if (this.stt == MgmDojo.STATE.DEACTIVATING)
			{
				num2 = 1f - X.ZLINE(this.t_state, 40f);
			}
			float num4 = X.ZPOW(this.tz_beat);
			for (int i = 0; i < 2; i++)
			{
				this.APr[i].drawTo(this.MdFig, num, num4, num2);
			}
			this.MdFig.updateForMeshRenderer(false);
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end && this.stt != MgmDojo.STATE.DEACTIVATING)
			{
				EV.remListener(this);
				EV.remWaitListener(this);
				this.changeState(MgmDojo.STATE.DEACTIVATING);
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

		public void learnSkill()
		{
			string skill_key = this.GM.get_skill_key();
			PrSkill prSkill = SkillManager.Get(skill_key);
			if (prSkill != null && !prSkill.visible)
			{
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				if (nelM2DBase != null)
				{
					prSkill.Obtain(false);
					nelM2DBase.IMNG.get_DescBox().addTaskFocus(prSkill, true);
					this.changeState(MgmDojo.STATE.PREPARE);
					this.initWait(MgmDojo.EWAIT.LEARN);
				}
				else
				{
					prSkill.visible = true;
				}
				this.UiCon.need_create_skill_rows = true;
			}
			DjSaveData.SD data = COOK.Mgm.Dojo.GetData(skill_key);
			data.win_count += 1;
			data.minimum_miss = (byte)X.Mn((int)data.minimum_miss, this.GM.LifePr.get_miss_count());
			COOK.Mgm.Dojo.WriteData(skill_key, data);
		}

		public int beat4
		{
			get
			{
				return BGM.beatcount % 4;
			}
		}

		public bool bgm_playing
		{
			get
			{
				return this.cur_bgm_id >= 0;
			}
		}

		public override string ToString()
		{
			return "MgmDojo";
		}

		private const string pxl_key = "mg_dojo";

		public readonly PxlCharacter Pxc;

		private MgmDojo.EWAIT waitstt;

		private MgmDojo.STATE stt;

		private readonly NelM2DBase NM2D;

		internal float t_state;

		public bool need_redraw;

		private bool need_redraw_bg;

		public const int COST_NO_LEARN_FIRST = 300;

		public const int COST_NO_LEARN_SECOND = 15;

		public const int COST_LEARN = 5;

		internal readonly MultiMeshRenderer MMRD;

		internal readonly MeshDrawer MdBg;

		internal readonly MeshDrawer MdFig;

		internal MImage MI;

		private const float MAXT_DEACTIVATE = 40f;

		private const float MAXT_INTRO = 150f;

		private const float MAXT_INTRO_ANM = 120f;

		private BGM.FnBeatAttack FD_BeatCheck;

		public float tz_beat;

		public float bpm_r;

		private int cur_bgm_id = -1;

		internal readonly DjFigure[] APr;

		internal readonly DjIndicator Indg;

		public const float Z_BASE = -3.4f;

		public const float Z_FIG = -3.55f;

		public const float Z_EFT = -3.6499999f;

		private readonly DjUI UiCon;

		public readonly DjGM GM;

		public const string tutorial_key = "_tuto";

		public readonly GameObject Gob;

		internal enum EWAIT
		{
			NONE,
			LOAD,
			MENU,
			GAME_PLAYING,
			GAME_BGM_INIT,
			LEARN
		}

		private enum STATE
		{
			LOAD_PXC,
			DEACTIVATING,
			INTRO,
			PREPARE,
			MENU,
			GAME
		}
	}
}
