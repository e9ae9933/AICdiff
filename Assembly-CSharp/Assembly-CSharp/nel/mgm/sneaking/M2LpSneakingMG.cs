using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using XX;

namespace nel.mgm.sneaking
{
	public class M2LpSneakingMG : NelLpRunner, IEventListener, IListenerEvcReload, IM2DebugTarget, NelItemManager.IPickupListener
	{
		public M2LpSneakingMG(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.ABright = new List<M2LpSneakingMG.BrightArea>(8);
			this.AMovePat = new List<M2MovePatSneaker>(8);
			this.AMovePatChecking = new List<M2MovePatSneaker>(3);
			this.ADark = new List<M2LpSneakingMG.DarkArea>(16);
			this.ODropEvent = new BDic<NelItemManager.NelItemDrop, string>(1);
			this.OAWalkArea = new BDic<int, M2LpSneakingMG.LigAreaContainer>(3);
			this.AStair = new List<M2LabelPoint>(3);
		}

		public override void initActionPre()
		{
			base.initActionPre();
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			base.initAction(normal_map);
			this.Mp.getEventContainer().addReloadListener(this);
			EV.addListener(this);
		}

		public override void closeAction(bool normal_map)
		{
			base.closeAction(normal_map);
			this.Mp.getEventContainer().remReloadListener(this);
			EV.remListener(this);
			this.closeMG();
			if (this.PrRunMng != null)
			{
				this.PrRunMng.destruct();
			}
		}

		private void closeMG()
		{
			for (int i = this.AMovePat.Count - 1; i >= 0; i--)
			{
				this.AMovePat[i].Mv.destructMovePat();
			}
			for (int j = this.ADark.Count - 1; j >= 0; j--)
			{
				this.ADark[j].destruct();
			}
			for (int k = this.ABright.Count - 1; k >= 0; k--)
			{
				this.ABright[k].destruct();
			}
			base.nM2D.IMNG.remPickupListener(this);
			foreach (KeyValuePair<NelItemManager.NelItemDrop, string> keyValuePair in this.ODropEvent)
			{
				if (keyValuePair.Key.Dro != null)
				{
					base.nM2D.IMNG.removeItData(keyValuePair.Key.Dro, null, false);
				}
			}
			this.Mp.MovRenderer.z_cm_override = -1000f;
			this.OAWalkArea.Clear();
			this.ABright.Clear();
			this.ODropEvent.Clear();
			this.AMovePatChecking.Clear();
			this.AStair.Clear();
			this.AMovePat.Clear();
			this.ADark.Clear();
			this.Mp.remRunnerObject(this);
			BGM.remHalfFlag("SNEAK_MG");
			if (this.PrRunMng != null)
			{
				this.PrRunMng.removeEffect();
				this.PrRunMng.deactivateUI();
			}
		}

		public bool EvtM2Reload(Map2d Mp)
		{
			this.closeMG();
			if (this.Meta.GetBE("is_active", true))
			{
				this.activateMG();
			}
			return true;
		}

		public override void activate()
		{
			base.activate();
			if (this.ABright.Count == 0)
			{
				this.activateMG();
			}
		}

		public override void deactivate()
		{
			base.deactivate();
			this.closeMG();
		}

