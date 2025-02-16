using System;
using UnityEngine;

namespace XX
{
	public abstract class MonoBehaviourAutoRun : MonoBehaviour, IRunAndDestroy
	{
		public override string ToString()
		{
			if (this._tostring == null)
			{
				this._tostring = base.gameObject.name;
			}
			return this._tostring;
		}

		public virtual void OnEnable()
		{
			this.runner_assigned = true;
		}

		public virtual void OnDisable()
		{
			this.runner_assigned = false;
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					IN.addRunner(this);
					return;
				}
				IN.remRunner(this);
			}
		}

		public virtual void OnDestroy()
		{
			this.OnDisable();
		}

		bool IRunAndDestroy.run(float fcnt)
		{
			if (!this.runIRD(fcnt))
			{
				this.runner_assigned_ = false;
				return false;
			}
			return true;
		}

		protected abstract bool runIRD(float fcnt);

		public virtual void destruct()
		{
			this.OnDisable();
		}

		private bool runner_assigned_;

		private string _tostring;
	}
}
