using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjRPC
	{
		public bool is_pr
		{
			get
			{
				return !this.is_en;
			}
		}

		public DjRPC(DjGM _GM, bool _is_en)
		{
			this.is_en = _is_en;
			this.GM = _GM;
			this.Life = new DjLife(this, delegate
			{
				if (this.t < 0f)
				{
					return X.ZSIN(70f + this.t, 70f);
				}
				return X.ZSIN(this.t, 70f);
			});
			this.FD_FnDrawRPC = new FnEffectRun(this.FnDrawRPC);
			if (this.is_pr)
			{
				this.ATx = new TextRenderer[3];
				for (int i = 0; i < 3; i++)
				{
					TextRenderer textRenderer = (this.ATx[i] = IN.CreateGob(this.GM.DJ.MMRD.gameObject, "-TxRPC_" + i.ToString()).AddComponent<TextRenderer>());
					textRenderer.size = 20f;
					textRenderer.html_mode = true;
					textRenderer.text_content = ((i == 0) ? "<key la/>" : ((i == 1) ? "<key ra/>" : "<key ta/>"));
					textRenderer.transform.position = new Vector3(this.drawx_u + DjRPC.i2posx_u(i) + 0.546875f, DjRPC.drawy_u + DjRPC.i2posy_u(i), -4.05f);
					textRenderer.alpha = 0f;
					textRenderer.MakeValot(null, null);
				}
			}
		}

		public void activate()
		{
			if (this.t < 0f)
			{
				this.t = X.Mx(70f + this.t, 0f);
				if (this.Life.is_alive_c)
				{
					SND.Ui.play("dojo_rpc_appear", false);
				}
				if (this.is_pr)
				{
					for (int i = 0; i < 3; i++)
					{
						this.ATx[i].gameObject.SetActive(true);
						this.ATx[i].alpha = 0f;
					}
				}
			}
			this.hit_t = 0f;
			if (this.Ef == null)
			{
				this.Ef = this.GM.get_EF().setEffectWithSpecificFn(this.is_pr ? "RPC_pr" : "RPC_en", this.drawx_u, DjRPC.drawy_u, 0f, 0, 0, this.FD_FnDrawRPC);
			}
			this.Ef.time = 0;
			this.pre_hand = -1;
		}

		public void deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = X.Mn(-1f, -70f + this.t);
			}
		}

		public void initHitHand(bool failure, int hand_type)
		{
			if (this.t < 0f)
			{
				return;
			}
			this.hit_t = (failure ? (-25f) : 70f);
			this.hand_type = (byte)hand_type;
			if (!failure)
			{
				this.GM.PtcVar("cx", this.drawx_u).PtcVar("cy", DjRPC.drawy_u).PtcVarS("pr", this.is_pr ? "pr" : "en")
					.PtcVar("type", (float)hand_type)
					.PtcST("dojo_hit_success", null);
				if (this.is_pr)
				{
					for (int i = 0; i < 3; i++)
					{
						this.ATx[i].alpha = 0f;
					}
				}
			}
		}

		public void closeHitHand()
		{
			if (this.hit_t != 0f && this.t >= 0f)
			{
				if (this.hand_type == 255)
				{
					this.hit_t = 0f;
					this.hand_type = 0;
					return;
				}
				this.hand_type |= 16;
				if (this.hit_t > 50f)
				{
					this.hit_t = 50f;
				}
			}
		}

		public void run(float fcnt)
		{
			if (this.hand_type != 255)
			{
				if (this.hit_t != 0f && (this.hand_type & 16) != 0)
				{
					if (this.hit_t > 0f)
					{
						this.fine_tx_alpha = true;
					}
					this.hit_t = X.VALWALK(this.hit_t, 0f, fcnt);
				}
				else if (this.hit_t > 50f)
				{
					this.hit_t = X.VALWALK(this.hit_t, 50f, fcnt);
				}
			}
			if (this.t >= 0f)
			{
				if (this.t < 100f)
				{
					this.t += fcnt;
					this.fine_tx_alpha = true;
					if (this.t >= 70f)
					{
						this.Life.run(fcnt, true);
						if (this.Ef != null && this.Ef.time == 0)
						{
							this.Ef.time = 1;
							if (this.Life.is_alive_c)
							{
								SND.Ui.play("dojo_rpc_appear_1", false);
								for (int i = 0; i < 3; i++)
								{
									this.GM.PtcVar("cx", this.drawx_u + DjRPC.i2posx_u(i)).PtcVar("cy", DjRPC.drawy_u + DjRPC.i2posy_u(i)).PtcST("dojo_rpc_appear", null);
								}
							}
						}
					}
					else
					{
						this.Life.run(fcnt, false);
					}
				}
				else
				{
					this.Life.run(fcnt, true);
				}
				if (this.is_pr && this.fine_tx_alpha)
				{
					float num = X.ZLINE(this.t - 70f, 30f) * (float)((this.Life.is_alive_c && !this.GM.isClearedState()) ? 1 : 0);
					if (this.hit_t > 0f)
					{
						num *= X.ZLINE(50f - this.hit_t, 25f);
					}
					for (int j = 0; j < 3; j++)
					{
						this.ATx[j].alpha = num;
					}
					this.fine_tx_alpha = false;
					return;
				}
			}
			else if (this.t > -70f)
			{
				this.Life.run(fcnt, false);
				this.t -= fcnt;
				if (this.is_pr)
				{
					this.fine_tx_alpha = false;
					for (int k = 0; k < 3; k++)
					{
						this.ATx[k].alpha = X.ZLINE(35f + this.t, 35f);
					}
				}
				if (this.t <= -70f)
				{
					if (this.Ef != null)
					{
						this.Ef.destruct();
						this.Ef = null;
					}
					if (this.is_pr)
					{
						for (int l = 0; l < 3; l++)
						{
							this.ATx[l].gameObject.SetActive(true);
						}
					}
				}
			}
		}

		public bool FnDrawRPC(EffectItem E)
		{
			float num;
			if (this.t >= 0f)
			{
				num = 1f - X.ZPOW(this.t, 70f);
			}
			else
			{
				num = X.ZSINV(-this.t, 70f);
			}
			MeshDrawer meshImg = E.GetMeshImg("rpc", this.GM.MI, BLEND.NORMALP3, true);
			if (meshImg.getTriMax() == 0)
			{
				meshImg.base_z += 0.42f;
			}
			meshImg.allocUv23(72, false);
			int i = 0;
			while (i < 3)
			{
				float num2 = DjRPC.i2posx(i);
				float num3 = DjRPC.i2posy(i);
				meshImg.Col = meshImg.ColGrd.White().C;
				Color32 color = MTRX.ColTrnsp;
				PxlFrame frame = this.GM.SqRpc.getFrame(i);
				float num4 = 1f;
				bool flag = false;
				float num5 = 1f;
				if (this.hit_t > 0f)
				{
					if (i == (int)(this.hand_type & 3))
					{
						flag = true;
						float num6 = X.ZSIN(50f - this.hit_t, 25f);
						if (this.hit_t < 50f)
						{
							float num7 = 1f - X.ZLINE(25f - this.hit_t, 25f);
							MeshDrawer meshImg2 = E.GetMeshImg("rpca", this.GM.MI, BLEND.ADD, true);
							if (meshImg2.getTriMax() == 0)
							{
								meshImg2.base_z += 0.44f;
							}
							PxlFrame frame2 = this.GM.SqRpc.getFrame(i + 6);
							for (int j = 0; j < 10; j++)
							{
								float num8 = num6 - ((float)j + 0.5f) / 10f;
								if (num8 <= 0f)
								{
									break;
								}
								meshImg2.Col = meshImg2.ColGrd.Set(DjRPC.i2col(i)).mulA(num7 * 0.5f * (float)(10 - j) / 10f).C;
								meshImg2.RotaPF(num2 * num8, num3 * num8, 1f, 1f, 0f, frame2, false, false, false, uint.MaxValue, false, 0);
							}
						}
						num2 *= num6;
						num3 *= num6;
						if (this.hit_t > 50f)
						{
							color = meshImg.ColGrd.Set(MTRX.ColTrnsp).blend(16777215U, 0.75f + 0.25f * X.ZSIN2(70f - this.hit_t, 3f) - X.ZSIN(70f - this.hit_t - 3f, 14f)).C;
							meshImg.ColGrd.White();
							num5 += 0.75f + 0.25f * X.ZSIN2(70f - this.hit_t, 3f) - X.ZSIN(70f - this.hit_t - 3f, 8f);
							goto IL_0424;
						}
						goto IL_0424;
					}
					else
					{
						num4 *= X.ZLINE(50f - this.hit_t, 25f);
						if (num4 != 0f)
						{
							goto IL_0424;
						}
					}
				}
				else
				{
					if (this.hit_t >= 0f)
					{
						goto IL_0424;
					}
					float num9 = 1f - X.ZLINE(25f + this.hit_t, 25f);
					if (this.hand_type == 255)
					{
						meshImg.ColGrd.blend(2573230176U, num9);
						goto IL_0424;
					}
					if (i == (int)(this.hand_type & 3))
					{
						flag = true;
						meshImg.ColGrd.blend(4285822068U, num9 * (0.5f + 0.2f * X.COSIT(13.3f) + 0.1f * X.COSIT(6.81f)));
						num2 += num9 * (float)X.IntR(3f * X.COSI((float)((int)(this.GM.t_state / 2f)), 2.44f));
						num3 += num9 * (float)X.IntR(3f * X.COSI((float)((int)(this.GM.t_state / 2f)), 1.19f));
						goto IL_0424;
					}
					meshImg.ColGrd.blend(4285822068U, num9);
					goto IL_0424;
				}
				IL_0579:
				i++;
				continue;
				IL_0424:
				if (num > 0f)
				{
					uint ran = X.GETRAN2(E.f0 + 443 + i * 3, 3 + i * 5);
					float num10 = X.RAN(ran, 1124) * 0.125f;
					float num11 = X.ZLINE(num - num10, 1f - num10);
					num3 += (140f + X.RAN(ran, 2870) * 70f) * num11;
					num4 *= 1f - X.ZLINE(num11 - 0.75f, 0.25f);
				}
				meshImg.Uv23(color, false);
				meshImg.Col = meshImg.ColGrd.mulA(num4).C;
				if (num == 0f && !flag)
				{
					float num12 = X.NI(1f, 1.08f, this.GM.tz_beat) * num5;
					meshImg.RotaPF(num2, num3, num12, num12, 0f, frame, false, false, false, 4294967263U, false, 0);
					num12 = X.NI(1f, 1.04f, this.GM.tz_beat) * num5;
					meshImg.RotaPF(num2, num3, num12, num12, 0f, frame, false, false, false, 32U, false, 0);
				}
				else
				{
					meshImg.RotaPF(num2, num3, num5, num5, 0f, frame, false, false, false, uint.MaxValue, false, 0);
				}
				meshImg.allocUv23(0, true);
				goto IL_0579;
			}
			this.Life.drawTo(E);
			return true;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public static int checkWin(int m_hand_type, int o_hand_type)
		{
			if (m_hand_type != 0)
			{
				if (m_hand_type != 1)
				{
					if (o_hand_type == 2)
					{
						return 0;
					}
					if (o_hand_type != 0)
					{
						return -1;
					}
					return 1;
				}
				else
				{
					if (o_hand_type == 1)
					{
						return 0;
					}
					if (o_hand_type != 2)
					{
						return -1;
					}
					return 1;
				}
			}
			else
			{
				if (o_hand_type == 0)
				{
					return 0;
				}
				if (o_hand_type != 1)
				{
					return -1;
				}
				return 1;
			}
		}

		public static float i2posx(int i)
		{
			if (i == 0)
			{
				return -125f;
			}
			if (i != 1)
			{
				return 0f;
			}
			return 125f;
		}

		public static float i2posy(int i)
		{
			if (i != 2)
			{
				return -30f;
			}
			return 80f;
		}

		public static float i2posx_u(int i)
		{
			return DjRPC.i2posx(i) * 0.015625f;
		}

		public static float i2posy_u(int i)
		{
			return DjRPC.i2posy(i) * 0.015625f;
		}

		public static uint i2col(int i)
		{
			if (i == 0)
			{
				return 4294461255U;
			}
			if (i != 1)
			{
				return 4282934775U;
			}
			return 4282971982U;
		}

		public float drawx
		{
			get
			{
				return DjRPC.drawxS(this.is_en);
			}
		}

		public static float drawxS(bool is_en)
		{
			return IN.wh * (float)X.MPF(is_en) * 0.55f;
		}

		public float drawx_u
		{
			get
			{
				return this.drawx * 0.015625f;
			}
		}

		public static float drawy
		{
			get
			{
				return IN.hh * 0.45f + 30f;
			}
		}

		public static float drawy_u
		{
			get
			{
				return DjRPC.drawy * 0.015625f;
			}
		}

		public bool full_appeared
		{
			get
			{
				return this.t >= 90f;
			}
		}

		public readonly DjGM GM;

		public readonly DjLife Life;

		public readonly bool is_en;

		private float t = -70f;

		private const float T_FADE = 70f;

		private EffectItem Ef;

		private FnEffectRun FD_FnDrawRPC;

		public const int MAX_HAND = 3;

		public const int HAND_RK = 0;

		public const int HAND_SC = 1;

		public const int HAND_PA = 2;

		private const int layer_count = 6;

		private const uint top_laye_bit = 32U;

		private const float shift_x = 125f;

		private const float shift_y = 80f;

		private const float shift_y_b = -30f;

		private float hit_t;

		private byte hand_type;

		private const uint col_rk = 4294461255U;

		private const uint col_sc = 4282971982U;

		private const uint col_pa = 4282934775U;

		private const byte BIT_DEACTIVATING = 16;

		private const float MAXT_HIT = 50f;

		private const float MAXT_HIT_BUMP = 70f;

		private const float MAXT_HIT_FAIL = 25f;

		private TextRenderer[] ATx;

		public bool fine_tx_alpha;

		private int pre_hand = -1;
	}
}
