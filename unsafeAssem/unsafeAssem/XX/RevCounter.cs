using System;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class RevCounter
	{
		public virtual RevCounter Add(float addval, bool once_in_one_frame = false, bool soft = false)
		{
			if (once_in_one_frame && this.val > 0f)
			{
				return this;
			}
			if (soft)
			{
				this.val += (float)X.MPF(this.val >= 0f) * addval;
			}
			else
			{
				this.val = ((this.val < 0f) ? (-this.val) : this.val) + addval;
			}
			return this;
		}

		public RevCounter Add(RevCounter _Addval, bool once_in_one_frame = false)
		{
			return this.Add(_Addval.val, once_in_one_frame, false);
		}

		public RevCounter AddMn(float addval, float mn, bool once_in_one_frame = false)
		{
			if (once_in_one_frame && this.val > 0f)
			{
				return this;
			}
			this.val = X.Mn(mn, ((this.val < 0f) ? (-this.val) : this.val) + addval);
			return this;
		}

		public RevCounter Max(float t, bool once_in_one_frame = false)
		{
			if (once_in_one_frame && this.val > 0f)
			{
				return this;
			}
			this.val = X.Mx((this.val < 0f) ? (-this.val) : this.val, t);
			return this;
		}

		public virtual RevCounter Clear()
		{
			this.val = 0f;
			return this;
		}

		public RevCounter Set(float _t, bool soft = false)
		{
			if (soft)
			{
				this.val = (float)X.MPF(this.val >= 0f) * _t;
			}
			else
			{
				this.val = _t;
			}
			return this;
		}

		public RevCounter Set(RevCounter _Src)
		{
			this.val = _Src.val;
			return this;
		}

		public virtual RevCounter Update(float fcnt)
		{
			if (this.val > 0f)
			{
				this.val *= -1f;
			}
			else if (this.val < 0f)
			{
				this.val = X.Mn(this.val + fcnt, 0f);
			}
			return this;
		}

		public bool isAdding()
		{
			return this.val > 0f;
		}

		public bool Equals(float _v)
		{
			return X.Abs(this.val) == _v;
		}

		public bool Equals(int _v)
		{
			return X.Abs(this.val) == (float)_v;
		}

		public bool Equals(uint _v)
		{
			return X.Abs(this.val) == _v;
		}

		public override int GetHashCode()
		{
			ByteArray byteArray = new ByteArray(8U);
			byteArray.writeFloat(this.val);
			byteArray.position = 0UL;
			return byteArray.readInt();
		}

		public float Get()
		{
			return X.Abs(this.val);
		}

		public int CompareTo(RevCounter obj)
		{
			float num = this.val - obj.val;
			if (num == 0f)
			{
				return 0;
			}
			if (num >= 0f)
			{
				return 1;
			}
			return -1;
		}

		public int CompareTo(float obj)
		{
			float num = this.val - obj;
			if (num == 0f)
			{
				return 0;
			}
			if (num >= 0f)
			{
				return 1;
			}
			return -1;
		}

		public int CompareTo(int obj)
		{
			float num = this.val - (float)obj;
			if (num == 0f)
			{
				return 0;
			}
			if (num >= 0f)
			{
				return 1;
			}
			return -1;
		}

		public int CompareTo(uint obj)
		{
			float num = this.val - obj;
			if (num == 0f)
			{
				return 0;
			}
			if (num >= 0f)
			{
				return 1;
			}
			return -1;
		}

		public bool isPreFrameInputted()
		{
			return this.val > 0f;
		}

		public static RevCounter operator +(RevCounter RC, float _v)
		{
			return RC.Add(_v, false, false);
		}

		public static bool operator >(RevCounter RC, float _v)
		{
			return X.Abs(RC.val) > _v;
		}

		public static bool operator <(RevCounter RC, float _v)
		{
			return X.Abs(RC.val) < _v;
		}

		public static bool operator >=(RevCounter RC, float _v)
		{
			return X.Abs(RC.val) >= _v;
		}

		public static bool operator <=(RevCounter RC, float _v)
		{
			return X.Abs(RC.val) <= _v;
		}

		public override string ToString()
		{
			return this.val.ToString();
		}

		protected float val;
	}
}
