using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeValidate.Test.TestData
{
	public class Result : IValidatorResults
	{
		public bool IsValid => Messages.Count == 0;
		public Result()
		{
			Messages = new List<string>();
		}

		public List<string> Messages { get; set; }

		public void AddValidationMessage(string message)
		{
			Messages.Add((message));
		}
	}
}
