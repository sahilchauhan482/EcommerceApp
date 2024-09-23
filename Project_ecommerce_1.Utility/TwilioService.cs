using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Project_ecommerce_1.Utility
{
    public class TwilioService
    {
        private readonly TwilioSettings _twilioSettings;

        public TwilioService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public async Task SendOrderConfirmationSMS(string toPhoneNumber, string smsBody)
        {
            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                body: smsBody);

            // You can handle the response or log it if needed.
        }
    }
}