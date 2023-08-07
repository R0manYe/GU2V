using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Oracle.DataAccess.Client;
//using System.Data.OracleClient;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;


namespace GU2V
{
    class Program
    {

        static void Main(string[] args)
        {

           
            GoEtran ch = new GoEtran();
            var otv = ch.Proverka();
            Console.WriteLine(otv);
            CheckInInsertDbase ch1 = new CheckInInsertDbase();
            ch1.CheckInError();
            if (otv == 0)
            {
                GU2V_Vstavka vst = new GU2V_Vstavka();
                int prov = vst.VstavkaDoc_Item();
                Console.WriteLine("Получили ответ " + prov);

                Ins_Complex vs = new Ins_Complex();
                vs.InsComplex(prov);
                GU2G_Vstavka gu2g = new GU2G_Vstavka();
                gu2g.VstavkaGU2GDoc_Item();

            }
            else
            {

                string[] email1 = { "roman@abakan.vspt.ru" };
                DateTime parsedDate = DateTime.Now;

                foreach (string address in email1)
                {
                    string TextPisma = "неправильный ответ сервера на тест. C '" + parsedDate + "'";
                    string Zagolovok = "Не правильный ответ от АСУ при импорте ГУ2В ";
                    EmOpov opov1 = new EmOpov();
                    opov1.Opov_err(address, TextPisma, Zagolovok);
                }

            }
        }
     
    }
}
            
           

           






              

                        
                    

                
            
        













        

    



