using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class WanderingNPC
	{
		public WanderingNPC(WanderingManager _Con, WanderingManager.TYPE _type, string _lp_name, string _meet_flag_name, float _walk_len_min = 4f, float _walk_len_max = 8f, float _catchable = 4f, float _appear_ratio_default = 0.33f, string _pxc_name = "sub_t", string _pxc_pose_name = "stand")
		{
			this.Con = _Con;
			this.type = _type;
			this.catchable = _catchable;
			this.lp_name = _lp_name;
			this.meet_flag_name = _meet_flag_name;
			this.walk_len_min = _walk_len_min;
			this.walk_len_max = _walk_len_max;
			this.pxc_name = _pxc_name;
			this.pxc_pose_name = _pxc_pose_name;
			this.appear_ratio_default = _appear_ratio_default;
			this.appear_ratio = _appear_ratio_default;
			this.Anot_exist_map = new List<string>(4);
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.Con.M2D;
			}
		}

		public WholeMapManager WM
		{
			get
			{
				return this.Con.M2D.WM;
			}
		}

		public WholeMapItem CurWM
		{
			get
			{
				return this.Con.M2D.WM.CurWM;
			}
		}

		public void flush()
		{
			this.posx = (this.posy = -1000f);
			this.decided_key = null;
			this.Anot_exist_map.Clear();
			this.walk_around_flag_ = false;
			this.appear_ratio = this.appear_ratio_default;
			this.has_bell_ = 2;
		}

		public void fineFirstPosition(bool force = false)
		{
			if (!force && this.posx >= 0f)
			{
				return;
			}
			WholeMapItem curWM = this.M2D.WM.CurWM;
			this.posx = WanderingNPC.XORSP() * (curWM.Bounds.width - 1f);
			this.posy = WanderingNPC.XORSP() * (curWM.Bounds.height - 1f);
			this.agR = WanderingNPC.XORSPS() * 3.1415927f;
			this.walk_around_flag_ = false;
		}

		private void fineHasBellOrNotMeeting()
		{
			if (this.has_bell_ < 2)
			{
				return;
			}
			if (this.FD_CalcHasBell == null || !SCN.isWNpcEnable(this.M2D, this.type))
			{
				this.has_bell_ = 0;
				return;
			}
			this.has_bell_ = (this.FD_CalcHasBell(this) ? 1 : 0);
		}

		public void walkAround()
		{
			this.walk_around_flag_ = false;
			this.decided_key = null;
			this.Anot_exist_map.Clear();
			this.fineFirstPosition(false);
			this.fineHasBellOrNotMeeting();
			WholeMapItem curWM = this.M2D.WM.CurWM;
			if (curWM == null)
			{
				return;
			}
			float num = global::XX.X.NI(this.walk_len_min, this.walk_len_max, WanderingNPC.XORSP());
			int num2 = global::XX.X.IntC(num);
			float num3 = global::XX.X.Cos(this.agR);
			float num4 = -global::XX.X.Sin(this.agR);
			float num5 = curWM.Bounds.width - 1f;
			float num6 = curWM.Bounds.height - 1f;
			if (this.has_bell_ == 1 || WanderingNPC.XORSP() < 0.25f)
			{
				WholeMapItem.WMItem wmi = curWM.GetWmi(this.M2D.curMap, null);
				if (wmi != null && !wmi.Rc.isEmpty())
				{
					this.agR = this.M2D.curMap.GAR(this.posx + curWM.Bounds.x + 0.5f, this.posy + curWM.Bounds.y + 0.5f, wmi.Rc.cx, wmi.Rc.cy);
				}
			}
			for (int i = 0; i < num2; i++)
			{
				float num7 = ((i < num2 - 1) ? 1f : (num - (float)i));
				this.posx += num3 * num7;
				this.posy += num4 * num7;
				if (num3 < 0f && this.posx < 0f)
				{
					this.posx = -this.posx;
					this.agR = WanderingNPC.XORSPS() * 3.1415927f * 0.3f;
					num3 = global::XX.X.Cos(this.agR);
					num4 = -global::XX.X.Sin(this.agR);
				}
				else if (num3 > 0f && this.posx >= num5)
				{
					this.posx = num5 - (this.posx - num5);
					this.agR = 3.1415927f + WanderingNPC.XORSPS() * 3.1415927f * 0.3f;
					num3 = global::XX.X.Cos(this.agR);
					num4 = -global::XX.X.Sin(this.agR);
				}
				else if (num4 < 0f && this.posy < 0f)
				{
					this.posy = -this.posy;
					this.agR = -1.5707964f + WanderingNPC.XORSPS() * 3.1415927f * 0.3f;
					num3 = global::XX.X.Cos(this.agR);
					num4 = -global::XX.X.Sin(this.agR);
				}
				else if (num4 > 0f && this.posy >= num6)
				{
					this.posy = num6 - (this.posy - num6);
					this.agR = 1.5707964f + WanderingNPC.XORSPS() * 3.1415927f * 0.3f;
					num3 = global::XX.X.Cos(this.agR);
					num4 = -global::XX.X.Sin(this.agR);
				}
			}
		}

		public void blurDecided(string specific_map = null)
		{
			if (TX.valid(this.decided_key) && (TX.noe(specific_map) || this.decided_key == specific_map))
			{
				this.walkAround();
			}
		}

		public void setCurrentPos(Map2d Mp, bool do_not_decide = false)
		{
			if (Mp != null)
			{
				this.decided_key = (do_not_decide ? null : Mp.key);
				this.Anot_exist_map.Remove(Mp.key);
				WholeMapItem.WMItem wmi = this.M2D.WM.CurWM.GetWmi(Mp, null);
				if (wmi != null)
				{
					this.posx = wmi.Rc.cx;
					this.posy = wmi.Rc.cy;
				}
				this.Con.blurDecidedOtherFrom(Mp, this);
			}
		}

		public bool alreadyCalcedAt(Map2d Mp)
		{
			return this.Anot_exist_map.IndexOf(Mp.key) >= 0;
		}

		public bool canAppearToThisMap(Map2d Mp)
		{
			return !this.Con.isOtherNpcAppearHere(Mp, this) && this.isEnable() && SCN.isWNpcEnableInMap(Mp, this.type);
		}

		public bool checkBench(M2Chip Bench, float appear_ratio = -1f, float len_add = 0f, bool attach_mover = true)
		{
			Map2d mp = Bench.Mp;
			bool flag = mp.key == this.decided_key;
			METACImg meta = Bench.getMeta();
			if (flag || this.Anot_exist_map.IndexOf(mp.key) < 0)
			{
				if (flag && !SCN.isWNpcEnableInMap(this.M2D.Get(this.decided_key, true), this.type))
				{
					this.Anot_exist_map.Remove(this.decided_key);
					this.decided_key = null;
					flag = false;
				}
				this.fineFirstPosition(false);
				WholeMapItem.WMItem wmitem = null;
				if (!flag && this.decided_key == null && this.canAppearToThisMap(mp))
				{
					WholeMapItem curWM = this.M2D.WM.CurWM;
					wmitem = this.M2D.WM.CurWM.GetWmi(mp, null);
					this.fineHasBellOrNotMeeting();
					len_add += (float)((GF.getC(this.meet_flag_name) == 0U) ? 4 : 0);
					float num = this.catchable;
					if (this.FD_overrideCalcableLength != null)
					{
						num = this.FD_overrideCalcableLength(num);
					}
					if (wmitem != null && !wmitem.Rc.isEmpty() && global::XX.X.chkLENRectCirc(wmitem.Rc.x, wmitem.Rc.y, wmitem.Rc.right, wmitem.Rc.bottom, curWM.Bounds.x + 0.5f + this.posx, curWM.Bounds.y + 0.5f + this.posy, num + len_add))
					{
						bool flag2 = false;
						if (appear_ratio < 0f)
						{
							flag2 = true;
							appear_ratio = this.appear_ratio;
						}
						if (this.has_bell_ == 1)
						{
							appear_ratio = global::XX.X.Scr(appear_ratio, 0.5f);
						}
						flag = WanderingNPC.XORSP() < appear_ratio;
						if (!flag && flag2)
						{
							this.appear_ratio = global::XX.X.Scr(this.appear_ratio, 0.25f);
						}
					}
				}
				Vector2 zero = Vector2.zero;
				string text = "";
				string text2 = "";
				if (flag)
				{
					M2LabelPoint point = mp.getPoint(this.lp_name, true);
					if (point != null)
					{
						zero = new Vector2(point.mapfocx, point.bottom * mp.rCLEN);
						if (TX.valid(point.comment))
						{
							if (REG.match(point.comment, new Regex("aim[\\t \\s]+[TB]?([LR])")))
							{
								text = REG.R1;
								if (global::XX.CAim.parseString(text, -1) >= 0)
								{
									text2 = text;
								}
							}
							if (REG.match(point.comment, new Regex("greeting[\\t \\s]+[TB]?([LR])")))
							{
								text2 = REG.R1;
							}
						}
					}
					else
					{
						flag = false;
						int num2 = (int)(Bench.mbottom + 0.1f);
						bool flag3 = true;
						float num3 = 2f;
						if (meta.Get("npc_shift_first") != null)
						{
							num3 = meta.GetNm("npc_shift_first", num3, 0);
						}
						else if (meta.Get("npc_shift_first_d") != null)
						{
							num3 = meta.GetNm("npc_shift_first_d", num3, 0) / mp.CLEN;
						}
						if (meta.Get("npc_shift_set_from_right") != null)
						{
							flag3 = meta.GetB("npc_shift_set_from_right", false) != Bench.flip;
						}
						for (int i = 0; i < 8; i++)
						{
							float num4 = Bench.mapcx + (float)global::XX.X.MPF(i % 2 == 0 == flag3) * (num3 + (float)(i / 2));
							if (global::XX.X.BTW((float)mp.crop + 1.5f, num4, (float)(mp.clms - mp.crop) - 1.5f))
							{
								float footableY = mp.getFootableY(num4, (int)Bench.mapcy, 12, true, -1f, false, true, true, 0f);
								if (global::XX.X.BTW((float)(num2 - 2), footableY, (float)num2 + 1.5f))
								{
									flag = true;
									zero.Set(num4, footableY);
									break;
								}
							}
						}
					}
				}
				if (flag)
				{
					this.decided_key = mp.key;
					this.has_bell_ = 2;
					this.Anot_exist_map.Remove(mp.key);
					if (attach_mover)
					{
						try
						{
							M2Mover m2Mover = mp.getMoverByName(this.lp_name, true);
							int num5 = -1;
							if (TX.valid(text))
							{
								num5 = global::XX.CAim.parseString(text, -1);
							}
							if (num5 < 0)
							{
								if (meta.Get("npc_aim_fix") != null)
								{
									num5 = meta.getDirsI("npc_aim_fix", Bench.rotation, Bench.flip, 0, 0);
								}
								else
								{
									num5 = ((Bench.mapcx < zero.x) ? 0 : 2);
								}
							}
							if (m2Mover == null)
							{
								if (wmitem == null)
								{
									WholeMapItem curWM2 = this.M2D.WM.CurWM;
									wmitem = this.M2D.WM.CurWM.GetWmi(mp, null);
								}
								if (wmitem != null)
								{
									this.posx = wmitem.Rc.cx;
									this.posy = wmitem.Rc.cy;
								}
								M2EventContainer eventContainer = mp.getEventContainer();
								M2EventItem m2EventItem = ((this.FD_CreateEventMover == null) ? eventContainer.CreateAndAssign(this.lp_name) : this.FD_CreateEventMover(this.lp_name, eventContainer));
								if (m2EventItem != null)
								{
									m2EventItem.Size(12f, 68f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
									m2EventItem.setPxlChara(this.pxc_name, null, this.pxc_pose_name);
									m2Mover = m2EventItem;
									if (TX.noe(text2))
									{
										global::XX.AIM aim = (global::XX.AIM)num5;
										text2 = aim.ToString();
										string[] array = meta.Get("npc_greeting_aim");
										if (array != null)
										{
											if (array[0] == "L" || array[0] == "R")
											{
												text2 = array[0];
											}
											else if (array[0] == "-1")
											{
												text2 = ((text2 == "L") ? "R" : "L");
											}
										}
									}
									m2EventItem.assign("talk", "CHANGE_EVENT ___" + this.lp_name + "/_portal " + text2, false);
									if (this.FD_CreateEventMoverAfter != null)
									{
										this.FD_CreateEventMoverAfter(m2EventItem);
									}
								}
							}
							if (m2Mover != null)
							{
								this.M2D.ev_mobtype == "";
								m2Mover.setAim((global::XX.AIM)num5, false);
								m2Mover.SpSetShift(0f, 10.5f + meta.GetNm("npc_yshift", 0f, 0));
								m2Mover.setTo(zero.x, zero.y - m2Mover.sizey);
							}
						}
						catch (Exception ex)
						{
							global::XX.X.de(ex.ToString(), null);
						}
					}
				}
			}
			if (!flag)
			{
				if (this.decided_key == mp.key)
				{
					this.decided_key = null;
				}
				if (this.Anot_exist_map.IndexOf(mp.key) == -1)
				{
					this.Anot_exist_map.Add(mp.key);
				}
				if (meta.GetB("npc_not_here_hide", false))
				{
					Bench.arrangeable = true;
					Bench.addActiveRemoveKey("_NPC_NOT_HERE", false);
				}
			}
			return flag;
		}

		public void setTo(float x, float y)
		{
			this.posx = x;
			this.posy = y;
			this.appear_ratio = 1f;
		}

		public void setToHere()
		{
			this.decided_key = this.M2D.curMap.key;
			this.Anot_exist_map.Remove(this.decided_key);
		}

		public bool isEnable()
		{
			return SCN.isWNpcEnable(this.M2D, this.type);
		}

		public bool isPositionDecided()
		{
			return this.decided_key != null;
		}

		public bool alreadyMeet()
		{
			return GF.getC(this.meet_flag_name) > 0U;
		}

		public string getDecidedPosKey()
		{
			return this.decided_key;
		}

		public void setFineBellFlag()
		{
			this.has_bell_ = 2;
		}

		public bool walk_around_flag
		{
			get
			{
				return this.walk_around_flag_;
			}
			set
			{
				if (value && this.isEnable() && this.posx >= 0f)
				{
					this.walk_around_flag_ = value;
				}
			}
		}

		public bool isHere(Map2d Mp)
		{
			if (Mp != null)
			{
				return Mp.key == this.decided_key;
			}
			return this.decided_key == null;
		}

		public Vector2 getPosition()
		{
			if (this.decided_key != null)
			{
				Map2d map2d = this.M2D.Get(this.decided_key, false);
				if (map2d != null)
				{
					WholeMapItem.WMItem wmi = this.M2D.WM.CurWM.GetWmi(map2d, null);
					if (wmi != null && !wmi.Rc.isEmpty())
					{
						return new Vector2(wmi.Rc.cx, wmi.Rc.cy);
					}
				}
			}
			this.fineFirstPosition(false);
			WholeMapItem curWM = this.CurWM;
			return new Vector2(curWM.Bounds.x + 0.5f + (float)((int)this.posx), curWM.Bounds.y + 0.5f + (float)((int)this.posy));
		}

		public void readBinaryFrom(ByteArray Ba, int vers)
		{
			this.posx = Ba.readFloat();
			this.posy = Ba.readFloat();
			this.agR = Ba.readFloat();
			this.appear_ratio = Ba.readFloat();
			this.walk_around_flag_ = Ba.readBoolean();
			this.decided_key = Ba.readPascalString("utf-8", false);
			if (TX.noe(this.decided_key))
			{
				this.decided_key = null;
			}
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				this.Anot_exist_map.Add(Ba.readPascalString("utf-8", false));
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeFloat(this.posx);
			Ba.writeFloat(this.posy);
			Ba.writeFloat(this.agR);
			Ba.writeFloat(this.appear_ratio);
			Ba.writeBool(this.walk_around_flag_);
			Ba.writePascalString(this.decided_key, "utf-8");
			if (this.M2D.curMap != null && this.Anot_exist_map.IndexOf(this.M2D.curMap.key) == -1)
			{
				this.Anot_exist_map.Add(this.M2D.curMap.key);
			}
			int count = this.Anot_exist_map.Count;
			Ba.writeUShort((ushort)count);
			for (int i = 0; i < count; i++)
			{
				Ba.writePascalString(this.Anot_exist_map[i], "utf-8");
			}
		}

		public static int xors(int i)
		{
			return NightController.xors(i);
		}

		public static float NIXP(float v, float v2)
		{
			return global::XX.X.NI(v, v2, WanderingNPC.XORSP());
		}

		public static float XORSP()
		{
			return NightController.XORSP();
		}

		public static float XORSPS()
		{
			return (WanderingNPC.XORSP() - 0.5f) * 2f;
		}

		public static void shuffle<T>(T[] A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public static void shuffle<T>(List<T> A, int arraymax = -1)
		{
			NightController.shuffle<T>(A, arraymax);
		}

		public readonly WanderingManager Con;

		public readonly WanderingManager.TYPE type;

		public readonly string meet_flag_name;

		public readonly string lp_name;

		public readonly string pxc_name;

		public readonly string pxc_pose_name;

		public WanderingNPC.FnCalcHasBell FD_CalcHasBell;

		public WanderingNPC.FnCreateEventMover FD_CreateEventMover;

		public Action<M2EventItem> FD_CreateEventMoverAfter;

		public Func<float, float> FD_overrideCalcableLength;

		private float posx = -1000f;

		private float posy = -1000f;

		private float agR;

		private string decided_key;

		private bool walk_around_flag_;

		private readonly List<string> Anot_exist_map;

		public readonly float walk_len_max = 8f;

		public readonly float walk_len_min = 4f;

		public readonly float catchable = 4f;

		public readonly float appear_ratio_default;

		public float appear_ratio;

		public byte has_bell_ = 2;

		public delegate bool FnCalcHasBell(WanderingNPC Npc);

		public delegate M2EventItem FnCreateEventMover(string name, M2EventContainer EVC);
	}
}
