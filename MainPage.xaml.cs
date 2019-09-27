using FTTClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FTTClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly Regex ipRegex = new Regex(@"^([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})$");
        private readonly Regex domainRegex = new Regex(@"(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]");

        
        public MainPage()
        {
            InitializeComponent();

            if (Manager.Client != null)
            {
                Manager.Client.Clear();
                Manager.Client.Stop();
            }
            else
            {
                Manager.Client = new Client();
            }
            if(Manager.CPage != null)
            {
                Manager.CPage.ring = false;
            }
            Manager.Subscribed = false;
            Manager.CPage = new ClientPage();
            Manager.Port = 52323;
            if(Manager.IP != null)
            {
                Connect();
            }

        }

        public void Connect()
        {
            if (ipRegex.IsMatch(Manager.IP) || domainRegex.IsMatch(Manager.IP))
            {
                var a = Manager.Client.Initialize(Manager.IP, Manager.Port);
                if (a == 0)//연결 성공시
                {
                    if (Navigation.ModalStack.Count == 0)
                    {
                        Navigation.PushModalAsync(Manager.CPage);
                    }
                }
                else
                {
                    DisplayAlert("경고", "서버와의 연결이 원활하지 않습니다.", "알았어요.");
                }
           }
            else
            {
                DisplayAlert("경고", "올바르지 않은 IP입니다.", "알았어요.");
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Connect();
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            if (entry == Entry_IP)
            {
                Manager.IP = entry.Text;
            }
            else if (entry == Entry_Port)
            {
                try
                {
                    Manager.Port = Convert.ToInt32(entry.Text);
                }
                catch
                {
                    Manager.Port = 52323;
                }
            }

        }
    }
}
