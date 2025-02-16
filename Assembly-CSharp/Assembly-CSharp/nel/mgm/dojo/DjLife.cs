using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjLife
	{
		public DjLife(DjRPC _Rpc, Func<float> _FnTZ)
		{
			this.Rpc = _Rpc;
			this.FnTZ = _FnTZ;
		}

		public void reset(int _max, int _max_crack)
		{
			this.life = _max;
			this.max = _max;
			this.max_crack = _max_crack;
			this.alpha_c = 0f;
			this.crack = 0;
			this.t_cure = 0f;
			this.t_vib = -1f;
		}

		internal void run(float fcnt, bool crack_showable)
		{
			float num = (float)(crack_showable ? 1 : 0);
			if (num != this.alpha_c)
			{
				this.alpha_c = X.VALWALK(this.alpha_c, num, fcnt * 0.044f);
			}
			if (this.t_vib >= 0f)
			{
				this.t_vib += fcnt * X.Mx(0.001f, this.GM.DJ.bpm_r);
			}
			if (this.t_cure > 0f)
			{
				this.t_cure += fcnt;
				this.finishCureAnim(false);
			}
			if (this.t_cure < 0f)
			{
				this.t_cure = X.VALWALK(this.t_cure, 0f, fcnt);
			}
		}

		public void finishVibAnim(bool immediate = false)
		{
			if (this.t_vib >= 1f || (this.t_vib >= 0f && (immediate || this.GM.DJ.bpm_r <= 0f)))
			{
				this.GM.PtcVar("cx", this.tale_life_x_u).PtcVar("cy", this.drawy_u).PtcST("dojo_break_crack", null);
				this.life--;
				this.t_vib = -1f;
				this.crack = 0;
				this.alpha_c = 0f;
				if (this.life <= 0)
				{
					this.Rpc.fine_tx_alpha = true;
				}
			}
		}

		private void finishCureAnim(bool immediate = false)
		{
			if (this.t_cure >= 60f || (this.t_cure > 0f && immediate))
			{
				this.GM.PtcVar("cx", this.tale_life_x_u).PtcVar("cy", this.drawy_u).PtcST("dojo_cure1", null);
				this.t_cure = 0f;
				this.crack = 0;
				this.alpha_c = 0f;
			}
		}

		internal void breakOne(bool use_vib_anim = true)
		{
			this.finishCureAnim(true);
			if (this.t_cure < 0f)
			{
				this.t_cure = 0f;
			}
			if (use_vib_anim)
			{
				this.t_vib = 0f;
				return;
			}
			this.life--;
			this.alpha_c = 0f;
			this.t_vib = -1f;
			this.crack = 0;
		}

		public bool addCrack(bool reach_to_max = false)
		{
			this.finishCureAnim(true);
			if (this.max_crack == 0 || this.crack >= this.max_crack)
			{
				return false;
			}
			this.crack = (reach_to_max ? this.max_crack : X.Mn(this.crack + 1, this.max_crack));
			this.GM.PtcVar("cx", this.tale_life_x_u).PtcVar("cy", this.drawy_u).PtcVarS("pr", this.is_pr ? "pr" : "en")
				.PtcST("dojo_addc_crack", null);
			if (this.crack >= this.max_crack)
			{
				this.breakOne(true);
				return true;
			}
			this.alpha_c = 0.5f;
			this.t_cure = -60f;
			return false;
		}

		public void cureCrack(bool use_cure_anim = true)
		{
			this.finishCureAnim(true);
			this.finishVibAnim(true);
			if (this.crack == 0)
			{
				return;
			}
			if (use_cure_anim)
			{
				this.GM.PtcVar("cx", this.tale_life_x_u).PtcVar("cy", this.drawy_u).PtcVar("maxt", 60f)
					.PtcST("dojo_cure0", null);
				this.t_cure = 0.0001f;
				return;
			}
			this.alpha_c = 0f;
			this.t_cure = 0f;
			this.crack = 0;
		}

		internal void drawTo(EffectItem E)
		{
			MeshDrawer meshImg = E.GetMeshImg("_life", MTRX.MIicon, BLEND.NORMAL, false);
			if (!this.is_alive_c)
			{
				meshImg.base_z -= 0.45f;
			}
			else
			{
				meshImg.base_z += 0.05f;
			}
			meshImg.base_x = (this.drawx + this.left_shift_dx) * 0.015625f;
			meshImg.base_y = this.drawy * 0.015625f;
			float num = this.FnTZ();
			Color32 c = meshImg.ColGrd.White().mulA(X.ZLINE(num, 0.25f)).C;
			int num2 = this.life - 1;
			float num3 = 0f;
			for (int i = 0; i < this.max; i++)
			{
				uint num4 = uint.MaxValue;
				float num5 = (float)i * 56f * (float)X.MPF(this.is_pr);
				meshImg.Col = meshImg.ColGrd.C;
				float num6 = 0f;
				PxlFrame pxlFrame;
				if (i > num2)
				{
					pxlFrame = this.GM.SqLife.getFrame(2);
				}
				else
				{
					num3 = num5;
					pxlFrame = this.GM.SqLife.getFrame(this.is_pr ? 0 : 1);
					num4 = 1U;
					if (this.max_crack > 0 && i == num2)
					{
						int num7 = X.Mn(8, (int)(8f * (float)this.crack / (float)this.max_crack));
						for (int j = 0; j < num7; j++)
						{
							num4 |= 1U << j + 1;
						}
						if (this.t_vib >= 0f || this.t_cure < 0f)
						{
							float num8 = ((this.t_vib >= 0.5f) ? 2.4f : 1.5f);
							num5 += X.COSIT(5.7f) * num8;
							num6 += X.COSIT(6.9f) * num8;
							meshImg.Col = meshImg.ColGrd.blend(4285690482U, 0.6f + 0.14f * X.COSIT(4.53f) + 0.24f * X.COSIT(9.68f)).C;
							meshImg.ColGrd.Set(c);
						}
					}
				}
				meshImg.RotaPF(num5, num6, 2f, 2f, 0f, pxlFrame, false, false, false, num4, false, 0);
			}
			if (this.alpha_c > 0f && this.max_crack > 0 && num2 >= 0)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add(this.max_crack - this.crack);
					meshImg.Col = meshImg.ColGrd.blend(c, 0.5f).mulA(this.alpha_c).C;
					MTRX.ChrL.DrawScaleStringTo(meshImg, stb, num3, 0f, 2f, 2f, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
				}
			}
		}

		public bool isActive()
		{
			return this.Rpc.isActive();
		}

		public bool is_alive
		{
			get
			{
				return this.life > 0;
			}
		}

		public bool is_alive_c
		{
			get
			{
				return this.get_life(true) > 0;
			}
		}

		public float swidth
		{
			get
			{
				return (float)this.max * 56f;
			}
		}

		public float drawx
		{
			get
			{
				return (float)X.MPF(this.is_en) * X.NI(IN.wh + this.swidth * 0.5f + 30f, IN.wh - 35f - this.swidth * 0.5f, X.ZSIN(this.FnTZ()));
			}
		}

		public float drawy
		{
			get
			{
				return -IN.hh + 65f;
			}
		}

		public float drawy_u
		{
			get
			{
				return this.drawy * 0.015625f;
			}
		}

		public float left_shift_dx
		{
			get
			{
				return (float)(this.max - 1) * 0.5f * 56f * (float)X.MPF(this.is_en);
			}
		}

		public float tale_life_x
		{
			get
			{
				int num = X.Mx(0, this.life - 1);
				return this.drawx + this.left_shift_dx + 56f * (float)num * (float)X.MPF(this.is_pr);
			}
		}

		public float tale_life_x_u
		{
			get
			{
				return this.tale_life_x * 0.015625f;
			}
		}

		public int get_life(bool calc_crack = false)
		{
			return this.life - ((calc_crack && this.crack >= this.max_crack) ? 1 : 0);
		}

		public int get_miss_count()
		{
			return this.max - this.get_life(true);
		}

		public bool is_en
		{
			get
			{
				return this.Rpc.is_en;
			}
		}

		public bool is_pr
		{
			get
			{
				return this.Rpc.is_pr;
			}
		}

		public DjGM GM
		{
			get
			{
				return this.Rpc.GM;
			}
		}

		public readonly DjRPC Rpc;

		private readonly Func<float> FnTZ;

		private int max = 1;

		private int max_crack = 1;

		private int life = 1;

		private int crack;

		private float alpha_c;

		private const int CRACK_LAYER_MAX = 8;

		private const int CRACK_LAYER0 = 1;

		private const int PF_FRAME_BLANK = 2;

		private const int PF_FRAME_EN = 1;

		private const int PF_FRAME_PR = 0;

		private float t_vib = -1f;

		private float t_cure;

		private const float MAXT_CURE = 60f;

		private const float MAXT_VIB_S = 20f;

		private const float marginx = 56f;
	}
}
