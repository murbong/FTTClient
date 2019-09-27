using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTTClient.Core;

using Plugin.LocalNotifications;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FTTClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientPage : ContentPage
    {
        public bool ring;

        public ClientPage()
        {
            InitializeComponent();
            if (Manager.Subscribed == false)
            {
                Manager.Client.ReceivedStr += new Client.ReceivedString(Received);
                Manager.Subscribed = true;
            }
                
            ring = false;
        }

        public void CleanEditor()
        {
            ring = false;
            CrossLocalNotifications.Current.Show("경고", "통발이 터진것 같아요.");
        }

        private void Received(string str)
        {
            bool factor = (str.Contains("문제") && ring == false);
            Device.BeginInvokeOnMainThread(() =>
            {
                Manager.@string.AppendLine(DateTime.Now.ToString(@"yyyy\-MM\-dd hh\:mm\:ss \: ") + str);
                Editor_Command.Text = Manager.@string.ToString();
                Editor_Command.Focus();

                if (factor)
                {



                    DisplayAlert("경고", "통발이 터진거같아요...", "네").ContinueWith((arg) => { ring = false;
                        CrossLocalNotifications.Current.Cancel(0);
                    });
                    CrossLocalNotifications.Current.Show("경고", "통발이 터진것 같아요.",0);
                    var orientation = DependencyService.Get<IDevicePowerOn>().Wake();

                }
            });
            if (factor)
            {
                Debug.WriteLine("이이잉");
                ring = true;
                Task.Run(() => Ring());
            }
        }
        private async Task Ring()
        {
            while (ring)
            {
                try
                {
                    Vibration.Vibrate();
                    await Task.Delay(1000);
                    Vibration.Vibrate();
                    await Task.Delay(2000);
                }
                catch
                {
                    Debug.WriteLine("위이잉");
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (await DisplayAlert("경고", "서버와의 연결을 종료하겠습니까?", "네", "아니요"))
                {
                    Manager.Client.Stop();
                    base.OnBackButtonPressed();

                }
            });

            return true;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            OnBackButtonPressed();
        }
    }
}