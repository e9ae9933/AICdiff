using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class EvImgContainer
	{
		public EvImgContainer(string _key)
		{
			this.key = _key;
			this.OPc = new BDic<string, EvImg>();
			this.OPi = new BDic<PxlPose, EvImgContainer.PoseInfo>();
			this.AImgs = new List<EvImg>();
			this.OOPiFacePos = new BDic<PxlPose, BDic<string, Vector2>>(1);
		}

		public int cacheReadFor(string pckey)
		{
			EvImg pic = this.getPic(pckey, true, true);
			if (pic == null)
			{
				X.de("不明な EvImg :" + pckey, null);
				return 0;
			}
			if (this.cacheReadFor(pic) == null)
			{
				return 0;
			}
			return 2;
		}

		public EvPerson.EvPxlsLoader cacheReadFor(EvImg _Img)
		{
			if (_Img.PF.TxUsing == null)
			{
				EvPerson.EvPxlsLoader loaderForPxl = EV.getLoaderForPxl(_Img.PF.pChar);
				if (loaderForPxl != null)
				{
					loaderForPxl.preparePxlImage(false);
					return loaderForPxl;
				}
			}
			return null;
		}

		public EvImg getPic(string pckey, bool replace_term = true, bool no_error = true)
		{
			EvImg evImg = X.Get<string, EvImg>(this.OPc, pckey);
			if (evImg != null)
			{
				if (replace_term && evImg.checkTermReplace(this, ref pckey))
				{
					evImg = X.Get<string, EvImg>(this.OPc, pckey) ?? evImg;
				}
			}
			else if (!no_error)
			{
				X.de("不明なEvImg: " + pckey, null);
			}
			return evImg;
		}

		public EvPerson.EmotReplaceTerm[] getReplaceTerm(PxlPose P, bool check_term_cache_calced = true)
		{
			EvImgContainer.PoseInfo poseInfo;
			if (!this.OPi.TryGetValue(P, out poseInfo))
			{
				return null;
			}
			if (poseInfo.ATerm == null)
			{
				return null;
			}
			if (check_term_cache_calced && !poseInfo.term_cache_calced)
			{
				poseInfo.term_cache_calced = true;
			}
			return poseInfo.ATerm;
		}

		public void clearTermCache()
		{
			foreach (KeyValuePair<PxlPose, EvImgContainer.PoseInfo> keyValuePair in this.OPi)
			{
				EvImgContainer.PoseInfo value = keyValuePair.Value;
				if (value.term_cache_calced)
				{
					value.term_cache_calced = false;
					for (int i = value.ATerm.Length - 1; i >= 0; i--)
					{
						value.ATerm[i].term_cache = 2;
					}
				}
			}
		}

		public bool isSspBuffer(PxlPose P)
		{
			EvImgContainer.PoseInfo poseInfo;
			return this.OPi.TryGetValue(P, out poseInfo) && poseInfo.ssp_buffer;
		}

		public bool getFacePosition(PxlPose P, out Vector2 Out, string pos_id, float shift_y = 0f)
		{
			BDic<string, Vector2> bdic;
			if (this.OOPiFacePos.TryGetValue(P, out bdic) && bdic.TryGetValue(pos_id, out Out))
			{
				return true;
			}
			Out = Vector2.zero;
			return false;
		}

		public EvImgContainer destruct()
		{
			return null;
		}

		internal void initPx(EvPerson.EvPxlsLoader[] APxls)
		{
			if (APxls == null || this.loaded)
			{
				return;
			}
			this.loaded = true;
			int num = APxls.Length;
			for (int i = 0; i < num; i++)
			{
				PxlCharacter pc = APxls[i].Pc;
				if (pc != null)
				{
					string person_key = APxls[i].person_key;
					if (person_key != "")
					{
						EvPerson person = EvPerson.getPerson(person_key, null);
						if (person == null)
						{
							X.de("EvImgContainer:initPx :: 不明な人物 " + person_key, null);
						}
						else
						{
							person.initPxEmot(pc, true);
						}
					}
					else
					{
						for (int j = pc.countPoses() - 1; j >= 0; j--)
						{
							PxlPose pose = pc.getPose(j);
							if (pose.title.IndexOf("_") != 0 || !(pose.title != "_") || (pose.title.IndexOf("_emo_") == 0 && !(pose.title == "_") && pose.title.IndexOf("__") != 0))
							{
								this.getPxSprite(pose, 0, pose.title + "/");
							}
						}
					}
				}
			}
		}

		private void getPxSprite(PxlPose P, int margin = 0, string prefix = "")
		{
			uint num = 8U;
			int count = this.AImgs.Count;
			BDic<string, Vector2> bdic = null;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					this.getPxSprite(P.getSequence(num2), ref bdic, margin, prefix);
				}
				num2++;
			}
			EvPerson.EmotReplaceTerm[] array = null;
			if (TX.valid(P.comment) && this.AImgs.Count > count)
			{
				bool flag = false;
				bool flag2 = false;
				CsvReader csvReader = new CsvReader(P.comment, CsvReader.RegSpace, false);
				while (csvReader.read())
				{
					if (csvReader.cmd == "auto_replace")
					{
						flag = true;
						string text = csvReader.slice_join(3, " ", "");
						EvPerson.EmotReplaceTerm emotReplaceTerm = new EvPerson.EmotReplaceTerm(P, csvReader._1, csvReader._2, text);
						X.push<EvPerson.EmotReplaceTerm>(ref array, emotReplaceTerm, -1);
					}
					else if (csvReader.cmd == "ssp_buffer")
					{
						flag2 = (flag = true);
					}
				}
				if (flag)
				{
					this.OPi[P] = new EvImgContainer.PoseInfo
					{
						ATerm = array,
						ssp_buffer = flag2
					};
				}
			}
			if (bdic != null)
			{
				this.OOPiFacePos[P] = bdic;
			}
		}

		private void getPxSprite(PxlSequence P, ref BDic<string, Vector2> OFacePos, int margin = 0, string prefix = "")
		{
			int num = P.countFrames();
			for (int i = 0; i < num; i++)
			{
				PxlFrame frame = P.getFrame(i);
				string text = ((frame.name == "") ? frame.getLayer(0).name : frame.name);
				if (TX.isStart(text, "__face", 0))
				{
					int num2 = frame.countLayers();
					bool flag = false;
					for (int j = 0; j < num2; j++)
					{
						PxlLayer layer = frame.getLayer(j);
						text = layer.name;
						if (TX.isStart(text, "_point_", 0))
						{
							if (OFacePos == null)
							{
								OFacePos = new BDic<string, Vector2>(1);
							}
							OFacePos[TX.slice(text, "_point_".Length)] = new Vector2(layer.x, -layer.y);
							flag = true;
						}
					}
					if (!flag)
					{
						X.de("__faceのフレーム内に _point_ で開始するレイヤーが存在しませんでした。", null);
					}
				}
				else
				{
					string text2 = (TX.noe(prefix) ? text : (prefix + text));
					if (this.OPc.ContainsKey(text2))
					{
						X.de(string.Concat(new string[]
						{
							"EvImg の設定に名前重複あり: ",
							text2,
							" @(",
							frame.ToString(),
							")"
						}), null);
					}
					else
					{
						EvImg evImg = (this.OPc[text2] = new EvImg(text2, frame));
						this.AImgs.Add(evImg);
					}
				}
			}
		}

		public List<EvImg> getImageList()
		{
			return this.AImgs;
		}

		public string key;

		protected BDic<string, EvImg> OPc;

		protected BDic<PxlPose, EvImgContainer.PoseInfo> OPi;

		protected BDic<PxlPose, BDic<string, Vector2>> OOPiFacePos;

		protected List<EvImg> AImgs;

		private bool loaded;

		public static readonly Regex RegPicDir = new Regex("^([^\\/]+\\/)");

		public const string frm_header_face_pos = "__face";

		public const string lay_header_point = "_point_";

		public class PoseInfo
		{
			public EvPerson.EmotReplaceTerm[] ATerm;

			public bool ssp_buffer;

			public bool term_cache_calced;
		}
	}
}
