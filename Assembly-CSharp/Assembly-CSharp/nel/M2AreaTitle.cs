using System;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2AreaTitle : MonoBehaviour
	{
		private void Awake()
		{
			base.gameObject.layer = IN.LAY(IN.gui_layer_name);
			this.deactivate(true);
			this.Tx = IN.CreateGob(base.gameObject, "-Tx").AddComponent<TextRenderer>();
			IN.setZ(this.Tx.gameObject.transform, -0.0001f);
			this.Tx.TargetFont = TX.getTitleFont();
			this.Tx.alignx = ALIGN.CENTER;
			this.Tx.aligny = ALIGNY.MIDDLE;
			this.Tx.size = (float)this.Tx.TargetFont.fontSize;
			this.Tx.line_spacing = 1.1f;
			this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshAdd, 0f, -1, null, false, false);
			this.Mrd = base.GetComponent<MeshRenderer>();
			this.Valot = base.gameObject.AddComponent<ValotileRenderer>();
			this.Valot.InitUI(this.Md, this.Mrd);
			this.Tx.MakeValot(null, null);
			this.Tx.gameObject.SetActive(false);
		}

		public void OnDestroy()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
		}

		public void init(string tex, bool _dark_area, float size = 0f, bool _use_underline = true, bool no_fine_time = false)
		{
			if (!no_fine_time)
			{
				this.t = 0f;
			}
			if (this.Md == null)
			{
				this.Awake();
			}
			base.gameObject.SetActive(true);
			this.fineFonts();
			this.Tx.text_content = tex;
			this.x = 0f;
			this.y = IN.hh * 0.3f;
			this.use_underline = _use_underline;
			if (this.dark_area != _dark_area)
			{
				this.dark_area = _dark_area;
				Material material = ((!this.dark_area) ? MTRX.MtrMeshAdd : MTRX.MtrMeshNormal);
				this.Mrd.sharedMaterials = new Material[] { material };
				this.Md.setMaterial(material, false);
			}
			this.Tx.size = ((size <= 0f) ? ((float)this.Tx.TargetFont.fontSize) : size);
			this.Tx.Col(this.dark_area ? 4278190080U : uint.MaxValue);
			this.Tx.alpha = 0f;
			this.Md.clear(false, false);
			this.Md.updateForMeshRenderer(false);
			this.activate();
		}

		public void fineFonts()
		{
			if (this.Tx == null)
			{
				this.Awake();
			}
			this.Tx.TargetFont = TX.getTitleFont();
		}

		public bool valotile_enabled
		{
			get
			{
				return this.Tx.use_valotile;
			}
			set
			{
				this.Tx.use_valotile = value;
				this.Valot.enabled = value;
			}
		}

		public void activate()
		{
			this.finePos();
			this.M2D.MapTitle.FlgNotShow.Add("AREATITLE");
			if (this.t < 0f)
			{
				return;
			}
			this.Tx.gameObject.SetActive(true);
		}

		public void deactivate(bool immediate)
		{
			if (this.M2D != null && this.M2D.MapTitle != null)
			{
				this.M2D.MapTitle.FlgNotShow.Rem("AREATITLE");
			}
			if (immediate)
			{
				this.t = -1f;
				base.gameObject.SetActive(false);
				return;
			}
			this.Tx.gameObject.SetActive(false);
		}

		public void hideProgress()
		{
			if (!this.isActive())
			{
				return;
			}
			if (this.t >= 0f && this.t < 330f)
			{
				this.t = 330f;
			}
		}

		public void finePos()
		{
			IN.PosP(base.transform, this.M2D.ui_shift_x + this.x, this.y, -3.875f);
		}

		public void finePosPixelShift(float _x, float _y)
		{
			this.x = _x;
			this.y = _y;
			this.finePos();
		}

		public void run(float fcnt)
		{
			float num = 0f;
			if (this.t < 370f)
			{
				if (this.t >= 50f)
				{
					if (this.t <= 50f && this.M2D.FlgAreaTitleHide.isActive())
					{
						if (this.use_underline)
						{
							this.Md.clear(false, false);
						}
						this.t = 0f;
						this.Tx.alpha = 0f;
						return;
					}
					float num2 = this.t - 50f;
					if (this.use_underline)
					{
						float num3 = this.line_w * X.ZSIN(num2, 80f) / 2f;
						float line_y = this.line_y;
						this.Md.Col = this.Md.ColGrd.Set(this.dark_area ? 4278190080U : uint.MaxValue).mulA(X.ZLINE(num2, 30f)).C;
						this.Md.Line(-num3, line_y, num3, line_y, 1f, false, 0f, 0f);
					}
					num = X.ZLINE(num2 - 33f, 70f);
					this.Tx.alpha = num;
					if (this.t >= 355f)
					{
						this.M2D.MapTitle.FlgNotShow.Rem("AREATITLE");
					}
				}
			}
			else
			{
				float num4 = this.t - 370f;
				if (num4 >= 80f)
				{
					this.deactivate(true);
					return;
				}
				num = 1f - X.ZLINE(num4, 80f);
				if (this.use_underline)
				{
					this.Md.Col = this.Md.ColGrd.Set(this.dark_area ? 4278190080U : uint.MaxValue).mulA(num).C;
					float num5 = this.line_w / 2f;
					float line_y2 = this.line_y;
					this.Md.Line(-num5, line_y2, num5, line_y2, 1f, false, 0f, 0f);
				}
				this.Tx.alpha = num;
			}
			if (num > 0f)
			{
				this.drawEffect(num);
			}
			this.Md.updateForMeshRenderer(false);
			this.t += fcnt;
		}

		private void drawEffect(float tz)
		{
			float line_w = this.line_w;
			float num = this.t - 50f - 33f;
			for (int i = 0; i < 22; i++)
			{
				float num2 = num - 19.2f * (float)i;
				if (num2 >= 0f)
				{
					num2 %= 240f;
					float num3 = X.ZLINE(num2, 240f);
					float num4 = X.ZLINE(num2, 100f) * (1f - X.ZLINE(num2 - 80f, 160f)) * tz;
					if (num3 != 0f)
					{
						uint ran = X.GETRAN2(i * 7 + 93, i % 11);
						float num5 = X.RAN(ran, 2693) * 6.2831855f;
						float num6 = X.NI(60, 80, X.RAN(ran, 695)) * num3;
						float num7 = X.NI(20, 40, X.RAN(ran, 2707)) * num3;
						float num8 = 60f * (-0.5f + X.RAN(ran, 2409)) + (num6 + this.Tx.get_swidth_px() * 0.5f * X.NI(0.6f, 0.8f, X.RAN(ran, 2034))) * X.Cos(num5);
						float num9 = 24f * (-0.5f + X.RAN(ran, 1203)) + (num7 + this.Tx.get_sheight_px() * 0.5f * X.NI(0.5f, 0.74f, X.RAN(ran, 899))) * X.Sin(num5);
						float num10 = X.NI(13, 30, X.RAN(ran, 2705));
						this.Md.Col = this.Md.ColGrd.Set(1441396976).blend(0U, 1f - num4).C;
						this.Md.ColGrd.Transparent();
						this.Md.Circle(num8, num9, num10, 0f, false, 1f, 0f);
						this.Md.Col = this.Md.ColGrd.Set(4293523696U).setA1(X.NI(0.6f, 0.8f, X.RAN(ran, 1421)) * num4).C;
						this.Md.Box(num8, num9, 1f, 1f, 0f, false);
					}
				}
			}
		}

		public float line_w
		{
			get
			{
				return this.Tx.get_swidth_px() * 1.1f + 140f;
			}
		}

		public float line_y
		{
			get
			{
				return -(this.Tx.get_sheight_px() * 0.5f + 18f);
			}
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		private const int HEAD_T = 50;

		private const int maxt = 370;

		private const int TEXT_DELAY_T = 33;

		private const int FADEOUT_T = 80;

		public float t;

		private bool dark_area;

		private TextRenderer Tx;

		private MeshRenderer Mrd;

		private MeshDrawer Md;

		public NelM2DBase M2D;

		private bool use_underline;

		private float x;

		private float y;

		private ValotileRenderer Valot;
	}
}
