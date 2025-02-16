using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Chip : M2Puts
	{
		public M2Chip(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, -1, _Img)
		{
			this.inputRots(false);
		}

		public override Vector2Int getShift()
		{
			return this.Img.getShift(this.rotation, this.flip);
		}

		public M2Chip inputRots(bool consider_from_maploc = false)
		{
			bool flag = this.rotation % 2 == 1;
			this.iwidth = (flag ? this.Img.iheight : this.Img.iwidth);
			this.iheight = (flag ? this.Img.iwidth : this.Img.iheight);
			if (consider_from_maploc)
			{
				Vector2Int shift = this.getShift();
				Vector2Int rwh = this.Img.getRWH(this.rotation);
				this.drawx = (int)((float)this.mapx * base.CLEN);
				if (shift[0] < 0)
				{
					this.drawx -= rwh[0] - this.iwidth + shift[0];
				}
				else
				{
					this.drawx += shift[0];
				}
				this.drawy = (int)((float)this.mapy * base.CLEN);
				if (shift[1] < 0)
				{
					this.drawy -= rwh[1] - this.iheight + shift[1];
				}
				else
				{
					this.drawy += shift[1];
				}
			}
			this.inputXy();
			float num = (float)this.Img.width;
			float num2 = (float)this.Img.height;
			this.mclms = X.IntC(num / base.CLEN);
			this.rwidth = (int)((float)X.IntC((flag ? num2 : num) / base.CLEN) * base.CLEN);
			this.rheight = (int)((float)X.IntC((flag ? num : num2) / base.CLEN) * base.CLEN);
			this.clms = Mathf.CeilToInt((float)this.rwidth / base.CLEN);
			this.rows = Mathf.CeilToInt((float)this.rheight / base.CLEN);
			return this;
		}

		public override M2Puts inputXy()
		{
			Vector2Int shift = this.getShift();
			Vector2Int rwh = this.Img.getRWH(this.rotation);
			this.x = (float)this.drawx;
			if (shift[0] < 0)
			{
				this.x = this.x + (float)rwh[0] - (float)this.iwidth + (float)shift[0];
			}
			else
			{
				this.x -= (float)shift[0];
			}
			this.x /= base.CLEN;
			this.mapx = X.IntR(this.x);
			this.y = (float)this.drawy;
			if (shift[1] < 0)
			{
				this.y = this.y + (float)rwh[1] - (float)this.iheight + (float)shift[1];
			}
			else
			{
				this.y -= (float)shift[1];
			}
			this.mapy = X.IntR(this.y / base.CLEN);
			if (this.Img.horizony > -1000)
			{
				this.y = (float)(this.drawy + this.Img.horizony) / base.CLEN;
			}
			else
			{
				this.y /= base.CLEN;
			}
			return this;
		}

		public override int getConfig(int x, int y)
		{
			switch (this.rotation)
			{
			case 1:
				if (!X.BTW(0f, (float)x, (float)this.rows) || !X.BTW(0f, (float)y, (float)this.clms))
				{
					return 4;
				}
				return this.getConfigVal(y, this.clms - 1 - x);
			case 2:
				if (!X.BTW(0f, (float)x, (float)this.clms) || !X.BTW(0f, (float)y, (float)this.rows))
				{
					return 4;
				}
				return this.getConfigVal(this.clms - 1 - x, this.rows - 1 - y);
			case 3:
				if (!X.BTW(0f, (float)x, (float)this.rows) || !X.BTW(0f, (float)y, (float)this.clms))
				{
					return 4;
				}
				return this.getConfigVal(this.rows - 1 - y, x);
			default:
				if (!X.BTW(0f, (float)x, (float)this.clms) || !X.BTW(0f, (float)y, (float)this.rows))
				{
					return 4;
				}
				return this.getConfigVal(x, y);
			}
		}

		public override bool isOnCamera(float cam_left, float cam_top, float camw, float camh)
		{
			return (!base.active_removed || !(this.AttachCM == null)) && X.isCovering(cam_left, cam_left + camw, (float)this.drawx, (float)(this.drawx + this.iwidth), 0f) && X.isCovering(cam_top, cam_top + camh, (float)this.drawy, (float)(this.drawy + this.iheight), 0f);
		}

		private int getConfigVal(int cx, int cy)
		{
			int num = (this.flip ? (this.mclms * (cy + 1) - 1 - cx) : (this.mclms * cy + cx));
			int num2 = (int)(X.BTW(0f, (float)num, (float)this.Img.Aconfig.Length) ? this.Img.Aconfig[num] : 0);
			return (this.flip && CCON.isSlope(num2)) ? CCON.slopeFlip(num2) : num2;
		}

		public override bool draw(MeshDrawer Md, int fcnt = 1, float sx = 0f, float sy = 0f, bool force_use_atlas = false)
		{
			sx = this.Mp.pixel2meshx((float)X.IntU((float)this.drawx + sx) + (float)this.iwidth * 0.5f);
			sy = this.Mp.pixel2meshy((float)X.IntU((float)this.drawy + sy) + (float)this.iheight * 0.5f);
			float num = 1f;
			if (!force_use_atlas && Map2d.editor_decline_lighting)
			{
				if (this.Img.SourceLayer != null)
				{
					this.Img.MdInitImgBySourceLayer(Md, 0U);
					Md.RotaGraph(sx, sy, num, (float)(-(float)this.rotation) * 1.5707964f, null, this.flip);
				}
			}
			else if (this.Img.initAtlasMd(Md, 0U))
			{
				Md.RotaGraph(sx, sy, num, (float)(-(float)this.rotation) * 1.5707964f, null, this.flip);
			}
			return false;
		}

		public float draw_meshx
		{
			get
			{
				return this.drawx2meshx((float)this.drawx);
			}
		}

		public float draw_meshy
		{
			get
			{
				return this.drawy2meshy((float)this.drawy);
			}
		}

		public float draw_effectx
		{
			get
			{
				return base.M2D.ux2effectScreenx(this.drawx2meshx((float)this.drawx));
			}
		}

		public float draw_effecty
		{
			get
			{
				return base.M2D.uy2effectScreeny(this.drawy2meshy((float)this.drawy));
			}
		}

		public float drawx2meshx(float _drawx)
		{
			return this.Mp.pixel2meshx((float)X.IntU(_drawx) + (float)this.iwidth * 0.5f);
		}

		public float drawy2meshy(float _drawy)
		{
			return this.Mp.pixel2meshy((float)X.IntU(_drawy) + (float)this.iheight * 0.5f);
		}

		public override bool config_considerable
		{
			get
			{
				return this.Img != null && !this.Img.isBg();
			}
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdLB, MeshDrawer MdLT, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			return base.entryChipMesh(MdB, MdG, MdT, MdLB, MdLT, MdTT, this.drawx2meshx((float)this.drawx + sx), this.drawy2meshy((float)this.drawy + sy), _zm, _rotR - (float)this.rotation * 1.5707964f);
		}

		public override bool isWithin(float _x, float _y, int drawer_index, bool strict = false)
		{
			if (!base.isWithin(_x, _y, drawer_index, strict))
			{
				return false;
			}
			if (strict)
			{
				return X.BTWW((float)this.drawx * base.rCLEN - 0.018f, _x, (float)(this.drawx + this.iwidth) * base.rCLEN + 0.018f) && X.BTWW((float)this.drawy * base.rCLEN - 0.018f, _y, (float)(this.drawy + this.iheight) * base.rCLEN + 0.018f);
			}
			return X.BTWW((float)this.mapx - 0.018f, _x, (float)(this.mapx + this.clms) + 0.018f) && X.BTWW((float)this.mapy - 0.018f, _y, (float)(this.mapy + this.rows) + 0.018f);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<M2Chip> ";
				stb += this.mapx;
				stb += ",";
				stb += this.mapy;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public override List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			return new List<M2Chip>(1) { Lay.MakeChip(this.Img, (int)((float)this.drawx + (float)(x - this.mapx) * base.CLEN), (int)((float)this.drawy + (float)(y - this.mapy) * base.CLEN), opacity, rotation, flip) as M2Chip };
		}

		private int mclms;

		public int clms;

		public int rows;

		public bool dangerous;

		private static readonly float[] ACrossBuf = new float[2];

		public delegate M2Chip FnCreateChip(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img);
	}
}
