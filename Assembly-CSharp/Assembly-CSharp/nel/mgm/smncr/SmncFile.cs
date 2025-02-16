using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using SFB;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class SmncFile
	{
		public SmncFile(int enid_capacity = 4, int stgo_capacity = 4, int id2max_capacity = 4, int plant_capacity = 4, int enattr_capacity = 4)
		{
			this.Aen_list = new List<SmncFile.EnemyInfo>(enid_capacity);
			this.Astgo = new List<SmncStageEditorManager.StgObject>(stgo_capacity);
			this.Oid2appear = new BDic<ENEMYID, int>(id2max_capacity);
			this.Aplant = new List<SmncFile.PlantInfo>(plant_capacity);
			this.Oen_attr_count = new BDic<ENATTR, int>(enattr_capacity);
		}

		public SmncFile(SmncFile Src)
		{
			this.Aen_list = new List<SmncFile.EnemyInfo>(Src.Aen_list);
			this.Astgo = new List<SmncStageEditorManager.StgObject>(Src.Astgo);
			this.Aplant = new List<SmncFile.PlantInfo>(Src.Aplant);
			this.Oid2appear = new BDic<ENEMYID, int>(Src.Oid2appear);
			this.Oen_attr_count = new BDic<ENATTR, int>(Src.Oen_attr_count);
			this.maxappear = Src.maxappear;
		}

		public void openFile()
		{
			for (int i = this.Astgo.Count - 1; i >= 0; i--)
			{
				SmncStageEditorManager.StgObject stgObject = this.Astgo[i];
				if (!stgObject.map_initted)
				{
					stgObject.initMap();
					this.Astgo[i] = stgObject;
				}
			}
		}

		public SmncStageEditorManager.StgObject FindStgo(string key, out int index)
		{
			index = -1;
			int count = this.Astgo.Count;
			for (int i = 0; i < count; i++)
			{
				SmncStageEditorManager.StgObject stgObject = this.Astgo[i];
				if (stgObject.key == key)
				{
					index = i;
					return stgObject;
				}
			}
			return default(SmncStageEditorManager.StgObject);
		}

		public SmncFile.EnemyInfo FindEmemy(ENEMYID id, out int index)
		{
			index = -1;
			int count = this.Aen_list.Count;
			for (int i = 0; i < count; i++)
			{
				SmncFile.EnemyInfo enemyInfo = this.Aen_list[i];
				if (enemyInfo.id == id)
				{
					index = i;
					return enemyInfo;
				}
			}
			return default(SmncFile.EnemyInfo);
		}

		public static SmncFile readFromBytes(ByteReader Ba, NelM2DBase M2D, int vers)
		{
			SmncFile smncFile;
			if (vers >= 3)
			{
				int num = Ba.readByte();
				int num2 = Ba.readByte();
				int num3 = Ba.readByte();
				int num4 = Ba.readByte();
				int num5 = Ba.readByte();
				smncFile = new SmncFile(num2, num, num3, num4, num5);
				smncFile.maxappear = Ba.readByte();
				for (int i = 0; i < num; i++)
				{
					SmncStageEditorManager.StgObject stgObject = SmncStageEditorManager.StgObject.readBinary(Ba, M2D, vers);
					if (stgObject.valid)
					{
						smncFile.Astgo.Add(stgObject);
					}
				}
				for (int j = 0; j < num2; j++)
				{
					SmncFile.EnemyInfo enemyInfo = SmncFile.EnemyInfo.readBinary(Ba, vers);
					if (enemyInfo.valid)
					{
						smncFile.Aen_list.Add(enemyInfo);
					}
				}
				for (int k = 0; k < num3; k++)
				{
					ENEMYID enemyid = (ENEMYID)Ba.readInt();
					int num6 = Ba.readByte();
					smncFile.Oid2appear[enemyid] = num6;
				}
				for (int l = 0; l < num4; l++)
				{
					SmncFile.PlantInfo plantInfo = new SmncFile.PlantInfo((int)Ba.readShort(), (int)Ba.readShort(), Ba.readUShort());
					smncFile.Aplant.Add(plantInfo);
				}
				for (int m = 0; m < num5; m++)
				{
					ENATTR enattr = (ENATTR)Ba.readInt();
					int num7 = Ba.readByte();
					smncFile.Oen_attr_count[enattr] = num7;
				}
				if (vers >= 4)
				{
					if (vers >= 5)
					{
						smncFile.reward_flags = (SmncFile.REWARD)Ba.readUByte();
					}
					smncFile.id_count = Ba.readUShort();
					smncFile.dangerousness = (byte)Ba.readByte();
					smncFile.fix_nattr = Ba.readBoolean();
					smncFile.weather_bits = Ba.readUInt();
					smncFile.rand_seed = Ba.readUInt();
				}
			}
			else
			{
				smncFile = new SmncFile(4, 4, 4, 4, 4);
			}
			smncFile.need_fine_nattr_valid = true;
			return smncFile;
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(this.Astgo.Count);
			Ba.writeByte(this.Aen_list.Count);
			Ba.writeByte(this.Oid2appear.Count);
			Ba.writeByte(this.Aplant.Count);
			Ba.writeByte(this.Oen_attr_count.Count);
			Ba.writeByte(this.maxappear);
			int num = this.Astgo.Count;
			for (int i = 0; i < num; i++)
			{
				this.Astgo[i].writeBinaryTo(Ba);
			}
			num = this.Aen_list.Count;
			for (int j = 0; j < num; j++)
			{
				this.Aen_list[j].writeBinaryTo(Ba);
			}
			foreach (KeyValuePair<ENEMYID, int> keyValuePair in this.Oid2appear)
			{
				Ba.writeInt((int)keyValuePair.Key);
				Ba.writeByte(keyValuePair.Value);
			}
			num = this.Aplant.Count;
			for (int k = 0; k < num; k++)
			{
				SmncFile.PlantInfo plantInfo = this.Aplant[k];
				Ba.writeShort((short)plantInfo.x);
				Ba.writeShort((short)plantInfo.y);
				Ba.writeUShort(plantInfo.id);
			}
			foreach (KeyValuePair<ENATTR, int> keyValuePair2 in this.Oen_attr_count)
			{
				Ba.writeInt((int)keyValuePair2.Key);
				Ba.writeByte(keyValuePair2.Value);
			}
			Ba.writeByte((int)this.reward_flags);
			Ba.writeUShort(this.id_count);
			Ba.writeByte((int)this.dangerousness);
			Ba.writeBool(this.fix_nattr);
			Ba.writeUInt(this.weather_bits);
			Ba.writeUInt(this.rand_seed);
		}

		public bool need_fine_nattr_valid
		{
			get
			{
				if (this.need_fine_nattr_valid_ == 2)
				{
					this.need_fine_nattr_valid_ = 1;
					ENATTR enattr = ENATTR.NORMAL;
					foreach (KeyValuePair<ENATTR, int> keyValuePair in this.Oen_attr_count)
					{
						if (keyValuePair.Value > 0)
						{
							enattr |= keyValuePair.Key;
						}
					}
					for (int i = this.Aen_list.Count - 1; i >= 0; i--)
					{
						bool flag;
						NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId(this.Aen_list[i].id, out flag);
						if ((enattr & typeAndId.nattr_decline) != ENATTR.NORMAL)
						{
							this.need_fine_nattr_valid_ = 0;
							break;
						}
					}
				}
				return this.need_fine_nattr_valid_ == 1;
			}
			set
			{
				if (value)
				{
					this.need_fine_nattr_valid_ = 2;
				}
			}
		}

		public static void readFromFile(NelM2DBase M2D, Action<SmncFile, string> FD_Finished)
		{
			IN.clearPushDown(true);
			try
			{
				StandaloneFileBrowser.OpenFilePanelAsync(TX.Get("Smnc_load_file", ""), null, "aicsmnc", false, delegate(string[] Astr)
				{
					if (Astr == null || Astr.Length == 0)
					{
						FD_Finished(null, "Canceled.");
						return;
					}
					byte[] array = NKT.readSpecificFileBinary(Astr[0], 0, 0, false);
					if (array == null)
					{
						FD_Finished(null, "Read Error");
						return;
					}
					SmncFile.readFromFile(new ByteArray(array, false, false), M2D, FD_Finished);
				});
			}
			catch (Exception ex)
			{
				FD_Finished(null, ex.Message);
			}
		}

		private static void readFromFile(ByteArray Ba, NelM2DBase M2D, Action<SmncFile, string> FD_Finished)
		{
			try
			{
				if (Ba.readMultiByte("tigrina chan no hutomomo tyokkei 440m by hashinomizuha".Length, "utf-8") != "tigrina chan no hutomomo tyokkei 440m by hashinomizuha")
				{
					throw new Exception("HeaderError");
				}
				int num = (int)Ba.readUByte();
				SmncFile smncFile = SmncFile.readFromBytes(Ba, M2D, num);
				if (smncFile == null)
				{
					FD_Finished(null, "Loading Error");
				}
				else
				{
					FD_Finished(smncFile, null);
				}
			}
			catch (Exception ex)
			{
				FD_Finished(null, ex.Message);
			}
		}

		public void saveToFile(int index, Action<string, string> FD_Finished)
		{
			IN.clearPushDown(true);
			try
			{
				StandaloneFileBrowser.SaveFilePanelAsync(TX.Get("Smnc_save_file", ""), null, "GarageSimulation_" + (index + 1).ToString(), "aicsmnc", delegate(string _path)
				{
					if (TX.noe(_path))
					{
						FD_Finished(null, "Canceled.");
						return;
					}
					try
					{
						ByteArray byteArray = new ByteArray(0U);
						byteArray.writeMultiByte("tigrina chan no hutomomo tyokkei 440m by hashinomizuha", "utf-8");
						byteArray.writeByte(6);
						this.writeBinaryTo(byteArray);
						NKT.writeSpecificFileBinary(_path, byteArray, false);
						FD_Finished(_path, null);
					}
					catch (Exception ex2)
					{
						FD_Finished(null, ex2.Message);
					}
				});
			}
			catch (Exception ex)
			{
				FD_Finished(null, ex.Message);
			}
		}

		public const string local_file_header = "tigrina chan no hutomomo tyokkei 440m by hashinomizuha";

		public const string extension = "aicsmnc";

		public const string default_name_header = "GarageSimulation_";

		public const string prompt_canceled = "Canceled.";

		public int maxappear = 4;

		public byte dangerousness;

		public bool fix_nattr;

		public uint weather_bits = 1U;

		public SmncFile.REWARD reward_flags;

		public readonly List<SmncFile.EnemyInfo> Aen_list;

		public readonly List<SmncFile.PlantInfo> Aplant;

		public readonly List<SmncStageEditorManager.StgObject> Astgo;

		public readonly BDic<ENATTR, int> Oen_attr_count;

		public ushort id_count;

		public uint rand_seed;

		public byte need_fine_nattr_valid_ = 2;

		public BDic<ENEMYID, int> Oid2appear;

		public struct EnemyInfo
		{
			public void copyDescTo(STB Stb)
			{
				Stb.Add(NDAT.getEnemyName(this.id, false));
				Stb.Add(" x", this.count, "");
			}

			public bool valid
			{
				get
				{
					return this.count > 0;
				}
			}

			public static SmncFile.EnemyInfo readBinary(ByteReader Ba, int vers)
			{
				return new SmncFile.EnemyInfo
				{
					id = (ENEMYID)Ba.readInt(),
					count = Ba.readByte(),
					mp_min100 = Ba.readByte(),
					mp_max100 = Ba.readByte()
				};
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeInt((int)this.id);
				Ba.writeByte(this.count);
				Ba.writeByte(this.mp_min100);
				Ba.writeByte(this.mp_max100);
			}

			public string id_tostr
			{
				get
				{
					return FEnum<ENEMYID>.ToStr(this.id & (ENEMYID)2147483647U);
				}
			}

			public bool is_od
			{
				get
				{
					return (this.id & (ENEMYID)2147483648U) > (ENEMYID)0U;
				}
			}

			public ENEMYID id;

			public int count;

			public int mp_min100;

			public int mp_max100;
		}

		public struct PlantInfo
		{
			public PlantInfo(int _x, int _y, ushort _id)
			{
				this.x = _x;
				this.y = _y;
				this.id = _id;
				this.attach_index = -1;
			}

			public Vector2 getMapPos(M2LabelPoint LpArea)
			{
				return new Vector2((float)(LpArea.mapx + this.x) + 0.5f, (float)(LpArea.mapy + this.y + 1));
			}

			public M2ChipImage getImage(M2DBase M2D)
			{
				return M2D.IMGS.Get("mgplant/mgplant_" + ((int)(this.id % 11)).ToString());
			}

			public bool isSame(SmncFile.PlantInfo Src)
			{
				return this.x == Src.x && this.y == Src.y;
			}

			public bool is_flip
			{
				get
				{
					return this.id % 22 >= 11;
				}
			}

			public int x;

			public int y;

			public ushort id;

			public int attach_index;

			public const int MGPLANT_MAX = 11;

			public const string img_mgplant_header = "mgplant/mgplant_";
		}

		[Flags]
		public enum REWARD : byte
		{
			_UPDATE_ENEMIES_027 = 1,
			_UPDATE_ENEMIES_027_END = 2
		}
	}
}
