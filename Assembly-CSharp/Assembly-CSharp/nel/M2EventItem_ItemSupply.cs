using System;
using m2d;
using UnityEngine;

namespace nel
{
	public class M2EventItem_ItemSupply : M2EventItem, IItemSupplier, IM2RayHitAble
	{
		public NelItemEntry[] getDataList(ref bool is_reel)
		{
			is_reel = this.LpCon.is_reel;
			return this.IReel.ToArray();
		}

		public Vector4 getShowPos()
		{
			return new Vector4(base.x, base.y, this.sizey * base.CLEN + 115f, -this.sizey * base.CLEN - 20f);
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return 1f;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			bool flag = EnemySummoner.isActiveBorder();
			if (!this.hitable_ || flag || this.LpCon.already_collected)
			{
				return RAYHIT.NONE;
			}
			return RAYHIT.HITTED | (flag ? RAYHIT.DO_NOT_AUTO_TARGET : RAYHIT.BREAK);
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.OTHER;
		}

		public bool hitable
		{
			get
			{
				return this.hitable_;
			}
			set
			{
				if (this.hitable_ == value)
				{
					return;
				}
				this.hitable_ = value;
				if (this.hitable_)
				{
					if (this.CC == null)
					{
						this.CC = new M2MvColliderCreatorAtk(this);
					}
					this.CC.enabled = true;
					return;
				}
				if (this.CC != null)
				{
					this.CC.enabled = false;
				}
			}
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (this.LpCon == null || !this.hitable_ || this.LpCon.already_collected || this.lock_t > this.Mp.floort)
			{
				return 0;
			}
			if (Atk != null && Atk.AttackFrom is M2MoverPr && Atk.AttackFrom == this.Mp.Pr && !this.Mp.playerActionUseable())
			{
				return 0;
			}
			this.lock_t = this.Mp.floort + 40f;
			this.LpCon.setHitEffect();
			if (this.LpCon.is_reel)
			{
				this.execute(M2EventItem.CMD.CHECK, this);
			}
			else
			{
				this.LpCon.activate(this.IReel.ToArray(), true, true);
				this.LpCon.checkPreAftEvent(true, null);
			}
			return 1;
		}

		private bool hitable_;

		public M2LpItemSupplier LpCon;

		public ReelManager.ItemReelContainer IReel;

		public float lock_t;
	}
}
