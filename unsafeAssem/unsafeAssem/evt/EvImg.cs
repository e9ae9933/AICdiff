using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class EvImg : IIdvName
	{
		public EvImg(string _key, PxlFrame _PF)
		{
			this.key = _key;
			this.PF = _PF;
			this.swidth_px = (float)this.PF.pPose.width;
			this.sheight_px = (float)this.PF.pPose.height;
		}

		public void RotaGraphTo(MeshDrawer Md, float x, float y, float z = 1f, float agR = 0f, bool flip = false)
		{
		}

		public EvImg destruct()
		{
			return null;
		}

		public string get_individual_key()
		{
			return this.key;
		}

		public bool isSspBuffer(EvImgContainer Con)
		{
			return Con.isSspBuffer(this.PF.pPose);
		}

		public bool getFacePosition(EvImgContainer Con, out Vector2 Out, string pos_id, float shift_y = 0f)
		{
			return Con.getFacePosition(this.PF.pPose, out Out, pos_id, shift_y);
		}

		public int buffer_w
		{
			get
			{
				return this.PF.pPose.width;
			}
		}

		public int buffer_h
		{
			get
			{
				return this.PF.pPose.height;
			}
		}

		public bool checkTermReplace(EvImgContainer Con, ref string pckey)
		{
			EvPerson.EmotReplaceTerm[] replaceTerm = Con.getReplaceTerm(this.PF.pPose, true);
			if (replaceTerm == null)
			{
				return false;
			}
			bool flag = false;
			string text = null;
			string text2 = null;
			for (int i = replaceTerm.Length - 1; i >= 0; i--)
			{
				EvPerson.EmotReplaceTerm emotReplaceTerm = replaceTerm[i];
				if (text == null)
				{
					if (!REG.match(pckey, EvImgContainer.RegPicDir))
					{
						return false;
					}
					text = REG.R1;
					text2 = REG.rightContext;
				}
				if (emotReplaceTerm.useable && REG.match(text2, emotReplaceTerm.check))
				{
					text2 = REG.ReplaceExpression(emotReplaceTerm.repl);
					flag = true;
				}
			}
			if (flag)
			{
				pckey = text + text2;
			}
			return flag;
		}

		public readonly string key;

		public readonly PxlFrame PF;

		public readonly float swidth_px;

		public readonly float sheight_px;
	}
}
