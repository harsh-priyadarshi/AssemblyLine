using SQLite4Unity3d;

namespace AL.Database
{
    [System.Serializable]
    public class Person
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("[Person: Id={0}, UserName={1},  Name={2}, Password={3}]", Id, UserName, Name, Password);
        }
    }
}
