using System;
using UnityEngine;
using XX;

namespace nel.gm
{
	public abstract class UiGMC
	{
		public static void newGame()
		{
			UiGMCStat.newGameStat();
			UiGMCItem.item_tab = UiGMCItem.ITEM_CTG.MAIN;
			UiGMCScenario.scn_tab = UiGameMenu.SCENARIO_CTG.LOG;
		}

		internal static void releaseGMCInstance(ref UiGMC Instance)
		{
			if (Instance != null)
			{
				Instance.releaseEvac();
			}
			Instance = null;
		}

		internal UiGMC(UiGameMenu _GM, CATEG _categ, bool _cancelable = true, byte _subarea_top_clms = 0, byte _subarea_top_rows = 0, byte _subarea_btm_clms = 0, byte _subarea_btm_rows = 0, float _subarea_top_row_height = 1f, float _subarea_btm_row_height = 1f)
		{
			this.GM = _GM;
			this.Pr = this.GM.Pr;
			this.BxR = this.GM.BxR;
			this.BxCategory = this.GM.BxCategory;
			this.cancelable = _cancelable;
			this.TopTab = this.GM.TopTab;
			this.BtmTab = this.GM.BtmTab;
			this.BxDesc = this.GM.BxDesc;
			this.categ = _categ;
			this.subarea_top_clms = _subarea_top_clms;
			this.subarea_top_rows = _subarea_top_rows;
			this.subarea_btm_clms = _subarea_btm_clms;
			this.subarea_btm_rows = _subarea_btm_rows;
			this.subarea_top_row_height = _subarea_top_row_height;
			this.subarea_btm_row_height = _subarea_btm_row_height;
			int num = (int)(this.subarea_top_clms * this.subarea_top_rows);
			int num2 = (int)(this.subarea_btm_clms * this.subarea_btm_rows);
			if (num + num2 > 0)
			{
				this.AEvSubArea = new Designer.EvacuateContainer[num + num2];
			}
		}

		internal virtual void initAppearWhole()
		{
			this.initAppearMain();
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				int num2 = (int)((i == 0) ? (this.subarea_top_clms * this.subarea_top_rows) : (this.subarea_btm_clms * this.subarea_btm_rows));
				for (int j = 0; j < num2; j++)
				{
					UiBoxDesigner designer = ((i == 0) ? this.GM.TopTab : this.GM.BtmTab).GetDesigner(j);
					designer.Clear();
					designer.item_margin_x_px = 0f;
					designer.margin_in_lr = 30f;
					designer.margin_in_tb = 6f;
					designer.Focusable(true, true, this.GM.getRightBox().getBox());
					this.initAppearSubAreaInner(designer, num, i == 0);
					num++;
				}
			}
		}

		protected virtual bool initAppearSubAreaInner(UiBoxDesigner Ds, int ari, bool is_top)
		{
			return this.reassignEvacuated(ref this.AEvSubArea[ari], null);
		}

		public virtual bool initAppearMain()
		{
			this.BxR.selectable_loop = this.selectable_loop;
			if (this.reassignEvacuated())
			{
				this.containerResized();
				return true;
			}
			return false;
		}

		public virtual void quitAppear()
		{
			this.saveToEvac();
		}

		public virtual void containerResized()
		{
		}

		internal virtual bool canInitEdit()
		{
			return true;
		}

		internal abstract void initEdit();

		internal abstract void quitEdit();

		internal virtual void runAppearing()
		{
		}

		internal virtual GMC_RES runEdit(float fcnt, bool handle)
		{
			return GMC_RES.CONTINUE;
		}

		internal virtual void hideEditTemporary()
		{
		}

		internal NelM2DBase M2D
		{
			get
			{
				return this.GM.M2D;
			}
		}

