using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ReelOutlineDrawer : Block3DDrawer
	{
		public ReelOutlineDrawer()
		{
			this.Outer = new Block3DDrawer();
			this.Recreate();
			this.point_blur_lgt = (this.Outer.point_blur_lgt = 0f);
			this.hen_thick = (this.Outer.hen_thick = 0.015625f);
			this.point_blur_in_alp = (this.Outer.point_blur_in_alp = 0.03f);
			this.Outer.hen_blur_thick = (this.hen_blur_thick = 0.15625f);
			this.hen_blur_in_alp = 0.125f;
			this.Outer.hen_blur_in_alp = 0.18f;
			this.point_lgt = (this.Outer.point_lgt = 0.03125f);
		}

		public void Recreate()
		{
			base.clear();
			this.Outer.clear();
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(0f, 0f, -1.5f));
			this.Outer.AddPoly(1f, 12, matrix4x, false);
			matrix4x = Matrix4x4.Translate(new Vector3(0f, 0f, 1.5f));
			this.Outer.AddPoly(1f, 12, matrix4x, false);
			base.AddPoly(1.5f, 12, Matrix4x4.identity, false);
			matrix4x = Matrix4x4.Translate(new Vector3(0f, 0f, -1.12f));
			Vector3[] array = base.AddPoly(0.75f, 8, matrix4x, true);
			matrix4x = Matrix4x4.Translate(new Vector3(0f, 0f, 1.12f));
			Vector3[] array2 = base.AddPoly(0.75f, 8, matrix4x, true);
			for (int i = 0; i < 8; i++)
			{
				base.AddHen(array[i], array2[i], false);
			}
		}

		public void drawTo(MeshDrawer Md, float x, float y, float scale_px, uint ran0, float xy_rot_lvl = 1f)
		{
			this.drawTo(Md, x, y, Matrix4x4.identity, scale_px, ran0, xy_rot_lvl);
		}

		public void drawTo(MeshDrawer Md, float x, float y, Matrix4x4 MxPre, float scale_px, uint ran0, float xy_rot_lvl = 1f)
		{
			scale_px *= 0.015625f;
			this.ran0 = ran0;
			uint ran = X.GETRAN2(ran0, ran0 & 7U);
			float num = X.RAN(ran, 2154) * 360f + X.ANMPT(180, 360f);
			Matrix4x4 matrix4x;
			if (xy_rot_lvl > 0f)
			{
				matrix4x = Matrix4x4.Rotate(Quaternion.Euler((X.RAN(ran, 509) * 360f + X.ANMPT(240, 360f)) * xy_rot_lvl, 0f, 0f)) * MxPre;
				matrix4x *= Matrix4x4.Rotate(Quaternion.Euler(0f, (X.RAN(ran, 1318) * 360f + X.ANMPT(320, 360f)) * xy_rot_lvl, 0f));
			}
			else
			{
				matrix4x = MxPre;
			}
			Matrix4x4 matrix4x2 = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, num));
			Matrix4x4 matrix4x3 = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -num));
			Matrix4x4 matrix4x4 = Matrix4x4.Translate(new Vector3(x * 0.015625f, y * 0.015625f, 0f)) * Matrix4x4.Scale(new Vector3(scale_px, scale_px, scale_px));
			base.drawTo(Md, matrix4x4 * matrix4x2);
			this.Outer.drawTo(Md, matrix4x4 * matrix4x3);
		}

		public override uint col_point
		{
			get
			{
				return this.col_point_;
			}
			set
			{
				this.Outer.col_point = value;
				this.col_point_ = value;
			}
		}

		public override uint col_hen
		{
			get
			{
				return this.col_hen_;
			}
			set
			{
				this.Outer.col_hen = value;
				this.col_hen_ = value;
			}
		}

		public uint ran0;

		private Block3DDrawer Outer;
	}
}
