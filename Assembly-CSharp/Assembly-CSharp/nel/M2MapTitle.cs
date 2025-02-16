using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2MapTitle : MonoBehaviour, IRunAndDestroy, IValotileSetable, INelMSG
	{
		public void Awake()
		{
			base.gameObject.layer = IN.LAY(IN.gui_layer_name);
			IN.setZ(base.transform, -3.249f);
			this.FlgNotShow = new Flagger(delegate(FlaggerT<string> _)
			{
				if (this.show_t >= 0f)
				{
					this.show_t = ((this.t == 0f) ? (-30f) : X.Mn(-1f, -30f + this.show_t));
				}
			}, delegate(FlaggerT<string> _)
			{
				if (this.show_t < 0f)
				{
					this.show_t = ((this.t == 0f) ? 30f : X.Mx(0f, 30f + this.show_t));
				}
			});
		}

		public void OnDestroy()
		{
			if (this.Md != null)
			{
				this.Md.destruct();
			}
		}

		public void initS(Map2d Mp)
		{
			if (this.Md == null)
			{
				this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				MeshRenderer component = base.GetComponent<MeshRenderer>();
				this.Valot = base.gameObject.AddComponent<ValotileRenderer>();
				this.Valot.InitUI(this.Md, component);
				this.TxB = IN.CreateGob(base.gameObject, "-Tx").AddComponent<NelEvTextRenderer>();
				IN.setZ(this.TxB.gameObject.transform, -0.0001f);
				this.TxB.alignx = ALIGN.RIGHT;
				this.TxB.aligny = ALIGNY.MIDDLE;
				this.TxB.html_mode = true;
				this.TxB.size = 13f;
				this.TxB.BorderColor = MTRX.ColBlack;
				this.TxB.line_spacing = 1.24f;
				this.TxB.initNelMsg(this);
				this.TxB.bold = true;
				this.TxT = IN.CreateGob(base.gameObject, "-Tx").AddComponent<TextRenderer>();
				this.TxT.alignx = ALIGN.RIGHT;
				this.TxT.aligny = ALIGNY.MIDDLE;
				this.TxT.size = 26f;
				this.TxT.can_create_big_size_texture = true;
				this.TxT.BorderColor = MTRX.ColBlack;
				this.TxT.bold = true;
				this.TxT.line_spacing = 1.24f;
				IN.setZ(this.TxB.gameObject.transform, -0.0001f);
				this.use_valotile = this.M2D.use_valotile;
				this.M2D.addValotAddition(this);
			}
			this.TxB.TargetFont = TX.getDefaultFont();
			this.TxT.TargetFont = TX.getTitleFont();
			string text = M2MapTitle.get_tx_key(Mp);
			if (text != null)
			{
				this.TxT.alpha = 0f;
				this.TxT.clearMesh();
				this.all_char_shown = false;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA(text, false);
					this.TxT.Txt(stb);
					stb.Clear().Add("<s2>");
					if (this.M2D.WM.CurWM != null)
					{
						stb.Add(this.M2D.WM.CurWM.localized_name);
					}
					this.TxB.forceProgressNextStack(stb);
				}
				this.FlgNotShow.Rem("_");
				if (this.t == -1000f)
				{
					base.gameObject.SetActive(true);
					IN.addRunner(this);
					if (this.show_t >= 0f)
					{
						this.show_t = 30f;
					}
					else
					{
						this.show_t = -30f;
					}
				}
				this.t = 0f;
				this.reposit(true);
				return;
			}
			this.FlgNotShow.Add("_");
			if (this.t == -1000f)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this.deactivate(false, true);
		}

		public void deactivate(bool immediate = false, bool rem_runner = true)
		{
			if (immediate || this.t == 0f)
			{
				if (this.t != -1000f)
				{
					this.t = -1000f;
					base.gameObject.SetActive(false);
					if (rem_runner)
					{
						IN.remRunner(this);
						return;
					}
				}
			}
			else if (this.t >= 0f)
			{
				this.FlgNotShow.Add("_");
				this.t = -1f - this.t;
				this.TxB.showImmediate(false, false);
			}
		}

		public void destruct()
		{
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			if (this.t >= 0f)
			{
				if (this.t == 0f && this.FlgNotShow.isActive())
				{
					return true;
				}
				if (this.t <= 60f)
				{
					flag = true;
					this.t += fcnt;
				}
				else if (this.all_char_shown)
				{
					if (this.t >= 180f)
					{
						this.deactivate(false, true);
						flag = true;
					}
					else
					{
						this.t += fcnt;
					}
				}
			}
			else
			{
				if (this.show_t <= -30f || this.t == -1000f)
				{
					this.deactivate(true, false);
					return false;
				}
				this.t -= fcnt;
			}
			if (this.show_t >= 0f)
			{
				if (this.show_t <= 30f)
				{
					this.show_t += fcnt;
					flag = true;
				}
			}
			else if (this.show_t >= -30f)
			{
				this.show_t -= fcnt;
				flag = true;
			}
			if (this.t >= 60f)
			{
				this.TxB.run(fcnt, false);
			}
			if (flag)
			{
				this.reposit(false);
			}
			return true;
		}

		private void reposit(bool first = false)
		{
			float num = X.Abs(this.t);
			float num2 = X.NIL(0.7f, 1f, X.ZSIN(num, 25f), 1f);
			float num3 = num2 * ((this.show_t >= 0f) ? X.ZLINE(this.show_t, 30f) : X.ZLINE(30f + this.show_t, 30f));
			float num4 = X.ZLINE(num, 25f);
			this.TxT.alpha = X.ZLINE(num - 4f, 48f);
			Vector3 localPosition = base.transform.localPosition;
			Vector3 localPosition2 = this.TxT.transform.localPosition;
			Vector3 localPosition3 = this.TxB.transform.localPosition;
			float num5 = this.TxT.get_sheight_px() + this.TxB.get_sheight_px() + 8f;
			if (first)
			{
				localPosition.y = (IN.hh - 34f - num5 * 0.5f) * 0.015625f;
				localPosition2.y = (4f + this.TxT.get_sheight_px() * 0.5f - 2f) * 0.015625f;
				localPosition3.y = (-(4f + this.TxB.get_sheight_px() * 0.5f) - 2f) * 0.015625f;
			}
			float num6 = X.Mx(this.TxB.get_swidth_px(), X.Mx(this.TxT.get_swidth_px(), 220f)) + 80f;
			localPosition.x = (IN.wh + 1f + num6 * (1f - num3)) * 0.015625f;
			base.transform.localPosition = localPosition;
			localPosition2.x = (localPosition3.x = X.NI(15, -18, num3) * 0.015625f);
			this.TxT.transform.localPosition = localPosition2;
			this.TxB.transform.localPosition = localPosition3;
			this.Md.clear(false, false);
			this.Md.Col = this.Md.ColGrd.Black().mulA(num4 * 0.88f).C;
			this.Md.ColGrd.mulA(0f);
			num5 += 16f;
			float num7 = (float)X.IntR((num6 - 1f) * num2);
			float num8 = (float)X.IntR(140f * num2);
			this.Md.TriRectBL(3, 2, 4, 5);
			this.Md.RectBLGradation(-num7, -num5 * 0.5f, num8, num5, GRD.RIGHT2LEFT, false);
			this.Md.PosD(1f, num5 * 0.5f, null);
			this.Md.PosD(1f, -num5 * 0.5f, null);
			this.Md.updateForMeshRenderer(false);
		}

		public void progressReserved()
		{
		}

		public void executeRestMsgCmd(int count)
		{
		}

		public bool all_char_shown { get; set; }

		public string talker_snd
		{
			get
			{
				return null;
			}
		}

		public void setHkdsTypeToDefault(bool reserve = false)
		{
		}

		public void setHkdsType(NelMSG.HKDSTYPE _type)
		{
		}

		public EMOT default_emot
		{
			get
			{
				return EMOT.FADEIN;
			}
		}

		public uint default_col
		{
			get
			{
				return uint.MaxValue;
			}
		}

		public bool is_temporary_hiding
		{
			get
			{
				return false;
			}
		}

		public void TextRendererUpdated()
		{
		}

		public void FixTextContent(STB Stb)
		{
		}

		public override string ToString()
		{
			return "M2MapTitle";
		}

		public bool use_valotile
		{
			get
			{
				return this.Valot != null && this.Valot.enabled;
			}
			set
			{
				if (this.Valot != null)
				{
					this.Valot.enabled = true;
					this.TxT.use_valotile = true;
					this.TxB.use_valotile = true;
				}
			}
		}

		public static string get_tx_key(Map2d Mp)
		{
			string text = "MAP_" + Mp.key;
			if (TX.getTX(text, true, true, null) != null)
			{
				return text;
			}
			return null;
		}

		public NelM2DBase M2D;

		private MeshDrawer Md;

		private TextRenderer TxT;

		private NelEvTextRenderer TxB;

		private float t = -1000f;

		private float show_t = 30f;

		private const float MAXT_MAIN = 25f;

		private const float T_AUTO_HIDE = 180f;

		private const float T_TXB_PROG = 60f;

		private const float MAXT_SHOW = 30f;

		public Flagger FlgNotShow;

		public ValotileRenderer Valot;
	}
}
