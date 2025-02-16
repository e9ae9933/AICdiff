using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2ShieldHitable : MonoBehaviour, IM2RayHitAble, M2MagicCaster
	{
		public float scale
		{
			get
			{
				return this.Sld.scale;
			}
		}

		public void Init(M2Shield _Sld)
		{
			this.Sld = _Sld;
			this.SrcCaster = this.Sld.Mv as M2MagicCaster;
			this.Cld = base.gameObject.AddComponent<PolygonCollider2D>();
			this.Cld.pathCount = 1;
			this.Aposition = new Vector2[4];
		}

		public void OnEnable()
		{
			this.floort_lock = 0f;
			base.gameObject.layer = IN.LAY("TransparentFX");
		}

		public void reposit(float w, float scale)
		{
			Vector2 visivilityPos = this.Sld.getVisivilityPos();
			IN.Pos2(base.transform, this.Mp.map2ux(visivilityPos.x), this.Mp.map2uy(visivilityPos.y));
			if (this.PreWidthScale.x != w || this.PreWidthScale.y != scale)
			{
				this.PreWidthScale.Set(w, scale);
				w = w * scale * 0.015625f;
				float num = w * 1.18f;
				this.Aposition[0] = new Vector2(-w, 0f);
				this.Aposition[1] = new Vector2(0f, num);
				this.Aposition[2] = new Vector2(w, 0f);
				this.Aposition[3] = new Vector2(0f, -num);
			}
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			if (!base.enabled)
			{
				return RAYHIT.NONE;
			}
			return (RAYHIT)3;
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.PR;
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return -1f;
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (Atk == null || !base.enabled || this.Mp.floort < this.floort_lock)
			{
				return 0;
			}
			this.floort_lock = 0f;
			this.Sld.checkShield(Atk, (float)Atk.hpdmg_current, true);
			return 0;
		}

		public Map2d Mp
		{
			get
			{
				return this.Sld.Mp;
			}
		}

		public Vector2 getCenter()
		{
			return this.Sld.getVisivilityPos();
		}

		public Vector2 getTargetPos()
		{
			return this.Sld.getVisivilityPos();
		}

		public Vector2 getAimPos(MagicItem Mg)
		{
			Vector2 visivilityPos = this.Sld.getVisivilityPos();
			visivilityPos.x += this.Sld.Mv.mpf_is_right * 4f;
			return visivilityPos;
		}

		public int getAimDirection()
		{
			return this.SrcCaster.getAimDirection();
		}

		public float getPoseAngleRForCaster()
		{
			return this.SrcCaster.getPoseAngleRForCaster();
		}

		public float getHpDamagePublishRatio(MagicItem Mg)
		{
			return this.SrcCaster.getHpDamagePublishRatio(Mg);
		}

		public float getCastingTimeScale(MagicItem Mg)
		{
			return this.SrcCaster.getCastingTimeScale(Mg);
		}

		public float getCastableMp()
		{
			return this.SrcCaster.getCastableMp();
		}

		public bool canHoldMagic(MagicItem Mg)
		{
			return this.Sld.isLariatState() && !this.Sld.isManipulatableState();
		}

		public bool isManipulatingMagic(MagicItem Mg)
		{
			return false;
		}

		public bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitTarget)
		{
			if (HitTarget != null && HitTarget.Hit is M2Mover)
			{
				if (Atk == null)
				{
					return false;
				}
				this.Sld.checkShield(Atk, 16f, true);
			}
			return true;
		}

		public void initPublishKill(M2MagicCaster Target)
		{
			this.SrcCaster.initPublishKill(Target);
		}

		public AIM getAimForCaster()
		{
			return this.SrcCaster.getAimForCaster();
		}

		public void setAimForCaster(AIM a)
		{
			this.SrcCaster.setAimForCaster(a);
		}

		private M2Shield Sld;

		private M2MagicCaster SrcCaster;

		private PolygonCollider2D Cld;

		private bool enabled_;

		private Vector2[] Aposition;

		private Vector2 PreWidthScale;

		public float floort_lock;
	}
}
