using System;
using System.Collections.Generic;
using evt;
using m2d;
using XX;

namespace nel.mgm.farm
{
	public class M2LpMgmFarm : NelLpRunnerEf, IEventWaitListener, IEventListener, IM2WeedManager
	{
		public M2LpMgmFarm(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			EnemyData.getResources(base.nM2D, "MGMFARM_COW_NPC");
			this.ABelongWeed = new List<M2ManaWeed>(10);
			this.AActiveWeed = new List<M2ManaWeed>(10);
			this.Ui = new UiMgmFarmSuck(this);
			this.Agrade_score = new List<float>(16);
		}

		public override void initActionPre()
		{
			this.ABelongWeed.Clear();
			this.AActiveWeed.Clear();
			base.initActionPre();
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			int i = this.Meta.GetI("cow", 0, 0);
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			this.Ui.initAction();
			M2LpMgmFarm.VarCon.removeVar("farmgame", -1);
			this.AEnCow = new NelNMgmFarmCow[i];
			MvNelNNEAListener.FnCreateNNEA fnCreateNNEA = delegate(MvNelNNEAListener Mv)
			{
				NelNMgmFarmCow nelNMgmFarmCow = Mv.Mp.createMover<NelNMgmFarmCow>("Assigned_" + Mv.name, Mv.x, Mv.y, false, false);
				return this.assignCow(nelNMgmFarmCow);
			};
			for (int j = 0; j < i; j++)
			{
				MvNelNNEAListener mvNelNNEAListener = eventContainer.CreateAndAssignT<MvNelNNEAListener>("mgm_farm_cow_" + j.ToString(), false);
				mvNelNNEAListener.sync_enemy_size = true;
				mvNelNNEAListener.sync_enemy_position_first = true;
				mvNelNNEAListener.prepareCreateNNEA("cow", fnCreateNNEA, true);
			}
			EV.addWaitListener("MGFARM", this);
		}

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAconfig)
		{
			if (this.AWalkDecl != null && this.camdecline_enabled_)
			{
				for (int i = 0; i < 4; i++)
				{
					this.AWalkDecl[i].considerConfig4(_l, _t, _r, _b, AAconfig);
				}
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.stt != M2LpMgmFarm.STATE.OFFLINE)
			{
				this.deactivate();
			}
			this.Ui.closeAction();
			base.nM2D.FlgWarpEventNotInjectable.Rem("MGFARM");
			this.camdecline_enabled = false;
			if (this.AEnCow != null)
			{
				for (int i = this.AEnCow.Length - 1; i >= 0; i--)
				{
					this.AEnCow[i].destruct();
				}
				this.AEnCow = null;
			}
			if (this.AWalkDecl != null)
			{
				for (int j = 0; j < 4; j++)
				{
					this.Mp.M2D.Cam.remCropping(this.ACamDecl[j]);
				}
			}
			if (this.Timer != null)
			{
				this.Timer.destruct();
				this.Timer = null;
			}
			this.AWalkDecl = null;
			this.ACamDecl = null;
		}

		public void assignWeed(M2ManaWeed _Weed)
		{
			this.ABelongWeed.Add(_Weed);
			this.AActiveWeed.Add(_Weed);
			_Weed.charge_time_min = (_Weed.charge_time_max = 1320f);
		}

		public void deassignWeed(M2ManaWeed _Weed)
		{
			this.ABelongWeed.Remove(_Weed);
			this.AActiveWeed.Remove(_Weed);
		}

		public MANA_HIT deassignActiveWeed(M2ManaWeed _Weed, AttackInfo AtkHitExecute = null)
		{
			this.AActiveWeed.Remove(_Weed);
			return MANA_HIT.ALL;
		}

		public void assignActiveWeed(M2ManaWeed _Weed)
		{
			this.AActiveWeed.Add(_Weed);
		}

