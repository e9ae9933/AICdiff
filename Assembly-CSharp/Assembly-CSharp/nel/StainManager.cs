using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class StainManager : RBase<StainItem>, IRunAndDestroy
	{
		public override StainItem Create()
		{
			return new StainItem();
		}

		public StainManager(NelM2DBase _M2D)
			: base(4, false, false, true)
		{
			this.M2D = _M2D;
			this.FD_FnStainRunFire = new StainItem.FnStainRun(this.StainRunFire);
			this.FD_drawStain = new M2DrawBinder.FnEffectBind(this.drawStain);
		}

		public override void clear()
		{
			base.clear();
			this.Dmg_Fire = this.M2D.MDMGCon.Release(this.Dmg_Fire);
			this.t_refine_mover = 0f;
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
		}

		public void recheckWholeBcc(M2BlockColliderContainer BCCCon)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				StainItem stainItem = this.AItems[i];
				for (int j = 0; j < 4; j++)
				{
					M2BlockColliderContainer.BCCLine bcc = stainItem.GetBcc(j);
					if (bcc != null && bcc.BCC == BCCCon)
					{
						M2BlockColliderContainer.BCCLine sameLine = BCCCon.getSameLine(stainItem.GetBccInfo(j), true, true);
						if (sameLine != null)
						{
							stainItem.SetBcc(j, sameLine, false);
						}
					}
				}
			}
		}

		public StainItem Set(float mapx, float mapy, StainItem.TYPE type, AIM a, float maxt, M2BlockColliderContainer.BCCLine Bcc = null)
		{
			return this.Set(mapx, mapy, type, 1U << (int)a, maxt, Bcc);
		}

		public StainItem Set(float mapx, float mapy, StainItem.TYPE type, uint footable_bits, float maxt, M2BlockColliderContainer.BCCLine Bcc = null)
		{
			Map2d curMap = this.M2D.curMap;
			if (curMap.BCC == null)
			{
				return null;
			}
			StainItem stainItem = base.Pop(32).Set(curMap, mapx, mapy, type, footable_bits, maxt, Bcc);
			if (stainItem == null)
			{
				this.LEN = X.Mx(this.LEN - 1, 0);
				return null;
			}
			if (this.LEN == 1)
			{
				curMap.addRunnerObject(this);
			}
			if (this.t_refine_mover == 0f)
			{
				this.t_refine_mover = 6f;
			}
			if (type != StainItem.TYPE.FIRE)
			{
				if (type == StainItem.TYPE.ICE)
				{
					if (this.EfpIceSmoke == null)
					{
						this.EfpIceSmoke = new EfParticleOnce("stain_ice_floor_smoke", EFCON_TYPE.NORMAL);
					}
				}
			}
			else
			{
				if (this.Dmg_Fire == null)
				{
					this.Dmg_Fire = this.M2D.MDMGCon.Create(MAPDMG.FIRE, 0f, 0f, 1f, 1f, null);
				}
				stainItem.FnRun = this.FD_FnStainRunFire;
			}
			if (this.Ed == null)
			{
				this.Ed = curMap.setED("Stain", this.FD_drawStain, 0f);
			}
			StainItem stainItem2 = stainItem;
			uint num = this.id_count + 1U;
			this.id_count = num;
			stainItem2.id = num;
			return stainItem;
		}

		public override bool run(float fcnt)
		{
			if (this.t_refine_mover > 0f)
			{
				this.t_refine_mover -= fcnt;
				if (this.t_refine_mover <= 0f)
				{
					this.t_refine_mover = 0f;
					this.M2D.curMap.recheckBCCCurrentPos(null);
				}
			}
			return base.run(fcnt);
		}

		private bool StainRunFire(StainItem Stn, M2BlockColliderContainer.BCCLine Bcc, ref StainItem.FootT Fd, float fcnt, bool firstrun, IMapDamageListener Fm)
		{
			this.Dmg_Fire.Set(Stn);
			if (EnemySummoner.isActiveBorder())
			{
				Fm.applyMapDamage(this.Dmg_Fire, Bcc);
			}
			return true;
		}

		private bool drawStain(EffectItem Ef, M2DrawBinder Ed)
		{
			Map2d curMap = this.M2D.curMap;
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			MeshDrawer meshDrawer3 = null;
			StainItem.TYPE type = StainItem.TYPE._MAX;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				StainItem stainItem = this.AItems[i];
				uint ran = X.GETRAN2(stainItem.id + 44U, stainItem.id * 3U);
				float num = curMap.floort + X.NI(30, 90, X.RAN(ran, 1826));
				switch (stainItem.type)
				{
				case StainItem.TYPE.FIRE:
				{
					if (type != stainItem.type)
					{
						if (meshDrawer == null)
						{
							meshDrawer = Ef.GetMeshImg("_stain", MTRX.MIicon, BLEND.ADD, false);
						}
						if (meshDrawer3 == null)
						{
							bool flag;
							meshDrawer3 = Ef.EF.MeshInitImg("_stain", 0f, 0f, MTRX.MIicon, out flag, BLEND.SUB, false);
							if (flag)
							{
								meshDrawer3.base_z += 0.1f;
							}
						}
						meshDrawer3.initForImg(MTRX.EffBlurCircle245, 0);
					}
					float num2 = 0.5f + 0.5f * X.ZLINE(stainItem.t, 40f) * X.ZLINE(stainItem.maxt - stainItem.t, 70f);
					meshDrawer3.Col = meshDrawer3.ColGrd.Set(4285321464U).blend(4282216906U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num2)
						.C;
					Color32 c = meshDrawer.ColGrd.White().mulA(num2).C;
					meshDrawer.ColGrd.Set(4291384380U).blend(4290362895U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num2);
					bool flag2;
					Vector2 vector = Ef.EF.calcMeshXY(stainItem.centerx, stainItem.centery, null, out flag2);
					float num3 = 44f + X.NI(11, 14, X.RAN(ran, 2073)) * X.COSI(num, 9.35f) + X.NI(4, 13, X.RAN(ran, 606)) * X.COSI(num, 13.14f);
					meshDrawer.base_x = (meshDrawer3.base_x = vector.x);
					meshDrawer.base_y = (meshDrawer3.base_y = vector.y);
					float num4 = 33f + X.NI(6, 9, X.RAN(ran, 770)) * X.COSI(num + 33f, 11.55f) + X.NI(6, 7, X.RAN(ran, 2103)) * X.COSI(num + 90f, 15.72f);
					for (int j = 0; j < 4; j++)
					{
						if (((ulong)stainItem.footable_bits & (ulong)(1L << (j & 31))) != 0UL)
						{
							meshDrawer.Identity();
							meshDrawer.Rotate(CAim.get_agR((AIM)j, 0f) + 1.5707964f, false);
							meshDrawer3.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
							float num5 = (float)CAim._XD(j, 1) * stainItem.Afoot_len[j] * curMap.CLENB;
							float num6 = (float)(-(float)CAim._YD(j, 1)) * stainItem.Afoot_len[j] * curMap.CLENB;
							meshDrawer3.Rect(num5, num6, num3 * 2f, num3 * 2f, false);
							meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
							meshDrawer.Col = meshDrawer.ColGrd.C;
							meshDrawer.Rect(num5, num6, num4 * 2f, num4 * 2f, false);
							PxlFrame frame = MTR.SqEfStainBurning.getFrame(X.ANM((int)num, 11, X.NI(2f, 4.2f, X.RAN(ran, 1719))));
							meshDrawer.Col = c;
							meshDrawer.RotaPF(num5, num6, curMap.base_scale, curMap.base_scale, 0f, frame, (ran & 1U) == 0U, false, false, uint.MaxValue, false, 0);
							meshDrawer.Identity();
							meshDrawer3.Identity();
						}
					}
					break;
				}
				case StainItem.TYPE.ICE:
				{
					if (type != stainItem.type)
					{
						if (meshDrawer == null)
						{
							meshDrawer = Ef.GetMeshImg("_stain", MTRX.MIicon, BLEND.ADD, false);
						}
						if (meshDrawer3 == null)
						{
							bool flag3;
							meshDrawer3 = Ef.EF.MeshInitImg("_stain", 0f, 0f, MTRX.MIicon, out flag3, BLEND.SUB, false);
							if (flag3)
							{
								meshDrawer3.base_z += 0.1f;
							}
						}
						meshDrawer3.initForImg(MTRX.EffBlurCircle245, 0);
					}
					bool flag4;
					Vector2 vector2 = Ef.EF.calcMeshXY(stainItem.centerx, stainItem.centery, null, out flag4);
					meshDrawer.base_x = (meshDrawer3.base_x = vector2.x);
					meshDrawer.base_y = (meshDrawer3.base_y = vector2.y);
					for (int k = 0; k < 4; k++)
					{
						if (((ulong)stainItem.footable_bits & (ulong)(1L << (k & 31))) != 0UL)
						{
							meshDrawer.Identity();
							float num7 = (float)CAim._XD(k, 1) * stainItem.Afoot_len[k] * curMap.CLENB;
							float num8 = (float)CAim._YD(k, 1) * stainItem.Afoot_len[k] * curMap.CLENB;
							float num9 = CAim.get_agR((AIM)k, 0f) + 1.5707964f;
							this.EfpIceSmoke.drawTo(meshDrawer, num7 + meshDrawer.base_px_x, num8 + meshDrawer.base_px_y, num9, 0, num, this.EfpIceSmoke.loop_time);
							meshDrawer.Rotate(num9, false);
							PxlFrame frame2 = MTR.SqEfStainIce.getFrame((int)((ran & 255U) % (uint)MTR.SqEfStainIce.Length));
							meshDrawer.Col = meshDrawer.ColGrd.Set(4285501842U).blend(4281819291U, 0.125f * X.COSI(num + 43f, 11.3f + X.RAN(ran, 734) * 4.2f) + ((((int)num & 15) >= 8) ? 0.875f : 0f)).C;
							meshDrawer.RotaPF(num7, num8, curMap.base_scale, curMap.base_scale, 0f, frame2, X.RAN(ran, 2684) < 0.5f, false, false, uint.MaxValue, false, 0);
							meshDrawer3.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
							for (int l = 0; l < 9; l++)
							{
								uint ran2 = X.GETRAN2((int)((ran & 127U) + (uint)(l * 5)), 14 + l * 13 + k * 7);
								float num10 = num - (float)l * 20f;
								num10 = (num10 + 180f) % 180f;
								float num11 = X.ZLINE(num10, 40f) * X.ZLINE(90f - num10, 30f);
								if (num11 > 0f)
								{
									float num12 = X.ZLINE(num10, 90f);
									MeshDrawer meshDrawer4;
									if (stainItem.t < 8f)
									{
										meshDrawer4 = meshDrawer;
										meshDrawer4.Col = meshDrawer4.ColGrd.Set(4288740607U).mulA(num11).C;
									}
									else
									{
										meshDrawer4 = meshDrawer3;
										meshDrawer4.Col = meshDrawer4.ColGrd.Set(4292401197U).mulA(num11 * 0.65f).C;
									}
									float num13 = X.RANSH(ran2, 614) * curMap.CLENB;
									float num14 = -X.RAN(ran2, 8815) * curMap.CLENB * 0.3f - 6f - X.NI(9, 16, X.RAN(ran2, 1376)) * num12;
									float num15 = X.NI(1f, 1.5f, X.RAN(ran2, 1253));
									PxlFrame pxlFrame = MTR.SqEfStainIcePtc[(long)((ulong)ran2 % (ulong)((long)MTR.SqEfStainIcePtc.Length))];
									meshDrawer4.RotaPF(num7 + num13, num8 + num14, num15, num15, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
								}
							}
						}
					}
					break;
				}
				case StainItem.TYPE.WEB:
				{
					if (type != stainItem.type)
					{
						if (meshDrawer2 == null)
						{
							meshDrawer2 = Ef.GetMeshImg("_stain", MTRX.MIicon, BLEND.NORMAL, false);
						}
						if (meshDrawer3 == null)
						{
							bool flag5;
							meshDrawer3 = Ef.EF.MeshInitImg("_stain", 0f, 0f, MTRX.MIicon, out flag5, BLEND.SUB, false);
							if (flag5)
							{
								meshDrawer3.base_z += 0.1f;
							}
						}
						meshDrawer3.initForImg(MTRX.EffBlurCircle245, 0);
					}
					float num16 = 0.5f + 0.5f * X.ZLINE(stainItem.t, 40f) * X.ZLINE(stainItem.maxt - stainItem.t, 70f);
					meshDrawer3.Col = meshDrawer3.ColGrd.Set(1149662056).blend(1145191739U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num16)
						.C;
					Color32 c2 = meshDrawer2.ColGrd.White().mulA(num16).C;
					meshDrawer2.ColGrd.Set(uint.MaxValue).blend(4290758599U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num16);
					bool flag6;
					Vector2 vector3 = Ef.EF.calcMeshXY(stainItem.centerx, stainItem.centery, null, out flag6);
					float num17 = 44f + X.NI(3, 8, X.RAN(ran, 2073)) * X.COSI(num, 14.35f) + X.NI(4, 9, X.RAN(ran, 606)) * X.COSI(num, 23.14f);
					meshDrawer2.base_x = (meshDrawer3.base_x = vector3.x);
					meshDrawer2.base_y = (meshDrawer3.base_y = vector3.y);
					X.NI(6, 9, X.RAN(ran, 770));
					X.COSI(num + 33f, 11.55f);
					X.NI(6, 7, X.RAN(ran, 2103));
					X.COSI(num + 90f, 15.72f);
					for (int m = 0; m < 4; m++)
					{
						if (((ulong)stainItem.footable_bits & (ulong)(1L << (m & 31))) != 0UL)
						{
							uint ran3 = X.GETRAN2((int)((ran & 127U) + (uint)(m * 5)), 14 + m * 13);
							meshDrawer2.Identity();
							meshDrawer2.Rotate(CAim.get_agR((AIM)m, 0f) + 1.5707964f, false);
							float num18 = (float)CAim._XD(m, 1) * stainItem.Afoot_len[m] * curMap.CLENB;
							float num19 = (float)(-(float)CAim._YD(m, 1)) * stainItem.Afoot_len[m] * curMap.CLENB;
							PxlFrame frame3 = MTR.SqEfStainWebFloor.getFrame((int)(ran3 % (uint)MTR.SqEfStainWebFloor.Length));
							meshDrawer3.setCurrentMatrix(meshDrawer2.getCurrentMatrix(), false);
							meshDrawer3.Rect(num18, num19, num17 * 2f, num17 * 2f, false);
							meshDrawer2.Col = c2;
							meshDrawer2.RotaPF(num18, num19, curMap.base_scale, curMap.base_scale, 0f, frame3, X.RAN(ran3, 1874) < 0.5f, false, false, uint.MaxValue, false, 0);
							meshDrawer2.Identity();
						}
					}
					break;
				}
				}
			}
			if (this.LEN == 0)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return "StainManager";
		}

		public static bool isConflictStainType(StainItem.TYPE ta, StainItem.TYPE tb)
		{
			if (ta == tb)
			{
				return false;
			}
			if (ta > tb)
			{
				return StainManager.isConflictStainType(tb, ta);
			}
			return ta == StainItem.TYPE.FIRE && tb == StainItem.TYPE.ICE;
		}

		public static string stain_foottype_overwrite(StainItem.TYPE type)
		{
			string text;
			if (type != StainItem.TYPE.ICE)
			{
				if (type != StainItem.TYPE.WEB)
				{
					text = null;
				}
				else
				{
					text = "web";
				}
			}
			else
			{
				text = "ice";
			}
			return text;
		}

		public readonly NelM2DBase M2D;

		private readonly StainItem.FnStainRun FD_FnStainRunFire;

		private M2MapDamageContainer.M2MapDamageItem Dmg_Fire;

		private EfParticleOnce EfpIceSmoke;

		private uint id_count;

		private M2DrawBinder Ed;

		private float t_refine_mover;

		private M2DrawBinder.FnEffectBind FD_drawStain;
	}
}
