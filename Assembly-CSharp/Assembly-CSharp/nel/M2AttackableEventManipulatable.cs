using System;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class M2AttackableEventManipulatable : M2Attackable, M2MagicCaster
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			Mp.getEventContainer().addClearingRemoveObject(this);
			this.Par = Mp.getMoverByName(this.get_parent_name(), false);
			this.Pr = Mp.Pr as PR;
			if (this.Par != null)
			{
				if (this.Par is M2EventItem && this.replace_tecon)
				{
					this.TeCon = (this.Par as M2EventItem).TeCon;
					(this.Par as M2EventItem).TeCon = null;
				}
				this.setTo(this.Par.x, this.Par.y);
				base.Size(this.Par.sizex * base.CLENM, this.Par.sizey * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.Anm = Mp.getPxlAnimator(this.Par) as M2PxlAnimatorRT;
				base.gameObject.layer = this.Par.gameObject.layer;
				this.ParPhy = this.Par.getPhysic();
				if (this.ParPhy != null && this.CC != null)
				{
					this.ParPhy.addCollirderLock(this.CC.Cld, -1f, null, 1f);
				}
				this.fineAnmFrame();
			}
		}

		public override void runPost()
		{
			if (this.Par != null && this.Par.destructed)
			{
				this.Mp.removeMover(this);
				return;
			}
			base.runPost();
			this.fineAnmFrame();
			if (this.Par != null)
			{
				this.setTo(this.Par.x, this.Par.y);
			}
			this.t += this.TS;
		}

		protected void fineAnmFrame()
		{
			if (this.Anm != null)
			{
				PxlFrame currentDrawnFrame = this.Anm.getCurrentDrawnFrame();
				if (currentDrawnFrame != this.LastDrawn)
				{
					this.LastDrawn = currentDrawnFrame;
					if (this.OCanePos != null && !this.OCanePos.TryGetValue(currentDrawnFrame, out this.LastTargetPos))
					{
						this.OCanePos[currentDrawnFrame] = (this.LastTargetPos = this.calcCanePos(currentDrawnFrame));
					}
				}
			}
		}

		protected virtual Vector2 calcCanePos(PxlFrame F)
		{
			return Vector2.zero;
		}

		public abstract string get_parent_name();

		public override RAYHIT can_hit(M2Ray Ray)
		{
			return RAYHIT.NONE;
		}

		public abstract void EvtRead(StringHolder rER);

		public override HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.NONE;
		}

		public override bool isDamagingOrKo()
		{
			return false;
		}

		public void aimToPr()
		{
			this.aimToMv(this.Pr);
		}

		public void aimToMv(M2Mover Mv)
		{
			if (this.Par.x < Mv.x)
			{
				this.Par.setAim(AIM.R, false);
			}
			if (this.Par.x > Mv.x)
			{
				this.Par.setAim(AIM.L, false);
			}
		}

		public Vector2 getCenter()
		{
			if (!(this.Par != null))
			{
				return new Vector2(base.x, base.y);
			}
			return new Vector2(this.Par.x, this.Par.y);
		}

		public override Vector2 getTargetPos()
		{
			return this.getCenter() + this.LastTargetPos;
		}

		public virtual Vector2 getAimPos(MagicItem Mg)
		{
			return new Vector2(this.Pr.x, this.Pr.y);
		}

		public int getAimDirection()
		{
			return -1;
		}

		public float getPoseAngleRForCaster()
		{
			return 1.5707964f;
		}

		public float getHpDamagePublishRatio(MagicItem Mg)
		{
			return this.publish_hp_damage_ratio;
		}

		public float getCastingTimeScale(MagicItem Mg)
		{
			return this.casting_time_scale;
		}

		public float getCastableMp()
		{
			return (float)this.mp;
		}

		public virtual bool canHoldMagic(MagicItem Mg)
		{
			return !base.destructed;
		}

		public bool isManipulatingMagic(MagicItem Mg)
		{
			return false;
		}

		public bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitTarget)
		{
			return true;
		}

		public void initPublishKill(M2MagicCaster Target)
		{
		}

		public AIM getAimForCaster()
		{
			return this.aim;
		}

		public void setAimForCaster(AIM a)
		{
			this.setAim(a, false);
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (this.Par != null)
			{
				this.Par.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
			}
		}

		public void showMessage(string ev_name, string msg_label, float auto_hide_time = 280f)
		{
			NelMSG nelMSG = (EV.getMessageContainer() as NelMSGContainer).createDrawer(ev_name, msg_label, "#<" + this.Par.key + ">", false, NelMSG.HKDS_BOUNDS.ONELINE, NelMSG.HKDSTYPE._MAX, null, null);
			if (nelMSG != null)
			{
				nelMSG.auto_hide_time = auto_hide_time;
				nelMSG.FollowTo = this.Par;
				nelMSG.use_valotile = base.M2D.use_valotile;
			}
		}

		public static void ReadEvtS(EvReader ER, StringHolder rER, Map2d curMap)
		{
			M2AttackableEventManipulatable m2AttackableEventManipulatable = curMap.getMoverByName(rER._1, true) as M2AttackableEventManipulatable;
			if (m2AttackableEventManipulatable != null && !m2AttackableEventManipulatable.isParentActive())
			{
				curMap.removeMover(m2AttackableEventManipulatable);
			}
			if (m2AttackableEventManipulatable == null)
			{
				string _ = rER._1;
				if (_ != null)
				{
					if (_ == "PrimulaPVV11")
					{
						m2AttackableEventManipulatable = curMap.createMover<PrimulaPVV11>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), true, true);
						goto IL_01AC;
					}
					if (_ == "IxiaPVV102")
					{
						m2AttackableEventManipulatable = curMap.createMover<IxiaPVV102>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), true, true);
						goto IL_01AC;
					}
					if (_ == "IxiaPVV104")
					{
						m2AttackableEventManipulatable = curMap.createMover<IxiaPVV104>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), true, true);
						goto IL_01AC;
					}
					if (_ == "AlmaPVV105")
					{
						m2AttackableEventManipulatable = curMap.createMover<AlmaPVV105>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), true, true);
						goto IL_01AC;
					}
					if (_ == "MvEvtMuseumOwl")
					{
						m2AttackableEventManipulatable = curMap.createMover<MvEvtMuseumOwl>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), true, true);
						goto IL_01AC;
					}
				}
				if (rER._2 != "INIT" || rER.clength < 4)
				{
					X.de("Other ENGINE の始動は INIT <target_mover_key> で行うこと", null);
					return;
				}
				M2CastableEventOther m2CastableEventOther = curMap.createMover<M2CastableEventOther>(rER._1, (float)(curMap.clms / 2), (float)(curMap.rows / 2), false, true);
				m2CastableEventOther.target_mover_key = rER._3;
				curMap.assignMover(m2CastableEventOther, false);
				m2AttackableEventManipulatable = m2CastableEventOther;
				IL_01AC:
				if (m2AttackableEventManipulatable != null)
				{
					m2AttackableEventManipulatable.key = rER._1;
				}
			}
			if (m2AttackableEventManipulatable != null && rER.clength > 2)
			{
				m2AttackableEventManipulatable.EvtRead(rER);
			}
		}

		public NelM2DBase nM2D
		{
			get
			{
				return base.M2D as NelM2DBase;
			}
		}

		public bool isParentActive()
		{
			return this.Par != null && !this.Par.destructed;
		}

		protected bool PhaseP(int _phase, int time_threshold)
		{
			if (this.phase == _phase && this.t >= (float)time_threshold)
			{
				this.t = 0f;
				this.phase++;
				return true;
			}
			return false;
		}

		protected M2Mover Par;

		protected M2PxlAnimatorRT Anm;

		protected M2Phys ParPhy;

		protected PR Pr;

		protected float t;

		protected int phase;

		protected BDic<PxlFrame, Vector2> OCanePos;

		public M2Mover.DRAW_ORDER order;

		protected PxlFrame LastDrawn;

		protected Vector2 LastTargetPos;

		protected float publish_hp_damage_ratio = 1f;

		protected float casting_time_scale = 1f;

		public bool replace_tecon = true;
	}
}
