using SQLite4Unity3d;

namespace AL.Database
{
    public class StepResult
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string UserName { get; set; }
        public int StepNumber { get; set; }
        public string Name { get; set; }
        public int TimeTaken { get; set; }
        public string Status { get; set; }
        public int WrongAttempts { get; set; }

        public override string ToString()
        {
            return string.Format("[Person: Id={0}, UserName={1}, StartDate={2}, StepNumber={3}, Name={4}, TimeTaken={5}, Status={6}, WrongAttempts={7}]", Id, UserName, StartDate, StepNumber, Name, TimeTaken, Status, WrongAttempts);
        }
    }
}
