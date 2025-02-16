using System;
using UnityEngine;

namespace XX
{
	public abstract class BtnContainerBasic : IPauseable, IVariableObject
	{
		public BtnContainerBasic(GameObject _Base)
		{
			this.Base = _Base;
		}

		public virtual bool runCarrier(float t, ObjCarrierCon _Carr = null)
		{
			return true;
		}

		public BtnContainerRunner getRunner()
		{
			return this.Base.GetComponent<BtnContainerRunner>();
		}

		public abstract void Pause();

		public abstract void Resume();

		public abstract string getValueString();

		public abstract void setValue(string s);

		public GameObject getGob()
		{
			return this.Base;
		}

		public void replaceGobTarget(GameObject Gob)
		{
			this.Base = Gob;
		}

		public virtual int Length
		{
			get
			{
				return 0;
			}
		}

		public abstract aBtn GetButton(int i);

		public virtual void destruct()
		{
			if (this.Base != null)
			{
				global::UnityEngine.Object @base = this.Base;
				this.Base = null;
				IN.DestroyOne(@base);
			}
		}

		protected GameObject Base;

		public ScrollBox BelongScroll;

		public bool carr_alpha_tz = true;

		public bool carr_hiding_reverse;

		public int default_focus = -1;

		public int stencil_ref = -1;
	}
}
