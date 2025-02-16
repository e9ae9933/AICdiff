using System;
using Better;

namespace XX
{
	public class EfParticleLoader : EfParticleVarContainer
	{
		public EfParticleLoader(string _key)
		{
			this.KeyF = new BDic<string, float>();
			this.KeyI = new BDic<string, int>();
			this.KeyB = new BDic<string, bool>();
			this.KeyS = new BDic<string, string>();
			this.KeyCol = new BDic<string, C32>();
			this.key = _key;
			if (EfParticleLoader.BufWhiteCol == null)
			{
				EfParticleLoader.BufWhiteCol = new C32().White();
			}
			this.StbInput = new STB(256);
		}

		public void CopyFrom(EfParticleLoader Pl)
		{
			this.KeyF = new BDic<string, float>(Pl.KeyF);
			this.KeyI = new BDic<string, int>(Pl.KeyI);
			this.KeyB = new BDic<string, bool>(Pl.KeyB);
			this.KeyS = new BDic<string, string>(Pl.KeyS);
			this.KeyCol = new BDic<string, C32>(Pl.KeyCol);
		}

		public void addScript(StringHolder SH)
		{
			if (this.StbInput.Length != 0)
			{
				this.StbInput.Add("\n");
			}
			for (int i = 0; i < SH.clength; i++)
			{
				if (i > 0)
				{
					this.StbInput.Add(" ");
				}
				this.StbInput.Add(SH.getIndex(i));
			}
		}

		public void addScript(STB Stb, int lcs = 0, int lce = -1)
		{
			if (lce < 0)
			{
				lce = Stb.Length;
			}
			if (this.StbInput.Length != 0)
			{
				this.StbInput.Add("\n");
			}
			this.StbInput.Add(Stb, lcs, lce - lcs);
		}

		public EfParticleLoader endCR()
		{
			this.StbInput.TrimCapacity();
			return this;
		}

		public bool initted
		{
			get
			{
				return this.StbInput == null;
			}
		}