		private void activateMG()
		{
			string s = this.Meta.GetS("npc_behind_submap");
			if (TX.valid(s))
			{
				Map2d map2d = base.nM2D.Get(s, false);
				if (map2d != null && map2d.mode == MAPMODE.SUBMAP && map2d.SubMapData != null)
				{
					float num = this.Mp.gameObject.transform.localPosition.z + map2d.SubMapData.Pos.z + 0.1f;
					this.Mp.MovRenderer.z_cm_override = num;
				}
			}
			string s2 = this.Meta.GetS("bgm");
			if (TX.valid(s2))
			{
				BGM.load(s2, null, false);
				BGM.replace(80f, 80f, true, false);
			}
			this.heartbeat_maxt = 80f;
			this.t_heart = (EV.isActive(false) ? (-2f) : (200f - this.heartbeat_maxt));
			this.Mp.addRunnerObject(this);
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			int length = this.Lay.LP.Length;
			int i = 0;
			int num2 = eventContainer.countEvents();
			int num3 = this.mapx;
			int num4 = this.mapx + this.mapw;
			for (int j = 0; j < length; j++)
			{
				M2LabelPoint m2LabelPoint = this.Lay.LP.Get(j);
				if (TX.headerIs(m2LabelPoint.key, "Bright", 0, '_', true))
				{
					this.ABright.Add(new M2LpSneakingMG.BrightArea(m2LabelPoint));
					num3 = X.Mn(num3, m2LabelPoint.mapx);
					num4 = X.Mx(num4, m2LabelPoint.mapx + m2LabelPoint.mapw);
				}
				else if (TX.headerIs(m2LabelPoint.key, "Sneaker", 0, '_', true))
				{
					if (m2LabelPoint.comment.IndexOf("no_first_assign") < 0 && new META(m2LabelPoint.comment).GetBE("is_active", true))
					{
						bool flag = false;
						while (i < num2)
						{
							M2EventItem m2EventItem = eventContainer.Get(i);
							if (TX.headerIs(m2EventItem.key, "sneaker", 0, '_', true))
							{
								i++;
								M2MovePatSneaker m2MovePatSneaker = new M2MovePatSneaker(this, m2EventItem, m2LabelPoint);
								this.AMovePat.Add(m2MovePatSneaker);
								flag = true;
								break;
							}
							i++;
						}
						if (!flag)
						{
							X.de("Sneaker エリアに対してNPCが不足しています", null);
						}
					}
				}
				else if (TX.headerIs(m2LabelPoint.key, "ItemDropS", 0, '_', true))
				{
					META meta = new META(m2LabelPoint.comment);
					string s3 = meta.GetS("item");
					if (TX.valid(s3) && meta.GetBE("is_active", true))
					{
						NelItem byId = NelItem.GetById(s3, false);
						if (byId != null)
						{
							int num5 = meta.GetI("item", 1, 1);
							if (num5 < 0)
							{
								num5 = byId.stock;
							}
							int i2 = meta.GetI("grade", 2, 0);
							if (base.nM2D.IMNG.countItem(byId, i2, false, false) < num5)
							{
								NelItemManager.NelItemDrop nelItemDrop = base.nM2D.IMNG.dropManual(byId, num5, i2, m2LabelPoint.mapfocx, (float)(m2LabelPoint.mapy + m2LabelPoint.maph) - 0.35f, 0f, 0f, null, false, NelItemManager.TYPE.NORMAL);
								nelItemDrop.Dro.bounce_y_reduce = 0f;
								nelItemDrop.discarded = true;
								string text = meta.GetS("pickup_event") ?? "";
								this.ODropEvent[nelItemDrop] = text;
								if (this.ODropEvent.Count == 1)
								{
									base.nM2D.IMNG.addPickupListener(this);
								}
							}
						}
					}
				}
				else if (TX.isStart(m2LabelPoint.key, "Stair", 0))
				{
					this.AStair.Add(m2LabelPoint);
				}
			}
			if (i < num2)
			{
				string s4 = this.Meta.GetS("mv_hide_pos");
				if (TX.valid(s4))
				{
					M2LabelPoint point = this.Mp.getPoint(s4, false);
					if (point != null)
					{
						while (i < num2)
						{
							M2EventItem m2EventItem2 = eventContainer.Get(i);
							if (TX.headerIs(m2EventItem2.key, "sneaker", 0, '_', true))
							{
								m2EventItem2.setTo(point.mapfocx, point.mapfocy);
							}
							i++;
						}
					}
				}
			}
			this.ABright.Sort(delegate(M2LpSneakingMG.BrightArea A, M2LpSneakingMG.BrightArea B)
			{
				if (A.bottom != B.bottom)
				{
					return (int)A.bottom - (int)B.bottom;
				}
				return (int)A.x - (int)B.x;
			});
			num2 = this.AMovePat.Count;
			for (int k = 0; k < num2; k++)
			{
				M2MovePatSneaker m2MovePatSneaker2 = this.AMovePat[k];
				int num6 = (int)(m2MovePatSneaker2.dep_bottomy - 0.5f);
				M2LpSneakingMG.LigAreaContainer ligAreaContainer;
				if (this.OAWalkArea.TryGetValue(num6, out ligAreaContainer))
				{
					ligAreaContainer.AMvConnected.Add(m2MovePatSneaker2);
				}
				else
				{
					ligAreaContainer = (this.OAWalkArea[num6] = new M2LpSneakingMG.LigAreaContainer(8));
					ligAreaContainer.AMvConnected.Add(m2MovePatSneaker2);
					M2LpSneakingMG.DarkArea darkArea = null;
					M2LpSneakingMG.BrightArea brightArea = null;
					for (int l = num3; l < num4; l++)
					{
						M2LpSneakingMG.BrightArea brightArea2 = this.isinBrightArea((float)l, (float)num6);
						if (brightArea2 != null)
						{
							if (darkArea != null)
							{
								darkArea.Expand(brightArea2.x, darkArea.cy, 0f, 0f, true);
								if (darkArea.right >= brightArea2.x)
								{
									darkArea.width -= darkArea.right - brightArea2.x;
								}
								darkArea = null;
							}
							l = X.Mx(l, (int)(brightArea2.right - 0.05f));
						}
						else
						{
							if (darkArea == null)
							{
								darkArea = new M2LpSneakingMG.DarkArea((float)l, (float)(num6 - 3 + 1));
								this.ADark.Add(darkArea);
								if (brightArea != null)
								{
									darkArea.Expand(brightArea.right, darkArea.cy, 0f, 0f, true);
								}
								ligAreaContainer.Add(darkArea);
							}
							darkArea.width += 1f;
						}
						if (brightArea != brightArea2)
						{
							brightArea = brightArea2;
							if (brightArea2 != null)
							{
								ligAreaContainer.Add(brightArea2);
							}
						}
					}
				}
			}
			M2LightFn.FnDrawLight fnDrawLight = new M2LightFn.FnDrawLight(this.fnDrawBright);
			for (int m = this.ABright.Count - 1; m >= 0; m--)
			{
				this.ABright[m].prepareLight(this.Mp, fnDrawLight);
			}
			fnDrawLight = new M2LightFn.FnDrawLight(this.fnDrawDark);
			for (int n = this.ADark.Count - 1; n >= 0; n--)
			{
				this.ADark[n].prepareLight(this.Mp, new M2LightFn.FnDrawLight(this.fnDrawDark));
			}
			for (int num7 = this.Mp.count_players - 1; num7 >= 0; num7--)
			{
				PR pr = this.Mp.getPr(num7) as PR;
				if (pr.MyLight != null)
				{
					pr.MyLight.alpha_rgb = 0.25f;
					pr.MyLight.radius = 90f;
					pr.MyLight.fill_radius = 0f;
				}
			}
		}

