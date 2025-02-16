using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.smnp;
using PixelLiner;
using XX;

namespace nel
{
	public class UiFieldGuide : UiBoxDesignerFamily, IEventWaitListener, NelItem.IItemUser
	{
		public static NelItem Sanitize(NelItem Itm)
		{
			if (Itm != null && Itm.is_reelmbox && TX.isStart(Itm.key, "itemreelC_", 0))
			{
				Itm = NelItem.GetById("itemreelG_" + TX.slice(Itm.key, "itemreelC_".Length), false) ?? Itm;
			}
			return Itm;
		}

		private float bxl_btn_w
		{
			get
			{
				return this.bxl_w - 10f - 50f;
			}
		}

		private float bxl_x
		{
			get
			{
				return -this.out_w * 0.5f + this.bxl_w * 0.5f - 50f;
			}
		}

		private float right_w
		{
			get
			{
				return this.out_w - this.bxl_w - this.bxl_x_margin;
			}
		}

		private float right_x
		{
			get
			{
				return this.out_w * 0.5f - this.right_w * 0.5f - 50f;
			}
		}

		private float top_y
		{
			get
			{
				return this.out_h * 0.5f - this.top_h * 0.5f;
			}
		}

		private float right_h
		{
			get
			{
				return this.out_h - this.top_h - this.top_y_margin;
			}
		}

		private float right_y
		{
			get
			{
				return -this.out_h * 0.5f + this.right_h * 0.5f;
			}
		}

		private float detail_shift_x
		{
			get
			{
				return this.detail_marg_x + this.detail_w * 0.5f + this.out_w * 0.5f;
			}
		}

