using System;
using Better;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class SlotDrawer
	{
		public SlotDrawer(PxlFrame F, int _shift_x = -1000, int _shift_y = -1000)
		{
			this.Init(F, _shift_x, _shift_y);
		}

		public SlotDrawer Init(PxlFrame F, int _shift_x = -1000, int _shift_y = -1000)
		{
			this.BaseFrame = F;
			this.shift_x = ((_shift_x == -1000) ? F.pSq.width : _shift_x);
			this.shift_y = ((_shift_y == -1000) ? F.pSq.height : _shift_y);
			this.intv = F.countLayers();
			return this;
		}

		public SlotDrawer ReturnIntv(int _return_intv, int _return_shift_x, int _return_shift_y)
		{
			this.return_intv = _return_intv;
			this.return_shift_x = _return_shift_x;
			this.return_shift_y = _return_shift_y;
			return this;
		}

		public SlotDrawer Fill(PxlFrame FillFrame, int start_index, int count = 1)
		{
			if (this.OFilledData == null)
			{
				if (FillFrame == null)
				{
					return this;
				}
				this.OFilledData = new BDic<int, PxlFrame>(count);
			}
			for (int i = 0; i < count; i++)
			{
				if (FillFrame == null)
				{
					this.OFilledData.Remove(start_index++);
				}
				else
				{
					this.OFilledData[start_index++] = FillFrame;
				}
			}
			return this;
		}

		public SlotDrawer FillCol(Color32 C, int start_index, int count = 1)
		{
			if (this.OFilledColor == null)
			{
				this.OFilledColor = new BDic<int, Color32>(count);
			}
			for (int i = 0; i < count; i++)
			{
				this.OFilledColor[start_index++] = C;
			}
			return this;
		}

		public SlotDrawer DefaultCol(int start_index, int count = 1)
		{
			if (this.OFilledColor == null)
			{
				return this;
			}
			for (int i = 0; i < count; i++)
			{
				this.OFilledColor.Remove(start_index++);
			}
			if (this.OFilledColor.Count == 0)
			{
				this.OFilledColor = null;
			}
			return this;
		}

		public SlotDrawer clearFill()
		{
			this.OFilledData = null;
			return this;
		}

		public PxlFrame getFillAt(int i)
		{
			if (this.OFilledData == null)
			{
				return null;
			}
			return X.Get<int, PxlFrame>(this.OFilledData, i);
		}

		public float getSWidthByCount(int whole_count)
		{
			int num = X.IntC((float)whole_count / (float)this.intv);
			float num2 = (float)((this.pwidth == 0) ? this.BaseFrame.pSq.width : this.pwidth);
			return (float)num * num2 - (float)(num - 1) * (num2 - (float)X.Abs(this.shift_x));
		}

		public float getSHeightByCount(int whole_count)
		{
			int num = X.IntC((float)whole_count / (float)this.intv);
			float num2 = (float)((this.pheight == 0) ? this.BaseFrame.pSq.height : this.pheight);
			return (float)num * num2 - (float)(num - 1) * (num2 - (float)X.Abs(this.shift_y));
		}

		public Vector2 getPos(int id, float scalex, float scaley, float swidth = 0f, float sheight = 0f)
		{
			int num = id / this.intv;
			float num2 = (float)((this.pwidth == 0) ? this.BaseFrame.pSq.width : this.pwidth);
			float num3 = (float)((this.pheight == 0) ? this.BaseFrame.pSq.height : this.pheight);
			if (swidth < 0f)
			{
				swidth = (float)num * num2 - (float)(num - 1) * (num2 - (float)X.Abs(this.shift_x));
			}
			if (sheight < 0f)
			{
				sheight = (float)num * num3 - (float)(num - 1) * (num3 - (float)X.Abs(this.shift_y));
			}
			float num4 = (float)((int)(-(swidth * 0.5f - num2 * 0.5f) * scalex * (float)X.MPF(this.shift_x >= 0)));
			float num5 = (float)((int)(-(sheight * 0.5f - num3 * 0.5f) * scaley * (float)X.MPF(this.shift_y >= 0)));
			float num6 = num4 + (float)(num * this.shift_x) * scalex;
			num5 += (float)(num * this.shift_y) * scaley;
			int num7 = id % this.intv;
			PxlLayer layer = this.BaseFrame.getLayer(num7);
			return new Vector2(num6 + layer.x * scalex, num5 - layer.y * scaley);
		}

		public SlotDrawer drawTo(MeshDrawer Md, float x, float y, float scalex, float scaley, int cnt = 0, float swidth = -1f, float sheight = -1f)
		{
			if (cnt <= 0)
			{
				return this;
			}
			int num = X.IntC((float)cnt / (float)this.intv);
			if (this.return_intv > 0)
			{
				X.Mn(this.return_intv, num);
				X.IntC((float)num / (float)this.return_intv);
			}
			float num2 = (float)((this.pwidth == 0) ? this.BaseFrame.pSq.width : this.pwidth);
			float num3 = (float)((this.pheight == 0) ? this.BaseFrame.pSq.height : this.pheight);
			if (swidth < 0f)
			{
				swidth = (float)num * num2 - (float)(num - 1) * (num2 - (float)X.Abs(this.shift_x));
			}
			if (sheight < 0f)
			{
				sheight = (float)num * num3 - (float)(num - 1) * (num3 - (float)X.Abs(this.shift_y));
			}
			x = (float)((int)(x - (swidth * 0.5f - num2 * 0.5f) * scalex * (float)X.MPF(this.shift_x >= 0)));
			y = (float)((int)(y - (sheight * 0.5f - num3 * 0.5f) * scaley * (float)X.MPF(this.shift_y >= 0)));
			int num4 = 0;
			int num5 = 0;
			float num6 = x;
			float num7 = y;
			Color32 col = Md.Col;
			for (int i = 0; i < cnt; i++)
			{
				PxlLayer layer = (this.getFillAt(i) ?? this.BaseFrame).getLayer(num4);
				if (this.OFilledColor != null)
				{
					Md.Col = X.Get<int, Color32>(this.OFilledColor, i, col);
				}
				Md.Scale(scalex, scaley, false).TranslateP(x, y, false);
				Md.RotaL(0f, 0f, layer, false, false, 0);
				Md.Identity();
				if (++num4 >= this.intv)
				{
					num4 = 0;
					if (++num5 >= this.return_intv && this.return_intv > 0)
					{
						num6 += (float)this.return_shift_x * scalex;
						num7 += (float)this.return_shift_y * scaley;
						x = num6;
						y = num7;
						num5 = 0;
					}
					else
					{
						x += (float)this.shift_x * scalex;
						y += (float)this.shift_y * scaley;
					}
				}
			}
			if (this.OFilledColor != null)
			{
				Md.Col = col;
			}
			return this;
		}

		private PxlFrame BaseFrame;

		public int shift_x;

		public int shift_y;

		public int pwidth;

		public int pheight;

		public int intv;

		public int return_intv;

		public int return_shift_x;

		public int return_shift_y;

		public BDic<int, PxlFrame> OFilledData;

		public BDic<int, Color32> OFilledColor;
	}
}
