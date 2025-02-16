using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class CameraRenderBinderWaterSurface : CameraRenderBinderMesh
	{
		public CameraRenderBinderWaterSurface(Map2d _Mp, MistManager.MISTTYPE _misttype, Color32 SurfaceColor, MeshDrawer Md, Func<bool> _fnListenRemoveDrawer)
			: base("WaterSurface", Md, _Mp.Dgn.effect_layer_t_bottom, true, -1000f)
		{
			this.Mp = _Mp;
			this.misttype = _misttype;
			this.C = SurfaceColor;
			Material waterSurfaceReflectionMaterial = this.Mp.Dgn.getWaterSurfaceReflectionMaterial();
			waterSurfaceReflectionMaterial.SetTexture("_MainTex", this.Mp.M2D.Cam.getFinalizedTexture());
			this.fined_floort = this.Mp.floort;
			this.fnListenRemoveDrawer = _fnListenRemoveDrawer;
			Md.draw_gl_only = true;
			Md.activate("WaterSurface", waterSurfaceReflectionMaterial, true, MTRX.ColWhite, null);
			this.z_far = 40f;
			this.redrawKind(waterSurfaceReflectionMaterial);
		}

		public void redraw()
		{
			this.Md.clearSimple();
			this.Md.base_x = (this.Md.base_y = 0f);
			if (this.Md.hasMultipleTriangle())
			{
				this.Md.chooseSubMesh(0, false, true);
			}
			if (this.draw_height > 0f)
			{
				this.Mp.Dgn.drawWaterSurfaceReflection(this.Md, this.draw_height, this.C, this.alpha_);
			}
			this.ver_i = this.Md.getVertexMax();
			this.tri_i = this.Md.getTriMax();
			this.redraw_flag = false;
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				this.fined_floort = this.Mp.floort;
				if (this.alpha_ != value)
				{
					this.alpha_ = value;
					this.redraw_flag = true;
				}
			}
		}

		private float meshbottom
		{
			get
			{
				return this.Mp.map2meshy(this.Mp.M2D.Cam.lt_mapy) - this.Mp.M2D.Cam.get_h();
			}
		}

		public override bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			float meshbottom = this.meshbottom;
			float num = 1f - X.ZLINE(this.fined_floort - this.Mp.floort - 40f, 60f);
			float num2 = (this.drawt - meshbottom) * this.Mp.base_scale * num;
			if (num2 != this.draw_height)
			{
				this.redraw_flag = true;
				this.draw_height = num2;
			}
			if (this.draw_height > 0f)
			{
				if (this.alpha_ > 0f)
				{
					if (this.redraw_flag)
					{
						this.redraw();
					}
					Vector4 vector = M2DBase.Instance.Cam.PosMainTransform * 64f;
					vector.x /= IN.w;
					vector.y /= IN.h;
					this.Md.getMaterial().SetVector("_CamPos", vector);
					this.redrawKind(null);
					Vector3 posMainTransform = this.Mp.M2D.Cam.PosMainTransform;
					this.base_x = posMainTransform.x;
					this.base_y = posMainTransform.y;
					base.RenderToCam(XCon, JCon, Cam);
				}
				return true;
			}
			return this.fnListenRemoveDrawer();
		}

		private void redrawKind(Material InitMtr)
		{
			MistManager.MISTTYPE misttype = this.misttype;
			if (misttype != MistManager.MISTTYPE.LAVA)
			{
				if (misttype != MistManager.MISTTYPE.SEA)
				{
					if (InitMtr != null)
					{
						InitMtr.DisableKeyword("USELAVA");
					}
				}
				else if (InitMtr != null)
				{
					InitMtr.EnableKeyword("USELAVA");
					InitMtr.SetColor("_LavaCol", new Color32(byte.MaxValue, 0, 47, 53));
					return;
				}
				return;
			}
			if (InitMtr != null)
			{
				this.Md.chooseSubMesh(1, false, true);
				this.Md.setMaterial(MTRX.MtrMeshNormal, false);
				this.AMtr = this.Md.getMaterialArray(false);
				this.AEf = new List<EfParticleOnce>(4);
				InitMtr.EnableKeyword("USELAVA");
				InitMtr.SetColor("_LavaCol", new Color32(70, 14, 14, 65));
				return;
			}
			this.Md.revertVerAndTriIndex(this.ver_i, this.tri_i, false);
			this.Md.chooseSubMesh(1, false, true);
			if (this.particle_set_floort <= this.Mp.floort)
			{
				this.particle_set_floort = this.Mp.floort + 150f;
				EfParticleOnce efParticleOnce = new EfParticleOnce("watersurface_lava", EFCON_TYPE.UI);
				this.AEf.Add(efParticleOnce);
				efParticleOnce.x = this.Mp.M2D.Cam.PosMainTransform.x;
			}
			this.Md.base_y = (-IN.hh + this.draw_height) * 0.015625f;
			for (int i = this.AEf.Count - 1; i >= 0; i--)
			{
				EfParticleOnce efParticleOnce2 = this.AEf[i];
				efParticleOnce2.af += (float)X.AF;
				efParticleOnce2.z = this.alpha_;
				this.Md.base_x = efParticleOnce2.x - this.Mp.M2D.Cam.PosMainTransform.x;
				if (!efParticleOnce2.drawTo(this.Md, efParticleOnce2.af, 0f))
				{
					this.AEf.RemoveAt(i);
				}
			}
			this.Md.chooseSubMesh(0, false, false);
		}

		public readonly Map2d Mp;

		private float draw_height = -1000f;

		public float drawt = -1000f;

		private float alpha_ = 1f;

		private bool redraw_flag;

		private float fined_floort;

		private Color32 C;

		public Func<bool> fnListenRemoveDrawer;

		private MistManager.MISTTYPE misttype;

		private int ver_i;

		private int tri_i;

		private List<EfParticleOnce> AEf;

		private float particle_set_floort;
	}
}
