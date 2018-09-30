using System;
using System.Collections.Generic;
using System.Text;

namespace PipeValidate
{
	
	public interface IValidatorResults
	{
		List<string> Messages { get; set; }
	}
}
