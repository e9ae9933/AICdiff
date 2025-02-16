using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal sealed class FtEffect : Effect<EffectItem>
	{
		public FtEffect(FtLayer _Ftl, string name)
			: base(_Ftl.Gob, 64)
		{
			this.Ftl = _Ftl;
			this.TargetTransform = this.Ftl.transform;
			this.AFtp = new List<FtParticle>(12);
			this.initEffect(name, IN.getGUICamera(), new Effect<EffectItem>.FnCreateEffectItem(EffectItem.fnCreateOne), EFCON_TYPE.UI);
			base.draw_gl_only = (base.no_graphic_render = true);
			this.EfBindT = new CameraRenderBinderFunc("FT-" + name + "-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				if (this.Ftl.FtCon.menu_shown)
				{
					return true;
				}
				this.MeshCon.MatrixTransform = Matrix4x4.Scale(new Vector3(1f, 1f, 0f));
				base.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
				return true;
			}, this.topBaseZ);
			this.EfBindB = new CameraRenderBinderFunc("FT-" + name + "-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				if (this.Ftl.FtCon.menu_shown)
				{
					return true;
				}
				this.MeshCon.MatrixTransform = this.TargetTransform.localToWorldMatrix;
				base.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
				return true;
			}, this.bottomBaseZ);
			this.BindTo_ = this.Ftl.FtCon.getCameraBindingsFor(this.Ftl.buffer);
		}

		public override void clear()
		{
			base.clear();
			for (int i = 0; i < this.ftp_max; i++)
			{
				this.AFtp[i].release(false);
			}
			this.ftp_max = 0;
			this.enabled = (this.effect_ptc_enabled = false);
		}

		public bool enabled
		{
			get
			{
				return this.enabled_;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				this.enabled_ = value;
				if (value)
				{
					this.layer_effect_top = this.Gob.layer;
					this.layer_effect_bottom = this.Gob.layer;
					Vector3 position = this.TargetTransform.position;
					this.EfBindB.z_far = position.z - 0.1f;
					this.EfBindT.z_far = position.z - 0.5f;
				}
				ValotileRenderer.IValotConnetcable valotConnetcable = ((this.BindTo_ != null) ? null : this.Ftl.ConnectCam);
				if (valotConnetcable != null)
				{
					if (value)
					{
						valotConnetcable.assignRenderFuncXC(this.EfBindT, this.layer_effect_top, false, null);
						valotConnetcable.assignRenderFuncXC(this.EfBindB, this.layer_effect_bottom, false, null);
						return;
					}
					valotConnetcable.deassignRenderFunc(this.EfBindT, this.layer_effect_top);
					valotConnetcable.deassignRenderFunc(this.EfBindB, this.layer_effect_bottom);
					return;
				}
				else
				{
					CameraBidingsBehaviour cameraBidingsBehaviour = this.BindTo_ ?? CameraBidingsBehaviour.UiBind;
					if (value)
					{
						cameraBidingsBehaviour.assignPostRenderFunc(this.EfBindT);
						cameraBidingsBehaviour.assignPostRenderFunc(this.EfBindB);
						return;
					}
					cameraBidingsBehaviour.deassignPostRenderFunc(this.EfBindT);
					cameraBidingsBehaviour.deassignPostRenderFunc(this.EfBindB);
					return;
				}
			}
		}

		public FtParticle addPtc(string particle, string bone, float _intv, float _z, int _time, float _saf, bool is_effect = true, bool use_rot_to_z = false, bool is_once = false)
		{
			this.enabled = (this.effect_ptc_enabled = true);
			FtParticle ftParticle;
			if (this.ftp_max < this.AFtp.Count)
			{
				ftParticle = this.AFtp[this.ftp_max];
				ftParticle = ftParticle.Set(this.Ftl, particle, bone, _intv, _z, _time, _saf, is_effect, use_rot_to_z, is_once);
				if (ftParticle != null)
				{
					this.ftp_max++;
				}
				return ftParticle;
			}
			ftParticle = new FtParticle().Set(this.Ftl, particle, bone, _intv, _z, _time, _saf, is_effect, use_rot_to_z, is_once);
			if (ftParticle == null)
			{
				return ftParticle;
			}
			this.AFtp.Add(ftParticle);
			this.ftp_max = this.AFtp.Count;
			return ftParticle;
		}

		public override void runDraw(float fcnt = 1f, bool runsetter = true)
		{
			base.runDraw(fcnt, runsetter);
			for (int i = this.ftp_max - 1; i >= 0; i--)
			{
				FtParticle ftParticle = this.AFtp[i];
				if (!ftParticle.run(this, fcnt))
				{
					this.AFtp.RemoveAt(i);
					this.AFtp.Add(ftParticle);
					this.ftp_max--;
				}
			}
		}

		public CameraBidingsBehaviour BindTo
		{
			get
			{
				return this.BindTo_;
			}
			set
			{
				if (this.BindTo_ == value)
				{
					return;
				}
				bool flag = this.enabled_;
				this.enabled = false;
				this.BindTo_ = value;
				if (flag)
				{
					this.enabled = true;
				}
			}
		}

		private List<FtParticle> AFtp;

		public readonly FtLayer Ftl;

		private int ftp_max;

		public Transform TargetTransform;

		private CameraRenderBinderFunc EfBindT;

		private CameraRenderBinderFunc EfBindB;

		private CameraBidingsBehaviour BindTo_;

		private bool enabled_;

		public bool effect_ptc_enabled;
	}
}