		private M2LpSneakingMG.BrightArea isinBrightArea(float x, float y)
		{
			for (int i = this.ABright.Count - 1; i >= 0; i--)
			{
				M2LpSneakingMG.BrightArea brightArea = this.ABright[i];
				if (brightArea.isin(x, y, 0f))
				{
					return brightArea;
				}
			}
			return null;
		}

		internal int bright_area_count
		{
			get
			{
				return this.ABright.Count;
			}
		}

		internal M2LpSneakingMG.BrightArea getBrightArea(int i)
		{
			return this.ABright[i];
		}

		public override string ToString()
		{
			return "SneakingMG";
		}

		public override bool run(float fcnt)
		{
			foreach (KeyValuePair<int, M2LpSneakingMG.LigAreaContainer> keyValuePair in this.OAWalkArea)
			{
				M2LpSneakingMG.LigAreaContainer value = keyValuePair.Value;
				int count = value.AMvConnected.Count;
				int count2 = value.Count;
				int key = keyValuePair.Key;
				for (int i = 0; i < count2; i++)
				{
					M2LpSneakingMG.BrightArea brightArea = value[i] as M2LpSneakingMG.BrightArea;
					if (brightArea != null)
					{
						uint num = 0U;
						for (int j = 0; j < count; j++)
						{
							M2MovePatSneaker m2MovePatSneaker = value.AMvConnected[j];
							if (m2MovePatSneaker.isSightCovering(brightArea))
							{
								num |= 1U << j;
								brightArea.checkLightCovering(m2MovePatSneaker);
							}
						}
						if (this.PrRunMng != null)
						{
							this.PrRunMng.checkBrightArea(brightArea);
						}
						brightArea.run(this.Mp, fcnt);
						if (num != 0U && brightArea.isContainingXy(this.Mp.Pr.mleft, this.Mp.Pr.mbottom - 0.15f, this.Mp.Pr.mright, this.Mp.Pr.mbottom, 0.04f))
						{
							for (int k = 0; k < count; k++)
							{
								if ((num & (1U << k)) != 0U)
								{
									value.AMvConnected[k].checkSightPrCheck(this.Mp.Pr, brightArea, fcnt);
								}
							}
						}
					}
				}
			}
			if (this.t_heart >= 0f)
			{
				this.t_heart += fcnt;
				if (this.t_heart >= 200f)
				{
					SND.Ui.play("heartbeat_l", false);
					if (this.AMovePatChecking.Count == 0 && this.heartbeat_maxt < 80f)
					{
						bool flag = this.heartbeat_maxt < 50f;
						this.heartbeat_maxt = X.VALWALK(this.heartbeat_maxt, 80f, 5f);
						if (flag && this.heartbeat_maxt >= 50f)
						{
							BGM.fadein(100f, 180f);
						}
					}
					if (this.AMovePatChecking.Count > 0)
					{
						PostEffect.IT.setPEabsorbed(POSTM.JAMMING, 3f, 20f, 0.7f, 0);
					}
					if (this.AMovePatChecking.Count > 0)
					{
						this.Mp.M2D.Cam.Qu.HandShake(30f, 40f, 4f, 0);
					}
					this.t_heart = 200f - this.heartbeat_maxt;
				}
			}
			return true;
		}

