using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using XX;

namespace m2d
{
	public class M2EventCommand : EvReader
	{
		public M2EventCommand(int _cmd_index, M2EventItem _Ev, string _name, string _script, Map2d M2)
			: base("%M2EVENTCOMMAND__@" + M2.key + "~" + _name, 0, null, null)
		{
			this.cmd_index = _cmd_index;
			this.Ev = _Ev;
			this.Aexecuted = new List<IM2EvTrigger>();
			this.parseText(_script);
		}

		public override bool open(bool is_init)
		{
			if (!base.open(is_init))
			{
				return false;
			}
			if (is_init)
			{
				M2EventCommand.AstoreME = null;
				M2EventCommand.AstoreMV = null;
			}
			if (M2EventCommand.Ev0 == null)
			{
				M2EventCommand.LastExecuted = null;
			}
			M2EventCommand.cmd_index0 = this.cmd_index;
			if (this.Ev != null)
			{
				this.Ev.quitMoveScript(true);
				M2EventCommand.Ev0 = this.Ev;
				M2EventCommand.EvME = this.Ev;
				M2EventCommand.EvMV = this.Ev;
				this.Ev.event_is_active = true;
				if (this.Aexecuted.Count > 0)
				{
					M2EventCommand.LastExecuted = this.Aexecuted[0];
					this.Aexecuted.RemoveAt(0);
				}
			}
			return true;
		}

		public M2EventCommand stackExecuted(IM2EvTrigger Elm)
		{
			if (Elm != null)
			{
				this.Aexecuted.Insert(0, Elm);
			}
			return this;
		}

		public override bool close(bool is_last)
		{
			if (is_last)
			{
				M2EventCommand.Ev0 = null;
				M2EventCommand.EvME = null;
				M2EventCommand.EvMV = null;
				M2EventCommand.LastExecuted = null;
			}
			return true;
		}

		private Map2d Mp
		{
			get
			{
				if (!(this.Ev != null))
				{
					return M2DBase.Instance.curMap;
				}
				return this.Ev.Mp;
			}
		}

