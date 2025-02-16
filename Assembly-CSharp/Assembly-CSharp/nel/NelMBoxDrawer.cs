using System;
using UnityEngine;
using XX;

namespace nel
{
	public class NelMBoxDrawer
	{
		public NelMBoxDrawer(MBoxDrawer _Dr)
		{
			this.Dr = _Dr ?? MTR.DrMBox;
		}

		public NelMBoxDrawer prepareMesh(MeshDrawer Md, int _mid_base, int _mid_pic, MeshRenderer Mrd, int stencil_ref = -1)
		{
			this.mid_pic = _mid_pic;
			Md.chooseSubMesh(_mid_pic, false, false);
			Md.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, stencil_ref), false);
			this.mid_base = _mid_base;
			Md.chooseSubMesh(_mid_base, false, false);
			Md.setMaterial(MTRX.getMtr(BLEND.NORMAL, stencil_ref), false);
			if (Mrd != null)
			{
				Md.connectRendererToTriMulti(Mrd);
			}
			return this;
		}

		public void drawTo(MeshDrawer Md, float x, float y, Matrix4x4 Mx, float open_lvl = 0f, float alpha = 1f)
		{
			Md.chooseSubMesh(this.mid_base, false, false);
			Md.Col = C32.MulA(this.col_inner_light, alpha);
			Md.ColGrd.Set(this.col_inner_dark).mulA(alpha);
			this.Dr.drawTo(Md, 0f, 0f, this.wh_px, Mx, open_lvl, true, true, 3);
			for (int i = 1; i <= 2; i++)
			{
				Md.chooseSubMesh(this.mid_base, false, false);
				Md.Col = C32.MulA(this.col_base_light, alpha);
				Md.ColGrd.Set(this.col_base_dark).mulA(alpha);
				this.Dr.drawTo(Md, 0f, 0f, this.wh_px, Mx, open_lvl, true, false, i);
				Md.chooseSubMesh(this.mid_pic, false, false);
				Md.Col = C32.MulA(this.col_pic_light, alpha);
				Md.ColGrd.Set(this.col_pic_dark).mulA(alpha);
				this.Dr.drawTo(Md, 0f, 0f, this.wh_px, Mx, open_lvl, true, false, i);
			}
		}

		public void drawTo(MeshDrawer MdBase, MeshDrawer MdPic, float x, float y, Matrix4x4 Mx, float open_lvl = 0f)
		{
			MdBase.Col = C32.d2c(this.col_inner_light);
			MdBase.ColGrd.Set(this.col_inner_dark);
			this.Dr.drawTo(MdBase, 0f, 0f, this.wh_px, Mx, open_lvl, true, true, 3);
			MdBase.Col = C32.d2c(this.col_base_light);
			MdBase.ColGrd.Set(this.col_base_dark);
			MdPic.Col = C32.d2c(this.col_pic_light);
			MdPic.ColGrd.Set(this.col_pic_dark);
			for (int i = 1; i <= 2; i++)
			{
				this.Dr.drawTo(MdBase, 0f, 0f, this.wh_px, Mx, open_lvl, true, false, i);
				this.Dr.drawTo(MdPic, 0f, 0f, this.wh_px, Mx, open_lvl, true, false, i);
			}
		}

		public void setFillImageBlock(FillImageBlock Fi, MeshDrawer.FnGeneralDraw FnDraw = null)
		{
			MeshRenderer meshRenderer = Fi.getMeshRenderer();
			MeshDrawer meshDrawer = Fi.getMeshDrawer();
			this.prepareMesh(meshDrawer, 0, 1, meshRenderer, Fi.stencil_ref);
			if (FnDraw != null)
			{
				Fi.FnDraw = FnDraw;
			}
			else
			{
				Fi.FnDraw = new MeshDrawer.FnGeneralDraw(this.fnGeneralDraw);
			}
			Fi.redraw_flag = true;
		}

		public bool fnGeneralDraw(MeshDrawer Md, float alpha)
		{
			this.fnGeneralDraw(Md, alpha, NelMBoxDrawer.BasicMx, true);
			return true;
		}

		public bool fnGeneralDraw(MeshDrawer Md, float alpha, Matrix4x4 Mx, bool update_mesh_renderer)
		{
			this.drawTo(Md, 0f, 0f, Mx, 0f, alpha);
			if (update_mesh_renderer)
			{
				Md.updateForMeshRenderer(false);
			}
			return true;
		}

		public MBoxDrawer Dr;

		public int mid_base;

		public int mid_pic = 1;

		public uint col_inner_light = 4289239703U;

		public uint col_inner_dark = 4285686881U;

		public uint col_base_light = 4293342003U;

		public uint col_base_dark = 4287565583U;

		public uint col_pic_light = 4294438550U;

		public uint col_pic_dark = 4290162809U;

		public static Matrix4x4 BasicMx = X.RotMxZXY360(-10f, 0f, -25f);

		public float wh_px = 30f;
	}
}
