using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GU2V
{
    class Etran

    {
        static void GetEtran()
        { 
        XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
        string ds = soapEnvelopeXml.OuterXml;
        var _action = "http://192.168.1.125/Asu_proxy/Proxy.asmx/Zapros?Perem=" + ds;
        Console.WriteLine(_action);
            WebRequest request = WebRequest.Create(_action);
        WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        File.WriteAllText("vag_har.xml", line);
                    }
                   
                }
            }           
            response.Close();
            


        }
        public static XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(@"<GetInform><ns0:getReference25ASU xmlns:ns0='http://service.siw.pktbcki.rzd/'><ns0:Reference25ASURequest><idUser>3</idUser><idReference>25</idReference><vagons>" +
                "<vagon></vagon></vagons></ns0:Reference25ASURequest></ns0:getReference25ASU></GetInform>");
            return soapEnvelopeDocument;
        }


   

