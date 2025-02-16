using System;
using UnityEngine;
using XX;

namespace nel
{
	public class TesseractDrawer : Dim4DDrawer
	{
		public TesseractDrawer()
			: base(16)
		{
		}

		public override void drawLineTo(MeshDrawer Md, float scale_xy = 1f)
		{
			for (int i = 0; i < this.MAX_PT; i++)
			{
				Vector4 vector = base.ScaleP(i, scale_xy);
				if ((i & 1) == 0)
				{
					this.FD_DrawLine(Md, vector, base.ScaleP(i | 1, scale_xy));
				}
				if ((i & 2) == 0)
				{
					this.FD_DrawLine(Md, vector, base.ScaleP(i | 2, scale_xy));
				}
				if ((i & 4) == 0)
				{
					this.FD_DrawLine(Md, vector, base.ScaleP(i | 4, scale_xy));
				}
				if ((i & 8) == 0)
				{
					this.FD_DrawLine(Md, vector, base.ScaleP(i | 8, scale_xy));
				}
			}
		}

		public override void drawSurfaceTo(MeshDrawer Md, float scale_xy = 1f)
		{
			int num = 0;
			for (int i = 0; i < this.MAX_PT; i++)
			{
				Vector4 vector = base.ScaleP(i, scale_xy);
				for (int j = 0; j < 4; j++)
				{
					ValueTuple<int, int> valueTuple;
					switch (j)
					{
					case 0:
						valueTuple = new ValueTuple<int, int>(1, 2);
						break;
					case 1:
						valueTuple = new ValueTuple<int, int>(2, 4);
						break;
					case 2:
						valueTuple = new ValueTuple<int, int>(4, 8);
						break;
					default:
						valueTuple = new ValueTuple<int, int>(1, 8);
						break;
					}
					ValueTuple<int, int> valueTuple2 = valueTuple;
					int item = valueTuple2.Item1;
					int item2 = valueTuple2.Item2;
					if ((i & (item | item2)) == 0)
					{
						Vector4 vector2 = base.ScaleP(i | item | item2, scale_xy);
						this.FD_DrawSurface(Md, num++, vector, base.ScaleP(i | item, scale_xy), vector2);
						this.FD_DrawSurface(Md, num++, vector, vector2, base.ScaleP(i | item2, scale_xy));
					}
				}
			}
		}

		protected override Vector4 GetPt_Inner(int i)
		{
			return new Vector4((float)X.MPF((i & 1) != 0), (float)X.MPF((i & 2) != 0), (float)X.MPF((i & 4) != 0), (float)X.MPF((i & 8) != 0));
		}
	}
}
