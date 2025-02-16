using System;
using UnityEngine;
using XX;

namespace nel
{
	public class UILog : RBase<UILogRow>
	{
		public UILog(UIBase _Base, GameObject Gob)
			: base(0, true, false, false)
		{
			UILog.Instance = this;
			this.Base = _Base;
			this.MMRD = Gob.AddComponent<MultiMeshRenderer>();
			base.Alloc(10);
			this.Sort = new SORT<UILogRow>(null);
			this.FlgStopMoneyChangedAnnounce = new Flagger(null, null);
			this.FD_MoneyChanged = new CoinStorage.FnMoneyChanged(this.MoneyChanged);
			CoinStorage.FD_MoneyChanged += this.FD_MoneyChanged;
		}

		public override void destruct()
		{
			base.destruct();
			CoinStorage.FD_MoneyChanged -= this.FD_MoneyChanged;
			if (UILog.Instance == this)
			{
				UILog.Instance = null;
			}
		}

		public override UILogRow Create()
		{
			return new UILogRow(this);
		}

		public override void clear()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].destruct();
			}
			base.clear();
		}

		public UILogRow AddGetItem(NelItemManager IMNG, NelItem Itm, int count = 1, int grade = 0)
		{
			STB stb = TX.PopBld(Itm.key, 0);
			stb += "++";
			stb.Add(grade);
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow.type == UILogRow.TYPE.GETITEM && stb.Equals(uilogRow.a_key))
				{
					TX.ReleaseBld(stb);
					uilogRow.addCount(count, IMNG.getInventory().getCount(Itm, grade));
					return uilogRow;
				}
			}
			UILogRow uilogRow2 = base.Pop(16);
			uilogRow2.activateGetItem(stb, IMNG, Itm, count, grade);
			TX.ReleaseBld(stb);
			return uilogRow2;
		}

		public UILogRow AddConsumeItem(NelItem Itm, int grade, int rest_count)
		{
			STB stb = TX.PopBld(Itm.key, 0);
			stb += "--consume";
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow.type == UILogRow.TYPE.CONSUMEITEM && stb.Equals(uilogRow.a_key))
				{
					TX.ReleaseBld(stb);
					uilogRow.addCount(1, rest_count);
					return uilogRow;
				}
			}
			UILogRow uilogRow2 = base.Pop(16);
			uilogRow2.activateConsumeItem(stb, Itm, grade, rest_count);
			TX.ReleaseBld(stb);
			return uilogRow2;
		}

		public UILogRow AddMoney(int after_total, int added)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow.type == UILogRow.TYPE.MONEY)
				{
					uilogRow.addCount(added, added);
					return uilogRow;
				}
			}
			UILogRow uilogRow2 = base.Pop(16);
			uilogRow2.activateMoney(added);
			return uilogRow2;
		}

		public UILogRow AddAlertTX(string t, UILogRow.TYPE alert_type = UILogRow.TYPE.ALERT)
		{
			return this.AddAlert(TX.Get(t, ""), alert_type);
		}

		public UILogRow AddAlert(string t, UILogRow.TYPE alert_type = UILogRow.TYPE.ALERT)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow.type == alert_type && uilogRow.a_key == t)
				{
					uilogRow.deactivate(uilogRow.isHiding());
				}
			}
			UILogRow uilogRow2 = base.Pop(16);
			uilogRow2.activate(alert_type, t, t);
			switch (alert_type)
			{
			case UILogRow.TYPE.ALERT_EGG:
				uilogRow2.setIcon(MTRX.getPF("alert_egg"), 4290582552U).BgCol(uint.MaxValue, 4281338143U, 4283723098U, 4290642095U);
				break;
			case UILogRow.TYPE.ALERT_EP:
				uilogRow2.setIcon(MTRX.getPF("alert_ep"), 4294903993U).BgCol(4294903993U, 4280098077U, 2854034717U, 4294903993U);
				break;
			case UILogRow.TYPE.ALERT_EP2:
				uilogRow2.setIcon(MTRX.getPF("alert_ep"), 4280615451U).BgCol(4280098077U, 4294903993U, 4293656623U, uint.MaxValue);
				break;
			case UILogRow.TYPE.ALERT_GRAY:
				uilogRow2.setIcon(MTRX.getPF("alert"), uint.MaxValue).BgCol(uint.MaxValue, 3426236989U, 2860153984U, 4290032820U);
				break;
			case UILogRow.TYPE.ALERT_HUNGER:
				uilogRow2.setIcon(MTRX.getPF("alert"), 4294789449U).BgCol(4294789449U, 4281338378U, 2857651019U, 4293994536U);
				break;
			case UILogRow.TYPE.ALERT_BENCH:
				uilogRow2.setIcon(MTRX.getPF("wmap_bench"), uint.MaxValue).BgCol(4283780170U, 4286054279U, 4292083674U, uint.MaxValue);
				break;
			case UILogRow.TYPE.ALERT_PUPPET:
				uilogRow2.setIcon(MTRX.getPF("IconGolem"), uint.MaxValue).BgCol(4294907928U, 4278190080U, 4285797653U, 4290653999U);
				break;
			case UILogRow.TYPE.ALERT_FATAL:
				uilogRow2.setIcon(MTRX.getPF("alert"), uint.MaxValue).BgCol(uint.MaxValue, 4289931309U, 4285792279U, uint.MaxValue);
				break;
			case UILogRow.TYPE.ALERT_BARU:
				uilogRow2.setIcon(MTR.AItemIcon[62], 4292269782U).BgCol(uint.MaxValue, 3426236989U, 4278190080U, 4294945323U);
				break;
			default:
				uilogRow2.setIcon(MTRX.getPF("alert"), 4290582552U);
				break;
			}
			return uilogRow2;
		}

		public UILogRow AddLog(string t)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow.type == UILogRow.TYPE.NORMAL && uilogRow.a_key == t)
				{
					uilogRow.addCount(1, -1000);
					return uilogRow;
				}
			}
			UILogRow uilogRow2 = base.Pop(16);
			uilogRow2.activate(UILogRow.TYPE.NORMAL, t, t);
			return uilogRow2;
		}

		public override bool run(float fcnt)
		{
			this.act_count = (this.act_count_tdh = 0f);
			if (this.af >= 0f)
			{
				if (this.af < 44f)
				{
					this.af += fcnt;
					this.repositAll();
				}
			}
			else
			{
				if (this.af <= -45f)
				{
					return true;
				}
				this.af -= fcnt;
				this.repositAll();
			}
			if (this.ev_temporary == 2)
			{
				this.ev_temporary = 1;
				this.Sort.qSort(this.AItems, (UILogRow a, UILogRow b) => UILog.fnSortUsing(a, b), this.LEN);
			}
			if (this.LEN > 5)
			{
				this.top_fasten = 1;
			}
			else
			{
				this.top_fasten = 0;
			}
			base.run(fcnt);
			if (this.LEN > 0)
			{
				float num = 5f - (this.act_count + 1f);
				if (this.yshift != num)
				{
					this.yshift = X.VALWALK(this.yshift, num, 0.1f);
					for (int i = X.Mn(4, this.LEN - 1); i >= 0; i--)
					{
						this.AItems[i].y_level = -1f;
					}
				}
				return true;
			}
			return false;
		}

		public void RenderOneSide(bool bottom_flag, Matrix4x4 Multiple, Camera Cam = null)
		{
			int len = this.LEN;
			int num = ((!bottom_flag) ? 14 : 2);
			for (int i = 0; i < num; i++)
			{
				bool flag = false;
				for (int j = 0; j < len; j++)
				{
					UILogRow uilogRow = this.AItems[j];
					if (bottom_flag)
					{
						uilogRow.RenderOneSide(i == 1, ref flag, Multiple, Cam);
					}
					else
					{
						uilogRow.RenderOneSideText(i, ref flag, Multiple, Cam);
					}
				}
			}
		}

		private void MoneyChanged(CoinStorage.CTYPE ctype, int added)
		{
			if (ctype == CoinStorage.CTYPE.GOLD && !this.FlgStopMoneyChangedAnnounce.isActive())
			{
				UILog.Instance.AddMoney((int)CoinStorage.getCount(ctype), added);
			}
		}

		public bool valotile_enabled
		{
			get
			{
				return this.Base != null && this.Base.valotile_enabled;
			}
			set
			{
				if (this.Base == null)
				{
					return;
				}
				int len = this.LEN;
				for (int i = 0; i < len; i++)
				{
					this.AItems[i].valotile_enabled = value;
				}
			}
		}

		public void repositAll()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItems[i] != null)
				{
					this.AItems[i].repos = true;
				}
			}
		}

		public void fineFonts()
		{
			for (int i = 0; i < this.AItems.Length; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow != null)
				{
					uilogRow.fineFonts();
				}
			}
		}

		public void activateEventTemporary(bool force_reposit = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow != null)
				{
					uilogRow.event_temp_active = true;
					if (force_reposit)
					{
						uilogRow.repos = true;
					}
				}
			}
			this.ev_temporary = 0;
		}

		public void deactivateEventTemporary()
		{
			if (this.LEN == 0)
			{
				return;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				UILogRow uilogRow = this.AItems[i];
				if (uilogRow != null)
				{
					uilogRow.event_temp_active = false;
				}
			}
			if (this.ev_temporary == 0)
			{
				this.ev_temporary = 1;
			}
		}

		private static int fnSortUsing(UILogRow Ra, UILogRow Rb)
		{
			if (Ra == null || Rb == null)
			{
				if (Ra == Rb)
				{
					return 0;
				}
				if (Ra != null)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				bool flag = Ra.event_temp_active;
				bool flag2 = Rb.event_temp_active;
				if (flag != flag2)
				{
					if (!flag)
					{
						return 1;
					}
					return -1;
				}
				else
				{
					flag = Ra.completely_hidden;
					flag2 = Rb.completely_hidden;
					if (flag != flag2)
					{
						if (!flag)
						{
							return -1;
						}
						return 1;
					}
					else
					{
						if (Ra.y_level < Rb.y_level)
						{
							return -1;
						}
						if (Ra.y_level <= Rb.y_level)
						{
							return 0;
						}
						return 1;
					}
				}
			}
		}

		public void activate()
		{
			if (this.af < 0f)
			{
				this.af = 0f;
			}
			this.activateEventTemporary(true);
		}

		public void deactivate()
		{
			if (this.af >= 0f)
			{
				this.af = -1f;
				this.repositAll();
			}
		}

		public bool isVisible()
		{
			return this.af > -40f;
		}

		public float base_active_level
		{
			get
			{
				if (this.af < 0f)
				{
					return 1f - X.ZSIN2(-this.af, 40f);
				}
				return X.ZSIN2(this.af, 40f);
			}
		}

		public bool ev_temporary_sort
		{
			get
			{
				return this.ev_temporary == 2;
			}
			set
			{
				if (value && this.ev_temporary == 1)
				{
					this.ev_temporary = 2;
				}
			}
		}

		public const float SHOW_MAX = 4.5f;

		public const int SHOW_MAX_I = 5;

		public static UILog Instance;

		public readonly UIBase Base;

		public readonly MultiMeshRenderer MMRD;

		public float act_count;

		public float act_count_tdh;

		public float yshift;

		private const float FADE_T = 40f;

		public float af = 40f;

		public static float uilog_frame_base_speed = 1f;

		private byte ev_temporary;

		public byte top_fasten;

		private SORT<UILogRow> Sort;

		public readonly CoinStorage.FnMoneyChanged FD_MoneyChanged;

		public readonly Flagger FlgStopMoneyChangedAnnounce;
	}
}
