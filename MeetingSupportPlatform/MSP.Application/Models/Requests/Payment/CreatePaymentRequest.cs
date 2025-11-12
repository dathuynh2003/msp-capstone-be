using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Payment
{
    public class CreatePaymentRequest
    {
        //public string UserId { get; set; }
        //public long OrderCode { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        //public string Signature { get; set; }
    }
}
