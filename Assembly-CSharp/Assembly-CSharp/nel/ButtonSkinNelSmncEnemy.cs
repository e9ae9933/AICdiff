using System;
using nel.mgm.smncr;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinNelSmncEnemy : ButtonSkinNelUi
	{
		public ButtonSkinNelSmncEnemy(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			base.fix_text_size = 12f;
		}

		public void initNormal(string k)
		{
			this.EE = null;
			this.setTitle(" ");
			this.B.SetLocked(false, true, false);
			this.row_right_px = 4;
		}

		internal void initEnemy(UiSmncEnemyEditor _Con, EnemyEntry _EE)
		{
			this.EE = _EE;
			this.Con = _Con;
			this.B.setSkinTitle(this.EE.hidden ? "???" : " ");
			this.B.SetLocked(this.EE.hidden, true, false);
			this.EE.Sk = this;
			this.row_right_px = 20;
			MeshDrawer mdEf = this.MdEf;
		}

		public bool is_enemy
		{
			get
			{
				return this.EE != null;
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			base.RowFineAfter(w, h);
			bool flag = this.animate_t >= 0f;
			if (this.is_enemy)
			{
				float num = 0f;
				float num2 = 1f;
				float num3 = w * 0.5f;
				int num4 = 0;
				float num5;
				if (!this.EE.locked)
				{
					num = 1f;
					num5 = 1f;
				}
				else
				{
					num5 = (float)((this.EE.unlock == UiSmncEnemyEditor.UNLK.LOCKED_UNLOCKABLE) ? 1 : 0);
				}
				MeshDrawer meshDrawer = this.Md;
				Color32 color = (this.Md.Col = this.Md.ColGrd.Set(this.Tx.TextColor).mulA(X.NI(0.5f, 1f, num5)).C);
				if (this.animate_t >= 0f)
				{
					float animate_tz = this.animate_tz;
					meshDrawer = this.MdEf;
					float num6 = X.ZLINE(animate_tz, 0.25f) * X.ZLINE(1f - animate_tz, 0.25f);
					float num7 = 1f + 0.08f * num6;
					this.B.transform.localScale = new Vector3(num7, num7, 1f);
					color = this.MdEf.ColGrd.Set(color).blend(4278190080U, num6).C;
					num3 /= num7;
					this.MdEf.ColGrd.White();
					if (animate_tz < 0.5f)
					{
						this.MdEf.ColGrd.mulA(X.ZPOW(animate_tz, 0.5f) * (0.75f + 0.125f * X.COSIT(13.6f) + 0.125f * X.COSIT(4.33f)));
						this.MdEf.Col = this.MdEf.ColGrd.C;
						this.MdEf.Rect(0f, 0f, w, h, false);
					}
					else
					{
						num4 = 1;
						num = X.ZLINE(animate_tz - 0.5f, 0.45f) * 2f;
						for (int i = 0; i < 3; i++)
						{
							float num8 = animate_tz - 0.5f - 0.08f * (float)i;
							if (num8 < 0f)
							{
								break;
							}
							num8 = X.ZLINE(num8, 0.23f);
							if (num8 < 0.125f)
							{
								this.MdEf.Rect(0f, 0f, w, h, false);
							}
							else
							{
								float num9 = X.ZLINE(num8 - 0.125f, 0.875f);
								float num10 = 4f * (1f - num9);
								if (num10 > 0f)
								{
									float num11 = num9 * 50f;
									this.MdEf.Box(0f, 0f, w + num11, h + num11, num10, false);
								}
							}
						}
					}
				}
				meshDrawer.Col = C32.MulA(color, this.alpha_);
				Shape.DrawMeshIcon(this.Md, num3 - 12f, 0f, 13f * num2, num4, num);
				if (this.animate_tz >= 1f && this.animate_t > 0f)
				{
					this.animate_t = -1f;
					this.B.transform.localScale = new Vector3(1f, 1f, 1f);
				}
				if (this.EE.available_enemy && this.EE.DData.smnc_unlocked_new)
				{
					this.Md.Col = C32.MulA(4294926244U, this.alpha_ * 1f);
					this.Md.Circle(num3 - h * 0.42f - 18f, 0f, 3f, 0f, false, 0f, 0f);
				}
			}
			if (this.MdEf != null && flag)
			{
				this.MdEf.updateForMeshRenderer(false);
			}
		}

		public void UnlockAnimate()
		{
			this.animate_t = 0f;
			this.fine_flag = true;
			if (this.MdEf == null)
			{
				this.MdEf = base.makeMesh(MTRX.MtrMeshNormal);
				this.ValotEf = IN.GetOrAdd<ValotileRenderer>(this.MMRD.GetGob(this.MdEf));
			}
			IN.setZAbs(this.ValotEf.transform, -9.45f);
		}

		public bool runUnlockAnimate(float fcnt, out bool progress_unlocked)
		{
			progress_unlocked = false;
			if (fcnt == 0f)
			{
				return true;
			}
			if (this.animate_t == -1f)
			{
				return false;
			}
			this.fine_flag = true;
			if (this.animate_t == 0f)
			{
				SND.Ui.play("garage_enemy_unlock_charge", false);
			}
			int num = ((this.animate_t >= 70f) ? 1 : 0);
			this.animate_t += fcnt;
			bool flag = this.animate_t >= 70f;
			if (num == 0 && flag)
			{
				SND.Ui.play("recipe_fullfill", false);
				SND.Ui.play("unlock_dial", false);
				progress_unlocked = true;
			}
			return true;
		}

		public void hoverHideNewIcon()
		{
			if (this.is_enemy && this.EE.DData.smnc_unlocked_new)
			{
				this.newicon_hide_frame = IN.totalframe;
			}
		}

		public void blurHideNewIcon()
		{
			if (this.is_enemy && this.newicon_hide_frame > 0 && IN.totalframe - this.newicon_hide_frame >= 40)
			{
				this.EE.DData.smnc_unlocked_new = false;
				if (this.Con != null)
				{
					this.Con.need_fine_dex_list = true;
				}
				UiEnemyDex.Write(this.EE.DData);
				this.fine_flag = true;
			}
			this.newicon_hide_frame = 0;
		}

		public float animate_tz
		{
			get
			{
				if (this.animate_t < 0f)
				{
					return 1f;
				}
				return X.ZLINE(this.animate_t, 140f);
			}
		}

		private EnemyEntry EE;

		private float animate_t = -1f;

		private MeshDrawer MdEf;

		private ValotileRenderer ValotEf;

		private const float ANIMATE_MAXT = 140f;

		private int newicon_hide_frame;

		private UiSmncEnemyEditor Con;
	}
}
