using System;
using UnityEngine;

namespace XX
{
	public class PTCThreadRunnerOnce<T> : Effect<T> where T : EffectItem
	{
		public PTCThreadRunnerOnce(MeshDrawer Md, GameObject _Gob, int _max = 240)
			: base(_Gob, _max)
		{
			this.TargetMd = Md;
			this.useMeshDrawer = false;
		}

		public override MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, C32 C, Material Mtr, out bool z_resetted, bool bottom_flag = false, C32 CGrd = null, bool colormatch = false)
		{
			this.TargetMd.Col = C.C;
			this.TargetMd.base_x = _mesh_base_x;
			this.TargetMd.base_y = _mesh_base_y;
			this.TargetMd.base_z = ((!bottom_flag) ? this.topBaseZ : this.bottomBaseZ);
			this.TargetMd.base_z += MTRX.zshift_blend(this.TargetMd.getMaterial(), null) * 0.001f;
			z_resetted = true;
			this.TargetMd.pixelsPerUnit = this.ppu;
			return this.TargetMd;
		}

		public MeshDrawer TargetMd;
	}
}
