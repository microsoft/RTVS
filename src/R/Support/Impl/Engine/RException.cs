using System;

namespace Microsoft.R.Support.Engine
{
	public class RException : Exception
	{
		public RException(string message) : base(message)
		{
			
		}
	}
}