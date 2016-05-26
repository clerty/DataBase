using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DB
{
    public class DataBase
    {
        public string Path { get; protected set; }
        public Dictionary<Type, List<Record>> Records { get; private set; }
        public Dictionary<Type, List<Relationship>> Relationships { get; private set; }

        public DataBase()
        {
            Records = new Dictionary<Type, List<Record>>();
            Relationships = new Dictionary<Type, List<Relationship>>();
            Path = String.Empty;
        }

        public void CreateNew(string path)
        {
            string dbPath = System.IO.Path.Combine(path, this.GetType().Name);
            Directory.CreateDirectory(dbPath);
            Directory.CreateDirectory(System.IO.Path.Combine(dbPath, "Records"));
            Directory.CreateDirectory(System.IO.Path.Combine(dbPath, "Relationships"));
            Path = dbPath;
        }

        public Record Create<T>(params string[] fieldsValues) where T : Record, new()
        {
            Record record = new T();
            record.SetValues(fieldsValues);

            if (!Records.ContainsKey(typeof(T)))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(Path, "Records", typeof(T).Name));
                Records.Add(typeof(T), new List<Record>());
            }
            Records[typeof(T)].Add(record);

            string path = System.IO.Path.Combine(Path, "Records", typeof(T).Name, record.GetKeyString() + "_temp.txt");
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            record.WriteToFile(new StreamWriter(fs));

            return record;
        }

        public void AddToRelationship<T>(Record parent, params Record[] childs) where T : Relationship, new()
        {
            Relationship relationship = new T();
            relationship.AddMembers(parent, childs);

            if (!Relationships.ContainsKey(typeof(T)))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(Path, "Relationships", typeof(T).Name));
                Relationships.Add(typeof(T), new List<Relationship>());
            }
            bool found = false;
            foreach (Relationship rs in Relationships[typeof(T)])
            {
                found |= rs.Parent == parent;
                if (found)
                {
                    foreach (Record record in childs)
                        rs.Childs.Add(record);
                    break;
                }
            }
            if (!found)
                Relationships[typeof(T)].Add(relationship);


            string path = System.IO.Path.Combine(Path, "Relationships", typeof(T).Name, relationship.Parent.GetKeyString() + "_temp.txt");
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            relationship.WriteToFile(new StreamWriter(fs));
        }

        public List<Record> FindByField<T>(string field, string value) where T : Record
        {
            List<Record> foundRecords = new List<Record>();
            foreach (Record record in Records[typeof(T)])
                if (record[field].Value == value)
                    foundRecords.Add(record);
            return foundRecords;
        }

        public Record FindByKey<T>(params string[] values) where T : Record
        {
            foreach (Record record in Records[typeof(T)])
            {
                //foreach (Field field in record.Key)
                //{
                //    found &= field.Value == values[i];
                //    if (!found)
                //        break;
                //    i++;
                //}
                string[] fields = record.Key.ConvertAll<string>(x => x.Value).ToArray();
                if (fields.SequenceEqual(values))
                    return record;
            }
            return null;
        }

        //void RemoveChildFromRelationship<T>(Record record) where T : Relationship
        //{
        //    foreach (Relationship relationship in Relationships[typeof(T)])
        //        if (relationship.Childs.Contains(record))
        //        {
        //            relationship.Childs.Remove(record);
        //            return;
        //        }
        //}

        public void Delete(Record record)
        {
            Records[record.GetType()].Remove(record);

            foreach (Type type in Relationships.Keys)
                foreach (Relationship relationship in Relationships[type])
                    if (relationship.Parent == record)
                    {
                        Relationships[type].Remove(relationship);
                        if (Relationships[type].Count == 0)
                            Relationships.Remove(type);
                        return;
                    }
                    else if (relationship.Childs.Contains(record))
                    {
                        relationship.Childs.Remove(record);
                        if (relationship.Childs.Count == 0)
                            Relationships[type].Remove(relationship);
                        if (Relationships[type].Count == 0)
                            Relationships.Remove(type);
                        return;
                    }
        }

        public void Fill<T>(string path) where T : DataBase
        {
            Assembly typeAssembly = typeof(T).Assembly;

            string typeString;
            Type type;
            List<Record> recordsToAdd;
            Record recordToAdd;

            foreach (string recordDirectory in Directory.GetDirectories(System.IO.Path.Combine(path, "Records")))
            {
                typeString = recordDirectory.Substring(recordDirectory.LastIndexOf('\\') + 1);
                type = typeAssembly.GetType(typeAssembly.GetName().Name + "." + typeString);

                recordsToAdd = new List<Record>();
                foreach (string recordFile in Directory.GetFiles(recordDirectory))
                {
                    recordToAdd = (Record)Activator.CreateInstance(type);
                    recordToAdd.ReadFromFile(new StreamReader(recordFile));
                    recordsToAdd.Add(recordToAdd);
                }

                Records[type] = recordsToAdd;
            }

            Relationship relationship;
            string[] recordFields;
            string recordTypeString;

            foreach (string relationshipDirectory in Directory.GetDirectories(System.IO.Path.Combine(path, "Relationships")))
            {
                typeString = relationshipDirectory.Substring(relationshipDirectory.LastIndexOf('\\') + 1);
                typeAssembly = typeof(T).Assembly;
                type = typeAssembly.GetType(typeAssembly.GetName().Name + "." + typeString);
                Relationships.Add(type, new List<Relationship>());

                foreach (string relationshipFile in Directory.GetFiles(relationshipDirectory))
                {
                    relationship = (Relationship)Activator.CreateInstance(type);

                    recordTypeString = typeString.Substring(0, typeString.IndexOf('_'));
                    recordFields = relationshipFile.Substring(relationshipFile.LastIndexOf('\\') + 1, relationshipFile.LastIndexOf('.') - relationshipFile.LastIndexOf('\\') - 1).Split('^');
                    recordToAdd = (Record)typeof(DataBase).GetMethod("FindByKey").MakeGenericMethod(typeAssembly.GetType(typeAssembly.GetName().Name + "." + recordTypeString)).Invoke(this, new object[] { recordFields });

                    recordsToAdd = new List<Record>();
                    recordTypeString = typeString.Substring(typeString.IndexOf('_') + 1);
                    foreach (string recordString in File.ReadAllLines(relationshipFile))
                        recordsToAdd.Add((Record)typeof(DataBase).GetMethod("FindByKey").MakeGenericMethod(typeAssembly.GetType(typeAssembly.GetName().Name + "." + recordTypeString)).Invoke(this, new object[] { recordString.Split('^') }));

                    relationship.AddMembers(recordToAdd, recordsToAdd.ToArray());

                    Relationships[type].Add(relationship);
                }
            }
        }
    }
}