		private bool pickup_use
		{
			get
			{
				return this.CurPickUp.valid;
			}
			set
			{
				if (!value)
				{
					this.CurPickUp = default(UiFieldGuide.PickUp);
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.OAItm2Row = new BDic<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>();
			this.OAAItmRecipeDefinition = new BDic<NelItem, List<RCP.RecipeDescription>[]>();
			this.OAStoreItemListedUp = new BDic<StoreManager, List<NelItem>>(StoreManager.GetWholeStoreObject().Count);
			this.OABufAppearEn = new BDic<WholeMapItem, SummonerList>(1);
			this.ADetailHistory = new List<UiFieldGuide.FDR>(1);
			this.OAItm2EnemyID = new BDic<NelItem, List<ENEMYID>>(4);
			this.ARbSk = new List<ButtonSkinNelFieldGuide>(16);
			IN.setZ(base.transform, -4.275f);
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.enabled_enemy_info = (X.DEBUGALBUMUNLOCK || GF.getB("FG_ENEMY")) && this.M2D != null;
			UiAlchemyWorkBench.initReadScript();
			UiAlchemyWorkBench.AddTopicRow(null);
			this.ACtgHeader = new List<aBtn>();
			this.auto_deactive_gameobject = false;
			this.base_z = this.first_base_z;
			this.activate();
			this.initDesigner();
			if (M2DBase.Instance != null)
			{
				Flagger flagValotStabilize = this.M2D.FlagValotStabilize;
				if (flagValotStabilize != null)
				{
					flagValotStabilize.Add("Field");
				}
				Flagger flgValotileDisable = UIBase.FlgValotileDisable;
				if (flgValotileDisable != null)
				{
					flgValotileDisable.Add("Field");
				}
			}
			this.changeState(UiFieldGuide.STATE.FIRST);
		}

		private void initialize()
		{
			this.initItemContent();
			this.initDesignerDetail();
			this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
			bool flag = false;
			bool flag2 = false;
			WmDeperture wmDeperture = default(WmDeperture);
			if (UiFieldGuide.NextRevealAtAwake is ReelManager.ItemReelContainer)
			{
				ReelManager.ItemReelContainer itemReelContainer = UiFieldGuide.NextRevealAtAwake as ReelManager.ItemReelContainer;
				if (itemReelContainer.useableItem)
				{
					UiFieldGuide.NextRevealAtAwake = itemReelContainer.GReelItem;
				}
				else
				{
					NelItem individualItem = itemReelContainer.IndividualItem;
					if (individualItem != null)
					{
						UiFieldGuide.NextRevealAtAwake = individualItem;
					}
				}
			}
			if (UiFieldGuide.NextRevealAtAwake is M2EventItem_ItemSupply)
			{
				M2EventItem_ItemSupply m2EventItem_ItemSupply = UiFieldGuide.NextRevealAtAwake as M2EventItem_ItemSupply;
				ReelManager.ItemReelContainer ireel = m2EventItem_ItemSupply.IReel;
				NelItem individualItem2 = ireel.IndividualItem;
				if (individualItem2 != null)
				{
					UiFieldGuide.NextRevealAtAwake = individualItem2;
				}
				else
				{
					flag2 = true;
					this.CurPickUp = new UiFieldGuide.PickUp(ireel, ireel.getLocalizedName(m2EventItem_ItemSupply.LpCon));
				}
			}
			if (UiFieldGuide.NextRevealAtAwake is RCP.RecipeIngredient)
			{
				RCP.RecipeIngredient recipeIngredient = UiFieldGuide.NextRevealAtAwake as RCP.RecipeIngredient;
				if (recipeIngredient.Target != null)
				{
					UiFieldGuide.NextRevealAtAwake = recipeIngredient.Target;
				}
				if (recipeIngredient.TargetRecipe != null)
				{
					UiFieldGuide.NextRevealAtAwake = recipeIngredient.TargetRecipe;
				}
				if (recipeIngredient.target_category != (RCP.RPI_CATEG)0)
				{
					flag2 = true;
					this.CurPickUp = new UiFieldGuide.PickUp(recipeIngredient.target_category);
				}
				if (recipeIngredient.target_ni_category != NelItem.CATEG.OTHER)
				{
					this.CurPickUp = new UiFieldGuide.PickUp(recipeIngredient.target_ni_category);
				}
			}
			if (UiFieldGuide.NextRevealAtAwake is RCP.Recipe)
			{
				UiFieldGuide.NextRevealAtAwake = (UiFieldGuide.NextRevealAtAwake as RCP.Recipe).RecipeItem;
			}
			if (UiFieldGuide.NextRevealAtAwake is NelItem)
			{
				NelItem nelItem = UiFieldGuide.Sanitize(UiFieldGuide.NextRevealAtAwake as NelItem);
				flag = this.topicJump(nelItem);
			}
			UiFieldGuide.FDR fdr = default(UiFieldGuide.FDR);
			if (UiFieldGuide.NextRevealAtAwake is SummonerPlayer)
			{
				UiFieldGuide.NextRevealAtAwake = (UiFieldGuide.NextRevealAtAwake as SummonerPlayer).Summoner;
			}
			if (UiFieldGuide.NextRevealAtAwake is EnemySummoner && this.M2D != null)
			{
				fdr = this.getFDRFor(UiFieldGuide.NextRevealAtAwake as EnemySummoner);
			}
			if (UiFieldGuide.NextRevealAtAwake is UiFieldGuide.IFieldGuideOpenable)
			{
				fdr = (UiFieldGuide.NextRevealAtAwake as UiFieldGuide.IFieldGuideOpenable).getFDR();
			}
			if (UiFieldGuide.NextRevealAtAwake is UiFieldGuide.FDR)
			{
				fdr = (UiFieldGuide.FDR)UiFieldGuide.NextRevealAtAwake;
			}
			if (UiFieldGuide.NextRevealAtAwake is ENEMYID)
			{
				fdr = this.getFDRFor((ENEMYID)UiFieldGuide.NextRevealAtAwake);
			}
			if (fdr.valid)
			{
				if (fdr.enemyid > (ENEMYID)0U)
				{
					fdr = this.getFDRFor(fdr.enemyid);
				}
				if (fdr.valid)
				{
					flag = this.topicJump(fdr);
					if (fdr.FSmn != null)
					{
						WholeMapItem wholeFor = this.M2D.WM.GetWholeFor(fdr.FSmn.SInfoMp, false);
						if (wholeFor != null)
						{
							wmDeperture = new WmDeperture(wholeFor.text_key, fdr.FSmn.SInfoMp.key);
						}
					}
				}
			}
			if (flag)
			{
				this.changeState(UiFieldGuide.STATE.DETAIL);
				if (wmDeperture.map_key != null)
				{
					this.WmSkin.reveal(wmDeperture, true, false);
				}
			}
			else if (this.BConR.Length >= 2)
			{
				this.BConR.Get(1).Select(true);
			}
			if (UiFieldGuide.NextRevealAtAwake is M2EventItem_ItemSupply)
			{
				UiFieldGuide.NextRevealAtAwake = (UiFieldGuide.NextRevealAtAwake as M2EventItem_ItemSupply).IReel;
			}
			if (UiFieldGuide.NextRevealAtAwake is ReelManager.ItemReelContainer)
			{
				this.CurPickUp = new UiFieldGuide.PickUp(UiFieldGuide.NextRevealAtAwake as ReelManager.ItemReelContainer, "&&Catalog_pickup_current_reel_spot[" + ((this.M2D != null) ? this.M2D.getMapTitle(null) : "") + "]");
				flag2 = true;
			}
			if (UiFieldGuide.NextRevealAtAwake is SummonerList)
			{
				SummonerList summonerList = UiFieldGuide.NextRevealAtAwake as SummonerList;
				this.CurPickUp = new UiFieldGuide.PickUp(summonerList, "&&Guild_pickup_fof_fieldguide");
				flag2 = true;
			}
			if (flag2)
			{
				this.finePickUp();
			}
		}

		public override void destruct()
		{
			base.destruct();
			if (this.WmCtr != null)
			{
				this.WmCtr.destruct();
			}
			if (this.AEvcMem != null)
			{
				for (int i = this.AEvcMem.Count - 1; i >= 0; i--)
				{
					IDesignerBlock blk = this.AEvcMem[i].Blk;
					try
					{
						if (blk != null)
						{
							IN.DestroyOne(blk.getTransform().gameObject);
						}
					}
					catch
					{
					}
				}
			}
			Flagger flagValotStabilize = this.M2D.FlagValotStabilize;
			if (flagValotStabilize != null)
			{
				flagValotStabilize.Rem("Field");
			}
			Flagger flgValotileDisable = UIBase.FlgValotileDisable;
			if (flgValotileDisable != null)
			{
				flgValotileDisable.Rem("Field");
			}
			this.AEvcMem = null;
		}

		private void initDesigner()
		{
			this.BxL = base.Create("left", 0f, 0f, this.bxl_w, this.out_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxL.getBox().frametype = UiBox.FRAMETYPE.NONE;
			this.BxL.Smallest();
			this.BxL.use_scroll = false;
			this.BxL.margin_in_tb = 20f;
			this.BxL.item_margin_y_px = 8f;
			this.BxL.box_stencil_ref_mask = -1;
			this.BxL.stencil_ref = -1;
			int num = X.beki_cnt(131072U);
			this.OCategHead = new BDic<aBtn, UiFieldGuide.CategHead>(num);
			this.BxL.init();
			this.BxL.getBox().col(MTRX.ColTrnsp);
			this.BxL.P("<key sort/>", ALIGN.CENTER, this.BxL.use_w - 8f, true, 20f, "");
			this.BxL.Br();
			this.BxL.Focusable(false, false, null);
			this.BConL = this.BxL.addRadioT<aBtnNel>(new DsnDataRadio
			{
				name = "bxl_category",
				clms = 1,
				skin = "kadomaru_icon",
				w = this.bxl_btn_w,
				h = this.bxl_btn_w,
				SCA = new ScrollAppend(250, this.BxL.use_w, this.BxL.use_h - 8f, 2f, 6f, 0),
				margin_w = 0,
				margin_h = 10,
				navi_loop = 2,
				fnMaking = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingTopicLeft)
			});
			this.BConL.APool = new List<aBtn>(num);
			this.BxT = base.Create("top", 0f, 0f, this.right_w, this.top_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxT.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxT.Focusable(false, false, null);
			this.BxT.Small();
			this.BxT.margin_in_lr = 20f;
			this.BxT.margin_in_tb = 8f;
			this.BxT.init();
			float use_h = this.BxT.use_h;
			this.TopTab = ColumnRow.CreateT<aBtnNel>(this.BxT, "toptab", "row_tab", 0, null, new BtnContainerRadio<aBtn>.FnRadioBindings(this.FnChangedTopTab), this.BxT.use_w - 8f, use_h, false, true);
			this.TopTab.LR_valotile = true;
			this.TopTab.APool = new List<aBtn>(X.beki_cntC(524288U));
			this.BxT.Br();
			this.DTopPickUp = this.BxT.addTab("dpickup", this.BxT.use_w, use_h, this.BxT.use_w, use_h, false);
			this.DTopPickUp.Smallest();
			this.DTopPickUp.radius = use_h * 0.5f;
			this.DTopPickUp.margin_in_lr = 30f;
			this.DTopPickUp.bgcol = C32.d2c(4283780170U);
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				swidth = this.BxT.use_w - 125f,
				sheight = use_h,
				alignx = ALIGN.LEFT,
				name = "pickup_fb_L",
				text = " ",
				html = true,
				size = 15f,
				TxCol = C32.d2c(4293321691U),
				text_auto_wrap = true
			};
			this.FbPickUp = this.DTopPickUp.addP(dsnDataP, false);
			dsnDataP.alignx = ALIGN.RIGHT;
			dsnDataP.name = "pickup_fb_R";
			dsnDataP.swidth = this.BxT.use_w;
			this.DTopPickUp.addP(dsnDataP, false);
			this.BxT.endTab(true);
			this.BxT.getDesignerBlockMemory(this.DTopPickUp).active = false;
			this.BxR = base.Create("right", 0f, 0f, this.right_w, this.right_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxR.Focusable(false, false, null);
			this.BxR.margin_in_lr = 28f;
			this.BxR.item_margin_x_px = 0f;
			this.BxR.item_margin_y_px = 2f;
			this.BxR.animate_maxt = 0;
			this.BxR.alignx = ALIGN.LEFT;
			this.BxR.use_scroll = false;
			this.BxR.init();
			float num2 = this.BxR.use_w - 4f - 10f - 8f;
			DsnDataP dsnDataP2 = new DsnDataP("", false)
			{
				text = TX.Get("Enhancer_desc_fav", "") + " " + TX.Get("KD_catalog_move_between_favolite", ""),
				size = 12f,
				TxCol = C32.d2c(4283780170U),
				html = true,
				swidth = this.BxR.use_w * 0.7f,
				alignx = ALIGN.LEFT,
				use_valotile = 1
			};
			this.BxR.addP(dsnDataP2, false);
			this.BxR.addP(new DsnDataP("", false)
			{
				text = " ",
				size = 12f,
				TxCol = C32.d2c(4283780170U),
				name = "completion",
				html = true,
				swidth = this.BxR.use_w - 2f,
				alignx = ALIGN.RIGHT
			}, false);
			this.BxR.Br();
			this.BConR = this.BxR.addRadioT<aBtnNel>(new DsnDataRadio
			{
				name = "bxr_item",
				def = -1,
				clms = 1,
				skin = "kadomaru_icon",
				w = num2,
				h = this.bxl_btn_w,
				SCA = new ScrollAppend(251, this.BxR.use_w, this.BxR.use_h - 4f, 2f, 6f, 0),
				margin_w = 0,
				margin_h = 8,
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.FnChangedTopicRight)
			});
			this.BConR.APool = new List<aBtn>();
			dsnDataP2.text_margin_x = 22f;
			this.BxR.Br().addP(dsnDataP2.Text("<key z/> " + TX.Get("catalog_jump", "")), false);
			this.BConR.default_w = num2;
		}

		private aBtn initItemContent()
		{
			int num = X.beki_cnt(131072U);
			BDic<UiItemWholeSelector.WCATEG, UiFieldGuide.CategHead> bdic = new BDic<UiItemWholeSelector.WCATEG, UiFieldGuide.CategHead>(num);
			this.total_items = (this.valid_items = 0);
			aBtn aBtn = null;
			List<UiFieldGuide.FDR> list = new List<UiFieldGuide.FDR>();
			List<UiFieldGuide.FDR> list2 = new List<UiFieldGuide.FDR>();
			for (int i = -1; i < num; i++)
			{
				UiItemWholeSelector.WCATEG wcateg;
				if (i == -1)
				{
					wcateg = UiItemWholeSelector.WCATEG.RECIPE;
				}
				else
				{
					wcateg = (UiItemWholeSelector.WCATEG)(1 << i);
				}
				UiItemWholeSelector.WCATEG wcateg2 = wcateg;
				bool flag = true;
				if (wcateg2 != UiItemWholeSelector.WCATEG.SKILL && wcateg2 != UiItemWholeSelector.WCATEG.ENHANCER && wcateg2 != UiItemWholeSelector.WCATEG.SPCONFIG)
				{
					list2.Clear();
					int num2;
					if (wcateg2 == UiItemWholeSelector.WCATEG.RECIPE && i >= 0)
					{
						list2.AddRange(list);
					}
					else
					{
						if (wcateg2 == UiItemWholeSelector.WCATEG._ALL)
						{
							if (this.M2D == null)
							{
								goto IL_05CC;
							}
							this.OSummonerInfo = SupplyManager.getFDSummonerInfoData(this.M2D);
							using (Dictionary<string, FDSummonerInfo>.Enumerator enumerator = this.OSummonerInfo.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									KeyValuePair<string, FDSummonerInfo> keyValuePair = enumerator.Current;
									UiFieldGuide.FDR fdr = new UiFieldGuide.FDR(keyValuePair.Value);
									list2.Add(fdr);
								}
								goto IL_03D6;
							}
						}
						if (wcateg2 == UiItemWholeSelector.WCATEG.ENEMY)
						{
							if (!this.enabled_enemy_info || this.M2D == null)
							{
								goto IL_05CC;
							}
							List<UiEnemyDex.DefeatData> listupDefeated = UiEnemyDex.getListupDefeated(null, true, true);
							int count = listupDefeated.Count;
							for (int j = 0; j < count; j++)
							{
								UiFieldGuide.FDR fdr2 = new UiFieldGuide.FDR(listupDefeated[j].id, listupDefeated[j].get_count(-1));
								list2.Add(fdr2);
							}
						}
						else
						{
							using (BList<NelItem> blist = ListBuffer<NelItem>.Pop(0))
							{
								UiItemWholeSelector.PopForSpecificCategory(wcateg2, blist);
								num2 = blist.Count;
								if (wcateg2 == UiItemWholeSelector.WCATEG.RECIPE)
								{
									blist.Sort(delegate(NelItem A, NelItem B)
									{
										int num3 = (A.is_trm_episode ? 2 : (A.is_workbench_craft ? 1 : 0));
										int num4 = (B.is_trm_episode ? 2 : (B.is_workbench_craft ? 1 : 0));
										if (num3 == num4)
										{
											return (int)(A.id - B.id);
										}
										return num3 - num4;
									});
								}
								bool flag2 = false;
								if (list2.Capacity < num2)
								{
									list2.Capacity = num2;
								}
								int k = 0;
								while (k < num2)
								{
									NelItem nelItem = blist[k];
									bool flag3 = this.ignore_obtain_count || nelItem.obtain_count > 0;
									if (wcateg2 != UiItemWholeSelector.WCATEG.REEL)
									{
										goto IL_02C8;
									}
									NelItem nelItem2 = null;
									if (!TX.isStart(nelItem.key, "itemreelC_", 0))
									{
										if (TX.isStart(nelItem.key, "itemreelG_", 0))
										{
											nelItem2 = NelItem.GetById("itemreelC_" + TX.slice(nelItem.key, "itemreelG_".Length), false);
										}
										if (nelItem2 != null)
										{
											bool fd_favorite = nelItem.fd_favorite;
											nelItem.addObtainCount(nelItem2.visible_obtain_count);
											nelItem2.visible_obtain_count = 0;
											nelItem2.fd_favorite = (nelItem.fd_favorite = fd_favorite);
										}
										flag3 = this.ignore_obtain_count || nelItem.obtain_count > 0;
										goto IL_02C8;
									}
									IL_0388:
									k++;
									continue;
									IL_02C8:
									if (wcateg2 == UiItemWholeSelector.WCATEG.RECIPE)
									{
										RCP.Recipe recipeAllType = UiCraftBase.getRecipeAllType(nelItem);
										if (recipeAllType == null || recipeAllType.debug_recipe)
										{
											num2--;
											blist.RemoveAt(k);
											goto IL_0388;
										}
										flag3 = this.isRecipeEnabled(nelItem, recipeAllType, flag3);
										if (flag3)
										{
											recipeAllType.touchObtainCountAllIngredients();
										}
									}
									UiFieldGuide.FDR fdr3 = new UiFieldGuide.FDR(nelItem, flag3 ? this.getSpecificGrade(nelItem, false) : (-1));
									UiFieldGuide.FdrEntry fdrEntry;
									if (!this.OAItm2Row.TryGetValue(fdr3, out fdrEntry))
									{
										fdrEntry = (this.OAItm2Row[fdr3] = new UiFieldGuide.FdrEntry(1));
										if (flag)
										{
											this.total_items++;
											if (flag3)
											{
												this.valid_items++;
											}
										}
									}
									list2.Add(fdr3);
									if (flag3)
									{
										flag2 = true;
										goto IL_0388;
									}
									goto IL_0388;
								}
								if (num2 == 0 || (!flag2 && wcateg2 != UiItemWholeSelector.WCATEG.DUST))
								{
									goto IL_05CC;
								}
								if (wcateg2 == UiItemWholeSelector.WCATEG.RECIPE && i < 0)
								{
									list.AddRange(list2);
									goto IL_05CC;
								}
							}
						}
					}
					IL_03D6:
					num2 = list2.Count;
					if (num2 != 0)
					{
						string text = UiFieldGuide.WCATEG_ToStr(wcateg2);
						aBtnNel aBtnNel = this.BConL.MakeT<aBtnNel>("l_" + text, null);
						this.wcateg_using0 |= wcateg2;
						this.BConR.default_h = 38f;
						aBtnNel aBtnNel2 = this.BConR.MakeT<aBtnNel>("header___" + text, "row_dark");
						ButtonSkinRowNelDark buttonSkinRowNelDark = aBtnNel2.get_Skin() as ButtonSkinRowNelDark;
						buttonSkinRowNelDark.setTitle(TX.Get(UiItemWholeSelector.getTxKeyForCategory(wcateg2), ""));
						buttonSkinRowNelDark.addIcon((aBtnNel.get_Skin() as ButtonSkinKadomaruIcon).PFMesh, -1);
						buttonSkinRowNelDark.row_left_px = 12;
						buttonSkinRowNelDark.fix_text_size = 16f;
						aBtnNel2.secureNavi();
						aBtnNel2.SetChecked(false, true);
						this.BConR.default_h = 34f;
						aBtnFDRow aBtnFDRow = null;
						for (int l = 0; l < num2; l++)
						{
							UiFieldGuide.FDR fdr4 = list2[l];
							UiFieldGuide.FdrEntry fdrEntry2 = X.Get<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>(this.OAItm2Row, fdr4);
							if (fdrEntry2 == null)
							{
								fdrEntry2 = (this.OAItm2Row[fdr4] = new UiFieldGuide.FdrEntry(1));
							}
							aBtnFDRow aBtnFDRow2 = this.BConR.MakeT<aBtnFDRow>(text + "__" + fdr4.key, "fieldguide");
							fdrEntry2.Add(new UiFieldGuide.ItmAndCateg(aBtnFDRow2, wcateg2));
							if (aBtnFDRow == null && fdr4.valid)
							{
								aBtnFDRow = aBtnFDRow2;
							}
							if (fdrEntry2.Count == 1)
							{
								aBtnFDRow2.setItem(this, this.M2D, fdr4);
							}
							else
							{
								aBtnFDRow2.setItem(fdrEntry2[0].B);
							}
							aBtnFDRow2.do_not_tip_on_navi_loop = true;
							aBtnFDRow2.secureNavi();
							aBtnFDRow2.addHoverFn(new FnBtnBindings(this.fnHoverTopicRight));
						}
						Dictionary<UiItemWholeSelector.WCATEG, UiFieldGuide.CategHead> dictionary = bdic;
						UiItemWholeSelector.WCATEG wcateg3 = wcateg2;
						Dictionary<aBtn, UiFieldGuide.CategHead> ocategHead = this.OCategHead;
						aBtn aBtn2 = aBtnNel;
						UiFieldGuide.CategHead categHead = new UiFieldGuide.CategHead(wcateg2, aBtnNel2, aBtnFDRow);
						ocategHead[aBtn2] = categHead;
						dictionary[wcateg3] = categHead;
					}
				}
				IL_05CC:;
			}
			this.OCategHead0 = bdic;
			this.BConL.fnGenerateRemakeKeys = new FnGenerateRemakeKeys(this.remakeTopicLKeys);
			ScrollAppendBtnContainer<aBtn> scrollAppendBtnContainer;
			IDesignerBlock designerBlock;
			if (this.BxL.getSCBFor("bxl_category", out scrollAppendBtnContainer, out designerBlock))
			{
				scrollAppendBtnContainer.item_w = this.bxl_btn_w + 24f;
				scrollAppendBtnContainer.reposition(true);
				scrollAppendBtnContainer.getScrollBox().setSliderColor(C32.d2c(4293321691U), C32.d2c(4294966715U));
			}
			ScrollAppendBtnContainer<aBtn> scrollAppendBtnContainer2;
			IDesignerBlock designerBlock2;
			if (this.BxR.getSCBFor("bxr_item", out scrollAppendBtnContainer2, out designerBlock2))
			{
				scrollAppendBtnContainer2.item_h = -1000f;
				scrollAppendBtnContainer2.reposition(false);
			}
			this.BxR.setValueTo("completion", TX.GetA("catalog_obtain_ratio", (100 * this.valid_items / this.total_items).ToString()));
			foreach (KeyValuePair<string, StoreManager> keyValuePair2 in StoreManager.GetWholeStoreObject())
			{
				if (keyValuePair2.Value.appear_on_field_guide)
				{
					this.OAStoreItemListedUp[keyValuePair2.Value] = keyValuePair2.Value.listUpWholeItem();
				}
			}
			return aBtn;
		}

		private void remakeTopicLKeys(BtnContainerBasic BCon, List<string> Adest)
		{
			int num = X.beki_cnt(131072U);
			UiItemWholeSelector.WCATEG wcateg = (this.pickup_use ? this.pickup_wcateg_using : this.wcateg_using0);
			for (int i = 0; i < num; i++)
			{
				UiItemWholeSelector.WCATEG wcateg2 = (UiItemWholeSelector.WCATEG)(1 << i);
				if ((wcateg & wcateg2) != UiItemWholeSelector.WCATEG.ALL && (wcateg2 != UiItemWholeSelector.WCATEG.ENEMY || this.enabled_enemy_info))
				{
					UiFieldGuide.WCATEG_ToStr(wcateg2);
					Adest.Add("l_" + wcateg2.ToString());
				}
			}
		}

		public static string WCATEG_ToStr(UiItemWholeSelector.WCATEG wcateg)
		{
			return UiItemWholeSelector.WCATEG_ToStr(wcateg);
		}

		public bool fnMakingTopicLeft(BtnContainerBasic BCon, aBtn BL)
		{
			UiItemWholeSelector.WCATEG wcateg = FEnum<UiItemWholeSelector.WCATEG>.Parse(TX.slice(BL.title, 2), UiItemWholeSelector.WCATEG.ALL);
			ButtonSkinKadomaruIcon buttonSkinKadomaruIcon = BL.get_Skin() as ButtonSkinKadomaruIcon;
			PxlFrame categoryIconFor = UiItemWholeSelector.getCategoryIconFor(wcateg);
			buttonSkinKadomaruIcon.PFMesh = categoryIconFor;
			buttonSkinKadomaruIcon.col_icon = 4283780170U;
			buttonSkinKadomaruIcon.icon_scale = 2f;
			buttonSkinKadomaruIcon.alpha = 1f;
			BL.addHoverFn(new FnBtnBindings(this.fnHoverTopicLeft));
			BL.addClickFn(delegate(aBtn B)
			{
				this.topicBack2Right();
				IN.clearPushDown(false);
				return false;
			});
			if (this.OCategHead0 != null)
			{
				this.OCategHead[BL] = this.OCategHead0[wcateg];
			}
			return true;
		}

		private void changeState(UiFieldGuide.STATE new_st)
		{
			UiFieldGuide.STATE state = this.state;
			this.state = new_st;
			this.state_t = 0f;
			this.fine_current_detail_scroll = false;
			if ((state == UiFieldGuide.STATE._NOUSE && new_st == UiFieldGuide.STATE.FIRST) || new_st == UiFieldGuide.STATE._NOUSE)
			{
				this.BxL.posSetDA(this.bxl_x, 0f, 2, 600f, false);
				this.BxT.posSetDA(this.right_x, this.top_y, 3, 340f, false);
				this.BxR.posSetDA(this.right_x, this.right_y, 1, this.right_h + 300f, false);
				if (new_st == UiFieldGuide.STATE._NOUSE)
				{
					this.BxD.posSetA(this.detail_shift_x + 800f, 0f, -1000f, -1000f, false);
				}
			}
			if (state == UiFieldGuide.STATE.DETAIL)
			{
				this.BxL.position(-1000f, -1000f, this.bxl_x, 0f, false);
				this.BxT.position(-1000f, -1000f, this.right_x, this.top_y, false);
				this.BxR.position(-1000f, -1000f, this.right_x, this.right_y, false);
				this.BxD.position(-1000f, -1000f, this.detail_shift_x, 0f, false);
				if (this.WmCtr != null)
				{
					this.WmCtr.clearTarget();
					if (this.DetailSelectMain != null)
					{
						this.WmCtr.quitEdit(true);
					}
				}
			}
			else if (new_st == UiFieldGuide.STATE.DETAIL)
			{
				this.BxL.position(-1000f, -1000f, this.bxl_x - this.detail_shift_x, 0f, false);
				this.BxT.position(-1000f, -1000f, this.right_x - this.detail_shift_x, this.top_y, false);
				this.BxR.position(-1000f, -1000f, this.right_x - this.detail_shift_x, this.right_y, false);
				this.BxD.position(-1000f, -1000f, 0f, 0f, false);
			}
			this.DetailSelectMain = null;
			this.BDetailPre = null;
			switch (new_st)
			{
			case UiFieldGuide.STATE._NOUSE:
				this.deactivate(false);
				UiFieldGuide.NextRevealAtAwake = null;
				this.auto_deactive_gameobject = true;
				break;
			case UiFieldGuide.STATE.ITEM_TOPIC:
				this.TopTab.lr_input = !this.pickup_use;
				this.BxL.activate();
				this.BxT.activate();
				this.BxR.activate();
				this.BxD.activate();
				this.BxD.hide();
				this.BxR.Focus();
				this.BxR.Focusable(false, false, null);
				this.BxT.Focusable(false, false, null);
				this.BConR.setValue(-1, true);
				this.DPageL.gameObject.SetActive(false);
				this.DPageR.gameObject.SetActive(false);
				this.BxR.Focus();
				this.left_category_selected = 0f;
				if (state == UiFieldGuide.STATE.DETAIL && this.PreTopicRow.valid)
				{
					this.PreTopicRow.B.Select(true);
				}
				if (this.pickup_use)
				{
					this.finePickUpKD();
				}
				if (this.WmCtr != null)
				{
					this.WmCtr.hide();
				}
				break;
			case UiFieldGuide.STATE.DETAIL:
				this.left_category_selected = 0f;
				SND.Ui.play("paper", false);
				this.TopTab.lr_input = false;
				this.BxD.bind();
				this.BxR.Focusable(true, true, null);
				this.BxT.Focusable(true, true, null);
				this.DPageL.gameObject.SetActive(true);
				this.DPageR.gameObject.SetActive(true);
				this.BxD.Focus();
				this.BxT.hide();
				this.BxR.hide();
				this.BxL.hide();
				this.selectDetailDescRow();
				if (this.WmCtr != null)
				{
					this.WmCtr.bind();
					this.WmCtr.fine_flag = true;
				}
				this.fineTopInfoKDVisible();
				break;
			}
			this.t_add_push = 0f;
			IN.clearPushDown(true);
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				IN.DestroyOne(base.gameObject);
				return false;
			}
			if (this.state == UiFieldGuide.STATE.FIRST)
			{
				this.state_t += 1f;
				if (this.state_t >= 3f)
				{
					this.initialize();
				}
			}
			bool flag = this.state_t == 0f;
			if ((this.t_add_push > 0f && IN.isUiAddO()) || (this.t_add_push == 0f && IN.isUiAddPD()))
			{
				this.t_add_push = ((this.t_add_push == 0f) ? 1f : (this.t_add_push + fcnt));
			}
			else if (this.t_add_push > 0f)
			{
				this.t_add_push = -this.t_add_push;
			}
			else if (this.t_add_push < 0f)
			{
				this.t_add_push = 0f;
			}
			if (this.state > UiFieldGuide.STATE._NOUSE && this.fine_current_topic_scroll)
			{
				aBtnFDRow b = this.PreTopicRow.B;
				if (b != null)
				{
					this.fine_current_topic_scroll = false;
					this.BConR.BelongScroll.reveal(b, true, REVEALTYPE.IF_HIDING);
				}
			}
			this.state_t += fcnt;
			UiFieldGuide.STATE state = this.state;
			if (state != UiFieldGuide.STATE.ITEM_TOPIC)
			{
				if (state == UiFieldGuide.STATE.DETAIL)
				{
					this.runDetail(fcnt);
				}
			}
			else
			{
				this.runItemTopic(flag, fcnt);
			}
			return true;
		}

