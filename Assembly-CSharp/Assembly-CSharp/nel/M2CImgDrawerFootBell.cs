using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerFootBell : M2CImgDrawer, IRunAndDestroy, IBCCFootListener
	{
		public static void flush()
		{
			M2CImgDrawerFootBell.untouched = true;
			M2CImgDrawerFootBell.Abell_order = null;
			M2CImgDrawerFootBell.Running = null;
		}

		public M2CImgDrawerFootBell(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			this.FD_fnDrawLight = new M2DrawBinder.FnEffectBind(this.fnDrawLight);
			if (M2CImgDrawerFootBell.AOnMap != null)
			{
				M2CImgDrawerFootBell.AOnMap = null;
				this.removeRunner();
			}
			M2CImgDrawerFootBell.bell_pushed = -1;
			M2CImgDrawerFootBell.delay = -1f;
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (this.layer == 3)
			{
				return false;
			}
			bool flag = base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			if (!M2CImgDrawerFootBell.untouched && M2CImgDrawerFootBell.Abell_order == null && X.isinStr(M2CImgDrawerFootBell.Abell_order, this.Cp.ToString(), -1) >= 0)
			{
				this.light_on = false;
				return flag;
			}
			this.light_on = true;
			return flag;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.layer == 3)
			{
				return;
			}
			if (base.Mp.BCC != null)
			{
				base.Mp.BCC.addBCCFootListener(this);
				if (M2CImgDrawerFootBell.AOnMap == null)
				{
					M2CImgDrawerFootBell.AOnMap = new List<M2CImgDrawerFootBell>();
				}
				M2CImgDrawerFootBell.AOnMap.Add(this);
				if (this.light_on)
				{
					this.t_light0 = base.Mp.floort + 120f;
					this.Ed = base.Mp.setED("FootBell - Light", this.FD_fnDrawLight, 0f);
				}
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (base.Mp.BCC != null)
			{
				base.Mp.BCC.remBCCFootListener(this);
			}
			this.removeRunner();
			M2CImgDrawerFootBell.bell_pushed = -1;
			M2CImgDrawerFootBell.delay = -1f;
			this.Ed = base.Mp.remED(this.Ed);
		}

		private void removeRunner()
		{
			if (M2CImgDrawerFootBell.Running != null)
			{
				base.Mp.remRunnerObject(M2CImgDrawerFootBell.Running);
			}
			M2CImgDrawerFootBell.Running = null;
		}

		public uint getFootableAimBits()
		{
			return 8U;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			return BufRc.Set(this.Cp.mapcx - 1f, this.Cp.mtop - 3f, 2f, (float)this.Cp.iheight * base.Mp.rCLEN + 3f);
		}

		public bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd)
		{
			if (Fd.isCenterPr())
			{
				if (M2CImgDrawerFootBell.Abell_order == null)
				{
					return false;
				}
				if (M2CImgDrawerFootBell.Running == null)
				{
					M2CImgDrawerFootBell.Running = this;
					base.Mp.addRunnerObject(this);
				}
				int num = X.isinStr(M2CImgDrawerFootBell.Abell_order, this.Cp.ToString(), -1);
				if (num >= 0)
				{
					M2CImgDrawerFootBell.bell_pushed = X.Mx(M2CImgDrawerFootBell.bell_pushed, num);
				}
			}
			return true;
		}

		public bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false)
		{
			return true;
		}

		public void rewriteFootType(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd, ref string s)
		{
		}

		public bool deactivate()
		{
			if (this.light_on)
			{
				this.light_on = false;
				this.t_light0 = base.Mp.floort;
				return true;
			}
			return false;
		}

		public bool activate()
		{
			if (this.layer == 3)
			{
				return false;
			}
			if (!this.light_on)
			{
				this.light_on = true;
				if (this.Ed == null)
				{
					this.Ed = base.Mp.setED("FootBell - Light", this.FD_fnDrawLight, 0f);
				}
				this.t_light0 = base.Mp.floort;
				base.Mp.PtcSTsetVar("cx", (double)this.Cp.mapcx).PtcSTsetVar("cy", (double)this.Cp.mapcy).PtcSTsetVar("maxt", 30.0)
					.PtcST("general_circle_t", null, PTCThread.StFollow.NO_FOLLOW);
				return true;
			}
			return false;
		}

		public bool fnDrawLight(EffectItem Ef, M2DrawBinder Ed)
		{
			bool flag = false;
			if (this.t_light0 >= 0f)
			{
				float num = X.ZLINE(base.Mp.floort - this.t_light0, 24f);
				if (num >= 1f)
				{
					if (this.light_on)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				Ef.x = this.Cp.mapcx;
				Ef.y = this.Cp.mapcy;
				PxlMeshDrawer srcMesh = this.Cp.Img.getSrcMesh(3);
				if (srcMesh != null && Ed.isinCamera(Ef, 1f, 1f))
				{
					Color32 color = MTRX.ColWhite;
					if (this.light_on)
					{
						color = MTRX.cola.Gray().setA1(1f).blend(MTRX.ColWhite, 1f - num * 0.6f)
							.C;
					}
					else
					{
						color.a = (byte)(255f * (1f - num));
					}
					MeshDrawer meshImg = Ef.GetMeshImg("", base.M2D.IMGS.MIchip, BLEND.ADD, false);
					meshImg.Col = color;
					meshImg.RotaMesh(0f, 0f, base.Mp.base_scale, base.Mp.base_scale, this.Cp.draw_rotR, srcMesh, this.Cp.flip, false);
				}
			}
			if (!flag)
			{
				this.Ed = null;
				this.t_light0 = -1f;
			}
			return flag;
		}

		public static void initializeBellPosition(string order_key, string[] _Asound)
		{
			if (M2CImgDrawerFootBell.AOnMap == null || M2CImgDrawerFootBell.AOnMap.Count == 0 || (M2CImgDrawerFootBell.Running != null && M2CImgDrawerFootBell.bell_pushed >= 0))
			{
				return;
			}
			int count = M2CImgDrawerFootBell.AOnMap.Count;
			M2CImgDrawerFootBell.untouched = false;
			List<M2Puts> list = new List<M2Puts>(count);
			M2CImgDrawerFootBell.Abell_order = new string[count];
			bool flag = false;
			M2CImgDrawerFootBell.Asound = _Asound;
			Map2d map2d = null;
			for (int i = 0; i < count; i++)
			{
				M2CImgDrawerFootBell m2CImgDrawerFootBell = M2CImgDrawerFootBell.AOnMap[i];
				if (i == 0)
				{
					map2d = m2CImgDrawerFootBell.Mp;
				}
				list.Add(m2CImgDrawerFootBell.Cp);
				if (m2CImgDrawerFootBell.deactivate())
				{
					flag = true;
				}
			}
			if (flag)
			{
				SND.Ui.play("home_bell_turned_off", false);
			}
			string text = order_key.ToUpper();
			if (text != null)
			{
				if (!(text == "R2L"))
				{
					if (!(text == "L2R"))
					{
						if (text == "REVERSE")
						{
							list.Reverse();
						}
					}
					else
					{
						list.Sort(delegate(M2Puts Va, M2Puts Vb)
						{
							float mapcx = Va.mapcx;
							float mapcx2 = Vb.mapcx;
							if (mapcx == mapcx2)
							{
								return 0;
							}
							if (mapcx >= mapcx2)
							{
								return 1;
							}
							return -1;
						});
					}
				}
				else
				{
					list.Sort(delegate(M2Puts Va, M2Puts Vb)
					{
						float mapcx3 = Va.mapcx;
						float mapcx4 = Vb.mapcx;
						if (mapcx3 == mapcx4)
						{
							return 0;
						}
						if (mapcx3 >= mapcx4)
						{
							return -1;
						}
						return 1;
					});
				}
			}
			for (int j = 0; j < count; j++)
			{
				M2CImgDrawerFootBell.Abell_order[j] = list[j].ToString();
			}
			M2CImgDrawerFootBell.bell_pushed = -1;
			M2CImgDrawerFootBell.delay = -1f;
			M2CImgDrawerFootBell.bgm_fade_delay = ((M2CImgDrawerFootBell.bgm_fade_delay < 0f) ? (-1f) : map2d.floort);
		}

		public bool run(float fcnt)
		{
			if (M2CImgDrawerFootBell.Abell_order == null || M2CImgDrawerFootBell.Asound == null)
			{
				this.destruct();
				return false;
			}
			int num = -1;
			if (M2CImgDrawerFootBell.bell_pushed >= 0)
			{
				if (base.Mp.floort < M2CImgDrawerFootBell.delay)
				{
					return true;
				}
				for (int i = 0; i <= M2CImgDrawerFootBell.bell_pushed; i++)
				{
					if (M2CImgDrawerFootBell.Abell_order[i] != null)
					{
						num = i;
						break;
					}
				}
			}
			if (num >= 0)
			{
				if (M2CImgDrawerFootBell.bgm_fade_delay < 0f)
				{
					BGM.fadein(15f, 60f);
				}
				string text = M2CImgDrawerFootBell.Abell_order[num];
				M2CImgDrawerFootBell.Abell_order[num] = null;
				string text2 = M2CImgDrawerFootBell.Asound[num % M2CImgDrawerFootBell.Asound.Length];
				float num2 = 1f;
				if (REG.match(text2, M2CImgDrawerFootBell.RegTime))
				{
					text2 = REG.leftContext;
					num2 *= X.Nm(REG.R1, 1f, false);
				}
				M2CImgDrawerFootBell.delay = base.Mp.floort + 27.692308f * num2;
				M2CImgDrawerFootBell.bgm_fade_delay = base.Mp.floort + 100f;
				if (num >= M2CImgDrawerFootBell.bell_pushed)
				{
					M2CImgDrawerFootBell.bell_pushed = -1;
				}
				M2CImgDrawerFootBell m2CImgDrawerFootBell = M2CImgDrawerFootBell.findByKey(text);
				if (m2CImgDrawerFootBell != null)
				{
					m2CImgDrawerFootBell.activate();
					M2Mover baseMover = base.M2D.Cam.getBaseMover();
					if (baseMover == null)
					{
						base.M2D.Snd.play(text2);
					}
					else
					{
						base.M2D.Snd.playAt(text2, "", X.MMX(baseMover.x - 5f, m2CImgDrawerFootBell.Cp.mapcx, baseMover.x + 5f), baseMover.y, SndPlayer.SNDTYPE.SND, 1);
					}
				}
				else
				{
					base.M2D.Snd.play(text2);
				}
			}
			if (M2CImgDrawerFootBell.bgm_fade_delay >= 0f)
			{
				if (base.Mp.floort < M2CImgDrawerFootBell.bgm_fade_delay)
				{
					return true;
				}
				M2CImgDrawerFootBell.bgm_fade_delay = -1f;
				BGM.fadein(100f, 160f);
			}
			this.destruct();
			return false;
		}

		public static M2CImgDrawerFootBell findByKey(string cp_key)
		{
			for (int i = M2CImgDrawerFootBell.AOnMap.Count - 1; i >= 0; i--)
			{
				M2CImgDrawerFootBell m2CImgDrawerFootBell = M2CImgDrawerFootBell.AOnMap[i];
				if (m2CImgDrawerFootBell.Cp.ToString() == cp_key)
				{
					return m2CImgDrawerFootBell;
				}
			}
			return null;
		}

		public void destruct()
		{
			if (M2CImgDrawerFootBell.Running == this)
			{
				M2CImgDrawerFootBell.Running = null;
			}
		}

		public int id;

		private static bool untouched = true;

		private static List<M2CImgDrawerFootBell> AOnMap;

		private static string[] Abell_order;

		private static string[] Asound;

		private static M2CImgDrawerFootBell Running;

		private static int bell_pushed;

		private static float bgm_fade_delay = -1f;

		private static float delay;

		private bool light_on;

		private float t_light0 = -1f;

		private static Regex RegTime = new Regex("\\:([\\d\\.]+)$");

		private const float T_FADE = 24f;

		private M2DrawBinder Ed;

		private M2DrawBinder.FnEffectBind FD_fnDrawLight;
	}
}
