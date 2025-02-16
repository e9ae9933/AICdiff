using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerPendulumBell : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerPendulumBell(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, true)
		{
			this.pdl_height = Meta.GetI("pendulum_bell", _Cp.iwidth, 1);
			this.fulc_x = this.Cp.mapcx;
			this.fulc_y = (float)this.Cp.drawy * base.Mp.rCLEN;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.layer != 3)
			{
				this.DrLinked = null;
				if (this.Pend == null)
				{
					GameObject gameObject = base.Mp.createMoverGob<M2PendulumObject>("-pendulum_" + this.Cp.index.ToString(), this.Cp.mapcx, this.Cp.mapcy, true);
					this.Pend = IN.GetOrAdd<M2PendulumObject>(gameObject);
					this.Pend.appear(this, base.Mp);
					return;
				}
			}
			else
			{
				this.DrLinked = this.Cp.getAnyDrawer(4294967287U) as M2CImgDrawerPendulumBell;
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (when_map_close && this.Pend != null)
			{
				this.Pend.destruct();
				this.rotR = 0f;
				IN.DestroyOne(this.Pend.gameObject);
				this.Pend = null;
				this.DrLinked = null;
			}
		}

		public void activate()
		{
			this.need_reposit_flag = true;
			if (this.DrLinked != null)
			{
				this.rotR = this.DrLinked.rotR;
			}
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override void reposition(int shift_col = 3)
		{
			this.need_reposit_flag = false;
			if ((shift_col & 1) != 0)
			{
				Vector3[] vertexArray = this.Md.getVertexArray();
				int num = this.index_last - this.index_first;
				float num2 = base.Mp.map2ux(this.fulc_x);
				float num3 = base.Mp.map2uy(this.fulc_y);
				Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(num2, num3, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotR * 180f / 3.1415927f)) * Matrix4x4.Translate(new Vector3(-num2, -num3, 0f)) * Matrix4x4.Scale(new Vector3(0.015625f, 0.015625f, 1f));
				for (int i = 0; i < num; i++)
				{
					vertexArray[i + this.index_first] = matrix4x.MultiplyPoint3x4(this.APos[i]);
				}
			}
		}

		public int pdl_height;

		public float rotR;

		private M2PendulumObject Pend;

		public float fulc_x;

		public float fulc_y;

		private M2CImgDrawerPendulumBell DrLinked;
	}
}
