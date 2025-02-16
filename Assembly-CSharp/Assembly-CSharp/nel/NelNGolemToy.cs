﻿using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class NelNGolemToy : NelEnemy, EnemySummoner.IOtherKillListener
	{
		public abstract int create_count_normal { get; }

		public override void Awake()
		{
			if (this.Anm == null)
			{
				this.Anm = (this.AnmT = new EnemyAnimatorGolemToy(this, new EnemyAnimator.FnCreate(EnemyFrameDataBasic.Create), null));
			}
			base.battleable_enemy = false;
		}

		public static Type GetType(ENEMYID id)
		{
			switch (id & ENEMYID._GOLEMTOY_KIND)
			{
			case ENEMYID.GOLEMTOY_0:
				return typeof(NelNGolemToyMkb);
			case ENEMYID.GOLEMTOY_RM:
				return typeof(NelNGolemToyRainMaker);
			case ENEMYID.GOLEMTOY_POD:
				return typeof(NelNGolemToyMisPod);
			case ENEMYID.GOLEMTOY_BOW:
				return typeof(NelNGolemToyBow);
			default:
				return typeof(NelNGolemToyMkb);
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.ACreator = new List<NelNGolem>(3);
			this.kind = ENEMYKIND.MACHINE;
			float num = 60f;
			NOD.BasicData basicData;
			switch (this.id)
			{
			case ENEMYID.GOLEMTOY_RM:
				basicData = NOD.getBasicData("GOLEMTOY_RM");
				break;
			case ENEMYID.GOLEMTOY_POD:
				basicData = NOD.getBasicData("GOLEMTOY_POD");
				break;
			case ENEMYID.GOLEMTOY_BOW:
				basicData = NOD.getBasicData("GOLEMTOY_BOW");
				break;
			default:
				this.id = ENEMYID.GOLEMTOY_0;
				basicData = NOD.getBasicData("GOLEMTOY_MKB");
				break;
			}
			if (NelNGolemToy.FD_countNotGolemToy == null)
			{
				NelNGolemToy.FD_countNotGolemToy = new NelEnemy.FnCheckEnemy(NelNGolemToy.countNotGolemToy);
			}
			base.appear(_Mp, basicData);
			this.auto_fix_notfound_pose_to_stand = false;
			this.snd_die = "golemtoy_die";
			this.Lig.showing_delay = 1000;
			this.create_hummer_count = X.IntR((float)this.create_count_normal * X.NI(1f, 2.25f, base.mp_ratio));
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_top = -5f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.absorb_weight = 3;
			base.addF(NelEnemy.FLAG.HAS_SUPERARMOR);
		}

		public static bool countNotGolemToy(NelEnemy N)
		{
			return !(N is NelNGolemToy) || (N as NelNGolemToy).create_finished;
		}

		public virtual void setDeathDro()
		{
			this.Anm.setBreakerDropObject("golemtoy_break", 0f, 0f, 0f, null);
		}

		protected override bool initDeathEffect()
		{
			if (!base.hasF(NelEnemy.FLAG.INITDEATH_PREPARED))
			{
				this.AnmT.destructEffect();
			}
			if (this.create_finished)
			{
				this.setDeathDro();
			}
			return base.initDeathEffect();
		}

		public void addCreator(NelNGolem Glm)
		{
			this.ACreator.Add(Glm);
		}

		public void remCreator(NelNGolem Glm)
		{
			this.ACreator.Remove(Glm);
		}

		public bool creator_addable
		{
			get
			{
				return this.ACreator.Count < 3;
			}
		}

		public void fixFirstPos(M2BlockColliderContainer.BCCLine Bcc)
		{
			float num = Bcc.slopeBottomY(base.x, Bcc.BCC.base_shift_x, Bcc.BCC.base_shift_y, true);
			this.moveBy(0f, num - this.sizey - base.y, true);
			base.disappearing = (base.throw_ray = (this.cannot_move = true));
		}

		public bool progressCreate(NelNGolem Glm)
		{
			if (this.create_hummer_count == 0)
			{
				return true;
			}
			this.Anm.need_fine_mesh = true;
			if (base.disappearing)
			{
				base.battleable_enemy = false;
				this.t = 0f;
				base.disappearing = false;
				this.Anm.alpha = 1f;
				this.Lig.showing_t = 0;
				this.Lig.showing_delay = 50;
				this.hp = 1;
				this.Anm.setBorderMaskEnable(true);
				base.throw_ray = true;
				base.PtcVar("cy", (double)base.mbottom).PtcST("golemtoy_activate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			else
			{
				this.hp += this.maxhp / this.create_hummer_count;
				if (this.hp >= this.maxhp)
				{
					this.hp = this.maxhp;
					this.create_hummer_count = 0;
					base.throw_ray = false;
					base.PtcST("golemtoy_born", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					base.battleable_enemy = true;
					this.initBorn();
					return true;
				}
				Glm.PtcVar("cx", (double)(Glm.x + Glm.mpf_is_right * (this.sizex * 0.6f))).PtcVar("cy", (double)(Glm.y - Glm.sizey * 0.3f)).PtcST("golemtoy_creating", PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.NO_FOLLOW);
			}
			return false;
		}

		public void otherEnemyKilled(NelEnemy Other)
		{
			if (this.Summoner != null && this.Summoner.getRestCount() == 0 && !this.create_finished && this.Summoner.countActiveEnemy(NelNGolemToy.FD_countNotGolemToy, true) == 0)
			{
				this.changeStateToDie();
			}
		}

		protected abstract bool considerNormal(NAI Nai);

		public abstract void makeBone(List<Vector3> ABone);

		public virtual bool drawSpecial(MeshDrawer Md)
		{
			return false;
		}

		public virtual void drawAfter(MeshDrawer Md)
		{
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (!this.create_finished && base.disappearing)
			{
				this.Lig.showing_t = 0;
				if (this.t >= 120f)
				{
					this.changeStateToDie();
					base.disappearing = true;
					return;
				}
			}
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
		}

		public override bool runStun()
		{
			if (this.t <= 0f)
			{
				base.PtcVar("sizex", (double)(this.sizex * base.CLENM)).PtcVar("sizey", (double)(this.sizey * base.CLENM)).PtcST("mech_stunned", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.t = 0f;
			}
			if (!this.Ser.has(SER.EATEN))
			{
				base.killPtc("mech_stunned", false);
				return false;
			}
			return this;
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (!this.create_finished)
			{
				return false;
			}
			NAI.TYPE type = Tk.type;
			if (type == NAI.TYPE.WALK)
			{
				bool flag = Tk.initProgress(this);
				int num = base.walkThroughLift(flag, Tk, 20);
				return num >= 0 && num == 0;
			}
			if (type == NAI.TYPE.WAIT)
			{
				base.AimToLr((X.xors(2) == 0) ? 0 : 2);
				Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
				return false;
			}
			return base.readTicket(Tk);
		}

		protected virtual void initBorn()
		{
			this.snd_die = "machine_die";
			this.Anm.setBorderMaskEnable(false);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		public bool create_finished
		{
			get
			{
				return this.create_hummer_count == 0;
			}
		}

		public ENEMYID toy_kind
		{
			get
			{
				return this.id & ENEMYID._GOLEMTOY_KIND;
			}
		}

		public static string[] Acreate_key = new string[] { "GOLEMTOY_POD", "GOLEMTOY_BOW", "GOLEMTOY_RM", "GOLEMTOY_MKB" };

		private List<NelNGolem> ACreator;

		public const int toy_creatable_default = 3;

		public int create_hummer_count;

		protected EnemyAnimatorGolemToy AnmT;

		private static NelEnemy.FnCheckEnemy FD_countNotGolemToy;
	}
}
