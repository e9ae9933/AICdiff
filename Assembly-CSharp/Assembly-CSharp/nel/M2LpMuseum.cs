using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2LpMuseum : NelLp, IRunAndDestroy, IEventListener, IEventWaitListener, IListenerEvcReload
	{
		public M2LpMuseum(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			this.Mp.getEventContainer().addReloadListener(this);
			int num = this.Lay.count_chips;
			this.Opat2key = new BDic<uint, M2LpMuseum.PChips>();
			for (int i = 0; i < num; i++)
			{
				M2Puts chipByIndex = this.Lay.getChipByIndex(i);
				if (chipByIndex.pattern != 0U)
				{
					string[] array = chipByIndex.Img.Meta.Get("museum");
					if (array != null && array.Length != 0)
					{
						string text = array[0];
						if (TX.valid(text) && !this.Opat2key.ContainsKey(chipByIndex.pattern))
						{
							if (text == "1")
							{
								text = chipByIndex.Img.basename;
							}
							this.Opat2key[chipByIndex.pattern] = new M2LpMuseum.PChips(chipByIndex, chipByIndex.Img, array);
						}
					}
				}
			}
			for (int j = 0; j < num; j++)
			{
				M2Puts chipByIndex2 = this.Lay.getChipByIndex(j);
				M2LpMuseum.PChips pchips;
				if (this.Opat2key.TryGetValue(chipByIndex2.pattern, out pchips))
				{
					if (pchips.ACp[0] != chipByIndex2)
					{
						pchips.ACp.Add(chipByIndex2);
					}
					chipByIndex2.arrangeable = true;
					if (!pchips.visible)
					{
						chipByIndex2.addActiveRemoveKey("MUSEUM", false);
					}
					else
					{
						chipByIndex2.remActiveRemoveKey("MUSEUM", false);
					}
				}
			}
			num = this.Lay.LP.Length;
			for (int k = 0; k < num; k++)
			{
				M2LpMuseumEvacuator m2LpMuseumEvacuator = this.Lay.LP.Get(k) as M2LpMuseumEvacuator;
				if (m2LpMuseumEvacuator != null)
				{
					if (this.AEvac == null)
					{
						this.AEvac = new List<M2LpMuseumEvacuator>(1);
					}
					this.AEvac.Add(m2LpMuseumEvacuator);
				}
			}
		}

		public bool EvtM2Reload(Map2d Mp)
		{
			foreach (KeyValuePair<uint, M2LpMuseum.PChips> keyValuePair in this.Opat2key)
			{
				M2LpMuseum.PChips value = keyValuePair.Value;
				if (value.visible)
				{
					value.showEvent(Mp, this.viewer_pre_event);
				}
			}
			return true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			META meta = new META(this.comment);
			this.focus_mv_key = meta.GetS("focus_mv_key");
			this.check_comment_ev_prefix = meta.GetS("check_comment_ev_prefix");
			this.viewer_pre_event = meta.GetS("viewer_pre_event");
			EV.addListener(this);
			this.AevacPC = null;
			if (this.AEvac != null)
			{
				int count = this.AEvac.Count;
				for (int i = 0; i < count; i++)
				{
					M2LpMuseumEvacuator m2LpMuseumEvacuator = this.AEvac[i];
					int preload_image_count = m2LpMuseumEvacuator.preload_image_count;
					for (int j = 0; j < preload_image_count; j++)
					{
						M2ChipImage preLoadImg = m2LpMuseumEvacuator.GetPreLoadImg(j);
						if (this.AevacPC == null)
						{
							this.AevacPC = new List<M2LpMuseum.PChips>(preload_image_count);
						}
						this.AevacPC.Add(new M2LpMuseum.PChips(preLoadImg, m2LpMuseumEvacuator));
					}
				}
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.Mp.getEventContainer().remReloadListener(this);
			this.runner_assigned = false;
			EV.remListener(this);
			EV.remWaitListener(this);
			if (this.Viewer != null)
			{
				this.Viewer.destruct();
			}
			this.Viewer = null;
			this.AUpdated = null;
		}

		public override void activate()
		{
			int num = 0;
			foreach (KeyValuePair<uint, M2LpMuseum.PChips> keyValuePair in this.Opat2key)
			{
				M2LpMuseum.PChips value = keyValuePair.Value;
				if (this.activatePc(value))
				{
					num++;
				}
			}
			if (this.AevacPC != null)
			{
				int count = this.AevacPC.Count;
				for (int i = 0; i < count; i++)
				{
					M2LpMuseum.PChips pchips = this.AevacPC[i];
					if (this.activatePc(pchips))
					{
						num++;
					}
				}
			}
			if (num > 0)
			{
				this.AUpdated.Sort(delegate(M2LpMuseum.PChips A, M2LpMuseum.PChips B)
				{
					if (A.category_id != B.category_id)
					{
						return A.category_id - B.category_id;
					}
					if (A.id != B.id)
					{
						return A.id - B.id;
					}
					return string.Compare(A.key, B.key);
				});
				int num2 = this.AUpdated.Count;
				if (num2 >= 2)
				{
					int category_id = this.AUpdated[0].category_id;
					for (int j = 1; j < num2; j++)
					{
						if (this.AUpdated[j].category_id != category_id)
						{
							num -= num2 - j;
							this.AUpdated.RemoveRange(j, num2 - j);
							break;
						}
					}
				}
				num2 = this.AUpdated.Count;
				for (int k = 0; k < num2; k++)
				{
					M2LpMuseum.PChips pchips2 = this.AUpdated[k];
					COOK.setSF(pchips2.sf_key, 1);
					pchips2.fineVisible(this);
				}
				EV.getVariableContainer().define("_museum_category_id", this.AUpdated[0].category_id.ToString(), true);
			}
			EV.getVariableContainer().define("_museum_updated", num.ToString(), true);
		}

		private bool activatePc(M2LpMuseum.PChips Pc)
		{
			if (Pc.visible)
			{
				return false;
			}
			if (SCN.checkVisibilityForMuseum(Pc.key))
			{
				if (this.AUpdated == null)
				{
					this.AUpdated = new List<M2LpMuseum.PChips>(1);
				}
				this.AUpdated.Add(Pc);
				return true;
			}
			return false;
		}

		public override void deactivate()
		{
			foreach (KeyValuePair<uint, M2LpMuseum.PChips> keyValuePair in this.Opat2key)
			{
				M2LpMuseum.PChips value = keyValuePair.Value;
				if (!value.visible)
				{
					value.fineVisible(this);
				}
			}
			if (this.AevacPC != null)
			{
				int count = this.AevacPC.Count;
				for (int i = 0; i < count; i++)
				{
					M2LpMuseum.PChips pchips = this.AevacPC[i];
					if (!pchips.visible)
					{
						pchips.fineVisible(this);
					}
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.AUpdated == null || this.AUpdated.Count == 0)
			{
				this.runner_assigned_ = false;
				this.AUpdated = null;
				return false;
			}
			this.t_slideshow -= fcnt;
			if (this.t_slideshow <= 0f)
			{
				M2LpMuseum.PChips pchips = this.AUpdated[0];
				int num = 1;
				if (pchips.EvacSrc != null)
				{
					int count = this.AUpdated.Count;
					int num2 = 1;
					while (num2 < count && this.AUpdated[num2].EvacSrc == pchips.EvacSrc)
					{
						num++;
						num2++;
					}
				}
				this.AUpdated.RemoveRange(0, num);
				if (this.AUpdated.Count == 0)
				{
					return true;
				}
				this.t_slideshow = 80f;
				this.focusFront();
			}
			return true;
		}

		public void destruct()
		{
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					this.Mp.addRunnerObject(this);
					return;
				}
				this.Mp.remRunnerObject(this);
				this.MvFocus = null;
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			M2LpMuseum.WAITSTATE waitstate = this.stt;
			if (waitstate != M2LpMuseum.WAITSTATE.SLIDESHOW)
			{
				if (waitstate != M2LpMuseum.WAITSTATE.WATCHING)
				{
					return false;
				}
				if (this.Viewer != null && this.Viewer.isActive())
				{
					if (is_first)
					{
						this.Viewer.event_playing = false;
						IN.clearPushDown(false);
						EV.getVariableContainer().define("_museum_evt", "", true);
					}
					return !this.Viewer.event_playing;
				}
				this.stt = M2LpMuseum.WAITSTATE.OFFLINE;
				this.Viewer = null;
				return false;
			}
			else
			{
				if (this.AUpdated != null && this.AUpdated.Count > 0)
				{
					return true;
				}
				this.stt = M2LpMuseum.WAITSTATE.OFFLINE;
				return false;
			}
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd == "MUSEUM_SLIDESHOW")
			{
				if (TX.noe(this.focus_mv_key))
				{
					return rER.tError("focus_mv_keyが未指定: " + this.ToString());
				}
				this.MvFocus = this.Mp.getMoverByName(this.focus_mv_key, false);
				if (this.MvFocus == null)
				{
					return rER.tError("focus_mv_keyが見つかりません: " + this.focus_mv_key);
				}
				if (this.AUpdated == null || this.AUpdated.Count == 0)
				{
					return rER.tError("AUpdated が空 ");
				}
				this.stt = M2LpMuseum.WAITSTATE.SLIDESHOW;
				EV.initWaitFn(this, 0);
				this.runner_assigned = true;
				this.t_slideshow = 160f;
				this.focusFront();
				return true;
			}
			else if (rER.cmd == "MUSEUM_OPEN")
			{
				M2LpMuseum.PChips pcbyKey = this.getPCByKey(rER._1);
				if (pcbyKey == null)
				{
					return rER.tError("PChips が見つかりません: " + rER._1);
				}
				MuseumViewer museumViewer = new MuseumViewer(null);
				if (!pcbyKey.AddImageToViewer(museumViewer))
				{
					museumViewer.auto_destruct = true;
					return rER.tError("対象イメージが設定されていません: " + rER._1);
				}
				if (this.Viewer != null)
				{
					this.Viewer.destruct();
				}
				if (TX.valid(this.check_comment_ev_prefix))
				{
					museumViewer.ev_comment = this.check_comment_ev_prefix + rER._1;
				}
				this.Viewer = museumViewer;
				this.stt = M2LpMuseum.WAITSTATE.WATCHING;
				EV.initWaitFn(this, 0);
				return true;
			}
			else
			{
				if (!(rER.cmd == "MUSEUM_VIEWER_ACTIVATE"))
				{
					return false;
				}
				if (this.Viewer == null)
				{
					return rER.tError("Viewer 再生中ではありません");
				}
				this.Viewer.event_playing = false;
				this.stt = M2LpMuseum.WAITSTATE.WATCHING;
				EV.initWaitFn(this, 0);
				return true;
			}
		}

		private void focusFront()
		{
			if (this.MvFocus != null && this.AUpdated != null && this.AUpdated.Count > 0)
			{
				Vector2 mapCenter = this.AUpdated[0].MapCenter;
				this.MvFocus.setTo(mapCenter.x, mapCenter.y);
				base.nM2D.Cam.assignBaseMover(this.MvFocus, -1);
				base.nM2D.Cam.blurCenterIfFocusing(this.MvFocus);
			}
		}

		private M2LpMuseum.PChips getPCByKey(string key)
		{
			foreach (KeyValuePair<uint, M2LpMuseum.PChips> keyValuePair in this.Opat2key)
			{
				if (keyValuePair.Value.key == key)
				{
					return keyValuePair.Value;
				}
			}
			if (this.AevacPC != null)
			{
				int count = this.AevacPC.Count;
				for (int i = 0; i < count; i++)
				{
					M2LpMuseum.PChips pchips = this.AevacPC[i];
					if (pchips.key == key)
					{
						return pchips;
					}
				}
			}
			return null;
		}

		public M2LpMuseumEvacuator isEvacTarget(int category_id)
		{
			if (this.AEvac == null)
			{
				return null;
			}
			int count = this.AEvac.Count;
			for (int i = 0; i < count; i++)
			{
				M2LpMuseumEvacuator m2LpMuseumEvacuator = this.AEvac[i];
				if (m2LpMuseumEvacuator.isEvacTarget(category_id))
				{
					return m2LpMuseumEvacuator;
				}
			}
			return null;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			if (cmd == "MUSEUM_OPEN" && rER.clength > 1)
			{
				M2LpMuseum.PChips pcbyKey = this.getPCByKey(rER._1);
				if (pcbyKey != null)
				{
					return pcbyKey.LoadImageForEvent();
				}
			}
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		private M2LpMuseum.WAITSTATE stt;

		private BDic<uint, M2LpMuseum.PChips> Opat2key;

		private List<M2LpMuseum.PChips> AevacPC;

		private const string CP_REM_KEY = "MUSEUM";

		private string focus_mv_key;

		private string viewer_pre_event;

		private M2Mover MvFocus;

		private bool runner_assigned_;

		private List<M2LpMuseum.PChips> AUpdated;

		private List<M2LpMuseumEvacuator> AEvac;

		private float t_slideshow;

		private const float MAXT_SLIDESHOW_FIRST = 120f;

		private const float MAXT_SLIDESHOW = 80f;

		private MuseumViewer Viewer;

		private string check_comment_ev_prefix;

		private class PChips
		{
			public PChips(M2Puts Cp, M2ChipImage Ci, string[] As)
			{
				this.key = As[0];
				this.sf_key = "v_" + this.key;
				this.ACp = new List<M2Puts>(5);
				if (Cp != null)
				{
					this.ACp.Add(Cp);
					this.Apics = Ci.Meta.Get("pic");
					Cp.arrangeable = true;
				}
				this.visible = COOK.getSF(this.sf_key) != 0;
				if (As.Length >= 2)
				{
					this.id = X.NmI(As[1], 0, false, false);
					if (As.Length >= 3)
					{
						this.category_id = X.NmI(As[2], 0, false, false);
					}
				}
			}

			public PChips(M2ChipImage Ci, M2LpMuseumEvacuator _EvacSrc)
				: this(null, Ci, Ci.Meta.Get("museum"))
			{
				this.EvacSrc = _EvacSrc;
			}

			public void fineVisible(M2LpMuseum Con)
			{
				bool flag = COOK.getSF(this.sf_key) != 0;
				if (this.visible != flag)
				{
					this.visible = flag;
					for (int i = this.ACp.Count - 1; i >= 0; i--)
					{
						M2Puts m2Puts = this.ACp[i];
						if (this.visible)
						{
							m2Puts.remActiveRemoveKey("MUSEUM", false);
							this.showEvent(Con.Mp, Con.viewer_pre_event);
						}
						else
						{
							m2Puts.addActiveRemoveKey("MUSEUM", false);
						}
					}
				}
			}

			public void closeAction(Map2d Mp)
			{
				M2EventItem m2EventItem = Mp.getMoverByName(this.key, false) as M2EventItem;
				if (m2EventItem != null)
				{
					m2EventItem.destruct();
				}
			}

			public void showEvent(Map2d Mp, string viewer_pre_event)
			{
				if (!this.visible || this.Apics == null || this.ACp.Count == 0)
				{
					return;
				}
				M2EventItem m2EventItem = Mp.getMoverByName(this.key, false) as M2EventItem;
				if (m2EventItem == null)
				{
					m2EventItem = Mp.getEventContainer().CreateAndAssign(this.key);
					m2EventItem.assign(M2EventItem.CMD.CHECK, this.getEventContent(viewer_pre_event), false);
					Vector2 mapCenter = this.MapCenter;
					float footableY = Mp.getFootableY(mapCenter.x, (int)mapCenter.y, 12, true, -1f, false, true, true, 0f);
					m2EventItem.Size(45f, (footableY - mapCenter.y) * m2EventItem.CLENM * 0.5f + 15f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
					m2EventItem.setTo(mapCenter.x, footableY - m2EventItem.sizey);
				}
			}

			private string getEventContent(string viewer_pre_event)
			{
				string text;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AR("TITLECALL_HIDE");
					stb.AR("GREETING L -12");
					if (TX.valid(viewer_pre_event))
					{
						stb.AR("_result=1");
						stb.Add("CHANGE_EVENT2 ", viewer_pre_event, " ").AR("key");
						stb.AR("IF $_result'==0' ");
						stb.AR("  SEEK_END");
					}
					stb.AR("PIC_FILL &9 DARK");
					stb.AR("PIC_FADEIN &9 50");
					stb.AR("WAIT 52");
					stb.AR("PIC_FADEOUT &9 80");
					stb.AR("TITLECALL_HIDE 1");
					stb.AR("UI_DISABLE");
					stb.AR("MUSEUM_OPEN " + this.key);
					stb.AR("LABEL _MUS");
					stb.Add("IFSTR $", "_museum_evt", " ISNOT '' {").Ret("\n");
					stb.AR("CHANGE_EVENT2 $", "_museum_evt");
					stb.AR("MUSEUM_VIEWER_ACTIVATE");
					stb.AR("GOTO _MUS");
					stb.AR("}");
					stb.AR("UI_ENABLE");
					text = stb.ToString();
				}
				return text;
			}

			public Vector2 MapCenter
			{
				get
				{
					if (this.EvacSrc != null)
					{
						return new Vector2(this.EvacSrc.mapcx, this.EvacSrc.mapcy);
					}
					M2Puts m2Puts = this.ACp[0];
					return new Vector2(m2Puts.mapcx, m2Puts.mapcy);
				}
			}

			public bool AddImageToViewer(MuseumViewer Viewer)
			{
				if (this.Apics == null)
				{
					return false;
				}
				int num = this.ACp.Count;
				for (int i = 0; i < num; i++)
				{
					string[] array = this.ACp[i].Img.Meta.Get("pic_bg_col");
					if (array != null)
					{
						Viewer.ColBg = C32.d2c(X.NmUI(array[0], 0U, true, true));
					}
				}
				bool flag = false;
				num = this.Apics.Length;
				for (int j = 0; j < num; j++)
				{
					EvImg pic = EV.Pics.getPic(this.Apics[j], true, true);
					if (pic != null)
					{
						Viewer.addImg(pic);
						flag = true;
					}
					else
					{
						X.dl("不明なイメージ: " + this.Apics[j], null, false, false);
					}
				}
				return flag;
			}

			public int LoadImageForEvent()
			{
				if (this.Apics == null)
				{
					return 0;
				}
				int num = this.Apics.Length;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					num2 = X.Mx(num2, EV.Pics.cacheReadFor(this.Apics[i]));
				}
				return num2;
			}

			public readonly string key;

			public readonly int id;

			public readonly int category_id;

			public readonly string sf_key;

			public readonly List<M2Puts> ACp;

			public readonly string[] Apics;

			public readonly M2LpMuseumEvacuator EvacSrc;

			public bool visible;
		}

		private enum WAITSTATE
		{
			OFFLINE,
			SLIDESHOW,
			WATCHING
		}
	}
}
