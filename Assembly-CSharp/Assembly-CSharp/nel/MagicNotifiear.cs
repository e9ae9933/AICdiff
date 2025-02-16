using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MagicNotifiear
	{
		public MagicNotifiear(int cnt = 1)
		{
			this.AHit = new MagicNotifiear.MnHit[cnt];
		}

		public MagicNotifiear Duplicate()
		{
			return new MagicNotifiear(this.AHit.Length).CopyFrom(this);
		}

		public MagicNotifiear CopyFrom(MagicNotifiear Mn)
		{
			this.mnhit_max = 0;
			this.use_flexible_fix = Mn.use_flexible_fix;
			int num = (this.mnhit_max = Mn.mnhit_max);
			if (this.AHit.Length < num)
			{
				Array.Resize<MagicNotifiear.MnHit>(ref this.AHit, num);
			}
			for (int i = 0; i < num; i++)
			{
				if (this.AHit[i] == null)
				{
					this.AHit[i] = new MagicNotifiear.MnHit().CopyFrom(Mn.AHit[i]);
				}
				else
				{
					this.AHit[i].CopyFrom(Mn.AHit[i]);
				}
			}
			return this;
		}

		public M2Ray makeRayForMagic(MagicItem Mg, int id)
		{
			MagicNotifiear.MnHit mnHit = this.AHit[id];
			return Mg.MGC.makeRay(Mg, mnHit.thick, mnHit.wall_hit, mnHit.other_hit);
		}

		public M2Ray SetRay(M2Ray Ray, int id, float agR, float t)
		{
			return this.AHit[id].SetRay(Ray, agR, t);
		}

		public Vector2 drawTo(MeshDrawer Md, Map2d Mp, Vector2 MpPos0, float st, float agR, bool no_hit_pr, float kadomaru_t = 0f, M2Ray RayForCheck = null, RaycastHit2D[] ARayHitBuf = null)
		{
			int num = this.mnhit_max;
			float num2 = X.ANMP((int)kadomaru_t, 44, 1f);
			Vector2 vector = MpPos0;
			float num3 = X.ZLINE(kadomaru_t - 2f, 20f);
			for (int i = 0; i < 2; i++)
			{
				if (Md != null)
				{
					if (i == 0)
					{
						Md.base_x = (Md.base_y = 0f);
						Md.Col = C32.MulA(2281701376U, num3);
					}
					else
					{
						Md.Col = C32.MulA(no_hit_pr ? 4289050848U : 4293175737U, num3);
					}
				}
				vector = MpPos0;
				float num4 = st;
				for (int j = 0; j < num; j++)
				{
					MagicNotifiear.MnHit mnHit = this.AHit[j];
					if (!mnHit.aim_fixed)
					{
						mnHit.agR = agR;
					}
					else
					{
						agR = mnHit.agR;
					}
					if (RayForCheck != null && i == 0)
					{
						mnHit.CalcReachable(RayForCheck, vector, num4, ARayHitBuf);
					}
					Vector3 zero = Vector3.zero;
					if (Md != null)
					{
						mnHit.drawTo(Md, Mp, vector, num2, i == 0, no_hit_pr);
					}
					if (!mnHit.do_not_consider_magic_t)
					{
						num4 = X.Mx(num4 - mnHit.maxt, 0f);
					}
					vector = mnHit.getNextMapPos(Mp, vector);
				}
			}
			return vector;
		}

		public void RayShift(M2Ray Ray, int id, ref float x, ref float y, float fcnt)
		{
			MagicNotifiear.MnHit mnHit = this.AHit[id];
			x += Ray.difmapx * fcnt;
			y += Ray.difmapy * fcnt;
			mnHit.len = X.Mx(0f, mnHit.len - X.LENGTHXY(0f, 0f, Ray.difmapx, Ray.difmapy));
		}

		public MagicNotifiear.TARGETTING getManipulateTargetting(MagicItem Mg, PR Pr, ref int dx, ref int dy, bool is_first)
		{
			MagicNotifiear.MnHit mnHit = this.AHit[0];
			if (mnHit.fnManipulateTargetting == null)
			{
				return MagicNotifiearData.fnManipulateTargettingDefault(Mg, Pr, ref dx, ref dy, is_first);
			}
			return mnHit.fnManipulateTargetting(Mg, Pr, ref dx, ref dy, is_first);
		}

		public MagicNotifiear.MnHit _0
		{
			get
			{
				return this.AHit[0];
			}
		}

		public MagicNotifiear.MnHit _1
		{
			get
			{
				return this.AHit[1];
			}
		}

		public MagicNotifiear.MnHit _2
		{
			get
			{
				return this.AHit[2];
			}
		}

		public MagicNotifiear AddHit(MagicNotifiear.MnHit _Hit)
		{
			if (_Hit.cast_on_autotarget)
			{
				this.use_flexible_fix = true;
			}
			X.pushToEmptyS<MagicNotifiear.MnHit>(ref this.AHit, _Hit, ref this.mnhit_max, 1);
			return this;
		}

		public MagicNotifiear.MnHit GetHit(int i)
		{
			return this.AHit[i];
		}

		public void saveReachableLen(ref float[] A)
		{
			if (A == null || A.Length < this.mnhit_max)
			{
				A = new float[this.mnhit_max];
			}
			for (int i = 0; i < this.mnhit_max; i++)
			{
				A[i] = this.AHit[i].len;
			}
		}

		public void loadReachableLen(float[] A)
		{
			for (int i = 0; i < this.mnhit_max; i++)
			{
				this.AHit[i].len = A[i];
			}
		}

		public int Count
		{
			get
			{
				return this.mnhit_max;
			}
		}

		private MagicNotifiear.MnHit[] AHit;

		private int mnhit_max;

		public bool use_flexible_fix;

		public delegate bool FnManipulateMagic(MagicItem Mg, M2MagicCaster Mv, float fcnt);

		public delegate MagicNotifiear.TARGETTING FnManipulateTargetting(MagicItem Mg, PR Pr, ref int dx, ref int dy, bool is_first);

		public delegate bool FnManipulateTargetMoverPos(MagicItem Mg, M2MagicCaster Pr, float defx, float defy, ref Vector2 Aim, int fix_aim = -1);

		public enum HIT
		{
			CIRCLE
		}

		public enum TARGETTING
		{
			AIM_L = 1,
			AIM_T,
			AIM_R = 4,
			AIM_B = 8,
			AIM_LT = 16,
			AIM_TR = 32,
			AIM_BL = 64,
			AIM_RB = 128,
			_AIM_ALL = 255,
			_AUTO_TARGET
		}

		public sealed class MnHit
		{
			public MagicNotifiear.MnHit CopyFrom(MagicNotifiear.MnHit A)
			{
				this.type = A.type;
				this.x = A.x;
				this.y = A.y;
				this.thick = A.thick;
				this.agR = A.agR;
				this.flags = A.flags;
				this.v0 = A.v0;
				this.accel_mint = A.accel_mint;
				this.accel = A.accel;
				this.accel_maxt = A.accel_maxt;
				this.maxt = A.maxt;
				this.need_fine = A.need_fine;
				this.len = A.len;
				this.time = A.time;
				this.fnManipulateMagic = A.fnManipulateMagic;
				this.fnManipulateTargetting = A.fnManipulateTargetting;
				this.fnFixTargetMoverPos = A.fnFixTargetMoverPos;
				return this;
			}

			public bool wall_hit
			{
				get
				{
					return (this.flags & 1) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 1) : (this.flags & -2));
				}
			}

			public bool other_hit
			{
				get
				{
					return (this.flags & 2) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 2) : (this.flags & -3));
				}
			}

			public bool penetrate
			{
				get
				{
					return (this.flags & 4) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 4) : (this.flags & -5));
				}
			}

			public bool no_draw
			{
				get
				{
					return (this.flags & 8) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 8) : (this.flags & -9));
				}
			}

			public bool auto_target
			{
				get
				{
					return (this.flags & 16) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 16) : (this.flags & -17));
				}
			}

			public bool auto_target_fixable
			{
				get
				{
					return (this.flags & 32) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 32) : (this.flags & -33));
				}
			}

			public bool aim_fixed
			{
				get
				{
					return (this.flags & 64) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 64) : (this.flags & -65));
				}
			}

			public bool do_not_consider_magic_t
			{
				get
				{
					return (this.flags & 128) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 128) : (this.flags & -129));
				}
			}

			public bool penetrate_only_mv
			{
				get
				{
					return (this.flags & 256) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 256) : (this.flags & -257));
				}
			}

			public bool draw_thick_kadomaru
			{
				get
				{
					return (this.flags & 512) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 512) : (this.flags & -513));
				}
			}

			public bool cast_on_autotarget
			{
				get
				{
					return (this.flags & 1024) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 1024) : (this.flags & -1025));
				}
			}

			public bool parabola
			{
				get
				{
					return (this.flags & 2048) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 2048) : (this.flags & -2049));
				}
			}

			public bool no_reset_at_cursor_recast
			{
				get
				{
					return (this.flags & 4096) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 4096) : (this.flags & -4097));
				}
			}

			public bool draw_only_line
			{
				get
				{
					return (this.flags & 8192) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 8192) : (this.flags & -8193));
				}
			}

			public bool no_draw_wide
			{
				get
				{
					return (this.flags & 16384) > 0;
				}
				set
				{
					this.flags = (value ? (this.flags | 16384) : (this.flags & -16385));
				}
			}

			public float Spd(float t)
			{
				if (t < this.accel_mint || this.accel == 0f)
				{
					return this.v0;
				}
				t -= this.accel_mint;
				return X.Mx(0f, this.v0 + ((this.accel_maxt < 0f) ? t : X.Mn(this.accel_maxt, t)) * this.accel);
			}

			public float ReachableLen(float t0)
			{
				if (this.maxt == 0f)
				{
					return 0f;
				}
				float num = X.MMX(0f, t0, this.maxt);
				if (num < this.accel_mint)
				{
					return this.v0 * num;
				}
				if (this.accel_maxt < 0f)
				{
					return this.v0 * num;
				}
				float num2 = this.v0 * this.accel_mint;
				num -= this.accel_mint;
				float num3 = X.Mn(this.accel_maxt, this.maxt - this.accel_mint);
				if (num < num3)
				{
					return num2 + this.v0 * num + 0.5f * this.accel * num * num;
				}
				num2 += this.v0 * num3 + 0.5f * this.accel * num3 * num3;
				num -= num3;
				if (num > 0f)
				{
					num2 += num * (this.v0 + num3 * this.accel);
				}
				return num2;
			}

			public M2Ray SetRay(M2Ray Ray, float agR, float t)
			{
				Ray.PosMapShift(this.x, this.y);
				this.agR = agR;
				Ray.LenM(this.Spd(t)).AngleR(agR).RadiusM(this.thick);
				if (this.wall_hit)
				{
					Ray.check_hit_wall = true;
				}
				else
				{
					Ray.check_hit_wall = false;
				}
				if (this.other_hit)
				{
					Ray.hittype |= HITTYPE.OTHER;
				}
				else
				{
					Ray.hittype &= (HITTYPE)(-9);
				}
				return Ray;
			}

			public MagicNotifiear.MnHit CalcReachable(M2Ray Ray, Vector2 Pos, float st = 0f, RaycastHit2D[] ARayHitBuf = null)
			{
				if (this.do_not_consider_magic_t)
				{
					st = 0f;
				}
				if (this.v0 == 0f && this.accel == 0f)
				{
					this.len = 0f;
				}
				else if (this.parabola)
				{
					float num = X.Mn(X.Mn(this.maxt, this.accel_mint), st);
					this.len = X.Mx(0f, this.ReachableLen(num) - this.ReachableLen(X.Mn(this.accel_mint, st)));
				}
				else
				{
					this.len = this.ReachableLen(this.maxt) - this.ReachableLen(st);
				}
				if (Ray != null)
				{
					Map2d mp = Ray.Mp;
					if (!this.parabola)
					{
						float num2 = this.len;
						HITTYPE hittype = Ray.hittype;
						uint num3 = Ray.popLayerMask();
						Ray.penetrate = this.penetrate;
						float hit_lock_value = Ray.hit_lock_value;
						Ray.HitLock(0f, null);
						Ray.PosMap(Pos.x + this.x, Pos.y + this.y).AngleR(this.agR).LenM(this.len)
							.RadiusM(this.thick * 0.98f);
						if (!this.cast_on_autotarget)
						{
							Ray.check_other_hit = false;
							Ray.check_hit_wall = false;
						}
						else
						{
							if (this.penetrate_only_mv)
							{
								Ray.hittype = (this.wall_hit ? HITTYPE.WALL : HITTYPE.NONE);
							}
							Ray.check_other_hit = this.other_hit;
							Ray.check_hit_wall = this.wall_hit;
						}
						if (!Ray.penetrate && (hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
						{
							hittype &= (HITTYPE)(-12582945);
							if (this.clipRayLength(Ray, HITTYPE._TEMP_REFLECT))
							{
								Ray.LenM(this.len);
							}
						}
						Ray.hittype &= (HITTYPE)(-1048641);
						Ray.hittype |= HITTYPE.TARGET_CHECKER;
						Ray.clearReflectBuffer().Cast(true, (ARayHitBuf == null) ? M2Ray.AHitTest : ARayHitBuf, true);
						Ray.hittype = hittype;
						Ray.setLayerMaskManual(num3);
						if (this.cast_on_autotarget)
						{
							if (this.len > 0f && !this.penetrate)
							{
								this.clipRayLength(Ray, (HITTYPE)12615716);
							}
							if (num2 != this.len)
							{
								Ray.LenM(num2);
							}
						}
						Ray.releaseHittedMax();
						if (hit_lock_value != 0f)
						{
							Ray.HitLock(hit_lock_value, null);
						}
					}
					else
					{
						this.accel_maxt = this.maxt;
						float gravityVelocity = M2DropObject.getGravityVelocity(mp, this.accel);
						float num4 = X.Cos(this.agR);
						float num5 = X.Sin(this.agR);
						Vector2 vector = Pos + new Vector2(this.len * num4, -this.len * num5);
						float num6 = this.v0 * num4;
						float num7 = -this.v0 * num5;
						float num8 = 0.5f / this.v0;
						int num9 = X.IntC(this.maxt / num8);
						float num10 = num6 * num8;
						for (int i = 0; i < num9; i++)
						{
							bool flag = false;
							Vector2 vector2 = vector + new Vector2(num10, num7 * num8);
							float num11 = 1f;
							int num12 = (int)vector2.x;
							int num13 = (int)vector2.y;
							if (!mp.canStandAndNoWater(num12, num13) || (num13 == (int)vector.y + 1 && mp.isLift(num12, num13)))
							{
								flag = true;
								if ((int)vector2.x != (int)vector.x)
								{
									int num14 = ((vector.x < vector2.x) ? ((int)vector2.x) : ((int)vector.x));
									num11 = X.Mn(num11, X.Abs(((float)num14 - vector.x) / (vector2.x - vector.x)));
								}
								if ((int)vector2.y != (int)vector.y)
								{
									int num15 = ((vector.y < vector2.y) ? ((int)vector2.y) : ((int)vector.y));
									num11 = X.Mn(num11, X.Abs(((float)num15 - vector.y) / (vector2.y - vector.y)));
								}
								vector2 = X.NI(vector, vector2, num11);
							}
							vector = vector2;
							num7 += num11 * gravityVelocity * num8;
							if (flag)
							{
								this.accel_maxt = ((float)i + num11) * num8;
								break;
							}
						}
					}
				}
				return this;
			}

			public bool clipRayLength(M2Ray Ray, HITTYPE target)
			{
				int hittedMax = Ray.getHittedMax();
				if (hittedMax <= 0)
				{
					return false;
				}
				float num = this.len * this.len;
				float num2 = num;
				for (int i = 0; i < hittedMax; i++)
				{
					if ((Ray.GetHitted(i).type & target) != HITTYPE.NONE)
					{
						Vector2 hittedMapPos = Ray.getHittedMapPos(i);
						Vector2 mapPos = Ray.getMapPos(0f);
						num = X.Mn(num, X.LENGTHXY2(hittedMapPos.x, hittedMapPos.y, mapPos.x, mapPos.y));
						break;
					}
				}
				if (num2 > num)
				{
					this.len = Mathf.Sqrt(num);
					return true;
				}
				return false;
			}

			public bool hasLength()
			{
				return this.v0 != 0f || this.accel != 0f;
			}

			public MagicNotifiear.MnHit drawTo(MeshDrawer Md, Map2d Mp, Vector2 Pos, float kadomaru_poslevel = 0f, bool draw_border = false, bool no_hit_pr = false)
			{
				if (this.no_draw)
				{
					return this;
				}
				float clenb = Mp.CLENB;
				float num = Mp.map2globalux(Pos.x);
				float num2 = Mp.map2globaluy(Pos.y);
				Md.Rotate(this.agR, false).Translate(num, num2, false);
				float num3 = (this.len + ((this.len == 0f || this.no_draw_wide || no_hit_pr || this.parabola) ? 0f : 0.5f)) * clenb;
				if (num3 > 10f && this.thick > 0f && this.draw_thick_kadomaru)
				{
					num3 += this.thick * clenb;
					float num4 = (this.thick * 2f + (no_hit_pr ? 0f : 0.75f)) * clenb;
					if (draw_border)
					{
						Md.KadomaruRect(num3 * 0.5f, 0f, num3, num4, num4, 4f, false, 0f, 0f, false);
					}
					else
					{
						int num5 = X.IntC(num4 * 3.1415927f / 20f);
						Md.ButtonKadomaruDashed(num3 * 0.5f, 0f, num3, num4, kadomaru_poslevel, num5, 2f);
					}
				}
				else
				{
					float num6 = num3;
					if (num3 > 10f)
					{
						if (draw_border)
						{
							Md.Line(0f, 0f, num3, 0f, 4f, false, 0f, 0f);
						}
						else
						{
							int num7 = X.IntC(num3 / 20f);
							Md.LineDashed(0f, 0f, num3, 0f, kadomaru_poslevel, num7, 2f, false, 0.5f);
						}
					}
					if (this.parabola && this.v0 > 0f)
					{
						float num8 = X.Cos(this.agR);
						float num9 = X.Sin(this.agR);
						float gravityVelocity = M2DropObject.getGravityVelocity(Mp, this.accel);
						Vector2 vector = Pos + new Vector2(this.len * num8, -this.len * num9);
						float num10 = this.v0 * num8;
						float num11 = -this.v0 * num9;
						Pos += new Vector2(num3 * num8, -num3 * num9);
						Md.Identity();
						float num12 = 0.5f / this.v0;
						float num13 = this.accel_maxt / num12;
						float num14 = num10 * num12;
						int num15 = 0;
						float num16;
						float num17;
						if (kadomaru_poslevel < 0.5f)
						{
							num16 = kadomaru_poslevel;
							num17 = kadomaru_poslevel + 0.5f;
						}
						else
						{
							num17 = kadomaru_poslevel;
							num16 = kadomaru_poslevel - 0.5f;
						}
						float num18 = (float)(draw_border ? 4 : 2) * 0.015625f;
						Vector2 vector2;
						for (;;)
						{
							bool flag = false;
							vector2 = vector + new Vector2(num14, num11 * num12);
							float num19 = 1f;
							if ((float)(++num15) > num13)
							{
								num19 = num13 - (float)(num15 - 1);
								flag = true;
								vector2 = vector + new Vector2(num14, num11 * num12) * num19;
							}
							if (num19 > 0f)
							{
								if (draw_border)
								{
									Vector2 vector3 = X.NI(vector, vector2, num19);
									Vector2 vector4 = vector;
									Md.Line(Mp.map2globalux(vector4.x), Mp.map2globaluy(vector4.y), Mp.map2globalux(vector3.x), Mp.map2globaluy(vector3.y), num18, true, 0f, 0f);
								}
								else
								{
									Vector2 vector4 = X.NI(vector, vector2, X.Mn(num19, num16));
									Vector2 vector3 = X.NI(vector, vector2, X.Mn(num19, num17));
									if (kadomaru_poslevel < 0.5f)
									{
										Md.Line(Mp.map2globalux(vector4.x), Mp.map2globaluy(vector4.y), Mp.map2globalux(vector3.x), Mp.map2globaluy(vector3.y), num18, true, 0f, 0f);
									}
									else
									{
										Md.Line(Mp.map2globalux(vector.x), Mp.map2globaluy(vector.y), Mp.map2globalux(vector4.x), Mp.map2globaluy(vector4.y), num18, true, 0f, 0f);
										Md.Line(Mp.map2globalux(vector3.x), Mp.map2globaluy(vector3.y), Mp.map2globalux(vector2.x), Mp.map2globaluy(vector2.y), num18, true, 0f, 0f);
									}
								}
							}
							num11 += num19 * gravityVelocity * num12;
							if (flag)
							{
								break;
							}
							vector = vector2;
						}
						Md.Translate(Mp.map2globalux(vector2.x), Mp.map2globaluy(vector2.y), false);
						num6 = 0f;
					}
					if (this.thick > 0f && !this.draw_only_line)
					{
						float num20 = (this.thick * 2f + (no_hit_pr ? 0f : 0.75f)) * clenb;
						if (draw_border)
						{
							Md.Circle(num6, 0f, num20 / 2f, 4f, false, 0f, 0f);
						}
						else
						{
							int num21 = X.IntC(num20 * 3.1415927f / 20f);
							Md.ButtonKadomaruDashed(num6, 0f, num20, num20, kadomaru_poslevel, num21, 2f);
						}
					}
				}
				Md.Identity();
				return this;
			}

			public Vector2 getNextMapPos(Map2d Mp, Vector2 MpPos)
			{
				if (this.parabola && this.v0 > 0f)
				{
					float gravityVelocity = M2DropObject.getGravityVelocity(Mp, this.accel);
					float num = -X.Sin(this.agR) * this.v0;
					float num2 = X.Cos(this.agR) * this.v0;
					MpPos.x += num2 * (this.accel_maxt + this.accel_mint);
					MpPos.y += num * this.accel_mint + num * this.accel_maxt + 0.5f * gravityVelocity * this.accel_maxt * this.accel_maxt;
				}
				else if (this.len > 0f)
				{
					MpPos.x = MpPos.x + this.x + this.len * X.Cos(this.agR);
					MpPos.y = MpPos.y + this.y - this.len * X.Sin(this.agR);
				}
				else
				{
					MpPos.x += this.x;
					MpPos.y += this.y;
				}
				return MpPos;
			}

			public MagicNotifiear.HIT type;

			public float x;

			public float y;

			public float thick;

			public float agR;

			public int flags = 7;

			public float v0;

			public float accel_mint;

			public float accel;

			public float accel_maxt = -1f;

			public float maxt = -1f;

			public bool need_fine = true;

			public float len;

			public float time;

			public MagicNotifiear.FnManipulateMagic fnManipulateMagic;

			public MagicNotifiear.FnManipulateTargetting fnManipulateTargetting;

			public MagicNotifiear.FnManipulateTargetMoverPos fnFixTargetMoverPos;
		}
	}
}
