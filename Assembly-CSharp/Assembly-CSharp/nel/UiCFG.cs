using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using XX;

namespace nel
{
	public class UiCFG
	{
		public static ByteArray saveKeyConfigFileOnly()
		{
			ByteArray byteArray = ((UiCFG.BaRevertData != null) ? new ByteArray(UiCFG.BaRevertData, true) : CFG.createBinary(null));
			IN.getCurrentKeyAssignObject().getSaveString(byteArray);
			CFG.saveSdFile(byteArray);
			return byteArray;
		}

		public void ResetVariable()
		{
			this.SetValue("SEV", 80f);
			this.SetValue("VOV", 85f);
			this.SetValue("BGMV", 70f);
			this.SetValue("SE_stereo", 1f);
			this.SetValue("AutoSave", 3f);
			this.SetValue("Magsel", 4f);
			this.SetValue("MapEf_Level", 0f);
			this.SetValue("PostEf_Level", 0f);
			this.SetValue("UiEf_Level", 0f);
			this.SetValue("UiEf_TimeScale", 0f);
			this.SetValue("Draw_Fps", 0f);
			this.SetValue("vib_level", 100f);
			this.SetValue("Draw_EfFps", 0f);
			this.SetValue("UiEf_Density", 10f);
			this.SetValue("UiEf_Dirty", 5f);
			this.SetValue("UiEf_Dirty_Blood", 0f);
			this.SetValue("ui_sensitive_description", 5f);
			this.SetValue("Running_Double", 1f);
			this.SetValue("Running_Reverse", 0f);
			this.SetValue("Running_Stick_Thresh", 80f);
			this.SetValue("Shield_Hold_Evadable", 1f);
			this.SetValue("USel_Z_Push", 0f);
			this.SetValue("GO_Position", 1f);
			this.SetValue("MagSel_Decide", 0f);
			this.SetValue("UI_KD_appearance", 0f);
			IN.getCurrentKeyAssignObject().stick_threshold = 0.5f;
		}

		public UiCFG(UiBoxDesigner _Bx, UiBoxDesigner _BxDesc, Designer _DsSubmittion = null, bool _use_keycon = true, bool _show_difficulty = true, Action<Designer, string> FnDesignerCreateAfter = null)
		{
			this.BxOut = _Bx;
			this.BxDesc = _BxDesc;
			this.BaseTrs = this.BxOut.transform.parent;
			this.show_difficulty = _show_difficulty;
			this.pre_ui_effect_dirty = (int)CFG.ui_effect_dirty;
			this.pre_ui_effect_blood = CFG.blood_weaken;
			this.pre_ui_effect_density = CFG.ui_effect_density;
			this.pre_vsync = X.v_sync;
			this.pre_sensitive_level = X.sensitive_level;
			this.FD_fnChangeConfigValue = new aBtnMeter.FnMeterBindings(this.fnChangeConfigValue);
			this.FD_fnShowDesc = new FnBtnBindings(this.fnShowDesc);
			this.temp_window_size = Screen.width;
			this.DsSubmittion = _DsSubmittion;
			this.BxOut.alignx = ALIGN.CENTER;
			this.BxOut.item_margin_y_px = 10f;
			this.ui_state = UiCFG.STATE.INITTED;
			UiCFG.BaRevertData = CFG.createBinary(null);
			float use_w = _Bx.use_w;
			this.use_keycon = _use_keycon;
			this.enable_fullscreen = true;
			CFG.fullscreen_mode = Screen.fullScreen;
			this.BxOut.use_scroll = false;
			this.BxOut.use_button_connection = false;
			this.BxOut.init();
			if (CFGSP.isSpActivated())
			{
				this.DsTabSp = this.BxOut.addTab("-DsTabSp", this.BxOut.use_w, 30f, this.BxOut.use_w, 30f, true);
				this.DsTabSp.Smallest();
				this.DsTabSp.init();
				this.RTabSp = ColumnRow.CreateT<aBtnNel>(this.DsTabSp, "ctg_tab", "row_tab", 0, new string[]
				{
					"<img mesh=\"config_cog\" width=\"20\" height=\"24\" color=\"0x" + 4283780170U.ToString("x") + "\" />",
					"<img mesh=\"wholemap_bar\" width=\"20\" height=\"24\" color=\"0x" + 4283780170U.ToString("x") + "\" />"
				}, new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnCfgTopicChanged), this.DsTabSp.use_w, 0f, false, false);
				this.RTabSp.LR_valotile = true;
				this.BxOut.endTab(true);
			}
			float use_w2 = this.BxOut.use_w;
			float use_h = this.BxOut.use_h;
			this.OTab = new BDic<string, Designer>();
			this.OPreSelected = new BDic<string, aBtn>();
			Designer designer = this.BxOut.addTab("_DsmInner", use_w2, use_h, use_w2, use_h, true);
			this.setBoxMainStencil();
			this.createBoxDesignerContentMain();
			if (FnDesignerCreateAfter != null)
			{
				FnDesignerCreateAfter(designer, "MAIN");
			}
			this.BxOut.endTab(true);
			this.OTab["MAIN"] = designer;
			if (CFGSP.isSpActivated())
			{
				designer = this.BxOut.addTab("_DsmInner_Sp", use_w2, use_h, use_w2, use_h, true);
				this.setBoxMainStencil();
				CFGSP.createBoxDesignerContentSp(this, this.BxOut, designer, this.FD_fnShowDesc);
				if (FnDesignerCreateAfter != null)
				{
					FnDesignerCreateAfter(designer, "SP");
				}
				this.BxOut.endTab(true);
				this.OTab["SP"] = designer;
			}
			this.fineTabVisibility("MAIN", false, true);
		}

		public void setBoxMainStencil()
		{
			Designer currentAttachTarget = this.BxOut.CurrentAttachTarget;
			if (currentAttachTarget is UiBoxDesigner)
			{
				(currentAttachTarget as UiBoxDesigner).box_stencil_ref_mask = 239;
				currentAttachTarget.use_scroll = true;
				return;
			}
			currentAttachTarget.stencil_ref = 238;
			currentAttachTarget.margin_in_tb = 0f;
			currentAttachTarget.margin_in_lr = 22f;
			currentAttachTarget.item_margin_x_px = this.BxOut.item_margin_x_px;
			currentAttachTarget.item_margin_y_px = this.BxOut.item_margin_y_px;
			currentAttachTarget.scrolling_margin_in_lr = (currentAttachTarget.scrolling_margin_in_tb = 8f);
			currentAttachTarget.use_scroll = true;
			currentAttachTarget.alignx = ALIGN.CENTER;
			currentAttachTarget.selectable_loop = 2;
			currentAttachTarget.use_button_connection = true;
			currentAttachTarget.init();
		}

		public UiBoxDesigner P(string title, bool enable = true)
		{
			this.BxOut.Br().addP(new DsnDataP("", false)
			{
				text = TX.Get(title, ""),
				size = 18f * (X.ENG_MODE ? 0.7f : 1f),
				alignx = ALIGN.CENTER,
				Col = MTRX.ColTrnsp,
				TxCol = (enable ? C32.d2c(4283780170U) : C32.d2c(4288057994U)),
				swidth = 140f,
				sheight = 0f,
				text_auto_condense = true,
				html = false
			}, false);
			return this.BxOut;
		}

		public float sliderw
		{
			get
			{
				return this.BxOut.get_swidth_px() / 2f - this.BxOut.margin_in_lr - this.BxOut.scrolling_margin_in_lr - 8f - 104f;
			}
		}

		public float sliderw_sml
		{
			get
			{
				return this.sliderw - 100f;
			}
		}

		public float sliderw_middle
		{
			get
			{
				return this.sliderw - 60f;
			}
		}