		private void runItemTopic(bool init_fla, float fcnt)
		{
			if (this.pickup_use && IN.isMenuPD(1))
			{
				if (this.state_t >= 10f)
				{
					this.clearPickup();
				}
			}
			else if (IN.isCancel() && this.state_t >= 10f)
			{
				if (this.left_category_selected != 0f)
				{
					if (!IN.isUiSortO())
					{
						this.topicBack2Right();
					}
				}
				else if (this.pickup_use)
				{
					if (this.BDetailPre != null)
					{
						if (!this.BDetailPre.isActive())
						{
							this.clearPickup();
						}
						this.BDetailPre.Select(true);
						this.fine_current_topic_scroll = true;
						this.changeState(UiFieldGuide.STATE.DETAIL);
						SND.Ui.play("cancel", false);
						return;
					}
					this.clearPickup();
					return;
				}
				else
				{
					this.changeState(UiFieldGuide.STATE._NOUSE);
				}
				SND.Ui.play("cancel", false);
				return;
			}
			if (this.BxD.isFocused())
			{
				if (this.BDetailPre != null)
				{
					if (!this.BDetailPre.isActive())
					{
						this.clearPickup();
					}
					this.BDetailPre.Select(true);
				}
				this.changeState(UiFieldGuide.STATE.DETAIL);
				return;
			}
			if (IN.isUiSortPD())
			{
				if (this.left_category_selected != 0f)
				{
					this.topicBack2Right();
				}
				else
				{
					aBtn aBtn;
					this.getCategHead(this.PreTopicRow.categ, out aBtn);
					if (aBtn != null)
					{
						aBtn.Select(true);
						if (!this.pickup_use)
						{
							this.TopTab.setValue(0, true);
						}
						this.left_category_selected = 1f;
					}
				}
			}
			if (this.left_category_selected > 0f)
			{
				this.left_category_selected += fcnt;
			}
			if (this.left_category_selected > 0f && !IN.isUiSortO())
			{
				if (this.left_category_selected >= 35f)
				{
					this.topicBack2Right();
				}
				else
				{
					this.left_category_selected = -1f;
				}
			}
			if (this.left_category_selected == 0f)
			{
				this.simulateTopicNavi();
			}
			else if (IN.isLP(1) || IN.isRP(1))
			{
				this.topicBack2Right();
			}
			this.checkFavoriteSwitch();
		}

		private void checkFavoriteSwitch()
		{
			if (this.t_add_push < 0f && this.PreTopicRow.valid && this.t_add_push >= -12f)
			{
				UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
				if (!fdr.valid)
				{
					CURS.limitVib(this.PreTopicRow.B, AIM.R);
					SND.Ui.play("toggle_button_limit", false);
					return;
				}
				if (!fdr.fd_favorite)
				{
					SND.Ui.play("tool_drag_init", false);
					fdr.fd_favorite = true;
				}
				else
				{
					fdr.fd_favorite = false;
					SND.Ui.play("tool_drag_quit", false);
				}
				UiFieldGuide.FdrEntry fdrEntry;
				if (this.OAItm2Row.TryGetValue(this.PreTopicRow.B.getFDR(), out fdrEntry))
				{
					for (int i = fdrEntry.Count - 1; i >= 0; i--)
					{
						fdrEntry[i].B.Fine(false);
					}
				}
				this.DTitleIcon.redraw_flag = true;
			}
		}

		private bool simulateTopicNavi()
		{
			int num = 0;
			bool flag = false;
			if (this.t_add_push > 0f)
			{
				flag = true;
			}
			bool flag2;
			bool flag3;
			if (this.isTopic())
			{
				flag2 = this.subcateg_mask;
				if (IN.isL())
				{
					num -= 7;
				}
				if (IN.isR())
				{
					num += 7;
				}
				if (IN.isT())
				{
					num--;
				}
				if (IN.isB())
				{
					num++;
				}
				flag3 = X.Abs(num) > 1;
			}
			else
			{
				flag3 = (flag2 = true);
				if (IN.isL())
				{
					num--;
				}
				if (IN.isR())
				{
					num++;
				}
			}
			if (num != 0)
			{
				if (this.t_add_push > 0f)
				{
					this.t_add_push = 100f;
				}
				aBtn aBtn = this.simulateTopicNavi(num, flag3, flag2, flag);
				if (aBtn != this.PreTopicRow.B)
				{
					this.topicJump(aBtn);
					return true;
				}
				CURS.limitVib(this.PreTopicRow.B, (num > 0) ? AIM.B : AIM.T);
				SND.Ui.play("toggle_button_limit", false);
			}
			return false;
		}

		private UiFieldGuide.FDR getFDRFor(NelItem Itm)
		{
			foreach (KeyValuePair<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> keyValuePair in this.OAItm2Row)
			{
				if (keyValuePair.Key.Itm == Itm)
				{
					return keyValuePair.Key;
				}
			}
			return default(UiFieldGuide.FDR);
		}

		private UiFieldGuide.FDR getFDRFor(FDSummonerInfo FSmn)
		{
			if (FSmn.B != null)
			{
				return FSmn.B.getFDR();
			}
			return default(UiFieldGuide.FDR);
		}

		private UiFieldGuide.FDR getFDRFor(EnemySummoner Smn)
		{
			foreach (KeyValuePair<string, FDSummonerInfo> keyValuePair in this.OSummonerInfo)
			{
				if (keyValuePair.Value.Summoner == Smn && keyValuePair.Value.valid && keyValuePair.Value.B != null)
				{
					return keyValuePair.Value.B.getFDR();
				}
			}
			return default(UiFieldGuide.FDR);
		}

		private UiFieldGuide.FDR getFDRFor(ENEMYID id)
		{
			if (id > (ENEMYID)0U)
			{
				foreach (KeyValuePair<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> keyValuePair in this.OAItm2Row)
				{
					if (keyValuePair.Key.enemyid == id)
					{
						return keyValuePair.Key;
					}
				}
			}
			return default(UiFieldGuide.FDR);
		}

		private bool topicJump(NelItem Itm)
		{
			foreach (KeyValuePair<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> keyValuePair in this.OAItm2Row)
			{
				if (keyValuePair.Key.Itm == Itm)
				{
					return this.topicJump(keyValuePair.Key);
				}
			}
			return false;
		}

		private bool topicJump(ENEMYID id)
		{
			if (id > (ENEMYID)0U)
			{
				foreach (KeyValuePair<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> keyValuePair in this.OAItm2Row)
				{
					if (keyValuePair.Key.enemyid == id)
					{
						return this.topicJump(keyValuePair.Key);
					}
				}
				return false;
			}
			return false;
		}

		private bool topicJump(UiFieldGuide.FDR Fdr)
		{
			UiFieldGuide.FdrEntry fdrEntry;
			UiFieldGuide.ItmAndCateg categData = this.getCategData(Fdr, out fdrEntry);
			return categData.valid && this.topicJump(categData.B);
		}

		private bool topicJump(aBtn B)
		{
			if (!B.gameObject.activeSelf)
			{
				this.clearPickup();
			}
			this.fine_current_topic_scroll = true;
			if (this.isTopic())
			{
				SND.Ui.play("cursor", false);
				B.Select(false);
			}
			else
			{
				SND.Ui.play("paper", false);
				this.fnHoverTopicRight(B);
			}
			return true;
		}

		private UiFieldGuide.CategHead getCategHead(aBtn BL)
		{
			UiFieldGuide.CategHead categHead;
			if (this.OCategHead.TryGetValue(BL, out categHead))
			{
				return categHead;
			}
			return default(UiFieldGuide.CategHead);
		}

		private UiFieldGuide.CategHead getCategHeadRight(aBtn BHead, out aBtn BL)
		{
			BL = null;
			foreach (KeyValuePair<aBtn, UiFieldGuide.CategHead> keyValuePair in this.OCategHead)
			{
				if (keyValuePair.Value.BHead == BHead)
				{
					BL = keyValuePair.Key;
					return keyValuePair.Value;
				}
			}
			return default(UiFieldGuide.CategHead);
		}

		private UiFieldGuide.CategHead getCategHead(UiItemWholeSelector.WCATEG c, out aBtn BL)
		{
			BL = null;
			foreach (KeyValuePair<aBtn, UiFieldGuide.CategHead> keyValuePair in this.OCategHead)
			{
				if (keyValuePair.Value.categ == c)
				{
					BL = keyValuePair.Key;
					return keyValuePair.Value;
				}
			}
			return default(UiFieldGuide.CategHead);
		}

		private bool fnHoverTopicLeft(aBtn B)
		{
			if (!this.isTopic())
			{
				return false;
			}
			if (this.left_category_selected == 0f)
			{
				this.left_category_selected = -1f;
				this.BConL.setValue(-1, true);
				SND.Ui.play("enter_small", false);
			}
			UiFieldGuide.CategHead categHead = this.getCategHead(B);
			UiItemWholeSelector.WCATEG categ = categHead.categ;
			if (categ == UiItemWholeSelector.WCATEG.ALL)
			{
				return false;
			}
			if (this.PreTopicRow.valid)
			{
				UiFieldGuide.FdrEntry fdrEntry;
				this.getCateg(this.PreTopicRow.B, out fdrEntry);
				this.PreTopicRow = default(UiFieldGuide.ItmAndCateg);
				if (fdrEntry != null)
				{
					int count = fdrEntry.Count;
					for (int i = 0; i < count; i++)
					{
						UiFieldGuide.ItmAndCateg itmAndCateg = fdrEntry[i];
						if (itmAndCateg.categ == categ && itmAndCateg.valid_enabled)
						{
							this.PreTopicRow = itmAndCateg;
							break;
						}
					}
				}
			}
			if (!this.PreTopicRow.valid)
			{
				UiFieldGuide.FdrEntry fdrEntry2;
				this.PreTopicRow = this.getCategData(this.pickup_use ? categHead.BR_PickUp : categHead.BR, out fdrEntry2);
				if (this.PreTopicRow.valid_enabled)
				{
					this.fine_current_topic_scroll = true;
				}
			}
			this.resetTopicCategory();
			return true;
		}

		private void topicBack2Right()
		{
			if (this.PreTopicRow.valid)
			{
				this.PreTopicRow.B.Select(true);
				SND.Ui.play("cursor", false);
			}
			if (this.left_category_selected > 0f)
			{
				this.left_category_selected = -1f;
			}
		}

		private bool fnHoverTopicRight(aBtn B)
		{
			UiFieldGuide.FdrEntry fdrEntry;
			UiFieldGuide.ItmAndCateg categData = this.getCategData(B, out fdrEntry);
			if (!categData.valid)
			{
				return false;
			}
			aBtn aBtn;
			if (!this.getCategHead(categData.categ, out aBtn).valid)
			{
				return false;
			}
			UiItemWholeSelector.WCATEG categ = this.PreTopicRow.categ;
			UiFieldGuide.FDR fdr = (this.PreTopicRow.valid ? this.PreTopicRow.getFDR() : default(UiFieldGuide.FDR));
			this.PreTopicRow = categData;
			if (!fdr.valid || !fdr.Equals(this.PreTopicRow.getFDR()))
			{
				this.fineDetail(fdr);
			}
			if (this.isDetail())
			{
				this.BConR.setValue(this.BConR.getIndex(B), true);
				IN.clearPushDown(true);
			}
			if (categ != this.PreTopicRow.categ || this.left_category_selected != 0f)
			{
				this.BConL.setValue(this.BConL.getIndex(aBtn), true);
				this.BConL.BelongScroll.reveal(aBtn, true, REVEALTYPE.CENTER);
				this.left_category_selected = 0f;
				if (categ != this.PreTopicRow.categ)
				{
					if (this.state_t >= 2f)
					{
						SND.Ui.play("cursor_gear_reset", false);
					}
					this.resetTopicCategory();
				}
			}
			return true;
		}

		private bool FnChangedTopicRight(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			aBtnFDRow aBtnFDRow = _B.Get(cur_value) as aBtnFDRow;
			if (aBtnFDRow == null)
			{
				return false;
			}
			if (!aBtnFDRow.getFDR().valid)
			{
				SND.Ui.play("locked", false);
				return false;
			}
			if (this.PreTopicRow.B != aBtnFDRow)
			{
				this.fnHoverTopicRight(aBtnFDRow);
			}
			if (this.isTopic())
			{
				this.changeState(UiFieldGuide.STATE.DETAIL);
			}
			return true;
		}

		private void resetTopicCategory()
		{
			this.TopTab.DestroyButtons(true, false);
			this.TopTab.carr_fix_length = -1;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				UiItemWholeSelector.ListUpSubForSpecificCategory(this.PreTopicRow.categ, blist);
				int count = blist.Count;
				float num = this.TopTab.BelongScroll.get_swidth_px() - 12f - 10f;
				this.TopTab.default_w = X.Mx((float)(X.ENG_MODE ? 140 : 100), num / (float)count);
				for (int i = 0; i < count; i++)
				{
					this.TopTab.MakeT<aBtnNel>(blist[i], "row_tab").get_Skin().setTitle(UiItemWholeSelector.getTitleForSubKey(this.PreTopicRow.categ, blist[i]));
				}
				ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = this.TopTab.getBaseCarr() as ObjCarrierConBlockBtnContainer<aBtn>;
				objCarrierConBlockBtnContainer.BoundsPx(this.TopTab.default_w * (float)(count - 1), 0f, -1);
				objCarrierConBlockBtnContainer.Intv(0f, 0f, count);
				objCarrierConBlockBtnContainer.ItemSizePx(this.TopTab.default_w, this.TopTab.default_h, -1);
				this.TopTab.runCarrier(9000f, objCarrierConBlockBtnContainer);
				this.TopTab.ParentTab.RowRemakeHeightRecalc(objCarrierConBlockBtnContainer, null);
			}
			this.TopTab.setValue(0, true);
			this.TopTab.BelongScroll.setScrollLevelTo(0f, 0f, true);
		}

		private bool FnChangedTopTab(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (!this.isTopic() && _B != null)
			{
				return false;
			}
			if (cur_value <= 0)
			{
				if (this.subcateg_mask)
				{
					int length = this.BConR.Length;
					for (int i = 0; i < length; i++)
					{
						aBtnFDRow aBtnFDRow = this.BConR.Get(i) as aBtnFDRow;
						if (aBtnFDRow != null)
						{
							aBtnFDRow.SetLocked(false, true, false);
						}
					}
				}
				this.subcateg_mask = false;
			}
			else
			{
				this.subcateg_mask = true;
				aBtn aBtn = this.TopTab.Get(cur_value);
				if (aBtn == null)
				{
					return true;
				}
				string title = aBtn.title;
				int length2 = this.BConR.Length;
				UiItemWholeSelector.WCATEG wcateg = UiItemWholeSelector.WCATEG.ALL;
				for (int j = 0; j < length2; j++)
				{
					aBtn aBtn2 = this.BConR.Get(j);
					if (!(aBtn2 is aBtnFDRow))
					{
						aBtn aBtn3;
						wcateg = this.getCategHeadRight(aBtn2, out aBtn3).categ;
					}
					else
					{
						aBtnFDRow aBtnFDRow2 = aBtn2 as aBtnFDRow;
						UiFieldGuide.FDR fdr = aBtnFDRow2.getFDR();
						if (wcateg != this.PreTopicRow.categ || !fdr.valid)
						{
							aBtnFDRow2.SetLocked(true, true, false);
						}
						else if (fdr.Itm != null)
						{
							aBtnFDRow2.SetLocked(!UiItemWholeSelector.isValidForSubKey(fdr.Itm, wcateg, title), true, false);
						}
						else
						{
							aBtnFDRow2.SetLocked(true, true, false);
						}
					}
				}
			}
			return true;
		}

