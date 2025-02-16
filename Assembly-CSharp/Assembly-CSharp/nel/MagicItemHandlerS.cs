using System;

namespace nel
{
	public struct MagicItemHandlerS
	{
		public MagicItemHandlerS(MagicItem _Mg)
		{
			this.id = _Mg.id;
			this.Mg = _Mg;
		}

		public bool isActive(M2MagicCaster Caster)
		{
			return this.Mg != null && this.Mg.isActive(Caster, this.id);
		}

		public void destruct(M2MagicCaster Caster)
		{
			if (this.Mg != null && this.Mg.isActive(Caster, this.id))
			{
				this.Mg.kill(-1f);
			}
			this.id = -1;
			this.Mg = null;
		}

		public void release()
		{
			this.Mg = null;
		}

		public float sx
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.sx;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.sx = value;
				}
			}
		}

		public float sy
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.sy;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.sy = value;
				}
			}
		}

		public float sz
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.sz;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.sz = value;
				}
			}
		}

		public float sa
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.sa;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.sa = value;
				}
			}
		}

		public float dx
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.dx;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.dx = value;
				}
			}
		}

		public float dy
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.dy;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.dy = value;
				}
			}
		}

		public float dz
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.dz;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.dz = value;
				}
			}
		}

		public float da
		{
			get
			{
				if (this.Mg == null)
				{
					return 0f;
				}
				return this.Mg.da;
			}
			set
			{
				if (this.Mg != null)
				{
					this.Mg.da = value;
				}
			}
		}

		public int id;

		public MagicItem Mg;
	}
}
