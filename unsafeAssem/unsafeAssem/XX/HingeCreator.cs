using System;
using UnityEngine;

namespace XX
{
	public class HingeCreator
	{
		public HingeCreator(string _header, PAUSER _Pauser = null)
		{
			this.Pauser = _Pauser;
			this.header = _header;
		}

		public HingeCreator BE(Rigidbody2D _Base, Rigidbody2D _End)
		{
			this.Base = _Base;
			this.End = _End;
			return this;
		}

		public void destructGob()
		{
			if (this.AHinge != null)
			{
				for (int i = this.AHinge.Length - 1; i >= 0; i--)
				{
					this.AHinge[i].destruct();
				}
			}
			this.AHinge = null;
		}

		public void destruct()
		{
			this.destructGob();
		}

		public HingeCreator CreateGob()
		{
			this.destructGob();
			this.AHinge = new HingeCreator.HingePt[this.resolution];
			Transform parent = this.Base.transform.parent;
			int num = ((this.hinge_layer < 0) ? this.End.gameObject.layer : this.hinge_layer);
			float num2 = ((this.linear_drag < 0f) ? this.End.drag : this.linear_drag);
			float num3 = ((this.angular_drag < 0f) ? this.End.angularDrag : this.angular_drag);
			float num4 = ((this.mass < 0f) ? this.End.mass : this.mass);
			Vector3 localScale = this.Base.transform.localScale;
			Vector2 vector = new Vector2(-this.max_length / (float)this.resolution, 0f);
			Rigidbody2D rigidbody2D = this.Base;
			Vector3 vector2 = this.Base.transform.localToWorldMatrix.MultiplyPoint3x4(this.BaseHingeAnchor_);
			for (int i = 0; i < this.resolution; i++)
			{
				GameObject gameObject = new GameObject(this.header + "_" + i.ToString());
				gameObject.layer = num;
				gameObject.transform.SetParent(parent, false);
				gameObject.transform.position = vector2;
				gameObject.transform.localScale = localScale;
				Rigidbody2D rigidbody2D2 = gameObject.AddComponent<Rigidbody2D>();
				rigidbody2D2.angularDrag = num3;
				rigidbody2D2.drag = num2;
				rigidbody2D2.mass = num4;
				rigidbody2D2.gravityScale = ((this.wire_gravity_scale_ < 0f) ? this.End.gravityScale : this.wire_gravity_scale_);
				HingeJoint2D hingeJoint2D = gameObject.AddComponent<HingeJoint2D>();
				hingeJoint2D.autoConfigureConnectedAnchor = false;
				if (i == 0)
				{
					if (this.first_lock_rotation_)
					{
						rigidbody2D2.freezeRotation = true;
					}
					hingeJoint2D.connectedAnchor = this.BaseHingeAnchor_;
					if (this.BaseHingeAnchor_.x != 0f || this.BaseHingeAnchor_.y != 0f)
					{
						this.first_agR = X.GAR(0f, 0f, this.BaseHingeAnchor_.x, this.BaseHingeAnchor_.y);
						vector = new Vector2(X.Cos(this.first_agR) * vector.x, X.Sin(this.first_agR) * vector.x);
					}
					hingeJoint2D.anchor = Vector2.zero;
				}
				else
				{
					hingeJoint2D.anchor = vector;
					hingeJoint2D.connectedAnchor = Vector2.zero;
				}
				hingeJoint2D.connectedBody = rigidbody2D;
				this.AHinge[i] = new HingeCreator.HingePt(hingeJoint2D, rigidbody2D2, this);
				rigidbody2D = rigidbody2D2;
			}
			this.End.transform.position = vector2;
			this.End.transform.localScale = localScale;
			this.LastHj = IN.GetOrAdd<HingeJoint2D>(this.End.gameObject);
			this.LastHj.autoConfigureConnectedAnchor = false;
			this.LastHj.connectedAnchor = this.LastHingeAnchor;
			this.LastHj.connectedBody = rigidbody2D;
			this.LastDj = IN.GetOrAdd<DistanceJoint2D>(this.End.gameObject);
			this.LastDj.autoConfigureConnectedAnchor = false;
			this.LastDj.autoConfigureDistance = false;
			this.LastDj.connectedBody = this.Base;
			if (this.max_length >= 0f)
			{
				this.LastDj.distance = this.max_length;
				this.LastDj.maxDistanceOnly = this.maxDistanceOnly_;
				this.LastDj.enabled = this.use_max_range;
			}
			if (this.link_enable_end)
			{
				this.End.gameObject.SetActive(this.enabled_);
			}
			return this;
		}

