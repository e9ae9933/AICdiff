using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public static class SVD
	{
		public static string getDir()
		{
			return Application.persistentDataPath;
		}

		public static string getFileName(int index)
		{
			return "savedata_" + X.spr0(index, 2, '0') + ".aicsave";
		}

		public static int thumb_w
		{
			get
			{
				return (int)((IN.w - 240f) * 0.3333333f);
			}
		}

		public static int thumb_h
		{
			get
			{
				return (int)((IN.h - 180f) * 0.3333333f);
			}
		}

		public static List<SVD.sFile> prepareList(bool force = true)
		{
			if (!force && SVD.AFiles != null)
			{
				return SVD.AFiles;
			}
			SVD.exists_file_count = 0;
			SVD.AFiles = new List<SVD.sFile>();
			SVD.APreparingList = null;
			SVD.loaded_last_total_frame = 0;
			string[] array = Directory.GetFiles(SVD.getDir(), "*.aicsave", SearchOption.TopDirectoryOnly);
			int num = array.Length;
			if (num == 0)
			{
				if (!false)
				{
					return SVD.AFiles;
				}
				array = Directory.GetFiles(SVD.getDir(), "*.aicsave", SearchOption.TopDirectoryOnly);
				num = array.Length;
			}
			for (int i = 0; i < num; i++)
			{
				if (REG.match(array[i], SVD.RegSdFile))
				{
					int num2 = X.NmI(REG.R1, 0, false, false);
					if (X.BTWW(0f, (float)num2, 99f))
					{
						int num3 = X.Mn(num2 + 7, 99);
						if (SVD.AFiles.Capacity <= num3)
						{
							SVD.AFiles.Capacity = num3 + 1;
						}
						while (SVD.AFiles.Count <= num2)
						{
							SVD.AFiles.Add(null);
						}
						SVD.AFiles[num2] = new SVD.sFile(num2, false);
						SVD.exists_file_count++;
					}
				}
			}
			return SVD.AFiles;
		}

		public static bool isLastFocusedRow(SVD.sFile Sf)
		{
			return UiSVD.last_focused == Sf.index;
		}

		public static bool initPreparingFileHeader(SVD.sFile Sf, bool force = false)
		{
			byte[] array = new byte[24];
			return SVD.initPreparingFileHeader(Sf, force, ref array);
		}

		public static bool initPreparingFileHeader(SVD.sFile Sf, bool force, ref byte[] Abuffer)
		{
			if (!force && IN.totalframe - SVD.loaded_last_total_frame <= 6)
			{
				return false;
			}
			if (UiSVD.ui_active && !force && UiSVD.last_focused != Sf.index)
			{
				SVD.sFile file = SVD.GetFile(UiSVD.last_focused, true);
				if (file != null && file.loadstate == SVD.sFile.STATE.NO_LOAD)
				{
					return false;
				}
			}
			SVD.loaded_last_total_frame = IN.totalframe;
			bool loadstate = Sf.loadstate != SVD.sFile.STATE.NO_LOAD;
			bool flag = COOK.readFileHeader(Sf, Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index)), ref Abuffer);
			if (!loadstate)
			{
				Sf.loadstate = (flag ? SVD.sFile.STATE.LOADED : SVD.sFile.STATE.ERROR);
				if (flag && UiSVD.ui_active && Sf.index == UiSVD.last_focused)
				{
					UiSVD.fineDescStringS();
				}
			}
			return true;
		}

		public static SVD.sFile GetFile(int index = 0, bool no_make = true)
		{
			if (index >= SVD.AFiles.Count)
			{
				return null;
			}
			if (SVD.AFiles[index] == null && !no_make)
			{
				SVD.sFile sFile = (SVD.AFiles[index] = new SVD.sFile(index, true));
				SVD.exists_file_count++;
				return sFile;
			}
			return SVD.AFiles[index];
		}

		public static void releaseAllThumbs()
		{
			if (SVD.AFiles == null)
			{
				return;
			}
			int count = SVD.AFiles.Count;
			for (int i = 0; i < count; i++)
			{
				SVD.sFile sFile = SVD.AFiles[i];
				if (sFile != null)
				{
					sFile.releaseThumb();
				}
			}
		}

		public static ByteArray loadFileContent(SVD.sFile Sf)
		{
			return new ByteArray(NKT.readSpecificFileBinary(Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index)), 0, 0, false), false, false);
		}

		public static SVD.sFile CopyFile(SVD.sFile Sf, int dest)
		{
			return SVD.createFile(dest, false).CopyFrom(Sf, dest);
		}

		public static SVD.sFile createFile(int dest, bool no_destruct = false)
		{
			int num = X.Mn(dest + 7, 99);
			if (SVD.AFiles.Capacity <= num)
			{
				SVD.AFiles.Capacity = num + 1;
			}
			while (SVD.AFiles.Count <= dest)
			{
				SVD.AFiles.Add(null);
			}
			if (SVD.AFiles[dest] != null)
			{
				if (!no_destruct)
				{
					SVD.AFiles[dest].destruct();
				}
			}
			else
			{
				SVD.exists_file_count++;
			}
			return SVD.AFiles[dest] = new SVD.sFile(dest, true);
		}

		public static void replaceSaveFile(int index, SVD.sFile OvrFile)
		{
			SVD.sFile sFile = SVD.AFiles[index];
			if (sFile == OvrFile)
			{
				return;
			}
			if (sFile != null)
			{
				sFile.destruct();
			}
			SVD.AFiles[index] = OvrFile;
			if (OvrFile != null)
			{
				OvrFile.index = index;
			}
		}

		public static string saveBinary(SVD.sFile Sf, ByteArray Ba)
		{
			string text = null;
			try
			{
				string text2 = "temporary_" + SVD.getFileName(Sf.index);
				string text3 = Path.Combine(Application.persistentDataPath, SVD.getFileName(Sf.index));
				text = NKT.writeSdBinary(text2, Ba, true);
				if (TX.valid(text))
				{
					text = TX.getLine(text, 0);
					return text;
				}
				try
				{
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
				}
				catch
				{
				}
				File.Move(Path.Combine(Application.persistentDataPath, text2), text3);
				UiSVD.checkSavingBinaryS(Sf);
				CFG.saveSdFile(null);
				MGV.last_saved = (byte)Sf.index;
				MGV.saveSdFile(null);
			}
			catch (Exception ex)
			{
				string line = TX.getLine(ex.Message, 0);
				X.de("SAVE ERROR:" + ex.Message, null);
				text = ((text == null) ? line : (text + "\n" + line));
			}
			return text;
		}

		public static ByteArray changeOnlyMemo(SVD.sFile Sf)
		{
			if (Sf.thumb_position < 0)
			{
				X.de("初期化されていません", null);
				return null;
			}
			string text = Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index));
			string text2 = Path.Combine(SVD.getDir(), ".temp.aicsave");
			ByteArray byteArray = null;
			try
			{
				File.Copy(text, text2, true);
				ByteArray byteArray2 = new ByteArray(NKT.readSpecificFileBinary(text2, (int)Sf.thumb_position, 0, false), false, false);
				ByteArray byteArray3 = new ByteArray(0U);
				COOK.writeHeader(byteArray3, Sf);
				byteArray3.writeBytes(byteArray2, 0, -1);
				NKT.writeSdBinary(SVD.getFileName(Sf.index), byteArray3, false);
				byteArray = byteArray3;
			}
			catch (Exception ex)
			{
				X.de(ex.ToString(), null);
			}
			try
			{
				File.Delete(text2);
			}
			catch
			{
			}
			return byteArray;
		}

		private static bool prepareCompanyMove()
		{
			string persistentDataPath = Application.persistentDataPath;
			bool flag = false;
			try
			{
				int i = 0;
				while (i < 3)
				{
					string text = ((i == 0) ? "jp.NanameHacha.AliceInCradle" : ((i == 1) ? "jp.nonamehacha.AliceInCradle" : "jp.nanamehacha.AliceInCradle"));
					string text2 = Path.Combine(Path.GetDirectoryName(persistentDataPath), text);
					if (Directory.Exists(text2))
					{
						goto IL_0060;
					}
					text2 = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(persistentDataPath)), text);
					if (Directory.Exists(text2))
					{
						goto IL_0060;
					}
					IL_0123:
					i++;
					continue;
					IL_0060:
					string text3 = Path.Combine(text2, "AliceInCradle");
					if (Directory.Exists(text3))
					{
						text2 = text3;
					}
					if (!(text2 == persistentDataPath))
					{
						try
						{
							string[] files = Directory.GetFiles(text2, "*", SearchOption.TopDirectoryOnly);
							int num = files.Length;
							for (int j = 0; j < num; j++)
							{
								try
								{
									string text4 = files[j];
									string fileName = Path.GetFileName(text4);
									if (!TX.isStart(fileName, ".", 0))
									{
										if (fileName == "config.cfg" || fileName == "whole.data" || REG.match(fileName, SVD.RegSdFile))
										{
											string text5 = Path.Combine(persistentDataPath, fileName);
											if (!File.Exists(text5))
											{
												File.Copy(text4, text5);
												flag = true;
											}
										}
									}
								}
								catch
								{
								}
							}
						}
						catch
						{
						}
						goto IL_0123;
					}
					goto IL_0123;
				}
			}
			catch
			{
			}
			return flag;
		}

		public static void revealLocalDirectory()
		{
			NKT.openInExplorer(SVD.getDir() + "/");
		}

		private static List<SVD.sFile> AFiles;

		public const int AUTOSAVE_ID = 0;

		public const string fn_header = "savedata_";

		public const string fn_ext = ".aicsave";

		private static Regex RegSdFile = new Regex("savedata_(\\d\\d)\\.aicsave$");

		public const int file_max = 99;

		private const int list_margin_max = 7;

		public const int HEADER_BUF_LEN = 24;

		public static int exists_file_count;

		private static List<SVD.sFile> APreparingList;

		private static int loaded_last_total_frame = 0;

		public const float thumb_scale = 0.3333333f;

		public sealed class sFile
		{
			public sFile(int _index, bool created = false)
			{
				this.index = _index;
				this.loadstate = (created ? SVD.sFile.STATE.CREATED : SVD.sFile.STATE.NO_LOAD);
				this.Achive = new ACHIVE();
			}

			public SVD.sFile CopyFrom(SVD.sFile Src, int _index)
			{
				this.index = _index;
				this.first_version = Src.first_version;
				this.loadstate = SVD.sFile.STATE.CREATED;
				this.playtime = Src.playtime;
				this.memo = Src.memo;
				this.safe_area_memory = Src.safe_area_memory;
				this.modified = Src.modified;
				this.playstart = Src.playstart;
				this.thumb_position = Src.thumb_position;
				this.explore_timer = Src.explore_timer;
				return this;
			}

			public SVD.sFile saveInit()
			{
				this.releaseThumb();
				this.version = 36;
				this.loadstate = SVD.sFile.STATE.CREATED;
				int num = (int)((this.modified = DateTime.Now) - this.playstart).TotalSeconds;
				if (num > 0)
				{
					this.playtime += (uint)num;
				}
				this.revert_pos = false;
				this.playInit();
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				COOK.addTimer(nelM2DBase, false);
				nelM2DBase.WDR.walkAround(false);
				Map2d curMap = nelM2DBase.curMap;
				PRNoel prNoel = nelM2DBase.getPrNoel();
				prNoel.saveInit();
				if (nelM2DBase.IMNG != null)
				{
					nelM2DBase.IMNG.digestDiscardStack(nelM2DBase.curMap.Pr);
				}
				this.hp_noel = (ushort)prNoel.get_hp();
				this.maxhp_noel = (ushort)prNoel.get_maxhp();
				this.mp_noel = (ushort)prNoel.get_mp();
				this.maxmp_noel = (ushort)prNoel.get_maxmp();
				string pvv = GF.getPVV();
				this.phase = (ushort)(TX.valid(pvv) ? X.NmI(pvv, 0, false, false) : 0);
				WholeMapItem wholeFor = nelM2DBase.WM.GetWholeFor(curMap, false);
				this.whole_map_key = ((wholeFor != null) ? wholeFor.text_key : "???");
				this.explore_timer = (uint)COOK.calced_timer;
				this.Achive.CopyFrom(COOK.CurAchive);
				return this;
			}

			public void revertPos()
			{
				if (!this.revert_pos)
				{
					return;
				}
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				PRNoel prNoel = nelM2DBase.getPrNoel();
				if (this.last_saved_x > 0f && nelM2DBase.curMap != null && nelM2DBase.curMap.key == this.last_map_key)
				{
					Vector3 vector = prNoel.setToLoadGame(this.last_saved_x + 0.5f, this.last_saved_y - prNoel.sizey);
					if (vector.z > 0f)
					{
						this.last_saved_x = vector.x;
						this.last_saved_y = vector.y;
					}
				}
				this.revert_pos = false;
			}

			public void playInit()
			{
				this.first_load = false;
				this.playstart = DateTime.Now;
				this.assignRevertPosition(M2DBase.Instance as NelM2DBase);
				if (this.first_version == 0)
				{
					this.first_version = this.version;
				}
			}

			public void assignRevertPosition(NelM2DBase M2D)
			{
				if (M2D != null && M2D.curMap != null)
				{
					this.last_map_key = M2D.curMap.key;
					PRNoel prNoel = M2D.getPrNoel();
					this.last_saved_x = prNoel.x;
					this.last_saved_y = M2D.curMap.getFootableY((float)((int)prNoel.x), (int)prNoel.y, 12, true, -1f, false, true, true, prNoel.sizex);
				}
			}

			public void destruct()
			{
				this.releaseThumb();
			}

			public void setThumb(Texture2D Tx)
			{
				if (Tx == this.Thumbnail)
				{
					return;
				}
				this.releaseThumb();
				this.Thumbnail = Tx;
				UiSVD.descRedraw();
			}

			public void releaseThumb()
			{
				this.thumb_error = false;
				if (this.Thumbnail != null)
				{
					global::UnityEngine.Object.Destroy(this.Thumbnail);
				}
			}

			public Texture2D createThumbnail(NelM2DBase M2D)
			{
				this.releaseThumb();
				float num = 0.3333333f;
				RenderTexture finalizedTexture = M2D.Cam.getFinalizedTexture();
				int thumb_w = SVD.thumb_w;
				this.Thumbnail = BLIT.getSnapShot(finalizedTexture, (float)0 / IN.w * 0.86f, 0f, (float)SVD.thumb_w, (float)SVD.thumb_h, num, true);
				return this.Thumbnail;
			}

			public bool header_prepared
			{
				get
				{
					return this.loadstate == SVD.sFile.STATE.LOADED || this.loadstate == SVD.sFile.STATE.CREATED;
				}
			}

			public string getDescStringForUi()
			{
				if (this.index != 0)
				{
					return TX.GetA("SaveDataDesc", X.spr0(this.index, 2, '0'), this.strPlayTime);
				}
				return TX.GetA("SaveDataDesc_auto", this.strPlayTime);
			}

			public string strPlayTime
			{
				get
				{
					uint num = this.playtime / 3600U;
					uint num2 = this.playtime / 60U % 60U;
					uint num3 = this.playtime % 60U;
					return ((num > 0U) ? (num.ToString() + ":") : "") + X.spr0((int)num2, 2, '0') + ":" + X.spr0((int)num3, 2, '0');
				}
			}

			public int index;

			public SVD.sFile.STATE loadstate;

			public bool has_alice;

			public bool thumb_error;

			public byte version = 36;

			public byte first_version;

			public uint playtime;

			public ushort phase;

			public ushort hp_noel = 150;

			public ushort maxhp_noel = 150;

			public ushort mp_noel = 200;

			public ushort maxmp_noel = 200;

			public string whole_map_key = "???";

			public string memo = "";

			public DateTime modified = X.TimeEpoch;

			public Texture2D Thumbnail;

			public readonly ACHIVE Achive;

			public DateTime playstart;

			public float last_saved_x = -1f;

			public float last_saved_y = -1f;

			public string last_map_key;

			public short thumb_position = -1;

			public string safe_area_memory = "";

			public uint explore_timer;

			public bool revert_pos;

			public bool first_load;

			public enum STATE
			{
				NO_LOAD,
				LOADED,
				ERROR,
				CREATED
			}
		}
	}
}
