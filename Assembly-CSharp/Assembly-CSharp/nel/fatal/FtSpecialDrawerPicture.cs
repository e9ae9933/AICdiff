using System;
using Spine;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal class FtSpecialDrawerPicture : FtSpecialDrawer
	{
		public FtSpecialDrawerPicture(FtLayer Lay, SpvLoader SpvL, AtlasRegion _Atl, int _stencil_ref = -1, bool free_handleable = false)
			: base(Lay)
		{
			this.Atl = _Atl;
			this.stencil_ref = _stencil_ref;
			this.MI = SpvL.MtiImage.MI;
			this.maxt = 1f;
			base.Col0 = MTRX.ColWhite;
			if (free_handleable && this.stencil_ref >= 0)
			{
				this.free_handleable = 1;
			}
			this.fineMaterial(Lay.getMeshDrawer());
		}

		public bool isSame(AtlasRegion _Atl, int _stencil_ref)
		{
			return this.Atl == _Atl && this.stencil_ref == _stencil_ref;
		}

		protected override void fineMaterial(MeshDrawer Md)
		{
			if (this.stencil_ref == -2)
			{
				return;
			}
			base.Col0 = MTRX.ColWhite;
			Md.setMaterial(this.MI.getMtr(BLEND.NORMAL, (this.free_handleable == 2) ? (-1) : this.stencil_ref), false);
		}

		public override void initAlphaFade(bool fadein)
		{
			if (this.free_handleable >= 1)
			{
				this.stencil_disabled = !fadein;
			}
		}

		public bool stencil_disabled
		{
			get
			{
				return this.free_handleable == 2;
			}
			set
			{
				if (value != this.stencil_disabled && this.free_handleable >= 1)
				{
					this.free_handleable = (value ? 2 : 1);
					this.fineMaterial(this.Lay.getMeshDrawer());
					this.Lay.fineMrdMaterial();
					this.t = 0f;
				}
			}
		}

		public override void destruct(MeshDrawer Md)
		{
			Md.setMaterial(MTRX.MtrMeshNormal, false);
			base.destruct(Md);
		}

		public override void sinkTime(FtSpecialDrawer SD)
		{
		}

		public override bool sinkTimeIfSame(FtSpecialDrawer SD)
		{
			if (SD is FtSpecialDrawerPicture)
			{
				FtSpecialDrawerPicture ftSpecialDrawerPicture = SD as FtSpecialDrawerPicture;
				if (this.isSame(this.Atl, this.stencil_ref))
				{
					this.sinkTime(ftSpecialDrawerPicture);
					return true;
				}
			}
			return false;
		}

		public override bool checkRedraw(float fcnt)
		{
			return this.maxt > 0f && base.enabled && !this.Lay.alpha_is_zero && this.t == 0f;
		}

		public override void drawTo(MeshDrawer Md)
		{
			if (this.t == 0f)
			{
				this.t = 1f;
			}
			Md.Identity();
			Md.Col = base.Col0;
			Md.Rotate((float)(-(float)this.Atl.degrees) / 180f * 3.1415927f, false);
			Rect rect = new Rect(this.Atl.u, this.Atl.v2, this.Atl.u2 - this.Atl.u, this.Atl.v - this.Atl.v2);
			float num = rect.width * (float)this.Atl.page.width;
			float num2 = rect.height * (float)this.Atl.page.height;
			Md.initForImg(this.MI.Tx, rect, false).Rect(0f, 0f, num, num2, false);
			Md.updateForMeshRenderer(false);
		}

		private AtlasRegion Atl;

		private MImage MI;

		public int stencil_ref = -2;

		private byte free_handleable;
	}
}
