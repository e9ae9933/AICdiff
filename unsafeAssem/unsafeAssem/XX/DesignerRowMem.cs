using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class DesignerRowMem : DesignerRow
	{
		public DesignerRowMem(GameObject _Gob, float _bounds_w_px = 0f, DesignerRowMem.FnDesignerRowAddBlock _fnAddBlock = null)
			: base(_Gob, _bounds_w_px)
		{
			this.ABlk = new DesignerRowMem.DsnMem[16];
			this.fnAddBlock = _fnAddBlock;
			this.PreVal = new DesignerRow.DesignerRowVariable();
		}

		public override DesignerRow Clear(bool destruct_block = false)
		{
			base.Clear(false);
			for (int i = this.ABlk.Length - 1; i >= 0; i--)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (dsnMem != null)
				{
					if (destruct_block && dsnMem.Blk != null)
					{
						IN.DestroyE(dsnMem.Blk.getTransform().gameObject);
					}
					dsnMem.Blk = null;
				}
			}
			this.ids_cnt = (this.ids_line = 0);
			this.align = ALIGN.LEFT;
			return this;
		}

		public int block_count
		{
			get
			{
				return this.ids_cnt;
			}
		}

		public DesignerRow Align(ALIGN _align)
		{
			if (this.align != _align)
			{
				if (this.row_w <= 0f)
				{
					this.row_w_marg = 0f;
				}
				else
				{
					this.Br(true);
				}
				this.align = _align;
				this.Br(true);
			}
			return this;
		}

		private DesignerRowMem.DsnMem newDM(IDesignerBlock _Blk, float swidth, float sheight, bool activate = true)
		{
			DesignerRowMem.DsnMem dsnMem;
			if (this.ids_cnt >= this.ABlk.Length || this.ABlk[this.ids_cnt] == null)
			{
				X.pushToEmptyS<DesignerRowMem.DsnMem>(ref this.ABlk, dsnMem = new DesignerRowMem.DsnMem(null), ref this.ids_cnt, 16);
			}
			else
			{
				DesignerRowMem.DsnMem[] ablk = this.ABlk;
				int num = this.ids_cnt;
				this.ids_cnt = num + 1;
				dsnMem = ablk[num];
			}
			dsnMem.Blk = _Blk;
			dsnMem.w = swidth;
			dsnMem.h = sheight;
			dsnMem.line = this.ids_line;
			this.fine_designer_connect = true;
			dsnMem.active = activate;
			return dsnMem;
		}

		public DesignerRow Add(IDesignerBlock Blk, bool is_active)
		{
			float swidth_px = Blk.get_swidth_px();
			float sheight_px = Blk.get_sheight_px();
			return this.Add(Blk, swidth_px, sheight_px, this.newDM(Blk, swidth_px, sheight_px, is_active));
		}

		private DesignerRow Add(IDesignerBlock _Blk, float swidth, float sheight, DesignerRowMem.DsnMem DM)
		{
			this.PreVal.Push(this);
			base.Add(_Blk, swidth, sheight);
			if (!DM.active)
			{
				this.PreVal.Pop(this);
			}
			DM.line = this.ids_line;
			if (this.fnAddBlock != null)
			{
				this.fnAddBlock(_Blk, this.ids_line);
			}
			this.fine_designer_connect = true;
			return this;
		}

		public DesignerRow Add(DesignerRowMem.DsnMem Src)
		{
			return this.Add(Src.Blk, Src.w, Src.h, this.newDM(Src.Blk, Src.w, Src.h, true));
		}

		public DesignerRowMem HideLast()
		{
			if (this.ids_cnt == 0)
			{
				return this;
			}
			this.ABlk[this.ids_cnt - 1].active = false;
			this.PreVal.Pop(this);
			return this;
		}

		public DesignerRowMem Rem(IDesignerBlock _Blk, bool fine_flag = true)
		{
			int i = 0;
			while (i < this.ids_cnt)
			{
				if (this.ABlk[i].Blk == _Blk)
				{
					DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
					int line = dsnMem.line;
					float w = dsnMem.w;
					X.shiftEmpty<DesignerRowMem.DsnMem>(this.ABlk, 1, i, -1);
					DesignerRowMem.DsnMem[] ablk = this.ABlk;
					DesignerRowMem.DsnMem dsnMem2 = dsnMem;
					int num = this.ids_cnt - 1;
					this.ids_cnt = num;
					X.unshiftEmpty<DesignerRowMem.DsnMem>(ablk, dsnMem2, num, 1, -1);
					if (fine_flag || this.ids_cnt == 0)
					{
						this.Remake();
						return this;
					}
					if (i >= this.ids_cnt || this.ABlk[i].line != line)
					{
						this.row_w = X.Mx(0f, this.row_w - w - this.margin_x_px);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			return this;
		}

		public override DesignerRow XSh(float x)
		{
			if (x < 0f)
			{
				return this;
			}
			base.XSh(x);
			this.newDM(null, x, -1f, true);
			return this;
		}

		public override DesignerRow Br(bool manual_br = true)
		{
			if (manual_br || this.preserve_br_point)
			{
				this.newDM(null, (float)((this.align == ALIGN.LEFT) ? (-1) : ((this.align == ALIGN.CENTER) ? (-2) : (-3))), -1f, true);
			}
			if (this.row_w <= 0f)
			{
				this.row_w_marg = 0f;
				return this;
			}
			float row_w = this.row_w;
			this.BrRowAdjust();
			return this;
		}

		public void BrRowAdjust()
		{
			if (this.align != ALIGN.LEFT)
			{
				for (int i = this.ids_cnt - 1; i >= 0; i--)
				{
					DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
					if (dsnMem.Blk != null)
					{
						if (dsnMem.line == this.ids_line)
						{
							Transform transform = dsnMem.Blk.getTransform();
							Vector3 localPosition = transform.localPosition;
							localPosition.x += (this.bounds_w_px - this.row_w) * ((this.align == ALIGN.CENTER) ? 0.5f : 1f) * 0.015625f;
							transform.localPosition = localPosition;
						}
						else if (dsnMem.line < this.ids_line)
						{
							break;
						}
					}
				}
			}
			this.ids_line++;
			base.Br(false);
		}

		public DesignerRowMem.DsnMem getBlockMemory(IDesignerBlock Blk)
		{
			if (Blk == null)
			{
				return null;
			}
			for (int i = this.ids_cnt - 1; i >= 0; i--)
			{
				if (this.ABlk[i].Blk == Blk)
				{
					return this.ABlk[i];
				}
			}
			return null;
		}

		public DesignerRowMem Remake()
		{
			int num = this.ids_cnt;
			if (this.ids_cnt == 0)
			{
				this.Clear(false);
				return this;
			}
			base.Clear(false);
			this.align = ALIGN.LEFT;
			this.fine_designer_connect = true;
			this.ids_cnt = (this.ids_line = 0);
			bool flag = this.preserve_br_point;
			this.preserve_br_point = false;
			for (int i = 0; i < num; i++)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				this.Reassign(dsnMem);
			}
			this.preserve_br_point = flag;
			return this;
		}

		public void Reassign(DesignerRowMem.DsnMem M)
		{
			if (M.Blk != null)
			{
				this.Add(M.Blk, M.w, M.h, M);
				this.ids_cnt++;
				return;
			}
			if (M.w < 0f)
			{
				this.align = ((M.w == -1f) ? ALIGN.LEFT : ((M.w == -2f) ? ALIGN.CENTER : ALIGN.RIGHT));
				this.Br(true);
				return;
			}
			this.XSh(M.w);
		}

		public BList<aBtn> PopLastLineSelectable(bool only_front = false)
		{
			BList<aBtn> blist = ListBuffer<aBtn>.Pop(0);
			bool flag = true;
			int i;
			for (i = this.ids_cnt - 1; i >= 0; i--)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (dsnMem.Blk == null)
				{
					if (dsnMem.w < 0f && !flag)
					{
						break;
					}
				}
				else
				{
					flag = false;
				}
			}
			for (i++; i < this.ids_cnt; i++)
			{
				DesignerRowMem.DsnMem dsnMem2 = this.ABlk[i];
				if (dsnMem2.Blk != null)
				{
					dsnMem2.Blk.AddSelectableItems(blist, only_front);
				}
			}
			return blist;
		}

		public BList<aBtn> PopFirstLineSelectable(bool only_front = false)
		{
			BList<aBtn> blist = ListBuffer<aBtn>.Pop(0);
			bool flag = true;
			int i;
			for (i = 0; i < this.ids_cnt; i++)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (dsnMem.Blk == null)
				{
					if (dsnMem.w < 0f && !flag)
					{
						break;
					}
				}
				else
				{
					flag = false;
				}
			}
			for (int j = 0; j < i; j++)
			{
				DesignerRowMem.DsnMem dsnMem2 = this.ABlk[j];
				if (dsnMem2.Blk != null)
				{
					dsnMem2.Blk.AddSelectableItems(blist, only_front);
				}
			}
			return blist;
		}

		public void checkConnect(int selectable_loop)
		{
			this.fine_designer_connect = false;
			int num = this.ids_cnt;
			using (BList<aBtn> blist = ListBuffer<aBtn>.Pop(0))
			{
				using (BList<aBtn> blist2 = ListBuffer<aBtn>.Pop(0))
				{
					using (BList<aBtn> blist3 = ListBuffer<aBtn>.Pop(0))
					{
						bool flag = true;
						int num2 = 0;
						for (int i = 0; i < num; i++)
						{
							DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
							if (dsnMem.Blk != null)
							{
								if (dsnMem.line > num2)
								{
									if (blist.Count > 0)
									{
										if (flag && (selectable_loop & 2) > 0)
										{
											flag = false;
											blist3.AddRange(blist);
										}
										else
										{
											this.checkConnectRow(blist, (blist2.Count > 0) ? blist2 : null, selectable_loop, false, false);
										}
										blist2.Clear();
										blist2.AddRange(blist);
										blist.Clear();
									}
									num2 = dsnMem.line;
								}
								dsnMem.Blk.AddSelectableItems(blist, false);
							}
						}
						this.checkConnectRow(blist, (blist2.Count > 0) ? blist2 : null, selectable_loop, false, false);
						if (!flag)
						{
							this.checkConnectRow(blist3, blist, selectable_loop, false, false);
						}
						else if ((selectable_loop & 2) > 0)
						{
							this.checkConnectRow(blist, blist, selectable_loop | 8, false, false);
						}
					}
				}
			}
		}

		public void checkConnectRow(List<aBtn> ACur, List<aBtn> APre, int selectable_loop, bool important_tb = false, bool can_override = false)
		{
			int count = ACur.Count;
			new List<aBtn>();
			aBtn aBtn = (((selectable_loop & 1) != 0 && count >= 2) ? ACur[count - 1] : null);
			int num = ((APre != null) ? APre.Count : 0);
			bool flag = (selectable_loop & 8) != 0;
			for (int i = 0; i < count; i++)
			{
				aBtn aBtn2 = ACur[i];
				if (aBtn != null)
				{
					aBtn2.setNaviL(aBtn, false, false);
				}
				if (num > 0)
				{
					aBtn2.setNaviT(APre[flag ? (num - 1) : 0], false, important_tb);
					for (int j = 0; j < num; j++)
					{
						int num2 = (flag ? (num - 1 - j) : j);
						if (can_override || (APre[num2].navi_setted & 8) == 0)
						{
							APre[num2].setNaviB(aBtn2, false, important_tb);
						}
					}
				}
				aBtn = aBtn2;
			}
		}

		public void setAlpha(float value)
		{
			int num = this.ids_cnt;
			for (int i = 0; i < num; i++)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (dsnMem.Blk != null && dsnMem.Blk is IAlphaSetable)
				{
					(dsnMem.Blk as IAlphaSetable).setAlpha(value);
				}
			}
		}

		public DesignerRowMem setNaviToRow(int line, AIM a, List<aBtn> ADest, bool only_empty = true, bool reversible = false, bool important = false)
		{
			int count = ADest.Count;
			if (count == 0)
			{
				return this;
			}
			int num = (int)CAim.get_opposite(a);
			List<aBtn> list = new List<aBtn>();
			int i = 0;
			while (i < this.ids_cnt)
			{
				DesignerRowMem.DsnMem dsnMem;
				if (only_empty)
				{
					dsnMem = this.ABlk[i];
					if (dsnMem.Blk != null && dsnMem.line >= line)
					{
						if (dsnMem.line > line)
						{
							break;
						}
						goto IL_008F;
					}
				}
				else
				{
					dsnMem = this.ABlk[this.ids_cnt - 1 - i];
					if (dsnMem.Blk != null && dsnMem.line <= line)
					{
						if (dsnMem.line >= line)
						{
							goto IL_008F;
						}
						break;
					}
				}
				IL_013F:
				i++;
				continue;
				IL_008F:
				dsnMem.Blk.AddSelectableItems(list, false);
				int count2 = list.Count;
				for (int j = 0; j < count2; j++)
				{
					aBtn aBtn = list[j];
					if (!only_empty || aBtn.getNaviAim((int)a) == null)
					{
						aBtn.setNaviAim((int)a, ADest[0], false, important);
					}
					if (reversible)
					{
						for (int k = 0; k < count; k++)
						{
							aBtn aBtn2 = ADest[only_empty ? k : (count - 1 - k)];
							if (!only_empty || !(aBtn2.getNaviAim(num) != null))
							{
								aBtn2.setNaviAim(num, aBtn, false, important);
							}
						}
					}
				}
				list.Clear();
				goto IL_013F;
			}
			return this;
		}

		public int getLastRow()
		{
			for (int i = this.ids_cnt - 1; i >= 0; i--)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (dsnMem.Blk != null)
				{
					return dsnMem.line;
				}
			}
			return this.ids_line;
		}

		public bool has_real_block
		{
			get
			{
				for (int i = this.ids_cnt - 1; i >= 0; i--)
				{
					if (this.ABlk[i].Blk != null)
					{
						return true;
					}
				}
				return false;
			}
		}

		public IDesignerBlock GetBlockByIndex(int i)
		{
			return this.ABlk[i].Blk;
		}

		public bool has_content
		{
			get
			{
				return this.has_real_block || base.has_xsh;
			}
		}

		public void copyMems(List<DesignerRowMem.DsnMem> AList, Func<Designer, DesignerRowMem.DsnMem, bool> FnAvailableBlock = null, Designer DsForFn = null)
		{
			for (int i = 0; i < this.ids_cnt; i++)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (FnAvailableBlock == null || FnAvailableBlock(DsForFn, dsnMem))
				{
					AList.Add(dsnMem);
				}
			}
		}

		public void copyMems(List<Designer.EvacuateMem> AList, Func<Designer, DesignerRowMem.DsnMem, bool> FnAvailableBlock = null, Designer DsForFn = null)
		{
			for (int i = 0; i < this.ids_cnt; i++)
			{
				DesignerRowMem.DsnMem dsnMem = this.ABlk[i];
				if (FnAvailableBlock == null || FnAvailableBlock(DsForFn, dsnMem))
				{
					Designer.EvacuateMem evacuateMem = new Designer.EvacuateMem(dsnMem);
					AList.Add(evacuateMem);
				}
			}
		}

		public DesignerRowMem setNaviToRow(int line, AIM a, IDesignerBlock Dest, bool only_empty = true, bool reversible = false, bool important = false)
		{
			List<aBtn> list = new List<aBtn>();
			Dest.AddSelectableItems(list, false);
			return this.setNaviToRow(line, a, list, only_empty, reversible, important);
		}

		public DesignerRowMem setNaviLastRow(AIM a, IDesignerBlock Dest, bool only_empty = true, bool reversible = false, bool important = false)
		{
			List<aBtn> list = new List<aBtn>();
			Dest.AddSelectableItems(list, false);
			this.setNaviToRow(this.getLastRow(), a, list, only_empty, reversible, important);
			return this;
		}

		protected DesignerRow.DesignerRowVariable PreVal;

		protected DesignerRowMem.DsnMem[] ABlk;

		protected int ids_cnt;

		protected int ids_line;

		public bool preserve_br_point;

		public DesignerRowMem.FnDesignerRowAddBlock fnAddBlock;

		public bool fine_designer_connect;

		public ALIGN align = ALIGN.LEFT;

		public delegate void FnDesignerRowAddBlock(IDesignerBlock Blk, int last_added_line);

		public class DsnMem
		{
			public DsnMem(DesignerRowMem.DsnMem _Mem = null)
			{
				if (_Mem != null)
				{
					this.Blk = _Mem.Blk;
					this.w = _Mem.w;
					this.h = _Mem.h;
					this.line = _Mem.line;
					this.active = _Mem.active;
				}
			}

			public virtual bool active
			{
				get
				{
					return this.active_;
				}
				set
				{
					if (this.active_ == value)
					{
						return;
					}
					this.active_ = value;
					if (this.Blk != null)
					{
						this.Blk.getTransform().gameObject.SetActive(this.active_);
					}
				}
			}

			public IDesignerBlock Blk;

			public float w;

			public float h;

			public int line;

			protected bool active_ = true;
		}
	}
}
