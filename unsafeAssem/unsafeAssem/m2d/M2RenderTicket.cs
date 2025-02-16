using System;
using UnityEngine;
using UnityEngine.Rendering;
using XX;

namespace m2d
{
	public class M2RenderTicket
	{
		public M2RenderTicket(M2Mover.DRAW_ORDER order, Transform _Trs, M2RenderTicket.FnPrepareMd _FnMd, MeshDrawer _MdMain = null, M2Mover _AssignMover = null, IPosLitener _MapPosLsn = null)
		{
			this.order_ = order;
			this.Trs = _Trs;
			this.MdMain = _MdMain;
			this.AssignMover = _AssignMover;
			IPosLitener posLitener;
			if (_MapPosLsn != null)
			{
				posLitener = _MapPosLsn;
			}
			else
			{
				IPosLitener assignMover = this.AssignMover;
				posLitener = assignMover;
			}
			this.MapPosLsn = posLitener;
			this.FnMd = _FnMd;
			this.initGlRendering();
			CommandBuffer cb = M2RenderTicket.Cb;
		}

		public bool clipCheck(Map2d Mp, bool recheck = false)
		{
			if (recheck)
			{
				float num;
				float num2;
				if (this.AssignMover == null)
				{
					this.camera_in = true;
				}
				else if (this.MapPosLsn.getPosition(out num, out num2))
				{
					float num3 = this.draw_margin + this.AssignMover.sizex;
					float num4 = this.draw_margin + this.AssignMover.sizey;
					this.camera_in = Mp.isinCamera(num - num3, num2 - num4, num3 * 2f, num4 * 2f, 0f);
				}
				else
				{
					this.camera_in = false;
				}
			}
			return this.camera_in;
		}

		public void initGlRendering()
		{
			if (this.Trs != null)
			{
				this.Trs.gameObject.SetActive(false);
			}
			if (this.MdMain != null)
			{
				this.MdMain.draw_gl_only = true;
				this.MdMain.use_cache = false;
			}
		}

		public void releaseFromGlRendering()
		{
			if (this.Trs != null)
			{
				this.Trs.gameObject.SetActive(true);
			}
			if (this.MdMain != null)
			{
				this.MdMain.draw_gl_only = false;
				this.MdMain.use_cache = true;
			}
		}

		public bool Draw(ProjectionContainer JCon, Camera Cam, bool redraw, int draw_id, ref Material DMtr)
		{
			bool flag = false;
			if (redraw && this.Trs != null)
			{
				this.Matrix = this.Trs.localToWorldMatrix;
			}
			if (this.FnMd != null)
			{
				bool flag2 = false;
				MeshDrawer meshDrawer;
				flag = this.FnMd(Cam, this, redraw, draw_id, out meshDrawer, ref flag2);
				if (flag && meshDrawer != null)
				{
					int draw_triangle_count = meshDrawer.draw_triangle_count;
					if (draw_triangle_count == 0)
					{
						return true;
					}
					Material material = meshDrawer.getMaterial();
					if (material != DMtr)
					{
						DMtr = material;
						DMtr.SetPass(0);
					}
					GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * this.Matrix);
					if (flag2)
					{
						BLIT.RenderToGLImmediate001(meshDrawer, meshDrawer.getVertexArray(), meshDrawer.getUvArray(), null, draw_triangle_count, -1, false, true, null);
					}
					else
					{
						BLIT.RenderToGLImmediate001(meshDrawer, draw_triangle_count, -1, false, true, null);
					}
				}
			}
			return flag;
		}

		public M2Mover.DRAW_ORDER order
		{
			get
			{
				return this.order_;
			}
			set
			{
				if (value == this.order_)
				{
					return;
				}
				M2DBase.Instance.Cam.MovRender.changeOrder(this, ref this.order_, value);
			}
		}

		private M2Mover.DRAW_ORDER order_;

		public Transform Trs;

		public Matrix4x4 Matrix = Matrix4x4.identity;

		public MeshDrawer MdMain;

		public M2RenderTicket.FnPrepareMd FnMd;

		public M2Mover AssignMover;

		public IPosLitener MapPosLsn;

		private static CommandBuffer Cb;

		public float draw_margin = 3f;

		public bool camera_in;

		public delegate bool FnPrepareMd(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite);
	}
}
