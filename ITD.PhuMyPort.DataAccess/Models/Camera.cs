using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.DataAccess.Models
{
    public class Camera
    {
        [Key]
        [Required(ErrorMessage = "Camera code is required")]
        [MaxLength(50, ErrorMessage = "Camera Code max length is 50 characters")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Camera name is required")]
        [MaxLength(250, ErrorMessage = "Camera Name max length is 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "IP is required")]
        [MaxLength(250, ErrorMessage = "IP max length is 250 characters")]
        public string IP { get; set; }

        [Required(ErrorMessage = "Port is required")]
        [MaxLength(10, ErrorMessage = "Port max length is 10 characters")]
        public string Port { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50, ErrorMessage = "Username max length is 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(250, ErrorMessage = "Password max length is 250 characters")]
        public string Password { get; set; }

        //ForeignKey
        public string WorkplaceCode { get; set; }
        [ForeignKey("WorkplaceCode")]
        public Workplace Workplace { get; set; }
    }
}
