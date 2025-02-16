using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class EffectNelMapChip : EffectNel, IMapChipEffectSetter
	{
		public EffectNelMapChip(NelM2DBase _M2D, M2SubMap _SM, GameObject Gob, int _max = 240)
			: base(Gob, _max)
		{
			this.M2D = _M2D;
			this.SM = _SM;
			this.GMx = ((_SM == null) ? Matrix4x4.Scale(new Vector3(this.M2D.curMap.base_scale, this.M2D.curMap.base_scale, 1f)) : this.SM.getTransformForCamera(false)) * Matrix4x4.Scale(new Vector3(1f, 1f, 0f));
		}

		private EffectItem EfSetAfter(EffectItem _E)
		{
			if (_E != null && !this.cam_assigned)
			{
				this.assignToCamera();
			}
			return _E;
		}

		public void deassignRender()
		{
			if (this.BindB != null)
			{
				this.M2D.Cam.deassignRenderFunc(this.BindB, this.layer_effect_bottom);
				this.BindB = null;
			}
			if (this.BindT != null)
			{
				this.M2D.Cam.deassignRenderFunc(this.BindT, this.layer_effect_top);
				this.BindT = null;
			}
			if (this.ABind != null)
			{
				int count = this.ABind.Count;
				for (int i = 0; i < count; i++)
				{
					this.ABind[i].unbindToCamera();
				}
			}
			this.cam_assigned = false;
		}

		public void assignToCamera()
		{
			this.cam_assigned = true;
			if (!X.DEBUGSTABILIZE_DRAW && this.BindB == null)
			{
				this.M2D.Cam.assignRenderFunc(this.BindB = new CameraRenderBinderFunc(this.ToString() + "::BindB", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					base.RenderOneSide(true, JCon.CameraProjectionTransformed * this.GMx, Cam, false);
					return true;
				}, this.bottomBaseZ + ((this.SM != null) ? this.SM.base_z : 0f)), this.layer_effect_bottom, true, null);
				this.M2D.Cam.assignRenderFunc(this.BindT = new CameraRenderBinderFunc(this.ToString() + "::BindT", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					base.RenderOneSide(false, JCon.CameraProjectionTransformed * this.GMx, Cam, false);
					return true;
				}, this.topBaseZ + ((this.SM != null) ? this.SM.base_z : 0f)), this.layer_effect_top, true, null);
			}
			if (this.ABind != null)
			{
				int count = this.ABind.Count;
				for (int i = 0; i < count; i++)
				{
					this.ABind[i].bindToCamera();
				}
			}
		}

		public override EffectItem setEffectWithSpecificFn(string individual_name, float x, float y, float z, int time, int saf, FnEffectRun Fn)
		{
			return this.EfSetAfter(base.setEffectWithSpecificFn(individual_name, x, y, z, time, saf, Fn));
		}

		public override EffectItem setE(string etype, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			return this.EfSetAfter(base.setE(etype, _x, _y, _z, _time, _saf));
		}

		public override EffectItem PtcN(EfParticle Ptc, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			return this.EfSetAfter(base.PtcN(Ptc, _x, _y, _z, _time, _saf));
		}

		public override void destruct()
		{
			this.deassignRender();
			this.ABind = null;
		}

		public void addSubMapBinder(M2SubMap.EfSubMapEffectBinder Bd)
		{
			if (X.DEBUGSTABILIZE_DRAW)
			{
				return;
			}
			if (this.ABind == null)
			{
				this.ABind = new List<M2SubMap.EfSubMapEffectBinder>(2);
				this.ABind.Add(Bd);
			}
			if (this.cam_assigned)
			{
				Bd.bindToCamera();
			}
		}

		public void setGraphicMatrix(Matrix4x4 Mx)
		{
			this.GMx = Mx;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<EffectNelMapChip>",
				this.effect_name,
				" - L:",
				this.layer_effect_bottom.ToString(),
				",",
				this.layer_effect_top.ToString(),
				" / C:",
				base.Length.ToString()
			});
		}

		private NelM2DBase M2D;

		private Matrix4x4 GMx;

		private M2SubMap SM;

		public bool cam_assigned;

		public List<M2SubMap.EfSubMapEffectBinder> ABind;

		private CameraRenderBinderFunc BindB;

		private CameraRenderBinderFunc BindT;
	}
}
