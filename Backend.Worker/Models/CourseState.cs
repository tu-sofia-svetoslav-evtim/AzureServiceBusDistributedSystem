namespace Backend.Worker.Models
{
	public class CourseState
	{
		public string Name { get; set; }
		public int Capacity { get; set; }
		public List<string> EnrolledStudents { get; set; } = new List<string>();
	}
}