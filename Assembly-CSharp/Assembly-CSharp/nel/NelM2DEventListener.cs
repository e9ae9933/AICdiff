using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.gm;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class NelM2DEventListener : M2DEventListener, IRunAndDestroy
	{
		public NelM2DEventListener(NelM2DBase _M2D)
			: base(_M2D)
		{
			this.nM2D = _M2D;
		}

		public override void newGame()
		{
			this.deactivatePE(true);
			this.change_scene_on_ev_quit = null;
			this.need_autosave = false;
		}

		public override void destruct()
		{
			base.destruct();
		}

		public override int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			string cmd2 = rER.cmd;
			if (cmd2 != null)
			{
				if (cmd2 == "PREPARE_SV_TEXTURE")
				{
					this.nM2D.prepareSvTexture(rER._1, rER._B2);
					return 0;
				}
				if (cmd2 == "TX_BOARD")
				{
					if (rER._1.IndexOf("<<<") >= 0)
					{
						NelMSGContainer.checkHereDocument(rER._1, ER.name, null, rER, true, null, true);
					}
					return 0;
				}
			}
			return base.EvtCacheRead(ER, cmd, rER);
		}

		public override TxEvalListenerContainer createListenerEval(int cap_fn = 0)
		{
			TxEvalListenerContainer txEvalListenerContainer = base.createListenerEval(cap_fn + 22);
			txEvalListenerContainer.Add("masturbate_count", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)this.PlayerNoel.EpCon.masturbate_count);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("difficulty", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)DIFF.I);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(EnemySummoner.isActiveBorder() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_torned", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)(this.PlayerNoel.BetoMng.is_torned ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_ep", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)this.PlayerNoel.ep);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("pr_egged", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (!(M2EventCommand.EvMV is PR))
				{
					X.de("M2EventCommand.EvMV が PR ではない", null);
					return;
				}
				TX.InputE((float)(M2EventCommand.EvMV as PR).EggCon.total);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_wetten", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE(this.PlayerNoel.BetoMng.wetten);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_cloth_dirty", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (CFG.ui_effect_dirty == 0 || CFG.ui_effect_density == 0)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)(this.PlayerNoel.BetoMng.isActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_carrying_box", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)((this.PlayerNoel.getSkillManager().getCarryingBox() != null) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("stomach", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE(this.IMNG.StmNoel.eaten_cost);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("danger_level", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)this.NightCon.getDangerMeterVal(true, false));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("is_night", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(this.NightCon.isNight() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("load_version", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				SVD.sFile currentFile = COOK.getCurrentFile();
				TX.InputE((float)((currentFile != null) ? currentFile.version : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_barrier_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(EnemySummoner.isActiveBorder() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_defeated_this_session", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				NightController.SummonerData lpInfo = this.NightCon.GetLpInfo(EnemySummoner.Get(Aargs[0], true), this.nM2D.Get((Aargs.Count == 1) ? Aargs[0] : Aargs[1], false), false);
				TX.InputE((float)((lpInfo != null && lpInfo.defeated_in_session) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_bote", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				float nm = X.GetNm(Aargs, (float)CFGSP.threshold_pregnant * 0.01f, true, 0);
				PRNoel playerNoel = this.PlayerNoel;
				TX.InputE((float)((playerNoel.EggCon.total > (int)(playerNoel.get_maxmp() * nm)) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("walk_xspeed", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				Map2d curMap = M2DBase.Instance.curMap;
				if (curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2Mover moverByName = curMap.getMoverByName(Aargs[0], false);
				TX.InputE((moverByName is PR) ? (moverByName as PR).get_walk_xspeed() : ((moverByName != null) ? moverByName.vx : 0f));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("crouch", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				Map2d curMap2 = M2DBase.Instance.curMap;
				if (curMap2 == null || Aargs.Count == 0)
				{
					return;
				}
				M2Mover moverByName2 = curMap2.getMoverByName(Aargs[0], false);
				if (moverByName2 is PR)
				{
					TX.InputE((float)((moverByName2 as PR).isPoseCrouch(false) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SF", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)COOK.getSF(X.Get<string>(Aargs, 0)));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SfEvt", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)COOK.getSF("EV_" + X.Get<string>(Aargs, 0, "")));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("spcfg_enable", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)(CFGSP.isSpEnabled(Aargs[0]) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("fatal_watched", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)(MGV.isFatalSceneAlreadyWatched(Aargs[0]) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("visitted", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				Map2d map2d = this.nM2D.Get(Aargs[0], false);
				if (map2d == null)
				{
					return;
				}
				WholeMapItem wholeFor = this.WM.GetWholeFor(map2d, true);
				if (wholeFor != null)
				{
					TX.InputE((float)(wholeFor.isVisitted(map2d) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SkillHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				PrSkill prSkill = SkillManager.Get(Aargs[0]);
				if (prSkill == null)
				{
					X.de("unknown skill: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)(prSkill.visible ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("MagicHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				int num = 0;
				MGKIND mgkind;
				if (!FEnum<MGKIND>.TryParse(Aargs[0], out mgkind, true))
				{
					X.de("MGKIND パースエラー:" + Aargs[0], null);
				}
				else
				{
					num = (MagicSelector.isObtained(mgkind) ? 1 : 0);
				}
				TX.InputE((float)num);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SkillEnable", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				PrSkill prSkill2 = SkillManager.Get(Aargs[0]);
				if (prSkill2 == null)
				{
					X.de("unknown skill: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)((prSkill2.visible && prSkill2.enabled) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("ItemHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				int nmI = X.GetNmI(Aargs, -1, true, 1);
				NelItem byId = NelItem.GetById(Aargs[0], false);
				if (byId == null)
				{
					return;
				}
				ItemStorage[] inventoryArray = this.IMNG.getInventoryArray();
				int num2 = 0;
				for (int i = inventoryArray.Length - 1; i >= 0; i--)
				{
					num2 += inventoryArray[i].getCountMoreGrade(byId, nmI);
				}
				TX.InputE((float)num2);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("ItemCategoryHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				int nmI2 = X.GetNmI(Aargs, -1, true, 1);
				ItemStorage[] inventoryArray2 = this.IMNG.getInventoryArray();
				int num3 = 0;
				for (int j = inventoryArray2.Length - 1; j >= 0; j--)
				{
					num3 += inventoryArray2[j].getCountMoreGrade(Aargs[0], nmI2);
				}
				TX.InputE((float)num3);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("empty_bottle", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				ItemStorage[] inventoryArray3 = this.IMNG.getInventoryArray();
				int num4 = 0;
				for (int k = inventoryArray3.Length - 1; k >= 0; k--)
				{
					ItemStorage itemStorage = inventoryArray3[k];
					if (itemStorage == this.IMNG.getInventory())
					{
						num4 += itemStorage.getEmptyBottleCount();
					}
					else
					{
						num4 += itemStorage.getCount(NelItem.Bottle, -1);
					}
				}
				TX.InputE((float)num4);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("StoreItemCount", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				StoreManager storeManager = StoreManager.Get(Aargs[0], false);
				if (storeManager != null)
				{
					TX.InputE((float)storeManager.countItems());
					return;
				}
				TX.InputE(0f);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("NoelCasting", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				string text = Aargs[0];
				MGKIND mgkind2;
				if (!FEnum<MGKIND>.TryParse(text, out mgkind2, true))
				{
					X.de("不明なMGKIND: " + text, null);
					return;
				}
				TX.InputE((float)(this.PlayerNoel.isCastingSpecificMagic(mgkind2) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("item_capacity", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				NelItem byId2 = NelItem.GetById(Aargs[0], false);
				if (byId2 != null)
				{
					TX.InputE((float)this.IMNG.getInventory().getItemCapacity(byId2, false, false));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("quest_progress", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)this.QUEST.getProgress(Aargs[0]));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("guildquest_progressing", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)(this.GUILD.isProgressing(Aargs[0]) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("guildquest_rank", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)this.GUILD.current_grank);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noelRPI", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				RCP.RPI_EFFECT rpi_EFFECT;
				if (!FEnum<RCP.RPI_EFFECT>.TryParse(Aargs[0], out rpi_EFFECT, true))
				{
					X.de("不明なRecipeManager.RPI_EFFECT: " + Aargs[0], null);
					return;
				}
				TX.InputE(this.PlayerNoel.getRE(rpi_EFFECT));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SerHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				SER ser;
				if (!FEnum<SER>.TryParse(Aargs[0], out ser, true))
				{
					X.de("不明なSER: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)(this.PlayerNoel.Ser.has(ser) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SerLevel", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				SER ser2;
				if (!FEnum<SER>.TryParse(Aargs[0], out ser2, true))
				{
					X.de("不明なSER: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)this.PlayerNoel.Ser.getLevel(ser2));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("trm_has_newer_item", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)TRMManager.hasNewerItem(false));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("craft_ui_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(NEL.isCraftUiActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("alchemy_lectured", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(SCN.alchemy_lectured ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("CFG", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				string text2 = Aargs[0];
				if (text2 != null)
				{
					if (text2 == "autorun")
					{
						TX.InputE(M2MoverPr.jump_press_reverse);
						return;
					}
					if (text2 == "stick_thresh")
					{
						TX.InputE(M2MoverPr.running_thresh);
						return;
					}
					if (!(text2 == "bgm_volume"))
					{
						return;
					}
					TX.InputE(SND.bgm_volume01);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("achive", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)COOK.getAchive(Aargs[0]));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("nightingale_here", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(this.WDR.isNightingaleHere(base.curMap) ? 1 : 0));
			}, Array.Empty<string>());
			EnemySummoner.prepareListenerEval(txEvalListenerContainer);
			return txEvalListenerContainer;
		}

		public override bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1894169261U)
				{
					if (num <= 1017903279U)
					{
						if (num <= 578731911U)
						{
							if (num <= 231640342U)
							{
								if (num <= 167214176U)
								{
									if (num <= 81073948U)
									{
										if (num != 52259617U)
										{
											if (num != 81073948U)
											{
												goto IL_3C72;
											}
											if (!(cmd == "CONFIRM_WAIT_NIGHTINGALE"))
											{
												goto IL_3C72;
											}
											EV.initWaitFn(new GameObject("UiConfirmAreaChange").AddComponent<UiWarpConfirm>().Init(UiWarpConfirm.CTYPE.WAIT_NIGHTINGALE, null, null), 0);
											return true;
										}
										else
										{
											if (!(cmd == "DISABLESKILL_NOANNOUNCE"))
											{
												goto IL_3C72;
											}
											goto IL_1C5C;
										}
									}
									else if (num != 111508916U)
									{
										if (num != 167214176U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "ENABLESKILL_NOANNOUNCE"))
										{
											goto IL_3C72;
										}
										goto IL_1C5C;
									}
									else
									{
										if (!(cmd == "COFFEEMAKER_SET"))
										{
											goto IL_3C72;
										}
										Map2d map2d = this.nM2D.Get(rER._1, false);
										if (map2d != null && SCN.isWNpcEnable(this.nM2D, WanderingManager.TYPE.COF))
										{
											this.WDR.Get(WanderingManager.TYPE.COF).setCurrentPos(map2d, false);
										}
										return true;
									}
								}
								else if (num <= 218985409U)
								{
									if (num != 177837449U)
									{
										if (num != 218985409U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "GETITEM_SUPPLIER"))
										{
											goto IL_3C72;
										}
										goto IL_1841;
									}
									else
									{
										if (!(cmd == "DEFEAT_EVENT2"))
										{
											goto IL_3C72;
										}
										goto IL_39DA;
									}
								}
								else if (num != 224446392U)
								{
									if (num != 231640342U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "PR_ASSIGN_REVERT_POSITION"))
									{
										goto IL_3C72;
									}
									COOK.getCurrentFile().assignRevertPosition(this.nM2D);
									return true;
								}
								else
								{
									if (!(cmd == "ABORT_PR_MAGIC"))
									{
										goto IL_3C72;
									}
									this.PlayerNoel.getSkillManager().killHoldMagic(false, false);
									return true;
								}
							}
							else if (num <= 365229814U)
							{
								if (num <= 306224295U)
								{
									if (num != 269575565U)
									{
										if (num != 306224295U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "WM_CHANGE_SAVE_NIGHT_PROGRESS"))
										{
											goto IL_3C72;
										}
										WholeMapItem byTextKey = this.WM.GetByTextKey(rER._1);
										if (byTextKey == null)
										{
											return rER.tError("不明な text_key WM: " + rER._1);
										}
										if (!byTextKey.safe_area)
										{
											byTextKey.reached_night_level = (ushort)X.Mx((int)byTextKey.reached_night_level, this.NightCon.getDangerMeterVal(true, false));
										}
										return true;
									}
									else if (!(cmd == "GETMONEY"))
									{
										goto IL_3C72;
									}
								}
								else if (num != 340025110U)
								{
									if (num != 365229814U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "TITLECALL_HIDE"))
									{
										goto IL_3C72;
									}
									if (rER._B1)
									{
										this.nM2D.AreaTitle.deactivate(true);
									}
									else
									{
										this.nM2D.AreaTitle.hideProgress();
										this.areatitle_hide_progress = true;
									}
									return true;
								}
								else
								{
									if (!(cmd == "CONFIRM_AREA_CHANGE"))
									{
										goto IL_3C72;
									}
									X.dl("WM " + rER._1 + " に移動します...", null, false, false);
									WholeMapItem wholeDescriptionByName = this.WM.GetWholeDescriptionByName(rER._1, false);
									object obj;
									UiWarpConfirm.CTYPE ctype = UiWarpConfirm.checkUseConfirm(this.WM.CurWM, wholeDescriptionByName, out obj, null);
									if (ctype != UiWarpConfirm.CTYPE.OFFLINE)
									{
										EV.initWaitFn(new GameObject("UiConfirmAreaChange").AddComponent<UiWarpConfirm>().Init(ctype, obj, wholeDescriptionByName), 0);
									}
									else
									{
										EV.getVariableContainer().define("_result", "1", true);
									}
									return true;
								}
							}
							else if (num <= 404121968U)
							{
								if (num != 394793250U)
								{
									if (num != 404121968U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GETITEM"))
									{
										goto IL_3C72;
									}
									goto IL_1841;
								}
								else
								{
									if (!(cmd == "DANGER_ADDITIONAL"))
									{
										goto IL_3C72;
									}
									int num2 = rER.Int(1, 5);
									this.NightCon.addAdditionalDangerLevel(ref num2, 0, true);
									return true;
								}
							}
							else if (num != 448273782U)
							{
								if (num != 483038532U)
								{
									if (num != 578731911U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "TOUCHITEM"))
									{
										goto IL_3C72;
									}
									NelItem byId = NelItem.GetById(rER._1, true);
									if (byId == null)
									{
										ER.de("不明なアイテム id:" + rER._1);
										return true;
									}
									byId.touchObtainCount();
									return true;
								}
								else
								{
									if (!(cmd == "DEFINE_WHOLEMAP"))
									{
										goto IL_3C72;
									}
									ER.VarCon.define(TX.valid(rER._1) ? rER._1 : "_", (this.nM2D.WM.CurWM != null) ? this.nM2D.WM.CurWM.text_key : "", true);
									return true;
								}
							}
							else
							{
								if (!(cmd == "PRE_FLUSH_MAP"))
								{
									goto IL_3C72;
								}
								this.WDR.flush();
								return true;
							}
						}
						else if (num <= 739891850U)
						{
							if (num <= 666278659U)
							{
								if (num <= 612682214U)
								{
									if (num != 585959919U)
									{
										if (num != 612682214U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "OPEN_REELBOX_ACTIVATE_ONE"))
										{
											goto IL_3C72;
										}
										if (TX.valid(rER._3))
										{
											EV.getVariableContainer().define(rER._3, "0", true);
										}
										NelItem byId2 = NelItem.GetById(rER._1, true);
										if (byId2 == null || !byId2.is_reelmbox)
										{
											rER.tError("不明なアイテム: " + rER._1);
											return true;
										}
										ReelManager.ItemReelContainer ir = ReelManager.GetIR(byId2);
										if (ir == null)
										{
											rER.tError("不明なリール: " + rER._1);
											return true;
										}
										int num3 = rER.Int(2, 0);
										if (this.IMNG.countItem(byId2, num3, false, false) == 0)
										{
											rER.tError("このアイテムを持っていません: " + rER._1);
											return true;
										}
										ReelManager reelManager = this.IMNG.getReelManager();
										reelManager.clearItemReelCache();
										reelManager.destructGob();
										reelManager.assignCurrentItemReel(ir, false);
										this.IMNG.clearItemReelProgressMem(false).Add(new NelItemEntry(byId2, 1, (byte)num3));
										UiReelManager uiReelManager = reelManager.initUiState(ReelManager.MSTATE.PREPARE, null, true);
										uiReelManager.create_strage = true;
										this.IMNG.initItemReelUI(uiReelManager);
										uiReelManager.prepareMBoxDrawer();
										uiReelManager.digest_var_name = rER._3;
										uiReelManager.AddReelSpeedRecipeEffect(rER.Nm(4, 0f));
										EV.initWaitFn(reelManager, 0);
										return true;
									}
									else
									{
										if (!(cmd == "TX_BOARD_HIDE"))
										{
											goto IL_3C72;
										}
										this.IMNG.hideWindows();
										return true;
									}
								}
								else if (num != 648008802U)
								{
									if (num != 666278659U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GETMONEY_BOX"))
									{
										goto IL_3C72;
									}
								}
								else
								{
									if (!(cmd == "PR_OMORASHI"))
									{
										goto IL_3C72;
									}
									PR pr = M2EventCommand.EvMV as PR;
									if (pr == null)
									{
										return rER.tError("M2EventCommand.EvMV が PR ではない");
									}
									pr.JuiceCon.SplashFromEvent(rER);
									return true;
								}
							}
							else if (num <= 706067804U)
							{
								if (num != 675895584U)
								{
									if (num != 706067804U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "INIT_ITEM_REEL"))
									{
										goto IL_3C72;
									}
									if (this.NightCon.isUiActive())
									{
										this.NightCon.UiDg.deactivate(true);
									}
									ReelManager.ItemReelContainer ir2 = ReelManager.GetIR(rER._1, false, false);
									if (ir2 == null)
									{
										X.de("アイテムリール取得失敗:" + rER._1, null);
										return true;
									}
									ReelManager reelManager2 = new ReelManager(this.IMNG.getReelManager()).assignCurrentItemReel(ir2, false);
									UiReelManager uiReelManager2 = reelManager2.initUiState(ReelManager.MSTATE.OPENING_AUTO, null, true);
									if (!rER._B2)
									{
										uiReelManager2.autodecide_progressable = false;
									}
									if (base.curMap != null && TX.valid(rER._3))
									{
										reelManager2.assignDropLp(base.curMap.getLabelPoint(rER._3));
									}
									EV.initWaitFn(reelManager2, 0);
									return true;
								}
								else
								{
									if (!(cmd == "MANA_CLEAR"))
									{
										goto IL_3C72;
									}
									this.Mana.ClearAll();
									return true;
								}
							}
							else if (num != 723036878U)
							{
								if (num != 738197758U)
								{
									if (num != 739891850U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "PR_BREATH"))
									{
										goto IL_3C72;
									}
									if (!(M2EventCommand.EvMV is PR))
									{
										rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
									}
									else
									{
										(M2EventCommand.EvMV as PR).VO.breath_key = rER._1 ?? "";
									}
									return true;
								}
								else
								{
									if (!(cmd == "PE"))
									{
										goto IL_3C72;
									}
									POSTM postm;
									if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
									{
										rER.tError("不明なPOSTM: " + rER._1);
										return true;
									}
									int num4 = rER.Int(2, 40);
									if (num4 < 0)
									{
										if (this.OPEevent == null)
										{
											return true;
										}
										PostEffectItem postEffectItem;
										if (this.OPEevent.TryGetValue(postm, out postEffectItem))
										{
											num4 = rER.Int(3, (int)postEffectItem.z);
											rER.Nm(4, postEffectItem.x);
											postEffectItem.deactivate(false);
											this.OPEevent.Remove(postm);
										}
									}
									else
									{
										if (this.OPEevent == null)
										{
											this.OPEevent = new BDic<POSTM, PostEffectItem>(1);
											IN.addRunner(this);
										}
										PostEffectItem postEffectItem2;
										if (!this.OPEevent.TryGetValue(postm, out postEffectItem2))
										{
											float num5 = rER.Nm(3, 1f);
											postEffectItem2 = this.nM2D.PE.setPE(postm, (float)num4, num5, 0);
											if (postEffectItem2 != null)
											{
												this.OPEevent[postm] = postEffectItem2;
											}
										}
										else
										{
											float num6 = rER.Nm(3, postEffectItem2.x);
											postEffectItem2.x = num6;
											postEffectItem2.z = (float)num4;
										}
									}
									return true;
								}
							}
							else
							{
								if (!(cmd == "SAVE_SAFEAREA_DEPERTURE"))
								{
									goto IL_3C72;
								}
								COOK.getCurrentFile().safe_area_memory = rER._1 + " " + rER._2;
								return true;
							}
						}
						else if (num <= 908082156U)
						{
							if (num <= 795641719U)
							{
								if (num != 744731785U)
								{
									if (num != 795641719U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "BENCH_RELOAD"))
									{
										goto IL_3C72;
									}
									UiBenchMenu.defineEvents(this.PlayerNoel, rER.getB(1, false));
									return true;
								}
								else
								{
									if (!(cmd == "NEL_MAP_TRANSFER"))
									{
										goto IL_3C72;
									}
									M2LpMapTransferBase.executeTransfer(this.nM2D.Get(rER._1, false), rER._2, rER._3);
									if (base.curMap != null)
									{
										EV.initWaitFn(base.curMap, 0);
									}
									return true;
								}
							}
							else if (num != 799739536U)
							{
								if (num != 908082156U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "UIALERT_ITEMHOLDOVER"))
								{
									goto IL_3C72;
								}
								NelItem byId3 = NelItem.GetById(rER._1, true);
								if (byId3 == null)
								{
									ER.de("不明なアイテム id:" + rER._1);
									return true;
								}
								UILog.Instance.AddAlert(TX.GetA("Alert_item_holdover", byId3.getLocalizedName(rER.Int(2, 0))), UILogRow.TYPE.ALERT);
								return true;
							}
							else
							{
								if (!(cmd == "SER_APPLY_NEAR_PEE"))
								{
									goto IL_3C72;
								}
								PR pr2 = M2EventCommand.EvMV as PR;
								if (pr2 == null)
								{
									return rER.tError("M2EventCommand.EvMV が PR ではない");
								}
								pr2.JuiceCon.SerApplyNearPee(rER.Int(1, 0));
								return true;
							}
						}
						else if (num <= 950167350U)
						{
							if (num != 925299078U)
							{
								if (num != 950167350U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "BENCH_SITDOWN"))
								{
									goto IL_3C72;
								}
								M2EventCommand.saveMv();
								this.NightCon.UiDg.deactivate(true);
								rER.tError(M2EventCommand.focusEventMover(rER._1, false));
								if (M2EventCommand.EvMV == null)
								{
									rER.tError("移動スクリプト定義対象 M2Mover 未定義");
								}
								else if (M2EventCommand.EvMV is PRMain)
								{
									PRMain prmain = M2EventCommand.EvMV as PRMain;
									int num7 = (int)rER._N2;
									M2Chip m2Chip = ((num7 == -1000) ? prmain.getNearBench(true, true) : base.curMap.findChip(num7, (int)rER._N3, "bench"));
									if (m2Chip != null)
									{
										NelChipBench nelChipBench = m2Chip as NelChipBench;
										if (nelChipBench != null)
										{
											prmain.initBenchSitDown(nelChipBench, rER._N4 != 0f, false);
										}
										else
										{
											rER.tError(string.Concat(new string[]
											{
												"対象ベンチが座標 ",
												rER._N2.ToString(),
												",",
												rER._N3.ToString(),
												"に存在しませんでした"
											}));
										}
									}
									else
									{
										rER.tError(string.Concat(new string[]
										{
											"対象ベンチが座標 ",
											rER._N2.ToString(),
											",",
											rER._N3.ToString(),
											"に存在しませんでした"
										}));
									}
								}
								else
								{
									rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
								}
								M2EventCommand.restoreMv();
								return true;
							}
							else
							{
								if (!(cmd == "MAPTITLE_HIDE"))
								{
									goto IL_3C72;
								}
								this.nM2D.MapTitle.deactivate(false, true);
								return true;
							}
						}
						else if (num != 969352503U)
						{
							if (num != 1015950516U)
							{
								if (num != 1017903279U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "WEATHER_PROGRESS"))
								{
									goto IL_3C72;
								}
								if (rER._B1 && this.nM2D.debug_listener_created && this.NightCon.debug_lock_weather)
								{
									return true;
								}
								this.NightCon.weatherShuffle();
								return true;
							}
							else
							{
								if (!(cmd == "ACHIVE_SET"))
								{
									goto IL_3C72;
								}
								COOK.setAchiveCommandEval(rER._1, rER._2);
								return true;
							}
						}
						else
						{
							if (!(cmd == "NIGHTINGALE_SHUFFLE"))
							{
								goto IL_3C72;
							}
							this.WDR.getNightingale().blurDecided(rER._1);
							return true;
						}
						int num8 = rER.Int(1, 1);
						CoinStorage.addCount(num8, CoinStorage.CTYPE.GOLD, true);
						if (rER.cmd == "GETMONEY_BOX")
						{
							this.IMNG.get_DescBox().addTaskFocusMoney(num8);
						}
						return true;
					}
					if (num <= 1488682444U)
					{
						if (num <= 1153231774U)
						{
							if (num <= 1098978204U)
							{
								if (num <= 1089304152U)
								{
									if (num != 1039455231U)
									{
										if (num != 1089304152U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "FORCE_GAMEOVER_INIT"))
										{
											goto IL_3C72;
										}
										this.nM2D.initGameOver();
										return true;
									}
									else
									{
										if (!(cmd == "SF_SET"))
										{
											goto IL_3C72;
										}
										COOK.setSFcommandEval(rER._1, rER._2);
										return true;
									}
								}
								else if (num != 1089914396U)
								{
									if (num != 1098978204U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "EP_STATE_CLEAR"))
									{
										goto IL_3C72;
									}
									if (base.curMap.Pr is PR)
									{
										EpManager epCon = (base.curMap.Pr as PR).EpCon;
										epCon.newGame();
										epCon.fineCounter();
									}
									return true;
								}
								else
								{
									if (!(cmd == "CONFIRM_LOAD_SUCCESS"))
									{
										goto IL_3C72;
									}
									if (COOK.error_loaded_index >= 0)
									{
										this.change_scene_on_ev_quit = "SceneTitle";
										EV.getVariableContainer().define("_result", "0", true);
									}
									else
									{
										EV.getVariableContainer().define("_result", "-1", true);
									}
									return true;
								}
							}
							else if (num <= 1113227260U)
							{
								if (num != 1102987273U)
								{
									if (num != 1113227260U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "UIP_VALOTIZE"))
									{
										goto IL_3C72;
									}
									if (rER._B1)
									{
										UIBase.FlgUiEffectDisable.Rem("EVENT");
									}
									else
									{
										UIBase.FlgUiEffectDisable.Add("EVENT");
									}
									return true;
								}
								else
								{
									if (!(cmd == "HOUSE_FOOTBELL"))
									{
										goto IL_3C72;
									}
									if (rER.clength <= 2)
									{
										return rER.tError("サウンド名を列挙すること");
									}
									M2CImgDrawerFootBell.initializeBellPosition(rER._1, rER.slice(2, -1000));
									return true;
								}
							}
							else if (num != 1137130770U)
							{
								if (num != 1153231774U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "DEFINE_SF"))
								{
									goto IL_3C72;
								}
								ER.VarCon.define(rER._1, COOK.getSF(rER._2).ToString(), true);
								return true;
							}
							else
							{
								if (!(cmd == "MV_CURE"))
								{
									goto IL_3C72;
								}
								float n = rER._N1;
								float n2 = rER._N2;
								float n3 = rER._N3;
								if (!(M2EventCommand.EvMV is M2Attackable))
								{
									return rER.tError("M2EventCommand.EvMV が M2Attackable ではない");
								}
								M2Attackable m2Attackable = M2EventCommand.EvMV as M2Attackable;
								if (n > 0f)
								{
									m2Attackable.cureHp((int)n);
								}
								if (n2 > 0f)
								{
									m2Attackable.cureMp((int)n2);
								}
								if (m2Attackable is PR)
								{
									if (n3 != 0f)
									{
										(m2Attackable as PR).CureBench();
									}
									(m2Attackable as PR).recheck_emot = true;
								}
								else if (m2Attackable is NelEnemy && n3 != 0f)
								{
									(m2Attackable as NelEnemy).getSer().CureAll(true);
								}
								return true;
							}
						}
						else if (num <= 1231243708U)
						{
							if (num <= 1188747710U)
							{
								if (num != 1164727111U)
								{
									if (num != 1188747710U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "MIST_CLEAR"))
									{
										goto IL_3C72;
									}
									if (this.nM2D.MIST != null)
									{
										this.nM2D.MIST.clear(true);
									}
									return true;
								}
								else
								{
									if (!(cmd == "PR_ABSORB_EV_ASSIGN"))
									{
										goto IL_3C72;
									}
									EV.getVariableContainer().define("_absorb_result", "0", true);
									if (!(M2EventCommand.EvMV is PR))
									{
										rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
									}
									else if ((M2EventCommand.EvMV as PR).eventAbsorbBind(rER))
									{
										EV.getVariableContainer().define("_absorb_result", "1", true);
									}
									return true;
								}
							}
							else if (num != 1199814306U)
							{
								if (num != 1231243708U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "NEL_EXECUTE_FAST_TRAVEL"))
								{
									goto IL_3C72;
								}
								Map2d map2d2 = this.nM2D.Get(rER._1, true);
								if (map2d2 == null)
								{
									rER.tError("マップが見つかりません: " + rER._1);
									return true;
								}
								M2LpMapTransferBase.executeTransferFastTravel(map2d2, rER.Int(2, 0), rER.Int(3, 0), rER.Int(4, 40));
								return true;
							}
							else
							{
								if (!(cmd == "QUEST_REMOVE"))
								{
									goto IL_3C72;
								}
								this.QUEST.remove(rER._1);
								return true;
							}
						}
						else if (num <= 1385832824U)
						{
							if (num != 1297749395U)
							{
								if (num != 1385832824U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "TX_BOARD_LOCK"))
								{
									goto IL_3C72;
								}
								ItemDescBox descBox = this.IMNG.get_DescBox();
								if (descBox.isActive() && descBox.is_focus)
								{
									descBox.lock_input_focus = true;
								}
								return true;
							}
							else
							{
								if (!(cmd == "PR_OUTFIT"))
								{
									goto IL_3C72;
								}
								if (!(M2EventCommand.EvMV is PRNoel))
								{
									return rER.tError("M2EventCommand.EvMV が PR ではない");
								}
								PRNoel prnoel = M2EventCommand.EvMV as PRNoel;
								PRNoel.OUTFIT outfit;
								if (Enum.TryParse<PRNoel.OUTFIT>(rER._1, out outfit))
								{
									prnoel.setOutfitType(outfit, false, true);
									return true;
								}
								return rER.tError("不明なEnum " + rER._1);
							}
						}
						else if (num != 1465751184U)
						{
							if (num != 1469531706U)
							{
								if (num != 1488682444U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "WAIT_PR_EXPLODE_BURST"))
								{
									goto IL_3C72;
								}
								EV.initWaitFn(this.PlayerNoel.getWaitListenerNoelBurst(), 0);
								return true;
							}
							else
							{
								if (!(cmd == "CFG_SET"))
								{
									goto IL_3C72;
								}
								UiCFG.changeConfigValue(rER._1, rER.Nm(2, 0f), null);
								return true;
							}
						}
						else
						{
							if (!(cmd == "GQ_FLUSH_ITEMCOLLECT"))
							{
								goto IL_3C72;
							}
							this.GUILD.flushWholeMapSwitchingItemCollect(rER._1);
							return true;
						}
					}
					else if (num <= 1684521277U)
					{
						if (num <= 1545025978U)
						{
							if (num <= 1512790309U)
							{
								if (num != 1497418882U)
								{
									if (num != 1512790309U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "ALLOW_FASTTRAVEL"))
									{
										goto IL_3C72;
									}
									this.nM2D.FlgFastTravelDeclined.Rem("EVENT");
									return true;
								}
								else
								{
									if (!(cmd == "BENCH_UIPIC_SLIDE"))
									{
										goto IL_3C72;
									}
									bool flag = rER.Nm(1, 1f) != 0f;
									UIBase.Instance.gameMenuSlide(flag, false);
									UIBase.Instance.gameMenuBenchSlide(flag, false);
									return true;
								}
							}
							else if (num != 1523898194U)
							{
								if (num != 1545025978U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "GETITEM_BOX"))
								{
									goto IL_3C72;
								}
							}
							else
							{
								if (!(cmd == "ENGINE_NNEA"))
								{
									goto IL_3C72;
								}
								MvNelNNEAListener.ReadEvtS(ER, rER, base.curMap);
								return true;
							}
						}
						else if (num <= 1637866189U)
						{
							if (num != 1584889086U)
							{
								if (num != 1637866189U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "PR_CURE"))
								{
									goto IL_3C72;
								}
								bool b = rER._B1;
								bool b2 = rER._B2;
								bool b3 = rER._B3;
								bool b4 = rER._B4;
								bool b5 = rER._B5;
								int num9 = rER.Int(6, 0);
								for (int i = base.curMap.count_players - 1; i >= 0; i--)
								{
									M2MoverPr pr3 = base.curMap.getPr(i);
									if (b)
									{
										pr3.cureHp((int)pr3.get_maxhp());
									}
									if (pr3 is PR)
									{
										PR pr4 = pr3 as PR;
										if (b)
										{
											pr4.DMG.setHpCrack(0);
										}
										if (b2)
										{
											pr4.cureFull(true, true, b4, false);
										}
										else if (b4)
										{
											pr4.EggCon.clear(true);
										}
										if (b3)
										{
											pr4.CureBench();
										}
										pr4.recheck_emot = true;
										if (b && b2)
										{
											pr4.JuiceCon.cureFromEvent(15);
										}
										if (num9 > 0)
										{
											pr4.cureSerDrunk1((float)num9);
										}
										pr4.Ser.checkSer();
									}
									else if (b2)
									{
										pr3.cureMp((int)pr3.get_maxmp());
									}
								}
								if (b5)
								{
									UiBenchMenu.executeOtherCommand("shower_clean_cure_cloth", false);
								}
								return true;
							}
							else
							{
								if (!(cmd == "ITEMMNG_POP_BYTES"))
								{
									goto IL_3C72;
								}
								ByteArray eventCacheBa = this.IMNG.EventCacheBa;
								if (eventCacheBa != null)
								{
									eventCacheBa.position = 0UL;
									this.IMNG.readBinaryFrom(eventCacheBa, false, true, false);
									this.IMNG.digestDropObjectCache();
								}
								else
								{
									X.de("アイテムキャッシュ Ba がありません", null);
								}
								return true;
							}
						}
						else if (num != 1648323204U)
						{
							if (num != 1669423574U)
							{
								if (num != 1684521277U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "DARKSPOT"))
								{
									goto IL_3C72;
								}
								goto IL_38F3;
							}
							else
							{
								if (!(cmd == "NIGHTINGALE_SET"))
								{
									goto IL_3C72;
								}
								Map2d map2d3 = this.nM2D.Get(rER._1, false);
								if (map2d3 != null && SCN.isWNpcEnable(this.nM2D, WanderingManager.TYPE.NIG))
								{
									this.WDR.getNightingale().setCurrentPos(map2d3, false);
								}
								return true;
							}
						}
						else
						{
							if (!(cmd == "REEL_FLUSH"))
							{
								goto IL_3C72;
							}
							if (this.NightCon.isUiActive())
							{
								this.NightCon.UiDg.deactivate(true);
							}
							List<ItemStorage.IRow> list = new List<ItemStorage.IRow>();
							ReelManager reelManager3 = this.IMNG.getReelManager();
							if (this.IMNG.getInventory().getItemCountFn((ItemStorage.IRow IR) => IR.Data.is_reelmbox, list) > 0)
							{
								List<NelItemEntry> list2 = NelItemEntry.Clone(list);
								reelManager3.clearItemReelCache();
								reelManager3.assignCurrentItemReel(list2, false, true);
								this.IMNG.getInventory().Reduce(list2);
								UiReelManager uiReelManager3 = reelManager3.initUiState(rER._B1 ? ReelManager.MSTATE.REMOVE_REELS : ReelManager.MSTATE.OPENING, null, true);
								uiReelManager3.manual_deactivatable = false;
								uiReelManager3.after_clearreels = true;
								uiReelManager3.create_strage = true;
								uiReelManager3.no_draw_hidescreen = this.GameOver != null && this.GameOver.EvtWait(false);
								uiReelManager3.play_snd = !uiReelManager3.no_draw_hidescreen;
								uiReelManager3.prepareMBoxDrawer();
								if (rER._B2)
								{
									uiReelManager3.UiPictureStabilize();
								}
							}
							else if (reelManager3.getReelVector().Count > 0)
							{
								UiReelManager uiReelManager4 = reelManager3.initUiState(ReelManager.MSTATE.REMOVE_REELS, null, true);
								uiReelManager4.after_clearreels = true;
								uiReelManager4.create_strage = true;
								uiReelManager4.no_draw_hidescreen = this.GameOver != null && this.GameOver.EvtWait(false);
								uiReelManager4.play_snd = !uiReelManager4.no_draw_hidescreen;
								if (rER._B2)
								{
									uiReelManager4.UiPictureStabilize();
								}
							}
							else
							{
								this.IMNG.getReelManager().clearReels(false, true, true);
							}
							return true;
						}
					}
					else if (num <= 1783620490U)
					{
						if (num <= 1698386919U)
						{
							if (num != 1696697169U)
							{
								if (num != 1698386919U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "INIT_MAP_BGM"))
								{
									goto IL_3C72;
								}
								Map2d map2d4 = this.nM2D.Get(rER._1, false);
								if (map2d4 == null)
								{
									return rER.tError("Map2d が不明: " + rER._1);
								}
								this.nM2D.initMapBgm(map2d4, false);
								return true;
							}
							else
							{
								if (!(cmd == "REMITEM_NOANNOUNCE"))
								{
									goto IL_3C72;
								}
								goto IL_19BA;
							}
						}
						else if (num != 1780750093U)
						{
							if (num != 1783620490U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "WA_DEPERTURE"))
							{
								goto IL_3C72;
							}
							this.nM2D.WA.depertAssign(rER._1, rER._2, rER._3, rER._4);
							return true;
						}
						else
						{
							if (!(cmd == "WAIT_PR_EXPLODE_MAGIC"))
							{
								goto IL_3C72;
							}
							EV.initWaitFn(this.PlayerNoel.getWaitListenerNoelExplodeMagic(), 0);
							return true;
						}
					}
					else if (num <= 1789280585U)
					{
						if (num != 1788732065U)
						{
							if (num != 1789280585U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "PR_ACTIVATE_THROW_RAY"))
							{
								goto IL_3C72;
							}
							if (!(M2EventCommand.EvMV is PR))
							{
								rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
							}
							else
							{
								(M2EventCommand.EvMV as PR).activateThrowRayForEvent(rER._B1);
							}
							return true;
						}
						else if (!(cmd == "GETITEM_NOANNOUNCE"))
						{
							goto IL_3C72;
						}
					}
					else if (num != 1878547986U)
					{
						if (num != 1879117195U)
						{
							if (num != 1894169261U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "REMSKILL_NOANNOUNCE"))
							{
								goto IL_3C72;
							}
							goto IL_1C28;
						}
						else
						{
							if (!(cmd == "UI_FRONTLOG"))
							{
								goto IL_3C72;
							}
							if (rER.Nm(1, 1f) != 0f)
							{
								this.nM2D.Ui.FlgFrontLog.Add("EVENT");
							}
							else
							{
								this.nM2D.Ui.FlgFrontLog.Rem("EVENT");
							}
							return true;
						}
					}
					else
					{
						if (!(cmd == "PR_GACHA"))
						{
							goto IL_3C72;
						}
						PR pr5 = M2EventCommand.EvMV as PR;
						if (pr5 == null)
						{
							return rER.tError("M2EventCommand.EvMV が PR ではない");
						}
						PrGachaItem.TYPE type;
						if (!FEnum<PrGachaItem.TYPE>.TryParse(rER._1, out type, true))
						{
							return rER.tError("不正なGacha TYPE:" + rER._1);
						}
						int num10 = rER.Int(2, 0);
						if (num10 <= 0)
						{
							return rER.tError("不正なtap_count:" + rER._2);
						}
						M2PrGachaEventHandler m2PrGachaEventHandler = new M2PrGachaEventHandler(pr5, type, num10);
						m2PrGachaEventHandler.Init(rER);
						EV.initWaitFn(m2PrGachaEventHandler, 0);
						return true;
					}
					IL_1841:
					NelItem byId4 = NelItem.GetById(rER._1, true);
					if (byId4 == null)
					{
						ER.de("不明なアイテム id:" + rER._1);
						return true;
					}
					int num11 = (int)rER.Nm(2, 1f);
					if (num11 < 0)
					{
						num11 = byId4.stock;
					}
					this.nM2D.Ui.FlgFrontLog.Add("EVENT");
					int num12 = (int)rER.Nm(3, 0f);
					this.IMNG.getItem(byId4, num11, num12, rER.cmd != "GETITEM_NOANNOUNCE", rER.cmd != "GETITEM_SUPPLIER", false, false);
					if (rER.cmd == "GETITEM_BOX")
					{
						ItemDescBox descBox2 = this.IMNG.get_DescBox();
						if (byId4.is_enhancer)
						{
							descBox2.addTaskFocus(ENHA.Get(byId4), false);
						}
						else if (TX.isStart(byId4.key, "skillbook_", 0))
						{
							descBox2.addTaskFocus(SkillManager.Get(byId4), false);
						}
						else
						{
							List<NelItemEntry> list3 = new List<NelItemEntry>(1)
							{
								new NelItemEntry(byId4, num11, (byte)num12)
							};
							descBox2.addTaskFocus(list3);
						}
					}
					if (byId4.key == "enhancer_slot")
					{
						ENHA.fineEnhancerStorage(this.IMNG.getInventoryPrecious(), this.IMNG.getInventoryEnhancer());
					}
					if (byId4.key == "recipe_collection")
					{
						this.IMNG.has_recipe_collection = true;
					}
					return true;
				}
				else
				{
					if (num <= 3140054473U)
					{
						if (num <= 2463182076U)
						{
							if (num <= 2268747045U)
							{
								if (num <= 2071059468U)
								{
									if (num <= 2035097996U)
									{
										if (num != 2034944466U)
										{
											if (num != 2035097996U)
											{
												goto IL_3C72;
											}
											if (!(cmd == "DANGER_LEVEL_INIT_BOX"))
											{
												goto IL_3C72;
											}
											M2LpMapTransferWarp m2LpMapTransferWarp = base.curMap.getPoint(rER._2, false) as M2LpMapTransferWarp;
											if (m2LpMapTransferWarp == null)
											{
												return true;
											}
											WholeMapItem byTextKey2 = this.WM.GetByTextKey(rER._1);
											List<string> list4 = new List<string>(2);
											if (SCN.getMovableHomeWholeMap(list4) > 0 || byTextKey2.reached_night_level >= 16)
											{
												EV.initWaitFn(new GameObject("UiDangerLevelInitBox").AddComponent<UiDangerLevelInitBox>().Init(byTextKey2, list4, m2LpMapTransferWarp), 0);
											}
											else
											{
												EV.getVariableContainer().define("_result", "0", true);
											}
											return true;
										}
										else
										{
											if (!(cmd == "PR_VOICE"))
											{
												goto IL_3C72;
											}
											PR pr6;
											if (TX.valid(rER._2))
											{
												pr6 = M2EventCommand.getEventTargetMover(rER._2) as PR;
											}
											else
											{
												pr6 = M2EventCommand.EvMV as PR;
											}
											if (pr6 == null)
											{
												rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
											}
											else
											{
												pr6.playVo(rER._1, false, false);
											}
											return true;
										}
									}
									else if (num != 2054504399U)
									{
										if (num != 2071059468U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "BENCHMARK"))
										{
											goto IL_3C72;
										}
										TX.eval("SkillEnable[guard]", "");
										Bench.mark(rER._1, false, false);
										if (rER._1 == "txcheck")
										{
											int num13 = 20000;
											for (int j = 0; j < num13; j++)
											{
												TX.eval("NoelCasting[WHITEARROW]", "");
												TX.eval("SkillEnable[guard]", "");
											}
										}
										Bench.mark("", false, false);
										return true;
									}
									else if (!(cmd == "SETMAGIC_NOMANA"))
									{
										goto IL_3C72;
									}
								}
								else if (num <= 2163111453U)
								{
									if (num != 2090980678U)
									{
										if (num != 2163111453U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "PREPARE_ENEMY_RESOURCES"))
										{
											goto IL_3C72;
										}
										for (int k = 1; k < rER.clength; k++)
										{
											NDAT.getResources(this.nM2D, rER.getIndex(k));
										}
										return true;
									}
									else
									{
										if (!(cmd == "SER_APPLY"))
										{
											goto IL_3C72;
										}
										SER ser;
										if (!FEnum<SER>.TryParse(rER._1, out ser, true))
										{
											return rER.tError("不明なSER: " + rER._1);
										}
										int num14 = rER.Int(2, -1);
										int num15 = rER.Int(3, 99);
										if (M2EventCommand.EvMV is PR)
										{
											PR pr7 = M2EventCommand.EvMV as PR;
											pr7.Ser.Add(ser, num14, num15, false);
											pr7.recheck_emot = true;
										}
										else
										{
											if (!(M2EventCommand.EvMV is NelEnemy))
											{
												return rER.tError("M2EventCommand.EvMV が Ser を持っていない");
											}
											(M2EventCommand.EvMV as NelEnemy).getSer().Add(ser, num14, num15, false);
										}
										return true;
									}
								}
								else if (num != 2268338539U)
								{
									if (num != 2268747045U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "UI_RESTROOM_MENU"))
									{
										goto IL_3C72;
									}
									UiRestRoom uiRestRoom = UiRestRoom.CreateInstance();
									UiRestRoom.RSTATE rstate = UiRestRoom.RSTATE.OFFLINE;
									FEnum<UiRestRoom.RSTATE>.TryParse(rER._1, out rstate, true);
									uiRestRoom.Init(rstate);
									EV.initWaitFn(uiRestRoom, 0);
									return true;
								}
								else
								{
									if (!(cmd == "WA_RECORD"))
									{
										goto IL_3C72;
									}
									this.nM2D.WA.Touch(rER._1, rER._2, false, true);
									return true;
								}
							}
							else if (num <= 2333218688U)
							{
								if (num <= 2295247786U)
								{
									if (num != 2290201319U)
									{
										if (num != 2295247786U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "EPSITU_B"))
										{
											goto IL_3C72;
										}
										goto IL_23ED;
									}
									else
									{
										if (!(cmd == "TRM_FINE"))
										{
											goto IL_3C72;
										}
										TRMManager.fineExecute(true);
										return true;
									}
								}
								else if (num != 2310641853U)
								{
									if (num != 2333218688U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GM_ALLOW_OPEN"))
									{
										goto IL_3C72;
									}
									this.nM2D.FlagOpenGm.Rem("EV");
									return true;
								}
								else
								{
									if (!(cmd == "SVT_FLUSH"))
									{
										goto IL_3C72;
									}
									BetobetoManager.flushSpecificSvTexture(rER._1);
									return true;
								}
							}
							else if (num <= 2417376634U)
							{
								if (num != 2357528558U)
								{
									if (num != 2417376634U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "TX_BOARD"))
									{
										goto IL_3C72;
									}
									bool flag2 = false;
									string text = NelMSGContainer.checkHereDocument(rER._1, ER.name, "", (ER == rER) ? ER : null, true, null, false);
									if (text == null)
									{
										int num16 = rER._1.IndexOf("*");
										if (num16 == -1)
										{
											TX tx = TX.getTX(rER._1, true, false, null);
											if (tx == null)
											{
												return true;
											}
											text = tx.text;
										}
										else
										{
											using (BList<string> blist = ListBuffer<string>.Pop(0))
											{
												string text4;
												string text5;
												if (num16 == 0)
												{
													string text2 = ER.name;
													string text3 = TX.slice(rER._1, 1);
													text4 = text2;
													text5 = text3;
												}
												else
												{
													string text2 = TX.slice(rER._1, 0, num16);
													string text6 = TX.slice(rER._1, num16 + 1);
													text4 = text2;
													text5 = text6;
												}
												if (NelMSGResource.getContent(text4 + " " + text5, blist, false, false, false))
												{
													text = blist[0];
												}
												else
												{
													text = "<Unknown label: " + rER._1 + ">";
												}
											}
										}
									}
									NelItemManager.POPUP popup = NelItemManager.POPUP.FRAMED_BOARD;
									if (TX.valid(rER._2))
									{
										FEnum<NelItemManager.POPUP>.TryParse(rER._2, out popup, true);
									}
									string _ = rER._3;
									if (_.IndexOf('C') >= 0)
									{
										popup |= NelItemManager.POPUP._TX_CENTER;
									}
									if (_.IndexOf('N') >= 0)
									{
										flag2 = true;
									}
									ItemDescBox.IdbTask idbTask = this.IMNG.showPopUpAbs(popup | NelItemManager.POPUP._FOCUS_MODE, text, 0f, 50f);
									if (TX.valid(rER._4))
									{
										idbTask.position_key = rER._4;
										this.IMNG.fineTaskPosition(idbTask);
									}
									if (!flag2)
									{
										EV.initWaitFn(this.IMNG.get_DescBox(), 0);
									}
									return true;
								}
								else
								{
									if (!(cmd == "DANGER"))
									{
										goto IL_3C72;
									}
									this.NightCon.applyDangerousFromEvent(rER.Int(1, 0), rER._B2, false, false);
									return true;
								}
							}
							else if (num != 2421215947U)
							{
								if (num != 2453517722U)
								{
									if (num != 2463182076U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GETSKILL"))
									{
										goto IL_3C72;
									}
									goto IL_1BC8;
								}
								else
								{
									if (!(cmd == "BENCHMARK2"))
									{
										goto IL_3C72;
									}
									Bench.mark("txcheck2", false, true);
									Bench.P("txcheck2");
									List<string> list5 = new List<string>(1) { "WHITEARROW" };
									int num17 = 5000;
									for (int l = 0; l < num17; l++)
									{
										TX.evalLsnConvert("NoelCasting", list5);
									}
									Bench.mark("", false, false);
									Bench.Pend("txcheck2");
									return true;
								}
							}
							else
							{
								if (!(cmd == "UI_ENABLE"))
								{
									goto IL_3C72;
								}
								this.Ui.setEnabled(true);
								return true;
							}
						}
						else if (num <= 2791652511U)
						{
							if (num <= 2561770047U)
							{
								if (num <= 2499757005U)
								{
									if (num != 2498028900U)
									{
										if (num != 2499757005U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "TITLECALL"))
										{
											goto IL_3C72;
										}
										this.areatitle_hide_progress = false;
										this.nM2D.AreaTitle.init(TX.Get(rER._1, ""), this.WM.CurWM.dark_area, rER.Nm(2, 0f), rER._B3, false);
										return true;
									}
									else
									{
										if (!(cmd == "PR_MASTURB"))
										{
											goto IL_3C72;
										}
										PRMain prmain2 = M2EventCommand.EvMV as PRMain;
										ER.VarCon.define("_masturb_success", "-1", true);
										if (prmain2 == null)
										{
											return rER.tError("M2EventCommand.EvMV が PR ではない");
										}
										M2PrMasturbate m2PrMasturbate = prmain2.initMasturbation(true, false);
										if (m2PrMasturbate != null)
										{
											EV.initWaitFn(m2PrMasturbate, rER.Int(1, 0));
										}
										return true;
									}
								}
								else if (num != 2511127767U)
								{
									if (num != 2561770047U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GET_TRM_REWARD_BOX"))
									{
										goto IL_3C72;
									}
									ItemDescBox descBox3 = this.IMNG.get_DescBox();
									TRMManager.TRMReward currentReward = TRMManager.CurrentReward;
									if (currentReward != null)
									{
										descBox3.addTaskFocus(currentReward);
										TRMManager.CurrentReward = null;
									}
									return true;
								}
								else
								{
									if (!(cmd == "ENABLESKILL"))
									{
										goto IL_3C72;
									}
									goto IL_1C5C;
								}
							}
							else if (num <= 2616714340U)
							{
								if (num != 2599844165U)
								{
									if (num != 2616714340U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "DARKSPOT_DEACTIVATE"))
									{
										goto IL_3C72;
									}
									goto IL_38F3;
								}
								else
								{
									if (!(cmd == "DANGER_MANUAL"))
									{
										goto IL_3C72;
									}
									this.NightCon.changeTo(rER.Nm(1, 0f), (rER.Nm(2, 0f) != 0f) ? 0 : 260);
									return true;
								}
							}
							else if (num != 2650818810U)
							{
								if (num != 2697762179U)
								{
									if (num != 2791652511U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "AUTO_SAVE_BENCH"))
									{
										goto IL_3C72;
									}
									if (base.curMap == null || !(base.curMap.Pr is PR))
									{
										return rER.tError("PR ではない");
									}
									PR pr8 = base.curMap.Pr as PR;
									using (BList<M2Puts> blist2 = ListBuffer<M2Puts>.Pop(1))
									{
										base.curMap.getAllPointMetaPutsTo((int)pr8.x - 1, (int)(pr8.mbottom - 0.25f), 3, 1, blist2, "bench");
										if (blist2.Count == 0)
										{
											return rER.tError("近場にベンチがありません (ファストトラベルの失敗？)");
										}
										if (CFG.autosave_on_bench && SCN.canSave(true))
										{
											COOK.autoSave(this.nM2D, true, false);
										}
										M2BlockColliderContainer.BCCLine lastBCC = pr8.getFootManager().get_LastBCC();
										this.nM2D.CheckPoint.fineFoot(pr8, lastBCC, true);
									}
									return true;
								}
								else
								{
									if (!(cmd == "COOK_ADD_WALK_COUNT"))
									{
										goto IL_3C72;
									}
									Map2d map2d5 = this.nM2D.Get(rER._1, true);
									if (map2d5 != null && this.nM2D.DGN.getDgn(map2d5) != base.Cam.CurDgn)
									{
										if (COOK.map_walk_count >= 30)
										{
											this.nM2D.setFlushMapFlag();
											this.nM2D.setFlushOtherMatFlag();
										}
										else
										{
											COOK.map_walk_count++;
										}
									}
									else if (COOK.map_walk_count >= 50)
									{
										this.nM2D.setFlushMapFlag();
										this.nM2D.setFlushOtherMatFlag();
									}
									else
									{
										COOK.map_walk_count += 4;
									}
									return true;
								}
							}
							else
							{
								if (!(cmd == "PE_FADEINOUT"))
								{
									goto IL_3C72;
								}
								POSTM postm2;
								if (!FEnum<POSTM>.TryParse(rER._1, out postm2, true))
								{
									rER.tError("不明なPOSTM: " + rER._1);
									return true;
								}
								float num18 = rER.Nm(2, 40f);
								float num19 = rER.Nm(3, 40f);
								PostEffectItem postEffectItem3 = this.nM2D.PE.setPEfadeinout(postm2, num18, num19, rER.Nm(4, 1f), rER.Int(5, 0));
								if (postEffectItem3 != null)
								{
									postEffectItem3.fine((int)(num18 * 2f + num19 + 120f));
								}
								return true;
							}
						}
						else if (num <= 2871566880U)
						{
							if (num <= 2806365720U)
							{
								if (num != 2803222434U)
								{
									if (num != 2806365720U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "UI_DISABLE"))
									{
										goto IL_3C72;
									}
									this.Ui.setEnabled(false);
									return true;
								}
								else
								{
									if (!(cmd == "EPSITU_FLUSH"))
									{
										goto IL_3C72;
									}
									this.nM2D.flushLastExSituationTemp();
									return true;
								}
							}
							else if (num != 2863196886U)
							{
								if (num != 2871566880U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "CLEAR_TREASURE_BOX_WM_CACHE"))
								{
									goto IL_3C72;
								}
								this.WM.CurWM.treasureBoxOpened(base.curMap);
								return true;
							}
							else if (!(cmd == "SETMAGIC"))
							{
								goto IL_3C72;
							}
						}
						else if (num <= 2877243756U)
						{
							if (num != 2876823571U)
							{
								if (num != 2877243756U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "REMSKILL"))
								{
									goto IL_3C72;
								}
								goto IL_1C28;
							}
							else
							{
								if (!(cmd == "ITEMMNG_PUSH_BYTES"))
								{
									goto IL_3C72;
								}
								this.IMNG.EventCacheBa = this.IMNG.writeBinaryTo(null);
								return true;
							}
						}
						else if (num != 2978429305U)
						{
							if (num != 3006393793U)
							{
								if (num != 3140054473U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "SF_EVT_SET"))
								{
									goto IL_3C72;
								}
								COOK.setSFcommandEval("EV_" + ER.name, TX.noe(rER._1) ? "1" : rER._1);
								return true;
							}
							else
							{
								if (!(cmd == "DEFEAT_EVENT"))
								{
									goto IL_3C72;
								}
								goto IL_39DA;
							}
						}
						else
						{
							if (!(cmd == "TE_COLORBLINK_"))
							{
								goto IL_3C72;
							}
							try
							{
								rER.tError(M2EventCommand.focusEventMover(rER._1, false));
								if (M2EventCommand.EvMV is M2Attackable)
								{
									(M2EventCommand.EvMV as M2Attackable).TeCon.setColorBlink((float)rER.Int(2, 0), (float)rER.Int(3, 0), rER.Nm(4, 1f), rER.IntE(5, 16777215), rER.Int(6, 0));
								}
							}
							catch
							{
								rER.tError("TE_ パースエラー");
							}
							return true;
						}
						MGKIND mgkind;
						if (FEnum<MGKIND>.TryParse(rER._1, out mgkind, true))
						{
							M2MagicCaster m2MagicCaster = M2EventCommand.EvMV as M2MagicCaster;
							if (m2MagicCaster == null)
							{
								m2MagicCaster = this.PlayerNoel;
							}
							MagicItem magicItem = this.MGC.setMagic(m2MagicCaster, mgkind, ((rER._2 == "EN") ? MGHIT.EN : ((rER._2 == "EN|PR" || rER._2 == "PR|EN") ? MGHIT.BERSERK : MGHIT.PR)) | MGHIT.IMMEDIATE);
							Vector2 pos = base.curMap.getPos(rER._3, rER.Nm(4, 0f), rER.Nm(5, 0f), null);
							magicItem.sx = pos.x;
							magicItem.sy = pos.y;
							if (rER.cmd == "SETMAGIC_NOMANA")
							{
								magicItem.reduce_mp = 0f;
							}
						}
						else
						{
							rER.tError("MGKIND 不明:" + rER._1);
						}
						return true;
					}
					if (num <= 3569452120U)
					{
						if (num <= 3236607995U)
						{
							if (num <= 3185205341U)
							{
								if (num <= 3154114209U)
								{
									if (num != 3141388709U)
									{
										if (num != 3154114209U)
										{
											goto IL_3C72;
										}
										if (!(cmd == "GM_DENY_OPEN"))
										{
											goto IL_3C72;
										}
										this.nM2D.FlagOpenGm.Add("EV");
										return true;
									}
									else
									{
										if (!(cmd == "NEED_FINE_DEPERTURE"))
										{
											goto IL_3C72;
										}
										return true;
									}
								}
								else if (num != 3154698976U)
								{
									if (num != 3185205341U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "GETSKILL_NOANNOUNCE"))
									{
										goto IL_3C72;
									}
									goto IL_1BC8;
								}
								else
								{
									if (!(cmd == "NOEL_OUTFIT_TURNBACK"))
									{
										goto IL_3C72;
									}
									if (this.PlayerNoel.outfit_type == PRNoel.OUTFIT.BABYDOLL)
									{
										this.PlayerNoel.setOutfitType(PRNoel.OUTFIT.NORMAL, false, true);
									}
									return true;
								}
							}
							else if (num <= 3197668728U)
							{
								if (num != 3195761152U)
								{
									if (num != 3197668728U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "KILL_PR_MAGIC"))
									{
										goto IL_3C72;
									}
									this.MGC.killAllPlayerMagic(null, null);
									return true;
								}
								else
								{
									if (!(cmd == "PR_BETO"))
									{
										goto IL_3C72;
									}
									if (!(M2EventCommand.EvMV is PR))
									{
										rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
									}
									else if (!(M2EventCommand.EvMV as PR).BetoMng.addBetoFromEv(rER._1))
									{
										rER.tError("不明なBetoInfo:" + rER._1);
									}
									return true;
								}
							}
							else if (num != 3199224305U)
							{
								if (num != 3215738269U)
								{
									if (num != 3236607995U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "ENGINE"))
									{
										goto IL_3C72;
									}
									M2AttackableEventManipulatable.ReadEvtS(ER, rER, base.curMap);
									return true;
								}
								else
								{
									if (!(cmd == "HIDE_BLURSC"))
									{
										goto IL_3C72;
									}
									this.nM2D.BlurSc.remFlag("__EVENT");
									return true;
								}
							}
							else if (!(cmd == "QUEST_FINISH"))
							{
								goto IL_3C72;
							}
						}
						else if (num <= 3327087531U)
						{
							if (num <= 3283502208U)
							{
								if (num != 3281843679U)
								{
									if (num != 3283502208U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "REMITEM"))
									{
										goto IL_3C72;
									}
									goto IL_19BA;
								}
								else
								{
									if (!(cmd == "UIALERT"))
									{
										goto IL_3C72;
									}
									UILogRow.TYPE type2 = UILogRow.TYPE.ALERT;
									if (rER.clength >= 3)
									{
										UILogRow.TYPE type3;
										if (FEnum<UILogRow.TYPE>.TryParse(rER._2, out type3, true))
										{
											type2 = type3;
										}
										else
										{
											rER.tError("不明なUILogRow.TYPE: " + rER._2);
										}
									}
									UILog.Instance.AddAlert(TX.Get(rER._1, ""), type2);
									return true;
								}
							}
							else if (num != 3314111417U)
							{
								if (num != 3327087531U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "UI_RESTROOM_MENU_HILIGHT"))
								{
									goto IL_3C72;
								}
								UiRestRoom.setHilight(rER._1);
								return true;
							}
							else
							{
								if (!(cmd == "PUZZ_WARP"))
								{
									goto IL_3C72;
								}
								NelChipBarrierLit.eventWarp(rER.Int(1, -1), rER.Int(2, -1), rER.Int(3, -1));
								return true;
							}
						}
						else if (num <= 3371354388U)
						{
							if (num != 3344825186U)
							{
								if (num != 3371354388U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "REMOVEMAGIC"))
								{
									goto IL_3C72;
								}
								MGKIND mgkind2;
								if (!FEnum<MGKIND>.TryParse(rER._1U, out mgkind2, true))
								{
									rER.tError("MGKIND パースエラー:" + rER._1);
								}
								else
								{
									this.PlayerNoel.Skill.setMagicObtainFlag(mgkind2, false);
									if (mgkind2 == MGKIND.PR_BURST)
									{
										this.PlayerNoel.Skill.BurstSel.fineBurstMagic();
									}
								}
								return true;
							}
							else
							{
								if (!(cmd == "BENCH_CMD_EXECUTE"))
								{
									goto IL_3C72;
								}
								UiBenchMenu.ExecuteBenchCmd(rER._1, rER.Int(2, 0), true, rER._B3);
								return true;
							}
						}
						else if (num != 3479742772U)
						{
							if (num != 3555958544U)
							{
								if (num != 3569452120U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "BENCH_CMD_EXECUTE_O"))
								{
									goto IL_3C72;
								}
								int num20 = UiBenchMenu.executeOtherCommand(rER._1, false);
								if (TX.valid(rER._2))
								{
									EV.getVariableContainer().define(rER._2, num20.ToString(), true);
								}
								return true;
							}
							else
							{
								if (!(cmd == "GQ_FLUSH"))
								{
									goto IL_3C72;
								}
								this.GUILD.flushWholeMapSwitching(this.WM.GetByTextKey(rER._1), this.WM.GetByTextKey(rER._2));
								return true;
							}
						}
						else
						{
							if (!(cmd == "PREPARE_SV_TEXTURE"))
							{
								goto IL_3C72;
							}
							EV.initWaitFn(this.nM2D.PlayerNoel.BetoMng, 0);
							return true;
						}
					}
					else if (num <= 3919384666U)
					{
						if (num <= 3718600314U)
						{
							if (num <= 3674120902U)
							{
								if (num != 3638850237U)
								{
									if (num != 3674120902U)
									{
										goto IL_3C72;
									}
									if (!(cmd == "PR_WATER_DRUNK_MX"))
									{
										goto IL_3C72;
									}
									PR pr9 = M2EventCommand.EvMV as PR;
									if (pr9 == null)
									{
										return rER.tError("M2EventCommand.EvMV が PR ではない");
									}
									pr9.JuiceCon.updateWaterDrunkFromEvent(rER.Int(1, 0));
									return true;
								}
								else
								{
									if (!(cmd == "FLUSHED_MAP"))
									{
										goto IL_3C72;
									}
									UiGameMenu.need_whole_map_reentry = true;
									if (NEL.flushState())
									{
										StoreManager.flushSoldItemsAll();
									}
									this.PlayerNoel.cureMpNotHunger(false);
									this.PlayerNoel.EggCon.worm_total = 0;
									this.NightCon.clearWithoutNightLevel();
									this.NightCon.clearPuppetRevengeCache(true, null);
									this.WM.assignStoreFlushFlag(false, true);
									this.IMNG.getReelManager().flushObtainableReel();
									return true;
								}
							}
							else if (num != 3681775455U)
							{
								if (num != 3718600314U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "DENY_FASTTRAVEL"))
								{
									goto IL_3C72;
								}
								this.nM2D.FlgFastTravelDeclined.Add("EVENT");
								return true;
							}
							else
							{
								if (!(cmd == "WALKCOUNT"))
								{
									goto IL_3C72;
								}
								COOK.map_walk_count = rER.Int(1, COOK.map_walk_count);
								return true;
							}
						}
						else if (num <= 3787413069U)
						{
							if (num != 3724400530U)
							{
								if (num != 3787413069U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "STOREADD"))
								{
									goto IL_3C72;
								}
								if (TX.valid(rER._1))
								{
									StoreManager storeManager = StoreManager.Get(rER._1, true);
									if (storeManager == null)
									{
										rER.tError("不明な店ID: " + rER._1);
									}
									else
									{
										NelItem byId5 = NelItem.GetById(rER._2, true);
										if (byId5 == null)
										{
											rER.tError("不明な店ID: " + rER._1);
										}
										else
										{
											storeManager.Add(byId5, rER.Int(3, 1), rER.Int(4, 0), TX.valid(rER._5) ? rER._5 : "_", false);
										}
									}
								}
								return true;
							}
							else
							{
								if (!(cmd == "GETMAGIC"))
								{
									goto IL_3C72;
								}
								MGKIND mgkind3;
								if (!FEnum<MGKIND>.TryParse(rER._1U, out mgkind3, true))
								{
									rER.tError("MGKIND パースエラー:" + rER._1);
								}
								else
								{
									this.IMNG.get_DescBox().addTaskFocus(mgkind3);
									this.PlayerNoel.Skill.setMagicObtainFlag(mgkind3, true);
								}
								return true;
							}
						}
						else if (num != 3818058978U)
						{
							if (num != 3911183430U)
							{
								if (num != 3919384666U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "SHOW_BLURSC"))
								{
									goto IL_3C72;
								}
								this.nM2D.BlurSc.addFlag("__EVENT");
								return true;
							}
							else
							{
								if (!(cmd == "AUTO_SAVE"))
								{
									goto IL_3C72;
								}
								if (CFG.autosave_on_scenario)
								{
									this.need_autosave = true;
								}
								return true;
							}
						}
						else
						{
							if (!(cmd == "CLOSE_DESCBOX"))
							{
								goto IL_3C72;
							}
							this.IMNG.get_DescBox().clearStack();
							return true;
						}
					}
					else if (num <= 4019167028U)
					{
						if (num <= 4003384557U)
						{
							if (num != 3999105776U)
							{
								if (num != 4003384557U)
								{
									goto IL_3C72;
								}
								if (!(cmd == "DANGER_INITIALIZE_MEMORY"))
								{
									goto IL_3C72;
								}
								this.NightCon.first_battle_dlevel = this.NightCon.getDangerMeterVal(false, false);
								return true;
							}
							else
							{
								if (!(cmd == "DISABLESKILL"))
								{
									goto IL_3C72;
								}
								goto IL_1C5C;
							}
						}
						else if (num != 4013445799U)
						{
							if (num != 4019167028U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "SCN_MANUAL_BGM_REPLACE"))
							{
								goto IL_3C72;
							}
							SCN.manual_bgm_replace = ((rER.clength >= 2) ? rER._1 : null);
							return true;
						}
						else
						{
							if (!(cmd == "TITLE_POS_SHIFT"))
							{
								goto IL_3C72;
							}
							this.nM2D.AreaTitle.finePosPixelShift(rER._N1, rER._N2);
							return true;
						}
					}
					else if (num <= 4071042880U)
					{
						if (num != 4026835330U)
						{
							if (num != 4071042880U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "STOREFLUSH"))
							{
								goto IL_3C72;
							}
							if (TX.valid(rER._1))
							{
								StoreManager storeManager2 = StoreManager.Get(rER._1, true);
								if (storeManager2 == null)
								{
									rER.tError("不明な店ID: " + rER._1);
								}
								else
								{
									storeManager2.need_summon_flush = StoreManager.MODE.FLUSH;
									storeManager2.need_reload_basic = true;
									if (rER._B2)
									{
										storeManager2.countItems();
									}
								}
							}
							else
							{
								StoreManager.FlushWanderingStore(StoreManager.MODE.FLUSH);
							}
							return true;
						}
						else
						{
							if (!(cmd == "DEFAULT_MIST_POSE"))
							{
								goto IL_3C72;
							}
							this.nM2D.default_mist_pose = rER.Int(1, 0);
							return true;
						}
					}
					else if (num != 4189103509U)
					{
						if (num != 4212641147U)
						{
							if (num != 4250681545U)
							{
								goto IL_3C72;
							}
							if (!(cmd == "QUEST_PROGRESS"))
							{
								goto IL_3C72;
							}
						}
						else
						{
							if (!(cmd == "EPSITU"))
							{
								goto IL_3C72;
							}
							goto IL_23ED;
						}
					}
					else
					{
						if (!(cmd == "BENCH_AUTO_EXECUTE"))
						{
							goto IL_3C72;
						}
						PR pr10 = M2EventCommand.EvMV as PR;
						if (pr10 == null)
						{
							return rER.tError("M2EventCommand.EvMV が PR ではない");
						}
						UiBenchMenu.auto_start_stack_disable = false;
						UiBenchMenu.checkStackForceEvent(pr10, true, true);
						return true;
					}
					string text7;
					int num21;
					if (rER.cmd == "QUEST_PROGRESS")
					{
						text7 = rER._3;
						num21 = rER.Int(2, 0);
					}
					else
					{
						text7 = rER._2;
						num21 = 1000;
					}
					this.QUEST.updateQuest(rER._1, num21, text7.IndexOf('H') >= 0, text7.IndexOf('C') >= 0, text7.IndexOf('F') >= 0, text7.IndexOf('X') >= 0);
					return true;
					IL_1BC8:
					PrSkill prSkill = SkillManager.Get(rER._1);
					if (prSkill == null)
					{
						rER.tError("スキルが不明:" + rER._1);
					}
					else
					{
						if (rER.cmd != "GETSKILL_NOANNOUNCE")
						{
							this.IMNG.get_DescBox().addTaskFocus(prSkill, false);
						}
						prSkill.Obtain(rER._B2);
					}
					return true;
					IL_23ED:
					if (base.curMap == null || !(base.curMap.Pr is PR))
					{
						return rER.tError("PR ではない");
					}
					EpSituation situCon = (base.curMap.Pr as PR).EpCon.SituCon;
					if (rER.cmd == "EPSITU")
					{
						situCon.addTempSituation(rER._1, rER.Int(2, 1), false);
					}
					else if (rER.cmd == "EPSITU_B")
					{
						situCon.addTempSituation(rER._1, rER.Int(2, 1), true);
					}
					return true;
				}
				IL_19BA:
				NelItem byId6 = NelItem.GetById(rER._1, true);
				if (byId6 == null)
				{
					ER.de("不明なアイテム id:" + rER._1);
					return true;
				}
				int num22 = rER.Int(2, 1);
				if (num22 < 0)
				{
					num22 = byId6.stock;
				}
				int num23 = rER.Int(3, 0);
				int num24 = this.IMNG.reduceItem(byId6, num22, num23);
				if (num24 > 0 && rER.cmd != "REMITEM_NOANNOUNCE")
				{
					string text8 = "Alert_item_reduced";
					if (TX.valid(rER._4))
					{
						text8 = rER._4;
					}
					UILog.Instance.AddAlert(TX.GetA(text8, byId6.getLocalizedName(num23), num24.ToString()), UILogRow.TYPE.ALERT).setIcon(MTR.AItemIcon[byId6.getIcon(this.IMNG.getHouseInventory(), null)], C32.c2d(byId6.getColor(this.IMNG.getHouseInventory())));
				}
				return true;
				IL_1C28:
				PrSkill prSkill2 = SkillManager.Get(rER._1);
				if (prSkill2 == null)
				{
					rER.tError("スキルが不明:" + rER._1);
				}
				else
				{
					prSkill2.ReleaseObtain();
				}
				return true;
				IL_1C5C:
				PrSkill prSkill3 = SkillManager.Get(rER._1);
				if (prSkill3 == null)
				{
					rER.tError("スキルが不明:" + rER._1);
				}
				else
				{
					bool flag3 = rER.cmd.IndexOf("ENABLESKILL") == 0;
					if (prSkill3.enabled != flag3 && prSkill3.visible)
					{
						prSkill3.enabled = flag3;
						this.PlayerNoel.Skill.resetSkillConnection(false, false, false);
					}
				}
				return true;
				IL_38F3:
				if (M2EventCommand.EvMV == null)
				{
					rER.tError("移動スクリプト定義対象 M2Mover がありません ");
					return true;
				}
				DarkSpotEffect.SPOT spot;
				if (!FEnum<DarkSpotEffect.SPOT>.TryParse(rER._1, out spot, true))
				{
					spot = DarkSpotEffect.SPOT.FILL;
				}
				if (rER.cmd == "DARKSPOT")
				{
					DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.nM2D.DARKSPOT.Set(M2EventCommand.EvMV, spot);
					darkSpotEffectItem.BaseCol = C32.d2c(X.NmUI(rER._2, 4278190080U, false, true));
					darkSpotEffectItem.add_light_color = rER.Nm(3, 0f);
					darkSpotEffectItem.mul_light_color = rER.Nm(4, 1f);
					darkSpotEffectItem.sub_alpha = rER.Nm(5, 0.2f);
				}
				else
				{
					this.nM2D.DARKSPOT.deactivateFor(M2EventCommand.EvMV, spot);
				}
				return true;
				IL_39DA:
				this.PlayerNoel.getSkillManager().killHoldMagic(false, false);
				return base.EvtRead(ER, rER, skipping);
			}
			IL_3C72:
			return this.Ui.fnEvReadUI(ER, rER, skipping) || this.GM.EvtRead(rER) || base.EvtRead(ER, rER, skipping);
		}

		public override bool EvtClose(bool is_end)
		{
			base.EvtClose(is_end);
			if (is_end)
			{
				this.areatitle_hide_progress = true;
				this.deactivatePE(false);
				this.executeAutoSave(false);
				if (this.change_scene_on_ev_quit != null)
				{
					this.nM2D.quitGame(this.change_scene_on_ev_quit);
					this.nM2D.transferring_game_stopping = true;
				}
			}
			return true;
		}

		private void deactivatePE(bool immediate = false)
		{
			if (this.OPEevent != null)
			{
				foreach (KeyValuePair<POSTM, PostEffectItem> keyValuePair in this.OPEevent)
				{
					if (immediate)
					{
						keyValuePair.Value.destruct();
					}
					else
					{
						keyValuePair.Value.deactivate(false);
					}
				}
				this.OPEevent = null;
				IN.remRunner(this);
			}
		}

		public void executeAutoSave(bool force = false)
		{
			if ((this.need_autosave || force) && (!CFG.autosave_on_scenario || COOK.autoSave(this.nM2D, false, false) != null))
			{
				this.need_autosave = false;
			}
		}

		public bool run(float fcnt)
		{
			if (this.OPEevent != null)
			{
				if ((IN.totalframe & 63) == 0)
				{
					foreach (KeyValuePair<POSTM, PostEffectItem> keyValuePair in this.OPEevent)
					{
						if (keyValuePair.Value != null)
						{
							keyValuePair.Value.fine(120);
						}
					}
				}
				return true;
			}
			return false;
		}

		public WholeMapManager WM
		{
			get
			{
				return this.nM2D.WM;
			}
		}

		public QuestTracker QUEST
		{
			get
			{
				return this.nM2D.QUEST;
			}
		}

		public NelItemManager IMNG
		{
			get
			{
				return this.nM2D.IMNG;
			}
		}

		public NightController NightCon
		{
			get
			{
				return this.nM2D.NightCon;
			}
		}

		public UIBase Ui
		{
			get
			{
				return this.nM2D.Ui;
			}
		}

		public GuildManager GUILD
		{
			get
			{
				return this.nM2D.GUILD;
			}
		}

		public UiGameMenu GM
		{
			get
			{
				return this.nM2D.GM;
			}
		}

		public WanderingManager WDR
		{
			get
			{
				return this.nM2D.WDR;
			}
		}

		public MGContainer MGC
		{
			get
			{
				return this.nM2D.MGC;
			}
		}

		public M2ManaContainer Mana
		{
			get
			{
				return this.nM2D.Mana;
			}
		}

		public PRNoel PlayerNoel
		{
			get
			{
				return this.nM2D.PlayerNoel;
			}
		}

		public UiGO GameOver
		{
			get
			{
				return this.nM2D.GameOver;
			}
		}

		public override string ToString()
		{
			return "NelM2DEventListener";
		}

		public readonly NelM2DBase nM2D;

		public bool areatitle_hide_progress;

		private bool need_autosave;

		private BDic<POSTM, PostEffectItem> OPEevent;

		private string change_scene_on_ev_quit;
	}
}
