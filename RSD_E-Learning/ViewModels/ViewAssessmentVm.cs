
using System;
using System.Collections.Generic;
using static RSD_E_Learning.Models.DB;

namespace RSD_E_Learning.ViewModels
{
    public class ViewAssessmentVm
    {
        public Assessment Assessment { get; set; }
        public List<AssessmentQuestion> Questions { get; set; }
    }
}