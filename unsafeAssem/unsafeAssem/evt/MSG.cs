using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using XX;

namespace evt
{
	public class MSG : IMessageContainer
	{
		public MSG(GameObject par)
		{
			this.GobBase = IN.CreateGob(par, "-MSG");
			this.setPosition(0f);
			this.MMRD = this.GobBase.AddComponent<MultiMeshRenderer>();
			this.MdPop = this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
			this.GobPop = this.MMRD.GetGob(this.MdPop);
			this.MdaPop = new MdArranger(this.MdPop);
			this.MdArrow = this.MMRD.Make(BLEND.NORMAL, MTRX.MIicon);
			this.MdArrow.RotaPF(0f, 0f, 1f, 1f, 0f, MTRX.getPF("msg_arrow"), false, false, false, uint.MaxValue, false, 0);
			this.MdArrow.updateForMeshRenderer(false);
			this.MrdArrow = this.MMRD.GetMeshRenderer(this.MdArrow);
			this.TrsArrow = this.MMRD.GetGob(this.MdArrow).transform;
			this.MdFrame = this.MMRD.Make(BLEND.NORMAL, MTRX.MIicon);
			this.TalkerBox = new MsgTalkerViewer(this);
			this.ALine = new MsgLine[4];
			this.LineSub = new MsgLine(this, null).noMakeMode();
			this.clear();
		}

		public void destructGob()
		{
			this.clearLines();
		}

		public void fineTargetFont()
		{
			this.clearLines();
		}

		public void clearValues()
		{
			this.commandListener = null;
			this.show_arrow = false;
			this.handle = true;
		}

		public void initEvent(EvMsgCommand CmdListener)
		{
			this.commandListener = CmdListener;
			this.pre_msg_hide = this.AUTO_HIDE_TIME_DEFAULT;
			this.AUTO_HIDE_TIME_DEFAULT = 0;
			this.handle = true;
		}

		public void quitEvent()
		{
			this.hideMsg(false);
			this.AUTO_HIDE_TIME_DEFAULT = this.pre_msg_hide;
		}

		public MSG setPosition(float w = -1000f)
		{
			Vector3 localPosition = this.GobBase.transform.localPosition;
			if (w != -1000f)
			{
				localPosition.x = (this.basex = w * 0.015625f);
			}
			localPosition.y = (float)(-(float)EV.psh / 2 + MSG.MSG_SHIFT_Y + 106 + MSG.taleheight) * 0.015625f;
			this.GobBase.transform.localPosition = localPosition;
			return this;
		}

		public MSG reattach(GameObject par)
		{
			this.GobBase.transform.SetParent(par.transform);
			return this;
		}

		private void activate()
		{
			this.clear();
			this.af = 0f;
			this.firstShowTime = 22f;
			this.GobBase.SetActive(true);
			this.MMRD.GetMeshRenderer(this.MdPop).enabled = true;
		}

		public MSG clear()
		{
			this.af = (float)(-(float)this.hiding_time);
			this.popaf = 0f;
			this.msgf = 0f;
			this.msgfinf = -1f;
			this.p_hidef = -1f;
			this.popRefineFlag = true;
			this.alpha_changed_flag = false;
			this.clearLines();
			this.show_arrow = false;
			this.hkds_think = false;
			this.MMRD.deactivate(false);
			this.GobBase.SetActive(false);
			return this;
		}

		public MSG tempVisible(bool _f)
		{
			if (this.af >= 0f)
			{
				this.GobBase.SetActive(_f);
			}
			return this;
		}

		public MSG clearLines()
		{
			for (int i = 0; i < this.ALine.Length; i++)
			{
				try
				{
					if (this.ALine[i] != null)
					{
						this.ALine[i].destruct();
					}
				}
				catch
				{
				}
			}
			X.ALLN<MsgLine>(this.ALine);
			this.linemax = 0;
			this.lastline = 0;
			this.flushline = 0;
			return this;
		}

