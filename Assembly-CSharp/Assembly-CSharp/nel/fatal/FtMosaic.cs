using System;
using Spine;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal sealed class FtMosaic : MosaicShower, IMosaicDescriptor
	{
		public FtMosaic(Transform Trs)
			: base(Trs)
		{
			base.render_to_target_texture = true;
		}

		public bool enabled
		{
			get
			{
				return this.use_mosaic;
			}
			set
			{
				if (!value)
				{
					if (this.TargLayer != null && this.TargLayer.Mosaic == this)
					{
						this.TargLayer.Mosaic = null;
					}
					base.setTarget(null, false);
				}
			}
		}

		public void initMosaic(FtLayer Targ, MosaicShower.MosaicInfo _Info)
		{
			this.TargLayer = Targ;
			this.TargLayer.Mosaic = this;
			this.Info = _Info;
			this.CacheBone = null;
			this.SlotForAtc = null;
			this.Trs = Targ.Spv.gameObject.transform;
			base.BindTo = Targ.FtCon.getCameraBindingsFor(Targ.buffer);
			this.fineScale();
			base.draw_mesh_layer = -1;
			base.setTarget(this, false);
		}

		public void drawToMesh()
		{
			if (!base.draw_gl_only)
			{
				base.drawToMesh(this.CamForMeshDraw);
			}
		}

		public void fineScale()
		{
			if (this.TargLayer == null)
			{
				return;
			}
			base.resolution = X.IntC(X.Mx(4f, this.TargLayer.scale * 10f));
		}

		public int countMosaic(bool only_on_sensitive)
		{
			if (this.Info == null || (!only_on_sensitive && this.Info.only_appear_sensitive))
			{
				return 0;
			}
			return 1;
		}

		public bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot)
		{
			if (this.TargLayer == null || id != 0)
			{
				return false;
			}
			if (this.Info.for_attachment)
			{
				if (this.SlotForAtc == null)
				{
					this.SlotForAtc = this.TargLayer.Spv.GetSkeleton().FindSlot(this.Info.bone_key);
					if (this.SlotForAtc == null)
					{
						return false;
					}
				}
				BelongSlot = this.SlotForAtc;
				this.CacheBone = this.SlotForAtc.Bone;
				OutMesh = this.SlotForAtc.Attachment as MeshAttachment;
			}
			if (this.CacheBone == null)
			{
				this.CacheBone = this.TargLayer.FindBone(this.Info.bone_key);
				if (this.CacheBone == null)
				{
					return false;
				}
			}
			if (OutMesh != null)
			{
				Matrix4x4 matrix4x = Matrix4x4.identity;
				OutMesh.IsWeighted();
				Out = this.Trs.localToWorldMatrix * matrix4x;
			}
			else
			{
				float num = this.Info.radius * 0.015625f * this.TargLayer.scale;
				Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(num * 2f, num * 2f, 0f));
				Vector3 vector = this.Trs.TransformPoint(new Vector3(this.CacheBone.WorldX, this.CacheBone.WorldY, 0f));
				Out = Matrix4x4.Translate(new Vector3(vector.x, vector.y, 0f)) * matrix4x;
			}
			return true;
		}

		public FtLayer TargLayer;

		public MosaicShower.MosaicInfo Info;

		private Camera CamForMeshDraw;

		public Slot SlotForAtc;

		public Bone CacheBone;
	}
}
