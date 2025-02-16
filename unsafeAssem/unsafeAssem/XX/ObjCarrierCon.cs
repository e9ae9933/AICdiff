using System;
using UnityEngine;

namespace XX
{
	public class ObjCarrierCon : ObjCarrier
	{
		public ObjCarrierCon()
		{
			this.Acalced_tz = new float[8];
		}

		public override ObjCarrier Base(float x, float y)
		{
			this.conbase_x = x;
			this.conbase_y = y;
			return this;
		}

		private bool calcBase(int i, int len, float t, int real_index, Transform Trs, bool reverse)
		{
			if (len <= 1)
			{
				this.base_x = this.conbase_x;
				this.base_y = this.conbase_y;
			}
			else
			{
				int num = ((this.set_clmn == 0) ? len : this.set_clmn);
				int num2 = ((this.set_clmn == 0) ? 0 : (i / this.set_clmn));
				int num3 = ((this.set_clmn == 0) ? i : (i % this.set_clmn));
				if (this.bounds_w != 0f || this.bounds_h != 0f)
				{
					float num4 = (float)(num - 1);
					int num5 = X.IntC((float)len / (float)num);
					this.base_x = this.conbase_x;
					this.base_y = this.conbase_y;
					if (this.intv_x != 0f)
					{
						this.base_x += -0.5f * this.bounds_w + this.intv_x * (float)num3;
					}
					else if (num4 > 0f)
					{
						this.base_x += (-0.5f + (float)num3 / num4) * this.bounds_w;
					}
					if (this.intv_y != 0f)
					{
						this.base_y += -0.5f * this.bounds_h + this.intv_y * (float)num2;
					}
					else if (num5 > 1)
					{
						this.base_y += (-0.5f + (float)num2 / (float)(num5 - 1)) * this.bounds_h;
					}
				}
				else
				{
					this.base_x = this.conbase_x + this.intv_x * (float)num3;
					this.base_y = this.conbase_y + this.intv_y * (float)num2;
				}
			}
			float num6 = 1f - base.Calc(t - (float)i * this.delay, Trs, reverse);
			if (num6 >= 1f)
			{
				num6 = 1f - base.Calc(t - (float)i * this.delay, Trs, reverse);
			}
			if (reverse)
			{
				num6 = 1f - num6;
			}
			this.Acalced_tz[real_index] = num6;
			return num6 < 1f;
		}

		public bool CalcAllMb<T>(int t, T[] AMonoBehabiour, int len = -1, bool reverse = false) where T : MonoBehaviour
		{
			if (len == -1)
			{
				len = AMonoBehabiour.Length;
			}
			if (this.Acalced_tz.Length < len)
			{
				Array.Resize<float>(ref this.Acalced_tz, len);
			}
			bool flag = true;
			for (int i = 0; i < len; i++)
			{
				if (this.Acalced_tz[i] < 1f)
				{
					Transform transform = AMonoBehabiour[i].transform;
					if (this.calcBase(i, len, (float)t, i, transform, reverse))
					{
						flag = false;
					}
				}
			}
			return flag;
		}

		public bool CalcAllBtn(float t, aBtn[] ABtn, int len = -1, bool reverse = false)
		{
			if (len == -1)
			{
				len = ABtn.Length;
			}
			int num = X.Mn(len, ABtn.Length);
			if (this.Acalced_tz.Length < num)
			{
				Array.Resize<float>(ref this.Acalced_tz, num);
			}
			bool flag = true;
			if (this.carr_len < 0)
			{
				this.carr_len = len;
				for (int i = num - 1; i >= 0; i--)
				{
					aBtn aBtn = ABtn[i];
					if (!(aBtn == null) && aBtn.carr_index >= 0)
					{
						this.carr_len = X.Mx(aBtn.carr_index + 1, this.carr_len);
					}
				}
				if (this.carr_len < 0)
				{
					this.carr_len = len;
				}
			}
			for (int j = 0; j < num; j++)
			{
				if (this.Acalced_tz[j] < 1f)
				{
					aBtn aBtn2 = ABtn[j];
					if (!(aBtn2 == null) && aBtn2.carr_index >= 0)
					{
						Transform transform = aBtn2.transform;
						if (this.calcBase(aBtn2.carr_index, this.carr_len, t, j, transform, reverse))
						{
							flag = false;
						}
					}
				}
			}
			return flag;
		}

		public float getTz(float t, int i, bool reverse = false)
		{
			float num = t - this.delay * (float)i;
			float num2 = ((this.start_shift_maxt <= 0f) ? ((float)((num >= 0f) ? 1 : 0)) : X.ZLINE(num, this.start_shift_maxt));
			if (!reverse)
			{
				return num2;
			}
			return 1f - num2;
		}

