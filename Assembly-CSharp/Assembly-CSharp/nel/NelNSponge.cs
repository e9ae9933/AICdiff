using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class NelNSponge : NelEnemy
	{
		public static void FlushPxlData()
		{
			if (NelNSponge.Olayer2pos != null)
			{
				NelNSponge.Olayer2pos.Clear();
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			if (NelNSponge.Olayer2pos == null)
			{
				NelNSponge.Olayer2pos = new BDic<PxlFrame, int>();
				NelNSponge.PFStone = MTRX.getPF("sponge_shot");
			}
			this.OMgTarget = new BDic<MagicItem, NelEnemy>(2);
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			ENEMYID id = this.id;
			this.id = ENEMYID.SPONGE_0;
			NOD.BasicData basicData = NOD.getBasicData("SPONGE_0");
			base.base_gravity = 0.33f;
			base.appear(_Mp, basicData);
			this.EnemyLock = new FlagCounterR<NelEnemy>();
			this.enlarge_maximize_mp_ratio = 1f;
			this.shot_consume = (int)((float)this.maxmp * this.enlarge_maximize_mp_ratio / 8f);
			this.enlarge_splice_count = 8;
			this.cannot_move = true;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 19f;
			this.Nai.attackable_length_top = -6f;
			this.Nai.attackable_length_bottom = 6f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.absorb_weight = 1;
			this.Anm.fnFineFrame = new EnemyAnimator.FnFineFrame(this.fnFineSpongeFrame);
			base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.FD_MgRunNormalShot = new MagicItem.FnMagicRun(this.MgRunNormalShot);
			this.FD_MgDrawNormalShot = new MagicItem.FnMagicRun(NelNSponge.MgDrawNormalShot);
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			this.EnemyLock.run(this.TS);
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			if (!Nai.hasPriorityTicket(200, false, false))
			{
				if (this.mp >= this.shot_consume + this.TkiAttack.mp_consume + 8 && !Nai.hasTypeLock(NAI.TYPE.PUNCH) && Nai.target_sxdif <= 2.5f + Nai.RANtk(482) * 1.2f && Nai.isTargetYCovering(X.NI(0.33f, 1f, this.enlarge_level - 1f) * this.TkiAttack.calc_dify_map(this) - this.TkiAttack.radius + 0.2f, 3f, false))
				{
					bool flag = Nai.HasF(NAI.FLAG.ATTACKED, false);
					if (Nai.RANtk(3477) < (flag ? 0.85f : (0.2f + ((Nai.isPrMagicChanting(1f) || Nai.isPrAttacking()) ? 0.45f : 0f))))
					{
						Nai.AddTicket(NAI.TYPE.PUNCH, 200, true);
						return true;
					}
				}
				if (!Nai.hasPriorityTicket(0, false, false) && this.mp >= this.shot_consume && this.enlarge_level > 1f && this.Summoner != null && Nai.RANtk(4138) < 0.4f && !Nai.hasTypeLock(NAI.TYPE.MAG))
				{
					NelEnemy nelEnemy = this.findSupplyTarget();
					if (!(nelEnemy == null))
					{
						Nai.AddTicket(NAI.TYPE.MAG, 200, true).Dep(nelEnemy.x, nelEnemy.y, null);
						return true;
					}
					Nai.addTypeLock(NAI.TYPE.MAG, 70f);
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private NelEnemy findSupplyTarget()
		{
			float num = -1f;
			NelEnemy nelEnemy = null;
			List<List<NelEnemy>> aasummoned = this.Summoner.getAASummoned();
			for (int i = aasummoned.Count - 1; i >= 0; i--)
			{
				List<NelEnemy> list = aasummoned[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					NelEnemy nelEnemy2 = list[j];
					float num2 = this.calcScore(nelEnemy2);
					if (num2 >= 0f && (num < 0f || num2 < num))
					{
						nelEnemy = nelEnemy2;
						num = num2;
					}
				}
			}
			return nelEnemy;
		}

		private float calcScore(NelEnemy N)
		{
			if (this.EnemyLock.Has(N) || N is NelNSponge || N.destructed || !N.is_alive || N.get_mp() >= N.get_maxmp() - (float)this.shot_consume)
			{
				return -1f;
			}
			float num = X.LENGTH_SIZE(base.x, this.sizex, N.x, N.sizex);
			if (this.Nai.attackable_length_x < num)
			{
				return -1f;
			}
			float num2 = X.LENGTH_SIZE(base.y, this.sizey, N.y, N.sizey);
			if (!X.BTW(this.Nai.attackable_length_top, num2 * (float)X.MPF(base.y < N.y), this.Nai.attackable_length_bottom))
			{
				return -1f;
			}
			return num + num2 + N.hp_ratio * 4f + N.mp_ratio * 8f;
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			if (Tk != null && Tk.type == NAI.TYPE.PUNCH)
			{
				this.Phy.quitSoftFall(100f);
				base.killPtc("sponge_attack_act", true);
				this.Phy.walk_xspeed = 0f;
			}
			if (this.SndLoopAtk != null)
			{
				this.SndLoopAtk.destruct();
				this.SndLoopAtk = null;
			}
			base.remF((NelEnemy.FLAG)6291456);
		}

		public override void destruct()
		{
			base.destruct();
			if (this.SndLoopAtk != null)
			{
				this.SndLoopAtk.destruct();
				this.SndLoopAtk = null;
			}
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.PUNCH)
			{
				if (type != NAI.TYPE.WALK)
				{
					if (type != NAI.TYPE.PUNCH)
					{
						goto IL_0076;
					}
					return this.runSpongeAttack(Tk.initProgress(this), Tk);
				}
			}
			else
			{
				if (type == NAI.TYPE.MAG)
				{
					return this.runSpongeShotMag(Tk.initProgress(this), Tk);
				}
				if (type - NAI.TYPE.GAZE > 1)
				{
					goto IL_0076;
				}
			}
			base.AimToLr((X.xors(2) == 0) ? 0 : 2);
			Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
			return false;
			IL_0076:
			return base.readTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && (Mg.kind == MGKIND.BASIC_SHOT || (Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle));
		}

		protected override bool runSummoned()
		{
			if (this.t <= 0f)
			{
				float footableY = this.Mp.getFootableY(base.x, (int)(base.mbottom - 0.125f), 12, true, -1f, false, true, true, 0f);
				this.moveBy(0f, footableY - base.mbottom - 0.004f, true);
			}
			if (this.t >= 60f && this.walk_st == 0)
			{
				this.SpSetPose("awake", -1, null, false);
			}
			return base.runSummoned() || this.t < 60f + X.NI(33, 66, this.Nai.RANn(5690));
		}

		public bool runSpongeShotMag(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("throw_1", -1, null, false);
				base.PtcST("sponge_shot_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 90, true))
			{
				NelEnemy nelEnemy = Tk.TargetObject as NelEnemy;
				if (nelEnemy == null || this.calcScore(nelEnemy) < 0f)
				{
					nelEnemy = this.findSupplyTarget();
				}
				if (nelEnemy != null)
				{
					MagicItem magicItem = this.MagicInitialize(base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE), nelEnemy);
					base.PtcVar("agR", (double)magicItem.aim_agR).PtcVar("cy", (double)magicItem.sy).PtcST("sponge_shot_release", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Nai.addTypeLock(NAI.TYPE.MAG, 160f);
				}
				else
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 250f);
				}
				this.SpSetPose("throw_2", -1, null, false);
				Tk.after_delay = 90f;
				return false;
			}
			return true;
		}

		private MagicItem MagicInitialize(MagicItem Mg, NelEnemy Target)
		{
			Mg.initFunc(this.FD_MgRunNormalShot, this.FD_MgDrawNormalShot);
			Mg.Atk0 = this.AtkShotHit;
			Mg.efpos_s = (Mg.raypos_s = true);
			Mg.Ray.RadiusM(0.14f).HitLock(40f, null);
			Mg.Ray.projectile_power = 10;
			Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
			Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true);
			Mg.calced_aim_pos = false;
			Mg.phase = 1;
			Mg.mp_crystalize = 0f;
			Mg.sy = base.y - this.sizey * 0.4f;
			Mg.createDropper(X.absMn((Target.x - base.x) / 90f, 0.08f), X.NIXP(-0.08f, -0.15f), 0.3f, -1f, -1f);
			Mg.Dro.gravity_scale = 0.07f;
			Mg.Dro.bounce_x_reduce = 0.7f;
			Mg.Dro.bounce_y_reduce = 0.6f;
			this.OMgTarget[Mg] = Target;
			Mg.calcAimPos(false);
			int num = base.splitMyMana(this.shot_consume, 0f, 0);
			if (num > 0)
			{
				Mg.reduce_mp = X.IntC((float)num * 0.4f);
			}
			Mg.PtcST("sponge_shot_splash", PTCThread.StFollow.NO_FOLLOW, false);
			this.EnemyLock.Add(Target, 160f);
			return Mg;
		}

		public bool MgRunNormalShot(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				return Mg.t < 60f;
			}
			float num = 2f * X.ZLINE(20f - Mg.t, 20f);
			if (num > 0f)
			{
				Mg.Dro.x += Mg.Dro.vx * num * fcnt;
			}
			if (Mg.Dro.on_ground || Mg.Dro.y_bounced)
			{
				if (Mg.Dro.y_bounced)
				{
					Mg.PtcST("sponge_shot_bounce", PTCThread.StFollow.NO_FOLLOW, false);
					if (this.Mp != null)
					{
						NelEnemy nelEnemy = X.Get<MagicItem, NelEnemy>(this.OMgTarget, Mg);
						if (nelEnemy != null)
						{
							if (X.Abs(Mg.sx - nelEnemy.x) < 1.5f)
							{
								Mg.phase = 100;
							}
							else if (Mg.Dro.vx > 0f != Mg.sx < nelEnemy.x)
							{
								Mg.Dro.vx *= -1f;
							}
						}
						Mg.Dro.vx *= 0.85f;
					}
				}
				int phase = Mg.phase;
				Mg.phase = phase + 1;
				if (phase >= 3)
				{
					Mg.phase = 100;
				}
			}
			Mg.calcAimPos(false);
			Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
			Mg.Ray.LenM(0.015f);
			if ((Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir(Mg.Dro.vx), HITTYPE.NONE) & (HITTYPE)4292640) != HITTYPE.NONE || Mg.t >= 160f)
			{
				Mg.phase = 100;
			}
			if (Mg.phase >= 100)
			{
				Mg.Ray.RadiusM(1.1f);
				Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir(Mg.Dro.vx), HITTYPE.NONE);
				Mg.PtcST("sponge_shot_explode", PTCThread.StFollow.NO_FOLLOW, false);
				base.nM2D.Mana.AddMulti(Mg.sx, Mg.sy - 0.5f, (float)Mg.reduce_mp, (MANA_HIT)8202);
				return false;
			}
			return true;
		}

		public static bool MgDrawNormalShot(MagicItem Mg, float fcnt)
		{
			Map2d mp = Mg.Mp;
			BLEND blend;
			uint num;
			switch (X.ANMT(4, 3f))
			{
			case 1:
			case 3:
				blend = BLEND.SUB;
				num = 4282280576U;
				break;
			case 2:
				blend = BLEND.NORMAL;
				num = 4294957282U;
				break;
			default:
				blend = BLEND.ADD;
				num = 4284880165U;
				break;
			}
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, blend, false);
			object obj = ((blend == BLEND.SUB) ? meshImg : Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false));
			float num2 = 0.5f + 0.33f * X.COSI(mp.floort, 11.3f) + 0.04f * X.COSI(mp.floort, 2.9f);
			float num3 = 1f + X.COSI(mp.floort, 1.57f) * 0.13f + X.COSI(mp.floort, 4.13f) * 0.13f;
			object obj2 = obj;
			obj2.Col = obj2.ColGrd.Set(4281588422U).blend(4280697453U, num2).C;
			obj2.initForImg(MTRX.EffBlurCircle245, 0).Rect(0f, 0f, 66f * num3, 66f * num3, false);
			meshImg.Col = C32.d2c(num);
			Mg.calcAimPos(false);
			meshImg.RotaPF(0f, 0f, num3, num3, Mg.aim_agR, NelNSponge.PFStone, false, false, false, uint.MaxValue, false, 0);
			MeshDrawer mesh = Mg.Ef.GetMesh("sponge_mdc", uint.MaxValue, BLEND.NORMAL, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 0.1f;
				mesh.Col = mesh.ColGrd.Set(4294901760U).blend(2916365698U, 0.5f + 0.3f * X.COSI(mp.floort, 23f)).C;
				uint ran = X.GETRAN2(Mg.id, 13);
				for (int i = 0; i < 2; i++)
				{
					float num4 = X.RAN(ran, 1839 + i * 431) * 6.2831855f;
					float num5 = X.NI(50, 90, X.RAN(ran, 502 + i * 94)) / 6.2831855f;
					float num6 = X.RAN(ran, 1771 + i * 344) * 6.2831855f;
					float num7 = X.NI(50, 90, X.RAN(ran, 435 + i * 88)) / 6.2831855f;
					mesh.Identity().Scale(1f, X.Abs(X.Cos(num6 + mp.floort / num7)), false).Rotate(num4 + mp.floort / num5, false);
					mesh.Poly(0f, 0f, 45f * num3, 0f, 20, 2f, false, 0f, 0f);
				}
			}
			return true;
		}

		public bool runSpongeAttack(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				base.addF((NelEnemy.FLAG)6291456);
				this.SpSetPose("attack", -1, null, false);
				base.PtcVar("fy", (double)base.mbottom).PtcST("sponge_attack_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (this.SndLoopAtk != null)
				{
					this.SndLoopAtk.destruct();
				}
				if (!X.DEBUGNOSND)
				{
					this.SndLoopAtk = this.Mp.M2D.Snd.createInterval(this.snd_key, "sponge_atk_vo", 50f, this, 0f, 128).Change("sponge_atk_vo_loop", 33f);
				}
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 20, true))
			{
				float dify_map = this.TkiAttack.dify_map;
				this.TkiAttack.dify_map *= X.NI(0.33f, 1f, this.enlarge_level - 1f);
				base.tackleInit(this.AtkAttack, this.TkiAttack);
				base.PtcVar("reach", (double)this.TkiAttack.calc_dify_map(this)).PtcVar("scl", (double)this.Anm.scaleX).PtcST("sponge_attack_act", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.TkiAttack.dify_map = dify_map;
				this.Phy.initSoftFall(0.14f, 0f);
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.AtkAttack.burst_vx *= -1f;
				if (Tk.Progress((base.hasFoot() && this.t >= 150f) || base.AimPr == null))
				{
					if (this.SndLoopAtk != null)
					{
						this.SndLoopAtk.play_count = 0;
						this.SndLoopAtk = null;
					}
					this.t = 0f;
					base.killPtc("sponge_attack_act", true);
					this.can_hold_tackle = false;
					this.SpSetPose("attack2stand", -1, null, false);
				}
				else
				{
					this.Phy.walk_xspeed = X.VALWALK(this.Phy.walk_xspeed, 0.032f * (float)X.MPF(base.x < this.Nai.target_x), 0.0005f);
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.Phy.walk_xspeed = X.VALWALK(this.Phy.walk_xspeed, 0f, 0.0005f);
				if (this.t >= 80f)
				{
					this.Phy.quitSoftFall(100f);
					base.remF((NelEnemy.FLAG)6291456);
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 180f);
					return false;
				}
			}
			return true;
		}

		public void fnFineSpongeFrame(EnemyFrameDataBasic nF, PxlFrame F)
		{
			int num = -1;
			int num2 = F.countLayers();
			if (!NelNSponge.Olayer2pos.TryGetValue(F, out num))
			{
				int num3 = -1;
				for (int i = 0; i < num2; i++)
				{
					PxlLayer layer = F.getLayer(i);
					if (layer.name == "Layer_2")
					{
						num3 = i;
						if (num2 - num3 < 16)
						{
							X.de("レイヤー個数配置エラー: " + F.ToString(), null);
							num3 = -2;
							break;
						}
					}
					else if (num3 >= 0)
					{
						int num4 = i - num3;
						if (num4 >= 8)
						{
							num4 -= 8;
							if (num4 >= 8)
							{
								break;
							}
							num4 += 2;
							if (layer.name != "eye_" + num4.ToString())
							{
								X.de("eye レイヤー配置エラー: " + layer.ToString(), null);
								num3 = -2;
								break;
							}
						}
						else if (layer.name != "Layer_" + (num4 + 2).ToString())
						{
							X.de("Layer レイヤー配置エラー: " + layer.ToString(), null);
							num3 = -2;
							break;
						}
					}
				}
				num = (NelNSponge.Olayer2pos[F] = num3);
			}
			if (num < 0)
			{
				this.Anm.layer_mask = uint.MaxValue;
				return;
			}
			uint num5 = 0U;
			int num6 = X.IntR((this.enlarge_level - 1f) * 8f);
			for (int j = 0; j < num2; j++)
			{
				if (j < num)
				{
					num5 |= 1U << j;
				}
				else
				{
					int num7 = j - num;
					if (num7 >= 16)
					{
						num5 |= 1U << j;
					}
					else
					{
						num7 = NelNSponge.Alayer_link[num7 % 8];
						if (num7 <= num6)
						{
							num5 |= 1U << j;
						}
					}
				}
			}
			this.Anm.layer_mask = num5;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (this.enlarge_level == 1f || (Atk != null && Atk.isPlayerShotgun()))
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
			}
			int num = base.applyDamage(Atk, force);
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			return num;
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (nelAttackInfo.ndmg == NDMG.MAPDAMAGE)
			{
				return null;
			}
			if (!apply_execute)
			{
				return nelAttackInfo;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			if (this.applyDamage(nelAttackInfo, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * (base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL) ? 1.4f : 1f);
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.SpSetPose("damage", 1, null, false);
				base.addF(NelEnemy.FLAG.HAS_SUPERARMOR);
			}
			if (!base.runDamageSmall())
			{
				this.Nai.AddF(NAI.FLAG.ATTACKED, 50f);
				base.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
				return false;
			}
			return true;
		}

		public override void addHpFromMana(M2Mana Mana, float val)
		{
			if (!Mana.from_supplier)
			{
				base.addHpFromMana(Mana, val);
			}
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.CUT_H);
		}

		public override float getEnlargeLevel()
		{
			float mp_ratio = base.mp_ratio;
			return 1f + (float)((int)(X.ZLINE(mp_ratio, this.enlarge_maximize_mp_ratio) * 8f)) / 8f;
		}

		protected NelAttackInfo AtkShotHit = new NelAttackInfo
		{
			hpdmg0 = 9,
			burst_vx = 0.03f,
			huttobi_ratio = -100f,
			parryable = true
		}.Torn(0.005f, 0.01f);

		protected NelAttackInfo AtkAttack = new NelAttackInfo
		{
			hpdmg0 = 2,
			mpdmg0 = 5,
			split_mpdmg = 2,
			burst_vx = 0.17f,
			huttobi_ratio = 0.02f,
			attr = MGATTR.CUT_H,
			SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 30f),
			Beto = BetoInfo.TORNADO,
			parryable = false
		}.Torn(0.03f, 0.11f);

		private NOD.TackleInfo TkiAttack = NOD.getTackle("sponge_attack");

		private const int ball_count = 8;

		private static readonly int[] Alayer_link = new int[] { 1, 6, 7, 3, 5, 8, 4, 2 };

		private static BDic<PxlFrame, int> Olayer2pos;

		private static PxlFrame PFStone;

		private BDic<MagicItem, NelEnemy> OMgTarget;

		public int shot_consume;

		public const float shot_supply_ratio = 0.4f;

		private FlagCounterR<NelEnemy> EnemyLock;

		private M2SndInterval SndLoopAtk;

		private const float attack_walk_xspeed = 0.032f;

		private const float attack_walk_xspeed_accel = 0.0005f;

		private const float attack_softfall = 0.14f;

		private const float reachable_min = 0.33f;

		private const float shotgun_damage_ratio = 1.4f;

		private MagicItem.FnMagicRun FD_MgRunNormalShot;

		private MagicItem.FnMagicRun FD_MgDrawNormalShot;

		private const int PRI_WALK = 200;

		public const float shot_gravity_scale = 0.07f;
	}
}