		public int showImmediate(bool show_only_first_char = false, bool no_snd = false)
		{
			if (this.linemax == 0)
			{
				return 0;
			}
			int num = 0;
			if (this.flushline > 0)
			{
				for (int i = 0; i < X.Mn(this.linemax, this.flushline); i++)
				{
					this.ALine[0].destruct();
					X.shiftEmpty<MsgLine>(this.ALine, 1, 0, -1);
					num++;
				}
			}
			else
			{
				int num2 = this.linemax - 2;
				if (num2 > 0)
				{
					for (int i = 0; i < num2; i++)
					{
						this.ALine[0].destruct();
						X.shiftEmpty<MsgLine>(this.ALine, 1, 0, -1);
						num++;
					}
				}
			}
			if (num > 0)
			{
				this.lastline = X.Mx(0, this.lastline - num);
				this.flushline = X.Mx(0, this.flushline - num);
				this.linemax = X.Mx(0, this.linemax - num);
				this.msgf = 0f;
				this.msgfinf = -1f;
				this.p_hidef = -1f;
			}
			else
			{
				this.p_hidef = X.Mx(0f, this.p_hidef);
			}
			int num3 = 0;
			for (int i = 0; i < this.linemax; i++)
			{
				MsgLine msgLine = this.ALine[i];
				msgLine.y = this.getCorrectLineY((float)i);
				msgLine.visible = true;
				msgLine.alpha = 1f;
				if (!show_only_first_char)
				{
					msgLine.charsAllShow();
				}
				else
				{
					num3 = msgLine.showFirstChar(num3);
					if (num3 < 0)
					{
						break;
					}
				}
			}
			this.af = X.Mx(this.af, 40f);
			this.popaf = X.Mx(this.popaf, 40f);
			this.TalkerBox.showImmediate();
			return num3;
		}

		public bool run(float fcnt, bool skipping)
		{
			int num = this.LINE_MAX;
			int num2 = this.LINE_MAX;
			if ((this.linemax > this.LINE_MAX || this.flushline != 0) && this.p_hidef >= 0f)
			{
				this.p_hidef += fcnt;
				float num3 = X.ZLINE(this.p_hidef - (float)this.LINE_STEP_TIME, (float)this.PRELINE_SCROLL_TIME / ((this.flushline != 0) ? this.FLUSH_SPEED_RATE : 1f));
				if (num3 > 0f)
				{
					num2++;
					num += this.ALine[X.Mn(this.LINE_MAX, this.linemax - 1)].nest_count;
					if (X.D)
					{
						for (int i = 0; i < this.linemax; i++)
						{
							this.ALine[i].y = this.getCorrectLineY((float)i - num3);
						}
						this.ALine[0].alpha = (1f - num3) * ((this.flushline != 0) ? 0.5f : 1f);
						MsgLine msgLine = this.ALine[X.Mn(this.LINE_MAX, this.linemax - 1)];
						msgLine.alpha = num3 * ((2 < this.flushline) ? 0.5f : 1f);
						if (!msgLine.visible)
						{
							msgLine.visible = true;
						}
					}
					if (num3 >= 1f)
					{
						this.p_hidef = -1f;
						this.ALine[0].destruct();
						X.shiftEmpty<MsgLine>(this.ALine, 1, 0, -1);
						this.linemax--;
						if (this.lastline != 0)
						{
							this.lastline--;
						}
						num2--;
						num--;
						if (this.flushline != 0)
						{
							int num4 = this.flushline - 1;
							this.flushline = num4;
							if (num4 != 0)
							{
								num = (num2 = this.LINE_MAX - 1);
								this.p_hidef = (float)this.LINE_STEP_TIME;
							}
						}
					}
					else if (this.flushline != 0)
					{
						num = (num2 = this.LINE_MAX);
					}
				}
			}
			else if (X.BTW(0f, this.p_hidef, (float)this.LINE_STEP_TIME))
			{
				this.p_hidef += 1f;
			}
			if (this.af >= 0f)
			{
				this.af += fcnt;
				this.popaf += fcnt;
				this.msgf += fcnt;
				if (X.D)
				{
					float num5 = 1f;
					if (this.af <= 60f)
					{
						num5 = X.ZLINE(this.af, 40f);
					}
					if (this.popaf <= 60f)
					{
						float num6 = this.popaf / 20f;
						num5 *= X.ZLINE(num6 * 2f);
						Vector3 localScale = this.GobPop.transform.localScale;
						localScale.x = (localScale.y = 1.08f * X.ZSIN2(this.popaf, 8f) - 0.08f * X.ZCOS((this.popaf - 7f) / 26f));
						this.GobPop.transform.localScale = localScale;
					}
					this.alpha = num5;
					if (this.alpha_changed_flag)
					{
						this.setPopAlpha(this.alpha_, false);
					}
				}
				this.TalkerBox.runDraw(X.D);
				if (this.charsRunDraw(1, num, num2))
				{
					if (this.p_hidef < 0f)
					{
						this.p_hidef = 0f;
					}
					if (num >= this.linemax)
					{
						this.msgfinf = X.Mx(this.msgfinf, 0f);
					}
				}
				if (this.msgfinf >= 0f && this.GobBase.activeSelf)
				{
					if (this.show_arrow && this.can_progress)
					{
						this.MrdArrow.enabled = true;
						Vector3 localPosition = this.TrsArrow.localPosition;
						localPosition.y = (float)(-(float)X.ANM(IN.totalframe, 2, 15f) * 4 - MSG.taleheight - 106 + 2) * 0.015625f;
						this.TrsArrow.localPosition = localPosition;
					}
					this.msgfinf += fcnt;
				}
				if (this.auto_hide_time > 0 && this.msgfinf > (float)X.Abs(this.auto_hide_time))
				{
					this.hideMsg(false);
				}
				return true;
			}
			if (this.af <= (float)(-(float)this.hiding_time))
			{
				return false;
			}
			this.af -= fcnt;
			this.charsRunDraw(1, num, num2);
			if (X.D)
			{
				this.alpha = 1f - -this.af / (float)this.hiding_time;
			}
			this.TalkerBox.runDraw(X.D);
			if (this.alpha_changed_flag)
			{
				this.setPopAlpha(this.alpha_, false);
			}
			if (this.af <= (float)(-(float)this.hiding_time))
			{
				this.clear();
				return false;
			}
			return true;
		}

