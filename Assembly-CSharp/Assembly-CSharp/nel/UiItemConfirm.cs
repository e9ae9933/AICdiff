using System;
using System.Collections.Generic;
using XX;

namespace nel
{
	public class UiItemConfirm : UiBoxDesignerFamily
	{
		private float bounds_h
		{
			get
			{
				return IN.h - 110f;
			}
		}

		private float out_h
		{
			get
			{
				return this.bounds_h + 74f;
			}
		}

		private float confirm_h
		{
			get
			{
				return 72f;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.ATarget = new List<NelItemEntry>(1);
		}

		public void InitItemConfirm(NelM2DBase _M2D, List<NelItemEntry> _ATarget)
		{
			if (_ATarget == null || _ATarget.Count == 0)
			{
				return;
			}
			this.M2D = _M2D;
			this.current_target = 0;
			if (_ATarget != this.ATarget)
			{
				this.ATarget.Clear();
				this.ATarget.AddRange(_ATarget);
			}
			ItemStorageMem.Prepare(ref this.AInvSrc, this.M2D);
			this.InitItemInner();
		}

		public void UndoStorage()
		{
			ItemStorageMem.UndoAll(this.AInvSrc);
		}

		private bool InitItemInner()
		{
			if (this.current_target >= this.ATarget.Count)
			{
				return false;
			}
			SND.Ui.play("editor_open", false);
			NelItemEntry nelItemEntry = this.ATarget[this.current_target];
			NelItem data = nelItemEntry.Data;
			int grade = (int)nelItemEntry.grade;
			base.gameObject.SetActive(true);
			this.fine_navi_index = -1;
			this.createBox(this.M2D);
			this.activate();
			this.progressing_target = false;
			this.AStorage[0].clearAllItems(10);
			this.AStorage[1].clearAllItems(10);
			int count = this.AInvSrc.Count;
			for (int i = 0; i < count; i++)
			{
				ItemStorage inv = this.AInvSrc[i].Inv;
				if (inv != null)
				{
					ItemStorage.ObtainInfo info = inv.getInfo(nelItemEntry.Data);
					if (info != null)
					{
						for (int j = grade; j < 5; j++)
						{
							int count2 = info.getCount(j);
							if (count2 > 0)
							{
								this.AStorage[0].Add(data, count2, j, true, true);
							}
						}
					}
				}
			}
			this.StorageCheckAfter(nelItemEntry);
			this.IMvCon.createItemMoveBox(-155f, this.out_h * 0.5f - this.bounds_h * 0.5f, this.BxL, this.BxR, this.AStorage[0], this.AStorage[1], this.AMng[0], this.AMng[1], this.BxDesc, this.BxCmd);
			this.AMng[0].fineItemStartAvailable(grade);
			this.recheckConfirmEnabled();
			if (this.IMvCon.both_empty_rows)
			{
				this.AMng[0].fineItemStarsCount(false, data, this.AStorage[0].getInfo(data));
				this.BConMain.Get(1).Select(true);
				this.AMng[0].fineItemDetailInner(false, data, new ItemStorage.ObtainInfo(), null, grade, true, true, false);
				FillBlock fillBlock = this.BxDesc.Get("item_detail", false) as FillBlock;
				if (fillBlock != null)
				{
					fillBlock.Txt(NEL.error_tag + TX.Get("ItemConfirm_do_not_obtain", "") + NEL.error_tag_close);
				}
			}
			return true;
		}

		protected virtual void StorageCheckAfter(NelItemEntry CurIE)
		{
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this.IMvCon != null)
			{
				this.IMvCon.destruct();
				this.IMvCon = null;
			}
		}

		public bool runItemMove(float fcnt)
		{
			if (!this.IMvCon.runItemMove(fcnt))
			{
				aBtn aBtn = this.BConMain.Get(0);
				aBtn aBtn2 = aBtn.PreSelected;
				if (!aBtn.isLocked())
				{
					aBtn.Select(true);
				}
				else
				{
					aBtn = this.BConMain.Get(1);
					if (!aBtn.isSelected())
					{
						aBtn.Select(true);
					}
					else
					{
						aBtn.ExecuteOnSubmitKey();
						aBtn2 = null;
					}
				}
				if (aBtn2 is aBtnItemRow)
				{
					aBtn.setNaviT(aBtn2, false, false);
					this.fine_navi_index = aBtn.carr_index;
				}
			}
			if (this.fine_navi_index != -1)
			{
				this.BConMain.Get(1 - this.fine_navi_index).setNaviT(this.BConMain.Get(this.fine_navi_index).getNaviAim(1), true, false);
				this.BConMain.Get(1 - this.fine_navi_index).setNaviB(this.BConMain.Get(this.fine_navi_index).getNaviAim(3), true, false);
				this.fine_navi_index = -1;
			}
			if (aBtn.PreSelected == null && (IN.isTP(1) || IN.isBP(1)))
			{
				this.selectDefaultMain();
			}
			return true;
		}

