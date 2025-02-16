using System;
using System.Collections.Generic;
using PixelLiner;
using XX;

namespace nel
{
	public sealed class UiGmMapMarker
	{
		public UiGmMapMarker(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.AMk = new List<UiGmMapMarker.MKInfo>(4);
		}

		public static void initItem()
		{
			int num = 16;
			for (int i = 0; i < num; i++)
			{
				string text = "mapmarker_";
				UiGmMapMarker.MK mk = (UiGmMapMarker.MK)i;
				string text2 = text + mk.ToString().ToLower();
				NelItem.CreateItemEntry(text2, new NelItem(text2, 0, 64, 999)
				{
					category = (NelItem.CATEG)2097153U,
					FnGetName = NelItem.fnGetNameMapMarker,
					FnGetDesc = NelItem.fnGetDescMapMarker,
					SpecificColor = MTRX.ColWhite
				}, 62200 + i, false);
			}
		}

		public void newGame()
		{
			this.AMk.Clear();
		}

		public void newGameAfter(bool old_file_read)
		{
			ItemStorage inventoryPrecious = this.M2D.IMNG.getInventoryPrecious();
			if (old_file_read && SCN.fine_pvv(false) >= 100)
			{
				for (int i = 0; i < 4; i++)
				{
					inventoryPrecious.Add(UiGmMapMarker.GetItem((UiGmMapMarker.MK)i), 1, 0, true, true);
				}
			}
			this.AMk.Clear();
			for (int j = 0; j < 16; j++)
			{
				NelItem item = UiGmMapMarker.GetItem((UiGmMapMarker.MK)j);
				ItemStorage.ObtainInfo info = inventoryPrecious.getInfo(item);
				if (info != null)
				{
					if (item.visible_obtain_count == 0)
					{
						item.addObtainCount(info.total);
					}
					this.AMk.Add(new UiGmMapMarker.MKInfo((UiGmMapMarker.MK)j, info));
				}
			}
			this.AMk.Sort((UiGmMapMarker.MKInfo a, UiGmMapMarker.MKInfo b) => UiGmMapMarker.sortMarker(a, b));
		}

		private static int sortMarker(UiGmMapMarker.MKInfo a, UiGmMapMarker.MKInfo b)
		{
			if (a.Info == null || b.Info == null)
			{
				if (a.Info == null && b.Info == null)
				{
					return 0;
				}
				if (a.Info != null)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				if (a.Info.newer == b.Info.newer)
				{
					return 0;
				}
				if (a.Info.newer >= b.Info.newer)
				{
					return 1;
				}
				return -1;
			}
		}

		public static NelItem GetItem(UiGmMapMarker.MK mk)
		{
			return NelItem.GetById("mapmarker_" + mk.ToString().ToLower(), false);
		}

		public void addCount(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			UiGmMapMarker.MK mk;
			if (!FEnum<UiGmMapMarker.MK>.TryParse(TX.slice(Itm.key, "mapmarker_".Length).ToUpper(), out mk, true))
			{
				return;
			}
			UiGmMapMarker.MKInfo mkinfo = this.getMKInfo(mk);
			if (mkinfo == null)
			{
				this.AMk.Add(new UiGmMapMarker.MKInfo(mk, Obt));
				return;
			}
			mkinfo.count += Obt.total;
		}

		public UiGmMapMarker.MKInfo getMKInfo(UiGmMapMarker.MK k)
		{
			for (int i = this.AMk.Count - 1; i >= 0; i--)
			{
				UiGmMapMarker.MKInfo mkinfo = this.AMk[i];
				if (mkinfo.mk == k)
				{
					return mkinfo;
				}
			}
			return null;
		}

		public bool marker_enabled
		{
			get
			{
				return this.AMk.Count > 0;
			}
		}

