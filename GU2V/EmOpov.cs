using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GU2V
{
    class EmOpov
    {
        public void Opov_err(string address,string TextPisma,string Zagolovok)
        {
              
                DateTime parsedDate = DateTime.Now;
                SmtpClient Smtp = new SmtpClient("mail.vspt.org", 25);
                Smtp.EnableSsl = false;
                Smtp.Credentials = new NetworkCredential("robot", "Abakan_mail18");      // Логин и пароль почты отправителя            
                MailMessage MyMessage = new MailMessage();
                MyMessage.From = new MailAddress("robot@abakan.vspt.ru");      // От кого отправляем почту
                MyMessage.To.Add(address);                       // Кому отправляем почту
                MyMessage.Subject = Zagolovok;          // Тема письма
                MyMessage.Body = " Здравтсвуйте! \n " +
                TextPisma;
                Smtp.Send(MyMessage);
        
        }

        

        public void Opov_disp(string address, string TextPisma, string Naim_doc)
        {
          
                DateTime parsedDate = DateTime.Now.AddHours(-4);
                SmtpClient Smtp = new SmtpClient("mail.vspt.org", 25);
                Smtp.EnableSsl = false;
                Smtp.Credentials = new NetworkCredential("robot", "Abakan_mail18");      // Логин и пароль почты отправителя            
                MailMessage MyMessage = new MailMessage();
                MyMessage.From = new MailAddress("robot@vspt.ru");      // От кого отправляем почту
                MyMessage.To.Add(address);                    // Кому отправляем почту
                MyMessage.Subject = "ГУ2В № "+Naim_doc+" загружено в Комлекс АО";          // Тема письма
                MyMessage.Body = "Здравствуйте! \n" +
                TextPisma + "\n"+
                "Данные загружены в 'КОМПЛЕКС Приёмосдатчик' " + parsedDate+ "(МСК) \n" +
                "Это письмо сформировано автоматически.Отвечать на него не нужно.";
                Smtp.Send(MyMessage);               
        }
       


    }
}
