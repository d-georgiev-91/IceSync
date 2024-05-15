namespace IceSync.CommonAbstractions
{
    public interface IDateTimeService
    {
        public DateTime UtcNow { get; }
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
