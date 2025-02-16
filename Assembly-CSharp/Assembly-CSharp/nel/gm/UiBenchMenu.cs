using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel.gm
{
	public sealed class UiBenchMenu
	{
		public static bool gameovered
		{
			get
			{
				return (UiBenchMenu.several_flags & 1) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 1) : (UiBenchMenu.several_flags & -2));
			}
		}

		public static bool event_defined
		{
			get
			{
				return (UiBenchMenu.several_flags & 2) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 2) : (UiBenchMenu.several_flags & -3));
			}
		}

		public static bool need_save
		{
			get
			{
				return (UiBenchMenu.several_flags & 4) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 4) : (UiBenchMenu.several_flags & -5));
			}
		}

		public static bool enemy_orgasm
		{
			get
			{
				return (UiBenchMenu.several_flags & 8) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 8) : (UiBenchMenu.several_flags & -9));
			}
		}

		public static bool auto_start_stack_disable
		{
			get
			{
				return (UiBenchMenu.several_flags & 16) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 16) : (UiBenchMenu.several_flags & -17));
			}
		}

		public static bool orgasm_onemore
		{
			get
			{
				return (UiBenchMenu.several_flags & 32) > 0;
			}
			set
			{
				UiBenchMenu.several_flags = (value ? (UiBenchMenu.several_flags | 32) : (UiBenchMenu.several_flags & -33));
			}
		}

		public static void initBenchMenu()
		{
			UiBenchMenu.ACmd = new UiBenchMenu.BenchCmd[9];
			UiBenchMenu.ACmd[0] = new UiBenchMenu.BenchCmd("cure_hp", (PR Pr) => Pr.get_hp() < Pr.get_maxhp(), true, false);
			UiBenchMenu.ACmd[1] = new UiBenchMenu.BenchCmd("cure_cloth", (PR Pr) => Pr.BetoMng.is_torned || Pr.BetoMng.isActive(), true, false);
			UiBenchMenu.ACmd[2] = new UiBenchMenu.BenchCmd("cure_mp", (PR Pr) => Pr.get_mp() < Pr.get_maxmp() || Pr.GaugeBrk.isActive(), true, true);
			UiBenchMenu.ACmd[3] = new UiBenchMenu.BenchCmd("cure_ep", (PR Pr) => (float)Pr.ep >= 400f, false, false);
			UiBenchMenu.ACmd[4] = new UiBenchMenu.BenchCmd("cure_egged", (PR Pr) => Pr.NM2D.IMNG.getInventoryPrecious().getCount(NelItem.GetById("precious_egg_remover", false), -1) > 0 && ((float)Pr.ep >= 400f || Pr.EggCon.total > 0) && !Pr.NM2D.isSafeArea(), false, false);
			UiBenchMenu.ACmd[5] = new UiBenchMenu.BenchCmd("wait_nightingale", delegate(PR Pr)
			{
				WanderingNPC nightingale = Pr.NM2D.WDR.getNightingale();
				return nightingale.alreadyMeet() && nightingale.isEnable() && !nightingale.isHere(Pr.Mp);
			}, false, false);
			UiBenchMenu.ACmd[6] = new UiBenchMenu.BenchCmd("fast_travel", null, false, false);
			UiBenchMenu.ACmd[7] = new UiBenchMenu.BenchCmd("fast_travel_home", null, false, false);
			UiBenchMenu.ACmd[8] = new UiBenchMenu.BenchCmd("save", null, false, false);
			UiBenchMenu.CmdBenchPee = new UiBenchMenu.BenchCmd("pee", (PR Pr) => Pr.Ser.has(SER.NEAR_PEE) && !Pr.NM2D.isSafeArea() && X.sensitive_level == 0, false, false);
			UiBenchMenu.newGame();
		}

		public static void fineBenchCmdEnable()
		{
			int num = UiBenchMenu.ACmd.Length;
			for (int i = 0; i < num; i++)
			{
				UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[i];
				benchCmd.scn_enable = SCN.isBenchCmdEnable(benchCmd.key);
			}
		}

		public static string getCommandListTitle()
		{
			string text = "";
			int num = UiBenchMenu.ACmd.Length;
			for (int i = 0; i < num; i++)
			{
				string key = UiBenchMenu.ACmd[i].key;
				if (!(key == "wait_nightingale"))
				{
					text = TX.add(text, "- " + TX.Get("Bench_Cmd_" + key, ""), "\n");
				}
			}
			return text;
		}

		public static void newGame()
		{
			int num = UiBenchMenu.ACmd.Length;
			for (int i = 0; i < num; i++)
			{
				UiBenchMenu.ACmd[i].newGame();
			}
			UiBenchMenu.CmdBenchPee.newGame();
			UiBenchMenu.several_flags = 0;
			UiBenchMenu.auto_start_stack_disable = false;
			UiBenchMenu.auto_start_temp_disable = false;
			UiBenchMenu.EventCmd = null;
			UiBenchMenu.Adebug_keys = null;
		}

		public static void readBinaryFrom(ByteReader Ba)
		{
			int num = UiBenchMenu.ACmd.Length;
			uint num2 = Ba.readUInt();
			for (int i = 0; i < num; i++)
			{
				UiBenchMenu.ACmd[i].is_auto = ((ulong)num2 & (ulong)(1L << (i & 31))) > 0UL;
			}
			UiBenchMenu.several_flags = (int)Ba.readUShort();
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			uint num = 0U;
			int num2 = UiBenchMenu.ACmd.Length;
			for (int i = 0; i < num2; i++)
			{
				UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[i];
				num |= (benchCmd.is_auto ? (1U << i) : 0U);
			}
			Ba.writeUInt(num);
			UiBenchMenu.need_save = false;
			Ba.writeUShort((ushort)UiBenchMenu.several_flags);
		}

		public static void initBattle()
		{
			UiBenchMenu.gameovered = (UiBenchMenu.event_defined = (UiBenchMenu.orgasm_onemore = false));
		}

		public static void endS(bool force = false)
		{
			UiBenchMenu.auto_start_temp_disable = (UiBenchMenu.orgasm_onemore = false);
			if (SceneGame.svd_reading)
			{
				return;
			}
			if (UiBenchMenu.event_defined || force)
			{
				UiBenchMenu.event_defined = (UiBenchMenu.gameovered = (UiBenchMenu.enemy_orgasm = false));
				int num = UiBenchMenu.ACmd.Length;
				for (int i = -1; i < num; i++)
				{
					((i == -1) ? UiBenchMenu.CmdBenchPee : UiBenchMenu.ACmd[i]).clear();
				}
				UiBenchMenu.Adebug_keys = null;
			}
		}

		public static void defineEvents(PR Pr, bool clearing = false)
		{
			if (clearing)
			{
				UiBenchMenu.endS(false);
			}
			UiBenchMenu.auto_start_stack_disable = false;
			int num = UiBenchMenu.ACmd.Length;
			bool flag = Pr.EpCon.getOrgasmedIndividualTotal() >= 1 && ((float)Pr.ep >= 400f || UiBenchMenu.orgasm_onemore);
			flag = flag && ((UiBenchMenu.enemy_orgasm && (float)Pr.ep >= 400f) || UiBenchMenu.orgasm_onemore || (TX.valid(Pr.M2D.ev_mobtype) && Pr.EpCon.getOrgasmedTotal() >= 8));
			for (int i = -1; i < num; i++)
			{
				UiBenchMenu.BenchCmd benchCmd = ((i == -1) ? UiBenchMenu.CmdBenchPee : UiBenchMenu.ACmd[i]);
				if (!UiBenchMenu.event_defined)
				{
					benchCmd.clear();
				}
				int num2 = 0;
				string text = "";
				string key = benchCmd.key;
				if (key != null)
				{
					if (!(key == "cure_hp"))
					{
						if (!(key == "cure_cloth"))
						{
							if (!(key == "cure_ep"))
							{
								if (!(key == "cure_egged"))
								{
									if (key == "pee")
									{
										if (benchCmd.FnCanUse(Pr))
										{
											text = "_bench_excrete_juice";
											num2 = 1050;
										}
									}
								}
								else if (UiBenchMenu.hasDebugKey("EGG"))
								{
									text = "_bench_cure_egged_00";
									num2 = 1;
								}
								else if (benchCmd.FnCanUse(Pr))
								{
									if (TX.valid(Pr.M2D.ev_mobtype))
									{
										text = "_bench_cure_ep_do_not";
										num2 = 1;
									}
									else
									{
										text = "_bench_cure_egged_00";
										if ((float)Pr.EggCon.total >= Pr.get_maxmp() * 0.7f && !X.SENSITIVE)
										{
											num2 = 1010;
										}
										else
										{
											num2 = ((Pr.EggCon.total > 0) ? 10 : 1);
										}
									}
								}
							}
							else if (!Pr.poseIsBenchMusturbOrgasm())
							{
								if (flag || UiBenchMenu.hasDebugKey("EP1"))
								{
									text = "_bench_cure_ep_enemy_orgasm";
									num2 = ((X.sensitive_level > 0) ? 2 : 1010);
								}
								else if ((float)Pr.ep >= 400f || UiBenchMenu.hasDebugKey("EP0"))
								{
									text = ((TX.valid(Pr.M2D.ev_mobtype) || Pr.NM2D.WDR.isNightingaleHere(Pr.Mp)) ? "_bench_cure_ep_do_not" : "_bench_cure_ep_base");
									num2 = 1;
								}
							}
						}
						else if ((Pr.BetoMng.is_torned && TX.valid(Pr.M2D.ev_mobtype)) || UiBenchMenu.hasDebugKey("CLOTH_MOB"))
						{
							text = "_bench_cure_cloth_mob";
							num2 = 1002;
						}
					}
					else if (Pr.Ser.has(SER.HP_REDUCE) || UiBenchMenu.hasDebugKey("HP0"))
					{
						text = "_bench_cure_hp_00";
						num2 = 10;
					}
				}
				int num3 = X.Abs(benchCmd.event_pow);
				if (num2 > 0 && num2 > num3)
				{
					benchCmd.event_name = text;
					benchCmd.event_pow = num2;
					if (num2 >= 1000 && num2 > num3)
					{
						benchCmd.alloc_auto_play = true;
					}
				}
			}
			UiBenchMenu.event_defined = true;
		}

		public static bool checkStackForceEvent(PR Pr, bool execute_stack, bool execute_event = true)
		{
			if (!UiBenchMenu.auto_start_temp_disable && !UiBenchMenu.auto_start_stack_disable && !EV.isActive(false))
			{
				int num = 999;
				UiBenchMenu.BenchCmd benchCmd = null;
				int num2 = UiBenchMenu.ACmd.Length;
				for (int i = -1; i < num2; i++)
				{
					UiBenchMenu.BenchCmd benchCmd2 = ((i == -1) ? UiBenchMenu.CmdBenchPee : UiBenchMenu.ACmd[i]);
					if (!benchCmd2.can_set_auto || benchCmd2.is_auto)
					{
						if (benchCmd2.FnCanUse != null && !benchCmd2.FnCanUse(Pr))
						{
							if (!benchCmd2.is_default && benchCmd2.event_pow >= 1000)
							{
								benchCmd2.event_pow *= -1;
							}
						}
						else if (benchCmd2.event_name != "" && benchCmd2.event_pow > num && benchCmd2.alloc_auto_play)
						{
							num = benchCmd2.event_pow;
							benchCmd = benchCmd2;
						}
					}
				}
				if (benchCmd != null)
				{
					if (execute_stack)
					{
						UiBenchMenu.auto_start_stack_disable = true;
						UiBenchMenu.playEvent(benchCmd, execute_event, Pr);
					}
					return true;
				}
			}
			return false;
		}

		public static bool playEvent(int cm_id)
		{
			return UiBenchMenu.playEvent(UiBenchMenu.ACmd[cm_id], true, null);
		}

		private static bool playEvent(UiBenchMenu.BenchCmd Cm, bool can_execute = true, PR PrOnSitDown = null)
		{
			string event_name = Cm.event_name;
			if (event_name != "")
			{
				Cm.alloc_auto_play = false;
				if (can_execute)
				{
					UiBenchMenu.EventCmd = Cm;
					if (!UiBenchMenu.EventCmd.is_default && UiBenchMenu.EventCmd.event_pow > 0)
					{
						UiBenchMenu.EventCmd.event_pow *= -1;
					}
					EvReader evReader = new EvReader("%M2EVENTCOMMAND__@_BENCH_" + Cm.key, 0, new string[] { "1" }, null);
					if (M2DBase.Instance != null)
					{
						M2DBase.Instance.FlagValotStabilize.Rem("UIGM");
					}
					if (PrOnSitDown == null)
					{
						evReader.Avariables[0] = "0";
						using (STB stb = TX.PopBld(null, 0))
						{
							stb.AR("#< % >");
							stb.AR("CHANGE_EVENT2 ", event_name, " 0");
							evReader.parseText(stb.ToString());
							goto IL_0180;
						}
					}
					using (STB stb2 = TX.PopBld(null, 0))
					{
						stb2.AR("WAIT_FN UICUTIN 110");
						stb2.AR("<LOAD>");
						stb2.AR("#< ", PrOnSitDown.key, " >");
						stb2.AR("CHANGE_EVENT2 ", event_name, " 1");
						stb2.AR("IFDEF _bench_auto_decline SEEK_END ");
						stb2.AR("#< ", PrOnSitDown.key, " >");
						stb2.AR("BENCH_AUTO_EXECUTE");
						evReader.parseText(stb2.ToString());
					}
					IL_0180:
					EV.stackReader(evReader, -1);
				}
				return true;
			}
			return false;
		}

		private static UiBenchMenu.BenchCmd GetCmd(string cmd_key)
		{
			int cmdIndex = UiBenchMenu.GetCmdIndex(cmd_key);
			if (cmdIndex < 0)
			{
				return null;
			}
			return UiBenchMenu.ACmd[cmdIndex];
		}

		private static int GetCmdIndex(string cmd_key)
		{
			for (int i = UiBenchMenu.ACmd.Length - 1; i >= 0; i--)
			{
				if (UiBenchMenu.ACmd[i].key == cmd_key)
				{
					return i;
				}
			}
			return -1;
		}

		public static bool hasDebugKey(string key)
		{
			return UiBenchMenu.Adebug_keys != null && UiBenchMenu.Adebug_keys.IndexOf(key.ToLower()) >= 0;
		}

		public static void debugDefineCmd(string[] Acmd, int si = 0)
		{
			int num = Acmd.Length;
			UiBenchMenu.event_defined = false;
			if (UiBenchMenu.Adebug_keys == null)
			{
				UiBenchMenu.Adebug_keys = new List<string>(num - si);
			}
			for (int i = si; i < num; i++)
			{
				UiBenchMenu.Adebug_keys.Add(Acmd[i].ToLower());
			}
		}

		public static bool can_transfer_uipic
		{
			get
			{
				UiBenchMenu benchMenu = (M2DBase.Instance as NelM2DBase).GM.getBenchMenu();
				return benchMenu == null || benchMenu.isTempWaiting();
			}
		}

		internal UiBenchMenu(UiGameMenu _GM, UiBoxDesigner _Bx, NelChipBench _BenchCp, UiGameMenu.STATE cmd_categ)
		{
			this.Bx = _Bx;
			this.GM = _GM;
			this.BenchCp = _BenchCp;
			this.left_pos = -(UiGameMenu.bounds_wh + 40f);
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.EfZoom = PostEffect.IT.setPE(POSTM.ZOOM2, 40f, 1f, -40);
			this.remake_command = true;
			UiBenchMenu.fineBenchCmdEnable();
			UiBenchMenu.hideModalOfflineOnBench();
			this.M2D.ResumeMem(true);
			UiBenchMenu.EventCmd = null;
		}

		public void deactivateEdit(bool gm_deactivating = false)
		{
			if (!gm_deactivating && this.t == -1f)
			{
				return;
			}
			if (this.M2D.curMap != null)
			{
				PRMain prmain = this.M2D.curMap.getKeyPr() as PRMain;
				if (prmain != null)
				{
					prmain.benchWaitInit(false);
				}
			}
			if ((this.done_cmd || UiBenchMenu.need_save) && CFG.autosave_on_bench)
			{
				this.done_cmd = false;
				COOK.autoSave(this.M2D, true, false);
			}
			this.pause_mem_phys = true;
			this.Bx.deactivate();
			UiBenchMenu.EventCmd = null;
			if (this.t >= -1f)
			{
				this.t = -2f;
				this.GM.fine_bench_modal = true;
			}
		}

		public bool pause_mem_phys
		{
			get
			{
				return this.pause_mem_phys_;
			}
			set
			{
				if (this.pause_mem_phys == value)
				{
					return;
				}
				this.pause_mem_phys_ = value;
				if (value)
				{
					this.M2D.PauseMem(true);
					return;
				}
				this.M2D.ResumeMem(true);
			}
		}

		public static bool canSwitchToBenchMenu(PR Pr)
		{
			return Pr.isMagicExistState() || Pr.isPunchState() || Pr.isBenchOrBenchPreparingState();
		}

		public bool run(float fcnt)
		{
			if (this.EfZoom != null)
			{
				this.EfZoom.fine(120);
			}
			if (this.t >= 0f)
			{
				PRMain prmain = this.M2D.curMap.getKeyPr() as PRMain;
				if (prmain == null || this.BenchCp == null)
				{
					this.deactivateEdit(false);
					this.run(fcnt);
					return false;
				}
				if (!prmain.isOnBench(true))
				{
					if (IN.isCancel() || IN.isMenuPD(1) || !UiBenchMenu.canSwitchToBenchMenu(prmain))
					{
						this.deactivateEdit(false);
						SND.Ui.play("cancel", false);
					}
					else if (this.initialize_bench)
					{
						if (!prmain.hasFoot() && this.delay < 100f)
						{
							prmain.benchWaitInit(true);
							this.t += fcnt;
							this.delay += fcnt;
						}
						else
						{
							this.t += fcnt;
							if (prmain.isCovering(this.BenchCp.mleft - 1f, this.BenchCp.mright + 1f, this.BenchCp.mtop - 1f, this.BenchCp.mbottom + 3f, 0f) && prmain.initBenchSitDown(this.BenchCp, false, false))
							{
								this.delay = 36f;
								this.initialize_bench = false;
								this.initialize_bench_show = true;
							}
							else
							{
								this.deactivateEdit(false);
							}
						}
					}
					else
					{
						this.deactivateEdit(false);
					}
				}
				else
				{
					if (this.t >= this.delay)
					{
						this.initialize_bench = false;
						if (this.initialize_bench_show)
						{
							this.initShow();
						}
					}
					this.t += fcnt;
					if (IN.isCancel() || IN.isMenuPD(1))
					{
						this.deactivateEdit(false);
						SND.Ui.play("cancel", false);
					}
				}
			}
			else if (this.t <= -2f)
			{
				if (this.EfZoom != null)
				{
					this.EfZoom.deactivate(true);
					PostEffect.IT.runDraw(0f, true);
				}
				return false;
			}
			return true;
		}

		public void initShow()
		{
			this.initialize_bench_show = false;
			if (!this.remake_command)
			{
				this.Btns = (this.Bx.Get("btns", false) as BtnContainerRunner).BCon as BtnContainerRadio<aBtn>;
				if (this.Btns == null)
				{
					this.remake_command = true;
				}
			}
			if (this.remake_command)
			{
				this.remake_command = false;
				this.Bx = this.GM.initBxCmd(UiGameMenu.STATE.BENCH);
				this.Bx.getBox().frametype = UiBox.FRAMETYPE.DARK;
				this.Bx.margin_in_lr = 18f;
				this.Bx.margin_in_tb = 38f;
				this.Bx.item_margin_x_px = 0f;
				this.Bx.item_margin_y_px = 0f;
				this.Bx.WH(580f, this.Bx.margin_in_tb * 2f + 4f + (float)(UiBenchMenu.ACmd.Length + 1) * 24f);
				this.Bx.activate();
				this.Bx.init();
				this.Bx.positionD(this.left_pos + this.Bx.swidth * 0.5f + 40f, IN.hh - (this.Bx.sheight * 0.5f + 30f), 3, 150f);
				int num = UiBenchMenu.ACmd.Length;
				string[] array = new string[num + 1];
				for (int i = 0; i < num; i++)
				{
					array[i] = UiBenchMenu.ACmd[i].key;
				}
				array[num] = "&&Cancel";
				this.Btns = this.Bx.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "btns",
					skin = "blackwin_center",
					w = 285f,
					h = 24f,
					margin_h = 0,
					margin_w = 0,
					clms = 1,
					keys = array,
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.FnChangedMenu),
					locked_click = true,
					navi_loop = 2
				});
				this.Btns.Get(num).click_snd = "cancel";
				Designer designer = this.Bx.addTab("enable_auto", this.Bx.use_w, this.Btns.get_sheight_px(), this.Bx.use_w, this.Btns.get_sheight_px(), false);
				designer.Smallest();
				designer.margin_in_lr = 18f;
				aBtnNel aBtnNel = null;
				aBtnNel aBtnNel2 = null;
				for (int j = 0; j < num; j++)
				{
					aBtn aBtn = this.Btns.Get(j);
					if (UiBenchMenu.ACmd[j].can_set_auto)
					{
						aBtnNel aBtnNel3 = this.Bx.addButtonT<aBtnNel>(new DsnDataButton
						{
							name = "auto_" + j.ToString(),
							title = "auto_" + j.ToString(),
							skin_title = "&&CheckBox_Auto_Bench",
							skin = "checkbox_small_dark",
							def = UiBenchMenu.ACmd[j].is_auto,
							w = designer.use_w,
							h = 24f,
							fnClick = new FnBtnBindings(this.fnClickAutoCheck)
						});
						aBtnNel3.setNaviL(aBtn, false, true);
						this.Bx.Br();
						if (aBtnNel != null)
						{
							aBtnNel3.setNaviT(aBtnNel, true, true);
						}
						else
						{
							aBtnNel2 = aBtnNel3;
						}
						aBtnNel = aBtnNel3;
					}
					else
					{
						this.Bx.Hr(0.4f, 11.5f, 11.5f, 1f).Br();
					}
					if (aBtnNel != null)
					{
						aBtn.setNaviR(aBtnNel, false, true);
					}
				}
				if (aBtnNel != null)
				{
					aBtnNel.setNaviB(this.Btns.Get(num), false, false);
				}
				if (aBtnNel2 != null)
				{
					aBtnNel2.setNaviT(this.Btns.Get(num), false, false);
				}
			}
			else
			{
				this.Bx.activate();
			}
			this.setEnableBtns();
			this.Btns.Get(this.btn_sel_index).Select(true);
		}

		private void setEnableBtns()
		{
			int num = UiBenchMenu.ACmd.Length;
			PRNoel prNoel = this.M2D.getPrNoel();
			for (int i = 0; i < num; i++)
			{
				UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[i];
				benchCmd.currennt_useable = benchCmd.scn_enable && (benchCmd.FnCanUse == null || benchCmd.FnCanUse(prNoel)) && (!benchCmd.only_in_safearea || (M2DBase.Instance as NelM2DBase).isSafeArea());
				aBtn aBtn = this.Btns.Get(i);
				ButtonSkin skin = aBtn.get_Skin();
				bool flag = benchCmd.event_name != "" && X.Abs(benchCmd.event_pow) > 1;
				if (benchCmd.key == "wait_nightingale" && !prNoel.NM2D.WDR.alreadyMeet(WanderingManager.TYPE.NIG))
				{
					aBtn.setSkinTitle("???");
				}
				else
				{
					aBtn.setSkinTitle((benchCmd.scn_enable ? "" : "<shape lock tx_color/>") + TX.Get("Bench_Cmd_" + UiBenchMenu.ACmd[i].key, "") + (flag ? "<img mesh=\"book_exist\"/>" : ""));
				}
				aBtn.SetLocked(!flag && !benchCmd.currennt_useable, true, true);
				if (!benchCmd.currennt_useable && i == this.btn_sel_index)
				{
					this.btn_sel_index++;
				}
			}
			this.Btns.setValue(-1, true);
		}

		private bool FnChangedMenu(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value == UiBenchMenu.ACmd.Length)
			{
				this.deactivateEdit(false);
				return false;
			}
			if (cur_value < 0)
			{
				return true;
			}
			aBtn aBtn = _B.Get(cur_value);
			string title = aBtn.title;
			this.btn_sel_index = cur_value;
			UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[cur_value];
			if (title == "fast_travel")
			{
				string text = this.M2D.cantFastTravel();
				if (text != null)
				{
					SND.Ui.play("locked", false);
					CURS.limitVib(_B.Get(cur_value), AIM.L);
					UILog.Instance.AddAlertTX(text, UILogRow.TYPE.ALERT);
					return false;
				}
			}
			if (!benchCmd.scn_enable)
			{
				SND.Ui.play("locked", false);
				CURS.limitVib(_B.Get(cur_value), AIM.L);
				UILog.Instance.AddAlertTX("Alert_bench_execute_scenario_locked", UILogRow.TYPE.ALERT);
				return false;
			}
			if (benchCmd.only_in_safearea && !(M2DBase.Instance as NelM2DBase).isSafeArea())
			{
				SND.Ui.play("locked", false);
				CURS.limitVib(_B.Get(cur_value), AIM.L);
				UILog.Instance.AddAlertTX("Alert_bench_execute_only_in_safe_area", UILogRow.TYPE.ALERT);
				return false;
			}
			if (title != null)
			{
				if (!(title == "wait_nightingale"))
				{
					if (!(title == "fast_travel"))
					{
						if (!(title == "fast_travel_home"))
						{
							if (!(title == "save"))
							{
								goto IL_0328;
							}
							if (!SCN.canSave(this.GM.BenchChip != null))
							{
								SND.Ui.play("locked", false);
								CURS.limitVib(_B.Get(cur_value), AIM.L);
								UILog.Instance.AddAlertTX("Alert_bench_execute_scenario_locked", UILogRow.TYPE.ALERT);
								return false;
							}
							this.deactivateTemp();
							this.GM.initCategoryEdit(CATEG.SVD_SELECT, false);
						}
						else
						{
							bool flag = !this.M2D.isSafeArea();
							WholeMapItem.WMTransferPoint.WMRectItem wmrectItem = null;
							WholeMapItem wholeMapItem = null;
							Map2d map2d = null;
							if (flag)
							{
								wmrectItem = this.M2D.WM.getDepertureRectSafeAreaMemory(COOK.getCurrentFile().safe_area_memory, ref map2d, ref wholeMapItem);
							}
							if (!flag || wmrectItem == null)
							{
								SND.Ui.play("locked", false);
								CURS.limitVib(_B.Get(cur_value), AIM.L);
								return false;
							}
							M2MoverPr pr = this.M2D.curMap.Pr;
							this.deactivateTemp();
							UiBenchMenu.ExecuteFastTravel(default(WMIconPosition), wholeMapItem, map2d, wmrectItem);
						}
					}
					else
					{
						this.deactivateTemp();
						this.GM.initCategoryEdit(CATEG.MAP, false);
					}
				}
				else
				{
					if (aBtn.isLocked() || this.M2D.curMap == null)
					{
						SND.Ui.play("locked", false);
						CURS.limitVib(_B.Get(cur_value), AIM.L);
						return false;
					}
					if (this.M2D.WDR.isOtherNpcAppearHere(this.M2D.curMap, this.M2D.WDR.getNightingale()))
					{
						CURS.limitVib(_B.Get(cur_value), AIM.L);
						UILog.Instance.AddAlertTX("Alert_nighingale_locked_by_other_people", UILogRow.TYPE.ALERT);
						return false;
					}
					if (!SCN.isWNpcEnableInMap(this.M2D.curMap, WanderingManager.TYPE.NIG))
					{
						SND.Ui.play("locked", false);
						CURS.limitVib(_B.Get(cur_value), AIM.L);
						UILog.Instance.AddAlertTX("Alert_bench_execute_scenario_locked", UILogRow.TYPE.ALERT);
						return false;
					}
					this.deactivateTemp();
					UiBenchMenu.ExecuteWaitNightingale();
				}
				return true;
			}
			IL_0328:
			title == "cure_mp";
			if (!aBtn.isLocked())
			{
				this.done_cmd = true;
				UiBenchMenu.need_save = true;
			}
			if (TX.valid(benchCmd.event_name))
			{
				this.deactivateTemp();
				UiBenchMenu.playEvent(cur_value);
			}
			else
			{
				UiBenchMenu.ExecuteBenchCmd(cur_value, 0, true, false, false, 2);
			}
			return false;
		}

		private bool fnClickAutoCheck(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			if (REG.match(B.title, REG.RegSuffixNumber))
			{
				UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[X.NmI(REG.R1, 0, false, false)];
				if (benchCmd.can_set_auto)
				{
					benchCmd.is_auto = B.isChecked();
				}
			}
			return true;
		}

		public static int ExecuteBenchCmd(string cmd, int delay = 0, bool fine_pr_motion = true, bool nosnd = false)
		{
			int cmdIndex = UiBenchMenu.GetCmdIndex(cmd);
			if (cmdIndex == -1)
			{
				X.de("ベンチコマンドが見つかりません: " + cmd, null);
				return delay;
			}
			return UiBenchMenu.ExecuteBenchCmd(cmdIndex, delay, fine_pr_motion, false, nosnd, 2);
		}

		public static int ExecuteBenchCmd(int cmd_id, int delay = 0, bool fine_pr_motion = true, bool on_auto = false, bool nosnd = false, byte hp_low = 2)
		{
			if (cmd_id < 0)
			{
				int num = UiBenchMenu.ACmd.Length;
				delay += 18;
				UiBenchMenu.fineBenchCmdEnable();
				if (hp_low == 2)
				{
					for (int i = 0; i < num; i++)
					{
						PR pr = M2DBase.Instance.curMap.getPr(i) as PR;
						if (!(pr == null))
						{
							hp_low = ((pr.hp_ratio <= 0.66f) ? 1 : 0);
							break;
						}
					}
				}
				for (int j = 0; j < num; j++)
				{
					UiBenchMenu.BenchCmd benchCmd = UiBenchMenu.ACmd[j];
					if (!benchCmd.auto_event_enabled && benchCmd.can_set_auto && benchCmd.is_auto && benchCmd.scn_enable)
					{
						delay = UiBenchMenu.ExecuteBenchCmd(j, delay, false, true, nosnd, hp_low);
					}
				}
			}
			else
			{
				if (cmd_id >= UiBenchMenu.ACmd.Length)
				{
					return delay;
				}
				int count_players = M2DBase.Instance.curMap.count_players;
				UiBenchMenu.BenchCmd benchCmd2 = UiBenchMenu.ACmd[cmd_id];
				if (!benchCmd2.scn_enable)
				{
					return delay;
				}
				if (benchCmd2.only_in_safearea && !(M2DBase.Instance as NelM2DBase).isSafeArea())
				{
					return delay;
				}
				string key = benchCmd2.key;
				string text = null;
				for (int k = 0; k < count_players; k++)
				{
					PRMain prmain = M2DBase.Instance.curMap.getPr(k) as PRMain;
					if (!(prmain == null) && key != null)
					{
						if (!(key == "cure_hp"))
						{
							if (!(key == "cure_cloth"))
							{
								if (!(key == "cure_ep"))
								{
									if (!(key == "cure_egged"))
									{
										if (key == "cure_mp")
										{
											prmain.recheck_emot_in_gm = true;
											prmain.cureFull(true, false, false, false);
											if (!nosnd)
											{
												prmain.PtcVar("delay", (double)delay).PtcST("bench_cure_mp", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
												prmain.TeCon.setColorBlinkAdd(1f, 30f, 0.5f, 11665305, delay);
											}
											if (!fine_pr_motion || benchCmd2.currennt_useable)
											{
												UILog.Instance.AddAlertTX("Bench_execute_cure_mp", UILogRow.TYPE.ALERT_BENCH);
											}
										}
									}
									else if (on_auto || X.SENSITIVE || TX.noe(benchCmd2.event_name))
									{
										UiBenchMenu.executeOtherCommand("cure_egged", false);
									}
								}
								else
								{
									fine_pr_motion = false;
									if (on_auto || X.SENSITIVE || TX.noe(benchCmd2.event_name))
									{
										prmain.cureFull(false, true, false, false);
									}
									else
									{
										prmain.initMasturbation(true, true);
									}
								}
							}
							else
							{
								if (hp_low == 2)
								{
									hp_low = ((prmain.hp_ratio <= 0.66f) ? 1 : 0);
								}
								prmain.UP.CutinMng.initBenchCureCloth(delay, hp_low >= 1, false);
								text = "shower_clean_cure_cloth";
								if (!nosnd)
								{
									prmain.PtcVar("delay", (double)delay).PtcST("bench_cure_torned", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
									prmain.TeCon.setColorBlinkAdd(1f, 30f, 0.5f, 9369341, delay);
								}
								if (!fine_pr_motion || benchCmd2.currennt_useable)
								{
									UILog.Instance.AddAlertTX("Bench_execute_cure_cloth", UILogRow.TYPE.ALERT_BENCH);
								}
							}
						}
						else
						{
							prmain.UP.CutinMng.initBenchCureHp(delay, prmain.BetoMng.is_torned, false);
							prmain.cureHp((int)prmain.get_maxhp());
							prmain.DMG.setHpCrack(0);
							if (!nosnd)
							{
								prmain.PtcVar("delay", (double)delay).PtcST("bench_cure_hp", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								prmain.TeCon.setColorBlinkAdd(1f, 30f, 0.5f, 16757145, delay);
							}
							prmain.recheck_emot_in_gm = true;
							if (!fine_pr_motion || benchCmd2.currennt_useable)
							{
								UILog.Instance.AddAlertTX("Bench_execute_cure_hp", UILogRow.TYPE.ALERT_BENCH);
							}
						}
					}
				}
				if (text != null)
				{
					UiBenchMenu.executeOtherCommand(text, false);
				}
			}
			if (fine_pr_motion)
			{
				int count_players2 = M2DBase.Instance.curMap.count_players;
				for (int l = 0; l < count_players2; l++)
				{
					PRMain prmain2 = M2DBase.Instance.curMap.getPr(l) as PRMain;
					if (!(prmain2 == null))
					{
						prmain2.initBenchSitDown(null, true, false);
					}
				}
			}
			return delay + 18;
		}

		public static int executeOtherCommand(string cmd, bool show_ui_log)
		{
			int count_players = M2DBase.Instance.curMap.count_players;
			int num = 0;
			for (int i = 0; i < count_players; i++)
			{
				PR pr = M2DBase.Instance.curMap.getPr(i) as PR;
				if (!(pr == null))
				{
					if (cmd != null)
					{
						uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num2 <= 1497915634U)
						{
							if (num2 <= 513096600U)
							{
								if (num2 != 430355439U)
								{
									if (num2 != 513096600U)
									{
										goto IL_0279;
									}
									if (!(cmd == "shower_clean_cure_cloth"))
									{
										goto IL_0279;
									}
									goto IL_01DC;
								}
								else if (!(cmd == "cure_egged"))
								{
									goto IL_0279;
								}
							}
							else if (num2 != 575437287U)
							{
								if (num2 != 1497915634U)
								{
									goto IL_0279;
								}
								if (!(cmd == "pee_excrete"))
								{
									goto IL_0279;
								}
								goto IL_01BA;
							}
							else if (!(cmd == "restroom_cure_egged"))
							{
								goto IL_0279;
							}
							if (pr.EggCon.total > 0)
							{
								num = X.Mx(num, pr.EggCon.total);
								pr.EggCon.clear(true);
							}
							if (pr.NM2D.isSafeArea())
							{
								pr.cureFull(true, false, false, false);
								goto IL_0279;
							}
							goto IL_0279;
						}
						else if (num2 <= 2343068853U)
						{
							if (num2 != 2051777857U)
							{
								if (num2 != 2343068853U)
								{
									goto IL_0279;
								}
								if (!(cmd == "shower_clean"))
								{
									goto IL_0279;
								}
								goto IL_01DC;
							}
							else if (!(cmd == "pee"))
							{
								goto IL_0279;
							}
						}
						else if (num2 != 4178361163U)
						{
							if (num2 != 4275172322U)
							{
								goto IL_0279;
							}
							if (!(cmd == "shower_cure_cloth"))
							{
								goto IL_0279;
							}
							goto IL_01DC;
						}
						else
						{
							if (!(cmd == "shower"))
							{
								goto IL_0279;
							}
							goto IL_01DC;
						}
						IL_01BA:
						num = X.Mx(pr.JuiceCon.cureOnBench(cmd == "pee_excrete"), num);
						goto IL_0279;
						IL_01DC:
						pr.Ser.Cure(SER.SHAMED_WET);
						pr.BetoMng.setWetten(pr, false, false);
						bool flag = cmd == "shower_clean" || cmd == "shower_clean_cure_cloth";
						bool flag2 = cmd == "shower_cure_cloth" || cmd == "shower_clean_cure_cloth";
						if (flag && flag2)
						{
							pr.BetoMng.cleanAll(true);
						}
						else if (flag2)
						{
							pr.BetoMng.setTorned(pr, false, false);
						}
						else
						{
							pr.BetoMng.cleanBetobeto(false);
						}
						pr.fineClothTorned();
						pr.recheck_emot = true;
						pr.recheck_emot_in_gm = true;
					}
					IL_0279:
					pr.Ser.checkSer();
				}
			}
			if (show_ui_log)
			{
				UiBenchMenu.ShowLogForOtherCommand(cmd, num);
			}
			return num;
		}

		public static UILogRow ShowLogForOtherCommand(string cmd, int result)
		{
			string text = null;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1497915634U)
				{
					if (num <= 513096600U)
					{
						if (num != 430355439U)
						{
							if (num != 513096600U)
							{
								goto IL_0131;
							}
							if (!(cmd == "shower_clean_cure_cloth"))
							{
								goto IL_0131;
							}
							goto IL_012B;
						}
						else if (!(cmd == "cure_egged"))
						{
							goto IL_0131;
						}
					}
					else if (num != 575437287U)
					{
						if (num != 1497915634U)
						{
							goto IL_0131;
						}
						if (!(cmd == "pee_excrete"))
						{
							goto IL_0131;
						}
						goto IL_010B;
					}
					else if (!(cmd == "restroom_cure_egged"))
					{
						goto IL_0131;
					}
					if (result > 0)
					{
						text = "Alert_layed_egg";
						goto IL_0131;
					}
					goto IL_0131;
				}
				else if (num <= 2051777857U)
				{
					if (num != 2033152003U)
					{
						if (num != 2051777857U)
						{
							goto IL_0131;
						}
						if (!(cmd == "pee"))
						{
							goto IL_0131;
						}
					}
					else
					{
						if (!(cmd == "cure_cloth"))
						{
							goto IL_0131;
						}
						goto IL_012B;
					}
				}
				else if (num != 2343068853U)
				{
					if (num != 4275172322U)
					{
						goto IL_0131;
					}
					if (!(cmd == "shower_cure_cloth"))
					{
						goto IL_0131;
					}
					goto IL_012B;
				}
				else
				{
					if (!(cmd == "shower_clean"))
					{
						goto IL_0131;
					}
					goto IL_012B;
				}
				IL_010B:
				if (result == 0)
				{
					text = "Alert_no_pee_go_out";
					goto IL_0131;
				}
				if (cmd == "pee_excrete")
				{
					text = "Alert_no_content_in_stomach";
					goto IL_0131;
				}
				goto IL_0131;
				IL_012B:
				text = "Bench_execute_cure_cloth";
			}
			IL_0131:
			if (text != null)
			{
				return UILog.Instance.AddAlertTX(text, UILogRow.TYPE.ALERT_BENCH);
			}
			return null;
		}

		public static bool ExecuteFastTravel(WMIconPosition Dest, WholeMapItem SrcWM = null, Map2d SrcMp = null, WholeMapItem.WMTransferPoint.WMRectItem WmRect = null)
		{
			if (!Dest.valid && SrcWM == null)
			{
				return false;
			}
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			M2MoverPr keyPr = nelM2DBase.curMap.getKeyPr();
			Vector3 vector = M2Mover.checkDirectionWalkable(keyPr, keyPr.x, keyPr.footbottom, 2, false);
			EvReader evReader = new EvReader("%BENCH_EVENT", 0, null, null);
			string text = "";
			if (vector.x != keyPr.x)
			{
				text = ((keyPr.x < vector.x) ? " @R" : " @L");
			}
			STB stb = TX.PopBld(null, 0);
			bool flag = false;
			WholeMapItem wholeMapItem = null;
			if (SrcWM != null)
			{
				wholeMapItem = nelM2DBase.WM.GetWholeFor(WmRect.Mp, false);
				flag = true;
				M2LpMapTransferWarp.GoOutOtherAreaScript(stb, nelM2DBase.WM.CurWM, wholeMapItem, WmRect.Mp, null);
			}
			stb += "UIGM DEACTIVATE\n";
			stb += "SEND_EVENT_CORRUPTION ";
			stb.AR("PRE_UNLOAD");
			if (SrcWM == null)
			{
				stb.AR("_result=1");
				stb.Add("CHANGE_EVENT2 __M2D_FASTTRAVEL_PRE ").Add(Dest.getDepertureMap().key).Add(" ", (nelM2DBase.WM.CurWM != null) ? nelM2DBase.WM.CurWM.text_key : " ")
					.Ret("\n");
				stb.AR("IF $_result'==0' SEEK_END");
			}
			stb += "DENY_SKIP\n";
			stb += "STOP_LETTERBOX\n";
			stb += "\n";
			stb.AR("VALOTIZE");
			stb.Add("#MS_ % 'F P[bench2stand] w20 ", text);
			stb.Add(" P[walk~] >+[ ", (int)((vector.x - keyPr.x) * keyPr.CLEN), ",0 :90 ]'").Ret("\n");
			stb += "WAIT 40\n";
			stb.Add("PIC_FILL &9 ").AddColor(NEL.FillingBgCol.rgba).Ret("\n");
			stb += "PIC_FADEIN &9 50\n";
			stb += "PIC_TFADE &9 B2T \n";
			stb += "WAIT 30 \n";
			if (Dest.valid)
			{
				stb.AR("COOK_ADD_WALK_COUNT ", Dest.getDepertureMap().key);
				stb.AR("INIT_MAP_MATERIAL ", Dest.getDepertureMap().key, " 1");
			}
			else
			{
				stb.AR("INIT_MAP_MATERIAL ", WmRect.Mp.key, " 2");
			}
			stb += "WAIT 20 \n";
			stb += "WAIT_FN MAP_TRANSFER \n";
			if (flag)
			{
				stb.Add("NEL_MAP_TRANSFER", " ", SrcMp.key).Add(" ", WmRect.index, " ").Add(WmRect.key)
					.Ret("\n");
				stb.ARd("CHANGE_EVENT2 __M2D_FLUSHED_AREA", " ", nelM2DBase.WM.CurWM.text_key, wholeMapItem.text_key, null, null);
				AIM aim = WmRect.getAim();
				int num = ((WmRect.index < 0) ? 2 : 1);
				stb.Add("#MS_ % '>+[", 45 * num * CAim._XD(aim, 1), ",0 :").Add(60 * num).Add("]'");
			}
			else
			{
				Vector2 depertureMapPos = Dest.getDepertureMapPos();
				stb.Add("NEL_EXECUTE_FAST_TRAVEL '", Dest.getDepertureMap().key, "' ").Add((int)depertureMapPos.x).Add(" ", (int)depertureMapPos.y, " ")
					.Add(40)
					.Ret("\n");
			}
			stb.AR("ALLOW_SKIP");
			stb.AR("PIC_FADEOUT &9 55");
			stb.AR("PIC_TFADE &9 B2T ");
			stb.AR("WAIT_MOVE");
			stb.AR("PR_CURE 0 0 1");
			if (flag)
			{
				stb.AR("#< % >");
				stb.AR("PR_KEY_SIMULATE L 0");
				stb.AR("PR_KEY_SIMULATE R 0");
				stb.AR("WAIT_FN REELMNG");
				stb.AR("AUTO_SAVE");
			}
			else
			{
				stb.AR("AUTO_SAVE_BENCH");
				stb.AR("CHANGE_EVENT __M2D_FASTTRAVEL_AFTER");
			}
			evReader.parseText(stb);
			TX.ReleaseBld(stb);
			EV.stackReader(evReader, -1);
			return true;
		}

		public static void ExecuteWaitNightingale()
		{
			EV.stack("___Nightingale/_wait_at_bench", 0, -1, null, null);
		}

		public void deactivateTemp()
		{
			if (this.t >= 0f)
			{
				this.t = -1f;
				this.delay = 20f;
				this.pause_mem_phys = false;
				this.Bx.deactivate();
			}
		}

		internal bool activateTemp(UiGameMenu.STATE cmd_categ, bool checking_special_state = false)
		{
			if (this.t == -1f)
			{
				IN.clearPushDown(true);
				if (checking_special_state && this.M2D.curMap != null)
				{
					PR pr = this.M2D.curMap.getKeyPr() as PR;
					if (pr != null && pr.isMasturbateState() && pr.SpRunner is M2PrMasturbate)
					{
						return false;
					}
				}
				this.initialize_bench_show = true;
				this.pause_mem_phys = false;
				this.t = this.delay;
				this.remake_command = cmd_categ != UiGameMenu.STATE.BENCH;
			}
			return true;
		}

		public static void showModalOfflineOnBench()
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase.GM.isActive())
			{
				return;
			}
			PR pr = nelM2DBase.curMap.getKeyPr() as PR;
			if (!(pr != null) || !pr.isBenchState())
			{
				nelM2DBase.IMNG.hidePopUp(NelItemManager.POPUP.BENCH);
				return;
			}
			NelChipBench nearBench = pr.getNearBench(false, false);
			if (nearBench != null)
			{
				nelM2DBase.IMNG.showPopUp(NelItemManager.POPUP.BENCH, TX.Get("PopUp_bench", ""), pr.x, pr.y, 100f + nearBench.bottom_raise_px * 2f, -100f);
				return;
			}
			nelM2DBase.IMNG.hidePopUp(NelItemManager.POPUP.BENCH);
		}

		public static void hideModalOfflineOnBench()
		{
			(M2DBase.Instance as NelM2DBase).IMNG.hidePopUp(NelItemManager.POPUP.BENCH);
		}

		public bool isTempWaiting()
		{
			return this.t == -1f;
		}

		public void destruct()
		{
			this.pause_mem_phys = true;
		}

		public bool isActive()
		{
			return this.t >= -1f;
		}

		private const string bench_ev_name = "%BENCH_EVENT";

		private static int several_flags;

		public static bool auto_start_temp_disable;

		private static UiBenchMenu.BenchCmd[] ACmd;

		private static UiBenchMenu.BenchCmd CmdBenchPee;

		private static UiBenchMenu.BenchCmd EventCmd;

		private static List<string> Adebug_keys;

		public const int EVENT_POW_ON_SITDOWN = 1000;

		public const int EVENT_POW_ON_SITDOWN_MOB_EP = 1050;

		public const int EVENT_POW_DEFAULT = 1;

		public const int delay_default = 20;

		public float delay = 20f;

		private const float BTN_H = 24f;

		private UiGameMenu GM;

		private float t;

		private int btn_sel_index;

		private float left_pos;

		private NelM2DBase M2D;

		private UiBoxDesigner Bx;

		private PostEffectItem EfZoom;

		private NelChipBench BenchCp;

		public bool done_cmd;

		private bool remake_command;

		private bool initialize_bench = true;

		private bool initialize_bench_show = true;

		private BtnContainerRadio<aBtn> Btns;

		private bool pause_mem_phys_;

		public const string bench_menu_prefix = "Bench_Cmd_";

		private sealed class BenchCmd
		{
			public bool auto_event_enabled
			{
				get
				{
					return TX.valid(this.event_name) && this.event_pow >= 1000 && this.alloc_auto_play;
				}
			}

			public BenchCmd(string _key, Func<PR, bool> _FnCanUse, bool _can_set_auto = true, bool _only_in_safearea = false)
			{
				this.key = _key;
				this.FnCanUse = _FnCanUse;
				this.can_set_auto = _can_set_auto;
				this.only_in_safearea = _only_in_safearea;
			}

			public void eventActivate()
			{
				if (this.event_pow < 0)
				{
					this.event_pow *= -1;
				}
			}

			public bool is_default
			{
				get
				{
					return this.event_pow == 1;
				}
			}

			public UiBenchMenu.BenchCmd clear()
			{
				this.event_name = "";
				this.event_pow = 0;
				this.alloc_auto_play = false;
				return this;
			}

			public UiBenchMenu.BenchCmd newGame()
			{
				if (this.can_set_auto)
				{
					this.is_auto = true;
				}
				return this.clear();
			}

			public string key;

			public bool can_set_auto = true;

			public bool only_in_safearea;

			public bool is_auto;

			public bool currennt_useable;

			public string event_name = "";

			public bool alloc_auto_play;

			public int event_pow;

			public bool scn_enable = true;

			public Func<PR, bool> FnCanUse;
		}
	}
}
