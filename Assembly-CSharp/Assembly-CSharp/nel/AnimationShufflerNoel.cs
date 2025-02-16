using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class AnimationShufflerNoel : AnimationShuffler
	{
		public AnimationShufflerNoel(PRNoel _Pr)
			: base(_Pr)
		{
			this.Pr = _Pr;
		}

		public override void initS()
		{
			base.initS();
			this.TeFallShift = null;
		}

		public override void stateChanged()
		{
			if (this.TeFallShift != null)
			{
				this.TeFallShift.destruct();
				this.TeFallShift = null;
			}
		}

		public override string initSetPoseA(string title, int restart_anim, bool loop_jumping)
		{
			NoelAnimator animator = this.Pr.getAnimator();
			if (title == "stand")
			{
				if (this.Pr.isBreatheStop(false, false))
				{
					title = (this.Pr.isBreatheStop(true, false) ? "dmg_breathe" : "stand_hardbreathe");
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.applying_wind)
				{
					title = "stand_wind";
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.BetoMng.is_torned && !X.SENSITIVE)
				{
					title = "stand";
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.isWetPose() || this.Pr.isWeakPose())
				{
					title = "stand_wet_weak";
					this.pose_is_stand_t = -1f;
				}
				else
				{
					if (this.pose_is_stand_t > 0f && restart_anim > -2 && restart_anim != 1)
					{
						return null;
					}
					this.pose_is_stand_t = 1f;
					string title2 = animator.getCurrentPose().title;
					if (!loop_jumping && (this.Pr.isPoseCrouch(false) || title2 == "crouch2stand"))
					{
						title = "crouch2stand";
					}
					else
					{
						title = base.getWaitingPoseTitle("stand");
					}
				}
			}
			else if (title == "ladder")
			{
				if (this.Pr.BetoMng.is_torned)
				{
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.applying_wind)
				{
					this.pose_is_stand_t = -1f;
					title = "ladder_wind";
				}
				else
				{
					if (this.pose_is_stand_t > 0f && restart_anim > -2 && restart_anim != 1 && !loop_jumping)
					{
						return null;
					}
					this.pose_is_stand_t = 1f;
					title = base.getWaitingPoseTitle("ladder");
				}
			}
			else
			{
				this.pose_is_stand_t = 0f;
			}
			return title;
		}

		public override string initSetPoseB(string title, string pre_pose, bool loop_jumping)
		{
			NoelAnimator animator = this.Pr.getAnimator();
			M2PxlAnimator animator2 = animator.getAnimator();
			if (title != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(title);
				if (num <= 1325572695U)
				{
					if (num <= 508974720U)
					{
						if (num != 231209156U)
						{
							if (num != 508974720U)
							{
								return title;
							}
							if (!(title == "crawl"))
							{
								return title;
							}
							if (this.Pr.isBreatheStop(false, false))
							{
								return "crawl_hardbreathe";
							}
							return (this.isWetPose() || this.isWeakPose()) ? "crawl_wet_weak" : "crawl";
						}
						else if (!(title == "magic_hold"))
						{
							return title;
						}
					}
					else if (num != 718098122U)
					{
						if (num != 1313842571U)
						{
							if (num != 1325572695U)
							{
								return title;
							}
							if (!(title == "stand2bench"))
							{
								return title;
							}
							if (this.Pr.Ser.isShamed() || this.Pr.Ser.has(SER.MP_REDUCE))
							{
								return "bench_shamed";
							}
							return title;
						}
						else
						{
							if (!(title == "crouch"))
							{
								return title;
							}
							string title2 = animator.getCurrentPose().title;
							bool flag = TX.isStart(title2, "crawl", 0);
							title = (flag ? "crawl2crouch" : "stand2crouch");
							if (loop_jumping || (!(title2 == title) && !(title2 == "crouch2stand") && !flag && this.Pr.isPoseCrouch(false)))
							{
								title = "crouch";
							}
							if (this.Pr.isBreatheStop(false, false))
							{
								if (title == "stand2crouch")
								{
									title = "stand2crouch_hardbreathe";
								}
								if (title == "crouch")
								{
									return "crouch_hardbreathe";
								}
								return title;
							}
							else
							{
								if (title == "crouch" && this.Pr.applying_wind)
								{
									return "crouch_wind";
								}
								return title;
							}
						}
					}
					else
					{
						if (!(title == "run"))
						{
							return title;
						}
						if (this.Pr.isBreatheStop(false, false))
						{
							return "run_hardbreathe";
						}
						if (this.Pr.applying_wind)
						{
							return "run_wind";
						}
						return (this.isWetPose() || this.isWeakPose()) ? "run_wet_weak" : "run";
					}
				}
				else if (num <= 2502056623U)
				{
					if (num != 2248295223U)
					{
						if (num != 2502056623U)
						{
							return title;
						}
						if (!(title == "stand_ev"))
						{
							return title;
						}
						this.pose_is_stand_t = 3000f;
						return title;
					}
					else if (!(title == "magic_init"))
					{
						return title;
					}
				}
				else if (num != 2686853648U)
				{
					if (num != 2805947405U)
					{
						if (num != 4166649696U)
						{
							return title;
						}
						if (!(title == "fall"))
						{
							return title;
						}
						if (!loop_jumping && animator2.getCurrentPose().end_jump_title == "fall2" && !TX.isStart(pre_pose, "fall", 0))
						{
							return null;
						}
						if (this.Pr.isBreatheStop(false, false))
						{
							title = "fall_hardbreathe";
							if (pre_pose == "fall_hardbreathe2")
							{
								return null;
							}
							return title;
						}
						else
						{
							if (pre_pose == "fall2")
							{
								return null;
							}
							return title;
						}
					}
					else
					{
						if (!(title == "jump"))
						{
							return title;
						}
						if (!loop_jumping && animator2.getCurrentPose().end_jump_title == "fall2")
						{
							return null;
						}
						return title;
					}
				}
				else
				{
					if (!(title == "walk"))
					{
						return title;
					}
					if (this.Pr.isBreatheStop(false, false))
					{
						return this.Pr.isBreatheStop(true, false) ? "walk_dmg_breathe" : "walk_hardbreathe";
					}
					if (this.Pr.applying_wind)
					{
						return "walk_wind";
					}
					return this.isWetPose() ? "walk_wet_weak" : (this.isWeakPose() ? "walk_weak" : "walk");
				}
				title = this.getSpecialMagicPose(this.Pr.getCurMagic(), title == "magic_init");
			}
			return title;
		}

		public string getSpecialMagicPose(MagicItem Mg, bool is_init)
		{
			if (Mg != null)
			{
				MGKIND kind = Mg.kind;
				if (kind != MGKIND.FIREBALL)
				{
					if (kind == MGKIND.DROPBOMB)
					{
						if (!is_init)
						{
							return "magic_bomb_hold";
						}
						return "magic_bomb_init";
					}
				}
				else if (is_init)
				{
					return "magic_fireball_init";
				}
			}
			if (!is_init)
			{
				return "magic_hold";
			}
			return "magic_init";
		}

		public override void fineSerState()
		{
			string manualSettedPoseTitle = this.Pr.getAnimator().getManualSettedPoseTitle();
			if (this.pose_is_stand_t != 0f)
			{
				float pose_is_stand_t = this.pose_is_stand_t;
				this.setPose(manualSettedPoseTitle, -2);
				if (this.pose_is_stand_t > 0f && pose_is_stand_t > 0f)
				{
					this.pose_is_stand_t = X.Mx(this.pose_is_stand_t, pose_is_stand_t);
					return;
				}
			}
			else if (manualSettedPoseTitle != null && (manualSettedPoseTitle == "stand_ev" || manualSettedPoseTitle == "run" || manualSettedPoseTitle == "walk" || manualSettedPoseTitle == "crawl" || manualSettedPoseTitle == "crouch"))
			{
				this.setPose(manualSettedPoseTitle, -2);
			}
		}

		public override string setDefaultPose(string dep_pose, ref string fix_pose, ref int break_pose_fix_on_walk_level)
		{
			if (dep_pose == null)
			{
				M2FootManager footManager = this.Pr.getFootManager();
				dep_pose = "";
				NoelAnimator animator = this.Pr.getAnimator();
				M2Phys physic = this.Pr.getPhysic();
				M2PrMistApplier mistApplier = this.Pr.getMistApplier();
				if (mistApplier != null && mistApplier.isFloatingWaterChoke())
				{
					bool flag = this.Pr.forceCrouch(false, false);
					dep_pose = (this.Pr.view_crouching ? "water_choked_down" : "water_choked");
					physic.walk_xspeed *= 0.5f;
					this.Pr.jumpRaisingQuit(false);
					if (!flag)
					{
						this.Pr.is_crouch = false;
					}
				}
				else if (!((dep_pose = this.Pr.Skill.getPoseTitleOnNormal()) != ""))
				{
					if (footManager.FootIsLadder())
					{
						if (!animator.nextPoseIs("ladder"))
						{
							dep_pose = "ladder";
							if (animator.strictPoseIs("ladder"))
							{
								animator.timescale = (float)((animator.cframe % 2 == 1) ? 0 : 1);
							}
						}
						if (animator.strictPoseIs("ladder_wait"))
						{
							animator.timescale = 1f;
						}
					}
					else if (!this.Pr.canJump() && (animator.poseIs(POSE_TYPE.JUMP | POSE_TYPE.FALL, false) || this.Pr.jump_occuring || physic.gravity_added_velocity > 0.004f))
					{
						dep_pose = ((this.Pr.vy < -0.01f && (animator.poseIs(POSE_TYPE.JUMP, false) || this.Pr.jump_raising)) ? "jump" : "fall");
					}
					else
					{
						float walk_xspeed = physic.walk_xspeed;
						if (this.Pr.isRunStopping())
						{
							if (animator.poseIs("run_stop"))
							{
								dep_pose = "run_stop";
							}
						}
						else if (this.Pr.isRunning())
						{
							dep_pose = "run";
						}
						bool flag2 = this.Pr.view_crouching || this.Pr.forceCrouch(false, false);
						if (dep_pose == "")
						{
							PxlPose currentPose = animator.getCurrentPose();
							dep_pose = ((Mathf.Abs(walk_xspeed) > 0f) ? (this.Pr.isRunning() ? "run" : (flag2 ? "crawl" : "walk")) : (flag2 ? "crouch" : ((currentPose.end_jump_title != "stand") ? "stand" : "")));
						}
					}
				}
			}
			if (dep_pose == "fall" && this.Pr.isPoseCrouch(false))
			{
				this.TeFallShift = this.Pr.TeCon.setAppearFrom(AIM.B, 29f, 28f);
			}
			return base.setDefaultPose(dep_pose, ref fix_pose, ref break_pose_fix_on_walk_level);
		}

		public override string getWaitingPoseTitle(ref float pose_is_stand_t)
		{
			NoelAnimator animator = this.Pr.getAnimator();
			return this.getWaitingPoseTitle(animator.getManualSettedPoseTitle(), ref pose_is_stand_t);
		}

		public override string getWaitingPoseTitle(string pose0, ref float pose_is_stand_t)
		{
			NoelAnimator animator = this.Pr.getAnimator();
			M2PxlAnimator animator2 = animator.getAnimator();
			if (pose_is_stand_t < 0f)
			{
				return animator2.getCurrentPose().title;
			}
			if (pose0 == "ladder" || pose0 == "ladder_wait")
			{
				if (pose_is_stand_t >= 1400f)
				{
					pose_is_stand_t = (this.pose_is_stand_t = 1f);
				}
				if (pose_is_stand_t >= 400f)
				{
					return animator2.pose_title;
				}
				if (pose_is_stand_t >= 360f)
				{
					return "ladder_wait";
				}
				return "ladder";
			}
			else
			{
				if (pose_is_stand_t < 180f)
				{
					return "stand";
				}
				int pose_aim = animator.pose_aim;
				if (360f <= pose_is_stand_t && pose_is_stand_t < 2000f && animator2.looped_in_preveious_frame)
				{
					pose_is_stand_t = (this.pose_is_stand_t = 2000f);
					return "stand_slow";
				}
				if (2000f > pose_is_stand_t)
				{
					return "stand";
				}
				if (pose_is_stand_t < 2220f)
				{
					return "stand_slow";
				}
				if (pose_is_stand_t < 2500f)
				{
					pose_is_stand_t = (this.pose_is_stand_t = 3000f);
				}
				if (pose_is_stand_t < 3520f)
				{
					return "stand_ev";
				}
				if (this.Pr.BetoMng.is_torned || pose_aim == 7 || pose_aim == 6 || animator.outfit_type != PRNoel.OUTFIT.NORMAL)
				{
					pose_is_stand_t = (this.pose_is_stand_t = 3000f);
					return "stand_ev";
				}
				if (pose_is_stand_t < 3610f)
				{
					return "stand_wait_normal_0";
				}
				if (pose_is_stand_t < 3660f)
				{
					return "stand_wait_normal_1";
				}
				pose_is_stand_t = (this.pose_is_stand_t = 2600f);
				return "stand_ev";
			}
		}

		public override string absorb_default_pose(bool start_next_down)
		{
			if (this.Pr.isDownState() || this.Pr.isPoseDown(false))
			{
				if (!this.Pr.isPoseBackDown(false))
				{
					return "absorbed_down";
				}
				return "absorbed_downb";
			}
			else if (this.Pr.is_crouch || this.Pr.isPoseCrouch(false))
			{
				if (!start_next_down)
				{
					return "absorbed_crouch";
				}
				if (X.XORSP() >= 0.5f)
				{
					return "absorbed_crouch2absorbed_downb";
				}
				return "absorbed_crouch2absorbed_down";
			}
			else
			{
				if (!start_next_down)
				{
					return "absorbed";
				}
				return "absorbed2absorbed_crouch";
			}
		}

		public override string dmg_nokezori_pose(bool is_front, bool medium_damage)
		{
			bool flag = this.Pr.is_crouch;
			if (this.Pr.isPoseDown(false))
			{
				if (this.Pr.isWebTrappedState(true) || X.XORSP() < (this.Pr.EpCon.isOrgasm() ? 0.97f : 0.85f))
				{
					return (this.Pr.isPoseBack(false) == X.XORSP() < 0.85f) ? "downdamage" : "downdamage_t";
				}
				flag = true;
			}
			if (!flag && X.XORSP() < 0.85f && this.Pr.isPoseCrouch(false))
			{
				flag = true;
			}
			string text;
			if (is_front)
			{
				if (flag)
				{
					text = "dmg_crouch";
				}
				else
				{
					text = (medium_damage ? "damage_m" : "dmg_s");
				}
			}
			else
			{
				text = (flag ? "dmg_crouch_b" : "dmg_s_b");
			}
			return text;
		}

		public override string dmg_normal(AttackInfo Atk, int val, M2PrADmg.DMGRESULT res, float burst_vx, ref string fade_key, ref string fade_key0)
		{
			string text = "";
			NoelAnimator animator = this.Pr.getAnimator();
			AbsorbManagerContainer absorbContainer = this.Pr.getAbsorbContainer();
			if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.L)
			{
				if ((res & M2PrADmg.DMGRESULT._ABSORBING) == M2PrADmg.DMGRESULT.MISS)
				{
					if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) == M2PrADmg.DMGRESULT.MISS)
					{
						if ((res & M2PrADmg.DMGRESULT._DMGT) != M2PrADmg.DMGRESULT.MISS)
						{
							text = "dmg_t";
						}
						else if ((res & M2PrADmg.DMGRESULT._DMG_KIRIMOMI) == M2PrADmg.DMGRESULT.MISS)
						{
							if (burst_vx == 0f || burst_vx < 0f == this.Pr.is_right)
							{
								text = "dmg_hktb";
							}
							else
							{
								text = "dmg_hktb_b";
							}
						}
					}
				}
				else if (animator.poseIs("downdamage_t", "down"))
				{
					text = "downdamage_t";
				}
				else if (absorbContainer.current_pose_priority == 0)
				{
					text = this.absorb_default_pose(X.XORSP() < (0.25f + ((!this.Pr.is_alive) ? 0.25f : 0f)) * 2.8f);
					if (text == "absorb2absorb_crouch" && TX.noe(fade_key))
					{
						fade_key = "crouch";
					}
				}
				if (Atk.attr == MGATTR.THUNDER)
				{
					if (text == "dmg_hktb" || text == "dmg_hktb_b" || text == "downdamage_t" || text == "dmg_t")
					{
						text = "damage_thunder";
					}
					string text2;
					fade_key0 = (text2 = "damage_thunder_big");
					fade_key = text2;
				}
			}
			else if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.S)
			{
				if ((res & M2PrADmg.DMGRESULT._ABSORBING) == M2PrADmg.DMGRESULT.MISS)
				{
					if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) == M2PrADmg.DMGRESULT.MISS)
					{
						text = this.dmg_nokezori_pose(burst_vx == 0f || burst_vx < 0f == this.Pr.is_right == X.XORSP() < 0.78f, (double)X.XORSP() < (double)X.Mn(1f, (float)val / X.Mx(1f, this.Pr.get_hp())) * 0.6);
					}
				}
				else if (animator.poseIs("downdamage_t", "down"))
				{
					text = "downdamage_t";
				}
				else if (absorbContainer.current_pose_priority == 0)
				{
					text = this.absorb_default_pose(X.XORSP() < (0.11f + ((!this.Pr.is_alive) ? 0.11f : 0f)) * 2.8f);
					if (text == "absorb2absorb_crouch" && TX.noe(fade_key))
					{
						fade_key = "crouch";
					}
				}
			}
			return text;
		}

		public override void setPose(string s, int restart_anim = -1)
		{
			this.Pr.getAnimator().setPose(s, restart_anim, false);
		}

		public bool isWetPose()
		{
			return this.Pr.isWetPose();
		}

		public bool isWeakPose()
		{
			return this.Pr.isWeakPose();
		}

		private PRNoel Pr;

		private TransEffecterItem TeFallShift;

		public const float absorb_pose_change_ratio = 2.8f;
	}
}
