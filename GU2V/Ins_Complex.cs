using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GU2V
{
    class Ins_Complex
    {
        public string pereb_c { get; set; }
        public string st_nam { get; set; }
        public string address_c { get; set; }
        public string num_c { get; set; }
        public string kol_c { get; set; }
        public string vr_c { get; set; }
        public int ii { get; set; }

        public void InsComplex(int prov)
        {

            GU2V_Vstavka vs = new GU2V_Vstavka();          
            Console.WriteLine("Из нового метода");
            Console.WriteLine("Получили в новом методе prov=" + prov);

            for (int i = 1; i <= prov; i++)
            {
                using (OracleConnection conn = new OracleConnection("Data Source = flagman; Persist Security Info=True;User ID = vsptsvod; Password=sibpromtrans"))
                {
                    string perebor = "select  doc from (select rownum as ro,naim as doc from spr_collection where id_spr='GU2V_TEMP') where ro='" + i + "'";

                    OracleCommand command3 = new OracleCommand(perebor, conn);
                    conn.Open();
                    OracleDataReader pereb = command3.ExecuteReader();
                    while (pereb.Read())
                    {
                        pereb_c = pereb.GetValue(0).ToString();
                    }
                    Console.WriteLine("Номер " + i + " Код документа " + pereb_c);
                    conn.Close();
                    Console.WriteLine(" Код документа " + pereb_c);
                    string udal = "delete from COMPLEX.prsd_wagon_load where etran_doc_id is not null and date_fix_load is null and date_load<sysdate-2";
                    string vstav = "insert into complex.prsd_wagon_load(SERIAL_NUMBER,WAGON_ID,netto,CARGO_ID,COMPANY_ID,OWNER_ID,ARENDATOR_ID,TYPE_ID,doc_id,DISTRICT_ID,date_load,bu_id,WAGON_LOAD_ID,DOC_TYPE_ID,date1,n_poezd,tara,capacity," +
                         " ETRAN_DOC_ID,ETRAN_DOC_DATE,ETRAN_DOC_NUM,ETRAN_PLAN_DATE_PODACH,ETRAN_RZD_PRIM_FIO) " +                        
                             "select (select order_number from dislokacia  d where d.nom_vag=wagon_number) as SERIAL_NUMBER,wagon_number as wagon_id," +
                             "(select ves_grz / 1000 from dislokacia d where d.nom_vag = wagon_number) as NETTO, case when (select ves_grz/1000 from dislokacia  d where d.nom_vag=wagon_number) = 0 then '0' else  rpad(cargo_code, length(cargo_code) - 1) end as CARGO_ID," +
                             "(select inn from spr_etran_org where spr_etran_org.id=(select id_gruzpol from dislokacia where nom_vag=etran_gu2v_item.wagon_number)) as COMPANY_ID," +
                             " (select inn from spr_etran_org where etran_gu2v_item.wagownerid=spr_etran_org.id) as OWNER_ID," +
                             "(select inn from spr_etran_org where etran_gu2v_item.wagtrustedid = spr_etran_org.id) as ARENDATOR_ID," +
                             "(select type_id from complex.spr_wagon_type where complex.spr_wagon_type.rzd_id_sinhro = (select d.rod_vag_uch from dislokacia d where d.nom_vag = wagon_number))as TYPE_ID," +
                             "'" + pereb_c + "' as doc_id,case when railway_station_code in ('88250','882506') and GET_PLACE like '%ЗАВОД%' then 20210030111 when railway_station_code in ('88250','882506') and GET_PLACE like '%ПРОМЫШ%' " +
                             "then 20210030112 else (select DISTRICT_ID from complex.spr_station where DISTRICT_ID is not null and STATION_ECP_rzd =(case when LENGTH(railway_station_code)=6 then substr(railway_station_code,0,5) else railway_station_code end)) end as district_id," +
                             "sysdate - (4 / 24) as date_load,(select bu_id from complex.spr_district where DISTRICT_ID = (select DISTRICT_ID from complex.spr_station where DISTRICT_ID is not null and station_ecp_id is null and rownum = 1 and STATION_ECP_rzd = railway_station_code)) as bu_id," +
                             "complex.count_row.nextval as WAGON_LOAD_ID,'1866658' as DOC_TYPE_ID,(select date_op from dislokacia d where d.nom_vag = wagon_number) as date1," +
                             "(select NOM_POEZD from dislokacia d where d.nom_vag = wagon_number) as n_poezd,(select(carweightdep / 10) from SPR_ETRAN_VAGON where vagon = wagon_number) as tara," +
                             "(select replace(cartonnage, '.', ',') from SPR_ETRAN_VAGON where vagon = wagon_number) as capacity,'" + pereb_c + "' as ETRAN_doc_id," +
                             "to_date(SUBSTRb(notification_date, 5, 2) || '.' || (select id_f from mesac where name3 = SUBSTRB(notification_date, 1, 3)) || '.' || SUBSTRb(notification_date, 8, 4) || notification_time,'DD.MM.YYYY HH24:MI')  as ETRAN_DOC_DATE," +
                             " NUM as ETRAN_DOC_NUM, to_date(SUBSTRb(planned_filing_date, 5, 2) || '.' || (select id_f from mesac where name3 = SUBSTRB(planned_filing_date, 1, 3)) || '.' || SUBSTRb(planned_filing_date, 8, 4) || planned_filing_time,'DD.MM.YYYY HH24:MI')  as ETRAN_PLAN_DATE_PODACH," +
                             " position_fio as ETRAN_RZD_PRIM_FIO from etran_gu2v_item where id_doc_etran = '"+ pereb_c+"' and not EXISTS(select etran_doc_id from COMPLEX.prsd_wagon_load pwl where etran_doc_id is not null " +
                             " and id_doc_etran = pwl.etran_doc_id and pwl.date_fix_load is null)";

                    //до 29.09.22
                    /*  string vstav = "insert into complex.prsd_wagon_load(SERIAL_NUMBER,WAGON_ID,netto,CARGO_ID,COMPANY_ID,TYPE_ID,doc_id,DISTRICT_ID,date_load,bu_id,WAGON_LOAD_ID,DOC_TYPE_ID,date1,n_poezd,tara,capacity," +
                        " owner_id,ETRAN_DOC_ID,ETRAN_DOC_DATE,ETRAN_DOC_NUM,ETRAN_PLAN_DATE_PODACH,ETRAN_RZD_PRIM_FIO) select SERIAL_NUMBER,WAGON_ID,netto,CARGO_ID,COMPANY_ID,TYPE_ID,doc_id,DISTRICT_ID,date_load,bu_id," +
                        " complex.count_row.nextval as WAGON_LOAD_ID,DOC_TYPE_ID,date1,n_poezd,tara,capacity,owner_id,ETRAN_doc_id,ETRAN_DOC_DATE," +
                            "ETRAN_DOC_NUM,ETRAN_PLAN_DATE_PODACH,ETRAN_RZD_PRIM_FIO from(select (select order_NUMBER from ETRAN_GU2V_ITEM where dislokacia.nom_vag = ETRAN_GU2V_ITEM.WAGON_NUMBER and ID_DOC_ETRAN= '" + pereb_c + "') as SERIAL_NUMBER,nom_vag as wagon_id,ves_grz / 1000 as netto,case when ves_grz = 0 then '0' else  rpad(KOD_GRZ_UCH, length(KOD_GRZ_UCH) - 1) end as CARGO_ID," +
                            " (select inn from spr_cli where dislokacia.GRUZPOL_OKPO = okpo and dislokacia.stan_nazn = spr_cli.ID_ST ) as COMPANY_ID," +
                            "(select type_id from complex.spr_wagon_type where ROD_VAG_UCH = complex.spr_wagon_type.rzd_id_sinhro) as TYPE_ID," +
                            "'" + pereb_c + "' as doc_id," +
                            "(select DISTRICT_ID from complex.spr_station where DISTRICT_ID is not null and STATION_ECP_ID = stan_nazn) as district_id,sysdate-(4/24) as date_load," +
                            "(select bu_id from complex.spr_district where DISTRICT_ID = (select DISTRICT_ID from complex.spr_station where DISTRICT_ID is not null and STATION_ECP_ID = stan_nazn)) as bu_id," +
                            " '1866658' as DOC_TYPE_ID,DATE_OP as date1,NOM_POEZD as n_poezd,(select(carweightdep / 10) from SPR_ETRAN_VAGON where vagon = nom_vag) as tara," +
                            "(select replace(cartonnage, '.', ',') from SPR_ETRAN_VAGON where vagon = nom_vag) as capacity,(select carownerokpo from SPR_ETRAN_VAGON where vagon = nom_vag) as owner_id,'" + pereb_c + "' as ETRAN_doc_id," +
                            " (select  to_date(SUBSTRb(dat, 5, 2) || '.' || (select id_f from mesac where name3 = SUBSTRB(dat, 1, 3)) || '.' || SUBSTRb(dat, 8, 4) || time,'DD.MM.YYYY HH24:MI') " +
                            " as dat from(select DISTINCT notification_date as dat, notification_time as time from etran_gu2v_item where id_doc_etran = '" + pereb_c + "')) as ETRAN_DOC_DATE, " +
                            " (select docnum from etran_gu2v_doc where docid = '" + pereb_c + "') as ETRAN_DOC_NUM, " +
                            "(select  to_date(SUBSTRb(dat, 5, 2) || '.' || (select id_f from mesac where name3 = SUBSTRB(dat, 1, 3)) || '.' || SUBSTRb(dat, 8, 4) || time,'DD.MM.YYYY HH24:MI') as dat " +
                            " from(select DISTINCT planned_filing_date as dat, planned_filing_time as time from etran_gu2v_item where id_doc_etran ='" + pereb_c + "')) as ETRAN_PLAN_DATE_PODACH, " +
                            " (select  distinct position_fio from etran_gu2v_item where id_doc_etran = '" + pereb_c + "') as ETRAN_RZD_PRIM_FIO from dislokacia " +
                            "where " +
                            " EXISTS(select WAGON_NUMBER from ETRAN_GU2V_ITEM where dislokacia.nom_vag= ETRAN_GU2V_ITEM.WAGON_NUMBER and ID_DOC_ETRAN = '" + pereb_c + "')) " +
                            "ds where not EXISTS(select etran_doc_id from COMPLEX.prsd_wagon_load pwl where etran_doc_id is not null and ds.ETRAN_DOC_ID=pwl.etran_doc_id and pwl.date_fix_load is null)";*/
                    string ins_log = "insert into spr_etran_log (DATA,SPR,id_doc_etran) values(sysdate,'GU2V_COMPLEX','" + pereb_c + "')";

                    string kz = "select max(rownum) from spr_collection where id_spr='EMAIL_COMPLEX' and id_sp=(select distinct RAILWAY_STATION_CODE as st from ETRAN_GU2V_ITEM where ID_DOC_ETRAN='" + pereb_c + "')";
                    string nom = "select distinct num||' от '||to_date(SUBSTRb(NOTIFICATION_DATE,5,2)||'.'||(select id_f from mesac where name3=SUBSTRB(NOTIFICATION_DATE,1,3))||'.'||SUBSTRb(NOTIFICATION_DATE,8,4),'DD.MM.YYYY HH24:MI')||' '||NOTIFICATION_TIME||' МСК' " +
                    "as nn from ETRAN_GU2V_ITEM where ID_DOC_ETRAN = '" + pereb_c + "'";
                    string kol_v = "select distinct wagons_total from ETRAN_GU2V_ITEM where ID_DOC_ETRAN = '" + pereb_c + "'";
                    string st_naim = "select distinct railway_station_name||'('||railway_station_code||')' as st from ETRAN_GU2V_ITEM where ID_DOC_ETRAN = '" + pereb_c + "'";
                    string vr_v = "select to_date(SUBSTRb(NOTIFICATION_DATE,5,2)||'.'||(select id_f from mesac where name3=SUBSTRB(NOTIFICATION_DATE,1,3))||'.'||SUBSTRb(NOTIFICATION_DATE,8,4),'DD.MM.YYYY HH24:MI')||' '||NOTIFICATION_TIME||' МСК'  " +
                        " as dat from ETRAN_GU2V_ITEM where rownum=1 and ID_DOC_ETRAN= '" + pereb_c + "'";

                    //   conn.Close();
                    OracleCommand command0 = new OracleCommand(udal, conn);
                    OracleCommand command1 = new OracleCommand(vstav, conn);
                    OracleCommand command2 = new OracleCommand(ins_log, conn);
                    OracleCommand command4 = new OracleCommand(nom, conn);
                    OracleCommand command5 = new OracleCommand(kol_v, conn);
                    OracleCommand command6 = new OracleCommand(vr_v, conn);
                    OracleCommand command9 = new OracleCommand(st_naim, conn);
                    conn.Open();
                    OracleDataReader del_st = command0.ExecuteReader();
                    OracleDataReader ins_Com = command1.ExecuteReader();
                    conn.Close();
                    conn.Open();
                    OracleDataReader logir = command2.ExecuteReader();
                    conn.Close();
                    Console.WriteLine("лог вставлен " + pereb_c);
                    conn.Open();

                    OracleDataReader num = command4.ExecuteReader();
                    OracleDataReader kol = command5.ExecuteReader();
                    OracleDataReader vrem = command6.ExecuteReader();
                    OracleDataReader st_n = command9.ExecuteReader();

                    while (num.Read())
                    { num_c = num.GetValue(0).ToString(); }

                    while (kol.Read())
                    {
                        kol_c = kol.GetValue(0).ToString();
                        while (vrem.Read())
                        {
                            vr_c = vrem.GetValue(0).ToString();
                        }
                        while (st_n.Read())
                        {
                            st_nam = st_n.GetValue(0).ToString();
                        }
                    }
                    conn.Close();

                    OracleCommand command7 = new OracleCommand(kz, conn);
                    conn.Open();
                    OracleDataReader thiskol1 = command7.ExecuteReader();
                    while (thiskol1.Read())
                        if (thiskol1.GetValue(0).ToString() != String.Empty)
                        {
                            ii = Convert.ToInt16(thiskol1.GetValue(0).ToString());
                        }
                    conn.Close();
                    Console.WriteLine("ii=" + ii);
                    for (int i1 = 1; i1 <= ii; i1++)
                    {     //                
                        string email = "select naim from (select rownum as ro ,naim  from spr_collection where id_spr='EMAIL_COMPLEX' " +
                        "and id_sp=(select distinct RAILWAY_STATION_CODE as st from ETRAN_GU2V_ITEM where ID_DOC_ETRAN='" + pereb_c + "'))where ro='" + i1 + "'";
                        OracleCommand command8 = new OracleCommand(email, conn);
                        conn.Open();
                        OracleDataReader them = command8.ExecuteReader();
                        while (them.Read())
                        {
                            address_c = them.GetValue(0).ToString();
                            Console.WriteLine(" I " + i1 + " Переб " + pereb_c + " address " + address_c);
                            string TextPisma = "От РЖД для станции " + st_nam + " пришло уведомление ГУ-2В  №" + num_c + " на " + kol_c + " вагона(ов).";
                            EmOpov opov = new EmOpov();
                            opov.Opov_disp(address_c, TextPisma, num_c);
                        }
                        string em_log_ins = "insert into spr_etran_log (DATA,SPR,id_doc_etran,id_st,email) values(sysdate,'EMAIL_OPOV_GU2V','" + pereb_c + "','" + st_nam + "','" + address_c + "')";
                        OracleCommand command10 = new OracleCommand(em_log_ins, conn);
                        OracleDataReader ins_em_log = command10.ExecuteReader();
                        conn.Close();
                    }


                }
                //Оповещение Вольдемара
                string[] email1 = {"roman@abakan.vspt.ru", "vlad@vspt.ru", "gruz@vspt.ru"};
            //      string[] email1 = { "roman@abakan.vspt.ru" };
                foreach (string address in email1)
                {
                    string TextPisma1 = "От РЖД для станции " + st_nam + " пришло уведомление ГУ-2В  №" + num_c + " на " + kol_c + " вагона(ов).";
                    EmOpov opov1 = new EmOpov();
                    opov1.Opov_disp(address, TextPisma1, num_c);
                }

            }


        }
    }
}



