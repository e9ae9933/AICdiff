using System;

namespace XX
{
	public class EffectItemGbg : EffectItem
	{
		public static C32 cola
		{
			get
			{
				return EffectItemGbg.G.cola;
			}
		}

		public static C32 colb
		{
			get
			{
				return EffectItemGbg.G.colb;
			}
		}

		public static void initEffectItemGbg()
		{
			if (EffectItemGbg.AAstardot != null)
			{
				return;
			}
			EffectItemGbg.AAstardot = new int[8][];
			EffectItemGbg.AAstardot[0] = new int[2];
			EffectItemGbg.AAstardot[1] = new int[] { 0, 0, -1, 0, 0, -1 };
			EffectItemGbg.AAstardot[2] = new int[] { -1, -1 };
			EffectItemGbg.AAstardot[3] = new int[] { -1, 0, 0, -1, -2, 0, 0, -2 };
			EffectItemGbg.AAstardot[4] = new int[] { 0, 0, -1, 0, 0, -1 };
			EffectItemGbg.AAstardot[5] = new int[] { -1, -1, 0, -2, -2, 0 };
			EffectItemGbg.AAstardot[6] = new int[] { 0, 0, -1, -1 };
			EffectItemGbg.AAstardot[7] = new int[] { -1, 0, 0, -1 };
			EffectItemGbg.AAhanabidot = new int[7][];
			int[][] aahanabidot = EffectItemGbg.AAhanabidot;
			int num = 0;
			int[] array = new int[2];
			array[0] = 1;
			aahanabidot[num] = array;
			EffectItemGbg.AAhanabidot[1] = new int[] { 0, 0, 1, 1, 2, 1 };
			EffectItemGbg.AAhanabidot[2] = new int[] { 1, 0, 3, 0, 3, 1 };
			EffectItemGbg.AAhanabidot[3] = new int[] { 1, 1, 2, 0, 3, 1, 4, 2 };
			EffectItemGbg.AAhanabidot[4] = new int[] { 2, 2, 3, 3, 4, 0, 5, 2 };
			EffectItemGbg.AAhanabidot[5] = new int[] { 4, 3, 5, 4, 6, 1 };
			EffectItemGbg.AAhanabidot[6] = new int[] { 6, 0, 7, 3, 4, 4 };
			EffectItemGbg.AApreparestardot = new int[10][];
			EffectItemGbg.AApreparestardot[0] = new int[2];
			EffectItemGbg.AApreparestardot[1] = new int[0];
			EffectItemGbg.AApreparestardot[2] = new int[] { -1, 0, -2, 0, -4, 0, 0, 1, 0, 2 };
			EffectItemGbg.AApreparestardot[3] = new int[] { 0, 0, -1, 1 };
			EffectItemGbg.AApreparestardot[4] = new int[] { -1, 0, -1, 1, 0, 1, -2, 2, -3, 3 };
			int[][] aapreparestardot = EffectItemGbg.AApreparestardot;
			int num2 = 5;
			int[] array2 = new int[2];
			array2[0] = -6;
			aapreparestardot[num2] = array2;
			EffectItemGbg.AApreparestardot[6] = new int[]
			{
				0, 0, -2, 0, -2, 1, -1, 1, -1, 2,
				0, 2, -5, 0, -5, 1, -3, 3, -2, 4
			};
			EffectItemGbg.AApreparestardot[7] = new int[]
			{
				-1, 0, 0, 1, -3, 0, 0, 3, -3, 2,
				-2, 3
			};
			EffectItemGbg.AApreparestardot[8] = new int[] { -3, 1, -2, 2, -1, 3 };
			EffectItemGbg.AApreparestardot[9] = new int[]
			{
				-3, 0, -6, 0, -6, 1, -5, 2, -3, 3,
				-2, 5, -1, 5, 0, 3, 0, 5
			};
			EffectItemGbg.AAmoondot = new int[3][];
			EffectItemGbg.AAmoondot[0] = new int[] { 1, 0, 0, 1 };
			EffectItemGbg.AAmoondot[1] = new int[] { 1, 1, 2, 0 };
			EffectItemGbg.AAmoondot[2] = new int[] { 0, 2, 1, 1, 2, 0 };
		}

		public EffectItemGbg(EffectGbg _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
			: base(_EF, _title, _x, _y, _z, _time, _saf)
		{
			this.EGbg = _EF;
		}

		public override void destruct()
		{
			base.destruct();
		}

		public override EffectItem initEffect(string typename = "")
		{
			return base.initEffect("XX.EffectItemGbg,unsafeAssem");
		}

		public readonly EffectGbg EGbg;

		public static BM G;

		private static int[][] AAstardot;

		private static int[][] AAhanabidot;

		private static int[][] AApreparestardot;

		private static int[][] AAmoondot;
	}
}