		private NelNMgmFarmCow assignCow(NelNMgmFarmCow En)
		{
			int num = X.isinC<NelNMgmFarmCow>(this.AEnCow, null);
			if (num < 0)
			{
				En.destruct();
				return null;
			}
			En.BelongTo = this;
			En.cow_index = num;
			En.Weed.Clear();
			string si = this.Meta.GetSi(0, "col_" + num.ToString());
			if (si != null)
			{
				En.CBoard = C32.d2c(X.NmUI(si, 0U, true, true));
			}
			this.AEnCow[num] = En;
			return En;
		}

		public NelNMgmFarmCow removeCow(NelNMgmFarmCow En)
		{
			X.emptySpecific<NelNMgmFarmCow>(this.AEnCow, En, -1);
			return null;
		}

		public override void activate()
		{
			this.positionResetAll();
			if (this.Timer == null)
			{
				this.Timer = new UiMgmTimer(base.nM2D, "-Farm-Timer", 1)
				{
					attach_effect = true
				};
			}
			this.changeState(M2LpMgmFarm.STATE.PREPARE);
		}

		public override void deactivate()
		{
			if (this.stt != M2LpMgmFarm.STATE.TIMEOVER_AFTER)
			{
				this.positionResetAll();
			}
			this.changeState(M2LpMgmFarm.STATE.OFFLINE);
		}

		private void changeState(M2LpMgmFarm.STATE _stt)
		{
			M2LpMgmFarm.STATE state = this.stt;
			if (state == M2LpMgmFarm.STATE.OFFLINE != (_stt == M2LpMgmFarm.STATE.OFFLINE))
			{
				if (_stt == M2LpMgmFarm.STATE.OFFLINE)
				{
					this.Mp.remRunnerObject(this);
					EV.remListener(this);
					if (this.Timer != null)
					{
						this.Timer.deactivate(false);
					}
					base.nM2D.FlgWarpEventNotInjectable.Rem("MGFARM");
					this.camdecline_enabled = false;
					for (int i = this.AEnCow.Length - 1; i >= 0; i--)
					{
						NelNMgmFarmCow nelNMgmFarmCow = this.AEnCow[i];
						if (nelNMgmFarmCow != null)
						{
							nelNMgmFarmCow.activateEvent(false);
						}
					}
					this.FnSortByLen = null;
				}
				else
				{
					this.Mp.addRunnerObject(this);
					EV.addListener(this);
					this.camdecline_enabled = true;
					for (int j = this.AEnCow.Length - 1; j >= 0; j--)
					{
						NelNMgmFarmCow nelNMgmFarmCow2 = this.AEnCow[j];
						if (nelNMgmFarmCow2 != null)
						{
							nelNMgmFarmCow2.activateEvent(true);
						}
					}
					base.nM2D.loadMaterialSnd("minigame");
					base.nM2D.prepareSvTexture("cuts_cow_0", true);
					base.nM2D.prepareSvTexture("cuts_cow_1", true);
					this.resetScore();
					base.nM2D.FlgWarpEventNotInjectable.Add("MGFARM");
				}
				this.Ui.quitSuck();
				this.suck_target = -1000;
			}
			this.stt = _stt;
			switch (this.stt)
			{
			case M2LpMgmFarm.STATE.PREPARE:
				this.Timer.deactivate(false);
				return;
			case M2LpMgmFarm.STATE.COUNTDOWN:
			case M2LpMgmFarm.STATE.COUNTDOWN_AGAIN:
				this.camdecline_enabled = true;
				this.resetScore();
				this.Timer.activate(90, 3);
				EV.initWaitFn(this.Timer, 0);
				return;
			case M2LpMgmFarm.STATE.MAINGAME:
				if (state == M2LpMgmFarm.STATE.COUNTDOWN)
				{
					BGM.replace(0f, -1f, false, false);
				}
				BGM.GotoBlock("A", true);
				BGM.fadein(100f, 5f);
				return;
			case M2LpMgmFarm.STATE.TIMEOVER:
				this.quitSuck(false, false);
				BGM.fadeout(0f, 40f, false);
				return;
			case M2LpMgmFarm.STATE.TIMEOVER_AFTER:
				this.camdecline_enabled = false;
				this.Timer.deactivate(false);
				this.positionResetAll();
				return;
			default:
				return;
			}
		}

