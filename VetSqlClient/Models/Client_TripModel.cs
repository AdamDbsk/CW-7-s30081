using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biuro_Podróży.AppHost.Models;

class Client_TripModel
{
    public int ClientID { get; set; }
    public int TripID { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}
