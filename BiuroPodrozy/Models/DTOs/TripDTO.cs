using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biuro_Podróży.AppHost.Models.DTOs
{
    public class TripCreateDTO
    {
        [MaxLength(120)]
        public string Name { get; set; }
        [MaxLength(120)]
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }
    }
    public class TripGetDTO 
    {
        public int TripID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }
    }
    public class TripWithRegisterationGetDTO : TripGetDTO {
        public int RegisteredAt { get; set; }
        public int? PaymentDate { get; set; }
    }
    public class TripWithCountryGetDTO : TripGetDTO {
        public CountryGetDTO Country { get; set; }
    }
}
