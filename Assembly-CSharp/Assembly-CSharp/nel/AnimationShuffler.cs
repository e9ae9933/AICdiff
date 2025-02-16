using System;
using m2d;
using XX;

namespace nel
{
	public class AnimationShuffler
	{
		public AnimationShuffler(PR _Mv)
		{
			this.Mv = _Mv;
		}

		public virtual void initS()
		{
			if (this.pose_is_stand_t > 0f)
			{
				this.pose_is_stand_t = 1f;
			}
		}

		public virtual void stateChanged()
		{
		}

		public void clearStandPoseTime()
		{
			this.pose_is_stand_t = X.Mn(0f, this.pose_is_stand_t);
		}

		public virtual void runPost(float f)
		{
			if (this.pose_is_stand_t > 0f)
			{
				float num = (this.pose_is_stand_t = (Map2d.can_handle ? (this.pose_is_stand_t + f) : X.Mn(this.pose_is_stand_t, 3000f)));
				this.setPose(this.getWaitingPoseTitle(ref num), -1);
				this.pose_is_stand_t = num;
			}
		}

		public virtual string initSetPoseA(string title, int restart_anim, bool loop_jumping)
		{
			this.pose_is_stand_t = -1f;
			return title;
		}

		public virtual string initSetPoseB(string title, string pre_pose, bool loop_jumping)
		{
			return title;
		}

		public virtual void fineSerState()
		{
		}

		public virtual string setDefaultPose(string dep_pose, ref string fix_pose, ref int break_pose_fix_on_walk_level)
		{
			if (dep_pose != "")
			{
				if (fix_pose != null && (!(dep_pose == "stand") || break_pose_fix_on_walk_level >= 3 || this.pose_is_stand) && break_pose_fix_on_walk_level > 0)
				{
					fix_pose = null;
					break_pose_fix_on_walk_level = 0;
				}
				this.Mv.SpSetPose(dep_pose, (this.Mp.floort == 0f) ? 1 : (-1), null, false);
			}
			return dep_pose;
		}

		public string getWaitingPoseTitle(string pose0)
		{
			return this.getWaitingPoseTitle(pose0, ref this.pose_is_stand_t);
		}

		public virtual string getWaitingPoseTitle(ref float pose_is_stand_t)
		{
			return this.getWaitingPoseTitle("stand", ref pose_is_stand_t);
		}

		public virtual string getWaitingPoseTitle(string pose0, ref float pose_is_stand_t)
		{
			return pose0;
		}

		public virtual string absorb_default_pose(bool start_next_down)
		{
			return "damage";
		}

		public virtual string dmg_nokezori_pose(bool is_front, bool medium_damage)
		{
			return "damage";
		}

		public virtual string dmg_normal(AttackInfo Atk, int val, M2PrADmg.DMGRESULT res, float burst_vx, ref string fade_key, ref string fade_key0)
		{
			return "damage";
		}

		public bool pose_is_stand
		{
			get
			{
				return this.pose_is_stand_t != 0f;
			}
		}

		public virtual void setPose(string s, int restart_anim = -1)
		{
			this.Mv.SpSetPose(s, restart_anim, null, false);
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public const string damage_default = "damage";

		protected float pose_is_stand_t;

		public const string stand_pose = "stand";

		public const string crouch_pose = "crouch";

		private M2Mover Mv;

		public const int stand_ev_t = 3000;
	}
}
