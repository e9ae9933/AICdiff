using System;
using UnityEngine;

namespace XX
{
	public class DsPrmp : Designer
	{
		private static void MakeBase()
		{
			DsPrmp.BaseGob = new GameObject("Prompt");
			DsPrmp.BaseGob.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			DsPrmp.Base = DsPrmp.BaseGob.AddComponent<DsPrmpRunner>();
			IN.Pos(DsPrmp.BaseGob, 0f, 0f, -7.5f);
			DsPrmp.APrmp = new DsPrmp[4];
			DsPrmp.prompt_count = 0;
		}

		public static DsPrmp Make(string title)
		{
			if (DsPrmp.Base == null)
			{
				DsPrmp.MakeBase();
			}
			DsPrmp.Base.disable_next_frame = false;
			DsPrmp.Base.activate();
			DsPrmp.BaseGob.SetActive(true);
			for (int i = 0; i < DsPrmp.prompt_count; i++)
			{
				if (DsPrmp.APrmp[i].title == title)
				{
					DsPrmp.Rem(DsPrmp.APrmp[i]);
					break;
				}
			}
			DsPrmp dsPrmp = IN.CreateGob(DsPrmp.BaseGob, "-" + title).AddComponent<DsPrmp>();
			if (DsPrmp.prompt_count >= DsPrmp.APrmp.Length)
			{
				Array.Resize<DsPrmp>(ref DsPrmp.APrmp, DsPrmp.prompt_count + 4);
			}
			X.unshiftEmpty<DsPrmp>(DsPrmp.APrmp, dsPrmp, 0, 1, -1);
			DsPrmp.prompt_count++;
			for (int j = 1; j < DsPrmp.prompt_count; j++)
			{
				DsPrmp dsPrmp2 = DsPrmp.APrmp[j];
				if (dsPrmp2.isActive())
				{
					dsPrmp2.hideTemporary();
				}
			}
			dsPrmp.title = title;
			dsPrmp.hiding_apply_to_skin = true;
			dsPrmp.default_input_size = 16;
			dsPrmp.w = IN.w * 0.7f;
			dsPrmp.h = IN.h * 0.7f;
			dsPrmp.use_scroll = true;
			dsPrmp.activate();
			return dsPrmp;
		}

		public static void Rem(DsPrmp P)
		{
			int num = X.isinC<DsPrmp>(DsPrmp.APrmp, P);
			if (num >= 0)
			{
				DsPrmp.LastRemoved = P;
				if (num == 0 && DsPrmp.prompt_count > 1)
				{
					DsPrmp.APrmp[1].activate();
				}
				DsPrmp.prompt_count--;
				X.shiftEmpty<DsPrmp>(DsPrmp.APrmp, 1, num, -1);
				if (DsPrmp.prompt_count == 0)
				{
					DsPrmp.Base.deactivate(false);
				}
			}
		}

		public static void doneFront()
		{
			if (DsPrmp.prompt_count == 0)
			{
				return;
			}
			DsPrmp.Front.executeDone();
		}

		public static void cancelFront()
		{
			if (DsPrmp.prompt_count == 0)
			{
				return;
			}
			DsPrmp.Front.executeCancel();
		}

		private static void hideAnother(DsPrmp Pr)
		{
			if (DsPrmp.prompt_count <= 1)
			{
				return;
			}
			for (int i = DsPrmp.prompt_count - 1; i >= 0; i--)
			{
				DsPrmp dsPrmp = DsPrmp.APrmp[i];
				if (dsPrmp == Pr)
				{
					X.shiftEmpty<DsPrmp>(DsPrmp.APrmp, 1, i, -1);
				}
				else if (dsPrmp.isActive())
				{
					dsPrmp.hide();
				}
			}
			X.unshiftEmpty<DsPrmp>(DsPrmp.APrmp, Pr, 0, 1, -1);
		}

		public static DsPrmp Front
		{
			get
			{
				if (DsPrmp.prompt_count != 0)
				{
					return DsPrmp.APrmp[0];
				}
				return null;
			}
		}

		public static DsPrmp Ask(string s, FnPromptBindings fnDone)
		{
			DsPrmp dsPrmp = DsPrmp.Make("dialog");
			dsPrmp.title_string = "";
			dsPrmp.addP(new DsnDataP("", false)
			{
				text = s
			}, false);
			if (fnDone != null)
			{
				dsPrmp.addDoneFn(fnDone);
			}
			dsPrmp.WH(X.Mx(500f, dsPrmp.get_using_width_px() + 80f), X.Mx(250f, dsPrmp.get_using_height_px() + 110f));
			return dsPrmp;
		}

		public override Designer WH(float _wpx = 0f, float _hpx = 0f)
		{
			base.WH(_wpx, _hpx);
			if (this.Tm != null)
			{
				this.textReposit();
			}
			return this;
		}

		protected override void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			base.kadomaruRedraw(_t, update_mesh);
			if (this.Tm != null)
			{
				this.Tm.alpha = base.animating_alpha;
			}
		}

		protected override void destroyMe()
		{
			if (X.isinC<DsPrmp>(DsPrmp.APrmp, this, DsPrmp.prompt_count) == -1)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			base.gameObject.SetActive(false);
		}

		public override void destruct()
		{
			DsPrmp.Rem(this);
			if (DsPrmp.LastRemoved == this && DsPrmp.prompt_count == 0)
			{
				DsPrmp.Base.disable_next_frame = true;
			}
			base.destruct();
		}

