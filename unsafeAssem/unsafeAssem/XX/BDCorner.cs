using System;

namespace XX
{
	public class BDCorner
	{
		public BDCorner(float vpx, float vpy, int a, int vpz)
		{
			this.Set(vpx, vpy, a, vpz);
		}

		public BDCorner Set(float vpx, float vpy, int a, int vpz)
		{
			this.x = vpx;
			this.y = vpy;
			this.aim = a;
			this.z = vpz;
			return this;
		}

		public bool block_first
		{
			get
			{
				return (this.z & 1) > 0;
			}
		}

		public float border_x
		{
			get
			{
				return this.x;
			}
		}

		public float border_y
		{
			get
			{
				return this.y;
			}
		}

		public void translate(int _x, int _y)
		{
			this.x += (float)_x;
			this.y += (float)_y;
		}

		public override string ToString()
		{
			string[] array = new string[6];
			array[0] = "<BDCorner> ";
			array[1] = this.x.ToString();
			array[2] = ",";
			array[3] = this.y.ToString();
			array[4] = " =>a: ";
			int num = 5;
			string text;
			if (this.aim < 0)
			{
				text = this.aim.ToString();
			}
			else
			{
				AIM aim = (AIM)this.aim;
				text = aim.ToString();
			}
			array[num] = text;
			return string.Concat(array);
		}

		public float x;

		public float y;

		public int aim;

		public int z;
	}
}
