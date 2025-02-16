using System;
using m2d;
using XX;

namespace nel
{
	public class M2PrAssistant
	{
		public M2PrAssistant(PR _Pr)
		{
			this.Pr = _Pr;
			this.NM2D = M2DBase.Instance as NelM2DBase;
			this.Phy = this.Pr.getPhysic();
			if (this.Phy != null)
			{
				this.FootD = this.Phy.getFootManager();
			}
		}

		public virtual void initS()
		{
			this.Phy = this.Pr.getPhysic();
			if (this.Phy != null)
			{
				this.FootD = this.Phy.getFootManager();
			}
		}

		public override string ToString()
		{
			return "<Assistant>-" + this.Pr.ToString();
		}

		public virtual void newGame()
		{
		}

		public float TS
		{
			get
			{
				return this.Pr.TS;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Pr.Mp;
			}
		}

		public AIM aim
		{
			get
			{
				return this.Pr.aim;
			}
		}

		public float x
		{
			get
			{
				return this.Pr.x;
			}
		}

		public float y
		{
			get
			{
				return this.Pr.y;
			}
		}

		public float vx
		{
			get
			{
				return this.Pr.vx;
			}
		}

		public float vy
		{
			get
			{
				return this.Pr.vy;
			}
		}

		public float maxhp
		{
			get
			{
				return this.Pr.get_maxhp();
			}
		}

		public float maxmp
		{
			get
			{
				return this.Pr.get_maxmp();
			}
		}

		public float hp
		{
			get
			{
				return this.Pr.get_hp();
			}
		}

		public float mp
		{
			get
			{
				return this.Pr.get_mp();
			}
		}

		public float drawx
		{
			get
			{
				return this.Pr.drawx;
			}
		}

		public float drawy
		{
			get
			{
				return this.Pr.drawy;
			}
		}

		public float getCastableMp()
		{
			return this.Pr.getCastableMp();
		}

		public float mtop
		{
			get
			{
				return this.Pr.mtop;
			}
		}

		public float mbottom
		{
			get
			{
				return this.Pr.mbottom;
			}
		}

		public int magic_returnable_mp
		{
			get
			{
				return this.Pr.magic_returnable_mp;
			}
		}

		public PR.STATE state
		{
			get
			{
				return this.Pr.get_current_state();
			}
		}

		public M2MoverPr.DECL decline
		{
			get
			{
				return this.Pr.decline;
			}
			set
			{
				this.Pr.decline = value;
			}
		}

		public bool isMagicExistState()
		{
			return this.Pr.isMagicExistState(this.state);
		}

		public bool isSinkState()
		{
			return this.Pr.isSinkState(this.state);
		}

		public bool isEvadeState()
		{
			return this.Pr.isEvadeState(this.state);
		}

		public bool isSlidingState()
		{
			return this.Pr.isSlidingState(this.state);
		}

		public bool isNormalState()
		{
			return this.Pr.isNormalState();
		}

		public bool isPunchState()
		{
			return this.Pr.isPunchState();
		}

		protected bool isWormTrapped()
		{
			return this.Pr.isWormTrapped();
		}

		public bool isDamagingOrKo()
		{
			return this.Pr.isDamagingOrKo();
		}

		public bool isNormalBusy()
		{
			return this.Pr.isPunchState() || this.Pr.isEvadeState() || this.Pr.isMagicState();
		}

		public bool hasFoot()
		{
			return this.Pr.hasFoot();
		}

		protected bool canJump()
		{
			return this.Pr.canJump();
		}

		public M2Ser Ser
		{
			get
			{
				return this.Pr.Ser;
			}
		}

		protected bool isEvadePD(int alloc_frame = 1)
		{
			return this.Pr.isEvadePD(alloc_frame);
		}

		protected bool isEvadeU()
		{
			return this.Pr.isEvadeU();
		}

		protected bool isEvadeO()
		{
			return this.Pr.isEvadeO(0);
		}

		protected bool isMagicPD(int alloc_frame = 1)
		{
			return this.Pr.isMagicPD(alloc_frame);
		}

