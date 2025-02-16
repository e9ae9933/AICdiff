using System;
using XX;

namespace m2d
{
	public class M2MvColliderCreatorCM : M2MvColliderCreator
	{
		public M2MvColliderCreatorCM(ChipMover _Mv, ChipMover.ExMapChip[] _APuts, bool _do_not_bind_BCC = false)
			: base(_Mv)
		{
			this.CM = _Mv;
			this.APuts = _APuts;
			this.do_not_bind_BCC_ = _do_not_bind_BCC;
		}

		public override void destruct()
		{
			if (this.BCC != null)
			{
				base.Mp.removeCarryableBCC(this.BCC);
				this.BCC.active = false;
				this.BCC = null;
			}
			base.destruct();
		}

		public static bool FnCanStandDefault(int cfg)
		{
			return CCON.canStand(cfg) && !CCON.isBlockSlope(cfg) && !CCON.isLift(cfg);
		}

		protected override void recreateExecute()
		{
			if (this.Mv.floating)
			{
				return;
			}
			int num = this.APuts.Length;
			DRect BoundsRc = this.CM.getBoundsRect();
			int num2 = X.IntC(BoundsRc.width);
			int num3 = X.IntC(BoundsRc.height);
			float x = base.x;
			float y = base.y;
			M2Pt[,] array = new M2Pt[num2, num3];
			bool flag = false;
			Func<int, bool> func = this.FnCanStand;
			if (func == null)
			{
				func = new Func<int, bool>(M2MvColliderCreatorCM.FnCanStandDefault);
			}
			int num4 = (int)BoundsRc.x;
			int num5 = (int)BoundsRc.y;
			for (int i = 0; i < num; i++)
			{
				M2Chip m2Chip = this.APuts[i].Pt as M2Chip;
				if (m2Chip != null)
				{
					int num6 = m2Chip.mapx - num4;
					int num7 = m2Chip.mapy - num5;
					for (int j = 0; j < m2Chip.clms; j++)
					{
						for (int k = 0; k < m2Chip.rows; k++)
						{
							int config = m2Chip.getConfig(j, k);
							if (!func(config))
							{
								flag = true;
								M2Pt m2Pt = array[num6 + j, num7 + k];
								if (m2Pt == null)
								{
									m2Pt = (array[num6 + j, num7 + k] = new M2Pt(1, 0));
								}
								m2Pt.Add(m2Chip);
							}
						}
					}
				}
			}
			if (!flag)
			{
				this.Mv.floating = true;
				return;
			}
			for (int l = 0; l < num2; l++)
			{
				for (int m = 0; m < num3; m++)
				{
					M2Pt m2Pt2 = array[l, m];
					if (m2Pt2 != null)
					{
						m2Pt2.calcConfig(num4 + l, num5 + m, false);
					}
				}
			}
			if (this.BCC == null)
			{
				this.BCC = new M2BlockColliderContainer(base.Mp, this.Mv);
				if (!this.do_not_bind_BCC_)
				{
					base.Mp.assignCarryableBCC(this.BCC);
				}
			}
			M2Phys physic = this.Mv.getPhysic();
			if (physic != null)
			{
				physic.MyBCC = this.BCC;
			}
			new M2BorderCldCreator(base.Mp, (int)BoundsRc.x, (int)BoundsRc.y, -1f, true)
			{
				FnCanStand = this.FnCanStand,
				collider_margin_u = this.CM.collider_margin * base.Mp.CLEN / 64f
			}.createCollider(array, this.Cld, this.BCC, (float _mapx) => (BoundsRc.x - this.x + _mapx) * this.CLEN * this.Mp.base_scale * 0.015625f, (float _mapy) => -(BoundsRc.y - this.y + _mapy) * this.CLEN * this.Mp.base_scale * 0.015625f);
		}

		public bool do_not_bind_BCC
		{
			get
			{
				return this.do_not_bind_BCC_;
			}
			set
			{
				if (this.do_not_bind_BCC_ == value)
				{
					return;
				}
				this.do_not_bind_BCC_ = value;
				if (this.BCC == null || base.Mp == null)
				{
					return;
				}
				if (value)
				{
					base.Mp.removeCarryableBCC(this.BCC);
					return;
				}
				base.Mp.assignCarryableBCC(this.BCC);
			}
		}

		private readonly ChipMover CM;

		private readonly ChipMover.ExMapChip[] APuts;

		public M2BlockColliderContainer BCC;

		public Func<int, bool> FnCanStand;

		private bool do_not_bind_BCC_;
	}
}
