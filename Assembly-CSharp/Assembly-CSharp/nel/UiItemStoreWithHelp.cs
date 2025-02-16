using System;
using XX;

namespace nel
{
	public class UiItemStoreWithHelp : UiItemStore
	{
		protected override void prepareBoxes()
		{
			base.prepareBoxes();
			this.BoxHelp = this.CreateT<UiBoxDesignerHelp>("help", 0f, 0f, 200f, 100f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BoxHelp.detail_title_tx_key = this.help_title_tx_key;
			this.BoxHelp.detail_tx_key = this.help_detail_tx_key;
			this.BoxHelp.InitDH(delegate(aBtn B)
			{
				this.BxC.deactivate();
				this.CartBtn.hide();
				this.BxMoney.deactivate();
				this.BoxHelp.activateDetail();
				return true;
			});
			IN.setZ(this.BoxHelp.transform, -0.5f);
		}

		protected override void changeState(UiItemStore.STATE st)
		{
			UiItemStore.STATE stt = this.stt;
			base.changeState(st);
			if (stt == UiItemStore.STATE.TOP && st != UiItemStore.STATE.TOP)
			{
				this.BoxHelp.posSetDefault();
				this.BoxHelp.deactivate();
				return;
			}
			if (stt != UiItemStore.STATE.TOP && st == UiItemStore.STATE.TOP)
			{
				this.BoxHelp.posSetDefault();
				this.BoxHelp.activate();
			}
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			if (this.BoxHelp.isShowingDetail())
			{
				if (IN.isCancel() || IN.isMousePushDown() || IN.isUiSortPD())
				{
					this.BoxHelp.deactivateDetail();
					this.BxC.activate();
					this.BxMoney.activate();
					this.CartBtn.bind();
					this.BxC.Focus();
					SND.Ui.play("cancel", false);
				}
			}
			else if (this.stt == UiItemStore.STATE.TOP && IN.isUiSortPD())
			{
				this.BoxHelp.executeClickDetailBtn();
			}
			return true;
		}

		protected override bool can_progress_main_store
		{
			get
			{
				return base.can_progress_main_store && !this.BoxHelp.isShowingDetail();
			}
		}

		protected override bool cancelable_pause_main_store
		{
			get
			{
				return base.cancelable_pause_main_store && !this.BoxHelp.isShowingDetail();
			}
		}

		protected UiBoxDesignerHelp BoxHelp;

		protected string help_title_tx_key;

		protected string help_detail_tx_key;
	}
}
