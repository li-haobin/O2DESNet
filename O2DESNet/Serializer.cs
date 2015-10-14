using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Serializer
    {
        public static bool WriteTo(object obj, string fileName)
        {
            return ByteArrayToFile(fileName, ObjectToByteArray(obj));
        }
        public static T ReadFrom<T>(string fileName)
        {
            return (T)ByteArrayToObject(File.ReadAllBytes(fileName));
        }

        private static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                FileStream _FileStream =
                   new FileStream(fileName, FileMode.Create,
                                            FileAccess.Write);
                _FileStream.Write(byteArray, 0, byteArray.Length);
                _FileStream.Close();
                return true;
            }
            catch (Exception _Exception)
            {
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }
            return false;
        }

        // Convert an object to a byte array
        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object)binForm.Deserialize(memStream);
            return obj;
        }
    }
}