		internal void initCheck(M2MovePatSneaker K)
		{
			if (this.AMovePatChecking.IndexOf(K) < 0)
			{
				NEL.PadVib("sneaking_noticed", 1f);
				this.AMovePatChecking.Add(K);
				this.Mp.getEffectTop().setE("flash", 1f, 30f, 0.25f, 0, 0);
				if (this.AMovePatChecking.Count == 1)
				{
					this.heartbeat_maxt = 35f;
					this.t_heart = 150f;
					BGM.fadeout(0f, 5f, false);
					this.Mp.M2D.Cam.Qu.HandShake(30f, 40f, 4f, 0);
				}
			}
		}

		internal void quitCheck(M2MovePatSneaker K)
		{
			int num = this.AMovePatChecking.IndexOf(K);
			if (num >= 0)
			{
				this.AMovePatChecking.RemoveAt(num);
				int count = this.AMovePatChecking.Count;
			}
		}

		internal void initAware(M2MovePatSneaker K)
		{
			if (this.t_heart >= 0f)
			{
				this.t_heart = -1f;
				string s = this.Meta.GetS("aware_event");
				if (TX.valid(s))
				{
					EV.stack(s, 0, -1, new string[] { K.Mv.key }, null);
				}
				if (this.PrRunMng != null)
				{
					this.PrRunMng.initAware();
				}
			}
		}

		public void DropPickup(NelItemManager.NelItemDrop ItData, bool pickuped)
		{
			string text;
			if (this.ODropEvent.TryGetValue(ItData, out text))
			{
				this.ODropEvent.Remove(ItData);
				if (pickuped && TX.valid(text))
				{
					EV.stack(text, 0, -1, new string[] { ItData.Itm.key }, null);
				}
			}
		}