		public int makeFromEvent(StringHolder rER, EvReader curEv)
		{
			string _ = rER._1;
			TalkDrawer talkerDrawer = EV.getTalkerDrawer(_);
			bool b = rER._B2;
			int num = (int)rER.Nm(3, -1000f);
			if (curEv.restLines() < 2)
			{
				return -1;
			}
			Vector3 vector = ((talkerDrawer != null) ? talkerDrawer.getHkdsDepertPos() : new Vector3(-1000f, -1000f, 1f));
			this.flush(false, false).make(curEv.progLines(2), num, (int)vector.x, (talkerDrawer != null) ? talkerDrawer.getPersonMsgKey() : _);
			EvPerson person = EvPerson.getPerson(_, null);
			if (person != null)
			{
				this.talker_snd = person.talk_snd;
			}
			this.show_arrow = true;
			if (b)
			{
				this.commandListener.time_shift(this.showImmediate(true, false));
			}
			return 1;
		}

		public void make(string _str, int _auto_hide_time = -1000, int pt = -1000, string _talker = "")
		{
			MsgLine.fineFontStorage(this.ALine, this.linemax, _str);
			if (this.af < 0f)
			{
				this.activate();
				if (_auto_hide_time < -1000)
				{
					_auto_hide_time = -1000;
				}
			}
			else
			{
				this.firstShowTime = ((this.linemax == 1) ? (this.ALine[0].showt + 10f) : 12f);
			}
			bool flag = _talker != "" && _str.Length > 0 && MSG.RegThink.Match(_str).Success;
			this.firstShowTime = (float)((int)(this.firstShowTime * this.first_show_speed));
			if ((float)pt != this.basex || this.hkds_think != flag || this.TalkerBox.needChange(_talker))
			{
				this.basex = (float)pt;
				this.hkds_think = flag;
				this.popaf = 0f;
				this.TalkerBox.hideImmediate();
				this.popRefineFlag = true;
				this.flush(false, false);
			}
			if (this.popRefineFlag)
			{
				this.talker_snd = "talk_progress";
				MSG.taleheight = (this.hkds_think ? MSG.TALEHEIGHT_THINK : MSG.TALEHEIGHT_NORMAL);
				this.makeHukidasi();
				this.popaf = 0f;
				this.TalkerBox.setTalker(_talker, (this.basex == -1000f) ? 0f : this.basex);
				this.popRefineFlag = false;
				this.MMRD.GetMeshRenderer(this.MdFrame).enabled = !this.TalkerBox.isActive();
			}
			if (this.hkds_think && _str.Length > 0 && !TX.charIs(_str, _str.Length - 1, '）'))
			{
				_str = TX.linePrefix(TX.appendLast(_str, "）"), "\u3000", 1, 0);
			}
			this.msgf = 0f;
			this.msgfinf = -1f;
			if (_auto_hide_time >= -1000)
			{
				this.auto_hide_time = ((_auto_hide_time == -1000) ? this.AUTO_HIDE_TIME_DEFAULT : _auto_hide_time);
			}
			MsgLine msgLine = null;
			this.lastline = this.linemax;
			while (_str != "")
			{
				MsgLine msgLine2 = new MsgLine(this, msgLine);
				if (this.hkds_think)
				{
					msgLine2.default_color = MSG.color_think;
					if (msgLine == null)
					{
						msgLine2.color = MSG.color_think;
					}
				}
				msgLine2.commandListener = this.commandListener;
				msgLine2.y = this.getCorrectLineY((float)this.linemax);
				msgLine2.visible = this.linemax < this.LINE_MAX;
				_str = msgLine2.setText(_str);
				if (msgLine2.msx > 0f)
				{
					X.pushToEmptyS<MsgLine>(ref this.ALine, msgLine2.activate(), ref this.linemax, 4);
				}
				msgLine = msgLine2;
			}
			for (int i = this.lastline; i < this.linemax; i++)
			{
				this.ALine[i].nest_count = this.linemax - i;
			}
			this.MrdArrow.enabled = false;
		}

