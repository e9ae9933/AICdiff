using System;
using UnityEngine;

namespace XX
{
	public class TrajectoryDrawer
	{
		public TrajectoryDrawer(int capacity)
		{
			this.Alloc(capacity);
			if (TrajectoryDrawer.C == null)
			{
				TrajectoryDrawer.C = new C32();
				TrajectoryDrawer.COut = new C32();
			}
		}

		public TrajectoryDrawer Clear()
		{
			this.start_index = 0;
			this.current_use = 0;
			return this;
		}

		public TrajectoryDrawer Alloc(int capacity)
		{
			if (this.APos == null)
			{
				this.APos = new Vector2[capacity];
			}
			else if (this.APos.Length != capacity)
			{
				Array.Resize<Vector2>(ref this.APos, capacity);
			}
			this.start_index %= capacity;
			return this;
		}

		public TrajectoryDrawer Add(float x, float y)
		{
			return this.Add(new Vector2(x, y));
		}

		public TrajectoryDrawer Add(Vector2 P)
		{
			if (this.current_use < this.APos.Length)
			{
				Vector2[] apos = this.APos;
				int num = this.current_use;
				this.current_use = num + 1;
				apos[num] = P;
			}
			else
			{
				Vector2[] apos2 = this.APos;
				int num = this.start_index;
				this.start_index = num + 1;
				apos2[num] = P;
				if (this.start_index >= this.APos.Length)
				{
					this.start_index = 0;
				}
			}
			return this;
		}

		public TrajectoryDrawer BlurLineWTo(MeshDrawer Md, float x, float y, float thick, float blur_thick, float tale_len, Color32 Cf, Color32 Cb, float blur_center_alpha = 0.5f)
		{
			Md.Col = Cf;
			int num = this.start_index % this.APos.Length;
			Vector2 vector = this.DrawPos(num);
			int num2 = this.current_use;
			Md.Col = Cb;
			float num3 = 1f / (float)this.APos.Length;
			float num4 = num3;
			Md.ColGrd.Set(Cb).blend(Cf, num4);
			TrajectoryDrawer.C.Set(Md.ColGrd).mulA(blur_center_alpha);
			TrajectoryDrawer.COut.Set(Md.ColGrd).mulA(0f);
			for (int i = 1; i < num2; i++)
			{
				if (++num >= this.APos.Length)
				{
					num = 0;
				}
				Vector2 vector2 = this.DrawPos(num);
				if (vector2.x != vector.x || vector2.y != vector.y)
				{
					Md.BlurLineW(x + vector.x, y + vector.y, x + vector2.x, y + vector2.y, thick, blur_thick, (i == 1) ? 1 : 0, tale_len, 0f, 1f, TrajectoryDrawer.C, TrajectoryDrawer.COut, TrajectoryDrawer.COut);
				}
				num4 += num3;
				Md.Col = Md.ColGrd.C;
				Md.ColGrd.Set(Cb).blend(Cf, num4);
				TrajectoryDrawer.C.Set(Md.ColGrd).mulA(blur_center_alpha);
				TrajectoryDrawer.COut.Set(Md.ColGrd).mulA(0f);
				vector = vector2;
			}
			return this;
		}

		private Vector2 DrawPos(int i)
		{
			Vector2 vector = this.APos[i];
			if (this.FD_PosConvert != null)
			{
				vector = this.FD_PosConvert(vector);
			}
			return vector;
		}

		private int start_index;

		private int current_use;

		private Vector2[] APos;

		public TrajectoryDrawer.FnPos FD_PosConvert;

		private static C32 C;

		private static C32 COut;

		public delegate Vector2 FnPos(Vector2 Pos);
	}
}
