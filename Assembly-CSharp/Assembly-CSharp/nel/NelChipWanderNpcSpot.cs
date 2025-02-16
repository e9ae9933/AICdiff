using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipWanderNpcSpot : NelChip
	{
		public NelChipWanderNpcSpot(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public virtual WanderingManager.TYPE npc_type
		{
			get
			{
				return FEnum<WanderingManager.TYPE>.Parse(base.Meta.GetS("wander_npc"), WanderingManager.TYPE.NIG);
			}
		}

		public override void initAction(bool normal_map)
		{
			if (base.active_removed)
			{
				return;
			}
			if (this.hide_layer < 0)
			{
				this.checkNpcAppeared(true);
			}
			else
			{
				base.NM2D.WDR.Get(this.npc_type).checkBench(this, -1f, 0f, true);
			}
			base.initAction(normal_map);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.hide_layer = -1;
		}

		protected void checkNpcAppeared(bool attach_mover = true)
		{
			if (this.hide_layer >= 0)
			{
				return;
			}
			this.npc_appeared = base.NM2D.WDR.Get(this.npc_type).checkBench(this, -1f, 0f, attach_mover);
			this.hide_layer = 0;
			string[] array = base.Meta.Get("npc_not_here_hide_layer");
			if (array != null && !this.npc_appeared)
			{
				for (int i = array.Length - 1; i >= 0; i--)
				{
					int num = X.NmI(array[i], -1, false, false);
					if (num >= 0)
					{
						this.hide_layer |= 1 << num;
					}
				}
			}
			array = base.Meta.Get("npc_here_hide_layer");
			if (array != null && this.npc_appeared)
			{
				for (int j = array.Length - 1; j >= 0; j--)
				{
					int num2 = X.NmI(array[j], -1, false, false);
					if (num2 >= 0)
					{
						this.hide_layer |= 1 << num2;
					}
				}
			}
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (this.hide_layer < 0)
			{
				this.checkNpcAppeared(false);
			}
			if ((this.hide_layer & (1 << lay)) != 0)
			{
				return null;
			}
			return base.CreateDrawer(ref Md, lay, Meta, Pre_Drawer);
		}

		protected bool npc_appeared;

		private int hide_layer = -1;
	}
}
