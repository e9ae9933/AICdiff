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
						M2BlockColliderContainer.BCCLine sameLine = BCCCon.getSameLine(bcc, true, true);
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
			if (type == StainItem.TYPE.FIRE)
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

		private bool StainRunFire(StainItem Stn, float fcnt, bool firstrun, IMapDamageListener Fm)
		{
			DRect mapBounds = Fm.getMapBounds(M2BlockColliderContainer.BufRc);
			if (mapBounds == null)
			{
				return false;
			}
			if (!mapBounds.isCovering(Stn, 0f))
			{
				return mapBounds.isCovering(Stn, 1f);
			}
			M2BlockColliderContainer.BCCLine bccFor = Stn.GetBccFor(Fm);
			if (bccFor == null)
			{
				return false;
			}
			this.Dmg_Fire.Set(Stn);
			Fm.applyMapDamage(this.Dmg_Fire, bccFor);
			return true;
		}

		private bool drawStain(EffectItem Ef, M2DrawBinder Ed)
		{
			Map2d curMap = this.M2D.curMap;
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			StainItem.TYPE type = StainItem.TYPE._MAX;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				StainItem stainItem = this.AItems[i];
				uint ran = X.GETRAN2(stainItem.id + 44U, stainItem.id * 3U);
				float num = curMap.floort + X.NI(30, 90, X.RAN(ran, 1826));
				if (stainItem.type == StainItem.TYPE.FIRE)
				{
					if (type != stainItem.type)
					{
						if (meshDrawer == null)
						{
							meshDrawer = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
						}
						if (meshDrawer2 == null)
						{
							meshDrawer2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
						}
						meshDrawer2.initForImg(MTRX.EffBlurCircle245, 0);
					}
					float num2 = 0.5f + 0.5f * X.ZLINE(stainItem.t, 40f) * X.ZLINE(stainItem.maxt - stainItem.t, 70f);
					meshDrawer2.Col = meshDrawer2.ColGrd.Set(4285321464U).blend(4282216906U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num2)
						.C;
					Color32 c = meshDrawer.ColGrd.White().mulA(num2).C;
					meshDrawer.ColGrd.Set(4291384380U).blend(4290362895U, 0.5f + 0.5f * X.COSI(num + 444f, 48f)).mulA(num2);
					bool flag;
					Vector2 vector = Ef.EF.calcMeshXY(stainItem.centerx, stainItem.centery, null, out flag);
					float num3 = 44f + X.NI(11, 14, X.RAN(ran, 2073)) * X.COSI(num, 9.35f) + X.NI(4, 13, X.RAN(ran, 606)) * X.COSI(num, 13.14f);
					meshDrawer.base_x = (meshDrawer2.base_x = vector.x);
					meshDrawer.base_y = (meshDrawer2.base_y = vector.y);
					float num4 = 33f + X.NI(6, 9, X.RAN(ran, 770)) * X.COSI(num + 33f, 11.55f) + X.NI(6, 7, X.RAN(ran, 2103)) * X.COSI(num + 90f, 15.72f);
					for (int j = 0; j < 4; j++)
					{
						if (((ulong)stainItem.footable_bits & (ulong)(1L << (j & 31))) != 0UL)
						{
							meshDrawer.Rotate(CAim.get_agR((AIM)j, 0f) + 1.5707964f, false);
							meshDrawer2.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
							float num5 = (float)CAim._XD(j, 1) * stainItem.Afoot_len[j] * curMap.CLENB;
							float num6 = (float)(-(float)CAim._YD(j, 1)) * stainItem.Afoot_len[j] * curMap.CLENB;
							meshDrawer2.Rect(num5, num6, num3 * 2f, num3 * 2f, false);
							meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
							meshDrawer.Col = meshDrawer.ColGrd.C;
							meshDrawer.Rect(num5, num6, num4 * 2f, num4 * 2f, false);
							PxlFrame frame = MTR.SqEfStainBurning.getFrame(X.ANM((int)num, 11, X.NI(2f, 4.2f, X.RAN(ran, 1719))));
							meshDrawer.Col = c;
							meshDrawer.RotaPF(num5, num6, curMap.base_scale, curMap.base_scale, 0f, frame, (ran & 1U) == 0U, false, false, uint.MaxValue, false, 0);
							meshDrawer.Identity();
							meshDrawer2.Identity();
						}
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

		public readonly NelM2DBase M2D;

		private readonly StainItem.FnStainRun FD_FnStainRunFire;

		private M2MapDamageContainer.M2MapDamageItem Dmg_Fire;

		private uint id_count;

		private M2DrawBinder Ed;

		private float t_refine_mover;

		private M2DrawBinder.FnEffectBind FD_drawStain;
	}
}
