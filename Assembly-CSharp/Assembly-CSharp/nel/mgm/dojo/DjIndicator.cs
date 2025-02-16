using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjIndicator
	{
		public DjIndicator()
		{
			this.Alevel_l = new float[14];
			this.Alevel_r = new float[14];
			this.Alevel_buf = new float[14];
		}

		private void prepareDummyData()
		{
			float dummyIndicatorData = BGM.getDummyIndicatorData();
			float num = 0.07692308f;
			for (int i = 13; i >= 0; i--)
			{
				float num2 = (float)i * num;
				this.Alevel_buf[i] = dummyIndicatorData * X.NI(0.5f, 1f, X.Scr(X.ZSIN(X.Abs(num2 - 0.3f), 0.3f), X.ZSIN(X.Abs(num2 - 0.6f), 0.6f)));
			}
		}

		public void deattachSound()
		{
			if (this.Analyzer != null)
			{
				this.Analyzer.DetachExPlayer();
				this.Analyzer.Dispose();
				this.Analyzer = null;
			}
		}

		public void destruct()
		{
			this.deattachSound();
		}

		public void prepareMaterial(MgmDojo DJ)
		{
			if (this.Sq == null)
			{
				this.Sq = MTRX.PxlIcon.getPoseByName("dojo_indicator").getSequence(0);
			}
		}

		public void prepareSound(MgmDojo DJ, string cue_key)
		{
			this.deattachSound();
			if (X.DEBUGNOSND)
			{
				return;
			}
			this.Analyzer = new CriAtomExOutputAnalyzer(new CriAtomExOutputAnalyzer.Config
			{
				enablePcmCapture = true,
				numCapturedPcmSamples = 14
			});
			CriAtomExPlayer nextSoundPlayerInstance = BGM.getNextSoundPlayerInstance(true);
			if (nextSoundPlayerInstance == null || !this.Analyzer.AttachExPlayer(nextSoundPlayerInstance))
			{
				X.dl("アナライザーのアタッチに失敗", null, false, false);
			}
		}

		public bool run(bool bgm_active)
		{
			if (bgm_active && (this.Analyzer != null || X.DEBUGNOSND))
			{
				float num = ((SND.bgm_volume01 != 0f) ? (1f / SND.bgm_volume01) : 1f);
				if (this.Analyzer == null)
				{
					this.prepareDummyData();
				}
				for (int i = 0; i < 2; i++)
				{
					float[] array = ((i == 0) ? this.Alevel_l : this.Alevel_r);
					if (this.Analyzer != null)
					{
						this.Analyzer.GetPcmData(ref this.Alevel_buf, i);
					}
					for (int j = 0; j < 14; j++)
					{
						float num2 = X.Abs(X.ZSIN(this.Alevel_buf[j] * num));
						float num3 = array[j];
						if (num3 < num2)
						{
							array[j] = X.MULWALK(num3, num2, 0.1f);
						}
						else
						{
							array[j] = X.VALWALK(num3, num2, 0.012f);
						}
					}
				}
				return true;
			}
			bool flag = false;
			for (int k = 0; k < 14; k++)
			{
				float num4 = this.Alevel_l[k];
				if (num4 != 0f)
				{
					this.Alevel_l[k] = X.VALWALK(num4, 0f, 0.033f);
					flag = true;
				}
				num4 = this.Alevel_r[k];
				if (num4 != 0f)
				{
					this.Alevel_r[k] = X.VALWALK(num4, 0f, 0.033f);
					flag = true;
				}
			}
			return flag;
		}

		public void drawTo(MeshDrawer Md, float alpha)
		{
			if (this.Sq == null)
			{
				return;
			}
			Color32 color = C32.MulA(uint.MaxValue, alpha);
			for (int i = 0; i < 14; i++)
			{
				float num = (0.5f + (float)i) * this.indv;
				for (int j = 0; j < 2; j++)
				{
					float num2 = ((j == 0) ? this.Alevel_r[i] : this.Alevel_l[i]);
					float num3 = this.max_h * num2;
					float num4 = num3 * 0.5f;
					Color32 color2 = C32.MulA(uint.MaxValue, alpha * X.NIL(0.125f, 0.33f, num2, 0.25f));
					Color32 color3 = C32.MulA(uint.MaxValue, alpha * X.NIL(0f, 1f, num2, 0.75f));
					this.RotaPF(Md, num, 40f - num4, color, color2, color3, this.Sq.getFrame(0), 1f);
					this.RotaPF(Md, num, 40f - num4, color, color2, color3, this.Sq.getFrame(1), num3);
					this.RotaPF(Md, num, 40f + num4, color, color2, color3, this.Sq.getFrame(2), 1f);
					num = -num;
				}
			}
		}

		private void RotaPF(MeshDrawer Md, float dx, float dy, Color32 C, Color32 CBod, Color32 CBlurW, PxlFrame PF, float scaley = 1f)
		{
			Md.Col = C;
			Md.RotaPF(dx, dy, 1f, scaley, 0f, PF, false, false, false, 3U, false, 0);
			Md.Col = CBlurW;
			Md.RotaPF(dx, dy, 1f, scaley, 0f, PF, false, false, false, 4U, false, 0);
			Md.Col = CBod;
			Md.RotaPF(dx, dy, 1f, scaley, 0f, PF, false, false, false, 8U, false, 0);
		}

		private const int resolution = 14;

		private readonly float indv = (IN.wh - 10f) / 14f;

		private readonly float max_h = IN.h * 0.67f;

		private PxlSequence Sq;

		private CriAtomExOutputAnalyzer Analyzer;

		private float[] Alevel_l;

		private float[] Alevel_r;

		private float[] Alevel_buf;
	}
}
