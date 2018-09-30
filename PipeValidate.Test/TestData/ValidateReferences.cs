using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeValidate.Test.TestData
{
    public class ValidateReferences
    {

        public readonly PersonData Person;

        public ValidateReferences() { }
        public ValidateReferences(PersonData data)
        {
            this.Person = data;
        }

        public const string InValidAddress = "An valid address";

        public void AddInvalidAddress(Result ret)
        {
            if (Person?.Address?.Street == "")
            {
                ret.AddValidationMessage(InValidAddress);
            }
        }

    }
}
