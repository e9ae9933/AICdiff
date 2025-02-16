using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChipImageNested : M2ChipImage
	{
		public M2ChipImageNested(M2ImageContainer _IMGS, string _src)
			: base(_IMGS, _src)
		{
		}

		public M2ChipImageNested(M2ImageContainer _IMGS, string _dirname, string _basename_noext)
			: base(_IMGS, _dirname, _basename_noext)
		{
		}

		public bool is_parent
		{
			get
			{
				return this.Parent == this;
			}
		}

		public void getPosition(out int x, out int y)
		{
			this.Parent.getPosition(this.ns_index, out x, out y);
		}

		public override M2ChipImage copyAttributesFrom(M2ChipImage Src)
		{
			if (this != this.Parent)
			{
				this.Parent.copyBasicAttributesFrom(Src);
			}
			return this;
		}

		public void replaceImgMain(PxlMeshDrawer Img)
		{
			this.ImgMain = Img;
		}

		public PxlMeshDrawer getTargetMainImage()
		{
			return this.ImgMain;
		}

		public override void assignLayerAndAtlas(PxlLayer Lay, M2ImageAtlas.AtlasRect _Atlas, bool apply_basic = false)
		{
			this.SourceLayer_ = Lay;
			this.SourceAtlas_ = _Atlas;
		}

		public void getAtlasDrawArea()
		{
			this.Parent.getAtlasDrawArea(this.ns_index);
		}

		public override void recreateImageMesh(bool no_error = false)
		{
		}

		public virtual void copyChildrenTo(List<M2ChipImage> A)
		{
			if (this != this.Parent)
			{
				this.Parent.copyChildrenTo(A);
			}
		}

		public override PxlMeshDrawer getSrcMeshForPicture(int layer, uint pattern_for_picture)
		{
			if (pattern_for_picture == 0U)
			{
				return base.getSrcMeshForPicture(layer, pattern_for_picture);
			}
			return this.Parent.getSourceMesh(layer);
		}

		public override bool isWithinOnPicture(float px_x, float px_y)
		{
			if (base.SourceLayer != null && X.BTW(0f, px_x, (float)base.SourceLayer.Img.width) && X.BTW(0f, px_y, (float)base.SourceLayer.Img.height))
			{
				int num = (int)((px_x + (float)this.Parent.get_srcl()) / (float)(base.CLEN * this.Parent.iclms));
				int num2 = (int)((px_y + (float)this.Parent.get_srct()) / (float)(base.CLEN * this.Parent.irows));
				num = X.MMX(0, num, this.Parent.ns_clms - 1);
				num2 = X.MMX(0, num2, this.Parent.ns_rows - 1);
				return this.Parent.GetAt(num, num2) != null;
			}
			return false;
		}

		public override Rect getClipArea(uint pattern_for_picture = 0U)
		{
			if (pattern_for_picture <= 0U || !this.SourceAtlas_.valid)
			{
				return this.Parent.getClipArea(this.ns_index);
			}
			return new Rect(0f, 0f, (float)this.SourceAtlas_.w, (float)this.SourceAtlas_.h);
		}

		public override bool initAtlasMd(MeshDrawer Md, uint pattern_for_picture = 0U)
		{
			if (!this.SourceAtlas_.valid)
			{
				return false;
			}
			this.SourceAtlas_.initAtlasMd(Md, this.IMGS.MIchip, this.getClipArea(pattern_for_picture));
			return true;
		}

		public M2ChipImageNestedParent Parent;

		public int ns_index;
	}
}
