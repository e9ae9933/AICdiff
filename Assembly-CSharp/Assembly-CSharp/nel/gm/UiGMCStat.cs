using System;
using m2d;
using UnityEngine;
using XX;

namespace nel.gm
{
	internal class UiGMCStat : UiGMC
	{
		internal UiGMCStat(UiGameMenu _GM)
			: base(_GM, CATEG.STAT, true, 0, 0, 0, 0, 1f, 1f)
		{
			this.AEvCon = new Designer.EvacuateContainer[4];
		}

		internal static void newGameStat()
		{
			UiGMCStat.status_tab = UiGMCStat.STATUS_CTG.CONDITION;
		}

		public override bool initAppearMain()
		{
			PR pr = UIPicture.getPr();
			UiGMCStat.StatMem statMem = new UiGMCStat.StatMem(pr);
			if (this.RTabBar != null && !this.StatusMem.isEqual(statMem))
			{
				if (!this.AEvCon[(int)UiGMCStat.status_tab].valid)
				{
					this.AEvCon[(int)UiGMCStat.status_tab] = new Designer.EvacuateContainer(this.Tab, false);
				}
				base.releaseEvac();
			}
			this.StatusMem = statMem;
			if (this.pre_ser_bits != pr.Ser.get_pre_bits())
			{
				this.pre_ser_bits = pr.Ser.get_pre_bits();
				this.need_remake |= 1UL;
			}
			if (!base.initAppearMain())
			{
				this.BxR.margin_in_lr = 30f;
				this.BxR.item_margin_x_px = 0f;
				this.BxR.init();
				float use_w = this.BxR.use_w;
				this.BxR.PC(TX.Get("Name_Noel", ""), 0f, false).Br();
				int level = pr.Ser.getLevel(SER.CONFUSE);
				if (pr == null)
				{
					this.BxR.PC("HP: " + 150.ToString() + "/" + 150.ToString(), use_w / 2f - 2f, false);
					this.BxR.PC("MP: " + 200.ToString() + "/" + 200.ToString(), use_w / 2f - 2f, false);
				}
				else if (level >= 1)
				{
					this.BxR.PC("HP: <i>???</i> / MP: <i>???</i>\n" + TX.Get("Stat_Condition_Confuse_hide_hp_mp", ""), 0f, true);
				}
				else
				{
					this.BxR.PC("HP: " + pr.get_hp().ToString() + "/" + pr.get_maxhp().ToString(), use_w / 2f - 2f, false);
					string text = "MP: " + pr.get_mp().ToString() + "/" + pr.get_maxmp().ToString();
					if (pr.EggCon.total > 0)
					{
						text = text + "<i><font color=\"0xff5B7456\">-" + pr.EggCon.total_real.ToString() + "</font></i>";
					}
					this.BxR.PC(text, use_w / 2f - 2f, true);
				}
				this.BxR.Hr(0.6f, 10f, 14f, 1f);
				this.RTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.BxR.Br(), "ctg_tab", "row_tab", (int)UiGMCStat.status_tab, FEnum<UiGMCStat.STATUS_CTG>.ToStrListUp(4, "&&Status_Tab_", true), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnStatusTabChanged), use_w, 0f, false, false);
				this.Tab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, true);
				this.Tab.use_scroll = true;
				this.BxR.endTab(false);
				this.initStatusTab();
			}
			else if ((this.need_remake & (1UL << (int)UiGMCStat.status_tab)) != 0UL)
			{
				this.initStatusTab();
			}
			return true;
		}

		public override void quitAppear()
		{
			base.quitAppear();
		}

		internal override void initEdit()
		{
			this.statusTabScrollSelect();
		}

		internal override void quitEdit()
		{
		}

		public void initStatusTab()
		{
			this.Tab.Clear();
			Stomach stmNoel = base.M2D.IMNG.StmNoel;
			this.Tab.use_scroll = UiGMCStat.status_tab != UiGMCStat.STATUS_CTG.FOOD || !stmNoel.eaten_anything;
			this.Tab.scroll_area_selectable = this.GM.isEditState();
			this.Tab.init();
			if (this.Tab.use_scroll)
			{
				this.GM.EditFocusInitTo = this.Tab.getScrollBox().BView;
			}
			this.Tab.effect_confusion = base.effect_confusion;
			if ((this.need_remake & (1UL << (int)UiGMCStat.status_tab)) != 0UL)
			{
				this.releaseEvac(ref this.AEvCon[(int)UiGMCStat.status_tab]);
			}
			if (base.reassignEvacuated(ref this.AEvCon[(int)UiGMCStat.status_tab], this.Tab))
			{
				if (UiGMCStat.status_tab == UiGMCStat.STATUS_CTG.FOOD)
				{
					this.FbLunchCost.redraw_flag = true;
				}
			}
			else
			{
				this.need_remake &= ~(1UL << (int)UiGMCStat.status_tab);
				switch (UiGMCStat.status_tab)
				{
				case UiGMCStat.STATUS_CTG.CONDITION:
					if (!SCN.addSpecialNoelCondition(this.Tab, this.Pr) && (this.Pr == null || this.Pr.Ser.Length == 0))
					{
						UiBoxDesigner.addPTo(this.Tab, " " + TX.Get("Stat_no_status_error", ""), ALIGN.LEFT, 0f, false, 0f, "", -1);
					}
					else
					{
						int num = this.Pr.Ser.Length;
						bool flag = false;
						if (num > 0)
						{
							using (STB stb = TX.PopBld(null, 0))
							{
								for (int i = 0; i < num; i++)
								{
									M2SerItem m2SerItem = this.Pr.Ser.Get(i);
									if (m2SerItem.isActive() && !TX.noe(m2SerItem.getTitle(false, false)))
									{
										Color32 color = C32.d2c(base.effect_confusion ? 4289571750U : 4283780170U);
										if (m2SerItem.id == SER.CONFUSE)
										{
											this.Tab.effect_confusion = false;
											color = C32.d2c(4283780170U);
										}
										UiBoxDesigner.addPTo(this.Tab, m2SerItem.getTitle(true, true), ALIGN.CENTER, 120f, true, 0f, "", -1);
										this.Tab.addP(new DsnDataP("", false)
										{
											Stb = m2SerItem.getDesc(stb.Clear()),
											alignx = ALIGN.LEFT,
											swidth = this.Tab.use_w - 10f,
											html = true,
											size = 14f,
											text_auto_wrap = true,
											Col = color
										}, false);
										this.Tab.Br();
										flag = true;
										if (m2SerItem.id == SER.CONFUSE)
										{
											this.Tab.effect_confusion = base.effect_confusion;
										}
									}
								}
							}
						}
						if (!flag)
						{
							UiBoxDesigner.addPTo(this.Tab, " " + TX.Get("Stat_no_status_error", ""), ALIGN.LEFT, 0f, false, 0f, "", -1);
						}
					}
					break;
				case UiGMCStat.STATUS_CTG.FOOD:
				{
					NelItem.fineNameLocalizedWhole();
					this.Tab.createTempMMRD(false);
					this.Tab.margin_in_tb = 4f;
					if (this.SMtrLunchCostMask == null)
					{
						this.SMtrLunchCostMask = MTRX.getMtr(BLEND.MASK, 230);
						this.SMtrLunchCost = MTRX.getMtr(BLEND.NORMALST, 230);
					}
					this.FbLunchCost = this.Tab.addImg(new DsnDataImg
					{
						swidth = this.Tab.use_w,
						sheight = UiLunchTimeBase.costbx_h + 12f,
						FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.FnDrawLunchCost)
					});
					this.Tab.Br();
					Color32 color = C32.d2c(4283780170U);
					Stomach.DishInStomach[] eatenDishArray = stmNoel.getEatenDishArray();
					int num2 = eatenDishArray.Length;
					if (num2 > 0)
					{
						Designer designer = this.Tab.addTab("scroll_inner", this.Tab.use_w, this.Tab.use_h, this.Tab.use_w, this.Tab.use_h, true);
						designer.scroll_area_selectable = true;
						designer.init();
						for (int j = 0; j < num2; j++)
						{
							Stomach.DishInStomach dishInStomach = eatenDishArray[j];
							string text = dishInStomach.Dh.title + "\n" + TX.GetA("Stat_food_time_left", X.spr_after((float)X.IntC(dishInStomach.cost * 100f) / 100f, 2));
							if (dishInStomach.bonused)
							{
								text = text + "\n" + TX.Get("lunch_effect_double_s", "");
							}
							designer.addP(new DsnDataP("", false)
							{
								text = text,
								size = 16f,
								alignx = ALIGN.CENTER,
								Col = MTRX.ColTrnsp,
								TxCol = C32.d2c(4283780170U),
								swidth = 200f,
								html = true,
								text_auto_wrap = true,
								Aword_splitter = new char[] { '\b' }
							}, false);
							designer.addP(new DsnDataP("", false)
							{
								text = RecipeManager.getEffectListup(dishInStomach.Dh, dishInStomach.lvl01, "\n", "Item_for_food_no_effect"),
								alignx = ALIGN.LEFT,
								swidth = designer.use_w - 10f,
								html = true,
								text_auto_wrap = true,
								size = 14f,
								Col = color
							}, false);
							designer.Br();
						}
						this.Tab.endTab(true);
					}
					else
					{
						UiBoxDesigner.addPTo(this.Tab, TX.Get("Stat_not_eaten_anything", ""), ALIGN.LEFT, this.Tab.use_w, true, 0f, "", -1);
					}
					break;
				}
				case UiGMCStat.STATUS_CTG.SEXUAL:
					this.Pr.EpCon.addGMConditionDescript(this.Tab);
					break;
				case UiGMCStat.STATUS_CTG.DANGEROUSNESS:
				{
					if (this.DangerousMeter == null)
					{
						this.DangerousMeter = new DangerGageDrawer(null, null, false);
					}
					if (base.M2D.WM.CurWM != null && !base.M2D.WM.CurWM.safe_area)
					{
						UiBoxDesigner.addPTo(this.Tab, TX.GetA("Explore_timer", ((int)(COOK.calced_timer / 3600f)).ToString(), X.spr0((int)(COOK.calced_timer / 60f) % 60, 2, '0')), ALIGN.LEFT, 0f, true, 0f, "", -1);
						this.Tab.Br();
					}
					this.Tab.addImg(new DsnDataImg
					{
						name = "magnity",
						swidth = this.Tab.use_w - 4f,
						sheight = 160f,
						FnDraw = new MeshDrawer.FnGeneralDraw(this.fnDrawDangerousness)
					}).use_valotile = true;
					this.Tab.Br();
					UiBoxDesigner.addPTo(this.Tab, TX.GetA("Dangerousness_text", base.M2D.NightCon.getDangerMeterVal(false).ToString() ?? ""), ALIGN.LEFT, 0f, false, 0f, "", -1);
					this.Tab.Br();
					WeatherItem[] weatherArray = base.M2D.NightCon.getWeatherArray();
					int num = weatherArray.Length;
					Color32 color = C32.d2c(base.effect_confusion ? 4289571750U : 4283780170U);
					for (int i = ((num == 0) ? (-1) : 0); i < num; i++)
					{
						WeatherItem weatherItem = ((i < 0) ? null : weatherArray[i]);
						WeatherItem.WEATHER weather = ((weatherItem == null) ? WeatherItem.WEATHER.NORMAL : weatherItem.weather);
						UiBoxDesigner.addPTo(this.Tab, "<img mesh=\"weather." + ((int)((i == -1) ? WeatherItem.WEATHER.NORMAL : weatherItem.weather)).ToString() + "\" />" + TX.Get("Weather_" + weather.ToString().ToLower(), ""), ALIGN.CENTER, 120f, true, 0f, "", -1);
						this.Tab.addP(new DsnDataP("", false)
						{
							text = TX.Get("Weather_desc_" + weather.ToString().ToLower(), ""),
							alignx = ALIGN.LEFT,
							swidth = this.Tab.use_w - 10f,
							html = true,
							size = 14f,
							text_auto_wrap = true,
							Col = color
						}, false);
						this.Tab.Br();
					}
					break;
				}
				}
			}
			if (this.GM.isEditState())
			{
				this.statusTabScrollSelect();
			}
		}

		internal void resetStomach()
		{
			this.need_remake |= 2UL;
		}

		internal void resetEpStat()
		{
			this.need_remake |= 4UL;
		}

		internal void resetCondition()
		{
			this.need_remake |= 1UL;
		}

		private void statusTabScrollSelect()
		{
			if (this.Tab == null || !this.Tab.use_scroll)
			{
				return;
			}
			ScrollBox scrollBox = this.Tab.getScrollBox();
			if (scrollBox == null)
			{
				return;
			}
			scrollBox.Select();
			this.Tab.scroll_area_selectable = this.GM.isEditState();
		}

		private bool fnDrawDangerousness(MeshDrawer Md, float alpha)
		{
			if (!Md.hasMultipleTriangle())
			{
				FillImageBlock fillImageBlock = this.Tab.Get("magnity", false) as FillImageBlock;
				if (fillImageBlock == null)
				{
					return false;
				}
				this.DangerousMeter.initMesh(Md, fillImageBlock.getMeshRenderer(), true, 1f);
				this.DangerousMeter.speed_ratio = 0.7f;
				int dangerMeterVal = base.M2D.NightCon.getDangerMeterVal(false);
				this.DangerousMeter.val = dangerMeterVal;
				this.DangerousMeter.already_show = dangerMeterVal;
			}
			Md.clear(false, false);
			this.DangerousMeter.Redraw(Md, 4000f, true, alpha);
			Md.chooseSubMesh(0, false, false);
			Md.updateForMeshRenderer(true);
			return true;
		}

		private bool fnStatusTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (UiGMCStat.status_tab != (UiGMCStat.STATUS_CTG)cur_value)
			{
				this.AEvCon[(int)UiGMCStat.status_tab] = new Designer.EvacuateContainer(this.Tab, false);
				UiGMCStat.status_tab = (UiGMCStat.STATUS_CTG)cur_value;
				this.initStatusTab();
			}
			return true;
		}

		internal override void releaseEvac()
		{
			for (int i = this.AEvCon.Length - 1; i >= 0; i--)
			{
				this.releaseEvac(ref this.AEvCon[i]);
			}
			base.releaseEvac();
			this.RTabBar = null;
		}

		private bool FnDrawLunchCost(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (!Md.hasMultipleTriangle())
			{
				FI.use_valotile = true;
				FI.name = "LunchCOST";
				Md.setMaterial(this.SMtrLunchCostMask, false);
				Md.chooseSubMesh(1, false, false);
				Material mtr = MTRX.MIicon.getMtr(BLEND.NORMAL, -1);
				Md.setMaterial(mtr, false);
				Md.chooseSubMesh(2, false, false);
				Md.setMaterial(this.SMtrLunchCost, false);
				FI.replaceMaterialManual(this.SMtrLunchCost);
				Md.chooseSubMesh(3, false, false);
				Md.base_z = -0.06f;
				Md.setMaterial(MTRX.getMtr(BLEND.NORMAL, -1), false);
			}
			Md.base_px_y = (Md.base_px_y = 4f);
			Md.chooseSubMesh(0, false, false);
			UiLunchTimeBase.drawCostMeterS(Md, -2, 0, 0, alpha, base.M2D.IMNG.StmNoel, null);
			UiLunchTimeBase.drawCostMeterS(Md, 1, 2, 3, alpha, base.M2D.IMNG.StmNoel, null);
			update_meshdrawer = true;
			return false;
		}

		internal static UiGMCStat.STATUS_CTG status_tab;

		private ColumnRowNel RTabBar;

		private Material SMtrLunchCostMask;

		private Material SMtrLunchCost;

		private MeshRenderer MrdLunchCostMask;

		private FillImageBlock FbLunchCost;

		private DangerGageDrawer DangerousMeter;

		private FillBlock FbHp;

		private FillBlock FbMp;

		private Designer Tab;

		private ulong pre_ser_bits;

		private ulong need_remake;

		private UiGMCStat.StatMem StatusMem;

		private const int ui_confuse_level = 1;

		protected Designer.EvacuateContainer[] AEvCon;

		internal enum STATUS_CTG
		{
			CONDITION,
			FOOD,
			SEXUAL,
			DANGEROUSNESS,
			_MAX
		}

		public struct StatMem
		{
			public StatMem(PR Pr)
			{
				this.hp = (int)Pr.get_hp();
				this.maxhp = (int)Pr.get_maxhp();
				this.mp = (int)Pr.get_mp();
				this.maxmp = (int)Pr.get_maxmp();
				this.confuse = Pr.Ser.getLevel(SER.CONFUSE) >= 1;
			}

			public bool isEqual(UiGMCStat.StatMem S)
			{
				return this.hp == S.hp && this.maxhp == S.maxhp && this.mp == S.mp && this.maxmp == S.maxmp && this.confuse == S.confuse;
			}

			public int hp;

			public int maxhp;

			public int mp;

			public int maxmp;

			public bool confuse;
		}
	}
}
