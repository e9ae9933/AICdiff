using System;
using UnityEngine;

namespace XX
{
	public class HideScreen : MonoBehaviourAutoRun, IClickable, IValotileSetable
	{
		protected virtual void Awake()
		{
			base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			X.SCLOCK(this);
		}

		public Color32 Col
		{
			get
			{
				return this.Col_;
			}
			set
			{
				this.Col_ = value;
				if (this.Col_.a == 0)
				{
					if (this.Md != null)
					{
						this.Md.clear(false, false);
						return;
					}
				}
				else
				{
					this.initMeshDrawer(true, false);
					if (this.hide_time >= 0f)
					{
						this.hide_time = X.Mx(this.hide_time, 0f);
						return;
					}
					this.hide_time = X.Mn(this.hide_time, -1f);
				}
			}
		}

		protected void initMeshDrawer(bool md = true, bool valot = false)
		{
			if (md && this.Md == null)
			{
				this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				this.Mrd = base.GetComponent<MeshRenderer>();
				if (this.Valot != null)
				{
					this.Valot.InitUI(this.Md, this.Mrd);
				}
			}
			if (valot && this.Valot == null)
			{
				this.Valot = IN.GetOrAdd<ValotileRenderer>(base.gameObject);
				if (this.Md != null)
				{
					this.Valot.InitUI(this.Md, this.Mrd);
				}
				if (!this.valotize_)
				{
					this.Valot.enabled = false;
				}
			}
		}

		public void setZ(float z)
		{
			base.transform.position = new Vector3(0f, 0f, z);
		}

		public float base_z
		{
			get
			{
				return this.Md.base_z;
			}
			set
			{
				this.Md.base_z = value;
			}
		}

		public virtual void deactivate(bool immediate = false)
		{
			if (this.hide_time >= 0f)
			{
				if (this.hide_time > (float)this.HIDE_MAXT && this.FnCompletelyVisible != null)
				{
					this.FnCompletelyVisible(false);
				}
				this.hide_time = ((immediate || this.Col_.a == 0) ? X.Mn((float)(-(float)this.HIDE_MAXT + 1), this.hide_time) : X.Mn(-1f, (float)(-(float)this.HIDE_MAXT) + this.hide_time));
			}
			X.REMLOCK(this);
			IN.Click.remClickable(this, false);
		}

		public virtual void activate()
		{
			if (this.hide_time < 0f)
			{
				this.hide_time = ((this.Col_.a == 0) ? ((float)this.HIDE_MAXT) : X.Mx(0f, (float)this.HIDE_MAXT + this.hide_time));
			}
			X.SCLOCK(this);
			base.gameObject.SetActive(true);
			IN.Click.addClickable(this);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (this.hide_time == -1001f)
			{
				this.hide_time = -1000f;
				return;
			}
			if (this.hide_time < 0f)
			{
				this.activate();
				if (this.Md != null)
				{
					this.Md.clear(false, false);
				}
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();
			this.deactivate(true);
			MeshDrawer md = this.Md;
		}

		public bool full_shown
		{
			get
			{
				return this.hide_time >= (float)this.HIDE_MAXT;
			}
		}

		public bool full_hidden
		{
			get
			{
				return this.hide_time <= (float)(-(float)this.HIDE_MAXT);
			}
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.Md == null || this.hide_time <= -1000f)
			{
				if (base.enabled && this.hide_time >= 0f)
				{
					return true;
				}
				base.gameObject.SetActive(false);
				return false;
			}
			else
			{
				if (base.enabled)
				{
					if (this.hide_time >= 0f && this.hide_time <= (float)this.HIDE_MAXT)
					{
						if (this.Md == null || this.Col_.a == 0)
						{
							this.hide_time = (float)this.HIDE_MAXT;
						}
						else
						{
							this.hide_time += fcnt;
							if (this.FnCompletelyVisible != null && this.hide_time > (float)this.HIDE_MAXT)
							{
								this.FnCompletelyVisible(true);
							}
							this.drawToMd(X.ZLINE(this.hide_time, (float)this.HIDE_MAXT));
							this.Md.updateForMeshRenderer(false);
						}
					}
					else if (this.hide_time < 0f)
					{
						if (this.hide_time <= (float)(-(float)this.HIDE_MAXT) || this.Md == null || this.Col_.a == 0)
						{
							this.hide_time = (float)(-(float)this.HIDE_MAXT);
							base.gameObject.SetActive(false);
							return false;
						}
						this.hide_time -= fcnt;
						this.drawToMd(X.ZLINE((float)this.HIDE_MAXT + this.hide_time, (float)this.HIDE_MAXT));
						this.Md.updateForMeshRenderer(false);
					}
					return true;
				}
				return false;
			}
		}

		protected virtual void drawToMd(float alpha)
		{
			this.Md.Col = C32.MulA(this.Col_, alpha);
			this.Md.Rect(0f, 0f, IN.w + IN.wh, IN.h + IN.hh, false);
		}

		public void immediateFillColor()
		{
			if (this.hide_time > -1000f)
			{
				if (this.hide_time >= 0f && this.hide_time <= (float)this.HIDE_MAXT)
				{
					this.hide_time = (float)(this.HIDE_MAXT + 1);
					if (this.FnCompletelyVisible != null)
					{
						this.FnCompletelyVisible(true);
					}
					this.drawToMd(1f);
					this.Md.updateForMeshRenderer(false);
					return;
				}
				if (this.hide_time < 0f)
				{
					this.hide_time = (float)(-(float)this.HIDE_MAXT);
				}
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			X.REMLOCK(this);
			if (this.Md != null)
			{
				this.Md.destruct();
			}
		}

		public bool isActive()
		{
			return this.hide_time >= 0f;
		}

		public bool use_valotile
		{
			get
			{
				return this.valotize_;
			}
			set
			{
				if (this.valotize_ == value)
				{
					return;
				}
				this.valotize_ = value;
				if (this.valotize_)
				{
					this.initMeshDrawer(true, true);
					this.Valot.enabled = true;
					return;
				}
				if (this.Valot != null)
				{
					this.Valot.enabled = false;
				}
			}
		}

		public bool isCompletelyShown()
		{
			return this.hide_time >= (float)this.HIDE_MAXT;
		}

		public bool getClickable(Vector2 PosU, out IClickable Res)
		{
			if (this.hide_time >= 0f)
			{
				Res = this;
				return true;
			}
			Res = null;
			return false;
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void OnPointerEnter()
		{
		}

		public float getFarLength()
		{
			return base.transform.position.z;
		}

		public void OnPointerExit()
		{
		}

		public virtual bool OnPointerDown()
		{
			return false;
		}

		public void OnPointerUp(bool clicking)
		{
		}

		public Action<bool> FnCompletelyVisible;

		private Color32 Col_;

		private float hide_time = -1001f;

		public int HIDE_MAXT = 20;

		protected MeshDrawer Md;

		protected MeshRenderer Mrd;

		private bool valotize_;

		protected ValotileRenderer Valot;
	}
}
