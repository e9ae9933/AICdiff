using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;

namespace XX.mobpxl
{
	internal class SkltPalette : BDic<string, MobPCCContainer.ACC>, IIdvName
	{
		internal SkltPalette(string _key, int capacity = 0)
			: base(capacity)
		{
		}

		public virtual string get_individual_key()
		{
			return null;
		}

		public bool isEmpty()
		{
			if (base.Count == 0)
			{
				return true;
			}
			foreach (KeyValuePair<string, MobPCCContainer.ACC> keyValuePair in this)
			{
				if (!keyValuePair.Value.is_empty)
				{
					return false;
				}
			}
			return true;
		}

		public SkltPalette mergeBasePalette(SkltPalette Plt)
		{
			if (Plt == null)
			{
				return this;
			}
			foreach (KeyValuePair<string, MobPCCContainer.ACC> keyValuePair in Plt)
			{
				if (MobGenerator.is_base_oacc_parts(keyValuePair.Key))
				{
					base[keyValuePair.Key] = keyValuePair.Value;
				}
			}
			return this;
		}

		internal virtual void readFromBytes(ByteArray Ba, int ARMX = -1, PxlCharacter Pcr = null)
		{
			if (ARMX < 0)
			{
				ARMX = Ba.readByte();
			}
			for (int i = 0; i < ARMX; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				MobPCCContainer.ACC acc = MobPCCContainer.ACC.readFromBytesACC(Ba, null);
				if (acc != null)
				{
					if (acc.PI == null && Pcr != null)
					{
						int partsInfoIndex = Pcr.getPartsInfoIndex(text, false);
						if (partsInfoIndex < 0)
						{
							goto IL_0056;
						}
						acc.PI = Pcr.APartsInfo[partsInfoIndex];
					}
					base[text] = acc;
				}
				IL_0056:;
			}
		}
	}
}
