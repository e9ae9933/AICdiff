using System;
using UnityEngine;

namespace XX
{
	public class VariableInstance : IVariableObject
	{
		public VariableInstance(string _name, string _value = "")
		{
			this.name = _name;
			this.value = _value;
		}

		public string getValueString()
		{
			return this.value;
		}

		public void setValue(string v)
		{
			this.value = v;
		}

		public GameObject getGob()
		{
			return null;
		}

		public string name;

		public string value;
	}
}