		public void PosReset()
		{
			if (this.AHinge != null)
			{
				Vector3 vector = this.Base.transform.localToWorldMatrix.MultiplyPoint3x4(this.BaseHingeAnchor_);
				for (int i = this.AHinge.Length - 1; i >= 0; i--)
				{
					Transform transform = this.AHinge[i].gameObject.transform;
					this.AHinge[i].R2D.velocity = Vector2.zero;
					transform.position = vector;
				}
				this.End.transform.position = vector;
				this.End.transform.localRotation = Quaternion.identity;
				this.End.velocity = Vector2.zero;
			}
		}

		public HingeCreator LgtReset(bool pos_fine = true, Vector2 ShiftU = default(Vector2))
		{
			if (this.AHinge != null)
			{
				this.LastDj.distance = this.max_length;
				float num = -this.max_length / (float)this.resolution;
				Vector2 vector = new Vector2(X.Cos(this.first_agR) * num, X.Sin(this.first_agR) * num);
				int num2 = this.AHinge.Length;
				Vector2 vector2 = ShiftU;
				Vector2 vector3 = Vector2.zero;
				float num3 = 0f;
				for (int i = 0; i < num2; i++)
				{
					HingeCreator.HingePt hingePt = this.AHinge[i];
					Vector2 vector4 = hingePt.transform.position;
					if (i == 0)
					{
						if (pos_fine)
						{
							Vector3 localPosition = hingePt.transform.localPosition;
							localPosition.x += vector2.x;
							localPosition.y += vector2.y;
							hingePt.transform.localPosition = localPosition;
							this.Base.transform.localToWorldMatrix.MultiplyPoint3x4(this.BaseHingeAnchor_);
							this.Base.transform.localToWorldMatrix.MultiplyPoint3x4(hingePt.connectedAnchor);
							vector2 += this.BaseHingeAnchor_ - hingePt.connectedAnchor;
						}
						hingePt.connectedAnchor = this.BaseHingeAnchor_;
					}
					else
					{
						if (pos_fine && i > 0 && hingePt.anchor != vector)
						{
							if (i == 1)
							{
								num3 = X.Abs(num) - hingePt.anchor.magnitude;
							}
							Vector3 localPosition2 = hingePt.transform.localPosition;
							localPosition2.x += vector2.x;
							localPosition2.y += vector2.y;
							hingePt.transform.localPosition = localPosition2;
							float num4 = X.GAR2(vector3.x, vector3.y, vector4.x, vector4.y);
							vector2 += new Vector2(X.Cos(num4), X.Sin(num4)) * num3;
						}
						hingePt.anchor = vector;
					}
					vector3 = vector4;
				}
				if (pos_fine)
				{
					Vector3 localPosition3 = this.LastHj.transform.localPosition;
					localPosition3.x += vector2.x;
					localPosition3.y += vector2.y;
					this.LastHj.transform.localPosition = localPosition3;
					this.End.AddForce(vector2 * 0.125f, ForceMode2D.Force);
				}
			}
			return this;
		}

		public Vector2 LastHingeAnchor
		{
			get
			{
				return this.LastHingeAnchor_;
			}
			set
			{
				this.LastHingeAnchor_ = value;
				if (this.AHinge != null)
				{
					this.LastHj.connectedAnchor = -value;
				}
			}
		}

		public Vector2 BaseHingeAnchor
		{
			get
			{
				return this.BaseHingeAnchor_;
			}
			set
			{
				this.BaseHingeAnchor_ = value;
				if (this.AHinge != null)
				{
					this.AHinge[0].Hj.connectedAnchor = this.BaseHingeAnchor_;
				}
			}
		}

		public float end_gravity_scale
		{
			get
			{
				return this.End.gravityScale;
			}
			set
			{
				if (this.End.gravityScale == value)
				{
					return;
				}
				this.End.gravityScale = value;
				if (this.AHinge == null)
				{
					return;
				}
				int num = this.AHinge.Length;
				for (int i = 0; i < num; i++)
				{
					this.AHinge[i].R2D.gravityScale = value;
				}
			}
		}

		public bool enabled
		{
			get
			{
				return this.enabled_;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				this.enabled_ = value;
				if (this.AHinge == null)
				{
					return;
				}
				int num = this.AHinge.Length;
				for (int i = 0; i < num; i++)
				{
					this.AHinge[i].gameObject.SetActive(value);
				}
				if (this.link_enable_end)
				{
					this.End.gameObject.SetActive(value);
				}
			}
		}

