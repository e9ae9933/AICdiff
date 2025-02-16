using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpItemSupplier : M2LpEvent
	{
		public M2LpItemSupplier(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.ASupplyPositionPuts = null;
			this.ASupplyPositionPuts = this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, this.ASupplyPositionPuts, ulong.MaxValue, "item_supply");
			if (!SupplyManager.GetForLpSupplier(this.key, out this.IReel))
			{
				X.de("アイテムリール取得失敗: (reel_supply.csv に記述すること) ", null);
			}
			this.max_count = this.Meta.GetI("count", 1, 0);
			this.is_reel = !this.Meta.GetB("no_reel", false);
			this.player_aim = this.Meta.GetI("player_aim", -1000, 0);
			Vector2 zero = Vector2.zero;
			int num = 0;
			if (this.ASupplyPositionPuts != null)
			{
				for (int i = this.ASupplyPositionPuts.Count - 1; i >= 0; i--)
				{
					M2Puts m2Puts = this.ASupplyPositionPuts[i];
					m2Puts.arrangeable = true;
					if (m2Puts.getMeta().GetI("item_supply", 0, 0) >= 0)
					{
						num++;
						zero.x += m2Puts.mapcx;
						zero.y += m2Puts.mapcy;
					}
				}
			}
			this.EffectFocusPos = ((num == 0) ? new Vector2(base.mapfocx, base.mapfocy + 0.5f) : (zero / (float)num));
			this.already_collected = !this.infinity_supply && COOK.getSF(this.sf_key) >= this.max_count;
		}

		public override void initAction(bool normal_map)
		{
			if (this.IReel == null)
			{
				return;
			}
			base.initAction(normal_map);
		}

		public string sf_key
		{
			get
			{
				return "ISUPPLY_" + this.Mp.key + "_" + base.unique_key;
			}
		}

		public bool infinity_supply
		{
			get
			{
				return this.max_count < 0;
			}
		}

		protected override M2EventItem createEvent()
		{
			return this.Lay.Mp.getEventContainer().CreateAndAssignT<M2EventItem_ItemSupply>(base.unique_key, false);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (when_map_close && this.ASupplyPositionPuts != null)
			{
				for (int i = this.ASupplyPositionPuts.Count - 1; i >= 0; i--)
				{
					this.ASupplyPositionPuts[i].remActiveRemoveKey(base.unique_key, false);
				}
				this.ASupplyPositionPuts = null;
			}
			if (this.EdKira != null)
			{
				this.EdKira.destruct();
				this.EdKira = null;
			}
		}

		public override bool EvtM2Reload(Map2d Mp)
		{
			if (this.IReel == null)
			{
				return true;
			}
			this.EvS = null;
			base.EvtM2Reload(Mp);
			if (this.Ev == null)
			{
				return true;
			}
			this.EvS = this.Ev as M2EventItem_ItemSupply;
			this.EvS.hitable = this.Meta.GetB("hitable", false);
			bool flag = !this.already_collected;
			float y = this.EffectFocusPos.y;
			string text = (this.is_reel ? string.Concat(new string[]
			{
				"INIT_ITEM_REEL ",
				this.IReel.key,
				" ~noelRPI[REEL_SPEED]==0 ",
				this.Lay.name,
				"..",
				this.key
			}) : ("LP_ACTIVATE " + this.Lay.name + ".." + this.key));
			this.EvS.LpCon = this;
			this.EvS.IReel = this.IReel;
			if (this.EvS.hitable)
			{
				this.EvS.setToArea(this);
				this.Ev.check_desc_name = (this.already_collected ? "!EV_access_collected_already" : "!EV_access_collect_by_attack");
				flag = false;
				text = "STOP_LETTERBOX\nVALOTIZE\nDENY_SKIP\n" + text;
			}
			else
			{
				if (this.Ev.sizey >= 1.5f)
				{
					this.Ev.Size(this.Ev.sizex * base.CLEN, 1.5f * base.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
					this.Ev.setTo(base.mapcx, (float)(this.mapy + this.maph) - this.Ev.sizey);
				}
				this.Ev.check_desc_name = (this.already_collected ? "EV_access_collected_already" : "EV_access_collect");
				string text2 = "STOP_LETTERBOX\nVALOTIZE\nDENY_SKIP\n#< % >\n";
				if (this.player_aim != -1000)
				{
					text2 = text2 + "_aim=" + this.player_aim.ToString() + "\n";
				}
				else
				{
					text2 = text2 + "IF 'm2d_current_y>" + y.ToString() + "' {\n_aim=-1 \n} ELSE { \n_aim=1\n}\n";
				}
				text = string.Concat(new string[]
				{
					text2,
					"#MS 'P[collect] @x[",
					base.mapfocx.ToString(),
					", '$_aim'] '\n",
					text
				});
			}
			text = this.checkPreAftEvent(false, text);
			this.EvS.assign("CHECK", text, false);
			this.EvS.setExecutable(M2EventItem.CMD.CHECK, flag);
			if (this.already_collected)
			{
				this.activate(null, false, true);
				if (this.EdKira != null)
				{
					this.EdKira.destruct();
					this.EdKira = null;
				}
			}
			else
			{
				if (this.EdKira == null)
				{
					this.EdKira = Mp.setED(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawKira), 0f);
					this.RcKira = new DRect("");
					if (this.ASupplyPositionPuts != null && this.ASupplyPositionPuts.Count > 0)
					{
						for (int i = this.ASupplyPositionPuts.Count - 1; i >= 0; i--)
						{
							M2Puts m2Puts = this.ASupplyPositionPuts[i];
							float num = (float)m2Puts.iwidth * Mp.rCLEN * 0.5f;
							float num2 = (float)m2Puts.iheight * Mp.rCLEN * 0.5f;
							this.RcKira.Expand((float)m2Puts.drawx * Mp.rCLEN + num, (float)m2Puts.drawy * Mp.rCLEN + num2, num * 2f, num2 * 2f, false);
						}
					}
					else
					{
						this.RcKira.Expand(base.mapfocx - 1f, base.mapfocy - 1f, 2f, 2f, false);
					}
				}
				if (M2LpItemSupplier.PtcKira == null)
				{
					M2LpItemSupplier.PtcKira = new EfParticleLooper("supplier_kira");
				}
			}
			if (this.ASupplyPositionPuts != null)
			{
				for (int j = this.ASupplyPositionPuts.Count - 1; j >= 0; j--)
				{
					M2Puts m2Puts2 = this.ASupplyPositionPuts[j];
					m2Puts2.arrangeable = true;
					M2CImgDrawerItemSupply m2CImgDrawerItemSupply = m2Puts2.CastDrawer<M2CImgDrawerItemSupply>();
					if (m2CImgDrawerItemSupply != null)
					{
						m2CImgDrawerItemSupply.LpCon = this;
					}
				}
			}
			return true;
		}

		public string checkPreAftEvent(bool stack = false, string ev_content = null)
		{
			string text = this.Meta.GetS("ev_pre");
			if (TX.valid(text))
			{
				if (ev_content != null)
				{
					ev_content = "CHANGE_EVENT2 " + text + "\n" + ev_content;
				}
				if (stack)
				{
					EV.stack(text, 0, -1, null, null);
				}
			}
			text = this.Meta.GetS("ev_aft");
			if (TX.valid(text))
			{
				if (ev_content != null)
				{
					ev_content = ev_content + "\nCHANGE_EVENT " + text;
				}
				if (stack)
				{
					EV.stack(text, 0, -1, null, null);
				}
			}
			return ev_content;
		}

		public override void activate()
		{
			this.activate((this.is_reel || this.IReel == null) ? null : this.IReel.ToArray(), true, true);
		}

		public void activate(NelItemEntry[] ADrop, bool add_count, bool activate_to_chip)
		{
			if (add_count && this.already_collected)
			{
				return;
			}
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (ADrop != null)
			{
				int num = 0;
				for (int i = ADrop.Length - 1; i >= 0; i--)
				{
					NelItemEntry nelItemEntry = ADrop[i];
					int j = nelItemEntry.count;
					bool flag = j > nelItemEntry.Data.stock || nelItemEntry.Data.is_water;
					while (j > 0)
					{
						int num2 = ((j > nelItemEntry.Data.stock) ? nelItemEntry.Data.stock : (flag ? j : 1));
						M2Puts m2Puts = null;
						float num4;
						float num5;
						if (this.ASupplyPositionPuts != null && this.ASupplyPositionPuts.Count > 0)
						{
							int num3 = num % this.ASupplyPositionPuts.Count;
							m2Puts = this.ASupplyPositionPuts[num3];
							num4 = m2Puts.mapcx;
							num5 = m2Puts.mapcy;
						}
						else
						{
							num4 = base.mapfocx;
							num5 = base.mapfocy;
						}
						float num6 = X.NIXP(0f, 0.05f);
						float num7 = X.XORSPS() * 3.1415927f;
						if (this.ASupplyPositionPuts != null && num >= this.ASupplyPositionPuts.Count && this.ASupplyPositionPuts.Count > 1)
						{
							int num8 = (num + 1 + X.xors(this.ASupplyPositionPuts.Count - 1)) % this.ASupplyPositionPuts.Count;
							M2Puts m2Puts2 = this.ASupplyPositionPuts[num8];
							num4 = X.NI(num4, m2Puts2.x, X.XORSP());
							num5 = X.NI(num5, m2Puts2.y, X.XORSP());
						}
						num++;
						NelItemManager.NelItemDrop nelItemDrop = nelM2DBase.IMNG.dropManual(nelItemEntry.Data, num2, (int)nelItemEntry.grade, num4, num5, num6 * X.Cos(num7), -num6 * X.Sin(num7), null, false, NelItemManager.TYPE.ABSORB);
						if (m2Puts != null && m2Puts.Img.Meta.Get("drop_drawer") != null)
						{
							nelItemDrop.ChipPImg = m2Puts.Img.getMainMesh();
						}
						j -= num2;
					}
				}
			}
			int num9 = COOK.getSF(this.sf_key);
			if (add_count)
			{
				COOK.setSF(this.sf_key, ++num9);
				if (num9 >= this.max_count && !this.infinity_supply)
				{
					this.already_collected = true;
					if (this.EvS != null)
					{
						this.EvS.check_desc_name = (this.EvS.hitable ? "!EV_access_collected_already" : "EV_access_collected_already");
						this.EvS.setExecutable(M2EventItem.CMD.CHECK, false);
					}
				}
			}
			if (activate_to_chip && this.Mp.floort >= 5f)
			{
				List<M2Puts> list = null;
				int num10 = 0;
				if (this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, list, null) != null)
				{
					for (int k = 0; k < num10; k++)
					{
						M2Puts m2Puts3 = list[k];
						if (!m2Puts3.active_removed)
						{
							m2Puts3.activateToDrawer();
						}
					}
				}
				if (this.Meta.Get("ptcst_key") != null)
				{
					string s = this.Meta.GetS("ptcst_key");
					this.Mp.M2D.curMap.PtcSTsetVar("x", (double)(this.EffectFocusPos.x + this.Meta.GetNm("ptcst_key", 0f, 1))).PtcSTsetVar("y", (double)(this.EffectFocusPos.y + this.Meta.GetNm("ptcst_key", 0f, 2))).PtcST2Base(s, 0f, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (this.ASupplyPositionPuts != null && this.ASupplyPositionPuts.Count > 0 && this.already_collected)
			{
				for (int l = this.ASupplyPositionPuts.Count - 1; l >= 0; l--)
				{
					M2Puts m2Puts4 = this.ASupplyPositionPuts[l];
					if (X.Abs(m2Puts4.getMeta().GetI("item_supply", 0, 0)) >= 2)
					{
						m2Puts4.addActiveRemoveKey(base.unique_key, false);
					}
				}
			}
		}

		public void setHitEffect()
		{
			if (this.ASupplyPositionPuts != null && this.ASupplyPositionPuts.Count > 0)
			{
				for (int i = this.ASupplyPositionPuts.Count - 1; i >= 0; i--)
				{
					M2Puts m2Puts = this.ASupplyPositionPuts[i];
					string s = m2Puts.Img.Meta.GetS("hit_ptcst");
					if (TX.valid(s))
					{
						this.Mp.M2D.curMap.PtcSTsetVar("cx", (double)m2Puts.mapcx).PtcSTsetVar("cy", (double)m2Puts.mapcy).PtcST(s, null, PTCThread.StFollow.NO_FOLLOW);
					}
				}
				return;
			}
			string s2 = this.Meta.GetS("hit_ptcst");
			if (TX.valid(s2))
			{
				this.Mp.M2D.curMap.PtcSTsetVar("cx", (double)base.mapfocx).PtcSTsetVar("cy", (double)base.mapfocy).PtcST(s2, null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		private bool fnDrawKira(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.already_collected)
			{
				this.EdKira = null;
				return false;
			}
			Ef.x = this.RcKira.cx;
			Ef.y = this.RcKira.cy;
			if (!Ed.isinCamera(Ef, this.RcKira.w * 0.5f + 2f, this.RcKira.h * 0.5f + 2f))
			{
				return true;
			}
			Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			Ef.z = this.RcKira.w * 0.5f * this.Mp.CLENB / 0.35f;
			Ef.time = (int)(this.RcKira.h * 0.5f * this.Mp.CLENB / 0.35f);
			M2LpItemSupplier.PtcKira.Draw(Ef, M2LpItemSupplier.PtcKira.delay * M2LpItemSupplier.PtcKira.count * 0.5f + (float)(M2LpItemSupplier.PtcKira.maxt / 2));
			return true;
		}

		private int player_aim = -1000;

		private ReelManager.ItemReelContainer IReel;

		private int max_count = 1;

		private List<M2Puts> ASupplyPositionPuts;

		private WMIconCreator WmIco;

		private M2EventItem_ItemSupply EvS;

		public const string sf_header = "ISUPPLY_";

		public bool already_collected;

		public bool is_reel;

		private Vector2 EffectFocusPos;

		private M2DrawBinder EdKira;

		private static EfParticleLooper PtcKira;

		private DRect RcKira;
	}
}
