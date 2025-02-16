using System;
using XX;

namespace nel
{
	public class MgNWebShot : MgFDHolder
	{
		public static MagicItem addWebShot(NelM2DBase nM2D, M2MagicCaster Caster, MGHIT mg_hit, float x, float y, float vx, float vy, float maxt, float gravity_lock = 0f)
		{
			return MgNCandleShot.addCandleShot(MGKIND.WEB_SHOT, nM2D, Caster, mg_hit, x, y, vx, vy, maxt, gravity_lock);
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			bool flag = Mg.phase == 0;
			int num = MgNCandleShot.runCandleShot(Mg, fcnt, true, "webshot_appear", "fox_candleshot_erased", "webshot_ground", true);
			if (num == -1)
			{
				Mg.M2D.STAIN.Set(Mg.Dro.x, Mg.Dro.y + Mg.Dro.size + 0.25f, StainItem.TYPE.WEB, AIM.B, Mg.da, Mg.Dro.get_FootBcc());
			}
			if (num == 0)
			{
				Mg.kill(0.125f);
				return false;
			}
			if (flag && num > 0 && Mg.Dro != null)
			{
				Mg.Dro.gravity_scale = 0.66f;
			}
			return num > 0;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 10)
			{
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
				meshImg.Col = meshImg.ColGrd.Set(4282014922U).blend(4284569940U, 0.5f + X.COSI(Mg.t, 43f) * 0.5f).C;
				meshImg.initForImg(MTRX.EffBlurCircle245, 0);
				float num = X.NI(40, 80, 0.5f + X.COSI(Mg.t, 11.3f) * 0.4f + X.COSI(Mg.t, 3.72f) * 0.1f);
				meshImg.Rect(0f, 0f, num, num, false);
				MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
				mesh.ColGrd.Set(4282992969U).blend(4290033849U, 0.5f + X.COSI(Mg.t + 22f, 83f) * 0.5f);
				if (X.COSI(Mg.t, 13.3f) >= 0.5f)
				{
					mesh.ColGrd.blend(4294901760U, 0.875f);
				}
				mesh.Col = mesh.ColGrd.C;
				for (int i = 0; i < 2; i++)
				{
					mesh.Pos(0f, 0f, null);
					for (int j = 0; j < 8; j++)
					{
						mesh.Tri(-1, j, (j + 1) % 8, false);
					}
					float num2 = (float)X.MPF(i == 0) * (Mg.t + 15f) / 55f * 3.1415927f;
					for (int k = 0; k < 8; k++)
					{
						uint ran = X.GETRAN2(Mg.id + k * 13 + i * 7, k * 6 + i * 5);
						float num3 = 1f + X.COSI(Mg.t + X.RAN(ran, 2575) * 30f, 70f + X.RAN(ran, 2558) * 75f);
						if (k % 2 == 1)
						{
							num3 = ((num3 - 1f) * 0.25f + 1f) * 0.70710677f;
						}
						else if (num3 < 1f)
						{
							num3 = X.Scr(num3, 0.5f);
							num3 = X.Scr(num3, num3);
						}
						else
						{
							num3 -= 1f;
							num3 = X.Scr(num3, num3);
							num3 = X.Scr(num3, num3);
							num3 = num3 * 0.3f + 1f;
						}
						float num4 = num3 * 18f * 0.015625f;
						mesh.Pos(X.Cos(num2) * num4, X.Sin(num2) * num4, null);
						num2 -= 0.7853982f;
					}
				}
			}
			return true;
		}

		public const int RUN_RSLT_PROGRESS = 1;

		public const int RUN_RSLT_FOOTED = -1;

		public const int RUN_RSLT_KILL = 0;
	}
}
