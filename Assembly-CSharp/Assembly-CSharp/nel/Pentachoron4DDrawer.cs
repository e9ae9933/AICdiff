using System;
using UnityEngine;
using XX;

namespace nel
{
	public class Pentachoron4DDrawer : Dim4DDrawer
	{
		public Pentachoron4DDrawer()
			: base(5)
		{
		}

		public override void drawLineTo(MeshDrawer Md, float scale_xy = 1f)
		{
			for (int i = 0; i < this.MAX_PT; i++)
			{
				Vector4 vector = base.ScaleP(i, scale_xy);
				for (int j = i + 1; j < this.MAX_PT; j++)
				{
					this.FD_DrawLine(Md, vector, base.ScaleP(j, scale_xy));
				}
			}
		}

		protected override Vector4 GetPt_Inner(int i)
		{
			Vector4 vector;
			switch (i)
			{
			case 0:
				vector = new Vector4(1f, -0.4472136f, 1f, 1f);
				break;
			case 1:
				vector = new Vector4(1f, -0.4472136f, -1f, -1f);
				break;
			case 2:
				vector = new Vector4(-1f, -0.4472136f, 1f, -1f);
				break;
			case 3:
				vector = new Vector4(-1f, -0.4472136f, -1f, 1f);
				break;
			default:
				vector = new Vector4(0f, 1.7888544f, 0f, 0f);
				break;
			}
			return vector;
		}

		public override void drawSurfaceTo(MeshDrawer Md, float scale_xy = 1f)
		{
			int num = 0;
			for (int i = 0; i < this.MAX_PT - 2; i++)
			{
				Vector4 vector = base.ScaleP(i, scale_xy);
				for (int j = i + 1; j < this.MAX_PT - 1; j++)
				{
					Vector4 vector2 = base.ScaleP(j, scale_xy);
					for (int k = j + 1; k < this.MAX_PT; k++)
					{
						this.FD_DrawSurface(Md, num++, vector, vector2, base.ScaleP(k, scale_xy));
					}
				}
			}
		}

		public const float one_div_root5 = 0.4472136f;
	}
}