		public override Designer activate()
		{
			this.activate_flag = true;
			this.need_show = true;
			if (this.Tm == null)
			{
				this.Tm = IN.CreateGob(base.gameObject, "-Tm").AddComponent<FillBlock>();
				this.Tm.size = 20f;
				this.Tm.TxCol = MTRX.ColWhite;
				this.Tm.alignx = ALIGN.CENTER;
				this.Tm.Col = base.bgcol;
				this.Tm.margin_x = 26f;
				this.Tm.margin_y = 8f;
				this.Tm.radius = 100f;
				this.Tm.size = 14f;
				this.title_string = this.title;
				this.textReposit();
				GameObject gameObject = new GameObject(base.name + "-Screen-Collider");
				gameObject.layer = base.gameObject.layer;
				gameObject.transform.SetParent(base.transform, true);
				this.Screen = gameObject.AddComponent<BoxCollider2D>();
				this.Screen.size = new Vector2(IN.w * 0.015625f * 4f, IN.h * 0.015625f * 4f);
				SND.Ui.play("tool_prmp_init", false);
			}
			else
			{
				this.Tm.enabled = true;
			}
			base.enabled = true;
			base.gameObject.SetActive(true);
			return this;
		}

		private void textReposit()
		{
			IN.PosP(this.Tm.transform, 0f, this.h / 2f + 20f, -0.001f);
			this.Tm.alpha = base.animating_alpha;
		}

		public DsPrmp P(string s)
		{
			base.add(new DsnDataP("", false)
			{
				text = s,
				TxCol = MTRX.ColWhite
			});
			return this;
		}

		public void fineOkAndCancelBtn()
		{
			if (this.make_ok)
			{
				this.make_ok = false;
				base.Br().add(new DsnDataButton
				{
					name = "ok",
					skin = "normal_hilighted",
					title = "ok",
					fnClick = new FnBtnBindings(this.fnClickOkCancel)
				});
			}
			if (this.make_cancel)
			{
				this.make_cancel = false;
				base.add(new DsnDataButton
				{
					name = "cancel",
					title = "cancel",
					fnClick = new FnBtnBindings(this.fnClickOkCancel),
					click_snd = "close_ui"
				});
			}
		}

		public override bool run(float fcnt)
		{
			this.fineOkAndCancelBtn();
			if (this.activate_flag)
			{
				DsPrmp.hideAnother(this);
				base.bind();
				this.activate_flag = false;
				base.enabled = true;
				this.animate_t = 0f;
				this.kadomaruRedraw(this.animate_t, true);
				IN.setZ(base.transform, -0.04f);
				if (this.DefSel == null)
				{
					this.DefSel = base.Get("ok", false) as aBtn;
				}
				if (this.DefSel != null)
				{
					this.DefSel.Select(false);
				}
			}
			if (this.need_show)
			{
				this.need_show = false;
				this.runFn(this.AFnShow);
			}
			return base.run(fcnt);
		}

		private bool fnClickOkCancel(aBtn B)
		{
			if (B.title == "ok")
			{
				return this.executeDone();
			}
			return !(B.title == "cancel") || this.executeCancel();
		}

		private bool executeDone()
		{
			if (LabeledInputField.CurFocused != null)
			{
				LabeledInputField.CurFocused.Blur();
			}
			if (this.runFn(this.AFnDone))
			{
				this.deactivate();
				return true;
			}
			return false;
		}

		private bool executeCancel()
		{
			if (LabeledInputField.CurFocused != null)
			{
				LabeledInputField.CurFocused.Blur();
			}
			if (this.runFn(this.AFnCancel))
			{
				this.deactivate();
				return true;
			}
			return false;
		}

		public override Designer deactivate()
		{
			this.runFn(this.AFnHide);
			this.activate_flag = false;
			base.deactivate();
			DsPrmp.Rem(this);
			return this;
		}

		public DsPrmp addShowFn(FnPromptBindings Fn)
		{
			return this.addFn(ref this.AFnShow, Fn);
		}

		public DsPrmp addHideFn(FnPromptBindings Fn)
		{
			return this.addFn(ref this.AFnHide, Fn);
		}

		public DsPrmp addDoneFn(FnPromptBindings Fn)
		{
			return this.addFn(ref this.AFnDone, Fn);
		}

		public DsPrmp addCancelFn(FnPromptBindings Fn)
		{
			return this.addFn(ref this.AFnCancel, Fn);
		}

		protected DsPrmp addFn(ref FnPromptBindings[] AFn, FnPromptBindings Fn)
		{
			aBtn.addFnT<FnPromptBindings>(ref AFn, Fn);
			return this;
		}

		protected bool runFn(FnPromptBindings[] AFn)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnPromptBindings fnPromptBindings = AFn[i];
				if (fnPromptBindings == null)
				{
					return flag;
				}
				flag = fnPromptBindings(this) && flag;
			}
			return flag;
		}

		public string title_string
		{
			get
			{
				if (!(this.Tm == null))
				{
					return this.Tm.text_content;
				}
				return this.title;
			}
			set
			{
				if (this.Tm != null)
				{
					this.Tm.text_content = X.T2K(value);
				}
			}
		}

		public DsPrmp hideTemporary()
		{
			base.hide();
			this.need_show = true;
			base.enabled = false;
			IN.setZ(base.transform, -100f);
			if (this.Tm != null)
			{
				this.Tm.enabled = false;
			}
			return this;
		}

		private static DsPrmpRunner Base;

		private static GameObject BaseGob;

		private static DsPrmp[] APrmp;

		private static int prompt_count;

		private const float PRMP_Z = -7.5f;

		private static DsPrmp LastRemoved;

		public string title = "";

		private FillBlock Tm;

		private bool activate_flag = true;

		public bool make_ok = true;

		public bool make_cancel = true;

		private bool need_show = true;

		private FnPromptBindings[] AFnShow;

		private FnPromptBindings[] AFnHide;

		private FnPromptBindings[] AFnDone;

		private FnPromptBindings[] AFnCancel;

		private BoxCollider2D Screen;
	}
}
