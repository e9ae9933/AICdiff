using System;
using System.Collections.Generic;
using PixelLiner;
using Spine;
using UnityEngine;
using XX;

namespace evt
{
	public sealed class ManpuDrawer
	{
		public ManpuDrawer(EvDrawer _Con, EvDrawerContainer _DC, string _pos_id)
		{
			this.DC = _DC;
			this.Con = _Con;
			this.pos_id = _pos_id;
			this.AMp = new List<ManpuDrawer.MpItem>(1);
		}

		public static Texture2D reloadTexture()
		{
			if (ManpuDrawer.TxMP == null)
			{
				ManpuDrawer.TxMP = MTI.LoadContainerOneImage("SpineAnimEv/manpu__manpu", "_", null).Load<Texture2D>("Manpu.png");
			}
			if (ManpuDrawer.MtiAtlas == null)
			{
				ManpuDrawer.MtiAtlas = MTI.LoadContainerSpine("SpineAnimEv/manpu__manpu.atlas", "Manpu", null);
			}
			return ManpuDrawer.TxMP;
		}

		public static void fineManpuIconImages()
		{
			if (ManpuDrawer.AEvtManpu == null)
			{
				ManpuDrawer.AEvtManpu = MTRX.getPFArray(MTRX.PxlIcon.getPoseByName("evt_emo"), 0f, 0f);
			}
		}

		public static Texture2D releaseTexture()
		{
			if (ManpuDrawer.TxMP != null)
			{
				MTI.ReleaseContainer("SpineAnimEv/manpu__manpu", "_");
				ManpuDrawer.TxMP = null;
			}
			return ManpuDrawer.TxMP;
		}

		public static void releaseMti()
		{
			ManpuDrawer.releaseTexture();
			if (ManpuDrawer.MtiAtlas != null)
			{
				ManpuDrawer.MtiAtlas.remLoadKey("_MP");
			}
			ManpuDrawer.MtiAtlas = null;
		}

		public void activateManpu(Transform _ConTrs, string[] Akey, bool immediate = false)
		{
			int num = Akey.Length;
			if (num == 1 && Akey[0].Length == 0)
			{
				this.deactivate(immediate);
				return;
			}
			bool flag = true;
			for (int i = 0; i < num; i++)
			{
				string text = Akey[i];
				if (TX.isStart(text, "+", 0))
				{
					flag = false;
					text = TX.slice(text, 1);
				}
				ManpuDrawer.MP mp;
				if (!FEnum<ManpuDrawer.MP>.TryParse(text, out mp, true))
				{
					X.dl("不明な漫符ID: " + text, null, false, false);
				}
				else
				{
					if (flag)
					{
						flag = false;
						this.deactivate(true);
					}
					if ((this.used_mp & mp) == (ManpuDrawer.MP)0U)
					{
						this.AMp.Add(new ManpuDrawer.MpItem(this, mp, i));
					}
				}
			}
			this.ConTrs = _ConTrs;
			this.finePosition();
		}

		private void release(ManpuDrawer.MpItem Mpi, int index = -1)
		{
			if (Mpi == null)
			{
				return;
			}
			Mpi.destruct(this);
			if (Mpi.Md != null)
			{
				this.DC.fine_manpu_md = true;
			}
			this.used_mp &= ~Mpi.mp;
			if (index == -1)
			{
				this.AMp.Remove(Mpi);
				return;
			}
			this.AMp.RemoveAt(index);
		}

		public void deactivate(bool immediate = false)
		{
			if (immediate)
			{
				this.used_mp = (ManpuDrawer.MP)0U;
				for (int i = this.AMp.Count - 1; i >= 0; i--)
				{
					this.release(this.AMp[i], i);
				}
				this.AMp.Clear();
				this.ConTrs = null;
				return;
			}
			for (int j = this.AMp.Count - 1; j >= 0; j--)
			{
				this.AMp[j].deactivate();
			}
		}

		public void finePosition()
		{
			this.need_fine_pos = true;
		}

