using System;
using Better;
using UnityEngine;

namespace XX
{
	public class PTCThread : RBase<EffectStocker>, IRunAndDestroy
	{
		public PTCThread(PTCThreadRunner _STCon)
			: base(8, true, false, false)
		{
			this.STCon = _STCon;
			this.VP = new VariableP(5);
		}

		public override void destruct()
		{
			this.kill(false);
		}

		public override EffectStocker Create()
		{
			return new EffectStocker();
		}

		public PTCThread VPClear()
		{
			this.VP.Clear();
			return this;
		}

		public PTCThread PreDefine(VariableP PreP)
		{
			this.VP.Add(PreP, true);
			return this;
		}

		public PTCThread Set(EfSetterP _Target, IEfPInteractale Listener = null, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW)
		{
			this.Target = _Target;
			this.Target.ParseScript();
			this.Listener_ = Listener;
			this.follow = _follow;
			this.ptc_can_set = true;
			this.t_wait = (float)(this.af = 0);
			this.cur_line = 0;
			this.LEN = 0;
			this.rER = null;
			this.id = (EfParticleManager.create_count += 1U);
			return this;
		}

		public PTCThread ReloadScript()
		{
			if (this.Target == null)
			{
				return this;
			}
			int num = this.Target.countLines();
			string key = this.Target.key;
			this.Target = EfParticleManager.GetSetterScript(key);
			if (this.Target == null)
			{
				X.de("EfSetterP " + key + " は削除されています", null);
				return this;
			}
			this.Target.ParseScript();
			if (this.Target.countLines() != num)
			{
				this.cur_line = 0;
			}
			this.t_wait = 0f;
			return this;
		}

		public HashP Hash
		{
			get
			{
				return this.Target.Hash;
			}
		}

		public string key
		{
			get
			{
				if (this.Target == null)
				{
					return null;
				}
				return this.Target.key;
			}
		}

		public string getIndex(int _i)
		{
			if (this.rER == null)
			{
				return null;
			}
			return this.rER.getIndex(_i);
		}

		public float Nm(int _i, float defv = 0f)
		{
			if (this.rER == null)
			{
				return defv;
			}
			return this.rER.Nm(_i, defv);
		}

		public StringKey getRandom(int i = 1, int len = -1)
		{
			if (this.rER == null)
			{
				return null;
			}
			return this.rER.getRandomHash(this.Hash, i, len);
		}

		public float NmE(int _i, float defv = 0f)
		{
			return this.Nm(_i, defv);
		}

		public int Int(int _i, int defv = 0)
		{
			return (int)this.Nm(_i, (float)defv);
		}

		public int IntE(int _i, int defv = 0)
		{
			return (int)this.Nm(_i, (float)defv);
		}

		public StringKey getHash(int i)
		{
			if (this.rER == null)
			{
				return null;
			}
			ReplacableString replacable = this.rER.getReplacable(i);
			if (replacable == null)
			{
				return null;
			}
			return this.Hash.Get(replacable, this.TargetVP);
		}

		public void CopyBaked(int i, STB Dep)
		{
			if (this.rER == null)
			{
				return;
			}
			ReplacableString replacable = this.rER.getReplacable(i);
			if (replacable == null)
			{
				return;
			}
			replacable.CopyBaked(this.VP, Dep);
		}

		public bool isNm(int i)
		{
			return this.rER != null && this.rER.isNm(i);
		}

		public string cmd
		{
			get
			{
				return this.getIndex(0);
			}
		}

		public string _1
		{
			get
			{
				return this.getIndex(1);
			}
		}

		public string _2
		{
			get
			{
				return this.getIndex(2);
			}
		}

		public string _3
		{
			get
			{
				return this.getIndex(3);
			}
		}

		public string _4
		{
			get
			{
				return this.getIndex(4);
			}
		}

		public string _5
		{
			get
			{
				return this.getIndex(5);
			}
		}

		public string _6
		{
			get
			{
				return this.getIndex(6);
			}
		}

		public string _7
		{
			get
			{
				return this.getIndex(7);
			}
		}

		public float _N1
		{
			get
			{
				return this.Nm(1, 0f);
			}
		}

		public float _N2
		{
			get
			{
				return this.Nm(2, 0f);
			}
		}

