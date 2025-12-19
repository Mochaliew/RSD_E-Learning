namespace RSD_E_Learning.ViewModels
{
    public class StudentMyCourseVm
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Instructor { get; set; } = "";
        public int ProgressPercentage { get; set; }
    }
}
