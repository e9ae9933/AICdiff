using System;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class ButtonSkinDJRow : ButtonSkinNelUi
	{
		public ButtonSkinDJRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.TxR = base.MakeTx("-text_r");
			this.TxR.stencil_ref = _B.container_stencil_ref;
			this.TxR.html_mode = true;
			this.TxR.max_swidth_px = 70f;
			this.TxR.alignx = ALIGN.LEFT;
			this.TxR.aligny = ALIGNY.MIDDLE;
			base.fix_text_size = 16f;
			this.TxR.size = base.fix_text_size;
		}

		protected void repositRightText()
		{
			if (this.price < 0)
			{
				this.TxR.gameObject.SetActive(false);
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add("<img mesh=\"", CoinStorage.icon_key(CoinStorage.CTYPE.GOLD), "\" width=\"30\" />");
				stb.spr0(this.price, 3, ' ');
				this.TxR.Txt(stb);
			}
			Vector3 localPosition = this.TxR.transform.localPosition;
			localPosition.x = this.w / 2f - (this.right_shift_pixel + this.TxR.get_swidth_px()) * 0.015625f;
			this.TxR.transform.localPosition = localPosition;
		}

		public void initSkill(MgmDojo DJ)
		{
			this.already_learn = false;
			this.price = -1;
			this.Sk = null;
			this.row_left_px = 32;
			this.tx_col_normal = 4283780170U;
			this.grade = -1;
			if (this.B.title == "_tuto")
			{
				this.price = 0;
				this.setTitleText(TX.Get("Mgm_dojo_tutorial", ""));
			}
			else
			{
				this.Sk = SkillManager.Get(this.B.title);
				DjHkdsGenerator gen = DJ.GM.HK.GetGen(this.B.title);
				DjSaveData.SD data = COOK.Mgm.Dojo.GetData(this.B.title);
				if (this.Sk != null)
				{
					this.already_learn = this.Sk.visible;
					this.price = (this.already_learn ? 5 : ((data.play_count > 0) ? 15 : 300));
					this.setTitleText(this.Sk.title);
					this.tx_col_normal = (this.already_learn ? 4285095516U : 4283780170U);
					bool flag = this.already_learn;
				}
				if (gen != null)
				{
					this.grade = gen.grade;
					base.prepareIconMesh();
				}
			}
			if (this.price < 0)
			{
				this.setTitleText("???");
				this.B.SetLocked(true, true, false);
			}
			else if ((long)this.price > (long)((ulong)CoinStorage.getCount(CoinStorage.CTYPE.GOLD)))
			{
				this.B.SetLocked(true, true, false);
			}
			else
			{
				this.B.SetLocked(false, true, false);
			}
			if (this.grade < 0 && this.MdIco != null)
			{
				this.MdIco.clear(false, false);
			}
			this.fine_flag = true;
			this.repositRightText();
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			base.Fine();
			this.TxR.TextColor = this.Tx.TextColor;
			return this;
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.grade >= 0)
			{
				this.MdIco.Col = this.Tx.TextColor;
				this.MdIco.RotaPF(w * 0.5f - 45f - this.TxR.get_swidth_px(), 0f, 1f, 1f, 0f, MTR.AItemGradeStars[this.grade + 5], false, false, false, uint.MaxValue, false, 0);
			}
			if (this.already_learn)
			{
				this.Md.Col = this.Tx.TextColor;
				this.Md.CheckMark(-w * 0.5f + h * 0.5f, 0f, h * 0.8f, 4f, false);
			}
			base.RowFineAfter(w, h);
		}

		public override float alpha
		{
			set
			{
				base.alpha = value;
				this.TxR.alpha = value;
			}
		}

		public float right_shift_pixel = 22f;

		public int price;

		public bool already_learn;

		public PrSkill Sk;

		public int grade = -1;

		protected TextRenderer TxR;
	}
}
