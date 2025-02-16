using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MGContainer : RBase<MagicItem>
	{
		public MGContainer(NelM2DBase _M2D)
			: base(256, true, false, false)
		{
			this.M2D = _M2D;
			this.ray_target_layer = (byte)IN.LAY("TransparentFX");
			this.ACohitable = new List<M2Ray>(8);
			this.AtkDefault = new NelAttackInfo();
			this.PoolRay = new ClsPool<M2Ray>(() => new M2Ray(), 4);
			this.PoolAtk = new ClsPool<NelAttackInfo>(() => new NelAttackInfo(), 8);
			this.PoolSDmg = new ClsPool<FlagCounter<SER>>(() => new FlagCounter<SER>(4), 2);
			this.OPoolHitLink = new BDic<MGKIND, BDic<IM2RayHitAble, float>>(2);
			this.OHoldFD = new BDic<MGKIND, MgFDHolder>();
			this.OHoldFD[MGKIND.FIREBALL] = new MgFireBall();
			this.OHoldFD[MGKIND.PR_BURST] = new MgBurst();
			this.OHoldFD[MGKIND.DROPBOMB] = new MgDropBomb();
			this.OHoldFD[MGKIND.POWERBOMB] = new MgPowerBomb();
			this.OHoldFD[MGKIND.THUNDERBOLT] = new MgThunderbolt();
			this.OHoldFD[MGKIND.WHITEARROW] = new MgWhiteArrow();
			this.OHoldFD[MGKIND.GROUND_SHOCKWAVE] = new MgNGroundShockwave();
			this.OHoldFD[MGKIND.CANDLE_SHOT] = new MgNCandleShot();
			this.OHoldFD[MGKIND.ICE_SHOT] = new MgNIceShot();
			this.OHoldFD[MGKIND.ITEMBOMB_NORMAL] = new MgItemBombNormal();
			this.OHoldFD[MGKIND.ITEMBOMB_LIGHT] = new MgItemBombFlashBang();
			this.OHoldFD[MGKIND.ITEMBOMB_MAGIC] = new MgItemBombMagic();
			this.OHoldFD[MGKIND.CEILDROP] = new MgNCeilDrop();
			this.Notf = new MagicNotifiearData(this.OHoldFD);
		}

		public override MagicItem Create()
		{
			return new MagicItem(this);
		}

		public override void clear()
		{
			this.id = 0;
			this.no_set_kill_effect = true;
			for (int i = 0; i < this.LEN; i++)
			{
				MagicItem magicItem = this.AItems[i];
				if (magicItem != null)
				{
					magicItem.releasePooledObject(true, true);
				}
			}
			this.PoolRay.releaseAll(false, false);
			this.PoolAtk.releaseAll(false, false);
			this.PoolSDmg.releaseAll(false, false);
			this.ACohitable.Clear();
			this.clearPoolHitLink();
			base.clear();
		}

		public void clearPoolHitLink()
		{
			foreach (KeyValuePair<MGKIND, BDic<IM2RayHitAble, float>> keyValuePair in this.OPoolHitLink)
			{
				keyValuePair.Value.Clear();
			}
		}

		public void initS(Map2d _Mp)
		{
			this.Mp_ = _Mp;
			this.Notf.initS();
			this.clear();
		}

		public List<M2Ray> getCohitableRayList()
		{
			return this.ACohitable;
		}

		public MagicItem setMagic(M2MagicCaster Caster, MGKIND kind, MGHIT hittype = MGHIT.AUTO)
		{
			this.no_set_kill_effect = false;
			if (hittype == MGHIT.AUTO)
			{
				if (Caster is M2MoverPr)
				{
					hittype = MGHIT.PR;
				}
				else
				{
					hittype = MGHIT.EN;
				}
			}
			MagicItem magicItem = base.Pop(64);
			int num = this.id;
			this.id = num + 1;
			return magicItem.init(num, Caster, kind, hittype);
		}

		public MagicItem initFunc(MagicItem Mg)
		{
			return this.initFunc(Mg, Mg.kind);
		}

		public MagicItem initFunc(MagicItem Mg, MGKIND hold_key)
		{
			this.OHoldFD[hold_key].initFunc(Mg);
			return Mg;
		}

		public M2Ray makeRay(MagicItem Mg, float radius = 0f, bool wall_hit = true, bool other_object_hit = true)
		{
			M2Ray m2Ray = this.makeRay(Mg.Caster as M2Mover, (Mg.hittype & MGHIT.EN) > (MGHIT)0, (Mg.hittype & MGHIT.PR) > (MGHIT)0, Mg.is_chanted_magic, radius, wall_hit, other_object_hit);
			m2Ray.projectile_power = Mg.projectile_power;
			if (Mg.raypos_s)
			{
				m2Ray.PosMap(Mg.sx, Mg.sy);
			}
			if (Mg.raypos_d)
			{
				m2Ray.PosMap(Mg.dx, Mg.dy);
			}
			return m2Ray;
		}

		public M2Ray makeRay(M2Mover Caster, bool pr_hit, bool en_hit, bool is_chanted_magic, float radius = 0f, bool wall_hit = true, bool other_object_hit = true)
		{
			M2Ray m2Ray = this.PoolRay.Pool().Set(this.Mp, Caster, radius, (en_hit ? HITTYPE.EN : HITTYPE.NONE) | (pr_hit ? HITTYPE.PR : HITTYPE.NONE) | (wall_hit ? HITTYPE.WALL : HITTYPE.NONE) | (other_object_hit ? HITTYPE.OTHER : HITTYPE.NONE) | (is_chanted_magic ? HITTYPE.CHANTED_MAGIC : HITTYPE.NONE));
			m2Ray.ACohitableCheck = this.ACohitable;
			return m2Ray;
		}

		public BDic<IM2RayHitAble, float> getHitLink(MGKIND k)
		{
			BDic<IM2RayHitAble, float> bdic;
			if (!this.OPoolHitLink.TryGetValue(k, out bdic))
			{
				return this.OPoolHitLink[k] = new BDic<IM2RayHitAble, float>(4);
			}
			return bdic;
		}

		public void initRayCohitable(M2Ray Ray)
		{
			if (Ray == null || Ray.cohitable_assigned)
			{
				return;
			}
			Ray.cohitable_assigned = true;
			this.ACohitable.Add(Ray);
		}

		public void quitRayCohitable(M2Ray Ray)
		{
			if (Ray != null && Ray.cohitable_assigned)
			{
				Ray.cohitable_assigned = false;
				this.ACohitable.Remove(Ray);
			}
		}

		public M2Ray destructRay(M2Ray Ray)
		{
			if (Ray != null)
			{
				this.quitRayCohitable(Ray);
				Ray.releaseLinkedHittedTarget();
				this.PoolRay.Release(Ray);
			}
			return null;
		}

		public void killAllPlayerMagic(M2MoverPr Pr = null, Func<MagicItem, bool> Fn = null)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				MagicItem magicItem = this.AItems[i];
				if (((Pr == null) ? (magicItem.Caster is M2MoverPr) : (magicItem.Caster == Pr as M2MagicCaster)) && (Fn == null || Fn(magicItem)))
				{
					magicItem.kill(0f);
				}
			}
		}

		public NelAttackInfo makeAtk()
		{
			NelAttackInfo nelAttackInfo = this.PoolAtk.Pool();
			nelAttackInfo.CopyFrom(this.AtkDefault);
			return nelAttackInfo;
		}

		public NelAttackInfo destructAtk(NelAttackInfo Atk)
		{
			if (Atk != null)
			{
				this.destructSDmg(Atk.SerDmg);
				this.PoolAtk.Release(Atk);
			}
			return null;
		}

		public FlagCounter<SER> makeSDmg()
		{
			FlagCounter<SER> flagCounter = this.PoolSDmg.Pool();
			flagCounter.Clear();
			return flagCounter;
		}

		public FlagCounter<SER> destructSDmg(FlagCounter<SER> SDmg)
		{
			if (SDmg != null)
			{
				this.PoolSDmg.Release(SDmg);
			}
			return null;
		}

		public HITTYPE CircleCast(MagicItem Mg, M2Ray Ray, NelAttackInfo Atk, HITTYPE ignore_hittype = HITTYPE.NONE)
		{
			bool flag = false;
			HITTYPE hittype = Ray.hittype;
			if (Atk.pr_myself_fire)
			{
				flag = true;
				Ray.hittype |= (HITTYPE)17;
			}
			Ray.Atk = Atk;
			HITTYPE hittype2 = Ray.Cast(true, null, true);
			if (flag)
			{
				Ray.hittype = hittype;
			}
			if (hittype2 == HITTYPE.NONE)
			{
				return HITTYPE.NONE;
			}
			Atk.PublishMagic = Mg;
			int split_mpdmg = Atk.split_mpdmg;
			float burst_vx = Atk.burst_vx;
			float burst_vy = Atk.burst_vy;
			float num = -1f;
			float hpDamagePublishRatio = Mg.Caster.getHpDamagePublishRatio(Mg);
			Atk.CenterXy(this.Mp.globaluxToMapx(Ray.Pos.x), this.Mp.globaluyToMapy(Ray.Pos.y), Ray.radius_map);
			Atk.Caster = Mg.Caster;
			Atk.CurrentAbsorbedBy = null;
			Atk._burst_huttobi_thresh = 0f;
			if (Atk.attack_max == -2)
			{
				Atk.resetAttackCount();
			}
			float num2 = 0f;
			if (Atk.burst_center != 0f)
			{
				num = X.Abs(Atk.burst_vx);
				num2 = X.Abs(Atk.burst_center);
			}
			int hittedMax = Ray.getHittedMax();
			M2Attackable m2Attackable = (Atk.AttackFrom = Mg.Caster as M2Attackable);
			if (!Atk.Caster.initPublishAtk(Mg, Atk, HITTYPE.NONE, null))
			{
				return hittype2;
			}
			bool flag2 = true;
			int num3 = 0;
			while (num3 < hittedMax && !Mg.killed)
			{
				M2Ray.M2RayHittedItem hitted = Ray.GetHitted(num3);
				if ((ignore_hittype & hitted.type) == HITTYPE.NONE && hitted.type != HITTYPE.NONE)
				{
					Atk._apply_knockback_current = true;
					Atk.shuffleHpMpDmg(hitted.Mv, hpDamagePublishRatio, 1f, Atk.hpdmg0, Atk.mpdmg0);
					if (Atk.Caster.initPublishAtk(Mg, Atk, hitted.type, hitted))
					{
						IM2RayHitAble hit = hitted.Hit;
						if (hit == null)
						{
							hittype2 |= hitted.type;
						}
						else
						{
							Atk.HitXy(this.Mp.globaluxToMapx(hitted.hit_ux), this.Mp.globaluyToMapy(hitted.hit_uy), false);
							NelM2Attacker nelM2Attacker;
							M2Attackable m2Attackable2;
							HITTYPE hittype3;
							if (hit is NelEnemy)
							{
								if (!flag2)
								{
									goto IL_046B;
								}
								nelM2Attacker = hit as NelEnemy;
								m2Attackable2 = hit as NelEnemy;
								hittype2 |= HITTYPE.EN;
								hittype3 = HITTYPE.HITTED_EN;
							}
							else if (hit is PR)
							{
								if (!flag2)
								{
									goto IL_046B;
								}
								nelM2Attacker = hit as PR;
								m2Attackable2 = hit as PR;
								hittype2 |= HITTYPE.PR;
								hittype3 = HITTYPE.HITTED_PR;
								if (flag)
								{
									Atk.HitXy(Mg.sx, Mg.sy, false);
									if ((nelM2Attacker as PR).applyMySelfFire(Mg, Ray, Atk) < 0)
									{
										goto IL_046B;
									}
								}
							}
							else
							{
								nelM2Attacker = null;
								m2Attackable2 = null;
								hittype2 |= HITTYPE.OTHER;
								hittype3 = HITTYPE.HITTED_OTHER;
							}
							Atk._hitlock_ignore = false;
							bool flag3 = false;
							if (nelM2Attacker != null)
							{
								if (Atk.burst_center != 0f)
								{
									float num4 = this.Mp.GAR(Atk.center_x, Atk.center_y, m2Attackable2.x, m2Attackable2.y);
									float num5 = X.Cos(num4);
									float num6 = -X.Sin(num4) * num * num2;
									Atk._burst_huttobi_thresh = X.Mn(0f, num6);
									float num7;
									if (Atk.burst_center < 0f)
									{
										num7 = burst_vx * X.ZLINE(1f - num2);
									}
									else
									{
										num7 = X.Abs(burst_vx) * (float)X.MPF(num5 > 0f) * X.ZLINE(1f - num2);
									}
									Atk.Burst(num7 + num5 * num * num2, burst_vy * X.ZLINE(1f - num2) + num6);
								}
								if (Atk.ignore_nodamage_time)
								{
									(nelM2Attacker as M2Attackable).penetrateNoDamageTime(Atk.ndmg, -1);
								}
								if (nelM2Attacker.applyDamage(Atk, false) > 0)
								{
									hittype2 |= hittype3;
									flag3 = true;
								}
								if (Atk._apply_knockback_current && Atk.knockback_len > 0f && m2Attackable != null)
								{
									M2Attackable m2Attackable3 = nelM2Attacker as M2Attackable;
									m2Attackable.addKnockBack(m2Attackable3, Atk, (float)((Ray.difx == 0f) ? X.MPF(m2Attackable.x < m2Attackable3.x) : X.MPF(Ray.difx > 0f)));
								}
								if (Atk.reduceAttackCount())
								{
									flag2 = false;
								}
							}
							else if (hit.applyHpDamage(Atk.hpdmg0, false, Atk) > 0)
							{
								hittype2 |= hittype3;
								flag3 = true;
							}
							if (!Atk._hitlock_ignore)
							{
								Ray.setHitLock(hit);
							}
							if (Atk.fnHitEffectAfter != null)
							{
								Atk.fnHitEffectAfter(Atk, hitted.Hit, flag3 ? hittype3 : HITTYPE.NONE);
							}
						}
					}
				}
				IL_046B:
				num3++;
			}
			Atk.split_mpdmg = split_mpdmg;
			Atk.hpdmg_current = -1000;
			Atk.mpdmg_current = -1000;
			Atk.Burst(burst_vx, burst_vy);
			return hittype2 & ~ignore_hittype;
		}

		public void applyWind(WindItem Wind, float vx, float vy)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				MagicItem magicItem = this.AItems[i];
				if (magicItem.wind_apply_s_level > 0f && magicItem.Ray != null)
				{
					float num = magicItem.wind_apply_s_level * Wind.isinP(magicItem.sx, magicItem.sy, magicItem.Ray.radius_map * 0.3f);
					if (num > 0f)
					{
						magicItem.wind_velocity = new Vector3(vx * num, vy * num, 12f);
					}
				}
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Mp_;
			}
		}

		public MagicItem getMg(int i)
		{
			return this.AItems[i];
		}

		public MagicItem FindMg(MagicItem.FnCheckMagicFrom Fn, M2MagicCaster Caster)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				MagicItem magicItem = this.AItems[i];
				if (Fn(magicItem, Caster))
				{
					return magicItem;
				}
			}
			return null;
		}

		public int countMg(MagicItem.FnCheckMagicFrom Fn, M2MagicCaster Caster)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				MagicItem magicItem = this.AItems[i];
				if (Fn(magicItem, Caster))
				{
					num++;
				}
			}
			return num;
		}

		public MagicItem FindMg(float cx, float cy, float margin_x, float margin_y, MagicItem.FnCheckMagicFrom Fn, M2MagicCaster Caster)
		{
			return this.FindMg(cx, cy - margin_y, margin_y * 2f, 1.5707964f, 0f, margin_x, Fn, Caster);
		}

		public MagicItem FindMg(float cx, float cy, float len, float agR, float margin_f, float margin_t, MagicItem.FnCheckMagicFrom Fn, M2MagicCaster Caster)
		{
			float num = X.Cos(agR);
			float num2 = X.Sin(agR);
			for (int i = 0; i < this.LEN; i++)
			{
				MagicItem magicItem = this.AItems[i];
				if (magicItem.Ray != null)
				{
					Vector2 mapPos = magicItem.Ray.getMapPos(0f);
					float num3 = mapPos.x - cx;
					float num4 = mapPos.y - cy;
					Vector2 vector = X.ROTV2e(new Vector2(num3, -num4), num, -num2);
					if (X.BTW(-margin_f, vector.x, len + margin_f) && X.Abs(vector.y) < margin_t && Fn(magicItem, Caster))
					{
						return magicItem;
					}
				}
			}
			return null;
		}

		public readonly NelM2DBase M2D;

		private Map2d Mp_;

		private MagicItem[] AMg;

		private MagicItem[] AMgBuf;

		private readonly BDic<MGKIND, MgFDHolder> OHoldFD;

		public readonly MagicNotifiearData Notf;

		private ClsPool<M2Ray> PoolRay;

		private ClsPool<NelAttackInfo> PoolAtk;

		private ClsPool<FlagCounter<SER>> PoolSDmg;

		private NelAttackInfo AtkDefault;

		private int id;

		private byte ray_target_layer;

		public bool no_set_kill_effect;

		private List<M2Ray> ACohitable;

		private BDic<MGKIND, BDic<IM2RayHitAble, float>> OPoolHitLink;

		public const int MAGIC_MAX = 256;
	}
}