		private aBtn simulateTopicNavi(int navi, bool skip_lock, bool skip_cache, bool only_fav = false)
		{
			if (!this.PreTopicRow.valid || navi == 0)
			{
				return this.PreTopicRow.B;
			}
			int num = ((aBtn.PreSelected != null) ? this.BConR.getIndex(aBtn.PreSelected) : (-1));
			if (num == -1)
			{
				num = this.BConR.getIndex(this.PreTopicRow.B);
			}
			if (num == -1)
			{
				return this.PreTopicRow.B;
			}
			int num2 = X.MPF(navi > 0);
			int num3 = X.Abs(navi);
			bool flag = true;
			int length = this.BConR.Length;
			aBtnFDRow aBtnFDRow = this.PreTopicRow.B;
			UiItemWholeSelector.WCATEG wcateg = this.PreTopicRow.categ;
			for (int i = 0; i < length; i++)
			{
				num += num2;
				if (num < 0 || num >= length)
				{
					if (!flag)
					{
						break;
					}
					num = (num + length) % length;
					num3 = X.Mn(num3, 1);
				}
				aBtnFDRow aBtnFDRow2 = this.BConR.Get(num) as aBtnFDRow;
				if (aBtnFDRow2 == null || !aBtnFDRow2.gameObject.activeSelf)
				{
					num3 = X.Mn(num3, 1);
				}
				else
				{
					UiFieldGuide.FDR fdr = aBtnFDRow2.getFDR();
					if (((aBtnFDRow2.isLocked() || !fdr.valid) && skip_lock) || (fdr.is_cache_item && skip_cache) || (only_fav && !fdr.fd_favorite))
					{
						if (!flag)
						{
							break;
						}
						num3 = X.Mn(num3, 1);
					}
					else
					{
						UiFieldGuide.FdrEntry fdrEntry;
						UiFieldGuide.ItmAndCateg categData = this.getCategData(aBtnFDRow2, out fdrEntry);
						if (categData.valid)
						{
							if (categData.categ != wcateg)
							{
								if (!flag)
								{
									break;
								}
								wcateg = categData.categ;
								num3 = X.Mn(num3, 1);
							}
							flag = false;
							aBtnFDRow = aBtnFDRow2;
							if (--num3 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			return aBtnFDRow;
		}

		private void clearPickup()
		{
			if (this.pickup_use)
			{
				this.pickup_use = false;
				this.finePickUp();
				SND.Ui.play("cursor_gear_reset", false);
			}
		}

		private void finePickUp()
		{
			if (this.subcateg_mask)
			{
				this.TopTab.setValue(0, false);
				this.FnChangedTopTab(null, 0, 0);
			}
			UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
			UiItemWholeSelector.WCATEG wcateg = (this.PreTopicRow.valid ? this.PreTopicRow.categ : UiItemWholeSelector.WCATEG.ALL);
			bool pickup_use = this.pickup_use;
			this.BConR.DestroyButtons(true, true);
			this.TopTab.setRowMemoryVisible(!pickup_use);
			this.BxT.getDesignerBlockMemory(this.DTopPickUp).active = pickup_use;
			this.BxT.row_remake_flag = true;
			this.pickup_wcateg_using = UiItemWholeSelector.WCATEG.ALL;
			if (pickup_use)
			{
				if (this.ADetailHistory.Count > 0)
				{
					this.ADetailHistory.Clear();
					this.need_recreate_detail_dpage = true;
				}
				foreach (KeyValuePair<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> keyValuePair in this.OAItm2Row)
				{
					if (keyValuePair.Value != null && keyValuePair.Value.Count != 0)
					{
						UiFieldGuide.FDR key = keyValuePair.Key;
						bool flag = this.CurPickUp.isPickedUpFor(key);
						int count = keyValuePair.Value.Count;
						if (flag)
						{
							aBtnFDRow aBtnFDRow = null;
							UiItemWholeSelector.WCATEG wcateg2 = UiItemWholeSelector.WCATEG.ALL;
							for (int i = 0; i < count; i++)
							{
								UiFieldGuide.ItmAndCateg itmAndCateg = keyValuePair.Value[i];
								if (itmAndCateg.categ == this.PreTopicRow.categ)
								{
									aBtnFDRow = itmAndCateg.B;
									wcateg2 = itmAndCateg.categ;
									break;
								}
							}
							if (aBtnFDRow == null)
							{
								aBtnFDRow = keyValuePair.Value[0].B;
								wcateg2 = keyValuePair.Value[0].categ;
							}
							this.BConR.addBtn(aBtnFDRow);
							aBtnFDRow.gameObject.SetActive(true);
							aBtnFDRow.bind();
							this.BConR.APool.Remove(aBtnFDRow);
							ButtonSkinFDItemRow buttonSkinFDItemRow = aBtnFDRow.get_Skin() as ButtonSkinFDItemRow;
							if (buttonSkinFDItemRow != null)
							{
								string text = null;
								int num = 0;
								buttonSkinFDItemRow.clearCenterPowerT();
								if (this.CurPickUp.rpi != RCP.RPI_EFFECT.NONE)
								{
									int num2 = (key.individual_grade ? this.getSpecificGrade(key.Itm, false) : this.target_grade);
									buttonSkinFDItemRow.setCenterPowerTRpi(this.CurPickUp.rpi, num2);
								}
								if (text != null)
								{
									buttonSkinFDItemRow.setCenterPowerT(num, text);
								}
							}
							if ((this.pickup_wcateg_using & wcateg2) == UiItemWholeSelector.WCATEG.ALL)
							{
								this.pickup_wcateg_using |= wcateg2;
								UiFieldGuide.CategHead categHead;
								if (this.OCategHead0.TryGetValue(wcateg2, out categHead))
								{
									categHead.BR_PickUp = aBtnFDRow;
									this.OCategHead0[wcateg2] = categHead;
								}
							}
						}
					}
				}
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("catalog_pickup", false).Add(": ");
					this.CurPickUp.AddTxDescription(stb);
					this.FbPickUp.Txt(stb);
					goto IL_03AB;
				}
			}
			this.BConR.APool.Sort((aBtn A, aBtn B) => A.carr_index - B.carr_index);
			int count2 = this.BConR.APool.Count;
			for (int j = 0; j < count2; j++)
			{
				aBtn aBtn = this.BConR.APool[j];
				this.BConR.addBtn(aBtn);
				aBtn.gameObject.SetActive(true);
				aBtn.bind();
				ButtonSkinFDItemRow buttonSkinFDItemRow2 = aBtn.get_Skin() as ButtonSkinFDItemRow;
				if (buttonSkinFDItemRow2 != null)
				{
					buttonSkinFDItemRow2.clearCenterPowerT();
				}
			}
			this.BConR.APool.Clear();
			this.pickup_wcateg_using = this.wcateg_using0;
			IL_03AB:
			this.BConR.OuterScrollBox.reposition(false);
			this.TopTab.lr_input = !pickup_use;
			this.DTabDesc.row_remake_flag = true;
			aBtn aBtn2 = null;
			if (fdr.valid)
			{
				UiFieldGuide.FdrEntry fdrEntry;
				UiFieldGuide.ItmAndCateg categData = this.getCategData(fdr, wcateg, out fdrEntry);
				if (categData.valid && categData.B.isActive())
				{
					if (this.isTopic())
					{
						categData.B.Select(true);
					}
					else
					{
						this.fnHoverTopicRight(categData.B);
					}
					aBtn2 = categData.B;
				}
			}
			if (aBtn2 == null && this.BConR.Length > 0)
			{
				aBtn2 = this.BConR.Get(0);
				aBtn2.Select(true);
			}
			if (aBtn2 != null)
			{
				this.fine_current_topic_scroll = true;
			}
			this.need_recreate_detail_dpage = true;
			this.OCategHead.Clear();
			this.BConL.RemakeT<aBtnNel>(null, null);
			this.finePickUpKD();
		}

		private void finePickUpKD()
		{
			FillBlock fillBlock = this.DTopPickUp.Get("pickup_fb_R", false) as FillBlock;
			if (fillBlock != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					if (this.BDetailPre == null)
					{
						stb.Add("<key x/>");
					}
					stb.Add("<key menu/>");
					stb.AddTxA("Reset", false);
					fillBlock.Txt(stb);
				}
			}
		}

		private void initDesignerDetail()
		{
			this.BxD = base.Create("right", this.detail_shift_x, 0f, this.detail_w, this.out_h, 0, 200f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxD.Focusable(true, true, null);
			this.BxD.margin_in_lr = 38f;
			this.BxD.margin_in_tb = 22f;
			this.BxD.item_margin_x_px = 0f;
			this.BxD.item_margin_y_px = 2f;
			this.BxD.animate_maxt = 0;
			this.BxD.alignx = ALIGN.LEFT;
			this.BxD.use_scroll = false;
			this.BxD.init();
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				text = " ",
				size = 14f,
				swidth = this.BxD.use_w * 0.5f - 1f,
				html = true,
				alignx = ALIGN.LEFT,
				sheight = 16f,
				use_valotile = 1
			};
			this.DsnDetailP = dsnDataP;
			this.DPageL = this.BxD.addP(dsnDataP, false);
			dsnDataP.alignx = ALIGN.RIGHT;
			this.DPageR = this.BxD.addP(dsnDataP, false);
			DsnDataHr dsnDataHr = new DsnDataHr
			{
				line_height = 2f,
				dashed_oneline_lgt = 6f,
				draw_width_rate = 1f,
				margin_b = 8f,
				margin_t = 6f
			};
			this.BxD.Br();
			this.BxD.addHr(dsnDataHr.W(this.BxD.use_w));
			this.BxD.Br();
			float num = this.BxD.use_h - 30f;
			this.DTabL = this.BxD.addTab("DTabL", this.BxD.use_w * 0.52f, num, this.BxD.use_w * 0.52f, num, false);
			this.DTabL.Smallest();
			this.DTabL.item_margin_x_px = 0f;
			this.DTabL.XSh(8f);
			this.DTitleIcon = this.BxD.addImg(new DsnDataImg
			{
				sheight = 84f,
				swidth = 58f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDtDrawIcon)
			});
			this.DTabLT = this.DTabL.addTab("DTabL", this.DTabL.use_w * 0.68f, 84f, this.BxD.use_w * 0.68f, 84f, false);
			this.DTabLT.Smallest();
			this.DTabLT.margin_in_tb = 6f;
			this.DTabLT.init();
			this.DTitle = this.DTabLT.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				size = 32f,
				sheight = 64f - this.DTabLT.margin_in_tb * 2f,
				swidth = this.DTabLT.use_w,
				text = " ",
				text_margin_x = 4f,
				text_auto_condense = true,
				text_auto_wrap = false,
				aligny = ALIGNY.BOTTOM,
				text_margin_y = 6f,
				TargetFont = TX.getTitleFont()
			}, false);
			this.DTitleKind = this.DTabLT.Br().addP(new DsnDataP("", false)
			{
				TxCol = C32.MulA(4283780170U, 0.33f),
				text = " ",
				size = 12f,
				html = true,
				sheight = 20f,
				text_auto_condense = true,
				swidth = this.DTabLT.use_w,
				aligny = ALIGNY.MIDDLE,
				text_margin_x = 0f,
				text_margin_y = 0f
			}, false);
			this.DTabL.endTab(true);
			this.DTabInfo = this.DTabL.addTab("DTabInfo", this.DTabL.use_w, this.top_h, this.DTabL.use_w, this.top_h, false);
			this.DTabInfo.Smallest();
			this.DInfo = this.DTabInfo.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				text = " ",
				size = 12f,
				html = true,
				text_auto_wrap = true,
				swidth = this.DTabInfo.use_w,
				sheight = this.DTabInfo.use_h - 24f
			}, false);
			this.DTabInfo.Br();
			DsnDataP dsnDataP2 = new DsnDataP("", false)
			{
				swidth = 38f,
				sheight = 24f,
				alignx = ALIGN.CENTER,
				text_margin_x = 0f,
				text_margin_y = 0f,
				size = 16f,
				html = true,
				text = "<key ltab/>",
				name = "tabinfo_KD_L",
				use_valotile = 1
			};
			this.DTabInfo.addP(dsnDataP2, false);
			this.DGrade = this.DTabInfo.addImg(new DsnDataImg
			{
				sheight = 24f,
				swidth = this.DTabInfo.use_w - dsnDataP2.swidth,
				MI = MTRX.MIicon,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDtDrawGrade)
			});
			dsnDataP2.Text("<key rtab/>");
			dsnDataP2.name = "tabinfo_KD_R";
			this.DTabInfo.addP(dsnDataP2, false);
			this.DTabL.endTab(true);
			this.DTabL.Br();
			this.DTabL.addHr(dsnDataHr.W(this.DTabL.use_w));
			this.DTabL.Br();
			this.DTabDesc = this.DTabL.addTab("DTabDesc", this.DTabL.use_w, this.DTabL.use_h, this.DTabL.use_w, this.DTabL.use_h, true);
			this.DTabDesc.margin_in_lr = 18f;
			this.DTabDesc.margin_in_tb = 10f;
			this.DTabDesc.item_margin_x_px = (this.DTabDesc.item_margin_y_px = 0f);
			this.DTabDesc.scrolling_margin_in_lr = 8f;
			this.DTabDesc.scrolling_margin_in_tb = 4f;
			this.DTabDesc.init();
			this.DTabL.endTab(true);
			this.BxD.endTab(true);
			dsnDataHr.W(num);
			dsnDataHr.vertical = true;
			this.BxD.addHr(dsnDataHr);
			dsnDataHr.vertical = false;
			if (this.M2D != null)
			{
				Designer designer = this.BxD.addTab("DTabR", this.BxD.use_w, num, this.BxD.use_w, num, false);
				designer.Smallest();
				designer.margin_in_lr = 10f;
				designer.margin_in_tb = 2f;
				designer.item_margin_y_px = 4f;
				designer.stencil_ref = -1;
				WholeMapItem curWM = this.M2D.WM.CurWM;
				designer.init();
				designer.addHr(new DsnDataHr().H(8f));
				designer.Br();
				this.WmCtr = new FieldGuideWmController(this.M2D, new Func<bool>(this.isDetail)).createUiTo(this.BxD, designer.use_w, designer.use_h - 42f);
				designer.Br();
				this.DMapKD = this.BxD.addP(new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					text = " ",
					size = 16f,
					html = true,
					swidth = designer.use_w,
					sheight = 34f,
					text_auto_wrap = true,
					alignx = ALIGN.RIGHT,
					aligny = ALIGNY.MIDDLE,
					text_margin_x = 3f,
					text_margin_y = 2f,
					use_valotile = 1
				}, false);
				this.BxD.endTab(true);
			}
			this.BxD.Br();
			dsnDataHr.vertical = false;
			this.BxD.addHr(dsnDataHr.W(this.BxD.use_w));
			this.BxD.Br();
			this.BxD.alignx = ALIGN.LEFT;
			this.DBottomHold = this.BxD.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				text = " ",
				size = 16f,
				html = true,
				swidth = this.BxD.use_w * 0.3f,
				sheight = 18f,
				text_auto_condense = true,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.MIDDLE,
				text_margin_x = 24f,
				text_margin_y = 2f
			}, false);
			this.DBottomObtain = this.BxD.addP(new DsnDataP("", false)
			{
				TxCol = C32.d2c(4283780170U),
				text = " ",
				size = 16f,
				html = true,
				swidth = this.BxD.use_w - 10f,
				sheight = 18f,
				text_auto_condense = true,
				alignx = ALIGN.RIGHT,
				aligny = ALIGNY.MIDDLE,
				text_margin_x = 18f,
				text_margin_y = 2f
			}, false);
			this.BxD.Br();
			this.DsnDetailP.swidth = this.DTabDesc.use_w;
			this.DsnDetailP.size = 16f;
			this.DsnDetailP.alignx = ALIGN.LEFT;
			this.DsnDetailP.aligny = ALIGNY.BOTTOM;
			this.DsnDetailP.sheight = 34f;
			this.DsnDetailP.text_auto_wrap = true;
			this.DsnDetailP.text_margin_x = 6f;
			this.DsnDetailP.text_margin_y = 6f;
			this.DsnDetailHr = new DsnDataHr
			{
				margin_t = 12f,
				margin_b = 12f,
				Col = C32.MulA(4283780170U, 0.7f)
			};
			this.DsnDetailRB = new DsnDataButton
			{
				title = "_",
				w = this.DTabDesc.use_w,
				h = 30f,
				skin = "row_fieldguide",
				fnClick = new FnBtnBindings(this.fnClickDetailRow),
				fnHover = new FnBtnBindings(this.fnHoverDetailRow)
			};
		}

		private void runDetail(float fcnt)
		{
			if ((IN.isCancel() && this.DetailSelectMain == null) || this.BxR.isFocused() || this.BxT.isFocused())
			{
				SND.Ui.play("cancel", false);
				if (this.ADetailHistory.Count > 0)
				{
					this.ADetailHistory.Clear();
					this.need_recreate_detail_dpage = true;
				}
				this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
				return;
			}
			if (!this.PreTopicRow.valid)
			{
				return;
			}
			UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
			if (this.DetailSelectMain == null)
			{
				if (this.fine_current_detail_scroll)
				{
					this.fine_current_detail_scroll = false;
					aBtn preSelected = aBtn.PreSelected;
					if (preSelected != null && preSelected.get_Skin() is ButtonSkinNelFieldGuide)
					{
						preSelected.BelongScroll.reveal(preSelected, true, REVEALTYPE.IF_HIDING);
					}
					fdr = this.PreTopicRow.getFDR();
				}
				if (this.need_recreate_detail)
				{
					this.fineDetail(this.PreTopicRow.getFDR());
				}
				if ((this.t_add_push < 0f || this.t_add_push == 1f) && this.ADetailHistory.Count == 0)
				{
					if (this.t_add_push == 1f)
					{
						SND.Ui.play("store_cart_out", false);
					}
					this.fineDetailPageShift();
				}
				if (this.need_recreate_detail_dpage)
				{
					this.fineDetailPageShift();
				}
				if (!fdr.individual_grade)
				{
					int num = 0;
					if (IN.isLTabPD())
					{
						num = -1;
					}
					if (IN.isRTabPD())
					{
						num = 1;
					}
					if (num != 0)
					{
						this.target_grade = (this.target_grade + num + 5) % 5;
						SND.Ui.play("tool_gradation", false);
						this.fineTopInfoKDVisible();
						this.fineDetailBasicInfo();
						this.item_value_refining = 0;
						if (fdr.Itm != null)
						{
							NelItem itm = fdr.Itm;
							itm.Use(null, null, this.target_grade, this);
							if (itm.RecipeInfo != null && itm.RecipeInfo.Oeffect100.Count > 0)
							{
								BtnContainer<aBtn> btnContainer = this.DTabDesc.getBtnContainer();
								int length = btnContainer.Length;
								for (int i = 0; i < length; i++)
								{
									ButtonSkinNelFieldGuide buttonSkinNelFieldGuide = btnContainer.Get(i).get_Skin() as ButtonSkinNelFieldGuide;
									float num2;
									if (buttonSkinNelFieldGuide != null && buttonSkinNelFieldGuide.rpi_ef != RCP.RPI_EFFECT.NONE && itm.RecipeInfo.Oeffect100.TryGetValue(buttonSkinNelFieldGuide.rpi_ef, out num2))
									{
										buttonSkinNelFieldGuide.initRpiEffect(itm, buttonSkinNelFieldGuide.rpi_ef, num2, this.target_grade);
									}
								}
							}
							if (this.detail_store_activated)
							{
								BtnContainer<aBtn> btnContainer2 = this.DTabDesc.getBtnContainer();
								int length2 = btnContainer2.Length;
								for (int j = 0; j < length2; j++)
								{
									ButtonSkinNelFieldGuide buttonSkinNelFieldGuide2 = btnContainer2.Get(j).get_Skin() as ButtonSkinNelFieldGuide;
									if (buttonSkinNelFieldGuide2 != null && buttonSkinNelFieldGuide2.Store != null)
									{
										buttonSkinNelFieldGuide2.initStoreManager(itm, buttonSkinNelFieldGuide2.Store, this.target_grade);
									}
								}
							}
						}
					}
				}
				bool flag = false;
				if (this.ADetailHistory.Count > 0)
				{
					int num3 = 0;
					if (IN.isLP(1))
					{
						num3 = -1;
					}
					if (IN.isRP(1))
					{
						num3 = 1;
					}
					if (num3 != 0)
					{
						int num4 = this.ADetailHistory.IndexOf(fdr);
						if (num4 == -1 || (num4 == 0 && num3 == -1))
						{
							SND.Ui.play("cursor_gear_reset", false);
							this.ADetailHistory.Clear();
							this.fineDetailPageShift();
						}
						else
						{
							num4 += num3;
							if (0 <= num4 && num4 < this.ADetailHistory.Count)
							{
								this.topicJump(this.ADetailHistory[num4]);
								flag = true;
							}
							else
							{
								CURS.limitVib(null, (num3 < 0) ? AIM.L : AIM.R);
								SND.Ui.play("toggle_button_limit", false);
							}
						}
					}
				}
				else
				{
					flag = this.simulateTopicNavi();
				}
				this.checkFavoriteSwitch();
				if (IN.isUiSortPD() && this.WmCtr != null)
				{
					this.DetailSelectMain = aBtn.PreSelected;
					this.WmCtr.initEdit();
					SND.Ui.play("toggle_button_open", false);
					this.fineTopInfoKDVisible();
				}
				if (flag)
				{
					return;
				}
			}
			else
			{
				byte b = 0;
				if (this.WmCtr == null || (IN.isCancelPD() && !this.WmSkin.isDragging()))
				{
					this.quitDetailMapEdit();
					return;
				}
				this.WmCtr.runEdit(fcnt, true, ref b);
				if (this.WmSkin.is_detail)
				{
					aBtnFDRow b2 = this.PreTopicRow.B;
					if (IN.isSubmitUp(9) && this.WmCtr.DetailMapIR != null)
					{
						SND.Ui.play("toggle_button_open", false);
						this.need_recreate_detail_dpage = true;
						UiFieldGuide.FDR fdrfor = this.getFDRFor(this.WmCtr.DetailMapIR.GReelItem);
						if (this.topicJump(fdrfor))
						{
							this.addDetailHistory(fdrfor, (b2 != null) ? b2.getFDR() : default(UiFieldGuide.FDR));
							this.quitDetailMapEdit();
						}
						else
						{
							SND.Ui.play("enter_small", false);
							this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
							this.CurPickUp = new UiFieldGuide.PickUp(this.WmCtr.DetailMapIR, "&&Catalog_pickup_current_reel_spot[" + ((this.M2D != null) ? this.M2D.getMapTitle(this.M2D.Get(this.WmCtr.detail_map_ir_map, false)) : "") + "]");
							this.BDetailPre = b2;
							this.finePickUp();
						}
					}
					if ((this.WmCtr.DetailMapSmn != null && IN.isLTabPD()) || (IN.isSubmitUp(9) && this.WmCtr.DetailMapIR == null))
					{
						UiFieldGuide.FDR fdrfor2 = this.getFDRFor(this.WmCtr.DetailMapSmn);
						if (this.topicJump(fdrfor2))
						{
							SND.Ui.play("toggle_button_open", false);
							this.addDetailHistory(fdrfor2, (b2 != null) ? b2.getFDR() : default(UiFieldGuide.FDR));
							this.quitDetailMapEdit();
						}
					}
				}
				if (b > 0)
				{
					this.fine_map_kd = true;
				}
			}
			if (this.WmCtr.runAppearing())
			{
				this.fine_map_kd = true;
			}
			if (this.fine_map_kd)
			{
				this.fineMapKD();
			}
		}

		private void quitDetailMapEdit()
		{
			if (this.DetailSelectMain != null)
			{
				SND.Ui.play("toggle_button_close", false);
				this.WmCtr.quitEdit(false);
				this.DetailSelectMain.Select(true);
				this.DetailSelectMain = null;
				this.WmCtr.clearTarget();
				this.fineTopInfoKDVisible();
			}
		}

		private void fineDetail(UiFieldGuide.FDR PreFdr)
		{
			if (!this.PreTopicRow.valid)
			{
				return;
			}
			this.need_recreate_detail = false;
			bool flag = this.target_grade < 0 || !PreFdr.valid;
			if (flag)
			{
				this.target_grade = 0;
			}
			UiFieldGuide.FDR fdr = this.PreTopicRow.B.getFDR();
			NelItem itm = fdr.Itm;
			this.DTitleIcon.redraw_flag = true;
			this.DetailSelectMain != null;
			if (flag || PreFdr.individual_grade != fdr.individual_grade)
			{
				this.fineTopInfoKDVisible();
			}
			this.fineDetailDesc();
			this.fineDetailBasicInfo();
			this.fineDetailPageShift();
			if (this.M2D != null)
			{
				if (itm != null)
				{
					using (STB stb = TX.PopBld(null, 0))
					{
						this.M2D.IMNG.holdItemString(stb, itm, -1, true);
						this.DBottomHold.Txt(stb);
						stb.Clear().AddTxA("CatalogI_item_obtained", false);
						int visible_obtain_count = itm.visible_obtain_count;
						if (visible_obtain_count >= 100)
						{
							stb.Add("99+");
						}
						else
						{
							stb.Add(visible_obtain_count);
						}
						this.DBottomObtain.Txt(stb);
					}
				}
				if (fdr.FSmn != null)
				{
					this.DBottomHold.text_content = "";
					if (fdr.valid)
					{
						using (STB stb2 = TX.PopBld(null, 0))
						{
							stb2.AddTxA("CatalogN_enemyspot_defeated", false).TxRpl(fdr.grade);
							this.DBottomObtain.Txt(stb2);
							goto IL_0183;
						}
					}
					this.DBottomObtain.text_content = "";
				}
				IL_0183:
				if (fdr.enemyid_available)
				{
					this.DBottomHold.text_content = "";
					if (fdr.valid)
					{
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb3.AddTxA("CatalogN_enemy_defeated", false).TxRpl(fdr.grade);
							this.DBottomObtain.Txt(stb3);
							goto IL_01F3;
						}
					}
					this.DBottomObtain.text_content = "";
				}
			}
			IL_01F3:
			if (this.ARbSk.Count > 0)
			{
				this.ARbSk[0].getBtn().setNaviT(this.ARbSk[this.ARbSk.Count - 1].getBtn(), true, true);
			}
			if (this.isDetail())
			{
				if (this.WmSkin != null)
				{
					this.WmSkin.fine_flag = true;
				}
				this.selectDetailDescRow();
			}
		}

		private void fineDetailPageShift()
		{
			if (!this.PreTopicRow.valid)
			{
				return;
			}
			this.need_recreate_detail_dpage = false;
			UiFieldGuide.FDR fdr = this.PreTopicRow.B.getFDR();
			if (this.ADetailHistory.Count <= 0)
			{
				bool flag = this.t_add_push > 0f;
				aBtnFDRow aBtnFDRow = this.simulateTopicNavi(-1, true, true, flag) as aBtnFDRow;
				this.fineDetailPageShift((aBtnFDRow != null && aBtnFDRow != this.PreTopicRow.B) ? aBtnFDRow.getFDR() : default(UiFieldGuide.FDR), this.DPageL);
				aBtnFDRow = this.simulateTopicNavi(1, true, true, flag) as aBtnFDRow;
				this.fineDetailPageShift((aBtnFDRow != null && aBtnFDRow != this.PreTopicRow.B) ? aBtnFDRow.getFDR() : default(UiFieldGuide.FDR), this.DPageR);
				return;
			}
			int num = this.ADetailHistory.IndexOf(fdr);
			if (num == -1)
			{
				this.DPageL.text_content = "";
				this.DPageR.text_content = "";
				return;
			}
			this.fineDetailPageShift((num > 0) ? this.ADetailHistory[num - 1] : default(UiFieldGuide.FDR), this.DPageL);
			this.fineDetailPageShift((num < this.ADetailHistory.Count - 1) ? this.ADetailHistory[num + 1] : default(UiFieldGuide.FDR), this.DPageR);
		}

		private void fineDetailPageShift(UiFieldGuide.FDR Fdr, FillBlock Fb)
		{
			if (!Fdr.valid)
			{
				Fb.text_content = "";
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				if (Fb == this.DPageL)
				{
					stb.Add("<key la/>");
				}
				else if (Fdr.fd_favorite)
				{
					stb.Add("<img mesh=\"favorite_star\" width=\"22\" />");
				}
				Fdr.copyNameTo(stb);
				if (Fb == this.DPageR)
				{
					stb.Add("<key ra/>");
				}
				else if (Fdr.fd_favorite)
				{
					stb.Add("<img mesh=\"favorite_star\" width=\"22\" />");
				}
				Fb.Txt(stb);
			}
		}

		private void selectDetailDescRow()
		{
			ScrollBox scrollBox = this.DTabDesc.getScrollBox();
			if (this.ARbSk.Count > 0)
			{
				UiFieldGuide.FDR fdr = (this.PreTopicRow.valid ? this.PreTopicRow.B.getFDR() : default(UiFieldGuide.FDR));
				this.DetailSelectMain = null;
				scrollBox.area_selectable = false;
				bool flag = false;
				if (fdr.valid)
				{
					UiFieldGuide.FdrEntry fdrEntry = X.Get<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>(this.OAItm2Row, fdr);
					if (fdrEntry != null)
					{
						flag = this.SelectDescRBByIndex((int)fdrEntry.last_selected);
					}
				}
				if (!flag)
				{
					this.ARbSk[0].getBtn().Select(true);
				}
			}
			else
			{
				scrollBox.area_selectable = true;
				scrollBox.Select();
			}
			this.fine_map_kd = true;
		}

		private void fineDetailBasicInfoEmpty(out bool detail_showing_rpi_categ)
		{
			detail_showing_rpi_categ = false;
			this.DTabLT.getDesignerBlockMemory(this.DTitleKind).active = false;
			using (STB stb = TX.PopBld(null, 0))
			{
				this.DTitle.Txt(NelItem.Unknown.getLocalizedName(stb, 0));
			}
			this.DInfo.text_content = "";
		}

		private void fineDetailBasicInfo()
		{
			UiFieldGuide.FDR fdr = this.PreTopicRow.B.getFDR();
			bool flag;
			if (!fdr.valid)
			{
				this.fineDetailBasicInfoEmpty(out flag);
			}
			else
			{
				if (fdr.Itm != null)
				{
					NelItem itm = fdr.Itm;
					int specificGrade = this.getSpecificGrade(itm, true);
					if (specificGrade < 0)
					{
						specificGrade = this.target_grade;
					}
					bool flag2 = true;
					RCP.RPI_CATEG rpi_CATEG = ((itm.RecipeInfo != null) ? itm.RecipeInfo.categ : ((RCP.RPI_CATEG)0));
					flag = rpi_CATEG > (RCP.RPI_CATEG)0;
					if (rpi_CATEG > (RCP.RPI_CATEG)0)
					{
						using (STB stb = TX.PopBld(null, 0))
						{
							RCP.getCategoryStringMultiTo(stb, rpi_CATEG);
							this.DTitleKind.Txt(stb);
						}
					}
					if (this.PreTopicRow.categ == UiItemWholeSelector.WCATEG.RECIPE)
					{
						RCP.Recipe recipeAllType = UiCraftBase.getRecipeAllType(itm);
						if (recipeAllType != null && recipeAllType.categ != RCP.RP_CATEG.ALOMA)
						{
							this.DTitle.text_content = recipeAllType.title;
							flag2 = false;
						}
					}
					using (STB stb2 = TX.PopBld(null, 0))
					{
						if (flag2)
						{
							itm.getLocalizedName(stb2.Clear(), specificGrade);
							this.DTitle.Txt(stb2);
						}
						stb2.Clear();
						if (!itm.is_precious)
						{
							stb2.AppendTxA("Item_dbox_stock", "\n").TxRpl(itm.stock);
						}
						if (!itm.is_precious || itm.price > 2)
						{
							stb2.AppendTxA("catalog_price_buy", "\n").Add((int)itm.getPrice(specificGrade));
							if (!itm.is_precious)
							{
								stb2.Add(X.ENG_MODE ? "\n" : " / ");
								stb2.AddTxA("catalog_price_sell", false).Add((int)(itm.getSellPrice(specificGrade) * 0.4f));
							}
						}
						this.DInfo.Txt(stb2);
						goto IL_02FC;
					}
				}
				if (fdr.FSmn != null)
				{
					flag = true;
					FDSummonerInfo fsmn = fdr.FSmn;
					using (STB stb3 = TX.PopBld(null, 0))
					{
						WholeMapItem wholeFor = this.M2D.WM.GetWholeFor(fsmn.SInfoMp, false);
						if (wholeFor != null)
						{
							stb3.Add(wholeFor.localized_name);
							stb3.Add(", ");
						}
						stb3.AddTxA("Catalog_enemyspot_dangerous", false).TxRpl(fsmn.Summoner.grade);
						this.DTitleKind.Txt(stb3);
						stb3.Clear();
						this.DTitle.text_content = fsmn.Summoner.name_localized;
						this.DInfo.text_content = "";
						goto IL_02FC;
					}
				}
				if (fdr.enemyid > (ENEMYID)0U)
				{
					NOD.BasicData basicData = NOD.getBasicData(fdr.enemyid_tostr);
					flag = true;
					if (basicData != null)
					{
						this.DTitle.text_content = NDAT.getEnemyName(fdr.enemyid, true);
						this.DInfo.text_content = "";
						this.DTitleKind.Txt(NDAT.getEnemyKindName(basicData.is_machine ? ENEMYKIND.MACHINE : ENEMYKIND.DEVIL));
					}
					else
					{
						this.fineDetailBasicInfoEmpty(out flag);
					}
				}
				else
				{
					flag = false;
				}
			}
			IL_02FC:
			if (flag != this.detail_showing_rpi_categ)
			{
				this.DTabLT.getDesignerBlockMemory(this.DTitleKind).active = (this.detail_showing_rpi_categ = flag);
				this.DTitle.heightPixel = 84f - (flag ? 20f : 0f) - this.DTabLT.margin_in_tb * 2f;
				this.DTitle.aligny = (flag ? ALIGNY.BOTTOM : ALIGNY.MIDDLE);
				this.DTitle.fineTxTransform();
				this.DTabLT.RowRemakeHeightRecalc(this.DTitle, null);
			}
		}

		public bool fnDtDrawIcon(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (!this.PreTopicRow.valid)
			{
				return false;
			}
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(0, false, false);
				Md.setMaterial(MTRX.MtrMeshNormal, false);
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTRX.MIicon.getMtr(-1), false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
			bool flag = false;
			if (fdr.valid && fdr.Itm != null)
			{
				fdr.Itm.drawIconTo(Md, null, 0, 1, 0f, 0f, 2f, 1f, null);
				flag = fdr.Itm.fd_favorite;
			}
			if (flag)
			{
				Md.chooseSubMesh(1, false, false);
				Md.Col = C32.MulA(uint.MaxValue, alpha);
				Md.RotaPF(22f, -22f, 1f, 1f, 0f, MTRX.getPF("favorite_star"), false, false, false, uint.MaxValue, false, 0);
			}
			update_meshdrawer = true;
			return true;
		}

		public bool fnDtDrawGrade(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (!this.PreTopicRow.valid)
			{
				return false;
			}
			NelItem itm = this.PreTopicRow.getFDR().Itm;
			Md.clear(false, false);
			if (itm != null)
			{
				Md.ColGrd.Set(4283780170U);
				if (itm.individual_grade)
				{
					Md.Col = Md.ColGrd.mulA(0.3f * alpha).C;
					Md.RotaPF(0f, 0f, 1f, 1f, 0f, MTR.AItemGradeStars[4], false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					Md.Col = Md.ColGrd.mulA(alpha).C;
					Md.RotaPF(0f, 0f, 1f, 1f, 0f, MTR.AItemGradeStars[this.target_grade], false, false, false, uint.MaxValue, false, 0);
				}
			}
			update_meshdrawer = true;
			return true;
		}

		private void fineTopInfoKDVisible()
		{
			if (!this.PreTopicRow.valid)
			{
				return;
			}
			UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
			if (this.isTopic() || fdr.individual_grade || this.DetailSelectMain != null)
			{
				(this.DTabInfo.Get("tabinfo_KD_L", false) as FillBlock).gameObject.SetActive(false);
				(this.DTabInfo.Get("tabinfo_KD_R", false) as FillBlock).gameObject.SetActive(false);
			}
			else
			{
				(this.DTabInfo.Get("tabinfo_KD_L", false) as FillBlock).gameObject.SetActive(true);
				(this.DTabInfo.Get("tabinfo_KD_R", false) as FillBlock).gameObject.SetActive(true);
			}
			this.DGrade.redraw_flag = true;
			this.fine_map_kd = true;
		}

		private void fineMapKD()
		{
			if (this.WmCtr.fineMapKD(this.DMapKD, false, true))
			{
				this.fine_map_kd = false;
			}
		}

		private void fineDetailDesc()
		{
			UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
			this.DTabDesc.EvacuateMemory(this.AEvcMem, null, true);
			this.DTabDesc.Clear();
			this.item_value_refining = -1;
			this.ARbSk.Clear();
			this.WmCtr.clearTarget();
			if (fdr.Itm != null && fdr.valid)
			{
				NelItem itm = fdr.Itm;
				int num = (itm.individual_grade ? this.getSpecificGrade(itm, false) : this.target_grade);
				if (itm.useable && !itm.is_bomb)
				{
					this.PopDescPTx("CatalogI_tab_info", 29, null);
					itm.Use(null, null, num, this);
				}
				if (itm.RecipeInfo != null && itm.RecipeInfo.Oeffect100.Count > 0)
				{
					Dictionary<RCP.RPI_EFFECT, float> oeffect = itm.RecipeInfo.Oeffect100;
					this.PopDescPTx("Item_for_food_effect", 29, null);
					foreach (KeyValuePair<RCP.RPI_EFFECT, float> keyValuePair in oeffect)
					{
						ButtonSkinNelFieldGuide buttonSkinNelFieldGuide;
						this.PopDescRB(out buttonSkinNelFieldGuide, 48f);
						buttonSkinNelFieldGuide.initRpiEffect(itm, keyValuePair.Key, keyValuePair.Value, num);
					}
				}
				using (BList<RCP.Recipe> blist = ListBuffer<RCP.Recipe>.Pop(0))
				{
					if (itm.is_recipe || itm.is_workbench_craft || itm.is_trm_episode)
					{
						RCP.Recipe recipeAllType = UiCraftBase.getRecipeAllType(itm);
						if (recipeAllType != null)
						{
							blist.Add(recipeAllType);
							if (recipeAllType.Completion != null && (recipeAllType.categ == RCP.RP_CATEG.ALCHEMY || recipeAllType.categ == RCP.RP_CATEG.COOK))
							{
								this.PopDescPTx("CatalogI_recipe_completion", 29, null);
								ButtonSkinNelFieldGuide buttonSkinNelFieldGuide2;
								this.PopDescRB(out buttonSkinNelFieldGuide2, 30f);
								buttonSkinNelFieldGuide2.initItem(recipeAllType.Completion, recipeAllType.create_count, -1, true, null);
							}
						}
					}
					else
					{
						RCP.listupRecipeForCompletion(itm, blist);
					}
					if (blist.Count > 0)
					{
						int count = blist.Count;
						for (int i = 0; i < count; i++)
						{
							RCP.Recipe recipe = blist[i];
							this.PopDescPTx("CatalogI_ingredient", 22, null);
							int count2 = recipe.AIng.Count;
							for (int j = 0; j < count2; j++)
							{
								RCP.RecipeIngredient recipeIngredient = recipe.AIng[j];
								ButtonSkinNelFieldGuide buttonSkinNelFieldGuide3;
								this.PopDescRB(out buttonSkinNelFieldGuide3, 30f);
								buttonSkinNelFieldGuide3.initRecipeIngredient(recipeIngredient);
							}
						}
					}
				}
				this.detail_store_activated = false;
				if (itm.is_reelmbox)
				{
					ReelManager.ItemReelContainer ir = ReelManager.GetIR(itm);
					if (ir != null)
					{
						this.WmCtr.DetailBaseIR = ir;
						this.PopDescPTx("Inventory_reel_content", 56, null);
						int count3 = ir.Count;
						for (int k = 0; k < count3; k++)
						{
							NelItemEntry nelItemEntry = ir[k];
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide4;
							this.PopDescRB(out buttonSkinNelFieldGuide4, 30f);
							buttonSkinNelFieldGuide4.initItem(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade, true, null);
						}
					}
					bool flag = false;
					using (Dictionary<string, FDSummonerInfo>.Enumerator enumerator2 = this.OSummonerInfo.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							KeyValuePair<string, FDSummonerInfo> keyValuePair2 = enumerator2.Current;
							if (keyValuePair2.Value.SupLink.isContains(ir))
							{
								if (!flag)
								{
									flag = true;
									this.PopDescPTx("CatalogI_tab_enemyspot", 9, null);
								}
								ButtonSkinNelFieldGuide buttonSkinNelFieldGuide5;
								this.PopDescRB(out buttonSkinNelFieldGuide5, 30f);
								buttonSkinNelFieldGuide5.initSummonerInfo(this.M2D, keyValuePair2.Value);
							}
						}
						goto IL_062F;
					}
				}
				List<RCP.RecipeDescription>[] array;
				if (!this.OAAItmRecipeDefinition.TryGetValue(itm, out array))
				{
					array = (this.OAAItmRecipeDefinition[itm] = RCP.listupDefinitionRecipe(itm, !this.ignore_obtain_count));
				}
				if (array != null)
				{
					if (array[0] != null)
					{
						this.PopDescPTx("CatalogI_tab_usedrecipe", 28, null);
						int count4 = array[0].Count;
						for (int l = 0; l < count4; l++)
						{
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide6;
							this.PopDescRB(out buttonSkinNelFieldGuide6, 30f);
							RCP.RecipeDescription recipeDescription = array[0][l];
							buttonSkinNelFieldGuide6.initItem(recipeDescription.Item, -1, -1, this.ignore_obtain_count, recipeDescription.title);
						}
					}
					if (array[1] != null)
					{
						this.PopDescPTx("CatalogI_tab_recipe", 22, null);
						int count5 = array[1].Count;
						for (int m = 0; m < count5; m++)
						{
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide7;
							this.PopDescRB(out buttonSkinNelFieldGuide7, 30f);
							RCP.RecipeDescription recipeDescription2 = array[1][m];
							buttonSkinNelFieldGuide7.initItem(recipeDescription2.Item, -1, -1, this.ignore_obtain_count, recipeDescription2.title);
						}
					}
				}
				if (!itm.is_precious)
				{
					this.WmCtr.PickupTarget = itm;
					ReelManager.ReelDecription reelDescriptionForCurrentItem = this.WmCtr.getReelDescriptionForCurrentItem();
					if (reelDescriptionForCurrentItem.AIR_useable != null)
					{
						this.PopDescPTx("CatalogI_tab_chest", 29, null);
						int count6 = reelDescriptionForCurrentItem.AIR_useable.Count;
						for (int n = 0; n < count6; n++)
						{
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide8;
							this.PopDescRB(out buttonSkinNelFieldGuide8, 30f);
							buttonSkinNelFieldGuide8.initTreasure(reelDescriptionForCurrentItem.AIR_useable[n], this.ignore_obtain_count, itm);
						}
					}
				}
				if (this.enabled_enemy_info && itm.RecipeInfo != null && (itm.RecipeInfo.categ & RCP.RPI_CATEG.ENEMY) != (RCP.RPI_CATEG)0)
				{
					List<ENEMYID> list;
					if (!this.OAItm2EnemyID.TryGetValue(itm, out list))
					{
						list = (this.OAItm2EnemyID[itm] = NOD.listupEnemyForDropItem(itm));
					}
					int count7 = list.Count;
					if (count7 > 0)
					{
						this.PopDescPTx("CatalogI_drop_enemy", 29, null);
						for (int num2 = 0; num2 < count7; num2++)
						{
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide9;
							this.PopDescRB(out buttonSkinNelFieldGuide9, 30f);
							UiFieldGuide.FDR fdrfor = this.getFDRFor(list[num2]);
							buttonSkinNelFieldGuide9.initEnemy(fdrfor.valid ? list[num2] : ((ENEMYID)0U));
						}
					}
				}
				this.detail_store_activated = false;
				foreach (KeyValuePair<StoreManager, List<NelItem>> keyValuePair3 in this.OAStoreItemListedUp)
				{
					if (keyValuePair3.Value.IndexOf(itm) >= 0)
					{
						if (!this.detail_store_activated)
						{
							this.detail_store_activated = true;
							this.PopDescPTx("CatalogI_tab_shopping", 29, null);
						}
						ButtonSkinNelFieldGuide buttonSkinNelFieldGuide10;
						this.PopDescRB(out buttonSkinNelFieldGuide10, 44f);
						buttonSkinNelFieldGuide10.initStoreManager(itm, keyValuePair3.Key, num);
					}
				}
				IL_062F:
				using (STB stb = TX.PopBld(null, 0))
				{
					itm.getDescLocalized(stb, null, 0);
					if (stb.Length > 0)
					{
						this.PopDescHr();
						FillBlock fillBlock = this.PopDescP();
						fillBlock.margin_x = 40f;
						fillBlock.margin_y = 20f;
						fillBlock.size = 12f;
						fillBlock.Txt(stb);
						this.DTabDesc.RowRemakeHeightRecalc(fillBlock, null);
					}
				}
			}
			if (fdr.FSmn != null && fdr.valid && this.M2D != null)
			{
				this.WmCtr.PickupTarget = fdr.FSmn;
				this.PopDescPTx("CatalogI_tab_address", 35, null);
				FDSummonerInfo fsmn = fdr.FSmn;
				int num3 = fsmn.AMp.Count;
				for (int num4 = 0; num4 < num3; num4++)
				{
					ButtonSkinNelFieldGuide buttonSkinNelFieldGuide11;
					this.PopDescRB(out buttonSkinNelFieldGuide11, 30f);
					buttonSkinNelFieldGuide11.initMapAddress(this.M2D, fsmn.AMp[num4]);
				}
				if (fsmn.SupLink.valid)
				{
					this.PopDescPTx("CatalogI_tab_chest_from_here", 56, null);
					if (fsmn.SupLink.AReel != null)
					{
						num3 = fsmn.SupLink.AReel.Length;
						for (int num5 = 0; num5 < num3; num5++)
						{
							ReelManager.ItemReelContainer itemReelContainer = fsmn.SupLink.AReel[num5];
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide12;
							this.PopDescRB(out buttonSkinNelFieldGuide12, 30f);
							buttonSkinNelFieldGuide12.initTreasure(itemReelContainer, true, null);
						}
					}
					ReelManager.ItemReelContainer[] array2 = fsmn.SupLink.AReelSecret;
					if (array2 == null && fsmn.Summoner != null && fsmn.Summoner.getAppearEnemyOdAbout() > 0)
					{
						array2 = SupplyManager.getSecretDefault(fsmn.summoner_key);
					}
					if (array2 != null)
					{
						num3 = array2.Length;
						for (int num6 = 0; num6 < num3; num6++)
						{
							ReelManager.ItemReelContainer itemReelContainer2 = array2[num6];
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide13;
							this.PopDescRB(out buttonSkinNelFieldGuide13, 30f);
							buttonSkinNelFieldGuide13.initTreasure(itemReelContainer2, false, null);
						}
					}
				}
				if (fsmn.Summoner != null && this.enabled_enemy_info)
				{
					using (BList<int> blist2 = ListBuffer<int>.Pop(0))
					{
						fsmn.Summoner.listupAboutEnemyForFG(blist2);
						num3 = blist2.Count;
						this.PopDescPTx("Smnc_title_enemy", 74, null);
						for (int num7 = 0; num7 < num3; num7++)
						{
							ENEMYID enemyid = (ENEMYID)blist2[num7];
							UiFieldGuide.FDR fdrfor2 = this.getFDRFor(enemyid);
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide14;
							this.PopDescRB(out buttonSkinNelFieldGuide14, 30f);
							buttonSkinNelFieldGuide14.initEnemy(fdrfor2.valid ? fdrfor2.enemyid : ((ENEMYID)0U));
						}
					}
				}
			}
			if (fdr.enemyid > (ENEMYID)0U && fdr.valid && this.M2D != null && this.enabled_enemy_info)
			{
				bool flag2 = (fdr.enemyid & (ENEMYID)2147483648U) > (ENEMYID)0U;
				this.WmCtr.PickupTarget = fdr.enemyid;
				this.OABufAppearEn.Clear();
				EnemySummonerManager.getAppearListForWholeMaps(this.M2D, fdr.enemyid, this.OABufAppearEn);
				foreach (KeyValuePair<WholeMapItem, SummonerList> keyValuePair4 in this.OABufAppearEn)
				{
					SummonerList value = keyValuePair4.Value;
					int count8 = value.Count;
					if (count8 != 0)
					{
						this.PopDescPTx("CatalogI_tab_appear_enemyspot", 74, keyValuePair4.Key.localized_name);
						for (int num8 = 0; num8 < count8; num8++)
						{
							ButtonSkinNelFieldGuide buttonSkinNelFieldGuide15;
							this.PopDescRB(out buttonSkinNelFieldGuide15, 30f);
							buttonSkinNelFieldGuide15.initSummonerInfo(this.M2D, X.Get<string, FDSummonerInfo>(this.OSummonerInfo, value[num8]));
						}
					}
				}
				NOD.BasicData basicData = NOD.getBasicData(fdr.enemyid);
				if (basicData != null)
				{
					using (BList<int> blist3 = ListBuffer<int>.Pop(0))
					{
						int num9 = ((basicData.Adrop_family != null) ? basicData.Adrop_family.Count : 0);
						for (int num10 = ((basicData.Adrop_family != null) ? 0 : (-1)); num10 < num9; num10++)
						{
							NOD.BasicData basicData2;
							if (num10 < 0)
							{
								basicData2 = basicData;
							}
							else
							{
								basicData2 = NOD.getBasicData(basicData.Adrop_family[num10]);
							}
							if (basicData2 != null)
							{
								NelItem nelItem = (flag2 ? basicData2.DropItemOd : basicData2.DropItemNormal);
								if (nelItem != null && X.pushIdentical<int>(blist3, (int)nelItem.id))
								{
									if (blist3.Count == 1)
									{
										this.PopDescPTx("CatalogN_dropitem", 41, null);
									}
									ButtonSkinNelFieldGuide buttonSkinNelFieldGuide16;
									this.PopDescRB(out buttonSkinNelFieldGuide16, 30f);
									buttonSkinNelFieldGuide16.initItem(nelItem, 1, -1, true, null);
								}
							}
						}
					}
				}
				string text = FEnum<ENEMYID>.ToStr(fdr.enemyid & (ENEMYID)2147483647U);
				TX tx = TX.getTX("Enemy_description_" + text + (flag2 ? "_OD" : ""), true, true, null);
				if (tx == null && REG.match(text, REG.RegSuffixNumber))
				{
					tx = TX.getTX("Enemy_description_" + REG.leftContext + (flag2 ? "_OD" : ""), true, true, null);
				}
				if (tx != null)
				{
					this.PopDescHr();
					FillBlock fillBlock2 = this.PopDescP();
					fillBlock2.margin_x = 40f;
					fillBlock2.margin_y = 20f;
					fillBlock2.size = 12f;
					fillBlock2.Txt(tx.text);
					this.DTabDesc.RowRemakeHeightRecalc(fillBlock2, null);
				}
			}
			this.WmCtr.DetailCurIR = this.WmCtr.DetailBaseIR;
		}

		private FillBlock PopDescPTx(string tx_key, int icon = 29, string txa0 = null)
		{
			FillBlock fillBlock = this.PopDescP();
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add("<img mesh=\"itemrow_category.", icon, "\" width=\"36\" tx_color />");
				stb.AddTxA(tx_key, false);
				if (txa0 != null)
				{
					stb.TxRpl(txa0);
				}
				stb.RemoveChar('\n', 0, -1);
				fillBlock.Txt(stb);
				this.DTabDesc.RowRemakeHeightRecalc(fillBlock, null);
			}
			return fillBlock;
		}

		private FillBlock PopDescP()
		{
			int count = this.AEvcMem.Count;
			for (int i = 0; i < count; i++)
			{
				Designer.EvacuateMem evacuateMem = this.AEvcMem[i];
				if (evacuateMem.Blk is FillBlock)
				{
					this.AEvcMem.RemoveAt(i);
					FillBlock fillBlock = evacuateMem.Blk as FillBlock;
					fillBlock.size = this.DsnDetailP.size;
					fillBlock.margin_x = this.DsnDetailP.text_margin_x;
					fillBlock.margin_y = this.DsnDetailP.text_margin_y;
					fillBlock.Txt("");
					this.DTabDesc.ReassignEvacuatedMemory(evacuateMem);
					this.DTabDesc.Br();
					return fillBlock;
				}
			}
			FillBlock fillBlock2 = this.DTabDesc.addP(this.DsnDetailP, false);
			fillBlock2.alloc_extending = true;
			this.DTabDesc.Br();
			return fillBlock2;
		}

		private aBtn PopDescRB(out ButtonSkinNelFieldGuide Sk, float btn_height = 30f)
		{
			aBtn aBtn = null;
			Sk = null;
			int count = this.AEvcMem.Count;
			for (int i = 0; i < count; i++)
			{
				Designer.EvacuateMem evacuateMem = this.AEvcMem[i];
				if (evacuateMem.Blk is aBtnNel)
				{
					aBtn aBtn2 = evacuateMem.Blk as aBtnNel;
					if (aBtn2.get_Skin() is ButtonSkinNelFieldGuide)
					{
						Sk = aBtn2.get_Skin() as ButtonSkinNelFieldGuide;
						this.AEvcMem.RemoveAt(i);
						this.DTabDesc.addEvacuatedButton(aBtn2, "", false, true);
						aBtn = aBtn2;
						break;
					}
				}
			}
			if (aBtn == null)
			{
				aBtn = this.DTabDesc.addButtonT<aBtnNel>(this.DsnDetailRB);
				Sk = aBtn.get_Skin() as ButtonSkinNelFieldGuide;
				Sk.row_left_px = 30;
			}
			if (this.ARbSk.Count > 0)
			{
				aBtn btn = this.ARbSk[this.ARbSk.Count - 1].getBtn();
				btn.setNaviB(aBtn, true, true);
				aBtn.CopyFunctionFrom(btn);
			}
			this.ARbSk.Add(Sk);
			Sk.clearVisual(this);
			if (aBtn.h != btn_height)
			{
				aBtn.WH(aBtn.w, btn_height);
				this.DTabDesc.RowRemakeHeightRecalc(aBtn, null);
			}
			this.DTabDesc.Br();
			return aBtn;
		}

		private aBtn PopDescRBFineUsing(out ButtonSkinNelFieldGuide Sk)
		{
			if (this.item_value_refining < 0)
			{
				return this.PopDescRB(out Sk, 30f);
			}
			BtnContainer<aBtn> btnContainer = this.DTabDesc.getBtnContainer();
			int length = btnContainer.Length;
			if (this.item_value_refining < length)
			{
				BtnContainer<aBtn> btnContainer2 = btnContainer;
				int num = this.item_value_refining;
				this.item_value_refining = num + 1;
				aBtn aBtn = btnContainer2.Get(num);
				Sk = aBtn.get_Skin() as ButtonSkinNelFieldGuide;
				return aBtn;
			}
			return this.PopDescRB(out Sk, 30f);
		}

		private DesignerHr PopDescHr()
		{
			int count = this.AEvcMem.Count;
			for (int i = 0; i < count; i++)
			{
				Designer.EvacuateMem evacuateMem = this.AEvcMem[i];
				if (evacuateMem.Blk is DesignerHr)
				{
					this.AEvcMem.RemoveAt(i);
					this.DTabDesc.Br();
					this.DTabDesc.ReassignEvacuatedMemory(evacuateMem);
					this.DTabDesc.Br();
					return evacuateMem.Blk as DesignerHr;
				}
			}
			return this.DTabDesc.addHr(this.DsnDetailHr);
		}

		public bool SelectDescRBByIndex(int index = 0)
		{
			if (X.BTW(0f, (float)index, (float)this.ARbSk.Count))
			{
				this.ARbSk[index].getBtn().Select(true);
				return true;
			}
			return false;
		}

		public int getDescRBIndex(aBtn B)
		{
			ButtonSkinNelFieldGuide buttonSkinNelFieldGuide = (B as aBtnNel).get_Skin() as ButtonSkinNelFieldGuide;
			if (buttonSkinNelFieldGuide == null)
			{
				return -1;
			}
			return this.ARbSk.IndexOf(buttonSkinNelFieldGuide);
		}

		public void NelItemUseInt(NelItem Itm, NelItem.CATEG categ, int grade, int v_int, ref int ef_delay, ref string play_snd, ref bool quit_flag)
		{
			ButtonSkinNelFieldGuide buttonSkinNelFieldGuide;
			this.PopDescRBFineUsing(out buttonSkinNelFieldGuide);
			using (STB stb = TX.PopBld(null, 0))
			{
				if (categ <= NelItem.CATEG.CURE_MP)
				{
					if (categ != NelItem.CATEG.CURE_HP)
					{
						if (categ == NelItem.CATEG.CURE_MP)
						{
							if (v_int >= 0)
							{
								buttonSkinNelFieldGuide.setTitleTextS(stb.AddTxA("Item_detail_cure_mp", false).TxRpl(v_int));
							}
							else
							{
								buttonSkinNelFieldGuide.setTitleTextS(stb.Add(NEL.error_tag).AddTxA("Catalog_categ_sub_reduce_mp", false).Add(" ", -v_int, "")
									.Add(NEL.error_tag_close));
							}
						}
					}
					else if (v_int >= 0)
					{
						buttonSkinNelFieldGuide.setTitleTextS(stb.AddTxA("Item_detail_cure_hp", false).TxRpl(v_int));
					}
					else
					{
						buttonSkinNelFieldGuide.setTitleTextS(stb.Add(NEL.error_tag).AddTxA("Catalog_categ_sub_reduce_hp", false).Add(" ", -v_int, "")
							.Add(NEL.error_tag_close));
					}
				}
				else if (categ != NelItem.CATEG.CURE_EP)
				{
					if (categ == NelItem.CATEG.CURE_MP_CRACK)
					{
						buttonSkinNelFieldGuide.setTitleTextS(stb.AddTxA("Item_detail_cure_mp_crack", false).TxRpl(v_int));
					}
				}
				else if (v_int >= 0)
				{
					buttonSkinNelFieldGuide.setTitleTextS(stb.AddTxA("Item_detail_cure_ep", false).TxRpl(v_int));
				}
				else
				{
					buttonSkinNelFieldGuide.setTitleTextS(stb.AddTxA("Item_detail_dmg_ep", false).TxRpl(-v_int));
				}
			}
		}

		public void NelItemUseUint(NelItem Itm, NelItem.CATEG categ, int grade, ulong v_uint, ref int ef_delay, ref string play_snd, int val)
		{
			ButtonSkinNelFieldGuide buttonSkinNelFieldGuide;
			this.PopDescRBFineUsing(out buttonSkinNelFieldGuide);
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					if (categ != NelItem.CATEG.SER_APPLY)
					{
						if (categ == NelItem.CATEG.SER_CURE)
						{
							M2Ser.listupAllTitle(v_uint, stb2);
							if (stb2.Length != 0)
							{
								stb.AddTxA("Item_detail_cure_ser", false).TxRpl(stb2);
							}
							buttonSkinNelFieldGuide.setTitleTextS(stb);
						}
					}
					else
					{
						M2Ser.listupAllTitle(v_uint, stb2);
						if (stb2.Length != 0)
						{
							stb.AddTxA("Item_detail_dmg_ser", false).TxRpl(stb2);
						}
						buttonSkinNelFieldGuide.setTitleTextS(stb);
					}
				}
			}
		}

		private bool fnHoverDetailRow(aBtn B)
		{
			if (!this.isDetail())
			{
				return false;
			}
			this.quitDetailMapEdit();
			ButtonSkinNelFieldGuide buttonSkinNelFieldGuide = B.get_Skin() as ButtonSkinNelFieldGuide;
			if (buttonSkinNelFieldGuide == null)
			{
				return false;
			}
			this.WmCtr.fineCurCR(buttonSkinNelFieldGuide.IR);
			this.fine_current_detail_scroll = true;
			int descRBIndex = this.getDescRBIndex(B);
			if (descRBIndex >= 0)
			{
				UiFieldGuide.FDR fdr = this.PreTopicRow.getFDR();
				UiFieldGuide.FdrEntry fdrEntry = X.Get<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>(this.OAItm2Row, fdr);
				if (fdrEntry != null)
				{
					fdrEntry.last_selected = (byte)descRBIndex;
				}
				buttonSkinNelFieldGuide.triggerHover(this.WmCtr);
				if (descRBIndex == this.ARbSk.Count - 1)
				{
					ScrollBox scrollBox = this.DTabDesc.getScrollBox();
					if (scrollBox != null && scrollBox.isActiveAndEnabled)
					{
						scrollBox.setScrollLevelTo(0f, 1f, true);
					}
				}
			}
			return true;
		}

		private bool fnClickDetailRow(aBtn B)
		{
			if (!this.isDetail())
			{
				return false;
			}
			ButtonSkinNelFieldGuide buttonSkinNelFieldGuide = B.get_Skin() as ButtonSkinNelFieldGuide;
			if (buttonSkinNelFieldGuide == null)
			{
				return false;
			}
			if (buttonSkinNelFieldGuide.Itm != null && buttonSkinNelFieldGuide.Itm != NelItem.Unknown)
			{
				UiFieldGuide.FDR fdrfor = this.getFDRFor(buttonSkinNelFieldGuide.Itm);
				if (fdrfor.valid)
				{
					this.addDetailHistory(fdrfor, default(UiFieldGuide.FDR));
					if (!this.topicJump(fdrfor))
					{
						CURS.limitVib(B, AIM.R);
						SND.Ui.play("toggle_button_limit", false);
						return false;
					}
				}
				return true;
			}
			if (buttonSkinNelFieldGuide.FSmn != null)
			{
				UiFieldGuide.FDR fdrfor2 = this.getFDRFor(buttonSkinNelFieldGuide.FSmn);
				if (fdrfor2.valid && buttonSkinNelFieldGuide.FSmn.valid)
				{
					this.addDetailHistory(fdrfor2, default(UiFieldGuide.FDR));
					if (!this.topicJump(fdrfor2))
					{
						CURS.limitVib(B, AIM.R);
						SND.Ui.play("toggle_button_limit", false);
						return false;
					}
				}
				return true;
			}
			if (buttonSkinNelFieldGuide.enid > (ENEMYID)0U)
			{
				UiFieldGuide.FDR fdrfor3 = this.getFDRFor(buttonSkinNelFieldGuide.enid);
				if (fdrfor3.valid)
				{
					this.addDetailHistory(fdrfor3, default(UiFieldGuide.FDR));
					if (!this.topicJump(fdrfor3))
					{
						CURS.limitVib(B, AIM.R);
						SND.Ui.play("toggle_button_limit", false);
						return false;
					}
				}
				return true;
			}
			aBtnFDRow b = this.PreTopicRow.B;
			if (buttonSkinNelFieldGuide.rpi_ef != RCP.RPI_EFFECT.NONE)
			{
				this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
				this.CurPickUp = new UiFieldGuide.PickUp(buttonSkinNelFieldGuide.rpi_ef);
				this.BDetailPre = b;
				this.finePickUp();
				return true;
			}
			if (buttonSkinNelFieldGuide.RecipeIng != null)
			{
				if (buttonSkinNelFieldGuide.RecipeIng.target_category != (RCP.RPI_CATEG)0)
				{
					this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
					this.CurPickUp = new UiFieldGuide.PickUp(buttonSkinNelFieldGuide.RecipeIng.target_category);
					this.BDetailPre = b;
					this.finePickUp();
					return true;
				}
				if (buttonSkinNelFieldGuide.RecipeIng.target_ni_category != NelItem.CATEG.OTHER && (buttonSkinNelFieldGuide.RecipeIng.target_ni_category & NelItem.CATEG.TOOL) != NelItem.CATEG.OTHER)
				{
					this.clearPickup();
					this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
					aBtn aBtn;
					UiFieldGuide.CategHead categHead = this.getCategHead(UiItemWholeSelector.WCATEG.TOOL, out aBtn);
					if (categHead.valid && categHead.BR != null)
					{
						categHead.BR.Select(true);
						this.fine_current_topic_scroll = true;
					}
					return true;
				}
			}
			if (buttonSkinNelFieldGuide.Store != null)
			{
				this.changeState(UiFieldGuide.STATE.ITEM_TOPIC);
				this.CurPickUp = new UiFieldGuide.PickUp(buttonSkinNelFieldGuide.Store);
				this.BDetailPre = b;
				this.finePickUp();
				return true;
			}
			if (buttonSkinNelFieldGuide.TargetMp != null && buttonSkinNelFieldGuide.TargetWM != null)
			{
				this.WmSkin.reveal(new WmDeperture(buttonSkinNelFieldGuide.TargetWM.text_key, buttonSkinNelFieldGuide.TargetMp.key), true, false);
				SND.Ui.play("cursor_gear_reset", false);
				return true;
			}
			SND.Ui.play("toggle_button_limit", false);
			CURS.limitVib(B, AIM.R);
			return false;
		}

		private void addDetailHistory(UiFieldGuide.FDR Fdr, UiFieldGuide.FDR PreFdr = default(UiFieldGuide.FDR))
		{
			PreFdr = (PreFdr.valid ? PreFdr : this.PreTopicRow.getFDR());
			if (this.ADetailHistory.Count == 0)
			{
				if (Fdr.Equals(PreFdr))
				{
					return;
				}
				this.ADetailHistory.Add(PreFdr);
			}
			else
			{
				int num = this.ADetailHistory.IndexOf(PreFdr);
				if (num >= 0)
				{
					this.ADetailHistory.RemoveRange(num + 1, this.ADetailHistory.Count - 1 - num);
				}
			}
			this.ADetailHistory.Remove(Fdr);
			this.ADetailHistory.Add(Fdr);
			this.need_recreate_detail_dpage = true;
		}

		public int getSpecificGrade(NelItem I, bool is_topic_title = false)
		{
			if (I != null)
			{
				string key = I.key;
				if (key != null && (key == "precious_noel_shorts" || key == "precious_noel_cloth"))
				{
					return 4;
				}
				if (I.is_reelmbox)
				{
					return 0;
				}
			}
			if (!is_topic_title)
			{
				return 0;
			}
			return -1;
		}

		private UiItemWholeSelector.WCATEG getCateg(aBtn BR, out UiFieldGuide.FdrEntry AIc)
		{
			return this.getCategData(BR, out AIc).categ;
		}

		private UiFieldGuide.ItmAndCateg getCategData(aBtn BR, out UiFieldGuide.FdrEntry AIc)
		{
			aBtnFDRow aBtnFDRow = BR as aBtnFDRow;
			AIc = null;
			if (aBtnFDRow == null)
			{
				return default(UiFieldGuide.ItmAndCateg);
			}
			AIc = X.Get<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>(this.OAItm2Row, aBtnFDRow.getFDR());
			if (AIc == null || AIc.Count == 0)
			{
				return default(UiFieldGuide.ItmAndCateg);
			}
			int count = AIc.Count;
			for (int i = 0; i < count; i++)
			{
				UiFieldGuide.ItmAndCateg itmAndCateg = AIc[i];
				if (itmAndCateg.B == aBtnFDRow)
				{
					return itmAndCateg;
				}
			}
			return default(UiFieldGuide.ItmAndCateg);
		}

		private UiFieldGuide.ItmAndCateg getCategData(UiFieldGuide.FDR Fdr, out UiFieldGuide.FdrEntry AIc)
		{
			return this.getCategData(Fdr, this.PreTopicRow.valid ? this.PreTopicRow.categ : UiItemWholeSelector.WCATEG.ALL, out AIc);
		}

		private UiFieldGuide.ItmAndCateg getCategData(UiFieldGuide.FDR Fdr, UiItemWholeSelector.WCATEG categ, out UiFieldGuide.FdrEntry AIc)
		{
			AIc = null;
			if (!Fdr.valid)
			{
				return default(UiFieldGuide.ItmAndCateg);
			}
			AIc = X.Get<UiFieldGuide.FDR, UiFieldGuide.FdrEntry>(this.OAItm2Row, Fdr);
			if (AIc == null || AIc.Count == 0)
			{
				return default(UiFieldGuide.ItmAndCateg);
			}
			if (categ != UiItemWholeSelector.WCATEG.ALL)
			{
				int count = AIc.Count;
				for (int i = 0; i < count; i++)
				{
					UiFieldGuide.ItmAndCateg itmAndCateg = AIc[i];
					if (itmAndCateg.categ == categ)
					{
						return itmAndCateg;
					}
				}
			}
			return AIc[0];
		}

		public bool isRecipeEnabled(NelItem I, RCP.Recipe Rcp, bool basic_flag = true)
		{
			bool flag = basic_flag;
			if (this.M2D != null)
			{
				if (Rcp.created > 0U)
				{
					flag = true;
				}
				if (Rcp.categ == RCP.RP_CATEG.ALCHEMY_WORKBENCH)
				{
					flag = SCN.alchemy_workbench_enabled;
				}
				else if (Rcp.categ == RCP.RP_CATEG.ALOMA)
				{
					flag = TRMManager.isTrmActive(I, null);
					if (flag)
					{
						I.touchObtainCount();
					}
				}
				else
				{
					flag = flag || Rcp.eaten > 0;
				}
			}
			return flag;
		}

		public bool isRowAssigned(NelItem Itm)
		{
			if (Itm == NelItem.Unknown)
			{
				return false;
			}
			if (this.M2D == null)
			{
				return true;
			}
			UiFieldGuide.FDR fdr = new UiFieldGuide.FDR(Itm, this.getSpecificGrade(Itm, false));
			UiFieldGuide.FdrEntry fdrEntry;
			return this.OAItm2Row.TryGetValue(fdr, out fdrEntry);
		}

		public bool ignore_obtain_count
		{
			get
			{
				return this.M2D == null;
			}
		}

		public bool isTopic()
		{
			return this.state == UiFieldGuide.STATE.ITEM_TOPIC;
		}

		public bool isDetail()
		{
			return this.state == UiFieldGuide.STATE.DETAIL;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		public ButtonSkinWholeMapArea WmSkin
		{
			get
			{
				if (this.WmCtr == null)
				{
					return null;
				}
				return this.WmCtr.WmSkin;
			}
		}

		public static object NextRevealAtAwake;

		public const string item_key_recipe_book = "recipe_collection";

		public NelM2DBase M2D;

		private UiBoxDesigner BxL;

		private UiBoxDesigner BxT;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxD;

		private readonly float out_w = IN.w * 0.72f;

		private readonly float out_h = IN.h * 0.93f;

		private readonly float bxl_w = 148f;

		private readonly float bxl_x_margin = 25f;

		private readonly float top_h = 80f;

		private readonly float top_y_margin = 16f;

		private readonly float detail_marg_x = 60f;

		private readonly float detail_w = IN.w * 0.85f;

		private int total_items;

		private int valid_items;

		protected float first_base_z;

		private UiFieldGuide.STATE state = UiFieldGuide.STATE._NOUSE;

		private float state_t;

		private BDic<UiFieldGuide.FDR, UiFieldGuide.FdrEntry> OAItm2Row;

		private BDic<NelItem, List<RCP.RecipeDescription>[]> OAAItmRecipeDefinition;

		private BDic<StoreManager, List<NelItem>> OAStoreItemListedUp;

		private BDic<string, FDSummonerInfo> OSummonerInfo;

		private BDic<NelItem, List<ENEMYID>> OAItm2EnemyID;

		private BDic<WholeMapItem, SummonerList> OABufAppearEn;

		private BDic<aBtn, UiFieldGuide.CategHead> OCategHead;

		private BDic<UiItemWholeSelector.WCATEG, UiFieldGuide.CategHead> OCategHead0;

		private List<aBtn> ACtgHeader;

		private aBtnFDRow BDetailPre;

		private ColumnRow TopTab;

		private Designer DTopPickUp;

		private FillBlock FbPickUp;

		public bool enabled_enemy_info;

		private BtnContainerRadio<aBtn> BConL;

		private BtnContainerRadio<aBtn> BConR;

		private float t_add_push;

		private float left_category_selected;

		private bool subcateg_mask;

		private bool detail_store_activated;

		private UiFieldGuide.ItmAndCateg PreTopicRow;

		private List<UiFieldGuide.FDR> ADetailHistory;

		private bool need_recreate_detail;

		private bool need_recreate_detail_dpage;

		private bool fine_current_detail_scroll;

		private bool fine_current_topic_scroll;

		private bool fine_map_kd;

		private bool detail_showing_rpi_categ = true;

		private UiFieldGuide.PickUp CurPickUp;

		private UiItemWholeSelector.WCATEG pickup_wcateg_using;

		private UiItemWholeSelector.WCATEG wcateg_using0;

		private aBtn DetailSelectMain;

		private Designer DTabL;

		private Designer DTabLT;

		private FillBlock DPageL;

		private FillBlock DPageR;

		private Designer DTabInfo;

		private FillBlock DTitle;

		private FillBlock DTitleKind;

		private FillImageBlock DTitleIcon;

		private FillImageBlock DGrade;

		private FillBlock DInfo;

		private FillBlock DBottomHold;

		private FillBlock DBottomObtain;

		private FillBlock DMapKD;

		private Designer DTabDesc;

		private DsnDataP DsnDetailP;

		private DsnDataHr DsnDetailHr;

		private DsnDataButton DsnDetailRB;

		private FieldGuideWmController WmCtr;

		private const float detail_toph = 84f;

		private const float detail_top_kind = 20f;

		public const float desc_row_btnh = 30f;

		private List<Designer.EvacuateMem> AEvcMem = new List<Designer.EvacuateMem>(8);

		private int target_grade = -1;

		private List<ButtonSkinNelFieldGuide> ARbSk;

		private int item_value_refining;

		private enum STATE
		{
			_NOUSE = -1,
			FIRST,
			ITEM_TOPIC,
			DETAIL
		}

		public interface IFieldGuideOpenable
		{
			UiFieldGuide.FDR getFDR();
		}

		private struct ItmAndCateg
		{
			public ItmAndCateg(aBtnFDRow _B, UiItemWholeSelector.WCATEG _categ)
			{
				this.B = _B;
				this.categ = _categ;
			}

			public bool valid
			{
				get
				{
					return this.B != null;
				}
			}

			public ButtonSkinFDItemRow Sk
			{
				get
				{
					if (!(this.B != null))
					{
						return null;
					}
					return this.B.RowSkin;
				}
			}

			public bool valid_enabled
			{
				get
				{
					return this.B != null && this.B.isActive();
				}
			}

			public bool fd_favorite
			{
				get
				{
					return this.B != null && this.B.RowSkin.Fdr.fd_favorite;
				}
				set
				{
					if (this.B != null)
					{
						this.B.RowSkin.Fdr.fd_favorite = value;
					}
				}
			}

			public UiFieldGuide.FDR getFDR()
			{
				if (!(this.B != null))
				{
					return default(UiFieldGuide.FDR);
				}
				return this.B.getFDR();
			}

			public readonly UiItemWholeSelector.WCATEG categ;

			public readonly aBtnFDRow B;
		}

		public struct FDR
		{
			public FDR(NelItem _Itm, int _grade)
			{
				this.Itm = UiFieldGuide.Sanitize(_Itm);
				this.FSmn = null;
				this.enemyid_ = 0;
				this.enemyid_tostr = null;
				this.grade = _grade;
			}

			public FDR(FDSummonerInfo _FSmn)
			{
				this.FSmn = _FSmn;
				this.Itm = null;
				this.enemyid_ = 0;
				this.enemyid_tostr = null;
				this.grade = (this.FSmn.valid ? this.FSmn.defeat_count : (-1));
			}

			public FDR(ENEMYID _id, int defeat_count)
			{
				this.enemyid_tostr = (_id & (ENEMYID)2147483647U).ToString();
				this.FSmn = null;
				this.Itm = null;
				this.enemyid_ = (int)_id;
				this.grade = defeat_count;
			}

			public bool enemyid_available
			{
				get
				{
					return this.enemyid_tostr != null;
				}
			}

			public ENEMYID enemyid
			{
				get
				{
					return (ENEMYID)this.enemyid_;
				}
			}

			public string key
			{
				get
				{
					if (this.Itm != null)
					{
						return this.Itm.key;
					}
					if (this.FSmn != null)
					{
						return this.FSmn.summoner_key;
					}
					if (this.enemyid_tostr != null)
					{
						return this.enemyid_tostr;
					}
					return null;
				}
			}

			public bool valid
			{
				get
				{
					return ((this.Itm != null || this.FSmn != null) && this.grade >= 0) || (this.enemyid > (ENEMYID)0U && this.grade >= 0);
				}
			}

			public bool individual_grade
			{
				get
				{
					return this.Itm == null || this.Itm.individual_grade;
				}
			}

			public bool is_cache_item
			{
				get
				{
					return this.Itm != null && this.Itm.is_cache_item;
				}
			}

			public bool Equals(UiFieldGuide.FDR dest)
			{
				return this.Itm == dest.Itm && this.grade == dest.grade && this.FSmn == dest.FSmn && this.enemyid_ == dest.enemyid_;
			}

			public void copyNameTo(STB Stb)
			{
				if (!this.valid)
				{
					Stb.Add(NelItem.Unknown.getLocalizedName(0));
					return;
				}
				if (this.Itm != null)
				{
					Stb.Add(this.Itm.getLocalizedName(this.grade));
				}
				if (this.FSmn != null)
				{
					Stb.Add(this.FSmn.Summoner.name_localized);
				}
				if (this.enemyid_tostr != null)
				{
					Stb.Add(NDAT.getEnemyName(this.enemyid, true));
				}
			}

			public bool fd_favorite
			{
				get
				{
					if (this.Itm != null)
					{
						return this.Itm.fd_favorite;
					}
					if (this.FSmn != null)
					{
						return this.FSmn.fd_favorite;
					}
					return this.enemyid_tostr != null && UiEnemyDex.get_fd_favorite(this.enemyid);
				}
				set
				{
					if (this.Itm != null)
					{
						this.Itm.fd_favorite = value;
					}
					if (this.FSmn != null)
					{
						this.FSmn.fd_favorite = value;
					}
					if (this.enemyid_tostr != null)
					{
						UiEnemyDex.set_fd_favorite(this.enemyid, value);
					}
				}
			}

			public NelItem Itm;

			public FDSummonerInfo FSmn;

			public string enemyid_tostr;

			public int grade;

			private int enemyid_;
		}

		private struct PickUp
		{
			public PickUp(RCP.RPI_CATEG _rpc)
			{
				this.tx_title = "";
				this.rpc = _rpc;
				this.rpi = RCP.RPI_EFFECT.NONE;
				this.Reel = null;
				this.Store = null;
				this.itcateg = NelItem.CATEG.OTHER;
				this.ASmn = null;
				this.Aenemy_id = null;
			}

			public PickUp(NelItem.CATEG _itcateg)
			{
				this.tx_title = "";
				this.rpc = (RCP.RPI_CATEG)0;
				this.rpi = RCP.RPI_EFFECT.NONE;
				this.Reel = null;
				this.Store = null;
				this.itcateg = _itcateg;
				this.ASmn = null;
				this.Aenemy_id = null;
			}

			public PickUp(RCP.RPI_EFFECT _rpi)
			{
				this.tx_title = "&&recipe_effect_" + FEnum<RCP.RPI_EFFECT>.ToStr(_rpi).ToLower();
				this.rpc = (RCP.RPI_CATEG)0;
				this.rpi = _rpi;
				this.Reel = null;
				this.Store = null;
				this.itcateg = NelItem.CATEG.OTHER;
				this.ASmn = null;
				this.Aenemy_id = null;
			}

			public PickUp(ReelManager.ItemReelContainer _Reel, string _tx_title = "")
			{
				this.tx_title = _tx_title;
				this.rpc = (RCP.RPI_CATEG)0;
				this.rpi = RCP.RPI_EFFECT.NONE;
				this.Reel = _Reel;
				this.Store = null;
				this.itcateg = NelItem.CATEG.OTHER;
				this.ASmn = null;
				this.Aenemy_id = null;
			}

			public PickUp(StoreManager _Store)
			{
				this.tx_title = "";
				this.rpc = (RCP.RPI_CATEG)0;
				this.rpi = RCP.RPI_EFFECT.NONE;
				this.Reel = null;
				this.Store = _Store;
				this.itcateg = NelItem.CATEG.OTHER;
				this.ASmn = null;
				this.Aenemy_id = null;
			}

			public PickUp(SummonerList _ASmn, string _tx_title = "")
			{
				this.tx_title = _tx_title;
				this.rpc = (RCP.RPI_CATEG)0;
				this.rpi = RCP.RPI_EFFECT.NONE;
				this.Reel = null;
				this.Store = null;
				this.itcateg = NelItem.CATEG.OTHER;
				this.Aenemy_id = null;
				this.ASmn = _ASmn;
				if (this.ASmn.enemyid_fix > (ENEMYID)0U)
				{
					this.Aenemy_id = new List<ENEMYID>(1);
					this.Aenemy_id.Add(this.ASmn.enemyid_fix);
				}
			}

			public bool isPickedUpFor(UiFieldGuide.FDR Fdr)
			{
				NelItem itm = Fdr.Itm;
				if (itm != null)
				{
					if (itm.RecipeInfo != null)
					{
						if (this.rpc != (RCP.RPI_CATEG)0 && (itm.RecipeInfo.categ & this.rpc) != (RCP.RPI_CATEG)0)
						{
							return true;
						}
						if (this.rpi != RCP.RPI_EFFECT.NONE && itm.RecipeInfo.Oeffect100.ContainsKey(this.rpi))
						{
							return true;
						}
					}
					if (this.itcateg != NelItem.CATEG.OTHER)
					{
						return (itm.category & this.itcateg) > NelItem.CATEG.OTHER;
					}
					if (this.Reel != null && this.Reel.isin(itm) >= 0)
					{
						return true;
					}
					if (this.Store != null && this.Store.listUpWholeItem().IndexOf(itm) >= 0)
					{
						return true;
					}
				}
				if (Fdr.FSmn != null && Fdr.valid)
				{
					if (this.Reel != null && Fdr.FSmn.SupLink.isContains(this.Reel))
					{
						return true;
					}
					if (this.ASmn != null && this.ASmn.IndexOf(Fdr.FSmn.summoner_key) >= 0)
					{
						return true;
					}
				}
				return Fdr.enemyid > (ENEMYID)0U && this.Aenemy_id != null && this.Aenemy_id.IndexOf(Fdr.enemyid) >= 0;
			}

			public void AddTxDescription(STB Stb)
			{
				if (this.rpc != (RCP.RPI_CATEG)0)
				{
					RCP.RecipeIngredient.ingredientDescForRPCategTo(Stb, this.rpc, true);
				}
				if (this.itcateg != NelItem.CATEG.OTHER)
				{
					NelItem.addCategoryDescTo(Stb, this.itcateg);
				}
				if (this.Store != null)
				{
					this.Store.AddStoreTitleTo(Stb);
				}
				if (TX.valid(this.tx_title))
				{
					Stb.Add(TX.ReplaceTX(this.tx_title, false));
				}
			}

			public bool valid
			{
				get
				{
					return this.tx_title != null;
				}
			}

			public readonly string tx_title;

			public readonly RCP.RPI_CATEG rpc;

			public readonly RCP.RPI_EFFECT rpi;

			public readonly NelItem.CATEG itcateg;

			public readonly ReelManager.ItemReelContainer Reel;

			public readonly StoreManager Store;

			public readonly SummonerList ASmn;

			public readonly List<ENEMYID> Aenemy_id;
		}

		private class FdrEntry : List<UiFieldGuide.ItmAndCateg>
		{
			public FdrEntry(int capacity)
				: base(capacity)
			{
			}

			public byte last_selected;
		}

		private struct CategHead
		{
			public CategHead(UiItemWholeSelector.WCATEG _categ, aBtn _BHead, aBtnFDRow _BR)
			{
				this.categ = _categ;
				this.BHead = _BHead;
				this.BR = _BR;
				this.BR_PickUp = this.BR;
			}

			public bool valid
			{
				get
				{
					return this.BHead != null;
				}
			}

			public bool br_valid
			{
				get
				{
					return this.BR != null;
				}
			}

			public UiItemWholeSelector.WCATEG categ;

			public readonly aBtn BHead;

			public readonly aBtnFDRow BR;

			public aBtnFDRow BR_PickUp;
		}
	}
}
