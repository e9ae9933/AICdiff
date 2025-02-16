using System;
using System.Collections.Generic;
using Better;
using evt;
using PixelLiner;
using XX;

namespace nel
{
	public static class NelDwarfCharManager
	{
		public static bool loadPxl(bool reloading = false)
		{
			NelDwarfCharManager.Pc = PxlsLoader.getPxlCharacter("character_dwarf");
			if (NelDwarfCharManager.Pc != null)
			{
				if (!reloading)
				{
					return false;
				}
				PxlsLoader.disposeCharacter("character_dwarf", true);
				MTRX.releaseMI(NelDwarfCharManager.Pc, true);
			}
			NelDwarfCharManager.Pc = MTRX.loadMtiPxc("character_dwarf", "EvImg/character_dwarf.pxls", "NEL", false, true, false);
			EV.addExternalPxlsAfter("character_dwarf", NelDwarfCharManager.Pc);
			return true;
		}

		public static void reloadScript()
		{
			string resource = TX.getResource("Data/dwarf", ref NelDwarfCharManager.ResourceDate, ".csv", false, "Resources/");
			if (TX.noe(resource))
			{
				return;
			}
			NelDwarfCharManager.Pc = PxlsLoader.getPxlCharacter("character_dwarf");
			NelDwarfCharManager.AOchar = new List<BDic<string, NelDwarfCharManager.DwarfC>>();
			NelDwarfCharManager.Ochar1 = new BDic<char, NelDwarfCharManager.DwarfC>(100);
			CsvReader csvReader = new CsvReader(resource, CsvReader.RegSpace, false);
			PxlSequence pxlSequence = null;
			int num = 0;
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					PxlPose poseByName = NelDwarfCharManager.Pc.getPoseByName(index);
					pxlSequence = null;
					num = 0;
					if (poseByName == null)
					{
						csvReader.tError("不明なポーズ: " + index);
					}
					else
					{
						pxlSequence = poseByName.getSequence(0);
					}
				}
				else if (pxlSequence != null && csvReader.clength > 0)
				{
					PxlFrame frame = pxlSequence.getFrame(num++);
					for (int i = 0; i < csvReader.clength; i++)
					{
						string index2 = csvReader.getIndex(i);
						if (!TX.noe(index2))
						{
							int length = index2.Length;
							if (length == 1)
							{
								char c = index2[0];
								NelDwarfCharManager.Ochar1[c] = new NelDwarfCharManager.DwarfC(frame);
							}
							else
							{
								int num2 = length - 2;
								while (NelDwarfCharManager.AOchar.Count <= num2)
								{
									NelDwarfCharManager.AOchar.Add(new BDic<string, NelDwarfCharManager.DwarfC>(8));
								}
								NelDwarfCharManager.AOchar[num2][index2] = new NelDwarfCharManager.DwarfC(frame);
							}
						}
					}
				}
			}
		}

		public static bool getCharacter(STB Stb, int index, out PxlFrame PFReplace, out float scale)
		{
			char c = Stb[index];
			PFReplace = null;
			scale = 1f;
			if (c == ' ' || c == '\t')
			{
				return false;
			}
			if (NelDwarfCharManager.AOchar == null)
			{
				return false;
			}
			if (index == 0 || TX.isSpace(Stb[index - 1]))
			{
				for (int i = NelDwarfCharManager.AOchar.Count - 1; i >= 0; i--)
				{
					foreach (KeyValuePair<string, NelDwarfCharManager.DwarfC> keyValuePair in NelDwarfCharManager.AOchar[i])
					{
						if (Stb.isStart(keyValuePair.Key, index))
						{
							return true;
						}
					}
				}
			}
			NelDwarfCharManager.DwarfC dwarfC;
			if (NelDwarfCharManager.Ochar1.TryGetValue(c, out dwarfC))
			{
				PFReplace = dwarfC.PF;
				scale *= 0.38f;
				return true;
			}
			return false;
		}

		public static MImage getMI()
		{
			if (NelDwarfCharManager.Pc == null)
			{
				return null;
			}
			return MTRX.getMI(NelDwarfCharManager.Pc, false);
		}

		private static List<BDic<string, NelDwarfCharManager.DwarfC>> AOchar;

		private static BDic<char, NelDwarfCharManager.DwarfC> Ochar1;

		private static PxlCharacter Pc;

		private const string script_resources_dir = "Data/dwarf";

		private const string pxl_name = "character_dwarf";

		private static DateTime ResourceDate;

		public struct DwarfC
		{
			public DwarfC(PxlFrame _PF)
			{
				this.PF = _PF;
			}

			public PxlFrame PF;
		}
	}
}
