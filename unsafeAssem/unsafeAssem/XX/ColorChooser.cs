using System;
using UnityEngine;

namespace XX
{
	public class ColorChooser : Designer
	{
		public static ColorChooser MakeChooser(GameObject Parent, string gob_name)
		{
			return IN.CreateGobGUI(Parent, gob_name).AddComponent<ColorChooser>();
		}

		private static void createTexture(out Texture2D TxH, out Texture2D TxSV)
		{
			TxH = new Texture2D(1, 360, TextureFormat.RGB24, false);
			TxH.filterMode = FilterMode.Bilinear;
			TxSV = new Texture2D(50, 50, TextureFormat.RGBA32, false);
			TxSV.filterMode = FilterMode.Bilinear;
			Color[] array = new Color[360];
			C32 c = MTRX.colb.Set(uint.MaxValue);
			for (int i = 0; i < 360; i++)
			{
				array[i] = c.HSV((float)i, 100f, 100f).C;
			}
			TxH.SetPixels(array);
			TxH.Apply();
			array = new Color[TxSV.width * TxSV.height];
			for (int j = 0; j < TxSV.height; j++)
			{
				float num = (float)j / (float)(TxSV.height - 1);
				for (int k = 0; k < TxSV.width; k++)
				{
					float num2 = 1f - (float)k / (float)(TxSV.width - 1) * num;
					c.Set(uint.MaxValue).multiply(num, num, num, num2);
					array[j * TxSV.width + k] = c.C;
				}
			}
			TxSV.SetPixels(array);
			TxSV.Apply();
		}

		protected override void Awake()
		{
			base.Awake();
			this.C = new C32();
			base.bgcol = C32.d2c(2281701376U);
			base.item_margin_x_px = 12f;
			base.item_margin_y_px = 0f;
			this.margin_in_lr = 24f;
			this.margin_in_tb = 20f;
			this.w = 220f + this.margin_in_lr * 2f + 220f;
			this.h = 220f;
		}

