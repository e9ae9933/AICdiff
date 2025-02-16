using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class WholeMapManager
	{
		public WholeMapManager(NelM2DBase _M2D)
		{
			this.OWm = new BDic<string, WholeMapItem>();
			this.M2D = _M2D;
			this.refine_area_title = true;
			this.PeMg = new EffectHandlerPE(2);
		}

		public void restartGame()
		{
			this.refine_area_title = true;
			this.flush_material = true;
			this.CurWM = null;
			this.PeMg.deactivate(false);
		}

		public void endS(Map2d Mp)
		{
			this.refine_area_title = (this.changed_area = (this.init_game = (this.flush_material = false)));
			this.PeMg.deactivate(false);
		}

		public void initS(Map2d Mp)
		{
			if (Mp == null)
			{
				return;
			}
			WholeMapItem.WMRegex wmregex = null;
			if (this.CurWM != null && (wmregex = this.CurWM.isMatch(Mp)) != null)
			{
				this.CurWM.initS(Mp, wmregex);
			}
			else
			{
				WholeMapItem curWM = this.CurWM;
				this.CurWM = null;
				foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
				{
					if (keyValuePair.Value != curWM && (wmregex = keyValuePair.Value.isMatch(Mp)) != null)
					{
						this.CurWM = keyValuePair.Value;
						break;
					}
				}
				if (this.CurWM == null)
				{
					this.CurWM = this.DefWM;
				}
				if (this.CurWM != curWM)
				{
					string localized_name_areatitle = this.CurWM.localized_name_areatitle;
					if (this.pre_area_title_ != localized_name_areatitle && localized_name_areatitle != "")
					{
						this.pre_area_title_ = localized_name_areatitle;
						this.refine_area_title = true;
						if (!this.init_game)
						{
							this.flush_material = true;
						}
					}
					this.changed_area = true;
					this.M2D.WA.Touch(this.CurWM.text_key, "_whole_" + this.CurWM.text_key, false, false);
				}
				this.CurWM.initS(Mp, wmregex);
			}
			this.M2D.WA.Touch(this.CurWM.text_key, Mp.key, false, false);
		}

		public void initSAfter(Map2d Mp)
		{
			if (this.CurWM == null)
			{
				return;
			}
			if (this.CurWM.FD_initS != null)
			{
				MagicItem magicItem = this.M2D.MGC.setMagic(this.M2D.PlayerNoel, MGKIND.BASIC_SHOT, MGHIT.IMMEDIATE);
				MagicItem.FnMagicRun fd_initS = this.CurWM.FD_initS;
				MagicItem.FnMagicRun fnMagicRun;
				if (this.CurWM.FD_initS_draw == null)
				{
					fnMagicRun = (MagicItem Mg, float fcnt) => MagicItem.drawNone(Mg, fcnt);
				}
				else
				{
					fnMagicRun = this.CurWM.FD_initS_draw;
				}
				magicItem.initFunc(fd_initS, fnMagicRun).run(0f);
			}
		}

		public WholeMapManager reloadScript()
		{
			CsvReader csvReader = new CsvReader(NKT.readStreamingText("m2d/___whole_map_list.dat", false), CsvReader.RegSpace, true);
			WholeMapItem wholeMapItem = null;
			this.DefWM = null;
			this.OWm.Clear();
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					if (!this.OWm.TryGetValue(index, out wholeMapItem))
					{
						Map2d map2d = this.M2D.Get(index, true);
						if (map2d == null)
						{
							global::XX.X.de("whole map 対象 " + index + " がありません", null);
						}
						else
						{
							wholeMapItem = (this.OWm[map2d.key] = new WholeMapItem(this, map2d));
							if (this.DefWM == null)
							{
								this.DefWM = wholeMapItem;
							}
						}
					}
				}
				else if (wholeMapItem != null)
				{
					if (csvReader.cmd == "%DUNGEON")
					{
						wholeMapItem.DefDungeon = this.M2D.getDgnByKey(csvReader._1);
					}
					else if (csvReader.cmd == "%HEADER")
					{
						WholeMapItem.WMRegex wmregex;
						wholeMapItem.AReg.Add(wmregex = new WholeMapItem.WMRegex("^_*" + csvReader._1));
						if (csvReader.clength >= 3)
						{
							wmregex.Dgn = this.M2D.getDgnByKey(csvReader._2);
						}
					}
					else if (csvReader.cmd == "%REG")
					{
						WholeMapItem.WMRegex wmregex;
						wholeMapItem.AReg.Add(wmregex = new WholeMapItem.WMRegex(csvReader._1));
						if (csvReader.clength >= 3)
						{
							wmregex.Dgn = this.M2D.getDgnByKey(csvReader._2);
						}
					}
					else if (csvReader.cmd == "%TEXT_KEY")
					{
						wholeMapItem.text_key = csvReader._1;
					}
					else if (csvReader.cmd == "%TEXT_KEY_AREATITLE")
					{
						wholeMapItem.text_key_areatitle = csvReader._1;
					}
					else if (csvReader.cmd == "%MOBTYPE")
					{
						wholeMapItem.ev_mobtype = csvReader._1;
					}
					else if (csvReader.cmd == "%EV_OMORASHI")
					{
						wholeMapItem.ev_omorashi = csvReader._1;
					}
					else if (csvReader.cmd == "%SAFE")
					{
						wholeMapItem.safe_area = true;
					}
					else if (csvReader.cmd == "%DARK")
					{
						wholeMapItem.dark_area = true;
					}
					else if (csvReader.cmd == "%ZX_EVT")
					{
						wholeMapItem.zx_evt = csvReader._1;
					}
					else if (csvReader.cmd == "%BGM")
					{
						wholeMapItem.bgm_cue_key = csvReader._2;
						wholeMapItem.bgm_sheet_timing = csvReader._1;
					}
					else
					{
						if (csvReader.cmd == "%MG_INITS")
						{
							try
							{
								wholeMapItem.FD_initS = (MagicItem.FnMagicRun)Delegate.CreateDelegate(typeof(MagicItem.FnMagicRun), this, "fnMgRun_initS_" + csvReader._1);
							}
							catch
							{
							}
							try
							{
								wholeMapItem.FD_initS_draw = (MagicItem.FnMagicRun)Delegate.CreateDelegate(typeof(MagicItem.FnMagicRun), this, "fnMgDraw_initS_" + csvReader._1);
								continue;
							}
							catch
							{
								continue;
							}
						}
						if (csvReader.cmd == "%MATERIAL")
						{
							wholeMapItem.assignLoadMaterial(csvReader.slice(1, -1000));
						}
						else if (csvReader.cmd == "%STORE_FLUSH_BATTLE")
						{
							StoreManager.MODE mode;
							if (!FEnum<StoreManager.MODE>.TryParse(TX.valid(csvReader._2) ? csvReader._2 : "FLUSH", out mode, true))
							{
								csvReader.tError("不明なStoreManager.MODE: " + csvReader._2);
							}
							else
							{
								wholeMapItem.store_flush_onbattle_key = csvReader._1;
								wholeMapItem.store_flush_onbattle_type = mode;
							}
						}
						else if (csvReader.cmd == "%STORE_FLUSH")
						{
							StoreManager.MODE mode2;
							if (!FEnum<StoreManager.MODE>.TryParse(TX.valid(csvReader._2) ? csvReader._2 : "FLUSH", out mode2, true))
							{
								csvReader.tError("不明なStoreManager.MODE: " + csvReader._2);
							}
							else
							{
								wholeMapItem.store_flush_onenter_key = csvReader._1;
								wholeMapItem.store_flush_onenter_type = mode2;
							}
						}
						else if (csvReader.cmd == "%SHOW_ICON_ALWAYS")
						{
							wholeMapItem.default_show_icon_always = csvReader.Nm(1, 1f) != 0f;
						}
						else if (csvReader.cmd == "%WA_LINK")
						{
							WAManager.linkWA(wholeMapItem.text_key, csvReader);
						}
						else if (csvReader.cmd == "%WA")
						{
							WAManager.WARecord wa = WAManager.GetWa(wholeMapItem.text_key, true);
							if (wa == null)
							{
								csvReader.tError("不明なWA: " + wholeMapItem.text_key);
							}
							else
							{
								string _ = csvReader._1;
								if (_ != null && _ == "rect_already_known")
								{
									wa.rect_already_known = csvReader._B2;
								}
							}
						}
					}
				}
			}
			return this;
		}

		public WholeMapManager PrepareMapData()
		{
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
			{
				keyValuePair.Value.PrepareMapData();
			}
			return this;
		}

		public WholeMapItem GetWholeFor(Map2d Targ, bool alloc_empty = false)
		{
			if (Targ != null)
			{
				if (this.CurWM != null && Targ == this.CurWM.CurMap)
				{
					return this.CurWM;
				}
				foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
				{
					WholeMapItem value = keyValuePair.Value;
					if (value.isMatch(Targ) != null)
					{
						return value;
					}
				}
			}
			if (!alloc_empty)
			{
				return this.DefWM;
			}
			return null;
		}

		public WholeMapItem GetWholeFor(WholeMapItem.WMItem Wmi)
		{
			return this.getWholeMapDescription(Wmi.Lay.Mp);
		}

		public WholeMapItem getWholeMapDescription(Map2d WholeMap)
		{
			return global::XX.X.Get<string, WholeMapItem>(this.OWm, WholeMap.key);
		}

		public WholeMapItem GetWholeDescriptionByName(string _key, bool find_with_text_key = false)
		{
			WholeMapItem wholeMapItem;
			if (this.OWm.TryGetValue(_key, out wholeMapItem))
			{
				return wholeMapItem;
			}
			if (find_with_text_key)
			{
				foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
				{
					WholeMapItem value = keyValuePair.Value;
					if (value.text_key == _key)
					{
						return value;
					}
				}
			}
			return null;
		}

		public bool isWholeMap(Map2d Mp)
		{
			return this.OWm.ContainsKey(Mp.key);
		}

		public bool is_current_default
		{
			get
			{
				return this.CurWM != null && this.CurWM == this.DefWM;
			}
		}

		public float CLEN
		{
			get
			{
				return this.M2D.CLEN;
			}
		}

		public string pre_area_title
		{
			get
			{
				return this.pre_area_title_;
			}
		}

		public void clearAccess()
		{
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
			{
				keyValuePair.Value.clearAccess();
			}
			this.M2D.WA.newGame();
		}

		public WholeMapItem.WMTransferPoint.WMRectItem getDepertureRectSafeAreaMemory(string key, ref Map2d SrcMp, ref WholeMapItem DepWM)
		{
			if (TX.noe(key))
			{
				return null;
			}
			string text = SCN.replaceSafeAreaMemory(key, null);
			if (text != null)
			{
				key = text;
			}
			string[] array = TX.split(key, " ");
			if (array.Length != 2)
			{
				return null;
			}
			SrcMp = this.M2D.Get(array[0], false);
			if (SrcMp == null)
			{
				return null;
			}
			WholeMapItem wholeFor = this.GetWholeFor(SrcMp, true);
			if (wholeFor == null)
			{
				return null;
			}
			DepWM = wholeFor;
			string text2 = array[1];
			if (TX.isStart(text2, '!'))
			{
				return WholeMapItem.WMTransferPoint.WMRectItem.createTemporary(SrcMp, text2);
			}
			WholeMapItem.WMTransferPoint.WMRectItem depertureRect = wholeFor.getDepertureRect(SrcMp, -2, array[1]);
			if (depertureRect == null)
			{
				return null;
			}
			text = SCN.replaceSafeAreaMemory(key, depertureRect);
			if (text != null)
			{
				return this.getDepertureRectSafeAreaMemory(text, ref SrcMp, ref DepWM);
			}
			return depertureRect;
		}

		public void initWmLoadMaterial()
		{
			if (this.CurWM != null)
			{
				this.CurWM.initWmLoadMaterial();
			}
		}

		public void GetWMItem(string wm_key, string wmi_key, ref WholeMapItem Wm, ref WholeMapItem.WMItem Wmi, ref WholeMapItem.WMSpecialIcon WmSpIco)
		{
			wm_key = "__whole_" + wm_key;
			Wm = this.GetWholeDescriptionByName(wm_key, false);
			if (Wm == null)
			{
				return;
			}
			Wm.GetWMItem(wmi_key, ref Wmi, ref WmSpIco);
		}

		public WholeMapItem GetByTextKey(string text_key)
		{
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
			{
				if (keyValuePair.Value.text_key == text_key)
				{
					return keyValuePair.Value;
				}
			}
			return null;
		}

		public void assignStoreFlushFlag(bool onbattle, bool flush_wander_store = true)
		{
			if (this.CurWM == null)
			{
				return;
			}
			StoreManager.MODE mode = (onbattle ? this.CurWM.store_flush_onbattle_type : this.CurWM.store_flush_onenter_type);
			if (mode == StoreManager.MODE.NONE)
			{
				return;
			}
			StoreManager storeManager = StoreManager.Get(onbattle ? this.CurWM.store_flush_onbattle_key : this.CurWM.store_flush_onenter_key, false);
			if (storeManager != null)
			{
				storeManager.need_summon_flush = mode;
			}
			if (flush_wander_store)
			{
				StoreManager.FlushWanderingStore(mode);
			}
		}

		public BDic<UiGmMapMarker.MK, int> countMapMarker()
		{
			BDic<UiGmMapMarker.MK, int> bdic = new BDic<UiGmMapMarker.MK, int>(16);
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
			{
				keyValuePair.Value.countMapMarker(bdic);
			}
			return bdic;
		}

		public void readBinaryFrom(ByteArray Ba_first, int sf_version)
		{
			this.clearAccess();
			ByteArray byteArray = Ba_first.readExtractBytesShifted(4);
			int num = (int)byteArray.readUByte();
			for (int i = 0; i < num; i++)
			{
				string text = byteArray.readString("utf-8", false);
				if (!(text == ""))
				{
					ByteArray byteArray2 = byteArray.readExtractBytes(2);
					WholeMapItem wholeMapItem;
					if (this.OWm.TryGetValue(text, out wholeMapItem))
					{
						wholeMapItem.readBinaryFrom(byteArray2, sf_version);
					}
					else
					{
						global::XX.X.de("不明な WM: " + text, null);
					}
				}
			}
			bool flag = false;
			bool flag2 = false;
			if (sf_version >= 16)
			{
				try
				{
					this.M2D.WA.readBinaryFrom(byteArray);
					flag = true;
				}
				catch
				{
					flag2 = true;
				}
			}
			this.M2D.WA.initializeWmaTouchedData(this);
			if (!flag)
			{
				SCN.initializePreviousWA(this.M2D.WA);
			}
			else if (sf_version < 27)
			{
				this.M2D.WA.FirstAssign26();
			}
			if (flag2)
			{
				this.M2D.WA.validate();
			}
		}

		public void writeBinaryTo(ByteArray Ba_first)
		{
			ByteArray byteArray = new ByteArray(0U);
			int count = this.OWm.Count;
			byteArray.writeByte(count);
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in this.OWm)
			{
				ByteArray byteArray2 = keyValuePair.Value.createBinary();
				if (byteArray2 == null)
				{
					byteArray.writeString("", "utf-8");
				}
				else
				{
					byteArray.writeString(keyValuePair.Key, "utf-8");
					byteArray.writeExtractBytes(byteArray2, 2, -1);
				}
			}
			this.M2D.WA.writeBinaryTo(byteArray);
			Ba_first.writeExtractBytesShifted(byteArray, 161, 4, -1);
		}

		public BDic<string, WholeMapItem> getWholeMapDescriptionObject()
		{
			return this.OWm;
		}

		private bool fnMgRun_initS_Grazia(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase++;
				if (Mg.Mp.key.IndexOf("city_in") == 0 || Mg.Mp.key.IndexOf("school_in") == 0)
				{
					return false;
				}
				Mg.Other = new List<Vector4>(60);
				Mg.t = (Mg.sz = 20f);
				Mg.da = 0f;
				Mg.Mp.Meta.Get("sacred_mist");
			}
			this.PeMg.fine(100);
			if (Mg.t * (1f + (float)this.M2D.default_mist_pose * 0.7f) >= Mg.sz)
			{
				Mg.t = 0f;
				float num = global::XX.X.NIXP(0.5f, 1.75f);
				float x = Mg.Cen.x;
				float y = Mg.Cen.y;
				float num2 = num;
				float da = Mg.da;
				Mg.da = da + 1f;
				Vector4 vector = new Vector4(x, y, num2, da);
				vector.x = (this.M2D.Cam.x * Mg.Mp.rCLEN + global::XX.X.XORSPS() * IN.wh * Mg.Mp.rCLENB * 1.25f - (float)Mg.Mp.clms * 0.5f) / num * Mg.Mp.CLENB;
				vector.y = -(this.M2D.Cam.y * Mg.Mp.rCLEN + global::XX.X.XORSPS() * IN.hh * Mg.Mp.rCLENB * 1.25f - (float)Mg.Mp.rows * 0.5f) / num * Mg.Mp.CLENB;
				List<Vector4> list = Mg.Other as List<Vector4>;
				if (list != null)
				{
					list.Add(vector);
				}
			}
			return true;
		}

		private void fnMgDraw_drawAula(MagicItem Mg, List<Vector4> A, float fcnt, float min_maxt = 120f, float max_maxt = 220f, float min_zm = 60f, float max_zm = 145f, float min_alpha = 0.35f, float max_alpha = 1f, uint col = 4284990601U)
		{
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			meshImg.base_x = (meshImg.base_y = 0f);
			meshImg.initForImg(MTRX.EffBlurCircle245, 0);
			Vector3 posMainTransform = this.M2D.Cam.PosMainTransform;
			for (int i = A.Count - 1; i >= 0; i--)
			{
				Vector4 vector = A[i];
				float num = Mg.Mp.floort - vector.w * Mg.sz;
				uint ran = global::XX.X.GETRAN2((int)vector.w, 13 + ((int)vector.w & 7));
				float num2 = global::XX.X.ZLINE(num, global::XX.X.NI(min_maxt, max_maxt, global::XX.X.RAN(ran, 796)));
				if (num2 >= 1f)
				{
					A.RemoveAt(i);
				}
				else
				{
					meshImg.Identity();
					meshImg.TranslateP(vector.x - posMainTransform.x * 64f, vector.y - posMainTransform.y * 64f, false).Scale(vector.z, vector.z, false).TranslateP(posMainTransform.x * 64f, posMainTransform.y * 64f, false);
					float num3 = global::XX.X.RAN(ran, 1744) * 6.2831855f;
					float num4 = (global::XX.X.ZLINE(num2, 0.4f) - global::XX.X.ZLINE(num2 - 0.6f, 0.4f)) * global::XX.X.NI(min_alpha, max_alpha, global::XX.X.RAN(ran, 1401));
					float num5 = global::XX.X.NI(min_zm, max_zm, global::XX.X.RAN(ran, 727));
					float num6 = global::XX.X.NI(80, 140, global::XX.X.RAN(ran, 1437)) * num5 / 65f * num2;
					float num7 = global::XX.X.Cos(num3) * num6;
					float num8 = global::XX.X.Sin(num3) * num6;
					while (num4 > 0f)
					{
						meshImg.Col = meshImg.ColGrd.Set(col).mulA(global::XX.X.Mn(1f, num4)).C;
						meshImg.Rect(num7, num8, num5, num5, false);
						num4 -= 1f;
					}
				}
			}
		}

		private bool fnMgDraw_initS_Grazia(MagicItem Mg, float fcnt)
		{
			List<Vector4> list = Mg.Other as List<Vector4>;
			if (list == null)
			{
				return true;
			}
			this.fnMgDraw_drawAula(Mg, list, fcnt, 120f, 220f, 60f, 145f, 0.35f, 1f, 4284990601U);
			return true;
		}

		private bool fnMgRun_initS_Sacred(MagicItem Mg, float fcnt)
		{
			Map2d mp = Mg.Mp;
			if (Mg.phase == 0)
			{
				Mg.phase++;
				Mg.input_null_to_other_when_quit = true;
				PostEffectItem postEffectItem = null;
				PostEffectItem postEffectItem2 = null;
				PostEffectItem postEffectItem3 = null;
				PostEffectItem postEffectItem4;
				PostEffectItem postEffectItem5;
				if (mp.Meta.GetB("motherstone", false))
				{
					postEffectItem4 = PostEffect.IT.setPE(POSTM.POST_BLOOM, 20f, 0.15f, -20);
					postEffectItem5 = PostEffect.IT.setPE(POSTM.FINAL_ALPHA, 20f, 0.15f, -20);
					postEffectItem = PostEffect.IT.setPE(POSTM.BURST, 20f, 0.15f, -20);
					postEffectItem2 = PostEffect.IT.setPE(POSTM.JAMMING, 20f, 0.01f, -20);
					postEffectItem3 = PostEffect.IT.setPE(POSTM.ZOOM2_EATEN, 80f, 0.01f, -20);
					this.PeMg.Set(postEffectItem4);
					this.PeMg.Set(postEffectItem5);
					this.PeMg.Set(postEffectItem);
					this.PeMg.Set(postEffectItem2);
					this.PeMg.Set(postEffectItem3);
				}
				else
				{
					postEffectItem4 = PostEffect.IT.setPE(POSTM.POST_BLOOM, 20f, 0.15f, -20);
					postEffectItem5 = PostEffect.IT.setPE(POSTM.FINAL_ALPHA, 20f, 0.15f, -20);
				}
				Mg.dz = 10f;
				Mg.dx = 0f;
				Mg.dy = 0f;
				Mg.t = (Mg.sz = 3f);
				Mg.sa = 0f;
				Mg.da = 0f;
				M2LabelPoint point = Mg.Mp.getPoint("stone_center", true);
				if (point == null)
				{
					Mg.sx = (float)Mg.Mp.clms * 0.5f;
					Mg.sy = (float)Mg.Mp.rows * 0.5f;
				}
				else
				{
					Mg.sx = point.mapfocx;
					Mg.sy = point.mapfocy;
				}
				Mg.Other = new WholeMapManager.VHld_Sacred(Mg.Mp, Mg.sx, Mg.sy, postEffectItem4, postEffectItem5, postEffectItem, postEffectItem2, postEffectItem3);
				this.M2D.default_mist_pose = 1;
				this.M2D.pr_mp_consume_ratio = 0.08f;
			}
			this.PeMg.fine(100);
			WholeMapManager.VHld_Sacred vhld_Sacred = Mg.Other as WholeMapManager.VHld_Sacred;
			if (vhld_Sacred == null)
			{
				return false;
			}
			if (Mg.t >= Mg.sz)
			{
				Mg.t = 0f;
				float num = global::XX.X.NIXP(0.5f, 2.15f);
				float x = Mg.Cen.x;
				float y = Mg.Cen.y;
				float num2 = num;
				float da = Mg.da;
				Mg.da = da + 1f;
				Vector4 vector = new Vector4(x, y, num2, da);
				vector.x = (this.M2D.Cam.x * Mg.Mp.rCLEN + global::XX.X.XORSPS() * IN.wh * Mg.Mp.rCLENB * 1.25f - (float)Mg.Mp.clms * 0.5f) / num * Mg.Mp.CLENB;
				vector.y = -(this.M2D.Cam.y * Mg.Mp.rCLEN + global::XX.X.XORSPS() * IN.hh * Mg.Mp.rCLENB * 1.25f - (float)Mg.Mp.rows * 0.5f) / num * Mg.Mp.CLENB;
				vhld_Sacred.A.Add(vector);
			}
			PR pr = mp.Pr as PR;
			Mg.dz -= fcnt;
			if (Mg.dz <= 0f)
			{
				Mg.dz = 10f;
				if (pr != null)
				{
					Mg.dy = global::XX.X.ZLINE(global::XX.X.LENGTHXYS(Mg.sx, Mg.sy, pr.x, pr.y) - 25f, 52f);
				}
			}
			if (pr != null)
			{
				if (pr.isBurstState())
				{
					Mg.dx = -300f;
				}
				else if (Mg.dx >= 0f)
				{
					if (!pr.is_alive)
					{
						Mg.dx = -300f;
					}
					else if (!Map2d.can_handle)
					{
						Mg.dx = 0f;
					}
					else
					{
						Mg.dx += fcnt;
						if (Mg.dx >= 5f && pr.get_mp() < pr.get_maxmp() * pr.GaugeBrk.safe_holdable_ratio)
						{
							pr.cureMp(1);
							Mg.dx -= 5f;
							UIStatus.showHold(90, false);
						}
						if (Mg.dx >= 180f)
						{
							Mg.dx -= 50f;
							if (pr.GaugeBrk.gageDamage(false) < 0)
							{
								Mg.dx = -300f;
							}
							UIStatus.showHold(90, false);
						}
					}
				}
				else if (!Map2d.can_handle || !pr.is_alive)
				{
					Mg.dx = -300f;
				}
				else
				{
					Mg.dx += fcnt;
					if (Mg.dx >= 0f)
					{
						if (pr.GaugeBrk.gageDamage(false) < 0)
						{
							Mg.dx = -300f;
						}
						else
						{
							Mg.dx = 100f;
						}
					}
				}
			}
			if (vhld_Sacred.PeBurst != null)
			{
				vhld_Sacred.PeZm2.x = global::XX.X.VALWALK(vhld_Sacred.PeZm2.x, (float)((Mg.dy < 0.25f && this.can_handle && !global::XX.X.DEBUGNODAMAGE) ? 1 : 0), fcnt * 0.0045454544f);
				float num3 = global::XX.X.NI(0.55f, 1f, vhld_Sacred.PeZm2.x);
				float num4 = global::XX.X.Scr(Mg.dy, 1f - num3);
				vhld_Sacred.PeBloom.x = global::XX.X.MULWALK(vhld_Sacred.PeBloom.x, global::XX.X.NI(0.5f, 0.15f, num4), 0.008f);
				vhld_Sacred.PeAlpha.x = global::XX.X.MULWALK(vhld_Sacred.PeAlpha.x, global::XX.X.NI(0.78f, 0.02f, global::XX.X.ZSIN(num4)), 0.012f);
				vhld_Sacred.PeBurst.x = global::XX.X.MULWALK(vhld_Sacred.PeBurst.x, global::XX.X.NI(1f, 0.4f, num4), 0.008f);
				vhld_Sacred.PeNoise.x = global::XX.X.MULWALK(vhld_Sacred.PeNoise.x, global::XX.X.saturate(global::XX.X.NI(1f, -0.2f, global::XX.X.ZSIN(num4))), 0.012f);
				if (vhld_Sacred.PeZm2.x >= 0.75f && Mg.dy < 0.25f)
				{
					if (pr != null && !pr.isBurstState())
					{
						if (pr.hp_crack > 0 && UIStatus.PrIs(pr))
						{
							UIStatus.showHold(90, false);
						}
						if (Mg.sa >= 0f)
						{
							if (Mg.dx < 0f)
							{
								Mg.sa += fcnt * ((pr.hp_crack == 5) ? 0.3f : 1f);
							}
							else
							{
								Mg.dx += fcnt * 1.5f;
							}
							if (Mg.sa >= 80f)
							{
								Mg.sa = -180f;
								pr.setHpCrack(pr.hp_crack + 1);
							}
						}
					}
				}
				else if (Mg.sa > 0f)
				{
					Mg.sa = global::XX.X.VALWALK(Mg.sa, 0f, fcnt);
				}
			}
			else
			{
				vhld_Sacred.PeBloom.x = global::XX.X.MULWALK(vhld_Sacred.PeBloom.x, global::XX.X.NI(0.3f, 0.15f, Mg.dy), 0.008f);
				vhld_Sacred.PeAlpha.x = global::XX.X.MULWALK(vhld_Sacred.PeAlpha.x, global::XX.X.NI(0.5f, 0.24f, global::XX.X.ZSIN(Mg.dy)), 0.012f);
			}
			if (Mg.sa < 0f)
			{
				Mg.sa += fcnt;
			}
			return true;
		}

		private bool fnMgDraw_initS_Sacred(MagicItem Mg, float fcnt)
		{
			WholeMapManager.VHld_Sacred vhld_Sacred = Mg.Other as WholeMapManager.VHld_Sacred;
			if (vhld_Sacred == null)
			{
				return true;
			}
			this.fnMgDraw_drawAula(Mg, vhld_Sacred.A, fcnt, 60f, 150f, 25f, 220f, 1.5f, 3.4f, 4284990601U);
			return true;
		}

		public bool can_handle
		{
			get
			{
				return Map2d.can_handle && !this.M2D.debug_listener_active;
			}
		}

		private readonly BDic<string, WholeMapItem> OWm;

		public readonly NelM2DBase M2D;

		public WholeMapItem CurWM;

		private WholeMapItem DefWM;

		public bool refine_area_title;

		public bool flush_material;

		public bool init_game = true;

		public bool changed_area;

		private string pre_area_title_;

		private EffectHandlerPE PeMg;

		private const float sacred_zoom_maxt = 0.0045454544f;

		private class VHld_Sacred : IDisposable
		{
			public VHld_Sacred(Map2d Mp, float cx, float cy, PostEffectItem _PeBloom, PostEffectItem _PeAlpha, PostEffectItem _PeBurst, PostEffectItem _PeNoise, PostEffectItem _PeZm2)
			{
				this.SndA = Mp.M2D.Snd.Environment.AddLoop("sacred_noise_0", "vhld_sacred", cx, cy, 65f, 65f, 20f, 20f, null);
				if (_PeNoise != null)
				{
					this.SndB = Mp.M2D.Snd.Environment.AddLoop("sacred_noise_1", "vhld_sacred", cx, cy, 32f, 32f, 4f, 4f, null);
					this.SndB.min_volume = 0.125f;
				}
				else
				{
					this.SndA.Vol("vhld_sacred", 0.5f);
				}
				this.PeBloom = _PeBloom;
				this.PeAlpha = _PeAlpha;
				this.PeBurst = _PeBurst;
				this.PeNoise = _PeNoise;
				this.PeZm2 = _PeZm2;
			}

			public void Dispose()
			{
				if (this.PeBloom != null)
				{
					this.PeBloom.deactivate(false);
				}
				if (this.PeAlpha != null)
				{
					this.PeAlpha.deactivate(false);
				}
				if (this.PeBurst != null)
				{
					this.PeBurst.deactivate(false);
				}
				if (this.PeNoise != null)
				{
					this.PeNoise.deactivate(false);
				}
				if (this.PeZm2 != null)
				{
					this.PeZm2.deactivate(false);
				}
			}

			public readonly List<Vector4> A = new List<Vector4>(60);

			public readonly M2SndLoopItem SndA;

			public readonly M2SndLoopItem SndB;

			public readonly PostEffectItem PeBloom;

			public readonly PostEffectItem PeAlpha;

			public readonly PostEffectItem PeNoise;

			public readonly PostEffectItem PeZm2;

			public readonly PostEffectItem PeBurst;
		}
	}
}
