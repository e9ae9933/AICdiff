using System;
using evt;
using UnityEngine;
using XX;

namespace m2d
{
	public abstract class M2Attackable : M2Mover, IM2RayHitAble, IEfPInteractale
	{
		public override void appear(Map2d _Mp)
		{
			this.appear(_Mp, true);
			if (this.PtcHld == null)
			{
				this.PtcHld = new PtcHolder(this, 4, 4);
			}
		}

		protected void appear(Map2d _Mp, bool create_CC)
		{
			this.maxhp = X.Mx(this.maxhp, 1);
			this.maxmp = X.Mx(this.maxmp, 1);
			if (this.hp == -1000)
			{
				this.hp = this.maxhp;
			}
			if (this.mp == -1000)
			{
				this.mp = this.maxmp;
			}
			this.knockback_time = ((this.knockback_time == 0) ? 22 : this.knockback_time);
			if (this.CC == null && create_CC)
			{
				this.CC = new M2MvColliderCreatorAtk(this);
			}
			this.PressCheck = null;
			base.appear(_Mp);
			if (this.TeCon == null)
			{
				this.TeCon = new TransEffecter("M2m-" + this.ToString(), null, 10, 0, 0, EFCON_TYPE.NORMAL);
				this.TeCon.is_player = this is M2MoverPr;
			}
			else
			{
				this.TeCon.clearRegistered();
			}
			if (this.NoDamage == null)
			{
				this.NoDamage = new M2NoDamageManager(_Mp);
			}
			else
			{
				this.NoDamage.initS(_Mp);
			}
			this.TeCon.layer_effect_bottom = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_bottom : base.gameObject.layer);
			this.TeCon.layer_effect_top = ((this.Mp.Dgn != null) ? this.Mp.Dgn.effect_layer_top : base.gameObject.layer);
			this.killPtc();
		}

		public override void deactivateFromMap()
		{
			base.deactivateFromMap();
			if (this.PtcHld != null)
			{
				this.PtcHld.endS();
			}
			if (this.TeCon != null)
			{
				this.TeCon.clearRegistered();
			}
		}

		public override void positionChanged(float prex, float prey)
		{
			base.positionChanged(prex, prey);
		}

		public override void runPre()
		{
			base.runPre();
		}

