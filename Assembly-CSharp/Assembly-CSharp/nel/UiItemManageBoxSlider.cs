using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiItemManageBoxSlider : UiItemManageBox
	{
		public UiItemManageBoxSlider(NelItemManager _IMNG, Transform TrsEvacuateTo = null)
			: base(_IMNG, TrsEvacuateTo)
		{
			this.OSliderData = new BDic<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB>();
		}

		protected override void initDesignerContentMain()
		{
			this.DefaultTargetInventory = null;
			base.initDesignerContentMain();
		}

		protected override bool fineItemUsingCommand(aBtnItemRow Bi)
		{
			if (this.BxCmd == null)
			{
				return false;
			}
			this.OSliderData.Clear();
			this.BWheelTarget = null;
			if (!base.fineItemUsingCommand(Bi))
			{
				return false;
			}
			if (this.Inventory == null)
			{
				return false;
			}
			this.Inventory.MemoryNewer(this.UsingTarget);
			if (this.DefaultTargetInventory != null)
			{
				this.DefaultTargetInventory.MemoryNewer(this.UsingTarget);
			}
			if (this.OSliderData.Count > 0)
			{
				this.ANeedCheckGrade = new List<aBtnMeterNel>(this.OSliderData.Count);
			}
			return true;
		}

		public void assignSliderDsn(aBtnMeterNel Bslider, UiItemManageBoxSlider.DsnDataSliderIMB Mde)
		{
			this.OSliderData[Bslider] = Mde;
		}

		protected override void changeGradeCursorInUsing(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade)
		{
			foreach (KeyValuePair<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB> keyValuePair in this.OSliderData)
			{
				this.fineMeter(keyValuePair.Key, keyValuePair.Value, Itm, Obt);
			}
			base.changeGradeCursorInUsing(Itm, Obt, grade);
		}

		protected override void closeUsingState(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			this.ANeedCheckGrade = null;
			this.Inventory.MemoryNewer(null);
			if (this.DefaultTargetInventory != null)
			{
				this.DefaultTargetInventory.MemoryNewer(null);
			}
			base.closeUsingState(Itm, Obt);
			this.OSliderData.Clear();
			this.BWheelTarget = null;
		}

		public aBtnMeterNel createSlider(UiItemManageBoxSlider.DsnDataSliderIMB Mde, float width_fill_ratio = 1f)
		{
			if (Mde.fnChanged == null)
			{
				Mde.fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangeSlider);
			}
			if (Mde.fnClick == null)
			{
				Mde.fnClick = new FnBtnBindings(this.fnClickDefault);
			}
			aBtnMeterNel aBtnMeterNel = this.BxCmd.addSliderCT(Mde, (this.BxCmd.use_w - 2f) * width_fill_ratio, null, false);
			if (width_fill_ratio >= 1f)
			{
				this.BxCmd.Br();
			}
			this.OSliderData[aBtnMeterNel] = Mde;
			if (Mde.use_wheel)
			{
				this.BWheelTarget = aBtnMeterNel;
			}
			this.fineMeter(aBtnMeterNel, Mde);
			return aBtnMeterNel;
		}

		private bool fnClickDefault(aBtn B)
		{
			if (TX.valid(this.meter_click_btn_name))
			{
				aBtn btn = this.BxCmd.getBtn(this.meter_click_btn_name);
				if (btn != null)
				{
					if (this.meter_click_btn_name.IndexOf("Cancel") >= 0)
					{
						btn.SetChecked(true, true);
					}
					btn.ExecuteOnClick();
				}
			}
			return true;
		}

		public void fineMeter(aBtnMeterNel Meter, UiItemManageBoxSlider.DsnDataSliderIMB Mde = null)
		{
			if (Mde == null)
			{
				Mde = X.Get<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB>(this.OSliderData, Meter);
			}
			NelItem nelItem = null;
			ItemStorage.ObtainInfo obtainInfo = null;
			if (!base.getCurrentFocusItem(true, out nelItem, out obtainInfo))
			{
				return;
			}
			this.fineMeter(Meter, Mde, nelItem, obtainInfo);
		}

		private void fineMeter(aBtnMeterNel Meter, UiItemManageBoxSlider.DsnDataSliderIMB Mde, NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			int num;
			int num2;
			int num3;
			if (!this.getMeterVariable(Meter, Mde, Itm, Obt, this.grade_cursor, out num, out num2, out num3))
			{
				return;
			}
			Meter.initMeter((float)num2, (float)num3, 1f, (float)num, 0f);
		}

		private bool getMeterVariable(aBtnMeterNel Meter, UiItemManageBoxSlider.DsnDataSliderIMB Mde, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade, out int meter_v, out int meter_mn, out int meter_max)
		{
			if (Mde != null && Mde.fnGetMeterVariable != null && Mde.fnGetMeterVariable(Meter, Itm, Obt, grade, out meter_v, out meter_mn, out meter_max))
			{
				return true;
			}
			if (this.DefaultTargetInventory != null)
			{
				int reduceable = this.DefaultTargetInventory.getReduceable(Itm, grade);
				meter_v = reduceable;
				int num = this.Inventory.getReduceable(Itm, grade);
				if (!this.DefaultTargetInventory.infinit_stockable)
				{
					num = X.Mn(this.DefaultTargetInventory.getItemCapacity(Itm, false, false), num);
				}
				meter_max = meter_v + num;
				meter_mn = 0;
				return true;
			}
			meter_v = 0;
			meter_mn = 0;
			meter_max = 1;
			return false;
		}

		private bool fnChangeSlider(aBtnMeter _B, float pre_value_float, float cur_value_float)
		{
			aBtnMeterNel aBtnMeterNel = _B as aBtnMeterNel;
			UiItemManageBoxSlider.DsnDataSliderIMB dsnDataSliderIMB;
			return !(aBtnMeterNel == null) && (dsnDataSliderIMB = X.Get<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB>(this.OSliderData, aBtnMeterNel)) != null && this.fnChangeSlider(aBtnMeterNel, pre_value_float, cur_value_float, dsnDataSliderIMB);
		}

		public bool fnChangeSlider(aBtnMeterNel B, float pre_value_float, float cur_value_float, UiItemManageBoxSlider.DsnDataSliderIMB Mde)
		{
			int num = X.IntR(cur_value_float);
			int num2 = X.IntR(pre_value_float);
			int num3 = num - num2;
			bool flag = true;
			if (Mde.fnSlidedMeter != null)
			{
				int num4 = Mde.fnSlidedMeter(B, num3, num2, num);
				if (num4 != num)
				{
					B.setValue((float)(num = num4), false);
					num3 = num - num2;
					flag = false;
				}
			}
			if (this.DefaultTargetInventory != null)
			{
				int num5 = num3;
				NelItem usingTarget = this.UsingTarget;
				this.animation_immediate_flag = true;
				if (num5 > 0)
				{
					if (usingTarget != null && this.DefaultTargetInventory.isAddable(usingTarget))
					{
						int num6 = X.Mn(num5, this.Inventory.getReduceable(usingTarget, this.grade_cursor));
						num6 = this.DefaultTargetInventory.Add(usingTarget, num6, this.grade_cursor, true, false);
						if (num6 > 0)
						{
							num5 -= num6;
							this.DefaultTargetInventory.Add(usingTarget, num6, this.grade_cursor, true, true);
							this.Inventory.Reduce(usingTarget, num6, this.grade_cursor, true);
							this.DefaultTargetInventory.fineSpecificRow(usingTarget);
						}
					}
					if (num5 > 0)
					{
						B.setValue((float)(num -= num5), false);
						num3 = num - num2;
						flag = false;
					}
				}
				else
				{
					num5 = -num5;
					if (usingTarget != null)
					{
						int num7 = X.Mn(num5, this.DefaultTargetInventory.getReduceable(usingTarget, this.grade_cursor));
						num7 = X.Mn(num7, this.Inventory.Add(usingTarget, num7, this.grade_cursor, true, false));
						if (num7 > 0)
						{
							num5 -= num7;
							this.DefaultTargetInventory.Reduce(usingTarget, num7, this.grade_cursor, true);
							this.Inventory.Add(usingTarget, num7, this.grade_cursor, true, true);
						}
					}
					if (num5 > 0)
					{
						B.setValue((float)(num += num5), false);
						num3 = num - num2;
						flag = false;
					}
				}
			}
			if (this.ANeedCheckGrade != null && this.ANeedCheckGrade.IndexOf(B) == -1)
			{
				this.ANeedCheckGrade.Add(B);
			}
			return flag;
		}

		protected override bool runEditItem(bool using_mode, bool first = false, bool execute_select_button = true)
		{
			if (!base.runEditItem(using_mode, first, execute_select_button))
			{
				return false;
			}
			if (using_mode && this.ANeedCheckGrade != null)
			{
				int count = this.ANeedCheckGrade.Count;
				int grade_cursor = this.grade_cursor;
				if (this.wheel_lock < IN.totalframe && this.BWheelTarget != null && this.BxCmd != null && CLICK.getClickableRectSimple(IN.getMousePos(null), this.BxCmd.transform, (this.BxCmd.get_swidth_px() + 40f) * 0.015625f, (this.BxCmd.get_sheight_px() + 40f) * 0.015625f))
				{
					if (IN.MouseWheel.y < 0f)
					{
						this.wheel_lock = IN.totalframe + 4;
						this.BWheelTarget.simulateLRtoSetter(this.BWheelTarget, AIM.R, true);
					}
					else if (IN.MouseWheel.y > 0f)
					{
						this.wheel_lock = IN.totalframe + 4;
						this.BWheelTarget.simulateLRtoSetter(this.BWheelTarget, AIM.L, true);
					}
				}
				if (count > 0 && base.fineCount(false) && this.grade_cursor == grade_cursor)
				{
					for (int i = 0; i < count; i++)
					{
						aBtnMeterNel aBtnMeterNel = this.ANeedCheckGrade[i];
						int num = X.IntR(aBtnMeterNel.getValue());
						UiItemManageBoxSlider.DsnDataSliderIMB dsnDataSliderIMB = X.Get<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB>(this.OSliderData, aBtnMeterNel);
						int num2 = X.IntR(aBtnMeterNel.minval);
						int num3 = X.IntR(aBtnMeterNel.maxval);
						bool flag = num == num2;
						bool flag2 = num == num3;
						if (num2 != num3 && (flag || flag2))
						{
							NelItem nelItem = null;
							ItemStorage.ObtainInfo obtainInfo = null;
							if (base.getCurrentFocusItem(this.state == UiItemManageBox.STATE.USING, out nelItem, out obtainInfo))
							{
								int grade_cursor2 = this.grade_cursor;
								for (int j = 1; j < 5; j++)
								{
									int num4 = (grade_cursor2 + ((j == 0) ? 0 : ((j + 1) / 2 * X.MPF(j % 2 == 1))) + 10) % 5;
									if (this.OTouchGrade == null || base.isGradeTouched(nelItem, obtainInfo, num4, false))
									{
										bool flag3 = false;
										int num5;
										int num6;
										int num7;
										if (this.getMeterVariable(aBtnMeterNel, dsnDataSliderIMB, nelItem, obtainInfo, num4, out num5, out num6, out num7))
										{
											if (flag ? (num5 > num6) : (num5 < num7))
											{
												flag3 = true;
											}
										}
										else
										{
											flag3 = true;
										}
										if (flag3)
										{
											int num8 = X.MPF(this.grade_cursor < num4);
											this.grade_cursor = num4 - num8;
											base.checkGradeShift(true, nelItem, obtainInfo, num8, false, true);
											break;
										}
									}
								}
							}
						}
					}
				}
				this.ANeedCheckGrade.Clear();
				if (this.grade_cursor != grade_cursor)
				{
					SND.Ui.play("tool_gradation", false);
				}
			}
			return true;
		}

		private aBtnMeterNel BWheelTarget;

		private int wheel_lock;

		private BDic<aBtnMeterNel, UiItemManageBoxSlider.DsnDataSliderIMB> OSliderData;

		public string meter_click_btn_name = "Cancel";

		public ItemStorage DefaultTargetInventory;

		private List<aBtnMeterNel> ANeedCheckGrade;

		public delegate bool FnGetMeterVariable(aBtnMeterNel Btn, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade, out int meter_v, out int meter_mn, out int meter_max);

		public delegate int FnSlidedMeter(aBtnMeterNel Btn, int inclease, int pre_val, int cur_val);

		public class DsnDataSliderIMB : DsnDataSlider
		{
			public DsnDataSliderIMB(UiItemManageBoxSlider.FnGetMeterVariable fnGetMeter, UiItemManageBoxSlider.FnSlidedMeter fnSlided)
			{
				this.fnGetMeterVariable = fnGetMeter;
				this.fnSlidedMeter = fnSlided;
				this.skin_title = " ";
				this.skin = "invisible";
				this.def = 0f;
				this.mn = 0f;
				this.mx = 1f;
				this.valintv = 1f;
				this.w = 1f;
			}

			public UiItemManageBoxSlider.FnGetMeterVariable fnGetMeterVariable;

			public UiItemManageBoxSlider.FnSlidedMeter fnSlidedMeter;

			public bool use_wheel;
		}
	}
}