		public override EfParticleVarContainer initParticle()
		{
			if (this.StbInput == null)
			{
				return this;
			}
			CsvReader csvReader = new CsvReader(this.StbInput.ToString(), CsvReader.RegSpace, true);
			this.StbInput = null;
			BDic<string, int> typeObject = EfParticle.getTypeObject();
			if (this.CloneFrom != null)
			{
				this.CloneFrom.initParticle();
				this.CopyFrom(this.CloneFrom);
			}
			if (this.MergeFrom != null)
			{
				this.MergeFrom.initParticle();
				this.mergeFrom(this.MergeFrom);
			}
			while (csvReader.read())
			{
				if (csvReader.cmd == "layer")
				{
					try
					{
						EFLAY eflay;
						if (!FEnum<EFLAY>.TryParse(csvReader._1, out eflay, true))
						{
							X.de(string.Concat(new string[]
							{
								"不明なレイヤー指定 line",
								csvReader.get_cur_line().ToString(),
								":",
								csvReader.getLastStr(),
								" (TOP_ADD みたいに指定する)"
							}), null);
						}
						else
						{
							string text = "layer";
							int num = (int)eflay;
							this.SetV(text, num.ToString());
						}
						continue;
					}
					catch
					{
						continue;
					}
				}
				if (csvReader.cmd == "alp_s" || csvReader.cmd == "alp_e")
				{
					try
					{
						this.SetV(csvReader.cmd, (TX.eval(csvReader.slice_join(1, " ", ""), "") / 255.0).ToString());
						continue;
					}
					catch
					{
						X.dl(string.Concat(new string[]
						{
							"コマンド入力 エラー line",
							csvReader.get_cur_line().ToString(),
							":",
							csvReader.getLastStr(),
							" "
						}), null, false, false);
						continue;
					}
				}
				if (csvReader.cmd.ToLower() == "type")
				{
					int num2;
					PtTYPE ptTYPE;
					if (typeObject.TryGetValue(csvReader._1, out num2))
					{
						this.SetV("type", num2.ToString());
					}
					else if (!FEnum<PtTYPE>.TryParse(csvReader._1, out ptTYPE, true))
					{
						X.dl("type エラー line" + csvReader.get_cur_line().ToString() + ":" + csvReader.getLastStr(), null, false, false);
					}
					else
					{
						string text2 = "type";
						int num = (int)ptTYPE;
						this.SetV(text2, num.ToString());
					}
				}
				else
				{
					if (EfParticleManager.RegW.Match(csvReader.cmd).Success)
					{
						try
						{
							string cmd = csvReader.cmd;
							if (cmd == "slen" || cmd == "slen_i" || cmd == "slen_agR_i" || cmd == "ex" || cmd == "exagR" || cmd == "zm" || cmd == "mv" || cmd == "bezier" || cmd == "xdf" || cmd == "ydf" || cmd == "line_len" || cmd == "thick" || cmd == "draw_sagR" || cmd == "draw_eagR" || cmd == "bezier" || cmd == "bezier_center" || cmd == "line_ex_time" || cmd == "mesh_scale_y" || cmd == "mesh_rot_agR" || cmd == "mesh_translate")
							{
								EfParticleLoader.String2 @string;
								if (!EfParticleLoader.Ostr_doublekey.TryGetValue(cmd, out @string))
								{
									@string = (EfParticleLoader.Ostr_doublekey[cmd] = new EfParticleLoader.String2(cmd + "_min", cmd + "_max"));
								}
								this.SetV(@string.s0, TX.eval(csvReader._1, "").ToString());
								this.SetV(@string.s1, TX.eval(csvReader._2, "").ToString());
							}
							else if (cmd == "alp")
							{
								this.SetV("alp_s", (TX.eval(csvReader._1, "") / 255.0).ToString());
								this.SetV("alp_e", (TX.eval(csvReader._2, "") / 255.0).ToString());
							}
							else if (cmd == "camera_check_zm")
							{
								this.SetV("camera_check_zm_mul", TX.eval(csvReader._1, "").ToString());
								this.SetV("camera_check_zm_add", TX.eval(csvReader._2, "").ToString());
							}
							else if (cmd == "col_s")
							{
								this.SetV("col_s0", csvReader._1);
								this.SetV("col_s1", csvReader._2);
							}
							else if (cmd == "col_e")
							{
								this.SetV("col_e0", csvReader._1);
								this.SetV("col_e1", csvReader._2);
							}
							else
							{
								string valType = EfParticle.getValType(cmd);
								string text3 = csvReader.slice_join(1, " ", "");
								this.SetV(csvReader.cmd, (valType == "float" || valType == "int" || valType == "uint") ? TX.eval(text3, "").ToString() : text3);
							}
							continue;
						}
						catch
						{
							X.dl(string.Concat(new string[]
							{
								"コマンド入力 エラー line",
								csvReader.get_cur_line().ToString(),
								":",
								csvReader.getLastStr(),
								" "
							}), null, false, false);
							continue;
						}
					}
					X.de("curPtc " + this.key + " に対する不明なコマンド: " + csvReader.cmd, null);
				}
			}
			return this;
		}

		public override string key
		{
			get
			{
				return this.GetV("key", "").ToString();
			}
			set
			{
				this.SetV("key", value);
			}
		}

		public override EfParticleVarContainer SetV(string k, string val)
		{
			string valType = EfParticle.getValType(k);
			if (valType != null)
			{
				if (!(valType == "float"))
				{
					if (!(valType == "int"))
					{
						if (!(valType == "bool"))
						{
							if (!(valType == "string"))
							{
								if (valType == "C32")
								{
									this.KeyCol[k] = new C32(val);
								}
							}
							else
							{
								this.KeyS[k] = val;
							}
						}
						else
						{
							this.KeyB[k] = X.Nm(val, 0f, true) != 0f;
						}
					}
					else
					{
						this.KeyI[k] = X.NmI(val, 0, true, false);
					}
				}
				else
				{
					this.KeyF[k] = X.Nm(val, 0f, true);
				}
			}
			return this;
		}

