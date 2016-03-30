﻿using System;
using System.Diagnostics;
using Windows.Phone.Devices.Notification;
using Windows.UI;
using Windows.UI.Xaml;
using ToastmasterTools.Core.Features.Analytics;
using ToastmasterTools.Core.Helpers;
using ToastmasterTools.Core.Models;
using ToastmasterTools.Core.Mvvm;
using ToastmasterTools.Core.Services.SettingsServices;

namespace ToastmasterTools.Core.ViewModels
{
    public class ToastmastersTimerViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IAppSettings _appSettings;
        private bool _timerIsRunning;
        Stopwatch _stopWatch;
        private string _secondsText;
        private string _minutesText;
        private DispatcherTimer _dispatcherTimer;
        private Color _selectedBackground;
        private Speech _currentSpeech;

        private TimerState CurrentState { get; set; }

        private Color GreenTimeBackground { get; }

        private Color YellowTimeBackground { get; }

        private Color RedTimeBackground { get; }

        public ToastmastersTimerViewModel(IStatisticsService statisticsService, IAppSettings appSettings)
        {
            _statisticsService = statisticsService;
            _appSettings = appSettings;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SelectedBackground = DarkBackground;
                return;
            }
            GreenTimeBackground = Colors.ForestGreen;
            YellowTimeBackground = Color.FromArgb(255, 242, 223, 116);
            RedTimeBackground = Color.FromArgb(255, 205, 32, 44);
            Reset();
        }

        #region Properties

       
        public bool TimerIsRunning
        {
            get { return _timerIsRunning; }
            set
            {
                _timerIsRunning = value;
                RaisePropertyChanged();
                _timerIsRunning = value;
            }
        }

        public Color SelectedBackground
        {
            get { return _selectedBackground; }
            set
            {
                if (_selectedBackground == value)
                {
                    return;
                }
                _selectedBackground = value;
                RaisePropertyChanged();
            }
        }

        private void Vibrate()
        {
            try
            {
                if (_appSettings.Get<bool>(StorageKey.VibrationEnabled))
                {
                    VibrationDevice v = VibrationDevice.GetDefault();
                    v.Vibrate(TimeSpan.FromMilliseconds(500));
                }
            }
            catch (Exception)
            {

            }
        }

        public string MinutesText
        {
            get { return _minutesText; }
            set
            {
                _minutesText = value;
                RaisePropertyChanged();
            }
        }

        public string SecondsText
        {
            get { return _secondsText; }
            set
            {
                _secondsText = value;
                RaisePropertyChanged();
            }
        }

        public Speech CurrentSpeech
        {
            get { return _currentSpeech; }
            set
            {
                _currentSpeech = value;
                RaisePropertyChanged();
            }
        }

        private TimeSpan GreenCardTimeSpan => GetTimeSpanFromCardTime(CurrentSpeech.SpeechType.GreenCardTime);

        private TimeSpan YellowCardTimeSpan => GetTimeSpanFromCardTime(CurrentSpeech.SpeechType.YellowCardTime);

        private TimeSpan RedCardTimeSpan => GetTimeSpanFromCardTime(CurrentSpeech.SpeechType.RedCardTime);

        #endregion

        #region Timer

        public Color DarkBackground { get; set; } = Color.FromArgb(255, 0, 63, 97);
        public event EventHandler<Speech> SpeechStopped ;

        public void StartTimer()
        {
            _dispatcherTimer.Start();
            _stopWatch.Start();
            TimerIsRunning = true;
            _statisticsService.RegisterEvent(EventCategory.UserEvent, EventAction.Timer, "start_" + _currentSpeech.SpeechType.Name);
        }

        public void PauseTimer()
        {
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Stop();
                _stopWatch.Stop();
                TimerIsRunning = false;
            }
            else
            {
                _dispatcherTimer.Start();
                _stopWatch.Start();
                TimerIsRunning = true;
            }
            _statisticsService.RegisterEvent(EventCategory.UserEvent, EventAction.Timer, "pause_" + _currentSpeech.SpeechType.Name);
        }

        public void StopTimer()
        {
            _statisticsService.RegisterEvent(EventCategory.UserEvent, EventAction.Timer, "stop_" + _currentSpeech.SpeechType.Name);
            OnSpeechStopped(CurrentSpeech);
            CurrentSpeech.SpeechTimeInSeconds = _stopWatch.Elapsed.TotalSeconds;
            _dispatcherTimer?.Stop();
            _stopWatch?.Stop();
        }

        #endregion

        #region Private

        public void Reset()
        {
            SelectedBackground = DarkBackground;
            MinutesText = SecondsText = "00";
            _dispatcherTimer?.Stop();
            _stopWatch?.Stop();
            _stopWatch = new Stopwatch();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            _dispatcherTimer.Tick += TimerCallback;
            TimerIsRunning = false;
            CurrentState = TimerState.None;
        }

        private void TimerCallback(object sender, object e)
        {
            var timeSpan = _stopWatch.Elapsed;
            MinutesText = timeSpan.Minutes.GetTimeText();
            SecondsText = timeSpan.Seconds.GetTimeText();
            UpdateBackground(timeSpan);
        }

        private void UpdateBackground(TimeSpan timeSpan)
        {
            if (TimeIsBetweenGreenAndYellow(timeSpan))
                SwitchToGreen();
            else if (TimeIsBetweenYellowAndRed(timeSpan))
            {
                SwitchToYellow();
            }
            else if (TimeIsAfterRed(timeSpan))
                SwitchToRed();
        }

        private void SwitchToRed()
        {
            if (CurrentState == TimerState.Yellow)
                Vibrate();
            CurrentState = TimerState.Red;
            SelectedBackground = RedTimeBackground;
        }

        private void SwitchToYellow()
        {
            if (CurrentState == TimerState.Green)
                Vibrate();
            CurrentState = TimerState.Yellow;
            SelectedBackground = YellowTimeBackground;
        }

        private void SwitchToGreen()
        {
            if (CurrentState == TimerState.None)
                Vibrate();
            CurrentState = TimerState.Green;
            SelectedBackground = GreenTimeBackground;
        }

        private bool TimeIsBetweenGreenAndYellow(TimeSpan timeSpan)
        {
            return timeSpan.TotalMilliseconds >= GreenCardTimeSpan.TotalMilliseconds &&
                   timeSpan.TotalMilliseconds < YellowCardTimeSpan.TotalMilliseconds;
        }

        private bool TimeIsBetweenYellowAndRed(TimeSpan timeSpan)
        {
            return timeSpan.TotalMilliseconds >= YellowCardTimeSpan.TotalMilliseconds &&
                   timeSpan.TotalMilliseconds < RedCardTimeSpan.TotalMilliseconds;
        }

        private bool TimeIsAfterRed(TimeSpan timeSpan)
        {
            return timeSpan.TotalMilliseconds >= RedCardTimeSpan.TotalMilliseconds;
        }

        private TimeSpan GetTimeSpanFromCardTime(CardTime cardTime)
        {
            return new TimeSpan(0, cardTime.Minutes, cardTime.Seconds);
        }

        #endregion

        protected virtual void OnSpeechStopped(Speech e)
        {
            SpeechStopped?.Invoke(this, e);
        }
    }
}
