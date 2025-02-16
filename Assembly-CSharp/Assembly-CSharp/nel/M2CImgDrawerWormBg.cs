using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerWormBg : M2CImgDrawer
	{
		public M2CImgDrawerWormBg(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta, string tag_key)
			: base(Md, _lay, _Cp, true)
		{
			this.index = this.Cp.index;
			this.use_shift = true;
			this.slice_x = X.Mx(Meta.GetI(tag_key, 1, 0), 1);
			this.slice_y = X.Mx(Meta.GetI(tag_key, 1, 1), 1);
			this.wait = (float)((int)(18446744073709551612UL - (ulong)(X.xors() % 46U)));
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (Ms.vertexCount != (this.slice_x + 1) * (this.slice_y + 1))
			{
				base.SliceRectMesh(Ms, this.slice_x, this.slice_y);
			}
			return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public override int redraw(float fcnt)
		{
			if (Map2d.editor_decline_lighting || this.Cp.Lay.is_fake)
			{
				return base.redraw(fcnt);
			}
			this.wait += fcnt;
			if (this.wait < 0f && !this.force_redraw)
			{
				return base.redraw(fcnt);
			}
			this.dcnt += fcnt;
			if (this.dcnt >= 8f)
			{
				this.dcnt = 0f;
			}
			else if (!this.force_redraw)
			{
				return base.redraw(fcnt);
			}
			int num = this.slice_x + 1;
			int num2 = this.slice_y + 1;
			int num3 = num * num2;
			if (this.bounce_index == -1)
			{
				int num4 = X.xors(num * 2 + num2 * 2);
				int num5;
				if (num4 < num)
				{
					num5 = num4;
				}
				else if (num4 < num + num2)
				{
					num5 = num - 1 + (num4 - num) * num;
				}
				else if (num4 < num + num2 + num)
				{
					num5 = (num2 - 1) * num + num4 - (num + num2);
				}
				else
				{
					num5 = (num4 - (num + num2 + num)) * num;
				}
				this.bounce_index = num5;
				Vector2 vector = this.APos[this.bounce_index];
				Vector2 vector2 = this.APos[0];
				Vector2 vector3 = this.APos[num3 - 1];
				this.bounce_agR = X.GAR((vector2.x + vector3.x) / 2f, (vector2.y + vector3.y) / 2f, vector.x, vector.y) + X.NIXP(-15f, 15f) / 180f * 3.1415927f;
				this.lvl = 3.6f + 10.7f * X.XORSP();
			}
			int num6 = this.bounce_index % num;
			int num7 = this.bounce_index / num;
			float num8 = X.ZPOW(40f - this.wait, 40f);
			float num9 = this.lvl * num8 * X.Cos(this.bounce_agR);
			float num10 = this.lvl * num8 * X.Sin(this.bounce_agR);
			for (int i = 0; i < num3; i++)
			{
				Vector3 vector4 = this.ASh[i];
				int num11 = i % num;
				int num12 = i / num;
				float num13 = X.ZPOW(1f - X.LENGTHXY((float)num11, (float)num12, (float)num6, (float)num7) / 4f);
				vector4.Set(num13 * num9, num13 * num10, vector4.z);
				this.ASh[i] = vector4;
			}
			if (this.wait >= 40f)
			{
				this.wait = (float)(18446744073709551612UL - (ulong)(X.xors() % 20U));
				this.bounce_index = -1;
				this.dcnt = 0f;
			}
			this.need_reposit_flag = true;
			return base.redraw(fcnt);
		}

		protected int slice_x;

		protected int slice_y;

		private int index;

		private float wait = -90f;

		private float dcnt;

		private float lvl = 3f;

		private int bounce_index = -1;

		private float bounce_agR;

		protected bool force_redraw;
	}
}
