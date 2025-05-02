using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biuro_Podróży.AppHost.Models.DTOs
{
    public class ClientCreateDTO
    {
        [MaxLength(120)]
        public string FirstName { get; set; }
        [MaxLength(120)]
        public string LastName { get; set; }
        [MaxLength(120)]
        [RegularExpression("^.+@.+\\..+$")]
        public string Email { get; set; }
        [MaxLength(120)]
        public string PhoneNumber { get; set; }
        [MaxLength(120)]
        public string Pesel { get; set; }
    }
    public class ClientGetDTO {
        public int ClientID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Pesel { get; set; }
        
    }
}