		public int get_maxt()
		{
			return (int)(((this.Acalced_tz != null) ? (this.delay * (float)this.Acalced_tz.Length) : 100f) + this.start_shift_maxt);
		}

		public void refineBtnConnection<T>(T[] ABtn, int ARMX, int navi_loop = 0) where T : aBtn
		{
			if (this.carr_len <= 1)
			{
				return;
			}
			int num = ((this.set_clmn == 0) ? this.carr_len : this.set_clmn);
			int num2 = X.IntC((float)this.carr_len / (float)num);
			aBtn[,] array = new aBtn[num, num2];
			for (int i = 0; i < ARMX; i++)
			{
				aBtn aBtn = ABtn[i];
				if (aBtn == null)
				{
					break;
				}
				if (aBtn.carr_index >= 0)
				{
					int num3 = ((this.set_clmn == 0) ? 0 : (aBtn.carr_index / this.set_clmn));
					int num4 = ((this.set_clmn == 0) ? aBtn.carr_index : (aBtn.carr_index % this.set_clmn));
					if (X.BTW(0f, (float)num4, (float)num) && X.BTW(0f, (float)num3, (float)num2))
					{
						array[num4, num3] = aBtn;
					}
				}
			}
			int num5 = num - 1;
			int num6 = num2 - 1;
			bool flag = (navi_loop & 1) != 0;
			bool flag2 = (navi_loop & 2) != 0;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					aBtn aBtn2 = array[j, k];
					if (!(aBtn2 == null))
					{
						if (this.bounds_w > 0f || this.intv_x > 0f)
						{
							if (j > 0)
							{
								aBtn2.setNaviL(array[j - 1, k], true, true);
							}
							if (j < num5)
							{
								aBtn2.setNaviR(array[j + 1, k], true, true);
							}
							else if (flag)
							{
								aBtn2.setNaviR(array[0, k], true, true);
							}
						}
						else if (this.bounds_w < 0f || this.intv_x < 0f)
						{
							if (j > 0)
							{
								aBtn2.setNaviR(array[j - 1, k], true, true);
							}
							if (j < num5)
							{
								aBtn2.setNaviL(array[j + 1, k], true, true);
							}
							else if (flag)
							{
								aBtn2.setNaviL(array[0, k], true, true);
							}
						}
						if (this.bounds_h < 0f || this.intv_y < 0f)
						{
							if (k > 0)
							{
								aBtn2.setNaviT(array[j, k - 1], true, true);
							}
							if (k < num6)
							{
								aBtn2.setNaviB(array[j, k + 1], true, true);
							}
							else if (flag2)
							{
								aBtn2.setNaviB(array[j, 0], true, true);
							}
						}
						if (this.bounds_h > 0f || this.intv_y > 0f)
						{
							if (k > 0)
							{
								aBtn2.setNaviB(array[j, k - 1], true, true);
							}
							if (k < num6)
							{
								aBtn2.setNaviT(array[j, k + 1], true, true);
							}
							else if (flag2)
							{
								aBtn2.setNaviT(array[j, 0], true, true);
							}
						}
					}
				}
			}
		}

		public ObjCarrierCon Delay(float x)
		{
			this.delay = x;
			return this;
		}

		public ObjCarrierCon Bounds(float w, float h = 0f, int clmn = -1)
		{
			this.bounds_w = w;
			this.bounds_h = h;
			if (clmn >= 0)
			{
				this.set_clmn = clmn;
			}
			return this;
		}

		public ObjCarrierCon BoundsPx(float w, float h, int clmn = -1)
		{
			this.bounds_w = w * 0.015625f;
			this.bounds_h = h * 0.015625f;
			if (clmn >= 0)
			{
				this.set_clmn = clmn;
			}
			return this;
		}

		public ObjCarrierCon Intv(float x, float y, int clmn = -1)
		{
			this.intv_x = x;
			this.intv_y = y;
			if (clmn >= 0)
			{
				this.set_clmn = clmn;
			}
			return this;
		}

		public ObjCarrierCon IntvPx(float x, float y, int clmn = -1)
		{
			this.intv_x = x * 0.015625f;
			this.intv_y = y * 0.015625f;
			if (clmn >= 0)
			{
				this.set_clmn = clmn;
			}
			return this;
		}

		public ObjCarrierCon resetCalced()
		{
			int num = this.Acalced_tz.Length;
			for (int i = 0; i < num; i++)
			{
				this.Acalced_tz[i] = 0f;
			}
			this.carr_len = -1;
			return this;
		}

		public float delay;

		public int set_clmn;

		public float conbase_x;

		public float conbase_y;

		public float bounds_w;

		public float bounds_h;

		public float intv_x;

		public float intv_y;

		private float[] Acalced_tz;

		private int carr_len = -1;
	}
}
