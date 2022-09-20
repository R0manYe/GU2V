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
            if (otv == 0)
            {
                GU2V_Vstavka vst = new GU2V_Vstavka();
                int prov = vst.VstavkaDoc_Item();
                Console.WriteLine("Получили ответ " + prov);

                Ins_Complex vs = new Ins_Complex();
                vs.InsComplex(prov);
                GU2G_Vstavka gu2g = new GU2G_Vstavka();
                gu2g.VstavkaGU2GDoc_Item();
                // Check a1 = new Check();
                //string d=a1.Poverka_pz();
                //Console.WriteLine(d);
            }
            else
            {
               
                string[] email1 = { "roman@abakan.vspt.ru" };

                foreach (string address in email1)
                {
                    string TextPisma1 = "неправильный ответ сервера на тест.";
                    EmOpov opov1 = new EmOpov();
                    opov1.Opov_err(address, TextPisma1);
                }

            }
        }
     
    }
}
            
           

           






              

                        
                    

                
            
        













        

    



