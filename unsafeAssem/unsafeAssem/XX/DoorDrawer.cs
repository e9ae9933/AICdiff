using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class DoorDrawer
	{
		public DoorDrawer SetByPixel(float _w, float _h, float _zw, float _handle_w, float _handle_rail_h, float _handle_h, float _handle_rail_w = 0f, float _handle_zw = -1f)
		{
			this.w = _w * 0.015625f;
			this.h = _h * 0.015625f;
			this.zw = _zw * 0.015625f;
			this.handle_w = _handle_w * 0.015625f;
			this.handle_rail_h = _handle_rail_h * 0.015625f;
			this.handle_h = _handle_h * 0.015625f;
			this.need_reentry = true;
			this.handle_rail_w = ((_handle_rail_w == 0f) ? this.handle_w : (_handle_rail_w * 0.015625f));
			this.handle_zw = ((_handle_zw < 0f) ? this.handle_w : (_handle_zw * 0.015625f));
			return this;
		}

		public DoorDrawer HandlePosPixel(float hx, float hy)
		{
			this.handle_x = hx * 0.015625f;
			this.handle_y = hy * 0.015625f;
			this.tyoutugai_l = hx > 0f;
			this.need_reentry = true;
			return this;
		}

		public PxlMeshDrawer getMesh(bool flip = false)
		{
			if (this.need_reentry || this.PMesh == null)
			{
				this.RedrawMesh(flip);
			}
			return this.PMesh;
		}

		public DoorDrawer drawTo(MeshDrawer Md, float x, float y, float open_level, float scalex = 1f, float scaley = 1f, float agR = 0f, bool flip = false)
		{
			PxlMeshDrawer mesh = this.getMesh(scalex * scaley < 0f);
			Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(x, y, 0f) * 0.015625f, Quaternion.Euler(0f, -open_level * (float)X.MPF(this.tyoutugai_l != this.open_to_behind) * 90f, 0f), Vector3.one);
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.setCurrentMatrix(matrix4x, false);
			Md.RotaMesh(0f, 0f, scalex, scaley, agR, mesh, flip, true);
			Md.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		public PxlMeshDrawer RedrawMesh(bool flip = false)
		{
			this.need_reentry = false;
			if (DoorDrawer.Md == null)
			{
				DoorDrawer.Md = new MeshDrawer(null, 4, 6);
			}
			DoorDrawer.Md.draw_gl_only = true;
			DoorDrawer.Md.activate("", MTRX.MtrMeshNormal, true, MTRX.ColWhite, null);
			float num = this.w * 0.5f * (float)X.MPF(this.tyoutugai_l);
			bool flag = this.handle_h < this.handle_rail_w * 2.25f;
			float num2 = this.handle_x + this.handle_w * 0.5f - this.handle_rail_w * 0.5f;
			if (this.handle_w > 0f && !this.do_not_draw_behind_handle && this.HandleImg.width > 0f)
			{
				this.InitImg(this.HandleImg);
				DoorDrawer.Md.Col = this.HandleCol;
				this.Kakoi(1, num + this.handle_x, this.handle_y, this.zw + this.handle_rail_h + this.handle_zw * 0.5f, this.handle_w, this.handle_h, this.handle_zw);
				if (this.handle_rail_h > 0f)
				{
					this.InitImg(this.HandleRailImg);
					DoorDrawer.Md.Col = this.HandleRailCol;
					float num3 = this.zw + this.handle_rail_h * 0.5f;
					for (int i = (flag ? 1 : 0); i >= 0; i--)
					{
						float num4 = (flag ? this.handle_y : (this.handle_y + (this.handle_h - this.handle_w) * 0.38f * (float)X.MPF(i > 0)));
						this.Men(1, num + num2, num4, num3, this.handle_rail_h, this.handle_rail_w, this.handle_rail_h);
						this.Men(3, num + num2, num4, num3, this.handle_rail_h, this.handle_rail_w, this.handle_rail_h);
					}
				}
			}
			if (this.DoorSideImg.width > 0f)
			{
				this.InitImg(this.DoorSideImg);
				DoorDrawer.Md.Col = this.DoorSideCol;
				if (this.zw > 0f)
				{
					this.Men(1, num, 0f, this.zw * 0.5f, this.zw, this.h, this.w);
					this.Men(3, num, 0f, this.zw * 0.5f, this.zw, this.h, this.w);
				}
			}
			if (this.FrontImg.width > 0f)
			{
				this.InitImg(this.FrontImg);
				DoorDrawer.Md.Col = this.FrontCol;
				this.Men(0, num, 0f, this.zw * 0.5f, this.w, this.h, this.zw);
				if (this.FnDrawFront != null)
				{
					this.FnDrawFront(DoorDrawer.Md, this, num * 64f);
				}
			}
			if (this.handle_w > 0f)
			{
				if (this.handle_rail_h > 0f && this.HandleRailImg.width > 0f)
				{
					float num5 = -this.handle_rail_h * 0.5f;
					this.InitImg(this.HandleRailImg);
					DoorDrawer.Md.Col = this.HandleRailCol;
					for (int j = (flag ? 1 : 0); j >= 0; j--)
					{
						float num6 = (flag ? this.handle_y : (this.handle_y + (this.handle_h - this.handle_w) * 0.38f * (float)X.MPF(j > 0)));
						this.Men(1, num + num2, num6, num5, this.handle_rail_h, this.handle_rail_w, this.handle_rail_h);
						this.Men(3, num + num2, num6, num5, this.handle_rail_h, this.handle_rail_w, this.handle_rail_h);
					}
				}
				if (this.HandleImg.width > 0f)
				{
					this.InitImg(this.HandleImg);
					DoorDrawer.Md.Col = this.HandleCol;
					this.Kakoi(1, num + this.handle_x, this.handle_y, -this.handle_rail_h - this.handle_zw * 0.5f, this.handle_w, this.handle_h, this.handle_zw);
				}
			}
			DoorDrawer.Md.updateForMeshRenderer(false);
			this.PMesh = DoorDrawer.Md.createSimplePxlMesh(null, true, true, false);
			return this.PMesh;
		}

		private void InitImg(Rect Rc)
		{
			if (Rc.width > 0f && this.TargetMI != null)
			{
				DoorDrawer.Md.initForImg(this.TargetMI.Tx, Rc.x, Rc.y, Rc.width, Rc.height);
			}
		}

		private DoorDrawer Men(int id, float cx, float cy, float cz, float w, float h, float zw)
		{
			Matrix4x4 matrix4x = Matrix4x4.identity;
			switch (id)
			{
			case 1:
				matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, -90f, 0f));
				DoorDrawer.Md.TriRectBL(0);
				break;
			case 2:
				matrix4x = Matrix4x4.Rotate(Quaternion.Euler(-90f, 0f, 0f));
				DoorDrawer.Md.TriRectBL(0);
				break;
			case 3:
				matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 90f, 0f));
				DoorDrawer.Md.TriRectBL(0);
				break;
			case 4:
				matrix4x = Matrix4x4.Rotate(Quaternion.Euler(90f, 0f, 0f));
				DoorDrawer.Md.TriRectBL(0);
				break;
			case 5:
				matrix4x = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
				DoorDrawer.Md.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
				break;
			default:
				matrix4x = Matrix4x4.identity;
				DoorDrawer.Md.TriRectBL(0);
				break;
			}
			matrix4x = Matrix4x4.Translate(new Vector3(cx, cy, cz)) * matrix4x;
			w *= 0.5f;
			h *= 0.5f;
			zw *= 0.5f;
			Vector3 vector = new Vector3(-w, -h, -zw);
			vector = matrix4x.MultiplyPoint3x4(vector);
			DoorDrawer.Md.base_z = vector.z;
			DoorDrawer.Md.Pos(vector.x, vector.y, null);
			vector.Set(-w, h, -zw);
			vector = matrix4x.MultiplyPoint3x4(vector);
			DoorDrawer.Md.base_z = vector.z;
			DoorDrawer.Md.Pos(vector.x, vector.y, null);
			vector.Set(w, h, -zw);
			vector = matrix4x.MultiplyPoint3x4(vector);
			DoorDrawer.Md.base_z = vector.z;
			DoorDrawer.Md.Pos(vector.x, vector.y, null);
			vector.Set(w, -h, -zw);
			vector = matrix4x.MultiplyPoint3x4(vector);
			DoorDrawer.Md.base_z = vector.z;
			DoorDrawer.Md.Pos(vector.x, vector.y, null);
			DoorDrawer.Md.InputImageUv();
			return this;
		}

		private DoorDrawer Kakoi(int id, float cx, float cy, float cz, float w, float h, float zw)
		{
			this.Men(5, cx, cy, cz, w, h, zw);
			this.Men(1, cx, cy, cz, zw, h, zw);
			this.Men(3, cx, cy, cz, zw, h, zw);
			this.Men(0, cx, cy, cz, w, h, zw);
			return this;
		}

		public float get_swidth_px()
		{
			return this.w * 64f;
		}

		public float get_sheight_px()
		{
			return this.h * 64f;
		}

		public bool isTyoutsugaiLeft()
		{
			return this.tyoutugai_l;
		}

		private PxlMeshDrawer PMesh;

		public bool need_reentry = true;

		public bool open_to_behind;

		public bool do_not_draw_behind_handle;

		private float w;

		private float h;

		private float zw;

		private float handle_w;

		private float handle_zw;

		private float handle_rail_h;

		private float handle_rail_w;

		private float handle_h;

		private float handle_x;

		private float handle_y;

		private bool tyoutugai_l;

		public MImage TargetMI;

		public Rect FrontImg;

		public Color32 FrontCol = MTRX.ColWhite;

		public Rect DoorSideImg;

		public Color32 DoorSideCol = MTRX.ColWhite;

		public Rect HandleRailImg;

		public Color32 HandleRailCol = MTRX.ColWhite;

		public Rect HandleImg;

		public Color32 HandleCol = MTRX.ColWhite;

		public Action<MeshDrawer, DoorDrawer, float> FnDrawFront;

		private static MeshDrawer Md;
	}
}