		private void fnDrawBright(MeshDrawer MdL, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			y -= 1.5f * this.Mp.CLEN * 0.88f;
			float num = Lt.radius * 2f + 60f;
			float num2 = 2f * this.Mp.CLEN * 1.5f;
			MdL.Identity();
			MdL.Scale(Lt.radius * 2f / num, 1f, false).TranslateP(x, y, false);
			MdL.uvRect(-num * 0.5f * 0.015625f, -num2 * 0.5f * 0.015625f, num * 0.015625f, num2 * 0.015625f, MTRX.IconWhite, false, true);
			MdL.Col = MdL.ColGrd.Set(uint.MaxValue).mulA(X.NI(0.18f, 0.5f, Lt.alpha_rgb) * alpha).C;
			MdL.ColGrd.mulA(0f);
			MdL.KadomaruRect(0f, 0f, num, num2, num2, num2 * 0.34f, false, 1f, 0f, true);
			MdL.Identity();
		}

		private void fnDrawDark(MeshDrawer MdL, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			if (!MdL.hasMultipleTriangle())
			{
				MdL.chooseSubMesh(2, false, false);
				MdL.setMaterial(MTRX.getMtr(BLEND.SUB, -1), false);
			}
			MdL.chooseSubMesh(2, false, false);
			MdL.Col = MdL.ColGrd.Set(uint.MaxValue).mulA(0.8f).C;
			MdL.ColGrd.mulA(0f);
			MdL.KadomaruRect(x, y, Lt.radius * 2f + 45f, 3f * this.Mp.CLEN * 1.5f, 52.5f, this.Mp.CLEN * 0.75f * 1.5f, false, 1f, 0f, true);
			MdL.Col = C32.WMulA(0.4f);
			MdL.KadomaruRect(x, y, Lt.radius * 2f + 120f, 3f * this.Mp.CLEN * 1.5f * 1.5f, 142.5f, 3f * this.Mp.CLEN * 0.66f * 1.5f, false, 1f, 0f, true);
		}

		public bool isDestructed()
		{
			return this.AMovePat.Count == 0;
		}

		public bool isAware()
		{
			return this.t_heart == -1f;
		}

		bool IEventListener.EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			if (rER.cmd == "SNEAKINGMG")
			{
				string _ = rER._1;
				if (_ != null)
				{
					if (!(_ == "STOPNPC"))
					{
						if (!(_ == "CLOSE"))
						{
							if (!(_ == "SWITCH_NPCPOS"))
							{
								if (_ == "INIT_PR_RUNNING")
								{
									if (this.Mp.Pr is PR)
									{
										if (this.PrRunMng == null)
										{
											this.PrRunMng = new PrRunMGSneaking(this);
										}
										this.PrRunMng.activate();
									}
								}
							}
							else
							{
								for (int i = this.AMovePat.Count - 1; i >= 0; i--)
								{
									M2MovePatSneaker m2MovePatSneaker = this.AMovePat[i];
									if (m2MovePatSneaker.Mv.key == rER._2)
									{
										M2LabelPoint m2LabelPoint = this.Lay.LP.Get(rER._3, true, true);
										if (m2LabelPoint == null)
										{
											rER.tError("Lpが存在しません: " + rER._3);
										}
										else
										{
											m2MovePatSneaker.switchLp(m2LabelPoint, true);
										}
										return true;
									}
								}
								rER.tError("NPCが MovePat に存在しません: " + rER._2);
							}
						}
						else
						{
							this.closeMG();
						}
					}
					else
					{
						for (int j = this.AMovePat.Count - 1; j >= 0; j--)
						{
							this.AMovePat[j].StopFromEvent();
						}
					}
				}
				return true;
			}
			return false;
		}

		bool IEventListener.EvtOpen(bool is_first_or_end)
		{
			if (is_first_or_end && this.PrRunMng != null)
			{
				this.PrRunMng.EvtOpen();
			}
			return true;
		}

