using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipWanderTilde : NelChipWanderNpcSpot
	{
		public NelChipWanderTilde(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.Bonf = new BonfireEffector(this).readMeta(base.Meta, "wander_tilde", 0);
		}

		public override WanderingManager.TYPE npc_type
		{
			get
			{
				return WanderingManager.TYPE.TLD;
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.npc_appeared)
			{
				this.EdPt = this.Bonf.setEDC(null, null);
				this.EdOnEf = this.Bonf.setEDOnEffect(null, null);
				if (this.SLoop == null)
				{
					this.SLoop = this.Mp.M2D.Snd.Environment.AddLoop("npc_atelier_nabe", base.unique_key, this.mapcx, this.mapcy, 4f, 4f, 26f, 26f, null);
				}
			}
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.EdPt = this.Mp.remED(this.EdPt);
			this.EdOnEf = this.Mp.remED(this.EdOnEf);
			if (this.SLoop != null)
			{
				this.Mp.M2D.Snd.Environment.RemLoop("atelier_nabe", base.unique_key);
				this.SLoop = null;
			}
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdL, MeshDrawer MdLT, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			return base.entryChipMesh(MdB, MdB, MdB, MdT, MdTT, MdTT, sx, sy, _zm, _rotR);
		}

		private BonfireEffector Bonf;

		protected M2DrawBinder EdPt;

		protected M2DrawBinder EdOnEf;

		private M2SndLoopItem SLoop;
	}
}
