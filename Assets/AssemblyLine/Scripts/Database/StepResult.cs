using SQLite4Unity3d;

namespace AL.Database
{
    public class StepResult
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StartDate { get; set; }
        public int StepNumber { get; set; }
        public string Name { get; set; }
        public int TimeTaken { get; set; }
        public string Status { get; set; }
        public int WrongAttempts { get; set; }

        public override string ToString()
        {
            return string.Format("[Person: Id={0}, StartDate={1}, StepNumber={2}, Name={3}, TimeTaken={4}, Status={5}, WrongAttempts={6}]", Id, StartDate, StepNumber, Name, TimeTaken, Status, WrongAttempts);
        }
    }
}
