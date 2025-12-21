namespace RSD_E_Learning.ViewModels
{
    public class StudentDashboardVm
    {
        public List<RecentLessonVm> RecentLessons { get; set; } = new();
        public List<PendingFinalExamVm> PendingFinalExams { get; set; } = new();
    }

    public class RecentLessonVm
    {
        public string CourseTitle { get; set; } = "";
        public string LessonTitle { get; set; } = "";
        public DateTime? ScheduleDate { get; set; }
    }

    public class PendingFinalExamVm
    {
        public int FinalId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime DeadLine { get; set; }
        public int AttemptsUsed { get; set; }
    }
}
