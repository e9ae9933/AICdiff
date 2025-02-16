using System;
using m2d;
using XX;

namespace nel
{
	public class MgItemBombMagic : MgItemBomb
	{
		public MgItemBombMagic()
		{
			this.col_stroke_add0 = 4279760310U;
			this.col_stroke_add1 = 205967U;
			this.col_stroke_sub0 = 2580949843U;
			this.col_stroke_sub1 = 8405014U;
			this.col_explode_sub_circle = 2013214302U;
			this.ptc_key_explode = null;
			this.ptc_key_explode_prepare = "itembomb_magic_prepare";
			this.snd_key_dro = "itembomb_mag_drop";
			this.explode_radius_min = 4.5f;
			this.explode_radius_max = 5.8f;
			this.pf_start_id = 4;
			this.explode_maxt = 103f;
		}

		protected override void InitItemInner(MgItemBomb.MgBombMem Mem, float gratio)
		{
			NelAttackInfo atk = Mem.Mg.Atk0;
			if (this.FD_fnHitAfter == null)
			{
				this.FD_fnHitAfter = new AttackInfo.FnHitAfter(this.fnHitAfter);
			}
			atk.hpdmg0 = (int)(X.NI(70, 150, gratio) / 10f);
			atk.mpdmg0 = (int)(X.NI(60, 170, gratio) / 10f);
			atk.attr = MGATTR.BOMB;
			atk.split_mpdmg = 15;
			atk.ignore_nodamage_time = true;
			atk.pr_myself_fire = true;
			atk.attr = MGATTR.BOMB;
			atk.burst_vx = 0.02f;
			atk.burst_vy = -0.3f;
			atk.shield_break_ratio = 0.001f;
			atk.attack_max0 = -1;
			atk.burst_center = 1f;
			atk.huttobi_ratio = -1000f;
			atk.centerblur_damage_ratio = 0.5f;
			atk.ndmg = NDMG.GRAB_PENETRATE;
			atk.damage_randomize_min = 0.95f;
			atk.hit_ptcst_name = "hit_en_itembomb_mag";
			atk.fnHitEffectAfter = this.FD_fnHitAfter;
			Mem.Mg.phase = 100;
			Mem.Mg.Mn._1.thick = X.NI(this.explode_radius_min, this.explode_radius_max, gratio);
		}

		public override void initThrow(MagicItem Mg, bool dropping = false, bool self_explode_bomb = false)
		{
			base.initThrow(Mg, dropping, self_explode_bomb);
			Mg.playSndPos("itembomb_magic_countdown", Mg.sx, Mg.sy, false, PTCThread.StFollow.FOLLOW_S);
		}

		protected override void explodeExecute(MagicItem Mg, bool first)
		{
			if (first)
			{
				Mg.Ray.HitLock(10f, null);
				Mg.PtcVar("dmg_count", 10.0).PtcVar("dmg_intv", 10.0);
				Mg.PtcST("itembomb_magic_explode", PTCThread.StFollow.NO_FOLLOW, true);
				Mg.projectile_power = 1;
				Mg.MGC.quitRayCohitable(Mg.Ray);
			}
			base.explodeExecute(Mg, first);
		}

		public override void drawExplodePrepare(MagicItem Mg, MeshDrawer MdAdd, float count_tz, float r, float scl)
		{
			float num = Mg.Mn._1.thick / this.explode_radius_min;
			UniBrightDrawer uniBrightDrawer = MTRX.UniBright.Count(X.IntC(count_tz * (2f + 3f * num))).RotTime(550f, 780f).Col(C32.MulA(4281134591U, count_tz), C32.MulA(205967U, count_tz));
			uniBrightDrawer.Radius(r * scl + 20f, r * scl + 25f).CenterCicle(30f * scl, 40f * scl, 160f).Thick(20f * num, 33f * num);
			uniBrightDrawer.drawTo(MdAdd, 0f, 0f, Mg.t, false);
		}

		public void fnHitAfter(AttackInfo Atk, IM2RayHitAble Target, HITTYPE touched_hittpe)
		{
			NelAttackInfo nelAttackInfo = Atk as NelAttackInfo;
			if (nelAttackInfo == null || nelAttackInfo.PublishMagic == null)
			{
				return;
			}
			MgItemBomb.MgBombMem mgBombMem = nelAttackInfo.PublishMagic.Other as MgItemBomb.MgBombMem;
			if (mgBombMem == null)
			{
				return;
			}
			if (Target is NelEnemy)
			{
				NelEnemy nelEnemy = Target as NelEnemy;
				if (!nelEnemy.is_alive || nelEnemy.destructed)
				{
					return;
				}
				if (!nelEnemy.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD))
				{
					nelEnemy.getSer().Add(SER.STRONG_HOLD, (int)X.NIL(380f, 110f, (float)mgBombMem.grade, 4f), 99, false);
				}
			}
		}

		private const int attack_count = 10;

		private const float damage_intv = 10f;

		private const float enemy_lock_mana_desire_min = 380f;

		private const float enemy_lock_mana_desire_max = 110f;

		private AttackInfo.FnHitAfter FD_fnHitAfter;
	}
}
