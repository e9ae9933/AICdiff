using System;
using XX;

namespace m2d
{
	public abstract class M2MovePat
	{
		public M2MovePat(M2EventItem _Mv, M2EventItem.MOV_PAT _type)
		{
			this.Mv = _Mv;
			this.Mp = this.Mv.Mp;
			this.type = _type;
		}

		public static void createMovePattern(StringHolder rER, M2EventItem Mv, ref M2MovePat MPat)
		{
			M2MovePat.createMovePattern(rER, rER._1, Mv, ref MPat);
		}

		public static void createMovePattern(StringHolder rER, string _key, M2EventItem Mv, ref M2MovePat MPat)
		{
			_key = _key.ToUpper();
			M2EventItem.MOV_PAT mov_PAT;
			if (!FEnum<M2EventItem.MOV_PAT>.TryParse(_key, out mov_PAT, true))
			{
				X.de("M2EventItem::setMovePattern - 不明な移動パターン指定: " + _key, null);
				return;
			}
			switch (mov_PAT)
			{
			case M2EventItem.MOV_PAT.SEE_AROUND:
				if (!(MPat is M2MovePatSeeAround))
				{
					Mv.destructMovePat();
					MPat = new M2MovePatSeeAround(Mv);
				}
				else
				{
					MPat.evQuit();
				}
				break;
			case M2EventItem.MOV_PAT.WALKAROUND_LR:
				if (!(MPat is M2MovePatWalkAround))
				{
					Mv.destructMovePat();
					MPat = new M2MovePatWalkAround(Mv, M2EventItem.MOV_PAT.WALKAROUND_LR);
				}
				else
				{
					MPat.evQuit();
				}
				break;
			case M2EventItem.MOV_PAT.WALKAROUND_MAP:
				if (!(MPat is M2MovePatWalkMapAround))
				{
					Mv.destructMovePat();
					MPat = new M2MovePatWalkMapAround(Mv);
				}
				else
				{
					MPat.evQuit();
				}
				break;
			case M2EventItem.MOV_PAT.CRAWLAROUND_LR:
				if (!(MPat is M2MovePatCrawlAround))
				{
					Mv.destructMovePat();
					MPat = new M2MovePatCrawlAround(Mv);
				}
				else
				{
					MPat.evQuit();
				}
				break;
			default:
				if (MPat != null)
				{
					Mv.destructMovePat();
				}
				return;
			}
			if (rER != null && rER.clength >= 2)
			{
				Mv.pose_prefix = (TX.valid(rER._2) ? rER._2 : null);
				if (rER.clength >= 3)
				{
					MPat.readMovePat(rER);
				}
			}
		}

		public virtual bool run(float fcnt)
		{
			return false;
		}

		public virtual void evStart(bool start_my_event)
		{
			if (!start_my_event)
			{
				this.Mv.no_foot_sound = true;
			}
		}

		public virtual void assignMoveScript(bool soft_touch)
		{
			if (!soft_touch)
			{
				this.Mv.no_foot_sound = false;
			}
		}

		public virtual void evQuit()
		{
			this.Mv.no_foot_sound = false;
		}

		public virtual void destruct()
		{
		}

		public virtual void readMovePat(StringHolder rER)
		{
		}

		public virtual void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (TX.valid(nPose))
			{
				this.Mv.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
			}
		}

		public M2MovePat setAim(AIM n, bool sprite_force_aim_set = false)
		{
			this.Mv.setAim(n, sprite_force_aim_set);
			return this;
		}

		public float vx
		{
			get
			{
				return this.Mv.vx;
			}
		}

		public float vy
		{
			get
			{
				return this.Mv.vy;
			}
		}

		public M2Phys Phy
		{
			get
			{
				return this.Mv.getPhysic();
			}
		}

		public void killSpeedForce(bool x_flag = true, bool y_flag = true, bool kill_phy_translate_stack = false)
		{
			this.Mv.killSpeedForce(x_flag, y_flag, kill_phy_translate_stack);
		}

		public void setVelocityForce(float _vx, float _vy)
		{
			this.Mv.setVelocityForce(_vx, _vy);
		}

		public virtual IFootable canFootOn(IFootable F)
		{
			return F;
		}

		public readonly M2EventItem Mv;

		public readonly Map2d Mp;

		public readonly M2EventItem.MOV_PAT type;
	}
}
