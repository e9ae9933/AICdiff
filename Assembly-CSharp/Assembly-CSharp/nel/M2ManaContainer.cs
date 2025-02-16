using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class M2ManaContainer : RBase<M2Mana>
	{
		public M2ManaContainer(Map2d _Mp)
			: base(32, true, false, false)
		{
			this.Mp = _Mp;
			this.ABuf = new M2Mana[8];
			this.ODesire = new BDic<NelM2Attacker, float>();
			this.Ostock = new BDic<MANA_HIT, byte>();
			this.can_collect_en_mana_immediately = -1;
			this.AreaDr = new DRect("");
		}

		public override M2Mana Create()
		{
			return new M2Mana(this);
		}

		public M2Mana Add(float appearx, float appeary, float mapx, float mapy, float start_agR, MANA_HIT mana_hit = MANA_HIT.ALL)
		{
			if (this.Ed == null)
			{
				if (this.FD_fnDrawMana == null)
				{
					this.FD_fnDrawMana = new M2DrawBinder.FnEffectBind(this.fnDrawMana);
				}
				this.Ed = this.Mp.setED("ManaCon", this.FD_fnDrawMana, 0f);
			}
			M2Mana m2Mana = base.Pop(32);
			int num = this.index_count;
			this.index_count = num + 1;
			M2Mana m2Mana2 = m2Mana.clear(mapx, mapy, mapx, mapy, start_agR, mana_hit, num);
			if (this.target_t == 0f)
			{
				this.target_t = 10f;
			}
			if ((mana_hit & MANA_HIT.EN) != MANA_HIT.NOUSE)
			{
				this.can_collect_en_mana_immediately = -1;
			}
			return m2Mana2;
		}

		public void AddMulti(float mapx, float mapy, float mp, MANA_HIT mana_hit = MANA_HIT.ALL)
		{
			byte b2;
			byte b = (this.Ostock.TryGetValue(mana_hit, out b2) ? (b2 / 32) : 0);
			mp += (float)b;
			int num = (int)(mp / 4f);
			b = (byte)(X.IntR(mp * 32f) - num * 4 * 32);
			this.Ostock[mana_hit] = b;
			float num2 = 0.05f + X.ZLINE((float)(num - 2), 5f) * 0.11f;
			int num3 = (int)((float)num * 0.3f);
			for (int i = 0; i < num; i++)
			{
				float num4 = ((((float)i + 0.5f) / (float)num - 0.5f) * num2 + 0.023f * (-0.5f + X.XORSP()) + 0.25f) * 6.2831855f;
				this.Add(mapx, mapy - 1f, mapx + X.Cos(num4) * X.NIXP(1.3f, 2f), mapy + X.Sin(num4) * X.NIXP(0.2f, 0.4f) + X.NIXP(0f, 0.4f), num4, mana_hit | ((i < num3) ? MANA_HIT.IMMEDIATE_COLLECTABLE : MANA_HIT.NOUSE));
			}
			if ((mana_hit & MANA_HIT.CHECK_BIT) != MANA_HIT.NOUSE && (mana_hit & MANA_HIT.FALL) != MANA_HIT.NOUSE && (mana_hit & MANA_HIT.CRYSTAL) == MANA_HIT.NOUSE)
			{
				if ((mana_hit & MANA_HIT.FROM_DAMAGE_SPLIT) > MANA_HIT.NOUSE && (mana_hit & MANA_HIT.PR) == MANA_HIT.NOUSE)
				{
					this.Mp.PtcST("pr_mana_fall_absorbed_init", null, PTCThread.StFollow.NO_FOLLOW);
				}
				this.AddMulti(mapx, mapy, X.NIXP(4f, 8f) * 4f, MANA_HIT.FALL | (mana_hit & MANA_HIT.FROM_DAMAGE_SPLIT) | (((mana_hit & MANA_HIT.PR) != MANA_HIT.NOUSE) ? MANA_HIT.FALL_PR : MANA_HIT.FALL_EN));
			}
		}

		public void transformMana(float mapx, float mapy, float len, MANA_HIT mana_hit = MANA_HIT.ALL)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2Mana m2Mana = this.AItems[i];
				if (!m2Mana.only_effect && !m2Mana.from_player_gage && ((m2Mana.mana_hit != mana_hit && len < 0f) || X.chkLEN(m2Mana.x, m2Mana.y, mapx, mapy, len)))
				{
					m2Mana.transformManaHitType(mana_hit);
				}
			}
			this.fineRecheckTarget(10f);
		}

		public override bool run(float fcnt)
		{
			if (this.can_collect_en_mana_immediately == -1)
			{
				this.can_collect_en_mana_immediately = (this.Mp.isPlayerFacingEnemy() ? 0 : 1);
			}
			if (!base.run(fcnt))
			{
				if (this.Ed != null)
				{
					this.Ed = this.Mp.remED(this.Ed);
					this.Ed = null;
					this.index_count = 0;
				}
				this.target_t = 0f;
				return false;
			}
			this.anmp = X.ANMP((int)this.Mp.floort, 24, 1f);
			if (this.target_t > 0f)
			{
				this.target_t = X.VALWALK(this.target_t, 0f, fcnt);
				if (this.target_t == 0f)
				{
					this.considerTarget();
				}
			}
			return true;
		}

		public void considerTarget()
		{
			int num = 0;
			int num2 = 0;
			float num3 = 0f;
			float num4 = 0f;
			this.ODesire.Clear();
			int num5 = 0;
			if (this.TargetFirst != null)
			{
				if (this.TargetFirst.isActive())
				{
					num5 = X.Mx(0, X.isinC<M2Mana>(this.AItems, this.TargetFirst, this.LEN));
				}
				this.TargetFirst = null;
			}
			int num6 = 0;
			this.AreaDr.w = (this.AreaDr.h = 0f);
			for (int i = 0; i < this.LEN; i++)
			{
				M2Mana m2Mana = this.AItems[(i + num5) % this.LEN];
				NelM2Attacker target = m2Mana.getTarget();
				if (target != null)
				{
					float num7;
					if (!this.ODesire.TryGetValue(target, out num7))
					{
						this.ODesire[target] = -1f;
					}
					else
					{
						this.ODesire[target] = num7 - 1f;
					}
				}
				else
				{
					int num8 = (int)(m2Mana.mana_hit & MANA_HIT.CHECK_BIT);
					if ((num8 & 3) != 0)
					{
						if (num == 0)
						{
							num3 = m2Mana.x;
							num4 = m2Mana.y;
							num6 = num8;
						}
						else if (num8 != num6 || X.LENGTHXYS(m2Mana.x, m2Mana.y, num3, num4) >= 7f)
						{
							num2++;
							if (this.TargetFirst == null)
							{
								this.TargetFirst = m2Mana;
								goto IL_0194;
							}
							goto IL_0194;
						}
						this.ABuf[num++] = m2Mana;
						this.AreaDr.Expand(m2Mana.x - 0.0625f, m2Mana.y - 0.0625f, 0.125f, 0.125f, false);
						if (num >= 8)
						{
							break;
						}
					}
				}
				IL_0194:;
			}
			this.target_t = (float)((num2 > 0) ? 10 : 0);
			if (num == 0)
			{
				return;
			}
			int count_movers = this.Mp.count_movers;
			float num9 = 0f;
			NelM2Attacker nelM2Attacker = null;
			NelM2Attacker nelM2Attacker2 = null;
			float num10 = 0f;
			float num11 = 0f;
			this.ODesire.Clear();
			for (int j = 0; j < count_movers; j++)
			{
				M2Attackable m2Attackable = this.Mp.getMv(j) as M2Attackable;
				if (m2Attackable is NelM2Attacker)
				{
					NelM2Attacker nelM2Attacker3 = m2Attackable as NelM2Attacker;
					float num12;
					float manaDesire = MDAT.getManaDesire(nelM2Attacker3, m2Attackable, num6, this.AreaDr, this.ODesire.TryGetValue(nelM2Attacker3, out num12) ? ((int)X.Mx(0f, -num12)) : 0);
					if (manaDesire > 0f)
					{
						num9 += manaDesire;
						this.ODesire[nelM2Attacker3] = manaDesire;
						if (nelM2Attacker == null || num10 > manaDesire)
						{
							nelM2Attacker = nelM2Attacker3;
							num10 = manaDesire;
						}
						if (nelM2Attacker2 == null || num11 < manaDesire)
						{
							nelM2Attacker2 = nelM2Attacker3;
							num11 = manaDesire;
						}
					}
				}
			}
			if (num9 == 0f)
			{
				this.TargetFirst = this.AItems[X.xors(this.LEN)];
				this.target_t = 15f;
				return;
			}
			X.shuffle<M2Mana>(this.ABuf, num);
			int num13 = 0;
			using (Dictionary<NelM2Attacker, float>.Enumerator enumerator = this.ODesire.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<NelM2Attacker, float> keyValuePair = enumerator.Current;
					NelM2Attacker key = keyValuePair.Key;
					if (key != nelM2Attacker && keyValuePair.Value >= 0f)
					{
						float num14 = (float)num * keyValuePair.Value / num9;
						int num15 = X.Mn(num - num13, (key == nelM2Attacker2) ? X.IntC(num14) : X.IntR(num14));
						for (int k = 0; k < num15; k++)
						{
							this.ABuf[num13++].setTarget(key, num13, keyValuePair.Value);
						}
					}
				}
				goto IL_03AF;
			}
			IL_0395:
			this.ABuf[num13].setTarget(nelM2Attacker, num13, num10);
			num13++;
			IL_03AF:
			if (num13 >= num)
			{
				return;
			}
			goto IL_0395;
		}

		public override void destruct()
		{
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			base.destruct();
			this.TargetFirst = null;
			this.Ed = null;
			this.index_count = 0;
		}

		public void fineRecheckTarget(float t = 10f)
		{
			this.target_t = ((this.target_t <= 0f) ? t : X.Mn(this.target_t, t));
		}

		private bool fnDrawMana(EffectItem Ef, M2DrawBinder Ed)
		{
			MeshDrawer meshImg = Ef.GetMeshImg("mana_container_s", MTRX.MIicon, BLEND.SUB, true);
			MeshDrawer mesh = Ef.GetMesh("mana_container_stroke", MTRX.MtrMeshAdd, true);
			MeshDrawer mesh2 = Ef.GetMesh("mana_container_a", MTRX.MIicon.getMtr(BLEND.ADD, -1), true);
			PxlFrame pxlFrame = MTRX.AEff[0];
			meshImg.initForImg(pxlFrame.getLayer(0).Img, 0);
			for (int i = 0; i < this.LEN; i++)
			{
				M2Mana m2Mana = this.AItems[i];
				meshImg.base_x = (mesh2.base_x = (mesh.base_x = this.Mp.ux2effectScreenx(this.Mp.map2ux(m2Mana.x))));
				meshImg.base_y = (mesh2.base_y = (mesh.base_y = this.Mp.uy2effectScreeny(this.Mp.map2uy(m2Mana.y + m2Mana.y_vib))));
				m2Mana.draw(meshImg, mesh2, mesh, (float)X.AF * Map2d.TSbase);
			}
			if (this.LEN == 0)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			return true;
		}

		public readonly Map2d Mp;

		private M2DrawBinder Ed;

		public int index_count;

		private float target_t;

		private DRect AreaDr;

		private M2Mana[] ABuf;

		private const int one_check_mana_max = 8;

		private BDic<NelM2Attacker, float> ODesire;

		private BDic<MANA_HIT, byte> Ostock;

		private M2Mana TargetFirst;

		public int can_collect_en_mana_immediately = -1;

		public float anmp;

		public const float immediate_collectable_ratio = 0.3f;

		public const float juice_mana_mulitple = 12f;

		public const float juice_mana_mulitple_max = 48f;

		private M2DrawBinder.FnEffectBind FD_fnDrawMana;
	}
}