		protected bool isMagicO()
		{
			return this.Pr.isMagicO(0);
		}

		protected bool isAtkPD(int alloc_frame = 1)
		{
			return this.Pr.isAtkPD(alloc_frame);
		}

		protected bool isAtkO()
		{
			return this.Pr.isAtkO(0);
		}

		protected bool isLP(int press_max = 1)
		{
			return this.Pr.isLP(press_max);
		}

		protected bool isLU()
		{
			return this.Pr.isLU();
		}

		protected bool isLO()
		{
			return this.Pr.isLO(0);
		}

		protected bool isRP(int press_max = 1)
		{
			return this.Pr.isRP(press_max);
		}

		protected bool isRU()
		{
			return this.Pr.isRU();
		}

		protected bool isRO()
		{
			return this.Pr.isRO(0);
		}

		protected bool isBP()
		{
			return this.Pr.isBP();
		}

		protected bool isBO()
		{
			return this.Pr.isBO(0);
		}

		protected bool isTP()
		{
			return this.Pr.isTP(1);
		}

		protected bool isTO()
		{
			return this.Pr.isTO(0);
		}

		public bool hasD(M2MoverPr.DECL d)
		{
			return this.Pr.hasD(d);
		}

		public bool hasD_stopact()
		{
			return this.Pr.hasD_stopact();
		}

		public bool hasD_stopevade()
		{
			return this.Pr.hasD_stopevade();
		}

		public bool hasD_stopmag()
		{
			return this.Pr.hasD_stopmag();
		}

		public void addD(M2MoverPr.DECL d)
		{
			this.Pr.decline |= d;
		}

		public void remD(M2MoverPr.DECL d)
		{
			this.Pr.decline &= ~d;
		}

		protected bool isPuzzleManagingMp()
		{
			return this.Pr.isPuzzleManagingMp();
		}

		protected bool isFacingEnemy()
		{
			return this.Pr.isFacingEnemy();
		}

		protected bool isOnBench(bool only_state_check = false)
		{
			return this.Pr.isOnBench(only_state_check);
		}

		protected bool is_crouch
		{
			get
			{
				return this.Pr.is_crouch;
			}
		}

		protected bool is_alive
		{
			get
			{
				return this.Pr.is_alive;
			}
		}

		public void defineParticlePreVariable()
		{
			this.Pr.defineParticlePreVariable();
		}

		public bool getEH(EnhancerManager.EH ehbit)
		{
			return this.Pr.getEH(ehbit);
		}

		public bool poseIs(string pose)
		{
			return this.Pr.poseIs(pose);
		}

		protected M2PrMistApplier MistApply
		{
			get
			{
				return this.Pr.getMistApplier();
			}
		}

		public void setAim(AIM a, bool sprite_force_aim_set = false)
		{
			this.Pr.setAim(a, sprite_force_aim_set);
		}

		protected void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			this.Pr.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
		}

		protected MpGaugeBreaker GaugeBrk
		{
			get
			{
				return this.Pr.GaugeBrk;
			}
		}

		protected TransEffecter TeCon
		{
			get
			{
				return this.Pr.TeCon;
			}
		}

		protected PrEggManager EggCon
		{
			get
			{
				return this.Pr.EggCon;
			}
		}

		protected AbsorbManagerContainer AbsorbCon
		{
			get
			{
				return this.Pr.getAbsorbContainer();
			}
		}

		public M2PrAssistant PtcVar(string key, float v)
		{
			this.Pr.PtcVar(key, (double)v);
			return this;
		}

		public M2PrAssistant PtcVarS(string key, string v)
		{
			this.Pr.PtcVarS(key, v);
			return this;
		}

		public PTCThread PtcST(string ptc_key, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.Pr.PtcST(ptc_key, hold, follow);
		}

		public PTCThread PtcSTTimeFixed(string ptc_key, float factor, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.Pr.PtcHld.PtcSTTimeFixed(ptc_key, factor, hold, follow, false);
		}

		public readonly PR Pr;

		public M2Phys Phy;

		public M2FootManager FootD;

		public readonly NelM2DBase NM2D;
	}
}
