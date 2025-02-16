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
	public class CFG
	{
		public static bool autosave_on_bench
		{
			get
			{
				return (CFG.autosave_timing & 1) > 0;
			}
		}

		public static bool autosave_on_scenario
		{
			get
			{
				return (CFG.autosave_timing & 2) > 0;
			}
		}

		public static byte blood_weaken
		{
			get
			{
				return M2DBase.blood_weaken;
			}
			set
			{
				M2DBase.blood_weaken = value;
			}
		}

		public static byte BLOOD_WEAKEN_MAX
		{
			get
			{
				return 100;
			}
		}

		public static void clearSpVariable()
		{
			CFG.sp_uipic_lr = CFG.UIPIC_LR.L;
			CFG.sp_dmg_multivoice = 0;
			CFG.sp_cloth_strength = 100;
			CFG.sp_cloth_broken_debuff = false;
			CFG.sp_publish_milk = 0;
			CFG.sp_epdmg_vo_mouth = 100;
			CFG.sp_epdmg_vo_iku = 0;
			CFG.sp_publish_juice = 100;
			CFG.sp_juice_cutin = false;
			CFG.sp_deadburned = false;
			CFG.sp_dmgte_pixel_density = 100;
			CFG.sp_dmgte_pixel_duration = 100;
			CFG.sp_dmgte_ui_density = 100;
			CFG.sp_dmgte_ui_duration = 100;
			CFG.sp_voice_for_pleasure = 1f;
			CFG.sp_voice_for_pleasure2m = 20f;
			CFG.sp_use_uipic_press_gimmick = false;
			CFG.sp_use_uipic_press_enemy = 0;
			CFG.sp_use_uipic_press_balance = 100;
			CFG.sp_go_cheat = false;
			CFG.sp_dmgcounter_position = CFG.UIPIC_DMGCNT.MAP;
			CFG.sp_opacity_marunomi = 100;
			CFG.sp_threshold_pregnant = 50;
			CFG.vib_level = 100;
			if (NEL.Instance != null && NEL.Instance.Vib != null)
			{
				NEL.Instance.Vib.base_level = (float)CFG.vib_level * 0.01f;
			}
			CFG.SpEpBoostPrepare(true);
		}

		private static bool isSpEpBoostTarget(EPCATEG epcateg)
		{
			return epcateg != EPCATEG.OTHER && epcateg != EPCATEG.UTERUS;
		}

		private static void SpEpBoostPrepare(bool force = false)
		{
			if (CFG.Osp_ep_boost.Count == 0 || force)
			{
				for (int i = 0; i < 11; i++)
				{
					EPCATEG epcateg = (EPCATEG)i;
					if (CFG.isSpEpBoostTarget(epcateg))
					{
						CFG.Osp_ep_boost[epcateg] = 100;
					}
				}
			}
		}

		public static float getEpApplyRatio(EPCATEG epcateg)
		{
			byte b;
			if (!CFG.Osp_ep_boost.TryGetValue(epcateg, out b))
			{
				return 1f;
			}
			if (b > 200)
			{
				return (float)((b - 200) * 200) * 0.01f;
			}
			if (b == 100)
			{
				return 1f;
			}
			if (b != 200)
			{
				return (float)b * 0.01f;
			}
			return 2f;
		}

		public static void endEdit()
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase != null)
			{
				PRNoel prNoel = nelM2DBase.getPrNoel();
				if (prNoel != null)
				{
					prNoel.setVoiceOverrideAllowLevel(0f);
					if (prNoel.BetoMng.is_torned)
					{
						prNoel.fineClothTorned();
						M2SerItem m2SerItem = prNoel.Ser.Get(SER.CLT_BROKEN);
						if (m2SerItem != null)
						{
							prNoel.Ser.resetFlags();
							m2SerItem.need_init = true;
						}
					}
				}
			}
		}

		public static void refineAllLanguageCache(bool evt_check = true)
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase != null)
			{
				nelM2DBase.refineAllLanguageFonts(evt_check);
			}
			if (SceneTitleTemp.Instance != null)
			{
				SceneTitleTemp.Instance.fineTexts();
			}
			MagicSelector.refineDecideTypeText();
			UiSkillManageBox.releaseTabKeysCache();
			NelMSGResource.initResource(true);
			NelItem.fineNameLocalizedWhole();
			QuestTracker.fineNameLocalizedWhole();
			COOK.dlLang();
		}

		public static void newGameSp()
		{
			CFG.Aenabled_sp_value.Clear();
		}

		public static ByteArray loadSdFile(bool consider_fullscreen_mode = false, bool keycon_apply_pad_mode = false)
		{
			ByteArray byteArray = NKT.readSdBinary("config.cfg", true);
			if (global::XX.X.DEBUGNOCFG)
			{
				return byteArray;
			}
			if (byteArray != null)
			{
				global::XX.X.dl("オプションの読み込みを開始", null, false, false);
				CFG.readBinary(byteArray, true);
				if (byteArray.bytesAvailable > 0UL)
				{
					KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
					currentKeyAssignObject.readSaveString(byteArray, keycon_apply_pad_mode);
					currentKeyAssignObject.fineCurrentPadState();
				}
				if (consider_fullscreen_mode && CFG.fullscreen_mode != Screen.fullScreen)
				{
					IN.setFullScreenMode(CFG.fullscreen_mode, 0f);
				}
			}
			return byteArray;
		}

		public static ByteArray saveSdFile(ByteArray Ba = null)
		{
			if (Ba == null)
			{
				Ba = CFG.createBinary(null);
				IN.getCurrentKeyAssignObject().getSaveString(Ba);
			}
			NKT.writeSdBinary("config.cfg", Ba, false);
			IN.save_prefs = true;
			return Ba;
		}

		public static ByteArray createBinary(ByteArray Ba0 = null)
		{
			if (Ba0 == null)
			{
				Ba0 = new ByteArray(0U);
			}
			try
			{
				Ba0.writeUInt(1730024101U);
				Ba0.writeByte(13);
				ByteArray byteArray = new ByteArray(0U);
				byteArray.writeByte((CFG.fullscreen_mode ? 1 : 0) | (global::XX.X.ENG_MODE ? 2 : 0) | (global::XX.X.SENSITIVE ? 4 : 0) | (global::XX.X.SP_SENSITIVE ? 8 : 0));
				byteArray.writeByte((int)((byte)SND.volume));
				byteArray.writeByte((int)((byte)SND.voice_volume));
				byteArray.writeByte((int)((byte)SND.bgm_volume));
				byteArray.writeByte((int)CFG.autosave_timing);
				byteArray.writeByte((int)CFG.magsel_slow);
				byteArray.writeByte(global::XX.X.AF);
				byteArray.writeByte(global::XX.X.AF_EF);
				byteArray.writeByte(global::XX.X.IntR(global::XX.X.EF_LEVEL_NORMAL * 200f));
				byteArray.writeByte(global::XX.X.IntR(global::XX.X.EF_LEVEL_UI * 200f));
				byteArray.writeByte(global::XX.X.IntR(global::XX.X.EF_TIMESCALE_UI * 200f));
				byteArray.writeByte((int)CFG.ui_effect_dirty);
				byteArray.writeString(TX.getCurrentFamilyName(), "utf-8");
				byteArray.writeByte((int)CFG.ui_sensitive_description);
				byteArray.writeByte((int)CFG.posteffect_weaken);
				byteArray.writeBool(M2SoundPlayer.monoral);
				byteArray.writeByte((int)CFG.blood_weaken);
				byteArray.writeByte((int)CFG.ui_effect_density);
				byteArray.writeBool(M2MoverPr.double_tap_running);
				byteArray.writeByte((int)((byte)(M2MoverPr.running_thresh * 100f)));
				byteArray.writeBool(M2MoverPr.jump_press_reverse);
				byteArray.writeBool(CFG.shield_hold_evadable);
				byteArray.writeByte((int)CFG.fatal_automode_speed);
				byteArray.writeByte((int)CFG.item_usel_type);
				byteArray.writeBool(CFG.go_text_pos_top);
				byteArray.writeByte((int)CFG.magsel_decide_type);
				byteArray.writeByte((int)CFG.sp_uipic_lr);
				byteArray.writeByte((int)CFG.sp_dmg_multivoice);
				byteArray.writeByte((int)CFG.sp_cloth_strength);
				byteArray.writeByte(CFG.sp_cloth_broken_debuff ? 1 : 0);
				byteArray.writeByte((int)CFG.sp_publish_milk);
				byteArray.writeByte((int)CFG.sp_publish_juice);
				byteArray.writeByte(CFG.sp_deadburned ? 1 : 0);
				byteArray.writeByte((int)CFG.sp_dmgte_pixel_density);
				byteArray.writeByte((int)CFG.sp_dmgte_pixel_duration);
				byteArray.writeByte((int)CFG.sp_dmgte_ui_density);
				byteArray.writeByte((int)CFG.sp_dmgte_ui_duration);
				byteArray.writeByte(CFG.sp_juice_cutin ? 1 : 0);
				byteArray.writeFloat(CFG.sp_voice_for_pleasure);
				byteArray.writeFloat(CFG.sp_voice_for_pleasure2m);
				CFG.SpEpBoostPrepare(false);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.CLI]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.VAGINA]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.CANAL]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.GSPOT]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.URETHRA]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.BREAST]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.ANAL]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.MOUTH]);
				byteArray.writeByte((int)CFG.Osp_ep_boost[EPCATEG.EAR]);
				byteArray.writeBool(CFG.sp_go_cheat);
				byteArray.writeByte((int)CFG.sp_opacity_marunomi);
				byteArray.writeByte((int)CFG.sp_epdmg_vo_mouth);
				byteArray.writeBool(CFG.sp_use_uipic_press_gimmick);
				byteArray.writeByte((int)CFG.sp_use_uipic_press_balance);
				byteArray.writeByte((int)CFG.sp_use_uipic_press_enemy);
				byteArray.writeByte((int)CFG.sp_dmgcounter_position);
				byteArray.writeByte((int)CFG.vib_level);
				byteArray.writeByte((int)CFG.sp_epdmg_vo_iku);
				byteArray.writeByte((int)CFG.sp_threshold_pregnant);
				Ba0.writeExtractBytes(byteArray, 2, -1);
			}
			catch (Exception ex)
			{
				global::XX.X.de(ex.ToString(), null);
			}
			return Ba0;
		}

		public static ByteArray readBinary(ByteArray Ba0, bool apply_language = true)
		{
			bool flag = global::XX.X.ENG_MODE;
			int num = -1;
			string text = "";
			CFG.clearSpVariable();
			CFG.ui_effect_density = 10;
			try
			{
				if (Ba0.readUInt() != 1730024101U)
				{
					throw new Exception("Cfg read: Header error");
				}
				num = Ba0.readByte();
				CFG.go_text_pos_top = false;
				ByteArray byteArray = Ba0.readExtractBytes(2);
				CFG.first_startup = false;
				int num2 = byteArray.readByte();
				CFG.fullscreen_mode = (num2 & 1) != 0;
				flag = (num2 & 2) != 0;
				if ((num2 & 8) != 0)
				{
					global::XX.X.sensitive_level = 2;
				}
				else if ((num2 & 4) != 0)
				{
					global::XX.X.sensitive_level = 1;
				}
				else
				{
					global::XX.X.sensitive_level = 0;
				}
				SND.volume = byteArray.readByte();
				SND.voice_volume = byteArray.readByte();
				SND.bgm_volume = byteArray.readByte();
				CFG.autosave_timing = (byte)byteArray.readByte();
				CFG.magsel_slow = (byte)byteArray.readByte();
				global::XX.X.AF = global::XX.X.MMX(1, byteArray.readByte(), 4);
				global::XX.X.AF_EF = global::XX.X.MMX(1, byteArray.readByte(), 4);
				global::XX.X.EF_LEVEL_NORMAL = global::XX.X.ZLINE((float)byteArray.readByte(), 200f);
				global::XX.X.EF_LEVEL_UI = global::XX.X.ZLINE((float)byteArray.readByte(), 200f);
				global::XX.X.EF_TIMESCALE_UI = global::XX.X.ZLINE((float)byteArray.readByte(), 200f);
				CFG.ui_effect_dirty = (byte)byteArray.readByte();
				text = byteArray.readString("utf-8", false);
				CFG.ui_sensitive_description = (byte)byteArray.readByte();
				CFG.posteffect_weaken = (byte)byteArray.readByte();
				M2SoundPlayer.monoral = byteArray.readBoolean();
				CFG.blood_weaken = (byte)byteArray.readByte();
				byte b = (byte)byteArray.readByte();
				if (num >= 13)
				{
					CFG.ui_effect_density = b;
				}
				M2MoverPr.double_tap_running = byteArray.readBoolean();
				M2MoverPr.running_thresh = global::XX.X.MMX(0f, (float)byteArray.readByte() / 100f, 1f);
				M2MoverPr.jump_press_reverse = byteArray.readBoolean();
				CFG.shield_hold_evadable = byteArray.readBoolean();
				CFG.fatal_automode_speed = (byte)byteArray.readByte();
				CFG.item_usel_type = (CFG.USEL_TYPE)byteArray.readByte();
				CFG.go_text_pos_top = byteArray.readBoolean();
				CFG.magsel_decide_type = (CFG.MAGSEL_TYPE)byteArray.readByte();
				CFG.sp_uipic_lr = (CFG.UIPIC_LR)byteArray.readUByte();
				CFG.sp_dmg_multivoice = byteArray.readUByte();
				CFG.sp_cloth_strength = byteArray.readUByte();
				CFG.sp_cloth_broken_debuff = byteArray.readUByte() > 0;
				CFG.sp_publish_milk = byteArray.readUByte();
				CFG.sp_publish_juice = byteArray.readUByte();
				CFG.sp_deadburned = byteArray.readUByte() > 0;
				CFG.sp_dmgte_pixel_density = byteArray.readUByte();
				CFG.sp_dmgte_pixel_duration = byteArray.readUByte();
				CFG.sp_dmgte_ui_density = byteArray.readUByte();
				CFG.sp_dmgte_ui_duration = byteArray.readUByte();
				CFG.sp_juice_cutin = byteArray.readUByte() > 0;
				CFG.sp_voice_for_pleasure = byteArray.readFloat();
				CFG.sp_voice_for_pleasure2m = byteArray.readFloat();
				CFG.Osp_ep_boost[EPCATEG.CLI] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.VAGINA] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.CANAL] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.GSPOT] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.URETHRA] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.BREAST] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.ANAL] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.MOUTH] = byteArray.readUByte();
				CFG.Osp_ep_boost[EPCATEG.EAR] = byteArray.readUByte();
				CFG.sp_go_cheat = byteArray.readUByte() > 0;
				CFG.sp_opacity_marunomi = byteArray.readUByte();
				CFG.sp_epdmg_vo_mouth = byteArray.readUByte();
				CFG.sp_use_uipic_press_gimmick = byteArray.readBoolean();
				CFG.sp_use_uipic_press_balance = byteArray.readUByte();
				CFG.sp_use_uipic_press_enemy = byteArray.readUByte();
				CFG.sp_dmgcounter_position = (CFG.UIPIC_DMGCNT)byteArray.readByte();
				CFG.vib_level = byteArray.readUByte();
				CFG.sp_epdmg_vo_iku = byteArray.readUByte();
				CFG.sp_threshold_pregnant = byteArray.readUByte();
			}
			catch (Exception ex)
			{
				if (ex.Message != "ERROR_EOF")
				{
					num = -1;
					global::XX.X.de(ex.ToString(), null);
				}
			}
			if (NEL.Instance != null && NEL.Instance.Vib != null)
			{
				NEL.Instance.Vib.base_level = (float)CFG.vib_level * 0.01f;
			}
			if (num >= 0)
			{
				if (apply_language)
				{
					if (num <= 10)
					{
						text = (flag ? "en" : "_");
					}
					if (!TX.familyIs(text))
					{
						TX.changeFamily((text == "zh-cnB") ? "zh-cn" : text);
					}
				}
				global::XX.X.AF_EF = global::XX.X.Mx(global::XX.X.AF_EF, global::XX.X.AF);
			}
			return Ba0;
		}

		public static ByteArray saveKeyConfigFileOnly()
		{
			ByteArray byteArray = ((CFG.BaRevertData != null) ? new ByteArray(CFG.BaRevertData, true) : CFG.createBinary(null));
			IN.getCurrentKeyAssignObject().getSaveString(byteArray);
			CFG.saveSdFile(byteArray);
			return byteArray;
		}

		public static void readBinarySp(ByteArray Ba)
		{
			int num = Ba.readByte();
			for (int i = 0; i < num; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				if (CFG.Aenabled_sp_value.IndexOf(text) == -1)
				{
					CFG.Aenabled_sp_value.Add(text);
				}
			}
		}

		public static void writeBinarySp(ByteArray Ba)
		{
			int count = CFG.Aenabled_sp_value.Count;
			Ba.writeByte(count);
			for (int i = 0; i < count; i++)
			{
				string text = CFG.Aenabled_sp_value[i];
				Ba.writePascalString(text, "utf-8");
			}
		}

		public static void addSp(string key)
		{
			if (!CFG.isSpEnabled(key))
			{
				CFG.Aenabled_sp_value.Add(key);
				if (key != null && key == "go_cheat")
				{
					CFG.sp_go_cheat = true;
				}
			}
		}

		public static bool isSpEnabled(string key)
		{
			return CFG.Aenabled_sp_value.IndexOf(key) >= 0;
		}

		public CFG(UiBoxDesigner _Bx, UiBoxDesigner _BxDesc, Designer _DsSubmittion = null, bool _use_keycon = true, bool _show_difficulty = true, Action<Designer, string> FnDesignerCreateAfter = null)
		{
			this.BxOut = _Bx;
			this.BxDesc = _BxDesc;
			this.BaseTrs = this.BxOut.transform.parent;
			this.show_difficulty = _show_difficulty;
			this.pre_ui_effect_dirty = (int)CFG.ui_effect_dirty;
			this.pre_ui_effect_blood = CFG.blood_weaken;
			this.pre_ui_effect_density = CFG.ui_effect_density;
			this.pre_vsync = global::XX.X.v_sync;
			this.pre_sensitive_level = global::XX.X.sensitive_level;
			this.FD_fnChangeConfigValue = new aBtnMeter.FnMeterBindings(this.fnChangeConfigValue);
			this.FD_fnShowDesc = new FnBtnBindings(this.fnShowDesc);
			this.temp_window_size = Screen.width;
			this.DsSubmittion = _DsSubmittion;
			this.BxOut.alignx = ALIGN.CENTER;
			this.BxOut.item_margin_y_px = 10f;
			this.ui_state = CFG.STATE.INITTED;
			CFG.BaRevertData = CFG.createBinary(null);
			float use_w = _Bx.use_w;
			this.use_keycon = _use_keycon;
			this.enable_fullscreen = true;
			CFG.fullscreen_mode = Screen.fullScreen;
			this.BxOut.use_scroll = false;
			this.BxOut.use_button_connection = false;
			this.BxOut.init();
			if (CFG.Aenabled_sp_value.Count > 0)
			{
				this.DsTabSp = this.BxOut.addTab("-DsTabSp", this.BxOut.use_w, 30f, this.BxOut.use_w, 30f, true);
				this.DsTabSp.Smallest();
				this.DsTabSp.init();
				this.RTabSp = ColumnRow.CreateT<aBtnNel>(this.DsTabSp, "ctg_tab", "row_tab", 0, new string[]
				{
					"<img mesh=\"config_cog\" width=\"20\" height=\"24\" color=\"0x" + 4283780170U.ToString("x") + "\" />",
					"<img mesh=\"wholemap_bar\" width=\"20\" height=\"24\" color=\"0x" + 4283780170U.ToString("x") + "\" />"
				}, new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnCfgTopicChanged), this.DsTabSp.use_w, 0f, false, false).LrInput(false);
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
			if (CFG.Aenabled_sp_value.Count > 0)
			{
				designer = this.BxOut.addTab("_DsmInner_Sp", use_w2, use_h, use_w2, use_h, true);
				this.setBoxMainStencil();
				this.createBoxDesignerContentSp();
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

		private UiBoxDesigner P(string title, bool enable = true)
		{
			this.BxOut.Br().addP(new DsnDataP("", false)
			{
				text = TX.Get(title, ""),
				size = 18f * (global::XX.X.ENG_MODE ? 0.7f : 1f),
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
			int num = global::XX.X.isinS<int>(this.Ascreen_width, 1280);
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
				int num4 = global::XX.X.Abs(num3 - Screen.width);
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
				def = (float)global::XX.X.sensitive_level,
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
				def = (float)(global::XX.X.AF - 1),
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
				def = (float)(global::XX.X.AF_EF - 1),
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
				def = (float)(global::XX.X.v_sync ? 1 : 0),
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
				def = (float)(100 - global::XX.X.IntR(global::XX.X.EF_LEVEL_NORMAL * 100f)),
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
				def = (float)(100 - global::XX.X.IntR(global::XX.X.EF_LEVEL_UI * 100f)),
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
				def = (float)(100 - global::XX.X.IntR(global::XX.X.EF_TIMESCALE_UI * 100f)),
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
				def = (float)global::XX.X.IntR(M2MoverPr.running_thresh * 100f),
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

		private void createBoxDesignerContentSp()
		{
			FnBtnMeterLine fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
			{
				int num2 = global::XX.X.IntR(val);
				if (num2 != 0)
				{
					if (num2 == 50)
					{
						return 0.66f;
					}
					if (num2 != 100)
					{
						return 0.33f;
					}
				}
				return 1f;
			};
			FnBtnMeterLine fnBtnMeterLine2 = delegate(aBtnMeter B, int index, float val)
			{
				int num3 = global::XX.X.IntR(val);
				if (num3 != 0)
				{
					if (num3 == 100)
					{
						return 0.66f;
					}
					if (num3 != 200)
					{
						return 0.33f;
					}
				}
				return 1f;
			};
			FnBtnMeterLine fnBtnMeterLine3 = delegate(aBtnMeter B, int index, float val)
			{
				int num4 = global::XX.X.IntR(val);
				if (num4 != 0)
				{
					if (num4 == 100)
					{
						return 0.5f;
					}
					if (num4 != 200)
					{
						if (val <= 200f)
						{
							return 0.25f;
						}
						return 1f;
					}
				}
				return 0.75f;
			};
			if (CFG.isSpEnabled("uipic_lr"))
			{
				this.P("Config_spconfig_uipic_lr", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_uipic_lr",
					title = "spconfig_uipic_lr",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)CFG.sp_uipic_lr,
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_left", "Config_right", "Config_hidden"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("use_uipic_press"))
			{
				this.P("Config_spconfig_use_uipic_press_gimmick", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_gimmick",
					title = "spconfig_use_uipic_press_gimmick",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFG.sp_use_uipic_press_gimmick ? 1 : 0),
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, true);
				this.P("Config_spconfig_use_uipic_press_enemy", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_enemy",
					title = "spconfig_use_uipic_press_enemy",
					skin_title = "",
					def = (float)CFG.sp_use_uipic_press_enemy,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
				this.P("Config_spconfig_use_uipic_press_balance", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_balance",
					title = "spconfig_use_uipic_press_balance",
					skin_title = "",
					def = (float)CFG.sp_use_uipic_press_balance,
					w = this.sliderw_sml,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertPressBalance),
					fnBtnMeterLine = fnBtnMeterLine3
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("threshold_pregnant"))
			{
				this.P("Config_spconfig_threshold_pregnant", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_threshold_pregnant",
					title = "spconfig_threshold_pregnant",
					skin_title = "",
					def = (float)CFG.sp_threshold_pregnant,
					w = this.sliderw_sml,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertThresholdPregnant),
					fnBtnMeterLine = fnBtnMeterLine
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("dmgcounter_position"))
			{
				this.P("Config_spconfig_dmgcounter_position", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgcounter_position",
					title = "spconfig_dmgcounter_position",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)CFG.sp_dmgcounter_position,
					mn = 0f,
					mx = 3f,
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_dmgcounter_0", "Config_dmgcounter_1", "Config_dmgcounter_2", "Config_dmgcounter_3"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("dmg_multivoice"))
			{
				this.P("Config_spconfig_dmg_multivoice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmg_multivoice",
					title = "spconfig_dmg_multivoice",
					skin_title = "",
					def = (float)CFG.sp_dmg_multivoice,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("voice_for_pleasure"))
			{
				this.P("Config_spconfig_voice_for_pleasure", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_voice_for_pleasure",
					title = "spconfig_voice_for_pleasure",
					skin_title = "",
					def = (float)global::XX.X.IntR(CFG.sp_voice_for_pleasure * 100f),
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent),
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
				this.P("Config_spconfig_voice_for_pleasure2m", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_voice_for_pleasure2m",
					title = "spconfig_voice_for_pleasure2m",
					skin_title = "",
					def = (float)global::XX.X.IntR(CFG.sp_voice_for_pleasure2m * 100f),
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("epdmg_vo_mouth"))
			{
				this.P("Config_spconfig_epdmg_vo_mouth", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_epdmg_vo_mouth",
					title = "spconfig_epdmg_vo_mouth",
					skin_title = "",
					def = (float)CFG.sp_epdmg_vo_mouth,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine2,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("epdmg_vo_iku"))
			{
				this.P("Config_spconfig_epdmg_vo_iku", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_epdmg_vo_iku",
					title = "spconfig_epdmg_vo_iku",
					skin_title = "",
					def = (float)CFG.sp_epdmg_vo_iku,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("cloth_strength"))
			{
				this.P("Config_spconfig_cloth_strength", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_cloth_strength",
					title = "spconfig_cloth_strength",
					skin_title = "",
					def = (float)CFG.sp_cloth_strength,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine2,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningClothStrength)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("cloth_broken_debuff"))
			{
				this.P("Config_spconfig_cloth_broken_debuff", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_cloth_broken_debuff",
					title = "spconfig_cloth_broken_debuff",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFG.sp_cloth_broken_debuff ? 1 : 0),
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("milk"))
			{
				this.P("Config_spconfig_milk", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_milk",
					title = "spconfig_milk",
					skin_title = "",
					def = (float)CFG.sp_publish_milk,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
				this.P("Config_spconfig_juice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_juice",
					title = "spconfig_juice",
					skin_title = "",
					def = (float)CFG.sp_publish_juice,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("juice_cutin"))
			{
				this.P("Config_spconfig_juice_cutin", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_juice_cutin",
					title = "spconfig_juice_cutin",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFG.sp_juice_cutin ? 1 : 0),
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_normal", "Config_juice_cutin_1"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("deadburned"))
			{
				this.P("Config_spconfig_deadburned", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_deadburned",
					title = "spconfig_deadburned",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFG.sp_deadburned ? 1 : 0),
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			int num = 11;
			CFG.SpEpBoostPrepare(false);
			for (int i = 0; i < num; i++)
			{
				EPCATEG epcateg = (EPCATEG)i;
				if (CFG.isSpEpBoostTarget(epcateg))
				{
					string text = FEnum<EPCATEG>.ToStr(epcateg).ToLower();
					byte b;
					if (CFG.isSpEnabled("ep_boost_" + text) && CFG.Osp_ep_boost.TryGetValue(epcateg, out b))
					{
						this.P("Config_spconfig_ep_boost_" + text, true).addSliderCT(new DsnDataSlider
						{
							name = "spconfig_ep_boost_" + text,
							title = "spconfig_ep_boost_" + text,
							skin_title = "",
							def = (float)b,
							w = this.sliderw_middle,
							mn = 0f,
							mx = 250f,
							valintv = 10f,
							fnChanged = this.FD_fnChangeConfigValue,
							fnHover = this.FD_fnShowDesc,
							fnDescConvert = new FnDescConvert(this.fnDescConvertRunningEpBoost),
							fnBtnMeterLine = fnBtnMeterLine3
						}, 154f, null, false);
					}
				}
			}
			if (CFG.isSpEnabled("go_cheat"))
			{
				this.P("Config_spconfig_go_cheat", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_go_cheat",
					title = "spconfig_go_cheat",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFG.sp_go_cheat ? 1 : 0),
					w = this.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFG.isSpEnabled("opacity_marunomi"))
			{
				this.P("Config_spconfig_opacity_marunomi", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_opacity_marunomi",
					title = "spconfig_opacity_marunomi",
					skin_title = "",
					def = (float)CFG.sp_opacity_marunomi,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("dmgte_pixel_density"))
			{
				this.P("Config_spconfig_dmgte_pixel_density", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_pixel_density",
					title = "spconfig_dmgte_pixel_density",
					skin_title = "",
					def = (float)CFG.sp_dmgte_pixel_density,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent)
				}, 114f, null, false);
				this.P("Config_spconfig_dmgte_pixel_duration", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_pixel_duration",
					title = "spconfig_dmgte_pixel_duration",
					skin_title = "",
					def = (float)CFG.sp_dmgte_pixel_duration,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent),
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
			}
			if (CFG.isSpEnabled("dmgte_ui_density"))
			{
				this.P("Config_spconfig_dmgte_ui_density", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_ui_density",
					title = "spconfig_dmgte_ui_density",
					skin_title = "",
					def = (float)CFG.sp_dmgte_ui_density,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent),
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
				this.P("Config_spconfig_dmgte_ui_duration", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_ui_duration",
					title = "spconfig_dmgte_ui_duration",
					skin_title = "",
					def = (float)CFG.sp_dmgte_ui_duration,
					w = this.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = this.FD_fnChangeConfigValue,
					fnHover = this.FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(this.fnDescConvertRunningPersent),
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
			}
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

		private string fnDescConvertEffectRestrict(string V)
		{
			if (!(V == "0"))
			{
				return (100f - global::XX.X.Nm(V, 0f, false)).ToString() + "%";
			}
			return TX.Get("Config_Effect_No_Restriction", "");
		}

		private string fnDescConvertDensity(string V)
		{
			return global::XX.X.IntR(global::XX.X.Nm(V, 0f, false) / 10f * 100f).ToString() + "%";
		}

		private string fnDescConvertRunningThresh(string V)
		{
			if (V == "100")
			{
				return TX.Get("Disabled", "");
			}
			return TX.GetA("Config_val_sensitivity", V + "%");
		}

		private string fnDescConvertRunningPersent(string V)
		{
			return V + "%";
		}

		private string fnDescConvertRunningClothStrength(string V)
		{
			if (V == "0")
			{
				return TX.Get("Config_Super_fragile", "");
			}
			if (global::XX.X.NmI(V, 0, false, false) == 200)
			{
				return TX.Get("Config_Unbreakable", "");
			}
			return V + "%";
		}

		private string fnDescConvertRunningEpBoost(string V)
		{
			float num = global::XX.X.Nm(V, 0f, false);
			if (num > 200f)
			{
				return global::XX.X.IntR((num - 200f) * 100f).ToString() + "%";
			}
			return V + "%";
		}

		private string fnDescConvertThresholdPregnant(string V)
		{
			if (global::XX.X.Nm(V, 0f, false) >= 100f)
			{
				return TX.Get("Config_not_visible", "");
			}
			return V + "%";
		}

		private string fnDescConvertPressBalance(string V)
		{
			if (V == "100")
			{
				return TX.Get("Config_press_balance_fare", "");
			}
			float num = global::XX.X.Nm(V, 0f, false) - 100f;
			if (num < 0f)
			{
				return TX.GetA("Config_press_balance_n", (-num).ToString());
			}
			return TX.GetA("Config_press_balance_t", num.ToString());
		}

		private bool fnChangeConfigValue(aBtnMeter _B, float pre_value, float cur_value)
		{
			return !_B.isLocked() && CFG.changeConfigValue(_B.title, cur_value, this);
		}

		public static bool changeConfigValue(string key, float cur_value, CFG CfgInstance = null)
		{
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 2129104165U)
				{
					if (num <= 1258391049U)
					{
						if (num <= 290938845U)
						{
							if (num <= 92627239U)
							{
								if (num <= 65495999U)
								{
									if (num != 39734714U)
									{
										if (num != 65495999U)
										{
											goto IL_0EA3;
										}
										if (!(key == "GO_Position"))
										{
											goto IL_0EA3;
										}
										CFG.go_text_pos_top = cur_value != 0f;
										return true;
									}
									else
									{
										if (!(key == "spconfig_use_uipic_press_enemy"))
										{
											goto IL_0EA3;
										}
										CFG.sp_use_uipic_press_enemy = (byte)cur_value;
										return true;
									}
								}
								else if (num != 71514319U)
								{
									if (num != 92627239U)
									{
										goto IL_0EA3;
									}
									if (!(key == "spconfig_deadburned"))
									{
										goto IL_0EA3;
									}
									CFG.sp_deadburned = cur_value != 0f;
									return true;
								}
								else
								{
									if (!(key == "UiEf_Density"))
									{
										goto IL_0EA3;
									}
									CFG.ui_effect_density = (byte)cur_value;
									return true;
								}
							}
							else if (num <= 224660360U)
							{
								if (num != 98480248U)
								{
									if (num != 224660360U)
									{
										goto IL_0EA3;
									}
									if (!(key == "spconfig_voice_for_pleasure2m"))
									{
										goto IL_0EA3;
									}
									CFG.sp_voice_for_pleasure2m = cur_value / 100f;
									return true;
								}
								else
								{
									if (!(key == "spconfig_ep_boost_mouth"))
									{
										goto IL_0EA3;
									}
									CFG.Osp_ep_boost[EPCATEG.MOUTH] = (byte)cur_value;
									return true;
								}
							}
							else if (num != 256173129U)
							{
								if (num != 290938845U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_cloth_strength"))
								{
									goto IL_0EA3;
								}
								CFG.sp_cloth_strength = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "spconfig_dmg_multivoice"))
								{
									goto IL_0EA3;
								}
								CFG.sp_dmg_multivoice = (byte)cur_value;
								return true;
							}
						}
						else if (num <= 731969625U)
						{
							if (num <= 375697930U)
							{
								if (num != 337898362U)
								{
									if (num != 375697930U)
									{
										goto IL_0EA3;
									}
									if (!(key == "spconfig_use_uipic_press_balance"))
									{
										goto IL_0EA3;
									}
									CFG.sp_use_uipic_press_balance = (byte)cur_value;
									return true;
								}
								else
								{
									if (!(key == "spconfig_milk"))
									{
										goto IL_0EA3;
									}
									CFG.sp_publish_milk = (byte)cur_value;
									return true;
								}
							}
							else if (num != 716595213U)
							{
								if (num != 731969625U)
								{
									goto IL_0EA3;
								}
								if (!(key == "UiEf_Level"))
								{
									goto IL_0EA3;
								}
								global::XX.X.EF_LEVEL_UI = 1f - cur_value / 100f;
								return true;
							}
							else
							{
								if (!(key == "MagSel_Decide"))
								{
									goto IL_0EA3;
								}
								CFG.magsel_decide_type = (CFG.MAGSEL_TYPE)cur_value;
								return true;
							}
						}
						else if (num <= 1136986463U)
						{
							if (num != 747116006U)
							{
								if (num != 1136986463U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_ep_boost_anal"))
								{
									goto IL_0EA3;
								}
								CFG.Osp_ep_boost[EPCATEG.ANAL] = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "VSync"))
								{
									goto IL_0EA3;
								}
								global::XX.X.v_sync = cur_value != 0f;
								return true;
							}
						}
						else if (num != 1175287288U)
						{
							if (num != 1258391049U)
							{
								goto IL_0EA3;
							}
							if (!(key == "SEV"))
							{
								goto IL_0EA3;
							}
							SND.volume = global::XX.X.IntR(cur_value);
							if (M2DBase.Instance != null)
							{
								M2DBase.Instance.Snd.fineVolume();
							}
							SND.Ui.play("talk_progress", false);
							return true;
						}
						else
						{
							if (!(key == "spconfig_uipic_lr"))
							{
								goto IL_0EA3;
							}
							CFG.sp_uipic_lr = (CFG.UIPIC_LR)cur_value;
							return true;
						}
					}
					else if (num <= 1701121667U)
					{
						if (num <= 1409041749U)
						{
							if (num <= 1363510949U)
							{
								if (num != 1318626345U)
								{
									if (num != 1363510949U)
									{
										goto IL_0EA3;
									}
									if (!(key == "BGMV"))
									{
										goto IL_0EA3;
									}
								}
								else
								{
									if (!(key == "spconfig_opacity_marunomi"))
									{
										goto IL_0EA3;
									}
									CFG.sp_opacity_marunomi = (byte)cur_value;
									return true;
								}
							}
							else if (num != 1391843111U)
							{
								if (num != 1409041749U)
								{
									goto IL_0EA3;
								}
								if (!(key == "PostEf_Level"))
								{
									goto IL_0EA3;
								}
								CFG.posteffect_weaken = (byte)(cur_value / 10f);
								return true;
							}
							else
							{
								if (!(key == "Window_Size"))
								{
									goto IL_0EA3;
								}
								if (CfgInstance != null)
								{
									CfgInstance.temp_window_size = CfgInstance.Ascreen_width[global::XX.X.IntR(cur_value)];
									return true;
								}
								return true;
							}
						}
						else if (num <= 1469028275U)
						{
							if (num != 1447527407U)
							{
								if (num != 1469028275U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_go_cheat"))
								{
									goto IL_0EA3;
								}
								CFG.sp_go_cheat = cur_value != 0f;
								return true;
							}
							else
							{
								if (!(key == "autorun"))
								{
									goto IL_0EA3;
								}
								goto IL_0C51;
							}
						}
						else if (num != 1574728676U)
						{
							if (num != 1701121667U)
							{
								goto IL_0EA3;
							}
							if (!(key == "Draw_Fps"))
							{
								goto IL_0EA3;
							}
							global::XX.X.AF = (int)(cur_value + 1f);
							if (global::XX.X.AF_EF >= global::XX.X.AF)
							{
								return true;
							}
							global::XX.X.AF_EF = global::XX.X.AF;
							if (CfgInstance != null)
							{
								CfgInstance.BxOut.Get("Draw_EfFps", false).setValue((global::XX.X.AF_EF - 1).ToString());
								return true;
							}
							return true;
						}
						else
						{
							if (!(key == "Running_Double"))
							{
								goto IL_0EA3;
							}
							M2MoverPr.double_tap_running = cur_value > 0.5f;
							return true;
						}
					}
					else if (num <= 1788935810U)
					{
						if (num <= 1737481301U)
						{
							if (num != 1719909786U)
							{
								if (num != 1737481301U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_dmgcounter_position"))
								{
									goto IL_0EA3;
								}
								CFG.sp_dmgcounter_position = (CFG.UIPIC_DMGCNT)cur_value;
								return true;
							}
							else
							{
								if (!(key == "spconfig_ep_boost_gspot"))
								{
									goto IL_0EA3;
								}
								CFG.Osp_ep_boost[EPCATEG.GSPOT] = (byte)cur_value;
								return true;
							}
						}
						else if (num != 1778072347U)
						{
							if (num != 1788935810U)
							{
								goto IL_0EA3;
							}
							if (!(key == "spconfig_epdmg_vo_mouth"))
							{
								goto IL_0EA3;
							}
							CFG.sp_epdmg_vo_mouth = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "Shield_Hold_Evadable"))
							{
								goto IL_0EA3;
							}
							CFG.shield_hold_evadable = cur_value != 0f;
							return true;
						}
					}
					else if (num <= 1902301588U)
					{
						if (num != 1789562456U)
						{
							if (num != 1902301588U)
							{
								goto IL_0EA3;
							}
							if (!(key == "spconfig_cloth_broken_debuff"))
							{
								goto IL_0EA3;
							}
							CFG.sp_cloth_broken_debuff = cur_value != 0f;
							return true;
						}
						else
						{
							if (!(key == "spconfig_dmgte_ui_density"))
							{
								goto IL_0EA3;
							}
							CFG.sp_dmgte_ui_density = (byte)cur_value;
							return true;
						}
					}
					else if (num != 2022456398U)
					{
						if (num != 2072892167U)
						{
							if (num != 2129104165U)
							{
								goto IL_0EA3;
							}
							if (!(key == "spconfig_voice_for_pleasure"))
							{
								goto IL_0EA3;
							}
							CFG.sp_voice_for_pleasure = cur_value / 100f;
							return true;
						}
						else
						{
							if (!(key == "spconfig_ep_boost_ear"))
							{
								goto IL_0EA3;
							}
							CFG.Osp_ep_boost[EPCATEG.EAR] = (byte)cur_value;
							return true;
						}
					}
					else
					{
						if (!(key == "UI_KD_appearance"))
						{
							goto IL_0EA3;
						}
						KEY.keydesc_appearance = (byte)cur_value;
						return true;
					}
				}
				else if (num <= 3155854328U)
				{
					if (num <= 2702396622U)
					{
						if (num <= 2336147325U)
						{
							if (num <= 2258767131U)
							{
								if (num != 2233508368U)
								{
									if (num != 2258767131U)
									{
										goto IL_0EA3;
									}
									if (!(key == "vib_level"))
									{
										goto IL_0EA3;
									}
									CFG.vib_level = (byte)cur_value;
									NEL.Instance.Vib.base_level = cur_value * 0.01f;
									return true;
								}
								else
								{
									if (!(key == "Difficulty"))
									{
										goto IL_0EA3;
									}
									DIFF.I = global::XX.X.IntR(cur_value);
									return true;
								}
							}
							else if (num != 2287149779U)
							{
								if (num != 2336147325U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_ep_boost_cli"))
								{
									goto IL_0EA3;
								}
								CFG.Osp_ep_boost[EPCATEG.CLI] = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "AutoSave"))
								{
									goto IL_0EA3;
								}
								CFG.autosave_timing = (byte)cur_value;
								return true;
							}
						}
						else if (num <= 2499870687U)
						{
							if (num != 2415980917U)
							{
								if (num != 2499870687U)
								{
									goto IL_0EA3;
								}
								if (!(key == "UiEf_Dirty"))
								{
									goto IL_0EA3;
								}
								CFG.ui_effect_dirty = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "ui_sensitive_description"))
								{
									goto IL_0EA3;
								}
								CFG.ui_sensitive_description = (byte)cur_value;
								return true;
							}
						}
						else if (num != 2695456225U)
						{
							if (num != 2702396622U)
							{
								goto IL_0EA3;
							}
							if (!(key == "stick_thresh"))
							{
								goto IL_0EA3;
							}
							M2MoverPr.running_thresh = cur_value;
							return true;
						}
						else
						{
							if (!(key == "Running_Reverse"))
							{
								goto IL_0EA3;
							}
							goto IL_0C51;
						}
					}
					else if (num <= 3070305732U)
					{
						if (num <= 2859991500U)
						{
							if (num != 2802935069U)
							{
								if (num != 2859991500U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_dmgte_pixel_duration"))
								{
									goto IL_0EA3;
								}
								CFG.sp_dmgte_pixel_duration = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "spconfig_use_uipic_press_gimmick"))
								{
									goto IL_0EA3;
								}
								CFG.sp_use_uipic_press_gimmick = cur_value != 0f;
								return true;
							}
						}
						else if (num != 2963279150U)
						{
							if (num != 3070305732U)
							{
								goto IL_0EA3;
							}
							if (!(key == "Magsel"))
							{
								goto IL_0EA3;
							}
							CFG.magsel_slow = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "spconfig_epdmg_vo_iku"))
							{
								goto IL_0EA3;
							}
							CFG.sp_epdmg_vo_iku = (byte)cur_value;
							return true;
						}
					}
					else if (num <= 3093531377U)
					{
						if (num != 3086297070U)
						{
							if (num != 3093531377U)
							{
								goto IL_0EA3;
							}
							if (!(key == "spconfig_ep_boost_vagina"))
							{
								goto IL_0EA3;
							}
							CFG.Osp_ep_boost[EPCATEG.VAGINA] = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "USel_Z_Push"))
							{
								goto IL_0EA3;
							}
							CFG.item_usel_type = (CFG.USEL_TYPE)cur_value;
							return true;
						}
					}
					else if (num != 3132036350U)
					{
						if (num != 3152328284U)
						{
							if (num != 3155854328U)
							{
								goto IL_0EA3;
							}
							if (!(key == "Kisekae"))
							{
								goto IL_0EA3;
							}
							CFG.temp_kisekae = (int)((byte)cur_value);
							return true;
						}
						else
						{
							if (!(key == "spconfig_ep_boost_canal"))
							{
								goto IL_0EA3;
							}
							CFG.Osp_ep_boost[EPCATEG.CANAL] = (byte)cur_value;
							return true;
						}
					}
					else
					{
						if (!(key == "SE_stereo"))
						{
							goto IL_0EA3;
						}
						M2SoundPlayer.monoral = cur_value == 0f;
						return true;
					}
				}
				else if (num <= 3317469770U)
				{
					if (num <= 3210407456U)
					{
						if (num <= 3180355272U)
						{
							if (num != 3163520979U)
							{
								if (num != 3180355272U)
								{
									goto IL_0EA3;
								}
								if (!(key == "spconfig_ep_boost_urethra"))
								{
									goto IL_0EA3;
								}
								CFG.Osp_ep_boost[EPCATEG.URETHRA] = (byte)cur_value;
								return true;
							}
							else
							{
								if (!(key == "MapEf_Level"))
								{
									goto IL_0EA3;
								}
								global::XX.X.EF_LEVEL_NORMAL = 1f - cur_value / 100f;
								return true;
							}
						}
						else if (num != 3198812103U)
						{
							if (num != 3210407456U)
							{
								goto IL_0EA3;
							}
							if (!(key == "UiEf_Dirty_Blood"))
							{
								goto IL_0EA3;
							}
							CFG.blood_weaken = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "spconfig_juice_cutin"))
							{
								goto IL_0EA3;
							}
							CFG.sp_juice_cutin = cur_value != 0f;
							return true;
						}
					}
					else if (num <= 3237703372U)
					{
						if (num != 3235683140U)
						{
							if (num != 3237703372U)
							{
								goto IL_0EA3;
							}
							if (!(key == "VOV"))
							{
								goto IL_0EA3;
							}
							SND.voice_volume = global::XX.X.IntR(cur_value);
							return true;
						}
						else
						{
							if (!(key == "Windowed"))
							{
								goto IL_0EA3;
							}
							CFG.fullscreen_mode = cur_value != 0f;
							return true;
						}
					}
					else if (num != 3271972118U)
					{
						if (num != 3317469770U)
						{
							goto IL_0EA3;
						}
						if (!(key == "UiEf_TimeScale"))
						{
							goto IL_0EA3;
						}
						global::XX.X.EF_TIMESCALE_UI = 1f - cur_value / 100f;
						return true;
					}
					else
					{
						if (!(key == "Draw_EfFps"))
						{
							goto IL_0EA3;
						}
						global::XX.X.AF_EF = (int)(cur_value + 1f);
						return true;
					}
				}
				else if (num <= 3921204200U)
				{
					if (num <= 3579481448U)
					{
						if (num != 3377067378U)
						{
							if (num != 3579481448U)
							{
								goto IL_0EA3;
							}
							if (!(key == "spconfig_dmgte_ui_duration"))
							{
								goto IL_0EA3;
							}
							CFG.sp_dmgte_ui_duration = (byte)cur_value;
							return true;
						}
						else
						{
							if (!(key == "Running_Stick_Thresh"))
							{
								goto IL_0EA3;
							}
							M2MoverPr.running_thresh = cur_value / 100f;
							return true;
						}
					}
					else if (num != 3623002796U)
					{
						if (num != 3921204200U)
						{
							goto IL_0EA3;
						}
						if (!(key == "spconfig_threshold_pregnant"))
						{
							goto IL_0EA3;
						}
						CFG.sp_threshold_pregnant = (byte)cur_value;
						return true;
					}
					else
					{
						if (!(key == "spconfig_dmgte_pixel_density"))
						{
							goto IL_0EA3;
						}
						CFG.sp_dmgte_pixel_density = (byte)cur_value;
						return true;
					}
				}
				else if (num <= 4044785525U)
				{
					if (num != 3956288208U)
					{
						if (num != 4044785525U)
						{
							goto IL_0EA3;
						}
						if (!(key == "Lang"))
						{
							goto IL_0EA3;
						}
						TX.changeFamilyToIndex((int)cur_value);
						if (CfgInstance != null)
						{
							CfgInstance.fnShowDesc(CfgInstance.BxOut.getBtn("Lang"));
							return true;
						}
						return true;
					}
					else
					{
						if (!(key == "spconfig_ep_boost_breast"))
						{
							goto IL_0EA3;
						}
						CFG.Osp_ep_boost[EPCATEG.BREAST] = (byte)cur_value;
						return true;
					}
				}
				else if (num != 4127457007U)
				{
					if (num != 4171608391U)
					{
						if (num != 4241328060U)
						{
							goto IL_0EA3;
						}
						if (!(key == "bgm_volume"))
						{
							goto IL_0EA3;
						}
					}
					else
					{
						if (!(key == "spconfig_juice"))
						{
							goto IL_0EA3;
						}
						CFG.sp_publish_juice = (byte)cur_value;
						return true;
					}
				}
				else
				{
					if (!(key == "Sensitive"))
					{
						goto IL_0EA3;
					}
					global::XX.X.sensitive_level = (byte)cur_value;
					return true;
				}
				SND.bgm_volume = global::XX.X.IntR(cur_value);
				return true;
				IL_0C51:
				M2MoverPr.jump_press_reverse = cur_value > 0.5f;
				return true;
			}
			IL_0EA3:
			global::XX.X.de("不明なコンフィグ指定: " + key, null);
			return true;
		}

		private bool fnShowDesc(aBtn B)
		{
			if (this.BxDesc == null)
			{
				return true;
			}
			string title = B.title;
			string text;
			if (title != null)
			{
				if (title == "VSync")
				{
					text = TX.GetA("Config_desc_" + B.title, IN.enable_vsync ? TX.Get("Enabled", "") : TX.Get("Disabled", ""));
					goto IL_00FD;
				}
				if (title == "spconfig_dmgte_pixel_duration")
				{
					text = TX.Get("Config_desc_spconfig_dmgte_pixel_density", "");
					goto IL_00FD;
				}
				if (title == "spconfig_dmgte_ui_duration")
				{
					text = TX.Get("Config_desc_spconfig_dmgte_ui_density", "");
					goto IL_00FD;
				}
			}
			string text2;
			if (TX.isStart(B.title, "spconfig_ep_boost_", out text2, 0))
			{
				text = TX.GetA("Config_desc_spconfig_ep_boost", TX.Get("EP_Targ_" + text2, ""));
			}
			else
			{
				text = TX.Get("Config_desc_" + B.title, "");
			}
			IL_00FD:
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
					this.ui_state = CFG.STATE.KEYCON;
					this.OPreSelected["MAIN"] = B;
				}
			}
			return true;
		}

		public CFG destruct()
		{
			CFG.BaRevertData = null;
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
				if (global::XX.X.SENSITIVE && prNoel.BetoMng.is_torned)
				{
					prNoel.BetoMng.setTorned(prNoel, false, true);
				}
				if (this.pre_sensitive_level != global::XX.X.sensitive_level && prNoel.UP != null)
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
			this.pre_sensitive_level = global::XX.X.sensitive_level;
			if (global::XX.X.v_sync != this.pre_vsync)
			{
				this.pre_vsync = global::XX.X.v_sync;
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
			if (this.ui_state == CFG.STATE.STICK_SENSITIVE || this.ui_state == CFG.STATE.STICK_SENSITIVE_AUTO)
			{
				this.quitStickSensitivityCheck();
			}
			if (this.ui_state == CFG.STATE.KEYCON)
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
			this.ui_state = CFG.STATE.INITTED;
			this.BxOut.Focus();
			aBtn aBtn = this.getFirstFocusButton();
			if (aBtn == null)
			{
				aBtn = this.CurShowingTab.getBtn(0);
			}
			if (aBtn != null)
			{
				aBtn.Select(false);
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
			if (this.CurShowingTab != null && global::XX.X.GetKeyByValue<string, Designer>(this.OTab, this.CurShowingTab, out text))
			{
				tabname = text;
				aBtn aBtn = global::XX.X.Get<string, aBtn>(this.OPreSelected, text);
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
			if (this.ui_state == CFG.STATE.INITTED)
			{
				this.ui_state = CFG.STATE.ACTIVE;
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
			else if (this.ui_state == CFG.STATE.ACTIVE)
			{
				if (IN.isCancel() && !CtSetter.hasFocus())
				{
					flag = false;
				}
				if (this.RTabSp != null)
				{
					this.RTabSp.runLRInput(-2);
				}
			}
			else if (this.ui_state == CFG.STATE.STICK_SENSITIVE_AUTO)
			{
				if (IN.isCancel() || this.runAutoStickSensitivity())
				{
					this.initStickSensitivityCheck(CFG.STATE.STICK_SENSITIVE);
				}
			}
			else if (this.ui_state == CFG.STATE.STICK_SENSITIVE)
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
						aBtn3.Select(false);
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
			if (CFG.BaRevertData != null)
			{
				CFG.BaRevertData.position = 0UL;
				CFG.readBinary(CFG.BaRevertData, true);
			}
			CFG.BaRevertData = null;
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
					firstFocusButton.Select(false);
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

		public CFG reveal(Transform T, float posx = 0f, float posy = 0f, bool animate = true)
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

		public CFG reveal(IDesignerBlock B, bool animate = true, REVEALTYPE type = REVEALTYPE.ALWAYS)
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
				global::XX.X.GetKeyByValue<string, Designer>(this.OTab, this.CurShowingTab, out key);
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
				this.initStickSensitivityCheck(CFG.STATE.STICK_SENSITIVE_AUTO);
				return;
			}
			this.initStickSensitivityCheck(CFG.STATE.STICK_SENSITIVE);
		}

		private void initStickSensitivityCheck(CFG.STATE stt)
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
				if (stt == CFG.STATE.STICK_SENSITIVE)
				{
					dsnDataP.sheight = 35f;
					SND.Ui.play("reset_var", false);
					LabeledInputField labeledInputField = tab.addInput(new DsnDataInput
					{
						min = 0.0,
						max = 1.0,
						number = true,
						def = global::XX.X.spr_after(1f - this.cur_sensitivity_level, 4),
						bounds_w = 266f,
						label = TX.Get("Stick_sensitivity_value", ""),
						fnChangedDelay = delegate(LabeledInputField LI)
						{
							this.cur_sensitivity_level = global::XX.X.saturate(1f - global::XX.X.Nm(LI.text, 0.5f, false));
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
					aBtn2.Select(false);
				}
				if (stt == CFG.STATE.STICK_SENSITIVE_AUTO)
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
					aBtnNel.Select(false);
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
			this.ui_state = CFG.STATE.ACTIVE;
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
				Md.base_px_x = (float)((i == -1) ? 0 : (-120 * global::XX.X.MPF(i == 0)));
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
							float num9 = global::XX.CAim.get_agR((global::XX.AIM)j, 0f);
							Md.Arc2(0f, 0f, 84f, 84f, num9 - 0.7853982f, num9 + 0.7853982f, 0f, 0f, 1f);
						}
					}
				}
				if (this.ui_state == CFG.STATE.STICK_SENSITIVE_AUTO)
				{
					Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.4f : 1f) * (0.35f + 0.2f * global::XX.X.COSIT(45f)));
					Md.Poly(0f, 0f, 80f, 0f, 24, 0f, false, 0f, 0f);
				}
				else
				{
					Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.4f : 1f));
					Md.Poly(0f, 0f, 80f, 0f, 24, 1f, false, 0f, 0f);
				}
				Md.chooseSubMesh(1, false, false);
				float num10 = 80f * num3;
				Md.ButtonKadomaruDashedM(0f, 0f, num10 * 2f, num10 * 2f, num10, global::XX.X.IntR(num10 * 2f * 3.1415927f / 12f), 2f, false, 0.5f, 24);
				Md.chooseSubMesh(0, false, false);
				Md.Col = C32.MulA(4283780170U, alpha * ((stickControl == null) ? 0.11f : 0.4f));
				Md.Poly(0f, 0f, num10, 0f, 24, 0f, false, 0f, 0f);
				if (stickControl != null)
				{
					Vector2 vector = new Vector2(-num4 + num5, num6 - num7) * 80f;
					if (vector.x != 0f || vector.y != 0f)
					{
						float num11 = global::XX.X.GAR2(0f, 0f, vector.x, vector.y);
						vector.x *= global::XX.X.Abs(global::XX.X.Cos(num11));
						vector.y *= global::XX.X.Abs(global::XX.X.Sin(num11));
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
							aBtn.Select(false);
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
							aBtn2.Select(false);
						}
					}
				}
			}
			return false;
		}

		private bool runAutoStickSensitivity()
		{
			bool flag = this.ui_state == CFG.STATE.STICK_SENSITIVE_AUTO;
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
							num4 = global::XX.X.Mn(0.9375f, global::XX.X.Mx(num4, num10));
						}
						if (!flag && num10 > num3 * 0.5f)
						{
							flag2 = true;
						}
					}
				}
			}
			num4 = global::XX.X.Mx(num4, this.cur_sensitivity_level);
			if (!flag)
			{
				return flag2;
			}
			if (num4 != this.cur_sensitivity_level)
			{
				this.cur_sensitivity_level = global::XX.X.MULWALKMX(this.cur_sensitivity_level, num4, 0.04f, 0.004f);
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
							this.initStickSensitivityCheck(CFG.STATE.STICK_SENSITIVE_AUTO);
						}
					}
					else
					{
						this.quitStickSensitivityCheck();
					}
				}
				else
				{
					this.initStickSensitivityCheck(CFG.STATE.STICK_SENSITIVE);
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

		private static bool fullscreen_mode = false;

		public const string sd_file_name = "config.cfg";

		public static bool first_startup = true;

		private static byte autosave_timing = 3;

		public static byte magsel_slow = 4;

		public static byte ui_effect_dirty = 5;

		public static byte ui_sensitive_description = 5;

		public const byte ui_effect_dirty_max = 10;

		public static byte ui_effect_density = 10;

		public const byte ui_effect_density_thesh = 10;

		public const int ui_effect_density_max = 20;

		public static bool simplify_bg_drawing = true;

		public static bool shield_hold_evadable = true;

		public static CFG.USEL_TYPE item_usel_type;

		public static byte posteffect_weaken = 0;

		public static byte fatal_automode_speed = 8;

		public static bool go_text_pos_top = true;

		public static byte vib_level = 100;

		public static CFG.MAGSEL_TYPE magsel_decide_type = CFG.MAGSEL_TYPE.NORMAL;

		public static byte FATAL_SPEED_MAX = 18;

		public const byte POSTEFFECT_WEAKEN_MAX = 10;

		public static CFG.UIPIC_LR sp_uipic_lr;

		public static byte sp_dmg_multivoice;

		public static byte sp_cloth_strength = 100;

		public static byte sp_threshold_pregnant = 50;

		public static byte sp_epdmg_vo_mouth = 100;

		public static byte sp_epdmg_vo_iku = 0;

		public const int CLOTH_STR_MAX = 200;

		public static bool sp_cloth_broken_debuff;

		public static byte sp_publish_milk;

		public static byte sp_publish_juice = 100;

		public static bool sp_juice_cutin;

		public static bool sp_deadburned;

		public static byte sp_dmgte_pixel_density = 100;

		public static byte sp_dmgte_pixel_duration = 100;

		public static byte sp_dmgte_ui_density = 100;

		public static byte sp_dmgte_ui_duration = 100;

		public static float sp_voice_for_pleasure = 1f;

		public static float sp_voice_for_pleasure2m = 0f;

		public static bool sp_use_uipic_press_gimmick = false;

		public static byte sp_use_uipic_press_enemy = 0;

		public static byte sp_use_uipic_press_balance = 100;

		public static CFG.UIPIC_DMGCNT sp_dmgcounter_position = CFG.UIPIC_DMGCNT.MAP;

		public static bool sp_go_cheat = false;

		private static readonly BDic<EPCATEG, byte> Osp_ep_boost = new BDic<EPCATEG, byte>(9);

		public static byte sp_opacity_marunomi = 100;

		private static readonly List<string> Aenabled_sp_value = new List<string>();

		public const int CFG_VER = 13;

		public const int CFG_VER_FIRST = 10;

		private static int temp_kisekae;

		private static ByteArray BaRevertData;

		public CFG.STATE ui_state;

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

		public enum MAGSEL_TYPE
		{
			NORMAL,
			SUBMIT,
			Z,
			C
		}

		public enum USEL_TYPE : byte
		{
			D_RELEASE,
			Z_PUSH,
			D_TAP_Z
		}

		public enum UIPIC_LR : byte
		{
			L,
			R,
			NONE
		}

		public enum UIPIC_DMGCNT : byte
		{
			NONE,
			MAP,
			UI,
			MAPUI
		}

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
