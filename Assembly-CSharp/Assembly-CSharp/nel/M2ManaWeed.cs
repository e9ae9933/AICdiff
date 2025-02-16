using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2ManaWeed : MonoBehaviour, IM2RayHitAble, IRunAndDestroy
	{
		public void appear(NelChipManaWeed _Dr, Map2d _Mp)
		{
			this.Mp = _Mp;
			base.gameObject.isStatic = !_Dr.arrangeable;
			this.M2D = this.Mp.M2D as NelM2DBase;
			this.Dr = _Dr;
			this._tostring = "ManaWeed-" + _Dr.mapcx.ToString() + "," + _Dr.mapcy.ToString();
			if (this.Lig == null)
			{
				this.Lig = new M2Light(_Mp, null);
				this.Lig.Pos(_Dr.mapcx, _Dr.mapcy);
				this.Lig.follow_speed = 1f;
				this.Lig.Col.Set(2859760228U);
				this.Lig.radius = 40f;
				this.Mp.addLight(this.Lig, -1);
			}
			this.FirstPos = base.transform.localPosition;
			if (this.LpSummon != null && this.LpSummon.destructed)
			{
				this.LpSummon = null;
			}
			int count_layers = this.Mp.count_layers;
			LabelPointListener<M2LabelPoint> labelPointListener = new LabelPointListener<M2LabelPoint>();
			int num = 0;
			while (num < count_layers && this.LpSummon == null)
			{
				this.Mp.getLayer(num).LP.beginAll(labelPointListener);
				while (labelPointListener.next())
				{
					if (labelPointListener.cur is IM2WeedManager && labelPointListener.cur.isConveringMapXy(this.Dr.mleft, this.Dr.mtop, this.Dr.mright, this.Dr.mbottom - 0.1f))
					{
						this.LpSummon = labelPointListener.cur as IM2WeedManager;
						this.LpSummon.assignWeed(this);
						break;
					}
				}
				num++;
			}
			this.Mp.addRunnerObject(this);
		}

		public void finePos(float trans_ux = 0f, float trans_uy = 0f)
		{
			base.transform.localPosition = new Vector3(this.FirstPos.x + trans_ux, this.FirstPos.y + trans_uy, this.FirstPos.z);
		}

		public void closeAction()
		{
			if (this.LpSummon != null)
			{
				this.LpSummon.deassignWeed(this);
			}
			if (this.Lig != null)
			{
				this.Mp.remLight(this.Lig);
			}
			this.Mp.remRunnerObject(this);
			this.Lig = null;
		}

		public void destruct()
		{
		}

		public bool run(float fcnt)
		{
			if (this.Dr == null)
			{
				return true;
			}
			if (this.time > 0f)
			{
				if (this.Lig.radius != 30f)
				{
					this.Lig.radius = X.MULWALKMN(this.Lig.radius, 30f, 0.04f, 0.0001f);
				}
				if (this.safe_after_battle_ && this.time == 6000f)
				{
					return true;
				}
				this.time -= fcnt;
				if (this.time <= 0f)
				{
					this.cureImmediate(true, 0);
				}
			}
			else if (this.time < 0f)
			{
				this.time = X.Mn(this.time + fcnt, 0f);
				this.Lig.radius = X.MULWALK(this.Lig.radius, 40f, 0.04f);
			}
			return true;
		}

		public void cureImmediate(bool force, int margin = 0)
		{
			float num = this.time;
			if (this.time > 0f || force)
			{
				if (margin > 0)
				{
					this.time = X.Mn(this.time, (float)margin);
				}
				else
				{
					this.time = -120f;
					Vector3 localPosition = base.transform.localPosition;
					float num2 = this.Mp.uxToMapx(localPosition.x);
					this.Mp.PtcSTsetVar("x", (double)num2).PtcSTsetVar("y", (double)X.NI(this.Dr.mtop, this.Dr.mbottom, 0.6f)).PtcST("appear_weed", null, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (this.LpSummon != null && (num > 0f || force) && this.time <= 0f)
			{
				this.LpSummon.assignActiveWeed(this);
			}
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			if (!this.isActive())
			{
				return RAYHIT.DO_NOT_AUTO_TARGET;
			}
			return (RAYHIT)33;
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo _Atk = null)
		{
			NelAttackInfo nelAttackInfo = _Atk as NelAttackInfo;
			if (this.time > 0f || nelAttackInfo == null || nelAttackInfo.AttackFrom == null || nelAttackInfo.PublishMagic == null)
			{
				return 0;
			}
			MANA_HIT mana_HIT = MANA_HIT.ALL;
			if (nelAttackInfo.Caster is M2MoverPr)
			{
				if (nelAttackInfo.PublishMagic.projectile_power >= 0 || !nelAttackInfo.PublishMagic.is_normal_attack)
				{
					return 0;
				}
			}
			else
			{
				if (!(nelAttackInfo.Caster is NelEnemy))
				{
					return 0;
				}
				if ((nelAttackInfo.Caster as NelEnemy).nattr_mp_stable)
				{
					mana_HIT = MANA_HIT.EN | MANA_HIT.FROM_SUPPLIER;
				}
			}
			Vector3 localPosition = base.transform.localPosition;
			float num = this.Mp.uxToMapx(localPosition.x);
			float num2 = this.Mp.uyToMapy(localPosition.y);
			if (!X.BTW(num - nelAttackInfo.AttackFrom.sizex - 2f, nelAttackInfo.AttackFrom.x, num + nelAttackInfo.AttackFrom.sizex + 2f) || !X.BTW(num2 - nelAttackInfo.AttackFrom.sizey - 2f, nelAttackInfo.AttackFrom.y, num2 + nelAttackInfo.AttackFrom.sizey + 2f))
			{
				return 0;
			}
			this.SplashManaInit(mana_HIT, num, num2, nelAttackInfo.center_x, nelAttackInfo.center_y, nelAttackInfo.hit_x, nelAttackInfo.hit_y, true);
			if (this.LpSummon != null)
			{
				mana_HIT = this.LpSummon.deassignActiveWeed(this, _Atk);
			}
			return 1;
		}

		public void SplashManaInit(MANA_HIT mana_hit, float hit_x, float hit_y, bool play_snd = true, bool call_deassign_weed = false)
		{
			if (call_deassign_weed && this.LpSummon != null)
			{
				mana_hit |= this.LpSummon.deassignActiveWeed(this, null);
			}
			Vector3 localPosition = base.transform.localPosition;
			float num = this.Mp.uxToMapx(localPosition.x);
			float num2 = this.Mp.uyToMapy(localPosition.y);
			this.SplashManaInit(mana_hit, num, num2, hit_x, hit_y, hit_x, hit_y, play_snd);
		}

		private void SplashManaInit(MANA_HIT mana_hit, float cx, float cy, float center_x, float center_y, float hit_x, float hit_y, bool play_snd = true)
		{
			if (mana_hit != MANA_HIT.NOUSE)
			{
				this.M2D.Mana.AddMulti(cx, cy, (20f + (float)X.xors(11)) * this.M2D.NightCon.ManaWeedRatio(), mana_hit, 1f);
			}
			this.time = (this.safe_after_battle_ ? 6000f : X.NIXP(this.charge_time_min, this.charge_time_max));
			this.Mp.PtcSTsetVar("x", (double)center_x).PtcSTsetVar("y", (double)center_y).PtcSTsetVar("hit_x", (double)hit_x)
				.PtcSTsetVar("hit_y", (double)hit_y)
				.PtcSTsetVar("weed_x", (double)cx)
				.PtcSTsetVar("weed_y", (double)cy)
				.PtcSTsetVar("_playsnd", (double)(play_snd ? 1 : 0))
				.PtcST("break_weed", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public bool safe_after_battle
		{
			get
			{
				return this.safe_after_battle_;
			}
			set
			{
				if (this.safe_after_battle_ == value)
				{
					return;
				}
				this.safe_after_battle_ = value;
				if (this.time > 0f && (value || this.time == 6000f))
				{
					this.time = X.Mn(this.time, X.NIXP(60f, 90f));
				}
			}
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.OTHER;
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return 0f;
		}

		public bool isActive()
		{
			return this.time <= 0f;
		}

		public void OnDestroy()
		{
			this.closeAction();
			this.Mp = null;
		}

		public M2BlockColliderContainer.BCCLine GetBcc()
		{
			return this.Dr.GetBcc();
		}

		public bool destructed
		{
			get
			{
				return this.Mp == null;
			}
		}

		public float mleft
		{
			get
			{
				return this.Dr.mapcx - this.Dr.sizex;
			}
		}

		public float mright
		{
			get
			{
				return this.Dr.mapcx + this.Dr.sizex;
			}
		}

		public float mbottom
		{
			get
			{
				return this.Dr.mbottom;
			}
		}

		public float mtop
		{
			get
			{
				return this.Dr.mbottom - this.Dr.sizey * 2f;
			}
		}

		public float sizex
		{
			get
			{
				return this.Dr.sizex;
			}
		}

		public float sizey
		{
			get
			{
				return this.Dr.sizey;
			}
		}

		public override string ToString()
		{
			return this._tostring;
		}

		public float mapcx
		{
			get
			{
				return this.Dr.mapcx;
			}
		}

		public float mapcy
		{
			get
			{
				return this.Dr.mbottom - this.Dr.sizey;
			}
		}

		private float time;

		private const float CHARGE_TIME_MIN_DEF = 2400f;

		private const float CHARGE_TIME_MAX_DEF = 3000f;

		public float charge_time_min = 2400f;

		public float charge_time_max = 3000f;

		private const float charge_time_when_safe = 6000f;

		private const float light_radius_min = 30f;

		private const float light_radius_max = 40f;

		private Map2d Mp;

		private NelM2DBase M2D;

		private NelChipManaWeed Dr;

		private M2Light Lig;

		private IM2WeedManager LpSummon;

		private bool safe_after_battle_;

		private Vector3 FirstPos;

		private M2BlockColliderContainer.BCCLine BelongTo_;

		private string _tostring;

		public struct WeedLink
		{
			public int isLocked(Map2d Mp)
			{
				if (!(this.Weed != null) || Mp.floort >= this.lock_floort)
				{
					return 0;
				}
				if (!this.hard_link)
				{
					return 1;
				}
				return 2;
			}

			public void Clear()
			{
				this.Weed = null;
				this.lock_floort = 0f;
				this.hard_link = false;
			}

			public bool valid(Map2d Mp, M2ManaWeed _WeedTarget = null)
			{
				return ((_WeedTarget == null) ? (this.Weed != null) : (this.Weed == _WeedTarget)) && this.Weed.isActive() && Mp.floort < this.lock_floort;
			}

			public void Init(M2ManaWeed _Weed, float t, bool _hard_link = false)
			{
				if (_Weed == null)
				{
					this.Clear();
					return;
				}
				this.Weed = _Weed;
				this.lock_floort = this.Weed.Mp.floort + t;
				this.hard_link = _hard_link;
			}

			public M2ManaWeed Weed;

			public float lock_floort;

			public bool hard_link;
		}
	}
}
