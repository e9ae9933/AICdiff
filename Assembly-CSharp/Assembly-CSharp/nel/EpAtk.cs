using System;
using XX;

namespace nel
{
	public class EpAtk
	{
		public EpAtk(int _val, string _situation_key)
		{
			this.val = _val;
			this.situation_key = _situation_key;
		}

		public EpAtk Situation(string _s)
		{
			this.situation_key = _s;
			return this;
		}

		public EpAtk MultipleOrgasm(float val)
		{
			this.multiple_orgasm = val;
			return this;
		}

		public EpAtk Clear()
		{
			X.ALL0(this.Atarget);
			this.target_max = 0;
			this.target_bits = (EPCATEG_BITS)0;
			return this;
		}

		public EpAtk Recalc()
		{
			this.target_max = 0;
			this.target_bits = (EPCATEG_BITS)0;
			for (int i = 0; i < 11; i++)
			{
				int num = (int)this.Atarget[i];
				if (num > 0)
				{
					this.target_max += num;
					this.target_bits |= (EPCATEG_BITS)(1 << i);
				}
			}
			return this;
		}

		public EpAtk CopyFrom(EpAtk Src)
		{
			for (int i = 0; i < 11; i++)
			{
				this.Atarget[i] = (byte)X.Mn(255, (int)(this.Atarget[i] + Src.Atarget[i]));
			}
			return this.Recalc();
		}

		public EpAtk Randomize(float min_l, float max_r)
		{
			for (int i = 0; i < 11; i++)
			{
				this.Atarget[i] = (byte)X.Mn(255f, (float)this.Atarget[i] + X.NIXP(0.75f, 1.5f));
			}
			return this.Recalc();
		}

		public byte Get(int i)
		{
			return this.Atarget[i];
		}

		public EpAtk Set(int i, byte value)
		{
			this.target_max += (int)(value - this.Atarget[i]);
			this.Atarget[i] = value;
			if (value > 0)
			{
				this.target_bits |= (EPCATEG_BITS)(1 << i);
			}
			return this;
		}

		public bool only_anal
		{
			get
			{
				return this.target_bits == EPCATEG_BITS.ANAL;
			}
		}

		public byte cli
		{
			get
			{
				return this.Atarget[0];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[0]);
				this.Atarget[0] = value;
				this.target_bits |= EPCATEG_BITS.CLI;
			}
		}

		public byte vagina
		{
			get
			{
				return this.Atarget[1];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[1]);
				this.Atarget[1] = value;
				this.target_bits |= EPCATEG_BITS.VAGINA;
			}
		}

		public byte anal
		{
			get
			{
				return this.Atarget[2];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[2]);
				this.Atarget[2] = value;
				this.target_bits |= EPCATEG_BITS.ANAL;
			}
		}

		public byte breast
		{
			get
			{
				return this.Atarget[3];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[3]);
				this.Atarget[3] = value;
				this.target_bits |= EPCATEG_BITS.BREAST;
			}
		}

		public byte mouth
		{
			get
			{
				return this.Atarget[4];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[4]);
				this.Atarget[4] = value;
				this.target_bits |= EPCATEG_BITS.MOUTH;
			}
		}

		public byte ear
		{
			get
			{
				return this.Atarget[5];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[5]);
				this.Atarget[5] = value;
				this.target_bits |= EPCATEG_BITS.EAR;
			}
		}

		public byte canal
		{
			get
			{
				return this.Atarget[6];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[6]);
				this.Atarget[6] = value;
				this.target_bits |= EPCATEG_BITS.CANAL;
			}
		}

		public byte gspot
		{
			get
			{
				return this.Atarget[7];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[7]);
				this.Atarget[7] = value;
				this.target_bits |= EPCATEG_BITS.GSPOT;
			}
		}

		public byte uterus
		{
			get
			{
				return this.Atarget[8];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[8]);
				this.Atarget[8] = value;
				this.target_bits |= EPCATEG_BITS.UTERUS;
			}
		}

		public byte urethra
		{
			get
			{
				return this.Atarget[9];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[9]);
				this.Atarget[9] = value;
				this.target_bits |= EPCATEG_BITS.URETHRA;
			}
		}

		public byte other
		{
			get
			{
				return this.Atarget[10];
			}
			set
			{
				this.target_max += (int)(value - this.Atarget[10]);
				this.Atarget[10] = value;
				this.target_bits |= EPCATEG_BITS.OTHER;
			}
		}

		public EPCATEG getRandom()
		{
			if (this.target_bits == (EPCATEG_BITS)0)
			{
				return EPCATEG.OTHER;
			}
			int num = X.bit_count((uint)this.target_bits);
			int num2 = X.bit_on_index((uint)this.target_bits, X.xors(num));
			if (num2 >= 0)
			{
				return (EPCATEG)num2;
			}
			return EPCATEG.OTHER;
		}

		public string situation_key;

		public int val;

		private byte[] Atarget = new byte[11];

		public int target_max;

		public EPCATEG_BITS target_bits;

		public float multiple_orgasm;
	}
}
