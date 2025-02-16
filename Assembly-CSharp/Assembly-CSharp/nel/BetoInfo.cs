using System;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class BetoInfo
	{
		public BetoInfo(BetoInfo B = null)
		{
			if (B != null)
			{
				this.CopyFrom(B);
			}
		}

		public BetoInfo(float _power, Color32 _Col, Color32 _Col2, float _level, BetoInfo.TYPE _type = BetoInfo.TYPE.SMOKE, float _scale = 1f, int _thread = 0, float _jumprate = 0.2f)
		{
			this.power = _power;
			this.Col = _Col;
			this.Col2 = _Col2;
			this.level = _level;
			this.type = _type;
			this.scale = _scale;
			this.thread = _thread;
			this.jumprate = _jumprate;
		}

		public BetoInfo CopyFrom(BetoInfo B)
		{
			this.power = B.power;
			this.Col = B.Col;
			this.Col2 = B.Col2;
			this.type = B.type;
			this.level = B.level;
			this.scale = B.scale;
			this.thread = B.thread;
			this.jumprate = B.jumprate;
			this.BloodReplaceCol = B.BloodReplaceCol;
			return this;
		}

		public void readBinaryFrom(ByteReader Ba, bool read_jumprate, bool read_bloodreplace)
		{
			this.fill_id = Ba.readInt();
			this.thread = Ba.readInt();
			try
			{
				this.type = (BetoInfo.TYPE)Ba.readByte();
			}
			catch
			{
				this.type = BetoInfo.TYPE.SMOKE;
			}
			this.power = Ba.readFloat();
			this.Col = C32.d2c(Ba.readUInt());
			this.Col2 = C32.d2c(Ba.readUInt());
			this.level = Ba.readFloat();
			this.scale = Ba.readFloat();
			this.level = X.MMX(0f, this.level, 4f);
			this.scale = X.MMX(0f, this.scale, 4f);
			if (read_jumprate)
			{
				this.jumprate = Ba.readFloat();
			}
			else
			{
				this.jumprate = 0.2f;
			}
			if (read_bloodreplace)
			{
				this.BloodReplaceCol = C32.d2c(Ba.readUInt());
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeInt(this.fill_id);
			Ba.writeInt(this.thread);
			Ba.writeByte((int)this.type);
			Ba.writeFloat(this.power);
			Ba.writeUInt(C32.c2d(this.Col));
			Ba.writeUInt(C32.c2d(this.Col2));
			Ba.writeFloat(this.level);
			Ba.writeFloat(this.scale);
			Ba.writeFloat(this.jumprate);
			Ba.writeUInt(C32.c2d(this.BloodReplaceCol));
		}

		public BetoInfo Fix()
		{
			this.Col = MTRX.colb.Set(this.Col).randomize(3f, 7f, 7f, 0f).C;
			this.level /= 100f;
			return this;
		}

		public BetoInfo FillId(ref int total_fill_count)
		{
			int num = total_fill_count;
			total_fill_count = num + 1;
			this.fill_id = num;
			total_fill_count &= 16777215;
			return this;
		}

		public BetoInfo Thread(int _thread, bool no_copy = false)
		{
			BetoInfo betoInfo = (no_copy ? this : new BetoInfo(this));
			betoInfo.thread = _thread;
			return betoInfo;
		}

		public BetoInfo Pow(int _p, bool no_copy = false)
		{
			BetoInfo betoInfo = (no_copy ? this : new BetoInfo(this));
			betoInfo.power = (float)_p;
			return betoInfo;
		}

		public BetoInfo LockF(int _p, bool no_copy = false)
		{
			BetoInfo betoInfo = (no_copy ? this : new BetoInfo(this));
			betoInfo.lockf = _p;
			return betoInfo;
		}

		public BetoInfo JumpRate(float jumprate, bool no_copy = false)
		{
			BetoInfo betoInfo = (no_copy ? this : new BetoInfo(this));
			betoInfo.jumprate = jumprate;
			return betoInfo;
		}

		public BetoInfo Level(float level, bool no_copy = false)
		{
			BetoInfo betoInfo = (no_copy ? this : new BetoInfo(this));
			betoInfo.level = level;
			return betoInfo;
		}

		public const int THREAD_NORMAL = 0;

		public const int THREAD_GROUND = 1;

		public const int THREAD_ONESHOT = -1;

		public BetoInfo.TYPE type;

		public float power;

		public int thread;

		public Color32 Col;

		public Color32 Col2;

		public float level;

		public float scale = 1f;

		public int lockf = 30;

		public float jumprate;

		public const float jumprate_default = 0.2f;

		public Color32 BloodReplaceCol = MTRX.ColTrnsp;

		public int fill_id;

		public static BetoInfo Normal = new BetoInfo(15f, C32.d2c(3428872574U), C32.d2c(2291432327U), 20f, BetoInfo.TYPE.SMOKE, 0.65f, 0, 0.2f).LockF(50, true);

		public static BetoInfo NormalS = new BetoInfo(15f, C32.d2c(3428872574U), C32.d2c(2291432327U), 12f, BetoInfo.TYPE.SMOKE, 0.55f, 0, 0.2f).LockF(50, true);

		public static BetoInfo Absorbed = new BetoInfo(45f, C32.d2c(3428872574U), C32.d2c(2291432327U), 26f, BetoInfo.TYPE.SMOKE, 0.62f, 0, 0.2f).LockF(240, true);

		public static BetoInfo Worm = new BetoInfo(45f, C32.d2c(3428010323U), C32.d2c(2293520729U), 25f, BetoInfo.TYPE.SMOKE, 0.45f, 0, 0.2f);

		public static BetoInfo Sperm = new BetoInfo(500f, C32.d2c(3438408163U), C32.d2c(2290188654U), 40f, BetoInfo.TYPE.SMOKE, 0.3f, -1, 0.2f);

		public static BetoInfo Mud = new BetoInfo(20f, C32.d2c(4289441166U), C32.d2c(2576971605U), 15f, BetoInfo.TYPE.LIQUID, 0.6f, -1, 0.1f);

		public static BetoInfo Sperm2 = new BetoInfo(500f, C32.d2c(uint.MaxValue), C32.d2c(3449723265U), 85f, BetoInfo.TYPE.LIQUID, 0.4f, -1, 0.1f);

		public static BetoInfo EggLay = new BetoInfo(40f, C32.d2c(4285106805U), C32.d2c(4288250517U), 18f, BetoInfo.TYPE.LIQUID, 0.35f, 0, 0.05f).LockF(5, false);

		public static BetoInfo Ydrg = new BetoInfo(8f, C32.d2c(2003074155U), C32.d2c(1438202840U), 30f, BetoInfo.TYPE.STAIN, 0.27f, 0, 0.4f).LockF(40, false);

		public static BetoInfo BigBite = new BetoInfo(30f, C32.d2c(3388997632U), C32.d2c(3449373288U), 25f, BetoInfo.TYPE.SMOKE, 0.7f, 0, 0.2f);

		public static BetoInfo Ground = new BetoInfo(35f, C32.d2c(1432901201U), C32.d2c(4290616654U), 7f, BetoInfo.TYPE.STAIN, 1.65f, 0, 0.2f);

		public static BetoInfo GroundHard = new BetoInfo(60f, C32.d2c(2573751889U), C32.d2c(4290616654U), 100f, BetoInfo.TYPE.STAIN, 1.65f, 0, 0.2f);

		public static BetoInfo Blood = new BetoInfo(25f, C32.d2c(2009530428U), C32.d2c(1437079106U), 18f, BetoInfo.TYPE.STAIN, 0.72f, 0, 0.2f)
		{
			BloodReplaceCol = C32.d2c(2573036389U)
		};

		public static BetoInfo Lava = new BetoInfo(190f, C32.d2c(1430734663U), C32.d2c(4278190080U), 140f, BetoInfo.TYPE.STAIN, 2.2f, 1, 0.2f).LockF(120, false);

		public static BetoInfo Thunder = new BetoInfo(20f, C32.d2c(2003388485U), C32.d2c(4278190080U), 50f, BetoInfo.TYPE.STAIN, 0.7f, 1, 0.05f).LockF(120, false);

		public static BetoInfo Grab = new BetoInfo(30f, C32.d2c(860309319U), C32.d2c(4278190080U), 80f, BetoInfo.TYPE.STAIN, 2.2f, 1, 0.2f).LockF(120, false);

		public static BetoInfo SLASH = new BetoInfo(38f, C32.d2c(2861519699U), C32.d2c(4283957248U), 23f, BetoInfo.TYPE.CUTTED, 0.5f, 0, 0.33f)
		{
			BloodReplaceCol = C32.d2c(2859495536U)
		}.LockF(70, false);

		public static BetoInfo TORNADO = new BetoInfo(15f, C32.d2c(2865300484U), C32.d2c(4283957248U), 15f, BetoInfo.TYPE.CUTTED, 0.5f, 0, 2f)
		{
			BloodReplaceCol = C32.d2c(2862523299U)
		}.LockF(5, false);

		public static BetoInfo DarkTornado = new BetoInfo(15f, C32.d2c(3428872574U), C32.d2c(2291432327U), 12f, BetoInfo.TYPE.CUTTED, 0.55f, 0, 0.2f).LockF(5, false);

		public static BetoInfo FREEZE = new BetoInfo(0f, C32.d2c(uint.MaxValue), C32.d2c(uint.MaxValue), 0f, BetoInfo.TYPE.FROZEN, 1f, 0, 0.2f);

		public static BetoInfo STONE_WHOLE = new BetoInfo(0f, C32.d2c(4288716960U), C32.d2c(4278190080U), 0f, BetoInfo.TYPE.STONE_WHOLE, 1f, 0, 0.2f);

		public static BetoInfo WEB_TRAPPED = new BetoInfo(0f, C32.d2c(uint.MaxValue), C32.d2c(4287335047U), 75f, BetoInfo.TYPE.WEB_TRAPPED, 1.8f, 0, 0.78f);

		public static BetoInfo Vore = new BetoInfo(30f, C32.d2c(3405774847U), C32.d2c(4284119659U), 50f, BetoInfo.TYPE.LIQUID, 0.6f, 0, 0.7f);

		public const uint stonecol = 4288716960U;

		public enum TYPE : byte
		{
			SMOKE,
			LIQUID,
			STAIN,
			CUTTED,
			FROZEN,
			STONE_WHOLE,
			WEB_TRAPPED,
			_MAX
		}
	}
}