		public void resetScore()
		{
			this.score = 0;
			this.Agrade_score.Clear();
			this.grab_count = 0;
			this.cow_angry_count = 0;
			this.grab_atk_count = 0;
			this.suck_target = -1000;
			this.recoverAllWeeds(false);
			this.Ui.resetScore();
			this.Timer.resetScoreEntry("wholemap_milk", 0f, "ml", UiMgmTimer.SCORE_TYPE.ICON_SUFFIX);
		}

		private void recoverAllWeeds(bool immediate = false)
		{
			for (int i = this.ABelongWeed.Count - 1; i >= 0; i--)
			{
				this.ABelongWeed[i].cureImmediate(immediate, 0);
			}
		}

		public bool camdecline_enabled
		{
			get
			{
				return this.camdecline_enabled_;
			}
			set
			{
				if (this.camdecline_enabled == value)
				{
					return;
				}
				this.camdecline_enabled_ = value;
				if (value && this.ACamDecl == null)
				{
					this.ACamDecl = new M2LpCamDecline[4];
					this.AWalkDecl = new M2LpWalkDecline[4];
					for (int i = 0; i < 4; i++)
					{
						this.AWalkDecl[i] = M2LpWalkDecline.createBounds("MGMFARM_WD_", i, this.x, base.y, base.right, base.bottom, 0f, 0f, this.Lay);
						M2LpCamDecline m2LpCamDecline = (this.ACamDecl[i] = M2LpCamDecline.createBounds("MGMFARM_CD_", i, this.x, base.y, base.right, base.bottom, base.CLEN * 2.5f, base.CLEN * 3f));
						this.Mp.M2D.Cam.addCropping(m2LpCamDecline);
					}
				}
				for (int j = 0; j < 4; j++)
				{
					DRect drect = this.AWalkDecl[j];
					this.ACamDecl[j].active = value;
					drect.active = value;
				}
				this.Mp.considerConfig4(0, 0, this.Mp.clms, this.Mp.rows);
				this.Mp.need_update_collider = true;
			}
		}

		public override bool run(float fcnt)
		{
			if ((this.stt == M2LpMgmFarm.STATE.COUNTDOWN || this.stt == M2LpMgmFarm.STATE.COUNTDOWN_AGAIN) && this.Timer.isMainGame())
			{
				this.changeState(M2LpMgmFarm.STATE.MAINGAME);
			}
			if (this.isMainGame() && this.Timer.isGameOver() && !EV.isActive(false))
			{
				SND.Ui.play("minigame_quit", false);
				this.changeState(M2LpMgmFarm.STATE.TIMEOVER);
				EV.stack("___city_farm/_game_timeover", 0, -1, null, null);
			}
			return true;
		}

		public void positionResetAll()
		{
			int num = this.AEnCow.Length;
			for (int i = 0; i < num; i++)
			{
				this.AEnCow[i].positionReset(true, true, true);
			}
			this.recoverAllWeeds(false);
		}

		public int weed_index(M2ManaWeed.WeedLink WL)
		{
			return this.AActiveWeed.IndexOf(WL.Weed);
		}

		public int weed_index(M2ManaWeed Weed)
		{
			return this.AActiveWeed.IndexOf(Weed);
		}

