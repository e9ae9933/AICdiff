using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public sealed class NelChipUkiwa : NelChip
	{
		public NelChipUkiwa(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (!Map2d.editor_decline_lighting)
			{
				if (lay <= 2)
				{
					Md = this.Mp.MyDrawerG;
				}
				return new NelChipUkiwa.M2CImgDrawerDrawUkiwa(Md, lay, this);
			}
			return base.CreateDrawer(ref Md, lay, Meta, Pre_Drawer);
		}

		public override void initAction(bool normal_map)
		{
			if (this.Mv != null)
			{
				return;
			}
			base.initAction(normal_map);
			this.Mv = this.Mp.createMover<NelChipUkiwa.UkiwaMover>("CM-" + base.unique_key, this.mapcx, this.mapcy, false, false);
			this.Mv.initUkiwa(this);
			List<M2Puts> list = new List<M2Puts>(1) { this };
			this.Mv.attractChips(list, false);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.Mv != null)
			{
				this.Mv.close_action_destruction = true;
				this.Mp.removeMover(this.Mv);
				this.Mv = null;
			}
		}

		public bool isPukapuka()
		{
			return this.Mv != null && this.Mv.isPukapuka();
		}

		private NelChipUkiwa.UkiwaMover Mv;

		private class UkiwaMover : ChipMover
		{
			public void initUkiwa(NelChipUkiwa _Cp)
			{
				this.Cp = _Cp;
				this.cp_drawy0 = this.Cp.drawy;
				base.initTransEffecter();
			}

			public override void appear(Map2d Mp)
			{
				base.appear(Mp);
				this.dep_drawy = this.drawy0;
			}

			public bool checkWaterFill(int shift_y)
			{
				int num = (int)((float)this.Cp.drawx * this.Mp.rCLEN);
				int num2 = (int)(X.Mn((float)this.cp_drawy0, (float)this.Cp.drawy + (float)this.Cp.iheight * 0.8f) * this.Mp.rCLEN) + shift_y;
				int num3 = X.Mx(num + 1, X.IntC((float)(this.Cp.drawx + this.Cp.iwidth) * this.Mp.rCLEN));
				int num4 = num2 + 1;
				for (int i = num; i < num3; i++)
				{
					for (int j = num2; j < num4; j++)
					{
						if (!CCON.isWater(this.Mp.getConfig(i, j)))
						{
							return false;
						}
					}
				}
				return true;
			}

			public override void runPre()
			{
				base.runPre();
				if (this.fine_t <= 0f)
				{
					this.fine_t = 5f;
					if (!this.checkWaterFill(0))
					{
						this.dep_drawy = (float)((int)X.Mn(this.drawy + this.Mp.CLEN, this.drawy0));
					}
					else
					{
						float num = (float)(this.checkWaterFill(-1) ? (-1) : 0);
						this.dep_drawy = X.Mn(this.drawy0, ((float)X.IntC(this.Cp.mbottom - 0.12f) + num - 0.6f) * base.CLEN);
					}
				}
				this.fine_t -= this.TS;
				if (this.dep_drawy != this.drawy)
				{
					this.moveBy(0f, (X.VALWALK(this.drawy, this.dep_drawy, ((this.drawy < this.dep_drawy) ? 0.04f : 0.02f) * this.Mp.CLEN) - this.drawy) * this.Mp.rCLEN, false);
					this.pos_reset = true;
				}
			}

			public bool isPukapuka()
			{
				return X.Abs((int)this.drawy - (int)this.drawy0) > 2;
			}

			public override IFootable initCarry(ICarryable FootD)
			{
				IFootable footable = base.initCarry(FootD);
				if (footable != null)
				{
					this.TeCon.setInitCarryBouncy(4f, 24, 0);
				}
				return footable;
			}

			public override void quitCarry(ICarryable FootD)
			{
				base.quitCarry(FootD);
				if (this.TeCon != null)
				{
					this.TeCon.setInitCarryBouncy(3f, 24, 0);
				}
			}

			private float fine_t;

			private NelChipUkiwa Cp;

			private float dep_drawy;

			private int cp_drawy0;
		}

		private sealed class M2CImgDrawerDrawUkiwa : M2CImgDrawer
		{
			public M2CImgDrawerDrawUkiwa(MeshDrawer Md, int _lay, M2Puts _Cp)
				: base(Md, _lay, _Cp, true)
			{
				this.CpUkiwa = this.Cp as NelChipUkiwa;
			}

			public override int redraw(float fcnt)
			{
				bool flag = this.CpUkiwa.isPukapuka();
				if (!flag && !this.pukapuka)
				{
					return base.redraw(fcnt);
				}
				int vertexMax = this.Md.getVertexMax();
				int triMax = this.Md.getTriMax();
				base.revertVerAndTriIndexFirstSaved(false);
				float num = this.CpUkiwa.drawx2meshx((float)this.Cp.drawx);
				float num2 = this.CpUkiwa.drawy2meshy((float)this.Cp.drawy);
				PxlMeshDrawer mainMesh = this.Cp.Img.getMainMesh();
				if (mainMesh == null)
				{
					return 0;
				}
				this.Md.Col = base.Lay.LayerColor.C;
				this.Md.RotaMesh(num, num2, 1f, 1f, this.Cp.draw_rotR + (flag ? (X.COSI(base.Mp.floort + (float)(this.Cp.index * 13), 330f) * 0.02f * 3.1415927f) : 0f), mainMesh, this.Cp.flip, false);
				this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
				this.pukapuka = flag;
				return base.layer2update_flag;
			}

			private readonly NelChipUkiwa CpUkiwa;

			private bool pukapuka;
		}
	}
}
