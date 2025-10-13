using System;

namespace Services.Utils
{
    /// <summary>
    /// Utility class for converting DateTime to Vietnam timezone (UTC+7)
    /// Provides methods to convert between UTC and Vietnam local time
    /// </summary>
    public static class TimeZoneConverter
    {
        // Vietnam timezone info (UTC+7)
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        
        /// <summary>
        /// Convert UTC DateTime to Vietnam local time (UTC+7)
        /// </summary>
        /// <param name="utcDateTime">UTC DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ConvertUtcToVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Input DateTime must be in UTC format", nameof(utcDateTime));
            }
            
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Convert local DateTime to Vietnam time (UTC+7)
        /// Assumes input is in system local time
        /// </summary>
        /// <param name="localDateTime">Local DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ConvertLocalToVietnamTime(DateTime localDateTime)
        {
            // Convert local time to UTC first, then to Vietnam time
            var utcDateTime = localDateTime.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Convert any DateTime to Vietnam time
        /// Handles different DateTimeKind values appropriately
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ConvertToVietnamTime(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => ConvertUtcToVietnamTime(dateTime),
                DateTimeKind.Local => ConvertLocalToVietnamTime(dateTime),
                DateTimeKind.Unspecified => TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), VietnamTimeZone),
                _ => throw new ArgumentException("Invalid DateTimeKind", nameof(dateTime))
            };
        }

        /// <summary>
        /// Convert Vietnam time to UTC
        /// </summary>
        /// <param name="vietnamDateTime">DateTime in Vietnam timezone</param>
        /// <returns>UTC DateTime</returns>
        public static DateTime ConvertVietnamTimeToUtc(DateTime vietnamDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Get current time in Vietnam timezone
        /// </summary>
        /// <returns>Current DateTime in Vietnam timezone</returns>
        public static DateTime GetVietnamCurrentTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        /// <summary>
        /// Convert nullable DateTime to Vietnam time
        /// </summary>
        /// <param name="dateTime">Nullable DateTime to convert</param>
        /// <returns>Nullable DateTime in Vietnam timezone</returns>
        public static DateTime? ConvertToVietnamTime(DateTime? dateTime)
        {
            return dateTime?.let(dt => ConvertToVietnamTime(dt));
        }

        /// <summary>
        /// Format DateTime to Vietnam time string with default format
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="format">Format string (default: "dd/MM/yyyy HH:mm:ss")</param>
        /// <returns>Formatted datetime string in Vietnam timezone</returns>
        public static string FormatVietnamTime(DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            var vietnamTime = ConvertToVietnamTime(dateTime);
            return vietnamTime.ToString(format);
        }

        /// <summary>
        /// Format nullable DateTime to Vietnam time string
        /// </summary>
        /// <param name="dateTime">Nullable DateTime to format</param>
        /// <param name="format">Format string (default: "dd/MM/yyyy HH:mm:ss")</param>
        /// <param name="nullText">Text to return if datetime is null (default: "N/A")</param>
        /// <returns>Formatted datetime string in Vietnam timezone or null text</returns>
        public static string FormatVietnamTime(DateTime? dateTime, string format = "dd/MM/yyyy HH:mm:ss", string nullText = "N/A")
        {
            if (dateTime == null)
                return nullText;
            
            return FormatVietnamTime(dateTime.Value, format);
        }

        /// <summary>
        /// Check if a DateTime is within Vietnam business hours (8:00 AM - 5:00 PM)
        /// </summary>
        /// <param name="dateTime">DateTime to check</param>
        /// <returns>True if within business hours, false otherwise</returns>
        public static bool IsVietnamBusinessHours(DateTime dateTime)
        {
            var vietnamTime = ConvertToVietnamTime(dateTime);
            var hour = vietnamTime.Hour;
            return hour >= 8 && hour < 17; // 8:00 AM to 5:00 PM
        }

        /// <summary>
        /// Get the start of day (00:00:00) in Vietnam timezone for a given date
        /// </summary>
        /// <param name="date">Date to get start of day for</param>
        /// <returns>DateTime representing start of day in Vietnam timezone</returns>
        public static DateTime GetVietnamStartOfDay(DateTime date)
        {
            var vietnamTime = ConvertToVietnamTime(date);
            return new DateTime(vietnamTime.Year, vietnamTime.Month, vietnamTime.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Get the end of day (23:59:59.999) in Vietnam timezone for a given date
        /// </summary>
        /// <param name="date">Date to get end of day for</param>
        /// <returns>DateTime representing end of day in Vietnam timezone</returns>
        public static DateTime GetVietnamEndOfDay(DateTime date)
        {
            var vietnamTime = ConvertToVietnamTime(date);
            return new DateTime(vietnamTime.Year, vietnamTime.Month, vietnamTime.Day, 23, 59, 59, 999, DateTimeKind.Unspecified);
        }
    }

    /// <summary>
    /// Extension methods for DateTime to make timezone conversion more convenient
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Extension method to convert DateTime to Vietnam time
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ToVietnamTime(this DateTime dateTime)
        {
            return TimeZoneConverter.ConvertToVietnamTime(dateTime);
        }

        /// <summary>
        /// Extension method to convert nullable DateTime to Vietnam time
        /// </summary>
        /// <param name="dateTime">Nullable DateTime to convert</param>
        /// <returns>Nullable DateTime in Vietnam timezone</returns>
        public static DateTime? ToVietnamTime(this DateTime? dateTime)
        {
            return TimeZoneConverter.ConvertToVietnamTime(dateTime);
        }

        /// <summary>
        /// Extension method to format DateTime as Vietnam time string
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="format">Format string</param>
        /// <returns>Formatted datetime string in Vietnam timezone</returns>
        public static string ToVietnamTimeString(this DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            return TimeZoneConverter.FormatVietnamTime(dateTime, format);
        }

        /// <summary>
        /// Extension method to format nullable DateTime as Vietnam time string
        /// </summary>
        /// <param name="dateTime">Nullable DateTime to format</param>
        /// <param name="format">Format string</param>
        /// <param name="nullText">Text to return if datetime is null</param>
        /// <returns>Formatted datetime string in Vietnam timezone or null text</returns>
        public static string ToVietnamTimeString(this DateTime? dateTime, string format = "dd/MM/yyyy HH:mm:ss", string nullText = "N/A")
        {
            return TimeZoneConverter.FormatVietnamTime(dateTime, format, nullText);
        }
    }
}

// Helper extension for functional programming style
public static class FunctionalExtensions
{
    public static TResult let<T, TResult>(this T obj, Func<T, TResult> func)
    {
        return func(obj);
    }
}
