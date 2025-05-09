using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailJetSettingss { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            _configuration= configuration;
        }




        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }



        // this method will executed and an emial will be send 
        // whenever resgiter a new account / forget password

        public async Task Execute(string email , string subject , string body)
        {
            _mailJetSettingss = _configuration.GetSection("MailJet").Get<MailJetSettings>();

            MailjetClient client = new MailjetClient(_mailJetSettingss.ApiKey, _mailJetSettingss.SecretKey)
            {
                Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
                                     .Property(Send.Messages, new JArray {
                             new JObject {
                              {
                               "From",
                               new JObject {
                                {"Email", "ahlamabedaljawwad99@gmail.com"},
                                {"Name", "BookShop Website"}
                               }
                              }, {
                               "To",
                               new JArray {
                                new JObject {
                                 {
                                  "Email",
                                  email
                                 }, {
                                  "Name",
                                  "BookShop"
                                 }
                                }
                               }
                              }, {
                               "Subject",
                               subject
                              },  {
                               "HTMLPart",
                               body
                              }
                              
                             }
                                     });
                                    await client.PostAsync(request);

        }


    }
}
