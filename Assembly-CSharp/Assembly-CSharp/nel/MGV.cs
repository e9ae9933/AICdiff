using System;
using System.Collections.Generic;
using System.IO;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public static class MGV
	{
		private static void clear()
		{
			MGV.Afatal_seen = new List<string>(1);
		}

		public static ByteArray loadSdFile()
		{
			MGV.clear();
			if (MGV.temp_kisekae_max < 0)
			{
				int num = 99;
				int num2 = 1;
				while (num2 <= num && File.Exists(MGV.kisekaeBinaryPath(num2)))
				{
					num2++;
				}
				MGV.temp_kisekae_max = num2 - 1;
			}
			ByteArray byteArray = NKT.readSdBinary("whole.data", true);
			if (byteArray != null)
			{
				MGV.readBinary(byteArray, true);
			}
			return byteArray;
		}

		public static string kisekaeBinaryPath(int c)
		{
			return Path.Combine(Application.streamingAssetsPath, "noel_kisekae" + global::XX.X.spr0(c, 2, '0') + ".pxls.bytes");
		}

		public static ByteArray saveSdFile(ByteArray Ba = null)
		{
			if (Ba == null)
			{
				Ba = MGV.createBinary(null);
			}
			NKT.writeSdBinary("whole.data", Ba, false);
			return Ba;
		}

		public static ByteArray createBinary(ByteArray Ba = null)
		{
			if (MGV.Afatal_seen == null)
			{
				MGV.clear();
			}
			if (Ba == null)
			{
				Ba = new ByteArray(0U);
			}
			if (IN.isPadMode())
			{
				MGV.pad_checked = true;
			}
			try
			{
				Ba.writeUInt(2455153648U);
				Ba.writeByte(14);
				Ba.writeByte((int)MGV.last_saved);
				Ba.writeByte(MGV.temp_kisekae);
				Ba.writeByte(MGV.Afatal_seen.Count);
				int count = MGV.Afatal_seen.Count;
				for (int i = 0; i < count; i++)
				{
					Ba.writePascalString(MGV.Afatal_seen[i], "utf-8");
				}
				Ba.writeBool(global::XX.X.v_sync);
				Ba.writeBool(MGV.pad_checked);
				CFG.writeBinarySp(Ba);
			}
			catch (Exception ex)
			{
				global::XX.X.de(ex.ToString(), null);
			}
			return Ba;
		}

		private static ByteArray readBinary(ByteArray Ba, bool apply_language = true)
		{
			try
			{
				if (Ba.readUInt() != 2455153648U)
				{
					throw new Exception("Mgv read: Header error");
				}
				int num = Ba.readByte();
				if (num < 13)
				{
					MGV.pad_checked = true;
				}
				MGV.last_saved = (byte)Ba.readByte();
				MGV.temp_kisekae = global::XX.X.MMX(0, Ba.readByte(), MGV.temp_kisekae_max);
				if (num >= 11)
				{
					int num2 = Ba.readByte();
					for (int i = 0; i < num2; i++)
					{
						MGV.Afatal_seen.Add(Ba.readPascalString("utf-8", false));
					}
					if (num >= 12)
					{
						global::XX.X.v_sync = Ba.readBoolean();
						if (num >= 13)
						{
							MGV.pad_checked = Ba.readBoolean();
						}
					}
					if (num >= 14)
					{
						CFG.readBinarySp(Ba);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.Message != "ERROR_EOF")
				{
					global::XX.X.de(ex.ToString(), null);
				}
			}
			if (!MGV.read_first)
			{
				MGV.read_first = true;
				IN.enable_vsync = global::XX.X.v_sync;
			}
			return Ba;
		}

		public static void fatalSceneWatch(string key)
		{
			if (MGV.Afatal_seen == null)
			{
				MGV.clear();
			}
			if (key != null && MGV.Afatal_seen.IndexOf(key) == -1)
			{
				MGV.Afatal_seen.Add(key);
			}
		}

		public static bool isFatalSceneAlreadyWatched(string key)
		{
			if (MGV.Afatal_seen == null)
			{
				MGV.clear();
			}
			return MGV.Afatal_seen.IndexOf(key) >= 0;
		}

		public static byte last_saved;

		public const int MGV_VER = 14;

		public const int MGV_VER_FIRST = 10;

		public const string mgv_file_name = "whole.data";

		public static int temp_kisekae = 0;

		public static int temp_kisekae_max = -1;

		public static int pixel_scale = 1;

		public static bool pad_checked;

		private static bool read_first;

		private static List<string> Afatal_seen;
	}
}