		public float _N3
		{
			get
			{
				return this.Nm(3, 0f);
			}
		}

		public float _N4
		{
			get
			{
				return this.Nm(4, 0f);
			}
		}

		public float _N5
		{
			get
			{
				return this.Nm(5, 0f);
			}
		}

		public float _N6
		{
			get
			{
				return this.Nm(6, 0f);
			}
		}

		public float _N7
		{
			get
			{
				return this.Nm(7, 0f);
			}
		}

		public float _NE1
		{
			get
			{
				return this.Nm(1, 0f);
			}
		}

		public float _NE2
		{
			get
			{
				return this.Nm(2, 0f);
			}
		}

		public float _NE3
		{
			get
			{
				return this.Nm(3, 0f);
			}
		}

		public float _NE4
		{
			get
			{
				return this.Nm(4, 0f);
			}
		}

		public float _NE5
		{
			get
			{
				return this.Nm(5, 0f);
			}
		}

		public float _NE6
		{
			get
			{
				return this.Nm(6, 0f);
			}
		}

		public float _NE7
		{
			get
			{
				return this.Nm(7, 0f);
			}
		}

		public int clength
		{
			get
			{
				if (this.rER == null)
				{
					return 0;
				}
				return this.rER.clength;
			}
		}

		public VariableP TargetVP
		{
			get
			{
				return EfSetterP.TargetVP;
			}
		}

		public virtual IEffectSetter Ef
		{
			get
			{
				return this.STCon.Ef;
			}
		}

		public PTCThread kill(bool do_not_kill_stock_effect = false)
		{
			this.rER = null;
			this.cur_line = 0;
			this.Target = null;
			if (!do_not_kill_stock_effect)
			{
				this.killStockEffect();
			}
			if (PTCThread.CurrentReading == this)
			{
				PTCThread.CurrentReading = null;
			}
			return this;
		}

