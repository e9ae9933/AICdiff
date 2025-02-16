using System;
using m2d;
using XX;

namespace nel.mgm.smncr
{
	public class UiSmncBattleConfirm
	{
		private float bx_w
		{
			get
			{
				return IN.w * 0.6f;
			}
		}

		private float bx_h
		{
			get
			{
				return IN.h * 0.6f;
			}
		}

		public UiSmncBattleConfirm(UiBoxDesignerFamily _DsFam, M2LpUiSmnCreator _LpArea, SmncStageEditorManager.TYPE type)
		{
			this.Bx = _DsFam.Create("BattleConfirm", 0f, 0f, this.bx_w, this.bx_h, 1, 40f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.LpArea = _LpArea;
			this.target_type = type;
			this.createUi();
		}

		private void createUi()
		{
			this.Bx.margin_in_lr = 50f;
			this.Bx.margin_in_tb = 30f;
			this.Bx.item_margin_y_px = 10f;
			this.Bx.selectable_loop = 2;
			this.Bx.alignx = ALIGN.CENTER;
			this.Bx.init();
			this.FbError = this.Bx.addP(new DsnDataP("", false)
			{
				text = " ",
				size = 14f,
				html = true,
				text_margin_x = 60f,
				text_margin_y = 2f,
				alignx = ALIGN.LEFT,
				TxCol = NEL.ColText,
				swidth = this.Bx.use_w,
				text_auto_wrap = true
			}, false);
			this.Bx.Br().addHr(new DsnDataHr
			{
				draw_width_rate = 0.6f,
				Col = NEL.ColText,
				margin_t = 2f
			});
			this.Bx.Br();
			UiBoxDesigner bx = this.Bx;
			UiItemManageBoxSlider.DsnDataSliderIMB dsnDataSliderIMB = new UiItemManageBoxSlider.DsnDataSliderIMB(null, null);
			dsnDataSliderIMB.name = "danger";
			dsnDataSliderIMB.title = "danger";
			dsnDataSliderIMB.skin = "normal";
			dsnDataSliderIMB.def = 0f;
			dsnDataSliderIMB.mn = 0f;
			dsnDataSliderIMB.mx = 160f;
			dsnDataSliderIMB.valintv = 1f;
			dsnDataSliderIMB.w = this.Bx.use_w * 0.4f;
			dsnDataSliderIMB.h = 26f;
			dsnDataSliderIMB.fnDescConvert = new FnDescConvert(this.fnDescConvertDanger);
			dsnDataSliderIMB.fnChanged = delegate(aBtnMeter B, float pre, float cur)
			{
				if (!this.isActive() || this.CurFile == null)
				{
					return false;
				}
				this.CurFile.dangerousness = (byte)X.IntR(cur);
				return true;
			};
			dsnDataSliderIMB.fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
			{
				if (index == 0 || (float)index == 160f)
				{
					return 0.8f;
				}
				if ((float)index == 80f)
				{
					return 0.5f;
				}
				if (index % 16 != 0)
				{
					return 0f;
				}
				return 0.3f;
			};
			this.BSliderDanger = bx.addSliderCT(dsnDataSliderIMB, this.Bx.use_w * 0.28f, null, false);
			this.Bx.Br();
			Designer bx2 = this.Bx;
			DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
			dsnDataButtonMulti.name = "weather";
			dsnDataButtonMulti.titles = X.makeToStringed<int>(X.makeCountUpArray(7, 0, 1));
			dsnDataButtonMulti.clms = 0;
			dsnDataButtonMulti.def = 1;
			dsnDataButtonMulti.margin_w = 8f;
			dsnDataButtonMulti.w = 30f;
			dsnDataButtonMulti.h = 30f;
			dsnDataButtonMulti.skin = "mini_fixcolor";
			dsnDataButtonMulti.click_snd = "enter_small";
			dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
			{
				B.setSkinTitle("weather." + B.title);
				return true;
			};
			dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickWeather);
			this.BConWt = bx2.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			this.FbWt = this.Bx.addP(new DsnDataP("", false)
			{
				text = " ",
				TxCol = NEL.ColText,
				size = 14f,
				swidth = 200f,
				sheight = 30f,
				aligny = ALIGNY.MIDDLE
			}, false);
			this.BRandomizeNattr = this.Bx.Br().addButtonT<aBtnNel>(new DsnDataButton
			{
				skin = "checkbox",
				w = 360f,
				h = 28f,
				title = "randomize_nattr",
				skin_title = "&&Smnc_start_battle_randomize_nattr",
				fnClick = delegate(aBtn B)
				{
					if (!this.isActive() || this.CurFile == null)
					{
						return false;
					}
					B.SetChecked(!B.isChecked(), true);
					this.CurFile.fix_nattr = !B.isChecked();
					return true;
				}
			});
			this.Bx.addP(new DsnDataP("", false)
			{
				text = TX.Get("Smnc_start_battle_seed", ""),
				size = 12f,
				TxCol = NEL.ColText,
				swidth = 80f
			}, false);
			this.LiSeed = this.Bx.addInput(new DsnDataInput
			{
				h = 28f,
				integer = true,
				bounds_w = 110f,
				min = 0.0,
				max = 2147483647.0,
				fnChangedDelay = delegate(LabeledInputField Li)
				{
					if (!this.isActive() || this.CurFile == null)
					{
						return false;
					}
					this.CurFile.rand_seed = (uint)X.NmI(Li.text, 0, false, false);
					return true;
				}
			});
			this.BRandomizeNattr.setNaviR(this.LiSeed, true, true);
			this.Bx.Br().addHr(new DsnDataHr
			{
				draw_width_rate = 0.6f,
				Col = NEL.ColText
			});
			this.Bx.Br();
			DsnDataButton dsnDataButton = new DsnDataButton
			{
				skin = "normal",
				w = 420f,
				h = 36f,
				locked_click = true,
				title = "&&Smnc_start_battle_submit",
				fnClick = new FnBtnBindings(this.fnClickSubmit)
			};
			this.BSubmit = this.Bx.addButtonT<aBtnNel>(dsnDataButton);
			this.Bx.Br();
			dsnDataButton.w = 300f;
			dsnDataButton.h = 24f;
			dsnDataButton.title = "&&Cancel";
			dsnDataButton.click_snd = "cancel";
			this.BCancel = this.Bx.addButtonT<aBtnNel>(dsnDataButton);
		}

