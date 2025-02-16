using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2PxlAnimatorRT : M2PxlAnimator
	{
		public M2PxlAnimatorRT initRenderTicket(M2Mover.DRAW_ORDER order)
		{
			this.multiply_global_timescale = false;
			this.MyMd = new MeshDrawer(null, 4, 6);
			if (!X.DEBUGSTABILIZE_DRAW)
			{
				this.RTkt = base.Mp.MovRenderer.assignDrawable(order, null, new M2RenderTicket.FnPrepareMd(this.RenderPrepareMesh), null, this.Mv, null);
				base.gameObject.SetActive(false);
				this.Start();
				this.MyMd.draw_gl_only = true;
			}
			Material material = base.getRendererMaterial();
			if (material == null)
			{
				material = MTRX.MtrMeshNormal;
			}
			this.MyMd.activate(this.Mv.key, material, false, MTRX.ColWhite, null);
			return this;
		}

		public M2Mover.DRAW_ORDER order
		{
			get
			{
				return this.RTkt.order;
			}
			set
			{
				this.RTkt.order = value;
			}
		}

		public override void OnDestroy()
		{
			if (this.RTkt != null)
			{
				this.RTkt = M2DBase.Instance.Cam.MovRender.deassignDrawable(this.RTkt, -1);
			}
			if (this.MyMd != null)
			{
				this.MyMd.destruct();
			}
			base.OnDestroy();
		}

		public override void fineCurrentFrameMeshManual()
		{
			if (this.RTkt == null)
			{
				base.fineCurrentFrameMeshManual();
			}
		}

		public override void poseCanged(string title, PxlPose New, PxlPose Pre)
		{
			base.poseCanged(title, New, Pre);
			if (this.pSq == null)
			{
				return;
			}
			if (this.MyMd != null)
			{
				this.MyMd.setMaterial(base.getRendererMaterial(), false);
			}
			this.need_fine = true;
		}

		protected bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			MdOut = null;
			if (this.Mv.destructed)
			{
				return false;
			}
			if (draw_id == 0 && need_redraw && this.auto_replace_matrix)
			{
				float num = this.Trs.localScale.x * base.Mp.base_scale;
				float num2 = this.Trs.localScale.y * base.Mp.base_scale;
				float num3 = this.Trs.localPosition.x * base.Mp.base_scale + base.offsetPixelX * 0.015625f;
				float num4 = this.Trs.localPosition.y * base.Mp.base_scale + base.offsetPixelY * 0.015625f;
				Tk.Matrix = Matrix4x4.Translate(new Vector3(num3, num4, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotationR / 3.1415927f * 180f)) * Matrix4x4.Scale(new Vector3(num, num2, 1f));
			}
			if (this.need_fine && this.auto_replace_mesh)
			{
				this.need_fine = false;
				this.MyMd.clearSimple();
				PxlFrame currentDrawnFrame = this.getCurrentDrawnFrame();
				this.MyMd.Col = this.ColorForMaterial;
				this.MyMd.Uv23(this.CAdd, false);
				this.MyMd.RotaPF(0f, 0f, 1f, 1f, 0f, currentDrawnFrame, false, false, false, uint.MaxValue, false, 0);
				this.MyMd.allocUv23(0, true);
			}
			if (this.FnReplaceRender != null)
			{
				return this.FnReplaceRender(Cam, Tk, need_redraw, draw_id, out MdOut, ref paste_mesh);
			}
			if (draw_id == 0)
			{
				MdOut = this.MyMd;
				return true;
			}
			return false;
		}

		public Vector2 getMapPosForLayer(PxlLayer L)
		{
			Vector2 vector = this.RTkt.Matrix.MultiplyPoint3x4(new Vector3(L.x * 0.015625f, -L.y * 0.015625f, 0f));
			return new Vector2(base.Mp.uxToMapx(base.Mp.M2D.effectScreenx2ux(vector.x)), base.Mp.uyToMapy(base.Mp.M2D.effectScreeny2uy(vector.y)));
		}

		public MeshDrawer getMainMeshDrawer()
		{
			return this.MyMd;
		}

		public override void setRendererMaterial(Material ReplaceMaterial)
		{
			if (this.RTkt == null)
			{
				base.setRendererMaterial(ReplaceMaterial);
				return;
			}
			this.Mtr = ReplaceMaterial;
			this.AMtr[0] = ReplaceMaterial;
			this.MyMd.setMaterial(this.Mtr, false);
			this.Mtr.mainTexture = base.getCurrentTexture();
		}

		public override Color32 color
		{
			get
			{
				return this.ColorForMaterial;
			}
			set
			{
				this.ColorForMaterial = value;
				this.need_fine = true;
			}
		}

		public override float alpha
		{
			get
			{
				return (float)this.ColorForMaterial.a / 255f;
			}
			set
			{
				this.ColorForMaterial.a = (byte)X.MMX(0, (int)(value * 255f), 255);
				this.need_fine = true;
			}
		}

		public override void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.color = CMul.C;
			this.CAdd = CAdd.C;
			this.need_fine = true;
		}

		public M2RenderTicket.FnPrepareMd FnReplaceRender;

		public M2RenderTicket RTkt;

		private MeshDrawer MyMd;

		public float rotationR;

		public Color32 CAdd = MTRX.ColTrnsp;

		public bool auto_replace_matrix = true;
	}
}
