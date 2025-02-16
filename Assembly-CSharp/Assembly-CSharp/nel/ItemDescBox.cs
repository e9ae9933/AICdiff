using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class ItemDescBox : MsgBox, IEventWaitListener
	{
		protected override void Awake()
		{
			base.Awake();
			if (this.Md != null)
			{
				return;
			}
			base.Align(ALIGN.CENTER, ALIGNY.MIDDLE);
			this.html_mode = true;
			base.position_max_time(24, -1).appear_time(24).hideTime(26);
			base.AlignX(ALIGN.LEFT);
			IN.setZ(base.transform, 20f);
			this.ATask = new List<ItemDescBox.IdbTask>(2);
			this.make("");
			this.TxTitle = IN.CreateGob(base.gameObject, "-Item").AddComponent<TextRenderer>();
			this.TxTitle.Col(uint.MaxValue).Size(16f).LetterSpacing(0.95f);
			this.TxTitle.html_mode = true;
			this.TxHave = IN.CreateGob(base.gameObject, "-have").AddComponent<TextRenderer>();
			this.TxHave.Col(uint.MaxValue).Size(10f).LetterSpacing(0.95f);
			this.TxHave.html_mode = true;
			this.Md = this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
			this.MrdMain = this.MMRD.GetMeshRenderer(this.Md);
			this.Md.chooseSubMesh(1, false, true);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(0, false, false);
			this.GobScreen = new GameObject("ItemDesc-Screen");
			this.GobScreen.layer = IN.gui_layer;
			this.MdScreen = MeshDrawer.prepareMeshRenderer(this.GobScreen, MTRX.MtrMeshMul, 0f, -1, null, false, false);
			this.MdScreen.base_z = 0f;
			this.VltScreen = ValotileRenderer.Create(this.MdScreen, this.GobScreen, false);
			this.GobScreen.SetActive(false);
			this.Ma = this.MMRD.GetArranger(this.Md);
			base.gameObject.SetActive(false);
			base.enabled = false;
		}

		public override void destruct()
		{
			base.destruct();
			if (this.MdScreen != null)
			{
				this.MdScreen.destruct();
			}
		}

		public override bool use_valotile
		{
			get
			{
				return base.use_valotile;
			}
			set
			{
				base.use_valotile = value;
				this.TxTitle.use_valotile = value;
				this.TxHave.use_valotile = value;
				this.VltScreen.enabled = value;
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || (this.ATask.Count > 0 && this.ATask[0].type == ItemDescBox.TYPE.FOCUS);
		}

		public ItemDescBox.IdbTask addTask(ItemDescBox.IdbTask Task)
		{
			int count = this.ATask.Count;
			int num = -1;
			for (int i = 0; i < count; i++)
			{
				ItemDescBox.IdbTask idbTask = this.ATask[i];
				if (idbTask.type <= Task.type)
				{
					if (idbTask.type != Task.type)
					{
						num = i;
						this.ATask.Insert(i, Task);
						break;
					}
					if (Task.type != ItemDescBox.TYPE.FOCUS)
					{
						num = i;
						this.ATask[i] = idbTask;
						break;
					}
				}
			}
			if (num == -1)
			{
				num = count;
				this.ATask.Add(Task);
			}
			if (num == 0)
			{
				this.readTask();
			}
			return Task;
		}

		private void readTask()
		{
			if (this.read_delay_ > 0f)
			{
				return;
			}
			if (this.ATask.Count > 0 && this.ATask[0].type == ItemDescBox.TYPE.AUTO && !Map2d.can_handle)
			{
				this.read_delay_ = X.Mx(8f, this.read_delay_);
				return;
			}
			this.readTaskExecute();
		}

		private ItemDescBox readTaskExecute()
		{
			ItemDescBox.IdbTask idbTask;
			for (;;)
			{
				this.FocusRow = null;
				this.read_delay_ = 0f;
				if (this.ATask.Count == 0)
				{
					break;
				}
				idbTask = this.ATask[0];
				this.type = idbTask.type;
				this.PosTarget = Vector4.zero;
				this.lock_input_focus = false;
				bool flag = false;
				if (idbTask.Target is ItemDescBox.IdbTask.IdbPopup)
				{
					ItemDescBox.IdbTask.IdbPopup idbPopup = idbTask.Target as ItemDescBox.IdbTask.IdbPopup;
					flag = this.makePopUp(idbPopup.p_categ, idbPopup.text, idbPopup.mapx, idbPopup.mapy, idbPopup.shifty_toupper, idbPopup.shifty_tounder, idbPopup.abs);
				}
				else if (idbTask.Target is IItemSupplier)
				{
					flag = this.make(idbTask.Target as IItemSupplier);
				}
				else if (idbTask.Target is List<NelItemEntry>)
				{
					flag = this.makeFocus(idbTask, idbTask.Target as List<NelItemEntry>);
				}
				else if (idbTask.Target is PrSkill)
				{
					flag = this.makeFocus(idbTask, idbTask.Target as PrSkill);
				}
				else if (idbTask.Target is ENHA.Enhancer)
				{
					flag = this.makeFocus(idbTask, idbTask.Target as ENHA.Enhancer);
				}
				else if (idbTask.Target is TRMManager.TRMReward)
				{
					flag = this.makeFocus(idbTask, idbTask.Target as TRMManager.TRMReward);
				}
				else if (idbTask.Target is string)
				{
					string text = idbTask.Target as string;
					if (TX.isStart(text, "MGKIND:", 0))
					{
						MGKIND mgkind;
						if (FEnum<MGKIND>.TryParse(TX.slice(text, "MGKIND:".Length), out mgkind, true))
						{
							flag = this.makeFocus(idbTask, mgkind);
						}
					}
					else if (TX.isStart(text, "MONEY:", 0))
					{
						flag = this.makeFocusMoney(idbTask, X.NmI(TX.slice(text, "MONEY:".Length), 0, false, false));
					}
				}
				if (flag)
				{
					goto IL_01F2;
				}
				this.spliceTask(0);
			}
			this.type = ItemDescBox.TYPE.AUTO;
			this.deactivate();
			return this;
			IL_01F2:
			if (idbTask.type == ItemDescBox.TYPE.FOCUS)
			{
				BGM.addHalfFlag("IDESCBOX");
			}
			this.fineTaskPosition(idbTask);
			return this;
		}

		public void fineTaskPosition(ItemDescBox.IdbTask Task)
		{
			if (Task.position_key != null && this.ATask.IndexOf(Task) == 0)
			{
				string text = Task.position_key;
				if (TX.isStart(text, "@", 0))
				{
					text = TX.slice(text, 1);
				}
				Vector4 vector;
				if (TalkDrawer.getDefinedPosition(text, out vector))
				{
					if (this.PosTarget.z == 0f)
					{
						this.PosTarget.x = this.PosTarget.x + vector.x;
						this.PosTarget.y = this.PosTarget.y + vector.y;
					}
					else
					{
						this.PosTarget.x = this.PosTarget.x + vector.x * this.M2D.curMap.rCLENB;
						this.PosTarget.y = this.PosTarget.y + vector.y * this.M2D.curMap.rCLENB;
					}
					this.runUiPosition(true);
				}
			}
		}

		public void spliceStackIf(NelItemManager.POPUP p)
		{
			bool flag = false;
			p &= (NelItemManager.POPUP)(-983041);
			for (int i = this.ATask.Count - 1; i >= 0; i--)
			{
				ItemDescBox.IdbTask idbTask = this.ATask[i];
				if (idbTask.Target is ItemDescBox.IdbTask.IdbPopup && (idbTask.Target as ItemDescBox.IdbTask.IdbPopup).p_categ == p)
				{
					this.spliceTask(i);
					if (i == 0)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.readTask();
			}
		}

		public void spliceStackIf(IItemSupplier Sup)
		{
			bool flag = false;
			for (int i = this.ATask.Count - 1; i >= 0; i--)
			{
				ItemDescBox.IdbTask idbTask = this.ATask[i];
				if (idbTask.Target is IItemSupplier && idbTask.Target as IItemSupplier == Sup)
				{
					this.spliceTask(i);
					if (i == 0)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.readTask();
			}
		}

		private void spliceTask(int i = 0)
		{
			if (i >= this.ATask.Count)
			{
				return;
			}
			ItemDescBox.IdbTask idbTask = this.ATask[i];
			if (idbTask.type == ItemDescBox.TYPE.FOCUS)
			{
				BGM.remHalfFlag("IDESCBOX");
			}
			if (idbTask.Target is IItemSupplier && idbTask.Target as IItemSupplier == this.NextSplTarget)
			{
				this.NextSplTarget = null;
			}
			this.ATask.RemoveAt(i);
			if (i == 0)
			{
				this.deactivate();
			}
		}

		public void clearStack()
		{
			this.NextSplTarget = null;
			this.ATask.Clear();
			this.read_delay_ = 0f;
			BGM.remHalfFlag("IDESCBOX");
			this.lock_input_focus = false;
			this.readTaskExecute();
		}

		public ItemDescBox addTaskSupplier(IItemSupplier _Target)
		{
			if (_Target == null)
			{
				if (this.NextSplTarget != null)
				{
					this.spliceStackIf(this.NextSplTarget);
				}
				this.NextSplTarget = null;
				return this;
			}
			this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.AUTO, _Target, false));
			this.NextSplTarget = _Target;
			return this;
		}

		public ItemDescBox.IdbTask addTaskPopUp(NelItemManager.POPUP p_categ, string text, float mapx, float mapy, float shifty_toupper = 0f, float shifty_tounder = 0f, bool abs = false)
		{
			ItemDescBox.TYPE type = ItemDescBox.TYPE.POPUP;
			if ((p_categ & NelItemManager.POPUP._FOCUS_MODE) != NelItemManager.POPUP.HIDE)
			{
				p_categ &= (NelItemManager.POPUP)(-65537);
				type = ItemDescBox.TYPE.FOCUS;
			}
			return this.addTask(new ItemDescBox.IdbTask(type, new ItemDescBox.IdbTask.IdbPopup(p_categ, text, mapx, mapy, shifty_toupper, shifty_tounder, abs), false));
		}

		public ItemDescBox.IdbTask addTaskFocus(PrSkill _Target, bool is_supertop = false)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, _Target, is_supertop));
		}

		public ItemDescBox.IdbTask addTaskFocus(ENHA.Enhancer _Target, bool is_supertop = false)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, _Target, is_supertop));
		}

		public ItemDescBox.IdbTask addTaskFocus(MGKIND _Target)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, "MGKIND:" + _Target.ToString(), false));
		}

		public ItemDescBox.IdbTask addTaskFocus(List<NelItemEntry> _Target)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, _Target, false));
		}

		public ItemDescBox.IdbTask addTaskFocus(TRMManager.TRMReward _Target)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, _Target, false));
		}

		public ItemDescBox.IdbTask addTaskFocusMoney(int money)
		{
			return this.addTask(new ItemDescBox.IdbTask(ItemDescBox.TYPE.FOCUS, "MONEY:" + money.ToString(), false));
		}

		private float Set1(int _radius, bool bkg_scaling, float lr_margin, float tb_margin, float desc_txsize, float itemtitle_txsize, Color32[] Acol_basebox)
		{
			if (this.Md == null)
			{
				this.Awake();
			}
			base.position_max_time(24, -1).appear_time(24).hideTime(26)
				.col(3707764736U)
				.TxCol(uint.MaxValue)
				.bg_border(MTRX.ColTrnsp, 0f)
				.AlignX(ALIGN.LEFT);
			this.delayt = 0;
			this.radius = _radius;
			this.ginitted = false;
			this.lock_input_focus = false;
			this.frame_color = (this.letter_line_color = 0U);
			this.ShadowCol = MTRX.ColTrnsp;
			this.frame_start_vertex = (this.frame_start_tri = -1);
			this.open_snd = null;
			IN.setZ(base.transform, 0.0015f - this.MMRD.slip_z);
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			this.TrmRwd = null;
			CameraBidingsBehaviour.UiBind.need_sort_binds = true;
			this.PosTarget = Vector4.zero;
			this.AEntryReel = null;
			this.reel_t = 0f;
			if (this.Tx != null)
			{
				this.Tx.auto_wrap = false;
				this.Tx.max_swidth_px = 0f;
				this.Tx.HeadXShift(0f);
			}
			base.bkg_scale(bkg_scaling, bkg_scaling, true);
			base.TargetFont = null;
			uint num;
			if (Acol_basebox == ItemDescBox.Acol_focus || Acol_basebox == ItemDescBox.Acol_focus2)
			{
				num = 4283780170U;
				this.ShadowCol = C32.MulA(4282004532U, 0.25f);
			}
			else
			{
				num = uint.MaxValue;
			}
			base.TxSize(desc_txsize).TxCol(num).Align(ALIGN.LEFT, ALIGNY.TOP)
				.LineSpacing(1.5f);
			base.margin(new float[] { lr_margin, tb_margin, lr_margin, tb_margin });
			this.TxTitle.Size(itemtitle_txsize).Col(num);
			this.TxTitle.auto_condense = false;
			this.TxHave.Size(itemtitle_txsize * 10f / 16f).Col(num);
			this.TxHave.auto_condense = false;
			this.TxTitle.TargetFont = null;
			this.TxHave.TargetFont = null;
			this.ScreenTask = null;
			float num2 = 1.875f * this.TxTitle.size;
			this.TxTitle.LineSpacePixel(num2);
			this.TxHave.LineSpacePixel(num2).Align(ALIGN.LEFT).AlignY(ALIGNY.TOP);
			this.gradation(Acol_basebox, null);
			this.Md.clear(false, false);
			this.Md.chooseSubMesh(1, false, true);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Md.chooseSubMesh(0, false, true);
			this.Md.Col = MTRX.ColWhite;
			this.Ma.clear(this.Md);
			this.TxTitle.text_content = "";
			this.TxHave.text_content = "";
			return num2;
		}

		private void SetToMapPos(float mapx, float mapy, float shifty_toupper = 0f, float shifty_tounder = 0f)
		{
			this.PosTarget.Set(mapx, mapy, X.Mx(0.125f, shifty_toupper), shifty_tounder);
			this.runUiPosition(true);
		}

		private void popupMakingFinalize(bool pre_active)
		{
			if (pre_active)
			{
				this.T_SHOW = 30;
				this.showt = 0;
				this.text_alpha = 0f;
			}
			else
			{
				this.T_SHOW = 10;
				this.showt = 0;
				this.text_alpha = 0f;
			}
			this.Md.connectRendererToTriMulti(this.MrdMain);
			this.frame_start_vertex = (this.frame_start_tri = -1);
			this.fineTextAlpha(true);
		}

		private bool makePopUp(NelItemManager.POPUP p_categ, string text, float mapx, float mapy, float shifty_toupper = 0f, float shifty_tounder = 0f, bool abs = false)
		{
			string text2 = null;
			int num = 40;
			int num2 = 28;
			Color32[] array = ItemDescBox.Acol_normal;
			uint num3 = 0U;
			uint num4 = 0U;
			uint num5 = 0U;
			int num6 = 24;
			ALIGN align = ALIGN.LEFT;
			if ((p_categ & NelItemManager.POPUP._TX_CENTER) != NelItemManager.POPUP.HIDE)
			{
				p_categ &= (NelItemManager.POPUP)(-4097);
				align = ALIGN.CENTER;
			}
			if (p_categ != NelItemManager.POPUP.FRAMED_BOARD)
			{
				if (p_categ == NelItemManager.POPUP.FRAMED_PAPER)
				{
					num = 120;
					num2 = 178;
					num3 = 4283780170U;
					array = ItemDescBox.Acol_focus2;
					num6 = 0;
					text2 = "paper";
					num4 = 4283780170U;
					num5 = C32.c2d(C32.MulA(4283780170U, 0.25f));
				}
			}
			else
			{
				text2 = "tool_selrect";
				num = 50;
				num2 = 68;
				num6 = 48;
				num3 = uint.MaxValue;
			}
			this.Set1(num6, true, (float)num, (float)num2, 18f, 18f, array);
			base.AlignX(align);
			this.frame_color = num3;
			this.letter_line_color = num5;
			if (num4 != 0U)
			{
				base.bg_border(C32.d2c(num4), 1f);
			}
			this.open_snd = text2;
			bool flag = false;
			if (!base.isActive())
			{
				this.delayt = 0;
			}
			else
			{
				flag = true;
			}
			this.activate();
			this.make(text);
			this.wh(this.Tx.get_swidth_px(), this.Tx.get_sheight_px());
			this.popupMakingFinalize(flag);
			if (abs)
			{
				this.PosTarget.Set(0f, 0f, 0f, mapy);
				this.delayt = 18;
				this.runUiPosition(true);
			}
			else
			{
				this.SetToMapPos(mapx, mapy, shifty_toupper, shifty_tounder);
			}
			return true;
		}

		private STB getItemEntry(STB Stb, NelItemEntry E, int entry_count = -1)
		{
			if (entry_count < 0)
			{
				entry_count = E.count;
			}
			Stb.Add(E.Data.getLocalizedName((int)E.grade));
			if (!E.Data.has(NelItem.CATEG.INDIVIDUAL_GRADE))
			{
				Stb.Add("<img mesh=\"nel_item_grade.", (int)E.grade, "\" />");
			}
			if (entry_count > 1)
			{
				Stb.Add(" x", entry_count, "");
			}
			return Stb;
		}

		private STB getHaving(STB Stb, NelItem E, int grade)
		{
			return Stb.AddTxA("Item_has", false).TxRpl(this.M2D.IMNG.getInventory().getCount(E, grade));
		}

		private bool make(IItemSupplier SplTarget)
		{
			if (SplTarget == null)
			{
				return false;
			}
			float num = this.Set1(24, false, 60f, 18f, 12f, 16f, ItemDescBox.Acol_normal);
			bool flag = false;
			if (!base.isActive())
			{
				this.delayt = 10;
			}
			else
			{
				flag = true;
			}
			bool flag2 = false;
			NelItemEntry[] dataList = SplTarget.getDataList(ref flag2);
			int num2 = dataList.Length;
			if (num2 == 0)
			{
				return false;
			}
			if (flag2)
			{
				num2 = 1;
				this.AEntryReel = dataList;
			}
			this.activate();
			float num3 = 280f;
			this.TxTitle.auto_condense = true;
			this.TxTitle.max_swidth_px = 220f;
			this.TxHave.auto_condense = true;
			this.TxHave.max_swidth_px = 65f;
			bool flag3 = false;
			NelItemEntry nelItemEntry = null;
			float num4 = 0f;
			int num5 = 0;
			int num6 = 0;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					using (STB stb3 = TX.PopBld(null, 0))
					{
						for (int i = 0; i <= num2; i++)
						{
							NelItemEntry nelItemEntry2 = ((i == num2) ? null : dataList[i]);
							if (nelItemEntry != null && (nelItemEntry2 == null || !nelItemEntry2.isSame(nelItemEntry)))
							{
								using (STB stb4 = TX.PopBld(null, 0))
								{
									stb2.Append(this.getItemEntry(stb4, nelItemEntry, -1), "\n", 0, -1);
									stb4.Clear();
									stb3.Append(this.getHaving(stb4, nelItemEntry.Data, flag2 ? (-1) : ((int)nelItemEntry.grade)), "\n", 0, -1);
								}
								this.Md.chooseSubMesh(1, false, false);
								nelItemEntry.Data.touchObtainCount();
								if (nelItemEntry.Data.is_reelmbox)
								{
									ReelManager.ItemReelContainer ir = ReelManager.GetIR(nelItemEntry.Data);
									if (ir != null)
									{
										ir.drawSmallIcon(this.Md, 0f, num4, 1f, 1f, false);
									}
								}
								else
								{
									this.Md.RotaPF(0f, num4, 1f, 1f, 0f, MTR.AItemIcon[nelItemEntry.Data.getIcon(this.M2D.IMNG.getInventory(), null)], false, false, false, uint.MaxValue, false, 0);
								}
								num4 -= num;
								num6++;
								num5 = 0;
							}
							if (i == num2)
							{
								break;
							}
							NelItem nelItem = ((nelItemEntry2 != null) ? nelItemEntry2.Data : null);
							if (nelItem.has(NelItem.CATEG.WATER) && !flag3)
							{
								flag3 = true;
								if (this.M2D.IMNG.hasEmptyBottle())
								{
									stb.AppendTxA("Item_dbox_water_to_bottle", "\n").TxRpl(nelItemEntry2.Data.stock);
								}
								else if (this.M2D.IMNG.canAddItem(nelItem, 1, true) == 0)
								{
									stb.AppendTxA("Item_dbox_water_cannot_obtain", "\n");
								}
							}
							nelItemEntry = nelItemEntry2;
							num5 += nelItemEntry2.count;
						}
						this.TxTitle.Txt(stb2);
						this.TxHave.Txt(stb3);
					}
				}
				if (num6 == 1 && !dataList[0].Data.is_precious)
				{
					stb.AppendTxA("Item_dbox_stock", "\n").TxRpl(dataList[0].Data.stock);
				}
				string text = stb.ToString();
				this.tmarg = this.bmarg + num * (float)num6 + (float)((text == "") ? 0 : 10);
				this.make(text);
				this._appear_time = 26;
				this.wh(num3, (text == "") ? 1f : (this.Tx.get_sheight_px() + 10f));
			}
			float num7 = -base.get_swidth_px() / 2f + 42f;
			float num8 = base.get_sheight_px() * 0.5f - 40f;
			this.Ma.SetWhole(true);
			this.Ma.translateAll(num7, num8 + 6f, false);
			this.text_alpha = 0f;
			IN.PosP(this.TxTitle.transform, num7 + this.TxTitle.size * 1.376f, num8 + num / 2f - 4f, -0.01f);
			IN.PosP(this.TxHave.transform, base.get_swidth_px() / 2f - 40f - this.TxHave.get_swidth_px(), num8 + num / 2f - 4f, -0.01f);
			if (flag3)
			{
				this.M2D.IMNG.getInventory().drawTempBottle(this.Md, 1, 1, base.get_swidth_px() / 2f - this.rmarg + 4f, (float)((int)(-base.get_sheight_px() / 2f + this.bmarg + this.h / 2f)), false, uint.MaxValue);
			}
			this.Md.chooseSubMesh(0, false, false);
			this.Ma.SetWhole(true);
			this.popupMakingFinalize(flag);
			Vector4 showPos = SplTarget.getShowPos();
			this.SetToMapPos(showPos.x, showPos.y, showPos.z, showPos.w);
			return true;
		}

		private bool makeFocus(ItemDescBox.IdbTask Task, List<NelItemEntry> AItm)
		{
			if (AItm == null || AItm.Count == 0)
			{
				return false;
			}
			this.FocusRow = AItm;
			float num = this.Set1(44, true, 90f, 72f, 16f, 24f, ItemDescBox.Acol_focus);
			this.activate();
			this.PosTarget.Set(0f, 0f, 0f, IN.hh * 0.37f);
			this.tmarg = this.bmarg + num * (float)AItm.Count;
			int count = AItm.Count;
			using (STB stb = TX.PopBld(null, 0))
			{
				AItm[count - 1].Data.getDescLocalized(stb, null, (int)AItm[0].grade);
				string text = stb.ToString();
				stb.Clear();
				float num2 = (this.M2D.Ui.draw_letter_box ? 580f : 330f);
				using (STB stb2 = TX.PopBld(null, 0))
				{
					for (int i = 0; i < count; i++)
					{
						NelItemEntry nelItemEntry = AItm[i];
						if (stb.Length > 0)
						{
							stb.Ret("\n");
						}
						nelItemEntry.Data.getLocalizedName(stb, (int)nelItemEntry.grade);
						if (nelItemEntry.grade >= 1 && !nelItemEntry.Data.individual_grade)
						{
							stb.Add("<img mesh=\"nel_item_grade.", (int)nelItemEntry.grade, "\" tx_color />");
						}
						stb2.AppendTxA("Item_get_count", "\n").TxRpl(nelItemEntry.count);
					}
					this.TxTitle.Txt(stb);
					this.TxHave.Txt(stb2);
				}
				this.make(text);
				this.Tx.auto_wrap = true;
				this.Tx.max_swidth_px = num2;
				this.Tx.Redraw(false);
				this.frame_color = 4283780170U;
				this._appear_time = 65;
				this.wh(num2, (text == "") ? 1f : this.Tx.get_sheight_px());
			}
			float num3 = base.get_sheight_px() / 2f - this.bmarg - num / 2f + 4f;
			float num4 = -base.get_swidth_px() / 2f + 64f;
			IN.PosP(this.TxTitle.transform, num4 + this.TxTitle.size * 1.376f, num3 + num / 2f - 4f, -0.01f);
			IN.PosP(this.TxHave.transform, base.get_swidth_px() / 2f - 93f, num3 + num / 2f - 4f, -0.01f);
			for (int j = 0; j < count; j++)
			{
				AItm[j].Data.drawIconTo(this.Md, this.M2D.IMNG.getInventory(), 0, 1, num4, num3 + 9f, 1f, this.alpha_, null);
				num3 += num;
			}
			this.focusMakeFinalize(Task, "item_get_fanfare", false, false);
			return true;
		}

		private bool makeFocusMoney(ItemDescBox.IdbTask Task, int money)
		{
			if (money <= 0)
			{
				return false;
			}
			this.Set1(44, true, 70f, 68f, 16f, 24f, ItemDescBox.Acol_focus);
			this.activate();
			this.make(TX.GetA("MoneyBox_adding", money.ToString()));
			this._appear_time = 65;
			this.frame_color = 4283780170U;
			base.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
			this.Tx.auto_wrap = true;
			this.Tx.max_swidth_px = 330f;
			this.Tx.Redraw(false);
			this.wh(330f, this.Tx.get_sheight_px());
			this.focusMakeFinalize(Task, null, false, false);
			return true;
		}

		private bool makeFocus(ItemDescBox.IdbTask Task, TRMManager.TRMReward Rwd)
		{
			if (Rwd == null)
			{
				return false;
			}
			this.Set1(44, true, 0f, 0f, 16f, 16f, ItemDescBox.Acol_focus);
			base.margin(new float[] { 120f, 74f, 291.8f, 65f });
			this.TxHave.Size(this.Tx.size).Align(ALIGN.RIGHT);
			this.TrmRwd = Rwd;
			this.activate();
			this.make(" ");
			this._appear_time = 65;
			this.frame_color = 4283780170U;
			this.Tx.HeadXShift(-70f);
			this.Tx.auto_wrap = false;
			this.Tx.auto_condense = false;
			this.Tx.max_swidth_px = 780f - this.lmarg - this.rmarg;
			base.swh(780f, 255f + this.tmarg + this.bmarg);
			IN.PosP2(this.TxHave.transform, 256f, base.sheight * 0.5f - this.tmarg);
			this.Tx.line_spacing = (this.TxHave.line_spacing = 1.9f);
			this.reel_t = -1f;
			this.focusMakeFinalize(Task, null, false, false);
			return true;
		}

		private ItemDescBox makeFocus(ItemDescBox.IdbTask Task, MGKIND mg)
		{
			if (mg == MGKIND.NONE)
			{
				this.deactivate();
				return this;
			}
			this.makeBigItemGet(Task, TX.Get("Mag_title_" + mg.ToString().ToLower(), ""), TX.Get("Mag_desc_" + mg.ToString().ToLower(), ""), "get_magic", true);
			MagicSelector.getKind2IconId(mg);
			return this;
		}

		private bool makeFocus(ItemDescBox.IdbTask Task, PrSkill Sk)
		{
			if (Sk == null)
			{
				return false;
			}
			string text = TX.trim(Sk.manipulate);
			this.makeBigItemGet(Task, Sk.title, ((text != "") ? (text + "\n") : "") + Sk.descript, "get_skill", Sk.getPF() != null);
			Sk.getPF();
			return true;
		}

		private bool makeFocus(ItemDescBox.IdbTask Task, ENHA.Enhancer Eh)
		{
			if (Eh == null)
			{
				return false;
			}
			this.makeBigItemGet(Task, Eh.title, Eh.descript, "get_enhancer", false);
			ENHA.SqImgIcon.getFrameByName(Eh.key);
			return true;
		}

		private ItemDescBox makeBigItemGet(ItemDescBox.IdbTask Task, string title, string desc, string snd_key, bool use_icon_l = false)
		{
			this.Set1(0, false, 80f, 80f, 14f, 18f, ItemDescBox.Acol_trnsp);
			this.activate();
			this.PosTarget.Set(0f, 0f, 0f, 40f);
			this.TxHave.Size(18f).LineSpacing(1.25f);
			this.TxHave.text_content = desc;
			base.TxSize(40f).LineSpacing(4f);
			base.TargetFont = TX.getTitleFont();
			this.wh(330f, 200f + this.TxHave.get_sheight_px());
			this.make("\u3000\n" + title);
			base.Align(ALIGN.CENTER, ALIGNY.TOP);
			this._appear_time = 65;
			this.frame_color = 0U;
			IN.PosP(this.TxHave.transform, 0f, -this.h * 0.5f + this.TxHave.get_sheight_px() / 2f - 30f, -0.01f);
			this.TxHave.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			this.focusMakeFinalize(Task, snd_key, true, use_icon_l);
			return this;
		}

		private void focusMakeFinalize(ItemDescBox.IdbTask Task, string snd, bool use_screen, bool use_icon_l = false)
		{
			this.text_alpha = 0f;
			this.Md.chooseSubMesh(0, false, false);
			this.Ma.SetWhole(true);
			this.fineTextAlpha(true);
			this.Md.updateForMeshRenderer(true);
			this.post = -1;
			if (TX.valid(snd))
			{
				M2DBase.playSnd(snd);
			}
			this.runUiPosition(true);
			IN.setZAbs(base.transform, -4f - (Task.supertop ? 0.66f : 0.2f));
			if (use_screen)
			{
				this.MdScreen.clear(false, false);
				this.Md.chooseSubMesh(1, false, false);
				Material mtr = (use_icon_l ? MTR.MIiconL : MTRX.MIicon).getMtr(BLEND.NORMAL, -1);
				if (this.Md.getMaterial() != mtr)
				{
					this.Md.setMaterial(mtr, false);
				}
				IN.setZ(this.GobScreen.transform, Task.supertop ? (-4.63f) : 20.16f);
				this.GobScreen.SetActive(true);
				this.VltScreen.enabled = this.use_valotile;
				this.ScreenTask = this.ATask[0];
				if (this.t_screen < 0f)
				{
					this.t_screen = X.Mx(0f, 60f + this.t_screen);
				}
			}
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(this.MrdMain);
		}

		protected override MsgBox setBkgScale(float tz)
		{
			float num = tz;
			if (base.isActive() && this.type == ItemDescBox.TYPE.FOCUS)
			{
				num = ((tz >= 1f) ? 1f : (1.125f * X.ZPOW(tz, 0.5f) - 0.25f * X.ZSIN(tz - 0.5f, 0.3f) + 0.125f * X.ZPOW(tz - 0.65f, 0.35000002f)));
			}
			return base.setBkgScale(num);
		}

		public float read_delay
		{
			get
			{
				return this.read_delay_;
			}
			set
			{
				if (value == this.read_delay_)
				{
					return;
				}
				this.read_delay_ = value;
			}
		}

		public override MsgBox run(float fcnt, bool force_draw = false)
		{
			if (this.t_screen >= 0f)
			{
				this.t_screen += fcnt;
				if (X.D)
				{
					this.drawHideScreen();
				}
			}
			else if (this.t_screen > -60f)
			{
				this.t_screen -= fcnt;
				if (this.t_screen <= -60f)
				{
					this.GobScreen.SetActive(false);
					this.ScreenTask = null;
				}
				else if (X.D)
				{
					this.drawHideScreen();
				}
			}
			if (this.read_delay_ > 0f)
			{
				this.read_delay_ -= fcnt;
				if (this.read_delay_ <= 0f)
				{
					this.read_delay_ = 0f;
					this.readTask();
				}
				if (!base.visible)
				{
					return this;
				}
			}
			bool flag = this.is_focus && this.t >= 20f && (!this.lock_input_focus || !EV.isActive(false) || EV.isWaiting(this));
			if (this.TrmRwd != null)
			{
				this.runRewardText((this.t >= 0f) ? this.t : 1000f, ref flag);
			}
			if ((flag && (IN.isCancelPD() || IN.isSubmitPD(1) || IN.isCheckPD(1) || IN.isMousePushDown(1))) || (this.ATask.Count == 0 && base.isActive()))
			{
				this.read_delay_ = 25f;
				SND.Ui.play("talk_progress", false);
				int count = this.ATask.Count;
				if (count > 0 && this.ATask[0].Target is ItemDescBox.IdbTask.IdbPopup)
				{
					ItemDescBox.IdbTask.IdbPopup idbPopup = this.ATask[0].Target as ItemDescBox.IdbTask.IdbPopup;
					(M2DBase.Instance as NelM2DBase).IMNG.hidePopUp(idbPopup.p_categ);
					if (this.ATask.Count == count)
					{
						this.spliceTask(0);
					}
				}
				else
				{
					this.spliceTask(0);
				}
				this.deactivate();
			}
			if (base.run(fcnt, force_draw) == null)
			{
				this.AEntryReel = null;
				return null;
			}
			if (this.AEntryReel != null && this.AEntryReel.Length > 1)
			{
				int num = (int)(this.reel_t / 10f);
				this.reel_t += fcnt;
				int num2 = (int)(this.reel_t / 10f);
				if (num != num2)
				{
					NelItemEntry nelItemEntry = this.AEntryReel[num];
					num2 %= this.AEntryReel.Length;
					this.reel_t = (float)(num2 * 10);
					NelItemEntry nelItemEntry2 = this.AEntryReel[num2];
					if (nelItemEntry.Data != nelItemEntry2.Data)
					{
						using (STB stb = TX.PopBld(null, 0))
						{
							this.TxHave.Txt(this.getHaving(stb, nelItemEntry.Data, -1));
						}
					}
					using (STB stb2 = TX.PopBld(null, 0))
					{
						this.TxTitle.Txt(this.getItemEntry(stb2, nelItemEntry2, -1));
					}
				}
			}
			if (X.D)
			{
				this.runUiPosition(false);
			}
			return this;
		}

		public void runUiPosition(bool out_pos = false)
		{
			if (this.PosTarget.z == 0f)
			{
				float num = this.M2D.ui_shift_x * 0.5f;
				base.posSetA(this.PosTarget.x + num, this.PosTarget.y - this.PosTarget.w, this.PosTarget.x + num, this.PosTarget.y, !out_pos);
				return;
			}
			if (base.visible)
			{
				M2BoxOneLine.fineBoxPosOnMap(this, M2DBase.Instance, this.PosTarget, out_pos, false, 0f, 0f);
			}
		}

		public void runRewardText(float t, ref bool cancelable)
		{
			if (this.TrmRwd == null)
			{
				return;
			}
			int num = ((this.reel_t >= 100f) ? 101 : ((t < 20f) ? (-1) : ((int)X.Mn(20f, (t - 20f) / 26f))));
			if ((float)num > this.reel_t)
			{
				this.reel_t = (float)num;
				bool flag = num == 0 || num == 101;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("<font size=\"20\">").AddTxA(this.TrmRwd.success ? "MoneyBox_trm_reward_success" : "MoneyBox_trm_reward_failure", false);
					stb.TxRpl(this.TrmRwd.Target.getNameLocalized()).Add("   </font>");
					if (num >= 4)
					{
						num -= 4;
						flag = flag || (num <= 6 && num != 4);
						stb.Ret("\n").Ret("\n").AddTxA("MoneyBox_trm_reward_base", false);
						if (num >= 2)
						{
							stb.Ret("\n").AddTxA("MoneyBox_trm_reward_grade", false).TxRpl(this.TrmRwd.grade);
						}
						if (num >= 5)
						{
							stb.Ret("\n").Ret("\n").Add("<font size=\"20\"> \n")
								.AddTxA("MoneyBox_trm_reward_total", false);
							stb.Add("</font>");
						}
						if (num >= 1)
						{
							using (STB stb2 = TX.PopBld("<font size=\"20\"> \n </font>\n \n", 0))
							{
								stb2.AddTxA("MoneyBox_num", false).TxRpl(this.TrmRwd.base_reward_spr5);
								if (num >= 3)
								{
									stb2.Ret("\n");
									if (this.TrmRwd.grade > 0)
									{
										stb2.Add("<font color=\"ff:#1335DF\">").AddTxA("MoneyBox_trm_reward_chip", false);
										stb2.AddTxA("MoneyBox_num", false).TxRpl(this.TrmRwd.chip_reward_spr5);
										stb2.Add("</font>");
									}
									else
									{
										stb2.AddTxA("MoneyBox_trm_reward_chip", false);
										stb2.AddTxA("MoneyBox_num", false).TxRpl("0");
									}
									if (num >= 6)
									{
										stb2.Ret("\n").Ret("\n").Add("<font size=\"20\">\n")
											.AddTxA("MoneyBox_num", false)
											.TxRpl(this.TrmRwd.total_reward_spr5);
										stb2.Add("</font>");
									}
								}
								this.TxHave.Txt(stb2);
							}
						}
					}
					this.Tx.Txt(stb);
				}
				if (flag)
				{
					SND.Ui.play("talk_progress", false);
				}
			}
			if (num < 10)
			{
				cancelable = false;
			}
			if (!cancelable && t >= 30f && base.isActive() && IN.ketteiPD(1))
			{
				this.reel_t = 100f;
				IN.clearPushDown(false);
			}
		}

		public override MsgBox deactivate()
		{
			if (base.isActive())
			{
				base.deactivate();
				if (this.t_screen >= 0f)
				{
					this.t_screen = X.Mn(-1f, -59f + this.t_screen);
				}
				this.runUiPosition(true);
			}
			return this;
		}

		public void deactivateFinalize()
		{
			if (!base.isActive() && this.ATask.Count == 0)
			{
				try
				{
					if (base.visible)
					{
						base.alpha = 0f;
						base.visible = false;
						this.t = (float)(-(float)this.t_hide);
					}
				}
				catch
				{
				}
				try
				{
					if (this.t_screen > -60f)
					{
						if (this.MdScreen != null)
						{
							this.MdScreen.clear(false, false);
						}
						this.t_screen = -60f;
						this.GobScreen.SetActive(false);
						this.ScreenTask = null;
					}
				}
				catch
				{
				}
			}
		}

		public bool is_focus
		{
			get
			{
				return this.type == ItemDescBox.TYPE.FOCUS;
			}
		}

		protected override MsgBox fineTextAlpha()
		{
			this.fineTextAlpha(false);
			return this;
		}

		protected MsgBox fineTextAlpha(bool first)
		{
			base.fineTextAlpha();
			bool flag = first;
			if (this.TxTitle != null)
			{
				float num = this.text_alpha * this.alpha_;
				this.TxTitle.alpha = num;
				this.TxHave.alpha = num;
				this.Ma.setAlpha1(num, false);
				if (this.Md.getMesh().subMeshCount >= 3)
				{
					this.Md.getMesh().GetTriangles(0);
					this.Md.getMesh().GetTriangles(2);
				}
				flag = true;
			}
			if ((this.frame_color & 4278190080U) != 0U)
			{
				this.Md.chooseSubMesh(1, false, false);
				if (this.frame_start_vertex < 0)
				{
					this.frame_start_vertex = this.Md.getVertexMax();
					this.frame_start_tri = this.Md.getTriMax();
				}
				else
				{
					this.Md.revertVerAndTriIndex(this.frame_start_vertex, this.frame_start_tri, false);
				}
				float num2 = this.text_alpha * this.alpha_;
				float num3 = (base.get_sheight_px() * 0.5f - 42f) * X.NI(0.75f, 1f, base.isActive() ? X.ZSIN(num2) : num2);
				this.Md.Col = C32.MulA(this.frame_color, num2);
				for (int i = 0; i < 4; i++)
				{
					this.Md.Identity().Scale((float)X.MPF(i < 2), (float)X.MPF(i % 2 == 1), false);
					this.Md.initForImg(MTRX.ItemGetDescFrame, 0);
					this.Md.DrawScaleGraph2(-4f, num3, 1f, 0f, 1f, 1f, null);
				}
				this.Md.Identity();
				flag = true;
			}
			if (flag)
			{
				this.Md.updateForMeshRenderer(true);
			}
			return this;
		}

		protected override void redrawBgMeshInner(MeshDrawer Md, MdArranger Ma, float wpx, float hpx)
		{
			if (this.ShadowCol.a != 0)
			{
				float num = X.Mn(160f, X.Mn(wpx, hpx) * 0.5f);
				Color32 col = Md.Col;
				Md.Col = this.ShadowCol;
				Md.ColGrd.Set(this.ShadowCol).setA(0f);
				Md.KadomaruRect(0f, 0f, wpx + num * 2f, hpx + num * 2f, (float)this.radius + num, num, false, 1f, 0f, true);
				Md.Col = col;
			}
			base.redrawBgMeshInner(Md, Ma, wpx, hpx);
		}

		protected override MsgBox redrawBg(float w = 0f, float h = 0f, bool update_mrd = true)
		{
			if (!this.ginitted)
			{
				return this;
			}
			base.redrawBg(w, h, false);
			if (this.letter_line_color > 0U && this.alpha_ > 0.02f)
			{
				if (!this.MdBkg.hasMultipleTriangle())
				{
					this.MdBkg.chooseSubMesh(1, false, false);
					Material material = MTRX.newMtr("Hachan/ShaderMeshStriped");
					material.SetFloat("_TimeScale", 0f);
					this.MdBkg.setMaterial(material, true);
					this.MdBkg.connectRendererToTriMulti(this.GobBkg.GetComponent<MeshRenderer>());
				}
				else
				{
					this.MdBkg.chooseSubMesh(1, false, true);
				}
				this.MdBkg.Identity();
				this.MdBkg.Col = C32.MulA(this.letter_line_color, this.alpha_);
				int num = this.Tx.countLines();
				float line_spacing_pixel = this.Tx.line_spacing_pixel;
				float num2 = -this.Tx.get_sheight_px() * 0.5f - 3f;
				float num3 = this.w * 0.5f + 12f;
				this.MdBkg.allocRevtVerTri(num + 1, 60);
				this.MdBkg.uvRectN(1f, 0f);
				this.MdBkg.allocUv2((num + 1) * 4, false).Uv2(-15f, 0.5f, false);
				for (int i = 0; i <= num; i++)
				{
					this.MdBkg.Rect(0f, num2, num3 * 2f, 1.5f, false);
					num2 += line_spacing_pixel;
				}
				this.MdBkg.allocUv2(0, true);
				this.MdBkg.chooseSubMesh(0, false, false);
			}
			if (update_mrd)
			{
				this.MdBkg.updateForMeshRenderer(true);
			}
			return this;
		}

		private void drawHideScreen()
		{
			float num = ((this.t_screen >= 0f) ? X.ZLINE(this.t_screen, 60f) : X.ZLINE(60f + this.t_screen, 60f));
			if (num <= 0f)
			{
				return;
			}
			this.MdScreen.clear(false, false);
			this.MdScreen.Col = C32.MulA(2332033024U, num);
			this.MdScreen.Rect(0f, 0f, IN.w + 340f + 40f, IN.h + 40f, false);
			this.MdScreen.updateForMeshRenderer(false);
			if (this.ScreenTask == null)
			{
				return;
			}
			float num2 = 0f;
			PxlFrame pxlFrame = null;
			PxlFrame pxlFrame2 = null;
			float num3 = 90f;
			bool flag = true;
			bool flag2 = false;
			this.Md.clear(false, false);
			this.Md.chooseSubMesh(1, false, true);
			Color32 color = (this.Md.Col = this.Md.ColGrd.White().mulA(num).C);
			this.Md.ColGrd.setA(0f);
			if (this.ScreenTask.Target is PrSkill)
			{
				num2 = 2f;
				PrSkill prSkill = this.ScreenTask.Target as PrSkill;
				pxlFrame2 = prSkill.getPF();
				if (pxlFrame2 == null)
				{
					NelItem bookItem = prSkill.GetBookItem();
					if (bookItem != null)
					{
						pxlFrame2 = MTRX.getPF("itemrow_category." + bookItem.specific_icon_id.ToString());
						num2 = 4f;
					}
				}
			}
			else if (this.ScreenTask.Target is ENHA.Enhancer)
			{
				num2 = 4f;
				pxlFrame = (this.ScreenTask.Target as ENHA.Enhancer).PF;
				flag2 = true;
			}
			else if (this.ScreenTask.Target is string)
			{
				string text = this.ScreenTask.Target as string;
				num2 = 1f;
				if (TX.isStart(text, "MGKIND:", 0))
				{
					MGKIND mgkind;
					FEnum<MGKIND>.TryParse(TX.slice(text, "MGKIND:".Length), out mgkind, true);
					int kind2IconId = MagicSelector.getKind2IconId(mgkind);
					if (kind2IconId >= 0)
					{
						pxlFrame2 = MTR.AMagicIconL[kind2IconId];
					}
					flag = false;
				}
			}
			float num4 = num3 + ((this.t_screen < 0f) ? 1f : X.ZSIN(num)) * 100f;
			if (pxlFrame2 != null)
			{
				this.Md.RotaPF(0f, num4, num2, num2, 0f, pxlFrame2, false, false, false, uint.MaxValue, false, 0);
			}
			else if (pxlFrame != null)
			{
				this.Md.RotaPF(0f, num4, num2, num2, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			}
			if (num2 == 0f)
			{
				return;
			}
			this.Md.chooseSubMesh(0, false, true);
			this.Ma.SetWhole(true);
			if (flag2)
			{
				Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(0f, num4 * 0.015625f, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, X.ANMP(IN.totalframe, 150, 1f) * 360f, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(20f, 0f, -37f));
				MTR.BkEnhancer.col_point = this.Md.ColGrd.rgba;
				MTR.BkEnhancer.col_hen = this.Md.ColGrd.rgba;
				MTR.BkEnhancer.drawTo(this.Md, matrix4x);
				this.Md.Col = color;
				this.Md.Identity();
			}
			if (flag)
			{
				UniBrightDrawer uniBrightDrawer = MTRX.UniBright.Count(8).RotTime(180f, 340f).Col(C32.MulA(uint.MaxValue, num * 0.75f), C32.MulA(16777215U, num));
				uniBrightDrawer.rand_hash = X.GETRAN2(this.ScreenTask.f0, 4);
				uniBrightDrawer.Radius(170f, 290f).CenterCicle(120f, 140f, 160f).Thick(20f, 45f);
				uniBrightDrawer.rot_anti_ratio = 0f;
				uniBrightDrawer.drawTo(this.Md, 0f, num4, (float)IN.totalframe, false);
				uniBrightDrawer.rand_hash = X.GETRAN2(233 + this.ScreenTask.f0, 8);
				uniBrightDrawer.CenterCicle(0f, -1f, 160f);
				uniBrightDrawer.rot_anti_ratio = 1f;
				uniBrightDrawer.drawTo(this.Md, 0f, num4, (float)IN.totalframe, false);
				this.Md.Col = color;
				this.Md.Identity();
			}
			this.Md.updateForMeshRenderer(true);
		}

		public NelM2DBase M2D;

		public IItemSupplier NextSplTarget;

		public TextRenderer TxTitle;

		public TextRenderer TxHave;

		private List<ItemDescBox.IdbTask> ATask;

		private ItemDescBox.IdbTask ScreenTask;

		private NelItemEntry[] AEntryReel;

		private TRMManager.TRMReward TrmRwd;

		private float reel_t;

		private const int reel_rotation = 10;

		public const float row_intv_mul_size = 1.875f;

		private MeshDrawer Md;

		private MeshRenderer MrdMain;

		private MdArranger Ma;

		private MeshDrawer MdScreen;

		private ValotileRenderer VltScreen;

		private GameObject GobScreen;

		private const int SCREEN_MAXT = 60;

		private float t_screen = -60f;

		private string open_snd;

		private float read_delay_;

		public bool lock_input_focus;

		public static Color32[] Acol_normal = new Color32[]
		{
			C32.d2c(2855482163U),
			C32.d2c(4278190080U)
		};

		public static Color32[] Acol_focus = new Color32[]
		{
			C32.MulA(4291611332U, 0.82f),
			C32.MulA(4293321691U, 0.82f)
		};

		public static Color32[] Acol_focus2 = new Color32[]
		{
			C32.d2c(4291611332U),
			C32.d2c(4293321691U)
		};

		public static Color32[] Acol_trnsp = new Color32[]
		{
			C32.d2c(0U),
			C32.d2c(0U)
		};

		private ItemDescBox.TYPE type;

		public uint frame_color;

		public uint letter_line_color;

		public int frame_start_vertex;

		public int frame_start_tri;

		public Color32 ShadowCol = MTRX.ColTrnsp;

		private Vector4 PosTarget;

		private const string half_bgm_key = "IDESCBOX";

		private List<NelItemEntry> FocusRow;

		private const float focus_width = 330f;

		private const float focus_width_wide = 580f;

		public enum TYPE
		{
			AUTO,
			POPUP,
			FOCUS
		}

		private enum MESH
		{
			FRAME,
			ICO
		}

		public class IdbTask
		{
			public IdbTask(ItemDescBox.TYPE _type, object _Target, bool _is_supertop = false)
			{
				this.type = _type;
				this.Target = _Target;
				this.f0 = IN.totalframe;
				this.supertop = _is_supertop;
			}

			public ItemDescBox.TYPE type;

			public object Target;

			public int f0;

			public bool supertop;

			public string position_key;

			public class IdbPopup
			{
				public IdbPopup(NelItemManager.POPUP _p_categ, string _text, float _mapx, float _mapy, float _shifty_toupper = 0f, float _shifty_tounder = 0f, bool _abs = false)
				{
					this.p_categ = _p_categ;
					this.text = _text;
					this.mapx = _mapx;
					this.mapy = _mapy;
					this.shifty_toupper = _shifty_toupper;
					this.shifty_tounder = _shifty_tounder;
					this.abs = _abs;
				}

				public NelItemManager.POPUP p_categ;

				public string text;

				public float mapx;

				public float mapy;

				public float shifty_toupper;

				public float shifty_tounder;

				public bool abs;
			}
		}
	}
}
