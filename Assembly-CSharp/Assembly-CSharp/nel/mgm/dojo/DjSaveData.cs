using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace nel.mgm.dojo
{
	public class DjSaveData
	{
		public DjSaveData()
		{
			this.OSd = new BDic<string, DjSaveData.SD>(16);
		}

		public void clear()
		{
			this.OSd.Clear();
		}

		public DjSaveData.SD GetData(string key)
		{
			DjSaveData.SD sd;
			if (this.OSd.TryGetValue(key, out sd))
			{
				return sd;
			}
			return new DjSaveData.SD
			{
				minimum_miss = 99
			};
		}

		public void WriteData(string key, DjSaveData.SD Sd)
		{
			this.OSd[key] = Sd;
		}

		public void readFromBytes(ByteArray Ba)
		{
			this.clear();
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				DjSaveData.SD sd = default(DjSaveData.SD);
				sd.readFromBytes(Ba);
				this.OSd[text] = sd;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeUShort((ushort)this.OSd.Count);
			foreach (KeyValuePair<string, DjSaveData.SD> keyValuePair in this.OSd)
			{
				Ba.writePascalString(keyValuePair.Key, "utf-8");
				keyValuePair.Value.writeToBytes(Ba);
			}
		}

		public const int MAX_COUNT = 9999;

		private readonly BDic<string, DjSaveData.SD> OSd;

		public struct SD
		{
			public void readFromBytes(ByteArray Ba)
			{
				this.play_count = Ba.readUShort();
				this.win_count = Ba.readUShort();
				this.minimum_miss = Ba.readUByte();
			}

			public void writeToBytes(ByteArray Ba)
			{
				Ba.writeUShort(this.play_count);
				Ba.writeUShort(this.win_count);
				Ba.writeByte((int)this.minimum_miss);
			}

			public ushort play_count;

			public ushort win_count;

			public byte minimum_miss;
		}
	}
}
