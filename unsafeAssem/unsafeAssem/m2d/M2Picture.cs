using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Picture : M2Puts
	{
		public M2Picture(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, int index, M2ChipImage Img)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, index, Img)
		{
			this.finePos((float)drawx, (float)drawy, (float)rotation / 180f * 3.1415927f, false);
		}

		public M2Picture finePos(float _drawx, float _drawy, float _rotR, bool center_flag = true)
		{
			if (center_flag)
			{
				this.rotation = X.IntR(_rotR * 180f / 3.1415927f);
				this.rotR = (float)this.rotation / 180f * 3.1415927f;
			}
			else
			{
				this.rotR = _rotR;
			}
			float num = Mathf.Abs(X.Cos(this.rotR));
			float num2 = Mathf.Abs(X.Sin(this.rotR));
			this.dwidth = (float)this.Img.iwidth * num + (float)this.Img.iheight * num2;
			this.dheight = (float)this.Img.iheight * num + (float)this.Img.iwidth * num2;
			float num3;
			float num4;
			if (center_flag)
			{
				num3 = (float)(this.drawx = X.IntR(_drawx - this.dwidth * 0.5f));
				num4 = (float)(this.drawy = X.IntR(_drawy - this.dheight * 0.5f));
			}
			else
			{
				num3 = (float)(this.drawx = X.IntR(_drawx));
				num4 = (float)(this.drawy = X.IntR(_drawy));
			}
			this.mapx = (int)(num3 / base.CLEN);
			this.mapy = (int)(num4 / base.CLEN);
			this.rwidth = (int)((Mathf.Ceil((num3 + this.dwidth) / base.CLEN) - (float)this.mapx) * base.CLEN);
			this.rheight = (int)((Mathf.Ceil((num4 + this.dheight) / base.CLEN) - (float)this.mapy) * base.CLEN);
			return this;
		}

		public override float draw_rotR
		{
			get
			{
				return -this.rotR;
			}
		}

		protected override PxlMeshDrawer getSrcMeshForEntry(int _layer)
		{
			return this.Img.getSrcMeshForPicture(_layer, this.pattern);
		}

		public override bool draw(MeshDrawer Md, int fcnt = 1, float sx = 0f, float sy = 0f, bool force_use_atlas = false)
		{
			sx = this.Mp.pixel2meshx((float)this.drawx + sx + this.dwidth * 0.5f);
			sy = this.Mp.pixel2meshy((float)this.drawy + sy + this.dheight * 0.5f);
			if (!force_use_atlas && Map2d.editor_decline_lighting)
			{
				if (this.Img.SourceLayer != null)
				{
				}
			}
			else if (this.Img.initAtlasMd(Md, this.pattern))
			{
				Md.RotaGraph(sx, sy, 1f, -this.rotR, null, this.flip);
			}
			Md.Identity();
			return false;
		}

		public bool isContainingRotatedRect(float mpx, float mpy, float extend = 0f)
		{
			mpx -= this.mapcx;
			mpy -= this.mapcy;
			Vector2 vector = X.ROTV2e(new Vector2(mpx, mpy), -this.rotR);
			float num = (float)(this.iwidth / 2) / base.CLEN + extend;
			float num2 = (float)(this.iheight / 2) / base.CLEN + extend;
			return X.BTW(-num, vector.x, num) && X.BTW(-num2, vector.y, num2);
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdL, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			return base.entryChipMesh(MdB, MdG, MdT, MdL, MdTT, sx = this.Mp.pixel2meshx((float)this.drawx + sx + this.dwidth * 0.5f), sy = this.Mp.pixel2meshy((float)this.drawy + sy + this.dheight * 0.5f), _zm, _rotR - this.rotR);
		}

		public override bool isWithin(float _x, float _y, int drawer_index, bool strict = false)
		{
			if (!base.isWithin(_x, _y, drawer_index, strict))
			{
				return false;
			}
			Vector2 vector = X.ROTV2e(new Vector2(_x - this.mapcx, -(_y - this.mapcy)), -this.draw_rotR);
			return this.Img.isWithinOnPicture((float)this.Img.iwidth * 0.5f + vector.x * base.CLEN, (float)this.Img.iheight * 0.5f - vector.y * base.CLEN);
		}

		public override float mapcx
		{
			get
			{
				return ((float)this.drawx + this.dwidth * 0.5f) / base.CLEN;
			}
		}

		public override float mapcy
		{
			get
			{
				return ((float)this.drawy + this.dheight * 0.5f) / base.CLEN;
			}
		}

		public override List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			return new List<M2Picture>(1) { Lay.MakePicture(this.Img, X.IntR(x * base.CLEN), X.IntR(y * base.CLEN), opacity, rotation, flip, -1) };
		}

		public float rotR;

		private float dwidth;

		private float dheight;
	}
}
