using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public static class CFGSP
	{
		public static void newGameSp()
		{
			CFGSP.Aenabled_value.Clear();
		}

		public static void clearSpVariable()
		{
			CFGSP.uipic_lr = CFGSP.UIPIC_LR.L;
			CFGSP.dmg_multivoice = 0;
			CFGSP.cloth_strength = 100;
			CFGSP.cloth_broken_debuff = false;
			CFGSP.publish_milk = 0;
			CFGSP.epdmg_vo_mouth = 100;
			CFGSP.epdmg_vo_iku = 0;
			CFGSP.publish_juice = 100;
			CFGSP.juice_cutin = false;
			CFGSP.deadburned = false;
			CFGSP.dmgte_pixel_density = 100;
			CFGSP.dmgte_pixel_duration = 100;
			CFGSP.dmgte_ui_density = 100;
			CFGSP.dmgte_ui_duration = 100;
			CFGSP.voice_for_pleasure = 1f;
			CFGSP.voice_for_pleasure2m = 20f;
			CFGSP.use_uipic_press_gimmick = false;
			CFGSP.use_uipic_press_enemy = 0;
			CFGSP.use_uipic_press_balance = 100;
			CFGSP.go_cheat = false;
			CFGSP.dmgcounter_position = CFGSP.UIPIC_DMGCNT.MAP;
			CFGSP.opacity_marunomi = 100;
			CFGSP.threshold_pregnant = 50;
			CFGSP.gameover_counter = 0;
			CFGSP.frozen_dmg_voice = 100;
			CFGSP.stone_dmg_voice = 0;
			CFGSP.frozen_lock_orgasm = false;
			CFGSP.stone_lock_orgasm = true;
			CFGSP.SpEpBoostPrepare(true);
		}

		public static void ResetVariable(UiCFG Cfg, UiBoxDesigner Container, Designer Tab, FnBtnBindings FD_fnShowDesc)
		{
			CFGSP.clearSpVariable();
			float num = ((Tab.getScrollBox() != null) ? Tab.getScrollBox().scrolled_level_y : (-1f));
			Tab.ClearRowGameObject();
			Tab.Clear();
			Container.assignCurrentTargetTabManual(Tab);
			Cfg.setBoxMainStencil();
			CFGSP.createBoxDesignerContentSp(Cfg, Container, Tab, FD_fnShowDesc);
			Container.assignCurrentTargetTabManual(null);
			if (num >= 0f)
			{
				Tab.getScrollBox().setScrollLevelTo(0f, num, false);
			}
			Tab.getBtn("ResetAllSP").Select(true);
			IN.clearPushDown(true);
		}

		private static bool isSpEpBoostTarget(EPCATEG epcateg)
		{
			return epcateg != EPCATEG.OTHER && epcateg != EPCATEG.UTERUS;
		}

		private static void SpEpBoostPrepare(bool force = false)
		{
			if (CFGSP.Oep_boost.Count == 0 || force)
			{
				for (int i = 0; i < 11; i++)
				{
					EPCATEG epcateg = (EPCATEG)i;
					if (CFGSP.isSpEpBoostTarget(epcateg))
					{
						CFGSP.Oep_boost[epcateg] = 100;
					}
				}
			}
		}

		public static float getEpApplyRatio(EPCATEG epcateg)
		{
			byte b;
			if (!CFGSP.Oep_boost.TryGetValue(epcateg, out b))
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

		public static bool isSpActivated()
		{
			return CFGSP.Aenabled_value.Count > 0;
		}

		public static void writeToBytes(ByteArray Ba, int phase)
		{
			if (phase == 0)
			{
				Ba.writeByte((int)CFGSP.uipic_lr);
				Ba.writeByte((int)CFGSP.dmg_multivoice);
				Ba.writeByte((int)CFGSP.cloth_strength);
				Ba.writeByte(CFGSP.cloth_broken_debuff ? 1 : 0);
				Ba.writeByte((int)CFGSP.publish_milk);
				Ba.writeByte((int)CFGSP.publish_juice);
				Ba.writeByte(CFGSP.deadburned ? 1 : 0);
				Ba.writeByte((int)CFGSP.dmgte_pixel_density);
				Ba.writeByte((int)CFGSP.dmgte_pixel_duration);
				Ba.writeByte((int)CFGSP.dmgte_ui_density);
				Ba.writeByte((int)CFGSP.dmgte_ui_duration);
				Ba.writeByte(CFGSP.juice_cutin ? 1 : 0);
				Ba.writeFloat(CFGSP.voice_for_pleasure);
				Ba.writeFloat(CFGSP.voice_for_pleasure2m);
				CFGSP.SpEpBoostPrepare(false);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.CLI]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.VAGINA]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.CANAL]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.GSPOT]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.URETHRA]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.BREAST]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.ANAL]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.MOUTH]);
				Ba.writeByte((int)CFGSP.Oep_boost[EPCATEG.EAR]);
				Ba.writeBool(CFGSP.go_cheat);
				Ba.writeByte((int)CFGSP.opacity_marunomi);
				Ba.writeByte((int)CFGSP.epdmg_vo_mouth);
				Ba.writeBool(CFGSP.use_uipic_press_gimmick);
				Ba.writeByte((int)CFGSP.use_uipic_press_balance);
				Ba.writeByte((int)CFGSP.use_uipic_press_enemy);
				Ba.writeByte((int)CFGSP.dmgcounter_position);
				return;
			}
			if (phase == 1)
			{
				Ba.writeByte((int)CFGSP.epdmg_vo_iku);
				Ba.writeByte((int)CFGSP.threshold_pregnant);
				Ba.writeByte((int)CFGSP.gameover_counter);
				Ba.writeByte((int)CFGSP.frozen_dmg_voice);
				Ba.writeByte((int)CFGSP.stone_dmg_voice);
				Ba.writeBool(CFGSP.frozen_lock_orgasm);
				Ba.writeBool(CFGSP.stone_lock_orgasm);
			}
		}

		public static void readBinaryFrom(ByteReader Ba, int phase)
		{
			if (phase == 0)
			{
				CFGSP.clearSpVariable();
				CFGSP.uipic_lr = (CFGSP.UIPIC_LR)Ba.readUByte();
				CFGSP.dmg_multivoice = Ba.readUByte();
				CFGSP.cloth_strength = Ba.readUByte();
				CFGSP.cloth_broken_debuff = Ba.readUByte() > 0;
				CFGSP.publish_milk = Ba.readUByte();
				CFGSP.publish_juice = Ba.readUByte();
				CFGSP.deadburned = Ba.readUByte() > 0;
				CFGSP.dmgte_pixel_density = Ba.readUByte();
				CFGSP.dmgte_pixel_duration = Ba.readUByte();
				CFGSP.dmgte_ui_density = Ba.readUByte();
				CFGSP.dmgte_ui_duration = Ba.readUByte();
				CFGSP.juice_cutin = Ba.readUByte() > 0;
				CFGSP.voice_for_pleasure = Ba.readFloat();
				CFGSP.voice_for_pleasure2m = Ba.readFloat();
				CFGSP.Oep_boost[EPCATEG.CLI] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.VAGINA] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.CANAL] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.GSPOT] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.URETHRA] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.BREAST] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.ANAL] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.MOUTH] = Ba.readUByte();
				CFGSP.Oep_boost[EPCATEG.EAR] = Ba.readUByte();
				CFGSP.go_cheat = Ba.readUByte() > 0;
				CFGSP.opacity_marunomi = Ba.readUByte();
				CFGSP.epdmg_vo_mouth = Ba.readUByte();
				CFGSP.use_uipic_press_gimmick = Ba.readBoolean();
				CFGSP.use_uipic_press_balance = Ba.readUByte();
				CFGSP.use_uipic_press_enemy = Ba.readUByte();
				CFGSP.dmgcounter_position = (CFGSP.UIPIC_DMGCNT)Ba.readByte();
				return;
			}
			if (phase == 1)
			{
				CFGSP.epdmg_vo_iku = Ba.readUByte();
				CFGSP.threshold_pregnant = Ba.readUByte();
				CFGSP.gameover_counter = Ba.readUByte();
				CFGSP.frozen_dmg_voice = Ba.readUByte();
				CFGSP.stone_dmg_voice = Ba.readUByte();
				CFGSP.frozen_lock_orgasm = Ba.readBoolean();
				CFGSP.stone_lock_orgasm = Ba.readBoolean();
			}
		}

		public static void readBinarySp(ByteReader Ba)
		{
			int num = Ba.readByte();
			for (int i = 0; i < num; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				if (CFGSP.Aenabled_value.IndexOf(text) == -1)
				{
					CFGSP.Aenabled_value.Add(text);
				}
			}
		}

		public static void writeBinarySp(ByteArray Ba)
		{
			int count = CFGSP.Aenabled_value.Count;
			Ba.writeByte(count);
			for (int i = 0; i < count; i++)
			{
				string text = CFGSP.Aenabled_value[i];
				Ba.writePascalString(text, "utf-8");
			}
		}

		public static void addSp(string key)
		{
			if (!CFGSP.isSpEnabled(key))
			{
				CFGSP.Aenabled_value.Add(key);
				if (key != null && key == "go_cheat")
				{
					CFGSP.go_cheat = true;
				}
			}
		}

		public static bool isSpEnabled(string key)
		{
			return CFGSP.Aenabled_value.IndexOf(key) >= 0;
		}

		public static void createBoxDesignerContentSp(UiCFG Cfg, UiBoxDesigner Container, Designer Tab, FnBtnBindings FD_fnShowDesc)
		{
			FnBtnMeterLine fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
			{
				int num2 = X.IntR(val);
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
				int num3 = X.IntR(val);
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
				int num4 = X.IntR(val);
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
			FnBtnMeterLine fnBtnMeterLine4 = delegate(aBtnMeter B, int index, float vals)
			{
				int num5 = X.IntR(vals);
				if (num5 <= 6)
				{
					if (num5 == 0)
					{
						return 0.125f;
					}
					if (num5 != 1 && num5 != 6)
					{
						goto IL_003B;
					}
				}
				else
				{
					if (num5 == 11 || num5 == 16)
					{
						return 0.5f;
					}
					if (num5 != 21)
					{
						goto IL_003B;
					}
				}
				return 0.75f;
				IL_003B:
				return 0.125f;
			};
			aBtnMeter.FnMeterBindings fnMeterBindings = new aBtnMeter.FnMeterBindings(CFGSP.fnChangeConfigValue);
			FnDescConvert fnDescConvert = new FnDescConvert(Cfg.fnDescConvertRunningPersent);
			if (CFGSP.isSpEnabled("uipic_lr"))
			{
				Cfg.P("Config_spconfig_uipic_lr", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_uipic_lr",
					title = "spconfig_uipic_lr",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)CFGSP.uipic_lr,
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_left", "Config_right", "Config_hidden"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("use_uipic_press"))
			{
				Cfg.P("Config_spconfig_use_uipic_press_gimmick", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_gimmick",
					title = "spconfig_use_uipic_press_gimmick",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.use_uipic_press_gimmick ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, true);
				Cfg.P("Config_spconfig_use_uipic_press_enemy", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_enemy",
					title = "spconfig_use_uipic_press_enemy",
					skin_title = "",
					def = (float)CFGSP.use_uipic_press_enemy,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
				Cfg.P("Config_spconfig_use_uipic_press_balance", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_use_uipic_press_balance",
					title = "spconfig_use_uipic_press_balance",
					skin_title = "",
					def = (float)CFGSP.use_uipic_press_balance,
					w = Cfg.sliderw_sml,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(CFGSP.fnDescConvertPressBalance),
					fnBtnMeterLine = fnBtnMeterLine3
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("threshold_pregnant"))
			{
				Cfg.P("Config_spconfig_threshold_pregnant", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_threshold_pregnant",
					title = "spconfig_threshold_pregnant",
					skin_title = "",
					def = (float)CFGSP.threshold_pregnant,
					w = Cfg.sliderw_sml,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(CFGSP.fnDescConvertThresholdPregnant),
					fnBtnMeterLine = fnBtnMeterLine
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("dmgcounter_position"))
			{
				Cfg.P("Config_spconfig_dmgcounter_position", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgcounter_position",
					title = "spconfig_dmgcounter_position",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)CFGSP.dmgcounter_position,
					mn = 0f,
					mx = 3f,
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_dmgcounter_0", "Config_dmgcounter_1", "Config_dmgcounter_2", "Config_dmgcounter_3"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("dmg_multivoice"))
			{
				Cfg.P("Config_spconfig_dmg_multivoice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmg_multivoice",
					title = "spconfig_dmg_multivoice",
					skin_title = "",
					def = (float)CFGSP.dmg_multivoice,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("voice_for_pleasure"))
			{
				Cfg.P("Config_spconfig_voice_for_pleasure", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_voice_for_pleasure",
					title = "spconfig_voice_for_pleasure",
					skin_title = "",
					def = (float)X.IntR(CFGSP.voice_for_pleasure * 100f),
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert,
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
				Cfg.P("Config_spconfig_voice_for_pleasure2m", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_voice_for_pleasure2m",
					title = "spconfig_voice_for_pleasure2m",
					skin_title = "",
					def = (float)X.IntR(CFGSP.voice_for_pleasure2m * 100f),
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("epdmg_vo_mouth"))
			{
				Cfg.P("Config_spconfig_epdmg_vo_mouth", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_epdmg_vo_mouth",
					title = "spconfig_epdmg_vo_mouth",
					skin_title = "",
					def = (float)CFGSP.epdmg_vo_mouth,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine2,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("epdmg_vo_iku"))
			{
				Cfg.P("Config_spconfig_epdmg_vo_iku", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_epdmg_vo_iku",
					title = "spconfig_epdmg_vo_iku",
					skin_title = "",
					def = (float)CFGSP.epdmg_vo_iku,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("cloth_strength"))
			{
				Cfg.P("Config_spconfig_cloth_strength", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_cloth_strength",
					title = "spconfig_cloth_strength",
					skin_title = "",
					def = (float)CFGSP.cloth_strength,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnBtnMeterLine = fnBtnMeterLine2,
					fnDescConvert = new FnDescConvert(CFGSP.fnDescConvertRunningClothStrength)
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("cloth_broken_debuff"))
			{
				Cfg.P("Config_spconfig_cloth_broken_debuff", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_cloth_broken_debuff",
					title = "spconfig_cloth_broken_debuff",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.cloth_broken_debuff ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("milk"))
			{
				Cfg.P("Config_spconfig_milk", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_milk",
					title = "spconfig_milk",
					skin_title = "",
					def = (float)CFGSP.publish_milk,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
				Cfg.P("Config_spconfig_juice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_juice",
					title = "spconfig_juice",
					skin_title = "",
					def = (float)CFGSP.publish_juice,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("juice_cutin"))
			{
				Cfg.P("Config_spconfig_juice_cutin", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_juice_cutin",
					title = "spconfig_juice_cutin",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.juice_cutin ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Config_normal", "Config_juice_cutin_1"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("frozen_dmg_voice"))
			{
				Cfg.P("Config_spconfig_frozen_dmg_voice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_frozen_dmg_voice",
					title = "spconfig_frozen_dmg_voice",
					skin_title = "",
					def = (float)CFGSP.frozen_dmg_voice,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("stone_dmg_voice"))
			{
				Cfg.P("Config_spconfig_stone_dmg_voice", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_stone_dmg_voice",
					title = "spconfig_stone_dmg_voice",
					skin_title = "",
					def = (float)CFGSP.stone_dmg_voice,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("frozen_lock_orgasm"))
			{
				Cfg.P("Config_spconfig_frozen_lock_orgasm", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_frozen_lock_orgasm",
					title = "spconfig_frozen_lock_orgasm",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.frozen_lock_orgasm ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("stone_lock_orgasm"))
			{
				Cfg.P("Config_spconfig_stone_lock_orgasm", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_stone_lock_orgasm",
					title = "spconfig_stone_lock_orgasm",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.stone_lock_orgasm ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("deadburned"))
			{
				Cfg.P("Config_spconfig_deadburned", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_deadburned",
					title = "spconfig_deadburned",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.deadburned ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			int num = 11;
			CFGSP.SpEpBoostPrepare(false);
			for (int i = 0; i < num; i++)
			{
				EPCATEG epcateg = (EPCATEG)i;
				if (CFGSP.isSpEpBoostTarget(epcateg))
				{
					string text = FEnum<EPCATEG>.ToStr(epcateg).ToLower();
					byte b;
					if (CFGSP.isSpEnabled("ep_boost_" + text) && CFGSP.Oep_boost.TryGetValue(epcateg, out b))
					{
						UiBoxDesigner uiBoxDesigner = Cfg.P("Config_spconfig_ep_boost_" + text, true);
						DsnDataSlider dsnDataSlider = new DsnDataSlider();
						dsnDataSlider.name = "spconfig_ep_boost_" + text;
						dsnDataSlider.title = "spconfig_ep_boost_" + text;
						dsnDataSlider.skin_title = "";
						dsnDataSlider.def = (float)b;
						dsnDataSlider.w = Cfg.sliderw_middle;
						dsnDataSlider.mn = 0f;
						dsnDataSlider.mx = 250f;
						dsnDataSlider.valintv = 10f;
						dsnDataSlider.fnChanged = fnMeterBindings;
						dsnDataSlider.fnHover = FD_fnShowDesc;
						dsnDataSlider.fnDescConvert = delegate(STB Stb)
						{
							CFGSP.fnDescConvertRunningEpBoost(Stb);
						};
						dsnDataSlider.fnBtnMeterLine = fnBtnMeterLine3;
						uiBoxDesigner.addSliderCT(dsnDataSlider, 154f, null, false);
					}
				}
			}
			if (CFGSP.isSpEnabled("go_cheat"))
			{
				Cfg.P("Config_spconfig_go_cheat", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_go_cheat",
					title = "spconfig_go_cheat",
					skin_title = "",
					checkbox_mode = 1,
					def = (float)(CFGSP.go_cheat ? 1 : 0),
					w = Cfg.sliderw_sml,
					Adesc_keys = TX.GetArray("Disabled", "Enabled"),
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("gameover_counter"))
			{
				Cfg.P("Config_spconfig_gameover_counter", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_gameover_counter",
					title = "spconfig_gameover_counter",
					skin_title = "",
					def = (float)CFGSP.gameover_counter,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 21f,
					valintv = 1f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = new FnDescConvert(CFGSP.fnDescConvertGameoverCountdown),
					fnBtnMeterLine = fnBtnMeterLine4
				}, 214f, null, false);
			}
			if (CFGSP.isSpEnabled("opacity_marunomi"))
			{
				Cfg.P("Config_spconfig_opacity_marunomi", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_opacity_marunomi",
					title = "spconfig_opacity_marunomi",
					skin_title = "",
					def = (float)CFGSP.opacity_marunomi,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("dmgte_pixel_density"))
			{
				Cfg.P("Config_spconfig_dmgte_pixel_density", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_pixel_density",
					title = "spconfig_dmgte_pixel_density",
					skin_title = "",
					def = (float)CFGSP.dmgte_pixel_density,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 100f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert
				}, 114f, null, false);
				Cfg.P("Config_spconfig_dmgte_pixel_duration", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_pixel_duration",
					title = "spconfig_dmgte_pixel_duration",
					skin_title = "",
					def = (float)CFGSP.dmgte_pixel_duration,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert,
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
			}
			if (CFGSP.isSpEnabled("dmgte_ui_density"))
			{
				Cfg.P("Config_spconfig_dmgte_ui_density", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_ui_density",
					title = "spconfig_dmgte_ui_density",
					skin_title = "",
					def = (float)CFGSP.dmgte_ui_density,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert,
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
				Cfg.P("Config_spconfig_dmgte_ui_duration", true).addSliderCT(new DsnDataSlider
				{
					name = "spconfig_dmgte_ui_duration",
					title = "spconfig_dmgte_ui_duration",
					skin_title = "",
					def = (float)CFGSP.dmgte_ui_duration,
					w = Cfg.sliderw_middle,
					mn = 0f,
					mx = 200f,
					valintv = 10f,
					fnChanged = fnMeterBindings,
					fnHover = FD_fnShowDesc,
					fnDescConvert = fnDescConvert,
					fnBtnMeterLine = fnBtnMeterLine2
				}, 114f, null, false);
			}
			Container.Hr(0.6f, 18f, 26f, 1f);
			Container.addButton(new DsnDataButton
			{
				name = "ResetAllSP",
				title = "ResetAllSP",
				skin_title = TX.Get("Bench_Cmd_reset_all", ""),
				w = 246f,
				h = 32f,
				fnClick = delegate(aBtn B)
				{
					CFGSP.ResetVariable(Cfg, Container, Tab, FD_fnShowDesc);
					return true;
				},
				fnHover = FD_fnShowDesc,
				click_snd = "reset_var"
			});
		}

		private static bool fnChangeConfigValue(aBtnMeter _B, float pre_value, float cur_value)
		{
			return !_B.isLocked() && CFGSP.changeConfigValue(_B.title, cur_value);
		}

		public static bool changeConfigValue(string key, float cur_value)
		{
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 1788935810U)
				{
					if (num <= 474969761U)
					{
						if (num <= 224660360U)
						{
							if (num <= 92627239U)
							{
								if (num != 39734714U)
								{
									if (num == 92627239U)
									{
										if (key == "spconfig_deadburned")
										{
											CFGSP.deadburned = cur_value != 0f;
											return true;
										}
									}
								}
								else if (key == "spconfig_use_uipic_press_enemy")
								{
									CFGSP.use_uipic_press_enemy = (byte)cur_value;
									return true;
								}
							}
							else if (num != 98480248U)
							{
								if (num == 224660360U)
								{
									if (key == "spconfig_voice_for_pleasure2m")
									{
										CFGSP.voice_for_pleasure2m = cur_value / 100f;
										return true;
									}
								}
							}
							else if (key == "spconfig_ep_boost_mouth")
							{
								CFGSP.Oep_boost[EPCATEG.MOUTH] = (byte)cur_value;
								return true;
							}
						}
						else if (num <= 290938845U)
						{
							if (num != 256173129U)
							{
								if (num == 290938845U)
								{
									if (key == "spconfig_cloth_strength")
									{
										CFGSP.cloth_strength = (byte)cur_value;
										return true;
									}
								}
							}
							else if (key == "spconfig_dmg_multivoice")
							{
								CFGSP.dmg_multivoice = (byte)cur_value;
								return true;
							}
						}
						else if (num != 337898362U)
						{
							if (num != 375697930U)
							{
								if (num == 474969761U)
								{
									if (key == "spconfig_frozen_lock_orgasm")
									{
										CFGSP.frozen_lock_orgasm = cur_value != 0f;
										return true;
									}
								}
							}
							else if (key == "spconfig_use_uipic_press_balance")
							{
								CFGSP.use_uipic_press_balance = (byte)cur_value;
								return true;
							}
						}
						else if (key == "spconfig_milk")
						{
							CFGSP.publish_milk = (byte)cur_value;
							return true;
						}
					}
					else if (num <= 1280520366U)
					{
						if (num <= 1136986463U)
						{
							if (num != 624716156U)
							{
								if (num == 1136986463U)
								{
									if (key == "spconfig_ep_boost_anal")
									{
										CFGSP.Oep_boost[EPCATEG.ANAL] = (byte)cur_value;
										return true;
									}
								}
							}
							else if (key == "spconfig_stone_dmg_voice")
							{
								CFGSP.stone_dmg_voice = (byte)cur_value;
								return true;
							}
						}
						else if (num != 1175287288U)
						{
							if (num == 1280520366U)
							{
								if (key == "spconfig_gameover_counter")
								{
									CFGSP.gameover_counter = (byte)cur_value;
									return true;
								}
							}
						}
						else if (key == "spconfig_uipic_lr")
						{
							CFGSP.uipic_lr = (CFGSP.UIPIC_LR)cur_value;
							return true;
						}
					}
					else if (num <= 1469028275U)
					{
						if (num != 1318626345U)
						{
							if (num == 1469028275U)
							{
								if (key == "spconfig_go_cheat")
								{
									CFGSP.go_cheat = cur_value != 0f;
									return true;
								}
							}
						}
						else if (key == "spconfig_opacity_marunomi")
						{
							CFGSP.opacity_marunomi = (byte)cur_value;
							return true;
						}
					}
					else if (num != 1719909786U)
					{
						if (num != 1737481301U)
						{
							if (num == 1788935810U)
							{
								if (key == "spconfig_epdmg_vo_mouth")
								{
									CFGSP.epdmg_vo_mouth = (byte)cur_value;
									return true;
								}
							}
						}
						else if (key == "spconfig_dmgcounter_position")
						{
							CFGSP.dmgcounter_position = (CFGSP.UIPIC_DMGCNT)cur_value;
							return true;
						}
					}
					else if (key == "spconfig_ep_boost_gspot")
					{
						CFGSP.Oep_boost[EPCATEG.GSPOT] = (byte)cur_value;
						return true;
					}
				}
				else if (num <= 3093531377U)
				{
					if (num <= 2129104165U)
					{
						if (num <= 1902301588U)
						{
							if (num != 1789562456U)
							{
								if (num == 1902301588U)
								{
									if (key == "spconfig_cloth_broken_debuff")
									{
										CFGSP.cloth_broken_debuff = cur_value != 0f;
										return true;
									}
								}
							}
							else if (key == "spconfig_dmgte_ui_density")
							{
								CFGSP.dmgte_ui_density = (byte)cur_value;
								return true;
							}
						}
						else if (num != 2072892167U)
						{
							if (num == 2129104165U)
							{
								if (key == "spconfig_voice_for_pleasure")
								{
									CFGSP.voice_for_pleasure = cur_value / 100f;
									return true;
								}
							}
						}
						else if (key == "spconfig_ep_boost_ear")
						{
							CFGSP.Oep_boost[EPCATEG.EAR] = (byte)cur_value;
							return true;
						}
					}
					else if (num <= 2802935069U)
					{
						if (num != 2336147325U)
						{
							if (num == 2802935069U)
							{
								if (key == "spconfig_use_uipic_press_gimmick")
								{
									CFGSP.use_uipic_press_gimmick = cur_value != 0f;
									return true;
								}
							}
						}
						else if (key == "spconfig_ep_boost_cli")
						{
							CFGSP.Oep_boost[EPCATEG.CLI] = (byte)cur_value;
							return true;
						}
					}
					else if (num != 2859991500U)
					{
						if (num != 2963279150U)
						{
							if (num == 3093531377U)
							{
								if (key == "spconfig_ep_boost_vagina")
								{
									CFGSP.Oep_boost[EPCATEG.VAGINA] = (byte)cur_value;
									return true;
								}
							}
						}
						else if (key == "spconfig_epdmg_vo_iku")
						{
							CFGSP.epdmg_vo_iku = (byte)cur_value;
							return true;
						}
					}
					else if (key == "spconfig_dmgte_pixel_duration")
					{
						CFGSP.dmgte_pixel_duration = (byte)cur_value;
						return true;
					}
				}
				else if (num <= 3579481448U)
				{
					if (num <= 3180355272U)
					{
						if (num != 3152328284U)
						{
							if (num == 3180355272U)
							{
								if (key == "spconfig_ep_boost_urethra")
								{
									CFGSP.Oep_boost[EPCATEG.URETHRA] = (byte)cur_value;
									return true;
								}
							}
						}
						else if (key == "spconfig_ep_boost_canal")
						{
							CFGSP.Oep_boost[EPCATEG.CANAL] = (byte)cur_value;
							return true;
						}
					}
					else if (num != 3198812103U)
					{
						if (num != 3366677609U)
						{
							if (num == 3579481448U)
							{
								if (key == "spconfig_dmgte_ui_duration")
								{
									CFGSP.dmgte_ui_duration = (byte)cur_value;
									return true;
								}
							}
						}
						else if (key == "spconfig_frozen_dmg_voice")
						{
							CFGSP.frozen_dmg_voice = (byte)cur_value;
							return true;
						}
					}
					else if (key == "spconfig_juice_cutin")
					{
						CFGSP.juice_cutin = cur_value != 0f;
						return true;
					}
				}
				else if (num <= 3669376892U)
				{
					if (num != 3623002796U)
					{
						if (num == 3669376892U)
						{
							if (key == "spconfig_stone_lock_orgasm")
							{
								CFGSP.stone_lock_orgasm = cur_value != 0f;
								return true;
							}
						}
					}
					else if (key == "spconfig_dmgte_pixel_density")
					{
						CFGSP.dmgte_pixel_density = (byte)cur_value;
						return true;
					}
				}
				else if (num != 3921204200U)
				{
					if (num != 3956288208U)
					{
						if (num == 4171608391U)
						{
							if (key == "spconfig_juice")
							{
								CFGSP.publish_juice = (byte)cur_value;
								return true;
							}
						}
					}
					else if (key == "spconfig_ep_boost_breast")
					{
						CFGSP.Oep_boost[EPCATEG.BREAST] = (byte)cur_value;
						return true;
					}
				}
				else if (key == "spconfig_threshold_pregnant")
				{
					CFGSP.threshold_pregnant = (byte)cur_value;
					return true;
				}
			}
			X.de("不明なコンフィグ指定: " + key, null);
			return true;
		}

		public static bool getSpecialDesc(string title, out string desc)
		{
			desc = null;
			if (title != null)
			{
				if (title == "spconfig_dmgte_pixel_duration")
				{
					desc = TX.Get("Config_desc_spconfig_dmgte_pixel_density", "");
					return true;
				}
				if (title == "spconfig_dmgte_ui_duration")
				{
					desc = TX.Get("Config_desc_spconfig_dmgte_ui_density", "");
					return true;
				}
			}
			string text;
			if (TX.isStart(title, "spconfig_ep_boost_", out text, 0))
			{
				desc = TX.GetA("Config_desc_spconfig_ep_boost", TX.Get("EP_Targ_" + text, ""));
				return true;
			}
			return false;
		}

		private static void fnDescConvertRunningClothStrength(STB Stb)
		{
			if (Stb.Equals("0"))
			{
				Stb.SetTxA("Config_Super_fragile", false);
				return;
			}
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				if ((int)STB.NmRes(parseres, -1.0) == 200)
				{
					Stb.SetTxA("Config_Unbreakable", false);
					return;
				}
				Stb.Add("%");
			}
		}

		private static void fnDescConvertRunningEpBoost(STB Stb)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				int num = (int)STB.NmRes(parseres, -1.0);
				if (num > 200)
				{
					Stb.Clear();
					Stb.Add(X.IntR((float)((num - 200) * 100))).Add("%");
					return;
				}
			}
			Stb.Add("%");
		}

		private static void fnDescConvertGameoverCountdown(STB Stb)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				int num = (int)STB.NmRes(parseres, -1.0);
				if (num == 0)
				{
					Stb.Clear().AddTxA("Disabled", false);
					return;
				}
				Stb.Clear().Add(9 + num);
			}
		}

		private static void fnDescConvertPressBalance(STB Stb)
		{
			if (Stb.Equals("100"))
			{
				Stb.SetTxA("Config_press_balance_fare", false);
				return;
			}
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				int num = (int)STB.NmRes(parseres, -1.0) - 100;
				Stb.Clear();
				if (num < 0)
				{
					Stb.SetTxA("Config_press_balance_n", false).TxRpl(-num);
					return;
				}
				Stb.SetTxA("Config_press_balance_t", false).TxRpl(num);
			}
		}

		private static void fnDescConvertThresholdPregnant(STB Stb)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres) && (int)STB.NmRes(parseres, -1.0) >= 100)
			{
				Stb.SetTxA("Config_not_visible", false);
				return;
			}
			Stb.Add("%");
		}

		public static CFGSP.UIPIC_LR uipic_lr;

		public static byte dmg_multivoice;

		public static byte cloth_strength = 100;

		public static byte threshold_pregnant = 50;

		public static byte epdmg_vo_mouth = 100;

		public static byte epdmg_vo_iku = 0;

		public const int CLOTH_STR_MAX = 200;

		public static bool cloth_broken_debuff;

		public static byte publish_milk;

		public static byte publish_juice = 100;

		public static bool juice_cutin;

		public static bool deadburned;

		public static byte dmgte_pixel_density = 100;

		public static byte dmgte_pixel_duration = 100;

		public static byte dmgte_ui_density = 100;

		public static byte dmgte_ui_duration = 100;

		public static float voice_for_pleasure = 1f;

		public static float voice_for_pleasure2m = 0f;

		public static byte frozen_dmg_voice = 100;

		public static byte stone_dmg_voice = 0;

		public static bool frozen_lock_orgasm = false;

		public static bool stone_lock_orgasm = true;

		public static bool use_uipic_press_gimmick = false;

		public static byte use_uipic_press_enemy = 0;

		public static byte use_uipic_press_balance = 100;

		public static byte gameover_counter = 0;

		public static CFGSP.UIPIC_DMGCNT dmgcounter_position = CFGSP.UIPIC_DMGCNT.MAP;

		public static bool go_cheat = false;

		private static readonly BDic<EPCATEG, byte> Oep_boost = new BDic<EPCATEG, byte>(9);

		public static byte opacity_marunomi = 100;

		private static readonly List<string> Aenabled_value = new List<string>();

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
	}
}
