using System;
using UnityEngine;

namespace XX
{
	public class ShakeChecker
	{
		public ShakeChecker()
		{
			this.ASpd = new Vector2[40];
		}

		public bool update(Vector2 WorldPos)
		{
			bool flag = false;
			bool flag2 = false;
			float num = WorldPos.x * 64f;
			float num2 = WorldPos.y * 64f;
			if (this.cpos < 0)
			{
				flag = true;
				this.cpos = 0;
			}
			int num3 = this.cpos;
			int num4 = this.cpos + 1;
			this.cpos = num4;
			if (num4 >= 40)
			{
				this.cpos = 0;
			}
			if (flag)
			{
				this.nx = num;
				this.ny = num2;
			}
			else
			{
				Vector2 vector = this.ASpd[this.cpos];
				Vector2 vector2 = this.ASpd[num3];
				vector.x = num - this.nx;
				vector.y = num2 - this.ny;
				this.nx = num;
				this.ny = num2;
				if ((vector.x >= (float)this.threshold_pixel && vector2.x <= 0f) || (vector.x <= (float)(-(float)this.threshold_pixel) && vector2.x >= 0f) || (vector.y >= (float)this.threshold_pixel && vector2.y <= 0f) || (vector.y <= (float)(-(float)this.threshold_pixel) && vector2.y >= 0f))
				{
					bool flag3 = vector.x > 0f;
					bool flag4 = vector.y > 0f;
					float num5 = (float)this.threshold_pixel * 0.3f;
					float num6 = (float)((vector.x == 0f) ? (-1) : 0);
					float num7 = (float)((vector.y == 0f) ? (-1) : 0);
					int num8 = ((vector.x >= (float)this.threshold_pixel) ? 1 : ((vector.x <= (float)(-(float)this.threshold_pixel)) ? 2 : 0));
					int num9 = ((vector.y >= (float)this.threshold_pixel) ? 1 : ((vector.y <= (float)(-(float)this.threshold_pixel)) ? 2 : 0));
					int num10 = num3;
					while (num10 != this.cpos)
					{
						Vector2 vector3 = this.ASpd[num10];
						if (num6 >= 0f && X.Abs(vector3.x) >= num5 && vector3.x > 0f != flag3)
						{
							flag3 = vector3.x > 0f;
							num8 |= ((vector3.x >= (float)this.threshold_pixel) ? 1 : ((vector3.x <= (float)(-(float)this.threshold_pixel)) ? 2 : 0));
							if ((num6 += 1f) >= (float)this.orikaesi_count && num8 == 3)
							{
								break;
							}
						}
						if (num7 >= 0f && X.Abs(vector3.y) >= num5 && vector3.y > 0f != flag4)
						{
							flag4 = vector3.y > 0f;
							num9 |= ((vector3.y >= (float)this.threshold_pixel) ? 1 : ((vector3.y <= (float)(-(float)this.threshold_pixel)) ? 2 : 0));
							if ((num7 += 1f) >= (float)this.orikaesi_count && num9 == 3)
							{
								break;
							}
						}
						if (--num10 < 0)
						{
							num10 = 39;
						}
					}
					if ((num6 >= (float)this.orikaesi_count && num8 == 3) || (num7 >= (float)this.orikaesi_count && num9 == 3))
					{
						flag2 = this.shaked_t == 0;
						this.shaked_t = this.shake_continue_time;
					}
				}
			}
			if (!flag2 && this.shaked_t > 0)
			{
				num4 = this.shaked_t - 1;
				this.shaked_t = num4;
				if (num4 == 0)
				{
					flag2 = true;
				}
			}
			return flag2;
		}

		public bool shaked
		{
			get
			{
				return this.shaked_t > 0;
			}
		}

		public int shaked_t;

		public int shake_continue_time = 150;

		public int threshold_pixel = 20;

		public int orikaesi_count = 3;

		private Vector2[] ASpd;

		private const int PMAX = 40;

		private int cpos = -1;

		private float nx;

		private float ny;
	}
}
