using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2BlockColliderContainer
	{
		public M2BlockColliderContainer(Map2d _Mp, M2Mover _BelongTo = null)
		{
			this.Mp = _Mp;
			this.Bounds = new DRect("");
			this.BoundsLift = new DRect("");
			this.LinePool = new ClsPoolD<M2BlockColliderContainer.BCCLine>(() => new M2BlockColliderContainer.BCCLine(this), 8);
			if (M2BlockColliderContainer.BufRc == null)
			{
				M2BlockColliderContainer.BufRc = new DRect("BufRc");
			}
			if (_BelongTo != null)
			{
				this.BelongTo = _BelongTo;
			}
		}

		public M2BlockColliderContainer InitS(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.is_prepared_ = false;
			if (this.AOtherLines != null)
			{
				this.AOtherLines.Clear();
			}
			if (this.ALsn != null)
			{
				this.ALsn.Clear();
			}
			if (this.AFootLsn != null)
			{
				this.AFootLsn.Clear();
			}
			return this.Dispose();
		}

		public M2BlockColliderContainer.BCCLine addAdditionalLine(M2BorderPt Ps, M2BorderPt Pd, bool is_lift = false)
		{
			return this.addAdditionalLine(new M2BlockColliderContainer.BCCLine(this, -1, Ps, Pd, true, is_lift), false);
		}

		public M2BlockColliderContainer.BCCLine addAdditionalLine(M2BlockColliderContainer.BCCLine _Bcc, bool check_dupe = true)
		{
			if (this.AOtherLines == null)
			{
				this.AOtherLines = new List<M2BlockColliderContainer.BCCLine>();
			}
			else if (check_dupe && this.AOtherLines.IndexOf(_Bcc) >= 0)
			{
				return _Bcc;
			}
			this.AOtherLines.Add(_Bcc);
			if (this.is_prepared_ && _Bcc.is_lift)
			{
				if (this.ALift == null)
				{
					this.ALift = new List<M2BlockColliderContainer.BCCLine>(1);
					this.BoundsLift.key = "";
				}
				this.ALift.Add(_Bcc);
				this.expandLiftBounds(_Bcc);
			}
			return _Bcc;
		}

		public void remAdditionalLine(M2BlockColliderContainer.BCCLine _Bcc)
		{
			if (this.AOtherLines != null)
			{
				this.AOtherLines.Remove(_Bcc);
			}
		}

		public M2BlockColliderContainer Dispose()
		{
			this.LinePool.releaseAll(true, true);
			this.AALine = null;
			if (this.PoolListEvl != null)
			{
				this.PoolListEvl.releaseAll(true, true);
			}
			if (this.PoolListFl != null)
			{
				this.PoolListFl.releaseAll(true, true);
			}
			if (this.PoolListMd != null)
			{
				this.PoolListMd.releaseAll(true, true);
			}
			return this;
		}

		public void setPathCount(int i)
		{
			this.gen_id++;
			this.Dispose();
			this.has_dangerous_chip = false;
			this.Bounds.key = (this.BoundsLift.key = "");
			this.Bounds.Set(0f, 0f, 0f, 0f);
			this.BoundsLift.Set(0f, 0f, 0f, 0f);
			if (this.AFootable == null)
			{
				this.AFootable = new List<M2BlockColliderContainer.BCCLine>(16);
			}
			else
			{
				this.AFootable.Clear();
			}
			if (this.ALift != null)
			{
				this.ALift.Clear();
			}
			if (this.ALsn != null)
			{
				for (int j = this.ALsn.Count - 1; j >= 0; j--)
				{
					this.ALsn[j].BCCInitializing(this);
				}
			}
			BList<M2BlockColliderContainer.BCCLine> blist = null;
			if (this.AOtherLines != null)
			{
				int count = this.AOtherLines.Count;
				for (int k = 0; k < count; k++)
				{
					M2BlockColliderContainer.BCCLine bccline = this.AOtherLines[k];
					if (!bccline.is_lift)
					{
						if (blist == null)
						{
							blist = ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(count);
						}
						blist.Add(bccline);
						this.Bounds.ExpandRc(bccline, false);
					}
				}
			}
			if (blist != null)
			{
				this.AALine = new M2BlockColliderContainer.BCCLine[i + 1][];
				this.AALine[i] = blist.ToArray();
				ListBuffer<M2BlockColliderContainer.BCCLine>.Release(blist);
			}
			else
			{
				this.AALine = new M2BlockColliderContainer.BCCLine[i][];
			}
			this.is_prepared_ = true;
		}

		public void SetPath(int path_index, List<BDVector> Apos, FnZoom fnCalcX, FnZoom fnCalcY)
		{
			int num = Apos.Count;
			using (BList<M2BlockColliderContainer.BCCLine> blist = ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(num))
			{
				M2BorderPt m2BorderPt = Apos[num - 1] as M2BorderPt;
				for (int i = 0; i < num; i++)
				{
					M2BorderPt m2BorderPt2 = Apos[i] as M2BorderPt;
					M2BlockColliderContainer.BCCLine bccline = this.createLine(path_index, m2BorderPt, m2BorderPt2);
					if (bccline != null && bccline.valid)
					{
						blist.Add(bccline);
						if (this.Bounds.key == "")
						{
							this.Bounds.Set(bccline);
							this.Bounds.key = "normal";
						}
						else
						{
							this.Bounds.ExpandRc(bccline, true);
						}
					}
					m2BorderPt = m2BorderPt2;
				}
				num = blist.Count;
				if (num > 0)
				{
					M2BlockColliderContainer.BCCLine bccline2 = blist[num - 1];
					M2BlockColliderContainer.BCCLine bccline3 = blist[0];
					for (int j = 1; j <= num; j++)
					{
						M2BlockColliderContainer.BCCLine bccline4 = blist[j % num];
						bccline3.setSideLink(bccline2, bccline4);
						bccline2 = bccline3;
						bccline3 = bccline4;
					}
				}
				if (this.Mp.BCC == this)
				{
					for (int k = num - 1; k >= 0; k--)
					{
						M2BlockColliderContainer.BCCLine bccline5 = blist[k];
						if (!bccline5.valid)
						{
							blist.RemoveAt(k);
						}
						else if (bccline5._yd < 0)
						{
							this.AFootable.Add(bccline5);
						}
					}
				}
				else
				{
					for (int l = num - 1; l >= 0; l--)
					{
						M2BlockColliderContainer.BCCLine bccline6 = blist[l];
						if (bccline6._yd < 0)
						{
							this.AFootable.Add(bccline6);
						}
					}
				}
				this.AALine[path_index] = blist.ToArray();
			}
		}

		protected M2BlockColliderContainer.BCCLine createLine(int _block_index, M2BorderPt Ps, M2BorderPt Pd)
		{
			return this.LinePool.Pool().Initialize(_block_index, Ps, Pd, false, false);
		}

		public void calcLift(BDic<uint, M2BorderCldCreator.LiftBuffer> OLiftBuf, M2BorderCldCreator.BorderPtPool BPtPool)
		{
			if (OLiftBuf.Count != 0)
			{
				using (BList<M2BorderCldCreator.LiftBuffer> blist = ListBuffer<M2BorderCldCreator.LiftBuffer>.Pop(8))
				{
					using (BList<uint> blist2 = ListBuffer<uint>.Pop(8))
					{
						(M2BlockColliderContainer.OCnctBuffer = M2BlockColliderContainer.OCnctBuffer ?? new BDic<uint, BList<M2BlockColliderContainer.BCCLine>>()).Clear();
						if (this.ALift == null)
						{
							this.ALift = new List<M2BlockColliderContainer.BCCLine>(8);
						}
						else
						{
							this.ALift.Clear();
						}
						if (this.AFootable == null)
						{
							this.AFootable = new List<M2BlockColliderContainer.BCCLine>();
						}
						this.BoundsLift.key = "";
						using (BList<uint> blist3 = ListBuffer<uint>.Pop(OLiftBuf.Count))
						{
							X.objKeys<uint, M2BorderCldCreator.LiftBuffer>(OLiftBuf, blist3);
							int num = 0;
							for (int i = 0; i < OLiftBuf.Count; i++)
							{
								uint num2 = blist3[i];
								M2BorderCldCreator.LiftBuffer liftBuffer = OLiftBuf[num2];
								if (liftBuffer.AttachedBcc == null)
								{
									M2Pt pt = liftBuffer.Pt;
									uint num3 = Map2d.b2x(num2);
									uint num4 = Map2d.b2y(num2);
									int slopeCfg = liftBuffer.getSlopeCfg((int)num3, (int)num4);
									M2BlockColliderContainer.M2PtPos m2PtPos = new M2BlockColliderContainer.M2PtPos((int)num3, (int)num4, pt);
									blist.Clear();
									blist2.Clear();
									M2BorderPt m2BorderPt = null;
									M2BorderPt m2BorderPt2 = null;
									blist.Add(liftBuffer);
									blist2.Add(num2);
									for (int j = 0; j < 2; j++)
									{
										int num5 = (int)num4;
										int num6 = (int)num3;
										int num7 = ((j == 0) ? (-1) : 1);
										int num8 = slopeCfg;
										M2BorderPt m2BorderPt3;
										if (j == 0)
										{
											m2BorderPt3 = BPtPool.Pool((float)(num6 + j), (float)num5, m2PtPos);
										}
										else
										{
											m2BorderPt3 = BPtPool.Pool((float)(num6 + j), (float)num5);
										}
										m2BorderPt3.Init((float)(num6 + j), (float)num5, m2PtPos);
										for (int k = 0; k < this.Mp.clms; k++)
										{
											int num9 = num5;
											float slopeLevel = CCON.getSlopeLevel(num8, j == 1);
											m2BorderPt3.Set((float)(num6 + j), (float)num5 + slopeLevel);
											if (CCON.isLiftSlope(num8))
											{
												if (j == 0)
												{
													if (CCON.getTiltLevel(num8) > 0f)
													{
														if (slopeLevel == 0f)
														{
															num5--;
														}
													}
													else if (slopeLevel == 1f)
													{
														num5++;
													}
												}
												else if (CCON.getTiltLevel(num8) > 0f)
												{
													if (slopeLevel == 1f)
													{
														num5++;
													}
												}
												else if (slopeLevel == 0f)
												{
													num5--;
												}
											}
											num6 += num7;
											uint num10 = Map2d.xy2b(num6, num5);
											M2BorderCldCreator.LiftBuffer liftBuffer2;
											if (!OLiftBuf.TryGetValue(num10, out liftBuffer2))
											{
												break;
											}
											int slopeCfg2 = liftBuffer2.getSlopeCfg(num6, num5, num8, num9, j == 0);
											float slopeLevel2 = CCON.getSlopeLevel(slopeCfg2, j == 0);
											if (CCON.getTiltLevel(slopeCfg2) != CCON.getTiltLevel(num8) || (float)num5 + slopeLevel2 != (float)num9 + slopeLevel)
											{
												break;
											}
											blist.Add(liftBuffer2);
											blist2.Add(num10);
											M2BlockColliderContainer.M2PtPos m2PtPos2 = new M2BlockColliderContainer.M2PtPos(num6, num5, liftBuffer2.Pt);
											m2BorderPt3.APt.Add(m2PtPos2);
											num8 = slopeCfg2;
										}
										if (j == 0)
										{
											m2BorderPt3.APt.Reverse();
											m2BorderPt = m2BorderPt3;
										}
										else
										{
											m2BorderPt2 = m2BorderPt3;
										}
									}
									M2BlockColliderContainer.BCCLine bccline = this.LinePool.Pool().Initialize(num, m2BorderPt, m2BorderPt2, true, true);
									if (bccline.valid)
									{
										this.ALift.Add(bccline);
										if (bccline._yd < 0)
										{
											this.AFootable.Add(bccline);
										}
										this.expandLiftBounds(bccline);
										num++;
										for (int l = blist.Count - 1; l >= 0; l--)
										{
											M2BorderCldCreator.LiftBuffer liftBuffer3 = (blist[l] = blist[l].SetChecker(bccline));
											uint num11 = blist2[l];
											if (OLiftBuf.ContainsKey(num11))
											{
												OLiftBuf[num11] = liftBuffer3;
											}
										}
									}
								}
							}
						}
						int count = this.ALift.Count;
						int num12 = this.AALine.Length;
						for (int m = 0; m < count; m++)
						{
							M2BlockColliderContainer.BCCLine bccline2 = this.ALift[m];
							if (bccline2.SideL == null || bccline2.SideR == null)
							{
								float x = bccline2.x;
								float num13 = bccline2.shifted_left_y + this.base_shift_y;
								float right = bccline2.right;
								float num14 = bccline2.shifted_right_y + this.base_shift_y;
								AIM foot_aim = bccline2.foot_aim;
								M2BlockColliderContainer.BCCLine[] array = null;
								bool flag = false;
								int num15 = -1;
								while (num15 < num12 && !flag)
								{
									int num16;
									if (num15 < 0)
									{
										num16 = count;
										goto IL_0456;
									}
									array = this.AALine[num15];
									if (array != null)
									{
										num16 = array.Length;
										goto IL_0456;
									}
									IL_063B:
									num15++;
									continue;
									IL_0456:
									int num17 = 0;
									while (num17 < num16 && !flag)
									{
										M2BlockColliderContainer.BCCLine bccline3 = ((num15 < 0) ? this.ALift[num17] : array[num17]);
										if (bccline3 != bccline2 && bccline3.foot_aim == foot_aim && X.isCovering(bccline3.x, bccline3.right, bccline2.x, bccline2.right, 1f) && X.isCovering(bccline3.y, bccline3.bottom, bccline2.y, bccline2.bottom, 1f))
										{
											if (bccline2.SideL == null && X.BTWM(bccline3.x, x, bccline3.right))
											{
												if (bccline3.slopeBottomY(x, 0f, 0f, false) == num13)
												{
													bccline2.SideL = bccline3;
													if (x == bccline3.right && bccline3.SideR == null)
													{
														bccline3.SideR = bccline2;
														if (num15 < 0)
														{
															bccline3.block_index = (bccline2.block_index = X.Mn(bccline3.block_index, bccline2.block_index));
														}
													}
												}
												if (bccline2.SideL != null && bccline2.SideR != null)
												{
													flag = true;
												}
											}
											if (bccline2.SideR == null && X.BTW(bccline3.x, right, bccline3.right))
											{
												if (bccline3.slopeBottomY(right, 0f, 0f, false) == num14)
												{
													bccline2.SideR = bccline3;
													if (right == bccline3.left && bccline3.SideL == null)
													{
														bccline3.SideL = bccline2;
														if (num15 < 0)
														{
															bccline3.block_index = (bccline2.block_index = X.Mn(bccline3.block_index, bccline2.block_index));
														}
													}
												}
												if (bccline2.SideL != null && bccline2.SideR != null)
												{
													flag = true;
												}
											}
										}
										num17++;
									}
									goto IL_063B;
								}
							}
						}
					}
				}
			}
			if (this.AOtherLines != null)
			{
				if (this.ALift == null)
				{
					this.ALift = new List<M2BlockColliderContainer.BCCLine>(8);
					this.BoundsLift.key = "";
				}
				int count2 = this.AOtherLines.Count;
				for (int n = 0; n < count2; n++)
				{
					M2BlockColliderContainer.BCCLine bccline4 = this.AOtherLines[n];
					if (bccline4.is_lift)
					{
						this.ALift.Add(bccline4);
						if (bccline4._yd < 0)
						{
							this.AFootable.Add(bccline4);
						}
						this.expandLiftBounds(bccline4);
					}
				}
			}
		}

		public void expandLiftBounds(M2BlockColliderContainer.BCCLine Bcc)
		{
			if (this.BoundsLift.key == "")
			{
				this.BoundsLift.Set(Bcc);
				this.BoundsLift.key = "lift";
				return;
			}
			this.BoundsLift.ExpandRc(Bcc, true);
		}

		public void finalizeBorderCreate()
		{
			Func<int, int, M2BlockColliderContainer.BCCLine, bool> func = new Func<int, int, M2BlockColliderContainer.BCCLine, bool>(this.FnWriteWallToM2Pt);
			if (this == this.Mp.BCC)
			{
				for (int i = this.AALine.Length - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
					if (array != null)
					{
						for (int j = array.Length - 1; j >= 0; j--)
						{
							array[j].EachPixel(func, false, 0f);
						}
					}
				}
			}
		}

		public void finalizeBorderCreateLiftAfter()
		{
			if (this.ALift != null && this.ALift.Count == 0)
			{
				this.ALift = null;
			}
			if (this.AFootLsn != null)
			{
				DRect Rc = null;
				uint b = 0U;
				IBCCFootListener L = null;
				Func<M2BlockColliderContainer.BCCLine, bool> func = delegate(M2BlockColliderContainer.BCCLine _C)
				{
					if ((b & (1U << (int)_C.foot_aim)) == 0U)
					{
						return false;
					}
					if (Rc.isCovering(_C, 0.25f))
					{
						_C.addBCCFootListener(L, true);
					}
					return false;
				};
				for (int i = this.AFootLsn.Count - 1; i >= 0; i--)
				{
					L = this.AFootLsn[i];
					b = L.getFootableAimBits();
					M2BlockColliderContainer.BufRc.active = true;
					Rc = L.getMapBounds(M2BlockColliderContainer.BufRc);
					if (Rc != null && Rc.active && this.Bounds.isCovering(Rc, 1f))
					{
						this.findBcc(func);
					}
				}
			}
		}

		private bool FnWriteWallToM2Pt(int px, int py, M2BlockColliderContainer.BCCLine Tgt)
		{
			M2Pt pointPuts = this.Mp.getPointPuts(px, py, true, false);
			if (pointPuts != null)
			{
				pointPuts.writeBccWall(Tgt);
			}
			return true;
		}

		public List<IBCCEventListener> PopListenerArray()
		{
			if (this.PoolListEvl == null)
			{
				this.PoolListEvl = new ClsPool<List<IBCCEventListener>>(() => new List<IBCCEventListener>(2), 0);
			}
			List<IBCCEventListener> list = this.PoolListEvl.Pool();
			list.Clear();
			return list;
		}

		public List<IBCCEventListener> ReleaseListenerArray(List<IBCCEventListener> A)
		{
			if (A != null)
			{
				A.Clear();
				if (this.PoolListEvl != null)
				{
					this.PoolListEvl.Release(A);
				}
			}
			return null;
		}

		public List<IBCCFootListener> PopFLArray()
		{
			if (this.PoolListFl == null)
			{
				this.PoolListFl = new ClsPool<List<IBCCFootListener>>(() => new List<IBCCFootListener>(2), 0);
			}
			List<IBCCFootListener> list = this.PoolListFl.Pool();
			list.Clear();
			return list;
		}

		public List<IBCCFootListener> ReleaseFLArray(List<IBCCFootListener> A)
		{
			if (A != null)
			{
				A.Clear();
				if (this.PoolListFl != null)
				{
					this.PoolListFl.Release(A);
				}
			}
			return null;
		}

		public List<M2MapDamageContainer.M2MapDamageItem> PopMDArray()
		{
			if (this.PoolListMd == null)
			{
				this.PoolListMd = new ClsPool<List<M2MapDamageContainer.M2MapDamageItem>>(() => new List<M2MapDamageContainer.M2MapDamageItem>(2), 0);
			}
			List<M2MapDamageContainer.M2MapDamageItem> list = this.PoolListMd.Pool();
			list.Clear();
			return list;
		}

		public List<M2MapDamageContainer.M2MapDamageItem> ReleaseMDArray(List<M2MapDamageContainer.M2MapDamageItem> A)
		{
			if (A != null)
			{
				A.Clear();
				if (this.PoolListMd != null)
				{
					this.PoolListMd.Release(A);
				}
			}
			return null;
		}

		public void getBaseShift(out float shiftx, out float shifty)
		{
			shiftx = this.base_shift_x;
			shifty = this.base_shift_y;
		}

		public float base_shift_x
		{
			get
			{
				if (!(this.BelongTo != null))
				{
					return 0f;
				}
				return -this.BelongTo.x - this.b_shiftx;
			}
		}

		public float base_shift_y
		{
			get
			{
				if (!(this.BelongTo != null))
				{
					return 0f;
				}
				return -this.BelongTo.y - this.b_shifty;
			}
		}

		public void cacheNearFoot(M2FootManager FootD, List<M2BlockColliderContainer.BCCLine> Acc, float l, float t, float r, float b, float margin = 1f)
		{
			if (this.AALine == null)
			{
				return;
			}
			int count = Acc.Count;
			this.cacheNearFoot(Acc, l, t, r, b, margin, true, 15U, false);
			for (int i = Acc.Count - 1; i >= count; i--)
			{
				Acc[i].ListenerTouched(FootD);
			}
		}

		public void cacheNearFoot(List<M2BlockColliderContainer.BCCLine> Acc, float l, float t, float r, float b, float margin, bool consider_lift, uint aim_bit = 15U, bool containing_whole = true)
		{
			if (this.AALine == null)
			{
				return;
			}
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			l += num;
			t += num2;
			r += num;
			b += num2;
			for (int i = this.AALine.Length - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					for (int j = array.Length - 1; j >= 0; j--)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if (bccline.isUseableDir(aim_bit) && (containing_whole ? bccline.isCoveringXy(l, t, r, b, margin, -1000f) : bccline.isUseablePos(l, t, r, b, margin)))
						{
							Acc.Add(bccline);
						}
					}
				}
			}
			if (consider_lift && this.ALift != null && this.BoundsLift.isCoveringXy(l, t, r, b, margin + 0.5f, -1000f))
			{
				for (int k = this.ALift.Count - 1; k >= 0; k--)
				{
					M2BlockColliderContainer.BCCLine bccline2 = this.ALift[k];
					if (bccline2.isUseableDir(aim_bit) && (containing_whole ? bccline2.isCoveringXy(l, t, r, b, margin, -1000f) : bccline2.isUseablePos(l, t, r, b, margin)))
					{
						Acc.Add(bccline2);
					}
				}
			}
		}

		public bool isBoundsCovering(float l, float t, float r, float b, float margin = 1f)
		{
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			return this.isBoundsCoveringS(num, num2, l, t, r, b, margin, false);
		}

		private bool isBoundsCoveringS(float shiftx, float shifty, float l, float t, float r, float b, float margin = 1f, bool lift = false)
		{
			if (!this.active)
			{
				return false;
			}
			l += shiftx - margin;
			t += shifty - margin;
			r += shiftx + margin;
			b += shifty + margin;
			return (lift ? this.BoundsLift : this.Bounds).isCoveringXy(l, t, r, b, 0f, -1000f);
		}

		public M2BlockColliderContainer.BCCLine checkFootCarryable(M2FootManager FootD)
		{
			if (this.AALine == null)
			{
				return null;
			}
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			float num3 = FootD.Mv.vx;
			float num4 = FootD.Mv.vy;
			if (this.BelongTo != null)
			{
				num3 -= this.BelongTo.vx;
				num4 -= this.BelongTo.vy;
			}
			if (this.isBoundsCoveringS(num, num2, FootD.Mv.mleft, FootD.Mv.mtop, FootD.Mv.mright, FootD.Mv.mbottom, 1f, false))
			{
				for (int i = this.AALine.Length - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
					if (array != null)
					{
						M2BlockColliderContainer.BCCLine bccline = this.checkFootCarryable(FootD, array, num, num2, num3, num4, 0f);
						if (bccline != null)
						{
							return bccline;
						}
					}
				}
			}
			if (this.ALift != null && this.isBoundsCoveringS(num, num2, FootD.Mv.mleft, FootD.Mv.mtop, FootD.Mv.mright, FootD.Mv.mbottom, 1f, true))
			{
				M2BlockColliderContainer.BCCLine bccline2 = this.checkFootCarryable(FootD, this.ALift, num, num2, num3, num4, 0f);
				if (bccline2 != null)
				{
					return bccline2;
				}
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine checkFootCarryable(M2FootManager FootD, M2BlockColliderContainer.BCCLine[] ALine, float shiftx, float shifty, float vx, float vy, float pre_fall_y = 0f)
		{
			for (int i = ALine.Length - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine bccline = ALine[i];
				if (bccline.isUseableVelocity(FootD.Phy) && bccline.isCarryableShifted(FootD, shiftx, shifty, 0f, false, pre_fall_y) != null)
				{
					return bccline;
				}
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine checkFootCarryable(M2FootManager FootD, List<M2BlockColliderContainer.BCCLine> ALine, float pre_fall_y)
		{
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			float pre_force_velocity_x = FootD.Phy.pre_force_velocity_x;
			float force_velocity_y_with_gravity = FootD.Phy.force_velocity_y_with_gravity;
			return this.checkFootCarryable(FootD, ALine, num, num2, pre_force_velocity_x, force_velocity_y_with_gravity, pre_fall_y);
		}

		public M2BlockColliderContainer.BCCLine checkFootCarryable(M2FootManager FootD, List<M2BlockColliderContainer.BCCLine> ALine, float shiftx, float shifty, float vx, float vy, float pre_fall_y = 0f)
		{
			for (int i = ALine.Count - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine bccline = ALine[i];
				if (bccline.isUseableVelocity(FootD.Phy, vx, vy) && bccline.isCarryableShifted(FootD, shiftx, shifty, 0f, false, pre_fall_y) != null)
				{
					return bccline;
				}
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine checkBCCEvent(M2Attackable MvA, ref bool check_dangerous)
		{
			if (this.AALine == null)
			{
				return null;
			}
			if (this.ALsn != null || (check_dangerous && this.has_dangerous_chip))
			{
				float num;
				float num2;
				this.getBaseShift(out num, out num2);
				for (int i = this.AALine.Length - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
					if (array != null)
					{
						for (int j = array.Length - 1; j >= 0; j--)
						{
							M2BlockColliderContainer.BCCLine bccline = array[j];
							MvA.checkBCCEvent(bccline, ref check_dangerous, num, num2);
						}
					}
				}
			}
			return null;
		}

		public void addBCCEventListener(IBCCEventListener Lsn)
		{
			if (this.ALsn == null)
			{
				this.ALsn = new List<IBCCEventListener>(1);
			}
			this.ALsn.Add(Lsn);
		}

		public void remBCCEventListener(IBCCEventListener Lsn)
		{
			if (this.ALsn == null)
			{
				return;
			}
			this.ALsn.Remove(Lsn);
		}

		public void addBCCFootListener(IBCCFootListener Lsn)
		{
			if (this.AFootLsn == null)
			{
				this.AFootLsn = new List<IBCCFootListener>(1);
			}
			this.AFootLsn.Add(Lsn);
		}

		public void remBCCFootListener(IBCCFootListener Lsn)
		{
			if (this.AFootLsn == null)
			{
				return;
			}
			this.AFootLsn.Remove(Lsn);
		}

		public float canGoToSide(out M2BlockColliderContainer.BCCLine L, M2FootManager FootD, float ft_shiftx, float ft_shifty, AIM fa, float marginf, float margins, bool ignore_naname, bool refix_position = false)
		{
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			num += ft_shiftx;
			num2 += ft_shifty;
			if (!this.active || this.AALine == null)
			{
				L = null;
				return 0f;
			}
			if (fa == AIM.L || fa == AIM.R)
			{
				float num3 = FootD.Mv.x + (float)CAim._XD(fa, 1) * (FootD.sizex + marginf) + num;
				if (!this.Bounds.isCoveringXy(num3, FootD.mtop + num2, num3, FootD.mbottom + num2, 0f, -1000f))
				{
					L = null;
					return 0f;
				}
			}
			else
			{
				float num4 = FootD.Mv.y - (float)CAim._YD(fa, 1) * (FootD.sizey + marginf) + num2;
				if (!this.Bounds.isCoveringXy(FootD.mleft + num, num4, FootD.mright + num, num4, 0f, -1000f))
				{
					L = null;
					return 0f;
				}
			}
			for (int i = this.AALine.Length - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					for (int j = array.Length - 1; j >= 0; j--)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if ((!bccline.is_naname || !ignore_naname) && !bccline.is_lift)
						{
							float num5 = bccline.canGoToSideShifted(FootD, num, num2, fa, marginf, margins, refix_position);
							if (num5 != 0f)
							{
								L = bccline;
								return num5;
							}
						}
					}
				}
			}
			L = null;
			return 0f;
		}

		public Vector3 crosspoint(float x0, float y0, float x1, float y1, float radiusx, float radiusy, List<M2BlockColliderContainer.BCCHitInfo> AHitInfo = null, bool directional_check = false, Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FnReturnable = null)
		{
			if (!this.active || this.AALine == null)
			{
				return Vector3.zero;
			}
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			float num3 = x0 + num;
			float num4 = y0 + num2;
			float num5 = x1 + num;
			float num6 = y1 + num2;
			if (!this.Bounds.isCoveringXyR(num3 - radiusx, num4 - radiusy, num5 + radiusx, num6 + radiusy, 0f))
			{
				return Vector3.zero;
			}
			float num7 = 0f;
			float num8 = -1f;
			float num9 = 0f;
			float num10 = 0f;
			Vector3 vector = Vector3.zero;
			for (int i = this.AALine.Length - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					for (int j = array.Length - 1; j >= 0; j--)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if (bccline.TwoPointCrossingShifted(num3, num4, num5, num6, radiusx, radiusy, directional_check))
						{
							if (num8 < 0f)
							{
								X.calcLineGraphVariable(num3, num4, num5, num6, out num7, out num8, out num9, out num10);
							}
							Vector3 vector2 = bccline.crosspointLshifted(num7, num8, num9, num10, radiusx, radiusy, 0f, 0f);
							if (vector2.z >= 2f && (FnReturnable == null || FnReturnable(bccline, vector2)))
							{
								if (AHitInfo == null)
								{
									return vector2;
								}
								vector = vector2;
								AHitInfo.Add(new M2BlockColliderContainer.BCCHitInfo(bccline, vector2.x - num, vector2.y - num2));
							}
						}
					}
				}
			}
			return vector;
		}

		public float isFallable(float cx, float cy, float marginx, float marginy, out M2BlockColliderContainer.BCCLine Out, bool check_main = true, bool check_lift = true, float near_y = -1f, M2BlockColliderContainer.BCCLine BccCalcStartFrom = null)
		{
			if (!this.active || this.AALine == null)
			{
				Out = null;
				return -1f;
			}
			if (BccCalcStartFrom != null)
			{
				if (BccCalcStartFrom.foot_aim == AIM.B && !BccCalcStartFrom.is_lift && X.BTW(BccCalcStartFrom.shifted_x - marginx, cx, BccCalcStartFrom.shifted_right + marginx))
				{
					float num = BccCalcStartFrom.slopeBottomY(X.MMX(BccCalcStartFrom.shifted_x, cx, BccCalcStartFrom.shifted_right));
					cy = X.Mn(cy, num - 0.125f);
				}
				using (BList<M2BlockColliderContainer.BCCLine> blist = ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(0))
				{
					for (int i = 0; i < 2; i++)
					{
						blist.Clear();
						M2BlockColliderContainer.BCCLine bccline = BccCalcStartFrom;
						blist.Add(BccCalcStartFrom);
						for (;;)
						{
							M2BlockColliderContainer.BCCLine bccline2 = bccline;
							bccline = ((i == 0) ? bccline.LinkS : bccline.LinkD);
							if (bccline == null || blist.IndexOf(bccline) >= 0)
							{
								break;
							}
							if (bccline.foot_aim == AIM.B && !bccline.is_lift && X.BTW(bccline.shifted_x - marginx, cx, bccline.shifted_right + marginx))
							{
								float num2 = bccline.slopeBottomY(X.MMX(bccline.shifted_x, cx, bccline.shifted_right));
								cy = X.Mn(cy, num2 - 0.125f);
							}
							if (bccline2.is_lift != bccline.is_lift || bccline2.block_index != bccline.block_index)
							{
								blist.Add(bccline);
							}
						}
					}
				}
			}
			float num3;
			float num4;
			this.getBaseShift(out num3, out num4);
			cx += num3;
			cy += num4;
			if (check_main)
			{
				check_main = this.Bounds.isin(cx, cy, marginx, marginy);
			}
			if (check_lift)
			{
				check_lift = this.BoundsLift.isin(cx, cy, marginx, marginy);
			}
			float num5 = -1f;
			Out = null;
			if (check_main || check_lift)
			{
				float num6 = cy;
				float num7 = -1f;
				for (int j = this.AFootable.Count - 1; j >= 0; j--)
				{
					M2BlockColliderContainer.BCCLine bccline3 = this.AFootable[j];
					if (bccline3.is_lift ? check_lift : check_main)
					{
						float num8 = bccline3.isFallableShifted(0f, 0f, cx, cy, marginx, marginy);
						if (num8 != -1000f)
						{
							num8 += -num4 + marginy;
							if (num8 >= num6 && ((near_y >= 0f) ? (num5 < 0f || X.Abs(near_y - num8) < num7) : (num5 < 0f || num8 < num5)))
							{
								num5 = num8;
								Out = bccline3;
								if (near_y >= 0f)
								{
									num7 = X.Abs(near_y - num8);
								}
							}
						}
					}
				}
			}
			return num5;
		}

		public M2BlockColliderContainer.BCCLine getNear(float cx, float cy, float marginx, float marginy, int aim, List<M2BlockColliderContainer.BCCLine> ARet, bool find_lift = false, bool strict_aim = false, float inside_len = 0f)
		{
			if (!this.active || this.AALine == null)
			{
				return null;
			}
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			cx += num;
			cy += num2;
			if (this.Bounds.isin(cx, cy, marginx, marginy))
			{
				for (int i = this.AALine.Length - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
					if (array != null)
					{
						for (int j = array.Length - 1; j >= 0; j--)
						{
							M2BlockColliderContainer.BCCLine bccline = array[j];
							if ((inside_len > 0f) ? bccline.isNearInsideShifted(cx, cy, marginx, marginy, inside_len, aim, strict_aim) : bccline.isNearShifted(cx, cy, marginx, marginy, aim, strict_aim))
							{
								if (ARet == null)
								{
									return bccline;
								}
								ARet.Add(bccline);
							}
						}
					}
				}
			}
			if (find_lift && this.ALift != null && this.BoundsLift.isin(cx, cy, marginx, marginy))
			{
				for (int k = this.ALift.Count - 1; k >= 0; k--)
				{
					M2BlockColliderContainer.BCCLine bccline2 = this.ALift[k];
					if ((inside_len > 0f) ? bccline2.isNearInsideShifted(cx, cy, marginx, marginy, inside_len, aim, strict_aim) : bccline2.isNearShifted(cx, cy, marginx, marginy, aim, strict_aim))
					{
						if (ARet == null)
						{
							return bccline2;
						}
						ARet.Add(bccline2);
					}
				}
			}
			return null;
		}

		public static M2BlockColliderContainer.BCCLine getNearest(float x, float y, List<M2BlockColliderContainer.BCCLine> ABcc)
		{
			if (ABcc == null || ABcc.Count == 0)
			{
				return null;
			}
			if (ABcc.Count == 1)
			{
				return ABcc[0];
			}
			M2BlockColliderContainer.BCCLine bccline = null;
			float num = 0f;
			float num2;
			float num3;
			ABcc[0].BCC.getBaseShift(out num2, out num3);
			for (int i = ABcc.Count - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCLine bccline2 = ABcc[i];
				float num4 = bccline2.point_line_len2_shifted(x, y, num2, num3);
				if (bccline == null || num > num4)
				{
					bccline = bccline2;
					num = num4;
				}
			}
			return bccline;
		}

		public Rect getBoundsShifted()
		{
			float num;
			float num2;
			this.getBaseShift(out num, out num2);
			Rect rect = new Rect(this.Bounds.x, this.Bounds.y, this.Bounds.width, this.Bounds.height);
			rect.x -= num;
			rect.y -= num2;
			return rect;
		}

		public M2BlockColliderContainer.BCCLine findBcc(Func<M2BlockColliderContainer.BCCLine, bool> Fn)
		{
			if (this.AALine == null)
			{
				return null;
			}
			int num = this.AALine.Length;
			for (int i = 0; i < num; i++)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if (Fn(bccline))
						{
							return bccline;
						}
					}
				}
			}
			if (this.ALift != null)
			{
				num = this.ALift.Count;
				for (int k = 0; k < num; k++)
				{
					M2BlockColliderContainer.BCCLine bccline2 = this.ALift[k];
					if (Fn(bccline2))
					{
						return bccline2;
					}
				}
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine getSameLine(M2BlockColliderContainer.BCCLine B, bool iscontaining = false, bool b_containing = false)
		{
			if (this.AALine == null)
			{
				return null;
			}
			bool flag = iscontaining && b_containing;
			int num = this.AALine.Length;
			for (int i = 0; i < num; i++)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if (b_containing ? B.isSameLine(bccline, true, flag) : bccline.isSameLine(B, iscontaining, false))
						{
							return bccline;
						}
					}
				}
			}
			if (this.ALift != null)
			{
				num = this.ALift.Count;
				for (int k = 0; k < num; k++)
				{
					M2BlockColliderContainer.BCCLine bccline2 = this.ALift[k];
					if (b_containing ? B.isSameLine(bccline2, true, flag) : bccline2.isSameLine(B, iscontaining, false))
					{
						return bccline2;
					}
				}
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine getSameLine(M2BlockColliderContainer.BCCInfo B, bool iscontaining = false, bool iscovering = false)
		{
			if (this.AALine == null)
			{
				return null;
			}
			int num = this.AALine.Length;
			for (int i = 0; i < num; i++)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						if (bccline.isSameLine(B, iscontaining, false))
						{
							return bccline;
						}
					}
				}
			}
			if (this.ALift != null)
			{
				num = this.ALift.Count;
				for (int k = 0; k < num; k++)
				{
					M2BlockColliderContainer.BCCLine bccline2 = this.ALift[k];
					if (bccline2.isSameLine(B, iscontaining, false))
					{
						return bccline2;
					}
				}
			}
			return null;
		}

		public int getConnectedBcc(M2BlockColliderContainer.BCCLine From, List<M2BlockColliderContainer.BCCHitInfo> AHit, bool calc_normal = true, bool calc_lift = true, bool add_original_side = true, bool check_left = true, bool check_right = true)
		{
			return this.getConnectedBcc(From, 0f, -1000f, AHit, calc_normal, calc_lift, add_original_side, check_left, check_right);
		}

		public int getConnectedBcc(M2BlockColliderContainer.BCCLine From, float center_x, float margin_x, List<M2BlockColliderContainer.BCCHitInfo> AHit, bool calc_normal = true, bool calc_lift = true, bool add_original_side = true, bool check_left = true, bool check_right = true)
		{
			int count = AHit.Count;
			M2BlockColliderContainer.BCCLine sideL = From.SideL;
			M2BlockColliderContainer.BCCLine sideR = From.SideR;
			float num;
			float num2;
			this.Mp.BCC.getBaseShift(out num, out num2);
			bool flag = margin_x != -1000f;
			center_x += num;
			AIM foot_aim = From.foot_aim;
			if (add_original_side)
			{
				if (From.SideL != null)
				{
					AHit.Add(new M2BlockColliderContainer.BCCHitInfo(sideL, From.shifted_x, From.shifted_left_y));
				}
				if (From.SideR != null)
				{
					AHit.Add(new M2BlockColliderContainer.BCCHitInfo(sideR, From.shifted_right, From.shifted_right_y));
				}
			}
			if (calc_lift && this.ALift != null)
			{
				int count2 = this.ALift.Count;
				for (int i = 0; i < count2; i++)
				{
					M2BlockColliderContainer.BCCLine bccline = this.ALift[i];
					if (bccline.foot_aim == foot_aim)
					{
						M2BlockColliderContainer.BCCHitInfo bcchitInfo;
						if (bccline.SideR == From && check_right)
						{
							if (flag && X.Abs(center_x - bccline.right) > margin_x)
							{
								goto IL_0150;
							}
							bcchitInfo = new M2BlockColliderContainer.BCCHitInfo(bccline, bccline.shifted_right, bccline.shifted_right_y);
						}
						else
						{
							if (bccline.SideL != From || !check_left || (flag && X.Abs(center_x - bccline.x) > margin_x))
							{
								goto IL_0150;
							}
							bcchitInfo = new M2BlockColliderContainer.BCCHitInfo(bccline, bccline.shifted_x, bccline.shifted_left_y);
						}
						X.pushIdentical<M2BlockColliderContainer.BCCHitInfo>(AHit, bcchitInfo);
					}
					IL_0150:;
				}
			}
			if (calc_normal)
			{
				for (int j = this.AALine.Length - 1; j >= 0; j--)
				{
					M2BlockColliderContainer.BCCLine[] array = this.AALine[j];
					if (array != null)
					{
						for (int k = array.Length - 1; k >= 0; k--)
						{
							M2BlockColliderContainer.BCCLine bccline2 = array[k];
							if (!bccline2.is_lift && bccline2.foot_aim == foot_aim)
							{
								M2BlockColliderContainer.BCCHitInfo bcchitInfo;
								if (bccline2.SideR == From && check_right)
								{
									if (flag && X.Abs(center_x - bccline2.right) > margin_x)
									{
										goto IL_0239;
									}
									bcchitInfo = new M2BlockColliderContainer.BCCHitInfo(bccline2, bccline2.shifted_right, bccline2.shifted_right_y);
								}
								else
								{
									if (bccline2.SideL != From || !check_left || (flag && X.Abs(center_x - bccline2.x) > margin_x))
									{
										goto IL_0239;
									}
									bcchitInfo = new M2BlockColliderContainer.BCCHitInfo(bccline2, bccline2.shifted_x, bccline2.shifted_left_y);
								}
								X.pushIdentical<M2BlockColliderContainer.BCCHitInfo>(AHit, bcchitInfo);
							}
							IL_0239:;
						}
					}
				}
			}
			return AHit.Count - count;
		}

		public static Vector3 extractFromStuck(Map2d Mp, float fx, float fy, ref M2BlockColliderContainer.BCCLine PreTarg_, int calc_aim_bits = 15, float extract_marginx = 0f, float extract_marginy = -1f, bool no_check_lcfg = false)
		{
			if (extract_marginy < 0f)
			{
				extract_marginy = extract_marginx;
			}
			int num = (int)fx;
			int num2 = (int)fy;
			M2Pt pointPuts = Mp.getPointPuts(num, num2, false, true);
			M2BlockColliderContainer.BCCLine bccline = PreTarg_;
			PreTarg_ = null;
			if (pointPuts == null || (!no_check_lcfg && !pointPuts.bcc_line_cfg) || Mp.BCC == null)
			{
				return new Vector3(0f, 0f, -1f);
			}
			M2BlockColliderContainer.BCCLine bccline2 = null;
			float num3;
			float num4;
			Mp.BCC.getBaseShift(out num3, out num4);
			float num5 = -1f;
			for (int i = -1; i < 4; i++)
			{
				if (i < 0 || (calc_aim_bits & (1 << i)) != 0)
				{
					M2BlockColliderContainer.BCCLine bccline3 = ((i == -1) ? bccline : pointPuts.getSideBcc(Mp, num, num2, (AIM)i));
					if (bccline3 != null && bccline3 != bccline2 && bccline3.isinWall(fx, fy, num3, num4, 0f, 0f))
					{
						AIM aim = ((i == -1) ? bccline3.foot_aim : ((AIM)i));
						M2BlockColliderContainer.BCCLine sideBcc = pointPuts.getSideBcc(Mp, num, num2, CAim.get_opposite(aim));
						if (sideBcc != null)
						{
							switch (i)
							{
							case 0:
								if (X.MMX(sideBcc.shifted_x, fx, sideBcc.shifted_right) >= fx)
								{
									goto IL_018E;
								}
								break;
							case 1:
								if (X.MMX(sideBcc.shifted_y, fy, sideBcc.shifted_bottom) >= fy)
								{
									goto IL_018E;
								}
								break;
							case 2:
								if (X.MMX(sideBcc.shifted_x, fx, sideBcc.shifted_right) <= fx)
								{
									goto IL_018E;
								}
								break;
							case 3:
								if (X.MMX(sideBcc.shifted_y, fy, sideBcc.shifted_bottom) <= fy)
								{
									goto IL_018E;
								}
								break;
							}
						}
						float num6 = bccline3.point_line_len2_shifted(fx, fy, num3, num4);
						if (bccline2 == null || num5 > num6)
						{
							bccline2 = bccline3;
							num5 = num6;
						}
					}
				}
				IL_018E:;
			}
			if (bccline2 != null)
			{
				float num7 = CAim.get_opposite(bccline2.aim);
				float shifted_x = bccline2.shifted_x;
				float shifted_y = bccline2.shifted_y;
				PreTarg_ = bccline2;
				switch (bccline2.aim)
				{
				case AIM.L:
				case AIM.R:
				{
					float num8 = shifted_x - (float)bccline2._xd * extract_marginx;
					return new Vector3((bccline2.aim == AIM.R == num8 < fx) ? num8 : fx, X.MMX(shifted_y + extract_marginy, fy, shifted_y + bccline2.height - extract_marginy), num7);
				}
				case AIM.T:
				case AIM.B:
				{
					float num9 = shifted_y + (float)bccline2._yd * extract_marginy;
					return new Vector3(X.MMX(shifted_x + extract_marginx, fx, shifted_x + bccline2.width - extract_marginx), (bccline2.aim == AIM.B == num9 < fy) ? num9 : fy, num7);
				}
				default:
				{
					fx = X.MMX(shifted_x + extract_marginx, fx, shifted_x + bccline2.width - extract_marginx);
					Vector3 vector = bccline2.crosspointLshifted(1f, 0f, -fx, 1f, 1f, 1f, num3, num4);
					if (vector.z >= 2f)
					{
						float num9 = vector.y + (float)bccline2._yd * extract_marginy;
						if (bccline2.foot_aim == AIM.B != num9 < fy)
						{
							return new Vector3(0f, 0f, -1f);
						}
						return new Vector3(vector.x, num9, num7);
					}
					break;
				}
				}
			}
			return new Vector3(0f, 0f, -1f);
		}

		public bool is_prepared
		{
			get
			{
				return this.is_prepared_;
			}
		}

		public void ItrInit(out M2BlockColliderContainer.BCCIterator Itr, bool _check_lift = true)
		{
			Itr = new M2BlockColliderContainer.BCCIterator(this, _check_lift);
		}

		public MeshDrawer LineDashed(EffectItem Ef, MeshDrawer Md, float x, float y, float pos_level, float thick, float fillrate = 0.5f, float line_len_rate = 0.5f)
		{
			if (this.AALine == null)
			{
				return Md;
			}
			int num = this.AALine.Length;
			float num2;
			float num3;
			this.getBaseShift(out num2, out num3);
			Md.base_x = (Md.base_y = 0f);
			thick *= 0.015625f;
			x *= 0.015625f;
			y *= 0.015625f;
			for (int i = 0; i < num; i++)
			{
				M2BlockColliderContainer.BCCLine[] array = this.AALine[i];
				if (array != null)
				{
					int num4 = array.Length;
					for (int j = 0; j < num4; j++)
					{
						M2BlockColliderContainer.BCCLine bccline = array[j];
						bool flag;
						Vector2 vector = Ef.EF.calcMeshXY(bccline.sx - num2, bccline.sy - num3, Md, out flag);
						Vector2 vector2 = Ef.EF.calcMeshXY(bccline.dx - num2, bccline.dy - num3, Md, out flag);
						Md.LineDashed(vector.x + x, vector.y + y, vector2.x + x, vector2.y + y, pos_level, X.IntR(X.LENGTHXYS(0f, 0f, bccline.w, bccline.h) / line_len_rate), thick, true, fillrate);
					}
				}
			}
			return Md;
		}

		public bool active
		{
			get
			{
				return this.Bounds.active;
			}
			set
			{
				this.Bounds.active = value;
				this.BoundsLift.active = value;
			}
		}

		public M2BlockColliderContainer.BCCLine[][] getLineVector()
		{
			return this.AALine;
		}

		public Map2d Mp;

		public static DRect BufRc;

		public readonly M2Mover BelongTo;

		public DRect Bounds;

		public DRect BoundsLift;

		private ClsPoolD<M2BlockColliderContainer.BCCLine> LinePool;

		private ClsPool<List<IBCCEventListener>> PoolListEvl;

		private ClsPool<List<IBCCFootListener>> PoolListFl;

		private ClsPool<List<M2MapDamageContainer.M2MapDamageItem>> PoolListMd;

		private M2BlockColliderContainer.BCCLine[][] AALine;

		private List<M2BlockColliderContainer.BCCLine> ALift;

		private List<M2BlockColliderContainer.BCCLine> AFootable;

		private bool is_prepared_;

		private List<M2BlockColliderContainer.BCCLine> AOtherLines;

		private List<IBCCEventListener> ALsn;

		private List<IBCCFootListener> AFootLsn;

		public bool has_dangerous_chip;

		public float b_shiftx;

		public float b_shifty;

		public int gen_id;

		private static BDic<uint, BList<M2BlockColliderContainer.BCCLine>> OCnctBuffer;

		public struct BCCInfo
		{
			public BCCInfo(M2BlockColliderContainer.BCCLine Bcc)
			{
				this.sx = Bcc.shifted_sx;
				this.sy = Bcc.shifted_sy;
				this.dx = Bcc.shifted_dx;
				this.dy = Bcc.shifted_dy;
				this.is_lift = Bcc.is_lift;
				this.aim = Bcc.aim;
				this.line_a = Bcc.line_a;
			}

			public float x
			{
				get
				{
					return X.Mn(this.sx, this.dx);
				}
			}

			public float right
			{
				get
				{
					return X.Mx(this.sx, this.dx);
				}
			}

			public float y
			{
				get
				{
					return X.Mn(this.sy, this.dy);
				}
			}

			public float bottom
			{
				get
				{
					return X.Mx(this.sy, this.dy);
				}
			}

			public float slopeBottomY(float fx, float shiftx, float shifty)
			{
				if (this.aim == AIM.B || this.aim == AIM.T)
				{
					return this.y - shifty;
				}
				return X.MMX(this.y - shifty, this.line_a * fx + (this.sy - shifty - (this.sx - shiftx) * this.line_a), this.bottom - shifty);
			}

			public float sx;

			public float sy;

			public float dx;

			public float dy;

			public float line_a;

			public AIM aim;

			public bool is_lift;
		}

		public struct BCCIterator
		{
			public BCCIterator(M2BlockColliderContainer _BCC, bool _check_lift = true)
			{
				this.BCC = _BCC;
				this.ari = 0;
				this.arj = 0;
				this.Cur = null;
				this.check_lift = _check_lift;
			}

			public bool Next()
			{
				M2BlockColliderContainer.BCCLine bccline;
				return this.Next(out bccline);
			}

			public bool Next(out M2BlockColliderContainer.BCCLine _Cur)
			{
				_Cur = null;
				M2BlockColliderContainer.BCCLine[][] lineVector = this.BCC.getLineVector();
				while (this.ari >= -1)
				{
					if (this.ari == -1)
					{
						if (this.BCC.ALift == null || this.BCC.ALift.Count <= this.arj)
						{
							this.ari = -2;
							return false;
						}
						List<M2BlockColliderContainer.BCCLine> alift = this.BCC.ALift;
						int num = this.arj;
						this.arj = num + 1;
						M2BlockColliderContainer.BCCLine bccline;
						_Cur = (bccline = alift[num]);
						this.Cur = bccline;
						if (this.Cur != null)
						{
							return true;
						}
					}
					else if (lineVector == null || this.ari >= lineVector.Length)
					{
						this.ari = (this.check_lift ? (-1) : (-2));
					}
					else
					{
						M2BlockColliderContainer.BCCLine[] array = lineVector[this.ari];
						if (array == null || array.Length <= this.arj)
						{
							this.arj = 0;
							this.ari++;
						}
						else
						{
							M2BlockColliderContainer.BCCLine[] array2 = array;
							int num = this.arj;
							this.arj = num + 1;
							M2BlockColliderContainer.BCCLine bccline;
							_Cur = (bccline = array2[num]);
							this.Cur = bccline;
							if (this.Cur != null)
							{
								return true;
							}
						}
					}
				}
				return false;
			}

			public bool check_lift;

			private int ari;

			private int arj;

			private M2BlockColliderContainer BCC;

			public M2BlockColliderContainer.BCCLine Cur;
		}

		public struct BCCHitInfo
		{
			public BCCHitInfo(M2BlockColliderContainer.BCCLine _Hit, float _x, float _y)
			{
				this.Hit = _Hit;
				this.x = _x;
				this.y = _y;
			}

			public bool valid
			{
				get
				{
					return this.Hit != null;
				}
			}

			public M2BlockColliderContainer.BCCPos getHitChip()
			{
				return M2BlockColliderContainer.BCCHitInfo.getHitChipS(this.Hit, this.x, this.y);
			}

			public static M2BlockColliderContainer.BCCPos getHitChipS(M2BlockColliderContainer.BCCLine Hit, float x, float y)
			{
				float num = x - (float)((Hit.aim == AIM.L) ? 1 : 0);
				float num2 = y - (float)((Hit.aim == AIM.T) ? 1 : 0);
				return Hit.getFootStampChip(0, num, num2, 0f, 0f);
			}

			public readonly M2BlockColliderContainer.BCCLine Hit;

			public readonly float x;

			public readonly float y;
		}

		public struct M2PtPos
		{
			public M2PtPos(int _x, int _y, M2Pt _Src)
			{
				this.x_ = _x;
				this.y_ = _y;
				this.SrcPt = _Src;
			}

			public int x
			{
				get
				{
					return this.x_;
				}
			}

			public int y
			{
				get
				{
					return this.y_;
				}
			}

			public int count
			{
				get
				{
					return this.SrcPt.count;
				}
			}

			public int cfg
			{
				get
				{
					return this.SrcPt.cfg;
				}
			}

			public M2Chip GetC(int i)
			{
				return this.SrcPt.GetC(i);
			}

			private int x_;

			private int y_;

			private M2Pt SrcPt;
		}

		public struct BCCPos
		{
			public BCCPos(M2Puts _Cp, int _x, int _y, int _cfg = -1)
			{
				this.x = _x;
				this.y = _y;
				this.Cp = _Cp;
				this.cfg = 0;
				if (_cfg >= 0)
				{
					this.cfg = _cfg;
				}
				this.foot_type = null;
			}

			public bool valid
			{
				get
				{
					return this.Cp != null;
				}
			}

			public int x;

			public int y;

			public M2Puts Cp;

			public int cfg;

			public string foot_type;
		}

		public sealed class BCCLine : DRect, IFootable, IDisposable
		{
			public BCCLine(M2BlockColliderContainer _BCC)
				: base("")
			{
				this.BCC = _BCC;
			}

			public BCCLine(M2BlockColliderContainer _BCC, int _block_index, M2BorderPt _Ps, M2BorderPt _Pd, bool check_pd_chips = false, bool _is_lift = false)
				: this(_BCC)
			{
				this.Initialize(_block_index, _Ps, _Pd, check_pd_chips, _is_lift);
			}

			public void Dispose()
			{
				this.ALsnRegistered = this.BCC.ReleaseListenerArray(this.ALsnRegistered);
				this.AFootLsnRegistered = this.BCC.ReleaseFLArray(this.AFootLsnRegistered);
				if (this.AMapDmg != null)
				{
					this.BCC.Mp.M2D.MDMGCon.Release(this.AMapDmg);
				}
				this.valid = false;
				this._tostring = null;
				this.AMapDmg = this.BCC.ReleaseMDArray(this.AMapDmg);
			}

			public bool initPosition(float _sx, float _sy, float _dx, float _dy, out bool is_naname)
			{
				this.sx = _sx;
				this.sy = _sy;
				this.dx = _dx;
				this.dy = _dy;
				is_naname = false;
				this.line_a = (this.line_div = 0f);
				if (this.ACp != null)
				{
					this.ACp.Clear();
				}
				if (_sx == _dx)
				{
					this.aim = ((_sy < _dy) ? AIM.L : AIM.R);
					if (this.is_map_bcc && ((this.aim == AIM.L) ? ((float)(this.BCC.Mp.clms - this.BCC.Mp.crop) <= _sx) : ((float)this.BCC.Mp.crop > _sx)))
					{
						return false;
					}
				}
				else
				{
					this.aim = ((_sx < _dx) ? AIM.B : AIM.T);
					if (_sy != _dy)
					{
						if (this.is_map_bcc && ((this.aim == AIM.T) ? ((float)(this.BCC.Mp.rows - this.BCC.Mp.crop) <= X.Mn(_sy, _dy)) : ((float)this.BCC.Mp.crop > X.Mx(_sy, _dy))))
						{
							return false;
						}
						is_naname = true;
						this.aim = CAim.get_aim2(0f, 0f, (float)CAim._XD((_sy < _dy) ? AIM.L : AIM.R, 1), (float)CAim._YD(this.aim, 1), false);
						this.line_a = (_sy - _dy) / (_sx - _dx);
						this.line_div = 1f / (this.line_a * this.line_a + 1f);
					}
					else if (this.is_map_bcc && ((this.aim == AIM.T) ? ((float)this.BCC.Mp.rows <= _sy) : (0f >= _sy)))
					{
						return false;
					}
				}
				base.Set(_sx, _sy, 0f, 0f);
				base.Expand(_dx, _dy, 0f, 0f, true);
				this._tostring = null;
				return true;
			}

			public M2BlockColliderContainer.BCCLine Initialize(int _block_index, M2BorderPt _Ps, M2BorderPt _Pd, bool check_pd_chips = false, bool _is_lift = false)
			{
				this.FootStampChipFix = default(M2BlockColliderContainer.BCCPos);
				this.Dispose();
				this._tostring = null;
				this.block_index = _block_index;
				this.SideL = (this.SideR = null);
				this.has_arrangeable = (this.upper_area_checked = (this.has_danger_upper = (this.has_danger_another = false)));
				bool flag;
				if (!this.initPosition(_Ps.x, _Ps.y, _Pd.x, _Pd.y, out flag))
				{
					return this;
				}
				M2BlockColliderContainer.BCCLine.RcPosChk.Set(this.x, base.y, base.w, base.h);
				if (!flag)
				{
					int num = CAim._XD(this.aim, 1);
					int num2 = CAim._YD(this.aim, 1);
					if (num2 == 0)
					{
						M2BlockColliderContainer.BCCLine.RcPosChk.width += 1f;
					}
					if (num == 0)
					{
						M2BlockColliderContainer.BCCLine.RcPosChk.height += 1f;
					}
					if (num2 > 0)
					{
						DRect rcPosChk = M2BlockColliderContainer.BCCLine.RcPosChk;
						float y = rcPosChk.y;
						rcPosChk.y = y - 1f;
					}
					if (num < 0)
					{
						M2BlockColliderContainer.BCCLine.RcPosChk.x -= 1f;
					}
				}
				this.is_lift = _is_lift;
				this.valid = true;
				if (X.LENGTHXYS(this.sx, this.sy, this.dx, this.dy) < 0.5f)
				{
					return this;
				}
				int num3 = (int)X.LENGTHXYN(this.sx, this.sy, this.dx, this.dy);
				if (this.ACp == null)
				{
					this.ACp = new List<M2BlockColliderContainer.BCCPos>(num3);
				}
				else if (this.ACp.Capacity < num3)
				{
					this.ACp.Capacity = num3;
				}
				float num4;
				float num5;
				this.BCC.getBaseShift(out num4, out num5);
				M2BlockColliderContainer.BCCLine.DangerRc.key = "";
				for (int i = 0; i < 2; i++)
				{
					M2BorderPt m2BorderPt = ((i == 0) ? _Ps : _Pd);
					int count = m2BorderPt.APt.Count;
					for (int j = 0; j < count; j++)
					{
						M2BlockColliderContainer.M2PtPos m2PtPos = m2BorderPt.APt[j];
						float num6 = (float)m2PtPos.x + num4;
						float num7 = (float)m2PtPos.y + num5;
						if (M2BlockColliderContainer.BCCLine.RcPosChk.isin(num6, num7, 0f))
						{
							int num8 = X.IntR(num6);
							int num9 = X.IntR(num7);
							M2BlockColliderContainer.BCCPos bccpos;
							if (!this.GetPosAt(num8, num9, out bccpos))
							{
								int count2 = m2PtPos.count;
								M2BlockColliderContainer.BCCPos bccpos2 = default(M2BlockColliderContainer.BCCPos);
								bool flag2 = this.is_lift;
								bool flag3 = false;
								for (int k = 0; k < 2; k++)
								{
									for (int l = count2 - 1; l >= 0; l--)
									{
										M2Chip c = m2PtPos.GetC(l);
										if (c != null && k == 0 != c.Img.isBg() && (!this.is_map_bcc || (!c.active_removed && !c.Lay.do_not_consider_config)))
										{
											if (!flag3)
											{
												string[] array = c.getMeta().Get("foot_type");
												if (array != null)
												{
													if (!bccpos2.valid)
													{
														bccpos2 = new M2BlockColliderContainer.BCCPos(c, num8, num9, m2PtPos.cfg);
													}
													bccpos2.foot_type = array[0];
													flag3 = true;
												}
											}
											if (!flag2 && !c.Img.isBg())
											{
												int config = c.getConfig(m2PtPos.x - c.mapx, m2PtPos.y - c.mapy);
												if (config == m2PtPos.cfg)
												{
													if (!bccpos2.valid)
													{
														bccpos2 = new M2BlockColliderContainer.BCCPos(c, num8, num9, config);
													}
													bool flag4 = CCON.isDangerous(config);
													bool flag5 = false;
													if (flag4)
													{
														if (this.AMapDmg == null)
														{
															this.AMapDmg = this.BCC.PopMDArray();
															this.BCC.has_dangerous_chip = true;
															M2BlockColliderContainer.BCCLine.DangerRc.key = "";
														}
														string text = c.getMeta().GetS("mapdmg");
														if (TX.noe(text))
														{
															text = "_";
														}
														if (M2BlockColliderContainer.BCCLine.DangerRc.key == "")
														{
															M2BlockColliderContainer.BCCLine.DangerRc.key = text;
															M2BlockColliderContainer.BCCLine.DangerRc.Set((float)m2PtPos.x + num4, (float)m2PtPos.y + num5, 1f, 1f);
														}
														else if (text != M2BlockColliderContainer.BCCLine.DangerRc.key)
														{
															this.AMapDmg.Add(this.BCC.Mp.M2D.MDMGCon.Create(M2BlockColliderContainer.BCCLine.DangerRc.key, M2BlockColliderContainer.BCCLine.DangerRc, null));
															M2BlockColliderContainer.BCCLine.DangerRc.key = text;
															M2BlockColliderContainer.BCCLine.DangerRc.Set((float)m2PtPos.x + num4, (float)m2PtPos.y + num5, 1f, 1f);
														}
														else
														{
															M2BlockColliderContainer.BCCLine.DangerRc.Expand((float)m2PtPos.x + num4, (float)m2PtPos.y + num5, 1f, 1f, false);
														}
														if (flag)
														{
															flag5 = true;
														}
													}
													else if (M2BlockColliderContainer.BCCLine.DangerRc != null && M2BlockColliderContainer.BCCLine.DangerRc.key != "")
													{
														flag5 = true;
													}
													if (flag5)
													{
														this.AMapDmg.Add(this.BCC.Mp.M2D.MDMGCon.Create(M2BlockColliderContainer.BCCLine.DangerRc.key, M2BlockColliderContainer.BCCLine.DangerRc, null));
														M2BlockColliderContainer.BCCLine.DangerRc.key = "";
													}
													flag2 = true;
												}
											}
											if (flag2 && flag3)
											{
												break;
											}
										}
									}
									if (flag2 && flag3)
									{
										break;
									}
								}
								if (bccpos2.valid)
								{
									this.ACp.Add(bccpos2);
								}
							}
						}
					}
					if (M2BlockColliderContainer.BCCLine.DangerRc != null && M2BlockColliderContainer.BCCLine.DangerRc.key != "")
					{
						this.AMapDmg.Add(this.BCC.Mp.M2D.MDMGCon.Create(M2BlockColliderContainer.BCCLine.DangerRc.key, M2BlockColliderContainer.BCCLine.DangerRc, null));
						M2BlockColliderContainer.BCCLine.DangerRc.key = "";
					}
					if (i == 0 && !check_pd_chips)
					{
						break;
					}
				}
				if (this.BCC.ALsn != null)
				{
					for (int m = this.BCC.ALsn.Count - 1; m >= 0; m--)
					{
						IBCCEventListener ibcceventListener = this.BCC.ALsn[m];
						if (ibcceventListener.isBCCListenerActive(this))
						{
							this.addBCCEventListener(ibcceventListener, false);
						}
					}
				}
				return this;
			}

			public bool GetPosAt(int x, int y, out M2BlockColliderContainer.BCCPos Pos)
			{
				if (this.ACp != null)
				{
					for (int i = this.ACp.Count - 1; i >= 0; i--)
					{
						M2BlockColliderContainer.BCCPos bccpos = this.ACp[i];
						if (bccpos.x == x && bccpos.y == y)
						{
							Pos = bccpos;
							return true;
						}
					}
				}
				Pos = default(M2BlockColliderContainer.BCCPos);
				return false;
			}

			public void addBCCEventListener(IBCCEventListener L, bool check_dupe = false)
			{
				if (this.ALsnRegistered == null)
				{
					this.ALsnRegistered = this.BCC.PopListenerArray();
				}
				else if (check_dupe && this.ALsnRegistered.IndexOf(L) >= 0)
				{
					return;
				}
				this.ALsnRegistered.Add(L);
			}

			public void addBCCFootListener(IBCCFootListener L, bool check_dupe = false)
			{
				if (this.AFootLsnRegistered == null)
				{
					this.AFootLsnRegistered = this.BCC.PopFLArray();
				}
				else if (check_dupe && this.AFootLsnRegistered.IndexOf(L) >= 0)
				{
					return;
				}
				this.AFootLsnRegistered.Add(L);
			}

			public void remBCCFootListener(IBCCFootListener L, bool check_dupe = false)
			{
				if (this.AFootLsnRegistered == null)
				{
					return;
				}
				this.AFootLsnRegistered.Remove(L);
			}

			public bool LinkIsStraight
			{
				get
				{
					if (this.sx != this.dx)
					{
						return CAim._YD(this.aim, 1) == -1;
					}
					return this.aim == AIM.L;
				}
			}

			public bool is_naname
			{
				get
				{
					return CAim.is_naname(this.aim);
				}
			}

			public AIM foot_aim
			{
				get
				{
					if (!this.is_naname)
					{
						return this.aim;
					}
					if (this.aim != AIM.BL && this.aim != AIM.RB)
					{
						return AIM.T;
					}
					return AIM.B;
				}
			}

			public void setSideLink(M2BlockColliderContainer.BCCLine Cpre, M2BlockColliderContainer.BCCLine Cnext)
			{
				if (this.LinkIsStraight)
				{
					this.SideL = (Cpre.valid ? Cpre : null);
					this.SideR = (Cnext.valid ? Cnext : null);
					return;
				}
				this.SideL = (Cnext.valid ? Cnext : null);
				this.SideR = (Cpre.valid ? Cpre : null);
			}

			public void setSideLinkLeft(M2BlockColliderContainer.BCCLine Cn)
			{
				if (this.SideL == null || this.SideL.y > Cn.y)
				{
					this.SideL = Cn;
				}
			}

			public void setSideLinkRight(M2BlockColliderContainer.BCCLine Cn)
			{
				if (this.SideR == null || this.SideR.y > Cn.y)
				{
					this.SideR = Cn;
				}
			}

			public void ListenerTouched(M2FootManager FootD)
			{
				if (this.ALsnRegistered == null)
				{
					return;
				}
				for (int i = this.ALsnRegistered.Count - 1; i >= 0; i--)
				{
					this.ALsnRegistered[i].BCCtouched(this, FootD);
				}
			}

			public void drawDebug(MeshDrawer Md, Map2d Mp, bool draw_blocks = false, bool draw_near = false, bool draw_side = false)
			{
				if (draw_side && !draw_near)
				{
					if (this.LinkS != null)
					{
						this.LinkS.drawDebug(Md, Mp, false, true, true);
					}
					if (this.LinkD != null)
					{
						this.LinkD.drawDebug(Md, Mp, false, true, true);
					}
				}
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				float num3 = Mp.map2meshx(this.sx - num);
				float num4 = Mp.map2meshy(this.sy - num2);
				float num5 = Mp.map2meshx(this.dx - num);
				float num6 = Mp.map2meshy(this.dy - num2);
				float num7 = (float)(CAim._XD(this.aim, 1) * 4);
				float num8 = (float)(CAim._YD(this.aim, 1) * 4);
				Md.ColGrd.Set(this.calcUpperDanger(true) ? 4294901760U : (this.is_lift ? 4289806728U : 4284509976U));
				if (draw_near && draw_side)
				{
					Md.ColGrd.blend(2583461830U, 0.55f);
				}
				else if (draw_near)
				{
					Md.ColGrd.blend(301989887U, 0.875f);
				}
				Md.Col = Md.ColGrd.C;
				Md.Line(num3 + num7, num4 + num8, num5 + num7, num6 + num8, 5f, false, 0f, 0f);
				Md.ColGrd.Set(this.is_lift ? 4294156340U : 4292210262U);
				if (draw_near && draw_side)
				{
					Md.ColGrd.blend(2583461830U, 0.55f);
				}
				else if (draw_near)
				{
					Md.ColGrd.blend(301989887U, 0.875f);
				}
				Md.Col = Md.ColGrd.C;
				Md.Line(num3, num4, num5, num6, 2f, false, 0f, 0f);
				Md.Poly((num3 + num5) * 0.5f + num7 * 0.3f, (num4 + num6) * 0.5f + num8 * 0.3f, 6f, CAim.get_agR(this.aim, 0f), 3, 0f, false, 0f, 0f);
				Md.ColGrd.Set(this.is_lift ? 4294156340U : 4289459557U);
				if (!draw_near || !draw_side)
				{
					if (draw_near)
					{
						Md.ColGrd.blend(301989887U, 0.875f);
					}
					Md.Col = Md.ColGrd.C;
					int num9 = 10;
					if (draw_blocks && this.LinkS != null)
					{
						num9 = (this.S_is_90(this.LinkS) ? 3 : 4);
					}
					Md.Poly(num3, num4, 10f, 0f, num9, 2f, false, 0f, 0f);
					if (this.is_lift || draw_blocks || (draw_near && !draw_side))
					{
						num9 = 10;
						if (draw_blocks && this.LinkD != null)
						{
							num9 = (this.D_is_90(this.LinkD) ? 3 : 4);
						}
						Md.Poly(num5, num6, 10f, 0f, num9, 2f, false, 0f, 0f);
					}
				}
				if (draw_blocks && this.ACp != null)
				{
					for (int i = this.ACp.Count - 1; i >= 0; i--)
					{
						M2BlockColliderContainer.BCCPos bccpos = this.ACp[i];
						M2Chip m2Chip = bccpos.Cp as M2Chip;
						if (m2Chip != null)
						{
							Md.ColGrd.Set(4293599778U);
							if (draw_near)
							{
								Md.ColGrd.blend(301989887U, 0.875f);
							}
							float num10 = Mp.map2meshx((float)m2Chip.mapx + (float)m2Chip.clms * 0.5f);
							float num11 = Mp.map2meshy((float)m2Chip.mapy + (float)m2Chip.rows * 0.5f);
							Md.Box(num10, num11, (float)m2Chip.clms * Mp.CLEN - 2f, (float)m2Chip.rows * Mp.CLEN - 2f, 2f, false);
							float num12 = Mp.map2meshx((float)bccpos.x + 0.5f - num);
							float num13 = Mp.map2meshy((float)bccpos.y + 0.5f - num2);
							Md.Poly(num12, num13, 2f, 0f, 12, 0f, false, 0f, 0f);
						}
					}
				}
			}

			public override string ToString()
			{
				if (this._tostring == null)
				{
					STB stb = TX.PopBld(null, 0);
					stb += this.sx;
					stb.Add(", ", this.sy, ">");
					stb += this.dx;
					stb.Add(", ", this.dy, " / a: ");
					stb += FEnum<AIM>.ToStr(this.aim);
					this._tostring = stb.ToString();
					TX.ReleaseBld(stb);
				}
				return this._tostring;
			}

			public bool isUseableDir(M2FootManager FootD)
			{
				return this.isUseableDir(FootD.footable_bits);
			}

			public bool isUseableDir(uint footable_bits)
			{
				if ((footable_bits & 15U) == 15U)
				{
					return true;
				}
				int num = CAim._XD(this.aim, 1);
				int num2 = CAim._YD(this.aim, 1);
				return (num != 0 && (footable_bits & ((num <= 0 || 4 != 0) ? 1U : 0U)) != 0U) || (num2 != 0 && (footable_bits & ((num2 > 0) ? 2U : 8U)) != 0U);
			}

			public bool isUseablePos(float l, float t, float r, float b, float margin)
			{
				switch (this.aim)
				{
				case AIM.L:
					return X.Abs(l - base.right) < margin + 0.125f && X.isCovering(base.y, base.bottom, t, b, margin);
				case AIM.T:
					return X.Abs(t - base.bottom) < margin + 0.125f && X.isCovering(this.x, base.right, l, r, margin);
				case AIM.R:
					return X.Abs(r - this.x) < margin + 0.125f && X.isCovering(base.y, base.bottom, t, b, margin);
				case AIM.B:
					return X.Abs(b - base.y) < margin + 0.125f && X.isCovering(this.x, base.right, l, r, margin);
				default:
					return X.isCovering(base.y, base.bottom, t, b, margin) && X.isCovering(this.x, base.right, l, r, margin);
				}
			}

			public bool isUseableVelocity(M2Phys Phy)
			{
				float pre_force_velocity_x = Phy.pre_force_velocity_x;
				float force_velocity_y_with_gravity = Phy.force_velocity_y_with_gravity;
				return this.isUseableVelocity(Phy, pre_force_velocity_x, force_velocity_y_with_gravity);
			}

			public bool isUseableVelocity(M2Phys Phy, float vx, float vy)
			{
				switch (this.aim)
				{
				case AIM.L:
					return vx <= 0f;
				case AIM.T:
					return vy <= 0f;
				case AIM.R:
					return vx >= 0f;
				case AIM.B:
					return vy >= 0f;
				default:
					return vy == 0f || CAim._YD(this.aim, 1) > 0 == vy < 0f;
				}
			}

			public IFootable isCarryable(M2FootManager FootD)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.isCarryableShifted(FootD, num, num2, 0f, false, 0f);
			}

			public IFootable isCarryable(M2FootManager FootD, float expand, bool bit_allow = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.isCarryableShifted(FootD, num, num2, expand, bit_allow, 0f);
			}

			public IFootable isCarryableShifted(M2FootManager FootD, float shiftx, float shifty, float expand, bool bit_allow = false, float pre_fall_y = 0f)
			{
				if (this.is_lift && !bit_allow)
				{
					IFootable footable = FootD.Mv.checkSkipLift(this);
					if (footable != this)
					{
						return footable;
					}
				}
				int pivot = (int)FootD.pivot;
				float num = 0.22f + expand;
				float num2 = FootD.Phy.tstacked_x - FootD.sizex * (float)pivot;
				float num3 = FootD.Phy.tstacked_y - FootD.sizey * (float)pivot;
				float num4 = FootD.Phy.tstacked_x + FootD.sizex * (float)pivot;
				float num5 = FootD.Phy.tstacked_y + FootD.sizey * (float)pivot;
				if (this.is_ladder)
				{
					num3 -= 1f;
				}
				if (this.aim == AIM.R)
				{
					if (!bit_allow && !FootD.FootIs(this) && (FootD.footable_bits & 4U) == 0U)
					{
						return null;
					}
					if (X.Abs(num4 + shiftx - this.x) >= num || !X.isCovering(base.y, base.bottom, num3 + shifty, num5 + shifty, expand))
					{
						return null;
					}
					return this;
				}
				else if (this.aim == AIM.L)
				{
					if (!bit_allow && !FootD.FootIs(this) && (FootD.footable_bits & 1U) == 0U)
					{
						return null;
					}
					if (X.Abs(num2 + shiftx - this.x) >= num || !X.isCovering(base.y, base.bottom, num3 + shifty, num5 + shifty, expand))
					{
						return null;
					}
					return this;
				}
				else
				{
					if (this.is_lift)
					{
						shifty += X.Mx(0f, num * 0.3f - X.Mx(0f, FootD.Phy.pre_force_velocity_y));
					}
					if (!X.isCovering(this.x, base.right, num2 + shiftx, num4 + shiftx, expand))
					{
						return null;
					}
					int num6 = CAim._XD(this.aim, 1);
					int num7 = CAim._YD(this.aim, 1);
					if (!FootD.FootIs(this) && (num6 == 0 || (FootD.footable_bits & ((num6 <= 0 || 4 != 0) ? 1U : 0U)) == 0U) && (FootD.footable_bits & ((num7 > 0) ? 2U : 8U)) == 0U)
					{
						return null;
					}
					float num8 = num;
					if (!FootD.hasFoot() && this.is_lift)
					{
						float force_velocity_y_with_gravity = FootD.Phy.force_velocity_y_with_gravity;
						if (num7 > 0 == force_velocity_y_with_gravity > 0f)
						{
							num8 += X.Abs(force_velocity_y_with_gravity);
						}
						if (num6 != 0)
						{
							num8 += X.Abs(FootD.Phy.pre_force_velocity_x);
						}
					}
					if (this.aim == AIM.T)
					{
						if (X.Abs(num3 + shifty - base.y) >= num8)
						{
							return null;
						}
						return this;
					}
					else if (this.aim == AIM.B)
					{
						if (X.Abs(num5 + shifty - base.y) >= num8)
						{
							return null;
						}
						return this;
					}
					else
					{
						if (!X.isCovering(base.y - num8, base.bottom + num8, num3 + shifty, num5 + shifty, expand))
						{
							return null;
						}
						if (X.Pow2(this.line_a * (((num6 < 0) ? num2 : num4) + shiftx) - ((num7 > 0) ? num3 : num5) - shifty + (this.sy - this.sx * this.line_a)) * this.line_div < num8)
						{
							return this;
						}
						float tstacked_x = FootD.Phy.tstacked_x;
						float num9 = tstacked_x + (float)num6 * FootD.sizex;
						if (!X.BTW(this.x - expand, num9 + shiftx, base.right + expand))
						{
							float num10;
							float num11;
							if (num6 > 0)
							{
								num10 = num9;
								num11 = tstacked_x - (float)num6 * FootD.sizex;
							}
							else
							{
								num11 = num9;
								num10 = tstacked_x - (float)num6 * FootD.sizex;
							}
							if (!X.isCovering(num11 + shiftx, num10 + shiftx, this.x, base.right, expand))
							{
								return null;
							}
						}
						num9 = X.MMX(this.x - shiftx, num9, base.right - shiftx);
						float num12 = this.slopeBottomY(num9, shiftx, shifty, true);
						float num13 = FootD.Phy.tstacked_y - (float)num7 * FootD.sizey;
						if (X.Abs(num12 - num13) < num8)
						{
							return this;
						}
						if (num7 < 0 == num12 < num13)
						{
							float num14 = tstacked_x - (float)num6 * FootD.sizex;
							float num15 = this.slopeBottomY(num14, shiftx, shifty, true);
							if (num7 < 0 == num15 > num13)
							{
								return this;
							}
						}
						return null;
					}
				}
			}

			public M2BlockColliderContainer.BCCLine SideRidingCheck(M2FootManager FootD, float x, float y, float moved_x, float moved_y, bool checking = false, float progress = 0f, bool force_over = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.SideRidingCheckShifted(FootD, x + num, y + num2, moved_x, moved_y, checking, progress, force_over);
			}

			private M2BlockColliderContainer.BCCLine SideRidingCheckShifted(M2FootManager FootD, float x, float y, float moved_x, float moved_y, bool checking = false, float progress = 0f, bool force_over = false)
			{
				int pivot = (int)FootD.pivot;
				float num = (FootD.Phy.is_stucking ? 0.15f : 0f);
				if (this.aim == AIM.L || this.aim == AIM.R)
				{
					if (this.SideL != null && moved_y < 0f && this.SideL.isUseableDir(FootD) && y - (float)pivot * FootD.sizey * (float)X.MPF(this.L_is_90 || force_over) - (progress + num) <= base.y)
					{
						if (this.SideL.isCarryable(FootD, 0.14f, false) == null)
						{
							return null;
						}
						FootD.rideInitTo(this.SideL, false);
						if (progress > 0f && FootD.get_FootBCC() == this.SideL)
						{
							float num2;
							float num3;
							this.SideL.fixToFootPos(FootD, x, y, out num2, out num3, false);
							FootD.Phy.addTranslateStack(num2, num3);
						}
						return this.SideL;
					}
					else if (this.SideR != null && moved_y > 0f && this.SideR.isUseableDir(FootD) && y + (float)pivot * FootD.sizey * (float)X.MPF(this.R_is_90 || force_over) + (progress + num) >= base.bottom)
					{
						if (this.SideR.isCarryable(FootD, 0.14f, false) == null)
						{
							return null;
						}
						FootD.rideInitTo(this.SideR, false);
						if (progress > 0f && FootD.get_FootBCC() == this.SideR)
						{
							float num4;
							float num5;
							this.SideR.fixToFootPos(FootD, x, y, out num4, out num5, false);
							FootD.Phy.addTranslateStack(num4, num5);
						}
						return this.SideR;
					}
				}
				else if (moved_x != 0f)
				{
					bool flag = moved_x < 0f;
					if (flag ? (x - FootD.sizex <= this.x) : (base.right <= x + FootD.sizex))
					{
						M2BlockColliderContainer.BCCLine bccline = this.checkSideRidingShifted(FootD, x, flag, (float)pivot * FootD.sizex, force_over, progress + num);
						if (bccline != null)
						{
							FootD.rideInitTo(bccline, false);
							if (progress > 0f && FootD.get_FootBCC() == bccline)
							{
								float num6;
								float num7;
								bccline.fixToFootPos(FootD, x, y, out num6, out num7, false);
								FootD.Phy.addTranslateStack(num6, num7);
							}
							return bccline;
						}
					}
				}
				return null;
			}

			private M2BlockColliderContainer.BCCLine checkSideRidingShifted(M2FootManager FootD, float shifted_x, bool to_left, float pivot_size, bool force_over, float progress_shiftx)
			{
				M2BlockColliderContainer.BCCLine bccline = (to_left ? this.SideL : this.SideR);
				bool flag = (to_left ? (this.line_a < 0f) : (this.line_a > 0f));
				if (bccline != null && !bccline.isUseableDir(FootD))
				{
					bccline = null;
				}
				bool flag2 = true;
				if (bccline != null && !bccline.is_lift)
				{
					bool flag3 = (to_left ? (bccline.line_a > 0f) : (bccline.line_a < 0f));
					if (!flag && flag3)
					{
						flag2 = false;
					}
				}
				int num = 1;
				M2BlockColliderContainer.BCCLine bccline2 = null;
				float num2 = 0f;
				float num3 = (float)X.MPF(to_left);
				int i = -1;
				while (i < num)
				{
					M2BlockColliderContainer.BCCLine bccline3;
					bool flag4;
					if (i < 0)
					{
						bccline3 = bccline;
						if (bccline3 != null)
						{
							flag4 = (to_left ? this.L_is_90 : this.R_is_90);
							goto IL_01C6;
						}
					}
					else
					{
						if (i == 0)
						{
							if (!flag2 || this.BCC.ALift == null)
							{
								break;
							}
							num = this.BCC.ALift.Count;
							if (num == 0)
							{
								break;
							}
						}
						bccline3 = this.BCC.ALift[i];
						if (bccline3 != bccline && bccline3 != this && bccline3.isUseableDir(FootD))
						{
							bool flag5;
							if (bccline3.line_a == 0f)
							{
								flag5 = X.BTWW(bccline3.x, to_left ? this.x : base.right, bccline3.right);
							}
							else
							{
								flag5 = (to_left ? (this.x == bccline3.right) : (base.right == bccline3.x));
							}
							if (flag5 && (to_left ? (this.left_y == bccline3.right_y) : (this.right_y == bccline3.left_y)))
							{
								flag4 = ((this.LinkIsStraight == to_left) ? this.S_is_90(bccline3) : this.D_is_90(bccline3));
								goto IL_01C6;
							}
						}
					}
					IL_0260:
					i++;
					continue;
					IL_01C6:
					float num4 = shifted_x - num3 * (pivot_size * (float)X.MPF(flag4 || force_over || bccline3.foot_aim == this.foot_aim) + progress_shiftx);
					if ((to_left ? (num4 <= this.x) : (num4 >= base.right)) && bccline3.isCarryable(FootD, 0.1f, false) != null)
					{
						if (bccline2 == null || (to_left ? (bccline3.line_a > num2) : (bccline3.line_a < num2)))
						{
							bccline2 = bccline3;
							num2 = bccline3.line_a;
							goto IL_0260;
						}
						goto IL_0260;
					}
					else
					{
						if (i == -1)
						{
							flag2 = true;
							goto IL_0260;
						}
						goto IL_0260;
					}
				}
				return bccline2;
			}

			public float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
			{
				return this.fixToFootPos(FootD, x, y, out dx, out dy, true);
			}

			public float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy, bool check_side_riding)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				if (check_side_riding)
				{
					M2BlockColliderContainer.BCCLine bccline = this.SideRidingCheckShifted(FootD, x + num, y + num2, x - FootD.Mv.x, y - FootD.Mv.y, false, 0f, false);
					if (bccline != null)
					{
						return bccline.fixToFootPos(FootD, x, y, out dx, out dy, false);
					}
				}
				int pivot = (int)FootD.pivot;
				float num3 = 0.32999998f;
				switch (this.aim)
				{
				case AIM.L:
					dx = base.right - num - (x - FootD.sizex * (float)pivot);
					dy = 0f;
					break;
				case AIM.T:
					dy = base.bottom - num2 - (y - FootD.sizey * (float)pivot);
					dx = 0f;
					break;
				case AIM.R:
					dx = this.x - num - (x + FootD.sizex * (float)pivot);
					dy = 0f;
					break;
				case AIM.B:
					dy = base.y - num2 - (y + FootD.sizey * (float)pivot);
					dx = 0f;
					break;
				default:
				{
					float num4 = x + FootD.sizex * (float)CAim._XD(this.aim, 1) * (float)pivot;
					float num5 = y - FootD.sizey * (float)CAim._YD(this.aim, 1) * (float)pivot;
					float num6 = this.slopeBottomY(num4, num, num2, true);
					dx = 0f;
					dy = num6 - num5;
					num3 += X.Abs((x - FootD.Mv.x) * this.line_a);
					break;
				}
				}
				return num3;
			}

			public float isFallable(float cx, float cy, float marginx, float marginy)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.isFallableShifted(num, num2, cx, cy, marginx, marginy);
			}

			public float isFallableShifted(float shiftx, float shifty, float cx, float cy, float marginx, float marginy)
			{
				if (!X.BTW(this.x - marginx, cx + shiftx, base.right + marginx))
				{
					return -1000f;
				}
				if (!((this._yd < 0) ? X.BTW(base.y - marginy, cy + shifty, base.bottom + marginy + 0.75f) : X.BTW(base.y - marginy - 0.75f, cy + shifty, base.bottom + marginy)))
				{
					return -1000f;
				}
				if (this.aim == AIM.B || this.aim == AIM.T)
				{
					if (X.Abs(this.sy - shifty - cy) > marginy)
					{
						return -1000f;
					}
					return this.sy - shifty + (float)this._yd * marginy;
				}
				else
				{
					float num = this.slopeBottomY(cx, shiftx, shifty, true);
					if (X.Abs(num - cy) > marginy)
					{
						return -1000f;
					}
					return num - marginy;
				}
			}

			public float stuckInWall_depert_len(float cx, float cy, float sizex, float sizey, float out_marg = 1.15f)
			{
				cx += this.BCC.base_shift_x;
				cy += this.BCC.base_shift_y;
				sizex = X.Mx(sizex - 0.2f, 0f);
				sizey = X.Mx(sizey - 0.2f, 0f);
				float num = 0f;
				if (this.aim == AIM.L)
				{
					if (base.bottom < cy + sizey)
					{
						num += cy + sizey - base.bottom + out_marg;
					}
					else if (cy - sizey < base.y)
					{
						num += base.y - (cy - sizey) + out_marg;
					}
					return num + ((this.aim == AIM.L) ? X.Mx(0f, this.shifted_x - (cx - sizex)) : X.Mx(0f, cx + sizex - this.shifted_x));
				}
				if (base.right < cx + sizex)
				{
					num += cx + sizex - base.right + out_marg;
					cx = base.right - sizex;
				}
				else if (cx - sizex < this.x)
				{
					num += this.x - (cx - sizex) + out_marg;
					cx = this.x + sizex;
				}
				if (this.aim == AIM.T)
				{
					num += X.Mx(0f, this.shifted_y - (cy - sizey));
				}
				else if (this.aim == AIM.B)
				{
					num += X.Mx(0f, cy + sizey - this.shifted_y);
				}
				else
				{
					float num2 = this.slopeBottomY(cx, 0f, 0f, true);
					if (this.foot_aim == AIM.T)
					{
						num += X.Mx(0f, num2 - (cy - sizey));
					}
					else
					{
						num += X.Mx(0f, cy + sizey - num2);
					}
				}
				return num;
			}

			public void quitCarry(ICarryable FootD)
			{
				if (this.BCC.BelongTo != null)
				{
					this.BCC.BelongTo.quitCarry(FootD);
				}
			}

			public IFootable initCarry(ICarryable FootD)
			{
				if (this.BCC.BelongTo != null && this.BCC.BelongTo.initCarry(FootD) == null)
				{
					return null;
				}
				return this;
			}

			public float get_carry_vx()
			{
				return 0f;
			}

			public float get_carry_vy()
			{
				return 0f;
			}

			public bool isFloor()
			{
				return CAim._YD(this.aim, 1) == -1;
			}

			public bool isWall()
			{
				return CAim._YD(this.aim, 1) == 0;
			}

			public bool isCeil()
			{
				return CAim._YD(this.aim, 1) == 1;
			}

			public bool has_danger_chip
			{
				get
				{
					return this.AMapDmg != null;
				}
			}

			public bool calcUpperDanger(bool fine_upper_area = false)
			{
				if (fine_upper_area && !this.upper_area_checked)
				{
					this.upper_area_checked = true;
					this.has_danger_upper = false;
					this.EachPixel(new Func<int, int, M2BlockColliderContainer.BCCLine, bool>(M2BlockColliderContainer.BCCLine.calcUpperDangerInner), true, 0f);
				}
				return this.has_danger_upper;
			}

			private static bool calcUpperDangerInner(int mapx, int mapy, M2BlockColliderContainer.BCCLine _Line)
			{
				Map2d mp = _Line.BCC.Mp;
				int yd = _Line._yd;
				if (yd != 0)
				{
					if (X.Abs(_Line.cx - (float)mapx) >= 3f)
					{
						return true;
					}
				}
				else if (_Line.bottom - (float)mapy > 4f)
				{
					return true;
				}
				AIM aim = _Line.aim;
				if (aim != AIM.L && aim != AIM.R)
				{
					int num;
					int num2;
					if (yd > 0)
					{
						num = mapy - 1;
						num2 = mapy + 2;
					}
					else
					{
						num = mapy - 2;
						num2 = mapy + 1;
					}
					for (int i = num; i <= num2; i++)
					{
						if (mp.isDangerous(mapx, i))
						{
							_Line.has_danger_upper = true;
							return false;
						}
					}
					return true;
				}
				if (mp.isDangerous(mapx, mapy) || mp.isDangerous(mapx + _Line._xd, mapy))
				{
					_Line.has_danger_upper = true;
					return false;
				}
				return true;
			}

			public Vector3 getSlideCarryAim(AIM depa, float cx, float cy)
			{
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
				{
					if (CAim._XD(depa, 1) != this._xd)
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 0f);
					}
					bool l_is_ = this.L_is_270;
					bool r_is_ = this.R_is_270;
					if (!l_is_ && !r_is_)
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 2f);
					}
					if (l_is_ && r_is_)
					{
						float num = X.Abs(this.shifted_y - cy);
						float num2 = X.Abs(this.shifted_bottom - cy);
						if (num >= num2)
						{
							return new Vector3(0f, 1f, 1f);
						}
						return new Vector3(0f, -1f, 1f);
					}
					else
					{
						if (!l_is_)
						{
							return new Vector3(0f, 1f, 1f);
						}
						return new Vector3(0f, -1f, 1f);
					}
					break;
				}
				case AIM.T:
				case AIM.B:
				{
					if (CAim._YD(depa, 1) != this._yd)
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 0f);
					}
					bool l_is_2 = this.L_is_270;
					bool r_is_2 = this.R_is_270;
					if (!l_is_2 && !r_is_2)
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 2f);
					}
					if (l_is_2 && r_is_2)
					{
						float num3 = X.Abs(this.shifted_x - cx);
						float num4 = X.Abs(this.shifted_right - cx);
						if (num3 >= num4)
						{
							return new Vector3(1f, 0f, 1f);
						}
						return new Vector3(-1f, 0f, 1f);
					}
					else
					{
						if (!l_is_2)
						{
							return new Vector3(1f, 0f, 1f);
						}
						return new Vector3(-1f, 0f, 1f);
					}
					break;
				}
				default:
				{
					bool flag = CAim._XD(depa, 1) != 0;
					int num5 = CAim._YD(depa, 1);
					if ((!flag || CAim._XD(depa, 1) != this._xd) && (num5 == 0 || CAim._YD(depa, 1) != this._yd))
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 0f);
					}
					bool flag2 = this.SideL == null || this.L_is_270 || this.SideL._yd == this._yd;
					bool flag3 = this.SideR == null || this.R_is_270 || this.SideR._yd == this._yd;
					if (!flag2 && !flag3)
					{
						return new Vector3((float)CAim._XD(depa, 1), (float)(-(float)CAim._YD(depa, 1)), 2f);
					}
					if (flag2 && flag3)
					{
						float num6 = X.Abs(this.shifted_x - cx);
						float num7 = X.Abs(this.shifted_right - cx);
						if (num6 >= num7)
						{
							return new Vector3(1f, 1f * this.line_a, 1f);
						}
						return new Vector3(-1f, -1f * this.line_a, 1f);
					}
					else
					{
						if (!flag2)
						{
							return new Vector3(1f, 1f * this.line_a, 1f);
						}
						return new Vector3(-1f, -1f * this.line_a, 1f);
					}
					break;
				}
				}
			}

			public bool is_map_bcc
			{
				get
				{
					return this.BCC.Mp.BCC == this.BCC;
				}
			}

			public bool L_is_90
			{
				get
				{
					if (this.SideL == null)
					{
						return false;
					}
					if (!this.LinkIsStraight)
					{
						return this.D_is_90(this.SideL);
					}
					return this.S_is_90(this.SideL);
				}
			}

			public bool R_is_90
			{
				get
				{
					if (this.SideR == null)
					{
						return false;
					}
					if (!this.LinkIsStraight)
					{
						return this.S_is_90(this.SideR);
					}
					return this.D_is_90(this.SideR);
				}
			}

			public bool L_is_270
			{
				get
				{
					if (this.SideL == null)
					{
						return false;
					}
					if (!this.LinkIsStraight)
					{
						return !this.D_is_90(this.SideL);
					}
					return !this.S_is_90(this.SideL);
				}
			}

			public bool R_is_270
			{
				get
				{
					if (this.SideR == null)
					{
						return false;
					}
					if (!this.LinkIsStraight)
					{
						return !this.S_is_90(this.SideR);
					}
					return !this.D_is_90(this.SideR);
				}
			}

			public bool L_is_same_yd
			{
				get
				{
					return this.SideL != null && CAim._YD(this.aim, 1) == CAim._YD(this.SideL.aim, 1);
				}
			}

			public bool R_is_same_yd
			{
				get
				{
					return this.SideR != null && CAim._YD(this.aim, 1) == CAim._YD(this.SideR.aim, 1);
				}
			}

			public int _xd
			{
				get
				{
					return CAim._XD(this.aim, 1);
				}
			}

			public int _yd
			{
				get
				{
					return CAim._YD(this.aim, 1);
				}
			}

			public float shifted_x
			{
				get
				{
					return this.x - this.BCC.base_shift_x;
				}
			}

			public float shifted_y
			{
				get
				{
					return base.y - this.BCC.base_shift_y;
				}
			}

			public float shifted_sx
			{
				get
				{
					return this.sx - this.BCC.base_shift_x;
				}
			}

			public float shifted_sy
			{
				get
				{
					return this.sy - this.BCC.base_shift_y;
				}
			}

			public float shifted_dx
			{
				get
				{
					return this.dx - this.BCC.base_shift_x;
				}
			}

			public float shifted_dy
			{
				get
				{
					return this.dy - this.BCC.base_shift_y;
				}
			}

			public float shifted_right
			{
				get
				{
					return this.x - this.BCC.base_shift_x + this.width;
				}
			}

			public float shifted_bottom
			{
				get
				{
					return base.y - this.BCC.base_shift_y + this.height;
				}
			}

			public float shifted_cx
			{
				get
				{
					return base.cx - this.BCC.base_shift_x;
				}
			}

			public float shifted_cy
			{
				get
				{
					return base.cy - this.BCC.base_shift_y;
				}
			}

			public float x_MMX_shifted(float _x)
			{
				return X.MMX(this.shifted_x, _x, this.shifted_right);
			}

			public float y_MMX_shifted(float _y)
			{
				return X.MMX(this.shifted_y, _y, this.shifted_bottom);
			}

			public float left_y
			{
				get
				{
					AIM aim = this.aim;
					float num;
					if (aim <= AIM.B)
					{
						num = base.y;
					}
					else
					{
						num = ((this._yd < 0) ? this.sy : this.dy);
					}
					return num;
				}
			}

			public float shifted_left_y
			{
				get
				{
					return this.left_y - this.BCC.base_shift_y;
				}
			}

			public float right_y
			{
				get
				{
					AIM aim = this.aim;
					float num;
					if (aim <= AIM.B)
					{
						num = base.y;
					}
					else
					{
						num = ((this._yd < 0) ? this.dy : this.sy);
					}
					return num;
				}
			}

			public float shifted_right_y
			{
				get
				{
					return this.right_y - this.BCC.base_shift_y;
				}
			}

			public float sright
			{
				get
				{
					return base.right - this.BCC.base_shift_x;
				}
			}

			public float sbottom
			{
				get
				{
					return base.bottom - this.BCC.base_shift_y;
				}
			}

			public bool isCoveringXyS(float _x, float _y, float _r, float _b, float _extend = 0f)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return X.isCovering(this.x - num, base.right - num, _x, _r, _extend) && X.isCovering(base.y - num2, base.bottom - num2, _y, _b, _extend);
			}

			private bool isContainingS(float l, float t, float r, float b, float _extend = 0f)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				l += num - _extend;
				t += num2 - _extend;
				r += num + _extend;
				b += num2 + _extend;
				return this.isContainingXy(l, t, r, b, 0f);
			}

			public bool isinWall(float _x, float _y, float shiftx, float shifty, float _extend_x = 0f, float _extend_y = 0f)
			{
				_x += shiftx;
				_y += shifty;
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					return _x > this.x == (this.aim == AIM.R);
				case AIM.T:
				case AIM.B:
					return _y > base.y == (this.aim == AIM.B);
				default:
				{
					float num = this.slopeBottomY(_x, 0f, 0f, true);
					return _y > num == (this.foot_aim == AIM.B);
				}
				}
			}

			public float housenagR
			{
				get
				{
					if (!this.is_naname)
					{
						return CAim.get_agR(CAim.get_opposite(this.aim), 0f);
					}
					return this.BCC.Mp.GAR(this.sx, this.sy, this.dx, this.dy) + 1.5707964f;
				}
			}

			public float sd_agR
			{
				get
				{
					return this.housenagR - 1.5707964f;
				}
			}

			public bool is_ladder
			{
				get
				{
					return this.is_lift && (this.aim == AIM.L || this.aim == AIM.R);
				}
			}

			public M2BlockColliderContainer.BCCLine LinkS
			{
				get
				{
					if (!this.LinkIsStraight)
					{
						return this.SideR;
					}
					return this.SideL;
				}
			}

			public M2BlockColliderContainer.BCCLine LinkD
			{
				get
				{
					if (!this.LinkIsStraight)
					{
						return this.SideL;
					}
					return this.SideR;
				}
			}

			public bool S_is_90(M2BlockColliderContainer.BCCLine Ln)
			{
				return Ln != null && (!CAim.is_naname(this.aim) || (Ln.aim != AIM.B && Ln.aim != AIM.T)) && (!CAim.is_naname(Ln.aim) || (this.aim != AIM.B && this.aim != AIM.T && !CAim.is_naname(this.aim))) && CAim.get_dif(Ln.aim, this.aim, 4) < 0;
			}

			public bool D_is_90(M2BlockColliderContainer.BCCLine Ln)
			{
				return Ln != null && (!CAim.is_naname(this.aim) || (Ln.aim != AIM.B && Ln.aim != AIM.T)) && (!CAim.is_naname(Ln.aim) || (this.aim != AIM.B && this.aim != AIM.T && !CAim.is_naname(this.aim))) && CAim.get_dif(this.aim, Ln.aim, 4) < 0;
			}

			public float point_line_len2_shifted(float px, float py, float shiftx, float shifty)
			{
				px += shiftx;
				py += shifty;
				float num;
				if (this.aim == AIM.L || this.aim == AIM.R)
				{
					num = X.Pow2(px - this.sx);
					if (py < base.top)
					{
						num += X.Pow2(base.top - py);
					}
					if (py > base.bottom)
					{
						num += X.Pow2(py - base.bottom);
					}
				}
				else
				{
					if (!this.is_naname)
					{
						num = X.Pow2(py - this.sy);
					}
					else
					{
						num = X.Pow2(this.line_a * px - py + (this.sy - this.sx * this.line_a)) * this.line_div;
					}
					if (px < base.left)
					{
						num += X.Pow2(base.left - px);
					}
					if (px > base.right)
					{
						num += X.Pow2(px - base.right);
					}
				}
				return num;
			}

			public float slopeBottomY(float fx)
			{
				return this.slopeBottomY(fx, this.BCC.base_shift_x, this.BCC.base_shift_y, true);
			}

			public float slopeBottomY(float fx, float shiftx, float shifty, bool use_limit = true)
			{
				if (this.aim == AIM.B || this.aim == AIM.T)
				{
					return base.y - shifty;
				}
				float num = this.line_a * fx + (this.sy - shifty - (this.sx - shiftx) * this.line_a);
				if (!use_limit)
				{
					return num;
				}
				return X.MMX(base.y - shifty, num, base.bottom - shifty);
			}

			public float getTiltLevel()
			{
				if (!this.is_naname)
				{
					return 0f;
				}
				return 100f * (base.right - this.x) / (base.bottom - base.top) * (float)X.MPF(this.aim == AIM.TR || this.aim == AIM.BL);
			}

			public float getTiltLevel01()
			{
				if (!this.is_naname)
				{
					return 0f;
				}
				return (base.right - this.x) / (base.bottom - base.top) * (float)X.MPF(this.aim == AIM.TR || this.aim == AIM.BL);
			}

			public float slopeHitX(float fy)
			{
				return this.slopeHitX(fy, this.BCC.base_shift_x, this.BCC.base_shift_y);
			}

			public float slopeHitX(float fy, float shiftx, float shifty)
			{
				if (this.aim == AIM.R || this.aim == AIM.L)
				{
					return this.x - shiftx;
				}
				if (this.line_a == 0f)
				{
					return X.NI(this.x, base.right, 0.5f) - shiftx;
				}
				return X.MMX(this.x - shiftx, (fy - (this.sy - shifty - (this.sx - shiftx) * this.line_a)) * this.line_div, base.right - shiftx);
			}

			public Vector2 linePosition(float absx, float absy, float marginx = 0f, float marginy = 0f)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					return new Vector2(this.x - num, X.MMX(base.y - num2 + marginy, absy, base.bottom - num2 - marginy));
				case AIM.T:
				case AIM.B:
					return new Vector2(X.MMX(this.x - num + marginx, absx, base.right - num - marginx), base.y - num2);
				default:
				{
					absx = X.MMX(this.x - num + marginx, absx, base.right - num - marginx);
					float num3 = this.slopeBottomY(absx, num, num2, true);
					return new Vector2(absx, num3);
				}
				}
			}

			public bool TwoPointCrossing(float x1, float y1, float x2, float y2, float radiusx, float radiusy, bool directional_check = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.TwoPointCrossingShifted(x1 + num, y1 + num2, x2 + num, y2 + num2, radiusx, radiusy, directional_check);
			}

			public bool TwoPointCrossingShifted(float x1, float y1, float x2, float y2, float radiusx, float radiusy, bool directional_check = false)
			{
				if (!X.isCoveringR(this.x, base.right, x1, x2, radiusx) || !X.isCoveringR(base.y, base.bottom, y1, y2, radiusy))
				{
					return false;
				}
				if (directional_check)
				{
					AIM aim = this.aim;
					if (aim == AIM.L || aim == AIM.R)
					{
						if (x1 == x2 || x1 > x2 != this._xd < 0)
						{
							return false;
						}
					}
					else
					{
						if ((this.aim == AIM.T || this.aim == AIM.B) && y1 == y2)
						{
							return false;
						}
						if (y1 > y2 != this._yd > 0)
						{
							return false;
						}
					}
				}
				float num = this.sx;
				float num2 = this.sy;
				float num3 = this.dx;
				float num4 = this.dy;
				float num5 = (num - num3) * (y1 - num2) - (num2 - num4) * (x1 - num);
				float num6 = (num - num3) * (y2 - num2) - (num2 - num4) * (x2 - num);
				return num5 * num6 <= 0f;
			}

			public Vector3 crosspoint(float sx, float sy, float dx, float dy, float radiusx, float radiusy)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				float num3;
				float num4;
				float num5;
				float num6;
				X.calcLineGraphVariable(sx, sy, dx, dy, out num3, out num4, out num5, out num6);
				return this.crosspointLshifted(num3, num4, num5, num6, radiusx, radiusy, num, num2);
			}

			public Vector3 crosspointL(float la, float lb, float lc, float la_div, float radiusx, float radiusy)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.crosspointLshifted(la, lb, lc, la_div, radiusx, radiusy, num, num2);
			}

			public Vector3 crosspointLshifted(float la, float lb, float lc, float la_div, float radiusx, float radiusy, float shiftx, float shifty)
			{
				Vector3 vector;
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					if (lb == 0f)
					{
						return Vector3.zero;
					}
					vector = new Vector3(this.sx - shiftx, -lc - la * (this.sx - shiftx), 1f);
					if (X.BTW(base.y - radiusy, vector.y, base.bottom + radiusy))
					{
						vector.z += 1f;
					}
					break;
				case AIM.T:
				case AIM.B:
					if (la == 0f)
					{
						return Vector3.zero;
					}
					vector = new Vector3((-lb * (this.sy - shifty) - lc) * la_div, this.sy - shifty, 1f);
					if (X.BTW(this.x - radiusx, vector.x, base.right + radiusx))
					{
						vector.z += 1f;
					}
					break;
				default:
					if (lb == 0f)
					{
						vector = new Vector3(-lc, this.slopeBottomY(-lc, shiftx, shifty, true), 1f);
						if (X.BTW(base.y - radiusy, vector.y, base.bottom + radiusy))
						{
							vector.z += 1f;
						}
					}
					else
					{
						if (this.line_a == -la)
						{
							return Vector3.zero;
						}
						float num = this.sy - shifty - (this.sx - shiftx) * this.line_a;
						float num2 = -(num + lc) / (this.line_a + la);
						float num3 = this.line_a * num2 + num;
						vector = new Vector3(num2, num3, 1f);
						if (X.BTW(this.x - radiusx, vector.x, base.right + radiusx))
						{
							vector.z += 1f;
						}
					}
					break;
				}
				return vector;
			}

			public void getNearPointMul(float cx, float cy, float lvl, out float depx, out float depy)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					depx = X.NI(cx, this.x - num, lvl);
					depy = 0f;
					return;
				case AIM.T:
				case AIM.B:
					depy = X.NI(cy, base.y - num2, lvl);
					depx = 0f;
					return;
				default:
				{
					float num3 = this.slopeBottomY(cx, num, num2, true);
					depy = X.NI(cy, num3, lvl);
					depx = 0f;
					return;
				}
				}
			}

			public float canGoToSide(M2FootManager FootD, float ft_shiftx, float ft_shifty, AIM fa, float marginf, float margins, bool refix_position = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.canGoToSideShifted(FootD, ft_shiftx + num, ft_shifty + num2, fa, marginf, margins, refix_position);
			}

			public float canGoToSideShifted(M2FootManager FootD, float shiftx, float shifty, AIM fa, float marginf, float margins, bool refix_position = false)
			{
				int num = CAim._XD(fa, 1);
				int num2 = CAim._YD(fa, 1);
				if (CAim.is_naname(this.aim))
				{
					if ((num != 0) ? (CAim._XD(this.aim, 1) != num) : (CAim._YD(this.aim, 1) != num2))
					{
						return 0f;
					}
				}
				else if (this.aim != fa)
				{
					return 0f;
				}
				AIM aim = this.aim;
				float num4;
				if (aim != AIM.L)
				{
					if (aim != AIM.R)
					{
						float num3 = (CAim.is_naname(this.aim) ? this.slopeBottomY(FootD.Mv.x + (float)CAim._XD(this.aim, 1) * FootD.sizex, shiftx, shifty, true) : (base.y - shifty));
						if (X.isCovering(this.x, base.right, FootD.rgdleft + shiftx, FootD.rgdright + shiftx, margins))
						{
							bool flag;
							if (num2 < 0)
							{
								num4 = num3 - (FootD.rgdbottom + marginf);
								flag = FootD.rgdtop < num3 + marginf && X.BTWS(-FootD.sizey * 2f - marginf, num4, 0f);
							}
							else
							{
								num4 = num3 - (FootD.rgdtop - marginf);
								flag = FootD.rgdbottom > num3 - marginf && X.BTWS(0f, num4, FootD.sizey * 2f + marginf);
							}
							if (flag)
							{
								if (refix_position)
								{
									FootD.Phy.addFoc(FOCTYPE.RESIZE, 0f, X.absMn(num4, 0.2f), -1f, -1, 1, 0, -1, 0);
								}
								return num4;
							}
						}
					}
					else if (X.isCovering(base.y, base.bottom, FootD.rgdtop + shifty, FootD.rgdbottom + shifty, margins) && FootD.rgdleft + shiftx < this.x + marginf && X.BTWS(-FootD.sizex * 2f - marginf, num4 = this.x - (FootD.rgdright + marginf + shiftx), 0f))
					{
						if (refix_position)
						{
							FootD.Phy.addFoc(FOCTYPE.RESIZE, X.absMn(num4, 0.2f), 0f, -1f, -1, 1, 0, -1, 0);
						}
						return num4;
					}
				}
				else if (X.isCovering(base.y, base.bottom, FootD.rgdtop + shifty, FootD.rgdbottom + shifty, margins) && FootD.rgdright + shiftx > this.x - marginf && X.BTWS(0f, num4 = this.x - shiftx - (FootD.rgdleft - marginf), FootD.sizex * 2f + marginf))
				{
					if (refix_position)
					{
						FootD.Phy.addFoc(FOCTYPE.RESIZE, X.absMn(num4, 0.2f), 0f, -1f, -1, 1, 0, -1, 0);
					}
					return num4;
				}
				return 0f;
			}

			public void fixFootStampChip(M2Puts Cp, int cfg = -1)
			{
				this.FootStampChipFix = new M2BlockColliderContainer.BCCPos(Cp, (int)this.sx, (int)this.sy, cfg);
				string text = Cp.getMeta().GetS("foot_type");
				if (text == null)
				{
					text = "";
				}
				this.FootStampChipFix.foot_type = text;
			}

			public M2BlockColliderContainer.BCCPos getFootStampChip(int pivot, float x, float y, float sizex = 0f, float sizey = 0f)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.getFootStampChipShifted(pivot, x, y, sizex, sizey, num, num2);
			}

			private M2BlockColliderContainer.BCCPos getFootStampChipShifted(int pivot, float x, float y, float sizex, float sizey, float shiftx, float shifty)
			{
				if (this.FootStampChipFix.valid)
				{
					return this.FootStampChipFix;
				}
				M2BlockColliderContainer.BCCPos bccpos;
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					this.GetPosAt((int)(x + shiftx + (float)(CAim._XD(this.aim, 1) * pivot) * (sizex + 0.5f)), (int)(y + shifty), out bccpos);
					break;
				case AIM.T:
				case AIM.B:
					this.GetPosAt((int)(x + shiftx), (int)(y + shifty - (float)(pivot * CAim._YD(this.aim, 1)) * (sizey + 0.5f)), out bccpos);
					break;
				default:
				{
					float num = (float)((int)(x + shiftx)) + 0.5f;
					float num2 = this.slopeBottomY(num, 0f, 0f, true);
					this.GetPosAt((int)num, (int)num2, out bccpos);
					break;
				}
				}
				return bccpos;
			}

			public int getConfig(float mapx, float mapy, out M2BlockColliderContainer.BCCPos Cp)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				mapx += num;
				mapy += num2;
				Cp = this.getFootStampChipShifted(0, mapx, mapy, 0f, 0f, 0f, 0f);
				if (!Cp.valid)
				{
					return 4;
				}
				return Cp.cfg;
			}

			public bool hasHardChip(M2Chip Pt)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				for (int i = Pt.rows - 1; i >= 0; i--)
				{
					int num3 = Pt.clms - 1;
					while (i >= 0)
					{
						M2BlockColliderContainer.BCCPos bccpos;
						if (this.GetPosAt((int)((float)(Pt.mapx + num3) + num), (int)((float)(Pt.mapy + i) + num2), out bccpos) && bccpos.Cp == Pt)
						{
							return true;
						}
						i--;
					}
				}
				return false;
			}

			public bool isNear(float cx, float cy, float marginx, float marginy, int aim, bool strict_aim = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.isNearShifted(cx + num, cy + num2, marginx, marginy, aim, strict_aim);
			}

			public bool isNearInside(float cx, float cy, float marginx, float marginy, float inside_len, int aim, bool strict_aim = false)
			{
				float num;
				float num2;
				this.BCC.getBaseShift(out num, out num2);
				return this.isNearInsideShifted(cx + num, cy + num2, marginx, marginy, inside_len, aim, strict_aim);
			}

			public bool isNearShifted(float cx, float cy, float marginx, float marginy, int aim, bool strict_aim = false)
			{
				return (aim < 0 || !(strict_aim ? (this.aim != (AIM)aim) : (!this.isUseableDir(1U << aim)))) && base.isin(cx, cy, marginx, marginy);
			}

			public bool isNearInsideShifted(float cx, float cy, float marginx, float marginy, float inside_len, int aim, bool strict_aim = false)
			{
				if (aim >= 0 && (strict_aim ? (this.aim != (AIM)aim) : (!this.isUseableDir(1U << aim))))
				{
					return false;
				}
				switch (this.aim)
				{
				case AIM.L:
					return X.BTW(base.y - marginy, cy, base.bottom + marginy) && X.BTW(this.x - inside_len - marginx, cx, this.x + marginx);
				case AIM.T:
					return X.BTW(this.x - marginx, cx, base.right + marginx) && X.BTW(base.y - marginy - inside_len, cy, base.y + marginy);
				case AIM.R:
					return X.BTW(base.y - marginy, cy, base.bottom + marginy) && X.BTW(this.x - marginx, cx, this.x + inside_len + marginx);
				case AIM.B:
					return X.BTW(this.x - marginx, cx, base.right + marginx) && X.BTW(base.y - marginy, cy, base.y + marginy + inside_len);
				default:
				{
					if (!X.BTW(this.x - marginx, cx, base.right + marginx))
					{
						return false;
					}
					float num = this.slopeBottomY(cx, 0f, 0f, true);
					if (this.aim == AIM.LT || this.aim == AIM.TR)
					{
						return X.BTW(num - inside_len - marginy, cy, num + marginy);
					}
					return X.BTW(num - marginy, cy, num + inside_len + marginy);
				}
				}
			}

			public int isLinearWalkableTo(M2BlockColliderContainer.BCCLine TBcc, bool can_fall)
			{
				return this.isLinearWalkableTo(TBcc, can_fall ? 30 : 0);
			}

			public int isLinearWalkableTo(M2BlockColliderContainer.BCCLine TBcc, int fallable_len)
			{
				int num = 0;
				if (TBcc == null || this.is_ladder || TBcc.is_ladder || TBcc.BCC != this.BCC)
				{
					return 0;
				}
				int num2 = ((this.is_lift || this.block_index < 0 || this.BCC.AALine[this.block_index] == null) ? 60 : (this.BCC.AALine[this.block_index].Length * 2 + 10));
				for (int i = 0; i < 2; i++)
				{
					M2BlockColliderContainer.BCCLine bccline = this;
					bool flag = true;
					int num3 = 0;
					while (num3++ < num2)
					{
						if (flag)
						{
							flag = false;
						}
						else if (bccline == this)
						{
							break;
						}
						if (bccline == TBcc)
						{
							num |= ((i == 0) ? 1 : 2);
							break;
						}
						M2BlockColliderContainer.BCCLine bccline2 = ((i == 0) ? bccline.SideL : bccline.SideR);
						bool flag2 = false;
						if (bccline2 == null)
						{
							flag2 = true;
						}
						else if (bccline2._xd != 0 && bccline2._yd == 0)
						{
							if (!((i == 0) ? bccline.L_is_270 : bccline.R_is_270))
							{
								break;
							}
							flag2 = true;
						}
						if (flag2)
						{
							if (fallable_len <= 0)
							{
								break;
							}
							float num4 = ((i == 0) ? (bccline.shifted_x - 0.125f) : (bccline.shifted_right + 0.125f));
							float num5 = ((i == 0) ? bccline.shifted_left_y : bccline.shifted_right_y);
							if (TBcc.is_map_bcc)
							{
								bccline2 = this.BCC.Mp.getSideBcc((int)num4, (int)num5, AIM.B);
								if (bccline2 != null)
								{
									bccline = bccline2;
									continue;
								}
							}
							else
							{
								TBcc.BCC.isFallable(num4, num5, 0f, (float)fallable_len, out bccline2, true, true, -1f, null);
								if (bccline2 != null)
								{
									bccline = bccline2;
									continue;
								}
							}
							if (bccline.BCC == TBcc.BCC)
							{
								break;
							}
							if (bccline.is_map_bcc)
							{
								bccline2 = this.BCC.Mp.getSideBcc((int)num4, (int)num5, AIM.B);
								if (bccline2 == null)
								{
									break;
								}
								bccline = bccline2;
							}
							else
							{
								bccline.BCC.isFallable(num4, num5, 0f, (float)fallable_len, out bccline2, true, true, -1f, null);
								if (bccline2 == null)
								{
									break;
								}
								bccline = bccline2;
							}
						}
						else
						{
							bccline = bccline2;
						}
					}
				}
				return num;
			}

			public void getLinearWalkableArea(float fallable_len, out float area_left, out float area_right, out M2BlockColliderContainer.BCCLine LBcc, out M2BlockColliderContainer.BCCLine RBcc, float search_max = 0f)
			{
				this.getLinearWalkableArea(fallable_len, out area_left, out area_right, out LBcc, out RBcc, (search_max <= 0f) ? (-1f) : (this.x - search_max), (search_max <= 0f) ? (-1f) : (base.right + search_max));
			}

			public void getLinearWalkableArea(float fallable_len, out float area_left, out float area_right, out M2BlockColliderContainer.BCCLine LBcc, out M2BlockColliderContainer.BCCLine RBcc, float search_max_left, float search_max_right)
			{
				area_left = this.x;
				area_right = base.right;
				RBcc = this;
				LBcc = this;
				if (this.is_ladder)
				{
					return;
				}
				int num = ((this.is_lift || this.block_index < 0 || this.BCC.AALine[this.block_index] == null) ? 60 : (this.BCC.AALine[this.block_index].Length * 2 + 10));
				for (int i = 0; i < 2; i++)
				{
					M2BlockColliderContainer.BCCLine bccline = this;
					int num2 = (int)bccline.foot_aim;
					if (CAim._XD(num2, 1) == 0)
					{
						float num3 = ((i == 0) ? search_max_left : search_max_right);
						for (int j = 0; j < num; j++)
						{
							M2BlockColliderContainer.BCCLine bccline2 = ((i == 0) ? bccline.SideL : bccline.SideR);
							if (bccline2 == null)
							{
								break;
							}
							int foot_aim = (int)bccline2.foot_aim;
							if (foot_aim != num2)
							{
								if (fallable_len <= 0f || ((i == 0) ? (foot_aim == 0) : (foot_aim == 2)))
								{
									break;
								}
								float num4 = bccline.slopeBottomY((i == 0) ? area_left : area_right);
								float num5 = ((i == 0) ? (area_left - 0.5f) : (area_right + 0.5f));
								bccline.BCC.isFallable(num5, num4, 0f, fallable_len, out bccline2, true, true, -1f, null);
								if (bccline2 == null)
								{
									if (!bccline.is_map_bcc)
									{
										bccline.BCC.Mp.BCC.isFallable(num5, num4, 0f, fallable_len, out bccline2, true, true, -1f, null);
									}
									if (bccline2 == null)
									{
										break;
									}
								}
								float num6 = bccline2.slopeBottomY((i == 0) ? area_left : area_right);
								num2 = (int)bccline2.foot_aim;
								if (num6 - num4 > 0f != (num2 == 3) || X.Abs(num6 - num4) > fallable_len)
								{
									break;
								}
							}
							bccline = bccline2;
							if (i == 0)
							{
								if (bccline.x > area_left)
								{
									break;
								}
								LBcc = bccline;
								area_left = bccline.x;
								if (num3 > 0f && area_left <= num3)
								{
									area_left = num3;
									break;
								}
							}
							else
							{
								if (bccline.right < area_right)
								{
									break;
								}
								RBcc = bccline;
								area_right = bccline.right;
								if (num3 > 0f && area_right >= num3)
								{
									area_right = num3;
									break;
								}
							}
						}
					}
				}
			}

			public bool EachPixel(Func<int, int, M2BlockColliderContainer.BCCLine, bool> Fn, bool check_air, float float_margin = 0f)
			{
				int num = 0;
				float num2;
				float num3;
				this.BCC.getBaseShift(out num2, out num3);
				float num4;
				float num5;
				int num6;
				float num7;
				switch (this.aim)
				{
				case AIM.L:
					num4 = this.sx + (check_air ? 0.2f : (-0.2f));
					num5 = this.sy;
					num6 = (int)this.height;
					num7 = 1f;
					if (float_margin > 0f && X.fracMn(num5 - num3) > float_margin)
					{
						num6++;
					}
					else
					{
						num5 += 0.5f;
					}
					break;
				case AIM.T:
				case AIM.LT:
				case AIM.TR:
					num5 = this.sy + (check_air ? (this.is_naname ? 0.03f : 0.2f) : (this.is_naname ? 0f : (-0.2f)));
					num4 = this.sx;
					num6 = (int)this.width;
					num = -1;
					if (this.is_naname)
					{
						num7 = this.height / this.width * (float)X.MPF(this.aim == AIM.LT);
						num5 += num7 * 0.5f;
					}
					else
					{
						num7 = 0f;
					}
					if (float_margin > 0f && X.fracMn(num4 - num2) > float_margin)
					{
						num6++;
					}
					else
					{
						num4 -= 0.5f;
					}
					break;
				case AIM.R:
					num4 = this.sx - (check_air ? 0.2f : 0f);
					num5 = this.sy;
					num6 = (int)this.height;
					num7 = -1f;
					if (float_margin > 0f && X.fracMn(num5 - num3) > float_margin)
					{
						num6++;
					}
					else
					{
						num5 -= 0.5f;
					}
					break;
				case AIM.B:
				case AIM.BL:
				case AIM.RB:
					num5 = this.sy + (check_air ? (this.is_naname ? (-0.03f) : (-0.2f)) : 0f);
					num4 = this.sx;
					num6 = (int)this.width;
					num = 1;
					if (this.is_naname)
					{
						num7 = this.height / this.width * (float)X.MPF(this.aim == AIM.BL);
						num5 += num7 * 0.5f;
					}
					else
					{
						num7 = 0f;
					}
					if (float_margin > 0f && X.fracMn(num4 - num2) > float_margin)
					{
						num6++;
					}
					else
					{
						num4 += 0.5f;
					}
					break;
				default:
					return false;
				}
				num4 -= num2;
				num5 -= num3;
				int num8 = (int)num4;
				int num9 = (int)num5;
				while (--num6 >= 0)
				{
					if (!Fn(num8, num9, this))
					{
						return false;
					}
					num4 += (float)num;
					num8 += num;
					if (num7 != 0f)
					{
						num5 += num7;
						num9 = (int)num5;
					}
				}
				return true;
			}

			public bool isSameLine(M2BlockColliderContainer.BCCLine B, bool iscontaining = false, bool iscovering = false)
			{
				if (B.aim != this.aim || B.is_lift != this.is_lift)
				{
					return false;
				}
				if (!iscontaining && !iscovering)
				{
					return this.shifted_sx == B.shifted_sx && this.shifted_sy == B.shifted_sy && this.shifted_dx == B.shifted_dx && this.shifted_dy == B.shifted_dy;
				}
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					if (B.shifted_x != this.shifted_x)
					{
						return false;
					}
					if (!iscovering)
					{
						return X.isContaining(this.shifted_y, this.shifted_bottom, B.shifted_y, B.shifted_bottom, 0f);
					}
					return X.isCovering(this.shifted_y, this.shifted_bottom, B.shifted_y, B.shifted_bottom, 0f);
				case AIM.T:
				case AIM.B:
					if (B.shifted_y != this.shifted_y)
					{
						return false;
					}
					if (!iscovering)
					{
						return X.isContaining(this.shifted_x, this.shifted_right, B.shifted_x, B.shifted_right, 0f);
					}
					return X.isCovering(this.shifted_x, this.shifted_right, B.shifted_x, B.shifted_right, 0f);
				default:
				{
					float num = (float)X.IntR(this.line_a * 100f);
					float num2 = (float)X.IntR(B.line_a * 100f);
					if (num != num2)
					{
						return false;
					}
					float num3 = (float)X.IntR(this.slopeBottomY(B.shifted_x, this.BCC.base_shift_x, this.BCC.base_shift_y, true) * 100f);
					float num4 = (float)X.IntR(B.slopeBottomY(B.shifted_x, B.BCC.base_shift_x, B.BCC.base_shift_y, true) * 100f);
					if (num3 != num4)
					{
						return false;
					}
					if (!iscovering)
					{
						return this.isContainingS(B.shifted_x, B.shifted_y, B.shifted_right, B.shifted_bottom, 0f);
					}
					return this.isCoveringXyS(B.shifted_x, B.shifted_y, B.shifted_right, B.shifted_bottom, 0f);
				}
				}
			}

			public bool isSameLine(M2BlockColliderContainer.BCCInfo B, bool iscontaining = false, bool iscovering = false)
			{
				if (B.aim != this.aim || B.is_lift != this.is_lift)
				{
					return false;
				}
				if (!iscontaining && !iscovering)
				{
					return this.shifted_sx == B.sx && this.shifted_sy == B.sy && this.shifted_dx == B.dx && this.shifted_dy == B.dy;
				}
				switch (this.aim)
				{
				case AIM.L:
				case AIM.R:
					if (B.x != this.shifted_x)
					{
						return false;
					}
					if (!iscovering)
					{
						return X.isContaining(this.shifted_y, this.shifted_bottom, B.y, B.bottom, 0f);
					}
					return X.isCovering(this.shifted_y, this.shifted_bottom, B.y, B.bottom, 0f);
				case AIM.T:
				case AIM.B:
					if (B.y != this.shifted_y)
					{
						return false;
					}
					if (!iscovering)
					{
						return X.isContaining(this.shifted_x, this.shifted_right, B.x, B.right, 0f);
					}
					return X.isCovering(this.shifted_x, this.shifted_right, B.x, B.right, 0f);
				default:
				{
					float num = (float)X.IntR(this.line_a * 100f);
					float num2 = (float)X.IntR(B.line_a * 100f);
					if (num != num2)
					{
						return false;
					}
					float num3 = (float)X.IntR(this.slopeBottomY(B.x, this.BCC.base_shift_x, this.BCC.base_shift_y, true) * 100f);
					float num4 = (float)X.IntR(B.slopeBottomY(B.x, this.BCC.base_shift_x, this.BCC.base_shift_y) * 100f);
					if (num3 != num4)
					{
						return false;
					}
					if (!iscovering)
					{
						return this.isContainingS(B.x, B.y, B.right, B.bottom, 0f);
					}
					return this.isCoveringXyS(B.x, B.y, B.right, B.bottom, 0f);
				}
				}
			}

			public readonly M2BlockColliderContainer BCC;

			public int block_index;

			public M2BlockColliderContainer.BCCLine SideL;

			public M2BlockColliderContainer.BCCLine SideR;

			public bool is_lift;

			public bool has_danger_another;

			private bool has_danger_upper;

			public bool upper_area_checked;

			public bool has_arrangeable;

			public bool valid;

			private List<M2BlockColliderContainer.BCCPos> ACp;

			public List<M2MapDamageContainer.M2MapDamageItem> AMapDmg;

			public float sx;

			public float sy;

			public float dx;

			public float dy;

			public List<IBCCEventListener> ALsnRegistered;

			public List<IBCCFootListener> AFootLsnRegistered;

			public AIM aim;

			public float line_a;

			public float line_div;

			private string _tostring;

			public const float foot_alloc_lgt = 0.22f;

			private static DRect RcPosChk = new DRect("");

			private static DRect DangerRc = new DRect("");

			public M2BlockColliderContainer.BCCPos FootStampChipFix;
		}
	}
}