		public EfParticleLoader mergeFrom(EfParticleLoader Src)
		{
			if (Src == null)
			{
				return this;
			}
			string key = this.key;
			X.objMerge<string, float>(this.KeyF, Src.KeyF, false);
			X.objMerge<string, int>(this.KeyI, Src.KeyI, false);
			X.objMerge<string, bool>(this.KeyB, Src.KeyB, false);
			X.objMerge<string, string>(this.KeyS, Src.KeyS, false);
			X.objMerge<string, C32>(this.KeyCol, Src.KeyCol, false);
			this.key = key;
			return this;
		}

		public override object GetV(string k, object default_val)
		{
			return this.GetV0D(k, default_val, false);
		}

		public override object GetV0D(string k, object default_val, bool default_on_false = true)
		{
			string valType = EfParticle.getValType(k);
			if (valType != null)
			{
				if (!(valType == "float"))
				{
					if (!(valType == "int"))
					{
						if (!(valType == "bool"))
						{
							if (!(valType == "string"))
							{
								if (valType == "C32")
								{
									C32 c;
									if (!this.KeyCol.TryGetValue(k, out c))
									{
										return EfParticleLoader.BufWhiteCol;
									}
									return c;
								}
							}
							else
							{
								string text;
								if (!this.KeyS.TryGetValue(k, out text))
								{
									return default_val.ToString();
								}
								if (text == "" || text == null)
								{
									return default_val.ToString();
								}
								return text;
							}
						}
						else
						{
							bool flag;
							if (!this.KeyB.TryGetValue(k, out flag))
							{
								return default_val;
							}
							return flag;
						}
					}
					else
					{
						if (default_val.GetType() == typeof(float))
						{
							default_val = (int)((float)default_val);
						}
						int num;
						if (!this.KeyI.TryGetValue(k, out num))
						{
							return default_val;
						}
						if (default_on_false && num == 0)
						{
							return default_val;
						}
						return num;
					}
				}
				else
				{
					if (default_val.GetType() == typeof(int))
					{
						default_val = (float)((int)default_val);
					}
					float num2;
					if (!this.KeyF.TryGetValue(k, out num2))
					{
						return default_val;
					}
					if (default_on_false && num2 == 0f)
					{
						return default_val;
					}
					return num2;
				}
			}
			return null;
		}

		public override bool IsSet(string k)
		{
			string valType = EfParticle.getValType(k);
			if (valType != null)
			{
				if (valType == "float")
				{
					return this.KeyF.ContainsKey(k);
				}
				if (valType == "int")
				{
					return this.KeyI.ContainsKey(k);
				}
				if (valType == "bool")
				{
					return this.KeyB.ContainsKey(k);
				}
				if (valType == "string")
				{
					return this.KeyS.ContainsKey(k);
				}
				if (valType == "C32")
				{
					return this.KeyCol.ContainsKey(k);
				}
			}
			return false;
		}

		private BDic<string, float> KeyF;

		private BDic<string, int> KeyI;

		private BDic<string, bool> KeyB;

		private BDic<string, string> KeyS;

		private BDic<string, C32> KeyCol;

		public string clone_from;

		public string merge_from;

		public EfParticleLoader CloneFrom;

		public EfParticleLoader MergeFrom;

		private STB StbInput;

		private static C32 BufWhiteCol;

		private static BDic<string, EfParticleLoader.String2> Ostr_doublekey = new BDic<string, EfParticleLoader.String2>();

		private struct String2
		{
			public String2(string _s0, string _s1)
			{
				this.s0 = _s0;
				this.s1 = _s1;
			}

			public string s0;

			public string s1;
		}
	}
}
