using System;
using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Customer : Record
    {
        public Customer()
            : base()
        {
            this.Add("Name", new Field(typeof(string)));//FieldType.Varchar
            this.Add("Address", new Field(typeof(string)));//FieldType.Varchar
            Key = new List<Field>();
            Key.Add(this["Name"]);
            Key.Add(this["Address"]);
        }
    }
}
