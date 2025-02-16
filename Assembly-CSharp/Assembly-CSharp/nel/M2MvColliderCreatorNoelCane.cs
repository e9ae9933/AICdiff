using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2MvColliderCreatorNoelCane : M2MvColliderCreator
	{
		public M2MvColliderCreatorNoelCane(M2Mover _Mv, float _mover_scale)
			: base(_Mv)
		{
			this.mover_scale = _mover_scale;
		}

		public bool is_sphere
		{
			get
			{
				return this.is_sphere_;
			}
			set
			{
				if (this.is_sphere == value)
				{
					return;
				}
				this.is_sphere_ = value;
				this.need_recreate = true;
			}
		}

		public PxlImage BaseImg
		{
			get
			{
				return this.BaseImg_;
			}
			set
			{
				if (this.BaseImg == value)
				{
					return;
				}
				this.BaseImg_ = value;
				this.need_recreate = true;
			}
		}

		protected override void recreateExecute()
		{
			float num = this.mover_scale * 0.015625f;
			Vector2[] array;
			if (this.is_sphere_)
			{
				float num2 = (float)this.BaseImg_.height * num / 2f;
				float num3 = (float)this.BaseImg_.width * num / 2f;
				float num4 = -num3 + num2;
				int num5 = 10;
				array = new Vector2[num5 + 2];
				for (int i = 0; i < num5; i++)
				{
					float num6 = ((float)i + 0.5f) / (float)num5;
					array[i].Set(num4 + num2 * X.Cos0(num6), num2 * X.Sin0(num6));
				}
				array[num5].Set(num3, array[num5 - 1].y);
				array[num5 + 1].Set(num3, array[0].y);
			}
			else
			{
				array = new Vector2[4];
				float num7 = (float)this.BaseImg_.width * num / 2f;
				float num8 = 4f * num / 2f;
				array[0].Set(-num7, -num8);
				array[1].Set(-num7, num8);
				array[2].Set(num7, num8);
				array[3].Set(num7, -num8);
			}
			this.Cld.pathCount = 1;
			this.Cld.SetPath(0, array);
		}

		private bool is_sphere_;

		private float mover_scale;

		private PxlImage BaseImg_;
	}
}
