using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GU2V
{
    public class Decode64
    {
        string ext { get; set; }
        public string Decoder()
        {
           // string ext = null;
            XmlDocument xml = new XmlDocument();
            xml.Load(@"C:\xml\gu2v_item.xml");
            foreach (XmlNode node1 in xml.SelectNodes("/getGU2VReply/Request/Documentdata/Doccontent"))
            {
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    string temp = node2.Value.ToString();
                    string inputStr = Encoding.UTF8.GetString(Convert.FromBase64String(node2.Value.ToString()));

                    XDocument xdoc = XDocument.Parse(inputStr);
                    xdoc.Declaration = null;
                    File.WriteAllText(@"c:\xml\gu2_64.xml", xdoc.ToString());
                    ext = xdoc.ToString();

                }
            }
            return ext;
        }


    }
}

