using System;
using UnityEngine;
using XX;

namespace evt
{
	public class MsgTalkerViewer
	{
		public MsgTalkerViewer(MSG _Msg)
		{
			this.Msg = _Msg;
			MultiMeshRenderer mmrd = this.Msg.get_MMRD();
			this.MdFrm = mmrd.Make(BLEND.NORMAL, MTRX.MIicon);
			this.MdBox = mmrd.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
			this.Gob = mmrd.GetGob(this.MdBox);
			this.Trs = this.Gob.transform;
			mmrd.GetGob(this.MdFrm).transform.SetParent(this.Trs, false);
			this.MrdBox = mmrd.GetMeshRenderer(this.MdBox);
			this.MrdFrm = mmrd.GetMeshRenderer(this.MdFrm);
			this.Tx = IN.CreateGob(this.Gob, "-tx").AddComponent<TextRenderer>().Size(18f)
				.Align(ALIGN.CENTER)
				.AlignY(ALIGNY.BOTTOM);
		}

		public MsgTalkerViewer activate()
		{
			this.MrdBox.enabled = true;
			this.MrdFrm.enabled = true;
			this.Tx.enabled = true;
			return this;
		}

		public MsgTalkerViewer deactivate()
		{
			if (this.t > -this.T_FADEOUT)
			{
				this.t = X.Mn(this.t, -1);
				this.talker_name = "";
			}
			return this;
		}

		public MsgTalkerViewer showImmediate()
		{
			if (this.t < 0)
			{
				return this.hideImmediate();
			}
			if (this.t < 22)
			{
				this.t = 22;
				this.fineZ(1f);
			}
			return this;
		}

		public MsgTalkerViewer hideImmediate()
		{
			this.MrdBox.enabled = false;
			this.MrdFrm.enabled = false;
			this.Tx.enabled = false;
			this.fine_flag = false;
			this.t = -this.T_FADEOUT;
			return this;
		}

		public bool setTalker(string s, float _basex)
		{
			this.talker_key = s;
			EvPerson evPerson = (TX.noe(s) ? null : EvPerson.getPerson(s, null));
			if (evPerson == null)
			{
				this.t = X.Mn(this.t, -1);
				this.talker_name = "";
			}
			else
			{
				this.t = ((this.talker_name != evPerson.talker_name) ? 0 : X.Mx(this.t, 0));
				this.basex = _basex;
				this.talker_name = evPerson.talker_name;
				if (this.talker_name != "")
				{
					this.Tx.Txt(this.talker_name);
				}
				this.Trs.eulerAngles = new Vector3(0f, 0f, (float)(2 * X.MPF(this.is_left)));
			}
			this.fine_flag = true;
			if (this.t > -this.T_FADEOUT)
			{
				this.activate();
			}
			this.runDraw(true);
			return true;
		}

		public bool needChange(string s)
		{
			EvPerson evPerson = (TX.noe(s) ? null : EvPerson.getPerson(s, null));
			return ((evPerson == null) ? this.talker_name : evPerson.talker_name) != this.talker_name;
		}

		public void runDraw(bool draw_flag)
		{
			if (this.t >= 0)
			{
				if ((draw_flag && this.t <= 30) || this.fine_flag)
				{
					this.fineZ(X.ZSIN((float)this.t, 22f));
				}
				this.t++;
				return;
			}
			if (this.t > -this.T_FADEOUT)
			{
				if (draw_flag)
				{
					this.fineZ(X.ZSINV((float)(this.T_FADEOUT + this.t), (float)this.T_FADEOUT));
				}
				int num = this.t - 1;
				this.t = num;
				if (num <= -this.T_FADEOUT)
				{
					this.hideImmediate();
				}
			}
		}

		private void fineZ(float z)
		{
			this.fine_flag = false;
			float num = X.NAIBUN_I(36f, 160f, z);
			float num2 = this.Msg.alpha * z;
			this.MdBox.Col = MTRX.cola.Set(MSG.AmsgColors[0]).setA1(num2).C;
			this.MdBox.KadomaruRect(0f, 0f, num, 36f, 18f, 0f, false, 0f, 0f, false);
			float num3 = X.NAIBUN_I(64f, 158f, z);
			this.MdFrm.Col = MTRX.cola.Set(this.Msg.ColTop).setA1(num2).C;
			MTRX.TalkerFrame3.DrawTo(this.MdFrm, 0f, 0f, num3, 36f);
			this.MdBox.updateForMeshRenderer(false);
			this.MdFrm.updateForMeshRenderer(false);
			this.Tx.Col(MTRX.cola.Set(MSG.AmsgColors[8]).setA1(num2).C);
			float num4 = ((this.t < 0) ? 1f : z);
			Vector3 localPosition = this.Trs.localPosition;
			localPosition.x = (-273f + 30f * (1f - num4)) * (float)X.MPF(this.is_left) * 0.015625f;
			localPosition.y = (float)(-(float)MSG.taleheight - 1) * 0.015625f;
			this.Trs.localPosition = localPosition;
		}

		private bool is_left
		{
			get
			{
				return this.basex <= 0f;
			}
		}

		private int T_FADEOUT
		{
			get
			{
				return this.Msg.hiding_time;
			}
		}

		public bool isActive()
		{
			return this.t >= 0;
		}

		private readonly MSG Msg;

		private readonly GameObject Gob;

		private readonly Transform Trs;

		private readonly MeshDrawer MdBox;

		private readonly MeshDrawer MdFrm;

		private readonly MeshRenderer MrdBox;

		private readonly MeshRenderer MrdFrm;

		private readonly TextRenderer Tx;

		private int t;

		public string talker_key = "";

		private string talker_name = "";

		private const int T_FADE = 22;

		public bool fine_flag;

		private float basex;

		private const float radius = 18f;

		private const float box_w = 160f;

		private const float box_h = 36f;

		private const float box_wh = 80f;
	}
}
