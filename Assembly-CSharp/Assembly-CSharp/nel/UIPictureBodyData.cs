using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public class UIPictureBodyData : IMosaicDescriptor
	{
		public Vector3 PosSyncSlide
		{
			get
			{
				return this.PCon.PosSyncSlide;
			}
		}

		public UIPictureBodyData(string _name, UIPictureBase _PCon, PxlLayer _BaseLay)
		{
			this.BaseLay = _BaseLay;
			this.name = _name;
			this.PCon = _PCon;
			this.APos = new List<Vector2>(4);
			this.APos.Add(new Vector2(0f, -200f));
			this.APos.Add(new Vector2(0f, -110f));
			this.APos.Add(new Vector2(0f, -20f));
			this.APos.Add(new Vector2(0f, 40f));
			if (UIPictureBodyData.Qu == null)
			{
				UIPictureBodyData.Qu = new Quaker(null);
				UIPictureBodyData.Ptl = new EfParticleLooper("ui_breathe");
			}
			this.FD_fnDrawBreathe = new FnEffectRun(this.fnDrawBreathe);
			if (this.BaseLay == null)
			{
				return;
			}
			int num = this.BaseLay.pFrm.countLayers();
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = this.BaseLay.pFrm.getLayer(i);
				if (layer != this.BaseLay && TX.isStart(layer.name, "add_", 0))
				{
					string text = TX.slice(layer.name, 4);
					if (REG.match(text, REG.RegSuffixNumber))
					{
						text = REG.leftContext + REG.R1;
					}
					UIPictureBase.EMSTATE_ADD emstate_ADD = UIPictureBase.EMSTATE_ADD.NORMAL;
					FEnum<UIPictureBase.EMSTATE_ADD>.TryParse(text.ToUpper(), out emstate_ADD, true);
					if (emstate_ADD != UIPictureBase.EMSTATE_ADD.NORMAL)
					{
						if (this.Alay_attr == null)
						{
							this.Alay_attr = new UIPictureBase.EMSTATE_ADD[num];
						}
						this.Alay_attr[i] = emstate_ADD;
					}
					else
					{
						X.de("不明なadd領域: '" + layer.name + "' " + layer.pFrm.ToString(), null);
					}
				}
			}
		}

		public virtual Material initEmot(UIPictureBase.EMSTATE st, MeshDrawer Md)
		{
			this.af = 0f;
			UIPictureBodyData.Qu.clear();
			this.current_state = st;
			this.current_sta = this.PCon.getAdditionalState(false);
			if (this.Has(UIPictureBodyData.ANIM.GAZE))
			{
				this.gaze_fx = X.NIXP(260f, 322f);
				this.gaze_fy = X.NIXP(100f, 222f);
			}
			if (this.Has(UIPictureBodyData.ANIM.BREATH))
			{
				if (UIPictureBodyData.EfBreathe == null && UIBase.Instance != null)
				{
					UIPictureBodyData.EfBreathe = UIBase.Instance.getEffect().setEffectWithSpecificFn("uipic_breathe", 0f, 0f, 0f, 0, X.xors(200), this.FD_fnDrawBreathe);
				}
			}
			else if (UIPictureBodyData.EfBreathe != null)
			{
				UIPictureBodyData.EfBreathe.destruct();
				UIPictureBodyData.EfBreathe = null;
			}
			if (Md != null)
			{
				Md.setMaterial(this.Mtr, false);
				this.PCon.fineMeshTarget(Md.getMesh());
				Md.initForImgAndTexture(this.texture);
			}
			return this.Mtr;
		}

		public virtual void fineMaterial(MeshDrawer Md)
		{
			Md.setMaterial(this.Mtr, false);
			this.PCon.fineMeshTarget(Md.getMesh());
			Md.initForImgAndTexture(this.texture);
		}

		public virtual void closeEmot(UIPictureBodyData NextBd)
		{
		}

		public virtual bool redraw(MeshDrawer Md, UIPictureBase.PrEmotion.BodyPaint Bp, UIPictureBase.EMSTATE st, int first, IEfPtcSetable EfSt)
		{
			PxlFrame pxlFrame = Bp.AFrm[0];
			PxlFrame pxlFrame2 = pxlFrame;
			int num = (int)(-this.shift_ux * 64f);
			int num2 = (int)(-this.shift_uy * 64f);
			int num3 = pxlFrame.countLayers();
			for (int i = 0; i < num3; i++)
			{
				PxlLayer layer = pxlFrame.getLayer(i);
				if (layer == this.BaseLay && pxlFrame2 != pxlFrame)
				{
					Md.RotaPF((float)num, (float)num2, 1f, 1f, 0f, pxlFrame2, false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					if (this.Alay_attr != null)
					{
						UIPictureBase.EMSTATE_ADD emstate_ADD = this.Alay_attr[i];
						if (emstate_ADD != UIPictureBase.EMSTATE_ADD.NORMAL)
						{
							if ((emstate_ADD & this.current_sta) == UIPictureBase.EMSTATE_ADD.NORMAL)
							{
								goto IL_0254;
							}
						}
						else if (layer.alpha == 0f)
						{
							goto IL_0254;
						}
					}
					else if (layer.alpha == 0f)
					{
						goto IL_0254;
					}
					Md.initForImg(layer.Img, 0);
					if (layer.zmx == 1f && layer.zmy == 1f && layer.rotR == 0f)
					{
						float num4 = Mathf.Round(Md.texture_width * layer.Img.RectIUv.width);
						float num5 = Mathf.Round(Md.texture_height * layer.Img.RectIUv.height);
						Md.RectBL(layer.x - (float)((int)(num4 * 0.5f)) + (float)num, -layer.y + (float)((int)(num5 * 0.5f)) - num5 + (float)num2, num4, num5, false);
					}
					else
					{
						float num4 = Md.texture_width * layer.Img.RectIUv.width * 1f;
						float num5 = Md.texture_height * layer.Img.RectIUv.height * 1f;
						float pixelsPerUnit = pxlFrame.pChar.pixelsPerUnit;
						Md.Scale(layer.zmx, layer.zmy, false);
						if (layer.rotR != 0f)
						{
							Md.Rotate(layer.rotR, false);
						}
						Md.Translate((layer.x + (float)num) * 0.015625f, (-layer.y + (float)num2) * 0.015625f, false);
						Md.RectBL((float)(0 - (int)(num4 * 0.5f)), (float)((int)(num5 * 0.5f)) - num5, num4, num5, false);
						Md.Identity();
					}
				}
				IL_0254:;
			}
			Md.updateForMeshRenderer(false);
			return true;
		}

		public virtual Material Mtr
		{
			get
			{
				return this.PCon.MtrPxl;
			}
		}

		public virtual Texture texture
		{
			get
			{
				return this.Emot.DefaultTx;
			}
		}

		public virtual Vector2 getPosition(UIPictureBodyData.POS id)
		{
			if (this.APos != null && X.BTW(0f, (float)id, (float)this.APos.Count))
			{
				Vector2 vector = this.APos[(int)id];
				return new Vector2(vector.x, vector.y);
			}
			return Vector2.zero;
		}

		public bool Has(UIPictureBodyData.ANIM a)
		{
			return (this.anim & a) > UIPictureBodyData.ANIM.FREEZE;
		}

		public virtual void run(Transform Tr, float fcnt_d, float TS, bool force = false)
		{
			float num = TS * fcnt_d;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = new Vector3(this.scale, this.scale, 1f);
			if (this.Has(UIPictureBodyData.ANIM.SCWAT))
			{
				float num2 = X.ZCOS(X.ZSIN(this.af % 95f, 95f));
				vector.y += X.ZLINE(0.5f - X.Abs(num2 - 0.5f) - 0.1f, 0.4f) * -5f;
				vector2.y *= 1f - X.ZSIN(0.5f - X.Abs(num2 - 0.5f) - 0.3f, 0.2f) * 0.0021f;
			}
			if (this.Has(UIPictureBodyData.ANIM.TIRED))
			{
				float num3 = X.ZCOS(X.ZSIN(this.af % 175f, 175f));
				vector.y += X.ZLINE(0.5f - X.Abs(num3 - 0.5f) - 0.1f, 0.4f) * -3f;
				vector2.y *= 1f - X.ZSIN(0.5f - X.Abs(num3 - 0.5f) - 0.1f, 0.4f) * 0.0004f;
			}
			if (this.Has(UIPictureBodyData.ANIM.VIB) && UIPictureBodyData.Qu.Length == 0)
			{
				UIPictureBodyData.Qu.Vib(2f, X.NIXP(4f, 14f), 1f, (int)X.NIXP(60f, 120f));
			}
			if (this.Has(UIPictureBodyData.ANIM.GAZE))
			{
				vector.x += 2f * X.COSI(this.af + 333f, this.gaze_fx);
				vector.y += 3.5f * X.COSI(this.af + 411f, this.gaze_fy);
			}
			UIPictureBodyData.Qu.run(num);
			vector.x = (float)((int)(vector.x + (this.shift_ux * this.PCon.PosTeScale.x + this.PCon.gm_shift_x + UIPictureBodyData.Qu.x) * 64f));
			vector.y = (float)((int)(vector.y + (this.shift_uy * this.PCon.PosTeScale.y + UIPictureBodyData.Qu.y) * 64f));
			if (this.PCon.ground_level > 0f)
			{
				vector.y -= 50f * X.ZPOW(this.PCon.ground_level - 0.5f, 0.5f);
			}
			else if (this.PCon.ground_level < 0f)
			{
				vector.y -= 50f * X.ZSINV(1f + this.PCon.ground_level);
			}
			vector *= 0.015625f;
			this.last_x = vector.x;
			this.last_y = vector.y;
			Tr.localPosition = vector;
			Tr.localScale = new Vector3(vector2.x * this.PCon.PosTeScale.x, vector2.y * this.PCon.PosTeScale.y, 1f);
			this.af += num;
		}

		private bool fnDrawBreathe(EffectItem Ef)
		{
			Ef.x = this.APos[3].x;
			Ef.y = this.APos[3].y - 40f;
			UIPictureBodyData.Ptl.Draw(Ef, 150f);
			return true;
		}

		public virtual float shift_ux
		{
			get
			{
				return this.PosSyncSlide.x * this.scale;
			}
		}

		public virtual float shift_uy
		{
			get
			{
				return this.PosSyncSlide.y * this.scale + UIPictureBase.base_y;
			}
		}

		public virtual float scale
		{
			get
			{
				return 1f;
			}
		}

		public float effect_shift_ux
		{
			get
			{
				return -this.shift_ux / this.scale;
			}
		}

		public float effect_shift_uy
		{
			get
			{
				return -this.shift_uy / this.scale;
			}
		}

		protected PxlImage getBaseLayImg()
		{
			if (this.BaseLay == null)
			{
				return null;
			}
			return this.BaseLay.Img;
		}

		public virtual float base_swidth
		{
			get
			{
				return (float)this.BaseLay.pFrm.pPose.width;
			}
		}

		public virtual float base_sheight
		{
			get
			{
				return (float)this.BaseLay.pFrm.pPose.height;
			}
		}

		protected virtual float base_scale
		{
			get
			{
				return 1f;
			}
		}

		protected virtual void readSpineData(CsvReader CR, string chara_key)
		{
		}

		public virtual bool isPreparedResource()
		{
			return true;
		}

		protected UIPictureBodyData.MosaicPos addMosaicPosData(string bone_key, string state_key, string sta_key, bool only_on_sensitive = false, bool outmesh = false)
		{
			UIPictureBase.EMSTATE multipleEmot = this.PCon.getMultipleEmot(state_key);
			UIPictureBase.EMSTATE_ADD multipleAdditionalEmot = this.PCon.getMultipleAdditionalEmot(sta_key);
			return this.addMosaicPosData(bone_key, multipleEmot, multipleAdditionalEmot, only_on_sensitive, outmesh);
		}

		protected UIPictureBodyData.MosaicPos addMosaicPosData(string bone_key, UIPictureBase.EMSTATE _state, UIPictureBase.EMSTATE_ADD _sta, bool only_on_sensitive = false, bool outmesh = false)
		{
			if (bone_key.IndexOf("|") >= 0)
			{
				string[] array = bone_key.Split(new char[] { '|' });
				int num = array.Length;
				UIPictureBodyData.MosaicPos mosaicPos = default(UIPictureBodyData.MosaicPos);
				for (int i = 0; i < num; i++)
				{
					mosaicPos = this.addMosaicPosData(array[i], _state, _sta, only_on_sensitive, outmesh);
				}
				return mosaicPos;
			}
			UIPictureBodyData.MosaicPos mosaicPos2 = new UIPictureBodyData.MosaicPos(bone_key, _state, _sta, only_on_sensitive, outmesh);
			int mosaicIndex = this.getMosaicIndex(bone_key);
			if (mosaicIndex >= 0)
			{
				this.AMosaicPos[mosaicIndex] = mosaicPos2;
			}
			else
			{
				if (this.AMosaicPos == null)
				{
					this.AMosaicPos = new List<UIPictureBodyData.MosaicPos>(1);
				}
				this.AMosaicPos.Add(mosaicPos2);
			}
			return mosaicPos2;
		}

		public virtual int countMosaic(bool only_on_sensitive)
		{
			if (this.AMosaicPos == null || this.mosaic_radius_px <= 0)
			{
				return 0;
			}
			int num = 0;
			int count = this.AMosaicPos.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AMosaicPos[i].isActive(only_on_sensitive, this.current_state, this.current_sta))
				{
					num++;
				}
			}
			return num;
		}

		public UIPictureBodyData.MosaicPos getMosaicPosDataAppearAlways()
		{
			int num;
			return this.getMosaicPosDataAppearAlways(out num);
		}

		public UIPictureBodyData.MosaicPos getMosaicPosDataAppearAlways(out int index)
		{
			index = -1;
			if (this.AMosaicPos == null)
			{
				return default(UIPictureBodyData.MosaicPos);
			}
			int count = this.AMosaicPos.Count;
			for (int i = 0; i < count; i++)
			{
				UIPictureBodyData.MosaicPos mosaicPos = this.AMosaicPos[i];
				if (!mosaicPos.only_sensitive_mode)
				{
					index = i;
					return mosaicPos;
				}
			}
			return default(UIPictureBodyData.MosaicPos);
		}

		protected int getMosaicIndex(string bone_key)
		{
			if (this.AMosaicPos == null)
			{
				return -1;
			}
			int count = this.AMosaicPos.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AMosaicPos[i].bone_key == bone_key)
				{
					return i;
				}
			}
			return -1;
		}

		public virtual bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot)
		{
			if (this.AMosaicPos == null)
			{
				return false;
			}
			if (X.BTW(0f, (float)id, (float)this.AMosaicPos.Count))
			{
				UIPictureBodyData.MosaicPos mosaicPos = this.AMosaicPos[id];
				float num = (float)this.mosaic_radius_px * 0.015625f;
				Vector2 centerUPos = this.PCon.getCenterUPos(true);
				centerUPos.x += this.APos[0].x * 0.015625f;
				centerUPos.y += this.APos[0].y * 0.015625f;
				Out = Matrix4x4.Translate(new Vector3(centerUPos.x, centerUPos.y, 0f)) * Matrix4x4.Scale(new Vector3(num * 2f, num * 2f, 0f));
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return "<BODY>" + this.name + " ... EMOT:" + this.Emot.emot_id.ToString();
		}

		public UIPictureBodyData getReplaceTerm()
		{
			if (this.ReplaceTerm == null || this.ReplaceTerm.getValue(null) == 0.0)
			{
				return null;
			}
			return this.ReplaceBd;
		}

		public bool getEffectReposition(PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			V = Vector3.zero;
			switch (follow)
			{
			case PTCThread.StFollow.FOLLOW_HIP:
				return this.getEffectReposition("follow_hip", out V);
			case PTCThread.StFollow.FOLLOW_HEAD:
				return this.getEffectReposition("follow_head", out V);
			case PTCThread.StFollow.FOLLOW_S:
				return this.getEffectReposition("follow_s", out V);
			case PTCThread.StFollow.FOLLOW_D:
				return this.getEffectReposition("follow_d", out V);
			case PTCThread.StFollow.FOLLOW_MAGICCIRCLE:
				return this.getEffectReposition("follow_mg", out V);
			default:
				return false;
			}
		}

		public virtual bool getEffectReposition(string s, out Vector3 V)
		{
			V = Vector2.zero;
			if (this.BaseLay != null)
			{
				PxlLayer layerByName = this.BaseLay.pFrm.getLayerByName(s);
				if (layerByName != null)
				{
					V = this.PCon.Gob.transform.TransformPoint(new Vector3(layerByName.x * 0.015625f, -layerByName.y * 0.015625f, layerByName.rotR));
					return true;
				}
			}
			return false;
		}

		public virtual bool readPtcScript(PTCThread rER)
		{
			return UIPictureBodyData.Qu.readPtcScript(rER, 1f);
		}

		public static void initUiPictureBodyData()
		{
			UIPictureBodyData.OBodyData = new BDic<string, UIPictureBodyData>();
		}

		public static void readBodyDataScript(UIPictureBase _PCon)
		{
			CsvReaderA csvReaderA = new CsvReaderA(MTR.Read("body_noel", "", ".csv"), false);
			csvReaderA.VarCon = new CsvVariableContainer();
			List<UIPictureBodyData> list = new List<UIPictureBodyData>();
			csvReaderA.tilde_replace = true;
			string text = "";
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "%CHARA")
				{
					if (text != csvReaderA._1)
					{
						text = csvReaderA._1;
						BetobetoManager.GetManager(text).fnUpdated = new Func<BetobetoManager.SvTexture, bool>(_PCon.FineBetobeto);
						_PCon.FDCon = new UIPictureFader(text);
					}
				}
				else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					string[] array = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1).Split(new char[] { '|' });
					int num = array.Length;
					list.Clear();
					int i = 0;
					while (i < num)
					{
						string text2 = array[i];
						bool flag = false;
						bool flag2 = false;
						if (text2.IndexOf("*") >= 0)
						{
							if (TX.charIs(text2, text2.Length - 1, '*'))
							{
								text2 = TX.slice(text2, 0, text2.Length - 1);
								flag = true;
							}
							if (TX.isStart(text2, "*", 0))
							{
								text2 = TX.slice(text2, 1);
								flag2 = true;
								goto IL_019B;
							}
							goto IL_019B;
						}
						else
						{
							if (!TX.isStart(text2, "SPINE_", 0) || UIPictureBodyData.Get(text2, null, null, true) != null)
							{
								goto IL_019B;
							}
							UIPictureBodySpine uipictureBodySpine = new UIPictureBodySpine(text2, _PCon);
							UIPictureBodyData.OBodyData[text2] = uipictureBodySpine;
							list.Add(uipictureBodySpine);
						}
						IL_0254:
						i++;
						continue;
						IL_019B:
						if (flag || flag2)
						{
							foreach (KeyValuePair<string, UIPictureBodyData> keyValuePair in UIPictureBodyData.OBodyData)
							{
								int num2 = keyValuePair.Key.IndexOf(text2);
								if ((flag2 && flag) ? (num2 >= 0) : (flag2 ? (num2 == keyValuePair.Key.Length - text2.Length) : (num2 == 0)))
								{
									list.Add(keyValuePair.Value);
								}
							}
							goto IL_0254;
						}
						UIPictureBodyData uipictureBodyData = UIPictureBodyData.Get(text2, null, null, false);
						if (uipictureBodyData != null)
						{
							list.Add(uipictureBodyData);
							goto IL_0254;
						}
						csvReaderA.tError("... ");
						goto IL_0254;
					}
					if (list.Count == 0)
					{
						X.de("条件に合う BodyData が不明:: " + TX.join<string>(" | ", array, 0, -1), null);
						continue;
					}
					continue;
				}
				if (list.Count != 0)
				{
					string cmd = csvReaderA.cmd;
					if (cmd != null)
					{
						if (cmd == "%ANIM" || cmd == "%ANIM_SET")
						{
							int clength = csvReaderA.clength;
							if (csvReaderA.cmd == "%ANIM_SET")
							{
								for (int j = list.Count - 1; j >= 0; j--)
								{
									list[j].anim = UIPictureBodyData.ANIM.FREEZE;
								}
							}
							for (int k = 1; k < clength; k++)
							{
								UIPictureBodyData.ANIM anim;
								if (!FEnum<UIPictureBodyData.ANIM>.TryParse(csvReaderA.getIndex(k), out anim, true))
								{
									X.de("不明な UIPictureBodyData::ANIM " + csvReaderA.getIndex(k), null);
								}
								else
								{
									for (int l = list.Count - 1; l >= 0; l--)
									{
										list[l].anim |= anim;
									}
								}
							}
							continue;
						}
						if (!(cmd == "%POS"))
						{
							if (cmd == "%DRIP_AIMING")
							{
								bool flag3 = csvReaderA.Nm(1, 1f) != 0f;
								for (int m = list.Count - 1; m >= 0; m--)
								{
									list[m].drip_aiming = flag3;
								}
								continue;
							}
							if (cmd == "%USE_MOSAIC")
							{
								for (int n = list.Count - 1; n >= 0; n--)
								{
									UIPictureBodyData uipictureBodyData2 = list[n];
									uipictureBodyData2.mosaic_radius_px = csvReaderA.Int(1, 30);
									uipictureBodyData2.addMosaicPosData("", csvReaderA._2, csvReaderA._3, false, false);
								}
								continue;
							}
							if (cmd == "%REPLACE_TERM")
							{
								EvalP evalP = new EvalP(null);
								evalP.Parse(csvReaderA.slice_join(2, " ", ""));
								UIPictureBodyData uipictureBodyData3 = X.Get<string, UIPictureBodyData>(UIPictureBodyData.OBodyData, csvReaderA._1);
								if (uipictureBodyData3 == null)
								{
									X.de("不明なReplace先:" + csvReaderA._1, null);
									continue;
								}
								for (int num3 = list.Count - 1; num3 >= 0; num3--)
								{
									UIPictureBodyData uipictureBodyData4 = list[num3];
									uipictureBodyData4.ReplaceBd = uipictureBodyData3;
									uipictureBodyData4.ReplaceTerm = evalP;
								}
								continue;
							}
						}
						else
						{
							if (csvReaderA.clength < 9)
							{
								csvReaderA.tNote("%POS は 下 へそ 胸 口 で指定する", false);
								continue;
							}
							for (int num4 = list.Count - 1; num4 >= 0; num4--)
							{
								UIPictureBodyData uipictureBodyData5 = list[num4];
								float num5 = -uipictureBodyData5.base_swidth / 2f;
								float num6 = uipictureBodyData5.base_sheight / 2f;
								uipictureBodyData5.APos[0] = new Vector2(csvReaderA.Nm(1, 0f) + num5, -csvReaderA.Nm(2, 0f) + num6);
								uipictureBodyData5.APos[1] = new Vector2(csvReaderA.Nm(3, 0f) + num5, -csvReaderA.Nm(4, 0f) + num6);
								uipictureBodyData5.APos[2] = new Vector2(csvReaderA.Nm(5, 0f) + num5, -csvReaderA.Nm(6, 0f) + num6);
								uipictureBodyData5.APos[3] = new Vector2(csvReaderA.Nm(7, 0f) + num5, -csvReaderA.Nm(8, 0f) + num6);
							}
							continue;
						}
					}
					if (TX.isStart(csvReaderA.cmd, "%SPINE", 0))
					{
						if (TX.noe(text))
						{
							X.de("キャラクター名を指定してから Spine を呼ぶ必要があります ", null);
							return;
						}
						for (int num7 = list.Count - 1; num7 >= 0; num7--)
						{
							list[num7].readSpineData(csvReaderA, text);
						}
					}
				}
			}
		}

		public static void releaseWithSvTexture(BetobetoManager.SvTexture Svt)
		{
			foreach (KeyValuePair<string, UIPictureBodyData> keyValuePair in UIPictureBodyData.OBodyData)
			{
				UIPictureBodySpine uipictureBodySpine = keyValuePair.Value as UIPictureBodySpine;
				if (uipictureBodySpine != null)
				{
					uipictureBodySpine.releaseWithSvTextureSpine(Svt);
				}
			}
		}

		public static UIPictureBodyData Get(PxlFrame F, UIPictureBase _PCon)
		{
			int num = F.countLayers();
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = F.getLayer(i);
				if (TX.isStart(layer.name, "body_", 0))
				{
					return UIPictureBodyData.Get(TX.slice(layer.name, 5), layer, _PCon, false);
				}
			}
			PxlLayer layer2 = F.getLayer(0);
			return UIPictureBodyData.Get(layer2.name, layer2, null, false);
		}

		public static UIPictureBodyData Get(string name, PxlLayer L = null, UIPictureBase _PCon = null, bool no_error = false)
		{
			UIPictureBodyData uipictureBodyData;
			if (UIPictureBodyData.OBodyData.TryGetValue(name, out uipictureBodyData))
			{
				if (L != null && uipictureBodyData.getBaseLayImg() != L.Img)
				{
					X.de("異なるイメージのレイヤーを同じボディとして登録しようとした: " + L.pFrm.pPose.title + ":" + L.pFrm.name, null);
				}
				return uipictureBodyData;
			}
			if (L == null)
			{
				if (!no_error)
				{
					X.de("BodyData " + name + " を作っていません", null);
				}
				return null;
			}
			UIPictureBodyData uipictureBodyData2 = new UIPictureBodyData(name, _PCon, L);
			UIPictureBodyData.OBodyData[name] = uipictureBodyData2;
			return uipictureBodyData2;
		}

		public readonly UIPictureBase PCon;

		public readonly string name;

		public UIPictureBase.PrEmotion Emot;

		public PxlLayer BaseLay;

		public UIPictureBodyData.ANIM anim;

		public UIPictureBase.EMSTATE_ADD[] Alay_attr;

		public List<Vector2> APos;

		private float af;

		private float gaze_fx;

		private float gaze_fy;

		public static Quaker Qu;

		private static EfParticleLooper Ptl;

		private static EffectItem EfBreathe;

		protected float last_x;

		protected float last_y;

		public bool drip_aiming;

		protected List<UIPictureBodyData.MosaicPos> AMosaicPos;

		protected int mosaic_radius_px = 30;

		protected UIPictureBase.EMSTATE current_state;

		protected UIPictureBase.EMSTATE_ADD current_sta;

		public UIPictureBase.EMSTATE variation_bits;

		private FnEffectRun FD_fnDrawBreathe;

		private EvalP ReplaceTerm;

		private UIPictureBodyData ReplaceBd;

		private static BDic<string, UIPictureBodyData> OBodyData;

		public enum ANIM
		{
			FREEZE,
			SCWAT,
			VIB,
			BREATH = 4,
			GAZE = 8,
			TIRED = 16,
			ROTATE = 32
		}

		public enum POS
		{
			UNDER,
			HESO,
			BREAST,
			MOUTH
		}

		public struct MosaicPos
		{
			public MosaicPos(string bone_key, UIPictureBase.EMSTATE state = UIPictureBase.EMSTATE.NORMAL, UIPictureBase.EMSTATE_ADD state_add = UIPictureBase.EMSTATE_ADD.NORMAL, bool only_sensitive_mode = false, bool use_outmesh = false)
			{
				this.bone_key = bone_key;
				this.state = state;
				this.state_add = state_add;
				this.use_outmesh = use_outmesh;
				this.only_sensitive_mode = only_sensitive_mode;
			}

			public bool valid
			{
				get
				{
					return this.bone_key != null;
				}
			}

			public bool isActive(bool only_on_sensitive, UIPictureBase.EMSTATE current_state, UIPictureBase.EMSTATE_ADD current_sta)
			{
				return (!this.only_sensitive_mode || only_on_sensitive) && (current_state & this.state) == this.state && (current_sta & this.state_add) == this.state_add;
			}

			public string bone_key;

			public UIPictureBase.EMSTATE state;

			public UIPictureBase.EMSTATE_ADD state_add;

			public bool use_outmesh;

			public bool only_sensitive_mode;
		}
	}
}