		public void setFocusByMarkerId(int marker_id)
		{
			if (marker_id == -1)
			{
				this.focus = -1;
				return;
			}
			int count = this.AMk.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AMk[i].mk == (UiGmMapMarker.MK)marker_id)
				{
					this.focus = i;
					return;
				}
			}
		}

		public BtnContainerRadio<aBtn> makeRadioTo(Designer Ds, List<string> AkeysL, int clms, float btnw = 44f, int _clms = 0, bool _unselectable = false, bool _default_focus = false)
		{
			int num = 8;
			if (clms > 5)
			{
				btnw *= 0.66f;
				num = 4;
			}
			return Ds.addRadioT<aBtnNel>(new DsnDataRadio
			{
				keysL = AkeysL,
				def = this.focus + (this.make_empty ? 1 : 0),
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnRadioChange),
				skin = "kadomaru_icon",
				name = "_mapmarker_radio",
				w = btnw,
				h = btnw,
				margin_w = num,
				margin_h = 3,
				navi_loop = 1,
				fnClick = this.FnClick,
				fnMakingAfter = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingBtn),
				unselectable = (_unselectable ? 2 : 0),
				default_focus = (_default_focus ? 100 : (-100)),
				clms = clms
			});
		}

		private bool fnMakingBtn(BtnContainer<aBtn> BCon, aBtn _B)
		{
			int num = X.NmI(_B.title, 0, false, false);
			ButtonSkinKadomaruIcon buttonSkinKadomaruIcon = _B.get_Skin() as ButtonSkinKadomaruIcon;
			if (buttonSkinKadomaruIcon == null)
			{
				return true;
			}
			if (num >= 0)
			{
				buttonSkinKadomaruIcon.PFMesh = MTR.AImgMapMarker[(int)this.AMk[num].mk];
				_B.click_snd = "tool_penki";
			}
			else
			{
				buttonSkinKadomaruIcon.PFMesh = MTRX.getPF("nel_x");
				_B.click_snd = "tool_lpoint";
			}
			return true;
		}

		public List<string> getCheckKeys(List<string> ADest, bool _make_empty, out int clms)
		{
			this.make_empty = _make_empty;
			int num = this.AMk.Count + (this.make_empty ? 1 : 0);
			if (ADest.Capacity < num)
			{
				ADest.Capacity = num;
			}
			while (ADest.Count < num)
			{
				ADest.Add(null);
			}
			for (int i = 0; i < this.AMk.Count; i++)
			{
				UiGmMapMarker.MKInfo mkinfo = this.AMk[i];
				ADest[i + (this.make_empty ? 1 : 0)] = i.ToString();
			}
			if (this.make_empty)
			{
				ADest[0] = "-1";
			}
			int num2 = 5;
			if (ADest.Count > num2)
			{
				num2 += 3;
			}
			clms = X.Mn(num2, ADest.Count);
			return ADest;
		}

		public int getMarkerId(string icon_key)
		{
			if (icon_key == "-1")
			{
				return -1;
			}
			int num = X.NmI(icon_key, -1, true, false);
			if (X.BTW(0f, (float)num, (float)this.AMk.Count))
			{
				return (int)this.AMk[num].mk;
			}
			return -1;
		}

		public int getFocus()
		{
			return this.focus;
		}

		private bool fnRadioChange(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (this.make_empty)
			{
				cur_value--;
			}
			if (this.FnChanged != null && !this.FnChanged(_B, pre_value, this.focus))
			{
				return false;
			}
			this.focus = cur_value;
			return true;
		}

		public static void drawTo(MeshDrawer Md, float x, float y, int id)
		{
			if (id < 0 || id >= MTR.AImgMapMarker.Length)
			{
				return;
			}
			PxlFrame pxlFrame = MTR.AImgMapMarker[id];
			Md.RotaPF(x, y, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
		}

		public static void drawTo(MeshDrawer Md, float x, float y, NelItem I)
		{
			UiGmMapMarker.drawTo(Md, x, y, (int)(I.id - 62200));
		}

		public readonly NelM2DBase M2D;

		public int focus = -1;

		public const string radio_name = "_mapmarker_radio";

		public const string itemheader_marker = "mapmarker_";

		public BtnContainerRadio<aBtn>.FnRadioBindings FnChanged;

		public const int ITEM_PRICE = 64;

		public List<UiGmMapMarker.MKInfo> AMk;

		public FnBtnBindings FnClick;

		private bool make_empty;

		public enum MK
		{
			RED,
			YELLOW,
			GREEN,
			BLUE,
			HEART,
			SKULL,
			HUMAN,
			COFFEE,
			EXC,
			QUE,
			TREASURE,
			ENEMY,
			CRAFTS,
			FRUIT,
			VEGGIE,
			MEAT,
			_MAX
		}

		public class MKInfo
		{
			public MKInfo(UiGmMapMarker.MK _mk, ItemStorage.ObtainInfo _Info)
			{
				this.mk = _mk;
				this.count = _Info.total;
				this.Info = _Info;
			}

			public MKInfo(UiGmMapMarker.MK _mk, int _cnt, int _using_count = 0)
			{
				this.mk = _mk;
				this.count = _cnt;
				this.Info = null;
			}

			public UiGmMapMarker.MK mk;

			public int count;

			public ItemStorage.ObtainInfo Info;
		}
	}
}
