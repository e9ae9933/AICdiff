using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class UiEvtDesignerBox : UiBoxDesignerFamily, IEventWaitListener
	{
		protected abstract string reserve_key { get; }

		protected abstract float main_w { get; }

		protected abstract float main_h { get; }

		protected override void Awake()
		{
			base.Awake();
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.M2D.FlagValotStabilize.Add(this.reserve_key);
			IN.setZ(base.transform, -4.8500004f);
			this.auto_deactive_gameobject = false;
			base.gameObject.layer = IN.gui_layer;
			this.BxC = base.Create("main_cmd", 0f, 0f, this.main_w, this.main_h, 3, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxC.anim_time(22);
			this.BxC.use_scroll = false;
			this.BxC.Focusable(false, false, null);
			this.BxC.margin_in_lr = this.margin_in_x;
			this.BxC.margin_in_tb = this.margin_in_y;
			this.BxC.btn_height = 30f;
			this.BxC.item_margin_y_px = 4f;
			this.BxC.item_margin_x_px = 0f;
			this.BxC.selectable_loop |= 2;
			this.BxC.getBox().frametype = UiBox.FRAMETYPE.MAIN;
			this.BxC.animate_maxt = 0;
			this.BxC.alignx = ALIGN.CENTER;
			this.MdStencil = UiBoxDesigner.createStencilTB(this.BxC, out this.GobMdStencil, 42f, this.MaStencil);
			IN.setZ(this.GobMdStencil.transform, this.z_stencil);
			this.deactivate(true);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			Flagger flagValotStabilize = this.M2D.FlagValotStabilize;
			if (flagValotStabilize != null)
			{
				flagValotStabilize.Rem(this.reserve_key);
			}
			if (this.MdStencil != null)
			{
				this.MdStencil.destruct();
			}
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.Focusable(false, false, null);
			t.WHanim(t.get_swidth_px(), t.get_sheight_px(), false, false);
			t.stencil_ref = (t.box_stencil_ref_mask = 11);
			return t;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			base.deactivate(immediate);
			IN.clearPushDown(true);
			return this;
		}

		public virtual UiEvtDesignerBox Init()
		{
			this.activate();
			this.BxC.Clear();
			this.BxC.init();
			IN.clearPushDown(true);
			return this;
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				this.active = false;
				IN.DestroyOne(base.gameObject);
				return false;
			}
			byte b = 0;
			if (IN.isCancel())
			{
				b = 1;
			}
			else if (this.submit_to_cancel && IN.isSubmitPD(1))
			{
				b = 2;
			}
			if (b > 0)
			{
				if (b == 1)
				{
					SND.Ui.play("cancel", false);
				}
				if (this.CancelBtn != null)
				{
					if (this.CancelBtn.isSelected())
					{
						if (this.cancel_pressable_x && b == 1)
						{
							this.CancelBtn.ExecuteOnSubmitKey();
						}
					}
					else
					{
						if (b == 2)
						{
							IN.clearPushDown(true);
						}
						this.CancelBtn.Select(true);
					}
				}
			}
			return true;
		}

		public string prompt_result
		{
			get
			{
				return this.prompt_result_;
			}
			set
			{
				this.prompt_result_ = value;
				if (this.input_to_varcon)
				{
					EV.getVariableContainer().define("_result", value, true);
				}
			}
		}

		protected NelM2DBase M2D;

		protected UiBoxDesigner BxC;

		protected float margin_in_x = 24f;

		protected float margin_in_y = 68f;

		private const float btn_h = 30f;

		protected float z_stencil;

		protected MeshDrawer MdStencil;

		protected aBtn CancelBtn;

		private string prompt_result_;

		public bool input_to_varcon = true;

		protected bool cancel_pressable_x = true;

		protected bool submit_to_cancel = true;

		protected MdArranger MaStencil;

		private GameObject GobMdStencil;
	}
}
