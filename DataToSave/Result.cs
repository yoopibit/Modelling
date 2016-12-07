using DataToSave.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataToSave
{
    [DataContract]
    public class Result
    {
        [DataMember]
        public IEnumerable<String> GamerNames { get; set; }
        public IEnumerable<int> CurrentResult { get; set; }

        public static string DataPath = "result.dat";

        public static Result Load()
        {
            if (File.Exists(DataPath))
            {
                return (Result)DataSerializer.DeserializeItem(DataPath, typeof(Result));
            }
            return new Result();
        }
        public void Save()
        {
            DataSerializer.SerializeData(DataPath, typeof(Result), this);
        }
    }
}
