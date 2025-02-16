using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class BDVector
	{
		public BDVector()
		{
		}

		public BDVector(float _x, float _y)
		{
			this.x = _x;
			this.y = _y;
		}

		public BDVector Set(float _x, float _y)
		{
			this.x = _x;
			this.y = _y;
			return this;
		}

		public virtual void Merge(BDVector Src)
		{
		}

		public static BDVector CreateDefault(float _x, float _y)
		{
			return new BDVector(_x, _y);
		}

		public Vector2 Convert(FnZoom fnCalcX = null, FnZoom fnCalcY = null)
		{
			return new Vector2((fnCalcX != null) ? fnCalcX(this.x) : this.x, (fnCalcY != null) ? fnCalcY(this.y) : this.y);
		}

		public override string ToString()
		{
			return this.x.ToString() + ", " + this.y.ToString();
		}

		public static void SlicePoints<T>(List<T> Apos, bool consider_loop = true) where T : BDVector
		{
			int num = Apos.Count;
			if (num <= 2)
			{
				return;
			}
			bool flag = true;
			int num2 = (consider_loop ? 3 : 2);
			while (flag && num > num2)
			{
				flag = false;
				BDVector bdvector = Apos[0];
				bool flag2 = false;
				int num3 = 0;
				while (num3 <= num && (consider_loop || num3 < num - 1))
				{
					int num4 = (num3 + 1) % num;
					BDVector bdvector2 = Apos[num4];
					float num5 = bdvector2.x - bdvector.x;
					float num6 = bdvector2.y - bdvector.y;
					if (X.IntR(num5 * 20f) == 0 && X.IntR(num6 * 20f) == 0)
					{
						bdvector.Merge(bdvector2);
						Apos.RemoveAt(num4);
						num3--;
						flag = true;
						if (--num <= num2)
						{
							break;
						}
					}
					else
					{
						bdvector = bdvector2;
					}
					num3++;
				}
				if (num <= num2)
				{
					break;
				}
				int num7 = 0;
				bdvector = Apos[0];
				float num8 = 0f;
				float num9 = 0f;
				int num10 = 0;
				while ((num10 <= num || !flag2) && (consider_loop || num10 < num - 1))
				{
					int num11 = (num10 + 1) % num;
					BDVector bdvector3 = Apos[num11];
					float num12 = bdvector3.x - bdvector.x;
					float num13 = bdvector3.y - bdvector.y;
					if (num8 != 0f || num9 != 0f)
					{
						if (!BDVector.isSameTilt(num8, num9, num12, num13))
						{
							int num14 = num10 - num7 - 1;
							if (num14 > 0)
							{
								BDVector bdvector4 = Apos[num7 % num];
								int num15 = (num7 + 1) % num;
								for (int i = 0; i < num14; i++)
								{
									int num16 = ((num15 >= num) ? 0 : num15);
									bdvector4.Merge(Apos[num16]);
									Apos.RemoveAt(num16);
									num--;
								}
								num10 -= num14;
								if (num <= num2)
								{
									flag2 = true;
									break;
								}
								flag = true;
								flag2 = false;
							}
							else
							{
								flag2 = true;
							}
							num7 = num10;
						}
						else
						{
							flag2 = false;
						}
					}
					num8 = num12;
					num9 = num13;
					bdvector = bdvector3;
					num10++;
				}
				if (!flag2 && !consider_loop)
				{
					int num17 = num - 1 - num7 - 1;
					if (num17 > 0)
					{
						BDVector bdvector5 = Apos[num7 % num];
						for (int j = 0; j < num17; j++)
						{
							bdvector5.Merge(Apos[num7 + 1]);
							Apos.RemoveAt(num7 + 1);
							num--;
						}
					}
				}
			}
		}

		private static bool isSameTilt(float pdx, float pdy, float ndx, float ndy)
		{
			if (pdx == 0f || ndx == 0f)
			{
				return pdx == 0f == (ndx == 0f);
			}
			if (pdy == 0f || ndy == 0f)
			{
				return pdy == 0f == (ndy == 0f);
			}
			pdy *= 3f;
			ndy *= 3f;
			if (X.BTW(0.989f, X.Abs(pdy), 0.111f))
			{
				pdy = (float)X.MPF(pdy > 0f);
			}
			if (X.BTW(0.989f, X.Abs(ndy), 0.111f))
			{
				ndy = (float)X.MPF(ndy > 0f);
			}
			float num = (float)X.IntR(100f * pdy / pdx / 3f);
			float num2 = (float)X.IntR(100f * ndy / ndx / 3f);
			return num == num2;
		}

		public float x;

		public float y;
	}
}
