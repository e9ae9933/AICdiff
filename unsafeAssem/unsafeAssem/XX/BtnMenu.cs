using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace XX
{
	public class BtnMenu<T> : BtnContainer<T>, BtnMenuRunner.IMenu where T : aBtn
	{
		public BtnMenu(string _name, float _w = 300f, float _h = 20f, int alloc_size = 0)
			: base(new GameObject("BtnMenu-" + _name), alloc_size)
		{
			this.name = _name;
			this.BaseScreen = this.Base;
			this.Runner = this.BaseScreen.AddComponent<BtnMenuRunner>();
			this.Runner.BCon = this;
			this.Runner.Mn = this;
			this.Runner.enabled = false;
			this.carr_alpha_tz = false;
			this.Base = IN.CreateGob(this.BaseScreen, "-mn");
			this.BaseScreen.transform.localPosition = new Vector3(0f, 0f, -8.8f);
			this.default_w = _w;
			this.default_h = _h;
			this.skin_default = "row";
			this._Carr = new ObjCarrierCon();
		}

		public override void destruct()
		{
			base.destruct();
			if (this.Md != null)
			{
				this.Md.destruct();
			}
			GameObject baseScreen = this.BaseScreen;
			GameObject @base = this.Base;
			this.BaseScreen = null;
			this.Base = null;
			if (baseScreen != null)
			{
				try
				{
					IN.DestroyOne(baseScreen);
				}
				catch
				{
				}
			}
			if (@base != null)
			{
				try
				{
					IN.DestroyOne(@base);
				}
				catch
				{
				}
			}
		}

		public override TBtn MakeT<TBtn>(string title, string skin = "")
		{
			if (title.IndexOf("----") == 0)
			{
				this.carr_index++;
				return default(TBtn);
			}
			TBtn tbtn = base.MakeT<TBtn>(title, skin);
			tbtn.addClickFn(new FnBtnBindings(this.fnClicked));
			tbtn.addHoverFn(new FnBtnBindings(this.fnHovered));
			tbtn.navi_auto_fill = false;
			if (!this.set_navi && this.qs_selector)
			{
				tbtn.z_push_click = false;
			}
			this.mesh_remake = true;
			return tbtn;
		}

		public BtnMenu<T> clearSelectedFn()
		{
			if (this.AFnSelected != null)
			{
				X.ALLN<BtnMenu<T>.FnMenuSelectedBindings>(this.AFnSelected);
			}
			return this;
		}

		public BtnMenu<T> addSelectedFn(BtnMenu<T>.FnMenuSelectedBindings Fn)
		{
			return this.addFn(ref this.AFnSelected, Fn);
		}

		protected BtnMenu<T> addFn(ref BtnMenu<T>.FnMenuSelectedBindings[] AFn, BtnMenu<T>.FnMenuSelectedBindings Fn)
		{
			aBtn.addFnT<BtnMenu<T>.FnMenuSelectedBindings>(ref AFn, Fn);
			return this;
		}

		protected bool runFn(BtnMenu<T>.FnMenuSelectedBindings[] AFn, int selected_index, string selected_title)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				BtnMenu<T>.FnMenuSelectedBindings fnMenuSelectedBindings = AFn[i];
				if (fnMenuSelectedBindings == null)
				{
					return flag;
				}
				flag = fnMenuSelectedBindings(this, selected_index, selected_title) && flag;
			}
			return flag;
		}

		public bool checkClicked()
		{
			if (this.HoveredRow != null)
			{
				return this.fnClicked(this.HoveredRow);
			}
			this.hide(false, false);
			return true;
		}

		public bool fnHovered(aBtn Row)
		{
			this.HoveredRow = Row;
			return true;
		}

		public bool fnClicked(aBtn Row)
		{
			if (this.runFn(this.AFnSelected, base.getIndex(Row), Row.title))
			{
				this.decided_title = Row.title;
				this.hide(false, false);
			}
			return true;
		}

		public void showBottom(IDesignerBlock Blk, aBtn Row = null)
		{
			this.showBottom(Blk, (Row == null) ? 0 : base.getIndex(Row));
		}

		public void showBottom(IDesignerBlock Blk, int focused_index)
		{
			Transform transform = Blk.getTransform();
			Vector3 position = transform.position;
			int num = X.IntC((float)this.carr_index / (float)this.clms);
			float swidth_px = Blk.get_swidth_px();
			float sheight_px = Blk.get_sheight_px();
			position.y -= (sheight_px * transform.lossyScale.y / 2f + this.default_h * (float)num / 2f) * 0.015625f;
			position.x += (-swidth_px * transform.lossyScale.x + this.default_w * (float)this.clms) / 2f * 0.015625f;
			this.show(position, focused_index);
		}

		public void showMouse(int focused_index)
		{
			this.show(IN.getMousePos(null), focused_index);
		}

		public void showMouse(string focused_title = "")
		{
			this.show(IN.getMousePos(null), base.getTitleIndex(focused_title));
		}

		public void showMouse(aBtn Row)
		{
			this.show(IN.getMousePos(null), base.getIndex(Row));
		}

		public void show(Vector3 PosU, aBtn Row)
		{
			this.show(PosU, base.getIndex(Row));
		}

		public void show(Vector3 PosU, string focused_title = "")
		{
			this.show(PosU, base.getTitleIndex(focused_title));
		}

		public void show(Vector3 PosU, int focused_index = -1)
		{
			this.Base.gameObject.SetActive(true);
			IN.Click.can_hover_other_object_on_dragging = true;
			int num = X.IntC((float)this.carr_index / (float)this.clms);
			float num2 = this.default_w * 0.015625f;
			float num3 = this.default_w / 2f;
			float num4 = this.default_h * 0.015625f;
			float num5 = this.default_w * (float)this.clms * 0.015625f;
			float num6 = this.default_h * (float)num * 0.015625f;
			float num7 = num6;
			float num8 = 0f;
			if (this.qs_selector)
			{
				num8 = 0.140625f;
				num6 += 0.28125f;
				if (this.TxQs == null)
				{
					this.StbQs = new STB();
					this.TxQs = IN.CreateGob(this.Base.gameObject, "-txqs").AddComponent<TextRenderer>();
					this.TxQs.size = 13f;
					this.TxQs.alignx = ALIGN.LEFT;
					this.TxQs.aligny = ALIGNY.MIDDLE;
				}
				this.StbQs.Set("");
				IN.Pos(this.TxQs.transform, -num5 * 0.5f + 0.125f, -num6 * 0.5f + num8, -0.01f);
			}
			this.FirstQsBtn = default(T);
			float num9 = IN.screen_visible_w * 0.5f - num5 * 64f / 2f;
			float num10 = IN.screen_visible_h * 0.5f - num6 * 64f / 2f;
			this._Carr.resetCalced();
			this._Carr.Base(0f, num8);
			this._Carr.Bounds(num5 - num2, -(num7 - num4), this.clms);
			this.runCarrier(9999f, this._Carr);
			if (this.set_navi)
			{
				this._Carr.refineBtnConnection<T>(this.ABtn, this.Length, this.navi_loop);
			}
			if (this.Md == null)
			{
				this.Md = MeshDrawer.prepareMeshRenderer(this.Base, MTRX.MtrMeshNormal, 0.005f, -1, null, false, false);
				this.Mrd = this.Base.GetComponent<MeshRenderer>();
				this.mesh_remake = true;
			}
			if (this.mesh_remake)
			{
				int length = this.Length;
				this.Md.Col = C32.d2c(3140892214U);
				this.Md.Box(0f, 0f, num5, num6, 0f, true);
				this.Md.Col = C32.d2c(3147997858U);
				this.Md.Box(0f, 0f, num5, num6, 0.015625f, true);
				if (this.qs_selector)
				{
					this.Md.Col = C32.d2c(3154116607U);
					this.Md.Rect(0f, -num6 * 0.5f + num8, num5, num8 * 2f, true);
				}
				this.Md.updateForMeshRenderer(false);
				this.mesh_remake = false;
			}
			this.decided_title = null;
			this.runCarrier(1024f, this._Carr);
			if (focused_index >= 0)
			{
				PosU.y = PosU.y - num6 / 2f + (num4 + 0.4f) * (float)(focused_index / X.Mx(1, this._Carr.set_clmn));
				if (this.focused_index_to_check)
				{
					base.setValueBits(1U << focused_index);
				}
			}
			else if (this.focused_index_to_check)
			{
				base.setValueBits(0U);
			}
			PosU.x = (float)X.IntR(X.MMX(-num9, PosU.x * 64f, num9)) * 0.015625f;
			PosU.y = (float)X.IntR(X.MMX(-num10, PosU.y * 64f, num10)) * 0.015625f;
			PosU.z = -0.01f;
			this.Base.transform.localPosition = PosU;
			this.bind(true, false);
			IN.clearCursDown();
			this.Runner.resetTimer();
			if (this.qs_selector)
			{
				this.fineQsContent(true);
			}
		}

		public override BtnContainer<T> bind(bool apply_to_skin = false, bool clear_navigation = false)
		{
			if (this.Base == null)
			{
				return this;
			}
			this.Base.gameObject.SetActive(true);
			base.bind(apply_to_skin, clear_navigation);
			return this;
		}

		public override BtnContainer<T> hide(bool apply_to_skin = false, bool clear_navigation = false)
		{
			if (this.Base == null)
			{
				return this;
			}
			base.hide(apply_to_skin, clear_navigation);
			if (this.FirstQsBtn != null)
			{
				this.FirstQsBtn.Deselect(true);
				this.FirstQsBtn = default(T);
			}
			this.Base.gameObject.SetActive(false);
			return this;
		}

		public override BtnContainer<T> setEnable(bool f, bool apply_to_skin = false, int lay = -1, bool clear_navigation = false)
		{
			if (!f && this.Runner.submitted)
			{
				this.Runner.submitted = false;
				this.checkClicked();
				return this;
			}
			if (this.Runner.enabled != f)
			{
				if (f)
				{
					SND.Ui.play(this.snd_activate, false);
				}
				else
				{
					SND.Ui.play((this.decided_title != null) ? this.snd_decide : this.snd_deactivate, false);
				}
			}
			if (this.Mrd != null)
			{
				this.Mrd.enabled = f;
			}
			this.HoveredRow = null;
			this.Runner.enabled = f;
			return base.setEnable(f, true, lay, clear_navigation);
		}

		public override ObjCarrierCon getMainCarr()
		{
			return this._Carr;
		}

		public bool isActive()
		{
			return this.Runner.enabled;
		}

		public void fineQsContent(bool fine_btn = true)
		{
			if (!this.qs_selector)
			{
				return;
			}
			bool flag = false;
			if (this.StbQs == null || this.StbQs.Length == 0)
			{
				this.TxQs.Col(1996488704U);
				this.TxQs.Txt("(selector)");
				flag = true;
			}
			else
			{
				this.TxQs.Col(4278190080U);
				this.TxQs.Txt(this.StbQs);
			}
			if (fine_btn)
			{
				bool flag2 = false;
				if (this.FirstQsBtn != null)
				{
					this.FirstQsBtn.Deselect(true);
				}
				this.FirstQsBtn = default(T);
				using (STB stb = TX.PopBld(null, 0))
				{
					for (int i = this.Length - 1; i >= 0; i--)
					{
						T t = base.Get(i);
						stb.Set(t.title).ToLower();
						if (flag || TX.qsCheckRelative(stb, this.TxQs.text_content, true) > 0f)
						{
							t.setAlpha(1f);
							if (!flag2)
							{
								flag2 = true;
								this.FirstQsBtn = t;
								if (this.StbQs.Length > 0)
								{
									this.FirstQsBtn.Select(true);
								}
							}
						}
						else
						{
							t.setAlpha(0.6f);
						}
					}
				}
			}
		}

		public void runMenu(out bool submitted, out bool canceled)
		{
			submitted = (canceled = false);
			bool flag = true;
			if (this.qs_selector)
			{
				Keyboard current = Keyboard.current;
				Key key = Key.A;
				bool flag2 = false;
				if (this.StbQs.Length > 0)
				{
					if (current[Key.Escape].wasPressedThisFrame)
					{
						flag2 = true;
						this.StbQs.Clear();
						flag = false;
					}
					if (current[Key.Backspace].wasPressedThisFrame || current[Key.Delete].wasPressedThisFrame)
					{
						flag2 = true;
						STB stbQs = this.StbQs;
						int length = stbQs.Length;
						stbQs.Length = length - 1;
						flag = false;
					}
				}
				for (int i = 0; i < 26; i++)
				{
					if (current[key + i].wasPressedThisFrame)
					{
						this.StbQs.Add((char)(97 + i));
						flag2 = true;
					}
				}
				if (current[Key.Backslash].wasPressedThisFrame)
				{
					this.StbQs.Add('_');
					flag2 = true;
				}
				if (flag2)
				{
					this.fineQsContent(true);
				}
				if (current[Key.Enter].wasPressedThisFrame && this.FirstQsBtn != null)
				{
					this.FirstQsBtn.ExecuteOnSubmitKey();
					submitted = true;
				}
			}
			else if (IN.isSubmit())
			{
				submitted = true;
			}
			if (flag && IN.isCancel())
			{
				canceled = true;
			}
		}

		public GameObject BaseScreen;

		public readonly string name;

		public readonly BtnMenuRunner Runner;

		public bool qs_selector;

		public bool set_navi;

		private STB StbQs;

		public const int QS_SEL_H = 18;

		public bool focused_index_to_check;

		public TextRenderer TxQs;

		public T FirstQsBtn;

		private MeshDrawer Md;

		private MeshRenderer Mrd;

		private const float Z_MENU = -8.8f;

		private aBtn HoveredRow;

		private ObjCarrierCon _Carr;

		private bool mesh_remake;

		public string decided_title;

		public int clms = 1;

		public string snd_activate = "tool_hand_init";

		public string snd_deactivate = "tool_hand_quit";

		public string snd_decide = "value_assign";

		private BtnMenu<T>.FnMenuSelectedBindings[] AFnSelected;

		public delegate bool FnMenuSelectedBindings(BtnMenu<T> Menu, int selected, string selected_title);
	}
}
