using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace evt
{
	public class EV : MonoBehaviour, ConfirmAnnouncer.IConfirmHolder
	{
		private static EV.ST state
		{
			get
			{
				return EV.state_;
			}
			set
			{
				if (EV.state == value)
				{
					return;
				}
				EV.state_ = value;
				EV.t_msgf = 0f;
			}
		}

		public static GameObject Gob
		{
			get
			{
				return EV.Instance.gameObject;
			}
		}

		public static void loadEV()
		{
			if (EV.Oevt_content != null)
			{
				return;
			}
			LoadTicketManager.PrepareLoadManager();
			EV.setWH((int)IN.w, (int)IN.h, (int)IN.w, (int)IN.h);
			EvPerson.loadPerson(NKT.readStreamingText("evt/__vp_person.dat", false), ref EV.APxls);
			TalkDrawer.loadTalkerPos(NKT.readStreamingText("evt/__vp_talker_pos.dat", false));
			EV.Oevt_content = new BDic<string, string>();
			if (EV.Pics == null)
			{
				EV.Pics = new EvImgContainer("");
			}
		}

		public static bool ev_prepared
		{
			get
			{
				return EV.Oevt_content != null;
			}
		}

		public static bool material_prepared
		{
			get
			{
				if (EV.AStack != null)
				{
					return true;
				}
				if (EV.Pics == null)
				{
					EV.loadEV();
				}
				return X.DEBUGNOEVENT || !EV.ticket_loading;
			}
		}

		public static void initEvent(IMessageContainer _Msg = null, GameObject attachedTo = null, ITutorialBox _Tuto = null)
		{
			EV.destructItems();
			GameObject gameObject;
			if (attachedTo != null)
			{
				gameObject = IN.CreateGob(attachedTo, "-EV");
			}
			else
			{
				gameObject = new GameObject("EV");
				gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			}
			IN.Pos(gameObject, 0f, 0f, -3f);
			EV.Instance = gameObject.AddComponent<EV>();
			EV.MMRD = gameObject.AddComponent<MultiMeshRenderer>();
			GameObject gameObject2 = IN.CreateGobGUI(gameObject.gameObject, "-MdLoad");
			EV.MdLoad = MeshDrawer.prepareMeshRenderer(gameObject2, MTRX.MtrMeshNormal, -2.5f, -1, null, true, true);
			EV.MrdLoad = gameObject2.GetComponent<MeshRenderer>();
			EV.ValotLoad = gameObject2.GetComponent<ValotileRenderer>();
			IN.setZAbs(gameObject2.transform, -9.25f);
			EV.MrdLoad.gameObject.SetActive(!EV.do_not_draw_loading_circle);
			if (EV.FnCreateConfirmer != null)
			{
				EV.Confirmer = EV.FnCreateConfirmer(gameObject);
			}
			else
			{
				EV.Confirmer = IN.CreateGob(gameObject, "-Confirmer").AddComponent<ConfirmAnnouncer>();
			}
			EV.AStack = new List<EvReader>(16);
			if (EV.ALsn == null)
			{
				EV.ALsn = new List<IEventListener>(4);
			}
			EV.OLsnWait = new BDic<string, IEventWaitListener>();
			EV.addWaitListener("PXL_LOAD", new EvWaitPxlChars());
			EV.state = EV.ST.NONE;
			EV.no_first_run = false;
			EV.curEv = (EV.nexEv = null);
			if (!GF.initted)
			{
				GF.init(0U, 0U);
			}
			EV.wait_frame = 0f;
			if (_Msg != null)
			{
				EV.msg_run = false;
				EV.MsgCon = _Msg;
			}
			else
			{
				EV.msg_run = true;
				EV.MsgDefault = new MSG(gameObject);
				EV.MsgCon = EV.MsgDefault;
				IN.setZ(EV.MsgDefault.transform, -1.8000002f);
			}
			EV.DC = new EvDrawerContainer(EV.MMRD);
			EV.TutoBox = _Tuto;
			EV.Sel = IN.CreateGob(gameObject, "-Sel").AddComponent<EvSelector>();
			EV.AHkdsFollowCon = new List<IHkdsFollowableContainer>(2);
			EV.AHkdsFollowCon.Add(EV.DC);
			EV.MsgCmd = new EvMsgCommand();
			EV.TlCmd = new EvTLCommand();
			EV.PoolERCache = new ClsPool<CsvReader>(() => new CsvReader(null, null, false), 2);
			EV.VarConCache = new CsvVariableContainer();
			EV.Pics.initPx(EV.APxls);
			EV.createListenerTxEval(gameObject);
			if (EV.TutoBox != null)
			{
				EV.TutoBox.setPositionDefault();
				EV.TutoBox.RemText(true, true);
			}
			EV.clearGameValues();
			bool debugnosnd = X.DEBUGNOSND;
			EV.first_event_stacked = false;
			EV.stackFirstEvent();
		}

		public static void newGame()
		{
			if (EV.VarCon != null)
			{
				EV.VarCon.removeVarAll();
			}
		}

		public static bool initDebugger(bool execute_debugger_initialize = true)
		{
			EV.debug = ((X.DEBUGANNOUNCE && X.DEBUGTIMESTAMP) ? EV.EVDEBUG.ALLOC_CONSOLE : ((EV.EVDEBUG)0));
			if (EV.debug != (EV.EVDEBUG)0)
			{
				if (EV.Dbg == null)
				{
					EV.Dbg = IN.CreateGob(EV.Gob, "-Debugger").AddComponent<EvDebugger>();
				}
				if (execute_debugger_initialize)
				{
					EV.Dbg.initEvDebugger();
				}
				return true;
			}
			if (EV.Dbg != null)
			{
				EV.Dbg.destruct();
				IN.DestroyOne(EV.Dbg.gameObject);
				EV.Dbg = null;
			}
			return false;
		}

		public static void destructItems()
		{
			if (EV.MsgDefault != null)
			{
				EV.MsgDefault = (EV.msg_run ? EV.MsgDefault.destruct() : null);
			}
			EV.AStack = null;
			EV.curEv = (EV.nexEv = null);
			if (EV.DC != null)
			{
				EV.DC = EV.DC.destruct();
			}
			if (EV.TutoBox != null)
			{
				EV.TutoBox.destructTutoBox();
				EV.TutoBox = null;
			}
			EV.need_fine_confirmer = false;
			EV.MsgCmd = null;
			EV.MsgCon = null;
			EV.no_reload = false;
			EV.cache_loading = -2;
			if (EV.MdLoad != null)
			{
				EV.MdLoad.destruct();
			}
			EV.MMRD = null;
			EV.MdLoad = null;
			EV.valotile_overwrite_msg_ = false;
			EV.MrdLoad = null;
			EV.ValotLoad = null;
			EV.AHkdsFollowCon = null;
			EV.PoolERCache = null;
			EV.VarConCache = null;
			EV.debug = (EV.EVDEBUG)0;
			if (EV.Dbg != null)
			{
				EV.Dbg.destruct();
			}
			EV.Dbg = null;
			if (EV.Instance != null)
			{
				TX.removeListenerEval(EV.Gob);
				IN.DestroyOne(EV.Gob);
				EV.Instance = null;
			}
		}

		public static void clearGameValues()
		{
			EV.MsgCmd.clear();
			EV.MsgCon.clearValues();
			EV.stop_game = (EV.stop_gdraw = false);
			EV.handle_game = true;
			EV.no_reload = false;
			EV.first_read_flag = true;
			EV.bg_full_filled_ = 2;
			EV.alloc_evhandle_key = KEY.SIMKEY._EVHANDLE;
			EV.active_load = false;
		}

		public static bool getEventContent(string _name, EvReader ER)
		{
			string text = X.Get<string, string>(EV.Oevt_content, _name);
			if (text == null)
			{
				if (_name.IndexOf("%") == 0)
				{
					return true;
				}
				string text2 = NKT.readStreamingText("evt/" + _name + ".cmd", false);
				if (TX.noe(text2))
				{
					return false;
				}
				text = (EV.Oevt_content[_name] = text2);
			}
			if (ER != null)
			{
				ER.parseText(text);
			}
			return true;
		}

		public static void setEventContent(string _name, string tex)
		{
			if (!TX.noe(tex))
			{
				EV.Oevt_content[_name] = tex;
			}
		}

		public static int pw
		{
			get
			{
				return EV.pw_;
			}
			set
			{
				EV.setWH(value, 0, 0, 0);
			}
		}

		public static int ph
		{
			get
			{
				return EV.ph_;
			}
			set
			{
				EV.setWH(0, value, 0, 0);
			}
		}

		public static int psw
		{
			get
			{
				return EV.psw_;
			}
		}

		public static int psh
		{
			get
			{
				return EV.psh_;
			}
		}

		public static void setWH(int _w, int _h, int _sw = 0, int _sh = 0)
		{
			if (_w > 0)
			{
				if (_sw <= 0)
				{
					_sw = _w;
				}
				EV.pw_ = _w;
			}
			if (_h > 0)
			{
				if (_sh <= 0)
				{
					_sh = _h;
				}
				EV.ph_ = _h;
			}
			if (_sw > 0)
			{
				EV.psw_ = _sw;
			}
			if (_sh > 0)
			{
				EV.psh_ = _sh;
			}
		}

		public static void stackFirstEvent()
		{
			EV.first_event_stacked = true;
			EV.stack("__INITG", 0, 0, null, null);
		}

		public static void initS()
		{
		}

		public void FixedUpdate()
		{
			this.runEvInner(1f, false, true);
			if ((EV.debug & EV.EVDEBUG.ALLOC_CONSOLE) != (EV.EVDEBUG)0 && IN.getKD(Key.F7, -1))
			{
				EV.Dbg.changeActivate(!EV.Dbg.isActive() && !EV.Dbg.isELActive());
			}
		}

		public void OnDestroy()
		{
			EV.destructItems();
			if (EV.Log != null)
			{
				EV.Log.destruct();
				EV.Log = null;
			}
		}

		private void runEvInner(float fcnt, bool no_process = false, bool draw_flg = true)
		{
			if (EV.debug != (EV.EVDEBUG)0 && EV.Dbg.isActive())
			{
				no_process = true;
			}
			draw_flg = draw_flg && X.D;
			EV.msg_active = EV.MsgCon.run(fcnt * (float)(((EV.skipping & 2) > 0) ? 5 : (((EV.skipping & 1) > 0) ? 2 : 1)), EV.skipping > 0);
			if (!EV.active_load && EV.drawLoading(fcnt, EV.MdLoad))
			{
				EV.DC.run(fcnt, false);
			}
			else if (no_process)
			{
				EV.DC.run(fcnt, false);
			}
			else if (EV.curEv != null)
			{
				EV.evRun(fcnt);
			}
			else
			{
				if (EV.nexEv != null)
				{
					EV.evStart();
				}
				EV.DC.run(fcnt, EV.nexEv == null);
			}
			if (EV.Log != null)
			{
				EV.Log.runMesh(fcnt);
			}
		}

		public static EvReader stack(string _name, int start_line = 0, int push_to = -1, string[] Avariables = null, EvReader cloneFrom = null)
		{
			return EV.stackReader(new EvReader((cloneFrom != null) ? cloneFrom.name : _name, start_line, Avariables, cloneFrom), push_to);
		}

		public static void stackA(string[] Aname)
		{
			if (Aname == null)
			{
				return;
			}
			int num = Aname.Length;
			for (int i = 0; i < num; i++)
			{
				EV.stack(Aname[i], 0, -1, null, null);
			}
		}

		public static EvReader stackReader(EvReader ER, int push_to = -1)
		{
			if (ER == null)
			{
				return null;
			}
			Bench.P("EV Stack");
			string name = ER.name;
			Bench.P(name);
			if (push_to == -1 || push_to >= EV.AStack.Count)
			{
				EV.AStack.Add(ER);
			}
			else
			{
				EV.AStack.Insert(push_to, ER);
			}
			if (EV.first_event_stacked)
			{
				if (EV.nexEv == null)
				{
					EV.nexEv = EV.AStack[0];
				}
				if (EV.nexEv != null && EV.curEv != null && EV.ev_stack_stop && EV.evEnd(false))
				{
					EV.state = EV.ST.GOING;
				}
				if (EV.curEv == null && EV.nexEv != null)
				{
					EV.evStart();
				}
			}
			Bench.Pend(name);
			Bench.Pend("EV Stack");
			return ER;
		}

		public static void unstackReader(string ev_name)
		{
			if (EV.curEv != null && EV.curEv.name == ev_name)
			{
				EV.unstackReader(EV.curEv);
				return;
			}
			EV.unstackReader(EV.getStacked(ev_name));
		}

		public static void unstackReader(EvReader ER)
		{
			if (ER == null)
			{
				return;
			}
			if (ER == EV.curEv)
			{
				EV.evEnd(false);
			}
			int num = EV.AStack.IndexOf(ER);
			if (num >= 0)
			{
				EV.AStack.RemoveAt(num);
				if (EV.curEv == null)
				{
					if (EV.AStack.Count == 0)
					{
						EV.evEnd(true);
						return;
					}
					EV.nexEv = EV.AStack[0];
				}
			}
		}

		public static void unstackReaderAllForcibly()
		{
			for (int i = EV.AStack.Count - 1; i >= 0; i--)
			{
				EV.unstackReader(EV.AStack[i]);
			}
			while (EV.curEv != null)
			{
				EV.unstackReader(EV.curEv);
			}
		}

		public static bool use_valotile
		{
			get
			{
				return EV.DC.use_valotile;
			}
			set
			{
				EV.DC.use_valotile = value;
				EV.Sel.use_valotile = value || EV.valotile_overwrite_msg;
				if (EV.TutoBox is IValotileSetable)
				{
					(EV.TutoBox as IValotileSetable).use_valotile = value;
				}
				if (EV.MsgCon is IValotileSetable)
				{
					(EV.MsgCon as IValotileSetable).use_valotile = value || EV.valotile_overwrite_msg_;
				}
				EV.Confirmer.fineValotile(value);
			}
		}

		public static bool valotile_overwrite_msg
		{
			get
			{
				return EV.valotile_overwrite_msg_;
			}
			set
			{
				if (EV.valotile_overwrite_msg_ != value)
				{
					EV.valotile_overwrite_msg_ = value;
					EV.Sel.use_valotile = value || EV.valotile_overwrite_msg;
					if (EV.MsgCon is IValotileSetable)
					{
						(EV.MsgCon as IValotileSetable).use_valotile = EV.use_valotile || EV.valotile_overwrite_msg_;
					}
				}
				EV.Confirmer.fineValotile(value);
			}
		}

		private static void evInit()
		{
			EV.TlCmd.executeAll();
			EV.getVariableContainer().removeTemp();
			EV.state = EV.ST.GOING;
			EV.wait_frame = 0f;
			EV.stop_gdraw = (EV.stop_game = false);
			EV.handle_game = false;
			EV.deny_skip = false;
			EV.Confirmer.default_content = "<key cancel/>/<key submit/>";
			EV.run_drawer = true;
			EV.cache_loading = -2;
			EV.no_reload = true;
			IN.FlgUiUse.Add("EVENT");
			EV.first_read_flag = true;
			EV.msg_skip = false;
			EvPerson.evInit();
			EV.DC.initEvent();
			EV.Sel.evInit();
			if (EV.TutoBox != null)
			{
				EV.TutoBox.addActiveFlag("EVENT");
			}
			if (EV.Log != null)
			{
				EV.Log.evInit();
			}
			EV.MsgCon.initEvent(EV.MsgCmd);
			EV.OCharAlias = null;
			EV.alloc_evhandle_key = KEY.SIMKEY._EVHANDLE;
			EV.msg_hide = false;
			EV.waitFn = null;
			CURS.Active.Add("EVENT");
			CURS.Active.Rem("EVENT");
		}

		public static void evStart()
		{
			EV.curEv = EV.nexEv;
			EV.nexEv = null;
			bool flag = EV.state == EV.ST.NONE;
			if (EV.curEv == null || !EV.curEv.open(flag) || !EV.listenerOpen(flag))
			{
				EV.evEnd(false);
				return;
			}
			EV.active_load = false;
			EV.AStack.RemoveAt(0);
			if (EV.state == EV.ST.NONE)
			{
				EV.evInit();
			}
			EV.ev_stack_stop = false;
			if (!EV.curEv.isVariable())
			{
				EV.evEnd(false);
				return;
			}
			EV.curEv.VarCon.defineArrayNumeric(EV.curEv.Avariables);
			if (!EV.curEv.no_init_load)
			{
				EV.cache_loading = X.Mx(EV.curEv.get_cur_line(), 0);
			}
			if (EV.no_first_run)
			{
				EV.no_first_run = false;
				return;
			}
			if (!EV.curEv.do_not_announce)
			{
				X.dl("EV::start - " + EV.curEv.name + " line:" + (EV.curEv.get_cur_line() + 1).ToString(), null, false, false);
			}
			if (flag)
			{
				EV.evRun(1f);
			}
		}

		public static bool evEnd(bool _all = false)
		{
			if (EV.curEv != null)
			{
				bool flag = EV.AStack.Count == 0 || EV.AStack[0] == null || _all;
				if (!EV.curEv.close(flag) || !EV.listenerClose(flag))
				{
					return false;
				}
				if (!EV.curEv.isVariable())
				{
					X.de("イベント " + EV.curEv.name + " がロードされていない状態で閉じられました。 LOADA の位置を見なおして下さい。", null);
				}
				else if (!EV.curEv.do_not_announce)
				{
					X.dl("EV::end - " + EV.curEv.name, null, false, false);
				}
				if (!X.DEBUGNOSND)
				{
					BGM.closeEvBgm(flag);
				}
			}
			EV.MsgCmd.clear();
			EV.curEv = null;
			EV.cache_loading = -2;
			if (EV.AStack.Count > 0 && EV.AStack[0] != null && !_all)
			{
				EV.nexEv = EV.AStack[0];
			}
			else
			{
				EV.nexEv = null;
				EV.AStack.RemoveRange(0, EV.AStack.Count);
				EV.msg_skip = false;
				EV.msg_hide = false;
				EV.need_fine_confirmer = false;
				EV.Pics.clearTermCache();
				EV.DC.deactivateEvent();
				EV.Sel.evEnd();
				EV.skipping = 0;
				EV.deny_skip = false;
				EV.run_drawer = true;
				EV.state = EV.ST.NONE;
				EV.bg_full_filled_ = 0;
				EV.alloc_evhandle_key = KEY.SIMKEY._EVHANDLE;
				IN.FlgUiUse.Rem("EVENT");
				if (EV.TutoBox != null)
				{
					EV.TutoBox.remActiveFlag("EVENT");
					EV.TutoBox.local_z = EV.TutoBox.local_z;
				}
				if (EV.Log != null)
				{
					EV.Log.evQuit();
				}
				EV.MsgCon.quitEvent();
				EV.clearGameValues();
				EV.Confirmer.deactivate();
				IN.clearPushDown(true);
				bool debugreloadmtr = X.DEBUGRELOADMTR;
				EV.clearEventContent(null);
				if (EV.debug != (EV.EVDEBUG)0)
				{
					EV.Dbg.evLineRelease();
				}
				EV.waitFn = null;
				EV.no_first_run = false;
			}
			return true;
		}

		private static bool cacheMaterial(EvReader ER = null, bool first = true)
		{
			ER = ((ER == null) ? EV.curEv : ER);
			if (ER == null)
			{
				return false;
			}
			if (first && (EV.debug & EV.EVDEBUG.ALLOC_CONSOLE) != (EV.EVDEBUG)0)
			{
				EV.Dbg.evLineCache(ER, true);
			}
			ER.save();
			int num = EV.cache_loading;
			if (first)
			{
				if (EV.cache_loading >= 0)
				{
					ER.seek_set(EV.cache_loading);
				}
				EV.cache_loading = -2;
				EV.VarConCache.removeVarAll().CopyFrom(ER.VarCon);
				EV.VarConCache.no_undef_error = true;
			}
			bool flag = true;
			CsvVariableContainer csvVariableContainer = EV.VarConCache;
			EV.VarConCache.defineArrayNumeric(ER.Avariables);
			EvReader evReader = ER;
			CsvVariableContainer csvVariableContainer2 = csvVariableContainer;
			CsvVariableContainer varCon = ER.VarCon;
			evReader.VarCon = csvVariableContainer2;
			csvVariableContainer = varCon;
			CsvReader csvReader = EV.PoolERCache.Pool().CopyDataFrom(ER);
			csvReader.skip_debug_cmd = false;
			bool flag2 = !first;
			while (csvReader.read())
			{
				if (!(csvReader.getLastStr() == "") && !TX.noe(csvReader.cmd))
				{
					bool flag3 = flag2;
					flag2 = false;
					while (csvReader.cmd == "MTL" || csvReader.cmd == "TL")
					{
						csvReader.shiftEmpty((csvReader.cmd == "MTL") ? 1 : 2, 0);
					}
					string cmd = csvReader.cmd;
					if (cmd == "DEBUG")
					{
						csvReader.tNote("cachMaterial DEBUG:" + csvReader.slice_join(1, " ", ""), false);
					}
					else if (cmd == "MSG")
					{
						int num2 = EV.MsgCon.getLoadLineCount(ER, csvReader, null, false);
						while (!csvReader.isEnd())
						{
							if (--num2 < 0)
							{
								break;
							}
							csvReader.readCorrectly();
						}
					}
					else if (cmd.IndexOf("//") != 0 && !TX.noe(cmd))
					{
						cmd == "<CLEAR_CACHE>";
						if (cmd == "<NOLOAD>")
						{
							break;
						}
						if (cmd == "<LOAD>" || cmd == "<LOADA>")
						{
							if (!flag3)
							{
								break;
							}
						}
						else
						{
							bool flag4 = TX.charIs(cmd, 0, '@');
							if (cmd == "MODULE" || flag4 || cmd == "<MODULE_LOAD>")
							{
								EvReader evReader2 = new EvReader(flag4 ? TX.slice(cmd, 1) : csvReader._1, 0, csvReader.slice(flag4 ? 1 : 2, -1000), null);
								if (evReader2.isVariable())
								{
									flag = EV.cacheMaterial(evReader2, false) && flag;
								}
							}
							else if (!EV.listenerCacheRead(ER, cmd, csvReader))
							{
								BGM.EvtCacheRead(ER, cmd, csvReader);
							}
						}
					}
				}
			}
			ER.restore();
			EV.PoolERCache.Release(csvReader);
			if (ER.VarCon != null)
			{
				evReader = ER;
				csvVariableContainer2 = csvVariableContainer;
				CsvVariableContainer varCon2 = ER.VarCon;
				evReader.VarCon = csvVariableContainer2;
			}
			if (first && EV.cache_loading == -1)
			{
				EV.cache_loading = ((num >= 0) ? num : (ER.get_cur_line() + 1));
			}
			if (EV.isLoading())
			{
				flag = false;
				if (first && !EV.active_load)
				{
					EV.state = EV.ST.LOADING;
				}
			}
			return flag;
		}

		private static void evRun(float fcnt0)
		{
			float num = (float)(((EV.skipping & 2) > 0) ? 100 : (((EV.skipping & 1) > 0) ? 4 : 1));
			EV.TlCmd.run(num * fcnt0);
			int num2 = 1;
			int num3 = 1;
			bool flag = EV.skipping != 0;
			while (--num3 >= 0 && EV.state != EV.ST.NONE)
			{
				if (EV.cache_loading > -2)
				{
					if (--num2 < 0)
					{
						break;
					}
					if ((!EV.cacheMaterial(EV.curEv, true) || EV.cache_loading >= 0) && !EV.active_load)
					{
						return;
					}
					if (EV.cache_loading != -2)
					{
						break;
					}
				}
				if (EV.first_read_flag)
				{
					EV.first_read_flag = false;
				}
				if (EV.state == EV.ST.GOING && EV.curEv.hasPreservedState())
				{
					EV.state = EV.curEv.popPreservedState();
				}
				while (EV.state == EV.ST.GOING)
				{
					if (EV.debug != (EV.EVDEBUG)0 && !EV.Dbg.evLineCanProgress(EV.curEv))
					{
						num3 = 0;
						break;
					}
					if (!EV.curEv.read())
					{
						EV.state = EV.ST.CHANGE;
						break;
					}
					EV.ST st = EV.readOneLine(EV.curEv, null, ref num);
					EV.state = ((st == EV.ST.NONE) ? EV.state : st);
					if (EV.isConfirmerState())
					{
						EV.need_fine_confirmer = true;
					}
				}
				if (EV.state == EV.ST.NONE || EV.state == EV.ST.TEMPSTOP)
				{
					break;
				}
				if (EV.state == EV.ST.LOADING)
				{
					if (EV.isLoading())
					{
						EV.active_load = false;
						return;
					}
					EV.state = EV.ST.GOING;
					num3++;
				}
				if (EV.state == EV.ST.MESSAGE)
				{
					if (EV.t_msgf < 0f)
					{
						if (!EV.INisKetteiOn() || EV.skipping > 0)
						{
							EV.t_msgf += num;
						}
					}
					else
					{
						EV.t_msgf += num;
					}
					EV.progressMsg(ref num3);
				}
				else if (EV.state == EV.ST.MSGSKIP)
				{
					IN.clearSubmitPushDown(false);
					EV.state = EV.ST.GOING;
					EV.need_fine_confirmer = true;
					num3++;
				}
				else if (EV.state == EV.ST.SELECT)
				{
					bool flag2 = true;
					if (EV.Log != null)
					{
						bool allow_msglog = EV.Log.allow_msglog;
						if (EV.Log.runInEvent(EV.MsgCon, EV.MsgCmd))
						{
							flag2 = false;
						}
					}
					if (EV.Sel.can_handle != flag2)
					{
						EV.Sel.set_handle(flag2);
					}
					if (EV.Sel.result != "")
					{
						EV.state = EV.ST.GOING;
						if (EV.Sel.define_to != "")
						{
							EV.getVariableContainer().define(EV.Sel.define_to, EV.Sel.result, true);
						}
						else
						{
							EV.curEv.jumpTo(EV.Sel.result);
						}
						IN.clearSubmitPushDown(false);
					}
				}
				else if (EV.state == EV.ST.CHANGE)
				{
					EV.evEnd(false);
					if (EV.nexEv != null && !EV.nexEv.isVariable())
					{
						EV.state = EV.ST.LOADING;
						break;
					}
					EV.evStart();
					if (EV.curEv == null)
					{
						EV.state = EV.ST.NONE;
					}
					else
					{
						if (EV.isLoading())
						{
							EV.state = EV.ST.LOADING;
							break;
						}
						EV.state = EV.ST.GOING;
						num3++;
						num2++;
					}
				}
				else if (EV.state == EV.ST.DEBUGSTOP)
				{
					EV.run_drawer = false;
					if (EV.INisSubmitUp())
					{
						EV.state = EV.ST.GOING;
						EV.run_drawer = true;
						break;
					}
					if (EV.INisCancel())
					{
						EV.state = EV.ST.NONE;
						EV.run_drawer = true;
						break;
					}
				}
				else if (EV.state == EV.ST.WAIT)
				{
					EV.wait_frame -= num * fcnt0;
					if (EV.wait_frame <= 0f)
					{
						EV.wait_frame = 0f;
						EV.state = EV.ST.GOING;
					}
					if (EV.skipping != 0 && EV.wait_frame > 1f)
					{
						EV.wait_frame = 1f;
					}
				}
				else if (EV.state == EV.ST.WAIT_BGM_LOAD)
				{
					EV.state = EV.ST.GOING;
				}
				else if (EV.state == EV.ST.WAIT_BTN)
				{
					EV.skipping &= -2;
					if ((EV.Log == null || !EV.Log.runInEvent(null, null)) && EV.canEventHandle())
					{
						bool flag3 = EV.INisKetteiM3();
						if (!flag3 && EV.wait_frame > 0f)
						{
							EV.wait_frame -= num * fcnt0;
							if (EV.wait_frame <= 0f)
							{
								flag3 = true;
							}
						}
						if (flag3)
						{
							SND.Ui.play("talk_progress", false);
							EV.wait_frame = 0f;
							EV.state = EV.ST.GOING;
							EV.need_fine_confirmer = true;
							IN.clearPushDown(true);
						}
					}
				}
				else if (EV.state == EV.ST.WAIT_TRANS)
				{
					X.de("WAIT_TRANS 未実装", null);
				}
				else if (EV.state == EV.ST.WAIT_FN)
				{
					bool flag4 = EV.waitFn == null || !EV.waitFn.EvtWait(false);
					if (!flag4 && EV.wait_frame > 0f)
					{
						EV.wait_frame -= num * fcnt0;
						if (EV.wait_frame <= 0f)
						{
							flag4 = true;
						}
					}
					if (flag4)
					{
						EV.wait_frame = 0f;
						EV.waitFn = null;
						EV.state = EV.ST.GOING;
						num3++;
					}
				}
				else if (EV.state == EV.ST.WAIT_MOVE)
				{
					bool flag5 = EV.listenerMoveCheck();
					if (!flag5 && EV.wait_frame > 0f)
					{
						EV.wait_frame -= num * fcnt0;
						if (EV.wait_frame <= 0f)
						{
							flag5 = true;
						}
					}
					if (flag5)
					{
						EV.wait_frame = 0f;
						EV.state = EV.ST.GOING;
					}
				}
			}
			if (EV.state != EV.ST.MESSAGE && EV.state != EV.ST.GOING && EV.msg_hide)
			{
				EV.MsgCon.hideMsg(false);
				EV.msg_hide = false;
			}
			if (EV.state != EV.ST.NONE)
			{
				if (!EV.deny_skip && EV.canEventHandle())
				{
					if (EV.INisMenuOn())
					{
						EV.skipping |= 2;
					}
					else
					{
						EV.skipping &= -3;
					}
					if (EV.INisCancelOn())
					{
						EV.skipping |= 1;
					}
					else
					{
						EV.skipping &= -2;
					}
				}
				else
				{
					EV.skipping = 0;
				}
				if (EV.skipping != 0 && !flag)
				{
					EV.DC.fine();
				}
			}
			if (EV.state == EV.ST.WAIT_1)
			{
				EV.state = EV.ST.GOING;
			}
			if (EV.state == EV.ST.NONE)
			{
				EV.evEnd(true);
				return;
			}
			if (EV.need_fine_confirmer)
			{
				EV.need_fine_confirmer = false;
				EV.fineConfirmerFront();
			}
			if (EV.run_drawer)
			{
				EV.DC.run(num * fcnt0, EV.curEv == null);
			}
			if (EV.debug != (EV.EVDEBUG)0)
			{
				EV.Dbg.evLineCanProgress(EV.curEv);
			}
		}

		private static bool progressMsg(ref int repeatcnt)
		{
			bool flag = !EV.MsgCon.isActive() || EV.msg_skip;
			bool flag2 = EV.canEventHandle();
			bool flag3 = true;
			if (EV.Log != null)
			{
				flag3 = EV.Log.allow_msglog;
				if (EV.Log.runInEvent(EV.MsgCon, EV.MsgCmd))
				{
					flag2 = false;
				}
			}
			if (EV.MsgCon.isHidingTemporary())
			{
				flag = false;
			}
			if (EV.MsgCon.checkHideVisibilityTemporary(flag3 && flag2, false))
			{
				SND.Ui.play(EV.MsgCon.isHidingTemporary() ? "tool_drag_init" : "tool_drag_quit", false);
				flag = (flag2 = false);
			}
			if (!flag)
			{
				bool flag4 = EV.MsgCon.isAllCharsShown();
				if (!flag4 && flag2 && ((EV.t_msgf > 1f && (EV.skipping != 0 || EV.INisKetteiPD(1) || (EV.deny_skip && EV.INisCancelPD()))) || (EV.t_msgf > 22f && (IN.ketteiOn() || IN.isCheckO(0) || (EV.deny_skip && EV.INisCancelOn())))))
				{
					EV.MsgCon.showImmediate(false, false);
					flag4 = true;
					EV.t_msgf = -1f;
				}
				EV.MsgCmd.run();
				if ((EV.t_msgf > 11f && EV.skipping >= 1) || (EV.t_msgf > 2f && EV.skipping >= 2))
				{
					EV.MsgCon.showImmediate(false, false);
					flag = flag2;
				}
				else if (EV.t_msgf > 8f && (((flag4 ? EV.INisKetteiPD(8) : EV.INisKettei()) || (EV.deny_skip && EV.INisCancelPD())) && flag2))
				{
					if (!flag4)
					{
						EV.MsgCon.showImmediate(false, false);
					}
					else
					{
						flag = flag2;
					}
				}
			}
			if (flag)
			{
				flag = (flag2 || !EV.MsgCon.isActive() || EV.msg_skip) && EV.state == EV.ST.MESSAGE;
			}
			if (!flag)
			{
				EV.msg_hide = false;
				return false;
			}
			SND.Ui.play("talk_progress", false);
			if (EV.MsgCon.progressNextParagraph())
			{
				EV.t_msgf = 0f;
				return true;
			}
			EV.msg_hide = true;
			EV.state = EV.ST.GOING;
			EV.need_fine_confirmer = true;
			IN.clearSubmitPushDown(false);
			repeatcnt++;
			return true;
		}

		public static bool forceWriteStateToGoing()
		{
			if (EV.state != EV.ST.GOING && EV.isActive(false))
			{
				if (EV.state == EV.ST.LOADING)
				{
					return false;
				}
				IN.clearPushDown(true);
				EV.wait_frame = 0f;
				EV.waitFn = null;
				EV.state = EV.ST.GOING;
			}
			return true;
		}

		internal static EV.ST readOneLine(EvReader ER = null, StringHolder rER = null)
		{
			float num = 1f;
			return EV.readOneLine(ER, rER, ref num);
		}

		internal static EV.ST readOneLine(EvReader ER, StringHolder rER, ref float fcnt_main)
		{
			ER = ((ER == null) ? EV.curEv : ER);
			rER = ((rER == null) ? ER : rER);
			if (ER == null || rER == null)
			{
				return EV.ST.CHANGE;
			}
			CsvVariableContainer varCon = ER.VarCon;
			string text = rER.cmd.ToUpper();
			if (EV.listenerRead(ER, rER, EV.skipping))
			{
				return EV.state;
			}
			if (text == "MTL")
			{
				EV.MsgCmd.add(rER.slice(1, -1000));
			}
			else if (text == "CLEARMTL")
			{
				EV.MsgCmd.clear();
			}
			else if (text == "DOMTL")
			{
				EV.MsgCmd.executeAll();
			}
			else if (text == "TL")
			{
				EV.TlCmd.add(rER.slice(2, -1000), (int)rER._N1);
			}
			else if (text == "CLEARTL")
			{
				EV.TlCmd.clear();
			}
			else if (text == "DOTL")
			{
				EV.TlCmd.executeAll();
			}
			else
			{
				int num = EV.MsgCon.makeFromEvent(rER, EV.curEv);
				if (num != 0)
				{
					EV.msg_skip = false;
					if (num == -1)
					{
						EV.ErrorQuit("EV::readOneLine:: MSG に必要な残り行数がありません: " + EV.curEv.get_cur_line().ToString());
					}
					if (num == 1)
					{
						EV.msg_hide = false;
						return EV.ST.MESSAGE;
					}
					return EV.state;
				}
				else
				{
					if (text != null)
					{
						uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text);
						if (num2 <= 2143797896U)
						{
							if (num2 <= 1201320931U)
							{
								if (num2 <= 365949939U)
								{
									if (num2 <= 191543696U)
									{
										if (num2 != 91365934U)
										{
											if (num2 != 191543696U)
											{
												goto IL_0FF1;
											}
											if (!(text == "TUTO_CAP"))
											{
												goto IL_0FF1;
											}
											if (EV.TutoBox == null)
											{
												return EV.state;
											}
											EvImg pic = EV.Pics.getPic(rER._1, true, true);
											if (pic == null || pic.PF == null)
											{
												rER.tError("不明なイメージ: " + rER._1);
												goto IL_120B;
											}
											EV.TutoBox.AddImage(pic.PF, rER.slice_join(2, " ", ""));
											EV.TutoBox.remActiveFlag("EVENT");
											goto IL_120B;
										}
										else
										{
											if (!(text == "SELECTARRAY"))
											{
												goto IL_0FF1;
											}
											EV.Sel.addRow(rER._1, rER._2, rER._3, rER.IntE(4, 0) != 0);
											goto IL_120B;
										}
									}
									else if (num2 != 234477756U)
									{
										if (num2 != 313177741U)
										{
											if (num2 != 365949939U)
											{
												goto IL_0FF1;
											}
											if (!(text == "START_GDRAW"))
											{
												goto IL_0FF1;
											}
											EV.stop_gdraw = false;
											goto IL_120B;
										}
										else
										{
											if (!(text == "STOP_LOG_RECORD_SELECTION"))
											{
												goto IL_0FF1;
											}
											if (EV.Log != null)
											{
												EV.Log.record_selection = false;
												goto IL_120B;
											}
											goto IL_120B;
										}
									}
									else if (!(text == "TUTO_REMOVE_ALL"))
									{
										goto IL_0FF1;
									}
								}
								else if (num2 <= 867112766U)
								{
									if (num2 != 518391197U)
									{
										if (num2 != 572755376U)
										{
											if (num2 != 867112766U)
											{
												goto IL_0FF1;
											}
											if (!(text == "TUTO_REM_ACTIVE_FLAG"))
											{
												goto IL_0FF1;
											}
											EV.TutoBox.remActiveFlag("EVENT");
											goto IL_120B;
										}
										else
										{
											if (!(text == "<BREAK>"))
											{
												goto IL_0FF1;
											}
											if ((EV.debug & EV.EVDEBUG.ALLOC_CONSOLE) != (EV.EVDEBUG)0)
											{
												EV.Dbg.initBreakPoint(ER);
												return EV.state;
											}
											goto IL_120B;
										}
									}
									else
									{
										if (!(text == "STOP_GHANDLE"))
										{
											goto IL_0FF1;
										}
										EV.handle_game = false;
										goto IL_120B;
									}
								}
								else if (num2 != 901094556U)
								{
									if (num2 != 1095692192U)
									{
										if (num2 != 1201320931U)
										{
											goto IL_0FF1;
										}
										if (!(text == "ALLOW_EVENTHANDLE"))
										{
											goto IL_0FF1;
										}
										EV.MsgCon.setHandle(true);
										EV.Sel.set_handle(true);
										EV.alloc_evhandle_key |= KEY.SIMKEY._EVHANDLE;
										if (EV.isConfirmerState())
										{
											EV.need_fine_confirmer = true;
											goto IL_120B;
										}
										goto IL_120B;
									}
									else if (!(text == "TUTO_REMOVE"))
									{
										goto IL_0FF1;
									}
								}
								else
								{
									if (!(text == "WAIT_MOVE"))
									{
										goto IL_0FF1;
									}
									if (EV.ALsn.Count == 0)
									{
										return EV.ErrorGo("ALsnMoveCheck に何も登録されていません:" + rER.cmd, false);
									}
									if (EV.listenerMoveCheck())
									{
										return EV.ErrorGo("アクティブな動作物がありません: " + rER.cmd, false);
									}
									EV.wait_frame = (float)((int)rER.Nm(1, 0f));
									fcnt_main = 1f;
									return EV.ST.WAIT_MOVE;
								}
								if (EV.TutoBox == null)
								{
									return EV.state;
								}
								EV.TutoBox.RemText(text == "TUTO_REMOVE_ALL", false);
								goto IL_120B;
							}
							else if (num2 <= 1588480151U)
							{
								if (num2 <= 1470622001U)
								{
									if (num2 != 1261320805U)
									{
										if (num2 != 1460223330U)
										{
											if (num2 == 1470622001U)
											{
												if (text == "WAIT_LOAD")
												{
													if (EV.isLoading() || EV.cache_loading > -2)
													{
														EV.active_load = false;
														fcnt_main = 1f;
														return EV.ST.LOADING;
													}
													goto IL_120B;
												}
											}
										}
										else if (text == "TUTO_TEMP_FRONT")
										{
											IN.setZAbs(EV.TutoBox.getTransform(), -3.04f);
											goto IL_120B;
										}
									}
									else if (text == "DENY_MSGLOG")
									{
										if (EV.Log != null)
										{
											EV.Log.allow_msglog = false;
											goto IL_120B;
										}
										goto IL_120B;
									}
								}
								else if (num2 != 1473569256U)
								{
									if (num2 != 1570802611U)
									{
										if (num2 == 1588480151U)
										{
											if (text == "MSG_HIDE")
											{
												EV.MsgCon.hideMsg(rER._B1);
												EV.msg_hide = false;
												goto IL_120B;
											}
										}
									}
									else if (text == "ALLOW_EVENTHANDLE_KEY")
									{
										if (rER.clength == 1)
										{
											EV.alloc_evhandle_key = KEY.SIMKEY._EVHANDLE;
											goto IL_120B;
										}
										for (int i = 1; i < rER.clength; i++)
										{
											KEY.SIMKEY simkey;
											if (FEnum<KEY.SIMKEY>.TryParse(rER.getIndex(i).ToUpper(), out simkey, true))
											{
												EV.alloc_evhandle_key |= simkey;
											}
										}
										goto IL_120B;
									}
								}
								else if (text == "START_GMAIN")
								{
									EV.stop_game = false;
									goto IL_120B;
								}
							}
							else if (num2 <= 1729229931U)
							{
								if (num2 != 1647995660U)
								{
									if (num2 != 1658646193U)
									{
										if (num2 == 1729229931U)
										{
											if (text == "EV_STACK_HOLD")
											{
												EV.ev_stack_stop = false;
												goto IL_120B;
											}
										}
									}
									else if (text == "MSG_SKIPHOLD")
									{
										if (EV.state != EV.ST.MESSAGE)
										{
											return EV.ErrorGo("メッセージ時以外のタイミングで MSG_SKIPHOLD が呼び出されました。", true);
										}
										EV.MsgCon.hold();
										EV.msg_hide = false;
										return EV.ST.MSGSKIP;
									}
								}
								else if (text == "SELECTARRAY_CLEAR")
								{
									EV.Sel.clear();
									goto IL_120B;
								}
							}
							else if (num2 != 1803820898U)
							{
								if (num2 != 1854290132U)
								{
									if (num2 == 2143797896U)
									{
										if (text == "SELECT_RESULT_TO_LOG")
										{
											EV.Sel.addLastSelectionToLog();
											goto IL_120B;
										}
									}
								}
								else if (text == "DEFINE_NEXT_EV")
								{
									if (EV.AStack.Count == 0)
									{
										rER.tError("スタックされているイベントがありません");
										goto IL_120B;
									}
									ER.VarCon.define(rER._1, EV.AStack[0].name, true);
									goto IL_120B;
								}
							}
							else if (text == "ALLOW_MSGLOG")
							{
								if (EV.Log != null)
								{
									EV.Log.allow_msglog = true;
									goto IL_120B;
								}
								goto IL_120B;
							}
						}
						else if (num2 <= 2861077682U)
						{
							if (num2 <= 2355233479U)
							{
								if (num2 <= 2215466556U)
								{
									if (num2 != 2188010346U)
									{
										if (num2 == 2215466556U)
										{
											if (text == "ALLOW_SKIP")
											{
												EV.deny_skip = false;
												EV.Confirmer.default_content = "<key cancel/>/<key submit/>";
												goto IL_120B;
											}
										}
									}
									else if (text == "WAIT_BGM_LOAD")
									{
										X.dl("BGMのロードを待ちます...", null, false, false);
										fcnt_main = 1f;
										return EV.ST.WAIT_BGM_LOAD;
									}
								}
								else if (num2 != 2280906142U)
								{
									if (num2 != 2291262006U)
									{
										if (num2 == 2355233479U)
										{
											if (text == "SELECT_FOCUS_RANDOM_TALK")
											{
												if (rER._1 == ".")
												{
													EV.Sel.considerRandomTalkFocus(true);
													goto IL_120B;
												}
												EV.Sel.initRandomTalkFocus(rER._1);
												goto IL_120B;
											}
										}
									}
									else if (text == "DENY_EVENTHANDLE")
									{
										EV.MsgCon.setHandle(false);
										EV.Sel.set_handle(false);
										EV.alloc_evhandle_key &= ~(KEY.SIMKEY.SUBMIT | KEY.SIMKEY.CANCEL | KEY.SIMKEY.CHECK);
										EV.skipping = 0;
										if (EV.isConfirmerState())
										{
											EV.need_fine_confirmer = true;
											goto IL_120B;
										}
										goto IL_120B;
									}
								}
								else if (text == "MSG_HOLD")
								{
									EV.msg_hide = false;
									if (rER.clength > 1)
									{
										EV.MsgCon.hideMsg(rER, false);
									}
									else
									{
										EV.MsgCon.hold();
									}
									EV.need_fine_confirmer = true;
									goto IL_120B;
								}
							}
							else if (num2 <= 2590772403U)
							{
								if (num2 != 2393865632U)
								{
									if (num2 != 2502237385U)
									{
										if (num2 == 2590772403U)
										{
											if (text == "START_GHANDLE")
											{
												EV.handle_game = true;
												goto IL_120B;
											}
										}
									}
									else if (text == "TUTO_MSG")
									{
										if (EV.TutoBox == null)
										{
											return EV.state;
										}
										int num3 = rER.Int(1, -1000);
										string text2;
										if (num3 == -1000)
										{
											text2 = "&&" + rER._1;
										}
										else
										{
											text2 = EV.curEv.progLines(num3);
										}
										int num4 = rER.Int(2, -1);
										string _ = rER._3;
										EV.TutoBox.AddText(text2, num4, _);
										EV.TutoBox.remActiveFlag("EVENT");
										goto IL_120B;
									}
								}
								else if (text == "WAIT")
								{
									EV.wait_frame = (float)((int)rER.Nm(1, 0f));
									fcnt_main = 0.5f;
									return EV.ST.WAIT;
								}
							}
							else if (num2 != 2707838650U)
							{
								if (num2 != 2755367452U)
								{
									if (num2 == 2861077682U)
									{
										if (text == "DENY_EVENTHANDLE_KEY")
										{
											for (int j = 1; j < rER.clength; j++)
											{
												KEY.SIMKEY simkey2;
												if (FEnum<KEY.SIMKEY>.TryParse(rER.getIndex(j).ToUpper(), out simkey2, true))
												{
													EV.alloc_evhandle_key &= ~simkey2;
												}
											}
											goto IL_120B;
										}
									}
								}
								else if (text == "SELECT_POS")
								{
									EV.Sel.setPos(rER._1);
									goto IL_120B;
								}
							}
							else if (text == "SELECT_FOCUS")
							{
								int num5 = rER.Int(1, -2);
								if (num5 == -2)
								{
									EV.Sel.setDefaultFocus(rER._1);
								}
								else
								{
									EV.Sel.default_focus = num5;
								}
								if (rER._B2)
								{
									EV.Sel.setCheckMark(num5);
									goto IL_120B;
								}
								goto IL_120B;
							}
						}
						else if (num2 <= 3391144223U)
						{
							if (num2 <= 3107000490U)
							{
								if (num2 != 2890002407U)
								{
									if (num2 != 3022600877U)
									{
										if (num2 == 3107000490U)
										{
											if (text == "STOP_GMAIN")
											{
												EV.stop_game = true;
												goto IL_120B;
											}
										}
									}
									else if (text == "SELECT")
									{
										float num6 = X.Nm(rER._1, -1f, false);
										string _2 = rER._2;
										if (num6 <= 0f)
										{
											if (!EV.Sel.activate(_2))
											{
												return EV.state;
											}
											return EV.ST.SELECT;
										}
										else
										{
											if ((float)EV.curEv.restLines() < num6)
											{
												return EV.ErrorQuit("EV::readOneLine:: SELECT に必要な残り行数がありません: " + EV.curEv.get_cur_line().ToString());
											}
											EV.Sel.clear();
											while ((num6 -= 1f) >= 0f)
											{
												ER.read();
												if (ER._2U.IndexOf("D") >= 0)
												{
													EV.Sel.default_focus = EV.Sel.countRows();
												}
												EV.Sel.addRow(ER.cmd, ER._1, ER._2, false);
											}
											if (!EV.Sel.activate(_2))
											{
												return EV.state;
											}
											return EV.ST.SELECT;
										}
									}
								}
								else if (text == "START_LOG_RECORD_SELECTION")
								{
									if (EV.Log != null)
									{
										EV.Log.record_selection = true;
										goto IL_120B;
									}
									goto IL_120B;
								}
							}
							else if (num2 != 3126320842U)
							{
								if (num2 != 3242984370U)
								{
									if (num2 == 3391144223U)
									{
										if (text == "LOG_RECORD")
										{
											if (EV.Log != null)
											{
												EV.Log.ExplodeBuffer();
												goto IL_120B;
											}
											goto IL_120B;
										}
									}
								}
								else if (text == "TUTO_POS")
								{
									if (EV.TutoBox == null)
									{
										return EV.state;
									}
									if (rER.clength == 1)
									{
										EV.TutoBox.setPositionDefault();
										goto IL_120B;
									}
									EV.TutoBox.setPosition(rER._1U, rER._2U);
									goto IL_120B;
								}
							}
							else if (text == "EV_STACK_STOP")
							{
								EV.ev_stack_stop = true;
								goto IL_120B;
							}
						}
						else if (num2 <= 3952809996U)
						{
							if (num2 != 3440812095U)
							{
								if (num2 != 3815405721U)
								{
									if (num2 == 3952809996U)
									{
										if (text == "MSG_SKIP")
										{
											if (EV.state == EV.ST.MESSAGE)
											{
												EV.MsgCon.showImmediate(false, true);
												return EV.ST.MSGSKIP;
											}
											return EV.state;
										}
									}
								}
								else if (text == "WAIT_BUTTON")
								{
									EV.wait_frame = (float)((int)rER.Nm(1, -1f));
									fcnt_main = 1f;
									return EV.ST.WAIT_BTN;
								}
							}
							else if (text == "DENY_SKIP")
							{
								EV.deny_skip = true;
								EV.Confirmer.default_content = "<key submit/>";
								EV.skipping = 0;
								fcnt_main = 1f;
								goto IL_120B;
							}
						}
						else if (num2 != 3964561753U)
						{
							if (num2 != 4091258075U)
							{
								if (num2 == 4177142381U)
								{
									if (text == "WAIT_FN")
									{
										IEventWaitListener eventWaitListener = X.Get<string, IEventWaitListener>(EV.OLsnWait, rER._1);
										if (eventWaitListener == null)
										{
											return EV.ErrorGo("OLsnWait[" + rER.cmd + "]に何も登録されていません", false);
										}
										fcnt_main = 1f;
										return EV.initWaitFn(eventWaitListener, (int)rER.Nm(2, 0f));
									}
								}
							}
							else if (text == "EV_CLEAR_ALL_STACK")
							{
								int num7 = EV.AStack.IndexOf(ER);
								if (num7 >= 0)
								{
									EV.AStack.RemoveRange(num7 + 1, EV.AStack.Count - (num7 + 1));
									goto IL_120B;
								}
								EV.AStack.RemoveRange(0, EV.AStack.Count);
								goto IL_120B;
							}
						}
						else if (text == "STOP_GDRAW")
						{
							EV.stop_gdraw = true;
							goto IL_120B;
						}
					}
					IL_0FF1:
					if (GF.readEvLineGf(ER, rER, EV.skipping))
					{
						return EV.state;
					}
					if (BGM.readEvLineBgm(ER, rER, EV.skipping))
					{
						return EV.state;
					}
					bool flag = TX.charIs(text, 0, '@');
					if (text == "MODULE" || text == "CHANGE_EVENT2" || flag)
					{
						if (EV.moduleInit(ER, flag ? TX.slice(rER.cmd, 1) : rER._1, rER.slice(flag ? 1 : 2, -1000), text == "MODULE" || flag))
						{
							return EV.ST.CHANGE;
						}
					}
					else if (text == "CHANGE_EVENT")
					{
						if (EV.changeEvent(rER._1, 0, rER.slice(2, -1000)))
						{
							return EV.ST.CHANGE;
						}
					}
					else if (!(text == "<DEBUG>"))
					{
						if (text == "<LOAD>" || text == "<LOADA>")
						{
							if (X.DEBUG && text == "<LOAD>")
							{
								EV.skipping = 0;
							}
							EV.active_load = text == "<LOADA>";
							EV.cacheMaterial(ER, true);
							X.dl("ロードポイント: " + ((ER._1 == "") ? ("Line " + (ER.get_cur_line() + 1).ToString()) : ER._1), null, false, false);
							if (EV.state == EV.ST.LOADING)
							{
								return EV.ST.LOADING;
							}
						}
						else if (!(text == "<NOLOAD>") && !(text == "<MODULE_LOAD>") && !(text == "<CLEAR_CACHE>"))
						{
							if (!(text == "<CHECK>"))
							{
								return EV.ErrorGo("不明なコマンド:" + text, true);
							}
							if (X.DEBUG)
							{
								X.dl("チェックポイント: " + ((ER._1 == "") ? ("Line " + (ER.get_cur_line() + 1).ToString()) : ER._1), null, false, false);
								EV.skipping = 0;
							}
						}
					}
				}
			}
			IL_120B:
			return EV.state;
		}

		public static bool moduleInit(EvReader ER, string module_key, string[] Avariables = null, bool is_module = false)
		{
			EV.ev_stack_stop = false;
			EvReader evReader = EV.stack(module_key, 0, 0, Avariables, null);
			return evReader != null && EV.moduleInitAfter(ER, evReader, is_module);
		}

		public static bool moduleInit(EvReader ER, EvReader Mod, string[] Avariables = null, bool is_module = false)
		{
			if (Mod == null)
			{
				return false;
			}
			EV.ev_stack_stop = false;
			Mod.seekReset();
			Mod = EV.stackReader(Mod, 0);
			if (Mod == null)
			{
				return false;
			}
			Mod.Avariables = Avariables;
			return EV.moduleInitAfter(ER, Mod, is_module);
		}

		public static bool moduleInitAfter(EvReader ER, EvReader Mod, bool is_module = false)
		{
			if (Mod != null)
			{
				Mod.no_init_load = is_module;
			}
			if (ER.get_cur_line() < ER.getLength() - 1)
			{
				EV.stack("", ER.get_cur_line() + 1, 1, ER.VarCon.extractArrayNumeric(), ER).no_init_load = true;
			}
			if (!is_module)
			{
				EV.skipping = 0;
			}
			EV.state = EV.ST.CHANGE;
			return true;
		}

		public static bool changeEvent(string _event, int _start_line = 0, string[] Avariables = null)
		{
			EV.ev_stack_stop = false;
			EV.TlCmd.executeAll();
			EV.skipping = 0;
			if (EV.stack(_event, _start_line, 0, Avariables, null) == null)
			{
				return false;
			}
			EV.state = EV.ST.CHANGE;
			return true;
		}

		public static bool changeEvent(EvReader _Mod, int _start_line = 0, string[] Avariables = null)
		{
			EV.ev_stack_stop = false;
			EV.TlCmd.executeAll();
			EV.skipping = 0;
			_Mod.seekReset();
			if (_start_line > 0)
			{
				_Mod.seek_set(_start_line);
			}
			_Mod = EV.stackReader(_Mod, 0);
			if (_Mod == null)
			{
				return false;
			}
			_Mod.Avariables = Avariables;
			EV.state = EV.ST.CHANGE;
			return true;
		}

		public static EV.ST initWaitFn(IEventWaitListener _waitFn, int spc_wait_time = 0)
		{
			EV.wait_frame = (float)spc_wait_time;
			EV.waitFn = _waitFn;
			EV.state = EV.ST.WAIT_FN;
			try
			{
				EV.waitFn = _waitFn;
				if (EV.waitFn == null)
				{
					EV.state = ((EV.wait_frame > 0f) ? EV.ST.WAIT : EV.ST.GOING);
				}
				else if (!EV.waitFn.EvtWait(true))
				{
					EV.waitFn = null;
					EV.state = ((EV.wait_frame > 0f) ? EV.ST.WAIT : EV.ST.GOING);
				}
			}
			catch (Exception ex)
			{
				X.de(ex.ToString(), null);
				if (EV.waitFn == null || !EV.waitFn.EvtWait(false))
				{
					EV.waitFn = null;
					EV.state = ((EV.wait_frame > 0f) ? EV.ST.WAIT : EV.ST.GOING);
				}
			}
			return EV.state;
		}

		private static EV.ST ErrorGo(string str, bool is_error = true)
		{
			EV.curEv.tNote(str, is_error);
			return EV.state;
		}

		private static EV.ST ErrorQuit(string str)
		{
			EV.curEv.tError(str);
			return EV.ST.CHANGE;
		}

		public static void preserveEventExecuted(EV.ST _s)
		{
			if (_s == EV.ST.GOING || _s == EV.ST.NONE || _s == EV.state)
			{
				return;
			}
			if (EV.curEv != null)
			{
				if (_s != EV.ST.MSGSKIP)
				{
					string name = EV.curEv.name;
					int count = EV.AStack.Count;
					for (int i = ((_s == EV.ST.CHANGE) ? 0 : (-1)); i < count; i++)
					{
						EvReader evReader = ((i == -1) ? EV.curEv : EV.AStack[i]);
						if (evReader.name == name)
						{
							evReader.statePreserve(EV.state);
							break;
						}
					}
				}
				EV.state = _s;
				if (EV.isConfirmerState() || EV.Confirmer.trs_enabled)
				{
					EV.need_fine_confirmer = true;
				}
			}
		}

		public static bool isEverythingStasis()
		{
			return EV.nexEv == null && EV.curEv == null && (EV.do_not_draw_loading_circle || EV.t_load == 0f) && EV.DC.getActiveCount() == 0 && !EV.msg_active;
		}

		public static bool do_not_draw_loading_circle
		{
			get
			{
				return EV.t_load == -1000f;
			}
			set
			{
				if (EV.do_not_draw_loading_circle == value)
				{
					return;
				}
				EV.t_load = (float)(value ? (-1000) : 0);
				if (value && EV.MrdLoad != null)
				{
					EV.MrdLoad.gameObject.SetActive(false);
				}
			}
		}

		private static bool drawLoading(float fcnt, MeshDrawer _Md)
		{
			float num = 0f;
			bool flag = false;
			if (EV.isLoading())
			{
				if (EV.t_load == -1000f)
				{
					return true;
				}
				if (EV.t_load <= 0f)
				{
					EV.t_load = 0f;
				}
				EV.t_load += fcnt;
				num = 1f;
				flag = true;
			}
			else
			{
				if (EV.t_load == 0f || EV.t_load == -1f || EV.t_load == -1000f)
				{
					return false;
				}
				if (EV.t_load >= 0f || EV.t_load < -40f)
				{
					EV.t_load = -40f;
				}
				if (EV.t_load <= -1f)
				{
					EV.t_load = X.Mn(EV.t_load + fcnt, -1f);
					num = X.ZLINE(-EV.t_load - 1f, 39f);
				}
			}
			if (num != 0f)
			{
				if (X.D)
				{
					if (!EV.MrdLoad.gameObject.activeSelf)
					{
						EV.MrdLoad.gameObject.SetActive(true);
					}
					int num2 = 70 / 2;
					_Md.clear(false, false);
					EV.drawLoadingCircleTo(_Md, num, (float)(EV.pw / 2 - num2 - 40), (float)(-(float)EV.ph / 2 + num2 + 40));
					_Md.updateForMeshRenderer(false);
				}
			}
			else if (EV.MrdLoad.gameObject.activeSelf)
			{
				EV.MrdLoad.gameObject.SetActive(false);
			}
			return flag;
		}

		public static void drawLoadingCircleTo(MeshDrawer _Md, float draw_a, float cx0, float cy0)
		{
			int num = 3 - IN.totalframe / 11 % 4;
			float num2 = (float)(-(float)IN.totalframe) / 480f * 3.1415927f * 2f;
			for (int i = 0; i < 8; i++)
			{
				float num3 = (float)i / 8f * 3.1415927f * 2f - num2;
				float num4 = ((num == i || num + 4 == i) ? 1f : 1.5f);
				float num5 = cx0 + 22f * X.Cos(num3);
				float num6 = cy0 + 22f * X.Sin(num3);
				_Md.Col = MTRX.cola.Set(0).setA(100f * draw_a).C;
				_Md.Circle(num5, num6, 2f * num4 + 1f, 0f, false, 0f, 0f);
				_Md.Col = MTRX.cola.Set(16777215).setA(draw_a).C;
				_Md.Circle(num5, num6, 2f * num4, 0f, false, 0f, 0f);
			}
		}

		public static void addListener(IEventListener Fn)
		{
			if (Fn != null)
			{
				if (EV.ALsn == null)
				{
					EV.ALsn = new List<IEventListener>(4);
				}
				EV.ALsn.Add(Fn);
			}
		}

		public static void addWaitListener(string key, IEventWaitListener Fn)
		{
			if (Fn != null)
			{
				EV.remWaitListener(key);
				EV.remWaitListener(Fn);
				EV.OLsnWait[key] = Fn;
			}
		}

		public static void remListener(IEventListener Fn)
		{
			if (Fn != null && EV.ALsn != null)
			{
				EV.ALsn.Remove(Fn);
			}
		}

		public static void remWaitListener(string k)
		{
			if (EV.OLsnWait == null)
			{
				return;
			}
			EV.OLsnWait.Remove(k);
		}

		public static void remWaitListener(IEventWaitListener k)
		{
			if (EV.OLsnWait == null)
			{
				return;
			}
			BList<string> blist = null;
			foreach (KeyValuePair<string, IEventWaitListener> keyValuePair in EV.OLsnWait)
			{
				if (keyValuePair.Value == k)
				{
					if (blist == null)
					{
						blist = ListBuffer<string>.Pop(0);
					}
					blist.Add(keyValuePair.Key);
				}
			}
			if (blist != null)
			{
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					EV.remWaitListener(blist[i]);
				}
				ListBuffer<string>.Release(blist);
			}
		}

		private static bool listenerOpen(bool is_init)
		{
			int count = EV.ALsn.Count;
			for (int i = 0; i < count; i++)
			{
				IEventListener eventListener = EV.ALsn[i];
				if (eventListener == null)
				{
					break;
				}
				if (!eventListener.EvtOpen(is_init))
				{
					return false;
				}
			}
			return true;
		}

		public static void addHkdsFollowableListener(IHkdsFollowableContainer _FC)
		{
			if (EV.AHkdsFollowCon == null)
			{
				EV.AHkdsFollowCon = new List<IHkdsFollowableContainer>();
			}
			if (EV.AHkdsFollowCon.IndexOf(_FC) == -1)
			{
				EV.AHkdsFollowCon.Add(_FC);
			}
		}

		public static void remHkdsFollowableListener(IHkdsFollowableContainer _FC)
		{
			if (EV.AHkdsFollowCon != null)
			{
				EV.AHkdsFollowCon.Remove(_FC);
			}
		}

		public static IHkdsFollowable getHkdsFollowableObject(string key)
		{
			if (EV.AHkdsFollowCon == null)
			{
				return null;
			}
			for (int i = EV.AHkdsFollowCon.Count - 1; i >= 0; i--)
			{
				IHkdsFollowable hkdsFollowable = EV.AHkdsFollowCon[i].FindHkdsFollowableObject(key);
				if (hkdsFollowable != null)
				{
					return hkdsFollowable;
				}
			}
			return null;
		}

		private static bool listenerRead(EvReader ER = null, StringHolder rER = null, int skipping = 0)
		{
			ER = ((ER == null) ? EV.curEv : ER);
			rER = ((rER == null) ? ER : rER);
			int count = EV.ALsn.Count;
			for (int i = 0; i < count; i++)
			{
				IEventListener eventListener = EV.ALsn[i];
				if (eventListener == null)
				{
					break;
				}
				if (eventListener.EvtRead(ER, rER, skipping))
				{
					return true;
				}
			}
			return false;
		}

		public static bool listenerCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			string cmd2 = rER.cmd;
			if (cmd2 != null && cmd2 == "SELECT")
			{
				float num = X.Nm(rER._1, -1f, false);
				if (num > 0f)
				{
					while ((num -= 1f) >= 0f)
					{
						rER.readCorrectly();
					}
					return true;
				}
			}
			int count = EV.ALsn.Count;
			for (int i = 0; i < count; i++)
			{
				IEventListener eventListener = EV.ALsn[i];
				if (eventListener == null)
				{
					break;
				}
				int num2 = eventListener.EvtCacheRead(ER, cmd, rER);
				if (num2 > 0)
				{
					if (num2 >= 2)
					{
						EV.initLoadTicket();
					}
					return true;
				}
			}
			return false;
		}

		private static bool listenerClose(bool is_end)
		{
			int num = EV.ALsn.Count;
			int i = 0;
			while (i < num)
			{
				IEventListener eventListener = EV.ALsn[i];
				if (eventListener == null)
				{
					break;
				}
				if (!eventListener.EvtClose(is_end))
				{
					return false;
				}
				if (EV.ALsn.Count < num)
				{
					num = EV.ALsn.Count;
				}
				else
				{
					i++;
				}
			}
			return true;
		}

		private static bool listenerMoveCheck()
		{
			int count = EV.ALsn.Count;
			for (int i = 0; i < count; i++)
			{
				IEventListener eventListener = EV.ALsn[i];
				if (eventListener == null)
				{
					break;
				}
				if (!eventListener.EvtMoveCheck())
				{
					return false;
				}
			}
			return true;
		}

		public static void fixObjectPositionTo(Transform Tr, string key)
		{
			EV.DC.fixObjectPositionTo(Tr, key);
		}

		public static void clearEventContent(string _name = null)
		{
			if (_name == null)
			{
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					foreach (KeyValuePair<string, string> keyValuePair in EV.Oevt_content)
					{
						if (keyValuePair.Key.IndexOf("%") != 0)
						{
							blist.Add(keyValuePair.Key);
						}
					}
					int count = blist.Count;
					for (int i = 0; i < count; i++)
					{
						EV.clearEventContent(blist[i]);
					}
					return;
				}
			}
			EV.Oevt_content.Remove(_name);
		}

		public static void createListenerTxEval(object EvInstance)
		{
			TxEvalListenerContainer txEvalListenerContainer = TX.createListenerEval(EvInstance, 8, true);
			txEvalListenerContainer.Add("DEF", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				string text = X.Get<string>(Aargs, 0, "");
				TX.InputE((float)(EV.getVariableContainer().isDefined(text) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("game_stopping", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(EV.isStoppingGame() ? 1 : 0));
			}, Array.Empty<string>());
			GF.createListenerEval(txEvalListenerContainer);
		}

		public static void flushExternal()
		{
			if (EV.APxls == null)
			{
				return;
			}
			for (int i = EV.APxls.Length - 1; i >= 0; i--)
			{
				EvPerson.EvPxlsLoader evPxlsLoader = EV.APxls[i];
				if (!evPxlsLoader.is_static)
				{
					evPxlsLoader.releasePxlImage();
				}
			}
			BGM.flushEventLoadedTiming();
		}

		public static EvPerson.EvPxlsLoader getLoaderForPerson(string person_key)
		{
			for (int i = EV.APxls.Length - 1; i >= 0; i--)
			{
				EvPerson.EvPxlsLoader evPxlsLoader = EV.APxls[i];
				if (evPxlsLoader.person_key == person_key)
				{
					return evPxlsLoader;
				}
			}
			return null;
		}

		public static EvPerson.EvPxlsLoader getLoaderForPxl(PxlCharacter Pc)
		{
			for (int i = EV.APxls.Length - 1; i >= 0; i--)
			{
				EvPerson.EvPxlsLoader evPxlsLoader = EV.APxls[i];
				if (evPxlsLoader.Pc == Pc)
				{
					return evPxlsLoader;
				}
			}
			return null;
		}

		public static void initLoadTicket(string name, string name2, LoadTicketManager.FnLoadProgress FD_Progress, object Target = null, int priority = 0)
		{
			EV.initLoadTicket();
			LoadTicketManager.AddTicket(name, name2, FD_Progress, Target, priority);
		}

		private static void initLoadTicket()
		{
			if (!EV.ticket_loading)
			{
				EV.ticket_loading = true;
				LoadTicketManager.AddTicket("LOADEV", "", delegate(LoadTicketManager.LoadTicket Tk)
				{
					EV.ticket_loading = false;
					return false;
				}, null, 255);
			}
		}

		public static bool isConfirmerState()
		{
			return EV.state == EV.ST.MESSAGE || EV.state == EV.ST.WAIT_BTN;
		}

		public static void fineConfirmerFront()
		{
			EV.need_fine_confirmer = false;
			if (!EV.canEventHandle() || EV.curEv == null)
			{
				EV.Confirmer.deactivate();
				return;
			}
			if (EV.state == EV.ST.MESSAGE)
			{
				EV.MsgCon.fineFrontConfirmer();
				return;
			}
			if (EV.state == EV.ST.WAIT_BTN)
			{
				EV.Confirmer.init(EV.Instance, EV.Instance.transform, true);
				return;
			}
			EV.Confirmer.deactivate();
		}

		public void initConfirmAnnouncer(ConfirmAnnouncer Confirmer, out string content, out ALIGN align, out ALIGNY aligny, out Vector3 Pivot)
		{
			content = null;
			align = ALIGN.RIGHT;
			aligny = ALIGNY.BOTTOM;
			Pivot = new Vector3(IN.wh - 10f, -IN.hh + 8f, -1.8500001f);
			Confirmer.BorderCol(MTRX.ColWhite);
		}

		private static bool INisKetteiPD(int alloc_pd = 1)
		{
			if (EV.debug != (EV.EVDEBUG)0 && EV.Dbg.isELActive())
			{
				return IN.isSubmitPD(alloc_pd) || IN.isCheckPD(alloc_pd) || EV.Dbg.isELKettei();
			}
			return ((EV.alloc_evhandle_key & KEY.SIMKEY.SUBMIT) != (KEY.SIMKEY)0 && IN.ketteiPD(alloc_pd)) || ((EV.alloc_evhandle_key & KEY.SIMKEY.CHECK) != (KEY.SIMKEY)0 && IN.isCheckPD(alloc_pd));
		}

		public static bool INisKettei()
		{
			if (EV.debug != (EV.EVDEBUG)0 && EV.Dbg.isELActive())
			{
				return IN.isSubmit() || EV.Dbg.isELKettei();
			}
			return ((EV.alloc_evhandle_key & KEY.SIMKEY.SUBMIT) != (KEY.SIMKEY)0 && IN.kettei()) || ((EV.alloc_evhandle_key & KEY.SIMKEY.CHECK) != (KEY.SIMKEY)0 && IN.isCheckPD(1));
		}

		public static bool INisKetteiM3()
		{
			if (EV.debug != (EV.EVDEBUG)0 && EV.Dbg.isELActive())
			{
				return IN.isSubmit() || IN.isCancel() || EV.Dbg.isELKettei();
			}
			if ((EV.alloc_evhandle_key & KEY.SIMKEY._EVHANDLE) == KEY.SIMKEY._EVHANDLE)
			{
				return IN.ketteiM3();
			}
			return (EV.alloc_evhandle_key & KEY.SIMKEY._EVHANDLE) != (KEY.SIMKEY)0 && (((EV.alloc_evhandle_key & KEY.SIMKEY.SUBMIT) != (KEY.SIMKEY)0 && IN.kettei()) || ((EV.alloc_evhandle_key & KEY.SIMKEY.CHECK) != (KEY.SIMKEY)0 && IN.isCheckPD(1)) || ((EV.alloc_evhandle_key & KEY.SIMKEY.CANCEL) != (KEY.SIMKEY)0 && IN.isCancel()));
		}

		private static bool INisKetteiOn()
		{
			if (EV.debug != (EV.EVDEBUG)0 && EV.Dbg.isELActive())
			{
				return IN.isSubmitOn(0) || EV.Dbg.isELKettei();
			}
			return IN.ketteiOn();
		}

		private static bool INisSubmitUp()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY.SUBMIT) != (KEY.SIMKEY)0 && IN.isSubmitUp(-1);
		}

		private static bool INisCancelPD()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY.CANCEL) != (KEY.SIMKEY)0 && IN.isCancelPD();
		}

		public static bool INisCancel()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY.CANCEL) != (KEY.SIMKEY)0 && IN.isCancel();
		}

		private static bool INisCancelOn()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY.CANCEL) != (KEY.SIMKEY)0 && IN.isCancelOn(0);
		}

		private static bool INisMenuOn()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY.CANCEL) != (KEY.SIMKEY)0 && IN.isMenuO(0);
		}

		public static bool preLoadExternalImages()
		{
			int num = EV.APxls.Length;
			for (int i = 0; i < num; i++)
			{
				EvPerson.EvPxlsLoader evPxlsLoader = EV.APxls[i];
				evPxlsLoader.setStaticFlagForDebugger();
				evPxlsLoader.preparePxlChar(true, true);
			}
			return EV.ticket_loading;
		}

		public static bool preLoadExternalImagesAfter()
		{
			return false;
		}

		public static void addExternalPxlsAfter(string person_key, PxlCharacter Pc)
		{
			int num = EV.APxls.Length;
			EvPerson.EvPxlsLoader evPxlsLoader = null;
			for (int i = 0; i < num; i++)
			{
				if (EV.APxls[i].Pc == Pc)
				{
					evPxlsLoader = (EV.APxls[i] = new EvPerson.EvPxlsLoader(person_key, Pc));
					break;
				}
			}
			if (evPxlsLoader == null)
			{
				X.push<EvPerson.EvPxlsLoader>(ref EV.APxls, evPxlsLoader = new EvPerson.EvPxlsLoader(person_key, Pc), -1);
			}
			evPxlsLoader.preparePxlChar(true, true);
		}

		public static EvReader getStacked(string name)
		{
			int count = EV.AStack.Count;
			for (int i = 0; i < count; i++)
			{
				EvReader evReader = EV.AStack[i];
				if (evReader == null)
				{
					return null;
				}
				if (evReader.name == name)
				{
					return evReader;
				}
			}
			return null;
		}

		public static TalkDrawer getTalkerDrawer(string person)
		{
			return EV.DC.getTalker(person, false);
		}

		public static TalkDrawer[] getTalkerDrawerList(out int max)
		{
			return EV.DC.getTalkerDrawerList(out max);
		}

		public static bool isLoading()
		{
			return EV.ticket_loading;
		}

		public static bool canProgress()
		{
			return !EV.isStoppingGame();
		}

		public static IEventWaitListener getCurrentWaitListener()
		{
			return EV.waitFn;
		}

		public static void stopGMain(bool flag = true)
		{
			EV.stop_game = flag;
		}

		public static void setGHandleFlag(bool flag = true)
		{
			EV.handle_game = flag;
		}

		public static void StopGMainDrawFlag(bool flag = true)
		{
			EV.stop_gdraw = flag;
		}

		public static bool isStoppingGame()
		{
			return EV.isLoading() || EV.stop_game || (EV.nexEv != null && EV.curEv == null);
		}

		public static bool isStoppingGameDraw()
		{
			return EV.stop_gdraw;
		}

		public static bool isStoppingEventHandle()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY._EVHANDLE) == (KEY.SIMKEY)0 || EV.isLoading() || (EV.nexEv != null && EV.curEv == null);
		}

		public static bool isStoppingGameHandle()
		{
			return (EV.debug != (EV.EVDEBUG)0 && EV.Dbg != null && EV.Dbg.isActive()) || EV.isLoading() || !EV.handle_game || EV.stop_game || (EV.nexEv != null && EV.curEv == null);
		}

		public static EvDebugger getDebugger()
		{
			return EV.Dbg;
		}

		public static bool bg_full_filled
		{
			get
			{
				if (EV.bg_full_filled_ >= 2 && EV.DC != null)
				{
					EV.bg_full_filled_ = (byte)EV.DC.isFilledWholeScreen();
				}
				return EV.bg_full_filled_ == 1;
			}
			set
			{
				EV.bg_full_filled_ = (value ? 1 : 2);
			}
		}

		public static EvReader getCurrentEvent()
		{
			return EV.curEv;
		}

		public static IMessageContainer getMessageContainer()
		{
			return EV.MsgCon;
		}

		public static bool isActive(bool no_consider_loading = false)
		{
			return (!no_consider_loading && EV.isLoading()) || EV.nexEv != null || EV.curEv != null;
		}

		public static bool isActive(string key, bool check_only_front = false)
		{
			if (EV.curEv != null)
			{
				return EV.curEv.name == key;
			}
			return !check_only_front && EV.nexEv != null && EV.nexEv.name == key;
		}

		public static int isWaitMoveSkipping()
		{
			if (EV.state != EV.ST.WAIT_MOVE)
			{
				return 0;
			}
			return EV.skipping;
		}

		public static bool isWaiting(IEventWaitListener WFn)
		{
			return EV.state == EV.ST.WAIT_FN && EV.waitFn == WFn;
		}

		public static bool isSpecialWaitingState()
		{
			return EV.state == EV.ST.WAIT_FN;
		}

		public static bool canEventHandle()
		{
			return (EV.alloc_evhandle_key & KEY.SIMKEY._EVHANDLE) > (KEY.SIMKEY)0;
		}

		public static bool lockPrInputManipulate(KEY.SIMKEY key, bool default_flag = true, bool is_or = false)
		{
			if ((EV.alloc_evhandle_key & ~(KEY.SIMKEY.SUBMIT | KEY.SIMKEY.CANCEL | KEY.SIMKEY.CHECK)) == (KEY.SIMKEY)0)
			{
				return default_flag;
			}
			if (!is_or)
			{
				return (EV.alloc_evhandle_key & key) == key;
			}
			return (EV.alloc_evhandle_key & key) > (KEY.SIMKEY)0;
		}

		public static void addAllocEvHandleKey(KEY.SIMKEY key, bool flag = true)
		{
			if (flag)
			{
				EV.alloc_evhandle_key |= key;
				return;
			}
			EV.alloc_evhandle_key &= ~key;
		}

		public static bool handle_randamise_key_enabled
		{
			get
			{
				return (EV.alloc_evhandle_key & KEY.SIMKEY._RANDOMISE) > (KEY.SIMKEY)0;
			}
		}

		public static bool isMessageWaiting()
		{
			return (EV.nexEv != null || EV.curEv != null) && EV.state == EV.ST.MESSAGE;
		}

		public static CsvVariableContainer getVariableContainer()
		{
			if (EV.VarCon == null)
			{
				return CsvVariableContainer.getDefault(false);
			}
			return EV.VarCon;
		}

		public override string ToString()
		{
			return "EV";
		}

		public const string ev_dir = "evt/";

		public const string evimg_dir = "EvImg/";

		public const string ev_ext = ".cmd";

		public const string vp_dir = "VP/";

		public const int talker_max = 5;

		private static List<EvReader> AStack;

		private static List<IHkdsFollowableContainer> AHkdsFollowCon;

		private static int pw_ = 1024;

		private static int ph_ = 576;

		private static int psw_ = 1024;

		private static int psh_ = 576;

		private static EvReader curEv;

		private static EvReader nexEv;

		public static bool no_first_run = false;

		private static bool stop_game;

		private static bool stop_gdraw;

		private static bool handle_game = true;

		private static EV.ST state_ = EV.ST.NONE;

		private static float wait_frame = 0f;

		internal static KEY.SIMKEY alloc_evhandle_key = KEY.SIMKEY._EVHANDLE;

		private static IEventWaitListener waitFn;

		private static bool wait_fn_no_arg = false;

		public static EvSelector Sel;

		public static EvMsgCommand MsgCmd;

		public static EvTLCommand TlCmd;

		private static CsvVariableContainer VarConCache;

		private static ClsPool<CsvReader> PoolERCache;

		private static EvPerson.EvPxlsLoader[] APxls;

		private static MSG MsgDefault;

		private static IMessageContainer MsgCon;

		private static bool msg_run = true;

		private static int pre_msg_hide;

		public static EvDrawerContainer DC;

		public static EvImgContainer Pics;

		public static ITutorialBox TutoBox;

		public static ConfirmAnnouncer Confirmer;

		public static Func<GameObject, ConfirmAnnouncer> FnCreateConfirmer;

		public static EvTextLog Log;

		public static bool run_drawer = true;

		public static int skipping = 0;

		public static bool deny_skip = false;

		public static bool active_load = false;

		private static bool ev_stack_stop = false;

		public const int SKIP_ESC = 2;

		public const int SKIP_X = 1;

		private static List<IEventListener> ALsn;

		private static BDic<string, IEventWaitListener> OLsnWait;

		private static BDic<string, string> Oevt_content;

		private static float t_load = -1f;

		private static int cache_loading = -2;

		private static bool no_reload = false;

		private static bool first_read_flag = false;

		private static bool msg_hide = false;

		private static bool msg_skip = false;

		private static global::UnityEngine.Object OCharAlias = null;

		public static EV Instance;

		private static MultiMeshRenderer MMRD;

		private static MeshDrawer MdLoad;

		private static ValotileRenderer ValotLoad;

		private static MeshRenderer MrdLoad;

		private static byte bg_full_filled_;

		public static bool msg_active;

		public static bool need_fine_confirmer = false;

		private static float t_msgf;

		public static bool ticket_loading;

		private static CsvVariableContainer VarCon;

		public const float Z_EV = -3f;

		public const float Z_EV_T = -4.8f;

		public const float Z_EV_SEL = -4.925f;

		public const float Z_UI = -4f;

		public const float Z_EVDBUGGER = -5.5f;

		private static bool first_event_stacked;

		private static EvDebugger Dbg;

		public static EV.EVDEBUG debug;

		public static Action<bool> ActionEvDebugEnableChanged = delegate(bool c)
		{
		};

		private static bool valotile_overwrite_msg_;

		public enum ST
		{
			NONE = -1,
			GOING,
			WAIT,
			MESSAGE,
			WAIT_1,
			LOADING,
			SELECT,
			CHANGE,
			WAIT_BTN,
			WAIT_MOVE,
			DEBUGSTOP,
			WAIT_BGM_LOAD,
			MSGSKIP,
			TEMPSTOP,
			WAIT_TRANS,
			WAIT_FN
		}

		public enum EVDEBUG
		{
			ALLOC_CONSOLE = 1,
			_ALL = 1
		}
	}
}
