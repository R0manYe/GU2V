using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GU2V
{
    //Класс для проверки корректности загонки вагонов в Комплекс

    class Check
    {
        public int check_list { get; set; }
        public string stroka { get; set; }
        public string Poverka_pz()
        {
            Console.WriteLine("Метод вызван");
            using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
            {
                string nailic = "select t1.v1-t2.v2 as C from (select railway_station_name, id_doc_etran, num, max(wagons_total) as v1, date_ins, notification_time from " +
                    " etran_gu2v_item where id_doc_etran in (select naim  as doc from spr_collection where id_spr = 'GU2V_TEMP')  GROUP by id_doc_etran,date_ins,num,railway_station_name,notification_time) t1 " +
                    " left join  (select  etran_doc_id, count(wagon_id) as v2, date_load from COMPLEX.prsd_wagon_load where etran_doc_id is not null group by etran_doc_id, bu_id, date_load)t2 on " +
                    " t1.id_doc_etran = t2.etran_doc_id where t1.v1 != t2.v2";
                OracleCommand command = new OracleCommand(nailic, conn);
                conn.Open();
                OracleDataReader nal = command.ExecuteReader();
                while (nal.Read())
                {
                    string gnal = nal.GetValue(0).ToString();

                    if (nal == null)
                    {

                        stroka = "все норм!";
                    }
                    else
                    {
                        stroka = "Загонка с косяком!!!";
                    }
                }

            }

            return stroka;
        }
    }
}
