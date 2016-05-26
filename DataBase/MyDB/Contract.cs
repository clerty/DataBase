using System;
using System.Collections.Generic;
using DB;

namespace MyDB
{
    public class Contract : Record
    {
        public Contract()
        {
            this.Add("Number", new Field(typeof(int)));//FieldType.Integer
            this.Add("Date", new Field(typeof(DateTime)));//FieldType.DateTime
            this.Add("Cost", new Field(typeof(int)));//FieldType.Integer
            Key = new List<Field>();
            Key.Add(this["Number"]);
        }
    }
}