		public M2ManaWeed getTargetFromCow(NelNMgmFarmCow TargetCow, float alloc_len, bool alloc_soft_link = true, bool alloc_hard_link = false)
		{
			M2ManaWeed m2ManaWeed2;
			using (BList<int> blist = ListBuffer<int>.Pop(0))
			{
				using (BList<int> blist2 = ListBuffer<int>.Pop(0))
				{
					int count = this.AActiveWeed.Count;
					M2LpMgmFarm.target_cow_cx = TargetCow.x;
					if (this.FnSortByLen == null)
					{
						this.FnSortByLen = delegate(int a, int b)
						{
							M2ManaWeed m2ManaWeed3 = this.AActiveWeed[a];
							M2ManaWeed m2ManaWeed4 = this.AActiveWeed[b];
							float num4 = X.Abs(m2ManaWeed3.mapcx - M2LpMgmFarm.target_cow_cx);
							float num5 = X.Abs(m2ManaWeed4.mapcy - M2LpMgmFarm.target_cow_cx);
							if (num4 == num5)
							{
								return 0;
							}
							if (num4 >= num5)
							{
								return 1;
							}
							return -1;
						};
					}
					for (int i = 0; i < count; i++)
					{
						M2ManaWeed m2ManaWeed = this.AActiveWeed[i];
						if (X.Abs(TargetCow.x - m2ManaWeed.mapcx) <= alloc_len)
						{
							blist.Add(i);
						}
					}
					blist.Sort(this.FnSortByLen);
					int num = this.AEnCow.Length;
					using (BList<int> blist3 = ListBuffer<int>.Pop(0))
					{
						for (int j = 0; j < 2; j++)
						{
							blist3.Clear();
							for (int k = 0; k < num; k++)
							{
								NelNMgmFarmCow nelNMgmFarmCow = this.AEnCow[k];
								if (!(nelNMgmFarmCow == TargetCow) && !(nelNMgmFarmCow.Weed.Weed == null) && X.Abs(TargetCow.x - nelNMgmFarmCow.Weed.Weed.mapcx) <= alloc_len && nelNMgmFarmCow.Weed.valid(this.Mp, null) && nelNMgmFarmCow.Weed.hard_link == (j == 1))
								{
									int num2 = this.weed_index(nelNMgmFarmCow.Weed);
									int num3 = blist.IndexOf(num2);
									if (num3 != -1)
									{
										blist3.Add(num2);
										blist.RemoveAt(num3);
									}
								}
							}
							if (!((j == 0) ? alloc_soft_link : alloc_hard_link))
							{
								blist3.Sort(this.FnSortByLen);
								blist2.AddRange(blist3);
							}
						}
					}
					blist.AddRange(blist2);
				}
				if (blist.Count == 0)
				{
					m2ManaWeed2 = null;
				}
				else
				{
					m2ManaWeed2 = this.AActiveWeed[blist[0]];
				}
			}
			return m2ManaWeed2;
		}

		public bool is_hard_linked(M2ManaWeed Weed, NelNMgmFarmCow TargetCow = null)
		{
			int num = this.AEnCow.Length;
			for (int i = 0; i < num; i++)
			{
				NelNMgmFarmCow nelNMgmFarmCow = this.AEnCow[i];
				if (!(nelNMgmFarmCow == TargetCow) && nelNMgmFarmCow.Weed.hard_link && nelNMgmFarmCow.Weed.valid(this.Mp, Weed))
				{
					return true;
				}
			}
			return false;
		}

