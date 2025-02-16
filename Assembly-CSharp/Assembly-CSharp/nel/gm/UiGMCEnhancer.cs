using System;
using XX;

namespace nel.gm
{
	internal class UiGMCEnhancer : UiGMC
	{
		internal UiGMCEnhancer(UiGameMenu _GM)
			: base(_GM, CATEG.ENHANCER, true, 1, 1, 1, 1, 1.5f, 3f)
		{
			this.IMng = new UiItemManageBox(base.M2D.IMNG, null);
			this.IMng.topright_desc_width = 160f;
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				return true;
			}
			this.BxR.item_margin_x_px = 0f;
			this.BxR.item_margin_y_px = 0f;
			this.Tab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, false);
			this.Tab.Smallest();
			this.Tab.margin_in_tb = 6f;
			this.BxR.endTab(false);
			this.initEnhancerTab(true);
			this.IMng.initDesigner(base.M2D.IMNG.getInventoryEnhancer(), this.Tab, null, null, false, false, true);
			return true;
		}

		protected override bool initAppearSubAreaInner(UiBoxDesigner Ds, int i, bool is_top)
		{
			if (base.initAppearSubAreaInner(Ds, i, is_top))
			{
				return true;
			}
			EnhancerManager.UiPrepareTbArea(Ds, is_top, (EnhancerManager.Enhancer Eh) => this.GM.isEditState(this));
			return true;
		}

		internal override bool canInitEdit()
		{
			return this.IMng.canOpenEdit();
		}

		internal override void initEdit()
		{
			this.IMng.initDesigner(base.M2D.IMNG.getInventoryEnhancer(), this.Tab, null, null, false, true, true);
			this.DescArea = this.BtmTab.GetDesigner(0);
		}

		internal override void quitEdit()
		{
			this.IMng.blurDesc();
			M2PrSkill.resetSkillConnectionWhole(false, false, false);
		}

		public override void quitAppear()
		{
			base.quitAppear();
		}

		internal override void releaseEvac()
		{
			this.IMng.quitDesigner(false, false);
			base.releaseEvac();
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			EnhancerManager.runEnhancerUi(this.IMng);
			if (!this.IMng.runEditItem())
			{
				return GMC_RES.BACK_CATEGORY;
			}
			return GMC_RES.CONTINUE;
		}

		private void initEnhancerTab(bool clear = false)
		{
			if (clear)
			{
				this.Tab.Clear();
				this.IMng.quitDesigner(false, false);
			}
			this.IMng.Pr = null;
			this.IMng.fnDetailPrepare = new UiItemManageBox.FnDetailPrepare(this.showEnhancerDesc);
			this.IMng.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.attachEnhancer);
			this.IMng.fnDescAddition = new UiItemManageBox.FnDescAddition(EnhancerManager.fnDescAddition);
			this.IMng.item_row_skin = "enhancer";
			this.IMng.slice_height = 10f;
			this.IMng.row_height = 48f;
			this.IMng.fnItemRowInitAfter = new UiItemManageBox.FnItemRowInitAfter(EnhancerManager.fnItemRowCreate);
			this.IMng.effect_confusion = base.effect_confusion;
			this.IMng.title_text_content = "";
			this.IMng.stencil_ref = base.bxr_stencil_default;
			this.IMng.ParentBoxDesigner = this.BxR;
		}

		public void showEnhancerDesc(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR)
		{
			EnhancerManager.showEnhancerDesc(Itm, Obt, this.BtmTab.GetDesigner(0));
		}

		public bool attachEnhancer(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			NelItem itemData = BRow.getItemData();
			ItemStorage.ObtainInfo itemInfo = BRow.getItemInfo();
			aBtnItemRow selectedRow = IMng.Inventory.SelectedRow;
			return !(selectedRow == null) && itemInfo != null && EnhancerManager.attachEnhancer(itemData, itemInfo, this.BtmTab.GetDesigner(0), this.TopTab.GetDesigner(0), base.M2D.IMNG.getInventoryEnhancer(), selectedRow);
		}

		private aBtnMagSel MagSel;

		private UiItemManageBox IMng;

		private Designer Tab;

		private Designer DescArea;
	}
}
