using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GU2V
{
    class GU2V_Vstavka
    {

        public int i_cikl { get; set; }
        public string St { get; set; }
        private int VstavkaDoc()
        {

            string sborn = "SELECT  XMLAGG(XMLELEMENT(\"GU2VStatus\", XMLFOREST(to_char(sysdate-1,'DD.mm.yyyy HH24:MI:SS')\"fromDate\")" +
                       " \"secondGoup\",XMLFOREST((to_char(sysdate,'DD.mm.yyyy HH24:MI:SS'))\"toDate\")\"secondGoup\")) AS \"XML_QUERY\" FROM DUAL";
            GoEtran otv = new GoEtran();
            string pr = otv.Parsing(sborn);
            File.WriteAllText("ott.xml", pr);
            Console.WriteLine("Проверка в Vstavka " + pr.Length);
            //  if (Pars().Length > 100)

            //  {
            using (SqlConnection connection1 = new SqlConnection("Data Source=192.168.1.13;Initial Catalog=dislokacia;User ID=Roman;Password=238533"))
            {
                string proverka = "DECLARE @x xml SET @x = '" + pr + "' select count(T.c.value('(DocId)[1]', 'int')) AS DocId  FROM @x.nodes('/GU2VStatusReply/GU2V') T(c) where(T.c.value('(DocId)[1]', 'int'))  " +
                " not in  (SELECT[DOCID] FROM[FLAGMAN]..[VSPTSVOD].[ETRAN_GU2V_DOC])";
                SqlCommand com = new SqlCommand(proverka, connection1);
                connection1.Open();
                SqlDataReader prov = com.ExecuteReader();
                prov.Read();
                int i_prov = Convert.ToInt16(prov.GetValue(0).ToString());
                Console.WriteLine("Перед проверкой i_pov=", i_prov);
                if (i_prov > 0)
                {
                    Console.WriteLine("Происходит загонка");
                    string ins_gu2v_doc = "DECLARE @x xml SET @x = '" + pr + "' INSERT INTO[FLAGMAN]..[VSPTSVOD].[ETRAN_GU2V_DOC] ([DOCID],[DOCNUM],[DOCDATE],[DOCSTATEID],[DOCSTATE],[DOCLASTOPER],[DATE_INS])" +
                                        "select T.c.value('(DocId)[1]', 'int') AS DocId,T.c.value('(DocNum)[1]', 'int') AS DocNum,T.c.value('(DocDate)[1]', 'datetime') AS DocDate," +
                                    " T.c.value('(DocStateId)[1]', 'varchar(50)') AS DocStateId,T.c.value('(DocState)[1]', 'varchar(50)') AS DocState,T.c.value('(DocLastOper)[1]', 'datetime') AS DocLastOper,getdate() as dat " +
                                    " FROM @x.nodes('/GU2VStatusReply/GU2V') T(c) where(T.c.value('(DocId)[1]', 'int')) not in  (SELECT[DOCID] FROM[FLAGMAN]..[VSPTSVOD].[ETRAN_GU2V_DOC])";
                    //  SqlCommand com1 = new SqlCommand(proverka1, connection1);
                    connection1.Close();
                    connection1.Open();
                    SqlCommand command = new SqlCommand(ins_gu2v_doc, connection1);
                    connection1.Close();
                    connection1.Open();
                    SqlDataReader ud1 = command.ExecuteReader();
                    connection1.Close();
                    string ins_log = "insert into spr_etran_log (data,SPR,doc,ID_DOC_ETRAN,DATA_ETRAN_INS) select sysdate,'GU2V_DOC' as SPR,DOCNUM,docid,DOCLASTOPER from ETRAN_GU2V_DOC where DOCID not " +
                        " in (select id_doc_etran from SPR_ETRAN_LOG where spr = 'GU2V_DOC')";
                    using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
                    {
                        OracleCommand command1 = new OracleCommand(ins_log, conn);
                        conn.Open();
                        OracleDataReader logs = command1.ExecuteReader();
                        conn.Close();

                    }
                    Console.WriteLine("i_pov=" + i_prov);
                    return i_prov;
                }
                else
                {
                    Console.WriteLine("Исключение");
                    return i_prov;
                }
            }

        }
        public int VstavkaDoc_Item()
        {
            int per = VstavkaDoc();
            Console.WriteLine("Вход в Vstavka " + per);
            if (per > 0)
            {
                string ochistka = "delete from spr_collection where id_spr='GU2V_TEMP'";
                string zagonka = "insert into  spr_collection(ID_SPR,NAIM) select 'GU2V_TEMP' as naim, docid from(select docid from ETRAN_GU2V_DOC  where not exists (select id_doc_etran from " +
                    " ETRAN_GU2V_ITEM  where ETRAN_GU2V_DOC.DOCID=ETRAN_GU2V_ITEM.ID_DOC_ETRAN))";
                string opr_vst = "select count(naim) from spr_collection where id_spr='GU2V_TEMP'";
                string ins_log_doc_item = "insert into spr_etran_log (spr,ID_DOC_ETRAN,DATA) (select 'GU2V_ITEM' as spr, naim, sysdate from spr_collection where id_spr ='GU2V_TEMP'" +
                    " and naim not in (select id_doc_etran from spr_etran_log where spr='GU2V_ITEM'))";
                using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
                {
                    OracleCommand command1 = new OracleCommand(ochistka, conn);
                    OracleCommand command2 = new OracleCommand(zagonka, conn);
                    OracleCommand command = new OracleCommand(opr_vst, conn);
                    OracleCommand command4 = new OracleCommand(ins_log_doc_item, conn);
                    conn.Open();
                    OracleDataReader otch = command1.ExecuteReader();
                    conn.Close();
                    conn.Open();
                    OracleDataReader zag = command2.ExecuteReader();
                    conn.Close();
                    conn.Open();
                    OracleDataReader cikl = command.ExecuteReader();
                    cikl.Read();
                    i_cikl = Convert.ToInt32(cikl.GetValue(0).ToString());
                    Console.WriteLine(i_cikl);
                    if (i_cikl > 0)
                    {
                        Console.WriteLine("Данные есть!" + i_cikl);
                        //    Console.ReadKey();


                        for (int i = 1; (i_cikl) >= i; ++i)
                        {
                            string perebor = "select doc from (select rownum as ro,naim as doc from spr_collection where id_spr='GU2V_TEMP') where ro='" + i + "'";
                            conn.Close();
                            OracleCommand command3 = new OracleCommand(perebor, conn);
                            conn.Open();
                            OracleDataReader pereb = command3.ExecuteReader();
                            while (pereb.Read())
                            {
                                int pr = Convert.ToInt32(pereb.GetValue(0).ToString());
                                
                                Console.WriteLine("Номер " + i + " Код документа " + pr + " " + i_cikl);
                                //  Console.ReadKey();
                                string sborn1 = "SELECT  XMLAGG(XMLELEMENT(\"getGU2V\",XMLFOREST(('" + pr + "')\"Doc_ID\")\"secondGoup\")) AS \"XML_QUERY\" FROM DUAL";
                                GoEtran pars1 = new GoEtran();
                                string pr1 = pars1.Parsing(sborn1);
                                File.WriteAllText(@"C:\xml\pr1.xml", pr1);                              
                                var elem1 = XElement.Parse(pr1.Trim());
                                var otvet = Regex.Replace(elem1.ToString(), @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F]", "");
                                var Podgotovka = Regex.Replace(otvet, "'", "");
                                File.WriteAllText(@"C:\xml\elem1.xml", otvet);
                                File.WriteAllText(@"C:\xml\gu2v_item.xml", otvet);

                                XmlDocument xml = new XmlDocument();
                                xml.Load(@"C:\xml\gu2v_item.xml");
                                foreach (XmlNode node1 in xml.SelectNodes("/getGU2VReply/DocState/StateName"))
                                {
                                    foreach (XmlNode node2 in node1.ChildNodes)
                                    {
                                        string St = node2.Value.ToString();
                                    }
                                }

                                 Console.WriteLine("Вызов Декоде");
                                 Decode64 f = new Decode64();
                                 string pr2 = f.Decoder();
                                const string inputXMLFile = @"c:\xml\gu2_64.xml";
                                 pr2 = inputXMLFile;
                                Console.WriteLine(pr2);

                                using (SqlConnection connection1 = new SqlConnection("Data Source=192.168.1.13;Initial Catalog=dislokacia;User ID=Roman;Password=238533"))
                                {
                                    string proverka = "DECLARE @x xml SET @x = '" + Podgotovka + "'INSERT INTO [FLAGMAN]..[VSPTSVOD].[ETRAN_GU2V_ITEM]([RAILWAY_STATION_NAME],[RAILWAY_STATION_CODE]," +
                                        " [RAILWAY_NAME],[NUM],[NOTIFICATION_DATE],[ORGID],[CONTRAGENT],[GET_PLACE],[WAY_NAME],[ORDER_NUMBER],[WAGON_NUMBER],[PLANNED_FILING_DATE],[OPERATION]," +
                                        " [CARGO_NAME],[CARGO_CODE],[RECIPIENT],[WAGOWNERID],[wagOwnerOKPO],[wagOwnerName],[wagTrustedID],[wagTrustedOKPO],[wagTrustedName],[WAGONS_TOTAL],[POSITION_FIO],[DOCSTATE],[ID_DOC_ETRAN],[DATE_INS]) " +
                                        "select T.c.value('(railway_station_name)[1]', 'varchar(50)') AS railway_station_name,T.c.value('(railway_station_code)[1]', 'int') AS railway_station_code," +
                                        " T.c.value('(railway_name)[1]', 'varchar(50)') AS railway_name,T.c.value('(number)[1]', 'varchar(50)') AS number, T.c.value('(notification_date)[1]', 'datetime') " +
                                        " AS notification_date,T.c.value('(orgid)[1]', 'int') AS orgid, T.c.value('(contragent)[1]', 'varchar(200)') AS contragent,T.c.value('(get_place)[1]', 'varchar(50)') " +
                                        " AS get_place, T.c.value('(get_place)[1]', 'varchar(50)') AS way_name, P.c.value('(order_number)[1]', 'int') AS order_number, P.c.value('(wagon_number)[1]', 'int') " +
                                        " AS wagon_number, P.c.value('(planned_filing_date)[1]', 'datetime') AS planned_filing_date,P.c.value('(operation)[1]', 'varchar(50)') AS operation, " +
                                        " P.c.value('(cargo_name)[1]', 'varchar(50)') AS cargo_name, P.c.value('(cargo_code)[1]', 'int') AS cargo_code, P.c.value('(recipient)[1]', 'varchar(300)') AS RECIPIENT," +
                                        " P.c.value('(wagOwnerID)[1]', 'int') AS wagOwnerID,P.c.value('(wagOwnerOKPO)[1]', 'varchar(20)') AS wagOwnerOKPO,P.c.value('(wagOwnerName)[1]', 'varchar(300)') AS wagOwnerName," +
                                        " P.c.value('(wagTrustedID)[1]', 'int') AS wagTrustedID, P.c.value('(wagTrustedOKPO)[1]', 'varchar(20)') AS wagTrustedOKPO, P.c.value('(wagTrustedName)[1]', 'varchar(120)') AS wagTrustedName," +
                                        " T.c.value('(wagons_total)[1]', 'varchar(50)') AS wagons_total, T.c.value('(position_FIO)[1]', 'varchar(300)') AS position_FIO,T.c.value('(DocStateID)[1]', 'varchar(200)') AS DocState," +
                                        " "+pr+" as dd,getdate() as dat  FROM @x.nodes('/getGU2VReply') T(c)  cross apply  @x.nodes('/getGU2VReply/wagons/wagon') P(c)";
                                    SqlCommand com = new SqlCommand(proverka, connection1);
                                    connection1.Open();
                                    SqlDataReader prov = com.ExecuteReader();
                                    connection1.Close();
                                    Console.WriteLine("Загонка прошла " + pr + "  " + i + " из " + i_cikl);

                                }
                            }
                                conn.Close();
                               

                            }
                            return per;
                        }
                        conn.Close();
                        conn.Open();
                        OracleDataReader logs_item = command4.ExecuteReader();
                        conn.Close();

                        {
                            //   Console.WriteLine("Конец!");

                        }
                    
                        
                    }
                }
                return per;

            }
        }
    }

