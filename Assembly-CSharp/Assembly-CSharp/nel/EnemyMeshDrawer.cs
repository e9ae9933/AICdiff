using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyMeshDrawer : ITeColor, ITeScaler, IAnimListener
	{
		public EnemyMeshDrawer(Map2d _Mp)
		{
			this.initS(_Mp);
			this.TeScale = new Vector2(1f, 1f);
			this.CMul = new C32(this.def_mul_color);
			this.CAdd = new C32(0f, 0f, 0f, 0f);
		}

		public void initS(Map2d _Mp)
		{
			this.Mp = _Mp;
		}

		public EnemyMeshDrawer prepareMesh(MImage _MI, TransEffecter _TeCon = null)
		{
			this.Md = new MeshDrawer(null, 4, 6);
			NelM2DBase nelM2DBase = this.Mp.M2D as NelM2DBase;
			this.MI = _MI;
			bool flag;
			if (this.is_evil)
			{
				this.MtrBase = this.MI.getMtr(out flag, MTR.ShaderEnemyDark, -1);
				nelM2DBase.addEnemyDarkTexture(this.MtrBase);
			}
			else
			{
				this.MtrBase = (M2DBase.Instance as NelM2DBase).getWithLightTextureMaterial(this.MI);
			}
			this.MtrAddBody = this.MI.getMtr(out flag, MTR.ShaderEnemyDarkWhiter, -1);
			if (flag)
			{
				MTRX.setMaterialST(this.MtrAddBody, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			}
			this.MtrBorder = this.MI.getMtr(out flag, MTR.ShaderEnemyAulaAdd, -1);
			if (flag)
			{
				CameraComponentCollecter moverCameraCC = nelM2DBase.Cam.getMoverCameraCC();
				if (moverCameraCC != null)
				{
					this.MtrBorder.SetTexture("_MoverTex", moverCameraCC.Cam.targetTexture);
					this.MtrBorder.SetFloat("_ScreenMargin", 8f);
				}
			}
			this.MtrBuffer = this.MI.getMtr(out flag, MTR.ShaderEnemyBuffer, -1);
			if (_TeCon != null)
			{
				this.TeCon = _TeCon;
			}
			if (this.TeCon != null)
			{
				this.TeCon.RegisterCol(this, false);
				this.TeCon.RegisterScl(this);
			}
			this.recalcTransformMatrix();
			return this;
		}

		public void setBorderMaskEnable(bool flag)
		{
			this.Mp.Dgn.setBorderMask(ref this.MtrBase, this.MI, flag, null);
		}

		public float draw_margin
		{
			get
			{
				return this.draw_margin_;
			}
			set
			{
				if (this.draw_margin == value)
				{
					return;
				}
				this.draw_margin_ = value;
				if (this.Rtk != null)
				{
					this.Rtk.draw_margin = value;
				}
				if (this.RtkBuf != null)
				{
					this.RtkBuf.draw_margin = value;
				}
			}
		}

		public void prepareRendetTicket(M2Mover Mv, IPosLitener PosLsn, M2PxlAnimatorRT BaseAnm = null)
		{
			if (!this.Mp.M2D.Cam.stabilize_drawing)
			{
				this.Md.draw_gl_only = true;
				if (BaseAnm == null)
				{
					this.Rtk = this.Mp.MovRenderer.assignDrawable(this.current_order, null, new M2RenderTicket.FnPrepareMd(this.FnEnRenderBase), null, Mv, PosLsn);
					this.Rtk.draw_margin = this.draw_margin_;
				}
				else
				{
					BaseAnm.auto_replace_mesh = false;
					BaseAnm.FnReplaceRender = new M2RenderTicket.FnPrepareMd(this.FnEnRenderBase);
				}
				this.RtkBuf = this.Mp.MovRenderer.assignDrawable(M2Mover.DRAW_ORDER.BUF_1, null, new M2RenderTicket.FnPrepareMd(this.FnEnRenderBuffer), null, Mv, PosLsn);
				this.RtkBuf.draw_margin = this.draw_margin_;
			}
			this.MtrForSimple = this.MI.getMtr(BLEND.NORMAL, -1);
			this.MdEyeV = new MeshDrawer(null, 4, 6);
			this.MdEye = new MeshDrawer(null, 4, 6);
			this.MdAdd = new MeshDrawer(null, 4, 6);
			if (this.RtkBuf != null)
			{
				this.Md.draw_gl_only = true;
				this.MdEyeV.draw_gl_only = true;
				this.MdEye.draw_gl_only = true;
				this.MdAdd.draw_gl_only = true;
			}
			else
			{
				this.DbEye = this.Mp.setED("enemyeye", new M2DrawBinder.FnEffectBind(this.fnDrawEyeOnEffect), 0f);
			}
			this.Md.activate();
			this.MdEyeV.activate("eye", MTRX.getMtr(BLEND.ADDZT, -1), false, MTRX.ColWhite, null);
			this.MdEye.activate("eyev", this.MI.getMtr(BLEND.ADDZT, -1), false, MTRX.ColWhite, null);
			this.MdAdd.activate("addef", this.MtrAddBody, false, MTRX.ColWhite, null);
		}

		public virtual void destruct()
		{
			this.Rtk = this.Mp.MovRenderer.deassignDrawable(this.Rtk, -1);
			this.RtkBuf = this.Mp.MovRenderer.deassignDrawable(this.RtkBuf, -1);
			this.DbEye = this.Mp.remED(this.DbEye);
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			if (this.MdEye != null)
			{
				this.MdEye.destruct();
			}
			if (this.MdEyeV != null)
			{
				this.MdEyeV.destruct();
			}
			if (this.MdAdd != null)
			{
				this.MdAdd.destruct();
			}
			this.MtrAddBody = null;
			this.Md = null;
			this.MdEye = (this.MdEyeV = null);
			this.MdAdd = null;
			this.destructed = true;
		}

		public virtual void showToFront(bool val, bool force = false)
		{
			if (!force && this.is_front == val)
			{
				return;
			}
			this.is_front = val;
			if (this.Rtk != null)
			{
				this.Rtk.order = this.current_order;
			}
		}

		public virtual bool checkFrame()
		{
			return this.checkFrame(Map2d.TS * this.D_AF, false);
		}

		public float D_AF
		{
			get
			{
				return (float)(this.checkframe_on_drawing ? X.AF : 1);
			}
		}

		public virtual bool updateAnimator(float f)
		{
			if (this.destructed)
			{
				return false;
			}
			this.checkFrame(f, false);
			return true;
		}

		public virtual bool checkFrame(float TS, bool force = false)
		{
			if (force)
			{
				this.need_fine = true;
			}
			if (this.rotationR_speed != 0f)
			{
				this.rotationR_ += this.rotationR_speed * TS;
				this.need_fine = true;
			}
			return true;
		}

		public Matrix4x4 getAfterMultipleMatrix(bool ignore_rot = false)
		{
			Vector3 transformScale = this.getTransformScale();
			float num = transformScale.x * this.Mp.base_scale * this.TeScale.x;
			float num2 = transformScale.y * this.Mp.base_scale * this.TeScale.y;
			return this.getAfterMultipleMatrix(num, num2, ignore_rot);
		}

		public virtual Matrix4x4 getAfterMultipleMatrix(float scalex, float scaley, bool ignore_rot = false)
		{
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(this.after_offset_x_, this.after_offset_y_) * 0.015625f) * Matrix4x4.Translate(new Vector3(0f, 0f, this.rot_center_y_u));
			if (!ignore_rot)
			{
				matrix4x *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotationR_ / 3.1415927f * 180f));
			}
			matrix4x *= Matrix4x4.Translate(new Vector3(0f, 0f, -this.rot_center_y_u));
			return matrix4x * Matrix4x4.Scale(new Vector3(scalex, scaley, 1f));
		}

		protected virtual void clearBaseMd()
		{
			this.Md.clearSimple();
		}

		protected virtual void redrawBodyMeshInner()
		{
			this.need_fine_mesh = false;
		}

		protected virtual bool fnDrawEyeOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			MeshDrawer mesh = Ef.GetMesh(this.is_front ? "front" : "back", this.MI.getMtr(BLEND.ADD, -1), true);
			MeshDrawer mesh2 = Ef.GetMesh(this.is_front ? "front" : "back", MTRX.getMtr(BLEND.ADD, -1), true);
			mesh.base_x = (mesh.base_y = (mesh2.base_x = (mesh2.base_y = 0f)));
			mesh.base_z = (mesh2.base_z = this.efmesh_base_z);
			MeshDrawer meshDrawer = null;
			if (this.CAdd.rgb > 0U)
			{
				meshDrawer = Ef.GetMesh(this.is_front ? "front" : "back", this.MtrAddBody, true);
				meshDrawer.base_x = (meshDrawer.base_y = 0f);
				meshDrawer.base_z = this.efmesh_base_z;
			}
			this.fnDrawEyeInner(mesh, mesh2, meshDrawer);
			return true;
		}

		protected virtual void fnDrawEyeInner(MeshDrawer MdEye, MeshDrawer MdEyeV, MeshDrawer MdAdd)
		{
			this.need_fine_eye_mesh = false;
		}

		public virtual Vector3 getTransformScale()
		{
			return Vector3.one;
		}

		public virtual Vector2 getTransformPosition()
		{
			return Vector2.zero;
		}

		protected Matrix4x4 getTransformMatrix(bool ignore_scale = false)
		{
			Matrix4x4 matrix4x = this.MxTransformBuf;
			if (!ignore_scale)
			{
				Vector3 transformScale = this.getTransformScale();
				float num = transformScale.x * this.Mp.base_scale * this.TeScale.x;
				float num2 = transformScale.y * this.Mp.base_scale * this.TeScale.y;
				matrix4x *= Matrix4x4.Scale(new Vector3(num, num2, 1f));
			}
			return matrix4x;
		}

		public void recalcTransformMatrix()
		{
			Vector3 vector = this.getTransformPosition();
			float num = vector.x * this.Mp.base_scale;
			float num2 = vector.y * this.Mp.base_scale;
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(num, num2, 0f));
			this.MxTransformBuf = matrix4x;
		}

		private void EnRenderBaseBody(M2RenderTicket Tk, bool need_redraw)
		{
			uint ran = X.GETRAN2((int)(this.Mp.floort / 4f) + this.index, this.index % 8);
			this.recalcTransformMatrix();
			this.BaseMatrix = this.getTransformMatrix(false);
			if (this.scale_shuffle01 != 0f)
			{
				this.BaseMatrix *= Matrix4x4.Scale(new Vector3(1f + this.scale_shuffle01 * X.RAN(ran, 2963), 1f + this.scale_shuffle01 * X.RAN(ran, 2504), 1f));
			}
			if (this.base_rotate_shuffle360 != 0f)
			{
				this.BaseMatrix *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (-1f + X.RAN(ran, 1070) * 2f) * this.base_rotate_shuffle360));
			}
			this.RtkBuf.Matrix = this.BaseMatrix;
			this.pre_base_rendered = true;
			if (need_redraw)
			{
				if (this.need_fine_mesh)
				{
					this.clearBaseMd();
					this.redrawBodyMeshInner();
				}
				if (this.need_fine_eye_mesh)
				{
					this.MdEye.clearSimple();
					this.MdEyeV.clearSimple();
					this.MdAdd.clearSimple();
					this.fnDrawEyeInner(this.MdEye, this.MdEyeV, (this.CAdd.rgb > 0U) ? this.MdAdd : null);
				}
			}
		}

		protected void BasicColorInit(MeshDrawer Md)
		{
			float num = (float)this.CAdd.a / 255f;
			num = X.Scr(num, num);
			Md.Col = Md.ColGrd.Set(4294905358U).multiply(this.CMul.C, false).blend(4294905358U, 1f - (float)this.CMul.a / 255f * 0.5f)
				.Scr(this.CAdd, num)
				.mulA(this.alpha_)
				.C;
		}

		protected virtual void initMdAddUv2(int margin = 4, int seed = 0)
		{
			this.MdAdd.Col = this.CAdd.C;
			this.MdAdd.Col.a = (byte)((float)this.CAdd.a * this.alpha_);
			this.MdAdd.setCurrentMatrix(this.getAfterMultipleMatrix(false), false);
			this.MdAdd.allocUv2(margin, false).Uv2(X.ANMP((int)this.Mp.floort + seed * 23, 260, 1f), 1f - X.ANMP((int)this.Mp.floort + seed * 73, 293, 1f), true);
		}

		public virtual bool noNeedDraw()
		{
			return this.alpha_ == 0f;
		}

		protected bool FnEnRenderBuffer(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			return this.FnEnRenderBufferInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh);
		}

		protected virtual bool FnEnRenderBufferInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			MdOut = null;
			if (draw_id != 0)
			{
				draw_id--;
				return false;
			}
			if (this.noNeedDraw())
			{
				return false;
			}
			if (this.checkframe_on_drawing)
			{
				this.checkFrame();
			}
			this.EnRenderBaseBody(Tk, need_redraw);
			this.Md.setMaterial(this.MtrBuffer, false);
			MdOut = this.Md;
			return true;
		}

		private bool FnEnRenderBase(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (this.noNeedDraw())
			{
				MdOut = null;
				return false;
			}
			return this.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh);
		}

		protected virtual bool FnEnRenderBaseInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			MdOut = null;
			switch (draw_id)
			{
			case 0:
				return true;
			case 1:
			{
				if (!this.is_evil)
				{
					return true;
				}
				uint ran = X.GETRAN2((int)(this.Mp.floort / 4f) + this.index, this.index % 8);
				Vector3 transformScale = this.getTransformScale();
				Tk.Matrix = Matrix4x4.Translate(new Vector3((-2f + 4f * X.RAN(ran, 2354)) * 0.015625f / transformScale.x, (-2f + 4f * X.RAN(ran, 2748)) * 0.015625f / transformScale.y, 0f)) * this.BaseMatrix * Matrix4x4.Scale(new Vector3(1f + 0.03f * X.RAN(ran, 2963), 1f + 0.03f * X.RAN(ran, 2504), 1f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (-1f + X.RAN(ran, 1070) * 2f) * this.base_rotate_shuffle360));
				this.Md.setMaterial(this.MtrBorder, false);
				MdOut = this.Md;
				return true;
			}
			case 2:
				Tk.Matrix = this.BaseMatrix;
				if (Cam == null)
				{
					this.Md.setMaterial(this.MtrForSimple, false);
				}
				else
				{
					this.Md.setMaterial(this.MtrBase, false);
				}
				MdOut = this.Md;
				return true;
			case 3:
				Tk.Matrix = Matrix4x4.identity;
				MdOut = this.MdEyeV;
				return true;
			case 4:
				MdOut = this.MdEye;
				return true;
			case 5:
				if (this.CAdd.rgb > 0U)
				{
					Tk.Matrix = this.BaseMatrix;
					MdOut = this.MdAdd;
				}
				return true;
			default:
				draw_id -= 6;
				return false;
			}
		}

		public TransEffecterItem setDmgBlink(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.TeCon == null)
			{
				return null;
			}
			return this.TeCon.setDmgBlinkEnemy(attr, maxt, X.Pow(factor * 0.8f, 2), factor, _saf);
		}

		public TransEffecterItem setDmgBlinkFading(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.TeCon == null)
			{
				return null;
			}
			return this.TeCon.setDmgBlinkFading(attr, maxt, X.Pow(factor * 0.8f, 2), factor, _saf);
		}

		public void setAbsorbBlink(AbsorbManager Absorb)
		{
			if (Absorb == null)
			{
				return;
			}
			M2Attackable targetMover = Absorb.getTargetMover();
			if (targetMover == null)
			{
				return;
			}
			this.setAbsorbBlink(targetMover.drawx, targetMover.drawy);
		}

		public virtual void setAbsorbBlink(float map_pixel_x, float map_pixel_y)
		{
			this.AbsorbCenterPos.Set(map_pixel_x * this.Mp.base_scale * 0.015625f, -map_pixel_y * this.Mp.base_scale * 0.015625f, this.Mp.floort);
		}

		public Color32 getColorTe()
		{
			return MTRX.ColGray;
		}

		public void setColorTe(C32 Buf, C32 _CMul, C32 _CAdd)
		{
			this.need_fine = (this.need_fine_eye_mesh = true);
			this.CMul.Set(this.def_mul_color).multiply(_CMul.C, true);
			this.CAdd.Set(0f, 0f, 0f, 0f).add(_CAdd.C, true, 1f);
		}

		public Vector2 getScaleTe()
		{
			return this.TeScale;
		}

		public C32 getTeMulColor()
		{
			return this.CMul;
		}

		public void setScaleTe(Vector2 V)
		{
			this.TeScale = V;
			this.need_fine = true;
		}

		public Texture2D getMainTexture()
		{
			return this.MtrBase.mainTexture as Texture2D;
		}

		public Material getMainMtr()
		{
			return this.MtrBase;
		}

		public float after_offset_x
		{
			get
			{
				return this.after_offset_x_;
			}
			set
			{
				this.after_offset_x_ = value;
				this.need_fine = true;
			}
		}

		public float after_offset_y
		{
			get
			{
				return this.after_offset_y_;
			}
			set
			{
				this.after_offset_y_ = value;
				this.need_fine = true;
			}
		}

		public float rotationR
		{
			get
			{
				return this.rotationR_;
			}
			set
			{
				if (value != this.rotationR_)
				{
					this.rotationR_ = value;
					this.need_fine = true;
				}
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public virtual bool need_fine
		{
			get
			{
				return this.need_fine_;
			}
			set
			{
				if (value)
				{
					this.need_fine_ = value;
				}
			}
		}

		public virtual float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (value != this.alpha_)
				{
					this.alpha_ = value;
				}
			}
		}

		public virtual bool is_evil
		{
			get
			{
				return true;
			}
		}

		protected float efmesh_base_z
		{
			get
			{
				return this.transform_z - 0.0005f;
			}
		}

		protected float transform_z
		{
			get
			{
				return 300f - (float)X.MPF(this.is_front) * 0.001f;
			}
		}

		public Matrix4x4 drawMatrix
		{
			get
			{
				return this.BaseMatrix;
			}
		}

		public M2Mover.DRAW_ORDER current_order
		{
			get
			{
				if (!this.is_front)
				{
					return this.order_back_;
				}
				return this.order_front_;
			}
		}

		public M2Mover.DRAW_ORDER order_front
		{
			get
			{
				return this.order_front_;
			}
			set
			{
				if (this.order_front == value)
				{
					return;
				}
				this.order_front_ = value;
				if (this.Rtk != null)
				{
					this.Rtk.order = this.current_order;
				}
			}
		}

		public M2Mover.DRAW_ORDER order_back
		{
			get
			{
				return this.order_back_;
			}
			set
			{
				if (this.order_back == value)
				{
					return;
				}
				this.order_back_ = value;
				if (this.Rtk != null)
				{
					this.Rtk.order = this.current_order;
				}
			}
		}

		protected Map2d Mp;

		protected MeshDrawer Md;

		protected int index;

		protected MImage MI;

		protected M2DrawBinder DbEye;

		protected Material MtrBase;

		protected Material MtrAddBody;

		protected Material MtrBorder;

		protected Material MtrBuffer;

		protected Material MtrForSimple;

		protected MeshDrawer MdEye;

		protected MeshDrawer MdEyeV;

		protected MeshDrawer MdAdd;

		protected C32 CMul;

		protected C32 CAdd;

		protected Vector2 TeScale = Vector2.one;

		public uint def_mul_color = 4293055186U;

		protected Vector3 AbsorbCenterPos;

		protected bool is_front;

		protected M2RenderTicket Rtk;

		protected M2RenderTicket RtkBuf;

		protected Matrix4x4 BufShuffleMx = Matrix4x4.identity;

		protected Matrix4x4 BaseMatrix = Matrix4x4.identity;

		protected float alpha_ = 1f;

		protected bool need_fine_;

		public bool need_fine_mesh;

		protected bool need_fine_eye_mesh;

		protected float after_offset_x_;

		protected float after_offset_y_;

		public float rot_center_y_u;

		public bool checkframe_on_drawing = true;

		protected float rotationR_;

		public float rotationR_speed;

		public bool destructed;

		public float base_rotate_shuffle360 = 2f;

		public float scale_shuffle01 = 0.03f;

		protected TransEffecter TeCon;

		public static float add_color_white_blend_level = 0.5f;

		public uint add_color_eye_fade_out = 4294901760U;

		private M2Mover.DRAW_ORDER order_back_ = M2Mover.DRAW_ORDER.N_BACK1;

		private M2Mover.DRAW_ORDER order_front_ = M2Mover.DRAW_ORDER.N_TOP1;

		private Matrix4x4 MxTransformBuf = Matrix4x4.identity;

		public const int DRENDER_DID_BORDER = 1;

		public const int DRENDER_DID_MAIN = 2;

		public const int DRENDER_DID_EYEV = 3;

		private float draw_margin_ = 3f;

		private bool pre_base_rendered;
	}
}
