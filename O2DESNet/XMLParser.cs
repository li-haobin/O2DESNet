using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace O2DESNet
{
    public class XMLParser<T>
    {
        public static void Serialize(T obj, string file)
        {
            using (var sw = new StreamWriter(string.Format(file)))
                new XmlSerializer(typeof(T)).Serialize(XmlWriter.Create(sw), obj);
        }
        // 读取 XML 文件还原成类
        public static T Deserialize(string file)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(new StreamReader(file));
        }
    }
}
