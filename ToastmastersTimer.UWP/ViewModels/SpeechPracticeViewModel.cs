﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using ToastmastersTimer.UWP.Mvvm;

namespace ToastmastersTimer.UWP.ViewModels
{
    public class SpeechPracticeViewModel : ViewModelBase
    {
        private string _speechText;
        private SpeechRecognizer _speechRecognizer;
        private double _fontSize;
        private string _speechResult;
        private bool _isListening;

        public SpeechPracticeViewModel()
        {
            FontSize = 32;
            SpeechText =
                @"Let every nation know, whether it wishes us well or ill, that we shall pay any price, bear any
burden, meet any hardship, support any friend, oppose any foe, to assure the survival and the
success of liberty.
This much we pledge --and more.
To those old allies whose cultural and spiritual origins we share, we pledge the loyalty of
faithful friends.United there is little we cannot do in a host of cooperative ventures.Divided
there is little we can do -- for we dare not meet a powerful challenge at odds and split
asunder.
To those new states whom we welcome to the ranks of the free, we pledge our word that one
form of colonial control shall not have passed away merely to be replaced by a far more iron
tyranny.We shall not always expect to find them supporting our view.But we shall always
hope to find them strongly supporting their own freedom-- and to remember that, in the
past, those who foolishly sought power by riding the back of the tiger ended up inside.
To those people in the huts and villages of half the globe struggling to break the bonds of
mass misery, we pledge our best efforts to help them help themselves, for whatever period is
required --not because the Communists may be doing it, not because we seek their votes,
but because it is right.If a free society cannot help the many who are poor, it cannot save the
few who are rich.
To our sister republics south of our border, we offer a special pledge: to convert our good
words into good deeds, in a new alliance for progress, to assist free men and free
governments in casting off the chains of poverty.But this peaceful revolution of hope cannot
become the prey of hostile powers.Let all our neighbors know that we shall join with them to
oppose aggression or subversion anywhere in the Americas.And let every other power know
that this hemisphere intends to remain the master of its own house.
To that world assembly of sovereign states, the United Nations, our last best hope in an age
where the instruments of war have far outpaced the instruments of peace, we renew our
pledge of support-- to prevent it from becoming merely a forum for invective, to strengthen
its shield of the new and the weak, and to enlarge the area in which its writ may run.
Finally, to those nations who would make themselves our adversary, we offer not a pledge but
a request: that both sides begin anew the quest for peace, before the dark powers of
destruction unleashed by science engulf all humanity in planned or accidental self - destruction.
We dare not tempt them with weakness.For only when our arms are sufficient beyond doubt
can we be certain beyond doubt that they will never be employed.";
        }

        public bool IsListening
        {
            get { return _isListening; }
            set { _isListening = value; RaisePropertyChanged(); }
        }

        private void SpeechRecognizerHypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            Debug.WriteLine(args.Hypothesis.Text);
            /*await
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                    });*/
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            Debug.WriteLine(args.Result.Text);
            await
                  Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                      CoreDispatcherPriority.Normal, () =>
                      {
                            SpeechResult = args.Result.Text;
                            ProcessCommands(args.Result);
                      });
        }

        private void ProcessCommands(SpeechRecognitionResult result)
        {
            switch (result.Text)
            {
                case "stop":
                    //StopVoiceRecognition();
                    break;
                case "plus":
                    FontSize += 2;
                    break;
                case "minus":
                    FontSize -= 2;
                    break;
                case "next":
                    GoToNext();
                    break;
                case "back":
                    GoBack();
                    break;
            }
        }

        private void GoToNext()
        {
            var scroll = Scroll.VerticalOffset;
            Scroll.ChangeView(Scroll.HorizontalOffset, scroll + 150, null, false);
        }

        private void GoBack()
        {
            var scroll = Scroll.VerticalOffset;
            if (scroll - 150 < 0) return;
            Scroll.ChangeView(Scroll.HorizontalOffset, scroll - 150, null, false);
        }
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                RaisePropertyChanged();
            }
        }

        private void StopVoiceRecognition()
        {
            try
            {
                _speechRecognizer.Dispose();
                _speechRecognizer = null;
                IsListening = false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public string SpeechText
        {
            get { return _speechText; }
            set
            {
                _speechText = value;
                RaisePropertyChanged();
            }
        }

        public ScrollViewer Scroll { get; set; }

        public string SpeechResult
        {
            get { return _speechResult; }
            set
            {
                _speechResult = value;
                RaisePropertyChanged();
            }
        }

        public async Task StartListeningAsync()
        {
            IsListening = true;
            _speechRecognizer = new SpeechRecognizer();
            _speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        "plus"
                        }, "plus"));
            _speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        "minus"
                        }, "minus"));
            _speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        "next"
                        }, "next"));
            _speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        "back"
                        }, "back"));
            SpeechRecognitionCompilationResult compilationResult = await _speechRecognizer.CompileConstraintsAsync();
            if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                await new MessageDialog("Compilation failed").ShowAsync();
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated +=
                ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.HypothesisGenerated += SpeechRecognizerHypothesisGenerated;
            await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }
    }
}