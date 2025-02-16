using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class MokubaDrawer
	{
		public MokubaDrawer()
		{
			this.Md = new MeshDrawer(null, 4, 6);
			this.Md.draw_gl_only = true;
		}

		public bool draw_gl_only
		{
			get
			{
				return this.Md.draw_gl_only;
			}
			set
			{
				this.Md.draw_gl_only = value;
			}
		}

		public MokubaDrawer setImage(PxlImage _PIBone, PxlImage _PISide, PxlImage _PIFront, PxlImage _PIOther)
		{
			this.PIBone = _PIBone;
			this.PISide = _PISide;
			this.PIFront = _PIFront;
			this.PIOther = _PIOther;
			this.Md.activate("", MTRX.getMI(this.PIBone).getMtr(BLEND.NORMAL, -1), false, MTRX.ColWhite, null);
			return this;
		}

		public virtual void draw(MeshDrawer MdTarget, float x, float y, Matrix4x4 Mx, float hop_pixel_y = 0f)
		{
			if (this.PIBone == null)
			{
				return;
			}
			if (this.Md.getMaterial() == null)
			{
				this.Md.activate("", MTRX.getMI(this.PIBone).getMtr(BLEND.NORMAL, -1), false, MTRX.ColWhite, null);
			}
			if (this.Md.draw_triangle_count == 0)
			{
				this.recreate();
			}
			Matrix4x4 currentMatrix = MdTarget.getCurrentMatrix();
			MdTarget.setCurrentMatrix(Matrix4x4.Translate(new Vector3(x, y, 0f) * 0.015625f) * Mx * Matrix4x4.Scale(new Vector3(this.draw_w_scale, 1f, 1f)), false);
			MdTarget.RotaMesh(0f, 0f, 1f, 1f, 0f, this.Md.getVertexArray(), this.Md.getColorArray(), this.Md.getUvArray(), null, null, this.Md.getTriangleArray(), false, 0, this.Md.getTriMax(), 0, this.Md.getVertexMax());
			this.Md.setCurrentMatrix(currentMatrix, false);
		}

		public uint draw_bits
		{
			get
			{
				return this.draw_bits_;
			}
			set
			{
				if (this.draw_bits == value)
				{
					return;
				}
				this.draw_bits_ = value;
				this.Md.clearSimple();
			}
		}

		public void recreate()
		{
			this.Md.clear(false, false);
			this.Md.Col = MTRX.ColWhite;
			float num = (float)this.PIFront.width * 0.015625f;
			float num2 = (float)this.PIFront.height * 0.015625f;
			float num3 = num * 0.5f;
			float num4 = num2 * 0.5f;
			float num5 = (float)this.PISide.width * 0.015625f;
			float num6 = num5 * 0.5f;
			float num7 = this.board_px * 0.015625f;
			float num8 = this.bheight_px * 0.015625f;
			float num9 = num3 - num7 - num8;
			if ((this.draw_bits_ & 4U) != 0U)
			{
				if (num9 > 0f)
				{
					this.Md.Tri(2, 9, 3, false).Tri(3, 9, 8, false).Tri(5, 11, 10, false)
						.Tri(5, 4, 11, false);
				}
				this.Rect(this.PIOther, -num3, -num4, num6, num7, 0f, 0f, 0f, 0f, -num5);
				this.Rect(null, num3 - num7, -num4, num6, num7, 0f, 0f, 0f, 0f, -num5);
				if (num9 > 0f)
				{
					this.Md.Col = new Color32(70, 70, 70, byte.MaxValue);
					this.Rect(null, -num9, -num4 + num8, num6, num9 * 2f, 0f, 0f, 0f, 0f, -num5);
				}
			}
			this.Md.Col = MTRX.ColWhite;
			if ((this.draw_bits_ & 2U) != 0U)
			{
				this.Rect(this.PISide, num3, -num4, -num6, 0f, 0f, num5, -num3, num2, 0f);
				this.Rect(null, -num3, -num4, num6, 0f, 0f, -num5, num3, num2, 0f);
			}
			if ((this.draw_bits_ & 1U) != 0U)
			{
				this.Triangle(this.PIFront, -num3, -num4, -num6, num, 0f, 0f, 0f, num2, 0f);
				this.Triangle(this.PIFront, num3, -num4, num6, -num, 0f, 0f, 0f, num2, 0f);
			}
		}

		private MokubaDrawer Rect(PxlImage PI, float x0, float y0, float z0, float vx_x, float vx_y, float vx_z, float vy_x, float vy_y, float vy_z)
		{
			if (PI != null)
			{
				this.Md.initForImg(PI, 0);
			}
			this.Md.TriRectBL(0);
			this.Md.base_z = z0;
			this.Md.Pos(x0, y0, null);
			this.Md.base_z = z0 + vy_z;
			this.Md.Pos(x0 + vy_x, y0 + vy_y, null);
			this.Md.base_z = z0 + vy_z + vx_z;
			this.Md.Pos(x0 + vx_x + vy_x, y0 + vx_y + vy_y, null);
			this.Md.base_z = z0 + vx_z;
			this.Md.Pos(x0 + vx_x, y0 + vx_y, null);
			this.Md.InputImageUv();
			return this;
		}

		private MokubaDrawer Triangle(PxlImage PI, float x0, float y0, float z0, float vx_x, float vx_y, float vx_z, float vy_x, float vy_y, float vy_z)
		{
			this.Md.Tri012();
			Rect rectIUv = PI.RectIUv;
			this.Md.uvRectN(rectIUv.x, rectIUv.y);
			this.Md.base_z = z0;
			this.Md.Pos(x0, y0, null);
			this.Md.base_z = z0 + vy_z + vx_z * 0.5f;
			this.Md.uvRectN(rectIUv.x + rectIUv.width * 0.5f, rectIUv.y + rectIUv.height);
			this.Md.Pos(x0 + vx_x * 0.5f + vy_x, y0 + vx_y * 0.5f + vy_y, null);
			this.Md.base_z = z0 + vx_z;
			this.Md.uvRectN(rectIUv.x + rectIUv.width, rectIUv.y);
			this.Md.Pos(x0 + vx_x, y0 + vx_y, null);
			this.Md.InputImageUv();
			return this;
		}

		public PxlImage PIBone;

		public PxlImage PISide;

		public PxlImage PIFront;

		public PxlImage PIOther;

		public MeshDrawer Md;

		public float z_px;

		public float board_px = 4f;

		public float bheight_px = 23f;

		public const int DRAW_FRONT = 1;

		public const int DRAW_SIDE = 2;

		public const int DRAW_UNDER = 4;

		public const int DRAW_ALL = 7;

		private uint draw_bits_ = 7U;

		public float draw_w_scale = 1f;
	}
}
