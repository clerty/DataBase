using System.Collections.Generic;

namespace DB
{
    public enum Cardinality { OneToOne, OneToMany, ManyToMany }
    public abstract class Relationship
    {
        public Record Parent { get; protected set; }
        public Cardinality Cardinality { get; protected set; }
        public List<Record> Childs { get; protected set; }

        public Relationship()
        {
            Childs = new List<Record>();
        }

        public void AddMembers(Record parent, params Record[] childs)
        {
            Parent = parent;
            foreach (Record child in childs)
                Childs.Add(child);
        }
        public void WriteToFile(System.IO.StreamWriter file)
        {
            foreach (Record connection in Childs)
                file.WriteLine(connection.GetKeyString());
            file.Close();
        }
    }
}
