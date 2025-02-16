using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class Designer : MonoBehaviour, IDesignerBlock, IPauseable, IVariableObject, IAlphaSetable, IValotileSetable, IRunAndDestroy
	{
		public aBtn Area { get; protected set; }

		protected virtual void Awake()
		{
			if (this.OPauseable == null)
			{
				this.OPauseable = new BDic<string, IPauseable>();
				this.OnamedObject = new BDic<string, IVariableObject>();
				this.Ogob = new BDic<string, GameObject>();
				this.ORadioMde = new BDic<BtnContainerBasic, DsnData>();
				this.OBlockMem = new BDic<IDesignerBlock, DesignerRowMem.DsnMem>();
			}
		}

		public virtual void OnEnable()
		{
			if (this.auto_assign_runner && this.initted)
			{
				this.runner_assigned = true;
			}
		}

		public void OnDisable()
		{
			this.runner_assigned = false;
		}

		public Designer Small()
		{
			this.margin_in_lr = 4f;
			this.margin_in_tb = 2f;
			this.item_margin_x_px = 4f;
			this.item_margin_y_px = 6f;
			this.scrolling_margin_in_lr = 0f;
			this.scrolling_margin_in_tb = 0f;
			this.scrollbox_bottom_margin = 16f;
			this.animate_scaling_x = (this.animate_scaling_y = false);
			this.animate_maxt = 0;
			this.radius = 4f;
			return this;
		}

		public Designer Smallest()
		{
			this.margin_in_lr = 0f;
			this.margin_in_tb = 0f;
			this.item_margin_x_px = 0f;
			this.item_margin_y_px = 0f;
			this.scrolling_margin_in_lr = 0f;
			this.scrolling_margin_in_tb = 0f;
			this.scrollbox_bottom_margin = 0f;
			this.animate_scaling_x = (this.animate_scaling_y = false);
			this.animate_maxt = 0;
			this.radius = 0f;
			return this;
		}

		public virtual Designer WH(float _wpx = 0f, float _hpx = 0f)
		{
			float num = this.w;
			float num2 = this.h;
			this.w = ((_wpx > 0f) ? _wpx : this.w);
			this.h = ((_hpx > 0f) ? _hpx : this.h);
			if (this.initted && (this.w != num || this.h != num2))
			{
				this.row_remake_flag = true;
			}
			if (this.initted && this.Row.has_real_block)
			{
				this.init();
			}
			else
			{
				this.row_remake_flag = true;
				this.inw = this.w - this.margin_in_lr * 2f - (this.use_scroll ? (this.scrolling_margin_in_lr * 2f) : 0f);
				this.inh = this.h - this.margin_in_tb * 2f - (this.use_scroll ? (this.scrolling_margin_in_tb * 2f) : 0f);
				this.fineRow();
			}
			return this;
		}

		public bool WHchecking(float _wpx, float _hpx)
		{
			this.rowRemakeCheck(false);
			if (_wpx == 0f)
			{
				_wpx = this.w;
			}
			if (_hpx == 0f)
			{
				_hpx = this.h;
			}
			if (_wpx == this.w && _hpx == this.h)
			{
				return false;
			}
			this.WH(_wpx, _hpx);
			return true;
		}

		public virtual Designer init()
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			if (!base.enabled && this.auto_enable)
			{
				base.enabled = true;
			}
			if (this.CurTab != null)
			{
				this.endTab(true);
			}
			this.inw = this.w - this.margin_in_lr * 2f - (this.use_scroll ? (this.scrolling_margin_in_lr * 2f) : 0f);
			this.inh = this.h - this.margin_in_tb * 2f - (this.use_scroll ? (this.scrolling_margin_in_tb * 2f) : 0f);
			if (this.use_scroll)
			{
				if (this.Scr == null)
				{
					this.Scr = IN.CreateGob(base.gameObject, "-scr").AddComponent<ScrollBox>();
					this.Scr.border_color = this.scroll_border_color;
					this.Scr.stencil_ref = this.stencil_ref;
					this.Scr.scrollbar_shift_z = -0.05f;
					this.Scr.addDragInitFn(new ScrollBox.FnScrollBoxBindings(this.fnDragInitScroll));
					this.Scr.addDragInitFn(new ScrollBox.FnScrollBoxBindings(this.fnDragQuitScroll));
				}
				else
				{
					this.Scr.gameObject.SetActive(true);
				}
				this.Scr.WHAllPx(this.inw + this.scrolling_margin_in_lr * 2f, this.inh + this.scrolling_margin_in_tb * 2f, this.inw + this.scrolling_margin_in_lr * 2f, this.inh + this.scrolling_margin_in_tb * 2f);
				this.Scr.view_shift_z = -0.015625f;
				this.Scr.BelongDesigner = this;
				this.Scr.area_selectable = this.scroll_area_selectable;
				this.initScrollBoxColor(this.Scr);
			}
			else if (!this.use_scroll && this.Scr != null)
			{
				if (this.View != null)
				{
					this.View.transform.SetParent(base.transform, false);
				}
				this.Scr.gameObject.SetActive(false);
			}
			if (!this.initted)
			{
				this.View = IN.CreateGob(base.gameObject, "-designer_view_area");
				this.BCon = new BtnContainer<aBtn>(this.View, 0);
				this.Row = new DesignerRowMem(this.View, 0f, null);
				this.Row.fineBaseZ(this.tab_level);
				this.Row.margin_x_px = this.item_margin_x_px_;
				this.Row.margin_y_px = this.item_margin_y_px_;
			}
			else if (this.use_scroll)
			{
				this.Scr.border_color = this.scroll_border_color;
				this.Scr.stencil_ref = this.stencil_ref;
			}
			if ((this.use_valotile_ || this.prepare_valotile_) && this.AValset == null)
			{
				this.AValset = new List<IValotileSetable>(4);
			}
			if (this.use_canvas)
			{
				if (this.ViewCvs == null)
				{
					this.ViewCvs = this.View.AddComponent<Canvas>();
					CURS.Omazinai();
				}
			}
			else if (this.ViewCvs != null)
			{
				IN.DestroyOne(this.ViewCvs);
				this.ViewCvs = null;
			}
			if (this.auto_assign_runner && base.enabled && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
			{
				this.runner_assigned = true;
			}
			this.fineBackgroundMesh(false);
			if (this.use_scroll)
			{
				this.Scr.AssignToInner(this.View.transform);
			}
			this.fineRow();
			this.BCon.stencil_ref = this.stencil_ref;
			this.BCon.BelongScroll = this.CurrentAttachScroll;
			this.Row.Align(this.alignx_);
			if (this.Area != null)
			{
				this.Area.WH(this.inw, this.inh);
			}
			if (this.animate_t > 0f)
			{
				this.kadomaruRedraw(this.animate_t, true);
			}
			this.fineScrollInner(false);
			this.fine_scroll_inner |= 1;
			return this;
		}

		public float inner_row_width
		{
			get
			{
				return this.inner_row_width_;
			}
			set
			{
				this.inner_row_width_ = value;
				this.fineRow();
			}
		}

		protected virtual void initScrollBoxColor(ScrollBox Scr)
		{
			if (Scr != null)
			{
				Scr.border_color = this.scroll_border_color;
			}
		}

		public void fineRow()
		{
			if (this.Row == null)
			{
				return;
			}
			this.Row.bounds_w_px = ((this.inner_row_width_ >= 0f) ? this.inner_row_width_ : (this.inw - (this.use_scroll ? 16f : 0f)));
			if (this.use_scroll && this.Scr != null)
			{
				this.Scr.WHAllPx(this.inw + this.scrolling_margin_in_lr * 2f, this.inh + this.scrolling_margin_in_tb * 2f, this.inw + this.scrolling_margin_in_lr * 2f, this.inh + this.scrolling_margin_in_tb * 2f);
				this.Row.Base(this.Scr.getTopLeftPosMargined(this.scrolling_margin_in_lr, this.scrolling_margin_in_tb));
				return;
			}
			this.Row.BasePx(-this.inw / 2f, this.inh / 2f);
		}

		public aBtn makeAreaButton(string skin = "transparent")
		{
			if (this.CurTab != null)
			{
				return this.CurTab.makeAreaButton(skin);
			}
			if (!this.initted)
			{
				this.init();
			}
			if (this.Area == null)
			{
				this.Area = this.BCon.Make("designer_area", skin);
			}
			this.Area.WH(this.inw, this.inh);
			return this.Area;
		}

		private void fineScrollInner(bool check_scroll_inner_wh = true)
		{
			if (this.fine_scroll_inner != 0)
			{
				if ((this.fine_scroll_inner & 2) != 0)
				{
					Designer[] offSpring = this.getOffSpring();
					int num = offSpring.Length;
					for (int i = 0; i < num; i++)
					{
						Designer designer = offSpring[i];
						if (!(designer == this) && designer.use_scroll)
						{
							designer.row_remake_flag = true;
							designer.fine_scroll_inner |= 1;
						}
					}
					this.fine_scroll_inner &= -3;
				}
				if (check_scroll_inner_wh && (this.fine_scroll_inner & 1) != 0 && this.Scr != null && this.use_scroll)
				{
					this.Scr.WHAllPx(-1f, -1f, this.Row.get_swidth_px(), this.Row.get_sheight_px() + X.Mx(this.scrollbox_bottom_margin, this.scrolling_margin_in_tb));
					this.Row.Base(this.Scr.getTopLeftPosMargined(this.scrolling_margin_in_lr, this.scrolling_margin_in_tb));
				}
				this.fine_scroll_inner = 0;
			}
		}

		public virtual bool run(float fcnt)
		{
			if (!this.initted)
			{
				return true;
			}
			if (this.CurTab != null)
			{
				this.endTab(true);
			}
			if (this.animate_t >= 0f)
			{
				this.fineNaviConnection();
				if (X.D && this.animate_t <= (float)(this.animate_maxt + 8))
				{
					this.kadomaruRedraw(this.animate_t, true);
				}
				this.animate_t += fcnt;
				this.runDefaultCancel();
			}
			else
			{
				this.rowRemakeCheck(false);
				this.kadomaruRedraw((this.animate_maxt <= 0) ? 0f : ((float)this.animate_maxt + this.animate_t), true);
				this.animate_t = X.VALWALK(this.animate_t, (float)(-(float)this.animate_maxt - 40), fcnt);
				if (this.animate_t <= (float)(-(float)this.animate_maxt))
				{
					this.destroyMe();
				}
			}
			return true;
		}

		public void fineNaviConnection()
		{
			this.rowRemakeCheck(false);
			if (this.use_button_connection && this.Row.fine_designer_connect)
			{
				this.Row.checkConnect(this.selectable_loop);
			}
		}

		public bool need_redraw_background
		{
			set
			{
				if (value)
				{
					this.animate_t = X.Mn((float)this.animate_maxt, this.animate_t);
				}
			}
		}

		protected virtual void LateUpdate()
		{
			if (this.initted)
			{
				this.rowRemakeCheck(false);
			}
		}

		protected virtual void runDefaultCancel()
		{
			if (this.isActive() && this.CancelTargetBtn != null && this.animate_maxt >= 4 && IN.isCancel())
			{
				IN.clearCursDown();
				if (!this.CancelTargetBtn.isSelected())
				{
					this.CancelTargetBtn.Select(true);
					SND.Ui.play("cancel", false);
					return;
				}
				this.CancelTargetBtn.ExecuteOnSubmitKey();
			}
		}

		protected virtual void destroyMe()
		{
			if (this.auto_destruct_when_deactivate)
			{
				IN.DestroyE(base.gameObject);
				return;
			}
			base.gameObject.SetActive(false);
		}

		public void fineBackgroundMesh(bool force = false)
		{
			if (!this.initted)
			{
				return;
			}
			MeshDrawer mdKadomaru = this.MdKadomaru;
			if (this.use_mesh_background)
			{
				Material materialForBackground = this.getMaterialForBackground();
				if (this.MdKadomaru == null)
				{
					this.MdKadomaru = MeshDrawer.prepareMeshRenderer(base.gameObject, materialForBackground, 0.008f, -1, null, false, false);
					this.MrdKadomaru = base.GetComponent<MeshRenderer>();
					this.MdKadomaru.use_cache = false;
				}
				if (this.use_valotile_)
				{
					this.ValotKadomaru = ValotileRenderer.Create(this.MdKadomaru, this.MrdKadomaru, true);
				}
				if (this.MdKadomaru.getMaterial() != materialForBackground)
				{
					this.MdKadomaru.setMaterial(materialForBackground, false);
					this.MrdKadomaru.enabled = true;
					this.MrdKadomaru.sharedMaterial = this.MdKadomaru.getMaterial();
					return;
				}
			}
			else if (this.MdKadomaru != null)
			{
				if (this.ValotKadomaru != null)
				{
					IN.DestroyE(this.ValotKadomaru);
					this.ValotKadomaru = null;
				}
				this.MrdKadomaru.enabled = false;
			}
		}

		protected virtual Material getMaterialForBackground()
		{
			if (this.stencil_ref >= 0)
			{
				return MTRX.getMtr(this.no_write_mask ? BLEND.NORMALST : BLEND.MASK, this.stencil_ref);
			}
			return MTRX.MtrMeshNormal;
		}

		protected virtual void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			if (this.MdKadomaru == null)
			{
				return;
			}
			float animating_alpha = this.animating_alpha;
			this.MdKadomaru.Col = this.bgcol;
			this.MdKadomaru.Col.a = (byte)((float)this.MdKadomaru.Col.a * this.alpha_);
			this.MdKadomaru.KadomaruRect(0f, 0f, this.w * (this.animate_scaling_x ? animating_alpha : 1f), this.h * (this.animate_scaling_y ? animating_alpha : 1f), this.radius, 0f, false, 0f, 0f, false);
			if (update_mesh)
			{
				this.MdKadomaru.updateForMeshRenderer(false);
			}
		}

		public Designer bind()
		{
			if (this.BCon != null)
			{
				this.BCon.bind(true, false);
			}
			foreach (KeyValuePair<string, IPauseable> keyValuePair in this.OPauseable)
			{
				keyValuePair.Value.Resume();
			}
			if (this.Scr != null)
			{
				this.Scr.bind();
			}
			if (this.curs_active)
			{
				CURS.Active.Add(this.key);
			}
			return this;
		}

		public Designer hide()
		{
			if (this.BCon != null)
			{
				this.BCon.hide(this.hiding_apply_to_skin, false);
			}
			foreach (KeyValuePair<string, IPauseable> keyValuePair in this.OPauseable)
			{
				keyValuePair.Value.Pause();
			}
			if (this.Scr != null)
			{
				this.Scr.hide();
			}
			return this;
		}

		public static bool isMemBCon(Designer Ds, DesignerRowMem.DsnMem DMem, out BtnContainer<aBtn> BCon)
		{
			if (DMem.Blk is ObjCarrierConBlockBtnContainer<aBtn>)
			{
				BCon = (DMem.Blk as ObjCarrierConBlockBtnContainer<aBtn>).BCon;
				if (BCon != null)
				{
					return true;
				}
			}
			BCon = null;
			return false;
		}

		public List<Designer.EvacuateMem> EvacuateMemory(List<Designer.EvacuateMem> AList = null, Func<Designer, DesignerRowMem.DsnMem, bool> FnAvailableBlock = null, bool copy_only_element = false)
		{
			if (this.Row == null)
			{
				return null;
			}
			if (AList == null)
			{
				AList = new List<Designer.EvacuateMem>(this.Row.block_count);
			}
			int count = AList.Count;
			this.Row.copyMems(AList, FnAvailableBlock, null);
			int num = AList.Count;
			for (int i = count; i < num; i++)
			{
				Designer.EvacuateMem evacuateMem = AList[i];
				Designer.EvacuateMem evacuateMem2 = evacuateMem;
				if (evacuateMem2.Blk != null)
				{
					Transform transform = evacuateMem2.Blk.getTransform();
					transform.SetParent(null, false);
					transform.gameObject.SetActive(false);
					string text;
					if (X.GetKeyByValue<string, GameObject>(this.Ogob, transform.gameObject, out text))
					{
						this.Ogob.Remove(text);
						if (!TX.isStart(text, "DSN_GOB_", 0))
						{
							evacuateMem.name = text;
						}
					}
					if (evacuateMem2.Blk is aBtn)
					{
						aBtn aBtn = evacuateMem2.Blk as aBtn;
						aBtn.resetToPool(false);
						this.BCon.Rem(this.BCon.getIndex(aBtn), true);
					}
				}
				else if (copy_only_element)
				{
					AList.RemoveAt(i);
					num--;
					continue;
				}
			}
			return AList;
		}

		public Designer ReassignEvacuatedMemory(List<Designer.EvacuateMem> AList, BDic<string, IVariableObject> Onamed = null, bool no_auto_add_br = false)
		{
			if (AList != null)
			{
				this.checkInit();
				int count = AList.Count;
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					Designer.EvacuateMem evacuateMem = AList[i];
					if (i > 0 && !no_auto_add_br && num < evacuateMem.line)
					{
						this.Br();
					}
					this.ReassignEvacuatedMemory(evacuateMem);
					num = evacuateMem.line;
				}
			}
			if (Onamed != null)
			{
				foreach (KeyValuePair<string, IVariableObject> keyValuePair in Onamed)
				{
					if (!this.OnamedObject.ContainsKey(keyValuePair.Key) && keyValuePair.Value != null)
					{
						this.OnamedObject[keyValuePair.Key] = keyValuePair.Value;
					}
				}
			}
			return this;
		}

		public Designer ReassignEvacuatedMemory(Designer.EvacuateMem Src)
		{
			if (Src.Blk != null)
			{
				int num = this.item_count;
				this.addItem(Src.name, Src.Blk, Src.NameTarget ?? (Src.Blk as IVariableObject), null, -0.01f, Src.is_active);
				Transform transform = Src.Blk.getTransform();
				transform.gameObject.SetActive(Src.is_active);
				this.setItemParent(transform);
				if (Src.Blk is aBtn)
				{
					this.BCon.addBtn(Src.Blk as aBtn);
				}
				else
				{
					int num2 = this.item_count;
					this.item_count = num;
					string name = Src.name;
					this.fineGobName(ref name);
					this.addDeactivate(name, Src.DeactivateTarget ?? (Src.Blk as IPauseable));
					this.item_count = num2;
				}
			}
			return this;
		}

		public virtual Designer Clear()
		{
			if (!this.initted)
			{
				return this;
			}
			this.endTab(true);
			this.Row.Clear();
			if (this.TempMMRD != null)
			{
				this.TempMMRD.OnDestroy();
				IN.DestroyE(this.TempMMRD);
			}
			this.TempMMRD = null;
			this.AChild = null;
			this.BCon.Destroy();
			this.Area = null;
			this.CurTab = null;
			this.DefSel = null;
			this.alignx_ = ALIGN.LEFT;
			this.effect_confusion_ = false;
			this.default_focus_count = -100;
			if (this.OPauseable == null)
			{
				this.Awake();
			}
			foreach (KeyValuePair<string, GameObject> keyValuePair in this.Ogob)
			{
				IN.DestroyE(keyValuePair.Value);
			}
			this.OPauseable.Clear();
			this.OnamedObject.Clear();
			this.Ogob.Clear();
			this.ORadioMde.Clear();
			this.OBlockMem.Clear();
			if (this.AValset != null)
			{
				this.AValset.Clear();
			}
			this.item_count = 0;
			return this;
		}

		public void checkInit()
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			if (!base.enabled && this.auto_enable)
			{
				base.enabled = true;
			}
			if (!this.initted || (!this.Row.has_real_block && !this.Row.has_xsh && this.CurTab == null) || this.use_scroll != (this.Scr != null && this.Scr.gameObject.activeSelf))
			{
				this.init();
			}
			if (this.CurTab != null)
			{
				this.CurTab.checkInit();
			}
		}

		public Designer Br()
		{
			if (this.CurTab != null)
			{
				this.CurTab.Br();
				return this;
			}
			this.checkInit();
			this.Row.Br(true);
			this.fine_scroll_inner |= 1;
			return this;
		}

		public Designer XSh(float x)
		{
			if (this.CurTab != null)
			{
				this.CurTab.XSh(x);
				return this;
			}
			this.checkInit();
			this.Row.XSh(x);
			this.fine_scroll_inner |= 1;
			return this;
		}

		protected void addItem(string name, IDesignerBlock B, IVariableObject Mon, DsnData Mde, float shift_z = -0.01f, bool block_is_active = true)
		{
			this.checkInit();
			DesignerRowMem elemRow = this.ElemRow;
			elemRow.Add(B, block_is_active);
			this.OBlockMem[B] = elemRow.getBlockMemory(B);
			if (Mde != null && !Mde.visibility_on_designer)
			{
				elemRow.HideLast();
			}
			if (this.use_scroll && elemRow == this.Row)
			{
				X.Mx(this.inw, this.Row.get_swidth_px() + this.scrolling_margin_in_lr * 2f);
				X.Mx(this.inh, this.Row.get_sheight_px() + this.scrolling_margin_in_tb * 2f);
			}
			if ((Mde == null || !TX.noe(Mde.name)) && !TX.noe(name) && Mon != null)
			{
				this.OnamedObject[name] = Mon;
			}
			this.addGameObject(B.getTransform().gameObject, name, shift_z, false);
			if (B is aBtn)
			{
				(B as aBtn).BelongScroll = this.CurrentAttachScroll;
			}
			if (this.AValset != null)
			{
				if (Mon is IValotileSetable)
				{
					this.AValset.Add(Mon as IValotileSetable);
					(Mon as IValotileSetable).use_valotile = this.use_valotile_;
				}
				if (Mon != B && B is IValotileSetable)
				{
					this.AValset.Add(B as IValotileSetable);
					(B as IValotileSetable).use_valotile = this.use_valotile_;
				}
			}
			if (this.CurTab == null)
			{
				this.fine_scroll_inner |= 1;
			}
			else
			{
				this.CurTab.fine_scroll_inner |= 1;
			}
			this.Row.fine_designer_connect = (elemRow.fine_designer_connect = true);
			if (Mde != null && Mde.default_focus > this.default_focus_count && B != null)
			{
				List<aBtn> list = new List<aBtn>();
				B.AddSelectableItems(list, true);
				if (list.Count > 0)
				{
					this.DefSel = list[0];
					this.default_focus_count = Mde.default_focus;
				}
			}
			if (Mde != null && Mde.cancel_button > this.cancel_button_count && B != null)
			{
				List<aBtn> list2 = new List<aBtn>();
				B.AddSelectableItems(list2, true);
				if (list2.Count > 0)
				{
					this.CancelTargetBtn = list2[0];
					this.cancel_button_count = Mde.cancel_button;
				}
			}
			this.item_count++;
		}

		public GameObject addGameObject(GameObject Gob, string name, float shift_z = -0.01f, bool set_to_parent = true)
		{
			if (Gob != null)
			{
				this.checkInit();
				this.fineGobName(ref name);
				this.Ogob[name] = Gob;
				IN.setZ(Gob.transform, shift_z);
				if (set_to_parent)
				{
					this.setItemParent(Gob.transform);
				}
			}
			return Gob;
		}

		private void fineGobName(ref string name)
		{
			if (TX.noe(name))
			{
				name = "DSN_GOB_" + this.item_count.ToString();
			}
		}

		private string Name(DsnData D)
		{
			this.checkInit();
			if (!(D.name != ""))
			{
				return "DSN_GOB_" + this.item_count.ToString();
			}
			return D.name;
		}

		public void setItemParent(Transform Trs)
		{
			Trs.SetParent(this.use_scroll ? this.Scr.getViewArea().transform : this.Row.getTransform(), false);
		}

		public MultiMeshRenderer createTempMMRD(bool base_to_center = false)
		{
			if (this.TempMMRD != null)
			{
				return this.TempMMRD;
			}
			string text = "--temp_mmrd--";
			GameObject gameObject = this.addGameObject(new GameObject(base.name + text), text, -0.0001f, !base_to_center);
			if (base_to_center)
			{
				gameObject.transform.SetParent(base.transform, false);
			}
			gameObject.layer = base.gameObject.layer;
			MultiMeshRenderer multiMeshRenderer = gameObject.AddComponent<MultiMeshRenderer>();
			multiMeshRenderer.stencil_ref = this.stencil_ref;
			this.TempMMRD = multiMeshRenderer;
			return multiMeshRenderer;
		}

		public Designer add(DsnData Mde)
		{
			if (Mde is DsnDataButton)
			{
				this.addButton(Mde as DsnDataButton);
			}
			else if (Mde is DsnDataButtonMulti)
			{
				this.addButtonMulti(Mde as DsnDataButtonMulti);
			}
			else if (Mde is DsnDataSlider)
			{
				this.addSlider(Mde as DsnDataSlider);
			}
			else if (Mde is DsnDataInput)
			{
				this.addInput(Mde as DsnDataInput);
			}
			else if (Mde is DsnDataChecks)
			{
				this.addChecks(Mde as DsnDataChecks);
			}
			else if (Mde is DsnDataRadio)
			{
				this.addRadio(Mde as DsnDataRadio);
			}
			else if (Mde is DsnDataColorCell)
			{
				this.addColorCell(Mde as DsnDataColorCell);
			}
			else if (Mde is DsnDataImg)
			{
				this.addImg(Mde as DsnDataImg);
			}
			else if (Mde is DsnDataP)
			{
				this.addP(Mde as DsnDataP, false);
			}
			return this;
		}

		public Designer addVariableObject(string name, string def_val = "")
		{
			this.addNamedObject(name, new VariableInstance(name, def_val));
			return this;
		}

		public static void initSkinTitle(aBtn B, string title, string skin_title)
		{
			if (skin_title == "&&")
			{
				TX tx = TX.getTX(title, true, true, null);
				if (tx != null)
				{
					B.get_Skin().html_mode = true;
					B.setSkinTitle(tx.text);
					return;
				}
			}
			else
			{
				if (skin_title != null && skin_title.IndexOf("&&") >= 0)
				{
					B.get_Skin().html_mode = true;
					B.setSkinTitle(TX.ReplaceTX(skin_title, true));
					return;
				}
				if (TX.valid(skin_title))
				{
					B.setSkinTitle(skin_title);
					return;
				}
				if (title != null && title.IndexOf("&&") >= 0)
				{
					B.get_Skin().html_mode = true;
					B.setSkinTitle(TX.ReplaceTX(title, true));
				}
			}
		}

		public virtual T addButtonT<T>(DsnDataButton Mde) where T : aBtn
		{
			string text = this.Name(Mde);
			T t = this.CreateInnerGob(text).AddComponent<T>();
			t.w = Mde.w;
			t.h = Mde.h;
			Mde.setBtn(t);
			if (Mde.def)
			{
				t.SetChecked(true, true);
			}
			if (Mde.auto_checking)
			{
				t.addClickFn(Designer.fnAutoCheck);
			}
			if (Mde.fnClick != null)
			{
				t.addClickFn(Mde.fnClick);
			}
			if (Mde.fnDown != null)
			{
				t.addDownFn(Mde.fnDown);
			}
			if (Mde.fnHover != null)
			{
				t.addHoverFn(Mde.fnHover);
			}
			if (Mde.fnOut != null)
			{
				t.addOutFn(Mde.fnOut);
			}
			this.ElemBCon.addBtn(t);
			t.initializeSkin((Mde.skin == null) ? "normal" : Mde.skin, Mde.title);
			Designer.initSkinTitle(t, Mde.title ?? Mde.name, Mde.skin_title);
			t.bind();
			ButtonSkin skin = t.get_Skin();
			t.w = skin.swidth;
			t.h = skin.sheight;
			if (skin is ButtonSkinDesc && Mde.desc != null)
			{
				(skin as ButtonSkinDesc).desc = Mde.desc;
			}
			this.addItem(text, t, t, Mde, -0.01f, true);
			if (Mde.click_snd != null)
			{
				t.click_snd = Mde.click_snd;
			}
			else if (t.title.IndexOf("Cancel") >= 0)
			{
				t.click_snd = "cancel";
			}
			return t;
		}

		public aBtn addEvacuatedButton(aBtn B, string name, bool is_checked = false, bool _visibility_on_designer = true)
		{
			this.ElemBCon.addBtn(B);
			B.bind();
			B.gameObject.SetActive(true);
			this.addItem(name, B, B, null, -0.01f, true);
			B.SetChecked(is_checked, true);
			if (!_visibility_on_designer)
			{
				this.Row.HideLast();
			}
			else
			{
				B.fine_collider_flag = true;
			}
			return B;
		}

		public virtual aBtn addButton(DsnDataButton Mde)
		{
			return this.addButtonT<aBtn>(Mde);
		}

		public virtual BtnContainer<aBtn> addButtonMultiT<T>(DsnDataButtonMulti Mde) where T : aBtn
		{
			string text = this.Name(Mde);
			GameObject gameObject = this.CreateInnerGob(text);
			BtnContainerRunner btnContainerRunner = gameObject.AddComponent<BtnContainerRunner>();
			int num = ((Mde.titles != null) ? Mde.titles.Length : ((Mde.titlesL != null) ? Mde.titlesL.Count : 8));
			BtnContainer<aBtn> btnContainer = new BtnContainer<aBtn>(gameObject, num);
			this.ORadioMde[btnContainer] = Mde;
			btnContainerRunner.BCon = btnContainer;
			btnContainer.stencil_ref = this.element_stencil_ref;
			btnContainer.pausing_apply_to_skin = false;
			btnContainer.default_w = Mde.w;
			btnContainer.default_h = Mde.h;
			btnContainer.navi_loop = Mde.navi_loop;
			btnContainer.skin_default = Mde.skin;
			btnContainer.addMakingFn(new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnAddDefaultBMulti));
			btnContainer.addMakingFn(Mde.fnMaking);
			btnContainer.fnGenerateRemakeKeys = Mde.fnGenerateKeys;
			if (Mde.titles != null)
			{
				btnContainer.RemakeT<T>(Mde.titles, "");
			}
			else
			{
				btnContainer.RemakeLT<T>(Mde.titlesL, "");
			}
			btnContainer.BelongScroll = this.CurrentAttachScroll;
			btnContainer.carr_alpha_tz = false;
			int length = btnContainer.Length;
			for (int i = 0; i < length; i++)
			{
				btnContainer.Get(i).SetChecked((Mde.def & (1 << i)) > 0, true);
			}
			ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = new ObjCarrierConBlockBtnContainer<aBtn>(btnContainer, gameObject.transform);
			Designer.reboundCarrForBtnMulti(objCarrierConBlockBtnContainer, Mde.clms, Mde.margin_w, Mde.margin_h, 1f);
			this.addDeactivate(text, btnContainer);
			this.addItem(text, objCarrierConBlockBtnContainer, btnContainerRunner, Mde, -0.01f, true);
			return btnContainer;
		}

		public virtual BtnContainer<aBtn> addButtonMulti(DsnDataButtonMulti Mde)
		{
			return this.addButtonMultiT<aBtn>(Mde);
		}

		public virtual Designer addSlider(DsnDataSlider Mde)
		{
			this.addSliderT<aBtnMeter>(Mde);
			return this;
		}

		public T addSliderT<T>(DsnDataSlider Mde) where T : aBtnMeter
		{
			string text = this.Name(Mde);
			T t = this.CreateInnerGob(text).AddComponent<T>();
			t.w = Mde.w;
			t.h = Mde.h;
			t.fnBtnMeterLine = Mde.fnBtnMeterLine;
			t.fnDescConvert = Mde.fnDescConvert;
			t.submit_holding = Mde.submit_holding;
			Mde.setBtn(t);
			if (Mde.checkbox_mode > 0)
			{
				t.initAsCheckBox(Mde.Adesc_keys, (int)Mde.def, 0f, Mde.checkbox_mode == 2);
			}
			else
			{
				t.initMeter(Mde.mn, Mde.mx, Mde.valintv, Mde.def, 0f);
				t.Adesc_keys = Mde.Adesc_keys;
			}
			if (Mde.fnChanged != null)
			{
				t.addChangedFn(Mde.fnChanged);
			}
			if (Mde.fnHover != null)
			{
				t.addHoverFn(Mde.fnHover);
			}
			if (Mde.fnClick != null)
			{
				t.addClickFn(Mde.fnClick);
			}
			this.ElemBCon.addBtn(t);
			t.initializeSkin(Mde.skin, Mde.title);
			Designer.initSkinTitle(t, Mde.title, Mde.skin_title);
			t.bind();
			ButtonSkin skin = t.get_Skin();
			t.w = skin.swidth;
			t.h = skin.sheight;
			this.addItem(text, t, t, Mde, -0.01f, true);
			return t;
		}

		public Designer addColorCell(DsnDataColorCell Mde)
		{
			string text = this.Name(Mde);
			aBtnColorCell aBtnColorCell = this.CreateInnerGob(text).AddComponent<aBtnColorCell>();
			aBtnColorCell.w = Mde.w;
			aBtnColorCell.h = Mde.h;
			aBtnColorCell.open_prompt = Mde.open_prompt;
			aBtnColorCell.use_text = Mde.use_text;
			aBtnColorCell.use_alpha = Mde.use_alpha;
			Mde.setBtn(aBtnColorCell);
			if (Mde.fnPromptDone != null)
			{
				aBtnColorCell.addPromptDoneFn(Mde.fnPromptDone);
			}
			this.ElemBCon.addBtn(aBtnColorCell);
			aBtnColorCell.initializeSkin(Mde.skin, (Mde.title != "") ? Mde.title : Mde.name);
			Designer.initSkinTitle(aBtnColorCell, Mde.title, Mde.skin_title);
			aBtnColorCell.setValue(Mde.def);
			aBtnColorCell.bind();
			ButtonSkin skin = aBtnColorCell.get_Skin();
			aBtnColorCell.w = skin.swidth;
			aBtnColorCell.h = skin.sheight;
			this.addItem(text, aBtnColorCell, aBtnColorCell, Mde, -0.01f, true);
			return this;
		}

		public virtual LabeledInputField addInput(DsnDataInput Mde)
		{
			return this.addInputT<LabeledInputField>(Mde);
		}

		public T addInputT<T>(DsnDataInput Mde) where T : LabeledInputField
		{
			string text = this.Name(Mde);
			T t = this.CreateInnerGob(text).AddComponent<T>();
			if (Mde.w > 0f)
			{
				t.w = Mde.w;
			}
			t.text = Mde.def;
			t.size = ((Mde.size <= 0) ? this.default_input_size : Mde.size);
			t.h = Mde.h;
			t.max = Mde.max;
			t.min = Mde.min;
			t.multi_line = Mde.multi_line;
			t.max_len = Mde.max_len;
			t.label_top = Mde.label_top;
			t.skin = Mde.skin;
			t.integer = Mde.integer;
			t.alloc_empty = Mde.alloc_empty;
			t.hex_integer = Mde.hex_integer;
			t.return_blur = Mde.return_blur;
			t.number_mode = Mde.number;
			Mde.setBtn(t);
			if (!t.integer && !t.number_mode && Mde.max < 2147483647.0)
			{
				t.max_len = (int)Mde.max;
			}
			t.RegAllocChar = Mde.alloc_char;
			t.changed_delay_maxt = Mde.changed_delay_maxt;
			this.ElemBCon.addBtn(t);
			if (Mde.fnChanged != null)
			{
				t.addChangedFn(Mde.fnChanged);
			}
			if (Mde.fnChangedDelay != null)
			{
				t.addChangedDelayFn(Mde.fnChangedDelay);
			}
			if (Mde.fnFocus != null)
			{
				t.addFocusFn(Mde.fnFocus);
			}
			if (Mde.fnBlur != null)
			{
				t.addBlurFn(Mde.fnBlur);
			}
			if (Mde.fnReturn != null)
			{
				t.addReturnFn(Mde.fnReturn);
			}
			if (Mde.fnInputtingKeyDown != null)
			{
				t.addInputtingKeyDown(Mde.fnInputtingKeyDown);
			}
			t.setLabel(Mde.label);
			if (Mde.desc_aim_bit > 0)
			{
				t.use_desc = true;
				t.desc_aim_bit = Mde.desc_aim_bit;
			}
			if (Mde.bounds_w > 0f)
			{
				t.setBoundsWH(Mde.bounds_w, t.h);
			}
			if (!Mde.editable)
			{
				t.SetLocked(true, true, false);
			}
			t.text_pre = t.text;
			this.addItem(text, t, t, Mde, -0.01f, true);
			return t;
		}

		public FillBlock addP(DsnDataP Mde, bool extend_height = false)
		{
			bool flag = false;
			return this.addPExtendH(Mde, ref flag, extend_height);
		}

		public FillBlock addPExtendH(DsnDataP Mde, ref bool extend_h, bool extend_height = true)
		{
			string text = this.Name(Mde);
			FillBlock fillBlock = this.CreateInnerGob(text).AddComponent<FillBlock>();
			this.setPInner(Mde, fillBlock);
			fillBlock.alloc_extending = extend_height;
			fillBlock.StartFb(Mde.text, Mde.Stb, Mde.html);
			this.addDeactivate(text, fillBlock);
			if (extend_height && Mde.sheight > 0f && fillBlock.get_sheight_px() > Mde.sheight)
			{
				extend_h = true;
			}
			Mde.Stb = null;
			this.addItem(text, fillBlock, fillBlock, Mde, -0.01f, true);
			return fillBlock;
		}

		protected virtual FillBlock setPInner(DsnDataP Mde, FillBlock Fl)
		{
			Fl.widthPixel = Mde.swidth;
			Fl.heightPixel = Mde.sheight;
			Fl.alignx = Mde.alignx;
			Fl.aligny = Mde.aligny;
			Fl.margin_x = Mde.text_margin_x;
			Fl.margin_y = Mde.text_margin_y;
			Fl.image_aligny = Mde.image_aligny;
			Fl.radius = Mde.radius;
			Fl.do_not_error_unknown_tag = Mde.do_not_error_unknown_tag;
			Fl.TargetFont = Mde.TargetFont;
			Fl.effect_confusion = this.effect_confusion;
			Fl.text_auto_condense = Mde.text_auto_condense;
			Fl.text_auto_wrap = Mde.text_auto_wrap;
			Fl.Aword_splitter = Mde.Aword_splitter;
			Fl.use_valotile = ((Mde.use_valotile < 0) ? this.use_valotile : (Mde.use_valotile > 0));
			Fl.stencil_ref = this.element_stencil_ref;
			if (Mde.size == 0f)
			{
				Mde.size = (float)this.default_input_size;
			}
			if (Mde.size != 0f)
			{
				Fl.size = Mde.size;
			}
			if (Mde.lineSpacing > 0f)
			{
				Fl.lineSpacing = Mde.lineSpacing;
			}
			if (Mde.letterSpacing > 0f)
			{
				Fl.letterSpacing = Mde.letterSpacing;
			}
			Fl.TxCol = Mde.TxCol;
			Fl.TxBorderCol = Mde.TxBorderCol;
			Fl.Col = Mde.Col;
			return Fl;
		}

		public FillImageBlock addImg(DsnDataImg Mde)
		{
			string text = this.Name(Mde);
			FillImageBlock fillImageBlock = this.CreateInnerGob(text).AddComponent<FillImageBlock>();
			fillImageBlock.stencil_lessequal = Mde.stencil_lessequal;
			this.setPInner(Mde, fillImageBlock);
			fillImageBlock.UvRect = Mde.UvRect;
			fillImageBlock.MI = Mde.MI;
			if (Mde.PF != null)
			{
				fillImageBlock.PF = Mde.PF;
			}
			fillImageBlock.scale = Mde.scale;
			fillImageBlock.FnDrawFIB = Mde.FnDrawInFIB;
			fillImageBlock.FnDraw = Mde.FnDraw;
			fillImageBlock.StartFb(Mde.text, Mde.Stb, Mde.html);
			Mde.Stb = null;
			this.addDeactivate(text, fillImageBlock);
			this.addItem(text, fillImageBlock, fillImageBlock, Mde, -0.01f, true);
			return fillImageBlock;
		}

		public BtnContainer<aBtn> addChecks(DsnDataChecks Mde)
		{
			return this.addChecksT<aBtn>(Mde);
		}

		public virtual BtnContainer<aBtn> addChecksT<T>(DsnDataChecks Mde) where T : aBtn
		{
			string text = this.Name(Mde);
			GameObject gameObject = this.CreateInnerGob(text);
			BtnContainerRunner btnContainerRunner = gameObject.AddComponent<BtnContainerRunner>();
			BtnContainer<aBtn> btnContainer = new BtnContainer<aBtn>(gameObject, (Mde.keys != null) ? Mde.keys.Length : ((Mde.keysL != null) ? Mde.keysL.Count : 8));
			this.ORadioMde[btnContainer] = Mde;
			btnContainerRunner.BCon = btnContainer;
			btnContainer.BelongScroll = this.CurrentAttachScroll;
			btnContainer.stencil_ref = this.element_stencil_ref;
			btnContainer.pausing_apply_to_skin = false;
			btnContainer.default_w = Mde.w;
			btnContainer.default_h = Mde.h;
			btnContainer.navi_loop = Mde.navi_loop;
			btnContainer.skin_default = Mde.skin;
			btnContainer.fnGenerateRemakeKeys = Mde.fnGenerateKeys;
			btnContainer.addMakingFn(new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnAddDefaultChecker));
			btnContainer.addMakingFn(Mde.fnMaking);
			if (Mde.keys != null)
			{
				btnContainer.RemakeT<T>(Mde.keys, "");
			}
			else
			{
				btnContainer.RemakeLT<T>(Mde.keysL, "");
			}
			int length = btnContainer.Length;
			int clms = Mde.clms;
			X.IntC((float)length / (float)clms);
			for (int i = 0; i < length; i++)
			{
				btnContainer.Get(i).SetChecked((Mde.def & (1 << i)) > 0, true);
			}
			IN.Scl(gameObject.transform, Mde.scale, Mde.scale, 1f);
			ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = new ObjCarrierConBlockBtnContainer<aBtn>(btnContainer, gameObject.transform);
			Designer.reboundCarrForBtnMulti(objCarrierConBlockBtnContainer, Mde.clms, (float)Mde.margin_w, (float)Mde.margin_h, Mde.scale);
			this.addDeactivate(text, btnContainer);
			this.addItem(text, objCarrierConBlockBtnContainer, btnContainerRunner, Mde, -0.01f, true);
			return btnContainer;
		}

		public virtual BtnContainerNumCounter<aBtnNumCounter> addNumCounterT<T>(DsnDataNumCounter Mde) where T : aBtnNumCounter
		{
			string text = this.Name(Mde);
			GameObject gameObject = this.CreateInnerGob(text);
			BtnContainerRunnerNumCounter btnContainerRunnerNumCounter = gameObject.AddComponent<BtnContainerRunnerNumCounter>();
			BtnContainerNumCounter<aBtnNumCounter> btnContainerNumCounter = new BtnContainerNumCounter<aBtnNumCounter>(gameObject, 6, 0);
			this.ORadioMde[btnContainerNumCounter] = Mde;
			if (Mde.unselectable >= 1)
			{
				Mde.unselectable = 1;
			}
			btnContainerRunnerNumCounter.BCon = btnContainerNumCounter;
			btnContainerNumCounter.stencil_ref = this.element_stencil_ref;
			btnContainerNumCounter.pausing_apply_to_skin = false;
			btnContainerNumCounter.slide_cur_digit_only = Mde.slide_cur_digit_only;
			if (Mde.w > 0f)
			{
				btnContainerNumCounter.default_w = Mde.w;
			}
			if (Mde.h > 0f)
			{
				btnContainerNumCounter.default_h = Mde.h;
			}
			btnContainerNumCounter.navi_loop = Mde.navi_loop;
			btnContainerNumCounter.skin_default = Mde.skin;
			btnContainerNumCounter.addMakingFn(new BtnContainer<aBtnNumCounter>.FnBtnMakingBindings(this.fnAddDefaultNumCounter));
			btnContainerNumCounter.addMakingFn(Mde.fnMaking);
			ObjCarrierConBlockBtnContainer<aBtnNumCounter> objCarrierConBlockBtnContainer = btnContainerNumCounter.initNumCounter(gameObject, Mde.minval, Mde.maxval, Mde.def, Mde.digit);
			btnContainerNumCounter.RemakeDigitT<T>(Mde.skin);
			btnContainerNumCounter.BelongScroll = this.CurrentAttachScroll;
			btnContainerNumCounter.carr_alpha_tz = false;
			this.addDeactivate(text, btnContainerNumCounter);
			this.addItem(text, objCarrierConBlockBtnContainer, btnContainerRunnerNumCounter, Mde, -0.01f, true);
			return btnContainerNumCounter;
		}

		private bool fnAddDefaultNumCounter(BtnContainer<aBtnNumCounter> BCon, aBtn B)
		{
			DsnDataNumCounter dsnDataNumCounter = X.Get<BtnContainerBasic, DsnData>(this.ORadioMde, BCon) as DsnDataNumCounter;
			if (dsnDataNumCounter == null)
			{
				return true;
			}
			if (dsnDataNumCounter.fnClick != null)
			{
				B.addClickFn(dsnDataNumCounter.fnClick);
			}
			if (dsnDataNumCounter.locked)
			{
				B.SetLocked(true, true, false);
			}
			dsnDataNumCounter.setBtn(B);
			if (dsnDataNumCounter.click_snd != null)
			{
				B.click_snd = dsnDataNumCounter.click_snd;
			}
			if (this.effect_confusion)
			{
				ButtonSkinNumCounter buttonSkinNumCounter = B.get_Skin() as ButtonSkinNumCounter;
				if (buttonSkinNumCounter != null)
				{
					buttonSkinNumCounter.effect_confusion = true;
				}
			}
			this.fine_scroll_inner |= 3;
			if (this.use_scroll)
			{
				this.row_remake_flag = true;
			}
			return true;
		}

		private bool fnAddDefaultBMulti(BtnContainer<aBtn> BCon, aBtn B)
		{
			DsnDataButtonMulti dsnDataButtonMulti = X.Get<BtnContainerBasic, DsnData>(this.ORadioMde, BCon) as DsnDataButtonMulti;
			if (dsnDataButtonMulti == null)
			{
				return true;
			}
			if (dsnDataButtonMulti.fnClick != null)
			{
				B.addClickFn(dsnDataButtonMulti.fnClick);
			}
			if (dsnDataButtonMulti.fnDown != null)
			{
				B.addDownFn(dsnDataButtonMulti.fnDown);
			}
			if (dsnDataButtonMulti.fnHover != null)
			{
				B.addHoverFn(dsnDataButtonMulti.fnHover);
			}
			if (dsnDataButtonMulti.fnOut != null)
			{
				B.addOutFn(dsnDataButtonMulti.fnOut);
			}
			int index = BCon.getIndex(B);
			if ((dsnDataButtonMulti.locked & (1 << index)) != 0)
			{
				B.SetLocked(true, true, false);
			}
			dsnDataButtonMulti.setBtn(B);
			if (dsnDataButtonMulti.skin_title != null)
			{
				if (X.BTW(0f, (float)index, (float)dsnDataButtonMulti.skin_title.Length))
				{
					Designer.initSkinTitle(B, B.title, dsnDataButtonMulti.skin_title[index]);
				}
				else
				{
					Designer.initSkinTitle(B, B.title, null);
				}
			}
			else
			{
				Designer.initSkinTitle(B, B.title, null);
			}
			if (dsnDataButtonMulti.click_snd != null)
			{
				B.click_snd = dsnDataButtonMulti.click_snd;
			}
			else if (B.title.IndexOf("Cancel") >= 0)
			{
				B.click_snd = "cancel";
			}
			if (dsnDataButtonMulti.descs != null && X.BTW(0f, (float)index, (float)dsnDataButtonMulti.descs.Length))
			{
				ButtonSkinDesc buttonSkinDesc = B.get_Skin() as ButtonSkinDesc;
				if (buttonSkinDesc != null && dsnDataButtonMulti.descs[index] != null)
				{
					buttonSkinDesc.desc = dsnDataButtonMulti.descs[index];
				}
			}
			if (this.effect_confusion)
			{
				ButtonSkinRow buttonSkinRow = B.get_Skin() as ButtonSkinRow;
				if (buttonSkinRow != null)
				{
					buttonSkinRow.effect_confusion = true;
				}
			}
			this.fine_scroll_inner |= 3;
			if (this.use_scroll)
			{
				this.row_remake_flag = true;
			}
			return true;
		}

		private bool fnAddDefaultChecker(BtnContainer<aBtn> BCon, aBtn B)
		{
			B.addClickFn(Designer.fnAutoCheck);
			DsnDataChecks dsnDataChecks = X.Get<BtnContainerBasic, DsnData>(this.ORadioMde, BCon) as DsnDataChecks;
			if (dsnDataChecks == null)
			{
				return true;
			}
			Designer.initSkinTitle(B, B.title, null);
			if (dsnDataChecks.fnClick != null)
			{
				B.addClickFn(dsnDataChecks.fnClick);
			}
			int index = BCon.getIndex(B);
			dsnDataChecks.setBtn(B);
			if (dsnDataChecks.click_snd != null)
			{
				B.click_snd = dsnDataChecks.click_snd;
			}
			if (dsnDataChecks.descs != null && X.BTW(0f, (float)index, (float)dsnDataChecks.descs.Length))
			{
				ButtonSkinDesc buttonSkinDesc = B.get_Skin() as ButtonSkinDesc;
				if (buttonSkinDesc != null && dsnDataChecks.descs[index] != null)
				{
					buttonSkinDesc.desc = dsnDataChecks.descs[index];
				}
			}
			if (this.effect_confusion)
			{
				ButtonSkinRow buttonSkinRow = B.get_Skin() as ButtonSkinRow;
				if (buttonSkinRow != null)
				{
					buttonSkinRow.effect_confusion = true;
				}
			}
			this.fine_scroll_inner |= 3;
			if (this.use_scroll)
			{
				this.row_remake_flag = true;
			}
			return true;
		}

		public Designer addRadio(DsnDataRadio Mde)
		{
			this.addRadioT<aBtn>(Mde);
			return this;
		}

		public virtual BtnContainerRadio<aBtn> addRadioT<T>(DsnDataRadio Mde) where T : aBtn
		{
			string text = this.Name(Mde);
			GameObject gameObject = this.CreateInnerGob(text);
			BtnContainerRunner btnContainerRunner = gameObject.AddComponent<BtnContainerRunner>();
			ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = null;
			ScrollAppend sca = Mde.SCA;
			if (sca == null)
			{
				objCarrierConBlockBtnContainer = new ObjCarrierConBlockBtnContainer<aBtn>(null, gameObject.transform);
			}
			int num = ((Mde.keys != null) ? Mde.keys.Length : ((Mde.keysL != null) ? Mde.keysL.Count : 8));
			BtnContainerRadio<aBtn> btnContainerRadio = ((Mde.fnCreateContainer != null) ? Mde.fnCreateContainer(gameObject, Mde.def, num, objCarrierConBlockBtnContainer, sca) : new BtnContainerRadio<aBtn>(gameObject, Mde.def, num, objCarrierConBlockBtnContainer, sca));
			this.ORadioMde[btnContainerRadio] = Mde;
			btnContainerRunner.BCon = btnContainerRadio;
			btnContainerRadio.stencil_ref = ((sca == null) ? this.element_stencil_ref : sca.stencil_ref);
			if (Mde.fnChanged != null)
			{
				btnContainerRadio.addChangedFn(new BtnContainerRadio<aBtn>.FnRadioBindings(Mde.callBasic));
			}
			btnContainerRadio.pausing_apply_to_skin = false;
			btnContainerRadio.default_w = Mde.w;
			btnContainerRadio.default_h = Mde.h;
			btnContainerRadio.value_return_name = Mde.value_return_name;
			btnContainerRadio.skin_default = Mde.skin;
			btnContainerRadio.navi_loop = Mde.navi_loop;
			btnContainerRadio.fnGenerateRemakeKeys = Mde.fnGenerateKeys;
			btnContainerRadio.addMakingFn(Mde.fnMaking);
			btnContainerRadio.addMakingFn(new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnAddDefaultRadio));
			btnContainerRadio.addMakingFn(Mde.fnMakingAfter);
			btnContainerRadio.APool = Mde.APoolEvacuated;
			if (Mde.keys != null)
			{
				btnContainerRadio.RemakeT<T>(Mde.keys, "");
			}
			else if (Mde.keysL != null || btnContainerRadio.fnGenerateRemakeKeys != null)
			{
				btnContainerRadio.RemakeLT<T>(Mde.keysL, "");
			}
			IN.Scl(gameObject.transform, Mde.scale, Mde.scale, 1f);
			IPauseable pauseable = btnContainerRadio;
			IDesignerBlock designerBlock;
			if (sca != null)
			{
				ScrollAppendBtnContainer<aBtn> scrollAppendBtnContainer = new ScrollAppendBtnContainer<aBtn>(sca, Mde.name ?? X.xors(65536).ToString(), base.gameObject, btnContainerRadio);
				scrollAppendBtnContainer.item_w = Mde.w + (float)Mde.margin_w;
				scrollAppendBtnContainer.item_h = ((Mde.margin_h == -1000) ? (-1000f) : (Mde.h + (float)Mde.margin_h));
				scrollAppendBtnContainer.clms = Mde.clms;
				pauseable = scrollAppendBtnContainer;
				designerBlock = scrollAppendBtnContainer;
				ScrollBox scrollBox = scrollAppendBtnContainer.getScrollBox();
				this.initScrollBoxColor(scrollBox);
				scrollAppendBtnContainer.reposition(true);
			}
			else
			{
				btnContainerRadio.BelongScroll = this.CurrentAttachScroll;
				designerBlock = objCarrierConBlockBtnContainer;
				objCarrierConBlockBtnContainer.BCon = btnContainerRadio;
				Designer.reboundCarrForBtnMulti(objCarrierConBlockBtnContainer, Mde.clms, (float)Mde.margin_w, (float)Mde.margin_h, Mde.scale);
			}
			this.addDeactivate(text, pauseable);
			this.addItem(text, designerBlock, btnContainerRunner, Mde, -0.01f, true);
			return btnContainerRadio;
		}

		public void reboundCarrForBtnMulti(BtnContainer<aBtn> BCon, bool remaking = true)
		{
			DsnDataRadio dsnDataRadio = X.Get<BtnContainerBasic, DsnData>(this.ORadioMde, BCon) as DsnDataRadio;
			if (dsnDataRadio != null)
			{
				ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = BCon.getMainCarr() as ObjCarrierConBlockBtnContainer<aBtn>;
				if (objCarrierConBlockBtnContainer != null)
				{
					Designer.reboundCarrForBtnMulti(objCarrierConBlockBtnContainer, dsnDataRadio.clms, (float)dsnDataRadio.margin_w, (float)dsnDataRadio.margin_h, dsnDataRadio.scale);
					if (remaking)
					{
						this.RowRemakeHeightRecalc(objCarrierConBlockBtnContainer, null);
					}
				}
			}
		}

		public static void reboundCarrForBtnMulti(ObjCarrierConBlockBtnContainer<aBtn> Oc, int clms, float margin_w, float margin_h, float scale = 1f)
		{
			if (Oc == null)
			{
				return;
			}
			int num = X.Mx(Oc.BCon.Length, 1);
			float default_w = Oc.BCon.default_w;
			float default_h = Oc.BCon.default_h;
			clms = ((clms <= 0) ? num : X.Mn(clms, num));
			int num2 = X.IntC((float)num / (float)clms);
			Oc.BoundsPx(default_w * (float)clms + margin_w * (float)(clms - 1) - default_w, -(default_h * (float)num2 + margin_h * (float)(num2 - 1) - default_h), clms);
			Oc.ItemSizePx(default_w, default_h, -1);
			Oc.scale = scale;
			Oc.BCon.setCarrierContainer(Oc, false);
			Oc.BCon.runCarrier(9000f, null);
		}

		private bool fnAddDefaultRadio(BtnContainer<aBtn> BCon, aBtn B)
		{
			DsnDataRadio dsnDataRadio = X.Get<BtnContainerBasic, DsnData>(this.ORadioMde, BCon) as DsnDataRadio;
			if (dsnDataRadio == null)
			{
				return true;
			}
			Designer.initSkinTitle(B, B.title, null);
			if (!dsnDataRadio.all_function_same || !B.CopyFunctionFrom(BCon.Get(0)))
			{
				if (dsnDataRadio.fnClick != null)
				{
					B.addClickFn(dsnDataRadio.fnClick);
				}
				if (dsnDataRadio.fnHover != null)
				{
					B.addHoverFn(dsnDataRadio.fnHover);
				}
			}
			int index = BCon.getIndex(B);
			dsnDataRadio.setBtn(B);
			if (dsnDataRadio.click_snd != null)
			{
				B.click_snd = dsnDataRadio.click_snd;
			}
			if (dsnDataRadio.descs != null && X.BTW(0f, (float)index, (float)dsnDataRadio.descs.Length))
			{
				ButtonSkinDesc buttonSkinDesc = B.get_Skin() as ButtonSkinDesc;
				if (buttonSkinDesc != null && dsnDataRadio.descs[index] != null)
				{
					buttonSkinDesc.desc = dsnDataRadio.descs[index];
				}
			}
			if (this.effect_confusion)
			{
				ButtonSkinRow buttonSkinRow = B.get_Skin() as ButtonSkinRow;
				if (buttonSkinRow != null)
				{
					buttonSkinRow.effect_confusion = true;
				}
			}
			this.fine_scroll_inner |= 3;
			if (this.use_scroll)
			{
				this.row_remake_flag = true;
			}
			return true;
		}

		public DesignerHr addHr(DsnDataHr Mde)
		{
			string text = this.Name(Mde);
			DesignerHr designerHr = this.CreateInnerGob(text).AddComponent<DesignerHr>();
			if (Mde.swidth <= 0f && !Mde.vertical)
			{
				this.Br();
			}
			designerHr.width_px = ((Mde.swidth <= 0f) ? (Mde.vertical ? this.use_h : this.use_w) : Mde.swidth);
			designerHr.vertical = Mde.vertical;
			designerHr.height_px = Mde.line_height;
			designerHr.margin_t = Mde.margin_t;
			designerHr.margin_b = Mde.margin_b;
			designerHr.draw_width_rate = Mde.draw_width_rate;
			designerHr.stencil_ref = this.stencil_ref;
			if (Mde.dashed_oneline_lgt > 0f)
			{
				designerHr.dashed = X.IntC(designerHr.width_px * designerHr.draw_width_rate / Mde.dashed_oneline_lgt);
			}
			designerHr.Col = Mde.Col;
			this.addItem(Mde.name, designerHr, null, Mde, -0.01f, true);
			if (Mde.swidth <= 0f && !Mde.vertical)
			{
				this.Br();
			}
			return designerHr;
		}

		public virtual Designer addTab(string name, float _w = 0f, float _h = 0f, float min_w = 0f, float min_h = 0f, bool use_scroll = false)
		{
			return this.addTabT<Designer>(name, _w, _h, min_w, min_h, use_scroll);
		}

		public Designer addTabT<T>(string name, float _w = 0f, float _h = 0f, float min_w = 0f, float min_h = 0f, bool use_scroll = false) where T : Designer
		{
			this.checkInit();
			if (this.CurTab != null)
			{
				this.endTab(true);
			}
			this.CurTab = IN.CreateGob((this.use_scroll && this.Scr != null) ? this.Scr.getViewArea() : base.gameObject, "-tab-" + name).AddComponent<T>();
			this.CurTab.Small();
			this.curtab_name = name;
			this.CurTab.TabParentDesigner = this;
			this.CurTab.no_write_mask = this.no_write_mask;
			this.CurTab.w = ((_w > 0f) ? _w : this.w);
			this.CurTab.h = ((_h > 0f) ? _h : this.h);
			this.CurTab.is_tab = true;
			this.curtab_min_w = min_w;
			this.curtab_min_h = min_h;
			this.CurTab.stencil_ref = ((this.stencil_ref >= 0) ? (this.stencil_ref + (use_scroll ? 1 : 0)) : (use_scroll ? 10 : (-1)));
			this.CurTab.radius = 0f;
			this.CurTab.use_scroll = use_scroll;
			this.CurTab.curs_active = this.curs_active;
			this.CurTab.bgcol = MTRX.ColTrnsp;
			this.CurTab.use_valotile = this.use_valotile_;
			if (this.AChild == null)
			{
				this.AChild = new List<Designer>(1);
			}
			this.AChild.Add(this.CurTab);
			return this.CurTab;
		}

		public Designer endTab(bool extend_by_using_size = true)
		{
			if (this.CurTab == null)
			{
				return this;
			}
			if (!this.CurTab.initted)
			{
				this.CurTab.init();
			}
			this.CurTab.fineNaviConnection();
			Designer curTab = this.CurTab;
			this.CurTab = null;
			return this.assignTab(this.curtab_name, curTab, extend_by_using_size);
		}

		public Designer cropBounds(float min_w, float min_h)
		{
			float num;
			if (min_w < 0f)
			{
				num = this.w;
			}
			else
			{
				num = X.Mx(min_w, this.get_using_width_px());
			}
			float num2;
			if (min_h < 0f)
			{
				num2 = this.h;
			}
			else
			{
				num2 = X.Mx(min_h, this.get_using_height_px());
			}
			if (this.w != num || this.h != num2)
			{
				return this.WH(num, num2);
			}
			return this;
		}

		public Designer assignTab(string curtab_name, Designer Tab, bool extend_by_using_size = false)
		{
			if (extend_by_using_size && !Tab.use_scroll)
			{
				Tab.cropBounds(this.curtab_min_w, this.curtab_min_h);
			}
			this.addDeactivate("TAB__" + curtab_name, Tab);
			this.addItem("TAB__" + curtab_name, Tab, Tab, null, -0.01f, true);
			return this;
		}

		public Designer getTab(string t)
		{
			return this.Get("TAB__" + t, true) as Designer;
		}

		protected virtual bool fnDragInitScroll(ScrollBox Scr, aBtn B)
		{
			return true;
		}

		protected virtual bool fnDragQuitScroll(ScrollBox Scr, aBtn B)
		{
			return true;
		}

		public void ConnectVertical(Designer ConnectTo, bool my_top_side, bool my_bottom_side, bool important = false)
		{
			if (my_top_side)
			{
				using (BList<aBtn> blist = this.Row.PopFirstLineSelectable(false))
				{
					using (BList<aBtn> blist2 = ConnectTo.Row.PopLastLineSelectable(false))
					{
						this.Row.checkConnectRow(blist, blist2, 0, important, true);
					}
				}
			}
			if (my_bottom_side)
			{
				using (BList<aBtn> blist3 = this.Row.PopLastLineSelectable(false))
				{
					using (BList<aBtn> blist4 = ConnectTo.Row.PopFirstLineSelectable(false))
					{
						this.Row.checkConnectRow(blist4, blist3, 0, important, true);
					}
				}
			}
		}

		public void ConnectVertical(List<aBtn> ATopSide, List<aBtn> ABottomSide, bool important = false)
		{
			if (ATopSide != null)
			{
				using (BList<aBtn> blist = this.Row.PopFirstLineSelectable(false))
				{
					this.Row.checkConnectRow(blist, ATopSide, 0, important, false);
				}
			}
			if (ABottomSide != null)
			{
				using (BList<aBtn> blist2 = this.Row.PopLastLineSelectable(false))
				{
					this.Row.checkConnectRow(ABottomSide, blist2, 0, important, false);
				}
			}
		}

		public void ConnectVertical(aBtn TopSide, aBtn BottomSide, bool important = false)
		{
			if (TopSide != null)
			{
				using (BList<aBtn> blist = ListBuffer<aBtn>.Pop(0))
				{
					blist.Add(TopSide);
					using (BList<aBtn> blist2 = this.Row.PopFirstLineSelectable(false))
					{
						this.Row.checkConnectRow(blist2, blist, 0, important, false);
					}
				}
			}
			if (BottomSide != null)
			{
				using (BList<aBtn> blist3 = ListBuffer<aBtn>.Pop(0))
				{
					blist3.Add(BottomSide);
					using (BList<aBtn> blist4 = this.Row.PopLastLineSelectable(false))
					{
						this.Row.checkConnectRow(blist3, blist4, 0, important, false);
					}
				}
			}
		}

		private void addDeactivate(string name, IPauseable P)
		{
			if (name == null || P == null)
			{
				return;
			}
			if (P != null)
			{
				this.OPauseable[name] = P;
			}
		}

		private void addNamedObject(string name, IVariableObject P)
		{
			this.OnamedObject[name] = P;
		}

		private void setNamedObject(IVariableObject P, DsnData Mde)
		{
			if (!TX.noe(Mde.name))
			{
				this.OnamedObject[base.name] = P;
			}
		}

		public string getName(IVariableObject V)
		{
			foreach (KeyValuePair<string, IVariableObject> keyValuePair in this.OnamedObject)
			{
				if (keyValuePair.Value == V)
				{
					return keyValuePair.Key;
				}
			}
			return null;
		}

		public IVariableObject Get(string name, bool no_consider_btn_container = false)
		{
			IVariableObject variableObject = X.Get<string, IVariableObject>(this.OnamedObject, name);
			if (variableObject == null)
			{
				return no_consider_btn_container ? null : this.getBtn(name);
			}
			return variableObject;
		}

		public void renameVariableObject(string pre_name, string new_name)
		{
			aBtn btn = this.getBtn(pre_name);
			if (btn != null)
			{
				btn.title = new_name;
				return;
			}
			IVariableObject variableObject = X.Get<string, IVariableObject>(this.OnamedObject, pre_name);
			if (variableObject != null)
			{
				this.OnamedObject.Remove(pre_name);
				this.OnamedObject[new_name] = variableObject;
			}
		}

		public aBtn getBtn(string name)
		{
			if (this.BCon == null)
			{
				return null;
			}
			aBtn aBtn = this.BCon.Get(name);
			if (aBtn == null)
			{
				IVariableObject variableObject = this.Get(name, true);
				if (variableObject is aBtn)
				{
					return variableObject as aBtn;
				}
			}
			return aBtn;
		}

		public aBtn getBtn(int index)
		{
			if (this.BCon == null)
			{
				return null;
			}
			aBtn aBtn = this.BCon.Get(index);
			if (aBtn == null)
			{
				IVariableObject variableObject = this.Get(base.name, false);
				if (variableObject is aBtn)
				{
					return variableObject as aBtn;
				}
			}
			return aBtn;
		}

		public float get_swidth_px()
		{
			return this.w;
		}

		public float get_sheight_px()
		{
			return this.h;
		}

		public float get_innerw_px()
		{
			return this.inw;
		}

		public float get_innerh_px()
		{
			return this.inh;
		}

		public float get_using_width_px()
		{
			return this.Row.get_swidth_px() + this.margin_in_lr * 2f + (this.use_scroll ? (this.scrolling_margin_in_lr * 2f) : 0f);
		}

		public float get_using_height_px()
		{
			return this.Row.get_sheight_px() + this.margin_in_tb * 2f + (this.use_scroll ? (this.scrolling_margin_in_tb * 2f) : 0f);
		}

		public bool isActive()
		{
			return this.animate_t >= 0f;
		}

		public virtual Designer activate()
		{
			bool flag = this.animate_t > 0f;
			base.gameObject.SetActive(true);
			if (this.animate_t < 0f)
			{
				this.animate_t = X.Mx((float)this.animate_maxt + this.animate_t, 0f);
			}
			if (!this.initted)
			{
				if (this.curs_active)
				{
					CURS.Active.Add(this.key);
				}
				return this;
			}
			this.bind();
			IN.clearCursDown();
			if (!flag)
			{
				this.selectDefSel();
			}
			if (this.TempMMRD != null)
			{
				this.TempMMRD.activate();
			}
			return this;
		}

		public void selectDefSel()
		{
			if (this.DefSel != null && !this.DefSel.isSelected())
			{
				this.DefSel.Select(false);
			}
		}

		public virtual Designer deactivate()
		{
			if (this.curs_active)
			{
				CURS.Active.Rem(this.key);
			}
			if (this.animate_t >= 0f)
			{
				this.animate_t = X.Mn((float)(-(float)this.animate_maxt) + this.animate_t, -1f);
			}
			this.hide();
			if (this.TempMMRD != null)
			{
				this.TempMMRD.deactivate(false);
			}
			return this;
		}

		public void Pause()
		{
			this.hide();
		}

		public void Resume()
		{
			this.bind();
		}

		public void OnDestroy()
		{
			if (!this.destructed_)
			{
				this.destruct();
			}
		}

		public virtual void destruct()
		{
			CURS.Active.Rem(this.key);
			this.destructed_ = true;
			if (this.MdKadomaru != null)
			{
				this.MdKadomaru.destruct();
				this.MdKadomaru = null;
			}
			this.AChild = null;
			IN.DestroyE(base.gameObject);
		}

		public float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ == value || this.destructed_)
				{
					return;
				}
				this.alpha_ = value;
				this.kadomaruRedraw(this.animate_t, true);
				foreach (KeyValuePair<IDesignerBlock, DesignerRowMem.DsnMem> keyValuePair in this.OBlockMem)
				{
					if (keyValuePair.Key is IAlphaSetable)
					{
						(keyValuePair.Key as IAlphaSetable).setAlpha(this.alpha_);
					}
				}
			}
		}

		public virtual void setAlpha(float value)
		{
			if (this.alpha != value)
			{
				this.alpha = value;
				this.Row.setAlpha(value);
			}
		}

		public string getValueString()
		{
			if (this.is_tab)
			{
				return "";
			}
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				foreach (KeyValuePair<string, IVariableObject> keyValuePair in this.OnamedObject)
				{
					if (stb.Length > 0)
					{
						stb.Ret("\n");
					}
					string valueString = keyValuePair.Value.getValueString();
					stb.Add(keyValuePair.Key, ",").Add(TX.countLine(valueString)).Ret("\n")
						.Add(valueString);
				}
				text = stb.ToString();
			}
			return text;
		}

		public int getValueI(string k, int isNaNdefault = 0, bool emptyDefault = true)
		{
			return X.NmI(this.getValue(k), isNaNdefault, emptyDefault, false);
		}

		public string getValue(string k)
		{
			IVariableObject variableObject = X.Get<string, IVariableObject>(this.OnamedObject, k);
			if (variableObject == null)
			{
				return "";
			}
			return variableObject.getValueString();
		}

		public BDic<string, string> getValue()
		{
			if (this.OVal == null)
			{
				this.OVal = new BDic<string, string>();
			}
			foreach (KeyValuePair<string, IVariableObject> keyValuePair in this.OnamedObject)
			{
				this.OVal[keyValuePair.Key] = this.OVal[keyValuePair.Value.getValueString()];
			}
			return this.OVal;
		}

		public void setValue(string s)
		{
			if (this.is_tab)
			{
				return;
			}
			string[] array = s.Split(new char[] { '\n' });
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				string text = array[i];
				int num2 = text.IndexOf(",");
				if (num2 != -1)
				{
					string text2 = TX.slice(text, 0, num2);
					int num3 = X.NmI(TX.slice(text, num2 + 1, text.Length), -1, true, false);
					if (num3 > 0)
					{
						string text3 = "";
						while (--num3 >= 0)
						{
							text3 = text3 + ((text3 == "") ? "" : "\n") + array[++i];
						}
						IVariableObject variableObject = X.Get<string, IVariableObject>(this.OnamedObject, text2);
						if (variableObject != null)
						{
							variableObject.setValue(text3);
						}
					}
				}
			}
		}

		public void setValueTo(string k, string s)
		{
			IVariableObject variableObject = X.Get<string, IVariableObject>(this.OnamedObject, k);
			if (variableObject != null)
			{
				variableObject.setValue(s);
			}
		}

		public BtnContainer<aBtn> getBtnContainer()
		{
			return this.BCon;
		}

		public BDic<string, GameObject> getWholeGobObject()
		{
			return this.Ogob;
		}

		public ScrollBox getScrollBox()
		{
			if (!this.use_scroll)
			{
				return null;
			}
			return this.Scr;
		}

		public GameObject getGob()
		{
			return base.gameObject;
		}

		public bool isContainElement(IDesignerBlock Blk)
		{
			if (this.Row == null)
			{
				return false;
			}
			int block_count = this.Row.block_count;
			for (int i = 0; i < block_count; i++)
			{
				if (this.Row.GetBlockByIndex(i) == Blk)
				{
					return true;
				}
			}
			return false;
		}

		public DesignerRowMem.DsnMem getDesignerBlockMemory(IDesignerBlock Blk)
		{
			return X.Get<IDesignerBlock, DesignerRowMem.DsnMem>(this.OBlockMem, Blk);
		}

		public BDic<IDesignerBlock, DesignerRowMem.DsnMem> getDesignerBlockMemoryObject()
		{
			return this.OBlockMem;
		}

		public bool getSCBFor(string name, out ScrollAppendBtnContainer<aBtn> ScB, out IDesignerBlock Blk)
		{
			if (this.OBlockMem != null)
			{
				foreach (KeyValuePair<IDesignerBlock, DesignerRowMem.DsnMem> keyValuePair in this.OBlockMem)
				{
					DesignerRowMem.DsnMem value = keyValuePair.Value;
					if (value.Blk is ScrollAppendBtnContainer<aBtn>)
					{
						ScrollAppendBtnContainer<aBtn> scrollAppendBtnContainer = value.Blk as ScrollAppendBtnContainer<aBtn>;
						if (scrollAppendBtnContainer.name == name)
						{
							ScB = scrollAppendBtnContainer;
							Blk = keyValuePair.Key;
							return true;
						}
					}
				}
			}
			ScB = null;
			Blk = null;
			return false;
		}

		public void RowRemakeHeightRecalc(IDesignerBlock Blk, DesignerRowMem.DsnMem Mem = null)
		{
			if (this.Row != null)
			{
				if (Mem == null)
				{
					Mem = this.getDesignerBlockMemory(Blk);
				}
				if (Mem != null)
				{
					float sheight_px = Blk.get_sheight_px();
					float swidth_px = Blk.get_swidth_px();
					if (sheight_px != Mem.h || swidth_px != Mem.w)
					{
						Mem.w = swidth_px;
						Mem.h = sheight_px;
						this.row_remake_flag = true;
					}
				}
			}
		}

		public Designer rowRemakeCheck(bool force = false)
		{
			if (this.row_remake_flag || force)
			{
				this.Row.Remake();
				this.fine_scroll_inner |= 1;
				this.row_remake_flag = false;
			}
			if (this.Row.row_assigned && this.Row.align != ALIGN.LEFT)
			{
				this.Row.BrRowAdjust();
			}
			this.fineScrollInner(true);
			return this;
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			if (only_front && this.DefSel != null)
			{
				ASlc.Add(this.DefSel);
				return;
			}
			if (this.BCon != null)
			{
				int length = this.BCon.Length;
				ASlc.Capacity = X.Mx(ASlc.Count + this.BCon.Length, ASlc.Capacity);
				for (int i = 0; i < length; i++)
				{
					ASlc.Add(this.BCon.Get(i));
					if (only_front)
					{
						return;
					}
				}
			}
		}

		public DesignerRowMem getRowManager()
		{
			this.checkInit();
			return this.Row;
		}

		public Designer reveal(Transform T, float posx = 0f, float posy = 0f, bool animate = true)
		{
			if (this.use_scroll && this.Scr != null)
			{
				this.Scr.reveal(T, posx, posy, animate);
			}
			return this;
		}

		public bool isShowingOnScrollBox(IDesignerBlock Blk, float margin_pixel_x = 0f, float margin_pixel_y = 0f)
		{
			return !this.use_scroll || this.Scr == null || this.Scr.isShowing(Blk, margin_pixel_x, margin_pixel_y, 0f, 0f);
		}

		public Color32 bgcol
		{
			get
			{
				return this.bgcol_;
			}
			set
			{
				if (C32.isEqual(value, this.bgcol_))
				{
					return;
				}
				this.bgcol_ = value;
				if (this.initted && this.MdKadomaru != null != this.use_mesh_background)
				{
					this.fineBackgroundMesh(false);
				}
				if (this.MdKadomaru != null && this.animate_t > (float)this.animate_maxt)
				{
					this.kadomaruRedraw(this.animate_t, true);
				}
			}
		}

		public bool use_mesh_background
		{
			get
			{
				return this.bgcol_.a > 0 || ((this.TabParentDesigner == null || this.TabParentDesigner.stencil_ref == -1) && this.stencil_ref >= 0);
			}
		}

		public float item_margin_x_px
		{
			get
			{
				return this.item_margin_x_px_;
			}
			set
			{
				this.item_margin_x_px_ = value;
				if (this.Row != null)
				{
					this.Row.margin_x_px = this.item_margin_x_px_;
				}
			}
		}

		public float item_margin_y_px
		{
			get
			{
				return this.item_margin_y_px_;
			}
			set
			{
				this.item_margin_y_px_ = value;
				if (this.Row != null)
				{
					this.Row.margin_y_px = this.item_margin_y_px_;
				}
			}
		}

		public bool scroll_area_selectable
		{
			get
			{
				return this.scroll_area_selectable_;
			}
			set
			{
				this.scroll_area_selectable_ = value;
				if (this.use_scroll && this.Scr != null)
				{
					this.Scr.area_selectable = value;
				}
			}
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
					IN.addRunner(this);
					return;
				}
				IN.remRunner(this);
			}
		}

		public int element_stencil_ref
		{
			get
			{
				if (!(this.CurTab != null))
				{
					return this.stencil_ref;
				}
				return this.CurTab.stencil_ref;
			}
		}

		public float use_w
		{
			get
			{
				if (this.CurTab)
				{
					return this.CurTab.use_w;
				}
				if (this.Row != null && (this.Row.has_real_block || this.Row.has_xsh))
				{
					return this.Row.use_w;
				}
				return this.w - this.margin_in_lr * 2f - (this.use_scroll ? (this.scrolling_margin_in_lr * 2f + 16f) : 0f);
			}
		}

		public float use_ws
		{
			get
			{
				if (this.CurTab)
				{
					return this.CurTab.use_ws;
				}
				if (this.Row != null)
				{
					return this.Row.use_w;
				}
				return this.w - this.margin_in_lr * 2f - (this.use_scroll ? (this.scrolling_margin_in_lr * 2f + 16f) : 0f);
			}
		}

		public float use_h
		{
			get
			{
				float num = this.h - this.margin_in_tb * 2f - (this.use_scroll ? (this.scrolling_margin_in_tb * 2f) : 0f);
				if (this.Row != null)
				{
					float sheight_px = this.Row.get_sheight_px();
					return X.Mx(0f, num - (sheight_px + ((sheight_px > 0f) ? this.item_margin_y_px_ : 0f)));
				}
				return num;
			}
		}

		public float scroll_inner_height
		{
			get
			{
				if (this.use_scroll && this.Scr != null)
				{
					this.rowRemakeCheck(false);
					return this.Scr.get_inner_h() * 64f + this.scrolling_margin_in_tb * 2f + 10f + 16f;
				}
				return this.get_sheight_px();
			}
		}

		public int tab_level
		{
			get
			{
				if (this.TabParentDesigner != null)
				{
					return this.TabParentDesigner.tab_level + 1;
				}
				return 0;
			}
		}

		public ScrollBox CurrentAttachScroll
		{
			get
			{
				Designer designer = this.CurTab;
				while (designer != null)
				{
					Designer curTab = designer.CurTab;
					if (curTab == null)
					{
						break;
					}
					designer = curTab;
				}
				if (designer == null)
				{
					designer = this;
				}
				while (!designer.use_scroll)
				{
					if (designer.TabParentDesigner == null)
					{
						return null;
					}
					designer = designer.TabParentDesigner;
				}
				if (!(designer == null) && designer.use_scroll)
				{
					return designer.Scr;
				}
				return null;
			}
		}

		public Designer CurrentAttachTarget
		{
			get
			{
				if (!(this.CurTab != null))
				{
					return this;
				}
				return this.CurTab.CurrentAttachTarget;
			}
		}

		public Designer[] getOffSpring()
		{
			if (this.AChild == null)
			{
				return new Designer[] { this };
			}
			List<Designer> list = new List<Designer>(this.AChild.Count + 1);
			list.Add(this);
			int count = this.AChild.Count;
			for (int i = 0; i < count; i++)
			{
				list.AddRange(this.AChild[i].getOffSpring());
			}
			return list.ToArray();
		}

		public static Designer getTopParentDesigner(GameObject Gob)
		{
			Designer component;
			for (;;)
			{
				component = Gob.GetComponent<Designer>();
				if (component != null)
				{
					break;
				}
				Transform parent = Gob.transform.parent;
				if (parent == null)
				{
					goto Block_1;
				}
				Gob = parent.gameObject;
			}
			return component;
			Block_1:
			return null;
		}

		public ALIGN alignx
		{
			get
			{
				return this.alignx_;
			}
			set
			{
				this.alignx_ = value;
				if (this.Row != null)
				{
					this.Row.Align(value);
				}
			}
		}

		public virtual bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				this.use_valotile_ = value;
				if (value)
				{
					this.prepare_valotile_ = true;
				}
				ValotileRenderer.RecheckUse(this.MdKadomaru, this.MrdKadomaru, ref this.ValotKadomaru, value);
				if (this.AValset != null)
				{
					for (int i = this.AValset.Count - 1; i >= 0; i--)
					{
						this.AValset[i].use_valotile = value;
					}
				}
			}
		}

		public bool prepare_valotile
		{
			set
			{
				if (value)
				{
					this.prepare_valotile_ = true;
				}
			}
		}

		public bool effect_confusion
		{
			get
			{
				if (this.TabParentDesigner != null)
				{
					return this.TabParentDesigner.effect_confusion;
				}
				return this.effect_confusion_;
			}
			set
			{
				if (this.TabParentDesigner != null)
				{
					this.TabParentDesigner.effect_confusion = value;
					return;
				}
				this.effect_confusion_ = value;
			}
		}

		public BtnContainer<aBtn> ElemBCon
		{
			get
			{
				if (this.CurTab != null)
				{
					return this.CurTab.ElemBCon;
				}
				return this.BCon;
			}
		}

		public DesignerRowMem ElemRow
		{
			get
			{
				if (this.CurTab != null)
				{
					return this.CurTab.ElemRow;
				}
				return this.Row;
			}
		}

		public ScrollBox AttachScroll
		{
			get
			{
				if (this.use_scroll && this.Scr != null)
				{
					return this.Scr;
				}
				return this.BelongScroll;
			}
		}

		public ScrollBox BelongScroll
		{
			get
			{
				if (!(this.TabParentDesigner != null))
				{
					return null;
				}
				return this.TabParentDesigner.AttachScroll;
			}
		}

		public float maxh_pixel
		{
			get
			{
				if (this.Row != null && (this.Row.has_real_block || this.Row.has_xsh))
				{
					return this.Row.get_sheight_px();
				}
				return 0f;
			}
		}

		public bool initted
		{
			get
			{
				return this.Row != null;
			}
		}

		public float animating_alpha
		{
			get
			{
				if (this.animate_maxt > 0)
				{
					return X.ZSIN(this.isActive() ? this.animate_t : ((float)this.animate_maxt + this.animate_t), (float)this.animate_maxt);
				}
				return 1f;
			}
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		private GameObject CreateInnerGob(string name)
		{
			if (this.CurTab != null)
			{
				return this.CurTab.CreateInnerGob(name);
			}
			return IN.CreateGob(base.gameObject, name);
		}

		public string key
		{
			get
			{
				if (this.name_ == null)
				{
					this.name_ = base.gameObject.name;
				}
				return this.name_;
			}
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<Designer> ";
				stb += this.key;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public static Vector3 showBottom(IDesignerBlock Target, IDesignerBlock BlkBase, ALIGN alignx = ALIGN.RIGHT, bool move_target = true)
		{
			Transform transform = BlkBase.getTransform();
			Vector3 position = transform.position;
			if (alignx != ALIGN.CENTER)
			{
				position.x += (-BlkBase.get_swidth_px() * transform.lossyScale.x + Target.get_swidth_px()) / 2f * 0.015625f * (float)X.MPF(alignx == ALIGN.RIGHT);
			}
			position.y -= (BlkBase.get_sheight_px() * transform.lossyScale.y / 2f + Target.get_sheight_px() / 2f) * 0.015625f;
			if (move_target)
			{
				IN.Pos2(Target.getTransform(), position);
			}
			return position;
		}

		public static Vector3 showTop(IDesignerBlock Target, IDesignerBlock BlkBase, ALIGN alignx = ALIGN.CENTER, bool move_target = true)
		{
			Transform transform = BlkBase.getTransform();
			Vector3 position = transform.position;
			if (alignx != ALIGN.CENTER)
			{
				position.x += (-BlkBase.get_swidth_px() * transform.lossyScale.x + Target.get_swidth_px()) / 2f * 0.015625f * (float)X.MPF(alignx == ALIGN.RIGHT);
			}
			position.y += (BlkBase.get_sheight_px() * transform.lossyScale.y / 2f + Target.get_sheight_px() / 2f + 4f) * 0.015625f;
			if (move_target)
			{
				IN.Pos2(Target.getTransform(), position);
			}
			return position;
		}

		public static Vector3 ScreenInner(IDesignerBlock Target, Vector3 Pos, bool move_target = true)
		{
			float num = Target.get_swidth_px() / 2f;
			float num2 = Target.get_sheight_px() / 2f;
			Pos.x = X.MMX(-IN.wh + num, Pos.x * 64f, IN.wh - num) * 0.015625f;
			Pos.y = X.MMX(-IN.hh + num2, Pos.y * 64f, IN.hh - num2) * 0.015625f;
			if (move_target)
			{
				IN.Pos2(Target.getTransform(), Pos);
			}
			return Pos;
		}

		public float w = 400f;

		public float h = 320f;

		private Color32 bgcol_ = MTRX.ColMenu;

		public float radius = 28f;

		public float margin_in_lr = 28f;

		public float margin_in_tb = 11f;

		private float item_margin_x_px_ = 14f;

		private float item_margin_y_px_ = 18f;

		public bool use_scroll;

		public float scrolling_margin_in_lr = 18f;

		public float scrolling_margin_in_tb = 10f;

		public int stencil_ref = -1;

		public bool use_canvas;

		public float scrollbox_bottom_margin = 38f;

		public bool auto_assign_runner = true;

		public bool curs_active = true;

		public bool no_write_mask;

		public int animate_maxt = 22;

		public bool use_button_connection;

		public int selectable_loop;

		public bool auto_destruct_when_deactivate = true;

		public int default_input_size = 14;

		public bool hiding_apply_to_skin;

		private bool effect_confusion_;

		public uint scroll_border_color;

		private bool scroll_area_selectable_;

		public bool animate_scaling_x = true;

		public bool animate_scaling_y = true;

		public int fine_scroll_inner;

		private bool destructed_;

		public bool auto_enable = true;

		private ALIGN alignx_ = ALIGN.LEFT;

		private float inner_row_width_ = -1f;

		protected float alpha_ = 1f;

		protected MeshDrawer MdKadomaru;

		protected MeshRenderer MrdKadomaru;

		protected ValotileRenderer ValotKadomaru;

		private ScrollBox Scr;

		protected Designer CurTab;

		private Designer TabParentDesigner;

		private bool is_tab;

		private float curtab_min_w;

		private float curtab_min_h;

		private string curtab_name = "";

		private float inw;

		private float inh;

		protected float animate_t;

		protected DesignerRowMem Row;

		protected BtnContainer<aBtn> BCon;

		private GameObject View;

		private Canvas ViewCvs;

		private int item_count;

		protected BDic<string, IPauseable> OPauseable;

		protected BDic<string, IVariableObject> OnamedObject;

		protected BDic<string, GameObject> Ogob;

		protected BDic<string, string> OVal;

		protected List<IValotileSetable> AValset;

		protected BDic<BtnContainerBasic, DsnData> ORadioMde;

		protected BDic<IDesignerBlock, DesignerRowMem.DsnMem> OBlockMem;

		private List<Designer> AChild;

		public bool row_remake_flag;

		private int default_focus_count = -100;

		private MultiMeshRenderer TempMMRD;

		protected aBtn DefSel;

		private bool use_valotile_;

		private bool prepare_valotile_;

		private int cancel_button_count = -100;

		protected aBtn CancelTargetBtn;

		private bool runner_assigned_;

		private const string DSN_GOB_DEFAULT_NAME_HEADER = "DSN_GOB_";

		public const string tab_name_header = "TAB__";

		public static FnBtnBindings fnAutoCheck = delegate(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			return true;
		};

		private string name_;

		private string _tostring;

		public class EvacuateMem
		{
			public EvacuateMem(DesignerRowMem.DsnMem Mem)
			{
				this.Blk = Mem.Blk;
				this.is_active = Mem.active;
				if (this.Blk is ObjCarrierConBlockBtnContainer<aBtn>)
				{
					BtnContainer<aBtn> bcon = (this.Blk as ObjCarrierConBlockBtnContainer<aBtn>).BCon;
					if (bcon != null)
					{
						this.DeactivateTarget = bcon;
						this.NameTarget = bcon.getRunner();
					}
				}
				if (this.Blk is ScrollAppendBtnContainer<aBtn>)
				{
					ScrollAppendBtnContainer<aBtn> scrollAppendBtnContainer = this.Blk as ScrollAppendBtnContainer<aBtn>;
					this.NameTarget = scrollAppendBtnContainer.getRunner();
				}
				this.w = Mem.w;
				this.h = Mem.h;
				this.line = Mem.line;
			}

			public Designer.EvacuateMem nameClear()
			{
				this.name = "";
				return this;
			}

			public Designer.EvacuateMem setNameForTab(string _name)
			{
				this.name = "TAB__" + _name;
				return this;
			}

			public void destructObject()
			{
				try
				{
					IN.DestroyOne(this.Blk.getTransform().gameObject);
				}
				catch
				{
				}
			}

			public static Designer.EvacuateMem SpliceFromAEvc(List<Designer.EvacuateMem> AEvc, Func<Designer.EvacuateMem, bool> Fn = null)
			{
				if (AEvc == null)
				{
					return null;
				}
				int count = AEvc.Count;
				for (int i = 0; i < count; i++)
				{
					Designer.EvacuateMem evacuateMem = AEvc[i];
					if (Fn == null || Fn(evacuateMem))
					{
						AEvc.RemoveAt(i);
						return evacuateMem;
					}
				}
				return null;
			}

			public static void Destroy(List<Designer.EvacuateMem> AEvc)
			{
				if (AEvc == null)
				{
					return;
				}
				int count = AEvc.Count;
				for (int i = 0; i < count; i++)
				{
					try
					{
						IDesignerBlock blk = AEvc[i].Blk;
						if (blk != null)
						{
							IN.DestroyE(blk.getTransform().gameObject);
						}
					}
					catch
					{
					}
				}
			}

			public IDesignerBlock Blk;

			public IVariableObject NameTarget;

			public IPauseable DeactivateTarget;

			public float w;

			public float h;

			public int line;

			public string name = "";

			public bool is_active = true;
		}

		public struct EvacuateContainer
		{
			public EvacuateContainer(Designer _Target, List<Designer.EvacuateMem> _AEvc, bool clear_target = false)
			{
				this.Target = _Target;
				this.AEvc = _AEvc;
				this.margin_in_lr = this.Target.margin_in_lr;
				this.margin_in_tb = this.Target.margin_in_tb;
				this.item_margin_x_px = this.Target.item_margin_x_px;
				this.item_margin_y_px = this.Target.item_margin_y_px;
				this.scrolling_margin_in_lr = this.Target.scrolling_margin_in_lr;
				this.scrolling_margin_in_tb = this.Target.scrolling_margin_in_tb;
				this.scrollbox_bottom_margin = this.Target.scrollbox_bottom_margin;
				this.radius = this.Target.radius;
				this.use_scroll = this.Target.use_scroll;
				this.OnamedObject = new BDic<string, IVariableObject>(this.Target.OnamedObject);
				this.Target.Ogob.Clear();
				if (clear_target)
				{
					this.Target.Clear();
				}
			}

			public EvacuateContainer(Designer _Target, bool clear_target = false)
			{
				this = new Designer.EvacuateContainer(_Target, _Target.EvacuateMemory(new List<Designer.EvacuateMem>(), null, false), clear_target);
			}

			public bool valid
			{
				get
				{
					return this.AEvc != null;
				}
			}

			public void release(Designer _Target = null)
			{
				if (this.AEvc != null)
				{
					this.Target = ((_Target == null) ? this.Target : _Target);
					if (this.Target != null)
					{
						this.Target.Clear();
					}
					for (int i = this.AEvc.Count - 1; i >= 0; i--)
					{
						Designer.EvacuateMem evacuateMem = this.AEvc[i];
						if (evacuateMem.Blk != null)
						{
							IN.DestroyOne(evacuateMem.Blk.getTransform().gameObject);
						}
					}
					this.AEvc = null;
				}
			}

			public bool reassign(Designer _Target = null)
			{
				if (this.AEvc != null)
				{
					this.Target = ((_Target == null) ? this.Target : _Target);
					bool flag = false;
					if (this.Target != null)
					{
						this.Target.margin_in_lr = this.margin_in_lr;
						this.Target.margin_in_tb = this.margin_in_tb;
						this.Target.item_margin_x_px = this.item_margin_x_px;
						this.Target.item_margin_y_px = this.item_margin_y_px;
						this.Target.scrolling_margin_in_lr = this.scrolling_margin_in_lr;
						this.Target.scrolling_margin_in_tb = this.scrolling_margin_in_tb;
						this.Target.scrollbox_bottom_margin = this.scrollbox_bottom_margin;
						this.Target.radius = this.radius;
						this.Target.use_scroll = this.use_scroll;
						this.Target.ReassignEvacuatedMemory(this.AEvc, this.OnamedObject, false);
						flag = true;
					}
					else
					{
						this.release(null);
					}
					this.AEvc = null;
					return flag;
				}
				return false;
			}

			public Designer Target;

			public List<Designer.EvacuateMem> AEvc;

			public BDic<string, IVariableObject> OnamedObject;

			private float margin_in_lr;

			private float margin_in_tb;

			private float item_margin_x_px;

			private float item_margin_y_px;

			private float scrolling_margin_in_lr;

			private float scrolling_margin_in_tb;

			private float scrollbox_bottom_margin;

			private float radius;

			private bool use_scroll;
		}
	}
}
