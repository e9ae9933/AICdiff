using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public static class CoinStorage
	{
		public static event CoinStorage.FnMoneyChanged FD_MoneyChanged;

		public static void init()
		{
			CoinStorage.OData = new BDic<string, CoinStorage.CoinDataSheet>();
			CoinStorage.OCoinInCurMap = new BDic<PxlSequence, List<M2CImgDrawerCoin>>(32);
			for (int i = 2; i >= 0; i--)
			{
				CoinStorage.Acount[i] = new CoinStorage.CoinEntry();
			}
		}

		public static void initS()
		{
			CoinStorage.pitch_count = 0;
			CoinStorage.pre_get_floort = 0f;
			CoinStorage.OCoinInCurMap.Clear();
			if (CoinStorage.Ed != null)
			{
				CoinStorage.Ed.destruct();
				CoinStorage.Ed = null;
			}
		}

		public static void flush()
		{
			if (CoinStorage.OData == null)
			{
				CoinStorage.init();
			}
			CoinStorage.OData.Clear();
		}

		public static void Clear()
		{
			if (CoinStorage.OData == null)
			{
				CoinStorage.init();
			}
			CoinStorage.OData.Clear();
			for (int i = 2; i >= 0; i--)
			{
				CoinStorage.Acount[i].newGame();
			}
		}

		public static void Clear(CoinStorage.CTYPE type)
		{
			CoinStorage.Acount[(int)type].Reduce((int)CoinStorage.Acount[(int)type].Get());
		}

		private static uint coin_key(M2CImgDrawerCoin Pt)
		{
			return (uint)(Pt.Cp.mapx * 65536 + Pt.Cp.mapy * 256 + (int)Pt.coin_id);
		}

		public static bool isAlreadyTaken(M2CImgDrawerCoin Pt)
		{
			CoinStorage.CoinDataSheet sheet = CoinStorage.GetSheet(Pt.Mp, true);
			return sheet != null && sheet.Aget.IndexOf(CoinStorage.coin_key(Pt)) >= 0;
		}

		public static CoinStorage.CoinDataSheet GetSheet(Map2d Mp, bool no_make = true)
		{
			return CoinStorage.GetSheet(Mp.key, no_make);
		}

		private static CoinStorage.CoinDataSheet GetSheet(string mapkey, bool no_make = true)
		{
			CoinStorage.CoinDataSheet coinDataSheet = X.Get<string, CoinStorage.CoinDataSheet>(CoinStorage.OData, mapkey);
			if (coinDataSheet != null)
			{
				return coinDataSheet;
			}
			if (no_make)
			{
				return null;
			}
			return CoinStorage.OData[mapkey] = new CoinStorage.CoinDataSheet();
		}

		public static void takeCoin(M2CImgDrawerCoin Pt, PxlSequence Sq, bool play_sound = false)
		{
			CoinStorage.GetSheet(Pt.Mp, false).Aget.Add(CoinStorage.coin_key(Pt));
			CoinStorage.addCount(Pt.Meta.GetI("coin", 0, 0), CoinStorage.CTYPE.GOLD, true);
			CoinStorage.DeassignCoin(Pt, Sq);
			if (play_sound)
			{
				if (CoinStorage.pitch_count > 0 && Pt.Mp.floort - CoinStorage.pre_get_floort > 80f)
				{
					CoinStorage.pitch_count = 0;
				}
				if (!X.DEBUGSPEFFECT)
				{
					M2DBase.playSnd("getcoin_1_" + CoinStorage.pitch_count.ToString());
				}
				M2DBase.playSnd("getcoin_2_" + Pt.coin_id.ToString());
				if (CoinStorage.pitch_count < 14)
				{
					CoinStorage.pitch_count += 1;
				}
				CoinStorage.pre_get_floort = Pt.Mp.floort;
			}
		}

		public static void addCount(int v, bool show_log)
		{
			CoinStorage.addCount(v, CoinStorage.CTYPE.GOLD, show_log);
		}

		public static void addCount(int v, CoinStorage.CTYPE ctype = CoinStorage.CTYPE.GOLD, bool show_log = true)
		{
			if (v <= 0)
			{
				return;
			}
			v = CoinStorage.Acount[(int)ctype].Add(v);
			if (show_log && CoinStorage.FD_MoneyChanged != null)
			{
				CoinStorage.FD_MoneyChanged(ctype, v);
			}
		}

		public static void reduceCount(int v, CoinStorage.CTYPE ctype = CoinStorage.CTYPE.GOLD)
		{
			if (v <= 0)
			{
				return;
			}
			v = CoinStorage.Acount[(int)ctype].Reduce(v);
			if (CoinStorage.FD_MoneyChanged != null)
			{
				CoinStorage.FD_MoneyChanged(ctype, -v);
			}
		}

		public static string icon_key(CoinStorage.CTYPE c)
		{
			string text;
			if (c != CoinStorage.CTYPE.CRAFTS)
			{
				if (c != CoinStorage.CTYPE.JUICE)
				{
					text = "money_icon";
				}
				else
				{
					text = "money_juice";
				}
			}
			else
			{
				text = "money_crafts";
			}
			return text;
		}

		public static string getIconHtml(CoinStorage.CTYPE ctype, int w = 30)
		{
			return string.Concat(new string[]
			{
				"<img mesh=\"",
				CoinStorage.icon_key(ctype),
				"\" width=\"",
				w.ToString(),
				"\" />"
			});
		}

		public static STB getIconHtml(STB Stb, CoinStorage.CTYPE ctype, int w = 30)
		{
			Stb.Add("<img mesh=\"", CoinStorage.icon_key(ctype), "\" width=\"").Add("", w, "\" />");
			return Stb;
		}

		public static ByteArray writeBinaryTo(ByteArray Ba)
		{
			using (BList<string> blist = X.objKeysB<string, CoinStorage.CoinDataSheet>(CoinStorage.OData))
			{
				int count = blist.Count;
				Ba.writeInt(count);
				for (int i = 0; i < count; i++)
				{
					Ba.writeString(blist[i], "utf-8");
					CoinStorage.OData[blist[i]].writeBinaryTo(Ba);
				}
				for (int j = 0; j < 3; j++)
				{
					CoinStorage.Acount[j].writeBinaryTo(Ba);
				}
			}
			return Ba;
		}

		public static ByteReader readBinaryFrom(ByteReader Ba, int version)
		{
			CoinStorage.Clear();
			bool flag = version >= 20;
			bool flag2 = version >= 29;
			int num = Ba.readInt();
			for (int i = 0; i < num; i++)
			{
				CoinStorage.GetSheet(Ba.readString("utf-8", false), false).readBinaryFrom(Ba);
			}
			bool flag3 = version >= 35;
			CoinStorage.Acount[0].readBinaryFrom(Ba, flag3);
			if (flag)
			{
				CoinStorage.Acount[1].readBinaryFrom(Ba, flag3);
			}
			if (flag2)
			{
				CoinStorage.Acount[2].readBinaryFrom(Ba, flag3);
			}
			return Ba;
		}

		public static uint getCount(CoinStorage.CTYPE c = CoinStorage.CTYPE.GOLD)
		{
			if (c >= CoinStorage.CTYPE.GOLD && c < CoinStorage.CTYPE._MAX)
			{
				return CoinStorage.Acount[(int)c].Get();
			}
			return 0U;
		}

		public static Map2d Mp
		{
			get
			{
				return M2DBase.Instance.curMap;
			}
		}

		public static void AssignCoin(M2CImgDrawerCoin Dr, PxlSequence Sq)
		{
			if (Sq == null)
			{
				return;
			}
			List<M2CImgDrawerCoin> list = X.Get<PxlSequence, List<M2CImgDrawerCoin>>(CoinStorage.OCoinInCurMap, Sq);
			if (list == null)
			{
				list = (CoinStorage.OCoinInCurMap[Sq] = new List<M2CImgDrawerCoin>(32));
			}
			list.Add(Dr);
			if (CoinStorage.Ed == null)
			{
				CoinStorage.Ed = CoinStorage.Mp.setED("CoinDraw", (EffectItem Ef, M2DrawBinder Ed) => CoinStorage.fnDraw(Ef, Ed), 0f);
			}
		}

		public static void DeassignCoin(M2CImgDrawerCoin Dr, PxlSequence Sq)
		{
			if (CoinStorage.Ed == null || Sq == null)
			{
				return;
			}
			List<M2CImgDrawerCoin> list = X.Get<PxlSequence, List<M2CImgDrawerCoin>>(CoinStorage.OCoinInCurMap, Sq);
			if (list == null)
			{
				return;
			}
			list.Remove(Dr);
		}

		private static bool fnDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			MeshDrawer meshDrawer = null;
			bool flag = false;
			foreach (KeyValuePair<PxlSequence, List<M2CImgDrawerCoin>> keyValuePair in CoinStorage.OCoinInCurMap)
			{
				List<M2CImgDrawerCoin> value = keyValuePair.Value;
				PxlImage pxlImage = null;
				for (int i = value.Count - 1; i >= 0; i--)
				{
					flag = value[i].drawCoin(Ef, Ed, ref meshDrawer, ref pxlImage) || flag;
				}
			}
			if (!flag && Ed == CoinStorage.Ed)
			{
				CoinStorage.Ed = null;
			}
			return flag;
		}

		private static readonly CoinStorage.CoinEntry[] Acount = new CoinStorage.CoinEntry[3];

		public const uint MAX_COUNT = 999999U;

		private const byte snd_pitch_max = 14;

		private static byte pitch_count = 0;

		private static float pre_get_floort;

		private static BDic<string, CoinStorage.CoinDataSheet> OData;

		private static BDic<PxlSequence, List<M2CImgDrawerCoin>> OCoinInCurMap;

		private static M2DrawBinder Ed;

		public delegate void FnMoneyChanged(CoinStorage.CTYPE ctype, int added);

		public sealed class CoinDataSheet
		{
			public CoinDataSheet()
			{
				this.Aget = new List<uint>(1);
			}

			public void Clear()
			{
				this.Aget.Clear();
			}

			public ByteArray writeBinaryTo(ByteArray Ba)
			{
				int count = this.Aget.Count;
				Ba.writeInt(count);
				for (int i = 0; i < count; i++)
				{
					Ba.writeUInt(this.Aget[i]);
				}
				return Ba;
			}

			public ByteReader readBinaryFrom(ByteReader Ba)
			{
				this.Clear();
				int num = Ba.readInt();
				for (int i = 0; i < num; i++)
				{
					this.Aget.Add(Ba.readUInt());
				}
				return Ba;
			}

			public List<uint> Aget;
		}

		public class CoinEntry
		{
			public uint Get()
			{
				return this.current;
			}

			public void newGame()
			{
				this.current = (this.total_obtain = (this.total_consume = 0U));
			}

			public int Add(int v)
			{
				if (v > 0)
				{
					int num = X.Mx(0, X.Mn((int)(999999U - this.current), v));
					this.current += (uint)num;
					if (this.total_obtain < 999999U)
					{
						this.total_obtain += (uint)X.Mn((int)(999999U - this.total_obtain), v);
					}
					v = num;
				}
				return v;
			}

			public int Reduce(int v)
			{
				if (v > 0)
				{
					v = X.Mn(v, (int)this.current);
					this.current -= (uint)v;
					if (this.total_consume < 999999U)
					{
						this.total_consume += (uint)X.Mn((int)(999999U - this.total_consume), v);
					}
				}
				return v;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeUInt(this.current);
				Ba.writeUInt(this.total_obtain);
				Ba.writeUInt(this.total_consume);
			}

			public void readBinaryFrom(ByteReader Ba, bool read_obtaintotal)
			{
				this.current = Ba.readUInt();
				if (read_obtaintotal)
				{
					this.total_obtain = Ba.readUInt();
					this.total_consume = Ba.readUInt();
					return;
				}
				this.total_obtain = this.current;
			}

			private uint current;

			public uint total_obtain;

			public uint total_consume;
		}

		public enum CTYPE
		{
			GOLD,
			CRAFTS,
			JUICE,
			_MAX
		}
	}
}
