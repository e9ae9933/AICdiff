using System;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class EvEmotVisibility
	{
		public EvEmotVisibility(string _name, PxlFrame _F, uint _lay_vis_bit, uint _lay_emo_bit, EvPerson.EmotLayer[] _AEmot, EvPerson.EmotImportPose[] _AEmotPose, BDic<int, uint> _Oface_bits, bool _dont_appear_on_editor)
		{
			this.name = _name;
			this.SourceF = _F;
			this.swidth_px = (float)this.SourceF.pPose.width;
			this.sheight_px = (float)this.SourceF.pPose.height;
			this.lay_vis_bit = _lay_vis_bit;
			this.lay_emo_bit = _lay_emo_bit;
			this.AEmotions = _AEmot;
			this.AEmotPose = _AEmotPose;
			this.Oface_bits = _Oface_bits;
			this.dont_appear_on_editor = _dont_appear_on_editor;
			this.shift_y = 120f;
			if (this.AEmotions != null)
			{
				int num = this.SourceF.countLayers();
				for (int i = 0; i < num; i++)
				{
					if (((ulong)this.lay_emo_bit & (ulong)(1L << (i & 31))) != 0UL)
					{
						this.face_y = -this.SourceF.getLayer(i).y;
						return;
					}
				}
			}
		}

		public void drawTo(MeshDrawer Md, float x, float y, float scale, string cur_emotion, bool no_error = false)
		{
			int num = this.SourceF.countLayers();
			y = (float)((int)y);
			uint num2 = this.lay_vis_bit;
			if (this.Oface_bits != null && REG.match(cur_emotion, EvPerson.RegFacePrefix))
			{
				string rightContext = REG.rightContext;
				int num3 = X.NmI(REG.R1, 0, false, false);
				uint num4;
				if (this.Oface_bits.TryGetValue(num3, out num4))
				{
					num2 |= num4;
				}
			}
			int num5 = 0;
			this.last_emo_drawn = -1;
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = this.SourceF.getLayer(i);
				PxlLayer pxlLayer = layer;
				PxlFrame pxlFrame = null;
				EvPerson.EmotImportPose emotImportPose = null;
				if (((ulong)num2 & (ulong)(1L << (int)((byte)i & 31))) == 0UL)
				{
					pxlLayer = null;
				}
				if (((ulong)this.lay_emo_bit & (ulong)(1L << (int)((byte)i & 31))) != 0UL && this.AEmotPose != null)
				{
					EvPerson.EmotImportPose emotImportPose2 = this.AEmotPose[num5++];
					if (pxlLayer != null)
					{
						EvPerson.EmotLayer emotInfoByName = this.getEmotInfoByName(cur_emotion);
						if (emotInfoByName != null && emotInfoByName.F.pPose == emotImportPose2.Pose)
						{
							pxlFrame = emotInfoByName.F;
							emotImportPose = emotImportPose2;
						}
						pxlLayer = null;
					}
				}
				if (pxlLayer != null || pxlFrame != null)
				{
					if (layer.alpha > 0f)
					{
						Md.Col = C32.MulA(MTRX.ColWhite, layer.alpha / 100f);
					}
					else
					{
						Md.Col = MTRX.ColWhite;
					}
					Md.Identity();
					if (pxlLayer != null)
					{
						int width = pxlLayer.Img.width;
						int height = pxlLayer.Img.height;
						Md.TranslateP(x, y, true).Scale(scale, scale, true);
						Md.RotaL(0f, 0f, layer, false, false, 0);
					}
					else
					{
						float num6 = layer.zmx * scale;
						float num7 = layer.zmy * scale;
						Md.RotaPF(x + layer.x * scale + num6 * emotImportPose.shiftx, y - layer.y * scale + num6 * emotImportPose.shifty, num6, num7, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
						this.last_emo_drawn = i;
					}
				}
			}
		}

		public EvPerson.EmotLayer getEmotInfoByName(string s)
		{
			int emotInfoIndexByName = this.getEmotInfoIndexByName(s);
			if (emotInfoIndexByName < 0)
			{
				return null;
			}
			return this.AEmotions[emotInfoIndexByName];
		}

		public int getEmotInfoIndexByName(string s)
		{
			if (this.AEmotions == null)
			{
				return -1;
			}
			for (int i = this.AEmotions.Length - 1; i >= 0; i--)
			{
				if (this.AEmotions[i].key == s)
				{
					return i;
				}
			}
			return -1;
		}

		public EvPerson.EmotLayer[] getFaceEmotionArray()
		{
			return this.AEmotions;
		}

		public int getEmotByFace(PxlFrame FaceF)
		{
			for (int i = this.AEmotions.Length - 1; i >= 0; i--)
			{
				if (this.AEmotions[i].F == FaceF)
				{
					return i;
				}
			}
			return -1;
		}

		public Texture texture
		{
			get
			{
				return this.SourceF.getImageTexture();
			}
		}

		public Vector2 getMouthPos()
		{
			return this.getFacePos(-20f);
		}

		public Vector2 getFacePos(float shift_y = 0f)
		{
			if (this.last_emo_drawn < 0)
			{
				return new Vector2(0f, (float)this.SourceF.pPose.height * this.draw_scale * 0.2f);
			}
			PxlLayer layer = this.SourceF.getLayer(this.last_emo_drawn);
			EvPerson.EmotImportPose emotImportPose = this.AEmotPose[this.AEmotPose.Length - 1];
			return new Vector2((layer.x + emotImportPose.shiftx * layer.zmx) * this.draw_scale, (-layer.y + emotImportPose.shifty * layer.zmy) * this.draw_scale + shift_y);
		}

		public Vector2 getFaceShift(float shift_y = 0f)
		{
			if (this.last_emo_drawn < 0)
			{
				return Vector2.zero;
			}
			EvPerson.EmotImportPose emotImportPose = this.AEmotPose[this.AEmotPose.Length - 1];
			return new Vector2(emotImportPose.shiftx, emotImportPose.shifty);
		}

		public string emot_pose_listup
		{
			get
			{
				if (this.AEmotPose == null)
				{
					return "";
				}
				int num = this.AEmotPose.Length;
				string text = "";
				for (int i = 0; i < num; i++)
				{
					text = text + ((i > 0) ? "," : "") + this.AEmotPose[i].Pose.ToString();
				}
				return this.AEmotPose[0].Pose.pChar.title + "#" + text;
			}
		}

		public PxlPose SoursePose
		{
			get
			{
				return this.SourceF.pPose;
			}
		}

		public float get_draw_y(float scl = 1f)
		{
			return -this.face_y * this.draw_scale / scl + this.shift_y;
		}

		public float editor_swidth_px
		{
			get
			{
				return (float)this.SourceF.pSq.width;
			}
		}

		public float editor_sheight_px
		{
			get
			{
				return (float)this.SourceF.pSq.height;
			}
		}

		public readonly string name;

		public readonly PxlFrame SourceF;

		public readonly float swidth_px;

		public readonly float sheight_px;

		private uint lay_vis_bit;

		private uint lay_emo_bit;

		public float draw_scale = 1f;

		public readonly BDic<int, uint> Oface_bits;

		public float face_y;

		public float shift_y;

		public float shift_x;

		public float shift_x_onright;

		private int last_emo_drawn = -1;

		public bool dont_appear_on_editor;

		private EvPerson.EmotLayer[] AEmotions;

		private EvPerson.EmotImportPose[] AEmotPose;
	}
}
