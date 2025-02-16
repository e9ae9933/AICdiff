using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class BtnContainer<T> : BtnContainerBasic, IAlphaSetable, IDesignerBlock, IKeyArrayRemakeable, IValotileSetable where T : aBtn
	{
		public BtnContainer(int alloc_size = 0)
			: base(null)
		{
			this.ABtn = new T[alloc_size];
		}

		public BtnContainer(GameObject _Base, int alloc_size = 0)
			: base(_Base)
		{
			this.ABtn = new T[alloc_size];
			_Base.layer = LayerMask.NameToLayer(IN.gui_layer_name);
		}

		public void RemakeDefault(int execute_delay)
		{
			if (this.APool == null)
			{
				this.APool = new List<T>(this.LEN);
			}
			if (execute_delay < 0)
			{
				this.need_remake_delay = 0;
				return;
			}
			if (execute_delay <= 0)
			{
				this.need_remake_delay = 0;
				this.RemakeT<T>(null, "");
				return;
			}
			this.need_remake_delay = X.Mx(this.need_remake_delay, execute_delay);
		}

		public BtnContainer<T> Remake(string[] Atitle, string skin = "")
		{
			if (Atitle == null)
			{
				return this.RemakeLT<T>(null, skin);
			}
			BtnContainer<T> btnContainer;
			using (BList<string> blist = ListBuffer<string>.Pop(Atitle))
			{
				btnContainer = this.RemakeLT<T>(blist, skin);
			}
			return btnContainer;
		}

		public BtnContainer<T> RemakeT<TBtn>(string[] Atitle, string skin = "") where TBtn : T
		{
			if (Atitle == null)
			{
				return this.RemakeLT<TBtn>(null, skin);
			}
			BtnContainer<T> btnContainer;
			using (BList<string> blist = ListBuffer<string>.Pop(Atitle))
			{
				btnContainer = this.RemakeLT<TBtn>(blist, skin);
			}
			return btnContainer;
		}

		public virtual BtnContainer<T> RemakeLT<TBtn>(List<string> Atitle, string skin = "") where TBtn : T
		{
			float num = -1f;
			if (this.OuterScrollBox != null)
			{
				num = this.OuterScrollBox.getScrollBox().scrolled_level_y;
			}
			this.DestroyButtons(true, false);
			if (this.Carr != null)
			{
				this.Carr.resetCalced();
			}
			if (this.Carr_Hiding != null && this.Carr_Hiding != this.Carr)
			{
				this.Carr_Hiding.resetCalced();
			}
			this.carr_hiding_reverse = true;
			BList<string> blist = null;
			if (Atitle == null && this.fnGenerateRemakeKeys != null)
			{
				this.fnGenerateRemakeKeys(this, blist = ListBuffer<string>.Pop(blist));
			}
			if (Atitle == null && blist == null)
			{
				X.de("Remakeに必要な配列または fnGenerateRemakeKeys がありません", null);
				return this;
			}
			this.need_remake_delay = -1;
			if (blist != null)
			{
				this.MakeT<TBtn>(blist, skin);
				ListBuffer<string>.Release(blist);
			}
			else
			{
				this.MakeT<TBtn>(Atitle, skin);
			}
			this.need_remake_delay = 0;
			if (this.Carr_Hiding != null && this.Carr == null)
			{
				ObjCarrierCon objCarrierCon = (this.Carr = this.Carr_Hiding);
				this.Carr_Hiding = null;
				this.runCarrier((float)objCarrierCon.get_maxt(), objCarrierCon);
				this.Carr_Hiding = objCarrierCon;
				this.Carr = null;
			}
			else if (this.Carr != null)
			{
				this.runCarrier((float)this.Carr.get_maxt(), this.Carr);
			}
			if (this.OuterScrollBox != null)
			{
				this.OuterScrollBox.reposition(true);
				this.OuterScrollBox.getScrollBox().scrolled_level_y = num;
			}
			else
			{
				this.revealBelongScrollPosition();
			}
			return this;
		}

		public virtual void revealBelongScrollPosition()
		{
			if (this.BelongScroll == null || this.LEN == 0)
			{
				return;
			}
			this.revealBelongScrollPosition(this.ABtn[0], 0);
		}

		public void revealBelongScrollPosition(aBtn Target, int shifty_px = 0)
		{
			if (this.BelongScroll == null)
			{
				return;
			}
			if (this.BelongScroll.transform.InverseTransformPoint(Target.transform.TransformPoint(new Vector2(0f, -Target.get_sheight_px() * 0.5f * 0.015625f))).y > (this.BelongScroll.get_sheight_px() * 0.5f - 20f) * 0.015625f)
			{
				this.BelongScroll.reveal(Target.transform, 0f, (Target.get_sheight_px() * 0.5f + (float)shifty_px) * 0.015625f, false);
			}
		}

		public BtnContainer<T> Make(string[] Atitle, string skin = "")
		{
			int num = Atitle.Length;
			this.EnsureCapacityLeft(num);
			for (int i = 0; i < num; i++)
			{
				this.Make(Atitle[i], skin);
			}
			return this;
		}

		public BtnContainer<T> MakeT<TBtn>(string[] Atitle, string skin = "") where TBtn : T
		{
			if (Atitle == null)
			{
				return this;
			}
			int num = Atitle.Length;
			this.EnsureCapacityLeft(num);
			for (int i = 0; i < num; i++)
			{
				this.MakeT<TBtn>(Atitle[i], skin);
			}
			return this;
		}

		public BtnContainer<T> MakeT<TBtn>(List<string> Atitle, string skin = "") where TBtn : T
		{
			if (Atitle == null)
			{
				return this;
			}
			int count = Atitle.Count;
			this.EnsureCapacityLeft(count);
			for (int i = 0; i < count; i++)
			{
				this.MakeT<TBtn>(Atitle[i], skin);
			}
			return this;
		}

		public void EnsureCapacityLeft(int left_count)
		{
			if (left_count > 0)
			{
				int num = this.Length + left_count;
				if (num > this.ABtn.Length)
				{
					Array.Resize<T>(ref this.ABtn, num);
				}
			}
		}

		public virtual TBtn MakeT<TBtn>(string title, string skin = "") where TBtn : T
		{
			if (title == null)
			{
				this.carr_index++;
				return default(TBtn);
			}
			TBtn tbtn = default(TBtn);
			GameObject gameObject = null;
			bool flag = false;
			if (this.APool != null && this.APool.Count > 0)
			{
				T t = this.APool[0];
				tbtn = t as TBtn;
				if (tbtn != null)
				{
					gameObject = tbtn.gameObject;
					gameObject.name = this.Base.name + "_btn_" + title;
					gameObject.SetActive(true);
					this.APool.RemoveAt(0);
					flag = true;
				}
				else
				{
					this.APool.RemoveAt(0);
					this.APool.Add(t);
				}
			}
			if (gameObject == null)
			{
				gameObject = IN.CreateGob(this.Base, "_btn_" + title);
				if (this.layer >= 0)
				{
					gameObject.layer = this.layer;
				}
				tbtn = gameObject.AddComponent<TBtn>();
			}
			tbtn.addClickFn(new FnBtnBindings(this.fnClickMyBtn));
			if (this.base_z != 0f)
			{
				Vector3 localPosition = gameObject.transform.localPosition;
				localPosition.z = this.base_z;
				gameObject.transform.localPosition = localPosition;
			}
			tbtn.Container = this;
			tbtn.w = this.default_w;
			tbtn.h = this.default_h;
			tbtn.carr_index = this.carr_index;
			if (this.carr_fix_length >= 0)
			{
				this.carr_fix_length = X.Mx(tbtn.carr_index + 1, this.carr_fix_length);
			}
			tbtn.initializeSkin(TX.noe(skin) ? this.skin_default : skin, title);
			ButtonSkin skin2 = tbtn.get_Skin();
			if (flag && skin2 != null)
			{
				if (title != "")
				{
					skin2.setTitle(title);
				}
				skin2.alpha = 1f;
				skin2.fine_flag = true;
				skin2.WHPx(tbtn.w, tbtn.h);
			}
			if (this.default_focus >= 0 && this.LEN == this.default_focus)
			{
				tbtn.autoSelect = true;
			}
			this.addBtn((T)((object)tbtn));
			tbtn.autoBind = this.autoBind;
			if (!this.autoEnable)
			{
				tbtn.enabled = false;
			}
			else
			{
				tbtn.enabled = true;
			}
			if (this.Carr != null)
			{
				this.Carr.resetCalced();
			}
			if (this.Carr_Hiding != null && this.Carr != this.Carr_Hiding)
			{
				this.Carr_Hiding.resetCalced();
			}
			if (!this.runFn(this.AFnMaking, tbtn))
			{
				this.Rem(this.LEN - 1, false);
				return default(TBtn);
			}
			this.carr_index++;
			return tbtn;
		}

		public T Make(string title, string skin = "")
		{
			return this.MakeT<T>(title, skin);
		}

		public aBtn addBtn(T B)
		{
			if (this.LEN >= this.ABtn.Length)
			{
				Array.Resize<T>(ref this.ABtn, this.LEN + 8);
			}
			T[] abtn = this.ABtn;
			int len = this.LEN;
			this.LEN = len + 1;
			abtn[len] = B;
			B.Container = this;
			return B;
		}

		public BtnContainer<T> addMakingFn(BtnContainer<T>.FnBtnMakingBindings Fn)
		{
			aBtn.addFnT<BtnContainer<T>.FnBtnMakingBindings>(ref this.AFnMaking, Fn);
			return this;
		}

		public BtnContainer<T> addMakingFn(BtnContainer<T>.FnBtnMakingBindings[] Fn)
		{
			aBtn.addFnT<BtnContainer<T>.FnBtnMakingBindings>(ref this.AFnMaking, Fn);
			return this;
		}

		public BtnContainer<T> addToAllClickedFn(FnBtnBindings Fn)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.ABtn[i].addClickFn(Fn);
			}
			return this;
		}

		protected bool runFn(BtnContainer<T>.FnBtnMakingBindings[] AFn, aBtn B)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				BtnContainer<T>.FnBtnMakingBindings fnBtnMakingBindings = AFn[i];
				if (fnBtnMakingBindings == null)
				{
					return flag;
				}
				flag = fnBtnMakingBindings(this, B) && flag;
			}
			return flag;
		}

		public virtual BtnContainer<T> setEnable(bool f, bool apply_to_skin = false, int lay = -1, bool clear_navigation = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				T t = this.ABtn[i];
				if (clear_navigation)
				{
					t.clearNavi(15U, false);
				}
				if (f)
				{
					if (this.binding_check && t.isChecked())
					{
						t.SetChecked(false, true);
					}
					t.bind();
					if (this.default_focus == i)
					{
						t.Select(true);
					}
				}
				else
				{
					t.hide();
				}
				if (lay >= 0)
				{
					t.setLayer(lay);
				}
			}
			return this;
		}

		public BtnContainer<T> fineAll(bool immediate = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.ABtn[i].Fine(immediate);
			}
			return this;
		}

		public BtnContainer<T> SetCheckedAll(bool f = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.ABtn[i].SetChecked(f, true);
			}
			return this;
		}

		public virtual BtnContainer<T> hide(bool apply_to_skin = false, bool clear_navigation = false)
		{
			if (this.Carr != null && this.Carr_Hiding == null && this.carr_hiding_reverse)
			{
				this.Carr_Hiding = this.Carr;
				this.Carr = null;
			}
			if (this.Carr_Hiding != null)
			{
				this.Carr_Hiding.resetCalced();
			}
			this.carr_hiding_offline = false;
			return this.setEnable(false, apply_to_skin, this.hiding_layer, clear_navigation);
		}

		public virtual BtnContainer<T> bind(bool apply_to_skin = false, bool clear_navigation = false)
		{
			if (this.Carr == null && this.Carr_Hiding != null && this.carr_hiding_reverse)
			{
				this.Carr = this.Carr_Hiding;
				this.Carr_Hiding = null;
			}
			if (this.Carr != null)
			{
				this.Carr.resetCalced();
			}
			this.carr_hiding_offline = false;
			return this.setEnable(true, apply_to_skin, (this.hiding_layer >= 0) ? ((this.layer >= 0) ? this.layer : this.Base.layer) : (-1), clear_navigation);
		}

		public virtual void Destroy()
		{
			this.DestroyButtons(false, false);
			if (this.APool != null)
			{
				for (int i = this.APool.Count - 1; i >= 0; i--)
				{
					try
					{
						IN.DestroyE(this.APool[i].gameObject);
					}
					catch
					{
					}
				}
				this.APool.Clear();
			}
		}

		public void DestroyButtons(bool all_add_to_pool = false, bool only_disable = false)
		{
			if (this.APool == null)
			{
				all_add_to_pool = false;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				T t = this.ABtn[i];
				if (all_add_to_pool)
				{
					this.APool.Add(t);
					if (only_disable)
					{
						t.hide();
						t.gameObject.SetActive(false);
					}
					else
					{
						t.resetToPool(true);
					}
				}
				else if (only_disable)
				{
					t.hide();
					t.gameObject.SetActive(false);
				}
				else
				{
					IN.DestroyE(t.gameObject);
				}
				this.ABtn[i] = default(T);
			}
			this.LEN = 0;
			this.carr_index = 0;
		}

		public T Rem(string t)
		{
			return this.Rem(this.getIndex(this.Get(t)), false);
		}

		public T Rem(int i, bool do_not_destruct = false)
		{
			if (i >= this.LEN || i == -1)
			{
				return default(T);
			}
			T t = this.ABtn[i];
			if (this.APool != null)
			{
				this.APool.Add(t);
				t.resetToPool(true);
			}
			else if (!do_not_destruct)
			{
				IN.DestroyE(t.gameObject);
			}
			X.shiftEmpty<T>(this.ABtn, 1, i, -1);
			this.LEN--;
			return t;
		}

		private bool fnClickMyBtn(aBtn B)
		{
			this.clicked_title = B.title;
			if (this.default_focus >= 0)
			{
				aBtn[] abtn = this.ABtn;
				this.default_focus = X.isinC<aBtn>(abtn, B);
			}
			if (this.binding_check)
			{
				B.SetChecked(true, true);
			}
			return true;
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
				int len = this.LEN;
				for (int i = 0; i < len; i++)
				{
					this.ABtn[i].use_valotile = value;
				}
			}
		}

		public void wholeRun()
		{
			int len = this.LEN;
			for (int i = 0; i < len; i++)
			{
				aBtn aBtn = this.ABtn[i];
				if (!aBtn.run(0f))
				{
					IN.remRunner(aBtn);
				}
			}
		}

		public override bool runCarrier(float t, ObjCarrierCon _Carr = null)
		{
			if (this.need_remake_delay > 0)
			{
				int num = this.need_remake_delay - 1;
				this.need_remake_delay = num;
				if (num <= 0)
				{
					this.RemakeDefault(0);
				}
			}
			if (_Carr == null)
			{
				_Carr = this.Carr;
			}
			if (_Carr == null && this.Carr_Hiding != null && !this.carr_hiding_offline)
			{
				_Carr = this.Carr_Hiding;
			}
			bool flag = _Carr == this.Carr_Hiding;
			bool flag2 = true;
			if (_Carr != null)
			{
				ObjCarrierCon objCarrierCon = _Carr;
				aBtn[] abtn = this.ABtn;
				flag2 = objCarrierCon.CalcAllBtn(t, abtn, (this.carr_fix_length < 0) ? this.LEN : this.carr_fix_length, flag);
				if (this.carr_alpha_tz)
				{
					for (int i = 0; i < this.LEN; i++)
					{
						aBtn aBtn = this.ABtn[i];
						float num2 = (aBtn.get_Skin().alpha = _Carr.getTz(t, X.Abs(aBtn.carr_index), flag));
						if (flag ? (num2 > 0f) : (num2 < 1f))
						{
							flag2 = false;
						}
					}
				}
				if (flag2)
				{
					if (_Carr == this.Carr)
					{
						this.Carr.refineBtnConnection<T>(this.ABtn, this.LEN, this.navi_loop);
						if (this.carr_hiding_reverse)
						{
							this.Carr_Hiding = this.Carr;
						}
						this.Carr = null;
					}
					this.carr_hiding_offline = true;
				}
			}
			return flag2;
		}

		public int getTitleIndex(string _title)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.ABtn[i].title == _title)
				{
					return i;
				}
			}
			return -1;
		}

		public int getIndex(aBtn B)
		{
			if (B == null)
			{
				return -1;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.ABtn[i] == B)
				{
					return i;
				}
			}
			return -1;
		}

		public string getClickedTitle(bool reset = true)
		{
			string text = this.clicked_title;
			if (reset)
			{
				this.clicked_title = "";
			}
			return text;
		}

		public override int Length
		{
			get
			{
				return this.LEN;
			}
		}

		public override aBtn GetButton(int i)
		{
			return this.Get(i);
		}

		public T Get(int i)
		{
			if (!X.BTW(0f, (float)i, (float)this.LEN))
			{
				return default(T);
			}
			return this.ABtn[i];
		}

		public T Get(string s)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				T t = this.ABtn[i];
				if (t.title == s)
				{
					return t;
				}
			}
			return default(T);
		}

		public BtnContainer<T> setCarrierContainer(ObjCarrierCon _Carr, bool set_hover_to_select = true)
		{
			this.Carr = _Carr;
			this.Carr_Hiding = null;
			this.carr_hiding_offline = false;
			if (this.Carr != null)
			{
				this.Carr.resetCalced();
				if (set_hover_to_select || this.carr_alpha_tz)
				{
					for (int i = 0; i < this.LEN; i++)
					{
						T t = this.ABtn[i];
						if (set_hover_to_select)
						{
							t.hover_to_select = true;
						}
						if (this.carr_alpha_tz)
						{
							ButtonSkin skin = t.get_Skin();
							skin.alpha = 0f;
							skin.Fine();
						}
					}
				}
			}
			return this;
		}

		public aBtn getAbsNearBtn(Vector2 V)
		{
			aBtn aBtn = null;
			float num = 0f;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				T t = this.Get(i);
				Vector2 vector = t.transform.position;
				float num2 = X.LENGTHXY2(V.x, V.y, vector.x, vector.y);
				if (aBtn == null || num2 < num)
				{
					aBtn = t;
					num = num2;
				}
			}
			return aBtn;
		}

		public Transform getTransform()
		{
			ObjCarrierCon objCarrierCon = ((this.Carr == null) ? this.Carr_Hiding : this.Carr);
			if (objCarrierCon == null || !(objCarrierCon is ObjCarrierConBlock))
			{
				return null;
			}
			return (objCarrierCon as ObjCarrierConBlock).getTransform();
		}

		public float get_swidth_px()
		{
			ObjCarrierCon objCarrierCon = ((this.Carr == null) ? this.Carr_Hiding : this.Carr);
			if (objCarrierCon == null || !(objCarrierCon is ObjCarrierConBlock))
			{
				return 0f;
			}
			return (objCarrierCon as ObjCarrierConBlock).get_swidth_px();
		}

		public float get_sheight_px()
		{
			ObjCarrierCon objCarrierCon = ((this.Carr == null) ? this.Carr_Hiding : this.Carr);
			if (objCarrierCon == null || !(objCarrierCon is ObjCarrierConBlock))
			{
				return 0f;
			}
			return (objCarrierCon as ObjCarrierConBlock).get_sheight_px();
		}

		public virtual ObjCarrierCon getMainCarr()
		{
			if (this.Carr != null)
			{
				return this.Carr;
			}
			return this.Carr_Hiding;
		}

		public virtual void AddSelectableItems(List<aBtn> _ABtn, bool only_front)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				_ABtn.Add(this.ABtn[i]);
				if (only_front)
				{
					return;
				}
			}
		}

		public T[] getVector()
		{
			return this.ABtn;
		}

		public List<T> copyVectorTo(List<T> A)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				A.Add(this.ABtn[i]);
			}
			return A;
		}

		public BtnContainer<T> setVector(T[] V)
		{
			this.ABtn = V;
			return this;
		}

		public uint getValueBits()
		{
			uint num = 0U;
			for (int i = 0; i < this.LEN; i++)
			{
				num |= (this.Get(i).isChecked() ? (1U << i) : 0U);
			}
			return num;
		}

		public override string getValueString()
		{
			return this.getValueBits().ToString();
		}

		public override void setValue(string s)
		{
			int num = X.NmI(s, -1, true, false);
			if (num < 0)
			{
				return;
			}
			this.setValueBits((uint)num);
		}

		public void setValueBits(uint r)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.Get(i).SetChecked((r & (1U << i)) > 0U, true);
			}
		}

		public void setAlpha(float a)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.Get(i).get_Skin().alpha = a;
			}
		}

		public void setLockedAll(bool f)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.Get(i).SetLocked(f, true, false);
			}
		}

		public void clearNaviAll(uint aim_bit = 15U, bool only_not_important = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.Get(i).clearNavi(aim_bit, only_not_important);
			}
		}

		public override void destruct()
		{
			try
			{
				this.hide(false, false);
			}
			catch
			{
			}
			base.destruct();
		}

		public bool carr_hiding_running
		{
			get
			{
				return this.Carr == null && this.Carr_Hiding != null && !this.carr_hiding_offline;
			}
		}

		public override void Pause()
		{
			this.hide(this.pausing_apply_to_skin, false);
		}

		public override void Resume()
		{
			this.bind(true, false);
		}

		protected T[] ABtn;

		private int LEN;

		public int layer = -1;

		public int hiding_layer = -1;

		public bool binding_check;

		public bool pausing_apply_to_skin;

		public float base_z;

		public bool autoBind = true;

		public bool autoEnable = true;

		public float default_w;

		public float default_h;

		private ObjCarrierCon Carr;

		private ObjCarrierCon Carr_Hiding;

		public int navi_loop;

		public int carr_index;

		private string clicked_title = "";

		public int need_remake_delay;

		public int carr_fix_length = -1;

		public string skin_default = "normal";

		protected bool carr_hiding_offline;

		private BtnContainer<T>.FnBtnMakingBindings[] AFnMaking;

		public List<T> APool;

		public FnGenerateRemakeKeys fnGenerateRemakeKeys;

		public ScrollAppendBtnContainer<T> OuterScrollBox;

		private bool use_valotile_;

		public delegate bool FnBtnMakingBindings(BtnContainer<T> BCon, aBtn _B);
	}
}