		private void finePositionExecute()
		{
			if (this.ConTrs == null)
			{
				return;
			}
			float num = this.xlen_px;
			float num2 = this.ylen_px;
			for (int i = this.AMp.Count - 1; i >= 0; i--)
			{
				this.AMp[i].finePosition(this, this.DC, this.ConTrs);
			}
		}

		public bool runDraw()
		{
			if (this.AMp.Count == 0)
			{
				return false;
			}
			if (this.need_fine_pos)
			{
				this.need_fine_pos = false;
				this.finePositionExecute();
			}
			if (X.D || this.redraw_flag)
			{
				float drawAlpha = this.Con.getDrawAlpha();
				this.redraw_flag = false;
				for (int i = this.AMp.Count - 1; i >= 0; i--)
				{
					ManpuDrawer.MpItem mpItem = this.AMp[i];
					if (!mpItem.runDraw(this, (float)X.AF, drawAlpha))
					{
						this.release(mpItem, i);
					}
				}
			}
			return true;
		}

		public readonly EvDrawer Con;

		public readonly EvDrawerContainer DC;

		public readonly string pos_id;

		private List<ManpuDrawer.MpItem> AMp;

		public ManpuDrawer.MP used_mp;

		public float xlen_px = 90f;

		public float ylen_px = 60f;

		public static Texture2D TxMP;

		public Transform ConTrs;

		private bool need_fine_pos;

		private static MTISpine MtiAtlas;

		public bool redraw_flag;

		private static PxlFrame[] AEvtManpu;

		public const string mti_image_path = "SpineAnimEv/manpu__manpu";

		public const string mti_atlas_path = "SpineAnimEv/manpu__manpu.atlas";

		public const string mti_load_key = "_MP";

		public enum MP : uint
		{
			QUE = 1U,
			AWK,
			EXC = 4U,
			SWT = 8U,
			SWT2 = 16U,
			SWB = 32U,
			EXQ = 64U,
			SMK = 128U,
			PLE = 256U,
			BLS = 512U,
			DEP = 1024U,
			KIR = 2048U,
			ANG = 4096U,
			LAG = 8192U,
			HEA = 16384U,
			HEA2 = 32768U,
			TTT = 65536U,
			BSM = 131072U,
			LIG = 262144U,
			ONP = 524288U,
			ONP2 = 1048576U,
			PIY = 2097152U,
			PLE2 = 4194304U,
			HOT = 8388608U,
			_ALL = 251658240U,
			_USE_ICONMESH = 528U,
			X_SIDE_RIGHT = 12582912U,
			LEFTONLY = 65601U,
			TOPONLY = 3809280U,
			RIGHTFLIP = 12599718U,
			LEFTFLIP = 251691008U,
			_NO_LOOP = 264260U
		}

