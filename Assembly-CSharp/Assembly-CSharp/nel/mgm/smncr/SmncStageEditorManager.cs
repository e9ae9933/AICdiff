using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class SmncStageEditorManager
	{
		public SmncStageEditorManager(NelM2DBase _M2D, SmncStageEditorManager.TYPE _target_type)
		{
			this.M2D = _M2D;
			this.target_type_ = _target_type;
			this.OSteo = new BDic<string, SmncStageEditorManager.StgObject>();
		}

		public SmncStageEditorManager.TYPE target_type
		{
			get
			{
				return this.target_type_;
			}
			set
			{
				if (this.target_type_ != value)
				{
					this.target_type_ = value;
					this.need_load = true;
				}
			}
		}

		public void reload(bool force = false)
		{
			if (this.need_load || force)
			{
				this.need_load = false;
				this.need_prepare_map = true;
				CsvReader csvReader = new CsvReader(TX.getResource("Data/mgm_smnc/smnc_stage_object", ".csv", false), CsvReader.RegSpace, false);
				this.OSteo.Clear();
				SmncStageEditorManager.TYPE type = SmncStageEditorManager.TYPE._ALL;
				SmncStageEditorManager.StgObject stgObject = default(SmncStageEditorManager.StgObject);
				while (csvReader.read())
				{
					if (csvReader.cmd == "##TYPE")
					{
						type = (SmncStageEditorManager.TYPE)0;
						for (int i = 1; i < csvReader.clength; i++)
						{
							SmncStageEditorManager.TYPE type2;
							if (FEnum<SmncStageEditorManager.TYPE>.TryParse(csvReader.getIndex(i), out type2, true))
							{
								type |= type2;
							}
							else
							{
								csvReader.tError("不明なTYPE: " + csvReader.getIndex(i));
							}
						}
					}
					else if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
					{
						if (stgObject.valid)
						{
							this.OSteo[stgObject.key] = stgObject;
							stgObject = default(SmncStageEditorManager.StgObject);
						}
						if ((this.target_type & type) == this.target_type)
						{
							string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
							Map2d map2d = this.M2D.Get(index, true);
							if (map2d == null)
							{
								csvReader.tError("不明なMp: " + index);
							}
							else
							{
								stgObject = new SmncStageEditorManager.StgObject(map2d);
								this.M2D.initMapMaterialASync(map2d, 0, false);
							}
						}
					}
					else if (stgObject.valid)
					{
						if (csvReader.cmd == "flipable")
						{
							stgObject.flipable = csvReader.Nm(1, 0f) != 0f;
						}
						else
						{
							csvReader.tError("不明なコマンド " + csvReader.cmd);
						}
					}
				}
				if (stgObject.valid)
				{
					this.OSteo[stgObject.key] = stgObject;
					stgObject = default(SmncStageEditorManager.StgObject);
				}
			}
		}

		public bool initMap()
		{
			this.reload(false);
			if (this.M2D.transferring_game_stopping || this.M2D.isLoaderLoading())
			{
				return false;
			}
			if (this.need_prepare_map)
			{
				this.need_prepare_map = false;
				List<SmncStageEditorManager.StgObject> list = new List<SmncStageEditorManager.StgObject>(this.OSteo.Count);
				foreach (KeyValuePair<string, SmncStageEditorManager.StgObject> keyValuePair in this.OSteo)
				{
					SmncStageEditorManager.StgObject value = keyValuePair.Value;
					value.initMap();
					list.Add(value);
				}
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					SmncStageEditorManager.StgObject stgObject = list[i];
					this.OSteo[stgObject.key] = stgObject;
				}
			}
			return true;
		}

		public readonly NelM2DBase M2D;

		public readonly BDic<string, SmncStageEditorManager.StgObject> OSteo;

		private SmncStageEditorManager.TYPE target_type_;

		private bool need_prepare_map;

		private bool need_load = true;

		[Flags]
		public enum TYPE
		{
			GARAGE = 1,
			TD = 2,
			_ALL = 3
		}

		public struct StgObject
		{
			public StgObject(Map2d _Mp)
			{
				this.x = 0;
				this.y = 0;
				this.flip = (this.on_ground = false);
				this.count_chips = 0;
				this.flipable = true;
				this.Mp = _Mp;
				this.tx_key = null;
			}

			public void copyDictData(SmncStageEditorManager.StgObject Data)
			{
				this.flipable = Data.flipable;
				this.on_ground = Data.on_ground;
			}

			public bool map_initted
			{
				get
				{
					return this.tx_key != null;
				}
			}

			public void initMap()
			{
				if (this.tx_key != null)
				{
					return;
				}
				this.tx_key = "UiSmnCreator" + this.Mp.key;
				this.count_chips = 0;
				this.Mp.open(null, MAPMODE.LOAD_AND_CFG, null);
				for (int i = this.Mp.count_layers - 1; i >= 0; i--)
				{
					M2MapLayer layer = this.Mp.getLayer(i);
					int num = layer.count_chips;
					this.count_chips += num;
					for (int j = 0; j < num; j++)
					{
						M2Puts chipByIndex = layer.getChipByIndex(j);
						if (this.Mp.M2D.IMGS.Atlas.prepareChipImageDirectory(chipByIndex.Img, true))
						{
							this.Mp.M2D.IMGS.Atlas.alloc_m2d_loader_inactive = true;
						}
					}
				}
			}

			public int priority
			{
				get
				{
					if (!TX.isStart(this.key, "_smnc_generate_", 0))
					{
						return 1;
					}
					return 0;
				}
			}

			public string localized_name
			{
				get
				{
					return TX.Get(this.tx_key, "");
				}
			}

			public int clms
			{
				get
				{
					if (this.Mp == null)
					{
						return 0;
					}
					return this.Mp.clms;
				}
			}

			public int rows
			{
				get
				{
					if (this.Mp == null)
					{
						return 0;
					}
					return this.Mp.rows;
				}
			}

			public string key
			{
				get
				{
					if (this.Mp != null)
					{
						return this.Mp.key;
					}
					return null;
				}
			}

			public bool valid
			{
				get
				{
					return this.Mp != null;
				}
			}

			public M2Puts getPuts(int chip_i)
			{
				int count_layers = this.Mp.count_layers;
				for (int i = 0; i < count_layers; i++)
				{
					M2MapLayer layer = this.Mp.getLayer(i);
					if (chip_i < layer.count_chips)
					{
						return layer.getChipByIndex(chip_i);
					}
					chip_i -= layer.count_chips;
				}
				return null;
			}

			public float cx
			{
				get
				{
					return (float)this.x + (float)this.Mp.clms * 0.5f;
				}
			}

			public float cy
			{
				get
				{
					return (float)this.y + (float)this.Mp.rows * 0.5f;
				}
			}

			public Vector2 getMapPos(M2LabelPoint LpArea)
			{
				return new Vector2((float)(LpArea.mapx + this.x), (float)(LpArea.mapy + this.y));
			}

			public Vector2 getMapCenterPos(M2LabelPoint LpArea)
			{
				return new Vector2((float)LpArea.mapx + this.cx, (float)LpArea.mapy + this.cy);
			}

			public int getConfig(M2LabelPoint LpArea, int mapx, int mapy)
			{
				int num = mapx - (LpArea.mapx + this.x);
				int num2 = mapy - (LpArea.mapy + this.y);
				if (this.flip)
				{
					num = this.Mp.clms - 1 - num;
				}
				if (X.BTW(0f, (float)num, (float)this.Mp.clms) && X.BTW(0f, (float)num2, (float)this.Mp.rows))
				{
					return this.Mp.getConfig(num, num2);
				}
				return 0;
			}

			public static SmncStageEditorManager.StgObject readBinary(ByteReader Ba, NelM2DBase M2D, int vers)
			{
				SmncStageEditorManager.StgObject stgObject = default(SmncStageEditorManager.StgObject);
				Map2d map2d = M2D.Get(Ba.readPascalString("utf-8", false), false);
				int num = (int)Ba.readShort();
				int num2 = (int)Ba.readShort();
				bool flag = Ba.readBoolean();
				if (map2d != null)
				{
					stgObject = new SmncStageEditorManager.StgObject(map2d)
					{
						x = num,
						y = num2,
						flip = flag
					};
				}
				return stgObject;
			}

			public bool isSame(SmncStageEditorManager.StgObject Src)
			{
				return this.x == Src.x && this.y == Src.y && this.key == Src.key && this.flip == Src.flip;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writePascalString(this.key, "utf-8");
				Ba.writeShort((short)this.x);
				Ba.writeShort((short)this.y);
				Ba.writeBool(this.flip);
			}

			public override string ToString()
			{
				return "Stgo-" + ((this.Mp == null) ? "(empty)" : string.Concat(new string[]
				{
					this.Mp.key,
					" ",
					this.x.ToString(),
					",",
					this.y.ToString()
				}));
			}

			public Map2d Mp;

			public int x;

			public int y;

			public bool flip;

			public bool flipable;

			private string tx_key;

			public int count_chips;

			public bool on_ground;
		}
	}
}
