using System;

namespace XX
{
	public class EfParticleVarContainer
	{
		public virtual string key
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public virtual EfParticleVarContainer SetV(string k, string val)
		{
			return this;
		}

		public virtual EfParticleVarContainer initParticle()
		{
			return this;
		}

		public virtual object GetV(string k, object default_val)
		{
			return null;
		}

		public virtual object GetV0D(string k, object default_val, bool default_on_false = true)
		{
			return this.GetV(k, default_val);
		}

		public virtual bool IsSet(string k)
		{
			return false;
		}

		public object this[string s]
		{
			get
			{
				return this.GetV(s, 0);
			}
		}

		public virtual EfParticleLoader getLoader()
		{
			return null;
		}
	}
}
