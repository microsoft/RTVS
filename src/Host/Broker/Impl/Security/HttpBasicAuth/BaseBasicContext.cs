// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odachi.AspNetCore.Authentication.Basic
{
	public class BaseBasicContext : BaseControlContext
	{
		public BaseBasicContext(HttpContext context, BasicOptions options)
			: base(context)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Options = options;
		}

		public BasicOptions Options { get; }
	}
}
