using System.Configuration;
using FlightJournal.Web;
using FlightJournal.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twilio;

namespace FlightJournal.Tests.ServiceProviders
{
    [TestClass]
    public class TwilioTest
    {
        [Ignore] // The service costs money - the test does not need to run on each cycle
        [TestMethod]
        [TestCategory("ServiceProviders")]
        public void Twilio_Send_SMS_with_API()
        {
            var settings = ConfigurationManager.GetSection("serviceCredentials") as ServiceCredentialsConfigurationSection;
            if (settings == null)
                throw new ConfigurationErrorsException("Missing ServiceCredentials section");

            if (!string.IsNullOrWhiteSpace(settings.TwilioAccountSid)
                && !string.IsNullOrWhiteSpace(settings.TwilioAuthToken)
                && !string.IsNullOrWhiteSpace(settings.TwilioFromNumber))
            {
                // based of https://www.twilio.com/docs/sms/quickstart/csharp-dotnet-framework?code-sample=code-send-an-sms-using-twilio-with-c&code-language=C%23&code-sdk-version=5.x
                // Find your Account Sid and Auth Token at twilio.com/user/account 
                TwilioClient.Init(settings.TwilioAccountSid, settings.TwilioAuthToken);

                var smsmessage = Twilio.Rest.Api.V2010.Account.MessageResource.Create(
                    body: "Twilio Testing account setup for startlist.club",
                    from: new Twilio.Types.PhoneNumber(settings.TwilioFromNumber),
                    to: new Twilio.Types.PhoneNumber("+4524250682")
                );

                Assert.IsNotNull(smsmessage.Sid);
                Assert.IsNull(smsmessage.ErrorMessage);
            }
            else
            {
                Assert.Fail("Configuration not set");
            }
        }
    }
}