		public void fnDescConvertDanger(STB Stb)
		{
			int num = Stb.NmI(0, -1, 0);
			Stb.Clear();
			Stb.AddTxA("Dangerousness_text", false).TxRpl(num);
		}

		private bool fnClickWeather(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			if (B.isChecked())
			{
				if (B.carr_index == 0)
				{
					this.BConWt.setValueBits(1U);
				}
				else
				{
					this.BConWt.Get(0).SetChecked(false, true);
					WeatherItem.WeatherDescription description = WeatherItem.getDescription((WeatherItem.WEATHER)B.carr_index);
					int num = 7;
					for (int i = 0; i < num; i++)
					{
						if (i != B.carr_index && (description.conflict & (1U << i)) != 0U)
						{
							this.BConWt.Get(i).SetChecked(false, true);
						}
					}
				}
			}
			else if (this.BConWt.getValueBits() == 0U)
			{
				this.BConWt.setValueBits(1U);
			}
			this.fineWeather(false);
			return true;
		}

		private void fineWeather(bool set_to_bcon = true)
		{
			uint num;
			if (!set_to_bcon)
			{
				num = this.BConWt.getValueBits();
				this.CurFile.weather_bits = num;
			}
			else
			{
				num = this.CurFile.weather_bits;
				this.BConWt.setValueBits(num);
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Dangerousness_weather2", false);
				using (STB stb2 = TX.PopBld(null, 0))
				{
					for (int i = 0; i < 7; i++)
					{
						if ((num & (1U << i)) != 0U)
						{
							STB stb3 = stb2;
							string text = "Weather_";
							WeatherItem.WEATHER weather = (WeatherItem.WEATHER)i;
							stb3.Append(TX.Get(text + weather.ToString().ToLower(), ""), ",");
						}
					}
					stb.TxRpl(stb2);
				}
				this.FbWt.Txt(stb);
			}
		}

