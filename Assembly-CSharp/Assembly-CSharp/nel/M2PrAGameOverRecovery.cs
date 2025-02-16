using System;
using m2d;
using XX;

namespace nel
{
	internal sealed class M2PrAGameOverRecovery : M2PrARunner
	{
		public M2PrAGameOverRecovery(PR _Pr)
			: base(_Pr)
		{
			this.PrM = this.Pr as PRMain;
		}

		public override M2PrARunner activate()
		{
			base.addD(M2MoverPr.DECL.THROW_RAY);
			return this;
		}

		public override void quitSPPR(PR Pr, PR.STATE aftstate)
		{
		}

		public override bool runPreSPPR(PR Pr, float fcnt, ref float t_state)
		{
			PR.STATE state = base.state;
			PR.PunchDecline(50, false);
			if (t_state <= 0f)
			{
				Pr.VO.breath_key = "breath_e";
			}
			if (!base.hasD(M2MoverPr.DECL.INIT_A))
			{
				if (base.Mp.BCC != null && base.Mp.BCC.is_prepared)
				{
					float num = this.Phy.tstacked_y + base.sizey;
					M2BlockColliderContainer.BCCLine bccline;
					base.Mp.BCC.isFallable(base.x, num - 0.5f, base.sizex, 3f, out bccline, true, true, -1f, null);
					if (bccline != null)
					{
						float num2 = X.Mn(bccline.slopeBottomY(base.x - base.sizex), bccline.slopeBottomY(base.x + base.sizex));
						this.Phy.addTranslateStack(0f, X.absMn(num2 - num, 0.8f));
					}
					if (Pr.getNearBench(false, false) != null)
					{
						base.NM2D.CheckPoint.fineFoot(Pr, this.FootD.get_FootBCC(), true);
					}
				}
				else
				{
					this.Phy.addLockGravityFrame(1);
				}
			}
			bool flag = t_state >= 40f;
			if (flag && base.EggCon.isActive())
			{
				base.EggCon.forcePushout(false, true);
				flag = false;
			}
			M2PrABench.checkProgressBench(Pr, flag);
			return true;
		}

		private PRMain PrM;
	}
}
