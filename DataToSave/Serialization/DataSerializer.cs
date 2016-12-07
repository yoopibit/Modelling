using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataToSave.Serialization
{
    [DataContract]
    public class DataSerializer
    {
        public static void SerializeData(string fileName, Type type, object data)
        {
            var formatter = new DataContractSerializer(type);
            var s = new FileStream(fileName, FileMode.Create);
            formatter.WriteObject(s, data);
            s.Close();
        }

        public static object DeserializeItem(string fileName, Type type)
        {
            var s = new FileStream(fileName, FileMode.Open);
            var formatter = new DataContractSerializer(type);
            return formatter.ReadObject(s);
        }
    }
}