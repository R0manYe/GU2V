using System;
using System.Collections.Generic;
//using System.Data.OracleClient;
using Oracle.DataAccess.Client;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace GU2V
{
    public class GoEtran
    {
        public string soapResult { get; set; }
        public int result { get; set; }
        public string otvet { get; set; }
        public string Str(string sborn)
        {

            using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
            {
                OracleCommand command = new OracleCommand(sborn, conn);
                conn.Open();
                OracleDataReader sB = command.ExecuteReader();
                sB.Read();
                string toD = sB.GetValue(0).ToString();
              //  Console.WriteLine(toD);
                conn.Close();              
                var _action = "http://192.168.1.125/Asu_proxy/Proxy.asmx/Zapros?Perem=" + toD;

                WebRequest request = WebRequest.Create(_action);
                WebResponse response = request.GetResponse();
                string soapResult;
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        soapResult = reader.ReadToEnd();
                    }
                    File.WriteAllText("vag_har.xml", soapResult);                  
                    return soapResult;
                }

            }
        }
        internal string Parsing(string sborn)
        {
          //  GoEtran etr = new GoEtran();
            string perem = Str(sborn);
            Console.WriteLine("Вход в Parsing метод ParsDocItem " + perem.Length);
            string text = perem;
            var xml = XElement.Parse(text);
            var elem = XElement.Parse(xml.Value.Trim());
            string code3 = Regex.Replace(elem.ToString(), @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F]", "");
           // Console.WriteLine(code3);
            File.WriteAllText("otvet0.xml", code3);
            XDocument xdoc = XDocument.Parse(code3);
            XNamespace a = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace b = "SysEtranInt";
            XNamespace c = "http://service.siw.pktbcki.rzd/";

            foreach (XElement VElement in xdoc.Element(a + "Envelope").Element(a + "Body").Elements(b + "GetBlockResponse"))
            {
                XAttribute nameAttribute = VElement.Attribute("value");
                XElement priceElement = VElement.Element("Text");
                string text1 = priceElement.ToString();
                var xml1 = XElement.Parse(text1);
                var elem1 = XElement.Parse(xml1.Value.Trim());
                otvet = Regex.Replace(elem1.ToString(), @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F,','']", "");
                File.WriteAllText("otvet2.xml", otvet.ToString());
            }
            return otvet;
        }
        public int Proverka()
        {
            var _action = "http://192.168.1.125/Asu_proxy/Proxy.asmx/Zapros?Perem=<test></test>";

            WebRequest request = WebRequest.Create(_action);
            WebResponse response = request.GetResponse();

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    soapResult = reader.ReadToEnd();
                }
                File.WriteAllText("vag_har.xml", soapResult);

            }
            var xml = XElement.Parse(soapResult);
            var elem = XElement.Parse(xml.Value.Trim());
            string code3 = Regex.Replace(elem.ToString(), @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F,','']", "");
            // Console.WriteLine(code3);
            File.WriteAllText("otvet0.xml", code3);
            XDocument xdoc = XDocument.Parse(code3);
            XNamespace a = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace b = "SysEtranInt";
            XNamespace c = "http://service.siw.pktbcki.rzd/";

            foreach (XElement VElement in xdoc.Element(a + "Envelope").Element(a + "Body").Elements(b + "GetBlockResponse"))
            {
                XAttribute nameAttribute = VElement.Attribute("value");
                XElement priceElement = VElement.Element("Text");
                string text1 = priceElement.ToString();
                var xml1 = XElement.Parse(text1);
                var elem1 = XElement.Parse(xml1.Value.Trim());
                otvet = Regex.Replace(elem1.ToString(), @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F,','']", "");
                File.WriteAllText("otvet2.xml", otvet.ToString());

            }
            string s2 = "<testReply>";
            result = string.Compare(s2, otvet.Substring(0, 11));


            return result;
        }

    }
}



