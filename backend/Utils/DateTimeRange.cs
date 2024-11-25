namespace DashboardApi.Utils
{
    public class DateTimeRange
    {

        #region Construction
        public DateTimeRange()
        {

        }
        public DateTimeRange(DateTime start, DateTime end)
        {
            if (start > end)
            {
                throw new Exception("Invalid range edges.");
            }
            _Start = start;
            _End = end;
        }
        #endregion

        #region Properties
        private DateTime _Start;

        public DateTime Start
        {
            get { return _Start; }
            private set { _Start = value; }
        }
        private DateTime _End;

        public DateTime End
        {
            get { return _End; }
            private set { _End = value; }
        }

        public double Hours => GetHoursInRange();
        #endregion

        #region Operators
        public static bool operator ==(DateTimeRange range1, DateTimeRange range2)
        {
            return range1.Equals(range2);
        }

        public static bool operator !=(DateTimeRange range1, DateTimeRange range2)
        {
            return !(range1 == range2);
        }
        public override bool Equals(object obj)
        {
            if (obj is DateTimeRange)
            {
                var range1 = this;
                var range2 = (DateTimeRange)obj;
                return range1.Start == range2.Start && range1.End == range2.End;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Querying
        public bool Intersects(DateTimeRange range)
        {

            var type = GetIntersectionType(range);
            return type != IntersectionType.None;
        }
        public bool IsInRange(DateTime date)
        {
            return date >= Start && date <= End;
        }

        public IntersectionType GetIntersectionType(DateTimeRange range)
        {
            if (this == range)
            {
                return IntersectionType.RangesEqauled;
            }

            if (IsInRange(range.Start) && IsInRange(range.End))
            {
                return IntersectionType.ContainedInRange;
            }
            if (IsInRange(range.Start))
            {
                return IntersectionType.StartsInRange;
            }
            if (IsInRange(range.End))
            {
                return IntersectionType.EndsInRange;
            }
            if (range.IsInRange(Start) && range.IsInRange(End))
            {
                return IntersectionType.ContainsRange;
            }

            return IntersectionType.None;
        }
        public DateTimeRange GetIntersection(DateTimeRange range)
        {
            var type = GetIntersectionType(range);

            if (type == IntersectionType.RangesEqauled || type == IntersectionType.ContainedInRange)
            {
                return range;
            }

            if (type == IntersectionType.StartsInRange)
            {
                return new DateTimeRange(range.Start, End);
            }
            if (type == IntersectionType.EndsInRange)
            {
                return new DateTimeRange(Start, range.End);
            }
            if (type == IntersectionType.ContainsRange)
            {
                return this;
            }
            return new DateTimeRange();
        }

        private double GetHoursInRange()
        {
            if (Start.Year < 2000)
            {
                return 0;
            }

            return (End - Start).TotalHours;
        }
        #endregion


        public override string ToString()
        {
            return Start.ToString() + " - " + End.ToString();
        }
    }

    public enum IntersectionType
    {
        /// <summary>
        /// No Intersection
        /// </summary>
        None = -1,
        /// <summary>
        /// Given range ends inside the range
        /// </summary>
        EndsInRange,
        /// <summary>
        /// Given range starts inside the range
        /// </summary>
        StartsInRange,
        /// <summary>
        /// Both ranges are equaled
        /// </summary>
        RangesEqauled,
        /// <summary>
        /// Given range contained in the range
        /// </summary>
        ContainedInRange,
        /// <summary>
        /// Given range contains the range
        /// </summary>
        ContainsRange,
    }
}
