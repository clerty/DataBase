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

        List<Record> createdRecords, deletedRecords;
        List<Relationship> addedRelationships, changedRelationships;

        public DataBase()
        {
            Records = new Dictionary<Type, List<Record>>();
            Relationships = new Dictionary<Type, List<Relationship>>();
            createdRecords = new List<Record>();
            deletedRecords = new List<Record>();
            addedRelationships = new List<Relationship>();
            changedRelationships = new List<Relationship>();
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

        public void Fill(string path)
        {
            Path = path;
            FillRecords();
            FillRelationships();
        }

        void FillRecords()
        {
            Assembly typeAssembly = this.GetType().Assembly;
            string recordTypeString;
            Type recordType;
            Record recordToAdd;
            List<Record> recordsToAdd;

            string recordsDirectory = System.IO.Path.Combine(Path, "Records");
            if (Directory.Exists(recordsDirectory))
                foreach (string recordDirectory in Directory.GetDirectories(recordsDirectory))
                {
                    recordTypeString = recordDirectory.Substring(recordDirectory.LastIndexOf('\\') + 1);
                    recordType = typeAssembly.GetType(typeAssembly.GetName().Name + "." + recordTypeString);

                    recordsToAdd = new List<Record>();
                    foreach (string recordFile in Directory.GetFiles(recordDirectory))
                    {
                        recordToAdd = (Record)Activator.CreateInstance(recordType);
                        recordToAdd.ReadFromFile(new StreamReader(recordFile));
                        recordsToAdd.Add(recordToAdd);
                    }

                    Records[recordType] = recordsToAdd;
                }
        }

        void FillRelationships()
        {
            Assembly typeAssembly = this.GetType().Assembly;
            string relationshipTypeString;
            Type relationshipType;
            Relationship relationship;
            string recordTypeString;
            string[] recordFields;
            Record recordToAdd;
            List<Record> recordsToAdd;

            string relationshipsDirectory = System.IO.Path.Combine(Path, "Relationships");
            if (Directory.Exists(relationshipsDirectory))
                foreach (string relationshipDirectory in Directory.GetDirectories(relationshipsDirectory))
                {
                    relationshipTypeString = relationshipDirectory.Substring(relationshipDirectory.LastIndexOf('\\') + 1);
                    relationshipType = typeAssembly.GetType(typeAssembly.GetName().Name + "." + relationshipTypeString);
                    Relationships.Add(relationshipType, new List<Relationship>());

                    foreach (string relationshipFile in Directory.GetFiles(relationshipDirectory))
                    {
                        relationship = (Relationship)Activator.CreateInstance(relationshipType);

                        recordTypeString = relationshipTypeString.Substring(0, relationshipTypeString.IndexOf('_'));
                        recordFields = relationshipFile.Substring(relationshipFile.LastIndexOf('\\') + 1, relationshipFile.LastIndexOf('.') - relationshipFile.LastIndexOf('\\') - 1).Split('^');
                        recordToAdd = (Record)typeof(DataBase).GetMethod("FindByKey").MakeGenericMethod(typeAssembly.GetType(typeAssembly.GetName().Name + "." + recordTypeString)).Invoke(this, new object[] { recordFields });

                        recordsToAdd = new List<Record>();
                        recordTypeString = relationshipTypeString.Substring(relationshipTypeString.IndexOf('_') + 1);
                        foreach (string recordString in File.ReadAllLines(relationshipFile))
                            recordsToAdd.Add((Record)typeof(DataBase).GetMethod("FindByKey").MakeGenericMethod(typeAssembly.GetType(typeAssembly.GetName().Name + "." + recordTypeString)).Invoke(this, new object[] { recordString.Split('^') }));

                        relationship.AddMembers(recordToAdd, recordsToAdd.ToArray());

                        Relationships[relationshipType].Add(relationship);
                    }
                }
        }

        public Record Create<T>(params string[] fieldsValues) where T : Record, new()
        {
            Record record = new T();
            record.SetValues(fieldsValues);

            if (!Records.ContainsKey(typeof(T)))
                Records.Add(typeof(T), new List<Record>());
            Records[typeof(T)].Add(record);
            createdRecords.Add(record);

            return record;
        }

        public void AddToRelationship<T>(Record parent, params Record[] childs) where T : Relationship, new()
        {
            Relationship relationship = new T();

            if (!Relationships.ContainsKey(typeof(T)))
                Relationships.Add(typeof(T), new List<Relationship>());

            relationship = Relationships[typeof(T)].Find(rs => rs.Parent == parent);
            if (relationship.Equals(default(Relationship)))
            {
                relationship.AddMembers(parent, childs);
                Relationships[typeof(T)].Add(relationship);
                addedRelationships.Add(relationship);
            }
            else
            {
                relationship.Childs.AddRange(childs);
                changedRelationships.Add(relationship);
            }


            //string path = System.IO.Path.Combine(Path, "Relationships", typeof(T).Name, relationship.Parent.GetKeyString() + "_temp.txt");
            //FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            //relationship.WriteToFile(new StreamWriter(fs));
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
            deletedRecords.Add(record);

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

        public List<Record> FindByField<T>(string field, string value) where T : Record
        {
            return Records[typeof(T)].FindAll(record => record[field].Value == value);
        }

        public Record FindByKey<T>(params string[] values) where T : Record
        {
            return Records[typeof(T)].Find(record => record.Key.ConvertAll<string>(x => x.Value).ToArray().SequenceEqual(values));
        }

        
        public void Save()
        {
            string path = System.IO.Path.Combine(Path, "Records");
            FileStream fs;

            foreach (Record createdRecord in createdRecords)
            {
                fs = new FileStream(System.IO.Path.Combine(path, createdRecord.GetType().Name, createdRecord.GetKeyString()), FileMode.Create, FileAccess.Write);
                createdRecord.WriteToFile(new StreamWriter(fs));
            }
            foreach (Record deletedRecord in deletedRecords)
            {

            }

            path = System.IO.Path.Combine(Path, "Relationshops");
            foreach (Relationship addedRelationship in addedRelationships)
            {
                
            }
            foreach (Relationship changedRelationship in changedRelationships)
            {
                
            }
        }
    }
}
