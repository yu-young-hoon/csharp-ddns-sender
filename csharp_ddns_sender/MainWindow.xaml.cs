using csharp_ddns_sender.Control;
using csharp_ddns_sender.UI;
using ddns_setting.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace csharp_ddns_sender
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow main;
        System.Windows.Forms.NotifyIcon ni;
       
        public MainWindow()
        {
            main = this;
            InitializeComponent();
            LoginPanel.DataContext = YamlProperty.GetInstance.userData;
            
            LogList.ItemsSource = ItemProperty.GetInstance.logItems;
            AllOrigin.ItemsSource = ItemProperty.GetInstance.AllOriginItems;
            ChoiceOrigin.ItemsSource = ItemProperty.GetInstance.ChoiceOriginItems;
            
            setup_NotifyIcon();
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            // 로그인 패널 잠금
            LoginPanel.IsEnabled = false;

            Sender httpSender = new Sender();
            BackgroundWorker _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += httpSender.DoworkLogin;
            _backgroundWorker.RunWorkerAsync();
        }

        public void LoginResult(string result)
        {
            if (result.Equals("success"))
            {
                // 오리진 패널 활성
                OriginPanel.IsEnabled = true;
                LoginText.Text = "인증 성공";
                
                Sender httpSender = new Sender();
                BackgroundWorker _backgroundWorker = new BackgroundWorker();
                _backgroundWorker.DoWork += httpSender.DoworkGetUser;
                _backgroundWorker.RunWorkerAsync();

                // 인증 정보 저장
                YamlProperty.GetInstance.SaveUser();
            }
            else
            {
                // 로그인 패널 활성
                LoginPanel.IsEnabled = true;
                LoginText.Text = "인증 실패";
            }
        }

        private void ChoiceButtonClick(object sender, RoutedEventArgs e)
        {
            RecordItem item = AllOrigin.SelectedItem as RecordItem;
            ItemProperty.GetInstance.ChoiceOrigin(item);
            AllOrigin.Items.Refresh();
            ChoiceOrigin.Items.Refresh();
        }

        private void UnChoiceButtonClick(object sender, RoutedEventArgs e)
        {
            RecordItem item = ChoiceOrigin.SelectedItem as RecordItem;
            ItemProperty.GetInstance.UnChoiceOrigin(item);
            AllOrigin.Items.Refresh();
            ChoiceOrigin.Items.Refresh();
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            // 오리진 선택 패널 잠금.
            OriginPanel.IsEnabled = false;

            foreach (RecordItem item in ChoiceOrigin.Items)
            {
                Sender httpSender = new Sender();

                BackgroundWorker _backgroundWorker = new BackgroundWorker();
                // BackgroundWorker의 이벤트 처리기
                _backgroundWorker.DoWork += httpSender._backgroundWorker_DoWork;
                _backgroundWorker.RunWorkerCompleted += httpSender._backgroundWorker_RunWorkerCompleted;
                _backgroundWorker.RunWorkerAsync(item);
            }

            // 오리진 정보 저장
            YamlProperty.GetInstance.SaveOrigin();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosed(e);
            WindowState = System.Windows.WindowState.Minimized;
            this.Hide();
            e.Cancel = true;
        }

        private void setup_NotifyIcon()
        {
            Thread notifyThread = new Thread(
            delegate ()
            {
                ni = new System.Windows.Forms.NotifyIcon();
                ni.Icon = new System.Drawing.Icon("icon.ico");
                ni.Text = "DNSZI Setting";
                ni.Visible = true;
                ni.DoubleClick += new EventHandler(TrayDoubleClick);

                System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
                item1.Index = 0;
                item1.Text = "E&xit";
                item1.Click += new EventHandler(TrayCloseClick);

                ni.ContextMenu = new System.Windows.Forms.ContextMenu();
                ni.ContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { item1 });

                System.Windows.Forms.Application.Run();
            });
            notifyThread.Start();
        }

        private void TrayDoubleClick(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }));
        }

        private void TrayCloseClick(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Environment.Exit(0);
            }));
        }
    }
}
