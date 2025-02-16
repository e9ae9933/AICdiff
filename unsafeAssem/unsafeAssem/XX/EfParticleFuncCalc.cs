using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XX
{
	public sealed class EfParticleFuncCalc
	{
		public EfParticleFuncCalc(string tx0, string default_fn = "ZLINE", float end_rate = 1f)
		{
			if (tx0 != null)
			{
				this.Remake(tx0, default_fn, end_rate);
			}
		}

		public EfParticleFuncCalc Remake(string tx0, string default_fn = "ZLINE", float end_rate = 1f)
		{
			List<EfParticleFuncCalcItem> list = new List<EfParticleFuncCalcItem>(1);
			this.script = tx0;
			string[] array = EfParticleFuncCalc.RegSplitter.Split(tx0);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				string text = array[i];
				string text2 = default_fn;
				float num2 = 0f;
				float num3 = 1f;
				float num4 = 1f;
				bool flag = true;
				if (REG.match(text, EfParticleFuncCalc.RegNumber))
				{
					list.Add(new EfParticleFuncCalcItem("Z1", 0f, 1f, X.Nm(REG.R1, 0f, true), false));
				}
				else
				{
					if (REG.match(text, EfParticleFuncCalc.RegTime))
					{
						text = REG.otherContext;
						flag = false;
						num2 = X.Nm(REG.R1, 0f, true);
						num3 = X.Nm(REG.R2, -1000f, true);
						if (num3 != -1000f)
						{
							num3 *= (float)EfParticle.frame_rate;
						}
					}
					else if (REG.match(text, EfParticleFuncCalc.RegTimeRate))
					{
						text = REG.otherContext;
						num2 = X.Nm(REG.R1, 0f, true);
						num3 = X.Nm(REG.R2, end_rate, true);
					}
					if (REG.match(text, EfParticleFuncCalc.RegMulti))
					{
						num4 = X.Nm(REG.R1, 1f, true);
						text = REG.otherContext;
					}
					if (REG.match(text, EfParticleFuncCalc.RegFunc))
					{
						text2 = REG.R1;
						text = REG.otherContext;
					}
					list.Add(new EfParticleFuncCalcItem(text2, num2, num3, num4, flag));
				}
			}
			this.AItm = list.ToArray();
			return this;
		}

		public float Get(float maxt, float ratio = -1f, bool write_prevalue = false)
		{
			float num = 0f;
			if (ratio == -2f)
			{
				return this.pre_value;
			}
			int num2 = this.AItm.Length;
			for (int i = 0; i < num2; i++)
			{
				num += this.AItm[i].Get(maxt, ratio);
			}
			if (write_prevalue)
			{
				this.pre_value = num;
			}
			return num;
		}

		public override string ToString()
		{
			return this.script;
		}

		private EfParticleFuncCalcItem[] AItm;

		public string script;

		private static readonly Regex RegTime = new Regex("<< *(\\-?[\\.\\d]+)? *\\.\\. *(\\-?[\\.\\d]+)? *>+");

		private static readonly Regex RegTimeRate = new Regex("< *(\\-?[\\.\\d]+)? *\\.\\. *(\\-?[\\.\\d]+)? *>");

		private static readonly Regex RegMulti = new Regex("\\* *(\\-?[\\d]+(?:\\.[\\d]+)?)");

		private static readonly Regex RegFunc = new Regex("^ *(\\w+)");

		private static readonly Regex RegNumber = new Regex("^ *(\\-?[\\d]+(?:\\.[\\d]+)?) *$");

		private static readonly Regex RegSplitter = new Regex(" *\\|+ *");

		public float pre_value;
	}
}