		private sealed class MpItem
		{
			public MpItem(ManpuDrawer MD, ManpuDrawer.MP _mp, int _index)
			{
				this.mp = _mp;
				this.index = _index;
				if ((this.mp & ManpuDrawer.MP.TOPONLY) != (ManpuDrawer.MP)0U)
				{
					this.ag360 = 90f + X.NIXP(-4f, 4f);
				}
				else if ((this.mp & ManpuDrawer.MP.LEFTONLY) != (ManpuDrawer.MP)0U)
				{
					this.ag360 = 125f + X.NIXP(-10f, 20f);
				}
				else
				{
					this.ag360 = 55f + X.NIXP(-20f, 10f);
				}
				if ((this.mp & ManpuDrawer.MP.X_SIDE_RIGHT) != (ManpuDrawer.MP)0U && MD.Con.isOnRightFor(MD))
				{
					this.ag360 = 180f - this.ag360;
				}
				if (this.ag360 < 90f && (this.mp & ManpuDrawer.MP.RIGHTFLIP) != (ManpuDrawer.MP)0U)
				{
					this.flip = true;
				}
				if (this.ag360 >= 90f && (this.mp & ManpuDrawer.MP.LEFTFLIP) != (ManpuDrawer.MP)0U)
				{
					this.flip = true;
				}
				ManpuDrawer.MP mp = this.mp;
				if (mp <= ManpuDrawer.MP.DEP)
				{
					if (mp <= ManpuDrawer.MP.SWT2)
					{
						if (mp != ManpuDrawer.MP.EXC)
						{
							if (mp != ManpuDrawer.MP.SWT)
							{
								if (mp == ManpuDrawer.MP.SWT2)
								{
									this.len = 0f;
									this.scale *= 1.6f;
									this.ag360 += (float)(X.MPF(this.ag360 > 90f) * 30);
								}
							}
							else
							{
								this.len = 0.6f;
								this.ag360 += (float)(X.MPF(this.ag360 > 90f) * 30);
							}
						}
						else
						{
							this.len = 1.2f;
							this.ag360 += (float)(X.MPF(this.ag360 > 90f) * 10);
						}
					}
					else if (mp <= ManpuDrawer.MP.EXQ)
					{
						if (mp != ManpuDrawer.MP.SWB)
						{
							if (mp == ManpuDrawer.MP.EXQ)
							{
								this.scale = 0.3f;
								this.len = 1.06f;
							}
						}
						else
						{
							this.scale *= 1.25f;
							this.len = 0.98f;
							this.ag360 = (float)((this.ag360 > 90f) ? 180 : 0);
						}
					}
					else if (mp != ManpuDrawer.MP.BLS)
					{
						if (mp == ManpuDrawer.MP.DEP)
						{
							this.len = 1.15f;
						}
					}
					else
					{
						this.len = 0f;
						this.scale *= 2.8f;
						this.ag360 += (float)(X.MPF(this.ag360 > 90f) * 30);
					}
				}
				else
				{
					if (mp <= ManpuDrawer.MP.HEA)
					{
						if (mp <= ManpuDrawer.MP.ANG)
						{
							if (mp == ManpuDrawer.MP.KIR)
							{
								this.len = 1.5f;
								this.ag360 += (float)(X.MPF(this.ag360 > 90f) * 35);
								goto IL_0444;
							}
							if (mp != ManpuDrawer.MP.ANG)
							{
								goto IL_0444;
							}
							this.len = -85f;
							this.ag360 = 65f + X.NIXP(-4f, 10f);
							goto IL_0444;
						}
						else
						{
							if (mp == ManpuDrawer.MP.LAG)
							{
								this.scale *= 2f;
								this.len = 0.8f;
								goto IL_0444;
							}
							if (mp != ManpuDrawer.MP.HEA)
							{
								goto IL_0444;
							}
						}
					}
					else if (mp <= ManpuDrawer.MP.BSM)
					{
						if (mp != ManpuDrawer.MP.TTT)
						{
							if (mp != ManpuDrawer.MP.BSM)
							{
								goto IL_0444;
							}
							this.len = -15f;
							this.ag360 = 90f;
							goto IL_0444;
						}
					}
					else
					{
						if (mp != ManpuDrawer.MP.ONP && mp != ManpuDrawer.MP.ONP2)
						{
							goto IL_0444;
						}
						this.scale = 0.32f;
						this.len = -28f;
						goto IL_0444;
					}
					this.len = 1.1f;
					this.ag360 += (float)X.MPF(this.ag360 > 90f) * (25f + X.NIXP(-4f, 10f));
				}
				IL_0444:
				this.FD_fnSpineComplete = new AnimationState.TrackEntryDelegate(this.fnSpineComplete);
				if ((this.mp & ManpuDrawer.MP._USE_ICONMESH) != (ManpuDrawer.MP)0U)
				{
					this.Md = MD.DC.getManpuMeshDrawer();
					if (ManpuDrawer.AEvtManpu == null)
					{
						ManpuDrawer.fineManpuIconImages();
						return;
					}
				}
				else
				{
					this.Spb = MD.DC.getPooledSpineViewer(this.flip);
					this.Spb.enabled = false;
					Transform transform = this.Spb.transform;
					transform.localScale = new Vector3(this.scale * (float)X.MPF(!this.flip), this.scale, 1f);
					transform.localRotation = Quaternion.Euler(0f, 0f, this.drawag360);
				}
			}

