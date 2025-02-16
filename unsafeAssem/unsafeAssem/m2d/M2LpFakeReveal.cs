using System;
using XX;

namespace m2d
{
	public class M2LpFakeReveal : M2LpNearCheck
	{
		public M2LpFakeReveal(string _key, int _index, M2MapLayer _Lay, bool _created_by_after = false)
			: base(_key, _index, _Lay)
		{
			this.created_by_after = _created_by_after;
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.player_in_margin_pixel = 10f;
			if (this.Lay.LpFakeReveal != null)
			{
				X.de("複数の FakeDeclare を用意することはできません" + ((this != null) ? this.ToString() : null), null);
			}
			else
			{
				this.Lay.LpFakeReveal = this;
			}
			this.Meta = this.Lay.getLayerMeta();
		}

		public override void initAction(bool normal_map)
		{
			if (this.Meta.is_decrared && !this.Meta.auto_declare)
			{
				this.auto_assign_to_NM = false;
			}
			else
			{
				this.auto_assign_to_NM = true;
				this.T_FADE = 30f;
				this.release_map_margin = 0f;
			}
			this.df_state = M2LpFakeReveal.DF_STATE.SHOWN;
			base.initAction(normal_map);
		}

		protected override bool nearCheck(M2Mover Mv)
		{
			return (!this.Meta.auto_declare && base.activated) || base.nearCheck(Mv);
		}

		public override bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			return (this.Meta.auto_declare || !base.activated) && base.nearCheck(Mv, NTk);
		}

		public override void initEnter(M2Mover Mv)
		{
			if (!base.activated)
			{
				this.Meta.is_decrared = true;
				if (!this.Meta.auto_declare)
				{
					this.Mp.NM.Deassign(this);
				}
				this.Mp.getEffectForChip().setE("fake_layer_dissolve", 0f, this.T_FADE, 0f, this.Lay.index, 0);
				if (this.df_state == M2LpFakeReveal.DF_STATE.SHOWN)
				{
					this.df_state = M2LpFakeReveal.DF_STATE.NEED_HIDE;
				}
			}
			base.initEnter(Mv);
		}

		public override void quitEnter(M2Mover Mv)
		{
			if (base.activated)
			{
				if (!this.Meta.auto_declare)
				{
					return;
				}
				this.Mp.getEffectForChip().setE("fake_layer_dissolve", 0f, this.T_FADE, 1f, this.Lay.index, 0);
				this.Meta.is_decrared = false;
				if (this.df_state == M2LpFakeReveal.DF_STATE.HIDDEN || this.df_state == M2LpFakeReveal.DF_STATE.HIDDEN_FINALIZED)
				{
					this.df_state = M2LpFakeReveal.DF_STATE.NEED_SHOW;
				}
			}
			base.quitEnter(Mv);
		}

		public override bool run(float fcnt)
		{
			if (!X.D)
			{
				return true;
			}
			float t = this.t;
			fcnt = Map2d.TSbase * (float)X.AF;
			if (!base.run(fcnt))
			{
				if (this.df_state == M2LpFakeReveal.DF_STATE.NEED_SHOW)
				{
					this.df_state = M2LpFakeReveal.DF_STATE.SHOWN;
					this.Lay.fakeWallDissolveFinalize(true);
				}
				return false;
			}
			if (this.t >= 0f)
			{
				if (this.df_state != M2LpFakeReveal.DF_STATE.HIDDEN_FINALIZED && this.t <= this.T_FADE + 10f)
				{
					if (this.df_state == M2LpFakeReveal.DF_STATE.NEED_HIDE)
					{
						this.df_state = M2LpFakeReveal.DF_STATE.HIDDEN;
						this.Lay.fakeWallDissolveFinalize(false);
					}
					float num = X.ZLINE(this.t - 4f, this.T_FADE - 4f);
					this.Lay.GRD.setAlpha(1f - num);
					if (this.t >= this.T_FADE)
					{
						this.df_state = M2LpFakeReveal.DF_STATE.HIDDEN_FINALIZED;
						if (!this.Meta.auto_declare)
						{
							return false;
						}
					}
				}
			}
			else if (this.df_state != M2LpFakeReveal.DF_STATE.SHOWN && this.t <= this.T_FADE + 10f)
			{
				float num2 = X.ZLINE(-this.t, this.T_FADE);
				this.Lay.GRD.setAlpha(num2);
			}
			return true;
		}

		public readonly bool created_by_after;

		private METAMapLayer Meta;

		public M2DrawBinder Ed;

		private M2LpFakeReveal.DF_STATE df_state;

		private enum DF_STATE : byte
		{
			SHOWN,
			NEED_SHOW,
			NEED_HIDE,
			HIDDEN,
			HIDDEN_FINALIZED
		}
	}
}
