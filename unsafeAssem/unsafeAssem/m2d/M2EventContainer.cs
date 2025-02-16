using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2EventContainer : CsvReaderA
	{
		public M2EventContainer(Map2d _Mp, string _script)
			: base(null, null)
		{
			this.Mp = _Mp;
			this.tilde_replace = true;
			if (_script != null)
			{
				this.parseText(_script);
			}
			this.AEv = new M2EventItem[8];
			this.OASpecialCmd = new BDic<string, List<M2EventCommand>>();
			this.FlgStandDecline = new Flagger(null, delegate(FlaggerT<string> V)
			{
				M2MoverPr pr = this.Mp.Pr;
				if (pr != null)
				{
					pr.check_event_all = true;
				}
			});
			if (TX.isStart(_script, "##", 0) && _script.IndexOf("##USE_EVENT_VARCON##") >= 0)
			{
				this.VarCon = EV.getVariableContainer();
			}
			if (this.VarCon == null && this.Mp.is_submap)
			{
				try
				{
					this.VarCon = this.Mp.SubMapData.getBaseMap().getEventContainer().VarCon;
				}
				catch
				{
				}
			}
			if (this.VarCon == null)
			{
				this.VarCon = new CsvVariableContainer();
			}
		}

		public void reload(string only_this = "", bool no_load_and_loadonce = false)
		{
			base.seekReset();
			this.executeClearingRemoveObject();
			if (M2MobGenerator.Instance != null)
			{
				M2MobGenerator.Instance.ClearFinAtlas();
			}
			M2EventItem m2EventItem = null;
			this.AnotInitEvent = new List<M2EventItem>(8);
			this.ABccFootable = null;
			bool flag = false;
			M2LabelPoint m2LabelPoint = null;
			this.Mp.cmd_loadevent_execute &= 4294967293U;
			M2PxlAnimator m2PxlAnimator = null;
			bool flag2 = false;
			Map2d mp = this.Mp;
			M2EventContainer m2EventContainer = this;
			while (M2EventContainer.readContainerCmd0(this.Mp, ref mp, ref m2EventContainer, this, only_this, ref m2EventItem, ref m2LabelPoint, ref flag2, ref flag, ref m2PxlAnimator, this.AnotInitEvent, no_load_and_loadonce))
			{
			}
			this.runFn(this.AFnReload);
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.AEv[i] != null)
				{
					this.AEv[i].appear_first = false;
				}
			}
			if (this.AnotInitEvent.Count > 0)
			{
				this.Mp.cmd_reload_flg |= 8U;
			}
			else
			{
				this.AnotInitEvent = null;
			}
			X.dl("M2EventContainer::reload - コマンドの再読み込みを実行:" + this.Mp.key + " " + ((only_this != "") ? ("(イベント: " + only_this + " )") : " "), null, false, false);
		}

		public static bool readContainerCmd0(Map2d BaseMp, ref Map2d Mp, ref M2EventContainer Con, CsvReaderA rER, string only_this, ref M2EventItem curEv, ref M2LabelPoint DefineOnPoint, ref bool talk_target_ttt, ref bool map_assigned, ref M2PxlAnimator Anm, List<M2EventItem> AnotInitEvent, bool no_load_and_loadonce)
		{
			bool flag = !rER.read();
			if (TX.isStart(rER.cmd, "##", 0))
			{
				return true;
			}
			if (rER.cmd == "/*" || rER.cmd == "/*___" || flag)
			{
				M2EventItem m2EventItem = curEv;
				if (flag)
				{
					curEv = null;
				}
				else
				{
					string text = rER.getIndex((rER.cmd == "/*") ? 2 : 1);
					if (REG.match(text, M2EventContainer.RegEvTitle))
					{
						text = REG.R1;
						curEv = null;
						if (only_this == "" || text == only_this)
						{
							curEv = Con.Get(text, false, false);
							curEv.talk_target_ttt = talk_target_ttt;
							map_assigned = false;
							AnotInitEvent.Add(curEv);
						}
					}
				}
				if (m2EventItem != null && m2EventItem != curEv && Anm != null)
				{
					m2EventItem.spriteSetted();
				}
				Anm = null;
				if (flag)
				{
					return false;
				}
			}
			else if (rER.cmd == "%DEFINE_ON_POINT")
			{
				if (rER.clength > 1)
				{
					DefineOnPoint = Mp.getPoint(((rER._2 != "") ? (rER._2 + "..") : "") + rER._1, false);
				}
				else
				{
					DefineOnPoint = null;
				}
			}
			else if (rER.cmd == "%DOD_CM_BEHIND_BG")
			{
				Mp.MovRenderer.z_cm_override = 505f;
			}
			else if (rER.cmd == "%DARK_ACTIVATE")
			{
				Mp.Unstb.use_dark = rER.NmE(1, 1f) != 0f;
			}
			else if (rER.cmd == "%MOBTYPE")
			{
				Mp.M2D.ev_mobtype = rER._1 ?? "";
			}
			else if (rER.cmd == "%TARGET_MAP")
			{
				if (TX.noe(rER._1))
				{
					Mp = BaseMp;
					Con = Mp.getEventContainer();
				}
				else
				{
					Map2d map2d = BaseMp.M2D.Get(rER._1, false);
					if (map2d == null)
					{
						rER.tError("ターゲットマップ不明");
					}
					else if (map2d.SubMapData == null || map2d.SubMapData.getBaseMap() != BaseMp)
					{
						rER.tError("ターゲットマップ " + rER._1 + " はサブマップとして開かれていません");
					}
					else
					{
						Mp = map2d;
						Con = Mp.getEventContainer();
						DefineOnPoint = null;
						if (Con == null)
						{
							Con = Mp.prepareCommand(null);
						}
					}
				}
			}
			else if (TX.isStart(rER.cmd, '@'))
			{
				string text2 = M2DBase.readMapEventContent(TX.slice(rER.cmd, 1));
				if (TX.valid(text2))
				{
					CsvReaderA csvReaderA = new CsvReaderA(text2, Con.VarCon)
					{
						tilde_replace = Con.tilde_replace,
						no_replace_quote = Con.no_replace_quote,
						no_write_varcon = Con.no_write_varcon
					};
					while (M2EventContainer.readContainerCmd0(BaseMp, ref Mp, ref Con, csvReaderA, only_this, ref curEv, ref DefineOnPoint, ref talk_target_ttt, ref map_assigned, ref Anm, AnotInitEvent, no_load_and_loadonce))
					{
					}
				}
			}
			else if (curEv != null)
			{
				if (rER.cmd == "%CLEAR")
				{
					curEv.clear();
				}
				else if (rER.cmd == "%TALK_TARGET_TTT")
				{
					curEv.talk_target_ttt = rER.Nm(1, 1f) != 0f;
				}
				else
				{
					if (!map_assigned)
					{
						map_assigned = true;
						if (!curEv.do_not_map_assign && curEv.appear_first)
						{
							Mp.assignMover(curEv, false);
							if (DefineOnPoint != null)
							{
								curEv.setToArea(DefineOnPoint);
							}
						}
						X.pushToEmptyRI<M2EventItem>(ref Con.AEv, curEv, -1);
					}
					if (M2EventContainer.readContainerCmd(Mp, rER, curEv, no_load_and_loadonce, false))
					{
						AnotInitEvent.Remove(curEv);
					}
				}
			}
			else if (rER.cmd == "%TALK_TARGET_TTT")
			{
				talk_target_ttt = rER.Nm(1, 1f) != 0f;
			}
			return true;
		}

		public static bool readContainerCmd(Map2d Mp, CsvReaderA rER, M2EventItem curEv, bool no_load_and_loadonce = false, bool no_check_equal = false)
		{
			bool flag = false;
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2177608140U)
				{
					if (num <= 632223139U)
					{
						if (num <= 541049905U)
						{
							if (num != 39109648U)
							{
								if (num != 55238731U)
								{
									if (num != 541049905U)
									{
										goto IL_07FE;
									}
									if (!(cmd == "%SIZE"))
									{
										goto IL_07FE;
									}
									curEv.Size(rER._N1, X.Nm(rER._2, -1000f, true), ALIGN.CENTER, ALIGNY.MIDDLE, false);
									return flag;
								}
								else
								{
									if (!(cmd == "%MOBG_ATLAS_WH"))
									{
										goto IL_07FE;
									}
									curEv.setMobgAtlasWH(rER.Int(1, 0), rER.Int(2, 0));
									return flag;
								}
							}
							else
							{
								if (!(cmd == "%PXL_MTR_WITH_LIGHT"))
								{
									goto IL_07FE;
								}
								curEv.setPxlMtrWithLight(rER.getB(1, true));
								return flag;
							}
						}
						else if (num != 591069755U)
						{
							if (num != 627876397U)
							{
								if (num != 632223139U)
								{
									goto IL_07FE;
								}
								if (!(cmd == "%PXL_MTR_USE_MASK"))
								{
									goto IL_07FE;
								}
								curEv.setPxlMtrUseMask(rER.getB(1, true));
								return flag;
							}
							else
							{
								if (!(cmd == "%MOVE"))
								{
									goto IL_07FE;
								}
								curEv.setMovePattern(rER);
								return flag;
							}
						}
						else
						{
							if (!(cmd == "%MOBG"))
							{
								goto IL_07FE;
							}
							curEv.setMobgChara(rER._1, rER._2, rER._3);
							curEv.SpSetShift(0f, 3.5f);
							return true;
						}
					}
					else if (num <= 1853018414U)
					{
						if (num != 1365989493U)
						{
							if (num != 1668734952U)
							{
								if (num != 1853018414U)
								{
									goto IL_07FE;
								}
								if (!(cmd == "%SP_SHIFT_DEFAULT"))
								{
									goto IL_07FE;
								}
								curEv.SpSetShift(0f, 3.5f);
								return flag;
							}
							else
							{
								if (!(cmd == "%SP_SHIFT"))
								{
									goto IL_07FE;
								}
								if (!curEv.SpSetShift(X.Nm(rER._1, -1000f, true), X.Nm(rER._2, -1000f, true)))
								{
									rER.tError("M2EventContainer::reload - スプライトが空です");
									return flag;
								}
								return flag;
							}
						}
						else if (!(cmd == "%PXL_LOAD"))
						{
							goto IL_07FE;
						}
					}
					else if (num != 2007160852U)
					{
						if (num != 2021943417U)
						{
							if (num != 2177608140U)
							{
								goto IL_07FE;
							}
							if (!(cmd == "%PT"))
							{
								goto IL_07FE;
							}
							curEv.lp_set_pos_randw = null;
							if (curEv.appear_first || rER._B4)
							{
								curEv.setToLabelPt(rER._1, rER._N2 * Mp.rCLEN, rER._N3 * Mp.rCLEN);
								return flag;
							}
							return flag;
						}
						else
						{
							if (!(cmd == "%AIM"))
							{
								goto IL_07FE;
							}
							if (rER._1 == "LR" || rER._1 == "RL")
							{
								curEv.setAim((X.xors(2) == 0) ? AIM.L : AIM.R, false);
								return flag;
							}
							curEv.setAim((AIM)CAim.parseString(rER._1, 0), false);
							return flag;
						}
					}
					else
					{
						if (!(cmd == "%LIGHT"))
						{
							goto IL_07FE;
						}
						curEv.setLight(X.NmUI(rER._1, 0U, true, true), X.Nm(rER._2, -1f, false));
						return true;
					}
				}
				else if (num <= 3474896040U)
				{
					if (num <= 2366762826U)
					{
						if (num != 2294610188U)
						{
							if (num != 2328090668U)
							{
								if (num != 2366762826U)
								{
									goto IL_07FE;
								}
								if (!(cmd == "%PXL_FRAME"))
								{
									goto IL_07FE;
								}
								curEv.first_anm_frame = rER.Int(1, -1);
								return flag;
							}
							else
							{
								if (!(cmd == "%PXL_TS"))
								{
									goto IL_07FE;
								}
								curEv.anm_timescale = rER.Nm(1, 1f);
								return flag;
							}
						}
						else
						{
							if (!(cmd == "%MS"))
							{
								goto IL_07FE;
							}
							curEv.assignMoveScript(rER._1, false);
							return flag;
						}
					}
					else if (num != 2390737418U)
					{
						if (num != 2899045254U)
						{
							if (num != 3474896040U)
							{
								goto IL_07FE;
							}
							if (!(cmd == "%SND_LOAD"))
							{
								goto IL_07FE;
							}
							Mp.M2D.loadMaterialSnd(rER.slice(1, -1000));
							return true;
						}
						else
						{
							if (!(cmd == "%SET_ENABLE"))
							{
								goto IL_07FE;
							}
							if (rER.clength == 2)
							{
								curEv.setExecutableAll(rER.NmE(1, 1f) != 0f);
								return flag;
							}
							M2EventItem.CMD cmd2;
							if (FEnum<M2EventItem.CMD>.TryParse(rER._1U, out cmd2, true))
							{
								curEv.setExecutable(cmd2, rER.NmE(2, 1f) != 0f);
								return flag;
							}
							return flag;
						}
					}
					else
					{
						if (!(cmd == "%MOBG_CLIP"))
						{
							goto IL_07FE;
						}
						M2LabelPoint point = Mp.getPoint(rER._1, false);
						if (point == null)
						{
							rER.tError("不明なLP: " + rER._1);
							return flag;
						}
						if (!curEv.MobgClipArea(point))
						{
							rER.tError("MobGがありません");
							return flag;
						}
						return flag;
					}
				}
				else if (num <= 3771565507U)
				{
					if (num != 3555506413U)
					{
						if (num != 3589123125U)
						{
							if (num != 3771565507U)
							{
								goto IL_07FE;
							}
							if (!(cmd == "%PHY"))
							{
								goto IL_07FE;
							}
							curEv.initPhysics();
							return flag;
						}
						else
						{
							if (!(cmd == "%POS_RANDW"))
							{
								goto IL_07FE;
							}
							curEv.lp_set_pos_randw = rER._1 ?? "";
							return flag;
						}
					}
					else
					{
						if (!(cmd == "%POSE"))
						{
							goto IL_07FE;
						}
						curEv.SpSetPose((rER._1 == "") ? null : rER._1, -1, null, false);
						return flag;
					}
				}
				else if (num <= 3805527783U)
				{
					if (num != 3792388793U)
					{
						if (num != 3805527783U)
						{
							goto IL_07FE;
						}
						if (!(cmd == "%DOD"))
						{
							goto IL_07FE;
						}
						M2Mover.DRAW_ORDER draw_ORDER;
						if (!FEnum<M2Mover.DRAW_ORDER>.TryParse(rER._1, out draw_ORDER, true))
						{
							rER.tError("DOD が不明: " + rER._1);
							return flag;
						}
						if (!curEv.setDod(draw_ORDER))
						{
							rER.tError("DOD の設定に失敗");
							return flag;
						}
						return flag;
					}
					else
					{
						if (!(cmd == "%AREA"))
						{
							goto IL_07FE;
						}
						curEv.lp_set_pos_randw = null;
						if (curEv.appear_first)
						{
							curEv.setToLabelPt(rER._1 + ".r" + rER._2, rER._N3, rER._N4);
							return flag;
						}
						return flag;
					}
				}
				else if (num != 4044720000U)
				{
					if (num != 4087780412U)
					{
						goto IL_07FE;
					}
					if (!(cmd == "%PXL"))
					{
						goto IL_07FE;
					}
				}
				else
				{
					if (!(cmd == "%CHECK_DESC"))
					{
						goto IL_07FE;
					}
					curEv.check_desc_name = rER._1;
					return flag;
				}
				string text;
				if (TX.isStart(rER._1, "/", 0))
				{
					text = TX.slice(rER._1, 1);
				}
				else
				{
					text = "MapChars/" + rER._1;
				}
				if (rER.cmd == "%PXL_LOAD")
				{
					Mp.M2D.loadMaterialPxl(rER._1, text + ".pxls", true, true, false);
					return flag;
				}
				if (curEv.setPxlChara(rER._1, text, "stand") == null)
				{
					rER.tError("M2EventContainer::reload - 不明な PXL 指定:" + rER._1);
					return flag;
				}
				curEv.spriteSetted();
				return true;
			}
			IL_07FE:
			if (REG.match(rER.cmd, M2EventContainer.RegEventDefine) && (no_check_equal || rER._1 == "="))
			{
				flag = true;
				string text2 = REG.R2.ToUpper();
				bool flag2 = REG.R1 == "";
				bool flag3 = false;
				if ((text2 == "LOAD" || text2 == "LOAD_ONCE") && no_load_and_loadonce)
				{
					flag3 = true;
				}
				int num2 = ((rER._1 == "=") ? 2 : 1);
				string index = rER.getIndex(num2);
				if (index == "")
				{
					curEv.remove(text2);
				}
				else
				{
					STB stb = TX.PopBld(null, 0);
					if (index != "{")
					{
						stb.Add("CHANGE_EVENT ").AddJoin(rER, " ", num2);
					}
					else
					{
						int cur_line = rER.get_cur_line();
						int i = 1;
						while (i > 0)
						{
							if (cur_line != rER.get_cur_line())
							{
								stb.Append(rER.getLastStr(), "\n");
							}
							if (!rER.readCorrectly())
							{
								rER.tError("M2EventContainer::reload - イベントコマンドの読込中に EOF に到達。 first_line:" + cur_line.ToString());
								break;
							}
							string lastStr = rER.getLastStr();
							if (lastStr != "")
							{
								if (TX.charIs(lastStr, lastStr.Length - 1, '{'))
								{
									i++;
								}
								if (TX.charIs(lastStr, 0, '}'))
								{
									i--;
								}
							}
						}
					}
					if (!flag3)
					{
						curEv.assign(text2, stb.ToString(), false);
					}
					TX.ReleaseBld(stb);
				}
				M2EventItem.CMD cmd3;
				if (FEnum<M2EventItem.CMD>.TryParse(text2, out cmd3, true))
				{
					curEv.setExecutable(cmd3, flag2);
				}
			}
			else
			{
				rER.tError("m2dイベントロード中の不明なコマンド: " + rER.cmd);
			}
			return flag;
		}

		public override bool read()
		{
			if (base.isEnd())
			{
				return base.read();
			}
			bool flag = base.read();
			if (!this.jump_bracket)
			{
				return flag;
			}
			if (REG.match(base.cmd, M2EventContainer.RegEventDefine) && base._1 == "=" && base._2 == "{")
			{
				int cur_line = base.get_cur_line();
				while (base.getLastStr() != "}")
				{
					if (!base.readCorrectly())
					{
						X.de("M2EventContainer::read - イベントコマンドのスキップ中に EOF に到達。 first_line:" + cur_line.ToString(), null);
						return false;
					}
				}
				return base.read();
			}
			return flag;
		}

		public void getNearEvents(M2MoverPr FromMv, int x, int y, float chk_len, List<List<IM2TalkableObject>> AATalkable, List<List<M2EventItem>> AAEvStand, uint check_aim_bit)
		{
			float num = (float)x + 0.5f;
			float num2 = (float)y + 0.5f;
			float event_sizex = FromMv.event_sizex;
			float event_sizey = FromMv.event_sizey;
			float num3 = num2 - event_sizey;
			float num4 = num2 + event_sizey;
			float num5 = num - event_sizex;
			float num6 = num + event_sizex;
			if (AATalkable != null)
			{
				for (int i = 0; i < 8; i++)
				{
					if ((check_aim_bit & (1U << i)) != 0U)
					{
						while (AATalkable.Count <= i)
						{
							AATalkable.Add(null);
						}
						List<IM2TalkableObject> list = AATalkable[i];
						if (list == null)
						{
							list = (AATalkable[i] = new List<IM2TalkableObject>(2));
						}
						list.Clear();
						this.Mp.M2D.assignTalkableObject(FromMv, x + ((CAim._XD(i, 1) < 0) ? 1 : 0), y + ((-CAim._YD(i, 1) < 0) ? 1 : 0), (AIM)i, list);
					}
				}
			}
			int num7 = this.AEv.Length;
			List<M2EventItem> list2 = null;
			if (AAEvStand != null)
			{
				list2 = AAEvStand[0];
				list2.Clear();
			}
			for (int j = 0; j < num7; j++)
			{
				M2EventItem m2EventItem = this.AEv[j];
				if (m2EventItem == null)
				{
					break;
				}
				if (list2 != null && (m2EventItem.hasStand() || m2EventItem.hasStandRelease()))
				{
					int num8;
					if (!this.FlgStandDecline.isActive() && m2EventItem.isCovering(num5, num6, num3, num4, 1f + chk_len, 1f + chk_len))
					{
						if (list2.IndexOf(m2EventItem) == -1)
						{
							list2.Add(m2EventItem);
						}
					}
					else if ((num8 = list2.IndexOf(m2EventItem)) >= 0)
					{
						list2.RemoveAt(num8);
					}
				}
				if (AATalkable != null && m2EventItem.hasTalkOrCheck() && m2EventItem.isCovering(num5, num6, num3, num4, 1f + chk_len, 1f + chk_len))
				{
					for (int k = 0; k < 8; k++)
					{
						if ((check_aim_bit & (1U << k)) != 0U)
						{
							List<IM2TalkableObject> list3 = AATalkable[k];
							float num9 = (float)CAim._XD(k, 1);
							float num10 = (float)(-(float)CAim._YD(k, 1));
							if ((num9 == 0f || X.isCovering(m2EventItem.mtop, m2EventItem.mbottom, num3, num4, -0.001f)) && (num10 == 0f || X.isCovering(m2EventItem.mleft, m2EventItem.mright, num5, num6, -0.001f)) && (num9 <= 0f || X.BTW(m2EventItem.mleft - 1.8f - chk_len * 2f, num, m2EventItem.mright)) && (num9 >= 0f || X.BTW(m2EventItem.mleft, num, m2EventItem.mright + 1.8f + chk_len * 2f)) && (num10 <= 0f || X.BTW(m2EventItem.mtop - 1.8f - chk_len * 2f + 1f, num2, m2EventItem.mbottom)) && (num10 >= 0f || X.BTW(m2EventItem.mtop, num2, m2EventItem.mbottom + 1.8f + chk_len * 2f)))
							{
								list3.Add(m2EventItem);
							}
						}
					}
				}
			}
		}

		public bool checkNearEventsExecution(M2MoverPr FromMv, out bool executed, int aim, float chk_len, List<List<IM2TalkableObject>> AATalkable, List<List<M2EventItem>> AAEvStand, bool when_battle_busy)
		{
			bool flag = false;
			executed = false;
			bool flag2 = false;
			if (AAEvStand != null)
			{
				for (int i = 1; i >= 0; i--)
				{
					List<M2EventItem> list = AAEvStand[i];
					for (int j = list.Count - 1; j >= 0; j--)
					{
						M2EventItem m2EventItem = list[j];
						float num = (float)i * 0.0625f;
						if (!this.FlgStandDecline.isActive() && m2EventItem.isContainingMv(FromMv, num + m2EventItem.stand_extend_map_w, num + 0.25f))
						{
							if (i == 0 && AAEvStand[1].IndexOf(m2EventItem) < 0 && (!m2EventItem.hasStand() || m2EventItem.canExecutable(M2EventItem.CMD.STAND)) && (!flag2 || m2EventItem.stand_executable_on_event))
							{
								AAEvStand[1].Add(m2EventItem);
								m2EventItem.execute(M2EventItem.CMD.STAND, FromMv);
								flag2 = (executed = true);
							}
						}
						else if (i == 1)
						{
							list.RemoveAt(j);
							m2EventItem.execute(M2EventItem.CMD.STAND_RELEASE, FromMv);
							executed = true;
						}
					}
				}
			}
			if (AATalkable == null)
			{
				flag = true;
			}
			else if (AATalkable.Count > aim)
			{
				List<IM2TalkableObject> list2 = AATalkable[aim];
				float num2 = -1f;
				IM2TalkableObject im2TalkableObject = null;
				if (list2 != null && list2.Count > 0)
				{
					float num3 = FromMv.x;
					float num4 = FromMv.y;
					float num5 = FromMv.x;
					float num6 = FromMv.y;
					float num7 = num3;
					float num8 = num4;
					float num9 = (float)CAim._XD(aim, 1);
					float num10 = (float)(-(float)CAim._YD(aim, 1));
					if (num9 != 0f)
					{
						float num11 = num9 * chk_len;
						num7 += num11;
						if (num9 > 0f)
						{
							num5 = num7;
						}
						if (num9 < 0f)
						{
							num3 = num7;
						}
					}
					if (num10 != 0f)
					{
						float num12 = (FromMv.sizey + chk_len) * num10;
						num8 += num12;
						if (num10 > 0f)
						{
							num6 = num8;
							num4 += (FromMv.sizey + 0.0625f) * num10;
						}
						if (num10 < 0f)
						{
							num4 = num8;
							num6 += (FromMv.sizey + 0.0625f) * num10;
						}
					}
					else
					{
						num4 = FromMv.mtop + 0.0625f;
						num6 = FromMv.mbottom - 0.0625f;
					}
					float num13 = ((num9 == 0f) ? 0.125f : 1f);
					float num14 = ((num10 == 0f) ? 0.125f : 1f);
					for (int k = list2.Count - 1; k >= 0; k--)
					{
						IM2TalkableObject im2TalkableObject2 = list2[k];
						int num15 = im2TalkableObject2.canTalkable(when_battle_busy);
						if (num15 <= 0)
						{
							if (num15 != 0)
							{
								list2.RemoveAt(k);
							}
						}
						else
						{
							Vector4 mapPosAndSizeTalkableObject = im2TalkableObject2.getMapPosAndSizeTalkableObject();
							if (X.isCovering(mapPosAndSizeTalkableObject.x - mapPosAndSizeTalkableObject.z, mapPosAndSizeTalkableObject.x + mapPosAndSizeTalkableObject.z, num3, num5, 0f) && X.isCovering(mapPosAndSizeTalkableObject.y - mapPosAndSizeTalkableObject.w, mapPosAndSizeTalkableObject.y + mapPosAndSizeTalkableObject.w, num4, num6, 0f))
							{
								float num16 = X.LENGTHXYRECTS2((mapPosAndSizeTalkableObject.x - mapPosAndSizeTalkableObject.z) * num13, (mapPosAndSizeTalkableObject.y - mapPosAndSizeTalkableObject.w) * num14, (mapPosAndSizeTalkableObject.x + mapPosAndSizeTalkableObject.z) * num13, (mapPosAndSizeTalkableObject.y + mapPosAndSizeTalkableObject.w) * num14, num3 * num13, num4 * num14, num5 * num13, num6 * num14);
								if (num16 > 1E-05f)
								{
									num16 += 1000f;
								}
								else
								{
									num16 += X.LENGTHXYS(mapPosAndSizeTalkableObject.x * num13, mapPosAndSizeTalkableObject.y * num14, num7 * num13, num8 * num14);
								}
								if (num9 != 0f && 0f < num9 != FromMv.x < mapPosAndSizeTalkableObject.x)
								{
									num2 += 0.25f;
								}
								if (num10 != 0f && 0f < num10 != FromMv.y < mapPosAndSizeTalkableObject.y)
								{
									num2 += 0.25f;
								}
								if (num2 < 0f || num16 < num2)
								{
									num2 = num16;
									im2TalkableObject = im2TalkableObject2;
								}
							}
						}
					}
					if (im2TalkableObject != null)
					{
						IM2TalkableObject im2TalkableObject3 = im2TalkableObject;
						Vector4 vector = im2TalkableObject3.getMapPosAndSizeTalkableObject();
						im2TalkableObject = null;
						num2 = 0f;
						if (num9 != 0f)
						{
							num2 += X.Abs(vector.x - FromMv.x);
						}
						if (num10 != 0f)
						{
							num2 += X.Abs(vector.y - FromMv.y);
						}
						for (int l = list2.Count - 1; l >= 0; l--)
						{
							IM2TalkableObject im2TalkableObject4 = list2[l];
							if (im2TalkableObject4 != im2TalkableObject3)
							{
								Vector4 mapPosAndSizeTalkableObject2 = im2TalkableObject4.getMapPosAndSizeTalkableObject();
								if ((num9 == 0f || FromMv.x < vector.x == FromMv.x < mapPosAndSizeTalkableObject2.x || X.isContaining(vector.x - vector.z - 0.125f, vector.x + vector.z + 0.125f, num3 - chk_len, num5 + chk_len, 0f)) && (num10 == 0f || FromMv.y < vector.y == FromMv.y < mapPosAndSizeTalkableObject2.y || X.isContaining(vector.y - vector.w - 0.125f, vector.y + vector.w + 0.125f, num4 - chk_len, num6 + chk_len, 0f)) && ((num9 != 0f) ? X.BTW(num3, mapPosAndSizeTalkableObject2.x, num5) : X.isCovering(mapPosAndSizeTalkableObject2.x - mapPosAndSizeTalkableObject2.z, mapPosAndSizeTalkableObject2.x + mapPosAndSizeTalkableObject2.z, num3, num5, 0f)) && ((num10 != 0f) ? X.BTW(num4, mapPosAndSizeTalkableObject2.y, num6) : X.isCovering(mapPosAndSizeTalkableObject2.y - mapPosAndSizeTalkableObject2.w, mapPosAndSizeTalkableObject2.y + mapPosAndSizeTalkableObject2.w, num4, num6, 0f)))
								{
									float num17 = 0f;
									if (num9 != 0f)
									{
										num17 += X.Abs(mapPosAndSizeTalkableObject2.x - FromMv.x);
									}
									if (num10 != 0f)
									{
										num17 += X.Abs(mapPosAndSizeTalkableObject2.y - FromMv.y);
									}
									if ((num2 < 0f || num17 < num2) && (num9 <= 0f || mapPosAndSizeTalkableObject2.x - mapPosAndSizeTalkableObject2.z >= num3) && (num9 >= 0f || mapPosAndSizeTalkableObject2.x + mapPosAndSizeTalkableObject2.z < num5) && (num10 <= 0f || mapPosAndSizeTalkableObject2.y - mapPosAndSizeTalkableObject2.w >= num4) && (num10 >= 0f || mapPosAndSizeTalkableObject2.y + mapPosAndSizeTalkableObject2.w < num6))
									{
										num2 = num17;
										im2TalkableObject = im2TalkableObject4;
									}
								}
							}
						}
						if (im2TalkableObject != null)
						{
							im2TalkableObject3 = im2TalkableObject;
						}
						vector = im2TalkableObject3.getMapPosAndSizeTalkableObject();
						num2 = 0f;
						if (num9 != 0f)
						{
							num2 += X.Abs(vector.x - num7);
						}
						if (num10 != 0f)
						{
							num2 += X.Abs(vector.y - num8);
						}
						for (int m = list2.Count - 1; m >= 0; m--)
						{
							IM2TalkableObject im2TalkableObject5 = list2[m];
							if (im2TalkableObject5 != im2TalkableObject3)
							{
								Vector4 mapPosAndSizeTalkableObject3 = im2TalkableObject5.getMapPosAndSizeTalkableObject();
								if (((num9 < 0f) ? X.BTW(vector.x, mapPosAndSizeTalkableObject3.x, num7) : ((num9 > 0f) ? X.BTW(num7, mapPosAndSizeTalkableObject3.x, vector.x) : X.isCovering(mapPosAndSizeTalkableObject3.x - mapPosAndSizeTalkableObject3.z, mapPosAndSizeTalkableObject3.x + mapPosAndSizeTalkableObject3.z, num3, num5, 0f))) && ((num10 < 0f) ? X.BTW(vector.y, mapPosAndSizeTalkableObject3.y, num8) : ((num10 > 0f) ? X.BTW(num8, mapPosAndSizeTalkableObject3.y, vector.y) : X.isCovering(mapPosAndSizeTalkableObject3.y - mapPosAndSizeTalkableObject3.w, mapPosAndSizeTalkableObject3.y + mapPosAndSizeTalkableObject3.w, num4, num6, 0f))))
								{
									float num18 = 0f;
									if (num9 != 0f)
									{
										num18 += X.Abs(mapPosAndSizeTalkableObject3.x - num7);
									}
									if (num10 != 0f)
									{
										num18 += X.Abs(mapPosAndSizeTalkableObject3.y - num8);
									}
									if (num2 < 0f || num18 < num2)
									{
										num2 = num18;
										im2TalkableObject = im2TalkableObject5;
									}
								}
							}
						}
						if (im2TalkableObject != null)
						{
							im2TalkableObject3 = im2TalkableObject;
						}
						im2TalkableObject = im2TalkableObject3;
					}
				}
				if (!this.Mp.setTalkTarget(im2TalkableObject, false))
				{
					flag = true;
				}
			}
			return flag;
		}

		public M2EventItem Get(int key)
		{
			return this.AEv[key];
		}

		public M2EventItem CreateAndAssign(string key)
		{
			M2EventItem m2EventItem = this.Get(key, true, true);
			if (m2EventItem != null)
			{
				if (this.AnotInitEvent != null)
				{
					this.AnotInitEvent.Remove(m2EventItem);
				}
				return m2EventItem;
			}
			m2EventItem = this.CreateDefault<M2EventItem>(key);
			if (!m2EventItem.do_not_map_assign)
			{
				this.Mp.assignMover(m2EventItem, false);
			}
			X.pushToEmptyRI<M2EventItem>(ref this.AEv, m2EventItem, -1);
			return m2EventItem;
		}

		private T CreateDefault<T>(string key) where T : M2EventItem
		{
			T t = IN.CreateGob(this.Mp.gameObject, "-Ev-" + key).AddComponent<T>();
			t.key = key;
			t.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
			return t;
		}

		public T CreateAndAssignT<T>(string key, bool no_error = false) where T : M2EventItem
		{
			T t = this.Get(key, true, true) as T;
			if (t != null)
			{
				if (!no_error)
				{
					X.dl("既にイベント" + key + "が存在します", null, false, false);
				}
				if (this.AnotInitEvent != null)
				{
					this.AnotInitEvent.Remove(t);
				}
				return t;
			}
			t = this.CreateDefault<T>(key);
			if (!t.do_not_map_assign)
			{
				this.Mp.assignMover(t, false);
			}
			X.pushToEmptyRI<M2EventItem>(ref this.AEv, t, -1);
			return t;
		}

		public M2EventItem Get(string key, bool no_make = true, bool no_error = false)
		{
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.AEv[i] != null && this.AEv[i].key == key)
				{
					return this.AEv[i];
				}
			}
			if (no_make)
			{
				if (!no_error)
				{
					X.de("マップイベントオブジェクト " + key + " がありません。", null);
				}
				return null;
			}
			M2EventItem m2EventItem = this.CreateDefault<M2EventItem>(key);
			m2EventItem.create_from_evc = true;
			return m2EventItem;
		}

		public M2EventContainer remove(M2EventItem Ev)
		{
			X.emptySpecific<M2EventItem>(this.AEv, Ev, -1);
			if (this.AnotInitEvent != null)
			{
				this.AnotInitEvent.Remove(Ev);
			}
			return this;
		}

		public void destructReservedCheck()
		{
			if (this.AnotInitEvent != null)
			{
				List<M2EventItem> anotInitEvent = this.AnotInitEvent;
				this.AnotInitEvent = null;
				for (int i = anotInitEvent.Count - 1; i >= 0; i--)
				{
					anotInitEvent[i].destruct();
				}
			}
		}

		public int countEvents()
		{
			return X.countNotEmpty<M2EventItem>(this.AEv);
		}

		public void addClearingRemoveObject(M2Mover M)
		{
			if (this.AClearingObject == null)
			{
				this.AClearingObject = new List<M2Mover>(1);
			}
			this.AClearingObject.Add(M);
		}

		public void executeClearingRemoveObject()
		{
			if (this.AClearingObject != null)
			{
				for (int i = this.AClearingObject.Count - 1; i >= 0; i--)
				{
					M2Mover m2Mover = this.AClearingObject[i];
					if (m2Mover is M2EventItem)
					{
						this.Mp.destructEvent(m2Mover as M2EventItem);
					}
					else
					{
						this.Mp.removeMover(m2Mover);
					}
				}
				this.AClearingObject = null;
			}
		}

		public M2EventContainer clear(bool only_from_created_in_evc = false)
		{
			this.executeClearingRemoveObject();
			for (int i = this.AEv.Length - 1; i >= 0; i--)
			{
				M2EventItem m2EventItem = this.AEv[i];
				if (m2EventItem != null && (!only_from_created_in_evc || m2EventItem.create_from_evc))
				{
					m2EventItem.destruct();
				}
			}
			if (!only_from_created_in_evc)
			{
				this.OASpecialCmd.Clear();
				this.ABccFootable = null;
			}
			this.Mp.cmd_loadevent_execute &= 4294967293U;
			return this;
		}

		public void releaseBccCache()
		{
			this.ABccFootable = null;
		}

		public Vector3 getRandomFootable(M2EventItem Mv, M2LabelPoint Lp)
		{
			int num = this.Mp.crop;
			int num2 = this.Mp.crop;
			int num3 = this.Mp.clms - this.Mp.crop;
			int num4 = this.Mp.rows - this.Mp.crop;
			if (Lp != null)
			{
				num = Lp.mapx;
				num2 = Lp.mapy;
				num3 = Lp.mapx + Lp.mapw;
				num4 = Lp.mapy + Lp.maph;
			}
			if (this.ABccFootable == null)
			{
				this.ABccFootable = new List<M2BlockColliderContainer.BCCLine>(24);
			}
			if (this.ABccFootable.Count == 0)
			{
				M2BlockColliderContainer.BCCIterator bcciterator;
				this.Mp.BCC.ItrInit(out bcciterator, true);
				while (bcciterator.Next())
				{
					if (bcciterator.Cur.foot_aim == AIM.B)
					{
						this.ABccFootable.Add(bcciterator.Cur);
					}
				}
			}
			int count = this.ABccFootable.Count;
			if (count > 0)
			{
				using (BList<M2BlockColliderContainer.BCCLine> blist = ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(count))
				{
					for (int i = 0; i < 2; i++)
					{
						for (int j = 0; j < count; j++)
						{
							M2BlockColliderContainer.BCCLine bccline = this.ABccFootable[j];
							if (((i == 0) ? X.isContaining((float)num2, (float)num4, bccline.y, bccline.bottom, 0f) : X.isCovering((float)num2, (float)num4, bccline.y, bccline.bottom, 0f)) && X.isCovering((float)num, (float)num3, bccline.x, bccline.right, 0f))
							{
								blist.Add(bccline);
							}
						}
						if (blist.Count > 0)
						{
							break;
						}
					}
					if (blist.Count > 0)
					{
						M2BlockColliderContainer.BCCLine bccline2 = blist[X.xors(blist.Count)];
						float num5 = X.NIXP(bccline2.x + Mv.sizex, bccline2.right - Mv.sizex);
						return new Vector3(num5, bccline2.slopeBottomY(num5), 1f);
					}
				}
			}
			return new Vector3(Mv.x, Mv.mbottom, 0f);
		}

		internal bool evOpen(bool is_init)
		{
			if (is_init)
			{
				int num = this.AEv.Length;
				for (int i = 0; i < num; i++)
				{
					M2EventItem m2EventItem = this.AEv[i];
					if (m2EventItem == null)
					{
						break;
					}
					m2EventItem.savePreFloat();
				}
			}
			return true;
		}

		internal bool evClose(bool is_end)
		{
			if (is_end)
			{
				int num = this.AEv.Length;
				for (int i = 0; i < num; i++)
				{
					M2EventItem m2EventItem = this.AEv[i];
					if (m2EventItem == null)
					{
						break;
					}
					m2EventItem.event_stop = false;
				}
			}
			return true;
		}

		internal bool isMoveScriptActive(bool only_moved_by_event = false)
		{
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				M2EventItem m2EventItem = this.AEv[i];
				if (m2EventItem == null)
				{
					break;
				}
				if (m2EventItem.isMoveScriptActive(only_moved_by_event))
				{
					return true;
				}
			}
			return false;
		}

		public void closeAllStacked()
		{
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				M2EventItem m2EventItem = this.AEv[i];
				if (m2EventItem == null)
				{
					break;
				}
				m2EventItem.closeAllStacked();
			}
		}

		public void fineCheckDescName()
		{
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				M2EventItem m2EventItem = this.AEv[i];
				if (m2EventItem == null)
				{
					break;
				}
				m2EventItem.fineCheckDescName();
			}
		}

		public bool setSpecialCommand(string key, M2EventCommand _Cmd)
		{
			key = key.ToUpper();
			List<M2EventCommand> list = X.Get<string, List<M2EventCommand>>(this.OASpecialCmd, key);
			if (list == null)
			{
				list = (this.OASpecialCmd[key] = new List<M2EventCommand>());
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].name == _Cmd.name)
				{
					return false;
				}
			}
			list.Add(_Cmd);
			return true;
		}

		public bool stackSpecialCommand(string key, IM2EvTrigger executed_by, bool execute_stack = true)
		{
			key = key.ToUpper();
			bool flag = false;
			List<M2EventCommand> list = X.Get<string, List<M2EventCommand>>(this.OASpecialCmd, key);
			if (list != null)
			{
				int count = list.Count;
				if (!execute_stack && count > 0)
				{
					return true;
				}
				for (int i = 0; i < count; i++)
				{
					this.Mp.stackEventCommand(list[i], executed_by);
					flag = true;
				}
			}
			return flag;
		}

		public void runLoadExecuteNotAssinged()
		{
			if ((this.Mp.cmd_loadevent_execute & 1U) == 0U && EV.isActive(false))
			{
				return;
			}
			this.Mp.cmd_loadevent_execute &= 4294967292U;
			int num = this.AEv.Length;
			for (int i = 0; i < num; i++)
			{
				M2EventItem m2EventItem = this.AEv[i];
				if (m2EventItem == null)
				{
					break;
				}
				if (m2EventItem.need_load_event_check)
				{
					m2EventItem.evLoadExecute();
				}
			}
		}

		public void addReloadListener(IListenerEvcReload _Fn)
		{
			if (this.Mp.is_submap)
			{
				M2EventContainer eventContainer = this.Mp.SubMapData.getBaseMap().getEventContainer();
				if (eventContainer != null)
				{
					eventContainer.addReloadListener(_Fn);
				}
				return;
			}
			aBtn.addFnT<IListenerEvcReload>(ref this.AFnReload, _Fn);
			this.Mp.cmd_reload_flg |= 1U;
		}

		public void remReloadListener(IListenerEvcReload _Fn)
		{
			if (this.Mp.is_submap)
			{
				M2EventContainer eventContainer = this.Mp.SubMapData.getBaseMap().getEventContainer();
				if (eventContainer != null)
				{
					eventContainer.remReloadListener(_Fn);
				}
				return;
			}
			X.emptySpecific<IListenerEvcReload>(this.AFnReload, _Fn, -1);
		}

		protected bool runFn(IListenerEvcReload[] AFn)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				IListenerEvcReload listenerEvcReload = AFn[i];
				if (listenerEvcReload == null)
				{
					return flag;
				}
				flag = listenerEvcReload.EvtM2Reload(this.Mp) && flag;
			}
			return flag;
		}

		public override CsvReader parseText(string basetext)
		{
			base.parseText(basetext);
			base.seekReset();
			return this;
		}

		private readonly Map2d Mp;

		private M2EventItem[] AEv;

		private static readonly Regex RegEventDefine = new Regex("^(\\!)?(\\w+)");

		private static readonly Regex RegEvTitle = new Regex("^([\\$\\w]+)");

		private IListenerEvcReload[] AFnReload;

		private readonly BDic<string, List<M2EventCommand>> OASpecialCmd;

		private List<M2EventItem> AnotInitEvent;

		private List<M2Mover> AClearingObject;

		public Flagger FlgStandDecline;

		private const float default_sp_shift_x = 0f;

		private const float default_sp_shift_y = 3.5f;

		private List<M2BlockColliderContainer.BCCLine> ABccFootable;

		public const string sp_ev_key_pre_unload = "PRE_UNLOAD";

		public const string sp_ev_key_pre_load = "PRE_LOAD";

		public const string sp_ev_key_juice_explode = "JUICE_EXPL";
	}
}
