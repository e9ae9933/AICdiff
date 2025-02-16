using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class M2LpTreasureHunt : M2LpBreakable
	{
		public M2LpTreasureHunt(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		protected override void prepareChips()
		{
			this.pre_on = this.Meta.GetIE("pre_on", 1, 0) != 0;
			if (!this.pre_on)
			{
				return;
			}
			this.APuts = new List<M2Puts>();
			using (BList<int> blist = ListBuffer<int>.Pop(0))
			{
				for (int i = 0; i < this.mapw; i++)
				{
					for (int j = 0; j < this.maph; j++)
					{
						M2Pt pointPuts = this.Mp.getPointPuts(this.mapx + i, this.mapy + j, false, false);
						if (pointPuts != null && (pointPuts.isBlockSlope() || pointPuts.cfg == 128))
						{
							int count = pointPuts.count;
							for (int k = 0; k < count; k++)
							{
								M2Chip m2Chip = pointPuts[k] as M2Chip;
								if (m2Chip != null && (m2Chip.Lay == this.Lay || blist.IndexOf(m2Chip.index) < 0) && base.isContainingMapXy((float)m2Chip.mapx, (float)m2Chip.mapy, (float)((int)m2Chip.mright), (float)((int)m2Chip.mbottom)) && m2Chip.getConfig(this.mapx + i - m2Chip.mapx, this.mapy + j - m2Chip.mapy) == pointPuts.cfg)
								{
									blist.Add(m2Chip.index);
									M2Chip m2Chip2 = this.Lay.MakeChip(m2Chip.Img, m2Chip.drawx, m2Chip.drawy, m2Chip.opacity, m2Chip.rotation, m2Chip.flip) as M2Chip;
									m2Chip2.pattern = m2Chip.pattern;
									m2Chip2.arrangeable = true;
									this.Lay.assignNewMapChip(m2Chip2, -1, true, false);
									this.APuts.Add(m2Chip2);
								}
							}
						}
					}
				}
			}
		}

		protected override void initActionMeta(out int maxhp)
		{
			this.type = M2LpBreakable.BREAKT.BOMB;
			maxhp = 1;
			this.do_not_bind_BCC_ = true;
			this.ev_jump = this.Meta.GetS("ev");
		}

		public override void closeAction(bool when_map_close = false)
		{
			if (this.APuts != null)
			{
				for (int i = this.APuts.Count - 1; i >= 0; i--)
				{
					M2Puts m2Puts = this.APuts[i];
					this.Lay.removeChip(m2Puts, true, true);
				}
			}
			if (this.Dro != null)
			{
				this.Dro.destruct(true);
				this.Dro = null;
				this.Mp.destructEvent(this.DroEv);
				this.DroEv = null;
			}
			base.closeAction(when_map_close);
			this.APuts = null;
		}

		public override void initBreak(bool on_initaction = false)
		{
			base.setLayerContentVisible(true, !base.do_not_bind_BCC);
			if (!on_initaction)
			{
				if (this.Dro == null)
				{
					this.Dro = this.Mp.setDrop(new M2DropObject.FnDropObjectDraw(this.fnDrawDrop), base.mapfocx, base.mapfocy, 0f, 0f, X.XORSPS() * 6.2831855f * 0.05f, -1f);
					this.Dro.size = 0.24f;
					this.Dro.FnRun = new M2DropObject.FnDropObjectRun(this.fnRunDrop);
					this.Dro.type = (DROP_TYPE)944;
					this.DroEv = this.Lay.Mp.getEventContainer().CreateAndAssign(base.unique_key);
					this.DroEv.Size(0.74f * base.CLENB, 1.04f * base.CLENB, ALIGN.CENTER, ALIGNY.MIDDLE, false);
					this.PFTreasure = MTRX.getPF("retro_treasure_box");
					using (STB stb = TX.PopBld(null, 0))
					{
						this.initEventContent(stb);
						this.DroEv.assign(M2EventItem.CMD.CHECK, stb.ToString(), false);
					}
					if (this.Mp.Pr != null)
					{
						this.Mp.Pr.need_check_event = (this.Mp.Pr.need_check_talk_event_target = true);
					}
				}
				this.Dro.bounce_x_reduce = 0f;
				this.Dro.bounce_y_reduce = 0.02f;
				this.Dro.gravity_scale = 0.6f;
				int num = ((this.aim < 0) ? 1 : this.aim);
				int num2 = CAim._XD(num, 1);
				int num3 = CAim._YD(num, 1);
				if (num2 != 0)
				{
					this.Dro.x = base.mapcx + (base.w * this.Mp.rCLEN * 0.5f + 0.5f) * (float)num2;
					this.Dro.vx = (float)num2 * 0.014f;
				}
				if (num3 != 0)
				{
					this.Dro.y = base.mapcy - (base.h * this.Mp.rCLEN * 0.5f + 0.5f) * (float)num3;
					this.Dro.vy = (float)(-(float)num3) * 0.06f;
				}
				this.DroEv.setTo(this.Dro.x, this.Dro.y);
			}
		}

		public override void activate()
		{
			if (this.Dro != null)
			{
				this.Dro.destruct(true);
				this.Dro = null;
				COOK.setSF(base.sf_key, 1);
			}
			if (this.DroEv != null)
			{
				this.DroEv.destruct();
				this.DroEv = null;
			}
		}

		private void initEventContent(STB Stb)
		{
			Stb.AR("#< % >");
			Stb.Add("#MS '>>[#<", base.unique_key, "> <<0.08] P[collect~~]").Add(" @#[", base.unique_key, ",1]'").Ret("\n");
			Stb.AR("WAIT 30");
			Stb.Add("LP_ACTIVATE ", this.Lay.name, " ").AR(this.key);
			if (TX.valid(this.ev_jump))
			{
				Stb.Add("CHANGE_EVENT2 ", this.ev_jump).Ret("\n");
			}
			Stb.AR("#MS 'P[crouch2stand]'");
		}

		public bool fnRunDrop(M2DropObject Dro, float fcnt)
		{
			if (this.Dro == null)
			{
				return false;
			}
			if ((Dro.x != this.x || Dro.y != base.y) && this.Mp.Pr != null)
			{
				this.Mp.Pr.need_check_event = true;
				if (this.Mp.TalkTarget == this.DroEv)
				{
					this.Mp.Pr.need_check_talk_event_target = true;
				}
			}
			this.DroEv.setTo(Dro.x, Dro.y);
			return true;
		}

		public bool fnDrawDrop(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.GetMesh("", this.Mp.Dgn.getWithLightTextureMaterial(MTRX.MIicon, null, -1), false).RotaPF(0f, 0f, 1f, 1f, Dro.z, this.PFTreasure, false, false, false, uint.MaxValue, false, 0);
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.ADD, -1), false);
			float num = X.Mx(0f, 0.1f + 0.2f * X.COSI(this.Mp.floort, 90f));
			mesh.Col = C32.WMulA(num);
			mesh.RotaPF(0f, 0f, 1f, 1f, Dro.z, this.PFTreasure, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		private M2DropObject Dro;

		private M2EventItem DroEv;

		private PxlFrame PFTreasure;

		private string ev_jump;

		private const float DRO_SIZE = 0.24f;
	}
}
