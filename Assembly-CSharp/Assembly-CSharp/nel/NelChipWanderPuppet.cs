using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipWanderPuppet : NelChipWanderNpcSpot
	{
		public NelChipWanderPuppet(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override WanderingManager.TYPE npc_type
		{
			get
			{
				return WanderingManager.TYPE.PUP;
			}
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdL, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			return base.entryChipMesh(MdB, MdB, MdT, MdT, MdT, sx, sy, _zm, _rotR);
		}
	}
}
