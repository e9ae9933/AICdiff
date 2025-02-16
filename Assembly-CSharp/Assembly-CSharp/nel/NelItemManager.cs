using System;
using System.Collections.Generic;
using System.IO;
using Better;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class NelItemManager
	{
		public bool need_fine_center_position
		{
			get
			{
				return this.pr_active_count < 0;
			}
			set
			{
				if (value && this.pr_active_count >= 0)
				{
					this.pr_active_count = X.Mn(-1, this.pr_active_count);
				}
			}
		}

		public bool need_fine_bomb_self
		{
			get
			{
				return (this.bombself & NelItemManager.BOMB_SELF.NEED_FINE) > (NelItemManager.BOMB_SELF)0;
			}
			set
			{
				if (value)
				{
					this.bombself |= NelItemManager.BOMB_SELF.NEED_FINE;
				}
			}
		}

		public static string itemdrop_dir
		{
			get
			{
				return Path.Combine(Application.streamingAssetsPath, "DropItems");
			}
		}

		public NelItemManager(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.ODrop = new BDic<M2DropObject, NelItemManager.NelItemDrop>();
			this.StInventory = new ItemStorage("Inventory_noel", 12);
			this.StInventory.auto_update_topright_counter = true;
			this.StInventory.water_stockable = false;
			this.StInventory.FD_RowNameAddition = new UiItemManageBox.FnRowNameAddition(NelItemManager.ItemRowNameAddition);
			this.StInventory.FD_RowIconAddition = new UiItemManageBox.FnRowIconAddition(NelItemManager.ItemRowIconAddition);
			this.StInventory.check_quest_target = true;
			this.StInventory.fnAddable = (NelItem _Itm, bool ignore_area) => !_Itm.is_precious;
			this.StHouseInventory = new ItemStorage("Inventory_house", 12);
			this.StHouseInventory.fnAddable = (NelItem _Itm, bool ignore_area) => (ignore_area || this.M2D.canAccesableToHouseInventory()) && !_Itm.is_precious;
			this.StHouseInventory.check_quest_target = true;
			this.StPrecious = new ItemStorage("Inventory_precious", 255);
			this.StPrecious.sort_button_bits &= -11;
			this.StPrecious.infinit_stockable = (this.StPrecious.check_quest_target = true);
			this.StPrecious.fnAddable = (NelItem _Itm, bool ignore_area) => _Itm.is_precious && !_Itm.is_enhancer;
			this.StEnhancer = new ItemStorage("Inventory_enhancer", 255);
			this.StEnhancer.sort_button_bits &= -11;
			this.StEnhancer.sort_button_bits |= 48;
			this.StEnhancer.infinit_stockable = true;
			this.StEnhancer.fnAddable = (NelItem _Itm, bool ignore_area) => _Itm.is_enhancer;
			this.ReelM = new ReelManager(null);
			this.USel = new UseItemSelector(this);
			this.StHouseInventory.infinit_stockable = (this.StHouseInventory.water_stockable = true);
			this.AFD_PickupListener = new List<NelItemManager.IPickupListener>(1);
			this.AInventory = new ItemStorage[] { this.StInventory, this.StHouseInventory, this.StPrecious, this.StEnhancer };
			this.AInventoryNotHouse = new ItemStorage[] { this.StInventory, this.StPrecious, this.StEnhancer };
			this.fnRunDropObject = (M2DropObject Dro, float fcnt) => this.runDropObjectInner(Dro, fcnt);
			this.FD_fnDrawDropObject = new M2DrawBinder.FnEffectBind(this.fnDrawDropObject);
		}

		public ItemStorage[] getInventoryArray()
		{
			if (this.M2D.canAccesableToHouseInventory())
			{
				return this.AInventory;
			}
			return this.AInventoryNotHouse;
		}

		public ItemStorage[] getInventoryIngredientArray(bool include_presious = false)
		{
			if (include_presious)
			{
				if (this.M2D.canAccesableToHouseInventory())
				{
					return new ItemStorage[] { this.StInventory, this.StHouseInventory, this.StPrecious };
				}
				return new ItemStorage[] { this.StInventory, this.StPrecious };
			}
			else
			{
				if (this.M2D.canAccesableToHouseInventory())
				{
					return new ItemStorage[] { this.StInventory, this.StHouseInventory };
				}
				return new ItemStorage[] { this.StInventory };
			}
		}

		public void initGameObject()
		{
			this.DescBox = new GameObject("ItemDesc").AddComponent<ItemDescBox>();
			this.DescBox.gameObject.layer = IN.gui_layer;
			this.DescBox.M2D = this.M2D;
			this.DescBox.make(" ");
			this.BoxCheckTarget = new GameObject("Box_check_target").AddComponent<M2BoxOneLine>().Init(this.M2D, false);
			this.ODiscardStack = null;
			this.use_valotile = !X.DEBUGSTABILIZE_DRAW;
			this.StmNoel = new Stomach(null, this.M2D.PlayerNoel);
		}

		public bool use_valotile
		{
			get
			{
				return this.DescBox.use_valotile;
			}
			set
			{
				this.DescBox.use_valotile = value;
			}
		}

		public void setBoxLayer()
		{
		}

		public M2DropObjectContainer DropCon
		{
			get
			{
				return this.M2D.curMap.DropCon;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.M2D.curMap;
			}
		}

		public void initS()
		{
			this.ODrop.Clear();
			this.id_cnt = 0;
			this.holdover_alert = 0f;
			this.PreTalkable = null;
			this.SupplyTarget = (this.PreSupplyTarget = null);
			if (this.Lp == null)
			{
				this.Lp = new EfParticleLooper("dropitem_aura");
				this.Lpd = new EfParticleLooper("dropitem_aura_bits");
			}
			if (this.DescBox.isActive())
			{
				this.DescBox.clearStack();
				this.DescBox.deactivate();
				this.DescBox.fineMove();
			}
			if (this.BoxCheckTarget.isActive())
			{
				this.BoxCheckTarget.deactivate();
				this.BoxCheckTarget.fineMove();
				this.BoxCheckTarget.visible = false;
			}
			this.box_show_flag = NelItemManager.POPUP.HIDE;
			this.pr_active_count = -1;
			this.AFD_PickupListener.Clear();
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.ADroInCamera == null || this.ADroInCamera.Capacity > 8)
			{
				this.ADroInCamera = new List<NelItemManager.NelItemDrop>(8);
			}
			this.ODiscardStack = null;
		}

		public string savedrop_key
		{
			get
			{
				string text;
				return this.getSaveDropKey(out text);
			}
		}

		public string getSaveDropKey(out string local_file)
		{
			local_file = null;
			string text = null;
			string[] array = this.Mp.Meta.Get("save_drop_item");
			if (array != null && array.Length != 0)
			{
				if (array[0] == "1" || array[0] == "_" || TX.noe(array[0]))
				{
					text = "_" + this.Mp.key;
				}
				else
				{
					text = array[0];
					local_file = text + ".drpd";
				}
			}
			return text;
		}

		public void initSAfter()
		{
			string text;
			string saveDropKey = this.getSaveDropKey(out text);
			if (saveDropKey != null)
			{
				ByteArray byteArray = null;
				if (!COOK.ODropItemMem.TryGetValue(saveDropKey, out byteArray))
				{
					if (text == null)
					{
						goto IL_00C7;
					}
					try
					{
						byte[] array = NKT.readSpecificFileBinary(Path.Combine(NelItemManager.itemdrop_dir, text), 0, 0, false);
						if (array != null && array.Length != 0)
						{
							byteArray = new ByteArray(array, false, true);
							byteArray.position = 0UL;
							if (byteArray.Length < (ulong)((long)"osake_no_okage_de_ironna_mono_wo_otoshi_te_simatta_HashinoMizuha".Length) || byteArray.readMultiByte("osake_no_okage_de_ironna_mono_wo_otoshi_te_simatta_HashinoMizuha".Length, "utf-8") != "osake_no_okage_de_ironna_mono_wo_otoshi_te_simatta_HashinoMizuha")
							{
								throw new Exception("wrong header");
							}
						}
						goto IL_00C7;
					}
					catch (Exception ex)
					{
						byteArray = null;
						X.de(ex.ToString(), null);
						goto IL_00C7;
					}
				}
				COOK.ODropItemMem[saveDropKey] = null;
				if (byteArray != null)
				{
					byteArray.position = 0UL;
					if (byteArray.bytesAvailable == 0UL)
					{
						byteArray = null;
					}
				}
				IL_00C7:
				try
				{
					if (byteArray != null)
					{
						this.readDropObjectBinaryFrom(byteArray);
					}
				}
				catch
				{
					X.de("itemdropファイル展開エラー", null);
				}
			}
		}

		public void endS()
		{
			if (this.Mp == null || COOK.reloading)
			{
				return;
			}
			string text;
			string saveDropKey = this.getSaveDropKey(out text);
			if (saveDropKey != null)
			{
				ByteArray byteArray = new ByteArray(0U);
				this.writeDropObjectBinaryTo(byteArray);
				COOK.ODropItemMem[saveDropKey] = byteArray;
			}
		}

		public float battleFinishProgress(int grade, bool gameover)
		{
			this.M2D.WM.assignStoreFlushFlag(true, !gameover);
			float num;
			this.StmNoel.progress((float)(grade * (gameover ? 2 : 1)), true, out num, true, false);
			if (!gameover)
			{
				this.M2D.IMNG.StmNoel.Pr.JuiceCon.progressWaterDrunkCache(num, false, false);
			}
			if (!this.M2D.isSafeArea())
			{
				this.spoilRawDishes(true);
			}
			if (!gameover)
			{
				this.M2D.getPrNoel().cureMpNotHunger(false);
			}
			this.M2D.WDR.setWalkAroundFlag(false);
			return num;
		}

		public void prepareDefaultPreciousRows(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			int count = ASource.Count;
			for (int i = 0; i < count; i++)
			{
				ItemStorage.IRow row = ASource[i];
				bool flag = true;
				if (row.Data.is_recipe && this.has_recipe_collection)
				{
					flag = false;
				}
				if (TX.isStart(row.Data.key, "spconfig_", 0) || TX.isStart(row.Data.key, "__guildquest_", 0))
				{
					flag = false;
				}
				if (flag)
				{
					ADest.Add(row);
				}
			}
		}

		public NelItemManager.NelItemDrop dropManual(NelItem Itm, int count, int grade, float mapx, float mapy, float vx = 0f, float vy = 0f, META Meta = null, bool check_auto_absorb = false, NelItemManager.TYPE type_merge = NelItemManager.TYPE.NORMAL)
		{
			NelItemManager.NelItemDrop nelItemDrop = this.AddDrop(mapx, mapy, Itm, count, grade);
			nelItemDrop.rotR = X.XORSP() * 6.2831855f;
			nelItemDrop.Dro.vx = vx;
			nelItemDrop.Dro.vy = vy;
			this.fineDropState(nelItemDrop, Meta, false);
			if (vx == 0f && vy == 0f)
			{
				nelItemDrop.lock_time = 0f;
			}
			nelItemDrop.type |= type_merge;
			return nelItemDrop;
		}

		private void fineDropState(NelItemManager.NelItemDrop ItData, META Meta = null, bool check_auto_absorb = false)
		{
			if (Meta != null)
			{
				if (Meta.GetB("drop_invisible", false))
				{
					ItData.type |= NelItemManager.TYPE.INVISIBLE;
				}
				if (Meta.GetB("force_absorb", false))
				{
					ItData.type |= NelItemManager.TYPE.ABSORB;
				}
				if (Meta.GetB("no_get_effect", false))
				{
					ItData.type |= NelItemManager.TYPE.NO_GET_EFFECT;
				}
			}
			if (check_auto_absorb && (ItData.type & (NelItemManager.TYPE)97) == NelItemManager.TYPE.NORMAL)
			{
				NelItem itm = ItData.Itm;
				if (itm.is_precious || this.getInventory().existStockableRow(itm))
				{
					ItData.type |= NelItemManager.TYPE.ABSORB_AUTO;
				}
			}
		}

		public NelItemManager.NelItemDrop dropMBoxReel(ReelManager.ItemReelDrop Reel, float mapx, float mapy, float vx = 0f, float vy = 0f)
		{
			NelItem byId = NelItem.GetById((Reel.grade_add ? "itemreelG_" : "itemreelC_") + Reel.key, false);
			NelItemManager.NelItemDrop nelItemDrop = this.dropManual(byId, 1, Reel.grade, mapx, mapy, vx, vy, null, false, NelItemManager.TYPE.NORMAL);
			nelItemDrop.type |= NelItemManager.TYPE.ABSORB;
			return nelItemDrop;
		}

		public void assignTalkableObject(M2MoverPr FromMv, int x, int y, AIM k, List<IM2TalkableObject> ATk)
		{
			float num = (float)CAim._XD(k, 1);
			CAim._YD(k, 1);
			float num2;
			float num3;
			if (num != 0f)
			{
				num2 = ((num < 0f) ? ((float)(x - 1) - 0.75f) : ((float)x - 0.25f));
				num3 = ((num < 0f) ? ((float)x + 0.25f) : ((float)(x + 1) + 0.75f));
			}
			else
			{
				num2 = (float)x - 0.25f;
				num3 = (float)(x + 1) + 0.25f;
			}
			float num4 = (float)y - 2.25f;
			float num5 = (float)y + 0.5f + FromMv.sizey + 1.5f;
			foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair in this.ODrop)
			{
				M2DropObject key = keyValuePair.Key;
				NelItemManager.NelItemDrop value = keyValuePair.Value;
				if (X.BTW(num2, key.x, num3) && X.BTW(num4, key.y - 1.5f, num5) && value.isPickupAble(FromMv))
				{
					ATk.Add(value);
				}
			}
		}

		private NelItemManager.NelItemDrop AddDrop(float x, float y, NelItem Itm, int _count, int _grade)
		{
			if (_count <= 0)
			{
				_count = this.StInventory.getItemStockable(Itm);
			}
			_grade = X.MMX(0, _grade, 4);
			M2DropObject m2DropObject = this.DropCon.AddManual(x, y, 0f, X.NIXP(0f, -0.025f), 0f, 0f);
			m2DropObject.gravity_scale = 0.33f;
			m2DropObject.size = 0.35f;
			m2DropObject.FnRun = this.fnRunDropObject;
			m2DropObject.type = DROP_TYPE.NO_OPTION;
			m2DropObject.bounce_y_reduce = 0.24f;
			m2DropObject.type |= (DROP_TYPE)288;
			NelItemManager.NelItemDrop nelItemDrop = new NelItemManager.NelItemDrop();
			nelItemDrop.Itm = Itm;
			nelItemDrop.zspdR = (float)X.MPFXP() * X.NIXP(0.2f, 0.65f) / 360f * 6.2831855f;
			nelItemDrop.rotR = X.XORSP() * 6.2831855f;
			int num = this.id_cnt;
			this.id_cnt = num + 1;
			nelItemDrop.id = num;
			nelItemDrop.count = _count;
			nelItemDrop.grade = _grade;
			nelItemDrop.Dro = m2DropObject;
			nelItemDrop.Reel = (Itm.is_reelmbox ? ReelManager.GetIR(Itm) : null);
			NelItemManager.NelItemDrop nelItemDrop2 = nelItemDrop;
			m2DropObject.MyObj = nelItemDrop2;
			if (!this.need_fine_center_position)
			{
				this.ADroInCamera.Add(nelItemDrop2);
				if (nelItemDrop2.finePrActive(null))
				{
					this.pr_active_count++;
				}
			}
			this.ODrop[m2DropObject] = nelItemDrop2;
			if (this.Ed == null)
			{
				this.Ed = this.Mp.setED("ItemMng", this.FD_fnDrawDropObject, 0f);
			}
			return nelItemDrop2;
		}

		public bool removeItData(M2DropObject Dro, NelItemManager.NelItemDrop ItData = null, bool pickuped = false)
		{
			if (ItData == null)
			{
				ItData = Dro.MyObj as NelItemManager.NelItemDrop;
			}
			if (ItData != null)
			{
				int count = this.AFD_PickupListener.Count;
				for (int i = 0; i < count; i++)
				{
					this.AFD_PickupListener[i].DropPickup(ItData, pickuped);
				}
				if (this.SupplyTarget != null && this.SupplyTarget == ItData)
				{
					this.DescBox.spliceStackIf(this.SupplyTarget);
					this.SupplyTarget = null;
				}
				Dro.destruct(true);
				this.ADroInCamera.Remove(ItData);
				ItData.Dro = null;
				Dro.MyObj = null;
				this.ODrop.Remove(Dro);
				if (this.M2D.curMap.TalkTarget == ItData)
				{
					this.M2D.curMap.setTalkTarget(null, false);
				}
			}
			Dro.destruct();
			return false;
		}

		private bool runDropObjectInner(M2DropObject Dro, float fcnt)
		{
			NelItemManager.NelItemDrop nelItemDrop = Dro.MyObj as NelItemManager.NelItemDrop;
			if (nelItemDrop == null)
			{
				return false;
			}
			nelItemDrop.rotR += nelItemDrop.zspdR;
			nelItemDrop.zspdR = X.VALWALK(nelItemDrop.zspdR, 0f, 1.7453292E-05f);
			bool flag = (nelItemDrop.type & NelItemManager.TYPE.INVISIBLE) == NelItemManager.TYPE.NORMAL;
			if ((nelItemDrop.type & (NelItemManager.TYPE)3) != NelItemManager.TYPE.NORMAL)
			{
				if (nelItemDrop.lock_time > 0f)
				{
					nelItemDrop.lock_time -= fcnt;
					if (nelItemDrop.lock_time <= 0f)
					{
						nelItemDrop.lock_time = 0f;
						if (this.canAddItem(nelItemDrop.Itm, nelItemDrop.count, true) == 0)
						{
							nelItemDrop.type &= (NelItemManager.TYPE)(-8);
							return true;
						}
					}
				}
				else
				{
					if (nelItemDrop.lock_time < 0f)
					{
						nelItemDrop.lock_time = 0f;
					}
					M2MoverPr keyPr = Dro.Mp.getKeyPr();
					if (keyPr == null)
					{
						return true;
					}
					if (X.LENGTHXYS(keyPr.x, keyPr.y, Dro.x, Dro.y) <= (float)(nelItemDrop.Itm.is_reelmbox ? 200 : ((!flag) ? 12 : 5)))
					{
						if (X.isCovering(keyPr.mleft, keyPr.mright, Dro.x, Dro.x, 0.5f) && X.isCovering(keyPr.mtop, keyPr.mbottom, Dro.y, Dro.y, 0.5f))
						{
							if ((nelItemDrop.type & NelItemManager.TYPE.EVENT_SPILL) != NelItemManager.TYPE.NORMAL && !Map2d.can_handle)
							{
								return true;
							}
							int item = this.getItem(nelItemDrop.Itm, nelItemDrop.count, nelItemDrop.grade, true, (nelItemDrop.type & NelItemManager.TYPE.ABSORB) > NelItemManager.TYPE.NORMAL, false, nelItemDrop.do_not_add_obtain_count);
							nelItemDrop.count -= item;
							if (item > 0 && (nelItemDrop.type & NelItemManager.TYPE.NO_GET_EFFECT) == NelItemManager.TYPE.NORMAL)
							{
								nelItemDrop.PtcST(this.M2D.curMap);
							}
							if (nelItemDrop.count <= 0)
							{
								return this.removeItData(Dro, null, true);
							}
							if ((nelItemDrop.type & NelItemManager.TYPE.EVENT_SPILL) != NelItemManager.TYPE.NORMAL)
							{
								nelItemDrop.type &= (NelItemManager.TYPE)(-17);
								UILog.Instance.AddAlertTX("Alert_event_item_holdover", UILogRow.TYPE.ALERT);
							}
							nelItemDrop.type &= (NelItemManager.TYPE)(-8);
							Dro.gravity_scale = 0.33f;
							Dro.size = 0.35f;
							Dro.vx = X.absMn(Dro.vx, 0.07f);
							Dro.vy = X.Mn(Dro.vy, -0.1f);
							return true;
						}
						else
						{
							Dro.gravity_scale = 0f;
							Dro.af_ground = X.Mn(Dro.af_ground, 0f);
							if (Dro.af_ground == 0f || (int)Dro.af % 8 == 0)
							{
								Dro.z = Dro.Mp.GAR(Dro.x, Dro.y, keyPr.x, keyPr.y - keyPr.sizey * 0.5f);
							}
							float num = 0.2f;
							Dro.size = -1f;
							float num2 = num * X.Cos(Dro.z);
							float num3 = -num * X.Sin(Dro.z);
							Dro.vx = X.VALWALK(Dro.vx, num2, (flag ? (0.02f * fcnt) : (0.25f * fcnt)) * ((Dro.vx * num2 < 0f) ? 2.5f : 1f));
							Dro.vy = X.VALWALK(Dro.vy, num3, (flag ? (0.04f * fcnt) : (0.25f * fcnt)) * ((Dro.vx * num2 < 0f) ? 2.5f : 1f));
							Dro.af_ground -= fcnt;
						}
					}
					else
					{
						Dro.size = 0.35f;
						Dro.af_ground = X.Mx(Dro.af_ground, 0f);
						Dro.gravity_scale = X.VALWALK(Dro.gravity_scale, 0.33f, 0.05f * fcnt);
					}
				}
			}
			else
			{
				if (!flag)
				{
					this.removeItData(Dro, null, false);
					return false;
				}
				if (X.Abs(Dro.vx) + X.Abs(Dro.vy) < 0.1f && Dro.af_ground >= 4f)
				{
					if ((nelItemDrop.type & NelItemManager.TYPE.DISCARDED_NOELJUICE) != NelItemManager.TYPE.NORMAL && Dro.af >= 90f)
					{
						int count = nelItemDrop.count;
						bool flag2 = true;
						if (this.isNoelJuiceExplodeable() && this.M2D.NightCon.addAdditionalDangerLevel(ref nelItemDrop.count, nelItemDrop.grade, true))
						{
							if (count > nelItemDrop.count)
							{
								this.Mp.PtcSTsetVar("x", (double)Dro.x).PtcSTsetVar("y", (double)Dro.y).PtcST("noeljuice_explode", null, PTCThread.StFollow.NO_FOLLOW);
							}
							WanderingNPC puppet = this.M2D.WDR.getPuppet();
							if (!puppet.isHere(this.M2D.curMap) && puppet.alreadyCalcedAt(this.M2D.curMap) && !SCN.isPuppetWNpcDefeated() && puppet.canAppearToThisMap(this.M2D.curMap))
							{
								puppet.setCurrentPos(this.M2D.curMap, true);
								puppet.appear_ratio += 0.04f * (float)(1 + nelItemDrop.grade) * (float)(count - nelItemDrop.count);
							}
							flag2 = false;
						}
						if (flag2)
						{
							if (EV.isActive(false))
							{
								Dro.af = 0f;
								return true;
							}
							M2EventContainer eventContainer = this.M2D.curMap.getEventContainer();
							if (eventContainer != null)
							{
								eventContainer.stackSpecialCommand("JUICE_EXPL", this.M2D.getPrNoel(), true);
							}
						}
						if (nelItemDrop.count > 0)
						{
							this.M2D.Mana.AddMulti(Dro.x, Dro.y, (float)nelItemDrop.count * X.NIL(12f, 48f, (float)nelItemDrop.grade, 4f), MANA_HIT.EN | MANA_HIT.FROM_GAGE_SPLIT, 1f);
							this.Mp.PtcSTsetVar("x", (double)Dro.x).PtcSTsetVar("y", (double)Dro.y).PtcST("noeljuice_evaporate", null, PTCThread.StFollow.NO_FOLLOW);
						}
						this.removeItData(Dro, null, false);
						return false;
					}
					if ((nelItemDrop.type & (NelItemManager.TYPE)3) == NelItemManager.TYPE.NORMAL && nelItemDrop.lock_time >= 0f)
					{
						nelItemDrop.lock_time = -1f;
						this.fineDropState(nelItemDrop, null, true);
						M2MoverPr keyPr2 = Dro.Mp.getKeyPr();
						if (keyPr2 != null && nelItemDrop.isPickupAble(keyPr2))
						{
							keyPr2.need_check_event = true;
						}
					}
				}
			}
			return true;
		}

		private bool fnDrawDropObject(EffectItem Ef, M2DrawBinder Ed)
		{
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			MeshDrawer meshDrawer3 = null;
			MeshDrawer meshDrawer4 = null;
			PxlFrame pxlFrame = null;
			int num = 14 * this.pr_active_count;
			float num2 = (float)(this.ADroInCamera.Count * 9 + 30);
			float num3 = 0.7f + 0.15f * X.COSIT(34f) + 0.15f * X.COSIT(19.4f);
			for (int i = this.ADroInCamera.Count - 1; i >= 0; i--)
			{
				NelItemManager.NelItemDrop nelItemDrop = this.ADroInCamera[i];
				M2DropObject dro = nelItemDrop.Dro;
				if (dro.MyObj as NelItemManager.NelItemDrop != nelItemDrop)
				{
					this.removeItData(dro, nelItemDrop, false);
				}
				else if ((nelItemDrop.type & NelItemManager.TYPE.INVISIBLE) == NelItemManager.TYPE.NORMAL)
				{
					if (meshDrawer == null)
					{
						meshDrawer = Ef.GetMeshImg("", MTRX.MIicon, BLEND.MUL, true);
						meshDrawer2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, true);
						meshDrawer4 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
						pxlFrame = MTRX.getPF("border_circle40");
					}
					Ef.x = dro.x;
					Ef.y = dro.y;
					bool flag;
					Vector2 vector = Ef.EF.calcMeshXY(dro.x, dro.y, meshDrawer2, out flag);
					meshDrawer.base_x = (meshDrawer2.base_x = (meshDrawer4.base_x = vector.x));
					meshDrawer.base_y = (meshDrawer2.base_y = (meshDrawer4.base_y = vector.y));
					float num4 = (dro.af + (float)(dro.index * 9)) % (50f + num2);
					if (nelItemDrop.ChipPImg != null)
					{
						if (meshDrawer3 == null)
						{
							meshDrawer3 = Ef.GetMeshImg("", this.M2D.MIchip, BLEND.NORMAL, true);
						}
						meshDrawer3.base_x = vector.x;
						meshDrawer3.base_y = vector.y;
						meshDrawer3.Col = MTRX.ColWhite;
						meshDrawer3.Identity();
						meshDrawer3.RotaMesh(0f, 0f, 1f, 1f, 0f, nelItemDrop.ChipPImg, dro.index % 2 == 1, false);
					}
					else if (nelItemDrop.Reel != null)
					{
						meshDrawer2.Rotate(nelItemDrop.rotR, false);
						nelItemDrop.Reel.drawSmallIcon(meshDrawer2, 0f, 0f, num3, 2f, false);
						meshDrawer2.Identity();
					}
					else
					{
						meshDrawer2.Col = meshDrawer2.ColGrd.White().blend(nelItemDrop.Itm.getColor(null), num3).C;
						int icon = nelItemDrop.Itm.getIcon(null, null);
						if (X.BTW(0f, (float)icon, (float)MTR.AItemIcon.Length))
						{
							meshDrawer2.RotaPF(0f, 0f, 1f, 1f, nelItemDrop.rotR, MTR.AItemIcon[icon], nelItemDrop.flip, false, false, uint.MaxValue, false, 0);
						}
					}
					float num5 = 1f - X.ZLINE(X.Abs(num4 - 25f), 25f);
					num5 = (nelItemDrop.da = X.VALWALK(nelItemDrop.da, num5, 0.04f));
					if (num5 > 0f)
					{
						num5 *= num3;
						meshDrawer2.Col = meshDrawer2.ColGrd.White().mulA(0f).C;
						meshDrawer2.ColGrd.White();
						meshDrawer.Col = meshDrawer.ColGrd.Set(4290361785U).mulA(0.7f * num5).C;
						meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
						meshDrawer.Rect(0f, 0f, 30f, 30f, false);
						float num6 = X.ZSIN2(dro.af - 2f, 30f);
						meshDrawer2.initForImg(MTRX.EffCircle128, 0);
						meshDrawer2.RectC(0f, 0f, 40f, 40f, false, X.NI(1f, 0.75f, num6) * num5, X.NI(1f, 0.42f, num6) * num5);
					}
					nelItemDrop.ptca = X.VALWALK(nelItemDrop.ptca, (float)(nelItemDrop.pr_active ? 1 : 0), 0.05f);
					if (nelItemDrop.ptca > 0f && num > 0)
					{
						num4 = (dro.af + (float)(dro.index * 9)) % (float)(50 + this.pr_active_count * 20);
						float num7 = 1f - X.ZLINE(X.Abs(num4 - 25f), 25f);
						if (num7 > 0f)
						{
							meshDrawer4.Col = meshDrawer4.ColGrd.White().mulA(nelItemDrop.ptca * num3 * num7).C;
							for (int j = (nelItemDrop.pr_active ? 0 : 1); j < 2; j++)
							{
								uint ran = X.GETRAN2(nelItemDrop.id + 44 + j * 64, nelItemDrop.id % 33);
								float num8 = X.RAN(ran, 2247) + dro.af / X.NI(230, 420, X.RAN(ran, 659));
								float num9 = X.RAN(ran, 3236) + dro.af / X.NI(230, 420, X.RAN(ran, 927));
								meshDrawer4.Scale(X.Cos0(num8), 1f, false);
								meshDrawer4.Rotate01(num9, false);
								meshDrawer4.RotaPF(0f, 0f, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
								meshDrawer4.Identity();
							}
						}
						Ef.index = (uint)nelItemDrop.id;
						Ef.af = dro.af + (float)(dro.index % this.pr_active_count);
						Ef.time = (nelItemDrop.pr_active ? num : ((this.ADroInCamera.Count + 1) * 14));
						Ef.z = nelItemDrop.ptca;
						this.Lp.Draw(Ef, (float)this.Lp.maxt);
						Ef.index = (uint)(nelItemDrop.id + 120);
						this.Lpd.Draw(Ef, (float)this.Lpd.maxt);
					}
				}
			}
			if (this.ODrop.Count == 0)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			return true;
		}

		public int getItem(NelItem Itm, int count = 1, int grade = 0, bool announce = true, bool can_add_bottled_row = false, bool event_spill = false, bool do_not_add_obtain_count = false)
		{
			ItemStorage itemStorage = this.getStorageFor(Itm);
			int num = count;
			while (count > 0)
			{
				int visibleRowCount = itemStorage.getVisibleRowCount();
				int num2 = itemStorage.Add(Itm, count, grade, !Itm.is_water || can_add_bottled_row, true);
				if (Itm.is_water || itemStorage.getVisibleRowCount() > visibleRowCount)
				{
					this.fine_drop_state = true;
				}
				if (num2 > 0 && Itm.is_precious && TX.isStart(Itm.key, "mapmarker_", 0))
				{
					this.M2D.Marker.addCount(Itm, itemStorage.getInfo(Itm));
				}
				count -= num2;
				if (count > 0)
				{
					if (itemStorage != this.StInventory || !this.M2D.canAccesableToHouseInventory())
					{
						break;
					}
					itemStorage = this.StHouseInventory;
				}
			}
			int num3 = num - count;
			if (num3 > 0)
			{
				if (!do_not_add_obtain_count)
				{
					this.addObtainCount(Itm, num3, grade);
					if (Itm.is_recipe)
					{
						RCP.Recipe recipeAllType = UiCraftBase.getRecipeAllType(Itm);
						if (recipeAllType != null)
						{
							recipeAllType.touchObtainCountAllIngredients();
						}
					}
				}
				if (announce)
				{
					UILog.Instance.AddGetItem(this, Itm, num3, grade);
				}
			}
			if (event_spill && count > 0 && this.M2D.curMap != null)
			{
				M2MoverPr pr = this.M2D.curMap.Pr;
				NelItemManager.NelItemDrop nelItemDrop = this.dropManual(Itm, count, grade, (pr == null) ? (this.M2D.Cam.x / this.M2D.CLEN) : pr.x, (pr == null) ? (this.M2D.Cam.y / this.M2D.CLEN) : pr.y, 0f, 0f, null, false, NelItemManager.TYPE.NORMAL);
				nelItemDrop.type |= (NelItemManager.TYPE)23;
				Itm.touchObtainCount();
				if (do_not_add_obtain_count)
				{
					nelItemDrop.do_not_add_obtain_count = true;
				}
				nelItemDrop.lock_time = 0f;
			}
			if (Itm.is_bomb)
			{
				this.need_fine_bomb_self = true;
			}
			if (Itm.key == "oc_slot")
			{
				this.M2D.getPrNoel().getSkillManager().getOverChargeSlots()
					.fineSlots(true);
			}
			if (Itm.key == "enhancer_slot")
			{
				ENHA.fineEnhancerStorage(this.StPrecious, this.StEnhancer);
			}
			return num3;
		}

		public int reduceItem(NelItem Itm, int count, int grade = -1)
		{
			ItemStorage[] inventoryArray = this.getInventoryArray();
			int num = inventoryArray.Length;
			int num2 = 0;
			int num3 = 0;
			while (num3 < num && count > 0)
			{
				ItemStorage itemStorage = inventoryArray[num3];
				ItemStorage.ObtainInfo info = itemStorage.getInfo(Itm);
				if (info != null && info.total > 0)
				{
					if (grade < 0)
					{
						for (int i = 0; i < 5; i++)
						{
							if (count <= 0)
							{
								break;
							}
							int num4 = info.getCount(i);
							if (num4 > 0)
							{
								num4 = X.Mn(count, num4);
								count -= num4;
								num2 += num4;
								itemStorage.Reduce(Itm, num4, i, true);
							}
						}
					}
					else
					{
						int num5 = itemStorage.getCount(Itm, grade);
						if (num5 > 0)
						{
							num5 = X.Mn(count, num5);
							count -= num5;
							num2 += num5;
							itemStorage.Reduce(Itm, num5, grade, true);
						}
					}
				}
				num3++;
			}
			return num2;
		}

		public void DiscardStack(NelItem Data, int count, int grade)
		{
			if (count <= 0)
			{
				return;
			}
			if (this.ODiscardStack == null)
			{
				this.ODiscardStack = new BDic<NelItem, ItemStorage.ObtainInfo>(4);
			}
			ItemStorage.ObtainInfo obtainInfo = X.Get<NelItem, ItemStorage.ObtainInfo>(this.ODiscardStack, Data);
			if (obtainInfo == null)
			{
				obtainInfo = (this.ODiscardStack[Data] = new ItemStorage.ObtainInfo());
			}
			obtainInfo.AddCount(count, grade);
		}

		public void digestDiscardStack(M2Mover Pr)
		{
			if (this.ODiscardStack == null)
			{
				return;
			}
			if (Pr == null)
			{
				Pr = this.M2D.getPrNoel();
			}
			float num = Pr.x;
			if (Pr is PR && (Pr as PR).isBenchState())
			{
				num -= 0.8f * (float)CAim._XD(Pr.aim, 1);
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.ODiscardStack)
			{
				ItemStorage.ObtainInfo value = keyValuePair.Value;
				for (int i = 0; i < 5; i++)
				{
					int num2;
					for (int j = value.getCount(i); j > 0; j -= num2)
					{
						num2 = X.Mn(j, keyValuePair.Key.stock);
						if (keyValuePair.Key.is_noel_water)
						{
							num2 = X.Mn(num2, 1);
						}
						if (keyValuePair.Key.is_bomb)
						{
							this.need_fine_bomb_self = true;
						}
						NelItemManager.NelItemDrop nelItemDrop = this.dropManual(keyValuePair.Key, num2, i, num, Pr.y, X.NIXP(-0.003f, -0.07f) * (float)CAim._XD(Pr.aim, 1), X.NIXP(-0.01f, -0.04f), null, false, NelItemManager.TYPE.NORMAL);
						nelItemDrop.discarded = true;
						nelItemDrop.do_not_add_obtain_count = true;
						if (keyValuePair.Key.is_noel_water)
						{
							nelItemDrop.type |= NelItemManager.TYPE.DISCARDED_NOELJUICE;
						}
					}
				}
			}
			this.ODiscardStack = null;
		}

		public void addObtainCount(NelItem Itm, int c, int grade)
		{
			this.M2D.QUEST.updateQuestCollectItem(Itm, c, grade, true);
			this.M2D.GUILD.addObtainItemForGQ(Itm, c, grade);
			Itm.addObtainCount(c);
		}

		public void addObtainCount(NelItem Itm, ItemStorage.ObtainInfo Info)
		{
			for (int i = 0; i < 5; i++)
			{
				int count = Info.getCount(i);
				if (count > 0)
				{
					this.addObtainCount(Itm, count, i);
				}
			}
		}

		public bool combineToInventory(ItemStorage StReelSpliced, ItemStorage Target, bool show_get_log, string added_alert = null, bool add_obtain_count = false)
		{
			Target.fineRows(false);
			Dictionary<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = StReelSpliced.getWholeInfoDictionary();
			bool flag = false;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
			{
				if (Target == this.StInventory)
				{
					for (int i = 0; i < 5; i++)
					{
						int count = keyValuePair.Value.getCount(i);
						if (count > 0 && this.getItem(keyValuePair.Key, count, i, show_get_log, true, true, false) > 0 && !flag)
						{
							flag = true;
						}
					}
				}
				else if (keyValuePair.Value.total > 0)
				{
					Target.AddInfo(keyValuePair.Key, keyValuePair.Value);
					if (add_obtain_count)
					{
						this.addObtainCount(keyValuePair.Key, keyValuePair.Value);
					}
					flag = true;
				}
			}
			if (flag && TX.valid(added_alert))
			{
				UILog.Instance.AddAlertTX(added_alert, UILogRow.TYPE.ALERT);
			}
			return flag;
		}

		public void confirmStoreCheckout(ItemStorage StBuy, ItemStorage StSell)
		{
			Dictionary<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = StBuy.getWholeInfoDictionary();
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
			{
				NelItem key = keyValuePair.Key;
				if (key.is_enhancer)
				{
					this.DescBox.read_delay = 30f;
					flag = true;
					this.DescBox.addTaskFocus(ENHA.Get(key), true);
				}
				else if (key.is_precious && key.FnGetName == NelItem.fnGetNameSkillBook)
				{
					PrSkill prSkill = SkillManager.Get(TX.slice(key.key, 10));
					if (prSkill != null)
					{
						prSkill.Obtain(false);
						this.DescBox.read_delay = 30f;
						flag = true;
						this.DescBox.addTaskFocus(prSkill, true);
					}
				}
				else if (key.key == "enhancer_slot")
				{
					flag2 = true;
				}
				else if (key.key == "oc_slot")
				{
					flag3 = true;
				}
				else if (key.is_precious && TX.isStart(key.key, "mapmarker_", 0))
				{
					this.M2D.Marker.addCount(key, keyValuePair.Value);
				}
				else if (key.is_recipe)
				{
					RCP.Recipe recipeAllType = UiCraftBase.getRecipeAllType(key);
					if (recipeAllType != null)
					{
						recipeAllType.touchObtainCountAllIngredients();
					}
				}
				else if (TX.isStart(key.key, "spconfig_", 0))
				{
					CFGSP.addSp(TX.slice(key.key, "spconfig_".Length));
				}
				if (this.M2D.QUEST.fineItemQuestTargetList())
				{
					for (int i = 0; i < 5; i++)
					{
						int count = keyValuePair.Value.getCount(i);
						this.M2D.QUEST.updateQuestCollectItem(key, count, i, true);
					}
				}
			}
			if (flag3)
			{
				this.M2D.getPrNoel().getSkillManager().getOverChargeSlots()
					.fineSlots(true);
			}
			if (flag2)
			{
				ENHA.fineEnhancerStorage(this.StPrecious, this.StEnhancer);
			}
			if (flag)
			{
				this.M2D.BlurSc.remFlag("__EVENT");
				EV.initWaitFn(this.DescBox, 0);
			}
		}

		public int canAddItem(NelItem Itm, int count = 1, bool add_row = true)
		{
			int num = 0;
			for (int i = this.AInventory.Length - 1; i >= 0; i--)
			{
				ItemStorage itemStorage = this.AInventory[i];
				if ((itemStorage != this.StHouseInventory || this.M2D.canAccesableToHouseInventory()) && itemStorage.isAddable(Itm, false))
				{
					num += itemStorage.getItemCapacity(Itm, false, false);
				}
			}
			return num;
		}

		private ItemStorage getStorageFor(NelItem Itm)
		{
			for (int i = this.AInventory.Length - 1; i >= 0; i--)
			{
				ItemStorage itemStorage = this.AInventory[i];
				if (itemStorage != this.StHouseInventory && itemStorage.isAddable(Itm, false))
				{
					return itemStorage;
				}
			}
			return this.StInventory;
		}

		public int countItem(NelItem Itm, int grade = -1, bool force_can_access_house_inventory = false, bool min_grade = false)
		{
			int num = 0;
			for (int i = this.AInventory.Length - 1; i >= 0; i--)
			{
				ItemStorage itemStorage = this.AInventory[i];
				if ((itemStorage != this.StHouseInventory || force_can_access_house_inventory || this.M2D.canAccesableToHouseInventory()) && itemStorage.isAddable(Itm, force_can_access_house_inventory))
				{
					if (min_grade && grade >= 0)
					{
						for (int j = grade; j < 5; j++)
						{
							num += itemStorage.getCount(Itm, j);
						}
					}
					else
					{
						num += itemStorage.getCount(Itm, grade);
					}
				}
			}
			return num;
		}

		public void holdItemString(STB Stb, NelItem Data, int grade = -1, bool canaccess_house = true)
		{
			if (Data.is_precious)
			{
				this.holdItemString(Stb, Data, this.StPrecious, grade);
				return;
			}
			this.holdItemString(Stb, Data, this.StInventory, grade);
			if (canaccess_house)
			{
				Stb.Add("/");
				this.holdItemString(Stb, Data, this.StHouseInventory, grade);
			}
		}

		public void holdItemString(STB Stb, NelItem Data, ItemStorage Inventory, int grade = -1)
		{
			int count = Inventory.getCount(Data, grade);
			if (count == 0)
			{
				Stb.Add("<font alpha=\"0.4\">");
			}
			Stb.Add("<img mesh=\"", (Inventory == this.StHouseInventory) ? "house_inventory" : "IconNoel0", "\"");
			Stb.Add(" width=\"22\" height=\"24\" ", (count > 0) ? "" : "alpha=\"0.8\"", "/>x");
			Stb.Add(count);
			if (count == 0)
			{
				Stb.Add("</font>");
			}
		}

		public ItemStorage getInventory()
		{
			return this.StInventory;
		}

		public ItemStorage getHouseInventory()
		{
			return this.StHouseInventory;
		}

		public ItemStorage getInventoryPrecious()
		{
			return this.StPrecious;
		}

		public ItemStorage getInventoryEnhancer()
		{
			return this.StEnhancer;
		}

		public ReelManager getReelManager()
		{
			return this.ReelM;
		}

		public bool hasEmptyBottle()
		{
			return this.getInventory().getEmptyBottleCount() > 0;
		}

		public int countEmptyBottle()
		{
			return this.getInventory().getEmptyBottleCount();
		}

		public int countBottle()
		{
			return this.getInventory().getCount(NelItem.Bottle, -1);
		}

		public int getMaxEmptyBottleStockable()
		{
			return this.getInventory().hide_bottle_max;
		}

		public ItemDescBox.IdbTask showPopUp(NelItemManager.POPUP categ, string text, float mapx, float mapy, float shifty_toupper = 0f, float shifty_tounder = 0f)
		{
			ItemDescBox.IdbTask idbTask = this.DescBox.addTaskPopUp(categ, text, mapx, mapy, shifty_toupper, shifty_tounder, false);
			this.box_show_flag = categ;
			return idbTask;
		}

		public ItemDescBox.IdbTask showPopUpAbs(NelItemManager.POPUP categ, string text, float pxx, float pxy)
		{
			ItemDescBox.IdbTask idbTask = this.DescBox.addTaskPopUp(categ, text, pxx, pxy, 0f, -40f, true);
			this.box_show_flag = categ;
			return idbTask;
		}

		public void fineTaskPosition(ItemDescBox.IdbTask Task)
		{
			this.DescBox.fineTaskPosition(Task);
		}

		public MsgBox hidePopUp(NelItemManager.POPUP categ)
		{
			this.DescBox.spliceStackIf(categ);
			if (this.box_show_flag == categ)
			{
				this.box_show_flag = NelItemManager.POPUP.HIDE;
				this.need_fine_target_window = true;
			}
			return null;
		}

		public void hideWindows()
		{
			this.DescBox.clearStack();
			this.DescBox.deactivate();
			this.hideCheckBoxAt();
			this.box_show_flag = NelItemManager.POPUP.HIDE;
		}

		public bool isHoldingItemByPR(NelItem Itm, int grade = -1)
		{
			return this.M2D.PlayerNoel.Skill.isHoldingItem(Itm, grade);
		}

		public bool canSwitchItemMove()
		{
			return !this.M2D.PlayerNoel.Skill.isHoldingItem(null, -1);
		}

		public List<NelItemEntry> clearItemReelProgressMem(bool clearing = false)
		{
			this.AItemReelSource = (clearing ? null : new List<NelItemEntry>(1));
			return this.AItemReelSource;
		}

		public void initItemReelUI(UiReelManager UiReel)
		{
			UiReel.fnItemReelProgressing = new ReelManager.FnItemReelProgressing(this.fnReelProcess);
		}

		private bool fnReelProcess(ReelManager.ItemReelContainer _Reel)
		{
			if (this.AItemReelSource != null && this.AItemReelSource.Count > 0)
			{
				NelItemEntry nelItemEntry = this.AItemReelSource[0];
				this.reduceItem(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade);
				this.AItemReelSource.RemoveAt(0);
			}
			return true;
		}

		public bool spoilRawDishes(bool announce = true)
		{
			this.StInventory.getWholeInfoDictionary();
			int num = 0;
			NelItem nelItem = null;
			using (BList<ItemStorage.IRow> blist = ListBuffer<ItemStorage.IRow>.Pop(0))
			{
				if (this.StInventory.getItemCountFn(NelItem.isRawFood, blist) > 0)
				{
					nelItem = NelItem.GetById("rotten_food", false);
					int count = blist.Count;
					uint num2 = 0U;
					for (int i = 0; i < count; i++)
					{
						ItemStorage.IRow row = blist[i];
						num += row.total;
						num2 = X.Mx(row.Info.newer, num2);
					}
					for (int j = 0; j < count; j++)
					{
						ItemStorage.IRow row2 = blist[j];
						this.StInventory.Reduce(row2.Data, row2.Info.total, -1, false);
					}
					this.StInventory.need_reindex_newer = false;
					this.StInventory.fineRows(true);
					ItemStorage.IRow row3;
					this.StInventory.Add(nelItem, num, 0, out row3, true, true);
					nelItem.addObtainCount(num);
					if (row3 != null)
					{
						row3.Info.newer = num2;
					}
				}
			}
			foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair in this.ODrop)
			{
				if (NelItem.isRawFoodItem(keyValuePair.Value.Itm))
				{
					if (nelItem == null)
					{
						nelItem = NelItem.GetById("rotten_food", false);
					}
					keyValuePair.Value.Itm = nelItem;
					keyValuePair.Value.grade = 0;
					keyValuePair.Value.do_not_add_obtain_count = false;
					num += keyValuePair.Value.count;
				}
			}
			if (num > 0 && announce)
			{
				UILog.Instance.AddAlert(TX.GetA("Alert_dishes_rotten", num.ToString()), UILogRow.TYPE.ALERT);
			}
			bool flag = num > 0;
			num = 0;
			nelItem = NelItem.GetById("coffeemaker_ticket", false);
			ItemStorage.ObtainInfo info = this.StPrecious.getInfo(nelItem);
			if (info != null && info.total > 0)
			{
				this.StPrecious.Reduce(nelItem, info.total, -1, true);
				flag = true;
				if (announce)
				{
					UILog.Instance.AddAlert(TX.Get("Alert_coffeemaker_ticket_lost", ""), UILogRow.TYPE.ALERT);
				}
			}
			return flag;
		}

		public void showCheckBoxAt(float mapx, float mapy, float shift_px_y, string title, bool refining = false)
		{
			int crop_value = this.M2D.curMap.get_crop_value();
			this.BoxCheckTarget.setPos(mapx, mapy, X.Abs(shift_px_y), -40f);
			this.BoxCheckTarget.make(title, refining);
		}

		public void hideCheckBoxAt()
		{
			if (this.BoxCheckTarget.isActive())
			{
				this.BoxCheckTarget.deactivate();
			}
		}

		public void run(float fcnt)
		{
			if (this.pr_active_count < 0 && Map2d.can_handle)
			{
				M2Mover baseMover = this.M2D.Cam.getBaseMover();
				if (baseMover != null)
				{
					this.pr_active_count = 1;
					if (this.ADroInCamera.Capacity < this.ODrop.Count)
					{
						this.ADroInCamera.Capacity = this.ODrop.Count;
					}
					this.ADroInCamera.Clear();
					foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair in this.ODrop)
					{
						if (keyValuePair.Value.isinCamera(5f))
						{
							this.ADroInCamera.Add(keyValuePair.Value);
							if (keyValuePair.Value.finePrActive(baseMover))
							{
								this.pr_active_count++;
							}
						}
						else
						{
							keyValuePair.Value.pr_active = false;
						}
					}
				}
			}
			if (this.box_show_flag <= NelItemManager.POPUP.NORMAL)
			{
				if (this.need_fine_target_window)
				{
					this.need_fine_target_window = false;
					this.PreSupplyTarget = this.SupplyTarget;
					this.SupplyTarget = null;
					IM2TalkableObject talkTarget = this.Mp.TalkTarget;
					if (talkTarget != null)
					{
						bool flag = false;
						if (talkTarget != this.PreTalkable)
						{
							this.M2D.Snd.play("tool_eraser_init");
							flag = true;
						}
						if (talkTarget is NelItemManager.NelItemDrop)
						{
							NelItemManager.NelItemDrop nelItemDrop = talkTarget as NelItemManager.NelItemDrop;
							this.SupplyTarget = nelItemDrop;
							this.showCheckBoxAt(nelItemDrop.Dro.x, nelItemDrop.Dro.y, 160f, NelItem.tx_item_get, flag);
						}
						else if (talkTarget is M2EventItem)
						{
							M2EventItem m2EventItem = talkTarget as M2EventItem;
							if (talkTarget is M2EventItem_ItemSupply)
							{
								this.SupplyTarget = talkTarget as M2EventItem_ItemSupply;
								this.showCheckBoxAt(m2EventItem.event_cx, m2EventItem.event_cy + 1.5f, 40f, m2EventItem.check_desc_name, flag);
							}
							else
							{
								this.showCheckBoxAt(m2EventItem.event_cx, m2EventItem.event_cy - 1.5f, 40f, m2EventItem.check_desc_name, flag);
							}
						}
					}
					else
					{
						this.hideCheckBoxAt();
					}
					if (this.SupplyTarget != this.PreSupplyTarget)
					{
						if (this.PreSupplyTarget != null)
						{
							this.DescBox.spliceStackIf(this.PreSupplyTarget);
						}
						this.PreSupplyTarget = this.SupplyTarget ?? this.PreSupplyTarget;
					}
					this.PreTalkable = talkTarget;
				}
			}
			else
			{
				this.hideCheckBoxAt();
			}
			if (this.fine_drop_state)
			{
				this.fine_drop_state = false;
				foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair2 in this.ODrop)
				{
					this.fineDropState(keyValuePair2.Value, null, true);
				}
			}
			if (this.pickup_delay > 0f)
			{
				this.pickup_delay = X.VALWALK(this.pickup_delay, 0f, fcnt);
			}
			if (this.holdover_alert > 0f)
			{
				this.holdover_alert = X.VALWALK(this.holdover_alert, 0f, fcnt);
			}
			if (this.DescBox.NextSplTarget != this.SupplyTarget)
			{
				this.DescBox.addTaskSupplier(this.SupplyTarget);
			}
			this.need_fine_target_window = false;
			this.DescBox.run(fcnt, false);
		}

		private void executePickUp(NelItemManager.NelItemDrop ItData)
		{
			if (ItData != null && this.pickup_delay <= 0f)
			{
				IN.clearPushDown(false);
				this.M2D.t_lock_check_push_up = 6f;
				this.pickup_delay = 15f;
				int item = this.getItem(ItData.Itm, ItData.count, ItData.grade, true, true, false, ItData.do_not_add_obtain_count);
				ItData.count -= item;
				if (item > 0 && (ItData.type & NelItemManager.TYPE.NO_GET_EFFECT) == NelItemManager.TYPE.NORMAL)
				{
					ItData.PtcST(this.M2D.curMap);
				}
				PR.PunchDecline(5, true);
				if (ItData.count <= 0)
				{
					this.removeItData(ItData.Dro, null, true);
					return;
				}
				if (this.holdover_alert == 0f)
				{
					ItData.Itm.touchObtainCount();
					string text;
					if (this.getStorageFor(ItData.Itm) == this.StInventory && ItData.Itm.is_water)
					{
						text = TX.GetA("cannot_take_need_container_item", NelItem.Bottle.getLocalizedName(0));
					}
					else
					{
						text = TX.GetA("Alert_item_holdover", ItData.Itm.getLocalizedName(ItData.grade));
					}
					UILog.Instance.AddAlert(text, UILogRow.TYPE.ALERT);
					this.holdover_alert = 40f;
				}
			}
		}

		public ItemDescBox get_DescBox()
		{
			return this.DescBox;
		}

		public void BoxDeactivateFinalize()
		{
			if (this.DescBox != null)
			{
				this.DescBox.deactivateFinalize();
			}
		}

		public void CheckBombSelfExplode(PR Pr, MGATTR attr, float ratio = 1f)
		{
			if (attr != MGATTR.ENERGY && attr != MGATTR.FIRE && attr != MGATTR.BOMB && attr != MGATTR.EATEN && attr != MGATTR.THUNDER)
			{
				return;
			}
			NelItem nelItem = null;
			NelItem nelItem2 = null;
			NelItem nelItem3 = null;
			ItemStorage.ObtainInfo obtainInfo = null;
			ItemStorage.ObtainInfo obtainInfo2 = null;
			ItemStorage.ObtainInfo obtainInfo3 = null;
			if (this.need_fine_bomb_self)
			{
				this.bombself = (NelItemManager.BOMB_SELF)0;
				this.bombself_fire = (this.bombself_thunder = (this.bombself_magic = 0));
				nelItem = NelItem.GetById("throw_bomb", false);
				nelItem2 = NelItem.GetById("throw_lightbomb", false);
				nelItem3 = NelItem.GetById("throw_magicbomb", false);
				int num = this.StInventory.getCount(nelItem, -1);
				if (num > nelItem.stock)
				{
					this.bombself |= NelItemManager.BOMB_SELF.FIRE;
					this.bombself_fire = (byte)X.Mn(6, (num - 1) / nelItem.stock);
				}
				num = this.StInventory.getCount(nelItem2, -1);
				if (num > nelItem2.stock)
				{
					this.bombself |= NelItemManager.BOMB_SELF.THUNDER;
					this.bombself_thunder = (byte)X.Mn(6, (num - 1) / nelItem2.stock);
				}
				num = this.StInventory.getCount(nelItem3, -1);
				if (num > nelItem3.stock)
				{
					this.bombself |= NelItemManager.BOMB_SELF.MAGIC;
					this.bombself_magic = (byte)X.Mn(6, (num - 1) / nelItem3.stock);
				}
			}
			NelItemManager.BOMB_SELF bomb_SELF = (NelItemManager.BOMB_SELF)0;
			if (attr == MGATTR.FIRE || attr == MGATTR.BOMB)
			{
				bomb_SELF |= NelItemManager.BOMB_SELF.FIRE;
			}
			else if (attr == MGATTR.ENERGY)
			{
				bomb_SELF |= NelItemManager.BOMB_SELF.MAGIC;
				ratio *= 0.5f;
			}
			else if (attr == MGATTR.THUNDER)
			{
				bomb_SELF |= NelItemManager.BOMB_SELF.THUNDER;
			}
			else if (attr == MGATTR.EATEN)
			{
				bomb_SELF |= (NelItemManager.BOMB_SELF)14;
				ratio *= 0.25f;
			}
			bomb_SELF &= this.bombself;
			if (bomb_SELF == (NelItemManager.BOMB_SELF)0)
			{
				return;
			}
			ratio *= 0.2f;
			int num2 = 6;
			int num3 = (int)this.bombself_fire;
			int num4 = (int)this.bombself_thunder;
			int num5 = (int)this.bombself_magic;
			bool flag = false;
			while (num2 > 0 && bomb_SELF != (NelItemManager.BOMB_SELF)0)
			{
				bool flag2 = false;
				if ((bomb_SELF & NelItemManager.BOMB_SELF.FIRE) != (NelItemManager.BOMB_SELF)0 && num2 > 0 && num3 > 0)
				{
					flag2 = true;
					if (num3 > 0 && this.CheckBombSelfExplode(Pr, 300f, ref nelItem, ref obtainInfo, "throw_bomb", ratio, RCP.RPI_EFFECT.FIRE_DAMAGE_REDUCE))
					{
						break;
					}
					flag = true;
					num3--;
					num2--;
				}
				if ((bomb_SELF & NelItemManager.BOMB_SELF.THUNDER) != (NelItemManager.BOMB_SELF)0 && num2 > 0 && num4 > 0)
				{
					flag2 = true;
					if (num4 > 0 && this.CheckBombSelfExplode(Pr, 300f, ref nelItem2, ref obtainInfo2, "throw_lightbomb", ratio, RCP.RPI_EFFECT.ELEC_DAMAGE_REDUCE))
					{
						break;
					}
					flag = true;
					num4--;
					num2--;
				}
				if ((bomb_SELF & NelItemManager.BOMB_SELF.MAGIC) != (NelItemManager.BOMB_SELF)0 && num2 > 0 && num5 > 0)
				{
					flag2 = true;
					if (this.CheckBombSelfExplode(Pr, 600f, ref nelItem3, ref obtainInfo3, "throw_magicbomb", ratio, RCP.RPI_EFFECT.__LISTUP_MAX))
					{
						break;
					}
					flag = true;
					num5--;
					num2--;
				}
				if (!flag2)
				{
					num2--;
				}
			}
			if (flag)
			{
				Pr.LockCntOccur.Add(PR.OCCUR.BOMB_SELF, 60f);
			}
		}

		private bool CheckBombSelfExplode(PR Pr, float lock_t, ref NelItem Itm, ref ItemStorage.ObtainInfo Obt, string item_key, float ratio, RCP.RPI_EFFECT rpi_reduce = RCP.RPI_EFFECT.__LISTUP_MAX)
		{
			if (Itm == null)
			{
				Itm = NelItem.GetById(item_key, false);
			}
			if (Obt == null)
			{
				Obt = this.StInventory.getInfo(Itm);
			}
			if (Obt == null || Obt.getCount(-1) <= Itm.stock || Pr.LockCntOccur.isLocked(PR.OCCUR.BOMB_SELF))
			{
				return false;
			}
			int num = X.Mn(Obt.getCount(-1) - Itm.stock, Itm.stock);
			if (rpi_reduce != RCP.RPI_EFFECT.__LISTUP_MAX)
			{
				float re = Pr.getRE(rpi_reduce);
				if (re > 0f)
				{
					ratio *= X.NIL(1f, 0f, re * 1.4f, 1f);
				}
				else if (re < 0f)
				{
					ratio *= X.NIL(1f, 1.5f, -re, 1f);
				}
			}
			if (X.XORSP() < ratio * ((num >= Itm.stock) ? 1f : ((float)num / (float)Itm.stock)))
			{
				bool flag = false;
				if (!this.need_fine_bomb_self)
				{
					this.need_fine_bomb_self = true;
					flag = true;
					Pr.LockCntOccur.Add(PR.OCCUR.BOMB_SELF, lock_t);
					UILog.Instance.AddAlertTX("bomb_selfexplode", UILogRow.TYPE.ALERT);
				}
				for (int i = 0; i < num; i++)
				{
					int randomGrade = Obt.getRandomGrade();
					MgItemBomb.MgBombMem mgBombMem = M2PrSkill.CreateMagicForItemBomb(Pr, Itm, randomGrade).Other as MgItemBomb.MgBombMem;
					mgBombMem.Con.initSelfExplodeThrow(mgBombMem, Itm, flag);
					flag = false;
					Obt.ReduceCount(1, randomGrade);
				}
			}
			return true;
		}

		public void fineSpecialNoelRow(PR Pr)
		{
			if (Pr == null)
			{
				NelItem byId = NelItem.GetById("precious_noel_cloth", false);
				NelItem byId2 = NelItem.GetById("precious_noel_shorts", false);
				this.ObtNoelCloth = this.StPrecious.getInfo(byId);
				if (this.ObtNoelCloth == null)
				{
					this.StPrecious.Add(byId, 1, 4, true, true);
					byId.addObtainCount(1);
					this.ObtNoelCloth = this.StPrecious.getInfo(byId);
				}
				this.ObtNoelShorts = this.StPrecious.getInfo(byId2);
				if (this.ObtNoelShorts == null)
				{
					this.StPrecious.Add(byId2, 1, 4, true, true);
					byId2.addObtainCount(1);
					this.ObtNoelShorts = this.StPrecious.getInfo(byId2);
					return;
				}
			}
			else
			{
				bool flag = false;
				int num = 3;
				if (this.ObtNoelCloth != null)
				{
					if (Pr.BetoMng.is_torned)
					{
						num &= -3;
					}
					if (Pr.BetoMng.get_current_dirt() > 0)
					{
						num &= -2;
					}
					if (this.ObtNoelCloth.total == 0)
					{
						flag = true;
					}
					this.ObtNoelCloth.changeGradeForPrecious((num >= 3) ? 4 : num, 1);
				}
				num = 3;
				if (this.ObtNoelShorts != null)
				{
					if (Pr.BetoMng.wetten)
					{
						num &= -2;
					}
					if (Pr.EggCon.isActive())
					{
						num &= -3;
					}
					if (this.ObtNoelShorts.total == 0)
					{
						flag = true;
					}
					this.ObtNoelShorts.changeGradeForPrecious((num >= 3) ? 4 : num, 1);
				}
				if (flag)
				{
					this.StPrecious.fineRows(false);
				}
			}
		}

		public bool isNoelJuiceExplodeable()
		{
			if (this.M2D.curMap == null || !Map2d.can_handle || !this.M2D.NightCon.isNoelJuiceExplodable())
			{
				return false;
			}
			PR pr = this.M2D.curMap.Pr as PR;
			return !(pr == null) && (!EnemySummoner.isActiveBorder() && !pr.isBenchState()) && !PUZ.IT.barrier_active;
		}

		public int getNoelJuiceQuality(float dlevel_multiple = 1.35f)
		{
			return this.M2D.NightCon.getNoelJuiceQuality(dlevel_multiple);
		}

		public bool getNoelEggObtainable()
		{
			return !this.M2D.isSafeArea();
		}

		public void gatherWholeDropItem()
		{
			foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair in this.ODrop)
			{
				keyValuePair.Value.type |= (NelItemManager.TYPE)3;
			}
		}

		public bool increaseInenvoryCapacity(int i, int _max = 0)
		{
			if (((_max > 0) ? X.Mn(i, _max - this.StInventory.row_max) : i) <= 0)
			{
				return false;
			}
			this.StInventory.increaseCapacity(i);
			this.StPrecious.Add(NelItem.GetById("workbench_capacity", false), i, 0, true, true);
			return true;
		}

		public static void ItemRowNameAddition(STB Stb, ItemStorage.IRow Row, ItemStorage Storage)
		{
			ItemStorage.IRow row;
			if (!Storage.water_stockable && Row.Data.is_food && !Row.Data.is_water && Storage.isLinked(Row, out row))
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("food_in_lunchbox", false).TxRpl(Stb);
					Stb.Clear().Add(stb);
				}
			}
		}

		public static int ItemRowIconAddition(ItemStorage.IRow Row, ItemStorage Storage, int def_icon)
		{
			ItemStorage.IRow row;
			if (!Storage.water_stockable && Storage.isLinked(Row, out row))
			{
				return NelItemManager.item_icon_2_packed(def_icon);
			}
			return def_icon;
		}

		public static int item_icon_2_packed(int def_icon)
		{
			int num;
			if (def_icon != 6)
			{
				if (def_icon != 7)
				{
					if (def_icon != 59)
					{
						num = def_icon;
					}
					else
					{
						num = 5;
					}
				}
				else
				{
					num = 34;
				}
			}
			else
			{
				num = 5;
			}
			return num;
		}

		public string createWlinkPack(ItemStorage StOuter, ItemStorage.IRow Outer, int outer_grade, ItemStorage StInner, ItemStorage.IRow Inner, int inner_grade)
		{
			if (Outer == null || Inner == null || Outer.has_wlink || Inner.has_wlink)
			{
				return "";
			}
			if (outer_grade < 0)
			{
				outer_grade = Outer.split_or_top_grade(StOuter);
			}
			if (inner_grade < 0)
			{
				inner_grade = Inner.split_or_top_grade(StInner);
			}
			NelItem data = Outer.Data;
			NelItem data2 = Inner.Data;
			if (Outer.Info.getCount(outer_grade) <= 0 || Inner.Info.getCount(inner_grade) <= 0)
			{
				return "";
			}
			bool flag;
			bool flag2;
			if (!data.isWLinkUser(out flag) || !data2.isWLinkUser(out flag2))
			{
				return "";
			}
			string text = null;
			if (StOuter == this.StHouseInventory && StInner == this.StHouseInventory)
			{
				bool flag3 = this.StInventory.getVisibleRowCount() <= this.StInventory.row_max - 1;
				if (!flag3)
				{
					flag3 = this.StInventory.getItemCapacity(data, true, true) > 0 || this.StInventory.getItemCapacity(Inner.Data, true, true) > 0;
				}
				if (!flag3)
				{
					return TX.Get("cannot_take_need_enough_room", "");
				}
				ItemStorage.IRow row;
				if (this.StInventory.Add(data, 1, outer_grade, out row, true, true) <= 0)
				{
					return TX.Get("cannot_take_need_enough_room", "");
				}
				StOuter.Reduce(data, 1, outer_grade, true);
				this.StInventory.addWLink(row, null, false);
				if (this.StInventory.Add(data2, 1, inner_grade, true, true) <= 0)
				{
					return TX.Get("cannot_take_need_enough_room", "");
				}
				StInner.Reduce(data2, 1, inner_grade, true);
			}
			else if (StOuter == this.StHouseInventory)
			{
				this.StInventory.addWLink(null, Inner, false);
				ItemStorage.IRow row2;
				if (this.StInventory.Add(data, 1, outer_grade, out row2, true, true) <= 0)
				{
					text = TX.Get("cannot_take_need_enough_room", "");
				}
				else
				{
					StOuter.Reduce(data, 1, outer_grade, true);
				}
			}
			else if (StInner == this.StHouseInventory)
			{
				this.StInventory.addWLink(Outer, null, false);
				ItemStorage.IRow row3;
				if (this.StInventory.Add(data2, 1, inner_grade, out row3, true, true) <= 0)
				{
					text = TX.Get("cannot_take_need_enough_room", "");
				}
				else
				{
					StInner.Reduce(data2, 1, inner_grade, true);
				}
			}
			else
			{
				this.StInventory.addWLink(Outer, Inner, false);
			}
			this.StInventory.fineRows(false);
			return text;
		}

		public string createWlinkPack(NelItem OuterTarget, ItemStorage StInner, ItemStorage.IRow Inner, int inner_grade)
		{
			string text = null;
			for (int i = 0; i < 2; i++)
			{
				ItemStorage itemStorage = ((i == 0) ? this.StInventory : this.StHouseInventory);
				if (i != 1 || this.M2D.canAccesableToHouseInventory())
				{
					ItemStorage.ObtainInfo info = itemStorage.getInfo(OuterTarget);
					ItemStorage.IRow row = ((info != null) ? info.UnlinkRow : null);
					if (row != null)
					{
						string text2 = this.createWlinkPack(itemStorage, row, -1, StInner, Inner, inner_grade);
						if (text2 == null)
						{
							return null;
						}
						if (text == null)
						{
							text = text2;
						}
						else
						{
							text = TX.add(text, text2, "\n");
						}
					}
				}
			}
			if (TX.noe(text))
			{
				return "empty";
			}
			return text;
		}

		public string removeWLink(ItemStorage St, ItemStorage.IRow Row)
		{
			if (Row == null || !Row.has_wlink)
			{
				return "";
			}
			if (St != this.StInventory)
			{
				St.removeWLink(Row);
				St.fineRows(false);
				return null;
			}
			ItemStorage.IRow row;
			ItemStorage.IRow row2;
			if (!St.isLinked(Row, out row, out row2))
			{
				return "";
			}
			NelItem data = row.Data;
			int num = row.split_or_top_grade(St);
			if (this.StInventory.getItemCapacity(row.Data, true, false) <= 0)
			{
				if (!this.M2D.canAccesableToHouseInventory())
				{
					return TX.Get("cannot_take_need_enough_room", "");
				}
				this.StInventory.removeWLink(row);
				this.StInventory.Reduce(data, 1, num, false);
				this.StHouseInventory.Add(row.Data, 1, 0, true, true);
			}
			else
			{
				this.StInventory.removeWLink(row);
			}
			this.StInventory.fineRows(false);
			return null;
		}

		public Stomach getStomach(PR Pr)
		{
			return this.StmNoel;
		}

		public void addPickupListener(NelItemManager.IPickupListener FD)
		{
			this.AFD_PickupListener.Add(FD);
		}

		public void remPickupListener(NelItemManager.IPickupListener FD)
		{
			this.AFD_PickupListener.Remove(FD);
		}

		public void newGame()
		{
			this.EventCacheBa = null;
			this.AItemReelSource = null;
			this.DropObjectCache = null;
			this.StInventory.clearAllItems(12);
			this.StInventory.hide_bottle_max = 0;
			this.StPrecious.clearAllItems(255);
			this.StHouseInventory.clearAllItems(255);
			this.StEnhancer.clearAllItems(255);
			COOK.newGameItems(this.StInventory, this.StHouseInventory, this.StPrecious);
			this.has_recipe_collection = false;
			ENHA.newGame();
			this.ReelM.newGame();
			this.fineSpecialNoelRow(null);
			this.StmNoel.newGame();
			this.USel.newGame();
			this.bombself = NelItemManager.BOMB_SELF.NEED_FINE;
			if (this.StInventory.hide_bottle_max > 0)
			{
				this.StInventory.Add(NelItem.Bottle, this.StInventory.hide_bottle_max, 0, true, true);
			}
			this.initS();
		}

		public void readBinaryFrom(ByteReader Ba, bool use_shift = true, bool fine_pr_state = true, bool fix_ver024_recipeitem = false)
		{
			this.newGame();
			if (use_shift)
			{
				Ba = Ba.readExtractBytesShifted(4);
			}
			int num = (int)Ba.readUByte();
			this.StInventory.readBinaryFrom(Ba, num >= 1, true, true, num, fix_ver024_recipeitem);
			if (num >= 2)
			{
				this.StPrecious.readBinaryFrom(Ba, true, true, true, num, fix_ver024_recipeitem);
				this.fineSpecialNoelRow(null);
				if (num >= 3)
				{
					this.StEnhancer.readBinaryFrom(Ba, true, true, true, num, fix_ver024_recipeitem);
					this.StHouseInventory.readBinaryFrom(Ba, true, true, true, num, fix_ver024_recipeitem);
					this.StmNoel.readBinaryFrom(Ba, true, fine_pr_state);
					this.ReelM.readBinaryFrom(Ba, num);
				}
			}
			else
			{
				this.StInventory.hide_bottle_max = 0;
			}
			if (X.DEBUGALLSKILL)
			{
				ENHA.prepareDebug(this.StPrecious, this.StEnhancer, false);
			}
			ENHA.fineEnhancerStorage(this.StPrecious, this.StEnhancer);
			this.DropObjectCache = new NelItemManager.DropObjectBytes((byte)num).readBinaryFrom(Ba);
			if (num >= 5)
			{
				this.USel.readBinaryFrom(Ba);
			}
			if (num < 6)
			{
				int num2 = this.StInventory.row_max - 12;
				NelItem byId = NelItem.GetById("workbench_capacity", false);
				if (byId != null)
				{
					num2 -= this.StPrecious.getCount(byId, -1);
					if (num2 > 0)
					{
						this.StPrecious.Add(byId, num2, 0, true, true);
					}
				}
			}
			ItemStorage.ObtainInfo info = this.StPrecious.getInfo(NelItem.GetById("recipe_collection", false));
			this.has_recipe_collection = info != null && info.total > 0;
		}

		public ByteArray writeBinaryTo(ByteArray Ba_first)
		{
			ByteArray byteArray = new ByteArray(0U);
			byteArray.writeByte(9);
			this.StInventory.writeBinaryTo(byteArray);
			this.StPrecious.writeBinaryTo(byteArray);
			this.StEnhancer.writeBinaryTo(byteArray);
			this.StHouseInventory.writeBinaryTo(byteArray);
			this.StmNoel.writeBinaryTo(byteArray);
			this.ReelM.writeBinaryTo(byteArray);
			NelItemManager.DropObjectBytes.writeBinaryToS(byteArray, this.ODrop);
			this.USel.writeBinaryTo(byteArray);
			if (Ba_first != null)
			{
				Ba_first.writeExtractBytesShifted(byteArray, 131, 4, -1);
			}
			return byteArray;
		}

		public void refineSpConfigItemOnMainInventory()
		{
			try
			{
				List<NelItem> list = null;
				for (int i = 0; i < 2; i++)
				{
					ItemStorage itemStorage = ((i == 0) ? this.StInventory : this.StHouseInventory);
					foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in itemStorage.getWholeInfoDictionary())
					{
						if (keyValuePair.Key.is_precious && TX.isStart(keyValuePair.Key.key, "spconfig_", 0))
						{
							if (list == null)
							{
								list = new List<NelItem>(8);
							}
							list.Add(keyValuePair.Key);
							CFGSP.addSp(TX.slice(keyValuePair.Key.key, "spconfig_".Length));
						}
					}
					if (list != null && list.Count > 0)
					{
						for (int j = list.Count - 1; j >= 0; j--)
						{
							NelItem nelItem = list[j];
							itemStorage.Reduce(nelItem, 99, -1, false);
							this.StPrecious.Add(nelItem, 1, 0, true, true);
						}
						itemStorage.fineRows(false);
						list.Clear();
					}
				}
				if (list != null)
				{
					this.StPrecious.fineRows(false);
				}
			}
			catch
			{
			}
		}

		public void assignObtainCountPreVersion()
		{
			this.StInventory.assignObtainCountPreVersion();
			this.StHouseInventory.assignObtainCountPreVersion();
			this.StPrecious.assignObtainCountPreVersion();
		}

		public NelItemManager readDropObjectBinaryFrom(ByteArray Ba)
		{
			int num = Ba.readByte();
			this.DropObjectCache = new NelItemManager.DropObjectBytes((byte)num).readBinaryFrom(Ba);
			return this;
		}

		public ByteArray writeDropObjectBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(9);
			NelItemManager.DropObjectBytes.writeBinaryToS(Ba, this.ODrop);
			return Ba;
		}

		public void digestDropObjectCache()
		{
			if (this.DropObjectCache != null)
			{
				this.DropObjectCache.dropWhole(this);
				this.DropObjectCache = null;
			}
		}

		public EfParticleLooper Lp;

		public EfParticleLooper Lpd;

		public readonly NelM2DBase M2D;

		public const float gravity_scale = 0.33f;

		private readonly BDic<M2DropObject, NelItemManager.NelItemDrop> ODrop;

		public int id_cnt;

		private readonly ItemStorage StInventory;

		private readonly ItemStorage StHouseInventory;

		private readonly ItemStorage StPrecious;

		private readonly ItemStorage StEnhancer;

		private readonly ItemStorage[] AInventory;

		private readonly ItemStorage[] AInventoryNotHouse;

		private readonly ReelManager ReelM;

		public bool has_recipe_collection;

		public Stomach StmNoel;

		public UseItemSelector USel;

		public bool fine_drop_state;

		public const int inventory_max = 12;

		public const int inventory_expanded_max_olditem = 24;

		public const float drop_size = 0.35f;

		public const string alert_holdover = "Alert_item_holdover";

		public int pr_active_count;

		private float holdover_alert;

		private float pickup_delay;

		private ItemStorage.ObtainInfo ObtNoelCloth;

		private ItemStorage.ObtainInfo ObtNoelShorts;

		private NelItemManager.DropObjectBytes DropObjectCache;

		public ByteArray EventCacheBa;

		public byte bombself_fire;

		public byte bombself_thunder;

		public byte bombself_magic;

		public const int BOMB_EXPLODE_MAX = 6;

		private NelItemManager.BOMB_SELF bombself;

		private M2DrawBinder Ed;

		private List<NelItemManager.NelItemDrop> ADroInCamera;

		private BDic<NelItem, ItemStorage.ObtainInfo> ODiscardStack;

		public const string itemdrop_saved_header = "osake_no_okage_de_ironna_mono_wo_otoshi_te_simatta_HashinoMizuha";

		public const string itemdrop_saved_ext = ".drpd";

		private List<NelItemManager.IPickupListener> AFD_PickupListener;

		private List<NelItemEntry> AItemReelSource;

		private readonly M2DropObject.FnDropObjectRun fnRunDropObject;

		private readonly M2DrawBinder.FnEffectBind FD_fnDrawDropObject;

		private NelItemManager.POPUP box_show_flag;

		public IItemSupplier SupplyTarget;

		public IItemSupplier PreSupplyTarget;

		public IM2TalkableObject PreTalkable;

		private M2BoxOneLine BoxCheckTarget;

		private ItemDescBox DescBox;

		public bool need_fine_target_window;

		public const string tx_key_packed_food = "food_in_lunchbox";

		public const int ICO_FOOD = 7;

		public const int ICO_PACKED_FOOD = 34;

		public const int ICO_WATER = 6;

		public const int ICO_BOTTLED_WATER = 5;

		public const int ICO_COCKTAIL = 59;

		public const int FIXVER024_IMNG_VER = 9;

		public const int IMNG_SAVE_VER = 9;

		public interface IPickupListener
		{
			void DropPickup(NelItemManager.NelItemDrop ItData, bool pickuped);
		}

		public class NelItemDrop : IItemSupplier, IM2TalkableObject
		{
			public NelItemEntry[] getDataList(ref bool is_reel)
			{
				NelItemEntry[] array = new NelItemEntry[]
				{
					new NelItemEntry(this.Itm, this.count, (byte)this.grade)
				};
				is_reel = false;
				return array;
			}

			public Vector4 getShowPos()
			{
				return new Vector4(this.Dro.x, this.Dro.y, 222f, -120f);
			}

			public bool discarded
			{
				get
				{
					return (this.type & NelItemManager.TYPE.DISCARDED) > NelItemManager.TYPE.NORMAL;
				}
				set
				{
					if (value)
					{
						this.type &= (NelItemManager.TYPE)(-4);
						this.type |= NelItemManager.TYPE.DISCARDED;
						return;
					}
					this.type &= (NelItemManager.TYPE)(-33);
				}
			}

			public bool do_not_add_obtain_count
			{
				get
				{
					return (this.type & NelItemManager.TYPE.DO_NOT_PROGRESS_QUEST) > NelItemManager.TYPE.NORMAL;
				}
				set
				{
					if (value)
					{
						this.type |= NelItemManager.TYPE.DO_NOT_PROGRESS_QUEST;
						return;
					}
					this.type &= (NelItemManager.TYPE)(-129);
				}
			}

			public bool destructed
			{
				get
				{
					return this.Dro == null;
				}
			}

			public bool isPickupAble(M2MoverPr Pr)
			{
				return this.canTalkable(false) == 1 && Pr.isCovering(this.Dro.x - 1f - 0.7f, this.Dro.x + 1f + 0.7f, this.Dro.y - 3f - 0.7f, this.Dro.y + 1.5f + 0.7f, 0f);
			}

			public bool SubmitTalkable(M2MoverPr SubmitFrom)
			{
				if (SubmitFrom.isCheckO(0) && this.isPickupAble(SubmitFrom))
				{
					(M2DBase.Instance as NelM2DBase).IMNG.executePickUp(this);
					return true;
				}
				return false;
			}

			public int canTalkable(bool when_battle_busy)
			{
				if (this.destructed)
				{
					return -1;
				}
				if (this.Dro.af < 200f && ((this.type & (NelItemManager.TYPE)3) != NelItemManager.TYPE.NORMAL || this.Dro.af_ground < 4f))
				{
					return 0;
				}
				return 1;
			}

			public void FocusTalkable()
			{
				(M2DBase.Instance as NelM2DBase).IMNG.need_fine_target_window = true;
			}

			public void BlurTalkable(bool event_init)
			{
				(M2DBase.Instance as NelM2DBase).IMNG.need_fine_target_window = true;
			}

			public Vector4 getMapPosAndSizeTalkableObject()
			{
				if (!this.destructed)
				{
					return new Vector4(this.Dro.x, this.Dro.y - 1.5f, 0.35f, 3.35f);
				}
				return new Vector4(0f, 0f, 0f, 0f);
			}

			public void PtcST(Map2d Mp)
			{
				if (this.Dro == null)
				{
					return;
				}
				Mp.PtcSTsetVar("x", (double)this.Dro.x).PtcSTsetVar("y", (double)this.Dro.y);
				if (this.Reel != null)
				{
					Mp.PtcSTsetVar("color", 2130706432U | (this.Reel.ColSet.top & 16777215U)).PtcSTsetVar("color_sub", 1325400064U | (this.Reel.ColSet.icon & 16777215U)).PtcST("get_drop_itemreel_mbox", null, PTCThread.StFollow.NO_FOLLOW);
					return;
				}
				Mp.PtcST("get_drop_item", null, PTCThread.StFollow.NO_FOLLOW);
			}

			public bool finePrActive(M2Mover Mv = null)
			{
				if (Mv == null)
				{
					Mv = this.Dro.Mp.M2D.Cam.getBaseMover();
					if (Mv == null)
					{
						return false;
					}
				}
				return this.pr_active = Mv.isCovering(this.Dro.x, this.Dro.x, this.Dro.y, this.Dro.y, 6f);
			}

			public bool isinCamera(float margin)
			{
				return this.Dro.Mp.M2D.Cam.isCoveringMp(this.Dro.x, this.Dro.y, this.Dro.x, this.Dro.y, margin * this.Dro.Mp.CLEN);
			}

			public NelItem Itm;

			public M2DropObject Dro;

			public PxlMeshDrawer ChipPImg;

			public ReelManager.ItemReelContainer Reel;

			public float rotR;

			public float zspdR;

			public bool flip;

			public int id;

			public int count = 1;

			public int grade;

			public float lock_time = 23f;

			public float da = 1f;

			public float ptca = 1f;

			public bool pr_active;

			public NelItemManager.TYPE type;
		}

		private enum BOMB_SELF : byte
		{
			NEED_FINE = 1,
			FIRE,
			THUNDER = 4,
			MAGIC = 8
		}

		public enum TYPE
		{
			NORMAL,
			ABSORB,
			ABSORB_AUTO,
			INVISIBLE = 4,
			NO_GET_EFFECT = 8,
			EVENT_SPILL = 16,
			DISCARDED = 32,
			DISCARDED_NOELJUICE = 64,
			DO_NOT_PROGRESS_QUEST = 128
		}

		public enum POPUP
		{
			HIDE,
			NORMAL,
			BENCH,
			FRAMED_BOARD,
			FRAMED_PAPER,
			_FOCUS_MODE = 65536,
			_TX_CENTER = 4096,
			_ADDITIONAL = 983040,
			_ADDITIONAL_POPUP = 61440
		}

		private class DropObjectBytes
		{
			public DropObjectBytes(byte _vers)
			{
				this.vers = _vers;
				this.ACache = new List<NelItemManager.DropObjectBytes.NelItemDropCache>(4);
			}

			public NelItemManager.DropObjectBytes readBinaryFrom(ByteReader Ba)
			{
				int num = (int)Ba.readUShort();
				bool flag = this.vers >= 8;
				for (int i = 0; i < num; i++)
				{
					string text = Ba.readString("utf-8", false);
					float num2 = Ba.readFloat();
					float num3 = Ba.readFloat();
					bool flag2 = Ba.readBoolean();
					int num4 = Ba.readByte();
					int num5 = 0;
					NelItemManager.TYPE type = NelItemManager.TYPE.NORMAL;
					if (this.vers >= 1)
					{
						num5 = Ba.readByte();
					}
					if (flag)
					{
						type = (NelItemManager.TYPE)Ba.readUInt();
					}
					NelItem byId = NelItem.GetById(text, false);
					if (byId != null)
					{
						this.ACache.Add(new NelItemManager.DropObjectBytes.NelItemDropCache(byId, num2, num3, num4, num5, flag2, type));
						if (byId.RecipeInfo != null && byId.RecipeInfo.DishInfo != null)
						{
							byId.RecipeInfo.DishInfo.referred++;
						}
					}
				}
				return this;
			}

			public static void writeBinaryToS(ByteArray Ba, BDic<M2DropObject, NelItemManager.NelItemDrop> ODrop)
			{
				Ba.writeUShort((ushort)ODrop.Count);
				foreach (KeyValuePair<M2DropObject, NelItemManager.NelItemDrop> keyValuePair in ODrop)
				{
					NelItemManager.NelItemDrop value = keyValuePair.Value;
					Ba.writeString(value.Itm.key, "utf-8");
					Ba.writeFloat(keyValuePair.Key.x);
					Ba.writeFloat(keyValuePair.Key.y);
					Ba.writeBool(value.flip);
					Ba.writeByte(value.count);
					Ba.writeByte(value.grade);
					Ba.writeUInt((uint)value.type);
				}
			}

			public void dropWhole(NelItemManager IMNG)
			{
				IMNG.need_fine_center_position = true;
				bool flag = this.vers >= 8;
				for (int i = this.ACache.Count - 1; i >= 0; i--)
				{
					NelItemManager.DropObjectBytes.NelItemDropCache nelItemDropCache = this.ACache[i];
					NelItemManager.NelItemDrop nelItemDrop = IMNG.AddDrop(nelItemDropCache.x, nelItemDropCache.y, nelItemDropCache.Itm, nelItemDropCache.count, nelItemDropCache.grade);
					if (flag)
					{
						nelItemDrop.type = nelItemDropCache.type;
					}
					nelItemDrop.flip = nelItemDropCache.flip;
				}
			}

			private readonly byte vers;

			private readonly List<NelItemManager.DropObjectBytes.NelItemDropCache> ACache;

			private const int VERS_READ_TYPE = 8;

			private class NelItemDropCache
			{
				public NelItemDropCache(NelItem _Itm, float _x, float _y, int _ccnt, int _grade, bool _flip, NelItemManager.TYPE _type)
				{
					this.Itm = _Itm;
					this.count = _ccnt;
					this.grade = _grade;
					this.flip = _flip;
					this.x = _x;
					this.y = _y;
					this.type = _type;
				}

				public readonly NelItem Itm;

				public readonly bool flip;

				public readonly int count = 1;

				public readonly int grade;

				public readonly float x;

				public readonly float y;

				public NelItemManager.TYPE type;
			}
		}
	}
}
