using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class UiBoxDesignerFamily : MonoBehaviour, IAlphaSetable, IRunAndDestroy, IValotileSetable
	{
		protected virtual bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned_ == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					IN.addRunner(this);
					return;
				}
				IN.remRunner(this);
			}
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				this._tostring = base.gameObject.name;
			}
			return this._tostring;
		}

		public void OnEnable()
		{
			this.runner_assigned = true;
		}

		public void OnDisable()
		{
			this.runner_assigned = false;
		}

		public virtual void OnDestroy()
		{
			this.destruct();
		}

		public virtual void destruct()
		{
			this.OnDisable();
		}

		public void DestroyDesigners()
		{
			for (int i = this.ADs.Count - 1; i >= 0; i--)
			{
				IN.DestroyOne(this.ADs[i].gameObject);
			}
			this.ADs.Clear();
		}

		protected virtual void Awake()
		{
			this.ADs = new List<UiBoxDesigner>();
		}

		bool IRunAndDestroy.run(float fcnt)
		{
			this.runIRD(fcnt);
			return true;
		}

		public virtual bool runIRD(float fcnt)
		{
			int count = this.ADs.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				flag = this.ADs[i].run(fcnt) || flag;
			}
			if (!flag && this.auto_deactive_gameobject)
			{
				base.gameObject.SetActive(false);
			}
			return flag;
		}

		public UiBoxDesigner Create(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			return this.CreateT<UiBoxDesigner>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, mask);
		}

		public virtual T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX) where T : UiBoxDesigner
		{
			T t = IN.CreateGob(base.gameObject, "-" + name).AddComponent<T>();
			t.positionD(pixel_x, pixel_y, appear_dir_aim, appear_len);
			t.WH(pixel_w, pixel_h);
			t.auto_assign_runner = false;
			IN.setZ(t.transform, this.base_z);
			this.base_z += -this.slip_z;
			if (mask == UiBoxDesignerFamily.MASKTYPE.BOX)
			{
				t.box_stencil_ref_mask = 200 + this.ADs.Count;
			}
			else if (mask == UiBoxDesignerFamily.MASKTYPE.SCROLL)
			{
				t.stencil_ref = 200 + this.ADs.Count;
			}
			else
			{
				t.box_stencil_ref_mask = -1;
				t.stencil_ref = -1;
			}
			t.enabled = false;
			t.auto_enable = false;
			if (this.use_valotile_)
			{
				t.use_valotile = true;
			}
			this.ADs.Add(t);
			return t;
		}

		public void setAutoActivate(UiBoxDesigner B, bool flag = false)
		{
			int num = this.ADs.IndexOf(B);
			if (num >= 0)
			{
				if (flag)
				{
					this.auto_activate |= 1U << num;
					return;
				}
				this.auto_activate &= ~(1U << num);
			}
		}

		public virtual UiBoxDesignerFamily activate()
		{
			base.gameObject.SetActive(true);
			this.active = true;
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				if (((ulong)this.auto_activate & (ulong)(1L << (i & 31))) != 0UL)
				{
					this.ADs[i].activate();
				}
			}
			return this;
		}

		public virtual UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				this.ADs[i].deactivate(immediate);
			}
			this.active = false;
			if (immediate && this.auto_deactive_gameobject)
			{
				base.gameObject.SetActive(false);
			}
			return this;
		}

		public UiBoxDesignerFamily bind()
		{
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				this.ADs[i].bind();
			}
			return this;
		}

		public UiBoxDesignerFamily hide()
		{
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				this.ADs[i].hide();
			}
			return this;
		}

		public UiBoxDesigner Get<T>() where T : UiBoxDesigner
		{
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				UiBoxDesigner uiBoxDesigner = this.ADs[i];
				if (uiBoxDesigner is T)
				{
					return uiBoxDesigner;
				}
			}
			return null;
		}

		public virtual bool isActive()
		{
			return this.active && base.gameObject.activeSelf;
		}

		public void setAlpha(float value)
		{
			int count = this.ADs.Count;
			for (int i = 0; i < count; i++)
			{
				this.ADs[i].setAlpha(value);
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				this.use_valotile_ = value;
				int count = this.ADs.Count;
				for (int i = 0; i < count; i++)
				{
					this.ADs[i].use_valotile = value;
				}
			}
		}

		protected List<UiBoxDesigner> ADs;

		private const int ACTV_MAXT = 30;

		public float base_z;

		public float slip_z = 0.008f;

		protected bool active;

		public bool auto_deactive_gameobject = true;

		public uint auto_activate = uint.MaxValue;

		protected bool runner_assigned_;

		private string _tostring;

		private bool use_valotile_;

		public enum MASKTYPE
		{
			NO_MASK,
			BOX,
			SCROLL
		}
	}
}
