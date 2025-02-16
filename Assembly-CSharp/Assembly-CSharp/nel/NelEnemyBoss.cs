using System;
using m2d;
using nel.smnp;
using XX;

namespace nel
{
	public abstract class NelEnemyBoss : NelEnemy
	{
		public override void initSummoned(SmnEnemyKind K, bool is_sudden, int _dupe_count)
		{
			base.initSummoned(K, is_sudden, _dupe_count);
			this.reassignCamFocus();
		}

		public void reassignCamFocus()
		{
			if (this.FocArea == null && this.Summoner != null && this.is_alive)
			{
				if (this.Summoner.countActiveEnemy((NelEnemy N) => N is NelEnemyBoss, true) <= 1)
				{
					this.FocArea = new M2LpCamFocus("BossFocus", -1, this.Mp.get_KeyLayer());
					M2LpSummon lp = this.Summoner.Lp;
					this.FocArea.Set(lp);
					this.Mp.NM.Assign(NearManager.NCK.CENTER_PR, this.FocArea);
					this.FocArea.focus_level_x = this.foc_center_level_x;
					this.FocArea.focus_level_y = this.foc_center_level_y;
				}
			}
		}

		public static void checkCamFocus()
		{
		}

		public override void runPost()
		{
			base.runPost();
			if (this.need_check_cam_focus_area)
			{
				this.need_check_cam_focus_area = false;
				this.reassignCamFocus();
			}
			if (this.FocArea != null && X.D)
			{
				M2Mover baseMover = base.M2D.Cam.getBaseMover();
				if (baseMover != null)
				{
					float num = X.absMn((this.drawx - baseMover.drawx) * this.foc_shift_x_ratio, this.foc_shift_x_max);
					float num2 = X.absMn(-(this.drawy - baseMover.drawy) * this.foc_shift_y_ratio, this.foc_shift_y_max);
					this.FocArea.focus_shift_x = num + this.foc_base_shift_x;
					this.FocArea.focus_shift_y = num2 + this.foc_base_shift_y;
				}
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.FocArea != null)
			{
				if (this.Mp.NM != null)
				{
					this.Mp.NM.Deassign(this.FocArea);
				}
				this.FocArea = null;
			}
			if (this.Summoner != null)
			{
				this.Summoner.countActiveEnemy(delegate(NelEnemy N)
				{
					if (N is NelEnemyBoss)
					{
						(N as NelEnemyBoss).need_check_cam_focus_area = true;
					}
					return false;
				}, true);
				base.destruct();
			}
		}

		public override bool showFlashEatenEffect(bool for_effect = false)
		{
			return true;
		}

		protected float foc_shift_x_max = 80f;

		protected float foc_shift_y_max = 70f;

		protected float foc_shift_x_ratio = 0.7f;

		protected float foc_shift_y_ratio = 0.3f;

		protected float foc_center_level_x;

		protected float foc_center_level_y;

		protected float foc_base_shift_x;

		protected float foc_base_shift_y;

		protected M2LpCamFocus FocArea;

		private bool need_check_cam_focus_area;
	}
}