		public float getCorrectLineY(float _l = 0f)
		{
			return (float)(-(float)MSG.taleheight - 23) - (_l + 0.5f) * 31f;
		}

		private void makeHukidasi()
		{
			this.setPosition(-1000f);
			float num = ((this.basex > -1000f) ? this.basex : 0f);
			this.MdPop.clear(false, false);
			this.MdPop.Col = this.ColTop;
			this.MdaPop.Set(true);
			this.MdPop.KadomaruRect(-num, (float)(-(float)MSG.taleheight) - 53f, 666f, 106f, 30f, 0f, false, 0f, 0f, false);
			if (this.basex > -1000f)
			{
				float num2 = X.absMx(-this.basex * 0.3f, X.Abs(this.basex) - 333f + 100f);
				int num3 = 300;
				float num4 = (float)MSG.taleheight * this.basex / (float)num3;
				if (!this.hkds_think)
				{
					this.MdPop.Tri012();
					this.MdPop.PosD(num2 + num4, 0f, null);
					this.MdPop.PosD(num2 + 14f, (float)(-(float)MSG.taleheight), null);
					this.MdPop.PosD(num2 - 14f, (float)(-(float)MSG.taleheight), null);
				}
				else
				{
					this.MdPop.Arc(num2, (float)(-(float)MSG.taleheight), 17f, 0f, 3.1415927f, 0f);
					this.MdPop.Circle(num2 + num4 * 0.8f * 0.5f, (float)(-(float)MSG.taleheight) * 0.1f, 11f, 0f, false, 0f, 0f);
					this.MdPop.Circle(num2 + num4 * 0.8f * 0.9f, (float)MSG.taleheight * 0.8f, 6f, 0f, false, 0f, 0f);
				}
			}
			this.MdaPop.Set(false);
			Vector3 vector = this.GobPop.transform.localScale;
			vector.x = (vector.y = 0f);
			this.GobPop.transform.localScale = vector;
			vector = this.GobPop.transform.localPosition;
			vector.x = num * 0.015625f;
			this.GobPop.transform.localPosition = vector;
			this.setPopAlpha(0f, true);
		}