		private void selectDefaultMain()
		{
			if (!this.IMvCon.isUsingState())
			{
				aBtn aBtn = this.BConMain.Get(0);
				if (!aBtn.isLocked())
				{
					aBtn.Select(true);
					return;
				}
				this.BConMain.Get(1).Select(true);
			}
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			if (this.current_target > 0)
			{
				ItemStorageMem.UndoAll(this.AInvSrc);
			}
			this.progressing_target = false;
			return base.deactivate(immediate);
		}

		public void deactivateTemp()
		{
			base.deactivate(false);
			this.active = true;
			this.auto_deactive_gameobject = false;
		}

		private void createBox(NelM2DBase M2D)
		{
			if (this.IMvCon != null)
			{
				return;
			}
			this.AStorage = new ItemStorage[2];
			this.AMng = new UiItemManageBoxSlider[2];
			this.IMvCon = new UiItemMove
			{
				inventory0_tx_key = "Guild_Inventory_mine",
				inventory1_tx_key = "Guild_Inventory_deliver",
				fnUsingSliderDesc = new UiItemMove.FnUsingSliderDesc(this.fnItemMoveSliderDesc),
				force_clear_designer = false
			};
			this.BxL = base.Create("L", 0f, 0f, 300f, this.bounds_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxR = base.Create("R", 0f, 0f, 300f, this.bounds_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxDesc = base.Create("Desc", 0f, 0f, 300f, this.bounds_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.base_z -= 0.25f;
			this.BxCmd = base.Create("Cmd", 0f, 0f, 300f, 140f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxConfirm = base.Create("Cmd", 0f, 0f, IN.w * 0.75f, this.confirm_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.auto_activate = 23U;
			this.BxConfirm.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxConfirm.positionD(0f, -this.out_h * 0.5f + this.confirm_h * 0.5f, 1, 40f);
			this.BxConfirm.margin_in_lr = 40f;
			this.BxConfirm.margin_in_tb = 20f;
			this.BxConfirm.alignx = ALIGN.CENTER;
			this.BxConfirm.init();
			this.FbConfirm = null;
			this.FbConfirm = this.BxConfirm.addP(new DsnDataP("", false)
			{
				name = "mainp",
				alignx = ALIGN.LEFT,
				TxCol = NEL.ColText,
				html = true,
				text = " ",
				size = 14f,
				swidth = this.BxConfirm.use_w * 0.38f
			}, false);
			this.BConMain = this.BxConfirm.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				clms = 2,
				titles = new string[] { "&&Guild_Btn_deliver", "&&Cancel" },
				w = 230f,
				h = 24f,
				margin_w = 30f,
				fnHover = delegate(aBtn B)
				{
					this.fine_navi_index = B.carr_index;
					return true;
				},
				fnClick = delegate(aBtn B)
				{
					if (this.IMvCon.isUsingState())
					{
						return false;
					}
					IN.clearPushDown(true);
					if (B.title == "&&Guild_Btn_deliver")
					{
						if (!this.decideItems())
						{
							SND.Ui.play("locked", false);
							CURS.limitVib(AIM.R);
							return false;
						}
					}
					else
					{
						if (this.FD_fnItemConfirmFinished != null && !this.FD_fnItemConfirmFinished(this.CurEntry, false))
						{
							return false;
						}
						this.deactivate(false);
					}
					return true;
				}
			});
			this.BConMain.Get(1).click_snd = "cancel";
			this.IMvCon.fnItemRowRemakedAfter = delegate(ItemStorage Inv)
			{
				Inv.setTBNavi(this.BConMain, true, true, false);
				if (!this.IMvCon.current_sliding_item && Inv.getVisibleRowCount() == 0)
				{
					this.selectDefaultMain();
				}
			};
			this.IMvCon.fnSlidedMeter = delegate(ItemStorage Storage, int increase, int pre, int cur)
			{
				if (Storage != this.AStorage[0] || increase < 0)
				{
					return cur;
				}
				if (!this.recheckConfirmEnabled())
				{
					return cur;
				}
				return pre;
			};
			this.IMvCon.fnSlidedMeterAfter = delegate(ItemStorage Storage, int increase, int pre, int cur)
			{
				this.recheckConfirmEnabled();
				if (!this.IMvCon.current_sliding_item && Storage == this.AStorage[0] && Storage.getVisibleRowCount() == 0)
				{
					this.selectDefaultMain();
				}
				return cur;
			};
			for (int i = 0; i < 2; i++)
			{
				this.AStorage[i] = new ItemStorage("ItemConfirm0", 10)
				{
					water_stockable = true,
					infinit_stockable = true,
					grade_split = true,
					sort_button_bits = 0
				};
				this.AMng[i] = new UiItemManageBoxSlider(M2D.IMNG, base.transform)
				{
					slice_height = 38f,
					use_topright_counter = false,
					selectable_loop = false
				};
			}
		}

		private bool recheckConfirmEnabled()
		{
			aBtn aBtn = this.BConMain.Get(0);
			NelItemEntry curEntry = this.CurEntry;
			NelItem data = curEntry.Data;
			int count = this.AStorage[1].getCount(data, -1);
			using (STB stb = TX.PopBld(null, 0))
			{
				data.getLocalizedName(stb, (int)curEntry.grade);
				NelItem.getGradeMeshTxTo(stb, (int)curEntry.grade, 1, 50);
				stb.Ret("\n").AddTxA("Guild_item_deliver_count", false).TxRpl(curEntry.count - count);
				this.FbConfirm.Txt(stb);
			}
			if (count >= curEntry.count)
			{
				aBtn.SetLocked(false, true, true);
				return true;
			}
			aBtn.SetLocked(true, true, true);
			return false;
		}

		public bool decideItems()
		{
			if (!this.recheckConfirmEnabled())
			{
				return false;
			}
			NelItemEntry curEntry = this.CurEntry;
			bool flag = this.current_target >= this.ATarget.Count - 1;
			if (flag && this.FD_fnItemConfirmFinished != null && !this.FD_fnItemConfirmFinished(curEntry, true))
			{
				return false;
			}
			int count = this.AInvSrc.Count;
			NelItem data = curEntry.Data;
			ItemStorage.ObtainInfo info = this.AStorage[1].getInfo(data);
			if (info != null)
			{
				for (int i = (int)curEntry.grade; i < 5; i++)
				{
					int num = info.getCount(i);
					if (num > 0)
					{
						for (int j = 0; j < count; j++)
						{
							ItemStorage inv = this.AInvSrc[j].Inv;
							if (inv != null)
							{
								int num2 = X.Mn(inv.getCount(data, i), num);
								num -= num2;
								inv.Reduce(data, num2, i, false);
							}
						}
					}
				}
				for (int k = 0; k < count; k++)
				{
					ItemStorage inv2 = this.AInvSrc[k].Inv;
					if (inv2 != null)
					{
						inv2.fineRows(false);
					}
				}
			}
			if (flag)
			{
				ItemStorageMem.Release(this.AInvSrc);
				this.deactivate(false);
			}
			else
			{
				this.deactivateTemp();
				this.progressing_target = true;
				this.current_target++;
			}
			return true;
		}

		private void fnItemMoveSliderDesc(ItemStorage Inv, STB Stb, NelItem Itm, int grade)
		{
			int count = this.AStorage[1].getCount(Itm, -1);
			Stb.AddTxA("Guild_item_deliver_count", false).TxRpl(this.CurEntry.count - count);
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				if (this.progressing_target)
				{
					if (this.ATarget.Count > this.current_target)
					{
						this.InitItemInner();
						return true;
					}
					this.progressing_target = false;
				}
				return false;
			}
			return true;
		}

		public bool isTransition()
		{
			return this.progressing_target;
		}

		public NelItemEntry CurEntry
		{
			get
			{
				if (this.ATarget.Count <= this.current_target)
				{
					return null;
				}
				return this.ATarget[this.current_target];
			}
		}

		public UiItemConfirm.FnItemConfirmFinished FD_fnItemConfirmFinished;

		protected List<NelItemEntry> ATarget;

		private int current_target;

		protected NelM2DBase M2D;

		private UiItemMove IMvCon;

		private UiBoxDesigner BxConfirm;

		private UiBoxDesigner BxL;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		private UiBoxDesigner BxCmd;

		private BtnContainer<aBtn> BConMain;

		private List<ItemStorageMem> AInvSrc;

		private FillBlock FbConfirm;

		private int fine_navi_index = -1;

		private const float btnw = 230f;

		private const float btnh = 24f;

		private UiItemManageBoxSlider[] AMng;

		protected ItemStorage[] AStorage;

		private bool progressing_target;

		public delegate bool FnItemConfirmFinished(NelItemEntry IE, bool item_delivered);
	}
}
