using System;
using System.Collections.Generic;
using Better;
using XX;

namespace nel
{
	public sealed class UIPictureFader
	{
		public UIPictureFader(string chara_key)
		{
			this.Default = new UIPictureFader.UIPFader("_");
			this.Default.same_restart = 0;
			this.Default.time_max = -1;
			this.reloadFaderScript(chara_key);
		}

		private void reloadFaderScript(string chara_key)
		{
			this.OFd = new BDic<string, UIPictureFader.UIPFader>(3);
			this.OFd["_"] = this.Default;
			this.Cur = (this.Next = null);
			CsvReaderA csvReaderA = new CsvReaderA(MTR.Read("uipic_fader_" + chara_key, "", ".csv"), false);
			csvReaderA.VarCon = new CsvVariableContainer();
			new List<UIPictureBodyData>();
			csvReaderA.tilde_replace = true;
			List<UIPictureFader.UIPFader> list = new List<UIPictureFader.UIPFader>(2);
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "%CHARA")
				{
					if (chara_key != null && csvReaderA._1 != chara_key)
					{
						X.de("異なるスクリプトデータ", null);
						return;
					}
					chara_key = null;
				}
				if (chara_key == null)
				{
					if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
					{
						string[] array = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1).Split(new char[] { '|' });
						list.Clear();
						int num = array.Length;
						for (int i = 0; i < num; i++)
						{
							list.Add(this.Get(array[i], false, false));
						}
					}
					else
					{
						int num = list.Count;
						string cmd = csvReaderA.cmd;
						if (cmd != null)
						{
							uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
							if (num2 <= 2498028297U)
							{
								if (num2 <= 1564253156U)
								{
									if (num2 != 826051549U)
									{
										if (num2 != 1246471218U)
										{
											if (num2 == 1564253156U)
											{
												if (cmd == "time")
												{
													int num3 = csvReaderA.Int(1, 0);
													int num4 = csvReaderA.Int(2, -2);
													if (num4 <= -2)
													{
														num4 = num3;
													}
													for (int j = 0; j < num; j++)
													{
														UIPictureFader.UIPFader uipfader = list[j];
														uipfader.time_min = num3;
														uipfader.time_max = num4;
													}
												}
											}
										}
										else if (cmd == "progress_anim")
										{
											int num3 = csvReaderA.Int(1, 0);
											int num4 = csvReaderA.Int(2, -1);
											if (num4 < 0)
											{
												num4 = num3;
											}
											for (int k = 0; k < num; k++)
											{
												UIPictureFader.UIPFader uipfader2 = list[k];
												uipfader2.progress_anim_min = (byte)num3;
												uipfader2.progress_anim_max = (byte)num4;
											}
										}
									}
									else if (cmd == "%CLONE")
									{
										UIPictureFader.UIPFader uipfader3 = this.Get(csvReaderA._1, false, false);
										if (uipfader3 != null)
										{
											for (int l = 0; l < num; l++)
											{
												list[l].copyFrom(uipfader3);
											}
										}
									}
								}
								else if (num2 != 2142762121U)
								{
									if (num2 != 2228276373U)
									{
										if (num2 == 2498028297U)
										{
											if (cmd == "priority")
											{
												int num3 = csvReaderA.Int(1, 0);
												for (int m = 0; m < num; m++)
												{
													list[m].priority = num3;
												}
											}
										}
									}
									else if (cmd == "return_if_state_normal")
									{
										int num3 = csvReaderA.Int(1, 1);
										for (int n = 0; n < num; n++)
										{
											list[n].return_if_state_normal = num3 != 0;
										}
									}
								}
								else if (cmd == "addstate")
								{
									string[] array2 = csvReaderA.slice(1, -1000);
									for (int num5 = 0; num5 < num; num5++)
									{
										list[num5].setEmState(array2);
									}
								}
							}
							else if (num2 <= 3401048762U)
							{
								if (num2 != 2561476631U)
								{
									if (num2 != 2846199180U)
									{
										if (num2 == 3401048762U)
										{
											if (cmd == "alloc_insert_ratio")
											{
												float num6 = csvReaderA.Nm(1, 100f) / 100f;
												for (int num7 = 0; num7 < num; num7++)
												{
													list[num7].alloc_insert_ratio = num6;
												}
											}
										}
									}
									else if (cmd == "drop")
									{
										int num3 = csvReaderA.Int(1, 1);
										for (int num8 = 0; num8 < num; num8++)
										{
											list[num8].drop = num3 != 0;
										}
									}
								}
								else if (cmd == "same_restart")
								{
									int num3 = csvReaderA.Int(1, 100);
									for (int num9 = 0; num9 < num; num9++)
									{
										list[num9].same_restart = (byte)num3;
									}
								}
							}
							else if (num2 <= 3789661478U)
							{
								if (num2 != 3537079863U)
								{
									if (num2 == 3789661478U)
									{
										if (cmd == "ignore_in_frozen")
										{
											int num3 = csvReaderA.Int(1, 1);
											for (int num10 = 0; num10 < num; num10++)
											{
												list[num10].ignore_in_frozen = num3 != 0;
											}
										}
									}
								}
								else if (cmd == "immediate_load")
								{
									int num3 = csvReaderA.Int(1, 1);
									for (int num11 = 0; num11 < num; num11++)
									{
										list[num11].immediate_load = num3 != 0;
									}
								}
							}
							else if (num2 != 4029939182U)
							{
								if (num2 == 4063372922U)
								{
									if (cmd == "immediate")
									{
										int num3 = csvReaderA.Int(1, 1);
										for (int num12 = 0; num12 < num; num12++)
										{
											list[num12].immediate = num3 != 0;
										}
									}
								}
							}
							else if (cmd == "uiemot")
							{
								string[] array3 = csvReaderA.slice(1, -1000);
								for (int num13 = 0; num13 < num; num13++)
								{
									list[num13].setEmot(array3);
								}
							}
						}
					}
				}
			}
		}

		public UIPictureFader.UIPFader Get(string key, bool no_make = true, bool no_error = false)
		{
			if (key == null)
			{
				return null;
			}
			UIPictureFader.UIPFader uipfader = X.Get<string, UIPictureFader.UIPFader>(this.OFd, key);
			if (uipfader != null)
			{
				return uipfader;
			}
			if (no_make)
			{
				if (!no_error)
				{
					X.de("UIPictureFader::Get ... 不明なkey " + key, null);
				}
				return this.Default;
			}
			return this.OFd[key] = new UIPictureFader.UIPFader(key);
		}

		public void Blur()
		{
			this.Cur = null;
		}

		public UIPictureFader.UIP_RES Explode(UIPictureBase PCon, string key, bool immediate = false, bool force_change = false, bool do_not_restart = false)
		{
			if (key == null)
			{
				return UIPictureFader.UIP_RES.ERROR;
			}
			UIPictureFader.UIPFader uipfader = this.Get(key, true, false);
			if (uipfader == null)
			{
				X.de("不明なFade キー:" + key, null);
				return UIPictureFader.UIP_RES.ERROR;
			}
			UIPictureFader.UIP_RES uip_RES = UIPictureFader.UIP_RES.CHANGED;
			UIPictureFader.UIPFader uipfader2 = this.Next ?? this.Cur;
			if (uipfader2 != null)
			{
				if (!PCon.faderEnable(uipfader))
				{
					return (UIPictureFader.UIP_RES)0;
				}
				if (!force_change && !this.timeout_skip && (uipfader2.time_min < 0 || this.t < (float)uipfader2.time_min) && uipfader2 != uipfader && X.XORSP() >= (uipfader2.alloc_insert_ratio + X.ZLINE((float)(uipfader.priority - uipfader2.priority), 100f)) * (1f - X.ZLINE((float)(uipfader2.priority - uipfader.priority), 100f)))
				{
					uip_RES = (UIPictureFader.UIP_RES)0;
					if (do_not_restart)
					{
						return uip_RES;
					}
					if (this.Cur.progress_anim_max < 0)
					{
						this.t = X.Mx(0f, this.t + (float)this.Cur.progress_anim_min);
					}
					else
					{
						int num = X.IntR(X.NIXP((float)this.Cur.progress_anim_min, (float)this.Cur.progress_anim_max));
						if (num > 0)
						{
							this.t += (float)num;
							UIPictureBodySpine uipictureBodySpine = PCon.getBodyData() as UIPictureBodySpine;
							if (uipictureBodySpine != null)
							{
								uipictureBodySpine.getViewer().progressTimePositionAll(this.t / 60f);
							}
						}
					}
					return uip_RES;
				}
				else
				{
					this.timeout_skip = false;
					if (uipfader2 == uipfader)
					{
						if (do_not_restart)
						{
							return (UIPictureFader.UIP_RES)0;
						}
						uip_RES |= UIPictureFader.UIP_RES.RESTART | UIPictureFader.UIP_RES.REDRAW;
					}
					else
					{
						if (this.Cur != null)
						{
							if (!this.Cur.drop && uipfader.drop)
							{
								if (!uipfader.immediate && !immediate)
								{
									this.Next = uipfader;
									this.t_ground = 1f;
									this.maxt_ground = 22;
									PCon.readFader(this.Next, UIPictureFader.UIP_RES.JUST_PREPARE, UIPictureBase.EMSTATE.NORMAL, false);
									return UIPictureFader.UIP_RES.TO_DROP;
								}
								this.t_ground = -1f;
								uip_RES |= UIPictureFader.UIP_RES.DROPPED;
							}
							else if (this.Cur.drop && !uipfader.drop)
							{
								this.t_ground = -1f;
								this.maxt_ground = 60;
								uip_RES |= UIPictureFader.UIP_RES.STANDUP;
							}
						}
						uip_RES |= UIPictureFader.UIP_RES.RESTART | ((uipfader.immediate || immediate) ? UIPictureFader.UIP_RES.IMMEDIATE : ((UIPictureFader.UIP_RES)0));
						if (uipfader.immediate_load)
						{
							BetobetoManager.immediate_load_material = 6;
						}
					}
				}
			}
			else
			{
				uip_RES |= UIPictureFader.UIP_RES.RESTART;
			}
			if ((uip_RES & UIPictureFader.UIP_RES.RESTART) != (UIPictureFader.UIP_RES)0)
			{
				this.t = 0f;
				this.Next = null;
				if (this.Cur != uipfader && (uip_RES & (UIPictureFader.UIP_RES.DROPPED | UIPictureFader.UIP_RES.STANDUP)) == (UIPictureFader.UIP_RES)0)
				{
					this.t_ground = 0f;
					PCon.ground_level = 0f;
				}
			}
			this.Cur = uipfader;
			return uip_RES;
		}

		public UIPictureFader.UIP_RES run(UIPictureBase PCon, float fcnt, float fcnt_maintime)
		{
			UIPictureFader.UIP_RES uip_RES = this.runGroundLevel(PCon, fcnt, true);
			if (this.Cur != null)
			{
				this.t += fcnt * fcnt_maintime;
				if (this.Cur.time_max > 0)
				{
					if (UIPictureBase.FlgStopAutoFade.isActive())
					{
						this.t = X.MMX(0f, this.t, (float)(this.Cur.time_max / 2));
						return uip_RES;
					}
					if (this.t >= (float)this.Cur.time_max && (!this.Cur.return_if_state_normal || PCon.isPlayerStateNormal()))
					{
						PCon.recheck_emot = true;
						this.timeout_skip = true;
					}
				}
			}
			return uip_RES;
		}

		public UIPictureFader.UIP_RES runGroundLevel(UIPictureBase PCon, float fcnt, bool changeable = true)
		{
			UIPictureFader.UIP_RES uip_RES = (UIPictureFader.UIP_RES)0;
			if (this.t_ground > 0f && this.Next != null)
			{
				if (changeable)
				{
					this.t_ground += fcnt;
					if (this.t_ground >= (float)this.maxt_ground)
					{
						uip_RES |= UIPictureFader.UIP_RES.DROPPED;
						if (PCon.readFader(this.Next, UIPictureFader.UIP_RES.DROPPED | (this.Next.immediate ? UIPictureFader.UIP_RES.IMMEDIATE : ((UIPictureFader.UIP_RES)0)), UIPictureBase.EMSTATE.NORMAL, false) != UIPictureFader.UIP_RES.NOW_LOADING)
						{
							this.t = 0f;
							this.t_ground = -1f;
							this.maxt_ground = 90;
							this.Cur = this.Next;
							if (this.Cur.immediate_load)
							{
								BetobetoManager.immediate_load_material = 6;
							}
							this.Next = null;
						}
						else
						{
							this.maxt_ground = X.Mx(100, this.maxt_ground) + 10;
							this.t_ground = (float)(this.maxt_ground - 10);
						}
					}
					else
					{
						PCon.ground_level = X.ZLINE(this.t_ground, (float)X.Mn(100, this.maxt_ground));
					}
				}
				else
				{
					this.t_ground = X.VALWALK(this.t_ground, 1f, fcnt);
				}
			}
			else if (this.t_ground < 0f)
			{
				this.t_ground -= fcnt;
				if (this.t_ground < (float)(-(float)this.maxt_ground))
				{
					this.t_ground = 0f;
					PCon.ground_level = 0f;
				}
				else
				{
					PCon.ground_level = -X.ZLINE(-this.t_ground, (float)this.maxt_ground);
				}
			}
			else
			{
				PCon.ground_level = 0f;
			}
			return uip_RES;
		}

		public void AssignGroundNext(UIPictureFader.UIPFader _Next, int delay = 90)
		{
			this.Next = _Next;
			if (this.t_ground > 0f)
			{
				this.t_ground = X.Mx(1f, X.ZLINE(this.t_ground, (float)X.Mn(100, this.maxt_ground)) * (float)delay);
			}
			else
			{
				this.t_ground = 1f;
			}
			this.maxt_ground = delay;
		}

		public string getCurrentFadeKey(bool consider_next = true)
		{
			if (consider_next && this.Next != null)
			{
				return this.Next.key;
			}
			if (this.Cur == null)
			{
				return null;
			}
			return this.Cur.key;
		}

		private BDic<string, UIPictureFader.UIPFader> OFd;

		public UIPictureFader.UIPFader Default;

		public UIPictureFader.UIPFader Cur;

		private UIPictureFader.UIPFader Next;

		private float t;

		private float t_ground;

		private const int MAXT_TO_DROP = 22;

		private const int MAXT_DROPPED = 90;

		private const int MAXT_STANDUP = 60;

		public int maxt_ground = 30;

		private bool timeout_skip;

		public class UIPFader
		{
			public UIPFader(string _key)
			{
				this.key = _key;
				this.setEmot(new string[] { this.key });
			}

			public void copyFrom(UIPictureFader.UIPFader Src)
			{
				this.Auiemot = Src.Auiemot;
				this.add_state = Src.add_state;
				this.priority = Src.priority;
				this.alloc_insert_ratio = Src.alloc_insert_ratio;
				this.time_min = Src.time_min;
				this.time_max = Src.time_max;
				this.progress_anim_min = Src.progress_anim_min;
				this.progress_anim_max = Src.progress_anim_max;
				this.same_restart = Src.same_restart;
				this.immediate_load = Src.immediate_load;
				this.flags = Src.flags;
			}

			public UIPictureFader.UIPFader setEmot(params string[] Astr)
			{
				int num = Astr.Length;
				List<UIEMOT> list = ((this.Auiemot == null) ? new List<UIEMOT>(1) : new List<UIEMOT>(this.Auiemot));
				for (int i = 0; i < num; i++)
				{
					UIEMOT uiemot;
					if (FEnum<UIEMOT>.TryParse(Astr[i].ToUpper(), out uiemot, true))
					{
						list.Add(uiemot);
					}
				}
				if (list.Count > 0)
				{
					this.Auiemot = list.ToArray();
				}
				return this;
			}

			public UIPictureFader.UIPFader setEmState(params string[] Astr)
			{
				int num = Astr.Length;
				for (int i = 0; i < num; i++)
				{
					UIPictureBase.EMSTATE emstate;
					if (FEnum<UIPictureBase.EMSTATE>.TryParse(Astr[i].ToUpper(), out emstate, true))
					{
						this.add_state |= emstate;
					}
				}
				return this;
			}

			public UIEMOT getEmot()
			{
				if (this.Auiemot != null)
				{
					return this.Auiemot[X.xors(this.Auiemot.Length)];
				}
				return UIEMOT.STAND;
			}

			public bool return_if_state_normal
			{
				get
				{
					return (this.flags & 1) != 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 1) : (this.flags & -2));
				}
			}

			public bool immediate
			{
				get
				{
					return (this.flags & 2) != 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 2) : (this.flags & -3));
				}
			}

			public bool drop
			{
				get
				{
					return (this.flags & 4) != 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 4) : (this.flags & -5));
				}
			}

			public bool ignore_in_frozen
			{
				get
				{
					return (this.flags & 8) != 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 8) : (this.flags & -9));
				}
			}

			public string key;

			private UIEMOT[] Auiemot;

			public UIPictureBase.EMSTATE add_state;

			public int priority;

			public float alloc_insert_ratio = 1f;

			public int time_min;

			public int time_max = -1;

			public byte progress_anim_min;

			public byte progress_anim_max;

			public byte same_restart;

			public bool immediate_load;

			public int flags = 1;
		}

		[Flags]
		public enum UIP_RES
		{
			CHANGED = 1,
			RESTART = 2,
			ERROR = 4,
			REDRAW = 8,
			TO_DROP = 16,
			DROPPED = 32,
			STANDUP = 64,
			IMMEDIATE = 128,
			NOW_LOADING = 256,
			JUST_PREPARE = 512,
			STATE_ABSOLUTE = 1024
		}
	}
}
