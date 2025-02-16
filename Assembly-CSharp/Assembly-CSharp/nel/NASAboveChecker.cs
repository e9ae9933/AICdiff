using System;
using m2d;
using XX;

namespace nel
{
	public class NASAboveChecker : NelEnemyAssist
	{
		public NASAboveChecker(NelEnemy En, float _maxt = 240f, float _timescale_deactive = 3f)
			: base(En)
		{
			this.maxt = _maxt;
			this.timescale_deactive = _timescale_deactive;
		}

		public void Clear()
		{
			this.above_t = 0f;
			this.linear_check = NASAboveChecker.LINEAR_CHK.NEED_CHECK;
		}

		public bool run(float fcnt)
		{
			M2MoverPr aimPr = base.Nai.AimPr;
			if (aimPr == null)
			{
				return true;
			}
			bool flag;
			if (!aimPr.hasFoot())
			{
				this.linear_check = NASAboveChecker.LINEAR_CHK.NEED_CHECK;
				flag = base.mtop - 3.5f > aimPr.y;
			}
			else
			{
				if ((this.linear_check & NASAboveChecker.LINEAR_CHK.NEED_CHECK) == (NASAboveChecker.LINEAR_CHK)0)
				{
					M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
					M2BlockColliderContainer.BCCLine footBCC = aimPr.getFootManager().get_FootBCC();
					if (lastBCC != null && footBCC != null)
					{
						this.linear_check = ((footBCC.is_ladder != lastBCC.is_ladder && lastBCC.isLinearWalkableTo(footBCC, this.linear_check_fallable) != 0) ? NASAboveChecker.LINEAR_CHK.LINEAR : ((NASAboveChecker.LINEAR_CHK)0));
					}
				}
				else if ((this.linear_check & NASAboveChecker.LINEAR_CHK.LINEAR) != (NASAboveChecker.LINEAR_CHK)0)
				{
					M2BlockColliderContainer.BCCLine lastBCC2 = this.FootD.get_LastBCC();
					if (aimPr.getFootManager().get_FootBCC().is_ladder != lastBCC2.is_ladder)
					{
						this.linear_check &= (NASAboveChecker.LINEAR_CHK)254;
					}
				}
				flag = base.y - 1.5f > aimPr.y;
			}
			if (flag && (this.linear_check & NASAboveChecker.LINEAR_CHK.LINEAR) == (NASAboveChecker.LINEAR_CHK)0)
			{
				if (this.above_t < 0f)
				{
					this.above_t = -this.above_t;
				}
				this.above_t += fcnt;
			}
			else if (this.above_t > 0f)
			{
				if (this.above_t > 0f)
				{
					this.above_t = -this.above_t;
				}
				this.above_t = X.VALWALK(this.above_t, 0f, fcnt * this.timescale_deactive);
			}
			return this.above_t >= this.maxt;
		}

		public bool isAboveActive()
		{
			return this.above_t >= this.maxt;
		}

		public void ensureDelay(float t)
		{
			this.above_t = X.MMX(0f, X.Abs(this.above_t), this.maxt - t) * (float)X.MPF(this.above_t > 0f);
		}

		private float above_t;

		public float maxt;

		public float timescale_deactive = 1f;

		public bool linear_check_fallable = true;

		private NASAboveChecker.LINEAR_CHK linear_check;

		private enum LINEAR_CHK : byte
		{
			NEED_CHECK = 8,
			LINEAR = 1
		}
	}
}
