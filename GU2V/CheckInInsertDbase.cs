using System;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;

namespace GU2V
{
    class CheckInInsertDbase
    {
        public void CheckInError()
        {
            string strConn = "Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans";
            string sql = "select id from(select t1.id_doc_etran as id,t1.num,t1.railway_station_name as st_naim,t1.date_ins as PL_DAT,t1.notification_time as PL_TIME,t1.v1 as COL_W," +
                "t2.v2 as COL_W_F from(select railway_station_name, id_doc_etran, num, max(wagons_total) as v1, date_ins, notification_time from etran_gu2v_item GROUP by id_doc_etran, " +
                "date_ins, num, railway_station_name, notification_time) t1 left join(select etran_doc_id, count(wagon_id) as v2, date_load from COMPLEX.prsd_wagon_load where" +
                " etran_doc_id is not null group by etran_doc_id, bu_id, date_load)t2 on t1.id_doc_etran = t2.etran_doc_id order by t1.date_ins desc) where col_w_f is null and " +
                " ST_NAIM not in ('КИЯ-ШАЛТЫРЬ','СУХОВСКАЯ-ЮЖНАЯ','КИТОЙ-КОМБИНАТСКАЯ','СУХОВСКАЯ') and pl_dat>=(sysdate-1)";
            
            string del1 = "delete from COMPLEX.prsd_wagon_load where etran_doc_id in (" + sql + ")";
            string del2 = "delete from etran_gu2v_doc where docid in (" + sql + ")";
            string del3 = "delete from etran_gu2v_item where id_doc_etran in (" + sql + ")";
            using (OracleConnection conn = new OracleConnection(strConn))
            {
                conn.Open();
                OracleDataAdapter adapter = new OracleDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                ds.Locale = CultureInfo.InvariantCulture;
                adapter.Fill(ds);

                DataTable products = ds.Tables["Table"];
                var query = from product in products.AsEnumerable() select product;
                int count = query.Count();

                if (count > 0)
                {
                    conn.Close();
                    conn.Open();
                    OracleCommand com1 = new OracleCommand(del1, conn);
                    OracleCommand com2 = new OracleCommand(del2, conn);
                    OracleCommand com3 = new OracleCommand(del3, conn);
                   
                    OracleDataReader dell1 = com1.ExecuteReader();
                    OracleDataReader dell2 = com2.ExecuteReader();
                    OracleDataReader dell3 = com3.ExecuteReader();
                    conn.Close();
                }
            }
        }
    }
}
    

