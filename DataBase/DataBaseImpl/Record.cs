using System;
using System.IO;
using System.Collections.Generic;

namespace DB
{
    public class Record : Dictionary<string, Field>
    {
        public List<Field> Key { get; protected set; }

        public void SetValues(params string[] fieldsValues)
        {
            int i = 0;
            foreach (string field in this.Keys)
            {
                this[field].Value = fieldsValues[i];
                i++;
            }
        }

        public override string ToString()
        {
            List<string> output = new List<string>();
            foreach (Field field in this.Values)
            {
                output.Add(field.Value);
            }
            return String.Join(", ", output);
        }

        public string GetKeyString()
        {
            return String.Join("^", Key);
        }

        public void ReadFromFile(StreamReader file)
        {
            foreach (string key in this.Keys)
                this[key].Value = file.ReadLine();
            file.Close();
        }

        public void WriteToFile(System.IO.StreamWriter file)
        {
            foreach (Field field in this.Values)
                file.WriteLine(field.Value);
            file.Close();
        }
    }
}