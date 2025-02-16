using System;
using m2d;
using XX;

namespace nel
{
	public class MgItemBombFlashBang : MgItemBomb
	{
		public MgItemBombFlashBang()
		{
			this.col_stroke_add0 = 4294965016U;
			this.col_stroke_add1 = 13864745U;
			this.col_stroke_sub0 = 2567403468U;
			this.col_stroke_sub1 = 489420U;
			this.col_explode_sub_circle = 2702601712U;
			this.ptc_key_explode = "itembomb_lig_explode";
			this.ptc_key_explode_prepare = null;
			this.snd_key_dro = "itembomb_lig_drop";
			this.explode_radius_min = 6.5f;
			this.explode_radius_max = 7.2f;
			this.pf_start_id = 2;
			this.SerDmgSelf = new FlagCounter<SER>(4).Add(SER.CONFUSE, 150f);
		}

		protected override void InitItemInner(MgItemBomb.MgBombMem Mem, float gratio)
		{
			NelAttackInfo atk = Mem.Mg.Atk0;
			if (this.FD_fnHitAfter == null)
			{
				this.FD_fnHitAfter = new AttackInfo.FnHitAfter(this.fnHitAfter);
			}
			atk.hpdmg0 = (int)X.NI(55, 130, gratio);
			atk.split_mpdmg = 45;
			atk.ignore_nodamage_time = true;
			atk.pr_myself_fire = true;
			atk.attr = MGATTR.NORMAL;
			atk.shield_break_ratio = 0.001f;
			atk.burst_vx = 0f;
			atk.burst_vy = -0.6f;
			atk.attack_max0 = -1;
			atk.burst_center = 1f;
			atk.huttobi_ratio = 0f;
			atk.centerblur_damage_ratio = 0.5f;
			atk.ndmg = NDMG.GRAB_PENETRATE;
			atk.damage_randomize_min = 0.65f;
			atk.fnHitEffectAfter = this.FD_fnHitAfter;
			Mem.Mg.phase = 100;
			Mem.Mg.Mn._1.thick = X.NI(this.explode_radius_min, this.explode_radius_max, gratio);
		}

		protected override bool runFlying(MagicItem Mg, float fcnt, bool on_ground)
		{
			if (!base.runFlying(Mg, fcnt, on_ground))
			{
				return false;
			}
			MgItemBombNormal.runFlyingLavaExp(Mg, fcnt, on_ground);
			return true;
		}

		public override void applyMapDamage(MagicItem Mg, M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc)
		{
			if (MapDmg.Atk.attr == MGATTR.FIRE)
			{
				base.initExplode(Mg);
			}
		}

		public override void initThrow(MagicItem Mg, bool dropping = false, bool self_explode_bomb = false)
		{
			base.initThrow(Mg, dropping, self_explode_bomb);
			Mg.playSndPos("itembomb_light_countdown", Mg.sx, Mg.sy, false, PTCThread.StFollow.FOLLOW_S);
		}

		public override void drawExplodePrepare(MagicItem Mg, MeshDrawer MdAdd, float count_tz, float r, float scl)
		{
			int num = (int)(Mg.t / 1f);
			int num2 = X.IntR(X.NIL(6f, 15f, (float)num, 20f));
			float num3 = 6.2831855f / (float)num2;
			for (int i = 0; i < num2; i++)
			{
				uint ran = X.GETRAN2(Mg.id % 7 + num * 9 + i, Mg.id % 19);
				float num4 = -1f + X.RAN(ran, 712 + i * 15) * 2f + num3 * (float)i;
				float num5 = r * scl * count_tz - 9f + X.RAN(ran, 2913) * 20f;
				MdAdd.Col = MdAdd.ColGrd.Set(this.col_stroke_add0).blend(this.col_stroke_add1, X.RAN(ran, 1263)).setA1(0.6f + 0.4f * X.RAN(ran, 2907))
					.C;
				MdAdd.Line(0f, 0f, X.Cos(num4) * num5, X.Sin(num4) * num5, 1f, false, 0f, 0f);
			}
		}

		protected override void explodeExecute(MagicItem Mg, bool first)
		{
			if (first)
			{
				Mg.projectile_power = 1;
				Mg.MGC.quitRayCohitable(Mg.Ray);
			}
			base.explodeExecute(Mg, first);
		}

		public void fnHitAfter(AttackInfo Atk, IM2RayHitAble Target, HITTYPE touched_hittpe)
		{
			NelAttackInfo nelAttackInfo = Atk as NelAttackInfo;
			if (nelAttackInfo == null || nelAttackInfo.PublishMagic == null)
			{
				return;
			}
			MagicItem publishMagic = nelAttackInfo.PublishMagic;
			MgItemBomb.MgBombMem mgBombMem = publishMagic.Other as MgItemBomb.MgBombMem;
			if (mgBombMem == null)
			{
				return;
			}
			if ((touched_hittpe & HITTYPE.HITTED_PR) != HITTYPE.NONE && Target is PR)
			{
				PR pr = Target as PR;
				if ((X.Abs(pr.x - publishMagic.sx) < 0.3f && X.Abs(pr.y - publishMagic.sy) < pr.sizey + 0.5f) || pr.mpf_is_right > 0f == pr.x < publishMagic.sx)
				{
					pr.PtcVar("t", 5.0).PtcST("flashbang_self_explode", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
					PostEffect.IT.setSlow(680f, 45.333332f, 0);
					bool flag;
					DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = pr.NM2D.DARKSPOT.Set(pr, DarkSpotEffect.SPOT.FLUSHBANG, out flag);
					if (flag)
					{
						darkSpotEffectItem.fade_t = 70f;
						darkSpotEffectItem.add_light_color = -0.15f;
						darkSpotEffectItem.mul_light_color = 1.9f;
						darkSpotEffectItem.sub_alpha = 0.56f;
						darkSpotEffectItem.PeHn.Set(PostEffect.IT.setPE(POSTM.BGM_LOWER, 100f, 1f, -100));
						darkSpotEffectItem.PeHn.Set(PostEffect.IT.setPE(POSTM.BGM_WATER, 100f, 1f, -100));
						darkSpotEffectItem.PeHn.Set(PostEffect.IT.setPE(POSTM.FINAL_ALPHA, 80f, 0.2f, -80));
					}
					else
					{
						darkSpotEffectItem.resetTime();
					}
					pr.Ser.applySerDamage(this.SerDmgSelf, pr.getSerApplyRatio(), -1);
				}
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
					nelEnemy.applyFlashBangEatenSer((int)X.NIL(300f, 650f, (float)mgBombMem.grade, 4f));
				}
			}
		}

		public const float flash_self_delay = 5f;

		public const float flash_self_maxt = 680f;

		private FlagCounter<SER> SerDmgSelf;

		private FlagCounter<SER> SerDmgEnemy;

		public const float eaten_freeze_grade_min = 300f;

		public const float eaten_freeze_grade_max = 650f;

		private AttackInfo.FnHitAfter FD_fnHitAfter;
	}
}
