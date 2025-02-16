using System;
using Better;
using XX;

namespace m2d
{
	public class M2DropObjectReader
	{
		private static void initDrawFn()
		{
			if (M2DropObjectReader.OFnDraw == null)
			{
				M2DropObjectReader.OFnDraw = new BDic<string, M2DropObject.FnDropObjectDraw>(8);
				M2DropObjectReader.OFnDraw["PlayerDrip"] = new M2DropObject.FnDropObjectDraw(M2DropObject.fnDropRunDraw_PlayerDrip);
				M2DropObjectReader.OFnDraw["splash_blood"] = new M2DropObject.FnDropObjectDraw(M2DropObject.fnDropRunDraw_splash_blood);
				M2DropObjectReader.OFnDraw["splash_love_juice"] = new M2DropObject.FnDropObjectDraw(M2DropObject.fnDropRunDraw_splash_love_juice);
				M2DropObjectReader.OFnDraw["splash_sperma"] = new M2DropObject.FnDropObjectDraw(M2DropObject.fnDropRunDraw_splash_sperma);
			}
		}

		public static M2DropObject.FnDropObjectDraw GetFn(string s)
		{
			return X.Get<string, M2DropObject.FnDropObjectDraw>(M2DropObjectReader.OFnDraw, s);
		}

		public static void assignDrawFn(string t, M2DropObject.FnDropObjectDraw Fn)
		{
			M2DropObjectReader.initDrawFn();
			M2DropObjectReader.OFnDraw[t] = Fn;
		}

