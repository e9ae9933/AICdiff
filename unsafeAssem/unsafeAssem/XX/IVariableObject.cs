using System;
using UnityEngine;

namespace XX
{
	public interface IVariableObject
	{
		string getValueString();

		void setValue(string s);

		GameObject getGob();
	}
}