		private void setPopAlpha(float _alpha, bool first = false)
		{
			this.alpha_ = _alpha;
			_alpha *= 0.85f;
			if (first)
			{
				Color32 c = MTRX.cola.Set(this.ColBottom).setA1(_alpha).C;
				Color32 c2 = MTRX.cola.Set(this.ColTop).setA1(_alpha).C;
				this.MdaPop.setColAllGrdation((float)(-(float)MSG.taleheight - 106), (float)(-(float)MSG.taleheight), c, c2, GRD.BOTTOM2TOP, false, false);
			}
			else
			{
				this.MdaPop.setAlpha1(_alpha, false);
			}
			for (int i = 0; i < this.linemax; i++)
			{
				this.ALine[i].remakeCharMesh(false);
			}
			this.alpha_changed_flag = false;
			this.MdPop.updateForMeshRenderer(true);
			if (!this.TalkerBox.isActive())
			{
				this.MdFrame.Col = MTRX.cola.Set(MSG.AmsgColors[0]).setA1(_alpha * 0.4f).C;
				float num = 0.8f + X.ZSIN2((this.af < 0f) ? this.alpha_ : (this.alpha_ * 2f)) * 0.2f;
				float num2 = 670f * num;
				float num3 = 108f * num;
				MTRX.MsgFrame3.DrawTo(this.MdFrame, 0f, (float)(-(float)MSG.taleheight - 53), num2, num3);
				this.MdFrame.updateForMeshRenderer(false);
				return;
			}
			this.TalkerBox.fine_flag = true;
		}

		public bool isActive()
		{
			return this.af >= 0f;
		}

		public bool isShowing()
		{
			return this.af > (float)(-(float)this.hiding_time);
		}

		public MSG flush(bool immediate = false, bool for_announce = false)
		{
			if (this.linemax == 0)
			{
				return this;
			}
			if (for_announce && this.linemax <= 1)
			{
				return this;
			}
			if (immediate)
			{
				this.clearLines();
			}
			else
			{
				this.flushline = this.linemax - (for_announce ? 1 : 0);
				if (this.p_hidef < (float)this.LINE_STEP_TIME)
				{
					this.p_hidef = (float)this.LINE_STEP_TIME;
				}
			}
			return this;
		}

		public bool charsRunDraw(int fcnt, int run_line_max, int draw_line_max)
		{
			int num = X.Mn(run_line_max, this.linemax);
			bool flag = true;
			for (int i = 0; i < num; i++)
			{
				if (!this.ALine[i].runDraw(fcnt, i < draw_line_max) && i < draw_line_max)
				{
					flag = false;
				}
			}
			return flag;
		}

		public void hideMsg(bool immediate = false)
		{
			if (this.af >= 0f)
			{
				this.af = X.Mn(this.af, (float)(immediate ? (-(float)this.hiding_time) : (-1)));
			}
			this.show_arrow = false;
			this.MrdArrow.enabled = false;
			this.TalkerBox.deactivate();
		}

		public void hideMsg(StringHolder rER_without, bool immediate = false)
		{
			this.hideMsg(immediate);
		}

		public void setHandle(bool f)
		{
			this.handle = f;
		}

		public void hold()
		{
			if (!this.GobBase.activeSelf)
			{
				return;
			}
			if (this.alpha >= 0.75f && this.af < 0f)
			{
				this.af = X.Mx(50f, this.af);
				this.alpha = 1f;
			}
			else
			{
				this.af = X.Mx(0f, this.af);
			}
			this.show_arrow = false;
			this.MrdArrow.enabled = false;
			this.auto_hide_time = 0;
		}

		public MSG destruct()
		{
			this.clear();
			IN.DestroyOne(this.GobBase);
			return null;
		}

		public bool progressNextParagraph()
		{
			return false;
		}

		public void HkdsListenerMoved(IHkdsFollowable Assigned)
		{
		}

		public float get_rest_hiding_frame()
		{
			if ((this.linemax <= this.LINE_MAX && this.flushline == 0) || this.p_hidef < 0f)
			{
				return 0f;
			}
			return (float)this.LINE_STEP_TIME - this.p_hidef;
		}

		public bool isAllCharsShown()
		{
			return this.msgfinf >= 0f;
		}

		public float getAppearTime()
		{
			return this.msgf;
		}

		public MultiMeshRenderer get_MMRD()
		{
			return this.MMRD;
		}

