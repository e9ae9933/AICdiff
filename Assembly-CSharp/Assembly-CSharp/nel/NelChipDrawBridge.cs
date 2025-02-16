using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipDrawBridge : NelChip
	{
		public NelChipDrawBridge(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.ConLp = null;
			this.APuts = null;
			this.Acord_st = null;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (!normal_map)
			{
				return;
			}
			if (this.APuts == null || this.ConLp == null)
			{
				X.de("コンテナとなる M2LpDrawBridgeContainer および対象チップリストが設定されていない", null);
				return;
			}
			this.redrawBridge();
			if (!this.ConLp.activate_flag)
			{
				this.setChipConfigActivation(false);
			}
		}

		public void initCord(M2LpDrawBridgeContainer _ConLp, List<M2Puts> _APuts)
		{
			this.ConLp = _ConLp;
			this.APuts = _APuts;
			float[] nmA = this.Img.Meta.GetNmA("drawbridge");
			int num = nmA.Length;
			this.Acord_st = new Vector2[X.IntC((float)(num / 2))];
			this.Acord_en = new Vector2[this.Acord_st.Length];
			Vector2 vector = new Vector2(this.mapcx, this.mbottom);
			for (int i = 0; i < num; i += 2)
			{
				Vector2 vector2 = base.PixelToMapPoint(nmA[i], nmA[i + 1]);
				this.Acord_st[i / 2] = vector2 - vector;
			}
			M2Puts m2Puts = null;
			M2Puts m2Puts2 = null;
			for (int j = this.APuts.Count - 1; j >= 0; j--)
			{
				M2Puts m2Puts3 = this.APuts[j];
				if (m2Puts3 is NelChipDrawBridgePiece)
				{
					(m2Puts3 as NelChipDrawBridgePiece).CpRoot = this;
				}
				if (m2Puts2 == null || (this.flip ? (m2Puts2.drawx > m2Puts3.drawx) : (m2Puts2.drawx + m2Puts2.iwidth < m2Puts3.drawx + m2Puts3.iwidth)))
				{
					m2Puts2 = m2Puts3;
				}
				if (m2Puts == null || (this.flip ? (m2Puts.drawx + m2Puts.iwidth < m2Puts3.drawx + m2Puts3.iwidth) : (m2Puts.drawx > m2Puts3.drawx)))
				{
					m2Puts = m2Puts3;
				}
			}
			this.Piece_MapCen = new Vector2(this.flip ? m2Puts.mright : m2Puts.mleft, m2Puts.mapcy);
			for (int k = 0; k < num; k += 2)
			{
				Vector2 vector3 = new Vector2((float)m2Puts2.drawx + ((!this.flip) ? ((float)m2Puts2.iwidth - nmA[k + 1]) : nmA[k + 1]), (float)m2Puts2.drawy + m2Puts2.Img.Meta.GetNm("drawbridge_piece", (float)m2Puts2.iheight * 0.5f, 0)) * base.rCLEN;
				this.Acord_en[k / 2] = vector3 - this.Piece_MapCen;
			}
			this.rot_agR = (this.ConLp.activate_flag ? 0f : 1.4451327f) * (float)X.MPF(!this.flip);
			this.code_col = C32.d2c(X.NmUI(base.Meta.GetS("code_col"), 4278190080U, true, true));
		}

		public void setChipConfigActivation(bool flag)
		{
			bool flag2 = false;
			for (int i = this.APuts.Count - 1; i >= 0; i--)
			{
				if (this.APuts[i] is NelChipDrawBridgePiece)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				this.Mp.considerConfig4(this.ConLp);
				this.Mp.need_update_collider = true;
			}
		}

		public bool redrawBridge()
		{
			if (this.ConLp == null)
			{
				return false;
			}
			float num = (this.ConLp.activate_flag ? 0f : 1.2566371f) * (float)X.MPF(!this.flip);
			this.rot_agR = X.VALWALK(this.rot_agR, num, 0.006283186f);
			int num2 = base.redraw(0f);
			for (int i = this.APuts.Count - 1; i >= 0; i--)
			{
				M2Puts m2Puts = this.APuts[i];
				num2 |= m2Puts.redraw(0f);
			}
			this.Mp.addUpdateMesh(num2, false);
			if (X.Abs(this.rot_agR - num) > 0.0006283185f)
			{
				return true;
			}
			if (this.ConLp.activate_flag)
			{
				this.setChipConfigActivation(true);
			}
			return false;
		}

		public bool isConfigActive()
		{
			return this.ConLp == null || this.ConLp.activate_flag;
		}

		private M2LpDrawBridgeContainer ConLp;

		private List<M2Puts> APuts;

		public Vector2[] Acord_st;

		public Vector2[] Acord_en;

		public Vector2 Piece_MapCen;

		public Color32 code_col;

		public float rot_agR;

		public const float root_rotate_ratio = 0.85f;

		public class M2CImgDrawerDrawBridge : M2CImgDrawer
		{
			public M2CImgDrawerDrawBridge(MeshDrawer Md, int _lay, M2Puts _Cp)
				: base(Md, _lay, _Cp, false)
			{
				this.CpRoot = this.Cp as NelChipDrawBridge;
				this.use_shift = false;
				if (this.Cp is NelChipDrawBridgePiece)
				{
					this.CpRoot = (this.Cp as NelChipDrawBridgePiece).CpRoot;
				}
			}

			public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
			{
				if (this.CpRoot == null || this.CpRoot.ConLp == null)
				{
					return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
				}
				base.Set(false);
				Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, Ms, false, false);
				if (this.CpRoot != null && this.CpRoot == this.Cp && this.CpRoot.Acord_st != null)
				{
					this.ver_cord_st = Md.getVertexMax();
					this.tri_cord_st = Md.getTriMax();
					this.drawString(0f);
					this.drawStringFoc(0f);
				}
				else
				{
					this.ver_cord_st = Md.getVertexMax();
				}
				base.Set(false);
				base.initMdArray();
				if (this.Cp.active_removed)
				{
					base.repositActiveRemoveFlag();
				}
				return this.redraw_flag;
			}

			private void drawString(float rot_agR)
			{
				Color32 col = this.Md.Col;
				this.Md.Col = this.CpRoot.code_col;
				float num = base.Mp.map2meshx(this.CpRoot.mapcx);
				float num2 = base.Mp.map2meshy(this.CpRoot.mbottom);
				float num3 = base.Mp.map2meshx(this.CpRoot.Piece_MapCen.x);
				float num4 = base.Mp.map2meshy(this.CpRoot.Piece_MapCen.y);
				base.M2D.IMGS.Atlas.initForRectWhite(this.Md);
				for (int i = this.CpRoot.Acord_st.Length - 1; i >= 0; i--)
				{
					Vector2 vector = this.CpRoot.Acord_st[i];
					vector.x *= base.CLEN;
					vector.y *= -base.CLEN;
					vector = X.ROTV2e(vector, rot_agR * 0.85f);
					vector.x = num + vector.x;
					vector.y = num2 + vector.y;
					Vector2 vector2 = this.CpRoot.Acord_en[i];
					vector2.x *= base.CLEN;
					vector2.y *= -base.CLEN;
					vector2 = X.ROTV2e(vector2, rot_agR);
					vector2.x = num3 + vector2.x;
					vector2.y = num4 + vector2.y;
					this.Md.Line(vector.x, vector.y, vector2.x, vector2.y, 1.5f, false, 0f, 0f);
				}
				this.Md.Col = col;
			}

			private void drawStringFoc(float rot_agR)
			{
				Color32 col = this.Md.Col;
				this.Md.Col = this.CpRoot.code_col;
				float num = base.Mp.map2meshx(this.CpRoot.mapcx);
				float num2 = base.Mp.map2meshy(this.CpRoot.mbottom);
				Vector2 vector = new Vector2(base.Mp.map2meshx(this.CpRoot.ConLp.mapfocx), base.Mp.map2meshy(this.CpRoot.ConLp.mapfocy));
				base.M2D.IMGS.Atlas.initForRectWhite(this.Md);
				for (int i = this.CpRoot.Acord_st.Length - 1; i >= 0; i--)
				{
					Vector2 vector2 = this.CpRoot.Acord_st[i];
					vector2.x *= base.CLEN;
					vector2.y *= -base.CLEN;
					vector2 = X.ROTV2e(vector2, rot_agR);
					vector2.x = num + vector2.x;
					vector2.y = num2 + vector2.y;
					this.Md.Line(vector2.x, vector2.y, vector.x, vector.y, 1.5f, false, 0f, 0f);
				}
				this.Md.Col = col;
			}

			public override int redraw(float fcnt)
			{
				if (this.APos == null)
				{
					return base.redraw(fcnt);
				}
				Vector3[] vertexArray = this.Md.getVertexArray();
				int startVerIndex = base.getStartVerIndex();
				base.getEndVerIndex();
				float num;
				float num2;
				float num3;
				if (this.CpRoot == this.Cp)
				{
					num = this.CpRoot.rot_agR * 0.85f;
					num2 = base.Mp.map2meshx(this.CpRoot.mapcx);
					num3 = base.Mp.map2meshy(this.CpRoot.mbottom);
				}
				else
				{
					num = this.CpRoot.rot_agR;
					num2 = base.Mp.map2meshx(this.CpRoot.Piece_MapCen.x);
					num3 = base.Mp.map2meshy(this.CpRoot.Piece_MapCen.y);
				}
				for (int i = startVerIndex; i < this.ver_cord_st; i++)
				{
					Vector3 vector = this.APos[i - startVerIndex];
					vector.x -= num2;
					vector.y -= num3;
					vector = X.ROTV2e(vector, num);
					vector.x += num2;
					vector.y += num3;
					vertexArray[i] = vector * 0.015625f;
				}
				if (this.tri_cord_st >= 0)
				{
					int vertexMax = this.Md.getVertexMax();
					int triMax = this.Md.getTriMax();
					this.Md.revertVerAndTriIndex(this.ver_cord_st, this.tri_cord_st, false);
					this.drawString(this.CpRoot.rot_agR);
					this.drawStringFoc(num);
					this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
				}
				return base.layer2update_flag;
			}

			public NelChipDrawBridge CpRoot;

			private int ver_cord_st = -1;

			private int tri_cord_st = -1;
		}
	}
}
