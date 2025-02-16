using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public sealed class BtnContainerRunner : MonoBehaviourAutoRun, IPauseable, IVariableObject, IAlphaSetable, IDesignerBlock, IValotileSetable
	{
		protected override bool runIRD(float fcnt)
		{
			if (this.BCon == null)
			{
				return false;
			}
			this.t += fcnt;
			this.BCon.runCarrier(this.t, null);
			return true;
		}

		public void Pause()
		{
			this.BCon.hide(false, false);
		}

		public void Resume()
		{
			this.BCon.bind(false, false);
		}

		public string getValueString()
		{
			return this.BCon.getValueString();
		}

		public void setValue(string s)
		{
			this.BCon.setValue(s);
		}

		public aBtn Get(int i)
		{
			return this.BCon.Get(i);
		}

		public aBtn Get(string s)
		{
			return this.BCon.Get(s);
		}

		public void setAlpha(float f)
		{
			this.BCon.setAlpha(f);
		}

		public GameObject getGob()
		{
			return base.gameObject;
		}

		public float get_swidth_px()
		{
			return this.BCon.get_swidth_px();
		}

		public float get_sheight_px()
		{
			return this.BCon.get_sheight_px();
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			this.BCon.AddSelectableItems(ASlc, only_front);
		}

		public Transform getTransform()
		{
			return this.BCon.getTransform();
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				this.use_valotile_ = value;
				this.BCon.use_valotile = value;
			}
		}

		public BtnContainer<aBtn> BCon;

		private float t;

		public bool use_valotile_;
	}
}