		public PTCThread killStockEffect()
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItems[i].kill(this);
			}
			this.LEN = 0;
			return this;
		}

		public PTCThread updateFrameStockEffect(float mint)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItems[i].updateFrameStockEffect(mint);
			}
			return this;
		}

		public PTCThread fineEffect()
		{
			return this;
		}

		public IEfPInteractale Listener
		{
			get
			{
				return this.Listener_;
			}
		}

		public EffectItem StockEffect(EffectItem _E)
		{
			if (_E == null)
			{
				return null;
			}
			base.Pop(8).Set(_E);
			return _E;
		}

		public void StockSound(SndPlayer Snd)
		{
			if (Snd == null)
			{
				return;
			}
			base.Pop(8).Set(Snd);
		}

		public int countBracketsOnThisLine(ref int _cnt)
		{
			if (this.rER == null)
			{
				return _cnt = 0;
			}
			for (int i = this.clength - 1; i >= 0; i--)
			{
				string index = this.rER.getIndex(i);
				if (index != null)
				{
					if (!(index == "{"))
					{
						if (index == "}")
						{
							_cnt--;
						}
					}
					else
					{
						_cnt++;
					}
				}
			}
			return _cnt;
		}

		public PTCThread Def(string k, float def)
		{
			this.VP.Add(k, (double)def);
			return this;
		}

		public bool tError(string t)
		{
			X.de(t, null);
			X.de("Line: " + this.cur_line.ToString() + " / PTC " + this.key, null);
			return true;
		}

		public void seek_set()
		{
			this.cur_line = 0;
		}

		public override bool run(float fcnt = 1f)
		{
			if (!this.isActive())
			{
				return false;
			}
			try
			{
				bool flag = false;
				if (this.t_wait > 0f)
				{
					this.t_wait -= fcnt;
					if (this.t_wait > 0f)
					{
						flag = true;
					}
					else
					{
						this.t_wait = 0f;
					}
				}
				PTCThread.CurrentReading = this;
				VariableP.error_occured = false;
				if (!flag)
				{
					this.Target.goto_count = 0;
					string key = this.Target.key;
					Bench.P(key);
					this.VP.Add("_af", (double)this.af);
					EfSetterP.TargetVP = this.VP;
					while (!flag && this.isActive())
					{
						this.rER = this.Target.getNextLine(ref this.cur_line);
						if (this.rER == null || this.rER.espk == EfSetterP.ESPK.GOTO)
						{
							break;
						}
						Bench.P("Efsp - main loop");
						string text = Bench.P(this.rER.pre_script);
						if (this.Listener == null || !this.Listener.readPtcScript(this))
						{
							string cmd = this.rER.cmd;
							if (cmd != null)
							{
								uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
								if (num <= 1269775406U)
								{
									if (num <= 363198355U)
									{
										if (num != 182646103U)
										{
											if (num != 226166113U)
											{
												if (num != 363198355U)
												{
													goto IL_0576;
												}
												if (!(cmd == "%SND"))
												{
													goto IL_0576;
												}
											}
											else
											{
												if (!(cmd == "%WAIT"))
												{
													goto IL_0576;
												}
												goto IL_0345;
											}
										}
										else
										{
											if (!(cmd == "%SKIP"))
											{
												goto IL_0576;
											}
											goto IL_0372;
										}
									}
									else if (num <= 1009895389U)
									{
										if (num != 392739404U)
										{
											if (num != 1009895389U)
											{
												goto IL_0576;
											}
											if (!(cmd == "%FOLLOW"))
											{
												goto IL_0576;
											}
											if (TX.valid(this.rER._1) && !FEnum<PTCThread.StFollow>.TryParse(this.rER._1, out this.follow, true))
											{
												this.rER.tError("不明な StFollow :" + this.rER._1);
											}
											if (this.NmE(2, 0f) != 0f)
											{
												this.follow |= PTCThread.StFollow.__DEFINE_Z;
												this.fineReposit(fcnt);
												goto IL_0586;
											}
											goto IL_0586;
										}
										else
										{
											if (!(cmd == "%RANDOMSKIP"))
											{
												goto IL_0576;
											}
											this.t_wait = (float)X.xors(X.Mx(1, this.IntE(1, 1)));
											flag = true;
											goto IL_0586;
										}
									}
									else if (num != 1221529628U)
									{
										if (num != 1269775406U)
										{
											goto IL_0576;
										}
										if (!(cmd == "%%SND"))
										{
											goto IL_0576;
										}
									}
									else
									{
										if (!(cmd == "%GET"))
										{
											goto IL_0576;
										}
										StringKey hash = this.getHash(2);
										EfParticle efParticle = EfParticle.Get(hash, false);
										if (efParticle == null)
										{
											X.de("PTCThread::run Get のパーティクル対象が不明 '" + hash.Value + "'", null);
											goto IL_0586;
										}
										float num2;
										if (efParticle.getValueProp(this._3, out num2))
										{
											X.de("PTCThread::run Field 対象が不明 '" + this._3 + "'", null);
											goto IL_0586;
										}
										this.VP.Add((this._1 == "") ? "_" : this._1, (double)num2);
										goto IL_0586;
									}
									this.playSndIndex(1, this.clength - 1);
									goto IL_0586;
								}
								if (num <= 2393865632U)
								{
									if (num != 1521923729U)
									{
										if (num != 2126360770U)
										{
											if (num != 2393865632U)
											{
												goto IL_0576;
											}
											if (!(cmd == "WAIT"))
											{
												goto IL_0576;
											}
										}
										else
										{
											if (!(cmd == "SKIP"))
											{
												goto IL_0576;
											}
											goto IL_0372;
										}
									}
									else
									{
										if (!(cmd == "%CONTINUE"))
										{
											goto IL_0576;
										}
										this.af++;
										flag = true;
										goto IL_0586;
									}
								}
								else if (num <= 3742671904U)
								{
									if (num != 2608174344U)
									{
										if (num != 3742671904U)
										{
											goto IL_0576;
										}
										if (!(cmd == "%STOP_SND"))
										{
											goto IL_0576;
										}
										this.killStockSound(this._1);
										goto IL_0586;
									}
									else
									{
										if (!(cmd == "%AGD"))
										{
											goto IL_0576;
										}
										AttackGhostDrawer agd = EfParticleManager.GetAGD(this._1);
										if (agd == null)
										{
											goto IL_0586;
										}
										EffectItem effectItem = this.setEffectTo(this.Ef, true, 2, agd.FD_EfDraw);
										if (effectItem != null)
										{
											effectItem.setFunction(agd.FD_EfDraw, this._2);
											goto IL_0586;
										}
										goto IL_0586;
									}
								}
								else if (num != 3818006558U)
								{
									if (num != 3853545659U)
									{
										goto IL_0576;
									}
									if (!(cmd == "%EF"))
									{
										goto IL_0576;
									}
									this.setEffectTo(this.Ef, true, 1, null);
									goto IL_0586;
								}
								else
								{
									if (!(cmd == "%LOOP"))
									{
										goto IL_0576;
									}
									this.seek_set();
									this.af++;
									flag = true;
									goto IL_0586;
								}
								IL_0345:
								this.t_wait = this.NmE(1, 1f);
								this.af += (int)this.t_wait;
								flag = true;
								goto IL_0586;
								IL_0372:
								this.t_wait = (float)((int)this.NmE(1, 1f));
								flag = true;
								goto IL_0586;
							}
							IL_0576:
							this.setEffectTo(this.Ef, false, 0, null);
						}
						IL_0586:
						Bench.Pend(text);
						Bench.Pend("Efsp - main loop");
					}
					Bench.Pend(key);
					if (EfSetterP.TargetVP == this.VP)
					{
						EfSetterP.TargetVP = null;
					}
				}
				if (VariableP.error_occured)
				{
					this.tError("変数展開でエラー発生");
					VariableP.error_occured = false;
				}
				if (!this.isActive() || (this.cur_line >= this.Target.countLines() && !flag))
				{
					this.kill(false);
					return false;
				}
				if (PTCThread.CurrentReading == this)
				{
					this.fineReposit(fcnt);
					base.run(fcnt);
					PTCThread.CurrentReading = null;
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		private bool fineReposit(float fcnt)
		{
			bool flag = false;
			Vector3 vector;
			if (this.Listener_ != null && this.Listener_.getEffectReposition(this, this.follow & PTCThread.StFollow.__VALID_FOLLOW, fcnt, out vector))
			{
				PTCThread.PosEffectReposit.Set(vector.x, vector.y, 1f);
				flag = true;
				if ((this.follow & PTCThread.StFollow.__DEFINE_Z) != PTCThread.StFollow.NO_FOLLOW)
				{
					double num = (double)vector.z;
					this.VP.Add("__x", (double)vector.x);
					this.VP.Add("__y", (double)vector.y);
					this.VP.Add("__z", num);
				}
			}
			else
			{
				PTCThread.PosEffectReposit.z = 0f;
			}
			return flag;
		}

		public EfSetterP.ESPLine getNextLineSimple()
		{
			if (this.Target == null || this.cur_line >= this.Target.countLines())
			{
				return null;
			}
			EfSetterP target = this.Target;
			int num = this.cur_line;
			this.cur_line = num + 1;
			return target.GetLine(num);
		}

		public virtual EffectItem setEffectTo(IEffectSetter EFT, bool is_normal_effect = false, int ptc_i = 1, FnEffectRun EfRun = null)
		{
			if (EFT == null || this.rER == null)
			{
				return null;
			}
			STB stb = TX.PopBld(null, 0);
			this.rER.getReplacable(ptc_i).CopyBaked(this.TargetVP, stb);
			bool flag = false;
			if (stb.isStart('!'))
			{
				flag = true;
				stb.Splice(0, 1);
			}
			if (!flag && !this.ptc_can_set)
			{
				TX.ReleaseBld(stb);
				return null;
			}
			bool flag2 = false;
			if (stb.isStart('*'))
			{
				stb.Splice(0, 1);
				flag2 = true;
			}
			EffectItem effectItem = null;
			ptc_i--;
			StringKey stringKey = this.Hash.Get(stb, 0, -1);
			TX.ReleaseBld(stb);
			if (!is_normal_effect)
			{
				EfParticle efParticle = EfParticle.Get(stringKey, true);
				if (efParticle == null)
				{
					this.rER.tError("PTCThread::run パーティクル対象が不明 '" + stb.ToString() + "'");
				}
				else
				{
					effectItem = EFT.PtcN(efParticle, this.Nm(ptc_i + 2, 0f), this.Nm(ptc_i + 3, 0f), this.Nm(ptc_i + 4, 0f), this.Int(ptc_i + 5, 0), this.Int(ptc_i + 6, 0));
				}
			}
			else if (EfRun != null)
			{
				effectItem = EFT.setEffectWithSpecificFn(stringKey, this.Nm(ptc_i + 2, 0f), this.Nm(ptc_i + 3, 0f), this.Nm(ptc_i + 4, 0f), this.Int(ptc_i + 5, 0), this.Int(ptc_i + 6, 0), EfRun);
			}
			else
			{
				effectItem = EFT.setE(stringKey, this.Nm(ptc_i + 2, 0f), this.Nm(ptc_i + 3, 0f), this.Nm(ptc_i + 4, 0f), this.Int(ptc_i + 5, 0), this.Int(ptc_i + 6, 0));
				effectItem.title = "particle";
			}
			if (effectItem != null && this.Listener_ != null && !this.Listener_.initSetEffect(this, effectItem))
			{
				effectItem.destruct();
				effectItem = null;
			}
			if (flag2 && effectItem != null)
			{
				this.StockEffect(effectItem);
			}
			return effectItem;
		}

		public StringKey getSoundKey(int rer_si, int rer_len, out bool hold_act, out bool hold)
		{
			STB stb = TX.PopBld(null, 0);
			this.rER.getReplacable((rer_len <= 1) ? rer_si : (rer_si + X.xors(rer_len))).CopyBaked(this.VP, stb);
			StringKey stringKey = PTCThread.fixSoundKey(stb, out hold_act, out hold, this.Hash);
			TX.ReleaseBld(stb);
			return stringKey;
		}

		private void playSndIndex(int rer_si, int rer_len)
		{
			bool flag;
			bool flag2;
			StringKey soundKey = this.getSoundKey(rer_si, rer_len, out flag, out flag2);
			SND.Ui.play(soundKey, false);
			if (flag2)
			{
				this.StockSound(SND.Ui);
			}
		}

		public void killStockSound(string key)
		{
			for (int i = base.Length - 1; i >= 0; i--)
			{
				if (TX.noe(key) || this.AItems[i].Is(PTCThread.CurrentReading, key))
				{
					this.AItems[i].kill(PTCThread.CurrentReading);
				}
			}
		}

		public static StringKey fixSoundKey(STB Stb, out bool hold_act, out bool hold, HashP Hash)
		{
			hold_act = (hold = false);
			if (Stb.isStart('*'))
			{
				hold = true;
				if (Stb.isStart("**", 0))
				{
					Stb.Splice(0, 2);
					hold_act = true;
				}
				else
				{
					Stb.Splice(0, 1);
				}
			}
			return Hash.Get(Stb, 0, -1);
		}

		public bool quitReading()
		{
			bool flag = TX.isStart(this.cmd, "%", 0);
			this.kill(false);
			return flag;
		}

		public override bool isActive()
		{
			return this.Target != null;
		}

		public override string ToString()
		{
			if (this.Target == null)
			{
				return "(PTCThread)";
			}
			return this.Target.key + " @" + this.cur_line.ToString();
		}

		public bool isActive(uint _id)
		{
			return this.isActive() && this.id == _id;
		}

		public readonly PTCThreadRunner STCon;

		public EfSetterP Target;

		private IEfPInteractale Listener_;

		private VariableP VP;

		private int cur_line;

		public uint id;

		public float t_wait;

		public int af;

		public bool ptc_can_set = true;

		public PTCThread.StFollow follow;

		private EfSetterP.ESPLine rER;

		public static PTCThread CurrentReading;

		public static Vector3 PosEffectReposit;

		private const string FOLLOW_X_VAR_KEY = "__x";

		private const string FOLLOW_Y_VAR_KEY = "__y";

		private const string FOLLOW_Z_VAR_KEY = "__z";

		public enum StFollow : ushort
		{
			NO_FOLLOW,
			FOLLOW_C,
			FOLLOW_T,
			FOLLOW_HIP,
			FOLLOW_HEAD,
			FOLLOW_S,
			FOLLOW_D,
			FOLLOW_MAGICCIRCLE,
			_ALL,
			__DEFINE_Z = 256,
			__VALID_FOLLOW = 255
		}
	}
}
