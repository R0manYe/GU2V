using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GU2V
{
   
    class GU2G_Vstavka
    {
        public int i_cikl { get; set; }

       
           
               public int VstavkaGu2G()
                {
            using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
            {
                string del_doc = "delete from etran_gu2g_doc where docstateid=1429 and docdate >=(sysdate-1)";
                string del_item = "delete from etran_gu2g_item where stateid=1429 and gu2gdate >=(sysdate-1)";
                OracleCommand command = new OracleCommand(del_doc, conn);
                OracleCommand command1 = new OracleCommand(del_item, conn);
                conn.Open();
                OracleDataReader del_doc1 = command.ExecuteReader();               
                OracleDataReader del_item1 = command1.ExecuteReader();
                conn.Close();
            }


                string sborn = "SELECT  XMLAGG(XMLELEMENT(\"GU2GStatus\", XMLFOREST(to_char(sysdate-1,'DD.mm.yyyy HH24:MI:SS')\"fromDate\")" +
                           " \"secondGoup\",XMLFOREST((to_char(sysdate,'DD.mm.yyyy HH24:MI:SS'))\"toDate\")\"secondGoup\")) AS \"XML_QUERY\" FROM DUAL";
                    GoEtran otv = new GoEtran();
                    string pr = otv.Parsing(sborn);
                    Console.WriteLine("Проверка в VstavkaGU2G " + pr.Length);
                    File.WriteAllText("GU2G.xml", pr);
                    //  if (Pars().Length > 100)

                    //  {
                    using (SqlConnection connection1 = new SqlConnection("Data Source=192.168.1.13;Initial Catalog=dislokacia;User ID=Roman;Password=238533"))
                    {
                        string proverka = "DECLARE @x xml SET @x = '" + pr + "' select count(T.c.value('(DocId)[1]', 'int')) AS DocId  FROM @x.nodes('/GU2GStatusReply/GU2G') T(c) where(T.c.value('(DocId)[1]', 'int'))  " +
                        " not in  (SELECT[DOCID] FROM[FLAGMAN]..[VSPTSVOD].[ETRAN_GU2G_DOC])";
                        SqlCommand com = new SqlCommand(proverka, connection1);
                        connection1.Open();
                        SqlDataReader prov = com.ExecuteReader();
                        prov.Read();
                        int i_prov = Convert.ToInt16(prov.GetValue(0).ToString());
                        Console.WriteLine(i_prov);

                        if (i_prov > 0)
                        {
                            Console.WriteLine("Происходит загонка");
                            string ins_gu2v_doc = "DECLARE @x xml SET @x = '" + pr + "' INSERT INTO [FLAGMAN]..[VSPTSVOD].[ETRAN_GU2G_DOC] ([DOCID],[DOCNUM],[DOCSTATEID],[DOCSTATE],[DOCDATE],[ISREPEAT],[GU2GNEEDFORECP]" +
                            ",[DOCLASTOPER],[DATA_INS]) select T.c.value('(DocId)[1]', 'int') AS DocId, T.c.value('(DocNum)[1]', 'int') AS DocNum,T.c.value('(DocStateId)[1]', 'int') AS DocStateId," +
                            "T.c.value('(DocState)[1]', 'varchar(50)') AS DocState,T.c.value('(DocDate)[1]', 'Date') AS DocDate,T.c.value('(isRepeat)[1]', 'int') AS isRepeat,T.c.value('(Gu2GNeedForECP)[1]', 'int') " +
                            "AS Gu2GNeedForECP,T.c.value('(DocLastOper)[1]', 'DateTime') AS DocLastOper,GetDate()  FROM @x.nodes('//GU2GStatusReply/GU2G') T(c) where(T.c.value('(DocId)[1]', 'int')) " +
                            "not in  (SELECT[DOCID] FROM[FLAGMAN]..[VSPTSVOD].[ETRAN_GU2G_DOC])";
                            //  SqlCommand com1 = new SqlCommand(proverka1, connection1);
                            connection1.Close();
                            connection1.Open();
                            SqlCommand command = new SqlCommand(ins_gu2v_doc, connection1);
                            connection1.Close();
                            connection1.Open();
                            SqlDataReader ud1 = command.ExecuteReader();
                            connection1.Close();
                            string ins_log = "insert into spr_etran_log (data,SPR,doc,ID_DOC_ETRAN,DATA_ETRAN_INS) select sysdate,'GU2G_DOC' as SPR,DOCNUM,docid,DOCLASTOPER from ETRAN_GU2G_DOC where DOCID not " +
                                " in (select id_doc_etran from SPR_ETRAN_LOG where spr = 'GU2G_DOC')";
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
        public int VstavkaGU2GDoc_Item()
        {
            int per = VstavkaGu2G();
            Console.WriteLine("Вход в VstavkaGU2G " + per);
            if (per > 0)
            {
                string ochistka = "delete from spr_collection where id_spr='GU2G_TEMP'";
                string zagonka = "insert into  spr_collection(ID_SPR,NAIM) select 'GU2G_TEMP' as naim, docid from(select docid from ETRAN_GU2G_DOC  where not exists (select GU2G_ID from " +
                    " ETRAN_GU2G_ITEM  where ETRAN_GU2G_DOC.DOCID=ETRAN_GU2G_ITEM.GU2G_ID))";
                string opr_vst = "select count(naim) from spr_collection where id_spr='GU2G_TEMP'";
                string ins_log_doc_item = "insert into spr_etran_log (spr,ID_DOC_ETRAN,DATA) (select 'GU2G_ITEM' as spr, naim, sysdate from spr_collection where id_spr ='GU2G_TEMP'" +
                    " and naim not in (select id_doc_etran from spr_etran_log where spr='GU2G_ITEM'))";
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
                            string perebor = "select doc from (select rownum as ro,naim as doc from spr_collection where id_spr='GU2G_TEMP') where ro='" + i + "'";
                            conn.Close();
                            OracleCommand command3 = new OracleCommand(perebor, conn);
                            conn.Open();
                            OracleDataReader pereb = command3.ExecuteReader();
                            while (pereb.Read())
                            {
                                string pr = pereb.GetValue(0).ToString();
                                Console.WriteLine("Номер " + i + " Код документа " + pr + " " + i_cikl);
                                //  Console.ReadKey();
                                string sborn1 = "SELECT  XMLAGG(XMLELEMENT(\"getGU2G\",XMLFOREST(('" + pr + "')\"DocID\")\"secondGoup\")) AS \"XML_QUERY\" FROM DUAL";
                                GoEtran pars1 = new GoEtran();
                                string pr1 = pars1.Parsing(sborn1);
                                File.WriteAllText("GU2G_ITEM.xml", pr1);
                                //  Console.WriteLine(pr1);

                                using (SqlConnection connection1 = new SqlConnection("Data Source=192.168.1.13;Initial Catalog=dislokacia;User ID=Roman;Password=238533"))
                                {
                                    string proverka = "DECLARE @x xml SET @x = '" + pr1 + "'INSERT INTO [FLAGMAN]..[VSPTSVOD].[ETRAN_GU2G_ITEM] ([CARORDER],[CARID],[CARNUMBER],[CAROPERATION],[PLANDATEPODUBOR]" +
                                        ",[CARFREIGHTNAME],[CARRECIPNAME],[CARISREADY],[GU2G_ID],[GU2G_NUM],[STATEID],[STATENAME],[GU2GDATE],[GU2GNEEDFORECP],[GU2GDATECREATE],[GU2GCLIENTID],[GU2GCLIENTOKPO]" +
                                        ",[GU2GCLIENTNAME],[STATIONCODE],[STATIONNAME],[RWID],[RWNAME],[GU2GPLACE],[GU2GLOADWAY],[ISREPEAT],[WAGSAMOUNT],[DAT_INS]) select " +
                                        " T.c.value('(CarOrder)[1]', 'int') AS CarOrder,T.c.value('(CarID)[1]', 'int') AS CarID,T.c.value('(CarNumber)[1]', 'varchar(10)') AS CarNumber," +
                                        " T.c.value('(carOperation)[1]', 'varchar(10)') AS carOperation,T.c.value('(PlanDatePodUbor)[1]', 'Date') AS PlanDatePodUbor," +
                                        " T.c.value('(carFreightName)[1]', 'varchar(200)') AS carFreightName,T.c.value('(carRecipName)[1]', 'varchar(50)') AS carRecipName," +
                                        " T.c.value('(CarIsReady)[1]', 'int') AS CarIsReady,P.c.value('(Gu2G_ID)[1]', 'int') AS Gu2G_ID,P.c.value('(Gu2G_Num)[1]', 'int') AS Gu2G_Num," +
                                        " P.c.value('(StateID)[1]', 'int') AS StateID,P.c.value('(StateName)[1]', 'varchar(200)') AS StateName,P.c.value('(Gu2GDate)[1]', 'Date') AS Gu2GDate," +
                                        " P.c.value('(Gu2GNeedForECP)[1]', 'int') AS Gu2GNeedForECP,P.c.value('(Gu2GDateCreate)[1]', 'Date') AS Gu2GDateCreate,P.c.value('(Gu2GClientID)[1]', 'int') AS Gu2GClientID," +
                                        " P.c.value('(Gu2GClientOKPO)[1]', 'varchar(200)') AS Gu2GClientOKPO,P.c.value('(Gu2GClientName)[1]', 'varchar(200)') AS Gu2GClientName," +
                                        " P.c.value('(StationCode)[1]', 'int') AS StationCode,P.c.value('(StationName)[1]', 'varchar(50)') AS StationName,P.c.value('(RWID)[1]', 'int') AS RWID," +
                                        " P.c.value('(RWName)[1]', 'varchar(200)') AS RWName,P.c.value('(Gu2GPlace)[1]', 'varchar(200)') AS Gu2GPlace,P.c.value('(Gu2GLoadWay)[1]', 'varchar(200)') AS Gu2GLoadWay," +
                                        " P.c.value('(isRepeat)[1]', 'int') AS isRepeat,P.c.value('(WagsAmount)[1]', 'int') AS WagsAmount,getdate() as dat_ins  FROM @x.nodes('/getGU2GReply/wags/wag') T(c)" +
                                        "  cross apply  @x.nodes('/getGU2GReply') P(c)";
                                    SqlCommand com = new SqlCommand(proverka, connection1);
                                    connection1.Open();
                                    SqlDataReader prov = com.ExecuteReader();
                                    connection1.Close();
                                    Console.WriteLine("Загонка прошла " + pr + "  " + i + " из " + i_cikl);

                                  //    return i_cikl; 
                                }
                            }
                            conn.Close();


                        }

                    }
                    conn.Close();
                    conn.Open();
                    OracleDataReader logs_item = command4.ExecuteReader();
                    conn.Close();

                    {
                        //   Console.WriteLine("Конец!");

                    }
                }
                return i_cikl;
            }

            return i_cikl;

        }
            
        
    }
}
