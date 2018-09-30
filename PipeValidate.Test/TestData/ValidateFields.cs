using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeValidate.Test.TestData
{
	public class ValidateFields
	{
        public readonly PersonData Person;

        public ValidateFields() { }
        public ValidateFields(PersonData data) {
            this.Person = data;
        }

        public const string ExceptionMessage = "an exception message";
        public const string PersonNoValidEmail = "Person must have an email";
        public const string PersonNoValidName = "Person must have a name";
        public const string InvalidAge = "An invalid age";

        public static void AddInvalidAge(Result ret)
        {
        	ret.AddValidationMessage(InvalidAge);
        }

        public static void ValidationThatThrowsAnException()
        {
        	throw new InvalidOperationException(ExceptionMessage);
        }

        public void ValidateEmail(Result ret)
		{
            if (string.IsNullOrWhiteSpace(this.Person.Email))
            {
                ret.AddValidationMessage(PersonNoValidEmail);
            }
        }
		public void ValidatePerson(Result ret)
		{
            if (string.IsNullOrWhiteSpace(this.Person.Name))
            {
                ret.AddValidationMessage(PersonNoValidName);
            }
		}

	}
}
