using AutoMapper;
using Warehouse.Services.DTO;
using Warehouse.Services.Iservices;
using Warehouse.Data;
using Warehouse.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Services.services
{
    public class EmailService : IEmailService
    {
        private IUnitOfWork _UnitOfWork;
        private readonly IMapper _mapper;
        public EmailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _UnitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public Task SendAsync(IdentityMessage message)
        {

            if (message.Subject.Contains("Addnewpassword"))
            {
                // Task.Factory.StartNew(() =>
                //{
                //    sendMail_resetPassword(message);
                //});
               
                return Task.Factory.StartNew(() =>
                {
                    sendMail_resetPassword(message);
                });

            }
            else //if (message.Subject == "Security Code")
            {
                // Task.Factory.StartNew(() =>
                //{
                //    sendMail_SecurityCode(message);
                //});
                

                return Task.Factory.StartNew(() =>
                {
                    sendMail_SecurityCode(message);
                });

            }
           


        }



        void sendMail_resetPassword(IdentityMessage message)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("account@xlinecards.com");
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = "Reset password";
            msg.IsBodyHtml = true;
            msg.Body = GetWebPageContent(message.Body, message.Subject);

            SmtpClient smtpClient = new SmtpClient("mail5008.site4now.net", 25);
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("account@xlinecards.com", "1q2w!Q@WKhalil");
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = false;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            string userState = "test message1";
            smtpClient.SendAsync(msg, userState);
        }

        void sendMail_SecurityCode(IdentityMessage message)
        {

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("account@xlinecards.com");
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
            msg.IsBodyHtml = false;
            msg.Body = message.Body; //GetWebPageContent(message.Body, message.Subject);

            SmtpClient smtpClient = new SmtpClient("mail5008.site4now.net", 25);
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("account@xlinecards.com", "1q2w!Q@WKhalil");
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = false;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            string userState = "test message1";
            smtpClient.SendAsync(msg, userState);
        }

        private static string GetWebPageContent(string path, string link)
        {

            //  string filePath = @"E:\New folder (2)\خليل\محاضرات الاتصالات\ماجستير خليل\نمذجة مقاسم\курсовая работа\c sharp code\deleteme\WindowsFormsApplication1\index.html";
            string res = System.IO.File.ReadAllText(path);
            res = res.Replace("<a href=\"\"", "<a href=\"" + link + "\"");
            res = res.Replace("<a href=\"tt\">", link);
            ////<a href=
            return res;
        }
    }
}
