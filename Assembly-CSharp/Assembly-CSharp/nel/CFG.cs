using System;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public static class CFG
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

		public static void clearVariable()
		{
			CFG.vib_level = 100;
		}

		public static void clearSpVariable()
		{
			CFGSP.clearSpVariable();
			CFG.vib_level = 100;
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

		public static void loadSdFile(bool consider_fullscreen_mode = false, bool keycon_apply_pad_mode = false)
		{
			ByteReader byteReader = NKT.readSdBinary("config.cfg", true);
			if (X.DEBUGNOCFG)
			{
				return;
			}
			if (byteReader != null)
			{
				X.dl("オプションの読み込みを開始", null, false, false);
				CFG.readBinary(byteReader, true);
				if (byteReader.bytesAvailable > 0UL)
				{
					KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
					currentKeyAssignObject.readSaveString(byteReader, keycon_apply_pad_mode);
					currentKeyAssignObject.fineCurrentPadState();
				}
				if (consider_fullscreen_mode && CFG.fullscreen_mode != Screen.fullScreen)
				{
					IN.setFullScreenMode(CFG.fullscreen_mode, 0f);
				}
			}
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
				byteArray.writeByte((CFG.fullscreen_mode ? 1 : 0) | (X.ENG_MODE ? 2 : 0) | (X.SENSITIVE ? 4 : 0) | (X.SP_SENSITIVE ? 8 : 0));
				byteArray.writeByte((int)((byte)SND.volume));
				byteArray.writeByte((int)((byte)SND.voice_volume));
				byteArray.writeByte((int)((byte)SND.bgm_volume));
				byteArray.writeByte((int)CFG.autosave_timing);
				byteArray.writeByte((int)CFG.magsel_slow);
				byteArray.writeByte(X.AF);
				byteArray.writeByte(X.AF_EF);
				byteArray.writeByte(X.IntR(X.EF_LEVEL_NORMAL * 200f));
				byteArray.writeByte(X.IntR(X.EF_LEVEL_UI * 200f));
				byteArray.writeByte(X.IntR(X.EF_TIMESCALE_UI * 200f));
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
				CFGSP.writeToBytes(byteArray, 0);
				byteArray.writeByte((int)CFG.vib_level);
				CFGSP.writeToBytes(byteArray, 1);
				Ba0.writeExtractBytes(byteArray, 2, -1);
			}
			catch (Exception ex)
			{
				X.de(ex.ToString(), null);
			}
			return Ba0;
		}

		public static ByteReader readBinary(ByteReader Ba0, bool apply_language = true)
		{
			bool flag = X.ENG_MODE;
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
				using (ByteReader byteReader = Ba0.readExtractBytes(2))
				{
					CFG.first_startup = false;
					int num2 = byteReader.readByte();
					CFG.fullscreen_mode = (num2 & 1) != 0;
					flag = (num2 & 2) != 0;
					if ((num2 & 8) != 0)
					{
						X.sensitive_level = 2;
					}
					else if ((num2 & 4) != 0)
					{
						X.sensitive_level = 1;
					}
					else
					{
						X.sensitive_level = 0;
					}
					SND.volume = byteReader.readByte();
					SND.voice_volume = byteReader.readByte();
					SND.bgm_volume = byteReader.readByte();
					CFG.autosave_timing = (byte)byteReader.readByte();
					CFG.magsel_slow = (byte)byteReader.readByte();
					X.AF = X.MMX(1, byteReader.readByte(), 4);
					X.AF_EF = X.MMX(1, byteReader.readByte(), 4);
					X.EF_LEVEL_NORMAL = X.ZLINE((float)byteReader.readByte(), 200f);
					X.EF_LEVEL_UI = X.ZLINE((float)byteReader.readByte(), 200f);
					X.EF_TIMESCALE_UI = X.ZLINE((float)byteReader.readByte(), 200f);
					CFG.ui_effect_dirty = (byte)byteReader.readByte();
					text = byteReader.readString("utf-8", false);
					CFG.ui_sensitive_description = (byte)byteReader.readByte();
					CFG.posteffect_weaken = (byte)byteReader.readByte();
					M2SoundPlayer.monoral = byteReader.readBoolean();
					CFG.blood_weaken = (byte)byteReader.readByte();
					byte b = (byte)byteReader.readByte();
					if (num >= 13)
					{
						CFG.ui_effect_density = b;
					}
					M2MoverPr.double_tap_running = byteReader.readBoolean();
					M2MoverPr.running_thresh = X.MMX(0f, (float)byteReader.readByte() / 100f, 1f);
					M2MoverPr.jump_press_reverse = byteReader.readBoolean();
					CFG.shield_hold_evadable = byteReader.readBoolean();
					CFG.fatal_automode_speed = (byte)byteReader.readByte();
					CFG.item_usel_type = (CFG.USEL_TYPE)byteReader.readByte();
					CFG.go_text_pos_top = byteReader.readBoolean();
					CFG.magsel_decide_type = (CFG.MAGSEL_TYPE)byteReader.readByte();
					CFGSP.readBinaryFrom(byteReader, 0);
					CFG.vib_level_ = byteReader.readUByte();
					CFGSP.readBinaryFrom(byteReader, 1);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message != "ERROR_EOF")
				{
					num = -1;
					X.de(ex.ToString(), null);
				}
			}
			if (NEL.Instance != null && NEL.Instance.Vib != null)
			{
				NEL.Instance.Vib.base_level = (float)CFG.vib_level_ * 0.01f;
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
				X.AF_EF = X.Mx(X.AF_EF, X.AF);
			}
			return Ba0;
		}

		public static byte vib_level
		{
			get
			{
				return CFG.vib_level_;
			}
			set
			{
				if (CFG.vib_level_ == value)
				{
					return;
				}
				CFG.vib_level_ = value;
				if (NEL.Instance != null && NEL.Instance.Vib != null)
				{
					NEL.Instance.Vib.base_level = (float)CFG.vib_level_ * 0.01f;
				}
			}
		}

		public const int DEFAULT_AUTOSAVE_TIMING = 3;

		public const int DEFAULT_MAGSEL_SLOW = 4;

		public const int DEFAULT_UI_EFFECT_DIRTY = 5;

		public const int DEFAULT_UI_SENSITIVE_DESCRIPTION = 5;

		public const int DEFAULT_UI_EFFECT_DENSITY = 10;

		public const string sd_file_name = "config.cfg";

		internal static bool fullscreen_mode = false;

		public static bool first_startup = true;

		internal static byte autosave_timing = 3;

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

		private static byte vib_level_ = 100;

		public static CFG.MAGSEL_TYPE magsel_decide_type = CFG.MAGSEL_TYPE.NORMAL;

		public static byte FATAL_SPEED_MAX = 18;

		public const byte POSTEFFECT_WEAKEN_MAX = 10;

		public const int CFG_VER = 13;

		public const int CFG_VER_FIRST = 10;

		internal static int temp_kisekae;

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
	}
}
