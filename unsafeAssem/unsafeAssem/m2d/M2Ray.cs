using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Ray : IM2RayHitAble
	{
		public M2Ray()
		{
			this.AHitted = new M2Ray.M2RayHittedItem[16];
			for (int i = 0; i < 16; i++)
			{
				this.AHitted[i] = new M2Ray.M2RayHittedItem(null);
			}
			if (M2Ray.AHit == null)
			{
				M2Ray.AHit = new RaycastHit2D[16];
				M2Ray.AHitTest = new RaycastHit2D[32];
				M2Ray.SortMv = new SORT<M2Ray.M2RayHittedItem>(null);
			}
		}

		public M2Ray Set(Map2d _Mp, M2Mover _Caster, float _radius = 0f, HITTYPE _hittype = HITTYPE.PR | HITTYPE.EN | HITTYPE.WALL | HITTYPE.OTHER)
		{
			this.hittype = _hittype;
			this.Mp = _Mp;
			this.Caster = _Caster;
			this.radius = _radius * this.Mp.CLENB * 0.015625f;
			this.layerMask = (uint)(((ulong)(-5) & ~(1UL << (IN.gui_layer & 31))) | 4UL);
			this.check_hit_wall = (this.hittype & HITTYPE.WALL) > HITTYPE.NONE;
			this.hittype_to_week_projectile = HITTYPE.BREAK;
			this.hit_lock = 0f;
			this.LenM(0f);
			this.Pos = Vector2.zero;
			this.Dir_ = Vector3.zero;
			this.shape = RAYSHAPE.CIRCLE;
			this.hit_i = 0;
			this.OHFTLinked = null;
			if (this.OHittedFloorT_ != null)
			{
				this.OHittedFloorT_.Clear();
			}
			this.check_hitlock_manual = false;
			this.hit_target_max = 16;
			this.projectile_power = -1;
			this.ReflectAnotherRay = null;
			this.cohitable_min_projectile = 0;
			this.cohitable_allow_berserk = M2Ray.COHIT.NONE;
			this.cohitable_assigned = false;
			this.quit_on_reflection_break = true;
			return this;
		}

		public M2Ray CopyFrom(M2Ray Src)
		{
			this.Mp = Src.Mp;
			this.ACohitableCheck = Src.ACohitableCheck;
			this.Caster = Src.Caster;
			this.Pos = Src.Pos;
			this.Dir_ = Src.Dir_;
			this.len = Src.len;
			this.lenmp = Src.lenmp;
			this.shape = Src.shape;
			this.radius = Src.radius;
			this.projectile_power = Src.projectile_power;
			this.layerMask = Src.layerMask;
			this.hittype = Src.hittype;
			this.hittype_to_week_projectile = Src.hittype_to_week_projectile;
			this.HitLock(Src.hit_lock, null);
			this.check_hitlock_manual = Src.check_hitlock_manual;
			this.hit_target_max = Src.hit_target_max;
			X.Mx(16, this.hit_target_max);
			this.cohitable_allow_berserk = Src.cohitable_allow_berserk;
			this.cohitable_min_projectile = Src.cohitable_min_projectile;
			this.quit_on_reflection_break = Src.quit_on_reflection_break;
			this.hit_i = 0;
			return this;
		}

		public M2Ray LenM(float _len)
		{
			this.lenmp = _len;
			this.len = _len * this.Mp.CLENB * 0.015625f;
			return this;
		}

		public M2Ray RadiusM(float _radius)
		{
			this.radius = _radius * this.Mp.CLENB * 0.015625f;
			return this;
		}

		public M2Ray DirXyM(float dx, float dy)
		{
			if (dx == 0f && dy == 0f)
			{
				this.Dir_ = new Vector3(0f, -1f, 1.5707964f);
				this.len = (this.lenmp = 0f);
			}
			else
			{
				float num = X.GAR2(0f, 0f, dx, -dy);
				this.AngleR(num);
				this.LenM(X.LENGTHXY(dx, dy, 0f, 0f));
			}
			return this;
		}

		public Vector2 Dir
		{
			get
			{
				return this.Dir_;
			}
			set
			{
				float num = X.GAR2(0f, 0f, value.x, value.y);
				this.AngleR(num);
			}
		}

		public M2Ray HitType(HITTYPE _h)
		{
			this.hittype = _h;
			return this;
		}

		public M2Ray AngleR(float agR)
		{
			this.Dir_.Set(X.Cos(agR), X.Sin(agR), agR);
			return this;
		}

		public M2Ray PosMap(float mapx, float mapy)
		{
			this.Pos.Set(this.Mp.ux2effectScreenx(this.Mp.map2ux(mapx)), this.Mp.uy2effectScreeny(this.Mp.map2uy(mapy)));
			return this;
		}

		public M2Ray PosMap(Vector2 _Pos)
		{
			this.Pos.Set(this.Mp.ux2effectScreenx(this.Mp.map2ux(_Pos.x)), this.Mp.uy2effectScreeny(this.Mp.map2uy(_Pos.y)));
			return this;
		}

		public float getAngleR()
		{
			return this.Dir_.z;
		}

		public Vector2 getUPos()
		{
			return this.Pos;
		}

		public Vector2 getMapPos(float shift_level = 0f)
		{
			Vector2 vector = new Vector2(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(this.Pos.x)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(this.Pos.y)));
			if (shift_level != 0f)
			{
				vector.x += this.Dir_.x * this.lenmp * shift_level;
				vector.y += -this.Dir_.y * this.lenmp * shift_level;
			}
			return vector;
		}

		public Vector2 getMapSpeed()
		{
			return new Vector2(this.Dir_.x * this.lenmp, -this.Dir_.y * this.lenmp);
		}

		public M2Ray PosMapShift(float mapx, float mapy)
		{
			this.Pos.x = this.Pos.x + mapx * this.Mp.base_scale * this.Mp.CLEN * 0.015625f;
			this.Pos.y = this.Pos.y - mapy * this.Mp.base_scale * this.Mp.CLEN * 0.015625f;
			return this;
		}

		public float getMaxMapDistance(M2Mover Caster)
		{
			float num = this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(this.Pos.x));
			return X.Abs(Caster.x - num) + (this.len + this.radius) / this.Mp.CLENB * 64f;
		}

		public BDic<IM2RayHitAble, float> OHittedFloorT
		{
			get
			{
				return this.OHFTLinked ?? this.OHittedFloorT_;
			}
		}

		public float hit_lock_value
		{
			get
			{
				return this.hit_lock;
			}
		}

		public M2Ray HitLock(float _hit_lock, BDic<IM2RayHitAble, float> OLink = null)
		{
			this.hit_lock = _hit_lock;
			if (this.hit_lock != 0f)
			{
				if (OLink != null)
				{
					this.OHFTLinked = OLink;
				}
				else if (this.OHittedFloorT_ == null)
				{
					this.OHittedFloorT_ = new BDic<IM2RayHitAble, float>(this.hit_target_max);
				}
			}
			return this;
		}

		public M2Ray clearHittedTarget()
		{
			if (this.OHittedFloorT != null)
			{
				this.OHittedFloorT.Clear();
			}
			return this;
		}

		public void releaseLinkedHittedTarget()
		{
			this.OHFTLinked = null;
		}

		public M2Ray assignHittedTarget(IM2RayHitAble T, float lock_t = 0f)
		{
			BDic<IM2RayHitAble, float> bdic = this.OHittedFloorT;
			if (bdic == null)
			{
				bdic = (this.OHittedFloorT_ = new BDic<IM2RayHitAble, float>(this.hit_target_max));
			}
			bdic[T] = lock_t + this.Mp.floort;
			return this;
		}

		public M2Ray SyncHitLock(M2Ray _Ray)
		{
			if (_Ray != null)
			{
				if (_Ray.OHittedFloorT != null)
				{
					this.OHFTLinked = _Ray.OHittedFloorT;
				}
				else if (this.OHittedFloorT != null)
				{
					_Ray.OHFTLinked = this.OHittedFloorT;
				}
			}
			return this;
		}

		public M2Ray clearReflectBuffer()
		{
			this.hittype &= ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED);
			this.ReflectAnotherRay = null;
			return this;
		}

		private void ensureHitCapacity()
		{
			if (this.AHitted.Length < this.hit_target_max)
			{
				int num = this.AHitted.Length;
				Array.Resize<M2Ray.M2RayHittedItem>(ref this.AHitted, this.hit_target_max);
				for (int i = num; i < this.hit_target_max; i++)
				{
					this.AHitted[i] = new M2Ray.M2RayHittedItem(null);
				}
			}
		}

		public virtual HITTYPE Cast(bool sort = true, RaycastHit2D[] AHit = null, bool hitlock_calc_after = false)
		{
			HITTYPE hittype = HITTYPE.NONE;
			if ((this.hittype & (HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED)) != HITTYPE.NONE)
			{
				hittype |= (((this.hittype & HITTYPE.REFLECT_BROKEN) != HITTYPE.NONE) ? (HITTYPE.BREAK | HITTYPE.REFLECT_BROKEN) : HITTYPE.NONE);
				hittype |= (((this.hittype & HITTYPE.REFLECT_KILLED) != HITTYPE.NONE) ? (HITTYPE.KILLED | HITTYPE.REFLECT_KILLED) : HITTYPE.NONE);
				if ((this.hittype & HITTYPE.WATER_CUT) != HITTYPE.NONE)
				{
					this.checkWaterCut();
				}
				if (this.quit_on_reflection_break)
				{
					this.hittype |= hittype;
					return hittype;
				}
			}
			this.hittype &= ~(HITTYPE.HITTED_PR | HITTYPE.HITTED_EN | HITTYPE.HITTED_WALL | HITTYPE.HITTED_OTHER | HITTYPE.HITTED_WATER | HITTYPE.BREAK | HITTYPE.KILLED | HITTYPE.NO_RETURN_MANA);
			this.ensureHitCapacity();
			AHit = ((AHit == null) ? M2Ray.AHit : AHit);
			M2Ray.Flt.layerMask = (int)this.layerMask;
			int num = this.CastRayAndCollider(AHit);
			if (this.hit_i > 0)
			{
				this.hit_i = 0;
			}
			int num2 = X.Mn(num, M2Ray.AHitTest.Length);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit2D raycastHit2D = AHit[i];
				if (raycastHit2D.collider != null)
				{
					GameObject gameObject = raycastHit2D.collider.gameObject;
					Component component;
					if (gameObject == this.Mp.gameObject)
					{
						if ((this.hittype & HITTYPE.WALL) != HITTYPE.NONE)
						{
							this.hittype |= HITTYPE.HITTED_WALL;
							if ((hittype & HITTYPE.WALL) == HITTYPE.NONE)
							{
								this.AHitted[this.hit_i].SetHitMap(raycastHit2D, this.Mp, this);
								this.hit_i++;
								hittype |= HITTYPE.WALL;
							}
						}
					}
					else if (gameObject.TryGetComponent(typeof(IM2RayHitAble), out component))
					{
						IM2RayHitAble im2RayHitAble = component as IM2RayHitAble;
						RAYHIT rayhit = im2RayHitAble.can_hit(this);
						if (rayhit != RAYHIT.NONE && ((this.hittype & HITTYPE.AUTO_TARGET) == HITTYPE.NONE || (rayhit & RAYHIT.DO_NOT_AUTO_TARGET) == RAYHIT.NONE || ((this.hittype & HITTYPE.TARGET_CHECKER) != HITTYPE.NONE && (rayhit & RAYHIT.FINE_TARGET_CHECKER) != RAYHIT.NONE)))
						{
							M2Mover m2Mover = im2RayHitAble as M2Mover;
							if (this.hit_lock == 0f || this.check_hitlock_manual || !this.checkHitLock(im2RayHitAble))
							{
								HITTYPE hitType = im2RayHitAble.getHitType(this);
								if (hitType != HITTYPE.NONE)
								{
									M2Ray.M2RayHittedItem m2RayHittedItem = null;
									if ((rayhit & RAYHIT.HITTED) != RAYHIT.NONE)
									{
										if ((hitType & HITTYPE.PR) != HITTYPE.NONE)
										{
											if ((this.hittype & HITTYPE.TEMP_OFFLINE) != HITTYPE.NONE || (this.hittype & HITTYPE.PR) == HITTYPE.NONE || (!sort && num3 >= this.hit_target_max))
											{
												goto IL_054E;
											}
											M2MoverPr m2MoverPr = im2RayHitAble as M2MoverPr;
											if ((m2MoverPr == this.Caster && (this.hittype & HITTYPE.BERSERK_MYSELF) == HITTYPE.NONE) || m2MoverPr == null)
											{
												goto IL_054E;
											}
											this.hittype |= HITTYPE.HITTED_PR;
											hittype |= HITTYPE.PR;
											M2Ray.M2RayHittedItem[] ahitted = this.AHitted;
											int num4 = this.hit_i;
											this.hit_i = num4 + 1;
											m2RayHittedItem = ahitted[num4].Set(HITTYPE.PR, im2RayHitAble, m2MoverPr, this.Pos.x, this.Pos.y, this.Dir_);
											num3++;
										}
										else if ((hitType & HITTYPE.EN) != HITTYPE.NONE)
										{
											if ((this.hittype & HITTYPE.TEMP_OFFLINE) != HITTYPE.NONE)
											{
												goto IL_054E;
											}
											if ((hitType & HITTYPE.WALL) != HITTYPE.NONE)
											{
												if ((this.hittype & HITTYPE.EN) == HITTYPE.NONE && (this.hittype & HITTYPE.WALL) == HITTYPE.NONE)
												{
													goto IL_054E;
												}
											}
											else if ((this.hittype & HITTYPE.EN) == HITTYPE.NONE)
											{
												goto IL_054E;
											}
											if ((!sort && num3 >= this.hit_target_max) || (m2Mover == this.Caster && (this.hittype & HITTYPE.BERSERK_MYSELF) == HITTYPE.NONE))
											{
												goto IL_054E;
											}
											m2Mover == null;
											this.hittype |= HITTYPE.HITTED_EN;
											hittype |= HITTYPE.EN;
											HITTYPE hittype2 = HITTYPE.EN;
											if ((hitType & HITTYPE.WALL) != HITTYPE.NONE)
											{
												hittype2 |= HITTYPE.WALL;
												this.hittype |= HITTYPE.HITTED_EN | HITTYPE.HITTED_WALL;
												hittype |= HITTYPE.EN | HITTYPE.WALL;
											}
											if (m2Mover == null)
											{
												M2Ray.M2RayHittedItem[] ahitted2 = this.AHitted;
												int num4 = this.hit_i;
												this.hit_i = num4 + 1;
												m2RayHittedItem = ahitted2[num4].Set(hittype2, this.Mp, im2RayHitAble, raycastHit2D);
											}
											else
											{
												M2Ray.M2RayHittedItem[] ahitted3 = this.AHitted;
												int num4 = this.hit_i;
												this.hit_i = num4 + 1;
												m2RayHittedItem = ahitted3[num4].Set(hittype2, im2RayHitAble, m2Mover, this.Pos.x, this.Pos.y, this.Dir_);
											}
											num3++;
										}
										else if ((this.hittype & HITTYPE.OTHER) != HITTYPE.NONE)
										{
											if ((this.hittype & HITTYPE.TEMP_OFFLINE) != HITTYPE.NONE)
											{
												goto IL_054E;
											}
											M2Ray.M2RayHittedItem[] ahitted4 = this.AHitted;
											int num4 = this.hit_i;
											this.hit_i = num4 + 1;
											m2RayHittedItem = ahitted4[num4].Set(HITTYPE.OTHER, this.Mp, im2RayHitAble, raycastHit2D);
											this.hittype |= HITTYPE.HITTED_OTHER;
											hittype |= HITTYPE.OTHER;
										}
									}
									if ((rayhit & RAYHIT.NO_RETURN_MANA) != RAYHIT.NONE)
									{
										this.hittype |= HITTYPE.NO_RETURN_MANA;
										if (m2RayHittedItem != null)
										{
											m2RayHittedItem.type |= HITTYPE.NO_RETURN_MANA;
										}
										hittype |= HITTYPE.NO_RETURN_MANA;
									}
									if ((rayhit & RAYHIT.KILL) != RAYHIT.NONE)
									{
										this.hittype |= HITTYPE.KILLED;
										if (m2RayHittedItem != null)
										{
											m2RayHittedItem.type |= HITTYPE.KILLED;
										}
										hittype |= HITTYPE.KILLED;
									}
									if ((rayhit & RAYHIT.BREAK) != RAYHIT.NONE)
									{
										hittype |= HITTYPE.BREAK;
										if (m2RayHittedItem != null)
										{
											m2RayHittedItem.type |= HITTYPE.BREAK;
										}
										this.hittype |= HITTYPE.BREAK;
									}
									if (!hitlock_calc_after && this.hit_lock != 0f)
									{
										this.OHittedFloorT[im2RayHitAble] = this.Mp.floort + this.hit_lock;
									}
								}
							}
						}
					}
				}
				IL_054E:;
			}
			if ((this.hittype & HITTYPE.WATER_CUT) != HITTYPE.NONE)
			{
				hittype |= this.checkWaterCut();
			}
			if (this.ACohitableCheck != null && (this.hittype & HITTYPE.TARGET_CHECKER) == HITTYPE.NONE)
			{
				for (int j = this.ACohitableCheck.Count - 1; j >= 0; j--)
				{
					M2Ray m2Ray = this.ACohitableCheck[j];
					float num5;
					if (m2Ray != this && (m2Ray.hittype & HITTYPE.TARGET_CHECKER) == HITTYPE.NONE && !m2Ray.hit_full_count && (this.canCohit(m2Ray) || ((this.hittype & HITTYPE.PR) != HITTYPE.NONE && (m2Ray.hittype & HITTYPE.EN) != HITTYPE.NONE) || ((this.hittype & HITTYPE.EN) != HITTYPE.NONE && (m2Ray.hittype & HITTYPE.PR) != HITTYPE.NONE)) && (this.OHittedFloorT == null || !this.OHittedFloorT.TryGetValue(m2Ray, out num5) || num5 <= this.Mp.floort) && this.checkCohit(m2Ray))
					{
						hittype |= M2Ray.checkReflectCohitRay(this, m2Ray, true, hitlock_calc_after);
					}
				}
			}
			bool flag = (this.hittype & HITTYPE.ONLY_FIRST_BREAKER) > HITTYPE.NONE;
			if (sort || flag)
			{
				if (this.FD_fnSortMv == null)
				{
					this.FD_fnSortMv = new Comparison<M2Ray.M2RayHittedItem>(this.fnSortMv);
				}
				M2Ray.SortMv.qSort(this.AHitted, this.FD_fnSortMv, this.hit_i);
				int num6 = 0;
				Vector3 zero = Vector3.zero;
				for (int k = 0; k < this.hit_i; k++)
				{
					M2Ray.M2RayHittedItem m2RayHittedItem2 = this.AHitted[k];
					if (flag && (m2RayHittedItem2.type & (HITTYPE.PR | HITTYPE.EN | HITTYPE.WALL | HITTYPE.BREAK)) != HITTYPE.NONE)
					{
						if (zero.z == 0f)
						{
							zero = new Vector3(m2RayHittedItem2.hit_ux, m2RayHittedItem2.hit_uy, 1f);
						}
						else if (!X.chkLEN(zero.x, zero.y, m2RayHittedItem2.hit_ux, m2RayHittedItem2.hit_uy, this.radius))
						{
							m2RayHittedItem2.Mv = null;
							m2RayHittedItem2.type = HITTYPE.NONE;
						}
					}
					if ((m2RayHittedItem2.type & HITTYPE.PR_AND_EN) != HITTYPE.NONE && ++num6 > this.hit_target_max)
					{
						m2RayHittedItem2.type = HITTYPE.NONE;
					}
				}
			}
			return hittype;
		}

		public bool checkHitLock(IM2RayHitAble Hi)
		{
			float num;
			return this.hit_lock != 0f && this.OHittedFloorT.TryGetValue(Hi, out num) && (this.hit_lock == -1f || num > this.Mp.floort);
		}

		private bool canCohit(M2Ray OtherRay)
		{
			if (((this.hittype & HITTYPE.PR) != HITTYPE.NONE && (OtherRay.hittype & HITTYPE.PR) != HITTYPE.NONE) || ((this.hittype & HITTYPE.EN) != HITTYPE.NONE && (OtherRay.hittype & HITTYPE.EN) != HITTYPE.NONE))
			{
				if (this.cohitable_allow_berserk != M2Ray.COHIT.NONE)
				{
					if ((this.cohitable_allow_berserk & M2Ray.COHIT.BERSERK_N) != M2Ray.COHIT.NONE && OtherRay.projectile_power < 0)
					{
						return true;
					}
					if ((this.cohitable_allow_berserk & M2Ray.COHIT.BERSERK_M) != M2Ray.COHIT.NONE && OtherRay.projectile_power >= 0)
					{
						return true;
					}
				}
				if (OtherRay.cohitable_allow_berserk != M2Ray.COHIT.NONE)
				{
					if ((OtherRay.cohitable_allow_berserk & M2Ray.COHIT.BERSERK_N) != M2Ray.COHIT.NONE && this.projectile_power < 0)
					{
						return true;
					}
					if ((OtherRay.cohitable_allow_berserk & M2Ray.COHIT.BERSERK_M) != M2Ray.COHIT.NONE && this.projectile_power >= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void setHitLock(IM2RayHitAble Hi)
		{
			if (Hi != null && this.hit_lock != 0f && this.OHittedFloorT != null)
			{
				this.OHittedFloorT[Hi] = this.Mp.floort + this.hit_lock;
			}
		}

		public void setHitLock(IM2RayHitAble Hi, int lock_t)
		{
			if (Hi != null && this.hit_lock != 0f && this.OHittedFloorT != null)
			{
				this.OHittedFloorT[Hi] = this.Mp.floort + (float)lock_t;
			}
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return -1f;
		}

		private HITTYPE checkWaterCut()
		{
			Vector2 mapPos = this.getMapPos(0f);
			float num = this.lenmp;
			if (CCON.isWater(this.Mp.getConfig((int)(mapPos.x + this.Dir_.x * num), (int)(mapPos.y - this.Dir_.y * num))))
			{
				this.hittype |= HITTYPE.HITTED_WATER | HITTYPE.BREAK;
				return HITTYPE.HITTED_WATER | HITTYPE.BREAK;
			}
			return HITTYPE.NONE;
		}

		private int fnSortMv(M2Ray.M2RayHittedItem Ra, M2Ray.M2RayHittedItem Rb)
		{
			float num = X.LENGTHXY2(this.Pos.x, this.Pos.y, Ra.hit_ux, Ra.hit_uy);
			float num2 = X.LENGTHXY2(this.Pos.x, this.Pos.y, Rb.hit_ux, Rb.hit_uy);
			if (num == num2)
			{
				return 0;
			}
			if (num >= num2)
			{
				return 1;
			}
			return -1;
		}

		public int CastRayAndCollider(RaycastHit2D[] AHit = null)
		{
			return M2Ray.CastRayAndColliderS(this, this.shape, this.Pos, this.radius, this.Dir_, this.len, this.layerMask, AHit);
		}

		public static int CastRayAndColliderS(M2Ray Ray, RAYSHAPE shape, Vector2 Pos, float radius, Vector2 Dir_, float len, uint layerMask, RaycastHit2D[] AHit = null)
		{
			int num = 0;
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 0f;
			if (shape <= RAYSHAPE.DAIA)
			{
				if (shape == RAYSHAPE.RECT)
				{
					goto IL_006E;
				}
				if (shape == RAYSHAPE.DAIA)
				{
					num4 = 45f;
					goto IL_006E;
				}
			}
			else
			{
				switch (shape)
				{
				case RAYSHAPE.RECT_HH:
					num2 = 1.33f;
					goto IL_006E;
				case RAYSHAPE.RECT_HH2:
					num2 = 2f;
					goto IL_006E;
				case RAYSHAPE.RECT_HH2C:
					num3 = 0.5f;
					goto IL_006E;
				default:
					if (shape == RAYSHAPE.RECT_VH)
					{
						num3 = 1.33f;
						goto IL_006E;
					}
					break;
				}
			}
			num2 = 0f;
			num3 = radius;
			IL_006E:
			if (num2 == 0f)
			{
				if (AHit != null)
				{
					num = Physics2D.CircleCastNonAlloc(Pos, radius, Dir_, AHit, len, (int)layerMask);
				}
			}
			else
			{
				num2 *= radius;
				num3 *= radius;
				if (AHit != null)
				{
					num = Physics2D.BoxCastNonAlloc(Pos, new Vector2(num2 * 2f, num3 * 2f), num4, Dir_, AHit, len, (int)layerMask);
				}
			}
			return num;
		}

		private bool checkCohit(M2Ray RayA)
		{
			bool flag = this.len > 0.3125f;
			bool flag2 = RayA.len > 0.3125f;
			if (flag && flag2)
			{
				return X.chkLENLineCirc(this.Pos.x, this.Pos.y, this.Pos.x + this.Dir_.x * this.len, this.Pos.y + this.Dir_.y * this.len, RayA.Pos.x + RayA.Dir_.x * RayA.len * 0.5f, RayA.Pos.y + RayA.Dir_.y * RayA.len * 0.5f, this.radius + RayA.radius) || X.chkLENLineCirc(RayA.Pos.x, RayA.Pos.y, RayA.Pos.x + RayA.Dir_.x * RayA.len, RayA.Pos.y + RayA.Dir_.y * RayA.len, this.Pos.x + this.Dir_.x * this.len * 0.5f, this.Pos.y + this.Dir_.y * this.len * 0.5f, this.radius + RayA.radius) || X.vec_vec_X(this.Pos.x, this.Pos.y, this.Pos.x + this.Dir_.x * this.len, this.Pos.y + this.Dir_.y * this.len, RayA.Pos.x, RayA.Pos.y, RayA.Pos.x + RayA.Dir_.x * RayA.len, RayA.Pos.y + RayA.Dir_.y * RayA.len);
			}
			if (flag2)
			{
				return X.chkLENLineCirc(RayA.Pos.x, RayA.Pos.y, RayA.Pos.x + RayA.Dir_.x * RayA.len, RayA.Pos.y + RayA.Dir_.y * RayA.len, this.Pos.x + this.Dir_.x * this.len * 0.5f, this.Pos.y + this.Dir_.y * this.len * 0.5f, this.radius + RayA.radius);
			}
			return X.chkLENLineCirc(this.Pos.x, this.Pos.y, this.Pos.x + this.Dir_.x * this.len, this.Pos.y + this.Dir_.y * this.len, RayA.Pos.x + RayA.Dir_.x * RayA.len * 0.5f, RayA.Pos.y + RayA.Dir_.y * RayA.len * 0.5f, this.radius + RayA.radius);
		}

		public static HITTYPE checkReflectCohitRay(M2Ray MyRay, M2Ray Ray, bool assign_to_list = false, bool hitlock_calc_after = false)
		{
			int num = X.Abs(MyRay.projectile_power);
			int num2 = X.Abs(Ray.projectile_power);
			HITTYPE hittype = HITTYPE.NONE;
			if ((num >= num2 || (Ray.cohitable_min_projectile > 0 && num >= Ray.cohitable_min_projectile)) && MyRay.hittype_to_week_projectile != HITTYPE.NONE)
			{
				HITTYPE hittype2 = ((MyRay.hittype_to_week_projectile == HITTYPE.BREAK) ? HITTYPE.REFLECT_BROKEN : ((MyRay.hittype_to_week_projectile == HITTYPE.KILLED) ? HITTYPE.REFLECT_KILLED : HITTYPE.REFLECTED));
				Ray.ReflectAnotherRay = MyRay;
				hittype2 = ((Ray.projectile_power > 0) ? hittype2 : HITTYPE.REFLECTED);
				Ray.hittype |= hittype2;
				if (assign_to_list)
				{
					Ray.addReflected(MyRay, hittype2, hitlock_calc_after || MyRay.projectile_power > 0);
				}
			}
			if ((num <= num2 || (MyRay.cohitable_min_projectile > 0 && num2 >= MyRay.cohitable_min_projectile)) && Ray.hittype_to_week_projectile != HITTYPE.NONE)
			{
				HITTYPE hittype3 = ((Ray.hittype_to_week_projectile == HITTYPE.BREAK) ? HITTYPE.REFLECT_BROKEN : ((Ray.hittype_to_week_projectile == HITTYPE.KILLED) ? HITTYPE.REFLECT_KILLED : HITTYPE.REFLECTED));
				hittype3 = ((MyRay.projectile_power > 0) ? hittype3 : HITTYPE.REFLECTED);
				MyRay.hittype |= hittype3;
				MyRay.ReflectAnotherRay = Ray;
				hittype = hittype3;
				if (assign_to_list)
				{
					MyRay.addReflected(Ray, hittype3, hitlock_calc_after || Ray.projectile_power > 0);
				}
			}
			return hittype;
		}

		public M2Ray clearTempReflect()
		{
			this.hittype &= ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED);
			return this;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			return RAYHIT.DO_NOT_AUTO_TARGET;
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return M2Ray.checkReflectCohitRay(this, Ray, false, false);
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return 0;
		}

		public bool hit_full_count
		{
			get
			{
				return this.hit_i >= this.hit_target_max;
			}
		}

		public void addReflected(M2Ray RayA, HITTYPE _type, bool hitlock_calc_after = false)
		{
			this.ensureHitCapacity();
			if (this.hit_i >= this.hit_target_max)
			{
				return;
			}
			M2Ray.M2RayHittedItem[] ahitted = this.AHitted;
			int num = this.hit_i;
			this.hit_i = num + 1;
			M2Ray.M2RayHittedItem m2RayHittedItem = ahitted[num].Set(_type, RayA);
			if ((_type & HITTYPE.REFLECT_KILLED) != HITTYPE.NONE)
			{
				m2RayHittedItem.type |= HITTYPE.KILLED;
			}
			if ((_type & HITTYPE.REFLECT_BROKEN) != HITTYPE.NONE)
			{
				m2RayHittedItem.type |= HITTYPE.BREAK;
			}
			Vector2 mergeUPos = M2Ray.getMergeUPos(this, RayA);
			mergeUPos.x += (this.Dir_.x + RayA.Dir_.x) * 0.01f;
			mergeUPos.y += (this.Dir_.y + RayA.Dir_.y) * 0.01f;
			m2RayHittedItem.hit_ux = mergeUPos.x;
			m2RayHittedItem.hit_uy = mergeUPos.y;
			m2RayHittedItem.mv_mapx = this.Mp.globaluxToMapx(RayA.Pos.x);
			m2RayHittedItem.mv_mapy = this.Mp.globaluyToMapy(RayA.Pos.y);
			if (hitlock_calc_after)
			{
				if (this.OHittedFloorT == null)
				{
					this.OHittedFloorT_ = new BDic<IM2RayHitAble, float>(2);
				}
				this.OHittedFloorT[RayA] = this.Mp.floort + 30f;
			}
		}

		public static Vector2 getMergeUPos(M2Ray MyRay, M2Ray Ray)
		{
			if ((MyRay.Dir_.x == 0f && MyRay.Dir_.y == 0f) || (Ray.Dir_.x == 0f && Ray.Dir_.y == 0f))
			{
				float num = X.GAR2(MyRay.Pos.x, MyRay.Pos.y, Ray.Pos.x, Ray.Pos.y);
				float num2 = X.LENGTHXYS(MyRay.Pos.x, MyRay.Pos.y, Ray.Pos.x, Ray.Pos.y);
				num2 = X.Mn(num2, X.Mx(0.0625f, MyRay.radius));
				return new Vector2(MyRay.Pos.x + num2 * X.Cos(num), MyRay.Pos.y + num2 * X.Sin(num));
			}
			Vector3 vector = X.crosspoint(MyRay.Pos.x, MyRay.Pos.y, MyRay.Pos.x + MyRay.Dir_.x * MyRay.len, MyRay.Pos.y + MyRay.Dir_.y * MyRay.len, Ray.Pos.x, Ray.Pos.y, Ray.Pos.x + Ray.Dir_.x * Ray.len, Ray.Pos.y + Ray.Dir_.y * Ray.len);
			if (vector.z == 0f)
			{
				Vector2 vector2 = (MyRay.Pos + Ray.Pos) * 0.5f;
				if (!X.chkLEN(MyRay.Pos.x, MyRay.Pos.y, vector2.x, vector2.y, MyRay.len))
				{
					float num3 = X.GAR2(MyRay.Pos.x, MyRay.Pos.y, vector2.x, vector2.y);
					vector2.Set(MyRay.Pos.x + X.Cos(num3) * MyRay.len, MyRay.Pos.y + X.Sin(num3) * MyRay.len);
				}
				return vector2;
			}
			if (!X.chkLEN(MyRay.Pos.x, MyRay.Pos.y, vector.x, vector.y, MyRay.len))
			{
				float num4 = X.GAR2(MyRay.Pos.x, MyRay.Pos.y, vector.x, vector.y);
				vector.Set(MyRay.Pos.x + X.Cos(num4) * MyRay.len, MyRay.Pos.y + X.Sin(num4) * MyRay.len, 1f);
			}
			return vector;
		}

		public static M2Ray.M2RayHittedItem findAutoTarget(M2Mover CasterMv, float cx, float cy, float search_radius, float ray_thick, HITTYPE ht, int projectile_power, float shift_len = 0f, float shift_agR = 0f, M2Mover NearSearch = null, M2Mover TargetMv = null)
		{
			Map2d mp = CasterMv.Mp;
			if (M2Ray.RaySearch == null || M2Ray.RaySearch.Mp != mp)
			{
				M2Ray.RaySearch = new M2Ray().Set(mp, CasterMv, 0f, HITTYPE.PR | HITTYPE.EN | HITTYPE.WALL | HITTYPE.OTHER);
			}
			bool flag = (ht & HITTYPE.WALL) > HITTYPE.NONE;
			M2Ray.RaySearch.hittype = ht & ~HITTYPE.WALL;
			M2Ray.RaySearch.PosMap(cx, cy);
			M2Ray.RaySearch.projectile_power = projectile_power;
			M2Ray.RaySearch.LenM(shift_len).AngleR(shift_agR).RadiusM(search_radius)
				.Cast(false, null, true);
			int hittedMax = M2Ray.RaySearch.getHittedMax();
			if (hittedMax == 0)
			{
				return null;
			}
			Vector2 mapPos = M2Ray.RaySearch.getMapPos(0f);
			M2Ray.M2RayHittedItem m2RayHittedItem = null;
			float num = -1f;
			for (int i = 0; i < hittedMax; i++)
			{
				M2Ray.M2RayHittedItem hitted = M2Ray.RaySearch.GetHitted(i);
				if (hitted.Hit != null)
				{
					float num2 = hitted.Hit.auto_target_priority(CasterMv);
					if (num2 > 0f)
					{
						float num3 = 0f;
						float num4 = 0f;
						if (hitted.Mv != null)
						{
							num4 = X.Mx(hitted.Mv.sizex, hitted.Mv.sizey);
						}
						if (NearSearch != null)
						{
							float num5 = X.Mx(0f, X.LENGTHXYS(hitted.mv_mapx, hitted.mv_mapy, NearSearch.x, NearSearch.y) - num4);
							num3 += ((num5 >= 4f) ? ((num5 - 4f) / 5f) : X.NI(3.8f, 1f, num5 / 4f));
						}
						float num6 = X.LENGTHXYS(hitted.mv_mapx, hitted.mv_mapy, mapPos.x + X.Cos(shift_agR) * shift_len, mapPos.y - X.Sin(shift_agR) * shift_len);
						num3 += 30f * (1f - X.ZSIN(X.Abs(X.angledifR(shift_agR, mp.GAR(cx, cy, hitted.mv_mapx, hitted.mv_mapy))), 1.5707964f));
						num3 += 15f * (1f - X.ZPOW(X.LENGTHXYS(CasterMv.x, CasterMv.y, hitted.mv_mapx, hitted.mv_mapy) - 2f, 11f));
						num3 /= X.NI(1, 12, X.ZLINE(num6, search_radius));
						if (hitted.Mv != null)
						{
							num3 += (float)((CasterMv.x < hitted.Mv.x == CAim._XD(CasterMv.aim, 1) > 0) ? 2000 : 0);
						}
						num3 *= num2;
						if (flag && !mp.canThroughXy(mapPos.x, mapPos.y, hitted.mv_mapx, hitted.mv_mapy, ray_thick))
						{
							num3 *= 0.05f;
						}
						if (num < 0f || num < num3)
						{
							num = num3;
							m2RayHittedItem = hitted;
						}
					}
				}
			}
			return m2RayHittedItem;
		}

		public M2Ray drawDebug(float visible_second = 1f)
		{
			return this;
		}

		public float difmapx
		{
			get
			{
				return this.Dir_.x * this.lenmp;
			}
		}

		public float difmapy
		{
			get
			{
				return -this.Dir_.y * this.lenmp;
			}
		}

		public float radius_map
		{
			get
			{
				return this.radius * this.Mp.rCLENB * 64f;
			}
		}

		public float difx
		{
			get
			{
				return this.Dir_.x;
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public float CLENB
		{
			get
			{
				return this.Mp.CLENB;
			}
		}

		public bool hit_pr
		{
			get
			{
				return (this.hittype & HITTYPE.PR) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.PR;
					return;
				}
				this.hittype &= ~HITTYPE.PR;
			}
		}

		public bool hit_en
		{
			get
			{
				return (this.hittype & HITTYPE.EN) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.EN;
					return;
				}
				this.hittype &= ~HITTYPE.EN;
			}
		}

		public bool reflected
		{
			get
			{
				return (this.hittype & HITTYPE.REFLECTED) > HITTYPE.NONE;
			}
		}

		public uint popLayerMask()
		{
			return this.layerMask;
		}

		public void setLayerMaskManual(uint _layerMask)
		{
			this.layerMask = _layerMask;
		}

		public bool check_hit_wall
		{
			get
			{
				return (this.hittype & HITTYPE.WALL) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.WALL;
					this.layerMask |= 1U << this.Mp.M2D.map_object_layer;
					return;
				}
				this.hittype &= ~HITTYPE.WALL;
				this.layerMask &= ~(1U << this.Mp.M2D.map_object_layer);
			}
		}

		public bool check_other_hit
		{
			get
			{
				return (this.hittype & HITTYPE.OTHER) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.OTHER;
					return;
				}
				this.hittype &= ~HITTYPE.OTHER;
			}
		}

		public bool check_mv_hit
		{
			get
			{
				return (this.hittype & HITTYPE.PR_AND_EN) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.PR_AND_EN;
					return;
				}
				this.hittype &= ~(HITTYPE.PR | HITTYPE.EN);
			}
		}

		public bool penetrate
		{
			get
			{
				return (this.hittype & HITTYPE.PENETRATE) > HITTYPE.NONE;
			}
			set
			{
				if (value)
				{
					this.hittype |= HITTYPE.PENETRATE;
					return;
				}
				this.hittype &= ~HITTYPE.PENETRATE;
			}
		}

		public int getHittedMax()
		{
			return this.hit_i;
		}

		public void releaseHittedMax()
		{
			this.hit_i = 0;
		}

		public M2Ray.M2RayHittedItem GetHitted(int i)
		{
			return this.AHitted[i];
		}

		public M2Ray.M2RayHittedItem GetHittedType(HITTYPE t)
		{
			for (int i = 0; i < this.hit_i; i++)
			{
				M2Ray.M2RayHittedItem m2RayHittedItem = this.AHitted[i];
				if ((m2RayHittedItem.type & t) != HITTYPE.NONE)
				{
					return m2RayHittedItem;
				}
			}
			return null;
		}

		public M2Ray.M2RayHittedItem GetHitted(M2Ray.fnRayHitItemSearch Fn)
		{
			for (int i = 0; i < this.hit_i; i++)
			{
				M2Ray.M2RayHittedItem m2RayHittedItem = this.AHitted[i];
				if (Fn(m2RayHittedItem))
				{
					return m2RayHittedItem;
				}
			}
			return null;
		}

		public Vector2 getHittedMapPos(int i)
		{
			M2Ray.M2RayHittedItem m2RayHittedItem = this.AHitted[i];
			return new Vector2(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(m2RayHittedItem.hit_ux)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(m2RayHittedItem.hit_uy)));
		}

		public float get_hit_lock()
		{
			return this.hit_lock;
		}

		public const float AT_PRIORITY_ENEMY = 10f;

		public const float AT_PRIORITY_OBJECT_P = 4f;

		public const float AT_PRIORITY_OBJECT = 2f;

		public const float AT_PRIORITY_BASIC = 1f;

		public static RaycastHit2D[] AHit;

		public static RaycastHit2D[] AHitTest;

		public static SORT<M2Ray.M2RayHittedItem> SortMv;

		public static ContactFilter2D Flt;

		public List<M2Ray> ACohitableCheck;

		public Map2d Mp;

		public M2Mover Caster;

		public AttackInfo Atk;

		public Vector2 Pos;

		private Vector3 Dir_;

		public bool check_hitlock_manual;

		public int projectile_power = -1;

		public float len;

		public float lenmp;

		public float radius;

		private uint layerMask;

		public HITTYPE hittype;

		public HITTYPE hittype_to_week_projectile = HITTYPE.BREAK;

		public M2Ray.M2RayHittedItem[] AHitted;

		public const int HIT_MAX_DEFAULT = 16;

		public int hit_target_max = 16;

		public int hit_i;

		public RAYSHAPE shape;

		public bool cohitable_assigned;

		public M2Ray.COHIT cohitable_allow_berserk;

		public int cohitable_min_projectile;

		public bool quit_on_reflection_break = true;

		public M2Ray ReflectAnotherRay;

		private BDic<IM2RayHitAble, float> OHittedFloorT_;

		private BDic<IM2RayHitAble, float> OHFTLinked;

		private float hit_lock;

		private Comparison<M2Ray.M2RayHittedItem> FD_fnSortMv;

		private FnEffectRun FD_fnDrawGizmo;

		private static M2Ray RaySearch;

		public delegate bool fnRayHitItemSearch(M2Ray.M2RayHittedItem V);

		public class M2RayHittedItem
		{
			public M2RayHittedItem(M2Ray.M2RayHittedItem Src = null)
			{
				if (Src != null)
				{
					this.Set(Src);
				}
			}

			public M2Ray.M2RayHittedItem Set(M2Ray.M2RayHittedItem Src)
			{
				this.type = Src.type;
				this.Hit = Src.Hit;
				this.Mv = Src.Mv;
				this.hit_ux = Src.hit_ux;
				this.hit_uy = Src.hit_uy;
				this.mv_mapx = Src.mv_mapx;
				this.mv_mapy = Src.mv_mapy;
				return this;
			}

			public M2Ray.M2RayHittedItem SetMapPos(HITTYPE _type, IM2RayHitAble _Hit, M2Mover _Mv, float mapcx, float mapcy, Vector3 Dir)
			{
				return this.Set(_type, _Hit, _Mv, _Mv.Mp.ux2effectScreenx(_Mv.Mp.map2ux(mapcx)), _Mv.Mp.uy2effectScreeny(_Mv.Mp.map2uy(mapcy)), Dir);
			}

			public M2Ray.M2RayHittedItem SetHitMap(RaycastHit2D HitPos, Map2d Mp, M2Ray Ray)
			{
				this.Set(HITTYPE.WALL, null);
				Vector2 mapPos = Ray.getMapPos(0f);
				float num = X.Mn(0.05f, Ray.radius_map);
				float num2 = Ray.lenmp + 0.55f + Ray.radius_map;
				Vector3 vector = Mp.BCC.crosspoint(mapPos.x, mapPos.y, mapPos.x + Ray.Dir.x * num2, mapPos.y - Ray.Dir.y * num2, num, num, null, false, null);
				if (vector.z >= 2f)
				{
					this.mv_mapx = vector.x - Ray.Dir.x * num * 0.5f;
					this.mv_mapy = vector.y + Ray.Dir.y * num * 0.5f;
				}
				else
				{
					this.mv_mapx = mapPos.x + Ray.Dir.x * (num2 - num);
					this.mv_mapy = mapPos.y - Ray.Dir.y * (num2 - num);
				}
				this.hit_ux = Mp.ux2effectScreenx(Mp.map2ux(this.mv_mapx));
				this.hit_uy = Mp.uy2effectScreeny(Mp.map2uy(this.mv_mapy));
				return this;
			}

			public M2Ray.M2RayHittedItem Set(HITTYPE _type, IM2RayHitAble _Hit, M2Mover _Mv, float ray_cen_ux, float ray_cen_uy, Vector3 Dir)
			{
				this.Set(_type, _Hit);
				this.Mv = _Mv;
				this.mv_mapx = _Mv.x;
				this.mv_mapy = _Mv.y;
				Vector2 vector = this.Mv.calcHitUPosFromRay(this.Mv.Mp.uxToMapx(this.Mv.Mp.M2D.effectScreenx2ux(ray_cen_ux)), this.Mv.Mp.uyToMapy(this.Mv.Mp.M2D.effectScreeny2uy(ray_cen_uy)), Dir);
				this.hit_ux = _Mv.Mp.ux2effectScreenx(_Mv.Mp.map2ux(vector.x));
				this.hit_uy = _Mv.Mp.uy2effectScreeny(_Mv.Mp.map2uy(vector.y));
				return this;
			}

			public M2Ray.M2RayHittedItem Set(HITTYPE _type, IM2RayHitAble _Hit)
			{
				this.type = _type;
				this.Hit = _Hit;
				this.Mv = _Hit as M2Mover;
				return this;
			}

			public M2Ray.M2RayHittedItem Set(HITTYPE _type, Map2d Mp, IM2RayHitAble _Hit, RaycastHit2D HitPos)
			{
				this.Set(_type, _Hit);
				this.hit_ux = HitPos.point.x;
				this.hit_uy = HitPos.point.y;
				MonoBehaviour monoBehaviour = _Hit as MonoBehaviour;
				if (monoBehaviour != null)
				{
					Vector3 localPosition = monoBehaviour.transform.localPosition;
					this.mv_mapx = Mp.uxToMapx(localPosition.x);
					this.mv_mapy = Mp.uyToMapy(localPosition.y);
				}
				else
				{
					this.mv_mapx = (float)Mp.clms * 0.5f;
					this.mv_mapy = (float)Mp.rows * 0.5f;
				}
				return this;
			}

			public HITTYPE type;

			public IM2RayHitAble Hit;

			public M2Mover Mv;

			public float hit_ux;

			public float hit_uy;

			public float mv_mapx;

			public float mv_mapy;

			public static M2Ray.M2RayHittedItem Buffer = new M2Ray.M2RayHittedItem(null);
		}

		public enum COHIT
		{
			NONE,
			BERSERK_N,
			BERSERK_M,
			BERSERK_ALL
		}
	}
}
