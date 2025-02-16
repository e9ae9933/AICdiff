using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class Map2d : IIdvName, IEventWaitListener
	{
		public float base_scale
		{
			get
			{
				return this.M2D.Cam.base_scale;
			}
		}

		public float CLENB
		{
			get
			{
				return this.CLEN * this.M2D.Cam.base_scale;
			}
		}

		public float rCLENB
		{
			get
			{
				return this.rCLEN / this.M2D.Cam.base_scale;
			}
		}

		public static float TS
		{
			get
			{
				return Map2d.TScur * Map2d.TSbase;
			}
		}

		public static float TSpr
		{
			get
			{
				return Map2d.TScur * Map2d.TSbasepr;
			}
		}

		public M2SubMap SubMapData
		{
			get
			{
				return this.SubMapData_;
			}
			set
			{
				if (this.SubMapData == value)
				{
					return;
				}
				this.SubMapData_ = value;
			}
		}

		public M2MeshContainer get_MMRD()
		{
			return this.MMRD;
		}

		public void releaseMeshLink(bool mmed_release = true, bool clear_drawers = false)
		{
			if (clear_drawers)
			{
				this.clearChipsDrawer(true);
			}
			if (this.MMRD != null)
			{
				this.MMRD.OnDestroy();
			}
			if (mmed_release)
			{
				this.MMRD = null;
			}
			this.MyDrawerB = null;
			this.MyDrawerG = null;
			this.MyDrawerT = null;
			this.MyDrawerTT = null;
			this.MyDrawerL = null;
			this.MyDrawerUGrd = null;
			this.MyDrawerBGrd = null;
			this.MyDrawerGGrd = null;
			this.MyDrawerTGrd = null;
			this.MyDrawerUCol = null;
			this.MyDrawerWater = null;
			if (this.Unstb != null)
			{
				this.Unstb = this.Unstb.destruct(false);
			}
		}

		public Map2d(M2DBase _M2D, string _key, bool _loadOnlyBasicFlag = false)
		{
			this.M2D = _M2D;
			this.CLEN = this.M2D.CLEN;
			this.rCLEN = 1f / this.CLEN;
			this.key = _key;
			this.loadOnlyBasicFlag = _loadOnlyBasicFlag;
			this.AMov = new M2Mover[4];
			this.APxlAnim = new M2PxlAnimator[4];
			this.mover_count = 0;
			this.pxlanim_count = 0;
			this.AEvStack = new M2EventCommand[4];
		}

		public M2ImageContainer IMGS
		{
			get
			{
				return this.M2D.IMGS;
			}
		}

		public M2EventContainer getEventContainer()
		{
			return this.EVC;
		}

		public void prepareCommand(TextAsset LT)
		{
			this.prepareCommand((LT != null) ? LT.text : "");
		}

		public void prepareCommand(string data)
		{
			this.EVC = new M2EventContainer(this, data);
			this.cmd_reload_flg = 1U;
		}

		public void reloadCommand(string data)
		{
			this.EVC.clear(true);
			this.EVC.parseText(data);
			this.cmd_reload_flg = 1U;
		}

		public CsvReader prepareBodyCsvReader(string loaddata = null)
		{
			if (loaddata == null)
			{
				return this.CR;
			}
			if (this.CR == null)
			{
				this.CR = new CsvReader(loaddata, new Regex("[ \\s\\t]*\\,[ \\s\\t]*"), true);
			}
			else
			{
				this.CR.parseText(loaddata);
			}
			this.CR.seek_set(0);
			while (this.CR.read() && this.setBasicData(this.CR, true))
			{
			}
			this.CR.seek_set(0);
			return this.CR;
		}

		public ByteArray prepareBodyByteArray(byte[] Aloaddata)
		{
			if (Aloaddata == null || Aloaddata.Length == 0)
			{
				return this.BaLoad;
			}
			this.BaLoad = new ByteArray(Aloaddata, false, true);
			this.binary_version = 0;
			this.BaLoad.position = 0UL;
			this.binary_version = (byte)this.BaLoad.readByte();
			while (this.setBasicData(this.BaLoad, true, null))
			{
			}
			if (this.Meta == null)
			{
				this.Meta = new META();
			}
			this.binary_content_position = (uint)this.BaLoad.position;
			return this.BaLoad;
		}

		private bool setBasicData(CsvReader CR, bool no_error = false)
		{
			if (CR.cmd == "")
			{
				return false;
			}
			string cmd = CR.cmd;
			if (cmd != null)
			{
				if (!(cmd == "name"))
				{
					if (!(cmd == "size"))
					{
						if (!(cmd == "bgcol"))
						{
							if (!(cmd == "comment"))
							{
								if (!(cmd == "csp"))
								{
									if (!(cmd == "%EDITOR"))
									{
										goto IL_011A;
									}
									this.Aadditional_for_editor = CR.slice_unescape(1, -1000);
								}
							}
							else
							{
								this.setComment(TX.decodeURIComponent(CR._1), null, false);
							}
						}
						else
						{
							this.bgcol0 = (this.bgcol = ((CR.clength == 2) ? C32.d2c((uint)CR._N1) : new Color32((byte)CR._N1, (byte)CR._N2, (byte)CR._N3, byte.MaxValue)));
						}
					}
					else
					{
						this.resize((int)(CR._N1 / this.CLEN), (int)(CR._N2 / this.CLEN), true);
					}
				}
				return true;
			}
			IL_011A:
			if (!no_error)
			{
				global::XX.X.de("MAPS 不明なbasicDataキー: " + CR.cmd, null);
			}
			return false;
		}

		private bool setBasicData(ByteArray BaLoad, bool no_error = false, M2MapMaterialLoader Loader = null)
		{
			if (BaLoad.bytesAvailable == 0UL)
			{
				return false;
			}
			Map2d.BIN_CTG bin_CTG = (Map2d.BIN_CTG)BaLoad.readByte();
			switch (bin_CTG)
			{
			case Map2d.BIN_CTG.NAME:
				BaLoad.readPascalString("utf-8", false);
				break;
			case Map2d.BIN_CTG.SIZE:
			{
				int num = (int)BaLoad.readUShort() / (int)this.CLEN;
				int num2 = (int)BaLoad.readUShort() / (int)this.CLEN;
				if (Loader == null || this.clms != num || this.rows != num2)
				{
					this.resize(num, num2, true);
				}
				break;
			}
			case Map2d.BIN_CTG.BGCOL:
				this.bgcol0 = new Color32((byte)BaLoad.readByte(), (byte)BaLoad.readByte(), (byte)BaLoad.readByte(), byte.MaxValue);
				break;
			case Map2d.BIN_CTG.COMMENT:
			{
				string text = BaLoad.readString("utf-8", false);
				if (Loader == null || this.comment != text)
				{
					this.setComment(text, null, false);
				}
				break;
			}
			case Map2d.BIN_CTG.CSP:
			{
				int num3 = BaLoad.readByte();
				int num4 = BaLoad.readByte();
				bool flag = num3 == M2DBase.Achip_pxl_key.Length;
				if (Loader != null)
				{
					if (!Loader.pxl_loaded && flag)
					{
						Loader.pxl_loaded = true;
						this.M2D.IMGS.Atlas.initCspAtlas(M2DBase.Achip_pxl_key[0]);
					}
					this.M2D.IMGS.Atlas.prepareChipImageDirectory("obj/", false);
				}
				while (--num4 >= 0)
				{
					string text2 = BaLoad.readPascalString("utf-8", false);
					if (Loader != null && flag && text2 != M2DBase.Achip_pxl_key[0])
					{
						this.M2D.IMGS.Atlas.initCspAtlas(text2);
					}
				}
				break;
			}
			case Map2d.BIN_CTG.EDITOR_ADDITIONAL:
			{
				int num4 = BaLoad.readByte();
				if (Loader == null)
				{
					this.Aadditional_for_editor = new string[num4];
					for (int i = 0; i < num4; i++)
					{
						this.Aadditional_for_editor[i] = BaLoad.readPascalString("utf-8", false);
					}
				}
				else
				{
					for (int j = 0; j < num4; j++)
					{
						BaLoad.readPascalString("utf-8", false);
					}
				}
				break;
			}
			case Map2d.BIN_CTG.MESH_RECT:
				this.GetRcPreDefined(BaLoad.readByte(), false).Set(BaLoad.readFloat(), BaLoad.readFloat(), BaLoad.readFloat(), BaLoad.readFloat());
				break;
			default:
				if (!no_error)
				{
					global::XX.X.de("MAPS 不明なbasicDataキー: " + bin_CTG.ToString(), null);
				}
				BaLoad.position -= 1UL;
				return false;
			}
			return true;
		}

		public bool prepareMaterialFromCurrentReader(M2MapMaterialLoader Loader, ref int load_line_cnt, List<Map2d> ASubMapTemp, bool load_additional = true)
		{
			if (this.BaLoad != null)
			{
				while (load_line_cnt != 0 && this.BaLoad.position < this.BaLoad.Length)
				{
					if (!this.setBasicData(this.BaLoad, true, Loader))
					{
						Map2d.BIN_CTG bin_CTG = (Map2d.BIN_CTG)this.BaLoad.readByte();
						if (bin_CTG != Map2d.BIN_CTG.LAYER_REVERSE)
						{
							switch (bin_CTG)
							{
							case Map2d.BIN_CTG.LAYER_HEADER:
							{
								bool flag;
								M2MapLayer.readBytesContentLay(this.BaLoad, null, out flag);
								break;
							}
							case Map2d.BIN_CTG.PAT_CHANGE:
								this.BaLoad.readUInt();
								break;
							case Map2d.BIN_CTG.CP:
							case Map2d.BIN_CTG.PIC:
							{
								M2ChipImage m2ChipImage = this.IMGS.GetById(this.BaLoad.readUInt()) as M2ChipImage;
								if (bin_CTG == Map2d.BIN_CTG.CP)
								{
									M2MapLayer.readBytesContentCp(this.BaLoad, null, null, 0, 0);
								}
								else if (bin_CTG == Map2d.BIN_CTG.PIC)
								{
									M2MapLayer.readBytesContentPic(this.BaLoad, null, null, 0, 0);
								}
								if (m2ChipImage != null)
								{
									this.M2D.IMGS.Atlas.prepareChipImageDirectory(m2ChipImage, false);
								}
								load_line_cnt--;
								break;
							}
							case Map2d.BIN_CTG.LP:
							{
								Map2d.LpLoader lpLoader;
								M2LabelPoint.readBytesContentLp(this.BaLoad, null, out lpLoader, load_additional, false, 0, 0);
								if (load_additional)
								{
									this.M2D.loadMaterialForLabelPoint(this, lpLoader);
									load_line_cnt--;
								}
								break;
							}
							case Map2d.BIN_CTG.GRD:
								M2GradationRect.readBytesContentGrd(this.BaLoad, null, false, 0, 0);
								break;
							case Map2d.BIN_CTG.SM:
							{
								string text;
								M2SubMap.readBytesContentSm(this.BaLoad, null, -1, out text, (int)this.get_binary_version());
								Map2d map2d = this.M2D.Get(text, false);
								if (map2d != null && ASubMapTemp.IndexOf(map2d) == -1)
								{
									ASubMapTemp.Add(map2d);
								}
								break;
							}
							default:
								global::XX.X.de("prepareMaterialFromCurrentReader: 不明なバイトブロック: " + bin_CTG.ToString() + " @" + this.key, null);
								break;
							}
						}
						else
						{
							this.BaLoad.readInt();
						}
					}
				}
				if (this.BaLoad.position >= this.BaLoad.Length)
				{
					if (load_additional)
					{
						this.prepareMeta();
						this.M2D.loadMaterialSnd(this.Meta.Get("_load_snd"));
					}
					return false;
				}
			}
			else
			{
				while (load_line_cnt != 0 && this.CR.read())
				{
					string cmd = this.CR.cmd;
					if (cmd != null)
					{
						if (!(cmd == "c") && !(cmd == "p"))
						{
							if (!(cmd == "lp"))
							{
								if (!(cmd == "csp"))
								{
									if (!(cmd == "sm"))
									{
										if (cmd == "%EDITOR")
										{
											this.Aadditional_for_editor = this.CR.slice_unescape(1, -1000);
										}
									}
									else if (ASubMapTemp != null)
									{
										Map2d map2d2 = this.M2D.Get(this.CR._1, false);
										if (map2d2 != null && ASubMapTemp.IndexOf(map2d2) == -1)
										{
											ASubMapTemp.Add(map2d2);
										}
									}
								}
								else if (this.CR.Int(1, 0) == M2DBase.Achip_pxl_key.Length)
								{
									string[] array = this.CR.slice(1, -1000);
									array[0] = M2DBase.Achip_pxl_key[0];
									this.M2D.IMGS.Atlas.initCspAtlas(array);
									load_line_cnt--;
									Loader.pxl_loaded = true;
								}
							}
							else if (load_additional)
							{
								this.M2D.loadMaterialForLabelPoint(this, new Map2d.LpLoader
								{
									key = this.CR._1,
									command = this.CR._6,
									comment = this.CR._7
								});
								load_line_cnt--;
							}
						}
						else
						{
							M2ChipImage m2ChipImage2 = this.IMGS.Get(this.CR._1);
							if (m2ChipImage2 != null)
							{
								this.M2D.IMGS.Atlas.prepareChipImageDirectory(m2ChipImage2, false);
								if (load_additional && !m2ChipImage2.loaded_additional_material)
								{
									this.M2D.loadAdditionalMaterialForChip(m2ChipImage2);
								}
								load_line_cnt--;
							}
						}
					}
				}
				if (this.CR.isEnd())
				{
					if (load_additional)
					{
						this.prepareMeta();
						this.M2D.loadMaterialSnd(this.Meta.Get("_load_snd"));
					}
					return false;
				}
			}
			return true;
		}

		public Map2d load(bool removing_error_chips = false)
		{
			this.KeyLayer = null;
			this.ARunningObject = null;
			this.ALay = null;
			this.ASubMaps = new M2SubMap[0];
			M2MapLayer m2MapLayer = null;
			if (this.CR == null && this.BaLoad == null)
			{
				return this;
			}
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			if (this.BaLoad != null)
			{
				if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP)
				{
					if (this.AAPt == null)
					{
						this.AAPt = new M2Pt[this.clms, this.rows];
					}
					else
					{
						for (int i = 0; i < this.clms; i++)
						{
							for (int j = 0; j < this.rows; j++)
							{
								this.AAPt[i, j] = null;
							}
						}
					}
				}
				this.BaLoad.position = (ulong)this.binary_content_position;
				uint num2 = 0U;
				while (this.BaLoad.position < this.BaLoad.Length)
				{
					if (m2MapLayer == null || !m2MapLayer.load(this.BaLoad, ref num2, ref num, 0, 0))
					{
						Map2d.BIN_CTG bin_CTG = (Map2d.BIN_CTG)this.BaLoad.readByte();
						if (bin_CTG != Map2d.BIN_CTG.LAYER_REVERSE)
						{
							switch (bin_CTG)
							{
							case Map2d.BIN_CTG.LAYER_HEADER:
								m2MapLayer = this.loadReaderLayerInitialize2(this.loadReaderLayerInitialize(this.BaLoad, -1, this.loadOnlyBasicFlag, flag), false, out flag2);
								break;
							case Map2d.BIN_CTG.PAT_CHANGE:
								num2 = this.BaLoad.readUInt();
								break;
							case Map2d.BIN_CTG.CP:
							{
								M2ChipImage m2ChipImage = this.IMGS.GetById(this.BaLoad.readUInt()) as M2ChipImage;
								M2MapLayer.readBytesContentCp(this.BaLoad, null, m2ChipImage, 0, 0);
								break;
							}
							case Map2d.BIN_CTG.PIC:
							{
								M2ChipImage m2ChipImage = this.IMGS.GetById(this.BaLoad.readUInt()) as M2ChipImage;
								M2MapLayer.readBytesContentPic(this.BaLoad, null, m2ChipImage, 0, 0);
								break;
							}
							case Map2d.BIN_CTG.LP:
								M2LabelPoint.readBytesContentLp(this.BaLoad, null, false, 0, 0);
								break;
							case Map2d.BIN_CTG.GRD:
								M2GradationRect.readBytesContentGrd(this.BaLoad, null, false, 0, 0);
								break;
							case Map2d.BIN_CTG.SM:
							{
								string text;
								M2SubMap m2SubMap = M2SubMap.readBytesContentSm(this.BaLoad, this, this.ASubMaps.Length, out text, (int)this.get_binary_version());
								if (m2SubMap != null)
								{
									global::XX.X.push<M2SubMap>(ref this.ASubMaps, m2SubMap, -1);
								}
								break;
							}
							default:
								global::XX.X.de("load: バイナリに不明なデータ " + bin_CTG.ToString() + " @" + this.key, null);
								break;
							}
						}
						else
						{
							int num3 = this.BaLoad.readInt();
							this.ALay = new M2MapLayer[num3];
							flag = true;
						}
					}
				}
			}
			if (this.Meta == null)
			{
				this.Meta = new META(null);
			}
			if (this.clms == 0 || this.rows == 0)
			{
				this.resize(24, 24, true);
			}
			if (this.loadOnlyBasicFlag)
			{
				return this;
			}
			if (m2MapLayer == null)
			{
				m2MapLayer = new M2MapLayer(this, null, null, 0U);
				if (this.ALay == null)
				{
					this.ALay = new M2MapLayer[] { m2MapLayer };
				}
				else if (flag)
				{
					global::XX.X.unshiftEmpty<M2MapLayer>(this.ALay, m2MapLayer, 0, 1, -1);
				}
				else
				{
					global::XX.X.push<M2MapLayer>(ref this.ALay, m2MapLayer, -1);
				}
			}
			for (int k = this.ALay.Length - 1; k >= 0; k--)
			{
				this.ALay[k].index = k;
			}
			if (this.KeyLayer == null)
			{
				this.KeyLayer = this.ALay[0];
			}
			if (num > 0)
			{
				global::XX.X.dl("不明なチップが" + num.ToString() + " 枚あります", null, false, false);
			}
			return this;
		}

		public bool binary_loading_mode
		{
			get
			{
				return this.BaLoad != null;
			}
		}

		public int get_crop_value()
		{
			return this.crop;
		}

		public Vector2 getMapWmLevel(float mapx, float mapy)
		{
			this.prepared = true;
			float num = (float)this.get_crop_value();
			return new Vector2(global::XX.X.saturate((mapx - num) / ((float)this.clms - num * 2f)), global::XX.X.saturate((mapy - num) / ((float)this.rows - num * 2f)));
		}

		public Vector2 getMapWmLevel(float mapx, float mapy, float showw, float showh, bool check_wm_xy_calcuration = true, bool integerize = true)
		{
			if (showw <= 1f && showh <= 1f)
			{
				this.prepared = true;
				int crop_value = this.get_crop_value();
				return new Vector2(0.5f, 0.5f);
			}
			Vector2 vector = this.getMapWmLevel(mapx, mapy);
			if (check_wm_xy_calcuration)
			{
				using (BList<float> blist = ListBuffer<float>.Pop(0))
				{
					if (this.Meta.CopyTo("wm_xy_calcuration", blist, -1f))
					{
						float num = ((blist.Count > 0) ? blist[0] : 1f);
						float num2 = ((blist.Count > 1) ? blist[1] : 0f);
						float num3 = ((blist.Count > 2) ? blist[2] : 0f);
						float num4 = ((blist.Count > 3) ? blist[3] : 1f);
						float num5 = ((blist.Count > 4) ? blist[4] : 0f);
						float num6 = ((blist.Count > 5) ? blist[5] : 0f);
						vector = new Vector2(vector.x * num + vector.y * num2 + num5, vector.x * num3 + vector.y * num4 + num6);
					}
				}
			}
			if (integerize)
			{
				return new Vector2(global::XX.X.MMX(0.5f, (float)global::XX.X.IntR(vector.x * (showw - 1f)) + 0.5f, showw - 0.5f), global::XX.X.MMX(0.5f, (float)global::XX.X.IntR(vector.y * (showh - 1f)) + 0.5f, showh - 0.5f));
			}
			return new Vector2(global::XX.X.MMX(0.5f, (float)global::XX.X.IntR(vector.x * (showw - 1f) / 0.25f) * 0.25f + 0.5f, showw - 0.5f), global::XX.X.MMX(0.5f, (float)global::XX.X.IntR(vector.y * (showh - 1f) / 0.25f) * 0.25f + 0.5f, showh - 0.5f));
		}

		public int crop
		{
			get
			{
				if (this.crop_ == -1)
				{
					if (this.prepareMeta() == null)
					{
						return 0;
					}
					this.crop_ = this.Meta.GetI("crop", 0, 0);
				}
				return this.crop_;
			}
		}

		public bool reloadWholePxls(bool recreate_mmrd = false)
		{
			if (!this.prepared)
			{
				return false;
			}
			this.closeAction(true, true, false);
			this.closeSubMaps(false);
			this.releaseMeshLink(true, false);
			this.Dgn = null;
			this.prepareMeshDrawer(this.SubMapData, ref this.MMRD);
			this.openSubMaps();
			this.need_reentry_flag_ = (this.need_reentry_gradation_flag_ = true);
			this.update_mesh_flag_ = 26367 | ((this.mode == MAPMODE.NORMAL) ? 2048 : 0);
			return true;
		}

		public void reassignChipAtlas()
		{
			int num = this.count_layers;
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].reassignChipAtlas();
			}
			if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP)
			{
				num = this.count_submaps;
				for (int j = 0; j < num; j++)
				{
					this.ASubMaps[j].getTargetMap().reassignChipAtlas();
				}
			}
		}

		public M2MapLayer loadReaderLayerInitialize2(M2MapLayer CL, bool name_check, out bool debug_layer_mode)
		{
			debug_layer_mode = false;
			if (CL == null)
			{
				return null;
			}
			if (name_check)
			{
				CL.name = global::XX.X.fineIndividualName<M2MapLayer>(this.ALay, CL.name, CL);
			}
			if (!global::XX.X.DEBUG || global::XX.X.DEBUG_PLAYER)
			{
				debug_layer_mode = CL.name.IndexOf("DEBUG") == 0;
			}
			else
			{
				debug_layer_mode = CL.name.IndexOf("DEBUG_OFF") == 0;
			}
			if (debug_layer_mode)
			{
				CL.initDebugLayerMode();
			}
			return CL;
		}

		public M2MapLayer loadReaderLayerInitialize(ByteArray Ba, int index = -1, bool do_not_load = false, bool reverse = false)
		{
			bool flag;
			M2MapLayer m2MapLayer = M2MapLayer.readBytesContentLay(Ba, do_not_load ? null : this, out flag);
			if (m2MapLayer == null)
			{
				return null;
			}
			if (flag)
			{
				this.KeyLayer = m2MapLayer;
			}
			if (this.ALay == null)
			{
				this.ALay = new M2MapLayer[] { m2MapLayer };
			}
			else if (reverse)
			{
				global::XX.X.unshiftEmpty<M2MapLayer>(this.ALay, m2MapLayer, 0, 1, -1);
			}
			else
			{
				global::XX.X.push<M2MapLayer>(ref this.ALay, m2MapLayer, index);
			}
			return m2MapLayer;
		}

		public int fnSortChips(M2Puts CpA, M2Puts CpB)
		{
			int num = CpA.index - CpB.index;
			if (num > 0)
			{
				return 1;
			}
			if (num >= 0)
			{
				return 0;
			}
			return -1;
		}

		public bool point_prepared
		{
			get
			{
				return this.AAPt != null;
			}
		}

		public void reconnectWholeChips()
		{
			this.AAPt = new M2Pt[this.clms, this.rows];
			if (this.ALay == null)
			{
				return;
			}
			for (int i = this.ALay.Length - 1; i >= 0; i--)
			{
				this.ALay[i].copyValuesFromMap(this);
			}
		}

		public void resize(int _clms, int _rows, bool no_consider_config = false)
		{
			if (_clms <= 0 || _rows <= 0)
			{
				global::XX.X.de("Map2d::setBasicData 不正なclms/rows", null);
				_clms = global::XX.X.Mx(this.clms, 1);
				_rows = global::XX.X.Mx(this.rows, 1);
			}
			_clms = (int)global::XX.X.Mn((float)_clms, 500f * this.CLEN);
			_rows = (int)global::XX.X.Mn((float)_rows, 500f * this.CLEN);
			this.clms = _clms;
			this.rows = _rows;
			this.width = (int)((float)this.clms * this.CLEN);
			this.height = (int)((float)this.rows * this.CLEN);
			if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP || this.AAPt != null)
			{
				this.AAPt = new M2Pt[this.clms, this.rows];
			}
			if (this.AAPt != null && this.ALay != null)
			{
				for (int i = 0; i < this.ALay.Length; i++)
				{
					this.ALay[i].copyValuesFromMap(this);
				}
			}
			if (!no_consider_config)
			{
				this.considerConfig4(0, 0, this.clms, this.rows);
				for (int j = 0; j < this.mover_count; j++)
				{
					this.AMov[j].fineTransformToPos(true, false);
				}
			}
			if (this.AMov != null)
			{
				for (int k = this.mover_count - 1; k >= 0; k--)
				{
					this.AMov[k].fineTransformToPos(true, false);
				}
			}
		}

		public void considerConfig4(DRect Rc, int sx, int sy)
		{
			this.considerConfig4((int)Rc.x + sx, (int)Rc.y + sy, global::XX.X.IntC(Rc.right) + sx, global::XX.X.IntC(Rc.bottom) + sy);
		}

		public void considerConfig4(M2LabelPoint Rc)
		{
			this.considerConfig4(Rc.mapx, Rc.mapy, Rc.mapx + Rc.mapw, Rc.mapy + Rc.maph);
		}

		public void considerConfig4(int _l, int _t, int _r, int _b)
		{
			if (this.AAPt == null)
			{
				return;
			}
			int num = global::XX.X.Mx(0, this.crop - 2);
			int num2 = this.clms;
			for (int i = _l; i < _r; i++)
			{
				if (global::XX.X.BTW(0f, (float)i, (float)this.clms))
				{
					for (int j = _t; j < _b; j++)
					{
						if (global::XX.X.BTW(0f, (float)j, (float)this.rows))
						{
							M2Pt m2Pt = this.AAPt[i, j];
							if (m2Pt != null)
							{
								m2Pt.resetConfig();
							}
						}
					}
				}
			}
			int num3 = this.ALay.Length;
			for (int k = 0; k < num3; k++)
			{
				M2MapLayer m2MapLayer = this.ALay[k];
				if (!m2MapLayer.do_not_consider_config)
				{
					M2LabelPointContainer lp = m2MapLayer.LP;
					if (lp != null)
					{
						int length = lp.Length;
						for (int j = 0; j < length; j++)
						{
							lp.Get(j).considerConfig4(_l, _t, _r, _b, this.AAPt);
						}
					}
				}
			}
			for (int i = _l; i < _r; i++)
			{
				if (global::XX.X.BTW(0f, (float)i, (float)this.clms))
				{
					for (int j = _t; j < _b; j++)
					{
						if (global::XX.X.BTW(0f, (float)j, (float)this.rows))
						{
							M2Pt m2Pt2 = this.AAPt[i, j];
							if (m2Pt2 != null)
							{
								m2Pt2.calcConfig(i, j, true);
							}
						}
					}
				}
			}
			if (!Map2d.editor_decline_lighting && this.mode == MAPMODE.NORMAL)
			{
				this.M2D.configReconsidered(this.AAPt, _l, _t, _r, _b);
			}
		}

		public void setDangerousFlag(DRect Rc)
		{
			this.setDangerousFlag((int)Rc.x, (int)Rc.y, global::XX.X.IntC(Rc.right), global::XX.X.IntC(Rc.bottom));
		}

		public void setDangerousFlag(int l, int t, int w, int h)
		{
			int num = l + w;
			int num2 = t + h;
			for (int i = l; i < num; i++)
			{
				if (global::XX.X.BTW(0f, (float)i, (float)this.clms))
				{
					for (int j = t; j < num2; j++)
					{
						if (global::XX.X.BTW(0f, (float)j, (float)this.rows))
						{
							M2Pt m2Pt = this.AAPt[i, j];
							if (m2Pt == null)
							{
								m2Pt = (this.AAPt[i, j] = new M2Pt(0, 4));
							}
							m2Pt.dangerous = true;
						}
					}
				}
			}
		}

		public void checkReindexAllLayer()
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].checkReindex(false);
			}
		}

		public static void getPutsBounds(M2Puts Mcp, float CLEN, out int l, out int t, out int r, out int b)
		{
			l = Mcp.mapx;
			t = Mcp.mapy;
			r = global::XX.X.IntC((float)(Mcp.drawx + Mcp.iwidth) / CLEN);
			b = global::XX.X.IntC((float)(Mcp.drawy + Mcp.iheight) / CLEN);
		}

		public int connectImgLink(M2Puts Mcp, Map2d.CONNECTIMG connect = Map2d.CONNECTIMG.ASSIGN, DRect LayerBounds = null, bool no_consider_config = true)
		{
			int num = 0;
			int num2;
			int num3;
			int num4;
			int num5;
			Mcp.Img.getPutsBounds(Mcp, this.CLEN, out num2, out num3, out num4, out num5);
			int num6 = num4 - num2;
			int num7 = num5 - num3;
			if (LayerBounds != null && connect == Map2d.CONNECTIMG.ASSIGN)
			{
				this.extendBounds(LayerBounds, num2, num3, num6, num7);
			}
			if (this.AAPt != null)
			{
				for (int i = num3; i < num5; i++)
				{
					if (global::XX.X.BTW(0f, (float)i, (float)this.rows))
					{
						for (int j = num2; j < num4; j++)
						{
							if (global::XX.X.BTW(0f, (float)j, (float)this.clms))
							{
								M2Pt m2Pt = this.AAPt[j, i];
								if (connect != Map2d.CONNECTIMG.ASSIGN)
								{
									if (m2Pt != null)
									{
										m2Pt.Rem(Mcp);
										if (m2Pt.count == 0)
										{
											this.AAPt[j, i] = null;
										}
										num++;
									}
								}
								else
								{
									if (m2Pt == null)
									{
										m2Pt = (this.AAPt[j, i] = new M2Pt(4, 4));
									}
									m2Pt.Add(Mcp);
									num++;
								}
							}
						}
					}
				}
			}
			if (num != 0 && !no_consider_config && Mcp.config_considerable)
			{
				this.considerConfig4(num2, num3, num4, num5);
			}
			return num;
		}

		public void extendBounds(DRect Bounds, int l, int t, int w, int h)
		{
			global::XX.X.Mn(this.clms, l + w);
			global::XX.X.Mn(this.rows, t + h);
			l = global::XX.X.Mx(0, l);
			t = global::XX.X.Mx(0, t);
			float x = Bounds.x;
			float y = Bounds.y;
			Bounds.Expand((float)l, (float)t, (float)w, (float)h, false);
		}

		public M2Pt getPointPuts(int x, int y, bool making_if_not_exists = false, bool clipping = false)
		{
			if (this.AAPt == null)
			{
				global::XX.X.de("AAPtが存在しない状態で getPointPuts がコールされた " + this.key, null);
				return null;
			}
			if (clipping)
			{
				x = global::XX.X.MMX(0, x, this.clms - 1);
				y = global::XX.X.MMX(0, y, this.rows - 1);
			}
			else if (!global::XX.X.BTW(0f, (float)x, (float)this.clms) || !global::XX.X.BTW(0f, (float)y, (float)this.rows))
			{
				return null;
			}
			M2Pt m2Pt = this.AAPt[x, y];
			if (m2Pt == null && making_if_not_exists)
			{
				m2Pt = (this.AAPt[x, y] = new M2Pt(4, 4));
			}
			return m2Pt;
		}

		public static ulong LayArray2Bits(M2MapLayer Lay)
		{
			if (Lay == null)
			{
				return ulong.MaxValue;
			}
			return 1UL << Lay.index;
		}

		public static ulong LayArray2Bits(M2MapLayer[] ALays)
		{
			if (ALays.Length == 0)
			{
				return ulong.MaxValue;
			}
			ulong num = 0UL;
			for (int i = ALays.Length - 1; i >= 0; i--)
			{
				M2MapLayer m2MapLayer = ALays[i];
				if (m2MapLayer != null)
				{
					int index = m2MapLayer.index;
					if (index >= 0)
					{
						num |= 1UL << index;
					}
				}
			}
			return num;
		}

		public int getPointPutsTo(int x, int y, bool check_dupe, List<M2Puts> APuts, int adding_max_from_one_pixel = -1, M2MapLayer TargetLay = null)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			if (pointPuts == null)
			{
				return 0;
			}
			return pointPuts.getPointPutsTo(x, y, check_dupe, APuts, adding_max_from_one_pixel, Map2d.LayArray2Bits(TargetLay));
		}

		public int getFloatPointPutsTo(float x, float y, bool check_dupe, List<M2Puts> APuts, int adding_max_from_one_pixel = -1, float picture_strict_extend = -1f, M2MapLayer TargetLay = null)
		{
			M2Pt pointPuts = this.getPointPuts((int)x, (int)y, false, false);
			if (pointPuts == null)
			{
				return 0;
			}
			return pointPuts.getFloatPointPutsTo(x, y, check_dupe, APuts, adding_max_from_one_pixel, picture_strict_extend, Map2d.LayArray2Bits(TargetLay));
		}

		public int getFloatPointPutsTo(float x, float y, bool check_dupe, List<M2Puts> APuts, int adding_max_from_one_pixel, float picture_strict_extend, M2MapLayer[] ATargetLay)
		{
			M2Pt pointPuts = this.getPointPuts((int)x, (int)y, false, false);
			if (pointPuts == null)
			{
				return 0;
			}
			return pointPuts.getFloatPointPutsTo(x, y, check_dupe, APuts, adding_max_from_one_pixel, picture_strict_extend, Map2d.LayArray2Bits(ATargetLay));
		}

		public bool removeChip(M2Puts obj, bool no_consider_config = false, bool no_sort = false)
		{
			return obj.Lay.removeChip(obj, no_consider_config, no_sort);
		}

		public int removeChip(List<M2Puts> ACp, bool no_consider_config = false, bool no_sort = false)
		{
			int count = ACp.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				num = (ACp[i].Lay.removeChip(ACp[i], no_consider_config, no_sort) ? 1 : 0);
			}
			return num;
		}

		public void assignNewMapChip(List<M2Puts> ACp, bool no_consider_config = false, bool sorting = false, bool adding_in_playing = false)
		{
			int count = ACp.Count;
			for (int i = 0; i < count; i++)
			{
				ACp[i].Lay.assignNewMapChip(ACp[i], adding_in_playing ? (-2) : (-1), no_consider_config, sorting);
			}
		}

		public void activateToChip(int x, int y, int w, int h, bool deactivating = false, ulong layer_bits = 18446744073709551615UL, string meta_key = null)
		{
			if (this.AAPt == null)
			{
				global::XX.X.de("AAPt が指定されていない", null);
				return;
			}
			int num = x + w;
			int num2 = y + h;
			for (int i = x; i < num; i++)
			{
				if (global::XX.X.BTW(0f, (float)i, (float)this.clms))
				{
					for (int j = y; j < num2; j++)
					{
						if (global::XX.X.BTW(0f, (float)j, (float)this.rows))
						{
							M2Pt m2Pt = this.AAPt[i, j];
							if (m2Pt != null)
							{
								m2Pt.activateToChip(!deactivating, layer_bits, meta_key);
							}
						}
					}
				}
			}
		}

		public void fineSaveFlag()
		{
			int count_layers = this.count_layers;
			for (int i = 0; i < count_layers; i++)
			{
				this.ALay[i].fineSaveFlag();
			}
		}

		public GameObject createMoverGob<T>(string key, float mapx, float mapy, bool find_already_exist = false) where T : Object
		{
			GameObject gameObject = null;
			if (find_already_exist)
			{
				gameObject = GameObject.Find(this.gameObject.name + key);
				T t;
				if (gameObject != null && gameObject.TryGetComponent<T>(out t))
				{
					gameObject.transform.localPosition = new Vector3(this.map2ux(mapx), this.map2uy(mapy), 300f);
					return gameObject;
				}
			}
			if (gameObject == null)
			{
				gameObject = IN.CreateGob(this.gameObject, "-child");
				gameObject.name = key;
				gameObject.tag = "Mover";
			}
			Transform transform = gameObject.transform;
			transform.localScale = new Vector3(1f / this.mover_scale, 1f / this.mover_scale, 1f);
			transform.localPosition = new Vector3(this.map2ux(mapx), this.map2uy(mapy), 300f);
			return gameObject;
		}

		public T createMover<T>(string key, float mapx, float mapy, bool assign = false, bool find_already_exist = false) where T : M2Mover
		{
			T t = this.createMoverGob<T>(key, mapx, mapy, find_already_exist).AddComponent<T>();
			t.setTo(mapx, mapy);
			if (assign)
			{
				this.assignMover(t, false);
			}
			return t;
		}

		public M2Mover assignMover(M2Mover Mov, bool set_parent_gameobject = false)
		{
			if (Mov.Mp != null)
			{
				if (global::XX.X.isinC<M2Mover>(this.AMov, Mov, this.mover_count) >= 0)
				{
					global::XX.X.dl("重複して assign しようとしています: " + Mov.key, null, false, false);
					return Mov;
				}
				try
				{
					Mov.Mp.removeMover(Mov);
				}
				catch
				{
				}
			}
			global::XX.X.dl("assign: " + Mov.key, null, false, false);
			if ((this.update_mesh_flag_ & 2048) > 0)
			{
				this.initAction(true);
			}
			global::XX.X.pushToEmptyS<M2Mover>(ref this.AMov, Mov, ref this.mover_count, 16);
			Mov.appear(this);
			if (Mov is M2MoverPr)
			{
				if (this.MovP == null)
				{
					this.assignCenterPlayer(Mov as M2MoverPr);
				}
				global::XX.X.push<M2MoverPr>(ref this.AMovP, Mov as M2MoverPr, -1);
			}
			IN.setZ(Mov.transform, 300f);
			Mov.gameObject.SetActive(true);
			this.M2D.AssignPauseable(Mov);
			this.M2D.AssignPauseableP(Mov.getPhysic());
			if (set_parent_gameobject)
			{
				Mov.transform.SetParent(this.Gob.transform, false);
			}
			Rigidbody2D rigidbody = Mov.getRigidbody();
			if (rigidbody != null)
			{
				Map2d.PoserForTimeStop.Assign(rigidbody);
			}
			Mov.fineHittingLayer();
			return Mov;
		}

		public void assignCarryableBCC(M2BlockColliderContainer BCC)
		{
			if (BCC == null)
			{
				return;
			}
			if (this.ABcCon == null)
			{
				this.ABcCon = new List<M2BlockColliderContainer>(1);
			}
			this.ABcCon.Add(BCC);
		}

		public void assignPhysicsObject(M2Phys Phy)
		{
			this.APhysics.Add(Phy);
			if (this.OGob2Physics != null)
			{
				this.OGob2Physics[Phy.Mv.gameObject] = Phy;
			}
		}

		public void deassignPhysicsObject(M2Phys Phy)
		{
			try
			{
				this.APhysics.Remove(Phy);
				if (this.OGob2Physics != null)
				{
					this.OGob2Physics.Remove(Phy.Mv.gameObject);
				}
			}
			catch
			{
			}
		}

		public void removeCarryableBCC(M2BlockColliderContainer BCC)
		{
			if (this.ABcCon == null || BCC == null)
			{
				return;
			}
			this.ABcCon.Remove(BCC);
			this.recheckFootObject(BCC, null);
			if (this.ABcCon.Count == 0)
			{
				this.ABcCon = null;
			}
		}

		public void assignCenterPlayer(M2MoverPr Mov)
		{
			if (Mov == null)
			{
				this.M2D.Cam.resetBaseMoverIfFocusing(this.MovP, false);
				this.MovP = null;
				return;
			}
			this.MovP = Mov;
			this.M2D.Cam.assignBaseMover(this.MovP, -1);
			this.MovP.appearToCenterPr();
		}

		private void executeRemoveMoverStack(string bench_key)
		{
			int count = this.AMovRemoveStack.Count;
			if (count > 0)
			{
				Bench.P(bench_key);
				for (int i = 0; i < count; i++)
				{
					this.removeMover(this.AMovRemoveStack[i]);
				}
				this.AMovRemoveStack.Clear();
				Bench.Pend(bench_key);
			}
		}

		public bool removeMover(M2Mover Mov)
		{
			if (Mov == null)
			{
				return false;
			}
			bool flag = true;
			if (this.mover_running_i >= 0 && this.AMovRemoveStack != null)
			{
				this.AMovRemoveStack.Add(Mov);
				return false;
			}
			if (this.AMovP != null)
			{
				int num = global::XX.X.isinC<M2Mover>(this.AMov, Mov);
				if (this.mover_running_i >= 0 && num >= 0 && num <= this.mover_running_i)
				{
					this.mover_running_i--;
				}
				int num2 = this.mover_count;
				if (num >= 0)
				{
					global::XX.X.spliceEmpty<M2Mover>(this.AMov, num, 1);
					this.mover_count--;
				}
				M2Mover[] amovP = this.AMovP;
				int num3 = global::XX.X.isinC<M2Mover>(amovP, Mov);
				if (num3 >= 0)
				{
					global::XX.X.splice<M2MoverPr>(ref this.AMovP, num3, 1);
				}
				for (int i = this.pxlanim_count - 1; i >= 0; i--)
				{
					if (this.APxlAnim[i].get_Mv() == Mov)
					{
						try
						{
							this.APxlAnim[i].OnDestroy();
							IN.DestroyOne(this.APxlAnim[i].gameObject);
						}
						catch
						{
						}
						global::XX.X.shiftEmpty<M2PxlAnimator>(this.APxlAnim, 1, i, -1);
						this.pxlanim_count--;
					}
				}
				if (Mov == this.MovP)
				{
					this.assignCenterPlayer((this.AMovP.Length != 0) ? this.AMovP[0] : null);
				}
				Rigidbody2D rigidbody = Mov.getRigidbody();
				if (rigidbody != null)
				{
					Map2d.PoserForTimeStop.Deassign(rigidbody);
				}
				flag = num2 > this.mover_count;
			}
			if (Mov.do_not_destruct_when_remove)
			{
				Mov.deactivateFromMap();
				this.deassignPhysicsObject(Mov.getPhysic());
			}
			else
			{
				if (!Mov.destructed)
				{
					Mov.destruct();
				}
				Object gameObject = Mov.gameObject;
				IN.DestroyOne(Mov);
				IN.DestroyOne(gameObject);
				if (Mov is ITortureListener)
				{
					this.remTortureListener(Mov as ITortureListener);
				}
			}
			return flag;
		}

		public T createPxlAnimator<T>(M2Mover Mov, string chara_key, string pose_key = "stand", bool auto_start = false) where T : M2PxlAnimator
		{
			T t = IN.CreateGob(this.gameObject, "pxl_animator-" + Mov.key).AddComponent<T>();
			t.gameObject.layer = Mov.gameObject.layer;
			t.chara_title = chara_key;
			t.pose_title = pose_key;
			t.gameObject.layer = M2MovRenderContainer.drawer_t_layer;
			t.assignMover(Mov, auto_start);
			global::XX.X.pushToEmptyS<M2PxlAnimator>(ref this.APxlAnim, t, ref this.pxlanim_count, 16);
			return t;
		}

		public M2EventItem destructEvent(M2EventItem Ev)
		{
			if (this.EVC == null)
			{
				return null;
			}
			this.EVC.remove(Ev);
			if (this.TalkTarget_ is M2EventItem && Ev == this.TalkTarget_ as M2EventItem)
			{
				this.setTalkTarget(null, false);
			}
			if (this.AMovP != null)
			{
				int num = this.AMovP.Length;
				for (int i = 0; i < num; i++)
				{
					M2MoverPr m2MoverPr = this.AMovP[i];
					if (m2MoverPr == null)
					{
						break;
					}
					m2MoverPr.vanishLink(Ev);
				}
				this.removeMover(Ev);
			}
			return null;
		}

		public M2EventCommand executeLoadOnceCommand(M2EventItem Ev)
		{
			if (this.AEvLoadOnce == null)
			{
				this.AEvLoadOnce = new List<M2EventItem>(4);
			}
			if (Ev == null || this.AEvLoadOnce.IndexOf(Ev) != -1)
			{
				return null;
			}
			M2EventCommand m2EventCommand;
			if ((m2EventCommand = Ev.Get("load_once")) != null)
			{
				this.stackEventCommand(m2EventCommand, null);
				this.AEvLoadOnce.IndexOf(Ev);
				return m2EventCommand;
			}
			return null;
		}

		public M2EventCommand stackEventCommand(M2EventCommand C, IM2EvTrigger executed_by)
		{
			if (C != null)
			{
				if (executed_by != null)
				{
					C.stackExecuted(executed_by);
				}
				if (this.stack_to_map)
				{
					global::XX.X.pushToEmptyR<M2EventCommand>(ref this.AEvStack, C, 0);
				}
				else
				{
					C.seekReset();
					EV.stackReader(C, -1);
				}
			}
			return C;
		}

		public void commandReloadSpecific(string ev_key)
		{
			if (this.EVC != null)
			{
				this.EVC.reload(ev_key, false);
			}
		}

		public bool hasLight()
		{
			return this.ALight != null && this.light_cnt > 0;
		}

		public void addLight(M2Light Lig)
		{
			if (!this.isGameMode() || Lig == null)
			{
				return;
			}
			global::XX.X.pushToEmptyS<M2Light>(ref this.ALight, Lig, ref this.light_cnt, 4);
			if (this.light_cnt == 1 && this.mode == MAPMODE.NORMAL)
			{
				this.Unstb.FineAuto().Resume();
			}
		}

		public void remLight(M2Light Lig)
		{
			if (!this.isGameMode() || Lig == null)
			{
				return;
			}
			if (global::XX.X.emptySpecific<M2Light>(this.ALight, Lig, this.light_cnt))
			{
				this.light_cnt--;
			}
		}

		public void prepareTransferAfter()
		{
		}

		public bool run(float fcnt)
		{
			Map2d.TScur = fcnt;
			float ts = Map2d.TS;
			Bench.P("MapMain-Ev");
			if (this.t_load_cover < 0)
			{
				if (!EV.canProgress())
				{
					this.t_load_cover = -3;
				}
				else
				{
					this.t_load_cover++;
				}
			}
			else if (this.t_load_cover < 60)
			{
				this.t_load_cover++;
			}
			if ((this.update_mesh_flag_ & 2048) > 0)
			{
				this.initAction(true);
			}
			if (this.EVC != null)
			{
				Bench.P("EVC run");
				if ((this.cmd_reload_flg & 16U) != 0U)
				{
					this.cmd_reload_flg &= 4294967279U;
					this.initLPCommand();
				}
			}
			if (this.need_update_collider && this.mode == MAPMODE.NORMAL)
			{
				Bench.P("Make Collider");
				this.need_update_collider = false;
				if (this.MyCollider != null)
				{
					this.makeCollider(this.MyCollider);
				}
				Bench.Pend("Make Collider");
			}
			if (this.EVC != null)
			{
				if (this.cmd_reload_flg != 0U)
				{
					if ((!EV.isActive(false) && (this.cmd_reload_flg & 1U) != 0U) || (this.cmd_reload_flg & 2U) != 0U)
					{
						uint num = this.cmd_reload_flg;
						this.cmd_reload_flg &= 4294967288U;
						this.EVC.reload("", (num & 4U) > 0U);
						if (this.MovP != null)
						{
							this.MovP.need_check_event = true;
						}
					}
					else if ((this.cmd_reload_flg & 8U) != 0U)
					{
						this.cmd_reload_flg &= 4294967287U;
						this.EVC.destructReservedCheck();
					}
				}
				if ((this.cmd_loadevent_execute & 8U) == 0U && (this.cmd_loadevent_execute & 2U) != 0U)
				{
					this.EVC.runLoadExecuteNotAssinged();
				}
				Bench.Pend("EVC run");
			}
			if (EV.isStoppingGame())
			{
				Bench.Pend("MapMain-Ev");
				this.M2D.stopping_game = true;
				return false;
			}
			this.M2D.stopping_game = false;
			this.M2D.pre_map_active = true;
			Map2d.can_handle = !EV.isStoppingGameHandle();
			Bench.Pend("MapMain-Ev");
			Bench.P("TS running");
			if (ts > 0f)
			{
				Bench.P("Mv runPre - ");
				this.mover_running_i = 0;
				while (this.mover_running_i < this.mover_count)
				{
					M2Mover m2Mover = this.AMov[this.mover_running_i];
					Bench.P(m2Mover.key);
					m2Mover.runPre();
					Bench.Pend(m2Mover.key);
					this.mover_running_i++;
				}
				Bench.Pend("Mv runPre - ");
				this.mover_running_i = -1;
				this.executeRemoveMoverStack("pre_remove Mv");
				Bench.P("PxlAnim");
				for (int i = 0; i < this.pxlanim_count; i++)
				{
					this.APxlAnim[i].runPre(1f);
				}
				Bench.Pend("PxlAnim");
				if (this.ARunningObject != null)
				{
					Bench.P("Runner - ");
					for (int j = this.ARunningObject.Count - 1; j >= 0; j--)
					{
						IRunAndDestroy runAndDestroy = this.ARunningObject[j];
						string text = Bench.P(runAndDestroy.ToString());
						if (!runAndDestroy.run(ts))
						{
							this.ARunningObject.RemoveAt(j);
						}
						Bench.Pend(text);
					}
					Bench.Pend("Runner - ");
				}
				Bench.P("CameraRun");
				this.M2D.Cam.run(false);
				Bench.Pend("CameraRun");
				Bench.P("Mv runPost - ");
				this.mover_running_i = 0;
				while (this.mover_running_i < this.mover_count)
				{
					M2Mover m2Mover2 = this.AMov[this.mover_running_i];
					Bench.P(m2Mover2.key);
					this.AMov[this.mover_running_i].runPost();
					Bench.Pend(m2Mover2.key);
					this.mover_running_i++;
				}
				this.mover_running_i = -1;
				Bench.Pend("Mv runPost - ");
				this.executeRemoveMoverStack("post_remove Mv");
				Bench.P("DropCon");
				this.DropCon.run(ts);
				Bench.Pend("DropCon");
				Bench.P("Torture");
				if (this.ATortureListener != null)
				{
					for (int k = this.ATortureListener.Count - 1; k >= 0; k--)
					{
						this.ATortureListener[k].runPostTorture();
					}
				}
				Bench.Pend("Torture");
			}
			else if (this.Pr != null)
			{
				this.Pr.runInFloorPausing();
			}
			Bench.Pend("TS running");
			Bench.P("RunEf");
			this.runEffect();
			Bench.Pend("RunEf");
			Bench.P("PreDraw");
			if (global::XX.X.D && this.mode == MAPMODE.NORMAL)
			{
				Map2d.TScur = 1f;
				float num2 = (float)global::XX.X.AF * Map2d.TSbase;
				int num3 = this.ASubMaps.Length;
				for (int l = 0; l < num3; l++)
				{
					M2SubMap m2SubMap = this.ASubMaps[l];
					m2SubMap.runEffect(num2);
					m2SubMap.getTargetMap().drawCheck(num2);
				}
				Map2d.TScur = fcnt;
			}
			Bench.Pend("PreDraw");
			this.floort += ts;
			return true;
		}

		public bool runUi()
		{
			bool flag = false;
			if (this.AMovP != null)
			{
				int count_players = this.count_players;
				for (int i = 0; i < count_players; i++)
				{
					flag = this.AMovP[i].runUi() || flag;
				}
			}
			return flag;
		}

		public void drawLights(MeshDrawer MyDrawerLight, M2SubMap Sm, float alpha_smp, float fcnt, bool auto_blit = false)
		{
			Bench.P("LightDraw");
			for (int i = 0; i < this.light_cnt; i++)
			{
				this.ALight[i].drawLight(MyDrawerLight, Sm, alpha_smp, fcnt);
				if (auto_blit)
				{
					BLIT.RenderToGLOneTask(MyDrawerLight, -1, false);
				}
			}
			Bench.Pend("LightDraw");
			MyDrawerLight.updateForMeshRenderer(true);
		}

		public bool runPost()
		{
			if ((this.update_mesh_flag_ & 2048) == 0 && this.need_update_collider && this.mode == MAPMODE.NORMAL)
			{
				this.need_update_collider = false;
				if (this.MyCollider != null)
				{
					this.makeCollider(this.MyCollider);
				}
			}
			return this.M2D.runui_enabled && this.runUi();
		}

		public void runPhysics(float fcnt)
		{
			if (Map2d.TS == 0f)
			{
				return;
			}
			M2Phys.fixed_updating = true;
			for (int i = this.APhysics.Count - 1; i >= 0; i--)
			{
				M2Phys m2Phys = this.APhysics[i];
				if (!m2Phys.get_is_pause())
				{
					Bench.P(m2Phys.Mv.key);
					m2Phys.Mv.runPhysics(fcnt);
					Bench.Pend(m2Phys.Mv.key);
				}
			}
			M2Phys.fixed_updating = false;
		}

		public void runEffect()
		{
			if (this.EF != null)
			{
				this.EF.runDrawOrRedrawMesh(global::XX.X.D_EF, (float)global::XX.X.AF_EF, Map2d.TS);
			}
			if (this.EFT != null)
			{
				this.EFT.runDrawOrRedrawMesh(global::XX.X.D_EF, (float)global::XX.X.AF_EF, Map2d.TS);
			}
			if (this.EFC != null)
			{
				this.EFC.runDrawOrRedrawMesh(global::XX.X.D, (float)global::XX.X.AF, Map2d.TS);
			}
			if (global::XX.X.D_EF)
			{
				if (this.EFD != null)
				{
					this.EFD.run((float)global::XX.X.AF_EF * Map2d.TSbase);
				}
				if (this.EFDT != null)
				{
					this.EFDT.run((float)global::XX.X.AF_EF * Map2d.TSbase);
				}
			}
			if (global::XX.X.D && this.EFDC != null)
			{
				this.EFDC.run((float)global::XX.X.AF * Map2d.TSbase);
			}
		}

		public static void setTimeScale(float v, bool force = false)
		{
			if (!force && v == Map2d.TSbase)
			{
				return;
			}
			if (Map2d.TSbase == 0f)
			{
				Map2d.PoserForTimeStop.Resume();
			}
			else if (v == 0f)
			{
				Map2d.PoserForTimeStop.Pause();
			}
			Map2d.TSbasepr = v;
			Map2d.TSbase = v;
		}

		private void initActionPre()
		{
			if (Map2d.editor_decline_lighting)
			{
				return;
			}
			this.Gob.isStatic = this.mode == MAPMODE.NORMAL || Map2d.isTempMode(this.mode);
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].initActionPre(this.mode == MAPMODE.NORMAL);
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				int num2 = this.ASubMaps.Length;
				for (int j = 0; j < num2; j++)
				{
					Map2d targetMap = this.ASubMaps[j].getTargetMap();
					if ((targetMap.update_mesh_flag_ & 2048) > 0)
					{
						targetMap.initActionPre();
					}
				}
			}
		}

		private Map2d initAction(bool do_init_action_pre = true)
		{
			if (Map2d.editor_decline_lighting)
			{
				return this;
			}
			if (this.need_reentry_flag_)
			{
				this.drawCheck(0f);
				return this;
			}
			this.update_mesh_flag_ &= -2049;
			if (this.mode != MAPMODE.NORMAL && this.mode != MAPMODE.SUBMAP)
			{
				return this;
			}
			if (do_init_action_pre)
			{
				this.initActionPre();
			}
			int num = this.ALay.Length;
			this.M2D.initChipEffect(this, this.SubMapData, ref this.EFC, ref this.EFDC);
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].initActionLP(this.mode == MAPMODE.NORMAL);
			}
			for (int j = 0; j < num; j++)
			{
				this.ALay[j].initAction(this.mode == MAPMODE.NORMAL);
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				int num2 = this.ASubMaps.Length;
				for (int k = 0; k < num2; k++)
				{
					Map2d targetMap = this.ASubMaps[k].getTargetMap();
					if (targetMap != this)
					{
						targetMap.addUpdateMesh(2048, false);
					}
				}
				if (do_init_action_pre)
				{
					for (int l = 0; l < num2; l++)
					{
						Map2d targetMap2 = this.ASubMaps[l].getTargetMap();
						if ((targetMap2.update_mesh_flag_ & 2048) > 0)
						{
							targetMap2.initAction(true);
						}
					}
				}
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				this.M2D.mapActionInitted(this);
			}
			return this;
		}

		public void releaseSimplifiedTexture()
		{
			this.MMRD.clearRendered();
			this.need_reentry_flag = true;
			if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP)
			{
				int num = this.ASubMaps.Length;
				for (int i = 0; i < num; i++)
				{
					this.ASubMaps[i].releaseSimplifiedTexture();
				}
			}
		}

		public Map2d closeAction(bool no_set_init_flag = false, bool when_map_close = false, bool no_remove_chip_drawer = false)
		{
			if (this.mode != MAPMODE.NORMAL && this.mode != MAPMODE.SUBMAP)
			{
				return this;
			}
			if (!no_set_init_flag)
			{
				this.need_reentry_flag = true;
				this.update_mesh_flag_ |= 2048;
			}
			if (this.NM != null)
			{
				this.NM.clear();
			}
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				if (no_remove_chip_drawer)
				{
					this.ALay[i].closeActionWithoutRemoveDrawer(when_map_close);
				}
				else
				{
					this.ALay[i].closeAction(when_map_close);
				}
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				int num2 = this.ASubMaps.Length;
				string text = "\n" + this.key + "\n";
				for (int j = 0; j < num2; j++)
				{
					Map2d targetMap = this.ASubMaps[j].getTargetMap();
					if (targetMap != this && text.IndexOf("\n" + targetMap.key + "\n") == -1)
					{
						text = text + targetMap.key + "\n";
						targetMap.closeAction(no_set_init_flag, when_map_close, true);
					}
				}
			}
			else if (!no_set_init_flag && this.MyDrawerG != null)
			{
				this.drawCheck(0f);
			}
			if (this.EFC != null)
			{
				MultiMeshRenderer mmrdforMeshDrawerContainer = this.EFC.getMMRDForMeshDrawerContainer();
				if (mmrdforMeshDrawerContainer != null)
				{
					mmrdforMeshDrawerContainer.Clear(0);
				}
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				this.M2D.mapActionClosed(this);
				if (this.EVC != null)
				{
					this.EVC.executeClearingRemoveObject();
				}
			}
			return this;
		}

		public void addEfDrawer()
		{
		}

		public void addRedrawingChip(M2Puts Cp)
		{
			if (global::XX.X.pushToEmptyRI<M2Puts>(ref this.ARedrawingChip, Cp, this.redrawing_chip_count))
			{
				this.redrawing_chip_count++;
			}
		}

		public Map2d drawCheck(float fcnt)
		{
			bool flag = false;
			bool flag2 = true;
			bool flag3 = false;
			bool flag4;
			if (this.need_reentry_flag_)
			{
				this.need_reentry_flag_ = false;
				if (!Map2d.editor_decline_lighting && (this.update_mesh_flag_ & 2048) > 0)
				{
					this.initActionPre();
					flag2 = false;
				}
				if (this.need_reentry_gradation_flag_)
				{
					this.clearGradation();
					flag = true;
					this.update_mesh_flag_ |= 16384;
				}
				this.reentryAllChips();
				if (Map2d.editor_decline_lighting)
				{
					this.update_mesh_flag_ |= 16384;
				}
				flag4 = true;
			}
			else
			{
				flag4 = fcnt != 0f;
			}
			if (fcnt < 0f)
			{
				fcnt = 0f;
			}
			if (!Map2d.editor_decline_lighting && (this.update_mesh_flag_ & 2048) > 0)
			{
				this.initAction(flag2);
			}
			if (flag4 && this.redrawing_chip_count > 0)
			{
				Bench.P("ChipRedraw");
				for (int i = 0; i < this.redrawing_chip_count; i++)
				{
					M2Puts m2Puts = this.ARedrawingChip[i];
					if (m2Puts.isinCamera(0f))
					{
						this.update_mesh_flag_ |= m2Puts.redraw(fcnt);
					}
				}
				Bench.Pend("ChipRedraw");
			}
			if (this.need_reentry_gradation_flag_)
			{
				this.reentryGradation(!flag);
			}
			if (fcnt > 0f)
			{
				this.update_mesh_flag_ |= this.Dgn.drawCheck(fcnt);
			}
			if (this.update_mesh_flag_ > 0)
			{
				if ((this.update_mesh_flag_ & 16384) != 0)
				{
					this.MMRD.fineActivateState(this.SubMapData, this.SubMapData != null);
					flag3 = true;
				}
				if ((this.update_mesh_flag_ & 8) > 0 && this.MyDrawerUCol != null)
				{
					this.MyDrawerUCol.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 1) > 0)
				{
					this.MyDrawerB.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 2) > 0)
				{
					this.MyDrawerG.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 4) > 0)
				{
					this.MyDrawerT.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 512) > 0)
				{
					this.MyDrawerL.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 8192) > 0)
				{
					this.MyDrawerTT.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 16) > 0)
				{
					this.MyDrawerUGrd.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 128) > 0)
				{
					this.MyDrawerTGrd.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 64) > 0)
				{
					this.MyDrawerGGrd.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 32) > 0)
				{
					this.MyDrawerBGrd.updateForMeshRenderer(true);
				}
				if ((this.update_mesh_flag_ & 1024) > 0 && this.MyDrawerWater != null)
				{
					this.MyDrawerWater.updateForMeshRenderer(true);
					if (this.is_submap)
					{
						this.MMRD.getIndex(this.MyDrawerWater);
					}
				}
				if ((this.update_mesh_flag_ & 4096) > 0 && this.BlurDraw != null)
				{
					this.BlurDraw.updateAll(this);
				}
				this.update_mesh_flag_ = 0;
				if (flag3)
				{
					if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP)
					{
						for (int j = 0; j < this.count_submaps; j++)
						{
							this.ASubMaps[j].getTargetMap().drawCheck(0f);
						}
						this.fineSubMap();
						for (int k = 0; k < this.count_submaps; k++)
						{
							this.ASubMaps[k].fineMeshAndMaterials(false, false);
						}
						if (this.mode == MAPMODE.NORMAL && !Map2d.editor_decline_lighting)
						{
							if (this.Unstb == null)
							{
								this.Unstb = new M2UnstabilizeMapItem(this, null);
							}
							this.Unstb.FineAuto().Resume();
						}
					}
				}
				else
				{
					this.fineSubMap();
				}
			}
			else
			{
				this.fineSubMap();
			}
			return this;
		}

		public void unshiftSpecificMeshDrawer(MeshDrawer Md, int ver, int tri)
		{
			for (int i = 0; i < this.count_layers; i++)
			{
				this.ALay[i].unshiftSpecificMeshDrawer(Md, ver, tri);
			}
		}

		public void recheckBCCCache(M2BlockColliderContainer BCC = null)
		{
			if (BCC == null)
			{
				BCC = this.BCC;
			}
			for (int i = this.APhysics.Count - 1; i >= 0; i--)
			{
				M2FootManager footManager = this.APhysics[i].getFootManager();
				if (footManager != null)
				{
					M2BlockColliderContainer.BCCLine bccline = footManager.get_Foot() as M2BlockColliderContainer.BCCLine;
					if (BCC == this.BCC || (bccline != null && bccline.BCC == BCC))
					{
						footManager.need_recheck_bcc_cache = true;
					}
				}
			}
		}

		public void recheckBCCCurrentPos(M2BlockColliderContainer BCC = null)
		{
			if (BCC == null)
			{
				BCC = this.BCC;
			}
			for (int i = this.APhysics.Count - 1; i >= 0; i--)
			{
				M2FootManager footManager = this.APhysics[i].getFootManager();
				if (footManager != null)
				{
					M2BlockColliderContainer.BCCLine bccline = footManager.get_Foot() as M2BlockColliderContainer.BCCLine;
					if (BCC == this.BCC || (bccline != null && bccline.BCC == BCC))
					{
						footManager.need_recheck_current_pos = true;
					}
				}
			}
		}

		public void recheckFootObject(M2BlockColliderContainer BCC, List<M2BlockColliderContainer.BCCInfo> ACacheFoot = null)
		{
			if (BCC == null)
			{
				return;
			}
			for (int i = this.APhysics.Count - 1; i >= 0; i--)
			{
				M2FootManager footManager = this.APhysics[i].getFootManager();
				if (footManager != null)
				{
					if (BCC == this.BCC)
					{
						footManager.need_recheck_bcc_cache = true;
					}
					M2BlockColliderContainer.BCCLine bccline = footManager.get_Foot() as M2BlockColliderContainer.BCCLine;
					if (bccline != null && bccline.BCC == BCC)
					{
						M2BlockColliderContainer.BCCLine bccline2 = ((ACacheFoot == null) ? BCC.getSameLine(bccline, false, false) : BCC.getSameLine(ACacheFoot[i], false, false));
						if (bccline2 != null)
						{
							footManager.rideInitTo(bccline2, false);
						}
						else
						{
							footManager.initJump(true, true, false);
						}
					}
				}
			}
			if (BCC == this.BCC)
			{
				this.DropCon.colliderFined();
			}
		}

		public void setComment(string s, M2SubMap SubMapData, bool from_editor = false)
		{
			this.comment = s;
			this.Meta = new META(s);
			this.crop_ = -1;
			if (this.MMRD != null)
			{
				if (this.mode == MAPMODE.NORMAL || this.isTempMode())
				{
					this.Dgn = null;
					this.prepareMeshDrawer(SubMapData, ref this.MMRD);
				}
			}
		}

		public Map2d open(GameObject GobBase, MAPMODE _mode = MAPMODE.NORMAL, M2SubMap SubMapData = null)
		{
			this.prepared = true;
			if (_mode == MAPMODE.CLOSED)
			{
				if (this.prepared && !this.loaded)
				{
					this.load(false);
				}
				return this;
			}
			this.SubMapData = ((_mode == MAPMODE.SUBMAP) ? SubMapData : null);
			this.light_cnt = 0;
			this.mode = _mode;
			this.Gob = IN.CreateGob(GobBase, "-" + this.key);
			this.Gob.isStatic = false;
			this.Gob.layer = this.M2D.map_object_layer;
			this.AMovP = new M2MoverPr[0];
			this.Dgn = ((SubMapData != null) ? SubMapData.getBaseDgn() : null);
			if (_mode != MAPMODE.TEMP_WHOLE)
			{
				this.prepareMeshDrawer(SubMapData, ref this.MMRD);
			}
			if (_mode == MAPMODE.NORMAL)
			{
				this.Dgn.initS(this);
				Map2d.TScur = 0f;
				this.OGob_tags = new BDic<GameObject, string>(8);
				if (Map2d.PoserForTimeStop == null)
				{
					Map2d.PoserForTimeStop = new PAUSER();
				}
				else
				{
					Map2d.PoserForTimeStop.Clear();
				}
				Map2d.setTimeScale(Map2d.TSbase, true);
				Map2d.TScur = 1f;
				if (this.Unstb == null)
				{
					this.Unstb = new M2UnstabilizeMapItem(this, null);
				}
				else
				{
					this.Unstb.openMap();
				}
				this.APhysics = new List<M2Phys>(2);
				this.AMovRemoveStack = new List<M2Mover>(1);
				this.OGob2Physics = new BDic<GameObject, M2Phys>(2);
				if (this.M2D.curMap != this)
				{
					this.M2D.initMapMaterialASync(this, 0, false);
				}
				if (this.EVC == null)
				{
					this.M2D.readMapEventContent(this, false);
				}
				this.cmd_loadevent_execute = 0U;
			}
			else
			{
				if (this.Unstb != null)
				{
					this.Unstb = this.Unstb.destruct(false);
				}
				this.EVC = null;
			}
			if (this.isGameMode())
			{
				this.ALight = new M2Light[4];
			}
			bool flag = true;
			int num;
			if (this.ALay == null || this.clms == 0 || this.rows == 0)
			{
				this.load(false);
				flag = false;
				num = this.ALay.Length;
			}
			else
			{
				this.ARunningObject = null;
				num = this.ALay.Length;
				if (this.AAPt == null && (_mode == MAPMODE.NORMAL || _mode == MAPMODE.TEMP))
				{
					this.reconnectWholeChips();
				}
			}
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].reopen(flag);
			}
			this.considerConfig4(0, 0, this.clms, this.rows);
			this.redrawing_chip_count = 0;
			this.mover_running_i = -1;
			this.ARedrawingChip = null;
			this.AActivatable = null;
			this.TalkTarget_ = null;
			this.ATortureListener = null;
			if (this.EVC != null)
			{
				this.EVC.clear(false);
			}
			if (this.mode == MAPMODE.NORMAL)
			{
				M2CImgDrawerWhole.fine_flag = true;
			}
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].open();
			}
			this.handle = true;
			this.stack_to_map = false;
			this.t_load_cover = ((this.mode != MAPMODE.NORMAL) ? 60 : (-3));
			this.load_cover_col = 16777215U;
			this.bgcol = this.bgcol0;
			if (this.mode == MAPMODE.NORMAL)
			{
				this.cmd_reload_flg = 19U;
				this.M2D.Cam.init(this);
				this.floort = 0f;
				this.DropCon = new M2DropObjectContainer(this);
				this.NM = new NearManager(this);
				this.DmgCntCon = new M2DmgCounterContainer(this);
				this.initEffect(SubMapData);
				this.update_mesh_flag_ |= 16384;
				this.BCC = this.M2D.BufferBCC ?? new M2BlockColliderContainer(this, null);
				if (this.M2D.BufferBCC == this.BCC)
				{
					this.BCC.InitS(this);
					this.M2D.BufferBCC = null;
				}
			}
			else if (this.mode == MAPMODE.SUBMAP)
			{
				this.initEffect(SubMapData);
			}
			if (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.SUBMAP)
			{
				this.drawUCol();
			}
			this.openSubMaps();
			if (this.mode == MAPMODE.NORMAL)
			{
				this.MyCollider = IN.GetOrAdd<PolygonCollider2D>(this.Gob);
				this.MyCollider.enabled = false;
				this.need_update_collider = true;
			}
			if (this.mode == MAPMODE.TEMP_WHOLE)
			{
				this.Gob.SetActive(false);
			}
			else
			{
				this.need_reentry_flag_ = (this.need_reentry_gradation_flag_ = true);
				this.update_mesh_flag_ = 26367 | (this.isGameMode() ? 2048 : 0);
			}
			return this;
		}

		private void allLayerReopen()
		{
			int count_layers = this.count_layers;
			for (int i = 0; i < count_layers; i++)
			{
				this.ALay[i].reopen(true);
			}
		}

		public void allLayerFineCheckFlag(bool check_submap = true)
		{
			int count_layers = this.count_layers;
			for (int i = 0; i < count_layers; i++)
			{
				this.ALay[i].fineCheckFlag();
			}
			if (this.mode != MAPMODE.SUBMAP && this.ASubMaps != null && check_submap)
			{
				int num = this.ASubMaps.Length;
				for (int j = 0; j < num; j++)
				{
					if (this.ASubMaps[j].getTargetMap() != this)
					{
						this.allLayerFineCheckFlag(false);
					}
				}
			}
		}

		public void prepareMeshDrawer(M2SubMap SubMapData, ref M2MeshContainer MMRD)
		{
			this.releaseBlurDraw();
			Map2d.prepareMeshDrawer(this.Gob, this, this.mode, SubMapData, ref MMRD, ref this.MyDrawerUCol, ref this.MyDrawerB, ref this.MyDrawerG, ref this.MyDrawerT, ref this.MyDrawerL, ref this.MyDrawerTT, ref this.MyDrawerUGrd, ref this.MyDrawerBGrd, ref this.MyDrawerGGrd, ref this.MyDrawerTGrd);
		}

		private static void prepareMeshDrawer(GameObject Gob, Map2d Mp, MAPMODE mode, M2SubMap SubMapData, ref M2MeshContainer MMRD, ref MdMap MyDrawerUCol, ref MdMap MyDrawerB, ref MdMap MyDrawerG, ref MdMap MyDrawerT, ref MdMap MyDrawerL, ref MdMap MyDrawerTT, ref MdMap MyDrawerUGrd, ref MdMap MyDrawerBGrd, ref MdMap MyDrawerGGrd, ref MdMap MyDrawerTGrd)
		{
			if (Mp.Dgn != null && Mp.Dgn.key == "_editor" != Map2d.editor_decline_lighting)
			{
				Mp.Dgn = null;
			}
			bool flag = false;
			if (Gob == null)
			{
				Gob = IN.CreateGob(Mp.M2D.GobBase, "-" + Mp.key);
				Gob.layer = Mp.M2D.map_object_layer;
				if (Mp.Gob == null)
				{
					Mp.Gob = Gob;
				}
			}
			if (Mp.Dgn == null)
			{
				Mp.Dgn = (Map2d.editor_decline_lighting ? Mp.M2D.getDgnByKey("_editor") : Mp.M2D.getDgn(Mp));
				if (Mp.M2D.Cam.CurDgn != Mp.Dgn)
				{
					Mp.releaseMeshLink(false, false);
					if (mode == MAPMODE.NORMAL || Map2d.isTempMode(mode))
					{
						Mp.M2D.closeCamera();
						Mp.M2D.Cam.CurDgn = Mp.Dgn;
						Mp.Dgn.initMap(Mp, SubMapData).initCamera(Mp.M2D.Cam);
						flag = true;
					}
				}
			}
			Dungeon dgn = Mp.Dgn;
			dgn.initMapFinalize(Mp, SubMapData);
			M2MeshContainer.prepareMeshContainer(Gob, Mp, SubMapData, ref MMRD, Map2d.editor_decline_lighting, Mp.M2D);
			bool flag2 = MyDrawerB != null;
			Material material = Mp.Dgn.getChipMaterial(1, null);
			float num = ((SubMapData != null) ? SubMapData.base_z : 0f);
			if (material != null)
			{
				float num2;
				MMRD.CreateMesh(ref MyDrawerB, "-B", num2 = dgn.getDrawZ(mode, 0), dgn.getChipMaterial(0, null), dgn.getLayerForChip(0, Dungeon.MESHTYPE.CHIP), false, dgn.canBakeSimplify(false, 0), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerG, "-G", num2 = dgn.getDrawZ(mode, 1), material, dgn.getLayerForChip(1, Dungeon.MESHTYPE.CHIP), false, dgn.canBakeSimplify(false, 1), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerT, "-T", num2 = dgn.getDrawZ(mode, 2), dgn.getChipMaterial(2, null), dgn.getLayerForChip(2, Dungeon.MESHTYPE.CHIP), false, dgn.canBakeSimplify(false, 2), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerL, "-L", num2 = dgn.getDrawZ(mode, 3), dgn.getChipMaterial(3, null), dgn.getLayerForChip(3, Dungeon.MESHTYPE.CHIP), false, dgn.canBakeSimplify(false, 3), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerTT, "-TT", num2 = dgn.getDrawZ(mode, 4), dgn.getChipMaterial(4, null), dgn.getLayerForChip(4, Dungeon.MESHTYPE.CHIP), false, dgn.canBakeSimplify(false, 4), num2 + num > 300f, false);
			}
			else
			{
				MMRD.Remove(ref MyDrawerB);
				MMRD.Remove(ref MyDrawerG);
				MMRD.Remove(ref MyDrawerT);
				MMRD.Remove(ref MyDrawerTT);
				MMRD.Remove(ref MyDrawerL);
			}
			material = dgn.getGradationMaterial(-2, null);
			if (material != null)
			{
				float num2;
				MMRD.CreateMesh(ref MyDrawerUGrd, "-UGrd", num2 = dgn.getDrawZ(mode, 19), dgn.getGradationMaterial(-1, null), dgn.getLayerForChip(-1, Dungeon.MESHTYPE.GRADATION), false, dgn.canBakeSimplify(true, -1), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerBGrd, "-BGrd", num2 = dgn.getDrawZ(mode, 20), dgn.getGradationMaterial(0, null), dgn.getLayerForChip(0, Dungeon.MESHTYPE.GRADATION), false, dgn.canBakeSimplify(true, 0), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerGGrd, "-GGrd", num2 = dgn.getDrawZ(mode, 21), dgn.getGradationMaterial(1, null), dgn.getLayerForChip(1, Dungeon.MESHTYPE.GRADATION), false, dgn.canBakeSimplify(true, 1), num2 + num > 300f, false);
				MMRD.CreateMesh(ref MyDrawerTGrd, "-TGrd", num2 = dgn.getDrawZ(mode, 22), dgn.getGradationMaterial(2, null), dgn.getLayerForChip(2, Dungeon.MESHTYPE.GRADATION), false, dgn.canBakeSimplify(true, 2), num2 + num > 300f, false);
			}
			else
			{
				MMRD.Remove(ref MyDrawerUGrd);
				MMRD.Remove(ref MyDrawerBGrd);
				MMRD.Remove(ref MyDrawerGGrd);
				MMRD.Remove(ref MyDrawerTGrd);
			}
			if (Mp.MyDrawerWater != null && Mp.gameObject == Gob)
			{
				material = dgn.getWaterMaterial();
				if (material != Mp.MyDrawerWater.getMaterial())
				{
					MMRD.Remove(ref Mp.MyDrawerWater);
				}
			}
			Material material2;
			if (dgn.useBgColLayer(Mp, out material2))
			{
				MMRD.CreateMesh(ref MyDrawerUCol, "-UCol", 900f, material2, Mp.Dgn.getLayerForBgColor(), false, false, true, false);
			}
			else
			{
				MMRD.Remove(ref MyDrawerUCol);
			}
			if (mode == MAPMODE.NORMAL)
			{
				Mp.need_reentry_flag = (Mp.need_reentry_gradation_flag = true);
				if (flag2)
				{
					Mp.clearChipsDrawer(false);
				}
			}
			if (Mp.Meta.GetB("reverse_mesh_G", false))
			{
				MyDrawerG.reverse_mesh_simplify = true;
			}
			if (flag)
			{
				Mp.M2D.initCameraAfter(Mp);
				Mp.Dgn.fineCameraBgColor();
			}
			if (!Map2d.editor_decline_lighting && mode == MAPMODE.NORMAL)
			{
				Mp.M2D.Cam.run(true);
			}
		}

		public DRect GetRcPreDefined(int i, bool no_make = false)
		{
			if (this.ARcPreDefined == null)
			{
				if (no_make)
				{
					return null;
				}
				this.ARcPreDefined = new DRect[i + 1];
			}
			if (this.ARcPreDefined.Length <= i)
			{
				if (no_make)
				{
					return null;
				}
				Array.Resize<DRect>(ref this.ARcPreDefined, i + 1);
			}
			DRect drect = this.ARcPreDefined[i];
			if (drect == null)
			{
				if (no_make)
				{
					return null;
				}
				drect = (this.ARcPreDefined[i] = new DRect(""));
			}
			return drect;
		}

		public int fineMaterialLayer(MeshDrawer Md)
		{
			if (Md == this.MyDrawerB)
			{
				return this.Dgn.getLayerForChip(0, Dungeon.MESHTYPE.CHIP);
			}
			if (Md == this.MyDrawerG)
			{
				return this.Dgn.getLayerForChip(1, Dungeon.MESHTYPE.CHIP);
			}
			if (Md == this.MyDrawerT)
			{
				return this.Dgn.getLayerForChip(2, Dungeon.MESHTYPE.CHIP);
			}
			if (Md == this.MyDrawerL)
			{
				return this.Dgn.getLayerForChip(3, Dungeon.MESHTYPE.CHIP);
			}
			if (Md == this.MyDrawerTT)
			{
				return this.Dgn.getLayerForChip(4, Dungeon.MESHTYPE.CHIP);
			}
			if (Md == this.MyDrawerUGrd)
			{
				return this.Dgn.getLayerForChip(-1, Dungeon.MESHTYPE.GRADATION);
			}
			if (Md == this.MyDrawerBGrd)
			{
				return this.Dgn.getLayerForChip(0, Dungeon.MESHTYPE.GRADATION);
			}
			if (Md == this.MyDrawerGGrd)
			{
				return this.Dgn.getLayerForChip(1, Dungeon.MESHTYPE.GRADATION);
			}
			if (Md == this.MyDrawerTGrd)
			{
				return this.Dgn.getLayerForChip(2, Dungeon.MESHTYPE.GRADATION);
			}
			GameObject gob = this.MMRD.GetGob(Md);
			if (!(gob != null))
			{
				return this.Dgn.effect_layer_top;
			}
			return gob.layer;
		}

		public Material fineMaterialSubMapAttribute(MeshDrawer Md, Material Mtr, M2SubMap CheckSub, bool first = false)
		{
			if (Md == null)
			{
				return null;
			}
			Material material = Mtr;
			if (first)
			{
				Mtr = null;
			}
			if (Md == this.MyDrawerB)
			{
				Mtr = this.Dgn.getChipMaterial(0, Mtr);
			}
			else if (Md == this.MyDrawerG)
			{
				Mtr = this.Dgn.getChipMaterial(1, Mtr);
			}
			else if (Md == this.MyDrawerT)
			{
				Mtr = this.Dgn.getChipMaterial(2, Mtr);
			}
			else if (Md == this.MyDrawerL)
			{
				Mtr = this.Dgn.getChipMaterial(3, Mtr);
			}
			else if (Md == this.MyDrawerTT)
			{
				Mtr = this.Dgn.getChipMaterial(4, Mtr);
			}
			else if (Md == this.MyDrawerUGrd)
			{
				Mtr = this.Dgn.getGradationMaterial(-1, Mtr);
			}
			else if (Md == this.MyDrawerBGrd)
			{
				Mtr = this.Dgn.getGradationMaterial(0, Mtr);
			}
			else if (Md == this.MyDrawerGGrd)
			{
				Mtr = this.Dgn.getGradationMaterial(1, Mtr);
			}
			else if (Md == this.MyDrawerTGrd)
			{
				Mtr = this.Dgn.getGradationMaterial(2, Mtr);
			}
			else
			{
				Mtr = material;
			}
			if (first && Mtr != material && Mtr != null)
			{
				try
				{
					Mtr.mainTexture = ((material.mainTexture != null) ? material.mainTexture : this.M2D.MIchip.Tx);
				}
				catch
				{
				}
			}
			return Mtr;
		}

		public void fineMaterialNightColor(Dungeon Dgn = null, M2SubMap CheckSub = null)
		{
			if ((this.mode != MAPMODE.NORMAL && this.mode != MAPMODE.SUBMAP) || this.MyDrawerL == null)
			{
				return;
			}
			if (Dgn == null)
			{
				Dgn = ((CheckSub != null) ? CheckSub.getBaseDgn() : this.Dgn);
			}
			Dgn.initMap(this, CheckSub);
			if (CheckSub == null)
			{
				Dgn.resetColor();
				Dgn.fineMaterialColor();
			}
			if (this.MMRD != null)
			{
				if (CheckSub == null)
				{
					this.MMRD.fineMaterialNightColor(CheckSub);
				}
				Material bgColorMaterial = Dgn.getBgColorMaterial();
				if (this.MyDrawerUCol != null && bgColorMaterial != null)
				{
					this.MMRD.setMaterial(this.MyDrawerUCol, bgColorMaterial, false);
				}
				if (this.MyDrawerWater != null)
				{
					this.MMRD.setMaterial(this.MyDrawerWater, Dgn.getWaterMaterial(), false);
				}
			}
			if (CheckSub == null && (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP))
			{
				int count_submaps = this.count_submaps;
				bool flag = false;
				for (int i = 0; i < count_submaps; i++)
				{
					M2SubMap m2SubMap = this.ASubMaps[i];
					if (!m2SubMap.closed && !(m2SubMap.get_MMRD() == null))
					{
						this.ASubMaps[i].fineMaterialNightColor(this.Dgn, null);
					}
				}
				if (flag)
				{
					this.fineSubMap();
				}
			}
			if (CheckSub == null)
			{
				if (!this.need_reentry_flag)
				{
					this.drawUCol();
				}
				if (!this.need_reentry_gradation_flag_)
				{
					this.update_mesh_flag_ |= Dgn.reentryGradation(this, this.MyDrawerUGrd, this.MyDrawerBGrd, this.MyDrawerGGrd, this.MyDrawerTGrd);
					return;
				}
			}
			else
			{
				CheckSub.fineMeshAndMaterials(true, false);
			}
		}

		public MeshDrawer getWaterDrawer()
		{
			if (this.MyDrawerWater == null)
			{
				this.MMRD.CreateMesh(ref this.MyDrawerWater, "-Water", this.Dgn.getDrawZ(this.mode, -10), this.Dgn.getWaterMaterial(), this.Dgn.getLayerForWater(), true, false, false, true);
				if (this.Unstb != null && (this.update_mesh_flag_ & 16384) == 0)
				{
					this.Unstb.FineAuto();
				}
			}
			return this.MyDrawerWater;
		}

		public MeshDrawer getMeshDrawerForBluredChip(M2BlurImage Bri, M2Puts Cp)
		{
			if (this.BlurDraw == null)
			{
				this.BlurDraw = new M2BlurMeshDrawer(this);
			}
			return this.BlurDraw.getMeshDrawerForBluredChip(Bri, Cp);
		}

		public M2BlurMeshDrawer getBlurDrawerContainer()
		{
			return this.BlurDraw;
		}

		public bool releaseBlurDraw()
		{
			if (this.BlurDraw != null)
			{
				this.BlurDraw.destruct();
				this.BlurDraw = null;
				this.need_reentry_flag = true;
				return true;
			}
			return false;
		}

		public Map2d CheckSubMapExists()
		{
			if (this.ASubMaps == null)
			{
				return this;
			}
			for (int i = this.count_submaps - 1; i >= 0; i--)
			{
				Map2d targetMap = this.ASubMaps[i].getTargetMap();
				if (targetMap == null || this.M2D.Get(targetMap.key, false) != targetMap)
				{
					global::XX.X.splice<M2SubMap>(ref this.ASubMaps, 1, i);
				}
			}
			return this;
		}

		public void openSubMaps()
		{
			if (this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.CLOSED)
			{
				return;
			}
			for (int i = 0; i < this.ASubMaps.Length; i++)
			{
				this.ASubMaps[i].open("");
			}
		}

		public void closeSubMaps(bool with_stock = true)
		{
			if (this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.CLOSED)
			{
				return;
			}
			for (int i = this.ASubMaps.Length - 1; i >= 0; i--)
			{
				this.ASubMaps[i].close(with_stock);
			}
		}

		public void fineSubMap()
		{
			if (this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.CLOSED)
			{
				return;
			}
			float x = this.M2D.Cam.x;
			float y = this.M2D.Cam.y;
			for (int i = 0; i < this.ASubMaps.Length; i++)
			{
				this.ASubMaps[i].fineCam(x, y);
			}
		}

		public M2SubMap getSubMap(Map2d MpTarget)
		{
			if (this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.CLOSED)
			{
				return null;
			}
			int num = this.ASubMaps.Length;
			for (int i = 0; i < num; i++)
			{
				M2SubMap m2SubMap = this.ASubMaps[i];
				if (m2SubMap.getTargetMap() == MpTarget)
				{
					return m2SubMap;
				}
			}
			return null;
		}

		public void reopenSubMaps()
		{
			if (this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.CLOSED)
			{
				return;
			}
			int num = this.ASubMaps.Length;
			for (int i = 0; i < num; i++)
			{
				M2SubMap m2SubMap = this.ASubMaps[i];
				m2SubMap.close(false);
				m2SubMap.open("");
			}
			this.update_mesh_flag_ |= 16384;
			this.fineSubMap();
		}

		public void initEffect(M2SubMap SubMapData)
		{
			if (Map2d.isTempMode(this.mode) || this.mode == MAPMODE.CLOSED)
			{
				return;
			}
			if (this.EF != null)
			{
				this.EF.destruct();
			}
			if (this.EFT != null)
			{
				this.EFT.destruct();
			}
			if (this.EFC != null)
			{
				this.EFC.destruct();
			}
			if (this.EFD != null)
			{
				this.EFD.destruct();
			}
			if (this.EFDT != null)
			{
				this.EFDT.destruct();
			}
			if (this.EFDC != null)
			{
				this.EFDC.destruct();
			}
			this.EF = (this.EFT = (this.EFC = null));
			this.EFD = (this.EFDT = (this.EFDC = null));
			if (SubMapData == null)
			{
				this.M2D.initEffect(this, ref this.EF, ref this.EFT);
				if (this.EF != null && this.EFD == null)
				{
					this.EFD = new M2DrawBinderContainer(this, SubMapData, this.EF, "EF");
				}
				if (this.EFT != null && this.EFDT == null)
				{
					this.EFDT = new M2DrawBinderContainer(this, SubMapData, this.EFT, "EFT");
				}
			}
			this.M2D.initChipEffect(this, SubMapData, ref this.EFC, ref this.EFDC);
		}

		public void releaseChipEffect()
		{
			if (this.EFDC != null)
			{
				this.EFDC.destruct();
				this.EFDC = null;
			}
			if (this.EFC != null)
			{
				this.EFC.destruct();
				this.EFC = null;
			}
		}

		public void close(bool destruct_player, bool reload_layers)
		{
			if (destruct_player && this.AMovP != null)
			{
				for (int i = this.AMovP.Length - 1; i >= 0; i--)
				{
					this.AMovP[i].destruct();
				}
				global::XX.X.clrA<M2MoverPr>(this.AMovP);
				Array.Resize<M2MoverPr>(ref this.AMovP, 0);
			}
			if (this.opened)
			{
				this.close();
			}
			if (reload_layers && !this.is_whole)
			{
				this.ALay = null;
				this.closeSubMaps(false);
			}
		}

		public void close()
		{
			if (this.ALay == null)
			{
				return;
			}
			this.AAPt = null;
			if (this.EVC != null)
			{
				this.EVC.releaseBccCache();
			}
			this.closeAction(true, true, false);
			if (this.DropCon != null)
			{
				this.DropCon.clear();
			}
			if (this.DmgCntCon != null)
			{
				this.DmgCntCon.clear();
			}
			for (int i = this.mover_count - 1; i >= 0; i--)
			{
				M2Mover m2Mover = this.AMov[i];
				if (m2Mover.do_not_destruct_when_remove)
				{
					m2Mover.deactivateFromMap();
					m2Mover.transform.SetParent(null, false);
					m2Mover.gameObject.SetActive(false);
				}
				else
				{
					if (!m2Mover.destructed)
					{
						m2Mover.destruct();
					}
					Object gameObject = m2Mover.gameObject;
					IN.DestroyOne(m2Mover);
					IN.DestroyOne(gameObject);
				}
			}
			global::XX.X.clrA<M2Mover>(this.AMov);
			this.mover_count = 0;
			for (int i = this.pxlanim_count - 1; i >= 0; i--)
			{
				M2PxlAnimator m2PxlAnimator = this.APxlAnim[i];
				try
				{
					m2PxlAnimator.OnDestroy();
				}
				catch
				{
				}
				IN.DestroyOne(m2PxlAnimator.gameObject);
			}
			this.pxlanim_count = 0;
			if (this.mode == MAPMODE.NORMAL || Map2d.isTempMode(this.mode))
			{
				this.closeSubMaps(!Map2d.editor_decline_lighting);
			}
			if (this.BlurDraw != null)
			{
				this.BlurDraw.destruct();
				this.BlurDraw = null;
			}
			this.releaseMeshLink(true, false);
			this.TalkTarget_ = null;
			this.handle = false;
			this.bgcol = this.bgcol0;
			this.light_cnt = 0;
			this.ALight = null;
			this.ATortureListener = null;
			this.DropCon = null;
			this.DmgCntCon = null;
			this.NM = null;
			this.ARunningObject = null;
			this.OGob_tags = null;
			IN.DestroyOne(this.Gob);
			if (this.MyCollider != null)
			{
				IN.DestroyOne(this.MyCollider);
				this.MyCollider = null;
			}
			if (Map2d.PoserForTimeStop != null && this.mode == MAPMODE.NORMAL)
			{
				Map2d.PoserForTimeStop.Clear();
			}
			this.releaseBlurDraw();
			this.M2D.mapClosed(this, ref this.EF, ref this.EFT, ref this.EFC);
			this.MMRD = null;
			this.EF = (this.EFT = (this.EFC = null));
			if (this.EFD != null)
			{
				this.EFD.destruct();
			}
			if (this.EFDT != null)
			{
				this.EFDT.destruct();
			}
			if (this.EFDC != null)
			{
				this.EFDC.destruct();
			}
			this.EFD = (this.EFDC = (this.EFDT = null));
			this.redrawing_chip_count = 0;
			this.ARedrawingChip = null;
			this.MovP = null;
			this.AMovP = null;
			this.ABcCon = null;
			if (this.mode == MAPMODE.NORMAL && this.M2D.BufferBCC == null && this.BCC != null)
			{
				this.M2D.BufferBCC = this.BCC.Dispose();
			}
			this.BCC = null;
			this.SubMapData = null;
			int num = this.ALay.Length;
			for (int j = 0; j < num; j++)
			{
				this.ALay[j].close();
			}
			if (this.M2D.curMap == this)
			{
				this.M2D.curMap = null;
			}
			this.Gob = null;
			this.mode = MAPMODE.CLOSED;
		}

		public M2LabelPoint getPoint(string label_key, bool no_error = false)
		{
			if (REG.match(label_key, M2DBase.RegFindPointLayer))
			{
				label_key = REG.rightContext;
				M2MapLayer m2MapLayer = this.getLayer(REG.R1);
				M2LabelPointContainer m2LabelPointContainer = ((m2MapLayer != null) ? m2MapLayer.remakeLabelPoint(false) : null);
				if (m2LabelPointContainer != null)
				{
					return m2LabelPointContainer.Get(label_key, true, no_error);
				}
			}
			else if (REG.match(label_key, M2DBase.RegFindPointLayerIndex))
			{
				label_key = REG.rightContext;
				int num = global::XX.X.NmI(REG.R1, 0, false, false);
				M2MapLayer m2MapLayer = ((num < this.ALay.Length) ? this.ALay[num] : null);
				M2LabelPointContainer m2LabelPointContainer2 = ((m2MapLayer != null) ? m2MapLayer.remakeLabelPoint(false) : null);
				if (m2LabelPointContainer2 != null)
				{
					return m2LabelPointContainer2.Get(label_key, true, no_error);
				}
			}
			else
			{
				int num2 = this.ALay.Length;
				for (int i = 0; i < num2; i++)
				{
					M2LabelPointContainer m2LabelPointContainer3 = this.ALay[i].remakeLabelPoint(false);
					if (m2LabelPointContainer3 != null)
					{
						M2LabelPoint m2LabelPoint = m2LabelPointContainer3.Get(label_key, true, true);
						if (m2LabelPoint != null)
						{
							return m2LabelPoint;
						}
					}
				}
				if (!no_error)
				{
					global::XX.X.de("Map2d:: ラベルポイント " + label_key + " が見つかりません", null);
				}
			}
			return null;
		}

		public int getConfig(int x, int y)
		{
			if (this.AAPt == null || !global::XX.X.BTW(0f, (float)x, (float)this.clms) || !global::XX.X.BTW(0f, (float)y, (float)this.rows))
			{
				return 128;
			}
			M2Pt m2Pt = this.AAPt[x, y];
			if (m2Pt == null)
			{
				return 4;
			}
			return m2Pt.cfg;
		}

		public M2BlockColliderContainer.BCCLine getSideBcc(int x, int y, global::XX.AIM aim = global::XX.AIM.B)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, true, false);
			if (pointPuts == null)
			{
				return null;
			}
			return pointPuts.getSideBcc(this, x, y, aim);
		}

		public M2BlockColliderContainer.BCCLine getFallableBcc(float x, float y, float margx, float search_margy, float near_y = -1f, bool check_main = true, bool check_lift = true)
		{
			if (this.BCC == null || !this.BCC.is_prepared)
			{
				return null;
			}
			M2BlockColliderContainer.BCCLine bccline;
			this.BCC.isFallable(x, y, margx, search_margy, out bccline, check_main, check_lift, -1f);
			return bccline;
		}

		public bool getCenter(string label_key, ref Vector2 Pos, bool no_error = false)
		{
			DRect point = this.getPoint(label_key, no_error);
			if (point != null)
			{
				Pos = point.getCenter();
				return true;
			}
			return false;
		}

		public bool canStand(int x = 0, int y = 0)
		{
			return CCON.canStand(this.getConfig(x, y));
		}

		public bool canStandArea(M2Puts P)
		{
			return this.canStandArea((int)((float)P.drawx / this.CLEN), (int)((float)P.drawy / this.CLEN), global::XX.X.IntC(((float)P.drawx + (float)P.rwidth / this.base_scale) / this.CLEN), global::XX.X.IntC(((float)P.drawy + (float)P.rheight / this.base_scale) / this.CLEN));
		}

		public bool canStandArea(int x, int y, int r, int b)
		{
			for (int i = x; i < r; i++)
			{
				for (int j = y; j < b; j++)
				{
					if (!CCON.canStand(this.getConfig(i, j)))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool canStandAndNoLift(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || (pointPuts.canStand() && !pointPuts.isLift());
		}

		public bool canStandAndNoBlockSlope(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || (pointPuts.canStand() && !pointPuts.isBlockSlope());
		}

		public bool canStandAndNoWater(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || (pointPuts.canStand() && !pointPuts.isBlockSlope() && !pointPuts.isWater());
		}

		public bool canStandAndNoDangerous(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || (pointPuts.canStand() && !pointPuts.isBlockSlope() && !pointPuts.isDangerous());
		}

		public bool canStandAndNoWaterLift(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || (pointPuts.canStand() && !pointPuts.isWater() && !pointPuts.isLift());
		}

		public bool isLift(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts == null || pointPuts.isLift();
		}

		public bool isDangerous(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts != null && pointPuts.isDangerous();
		}

		public bool isDangerousCfg(int x = 0, int y = 0)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			return pointPuts != null && pointPuts.isDangerousCfg();
		}

		public bool canFootOn(int x = 0, int y = 0)
		{
			return CCON.canStand(this.getConfig(x, y - 1)) && CCON.canFootOn(this.getConfig(x, y), null);
		}

		public bool canThroughBcc(float mapx, float mapy, float dmapx, float dmapy, float radiusx = 0f, float radiusy = -1000f, int aim = -1, bool directional_check = false, bool check_other_bcc = false, Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FnReturnable = null, bool set_gizmo = true, List<M2BlockColliderContainer.BCCHitInfo> ABuf = null)
		{
			if (this.BCC == null)
			{
				return true;
			}
			if (!this.canStand((int)mapx, (int)mapy))
			{
				return false;
			}
			if (radiusy == -1000f)
			{
				radiusy = radiusx;
			}
			if (ABuf != null)
			{
				ABuf.Clear();
			}
			bool flag = this.BCC.crosspoint(mapx, mapy, dmapx, dmapy, radiusx, radiusy, ABuf, directional_check, FnReturnable).z < 2f;
			if (ABuf == null && !flag)
			{
				return flag;
			}
			if (check_other_bcc)
			{
				flag = this.canThroughBccAnother(mapx, mapy, dmapx, dmapy, radiusx, radiusy, aim, directional_check, FnReturnable, false, ABuf) && flag;
			}
			return flag;
		}

		public bool canThroughBccAnother(float mapx, float mapy, float dmapx, float dmapy, float radiusx = 0f, float radiusy = -1000f, int aim = -1, bool directional_check = false, Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FnReturnable = null, bool set_gizmo = true, List<M2BlockColliderContainer.BCCHitInfo> ABuf = null)
		{
			bool flag = true;
			for (int i = this.count_carryable_bcc - 1; i >= 0; i--)
			{
				flag = this.ABcCon[i].crosspoint(mapx, mapy, dmapx, dmapy, radiusx, radiusy, ABuf, directional_check, FnReturnable).z < 2f && flag;
				if (!flag && ABuf == null)
				{
					return false;
				}
			}
			return flag;
		}

		public bool canThroughBccR(float mapx, float mapy, float len, float agR, float radius = 0f)
		{
			return this.canThroughBcc(mapx, mapy, mapx + len * global::XX.X.Cos(agR), mapy - len * global::XX.X.Sin(agR), radius, -1000f, -1, false, false, null, true, null);
		}

		public Vector3 checkThroughBccNearestHitPos(float mapx, float mapy, float dmapx, float dmapy, float radiusx = 0f, float radiusy = -1000f, int aim = -1, bool directional_check = false, bool check_other_bcc = false, Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FnReturnable = null, bool set_gizmo = true)
		{
			M2BlockColliderContainer.BCCLine bccline;
			return this.checkThroughBccNearestHitPos(mapx, mapy, dmapx, dmapy, out bccline, radiusx, radiusy, aim, directional_check, check_other_bcc, FnReturnable, set_gizmo);
		}

		public Vector3 checkThroughBccNearestHitPos(float mapx, float mapy, float dmapx, float dmapy, out M2BlockColliderContainer.BCCLine BCCHit, float radiusx = 0f, float radiusy = -1000f, int aim = -1, bool directional_check = false, bool check_other_bcc = false, Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FnReturnable = null, bool set_gizmo = true)
		{
			Map2d.APosInfoBuf.Clear();
			BCCHit = null;
			Vector3 vector = new Vector3(dmapx, dmapy, -1f);
			if (!this.canThroughBcc(mapx, mapy, dmapx, dmapy, radiusx, radiusy, aim, directional_check, check_other_bcc, FnReturnable, set_gizmo, Map2d.APosInfoBuf) && Map2d.APosInfoBuf.Count > 0)
			{
				for (int i = Map2d.APosInfoBuf.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCHitInfo bcchitInfo = Map2d.APosInfoBuf[i];
					float num = global::XX.X.LENGTHXY2(mapx, mapy, bcchitInfo.x, bcchitInfo.y);
					if (vector.z < 0f || num < vector.z)
					{
						BCCHit = bcchitInfo.Hit;
						vector.Set(bcchitInfo.x, bcchitInfo.y, num);
					}
				}
			}
			return vector;
		}

		private static void initThrowRay()
		{
			Map2d.AHit = new RaycastHit2D[1];
			Map2d.ulayermask = 1 << M2DBase.Instance.map_object_layer;
			Map2d.Flt.layerMask = Map2d.ulayermask;
		}

		public bool canThroughR(float mapx, float mapy, float len, float agR, float radius = 0f)
		{
			if (Map2d.AHit == null)
			{
				Map2d.initThrowRay();
			}
			Vector2 vector = new Vector2(this.ux2effectScreenx(this.map2ux(mapx)), this.uy2effectScreeny(this.map2uy(mapy)));
			Vector2 vector2 = new Vector2(global::XX.X.Cos(agR), global::XX.X.Sin(agR));
			radius *= this.CLENB * 0.015625f;
			return Physics2D.CircleCastNonAlloc(vector, radius, vector2, Map2d.AHit, len, Map2d.ulayermask) == 0;
		}

		public bool canThroughRectR(float mapx, float mapy, float mapw, float maph, float len, global::XX.AIM a)
		{
			if (Map2d.AHit == null)
			{
				Map2d.initThrowRay();
			}
			len *= this.CLENB * 0.015625f;
			Vector2 vector = new Vector2(this.ux2effectScreenx(this.map2ux(mapx)), this.uy2effectScreeny(this.map2uy(mapy)));
			Vector2 vector2 = new Vector2((float)global::XX.CAim._XD(a, 1), (float)global::XX.CAim._YD(a, 1));
			float num = mapw * this.CLENB * 0.015625f;
			float num2 = maph * this.CLENB * 0.015625f;
			return Physics2D.BoxCastNonAlloc(vector + vector2 * len, new Vector2(num, num2), 0f, vector2, Map2d.AHit, 0f, Map2d.ulayermask) == 0;
		}

		public bool canThroughRectAgR(float mapx, float mapy, float mapw, float maph, float len, float agR)
		{
			if (Map2d.AHit == null)
			{
				Map2d.initThrowRay();
			}
			len *= this.CLENB * 0.015625f;
			Vector2 vector = new Vector2(this.ux2effectScreenx(this.map2ux(mapx)), this.uy2effectScreeny(this.map2uy(mapy)));
			Vector2 vector2 = new Vector2(global::XX.X.Cos(agR), global::XX.X.Sin(agR));
			float num = mapw * this.CLENB * 0.015625f;
			float num2 = maph * this.CLENB * 0.015625f;
			return Physics2D.BoxCastNonAlloc(vector, new Vector2(num, num2), 0f, vector2, Map2d.AHit, len, Map2d.ulayermask) == 0;
		}

		public bool canThroughXy(float mapx, float mapy, float dmapx, float dmapy, float radius = 0f)
		{
			return this.canThroughR(mapx, mapy, global::XX.X.LENGTHXY(mapx, mapy, dmapx, dmapy), this.GAR(mapx, mapy, dmapx, dmapy), radius);
		}

		public float getFootableY(float mapx, int sy, int seek = 12, bool check_slope = false, float near_y = -1f, bool force_calc_cfg = false, bool check_main = true, bool check_lift = true, float rect_size_x = 0f)
		{
			bool flag = false;
			if (seek < 0)
			{
				seek = -seek;
				flag = true;
			}
			if (force_calc_cfg || this.BCC == null || !this.BCC.is_prepared)
			{
				int num = 0;
				int num2 = -1;
				int num3 = (int)mapx;
				M2Pt pointPuts = this.getPointPuts(num3, sy, false, false);
				int num4 = ((pointPuts != null) ? pointPuts.cfg : 0);
				if ((check_main && CCON.canFootOn(num4, null)) || (check_lift && pointPuts != null && pointPuts.isLift()))
				{
					for (int i = 1; i <= seek; i++)
					{
						M2Pt pointPuts2 = this.getPointPuts(num3, sy - i, false, false);
						int num5 = ((pointPuts2 != null) ? pointPuts2.cfg : 4);
						if ((!check_main || !CCON.canFootOn(num5, null)) && (!check_lift || pointPuts2 == null || !pointPuts2.isLift()))
						{
							num2 = sy - i + 1;
							num = num4;
							break;
						}
						num4 = num5;
					}
					if (num == 0)
					{
						if (flag)
						{
							return -1000f;
						}
						num2 = sy;
					}
				}
				else
				{
					for (int j = 1; j <= seek; j++)
					{
						M2Pt pointPuts3 = this.getPointPuts(num3, sy + j, false, false);
						int num6 = ((pointPuts3 != null) ? pointPuts3.cfg : 4);
						if ((check_main && CCON.canFootOn(num6, null)) || (check_lift && pointPuts3 != null && pointPuts3.isLift()))
						{
							num2 = sy + j;
							num = num6;
							break;
						}
					}
					if (num == 0)
					{
						if (flag)
						{
							return -1000f;
						}
						num2 = sy + seek + 1;
					}
				}
				if (check_slope && num > 0 && CCON.isSlope(num))
				{
					float slopeLevel = CCON.getSlopeLevel(num, false);
					float slopeLevel2 = CCON.getSlopeLevel(num, true);
					return (float)num2 + global::XX.X.NI(slopeLevel, slopeLevel2, global::XX.X.frac(mapx)) - rect_size_x * global::XX.X.Abs(CCON.getTiltLevel01(num));
				}
				return (float)num2;
			}
			else
			{
				M2BlockColliderContainer.BCCLine bccline;
				this.BCC.isFallable(mapx, (float)sy, 0f, (float)seek, out bccline, check_main, check_lift, near_y);
				if (bccline == null)
				{
					return (float)(flag ? (-1000) : sy);
				}
				float num7 = bccline.slopeBottomY(mapx);
				if (!check_slope)
				{
					return (float)global::XX.X.IntC(num7);
				}
				if (bccline.is_naname)
				{
					if (rect_size_x != 0f)
					{
						num7 += (float)bccline._yd * bccline.line_a * rect_size_x;
					}
					return global::XX.X.MMX(bccline.y, num7, bccline.bottom);
				}
				return num7;
			}
		}

		public float getSafeRoomPosition(float cx, float cy, float sizex, float sizey, bool no_slope, bool no_water, bool no_dangerous = true, bool ensure_all_canstand = true)
		{
			int num = (int)global::XX.X.Mx(cx - sizex, 0f);
			int num2 = (int)global::XX.X.Mx(cy - sizey, 0f);
			int num3 = global::XX.X.Mx(num + 1, global::XX.X.Mn(global::XX.X.IntC(cx + sizex), this.clms));
			int num4 = global::XX.X.Mx(num2 + 1, global::XX.X.Mn(global::XX.X.IntC(cy + sizey), this.rows));
			float num5 = 0f;
			for (int i = num2; i < num4; i++)
			{
				for (int j = num; j < num3; j++)
				{
					M2Pt pointPuts = this.getPointPuts(j, i, false, false);
					if (pointPuts == null)
					{
						num5 += 1f;
					}
					else
					{
						if ((no_slope && pointPuts.isBlockSlope()) || (no_water && pointPuts.isWater()) || (no_dangerous && pointPuts.isDangerousNotWater()))
						{
							return -1f;
						}
						if (!pointPuts.canStand())
						{
							if (ensure_all_canstand)
							{
								return -1f;
							}
						}
						else
						{
							num5 += 1f;
						}
					}
				}
			}
			return num5;
		}

		public M2MoverPr Pr
		{
			get
			{
				return this.MovP;
			}
		}

		public bool playerActionUseable()
		{
			return Map2d.can_handle || !(this.Pr != null) || (!this.Pr.isMoveScriptActive(false) && !this.Pr.hasSimulateKey());
		}

		public M2MapLayer getLayer(string name)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				M2MapLayer m2MapLayer = this.ALay[i];
				if (m2MapLayer.name == name)
				{
					return m2MapLayer;
				}
			}
			return null;
		}

		public M2MapLayer getLayer(int i)
		{
			return this.ALay[i];
		}

		public M2MapLayer[] getLayerArray()
		{
			return this.ALay;
		}

		public void setLayerArray(M2MapLayer[] A)
		{
			this.ALay = A;
		}

		private Map2d initLPCommand()
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				M2MapLayer m2MapLayer = this.ALay[i];
				if (!m2MapLayer.unloaded)
				{
					M2LabelPointContainer lp = m2MapLayer.LP;
					int length = lp.Length;
					for (int j = 0; j < length; j++)
					{
						lp.Get(j).initEvent();
					}
				}
			}
			return this;
		}

		public Map2d closeLPCommand(bool set_reload_flag = true)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				M2LabelPointContainer lp = this.ALay[i].LP;
				int length = lp.Length;
				for (int j = 0; j < length; j++)
				{
					lp.Get(j).closeEvent();
				}
			}
			if (set_reload_flag)
			{
				this.cmd_reload_flg |= 16U;
			}
			return this;
		}

		public bool evOpenMap(bool is_init)
		{
			if (this.EVC == null)
			{
				return true;
			}
			if (!this.EVC.evOpen(is_init))
			{
				return false;
			}
			if (is_init)
			{
				if (this.TalkTarget_ is M2EventItem)
				{
					this.setTalkTarget(null, true);
				}
				for (int i = this.mover_count - 1; i >= 0; i--)
				{
					this.AMov[i].evInit();
				}
			}
			return true;
		}

		public bool evCloseMap(bool is_end)
		{
			if (this.EVC == null)
			{
				return true;
			}
			if (!this.EVC.evClose(is_end))
			{
				return false;
			}
			if (is_end)
			{
				if (this.MovP != null)
				{
					this.MovP.need_check_event = true;
				}
				for (int i = this.mover_count - 1; i >= 0; i--)
				{
					this.AMov[i].evQuit();
				}
			}
			return true;
		}

		public bool evReadMap(EvReader ER, StringHolder rER, int skipping = 0)
		{
			return M2EventCommand.readEvLineM2(ER, rER, skipping);
		}

		public bool evMoveCheck()
		{
			for (int i = this.mover_count - 1; i >= 0; i--)
			{
				if (this.AMov[i].isMoveScriptActive(false))
				{
					return false;
				}
			}
			return true;
		}

		bool IEventWaitListener.EvtWait(bool is_first)
		{
			return this.need_init_action || (this.cmd_reload_flg & 16U) != 0U || this.need_update_collider || (this.cmd_reload_flg & 2U) > 0U;
		}

		public M2Mover getMoverByName(string k, bool no_error = false)
		{
			if (k != "" && TX.charIs(k, 0, '%'))
			{
				return this.MovP;
			}
			for (int i = this.count_movers - 1; i >= 0; i--)
			{
				M2Mover m2Mover = this.AMov[i];
				if (m2Mover.key == k)
				{
					return m2Mover;
				}
			}
			return null;
		}

		public static Vector2 getFixPoint(Vector2 Pt)
		{
			Pt.Set(Mathf.Round(Pt.x / 0.5f) * 0.5f, Mathf.Round(Pt.y / 0.5f) * 0.5f);
			return Pt;
		}

		public Vector2 getPos(string key, float _shx = 0f, float _shy = 0f, M2Mover Mv = null)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			key = TX.trim(key);
			if (key == "")
			{
				return vector;
			}
			if (REG.match(key, M2DBase.RegFindShift))
			{
				key = REG.leftContext;
				_shx += global::XX.X.Nm(REG.R1, 0f, false) / this.CLEN;
				_shy += global::XX.X.Nm(REG.R2, 0f, false) / this.CLEN;
			}
			if (REG.match(key, M2DBase.RegFindMover))
			{
				M2Mover moverByName = this.getMoverByName(REG.R1, false);
				if (moverByName != null)
				{
					vector.Set(moverByName.x_shifted, moverByName.y_shifted);
				}
				else
				{
					global::XX.X.de("Map2d::getPos - Mover が見つかりません :" + key, null);
				}
			}
			else if (key.IndexOf("%") >= 0)
			{
				if (this.MovP != null)
				{
					vector.Set(this.MovP.x, this.MovP.y);
				}
				else
				{
					global::XX.X.de("Map2d::getPos - Pr が見つかりません: " + key, null);
				}
			}
			else
			{
				M2LabelPointContainer m2LabelPointContainer = null;
				if (REG.match(key, M2DBase.RegFindPointLayer))
				{
					key = REG.rightContext;
					M2MapLayer layer = this.getLayer(REG.R1);
					if (layer != null)
					{
						m2LabelPointContainer = layer.remakeLabelPoint(false);
					}
				}
				else
				{
					string text = key;
					int num = text.IndexOf(".");
					if (num >= 0)
					{
						text = TX.slice(text, 0, num);
					}
					int num2 = this.ALay.Length;
					int num3 = 0;
					while (num3 < num2 && m2LabelPointContainer == null)
					{
						M2LabelPointContainer m2LabelPointContainer2 = this.ALay[num3].remakeLabelPoint(false);
						if (m2LabelPointContainer2 != null && m2LabelPointContainer2.Get(text, true, true) != null)
						{
							m2LabelPointContainer = m2LabelPointContainer2;
						}
						num3++;
					}
				}
				vector = ((m2LabelPointContainer != null) ? (m2LabelPointContainer.getPosMv(key, _shx * this.CLEN, _shy * this.CLEN, (Mv != null) ? Mv : this.MovP) * this.rCLEN) : new Vector2(_shx, _shy));
				_shy = (_shx = 0f);
			}
			vector.x += _shx;
			vector.y += _shy;
			return vector;
		}

		public void addRunnerObject(IRunAndDestroy Lp)
		{
			if (this.SubMapData != null)
			{
				this.SubMapData.getBaseMap().addRunnerObject(Lp);
				return;
			}
			if (this.ARunningObject == null)
			{
				this.ARunningObject = new List<IRunAndDestroy>(4);
			}
			this.ARunningObject.Add(Lp);
		}

		public void remRunnerObject(IRunAndDestroy Lp)
		{
			if (this.SubMapData != null)
			{
				this.SubMapData.getBaseMap().remRunnerObject(Lp);
				return;
			}
			if (this.ARunningObject != null)
			{
				this.ARunningObject.Remove(Lp);
			}
		}

		private void remakeLabelPoint()
		{
			if (this.ARunningObject != null)
			{
				for (int i = this.ARunningObject.Count - 1; i >= 0; i--)
				{
					if (this.ARunningObject[i] is M2LabelPoint)
					{
						this.ARunningObject.RemoveAt(i);
					}
				}
				if (this.ARunningObject.Count == 0)
				{
					this.ARunningObject = null;
				}
			}
			if (this.ALay == null)
			{
				return;
			}
			int num = this.ALay.Length;
			for (int j = 0; j < num; j++)
			{
				this.ALay[j].remakeLabelPoint(true);
			}
		}

		public M2LabelPoint getLabelPoint(string key)
		{
			int num = key.IndexOf("..");
			if (num < 0)
			{
				int num2 = this.ALay.Length;
				for (int i = 0; i < num2; i++)
				{
					M2LabelPoint labelPoint = this.ALay[i].getLabelPoint(key);
					if (labelPoint != null)
					{
						return labelPoint;
					}
				}
				return null;
			}
			M2MapLayer layer = this.getLayer(TX.slice(key, 0, num));
			if (layer == null)
			{
				return null;
			}
			return layer.getLabelPoint(TX.slice(key, num + 2));
		}

		public M2LabelPoint getLabelPoint(M2LabelPoint.fnCheckLP Func)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				M2LabelPoint labelPoint = this.ALay[i].getLabelPoint(Func);
				if (labelPoint != null)
				{
					return labelPoint;
				}
			}
			return null;
		}

		public List<M2LabelPoint> getLabelPointAll(M2LabelPoint.fnCheckLP Func, List<M2LabelPoint> ALp = null)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				ALp = this.ALay[i].getLabelPointAll(Func, ALp);
			}
			return ALp;
		}

		public void EachLP(M2LabelPoint.fnEachLP Func)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				this.ALay[i].EachLP(Func);
			}
		}

		public IActivatable evItemActivate(string event_key, int activation = 1)
		{
			int num;
			if (this.AActivatable != null)
			{
				num = this.AActivatable.Count;
				for (int i = 0; i < num; i++)
				{
					IActivatable activatable = this.AActivatable[i];
					if (activatable.getActivateKey() == event_key)
					{
						if (activation > 0)
						{
							activatable.activate();
						}
						if (activation < 0)
						{
							activatable.deactivate();
						}
						return activatable;
					}
				}
			}
			num = this.count_layers;
			LabelPointListener<M2LabelPoint> labelPointListener = new LabelPointListener<M2LabelPoint>();
			for (int j = 0; j < num; j++)
			{
				this.ALay[j].LP.beginAll(labelPointListener);
				while (labelPointListener.next())
				{
					M2LabelPoint cur = labelPointListener.cur;
					if (cur.getActivateKey() == event_key)
					{
						if (activation > 0)
						{
							cur.activate();
						}
						if (activation < 0)
						{
							cur.deactivate();
						}
						return cur;
					}
				}
			}
			return null;
		}

		public void addActivateItem(IActivatable Ct)
		{
			if (Ct != null)
			{
				if (this.AActivatable == null)
				{
					this.AActivatable = new List<IActivatable>(4);
				}
				if (this.AActivatable.IndexOf(Ct) == -1)
				{
					this.AActivatable.Add(Ct);
				}
			}
		}

		public void remActivateItem(IActivatable Ct)
		{
			if (Ct != null && this.AActivatable != null)
			{
				this.AActivatable.Remove(Ct);
			}
		}

		public void addTortureListener(ITortureListener Tr)
		{
			if (Tr != null)
			{
				if (this.ATortureListener == null)
				{
					this.ATortureListener = new List<ITortureListener>(4);
				}
				if (this.ATortureListener.IndexOf(Tr) == -1)
				{
					this.ATortureListener.Add(Tr);
				}
			}
		}

		public void remTortureListener(ITortureListener Tr)
		{
			if (Tr != null && this.ATortureListener != null)
			{
				this.ATortureListener.Remove(Tr);
				if (this.ATortureListener.Count == 0)
				{
					this.ATortureListener = null;
				}
			}
		}

		public M2Manpu assignManpu(string key, M2Mover Mv, int maxt = 0)
		{
			return null;
		}

		private void makeCollider(PolygonCollider2D Cld)
		{
			if (this.AAPt == null)
			{
				return;
			}
			if (Cld == null)
			{
				Cld = this.MyCollider;
			}
			if (Cld == null)
			{
				return;
			}
			Cld.gameObject.tag = "Ground";
			M2BlockColliderContainer m2BlockColliderContainer = null;
			bool flag = false;
			List<M2BlockColliderContainer.BCCInfo> list = null;
			if (Cld == this.MyCollider)
			{
				m2BlockColliderContainer = this.BCC;
				if (m2BlockColliderContainer == null)
				{
					return;
				}
				flag = true;
				if (this.APhysics != null && this.APhysics.Count > 0)
				{
					list = Map2d.ABccInfoCache;
					list.Clear();
					int count = this.APhysics.Count;
					if (list.Capacity < count)
					{
						list.Capacity = count;
					}
					for (int i = 0; i < count; i++)
					{
						M2Phys m2Phys = this.APhysics[i];
						list.Add(default(M2BlockColliderContainer.BCCInfo));
						M2FootManager footManager = m2Phys.getFootManager();
						if (footManager != null && m2BlockColliderContainer == this.BCC)
						{
							M2BlockColliderContainer.BCCLine footBCC = footManager.get_FootBCC();
							if (footBCC != null && footBCC.BCC == this.BCC)
							{
								list[i] = new M2BlockColliderContainer.BCCInfo(footBCC);
							}
						}
					}
				}
			}
			this.M2D.ColliderCreator.initS(this).createCollider(this.AAPt, Cld, m2BlockColliderContainer, null, null);
			if (flag)
			{
				Cld.enabled = true;
				if (this.EVC != null)
				{
					this.EVC.releaseBccCache();
				}
				this.recheckFootObject(m2BlockColliderContainer, list);
			}
			if (m2BlockColliderContainer == this.BCC)
			{
				this.M2D.colliderUpdated();
			}
		}

		public void addBCCEventListener(IBCCEventListener Lsn)
		{
			if (this.BCC == null)
			{
				return;
			}
			this.BCC.addBCCEventListener(Lsn);
		}

		public void remBCCEventListener(IBCCEventListener Lsn)
		{
			if (this.BCC == null)
			{
				return;
			}
			this.BCC.remBCCEventListener(Lsn);
		}

		public void addBCCFootListener(IBCCFootListener Lsn)
		{
			if (this.BCC == null)
			{
				return;
			}
			this.BCC.addBCCFootListener(Lsn);
		}

		public void remBCCFootListener(IBCCFootListener Lsn)
		{
			if (this.BCC == null)
			{
				return;
			}
			this.BCC.remBCCFootListener(Lsn);
		}

		public void remBCCAdditionalLine(M2BlockColliderContainer.BCCLine Line)
		{
			if (this.BCC == null)
			{
				return;
			}
			this.BCC.remAdditionalLine(Line);
		}

		public int simulateDropItem(ref float x, ref float y, ref float vx0, ref float vy0, float gravity_scale, float bounce_x_reduce = 1f, float bounce_y_reduce = 0.67f, float timescale = 1f, float size = 0f, bool consider_water_as_ground = false, bool block_force_stop = true)
		{
			float num = vx0 * timescale;
			float num2 = vy0 * timescale;
			int num3 = (int)x;
			int num4 = (int)y;
			M2Pt pointPuts = this.getPointPuts(num3, num4, true, false);
			x += num;
			y += num2;
			float gravityVelocity = M2DropObject.getGravityVelocity(this, gravity_scale);
			vy0 += gravityVelocity * timescale;
			int num5 = 0;
			if (pointPuts != null)
			{
				if (vx0 != 0f)
				{
					M2BlockColliderContainer.BCCLine sideBcc = pointPuts.getSideBcc(this, num3, num4, (vx0 > 0f) ? global::XX.AIM.R : global::XX.AIM.L);
					if (sideBcc != null && global::XX.X.Abs(x + (float)global::XX.X.MPF(vx0 > 0f) * size - sideBcc.x) <= size + 0.02f + global::XX.X.Abs(num))
					{
						num5 |= 1;
						vx0 *= -bounce_x_reduce;
					}
				}
				M2BlockColliderContainer.BCCLine sideBcc2 = pointPuts.getSideBcc(this, num3, num4, (vy0 > 0f) ? global::XX.AIM.B : global::XX.AIM.T);
				if (sideBcc2 != null)
				{
					float num6 = sideBcc2.slopeBottomY(x, sideBcc2.BCC.base_shift_x, sideBcc2.BCC.base_shift_y, true);
					if (global::XX.X.Abs(y + (float)global::XX.X.MPF(vy0 > 0f) * size - num6) <= size + 0.18f + global::XX.X.Abs(num2))
					{
						num5 |= 2;
						if (global::XX.X.Abs(vy0) < gravityVelocity + 0.03f)
						{
							vy0 = 0f;
							num5 |= 4;
						}
						else
						{
							vy0 *= -bounce_y_reduce;
						}
					}
				}
				int cfg = pointPuts.cfg;
				if (CCON.isWater(cfg))
				{
					num5 |= 8;
				}
				if (block_force_stop && !CCON.canStand(cfg))
				{
					num5 |= 4;
				}
			}
			return num5;
		}

		public bool talkCurrentFocus(M2MoverPr SubmitFrom)
		{
			return this.TalkTarget_ != null && this.TalkTarget_.SubmitTalkable((SubmitFrom != null) ? SubmitFrom : this.getKeyPr());
		}

		public IM2TalkableObject TalkTarget
		{
			get
			{
				return this.TalkTarget_;
			}
		}

		public bool setTalkTarget(IM2TalkableObject _T, bool event_init = false)
		{
			if (this.TalkTarget_ == _T)
			{
				return true;
			}
			if (this.TalkTarget_ != null)
			{
				this.TalkTarget_.BlurTalkable(event_init);
			}
			bool flag = false;
			if (_T != null && _T.canTalkable(this.MovP != null && this.MovP.isFacingEnemy()) == 0)
			{
				_T = null;
				flag = true;
			}
			this.TalkTarget_ = _T;
			if (this.TalkTarget_ != null)
			{
				this.TalkTarget_.FocusTalkable();
			}
			if (this.MovP != null)
			{
				this.MovP.need_check_talk_event_target = true;
			}
			this.M2D.changeTalkTarget(this.TalkTarget_);
			return flag;
		}

		public M2SoundPlayerItem playSnd(string snd_title)
		{
			return this.M2D.Snd.play(snd_title);
		}

		public M2SoundPlayerItem playSnd(string snd_title, string pos_snd_key, float mapx, float mapy, byte _voice_priority_manual = 1)
		{
			return this.M2D.Snd.playAt(snd_title, pos_snd_key, mapx, mapy, SndPlayer.SNDTYPE.SND, _voice_priority_manual);
		}

		public EffectItem setE(string ef_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.setE(ef_name, mapx, mapy, _z, _time, _saf);
		}

		public EffectItem setEC(string ef_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EFC == null)
			{
				return null;
			}
			return this.EFC.setE(ef_name, mapx, mapy, _z, _time, _saf);
		}

		public EffectItem setE(string individual_name, FnEffectRun Fn, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.setEffectWithSpecificFn(individual_name, mapx, mapy, _z, _time, _saf, Fn);
		}

		public EffectItem setEC(string individual_name, FnEffectRun Fn, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EFC == null)
			{
				return null;
			}
			return this.EFC.setEffectWithSpecificFn(individual_name, mapx, mapy, _z, _time, _saf, Fn);
		}

		public EffectItem setET(string ef_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			IEffectSetter effectTop = this.getEffectTop();
			if (effectTop == null)
			{
				return null;
			}
			return effectTop.setE(ef_name, mapx, mapy, _z, _time, _saf);
		}

		public EffectItem setET(string individual_name, FnEffectRun Fn, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			IEffectSetter effectTop = this.getEffectTop();
			if (effectTop == null)
			{
				return null;
			}
			return effectTop.setEffectWithSpecificFn(individual_name, mapx, mapy, _z, _time, _saf, Fn);
		}

		public EffectItem PtcN(string ptc_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.PtcN(ptc_name, mapx, mapy, _z, _time, _saf);
		}

		public EffectItem PtcN(EfParticle Ptc, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.PtcN(Ptc, mapx, mapy, _z, _time, _saf);
		}

		public EffectItem PtcT(string ptc_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			IEffectSetter effectTop = this.getEffectTop();
			if (effectTop == null)
			{
				return null;
			}
			return effectTop.PtcN(ptc_name, mapx, mapy, _z, _time, _saf);
		}

		public PTCThread PtcSTF(string ptcst_name, IEfPInteractale Listener = null, bool follow = false, bool is_top = false)
		{
			IEffectSetter effectSetter = (is_top ? this.getEffectTop() : this.EF);
			if (effectSetter == null)
			{
				return null;
			}
			return effectSetter.PtcST(ptcst_name, Listener, follow ? PTCThread.StFollow.FOLLOW_C : PTCThread.StFollow.NO_FOLLOW, null);
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.PtcST(ptcst_name, Listener ?? this.M2D, follow, null);
		}

		public Map2d PtcSTsetVar(string key, double v)
		{
			if (this.EF == null)
			{
				return null;
			}
			PTCThreadRunner.PreVar(key, v);
			return this;
		}

		public PTCThread PtcST2Base(string ptcst_name, float slow_facter = 0f, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			if (this.EF == null)
			{
				return null;
			}
			M2MoverPr pr = this.Pr;
			if (pr != null)
			{
				PtcHolder ptcHld = pr.PtcHld;
				if (ptcHld != null)
				{
					pr.defineParticlePreVariable();
					return ptcHld.PtcSTTimeFixed(ptcst_name, slow_facter, PtcHolder.PTC_HOLD.NORMAL, follow, false);
				}
			}
			return this.EF.PtcST(ptcst_name, this.M2D, follow, null);
		}

		public M2DrawBinder setED(string name, M2DrawBinder.FnEffectBind fnEf, float saf = 0f)
		{
			return this.EFD.Add(name, fnEf, saf);
		}

		public M2DrawBinder setEDT(string name, M2DrawBinder.FnEffectBind fnEf, float saf = 0f)
		{
			return this.EFDT.Add(name, fnEf, saf);
		}

		public M2DrawBinder setEDC(string name, M2DrawBinder.FnEffectBind fnEf, float saf = 0f)
		{
			return this.EFDC.Add(name, fnEf, saf);
		}

		public M2DrawBinder remED(M2DrawBinder.FnEffectBind fnEf)
		{
			if (this.EFD != null)
			{
				this.EFD.Rem(fnEf);
			}
			return null;
		}

		public M2DrawBinder remED(M2DrawBinder Ed)
		{
			if (Ed != null)
			{
				Ed.destruct();
			}
			return null;
		}

		public M2DrawBinder remEDC(M2DrawBinder.FnEffectBind fnEf)
		{
			if (this.EFDC != null)
			{
				this.EFDC.Rem(fnEf);
			}
			return null;
		}

		public M2DropObject setDrop(M2DropObject.FnDropObjectDraw FnDraw, float _x, float _y, float _vx, float _vy, float _z = -1f, float _time = -1f)
		{
			return this.DropCon.Add(FnDraw, _x, _y, _vx, _vy, _z, _time);
		}

		public float GAR(float mpx, float mpy, float mpx2, float mpy2)
		{
			return global::XX.X.GAR2(mpx, -mpy, mpx2, -mpy2);
		}

		public float pixel2meshx(float x)
		{
			return x - (float)this.width * 0.5f;
		}

		public float pixel2meshy(float y)
		{
			return -y + (float)this.height * 0.5f;
		}

		public float meshx2pixel(float x)
		{
			return x + (float)this.width * 0.5f;
		}

		public float meshy2pixel(float y)
		{
			return -y + (float)this.height * 0.5f;
		}

		public float pixel2ux(float x)
		{
			return this.pixel2meshx(x) * 0.015625f;
		}

		public float pixel2uy(float y)
		{
			return this.pixel2meshy(y) * 0.015625f;
		}

		public float pixel2ux_rounded(float x)
		{
			return (float)global::XX.X.IntR(this.pixel2meshx(x) * this.mover_scale) / this.mover_scale * 0.015625f;
		}

		public float pixel2uy_rounded(float y)
		{
			return (float)global::XX.X.IntR(this.pixel2meshy(y) * this.mover_scale) / this.mover_scale * 0.015625f;
		}

		public float map2ux(float x)
		{
			return this.pixel2meshx(x * this.CLEN) * 0.015625f;
		}

		public float map2uy(float y)
		{
			return this.pixel2meshy(y * this.CLEN) * 0.015625f;
		}

		public float ux2effectScreenx(float x)
		{
			return this.M2D.ux2effectScreenx(x);
		}

		public float uy2effectScreeny(float y)
		{
			return this.M2D.uy2effectScreeny(y);
		}

		public float map2meshx(float x)
		{
			return this.pixel2meshx(x * this.CLEN);
		}

		public float map2meshy(float y)
		{
			return this.pixel2meshy(y * this.CLEN);
		}

		public float uxToMapx(float x)
		{
			return this.meshx2pixel(x * 64f) / this.CLEN;
		}

		public float uyToMapy(float y)
		{
			return this.meshy2pixel(y * 64f) / this.CLEN;
		}

		public float globaluxToMapx(float x)
		{
			return this.uxToMapx(this.M2D.effectScreenx2ux(x));
		}

		public float globaluyToMapy(float y)
		{
			return this.uyToMapy(this.M2D.effectScreeny2uy(y));
		}

		public float map2globalux(float x)
		{
			return this.M2D.ux2effectScreenx(this.map2ux(x));
		}

		public float map2globaluy(float y)
		{
			return this.M2D.uy2effectScreeny(this.map2uy(y));
		}

		public float uVelocityxToMapx(float x)
		{
			return x / 60f * 64f / this.CLENB;
		}

		public float uVelocityyToMapy(float y)
		{
			return -y / 60f * 64f / this.CLENB;
		}

		public float mapvxToUVelocityx(float x)
		{
			return x * 60f * 0.015625f * this.CLENB;
		}

		public float mapvyToUVelocityy(float y)
		{
			return -y * 60f * 0.015625f * this.CLENB;
		}

		public void drawConfig(MeshDrawer Md, int sx = 0, int sy = 0)
		{
			for (int i = 0; i < this.rows; i++)
			{
				for (int j = 0; j < this.clms; j++)
				{
					Md.Col = C32.d2c(CCON.canStand(this.AAPt[j, i].cfg) ? 4292280298U : 4292819224U);
					Md.Box((float)sx + (float)j * this.CLEN, (float)sy + (float)i * this.CLEN, (float)sx + (float)j * this.CLEN + this.CLEN - 1f, (float)sy + (float)i * this.CLEN + this.CLEN - 1f, 1f, false);
				}
			}
		}

		public bool update_mesh_flag
		{
			get
			{
				return (this.update_mesh_flag_ & -2049) > 0;
			}
			set
			{
				if (value)
				{
					this.update_mesh_flag_ = 15 | (this.update_mesh_flag_ & 2048);
				}
			}
		}

		public bool update_mesh_flag_has_splicemesh
		{
			get
			{
				return (this.update_mesh_flag_ & 16384) != 0;
			}
		}

		public bool update_mesh_flag_has_initaction
		{
			get
			{
				return (this.update_mesh_flag_ & 2048) != 0;
			}
		}

		public int getLayer2UpdateFlag(MeshDrawer Md)
		{
			if (Md == this.MyDrawerB)
			{
				return 1;
			}
			if (Md == this.MyDrawerG)
			{
				return 2;
			}
			if (Md == this.MyDrawerT)
			{
				return 4;
			}
			if (Md == this.MyDrawerL)
			{
				return 512;
			}
			if (Md == this.MyDrawerTT)
			{
				return 8192;
			}
			if (Md == this.MyDrawerWater)
			{
				return 1024;
			}
			return 0;
		}

		public void releaseUnstabilizeDrawer()
		{
			if (this.Unstb != null)
			{
				this.Unstb.deactivateAll(true);
			}
		}

		public void drawUCol()
		{
			if (this.MyDrawerUCol != null)
			{
				this.MyDrawerUCol.clear(false, false);
				if (this.Dgn != null && this.Dgn.drawUCol(this.MyDrawerUCol, this, true))
				{
					this.update_mesh_flag_ |= 8;
				}
			}
		}

		private void reentryAllChips()
		{
			this.MMRD.clearRendered();
			this.drawUCol();
			this.MyDrawerB.prepareReentry();
			this.MyDrawerG.prepareReentry();
			this.MyDrawerL.prepareReentry();
			this.MyDrawerT.prepareReentry();
			this.MyDrawerTT.prepareReentry();
			if (this.MyDrawerWater != null)
			{
				this.MyDrawerWater.clear(false, false);
			}
			if (this.BlurDraw != null)
			{
				this.BlurDraw.deactivate(true);
				this.BlurDraw.need_reentry = false;
			}
			this.redrawing_chip_count = 0;
			for (int i = this.ALay.Length - 1; i >= 0; i--)
			{
				Map2d.reentryAllChipsForOneLayer(this, this.ALay[i]);
			}
			this.MyDrawerB.Col = MTRX.ColWhite;
			this.MyDrawerG.Col = MTRX.ColWhite;
			this.MyDrawerL.Col = MTRX.ColWhite;
			this.MyDrawerT.Col = MTRX.ColWhite;
			this.MyDrawerTT.Col = MTRX.ColWhite;
			this.need_reentry_flag_ = false;
			this.MMRD.finishReentry();
		}

		public static void reentryAllChipsForOneLayer(Map2d Mp, M2MapLayer Lay)
		{
			Mp.update_mesh_flag_ |= Lay.reentryAllChips(Mp.MyDrawerB, Mp.MyDrawerG, Mp.MyDrawerT, Mp.MyDrawerL, Mp.MyDrawerTT, ref Mp.ARedrawingChip, ref Mp.redrawing_chip_count);
		}

		public void entryChipPlaying(M2Puts Cp)
		{
			if (this.need_reentry_flag_)
			{
				this.drawCheck(0f);
			}
			Cp.arrangeable = true;
			this.update_mesh_flag_ |= Cp.entryChipMesh(this.MyDrawerB, this.MyDrawerG, this.MyDrawerT, this.MyDrawerL, this.MyDrawerTT, 0f, 0f, 1f, 0f);
		}

		private void clearGradation()
		{
			if (!this.is_submap)
			{
				this.MMRD.clearBit(new MdMap[] { this.MyDrawerUGrd, this.MyDrawerBGrd, this.MyDrawerGGrd, this.MyDrawerTGrd });
				this.update_mesh_flag_ |= 16384;
			}
			this.MyDrawerUGrd.prepareReentry();
			this.MyDrawerBGrd.prepareReentry();
			this.MyDrawerGGrd.prepareReentry();
			this.MyDrawerTGrd.prepareReentry();
		}

		private void reentryGradation(bool clear_pre = true)
		{
			if (clear_pre)
			{
				this.clearGradation();
			}
			for (int i = this.ALay.Length - 1; i >= 0; i--)
			{
				this.update_mesh_flag_ |= this.ALay[i].reentryGradation(this.MyDrawerUGrd, this.MyDrawerBGrd, this.MyDrawerGGrd, this.MyDrawerTGrd);
			}
			if (this.Dgn == null)
			{
				this.update_mesh_flag_ |= this.Dgn.reentryGradation(this, this.MyDrawerUGrd, this.MyDrawerBGrd, this.MyDrawerGGrd, this.MyDrawerTGrd);
			}
			this.need_reentry_gradation_flag_ = false;
		}

		public bool isinCamera(float mapx, float mapy, float mapw, float maph, float extend_pixel = 0f)
		{
			return this.M2D.Cam.isCoveringMp(mapx, mapy, mapw + mapx, maph + mapy, extend_pixel);
		}

		public void clearChipsDrawer(bool close_action)
		{
			if (this.ALay == null)
			{
				return;
			}
			for (int i = this.ALay.Length - 1; i >= 0; i--)
			{
				this.ALay[i].clearChipsDrawer(close_action);
			}
		}

		public int getPointMetaPutsTo(int x, int y, List<M2Puts> APuts, string meta_key)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			if (pointPuts == null)
			{
				return 0;
			}
			return pointPuts.getPointMetaPutsTo(APuts, ulong.MaxValue, meta_key);
		}

		public List<M2Puts> getAllPointMetaPutsTo(int x, int y, int w, int h, List<M2Puts> APuts, string meta_key = null)
		{
			return this.getAllPointMetaPutsTo(x, y, w, h, APuts, ulong.MaxValue, meta_key);
		}

		public List<M2Puts> getAllPointMetaPutsTo(int x, int y, int w, int h, List<M2Puts> APuts, string meta_key0, string meta_key1)
		{
			return this.getAllPointMetaPutsTo(x, y, w, h, APuts, ulong.MaxValue, meta_key0, meta_key1);
		}

		public List<M2Puts> getAllPointMetaPutsTo(int x, int y, int w, int h, List<M2Puts> APuts, ulong layer_bits, string meta_key = null)
		{
			if (this.AAPt == null)
			{
				return APuts;
			}
			int num = x;
			int num2 = 0;
			while (num2 < w && num < this.clms)
			{
				if (num >= 0)
				{
					int num3 = y;
					int num4 = 0;
					while (num4 < h && num3 < this.rows)
					{
						if (num3 >= 0)
						{
							M2Pt pointPuts = this.getPointPuts(num, num3, false, false);
							if (pointPuts != null)
							{
								APuts = pointPuts.getPointMetaPutsCheckingTo(APuts, meta_key);
							}
						}
						num4++;
						num3++;
					}
				}
				num2++;
				num++;
			}
			return APuts;
		}

		public List<M2Puts> getAllPointMetaPutsTo(int x, int y, int w, int h, List<M2Puts> APuts, ulong layer_bits, string meta_key0, string meta_key1)
		{
			if (this.AAPt == null)
			{
				return APuts;
			}
			int num = x;
			int num2 = 0;
			while (num2 < w && num < this.clms)
			{
				if (num >= 0)
				{
					int num3 = y;
					int num4 = 0;
					while (num4 < h && num3 < this.rows)
					{
						if (num3 >= 0)
						{
							M2Pt pointPuts = this.getPointPuts(num, num3, false, false);
							if (pointPuts != null)
							{
								APuts = pointPuts.getPointMetaPutsCheckingTo(APuts, meta_key0, meta_key1);
							}
						}
						num4++;
						num3++;
					}
				}
				num2++;
				num++;
			}
			return APuts;
		}

		public List<M2Puts> getAllPointMetaPutsTo(int x, int y, int w, int h, List<M2Puts> APuts, ChipMover.FnAttractChip FnAttract)
		{
			if (this.AAPt == null)
			{
				return APuts;
			}
			int num = x;
			int num2 = 0;
			while (num2 < w && num < this.clms)
			{
				if (num >= 0)
				{
					int num3 = y;
					int num4 = 0;
					while (num4 < h && num3 < this.rows)
					{
						if (num3 >= 0)
						{
							M2Pt pointPuts = this.getPointPuts(num, num3, false, false);
							if (pointPuts != null)
							{
								APuts = pointPuts.findPuts(FnAttract, APuts);
							}
						}
						num4++;
						num3++;
					}
				}
				num2++;
				num++;
			}
			return APuts;
		}

		public M2Chip findChip(int mapx, int mapy, string metakey)
		{
			return this.findChip(mapx, mapy, ulong.MaxValue, metakey);
		}

		public M2Chip findChip(int mapx, int mapy, ulong layer_bits, string metakey)
		{
			if (this.ALay == null)
			{
				return null;
			}
			M2Pt pointPuts = this.getPointPuts(mapx, mapy, false, false);
			if (pointPuts != null)
			{
				return pointPuts.findChip(layer_bits, metakey);
			}
			return null;
		}

		public List<M2Puts> findPuts(int x, int y, ChipMover.FnAttractChip FnAttract, List<M2Puts> APt = null)
		{
			M2Pt pointPuts = this.getPointPuts(x, y, false, false);
			if (pointPuts != null)
			{
				APt = pointPuts.findPuts(FnAttract, APt);
			}
			return APt;
		}

		public M2Chip getHardChip(int mapx, int mapy, List<M2Chip> ACpReturn = null, bool include_lift = false, bool include_slope = true, string metakey = null)
		{
			M2Chip m2Chip = null;
			M2Pt pointPuts = this.getPointPuts(mapx, mapy, false, false);
			if (pointPuts == null)
			{
				return null;
			}
			for (int i = pointPuts.count - 1; i >= 0; i--)
			{
				M2Chip m2Chip2 = pointPuts[i] as M2Chip;
				if (m2Chip2 != null)
				{
					int config = m2Chip2.getConfig(mapx - m2Chip2.mapx, mapy - m2Chip2.mapy);
					if ((!CCON.canStand(config) || (include_lift && CCON.isLift(config)) || (include_slope && CCON.isBlockSlope(config))) && (metakey == null || m2Chip2.getMeta().GetS(metakey) != null))
					{
						m2Chip = m2Chip2;
						if (ACpReturn == null)
						{
							return m2Chip;
						}
						ACpReturn.Add(m2Chip);
					}
				}
			}
			return m2Chip;
		}

		public List<M2Puts> getAllMetaPutsTo(List<M2Puts> APuts, string metakey)
		{
			int num = this.ALay.Length;
			for (int i = 0; i < num; i++)
			{
				APuts = this.ALay[i].getAllMetaPutsTo(APuts, metakey);
			}
			return APuts;
		}

		public List<M2Puts> findPutsFilling(int mapx, int mapy, ChipMover.FnAttractChip FnAttract, List<M2Puts> ACp = null, DRect ClipRect = null)
		{
			if (ACp == null)
			{
				ACp = new List<M2Puts>();
			}
			List<M2Puts> list = this.findPuts(mapx, mapy, FnAttract, null);
			if (list == null)
			{
				return null;
			}
			List<int> list2 = new List<int>();
			List<M2Puts> list3 = null;
			new List<M2Puts>();
			int num = 0;
			int num2 = 0;
			int num3 = this.clms;
			int num4 = this.rows;
			if (ClipRect != null)
			{
				num = (int)(ClipRect.x / this.CLEN);
				num2 = (int)(ClipRect.y / this.CLEN);
				num3 = global::XX.X.IntC(ClipRect.right / this.CLEN);
				num4 = global::XX.X.IntC(ClipRect.bottom / this.CLEN);
			}
			List<int> list4 = new List<int>(24);
			list4.Add((mapx << 10) | mapy);
			while (list3 == null || list3.Count > 0)
			{
				list3 = new List<M2Puts>();
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					M2Puts m2Puts = list[i];
					if (list2.IndexOf(m2Puts.index) < 0)
					{
						list2.Add(m2Puts.index);
						ACp.Add(m2Puts);
						int num5 = global::XX.X.IntC((float)(m2Puts.drawx + m2Puts.iwidth) / this.CLEN);
						int num6 = global::XX.X.IntC((float)(m2Puts.drawy + m2Puts.iheight) / this.CLEN);
						for (int j = m2Puts.mapy; j < num6; j++)
						{
							for (int k = m2Puts.mapx; k < num5; k++)
							{
								for (int l = -1; l < 4; l++)
								{
									int num7 = ((l == -1) ? k : (k + global::XX.CAim._XD(l, 1)));
									int num8 = ((l == -1) ? j : (j + global::XX.CAim._YD(l, 1)));
									if (global::XX.X.BTW((float)num, (float)num7, (float)num3) && global::XX.X.BTW((float)num2, (float)num8, (float)num4))
									{
										int num9 = (num7 << 10) | num8;
										if (list4.IndexOf(num9) < 0)
										{
											list4.Add(num9);
											this.findPuts(num7, num8, FnAttract, list3);
										}
									}
								}
							}
						}
					}
				}
				list = list3;
			}
			return ACp;
		}

		public bool canHandle()
		{
			return this.handle && Map2d.can_handle;
		}

		public Map2d setHandle(bool f)
		{
			this.handle = f;
			return this;
		}

		public M2Mover[] getVectorMover()
		{
			return this.AMov;
		}

		public bool loaded
		{
			get
			{
				return this.ALay != null && this.ALay.Length != 0;
			}
		}

		public bool prepared
		{
			get
			{
				return this.loaded || this.CR != null || this.BaLoad != null;
			}
			set
			{
				if (value && this.CR == null && this.BaLoad == null)
				{
					this.M2D.readMapBody(this);
				}
			}
		}

		public bool opened
		{
			get
			{
				return this.mode > MAPMODE.CLOSED;
			}
		}

		public bool visible
		{
			get
			{
				return this.Gob != null && this.Gob.activeSelf;
			}
			set
			{
				if (this.MMRD == null)
				{
					return;
				}
				this.Gob.SetActive(value);
				if (value)
				{
					this.need_reentry_gradation_flag = true;
					this.MMRD.activate();
					if (this.mode == MAPMODE.NORMAL || Map2d.isTempMode(this.mode))
					{
						this.closeSubMaps(true);
						this.M2D.Cam.init2(this);
						this.Dgn = null;
						this.prepareMeshDrawer(null, ref this.MMRD);
						return;
					}
				}
				else
				{
					this.MMRD.deactivate(false);
				}
			}
		}

		public int count_layers
		{
			get
			{
				if (this.ALay == null)
				{
					return 0;
				}
				return this.ALay.Length;
			}
		}

		public int count_movers
		{
			get
			{
				return global::XX.X.countNotEmpty<M2Mover>(this.AMov);
			}
		}

		public int count_players
		{
			get
			{
				return this.AMovP.Length;
			}
		}

		public int count_submaps
		{
			get
			{
				return this.ASubMaps.Length;
			}
		}

		public int count_carryable_bcc
		{
			get
			{
				if (this.ABcCon == null)
				{
					return 0;
				}
				return this.ABcCon.Count;
			}
		}

		public bool is_dark
		{
			get
			{
				return this.Meta != null && this.Meta.GetB("dark", false);
			}
		}

		public bool isTempMode()
		{
			return this.mode == MAPMODE.TEMP_WHOLE || this.mode == MAPMODE.TEMP;
		}

		public static bool isTempMode(MAPMODE mode)
		{
			return mode == MAPMODE.TEMP_WHOLE || mode == MAPMODE.TEMP;
		}

		public bool isPlayerFacingEnemy()
		{
			for (int i = this.AMovP.Length - 1; i >= 0; i--)
			{
				if (this.AMovP[i].isFacingEnemy())
				{
					return true;
				}
			}
			return false;
		}

		public M2MoverPr getPlayerFromEvent(string key)
		{
			if (TX.noe(key))
			{
				return this.Pr;
			}
			for (int i = this.AMovP.Length - 1; i >= 0; i--)
			{
				M2MoverPr m2MoverPr = this.AMovP[i];
				if (m2MoverPr.key == key)
				{
					return m2MoverPr;
				}
			}
			global::XX.X.dl("プレイヤー " + key + " が見つかりません ", null, false, false);
			return this.Pr;
		}

		public M2PxlAnimator getPxlAnimator(M2Mover Mv)
		{
			for (int i = 0; i < this.pxlanim_count; i++)
			{
				M2PxlAnimator m2PxlAnimator = this.APxlAnim[i];
				if (m2PxlAnimator.get_Mv() == Mv)
				{
					return m2PxlAnimator;
				}
			}
			return null;
		}

		public M2PxlAnimator PxlAnimatorReinsert(M2PxlAnimator Pxl)
		{
			int num = global::XX.X.isinC<M2PxlAnimator>(this.APxlAnim, Pxl, this.pxlanim_count);
			if (num >= 0 && num < this.pxlanim_count - 1)
			{
				global::XX.X.shiftNotInput<M2PxlAnimator>(this.APxlAnim, 1, num, this.pxlanim_count);
				this.APxlAnim[this.pxlanim_count - 1] = Pxl;
			}
			return null;
		}

		public M2BlockColliderContainer getCarryableBCCByIndex(int i)
		{
			return this.ABcCon[i];
		}

		public bool closed
		{
			get
			{
				return this.AMovP == null;
			}
		}

		public static bool editor_decline_lighting
		{
			get
			{
				return false;
			}
		}

		public void getMapBodyContentReader(ref CsvReader CR, ref ByteArray BaLoad)
		{
			if (this.BaLoad != null)
			{
				BaLoad = this.BaLoad;
				return;
			}
			CR = this.CR;
		}

		public bool isJustOpened
		{
			get
			{
				return this.t_load_cover <= 20;
			}
		}

		public bool apply_chip_effect
		{
			get
			{
				if (Map2d.editor_decline_lighting)
				{
					return false;
				}
				Dungeon dungeon = ((this.SubMapData != null) ? this.SubMapData.getBaseMap().Dgn : this.Dgn);
				return dungeon != null && dungeon.key != "_editor";
			}
		}

		public string getTag(GameObject Gob)
		{
			if (this.OGob_tags == null)
			{
				return null;
			}
			string text;
			if (this.OGob_tags.TryGetValue(Gob, out text))
			{
				return text;
			}
			return this.OGob_tags[Gob] = Gob.tag;
		}

		public void setTag(GameObject Gob, string g)
		{
			if (this.OGob_tags == null)
			{
				return;
			}
			this.OGob_tags[Gob] = g;
			Gob.tag = g;
		}

		public M2Mover getMv(int i)
		{
			if (!global::XX.X.BTW(0f, (float)i, (float)this.AMov.Length))
			{
				return null;
			}
			return this.AMov[i];
		}

		public M2MoverPr getPr(int i)
		{
			if (!global::XX.X.BTW(0f, (float)i, (float)this.AMovP.Length))
			{
				return null;
			}
			return this.AMovP[i];
		}

		public M2MoverPr getKeyPr()
		{
			return this.MovP;
		}

		public M2Puts getChipAtLayerByIndex(int layind, int cind)
		{
			if (!global::XX.X.BTW(0f, (float)layind, (float)this.ALay.Length))
			{
				return null;
			}
			return this.ALay[layind].getChipByIndex(cind);
		}

		public bool Gob2Phys(GameObject Gob, out M2Phys Phy)
		{
			if (this.OGob2Physics != null && this.OGob2Physics.TryGetValue(Gob, out Phy))
			{
				return true;
			}
			Phy = null;
			return false;
		}

		public M2MapLayer get_KeyLayer()
		{
			return this.KeyLayer;
		}

		public M2SubMap[] getSubMapVector()
		{
			return this.ASubMaps;
		}

		public bool is_whole
		{
			get
			{
				return this.M2D.isWholeMap(this);
			}
		}

		public bool is_submap
		{
			get
			{
				return this.mode == MAPMODE.SUBMAP;
			}
		}

		public bool isGameMode()
		{
			return this.mode == MAPMODE.SUBMAP || this.mode == MAPMODE.NORMAL;
		}

		public void setSubMapVector(M2SubMap[] A, bool reopen = false)
		{
			if (reopen)
			{
				this.closeSubMaps(false);
			}
			this.ASubMaps = A;
			if (reopen)
			{
				this.openSubMaps();
			}
		}

		public GameObject gameObject
		{
			get
			{
				return this.Gob;
			}
		}

		public IEffectSetter getEffectTop()
		{
			if (this.EFT != null)
			{
				return this.EFT;
			}
			return this.M2D.getEffectTop();
		}

		public IEffectSetter getEffect()
		{
			return this.EF;
		}

		public IEffectSetter getEffectForChip()
		{
			return this.EFC;
		}

		public void destructEffectForChip()
		{
			if (this.EFC != null)
			{
				this.EFC.destruct();
				this.EFC = null;
			}
		}

		public META prepareMeta()
		{
			if (this.Meta == null)
			{
				this.prepared = true;
				if (this.Meta != null || (this.CR == null && this.BaLoad == null))
				{
					return this.Meta;
				}
				if (this.CR != null)
				{
					this.CR.seek_set(0);
					while (this.CR.read())
					{
						if (this.CR.cmd == "comment")
						{
							this.Meta = new META(this.CR.slice_join(1, " ", ""));
							break;
						}
						if (this.CR.cmd == "layer" || this.CR.cmd == "c")
						{
							break;
						}
					}
				}
				else
				{
					this.Meta = new META();
				}
			}
			return this.Meta;
		}

		public override string ToString()
		{
			return "<Map2d> " + this.key + " :" + this.mode.ToString();
		}

		public M2MovRenderContainer MovRenderer
		{
			get
			{
				return this.M2D.Cam.MovRender;
			}
		}

		public void setReentryFlag(bool chip = true, bool grad = true, bool to_submap = false)
		{
			if (chip)
			{
				this.checkCachedSimplifiedImageExists();
				this.need_reentry_flag_ = true;
				this.update_mesh_flag_ |= 519 | ((this.MyDrawerWater != null) ? 1024 : 0);
			}
			if (grad)
			{
				this.need_reentry_gradation_flag = true;
			}
			if (to_submap && (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP))
			{
				int num = this.ASubMaps.Length;
				for (int i = 0; i < num; i++)
				{
					this.ASubMaps[i].getTargetMap().setReentryFlag(chip, grad, false);
				}
			}
		}

		public bool need_reentry_gradation_flag
		{
			get
			{
				return this.need_reentry_gradation_flag_;
			}
			set
			{
				this.need_reentry_gradation_flag_ = value;
				if (this.is_submap)
				{
					this.checkCachedSimplifiedImageExists();
				}
				if (value)
				{
					this.update_mesh_flag_ |= 240;
				}
			}
		}

		public bool need_reentry_flag
		{
			get
			{
				return this.need_reentry_flag_;
			}
			set
			{
				this.need_reentry_flag_ = value;
				this.checkCachedSimplifiedImageExists();
				if (value)
				{
					this.update_mesh_flag_ |= 519 | ((this.MyDrawerWater != null) ? 1024 : 0);
				}
			}
		}

		public bool need_init_action
		{
			get
			{
				return (this.update_mesh_flag_ & 2048) != 0;
			}
		}

		private void checkCachedSimplifiedImageExists()
		{
		}

		public void addUpdateMesh(int i, bool set_to_submap = false)
		{
			this.update_mesh_flag_ |= i;
			if (set_to_submap && (this.mode == MAPMODE.NORMAL || this.mode == MAPMODE.TEMP))
			{
				int num = this.ASubMaps.Length;
				for (int j = 0; j < num; j++)
				{
					this.ASubMaps[j].getTargetMap().addUpdateMesh(i, false);
				}
			}
		}

		public string get_individual_key()
		{
			return this.key;
		}

		public M2UnstabilizeMapItem getUnstabilizeDrawerContainer()
		{
			return this.Unstb;
		}

		public static uint Cp2b(M2Chip Dr)
		{
			return (uint)((Dr.mapx << 10) | Dr.mapy);
		}

		public static uint xy2b(int mapx, int mapy)
		{
			return (uint)((mapx << 10) | mapy);
		}

		public static uint b2x(uint b)
		{
			return (b >> 10) & 1023U;
		}

		public static uint b2y(uint b)
		{
			return b & 1023U;
		}

		public string get_comment()
		{
			return this.comment;
		}

		public bool load_cover_disable
		{
			get
			{
				return this.t_load_cover >= 0;
			}
		}

		public bool has_loaded_layer
		{
			get
			{
				return this.ALay != null;
			}
		}

		public byte get_binary_version()
		{
			return this.binary_version;
		}

		public PolygonCollider2D getCollider()
		{
			return this.MyCollider;
		}

		public float base_floort
		{
			get
			{
				if (this.SubMapData == null)
				{
					return this.floort;
				}
				return this.SubMapData.getBaseMap().floort;
			}
		}

		public readonly M2DBase M2D;

		public readonly float CLEN = 28f;

		public readonly float rCLEN = 0.035714287f;

		public static bool can_handle = true;

		public static float TSbase = 1f;

		public static float TScur = 1f;

		public static float TSbasepr = 1f;

		public string key;

		public float mover_scale = 2f;

		private bool handle = true;

		public MAPMODE mode;

		public int width = 48;

		public int height = 48;

		public int clms;

		public int rows;

		private int crop_ = -1;

		public Color32 bgcol;

		private string comment;

		public META Meta;

		public Dungeon Dgn;

		private M2SubMap SubMapData_;

		internal Color32 bgcol0;

		private M2Pt[,] AAPt;

		public static bool process_event_on_walk;

		public bool make_draw_arranger;

		private bool need_reentry_flag_;

		private bool need_reentry_gradation_flag_;

		public bool need_update_collider;

		private int update_mesh_flag_;

		public bool loadOnlyBasicFlag;

		public string[] Aadditional_for_editor;

		private M2MapLayer[] ALay;

		private CsvReader CR;

		private ByteArray BaLoad;

		private byte binary_version;

		private uint binary_content_position;

		private int t_load_cover;

		private const int T_LOADCOV_MAX = 60;

		private uint load_cover_col;

		private M2Mover[] AMov;

		private List<M2Phys> APhysics;

		private List<M2Mover> AMovRemoveStack;

		private BDic<GameObject, M2Phys> OGob2Physics;

		private List<M2BlockColliderContainer> ABcCon;

		public int mover_count;

		private M2MoverPr MovP;

		private M2MoverPr[] AMovP;

		private M2SubMap[] ASubMaps;

		private M2EventContainer EVC;

		private List<IActivatable> AActivatable;

		private M2PxlAnimator[] APxlAnim;

		private int pxlanim_count;

		public uint cmd_reload_flg;

		public const uint CMD_RELOAD = 1U;

		public const uint CMD_RELOAD_IMMEDIATE = 2U;

		public const uint CMD_RELOAD_NOLOAD = 4U;

		public const uint CMD_RELOAD_DESTRUCT_CHECK = 8U;

		public const uint CMD_RELOAD_LP = 16U;

		private M2Light[] ALight;

		private int light_cnt;

		private DRect[] ARcPreDefined;

		public uint cmd_loadevent_execute;

		public const uint CMDLE_LOAD_EXECUTE_IMMEDIATE = 1U;

		public const uint CMDLE_EXISTS_LOAD = 2U;

		public const uint CMDLE_ALWAYS = 4U;

		public const uint CMDLE_DONOT = 8U;

		public float floort;

		internal M2MapLayer KeyLayer;

		private List<IRunAndDestroy> ARunningObject;

		private List<M2EventItem> AEvLoadOnce;

		private M2EventCommand[] AEvStack;

		private bool stack_to_map;

		private int mover_running_i = -1;

		public static M2Puts[] ABufPuts;

		private BDic<GameObject, string> OGob_tags;

		private PolygonCollider2D MyCollider;

		public M2BlockColliderContainer BCC;

		private int redrawing_chip_count;

		public List<ITortureListener> ATortureListener;

		private M2Puts[] ARedrawingChip;

		private IM2TalkableObject TalkTarget_;

		private GameObject Gob;

		private IEffectSetter EF;

		private IEffectSetter EFT;

		private IEffectSetter EFC;

		public M2DrawBinderContainer EFD;

		public M2DrawBinderContainer EFDT;

		public M2DrawBinderContainer EFDC;

		public M2DropObjectContainer DropCon;

		public NearManager NM;

		public M2DmgCounterContainer DmgCntCon;

		public static PAUSER PoserForTimeStop;

		public const float Z_UCOL = 900f;

		public const float Z_SUB_SKY = 600f;

		public const float Z_UGRD = 520f;

		public const float Z_BACK = 500f;

		public const float Z_SUB_BACK = 460f;

		public const float Z_CAM_RENDERED = 440f;

		public const float Z_CAM_RENDERED_FINALIZE = 440f;

		public const float Z_GROUND = 400f;

		public const float Z_WATER = 370f;

		public const float Z_WATER_MESH = 370f;

		public const float Z_TOP = 340f;

		public const float Z_TOPLIGHT = 320f;

		public const float Z_EF_BOTTOM = 315f;

		public const float Z_TT = 310f;

		public const float Z_MV = 300f;

		public const float Z_EF_TOP = 120f;

		public const float Z_TT_FRONT = 130f;

		public const float Z_LIGHT = 805f;

		public const float Z_SUB_GROUND = 70f;

		public const float Z_EFT_BOTTOM = 55f;

		public const float Z_WATER_REFLECT_SURFACE = 40f;

		public const float Z_SUB_TOP = 45f;

		public const float Z_EFT_TOP = 35f;

		public const float Z_PE = 31f;

		public const float Z_EVENT_BOX = 20f;

		public const float Z_LETTERBOX = 10f;

		public const float Z_FILTER_GAMEOVER = 9f;

		public const int MESHBIT_BGCOL = 8;

		public const int MESHBIT_B = 1;

		public const int MESHBIT_G = 2;

		public const int MESHBIT_T = 4;

		public const int MESHBIT_LT = 512;

		public const int MESHBIT_WATER = 1024;

		public const int MESHBIT_UGRD = 16;

		public const int MESHBIT_BGRD = 32;

		public const int MESHBIT_GGRD = 64;

		public const int MESHBIT_TGRD = 128;

		public const int MESHBIT__ALL = 9983;

		public const int MESHBIT_REDRAWING = 256;

		public const int MESHBIT_INIT_ACTION = 2048;

		public const int MESHBIT_BLURED_IMAGE = 4096;

		public const int MESHBIT_TT = 8192;

		public const int MESHBIT_SPLICE_MESH = 16384;

		private M2BlurMeshDrawer BlurDraw;

		public TransformMem EditorTransform;

		private M2MeshContainer MMRD;

		public MdMap MyDrawerUCol;

		public MdMap MyDrawerB;

		public MdMap MyDrawerG;

		public MdMap MyDrawerT;

		public MdMap MyDrawerTT;

		public MdMap MyDrawerL;

		public MdMap MyDrawerUGrd;

		public MdMap MyDrawerBGrd;

		public MdMap MyDrawerGGrd;

		public MdMap MyDrawerTGrd;

		public MdMap MyDrawerWater;

		public M2UnstabilizeMapItem Unstb;

		public const int mesh_count = 12;

		private const string string_MapMain_Ev = "MapMain-Ev";

		private const string string_TSrunning = "TS running";

		private const string string_MvrunPre_ = "Mv runPre - ";

		private const string string_PxlAnim = "PxlAnim";

		private const string string_Runner__ = "Runner - ";

		private const string string_CameraRun = "CameraRun";

		private const string string_MvrunPost_ = "Mv runPost - ";

		private const string string_DropCon = "DropCon";

		private const string string_Torture = "Torture";

		private const string string_RunEf = "RunEf";

		private const string string_PreDraw = "PreDraw";

		private const string string_LightDraw = "LightDraw";

		private static RaycastHit2D[] AHit;

		private static ContactFilter2D Flt;

		private static int ulayermask;

		public static List<M2BlockColliderContainer.BCCHitInfo> APosInfoBuf = new List<M2BlockColliderContainer.BCCHitInfo>(1);

		private static List<M2BlockColliderContainer.BCCInfo> ABccInfoCache = new List<M2BlockColliderContainer.BCCInfo>(4);

		public struct LpLoader
		{
			public string key;

			public string comment;

			public string command;
		}

		public enum BIN_CTG
		{
			NAME = 1,
			SIZE,
			BGCOL,
			COMMENT,
			CSP,
			EDITOR_ADDITIONAL,
			MESH_RECT,
			LAYER_REVERSE,
			LAYER_HEADER = 80,
			PAT_CHANGE,
			CP,
			PIC,
			LP,
			GRD,
			SM
		}

		public enum CONNECTIMG : byte
		{
			ASSIGN,
			DELETE,
			DELETE_TEMP
		}

		public delegate IFootable FnReplaceFootTarget(M2FootManager FootD);

		private delegate bool FnDropCfgCheck(ref int cfg, int x, int y);
	}
}
