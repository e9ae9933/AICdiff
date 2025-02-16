using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class NelMSGContainer : IMessageContainer, IValotileSetable
	{
		public NelMSGContainer()
		{
			IN.getGUICamera();
			this.AMsg = new List<NelMSG>(4);
			this.AMsgBuf = new List<NelMSG>(4);
			this.AMsgActive = new List<NelMSG>(4);
			this.OHkdsInfo = new BDic<string, NelMSGContainer.HkdsInfo>(4);
			this.Omsg_prefix = new BDic<string, string>(1);
			EV.FnCreateConfirmer = new Func<GameObject, ConfirmAnnouncer>(this.createConfirmAnnouncer);
		}

		public ConfirmAnnouncer createConfirmAnnouncer(GameObject _Gob)
		{
			ConfirmAnnouncer confirmAnnouncer = IN.CreateGobGUI(_Gob, "-Confirmer").AddComponent<ConfirmAnnouncer>();
			confirmAnnouncer.DefaultCol = NEL.ColText;
			this.TxHidingKC = IN.CreateGobGUI(EV.Gob, "-MsgTuto").AddComponent<TextRenderer>();
			this.TxHidingKC.html_mode = true;
			this.TxHidingKC.letter_spacing = 0.95f;
			this.TxHidingKC.color_apply_to_image = true;
			this.TxHidingKC.alignx = ALIGN.RIGHT;
			this.TxHidingKC.Col(NEL.ColText);
			this.TxHidingKC.MakeValot(null, CameraBidingsBehaviour.UiBind);
			this.TxHidingKC.use_valotile = this.use_valotile_;
			return confirmAnnouncer;
		}

		public void fineTargetFont()
		{
			MFont defaultFont = TX.getDefaultFont();
			if (this.Confirmer != null)
			{
				this.Confirmer.TargetFont = defaultFont;
			}
			if (this.TxHidingKC != null)
			{
				for (int i = this.AMsg.Count - 1; i >= 0; i--)
				{
					try
					{
						this.AMsg[i].fineTargetFont(defaultFont);
					}
					catch
					{
					}
				}
			}
		}

		public void destructGob()
		{
			if (this.TxHidingKC != null)
			{
				IN.DestroyOne(this.TxHidingKC.gameObject);
			}
			this.TxHidingKC = null;
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				try
				{
					NelMSG nelMSG = this.AMsg[i];
					nelMSG.releaseDrawer(true);
					IN.DestroyOne(nelMSG.gameObject);
				}
				catch
				{
				}
			}
			this.AMsg.Clear();
		}

		public void initEvent(EvMsgCommand CmdListener)
		{
			NelMSGResource.initResource(false);
			this.need_reposit_flag_ = false;
			this.last_label = null;
			this.auto_msg_hide = false;
			this.handle = true;
			this.Omsg_prefix.Clear();
			this.OHkdsInfo.Clear();
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				this.AMsg[i].releaseInfo();
			}
			this.msgcmd_run_flag = 0;
			this.t_temphd = -1f;
			this.TS = 1f;
		}

		public void quitEvent()
		{
			if (this.TxHidingKC == null)
			{
				return;
			}
			this.hideMsg(false);
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				this.AMsg[i].releaseReserved();
			}
			this.auto_msg_hide = false;
			if (X.DEBUGRELOADMTR)
			{
				NelMSGResource.setRecheckContentFlag();
			}
			this.TxHidingKC.gameObject.SetActive(false);
			this.Omsg_prefix.Clear();
			this.OHkdsInfo.Clear();
			this.handle = false;
			this.msgcmd_run_flag = 0;
			this.LastMessageER = null;
			this.last_message_key = (this.last_label = null);
		}

		public int makeFromEvent(StringHolder rER, EvReader curEv)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (!(cmd == "MSG"))
				{
					if (cmd == "HKDS")
					{
						NelMSGContainer.HkdsInfo hkdsInfo = X.Get<string, NelMSGContainer.HkdsInfo>(this.OHkdsInfo, rER._1);
						if (hkdsInfo == null)
						{
							hkdsInfo = (this.OHkdsInfo[rER._1] = new NelMSGContainer.HkdsInfo("=", "=", "=", "="));
						}
						if (rER._2 != "=")
						{
							hkdsInfo.pos_fix_key = rER._2;
						}
						if (rER._3 != "=")
						{
							hkdsInfo.follow_to_key = rER._3;
						}
						if (rER._4 != "=")
						{
							hkdsInfo.bounds_key = rER._4;
						}
						NelMSG nelMSG = this.GetById(rER._1);
						if (nelMSG != null && nelMSG.isActive())
						{
							nelMSG.readInfo(hkdsInfo);
						}
						return 2;
					}
					if (cmd == "TALKER_REPLACE")
					{
						NelMSGContainer.HkdsInfo hkdsInfo = X.Get<string, NelMSGContainer.HkdsInfo>(this.OHkdsInfo, rER._1);
						if (hkdsInfo == null)
						{
							hkdsInfo = (this.OHkdsInfo[rER._1] = new NelMSGContainer.HkdsInfo("=", "=", "=", "="));
							hkdsInfo.talker_replace_key = ((rER.clength > 2) ? rER._2 : null);
							hkdsInfo.talker_snd_key = ((rER.clength > 3) ? rER._3 : null);
						}
						else
						{
							if (rER._2 != "=")
							{
								hkdsInfo.talker_replace_key = ((rER.clength > 2) ? rER._2 : null);
							}
							if (rER._3 != "=")
							{
								hkdsInfo.talker_snd_key = ((rER.clength > 3) ? rER._3 : null);
							}
						}
						NelMSG nelMSG = this.GetById(rER._1);
						if (nelMSG != null && nelMSG.isActive())
						{
							nelMSG.readInfo(hkdsInfo);
						}
						return 2;
					}
					if (cmd == "AUTO_MSG_HIDE")
					{
						this.auto_msg_hide = rER.Nm(1, 1f) != 0f;
						return 2;
					}
					if (cmd == "MSG_PREFIX")
					{
						if (rER.clength >= 2)
						{
							this.Omsg_prefix[rER._1] = rER._2;
						}
						return 2;
					}
				}
				else
				{
					string _ = rER._1;
					string name = curEv.name;
					if (TX.valid(rER._2))
					{
						rER._2.IndexOf('B');
					}
					NelMSG nelMSG = this.createDrawer(curEv.name, rER._1, null, true, NelMSG.HKDS_BOUNDS._OFFLINE, NelMSG.HKDSTYPE._MAX, rER._2, (rER == curEv) ? curEv : null);
					if (nelMSG != null)
					{
						this.msgcmd_run_flag = 1;
						if (this.auto_msg_hide)
						{
							this.hideMsg(false, nelMSG);
						}
						this.last_label = _;
						int num = 1;
						if (TX.valid(rER._2))
						{
							if (rER._2.IndexOf('I') >= 0)
							{
								nelMSG.showImmediate(true, false);
							}
							if (rER._2.IndexOf('C') >= 0)
							{
								num = 2;
							}
						}
						return num;
					}
				}
			}
			return 0;
		}

		public NelMSG createDrawer(string ev_name, string msg_label, string fixpos_key, bool read_info = true, NelMSG.HKDS_BOUNDS bd = NelMSG.HKDS_BOUNDS._OFFLINE, NelMSG.HKDSTYPE hkds_type = NelMSG.HKDSTYPE._MAX, string _flags = null, EvReader Reader = null)
		{
			string text = msg_label;
			string text2 = "";
			string text3 = null;
			EvPerson evPerson = null;
			string text4 = null;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			BList<string> blist = null;
			if (TX.valid(_flags))
			{
				while (_flags.IndexOf('X') >= 0 && REG.match(_flags, NelMSGContainer.RegMsgTxInjection))
				{
					_flags = REG.leftContext + REG.rightContext;
					string r = REG.R1;
					if (blist == null)
					{
						blist = ListBuffer<string>.Pop(1);
					}
					blist.Add(r);
				}
				if (_flags.IndexOf('T') >= 0)
				{
					hkds_type = NelMSG.HKDSTYPE.THINK;
				}
				if (_flags.IndexOf('A') >= 0)
				{
					hkds_type = NelMSG.HKDSTYPE.ANGRY;
				}
				if (_flags.IndexOf('D') >= 0)
				{
					hkds_type = NelMSG.HKDSTYPE.DEVICE;
				}
				if (_flags.IndexOf('V') >= 0)
				{
					hkds_type = NelMSG.HKDSTYPE.EVIL;
				}
				if (_flags.IndexOf('B') >= 0)
				{
					flag = true;
				}
				if (_flags.IndexOf('M') >= 0)
				{
					flag2 = true;
				}
				if (_flags.IndexOf('F') >= 0)
				{
					flag3 = true;
				}
				if (_flags.IndexOf('S') >= 0)
				{
					flag4 = true;
				}
			}
			string text5 = null;
			if (REG.match(msg_label, NelMSGContainer.RegEventNameReplaceKey))
			{
				ev_name = REG.R1;
				text = REG.R2;
			}
			if (_flags != null && _flags.IndexOf('K') >= 0 && REG.match(_flags, NelMSGContainer.RegMsgFlagBookPos))
			{
				hkds_type = NelMSG.HKDSTYPE.BOOK;
				fixpos_key = REG.R1;
				string text6 = "@" + fixpos_key;
				text2 = "BOOK" + text6;
				text4 = "BOOK";
				text5 = NelMSGContainer.checkHereDocument(REG.R2, ev_name, text, Reader, false, null, false);
			}
			else if (REG.match(text, NelMSGContainer.RegPersonKey))
			{
				string r2;
				text4 = (r2 = REG.R1);
				text2 = r2;
				evPerson = EvPerson.getPerson(r2, null);
				text5 = NelMSGContainer.checkHereDocument(REG.R2, ev_name, r2, Reader, false, null, false);
			}
			if (TX.valid(_flags) && _flags.IndexOf('R') >= 0 && REG.match(_flags, NelMSGContainer.RegMsgFlagLabelReplace))
			{
				text3 = REG.R1;
			}
			NelMSGContainer.HkdsInfo hkdsInfo = (read_info ? X.Get<string, NelMSGContainer.HkdsInfo>(this.OHkdsInfo, text2) : null);
			if (text5 == null)
			{
				text5 = (this.last_resource_label = ev_name + " " + text);
			}
			bool flag5;
			bool flag6;
			NelMSG nelMSG = this.makeMessageInner(out flag5, out flag6, ev_name, text2, hkds_type, text5, text3, _flags, flag, evPerson, fixpos_key, hkdsInfo, bd, text4, flag3, flag2, false, null);
			if (blist != null)
			{
				if (flag6)
				{
					EV.Log.AddBufferInjection(blist.ToArray());
				}
				nelMSG.replaceTxInjection(blist);
				ListBuffer<string>.Release(blist);
			}
			if (flag4)
			{
				int num = 0;
				nelMSG.initNestShuffle(0, 30f);
				for (;;)
				{
					string text7 = nelMSG.popReservedContent();
					if (text7 == null)
					{
						break;
					}
					bool flag7;
					NelMSG nelMSG2 = this.makeMessageInner(out flag7, out flag6, ev_name, null, hkds_type, text5, "%%NOUSE", _flags, flag, evPerson, fixpos_key, null, bd, text4, flag3, flag2, true, text7);
					nelMSG2.readInfo(hkdsInfo);
					nelMSG2.reservePersonForNest(text2, nelMSG);
					nelMSG2.initNestShuffle(++num, 30f);
					flag5 = flag5 || flag7;
				}
			}
			if (!flag5)
			{
				return null;
			}
			return nelMSG;
		}

		private NelMSG makeMessageInner(out bool msg_created, out bool log_added, string ev_name, string hkds_id, NelMSG.HKDSTYPE hkds_type, string label, string backlog_key, string _flags, bool set_behind, EvPerson P, string fixpos_key, NelMSGContainer.HkdsInfo HkInfo, NelMSG.HKDS_BOUNDS bd, string log_person, bool always_focus, bool merge_flag, bool create_new = false, string replace_string = null)
		{
			NelMSG byDrawer = this.getByDrawer(hkds_id, false, hkds_type, set_behind, -1);
			log_added = false;
			if (hkds_id != null && !byDrawer.isSame(hkds_id, P))
			{
				byDrawer.initPerson(hkds_id, P);
			}
			if (fixpos_key != null)
			{
				byDrawer.initPositionFix(fixpos_key);
			}
			if (HkInfo != null)
			{
				byDrawer.readInfo(HkInfo);
			}
			if (bd != NelMSG.HKDS_BOUNDS._OFFLINE)
			{
				byDrawer.setBoundsType(bd, false);
			}
			if (backlog_key != "%%NOUSE" && EV.Log != null && log_person != null && EV.isActive(false))
			{
				EV.Log.AddBuffer(log_person, (backlog_key != null) ? (ev_name + " " + backlog_key) : label);
				log_added = true;
			}
			if (replace_string == null)
			{
				msg_created = byDrawer.makeMessage(label, P, hkds_type == NelMSG.HKDSTYPE._MAX, merge_flag);
			}
			else
			{
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					blist.Add(replace_string);
					msg_created = byDrawer.makeMessage(blist, P, hkds_type == NelMSG.HKDSTYPE._MAX, merge_flag);
				}
			}
			if (always_focus)
			{
				byDrawer.always_focus_color = true;
				byDrawer.activateFront();
			}
			this.fine_z = true;
			this.msgf = 0f;
			if (TX.valid(_flags) && _flags.IndexOf('P') >= 0 && REG.match(_flags, NelMSGContainer.RegMsgFlagAutoProgress))
			{
				byDrawer.maxt_auto_progress = X.Nm(REG.R1, -1f, false);
			}
			return byDrawer;
		}

		public static string checkHereDocument(string here_label, string ev_name, string person, CsvReader ER, bool return_first_content = false, List<string> Adest = null, bool no_eplode_varcon = false)
		{
			if (!TX.isStart(here_label, "<<<", 0))
			{
				return null;
			}
			if (ER == null)
			{
				ER.tError("Do not allowed using the here document function from TL/MTL.");
				return null;
			}
			string text = TX.slice(here_label, 3);
			bool flag = !no_eplode_varcon;
			if (TX.isStart(text, '\'') || TX.isEnd(text, '\''))
			{
				flag = false;
				text = TX.slice(text, 1, text.Length - 1) + ";";
			}
			else
			{
				text += ";";
			}
			int cur_line = ER.get_cur_line();
			string text2;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					while (ER.readCorrectly())
					{
						if (ER.getLastStr() == "*")
						{
							blist.Add(stb.ToString());
							stb.Clear();
						}
						else
						{
							if (ER.getLastStr() == text)
							{
								break;
							}
							if (flag)
							{
								ER.VarCon.read(ER, ER.getLastStr(), ER.no_write_varcon == 0, ER.no_replace_quote);
								ER.copyLastStrToOneCmd();
								ER.readAfterForVarCon(false);
								stb.AR(ER.cmd);
							}
							else
							{
								stb.AR(ER.getLastStr());
							}
						}
					}
					if (stb.Length > 0)
					{
						blist.Add(stb.ToString());
						stb.Clear();
					}
				}
				if (blist.Count == 0)
				{
					ER.tError("Encountered the end of File.");
					text2 = null;
				}
				else
				{
					if (Adest != null)
					{
						Adest.AddRange(blist);
					}
					string text3 = null;
					if (person != null)
					{
						text3 = string.Concat(new string[]
						{
							ev_name,
							" ",
							person,
							"_<<<.",
							cur_line.ToString(),
							"_",
							ER.get_cur_line().ToString()
						});
						NelMSGResource.addAdditionalLabel(text3, blist.ToArray());
					}
					text2 = (return_first_content ? blist[0] : text3);
				}
			}
			return text2;
		}

		private NelMSG getByDrawer(string hkds_id, bool no_make = true, NelMSG.HKDSTYPE type = NelMSG.HKDSTYPE._MAX, bool set_behind = false, int find_for_nest = -1)
		{
			int num = this.AMsg.Count;
			int num2 = -1;
			NelMSG nelMSG;
			if (hkds_id != null)
			{
				for (int i = 0; i < num; i++)
				{
					nelMSG = this.AMsg[i];
					if (((find_for_nest >= 0) ? (!nelMSG.isActive() && nelMSG.hasReservedContent() && (int)nelMSG.nest_shuffle == find_for_nest + 1) : (nelMSG.isActive() && (hkds_id != null || !nelMSG.hasReservedContent()))) && nelMSG.isSame(hkds_id))
					{
						num2 = i;
						break;
					}
				}
			}
			if (num2 == -1)
			{
				if (find_for_nest >= 0)
				{
					return null;
				}
				for (int j = 0; j < num; j++)
				{
					nelMSG = this.AMsg[j];
					if (nelMSG.isCompletelyHidden() && (hkds_id != null || !nelMSG.hasReservedContent()))
					{
						num2 = j;
						break;
					}
				}
			}
			bool flag = false;
			if (num2 >= 0)
			{
				nelMSG = this.AMsg[num2];
				if (!nelMSG.isActive() && type == NelMSG.HKDSTYPE._MAX)
				{
					type = NelMSG.HKDSTYPE.NORMAL;
				}
				nelMSG = this.AMsg[num2].initDrawer(hkds_id, false);
				if (!no_make)
				{
					this.msgf = 0f;
					this.AMsg.RemoveAt(num2);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				if (no_make)
				{
					return null;
				}
				int count = this.AMsg.Count;
				byte b = this.msg_index;
				this.msg_index = b + 1;
				nelMSG = NelMSG.createInstance(this, count, b).initDrawer(hkds_id, false);
				this.msgf = 0f;
				if (type == NelMSG.HKDSTYPE._MAX)
				{
					type = NelMSG.HKDSTYPE.NORMAL;
				}
			}
			if (!flag)
			{
				if (!set_behind || this.AMsg.Count == 0)
				{
					this.AMsg.Add(nelMSG);
					nelMSG.behind_flag = set_behind;
				}
				else
				{
					nelMSG.behind_flag = true;
					num = this.AMsg.Count;
					num2 = this.AMsg.Count;
					for (int k = 0; k < num; k++)
					{
						if (!this.AMsg[k].behind_flag)
						{
							num2 = k;
							break;
						}
					}
					this.AMsg.Insert(num2, nelMSG);
				}
			}
			if (hkds_id != null)
			{
				this.fineActivateFront(true);
				this.need_reposit_flag_ = true;
			}
			if (type != NelMSG.HKDSTYPE._MAX)
			{
				nelMSG.setHkdsType(type);
			}
			return nelMSG;
		}

		private void fineActivateFront(bool quit_temphide = false)
		{
			this.fine_z = true;
			bool flag = false;
			if (quit_temphide && this.t_temphd >= 0f)
			{
				this.t_temphd = -1f;
				if (this.TxHidingKC != null)
				{
					this.TxHidingKC.gameObject.SetActive(false);
				}
				for (int i = this.AMsg.Count - 1; i >= 0; i--)
				{
					this.AMsg[i].gameObject.SetActive(true);
				}
			}
			this.FrontMsg = null;
			for (int j = this.AMsg.Count - 1; j >= 0; j--)
			{
				NelMSG nelMSG = this.AMsg[j];
				if (nelMSG.isActive())
				{
					if (!flag && !nelMSG.behind_flag)
					{
						flag = true;
						this.FrontMsg = nelMSG;
						nelMSG.activateFront();
					}
					else if (nelMSG.always_focus_color)
					{
						nelMSG.activateFront();
					}
					else
					{
						nelMSG.deactivateFront();
					}
				}
			}
		}

		public void fineFrontConfirmer()
		{
			if (this.FrontMsg == null && this.AMsg.Count > 0)
			{
				this.fineActivateFront(false);
			}
			if (this.FrontMsg != null)
			{
				if (this.handle && !this.FrontMsg.has_nest_shuffle_buffer)
				{
					this.Confirmer.init(this.FrontMsg, this.FrontMsg.transform, true);
					return;
				}
				this.Confirmer.deactivate();
			}
		}

		public bool isHidingTemporary()
		{
			return this.t_temphd >= 0f;
		}

		public bool checkHideVisibilityTemporary(bool hideable, bool force_change = false)
		{
			bool flag;
			if (this.t_temphd < 0f)
			{
				flag = force_change || (hideable && IN.isUiRemPD());
			}
			else
			{
				flag = !force_change && (hideable && !IN.isUiRemPD() && !IN.isCancelPD() && !IN.isUiAddPD() && !IN.isLTabPD() && !IN.isRTabPD() && !IN.isSubmitPD(1) && !IN.isLP(1) && !IN.isRP(1) && !IN.isTP(1)) && !IN.isBP(1);
			}
			if (this.t_temphd < 0f && flag)
			{
				this.t_temphd = 0f;
				this.TxHidingKC.text_content = TX.Get("MsgCon_Hiding", "");
				this.TxHidingKC.transform.SetParent(EV.Gob.transform, false);
				this.TxHidingKC.BorderCol(MTRX.ColWhite);
				this.TxHidingKC.gameObject.SetActive(true);
				this.TxHidingKC.alpha = 0f;
				IN.PosP(this.TxHidingKC.transform, IN.wh - 30f, -IN.hh + 50f, -1.9000001f);
			}
			else
			{
				if (this.t_temphd < 0f || flag)
				{
					return false;
				}
				this.fineActivateFront(true);
				EV.need_fine_confirmer = true;
				this.TxHidingKC.gameObject.SetActive(false);
			}
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (this.t_temphd >= 0f || nelMSG.isActive())
				{
					nelMSG.gameObject.SetActive(this.t_temphd < 0f);
				}
				if (this.t_temphd >= 0f && nelMSG == this.FrontMsg && this.Confirmer.targetIs(nelMSG))
				{
					this.Confirmer.deactivate();
				}
			}
			return true;
		}

		private void repositAll()
		{
			this.need_reposit_flag_ = false;
			if (this.use_valotile)
			{
				CameraBidingsBehaviour.UiBind.need_sort_binds = true;
			}
			int num;
			TalkDrawer[] talkerDrawerList = EV.getTalkerDrawerList(out num);
			int count = this.AMsg.Count;
			float num2 = (float)(-(float)EV.pw / 2);
			float num3 = (float)(EV.pw / 2);
			float num4 = num3;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			bool flag = false;
			for (int i = 0; i < 2; i++)
			{
				int num10 = ((i == 0) ? count : num);
				int j = 0;
				while (j < num10)
				{
					NelMSG nelMSG = null;
					TalkDrawer talkDrawer = null;
					if (i == 0)
					{
						nelMSG = this.AMsg[j];
						if (nelMSG.isActive())
						{
							if (nelMSG.nest_shuffle > 0)
							{
								flag = true;
							}
							else
							{
								if (nelMSG.DrawerPersonFollowing == null)
								{
									nelMSG.person_index = -1;
									goto IL_00EF;
								}
								if (nelMSG.nest_shuffle <= 0)
								{
									num9++;
								}
							}
						}
					}
					else
					{
						talkDrawer = talkerDrawerList[j];
						if (talkDrawer != null && talkDrawer.isActive())
						{
							goto IL_00EF;
						}
					}
					IL_01A3:
					j++;
					continue;
					IL_00EF:
					float num11;
					if (talkDrawer != null)
					{
						num8++;
						num11 = talkDrawer.dx_real;
					}
					else
					{
						num11 = (float)((int)nelMSG.dx_real);
						int num12 = (int)nelMSG.dy_real;
						if (X.Abs(num12) > 20)
						{
							if (num12 > 20)
							{
								num5 |= 2;
							}
							else
							{
								num5 |= 8;
							}
						}
					}
					if (num11 == 0f)
					{
						goto IL_01A3;
					}
					if (num11 < 0f)
					{
						num5 |= 1;
						num6++;
						num2 = X.Mx(num11, num2);
						if (num6 != 1)
						{
							X.Mn(num11, num2);
							goto IL_01A3;
						}
						goto IL_01A3;
					}
					else
					{
						if (num11 > 0f)
						{
							num5 |= 4;
							num7++;
							num3 = X.Mn(num11, num3);
							num4 = ((num7 == 1) ? num3 : X.Mx(num11, num4));
							goto IL_01A3;
						}
						goto IL_01A3;
					}
				}
			}
			if (num9 != 0)
			{
				float num13;
				float num14;
				switch (num5 & 5)
				{
				case 1:
					num13 = 300f;
					num14 = 0f;
					goto IL_022A;
				case 4:
					num13 = 0f;
					num14 = 300f;
					goto IL_022A;
				case 5:
					num13 = 240f;
					num14 = 240f;
					goto IL_022A;
				}
				num13 = 40f;
				num14 = 40f;
				IL_022A:
				num13 = X.Mx((float)(-(float)EV.pw) * 0.5f + num13, num2 + 80f);
				num14 = X.Mn((float)EV.pw * 0.5f - num14, num3 - 80f);
				float num15 = (float)EV.ph * 0.4f;
				float num16 = (float)(-(float)EV.ph) * 0.3f;
				int num17 = num5 & 10;
				if (num17 != 2)
				{
					if (num17 != 8)
					{
						if (num17 == 10)
						{
							num15 -= 160f;
							num16 += 160f;
						}
					}
					else
					{
						num16 -= 220f;
					}
				}
				else
				{
					num15 -= 220f;
				}
				int num18 = 0;
				for (int k = 0; k < count; k++)
				{
					NelMSG nelMSG2 = this.AMsg[k];
					if (nelMSG2.isActive() && nelMSG2.person_index >= 0 && nelMSG2.nest_shuffle <= 0)
					{
						TalkDrawer drawerPersonFollowing = nelMSG2.DrawerPersonFollowing;
						if (drawerPersonFollowing != null)
						{
							float dx_real = drawerPersonFollowing.dx_real;
							bool flag2 = false;
							float num19;
							float num20;
							if (num9 == 1 || (dx_real == 0f && num6 == num7))
							{
								num19 = X.NI(num15, num16, 0.5f);
								if (dx_real == 0f && num6 == num7)
								{
									num20 = (float)EV.pw * 0.4f;
								}
								else
								{
									num20 = X.NI(num13, num14, 0.5f);
								}
							}
							else
							{
								int num21;
								if (num9 <= 2)
								{
									num20 = X.NI(num13, num14, 0.5f);
									if (num8 >= 2)
									{
										bool flag3 = (num2 + num3) * 0.5f < dx_real;
										num20 += (float)(X.MPF(flag3) * 14);
									}
									num21 = nelMSG2.person_index;
								}
								else
								{
									bool flag4;
									if (dx_real == 0f)
									{
										flag4 = num6 > num7;
									}
									else if (num6 == 0 || num7 == 0)
									{
										flag4 = (num2 + num3) * 0.5f < dx_real;
									}
									else
									{
										flag4 = dx_real > 0f;
									}
									num20 = ((!flag4) ? (num13 + nelMSG2.rect_w * 0.5f) : (num14 - nelMSG2.rect_h * 0.5f));
									if (flag4)
									{
										if ((num18 & 3) == 3)
										{
											num18 &= -4;
										}
										if ((num18 & 3) == 0)
										{
											num21 = ((drawerPersonFollowing.dx_real > num3 + 20f && nelMSG2.dy_real > X.NI(num15, num16, 0.5f) - 40f) ? 0 : 1);
										}
										else
										{
											num21 = (((num18 & 3) == 1) ? 0 : 1);
										}
										num18 |= ((num21 == 0) ? 2 : 1);
									}
									else
									{
										if ((num18 & 12) == 12)
										{
											num18 &= -13;
										}
										if ((num18 & 12) == 0)
										{
											num21 = ((drawerPersonFollowing.dx_real < num2 + 20f && nelMSG2.dy_real > X.NI(num15, num16, 0.5f) - 40f) ? 0 : 1);
										}
										else
										{
											num21 = (((num18 & 12) == 4) ? 0 : 1);
										}
										num18 |= ((num21 == 0) ? 8 : 4);
									}
								}
								num19 = X.NI(num15, num16, (0.5f + (float)num21) / 2f);
								switch (nelMSG2.person_index % 6)
								{
								case 2:
									num20 += 10f;
									num19 += 15f;
									break;
								case 3:
									num20 -= 8f;
									num19 += 15f;
									break;
								case 4:
									num20 -= 5f;
									num19 -= 11f;
									break;
								case 5:
									num20 += 8f;
									num19 -= 11f;
									break;
								}
							}
							nelMSG2.initPosition(num20, num19, flag2);
						}
					}
				}
			}
			if (flag)
			{
				for (int l = 0; l < count; l++)
				{
					NelMSG nelMSG3 = this.AMsg[l];
					if (nelMSG3.nest_shuffle == 0 && nelMSG3.isActive())
					{
						float dx_real2 = nelMSG3.dx_real;
						float dy_real = nelMSG3.dy_real;
						float num22 = (X.frac(dx_real2 * 113.7f + dy_real * 41.67f) + (float)l * 3.1415927f * 0.17f) * 6.2831855f;
						for (int m = l + 1; m < count; m++)
						{
							NelMSG nelMSG4 = this.AMsg[m];
							if (nelMSG4.isSamePerson(nelMSG3) && nelMSG4.isActive())
							{
								nelMSG4.initPosition(dx_real2 + 74f * X.Cos(num22), dy_real + 65f * X.Sin(num22), false);
								num22 += 2.2399557f;
							}
						}
					}
				}
			}
		}

		public bool progressNestShuffle(NelMSG Src, string hkds_id, EvPerson P)
		{
			NelMSG byDrawer = this.getByDrawer(hkds_id, true, NelMSG.HKDSTYPE._MAX, false, (int)Src.nest_shuffle);
			if (byDrawer != null)
			{
				byDrawer.container_TS = Src.container_TS;
				byDrawer.initPerson(hkds_id, P);
				this.fineFrontConfirmer();
				this.need_reposit_flag = true;
				return true;
			}
			Src.quitNestShuffle(Src == this.FrontMsg);
			return false;
		}

		public bool progressgNestShuffleFinal(NelMSG Src, string hkds_id)
		{
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (nelMSG != Src && nelMSG.isSamePerson(Src))
				{
					if (nelMSG.nest_shuffle == 0)
					{
						Src.initPosition(nelMSG.dx_real, nelMSG.dy_real, false);
					}
					nelMSG.hideMsg(false);
				}
			}
			this.need_reposit_flag = true;
			Src.initNestShuffle(-1, -1f);
			return false;
		}

		private void repositZ()
		{
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				NelMSG nelMSG = this.AMsg[i];
				float num = -1.8000002f;
				if (nelMSG.isActive() && nelMSG == this.FrontMsg)
				{
					num += -0.05f;
				}
				else
				{
					num += 0.4f;
				}
				num -= (float)i * 0.003f;
				if (nelMSG.transform.localPosition.z != num)
				{
					IN.setZ(nelMSG.transform, num);
					if (this.Confirmer.targetIs(nelMSG))
					{
						this.Confirmer.need_fine_position = true;
					}
				}
			}
			if (this.use_valotile)
			{
				CameraBidingsBehaviour.UiBind.need_sort_binds = true;
			}
			this.fine_z = false;
		}

		public bool run(float fcnt, bool skipping)
		{
			if (this.msgf < 0f)
			{
				return false;
			}
			this.TS = fcnt;
			bool flag = false;
			int count = this.AMsg.Count;
			if (this.t_temphd >= 0f)
			{
				flag = true;
				this.t_temphd += fcnt;
				if (this.TxHidingKC.gameObject.activeSelf)
				{
					this.TxHidingKC.alpha = (1f - X.ZLINE(this.t_temphd - 150f, 50f)) * X.ZLINE(this.t_temphd, 25f);
				}
			}
			else
			{
				this.msgf += this.TS;
				for (int i = 0; i < count; i++)
				{
					NelMSG nelMSG = this.AMsg[i];
					if (nelMSG.run(this.TS, skipping))
					{
						flag = true;
					}
					else if (this.AMsgActive.Count > 0)
					{
						this.AMsgActive.Remove(nelMSG);
					}
				}
			}
			if (!flag)
			{
				this.msgf = -1f;
			}
			else
			{
				if (this.need_person_reindex_flag)
				{
					this.need_person_reindex_flag = false;
					this.AMsgBuf.Clear();
					this.AMsg.Sort((NelMSG a, NelMSG b) => NelMSGContainer.fnSortPersonInitialize(a, b));
					if (this.AMsgBuf.Capacity < count)
					{
						this.AMsgBuf.Capacity = count;
					}
					bool flag2 = false;
					for (int j = 0; j < count; j++)
					{
						NelMSG nelMSG2 = this.AMsg[j];
						if (nelMSG2.isActive())
						{
							if (nelMSG2.nest_shuffle > 0)
							{
								flag2 = true;
							}
							else if (nelMSG2.DrawerPersonFollowing != null)
							{
								this.AMsgBuf.Add(nelMSG2);
								if (nelMSG2.person_index < 0)
								{
									nelMSG2.person_index = 200 + j;
								}
							}
							else
							{
								nelMSG2.person_index = -1;
							}
						}
					}
					this.AMsgBuf.Sort((NelMSG a, NelMSG b) => NelMSGContainer.fnSortPersonIndex(a, b));
					for (int k = this.AMsgBuf.Count - 1; k >= 0; k--)
					{
						this.AMsgBuf[k].person_index = k;
					}
					if (flag2)
					{
						for (int l = 0; l < count; l++)
						{
							NelMSG nelMSG3 = this.AMsg[l];
							if (nelMSG3.isActive() && nelMSG3.nest_shuffle > 0)
							{
								for (int m = 0; m < count; m++)
								{
									NelMSG nelMSG4 = this.AMsg[m];
									if (m != l && nelMSG4.isActive() && nelMSG4.nest_shuffle <= 0 && nelMSG4.isSamePerson(nelMSG3))
									{
										nelMSG3.person_index = nelMSG4.person_index;
										break;
									}
								}
							}
						}
					}
					this.AMsgBuf.Clear();
					this.need_reposit_flag_ = true;
				}
				if (this.need_reposit_flag_)
				{
					this.repositAll();
				}
			}
			if (this.fine_z)
			{
				this.repositZ();
			}
			return flag;
		}

		private static int fnSortPersonInitialize(NelMSG Ma, NelMSG Mb)
		{
			bool flag = Ma.isActive();
			bool flag2 = Mb.isActive();
			if (flag != flag2)
			{
				if (!flag)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				int num = (int)(Ma.nest_shuffle - Mb.nest_shuffle);
				if (num != 0)
				{
					return num;
				}
				float shown_level = Ma.shown_level;
				float shown_level2 = Mb.shown_level;
				if (shown_level == shown_level2)
				{
					return NelMSGContainer.fnSortPersonIndex(Ma, Mb);
				}
				if (shown_level >= shown_level2)
				{
					return -1;
				}
				return 1;
			}
		}

		private static int fnSortPersonIndex(NelMSG Ma, NelMSG Mb)
		{
			if (Ma.person_index == -1 == (Mb.person_index == -1))
			{
				return Ma.person_index - Mb.person_index;
			}
			if (Ma.person_index != -1)
			{
				return -1;
			}
			return 1;
		}

		public void hideMsg(bool immediate = false)
		{
			this.hideMsg(immediate, null);
		}

		public void hideMsg(bool immediate, NelMSG Without)
		{
			int count = this.AMsg.Count;
			for (int i = 0; i < count; i++)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (!(nelMSG == Without) && nelMSG.isActive())
				{
					nelMSG.hideMsg(immediate);
				}
			}
		}

		public void hideMsg(StringHolder rER_without, bool immediate = false)
		{
			int count = this.AMsg.Count;
			for (int i = 0; i < count; i++)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (nelMSG.isActive())
				{
					string text;
					nelMSG.getCurrentMsgKey(out text);
					if (rER_without.IndexOf(text) < 1)
					{
						nelMSG.hideMsg(immediate);
					}
				}
			}
		}

		public void hold()
		{
		}

		public void setHandle(bool f)
		{
			this.handle = f;
		}

		public void executeRestMessage(NelMSG Obj)
		{
			this.executeRestMsgCmd(Obj, 0);
			if (Obj.nest_shuffle >= 0)
			{
				for (int i = this.AMsg.Count - 1; i >= 0; i--)
				{
					NelMSG nelMSG = this.AMsg[i];
					if (nelMSG.nest_shuffle > 0 && nelMSG.isSamePerson(Obj))
					{
						nelMSG.hideMsg(false);
					}
				}
				this.need_reposit_flag = true;
			}
		}

		public void executeRestMsgCmd(NelMSG Obj, int count)
		{
			if (Obj.isFront() && this.msgcmd_run_flag == 1)
			{
				if (count > 0)
				{
					EV.MsgCmd.executeProgress(count);
					return;
				}
				if (count < 0 && Obj.nest_shuffle >= 0)
				{
					for (int i = this.AMsg.Count - 1; i >= 0; i--)
					{
						NelMSG nelMSG = this.AMsg[i];
						if (nelMSG != Obj && nelMSG.nest_shuffle > Obj.nest_shuffle && nelMSG.isSamePerson(Obj))
						{
							return;
						}
					}
				}
				this.msgcmd_run_flag = 2;
				EV.MsgCmd.executeAll();
			}
		}

		public int showImmediate(bool show_only_first_char = false, bool no_snd = false)
		{
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (nelMSG.isActive())
				{
					nelMSG.showImmediate(show_only_first_char, !no_snd && nelMSG.isFront());
				}
			}
			return 0;
		}

		public void clearValues()
		{
		}

		public void msgHided(NelMSG Msg)
		{
			if (Msg == this.FrontMsg)
			{
				this.FrontMsg = null;
			}
		}

		public bool progressNextParagraph()
		{
			if (this.AMsg.Count == 0)
			{
				return false;
			}
			this.msgf = 0f;
			NelMSG front = this.getFront();
			return front != null && front.progressNextParagraph();
		}

		public bool is_last_message
		{
			get
			{
				return this.last_label == this.last_message_key && EV.getCurrentEvent() == this.LastMessageER;
			}
		}

		public NelMSG getFront()
		{
			return this.FrontMsg;
		}

		public bool isAllCharsShown()
		{
			NelMSG front = this.getFront();
			return !(front != null) || front.isAllCharsShown();
		}

		public NelMSG GetById(string s)
		{
			int count = this.AMsg.Count;
			for (int i = 0; i < count; i++)
			{
				NelMSG nelMSG = this.AMsg[i];
				if (nelMSG.isSame(s) && nelMSG.isActive())
				{
					return nelMSG;
				}
			}
			return null;
		}

		public void AddActive(NelMSG Msg)
		{
			if (this.AMsgActive.IndexOf(Msg) == -1)
			{
				this.AMsgActive.Add(Msg);
			}
		}

		public void RemActive(NelMSG Msg)
		{
			int num = this.AMsgActive.IndexOf(Msg);
			if (num >= 0)
			{
				this.AMsgActive.RemoveAt(num);
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				int count = this.AMsgActive.Count;
				for (int i = 0; i < count; i++)
				{
					this.AMsgActive[i].use_valotile = value;
				}
				this.use_valotile_ = value;
				if (this.TxHidingKC != null)
				{
					this.TxHidingKC.use_valotile = value;
				}
			}
		}

		public bool need_reposit_flag
		{
			get
			{
				return this.need_reposit_flag_;
			}
			set
			{
				this.need_reposit_flag_ = value || this.need_reposit_flag_;
			}
		}

		public int getLoadLineCount(EvReader ER, CsvReader rER, List<string> Adest = null, bool no_error = false)
		{
			if (rER.clength >= 2)
			{
				string index = rER.getIndex(1);
				if (index.IndexOf("<<<") >= 0)
				{
					if (REG.match(index, NelMSGContainer.RegBookKey) && REG.R1 != null)
					{
						NelMSGContainer.checkHereDocument(REG.R1, ER.name, null, rER, true, Adest, true);
					}
					else if (REG.match(index, NelMSGContainer.RegPersonKey) && REG.R2 != null)
					{
						NelMSGContainer.checkHereDocument(REG.R2, ER.name, null, rER, true, Adest, true);
					}
				}
				else if (Adest != null)
				{
					string text = ER.name;
					string text2 = rER._1;
					if (REG.match(text2, NelMSGContainer.RegEventNameReplaceKey))
					{
						text = REG.R1;
						text2 = REG.R2;
					}
					NelMSGResource.getContent(text + " " + text2, Adest, false, no_error, false);
				}
			}
			return 0;
		}

		public int getCurrentMsgKey(out string person_key, out string label)
		{
			person_key = "";
			label = this.last_resource_label;
			if (this.FrontMsg == null)
			{
				return -1;
			}
			return this.FrontMsg.getCurrentMsgKey(out person_key);
		}

		public string main_key_desc
		{
			get
			{
				return TX.Get("EV_desc_msg", "");
			}
		}

		public string getPrefixForTalker(string talker)
		{
			string text;
			if (this.Omsg_prefix.TryGetValue(talker, out text))
			{
				return text;
			}
			return null;
		}

		public float getAppearTime()
		{
			return this.msgf;
		}

		public bool isActive()
		{
			for (int i = this.AMsg.Count - 1; i >= 0; i--)
			{
				if (this.AMsg[i].isActive())
				{
					return true;
				}
			}
			return false;
		}

		public ConfirmAnnouncer Confirmer
		{
			get
			{
				return EV.Confirmer;
			}
		}

		private const char MSGFLAG_IMMEDIATE = 'I';

		private const char MSGFLAG_THINK = 'T';

		private const char MSGFLAG_ANGRY = 'A';

		private const char MSGFLAG_CONTINUE = 'C';

		private const char MSGFLAG_BEHIND = 'B';

		private const char MSGFLAG_BOOK = 'K';

		private const char MSGFLAG_DEVICE = 'D';

		private const char MSGFLAG_EVIL = 'V';

		private const char MSGFLAG_AUTO_PROGRESS = 'P';

		private const char MSGFLAG_FOCUS_COLOR = 'F';

		private const char MSGFLAG_MERGE = 'M';

		private const char MSGFLAG_REPLACE_LABEL = 'R';

		private const char MSGFLAG_TX_INJECTION = 'X';

		private const char MSGFLAG_SHUFFLE = 'S';

		private List<NelMSG> AMsg;

		private List<NelMSG> AMsgBuf;

		private List<NelMSG> AMsgActive;

		private BDic<string, NelMSGContainer.HkdsInfo> OHkdsInfo;

		private BDic<string, string> Omsg_prefix;

		private static Regex RegMsgFlagAutoProgress = new Regex("P\\[([\\-\\d\\.]+)\\]");

		private static Regex RegMsgFlagBookPos = new Regex("K\\[([\\w]+)\\]");

		private static Regex RegMsgFlagLabelReplace = new Regex("R\\[([\\w]+)\\]");

		private static Regex RegMsgTxInjection = new Regex("X\\[([\\&\\w]+)\\]");

		private static Regex RegPersonKey = new Regex("^([a-zA-Z0-9]+)_(<<<\\w+)?");

		private static Regex RegBookKey = new Regex("^BOOK_(?:_*(<<<\\w+))?");

		private static Regex RegPosKey = new Regex("@([A-Z]+)");

		public static Regex RegEventNameReplaceKey = new Regex("^([^\\*]+)\\*([^\\*]+)$");

		private bool fine_z;

		private float msgf = -1f;

		private NelMSG FrontMsg;

		private byte msg_index;

		private TextRenderer TxHidingKC;

		private bool need_reposit_flag_;

		public bool need_person_reindex_flag;

		private byte msgcmd_run_flag;

		private byte confirmer_show_bits;

		private StringHolder LastMessageER;

		private string last_message_key;

		private string last_label;

		private string last_resource_label;

		public float TS = 1f;

		private bool auto_msg_hide;

		private float t_temphd = -1f;

		private const float TEMPHD_MAXT = 200f;

		private bool handle;

		public short nest_shuffle;

		private bool use_valotile_;

		public class HkdsInfo
		{
			public HkdsInfo(string _pos_fix_key = "=", string _follow_to_key = "=", string _bounds_key = "=", string _talker_replace_key = "=")
			{
				this.follow_to_key = _follow_to_key;
				this.pos_fix_key = _pos_fix_key;
				this.bounds_key = _bounds_key;
				this.talker_replace_key = _talker_replace_key;
			}

			public string follow_to_key;

			public string pos_fix_key;

			public string bounds_key;

			public string talker_replace_key;

			public string talker_snd_key = "=";

			public bool used;
		}
	}
}