		private void createColorChooser()
		{
			if (ColorChooser.TxH == null)
			{
				ColorChooser.createTexture(out ColorChooser.TxH, out ColorChooser.TxSV);
			}
			base.init();
			float use_w = base.use_w;
			float num = X.Mn(X.Mn(220f, base.use_h), use_w - base.item_margin_x_px - 24f);
			this.BtSV = this.addButtonT<aBtnDragger>(new DsnDataButton
			{
				name = "sv",
				title = "sv",
				w = num,
				h = num,
				skin = "colorchooser"
			});
			ButtonSkinDraggerColorChooser buttonSkinDraggerColorChooser = this.BtSV.get_Skin() as ButtonSkinDraggerColorChooser;
			buttonSkinDraggerColorChooser.initDraggerTexture(ColorChooser.TxSV);
			buttonSkinDraggerColorChooser.Bdr.addDraggingFn(new FnBtnBindings(this.fnDragArea));
			this.SkSV = buttonSkinDraggerColorChooser;
			this.BtH = this.addButtonT<aBtnDragger>(new DsnDataButton
			{
				name = "h",
				title = "h",
				w = 24f,
				h = num,
				skin = "colorchooser"
			});
			buttonSkinDraggerColorChooser = this.BtH.get_Skin() as ButtonSkinDraggerColorChooser;
			buttonSkinDraggerColorChooser.initDraggerTexture(ColorChooser.TxH);
			buttonSkinDraggerColorChooser.Bdr.moveable_x = 0f;
			buttonSkinDraggerColorChooser.Bdr.drag_loop_y = true;
			buttonSkinDraggerColorChooser.Bdr.addDraggingFn(new FnBtnBindings(this.fnDragArea));
			if (base.use_w < 120f)
			{
				base.Br();
			}
			Designer designer = this.addTab("right", base.use_w, num, base.use_w, num, false);
			designer.Smallest();
			designer.margin_in_lr = 4f;
			designer.item_margin_x_px = 14f;
			designer.item_margin_y_px = 4f;
			designer.init();
			float use_w2 = designer.use_w;
			DsnDataInput dsnDataInput = new DsnDataInput
			{
				bounds_w = (designer.use_w - designer.item_margin_x_px) / 2f - 4f,
				h = 30f,
				skin = "colorchooser_h",
				integer = true,
				min = 0.0,
				max = 100.0,
				fnChangedDelay = new FnFldBindings(this.fnChangedInput)
			};
			dsnDataInput.name = (dsnDataInput.label = "A");
			DsnDataInput dsnDataInput2 = dsnDataInput;
			DsnDataInput dsnDataInput3 = dsnDataInput;
			double num2 = 255.0;
			dsnDataInput2.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldA = designer.addInput(dsnDataInput);
			Designer designer2 = designer;
			DsnDataSlider dsnDataSlider = new DsnDataSlider();
			dsnDataSlider.name = "as";
			dsnDataSlider.title = "";
			dsnDataSlider.mn = 0f;
			dsnDataSlider.mx = 100f;
			dsnDataSlider.w = designer.use_w * 0.8f;
			dsnDataSlider.h = 30f;
			dsnDataSlider.valintv = 5f;
			dsnDataSlider.fnBtnMeterLine = delegate(aBtnMeter B, int index, float val)
			{
				if (val == 0f || val == 100f)
				{
					return 1f;
				}
				if (val != 50f)
				{
					return 0.3f;
				}
				return 0.75f;
			};
			dsnDataSlider.fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSliderAlpha);
			this.MeterA = designer2.addSliderT<aBtnMeter>(dsnDataSlider);
			designer.Br();
			dsnDataInput.name = (dsnDataInput.label = "H");
			DsnDataInput dsnDataInput4 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 360.0;
			dsnDataInput4.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldH = designer.addInput(dsnDataInput);
			dsnDataInput.name = (dsnDataInput.label = "R");
			DsnDataInput dsnDataInput5 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 255.0;
			dsnDataInput5.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldR = designer.addInput(dsnDataInput);
			dsnDataInput.name = (dsnDataInput.label = "S");
			DsnDataInput dsnDataInput6 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 100.0;
			dsnDataInput6.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldS = designer.addInput(dsnDataInput);
			dsnDataInput.name = (dsnDataInput.label = "G");
			DsnDataInput dsnDataInput7 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 255.0;
			dsnDataInput7.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldG = designer.addInput(dsnDataInput);
			dsnDataInput.name = (dsnDataInput.label = "V");
			DsnDataInput dsnDataInput8 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 100.0;
			dsnDataInput8.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldV = designer.addInput(dsnDataInput);
			dsnDataInput.name = (dsnDataInput.label = "B");
			DsnDataInput dsnDataInput9 = dsnDataInput;
			dsnDataInput3 = dsnDataInput;
			num2 = 255.0;
			dsnDataInput9.min = 0.0;
			dsnDataInput3.max = num2;
			this.FldB = designer.addInput(dsnDataInput);
			dsnDataInput.name = "D";
			dsnDataInput.label = "Code";
			dsnDataInput.bounds_w = use_w2 * 0.6f;
			dsnDataInput.hex_integer = true;
			dsnDataInput.max = 4294967295.0;
			this.FldD = designer.addInput(dsnDataInput);
			base.endTab(true);
		}

		public void init(uint col)
		{
			if (this.BtH == null)
			{
				this.createColorChooser();
			}
			this.setCode(col);
		}

		public void setCode(string rgbax)
		{
			this.C.rgbax = rgbax;
			this.setCode(this.C.rgba);
		}

		public void setCode(uint col)
		{
			this.C.Set(col);
			Vector3 vector = this.C.ToHSV();
			this.vHsv = new Vector3(vector.x, vector.y * 100f, vector.z * 100f);
			this.fineField(true);
		}

		private bool fnDragArea(aBtn B)
		{
			float num = this.BtH.getValueV(true).y * 360f;
			Vector2 valueV = this.BtSV.getValueV(true);
			this.vHsv.x = num;
			this.vHsv.y = valueV.x * 100f;
			this.vHsv.z = valueV.y * 100f;
			this.C.HSV(this.vHsv.x, this.vHsv.y, this.vHsv.z);
			this.fineField(false);
			this.FD_fnColChanged(this, this.C.rgba);
			return true;
		}