		public override void runPost()
		{
			base.runPost();
			if (base.destructed)
			{
				return;
			}
			if (this.FootD != null)
			{
				this.FootD.runPostAttakable(this);
			}
			if (base.destructed)
			{
				return;
			}
			if (this.TeCon != null)
			{
				this.TeCon.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, this.TS);
			}
			if (this.PtcHld != null && X.D)
			{
				this.PtcHld.checkUpdate(X.AF);
			}
		}

		public override void runPhysics(float fcnt)
		{
			if (this.PressCheck != null && this.Phy.main_updated_count >= 1 && !this.PressCheck.run())
			{
				this.PressCheck = null;
			}
			base.runPhysics(fcnt);
		}

		public virtual bool check_dangerous_bcc
		{
			get
			{
				return false;
			}
		}

		public virtual void checkBCCEvent(M2BlockColliderContainer.BCCLine Bcc, ref bool check_dangerous, float shiftx, float shifty)
		{
			shiftx += this.Phy.walk_xspeed * (base.hasFoot() ? 0.5f : 1f);
			if (Bcc.has_danger_chip & check_dangerous)
			{
				bool flag = this.Phy.getFootManager().get_FootBCC() == Bcc;
				float num = base.mleft + shiftx;
				float num2 = base.mtop + shifty;
				float num3 = base.mright + shiftx;
				float num4 = (flag ? base.mbottom : X.Mx(base.y, base.mbottom - 0.22f)) + shifty;
				if (Bcc.isCoveringXy(num, num2, num3, num4, 0.18f, -1000f))
				{
					for (int i = Bcc.AMapDmg.Count - 1; i >= 0; i--)
					{
						M2MapDamageContainer.M2MapDamageItem m2MapDamageItem = Bcc.AMapDmg[i];
						if (m2MapDamageItem.isCoveringXy(num, num2, num3, num4, 0.14f, (Bcc._xd != 0 || (!flag && Bcc.foot_aim == AIM.B)) ? (-0.23f) : (-1000f)))
						{
							float num5;
							float num6;
							AttackInfo atk = m2MapDamageItem.GetAtk(Bcc, this, out num5, out num6);
							if (atk != null)
							{
								this.applyDamageFromMap(m2MapDamageItem, atk, num5, num6, true);
								check_dangerous = false;
								break;
							}
						}
					}
				}
			}
			if (Bcc.ALsnRegistered != null && this.FootD != null)
			{
				int count = Bcc.ALsnRegistered.Count;
				for (int j = 0; j < count; j++)
				{
					Bcc.ALsnRegistered[j].runBCCEvent(Bcc, this.FootD);
				}
			}
		}

		protected override void OnCollisionStay2D(Collision2D col)
		{
			if (this.Mp.getTag(col.gameObject) == "Block" && this.pressdamage_applyable)
			{
				if (this.Phy != null && this.Phy.hasCollirderLock(col.collider))
				{
					return;
				}
				Component component;
				if (col.gameObject.TryGetComponent(typeof(IPresserBehaviour), out component))
				{
					IPresserBehaviour presserBehaviour = component as IPresserBehaviour;
					if (presserBehaviour.getPressAim(this) >= 0)
					{
						if (this.PressCheck == null)
						{
							this.PressCheck = new M2PressChecker(this);
						}
						this.PressCheck.addPresser(presserBehaviour, null);
					}
				}
			}
			base.OnCollisionStay2D(col);
		}

		public virtual bool pressdamage_applyable
		{
			get
			{
				return this.is_alive || this.overkill;
			}
		}

		public virtual void cureHp(int val)
		{
			this.hp = X.MMX(0, X.Mx(this.hp, 0) + val, this.maxhp);
		}

		public virtual void cureMp(int val)
		{
			this.mp = X.MMX(0, X.Mx(this.mp, 0) + val, this.maxmp);
		}

		public virtual int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (!force && this.applyHpDamageRatio(Atk) == 0f)
			{
				return 0;
			}
			bool is_alive = this.is_alive;
			val = (this.overkill ? val : X.Mn(val, this.hp));
			this.hp = X.Mx(this.hp - val, 0);
			if (is_alive && !this.is_alive && !this.initDeath())
			{
				this.hp = 1;
			}
			return val;
		}

		public virtual int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (!force && this.applyHpDamageRatio(Atk) == 0f)
			{
				return 0;
			}
			val = X.Mn(val, this.mp);
			this.mp -= val;
			return val;
		}

		public virtual int ratingHpDamageVal(float ratio)
		{
			return (int)X.Mx((float)this.maxhp * ratio, (float)((ratio > 0f && this.maxhp > 0) ? 1 : 0));
		}

		public virtual float applyHpDamageRatio(AttackInfo Atk)
		{
			return (float)((!this.NoDamage.isActive() && this.is_alive && !this.overkill) ? 1 : 0);
		}

		public bool isNoDamageActive()
		{
			return this.NoDamage.isActive();
		}

		public bool isNoDamageActive(NDMG key)
		{
			return this.NoDamage.isActive(key);
		}

		public virtual float applyMpDamageRatio(AttackInfo Atk)
		{
			return 1f;
		}

		public virtual AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo Atk, float efx, float efy, bool apply_execute = true)
		{
			return Atk;
		}

		public virtual bool applyPressDamage(IPresserBehaviour Press, int aim, out bool stop_carrier)
		{
			stop_carrier = true;
			return false;
		}

		public virtual void initTortureAbsorbPoseSet(string p, int set_frame = -1, int reset_animf = -1)
		{
			this.SpSetPose(p, reset_animf, null, false);
		}

		public virtual void quitTortureAbsorb()
		{
		}

		public virtual bool is_alive
		{
			get
			{
				return this.hp > 0;
			}
		}

		public abstract bool isDamagingOrKo();

		public virtual bool canApplyKnockBack()
		{
			return true;
		}

		public void addKnockBack(M2Attackable Target, AttackInfo Atk, float mpf)
		{
			if (Target == null || Target.Phy == null || this.Phy == null)
			{
				return;
			}
			float num = Atk.knockback_len;
			if (mpf == 0f)
			{
				mpf = (float)X.MPF(Target.x > base.x);
			}
			float num2 = 0f;
			float num3 = 0f;
			if (Target.weight < 0f || !Target.canApplyKnockBack() || Atk.knockback_ratio_t == 0f)
			{
				if (base.weight >= 0f && Atk.knockback_ratio_p > 0f)
				{
					num2 = -mpf * num;
				}
			}
			else if (base.weight < 0f || !this.canApplyKnockBack() || Atk.knockback_ratio_p == 0f)
			{
				num3 = mpf * num;
			}
			else
			{
				float num4 = ((base.weight >= 0f) ? (base.weight * Atk.knockback_ratio_p) : base.weight);
				float num5 = ((Target.weight >= 0f) ? (Target.weight * Atk.knockback_ratio_t) : Target.weight);
				float num6 = 0f;
				float num7 = 1f / (num4 + num5);
				float num9;
				float num8 = (num9 = mpf * num);
				Target.getMovableLen(ref num9, ref num6, -0.1f, false, false);
				float num10 = -num8;
				base.getMovableLen(ref num10, ref num6, -0.1f, false, false);
				num10 = X.Abs(num10);
				num9 = X.Abs(num9);
				num = X.Mn(num, num9 + num10);
				if (num10 < num9)
				{
					float num11 = X.Mn(num * num5 * num7, num10);
					num2 = -mpf * num11;
					num3 = X.Mn(mpf * (num - num11), num9);
				}
				else
				{
					float num12 = X.Mn(num * num4 * num7, num9);
					num3 = mpf * num12;
					num2 = X.Mn(-mpf * (num - num12), num10);
				}
			}
			for (int i = 0; i < 2; i++)
			{
				float num13 = ((i == 0) ? num2 : num3);
				if (num13 != 0f)
				{
					M2Attackable m2Attackable = ((i == 0) ? this : Target);
					float num14 = num13 / 22f * 1.35f;
					m2Attackable.addKnockbackVelocity(num14, Atk, (i == 0) ? Target : this, (FOCTYPE)0U);
				}
			}
		}

		public virtual void addKnockbackVelocity(float v0, AttackInfo Atk, M2Attackable Another, FOCTYPE _foctype_add = (FOCTYPE)0U)
		{
			this.Phy.addFoc(FOCTYPE.KNOCKBACK, v0, 0f, -4f, 0, 16, 5, -1, 0);
			this.Phy.addLockGravity(Another, 0.5f, 22f);
		}

		public float hp_ratio
		{
			get
			{
				return (float)X.Mx(this.hp, 0) / (float)this.maxhp;
			}
		}

		public float get_hp()
		{
			return (float)this.hp;
		}

		public float get_maxhp()
		{
			return (float)this.maxhp;
		}

		public float get_mp()
		{
			return (float)this.mp;
		}

		public float get_maxmp()
		{
			return (float)this.maxmp;
		}

		public float mp_ratio
		{
			get
			{
				if (this.maxmp > 0)
				{
					return (float)X.Mx(this.mp, 0) / (float)this.maxmp;
				}
				return 0f;
			}
		}

		public virtual bool initDeath()
		{
			if (this.TeCon != null)
			{
				this.TeCon.clearRegistered();
			}
			return true;
		}

		protected virtual void setDamageCounter(int delta_hp, int delta_mp, M2DmgCounterItem.DC ef = M2DmgCounterItem.DC.NORMAL, M2Attackable AbsorbedBy = null)
		{
			if (AbsorbedBy != null)
			{
				this.Mp.DmgCntCon.MakeAbsorb(this, AbsorbedBy, delta_hp, delta_mp, ef);
				return;
			}
			this.Mp.DmgCntCon.Make(this, delta_hp, delta_mp, ef, false);
		}

		public override void destruct()
		{
			if (this.TeCon != null)
			{
				this.TeCon.clearRegistered();
				this.TeCon.destruct();
				this.TeCon = null;
			}
			if (this.PtcHld != null)
			{
				this.PtcHld.killPtc(false);
				if (base.M2D.Snd != null)
				{
					base.M2D.Snd.kill(this.getSoundKey());
				}
			}
			base.destruct();
		}

		public virtual void defineParticlePreVariable()
		{
			this.PtcHld.first_ver = true;
			if (base.destructed)
			{
				this.PtcHld.Var("cx", (double)base.x);
				this.PtcHld.Var("cy", (double)base.y);
			}
			else
			{
				this.PtcHld.Var("cx", (double)(this.drawx * this.Mp.rCLEN));
				this.PtcHld.Var("cy", (double)(this.drawy * this.Mp.rCLEN));
			}
			this.PtcHld.Var("ax", (double)CAim._XD(this.aim, 1));
		}

		public virtual bool readPtcScript(PTCThread rER)
		{
			if (base.destructed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num > 1996746613U)
				{
					if (num > 2419201793U)
					{
						if (num <= 3072939085U)
						{
							if (num != 2491796050U)
							{
								if (num != 3041186419U)
								{
									if (num != 3072939085U)
									{
										goto IL_08D4;
									}
									if (!(cmd == "%MYPOS_SHAKE"))
									{
										goto IL_08D4;
									}
									rER.Def("cxs", base.x + X.XORSPS() * this.sizex * 0.7f);
									rER.Def("cys", base.y + X.XORSPS() * this.sizey * 0.7f);
									return true;
								}
								else
								{
									if (!(cmd == "%SND2"))
									{
										goto IL_08D4;
									}
									goto IL_0529;
								}
							}
							else if (!(cmd == "%QU_VIB2"))
							{
								goto IL_08D4;
							}
						}
						else if (num <= 3382953879U)
						{
							if (num != 3132573588U)
							{
								if (num != 3382953879U)
								{
									goto IL_08D4;
								}
								if (!(cmd == "%QU_SINV2"))
								{
									goto IL_08D4;
								}
								goto IL_084D;
							}
							else if (!(cmd == "%QU_VIB"))
							{
								goto IL_08D4;
							}
						}
						else if (num != 3472483256U)
						{
							if (num != 3543879655U)
							{
								goto IL_08D4;
							}
							if (!(cmd == "%TE_COLORBLINK"))
							{
								goto IL_08D4;
							}
							this.TeCon.setColorBlink(rER._N1, rER._N2, rER._N3, rER.Int(4, 16777215), rER.Int(5, 0));
							return true;
						}
						else
						{
							if (!(cmd == "%TE_QU_VIB"))
							{
								goto IL_08D4;
							}
							this.TeCon.setQuake(rER._N1, rER.Int(2, 0), rER._N3, rER.Int(4, 0));
							return true;
						}
						this.QuakeVib(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0), rER.cmd == "%QU_VIB2");
						return true;
					}
					if (num <= 2050094175U)
					{
						if (num != 2014939182U)
						{
							if (num != 2021943417U)
							{
								if (num != 2050094175U)
								{
									goto IL_08D4;
								}
								if (!(cmd == "%QU_SINV"))
								{
									goto IL_08D4;
								}
							}
							else
							{
								if (!(cmd == "%AIM"))
								{
									goto IL_08D4;
								}
								rER.Def("aim", (float)this.aim);
								rER.Def("ax", (float)CAim._XD(this.aim, 1));
								rER.Def("ay", (float)CAim._YD(this.aim, 1));
								return true;
							}
						}
						else
						{
							if (!(cmd == "%QU_HANDSHAKE2"))
							{
								goto IL_08D4;
							}
							goto IL_088F;
						}
					}
					else if (num != 2073177697U)
					{
						if (num != 2215071754U)
						{
							if (num != 2419201793U)
							{
								goto IL_08D4;
							}
							if (!(cmd == "%QU_SINH"))
							{
								goto IL_08D4;
							}
							goto IL_080B;
						}
						else
						{
							if (!(cmd == "%SND_TPOS"))
							{
								goto IL_08D4;
							}
							if (X.DEBUGNOSND)
							{
								return true;
							}
							Vector2 targetPos = this.getTargetPos();
							this.playSndPos(rER.getRandom(1, rER.clength - 1), targetPos.x, targetPos.y, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_T, null);
							return true;
						}
					}
					else
					{
						if (!(cmd == "%CALCPOS"))
						{
							goto IL_08D4;
						}
						this.getPtcCalc(rER);
						return true;
					}
					IL_084D:
					this.QuakeSinV(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0), rER.cmd == "%QU_SINV2");
					return true;
				}
				if (num <= 811585033U)
				{
					if (num <= 363198355U)
					{
						if (num != 27896808U)
						{
							if (num != 156884704U)
							{
								if (num != 363198355U)
								{
									goto IL_08D4;
								}
								if (!(cmd == "%SND"))
								{
									goto IL_08D4;
								}
							}
							else
							{
								if (!(cmd == "%MYPOS"))
								{
									goto IL_08D4;
								}
								rER.Def("cx", base.x);
								rER.Def("cy", base.y);
								return true;
							}
						}
						else
						{
							if (!(cmd == "%QU_HANDSHAKE"))
							{
								goto IL_08D4;
							}
							goto IL_088F;
						}
					}
					else if (num != 523093787U)
					{
						if (num != 541049905U)
						{
							if (num != 811585033U)
							{
								goto IL_08D4;
							}
							if (!(cmd == "%SND_CPOS"))
							{
								goto IL_08D4;
							}
							if (X.DEBUGNOSND)
							{
								return true;
							}
							this.PtcHld.playSndPos(rER, 1, rER.clength - 1, base.x, base.y, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, 1);
							return true;
						}
						else
						{
							if (!(cmd == "%SIZE"))
							{
								goto IL_08D4;
							}
							rER.Def("sizex", this.sizex);
							rER.Def("sizey", this.sizey);
							return true;
						}
					}
					else
					{
						if (!(cmd == "%TE_COLORBLINK_ADD"))
						{
							goto IL_08D4;
						}
						this.TeCon.setColorBlinkAdd(rER._N1, rER._N2, rER._N3, rER.Int(4, 16777215), rER.Int(5, 0));
						return true;
					}
				}
				else if (num <= 1363421846U)
				{
					if (num != 836404553U)
					{
						if (num != 1358799072U)
						{
							if (num != 1363421846U)
							{
								goto IL_08D4;
							}
							if (!(cmd == "%SND_ACT2"))
							{
								goto IL_08D4;
							}
						}
						else if (!(cmd == "%SND_ACT"))
						{
							goto IL_08D4;
						}
						if (X.DEBUGNOSND)
						{
							return true;
						}
						if (rER.clength >= 3 && rER.isNm(2))
						{
							this.PtcHld.playSndPos(rER, 1, 0, rER.Nm(2, 0f), rER.Nm(3, -1000f), PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, (rER.cmd == "%SND_ACT2") ? 2 : 1);
						}
						else
						{
							this.PtcHld.playSndPos(rER, 1, rER.clength - 1, base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, (rER.cmd == "%SND_ACT2") ? 2 : 1);
						}
						return true;
					}
					else
					{
						if (!(cmd == "%QU_SINH2"))
						{
							goto IL_08D4;
						}
						goto IL_080B;
					}
				}
				else if (num != 1487802141U)
				{
					if (num != 1750609613U)
					{
						if (num != 1996746613U)
						{
							goto IL_08D4;
						}
						if (!(cmd == "%SND_CONFUSE"))
						{
							goto IL_08D4;
						}
						if (X.DEBUGNOSND)
						{
							return true;
						}
						if (rER.clength >= 3 && rER.isNm(2))
						{
							this.PtcHld.playSndPos(rER, 1, 0, rER.Nm(2, 0f), rER.Nm(3, -1000f), PtcHolder.PTC_HOLD.CONFUSE, PTCThread.StFollow.FOLLOW_C, 1);
						}
						else
						{
							this.PtcHld.playSndPos(rER, 1, rER.clength - 1, base.x, base.y, PtcHolder.PTC_HOLD.CONFUSE, PTCThread.StFollow.FOLLOW_C, 1);
						}
						return true;
					}
					else
					{
						if (!(cmd == "%HOLD"))
						{
							goto IL_08D4;
						}
						PTCThread.StFollow stFollow;
						if (!FEnum<PTCThread.StFollow>.TryParse(rER._1, out stFollow, true))
						{
							rER.tError("不明な StFollow: " + rER._1);
						}
						else
						{
							this.PtcHld.changeCurrentBufferFollow(stFollow);
						}
						return true;
					}
				}
				else
				{
					if (!(cmd == "%TARGETPOS"))
					{
						goto IL_08D4;
					}
					Vector2 targetPos2 = this.getTargetPos();
					rER.Def("tx", targetPos2.x);
					rER.Def("ty", targetPos2.y);
					return true;
				}
				IL_0529:
				if (X.DEBUGNOSND)
				{
					return true;
				}
				if (rER.clength >= 3 && rER.isNm(2))
				{
					this.PtcHld.playSndPos(rER, 1, 0, rER.Nm(2, 0f), rER.Nm(3, -1000f), PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.FOLLOW_C, (rER.cmd == "%SND2") ? 2 : 1);
				}
				else
				{
					this.PtcHld.playSndPos(rER, 1, rER.clength - 1, base.x, base.y, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.NO_FOLLOW, (rER.cmd == "%SND2") ? 2 : 1);
				}
				return true;
				IL_080B:
				this.QuakeSinH(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0), rER.cmd == "%QU_SINH2");
				return true;
				IL_088F:
				this.QuakeHandShake(rER.Nm(1, 0f), rER.Nm(2, 0f), rER.Nm(3, 1f), rER.Int(4, 0), rER.cmd == "%QU_HANDSHAKE2");
				return true;
			}
			IL_08D4:
			return this.Mp.M2D.readPtcScript(rER);
		}

		public virtual bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		public M2Attackable PtcVar(string key, double v)
		{
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.Var(key, v);
			return this;
		}

		public M2Attackable PtcVarS(string key, string v)
		{
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.Var(key, v);
			return this;
		}

		public M2Attackable PtcVarS(string key, MGATTR v)
		{
			return this.PtcVarS(key, FEnum<MGATTR>.ToStr(v));
		}

		public PTCThread PtcST(string ptc_key, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			if (this.PtcHld == null)
			{
				return null;
			}
			return this.PtcHld.PtcST(ptc_key, hold, follow);
		}

		public PTCThread PtcSTT(string ptc_key, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			if (this.PtcHld == null)
			{
				return null;
			}
			return this.PtcHld.PtcSTT(ptc_key, hold, follow);
		}

		public void killPtc()
		{
			if (this.PtcHld != null)
			{
				this.PtcHld.killPtc(false);
			}
		}

		public void killPtc(string s, bool kill_only_reader = false)
		{
			if (this.PtcHld != null)
			{
				this.PtcHld.killPtc(s, kill_only_reader);
			}
		}

		public void killPtc(PtcHolder.PTC_HOLD _hold)
		{
			if (this.PtcHld != null)
			{
				this.PtcHld.killPtc(_hold);
			}
		}

		public M2SoundPlayerItem playSndPos(string cue_key, float x, float y, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, PTCThread Thread = null)
		{
			if (this.PtcHld == null)
			{
				return null;
			}
			return this.PtcHld.playSndPos(cue_key, x, y, hold, follow, Thread, 1);
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && this.Mp != null && this.PtcHld != null && this.PtcHld.isSoundActive(m2SoundPlayerItem);
		}

		public virtual bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			if (base.destructed)
			{
				V = new Vector3(base.x, base.y, 0f);
				return false;
			}
			float num = CAim.get_agR(this.aim, 0f);
			switch (follow)
			{
			case PTCThread.StFollow.NO_FOLLOW:
				V = Vector2.zero;
				return false;
			case PTCThread.StFollow.FOLLOW_T:
				V = this.getTargetPos();
				V.z = num;
				return true;
			case PTCThread.StFollow.FOLLOW_HEAD:
				V = new Vector3(this.drawx * this.Mp.rCLEN, this.drawy * this.Mp.rCLEN - this.sizey * 0.4f, num);
				return true;
			}
			V = new Vector3(this.drawx * this.Mp.rCLEN, this.drawy * this.Mp.rCLEN, num);
			return true;
		}

		public string getSoundKey()
		{
			return base.key;
		}

		protected virtual void getPtcCalc(PTCThread rER)
		{
			if (base.destructed)
			{
				rER.Def("x", base.x);
				rER.Def("y", base.y);
			}
			else
			{
				rER.Def("x", this.drawx * this.Mp.rCLEN);
				rER.Def("y", this.drawy * this.Mp.rCLEN);
			}
			rER.Def("vx", base.vx);
			rER.Def("vy", base.vy);
			rER.Def("aim_agR", CAim.get_agR(this.aim, 0f));
			rER.Def("raim_agR", CAim.get_agR(this.aim, 0f) + 3.1415927f);
			rER.Def("foot_exist", (float)(base.hasFoot() ? 1 : 0));
			rER.Def("mleft", base.mleft);
			rER.Def("mtop", base.mtop);
			rER.Def("mright", base.mright);
			rER.Def("mbottom", base.mbottom);
		}

		protected float quake_level
		{
			get
			{
				return this.getQuakeLevel(1f, 0f);
			}
		}

		protected float getQuakeLevel(float max = 1f, float min = 0f)
		{
			if (base.destructed)
			{
				return 0f;
			}
			M2Camera cam = base.M2D.Cam;
			if (cam.isBaseMover(this))
			{
				return 1f;
			}
			return min + (max - min) * X.Pow(1f - X.ZLINE(X.LENGTHXYN(base.x, base.y, cam.map_center_x, cam.map_center_y) - 8f, 8f), 2);
		}

		public M2Attackable QuakeVib(float _slevel, float _time, float _elevel = -1f, int _saf = 0, bool ignore_qlevel = false)
		{
			float num = (ignore_qlevel ? 1f : this.quake_level);
			if (num > 0f)
			{
				base.M2D.Cam.Qu.Vib(_slevel * num, _time, (_elevel < 0f) ? _elevel : (_elevel * num), _saf);
			}
			return this;
		}

		public M2Attackable QuakeSinH(float _slevel, float _time, float _elevel = -1f, int _saf = 0, bool ignore_qlevel = false)
		{
			float num = (ignore_qlevel ? 1f : this.quake_level);
			if (num > 0f)
			{
				base.M2D.Cam.Qu.SinH(_slevel * num, _time, (_elevel < 0f) ? _elevel : (_elevel * num), _saf);
			}
			return this;
		}

		public M2Attackable QuakeSinV(float _slevel, float _time, float _elevel = -1f, int _saf = 0, bool ignore_qlevel = false)
		{
			float num = (ignore_qlevel ? 1f : this.quake_level);
			if (num > 0f)
			{
				base.M2D.Cam.Qu.SinV(_slevel * num, _time, (_elevel < 0f) ? _elevel : (_elevel * num), _saf);
			}
			return this;
		}

		public M2Attackable QuakeHandShake(float _holdtime, float _fadetime, float _level, int _saf = 0, bool ignore_qlevel = false)
		{
			float num = (ignore_qlevel ? 1f : this.quake_level);
			if (num > 0f)
			{
				base.M2D.Cam.Qu.HandShake((float)((int)_holdtime), _fadetime, _level * num, _saf);
			}
			return this;
		}

		protected override bool noHitableAttack()
		{
			return false;
		}

		public abstract HITTYPE getHitType(M2Ray Ray);

		public virtual RAYHIT can_hit(M2Ray Ray)
		{
			if (!this.is_alive && !this.overkill)
			{
				return RAYHIT.NONE;
			}
			return (RAYHIT)3;
		}

		public void penetrateNoDamageTime(NDMG key, int reduce = -1)
		{
			this.NoDamage.Penetrate(key, reduce);
		}

		public void addNoDamage(NDMG key, float t)
		{
			this.NoDamage.Add(key, t);
		}

		public virtual Vector2 getTargetPos()
		{
			return new Vector2(base.x + (float)CAim._XD(this.aim, 1) * this.sizex * 0.85f, base.y);
		}

		public virtual Vector2 getDamageCounterShiftMapPos()
		{
			return new Vector2(0f, -X.Mx(this.sizey * 0.7f, 0.9f));
		}

		public virtual void setWaterReleaseEffect(int water_id)
		{
		}

		public virtual bool setWaterDunk(int water_id, int misttype)
		{
			return true;
		}

		public override void runMoveScript()
		{
			if (!this.MScr.run(false))
			{
				if (!this.MScr.lock_remove_ms || !EV.isActive(false))
				{
					this.MScr = null;
				}
				if (this.Phy != null)
				{
					this.Phy.fineGravityScale();
				}
			}
		}

		public override M2Mover assignMoveScript(string str, bool soft_touch = false)
		{
			base.assignMoveScript(str, soft_touch);
			if (this.isDamagingOrKo() && this.Phy != null)
			{
				this.Phy.remFoc(FOCTYPE.HIT | FOCTYPE.ABSORB | FOCTYPE.DAMAGE, true);
				this.Phy.clampSpeed(FOCTYPE.DAMAGE, 0f, 0f, 1f);
			}
			return this;
		}

		protected int hp = -1000;

		protected int maxhp;

		protected int mp = -1000;

		protected int maxmp;

		protected int knockback_time;

		protected M2NoDamageManager NoDamage;

		public bool overkill;

		public TransEffecter TeCon;

		public PtcHolder PtcHld;

		protected M2PressChecker PressCheck;

		public const int KNOCKBACK_TIME_DEFAULT = 22;

		public enum MAPDAMAGE
		{
			NONE,
			RECHECK,
			RECHECK_ONLY_FOOT
		}
	}
}