		private void createBoxDesignerContentMain()
		{
			this.P("Config_Windowed", this.enable_fullscreen).addSliderCT(new DsnDataSlider
			{
				name = "Windowed",
				title = "Windowed",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(CFG.fullscreen_mode ? 1 : 0),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_Windowed_", 2, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			if (!this.enable_fullscreen)
			{
				(this.BxOut.Get("Windowed", false) as aBtnMeter).SetLocked(true, true, false);
			}
			this.Ascreen_width_desc = new string[this.Ascreen_width.Length];
			int num = X.isinS<int>(this.Ascreen_width, 1280);
			int num2 = -1;
			for (int i = 0; i < this.Ascreen_width.Length; i++)
			{
				int num3 = this.Ascreen_width[i];
				string text = num3.ToString() + "x" + (num3 * 9 / 16).ToString();
				if ((float)((int)((float)num3 / IN.w)) == (float)num3 / IN.w)
				{
					text += TX.Get("window_size_recomennded", "");
				}
				this.Ascreen_width_desc[i] = text;
				int num4 = X.Abs(num3 - Screen.width);
				if (num2 < 0 || num4 < num2)
				{
					num2 = num4;
					num = i;
				}
			}
			this.temp_window_size = this.Ascreen_width[num];
			this.P("Config_Window_Size", this.enable_fullscreen).addSliderCT(new DsnDataSlider
			{
				name = "Window_Size",
				title = "Window_Size",
				skin_title = "",
				def = (float)num,
				mn = 0f,
				mx = (float)(this.Ascreen_width_desc.Length - 1),
				valintv = 1f,
				w = this.sliderw_middle,
				Adesc_keys = this.Ascreen_width_desc,
				fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
				{
					int num5 = this.Ascreen_width[index];
					if ((float)((int)((float)num5 / IN.w)) != (float)num5 / IN.w)
					{
						return 0.5f;
					}
					return 1f;
				},
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			using (BList<string> blist = TX.listUpFamilyName(ListBuffer<string>.Pop(0), true))
			{
				this.P("Config_Lang", true).addSliderCT(new DsnDataSlider
				{
					name = "Lang",
					title = "Lang",
					skin_title = "&&Config_Lang",
					checkbox_mode = 2,
					mx = (float)(blist.Count - 1),
					def = (float)TX.getCurrentFamilyIndex(),
					w = this.sliderw_sml,
					Adesc_keys = blist.ToArray(),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			this.P("Config_Sensitive", true).addSliderCT(new DsnDataSlider
			{
				name = "Sensitive",
				title = "Sensitive",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)X.sensitive_level,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_Sensitive_", 3, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_SEV", true).addSliderCT(new DsnDataSlider
			{
				name = "SEV",
				title = "SEV",
				skin_title = "",
				def = (float)SND.volume,
				mn = 0f,
				mx = 100f,
				valintv = 5f,
				w = this.sliderw,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 114f, null, false);
			this.P("Config_VOV", true).addSliderCT(new DsnDataSlider
			{
				name = "VOV",
				title = "VOV",
				skin_title = "",
				def = (float)SND.voice_volume,
				mn = 0f,
				mx = 100f,
				valintv = 5f,
				w = this.sliderw,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 114f, null, false);
			this.P("Config_BGMV", true).addSliderCT(new DsnDataSlider
			{
				name = "BGMV",
				title = "BGMV",
				skin_title = "",
				def = (float)SND.bgm_volume,
				mn = 0f,
				mx = 100f,
				valintv = 5f,
				w = this.sliderw,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 114f, null, false);
			this.P("Config_SE_stereo", true).addSliderCT(new DsnDataSlider
			{
				name = "SE_stereo",
				title = "SE_stereo",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(M2SoundPlayer.monoral ? 0 : 1),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_SE_stereo_", 2, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			if (this.show_difficulty)
			{
				this.P("Config_Difficulty", true).addSliderCT(new DsnDataSlider
				{
					name = "Difficulty",
					title = "Difficulty",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)DIFF.I,
					mn = 0f,
					mx = 2f,
					Adesc_keys = new string[]
					{
						TX.Get("Title_difficulty_cas", ""),
						TX.Get("Title_difficulty_nor", ""),
						TX.Get("Title_difficulty_pro", "")
					},
					valintv = 1f,
					w = this.sliderw_sml,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 254f, null, false);
			}
			this.P("Config_Draw_Fps", true).addSliderCT(new DsnDataSlider
			{
				name = "Draw_Fps",
				title = "Draw_Fps",
				skin_title = "",
				def = (float)(X.AF - 1),
				mn = 0f,
				mx = 3f,
				Adesc_keys = TX.GetArrayCountUp("Config_Draw_Fps_", 4, 0),
				valintv = 1f,
				w = this.sliderw_sml,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_Draw_EfFps", true).addSliderCT(new DsnDataSlider
			{
				name = "Draw_EfFps",
				title = "Draw_EfFps",
				skin_title = "",
				def = (float)(X.AF_EF - 1),
				mn = 0f,
				mx = 3f,
				Adesc_keys = TX.GetArrayCountUp("Config_Draw_EfFps_", 4, 0),
				valintv = 1f,
				w = this.sliderw_sml,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_VSync", true).addSliderCT(new DsnDataSlider
			{
				name = "VSync",
				title = "VSync",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(X.v_sync ? 1 : 0),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("Disabled", "Enabled"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_MapEf_Level", true).addSliderCT(new DsnDataSlider
			{
				name = "MapEf_Level",
				title = "MapEf_Level",
				skin_title = "",
				def = (float)(100 - X.IntR(X.EF_LEVEL_NORMAL * 100f)),
				mn = 0f,
				mx = 100f,
				fnDescConvert = new FnDescConvert(this.fnDescConvertEffectRestrict),
				valintv = 10f,
				w = this.sliderw_middle,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			this.P("Config_PostEf_Level", true).addSliderCT(new DsnDataSlider
			{
				name = "PostEf_Level",
				title = "PostEf_Level",
				skin_title = "",
				def = (float)(CFG.posteffect_weaken * 10),
				mn = 0f,
				mx = 100f,
				fnDescConvert = new FnDescConvert(this.fnDescConvertEffectRestrict),
				valintv = 10f,
				w = this.sliderw_middle,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			this.P("Config_UiEf_Level", true).addSliderCT(new DsnDataSlider
			{
				name = "UiEf_Level",
				title = "UiEf_Level",
				skin_title = "",
				def = (float)(100 - X.IntR(X.EF_LEVEL_UI * 100f)),
				mn = 0f,
				mx = 100f,
				fnDescConvert = new FnDescConvert(this.fnDescConvertEffectRestrict),
				valintv = 10f,
				w = this.sliderw_middle,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			this.P("Config_UiEf_TimeScale", true).addSliderCT(new DsnDataSlider
			{
				name = "UiEf_TimeScale",
				title = "UiEf_TimeScale",
				skin_title = "",
				def = (float)(100 - X.IntR(X.EF_TIMESCALE_UI * 100f)),
				mn = 0f,
				mx = 100f,
				fnDescConvert = new FnDescConvert(this.fnDescConvertEffectRestrict),
				valintv = 10f,
				w = this.sliderw_middle,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			this.P("Config_UiEf_Dirty", true).addSliderCT(new DsnDataSlider
			{
				name = "UiEf_Dirty",
				title = "UiEf_Dirty",
				skin_title = "",
				def = (float)CFG.ui_effect_dirty,
				mn = 0f,
				mx = 10f,
				valintv = 1f,
				w = this.sliderw,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 114f, null, false);
			UiBoxDesigner uiBoxDesigner = this.P("Config_UiEf_Density", true);
			DsnDataSlider dsnDataSlider = new DsnDataSlider();
			dsnDataSlider.name = "UiEf_Density";
			dsnDataSlider.title = "UiEf_Density";
			dsnDataSlider.skin_title = "";
			dsnDataSlider.def = (float)CFG.ui_effect_density;
			dsnDataSlider.mn = 0f;
			dsnDataSlider.mx = 20f;
			dsnDataSlider.valintv = 1f;
			dsnDataSlider.w = this.sliderw;
			dsnDataSlider.fnDescConvert = new FnDescConvert(this.fnDescConvertDensity);
			dsnDataSlider.fnChanged = this.FD_fnChangeConfigValue;
			dsnDataSlider.fnHover = this.FD_fnShowDesc;
			dsnDataSlider.fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
			{
				int num6 = (int)val;
				if (num6 != 0)
				{
					if (num6 == 10)
					{
						return 0.66f;
					}
					if (num6 != 20)
					{
						return 0.33f;
					}
				}
				return 1f;
			};
			uiBoxDesigner.addSliderCT(dsnDataSlider, 114f, null, false);
			this.P("Config_UiEf_Dirty_Blood", true).addSliderCT(new DsnDataSlider
			{
				name = "UiEf_Dirty_Blood",
				title = "UiEf_Dirty_Blood",
				skin_title = "",
				def = (float)CFG.blood_weaken,
				mn = 0f,
				mx = (float)CFG.BLOOD_WEAKEN_MAX,
				fnDescConvert = new FnDescConvert(this.fnDescConvertEffectRestrict),
				valintv = 10f,
				w = this.sliderw_middle,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 154f, null, false);
			this.P("Config_ui_sensitive_description", true).addSliderCT(new DsnDataSlider
			{
				name = "ui_sensitive_description",
				title = "ui_sensitive_description",
				skin_title = "",
				def = (float)CFG.ui_sensitive_description,
				mn = 0f,
				mx = 10f,
				valintv = 1f,
				w = this.sliderw,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 114f, null, false);
			this.P("Config_GO_Position", true).addSliderCT(new DsnDataSlider
			{
				name = "GO_Position",
				title = "GO_Position",
				skin_title = "",
				def = (float)(CFG.go_text_pos_top ? 1 : 0),
				checkbox_mode = 1,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_GO_Position_", 2, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_AutoSave", true).addSliderCT(new DsnDataSlider
			{
				name = "AutoSave",
				title = "AutoSave",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)CFG.autosave_timing,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_AutoSave_", 4, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_Magsel", true).addSliderCT(new DsnDataSlider
			{
				name = "Magsel",
				title = "Magsel",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)CFG.magsel_slow,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArrayCountUp("Config_Magsel_", 5, 0),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, true);
			this.P("Config_MagSel_Decide", true).addSliderCT(new DsnDataSlider
			{
				name = "MagSel_Decide",
				title = "MagSel_Decide",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)CFG.magsel_decide_type,
				w = this.sliderw_sml,
				Adesc_keys = new string[]
				{
					TX.Get("Config_MagSel_Decide_0", ""),
					TX.GetA("Config_MagSel_Decide_1", TX.Get("Submit", "")),
					TX.Get("Config_MagSel_Decide_2", ""),
					TX.Get("Config_MagSel_Decide_3", "")
				},
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, true);
			this.P("Config_Running_Double", true).addSliderCT(new DsnDataSlider
			{
				name = "Running_Double",
				title = "Running_Double",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(M2MoverPr.double_tap_running ? 1 : 0),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("Disabled", "Enabled"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, true);
			this.P("Config_Running_Reverse", true).addSliderCT(new DsnDataSlider
			{
				name = "Running_Reverse",
				title = "Running_Reverse",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(M2MoverPr.jump_press_reverse ? 1 : 0),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("Disabled", "Enabled"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_Running_Stick_Thresh", true).addSliderCT(new DsnDataSlider
			{
				name = "Running_Stick_Thresh",
				title = "Running_Stick_Thresh",
				skin_title = "",
				def = (float)X.IntR(M2MoverPr.running_thresh * 100f),
				mn = 10f,
				mx = 100f,
				valintv = 10f,
				w = this.sliderw_sml,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc,
				fnDescConvert = new FnDescConvert(this.fnDescConvertRunningThresh)
			}, 214f, null, false);
			this.P("Config_Shield_Hold_Evadable", true).addSliderCT(new DsnDataSlider
			{
				name = "Shield_Hold_Evadable",
				title = "Shield_Hold_Evadable",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)(CFG.shield_hold_evadable ? 1 : 0),
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("Disabled", "Enabled"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, false);
			this.P("Config_USel_Z_Push", true).addSliderCT(new DsnDataSlider
			{
				name = "USel_Z_Push",
				title = "USel_Z_Push",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)CFG.item_usel_type,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("USel_D_Release", "USel_Z_Push", "USel_D_Tap_Z"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, true);
			this.P("Config_vib_level", true).addSliderCT(new DsnDataSlider
			{
				name = "vib_level",
				title = "vib_level",
				skin_title = "",
				def = (float)CFG.vib_level,
				mn = 0f,
				mx = 100f,
				valintv = 10f,
				w = this.sliderw_sml,
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc,
				fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
			}, 214f, null, true);
			this.P("Config_UI_KD_appearance", true).addSliderCT(new DsnDataSlider
			{
				name = "UI_KD_appearance",
				title = "UI_KD_appearance",
				skin_title = "",
				checkbox_mode = 1,
				def = (float)KEY.keydesc_appearance,
				w = this.sliderw_sml,
				Adesc_keys = TX.GetArray("Config_UI_KD_appearance_0", "Config_UI_KD_appearance_1", "Config_UI_KD_appearance_2"),
				fnChanged = this.FD_fnChangeConfigValue,
				fnHover = this.FD_fnShowDesc
			}, 214f, null, true);
			CFG.temp_kisekae = MGV.temp_kisekae;
			if (MGV.temp_kisekae_max > 0)
			{
				this.BxOut.Hr(0.6f, 18f, 26f, 1f);
				this.P("Config_Kisekae", true).addSliderCT(new DsnDataSlider
				{
					name = "Kisekae",
					title = "Kisekae",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)CFG.temp_kisekae,
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArrayCountUp("Config_Kisekae_", MGV.temp_kisekae_max + 1, 0),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			string text2 = TX.Get("Stick_sensitivity_adjust", "");
			this.BxOut.Hr(0.6f, 18f, 26f, 1f).addButton(new DsnDataButton
			{
				name = "stick_sensitivity",
				title = "stick_sensitivity",
				skin_title = text2,
				w = 319.8f,
				h = 32f * (float)TX.countLine(text2),
				fnClick = new FnBtnBindings(this.fnClickRBtn),
				fnHover = this.FD_fnShowDesc,
				click_snd = "enter_small"
			});
			this.BxOut.Br().addButton(new DsnDataButton
			{
				name = "ResetAll",
				title = "ResetAll",
				skin_title = TX.Get("Bench_Cmd_reset_all", ""),
				w = 246f,
				h = 32f,
				fnClick = delegate(aBtn B)
				{
					this.ResetVariable();
					IN.clearPushDown(true);
					return true;
				},
				fnHover = this.FD_fnShowDesc,
				click_snd = "reset_var"
			});
			if (this.use_keycon)
			{
				string text3 = TX.Get("Keyconfig_title", "");
				this.BxOut.Br().addButton(new DsnDataButton
				{
					name = "keycon",
					title = "keycon",
					skin_title = text3,
					w = 246f,
					h = 32f * (float)TX.countLine(text3),
					fnClick = new FnBtnBindings(this.fnClickRBtn),
					fnHover = this.FD_fnShowDesc
				});
			}
			this.MainBoxRelink();
		}

		private void MainBoxRelink()
		{
			this.BxOut.fineNaviConnection();
			if (this.CurShowingTab != null)
			{
				this.CurShowingTab.fineNaviConnection();
				if (this.DsSubmittion != null)
				{
					this.CurShowingTab.ConnectVertical(this.DsSubmittion, true, true, true);
				}
			}
		}

		private void saveCurrentSelectingOnTab()
		{
			string text;
			this.getCurrentShowingTab(out text);
			if (TX.valid(text))
			{
				if (aBtn.PreSelected == null)
				{
					this.OPreSelected[text] = null;
					return;
				}
				if (this.CurShowingTab.isContainElement(aBtn.PreSelected))
				{
					this.OPreSelected[text] = aBtn.PreSelected;
				}
			}
		}

		private void fnDescConvertEffectRestrict(STB Stb)
		{
			if (Stb.Equals("0"))
			{
				Stb.SetTxA("Config_Effect_No_Restriction", false);
				return;
			}
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				int num = (int)STB.NmRes(parseres, -1.0);
				Stb.Clear().Add(100f - X.Nm(num, 0f)).Add("%");
			}
		}

		private void fnDescConvertDensity(STB Stb)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				float num = (float)STB.NmRes(parseres, -1.0);
				Stb.Clear();
				Stb.Add(X.IntR(num / 10f * 100f)).Add("%");
			}
		}

		private void fnDescConvertRunningThresh(STB Stb)
		{
			if (Stb.Equals("100"))
			{
				Stb.SetTxA("Disabled", false);
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Set(Stb).Add('%');
				Stb.SetTxA("Config_val_sensitivity", false).TxRpl(stb);
			}
		}

		public void fnDescConvertRunningPersent(STB Stb)
		{
			Stb.Add("%");
		}

		private void SetValue(string name, float value)
		{
			aBtnMeter aBtnMeter = this.BxOut.Get(name, false) as aBtnMeter;
			if (aBtnMeter != null)
			{
				aBtnMeter.setValueAndCallFunc(value, false);
				return;
			}
			X.de("empty field:" + name, null);
		}

		private bool fnChangeConfigValue(aBtnMeter _B, float pre_value, float cur_value)
		{
			return !_B.isLocked() && UiCFG.changeConfigValue(_B.title, cur_value, this);
		}

		public static bool changeConfigValue(string key, float cur_value, UiCFG CfgInstance = null)
		{
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 2287149779U)
				{
					if (num <= 1391843111U)
					{
						if (num <= 731969625U)
						{
							if (num <= 71514319U)
							{
								if (num != 65495999U)
								{
									if (num != 71514319U)
									{
										goto IL_07BD;
									}
									if (!(key == "UiEf_Density"))
									{
										goto IL_07BD;
									}
									CFG.ui_effect_density = (byte)cur_value;
									return true;
								}
								else
								{
									if (!(key == "GO_Position"))
									{
										goto IL_07BD;
									}
									CFG.go_text_pos_top = cur_value != 0f;
									return true;
								}
							}
							else if (num != 716595213U)
							{
								if (num != 731969625U)
								{
									goto IL_07BD;
								}
								if (!(key == "UiEf_Level"))
								{
									goto IL_07BD;
								}
								X.EF_LEVEL_UI = 1f - cur_value / 100f;
								return true;
							}
							else
							{
								if (!(key == "MagSel_Decide"))
								{
									goto IL_07BD;
								}
								CFG.magsel_decide_type = (CFG.MAGSEL_TYPE)cur_value;
								return true;
							}
						}
						else if (num <= 1258391049U)
						{
							if (num != 747116006U)
							{
								if (num != 1258391049U)
								{
									goto IL_07BD;
								}
								if (!(key == "SEV"))
								{
									goto IL_07BD;
								}
								SND.volume = X.IntR(cur_value);
								if (M2DBase.Instance != null)
								{
									M2DBase.Instance.Snd.fineVolume();
								}
								SND.Ui.play("talk_progress", false);
								return true;
							}
							else
							{
								if (!(key == "VSync"))
								{
									goto IL_07BD;
								}
								X.v_sync = cur_value != 0f;
								return true;
							}
						}
						else if (num != 1363510949U)
						{
							if (num != 1391843111U)
							{
								goto IL_07BD;
							}
							if (!(key == "Window_Size"))
							{
								goto IL_07BD;
							}
							if (CfgInstance != null)
							{
								CfgInstance.temp_window_size = CfgInstance.Ascreen_width[X.IntR(cur_value)];
								return true;
							}
							return true;
						}
						else if (!(key == "BGMV"))
						{
							goto IL_07BD;
						}
					}
					else if (num <= 1701121667U)
					{
						if (num <= 1447527407U)
						{
							if (num != 1409041749U)
							{
								if (num != 1447527407U)
								{
									goto IL_07BD;
								}
								if (!(key == "autorun"))
								{
									goto IL_07BD;
								}
								goto IL_0759;
							}
							else
							{
								if (!(key == "PostEf_Level"))
								{
									goto IL_07BD;
								}
								CFG.posteffect_weaken = (byte)(cur_value / 10f);
								return true;
							}
						}
						else if (num != 1574728676U)
						{
							if (num != 1701121667U)
							{
								goto IL_07BD;
							}
							if (!(key == "Draw_Fps"))
							{
								goto IL_07BD;
							}
							X.AF = (int)(cur_value + 1f);
							if (X.AF_EF >= X.AF)
							{
								return true;
							}
							X.AF_EF = X.AF;
							if (CfgInstance != null)
							{
								CfgInstance.BxOut.Get("Draw_EfFps", false).setValue((X.AF_EF - 1).ToString());
								return true;
							}
							return true;
						}
						else
						{
							if (!(key == "Running_Double"))
							{
								goto IL_07BD;
							}
							M2MoverPr.double_tap_running = cur_value > 0.5f;
							return true;
						}
					}
					else if (num <= 2022456398U)
					{
						if (num != 1778072347U)
						{
							if (num != 2022456398U)
							{
								goto IL_07BD;
							}
							if (!(key == "UI_KD_appearance"))
							{
								goto IL_07BD;
							}
							KEY.keydesc_appearance = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "Shield_Hold_Evadable"))
							{
								goto IL_07BD;
							}
							CFG.shield_hold_evadable = cur_value != 0f;
							return true;
						}
					}
					else if (num != 2233508368U)
					{
						if (num != 2258767131U)
						{
							if (num != 2287149779U)
							{
								goto IL_07BD;
							}
							if (!(key == "AutoSave"))
							{
								goto IL_07BD;
							}
							CFG.autosave_timing = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "vib_level"))
							{
								goto IL_07BD;
							}
							CFG.vib_level = (byte)cur_value;
							return true;
						}
					}
					else
					{
						if (!(key == "Difficulty"))
						{
							goto IL_07BD;
						}
						DIFF.I = X.IntR(cur_value);
						return true;
					}
				}
				else if (num <= 3163520979U)
				{
					if (num <= 2702396622U)
					{
						if (num <= 2499870687U)
						{
							if (num != 2415980917U)
							{
								if (num != 2499870687U)
								{
									goto IL_07BD;
								}
								if (!(key == "UiEf_Dirty"))
								{
									goto IL_07BD;
								}
								CFG.ui_effect_dirty = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "ui_sensitive_description"))
								{
									goto IL_07BD;
								}
								CFG.ui_sensitive_description = (byte)cur_value;
								return true;
							}
						}
						else if (num != 2695456225U)
						{
							if (num != 2702396622U)
							{
								goto IL_07BD;
							}
							if (!(key == "stick_thresh"))
							{
								goto IL_07BD;
							}
							M2MoverPr.running_thresh = cur_value;
							return true;
						}
						else
						{
							if (!(key == "Running_Reverse"))
							{
								goto IL_07BD;
							}
							goto IL_0759;
						}
					}
					else if (num <= 3086297070U)
					{
						if (num != 3070305732U)
						{
							if (num != 3086297070U)
							{
								goto IL_07BD;
							}
							if (!(key == "USel_Z_Push"))
							{
								goto IL_07BD;
							}
							CFG.item_usel_type = (CFG.USEL_TYPE)cur_value;
							return true;
						}
						else
						{
							if (!(key == "Magsel"))
							{
								goto IL_07BD;
							}
							CFG.magsel_slow = (byte)cur_value;
							return true;
						}
					}
					else if (num != 3132036350U)
					{
						if (num != 3155854328U)
						{
							if (num != 3163520979U)
							{
								goto IL_07BD;
							}
							if (!(key == "MapEf_Level"))
							{
								goto IL_07BD;
							}
							X.EF_LEVEL_NORMAL = 1f - cur_value / 100f;
							return true;
						}
						else
						{
							if (!(key == "Kisekae"))
							{
								goto IL_07BD;
							}
							CFG.temp_kisekae = (int)((byte)cur_value);
							return true;
						}
					}
					else
					{
						if (!(key == "SE_stereo"))
						{
							goto IL_07BD;
						}
						M2SoundPlayer.monoral = cur_value == 0f;
						return true;
					}
				}
				else if (num <= 3271972118U)
				{
					if (num <= 3235683140U)
					{
						if (num != 3210407456U)
						{
							if (num != 3235683140U)
							{
								goto IL_07BD;
							}
							if (!(key == "Windowed"))
							{
								goto IL_07BD;
							}
							CFG.fullscreen_mode = cur_value != 0f;
							return true;
						}
						else
						{
							if (!(key == "UiEf_Dirty_Blood"))
							{
								goto IL_07BD;
							}
							CFG.blood_weaken = (byte)cur_value;
							return true;
						}
					}
					else if (num != 3237703372U)
					{
						if (num != 3271972118U)
						{
							goto IL_07BD;
						}
						if (!(key == "Draw_EfFps"))
						{
							goto IL_07BD;
						}
						X.AF_EF = (int)(cur_value + 1f);
						return true;
					}
					else
					{
						if (!(key == "VOV"))
						{
							goto IL_07BD;
						}
						SND.voice_volume = X.IntR(cur_value);
						return true;
					}
				}
				else if (num <= 3377067378U)
				{
					if (num != 3317469770U)
					{
						if (num != 3377067378U)
						{
							goto IL_07BD;
						}
						if (!(key == "Running_Stick_Thresh"))
						{
							goto IL_07BD;
						}
						M2MoverPr.running_thresh = cur_value / 100f;
						return true;
					}
					else
					{
						if (!(key == "UiEf_TimeScale"))
						{
							goto IL_07BD;
						}
						X.EF_TIMESCALE_UI = 1f - cur_value / 100f;
						return true;
					}
				}
				else if (num != 4044785525U)
				{
					if (num != 4127457007U)
					{
						if (num != 4241328060U)
						{
							goto IL_07BD;
						}
						if (!(key == "bgm_volume"))
						{
							goto IL_07BD;
						}
					}
					else
					{
						if (!(key == "Sensitive"))
						{
							goto IL_07BD;
						}
						X.sensitive_level = (byte)cur_value;
						return true;
					}
				}
				else
				{
					if (!(key == "Lang"))
					{
						goto IL_07BD;
					}
					TX.changeFamilyToIndex((int)cur_value);
					if (CfgInstance != null)
					{
						CfgInstance.fnShowDesc(CfgInstance.BxOut.getBtn("Lang"));
						return true;
					}
					return true;
				}
				SND.bgm_volume = X.IntR(cur_value);
				return true;
				IL_0759:
				M2MoverPr.jump_press_reverse = cur_value > 0.5f;
				return true;
			}
			IL_07BD:
			X.de("不明なコンフィグ指定: " + key, null);
			return true;
		}

		private bool fnShowDesc(aBtn B)
		{
			if (this.BxDesc == null)
			{
				return true;
			}
			string text = null;
			string title = B.title;
			if (title != null && title == "VSync")
			{
				text = TX.GetA("Config_desc_" + B.title, IN.enable_vsync ? TX.Get("Enabled", "") : TX.Get("Disabled", ""));
			}
			else if (!CFGSP.getSpecialDesc(B.title, out text))
			{
				text = TX.Get("Config_desc_" + B.title, "");
			}
			if (TX.noe(text))
			{
				this.BxDesc.positionD(this.BxDesc.getBox().get_deperture_x(), this.BxDesc.getBox().get_deperture_y(), 2, 40f);
				this.BxDesc.deactivate();
			}
			else
			{
				FillBlock fillBlock = this.BxDesc.Get("__CFG_DESC_P", false) as FillBlock;
				float num = ((TX.countLine(text) >= 6) ? 0.94f : 1.16f);
				if (fillBlock == null)
				{
					this.BxDesc.Clear();
					this.BxDesc.margin_in_lr = 40f;
					this.BxDesc.margin_in_tb = 0f;
					this.BxDesc.WH(380f, 120f);
					this.BxDesc.alignx = ALIGN.LEFT;
					this.BxDesc.addP(new DsnDataP("", false)
					{
						text = text,
						name = "__CFG_DESC_P",
						size = 16f,
						alignx = ALIGN.CENTER,
						aligny = ALIGNY.MIDDLE,
						Col = MTRX.ColTrnsp,
						TxCol = C32.d2c(4283780170U),
						swidth = this.BxDesc.use_w,
						text_auto_wrap = true,
						sheight = this.BxDesc.use_h,
						lineSpacing = num,
						html = true
					}, false);
				}
				else
				{
					fillBlock.lineSpacing = num;
					fillBlock.text_content = text;
				}
				Vector3 localPosFromContainer = B.get_Skin().getLocalPosFromContainer();
				localPosFromContainer.x = (this.BxOut.swidth / 2f + 190f + 4f) * 0.015625f;
				Vector3 vector = localPosFromContainer;
				vector.x = vector.x * 64f + this.BxOut.getBox().get_deperture_x();
				vector.y = vector.y * 64f + this.BxOut.getBox().get_deperture_y();
				if (this.BxDesc.isActive())
				{
					this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
				}
				else
				{
					this.BxDesc.activate();
					this.BxDesc.positionD(vector.x, vector.y, 2, 40f);
				}
			}
			return true;
		}

		private bool fnClickRBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "keycon"))
				{
					if (title == "stick_sensitivity")
					{
						this.OPreSelected["MAIN"] = B;
						this.initStickSensitivityCheck();
					}
				}
				else
				{
					this.ui_state = UiCFG.STATE.KEYCON;
					this.OPreSelected["MAIN"] = B;
				}
			}
			return true;
		}

		public UiCFG destruct()
		{
			UiCFG.BaRevertData = null;
			this.BxDesc = (this.BxOut = null);
			this.DsSubmittion = null;
			Designer.EvacuateMem.Destroy(this.AEvcTab);
			return null;
		}

		public void deactivateDesigner()
		{
			IN.clearPushDown(true);
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			this.saveCurrentSelectingOnTab();
			if (nelM2DBase != null)
			{
				PRNoel prNoel = nelM2DBase.getPrNoel();
				if (X.SENSITIVE && prNoel.BetoMng.is_torned)
				{
					prNoel.BetoMng.setTorned(prNoel, false, true);
				}
				if (this.pre_sensitive_level != X.sensitive_level && prNoel.UP != null)
				{
					prNoel.UP.force_recheck_allocate = true;
				}
				if (UIPicture.Instance != null && (this.pre_ui_effect_dirty != (int)CFG.ui_effect_dirty || this.pre_ui_effect_blood != CFG.blood_weaken || this.pre_ui_effect_density != CFG.ui_effect_density) && BetobetoManager.redrawAll())
				{
					UIPicture.Instance.activateTextureUpdatingHide(true);
				}
				this.pre_ui_effect_dirty = (int)CFG.ui_effect_dirty;
				this.pre_ui_effect_blood = CFG.blood_weaken;
				this.pre_ui_effect_density = CFG.ui_effect_density;
			}
			this.pre_sensitive_level = X.sensitive_level;
			if (X.v_sync != this.pre_vsync)
			{
				this.pre_vsync = X.v_sync;
				MGV.saveSdFile(null);
			}
			if (this.BxOut == null)
			{
				return;
			}
			if (this.BxDesc != null)
			{
				this.BxDesc.deactivate();
			}
			if (this.ui_state == UiCFG.STATE.STICK_SENSITIVE || this.ui_state == UiCFG.STATE.STICK_SENSITIVE_AUTO)
			{
				this.quitStickSensitivityCheck();
			}
			if (this.ui_state == UiCFG.STATE.KEYCON)
			{
				this.OPreSelected["MAIN"] = this.BxOut.getBtn("keycon");
			}
			else if (aBtn.PreSelected != null)
			{
				this.saveCurrentSelectingOnTab();
			}
			if (this.enable_fullscreen && (CFG.fullscreen_mode != Screen.fullScreen || Screen.width != this.temp_window_size))
			{
				IN.setFullScreenMode(CFG.fullscreen_mode, (float)this.temp_window_size / IN.w);
			}
		}

		public void resume()
		{
			this.ui_state = UiCFG.STATE.INITTED;
			this.BxOut.Focus();
			aBtn aBtn = this.getFirstFocusButton();
			if (aBtn == null)
			{
				aBtn = this.CurShowingTab.getBtn(0);
			}
			if (aBtn != null)
			{
				aBtn.Select(true);
			}
		}

		public aBtn getFirstFocusButton()
		{
			string text;
			return this.getFirstFocusButton(out text);
		}

		public aBtn getFirstFocusButton(out string tabname)
		{
			tabname = null;
			string text;
			if (this.CurShowingTab != null && X.GetKeyByValue<string, Designer>(this.OTab, this.CurShowingTab, out text))
			{
				tabname = text;
				aBtn aBtn = X.Get<string, aBtn>(this.OPreSelected, text);
				if (aBtn != null)
				{
					return aBtn;
				}
			}
			return null;
		}

		public bool runBoxDesignerEdit()
		{
			Bench.P("CFG UI");
			bool flag = true;
			if (this.ui_state == UiCFG.STATE.INITTED)
			{
				this.ui_state = UiCFG.STATE.ACTIVE;
				if (this.BxOut != null)
				{
					if (!this.BxOut.isActive())
					{
						this.BxOut.activate();
					}
					this.BxOut.Focus();
				}
				string text;
				aBtn aBtn = this.getFirstFocusButton(out text);
				if (aBtn != null)
				{
					if (!aBtn.isLocked())
					{
						aBtn.Select(true);
					}
					else
					{
						aBtn = null;
					}
				}
				if (aBtn == null)
				{
					aBtn aBtn2 = this.BxOut.Get(this.enable_fullscreen ? "Windowed" : "SEV", false) as aBtn;
					if (aBtn2 != null)
					{
						aBtn2.Select(true);
					}
				}
			}
			else if (this.ui_state == UiCFG.STATE.ACTIVE)
			{
				if (IN.isCancel() && !CtSetter.hasFocus())
				{
					flag = false;
				}
			}
			else if (this.ui_state == UiCFG.STATE.STICK_SENSITIVE_AUTO)
			{
				if (IN.isCancel() || this.runAutoStickSensitivity())
				{
					this.initStickSensitivityCheck(UiCFG.STATE.STICK_SENSITIVE);
				}
			}
			else if (this.ui_state == UiCFG.STATE.STICK_SENSITIVE)
			{
				if (IN.isCancel() || this.runSensitiveSelect())
				{
					this.quitStickSensitivityCheck();
				}
				else if (!IN.isPadMode())
				{
					Designer tab = this.CurShowingTab.getTab("_sensitive_btns");
					aBtn aBtn3 = null;
					if (tab != null && (IN.isL() || IN.isR()))
					{
						aBtn3 = ((aBtn.PreSelected != null && aBtn.PreSelected.title == "submit") ? tab.getBtn("set_auto") : tab.getBtn("submit"));
					}
					if (aBtn3 != null)
					{
						SND.Ui.play("cursor", false);
						aBtn3.Select(true);
					}
				}
			}
			Bench.Pend("CFG UI");
			return flag;
		}

		private bool fnCfgTopicChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			this.fineTabVisibility((cur_value == 1) ? "SP" : "MAIN", true, true);
			return true;
		}

		public void revertData()
		{
			if (UiCFG.BaRevertData != null)
			{
				UiCFG.BaRevertData.position = 0UL;
				CFG.readBinary(UiCFG.BaRevertData, true);
			}
			UiCFG.BaRevertData = null;
		}

		public void submitData()
		{
			if (MGV.temp_kisekae != CFG.temp_kisekae)
			{
				MGV.temp_kisekae = CFG.temp_kisekae;
				MGV.saveSdFile(null);
			}
			CFG.saveSdFile(null);
		}

		private void fineTabVisibility(string key, bool select_memory_selection = false, bool show_sp_colmnrow = true)
		{
			this.BxOut.endTab(true);
			this.BxOut.rowRemakeCheck(false);
			this.saveCurrentSelectingOnTab();
			this.OTab.TryGetValue(key, out this.CurShowingTab);
			Designer.EvacuateContainer evacuateContainer = new Designer.EvacuateContainer(this.BxOut, this.BxOut.EvacuateMemory(this.AEvcTab, null, true), true);
			this.AEvcTab = evacuateContainer.AEvc;
			int num = this.AEvcTab.Count;
			this.BxOut.init();
			this.BxOut.alignx = ALIGN.CENTER;
			this.BxOut.ReassignEvacuatedMemory(null, evacuateContainer.OnamedObject, false);
			for (int i = (show_sp_colmnrow ? 0 : 1); i < 2; i++)
			{
				int j = 0;
				while (j < num)
				{
					Designer.EvacuateMem evacuateMem = this.AEvcTab[j];
					if ((i == 0) ? (evacuateMem.Blk == this.DsTabSp) : (evacuateMem.Blk == this.CurShowingTab))
					{
						this.BxOut.ReassignEvacuatedMemory(evacuateMem);
						this.BxOut.Br();
						num--;
						this.AEvcTab.RemoveAt(j);
					}
					else
					{
						j++;
					}
				}
			}
			this.MainBoxRelink();
			if (select_memory_selection && this.CurShowingTab != null)
			{
				aBtn firstFocusButton = this.getFirstFocusButton();
				if (firstFocusButton != null)
				{
					firstFocusButton.Select(true);
				}
				else
				{
					aBtn btn = this.CurShowingTab.getBtn(0);
					if (btn != null)
					{
						btn.Select(true);
					}
				}
			}
			this.BxOut.fineAllFillBlockAlpha(this.CurShowingTab);
		}

		public UiCFG reveal(Transform T, float posx = 0f, float posy = 0f, bool animate = true)
		{
			if (this.CurShowingTab != null)
			{
				ScrollBox scrollBox = this.CurShowingTab.getScrollBox();
				if (scrollBox != null)
				{
					scrollBox.reveal(T, posx, posy, animate);
				}
			}
			return this;
		}

		public UiCFG reveal(IDesignerBlock B, bool animate = true, REVEALTYPE type = REVEALTYPE.ALWAYS)
		{
			if (this.CurShowingTab != null)
			{
				ScrollBox scrollBox = this.CurShowingTab.getScrollBox();
				if (scrollBox != null)
				{
					scrollBox.reveal(B, animate, type);
				}
			}
			return this;
		}

		public Designer getCurrentShowingTab()
		{
			return this.CurShowingTab;
		}

		public Designer getCurrentShowingTab(out string key)
		{
			key = null;
			if (this.CurShowingTab != null)
			{
				X.GetKeyByValue<string, Designer>(this.OTab, this.CurShowingTab, out key);
			}
			return this.CurShowingTab;
		}

		private void initStickSensitivityCheck()
		{
			this.TargetDevice = null;
			this.TargetDeviceJS = null;
			this.cur_sensitivity_level = IN.getCurrentKeyAssignObject().stick_threshold;
			if (!this.OTab.ContainsKey("SENSTV"))
			{
				Designer designer = this.OTab["MAIN"];
				Designer designer2 = this.BxOut.addTab("SENSTV", designer.get_swidth_px(), designer.get_sheight_px(), designer.get_swidth_px(), designer.get_sheight_px(), false);
				if (this.FD_onDeviceCheck == null)
				{
					this.FD_onDeviceCheck = new Action<InputDevice, InputDeviceChange>(this.onDeviceCheck);
				}
				designer2.Small();
				InputSystem.onDeviceChange += this.FD_onDeviceCheck;
				designer2.alignx = ALIGN.CENTER;
				designer2.init();
				designer2.addHr(new DsnDataHr().H(designer2.use_h * 0.25f));
				designer2.Br();
				designer2.addImg(new DsnDataImg
				{
					html = true,
					TxCol = C32.d2c(4283780170U),
					text = TX.Get("pad_sensitivity_bg", ""),
					swidth = this.BxOut.use_w,
					sheight = 160f,
					FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawFibStick)
				});
				designer2.Br();
				Designer designer3 = designer2.addTab("_sensitive_btns", this.BxOut.use_w, 140f, this.BxOut.use_w, 140f, false);
				designer3.Smallest();
				designer3.scrolling_margin_in_tb = 8f;
				designer3.alignx = ALIGN.CENTER;
				designer3.init();
				designer2.endTab(true);
				this.BxOut.endTab(true);
				this.OTab["SENSTV"] = designer2;
			}
			this.fineTabVisibility("SENSTV", false, false);
			this.recheckTargetDevice();
			if (this.cur_sensitivity_level < 0f)
			{
				this.initStickSensitivityCheck(UiCFG.STATE.STICK_SENSITIVE_AUTO);
				return;
			}
			this.initStickSensitivityCheck(UiCFG.STATE.STICK_SENSITIVE);
		}

		private void initStickSensitivityCheck(UiCFG.STATE stt)
		{
			this.ui_state = stt;
			this.t_stick_auto = -1f;
			Designer tab = this.CurShowingTab.getTab("_sensitive_btns");
			if (tab != null)
			{
				tab.Clear();
				tab.alignx = ALIGN.CENTER;
				tab.item_margin_y_px = 4f;
				tab.item_margin_x_px = 20f;
				DsnDataP dsnDataP = new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					swidth = tab.w - tab.margin_in_lr * 2f,
					sheight = 70f
				};
				if (stt == UiCFG.STATE.STICK_SENSITIVE)
				{
					dsnDataP.sheight = 35f;
					SND.Ui.play("reset_var", false);
					LabeledInputField labeledInputField = tab.addInput(new DsnDataInput
					{
						min = 0.0,
						max = 1.0,
						number = true,
						def = X.spr_after(1f - this.cur_sensitivity_level, 4),
						bounds_w = 266f,
						label = TX.Get("Stick_sensitivity_value", ""),
						fnChangedDelay = delegate(LabeledInputField LI)
						{
							this.cur_sensitivity_level = X.saturate(1f - X.Nm(LI.text, 0.5f, false));
							KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
							currentKeyAssignObject.stick_threshold = this.cur_sensitivity_level;
							currentKeyAssignObject.finePressThreshold();
							return true;
						}
					});
					(labeledInputField.get_Skin() as ButtonSkinForLabel).label_col = 4283780170U;
					tab.Br();
					aBtn aBtn = tab.addButtonT<aBtnNel>(new DsnDataButton
					{
						title = "set_auto",
						skin_title = TX.Get("Stick_sensitivity_auto", ""),
						skin = "normal",
						fnClick = new FnBtnBindings(this.fnClickSensitivityCheckBtn),
						w = 285f,
						h = 32f
					});
					aBtn aBtn2 = tab.addButtonT<aBtnNel>(new DsnDataButton
					{
						title = "submit",
						skin_title = TX.Get("Submit", ""),
						skin = "normal",
						fnClick = new FnBtnBindings(this.fnClickSensitivityCheckBtn),
						w = 190f,
						h = 32f
					});
					aBtn.click_snd = "enter_small";
					labeledInputField.secureNavi();
					aBtn.secureNavi();
					aBtn2.secureNavi();
					labeledInputField.do_not_tip_on_navi_loop = (aBtn.do_not_tip_on_navi_loop = (aBtn2.do_not_tip_on_navi_loop = true));
					tab.Br();
					tab.addP(dsnDataP.Text(TX.Get("Stick_manage", "")), false);
					aBtn2.Select(true);
				}
				if (stt == UiCFG.STATE.STICK_SENSITIVE_AUTO)
				{
					tab.addP(dsnDataP.Text(TX.Get("Stick_sensitivity_adjust_auto_prompt", "")), false);
					tab.Br();
					aBtnNel aBtnNel = tab.addButtonT<aBtnNel>(new DsnDataButton
					{
						title = "cancel",
						skin_title = TX.Get("Cancel", ""),
						skin = "normal",
						fnClick = new FnBtnBindings(this.fnClickSensitivityCheckBtn),
						w = 246f,
						h = 32f
					});
					aBtnNel.click_snd = "";
					aBtnNel.do_not_tip_on_navi_loop = true;
					aBtnNel.Select(true);
				}
			}
			IN.clearPushDown(true);
		}

		private void recheckTargetDevice()
		{
			if (this.TargetDevice != null || this.TargetDeviceJS != null)
			{
				return;
			}
			int count = InputSystem.devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice = InputSystem.devices[i];
				if (inputDevice is Gamepad)
				{
					this.TargetDevice = inputDevice as Gamepad;
					return;
				}
				if (inputDevice is Joystick)
				{
					this.TargetDeviceJS = inputDevice as Joystick;
					return;
				}
			}
		}

		private void quitStickSensitivityCheck()
		{
			KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
			currentKeyAssignObject.stick_threshold = this.cur_sensitivity_level;
			currentKeyAssignObject.finePressThreshold();
			IN.clearPushDown(true);
			this.fineTabVisibility("MAIN", true, true);
			this.TargetDevice = null;
			this.TargetDeviceJS = null;
			this.ui_state = UiCFG.STATE.ACTIVE;
			InputSystem.onDeviceChange -= this.FD_onDeviceCheck;
		}

		private bool fnDrawFibStick(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTRX.MtrMeshDashLine, false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			this.recheckTargetDevice();
			int num = ((this.TargetDeviceJS != null) ? (-1) : 0);
			int num2 = ((this.TargetDeviceJS == null) ? 2 : 0);
			float num3 = this.cur_sensitivity_level;
			for (int i = num; i < num2; i++)
			{
				StickControl stickControl = null;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				if (this.TargetDevice != null && i >= 0)
				{
					stickControl = ((i == 0) ? this.TargetDevice.leftStick : this.TargetDevice.rightStick);
				}
				else if (i < 0 && this.TargetDeviceJS != null)
				{
					stickControl = this.TargetDeviceJS.stick;
				}
				Md.base_px_x = (float)((i == -1) ? 0 : (-120 * X.MPF(i == 0)));
				Md.chooseSubMesh(0, false, false);
				if (stickControl != null)
				{
					num4 = stickControl.left.ReadValue();
					num5 = stickControl.right.ReadValue();
					num6 = stickControl.up.ReadValue();
					num7 = stickControl.down.ReadValue();
					Md.Col = Md.ColGrd.Set(4294906718U).mulA(alpha * 0.7f).C;
					Md.ColGrd.mulA(0f);
					for (int j = 0; j < 4; j++)
					{
						float num8;
						switch (j)
						{
						case 0:
							num8 = num4;
							break;
						case 1:
							num8 = num6;
							break;
						case 2:
							num8 = num5;
							break;
						default:
							num8 = num7;
							break;
						}
						if (num8 >= num3)
						{
							float num9 = CAim.get_agR((AIM)j, 0f);
							Md.Arc2(0f, 0f, 84f, 84f, num9 - 0.7853982f, num9 + 0.7853982f, 0f, 0f, 1f);
						}
					}
				}
				if (this.ui_state == UiCFG.STATE.STICK_SENSITIVE_AUTO)
				{
					Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.4f : 1f) * (0.35f + 0.2f * X.COSIT(45f)));
					Md.Poly(0f, 0f, 80f, 0f, 24, 0f, false, 0f, 0f);
				}
				else
				{
					Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.4f : 1f));
					Md.Poly(0f, 0f, 80f, 0f, 24, 1f, false, 0f, 0f);
				}
				Md.chooseSubMesh(1, false, false);
				float num10 = 80f * num3;
				Md.ButtonKadomaruDashedM(0f, 0f, num10 * 2f, num10 * 2f, num10, X.IntR(num10 * 2f * 3.1415927f / 12f), 2f, false, 0.5f, 24);
				Md.chooseSubMesh(0, false, false);
				Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.11f : 0.4f));
				Md.Poly(0f, 0f, num10, 0f, 24, 0f, false, 0f, 0f);
				if (stickControl != null)
				{
					Vector2 vector = new Vector2(-num4 + num5, num6 - num7) * 80f;
					if (vector.x != 0f || vector.y != 0f)
					{
						float num11 = X.GAR2(0f, 0f, vector.x, vector.y);
						vector.x *= X.Abs(X.Cos(num11));
						vector.y *= X.Abs(X.Sin(num11));
					}
					Md.Col = C32.MulA(4283780170U, alpha);
					Md.Poly(vector.x, vector.y, 10f, 0f, 24, 0f, false, 0f, 0f);
					Md.Col = C32.MulA(uint.MaxValue, alpha);
					Md.Line(vector.x, vector.y - 11f, vector.x, vector.y + 11f, 1f, false, 0f, 0f);
					Md.Line(vector.x - 11f, vector.y, vector.x + 11f, vector.y, 1f, false, 0f, 0f);
				}
			}
			update_meshdrawer = true;
			return false;
		}

		private bool runSensitiveSelect()
		{
			if (!this.runAutoStickSensitivity())
			{
				if (IN.isT() || IN.isB())
				{
					Designer tab = this.BxOut.getTab("_sensitive_btns");
					if (tab != null)
					{
						aBtn aBtn = tab.getBtnContainer().Get((aBtn.PreSelected is LabeledInputField) ? 1 : 0);
						if (aBtn != null)
						{
							SND.Ui.play("cursor", false);
							aBtn.Select(true);
						}
					}
				}
				if ((IN.isL() || IN.isR()) && !(aBtn.PreSelected is LabeledInputField))
				{
					Designer tab2 = this.BxOut.getTab("_sensitive_btns");
					if (tab2 != null)
					{
						string text = ((aBtn.PreSelected != null) ? aBtn.PreSelected.title : "");
						aBtn aBtn2 = tab2.getBtnContainer().Get((text == "submit") ? 1 : 2);
						if (aBtn2 != null)
						{
							SND.Ui.play("cursor", false);
							aBtn2.Select(true);
						}
					}
				}
			}
			return false;
		}

		private bool runAutoStickSensitivity()
		{
			bool flag = this.ui_state == UiCFG.STATE.STICK_SENSITIVE_AUTO;
			if (this.t_stick_auto < 0f && flag)
			{
				this.t_stick_auto = 0f;
				this.cur_sensitivity_level = 0.5f;
			}
			int num = ((this.TargetDeviceJS != null) ? (-1) : 0);
			int num2 = ((this.TargetDeviceJS == null) ? 2 : 0);
			float num3 = this.cur_sensitivity_level;
			bool flag2 = false;
			float num4 = 0f;
			for (int i = num; i < num2; i++)
			{
				StickControl stickControl = null;
				if (this.TargetDevice != null && i >= 0)
				{
					stickControl = ((i == 0) ? this.TargetDevice.leftStick : this.TargetDevice.rightStick);
				}
				else if (i < 0 && this.TargetDeviceJS != null)
				{
					stickControl = this.TargetDeviceJS.stick;
				}
				if (stickControl != null)
				{
					float num5 = stickControl.left.ReadValue();
					float num6 = stickControl.right.ReadValue();
					float num7 = stickControl.up.ReadValue();
					float num8 = stickControl.down.ReadValue();
					for (int j = 0; j < 4; j++)
					{
						float num9;
						switch (j)
						{
						case 0:
							num9 = num5;
							break;
						case 1:
							num9 = num7;
							break;
						case 2:
							num9 = num6;
							break;
						default:
							num9 = num8;
							break;
						}
						float num10 = num9;
						if (flag)
						{
							num10 = 1f - (1f - num10) * 0.5f;
							num4 = X.Mn(0.9375f, X.Mx(num4, num10));
						}
						if (!flag && num10 > num3 * 0.5f)
						{
							flag2 = true;
						}
					}
				}
			}
			num4 = X.Mx(num4, this.cur_sensitivity_level);
			if (!flag)
			{
				return flag2;
			}
			if (num4 != this.cur_sensitivity_level)
			{
				this.cur_sensitivity_level = X.MULWALKMX(this.cur_sensitivity_level, num4, 0.04f, 0.004f);
				KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
				currentKeyAssignObject.stick_threshold = this.cur_sensitivity_level;
				currentKeyAssignObject.finePressThreshold();
				this.t_stick_auto = 0f;
			}
			else
			{
				this.t_stick_auto += 1f;
			}
			return this.t_stick_auto >= 150f;
		}

		private bool fnClickSensitivityCheckBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "cancel"))
				{
					if (!(title == "submit"))
					{
						if (title == "set_auto")
						{
							this.initStickSensitivityCheck(UiCFG.STATE.STICK_SENSITIVE_AUTO);
						}
					}
					else
					{
						this.quitStickSensitivityCheck();
					}
				}
				else
				{
					this.initStickSensitivityCheck(UiCFG.STATE.STICK_SENSITIVE);
				}
			}
			return true;
		}

		private void onDeviceCheck(InputDevice device, InputDeviceChange change)
		{
			if (change == InputDeviceChange.Removed && (device == this.TargetDevice || device == this.TargetDeviceJS))
			{
				this.TargetDevice = null;
				this.TargetDeviceJS = null;
			}
		}

		public const float SND_VOL_DEFAULT = 0.8f;

		public const float VO_VOL_DEFAULT = 0.85f;

		public const float BGM_VOL_DEFAULT = 0.7f;

		private static ByteArray BaRevertData;

		public UiCFG.STATE ui_state;

		private readonly bool enable_fullscreen;

		public const float desc_w = 380f;

		public const float desc_h = 120f;

		private readonly Transform BaseTrs;

		private readonly bool show_difficulty;

		public int pre_ui_effect_dirty;

		public byte pre_ui_effect_blood;

		public byte pre_ui_effect_density;

		public bool pre_vsync;

		public int temp_window_size;

		private UiBoxDesigner BxOut;

		private UiBoxDesigner BxDesc;

		private Designer DsSubmittion;

		private readonly bool use_keycon;

		private byte pre_sensitive_level;

		public const byte ui_effect_density_thesh = 10;

		public readonly BDic<string, aBtn> OPreSelected;

		private readonly BDic<string, Designer> OTab;

		private Designer CurShowingTab;

		private List<Designer.EvacuateMem> AEvcTab;

		public const string tab_main = "MAIN";

		public const string tab_sensitivity = "SENSTV";

		public const string tab_sp = "SP";

		private Designer DsTabSp;

		private ColumnRow RTabSp;

		private aBtnMeter.FnMeterBindings FD_fnChangeConfigValue;

		private FnBtnBindings FD_fnShowDesc;

		private readonly int[] Ascreen_width = new int[]
		{
			640, 800, 1024, 1280, 1400, 1600, 1768, 1920, 2160, 2340,
			2560, 3200, 3840, 4320, 5120
		};

		private string[] Ascreen_width_desc;

		private Gamepad TargetDevice;

		private Joystick TargetDeviceJS;

		private float t_stick_auto;

		private float cur_sensitivity_level;

		private const float dep_stick_level_default = 0.5f;

		private Action<InputDevice, InputDeviceChange> FD_onDeviceCheck;

		public enum STATE
		{
			OFFLINE,
			INITTED,
			ACTIVE,
			KEYCON,
			STICK_SENSITIVE,
			STICK_SENSITIVE_AUTO
		}
	}
}