		public static void initDroScript()
		{
			M2DropObjectReader.initDrawFn();
			string text = NKT.readStreamingText("m2d/___dropobject_list.dat", false);
			if (TX.noe(text))
			{
				return;
			}
			CsvReaderA csvReaderA = new CsvReaderA(text, true);
			M2DropObjectReader.ODroReader = new BDic<string, M2DropObjectReader>();
			csvReaderA.tilde_replace = true;
			M2DropObjectReader m2DropObjectReader = null;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					m2DropObjectReader = M2DropObjectReader.Get(index, true);
					if (m2DropObjectReader != null)
					{
						X.de("重複キー: " + index, null);
						m2DropObjectReader = null;
					}
					else
					{
						m2DropObjectReader = (M2DropObjectReader.ODroReader[index] = new M2DropObjectReader());
					}
				}
				if (m2DropObjectReader != null)
				{
					m2DropObjectReader.read(csvReaderA);
				}
			}
		}

		public static M2DropObjectReader Get(string key, bool no_error = false)
		{
			if (M2DropObjectReader.ODroReader == null)
			{
				X.de("M2DropObjectReaderが初期化されていない", null);
				return null;
			}
			M2DropObjectReader m2DropObjectReader = X.Get<string, M2DropObjectReader>(M2DropObjectReader.ODroReader, key);
			if (m2DropObjectReader == null)
			{
				if (!no_error)
				{
					X.de("M2DropObjectReader::Get " + key + " がありません", null);
				}
				return null;
			}
			return m2DropObjectReader;
		}

		public static M2DropObjectReader Get(StringKey key, bool no_error = false)
		{
			if (M2DropObjectReader.ODroReader == null)
			{
				X.de("M2DropObjectReaderが初期化されていない", null);
				return null;
			}
			M2DropObjectReader m2DropObjectReader = X.Get<M2DropObjectReader>(M2DropObjectReader.ODroReader, key);
			if (m2DropObjectReader == null)
			{
				if (!no_error)
				{
					X.de("M2DropObjectReader::Get " + key + " がありません", null);
				}
				return null;
			}
			return m2DropObjectReader;
		}

		public static int GetAndSet(Map2d Mp, string dr_key, float cx = 0f, float cy = 0f, float x_mul = 1f, float y_mul = 1f, float cz = 0f, M2DropObject.FnDropObjectDraw FnDraw = null, object MyObj = null)
		{
			M2DropObjectReader m2DropObjectReader = M2DropObjectReader.Get(dr_key, false);
			if (m2DropObjectReader == null)
			{
				return 0;
			}
			if (FnDraw == null)
			{
				return m2DropObjectReader.createObjects(Mp, m2DropObjectReader.FnDraw, cx, cy, x_mul, y_mul, cz, MyObj);
			}
			return m2DropObjectReader.createObjects(Mp, FnDraw, cx, cy, x_mul, y_mul, cz, MyObj);
		}

		public M2DropObjectReader copyFrom(M2DropObjectReader Src)
		{
			this.FnDraw = Src.FnDraw;
			this.count = Src.count;
			this.saf = Src.saf;
			this.saf_i = Src.saf_i;
			this.type = Src.type;
			this.x = Src.x;
			this.y = Src.y;
			this.vx = Src.vx;
			this.vy = Src.vy;
			this.mv = Src.mv;
			this.dirR = Src.dirR;
			this.z = Src.z;
			this.size = Src.size;
			this.time = Src.time;
			this.gravity_scale = Src.gravity_scale;
			this.bounce_x_reduce = Src.bounce_x_reduce;
			this.bounce_y_reduce = Src.bounce_y_reduce;
			this.bounce_x_reduce_when_ground = Src.bounce_x_reduce_when_ground;
			return this;
		}

		public static bool checkDRO(Map2d Mp, PTCThread CR)
		{
			StringKey hash = CR.getHash(1);
			float num = CR.NmE(2, 0f);
			float num2 = CR.NmE(3, 0f);
			int num3 = 0;
			M2DropObjectReader m2DropObjectReader = M2DropObjectReader.Get(hash, false);
			if (m2DropObjectReader != null)
			{
				float num4 = CR.NmE(4, 1f);
				float num5 = CR.NmE(5, 1f);
				return m2DropObjectReader.createObjects(Mp, m2DropObjectReader.FnDraw, num, num2, num4, num5, 0f, null) > 0;
			}
			if (CR.countBracketsOnThisLine(ref num3) == 0)
			{
				return false;
			}
			M2DropObjectReader m2DropObjectReader2 = new M2DropObjectReader();
			EfSetterP.ESPLine nextLineSimple;
			while ((nextLineSimple = CR.getNextLineSimple()) != null && CR.countBracketsOnThisLine(ref num3) > 0)
			{
				m2DropObjectReader2.read(nextLineSimple);
			}
			M2DropObject.FnDropObjectDraw fnDropObjectDraw = X.Get<M2DropObject.FnDropObjectDraw>(M2DropObjectReader.OFnDraw, hash);
			if (fnDropObjectDraw == null)
			{
				CR.tError("M2DropObjectReader::checkDRO 不明なfnkey:" + hash);
			}
			return m2DropObjectReader2.createObjects(Mp, fnDropObjectDraw, num, num2, 1f, 1f, 0f, null) > 0;
		}

		private bool read(IStringHolder CR)
		{
			string cmd = CR.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1564253156U)
				{
					if (num <= 967958004U)
					{
						if (num <= 744348338U)
						{
							if (num != 597743964U)
							{
								if (num == 744348338U)
								{
									if (cmd == "dirR")
									{
										return this.createArrayFromCR(ref this.dirR, CR);
									}
								}
							}
							else if (cmd == "size")
							{
								return this.createArrayFromCR(ref this.size, CR);
							}
						}
						else if (num != 826051549U)
						{
							if (num == 967958004U)
							{
								if (cmd == "count")
								{
									return this.createArrayFromCR(ref this.count, CR);
								}
							}
						}
						else if (cmd == "%CLONE")
						{
							M2DropObjectReader m2DropObjectReader = M2DropObjectReader.Get(CR._1, false);
							if (m2DropObjectReader != null)
							{
								this.copyFrom(m2DropObjectReader);
							}
							return true;
						}
					}
					else if (num <= 1296684707U)
					{
						if (num != 1279907088U)
						{
							if (num == 1296684707U)
							{
								if (cmd == "vx")
								{
									return this.createArrayFromCR(ref this.vx, CR);
								}
							}
						}
						else if (cmd == "vy")
						{
							return this.createArrayFromCR(ref this.vy, CR);
						}
					}
					else if (num != 1361572173U)
					{
						if (num != 1496191754U)
						{
							if (num == 1564253156U)
							{
								if (cmd == "time")
								{
									return this.createArrayFromCR(ref this.time, CR);
								}
							}
						}
						else if (cmd == "mv")
						{
							return this.createArrayFromCR(ref this.mv, CR);
						}
					}
					else if (cmd == "type")
					{
						this.type = DROP_TYPE.NO_OPTION;
						string[] array = TX.split(CR._1, "|");
						for (int i = array.Length - 1; i >= 0; i--)
						{
							DROP_TYPE drop_TYPE;
							if (FEnum<DROP_TYPE>.TryParse(TX.trim(array[i]), out drop_TYPE, true))
							{
								this.type |= drop_TYPE;
							}
						}
						return true;
					}
				}
				else if (num <= 3544432845U)
				{
					if (num <= 2337930737U)
					{
						if (num != 1678092008U)
						{
							if (num == 2337930737U)
							{
								if (cmd == "bounce_x_reduce")
								{
									return this.createArrayFromCR(ref this.bounce_x_reduce, CR);
								}
							}
						}
						else if (cmd == "bounce_x_reduce_when_ground")
						{
							return this.createArrayFromCR(ref this.bounce_x_reduce_when_ground, CR);
						}
					}
					else if (num != 2714685132U)
					{
						if (num != 3354697228U)
						{
							if (num == 3544432845U)
							{
								if (cmd == "saf_i")
								{
									return this.createArrayFromCR(ref this.saf_i, CR);
								}
							}
						}
						else if (cmd == "gravity_scale")
						{
							return this.createArrayFromCR(ref this.gravity_scale, CR);
						}
					}
					else if (cmd == "bounce_y_reduce")
					{
						return this.createArrayFromCR(ref this.bounce_y_reduce, CR);
					}
				}
				else if (num <= 3894193965U)
				{
					if (num != 3789641125U)
					{
						if (num == 3894193965U)
						{
							if (cmd == "saf")
							{
								return this.createArrayFromCR(ref this.saf, CR);
							}
						}
					}
					else if (cmd == "fn_key")
					{
						if (TX.noe(CR._1))
						{
							this.FnDraw = null;
						}
						else
						{
							this.FnDraw = X.Get<string, M2DropObject.FnDropObjectDraw>(M2DropObjectReader.OFnDraw, CR._1);
							if (this.FnDraw == null)
							{
								CR.tError("M2DropObjectReader::不明な fn_key " + CR._1);
							}
						}
						return true;
					}
				}
				else if (num != 4228665076U)
				{
					if (num != 4245442695U)
					{
						if (num == 4278997933U)
						{
							if (cmd == "z")
							{
								return this.createArrayFromCR(ref this.z, CR);
							}
						}
					}
					else if (cmd == "x")
					{
						return this.createArrayFromCR(ref this.x, CR);
					}
				}
				else if (cmd == "y")
				{
					return this.createArrayFromCR(ref this.y, CR);
				}
			}
			return false;
		}

		public bool createArrayFromCR(ref float[] A, IStringHolder CR)
		{
			if (CR.clength <= 1)
			{
				return true;
			}
			if (CR.clength == 2)
			{
				A = new float[] { CR.NmE(1, 0f) };
			}
			else
			{
				A = new float[]
				{
					CR.NmE(1, 0f),
					CR.NmE(2, 0f)
				};
			}
			return true;
		}

		public bool createArrayFromCR(ref int[] A, IStringHolder CR)
		{
			if (CR.clength <= 1)
			{
				return true;
			}
			if (CR.clength == 2)
			{
				A = new int[] { CR.IntE(1, 0) };
			}
			else
			{
				A = new int[]
				{
					CR.IntE(1, 0),
					CR.IntE(2, 0)
				};
			}
			return true;
		}

		public int createObjects(Map2d Mp, M2DropObject.FnDropObjectDraw FnDraw, float cx = 0f, float cy = 0f, float x_mul = 1f, float y_mul = 1f, float cz = 0f, object MyObj = null)
		{
			int num = 0;
			this.getValue(this.count, ref num);
			if (num == 0)
			{
				return 0;
			}
			M2DropObjectContainer dropCon = Mp.DropCon;
			float num2 = 0f;
			if (this.getValue(this.saf_i, ref num2))
			{
				num2 /= (float)num;
			}
			for (int i = 0; i < num; i++)
			{
				M2DropObject m2DropObject = dropCon.Add(FnDraw, cx + this.getValue0(this.x, 0f) * x_mul, cy + this.getValue0(this.y, 0f) * y_mul, this.getValue0(this.vx, 0f) * x_mul, this.getValue0(this.vy, 0f) * y_mul, this.getValue0(this.z, -1f), this.getValue0(this.time, -1f));
				this.Set1(this.SetV(m2DropObject));
				m2DropObject.MyObj = MyObj;
				m2DropObject.z += cz;
				m2DropObject.af -= num2 * (float)i;
			}
			return num;
		}

		public M2DropObject Set01(M2DropObject Dro)
		{
			this.getValue(this.x, ref Dro.x);
			this.getValue(this.y, ref Dro.y);
			this.getValue(this.vx, ref Dro.vx);
			this.getValue(this.vy, ref Dro.vy);
			this.getValue(this.z, ref Dro.z);
			this.getValue(this.time, ref Dro.time);
			return this.Set1(this.SetV(Dro));
		}

		public M2DropObject SetV(M2DropObject Dro)
		{
			float num = 0f;
			float num2 = 0f;
			if (this.type != DROP_TYPE._NONE)
			{
				Dro.type = this.type;
			}
			this.getValue(this.mv, ref num);
			this.getValue(this.dirR, ref num2);
			if (num > 0f)
			{
				Dro.vx += num * X.Cos(num2);
				Dro.vy -= num * X.Sin(num2);
			}
			if (this.getValue(this.saf, ref Dro.af))
			{
				Dro.af *= -1f;
			}
			return Dro;
		}

		public M2DropObject Set1(M2DropObject Dro)
		{
			this.getValue(this.size, ref Dro.size);
			this.getValue(this.gravity_scale, ref Dro.gravity_scale);
			this.getValue(this.bounce_x_reduce, ref Dro.bounce_x_reduce);
			this.getValue(this.bounce_y_reduce, ref Dro.bounce_y_reduce);
			this.getValue(this.bounce_x_reduce_when_ground, ref Dro.bounce_x_reduce_when_ground);
			return Dro;
		}

		private bool getValue(int[] A, ref int _v)
		{
			if (A == null)
			{
				return false;
			}
			if (A.Length == 1)
			{
				_v = A[0];
			}
			else
			{
				_v = X.IntR(X.NIXP((float)A[0], (float)A[1]));
			}
			return true;
		}

		private bool getValue(float[] A, ref float _v)
		{
			if (A == null)
			{
				return false;
			}
			if (A.Length == 1)
			{
				_v = A[0];
			}
			else
			{
				_v = X.NIXP(A[0], A[1]);
			}
			return true;
		}

		private int getValue0(int[] A, int def = 0)
		{
			this.getValue(A, ref def);
			return def;
		}

		private float getValue0(float[] A, float def = 0f)
		{
			this.getValue(A, ref def);
			return def;
		}

		private const string data_path = "m2d/___dropobject_list";

		private static BDic<string, M2DropObjectReader> ODroReader;

		private static BDic<string, M2DropObject.FnDropObjectDraw> OFnDraw;

		private static DateTime LoadDate;

		private M2DropObject.FnDropObjectDraw FnDraw;

		private int[] count;

		private DROP_TYPE type = DROP_TYPE._NONE;

		private float[] x;

		private float[] y;

		private float[] vx;

		private float[] vy;

		private float[] z;

		private float[] saf;

		private float[] saf_i;

		private float[] size;

		private float[] time;

		private float[] gravity_scale;

		private float[] bounce_x_reduce;

		private float[] bounce_y_reduce;

		private float[] bounce_x_reduce_when_ground;

		public float[] dirR;

		public float[] mv;
	}
}