		public bool first_lock_rotation
		{
			get
			{
				return this.first_lock_rotation_;
			}
			set
			{
				if (this.first_lock_rotation == value)
				{
					return;
				}
				this.first_lock_rotation_ = value;
				if (this.AHinge == null)
				{
					return;
				}
				this.AHinge[0].R2D.freezeRotation = value;
			}
		}

		public float wire_gravity_scale
		{
			get
			{
				return this.wire_gravity_scale_;
			}
			set
			{
				if (this.wire_gravity_scale == value)
				{
					return;
				}
				this.wire_gravity_scale_ = value;
				if (this.AHinge == null)
				{
					return;
				}
				int num = this.AHinge.Length;
				for (int i = 0; i < num; i++)
				{
					this.AHinge[i].gravityScale = ((this.wire_gravity_scale_ < 0f) ? this.End.gravityScale : this.wire_gravity_scale_);
				}
			}
		}

		public bool maxDistanceOnly
		{
			get
			{
				return this.maxDistanceOnly_;
			}
			set
			{
				if (this.maxDistanceOnly == value)
				{
					return;
				}
				this.maxDistanceOnly_ = value;
				if (this.AHinge == null)
				{
					return;
				}
				this.LastDj.maxDistanceOnly = value;
			}
		}

		public float first_hinge_agR
		{
			set
			{
				if (this.AHinge == null)
				{
					return;
				}
				this.AHinge[0].gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, value * 180f / 3.1415927f));
			}
		}

		public bool DrawBegin(out Vector2 Pos)
		{
			Pos = Vector2.zero;
			if (this.AHinge == null)
			{
				return false;
			}
			Pos = this.AHinge[0].transform.position;
			this.draw_i = 0;
			return true;
		}

		public bool DrawNext(out Vector2 Pos)
		{
			Pos = Vector2.zero;
			if (this.draw_i < 0 || this.draw_i > this.AHinge.Length)
			{
				return false;
			}
			Vector2 zero = Vector2.zero;
			HingeJoint2D hingeJoint2D;
			if (this.draw_i == this.AHinge.Length)
			{
				hingeJoint2D = this.LastHj;
				Vector2 lastHingeAnchor_ = this.LastHingeAnchor_;
			}
			else
			{
				hingeJoint2D = this.AHinge[this.draw_i].Hj;
			}
			this.draw_i++;
			Pos = hingeJoint2D.transform.position;
			return true;
		}

		public Rigidbody2D Base;

		public Rigidbody2D End;

		private DistanceJoint2D LastDj;

		private HingeJoint2D LastHj;

		public string header;

		public bool use_max_range = true;

		private bool first_lock_rotation_;

		private bool maxDistanceOnly_ = true;

		public int resolution = 8;

		public int hinge_layer = -1;

		public float max_length = 3.2f;

		public float mass = -1f;

		public float angular_drag = 0.5f;

		private float wire_gravity_scale_ = -1f;

		private float first_agR;

		public float linear_drag = 1f;

		public Vector2 BaseHingeAnchor_;

		private Vector2 LastHingeAnchor_;

		public PAUSER Pauser;

		private HingeCreator.HingePt[] AHinge;

		public bool link_enable_end;

		private bool enabled_ = true;

		private int draw_i = -1;

		private class HingePt : PauseMemItemRigidbody
		{
			public HingePt(HingeJoint2D _Hj, Rigidbody2D _Rgd, HingeCreator _Con)
				: base(_Rgd)
			{
				this.Hj = _Hj;
				this.Con = _Con;
				if (this.Con.Pauser != null)
				{
					this.Con.Pauser.Assign(this);
				}
				if (!this.Con.enabled)
				{
					this.gameObject.SetActive(false);
				}
			}

			public void destruct()
			{
				if (this.Con.Pauser != null)
				{
					this.Con.Pauser.Deassign(this);
				}
				IN.DestroyOne(this.gameObject);
			}

			public Vector2 anchor
			{
				get
				{
					return this.Hj.anchor;
				}
				set
				{
					this.Hj.anchor = value;
				}
			}

			public Vector2 connectedAnchor
			{
				get
				{
					return this.Hj.connectedAnchor;
				}
				set
				{
					this.Hj.connectedAnchor = value;
				}
			}

			public float gravityScale
			{
				get
				{
					return this.R2D.gravityScale;
				}
				set
				{
					this.R2D.gravityScale = value;
				}
			}

			public float rotR
			{
				get
				{
					return this.R2D.rotation * 3.1415927f / 180f;
				}
			}

			public GameObject gameObject
			{
				get
				{
					return this.R2D.gameObject;
				}
			}

			public Transform transform
			{
				get
				{
					return this.R2D.transform;
				}
			}

			public HingeJoint2D Hj;

			public HingeCreator Con;
		}
	}
}
