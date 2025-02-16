using System;

namespace XX
{
	public class ReplacableString
	{
		public ReplacableString()
		{
		}

		public ReplacableString(STB Stb, int key_si, int key_len)
		{
			this.Set(Stb, key_si, key_len);
		}

		public ReplacableString(ReplacableString Src)
		{
			this.Set(Src);
		}

		public ReplacableString(string v)
		{
			this.Set(v);
		}

		public ReplacableString unbake()
		{
			this.VpBakedSrc = null;
			return this;
		}

		public ReplacableString Set(ReplacableString Src)
		{
			this.VpBakedSrc = null;
			this.source = Src.source;
			if (Src.StbForVP != null)
			{
				this.StbForVP = new STB(this.source);
			}
			return this;
		}

		public ReplacableString Set(STB Stb, int key_si, int key_len)
		{
			int num = key_si + key_len;
			Stb.SkipSpace(key_si, out key_si, num);
			Stb.TaleTrimSpace(num, out num, key_si);
			key_len = num - key_si;
			if (Stb.IndexOf('$', key_si, num) >= 0)
			{
				this.StbForVP = new STB(Stb, key_si, key_len, key_len + 8);
			}
			else
			{
				this.StbForVP = null;
			}
			this.source = Stb.ToString(key_si, key_len);
			this.VpBakedSrc = null;
			return this;
		}

		public ReplacableString Set(string v)
		{
			this.source = v;
			if (v.IndexOf('$') >= 0)
			{
				this.StbForVP = new STB(v, v.Length + 8);
			}
			else
			{
				this.StbForVP = null;
			}
			this.VpBakedSrc = null;
			return this;
		}

		public ReplacableString Recache(VariableP VP)
		{
			if (this.StbForVP != null)
			{
				this.StbForVP.Set(this.source);
				VP.Convert(this.StbForVP, 0, null);
			}
			return this;
		}

		public ReplacableString Bake(VariableP VP)
		{
			if (this.StbForVP != null && VP != null && this.VpBakedSrc != VP)
			{
				this.VpBakedSrc = VP;
				this.Recache(VP);
			}
			return this;
		}

		public void CopyBaked(VariableP VP, STB Dep)
		{
			if (this.StbForVP != null && VP != null)
			{
				this.Bake(VP);
				Dep.Set(this.StbForVP);
				return;
			}
			Dep.Set(this.source);
		}

		public STB Recache(VariableP VP, STB StbOut)
		{
			if (this.StbForVP != null)
			{
				if (VP != null)
				{
					VP.Convert(this.StbForVP.Set(this.source), 0, StbOut);
				}
				else
				{
					StbOut.Set(this.source);
				}
			}
			else
			{
				StbOut.Set(this.source);
			}
			return StbOut;
		}

		public string ToString(VariableP VP)
		{
			this.Recache(VP);
			if (this.StbForVP == null)
			{
				return this.source;
			}
			return this.StbForVP.ToString();
		}

		public override string ToString()
		{
			return this.source;
		}

		public bool use_vp
		{
			get
			{
				return this.StbForVP != null;
			}
		}

		public STB VpBaked
		{
			get
			{
				return this.StbForVP;
			}
		}

		public int Length
		{
			get
			{
				return this.source.Length;
			}
		}

		public bool calcOperandIsStr(string ope, ReplacableString B, VariableP VP)
		{
			if (B == null)
			{
				return false;
			}
			STB stb = TX.PopBld(null, 0);
			STB stb2 = TX.PopBld(null, 0);
			this.CopyBaked(VP, stb);
			B.CopyBaked(VP, stb2);
			bool flag = stb.calcOperandIsStr(ope, stb2);
			TX.ReleaseBld(stb2);
			TX.ReleaseBld(stb);
			return flag;
		}

		public string source;

		protected STB StbForVP;

		private VariableP VpBakedSrc;
	}
}
