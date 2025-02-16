using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerCheckBonfire : M2CImgDrawerWithEffect, IActivatable
	{
		public M2CImgDrawerCheckBonfire(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			METACImg meta = this.Cp.getMeta();
			this.Bonf = new BonfireEffector(this.Cp).readMeta(meta, "bonfire", 0);
			this.start_af_min = 80;
			this.start_af_max = 130;
			this.Cp.arrangeable = true;
			this.effect_2_ed = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.EdPt = this.Bonf.setEDC(this.Cp, new M2DrawBinder.FnEffectBind(this.fnDrawToChip));
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.EdPt = base.Mp.remED(this.EdPt);
		}

		public void activate()
		{
			this.anm_t = (int)base.Mp.floort;
			Vector2 vector = this.Cp.PixelToMapPoint(this.Bonf.posx, this.Bonf.posy);
			base.Mp.PtcSTsetVar("x", (double)vector.x).PtcSTsetVar("y", (double)vector.y).PtcST("checkpoint_registered_bonfire", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			this.Bonf.fnDrawOnEffect(Ef, Ed);
			return true;
		}

		protected bool fnDrawToChip(EffectItem Ef, M2DrawBinder Ed)
		{
			this.Bonf.fnDrawToChip(Ef, Ed);
			if (this.anm_t >= 0)
			{
				int num = ((int)base.Mp.floort - this.anm_t) / 4;
				if (num >= MTR.AEfKagaribiActivate.Length)
				{
					this.anm_t = -1;
				}
				else
				{
					MeshDrawer mesh = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.ADD, -1), true);
					mesh.Col = MTRX.ColWhite;
					mesh.RotaPF(0f, 11f, 1f, 1f, 0f, MTR.AEfKagaribiActivate[num], false, false, false, uint.MaxValue, false, 0);
				}
			}
			return true;
		}

		private int anm_t = -1;

		private readonly BonfireEffector Bonf;

		protected M2DrawBinder EdPt;
	}
}
