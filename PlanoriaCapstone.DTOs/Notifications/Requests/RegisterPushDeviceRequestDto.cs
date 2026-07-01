using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Notifications.Requests
{
    public class RegisterPushDeviceRequestDto
    {
        [Required(ErrorMessage = "El token del dispositivo es obligatorio.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El token del dispositivo debe tener entre {2} y {1} caracteres.")]
        public string DeviceToken { get; set; }

        [Required(ErrorMessage = "La plataforma es obligatoria.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "La plataforma debe tener entre {2} y {1} caracteres.")]
        public string Platform { get; set; }

        [StringLength(100, ErrorMessage = "El nombre del dispositivo no puede superar los {1} caracteres.")]
        public string DeviceName { get; set; }
    }
}
