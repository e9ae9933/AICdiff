using System;
using System.Collections.Generic;
using System.IO;
using Better;
using evt;
using m2d;
using nel.mgm;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public static class COOK
	{
		public static void clear(bool save_diff = false)
		{
			COOK.CurFile = null;
			COOK.FlgTimerStop = new Flagger(delegate(FlaggerT<string> V)
			{
				if (M2DBase.Instance != null)
				{
					COOK.addTimer(M2DBase.Instance as NelM2DBase, true);
				}
			}, delegate(FlaggerT<string> V)
			{
				if (M2DBase.Instance != null)
				{
					COOK.resetCacheTimer(M2DBase.Instance as NelM2DBase);
				}
			});
			COOK.do_not_load_sf_cfg = false;
			COOK.loaded_index = -1;
			COOK.newGame(null, save_diff);
		}

		public static int current_file_index()
		{
			if (COOK.CurFile == null)
			{
				return 0;
			}
			return COOK.CurFile.index;
		}

		public static void newGame(NelM2DBase M2D = null, bool save_diff = false)
		{
			COOK.first_map_key = "forest_secret_lake";
			if (COOK.Osf == null)
			{
				COOK.Osf = new BDic<string, byte>();
			}
			else
			{
				COOK.Osf.Clear();
			}
			GF.clear();
			COOK.map_walk_count = 0;
			CFGSP.newGameSp();
			NEL.createTextLog().Clear();
			COOK.FlgTimerStop.Clear();
			NelItem.clearCacheItem();
			RCP.newGame();
			SkillManager.newGame();
			CoinStorage.Clear();
			StoreManager.newGame();
			UiEnemyDex.newGame();
			COOK.Mgm.Clear();
			EV.newGame();
			COOK.CurAchive.newGame();
			COOK.calced_floort = (COOK.calced_floort = 0f);
			UIPicture.tecon_TS = 1f;
			SCN.newGame();
			COOK.ODropItemMem.Clear();
			if (!save_diff)
			{
				DIFF.I = 1;
			}
			if (M2D != null)
			{
				M2D.newGame();
			}
			UiAlchemyRecipeBook.NextRevealAtAwake = null;
		}

		public static void NewGameFirstAssign(NelM2DBase M2D)
		{
			if (M2D != null)
			{
				M2D.WA.NewGameFirstAssign();
			}
		}

		public static void dlLang()
		{
			TX.TXFamily currentFamily = TX.getCurrentFamily();
			X.dl("Current TX Family: " + ((currentFamily == null) ? "(NULL)" : currentFamily.key), null, false, false);
		}

		public static void newGameItems(ItemStorage StInventory, ItemStorage StHouse, ItemStorage StPrecious)
		{
			StInventory.Add(NelItem.Bottle.addObtainCount(1), 1, 0, true, true);
			StInventory.Add(NelItem.GetById("mtr_lily_bulb0", false).addObtainCount(1), 7, 1, true, true);
			StInventory.Add(NelItem.GetById("mtr_black_harb0", false).addObtainCount(1), 3, 2, true, true);
			StInventory.hide_bottle_max = 0;
			StHouse.Add(NelItem.Bottle, 1, 0, true, true);
			StPrecious.Add(NelItem.GetById("precious_dangerous_meter", false).addObtainCount(1), 1, 0, true, true);
		}

		public static void setSF(string key, int b)
		{
			COOK.Osf[key] = (byte)X.MMX(0, b, 255);
		}

		public static void setSFcommandEval(string key, string cmd)
		{
			int sf = COOK.getSF(key);
			COOK.setSF(key, TX.commandEvalSet(cmd, sf));
		}

		public static int getSF(string key)
		{
			return (int)X.GetS<string, byte>(COOK.Osf, key, 0);
		}

		public static void setAchiveCommandEval(string key, string cmd)
		{
			int num = (int)COOK.CurAchive.Get(key);
			COOK.CurAchive.Set(key, TX.commandEvalSet(cmd, num));
		}

		public static int getAchive(string key)
		{
			return (int)COOK.CurAchive.Get(key);
		}

		public static int getAchive(ACHIVE.MENT key)
		{
			return (int)COOK.CurAchive.Get(key);
		}

		public static void clearSFbyHeader(string header)
		{
			using (BList<string> blist = X.objKeysB<string, byte>(COOK.Osf))
			{
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					string text = blist[i];
					if (TX.isStart(text, header, 0))
					{
						COOK.Osf.Remove(text);
					}
				}
			}
		}

		public static void addTimer(NelM2DBase M2D, bool ignore_flg = false)
		{
			if (M2D.curMap == null || M2D.WM.CurWM == null || M2D.WM.CurWM.safe_area)
			{
				return;
			}
			if (!ignore_flg && COOK.FlgTimerStop.isActive())
			{
				return;
			}
			COOK.calced_timer += X.Mx(0f, M2D.curMap.floort - COOK.calced_floort);
			COOK.calced_floort = M2D.curMap.floort;
		}

		public static void resetCacheTimer(NelM2DBase M2D)
		{
			if (M2D.curMap == null || M2D.WM.CurWM == null)
			{
				return;
			}
			COOK.calced_floort = M2D.curMap.floort;
		}

		public static void setLoadTarget(SVD.sFile Target, bool _do_not_load_sf_cfg)
		{
			COOK.CurFile = Target;
			COOK.loaded_index = Target.index;
			COOK.do_not_load_sf_cfg = _do_not_load_sf_cfg;
		}

		public static bool initGameScene(NelM2DBase M2D)
		{
			bool flag = false;
			COOK.save_failure_announce = (COOK.load_failure_announce = "");
			COOK.error_loaded_index = -1;
			if (COOK.CurFile != null)
			{
				if (!COOK.CurFile.header_prepared)
				{
					SVD.initPreparingFileHeader(COOK.CurFile, true);
				}
				flag = COOK.readBinaryContent(SVD.loadFileContent(COOK.CurFile), COOK.CurFile, M2D);
			}
			if (!flag)
			{
				if (COOK.CurFile != null)
				{
					COOK.error_loaded_index = (short)COOK.loaded_index;
				}
				COOK.loaded_index = 0;
				COOK.CurFile = new SVD.sFile(0, true);
				COOK.CurFile.version = 0;
				COOK.CurFile.first_version = 36;
				COOK.newGame(M2D, true);
				COOK.NewGameFirstAssign(M2D);
			}
			SCN.fine_pvv(true);
			CFG.refineAllLanguageCache(false);
			M2D.Marker.newGameAfter(COOK.CurFile.version > 0 && COOK.CurFile.version < 20);
			M2D.GM.newGameLoadAfter();
			COOK.CurFile.first_load = true;
			return flag;
		}

		public static SVD.sFile getCurrentFile()
		{
			if (COOK.CurFile == null)
			{
				COOK.CurFile = new SVD.sFile(0, true);
			}
			return COOK.CurFile;
		}

		public static bool canSave()
		{
			if (M2DBase.Instance == null || M2DBase.Instance.curMap == null || M2DBase.Instance.curMap.Pr == null)
			{
				return false;
			}
			PR pr = M2DBase.Instance.curMap.Pr as PR;
			return !(pr == null) && pr.is_alive;
		}

		public static UILogRow autoSave(NelM2DBase M2D, bool is_bench = false, bool force = false)
		{
			if (!force && !COOK.canSave())
			{
				return null;
			}
			if (COOK.CurFile != SVD.GetFile(0, true))
			{
				COOK.CurFile = SVD.CopyFile(COOK.CurFile, 0);
			}
			SVD.sFile curFile = COOK.CurFile;
			COOK.CurFile = SVD.createFile(0, true).CopyFrom(curFile, 0);
			COOK.do_not_load_sf_cfg = true;
			X.dli(">>AutoSave", null);
			string text = SVD.saveBinary(COOK.CurFile, COOK.createBinary(null, COOK.CurFile, M2D, true, true));
			if (text != null)
			{
				COOK.save_failure_announce = TX.add(COOK.save_failure_announce, text, "\n");
			}
			if (COOK.save_failure_announce != "")
			{
				SVD.replaceSaveFile(0, curFile);
				COOK.CurFile = curFile;
				return UILog.Instance.AddAlertTX("SVD_Alert_save_failure", UILogRow.TYPE.ALERT_FATAL);
			}
			if (curFile != null)
			{
				curFile.destruct();
			}
			return UILog.Instance.AddAlertTX("Alert_autosave", is_bench ? UILogRow.TYPE.ALERT_BENCH : UILogRow.TYPE.ALERT);
		}

		public static void createNewSave(SVD.sFile Sf, NelM2DBase M2D, bool save_cfg = true)
		{
			if (!COOK.canSave())
			{
				return;
			}
			if (COOK.CurFile != null)
			{
				Sf.CopyFrom(COOK.CurFile, Sf.index);
			}
			COOK.CurFile = Sf;
			COOK.loaded_index = COOK.CurFile.index;
			COOK.do_not_load_sf_cfg = true;
			X.dli(">>Saved SVD To: " + Sf.index.ToString(), null);
			string text = SVD.saveBinary(COOK.CurFile, COOK.createBinary(null, COOK.CurFile, M2D, true, save_cfg));
			if (text != null)
			{
				COOK.save_failure_announce = TX.add(COOK.save_failure_announce, text, "\n");
			}
		}

		public static void replaceCurFileTo(SVD.sFile Sf)
		{
			COOK.CurFile = Sf;
		}

		public static bool readFileHeader(SVD.sFile Sf, string path, ref byte[] Abuffer)
		{
			try
			{
				using (ByteReaderFS byteReaderFS = NKT.PopSpecificFileStream(path, 0, 0, false, 24, Abuffer))
				{
					bool flag = COOK.readBinaryHeader(byteReaderFS, Sf, true);
					Abuffer = byteReaderFS.getBufferRawArray();
					return flag;
				}
			}
			catch (Exception ex)
			{
				COOK.load_failure_announce = ex.Message;
				Sf.loadstate = SVD.sFile.STATE.ERROR;
				X.de("Loading header error: " + ex.ToString(), null);
			}
			return false;
		}

		private static bool readBinaryHeader(ByteReader Ba, SVD.sFile Sf, bool no_error = false)
		{
			Sf.thumb_position = -1;
			int num = -1;
			try
			{
				if (!Ba.readHeaderCheck("kawaisou ha kawaii. kono game ga ironna hito ni todokimasu youni. by hinayua"))
				{
					throw new Exception("HeaderError");
				}
				byte b = (byte)Ba.readByte();
				if (b >= 12)
				{
					Sf.version = (byte)Ba.readByte();
				}
				else
				{
					Sf.version = b;
				}
				Sf.playtime = Ba.readUInt();
				Sf.hp_noel = Ba.readUShort();
				Sf.maxhp_noel = Ba.readUShort();
				Sf.mp_noel = Ba.readUShort();
				Sf.maxmp_noel = Ba.readUShort();
				Sf.phase = Ba.readUShort();
				Sf.whole_map_key = Ba.readString("utf-8", false);
				Sf.modified = Ba.readDate();
				if (b >= 12)
				{
					num = (int)Ba.readUShort();
					if (num == 0)
					{
						Sf.memo = "";
					}
					else
					{
						Sf.memo = Ba.readMultiByte(num, "utf-8");
					}
					num = -1;
					if (b >= 13)
					{
						Sf.explore_timer = Ba.readUInt();
					}
				}
				if (b >= 35)
				{
					Sf.Achive.readFromBytes(Ba);
				}
				Sf.thumb_position = (short)Ba.position;
				if (Sf.loadstate == SVD.sFile.STATE.NO_LOAD)
				{
					Sf.loadstate = SVD.sFile.STATE.LOADED;
				}
			}
			catch (Exception ex)
			{
				if (!no_error || num < 0)
				{
					COOK.load_failure_announce = ex.Message;
					Sf.loadstate = SVD.sFile.STATE.ERROR;
					X.de("Loading header error: " + ex.ToString(), null);
				}
				return false;
			}
			return true;
		}

		public static void writeHeader(ByteArray Ba, SVD.sFile Sf)
		{
			Ba.writeMultiByte("kawaisou ha kawaii. kono game ga ironna hito ni todokimasu youni. by hinayua", "utf-8");
			Ba.writeByte(36);
			Ba.writeByte((int)Sf.version);
			Ba.writeUInt(Sf.playtime);
			Ba.writeUShort(Sf.hp_noel);
			Ba.writeUShort(Sf.maxhp_noel);
			Ba.writeUShort(Sf.mp_noel);
			Ba.writeUShort(Sf.maxmp_noel);
			Ba.writeUShort(Sf.phase);
			Ba.writeString(Sf.whole_map_key, "utf-8");
			Ba.writeDate(Sf.modified);
			Ba.writeString(Sf.memo, "utf-8");
			Ba.writeUInt(Sf.explore_timer);
			Sf.Achive.writeToBytes(Ba);
			Sf.thumb_position = (short)Ba.position;
		}

		private static bool readBinaryContent(ByteArray Ba, SVD.sFile Sf, NelM2DBase M2D = null)
		{
			if (Sf.thumb_position < 0)
			{
				Ba.position = 0UL;
				if (!COOK.readBinaryHeader(Ba, Sf, false))
				{
					return false;
				}
			}
			else
			{
				Ba.position = (ulong)Sf.thumb_position;
			}
			X.dli("<<Read SVD SVD From: " + Sf.index.ToString(), null);
			bool flag = true;
			string text = "";
			try
			{
				COOK.CurAchive.CopyFrom(Sf.Achive);
				SCN.newGame();
				uint num = Ba.readUInt();
				Ba.position += (ulong)num;
				if (Sf.version >= 9)
				{
					Sf.safe_area_memory = Ba.readString("utf-8", false);
				}
				else
				{
					Sf.safe_area_memory = "";
				}
				if (Sf.version >= 11)
				{
					Sf.first_version = (byte)Ba.readByte();
				}
				else
				{
					Sf.first_version = Sf.version;
				}
				text = "CFG";
				if (!COOK.do_not_load_sf_cfg)
				{
					using (ByteReader byteReader = Ba.readExtractBytes(4))
					{
						if (byteReader.Length != 0UL)
						{
							CFG.readBinary(byteReader, false);
						}
					}
					using (ByteReader byteReader2 = Ba.readExtractBytes(4))
					{
						if (byteReader2.Length != 0UL)
						{
							IN.getCurrentKeyAssignObject().readSaveString(byteReader2, false);
						}
						goto IL_015D;
					}
				}
				uint num2 = Ba.readUInt();
				Ba.position += (ulong)num2;
				num2 = Ba.readUInt();
				Ba.position += (ulong)num2;
				IL_015D:
				COOK.first_map_key = Ba.readString("utf-8", false);
				if (Sf.version < 9)
				{
					COOK.first_map_key = "forest_secret_lake";
				}
				text = "Osf";
				COOK.Osf = new BDic<string, byte>(Ba.readExtractBytesShifted(4).readDictionaryStrByte());
				if (Sf.version < 9)
				{
					COOK.Osf.Clear();
				}
				text = "GF";
				if (GF.readFromSvString(Ba) == null)
				{
					throw new Exception("");
				}
			}
			catch (Exception ex)
			{
				X.de("Loading content1 error: (at: " + text + ")" + ex.ToString(), null);
				flag = false;
			}
			Map2d map2d = M2D.Get(COOK.first_map_key, false);
			if (map2d == null)
			{
				X.de("Loading error: No specific map " + COOK.first_map_key, null);
				WholeMapItem wholeDescriptionByName = M2D.WM.GetWholeDescriptionByName(Sf.whole_map_key, false);
				if (wholeDescriptionByName != null)
				{
					map2d = wholeDescriptionByName.getFirstMap();
				}
				if (map2d == null)
				{
					map2d = M2D.Get("forest_secret_lake", false);
				}
			}
			bool flag2 = Sf.version < 31;
			Sf.last_map_key = map2d.key;
			M2D.setFlushAllFlag(true);
			PRNoel prNoel = M2D.getPrNoel();
			prNoel.newGame();
			M2D.AddToCoreMover(prNoel);
			CoinStorage.Clear();
			M2D.NightCon.clear();
			StoreManager.newGame();
			try
			{
				text = "Noel";
				prNoel.readBinaryFrom(Ba, Sf);
				if (Sf.version >= 9)
				{
					text = "RCP";
					RCP.readBinaryFrom(Ba, true);
				}
				if (Sf.version >= 34)
				{
					M2D.GUILD.readBinaryFrom(Ba, false);
				}
				text = "IMNG";
				M2D.IMNG.readBinaryFrom(Ba, true, false, flag2);
				prNoel.getSkillManager().fineIMNG();
				text = "WM";
				M2D.WM.readBinaryFrom(Ba, (int)Sf.version);
				if (Sf.version >= 6)
				{
					CoinStorage.readBinaryFrom(Ba, (int)Sf.version);
					if (Sf.version >= 7)
					{
						M2D.NightCon.readBinaryFrom(Ba);
						if (Sf.version <= 9)
						{
							M2D.NightCon.syncPosToWMIcon();
						}
						if (Sf.version >= 8)
						{
							StoreManager.StoreWholeReadBinaryFrom(Ba);
							if (Sf.version >= 9)
							{
								RCP.readingFinalize();
							}
							M2D.WDR.readBinaryFrom(Ba, Sf.version <= 18);
							if (Sf.version >= 14)
							{
								NEL.createTextLog().readBinaryFrom(Ba, false);
								if (Sf.version >= 15)
								{
									NelItem.readBinaryFrom(Ba, Sf.version <= 20, flag2);
									if (Sf.version >= 17)
									{
										M2D.QUEST.readBinaryFrom(Ba);
										try
										{
											DIFF.I = Ba.readByte();
											TRMManager.readBinaryFrom(Ba);
										}
										catch
										{
										}
										if (Sf.version >= 18)
										{
											UiEnemyDex.readBinaryFrom(Ba);
											if (Sf.version >= 25)
											{
												COOK.Mgm.readFromBytes(Ba, M2D);
											}
											else if (Sf.version >= 23)
											{
												COOK.Mgm.Dojo.readFromBytes(Ba);
												if (Sf.version >= 24)
												{
													Ba.readInt();
													Ba.readInt();
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (Sf.version >= 28)
				{
					uint num3 = Ba.readUInt();
					for (uint num4 = 0U; num4 < num3; num4 += 1U)
					{
						string text2 = Ba.readPascalString("utf-8", false);
						uint num5 = Ba.readUInt();
						if (num5 == 0U)
						{
							COOK.ODropItemMem[text2] = null;
						}
						else
						{
							ByteArray byteArray = (COOK.ODropItemMem[text2] = new ByteArray(num5));
							Ba.readBytes(byteArray, 0U, num5);
						}
					}
				}
				if (Sf.version >= 30)
				{
					CFGSP.readBinarySp(Ba);
					if (Sf.version < 36)
					{
						M2D.IMNG.refineSpConfigItemOnMainInventory();
					}
				}
			}
			catch (Exception ex2)
			{
				X.de("Loading content2 error: (at: " + text + ")" + ex2.ToString(), null);
				flag = false;
			}
			if (Sf.version < 35)
			{
				COOK.CurAchive.fineOldData(M2D);
			}
			if (Sf.version <= 20)
			{
				M2D.IMNG.assignObtainCountPreVersion();
			}
			if (Sf.version < 33)
			{
				SCN.initTotalBuyBarUnder(M2D);
			}
			M2D.changeMap(null);
			if (flag)
			{
				M2D.Cam.fineImmediately();
				M2D.NightCon.fineLevel(false);
				M2D.initMapMaterialASync(map2d, 2, false);
				M2D.setGfTempFlushFlagOnMapChange(false);
				if (Sf.version < 11)
				{
					StoreManager.reloadStorage(StoreManager.MODE.REMAKE);
				}
			}
			return flag;
		}

		public static ByteArray createBinary(ByteArray Ba = null, SVD.sFile Sf = null, NelM2DBase M2D = null, bool create_thumb = true, bool save_cfg = true)
		{
			COOK.save_failure_announce = "";
			if (Ba == null)
			{
				Ba = new ByteArray(0U);
			}
			if (Sf == null)
			{
				Sf = SVD.GetFile(0, false);
			}
			StoreManager.fineStorageBeforeSaveS();
			ulong position = Ba.position;
			Sf.saveInit();
			COOK.writeHeader(Ba, Sf);
			if (create_thumb)
			{
				Texture2D texture2D = Sf.createThumbnail(M2D);
				if (texture2D == null)
				{
					create_thumb = false;
				}
				else
				{
					ByteArray byteArray = COOK.createThumbJpegBinary(texture2D, 50);
					Ba.writeUInt((uint)byteArray.Length);
					Ba.writeBytes(byteArray, 0, -1);
				}
			}
			if (!create_thumb)
			{
				Sf.releaseThumb();
				Ba.writeUInt(0U);
			}
			Ba.writeString(Sf.safe_area_memory, "utf-8");
			Ba.writeByte((int)Sf.first_version);
			if (save_cfg)
			{
				ByteArray byteArray2 = CFG.createBinary(null);
				Ba.writeExtractBytes(byteArray2, 4, -1);
				byteArray2 = IN.getCurrentKeyAssignObject().getSaveString(null);
				Ba.writeExtractBytes(byteArray2, 4, -1);
			}
			else
			{
				Ba.writeUInt(0U);
				Ba.writeUInt(0U);
			}
			Ba.writeString(M2D.curMap.key, "utf-8");
			ByteArray byteArray3 = new ByteArray(0U);
			byteArray3.writeDictionary(COOK.Osf);
			Ba.writeExtractBytesShifted(byteArray3, 97, 4, -1);
			GF.writeSvString(Ba);
			M2D.getPrNoel().writeBinaryTo(Ba);
			RCP.writeBinaryTo(Ba);
			M2D.GUILD.writeBinaryTo(Ba);
			M2D.IMNG.writeBinaryTo(Ba);
			M2D.WM.writeBinaryTo(Ba);
			CoinStorage.writeBinaryTo(Ba);
			M2D.NightCon.writeBinaryTo(Ba);
			StoreManager.StoreWholeWriteBinaryTo(Ba);
			M2D.WDR.writeBinaryTo(Ba);
			NEL.createTextLog().writeBinaryTo(Ba, false);
			NelItem.writeBinaryTo(Ba);
			M2D.QUEST.writeBinaryTo(Ba);
			Ba.writeByte(DIFF.I);
			TRMManager.writeBinaryTo(Ba);
			UiEnemyDex.writeBinaryTo(Ba);
			COOK.Mgm.writeBinaryTo(Ba);
			uint count = (uint)COOK.ODropItemMem.Count;
			Ba.writeUInt(count);
			foreach (KeyValuePair<string, ByteArray> keyValuePair in COOK.ODropItemMem)
			{
				Ba.writePascalString(keyValuePair.Key, "utf-8");
				if (keyValuePair.Value == null)
				{
					Ba.writeUInt(0U);
				}
				else
				{
					Ba.writeUInt((uint)keyValuePair.Value.Length);
					Ba.writeBytes(keyValuePair.Value, 0, -1);
				}
			}
			CFGSP.writeBinarySp(Ba);
			return Ba;
		}

		public static ByteArray createThumbJpegBinary(Texture2D TxSrc, int quality = 20)
		{
			ByteArray byteArray = new ByteArray(0U);
			byteArray.writeUShort((ushort)TxSrc.width);
			byteArray.writeUShort((ushort)TxSrc.height);
			byteArray.writeBytes(TxSrc.EncodeToJPG(quality));
			return byteArray;
		}

		public static Texture2D readBinaryThumbnailFromLocal(SVD.sFile Sf)
		{
			FileStream fileStream = null;
			int num = 0;
			int num2 = 0;
			byte[] array2;
			try
			{
				string text = Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index));
				if (text.IndexOf("..") >= 0)
				{
					throw new Exception("invalid path string");
				}
				if (Sf.thumb_position < 0)
				{
					throw new Exception("Sf is not loaded");
				}
				int num3 = (int)Sf.thumb_position;
				fileStream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
				while (--num3 >= 0)
				{
					fileStream.ReadByte();
				}
				byte[] array = new byte[8];
				int i = 8;
				num3 = 0;
				while (i > 0)
				{
					int num4 = fileStream.Read(array, num3, X.Mn(8, i));
					num3 += num4;
					i -= num4;
				}
				uint num5 = (uint)(((int)array[0] << 24) | ((int)array[1] << 16) | ((int)array[2] << 8) | (int)array[3]);
				if (num5 == 0U)
				{
					throw new Exception("no thumbnail saved");
				}
				num5 -= 4U;
				num = ((int)array[4] << 8) | (int)array[5];
				num2 = ((int)array[6] << 8) | (int)array[7];
				int num6 = (int)X.Mn(fileStream.Length - (long)num3, (long)((ulong)num5));
				array2 = new byte[num6];
				i = num6;
				num3 = 0;
				while (i > 0)
				{
					int num4 = fileStream.Read(array2, num3, X.Mn(512, i));
					num3 += num4;
					i -= num4;
				}
				fileStream.Dispose();
			}
			catch
			{
				array2 = null;
				if (fileStream != null)
				{
					fileStream.Dispose();
				}
				Sf.thumb_error = true;
			}
			if (array2 != null && num > 0 && num2 > 0)
			{
				Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, false);
				texture2D.LoadImage(array2);
				texture2D.Apply(false, true);
				Sf.setThumb(texture2D);
				return texture2D;
			}
			UiSVD.descRedraw();
			return null;
		}

		private const string first_map_key_default = "forest_secret_lake";

		public static string first_map_key = "forest_secret_lake";

		private static BDic<string, byte> Osf;

		private static bool do_not_load_sf_cfg;

		private const string header_key = "kawaisou ha kawaii. kono game ga ironna hito ni todokimasu youni. by hinayua";

		public static int loaded_index = -1;

		public static short error_loaded_index = -1;

		public static bool reloading = false;

		private static SVD.sFile CurFile;

		public static ACHIVE CurAchive = new ACHIVE();

		public static float calced_timer;

		private static float calced_floort;

		public static Flagger FlgTimerStop;

		public static readonly MgmScoreHolder Mgm = new MgmScoreHolder();

		public static string save_failure_announce = "";

		public static string load_failure_announce = "";

		public static int map_walk_count = 0;

		public static readonly BDic<string, ByteArray> ODropItemMem = new BDic<string, ByteArray>();

		public const int SVD_VERSION = 36;

		public const int SVD_FIRST_VERSION_RECORDED = 11;
	}
}
