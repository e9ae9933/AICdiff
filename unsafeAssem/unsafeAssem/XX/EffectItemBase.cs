using System;

namespace XX
{
	public class EffectItemBase
	{
		public float addaf(float fcnt)
		{
			this.af += fcnt;
			return this.af;
		}

		public EffectItemBase(string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0)
		{
			this.clearBase(_title, _x, _y, _z, _time);
		}

		public virtual void destruct()
		{
		}

		public void clearBase(string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0)
		{
			this.af = 0f;
			this.title = _title;
			this.x = _x;
			this.y = _y;
			this.z = _z;
			this.time = _time;
			this.index = EffectItemBase.index_count++;
			this.f0 = IN.totalframe;
		}

		public bool isTitle(string _title)
		{
			string text = this.title;
			if (text == "")
			{
				return false;
			}
			char[] array = text.ToCharArray();
			if (array[0] == '+' || array[0] == '-')
			{
				text = text.Substring(1);
			}
			return _title == text;
		}

		public override string ToString()
		{
			return "EFI - " + this.title + " (" + EffectItemBase.index_count.ToString();
		}

		public float x;

		public float y;

		public uint index;

		public string title;

		public static uint index_count;

		public int time;

		public float af;

		public float z;

		public int f0;
	}
}
