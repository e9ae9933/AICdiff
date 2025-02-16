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
			if (!this.runner_assigned)
			{
				this.runner_assigned = true;
				IN.addRunner(this);
			}
		}

		public virtual void OnDisable()
		{
			if (this.runner_assigned)
			{
				this.runner_assigned = false;
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
				this.runner_assigned = false;
				return false;
			}
			return true;
		}

		protected abstract bool runIRD(float fcnt);

		public virtual void destruct()
		{
			this.OnDisable();
		}

		private bool runner_assigned;

		private string _tostring;
	}
}
