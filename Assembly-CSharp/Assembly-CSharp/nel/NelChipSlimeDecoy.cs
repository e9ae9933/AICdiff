using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipSlimeDecoy : NelChip, ITeColor
	{
		public NelChipSlimeDecoy(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Mw != null)
			{
				return;
			}
			this.damage_ratio = X.NI(1f, 0.4f, base.Meta.GetNm("slime_decoy", 1f, 0));
			this.hit_ptc = base.Meta.GetSi(0, "hit_ptcst");
			this.ManageArea = PUZ.IT.isBelongTo(this);
			if (this.Mw != null)
			{
				IN.DestroyOne(this.Mw.gameObject);
			}
			this.Mw = this.Mp.createMover<M2SlimeDecoy>("-slimedecoy-" + this.index.ToString(), this.mapcx, this.collider_cenpos_mapy, false, true);
			this.Mw.appear(this, this.Mp);
			this.BccChecker = new M2BCCPosCheckerCp(this, null);
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			return new NelChipSlimeDecoy.M2CImgDrawerSlimeDecoy(Md, lay, this);
		}

		public float collider_cenpos_mapy
		{
			get
			{
				return this.mbottom - this.cld_maph * 0.5f;
			}
		}

		public override void translateByChipMover(float ux, float uy, C32 AddCol, int drawx0, int drawy0, int move_drawx = 0, int move_drawy = 0, bool stabilize_move_map = false)
		{
			base.translateByChipMover(ux, uy, AddCol, drawx0, drawy0, move_drawx, move_drawy, stabilize_move_map);
			if (this.Mw != null)
			{
				this.Mw.finePos(ux, uy);
			}
		}

		public Color32 getColorTe()
		{
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Buf, C32 _CMul, C32 _CAdd)
		{
			this.CMul = _CMul.C;
			this.CAdd = _CAdd.C;
			this.need_fine_color = true;
		}

		protected override void initActiveRemoveKey()
		{
			if (this.Mw != null)
			{
				this.Mp.removeMover(this.Mw);
				IN.DestroyOne(this.Mw.gameObject);
				this.Mw = null;
			}
			base.initActiveRemoveKey();
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.Mw != null)
			{
				this.Mp.removeMover(this.Mw);
				IN.DestroyOne(this.Mw.gameObject);
				this.Mw = null;
			}
		}

		public M2BlockColliderContainer.BCCLine GetBcc()
		{
			return this.BccChecker.Get();
		}

		public bool isActive()
		{
			return !base.active_removed && (this.ManageArea == null || PUZ.IT.barrier_active);
		}

		public float sizex
		{
			get
			{
				return (float)this.iwidth * this.cld_mapw_scale * 0.5f * this.Mp.rCLEN;
			}
		}

		public float sizey
		{
			get
			{
				return this.cld_maph * 0.5f;
			}
		}

		public float cld_maph = 2.6f;

		public float cld_mapw_scale = 1.7f;

		private M2SlimeDecoy Mw;

		private M2BCCPosCheckerCp BccChecker;

		private M2LpPuzzManageArea ManageArea;

		public string hit_ptc;

		public float damage_ratio;

		public float hit_t;

		private bool need_fine_color;

		private Color32 CMul = MTRX.ColWhite;

		private Color32 CAdd;

		public class M2CImgDrawerSlimeDecoy : M2CImgDrawer
		{
			public M2CImgDrawerSlimeDecoy(MeshDrawer Md, int _layer, NelChipSlimeDecoy _Cp)
				: base(Md, _layer, _Cp, true)
			{
				this.Dr = _Cp;
			}

			public override int redraw(float fcnt)
			{
				if (this.Dr.hit_t != 0f || this.Dr.need_fine_color)
				{
					this.need_reposit_flag = true;
					float num = base.Mp.floort - X.Abs(this.Dr.hit_t);
					float num2 = ((this.Dr.hit_t == 0f) ? 0f : (1f - X.ZPOWV(num, 45f)));
					float num3 = ((this.Dr.hit_t == 0f) ? 0f : (0.21991149f * X.COSI(num, 11.5f) * num2 * (float)X.MPF(this.Dr.hit_t > 0f)));
					if (base.Length == 4)
					{
						int triMax = this.Md.getTriMax();
						int vertexMax = this.Md.getVertexMax();
						base.revertVerAndTriIndexFirstSaved(false);
						Matrix4x4 currentMatrix = this.Md.getCurrentMatrix();
						float num4 = this.Dr.drawx2meshx((float)this.Cp.drawx);
						float num5 = this.Dr.drawy2meshy((float)this.Cp.drawy);
						float num6 = (float)(-(float)this.Cp.rotation) * 1.5707964f;
						this.Md.TranslateP(0f, (float)this.Cp.Img.iheight * 0.5f, false);
						this.Md.Rotate(num3, false);
						this.Md.TranslateP(0f, (float)(-(float)this.Cp.Img.iheight) * 0.5f, false);
						this.Md.Rotate(num6, false);
						this.Md.TranslateP(num4, num5, false);
						Color32 col = this.Md.Col;
						this.Md.Col = this.Md.ColGrd.Set(base.Lay.LayerColor.C).multiply(this.Dr.CMul, false).Scr(this.Dr.CAdd, 1f)
							.C;
						this.Md.RotaMesh(0f, 0f, 1f, 1f, 0f, this.Cp.Img.getSrcMesh(this.layer), this.Cp.flip, false);
						this.Md.setCurrentMatrix(currentMatrix, false);
						this.Md.Col = col;
						this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
					}
					this.Dr.need_fine_color = false;
					if (num2 <= 0f)
					{
						this.Dr.hit_t = 0f;
					}
				}
				return base.redraw(fcnt);
			}

			private readonly NelChipSlimeDecoy Dr;
		}
	}
}
