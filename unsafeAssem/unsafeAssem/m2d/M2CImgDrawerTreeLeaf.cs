using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerTreeLeaf : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerTreeLeaf(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta, string tag_key)
			: base(Md, _lay, _Cp, true)
		{
			this.index = this.Cp.index;
			this.use_shift = true;
			this.slice_x = X.Mx(Meta.GetI(tag_key, 0, 0), 1);
			this.slice_y = X.Mx(Meta.GetI(tag_key, 0, 1), 1);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (Ms.vertexCount != (this.slice_x + 1) * (this.slice_y + 1))
			{
				base.SliceRectMesh(Ms, this.slice_x, this.slice_y);
			}
			return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public void activate()
		{
			this.act_time = 24f;
			base.Mp.PtcSTsetVar("x", (double)this.Cp.mapcx).PtcSTsetVar("y", (double)this.Cp.mapcy).PtcST("drawer_tree_leaf_activate", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public override int redraw(float fcnt)
		{
			this.dcnt += fcnt;
			if (this.act_time > 0f)
			{
				this.act_time = X.Mx(0f, this.act_time - fcnt);
			}
			if (this.dcnt >= (float)((this.act_time > 0f) ? 4 : 10))
			{
				this.dcnt = 0f;
				this.ni++;
			}
			else if (!this.force_redraw)
			{
				return base.redraw(fcnt);
			}
			this.force_redraw = false;
			int num = this.slice_x + 1;
			int num2 = this.slice_y + 1;
			for (int i = 0; i < 3; i++)
			{
				uint ran = X.GETRAN2(this.index * 9 + this.ni + i, (this.ni * 3 + this.index) % 11 + 2 + i);
				int num3 = (int)((X.RAN(ran, 474) * 0.33f + 0.33f) * (float)num);
				int num4 = (int)((X.RAN(ran, 1021) * 0.33f + 0.66f) * (float)num2);
				float num5 = (float)X.MPF(X.RAN(ran, 1893) > 0.5f) * 3.1415927f;
				if (this.act_time <= 0f)
				{
					num5 *= X.NI(0.003f, 0.008f, X.RAN(ran, 1110));
				}
				else
				{
					num5 *= X.NI(0.01f, 0.02f, X.RAN(ran, 1110));
				}
				for (int j = 0; j < 7; j++)
				{
					int num6 = num3 - 3 + j;
					if (num6 >= 0 && num6 < num)
					{
						for (int k = 0; k < 7; k++)
						{
							int num7 = num4 - 3 + k;
							if (num7 >= 0 && num7 < num2)
							{
								float num8 = X.LENGTHXYS((float)num3, (float)num4, (float)num6, (float)num7);
								if (num8 <= 3f)
								{
									float num9 = X.NI(1f, 0.12f, num8 / 3f) * num5;
									Vector2 vector = new Vector2((float)(num6 - num3) * (float)this.Cp.Img.iwidth / (float)num, (float)(num7 - num4) * (float)this.Cp.Img.iheight / (float)num2);
									Vector2 vector2 = X.ROTV2e(vector, num9);
									this.ASh[num7 * num + num6] = vector2 - vector;
								}
							}
						}
					}
				}
			}
			this.need_reposit_flag = true;
			return base.redraw(fcnt);
		}

		protected int slice_x;

		protected int slice_y;

		private int index;

		private int ni;

		private float dcnt;

		private float act_time;

		protected bool force_redraw;
	}
}