		private void fineField(bool fine_dragarea = true)
		{
			if (fine_dragarea)
			{
				this.BtH.setValue(new Vector2(0f, this.vHsv.x / 360f), true);
				this.BtSV.setValue(new Vector2(this.vHsv.y * 0.01f, this.vHsv.z * 0.01f), true);
			}
			uint rgba = MTRX.cola.rgba;
			this.SkSV.base_col = MTRX.cola.HSV(this.vHsv.x, 100f, 100f).setA1(1f).rgba;
			if (this.FldH != null)
			{
				this.MeterA.setValue((float)(this.C.a * 100) * 0.003921569f, false);
				this.FldH.setValue(((int)this.vHsv.x).ToString());
				this.FldS.setValue(((int)this.vHsv.y).ToString());
				this.FldV.setValue(((int)this.vHsv.z).ToString());
				this.FldA.setValue(this.C.a.ToString());
				this.FldR.setValue(this.C.r.ToString());
				this.FldG.setValue(this.C.g.ToString());
				this.FldB.setValue(this.C.b.ToString());
				this.FldD.setValue(this.C.rgbax);
			}
			MTRX.cola.rgba = rgba;
		}

		private bool fnChangedInput(LabeledInputField Li)
		{
			bool flag = false;
			string label = Li.label;
			if (label != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(label);
				if (num <= 3289118412U)
				{
					if (num <= 3238785555U)
					{
						if (num != 2036185364U)
						{
							if (num != 3238785555U)
							{
								goto IL_02CB;
							}
							if (!(label == "D"))
							{
								goto IL_02CB;
							}
						}
						else if (!(label == "Code"))
						{
							goto IL_02CB;
						}
						try
						{
							this.C.rgbax = Li.text;
							flag = true;
						}
						catch
						{
							return false;
						}
					}
					else if (num != 3255563174U)
					{
						if (num == 3289118412U)
						{
							if (label == "A")
							{
								this.C.a = (byte)X.NmI(Li.text, 0, false, false);
							}
						}
					}
					else if (label == "G")
					{
						this.C.g = (byte)X.NmI(Li.text, 0, false, false);
						flag = true;
					}
				}
				else if (num <= 3440116983U)
				{
					if (num != 3339451269U)
					{
						if (num == 3440116983U)
						{
							if (label == "H")
							{
								this.vHsv.x = (float)X.NmI(Li.text, 0, false, false);
								this.C.HSV(this.vHsv.x, this.vHsv.y, this.vHsv.z);
							}
						}
					}
					else if (label == "B")
					{
						this.C.b = (byte)X.NmI(Li.text, 0, false, false);
						flag = true;
					}
				}
				else if (num != 3540782697U)
				{
					if (num != 3591115554U)
					{
						if (num == 3607893173U)
						{
							if (label == "R")
							{
								this.C.r = (byte)X.NmI(Li.text, 0, false, false);
								flag = true;
							}
						}
					}
					else if (label == "S")
					{
						this.vHsv.y = (float)X.NmI(Li.text, 0, false, false);
						this.C.HSV(this.vHsv.x, this.vHsv.y, this.vHsv.z);
					}
				}
				else if (label == "V")
				{
					this.vHsv.z = (float)X.NmI(Li.text, 0, false, false);
					this.C.HSV(this.vHsv.x, this.vHsv.y, this.vHsv.z);
				}
			}
			IL_02CB:
			if (flag)
			{
				this.setCode(this.C.rgba);
				this.FD_fnColChanged(this, this.C.rgba);
			}
			else
			{
				this.fineField(true);
				this.FD_fnColChanged(this, this.C.rgba);
			}
			return true;
		}

		private bool fnChangedSliderAlpha(aBtnMeter _B, float pre_value, float cur_value)
		{
			this.C.a = (byte)(cur_value * 255f / 100f);
			this.fineField(true);
			this.FD_fnColChanged(this, this.C.rgba);
			return true;
		}

		public uint getColor()
		{
			return this.C.rgba;
		}

		public C32 getColorContainer()
		{
			return this.C;
		}

		private static Texture2D TxH;

		private static Texture2D TxSV;

		public ColorChooser.FnColChanged FD_fnColChanged = delegate(ColorChooser C, uint col)
		{
		};

		private aBtnDragger BtH;

		private aBtnDragger BtSV;

		private ButtonSkinDraggerColorChooser SkSV;

		private LabeledInputField FldA;

		private LabeledInputField FldH;

		private LabeledInputField FldS;

		private LabeledInputField FldV;

		private LabeledInputField FldR;

		private LabeledInputField FldG;

		private LabeledInputField FldB;

		private LabeledInputField FldD;

		private aBtnMeter MeterA;

		private C32 C;

		private Vector3 vHsv;

		public const int def_h = 220;

		public const int h_bar_width = 24;

		public delegate void FnColChanged(ColorChooser C, uint col);
	}
}
