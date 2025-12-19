using Application.Common.Interfaces;

namespace Infrastructure.Storage
{
    //1
    public class DatePartitionedPathStrategy : IStoragePathStrategy
    {
        private readonly string _root;

        public DatePartitionedPathStrategy(string root)
        {
            _root = root;
        }

        //public string GetPathForNewFile(string originalName)
        //{
        //    var year = DateTime.UtcNow.Year.ToString();
        //    var month = DateTime.UtcNow.Month.ToString("D2");
        //    var day = DateTime.UtcNow.Day.ToString("D2");

        //    return Path.Combine(_root, year, month, day, originalName);
        //}
        public string GetPathForNewFile(string originalName)
        {
            var year = DateTime.Now.Year.ToString();
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            return Path.Combine(_root, year, month, day, originalName);
        }

    }
}
