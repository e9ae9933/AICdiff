using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class NelTextRendererSLine : NelEvTextRenderer
	{
		protected override void Awake()
		{
			base.Awake();
			base.html_mode = true;
			this.Aline_ver_pos = new List<int>(3);
			this.Asyasen = new List<Vector2>(3);
			if (this.MaLine == null)
			{
				this.MaLine = new MdArranger(this.Md);
			}
		}

		public override void entryMesh()
		{
			base.entryMesh();
			base.initMeshSub(TextRenderer.MESH_TYPE.MESH_T1);
			this.MaLine.Set(true);
			this.Asyasen.Clear();
			this.snd_played = false;
			if (this.syasen_maxt_ > 0f)
			{
				this.Md.Col = this.Md.ColGrd.Set(this.Style.MyCol).mulA(base.alpha).C;
				this.Md.Identity();
				int count = this.Aline_ver_pos.Count;
				if (count >= 2)
				{
					int num = this.Aline_ver_pos[0];
					this.Md.allocVer(this.Md.getVertexMax() + count * 4, 0);
					Vector3[] vertexArray = this.Md.getVertexArray();
					float num2 = 0f;
					for (int i = 1; i < count; i++)
					{
						int num3 = this.Aline_ver_pos[i];
						Vector3 vector = vertexArray[num];
						Vector3 vector2 = vertexArray[num + 1];
						Vector3 vector3 = vertexArray[num3 - 1];
						Vector3 vector4 = vertexArray[num3 - 2];
						uint num4 = 1U << i - 1;
						float num5 = X.NI(vector.x, vector2.x, 0.5f);
						float num6 = X.Mx(X.NI(vector.y, vector2.y, 0.5f), X.NI(vector3.y, vector4.y, 0.5f));
						float num7 = X.Mx(X.NI(vector3.x, vector4.x, 0.5f) - num5, 0f);
						this.Md.RectBL(num5, num6 - this.stroke_t_px_ * 0.015625f * 0.5f, ((this.immediate_line_bits_ & num4) == 0U) ? 0f : num7, this.stroke_t_px_ * 0.015625f, true);
						this.Asyasen.Add(new Vector2(num7, 0f));
						num2 += num7;
						num = num3;
					}
					if (num2 > 0f)
					{
						for (int j = 1; j < count; j++)
						{
							Vector2 vector5 = this.Asyasen[j - 1];
							vector5.y = vector5.x / num2;
							this.Asyasen[j - 1] = vector5;
						}
					}
					else
					{
						this.Asyasen.Clear();
					}
				}
			}
			this.MaLine.Set(false);
		}

		public bool syasen_drawn
		{
			get
			{
				if (this.syasen_maxt_ > 0f)
				{
					return this.Asyasen.Count == 0;
				}
				return this.t >= this.syasen_after_delay;
			}
		}

		protected override void mainUpdateRun(float fcnt)
		{
			base.mainUpdateRun(fcnt);
			float num = this.t - this.syasen_startt_;
			if (num >= 0f && this.syasen_maxt_ > 0f && this.Asyasen != null && this.Asyasen.Count > 0)
			{
				bool flag = false;
				int count = this.Asyasen.Count;
				Vector3[] vertexArray = this.Md.getVertexArray();
				int num2 = this.MaLine.getStartVerIndex();
				float num3 = num / this.syasen_maxt_;
				Color32 c = this.Md.ColGrd.Set(this.Style.MyCol).mulA(this.Style.alpha).C;
				int num4 = 0;
				while (num4 < count && num3 > 0f)
				{
					Vector2 vector = this.Asyasen[num4];
					float x = vertexArray[num2].x;
					uint num5 = 1U << num4;
					if ((this.disable_line_bits_ & num5) != 0U)
					{
						num2 += 4;
					}
					else
					{
						float num6 = vector.x * (((this.immediate_line_bits_ & num5) != 0U) ? 1f : X.ZLINE(num3, vector.y));
						for (int i = 2; i < 4; i++)
						{
							Vector3 vector2 = vertexArray[num2 + i];
							vector2.x = x + num6;
							vertexArray[num2 + i] = vector2;
						}
						num2 += 4;
						if ((this.immediate_line_bits_ & num5) == 0U)
						{
							num3 -= vector.y;
							flag = true;
						}
					}
					num4++;
				}
				if (!this.snd_played && this.syasen_maxt_ >= 2f && flag)
				{
					this.snd_played = true;
					SND.Ui.play(this.snd_syasen_start, false);
				}
				if (num3 * this.syasen_maxt_ >= this.syasen_after_delay)
				{
					this.Asyasen.Clear();
					return;
				}
				this.need_mesh_update = true;
			}
		}

		public float syasen_maxt
		{
			get
			{
				return this.syasen_maxt_;
			}
			set
			{
				if (this.syasen_maxt == value)
				{
					return;
				}
				this.syasen_maxt_ = value;
				if (this.t >= 1f)
				{
					this.t = 1f;
					this.redraw_flag = true;
				}
			}
		}

		public float syasen_startt
		{
			get
			{
				return this.syasen_startt_;
			}
			set
			{
				if (this.syasen_startt == value)
				{
					return;
				}
				this.syasen_startt_ = value;
				if (this.t >= 1f)
				{
					this.t = 1f;
					this.redraw_flag = true;
				}
			}
		}

		public float stroke_t_px
		{
			get
			{
				return this.stroke_t_px_;
			}
			set
			{
				if (this.stroke_t_px == value)
				{
					return;
				}
				this.stroke_t_px_ = value;
				if (this.t >= 1f)
				{
					this.t = 1f;
					this.redraw_flag = true;
				}
			}
		}

		public uint disable_line_bits
		{
			get
			{
				return this.disable_line_bits_;
			}
			set
			{
				if (this.disable_line_bits == value)
				{
					return;
				}
				this.disable_line_bits_ = value;
				if (this.t >= 1f)
				{
					this.t = 1f;
					this.redraw_flag = true;
				}
			}
		}

		public uint immediate_line_bits
		{
			get
			{
				return this.immediate_line_bits_;
			}
			set
			{
				if (this.immediate_line_bits == value)
				{
					return;
				}
				this.immediate_line_bits_ = value;
				if (this.t >= 1f)
				{
					this.t = 1f;
					this.redraw_flag = true;
				}
			}
		}

		public override TextRenderer Alpha(float tz)
		{
			base.Alpha(tz);
			if (this.MaLine != null && !this.redraw_flag)
			{
				this.MaLine.setAlpha1(tz * (float)this.Style.MyCol.a / 255f, false);
			}
			return this;
		}

		private float syasen_maxt_;

		private float syasen_startt_;

		public float syasen_after_delay = 15f;

		private float stroke_t_px_ = 2f;

		public string snd_syasen_start = "quest_track_quit";

		private bool snd_played;

		private uint disable_line_bits_;

		private uint immediate_line_bits_;

		private MdArranger MaLine;

		private List<Vector2> Asyasen;
	}
}
