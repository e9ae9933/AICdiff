using System;

namespace XX
{
	public struct CsvVariableItem
	{
		public void Set(string _name, string _val)
		{
			this.name = _name;
			this.val = _val;
		}

		public void Set(CsvVariableItem Ci)
		{
			this.name = Ci.name;
			this.val = Ci.val;
		}

		public override string ToString()
		{
			return "<CsvVariableItem>" + this.name + " = " + this.val;
		}

		public string name;

		public string val;
	}
}
