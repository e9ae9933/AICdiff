using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class BtnContainerRadio<T> : BtnContainer<T> where T : aBtn
	{
		public BtnContainerRadio(GameObject _Base, int default_val = 0, int alloc_size = 0, ObjCarrierCon _Carr = null, ScrollAppend SCA = null)
			: base(_Base, alloc_size)
		{
			this.selected = default_val;
			this.default_w = 100f;
			this.default_h = 22f;
			this.skin_default = "radio";
			if (SCA == null)
			{
				this.BaseCarr = _Carr ?? new ObjCarrierCon();
				base.setCarrierContainer(this.BaseCarr, true);
			}
		}

		public override TBtn MakeT<TBtn>(string title, string skin = "")
		{
			int length = this.Length;
			TBtn tbtn = base.MakeT<TBtn>(title, skin);
			if (tbtn == null)
			{
				if (length < this.selected)
				{
					this.selected--;
				}
				return default(TBtn);
			}
			tbtn.addClickFn(new FnBtnBindings(this.fnCheckDefault));
			if (length == this.selected)
			{
				tbtn.SetChecked(true, true);
			}
			if (this.BaseCarr != null)
			{
				this.BaseCarr.resetCalced();
				if (this.swidth_px > 0f)
				{
					this.runCarrier(1f, this.BaseCarr);
				}
			}
			return tbtn;
		}

		public override BtnContainer<T> RemakeLT<TBtn>(List<string> Atitle, string skin = "")
		{
			if (this.selected_key != null)
			{
				this.selected = -1;
			}
			base.RemakeLT<TBtn>(Atitle, skin);
			if (this.selected_key != null)
			{
				this.selected = base.getTitleIndex(this.selected_key);
			}
			if (this.selected >= 0)
			{
				T t = base.Get(this.selected);
				if (t != null)
				{
					this.selected_key = t.title;
					t.SetChecked(true, true);
				}
			}
			return this;
		}

		public BtnContainerRadio<T> BasePx(float x, float y)
		{
			this.conbase_x = x * 0.015625f;
			this.conbase_y = y * 0.015625f;
			if (this.BaseCarr != null)
			{
				this.BaseCarr.Base(this.conbase_x, this.conbase_y);
			}
			return this;
		}

		public BtnContainerRadio<T> Bounds(float w, float h = 0f, int clmn = -1, bool no_reset_pos = false)
		{
			if (this.BaseCarr == null)
			{
				return this;
			}
			this.BaseCarr.Bounds(w, h, clmn);
			this.swidth_px = this.BaseCarr.bounds_w * 64f + this.default_w + this.title_w_px;
			if (!no_reset_pos)
			{
				this.runCarrier(1f, this.BaseCarr);
			}
			return this;
		}

		public BtnContainerRadio<T> BoundsPx(float w, float h, int clmn = -1, bool no_reset_pos = false)
		{
			return this.Bounds(w * 0.015625f, h * 0.015625f, clmn, no_reset_pos);
		}

		public BtnContainerRadio<T> addChangedFn(BtnContainerRadio<T>.FnRadioBindings Fn)
		{
			aBtn.addFnT<BtnContainerRadio<T>.FnRadioBindings>(ref this.AFnValChanged, Fn);
			return this;
		}

		public int getValue()
		{
			return this.selected;
		}

		private bool runFnRadioBindings(BtnContainerRadio<T>.FnRadioBindings[] AFn, int pre_value, int cur_value)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				BtnContainerRadio<T>.FnRadioBindings fnRadioBindings = AFn[i];
				if (fnRadioBindings == null)
				{
					return flag;
				}
				flag = fnRadioBindings(this, pre_value, cur_value) && flag;
			}
			return flag;
		}

		public BtnContainerRadio<T> setValue(int val, bool call_bindings = true)
		{
			int num = this.selected;
			this.selected = val;
			this.selected_key = null;
			for (int i = this.Length - 1; i >= 0; i--)
			{
				T t = base.Get(i);
				if (i == val)
				{
					t.SetChecked(true, true);
					this.selected_key = t.title;
				}
				else
				{
					t.SetChecked(false, true);
				}
			}
			if (this.selected_key == null)
			{
				this.selected = -1;
			}
			if (call_bindings)
			{
				this.runFnRadioBindings(this.AFnValChanged, num, this.selected);
			}
			return this;
		}

		protected bool fnCheckDefault(aBtn B)
		{
			this.setValue(B);
			return true;
		}

		public BtnContainerRadio<T> setValue(aBtn CheckedB)
		{
			int num = this.selected;
			this.selected = -1;
			for (int i = this.Length - 1; i >= 0; i--)
			{
				T t = base.Get(i);
				if (t == CheckedB)
				{
					t.SetChecked(true, true);
					this.selected = i;
					this.selected_key = t.title;
				}
				else
				{
					t.SetChecked(false, true);
				}
			}
			if (this.selected != num && !this.runFnRadioBindings(this.AFnValChanged, num, this.selected))
			{
				if (CheckedB != null)
				{
					CheckedB.SetChecked(false, true);
				}
				T t2 = base.Get(num);
				if (t2 != null)
				{
					this.selected = num;
					this.selected_key = t2.title;
					t2.SetChecked(true, true);
				}
				else
				{
					this.selected = -1;
					this.selected_key = null;
				}
			}
			return this;
		}

		public override void AddSelectableItems(List<aBtn> _ABtn, bool only_front)
		{
			if (only_front && this.selected >= 0)
			{
				_ABtn.Add(base.Get(this.selected));
				return;
			}
			base.AddSelectableItems(_ABtn, only_front);
		}

		public override string getValueString()
		{
			if (!this.value_return_name)
			{
				return this.selected.ToString();
			}
			T t = base.Get(this.selected);
			if (!(t != null))
			{
				return "";
			}
			return t.title;
		}

		public override void setValue(string s)
		{
			if (this.value_return_name)
			{
				this.setValue(base.Get(s));
				return;
			}
			int num = X.NmI(s, -2, true, false);
			if (num == -2)
			{
				this.setValue(base.Get(s));
				return;
			}
			this.setValue(base.Get(num));
		}

		public override void Destroy()
		{
			if (this.MrdCaption != null)
			{
				IN.DestroyOne(this.MrdCaption);
				this.MrdCaption = null;
			}
			base.Destroy();
		}

		public ObjCarrierCon getBaseCarr()
		{
			return this.BaseCarr;
		}

		public int rows
		{
			get
			{
				if (this.BaseCarr.set_clmn <= 0)
				{
					return this.Length;
				}
				return X.IntC((float)(this.Length / this.BaseCarr.set_clmn));
			}
		}

		public float swidth_px;

		private float title_w_px;

		protected int selected;

		protected string selected_key;

		private ObjCarrierCon BaseCarr;

		public float conbase_x;

		public float conbase_y;

		public bool value_return_name;

		private BtnContainerRadio<T>.FnRadioBindings[] AFnValChanged;

		private MeshRenderer MrdCaption;

		public delegate bool FnRadioBindings(BtnContainerRadio<T> _B, int pre_value, int cur_value);

		public delegate BtnContainerRadio<T> FnCreateContainer(GameObject _Base, int default_val = 0, int alloc_size = 0, ObjCarrierCon _Carr = null, ScrollAppend SCA = null);
	}
}
