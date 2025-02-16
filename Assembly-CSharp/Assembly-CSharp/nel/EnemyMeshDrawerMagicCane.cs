using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyMeshDrawerMagicCane : EnemyMeshDrawer
	{
		public EnemyMeshDrawerMagicCane(NelEnemy _En, int square_capacity)
			: base(_En.Mp)
		{
			this.En = _En;
			EnemyAttr.initAnimator(_En, this);
			this.AADancePos = new List<List<Vector3>>(7);
			this.MdSubSquare = new MeshDrawer(null, 4, 6);
			this.MdSubSquare.draw_gl_only = true;
			this.MdSubSquare.activate("MdSubSquare", MTRX.getMtr(BLEND.SUB, -1), false, MTRX.ColWhite, null);
			this.ASquarePre = new List<Vector3>(square_capacity);
			if (EnemyMeshDrawerMagicCane.ASquareBuf == null)
			{
				EnemyMeshDrawerMagicCane.ASquareBuf = new List<Vector3>(square_capacity);
			}
		}

		public override void destruct()
		{
			base.destruct();
			this.MdSubSquare.destruct();
		}

		public override bool checkFrame(float TS, bool force = false)
		{
			base.checkFrame(TS, force);
			this.dance_t += TS;
			if (this.dance_t >= 2f)
			{
				this.dance_t = 0f;
				List<Vector3> list = null;
				if (this.AADancePos.Count > 0)
				{
					list = this.AADancePos[this.AADancePos.Count - 1];
				}
				List<Vector3> list2;
				if (this.AADancePos.Count > 7)
				{
					list2 = this.AADancePos[0];
					this.AADancePos.RemoveRange(0, this.AADancePos.Count - 7);
				}
				else
				{
					list2 = new List<Vector3>(this.dance_object_count);
				}
				if (list2.Capacity < this.dance_object_count)
				{
					list2.Capacity = this.dance_object_count;
				}
				list2.Clear();
				Vector2 transformPosition = this.getTransformPosition();
				Matrix4x4 matrix4x = base.getAfterMultipleMatrix(false);
				matrix4x = Matrix4x4.Translate(transformPosition * this.Mp.base_scale) * matrix4x;
				for (int i = 0; i < this.dance_object_count; i++)
				{
					list2.Add(this.getDanceObjectPos(i, list, matrix4x));
				}
				this.AADancePos.Add(list2);
				this.need_fine_eye_mesh = true;
			}
			return true;
		}

		private Vector3 getDanceObjectPos(int i, List<Vector3> ALPre, Matrix4x4 MxAfterMultiple)
		{
			uint ran = X.GETRAN2(this.En.index + i * 7, i);
			float num = 70f + X.RAN(ran, 623) * 90f;
			float num2 = num + 20f + X.RAN(ran, 2596) * 40f;
			if (X.RAN(ran, 2386) < 0.5f)
			{
				float num3 = num;
				num = num2;
				num2 = num3;
			}
			Vector3 vector = new Vector3((this.dance_xrange_px - 30f + 50f * X.RAN(ran, 1250)) * X.COSI(this.Mp.floort + 40f + X.RAN(ran, 2578) * 120f, num), (this.dance_yrange_px - 24f + 48f * X.RAN(ran, 2960)) * X.SINI(this.Mp.floort + 50f + X.RAN(ran, 797) * 120f, num2));
			vector *= this.cane_scale;
			if (this.CaneL_ != null)
			{
				vector.x += this.CaneL_.x;
				vector.y -= this.CaneL_.y;
			}
			vector = MxAfterMultiple.MultiplyPoint3x4(vector * 0.015625f);
			if (ALPre != null && i < ALPre.Count)
			{
				Vector3 vector2 = ALPre[i];
				vector.x = X.NI(vector2.x, vector.x, 0.12f);
				vector.y = X.NI(vector2.y, vector.y, 0.12f);
			}
			vector.z = this.Mp.floort;
			return vector;
		}

		protected override void fnDrawEyeInner(MeshDrawer MdEye, MeshDrawer MdEyeV, MeshDrawer MdAdd)
		{
			this.need_fine_eye_mesh = false;
			MeshDrawer mdEyeV = this.MdEyeV;
			mdEyeV.clear(false, false);
			mdEyeV.base_x = (mdEyeV.base_y = 0f);
			int count = this.AADancePos.Count;
			for (int i = 0; i < count; i++)
			{
				List<Vector3> list = this.AADancePos[i];
				mdEyeV.Col = mdEyeV.ColGrd.Set(this.CMul.C).blend(16711680U, X.ZLINE((float)(this.AADancePos.Count - i), 7f)).C;
				int count2 = list.Count;
				for (int j = 0; j < count2; j++)
				{
					Vector3 vector = list[j];
					uint ran = X.GETRAN2(this.En.index + j * 13, 4);
					float num = 6.2831855f * X.NI(0.013f, 0.003f, X.RAN(ran, 657));
					mdEyeV.Poly(vector.x, vector.y, 0.3125f, num * vector.z, 4, 0.03125f, true, 0f, 0f);
				}
			}
		}

		protected MeshDrawer drawDancingSquare(MeshDrawer Md)
		{
			Matrix4x4 matrix4x = base.getAfterMultipleMatrix(false);
			matrix4x = Matrix4x4.Translate(this.getTransformPosition() * this.Mp.base_scale) * matrix4x;
			Md.clear(false, false);
			Md.Col = C32.d2c(4281356126U);
			if (EnemyMeshDrawerMagicCane.ASquareBuf.Capacity < this.dance_object_count)
			{
				EnemyMeshDrawerMagicCane.ASquareBuf.Capacity = this.dance_object_count;
			}
			EnemyMeshDrawerMagicCane.ASquareBuf.Clear();
			for (int i = 0; i < this.dance_object_count; i++)
			{
				Vector3 danceObjectPos = this.getDanceObjectPos(i, this.ASquarePre, matrix4x);
				uint ran = X.GETRAN2(this.En.index + i * 13, 4);
				float num = 6.2831855f * X.NI(0.013f, 0.003f, X.RAN(ran, 657));
				Md.Poly(danceObjectPos.x, danceObjectPos.y, 0.25f, -num * this.Mp.floort, 4, 0f, true, 0f, 0f);
				Md.Poly(danceObjectPos.x, danceObjectPos.y, 0.2890625f, -num * this.Mp.floort, 4, 0.015625f, true, 0f, 0f);
				EnemyMeshDrawerMagicCane.ASquareBuf.Add(danceObjectPos);
			}
			this.ASquarePre.Clear();
			this.ASquarePre.AddRange(EnemyMeshDrawerMagicCane.ASquareBuf);
			return Md;
		}

		protected override bool FnEnRenderBaseInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite)
		{
			MdOut = null;
			int num = draw_id;
			if (num == 2)
			{
				Tk.Matrix = Matrix4x4.identity;
				MdOut = this.MdSubSquare;
				return true;
			}
			if (num == 3)
			{
				MdOut = this.MdEyeV;
				return true;
			}
			if (draw_id > 3)
			{
				draw_id -= 3;
				return false;
			}
			return base.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref color_one_overwrite);
		}

		protected override void redrawBodyMeshInner()
		{
			this.need_fine_mesh = false;
			this.Md.clear(false, false);
			if (this.CaneL_ != null)
			{
				Matrix4x4 afterMultipleMatrix = base.getAfterMultipleMatrix(false);
				base.BasicColorInit(this.Md);
				PxlLayer caneL_ = this.CaneL_;
				PxlImage img = caneL_.Img;
				this.Md.Scale(caneL_.zmx * this.cane_scale, caneL_.zmy * this.cane_scale, false);
				this.Md.Rotate(-caneL_.rotR, false);
				float num = -((float)(img.width + 2) * 0.5f);
				float num2 = -((float)(img.height + 2) * 0.5f);
				this.Md.TranslateP(caneL_.x, -caneL_.y, false);
				this.Md.setCurrentMatrix(afterMultipleMatrix, true);
				this.Md.initForImg(img, 0);
				this.Md.RectBL(num, num2, (float)(img.width + 2), (float)(img.height + 2), false);
			}
			this.drawDancingSquare(this.MdSubSquare);
		}

		public override Vector3 getTransformScale()
		{
			return this.En.Anm.getTransformScale();
		}

		public override Vector2 getTransformPosition()
		{
			return this.En.Anm.getTransformPosition();
		}

		public PxlLayer CaneL
		{
			get
			{
				return this.CaneL_;
			}
			set
			{
				if (this.CaneL_ != value)
				{
					this.CaneL_ = value;
					this.need_fine_mesh = true;
				}
			}
		}

		public override bool noNeedDraw()
		{
			return this.En.disappearing || !this.Mp.M2D.Cam.isCoveringMp(this.En.x - this.En.sizex - 1.5f, this.En.y - this.En.sizey - 1.5f, this.En.x + this.En.sizex + 1.5f, this.En.y + this.En.sizey + 1.5f, 0f) || base.noNeedDraw();
		}

		private NelEnemy En;

		private List<List<Vector3>> AADancePos;

		private MeshDrawer MdSubSquare;

		private const int dance_mem = 7;

		public const float dance_fine_intv = 2f;

		public float dance_t;

		public int dance_object_count;

		public float dance_xrange_px = 72f;

		public float dance_yrange_px = 45f;

		private Transform TrsAnm;

		private PxlLayer CaneL_;

		public float cane_scale = 1f;

		private List<Vector3> ASquarePre;

		private static List<Vector3> ASquareBuf;
	}
}
