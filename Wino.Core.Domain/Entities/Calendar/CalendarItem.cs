﻿using System;
using Itenso.TimePeriod;
using SQLite;
using Wino.Core.Domain.Enums;
using Wino.Core.Domain.Interfaces;

namespace Wino.Core.Domain.Entities.Calendar
{
    public class CalendarItem : ICalendarItem
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string RemoteEventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate
        {
            get
            {
                return StartDate.AddSeconds(DurationInSeconds);
            }
        }

        public TimeSpan StartDateOffset { get; set; }
        public TimeSpan EndDateOffset { get; set; }

        private ITimePeriod _period;
        public ITimePeriod Period
        {
            get
            {
                _period ??= new TimeRange(StartDate, EndDate);

                return _period;
            }
        }

        /// <summary>
        /// Events that starts at midnight and ends at midnight are considered all-day events.
        /// </summary>
        public bool IsAllDayEvent
        {
            get
            {
                return
                    StartDate.TimeOfDay == TimeSpan.Zero &&
                    EndDate.TimeOfDay == TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Events that are not all-day events and last more than one day are considered multi-day events.
        /// </summary>
        public bool IsMultiDayEvent
        {
            get
            {
                return Period.Duration.TotalDays >= 1 && !IsAllDayEvent;
            }
        }

        public double DurationInSeconds { get; set; }
        public string Recurrence { get; set; }

        /// <summary>
        /// The id of the parent calendar item of the recurring event.
        /// Exceptional instances are stored as a separate calendar item.
        /// This makes the calendar item a child of the recurring event.s
        /// </summary>
        public Guid? RecurringCalendarItemId { get; set; }

        /// <summary>
        /// Indicates read-only events. Default is false.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Hidden events must not be displayed to the user.
        /// This usually happens when a child instance of recurring parent hapens.
        /// </summary>
        public bool IsHidden { get; set; }

        // TODO
        public string CustomEventColorHex { get; set; }
        public string HtmlLink { get; set; }
        public CalendarItemStatus Status { get; set; }
        public CalendarItemVisibility Visibility { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid CalendarId { get; set; }

        [Ignore]
        public IAccountCalendar AssignedCalendar { get; set; }
    }
}
