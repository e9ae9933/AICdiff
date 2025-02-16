using System;
using Better;
using XX;

namespace m2d
{
	public sealed class M2NoDamageManager
	{
		public static bool isParryDisableKey(NDMG key)
		{
			return key - NDMG.MAPDAMAGE <= 3U || key - NDMG.PRESSDAMAGE <= 3U;
		}

		public static bool isMapDamageKey(NDMG key)
		{
			return key - NDMG.MAPDAMAGE <= 3U;
		}

		public static bool isPenetrateDefault(NDMG key)
		{
			return key - NDMG.MAPDAMAGE_LAVA <= 4U || key - NDMG.GRAB_PENETRATE <= 2U;
		}

		public M2NoDamageManager AddBurstPrevent(float _time)
		{
			for (int i = 12; i >= 0; i--)
			{
				NDMG ndmg = (NDMG)i;
				if (!M2NoDamageManager.isPenetrateDefault(ndmg) || ndmg == NDMG.MAPDAMAGE_THUNDER)
				{
					this.Add(ndmg, _time);
				}
			}
			return this;
		}

		public M2NoDamageManager AddAll(float _time, uint ignore_bits = 0U)
		{
			for (int i = 12; i >= 0; i--)
			{
				if ((ignore_bits & (1U << i)) == 0U)
				{
					this.Add((NDMG)i, _time);
				}
			}
			return this;
		}

		public M2NoDamageManager(Map2d Mp)
		{
			this.Ond = new BDic<NDMG, M2NoDamageManager.NDItm>(2);
			this.initS(Mp);
			this.Add(0f);
		}

		public M2NoDamageManager initS(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Clear();
			return this;
		}

		public M2NoDamageManager Add(float time)
		{
			return this.Add(NDMG.DEFAULT, time);
		}

		public M2NoDamageManager Add(NDMG key = NDMG.DEFAULT, float _time = 0f)
		{
			if (key == NDMG._ALL)
			{
				return this.AddAll(_time, 0U);
			}
			if (key == NDMG._BURST_PREVENT)
			{
				this.Clear();
				return this.AddBurstPrevent(_time);
			}
			M2NoDamageManager.NDItm nditm;
			if (this.Ond.TryGetValue(key, out nditm))
			{
				nditm.time = X.Mx(this.Mp.floort + _time, nditm.time);
			}
			else
			{
				nditm = new M2NoDamageManager.NDItm(this.Mp.floort + _time);
			}
			this.Ond[key] = nditm;
			if (_time > 0f)
			{
				this.active_bits |= ((key == NDMG.DEFAULT) ? 8191U : (1U << (int)key));
			}
			return this;
		}

		public M2NoDamageManager Clear()
		{
			this.Ond.Clear();
			this.active_bits = 0U;
			this.Ond[NDMG.DEFAULT] = new M2NoDamageManager.NDItm(0f);
			return this;
		}

		public bool isActive()
		{
			uint num = 13U;
			if (this.active_bits == 0U)
			{
				return false;
			}
			for (uint num2 = 0U; num2 < num; num2 += 1U)
			{
				if ((this.active_bits & (1U << (int)num2)) != 0U && this.isActive((NDMG)num2))
				{
					return true;
				}
			}
			return false;
		}

		public bool isActive(NDMG key)
		{
			if ((this.active_bits & (1U << (int)key)) == 0U)
			{
				return false;
			}
			if (key == NDMG._ALL)
			{
				return this.isActive();
			}
			bool flag = false;
			M2NoDamageManager.NDItm nditm;
			if (this.Ond.TryGetValue(key, out nditm) && nditm.time > this.Mp.floort)
			{
				flag = true;
			}
			else if (key != NDMG.DEFAULT)
			{
				flag = !M2NoDamageManager.isPenetrateDefault(key) && this.isActiveDefault();
			}
			if (!flag)
			{
				this.active_bits &= ~(1U << (int)key);
			}
			return flag;
		}

		public bool isActiveDefault()
		{
			if ((this.active_bits & 4096U) == 0U)
			{
				return false;
			}
			M2NoDamageManager.NDItm nditm;
			bool flag = this.Ond.TryGetValue(NDMG.DEFAULT, out nditm) && nditm.time > this.Mp.floort;
			if (!flag)
			{
				this.active_bits &= 4294963199U;
			}
			return flag;
		}

		public M2NoDamageManager Penetrate(NDMG key, int reduce = -1)
		{
			if (key == NDMG._ALL)
			{
				return this.Clear();
			}
			M2NoDamageManager.NDItm nditm;
			if (this.Ond.TryGetValue(key, out nditm))
			{
				nditm.time = ((reduce < 0) ? this.Mp.floort : (nditm.time - (float)reduce));
				if (nditm.time <= this.Mp.floort && key != NDMG.DEFAULT)
				{
					this.Ond.Remove(key);
				}
				else
				{
					this.Ond[key] = nditm;
				}
			}
			return this;
		}

		public Map2d Mp;

		private BDic<NDMG, M2NoDamageManager.NDItm> Ond;

		private uint active_bits;

		private struct NDItm
		{
			public NDItm(float _time)
			{
				this.time = _time;
			}

			public float time;
		}
	}
}