		public bool EvtWait(bool is_first = false)
		{
			return !this.isTimeOverState() && (is_first || (this.AEnCow != null && X.isinC<NelNMgmFarmCow>(this.AEnCow, null) >= 0) || (this.Mp.Pr is PR && (this.Mp.Pr as PR).isSinkState()));
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd != "MGFARM")
			{
				return false;
			}
			string _ = rER._1;
			if (_ != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(_);
				if (num <= 2076447031U)
				{
					if (num <= 1023944928U)
					{
						if (num != 103559853U)
						{
							if (num != 244914798U)
							{
								if (num == 1023944928U)
								{
									if (_ == "UPDATE_RESULT")
									{
										float num2;
										if (ER.clength >= 3)
										{
											num2 = ER.Nm(2, 0f);
										}
										else
										{
											this.Timer.getScore("wholemap_milk", out num2);
										}
										bool flag = COOK.Mgm.UpdateScore(MGMSCORE.FARM, (int)num2, 0);
										UiMgmScore.createInstance().InitAftGame(flag, MGMSCORE.FARM).CurScore((int)num2);
									}
								}
							}
							else if (_ == "DEFINE_RESULT")
							{
								float num2;
								this.Timer.getScore("wholemap_milk", out num2);
								EV.getVariableContainer().define("_score", num2.ToString(), true);
								EV.getVariableContainer().define("_grade", (4 - X.MMX(0, X.IntR(X.sum(this.Agrade_score) / (float)this.Agrade_score.Count * 4f), 4)).ToString(), true);
								EV.getVariableContainer().define("_cow_angry", (this.grab_count >= 3 || this.grab_atk_count >= 26) ? "1" : "0", true);
								EV.getVariableContainer().define("_cow_angry_count", this.cow_angry_count.ToString(), true);
							}
						}
						else if (_ == "DEFINE_GRAB_COUNT")
						{
							EV.getVariableContainer().define(rER._2, this.grab_count.ToString(), true);
						}
					}
					else if (num != 1063120797U)
					{
						if (num != 1994269220U)
						{
							if (num == 2076447031U)
							{
								if (_ == "INIT_COUNTDOWN")
								{
									this.changeState((this.stt >= M2LpMgmFarm.STATE.TIMEOVER) ? M2LpMgmFarm.STATE.COUNTDOWN_AGAIN : M2LpMgmFarm.STATE.COUNTDOWN);
								}
							}
						}
						else if (_ == "SUCKER_GRAWL")
						{
							NelNMgmFarmCow suckTarget = this.getSuckTarget();
							if (suckTarget != null)
							{
								PR pr = this.Mp.Pr as PR;
								if (pr != null && suckTarget.isCoveringMv(pr, 0f, 0f))
								{
									pr.addEnemySink(300f, true, 0f);
								}
							}
						}
					}
					else if (_ == "INITSUCK")
					{
						this.defineIsInGame();
						this.Ui.initSuck(this.getSuckTarget());
					}
				}
				else
				{
					if (num <= 2365517667U)
					{
						if (num != 2147733226U)
						{
							if (num != 2212356747U)
							{
								if (num != 2365517667U)
								{
									return true;
								}
								if (!(_ == "TIMEOVER_AFTER"))
								{
									return true;
								}
								this.changeState(M2LpMgmFarm.STATE.TIMEOVER_AFTER);
								return true;
							}
							else
							{
								if (!(_ == "INIT_MAINGAME"))
								{
									return true;
								}
								return true;
							}
						}
						else if (!(_ == "TALK2COW"))
						{
							return true;
						}
					}
					else if (num <= 3372049273U)
					{
						if (num != 2519767236U)
						{
							if (num != 3372049273U)
							{
								return true;
							}
							if (!(_ == "TIMER_ACTIVE"))
							{
								return true;
							}
							if (this.Timer != null)
							{
								this.Timer.temp_visible = rER.Nm(1, 1f) != 0f;
								return true;
							}
							return true;
						}
						else
						{
							if (!(_ == "QUITSUCK"))
							{
								return true;
							}
							this.quitSuck(true, rER._B2);
							return true;
						}
					}
					else if (num != 3441525422U)
					{
						if (num != 3562214871U)
						{
							return true;
						}
						if (!(_ == "CROP"))
						{
							return true;
						}
						this.camdecline_enabled = rER.Nm(1, 1f) != 0f;
						return true;
					}
					else if (!(_ == "PREPARE_TALK2COW"))
					{
						return true;
					}
					int num3 = rER.Int(2, 0);
					if (X.BTW(0f, (float)num3, (float)this.AEnCow.Length))
					{
						bool flag2;
						if (rER._1 == "PREPARE_TALK2COW")
						{
							flag2 = this.AEnCow[num3].prepareTalkEvent(false);
						}
						else
						{
							flag2 = this.AEnCow[num3].triggeredTalkEvent();
							if (flag2)
							{
								this.suck_target = num3;
							}
						}
						M2LpMgmFarm.VarCon.define("_result", flag2 ? "1" : "0", true);
						this.defineIsInGame();
					}
				}
			}
			return true;
		}

		internal void quitSuck(bool from_event = false, bool success = false)
		{
			int num = this.Ui.quitSuck();
			if (num > 0)
			{
				this.Timer.addScore("wholemap_milk", (float)num);
			}
			NelNMgmFarmCow nelNMgmFarmCow = null;
			if (this.suck_target >= 0)
			{
				nelNMgmFarmCow = this.getSuckTarget();
				this.suck_target = -1 - this.suck_target;
				nelNMgmFarmCow.quitSuck(from_event, ref success);
			}
			PR pr = this.Mp.Pr as PR;
			if (pr != null)
			{
				if (pr != null && !success && nelNMgmFarmCow != null && nelNMgmFarmCow.isCoveringMv(pr, 0f, 0f))
				{
					pr.addEnemySink(300f, true, 0f);
					return;
				}
				if (pr.poseIs("collect"))
				{
					pr.setAim((pr.mpf_is_right > 0f) ? AIM.R : AIM.L, false);
					pr.SpSetPose("crouch2stand", -1, null, false);
				}
			}
		}

		public void addTotalScore(int v, float drop_level)
		{
			this.Agrade_score.Add(drop_level);
		}

		public bool isSuckTarget(NelNMgmFarmCow Cow)
		{
			return Cow.cow_index == this.suck_target;
		}

		public NelNMgmFarmCow getSuckTarget()
		{
			if (this.suck_target == -1000)
			{
				return null;
			}
			if (this.suck_target < 0)
			{
				return this.AEnCow[-this.suck_target - 1];
			}
			return this.AEnCow[this.suck_target];
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end && this.suck_target >= 0)
			{
				this.suck_target = -1 - this.suck_target;
			}
			return true;
		}

		public bool isCountOver()
		{
			return this.Timer.isGameOver();
		}

		public void defineIsInGame()
		{
			M2LpMgmFarm.VarCon.define("_in_game", (this.isMainGame() && !this.isCountOver()) ? "1" : "0", true);
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public static CsvVariableContainer VarCon
		{
			get
			{
				return EV.getVariableContainer();
			}
		}

		public bool isMainGame()
		{
			return this.stt >= M2LpMgmFarm.STATE.MAINGAME && this.stt < M2LpMgmFarm.STATE.TIMEOVER;
		}

		public bool isTimeOverState()
		{
			return this.stt >= M2LpMgmFarm.STATE.TIMEOVER;
		}

		public bool canFindWeed()
		{
			return this.isMainGame();
		}

		public int cow_count
		{
			get
			{
				return this.AEnCow.Length;
			}
		}

		public bool cowCanMove()
		{
			return this.stt != M2LpMgmFarm.STATE.PREPARE;
		}

		public override string ToString()
		{
			return "LpMgmFarm";
		}

		bool IM2WeedManager.get_destructed()
		{
			return base.destructed;
		}

		public const string lp_name = "MGFarm";

		private const ENEMYID enemyid = ENEMYID.MGMFARM_COW_NPC;

		private const string enemyid_key = "MGMFARM_COW_NPC";

		private const string quit_event_name = "___city_farm/_game_timeover";

		public const string absorb_event_name = "___city_farm/_game_absorb";

		public const string suck_event_name = "___city_farm/_game_cow_suck";

		private const string score_icon_pf = "wholemap_milk";

		public const int MILK_MAX_DROP0 = 120;

		public const int MILK_MAX_DROP1 = 250;

		public const int MILK_MAX_DROP1_OVER = 180;

		public const float MILK_RAND_ADD_SCORE = 0.8f;

		public const int WEED_CHARGE_TIME = 1320;

		private const int game_duration = 90;

		private NelNMgmFarmCow[] AEnCow;

		public float t_state;

		private M2LpMgmFarm.STATE stt;

		public UiMgmTimer Timer;

		private bool camdecline_enabled_;

		private M2LpCamDecline[] ACamDecl;

		private M2LpWalkDecline[] AWalkDecl;

		private readonly List<float> Agrade_score;

		public List<M2ManaWeed> ABelongWeed;

		public List<M2ManaWeed> AActiveWeed;

		public readonly UiMgmFarmSuck Ui;

		public int suck_target = -1000;

		public int score;

		public int grab_count;

		public int cow_angry_count;

		public int grab_atk_count;

		public Comparison<int> FnSortByLen;

		public static float target_cow_cx;

		private enum STATE
		{
			OFFLINE,
			PREPARE,
			COUNTDOWN,
			COUNTDOWN_AGAIN,
			MAINGAME,
			TIMEOVER,
			TIMEOVER_AFTER
		}
	}
}
