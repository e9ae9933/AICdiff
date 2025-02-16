using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class EffectNel : Effect<EffectItemNel>
	{
		public EffectNel(Map2d _Mp, int _max = 240)
			: base(_Mp.gameObject, _max)
		{
			this.Mp = _Mp;
			this.Cam = this.Mp.M2D.Cam;
		}

		public EffectNel(GameObject Gob, int _max = 240)
			: base(Gob, _max)
		{
		}

		public void mapPosAutoFix(Map2d Mp)
		{
			this.nomap_ratio_x = Mp.CLEN * 0.015625f;
			this.nomap_ratio_y = -Mp.CLEN * 0.015625f;
			this.nomap_add_x = (float)(-(float)Mp.width) * 0.015625f / 2f;
			this.nomap_add_y = (float)Mp.height * 0.015625f / 2f;
			this.Mp = null;
		}

		public override Vector3 calcMeshXY(float _mesh_base_x, float _mesh_base_y, MeshDrawer Md, out bool force_reset_z)
		{
			force_reset_z = false;
			if (this.Mp != null)
			{
				return new Vector3(this.Mp.ux2effectScreenx(this.Mp.map2ux(_mesh_base_x)), this.Mp.uy2effectScreeny(this.Mp.map2uy(_mesh_base_y)), (Md != null && Md.is_bottom) ? this.bottomBaseZ : this.topBaseZ);
			}
			return new Vector3(_mesh_base_x * this.nomap_ratio_x + this.nomap_add_x, _mesh_base_y * this.nomap_ratio_y + this.nomap_add_y, (Md != null && Md.is_bottom) ? this.bottomBaseZ : this.topBaseZ);
		}

		public override bool isinCameraPtc(EffectItem Ef, float cleft_px, float cbtm_px, float cright_px, float ctop_px, float camera_check_zm_mul = 1f, float camera_check_zm_add = 0f)
		{
			if (this.Mp != null)
			{
				bool flag;
				Vector3 vector = this.calcMeshXY(Ef.x, Ef.y, null, out flag);
				return this.Mp.M2D.Cam.isCoveringEffectUPos(vector.x + cleft_px * 0.015625f, vector.x + cright_px * 0.015625f, vector.y + cbtm_px * 0.015625f, vector.y + ctop_px * 0.015625f, 0f);
			}
			return true;
		}

		private Map2d Mp;

		private M2Camera Cam;

		public float nomap_ratio_x = 1f;

		public float nomap_add_x;

		public float nomap_ratio_y = 1f;

		public float nomap_add_y;
	}
}
