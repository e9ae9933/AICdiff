using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class MsgChar
	{
		public MsgChar(MsgLine _Line, MeshDrawer _Md, char _chr, float _msx, float _msy, Color32 _color, int _index = 0, int _showt = 0, EMOT _emo = EMOT.NORMAL, float zoom = 0f, PxlMeshDrawer _pImg = null, int mesh_width = 0)
		{
			this.Line = _Line;
			this.Md = _Md;
			this.pImg = _pImg;
			this.index = _index;
			this.emotion = _emo;
			this.Col = _color;
			this.showt = -_showt - 1;
			this.msx = _msx;
			this.msy = _msy;
			float num = 1f;
			if (zoom != 0f)
			{
				num = ((zoom > 0f) ? 2f : 0.8235294f);
			}
			this.character_scale = num;
			if (_pImg == null)
			{
				this.is_small = TX.is_small(_chr.ToString());
				this.chr = _chr;
				if (this.is_small)
				{
					FontStorage storage = MsgLine.Storage;
					CharacterInfo characterInfo;
					storage.TargetFont.Target.GetCharacterInfo(this.chr, out characterInfo);
					float num2;
					this.sizew = ((float)(characterInfo.maxX - characterInfo.minX - 2) + (float)storage.margin * storage.getLetterSpaceRatio(this.chr, false, out num2)) * num;
				}
				else
				{
					this.sizew = 18f * num;
					if (_Line != null && _Line.pre_is_small)
					{
						this.sizew += 5f * num;
						this.msx += 5f * num;
					}
				}
			}
			else
			{
				this.sizew = (float)mesh_width;
			}
			this.x = this.msx;
			this.y = this.msy;
			if (this.emotion == EMOT.PLEASURE || this.emotion == EMOT.FADEIN)
			{
				this.alpha = 0f;
			}
		}

		public bool runDraw(int fcnt = 1, bool can_draw = true)
		{
			this.showt += fcnt;
			if (this.showt < 0)
			{
				return false;
			}
			bool flag = !this.visible;
			bool flag2 = false;
			if (X.D && can_draw)
			{
				flag2 = true;
				if (this.emotion == EMOT.PLEASURE)
				{
					if (this.showt <= 12)
					{
						float num = X.ZLINE((float)this.showt, 5f);
						this.alpha = num;
						this.scaleX = (this.scaleY = 2.4f - 1.4f * num);
						this.x = this.msx - 15f * (1f - num);
						this.y = this.msy - 15f * (1f - num);
					}
					else
					{
						flag2 = false;
					}
				}
				else if (this.emotion == EMOT.JOY)
				{
					int num2 = IN.totalframe + this.index * 2;
					this.x = this.msx + 3f * X.Cos((float)num2 / 36f * 3.1415927f * 2f);
					this.y = this.msy + 3f * X.Sin((float)num2 / 36f * 3.1415927f * 2f);
				}
				else if (this.emotion == EMOT.SCARY)
				{
					int num2 = IN.totalframe + this.index * 2;
					this.x = this.msx + 1f * X.Cos((float)num2 / 0.27f * 3.1415927f * 2f);
					this.y = this.msy + 1f * X.Sin((float)num2 / 0.18f * 3.1415927f * 2f);
				}
				else if (this.emotion == EMOT.CRY)
				{
					int num2 = IN.totalframe + this.index * 2;
					this.y = this.msy + (1.5f + 1.9f * X.Sin(((float)num2 + 9.8f * (float)this.index) / 17f * 3.1415927f * 2f));
				}
				else if (this.emotion == EMOT.FADEIN)
				{
					float num = X.ZLINE((float)this.showt / 20f);
					this.alpha = num;
				}
				else if (this.emotion == EMOT.VIB)
				{
					int num2 = IN.totalframe;
					this.y = this.msy + 3f * X.Sin((float)num2 / 36f * 3.1415927f * 2f);
				}
				else
				{
					flag2 = false;
				}
				if (flag)
				{
					flag2 = (this.visible = true);
					if (this.pImg == null && this.chr != ' ' && this.chr != '\u3000' && this.chr != '.' && this.chr != '・')
					{
						this.Line.talkProgressSound();
					}
				}
			}
			if (flag2 || (this.mesh_remake_flag && this.visible))
			{
				this.remakeMesh();
			}
			return flag2;
		}

		public void remakeMesh()
		{
			this.mesh_remake_flag = false;
			bool flag = false;
			int num = 0;
			int num2 = 0;
			if (this.Mda == null)
			{
				this.Mda = new MdArranger(this.Md);
				flag = true;
				this.Mda.Set(true);
			}
			else
			{
				this.Mda.revertVerAndTriIndex(ref num, ref num2);
			}
			this.Md.Col = MTRX.cola.Set(this.Col).setA1(this.alpha * this.Line.char_alpha).C;
			if (this.pImg == null)
			{
				FontStorage storage = MsgLine.Storage;
				CharacterInfo characterInfo;
				storage.TargetFont.Target.GetCharacterInfo(this.chr, out characterInfo);
				float num3 = (float)(characterInfo.maxX - characterInfo.minX) * this.scaleX;
				this.Md.BoxCharacterInfo(this.x - num3 / 2f, this.y + 2f - (float)storage.TargetFont.fontSize * 0.5f * this.scaleY, this.scaleX, this.scaleY, this.chr, storage, characterInfo, false, null);
				this.Line.remake_mesh_flag |= 1U;
			}
			else
			{
				this.Md.DrawScaleMesh(this.x, this.y, this.scaleX, this.scaleY, this.pImg);
				this.Line.remake_mesh_flag |= 2U;
			}
			if (flag)
			{
				this.Mda.Set(false);
				return;
			}
			this.Mda.revertVerAndTriIndexAfter(num, num2, false);
		}

		public void showImmediate()
		{
			this.showt = 9999;
			this.runDraw(1, true);
		}

		public void timePlus(int pt = 0)
		{
			this.showt += pt;
		}

		public readonly bool is_small;

		private float x;

		private float y;

		private MsgLine Line;

		public int maxt = 1;

		public int showt = 1;

		private int index;

		private char chr;

		private Color32 Col;

		public float msx;

		public float msy;

		public float sizew;

		private float character_scale;

		public bool visible;

		private EMOT emotion;

		private float alpha = 1f;

		private float scaleX = 1f;

		private float scaleY = 1f;

		public bool mesh_remake_flag;

		private MeshDrawer Md;

		private MdArranger Mda;

		private PxlMeshDrawer pImg;
	}
}
