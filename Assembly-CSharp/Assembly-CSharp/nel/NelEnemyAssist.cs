using System;
using m2d;

namespace nel
{
	public class NelEnemyAssist
	{
		public NelEnemyAssist(NelEnemy _En)
		{
			this.En = _En;
			this.Phy = this.En.getPhysic();
			this.FootD = ((this.Phy != null) ? this.Phy.getFootManager() : null);
			this.Anm = this.En.getAnimator();
		}

		public M2MoverPr AimPr
		{
			get
			{
				return this.En.AimPr;
			}
		}

		public M2BlockColliderContainer.BCCLine targetBCC
		{
			get
			{
				M2MoverPr aimPr = this.AimPr;
				if (aimPr == null)
				{
					return null;
				}
				M2FootManager footManager = aimPr.getFootManager();
				if (footManager == null)
				{
					return null;
				}
				return footManager.get_LastBCC();
			}
		}

		public float x
		{
			get
			{
				return this.En.x;
			}
		}

		public float y
		{
			get
			{
				return this.En.y;
			}
		}

		public float sizex
		{
			get
			{
				return this.En.sizex;
			}
		}

		public float sizey
		{
			get
			{
				return this.En.sizey;
			}
		}

		public float mleft
		{
			get
			{
				return this.En.mleft;
			}
		}

		public float mright
		{
			get
			{
				return this.En.mright;
			}
		}

		public float mtop
		{
			get
			{
				return this.En.mtop;
			}
		}

		public float mbottom
		{
			get
			{
				return this.En.mbottom;
			}
		}

		public float mpf_is_right
		{
			get
			{
				return this.En.mpf_is_right;
			}
		}

		public float base_gravity
		{
			get
			{
				return this.En.base_gravity;
			}
		}

		public float target_x
		{
			get
			{
				return this.En.getAI().target_x;
			}
		}

		public float target_y
		{
			get
			{
				return this.En.getAI().target_y;
			}
		}

		public NAI Nai
		{
			get
			{
				return this.En.getAI();
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.En.Mp;
			}
		}

		public NelM2DBase nM2D
		{
			get
			{
				return this.En.nM2D;
			}
		}

		public EnemySummoner Summoner
		{
			get
			{
				return this.En.Summoner;
			}
		}

		public float TS
		{
			get
			{
				return this.En.TS;
			}
		}

		public bool hasFoot()
		{
			return this.En.hasFoot();
		}

		protected readonly NelEnemy En;

		protected readonly M2Phys Phy;

		protected readonly M2FootManager FootD;

		protected readonly EnemyAnimator Anm;
	}
}
