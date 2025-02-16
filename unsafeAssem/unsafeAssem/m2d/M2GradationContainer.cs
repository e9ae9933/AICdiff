using System;
using XX;

namespace m2d
{
	public class M2GradationContainer : LabelPointContainer<M2GradationRect>
	{
		public M2GradationContainer(M2MapLayer _Lay)
		{
			this.Lay = _Lay;
		}

		public float CLEN
		{
			get
			{
				return this.Lay.CLEN;
			}
		}

		public override void reindex()
		{
			for (int i = 0; i < base.Length; i++)
			{
				base.Get(i).index = i;
			}
		}

		public void initActionPre()
		{
			this.alpha = 1f;
		}

		public void setAlpha(float v)
		{
			this.alpha = v;
			Map2d mp = this.Lay.Mp;
			if (!mp.need_reentry_gradation_flag)
			{
				mp.addUpdateMesh(this.drawGradation(mp.MyDrawerUGrd, mp.MyDrawerBGrd, mp.MyDrawerGGrd, mp.MyDrawerTGrd, true), false);
				if (this.use_chip_layer_order)
				{
					mp.addUpdateMesh(this.drawGradationChipLayer(mp.MyDrawerB, mp.MyDrawerG, mp.MyDrawerT, true), false);
				}
			}
		}

		public int drawGradation(MeshDrawer MdU, MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, bool redrawing = false)
		{
			MdU.Col = MTRX.ColWhite;
			MdB.Col = MTRX.ColWhite;
			MdG.Col = MTRX.ColWhite;
			MdT.Col = MTRX.ColWhite;
			MdU.Identity();
			MdB.Identity();
			MdG.Identity();
			MdT.Identity();
			int length = base.Length;
			int num = 0;
			if (!redrawing)
			{
				this.ver_start_i_u = MdU.getVertexMax();
				this.ver_start_i_b = MdB.getVertexMax();
				this.ver_start_i_g = MdG.getVertexMax();
				this.ver_start_i_t = MdT.getVertexMax();
				int num2 = -1;
				for (int i = length - 1; i >= 0; i--)
				{
					M2GradationRect m2GradationRect = base.Get(i);
					switch (m2GradationRect.order)
					{
					case M2GradationRect.GRDORDER.SKY:
						m2GradationRect.drawTo(this.alpha, MdU, ref num2, false);
						num |= 16;
						break;
					case M2GradationRect.GRDORDER.BACK:
						m2GradationRect.drawTo(this.alpha, MdB, ref num2, false);
						num |= 32;
						break;
					case M2GradationRect.GRDORDER.GROUND:
						m2GradationRect.drawTo(this.alpha, MdG, ref num2, false);
						num |= 64;
						break;
					case M2GradationRect.GRDORDER.TOP:
						m2GradationRect.drawTo(this.alpha, MdT, ref num2, false);
						num |= 128;
						break;
					}
				}
				this.ver_end_i_u = MdU.getVertexMax();
				this.ver_end_i_b = MdB.getVertexMax();
				this.ver_end_i_g = MdG.getVertexMax();
				this.ver_end_i_t = MdT.getVertexMax();
			}
			else
			{
				int num3 = this.ver_start_i_u;
				int num4 = this.ver_start_i_b;
				int num5 = this.ver_start_i_g;
				int num6 = this.ver_start_i_t;
				for (int j = length - 1; j >= 0; j--)
				{
					M2GradationRect m2GradationRect2 = base.Get(j);
					switch (m2GradationRect2.order)
					{
					case M2GradationRect.GRDORDER.SKY:
						if (num3 < this.ver_end_i_u)
						{
							m2GradationRect2.drawTo(this.alpha, MdU, ref num3, false);
							num |= 16;
						}
						break;
					case M2GradationRect.GRDORDER.BACK:
						if (num4 < this.ver_end_i_b)
						{
							m2GradationRect2.drawTo(this.alpha, MdB, ref num4, false);
							num |= 32;
						}
						break;
					case M2GradationRect.GRDORDER.GROUND:
						if (num5 < this.ver_end_i_g)
						{
							m2GradationRect2.drawTo(this.alpha, MdG, ref num5, false);
							num |= 64;
						}
						break;
					case M2GradationRect.GRDORDER.TOP:
						if (num6 < this.ver_end_i_t)
						{
							m2GradationRect2.drawTo(this.alpha, MdT, ref num6, false);
							num |= 128;
						}
						break;
					}
				}
			}
			return num;
		}

