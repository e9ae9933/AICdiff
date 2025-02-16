using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASDangerChecker : NelEnemyAssist, IRunAndDestroy
	{
		public NASDangerChecker(NelEnemy _En, float _check_size)
			: base(_En)
		{
			this.Ray = this.En.nM2D.MGC.makeRay(this.En, false, true, false, 0f, false, false);
			this.t_danger = new RevCounter();
			this.check_size_ = _check_size;
			this.Ray.RadiusM(this.check_size_);
			this.Ray.LenM(0f);
			this.Ray.projectile_power = -1;
			this.Ray.hittype_to_week_projectile = HITTYPE.NONE;
			this.En.Mp.addRunnerObject(this);
		}

		public bool run(float fcnt)
		{
			if (this.enabled)
			{
				if (base.Nai.here_dangerous && base.Nai.isPrMagicExploded(1f))
				{
					this.t_danger.Max(this.hold_time, false);
				}
				else if ((this.Ray.hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
				{
					M2Ray reflectAnotherRay = this.Ray.ReflectAnotherRay;
					if (reflectAnotherRay != null && reflectAnotherRay.Atk is NelAttackInfo)
					{
						NelAttackInfo nelAttackInfo = reflectAnotherRay.ReflectAnotherRay.Atk as NelAttackInfo;
						if (nelAttackInfo.PublishMagic != null && nelAttackInfo.Caster != null && (this.only_target_player ? (this.En.AimPr as M2MagicCaster == nelAttackInfo.Caster) : (nelAttackInfo.Caster is M2MoverPr)) && ((this.consider_normal_attack && nelAttackInfo.PublishMagic.is_normal_attack) || (this.consider_magic_attack && !nelAttackInfo.PublishMagic.is_normal_attack)))
						{
							Vector2 mapPos = reflectAnotherRay.getMapPos(0f);
							float num = X.NI(base.sizex, base.sizey, 0.5f);
							float num2 = X.Mx(this.check_size_ - num, 0f) + 1f;
							if (X.chkLENLineCirc(mapPos.x, mapPos.y, reflectAnotherRay.Dir.x * num2, -reflectAnotherRay.Dir.y * num2, base.x, base.y, num))
							{
								this.t_danger.Max(this.hold_time, false);
							}
						}
					}
				}
			}
			this.Ray.hittype = (this.Ray.hittype | HITTYPE.TARGET_CHECKER) & (HITTYPE)(-12582945);
			this.Ray.PosMap(base.x, base.y);
			this.t_danger.Update(fcnt);
			return true;
		}

		public void destruct()
		{
			this.En.Mp.remRunnerObject(this);
			if (this.Ray != null)
			{
				this.En.nM2D.MGC.destructRay(this.Ray);
				this.Ray = null;
			}
		}

		public bool isDanger()
		{
			return this.t_danger > 0f;
		}

		public float check_size
		{
			get
			{
				return this.check_size_;
			}
			set
			{
				this.check_size_ = value;
				this.Ray.RadiusM(this.check_size_);
			}
		}

		private M2Ray Ray;

		public float hold_time = 8f;

		private float check_size_;

		public bool only_target_player = true;

		public bool consider_normal_attack;

		public bool consider_magic_attack = true;

		public bool enabled = true;

		private RevCounter t_danger;
	}
}