		internal static bool readEvLineM2(EvReader ER, StringHolder rER, int skipping = 0)
		{
			Map2d map2d = ((M2EventCommand.Ev0 != null) ? M2EventCommand.Ev0.Mp : null) ?? M2DBase.Instance.curMap;
			if (rER.cmd.IndexOf("#<") == 0)
			{
				if (REG.match(rER.slice_join(0, "", ""), M2EventCommand.RegCmdChangeMov))
				{
					rER.tError(M2EventCommand.focusEventMover(REG.R1, TX.valid(REG.R2)));
				}
				return true;
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1848018870U)
				{
					if (num <= 735327009U)
					{
						if (num <= 512870383U)
						{
							if (num <= 309484064U)
							{
								if (num != 184064836U)
								{
									if (num != 309484064U)
									{
										return false;
									}
									if (!(cmd == "#PTCST"))
									{
										return false;
									}
									goto IL_0AA1;
								}
								else
								{
									if (!(cmd == "#MS_SOFT_"))
									{
										return false;
									}
									goto IL_065C;
								}
							}
							else if (num != 448976475U)
							{
								if (num != 512870383U)
								{
									return false;
								}
								if (!(cmd == "LOAD_LAYER"))
								{
									return false;
								}
								M2MapLayer layer = map2d.getLayer(rER._1);
								if (layer == null)
								{
									return rER.tError("Layer が不明: " + rER._1);
								}
								if (!layer.unloaded)
								{
									return rER.tNote("Layer " + rER._1 + " は既に読み込まれています", false);
								}
								layer.loadLayerFromEvent();
								return true;
							}
							else
							{
								if (!(cmd == "LP_ACTIVATE"))
								{
									return false;
								}
								goto IL_0E3D;
							}
						}
						else if (num <= 640509306U)
						{
							if (num != 537334710U)
							{
								if (num != 640509306U)
								{
									return false;
								}
								if (!(cmd == "#MS"))
								{
									return false;
								}
							}
							else
							{
								if (!(cmd == "#REMOVE"))
								{
									return false;
								}
								if (M2EventCommand.EvME == null)
								{
									return rER.tError("コマンド削除対象イベント未定義");
								}
								M2EventCommand.EvME.remove((rER._1 != "") ? rER._1 : M2EventCommand.cmd_index0.ToString());
								return true;
							}
						}
						else if (num != 658875649U)
						{
							if (num != 735327009U)
							{
								return false;
							}
							if (!(cmd == "PTCST"))
							{
								return false;
							}
							if (map2d == null)
							{
								return rER.tError("Map2d 未定義");
							}
							PTCThreadRunner.PreVar(rER, 2);
							map2d.PtcST(rER._1, null, PTCThread.StFollow.NO_FOLLOW);
							return true;
						}
						else
						{
							if (!(cmd == "#BGCOL"))
							{
								return false;
							}
							if (map2d == null)
							{
								return rER.tError("Map2d 未定義");
							}
							map2d.bgcol = ((rER.clength == 1) ? map2d.bgcol0 : C32.d2c(4278190080U | rER.get_color(1)));
							return true;
						}
					}
					else if (num <= 1047935295U)
					{
						if (num <= 861497370U)
						{
							if (num != 755604140U)
							{
								if (num != 861497370U)
								{
									return false;
								}
								if (!(cmd == "#NO_DECLINE_AREA_CAMERA"))
								{
									return false;
								}
								if (map2d == null)
								{
									return rER.tError("Map2d 未定義");
								}
								map2d.M2D.Cam.do_not_consider_decline_area = true;
								return true;
							}
							else
							{
								if (!(cmd == "#CALL2"))
								{
									return false;
								}
								goto IL_094A;
							}
						}
						else if (num != 1033694922U)
						{
							if (num != 1047935295U)
							{
								return false;
							}
							if (!(cmd == "#MS_"))
							{
								return false;
							}
							goto IL_065C;
						}
						else
						{
							if (!(cmd == "#SND_LOAD"))
							{
								return false;
							}
							if (M2DBase.Instance != null)
							{
								M2DBase.Instance.loadMaterialSnd(rER.slice(1, -1000));
							}
							return true;
						}
					}
					else if (num <= 1578796092U)
					{
						if (num != 1457290945U)
						{
							if (num != 1578796092U)
							{
								return false;
							}
							if (!(cmd == "DARK_DEACTIVATE"))
							{
								return false;
							}
							goto IL_1185;
						}
						else
						{
							if (!(cmd == "#NO_LIMIT_CAMERA"))
							{
								return false;
							}
							M2Camera.no_limit_camera = true;
							return true;
						}
					}
					else if (num != 1597248115U)
					{
						if (num != 1676631637U)
						{
							if (num != 1848018870U)
							{
								return false;
							}
							if (!(cmd == "GREETING"))
							{
								return false;
							}
							if (M2EventCommand.EvME == null)
							{
								return rER.tError("GREETING 対象イベント未定義");
							}
							M2MoverPr m2MoverPr = M2EventCommand.LastExecuted as M2MoverPr;
							if (m2MoverPr == null)
							{
								m2MoverPr = M2EventCommand.EvMV as M2MoverPr;
								if (m2MoverPr == null)
								{
									return rER.tError("LastExecuted が M2MoverPr ではない ( #< % > を宣言しておくと良いよ )");
								}
							}
							int num3;
							if (rER._1 == "")
							{
								int num2 = CAim._XD(M2EventCommand.EvME.aim, 1);
								num3 = ((num2 < 0) ? 0 : ((num2 > 0) ? 2 : (-1)));
							}
							else if (rER._1 == "@")
							{
								if (m2MoverPr == M2EventCommand.EvME)
								{
									return true;
								}
								num3 = ((M2EventCommand.EvME.x > m2MoverPr.x) ? 0 : 2);
								M2EventCommand.EvME.setAim((AIM)num3, false);
							}
							else
							{
								num3 = rER.IntAim(1, -1);
							}
							if (num3 == -1)
							{
								num3 = ((M2EventCommand.EvME.x < m2MoverPr.x) ? 0 : 2);
							}
							m2MoverPr.eventGreeting(M2EventCommand.EvME, num3, rER.Int(2, 0));
							return true;
						}
						else
						{
							if (!(cmd == "#TE"))
							{
								return false;
							}
							M2EventItem m2EventItem = M2EventCommand.EvMV as M2EventItem;
							TransEffecter transEffecter = null;
							if (m2EventItem != null && m2EventItem.TeCon != null)
							{
								transEffecter = m2EventItem.TeCon;
							}
							else
							{
								M2Attackable m2Attackable = M2EventCommand.EvMV as M2Attackable;
								if (m2Attackable != null && m2Attackable.TeCon != null)
								{
									transEffecter = m2Attackable.TeCon;
								}
							}
							if (transEffecter == null)
							{
								return rER.tError("#TE 対象MoverからTeConを取得できませんでした");
							}
							if (rER._1 == "FADEIN")
							{
								transEffecter.setFadeIn(rER.Nm(2, 30f), rER.Nm(3, 0f));
							}
							return true;
						}
					}
					else if (!(cmd == "#MS_SOFT"))
					{
						return false;
					}
					if (M2EventCommand.EvMV == null)
					{
						return rER.tError("移動スクリプト定義対象 M2Mover 未定義");
					}
					M2EventCommand.EvMV.assignMoveScript(rER.slice_join(1, " ", ""), rER.cmd == "#MS_SOFT");
					return true;
					IL_065C:
					M2EventCommand.saveMv();
					rER.tError(M2EventCommand.focusEventMover(rER._1, false));
					if (M2EventCommand.EvMV == null)
					{
						rER.tError("移動スクリプト定義対象 M2Mover 未定義");
					}
					else
					{
						M2EventCommand.EvMV.assignMoveScript(rER.slice_join(2, " ", ""), rER.cmd == "#MS_SOFT_");
					}
					M2EventCommand.restoreMv();
					return true;
				}
				M2LabelPoint m2LabelPoint;
				if (num <= 2796222722U)
				{
					if (num <= 2325954431U)
					{
						if (num <= 2168786287U)
						{
							if (num != 2118851865U)
							{
								if (num != 2168786287U)
								{
									return false;
								}
								if (!(cmd == "#CMDRELOAD"))
								{
									return false;
								}
								if (map2d == null)
								{
									return rER.tError("Map2d 未定義");
								}
								bool flag = rER.has("IMMEDIATE_LOAD");
								map2d.cmd_reload_flg = ((flag || rER.has("IMMEDIATE")) ? 2U : 0U) | (rER.has("NOLOAD") ? 4U : 0U) | 1U;
								if (flag)
								{
									map2d.cmd_loadevent_execute |= 1U;
									EV.initWaitFn(map2d, 0);
								}
								return true;
							}
							else
							{
								if (!(cmd == "#LIMIT_CAMERA"))
								{
									return false;
								}
								M2Camera.no_limit_camera = false;
								return true;
							}
						}
						else if (num != 2285128582U)
						{
							if (num != 2325954431U)
							{
								return false;
							}
							if (!(cmd == "DARK_ACTIVATE"))
							{
								return false;
							}
							goto IL_1185;
						}
						else if (!(cmd == "CHIP_DEACTIVATE"))
						{
							return false;
						}
					}
					else if (num <= 2571011633U)
					{
						if (num != 2435694927U)
						{
							if (num != 2571011633U)
							{
								return false;
							}
							if (!(cmd == "#RELOADME"))
							{
								return false;
							}
							if (map2d == null)
							{
								return rER.tError("Map2d 未定義");
							}
							map2d.commandReloadSpecific((rER._1 != "") ? rER._1 : ((M2EventCommand.EvME != null) ? M2EventCommand.EvME.key : M2EventCommand.Ev0.key));
							return true;
						}
						else
						{
							if (!(cmd == "#MOVE"))
							{
								return false;
							}
							M2EventItem m2EventItem2 = M2EventCommand.EvMV as M2EventItem;
							if (m2EventItem2 == null)
							{
								return rER.tError("#MOVE 対象Mover未定義");
							}
							m2EventItem2.setMovePattern(rER._1);
							return false;
						}
					}
					else if (num != 2619134046U)
					{
						if (num != 2792518789U)
						{
							if (num != 2796222722U)
							{
								return false;
							}
							if (!(cmd == "PR_KEY_SIMULATE"))
							{
								return false;
							}
							if (map2d == null)
							{
								return rER.tError("Map2d 未定義");
							}
							if (M2EventCommand.EvMV == null)
							{
								return rER.tError("移動スクリプト定義対象 M2Mover 未定義");
							}
							if (!(M2EventCommand.EvMV is M2MoverPr))
							{
								return rER.tError("Mover が M2MoverPr ではない");
							}
							(M2EventCommand.EvMV as M2MoverPr).simulateKeyDown(rER._1, rER.Nm(2, 1f) != 0f);
							return true;
						}
						else
						{
							if (!(cmd == "#PTCST_REMOVE"))
							{
								return false;
							}
							goto IL_0AA1;
						}
					}
					else
					{
						if (!(cmd == "#CHECK_DESC_KEY"))
						{
							return false;
						}
						if (rER.clength == 2)
						{
							if (M2EventCommand.EvMV == null || !(M2EventCommand.EvMV is M2EventItem))
							{
								return rER.tError("DescKey 対象イベント未定義");
							}
							(M2EventCommand.EvMV as M2EventItem).check_desc_name = rER._1;
						}
						else
						{
							if (map2d == null)
							{
								return rER.tError("Map2d 未定義");
							}
							M2EventItem m2EventItem3 = map2d.getMoverByName(rER._1, false) as M2EventItem;
							if (!(m2EventItem3 != null))
							{
								return rER.tError("DescKey 対象イベント不明: " + rER._1);
							}
							m2EventItem3.check_desc_name = rER._2;
						}
						return true;
					}
				}
				else if (num <= 3174601361U)
				{
					if (num <= 2931185174U)
					{
						if (num != 2894219669U)
						{
							if (num != 2931185174U)
							{
								return false;
							}
							if (!(cmd == "#CALL"))
							{
								return false;
							}
							goto IL_094A;
						}
						else
						{
							if (!(cmd == "#VANISH"))
							{
								return false;
							}
							if (rER.clength == 1)
							{
								if (M2EventCommand.EvME == null)
								{
									return rER.tError("削除対象イベント未定義");
								}
								M2EventCommand.EvME.destruct();
							}
							else
							{
								if (map2d == null)
								{
									return rER.tError("Map2d 未定義");
								}
								M2Mover moverByName = map2d.getMoverByName(rER._1, false);
								if (!(moverByName != null))
								{
									return rER.tError("削除対象イベント不明: " + rER._1);
								}
								moverByName.destruct();
							}
							return true;
						}
					}
					else if (num != 2974406578U)
					{
						if (num != 3174601361U)
						{
							return false;
						}
						if (!(cmd == "CHIP_ACTIVATE"))
						{
							return false;
						}
					}
					else
					{
						if (!(cmd == "#CLEARSND"))
						{
							return false;
						}
						M2EventItem m2EventItem4 = M2EventCommand.EvMV as M2EventItem;
						if (m2EventItem4 == null)
						{
							return rER.tError("#CLEARSND 対象Mover未定義");
						}
						m2EventItem4.clearSndIntv(rER._1, rER._2);
						return true;
					}
				}
				else if (num <= 3978172628U)
				{
					if (num != 3486860610U)
					{
						if (num != 3978172628U)
						{
							return false;
						}
						if (!(cmd == "#SNDINTV"))
						{
							return false;
						}
						M2EventItem m2EventItem5 = M2EventCommand.EvMV as M2EventItem;
						if (m2EventItem5 == null)
						{
							return rER.tError("#SNDINTV 対象Mover未定義");
						}
						m2EventItem5.addSndIntv(rER._1, rER._2, rER.Nm(3, 120f), rER.Nm(4, 0f), rER.Int(5, -1));
						return true;
					}
					else
					{
						if (!(cmd == "#DECLINE_AREA_CAMERA"))
						{
							return false;
						}
						if (map2d == null)
						{
							return rER.tError("Map2d 未定義");
						}
						map2d.M2D.Cam.do_not_consider_decline_area = false;
						return true;
					}
				}
				else if (num != 4058571624U)
				{
					if (num != 4105947604U)
					{
						if (num != 4123001947U)
						{
							return false;
						}
						if (!(cmd == "LP_DEACTIVATE_TO_CHIP"))
						{
							return false;
						}
					}
					else if (!(cmd == "LP_ACTIVATE_TO_CHIP"))
					{
						return false;
					}
					if (map2d == null)
					{
						return rER.tError("Map2d 未定義");
					}
					if (rER.clength == 2)
					{
						m2LabelPoint = map2d.getLabelPoint(rER._1);
						if (m2LabelPoint == null)
						{
							return rER.tError("LP が不明: " + rER._1);
						}
					}
					else
					{
						M2MapLayer layer2 = map2d.getLayer(rER._1);
						if (layer2 == null)
						{
							return rER.tError("Layer が不明: " + rER._1);
						}
						m2LabelPoint = layer2.LP.Get(rER._2, true, false);
						if (m2LabelPoint == null)
						{
							return rER.tError("Layer " + rER._1 + " 上の LP が不明: " + rER._2);
						}
					}
					m2LabelPoint.Mp.activateToChip(m2LabelPoint.mapx, m2LabelPoint.mapy, m2LabelPoint.mapw, m2LabelPoint.maph, rER.cmd == "LP_DEACTIVATE_TO_CHIP", 1UL << m2LabelPoint.Lay.index, (rER.clength >= 4) ? rER._3 : null);
					return true;
				}
				else
				{
					if (!(cmd == "LP_DEACTIVATE"))
					{
						return false;
					}
					goto IL_0E3D;
				}
				if (map2d == null)
				{
					return rER.tError("Map2d 未定義");
				}
				M2MapLayer layer3 = map2d.getLayer(rER._1);
				if (layer3 == null)
				{
					return rER.tError("Layer が不明: " + rER._1);
				}
				M2Puts chipByIndex = layer3.getChipByIndex(rER.Int(2, -1));
				if (chipByIndex == null)
				{
					return rER.tError("Cp が不明: " + rER._2);
				}
				if (rER.cmd == "CHIP_ACTIVATE")
				{
					if (chipByIndex is IActivatable)
					{
						(chipByIndex as IActivatable).activate();
					}
					chipByIndex.activateToDrawer();
				}
				else
				{
					if (chipByIndex is IActivatable)
					{
						(chipByIndex as IActivatable).deactivate();
					}
					chipByIndex.deactivateToDrawer();
				}
				return true;
				IL_094A:
				string[] array = null;
				M2EventCommand m2EventCommand;
				if (rER.clength == 2)
				{
					if (M2EventCommand.Ev0 == null)
					{
						return rER.tError("#CALL 現在のイベントが不明 ");
					}
					M2EventItem m2EventItem6 = M2EventCommand.Ev0;
					m2EventCommand = m2EventItem6.Get(rER._1);
				}
				else
				{
					if (rER.clength < 3)
					{
						return true;
					}
					M2EventItem m2EventItem6 = map2d.getMoverByName(rER._1, false) as M2EventItem;
					if (m2EventItem6 == null)
					{
						return rER.tError("#CALL 対象のイベント名が見つかりません: " + rER._1);
					}
					m2EventCommand = m2EventItem6.Get(rER._2);
					array = rER.slice(3, -1000);
				}
				if (m2EventCommand == null)
				{
					return rER.tError("#CALL 対象のイベントトリガーにイベントがアサインがされていません: " + rER._1 + " " + rER._2);
				}
				m2EventCommand.stackExecuted(M2EventCommand.LastExecuted);
				if (rER.cmd == "#CALL")
				{
					EV.changeEvent(m2EventCommand, 0, null);
				}
				else
				{
					EV.moduleInit(ER, m2EventCommand, array, false);
				}
				return true;
				IL_0AA1:
				if (map2d == null)
				{
					return rER.tError("Map2d 未定義");
				}
				if (M2EventCommand.EvMV == null)
				{
					return rER.tError("#PTCST 対象Mover未定義");
				}
				if (M2EventCommand.EvMV is M2Attackable)
				{
					M2Attackable m2Attackable2 = M2EventCommand.EvMV as M2Attackable;
					if (rER.cmd == "#PTCST_REMOVE")
					{
						m2Attackable2.PtcHld.killPtc(rER._1, false);
					}
					else
					{
						m2Attackable2.defineParticlePreVariable();
						PTCThreadRunner.PreVar(rER, 2);
						m2Attackable2.PtcST(rER._1, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
				}
				else
				{
					if (rER.cmd == "#PTCST_REMOVE")
					{
						return rER.tError("PTCST_REMOVE はM2Attackableにのみ実行できます");
					}
					float num4 = M2EventCommand.EvMV.x;
					float num5 = M2EventCommand.EvMV.x;
					if (M2EventCommand.EvMV is M2EventItem)
					{
						num4 = (M2EventCommand.EvMV as M2EventItem).event_cx;
						num5 = (M2EventCommand.EvMV as M2EventItem).event_cy;
					}
					map2d.PtcSTsetVar("cx", (double)num4).PtcSTsetVar("cy", (double)num5);
					PTCThreadRunner.PreVar(rER, 2);
					map2d.PtcST(rER._1, null, PTCThread.StFollow.NO_FOLLOW);
				}
				return true;
				IL_0E3D:
				if (map2d == null)
				{
					return rER.tError("Map2d 未定義");
				}
				if (rER.clength == 2)
				{
					m2LabelPoint = map2d.getLabelPoint(rER._1);
					if (m2LabelPoint == null)
					{
						return rER.tError("LP が不明: " + rER._1);
					}
				}
				else
				{
					M2MapLayer layer4 = map2d.getLayer(rER._1);
					if (layer4 == null)
					{
						return rER.tError("Layer が不明: " + rER._1);
					}
					m2LabelPoint = layer4.LP.Get(rER._2, true, false);
					if (m2LabelPoint == null)
					{
						return rER.tError("Layer " + rER._1 + " 上の LP が不明: " + rER._2);
					}
				}
				if (m2LabelPoint != null)
				{
					if (rER.cmd == "LP_ACTIVATE")
					{
						((IActivatable)m2LabelPoint).activate();
					}
					if (rER.cmd == "LP_DEACTIVATE")
					{
						((IActivatable)m2LabelPoint).deactivate();
					}
				}
				return true;
				IL_1185:
				if (map2d == null)
				{
					return rER.tError("Map2d 未定義");
				}
				if (map2d.Unstb == null)
				{
					return rER.tError("Map2d.Unstb 未定義");
				}
				map2d.Unstb.use_dark = rER.cmd == "DARK_ACTIVATE";
				return true;
			}
			return false;
		}

		public static void saveMv()
		{
			if (M2EventCommand.AstoreME == null)
			{
				M2EventCommand.AstoreME = new List<M2EventItem>();
				M2EventCommand.AstoreMV = new List<M2Mover>();
			}
			M2EventCommand.AstoreME.Add(M2EventCommand.EvME);
			M2EventCommand.AstoreMV.Add(M2EventCommand.EvMV);
		}

		public static void restoreMv()
		{
			if (M2EventCommand.AstoreME == null || M2EventCommand.AstoreME.Count == 0)
			{
				M2EventCommand.focusEventMover("", false);
				return;
			}
			M2EventCommand.EvME = X.pop<M2EventItem>(M2EventCommand.AstoreME);
			M2EventCommand.EvMV = X.pop<M2Mover>(M2EventCommand.AstoreMV);
		}

		public static string focusEventMover(string key, bool default_override = false)
		{
			if ((M2EventCommand.Ev0 ? M2EventCommand.Ev0.Mp : null) == null)
			{
				Map2d curMap = M2DBase.Instance.curMap;
			}
			M2EventCommand.EvMV = M2EventCommand.getEventTargetMover(key);
			M2EventCommand.EvME = M2EventCommand.EvMV as M2EventItem;
			if (M2EventCommand.EvME != null)
			{
				if (default_override)
				{
					M2EventCommand.Ev0 = M2EventCommand.EvME;
				}
			}
			else
			{
				M2EventCommand.EvME = M2EventCommand.Ev0;
			}
			return null;
		}

		public static M2Mover getEventTargetMover(string key)
		{
			Map2d map2d = (M2EventCommand.Ev0 ? M2EventCommand.Ev0.Mp : null);
			if (map2d == null)
			{
				map2d = M2DBase.Instance.curMap;
			}
			if (TX.noe(key) || key == "#")
			{
				return M2EventCommand.Ev0;
			}
			if (key.IndexOf("%") == 0)
			{
				return map2d.getPlayerFromEvent(TX.slice(key, 1));
			}
			return map2d.getMoverByName(key, false);
		}

		public int cmd_index;

		public M2EventItem Ev;

		public List<IM2EvTrigger> Aexecuted;

		public static IM2EvTrigger LastExecuted;

		public static int cmd_index0;

		public static M2EventItem Ev0;

		public static M2EventItem EvME;

		public static M2Mover EvMV;

		public static List<M2EventItem> AstoreME = null;

		public static List<M2Mover> AstoreMV = null;

		public const string NAME_HEADER = "%M2EVENTCOMMAND__@";

		public static readonly Regex RegCmdChangeMov = new Regex("\\#<[ \\s\\t\\,]*(\\%?[#\\w]*)[ \\s\\t\\,]*>?(\\!)?$");
	}
}
