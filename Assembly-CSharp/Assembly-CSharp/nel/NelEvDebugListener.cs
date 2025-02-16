using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public class NelEvDebugListener : M2EvDebugListener
	{
		public NelEvDebugListener(NelM2DBase _NM2D)
			: base(_NM2D)
		{
			this.NM2D = _NM2D;
			this.OAtkFld = new BDic<int, NelEvDebugListener.AtkHolder>();
			this.OHpLock = new BDic<int, bool>();
			this.OMpLock = new BDic<int, bool>();
			this.Adeleted = new List<int>();
			if (this.Dbg != null)
			{
				this.Dbg.addIgnoreEventHeader("__M2D_");
				this.Dbg.addIgnoreEventHeader("%M2D");
				this.Dbg.addIgnoreEventHeader("__INIT");
			}
		}

		public override void destruct()
		{
			base.destruct();
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (base.isActive())
			{
				UIStatus.Instance.forceHide(true);
			}
			if (this.OAtkFld.Count != 0)
			{
				this.runAtkFld();
			}
			return true;
		}

		public override bool initS(Map2d Mp)
		{
			if (base.initS(Mp))
			{
				if (this.Dbg.isActive(EvDebugger.MODE.OTHER) && this.OAtkFld.Count > 0)
				{
					base.modeRecreate();
				}
				else
				{
					this.OAtkFld.Clear();
				}
				return true;
			}
			return false;
		}

		protected override void activateChanged(EvDebugger Dbg, bool flag = false)
		{
			if (flag)
			{
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable != null)
				{
					flgUiEffectDisable.Add("EVD");
				}
				Flagger flgValotileDisable = UIBase.FlgValotileDisable;
				if (flgValotileDisable != null)
				{
					flgValotileDisable.Add("EVD");
				}
			}
			else
			{
				Flagger flgUiEffectDisable2 = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable2 != null)
				{
					flgUiEffectDisable2.Rem("EVD");
				}
				Flagger flgValotileDisable2 = UIBase.FlgValotileDisable;
				if (flgValotileDisable2 != null)
				{
					flgValotileDisable2.Rem("EVD");
				}
			}
			base.activateChanged(Dbg, flag);
		}

		public override void tabLTInit(Designer DsLT)
		{
			base.tabLTInit(DsLT);
			DsLT.add(new DsnDataButton
			{
				title = "_D",
				skin_title = "Danger",
				w = 66f,
				h = 18f,
				unselectable = 2,
				fnClick = new FnBtnBindings(base.changeMode)
			});
			DsLT.add(new DsnDataButton
			{
				title = "_H",
				skin_title = "HP/MP",
				w = 66f,
				h = 18f,
				unselectable = 2,
				fnClick = new FnBtnBindings(base.changeMode)
			});
			DsLT.add(new DsnDataButton
			{
				title = "_I",
				skin_title = "Item",
				w = 66f,
				h = 18f,
				unselectable = 2,
				fnClick = new FnBtnBindings(base.changeMode)
			});
			DsLT.add(new DsnDataButton
			{
				title = "_R",
				skin_title = "Recipe",
				w = 66f,
				h = 18f,
				unselectable = 2,
				fnClick = new FnBtnBindings(base.changeMode)
			});
			DsLT.add(new DsnDataButton
			{
				title = "_A",
				skin_title = "Achive",
				w = 66f,
				h = 18f,
				unselectable = 2,
				fnClick = new FnBtnBindings(base.changeMode)
			});
		}

		public override bool categoryInit(string category, Designer DsT, Designer DsL, Designer DsR)
		{
			this.BConWt = null;
			this.OAtkFld.Clear();
			if (category != null)
			{
				if (category == "_D")
				{
					this.initCategoryDangerousness(DsT, DsL, DsR);
					return true;
				}
				if (category == "_H")
				{
					this.initCategoryEnemy(DsT, DsL, DsR);
					return true;
				}
				if (category == "_I")
				{
					this.initCategoryItem(DsT, DsL, DsR);
					return true;
				}
				if (category == "_R")
				{
					this.initCategoryRecipe(DsT, DsL, DsR);
					return true;
				}
				if (category == "_A")
				{
					this.initCategoryAchivement(DsT, DsL, DsR);
					return true;
				}
			}
			return base.categoryInit(category, DsT, DsL, DsR);
		}

		public void initCategoryDangerousness(Designer DsT, Designer DsL, Designer DsR)
		{
			DsT.Clear();
			DsL.Clear();
			DsR.Clear();
			NightController NC = this.NM2D.NightCon;
			DsL.add(new DsnDataInput
			{
				label = "Current Map",
				h = 24f,
				editable = false,
				bounds_w = DsL.use_w - 20f,
				size = 14,
				fnReturn = this.fnReturnNothing,
				def = ((this.NM2D.curMap != null) ? this.NM2D.curMap.key : "-")
			}).Br();
			base.addHr(DsL, 12, 18);
			DsL.add(new DsnDataInput
			{
				label = "Danger",
				bounds_w = 240f,
				integer = true,
				h = 40f,
				size = 22,
				min = 0.0,
				max = 160.0,
				def = NC.getDangerMeterVal(true, false).ToString(),
				fnReturn = this.fnReturnNothing,
				fnChangedDelay = delegate(LabeledInputField Li)
				{
					NC.applyDangerousFromEvent(X.NmI(Li.text, 0, true, false), true, false, false);
					return true;
				}
			}).Br();
			DsL.add(new DsnDataInput
			{
				label = " └ D:Juice",
				bounds_w = 240f,
				integer = true,
				min = 0.0,
				max = 45.0,
				def = NC.getDangerAddedVal().ToString(),
				size = 14,
				fnChangedDelay = delegate(LabeledInputField Li)
				{
					NC.setAdditionalDangerLevelManual(X.NmI(Li.text, 0, true, false));
					return true;
				}
			}).Br();
			DsL.add(new DsnDataInput
			{
				label = "Reel max",
				bounds_w = 320f,
				integer = true,
				size = 22,
				h = 40f,
				min = 0.0,
				max = 99.0,
				def = NC.getBattleCount().ToString(),
				fnReturn = this.fnReturnNothing,
				fnChangedDelay = delegate(LabeledInputField Li)
				{
					NC.setBattleCount(X.NmI(Li.text, 0, true, false));
					return true;
				}
			}).Br();
			base.addHr(DsL, 12, 18);
			DsL.add(new DsnDataChecks
			{
				clms = 1,
				margin_h = 4,
				w = DsL.use_w,
				h = 28f,
				keys = new string[] { "Lock Dangerousness", "Allow fast travel in night time" },
				def = ((NC.debug_lock_dangerousness ? 1 : 0) | (NC.debug_allow_night_travel ? 2 : 0)),
				fnClick = new FnBtnBindings(this.fnChangeLock)
			}).Br();
			int num = 7;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = "weather." + i.ToString();
			}
			this.BConWt = DsR.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				skin = "mini_dark",
				margin_h = 2f,
				margin_w = 2f,
				w = 32f,
				h = 32f,
				def = 0,
				titles = array,
				fnClick = new FnBtnBindings(this.fnClickWeather)
			});
			this.fineWeatherBCon();
			DsR.Br();
			DsR.add(new DsnDataChecks
			{
				clms = 1,
				margin_h = 4,
				w = DsL.use_w,
				h = 28f,
				keys = new string[] { "Lock weather" },
				def = (NC.debug_lock_weather ? 1 : 0),
				fnClick = new FnBtnBindings(this.fnChangeLock)
			}).Br();
		}

		private bool fnClickWeather(aBtn B)
		{
			if (this.BConWt == null)
			{
				return false;
			}
			WeatherItem.WEATHER weather = (WeatherItem.WEATHER)X.NmI(TX.slice(B.title, 8), 0, false, false);
			NightController nightCon = this.NM2D.NightCon;
			if (weather == WeatherItem.WEATHER.NORMAL)
			{
				if (B.isChecked())
				{
					return false;
				}
				this.BConWt.setValue("1");
				nightCon.applyWeatherDebug("NORMAL");
			}
			else
			{
				nightCon.applyWeatherDebug(((!B.isChecked()) ? "" : "!") + weather.ToString());
				this.fineWeatherBCon();
			}
			return true;
		}

		private void fineWeatherBCon()
		{
			if (this.BConWt == null)
			{
				return;
			}
			int num = 7;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				if (i == 0)
				{
					num2 |= ((this.NM2D.NightCon.current_weather_bit == 0) ? 1 : 0);
				}
				else
				{
					num2 |= (this.NM2D.NightCon.hasWeather((WeatherItem.WEATHER)i) ? (1 << i) : 0);
				}
			}
			this.BConWt.setValue(num2.ToString());
		}

		private bool fnChangeLock(aBtn B)
		{
			NightController nightCon = this.NM2D.NightCon;
			string title = B.title;
			if (title != null)
			{
				if (!(title == "Lock Dangerousness"))
				{
					if (!(title == "Allow fast travel in night time"))
					{
						if (title == "Lock weather")
						{
							nightCon.debug_lock_weather = B.isChecked();
						}
					}
					else
					{
						nightCon.debug_allow_night_travel = B.isChecked();
					}
				}
				else
				{
					nightCon.debug_lock_dangerousness = B.isChecked();
				}
			}
			return true;
		}

		public void initCategoryEnemy(Designer DsT, Designer DsL, Designer DsR)
		{
			DsT.Clear();
			DsL.Clear();
			DsR.Clear();
			DsT.add(new DsnDataButton
			{
				title = "Fine Columns",
				w = 200f,
				h = 18f,
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					base.modeRecreate();
					return true;
				}
			});
			DsT.add(new DsnDataButton
			{
				title = "Auto refine variables ",
				w = 150f,
				h = 18f,
				def = this.enemy_auto_refine,
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					B.SetChecked(this.enemy_auto_refine = !B.isChecked(), true);
					return true;
				}
			});
			Map2d curMap = this.NM2D.curMap;
			if (curMap == null)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				Designer designer = ((i == 0) ? DsL : DsR);
				float use_w = designer.use_w;
				int mover_count = curMap.mover_count;
				for (int j = 0; j < mover_count; j++)
				{
					M2Attackable Mv = curMap.getMv(j) as M2Attackable;
					if (!(Mv == null) && ((i == 0) ? (Mv is M2MoverPr) : (Mv is NelEnemy)))
					{
						if (!this.OHpLock.ContainsKey(Mv.index))
						{
							this.OHpLock[Mv.index] = false;
							this.OMpLock[Mv.index] = false;
						}
						NelEvDebugListener.AtkHolder atkHolder = (this.OAtkFld[Mv.index] = new NelEvDebugListener.AtkHolder());
						designer.add(new DsnDataP(Mv.key, false)
						{
							size = 14f,
							TxCol = C32.d2c(uint.MaxValue)
						});
						aBtn aBtn = designer.addButton(new DsnDataButton
						{
							w = 50f,
							h = 18f,
							title = "KILL",
							unselectable = 2
						});
						designer.Br();
						LabeledInputField labeledInputField = designer.addInput(new DsnDataInput
						{
							label = "HP",
							def = Mv.get_hp().ToString(),
							bounds_w = use_w / 2f - designer.item_margin_x_px * 4f - 44f,
							h = 32f,
							integer = true,
							min = 0.0,
							fnReturn = this.fnReturnNothing,
							max = (double)Mv.get_maxhp(),
							size = 18
						});
						aBtnNel aBtnNel = designer.addButtonT<aBtnNel>(new DsnDataButton
						{
							skin = "mini_dark",
							title = "hp_lock",
							skin_title = "mini_lock",
							def = this.OHpLock[Mv.index],
							w = 22f,
							h = 22f,
							fnClick = delegate(aBtn B)
							{
								B.SetChecked(this.OHpLock[Mv.index] = !B.isChecked(), true);
								return true;
							}
						});
						LabeledInputField labeledInputField2 = designer.addInput(new DsnDataInput
						{
							label = "MP",
							def = Mv.get_mp().ToString(),
							bounds_w = use_w / 2f - designer.item_margin_x_px * 4f - 44f,
							h = 32f,
							size = 18,
							integer = true,
							fnReturn = this.fnReturnNothing,
							min = 0.0,
							max = (double)Mv.get_maxmp()
						});
						aBtnNel aBtnNel2 = designer.addButtonT<aBtnNel>(new DsnDataButton
						{
							skin = "mini_dark",
							title = "mp_lock",
							skin_title = "mini_lock",
							def = this.OMpLock[Mv.index],
							w = 22f,
							h = 22f,
							fnClick = delegate(aBtn B)
							{
								B.SetChecked(this.OMpLock[Mv.index] = !B.isChecked(), true);
								return true;
							}
						});
						designer.Br();
						LabeledInputField labeledInputField3 = designer.addInput(new DsnDataInput
						{
							label = "Pos",
							bounds_w = use_w - designer.item_margin_x_px,
							h = 26f,
							fnReturn = this.fnReturnNothing,
							size = 12,
							alloc_char = REG.RegCharPos
						});
						base.addHr(designer, 8, 12);
						atkHolder.Set(labeledInputField, labeledInputField2, labeledInputField3, aBtn, aBtnNel, aBtnNel2, Mv);
					}
				}
			}
		}

		public void runAtkFld()
		{
			if (this.NM2D.curMap == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = this.Dbg.isActive(EvDebugger.MODE.OTHER);
			bool flag3 = flag2 && this.enemy_auto_refine;
			try
			{
				foreach (KeyValuePair<int, NelEvDebugListener.AtkHolder> keyValuePair in this.OAtkFld)
				{
					if (keyValuePair.Value.Atk.destructed)
					{
						flag = true;
						if (flag2)
						{
							break;
						}
					}
					else
					{
						keyValuePair.Value.Fine(flag3);
					}
				}
			}
			catch
			{
				flag = true;
			}
			if (flag && flag2)
			{
				base.modeRecreate();
			}
		}

		public void initCategoryItem(Designer DsT, Designer DsL, Designer DsR)
		{
			DsT.Clear();
			DsL.Clear();
			DsR.Clear();
			Designer designer = DsL.addTab("ilist", DsL.use_w, DsL.use_h - 75f, DsL.use_w, DsL.use_h - 75f, false);
			designer.Smallest();
			BtnContainerRadio<aBtn> BConIL = UiItemWholeSelector.prepareDesigner<aBtn, aBtn>(DsR, designer, 230, 32, 1, "checkbox_string", true, "row", 20);
			DsL.endTab(true);
			DsL.Br();
			Designer designer2 = DsL.addTab("ilist_b", DsL.use_w, DsL.use_h, DsL.use_w, DsL.use_h, false);
			designer2.Smallest();
			LabeledInputField Inp = DsL.addInput(new DsnDataInput
			{
				name = "grade",
				label = "Grade",
				bounds_w = 160f,
				h = 30f,
				size = 20,
				def = "0",
				integer = true,
				min = 0.0,
				max = 4.0,
				fnReturn = delegate(LabeledInputField B)
				{
					aBtn btn = DsL.getBtn("GetItem");
					if (btn != null)
					{
						btn.ExecuteOnClick();
					}
					this.fnReturnNothing(B);
					return true;
				}
			});
			DsL.addP(new DsnDataP("\u3000|\u3000", false), false);
			DsL.addButton(new DsnDataButton
			{
				name = "GetItem",
				title = "Get!",
				w = 160f,
				h = 30f,
				fnClick = delegate(aBtn B)
				{
					aBtn button = BConIL.GetButton(BConIL.getValue());
					if (button == null)
					{
						return false;
					}
					NelItem byId = NelItem.GetById(button.title, false);
					if (byId != null)
					{
						if (byId.is_skillbook)
						{
							this.NM2D.IMNG.getItem(byId, 1, 0, true, false, false, false);
							PrSkill prSkill = SkillManager.Get(byId);
							if (prSkill != null)
							{
								prSkill.Obtain(false);
							}
						}
						else if (TX.isStart(byId.key, "spconfig_", 0))
						{
							CFGSP.addSp(TX.slice(byId.key, "spconfig_".Length));
						}
						else
						{
							if (this.NM2D.curMap == null)
							{
								return false;
							}
							float num;
							float num2;
							if (this.NM2D.curMap.Pr == null)
							{
								num = this.NM2D.Cam.x;
								num2 = this.NM2D.Cam.y;
							}
							else
							{
								num = this.NM2D.curMap.Pr.x;
								num2 = this.NM2D.curMap.Pr.y - 1f;
							}
							this.NM2D.IMNG.dropManual(byId, 1, byId.individual_grade ? 0 : X.NmI(Inp.text, 0, false, false), num, num2, 0f, -0.05f, null, true, NelItemManager.TYPE.NORMAL).type |= NelItemManager.TYPE.ABSORB;
						}
					}
					return true;
				}
			});
			Designer designer3 = DsL.Br();
			DsnDataInput dsnDataInput = new DsnDataInput();
			dsnDataInput.name = "Money";
			dsnDataInput.label = "Money";
			dsnDataInput.bounds_w = 240f;
			dsnDataInput.h = 25f;
			dsnDataInput.integer = true;
			dsnDataInput.min = 0.0;
			dsnDataInput.max = 999999.0;
			dsnDataInput.def = CoinStorage.getCount(CoinStorage.CTYPE.GOLD).ToString();
			dsnDataInput.fnChangedDelay = delegate(LabeledInputField LI)
			{
				int count = (int)CoinStorage.getCount(CoinStorage.CTYPE.GOLD);
				int num3 = X.NmI(LI.text, count, false, false);
				if (count < num3)
				{
					CoinStorage.addCount(num3 - count, false);
				}
				else if (count > num3)
				{
					CoinStorage.reduceCount(count - num3, CoinStorage.CTYPE.GOLD);
				}
				return true;
			};
			designer3.addInput(dsnDataInput);
			DsL.endTab(true);
			IN.setZ(designer2.transform, -1f);
		}

		public void initCategoryRecipe(Designer DsT, Designer DsL, Designer DsR)
		{
			DsT.Clear();
			DsL.Clear();
			DsR.Clear();
			DsL.alignx = ALIGN.LEFT;
			Designer designer = DsL.addTab("ilist", DsL.use_w, DsL.use_h - 100f, DsL.use_w, DsL.use_h - 100f, true);
			designer.Small();
			designer.margin_in_tb = 10f;
			designer.margin_in_lr = 16f;
			designer.init();
			for (int i = 0; i < 28; i++)
			{
				Designer dsL = DsL;
				DsnDataP dsnDataP = new DsnDataP("", false);
				dsnDataP.swidth = 200f;
				dsnDataP.sheight = 18f;
				RCP.RPI_EFFECT rpi_EFFECT = (RCP.RPI_EFFECT)i;
				dsnDataP.text = rpi_EFFECT.ToString();
				dsnDataP.alignx = ALIGN.RIGHT;
				dsnDataP.TxCol = MTRX.ColWhite;
				dsnDataP.size = 13f;
				dsL.addP(dsnDataP, false);
				DsL.addInput(new DsnDataInput
				{
					bounds_w = designer.use_w - 40f,
					h = 18f,
					name = "rpi_" + i.ToString(),
					integer = true,
					def = "0",
					min = -100.0,
					max = 100.0
				});
				DsL.Br();
			}
			DsL.endTab(true);
			DsL.alignx = ALIGN.CENTER;
			DsL.addInput(new DsnDataInput
			{
				bounds_w = 140f,
				h = 30f,
				name = "cost",
				label = "cost",
				integer = true,
				def = "1",
				min = 1.0,
				max = 28.0
			});
			DsL.addButton(new DsnDataButton
			{
				w = 80f,
				h = 40f,
				title = "Apply",
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					this.applyEffects(DsL, DsR);
					return true;
				}
			});
			DsL.Br();
			DsL.addButton(new DsnDataButton
			{
				w = 80f,
				h = 26f,
				title = "Clear Field",
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					for (int k = 0; k < 28; k++)
					{
						DsL.setValueTo("rpi_" + k.ToString(), "0");
					}
					return true;
				}
			});
			DsL.addButton(new DsnDataButton
			{
				w = 80f,
				h = 26f,
				title = "Clear Effect",
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					this.NM2D.IMNG.StmNoel.clear();
					this.NM2D.IMNG.StmNoel.finePrStatus(false);
					this.fineRecipeEffects(DsR);
					return true;
				}
			});
			DsL.alignx = ALIGN.LEFT;
			for (int j = 0; j < 28; j++)
			{
				Designer dsR = DsR;
				DsnDataP dsnDataP2 = new DsnDataP("", false);
				dsnDataP2.swidth = 220f;
				dsnDataP2.sheight = 22f;
				RCP.RPI_EFFECT rpi_EFFECT = (RCP.RPI_EFFECT)j;
				dsnDataP2.text = rpi_EFFECT.ToString();
				dsnDataP2.alignx = ALIGN.RIGHT;
				dsnDataP2.TxCol = MTRX.ColWhite;
				dsnDataP2.size = 13f;
				dsR.addP(dsnDataP2, false);
				DsR.addP(new DsnDataP("", false)
				{
					swidth = DsR.use_w - 18f,
					sheight = 22f,
					name = "rpi_" + j.ToString(),
					text = "0",
					html = true,
					TxCol = MTRX.ColWhite,
					size = 13f
				}, false);
				DsR.Br();
			}
			this.fineRecipeEffects(DsR);
		}

		private void applyEffects(Designer DsL, Designer DsR)
		{
			RCP.RecipeDish recipeDish = null;
			BDic<RCP.RPI_EFFECT, float> bdic = null;
			for (int i = 0; i < 28; i++)
			{
				float num = X.Nm(DsL.getValue("rpi_" + i.ToString()), -101f, false);
				if (num >= -100f && num != 0f)
				{
					if (recipeDish == null)
					{
						recipeDish = new RCP.RecipeDish(null, 1);
						bdic = new BDic<RCP.RPI_EFFECT, float>();
						recipeDish.Create(RCP.Get("supersalad"));
						recipeDish.fineTitle(null, true, null);
					}
					bdic.Clear();
					bdic[(RCP.RPI_EFFECT)i] = num;
					recipeDish.addEffect100(bdic, 1f);
				}
			}
			if (recipeDish != null)
			{
				recipeDish.addCostInCreating(X.NmI(DsL.getValue("cost"), 1, false, false));
				recipeDish.finalizeDish();
				recipeDish.finalizeDishEffect();
				RCP.assignDish(recipeDish);
				this.NM2D.IMNG.StmNoel.addEffect(recipeDish, true, false, false);
				this.fineRecipeEffects(DsR);
			}
		}

		private void fineRecipeEffects(Designer DsR)
		{
			BDic<RCP.RPI_EFFECT, float> levelDictionary = this.NM2D.IMNG.StmNoel.getLevelDictionary01();
			for (int i = 0; i < 28; i++)
			{
				float num = 0f;
				levelDictionary.TryGetValue((RCP.RPI_EFFECT)i, out num);
				string text;
				if (num == 0f)
				{
					text = " - ";
				}
				else
				{
					text = "<font color=\"ff:#EAFF05\">" + X.IntC(num * 100f).ToString() + "</font>";
				}
				DsR.setValueTo("rpi_" + i.ToString(), text);
			}
		}

		public void initCategoryAchivement(Designer DsT, Designer DsL, Designer DsR)
		{
			DsT.Clear();
			DsL.Clear();
			DsR.Clear();
			DsL.item_margin_y_px = 2f;
			Dictionary<ACHIVE.MENT, ushort> wholeDictionary = COOK.CurAchive.getWholeDictionary();
			int num = 0;
			FnFldBindings fnFldBindings = (LabeledInputField Fld) => this.fineAchivementValue(DsL, Fld);
			foreach (KeyValuePair<ACHIVE.MENT, ushort> keyValuePair in wholeDictionary)
			{
				DsL.addP(new DsnDataP("", false)
				{
					name = "achivement__" + num.ToString(),
					text = keyValuePair.Key.ToString(),
					swidth = 200f,
					size = 14f,
					TxCol = MTRX.ColWhite
				}, false);
				DsL.addInput(new DsnDataInput
				{
					name = "achivement_val_" + num.ToString(),
					label = "",
					def = keyValuePair.Value.ToString(),
					w = DsL.use_w - 10f,
					size = 14,
					integer = true,
					min = 0.0,
					max = 65535.0,
					fnBlur = fnFldBindings
				});
				DsL.Br();
				num++;
			}
		}

		private bool fineAchivementValue(Designer DsL, LabeledInputField Fld)
		{
			string name = DsL.getName(Fld);
			if (name == null)
			{
				return false;
			}
			int num = X.NmI(TX.slice(name, "achivement_val_".Length), -1, false, false);
			if (num < 0)
			{
				return false;
			}
			FillBlock fillBlock = DsL.Get("achivement__" + num.ToString(), false) as FillBlock;
			if (fillBlock == null)
			{
				return false;
			}
			string text_content = fillBlock.text_content;
			COOK.CurAchive.Set(text_content, X.NmI(Fld.text, 0, false, false));
			return true;
		}

		public override bool debugCommandLine(string[] Acmd)
		{
			string text = Acmd[0].ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 2178102629U)
				{
					if (num <= 325011298U)
					{
						if (num <= 130093117U)
						{
							if (num != 43544391U)
							{
								if (num != 130093117U)
								{
									goto IL_07C7;
								}
								if (!(text == "MOBTYPE"))
								{
									goto IL_07C7;
								}
								if (M2DBase.Instance == null)
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									UiBenchMenu.event_defined = false;
									M2DBase.Instance.ev_mobtype = Acmd[1];
									X.dl("ev_mobtype を " + Acmd[1] + " に設定", null, false, false);
									goto IL_07C7;
								}
								X.dl("ev_mobtype は " + M2DBase.Instance.ev_mobtype + " です", null, false, false);
								goto IL_07C7;
							}
							else
							{
								if (!(text == "PRPOS"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									Vector2 pos = this.NM2D.curMap.getPos(Acmd[1], 0f, 0f, this.NM2D.getPrNoel());
									this.NM2D.getPrNoel().setTo(pos.x, pos.y);
									goto IL_07C7;
								}
								goto IL_07C7;
							}
						}
						else if (num != 166681459U)
						{
							if (num != 320605779U)
							{
								if (num != 325011298U)
								{
									goto IL_07C7;
								}
								if (!(text == "BENCH_ENEMY_ORGASM"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									UiBenchMenu.enemy_orgasm = X.NmI(Acmd[1], 0, false, false) != 0;
								}
								X.dl("gm.UiBenchMenu.enemy_orgasm: " + UiBenchMenu.enemy_orgasm.ToString(), null, false, false);
								goto IL_07C7;
							}
							else
							{
								if (!(text == "FADER"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									int num2 = (UIPicture.Instance.setFade(Acmd[1], UIPictureBase.EMSTATE.NORMAL, true, true, false) ? 1 : 0);
									X.dl("setFader: " + Acmd[1], null, false, false);
									return num2 == 0;
								}
								goto IL_07C7;
							}
						}
						else
						{
							if (!(text == "WEATHER"))
							{
								goto IL_07C7;
							}
							if (Acmd.Length < 2)
							{
								this.NM2D.NightCon.weatherShuffle();
								return false;
							}
							if (this.NM2D.NightCon.applyWeatherDebug(Acmd[1]))
							{
								return false;
							}
							goto IL_07C7;
						}
					}
					else if (num <= 961323417U)
					{
						if (num != 453043962U)
						{
							if (num != 808188337U)
							{
								if (num != 961323417U)
								{
									goto IL_07C7;
								}
								if (!(text == "BENCH"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									UiBenchMenu.debugDefineCmd(Acmd, 1);
									X.dl("ベンチコマンドを追加:" + TX.join<string>(",", Acmd, 1, -1), null, false, false);
									goto IL_07C7;
								}
								UiBenchMenu.endS(true);
								X.dl("ベンチコマンドを初期化", null, false, false);
								goto IL_07C7;
							}
							else
							{
								if (!(text == "NOELJUICE"))
								{
									goto IL_07C7;
								}
								if (M2DBase.Instance != null)
								{
									(M2DBase.Instance as NelM2DBase).getPrNoel().JuiceCon.executeSplashNoelJuice(false, true, 0, false, false, false, false);
								}
								return true;
							}
						}
						else
						{
							if (!(text == "UIDAMAGE"))
							{
								goto IL_07C7;
							}
							if (Acmd.Length >= 2 && this.NM2D.PlayerNoel.UP.applyDamageDebug(Acmd[1]))
							{
								return false;
							}
							goto IL_07C7;
						}
					}
					else if (num != 997919947U)
					{
						if (num != 1425517024U)
						{
							if (num != 2178102629U)
							{
								goto IL_07C7;
							}
							if (!(text == "NIGHTINGALE"))
							{
								goto IL_07C7;
							}
							this.NM2D.WDR.debugFlush(WanderingManager.TYPE.NIG, Acmd);
							goto IL_07C7;
						}
						else if (!(text == "FLUSH_MAP"))
						{
							goto IL_07C7;
						}
					}
					else
					{
						if (!(text == "HIDENOEL"))
						{
							goto IL_07C7;
						}
						if (M2DBase.Instance != null)
						{
							bool flag = Acmd.Length == 1 || X.NmI(Acmd[1], 1, false, false) != 0;
							(M2DBase.Instance as NelM2DBase).getPrNoel().getAnimator().hidden_flag = flag;
						}
						return true;
					}
				}
				else
				{
					if (num <= 3044942830U)
					{
						if (num <= 2400252072U)
						{
							if (num != 2338783123U)
							{
								if (num != 2357528558U)
								{
									if (num != 2400252072U)
									{
										goto IL_07C7;
									}
									if (!(text == "GATHER_ITEM"))
									{
										goto IL_07C7;
									}
									this.NM2D.IMNG.gatherWholeDropItem();
									return true;
								}
								else
								{
									if (!(text == "DANGER"))
									{
										goto IL_07C7;
									}
									if (Acmd.Length >= 2)
									{
										this.NM2D.NightCon.applyDangerousFromEvent(X.NmI(Acmd[1], 0, false, false), false, false, false);
										this.NM2D.NightCon.setBattleCount(64);
										return true;
									}
									goto IL_07C7;
								}
							}
							else if (!(text == "AUTOSAVE"))
							{
								goto IL_07C7;
							}
						}
						else if (num != 2450413691U)
						{
							if (num != 2463182076U)
							{
								if (num != 3044942830U)
								{
									goto IL_07C7;
								}
								if (!(text == "PUPPETNPC"))
								{
									goto IL_07C7;
								}
								this.NM2D.WDR.debugFlush(WanderingManager.TYPE.PUP, Acmd);
								goto IL_07C7;
							}
							else
							{
								if (!(text == "GETSKILL"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length < 2)
								{
									goto IL_07C7;
								}
								PrSkill prSkill = SkillManager.Get(Acmd[1]);
								if (prSkill == null)
								{
									X.de("スキルが不明:" + Acmd[1], null);
									goto IL_07C7;
								}
								this.NM2D.IMNG.get_DescBox().addTaskFocus(prSkill, false);
								prSkill.Obtain(false);
								goto IL_07C7;
							}
						}
						else
						{
							if (!(text == "FLUSH_MATERIAL"))
							{
								goto IL_07C7;
							}
							goto IL_05EE;
						}
					}
					else if (num <= 3911183430U)
					{
						if (num != 3613420707U)
						{
							if (num != 3681775455U)
							{
								if (num != 3911183430U)
								{
									goto IL_07C7;
								}
								if (!(text == "AUTO_SAVE"))
								{
									goto IL_07C7;
								}
							}
							else
							{
								if (!(text == "WALKCOUNT"))
								{
									goto IL_07C7;
								}
								if (Acmd.Length >= 2)
								{
									COOK.map_walk_count = X.NmI(Acmd[1], 0, false, false);
									X.dl("map_walk_count を " + Acmd[1] + " に設定", null, false, false);
									return true;
								}
								goto IL_07C7;
							}
						}
						else
						{
							if (!(text == "GET_ALL_ITEM"))
							{
								goto IL_07C7;
							}
							foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.getWholeDictionary())
							{
								if (!keyValuePair.Value.is_cache_item)
								{
									this.NM2D.IMNG.getItem(keyValuePair.Value, 1, 0, false, false, false, false);
								}
							}
							return true;
						}
					}
					else if (num != 3977484580U)
					{
						if (num != 4005525579U)
						{
							if (num != 4071042880U)
							{
								goto IL_07C7;
							}
							if (!(text == "STOREFLUSH"))
							{
								goto IL_07C7;
							}
							if (Acmd.Length < 2)
							{
								StoreManager.FlushAll(false);
								goto IL_07C7;
							}
							StoreManager storeManager = StoreManager.Get(Acmd[1], false);
							if (storeManager != null)
							{
								storeManager.need_summon_flush |= StoreManager.MODE.FLUSH;
								goto IL_07C7;
							}
							goto IL_07C7;
						}
						else
						{
							if (!(text == "COFFEEMAKER"))
							{
								goto IL_07C7;
							}
							this.NM2D.WDR.debugFlush(WanderingManager.TYPE.COF, Acmd);
							goto IL_07C7;
						}
					}
					else
					{
						if (!(text == "TILDENPC"))
						{
							goto IL_07C7;
						}
						this.NM2D.WDR.debugFlush(WanderingManager.TYPE.TLD, Acmd);
						goto IL_07C7;
					}
					if (M2DBase.Instance != null)
					{
						COOK.autoSave(M2DBase.Instance as NelM2DBase, false, true);
						goto IL_07C7;
					}
					goto IL_07C7;
				}
				IL_05EE:
				if (M2DBase.Instance != null)
				{
					M2DBase.Instance.setFlushOtherMatFlag();
					if (Acmd[0] == "FLUSH_MAP" || (Acmd.Length >= 2 && X.Nm(Acmd[1], 0f, false) != 0f))
					{
						M2DBase.Instance.setFlushAllFlag(true);
					}
				}
			}
			IL_07C7:
			return base.debugCommandLine(Acmd);
		}

		public readonly NelM2DBase NM2D;

		private BDic<int, NelEvDebugListener.AtkHolder> OAtkFld;

		private BDic<int, bool> OHpLock;

		private BDic<int, bool> OMpLock;

		private List<int> Adeleted;

		private bool enemy_auto_refine = true;

		private BtnContainer<aBtn> BConWt;

		private class AtkHolder
		{
			public NelEvDebugListener.AtkHolder Set(LabeledInputField _FldHp, LabeledInputField _FldMp, LabeledInputField _FldPos, aBtn KillBtn, aBtn _LockHp, aBtn _LockMp, M2Attackable Atk)
			{
				this.FldHp = _FldHp;
				this.FldMp = _FldMp;
				this.FldPos = _FldPos;
				this.LockHp = _LockHp;
				this.LockMp = _LockMp;
				this.Atk = Atk;
				this.hp = Atk.get_hp();
				this.mp = Atk.get_mp();
				KillBtn.addClickFn(delegate(aBtn B)
				{
					this.FldHp.setValue("0", true, false);
					return true;
				});
				this.FldHp.addChangedDelayFn(new FnFldBindings(this.fnChangeFldHp));
				this.FldMp.addChangedDelayFn(new FnFldBindings(this.fnChangeFldMp));
				this.FldPos.addChangedDelayFn(delegate(LabeledInputField Li)
				{
					if (Atk.destructed)
					{
						return false;
					}
					if (REG.match(Li.text, REG.RegPosition))
					{
						float num = X.Nm(REG.R1, this.pre_x, false);
						float num2 = X.Nm(REG.R2, this.pre_y, false);
						if (num == this.pre_x && num2 == this.pre_y)
						{
							return true;
						}
						Atk.moveBy(num - Atk.x, num2 - Atk.y, true);
						this.pre_x = Atk.x;
						this.pre_y = Atk.y;
					}
					return true;
				});
				return this;
			}

			private bool fnChangeFldHp(LabeledInputField _Li)
			{
				if (this.Atk.destructed)
				{
					return false;
				}
				int num = X.NmI(this.FldHp.text, (int)this.hp, true, false);
				if ((float)num == this.hp)
				{
					return true;
				}
				if (this.hp < (float)num)
				{
					this.Atk.cureHp((int)((float)num - this.hp));
				}
				else
				{
					this.Atk.applyHpDamage((int)(this.hp - (float)num), true, null);
					if (this.Atk is NelEnemy && !this.Atk.is_alive)
					{
						(this.Atk as NelEnemy).changeStateToDie();
					}
					if (this.Atk is PR && !this.Atk.is_alive)
					{
						this.Atk.penetrateNoDamageTime(NDMG._ALL, -1);
						(this.Atk as PR).applyDamage(new NelAttackInfo
						{
							hpdmg0 = 9999,
							ndmg = NDMG.PRESSDAMAGE
						}, true);
					}
				}
				if (this.Atk is NelEnemy)
				{
					(this.Atk as NelEnemy).addF(NelEnemy.FLAG.FINE_HPMP_BAR);
				}
				this.hp = (float)num;
				return true;
			}

			private bool fnChangeFldMp(LabeledInputField _Li)
			{
				if (this.Atk.destructed)
				{
					return false;
				}
				int num = X.NmI(this.FldMp.text, (int)this.mp, true, false);
				if ((float)num == this.mp)
				{
					return true;
				}
				if (this.mp < (float)num)
				{
					this.Atk.cureMp((int)((float)num - this.mp));
				}
				else
				{
					this.Atk.applyMpDamage((int)(this.mp - (float)num), true, null);
					if (this.Atk is NelEnemy)
					{
						(this.Atk as NelEnemy).addF(NelEnemy.FLAG.CHECK_ENLARGE);
					}
				}
				if (this.Atk is NelEnemy)
				{
					(this.Atk as NelEnemy).addF(NelEnemy.FLAG.FINE_HPMP_BAR);
				}
				this.mp = (float)num;
				return true;
			}

			public void Fine(bool enemy_auto_refine)
			{
				if (this.hp != this.Atk.get_hp())
				{
					if (this.LockHp.isChecked())
					{
						this.FldHp.setValue(this.hp.ToString(), false, false);
						this.hp = this.Atk.get_hp();
						this.fnChangeFldHp(null);
					}
					else if (enemy_auto_refine)
					{
						this.hp = this.Atk.get_hp();
						this.FldHp.text = this.hp.ToString();
					}
				}
				if (this.mp != this.Atk.get_mp())
				{
					if (this.LockMp.isChecked())
					{
						this.FldMp.setValue(this.mp.ToString(), false, false);
						this.mp = this.Atk.get_mp();
						this.fnChangeFldMp(null);
					}
					else if (enemy_auto_refine)
					{
						this.mp = this.Atk.get_mp();
						this.FldMp.text = this.mp.ToString();
					}
				}
				if (enemy_auto_refine && (this.pre_x != this.Atk.x || this.pre_y != this.Atk.y))
				{
					this.pre_x = this.Atk.x;
					this.pre_y = this.Atk.y;
					this.FldPos.text = this.pre_x.ToString() + ", " + this.pre_y.ToString();
				}
			}

			public M2Attackable Atk;

			private LabeledInputField FldHp;

			private LabeledInputField FldMp;

			private LabeledInputField FldPos;

			private aBtn LockHp;

			private aBtn LockMp;

			private float hp;

			private float mp;

			private float pre_x;

			private float pre_y;
		}
	}
}
