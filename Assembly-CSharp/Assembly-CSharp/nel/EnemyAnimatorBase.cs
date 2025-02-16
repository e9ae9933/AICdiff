using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyAnimatorBase : EnemyMeshDrawer
	{
		public EnemyAnimatorBase(NelEnemy _Mv)
			: base(_Mv.Mp)
		{
			this.Mv = _Mv;
		}

		public override Vector3 getTransformScale()
		{
			return this.Mv.getAnimator().transform.localScale;
		}

		public override Vector2 getTransformPosition()
		{
			Vector2 vector = this.Mv.getAnimator().transform.localPosition;
			if (this.NestTarget != null)
			{
				vector += this.NestTarget.getUShift(this.Mp) / this.Mp.base_scale;
			}
			return vector;
		}

		public override bool noNeedDraw()
		{
			return this.Mv.disappearing || base.noNeedDraw();
		}

		public override bool is_evil
		{
			get
			{
				return this.Mv.is_evil;
			}
		}

		protected override void initMdAddUv2(int margin = 4, int seed = 0)
		{
			base.initMdAddUv2(margin, this.Mv.index);
		}

		protected void getTextureDataForDraw(out float scale_rx, out float scale_ry, out Texture2D Tex, out float texture_width_r, out float texture_height_r, out Matrix4x4 MxAfterMultiple)
		{
			scale_rx = this.Mv.sizex / this.Mv.sizex0;
			scale_ry = this.Mv.sizey / this.Mv.sizey0;
			Tex = this.Mv.getAnimator().getMainTexture();
			texture_width_r = 1f / (float)Tex.width;
			texture_height_r = 1f / (float)Tex.height;
			MxAfterMultiple = this.getAfterMultipleMatrix(this.TeScale.x, this.TeScale.y, false);
		}

		protected void redrawBodyMeshInner(PxlFrame curF, EnemyFrameDataBasic CurFrmData)
		{
			this.need_fine_mesh = false;
			int num = curF.countLayers();
			float num2;
			float num3;
			Texture2D texture2D;
			float num4;
			float num5;
			Matrix4x4 matrix4x;
			this.getTextureDataForDraw(out num2, out num3, out texture2D, out num4, out num5, out matrix4x);
			base.BasicColorInit(this.Md);
			if (this.is_evil)
			{
				this.Md.Uv2(num2, num3, false).Uv3((float)this.CMul.r / 255f, (float)this.CMul.g / 255f, false);
			}
			else
			{
				this.Md.Col = this.CMul.C;
			}
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = curF.getLayer(i);
				bool flag;
				MeshDrawer meshDrawer;
				if (this.getTargetMeshDrawer(CurFrmData, i, layer, out flag, out meshDrawer))
				{
					this.drawLayerTo(meshDrawer, layer, texture2D, num4, num5, matrix4x, flag);
				}
			}
			this.redrawBodyMeshInnerAfter(matrix4x, curF, CurFrmData);
			if (this.is_evil)
			{
				this.Md.allocUv2(0, true);
				this.Md.allocUv3(0, true);
			}
		}

		protected virtual bool getTargetMeshDrawer(EnemyFrameDataBasic CurFrmData, int i, PxlLayer L, out bool extend, out MeshDrawer Md)
		{
			bool flag = (this.layer_mask & (1U << i)) != 0U;
			extend = true;
			Md = this.Md;
			return flag && !CurFrmData.isEye(i) && L.alpha > 0f && this.CMul.a > 0;
		}

		protected void drawLayerTo(MeshDrawer Md, PxlLayer L, Texture2D Tex, float texture_width_r, float texture_height_r, Matrix4x4 MxAfterMultiple, bool extend = true)
		{
			PxlImage img = L.Img;
			Md.Identity();
			Rect rectIUv = img.RectIUv;
			float num = 0f;
			if (extend)
			{
				num = 2f;
				rectIUv.x -= texture_width_r;
				rectIUv.y -= texture_height_r;
				rectIUv.width += texture_width_r * 2f;
				rectIUv.height += texture_height_r * 2f;
			}
			Md.initForImg(Tex, rectIUv, false);
			float num2;
			float num3;
			if (X.Abs(L.zmx) == 1f && L.zmy == 1f && L.rotR == 0f)
			{
				Md.Scale(L.zmx, L.zmy, false);
				num2 = (float)(-(float)((L.zmx == 1f) ? ((int)(((float)img.width + num) / 2f)) : X.IntC(((float)img.width + num) * 0.5f)));
				num3 = (float)(-(float)X.IntC(((float)img.height + num) * 0.5f));
			}
			else
			{
				Md.Scale(L.zmx, L.zmy, false);
				Md.Rotate(-L.rotR, false);
				num2 = -(((float)img.width + num) * 0.5f);
				num3 = -(((float)img.height + num) * 0.5f);
			}
			Md.TranslateP(L.x, -L.y, false);
			Md.setCurrentMatrix(MxAfterMultiple, true);
			Md.RectBL(num2, num3, (float)img.width + num, (float)img.height + num, false);
		}

		protected virtual void redrawBodyMeshInnerAfter(Matrix4x4 MxAfterMultiple, PxlFrame curF = null, EnemyFrameDataBasic CurFrmData = null)
		{
		}

		protected readonly NelEnemy Mv;

		public CCNestItem NestTarget;

		public uint layer_mask = uint.MaxValue;

		public string normalrender_header;
	}
}
