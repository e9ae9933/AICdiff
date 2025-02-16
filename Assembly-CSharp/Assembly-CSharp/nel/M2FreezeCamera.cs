using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2FreezeCamera : IRunAndDestroy
	{
		public M2FreezeCamera(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.FD_fnFreezeCamKD = new TxKeyDesc.FnGetKD(this.fnFreezeCamKD);
		}

		public void destruct()
		{
			this.deactivate();
		}

		public void activate(PR _Pr)
		{
			this.Pr = _Pr;
			if (!this.is_active)
			{
				IN.FlgUiUse.Add("FREEZE");
				CURS.Active.Add("FREEZE");
				CURS.Set("NORMAL", "tl_cross");
				this.PosMousePre = Vector3.zero;
				PostEffect.setable_camera_scale = false;
				this.M2D.Cam.blurCenterIfFocusing(this.M2D.Cam.getBaseMover());
				this.is_active = true;
				UIPictureBase.FlgStopAutoFade.Add("FREEZE");
				if (this.TkKD == null)
				{
					this.TkKD = this.M2D.TxKD.AddTicket(160, this.FD_fnFreezeCamKD, this);
				}
				this.Pe = PostEffect.IT.setSlowFading(0f, -1f, 0f, 0);
				this.M2D.PE.refineMaterialAlpha();
				IN.addRunner(this);
				this.setLockInput(4f);
			}
		}

		public void deactivate()
		{
			this.deactivate(false);
		}

		private void deactivate(bool no_deaassign_runner)
		{
			if (this.is_active)
			{
				IN.FlgUiUse.Rem("FREEZE");
				CURS.Active.Rem("FREEZE");
				CURS.Rem("NORMAL", "tl_cross");
				this.is_active = false;
				PostEffect.setable_camera_scale = true;
				UIPictureBase.FlgStopAutoFade.Rem("FREEZE");
				if (this.TkKD != null)
				{
					this.TkKD = this.TkKD.destruct();
				}
				if (this.Pe != null)
				{
					this.Pe.destruct();
					this.Pe = null;
				}
				if (!no_deaassign_runner)
				{
					IN.remRunner(this);
				}
			}
		}

		private void fnFreezeCamKD(STB Stb, object Target)
		{
			Stb.AddTxA("GO_KeyHelp_pause", false);
		}

		public bool run(float fcnt)
		{
			if (this.Pe != null)
			{
				this.Pe.fine(120);
			}
			if (this.M2D.curMap == null)
			{
				return true;
			}
			if (this.t_lock_up_input > 0f)
			{
				this.t_lock_up_input = X.VALWALK(this.t_lock_up_input, 0f, fcnt);
			}
			else if (Map2d.can_handle)
			{
				if (IN.isCancelU())
				{
					PostEffect.setable_camera_scale = true;
					this.M2D.Cam.assignBaseMover(this.Pr, 0);
					this.deactivate(true);
					return false;
				}
				Vector2 mouseWheel = IN.MouseWheel;
				if (IN.isLTabO() || mouseWheel.y < 0f)
				{
					this.M2D.Cam.scalingWheel(false);
				}
				if (IN.isRTabO() || mouseWheel.y > 0f)
				{
					this.M2D.Cam.scalingWheel(true);
				}
			}
			this.M2D.Cam.scrollingWheel(IN.isLO(0), IN.isTO(0), IN.isRO(0), IN.isBO(0));
			Map2d curMap = this.M2D.curMap;
			if (IN.isMousePushDown(1))
			{
				this.PosMousePre = 64f * IN.getMousePos(null);
				this.PosMousePre.z = 1f;
			}
			else if (this.PosMousePre.z >= 1f && IN.isMouseOn())
			{
				Vector2 vector = 64f * IN.getMousePos(null);
				if (!vector.Equals(this.PosMousePre))
				{
					float num = curMap.rCLENB * vector.x - curMap.rCLENB * this.PosMousePre.x;
					float num2 = curMap.rCLENB * vector.y - curMap.rCLENB * this.PosMousePre.y;
					float num3 = X.Mn(this.M2D.Cam.getScaleRev(), 1f);
					num *= num3;
					num2 *= num3;
					this.M2D.Cam.moveBy(-num, num2, -1000f);
				}
				X.dl(string.Concat(new string[]
				{
					"p:",
					this.PosMousePre.x.ToString(),
					",",
					this.PosMousePre.y.ToString(),
					"  /  n:",
					vector.x.ToString(),
					",",
					vector.y.ToString()
				}), null, false, false);
				this.PosMousePre = vector;
				this.PosMousePre.z = 1f;
			}
			else
			{
				this.PosMousePre.z = 0f;
			}
			return true;
		}

		public void setLockInput(float t = 20f)
		{
			if (!this.isActive())
			{
				t = X.Mn(t, 4f);
			}
			this.t_lock_up_input = X.Mx(this.t_lock_up_input, t);
		}

		public bool isActive()
		{
			return this.is_active;
		}

		public override string ToString()
		{
			return "M2FreezeCamera";
		}

		private NelM2DBase M2D;

		private TxKeyDesc.KDTicket TkKD;

		private PR Pr;

		private PostEffectItem Pe;

		private bool is_active;

		private TxKeyDesc.FnGetKD FD_fnFreezeCamKD;

		private float t_lock_up_input;

		private Vector3 PosMousePre;
	}
}