		public bool handle
		{
			get
			{
				return this.can_progress;
			}
			set
			{
				this.can_progress = value;
			}
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					this.alpha_ = value;
					this.alpha_changed_flag = true;
				}
			}
		}

		public int getCurrentMsgKey(out string person_key, out string label)
		{
			person_key = this.TalkerBox.talker_key;
			label = "";
			return 0;
		}

		public bool isHidingTemporary()
		{
			return this.GobBase.activeSelf;
		}

		public bool checkHideVisibilityTemporary(bool hideable, bool force_change = false)
		{
			bool flag;
			if (this.GobBase.activeSelf)
			{
				flag = force_change || (hideable && IN.isMapPD(1));
			}
			else
			{
				flag = !force_change && (hideable && !IN.isMapPD(1) && !IN.isCancelPD() && !IN.isSubmitPD(1) && !IN.isLP(1) && !IN.isRP(1) && !IN.isTP(1)) && !IN.isBP(1);
			}
			if (this.GobBase.activeSelf == flag)
			{
				this.GobBase.SetActive(!flag);
				return true;
			}
			return false;
		}

		public void fineFrontConfirmer()
		{
		}

		public int getLoadLineCount(EvReader ER, CsvReader rER, List<string> Adest = null, bool no_error = false)
		{
			int num = 2;
			if (Adest != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					while (--num >= 0)
					{
						if (!ER.readCorrectly())
						{
							num = 0;
							break;
						}
						stb.Append(ER.getLastStr(), "\n");
					}
					Adest.Add(stb.ToString());
				}
			}
			return num;
		}

		public Transform transform
		{
			get
			{
				return this.GobBase.transform;
			}
		}

		private float af;

		internal float msgf;

		private float msgfinf;

		public float firstShowTime;

		public float first_show_speed = 1f;

		private float popaf;

		private bool popRefineFlag;

		private float p_hidef;

		private float basex;

		private GameObject GobBase;

		private MultiMeshRenderer MMRD;

		private MeshDrawer MdPop;

		private GameObject GobPop;

		private MdArranger MdaPop;

		private MeshDrawer MdArrow;

		private MeshRenderer MrdArrow;

		private Transform TrsArrow;

		private MeshDrawer MdFrame;

		private MsgLine[] ALine;

		private int linemax;

		private int lastline;

		private int flushline;

		private MsgTalkerViewer TalkerBox;

		private int auto_hide_time;

		private bool can_progress = true;

		private MsgLine LineSub;

		private bool hkds_think;

		private int LINE_MAX = 2;

		public static int MSG_SHIFT_Y = 40;

		public int LINE_STEP_TIME = 30;

		public int PRELINE_SCROLL_TIME = 16;

		public float FLUSH_SPEED_RATE = 2f;

		public int AUTO_HIDE_TIME_DEFAULT;

		public bool show_arrow;

		public EvMsgCommand commandListener;

		private float alpha_;

		private bool alpha_changed_flag;

		public string talker_snd = "";

		public int hiding_time = 40;

		public const int charMargin = 18;

		public const int charMargin_s = 12;

		public const int charLineheight = 31;

		public const int charSize = 17;

		public const int charSize_l = 34;

		public const int charSize_s = 14;

		internal static int taleheight = MSG.TALEHEIGHT_NORMAL;

		private static readonly int TALEHEIGHT_NORMAL = 40;

		private static readonly int TALEHEIGHT_THINK = 70;

		public const int hkds_margin_tb = 23;

		public const int hkds_margin_lr = 53;

		public const int hkds_width = 666;

		public const int hkds_height = 106;

		internal static Regex RegThink = new Regex("^(?:[Ww]\\d)*（");

		public const string talker_snd_default = "talk_progress";

		public const float pop_base_alpha = 0.85f;

		public Color32 ColTop = C32.d2c(15131611U);

		public Color32 ColBottom = C32.d2c(13421252U);

		private int pre_msg_hide;

		public static Color32[] AmsgColors = new Color32[]
		{
			C32.d2c(6181972U),
			C32.d2c(16597056U),
			C32.d2c(16749343U),
			C32.d2c(16770333U),
			C32.d2c(5619770U),
			C32.d2c(6518271U),
			C32.d2c(14048948U),
			C32.d2c(10392975U),
			C32.d2c(12892852U)
		};

		public static Color32[] AmsgColorsDark = new Color32[]
		{
			C32.d2c(16777215U),
			C32.d2c(16757940U),
			C32.d2c(16776960U),
			C32.d2c(12255156U),
			C32.d2c(11862009U),
			C32.d2c(11844351U),
			C32.d2c(16766706U),
			C32.d2c(8947848U),
			C32.d2c(4473924U)
		};

		public static Color32 color_think = C32.d2c(14478069U);
	}
}
