using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class VPts
	{
		public VPts()
		{
			this.APt = new List<Vector3>();
		}

		public VPts Add(Vector2 P)
		{
			this.APt.Add(P);
			return this;
		}

		public VPts Add(Vector2[] AList, int len = -1)
		{
			if (len < 0)
			{
				len = AList.Length;
			}
			for (int i = 0; i < len; i++)
			{
				this.Add(AList[i]);
			}
			return this;
		}

		public VPts Add(List<BDVector> AList, int len = -1, FnZoom fnCalcX = null, FnZoom fnCalcY = null)
		{
			if (len < 0)
			{
				len = AList.Count;
			}
			for (int i = 0; i < len; i++)
			{
				BDVector bdvector = AList[i];
				this.Add(bdvector.Convert(fnCalcX, fnCalcY));
			}
			return this;
		}

		public VPts Clear()
		{
			this.APt.Clear();
			return this;
		}

		public VPts Reverse()
		{
			this.APt.Reverse();
			return this;
		}

		public int Count
		{
			get
			{
				return this.APt.Count;
			}
		}

		public Vector3 Get(int i)
		{
			return this.APt[i];
		}

		public Vector3[] ToArray()
		{
			return this.APt.ToArray();
		}

		public Vector2[] ToArrayV2()
		{
			int count = this.APt.Count;
			Vector2[] array = new Vector2[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = this.APt[i];
			}
			return array;
		}

		public VPts removeDupe(bool consider_loop = true)
		{
			int num = this.APt.Count;
			int num2 = 0;
			while (num2 < num && num > 2 && (consider_loop || num2 < num - 1))
			{
				Vector3 vector = this.APt[num2];
				Vector3 vector2 = ((num2 == num - 1) ? this.APt[0] : this.APt[num2 + 1]);
				if (vector.x == vector2.x && vector.y == vector2.y)
				{
					this.APt.RemoveAt(num2);
					num--;
				}
				else
				{
					num2++;
				}
			}
			num2 = (consider_loop ? 0 : 1);
			while (num2 < num && num > 2)
			{
				Vector3 vector3 = this.APt[(num2 == 0) ? (num - 1) : (num2 - 1)];
				Vector3 vector4 = this.APt[num2];
				VPts.BufPt0 = VPts.setSaisyoKoyakusu(VPts.BufPt0, X.IntR((vector4.x - vector3.x) * (float)this.resolution), X.IntR((vector4.y - vector3.y) * (float)this.resolution));
				while (num > 2 && (consider_loop || num2 < num - 1))
				{
					Vector3 vector5 = this.APt[(num2 + 1) % num];
					VPts.BufPt1 = VPts.setSaisyoKoyakusu(VPts.BufPt1, X.IntR((vector5.x - vector4.x) * (float)this.resolution), X.IntR((vector5.y - vector4.y) * (float)this.resolution));
					if (VPts.BufPt0.x == VPts.BufPt1.x && VPts.BufPt0.y == VPts.BufPt1.y)
					{
						this.APt.RemoveAt(num2);
						vector4 = vector5;
						VPts.BufPt0 = VPts.setSaisyoKoyakusu(VPts.BufPt0, X.IntR(vector4.x - vector3.x) * this.resolution, X.IntR(vector4.y - vector3.y) * this.resolution);
						num--;
					}
					else
					{
						if (VPts.BufPt0.x != -VPts.BufPt1.x || VPts.BufPt0.y != -VPts.BufPt1.y)
						{
							break;
						}
						this.APt.RemoveAt((num2 + 1) % num);
						num--;
					}
				}
				num2++;
			}
			return this;
		}

		private static Vector3 setSaisyoKoyakusu(Vector3 Buf, int x, int y)
		{
			int num = X.MPF(x > 0);
			int num2 = X.MPF(y > 0);
			if (x == 0)
			{
				Buf.Set(0f, (float)((y != 0) ? num2 : 0), Buf.z);
				return Buf;
			}
			if (y == 0)
			{
				Buf.Set((float)((x != 0) ? num : 0), 0f, Buf.z);
				return Buf;
			}
			x *= num;
			y *= num2;
			for (;;)
			{
				int num3 = X.yakusu(x, y);
				if (num3 <= 1)
				{
					break;
				}
				x /= num3;
				y /= num3;
			}
			Buf.Set((float)(x * num), (float)(y * num2), Buf.z);
			return Buf;
		}

		protected List<Vector3> APt;

		private static Vector3 BufPt0;

		private static Vector3 BufPt1;

		public int resolution = 28;
	}
}