			public void deactivate()
			{
				if (this.t >= 0f)
				{
					this.t = -1f;
				}
			}

			public void getDrawShift(ManpuDrawer Con, out float _x, out float _y)
			{
				if (this.len < 0f)
				{
					_x = -this.len * X.Cos360(this.ag360);
					_y = -this.len * X.Sin360(this.ag360);
					return;
				}
				_x = Con.xlen_px * this.len * X.Cos360(this.ag360);
				_y = Con.ylen_px * this.len * X.Sin360(this.ag360);
			}

			public void finePosition(ManpuDrawer Con, EvDrawerContainer DC, Transform ConTrs)
			{
				if (this.Spb != null)
				{
					float num;
					float num2;
					this.getDrawShift(Con, out num, out num2);
					Vector3 vector;
					Con.Con.getFacePosition(out vector, Con.pos_id, 0f);
					IN.PosP(this.Spb.transform, vector.x + num, vector.y + num2, DC.manpu_draw_z - 0.002f);
				}
				if (this.Md != null)
				{
					this.Md.base_z = DC.manpu_draw_z - 2E-05f;
				}
			}

			public bool runDraw(ManpuDrawer Con, float fcnt, float alp_talker)
			{
				if (this.t >= 0f)
				{
					if (this.t == 0f && this.Spb != null)
					{
						this.Spb.atlas_key = "Manpu";
						string text = "MP_" + this.mp.ToString().ToLower();
						this.Spb.reloadAnimKey(text);
						this.Spb.prepareTexturePreLoaded(ManpuDrawer.reloadTexture());
						this.Spb.attachTextMTI(ManpuDrawer.MtiAtlas);
						ManpuDrawer.MtiAtlas.addLoadKey("_MP", false);
						this.Spb.clearAnim("animation", -1000, null);
						this.Spb.call_complete_evnet_when_track0 = true;
						this.Spb.getViewer().update_sharedmaterials_array = false;
						this.Spb.addListenerComplete(this.FD_fnSpineComplete);
						this.Spb.prepareValot(Con.DC.use_valotile, CameraBidingsBehaviour.UiBind);
						this.Spb.valotile_enabled = Con.DC.use_valotile;
					}
					if (this.Spb != null)
					{
						this.Spb.setColorTe(EffectItem.Col1.White(), MTRX.cola.White().mulA(alp_talker), MTRX.colb.Transparent());
					}
					if (this.Md != null)
					{
						this.drawMd(Con, 1f, alp_talker);
					}
					this.t += fcnt;
				}
				else
				{
					this.t -= fcnt;
					if (this.t <= -40f)
					{
						return false;
					}
					if (this.Spb != null)
					{
						this.Spb.setColorTe(EffectItem.Col1.White(), MTRX.cola.White().mulA(alp_talker * X.ZLINE(40f + this.t, 40f)), MTRX.colb.Transparent());
					}
					if (this.Md != null)
					{
						this.drawMd(Con, X.ZLINE(40f + this.t, 40f), alp_talker);
					}
				}
				if (this.Spb != null)
				{
					bool flag = alp_talker > 0f;
					if (this.Spb.gameObject.activeSelf != flag)
					{
						this.Spb.gameObject.SetActive(flag);
					}
					this.Spb.updateAnim(true, (float)((int)(fcnt * this.anim_timescale * 1.5f)));
				}
				return true;
			}