		public void activate(SmncFile _CurFile)
		{
			this.CurFile = _CurFile;
			this.Bx.activate();
			SND.Ui.play("tool_hand_init", false);
			this.state = UiSmncBattleConfirm.STATE.CHOOSE;
			bool flag = true;
			using (STB stb = TX.PopBld(null, 0))
			{
				int num;
				if (this.CurFile.FindStgo("_smnc_generate_pr", out num).valid)
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[num];
					int num2 = this.LpArea.mapx + stgObject.x;
					int num3 = this.LpArea.mapy + stgObject.y;
					if (!this.Mp.canStand(num2, num3) || !this.Mp.canStand(num2, num3 + 1))
					{
						flag = false;
						stb.Append("", "\n");
						stb.Add(NEL.error_tag, "<img mesh=\"alert\" tx_color />").AddTxA("Smnc_alert_invalid_start", false).Add(NEL.error_tag_close);
					}
				}
				if (this.CurFile.Aen_list.Count == 0)
				{
					flag = false;
					stb.Append("", "\n");
					stb.Add(NEL.error_tag, "<img mesh=\"alert\" tx_color />").AddTxA("Smnc_alert_no_enemy", false).Add(NEL.error_tag_close);
				}
				if (!this.CurFile.FindStgo("_smnc_generate_en", out num).valid && !this.CurFile.FindStgo("_smnc_generate_en_fix", out num).valid)
				{
					stb.Append("<img mesh=\"alert\" tx_color />", "\n").AddTxA("Smnc_alert_no_enemypos", false);
				}
				if (this.CurFile.Aplant.Count == 0)
				{
					stb.Append("<img mesh=\"alert\" tx_color />", "\n").AddTxA("Smnc_alert_no_plants", false);
				}
				if (!this.CurFile.need_fine_nattr_valid)
				{
					stb.Append("<img mesh=\"alert\" tx_color />", "\n").AddTxA("Smnc_alert_wrong_attr", false);
				}
				this.FbError.Txt(stb);
				this.Bx.RowRemakeHeightRecalc(this.FbError, null);
				this.Bx.cropBounds(this.bx_w, this.bx_h);
			}
			this.BSliderDanger.setValue((float)this.CurFile.dangerousness, false);
			this.BRandomizeNattr.SetChecked(!this.CurFile.fix_nattr, true);
			if (this.CurFile.weather_bits == 0U)
			{
				this.CurFile.weather_bits = 1U;
			}
			this.LiSeed.setValue(this.CurFile.rand_seed.ToString(), false, false);
			this.fineWeather(true);
			(flag ? this.BSubmit : this.BCancel).Select(true);
			this.BSubmit.SetLocked(!flag, true, false);
		}

		public void deactivate()
		{
			if (this.isActive())
			{
				this.Bx.deactivate();
				this.state = UiSmncBattleConfirm.STATE.OFFLINE;
			}
		}

		private bool fnClickSubmit(aBtn B)
		{
			if (B.isLocked())
			{
				CURS.limitVib(B, AIM.R);
				SND.Ui.play("locked", false);
				return false;
			}
			if (B.title == "&&Cancel")
			{
				this.deactivate();
			}
			else if (B.title == "&&Smnc_start_battle_submit")
			{
				uint weather_bits = this.CurFile.weather_bits;
				int dangerousness = (int)this.CurFile.dangerousness;
				if (this.LpArea.summoner_openable)
				{
					if (this.LpArea.auto_save_on_opening_summoner && CFG.autosave_on_scenario)
					{
						COOK.autoSave(this.LpArea.nM2D, false, false);
					}
					this.LpArea.openSummoner(dangerousness, weather_bits);
					if (this.FD_BattleConfirm != null)
					{
						this.FD_BattleConfirm(dangerousness, weather_bits);
					}
				}
			}
			return true;
		}

		public bool run(float fcnt)
		{
			if (this.state == UiSmncBattleConfirm.STATE.OFFLINE)
			{
				return false;
			}
			if (this.state == UiSmncBattleConfirm.STATE.CHOOSE && IN.isCancel() && !IN.isMenuO(0))
			{
				SND.Ui.play("cancel", false);
				return false;
			}
			UiSmncBattleConfirm.STATE state = this.state;
			return true;
		}

		public bool isActive()
		{
			return this.state > UiSmncBattleConfirm.STATE.OFFLINE;
		}

		public bool isChooseState()
		{
			return this.state == UiSmncBattleConfirm.STATE.CHOOSE;
		}

		public Map2d Mp
		{
			get
			{
				return this.LpArea.Mp;
			}
		}

		public UiSmncBattleConfirm.FnBattleConfirm FD_BattleConfirm;

		public int pre_state0;

		public int pre_state1;

		public readonly UiBoxDesigner Bx;

		private readonly M2LpUiSmnCreator LpArea;

		private FillBlock FbError;

		private SmncFile CurFile;

		private readonly SmncStageEditorManager.TYPE target_type;

		private aBtnMeterNel BSliderDanger;

		private BtnContainer<aBtn> BConWt;

		private aBtn BRandomizeNattr;

		private LabeledInputField LiSeed;

		private FillBlock FbWt;

		private aBtn BSubmit;

		private aBtn BCancel;

		private UiSmncBattleConfirm.STATE state;

		public delegate bool FnBattleConfirm(int danger, uint weather);

		private enum STATE
		{
			OFFLINE,
			CHOOSE,
			BATTLE
		}
	}
}
