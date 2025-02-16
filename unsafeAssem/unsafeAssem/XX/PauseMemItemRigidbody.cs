using System;
using UnityEngine;

namespace XX
{
	public class PauseMemItemRigidbody : PauseMemItem
	{
		public PauseMemItemRigidbody(Rigidbody2D _R2D)
		{
			this.R2D = _R2D;
		}

		public override void Pause()
		{
			if (this.sleep_set)
			{
				return;
			}
			this.angularVelocity = this.R2D.angularVelocity;
			this.velocity = this.R2D.velocity;
			this.gravityScale = this.R2D.gravityScale;
			this.isKinematic = this.R2D.isKinematic;
			this.R2D.isKinematic = true;
			this.R2D.Sleep();
			this.sleep_set = true;
		}

		public override void Resume()
		{
			if (!this.sleep_set)
			{
				return;
			}
			this.R2D.angularVelocity = this.angularVelocity;
			this.R2D.velocity = this.velocity;
			this.R2D.isKinematic = this.isKinematic;
			this.R2D.WakeUp();
			this.sleep_set = false;
		}

		public readonly Rigidbody2D R2D;

		private float angularVelocity;

		private Vector2 velocity;

		private float gravityScale;

		public bool isKinematic;

		private bool sleep_set;
	}
}
