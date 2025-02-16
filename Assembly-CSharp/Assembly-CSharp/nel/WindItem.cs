using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class WindItem : IRunAndDestroy
	{
		public WindManager Con
		{
			get
			{
				return (M2DBase.Instance as NelM2DBase).WIND;
			}
		}

		public NelM2DBase M2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
		}

		public Map2d Mp
		{
			get
			{
				return (M2DBase.Instance as NelM2DBase).curMap;
			}
		}

		public WindItem Set(uint _id, float sx, float sy, float _radius, float _agR, float _max_lgt, float _apply_velocity, float _maxt = -1f, float _near_multiple = 1f)
		{
			this.x = sx;
			this.y = sy;
			this.radius = _radius;
			this.agR_ = _agR;
			this._cos = X.Cos(this.agR_);
			this._sin = X.Sin(this.agR_);
			this.t = 0f;
			this.fine_t = 0f;
			this.max_lgt = _max_lgt;
			this.maxt = _maxt;
			this.apply_velocity = _apply_velocity;
			this.near_multiple = _near_multiple;
			this.cur_lgt = -0.1f;
			this.id = _id;
			this.fineEffCount();
			return this;
		}

		public void fineEffCount()
		{
			this.eff_one_maxt = X.Abs(this.max_lgt) / this.apply_velocity;
			this.eff_cnt = (int)X.Mx(2f, this.radius * X.NIL(8f, 2.5f, this.radius, 45f)) * ((this.maxt < 0f) ? 1 : 2);
		}

		public bool need_fine_length
		{
			get
			{
				return this.cur_lgt < 0f;
			}
			set
			{
				if (value && this.cur_lgt >= 0f && this.check_wall)
				{
					this.cur_lgt = X.Mn(-this.cur_lgt, -0.1f);
				}
			}
		}

		public float agR
		{
			get
			{
				return this.agR_;
			}
			set
			{
				if (this.agR == value)
				{
					return;
				}
				this.agR_ = value;
				this._cos = X.Cos(this.agR_);
				this._sin = X.Sin(this.agR_);
				this.cur_lgt = -X.Abs(this.cur_lgt);
			}
		}

		public void setPos(float _x, float _y)
		{
			this.x = _x;
			this.y = _y;
			this.cur_lgt = -X.Abs(this.cur_lgt);
		}

		public void fineLgt()
		{
			if (this.cur_lgt < 0f)
			{
				if (this.max_lgt <= 0f)
				{
					this.cur_lgt = -this.max_lgt;
					return;
				}
				Vector3 vector = this.Mp.checkThroughBccNearestHitPos(this.x, this.y, this.x + this._cos * this.max_lgt, this.y - this._sin * this.max_lgt, this.radius, this.radius, -1, false, true, null, true);
				this.cur_lgt = ((vector.z < 0f) ? this.max_lgt : Mathf.Sqrt(vector.z));
			}
		}

		public bool run(float fcnt)
		{
			if (this.maxt == 0f)
			{
				return false;
			}
			if (this.t >= 0f)
			{
				if (this.maxt >= 0f && this.t >= this.maxt)
				{
					this.maxt = 0f;
					return false;
				}
				this.t += fcnt;
				if (this.maxt > 0f && this.t >= this.maxt - 30f)
				{
					return true;
				}
				this.fine_t += fcnt;
				this.fineLgt();
				if (this.fine_t >= 8f)
				{
					this.fine_t -= 8f;
					Map2d mp = this.Mp;
					float num = this._cos * this.apply_velocity;
					float num2 = -this._sin * this.apply_velocity;
					for (int i = mp.count_movers - 1; i >= 0; i--)
					{
						M2Mover mv = mp.getMv(i);
						if (!mv.move_script_attached)
						{
							IWindApplyable windApplyable = mv as IWindApplyable;
							if (windApplyable != null)
							{
								float num3 = this.isinP(mv.x, mv.y, X.Mn(mv.sizex, mv.sizey));
								if (num3 > 0f)
								{
									num3 *= windApplyable.getWindApplyLevel(this);
									if (num3 > 0f)
									{
										windApplyable.applyWindFoc(this, num3 * num, num3 * num2);
									}
								}
							}
						}
					}
					this.M2D.MGC.applyWind(this, num, num2);
				}
			}
			else if (this.t > -30f)
			{
				this.t -= fcnt;
			}
			return true;
		}

		public float isinP(float ax, float ay, float margin)
		{
			this.fineLgt();
			ax -= this.x;
			ay -= this.y;
			float num = margin + this.radius;
			Vector2 vector = X.ROTV2e(new Vector2(ax, -ay), this._cos, -this._sin);
			if (X.BTW(-num, vector.x, margin + this.cur_lgt) && X.Abs(vector.y) < num)
			{
				float num2 = 1f;
				if (this.near_multiple != 1f)
				{
					num2 *= X.NI(this.near_multiple, 1f, X.ZLINE(X.Abs(vector.x), X.Abs(this.max_lgt) * 0.3f));
				}
				return num2;
			}
			return 0f;
		}

		public void destruct()
		{
			this.maxt = 0f;
		}

		public void draw(EffectItem Ef, ref MeshDrawer Md)
		{
			if (this.maxt == 0f)
			{
				return;
			}
			this.fineLgt();
			float num;
			if (this.t >= 0f)
			{
				num = ((this.maxt <= -2f) ? 1f : (X.ZLINE(this.t + 6f, 30f) * ((this.maxt < 0f) ? 1f : X.ZLINE(this.maxt - this.t, 30f)) * 0.6f));
			}
			else
			{
				num = X.ZLINE(30f + this.t, 30f);
				if (num <= 0f)
				{
					return;
				}
			}
			if (Md == null)
			{
				Md = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
				Md.base_x = (Md.base_y = 0f);
			}
			Map2d mp = this.Mp;
			float num2 = 2.5f;
			float num3 = 1f / num2;
			float clenb = mp.CLENB;
			float num4 = X.Abs(this.max_lgt);
			Md.Scale(num2, 1f, false);
			Md.Rotate(this.agR, false);
			Md.Translate(mp.map2globalux(this.x), mp.map2globaluy(this.y), false);
			float num5 = this.eff_one_maxt / (float)this.eff_cnt;
			float num6 = num4 * clenb;
			float num7 = this.eff_one_maxt * 0.2f;
			for (int i = 0; i < this.eff_cnt; i++)
			{
				float num8 = this.t - (float)i * num5 + ((this.maxt == -2f) ? this.eff_one_maxt : 0f);
				if (num8 < 0f)
				{
					break;
				}
				int num9 = (int)num8 / (int)this.eff_one_maxt % 6 * 100 + i % 100;
				num8 %= this.eff_one_maxt;
				uint ran = X.GETRAN2((uint)((ulong)(this.id * 13U) + (ulong)((long)(i * 7))), (uint)(num9 + i * 3));
				Md.Col = Md.ColGrd.White().setA1(num * X.ZLINE(num8, num7) * X.ZLINE(this.eff_one_maxt - num8, num7)).mulA(((this.maxt < 0f) ? 0.33f : 1f) * X.NI(0.5f, 1f, X.RAN(ran, 1235)))
					.C;
				float num10 = (X.RAN(ran, 461) * 1.1f - 0.3f) * this.radius * clenb;
				float num11 = this.radius * clenb * (X.RANS(ran, 1569 + num9 * 3) * 0.33f + X.RANS(ran, 3125 + num9 * 2) * 0.33f + X.RANS(ran, 315 + num9 * 5) * 0.33f);
				bool flag = (i & 1) == 1;
				float num12 = num6 * X.ZLINE(num8, flag ? this.eff_one_maxt : X.Mx(this.eff_one_maxt * 0.65f, this.eff_one_maxt - 50f)) * X.NI(1f, 0.88f, X.RAN(ran, 1661));
				if (flag)
				{
					if (this.cur_lgt < num4)
					{
						float num13 = clenb * this.cur_lgt;
						float num14 = num10 + num12;
						if (num14 > num13)
						{
							Md.Col = Md.ColGrd.mulA(1f - X.ZLINE(num14 - num13, 40f)).C;
							if (Md.Col.a <= 0)
							{
								goto IL_0493;
							}
						}
					}
					Md.initForImg(MTRX.EffBlurCircle245, 0);
					float num15 = X.NI(10, 27, X.RAN(ran, 883));
					Md.Rect((num10 + num12) * num3, num11, num15 * num3, num15, false);
				}
				else
				{
					float num16 = num6 * X.NI(0.4f, 0.7f, X.RAN(ran, 690)) * X.ZLINE(this.eff_one_maxt - num8, this.eff_one_maxt * 0.3f);
					float num17 = X.Mx(0f, num12 - num16);
					if (this.cur_lgt < num4)
					{
						float num18 = clenb * this.cur_lgt;
						float num19 = num10 + num17 + num12;
						if (num19 > num18)
						{
							num12 -= num19 - num18;
							if (num12 <= 0f)
							{
								goto IL_0493;
							}
						}
					}
					float num20 = X.NI(3, 12, X.RAN(ran, 883));
					Shape.kadomaruRectExtImg(Md, (num10 + num17 + num12 * 0.5f) * num3, num11, num12 * num3, num20 * 2f, num20, MTRX.EffBlurCircle245, false);
				}
				IL_0493:;
			}
			Md.Identity();
		}

		public bool last_forever
		{
			get
			{
				return this.maxt < 0f;
			}
		}

		public bool check_wall
		{
			get
			{
				return this.max_lgt > 0f;
			}
			set
			{
				if (value)
				{
					this.max_lgt = X.Abs(this.max_lgt);
					this.cur_lgt = X.Mn(-0.1f, -X.Abs(this.cur_lgt));
					return;
				}
				this.max_lgt = -X.Abs(this.max_lgt);
				this.cur_lgt = -this.max_lgt;
			}
		}

		public bool enabled
		{
			get
			{
				return this.t >= 0f;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				if (value)
				{
					this.t = 0f;
					this.cur_lgt = X.Mn(-0.1f, -X.Abs(this.cur_lgt));
					this.fine_t = 0f;
					if (this.maxt <= -2f)
					{
						this.maxt = -1f;
						return;
					}
				}
				else
				{
					this.t = -1f;
				}
			}
		}

		public uint id;

		private float t;

		private float maxt;

		private float x;

		private float y;

		private float agR_;

		private float max_lgt;

		private float radius;

		private float apply_velocity;

		private float near_multiple;

		private float fine_t;

		private float _cos;

		private float _sin;

		private int eff_cnt;

		private float eff_one_maxt;

		private const float FADE_T = 30f;

		public float cur_lgt = -1f;
	}
}
