using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class UiCraftBase : UiBoxDesignerFamily, IEventWaitListener
	{
		public virtual string topic_row_skin
		{
			get
			{
				return "recipe";
			}
		}

		private float money_y
		{
			get
			{
				return 0.5f * this.out_h - 22f;
			}
		}

		protected virtual float cartbtn_x
		{
			get
			{
				return this.out_w * 0.5f - 60f;
			}
		}

		protected float cartbtn_y
		{
			get
			{
				return this.money_y + 20f;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.stabilize_key = "CRAFT" + IN.totalframe.ToString();
			this.auto_deactive_gameobject = false;
			this.base_z = this.first_base_z;
			base.gameObject.layer = IN.gui_layer;
			this.M2D = M2DBase.Instance as NelM2DBase;
			EV.getVariableContainer().define("_created", "-1", true);
			this.Sort = new SORT<aBtn>(null);
			this.FD_fnSortAutoCreationItemRow = new Comparison<ItemStorage.IRow>(this.fnSortAutoCreationItemRow);
			UiItemStore.prepareOneSet(this, this.out_w, this.out_h, this.right_h, this.right_h_margin, ref this.BxR, ref this.BxDesc, ref this.BxCmd, 0f, 0f);
			if (this.cartbtn_icon != null)
			{
				this.CartBtn = IN.CreateGob(base.gameObject, "-cartbtn").AddComponent<aBtnNelPopper>();
				this.CartBtn.initializeSkin("popper", this.cartbtn_icon);
				this.CartBtn.pop_forward_snd = "recipe_drop";
				this.CartBtn.position(this.cartbtn_x + 300f, this.cartbtn_y, this.cartbtn_x, this.cartbtn_y, false);
				this.CartBtn.hide();
				IN.setZ(this.CartBtn.transform, -0.125f);
			}
			this.EfpInsert = new EfParticleOnce("alchemy_insert", EFCON_TYPE.UI);
			this.MdEf = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshNormal, this.first_base_z - 0.9f, -1, null, true, true);
			this.M2D.FlagValotStabilize.Add(this.stabilize_key);
			this.deactivate(true);
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.stencil_ref = -1;
			t.Focusable(false, false, null);
			t.WHanim(t.get_swidth_px(), t.get_sheight_px(), false, false);
			return t;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		protected abstract ItemStorage getRecipeTopicDefault();

		public virtual void InitManager(ItemStorage[] _AInventory, ItemStorage _StRecipeTopic, int _init_tptab_index = -1, PxlFrame _PFCompleteCutin = null)
		{
			this.ReleaseStorage();
			if (_AInventory == null)
			{
				_AInventory = global::XX.X.concat<ItemStorage>(this.M2D.IMNG.getInventoryIngredientArray(true), null, -1, -1);
			}
			if (_StRecipeTopic == null)
			{
				_StRecipeTopic = this.getRecipeTopicDefault();
			}
			IN.setZ(base.transform, -4.05f);
			this.AStorage = _AInventory;
			this.StRecipeTopic = _StRecipeTopic;
			this.need_recipe_recheck = true;
			this.init_tptab_index = _init_tptab_index;
			this.PFCompleteCutin = _PFCompleteCutin;
			this.StForUse = new ItemStorage("ingredible", 99).clearAllItems(99);
			this.StForUse.check_quest_target = true;
			this.StForUse.infinit_stockable = (this.StForUse.water_stockable = (this.StForUse.grade_split = true));
			this.StForUse.sort_button_bits = 7;
			this.ItemMng = new UiItemManageBox((this.M2D != null) ? this.M2D.IMNG : null, null)
			{
				cmd_w = 390f,
				item_row_skin = "recipe",
				TrsEvacuateTo = base.transform
			};
			EV.getVariableContainer().define("_final_cooked_item", "", true);
			this.active = true;
			base.gameObject.SetActive(true);
			if (this.stt == UiCraftBase.STATE._NOUSE)
			{
				this.changeState(UiCraftBase.STATE.RECIPE_TOPIC);
			}
		}

		public void ReleaseStorage()
		{
			if (this.ItemMng != null)
			{
				this.ItemMng.quitDesigner(false, false);
			}
			this.quitLunch(false);
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			if (this.ALostFood != null && this.M2D != null)
			{
				int count = this.ALostFood.Count;
				for (int i = 0; i < count; i++)
				{
					NelItemEntry nelItemEntry = this.ALostFood[i];
					this.M2D.IMNG.DiscardStack(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade);
				}
				this.M2D.IMNG.digestDiscardStack(this.M2D.curMap.Pr);
			}
			return base.deactivate(immediate);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
			if (this.MdTutoArrow != null)
			{
				this.MdTutoArrow.destruct();
			}
			if (this.MdEf != null)
			{
				this.MdEf.destruct();
			}
		}

		public override bool runIRD(float fcnt)
		{
			bool handle = this.handle;
			Bench.P("Alchemy UI");
			if (handle != this.pre_handle)
			{
				IN.clearPushDown(true);
				this.need_fine_tutorial_arrow = true;
				this.pre_handle = handle;
				if (this.pre_handle)
				{
					if (this.PreSelected != null && !this.PreSelected.destructed)
					{
						try
						{
							this.PreSelected.Select(false);
						}
						catch
						{
						}
					}
					this.PreSelected = null;
				}
				else
				{
					this.PreSelected = aBtn.PreSelected;
					if (this.PreSelected != null)
					{
						this.PreSelected.Deselect(true);
					}
				}
			}
			bool flag = base.runIRD(fcnt);
			if (this.deactivatable && !flag)
			{
				this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
				Object.Destroy(base.gameObject);
				this.destructed = true;
			}
			else
			{
				if (this.auto_clear_mdef && global::XX.X.D)
				{
					this.MdEf.clear(false, false);
				}
				if (!this.runLunch(fcnt))
				{
					if (this.opening_recipe_book)
					{
						if (this.RecipeBook == null || !this.RecipeBook.isActive())
						{
							if (this.RecipeBook != null)
							{
								this.RecipeBook.auto_deactive_gameobject = true;
							}
							this.opening_recipe_book = false;
							if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC)
							{
								this.BxR.activate();
								this.BxDesc.activate();
							}
							else
							{
								this.BxIng.activate();
								this.BxCmd.activate();
								this.BxRie.activate();
								this.BxConfirm.activate();
								this.BxKD.activate();
								if (this.stt == UiCraftBase.STATE.ING_CONTENT)
								{
									this.BxR.activate();
									this.BxDesc.activate();
								}
							}
							IN.clearSubmitPushDown(true);
							this.RecipeBook = null;
							if (this.Rbk_PreSelected != null)
							{
								this.Rbk_PreSelected.Select(false);
							}
						}
					}
					else if (handle && this.alloc_open_recipe_book && this.M2D != null && (this.stt == UiCraftBase.STATE.RECIPE_TOPIC || this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW || this.stt == UiCraftBase.STATE.ING_CONTENT) && this.M2D.isRbkPD())
					{
						if (this.RecipeBook != null)
						{
							IN.DestroyOne(this.RecipeBook.gameObject);
						}
						UiFieldGuide.NextRevealAtAwake = null;
						this.Rbk_PreSelected = aBtn.PreSelected;
						if (this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
						{
							UiFieldGuide.NextRevealAtAwake = this.TargetIng;
						}
						else if (this.Rbk_PreSelected != null)
						{
							if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC)
							{
								ButtonSkinRecipeRow buttonSkinRecipeRow = this.Rbk_PreSelected.get_Skin() as ButtonSkinRecipeRow;
								if (buttonSkinRecipeRow != null)
								{
									UiFieldGuide.NextRevealAtAwake = buttonSkinRecipeRow.get_Recipe();
								}
							}
							if (UiFieldGuide.NextRevealAtAwake == null)
							{
								ButtonSkinItemRow buttonSkinItemRow = this.Rbk_PreSelected.get_Skin() as ButtonSkinItemRow;
								if (buttonSkinItemRow != null)
								{
									UiFieldGuide.NextRevealAtAwake = buttonSkinItemRow.getItemData();
								}
							}
						}
						if (UiFieldGuide.NextRevealAtAwake == null)
						{
							UiFieldGuide.NextRevealAtAwake = this.TargetRcp;
						}
						this.RecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
						SND.Ui.play("tool_hand_init", false);
						this.deactivate(false);
						this.CartBtn.hide();
						this.active = true;
						IN.setZAbs(this.RecipeBook.transform, base.transform.position.z - 0.125f);
						this.opening_recipe_book = true;
					}
					else
					{
						UiCraftBase.STATE state = this.stt;
						if (state != UiCraftBase.STATE._NOUSE)
						{
							if (state == UiCraftBase.STATE.RECIPE_TOPIC)
							{
								if (handle)
								{
									if (!IN.isUiAddPD() && this.RTabBar != null)
									{
										this.RTabBar.runLRInput(-2);
									}
									this.ItemMng.runEditItem();
									if (IN.isCancel() && this.cancelable)
									{
										this.changeState(UiCraftBase.STATE._NOUSE);
										SND.Ui.play("cancel", false);
									}
								}
							}
							else
							{
								this.runRecipeEdit(handle);
							}
						}
					}
				}
				if (global::XX.X.D && this.compete_eff_t >= 0)
				{
					if (!this.drawCompleteEffect(this.MdEf, this.compete_eff_t, ref this.use_effect))
					{
						this.compete_eff_t = -1;
					}
					else
					{
						this.compete_eff_t += global::XX.X.AF;
						this.use_effect = true;
					}
				}
				if (this.use_effect)
				{
					this.MdEf.updateForMeshRenderer(false);
					this.use_effect = false;
				}
			}
			if (this.need_fine_tutorial_arrow)
			{
				this.fineTutorialArrow();
			}
			else if (global::XX.X.D)
			{
				this.fineTutorialArrowPos();
			}
			Bench.Pend("Alchemy UI");
			return flag;
		}

		protected virtual bool deactivatable
		{
			get
			{
				return this.stt == UiCraftBase.STATE._NOUSE;
			}
		}

		protected virtual void changeState(UiCraftBase.STATE st)
		{
			IN.clearPushDown(false);
			if (this.CartBtn != null)
			{
				this.CartBtn.resetPopPitch();
			}
			this.BxDesc.fineMove();
			UiCraftBase.STATE state = this.stt;
			Bench.P("craft state change");
			string text = Bench.P(FEnum<UiCraftBase.STATE>.ToStr(state));
			string text2 = Bench.P(FEnum<UiCraftBase.STATE>.ToStr(st));
			this.stt = st;
			this.ingredient_insert = false;
			if (this.compete_eff_t >= 0)
			{
				this.compete_eff_t = 160;
			}
			this.quitLunch(false);
			if (state != UiCraftBase.STATE.RECIPE_TOPIC)
			{
				if (state == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
				{
					if (st != UiCraftBase.STATE.RECIPE_TOPIC)
					{
						this.FocusPre = this.FocusIng ?? aBtn.PreSelected;
						if (this.FocusIng != null)
						{
							this.IngConSetValue(global::XX.X.NmI(this.FocusIng.title, 0, false, false), false);
						}
					}
					this.BxIng.hide();
					this.BxConfirm.hide();
					this.BxIng.Focusable(true, false, null);
					this.BxConfirm.Focusable(true, false, null);
				}
			}
			else
			{
				this.BxR.deactivate();
				this.BxDesc.deactivate();
				if (this.stt != UiCraftBase.STATE._NOUSE)
				{
					this.use_effect = true;
				}
			}
			bool flag = false;
			switch (this.stt)
			{
			case UiCraftBase.STATE._NOUSE:
				this.ItemMng.quitDesigner(false, false);
				NEL.ReleaseUiAlchemyObject();
				if (this.active)
				{
					this.deactivate(false);
				}
				this.ReleaseStorage();
				if (this.CartBtn != null)
				{
					this.CartBtn.hide();
				}
				break;
			case UiCraftBase.STATE.RECIPE_TOPIC:
				this.cancelCopleteEff();
				this.use_effect = true;
				if (this.AInsertEffect != null)
				{
					this.AInsertEffect.Clear();
				}
				this.compete_eff_t = -1;
				this.MdEf.clear(false, false);
				if (this.TxSuccess != null)
				{
					this.TxSuccess.gameObject.SetActive(false);
				}
				if (state == UiCraftBase.STATE.RECIPE_CHOOSE_ROW || state == UiCraftBase.STATE.AUTOFILLING_IC || state == UiCraftBase.STATE.AUTOFILLING)
				{
					this.resetBoxPos();
					if (state == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
					{
						SND.Ui.play("cancel", false);
					}
					else
					{
						SND.Ui.play("enter_small", false);
						this.BxRie.Focusable(false, false, null);
						this.BxConfirm.Focusable(false, false, null);
						this.BxKD.Focusable(false, false, null);
						this.BxCmd.Focusable(false, false, null);
						this.BxIng.Focusable(false, false, null);
						this.BxR.Focusable(false, false, null);
						this.BxDesc.Focusable(false, false, null);
						this.BxR.deactivate();
						this.BxDesc.deactivate();
					}
					this.BxRie.deactivate();
					this.BxConfirm.deactivate();
					this.BxKD.deactivate();
					this.BxCmd.deactivate();
					this.BxIng.deactivate();
					this.ItemMng.quitDesigner(false, false);
				}
				if (this.ACreateMem != null && this.ACreateMem.Count > 0)
				{
					UiCraftBase.CookMem cookMem = this.ACreateMem[0];
					if (this.ACreateMem.Count == 1)
					{
						this.ACreateMem = null;
					}
					else
					{
						this.ACreateMem.RemoveAt(0);
					}
					this.prepareRecipe(cookMem.Rcp, cookMem.RcpItm, cookMem.AAIng);
					flag = true;
				}
				else
				{
					this.initRecipeTopicWindow();
					if (this.CartBtn != null)
					{
						this.CartBtn.posSetA(this.cartbtn_x + 300f, this.cartbtn_y, this.cartbtn_x, this.cartbtn_y, true);
						this.CartBtn.hide();
					}
				}
				break;
			case UiCraftBase.STATE.RECIPE_CHOOSE_ROW:
				this.need_kd_fine = true;
				if (state == UiCraftBase.STATE.RECIPE_CONFIRM || this.isCompletionState(state))
				{
					this.BxConfirm.Focusable(false, false, null).activate();
					this.BxConfirm.position(this.confirm_x, this.confirm_y, -1000f, -1000f, false);
					this.BxKD.Focusable(false, false, null).activate();
					this.BxKD.position(this.kd_x, this.kd_y, -1000f, -1000f, false);
					this.BxCmd.activate();
					this.initCmd(false, -1);
					this.BxCmd.position(this.cmd_x, this.cmd_y, -1000f, -1000f, false);
				}
				if (state != UiCraftBase.STATE.ING_CONTENT)
				{
					this.initKD(state == UiCraftBase.STATE.RECIPE_TOPIC);
				}
				else
				{
					this.quitIngredientContent();
				}
				if (state == UiCraftBase.STATE.RECIPE_TOPIC)
				{
					this.initCmd(state == UiCraftBase.STATE.RECIPE_TOPIC, -1);
					if (this.CartBtn != null)
					{
						this.CartBtn.posSetA(this.cartbtn_x + 300f, this.cartbtn_y, this.cartbtn_x, this.cartbtn_y, false);
						this.CartBtn.bind();
					}
					this.BxConfirm.activate();
					this.BxKD.activate();
					this.BxCmd.activate();
					this.BxIng.activate();
					if (!this.read_only)
					{
						this.BxRie.activate();
					}
				}
				else
				{
					this.rie_anim_delay = 0;
					if (this.FocusIng != null)
					{
						this.FocusIng.Select(false);
					}
					else if (this.FocusPre != null)
					{
						this.FocusPre.Select(false);
					}
				}
				this.BxIng.Focusable(false, false, null);
				this.BxConfirm.Focusable(false, false, null);
				this.BxIng.bind();
				this.BxConfirm.bind();
				this.IngConSetValue(-1, false);
				break;
			case UiCraftBase.STATE.ING_CONTENT:
				if (state == UiCraftBase.STATE.AUTOFILLING_IC)
				{
					this.BxR.Focusable(false, false, null);
					this.BxDesc.Focusable(false, false, null);
					this.BxR.bind();
					this.BxDesc.bind();
					this.initKD(false);
					this.need_kd_fine = true;
				}
				else
				{
					this.initIngredientContent();
				}
				if (this.ItemMng.Inventory != null && this.ItemMng.Inventory.SelectedRow != null)
				{
					this.ItemMng.Inventory.SelectedRow.Select(false);
				}
				break;
			case UiCraftBase.STATE.AUTOFILLING:
				this.cancelCopleteEff();
				SND.Ui.play("tool_hand_init", false);
				this.BxIng.Focusable(true, false, null);
				this.BxConfirm.Focusable(true, false, null);
				this.need_kd_fine = true;
				this.initKD(false);
				break;
			case UiCraftBase.STATE.AUTOFILLING_IC:
				this.cancelCopleteEff();
				SND.Ui.play("tool_hand_init", false);
				this.BxR.Focusable(true, false, null);
				this.BxDesc.Focusable(true, false, null);
				this.BxR.hide();
				this.BxDesc.hide();
				this.initKD(false);
				break;
			case UiCraftBase.STATE.RECIPE_CONFIRM:
				if (!this.read_only)
				{
					this.BxConfirm.posSetA(this.confirm_x - 110f - 220f, this.confirm_y, -1000f, -1000f, true);
					this.BxKD.posSetA(this.kd_x - this.kd_w * 0.5f - 220f, this.kd_y, -1000f, -1000f, true);
					this.BxConfirm.Focusable(true, false, null).deactivate();
					this.BxKD.Focusable(true, false, null).deactivate();
					this.initRecipeConfirm();
				}
				break;
			case UiCraftBase.STATE.COMPLETE:
			case UiCraftBase.STATE.COMPLETE_ENOUGH:
				this.ScBoxCmdR = null;
				break;
			}
			if (!flag)
			{
				this.need_fine_tutorial_arrow = true;
			}
			Bench.Pend(text2);
			Bench.Pend(text);
			Bench.Pend("craft state change");
		}

		private void initTemporaryCreateRecipe(RecipeManager.Recipe Rcp, NelItem RcpItm)
		{
			if (this.ACreateMem == null)
			{
				this.ACreateMem = new List<UiCraftBase.CookMem>(2);
			}
			this.ACreateMem.Insert(0, new UiCraftBase.CookMem(this.AAIngCreate, this.TargetRcp, this.TargetRcpItm));
			this.ACreateMem.Insert(0, new UiCraftBase.CookMem(null, Rcp, RcpItm));
			this.changeState(UiCraftBase.STATE.RECIPE_TOPIC);
		}

		private void initRecipeTopicWindow()
		{
			this.BxDesc.Clear();
			this.BxR.Clear();
			this.BxR.margin_in_lr = 28f;
			this.BxR.margin_in_tb = 30f;
			if (this.need_recipe_recheck)
			{
				RecipeManager.flush();
				this.prepareRecipeCreatable();
			}
			string[] rcpTopicTabKeys = this.getRcpTopicTabKeys();
			int num = ((rcpTopicTabKeys != null) ? rcpTopicTabKeys.Length : 1);
			if (this.need_recipe_recheck && this.init_tptab_index != -1)
			{
				this.fineTabIndexFirst(num);
			}
			this.need_recipe_recheck = false;
			this.BxDesc.activate();
			this.BxR.activate();
			float num2 = this.out_w - 360f - 20f;
			this.BxR.posSetDA(this.out_w * 0.5f - num2 * 0.5f, 0f, 0, num2 * 1.25f, true);
			this.BxR.WHanim(num2, this.right_h, true, true);
			this.BxDesc.posSetDA(-this.out_w * 0.5f + 180f, 0f, 2, num2 * 1.25f, true);
			this.BxDesc.WHanim(360f, this.right_h, true, true);
			this.RTabBar = null;
			this.BxR.Clear();
			float use_w = this.BxR.use_w;
			if (num >= 2)
			{
				this.tptab_index %= num;
				this.RTabBar = ColumnRow.CreateT<aBtnNel>(this.BxR, "ctg_tab", "row_tab", this.tptab_index, rcpTopicTabKeys, new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnRecipeTopicTabChanged), use_w, 0f, false, false).LrInput(false);
			}
			else
			{
				this.tptab_index = 0;
			}
			this.ItemTab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, false);
			this.ItemTab.Smallest();
			this.ItemTab.margin_in_tb = 6f;
			this.ItemMng.slice_height = 0f;
			this.BxR.endTab(false);
			this.BxR.Br();
			this.initItemManagerTab(true);
		}

		protected virtual string topic_title_text_content
		{
			get
			{
				return null;
			}
		}

		protected virtual void prepareRecipeCreatable()
		{
		}

		protected void prepareRecipeCreatableS(out uint rcp_use_bits)
		{
			rcp_use_bits = 0U;
			using (BList<RecipeManager.Recipe> blist = ListBuffer<RecipeManager.Recipe>.Pop(0))
			{
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.StRecipeTopic.getWholeInfoDictionary())
				{
					if (TX.isStart(keyValuePair.Key.key, "Recipe_", 0))
					{
						RecipeManager.Recipe recipe = this.getRecipe(keyValuePair.Key);
						if (recipe != null)
						{
							blist.Add(recipe);
							recipe.CInfo.obtain_flag = true;
							rcp_use_bits |= 1U << (int)recipe.categ;
						}
					}
				}
				using (BList<RecipeManager.RecipeIngredient> blist2 = ListBuffer<RecipeManager.RecipeIngredient>.Pop(0))
				{
					for (int i = blist.Count - 1; i >= 0; i--)
					{
						blist[i].checkUseable(blist2, this.AStorage, 1);
					}
				}
			}
		}

		protected virtual void fineTabIndexFirst(int maxtab)
		{
			this.tptab_index = global::XX.X.MMX(0, this.init_tptab_index, maxtab - 1);
		}

		protected virtual void saveTabIndexFirst()
		{
			this.init_tptab_index = this.tptab_index;
		}

		private void initIngUseWindow()
		{
			this.BxDesc.Clear();
			this.BxR.Clear();
			this.BxR.margin_in_lr = 38f;
			this.BxR.margin_in_tb = 6f;
			this.BxDesc.activate();
			this.BxDesc.margin_in_lr = 8f;
			this.BxR.activate();
			float num = this.ing_w * 0.52f - 10f;
			this.BxR.posSetDA(this.ing_x + this.ing_w * 0.5f - num * 0.5f, this.ingct_y, 2, num * 1.25f, true);
			this.BxR.WHanim(num, this.ingct_h, true, true);
			float num2 = this.ing_w - num - 10f;
			this.BxDesc.posSetDA(-this.out_w * 0.5f + num2 * 0.5f, this.ingct_y, 2, num * 1.25f, true);
			this.BxDesc.WHanim(num2, this.ingct_h, true, true);
			this.ItemTab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, false);
			this.ItemTab.Smallest();
			this.ItemTab.margin_in_tb = 3f;
			this.ItemMng.slice_height = 0f;
			this.BxR.endTab(false);
			this.BxR.Br();
			this.initItemManagerTab(true);
		}

		public virtual void changeTopicTab(RecipeManager.RP_CATEG categ)
		{
		}

		public static void changeTopicTabS(UiCraftBase Ui, RecipeManager.RP_CATEG categ, uint rcp_use_bits)
		{
			int num = global::XX.X.bit_index(rcp_use_bits, 1U << (int)categ);
			if (num >= 0 && Ui.RTabBar != null && num < Ui.RTabBar.Length)
			{
				Ui.RTabBar.Get(num).ExecuteOnClick();
			}
		}

		private void initItemManagerTab(bool clear = false)
		{
			Designer itemTab = this.ItemTab;
			if (clear)
			{
				this.ItemMng.quitDesigner(false, true);
				itemTab.Clear();
			}
			this.ItemMng.quitDesigner(false, false);
			this.ItemMng.use_topright_counter = false;
			this.ItemMng.use_grade_stars = false;
			this.ItemMng.auto_select_on_adding_row = false;
			this.ItemMng.title_text_content = null;
			this.ItemMng.fnSortInjectMng = null;
			ItemStorage itemStorage;
			if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC)
			{
				itemStorage = this.StRecipeTopic;
				this.ItemMng.use_topright_counter = this.topic_use_topright_counter;
				this.ItemMng.title_text_content = this.topic_title_text_content;
				this.ItemMng.fnWholeRowsPrepare = new UiItemManageBox.FnWholeRowsPrepare(this.fnRecipeTopicRowsPrepare);
				this.ItemMng.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.fnRecipeChoosen);
				this.ItemMng.fnDescAddition = new UiItemManageBox.FnDescAddition(this.fnRecipeTopicDescAddition);
				this.ItemMng.item_row_skin = this.topic_row_skin;
				this.ItemMng.row_height = 30f;
				this.need_fine_tutorial_arrow = true;
				this.ItemMng.fnSortInjectMng = delegate(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort, out int ret)
				{
					ret = 0;
					this.need_fine_tutorial_arrow = true;
					return false;
				};
				this.ItemMng.detail_main_item_effect = true;
				this.ItemMng.APoolEvacuated = this.APoolEvacuated;
			}
			else
			{
				this.ItemMng.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.fnIngredientChoosen);
				this.ItemMng.fnWholeRowsPrepare = new UiItemManageBox.FnWholeRowsPrepare(this.fnIngredientUseRowsPrepare);
				this.ItemMng.fnDescAddition = new UiItemManageBox.FnDescAddition(this.fnRecipeIngredientDescAddition);
				itemStorage = this.StForUse;
				this.ItemMng.row_height = 22.5f;
				this.ItemMng.item_row_skin = this.ingredient_item_row_skin;
				this.ItemMng.detail_main_item_effect = false;
				this.ItemMng.APoolEvacuated = this.APoolEvacuatedIng;
			}
			this.ItemMng.stencil_ref = 239;
			this.ItemMng.initDesigner(itemStorage, itemTab, this.BxDesc, null, true, true, true);
			if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC)
			{
				this.APoolEvacuated = this.ItemMng.APoolEvacuated;
			}
			else
			{
				this.APoolEvacuatedIng = this.ItemMng.APoolEvacuated;
			}
			this.ItemMng.ParentBoxDesigner = this.BxR;
		}

		protected virtual string[] getRcpTopicTabKeys()
		{
			return null;
		}

		public static string[] getRcpTopicTabKeysS(uint rcp_use_bits)
		{
			int num = global::XX.X.bit_count(rcp_use_bits);
			string[] array = new string[num];
			int num2 = 0;
			for (int i = 0; i < 5; i++)
			{
				if ((rcp_use_bits & (1U << i)) != 0U)
				{
					string[] array2 = array;
					int num3 = num2;
					string text = "&&recipe_categ_";
					RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)i;
					array2[num3] = text + rp_CATEG.ToString().ToLower();
					if (++num2 >= num)
					{
						break;
					}
				}
			}
			return array;
		}

		protected abstract void fnRecipeTopicRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest);

		protected void fnRecipeTopicRowsPrepareS(List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest, RecipeManager.RP_CATEG categ)
		{
			int count = ASource.Count;
			for (int i = 0; i < count; i++)
			{
				ItemStorage.IRow row = ASource[i];
				if (categ == RecipeManager.RP_CATEG.ALOMA)
				{
					if (TX.isStart(row.Data.key, "TrmItem_", 0))
					{
						ADest.Add(row);
					}
				}
				else if (TX.isStart(row.Data.key, "Recipe_", 0))
				{
					RecipeManager.Recipe recipe = this.getRecipe(row.Data);
					if (recipe != null && recipe.categ == categ)
					{
						ADest.Add(row);
					}
				}
			}
		}

		private bool fnRecipeTopicTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (!this.handle)
			{
				return false;
			}
			if (this.tptab_index != cur_value)
			{
				this.tptab_index = cur_value;
				this.initItemManagerTab(true);
			}
			return true;
		}

		protected virtual string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			return def_string;
		}

		protected static string fnRecipeTopicDescAdditionSForBasic(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count, ItemStorage[] AStorage, ItemStorage StRecipeTopic, ref string recipe_have_count)
		{
			if (row == UiItemManageBox.DESC_ROW.DETAIL)
			{
				RecipeManager.Recipe recipeBasic = UiCraftBase.getRecipeBasic(Itm);
				if (recipeBasic != null)
				{
					return recipeBasic.listupIngredients("\n", true, true);
				}
			}
			else if (row == UiItemManageBox.DESC_ROW.NAME)
			{
				RecipeManager.Recipe recipeBasic2 = UiCraftBase.getRecipeBasic(Itm);
				recipe_have_count = UiCraftBase.fineRTHaveCount(recipeBasic2, AStorage, StRecipeTopic);
				return def_string + "  <font size=\"12\">" + recipe_have_count + "</font>";
			}
			return def_string;
		}

		protected static string fineRTHaveCount(RecipeManager.Recipe Rcp, ItemStorage[] AStorage, ItemStorage StPrecious)
		{
			int num = AStorage.Length;
			int[] array = new int[num];
			for (int i = ((Rcp.categ == RecipeManager.RP_CATEG.COOK) ? 0 : (-1)); i < num; i++)
			{
				ItemStorage itemStorage = ((i == -1) ? StPrecious : AStorage[i]);
				if (itemStorage != null)
				{
					array[global::XX.X.Mx(i, 0)] += itemStorage.getCount(Rcp);
				}
			}
			string text = "<img mesh=\"IconNoel0\" width=\"22\" height=\"24\" />x" + array[0].ToString();
			if (num >= 2)
			{
				text = text + "/<img mesh=\"house_inventory\" width=\"22\" height=\"24\" />x" + array[1].ToString();
			}
			return text;
		}

		protected bool fnRecipeChoosen(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			NelItem itemData = BRow.getItemData();
			BRow.getItemInfo();
			IN.clearPushDown(false);
			if (!this.handle)
			{
				return false;
			}
			RecipeManager.Recipe recipe = this.getRecipe(itemData);
			if (recipe != null)
			{
				if (this.TutorialTarget != null && recipe != this.TutorialTarget)
				{
					return false;
				}
				SND.Ui.play("enter_small", false);
				this.saveTabIndexFirst();
				this.prepareRecipe(recipe, itemData, null);
			}
			return false;
		}

		protected virtual void prepareRecipe(RecipeManager.Recipe Rcp, NelItem Itm, List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			this.TargetRcp = Rcp;
			this.TargetRcpItm = Itm;
			this.ItemMng.need_blur_checked_row = false;
			this.ItemMng.quitDesigner(false, true);
			this.AAEntryFinished = null;
			this.prepareRecipeIngredient(AAPre);
		}

		protected virtual void prepareRecipeIngredient(List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			this.StForUse.clearAllItems(99);
			int num = this.AStorage.Length;
			this.StForUse.do_not_input_newer = true;
			for (int i = 0; i < num; i++)
			{
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.AStorage[i].getWholeInfoDictionary())
				{
					if (this.TargetRcp.forNeeds(keyValuePair.Key))
					{
						this.StForUse.AddInfo(keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
			for (int j = 0; j < num; j++)
			{
				this.StForUse.copyNewer(this.AStorage[j], true);
			}
			this.StForUse.do_not_input_newer = false;
			this.StForUse.fineRows(false);
			this.MdEf.clear(false, false);
			this.AInsertEffect = new List<Vector3>();
			this.FocusIng = null;
			this.CompletionImage = null;
			this.PFCompletion = MTR.AItemIcon[this.TargetRcp.get_icon_id()];
			this.AAIngCreate = UiCraftBase.IngEntryRow.allocAA(null, this.TargetRcp);
			this.AIngView = new List<UiCraftBase.IngEntryRow>(64);
			this.rie_r_anim_i = 0;
			this.rie_r_fine_link = -1;
			this.rie_anim_delay = 20;
			this.need_use_finerow = false;
			this.need_ingred_fine = (this.need_kd_fine = (this.recipe_creatable = false));
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.COOK)
			{
				this.Astomach_cost = new List<Stomach.CostData>(this.M2D.IMNG.StmNoel.getEatenCostArray());
				this.stomach_cost_total = this.M2D.IMNG.StmNoel.cost;
				this.stomach_cost_max = this.M2D.IMNG.StmNoel.cost_max;
			}
			aBtn aBtn = this.initRecipeCreateUi();
			this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
			aBtn.Select(false);
			if (AAPre != null)
			{
				num = global::XX.X.Mn(AAPre.Count, this.TargetRcp.AIng.Count);
				for (int k = 0; k < num; k++)
				{
					List<UiCraftBase.IngEntryRow> list = AAPre[k];
					int count = list.Count;
					for (int l = 0; l < count; l++)
					{
						UiCraftBase.IngEntryRow ingEntryRow = list[l];
						if (ingEntryRow.Itm != null && ingEntryRow.generation == 0)
						{
							UiCraftBase.IngEntryRow ingEntryRow2 = this.createEntry(null, null, this.TargetRcp.AIng[k], ingEntryRow.Itm, null, ingEntryRow.grade);
							if (ingEntryRow2 != null)
							{
								this.AAIngCreate[k].Add(ingEntryRow2);
							}
						}
					}
				}
			}
			else if (!this.read_only)
			{
				UiCraftBase.AutoCreationInfo firstAutoCreateInfo = this.getFirstAutoCreateInfo();
				if (firstAutoCreateInfo != null)
				{
					this.createAutoForRecipe(null, firstAutoCreateInfo, this.TargetRcp, 0, true, true);
				}
			}
			this.fineORIngredientEnable(-1);
		}

		protected virtual UiCraftBase.AutoCreationInfo getFirstAutoCreateInfo()
		{
			return new UiCraftBase.AutoCreationInfo
			{
				previous = true
			};
		}

		protected virtual string getIngredientTitle()
		{
			return TX.GetA(this.tx_ingredient_title, this.TargetRcp.title + ((this.TargetRcp.create_count > 1) ? (" x" + this.TargetRcp.create_count.ToString()) : ""));
		}

		private void remakeIngredientRow()
		{
			this.BxIng.Clear().init();
			this.BxIng.XSh(20f).addP(new DsnDataP("", false)
			{
				text = this.getIngredientTitle(),
				TxCol = C32.d2c(4283780170U),
				size = 14f,
				swidth = this.BxIng.use_w - 60f,
				text_auto_condense = true,
				html = true,
				alignx = ALIGN.LEFT,
				name = "ing_row_p"
			}, false);
			this.BxIng.Br();
			Designer designer = this.BxIng.addTab("IngCon", this.BxIng.use_w, this.BxIng.use_h - 10f, this.BxIng.use_w, this.BxIng.use_h - 10f, true);
			designer.Smallest();
			designer.init();
			DsnDataButton dsnDataButton = new DsnDataButton
			{
				skin = this.ingredient_row_skin,
				w = designer.use_w - 15f,
				h = 48f,
				fnHover = new FnBtnBindings(this.fnHoverIngredient),
				fnClick = new FnBtnBindings(this.fnChangedIngredientRow)
			};
			int count = this.TargetRcp.AIng.Count;
			int num = 0;
			aBtn btn = this.BxConfirm.getBtn("&&Cancel");
			aBtnItemRow aBtnItemRow = null;
			for (int i = 0; i < count; i++)
			{
				if (num < this.TargetRcp.Aor_splitter.Length && i >= this.TargetRcp.Aor_splitter[num])
				{
					num++;
					UiBoxDesigner.addPTo(designer, TX.Get("alchemy_ingredient_or", ""), ALIGN.CENTER, 0f, false, 0f, "", -1);
					this.BxIng.Br();
				}
				dsnDataButton.title = (dsnDataButton.name = i.ToString());
				aBtnItemRow aBtnItemRow2 = this.BxIng.addButtonT<aBtnItemRow>(dsnDataButton);
				(aBtnItemRow2.get_Skin() as ButtonSkinAlchemyIngredientRow).setItem(this, this.AAIngCreate, this.TargetRcp, this.TargetRcp.AIng[i], this.StForUse);
				this.BxIng.Br();
				(aBtnItemRow ?? btn).setNaviB(aBtnItemRow2, true, true);
				aBtnItemRow = aBtnItemRow2;
			}
			this.BxIng.endTab(true);
			IN.setZ(designer.transform, 0.0099f);
			if (!this.read_only)
			{
				aBtn aBtn = (this.FocusPre = this.BxConfirm.getBtn("Submit"));
				btn.setNaviT(aBtn, true, true);
				aBtn.setNaviT(aBtnItemRow, true, true);
				return;
			}
			btn.setNaviT(aBtnItemRow, true, true);
		}

		public void IngConSetValue(int i, bool call_bindings = true)
		{
			if (this.TargetRcp == null || this.BxIng == null)
			{
				return;
			}
			for (int j = this.TargetRcp.AIng.Count - 1; j >= 0; j--)
			{
				this.BxIng.getBtn(j.ToString()).SetChecked(i == j, true);
			}
		}

		public aBtnItemRow getIngredientRow(int i)
		{
			return this.BxIng.getBtn(i.ToString()) as aBtnItemRow;
		}

		private void fineORIngredientEnable(int use_index = -1)
		{
			if (this.TargetRcp == null || this.TargetRcp.Aor_splitter.Length == 0)
			{
				return;
			}
			int num = 0;
			int i = 0;
			int count = this.TargetRcp.AIng.Count;
			while (i < count)
			{
				if (num < this.TargetRcp.Aor_splitter.Length && i >= this.TargetRcp.Aor_splitter[num])
				{
					int num2 = i;
					int num3 = ((++num >= this.TargetRcp.Aor_splitter.Length) ? count : this.TargetRcp.Aor_splitter[num]);
					int num4 = -1;
					if (use_index >= 0 && global::XX.X.BTW((float)num2, (float)use_index, (float)num3) && this.TargetRcp.AIng[use_index].isCreatable(this.AAIngCreate[use_index]))
					{
						num4 = use_index;
						i = num3;
					}
					if (num4 < 0)
					{
						while (i < num3)
						{
							if (i != use_index && this.TargetRcp.AIng[i].isCreatable(this.AAIngCreate[i]))
							{
								num4 = i;
								i = num3;
								break;
							}
							i++;
						}
					}
					for (int j = i - 1; j >= num2; j--)
					{
						aBtn ingredientRow = this.getIngredientRow(j);
						bool flag = num4 >= 0 && num4 != j;
						ingredientRow.SetLocked(flag, true, false);
						if (flag)
						{
							for (int k = this.AAIngCreate[j].Count - 1; k >= 0; k--)
							{
								this.removeIngEntry(this.AAIngCreate[j][k], true, true, true);
							}
							this.need_rie_scrollbox_fine = true;
						}
					}
				}
				else
				{
					i++;
					this.getIngredientRow(i).SetLocked(false, true, false);
				}
			}
		}

		private void removeIngEntry(UiCraftBase.IngEntryRow Rie, bool fine_count_ing_con = true, bool remove_from_top_aa = true, bool resort_buttons = true)
		{
			if (this.need_use_finerow)
			{
				this.need_use_finerow = false;
				this.StForUse.fineRows(true);
			}
			this.ingredient_insert = false;
			List<List<UiCraftBase.IngEntryRow>> innerIngredient = Rie.getInnerIngredient();
			if (innerIngredient != null)
			{
				for (int i = innerIngredient.Count - 1; i >= 0; i--)
				{
					List<UiCraftBase.IngEntryRow> list = innerIngredient[i];
					if (list != null)
					{
						for (int j = list.Count - 1; j >= 0; j--)
						{
							this.removeIngEntry(list[j], false, false, true);
						}
					}
				}
			}
			if (fine_count_ing_con && Rie.generation == 0)
			{
				aBtnItemRow ingredientRow = this.getIngredientRow(Rie.Source.index);
				if (ingredientRow != null)
				{
					ingredientRow.fineCount();
				}
			}
			if (remove_from_top_aa)
			{
				((Rie.Parent != null) ? Rie.Parent.getInnerIngredient() : this.AAIngCreate)[Rie.index_ingredient].Remove(Rie);
			}
			Rie.destruct(this, this.StForUse);
			int num = this.AIngView.IndexOf(Rie);
			if (num >= 0)
			{
				aBtnItemRow bindedButton = Rie.getBindedButton();
				Rie.visible = false;
				if (bindedButton != null)
				{
					int index = this.RieCon.getIndex(bindedButton);
					if (index >= 0)
					{
						this.RieCon.Rem(index, false);
						if (resort_buttons)
						{
							this.reindexRieButtons(index);
						}
					}
				}
				this.AIngView.RemoveAt(num);
				this.rie_r_fine_link = ((this.rie_r_fine_link < 0) ? num : global::XX.X.Mn(this.rie_r_fine_link, num));
				if (this.rie_r_anim_i > num)
				{
					this.rie_r_anim_i--;
				}
			}
		}

		private UiCraftBase.IngEntryRow createAutoForRecipe(UiCraftBase.IngEntryRow Parent, UiCraftBase.AutoCreationInfo CInfo, RecipeManager.Recipe Rcp, int min_grade, bool no_execute_fineor = false, bool use_already_stock = true)
		{
			if (Parent == null)
			{
				Rcp = this.TargetRcp;
				use_already_stock = false;
				min_grade = 0;
			}
			int num;
			int i;
			if (use_already_stock && !CInfo.cannot_use_already_food)
			{
				List<RecipeManager.RecipeDish> dishItemList = RecipeManager.getDishItemList(Rcp);
				num = dishItemList.Count;
				for (i = 0; i < num; i++)
				{
					RecipeManager.RecipeDish recipeDish = dishItemList[i];
					ItemStorage.ObtainInfo info = this.StForUse.getInfo(recipeDish.ItemData);
					if (info != null && info.getCountMoreGrade(min_grade) != 0)
					{
						UiCraftBase.IngEntryRow ingEntryRow = this.createEntry(Parent.Parent, CInfo, Parent.Source, recipeDish.ItemData, info, -1);
						if (ingEntryRow != null)
						{
							return ingEntryRow;
						}
					}
				}
			}
			if (!Rcp.CInfo.obtain_flag || (Parent != null && Rcp.created == 0U))
			{
				return null;
			}
			num = Rcp.AIng.Count;
			int[] aor_splitter = Rcp.Aor_splitter;
			List<List<UiCraftBase.IngEntryRow>> list;
			if (Parent != null)
			{
				list = Parent.getInnerIngredient();
				UiCraftBase.IngEntryRow.allocAA(list, Parent.Source.TargetRecipe);
			}
			else
			{
				list = this.AAIngCreate;
				UiCraftBase.IngEntryRow.allocAA(list, this.TargetRcp);
			}
			int num2 = 0;
			i = 0;
			while (i < num)
			{
				if (num2 < aor_splitter.Length && i >= aor_splitter[num2])
				{
					int num3 = ((++num2 >= aor_splitter.Length) ? num : aor_splitter[num2]);
					bool flag = false;
					while (i < num3)
					{
						if (this.createAutoForRecipeColumn(Parent, CInfo, Rcp, i, list[i], true))
						{
							flag = true;
							i = num3;
							break;
						}
						List<UiCraftBase.IngEntryRow> list2 = list[i];
						for (int j = list2.Count - 1; j >= 0; j--)
						{
							this.removeIngEntry(list2[j], true, true, true);
						}
						i++;
					}
					if (!flag)
					{
						Parent = null;
						break;
					}
				}
				else
				{
					if (!this.createAutoForRecipeColumn(Parent, CInfo, Rcp, i, list[i], true) && Parent != null)
					{
						Parent = null;
						break;
					}
					i++;
				}
			}
			if (Parent != null && Parent.generation == 0)
			{
				this.insertViewRow(Parent);
			}
			if (Rcp == this.TargetRcp && !no_execute_fineor)
			{
				this.fineORIngredientEnable(-1);
			}
			return Parent;
		}

		protected virtual bool can_create_auto_without_no_experience(RecipeManager.RecipeIngredient TargetIng)
		{
			return TargetIng.is_tool;
		}

		private bool createAutoForRecipeColumn(UiCraftBase.IngEntryRow Parent, UiCraftBase.AutoCreationInfo CInfo, RecipeManager.Recipe TargetRcp, int ing_i, List<UiCraftBase.IngEntryRow> L, bool check_event_decline = false)
		{
			RecipeManager.RecipeIngredient recipeIngredient = TargetRcp.AIng[ing_i];
			bool flag = true;
			if (((CInfo.previous && TargetRcp.created == 0U) || (check_event_decline && this.evwait != UiCraftBase.EVWAIT.NOUSE && this.evwait < UiCraftBase.EVWAIT.ING_AUTO_FINISHED && this.evwait != UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_FILLED_ALLOW_AUTO)) && !this.can_create_auto_without_no_experience(recipeIngredient))
			{
				return false;
			}
			int num = ((CInfo.quantity == UiCraftBase.AF_QUANTITY.MAXIMUM) ? recipeIngredient.max : recipeIngredient.need);
			for (int i = L.Count; i < num; i++)
			{
				UiCraftBase.IngEntryRow ingEntryRow = this.createAuto(Parent, CInfo, recipeIngredient, i);
				if (ingEntryRow == null)
				{
					if (i < recipeIngredient.need)
					{
						flag = false;
					}
					if (Parent != null)
					{
						break;
					}
					if (!CInfo.previous)
					{
						break;
					}
				}
				else
				{
					ingEntryRow.index = L.Count;
					L.Add(ingEntryRow);
				}
			}
			return flag;
		}

		private UiCraftBase.IngEntryRow createAuto(UiCraftBase.IngEntryRow Parent, UiCraftBase.AutoCreationInfo CInfo, RecipeManager.RecipeIngredient TargetIng, int ing_index)
		{
			RecipeManager.Recipe recipe = ((Parent == null) ? this.TargetRcp : Parent.Source.TargetRecipe);
			if (CInfo.previous && recipe != null && recipe.created > 0U && recipe.getPrevList() != null)
			{
				List<List<UiCraftBase.IngEntryRow>> prevList = recipe.getPrevList();
				int index = TargetIng.index;
				UiCraftBase.IngEntryRow ingEntryRow = null;
				if (prevList.Count > index && prevList[index].Count > ing_index)
				{
					UiCraftBase.IngEntryRow ingEntryRow2 = prevList[index][ing_index];
					ingEntryRow = this.createEntryForPrevious(Parent, CInfo, ingEntryRow2, TargetIng);
				}
				if (ingEntryRow != null)
				{
					return ingEntryRow;
				}
				if (TargetIng.TargetRecipe != null && TargetIng.TargetRecipe.create_count > 0)
				{
					return this.createAutoForRecipe(new UiCraftBase.IngEntryRow(Parent, TargetIng), CInfo, TargetIng.TargetRecipe, TargetIng.grade, false, false);
				}
			}
			else
			{
				if (TargetIng.Target != null)
				{
					return this.createEntry(Parent, CInfo, TargetIng, TargetIng.Target, null, -1);
				}
				if (TargetIng.target_category == (RecipeManager.RPI_CATEG)0 && TargetIng.target_ni_category == NelItem.CATEG.OTHER)
				{
					return this.createAutoForRecipe(new UiCraftBase.IngEntryRow(Parent, TargetIng), CInfo, TargetIng.TargetRecipe, TargetIng.grade, false, true);
				}
				Dictionary<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = this.StForUse.getWholeInfoDictionary();
				if (this.ATempRow == null)
				{
					this.ATempRow = new List<ItemStorage.IRow>(24);
				}
				this.ATempRow.Clear();
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
				{
					if ((TargetIng.target_category == (RecipeManager.RPI_CATEG)0 || keyValuePair.Key.RecipeInfo != null) && ((TargetIng.target_category != (RecipeManager.RPI_CATEG)0) ? ((keyValuePair.Key.RecipeInfo.categ & TargetIng.target_category) > (RecipeManager.RPI_CATEG)0) : ((keyValuePair.Key.category & TargetIng.target_ni_category) == TargetIng.target_ni_category)) && (TargetIng.grade <= 0 || keyValuePair.Value.getCountMoreGrade(TargetIng.grade) != 0))
					{
						this.ATempRow.Add(new ItemStorage.IRow(keyValuePair.Key, keyValuePair.Value, false));
					}
				}
				if (CInfo.previous && this.ATempRow.Count != 1)
				{
					return null;
				}
				UiCraftBase.AutoCreationInfo.CurrentSort = CInfo;
				UiCraftBase.AutoCreationInfo.CurrentIng = TargetIng;
				int count = this.ATempRow.Count;
				global::XX.X.shuffle<ItemStorage.IRow>(this.ATempRow, count, null);
				this.ATempRow.Sort(this.FD_fnSortAutoCreationItemRow);
				for (int i = 0; i < count; i++)
				{
					ItemStorage.IRow row = this.ATempRow[i];
					UiCraftBase.IngEntryRow ingEntryRow3 = this.createEntry(Parent, CInfo, TargetIng, row.Data, row.Info, -1);
					if (ingEntryRow3 != null)
					{
						return ingEntryRow3;
					}
				}
			}
			return null;
		}

		protected virtual int fnSortAutoCreationItemRow(ItemStorage.IRow Ra, ItemStorage.IRow Rb)
		{
			UiCraftBase.AutoCreationInfo currentSort = UiCraftBase.AutoCreationInfo.CurrentSort;
			RecipeManager.RecipeItemInfo recipeInfo = Ra.Data.RecipeInfo;
			RecipeManager.RecipeItemInfo recipeInfo2 = Rb.Data.RecipeInfo;
			if (this.autofilling_use_cost_field && currentSort.cost != UiCraftBase.AF_COST.NONE && recipeInfo.cost != recipeInfo2.cost)
			{
				return (recipeInfo.cost - recipeInfo2.cost) * global::XX.X.MPF(currentSort.cost == UiCraftBase.AF_COST.LOW_COST);
			}
			if (currentSort.kind != UiCraftBase.AF_KIND.NONE && Ra.total != Rb.total)
			{
				return (Ra.total - Rb.total) * global::XX.X.MPF(currentSort.kind == UiCraftBase.AF_KIND.FEW);
			}
			if (currentSort.grade < 5)
			{
				return Rb.Info.nearGradeCountScore((int)currentSort.grade) - Ra.Info.nearGradeCountScore((int)currentSort.grade);
			}
			return 0;
		}

		private UiCraftBase.IngEntryRow createEntryForPrevious(UiCraftBase.IngEntryRow Parent, UiCraftBase.AutoCreationInfo CInfo, UiCraftBase.IngEntryRow Src, RecipeManager.RecipeIngredient TargetIng)
		{
			if (!TargetIng.forNeeds(Src.Itm, true) || TargetIng.grade > Src.grade)
			{
				return null;
			}
			if (TargetIng.TargetRecipe != null)
			{
				return this.createAutoForRecipe(new UiCraftBase.IngEntryRow(Parent, TargetIng), CInfo, TargetIng.TargetRecipe, TargetIng.grade, false, true);
			}
			if (Src.Itm != null)
			{
				return this.createEntry(Parent, CInfo, TargetIng, Src.Itm, null, Src.grade);
			}
			return null;
		}

		private UiCraftBase.IngEntryRow createEntry(UiCraftBase.IngEntryRow Parent, UiCraftBase.AutoCreationInfo CInfo, RecipeManager.RecipeIngredient TargetIng, NelItem ItemData, ItemStorage.ObtainInfo Obt, int grade)
		{
			if (ItemData == null)
			{
				return null;
			}
			if (Obt == null)
			{
				Obt = this.StForUse.getInfo(ItemData);
			}
			if (Obt == null)
			{
				return null;
			}
			int num = 11;
			if (grade < 0)
			{
				if (CInfo == null)
				{
					grade = 2;
				}
				else if (CInfo.grade == 6)
				{
					grade = Obt.few_grade;
				}
				else if (CInfo.grade == 5)
				{
					grade = Obt.enough_grade;
				}
				else
				{
					grade = (int)CInfo.grade;
				}
			}
			int num2 = -1;
			for (int i = 0; i < num; i++)
			{
				int num3 = grade + ((i == 0) ? 0 : ((i + 1) / 2 * global::XX.X.MPF(i % 2 == 1)));
				if (global::XX.X.BTW(0f, (float)num3, 5f) && num3 >= TargetIng.grade && Obt.getCount(num3) != 0)
				{
					num2 = num3;
					break;
				}
			}
			if (num2 < 0)
			{
				return null;
			}
			UiCraftBase.IngEntryRow ingEntryRow = this.insertViewRow(new UiCraftBase.IngEntryRow(Parent, TargetIng, ItemData, num2));
			if (this.reduceOnCreating(ItemData, num2))
			{
				this.StForUse.Reduce(ItemData, 1, num2, this.stt == UiCraftBase.STATE.ING_CONTENT);
			}
			this.need_ingred_fine = true;
			if (this.stt != UiCraftBase.STATE.ING_CONTENT)
			{
				this.need_use_finerow = true;
			}
			return ingEntryRow;
		}

		private UiCraftBase.IngEntryRow insertViewRow(UiCraftBase.IngEntryRow Rie)
		{
			if (this.AIngView.Count > 0 && Rie.Parent == null)
			{
				int count = this.AIngView.Count;
				int num = Rie.insertViewRow(this.AIngView, -2, 0);
				int num2 = this.AIngView.Count - count;
				int num3 = num2 + num;
				this.rie_r_fine_link = ((this.rie_r_fine_link == -1) ? num : global::XX.X.Mn(this.rie_r_fine_link, num));
				this.rie_r_anim_i = global::XX.X.Mn(this.rie_r_fine_link, num);
				for (int i = this.RieCon.Length - 1; i >= num; i--)
				{
					this.RieCon.Get(i).carr_index += num2;
				}
				for (int j = num; j < num3; j++)
				{
					this.RieCon.MakeT<aBtnItemRow>(j.ToString(), "alchemy_use").carr_index = j;
				}
				this.RieCon.carr_index = this.RieCon.Length;
				this.Sort.qSort(this.RieCon.getVector(), (aBtn a, aBtn b) => UiCraftBase.fnSortBtn(a, b), this.RieCon.Length);
				this.need_rie_scrollbox_fine = true;
			}
			return Rie;
		}

		private void reindexRieButtons(int start_i = 0)
		{
			int length = this.RieCon.Length;
			for (int i = global::XX.X.Mx(0, start_i); i < length; i++)
			{
				this.RieCon.Get(i).carr_index = i;
			}
			this.RieCon.carr_index = length;
			this.need_rie_scrollbox_fine = true;
		}

		private static int fnSortBtn(aBtn Ba, aBtn Bb)
		{
			return Ba.carr_index - Bb.carr_index;
		}

		protected float rie_w
		{
			get
			{
				return this.out_w * (this.read_only ? 0f : 0.3f);
			}
		}

		protected float rie_h
		{
			get
			{
				return this.out_h - 50f;
			}
		}

		private float compl_h
		{
			get
			{
				return 240f;
			}
		}

		protected float ing_w
		{
			get
			{
				return this.out_w - this.rie_w - (float)(this.read_only ? 0 : 26);
			}
		}

		private float ing_h
		{
			get
			{
				return this.out_h - this.compl_h - 18f - this.kd_h - 8f;
			}
		}

		protected float ing_x
		{
			get
			{
				return -this.out_w * 0.5f + this.ing_w * 0.5f;
			}
		}

		private float ing_y
		{
			get
			{
				return this.out_center_y + this.out_h * 0.5f - this.ing_h * 0.5f;
			}
		}

		private float confirm_h
		{
			get
			{
				return (float)(140 - (this.read_only ? 40 : 0));
			}
		}

		private float confirm_x
		{
			get
			{
				return -this.out_w * 0.5f + 110f;
			}
		}

		private float confirm_y
		{
			get
			{
				return this.out_center_y - this.out_h * 0.5f + this.compl_h - this.confirm_h * 0.5f;
			}
		}

		private float kd_w
		{
			get
			{
				return this.ing_w;
			}
		}

		private float kd_h
		{
			get
			{
				return 38f;
			}
		}

		private float kd_x
		{
			get
			{
				return this.ing_x;
			}
		}

		private float kd_y
		{
			get
			{
				return this.ing_y - this.ing_h * 0.5f - this.kd_h * 0.5f - 4f;
			}
		}

		private float cmd_w
		{
			get
			{
				return this.ing_w - 220f - 10f;
			}
		}

		private float cmd_h
		{
			get
			{
				return this.compl_h;
			}
		}

		private float cmd_x
		{
			get
			{
				return this.confirm_x + 110f + this.cmd_w * 0.5f + 10f;
			}
		}

		private float cmd_y
		{
			get
			{
				return this.out_center_y - this.out_h * 0.5f + this.cmd_h * 0.5f;
			}
		}

		private float ingct_h
		{
			get
			{
				return this.out_h * 0.55f;
			}
		}

		private float ingct_y
		{
			get
			{
				return this.out_center_y + this.out_h * 0.5f - 40f - this.ingct_h * 0.5f;
			}
		}

		private float z_kd
		{
			get
			{
				return this.first_base_z - 0.5f;
			}
		}

		private float z_kd_selecting
		{
			get
			{
				return this.first_base_z - 0.9f;
			}
		}

		private void resetBoxPos()
		{
			this.BxIng.posSetDA(this.ing_x, this.ing_y, 3, this.ing_h * 1.5f, true).fineMove();
			this.BxConfirm.posSetDA(this.confirm_x, this.confirm_y, 1, this.compl_h + this.confirm_h * 0.5f, true).fineMove();
			this.BxKD.posSetDA(this.kd_x, this.kd_y, 1, this.compl_h + this.kd_h * 0.5f, true).fineMove();
			this.BxCmd.posSetDA(this.cmd_x, this.cmd_y, 1, 180f, true).fineMove();
		}

		private aBtn initRecipeCreateUi()
		{
			if (this.BxIng == null)
			{
				if (!this.read_only)
				{
					this.BxRie = base.Create("rie", this.out_w * 0.5f - this.rie_w * 0.5f, this.out_center_y - this.out_h * 0.5f + this.rie_h * 0.5f, this.rie_w, this.rie_h, 0, this.rie_w * 1.5f, UiBoxDesignerFamily.MASKTYPE.BOX);
				}
				else
				{
					this.BxRie = base.Create("rie", this.out_w * 2f, this.out_center_y, 300f, this.rie_h, 0, 20f, UiBoxDesignerFamily.MASKTYPE.BOX);
				}
				this.BxIng = base.Create("ing", this.ing_x, this.ing_y, this.ing_w, this.ing_h, 3, this.ing_h * 1.5f, UiBoxDesignerFamily.MASKTYPE.BOX);
				IN.setZ(this.BxIng.transform, this.first_base_z);
				this.BxConfirm = base.Create("confirm", this.confirm_x, this.confirm_y, 220f, this.confirm_h, 1, this.compl_h + this.confirm_h * 0.5f, UiBoxDesignerFamily.MASKTYPE.BOX);
				IN.setZ(this.BxConfirm.transform, this.first_base_z - 0.26f);
				this.BxKD = base.Create("KD", this.kd_x, this.kd_y, this.kd_w, this.kd_h, 1, this.compl_h + this.kd_h * 0.5f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxKD.stencil_ref = (this.BxKD.box_stencil_ref_mask = 225);
				this.BxRie.margin_in_lr = 12f;
				this.BxRie.margin_in_tb = 30f;
				this.BxIng.margin_in_lr = 33f;
				this.BxIng.margin_in_tb = 16f;
				this.BxIng.item_margin_y_px = 2f;
				this.BxKD.Small();
				this.BxKD.alignx = ALIGN.CENTER;
				this.z_confirm = this.BxConfirm.transform.localPosition.z;
			}
			IN.setZ(this.BxKD.transform, this.z_kd);
			this.initConfirmBox(true);
			this.FocusIng = null;
			this.BxRie.Clear().init();
			if (this.CartBtn != null)
			{
				this.CartBtn.bind();
			}
			Designer bxRie = this.BxRie;
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.def = -1;
			dsnDataRadio.skin = "alchemy_use";
			dsnDataRadio.keys = new string[0];
			dsnDataRadio.clms = 1;
			dsnDataRadio.margin_h = 0;
			dsnDataRadio.margin_w = 0;
			dsnDataRadio.w = this.BxRie.use_w - 30f;
			dsnDataRadio.h = 30f;
			dsnDataRadio.SCA = new ScrollAppend(239, this.BxRie.use_w, this.BxRie.use_h, 4f, 6f, 0);
			dsnDataRadio.fnChanged = (BtnContainerRadio<aBtn> __BCon, int __c, int __p) => false;
			this.RieCon = bxRie.addRadioT<aBtnItemRow>(dsnDataRadio);
			this.remakeIngredientRow();
			aBtn aBtn = null;
			if (!this.read_only)
			{
				aBtn = (this.FocusPre = this.BxConfirm.getBtn("Submit"));
				this.fineSubmitTitle(aBtn);
				aBtn.SetLocked(true, true, false);
			}
			aBtn btn = this.BxConfirm.getBtn("&&Cancel");
			this.BxCmd.activate();
			this.BxKD.activate();
			this.need_ingred_fine = true;
			return aBtn ?? btn;
		}

		protected void fineSubmitTitle(aBtn B)
		{
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				B.setSkinTitle(TX.Get("Submit_alchemy", ""));
				return;
			}
			B.setSkinTitle(TX.Get("Submit_" + this.TargetRcp.categ.ToString().ToLower(), ""));
		}

		private void initConfirmBox(bool reset_wh = false)
		{
			this.BxConfirm.Clear();
			if (reset_wh)
			{
				this.BxConfirm.WHanim(220f, this.confirm_h, true, true);
			}
			this.BxConfirm.Small();
			this.BxConfirm.margin_in_tb = 32f;
			this.BxConfirm.alignx = ALIGN.CENTER;
			this.BxConfirm.init();
			if (!this.read_only)
			{
				this.BxConfirm.addButtonT<aBtnNel>(new DsnDataButton
				{
					skin = "row_center",
					title = "Submit",
					name = "Submit",
					w = this.BxConfirm.use_w,
					h = 38f,
					fnClick = new FnBtnBindings(this.fnClickRcpInner),
					fnHover = new FnBtnBindings(this.fnBlurIngredient)
				});
			}
			string text = "&&Cancel";
			if (this.ACreateMem != null)
			{
				text = TX.GetA("alchemy_back_to_parent", this.ACreateMem[0].Rcp.title);
			}
			this.BxConfirm.Br().addButtonT<aBtnNel>(new DsnDataButton
			{
				skin = "row_center",
				title = "&&Cancel",
				skin_title = text,
				name = "&&Cancel",
				w = this.BxConfirm.use_w,
				h = 20f,
				fnClick = new FnBtnBindings(this.fnClickRcpInner),
				fnHover = new FnBtnBindings(this.fnBlurIngredient)
			});
			this.BxConfirm.use_button_connection = false;
		}

		private void initKD(bool first = false)
		{
			this.BxKD.Clear();
			if (this.stt == UiCraftBase.STATE.AUTOFILLING || this.stt == UiCraftBase.STATE.AUTOFILLING_IC)
			{
				RecipeManager.RecipeIngredient targetIng = this.TargetIng;
				RecipeManager.Recipe recipe = ((this.stt == UiCraftBase.STATE.AUTOFILLING_IC) ? targetIng.TargetRecipe : this.TargetRcp);
				this.CurCInfo = recipe.CInfo;
				this.BxKD.getBox().frametype = UiBox.FRAMETYPE.MAIN;
				this.BxKD.margin_in_lr = 30f;
				this.BxKD.item_margin_x_px = 0f;
				this.BxKD.item_margin_y_px = 4f;
				this.BxKD.margin_in_tb = 28f;
				float num = 290f;
				this.BxKD.alignx = ALIGN.CENTER;
				this.BxKD.WHanim(470f, num, true, true);
				IN.setZ(this.BxKD.transform, this.z_kd_selecting);
				if (this.FocusIng == null)
				{
					this.BxKD.position(this.confirm_x + 110f + 12f, this.confirm_y + 11f, -1000f, -1000f, false);
				}
				else
				{
					Vector3 vector = base.transform.InverseTransformPoint(this.FocusIng.get_Skin().local2global(new Vector3(30f, -num * 0.5f - 28f, 0f) * 0.015625f, true));
					this.BxKD.position(vector.x * 64f, vector.y * 64f, -1000f, -1000f, false);
				}
				this.BxKD.use_button_connection = true;
				this.BxKD.selectable_loop = 2;
				this.BxKD.init();
				float num2 = 180f;
				float num3 = this.BxKD.use_w - num2 - 4f;
				this.BxKD.P(TX.Get("alchemy_auto_q", ""), ALIGN.CENTER, num2, false, 26f, "");
				aBtnMeterNel aBtnMeterNel = this.BxKD.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
				{
					name = "auto_q",
					title = "auto_q",
					def = (float)this.CurCInfo.grade,
					mn = 0f,
					mx = 6f,
					valintv = 1f,
					h = 26f,
					fnDescConvert = new FnDescConvert(this.fnAutoFillDescQuality)
				}, num3, null, false);
				this.BxKD.Br();
				aBtnMeterNel aBtnMeterNel2 = null;
				if (this.autofilling_use_cost_field)
				{
					this.BxKD.P(TX.Get("alchemy_auto_cost", ""), ALIGN.CENTER, num2, false, 26f, "");
					aBtnMeterNel2 = this.BxKD.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
					{
						name = "auto_cost",
						title = "auto_cost",
						def = (float)this.CurCInfo.cost,
						mn = 0f,
						mx = 2f,
						valintv = 1f,
						h = 26f,
						fnDescConvert = new FnDescConvert(this.fnAutoFillDescCost)
					}, num3, null, false);
					this.BxKD.Br();
				}
				this.BxKD.P(TX.Get("alchemy_auto_kind", ""), ALIGN.CENTER, num2, false, 26f, "");
				aBtnMeterNel aBtnMeterNel3 = this.BxKD.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
				{
					name = "auto_kind",
					title = "auto_kind",
					def = (float)this.CurCInfo.kind,
					mn = 0f,
					mx = 2f,
					valintv = 1f,
					h = 26f,
					fnDescConvert = new FnDescConvert(this.fnAutoFillDescKind)
				}, num3, null, false);
				this.BxKD.Br();
				if (this.FocusIng == null || targetIng.max > targetIng.need)
				{
					this.BxKD.P(TX.Get("alchemy_auto_quantity", ""), ALIGN.CENTER, num2, false, 26f, "");
					int num4 = 0;
					if (num4 == 0 && ((this.FocusIng == null) ? this.recipe_creatable : (targetIng.need <= this.AAIngCreate[targetIng.index].Count)))
					{
						num4 = 1;
					}
					aBtnMeterNel aBtnMeterNel4 = this.BxKD.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
					{
						name = "auto_quantity",
						title = "auto_quantity",
						def = (float)num4,
						mn = 0f,
						mx = 1f,
						valintv = 1f,
						h = 26f,
						fnDescConvert = new FnDescConvert(this.fnAutoFillDescQuantity)
					}, num3, null, false);
					this.BxKD.Br();
					if (this.read_only)
					{
						aBtnMeterNel4.SetLocked(true, true, false);
					}
				}
				if (this.read_only)
				{
					aBtnMeterNel.SetLocked(true, true, false);
					if (aBtnMeterNel2 != null)
					{
						aBtnMeterNel2.SetLocked(true, true, false);
					}
					aBtnMeterNel3.SetLocked(true, true, false);
				}
				this.BxKD.Hr(0.6f, 11f, 11f, 1f);
				this.BxKD.Br();
				List<string> list = new List<string>(new string[] { "&&alchemy_auto_confirm" });
				if (this.cancelable)
				{
					list.Add("&&alchemy_auto_previous");
					if (targetIng != null && targetIng.TargetRecipe != null)
					{
						list.Add("&&alchemy_manual_create");
					}
				}
				list.Add("&&Cancel");
				BtnContainer<aBtn> btnContainer = this.BxKD.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					skin = "row_center",
					titles = list.ToArray(),
					w = this.BxKD.use_w * 0.75f,
					h = 26f,
					margin_h = 0f,
					margin_w = 0f,
					clms = 1,
					fnClick = new FnBtnBindings(this.fnClickedAutoFillBtn),
					fnMaking = delegate(BtnContainer<aBtn> _BCon, aBtn B)
					{
						if (this.read_only && !(B.title == "&&alchemy_manual_create") && !(B.title == "&&alchemy_manual_create"))
						{
							B.SetLocked(false, true, false);
						}
						return true;
					}
				});
				if (!this.read_only)
				{
					btnContainer.Get(0).Select(false);
				}
				else
				{
					(btnContainer.Get("&&alchemy_manual_create") ?? btnContainer.Get("&&Cancel")).Select(false);
				}
				btnContainer.Get(btnContainer.Length - 1).setNaviB(aBtnMeterNel, true, true);
				if (recipe.created == 0U && (targetIng == null || targetIng.TargetRecipe == null || targetIng.TargetRecipe.created == 0U))
				{
					aBtn aBtn = btnContainer.Get("&&alchemy_auto_previous");
					if (aBtn != null)
					{
						aBtn.SetLocked(true, true, false);
						return;
					}
				}
			}
			else
			{
				this.BxKD.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.BxKD.margin(new float[] { 60f, 3f });
				IN.setZ(this.BxKD.transform, this.z_kd);
				this.BxKD.WHanim(this.kd_w, this.kd_h, true, true);
				this.BxKD.margin_in_lr = 44f;
				this.BxKD.margin_in_tb = 3f;
				this.BxKD.alignx = ALIGN.CENTER;
				this.BxKD.init();
				if (first)
				{
					this.BxKD.posSetDA(this.kd_x, this.kd_y, 1, 180f, false);
				}
				else if (this.stt == UiCraftBase.STATE.ING_CONTENT)
				{
					this.initIngredientKDPos();
				}
				else
				{
					this.BxKD.position(this.kd_x, this.kd_y, -1000f, -1000f, false);
				}
				this.BxKD.addP(new DsnDataP("", false)
				{
					name = "KD",
					text = " ",
					alignx = ALIGN.LEFT,
					html = true,
					size = 16f,
					swidth = this.BxKD.use_w - 60f,
					sheight = this.BxKD.use_h,
					TxCol = C32.d2c(4283780170U)
				}, false);
				this.need_kd_fine = true;
			}
		}

		public virtual bool autofilling_use_cost_field
		{
			get
			{
				return this.TargetRcp != null && this.TargetRcp.categ == RecipeManager.RP_CATEG.COOK;
			}
		}

		private void initCmd(bool first, int additional_creatable = -1)
		{
			this.BxCmd.Clear();
			this.BxCmd.Small();
			this.BxCmd.item_margin_x_px = 8f;
			this.MdLunchCostMask = (this.MdLunchCost = null);
			bool flag = false;
			this.BxCmd.margin_in_lr = 40f;
			this.BxCmd.margin_in_tb = 10f;
			float num = 0f;
			if (this.stt == UiCraftBase.STATE.RECIPE_CONFIRM)
			{
				float num2 = this.out_h - this.ing_h - 18f;
				this.BxCmd.position(this.ing_x, -this.out_h * 0.5f + num2 * 0.5f, -1000f, -1000f, false);
				this.BxCmd.WHanim(this.ing_w, num2, true, true);
				this.BxCmd.margin_in_tb = 32f;
				num = -60f;
			}
			else
			{
				this.BxCmd.margin_in_lr = 38f;
				this.BxCmd.margin_in_tb = 20f;
				this.BxCmd.WHanim(this.cmd_w, this.cmd_h, true, true);
				if (first)
				{
					this.BxCmd.posSetDA(this.cmd_x, this.cmd_y, 1, 180f, true);
				}
				else
				{
					this.BxCmd.position(this.cmd_x, this.cmd_y, -1000f, -1000f, false);
				}
			}
			this.BxCmd.init();
			num += this.BxCmd.use_h;
			float use_w = this.BxCmd.use_w;
			FillImageBlock fillImageBlock = this.createCmdFIB(num);
			float num3 = use_w - fillImageBlock.get_swidth_px() - 35f;
			float num4 = num;
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.COOK)
			{
				Designer designer = this.BxCmd.addTab("cmd_r_tab", num3, num, num3, num, false);
				designer.Smallest();
				designer.stencil_ref = 240;
				flag = (designer.use_scroll = true);
				designer.init();
				this.ScBoxCmdR = designer.getScrollBox();
				this.ScBoxCmdR.startAutoScroll(20);
				MultiMeshRenderer multiMeshRenderer = designer.createTempMMRD(false);
				if (UiCraftBase.SMtrLunchCostMask == null)
				{
					UiCraftBase.SMtrLunchCostMask = MTRX.getMtr(BLEND.NORMALST, 752);
					UiCraftBase.SMtrLunchCost = MTRX.getMtr(BLEND.NORMALST, 241);
				}
				this.MdLunchCostMask = multiMeshRenderer.Make(UiCraftBase.SMtrLunchCostMask);
				this.MdLunchCost = multiMeshRenderer.Make(BLEND.NORMAL, MTRX.MIicon);
				this.MdLunchCost.chooseSubMesh(1, false, false);
				this.MdLunchCost.setMaterial(UiCraftBase.SMtrLunchCost, false);
				this.MdLunchCost.chooseSubMesh(2, false, false);
				this.MdLunchCost.connectRendererToTriMulti(multiMeshRenderer.GetMeshRenderer(this.MdLunchCost));
				this.MdLunchCostMask.Scale(0.5f, 0.5f, false);
				this.MdLunchCost.Scale(0.5f, 0.5f, false);
				this.need_redraw_stomach = true;
				designer.addHr(new DsnDataHr().H(UiLunchTimeBase.costbx_h * 0.5f + 8f));
			}
			this.BxCmd.addP(new DsnDataP("", false)
			{
				name = "cmd_r",
				text = this.fineCompletionDetail(false),
				TxCol = C32.d2c(4283780170U),
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				size = 14f,
				html = true,
				swidth = num3,
				sheight = (flag ? 0f : num4),
				text_auto_wrap = true
			}, false);
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.COOK)
			{
				this.BxCmd.Br().addHr(new DsnDataHr().H(12f));
				this.BxCmd.endTab(true);
			}
			if (this.stt == UiCraftBase.STATE.RECIPE_CONFIRM)
			{
				this.BxCmd.Hr(0.78f, 4f, 4f, 1f);
				float use_w2 = this.BxCmd.use_w;
				Designer designer2 = this.BxCmd.addTab("Cmd-bottom", use_w2, this.BxCmd.use_h, use_w2, this.BxCmd.use_h, false);
				designer2.Smallest();
				designer2.margin_in_tb = 2f;
				designer2.margin_in_lr = 0f;
				designer2.item_margin_y_px = 0f;
				designer2.item_margin_x_px = 4f;
				designer2.selectable_loop = 0;
				designer2.alignx = ALIGN.CENTER;
				designer2.use_button_connection = false;
				designer2.init();
				string completedWorkCountString = this.getCompletedWorkCountString();
				if (completedWorkCountString != null)
				{
					this.BxCmd.P(completedWorkCountString, ALIGN.CENTER, 220f, true, 32f, "");
				}
				aBtn aBtn = null;
				aBtn BtnSubmit = null;
				BtnContainerNumCounter<aBtnNumCounter> btnContainerNumCounter = null;
				if (this.alloc_multiple_creation)
				{
					btnContainerNumCounter = this.BxCmd.addNumCounterT<aBtnNumCounterNel>(new DsnDataNumCounter
					{
						name = "create_count",
						minval = 1,
						def = 1,
						maxval = 1 + global::XX.X.Mx(0, additional_creatable),
						digit = 3,
						fnClick = delegate(aBtn _B)
						{
							BtnSubmit.Select(false);
							IN.clearPushDown(false);
							return true;
						},
						h = 32f
					});
					this.BxCmd.XSh(20f);
					aBtn = btnContainerNumCounter.Get(btnContainerNumCounter.Length - 1);
				}
				float num5 = (float)(this.alloc_multiple_creation ? 150 : 200);
				BtnSubmit = this.BxCmd.addButtonT<aBtnNel>(new DsnDataButton
				{
					skin = "row_center",
					title = "&&Submit_alchemy",
					w = num5,
					h = 32f,
					fnClick = new FnBtnBindings(this.fnConfirmRecipeCreate)
				});
				this.fineSubmitTitle(BtnSubmit);
				aBtnNel aBtnNel = this.BxCmd.addButtonT<aBtnNel>(new DsnDataButton
				{
					skin = "row_center",
					title = "&&Cancel",
					w = num5,
					h = 32f,
					fnClick = delegate(aBtn __B)
					{
						SND.Ui.play("cancel", false);
						this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
						return true;
					}
				});
				BtnSubmit.setNaviR(aBtnNel, true, true);
				if (btnContainerNumCounter != null)
				{
					aBtn.setNaviR(BtnSubmit, true, true);
					btnContainerNumCounter.Get(0).setNaviL(aBtnNel, true, true);
				}
				else
				{
					aBtn = BtnSubmit;
					BtnSubmit.setNaviL(aBtnNel, true, true);
				}
				aBtn.Select(false);
				this.BxCmd.endTab(true);
			}
		}

		protected abstract string getCompletedWorkCountString();

		private string getKeyDescRecipeCreation()
		{
			this.need_kd_fine = false;
			string text4;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
				{
					if (this.FocusIng == null)
					{
						stb.AppendTxA("KD_submit", " ");
						if (!this.read_only)
						{
							stb.AppendTxA("alchemy_keydesc_confirm_randomize_all", " ");
						}
						if (this.AIngView.Count != 0)
						{
							stb.AppendTxA("alchemy_keydesc_confirm", " ");
						}
					}
					else if (this.AIngView.Count == 0)
					{
						stb.AppendTxA("alchemy_keydesc_ingredient_empty", "\n");
					}
					else
					{
						stb.AppendTxA("alchemy_keydesc_ingredient", "\n");
					}
				}
				else if (this.stt == UiCraftBase.STATE.ING_CONTENT)
				{
					RecipeManager.RecipeIngredient targetIng = this.TargetIng;
					string text = "";
					if (!this.read_only)
					{
						int count = this.AAIngCreate[targetIng.index].Count;
						string text2 = "alchemy_keydesc_content_basic";
						if (count >= targetIng.max)
						{
							text = "_enough";
						}
						if (this.StForUse.SelectedRow != null)
						{
							stb.AppendTxA(text2 + text, " ");
						}
						if (count > 0)
						{
							stb.AppendTxA("alchemy_keydesc_content_sendback", " ");
						}
					}
					if (targetIng.TargetRecipe != null)
					{
						string text3 = "alchemy_keydesc_content_recipe";
						if (!targetIng.TargetRecipe.CInfo.obtain_flag)
						{
							text = "_cannot";
						}
						stb.AppendTxA(text3 + text, " ");
					}
					stb.AppendTxA("KD_cancel", " ");
				}
				text4 = stb.ToString();
			}
			return text4;
		}

		private void runRecipeEdit(bool handle)
		{
			if (this.need_use_finerow && this.FocusIng == null)
			{
				this.need_use_finerow = false;
				this.StForUse.fineRows(true);
			}
			if (this.need_ingred_fine)
			{
				this.need_ingred_fine = false;
				if (this.AIngView.Count == 0)
				{
					this.need_kd_fine = true;
				}
				if (this.rie_r_anim_i == 0)
				{
					this.AIngView.Clear();
					this.rie_r_fine_link = 0;
					this.need_rie_scrollbox_fine = true;
					this.RieCon.carr_index = 0;
					int count = this.AAIngCreate.Count;
					for (int i = 0; i < count; i++)
					{
						List<UiCraftBase.IngEntryRow> list = this.AAIngCreate[i];
						int count2 = list.Count;
						for (int j = 0; j < count2; j++)
						{
							list[j].insertViewRow(this.AIngView, -1, this.rie_r_anim_i);
						}
					}
				}
			}
			int num = this.RieCon.Length - this.AIngView.Count;
			while (--num >= 0)
			{
				this.RieCon.Rem(this.RieCon.Length - 1, false);
				this.need_rie_scrollbox_fine = true;
			}
			while (this.RieCon.Length < this.AIngView.Count)
			{
				this.RieCon.MakeT<aBtnItemRow>(this.RieCon.Length.ToString(), "alchemy_use");
				this.need_rie_scrollbox_fine = true;
			}
			if (this.stt < UiCraftBase.STATE.RECIPE_CONFIRM)
			{
				this.runIngredientAnimation(false);
			}
			if (this.need_redraw_stomach && global::XX.X.D && this.MdLunchCost != null && this.stt < UiCraftBase.STATE.COMPLETE)
			{
				this.need_redraw_stomach = this.drawLunchCostMeter(null);
			}
			if (handle)
			{
				if (this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
				{
					if (this.need_kd_fine)
					{
						this.BxKD.Get("KD", false).setValue(this.getKeyDescRecipeCreation());
					}
					if ((this.cancelable || this.tutorial_target_row < 0) && IN.isUiRemPD())
					{
						this.removeEntryByKeyInput(IN.isUiShiftO());
					}
					if (IN.isCancel())
					{
						if (this.FocusIng != null && !this.read_only)
						{
							this.BxConfirm.getBtn("Submit").Select(false);
							SND.Ui.play("cursor", false);
						}
						else
						{
							aBtn btn = this.BxConfirm.getBtn("&&Cancel");
							if (btn.isSelected() || aBtn.PreSelected == btn)
							{
								btn.ExecuteOnSubmitKey();
							}
							else
							{
								btn.Select(false);
								SND.Ui.play("cancel", false);
							}
						}
					}
					if (IN.isUiAddPD() && this.canUseAutoFilling())
					{
						this.changeState(UiCraftBase.STATE.AUTOFILLING);
					}
				}
				else if (this.stt == UiCraftBase.STATE.AUTOFILLING)
				{
					if (IN.isCancel() && this.canUseAutoFillingCancel())
					{
						SND.Ui.play("cancel", false);
						this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
					}
				}
				else if (this.stt == UiCraftBase.STATE.AUTOFILLING_IC)
				{
					if (IN.isCancel())
					{
						SND.Ui.play("cancel", false);
						this.changeState(UiCraftBase.STATE.ING_CONTENT);
					}
				}
				else if (this.stt == UiCraftBase.STATE.ING_CONTENT)
				{
					this.ItemMng.runEditItem();
					if (this.ingredient_insert)
					{
						this.ingredientInsertSubmit();
					}
					if (this.need_kd_fine)
					{
						this.BxKD.Get("KD", false).setValue(this.getKeyDescRecipeCreation());
					}
					if (IN.isUiRemPD())
					{
						RecipeManager.RecipeIngredient targetIng = this.TargetIng;
						if (this.AAIngCreate[this.TargetIng.index].Count == 0)
						{
							if (this.canIngredientContentCancel())
							{
								SND.Ui.play("cancel", false);
								this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
							}
						}
						else if (this.removeEntryByKeyInput(IN.isUiShiftO()))
						{
							this.need_kd_fine = true;
						}
					}
					else if (IN.isCancel())
					{
						if (this.canIngredientContentCancel())
						{
							SND.Ui.play("cancel", false);
							this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
						}
					}
					else if (IN.isUiAddPD() && this.canUseAutoFilling() && this.TargetIng.TargetRecipe != null && this.TargetIng.TargetRecipe.CInfo.obtain_flag)
					{
						this.changeState(UiCraftBase.STATE.AUTOFILLING_IC);
					}
				}
				else if (this.stt == UiCraftBase.STATE.RECIPE_CONFIRM)
				{
					if (IN.isCancel())
					{
						SND.Ui.play("cancel", false);
						this.changeState(UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
					}
				}
				else if (this.isCompletionState(this.stt) && IN.isCancel())
				{
					SND.Ui.play("cancel", false);
					BtnContainerRunner btnContainerRunner = this.BxConfirm.Get("ubtn", false) as BtnContainerRunner;
					bool flag = this.compete_eff_t < 0 || (double)this.compete_eff_t >= 100.0;
					int num2 = 0;
					aBtn aBtn;
					for (;;)
					{
						string text = this.completion_state_cancel_btn_key(num2);
						if (text == null)
						{
							goto IL_0541;
						}
						aBtn = btnContainerRunner.Get(text);
						if (aBtn != null)
						{
							break;
						}
						num2++;
					}
					if (aBtn.isSelected() && flag)
					{
						aBtn.ExecuteOnSubmitKey();
					}
					else
					{
						if (this.compete_eff_t >= 0)
						{
							this.compete_eff_t = 160;
						}
						aBtn.Select(false);
					}
				}
			}
			IL_0541:
			if (this.stt <= UiCraftBase.STATE.RECIPE_TOPIC)
			{
				return;
			}
			this.use_effect = false;
			if (global::XX.X.D && this.AInsertEffect.Count > 0)
			{
				this.use_effect = true;
				if (this.MdEf.hasMultipleTriangle())
				{
					this.MdEf.chooseSubMesh(0, false, false);
				}
				for (int k = this.AInsertEffect.Count - 1; k >= 0; k--)
				{
					Vector3 vector = this.AInsertEffect[k];
					this.EfpInsert.f0 = (int)vector.z;
					this.EfpInsert.index = (uint)this.EfpInsert.f0;
					int num3 = IN.totalframe - this.EfpInsert.f0;
					if (!this.EfpInsert.drawTo(this.MdEf, vector.x, vector.y, 0f, 0, (float)num3, 0f))
					{
						this.AInsertEffect.RemoveAt(k);
					}
				}
			}
		}

		private void runIngredientAnimation(bool force = false)
		{
			if (this.rie_r_anim_i < this.AIngView.Count)
			{
				int num = this.rie_anim_delay - 1;
				this.rie_anim_delay = num;
				if (num <= 0 || force)
				{
					while (this.rie_r_anim_i < this.AIngView.Count && (this.AIngView[this.rie_r_anim_i].progressAnim(ref this.rie_r_anim_i) || force))
					{
					}
					if (this.rie_r_anim_i < this.AIngView.Count)
					{
						this.rie_anim_delay = 4;
					}
					else
					{
						this.rie_anim_delay = 0;
					}
				}
			}
			else
			{
				this.rie_anim_delay = 0;
			}
			if (this.rie_r_fine_link >= 0)
			{
				while (this.rie_r_fine_link < this.rie_r_anim_i)
				{
					UiCraftBase.IngEntryRow ingEntryRow = this.AIngView[this.rie_r_fine_link];
					if (!ingEntryRow.visible)
					{
						ButtonSkinAlchemyEntryRow buttonSkinAlchemyEntryRow = this.RieCon.Get(this.rie_r_fine_link).get_Skin() as ButtonSkinAlchemyEntryRow;
						ingEntryRow.bindToButton(buttonSkinAlchemyEntryRow, this.TargetRcp, this.StForUse);
						this.fineEntrySkinState(buttonSkinAlchemyEntryRow.getBtn(), ingEntryRow);
						if (this.CartBtn != null)
						{
							this.CartBtn.initPopping(true, 1);
						}
						if (ingEntryRow.generation == 0)
						{
							RecipeManager.RecipeIngredient offspringSource = ingEntryRow.getOffspringSource();
							aBtnItemRow ingredientRow = this.getIngredientRow(offspringSource.index);
							if (ingredientRow != null)
							{
								ingredientRow.fineCount();
								if (!force)
								{
									this.addInsertEffect(ingredientRow.get_Skin() as ButtonSkinAlchemyIngredientRow, ingEntryRow);
								}
							}
						}
					}
					this.rie_r_fine_link++;
				}
				if (this.rie_r_fine_link >= this.AIngView.Count)
				{
					this.rie_r_fine_link = -1;
				}
			}
			if (this.need_rie_scrollbox_fine)
			{
				this.need_rie_scrollbox_fine = false;
				bool flag = this.TargetRcp.isCreatable(this.AAIngCreate);
				if (flag != this.recipe_creatable)
				{
					this.recipe_creatable = flag;
					if (flag)
					{
						SND.Ui.play("recipe_fullfill", false);
					}
					if (!this.read_only)
					{
						this.BxConfirm.getBtn("Submit").SetLocked(!flag, true, false);
					}
				}
				this.CompletionImage = ((this.AIngView.Count == 0) ? null : this.TargetRcp.createDish(this.AAIngCreate, false, null));
				this.fineCompletionDetail(true);
				this.RieCon.OuterScrollBox.reposition(true);
			}
		}

		private bool removeEntryByKeyInput(bool whole)
		{
			this.ingredient_insert = false;
			if (this.FocusIng == null)
			{
				int num = -1;
				while (this.AIngView.Count > 0)
				{
					UiCraftBase.IngEntryRow offspringEntry = this.AIngView[this.AIngView.Count - 1].getOffspringEntry();
					if (num == -1 && !whole)
					{
						num = this.RieCon.getIndex(offspringEntry.getBindedButton());
					}
					this.removeIngEntry(offspringEntry, !whole, !whole, false);
					if (!whole)
					{
						break;
					}
				}
				this.need_rie_scrollbox_fine = true;
				if (whole)
				{
					int count = this.AAIngCreate.Count;
					this.RieCon.DestroyButtons(false, false);
					for (int i = 0; i < count; i++)
					{
						this.AAIngCreate[i].Clear();
						this.getIngredientRow(i).fineCount();
					}
				}
				else if (num > 0)
				{
					this.reindexRieButtons(num);
				}
				if (this.CartBtn != null)
				{
					this.CartBtn.initPopping(false, 1);
				}
				SND.Ui.play(whole ? "reset_var" : "cancel", false);
				this.fineORIngredientEnable(-1);
				return true;
			}
			int num2 = global::XX.X.NmI(this.FocusIng.title, 0, false, false);
			if (this.AAIngCreate[num2].Count != 0)
			{
				int j = this.AIngView.Count - 1;
				UiCraftBase.IngEntryRow ingEntryRow = null;
				int num3 = 0;
				bool flag = this.TargetRcp.AIng[num2].isCreatable(this.AAIngCreate[num2]);
				while (j >= 0)
				{
					UiCraftBase.IngEntryRow ingEntryRow2 = this.AIngView[j];
					if (ingEntryRow2.generation > 0 || ingEntryRow2.Source.index != num2)
					{
						if (ingEntryRow != null)
						{
							break;
						}
						j--;
					}
					else
					{
						ingEntryRow = ingEntryRow2;
						if (!whole)
						{
							num3 = this.RieCon.getIndex(ingEntryRow2.getBindedButton());
						}
						this.removeIngEntry(ingEntryRow2, false, true, true);
						j--;
						if (!whole)
						{
							break;
						}
					}
				}
				if (ingEntryRow != null)
				{
					this.reindexRieButtons(num3);
				}
				this.getIngredientRow(num2).fineCount();
				if (this.CartBtn != null)
				{
					this.CartBtn.initPopping(false, 1);
				}
				SND.Ui.play(whole ? "reset_var" : "cancel", false);
				if (this.TargetRcp.AIng[num2].isCreatable(this.AAIngCreate[num2]) != flag)
				{
					this.fineORIngredientEnable(-1);
				}
				return true;
			}
			if (this.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
			{
				return false;
			}
			int count2 = this.AAIngCreate.Count;
			int k;
			for (k = 0; k < count2; k++)
			{
				num2 = (num2 + 1) % count2;
				if (this.AAIngCreate[num2].Count != 0)
				{
					this.getIngredientRow(num2).Select(false);
					break;
				}
			}
			if (k == count2)
			{
				this.BxConfirm.getBtn("&&Cancel").Select(false);
			}
			SND.Ui.play("cancel", false);
			return false;
		}

		private bool fnHoverIngredient(aBtn B)
		{
			if (this.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
			{
				return false;
			}
			if (B == this.FocusIng)
			{
				return true;
			}
			this.focusIngredientTargetTo(B as aBtnItemRow);
			return true;
		}

		private void focusIngredientTargetTo(aBtnItemRow B)
		{
			if (B == this.FocusIng)
			{
				return;
			}
			this.FocusIng = B;
			this.need_kd_fine = true;
			int count = this.AIngView.Count;
			for (int i = 0; i < count; i++)
			{
				UiCraftBase.IngEntryRow ingEntryRow = this.AIngView[i];
				aBtnItemRow bindedButton = ingEntryRow.getBindedButton();
				if (!(bindedButton == null))
				{
					this.fineEntrySkinState(bindedButton, ingEntryRow);
				}
			}
		}

		private void fineEntrySkinState(aBtn _B, UiCraftBase.IngEntryRow Rie)
		{
			ButtonSkinAlchemyEntryRow buttonSkinAlchemyEntryRow = _B.get_Skin() as ButtonSkinAlchemyEntryRow;
			buttonSkinAlchemyEntryRow.hilighted = this.FocusIng != null && global::XX.X.NmI(this.FocusIng.title, 0, false, false) == Rie.getOffspringSource().index;
			_B.SetLocked(this.FocusIng != null && !buttonSkinAlchemyEntryRow.hilighted, true, false);
		}

		private bool fnBlurIngredient(aBtn B)
		{
			if (this.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
			{
				return false;
			}
			if (this.FocusIng != null)
			{
				this.focusIngredientTargetTo(null);
			}
			return true;
		}

		private bool fnChangedIngredientRow(aBtn B)
		{
			int num = global::XX.X.NmI(B.title, 0, false, false);
			if (num < 0)
			{
				return true;
			}
			if (this.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW || !this.handle)
			{
				return false;
			}
			if (this.evwait > UiCraftBase.EVWAIT.NOUSE && this.evwait == UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_AUTO)
			{
				return false;
			}
			if (this.tutorial_target_row >= 0 && num != this.tutorial_target_row)
			{
				return false;
			}
			if (B.isLocked())
			{
				CURS.limitVib(B, global::XX.AIM.L);
				SND.Ui.play("locked", false);
				return false;
			}
			this.focusIngredientTargetTo(B as aBtnItemRow);
			this.changeState(UiCraftBase.STATE.ING_CONTENT);
			return true;
		}

		private bool fnClickRcpInner(aBtn B)
		{
			if (this.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW || this.cancelCopleteEff() || !this.handle)
			{
				return false;
			}
			if (B.isLocked())
			{
				SND.Ui.play("locked", false);
				CURS.limitVib(B, global::XX.AIM.L);
				return false;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "Submit"))
				{
					if (title == "&&Cancel")
					{
						if (!this.cancelable)
						{
							return false;
						}
						this.changeState(UiCraftBase.STATE.RECIPE_TOPIC);
					}
				}
				else
				{
					if (this.evwait > UiCraftBase.EVWAIT.NOUSE && this.evwait != UiCraftBase.EVWAIT.COMPLETED && this.evwait != UiCraftBase.EVWAIT.COMPLETED_ALLOW_AUTO && this.evwait != UiCraftBase.EVWAIT.WHOLE_ALLOW_AUTO)
					{
						return false;
					}
					if (this.CompletionImage != null && this.recipe_creatable && !this.read_only)
					{
						this.runIngredientAnimation(true);
						if (this.CompletionImage != null && this.recipe_creatable)
						{
							this.changeState(UiCraftBase.STATE.RECIPE_CONFIRM);
						}
					}
				}
			}
			return true;
		}

		protected virtual FillImageBlock createCmdFIB(float __h)
		{
			FillImageBlock fillImageBlock = this.BxCmd.addImg(new DsnDataImg
			{
				name = "cmd_l",
				MI = MTRX.MIicon,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawCmdImage),
				swidth = 45f,
				sheight = __h
			});
			MeshDrawer meshDrawer = fillImageBlock.getMeshDrawer();
			meshDrawer.chooseSubMesh(1, false, false);
			meshDrawer.setMaterial(MTRX.MtrMeshNormal, false);
			meshDrawer.chooseSubMesh(0, false, false);
			meshDrawer.connectRendererToTriMulti(fillImageBlock.getMeshRenderer());
			return fillImageBlock;
		}

		protected virtual string fineCompletionDetail(bool set_field = false)
		{
			string completionDetail = this.getCompletionDetail();
			if (set_field)
			{
				FillBlock fillBlock = this.BxCmd.Get("cmd_r", false) as FillBlock;
				if (fillBlock != null && !fillBlock.textIs(completionDetail))
				{
					fillBlock.setValue(completionDetail);
					fillBlock.lineSpacing = ((TX.countLine(completionDetail) >= 6) ? 1.1f : 1.3f);
					if (this.ScBoxCmdR != null)
					{
						Designer tab = this.BxCmd.getTab("cmd_r_tab");
						if (tab != null)
						{
							tab.RowRemakeHeightRecalc(fillBlock, this.BxCmd.getDesignerBlockMemory(fillBlock));
						}
						this.ScBoxCmdR.setScrollLevelTo(0f, 0f, false);
						this.ScBoxCmdR.fineAutoScrollDelay(true);
					}
				}
			}
			this.need_redraw_stomach = true;
			return completionDetail;
		}

		protected virtual string getCompletionDetail()
		{
			string text;
			if (this.CompletionImage == null)
			{
				if (this.TargetRcp == null || this.TargetRcp.AIng.Count == 0)
				{
					text = TX.Get("workbench_limit_reached", "");
				}
				else
				{
					text = TX.GetA("alchemy_completion_image", this.TargetRcp.title);
				}
			}
			else if (this.TargetRcp.Completion != null)
			{
				int num = 0;
				if ((this.TargetRcp.Completion.category & NelItem.CATEG.INDIVIDUAL_GRADE) == NelItem.CATEG.OTHER)
				{
					num = this.CompletionImage.calced_grade;
					text = string.Concat(new string[]
					{
						"<img mesh=\"nel_item_grade.",
						num.ToString(),
						"\" width=\"34\" color=\"0x",
						C32.codeToCodeText(4283780170U),
						"\"/>"
					});
				}
				else
				{
					text = "";
				}
				text = TX.add(text, this.TargetRcp.Completion.getDetail(this.M2D.IMNG.getInventory(), num, null, true, true, true), "\n\n");
			}
			else
			{
				text = TX.GetA("Item_for_food_cost", RecipeManager.getCostString(this.CompletionImage.cost));
				text = text + "\n\n" + RecipeManager.getEffectListup(this.CompletionImage, "\n", "Item_for_food_no_effect");
			}
			return text;
		}

		public void addInsertEffect(ButtonSkinAlchemyIngredientRow Sk, UiCraftBase.IngEntryRow Rie)
		{
			if (Sk == null)
			{
				return;
			}
			Vector3 vector = base.transform.InverseTransformPoint(Sk.local2global(Sk.getSlotPosPixel(Rie.index) * 0.015625f, true));
			this.AInsertEffect.Add(new Vector3(vector.x * 64f, vector.y * 64f, (float)IN.totalframe));
		}

		private bool fnClickedAutoFillBtn(aBtn B)
		{
			if (B.title == "&&Cancel")
			{
				if (this.stt == UiCraftBase.STATE.AUTOFILLING && !this.canUseAutoFillingCancel())
				{
					return false;
				}
				this.changeState((this.stt == UiCraftBase.STATE.AUTOFILLING_IC) ? UiCraftBase.STATE.ING_CONTENT : UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
				return true;
			}
			else
			{
				if (B.isLocked())
				{
					SND.Ui.play("locked", false);
					CURS.limitVib(B, global::XX.AIM.L);
					return true;
				}
				if (this.stt != UiCraftBase.STATE.AUTOFILLING_IC)
				{
					RecipeManager.Recipe targetRcp = this.TargetRcp;
				}
				else
				{
					RecipeManager.Recipe targetRecipe = this.TargetIng.TargetRecipe;
				}
				if (B.title == "&&alchemy_manual_create")
				{
					return this.otherRecipeManualCreate(B);
				}
				if (this.read_only)
				{
					return false;
				}
				this.CurCInfo.previous = B.title == "&&alchemy_auto_previous";
				this.CurCInfo.grade = (byte)global::XX.X.NmI(this.BxKD.getValue("auto_q"), 0, false, false);
				int num = global::XX.X.NmI(this.BxKD.getValue("auto_cost"), -1, false, false);
				if (num >= 0)
				{
					this.CurCInfo.cost = (UiCraftBase.AF_COST)num;
				}
				this.CurCInfo.kind = (UiCraftBase.AF_KIND)global::XX.X.NmI(this.BxKD.getValue("auto_kind"), 0, false, false);
				this.CurCInfo.cannot_use_already_food = this.stt == UiCraftBase.STATE.AUTOFILLING_IC;
				string value = this.BxKD.getValue("auto_quantity");
				if (TX.valid(value))
				{
					this.CurCInfo.quantity = (UiCraftBase.AF_QUANTITY)global::XX.X.NmI(value, 0, false, false);
				}
				int ingredient_total = this.ingredient_total;
				int num2 = -1;
				if (this.FocusIng == null)
				{
					this.createAutoForRecipe(null, this.CurCInfo, this.TargetRcp, 0, true, true);
				}
				else
				{
					num2 = global::XX.X.NmI(this.FocusIng.title, 0, false, false);
					this.createAutoForRecipeColumn(null, this.CurCInfo, this.TargetRcp, num2, this.AAIngCreate[num2], false);
				}
				this.fineORIngredientEnable(num2);
				if (this.ingredient_total == ingredient_total)
				{
					SND.Ui.play("locked", false);
					B.setSkinTitle(NEL.error_tag + TX.Get("alchemy_auto_cannot_create", "") + NEL.error_tag_close);
					return false;
				}
				this.changeState((this.stt == UiCraftBase.STATE.AUTOFILLING_IC) ? UiCraftBase.STATE.ING_CONTENT : UiCraftBase.STATE.RECIPE_CHOOSE_ROW);
				return true;
			}
		}

		public bool otherRecipeManualCreate(aBtn B = null)
		{
			if (this.evwait > UiCraftBase.EVWAIT.NOUSE)
			{
				return false;
			}
			if (this.TargetIng.TargetRecipe != null)
			{
				if (this.TargetIng.TargetRecipe.created == 0U)
				{
					if (B != null)
					{
						B.setSkinTitle(NEL.error_tag + TX.Get("alchemy_never_created", "") + NEL.error_tag_close);
					}
					SND.Ui.play("locked", false);
					CURS.limitVib(B, global::XX.AIM.L);
					return false;
				}
				if (this.stt == UiCraftBase.STATE.AUTOFILLING_IC)
				{
					this.quitIngredientContent();
				}
				this.initTemporaryCreateRecipe(this.TargetIng.TargetRecipe, null);
			}
			else if (this.stt == UiCraftBase.STATE.AUTOFILLING_IC)
			{
				this.quitIngredientContent();
			}
			return true;
		}

		public string fnAutoFillDescQuality(string def)
		{
			int num = global::XX.X.NmI(def, 0, false, false);
			if (num == 5)
			{
				return TX.Get("alchemy_auto_enough", "");
			}
			if (num != 6)
			{
				return string.Concat(new string[]
				{
					"<img mesh=\"nel_item_grade.",
					def,
					"\" color=\"0x",
					C32.codeToCodeText(4283780170U),
					"\" /> "
				});
			}
			return TX.Get("alchemy_auto_few", "");
		}

		public string fnAutoFillDescCost(string def)
		{
			UiCraftBase.AF_COST af_COST = (UiCraftBase.AF_COST)global::XX.X.NmI(def, 0, false, false);
			if (af_COST == UiCraftBase.AF_COST.NONE)
			{
				RecipeManager.RP_CATEG rp_CATEG = this.TargetRcp.categ;
				if (rp_CATEG == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
				{
					rp_CATEG = RecipeManager.RP_CATEG.ALCHEMY;
				}
				return TX.Get("alchemy_auto_random_" + rp_CATEG.ToString().ToLower(), "");
			}
			return TX.Get("alchemy_auto_" + af_COST.ToString().ToLower(), "");
		}

		public string fnAutoFillDescKind(string def)
		{
			UiCraftBase.AF_KIND af_KIND = (UiCraftBase.AF_KIND)global::XX.X.NmI(def, 0, false, false);
			if (af_KIND == UiCraftBase.AF_KIND.NONE)
			{
				RecipeManager.RP_CATEG rp_CATEG = this.TargetRcp.categ;
				if (rp_CATEG == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
				{
					rp_CATEG = RecipeManager.RP_CATEG.ALCHEMY;
				}
				return TX.Get("alchemy_auto_random_" + rp_CATEG.ToString().ToLower(), "");
			}
			return TX.Get("alchemy_auto_" + af_KIND.ToString().ToLower(), "");
		}

		public string fnAutoFillDescQuantity(string def)
		{
			return TX.Get("alchemy_auto_quantity_" + ((UiCraftBase.AF_QUANTITY)global::XX.X.NmI(def, 0, false, false)).ToString().ToLower(), "");
		}

		public void initIngredientContent()
		{
			if (this.need_use_finerow)
			{
				this.need_use_finerow = false;
				this.StForUse.fineRows(true);
			}
			ButtonSkin skin = this.FocusIng.get_Skin();
			aBtn btn = skin.getBtn();
			this.RieCon.OuterScrollBox.getScrollBox().reveal(btn.transform, 0f, 0f, false);
			Vector3 vector = this.BxIng.transform.InverseTransformPoint(skin.local2global(Vector3.zero, true));
			float num = this.initIngredientKDPos();
			this.BxIng.position(this.ing_x, -vector.y * 64f + this.ingct_y + this.ingct_h * 0.5f + 36f, -1000f, -1000f, false);
			IN.setZ(this.BxR.transform, this.first_base_z - 0.45f);
			IN.setZ(this.BxDesc.transform, this.first_base_z - 0.45f);
			IN.setZ(this.BxKD.transform, this.first_base_z - 0.52f);
			this.BxConfirm.position(this.confirm_x, this.confirm_y + (num - this.kd_y + 12f), -1000f, -1000f, false);
			this.BxCmd.position(this.cmd_x, this.cmd_y + (num - this.kd_y + 12f), -1000f, -1000f, false);
			this.initIngUseWindow();
			this.need_kd_fine = true;
		}

		public float initIngredientKDPos()
		{
			float num = this.ingct_y - this.ingct_h * 0.5f - this.kd_h * 0.5f - 8f;
			this.BxKD.position(this.kd_x, num, -1000f, -1000f, false);
			return num;
		}

		public void quitIngredientContent()
		{
			this.BxIng.position(this.ing_x, this.ing_y, -1000f, -1000f, false);
			this.BxKD.position(this.kd_x, this.kd_y, -1000f, -1000f, false);
			this.BxConfirm.position(this.confirm_x, this.confirm_y, -1000f, -1000f, false);
			this.BxCmd.position(this.cmd_x, this.cmd_y, -1000f, -1000f, false);
			this.BxR.deactivate();
			this.BxDesc.deactivate();
			IN.setZ(this.BxKD.transform, this.z_kd);
			this.ItemMng.quitDesigner(false, true);
			this.fineORIngredientEnable(this.targeting_index);
			this.need_kd_fine = true;
		}

		private RecipeManager.RecipeIngredient TargetIng
		{
			get
			{
				if (!(this.FocusIng != null))
				{
					return null;
				}
				return this.TargetRcp.AIng[global::XX.X.NmI(this.FocusIng.title, 0, false, false)];
			}
		}

		private int targeting_index
		{
			get
			{
				if (!(this.FocusIng != null))
				{
					return -1;
				}
				return global::XX.X.NmI(this.FocusIng.title, 0, false, false);
			}
		}

		protected virtual string fnRecipeIngredientDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			return def_string;
		}

		private void fnIngredientUseRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			int count = ASource.Count;
			RecipeManager.RecipeIngredient targetIng = this.TargetIng;
			for (int i = 0; i < count; i++)
			{
				ItemStorage.IRow row = ASource[i];
				if (targetIng.forNeeds(row.Data, false))
				{
					ADest.Add(row);
				}
			}
		}

		private bool fnIngredientChoosen(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			BRow.getItemData();
			BRow.getItemInfo();
			IN.clearPushDown(false);
			if (this.StForUse.SelectedRow == null || this.stt != UiCraftBase.STATE.ING_CONTENT || this.cancelCopleteEff() || this.read_only)
			{
				return false;
			}
			RecipeManager.RecipeIngredient targetIng = this.TargetIng;
			if (this.AAIngCreate[targetIng.index].Count >= targetIng.max)
			{
				SND.Ui.play("locked", false);
				CURS.limitVib(this.ItemMng.getSelectingRowBtn(), global::XX.AIM.L);
				return false;
			}
			this.ingredient_insert = true;
			return true;
		}

		public void ingredientInsertSubmit()
		{
			this.ingredient_insert = false;
			if (this.stt != UiCraftBase.STATE.ING_CONTENT || this.StForUse.SelectedRow == null)
			{
				return;
			}
			RecipeManager.RecipeIngredient targetIng = this.TargetIng;
			UiCraftBase.IngEntryRow ingEntryRow = this.createEntry(null, null, targetIng, this.StForUse.SelectedRow.getItemData(), this.StForUse.SelectedRow.getItemInfo(), (int)this.StForUse.SelectedRow.getItemRow().splitted_grade);
			if (ingEntryRow != null)
			{
				ingEntryRow.index = this.AAIngCreate[targetIng.index].Count;
				this.AAIngCreate[targetIng.index].Add(ingEntryRow);
			}
			this.need_kd_fine = true;
		}

		public virtual bool reduceOnCreating(NelItem Itm, int grade)
		{
			return !Itm.is_tool;
		}

		public void initRecipeConfirm()
		{
			this.BxConfirm.deactivate();
			int num = -1;
			if (this.CompletionImage != null && this.alloc_multiple_creation)
			{
				BDic<NelItem, ItemStorage.ObtainInfo> bdic = new BDic<NelItem, ItemStorage.ObtainInfo>(this.AAIngCreate.Count);
				int count = this.AAIngCreate.Count;
				for (int i = 0; i < count; i++)
				{
					List<UiCraftBase.IngEntryRow> list = this.AAIngCreate[i];
					if (list != null)
					{
						int count2 = list.Count;
						for (int j = 0; j < count2; j++)
						{
							list[j].prepareCountCreatable(bdic);
						}
					}
				}
				num = UiCraftBase.IngEntryRow.countCreatable(this, this.StForUse, bdic);
			}
			this.initCmd(false, global::XX.X.Mx(0, num));
		}

		private void pushStorageMem()
		{
			int num = this.AStorage.Length;
			this.ABaStorage = new ByteArray[num];
			for (int i = 0; i < num; i++)
			{
				(this.ABaStorage[i] = this.AStorage[i].writeBinaryTo(new ByteArray(0U))).SeekSet();
			}
		}

		private void popStorageMem()
		{
			if (this.ABaStorage == null)
			{
				return;
			}
			int num = this.AStorage.Length;
			for (int i = 0; i < num; i++)
			{
				this.AStorage[i].readBinaryFrom(this.ABaStorage[i], true, true, false, 9, false);
			}
			this.ABaStorage = null;
		}

		private bool fnConfirmRecipeCreate(aBtn B)
		{
			int num = (this.alloc_multiple_creation ? global::XX.X.NmI(this.BxCmd.getValue("create_count"), 0, true, false) : 1);
			if (num == 0 || this.CompletionImage == null || this.read_only)
			{
				return false;
			}
			this.need_recipe_recheck = true;
			bool flag = false;
			Dictionary<NelItem, ItemStorage.ObtainInfo> ouseIngredient = this.CompletionImage.OUseIngredient;
			int num2 = this.AStorage.Length;
			this.pushStorageMem();
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in ouseIngredient)
			{
				bool flag2 = false;
				if (this.reduceOnCreating(keyValuePair.Key, -1))
				{
					ItemStorage.ObtainInfo obtainInfo = new ItemStorage.ObtainInfo(keyValuePair.Value);
					obtainInfo.countMultiply(num);
					for (int i = 0; i < num2; i++)
					{
						ItemStorage itemStorage = this.AStorage[i];
						ItemStorage.ObtainInfo info = itemStorage.getInfo(keyValuePair.Key);
						if (info != null)
						{
							for (int j = 4; j >= 0; j--)
							{
								if (!this.reduceOnCreating(keyValuePair.Key, j))
								{
									flag2 = true;
								}
								else
								{
									int num3 = global::XX.X.Mn(obtainInfo.getCount(j), info.getCount(j));
									itemStorage.Reduce(keyValuePair.Key, num3, j, false);
									obtainInfo.ReduceCount(num3, j);
								}
							}
						}
					}
					if (obtainInfo.total > 0 && !flag2)
					{
						global::XX.X.de(keyValuePair.Key.key + "で個数エラーが発生", null);
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				this.popStorageMem();
				return false;
			}
			for (int k = 0; k < num2; k++)
			{
				this.AStorage[k].fineRows(true);
			}
			this.CompletionImage.finalizeDishEffect();
			int num4 = num * this.TargetRcp.create_count;
			this.CompletionAddedStorage = null;
			this.AAEntryFinished = this.AAIngCreate;
			if (this.TargetRcp.Completion == null)
			{
				this.CompletionImage.fineTitle(this.AAIngCreate, true, null);
			}
			int num5 = this.addCompletionToStorage(num4);
			if (this.complete_item_key != null)
			{
				EV.getVariableContainer().define("_final_cooked_item", this.complete_item_key, true);
			}
			this.initCreationComplete(num4, num5);
			return true;
		}

		protected virtual int addCompletionToStorage(int add_rest)
		{
			this.CompletionImage.ItemData = null;
			int num = 0;
			NelItem nelItem = this.TargetRcp.Completion;
			int num2 = this.CompletionImage.calced_grade;
			while (add_rest > 0)
			{
				if (nelItem == null)
				{
					if (this.CompletionImage.ItemData == null)
					{
						nelItem = RecipeManager.assignDish(new RecipeManager.RecipeDish(this.CompletionImage, 1));
						this.CompletionImage.ItemData = nelItem;
					}
					else
					{
						nelItem = this.CompletionImage.ItemData;
					}
					this.CompletionImage.referred++;
				}
				if ((nelItem.category & NelItem.CATEG.INDIVIDUAL_GRADE) != NelItem.CATEG.OTHER)
				{
					num2 = (this.CompletionImage.calced_grade = 0);
				}
				this.complete_item_key = nelItem.key;
				bool flag = false;
				int num3 = this.AStorage.Length;
				for (int i = 0; i < num3; i++)
				{
					ItemStorage itemStorage = this.AStorage[i];
					if (itemStorage.isAddable(nelItem))
					{
						int num4 = itemStorage.Add(nelItem, 1, num2, true, true);
						if (num4 > 0)
						{
							if (!nelItem.is_cache_item)
							{
								nelItem.addObtainCount(num4);
							}
							add_rest -= num4;
							flag = true;
							this.CompletionAddedStorage = itemStorage;
							break;
						}
					}
				}
				if (!flag)
				{
					add_rest--;
					num++;
				}
			}
			if (num == 0)
			{
				this.AAIngCreate = UiCraftBase.IngEntryRow.allocAA(null, this.TargetRcp);
				this.remakeIngredientRow();
				this.AIngView.Clear();
				this.rie_r_anim_i = (this.rie_anim_delay = 0);
				this.rie_r_fine_link = -1;
				this.RieCon.DestroyButtons(false, false);
			}
			else
			{
				if (this.ALostFood == null)
				{
					this.ALostFood = new List<NelItemEntry>();
				}
				this.ALostFood.Add(new NelItemEntry(nelItem, num, (byte)num2));
			}
			return num;
		}

		private void initCompleteEffect()
		{
			this.compete_eff_t = 0;
			this.rie_anim_delay = 40;
		}

		protected virtual BtnContainer<aBtn> initCreationComplete(int created, int add_rest)
		{
			this.current_session_lostfood = add_rest;
			this.BxCmd.posSetA(this.cmd_x, this.cmd_y - 180f, -1000f, -1000f, true);
			this.BxCmd.deactivate();
			this.BxConfirm.Clear();
			this.BxConfirm.activate();
			this.BxConfirm.Focusable(false, false, null);
			this.BxConfirm.position(0f, 20f, -1000f, -1000f, false);
			this.BxConfirm.margin_in_lr = 54f;
			this.BxConfirm.margin_in_tb = 58f;
			this.BxConfirm.WHanim(this.out_w * 0.65f, this.out_h * 0.48f, true, true);
			this.BxConfirm.item_margin_x_px = 0f;
			this.BxConfirm.item_margin_y_px = 20f;
			this.BxConfirm.alignx = ALIGN.CENTER;
			this.BxConfirm.init();
			float use_w = this.BxConfirm.use_w;
			float num = this.creation_complete_fib_height;
			FillImageBlock fillImageBlock = this.BxConfirm.addImg(new DsnDataImg
			{
				name = "cmd_l",
				MI = null,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawConfirmImage),
				swidth = use_w * 0.3f,
				sheight = num
			});
			MeshDrawer meshDrawer = fillImageBlock.getMeshDrawer();
			meshDrawer.chooseSubMesh(0, false, false);
			meshDrawer.setMaterial(MTRX.MtrMeshNormal, false);
			meshDrawer.chooseSubMesh(1, false, false);
			meshDrawer.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			meshDrawer.connectRendererToTriMulti(fillImageBlock.getMeshRenderer());
			string localizedName = (this.CompletionImage.Rcp.Completion ?? this.CompletionImage.ItemData).getLocalizedName(this.CompletionImage.calced_grade, null);
			float num2 = 1.18f;
			string completeDialogPrompt = this.getCompleteDialogPrompt(localizedName, created, add_rest, ref num2);
			this.BxConfirm.addP(new DsnDataP("", false)
			{
				name = "cmd_r",
				text = completeDialogPrompt + "\n",
				swidth = use_w * 0.7f - 10f,
				sheight = num,
				html = true,
				size = 14f,
				TxCol = C32.d2c(4283780170U),
				lineSpacing = num2,
				text_auto_wrap = true,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE
			}, true);
			this.BxConfirm.Br();
			string text;
			string text2;
			int num3;
			BtnContainer<aBtn> btnContainer;
			using (BList<string> creationCompleteBtnKeys = this.getCreationCompleteBtnKeys(ListBuffer<string>.Pop(0), add_rest == 0, out text, out text2, out num3))
			{
				btnContainer = this.BxConfirm.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					titlesL = creationCompleteBtnKeys,
					name = "ubtn",
					skin = "row_center",
					margin_w = 0f,
					margin_h = 0f,
					w = use_w / (float)num3 - 2f,
					h = 30f,
					clms = global::XX.X.Mn(creationCompleteBtnKeys.Count, num3),
					fnClick = new FnBtnBindings(this.fnClickComplete),
					default_focus = 1
				});
				this.BxConfirm.cropBounds(-1f, this.BxConfirm.get_sheight_px());
				if (text2 != null)
				{
					btnContainer.Get(text2).click_snd = "cancel";
				}
				this.changeState((add_rest > 0) ? UiCraftBase.STATE.COMPLETE_ENOUGH : UiCraftBase.STATE.COMPLETE);
				if (add_rest == 0)
				{
					this.initCompleteEffect();
				}
				aBtn aBtn = null;
				if (this.ACreateMem != null && add_rest == 0)
				{
					aBtn = btnContainer.Get(creationCompleteBtnKeys.IndexOf(text));
				}
				(aBtn ?? btnContainer.Get(0)).Select(false);
			}
			return btnContainer;
		}

		protected virtual BList<string> getCreationCompleteBtnKeys(BList<string> Akey, bool no_rest, out string confirm_key, out string cancel_key, out int btn_clms)
		{
			btn_clms = 2;
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.COOK && this.CompletionImage.ItemData.is_food && (this.evwait == UiCraftBase.EVWAIT.NOUSE || this.evwait == UiCraftBase.EVWAIT.WHOLE_ALLOW_AUTO))
			{
				Akey.Add(this.CompletionImage.ItemData.is_water ? "&&alchemy_btn_drink_in" : "&&alchemy_btn_eat_in");
				if (this.CompletionAddedStorage != null && this.CompletionAddedStorage.getUnlinkCount(this.CompletionImage.ItemData, -1) > 0 && this.M2D.IMNG.getInventory().getUnlinkCount(NelItem.LunchBox, -1) > 0)
				{
					Akey.Add("&&Item_cmd_pack_in_box");
				}
			}
			string text;
			cancel_key = (text = (no_rest ? "&&Confirm" : "&&alchemy_btn_drop"));
			confirm_key = text;
			Akey.Add(confirm_key);
			if (!no_rest)
			{
				Akey.Add("&&alchemy_btn_cancel");
			}
			return Akey;
		}

		protected virtual string completion_state_cancel_btn_key(int i)
		{
			if (this.stt == UiCraftBase.STATE.COMPLETE_ENOUGH)
			{
				if (i == 0)
				{
					return "&&alchemy_btn_cancel";
				}
				i--;
			}
			if (i == 0)
			{
				return "&&Confirm";
			}
			if (i != 1)
			{
				return null;
			}
			return "&&alchemy_btn_drop";
		}

		protected virtual string getCompleteDialogPrompt(string item_name, int created, int add_rest, ref float lineSpacing)
		{
			string text = ((created <= 1) ? TX.GetA("alchemy_complete", item_name) : TX.GetA("alchemy_complete_multi", item_name, created.ToString()));
			string text2 = "";
			if (this.TargetRcp.Completion != null)
			{
				if (!this.TargetRcp.Completion.individual_grade)
				{
					text2 = string.Concat(new string[]
					{
						"\n <img mesh=\"nel_item_grade.",
						this.CompletionImage.calced_grade.ToString(),
						"\" width=\"34\" height=\"70\" scale=\"1\" color=\"0x",
						C32.codeToCodeText(4283780170U),
						"\"/> \n"
					});
				}
				lineSpacing *= 1.25f;
			}
			text += text2;
			if (add_rest > 0)
			{
				text = text + "\n" + TX.GetA("alchemy_complete_inventory_over", add_rest.ToString());
			}
			return text;
		}

		protected bool cancelCopleteEff()
		{
			this.hideSuccessTx();
			if (this.compete_eff_t >= 0 && (float)this.compete_eff_t <= 100f)
			{
				this.compete_eff_t = 160;
				return true;
			}
			if (this.compete_eff_t >= 0 && this.compete_eff_t <= 160)
			{
				this.compete_eff_t = 160;
			}
			return false;
		}

		private bool fnClickComplete(aBtn B)
		{
			if (this.cancelCopleteEff() || !this.handle)
			{
				return false;
			}
			if (B.isLocked())
			{
				return false;
			}
			B.SetChecked(true, true);
			string title = B.title;
			if (title != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(title);
				if (num <= 985858765U)
				{
					if (num != 655268048U)
					{
						if (num != 838860871U)
						{
							if (num != 985858765U)
							{
								return true;
							}
							if (!(title == "&&Item_cmd_pack_in_box"))
							{
								return true;
							}
						}
						else if (!(title == "&&Confirm"))
						{
							return true;
						}
					}
					else
					{
						if (!(title == "&&alchemy_btn_eat_in"))
						{
							return true;
						}
						goto IL_0358;
					}
				}
				else
				{
					if (num <= 3464531806U)
					{
						if (num != 1865509305U)
						{
							if (num != 3464531806U)
							{
								return true;
							}
							if (!(title == "&&alchemy_btn_cancel"))
							{
								return true;
							}
						}
						else
						{
							if (!(title == "&&alchemy_btn_drop"))
							{
								return true;
							}
							goto IL_0240;
						}
					}
					else if (num != 3998003280U)
					{
						if (num != 4044679368U)
						{
							return true;
						}
						if (!(title == "&&alchemy_btn_cancel_trm"))
						{
							return true;
						}
					}
					else
					{
						if (!(title == "&&alchemy_btn_drink_in"))
						{
							return true;
						}
						goto IL_0358;
					}
					if (B.isLocked() || this.ABaStorage == null)
					{
						SND.Ui.play("locked", false);
						CURS.limitVib(B, global::XX.AIM.L);
						return false;
					}
					if (this.ALostFood != null)
					{
						int num2 = 0;
						int num3 = this.ALostFood.Count - 1;
						while (num3 >= 0 && num2 < this.current_session_lostfood)
						{
							NelItemEntry nelItemEntry = this.ALostFood[num3];
							if (this.CompletionImage.Rcp.Completion != null)
							{
								if (nelItemEntry.Data == this.CompletionImage.Rcp.Completion && (int)nelItemEntry.grade == this.CompletionImage.calced_grade)
								{
									this.ALostFood.RemoveAt(num3);
									num2++;
								}
							}
							else if (nelItemEntry.Data.RecipeInfo != null && nelItemEntry.Data.RecipeInfo.DishInfo == this.CompletionImage)
							{
								this.ALostFood.RemoveAt(num3);
								num2++;
							}
							num3--;
						}
					}
					this.AAEntryFinished = null;
					this.popStorageMem();
					this.prepareRecipeIngredient(null);
					return true;
				}
				IL_0240:
				NelItem nelItem = this.CompletionImage.Rcp.Completion ?? this.CompletionImage.ItemData;
				if (B.title == "&&Item_cmd_pack_in_box" && this.CompletionAddedStorage != null && this.CompletionAddedStorage.getCount(nelItem, -1) > 0)
				{
					ItemStorage.IRow unlinkRow = this.CompletionAddedStorage.getInfo(nelItem).UnlinkRow;
					if (unlinkRow != null)
					{
						this.M2D.IMNG.createWlinkPack(NelItem.LunchBox, this.CompletionAddedStorage, unlinkRow, this.CompletionImage.calced_grade);
					}
				}
				bool flag = B.title == "&&alchemy_btn_drop";
				EV.getVariableContainer().define("_created", nelItem.key, true);
				this.TargetRcp.setPrevList(this.AAEntryFinished);
				this.TargetRcp.created += (uint)this.CompletionImage.referred;
				this.AAEntryFinished = null;
				this.ABaStorage = null;
				if (this.create_just_one)
				{
					this.changeState(UiCraftBase.STATE.RECIPE_TOPIC);
					this.changeState(UiCraftBase.STATE._NOUSE);
					return true;
				}
				this.prepareRecipeIngredient(null);
				if (flag)
				{
					this.initCompleteEffect();
				}
				return true;
				IL_0358:
				bool flag2 = this.initLunch();
				if (flag2 && aBtn.PreSelected == B)
				{
					B.Deselect(true);
				}
				return flag2;
			}
			return true;
		}

		protected virtual bool initLunch()
		{
			this.quitLunch(false);
			if (this.compete_eff_t >= 0)
			{
				this.compete_eff_t = 160;
			}
			this.LunchTime = IN.CreateGob(base.gameObject, "-Lunch").AddComponent<UiLunchTime>();
			this.LunchTime.enabled = false;
			this.LunchTime.default_select_item_key = this.complete_item_key;
			IN.setZ(this.LunchTime.transform, this.first_base_z - 0.65f);
			this.LunchTime.addExternal(this.AStorage);
			UiLunchTime lunchTime = this.LunchTime;
			lunchTime.FD_EatExecute = (UiLunchTimeBase.FnEatExecute)Delegate.Combine(lunchTime.FD_EatExecute, new UiLunchTimeBase.FnEatExecute(delegate(RecipeManager.RecipeDish AppliedDish)
			{
				if (AppliedDish.ItemData == this.CompletionImage.ItemData)
				{
					this.ABaStorage = null;
					int num = 2;
					BtnContainerRunner btnContainerRunner = this.BxConfirm.Get("ubtn", false) as BtnContainerRunner;
					if (btnContainerRunner != null)
					{
						for (int k = 0; k < num; k++)
						{
							aBtn aBtn = btnContainerRunner.BCon.Get((k == 0) ? "&&alchemy_btn_cancel" : "&&alchemy_btn_cancel_trm");
							if (aBtn != null)
							{
								aBtn.SetLocked(true, true, false);
							}
						}
					}
				}
				return true;
			}));
			if (this.ALostFood != null)
			{
				int count = this.ALostFood.Count;
				this.ALostFoodForLunch = new List<NelItem>(count);
				this.ALostItemNotFood = new List<NelItemEntry>(8);
				for (int i = 0; i < count; i++)
				{
					NelItemEntry nelItemEntry = this.ALostFood[i];
					if (nelItemEntry.Data.RecipeInfo == null || nelItemEntry.Data.RecipeInfo.DishInfo == null)
					{
						this.ALostItemNotFood.Add(nelItemEntry);
					}
					else
					{
						int count2 = nelItemEntry.count;
						for (int j = 0; j < count2; j++)
						{
							this.ALostFoodForLunch.Add(nelItemEntry.Data);
						}
					}
				}
				this.LunchTime.addExternal(this.ALostFoodForLunch);
			}
			if (this.stt == UiCraftBase.STATE.COMPLETE || this.stt == UiCraftBase.STATE.COMPLETE_ENOUGH)
			{
				this.BxConfirm.deactivate();
			}
			this.CartBtn.hide();
			return true;
		}

		protected virtual void quitLunch(bool do_not_destruct_element = false)
		{
			if (this.LunchTime != null)
			{
				int num = this.LunchTime.consumeEaten();
				if (!do_not_destruct_element)
				{
					Object.Destroy(this.LunchTime.gameObject);
				}
				else
				{
					this.LunchTime.enabled = true;
				}
				this.LunchTime = null;
				if (this.ALostFoodForLunch != null)
				{
					this.ALostFood.Clear();
					this.ALostFood.AddRange(this.ALostItemNotFood);
					int count = this.ALostFoodForLunch.Count;
					NelItemEntry nelItemEntry = null;
					for (int i = 0; i < count; i++)
					{
						NelItem nelItem = this.ALostFoodForLunch[i];
						if (nelItemEntry == null || nelItemEntry.Data != nelItem)
						{
							nelItemEntry = new NelItemEntry(nelItem, 1, (byte)nelItem.RecipeInfo.DishInfo.calced_grade);
							this.ALostFood.Add(nelItemEntry);
						}
						else
						{
							nelItemEntry.count++;
						}
					}
					this.ALostItemNotFood = null;
					this.ALostFoodForLunch = null;
				}
				if (this.stt == UiCraftBase.STATE.COMPLETE || this.stt == UiCraftBase.STATE.COMPLETE_ENOUGH)
				{
					this.BxConfirm.activate();
					BtnContainerRunner btnContainerRunner = this.BxConfirm.Get("ubtn", false) as BtnContainerRunner;
					if (btnContainerRunner != null)
					{
						for (int j = 0; j < 2; j++)
						{
							aBtn aBtn = btnContainerRunner.Get((j == 0) ? "&&alchemy_btn_eat_in" : "&&alchemy_btn_drink_in");
							if (aBtn != null)
							{
								aBtn.Select(false);
								aBtn.SetChecked(false, true);
								break;
							}
						}
						if (num > 0)
						{
							aBtn aBtn2 = btnContainerRunner.Get("&&alchemy_btn_cancel");
							if (aBtn2 != null)
							{
								aBtn2.SetLocked(false, true, false);
							}
						}
					}
				}
				if (this.CartBtn != null)
				{
					this.CartBtn.bind();
				}
			}
		}

		protected virtual bool runLunch(float fcnt)
		{
			if (this.LunchTime != null)
			{
				if (!this.LunchTime.isActive())
				{
					this.quitLunch(true);
				}
				else
				{
					this.LunchTime.runIRD(fcnt);
				}
				return true;
			}
			return false;
		}

		protected bool fnDrawConfirmImage(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			NelItem nelItem = this.CompletionImage.Rcp.Completion ?? this.CompletionImage.ItemData;
			if (this.CompletionImage != null && nelItem != null)
			{
				Md.Col = MTRX.ColWhite;
				nelItem.drawIconTo(Md, this.StForUse, 0, 1, 0f, 20f, 3f, 0.75f + 0.25f * (float)global::XX.X.ANMT(2, 10f), null);
				update_meshdrawer = true;
			}
			return false;
		}

		protected virtual bool fnDrawCmdImage(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			Md.chooseSubMesh(0, false, false);
			Md.Col = C32.MulA(4288383339U, alpha * (0.6f + 0.4f * global::XX.X.COSIT(100f)));
			float num = ((this.stt == UiCraftBase.STATE.RECIPE_CONFIRM) ? 0f : (this.BxCmd.sheight * 0.36f));
			Md.RotaPF(0f, num, 2f, 2f, 0f, this.PFCompletion, false, false, false, uint.MaxValue, false, 0);
			if (this.recipe_creatable)
			{
				Md.chooseSubMesh(1, false, false);
				Md.Col = C32.MulA(4283780170U, alpha * (0.8f + 0.2f * global::XX.X.COSIT(50f)));
				Md.CheckMark(0f, num - 10f, 55f, 4f, false);
				Md.chooseSubMesh(0, false, false);
			}
			update_meshdrawer = true;
			return false;
		}

		private bool drawLunchCostMeter(Designer Tab = null)
		{
			if (Tab == null)
			{
				Tab = this.BxCmd.getTab("cmd_r_tab");
			}
			if (Tab == null || this.MdLunchCost == null)
			{
				this.MdLunchCost = null;
				this.MdLunchCostMask = null;
				return false;
			}
			float num = -1f;
			float num2 = 0f;
			if (this.CompletionImage != null)
			{
				float num3 = (float)this.CompletionImage.cost;
				float num4 = (float)this.stomach_cost_max - this.stomach_cost_total;
				num = ((num3 > 0f) ? (num4 / num3) : (-1f));
				num2 = global::XX.X.Mn(num3, num4);
			}
			this.MdLunchCostMask.base_px_x = (this.MdLunchCost.base_px_x = (Tab.w - Tab.margin_in_lr * 2f) * 0.5f * 0.5f + (float)((this.stt == UiCraftBase.STATE.RECIPE_CONFIRM) ? 0 : 60));
			this.MdLunchCostMask.base_px_y = (this.MdLunchCost.base_px_y = 2f - UiLunchTimeBase.costbx_h * 0.5f * 0.5f);
			UiLunchTimeBase.drawCostMeterS(this.MdLunchCostMask, true, this.stomach_cost_max, this.Astomach_cost, num, num2);
			UiLunchTimeBase.drawCostMeterS(this.MdLunchCost, false, this.stomach_cost_max, this.Astomach_cost, num, num2);
			this.MdLunchCostMask.updateForMeshRenderer(false);
			this.MdLunchCost.updateForMeshRenderer(false);
			return this.CompletionImage != null;
		}

		protected virtual bool drawCompleteEffect(MeshDrawer Md, int af, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (af == 0)
			{
				SND.Ui.play("recipe_success", false);
				if (this.TxSuccess == null)
				{
					this.TxSuccess = IN.CreateGob(base.gameObject, "-TxSuccess").AddComponent<TextRenderer>();
					this.TxSuccess.TargetFont = TX.getTitleFont();
					this.TxSuccess.html_mode = true;
					this.TxSuccess.Size(60f).Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE)
						.Col(MTRX.ColBlack);
					IN.setZ(this.TxSuccess.transform, -0.55f);
					this.TxSuccess.use_valotile = true;
					this.TxSuccess.Txt(this.getCompleteTelop());
					this.EfpStar = new EfParticleOnce("alchemy_success", EFCON_TYPE.UI);
				}
				else
				{
					this.TxSuccess.gameObject.SetActive(true);
					this.TxSuccess.clearMesh();
				}
				if (!Md.hasMultipleTriangle())
				{
					Md.chooseSubMesh(1, false, false);
					if (this.PFCompleteCutin != null)
					{
						Md.setMaterial(MTRX.getMI(this.PFCompleteCutin).getMtr(BLEND.NORMAL, -1), false);
					}
					Md.chooseSubMesh(2, false, false);
					Md.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, -1), false);
					Md.base_z = 0.0625f;
				}
			}
			Md.chooseSubMesh(0, false, false);
			float num = global::XX.X.ZLINE((float)af, 160f) * 2f;
			if (num >= 2f)
			{
				this.TxSuccess.gameObject.SetActive(false);
				return false;
			}
			float num2 = -global::XX.X.ZCOS(num - 1.13f, 0.87f);
			if (num < 2f)
			{
				float num3 = (float)global::XX.X.IntR((global::XX.X.ZSIN(num, 0.3f) + num2) * IN.h * 0.5f);
				Md.Col = Md.ColGrd.White().mulA(1f + num2).C;
				C32 c = MTRX.cola.White().setA(0f);
				Md.BlurLineW(-IN.wh, 0f, IN.wh, 0f, num3 * 0.75f, num3 * 1.25f, 0, 0f, 0f, 0f, Md.ColGrd, c, c);
			}
			float num4 = ((this.PFCompleteCutin == null) ? 0f : (IN.w * 0.2f));
			this.TxSuccess.alpha = global::XX.X.ZLINE(num, 0.3f) + num2;
			IN.PosP2(this.TxSuccess.transform, (-1.675f + 0.675f * global::XX.X.ZSIN(num, 0.45f)) * num4 + 350f * global::XX.X.ZPOW(num - 1.25f, 0.75f), 0f);
			if (this.PFCompleteCutin != null)
			{
				Md.chooseSubMesh(1, false, false);
				Md.Col = Md.ColGrd.White().mulA(this.TxSuccess.alpha).C;
				Md.RotaPF(num4 * 0.5f, (-1f + global::XX.X.ZLINE(num, 0.25f)) * 80f + 30f, 1f, 1f, 0f, this.PFCompleteCutin, false, false, false, uint.MaxValue, false, 0);
			}
			Md.chooseSubMesh(2, false, false);
			this.EfpStar.drawTo(Md, 0f, 0f, 0f, 0, (float)af, 0f);
			Md.chooseSubMesh(0, false, false);
			return true;
		}

		public void hideSuccessTx()
		{
			if (this.TxSuccess != null)
			{
				this.TxSuccess.gameObject.SetActive(false);
			}
		}

		public virtual string getCompleteTelop()
		{
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				return TX.Get("alchemy_success", "");
			}
			return TX.Get(this.TargetRcp.categ.ToString().ToLower() + "_success", "");
		}

		public void initEventManipulate()
		{
			if (this.evwait == UiCraftBase.EVWAIT.NOUSE)
			{
				this.evwait = UiCraftBase.EVWAIT.PAUSE;
			}
		}

		public bool handle
		{
			get
			{
				return this.evwait == UiCraftBase.EVWAIT.NOUSE || EV.isStoppingEventHandle();
			}
		}

		public bool cancelable
		{
			get
			{
				return this.evwait == UiCraftBase.EVWAIT.NOUSE || this.waiting_cancelable;
			}
		}

		public virtual void EvtRead(StringHolder rER)
		{
			string _ = rER._1;
			if (_ != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(_);
				if (num <= 2393865632U)
				{
					if (num <= 953006739U)
					{
						if (num != 245169777U)
						{
							if (num != 953006739U)
							{
								return;
							}
							if (!(_ == "DEACTIVATE"))
							{
								return;
							}
							if (this.stt != UiCraftBase.STATE.RECIPE_TOPIC)
							{
								this.changeState(UiCraftBase.STATE.RECIPE_TOPIC);
							}
							this.changeState(UiCraftBase.STATE._NOUSE);
						}
						else
						{
							if (!(_ == "ALLOC_MULTIPLE_CREATION"))
							{
								return;
							}
							this.alloc_multiple_creation = rER.Nm(2, 1f) != 0f;
							return;
						}
					}
					else if (num != 1848297187U)
					{
						if (num != 2393865632U)
						{
							return;
						}
						if (!(_ == "WAIT"))
						{
							return;
						}
						if (!FEnum<UiCraftBase.EVWAIT>.TryParse(rER._2U, out this.evwait, true))
						{
							rER.tError("不明な WAIT 指定: " + rER._2U);
							return;
						}
						EV.initWaitFn(new UiCraftBase.WaitListenerUiAlchemyTutorial(this), 0);
						this.need_fine_tutorial_arrow = true;
						return;
					}
					else
					{
						if (!(_ == "CREATE_JUST_ONE"))
						{
							return;
						}
						this.create_just_one = rER.Nm(2, 1f) != 0f;
						return;
					}
				}
				else if (num <= 3124420071U)
				{
					if (num != 3093995666U)
					{
						if (num != 3124420071U)
						{
							return;
						}
						if (!(_ == "CANCELABLE"))
						{
							return;
						}
						this.waiting_cancelable = rER.Nm(2, 1f) != 0f;
						return;
					}
					else
					{
						if (!(_ == "ARROW_IMG"))
						{
							return;
						}
						EvImg pic = EV.Pics.getPic(rER._2, true, true);
						if (pic != null)
						{
							this.PFArrow = pic.PF;
							return;
						}
					}
				}
				else if (num != 3151233726U)
				{
					if (num != 4080126536U)
					{
						return;
					}
					if (!(_ == "TARGET"))
					{
						return;
					}
					RecipeManager.Recipe recipe = this.getRecipe(rER._2);
					if (recipe == null)
					{
						rER.tError("不明なレシピ: " + rER._2);
						return;
					}
					this.TutorialTarget = recipe;
					return;
				}
				else
				{
					if (!(_ == "TARGET_INGREDIENT"))
					{
						return;
					}
					this.tutorial_target_row = rER.Int(2, -1);
					this.need_fine_tutorial_arrow = true;
					return;
				}
			}
		}

		private bool canUseAutoFilling()
		{
			if (this.TargetRcp == null || this.TargetRcp.AIng.Count == 0 || this.read_only)
			{
				return false;
			}
			if (this.evwait == UiCraftBase.EVWAIT.NOUSE || this.evwait == UiCraftBase.EVWAIT.COMPLETED_ALLOW_AUTO || this.evwait == UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_FILLED_ALLOW_AUTO || this.evwait == UiCraftBase.EVWAIT.WHOLE_ALLOW_AUTO)
			{
				return true;
			}
			if (this.evwait == UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_AUTO || this.evwait == UiCraftBase.EVWAIT.ING_AUTO_FINISHED || this.evwait == UiCraftBase.EVWAIT.ING_CONTENT_OR_AUTO)
			{
				int num = -1;
				if (this.FocusIng != null)
				{
					num = global::XX.X.NmI(this.FocusIng.title, -1, false, false);
				}
				return this.tutorial_target_row == num;
			}
			return false;
		}

		private bool canUseAutoFillingCancel()
		{
			return this.evwait != UiCraftBase.EVWAIT.ING_AUTO_FINISHED;
		}

		private bool canIngredientContentCancel()
		{
			if (this.cancelable || this.evwait == UiCraftBase.EVWAIT.ING_CONTENT_OR_AUTO)
			{
				return true;
			}
			RecipeManager.RecipeIngredient targetIng = this.TargetIng;
			return this.evwait == UiCraftBase.EVWAIT.NOUSE || this.tutorial_target_row < 0 || this.AAIngCreate[targetIng.index].Count >= targetIng.need;
		}

		private void fineTutorialArrow()
		{
			this.need_fine_tutorial_arrow = false;
			if (this.PFArrow == null || this.evwait == UiCraftBase.EVWAIT.NOUSE)
			{
				return;
			}
			global::XX.AIM aim = global::XX.AIM.R;
			IDesignerBlock tutorialArrowTarget = this.getTutorialArrowTarget(ref aim);
			this.ArrowFocus = tutorialArrowTarget;
			if (tutorialArrowTarget != null)
			{
				if (this.GobTutorialArrow == null)
				{
					this.GobTutorialArrow = IN.CreateGob(this, "-Tutorial_arrow");
					IN.setZ(this.GobTutorialArrow.transform, -0.4f);
					this.MdTutoArrow = MeshDrawer.prepareMeshRenderer(this.GobTutorialArrow, MTRX.getMtr(BLEND.NORMAL, this.PFArrow, -1), 0f, -1, null, false, false);
					this.MdTutoArrow.RotaPF(0f, 0f, 1f, 1f, 0f, this.PFArrow, false, false, false, uint.MaxValue, false, 0);
					this.MdTutoArrow.updateForMeshRenderer(false);
				}
				this.GobTutorialArrow.transform.localRotation = Quaternion.Euler(0f, 0f, global::XX.CAim.get_agR(global::XX.CAim.get_opposite(aim), 0f) * 180f / 3.1415927f);
				this.GobTutorialArrow.SetActive(true);
				this.ArrowPos = new Vector3((float)global::XX.CAim._XD(aim, 1) * (tutorialArrowTarget.get_swidth_px() * 0.5f + 8f + 12f) * 0.015625f, (float)global::XX.CAim._YD(aim, 1) * (tutorialArrowTarget.get_sheight_px() * 0.5f - 10f + 12f) * 0.015625f, (float)aim);
				this.fineTutorialArrowPos();
				return;
			}
			if (this.GobTutorialArrow != null)
			{
				this.GobTutorialArrow.SetActive(false);
			}
		}

		protected virtual IDesignerBlock getTutorialArrowTarget(ref global::XX.AIM setaim)
		{
			if (this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW && this.tutorial_target_row >= 0)
			{
				return this.getIngredientRow(this.tutorial_target_row);
			}
			UiCraftBase.EVWAIT evwait = this.evwait;
			if (evwait != UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW)
			{
				if (evwait != UiCraftBase.EVWAIT.COMPLETED && evwait != UiCraftBase.EVWAIT.COMPLETED_ALLOW_AUTO)
				{
					goto IL_00B4;
				}
			}
			else
			{
				if (this.stt != UiCraftBase.STATE.RECIPE_TOPIC || this.TutorialTarget == null)
				{
					goto IL_00B4;
				}
				using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(this.TutorialTarget.RecipeItem))
				{
					if (blist != null && blist.Count > 0)
					{
						return blist[0];
					}
					goto IL_00B4;
				}
			}
			if (this.stt == UiCraftBase.STATE.RECIPE_CHOOSE_ROW && this.recipe_creatable)
			{
				return this.BxConfirm.getBtn(this.read_only ? "&&Cancel" : "Submit");
			}
			IL_00B4:
			return null;
		}

		private void fineTutorialArrowPos()
		{
			if (this.ArrowFocus == null)
			{
				return;
			}
			Transform transform = this.GobTutorialArrow.transform;
			global::XX.AIM aim = (global::XX.AIM)this.ArrowPos.z;
			transform.position = this.ArrowFocus.getTransform().position + new Vector3(this.ArrowPos.x, this.ArrowPos.y, 0f) - new Vector3((float)global::XX.CAim._XD(aim, 1), (float)global::XX.CAim._YD(aim, 1)) * (global::XX.X.COSIT(120f) * 12f * 0.015625f);
		}

		private int ingredient_total
		{
			get
			{
				if (this.AAIngCreate == null)
				{
					return 0;
				}
				int num = 0;
				for (int i = this.AAIngCreate.Count - 1; i >= 0; i--)
				{
					List<UiCraftBase.IngEntryRow> list = this.AAIngCreate[i];
					num += list.Count;
				}
				return num;
			}
		}

		protected bool isCompletionState(UiCraftBase.STATE stt)
		{
			return stt == UiCraftBase.STATE.COMPLETE_ENOUGH || stt == UiCraftBase.STATE.COMPLETE;
		}

		public virtual RecipeManager.Recipe getRecipe(string key)
		{
			return UiCraftBase.getRecipeBasic(key);
		}

		public virtual RecipeManager.Recipe getRecipe(NelItem Itm)
		{
			return UiCraftBase.getRecipeBasic(Itm);
		}

		public static RecipeManager.Recipe getRecipeBasic(string key)
		{
			return RecipeManager.Get(key);
		}

		public static RecipeManager.Recipe getRecipeBasic(NelItem Itm)
		{
			return RecipeManager.Get(Itm);
		}

		public static RecipeManager.Recipe getRecipeAllType(NelItem Itm)
		{
			RecipeManager.Recipe recipe;
			if ((recipe = UiCraftBase.getRecipeBasic(Itm)) == null)
			{
				recipe = UiAlchemyWorkBench.getRecipeS(Itm) ?? UiAlchemyTRM.getRecipeS(Itm);
			}
			return recipe;
		}

		protected UiCraftBase.STATE stt = UiCraftBase.STATE._NOUSE;

		protected NelM2DBase M2D;

		protected bool topic_use_topright_counter;

		protected string cartbtn_icon = "pict_pot";

		protected bool read_only;

		public bool alloc_open_recipe_book = true;

		public bool alloc_multiple_creation;

		public bool create_just_one;

		public PxlFrame PFCompleteCutin;

		public string tx_ingredient_title = "alchemy_ingredient_title";

		public string ingredient_row_skin = "alchemy_ingredient";

		public string ingredient_item_row_skin = "normal";

		private ItemStorage StForUse;

		public readonly float out_w = IN.w * 0.83f;

		private readonly float right_w = IN.w * 0.83f - 360f - 26f;

		public float out_h = IN.h * 0.86f;

		private readonly float right_h = IN.h * 0.86f - 40f;

		private readonly float right_h_margin = 16f;

		protected const string tag_inclease = "<font color=\"ff:#1335DF\">";

		protected const string tag_declease = "<font color=\"ff:#DE148D\">";

		private const float desc_w = 360f;

		private const float cmd_w_count_select = 390f;

		protected float creation_complete_fib_height = 120f;

		protected bool auto_clear_mdef = true;

		private bool use_effect;

		private bool opening_recipe_book;

		protected ItemStorage[] AStorage;

		protected ItemStorage StRecipeTopic;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		protected UiBoxDesigner BxCmd;

		protected UiBoxDesigner BxRie;

		protected UiBoxDesigner BxIng;

		protected UiBoxDesigner BxConfirm;

		protected UiBoxDesigner BxKD;

		public bool destructed;

		protected aBtnNelPopper CartBtn;

		protected UiItemManageBox ItemMng;

		protected int init_tptab_index;

		protected int tptab_index;

		private ColumnRow RTabBar;

		private Designer ItemTab;

		public bool need_recipe_recheck;

		private bool ingredient_insert;

		private SORT<aBtn> Sort;

		private EfParticleOnce EfpInsert;

		private EfParticleOnce EfpStar;

		private float z_confirm;

		private List<List<UiCraftBase.IngEntryRow>> AAEntryFinished;

		private List<NelItemEntry> ALostFood;

		private List<NelItemEntry> ALostItemNotFood;

		private List<NelItem> ALostFoodForLunch;

		private int current_session_lostfood;

		private List<UiCraftBase.CookMem> ACreateMem;

		private UiFieldGuide RecipeBook;

		private aBtn Rbk_PreSelected;

		protected TextRenderer TxSuccess;

		private string complete_item_key;

		private UiLunchTime LunchTime;

		private const int T_COMPLETE_EF_MAXT = 160;

		private int tutorial_target_row = -1;

		protected RecipeManager.Recipe TutorialTarget;

		protected UiCraftBase.EVWAIT evwait;

		private bool waiting_cancelable;

		private bool pre_handle = true;

		private aBtn PreSelected;

		private PxlFrame PFArrow;

		private bool need_fine_tutorial_arrow;

		private GameObject GobTutorialArrow;

		private IDesignerBlock ArrowFocus;

		private Vector3 ArrowPos;

		private List<aBtn> APoolEvacuated;

		private List<aBtn> APoolEvacuatedIng;

		private string stabilize_key;

		private Comparison<ItemStorage.IRow> FD_fnSortAutoCreationItemRow;

		protected const float cartbtn_z = -0.125f;

		protected float first_base_z = 0.4f;

		protected RecipeManager.Recipe TargetRcp;

		protected NelItem TargetRcpItm;

		protected UiCraftBase.AutoCreationInfo CurCInfo;

		private List<List<UiCraftBase.IngEntryRow>> AAIngCreate;

		private List<UiCraftBase.IngEntryRow> AIngView;

		private bool need_ingred_fine;

		private bool need_kd_fine;

		private bool need_rie_scrollbox_fine;

		private int rie_r_anim_i;

		private bool need_use_finerow;

		private int rie_anim_delay;

		private int rie_r_fine_link = -1;

		protected bool recipe_creatable;

		protected RecipeManager.RecipeDish CompletionImage;

		private List<Stomach.CostData> Astomach_cost;

		public float stomach_cost_total;

		public int stomach_cost_max;

		private MeshDrawer MdLunchCostMask;

		private MeshDrawer MdLunchCost;

		private bool need_redraw_stomach;

		private PxlFrame PFCompletion;

		private List<Vector3> AInsertEffect;

		private List<ItemStorage.IRow> ATempRow;

		protected float out_center_y;

		private const float confirm_w = 220f;

		private BtnContainerRadio<aBtn> RieCon;

		protected MeshDrawer MdEf;

		private MeshDrawer MdTutoArrow;

		private int compete_eff_t = -1;

		private aBtnItemRow FocusIng;

		private aBtn FocusPre;

		private static Material SMtrLunchCostMask;

		private static Material SMtrLunchCost;

		private ScrollBox ScBoxCmdR;

		private ByteArray[] ABaStorage;

		private ItemStorage CompletionAddedStorage;

		protected enum STATE
		{
			_NOUSE = -1,
			RECIPE_TOPIC,
			RECIPE_CHOOSE_ROW,
			ING_CONTENT,
			AUTOFILLING,
			AUTOFILLING_IC,
			RECIPE_CONFIRM,
			COMPLETE,
			COMPLETE_ENOUGH
		}

		public class IngEntryRow
		{
			public IngEntryRow(UiCraftBase.IngEntryRow _Parent)
			{
				this.Parent = _Parent;
				this.generation = ((this.Parent != null) ? (this.Parent.generation + 1) : 0);
			}

			public IngEntryRow(UiCraftBase.IngEntryRow _Parent, RecipeManager.RecipeIngredient _Source, NelItem _Itm, int _grade)
				: this(_Parent)
			{
				this.Itm = _Itm;
				this.grade = _grade;
				this.Source = _Source;
			}

			public IngEntryRow(UiCraftBase.IngEntryRow _Parent, RecipeManager.RecipeIngredient _Source)
				: this(_Parent)
			{
				RecipeManager.Recipe targetRecipe = _Source.TargetRecipe;
				this.Source = _Source;
				this.AACook = UiCraftBase.IngEntryRow.allocAA(null, targetRecipe);
			}

			public static List<List<UiCraftBase.IngEntryRow>> allocAA(List<List<UiCraftBase.IngEntryRow>> AA, RecipeManager.Recipe Rcp)
			{
				int count = Rcp.AIng.Count;
				if (AA == null)
				{
					AA = new List<List<UiCraftBase.IngEntryRow>>(count);
				}
				while (AA.Count < count)
				{
					AA.Add(new List<UiCraftBase.IngEntryRow>((Rcp != null) ? Rcp.AIng[AA.Count].need : 0));
				}
				if (AA.Count > count)
				{
					AA.RemoveRange(count, AA.Count - count);
				}
				return AA;
			}

			public void EmptyPot()
			{
				if (this.AACook != null)
				{
					UiCraftBase.IngEntryRow.allocAA(null, this.Source.TargetRecipe);
					for (int i = this.AACook.Count - 1; i >= 0; i--)
					{
						if (this.AACook[i] != null)
						{
							this.AACook[i].Clear();
						}
					}
				}
			}

			public List<List<UiCraftBase.IngEntryRow>> getInnerIngredient()
			{
				return this.AACook;
			}

			public UiCraftBase.IngEntryRow setInnerIngredientFromBinary(List<List<UiCraftBase.IngEntryRow>> AA)
			{
				this.AACook = AA;
				return this;
			}

			public int index_ingredient
			{
				get
				{
					return this.Source.index;
				}
			}

			public void destruct(UiCraftBase Con, ItemStorage St)
			{
				if (this.Itm != null && Con.reduceOnCreating(this.Itm, this.grade))
				{
					St.Add(this.Itm, 1, this.grade, true, true);
				}
			}

			public aBtnItemRow getBindedButton()
			{
				if (this.Skin == null)
				{
					return null;
				}
				return this.Skin.getBtn() as aBtnItemRow;
			}

			public bool visible
			{
				get
				{
					return this.Skin != null;
				}
				set
				{
					if (!value && this.Skin != null)
					{
						this.Skin.blur(this);
						this.Skin = null;
					}
				}
			}

			public bool isFilled()
			{
				if (this.AACook == null)
				{
					return true;
				}
				for (int i = this.AACook.Count - 1; i >= 0; i--)
				{
					List<UiCraftBase.IngEntryRow> list = this.AACook[i];
					if (list == null)
					{
						return false;
					}
					for (int j = list.Count - 1; j >= 0; j--)
					{
						if (!list[j].isFilled())
						{
							return false;
						}
					}
				}
				return true;
			}

			public void addIngredientCountToDish(RecipeManager.RecipeDish Dh, NelItem Itm, int count, int grade, bool add_to_use_ingredient = true)
			{
				if (count <= 0)
				{
					return;
				}
				bool flag = true;
				if (Itm.RecipeInfo != null && Itm.RecipeInfo.DishInfo != null)
				{
					foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in Itm.RecipeInfo.DishInfo.OUseIngredientSource)
					{
						for (int i = 0; i < 5; i++)
						{
							this.addIngredientCountToDish(Dh, keyValuePair.Key, keyValuePair.Value.getCount(i), i, false);
						}
					}
					flag = false;
				}
				for (int j = (flag ? 0 : 1); j < (add_to_use_ingredient ? 2 : 1); j++)
				{
					BDic<NelItem, ItemStorage.ObtainInfo> bdic = ((j == 0) ? Dh.OUseIngredientSource : Dh.OUseIngredient);
					ItemStorage.ObtainInfo obtainInfo = global::XX.X.Get<NelItem, ItemStorage.ObtainInfo>(bdic, Itm);
					if (obtainInfo == null)
					{
						obtainInfo = (bdic[Itm] = new ItemStorage.ObtainInfo());
					}
					obtainInfo.AddCount(count, grade);
				}
			}

			public void addEffectToDish(RecipeManager.RecipeDish Dh, float lvl, ref int total_item_for_quality, ref int total_grade, bool abort_if_missing = true)
			{
				if (this.AACook == null)
				{
					if (lvl > 0f)
					{
						if (this.Itm.RecipeInfo != null)
						{
							Dh.addCostInCreating(this.Itm.RecipeInfo.cost);
							if (!this.Itm.is_tool)
							{
								total_grade += this.grade;
								total_item_for_quality++;
							}
							if (this.Itm.RecipeInfo.DishInfo != null)
							{
								Dh.addEffect(this.Itm.RecipeInfo.DishInfo, lvl);
							}
							else
							{
								lvl *= this.Itm.getGradeMultiply(this.grade) / this.Itm.max_grade_enpower;
								Dh.addEffect100(this.Itm.RecipeInfo.Oeffect100, lvl);
							}
						}
						else if (!this.Itm.is_tool && Dh.Rcp.categ != RecipeManager.RP_CATEG.COOK)
						{
							total_grade += this.grade;
							total_item_for_quality++;
						}
					}
					this.addIngredientCountToDish(Dh, this.Itm, 1, this.grade, true);
					return;
				}
				int count = this.AACook.Count;
				RecipeManager.RecipeDish recipeDish = this.Source.TargetRecipe.createDish(this.AACook, abort_if_missing, null);
				for (int i = 0; i < count; i++)
				{
					List<UiCraftBase.IngEntryRow> list = this.AACook[i];
					if (list != null)
					{
						recipeDish.Rcp.AIng[i].createDish(Dh, list, 0f, ref total_item_for_quality, ref total_grade, abort_if_missing, default(RecipeManager.MakeDishIngDescription));
					}
				}
				Dh.addCostInCreating(recipeDish.cost);
				if (lvl > 0f)
				{
					Dh.addEffect(recipeDish, lvl);
				}
				total_grade += recipeDish.calced_grade;
				total_item_for_quality++;
			}

			public int insertViewRow(List<UiCraftBase.IngEntryRow> AInsert, int _index, int visible_thresh)
			{
				if (_index == -2)
				{
					int count = AInsert.Count;
					for (int i = 0; i < count; i++)
					{
						UiCraftBase.IngEntryRow ingEntryRow = AInsert[i];
						if (this.Parent == null)
						{
							if (ingEntryRow.Parent == null && ingEntryRow.Source.index > this.Source.index)
							{
								_index = i;
								break;
							}
						}
						else if (ingEntryRow == this.Parent)
						{
							_index = i + 1;
							break;
						}
					}
					if (_index < 0)
					{
						_index = AInsert.Count;
						AInsert.Add(this);
					}
					else
					{
						AInsert.Insert(_index, this);
					}
				}
				else if (_index == -1)
				{
					_index = AInsert.Count;
					AInsert.Add(this);
				}
				else
				{
					AInsert.Insert(_index, this);
				}
				int num = _index;
				if (this.AACook != null)
				{
					int count2 = this.AACook.Count;
					for (int j = 0; j < count2; j++)
					{
						List<UiCraftBase.IngEntryRow> list = this.AACook[j];
						if (list != null)
						{
							int count3 = list.Count;
							for (int k = 0; k < count3; k++)
							{
								list[k].insertViewRow(AInsert, ++_index, visible_thresh);
							}
						}
					}
				}
				return num;
			}

			public bool bindToButton(ButtonSkinAlchemyEntryRow _Skin, RecipeManager.Recipe _Rcp, ItemStorage _Storage)
			{
				if (this.Skin == _Skin)
				{
					return false;
				}
				this.Skin = _Skin;
				if (this.Skin != null)
				{
					this.Skin.setItem(this, _Rcp, _Storage);
				}
				return true;
			}

			public bool progressAnim(ref int counter)
			{
				counter++;
				if (this.AACook != null)
				{
					int count = this.AACook.Count;
					for (int i = 0; i < count; i++)
					{
						List<UiCraftBase.IngEntryRow> list = this.AACook[i];
						int num = ((list != null) ? list.Count : 0);
						for (int j = 0; j < num; j++)
						{
							list[j].progressAnim(ref counter);
						}
					}
				}
				return this.visible;
			}

			public void prepareCountCreatable(BDic<NelItem, ItemStorage.ObtainInfo> OUse)
			{
				if (this.AACook != null)
				{
					int count = this.AACook.Count;
					for (int i = 0; i < count; i++)
					{
						List<UiCraftBase.IngEntryRow> list = this.AACook[i];
						if (list != null)
						{
							int count2 = list.Count;
							for (int j = 0; j < count2; j++)
							{
								list[j].prepareCountCreatable(OUse);
							}
						}
					}
					return;
				}
				ItemStorage.ObtainInfo obtainInfo = global::XX.X.Get<NelItem, ItemStorage.ObtainInfo>(OUse, this.Itm);
				if (obtainInfo == null)
				{
					obtainInfo = (OUse[this.Itm] = new ItemStorage.ObtainInfo());
				}
				obtainInfo.AddCount(1, this.grade);
			}

			public static int countCreatable(UiCraftBase Con, ItemStorage Storage, BDic<NelItem, ItemStorage.ObtainInfo> OUse)
			{
				int num = -1;
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in OUse)
				{
					if (Con.reduceOnCreating(keyValuePair.Key, -1))
					{
						ItemStorage.ObtainInfo value = keyValuePair.Value;
						for (int i = 0; i < 5; i++)
						{
							if (Con.reduceOnCreating(keyValuePair.Key, i))
							{
								int count = value.getCount(i);
								if (count > 0)
								{
									int num2 = Storage.getCount(keyValuePair.Key, i) / count;
									num = ((num < 0) ? num2 : global::XX.X.Mn(num, num2));
									if (num == 0)
									{
										break;
									}
								}
							}
						}
						if (num == 0)
						{
							break;
						}
					}
				}
				if (num >= 0)
				{
					return num;
				}
				return 1;
			}

			public RecipeManager.RecipeIngredient getOffspringSource()
			{
				if (this.Parent == null)
				{
					return this.Source;
				}
				return this.Parent.getOffspringSource();
			}

			public UiCraftBase.IngEntryRow getOffspringEntry()
			{
				if (this.Parent == null)
				{
					return this;
				}
				return this.Parent.getOffspringEntry();
			}

			public readonly UiCraftBase.IngEntryRow Parent;

			public readonly int generation;

			public int index;

			public RecipeManager.RecipeIngredient Source;

			public readonly NelItem Itm;

			public readonly int grade;

			private List<List<UiCraftBase.IngEntryRow>> AACook;

			private ButtonSkinAlchemyEntryRow Skin;
		}

		public class AutoCreationInfo
		{
			public AutoCreationInfo()
			{
				this.grade = 2;
			}

			public AutoCreationInfo(byte _grade, UiCraftBase.AF_COST _cost, UiCraftBase.AF_KIND _kind, UiCraftBase.AF_QUANTITY _quantity)
			{
				this.grade = _grade;
				this.cost = _cost;
				this.kind = _kind;
				this.quantity = _quantity;
			}

			public AutoCreationInfo(UiCraftBase.AutoCreationInfo Src)
			{
				this.grade = Src.grade;
				this.cost = Src.cost;
				this.kind = Src.kind;
				this.quantity = Src.quantity;
				this.previous = Src.previous;
				this.obtain_flag = Src.obtain_flag;
			}

			public byte grade;

			public UiCraftBase.AF_COST cost;

			public UiCraftBase.AF_KIND kind;

			public UiCraftBase.AF_QUANTITY quantity;

			public bool previous;

			public static UiCraftBase.AutoCreationInfo CurrentSort;

			public static RecipeManager.RecipeIngredient CurrentIng;

			public bool cannot_use_already_food;

			public bool obtain_flag;
		}

		public enum AF_COST
		{
			NONE,
			HIGH_COST,
			LOW_COST,
			_MAX
		}

		public enum AF_KIND
		{
			NONE,
			ENOUGH,
			FEW,
			_MAX
		}

		public enum AF_QUANTITY
		{
			MINIMUM,
			MAXIMUM,
			_MAX
		}

		protected enum EVWAIT
		{
			NOUSE,
			PAUSE,
			RECIPE_CHOOSE_ROW,
			ING_CONTENT,
			RECIPE_CHOOSE_ROW_FILLED,
			RECIPE_CHOOSE_ROW_FILLED_ALLOW_AUTO,
			COMPLETED,
			RECIPE_CHOOSE_ROW_AUTO,
			ING_AUTO_FINISHED,
			COMPLETED_ALLOW_AUTO,
			ING_CONTENT_OR_AUTO,
			TRM_TT,
			WHOLE_ALLOW_AUTO
		}

		private class CookMem
		{
			public CookMem(List<List<UiCraftBase.IngEntryRow>> _AAIng, RecipeManager.Recipe _Rcp, NelItem _RcpItm)
			{
				this.AAIng = _AAIng;
				this.Rcp = _Rcp;
				this.RcpItm = _RcpItm;
			}

			public List<List<UiCraftBase.IngEntryRow>> AAIng;

			public RecipeManager.Recipe Rcp;

			public NelItem RcpItm;
		}

		private class WaitListenerUiAlchemyTutorial : IEventWaitListener
		{
			public WaitListenerUiAlchemyTutorial(UiCraftBase _Ua)
			{
				this.Ua = _Ua;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				if (this.Ua.stt == UiCraftBase.STATE._NOUSE)
				{
					return false;
				}
				switch (this.Ua.evwait)
				{
				case UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW:
					return this.Ua.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW;
				case UiCraftBase.EVWAIT.ING_CONTENT:
					return this.Ua.stt != UiCraftBase.STATE.ING_CONTENT;
				case UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_FILLED:
				case UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_FILLED_ALLOW_AUTO:
					return this.Ua.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW || !this.Ua.recipe_creatable;
				case UiCraftBase.EVWAIT.COMPLETED:
				case UiCraftBase.EVWAIT.COMPLETED_ALLOW_AUTO:
					return !this.Ua.isCompletionState(this.Ua.stt);
				case UiCraftBase.EVWAIT.RECIPE_CHOOSE_ROW_AUTO:
					return this.Ua.stt != UiCraftBase.STATE.AUTOFILLING;
				case UiCraftBase.EVWAIT.ING_AUTO_FINISHED:
					if (this.Ua.stt != UiCraftBase.STATE.RECIPE_CHOOSE_ROW)
					{
						return true;
					}
					if (this.Ua.tutorial_target_row >= 0)
					{
						RecipeManager.RecipeIngredient recipeIngredient = this.Ua.TargetRcp.AIng[this.Ua.tutorial_target_row];
						return this.Ua.AAIngCreate[this.Ua.tutorial_target_row].Count < recipeIngredient.need;
					}
					return !this.Ua.recipe_creatable;
				case UiCraftBase.EVWAIT.ING_CONTENT_OR_AUTO:
					if (this.Ua.tutorial_target_row >= 0)
					{
						RecipeManager.RecipeIngredient recipeIngredient2 = this.Ua.TargetRcp.AIng[this.Ua.tutorial_target_row];
						return this.Ua.AAIngCreate[this.Ua.tutorial_target_row].Count < recipeIngredient2.need;
					}
					return !this.Ua.recipe_creatable;
				case UiCraftBase.EVWAIT.WHOLE_ALLOW_AUTO:
					return true;
				}
				return false;
			}

			private readonly UiCraftBase Ua;
		}
	}
}
