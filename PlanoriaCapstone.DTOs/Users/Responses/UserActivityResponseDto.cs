using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Responses
{
    public class UserActivityResponseDto
    {
        public DateTime? LastLogin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo de estudio debe ser mayor o igual a 0")]
        public int TotalStudyTime { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Las tarjetas revisadas deben ser mayor o igual a 0")]
        public int TotalCardsReviewed { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los cuestionarios completados deben ser mayor o igual a 0")]
        public int TotalQuizzesCompleted { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los días de racha deben ser mayor o igual a 0")]
        public int StreakDays { get; set; }
    }
}