			private void drawMd(ManpuDrawer Con, float tz, float alpha01)
			{
				if (this.Md == null)
				{
					return;
				}
				Vector3 vector;
				if (!Con.Con.getFacePosition(out vector, Con.pos_id, 0f))
				{
					return;
				}
				float num = this.scale;
				this.Md.Col = MTRX.cola.White().mulA(tz * alpha01).C;
				float num2;
				float num3;
				this.getDrawShift(Con, out num2, out num3);
				this.Md.base_px_x = vector.x + num2;
				this.Md.base_px_y = vector.y + num3;
				ManpuDrawer.MP mp = this.mp;
				if (mp != ManpuDrawer.MP.SWT2)
				{
					if (mp == ManpuDrawer.MP.BLS)
					{
						if (this.t >= 0f)
						{
							this.Md.Col = C32.MulA(uint.MaxValue, X.ZLINE(this.t, 50f) * alpha01);
						}
						this.Md.RotaPF(0f, -10f, num, num, 0f, ManpuDrawer.AEvtManpu[0], false, false, false, uint.MaxValue, false, 0);
					}
				}
				else
				{
					if (this.t >= 0f)
					{
						this.Md.Col = C32.MulA(uint.MaxValue, X.ZLINE(this.t, 20f) * alpha01);
					}
					this.Md.ColGrd.Set(this.Md.Col);
					float num4 = this.ag360 / 180f * 3.1415927f;
					for (int i = 0; i < 20; i++)
					{
						float num5 = (float)((IN.totalframe - i * 6) % 80);
						float num6 = X.ZLINE(num5, 80f) * 2f;
						if (num6 < 1f && num5 >= 0f)
						{
							uint ran = X.GETRAN2(i * 13 + this.f0 + this.index * 19, 2 + (this.f0 & 3) + this.index);
							float num7 = num4 + 0.19f * (-1f + X.RAN(ran, 1843) * 2f) * 3.1415927f;
							float num8 = X.NI(50, 70, X.RAN(ran, 1010));
							float num9 = X.Cos(num7);
							float num10 = X.Sin(num7);
							float num11 = num8 * num9;
							float num12 = 1.2f * num8 * num10;
							float num13 = X.ZSIN(num6, 0.6f) * X.NI(43, 82, X.RAN(ran, 2035));
							num11 += num13 * num9;
							num12 += num13 * num10 - X.ZPOW(num6 - 0.25f, 0.75f) * X.NI(100f, 110f, 0.5f);
							if (num6 >= 0.25f)
							{
								num7 = X.VALWALKANGLER(num7, -1.5707964f, 0.016f + X.ZSIN(num6 - 0.25f, 0.75f) * 0.66f);
							}
							if (this.ag360 < 90f)
							{
								num7 += 3.1415927f;
							}
							float num14 = X.ZLINE(num6 - 0.5f, 0.5f);
							this.Md.Col = C32.MulA(this.Md.ColGrd.C, 1f - num14);
							float num15 = X.NI(0.66f, 0.1f, X.RAN(ran, 1699)) * num * (1f - X.ZPOW(num14));
							this.Md.RotaPF(num11, num12, num15, num15, num7, ManpuDrawer.AEvtManpu[(X.RAN(ran, 1825) < 0.66f) ? 1 : 2], this.ag360 < 90f, false, false, uint.MaxValue, false, 0);
						}
					}
				}
				Con.DC.fine_manpu_md = true;
			}

			public void fnSpineComplete(TrackEntry Trk)
			{
				if ((this.mp & ManpuDrawer.MP._NO_LOOP) != (ManpuDrawer.MP)0U)
				{
					this.t = -200f;
					return;
				}
				Trk.TrackTime = 0f;
			}

			public void destruct(ManpuDrawer MD)
			{
				if (this.Spb != null)
				{
					this.Spb.remListenerComplete(this.FD_fnSpineComplete);
					this.Spb = MD.DC.releasePooledSpineViewer(this.Spb);
				}
			}

			public SpineViewerBehaviour Spb;

			public MeshDrawer Md;

			public ManpuDrawer.MP mp;

			public float scale = 0.2f;

			public float drawag360;

			public float len = 1f;

			public bool flip;

			public float ag360;

			public float t;

			public float anim_timescale = 0.87f;

			private const int FADE_AMXT = 40;

			public int f0;

			public int index;

			public AnimationState.TrackEntryDelegate FD_fnSpineComplete;
		}
	}
}
