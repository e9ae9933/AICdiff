using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2NoelDropCloth : M2Mover
	{
		public virtual void initObject(NoelAnimator _PrAnm, PxlImage _BaseImg, float mover_scale)
		{
			this.BaseImg = _BaseImg;
			this.PrAnm = _PrAnm;
			if (this.Md == null)
			{
				this.Md = new MeshDrawer(null, 4, 6);
				this.Md.draw_gl_only = true;
				this.Md.setMaterial(MTRX.newMtr(MTRX.ShaderGDT), false);
			}
			this.Md.clear(false, false);
			this.Md.getMaterial().SetTexture("_MainTex", _BaseImg.get_I());
			this.Md.initForImg(_BaseImg, 0).DrawCen(0f, 0f, null);
			this.Md.updateForMeshRenderer(false);
		}

		public override void appear(Map2d _Mp)
		{
			base.gameObject.layer = IN.LAY("EnemySelf");
			_Mp.setTag(base.gameObject, "PhysicObject");
			if (this.Phy == null)
			{
				this.Phy = new M2Phys(this, IN.GetOrAdd<Rigidbody2D>(base.gameObject));
			}
			base.appear(_Mp);
			this.Phy.mass = 0.125f;
			this.Phy.freezeRotation = false;
			this.Phy.sharedMaterial = MTRX.PmdM2Bouncy;
			this.Phy.unity_physics_mode = true;
			this.Phy.always_rewrite_velocity = true;
			this.appear_t = -1f;
			if (this.RTkt == null)
			{
				this.RTkt = this.Mp.MovRenderer.assignDrawable(M2Mover.DRAW_ORDER.PR0, base.transform, new M2RenderTicket.FnPrepareMd(this.RenderPrepareMesh), this.Md, this, null);
				this.ticket_assigned_ = true;
				return;
			}
			this.ticket_assigned = true;
		}

		public bool ticket_assigned
		{
			get
			{
				return this.ticket_assigned_;
			}
			set
			{
				if (this.ticket_assigned == value)
				{
					return;
				}
				this.ticket_assigned_ = value;
				if (value)
				{
					if (this.RTkt != null)
					{
						this.Mp.MovRenderer.assignDrawable(this.RTkt);
						return;
					}
				}
				else if (this.RTkt != null)
				{
					this.Mp.MovRenderer.deassignDrawable(this.RTkt, -1);
				}
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (this.appear_t < 0f)
			{
				PRNoel prNoel = this.NM2D.getPrNoel();
				if (prNoel == null || prNoel.Mp == null)
				{
					return;
				}
				float num = X.XORSPS() * 1.5707964f * 0.8f + 1.5707964f;
				float num2 = X.NIXP(3f, 5f);
				this.setTo(prNoel.x + num2 * X.Cos(num), prNoel.y - num2 * 0.5f * X.Sin(num));
				this.appear_t = 0f;
			}
			this.appear_t += this.TS;
		}

		public void appearAt(float depx, float depy, float vx, float vy, float rotR, float rotspdR = 0f)
		{
			this.Phy.Pause();
			this.setTo(depx, depy);
			this.Phy.Resume();
			this.appearAfterVelocity(vx, vy, rotR, rotspdR);
		}

		public void appearAfterVelocity(float vx, float vy, float rotR, float rotspdR = 0f)
		{
			base.killSpeedForce(true, true, false);
			this.appear_t = 0f;
			base.transform.localEulerAngles = new Vector3(0f, 0f, rotR / 3.1415927f * 180f);
			this.Phy.clearLock().addLockMoverHitting(HITLOCK.APPEARING, 20f);
			this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, vx, vy, -1f, -1, 1, 0, -1, 0);
			this.Phy.angleSpeedR = X.absMn(rotspdR, 0.012566372f);
		}

		protected bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (draw_id == 0)
			{
				MdOut = this.Md;
				return true;
			}
			MdOut = null;
			return false;
		}

		public float angleSpeedR
		{
			get
			{
				if (this.Phy == null)
				{
					return 0f;
				}
				return this.Phy.angleSpeedR;
			}
		}

		public override void destruct()
		{
			if (base.destructed || this.Mp == null)
			{
				return;
			}
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			this.deactivateTicket();
			this.RTkt = null;
			base.destruct();
		}

		public override void deactivateFromMap()
		{
			this.deactivateTicket();
			base.deactivateFromMap();
		}

		protected void deactivateTicket()
		{
			this.ticket_assigned = false;
		}

		public NelM2DBase NM2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
		}

		protected NoelAnimator PrAnm;

		protected PxlImage BaseImg;

		private M2RenderTicket RTkt;

		private byte broken;

		private bool ticket_assigned_;

		private MeshDrawer Md;

		public float appear_t;
	}
}
