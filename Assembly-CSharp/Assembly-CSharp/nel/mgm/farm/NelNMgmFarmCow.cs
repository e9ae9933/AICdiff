using System;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.farm
{
	public class NelNMgmFarmCow : MvNelNNEAListener.NelNNpcEventAssign, IEventWaitListener
	{
		public float disturb_mem
		{
			get
			{
				return this.disturb_mem_;
			}
			set
			{
				if (value > 0f)
				{
					this.disturb_mem_ = value;
				}
				this.disturb_mem_ = value;
			}
		}

		public override void Awake()
		{
			base.Awake();
			this.Anm.normalrender_header = "nml";
			this.Anm.addAdditionalDrawer(new EnemyAnimator.EnemyAdditionalDrawFrame(null, (MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead) => this.fnChangeColoringTag(), true));
		}

		public override void appear(Map2d _Mp)
		{
			this.mover_hitable = (this.wall_hitable = true);
			base.appear(_Mp);
			this.throw_ray_magic_attack = (this.throw_ray_normal_attack = false);
			this.Weed.Clear();
			this.Nai.considerable_in_event = true;
			this.Nai.awake_length = 1000f;
			this.return_back_length = -1f;
			this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
			int num;
			switch (this.cow_index)
			{
			case 1:
				num = 182087195;
				break;
			case 2:
				num = 36728255;
				break;
			case 3:
				num = 767997549;
				break;
			case 4:
				num = 1856000515;
				break;
			case 5:
				num = 2086376659;
				break;
			case 6:
				num = 1726826941;
				break;
			case 7:
				num = 1994287725;
				break;
			case 8:
				num = 20871359;
				break;
			case 9:
				num = 1158769106;
				break;
			default:
				num = 1357719140;
				break;
			}
			this.Nai.ran_n = (uint)num;
			this.mp = 0;
			this.col_od_blink_color = 4285857747U;
			this.Anm.add_color_eye_fade_out = 7667667U;
			this.Anm.base_rotate_shuffle360 = 0.06f;
			this.Anm.eye_randomize_level = 0.15f;
			this.Anm.od_blink_extend_level = 1f;
			this.Anm.scale_shuffle01 = 0.002f;
			this.Anm.eye_fade_type = EnemyAnimator.EYE_FADE_TYPE.ZPOWV;
			this.Anm.def_mul_color = MTRX.cola.Set(2287984718U).blend(2294202504U, this.Nai.RANn(4388)).rgba;
			this.red_eye_blink = true;
			this.sink_ratio = 9000f;
			base.remF(NelEnemy.FLAG.CHECK_ENLARGE);
			base.remF((NelEnemy.FLAG)5242880);
			float num2 = this.NIn(this.areax + 2.2f, this.arear - 2.2f, 918);
			float footableY = this.Mp.getFootableY(num2, this.BelongTo.mapy, this.BelongTo.maph, true, (float)(this.BelongTo.mapy + this.BelongTo.maph), false, true, true, this.sizex);
			this.positionReset(false, true, false);
			this.prepareHpMpBarMesh();
			this.fineEnlargeScale(-1f, false, false);
			this.AttachEvent.Size(this.sizex * base.CLEN * 1.8f, this.sizey * base.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			this.setTo(num2, footableY - this.sizey);
			if (this.CBoard.a == 0)
			{
				this.CBoard = MTRX.cola.White().colorShift(this.NIn(0.5f, 1f, 1881), this.NIn(0.5f, 1f, 2789), this.NIn(0.5f, 1f, 2103), 1f, 0f, 0f, 0f, 0f).C;
			}
			this.fineCheckDesc();
			this.absorb_weight = 2;
		}

		public void positionReset(bool pos = true, bool aim = true, bool _state = true)
		{
			if (aim)
			{
				this.setAim((this.NIn(0f, 1f, 188) < 0.5f) ? AIM.R : AIM.L, true);
			}
			if (pos)
			{
				this.setToDefaultPosition(false, null);
			}
			if (_state)
			{
				this.Nai.clearTicket(-1, true);
				this.changeState(NelEnemy.STATE.STAND);
				this.mp = 0;
				this.hp = this.maxhp;
				this.disturb_mem = 0f;
				this.disturb_to_attack_mem_ = 0f;
				this.fineEnlargeScale(-1f, false, false);
				base.remF(NelEnemy.FLAG.CHECK_ENLARGE);
				this.prepareHpMpBarMesh();
				this.Nai.clearTypeLock();
				base.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
				this.Nai.RemFMulti(NAI.FLAG._ADDITIONAL);
				this.setExecutable(true);
				this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
				if (this.EfHkds != null)
				{
					this.EfHkds.destruct();
					this.EfHkds = null;
				}
			}
		}

		protected override NOD.BasicData initializeFirst(string npc_key)
		{
			return base.initializeFirst(npc_key ?? "NPC_ENEMY_MGMFARM_COW");
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (state == NelEnemy.STATE.DAMAGE)
			{
				this.disturb_to_attack_mem_ += 1f;
			}
			if (state == NelEnemy.STATE.ABSORB)
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 640f);
			}
			return this;
		}

		public override void changeStateToDie()
		{
			this.hp = 1;
		}

		public override void destruct()
		{
			if (this.BelongTo != null)
			{
				this.BelongTo.removeCow(this);
			}
			base.destruct();
		}

		private float WEED_MARGIN_X
		{
			get
			{
				return 1.5f * this.Anm.scaleX;
			}
		}

		protected override bool fnAwakeLogic(NAI Nai)
		{
			return this.fnSleepConsider(Nai);
		}

		public override void runPre()
		{
			if (base.destructed)
			{
				return;
			}
			base.runPre();
			if (this.Nai.AimPr != null && this.Nai.target_foot_slen < 3f && (this.Nai.AimPr.jump_starting || this.Nai.AimPr.isRunning()) && !this.Nai.HasF(NAI.FLAG.INJECTED, false) && !this.Nai.hasTypeLock(NAI.TYPE.BACKSTEP))
			{
				this.Nai.AddF(NAI.FLAG.INJECTED, 440f);
			}
		}

		protected override bool fnSleepConsider(NAI Nai)
		{
			NaTicket naTicket;
			if (!Nai.hasPT(128, false) && Nai.HasF(NAI.FLAG.INJECTED, true))
			{
				Nai.addTypeLock(NAI.TYPE.BACKSTEP, 520f);
				this.disturb_mem = X.Mx(1f, this.disturb_mem);
				this.playSndPos("cow_grawl", 1);
				this.setEmotHkds(false);
				this.applyHpDamage(X.Mn(this.hp - 1, X.IntC(this.disturb_mem * 32f)), false, null);
				naTicket = Nai.AddTicket(NAI.TYPE.BACKSTEP, 128, true);
				float num = X.NIXP(3f, 7f);
				if (base.x < (float)this.BelongTo.mapx + this.sizex + 1.8f)
				{
					naTicket.Dep((float)this.BelongTo.mapx + this.sizex + 0.5f + num, base.y, null);
				}
				else if (base.x < (float)(this.BelongTo.mapx + this.BelongTo.mapw) - this.sizex - 1.8f)
				{
					naTicket.Dep((float)(this.BelongTo.mapx + this.BelongTo.mapw) - this.sizex - 0.5f - num, base.y, null);
				}
				else
				{
					float num2 = X.NIL(0.4f, 0f, Nai.target_slen - 1.5f, 3f);
					if (X.XORSP() < num2 == base.mpf_is_right > 0f)
					{
						naTicket.Dep(base.x - num, base.y, null);
					}
					else
					{
						naTicket.Dep(base.x + num, base.y, null);
					}
				}
				return true;
			}
			if (Nai.hasPT(20, false))
			{
				return false;
			}
			if (this.isMoveScriptActive(false))
			{
				Nai.AddTicket(NAI.TYPE.WAIT, 20, true).Delay(60f);
				return true;
			}
			if (!this.BelongTo.cowCanMove())
			{
				Nai.AddTicket(NAI.TYPE.WAIT, 20, true).Delay(Nai.NIRANtk(30f, 60f, 2489));
				return true;
			}
			if (this.BelongTo.isSuckTarget(this))
			{
				if (EV.isActive("___city_farm/_game_cow_suck", false))
				{
					Nai.delay = 25f;
					return false;
				}
				this.BelongTo.quitSuck(false, false);
			}
			if (!Nai.hasTypeLock(NAI.TYPE.PUNCH_0))
			{
				float num3 = ((this.disturb_to_attack_mem_ + (float)this.hp_drop_level) / (float)this.hp_drop_max - 0.2f) / 0.8f;
				bool flag = num3 > 0f && Nai.RANtk(9114) < num3 && Nai.target_foot_slen < 5.5f;
				if (!flag && Nai.isPrAbsorbed() && (float)this.hp_drop_level >= (float)this.hp_drop_max * 0.5f && (base.AimPr as PR).getAbsorbContainer().countTortureItem(NelNMgmFarmCow.FD_countAbsorbingCow, false) == 1)
				{
					if (Nai.RANtk(3451) < 0.4f)
					{
						flag = true;
					}
					else
					{
						Nai.addTypeLock(NAI.TYPE.PUNCH_0, 550f);
					}
				}
				if (flag)
				{
					Nai.AddTicket(NAI.TYPE.PUNCH_0, 128, true);
					return true;
				}
			}
			bool flag2 = false;
			if (this.disturb_mem != 0f)
			{
				Nai.AddF(NAI.FLAG.BOTHERED, -1f);
				this.playSndPos("cow_grawl", 1);
				flag2 = true;
				if (this.hp > 1 && this.disturb_mem > 0f)
				{
					this.setEmotHkds(false);
					this.applyHpDamage(X.Mn(this.hp - 1, X.IntC(this.disturb_mem * 32f)), false, null);
				}
			}
			if (!flag2)
			{
				if (Nai.RANtk(3114) < 0.3f && !Nai.hasTypeLock(NAI.TYPE.WAIT))
				{
					Nai.AddTicket(NAI.TYPE.WAIT, 20, true).Delay(Nai.NIRANtk(60f, 190f, 4411));
					return true;
				}
				if (this.BelongTo.canFindWeed() && (this.mp < this.maxmp || Nai.RANtk(518) < 0.15f) && !Nai.hasTypeLock(NAI.TYPE.WALK_TO_WEED))
				{
					M2ManaWeed targetFromCow = this.BelongTo.getTargetFromCow(this, X.NI(13f, 1.5f, base.hp_ratio), true, false);
					if (targetFromCow != null)
					{
						this.Weed.Init(targetFromCow, 60f, false);
						naTicket = Nai.AddTicket(NAI.TYPE.WALK_TO_WEED, 20, true);
						naTicket.DepBCC = targetFromCow.GetBcc();
						naTicket.Dep(targetFromCow.mapcx - (float)X.MPF(base.x < targetFromCow.mapcx) * this.WEED_MARGIN_X, targetFromCow.mbottom, null);
						return true;
					}
				}
			}
			naTicket = Nai.AddTicket(NAI.TYPE.WALK, 20, true);
			bool flag3;
			if (X.XORSP() < (X.BTW(this.areax + 4f, base.x, this.arear - 4f) ? 0.66f : 0.4f))
			{
				flag3 = base.mpf_is_right > 0f;
			}
			else
			{
				flag3 = base.mpf_is_right <= 0f;
			}
			if (flag3 ? (base.x > this.arear - 1f) : (base.x < this.areax + 1f))
			{
				flag3 = !flag3;
			}
			if (flag3)
			{
				float num4 = X.Mn(this.arear, base.x + 3f);
				float num5 = X.Mn(this.arear, base.x + 3f + 5f);
				naTicket.DepX(X.NIXP(num4, num5));
			}
			else
			{
				float num6 = X.Mx(this.areax, base.x - 3f);
				float num7 = X.Mx(this.areax, base.x - 3f - 5f);
				naTicket.DepX(X.NIXP(num7, num6));
			}
			return true;
		}

		public void injectGrawlTicket()
		{
			if (this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE))
			{
				this.disturb_mem = X.Mx(this.disturb_mem, 1f);
				return;
			}
			if (this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE) && this.Nai.getCurTicket().prog < PROG.PROG2)
			{
				return;
			}
			this.Nai.clearTicket(-1, true);
			this.Nai.AddTicket(NAI.TYPE.PUNCH, 128, true);
		}

		public float areax
		{
			get
			{
				return (float)this.BelongTo.mapx + this.sizex + 2.2f;
			}
		}

		public float arear
		{
			get
			{
				return (float)(this.BelongTo.mapx + this.BelongTo.mapw) - this.sizey - 2.2f;
			}
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.GUARD)
			{
				switch (type)
				{
				case NAI.TYPE.WALK:
					break;
				case NAI.TYPE.WALK_TO_WEED:
					if (!this.runWalkToWeed(Tk.initProgress(this), Tk))
					{
						this.Nai.AddF(NAI.FLAG.WANDERING, 4f);
						return false;
					}
					return true;
				case NAI.TYPE.PUNCH:
					return this.runGrawl(Tk.initProgress(this), Tk);
				case NAI.TYPE.PUNCH_0:
					if (!this.runTackle(Tk.initProgress(this), Tk))
					{
						this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 850f);
						return false;
					}
					return true;
				default:
					if (type != NAI.TYPE.GUARD)
					{
						goto IL_013E;
					}
					if (!this.runEatWeed(Tk.initProgress(this), Tk))
					{
						this.Nai.AddF(NAI.FLAG.WANDERING, 4f);
						return false;
					}
					return true;
				}
			}
			else if (type != NAI.TYPE.BACKSTEP)
			{
				if (type != NAI.TYPE.WAIT)
				{
					goto IL_013E;
				}
				Tk.depx = -2f;
				Tk.aim = -2;
				if (Tk.initProgress(this))
				{
					this.SpSetPose("stand", -1, null, false);
				}
				if (Tk.t < 0f)
				{
					return true;
				}
				if (this.BelongTo.canFindWeed())
				{
					this.Nai.addTypeLock(NAI.TYPE.WAIT, 250f);
				}
				return false;
			}
			return this.runWalk(Tk.initProgress(this), Tk);
			IL_013E:
			return base.readTicket(Tk);
		}

		private bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.setAim((Tk.depx > base.x) ? AIM.R : AIM.L, false);
				this.SpSetPose("walk", -1, null, false);
				this.Nai.RemF(NAI.FLAG.BOTHERED);
				this.disturb_mem = 0f;
				this.Anm.timescale = this.walk_anm_timescale;
				this.prepareHpMpBarMesh();
			}
			if (this.walk_speed == 0f)
			{
				this.prepareHpMpBarMesh();
			}
			if (this.isMoveScriptActive(false))
			{
				return false;
			}
			this.Phy.walk_xspeed = base.mpf_is_right * (this.walk_speed * ((Tk.type == NAI.TYPE.BACKSTEP) ? 1.6f : 1f));
			if (X.Abs(base.x - Tk.depx) < 0.01f + this.walk_speed)
			{
				this.SpSetPose("stand", -1, null, false);
				this.Phy.walk_xspeed = 0f;
				Tk.AfterDelay(X.NIXP(60f, 100f));
				return false;
			}
			return true;
		}

		private bool runWalkToWeed(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				Tk.t = 0f;
				this.setAim((Tk.depx > base.x) ? AIM.R : AIM.L, false);
				this.SpSetPose("walk", -1, null, false);
				this.walk_st = 0;
				this.Anm.timescale = this.walk_anm_timescale;
				if (!this.Nai.HasF(NAI.FLAG.BOTHERED, false) && X.XORSP() < 0.5f)
				{
					this.playSndPos("cow_walk", 1);
				}
				this.disturb_mem = 0f;
				this.Nai.RemF(NAI.FLAG.BOTHERED);
				this.prepareHpMpBarMesh();
			}
			if (this.walk_speed == 0f)
			{
				this.prepareHpMpBarMesh();
			}
			if (this.isMoveScriptActive(false) || this.Weed.Weed == null)
			{
				return false;
			}
			this.Phy.walk_xspeed = base.mpf_is_right * this.walk_speed;
			if (X.Abs(base.x - Tk.depx) < 0.015f + X.Abs(this.walk_speed))
			{
				this.walk_st = 1;
				Tk.t = 1f;
			}
			if (Tk.t >= 0f)
			{
				Tk.Delay(40f);
				this.setAim((Tk.depx > base.x) ? AIM.R : AIM.L, false);
				if (!this.Weed.Weed.isActive() && X.Abs(Tk.depx - this.Weed.Weed.mapcx) < 50f + this.WEED_MARGIN_X)
				{
					this.disturb_mem += 1.5f;
				}
				if (!this.Weed.valid(this.Mp, null) || this.BelongTo.is_hard_linked(this.Weed.Weed, this))
				{
					Tk.AfterDelay(40f);
					return false;
				}
				this.Weed.Init(this.Weed.Weed, 60f, this.walk_st > 0);
			}
			if (this.walk_st <= 0)
			{
				return true;
			}
			if (!this.BelongTo.is_hard_linked(this.Weed.Weed, this))
			{
				this.Phy.walk_xspeed = 0f;
				Tk.Recreate(NAI.TYPE.GUARD, -1, false, null);
				return true;
			}
			return false;
		}

		private bool runEatWeed(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.Anm.timescale = 1f;
				this.Phy.walk_xspeed = 0f;
				if (this.Weed.Weed == null)
				{
					return false;
				}
				this.walk_time = X.NIXP(120f, 200f);
				this.setAim((this.Weed.Weed.mapcx > base.x) ? AIM.R : AIM.L, false);
				this.SpSetPose("eat", -1, null, false);
				this.walk_st = 0;
				Tk.t = 0f;
				this.t = 0f;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.Weed.Weed == null)
				{
					return false;
				}
				bool flag = Tk.Progress(ref this.t, (int)this.walk_time, true);
				if (!flag || Tk.t >= 0f)
				{
					Tk.Delay(20f);
					if (!this.Weed.Weed.isActive())
					{
						this.disturb_mem += 2.5f;
					}
					if (!this.Weed.valid(this.Mp, null) || this.BelongTo.is_hard_linked(this.Weed.Weed, this))
					{
						Tk.AfterDelay(40f);
						return false;
					}
					this.Weed.Init(this.Weed.Weed, 50f, true);
				}
				if (flag)
				{
					this.BelongTo.deassignActiveWeed(this.Weed.Weed, null);
					this.Weed.Weed.SplashManaInit(MANA_HIT.NOUSE, X.NI(this.Weed.Weed.mapcx, base.x, 0.2f), X.NI(this.Weed.Weed.mbottom, base.y, 0.5f), true, false);
					this.Weed.Clear();
					this.walk_time = X.NIXP(60f, 90f);
					this.t = 0f;
					this.Anm.setPose("eat2stand", -1);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= this.walk_time)
				{
					this.disturb_mem = 0f;
					this.addMpFromMana(this.maxmp);
					if (this.near_death)
					{
						this.cureHp(32);
					}
					this.Nai.addTypeLock(NAI.TYPE.WALK_TO_WEED, 520f);
					this.playSndPos("cow_grawl", 1);
					return false;
				}
				int num = X.IntR(this.t * (float)this.maxmp * 0.8f / this.walk_time);
				if (num > this.mp)
				{
					this.addMpFromMana(num - this.mp);
				}
			}
			return true;
		}

		private bool runGrawl(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.playSndPos("cow_grawl", 1);
				this.SpSetPose("grawl", -1, null, false);
				Tk.Delay(X.NIXP(60f, 90f));
				this.Phy.remLockMoverHitting(HITLOCK.EVENT);
				this.setExecutable(false);
			}
			return Tk.t < 0f;
		}

		private bool runTackle(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.AimToPlayer();
				this.SpSetPose("attack_0", -1, null, false);
				this.playSndPos("cow_atk", 1);
				this.t = 0f;
				this.walk_time = 0f;
				this.walk_st = (int)X.NIL(100f, 55f, (float)this.hp_drop_level, (float)this.hp_drop_max);
				this.can_hold_tackle = false;
				base.addF(NelEnemy.FLAG.HAS_SUPERARMOR);
				this.BelongTo.cow_angry_count++;
				this.Phy.remLockMoverHitting(HITLOCK.EVENT);
				this.setExecutable(false);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				bool flag = false;
				if (!this.can_hold_tackle)
				{
					PR pr = base.AimPr as PR;
					if (this.t >= 20f && pr != null && (pr.isAbsorbState() || pr.isSinkState()))
					{
						flag = true;
					}
				}
				if (Tk.Progress(ref this.t, this.walk_st, true))
				{
					this.SpSetPose("attack_1", -1, null, false);
					this.walk_st = 0;
					Tk.t = 0f;
					flag = true;
				}
				if (!this.can_hold_tackle && flag)
				{
					base.tackleInit(this.AtkTackleP, this.TkTackle);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.Phy.walk_xspeed = base.mpf_is_right * 0.14f * X.ZSIN(this.t, 11f);
				if (Tk.t >= 8f)
				{
					Tk.t -= 8f;
					if ((base.mpf_is_right > 0f) ? (this.Nai.target_x < base.mleft) : (this.Nai.target_x > base.mright))
					{
						this.walk_st++;
					}
					if ((base.mpf_is_right > 0f) ? (this.Nai.target_x >= (float)(this.BelongTo.mapx + this.BelongTo.mapw) - (this.sizex + 0.5f)) : (this.Nai.target_x < (float)this.BelongTo.mapx + (this.sizex + 0.5f)))
					{
						this.walk_st += 3;
					}
				}
				if (this.walk_st >= 4)
				{
					Tk.prog = PROG.PROG1;
					this.disturb_to_attack_mem_ -= (float)X.Mn(3, X.IntR(this.t / 25f));
					this.SpSetPose("attack_2", -1, null, false);
					this.t = 0f;
					this.walk_st = 0;
					this.can_hold_tackle = false;
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.Phy.walk_xspeed = base.mpf_is_right * 0.14f * (1f - X.ZSIN(this.t, 70f));
				if (Tk.Progress(ref this.t, 80, true))
				{
					if (this.disturb_to_attack_mem_ > 0f)
					{
						this.setAim((base.mpf_is_right > 0f) ? AIM.L : AIM.R, false);
						Tk.prog = PROG.PROG0;
						this.walk_st = 0;
						this.walk_time = 0f;
						Tk.t = 0f;
						base.tackleInit(this.AtkTackleP, this.TkTackle);
						this.SpSetPose("attack_1", -1, null, false);
						return true;
					}
					this.Phy.walk_xspeed = 0f;
					this.SpSetPose("attack2stand", -1, null, false);
					Tk.AfterDelay(X.NIXP(76f, 120f));
					this.disturb_to_attack_mem_ = 0f;
					this.disturb_mem = 0f;
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 520f);
					return false;
				}
			}
			if (Tk.prog < PROG.PROG1)
			{
				this.walk_time += this.TS;
				if (this.walk_time >= 14f)
				{
					this.walk_time -= 14f;
					base.PtcST("cow_grawl_aula", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			return true;
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (this.Nai.HasF(NAI.FLAG.BOTHERED, true))
			{
				this.disturb_mem = 0f;
			}
			this.Anm.timescale = 1f;
			this.PtcHld.killPtc("cow_grawl_aula", false);
			base.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
			this.can_hold_tackle = false;
			this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
			this.setExecutable(true);
			if (Tk != null)
			{
				if (this.Anm.poseIs("eat", false))
				{
					this.Anm.setPose("eat2stand", -1);
				}
				else if (this.Anm.poseIs("grawl", false))
				{
					this.Anm.setPose("grawl2stand", -1);
				}
				else if (this.Anm.poseIs("attack_2", "absorb"))
				{
					this.Anm.setPose("attack2stand", -1);
				}
				else if (!this.Anm.poseIs("eat2stand", "attack2stand", "grawl2stand"))
				{
					this.SpSetPose("stand", -1, null, false);
				}
				if (!this.Nai.HasF(NAI.FLAG.WANDERING, false))
				{
					if (Tk.type == NAI.TYPE.WALK_TO_WEED)
					{
						this.disturb_mem += 1.5f;
					}
					if (Tk.type == NAI.TYPE.GUARD)
					{
						this.disturb_mem += 2.5f;
					}
				}
				if (Tk.type == NAI.TYPE.PUNCH_0)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 520f);
					this.disturb_to_attack_mem_ = 0f;
				}
				this.Phy.walk_xspeed = 0f;
			}
			this.Weed.Clear();
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		public override void addMpFromMana(int val)
		{
			int mp = this.mp;
			base.addMpFromMana(val);
			if (mp < this.maxmp && this.mp >= this.maxmp)
			{
				this.setEmotHkds(true);
			}
		}

		public override bool runDamageSmall()
		{
			if (!base.runDamageSmall())
			{
				this.disturb_mem = X.Mx(1f, this.disturb_mem);
				return false;
			}
			return true;
		}

		public override void prepareHpMpBarMesh()
		{
			float num = (float)this.hp_drop_level;
			float num2 = (float)this.hp_drop_max;
			this.walk_anm_timescale = X.NIL(1f, 3.5f, num, num2);
			this.walk_speed = ((num >= num2 - 1f) ? 0.007f : X.NIL(0.014f, 0.08f, num, num2 - 2f));
			base.remF((NelEnemy.FLAG)768);
		}

		private bool fnChangeColoringTag()
		{
			MeshDrawer meshNormalRender = this.Anm.getMeshNormalRender();
			EnemyFrameDataBasic curFrameData = this.Anm.getCurFrameData();
			PxlFrame currentDrawnFrame = this.Anm.getCurrentDrawnFrame();
			if (curFrameData == null || meshNormalRender == null || curFrameData.normal_render_layer == 0U)
			{
				return true;
			}
			int num = currentDrawnFrame.countLayers();
			Color32[] colorArray = meshNormalRender.getColorArray();
			int vertexMax = meshNormalRender.getVertexMax();
			int num2 = 0;
			if (vertexMax == 0)
			{
				return true;
			}
			for (int i = 0; i < num; i++)
			{
				uint num3 = 1U << i;
				if ((curFrameData.normal_render_layer & num3) != 0U)
				{
					if (!TX.isStart(currentDrawnFrame.getLayer(i).name, "nml_board", 0))
					{
						num2 += 4;
					}
					else
					{
						meshNormalRender.Col = meshNormalRender.ColGrd.Set(colorArray[num2]).multiply(this.CBoard, false).C;
						for (int j = 0; j < 4; j++)
						{
							colorArray[num2] = meshNormalRender.Col;
							num2++;
						}
					}
					if (num2 >= vertexMax)
					{
						break;
					}
				}
			}
			return true;
		}

		public void setEmotHkds(bool is_happy = false)
		{
			if (this.FD_EfDrawHkds == null)
			{
				this.FD_EfDrawHkds = delegate(EffectItem _Ef)
				{
					if (!this.EfDrawHkds(_Ef))
					{
						if (_Ef == this.EfHkds)
						{
							this.EfHkds = null;
						}
						return false;
					}
					return true;
				};
			}
			if (this.EfHkds != null)
			{
				this.EfHkds.destruct();
			}
			if (is_happy)
			{
				this.playSndPos("cow_grawl", 1);
			}
			this.EfHkds = this.Mp.getEffectTop().setEffectWithSpecificFn("cow_emote", this.emot_hkds_x, this.emot_hkds_y, (float)(is_happy ? 1 : 0), 0, 0, this.FD_EfDrawHkds);
		}

		public bool EfDrawHkds(EffectItem Ef)
		{
			if (base.destructed)
			{
				return false;
			}
			Ef.x = X.VALWALK(Ef.x, this.emot_hkds_x, (float)X.AF_EF * 0.25f);
			Ef.y = X.VALWALK(Ef.y, this.emot_hkds_y, (float)X.AF_EF * 0.25f);
			return EffectItemNel.fnRunDraw_NpcHkds(Ef, (Ef.z == 1f) ? MTR.SqNpcLoveUp : MTR.SqNpcSmoke, 2f, 160f);
		}

		public float emot_hkds_x
		{
			get
			{
				return base.x + this.sizex * 1.6f * this.Anm.scaleX * base.mpf_is_right;
			}
		}

		public float emot_hkds_y
		{
			get
			{
				return base.y - 0.3f - 0.8f * this.Anm.scaleY;
			}
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (Abm == null || !(MvTarget is PR) || !Abm.checkPublisher(this) || !base.nM2D.FlgWarpEventNotInjectable.hasKey("MGFARM"))
			{
				return false;
			}
			PR pr = MvTarget as PR;
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			float num;
			if (!pr.isAbsorbState())
			{
				if (EV.isActive("___city_farm/_game_absorb", false))
				{
					return false;
				}
				EV.stack("___city_farm/_game_absorb", 0, -1, null, null);
				num = 0.14f;
				this.walk_st = 0;
			}
			else
			{
				if (!EV.isActive("___city_farm/_game_absorb", false))
				{
					return false;
				}
				AbsorbManager specificPublisher = Abm.Con.getSpecificPublisher(NelNMgmFarmCow.FD_countAbsorbingCowGachaActive);
				if (specificPublisher == null)
				{
					return false;
				}
				PrGachaItem gacha = specificPublisher.get_Gacha();
				PrGachaItem gacha2 = Abm.get_Gacha();
				gacha2.activate(gacha.type, (int)gacha.getTotalCount(false), 63U);
				gacha2.SoloPositionPixel = new Vector3(IN.wh * 0.37f, IN.hh * 0.25f, 1f);
				num = 0.046200003f;
				this.walk_st = 1000;
			}
			this.walk_speed = 0f;
			this.walk_time = 0f;
			this.SpSetPose("absorb", -1, null, false);
			Abm.target_pose = "absorbed_down";
			this.Phy.addFoc(FOCTYPE.SPECIAL_ATTACK, base.mpf_is_right * num, 0f, -2f, 0, 2, 25, -1, 0);
			pr.getPhysic().addFoc(FOCTYPE.SPECIAL_ATTACK, base.mpf_is_right * num, 0f, -2f, 0, 2, 25, -1, 0);
			Abm.release_from_publish_count = true;
			this.Anm.showToFront(true, false);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			int num = this.walk_st % 1000;
			bool flag = this.walk_st >= 1000;
			if (this.walk_time <= 60f)
			{
				this.walk_time += this.TS;
			}
			M2Phys physic = pr.getPhysic();
			Vector2 vector = new Vector2(physic.move_depert_x, physic.move_depert_y);
			float num2;
			float num3;
			pr.getMouthPosition(out num2, out num3);
			vector.x += (num2 - pr.x) * 0.4f;
			float num4 = vector.x - base.mpf_is_right * this.sizex * 1.66f;
			new Vector2(this.Phy.move_depert_x, this.Phy.move_depert_y);
			X.NIL(0.18f, 0.004f, this.walk_time, 40f);
			this.Phy.addTranslateStack((num4 - (this.Phy.tstacked_x + this.Phy.pre_force_velocity_x)) * 0.08f, 0f);
			if (num == 0)
			{
				if (this.t >= 240f)
				{
					return false;
				}
				if (EV.isActive("___city_farm/_game_absorb", true))
				{
					this.walk_st++;
					num++;
					this.t = 0f;
				}
			}
			if (num == 1)
			{
				PrGachaItem gacha = this.Absorb.get_Gacha();
				if (gacha.isUseable() || EV.isWaiting(this.Absorb.Con))
				{
					this.walk_st++;
					num++;
					this.t = 485f;
					if (!this.Absorb.get_Gacha().isUseable())
					{
						gacha.activate(PrGachaItem.TYPE.REP, 4, 63U);
						gacha.SoloPositionPixel = new Vector3(IN.wh * 0.37f * (float)X.MPF(flag), IN.hh * 0.25f, 1f);
					}
					if (!flag)
					{
						this.BelongTo.grab_count++;
					}
				}
				else if (!EV.isActive("___city_farm/_game_absorb", true) && this.t >= 30f)
				{
					return false;
				}
			}
			if (num >= 2 && this.t >= 500f)
			{
				this.t = 500f - X.NIXP(40f, 54f);
				bool flag2 = this.Absorb.Con.countTortureItem(NelNMgmFarmCow.FD_countAbsorbingCow, false) >= 2;
				this.Absorb.uipicture_fade_key = (flag2 ? "cuts_cow_1_licking_2" : "cuts_cow_1_licking");
				if (flag2)
				{
					this.AtkAbsorb.EpDmg.situation_key = ((X.XORSP() < 0.33f) ? "cow2lick" : "cow2");
				}
				else
				{
					this.AtkAbsorb.EpDmg.situation_key = ((X.XORSP() < 0.33f) ? "cowlick" : "cow");
				}
				base.PtcST("player_absorbed_licked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				float num5 = X.NIXP(4f, 6f) * (float)X.MPFXP();
				float num6 = X.NIXP(20f, 25f);
				if (X.XORSP() < 0.45f)
				{
					pr.TeCon.setQuakeSinH(num5, 52, num6, 0f, 0);
					this.TeCon.setQuakeSinH(num5 * 0.7f, 52, num6, 0f, 0);
				}
				base.runAbsorb();
				base.applyAbsorbDamageTo(pr, this.AtkAbsorb, num % 2 == 0, false, false, 1f, false, this.Absorb.uipicture_fade_key, true);
				this.Anm.randomizeFrame();
				pr.Ser.Add(SER.MILKY, 240, 99, false);
				pr.EpCon.SituCon.addTempSituation("&&GM_ep_situation_farm", 1, false);
				this.BelongTo.grab_atk_count++;
				if (pr.UP.isActive())
				{
					pr.UP.PtcST("ui_kiss_to_bust_cow", null);
				}
				if (num >= 90)
				{
					this.walk_st -= 40;
					num -= 40;
				}
				this.walk_st++;
				num++;
			}
			return true;
		}

		private void setExecutable(bool flag)
		{
			if (this.AttachEvent.canExecutable(M2EventItem.CMD.TALK) == flag)
			{
				return;
			}
			this.AttachEvent.setExecutable(M2EventItem.CMD.TALK, flag);
			this.fineCheckDesc();
		}

		private void fineCheckDesc()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add("<img mesh=\"cow_tag\" color=\"");
				stb.AddCol(C32.c2d(this.CBoard), "0x").Add("\" />");
				stb.AddTxA("EV_access_suckmilk", false);
				this.AttachEvent.setCheckDescNameRaw(stb, true);
			}
		}

		public void activateEvent(bool flag = true)
		{
			if (!flag)
			{
				this.AttachEvent.remove(1);
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AR("DENY_SKIP").AR("VALOTIZE");
				stb.Add("MGFARM PREPARE_TALK2COW ", this.cow_index, "").Ret("\n");
				stb.AR("IF $_result {");
				stb.AR("#< % >");
				stb.AR("_pr_x=~m2d_current_x");
				stb.AR("#< ", base.key, " >");
				stb.AR("_en_x=~m2d_current_x");
				stb.AR("IF $_pr_x'<'$_en_x _mpf=-12");
				stb.AR("ELSE _mpf=12");
				stb.AR("#MS_ % '>>[#<", base.key, "> '$_mpf',0 <<0.088]'");
				stb.Add("MGFARM TALK2COW ", this.cow_index, "").Ret("\n");
				stb.AR("IF $_result {");
				stb.Add("CHANGE_EVENT ", "___city_farm/_game_cow_suck", " ").AR(base.key);
				stb.AR("}");
				stb.AR("}");
				stb.AR("WAIT_FN MGFARM");
				stb.AR("WAIT 5");
				this.AttachEvent.assign(M2EventItem.CMD.TALK, stb.ToString(), false);
			}
		}

		public bool prepareTalkEvent(bool executing = false)
		{
			if (!this.BelongTo.isMainGame() || this.state != NelEnemy.STATE.STAND)
			{
				return false;
			}
			float num = 0f;
			if (this.mp == 0)
			{
				num += (float)this.hp_drop_level / (float)this.hp_drop_max * 0.45f;
			}
			if (executing && this.Nai.isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE))
			{
				num += 0.25f;
			}
			if (num > 0f && X.XORSP() < num)
			{
				this.injectGrawlTicket();
				return false;
			}
			if (this.Nai.isFrontType(NAI.TYPE.WALK, PROG.ACTIVE) || this.Nai.isFrontType(NAI.TYPE.BACKSTEP, PROG.ACTIVE) || this.Nai.isFrontType(NAI.TYPE.WALK_TO_WEED, PROG.ACTIVE))
			{
				this.Nai.clearTicket(-1, true);
			}
			return true;
		}

		public bool triggeredTalkEvent()
		{
			if (!this.prepareTalkEvent(true))
			{
				return false;
			}
			this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 300f);
			return true;
		}

		internal void quitSuck(bool from_event, ref bool success)
		{
			bool flag = base.mp_ratio > 0.25f;
			if (success && flag)
			{
				this.disturb_mem = 0f;
				this.disturb_to_attack_mem_ = 0f;
				success = true;
			}
			else
			{
				success = false;
				float disturb_mem = this.disturb_mem;
				this.disturb_mem = disturb_mem + 1f;
				this.disturb_to_attack_mem_ += 1f;
			}
			this.applyMpDamage((int)base.get_maxmp() * 60, true, null);
			if (from_event)
			{
				this.setEmotHkds(success);
				if (success)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 300f);
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 300f);
				}
				else
				{
					this.injectGrawlTicket();
				}
				base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			return this.Phy.hasLockMoverHitting(HITLOCK.ABSORB);
		}

		public float disturb_to_attack_mem
		{
			get
			{
				return this.disturb_to_attack_mem_;
			}
			set
			{
				this.disturb_to_attack_mem_ = X.Mx(this.disturb_to_attack_mem_, value);
			}
		}

		public bool near_death
		{
			get
			{
				return this.hp_drop_level >= this.hp_drop_max - 1;
			}
		}

		public int hp_drop_level
		{
			get
			{
				return (int)((base.get_maxhp() - base.get_hp()) * 0.03125f);
			}
		}

		public int hp_drop_max
		{
			get
			{
				return (int)((base.get_maxhp() - 1f) * 0.03125f);
			}
		}

		public int mp_drop_level
		{
			get
			{
				return (int)((base.get_maxmp() - base.get_mp()) * 0.0078125f);
			}
		}

		public int mp_drop_max
		{
			get
			{
				return (int)(base.get_maxmp() * 0.0078125f);
			}
		}

		public override float enlarge_level_to_anim_scale(float r = -1f)
		{
			return ((this.Nai != null) ? this.NIn(1f, 1.3f, 1931) : 1f) * base.enlarge_level_to_anim_scale(r);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (base.applyHpDamageRatio(Atk) == 0f)
			{
				return 0f;
			}
			return 0.33f;
		}

		public override float getMpDesireRatio(int add_mp)
		{
			return 0.95f;
		}

		public float NIn(float _v1, float _v2, int _seed)
		{
			return X.NI(_v1, _v2, X.RAN(this.Nai.ran_n, _seed));
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		internal M2LpMgmFarm BelongTo;

		public int cow_index;

		public Color32 CBoard;

		private float walk_anm_timescale;

		private const int HP_CLIP = 32;

		private const int MP_CLIP = 128;

		private const float HP_DMG_RATIO = 0.33f;

		private const float DISTURB_CLIP_LEVEL_WALKWEED = 1.5f;

		private const float DISTURB_CLIP_LEVEL_EAT = 2.5f;

		private const int CURE_HP_ON_FINISH_EAT = 64;

		private const int CURE_HP_ON_FINISH_EAT_NEARDEATH = 32;

		private const float run_speed = 0.14f;

		private const string absorb_fade_key = "cuts_cow_1_licking";

		private const string absorb_fade_key_licked_2 = "cuts_cow_1_licking_2";

		private const NAI.TYPE NT_EAT = NAI.TYPE.GUARD;

		private const NAI.TYPE NT_GRAWL = NAI.TYPE.PUNCH;

		private const NAI.TYPE NT_TACKLE = NAI.TYPE.PUNCH_0;

		public M2ManaWeed.WeedLink Weed;

		public float disturb_mem_;

		public float disturb_to_attack_mem_;

		protected NelAttackInfo AtkTackleP = new NelAttackInfo
		{
			hpdmg0 = 0,
			split_mpdmg = 33,
			absorb_replace_prob = 1000f,
			absorb_replace_prob_ondamage = 1000f,
			knockback_len = 0.7f,
			parryable = false,
			Beto = BetoInfo.Normal,
			shield_break_ratio = -9999f,
			burst_vx = 0.19f,
			burst_vy = -0.1f,
			ndmg = NDMG.GRAB_PENETRATE
		}.Torn(0.022f, 0.08f);

		protected NOD.TackleInfo TkTackle = NOD.getTackle("cow_tackle");

		protected NelAttackInfo AtkAbsorb = new NelAttackInfo
		{
			split_mpdmg = 2,
			attr = MGATTR.ABSORB,
			EpDmg = new EpAtk(15, "cow")
			{
				breast = 9
			},
			Beto = BetoInfo.Absorbed
		};

		private const int PRI_MAIN = 20;

		private const int PRI_OVR = 128;

		private const float WALK_RANGE = 5f;

		private const float WALK_MIN_LEN = 3f;

		private const float AREA_MARGIN = 2.2f;

		private const float RATIO_WALK_STRAIGHT = 0.66f;

		private const float WALK_SPEED_MIN = 0.014f;

		private const float WALK_SPEED_MAX = 0.08f;

		public float walk_speed;

		public const float WEED_BROKEN_ANGRY = 50f;

		private FnEffectRun FD_EfDrawHkds;

		private EffectItem EfHkds;

		private static Func<AbsorbManager, bool> FD_countAbsorbingCow = (AbsorbManager _Abm) => _Abm.getPublishMover() is NelNMgmFarmCow;

		private static Func<AbsorbManager, bool> FD_countAbsorbingCowGachaActive = (AbsorbManager _Abm) => _Abm.getPublishMover() is NelNMgmFarmCow && _Abm.get_Gacha().isActive();
	}
}
