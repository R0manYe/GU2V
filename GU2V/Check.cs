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
    //Класс для проверки корректности после загонки в Комплекс

    class Check
    {

        public void Poverka_pz()
        {
            string sql = "select WAGON_ID,company_id,bu_id,(select railway_station_name from etran_gu2v_item t1 WHERE t1.WAGON_NUMBER = WAGON_ID and t1.id_doc_etran = etran_doc_id) as stancia," +
                 "(select recipient from etran_gu2v_item t1 WHERE t1.WAGON_NUMBER = WAGON_ID and t1.id_doc_etran = etran_doc_id) as cli " +
                 "from COMPLEX.prsd_wagon_load where etran_doc_id in (select naim as doc from spr_collection where id_spr = 'GU2V_TEMP')  and company_id is null";

            using (OracleConnection connection = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
            {
                connection.Open();
                OracleDataAdapter adapter = new OracleDataAdapter(sql, connection);
                DataSet ds = new DataSet();
                ds.Locale = CultureInfo.InvariantCulture;
                adapter.Fill(ds);

                DataTable products = ds.Tables["Table"];
                var query = from product in products.AsEnumerable() select product;
                int count = query.Count();

                if (count > 0)
                {
                    Console.WriteLine("Больше нуля");

                    List<DataRow> Vagon = products.AsEnumerable().ToList();
                    string vagoni = "";
                    foreach (DataRow row in Vagon)
                    {
                        vagoni = vagoni + row[0].ToString() + ' ' + row[2].ToString() + ' ' + row[3].ToString() + ' ' + row[4].ToString() + "\n";

                    }
                    Console.WriteLine(count);
                    Console.WriteLine(vagoni);
                    //Оповещаем
                    string[] email1 = { "roman@abakan.vspt.ru" };
                    DateTime parsedDate = DateTime.Now;

                    foreach (string address in email1)
                    {
                        string TextPisma = "Вагон   БЕ  Станция Клиент  \n" + vagoni + "";
                        string Zagolovok = "Не корректный импорт данных клиентов";
                        EmOpov opov1 = new EmOpov();
                        opov1.Opov_err(address, TextPisma, Zagolovok);
                    }

                }
                else
                {
                    // Console.WriteLine("Меньше нуля");

                }


            }
        }
    }
}

