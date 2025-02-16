using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpEffector : M2LpNearCheck
	{
		public M2LpEffector(string __key, int _i, M2MapLayer L)
			: base(__key, 0, L)
		{
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			Map2d mp = this.Lay.Mp;
			this.auto_assign_to_NM = false;
			this.Meta = new META(this.comment);
			this.T_FADE = -1f;
			M2EventContainer eventContainer = mp.getEventContainer();
			if (this.Meta.GetB("effector_var0", false) && !eventContainer.Get(this.key, true, true))
			{
				this.auto_assign_to_NM = true;
				this.alloc_in_ev = true;
			}
			string[] array = this.Meta.Get("sndloop");
			this.SndI = null;
			if (array != null)
			{
				this.SndI = mp.M2D.Snd.Environment.AddLoop(array[0], base.unique_key, base.mapcx, base.mapcy, (float)this.mapw, (float)this.maph, this.Meta.GetNm("sndloop", 6f, 1), this.Meta.GetNm("sndloop", 6f, 2), null);
			}
			string s = this.Meta.GetS("shader");
			Shader shd;
			if (TX.valid(s) && (shd = MTR.getShd(s)) != null)
			{
				int i = this.Meta.GetI("shader", -1, 1);
				float num;
				if (this.Meta.GetNm("shader_pattern", out num, 0f, 0))
				{
					this.MtrPasting = MTR.MIiconP.getMtr(shd, i);
					MTRX.setMaterialST(this.MtrPasting, this.Meta.GetSi(1, "shader_pattern") ?? "_MainTex", MTRX.SqEfPattern.getImage((int)num, 0), 0f);
				}
				else
				{
					this.MtrPasting = MTRX.MIicon.getMtr(shd, i);
				}
				this.AColPasting = new Color32[4];
				string[] array2 = this.Meta.Get("col_grd");
				if (array2 != null && array2.Length >= 3)
				{
					Color32 color = C32.d2c(X.NmUI(array2[1], uint.MaxValue, true, true));
					Color32 color2 = C32.d2c(X.NmUI(array2[2], uint.MaxValue, true, true));
					string text = array2[0].ToUpper();
					if (text != null)
					{
						if (text == "BOTTOM2TOP")
						{
							this.AColPasting[0] = (this.AColPasting[3] = color);
							this.AColPasting[1] = (this.AColPasting[2] = color2);
							goto IL_03CD;
						}
						if (text == "TOP2BOTTOM")
						{
							this.AColPasting[0] = (this.AColPasting[3] = color2);
							this.AColPasting[1] = (this.AColPasting[2] = color);
							goto IL_03CD;
						}
						if (text == "LEFT2RIGHT")
						{
							this.AColPasting[0] = (this.AColPasting[1] = color);
							this.AColPasting[3] = (this.AColPasting[2] = color2);
							goto IL_03CD;
						}
						if (text == "RIGHT2LEFT")
						{
							this.AColPasting[0] = (this.AColPasting[1] = color2);
							this.AColPasting[3] = (this.AColPasting[2] = color);
							goto IL_03CD;
						}
					}
					X.ALLV<Color32>(this.AColPasting, MTRX.ColWhite);
				}
				else
				{
					array2 = this.Meta.Get("col");
					if (array2 != null && array2.Length >= 1)
					{
						X.ALLV<Color32>(this.AColPasting, C32.d2c(X.NmUI(array2[0], uint.MaxValue, true, false)));
					}
					else
					{
						X.ALLV<Color32>(this.AColPasting, MTRX.ColWhite);
					}
				}
				IL_03CD:
				this.set_edc = this.Meta.GetB("set_edc", false);
				if (this.set_edc)
				{
					this.EdPasting = mp.setEDC(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawBinderPasting), 0f);
				}
				else
				{
					this.EdPasting = mp.setED(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawBinderPasting), 0f);
				}
			}
			base.initAction(normal_map);
		}

		public override void initEnter(M2Mover Mv)
		{
			if (!base.activated)
			{
				this.activate();
			}
			base.initEnter(Mv);
		}

		public override void quitEnter(M2Mover Mv)
		{
			if (base.activated)
			{
				this.deactivate();
			}
			base.quitEnter(Mv);
		}

		public override void activate()
		{
			if (this.Pe == null && this.Meta != null && this.Meta.GetB("effector_var0", false))
			{
				this.Pe = (this.Mp.M2D as NelM2DBase).PE.setPE(POSTM.M2D_VAR_0, (float)X.Mx(this.Meta.GetI("effector_var0", 0, 0), 1), (float)X.MMX(0, this.Meta.GetI("effector_var0", 1, 1), 1), 0);
				if (this.Pe != null)
				{
					this.Pe.do_not_consider_timeout = true;
				}
			}
			if (this.SndI != null)
			{
				this.SndI.setVolume(base.unique_key, 1f);
			}
		}

		public override void deactivate()
		{
			if (this.Pe != null)
			{
				this.Pe.deactivate(false);
				this.Pe = null;
			}
			if (this.SndI != null)
			{
				this.SndI.setVolume(base.unique_key, 0f);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			if (this.Pe != null)
			{
				this.Pe.destruct();
				this.Pe = null;
			}
			if (this.Meta != null)
			{
				string[] array = this.Meta.Get("sndloop");
				if (array != null && array.Length != 0)
				{
					this.Mp.M2D.Snd.Environment.RemLoop(array[0], base.unique_key);
				}
			}
			if (this.EdPasting != null)
			{
				this.EdPasting = this.Mp.remED(this.EdPasting);
				this.EdPasting = null;
			}
			if (this.Ev != null)
			{
				this.Lay.Mp.destructEvent(this.Ev);
				this.Ev = null;
			}
		}

		private bool fnDrawBinderPasting(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.MtrPasting == null)
			{
				return false;
			}
			Ef.x = base.mapcx;
			Ef.y = base.mapcy;
			if (!Ed.isinCamera(Ef, (float)this.mapw * 0.5f + 0.5f, (float)this.maph * 0.5f + 0.5f))
			{
				return true;
			}
			object obj = (this.set_edc ? 1f : this.Mp.base_scale);
			MeshDrawer mesh = Ef.GetMesh("", this.MtrPasting, false);
			mesh.TriRectBL(0);
			object obj2 = obj;
			float num = obj2 * base.w * 0.5f;
			float num2 = obj2 * base.h * 0.5f;
			float num3 = this.Mp.rCLEN / 256f;
			mesh.Col = this.AColPasting[0];
			mesh.uvRectN(this.x * num3, ((float)this.Mp.height - (base.y + this.height)) * num3);
			mesh.PosD(-num, -num2, null);
			mesh.Col = this.AColPasting[1];
			mesh.uvRectN(this.x * num3, ((float)this.Mp.height - base.y) * num3);
			mesh.PosD(-num, num2, null);
			mesh.Col = this.AColPasting[2];
			mesh.uvRectN(base.right * num3, ((float)this.Mp.height - base.y) * num3);
			mesh.PosD(num, num2, null);
			mesh.Col = this.AColPasting[3];
			mesh.uvRectN(base.right * num3, ((float)this.Mp.height - (base.y + this.height)) * num3);
			mesh.PosD(num, -num2, null);
			return true;
		}

		private string aim;

		private string jump_key;

		private M2EventItem Ev;

		private PostEffectItem Pe;

		private META Meta;

		private Material MtrPasting;

		private Color32[] AColPasting;

		private M2DrawBinder EdPasting;

		private M2SndLoopItem SndI;

		private bool set_edc;
	}
}