		bool IEventListener.EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				if (this.Mp.Pr is PR && this.PrRunMng != null && this.PrRunMng.isEffectActive())
				{
					this.PrRunMng.SpPrRefine(this.Mp.Pr as PR, true);
				}
				if (this.t_heart == -2f)
				{
					this.t_heart = 200f - this.heartbeat_maxt;
				}
			}
			return true;
		}

		int IEventListener.EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		bool IEventListener.EvtMoveCheck()
		{
			return true;
		}

		private const string lp_sneaker_walkable_header = "Sneaker";

		private const string lp_bright_area_header = "Bright";

		internal const string lp_stair_area = "Stair";

		internal const string lp_stair_area_btm = "StairB";

		private const string lp_itemdrop = "ItemDropS";

		private const string npc_event_header = "sneaker";

		private const int MESH_ID_DARKEN = 2;

		private const int dark_height = 3;

		private readonly List<M2LpSneakingMG.BrightArea> ABright;

		private readonly List<M2LpSneakingMG.DarkArea> ADark;

		internal readonly List<M2LabelPoint> AStair;

		private readonly List<M2MovePatSneaker> AMovePat;

		private readonly List<M2MovePatSneaker> AMovePatChecking;

		internal readonly BDic<int, M2LpSneakingMG.LigAreaContainer> OAWalkArea;

		private readonly BDic<NelItemManager.NelItemDrop, string> ODropEvent;

		private const float heartbeat_attack_time = 200f;

		private const float heartbeat_default_maxt = 80f;

		private const float heartbeat_checked_maxt = 35f;

		private float heartbeat_maxt = 80f;

		private float t_heart;

		private PrRunMGSneaking PrRunMng;

		internal class LigArea : DRect
		{
			public LigArea(string _key, float a, float b = 0f, float _w = 0f, float _h = 0f)
				: base(_key, a, b, _w, _h, 0f)
			{
			}

			public void prepareLight(Map2d Mp, M2LightFn.FnDrawLight FD)
			{
				this.Lig = new M2LightFn(Mp, FD, null, null)
				{
					mapx = base.cx,
					mapy = base.cy,
					radius = base.w * 0.5f * Mp.CLEN
				};
				Mp.addLight(this.Lig, 0);
			}

			public bool isOnBorder(M2Mover Mv, AIM a)
			{
				if (a == AIM.R)
				{
					return Mv.mright > base.right - 0.15f;
				}
				return a == AIM.L && Mv.mleft < this.x + 0.15f;
			}

			public void destruct()
			{
				if (this.Lig != null)
				{
					this.Lig.Mp.remLight(this.Lig);
					this.Lig = null;
				}
			}

			protected M2LightFn Lig;
		}

		internal sealed class BrightArea : M2LpSneakingMG.LigArea
		{
			public BrightArea(M2LabelPoint Lp)
				: base(Lp.key, Lp.x * Lp.Mp.rCLEN, Lp.y * Lp.Mp.rCLEN, Lp.w * Lp.Mp.rCLEN, Lp.h * Lp.Mp.rCLEN)
			{
			}

			internal void checkLightCovering(M2MovePatSneaker Mv)
			{
				Map2d mp = Mv.Mp;
				if (this.fined_floort < mp.floort)
				{
					this.fined_floort = mp.floort;
					this.bright_level = 0f;
				}
				this.bright_level = X.Mx(Mv.getBrightLevel(this), this.bright_level);
			}

			internal void ScrBright(Map2d Mp, float l)
			{
				if (this.fined_floort < Mp.floort)
				{
					this.fined_floort = Mp.floort;
					this.bright_level = 0f;
				}
				this.bright_level = X.Scr(this.bright_level, l);
			}

			public void run(Map2d Mp, float fcnt)
			{
				if (this.fined_floort < Mp.floort)
				{
					this.bright_level = 0f;
				}
				this.Lig.alpha_rgb = X.VALWALK(this.Lig.alpha_rgb, this.bright_level, 0.02f * fcnt);
			}

			public float bright_level;

			public float fined_floort = -1f;
		}

		internal sealed class DarkArea : M2LpSneakingMG.LigArea
		{
			public DarkArea(float x, float y)
				: base("Dark", x, y, 0f, 3f)
			{
			}
		}

		internal class LigAreaContainer : List<M2LpSneakingMG.LigArea>
		{
			public LigAreaContainer(int capacity)
				: base(capacity)
			{
				this.AMvConnected = new List<M2MovePatSneaker>(3);
			}

			public readonly List<M2MovePatSneaker> AMvConnected;
		}
	}
}
