using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel.smnp
{
	public sealed class SmnPoint
	{
		public SmnPoint(SummonerPlayer Con, float _x, float _y, float _weight, string[] Aalloc_id_key = null)
		{
			uint num = NightController.Xors.get2((uint)Con.enemy_index, (uint)(Con.enemy_index % 39));
			this.Set(Con, _x, _y, _weight, X.RAN(num, 323), Aalloc_id_key);
		}

		public SmnPoint(SummonerPlayer Con, float _x, float _y, float _weight, float _ag01, string[] Aalloc_id_key = null)
		{
			this.Set(Con, _x, _y, _weight, _ag01, Aalloc_id_key);
		}

		public SmnPoint Set(SummonerPlayer Con, float _x, float _y, float _weight, float _ag01, string[] Aalloc_id_key = null)
		{
			this.x = _x;
			this.y = _y;
			this.Lp = Con.Lp;
			if (this.Lp == null)
			{
				this.pos_cnt = -1;
			}
			this.weight0 = ((_weight <= 0f) ? 1f : _weight);
			this.pos_agR = ((float)Con.enemy_index * 0.133f + _ag01) * 6.2831855f;
			if (Aalloc_id_key != null && Aalloc_id_key.Length != 0)
			{
				this.Aalloc_id = Aalloc_id_key;
				for (int i = this.Aalloc_id.Length - 1; i >= 0; i--)
				{
					this.Aalloc_id[i] = this.Aalloc_id[i].ToUpper();
				}
			}
			return this;
		}

		public bool available(float weight_add)
		{
			return this.weight0 + weight_add * this.weight0 - this.use_weight >= 1f;
		}

		public bool idMatch(string e)
		{
			if (this.Aalloc_id != null)
			{
				for (int i = this.Aalloc_id.Length - 1; i >= 0; i--)
				{
					string text = this.Aalloc_id[i];
					if (NDAT.isSame(e, text, false))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void AddTo(List<SmnPoint> ABuf, float weight_add)
		{
			int num = (int)(this.weight0 + weight_add * this.weight0 - this.use_weight);
			while (--num >= 0)
			{
				ABuf.Add(this);
			}
		}

		public float x;

		public float y;

		public float weight0;

		public float use_weight;

		public string[] Aalloc_id;

		public int pos_cnt;

		public float pos_agR;

		public float shuffle_ratio = 1f;

		public bool sudden_appear;

		public M2LabelPoint Lp;
	}
}
