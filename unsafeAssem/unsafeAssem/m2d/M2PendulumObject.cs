using System;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2PendulumObject : MonoBehaviour, IM2RayHitAble, IRunAndDestroy
	{
		public void appear(M2CImgDrawerPendulumBell _Dr, Map2d _Mp)
		{
			this.Dr = _Dr;
			this.Mp = _Mp;
			this.sound_key = this.Dr.Meta.GetSi(1, "pendulum_bell");
			BoxCollider2D orAdd = IN.GetOrAdd<BoxCollider2D>(base.gameObject);
			float num = (float)(this.Dr.pdl_height * 2) * this.Mp.base_scale * 0.015625f;
			orAdd.size = new Vector2(num, num);
			orAdd.isTrigger = true;
			this.angle_spd = X.NIL(2f, 0.5f, (float)(this.Dr.Cp.iheight - this.Dr.pdl_height - 30), 90f);
			this.recalcPos();
			this.Mp.addRunnerObject(this);
		}

		public void recalcPos()
		{
			float num = (float)(this.Dr.Cp.iheight - this.Dr.pdl_height) * this.Mp.rCLEN;
			float num2 = this.rotR - 1.5707964f;
			this.mapcx = this.Dr.fulc_x + num * X.Cos(num2);
			this.mapcy = this.Dr.fulc_y - num * X.Sin(num2);
			IN.Pos2(base.transform, this.Mp.map2ux(this.mapcx), this.Mp.map2uy(this.mapcy));
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return 0f;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			return (RAYHIT)33;
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			if ((Ray.hittype & (HITTYPE.AUTO_TARGET | HITTYPE.TARGET_CHECKER)) == HITTYPE.NONE)
			{
				this.initVibU(Ray.Pos.x, Ray.Pos.y);
			}
			return HITTYPE.NONE;
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return 0;
		}

		public void destruct()
		{
			if (this.Mp == null)
			{
				return;
			}
			this.Mp.remRunnerObject(this);
			this.Mp = null;
		}

		public bool run(float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (this.rotR_spd != 0f || this.rotR != 0f)
			{
				this.Dr.Cp.activateToDrawer();
				float num = X.Abs(this.rotR_spd);
				float num2 = X.Abs(this.rotR);
				if (this.snd_t >= 0f)
				{
					if (TX.valid(this.sound_key))
					{
						M2SoundPlayerItem m2SoundPlayerItem = this.Mp.M2D.Snd.playAt(this.sound_key, this.Dr.unique_key, this.mapcx, this.mapcy, SndPlayer.SNDTYPE.SND, 1);
						if (m2SoundPlayerItem != null)
						{
							m2SoundPlayerItem.setVolManual(m2SoundPlayerItem.volume_maunal * (0.125f + 0.5f * X.ZPOW(num2 + num * 10f, 0.4f)), true);
						}
					}
					this.snd_t = -0.9424779f;
				}
				this.snd_t += num * fcnt;
				if (num <= 0.001256637f && num2 <= 0.006283186f)
				{
					this.rotR_spd = 0f;
					this.rotR = 0f;
					this.vib_lock_t = 0f;
					this.snd_t = 0f;
				}
				else
				{
					this.rotR_spd = X.absMn(this.rotR_spd + (float)X.MPF(this.rotR < 0f) * 0.00038f * this.angle_spd * 3.1415927f, 0.06283186f);
					this.rotR += this.rotR_spd * ((this.rotR_spd > 0f != this.rotR > 0f) ? 1.2f : 1f) * fcnt;
				}
				this.recalcPos();
				float num3 = this.Mp.map2meshx(this.Dr.fulc_x);
				float num4 = this.Mp.map2meshy(this.Dr.fulc_y);
				this.Dr.Cp.setMatrixToChipLight(Matrix4x4.Translate(new Vector3(num3, num4, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotR * 180f / 3.1415927f)) * Matrix4x4.Translate(new Vector3(-num3, -num4, 0f)));
			}
			if (this.vib_lock_t > 0f)
			{
				this.vib_lock_t -= fcnt;
			}
			return true;
		}

		public void initVibU(float ux, float uy)
		{
			this.initVib(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(ux)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(uy)));
		}

		public void initVib(float _mapcx, float _mapcy)
		{
			if (this.vib_lock_t > 0f)
			{
				return;
			}
			float num = X.correctangleR(this.rotR - 1.5707964f);
			float num2 = X.angledifR(num, this.Mp.GAR(this.Dr.fulc_x, this.Dr.fulc_y, _mapcx, _mapcy));
			float num3 = this.rotR_spd;
			if (num2 < 0f)
			{
				this.rotR_spd = X.Mx(this.rotR_spd, 0f) + X.NIXP(0.008f, 0.015f) * X.MMX(0.7f, this.angle_spd, 1.25f) * X.NIL(1f, 0.2f, num + 1.5707964f, 1.1938052f) * 3.1415927f;
			}
			else
			{
				this.rotR_spd = X.Mn(this.rotR_spd, 0f) - X.NIXP(0.008f, 0.015f) * X.MMX(0.7f, this.angle_spd, 1.25f) * X.NIL(1f, 0.2f, -(num + 1.5707964f), 1.1938052f) * 3.1415927f;
			}
			if (num3 == 0f || num3 < 0f != this.rotR_spd < 0f || this.rotR_spd / num3 > 1.8f)
			{
				this.snd_t = 0f;
			}
			this.vib_lock_t = 8f;
		}

		public void OnTriggerEnter2D(Collider2D ClOther)
		{
			if (ClOther.transform == null)
			{
				return;
			}
			Vector3 position = ClOther.transform.position;
			this.initVibU(position.x, position.y);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<PendulumObject> ";
				stb += base.name;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public float rotR
		{
			get
			{
				return this.Dr.rotR;
			}
			set
			{
				this.Dr.rotR = value;
			}
		}

		private Map2d Mp;

		private M2CImgDrawerPendulumBell Dr;

		private float rotR_spd;

		private float vib_lock_t = 15f;

		private float snd_t;

		private string sound_key;

		private float mapcx;

		private float mapcy;

		private float angle_spd;

		private string _tostring;
	}
}
