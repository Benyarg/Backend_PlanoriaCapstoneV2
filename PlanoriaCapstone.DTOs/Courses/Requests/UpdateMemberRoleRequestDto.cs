using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Courses.Requests
{
    public class UpdateMemberRoleRequestDto
    {
        [Required(ErrorMessage = "El rol es requerido")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El rol debe tener entre 1 y 20 caracteres")]
        public string Role { get; set; }
    }
}