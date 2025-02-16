using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2MvColliderCreatorAtk : M2MvColliderCreator
	{
		public M2MvColliderCreatorAtk(M2Mover _Mv)
			: base(_Mv)
		{
		}

		protected override void recreateExecute()
		{
			this.Cld.pathCount = this.path_count;
			Vector2 vector = base.transform.localScale;
			float num = base.CLEN * 0.015625f;
			float num2 = base.sizex * num / vector.x;
			float num3 = base.sizey * num / vector.y;
			float num4 = X.Mn(num2, this.collider_foot_slice_px_x / base.Mp.mover_scale * 0.015625f / vector.x);
			float num5 = this.collider_foot_slice_px_y / base.Mp.mover_scale * 0.015625f / vector.y;
			num5 = X.Mn(num3 * 2f, num5);
			int num6 = ((num5 > 0f) ? 4 : 2);
			Vector2 vector2 = ((this.NestTarget != null) ? this.NestTarget.getUShift(base.Mp) : Vector2.zero);
			Vector2[] array;
			if (this.collider_head_slice <= 0)
			{
				array = new Vector2[num6 + 2];
				if (num6 == 2)
				{
					array[0].Set(vector2.x - num2, vector2.y - num3);
					array[1].Set(vector2.x + num2, vector2.y - num3);
				}
				else if (num6 == 4)
				{
					array[0].Set(vector2.x - num2, vector2.y - num3 + num5);
					array[1].Set(vector2.x - num2 + num4, vector2.y - num3);
					array[2].Set(vector2.x + num2 - num4, vector2.y - num3);
					array[3].Set(vector2.x + num2, vector2.y - num3 + num5);
				}
				array[num6].Set(vector2.x + num2, vector2.y + num3);
				array[num6 + 1].Set(vector2.x - num2, vector2.y + num3);
			}
			else
			{
				float num7 = 3.1415927f / (float)this.collider_head_slice;
				float num8 = 0f;
				float num9 = X.Mn(num2, num3 * 2f - num5);
				float num10 = num3 - num9;
				num6 = ((num10 <= -num3) ? 0 : num6);
				array = new Vector2[num6 + this.collider_head_slice + 1];
				if (num6 == 2)
				{
					array[0].Set(vector2.x - num2, vector2.y - num3);
					array[1].Set(vector2.x + num2, vector2.y - num3);
				}
				else if (num6 == 4)
				{
					array[0].Set(vector2.x - num2, vector2.y - num3 + num5);
					array[1].Set(vector2.x - num2 + num4, vector2.y - num3);
					array[2].Set(vector2.x + num2 - num4, vector2.y - num3);
					array[3].Set(vector2.x + num2, vector2.y - num3 + num5);
				}
				for (int i = 0; i <= this.collider_head_slice; i++)
				{
					array[i + num6].Set(vector2.x + X.Cos(num8) * num2, vector2.y + X.Sin(num8) * num9 + num10);
					num8 += num7;
				}
			}
			this.Cld.SetPath(0, array);
			this.Cld.enabled = true;
		}

		public virtual void setToRect()
		{
			this.collider_head_slice = 0;
			this.collider_foot_slice_px_x = 0f;
			this.collider_foot_slice_px_y = 0f;
		}

		public int collider_head_slice;

		public float collider_foot_slice_px_x;

		public float collider_foot_slice_px_y;

		public CCNestItem NestTarget;

		public int path_count = 1;
	}
}
