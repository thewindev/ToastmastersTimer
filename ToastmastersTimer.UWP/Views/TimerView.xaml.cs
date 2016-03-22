﻿using Windows.UI.Xaml;
using ToastmasterTools.Core.ViewModels;

namespace ToastmastersTimer.UWP.Views
{
    public sealed partial class TimerView
    {
        public TimerView()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectLesson();
        }

        public TimerViewModel ViewModel => DataContext as TimerViewModel;

        private void ShowSpeechUI(object sender, RoutedEventArgs e)
        {
            DefaultTimes.SelectedIndex = 0;
            ViewModel.SelectedLesson = ViewModel.Lessons[0];
            ViewModel.ShowSpeechUI();
        }
    }
}