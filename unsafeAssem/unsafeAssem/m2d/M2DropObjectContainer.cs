using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DropObjectContainer : RBase<M2DropObject>
	{
		public M2DropObjectContainer(Map2d _Mp)
			: base(32, true, false, false)
		{
			this.Mp = _Mp;
			this.FD_drawDropObject = new M2DrawBinder.FnEffectBind(this.drawDropObject);
		}

		public override void clear()
		{
			base.clear();
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
		}

		public override M2DropObject Create()
		{
			return new M2DropObject(this);
		}

		public M2DropObject Add(M2DropObject.FnDropObjectDraw FnDraw, float _x, float _y, float _vx, float _vy, float _z = -1f, float _time = -1f)
		{
			if (this.Ed == null)
			{
				this.Ed = this.Mp.setED("Dro", this.FD_drawDropObject, 0f);
			}
			M2DropObject m2DropObject = base.Pop(64);
			m2DropObject.Set(FnDraw, _x, _y, _vx, _vy, _z, _time);
			int num = this.index_count;
			this.index_count = num + 1;
			m2DropObject.index = num;
			return m2DropObject;
		}

		public M2DropObject AddManual(float _x, float _y, float _vx, float _vy, float _z = -1f, float _time = -1f)
		{
			if (this.Ed == null)
			{
				this.Ed = this.Mp.setED("Dro", this.FD_drawDropObject, 0f);
			}
			M2DropObject m2DropObject = base.Pop(64);
			m2DropObject.Set(null, _x, _y, _vx, _vy, _z, _time).TypeAdd(DROP_TYPE.REMOVE_MANUAL);
			m2DropObject.type &= (DROP_TYPE)(-2);
			int num = this.index_count;
			this.index_count = num + 1;
			m2DropObject.index = num;
			return m2DropObject;
		}

		private bool drawDropObject(EffectItem Ef, M2DrawBinder Ed)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].draw(Ef, Ed);
			}
			if (this.LEN == 0)
			{
				this.Ed = null;
				return false;
			}
			return true;
		}

		public override bool run(float fcnt)
		{
			base.run(fcnt);
			if (this.Ed != null && this.LEN == 0)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			return this.LEN > 0;
		}

		public void colliderFined()
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItems[i].colliderFined();
			}
		}

		public M2DropObject Get(int i)
		{
			if (i < this.LEN)
			{
				return this.AItems[i];
			}
			return null;
		}

		public M2DropObjectContainer setBlood(float cx, float cy, float range_w, float range_h, int count, uint color, float vx_level = 0f, bool is_blood = false)
		{
			float num = (color >> 24) / 255f;
			color &= 16777215U;
			int num2 = -1;
			if (is_blood && M2DBase.blood_weaken > 0)
			{
				num2 = X.Mx(0, (int)((float)count * (1f - (float)M2DBase.blood_weaken / 100f)));
			}
			M2DropObject.FnDropObjectDraw fn = M2DropObjectReader.GetFn("splash_blood");
			for (int i = 0; i < count; i++)
			{
				if (i == num2)
				{
					color = 6908265U;
				}
				float num3 = X.NIXP(-0.12f, 0.12f);
				num3 += 0.12f * X.absMn(vx_level, 1f);
				num3 *= X.Mx(X.Abs(vx_level), 1f);
				M2DropObject m2DropObject = this.Add(fn, cx + range_w * (-0.5f + X.XORSP()), cy + range_h * (-0.5f + X.XORSP()), num3, X.NIXP(-0.15f, -0.02f), num, (float)color);
				m2DropObject.bounce_x_reduce = 0f;
				m2DropObject.bounce_y_reduce = 0f;
				m2DropObject.type = DROP_TYPE.GROUND_STOP_X;
			}
			return this;
		}

		public M2DropObjectContainer setBlood(M2Mover Mv, int count, uint color, float vx_level = 0f, bool is_blood = false)
		{
			return this.setBlood(Mv.x, Mv.y, Mv.sizex * 2f * 0.88f, Mv.sizey * 2f * 0.88f, count, color, vx_level, is_blood);
		}

		public M2DropObjectContainer setLoveJuice(M2Mover Mv, int count, uint color = 4294967295U, float vx_level = 1f, bool pee = false)
		{
			float num = (color >> 24) / 255f;
			color &= 16777215U;
			Vector3 vector = Vector3.zero;
			if (Mv is M2MoverPr)
			{
				vector = (Mv as M2MoverPr).getHipPos();
			}
			else
			{
				vector.Set(Mv.x, Mv.y, (float)Mv.aim);
			}
			int num2 = CAim._XD((int)vector.z, 1);
			int num3 = -CAim._YD((int)vector.z, 1);
			float num4 = ((pee && num2 != 0) ? 0.5f : (-1f));
			float num5 = (pee ? 0.125f : 1f);
			M2DropObject.FnDropObjectDraw fn = M2DropObjectReader.GetFn("splash_love_juice");
			for (int i = X.Mx(1, (int)((float)count * X.EF_LEVEL_NORMAL)); i >= 0; i--)
			{
				float num6 = ((num2 != 0) ? ((pee ? X.NIXP(1.25f, 1.6f) : X.NIXP(-0.2f, 1f)) * 0.06f * (float)num2) : (X.NIXP(-0.3f, 0.3f) * num5));
				M2DropObject m2DropObject = this.Add(fn, vector.x + ((num2 != 0) ? 0.2f : 0.04f) * (-0.5f + X.XORSP()) * num5 * vx_level, vector.y + 0.04f * (-0.5f + X.XORSP()) * num5, num6, num4 * (0.03f + X.NIXP(0f, 0.11f) * ((num2 != 0) ? num5 : 1f) + (float)num3 * 0.03f), num, (float)color);
				m2DropObject.bounce_x_reduce = 0f;
				m2DropObject.bounce_y_reduce = 0f;
				m2DropObject.gravity_scale = 0.3f;
				m2DropObject.type = DROP_TYPE.GROUND_STOP_X;
			}
			return this;
		}

		public M2DropObjectContainer setGroundBreaker(float cx, float cy, float x_mul, float y_mul, M2DropObjectReader Dr)
		{
			if (Dr == null)
			{
				return this;
			}
			return this.setGroundBreaker(this.Mp.getHardChip((int)cx, (int)cy, null, false, true, null), cx, (float)X.IntR(cy), x_mul, y_mul, Dr);
		}

		public M2DropObjectContainer setGroundBreaker(float cx, float cy, float set_effect_x, float set_effect_y, float x_mul, float y_mul, M2DropObjectReader Dr)
		{
			if (Dr == null)
			{
				return this;
			}
			return this.setGroundBreaker(this.Mp.getHardChip((int)cx, (int)cy, null, false, true, null), set_effect_x, set_effect_y, x_mul, y_mul, Dr);
		}

		public M2DropObjectContainer setGroundBreaker(M2Chip Cp, float ef_x, float ef_y, float x_mul, float y_mul, M2DropObjectReader Dr)
		{
			if (Dr == null || Cp == null)
			{
				return this;
			}
			Dr.createObjects(this.Mp, Cp.Img.fnDrawForDropObject, ef_x, ef_y, x_mul, y_mul, 0f, null);
			return this;
		}

		public readonly Map2d Mp;

		private int index_count;

		private M2DrawBinder Ed;

		private readonly M2DrawBinder.FnEffectBind FD_drawDropObject;
	}
}
