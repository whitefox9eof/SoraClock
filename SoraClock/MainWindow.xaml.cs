using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoraClock
{
    public static class SCTools : Object
    {
        public static SolidColorBrush stringToSolidColorBrush(string colorCode)
        {
            Color color = (Color)ColorConverter.ConvertFromString(colorCode);
            return new SolidColorBrush(color);
        }
        /// <summary>
        /// voiceフォルダの音声を再生する
        /// </summary>
        /// <param name="file"></param>
        public static void playVoice(string file)
        {
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(@"resources\voice\" + file, UriKind.Relative));
            player.Volume = (double)MainSettings.Default.VoiceVolume / 100;
            Debug.WriteLine(player.Volume);
            player.Play();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainSettings settings;
        private DispatcherTimer timer;
        private int currentHour;

        public MainWindow()
        {
            InitializeComponent();
            // タイマーの初期設定
            timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            currentHour = DateTime.Now.Hour;
            timer.Start();
            // ウィンドウの初期設定
            settings = MainSettings.Default;
            Topmost = settings.Topmost;
            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + Width) < SystemParameters.VirtualScreenWidth)
            { Left = settings.WindowLeft; }
            else
            { Left = (SystemParameters.VirtualScreenWidth - Width) / 2; }
            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + Height) < SystemParameters.VirtualScreenHeight)
            { Top = settings.WindowTop; }
            else
            { Top = (SystemParameters.VirtualScreenHeight - Height) / 2; }
            // 色・不透明度
            windowBackgroundColor.Color = (Color)ColorConverter.ConvertFromString(settings.WindowBackgroundColor);
            windowBackgroundColor.Opacity = (double)settings.WindowOpacity / 100;
            // 枠線の初期化
            SolidColorBrush scb = SCTools.stringToSolidColorBrush(settings.ClockForegroundColor);
            outlineBorder.BorderBrush = (Brush)scb;
            inlineBorder.BorderBrush = outlineBorder.BorderBrush;
            // 時刻文字色の初期化
            clockTextBlock.Foreground = scb;
            DateTime time = DateTime.Now;
            clockTextBlock.Text = time.ToString(settings.TimeFormat);
            
        }
        /// <summary>
        /// 起動時ボイス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(1000);
            DateTime time = DateTime.Now;
            // 起動時のボイス
            if (time.Hour >= 4 && time.Hour <= 10)
            {
                SCTools.playVoice("luanch-0.wav");
            }
            else if (time.Hour >= 11 && time.Hour <= 17)
            {
                SCTools.playVoice("luanch-1.wav");
            }
            else
            {
                SCTools.playVoice("luanch-2.wav");
            }
        }

        /// <summary>
        /// 時刻更新イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime nowDateTime = DateTime.Now;
            clockTextBlock.Text = nowDateTime.ToString(settings.TimeFormat);
            if(nowDateTime.Hour != currentHour)
            {
                SCTools.playVoice("time-"+ nowDateTime.Hour + ".wav");
                currentHour = nowDateTime.Hour;
            }
        }

        /// <summary>
        /// コンテキストメニュー終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 終了前にウィンドウ位置を保存
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.Save();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 最前面表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topmostMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            settings.Topmost = true;
            settings.Save();
        }
        /// <summary>
        /// 最前面解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topmostMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            settings.Topmost = false;
            settings.Save();
        }

        /// <summary>
        /// ドラッグでウィンドウ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        /// <summary>
        /// 設定画面表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow window = new SettingWindow();
            window.Closed += Window_Closed;
            //MainSettings.Default.SettingChanging += Default_SettingChanging;
            MainSettings.Default.PropertyChanged += Default_PropertyChanged;
            window.Show();
           settingsMenuItem.IsEnabled = false;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Debug.WriteLine(e.PropertyName);
            switch (e.PropertyName)
            {
                // 全般
                case "TimeFormat":
                    clockTextBlock.Text = DateTime.Now.ToString(settings.TimeFormat);
                    break;
                // ウィンドウ
                case "WindowOpacity":
                    windowBackgroundColor.Opacity = (double)settings.WindowOpacity / 100;
                    break;
                case "WindowBackgroundColor":
                    windowBackgroundColor.Color = (Color)ColorConverter.ConvertFromString(settings.WindowBackgroundColor); 
                    break;
                case "WindowBorder":
                    if (settings.WindowBorder == true)
                    {
                        outlineBorder.BorderThickness = new Thickness(0);
                        inlineBorder.BorderThickness = new Thickness(0);
                    }
                    else
                    {
                        outlineBorder.BorderThickness = new Thickness(1);
                        inlineBorder.BorderThickness = new Thickness(3);
                    }
                    break;
                // フォント
                case "ClockForegroundColor":
                    clockTextBlock.Foreground = SCTools.stringToSolidColorBrush(settings.ClockForegroundColor);
                    break;
            }
        }

        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            Debug.WriteLine("ちぇんじんぐうううううううううううううう");
        }

        /// <summary>
        /// 設定画面を閉じたらコンテキストメニューの[設定]を有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            settingsMenuItem.IsEnabled = true;
        }

    }
}
