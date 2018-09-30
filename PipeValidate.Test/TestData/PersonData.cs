using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace PipeValidate.Test.TestData
{
    public class PersonData
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public bool IsStudent { get; set; }
        public PersonData(string email, string name, string phone, int age, bool isStudent)
        {
            Email = email;
            Name = name;
            Phone = phone;
            Age = age;
            IsStudent = isStudent;
        }

    }
    public class Address {
        public string Street { get; set; }
        public Address(string street) {
            this.Street = street;
        }
    }
}
