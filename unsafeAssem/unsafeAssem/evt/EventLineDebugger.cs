using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace evt
{
	public class EventLineDebugger : Designer
	{
		public static void initLineDebugger()
		{
			try
			{
				string @string = PlayerPrefs.GetString("EVDEBUGGER_LINE_DAT");
				if (TX.valid(@string))
				{
					string[] array = TX.split(@string, "\n");
					for (int i = array.Length - 1; i >= 0; i--)
					{
						switch (i)
						{
						case 0:
							EventLineDebugger.max_delay = X.NmI(array[i], EventLineDebugger.max_delay, true, false);
							break;
						case 1:
							EventLineDebugger.use_if_inject = X.Nm(array[i], 0f, true) != 0f;
							break;
						case 2:
							EventLineDebugger.use_breakpoint = X.Nm(array[i], 0f, true) != 0f;
							break;
						}
					}
				}
			}
			catch
			{
			}
		}

		public static void saveLineDebuggerData()
		{
			if (EventLineDebugger.need_save)
			{
				EventLineDebugger.need_save = false;
				string text = string.Concat(new string[]
				{
					EventLineDebugger.max_delay.ToString(),
					"\n",
					EventLineDebugger.use_if_inject ? "1" : "0",
					"\n",
					EventLineDebugger.use_breakpoint ? "1" : "0"
				});
				PlayerPrefs.SetString("EVDEBUGGER_LINE_DAT", text);
				IN.save_prefs = true;
			}
		}

		protected override void Awake()
		{
			if (this.LiTitle != null)
			{
				return;
			}
			base.gameObject.layer = IN.gui_layer;
			base.Awake();
			this.WH(IN.w * 0.28f, IN.h);
			IN.PosP(base.gameObject, IN.wh - base.get_swidth_px() * 0.5f, 0f, -8.5f);
			base.Small();
			base.bgcol = C32.d2c(2855613749U);
			this.radius = 12f;
			this.margin_in_lr = 12f;
			this.margin_in_tb = 18f;
			base.item_margin_y_px = 6f;
			base.item_margin_x_px = 8f;
			base.alignx = ALIGN.CENTER;
			this.init();
			this.LiTitle = this.addInput(new DsnDataInput
			{
				label = "EV:",
				bounds_w = base.use_w,
				editable = false
			});
			base.Br();
			float num = base.use_h - 190f;
			this.TabLines = this.addTab("tablines", base.use_w, num, base.use_w, num, true);
			this.TabLines.stencil_ref = 20;
			this.TabLines.margin_in_lr = (this.TabLines.margin_in_tb = 0f);
			this.TabLines.item_margin_x_px = 0f;
			this.TabLines.item_margin_y_px = 3f;
			this.TabLines.scrolling_margin_in_lr = 10f;
			this.TabLines.scrolling_margin_in_tb = 16f;
			this.TabLines.init();
			base.endTab(true);
			base.Br();
			Designer designer = this.addTab("pad", base.use_w - 60f, base.use_h - 20f, base.use_w - 60f, base.use_h - 20f, false);
			designer.Smallest();
			designer.item_margin_y_px = 6f;
			designer.item_margin_x_px = 8f;
			designer.alignx = ALIGN.CENTER;
			EventLineDebugger.prepareBasicPad(designer, new FnBtnBindings(this.fnClickPadBtn), true);
			base.endTab(true);
			IN.setZ(designer.transform, -0.25f);
			this.DsFlag = IN.CreateGob(base.gameObject, "-FlagShow").AddComponent<Designer>();
			this.DsFlag.radius = 12f;
			this.DsFlag.item_margin_x_px = 3f;
			this.DsFlag.item_margin_y_px = 5f;
			this.DsFlag.margin_in_lr = 2f;
			this.DsFlag.margin_in_tb = 3f;
			this.DsFlag.animate_maxt = 0;
			this.DsFlag.use_scroll = true;
			this.DsFlag.bgcol = MTRX.cola.Black().setA1(0.87f).C;
			this.DsFlag.w = 430f;
			this.DsFlag.h = IN.h;
			this.DsFlag.stencil_ref = 241;
			IN.PosP(this.DsFlag.transform, -this.w * 0.5f - this.DsFlag.w * 0.5f, 0f, 0f);
			this.DsFlag.gameObject.SetActive(false);
			this.ERCache = new CsvReader(null, null, false);
			this.ALineEntry = new List<EventLineDebugger.EvLine>(64);
		}

		public void finePausePad()
		{
			Designer tab = base.getTab("pad");
			if (tab != null)
			{
				BtnContainerRunner btnContainerRunner = tab.Get("padbtn", false) as BtnContainerRunner;
				if (btnContainerRunner != null)
				{
					btnContainerRunner.setValue(EventLineDebugger.is_pause ? "2" : "1");
				}
			}
		}

		public static void prepareBasicPad(Designer Ds, FnBtnBindings FnClick, bool add_playing_pad = false)
		{
			string[] array2;
			if (!add_playing_pad)
			{
				string[] array = new string[2];
				array[0] = "play";
				array2 = array;
				array[1] = "pause";
			}
			else
			{
				string[] array3 = new string[4];
				array3[0] = "play";
				array3[1] = "pause";
				array3[2] = "arrow_r";
				array2 = array3;
				array3[3] = "close";
			}
			string[] array4 = array2;
			Ds.addButtonMulti(new DsnDataButtonMulti
			{
				unselectable = 2,
				skin = "mini",
				name = "padbtn",
				titles = array4,
				w = 30f,
				h = 30f,
				margin_w = 4f,
				margin_h = 0f,
				clms = array4.Length,
				fnClick = (FnClick ?? new FnBtnBindings(EventLineDebugger.fnClickPadBtnBasic))
			}).Get(EventLineDebugger.is_pause ? 1 : 0).SetChecked(true, true);
			Ds.Br().addButton(new DsnDataButton
			{
				unselectable = 2,
				w = X.Mx(130f, Ds.use_w),
				h = 30f,
				title = "&&Debug_inject_if",
				name = "Debug_inject_if",
				skin = "checkbox_string",
				def = EventLineDebugger.use_if_inject,
				fnClick = (FnClick ?? new FnBtnBindings(EventLineDebugger.fnClickPadBtnBasic))
			});
			Ds.Br().addButton(new DsnDataButton
			{
				unselectable = 2,
				w = X.Mx(130f, Ds.use_w),
				h = 30f,
				title = "&&Debug_use_breakpoint",
				name = "Debug_use_breakpoint",
				skin = "checkbox_string",
				def = EventLineDebugger.use_breakpoint,
				fnClick = (FnClick ?? new FnBtnBindings(EventLineDebugger.fnClickPadBtnBasic))
			});
			Ds.Br().addSlider(new DsnDataSlider
			{
				unselectable = 2,
				name = "max_delay",
				title = "Speed",
				def = (float)(60 - EventLineDebugger.max_delay),
				mn = 0f,
				mx = 60f,
				valintv = 5f,
				w = 90f,
				h = 30f,
				fnChanged = new aBtnMeter.FnMeterBindings(EventLineDebugger.fnChangedDelayMeter)
			});
			Ds.addP(new DsnDataP("", false)
			{
				text = "[V]変数表示\n[F]GF表示",
				TxCol = MTRX.ColWhite,
				swidth = Ds.use_w - 4f,
				sheight = 30f,
				size = 14f,
				text_auto_wrap = true,
				alignx = ALIGN.CENTER
			}, false);
		}

		public bool evLineCache(EvReader ERSrc)
		{
			base.gameObject.SetActive(true);
			if (this.LiTitle == null)
			{
				this.Awake();
			}
			if (this.cached_line_ev == ERSrc.name)
			{
				return false;
			}
			this.cached_line_ev = ERSrc.name;
			CsvReader csvReader = this.ERCache.CopyDataFrom(ERSrc);
			bool no_undef_error = csvReader.VarCon.no_undef_error;
			csvReader.no_write_varcon = 2;
			csvReader.VarCon.no_undef_error = true;
			csvReader.skip_debug_cmd = false;
			this.el_cur_line = X.Mx(0, csvReader.get_cur_line());
			csvReader.seek_set(0);
			this.LiTitle.setValue(ERSrc.name);
			this.TabLines.Clear();
			this.ALineEntry.Clear();
			this.PreFocusedEntry = null;
			this.line_stepover = -1;
			Designer tabLines = this.TabLines;
			bool flag = false;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				while (csvReader.read())
				{
					if (csvReader.getLastStr() == "")
					{
						if (!flag)
						{
							flag = true;
							tabLines.add(EventLineDebugger.DDhrLines);
							tabLines.Br();
						}
					}
					else
					{
						flag = false;
						Designer designer = tabLines.addTab(csvReader.get_cur_line().ToString(), tabLines.use_w, 30f, tabLines.use_w, 30f, false);
						EventLineDebugger.EvLine evLine = new EventLineDebugger.EvLine(designer, csvReader, this.ALineEntry.Count);
						this.ALineEntry.Add(evLine);
						designer.Smallest();
						designer.no_write_mask = true;
						designer.margin_in_lr = 5f;
						designer.margin_in_tb = 3f;
						designer.item_margin_y_px = 3f;
						designer.item_margin_x_px = 8f;
						designer.init();
						designer.addButtonT<aBtnEv>(new DsnDataButton
						{
							title = X.spr0(evLine.line + 1, 3, ' '),
							w = 28f,
							h = 30f,
							skin = "linedebug",
							fnClick = new FnBtnBindings(this.fnClickLineJump),
							unselectable = 2
						});
						uint num = uint.MaxValue;
						uint num2 = 0U;
						List<string> list = null;
						string text = csvReader.getLastStr();
						bool flag2 = true;
						while (flag2)
						{
							flag2 = false;
							blist.Clear();
							for (int i = EventLineDebugger.AFlagCheck.Count - 1; i >= 0; i--)
							{
								if (REG.match(text, EventLineDebugger.AFlagCheck[i]))
								{
									flag2 = true;
									list = blist;
									text = REG.leftContext + "\n\n" + REG.rightContext;
									list.Add(EventLineDebugger.AFnFlagReplace[i]());
								}
							}
						}
						text = csvReader.getLastStr();
						if (text.IndexOf("//") == 0)
						{
							num = 4286940549U;
						}
						else
						{
							int num3 = 0;
							string text2;
							for (;;)
							{
								if (csvReader.cmd == "TL" && csvReader.clength >= 2)
								{
									text2 = "<font color=\"ff:#7C62FD\"><u>TL " + TX.escapeSlash(csvReader._1) + "</u></font>";
									csvReader.shiftEmpty(2, 0);
								}
								else
								{
									if (!(csvReader.cmd == "MTL"))
									{
										break;
									}
									csvReader.shiftEmpty(1, 0);
								}
							}
							string cmd = csvReader.cmd;
							if (cmd == "MSG")
							{
								text2 = "<font color=\"ff:#50FF6D\"><b>MSG</b></font>";
								num3++;
								blist.Clear();
								EV.getMessageContainer().getLoadLineCount(ERSrc, csvReader, blist, true);
								if (blist.Count > 0)
								{
									list = blist;
								}
								else
								{
									list = blist;
									list.Add(null);
									list.Add("<font color=\"ff:#B30039\"><b><img mesh=\"alert\" tx_color /> Unknown MSG Resource </b></font>");
								}
							}
							else if (TX.isStart(cmd, "TALKER", 0) || cmd == "HKDS")
							{
								text2 = "<font color=\"ff:#FFB47A\"><b>" + cmd + "</b></font>";
								num3++;
							}
							else if (TX.isStart(cmd, "PIC", 0))
							{
								text2 = "<font color=\"ff:#8BB6FF\"><b>" + cmd + "</b></font>";
								num3++;
							}
							else if (TX.isStart(cmd, "@", 0) || TX.isStart(cmd, "CHANGE_EVENT", 0) || cmd == "MODULE")
							{
								text2 = "<font color=\"ff:#FF75AC\"><b><i>" + cmd + "</i></b></font>";
								num3++;
							}
							else if (cmd == "DEBUG" || cmd.IndexOf("<") == 0)
							{
								num = 4282992969U;
								num2 = 2582054149U;
								text2 = TX.escapeSlash(cmd);
								num3++;
							}
							else
							{
								bool flag3 = false;
								int num4 = csvReader.get_cur_line();
								EV.listenerCacheRead(ERSrc, cmd, csvReader, ref flag3);
								if (num4 < csvReader.get_cur_line())
								{
									int cur_line = csvReader.get_cur_line();
									csvReader.seek_set(++num4);
									using (STB stb = TX.PopBld(null, 0))
									{
										list = blist;
										blist.Clear();
										int num5 = num4;
										while (num5 < cur_line && csvReader.readCorrectly())
										{
											stb.Append(csvReader.getLastStr(), "\n");
											num5++;
										}
										blist.Add(stb.ToString());
									}
								}
								text2 = "<font color=\"ff:#B98AB1\"><b>" + cmd + "</b></font>";
								num3++;
							}
							text = text2;
							int clength = csvReader.clength;
							for (int j = num3; j < clength; j++)
							{
								text = text + " " + TX.escapeSlash(csvReader.getIndex(j));
							}
						}
						designer.addP(new DsnDataP("", false)
						{
							TxCol = C32.d2c(num),
							Col = C32.d2c(num2),
							text = text,
							swidth = designer.use_w - 19f,
							html = true,
							size = 15f,
							text_auto_wrap = true,
							alignx = ALIGN.LEFT
						}, false);
						if (list != null && list.Count > 0)
						{
							int k = 0;
							bool flag4 = false;
							if (list[0] == null)
							{
								k = 1;
								flag4 = true;
							}
							int count = list.Count;
							while (k < count)
							{
								designer.Br().XSh(20f);
								designer.addP(new DsnDataP("", false)
								{
									html = flag4,
									text = list[k],
									size = 13f,
									swidth = designer.use_w - 18f,
									TxCol = C32.d2c(4283256141U),
									Col = C32.d2c(1725487320U),
									text_auto_condense = true,
									text_auto_wrap = true,
									alignx = ALIGN.LEFT
								}, false);
								if (k < count - 1)
								{
									designer.Br().addHr(new DsnDataHr
									{
										line_height = 1f,
										margin_t = 5f,
										margin_b = 5f
									});
								}
								k++;
							}
						}
						tabLines.endTab(true);
						IN.setZ(designer.transform, -0.004f);
						tabLines.Br();
						if (this.PreFocusedEntry == null && evLine.line >= this.el_cur_line)
						{
							this.PreFocusedEntry = evLine.Focus();
						}
					}
				}
			}
			csvReader.VarCon.no_undef_error = no_undef_error;
			csvReader.skip_debug_cmd = true;
			if (this.PreFocusedEntry != null)
			{
				this.TabLines.reveal(this.PreFocusedEntry.transform, 0f, 0f, true);
				this.delay = IN.totalframe + X.Mx(20, EventLineDebugger.max_delay);
			}
			return true;
		}

		public void evLineRelease()
		{
			this.cached_line_ev = null;
			if (this.PreFocusedEntry != null)
			{
				for (int i = this.PreFocusedEntry.index; i >= 0; i--)
				{
					EventLineDebugger.EvLine evLine = this.ALineEntry[i];
					if (!evLine.focused)
					{
						break;
					}
					evLine.Blur();
				}
			}
			this.PreFocusedEntry = null;
		}

		public bool evLineCanProgress(EvReader ER)
		{
			if (ER == null || ER.name != this.cached_line_ev)
			{
				return true;
			}
			if (EventLineDebugger.is_pause)
			{
				this.delay = IN.totalframe + X.Mx(20, EventLineDebugger.max_delay);
				if (this.line_stepover == this.el_cur_line)
				{
					this.line_stepover = -2;
					return true;
				}
			}
			if (ER.get_cur_line() > this.el_cur_line)
			{
				int i = 0;
				if (this.PreFocusedEntry != null)
				{
					for (i = this.PreFocusedEntry.index; i >= 0; i--)
					{
						EventLineDebugger.EvLine evLine = this.ALineEntry[i];
						if (!evLine.focused)
						{
							break;
						}
						evLine.Blur();
					}
					i = this.PreFocusedEntry.index + 1;
					this.PreFocusedEntry = null;
				}
				int count = this.ALineEntry.Count;
				int num = this.el_cur_line;
				this.el_cur_line = ER.get_cur_line();
				if (num >= 0)
				{
					while (i < count)
					{
						EventLineDebugger.EvLine evLine2 = this.ALineEntry[i];
						if (evLine2.line > this.el_cur_line)
						{
							break;
						}
						this.PreFocusedEntry = evLine2.Focus();
						i++;
					}
					if (base.gameObject.activeSelf)
					{
						this.delay = X.Mx(this.delay, IN.totalframe + X.Mx(1, EventLineDebugger.max_delay));
					}
				}
				else
				{
					for (i = 0; i < count; i++)
					{
						if (this.ALineEntry[i].line > this.el_cur_line)
						{
							this.PreFocusedEntry = this.ALineEntry[X.Mx(i - 1, 0)].Focus();
							break;
						}
					}
					if (base.gameObject.activeSelf)
					{
						this.delay = X.Mx(this.delay, IN.totalframe + X.Mx(EventLineDebugger.max_delay, 60));
					}
				}
				if (this.PreFocusedEntry != null && !this.TabLines.isShowingOnScrollBox(this.PreFocusedEntry.Row, 0f, -30f))
				{
					this.TabLines.reveal(this.PreFocusedEntry.transform, 0f, 0f, true);
				}
				this.line_stepover = -1;
			}
			return !EventLineDebugger.is_pause && (this.line_stepover == this.el_cur_line || this.delay <= IN.totalframe);
		}

		public bool BreakPointConsidered(EvReader ER)
		{
			base.gameObject.SetActive(true);
			if (ER.name != this.cached_line_ev)
			{
				this.evLineCache(ER);
			}
			if (!EventLineDebugger.is_pause)
			{
				EventLineDebugger.is_pause = true;
				this.finePausePad();
			}
			this.line_stepover = -1;
			this.delay = IN.totalframe + X.Mx(20, EventLineDebugger.max_delay);
			return true;
		}

		public override bool run(float fcnt)
		{
			base.run(fcnt);
			if (X.D && this.cached_line_ev != null && this.PreFocusedEntry != null)
			{
				for (int i = this.PreFocusedEntry.index; i >= 0; i--)
				{
					EventLineDebugger.EvLine evLine = this.ALineEntry[i];
					if (!evLine.focused)
					{
						break;
					}
					evLine.Focus();
				}
			}
			if (IN.getK(Key.F, -1))
			{
				if (this.flagshow != EventLineDebugger.FLAGSHOW.GF)
				{
					this.flagshow = EventLineDebugger.FLAGSHOW.GF;
					this.DsFlag.gameObject.SetActive(true);
					this.DsFlag.Clear();
					this.DsFlag.init();
					this.DsFlag.add(new DsnDataP("", false)
					{
						name = "gfc_data",
						swidth = this.DsFlag.use_w,
						text_auto_condense = true,
						size = 13f,
						html = true,
						TxCol = MTRX.ColWhite,
						text = GF.getDebugStringForTextRenderer()
					});
				}
			}
			else if (IN.getK(Key.V, -1))
			{
				if (this.flagshow != EventLineDebugger.FLAGSHOW.VARCON)
				{
					this.flagshow = EventLineDebugger.FLAGSHOW.VARCON;
					this.DsFlag.gameObject.SetActive(true);
					this.DsFlag.Clear();
					this.DsFlag.init();
					EV.getVariableContainer().show(this.DsFlag);
				}
			}
			else if (this.flagshow != EventLineDebugger.FLAGSHOW.NONE)
			{
				this.flagshow = EventLineDebugger.FLAGSHOW.NONE;
				this.DsFlag.gameObject.SetActive(false);
			}
			if (!EV.isActive(false) && this.cached_line_ev == null)
			{
				EV.getDebugger().changeActivate(true);
				base.gameObject.SetActive(false);
				this.flagshow = EventLineDebugger.FLAGSHOW.NONE;
				this.DsFlag.gameObject.SetActive(false);
			}
			return true;
		}

		public bool isKettei()
		{
			return this.line_stepover == this.el_cur_line;
		}

		public static bool fnClickPadBtnBasic(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "play"))
				{
					if (!(title == "pause"))
					{
						if (!(title == "&&Debug_inject_if"))
						{
							if (title == "&&Debug_use_breakpoint")
							{
								B.SetChecked(!B.isChecked(), true);
								EventLineDebugger.use_breakpoint = B.isChecked();
								EventLineDebugger.need_save = true;
							}
						}
						else
						{
							B.SetChecked(!B.isChecked(), true);
							EventLineDebugger.use_if_inject = B.isChecked();
							EventLineDebugger.need_save = true;
						}
					}
					else if (!B.isChecked())
					{
						B.Container.setValue("2");
						EventLineDebugger.is_pause = true;
					}
				}
				else if (!B.isChecked())
				{
					B.Container.setValue("1");
					EventLineDebugger.is_pause = false;
				}
			}
			return true;
		}

		public bool fnClickPadBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "close"))
				{
					if (title == "arrow_r")
					{
						EvReader currentEvent = EV.getCurrentEvent();
						if (currentEvent != null && currentEvent.name == this.cached_line_ev)
						{
							this.line_stepover = this.el_cur_line;
							return true;
						}
						return true;
					}
				}
				else
				{
					if (EV.isActive(false))
					{
						EV.unstackReaderAllForcibly();
						return true;
					}
					return true;
				}
			}
			EventLineDebugger.fnClickPadBtnBasic(B);
			return true;
		}

		public static bool fnChangedDelayMeter(aBtnMeter _B, float pre_value, float cur_value)
		{
			EventLineDebugger.max_delay = X.IntR(60f - cur_value);
			EventLineDebugger.need_save = true;
			return true;
		}

		private bool fnClickLineJump(aBtn B)
		{
			int num = X.NmI(B.title, -1, true, false);
			if (num >= 0)
			{
				this.el_cur_line = (this.line_stepover = -1);
				EV.getCurrentEvent().seek_set(num - 1);
			}
			return true;
		}

		public bool isEventReading()
		{
			return this.cached_line_ev != null;
		}

		private string cached_line_ev;

		private int el_cur_line;

		private EventLineDebugger.EvLine PreFocusedEntry;

		private int el_wait;

		private int delay;

		private LabeledInputField LiTitle;

		private Designer TabLines;

		private List<EventLineDebugger.EvLine> ALineEntry;

		private EventLineDebugger.FLAGSHOW flagshow;

		private Designer DsFlag;

		private CsvReader ERCache;

		private const int MAX_DELAY_MAX = 60;

		private static DsnDataHr DDhrLines = new DsnDataHr
		{
			margin_t = 8f,
			margin_b = 8f,
			line_height = 0f
		};

		private static bool is_pause = false;

		public static bool use_if_inject = false;

		public static bool use_breakpoint = false;

		private static int max_delay = 10;

		private int line_stepover = -1;

		private static bool need_save;

		public static List<Regex> AFlagCheck = new List<Regex>(new Regex[] { GF.RegGF });

		public static List<REG.FnGetReplacedString> AFnFlagReplace = new List<REG.FnGetReplacedString>(new REG.FnGetReplacedString[]
		{
			delegate
			{
				string text = string.Concat(new string[]
				{
					"GF",
					REG.R1,
					"[",
					REG.R2,
					"]"
				});
				return text + " = " + GF.TxEvalContentForDebug(text);
			}
		});

		private sealed class EvLine
		{
			public EvLine(Designer _Row, CsvReader ER, int _index)
			{
				this.Row = _Row;
				this.line = ER.get_cur_line();
				this.index = _index;
				this.line_str = ER.getLastStr();
				this.Blur();
			}

			public EventLineDebugger.EvLine Focus()
			{
				this.Row.bgcol = C32.MulA(2583668048U, 0.6f + 0.4f * X.COSIT(75f));
				this.focused = true;
				return this;
			}

			public EventLineDebugger.EvLine Blur()
			{
				this.Row.bgcol = C32.d2c(2284069924U);
				this.focused = false;
				return this;
			}

			public Transform transform
			{
				get
				{
					return this.Row.transform;
				}
			}

			public override string ToString()
			{
				return string.Concat(new string[]
				{
					"EvLine:",
					this.line.ToString(),
					"(index:",
					this.index.ToString(),
					") - ",
					this.line_str
				});
			}

			public readonly Designer Row;

			public readonly int line;

			public readonly int index;

			public bool focused;

			public string line_str;
		}

		private enum FLAGSHOW
		{
			NONE,
			GF,
			VARCON
		}
	}
}