		public int drawGradationChipLayer(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, bool redrawing = false)
		{
			MdB.Col = MTRX.ColWhite;
			MdG.Col = MTRX.ColWhite;
			MdT.Col = MTRX.ColWhite;
			MdB.Identity();
			MdG.Identity();
			MdT.Identity();
			int length = base.Length;
			int num = 0;
			bool flag = true;
			if (!redrawing)
			{
				this.ver_start_i_cb = -2;
				this.ver_start_i_cg = -2;
				this.ver_start_i_ct = -2;
				int i = length - 1;
				while (i >= 0)
				{
					M2GradationRect m2GradationRect = base.Get(i);
					switch (m2GradationRect.order)
					{
					case M2GradationRect.GRDORDER.CHIP_B:
						this.drawGradationChipLayer(m2GradationRect, ref MdB, ref this.ver_start_i_cb, flag);
						num |= 1;
						goto IL_00D0;
					case M2GradationRect.GRDORDER.CHIP_G:
						this.drawGradationChipLayer(m2GradationRect, ref MdG, ref this.ver_start_i_cb, flag);
						num |= 2;
						goto IL_00D0;
					case M2GradationRect.GRDORDER.CHIP_T:
						this.drawGradationChipLayer(m2GradationRect, ref MdT, ref this.ver_start_i_cb, flag);
						num |= 4;
						goto IL_00D0;
					}
					IL_00D2:
					i--;
					continue;
					IL_00D0:
					flag = false;
					goto IL_00D2;
				}
				this.ver_end_i_cb = MdB.getVertexMax();
				this.ver_end_i_cg = MdG.getVertexMax();
				this.ver_end_i_ct = MdT.getVertexMax();
			}
			else
			{
				int num2 = this.ver_start_i_cb;
				int num3 = this.ver_start_i_cg;
				int num4 = this.ver_start_i_ct;
				for (int j = length - 1; j >= 0; j--)
				{
					M2GradationRect m2GradationRect2 = base.Get(j);
					switch (m2GradationRect2.order)
					{
					case M2GradationRect.GRDORDER.CHIP_B:
						if (num2 >= 0 && num2 < this.ver_end_i_cb)
						{
							m2GradationRect2.drawTo(this.alpha, MdB, ref num2, true);
							num |= 1;
						}
						break;
					case M2GradationRect.GRDORDER.CHIP_G:
						if (num3 >= 0 && num3 < this.ver_end_i_cg)
						{
							m2GradationRect2.drawTo(this.alpha, MdG, ref num3, true);
							num |= 2;
						}
						break;
					case M2GradationRect.GRDORDER.CHIP_T:
						if (num4 >= 0 && num4 < this.ver_end_i_ct)
						{
							m2GradationRect2.drawTo(this.alpha, MdT, ref num4, true);
							num |= 4;
						}
						break;
					}
				}
			}
			return num;
		}

		private void drawGradationChipLayer(M2GradationRect Grd, ref MeshDrawer Md0, ref int ver, bool first_chip_layer = false)
		{
			MeshDrawer meshDrawer = Md0;
			if (ver == -2)
			{
				ver = Md0.getVertexMax();
				if (!this.Lay.is_chip_arrangeable && Md0 is MdMap)
				{
					MeshDrawer meshDrawer2 = (Md0 as MdMap).checkArrangeableMeshEvacuation();
					if (meshDrawer2 != Md0)
					{
						Md0 = meshDrawer2;
						ver = -1;
					}
				}
				if (Md0 == meshDrawer && first_chip_layer && !Map2d.editor_decline_lighting)
				{
					X.de("Evacuation にGrdRectを乗せられません", null);
				}
			}
			int num = -1;
			Grd.drawTo(this.alpha, Md0, ref num, true);
		}

		public readonly M2MapLayer Lay;

		private float alpha = 1f;

		private int ver_start_i_u;

		private int ver_end_i_u;

		private int ver_start_i_b;

		private int ver_end_i_b;

		private int ver_start_i_g;

		private int ver_end_i_g;

		private int ver_start_i_t;

		private int ver_end_i_t;

		private int ver_start_i_cb;

		private int ver_end_i_cb;

		private int ver_start_i_cg;

		private int ver_end_i_cg;

		private int ver_start_i_ct;

		private int ver_end_i_ct;

		public bool use_chip_layer_order;
	}
}
