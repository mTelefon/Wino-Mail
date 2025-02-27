﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wino.Calendar.Args;
using Wino.Calendar.ViewModels.Data;
using Wino.Core.Domain.Models.Calendar;
using Wino.Helpers;

namespace Wino.Calendar.Controls
{
    public class WinoCalendarControl : Control
    {
        private const string PART_WinoFlipView = nameof(PART_WinoFlipView);
        private const string PART_IdleGrid = nameof(PART_IdleGrid);

        public event EventHandler<TimelineCellSelectedArgs> TimelineCellSelected;
        public event EventHandler<TimelineCellUnselectedArgs> TimelineCellUnselected;

        #region Dependency Properties

        public static readonly DependencyProperty DayRangesProperty = DependencyProperty.Register(nameof(DayRanges), typeof(ObservableCollection<DayRangeRenderModel>), typeof(WinoCalendarControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedFlipViewIndexProperty = DependencyProperty.Register(nameof(SelectedFlipViewIndex), typeof(int), typeof(WinoCalendarControl), new PropertyMetadata(-1));
        public static readonly DependencyProperty SelectedFlipViewDayRangeProperty = DependencyProperty.Register(nameof(SelectedFlipViewDayRange), typeof(DayRangeRenderModel), typeof(WinoCalendarControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ActiveCanvasProperty = DependencyProperty.Register(nameof(ActiveCanvas), typeof(WinoDayTimelineCanvas), typeof(WinoCalendarControl), new PropertyMetadata(null, new PropertyChangedCallback(OnActiveCanvasChanged)));
        public static readonly DependencyProperty IsFlipIdleProperty = DependencyProperty.Register(nameof(IsFlipIdle), typeof(bool), typeof(WinoCalendarControl), new PropertyMetadata(true, new PropertyChangedCallback(OnIdleStateChanged)));

        public DayRangeRenderModel SelectedFlipViewDayRange
        {
            get { return (DayRangeRenderModel)GetValue(SelectedFlipViewDayRangeProperty); }
            set { SetValue(SelectedFlipViewDayRangeProperty, value); }
        }

        public WinoDayTimelineCanvas ActiveCanvas
        {
            get { return (WinoDayTimelineCanvas)GetValue(ActiveCanvasProperty); }
            set { SetValue(ActiveCanvasProperty, value); }
        }

        public bool IsFlipIdle
        {
            get { return (bool)GetValue(IsFlipIdleProperty); }
            set { SetValue(IsFlipIdleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the collection of day ranges to render.
        /// Each day range usually represents a week, but it may support other ranges.
        /// </summary>
        public ObservableCollection<DayRangeRenderModel> DayRanges
        {
            get { return (ObservableCollection<DayRangeRenderModel>)GetValue(DayRangesProperty); }
            set { SetValue(DayRangesProperty, value); }
        }

        public int SelectedFlipViewIndex
        {
            get { return (int)GetValue(SelectedFlipViewIndexProperty); }
            set { SetValue(SelectedFlipViewIndexProperty, value); }
        }

        #endregion

        private WinoCalendarFlipView InternalFlipView;
        private Grid IdleGrid;

        public WinoCalendarControl()
        {
            DefaultStyleKey = typeof(WinoCalendarControl);
            SizeChanged += CalendarSizeChanged;
        }

        private static void OnIdleStateChanged(DependencyObject calendar, DependencyPropertyChangedEventArgs e)
        {
            if (calendar is WinoCalendarControl calendarControl)
            {
                calendarControl.UpdateIdleState();
            }
        }

        private static void OnActiveCanvasChanged(DependencyObject calendar, DependencyPropertyChangedEventArgs e)
        {
            if (calendar is WinoCalendarControl calendarControl)
            {
                if (e.OldValue is WinoDayTimelineCanvas oldCanvas)
                {
                    // Dismiss any selection on the old canvas.
                    calendarControl.DeregisterCanvas(oldCanvas);
                }

                if (e.NewValue is WinoDayTimelineCanvas newCanvas)
                {
                    calendarControl.RegisterCanvas(newCanvas);
                }

                calendarControl.ManageHighlightedDateRange();
            }
        }

        private void ManageHighlightedDateRange()
        {
            if (ActiveCanvas == null)
            {
                SelectedFlipViewDayRange = null;
            }
            else
            {
                SelectedFlipViewDayRange = InternalFlipView.SelectedItem as DayRangeRenderModel;
            }
        }

        private void DeregisterCanvas(WinoDayTimelineCanvas canvas)
        {
            if (canvas == null) return;

            canvas.SelectedDateTime = null;
            canvas.TimelineCellSelected -= ActiveTimelineCellSelected;
            canvas.TimelineCellUnselected -= ActiveTimelineCellUnselected;
        }

        private void RegisterCanvas(WinoDayTimelineCanvas canvas)
        {
            if (canvas == null) return;

            canvas.SelectedDateTime = null;
            canvas.TimelineCellSelected += ActiveTimelineCellSelected;
            canvas.TimelineCellUnselected += ActiveTimelineCellUnselected;
        }

        private void CalendarSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActiveCanvas == null) return;

            ActiveCanvas.SelectedDateTime = null;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InternalFlipView = GetTemplateChild(PART_WinoFlipView) as WinoCalendarFlipView;
            IdleGrid = GetTemplateChild(PART_IdleGrid) as Grid;

            UpdateIdleState();
        }

        private void UpdateIdleState()
        {
            InternalFlipView.Opacity = IsFlipIdle ? 0 : 1;
            IdleGrid.Visibility = IsFlipIdle ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ActiveTimelineCellUnselected(object sender, TimelineCellUnselectedArgs e)
            => TimelineCellUnselected?.Invoke(this, e);

        private void ActiveTimelineCellSelected(object sender, TimelineCellSelectedArgs e)
            => TimelineCellSelected?.Invoke(this, e);

        public void NavigateToDay(DateTime dateTime) => InternalFlipView.NavigateToDay(dateTime);

        public void ResetTimelineSelection()
        {
            if (ActiveCanvas == null) return;

            ActiveCanvas.SelectedDateTime = null;
        }

        public void GoNextRange()
        {
            if (InternalFlipView == null) return;

            InternalFlipView.GoNextFlip();
        }

        public void GoPreviousRange()
        {
            if (InternalFlipView == null) return;

            InternalFlipView.GoPreviousFlip();
        }

        public CalendarItemControl GetCalendarItemControl(CalendarItemViewModel calendarItemViewModel)
        {
            if (ActiveCanvas == null) return null;

            return this.FindDescendants<CalendarItemControl>().FirstOrDefault(a => a.CalendarItem == calendarItemViewModel);
        }
    }
}