		internal virtual void releaseEvac()
		{
			this.releaseEvac(ref this.EvCon);
			if (this.AEvSubArea != null)
			{
				int num = this.AEvSubArea.Length;
				for (int i = 0; i < num; i++)
				{
					this.releaseEvac(ref this.AEvSubArea[i]);
				}
			}
		}

		protected virtual void releaseEvac(ref Designer.EvacuateContainer EvCon)
		{
			if (EvCon.valid)
			{
				EvCon.release(null);
			}
		}

		protected void saveToEvac()
		{
			this.saveToEvac(ref this.EvCon, this.BxR);
			int num = (int)(this.subarea_top_clms * this.subarea_top_rows);
			for (int i = 0; i < num; i++)
			{
				this.saveToEvac(ref this.AEvSubArea[i], this.GM.TopTab.GetDesigner(i));
			}
			int num2 = (int)(this.subarea_btm_clms * this.subarea_btm_rows);
			for (int j = 0; j < num2; j++)
			{
				this.saveToEvac(ref this.AEvSubArea[num + j], this.GM.BtmTab.GetDesigner(j));
			}
		}

		protected void saveToEvac(ref Designer.EvacuateContainer Dep, Designer Ds)
		{
			Dep = new Designer.EvacuateContainer(Ds, false);
		}

		protected bool reassignEvacuated()
		{
			return this.reassignEvacuated(ref this.EvCon, null);
		}

		protected bool reassignEvacuated(ref Designer.EvacuateContainer EvCon, Designer Target = null)
		{
			if (!EvCon.valid)
			{
				return false;
			}
			EvCon.reassign(Target);
			return true;
		}

		protected bool effect_confusion
		{
			get
			{
				return this.GM.effect_confusion;
			}
		}

		public bool isShowingMe()
		{
			return this.GM.isShowingGMC(this);
		}

		public Vector3 InverseTransformPoint(Vector3 position)
		{
			return this.GM.transform.InverseTransformPoint(position);
		}

		internal int bxr_stencil_default
		{
			get
			{
				return this.GM.bxr_stencil_default;
			}
		}

		internal static float bounds_w
		{
			get
			{
				return UiGameMenu.bounds_w;
			}
		}

		internal float extend_right_w
		{
			get
			{
				return this.GM.extend_right_w;
			}
		}

		internal static float bounds_wh
		{
			get
			{
				return UiGameMenu.bounds_wh;
			}
		}

		internal static float bounds_h
		{
			get
			{
				return UiGameMenu.bounds_h;
			}
		}

		internal float right_box_center_x
		{
			get
			{
				return this.GM.right_box_center_x;
			}
		}

		internal float right_w
		{
			get
			{
				return this.GM.right_w;
			}
		}

		internal float right_wh
		{
			get
			{
				return this.GM.right_wh;
			}
		}

		internal float right_last_top_row_height
		{
			get
			{
				return this.GM.right_last_top_row_height;
			}
		}

		internal float right_last_btm_row_height
		{
			get
			{
				return this.GM.right_last_btm_row_height;
			}
		}

		internal UiGameMenu.POSTYPE postype
		{
			get
			{
				return this.GM.postype;
			}
		}

		internal readonly UiGameMenu GM;

		internal PR Pr;

		internal readonly CATEG categ;

		internal UiBoxDesigner BxR;

		internal UiBoxDesigner BxCategory;

		internal UiBoxDesigner BxDesc;

		internal bool cancelable;

		internal readonly byte subarea_top_clms;

		internal readonly byte subarea_top_rows;

		internal readonly byte subarea_btm_clms;

		internal readonly byte subarea_btm_rows;

		internal readonly float subarea_top_row_height = 1f;

		internal readonly float subarea_btm_row_height = 1f;

		internal UiGameMenuTopTab TopTab;

		internal UiGameMenuTopTab BtmTab;

		protected Designer.EvacuateContainer EvCon;

		internal const float BXR_Y = 45f;

		private Designer.EvacuateContainer[] AEvSubArea;

		protected int selectable_loop;
	}
}
