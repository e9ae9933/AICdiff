using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyAnimatorMultiMech : EnemyAnimator
	{
		public EnemyAnimatorMultiMech(NelEnemy _Mv)
			: base(_Mv, (EnemyAnimator Anm, PxlFrame F) => new EnemyFrameDataBasic(Anm, F), null)
		{
			this.normalrender_header = null;
			this.AMdCur = new List<MeshDrawer>(4);
			this.AMdMech = new List<MeshDrawer>(2);
			this.AMdDark = new List<MeshDrawer>(2);
		}

		public EnemyAnimatorMultiMech addNormalRendHeader(string s)
		{
			if (this.normalrender_header == null)
			{
				this.normalrender_header = s;
			}
			else
			{
				this.Anormalrender_header.Add(s);
			}
			return this;
		}

		protected override void redrawBodyMeshInner()
		{
			this.AMdCur.Clear();
			this.r_mech_cur = (this.r_dark_cur = 0);
			base.redrawBodyMeshInner();
		}

		protected override void redrawBodyMeshInnerAfter(Matrix4x4 MxAfterMultiple, PxlFrame curF = null, EnemyFrameDataBasic CurFrmData = null)
		{
			base.redrawBodyMeshInnerAfter(MxAfterMultiple, curF, CurFrmData);
			this.Md.Identity();
			for (int i = 0; i < (int)this.r_dark_cur; i++)
			{
				MeshDrawer meshDrawer = this.AMdDark[i];
				Vector2 vector = this.Md.getUv2Array(false)[0];
				meshDrawer.Uv2(vector.x, vector.y, false).allocUv2(0, true);
				vector = this.Md.getUv3Array(false)[0];
				meshDrawer.Uv3(vector.x, vector.y, false).allocUv3(0, true);
				this.Md.RotaTempMeshDrawer(0f, 0f, 1f, 1f, 0f, meshDrawer, false, false, 0, -1, 0, -1);
			}
		}

		protected override bool getTargetMeshDrawer(EnemyFrameDataBasic CurFrmData, int i, PxlLayer L, out bool extend, out MeshDrawer _Md)
		{
			uint num = this.layer_mask & (1U << i);
			if ((CurFrmData.normal_render_layer & num) == 0U && (CurFrmData.dark_bits & num) == 0U)
			{
				return base.getTargetMeshDrawer(CurFrmData, i, L, out extend, out _Md);
			}
			bool flag = !CurFrmData.isDark(i);
			_Md = this.PopNextMd(flag);
			extend = !flag;
			return true;
		}

		public MeshDrawer PopNextMd(bool is_mech)
		{
			int count = this.AMdCur.Count;
			MeshDrawer meshDrawer;
			if (count == 0 || is_mech != (this.AMdCur[count - 1].activation_key == "mech"))
			{
				if (is_mech)
				{
					meshDrawer = this.PopNextMd(this.AMdMech, ref this.r_mech_cur, true);
				}
				else
				{
					meshDrawer = this.PopNextMd(this.AMdDark, ref this.r_dark_cur, false);
				}
			}
			else
			{
				meshDrawer = this.AMdCur[count - 1];
			}
			meshDrawer.Col = ((!is_mech) ? this.Md.Col : this.CMul.C);
			return meshDrawer.Identity();
		}

		private MeshDrawer PopNextMd(List<MeshDrawer> AMd, ref byte r_cur, bool is_mech)
		{
			Material material;
			if (is_mech && this.r_dark_cur == 0 && r_cur == 0)
			{
				if (this.MtrMechFirst == null)
				{
					this.MtrMechFirst = this.Mp.Dgn.getWithLightTextureMaterial(this.MI, null, -50);
					this.MtrMechFirst.EnableKeyword("USEMASK");
					this.MtrMechFirst.SetTexture("_MaskTex", this.Mp.M2D.Cam.getMoverTexture());
				}
				material = this.MtrMechFirst;
			}
			else
			{
				material = (is_mech ? this.Mp.Dgn.getWithLightTextureMaterial(this.MI, null, -1) : this.MtrBase);
			}
			MeshDrawer meshDrawer;
			if (AMd.Count <= (int)r_cur)
			{
				meshDrawer = new MeshDrawer(null, 4, 6);
				meshDrawer.draw_gl_only = true;
				AMd.Add(meshDrawer);
				r_cur += 1;
			}
			else
			{
				byte b = r_cur;
				r_cur = b + 1;
				meshDrawer = AMd[(int)b];
			}
			meshDrawer.activate(is_mech ? "mech" : "dark", material, false, MTRX.ColWhite, null);
			this.AMdCur.Add(meshDrawer);
			return meshDrawer;
		}

		protected override bool FnEnRenderBaseInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (draw_id <= 1)
			{
				return base.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh);
			}
			draw_id -= 2;
			if (draw_id < this.AMdCur.Count)
			{
				MdOut = this.AMdCur[draw_id];
				Tk.Matrix = ((MdOut.activation_key == "mech") ? base.getTransformMatrix(false) : this.BaseMatrix);
				return true;
			}
			draw_id += -this.AMdCur.Count + 3;
			return base.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh);
		}

		public override bool is_normal_render(PxlLayer L)
		{
			if (base.is_normal_render(L))
			{
				return true;
			}
			for (int i = this.Anormalrender_header.Count - 1; i >= 0; i--)
			{
				if (TX.isStart(L.name, this.Anormalrender_header[i], 0))
				{
					return true;
				}
			}
			return false;
		}

		public override bool is_evil
		{
			get
			{
				return true;
			}
		}

		private List<MeshDrawer> AMdCur;

		private List<MeshDrawer> AMdMech;

		private List<MeshDrawer> AMdDark;

		private byte r_mech_cur;

		private byte r_dark_cur;

		private Material MtrMechFirst;

		private List<string> Anormalrender_header = new List<string>();
	}
}
