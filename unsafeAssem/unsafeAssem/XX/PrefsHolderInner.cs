using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class PrefsHolderInner : PrefsHolder
	{
		public PrefsHolderInner()
		{
			this.Ofloat = new BDic<string, float>();
			this.Ostring = new BDic<string, string>();
			this.OBa = new BDic<string, byte[]>();
		}

		public override float GetFloat(string key, float _default = 0f)
		{
			float num;
			if (this.Ofloat.TryGetValue(key, out num))
			{
				return num;
			}
			return _default;
		}

		public override string GetString(string key, string _default = null)
		{
			string text;
			if (this.Ostring.TryGetValue(key, out text))
			{
				return text;
			}
			return _default;
		}

		public override void SetFloat(string key, float _value)
		{
			this.Ofloat[key] = _value;
		}

		public override void SetString(string key, string _value)
		{
			this.Ostring[key] = _value;
		}

		public override void SetBytes(string key, byte[] ABa)
		{
			this.OBa[key] = ABa;
		}

		public override bool GetBytes(string key, out byte[] Aba, bool convert = true)
		{
			Aba = null;
			byte[] array;
			if (this.OBa.TryGetValue(key, out array))
			{
				Aba = array;
				return true;
			}
			return false;
		}

		public override void DeleteAll()
		{
			this.Ofloat.Clear();
			this.Ostring.Clear();
			this.OBa.Clear();
		}

		public override void DeleteKey(string key)
		{
			this.Ofloat.Remove(key);
			this.Ostring.Remove(key);
			this.OBa.Remove(key);
		}

		public void readFromBytes(ByteArray Ba)
		{
			this.DeleteAll();
			uint num = (uint)Ba.readUNumber(this.len_count_for_bytes);
			for (uint num2 = 0U; num2 < num; num2 += 1U)
			{
				string text = Ba.readPascalString("utf-8", false);
				float num3 = Ba.readFloat();
				this.Ofloat[text] = num3;
			}
			num = (uint)Ba.readUNumber(this.len_count_for_bytes);
			for (uint num4 = 0U; num4 < num; num4 += 1U)
			{
				string text2 = Ba.readPascalString("utf-8", false);
				string text3 = Ba.readString("utf-8", false);
				this.Ostring[text2] = text3;
			}
			num = (uint)Ba.readUNumber(this.len_count_for_bytes);
			for (uint num5 = 0U; num5 < num; num5 += 1U)
			{
				string text4 = Ba.readPascalString("utf-8", false);
				byte[] array = null;
				Ba.readByteA(ref array, false);
				this.OBa[text4] = array;
			}
		}

		public void writeToBytes(ByteArray Ba)
		{
			Ba.writeUNumber((ulong)((long)this.Ofloat.Count), this.len_count_for_bytes);
			foreach (KeyValuePair<string, float> keyValuePair in this.Ofloat)
			{
				Ba.writePascalString(keyValuePair.Key, "utf-8");
				Ba.writeFloat(keyValuePair.Value);
			}
			Ba.writeUNumber((ulong)((long)this.Ostring.Count), this.len_count_for_bytes);
			foreach (KeyValuePair<string, string> keyValuePair2 in this.Ostring)
			{
				Ba.writePascalString(keyValuePair2.Key, "utf-8");
				Ba.writeString(keyValuePair2.Value, "utf-8");
			}
			Ba.writeUNumber((ulong)((long)this.OBa.Count), this.len_count_for_bytes);
			foreach (KeyValuePair<string, byte[]> keyValuePair3 in this.OBa)
			{
				Ba.writePascalString(keyValuePair3.Key, "utf-8");
				Ba.writeByteA(keyValuePair3.Value, 2);
			}
		}

		private BDic<string, float> Ofloat;

		private BDic<string, string> Ostring;

		private BDic<string, byte[]> OBa;

		public int len_count_for_bytes = 4;
	}
}
