using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class TutorialBox : OneLineBox, ITutorialBox
	{
		protected override void Awake()
		{
			base.Awake();
			this.FlgDeclining = new Flagger(delegate(FlaggerT<string> V)
			{
				this.deactivate();
			}, delegate(FlaggerT<string> V)
			{
				if (this.active_key != null)
				{
					this.activate();
				}
			});
			this.M2D = M2DBase.Instance;
			base.margin(new float[] { 250f, 10f, 250f, 10f });
			base.TxSize(24f);
			this.html_mode = true;
			this.ACapPF = new List<PxlFrame>();
			this.Acap_tex = new List<string>();
			this.FbCaption = IN.CreateGob(this, "-FbCaption").AddComponent<FillImageBlock>();
			this.FbCaption.size = 24f;
			this.FbCaption.FnDraw = new MeshDrawer.FnGeneralDraw(this.fnDrawImageCaption);
			this.FbCaption.fix_width = true;
			this.FbCaption.widthPixel = 260f;
			this.FbCaption.text_auto_condense = true;
			this.FbCaption.TxCol = MTRX.ColBlack;
			this.FbCaption.TxBorderCol = MTRX.ColWhite;
			this.FbCaption.alignx = ALIGN.RIGHT;
			this.FbCaption.StartFb(" ", null, true);
			this.clearCaption();
			base.make(" ");
			base.col(2861206141U);
		}

		public float local_z
		{
			get
			{
				return this.local_z_;
			}
			set
			{
				this.local_z_ = value;
				IN.setZ(base.transform, this.local_z_);
				if (this.use_valotile)
				{
					CameraBidingsBehaviour.UiBind.need_sort_binds = true;
				}
			}
		}

		public override MsgBox make(string _tex)
		{
			base.make(_tex);
			if (this.Tx != null)
			{
				IN.setZ(this.Tx.transform, -0.15f);
			}
			this.wh(X.Mx(320f, (this.Tx != null) ? (this.Tx.get_swidth_px() + 20f) : this.w), this.Tx.size * 1.2f);
			this.ui_shift_x = ((this.M2D != null) ? this.M2D.ui_shift_x : 0f);
			if (base.isActive())
			{
				this.showt = 0;
				this.t = 0f;
			}
			this.finePos();
			return this;
		}

		private void finePos()
		{
			float num = (float)(-(float)this.aligny);
			base.position(0f, num * IN.hh * 0.8f, 0f, num * IN.hh * 0.6f, false);
		}

		public void AddText(string tex, int time, string image_key)
		{
			if (this.active_key == null)
			{
				this.aligny = ALIGNY.BOTTOM;
			}
			this.active_key = tex;
			tex = TX.ReplaceTX(tex, false);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			this.make(tex);
			this.clearCaption();
			this.ACapPF.Clear();
			this.Acap_tex.Clear();
			if (this.FlgDeclining.isActive())
			{
				base.gameObject.SetActive(false);
			}
			else
			{
				this.activate();
			}
			this.show_time = time;
		}

		public void AddImage(PxlFrame Img, string tex)
		{
			if (this.caption_t < 0)
			{
				this.ACapPF.Clear();
				this.Acap_tex.Clear();
				this.FbCaption.gameObject.SetActive(true);
			}
			this.cap_remove_flag = false;
			this.ACapPF.Add(Img);
			this.Acap_tex.Add(tex);
			if (this.caption_t < 0)
			{
				this.caption_t = 0;
				this.drawCaption();
			}
		}

		public void destructTutoBox()
		{
			if (!this.destructed)
			{
				this.clearCaption();
			}
			base.destruct();
		}

		public void RemText(bool whole_clear, bool immediate = true)
		{
			this.cap_remove_flag = true;
			if (this.active_key != null)
			{
				this.active_key = null;
				if (base.isActive())
				{
					this.deactivate();
				}
			}
			if (immediate && !base.isActive())
			{
				base.fineMove();
				base.gameObject.SetActive(false);
			}
		}

		public override MsgBox run(float fcnt, bool force_draw = false)
		{
			if (this.show_time >= 0 && this.t >= (float)this.show_time)
			{
				this.deactivate();
			}
			if (this.t > (float)(-(float)this.t_hide) && this.M2D != null && this.ui_shift_x != this.M2D.ui_shift_x)
			{
				this.ui_shift_x = this.M2D.ui_shift_x;
				this.post = X.Mn((this.t >= 0f) ? this.maxt_pos : this.maxt_pos_hide, this.post);
			}
			if (this.caption_t >= 0 && X.D)
			{
				this.runCaption();
			}
			MsgBox msgBox = base.run(fcnt, force_draw);
			if (!msgBox && this.cap_remove_flag)
			{
				this.clearCaption();
			}
			return msgBox;
		}

		public override MsgBox SetXyPx(float x, float y)
		{
			return base.SetXyPx(x + this.ui_shift_x, y);
		}

		public void remActiveFlag(string key)
		{
			this.FlgDeclining.Rem(key);
		}

		public void addActiveFlag(string key)
		{
			this.FlgDeclining.Add(key);
		}

		public void setPosition(string is_left, string _is_bottom)
		{
			ALIGNY aligny = ALIGNY.BOTTOM;
			if (_is_bottom != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(_is_bottom);
				if (num > 3322673650U)
				{
					if (num <= 3356228888U)
					{
						if (num != 3339451269U)
						{
							if (num != 3356228888U)
							{
								goto IL_0103;
							}
							if (!(_is_bottom == "M"))
							{
								goto IL_0103;
							}
							goto IL_00FD;
						}
						else if (!(_is_bottom == "B"))
						{
							goto IL_0103;
						}
					}
					else if (num != 3407844522U)
					{
						if (num != 3507227459U)
						{
							goto IL_0103;
						}
						if (!(_is_bottom == "T"))
						{
							goto IL_0103;
						}
						goto IL_0101;
					}
					else if (!(_is_bottom == "BOTTOM"))
					{
						goto IL_0103;
					}
					aligny = ALIGNY.BOTTOM;
					goto IL_0103;
				}
				if (num <= 879771736U)
				{
					if (num != 692843612U)
					{
						if (num != 879771736U)
						{
							goto IL_0103;
						}
						if (!(_is_bottom == "MIDDLE"))
						{
							goto IL_0103;
						}
					}
					else
					{
						if (!(_is_bottom == "TOP"))
						{
							goto IL_0103;
						}
						goto IL_0101;
					}
				}
				else if (num != 2865109572U)
				{
					if (num != 3322673650U)
					{
						goto IL_0103;
					}
					if (!(_is_bottom == "C"))
					{
						goto IL_0103;
					}
				}
				else if (!(_is_bottom == "CENTER"))
				{
					goto IL_0103;
				}
				IL_00FD:
				aligny = ALIGNY.MIDDLE;
				goto IL_0103;
				IL_0101:
				aligny = ALIGNY.TOP;
			}
			IL_0103:
			if (this.aligny != aligny)
			{
				this.aligny = aligny;
				if (this.active_key != null)
				{
					this.finePos();
				}
			}
		}

		public void setPositionDefault()
		{
			if (this.aligny != ALIGNY.TOP)
			{
				this.aligny = ALIGNY.TOP;
				if (this.active_key != null)
				{
					this.finePos();
				}
			}
		}

		private void clearCaption()
		{
			this.cap_remove_flag = false;
			this.caption_t = -1;
			this.FbCaption.getMeshDrawer().clear(false, false);
			this.FbCaption.MI = null;
			this.FbCaption.gameObject.SetActive(false);
		}

		private bool fnDrawImageCaption(MeshDrawer Md, float alpha)
		{
			if (this.caption_t < 0)
			{
				return true;
			}
			int num = this.caption_t / 150 % this.ACapPF.Count;
			PxlFrame pxlFrame = this.ACapPF[num];
			PxlImage img = pxlFrame.getLayer(0).Img;
			Md.Col = C32.MulA(uint.MaxValue, alpha);
			Md.RotaPF(-this.FbCaption.widthPixel * 0.5f + (float)img.width * 0.5f, (float)img.height * 0.4f, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			Md.updateForMeshRenderer(false);
			return true;
		}

		private int drawCaption()
		{
			if (this.caption_t < 0)
			{
				return -1;
			}
			int num = this.caption_t / 150 % this.ACapPF.Count;
			string text = this.Acap_tex[num];
			if (TX.valid(text))
			{
				if (this.ACapPF.Count > 1)
				{
					using (STB stb = TX.PopBld(null, 0))
					{
						stb.Add(num + 1).Add(": ", text, "");
						this.FbCaption.Txt(stb);
						goto IL_00A5;
					}
				}
				this.FbCaption.text_content = text;
			}
			else
			{
				this.FbCaption.text_content = "";
			}
			IL_00A5:
			PxlFrame pxlFrame = this.ACapPF[num];
			PxlImage img = pxlFrame.getLayer(0).Img;
			this.FbCaption.widthPixel = (float)img.width * (TX.valid(text) ? 1.5f : 1f);
			this.FbCaption.MI = MTRX.getMI(pxlFrame);
			Vector3 localPosition = this.FbCaption.transform.localPosition;
			localPosition.y = ((float)(-(float)img.height) * 0.9f - 6f) * 0.015625f;
			this.FbCaption.transform.localPosition = localPosition;
			this.FbCaption.fineWH(true);
			this.FbCaption.alpha = 0f;
			return num;
		}

		private void runCaption()
		{
			int num = this.caption_t / 150 % this.ACapPF.Count;
			int num2 = num * 150;
			int num3 = num2 + 150;
			float num4 = X.ZSIN((float)(this.caption_t - num2), 20f);
			float num5 = X.ZSIN((float)(num3 - this.caption_t), 20f);
			this.FbCaption.alpha = num4 * num5 * base.showing_alpha;
			Vector3 localPosition = this.FbCaption.transform.localPosition;
			localPosition.x = (30f * (1f - num4) - 30f * (1f - num5)) * 0.015625f;
			this.FbCaption.transform.localPosition = localPosition;
			if (this.ACapPF.Count > 1 || (float)this.caption_t < 20f)
			{
				this.caption_t += X.AF;
				if (this.caption_t >= num3)
				{
					num = this.drawCaption();
					this.caption_t = num * 150;
				}
			}
		}

		public override bool use_valotile
		{
			set
			{
				if (value == base.use_valotile)
				{
					return;
				}
				base.use_valotile = value;
				this.FbCaption.use_valotile = value;
			}
		}

		private M2DBase M2D;

		private Flagger FlgDeclining;

		private float ui_shift_x;

		private string active_key;

		private int show_time = -1;

		private FillImageBlock FbCaption;

		private int caption_t = -1;

		private List<PxlFrame> ACapPF;

		private List<string> Acap_tex;

		private bool cap_remove_flag;

		private const float CAP_FADE_T = 20f;

		private const float CAP_TRANSITION_T = 150f;

		private float local_z_;
	}
}
