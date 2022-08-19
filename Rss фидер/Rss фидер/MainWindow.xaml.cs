using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Threading;
using System.Windows.Threading;
using System.Xaml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Rss_фидер
{
    public partial class MainWindow : Window
    {
        public Setting CurrentSettings = new Setting();
        public static Setting DefaultSettings = new Setting()
        {
            UserName = null,
            Password = null,
            AddressProxy = null,
            AddressRss = new List<string> { "https://habr.com/rss/interesting/" },
            TimeUpdata = 5
        };
        public string SelectRss = DefaultSettings.AddressRss[0];
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                if (File.Exists(@"SettingsFile.xml"))
                {
                    CurrentSettings = ReadFileXml();
                    if (CurrentSettings != null)
                    {
                        GenerationRssItem(getRssData(CurrentSettings.AddressRss[0]));
                        SelectRssCB.ItemsSource = CurrentSettings.AddressRss;
                        RssListCB.ItemsSource = CurrentSettings.AddressRss;
                        SelectRssCB.SelectedIndex = 0;
                        Timer(CurrentSettings.TimeUpdata);
                    }
                    else
                    {
                        CreateXmlFile(DefaultSettings);
                        CurrentSettings = DefaultSettings;
                        GenerationRssItem(getRssData(CurrentSettings.AddressRss[0]));
                        SelectRssCB.ItemsSource = CurrentSettings.AddressRss;
                        RssListCB.ItemsSource = CurrentSettings.AddressRss;
                        SelectRssCB.SelectedIndex = 0;
                        Timer(CurrentSettings.TimeUpdata);
                    }
                }
                else
                {
                    CreateXmlFile(DefaultSettings);
                    CurrentSettings = DefaultSettings;
                    GenerationRssItem(getRssData(CurrentSettings.AddressRss[0]));
                    SelectRssCB.ItemsSource = CurrentSettings.AddressRss;
                    RssListCB.ItemsSource = CurrentSettings.AddressRss;
                    SelectRssCB.SelectedIndex = 0;
                    Timer(CurrentSettings.TimeUpdata);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Произошла ошибка:" + ex);
            }
            
        }

        private string[,] getRssData(string channel)
        {
            try
            {
                if (CurrentSettings.AddressProxy != null && CurrentSettings.UserName != null && CurrentSettings.Password != null)
                {
                    var proxyObject = new WebProxy(CurrentSettings.AddressProxy);
                    proxyObject.Credentials = new NetworkCredential(CurrentSettings.UserName, CurrentSettings.Password);
                    GlobalProxySelection.Select = proxyObject;
                }
                WebRequest myReqest = WebRequest.Create(channel);
                WebResponse myResponse = myReqest.GetResponse();

                Stream rssStream = myResponse.GetResponseStream();
                XmlDocument rssDoc = new XmlDocument();

                rssDoc.Load(rssStream);

                XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/item");

                string[,] tempRssData = new string[rssItems.Count, 4];

                for (int i = 0; i < rssItems.Count; i++)
                {
                    XmlNode rssNode;

                    rssNode = rssItems.Item(i).SelectSingleNode("title");
                    if (rssNode != null)
                    {
                        tempRssData[i, 0] = rssNode.InnerText;
                    }
                    else
                    {
                        tempRssData[i, 0] = "";
                    }

                    rssNode = rssItems.Item(i).SelectSingleNode("description");
                    if (rssNode != null)
                    {
                        tempRssData[i, 1] = rssNode.InnerText;
                    }
                    else
                    {
                        tempRssData[i, 1] = "";
                    }

                    rssNode = rssItems.Item(i).SelectSingleNode("link");
                    if (rssNode != null)
                    {
                        tempRssData[i, 2] = rssNode.InnerText;
                    }
                    else
                    {
                        tempRssData[i, 2] = "";
                    }
                    rssNode = rssItems.Item(i).SelectSingleNode("pubDate");
                    if (rssNode != null)
                    {
                        tempRssData[i, 3] = rssNode.InnerText;
                    }
                    else
                    {
                        tempRssData[i, 3] = "";
                    }
                }
                return tempRssData;
            }
           catch(Exception ex)
            {
                MessageBox.Show("Возникла ошибка:" + ex);
                return null;
            }
        }

        private void AddRss_Click(object sender, RoutedEventArgs e)
        {
            CurrentSettings.AddressRss.Add(channelTextBox.Text);
            MessageBox.Show("Лента добавленна");
            RssListCB.ItemsSource = CurrentSettings.AddressRss;
            SelectRssCB.ItemsSource= CurrentSettings.AddressRss;
        }

        public void GenerationRssItem(string[,] rssD)
        {
            StackPanel stack = new StackPanel();

            for (int i = 0; i < rssD.GetLength(0); i++)
            {
                Border border = new Border()
                {
                    BorderThickness= new Thickness(1),
                    BorderBrush= Brushes.LightGray,
                    Background = Brushes.AliceBlue,
                    Margin= new Thickness(0, 5, 0, 5)
                };
                StackPanel sta = new StackPanel();

                Label title = new Label
                {
                    Content = rssD[i,0],
                    Cursor = Cursors.Hand,
                    FontSize = 18,           
                    Margin = new Thickness(0, 5, 0, 5),
                    ToolTip=rssD[i,2],
                };

                TextBlock description = new TextBlock()
                {
                    Text = rssD[i,1],
                    Margin = new Thickness(5, 2, 0, 2),
                    TextWrapping= TextWrapping.Wrap
                };
                title.MouseLeftButtonDown += title_MouseLeftButtonDown;

                Label date = new Label()
                {
                    Content= rssD[i,3],
                    Margin = new Thickness(5, 2, 0,5)
                };

                sta.Children.Add(title);
                sta.Children.Add(description);
                sta.Children.Add(date);
                border.Child=sta;
                stack.Children.Add(border);
            }  
            Scrol.Content = stack;
        }

        private void title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            if (label.ToolTip.ToString()!=null)
            {
                Process.Start("explorer.exe", $"\"{label.ToolTip.ToString()}\"");
            }         
        }

        public static void CreateXmlFile(Setting settings)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Setting));
            using (FileStream fs = new FileStream("SettingsFile.xml", FileMode.OpenOrCreate))
            {
                xml.Serialize(fs, settings);
            }
        }

        public static Setting ReadFileXml()
        {
            var xmlSerializer = new XmlSerializer(typeof(Setting));
            using (FileStream fs = new FileStream("SettingsFile.xml", FileMode.OpenOrCreate))
            {
                if (fs.Length != 0)
                {
                    Setting s = (Setting)xmlSerializer.Deserialize(fs);
                    return s;
                }
                else
                {
                    return null;
                }
            }           
        }

        DispatcherTimer timer;
        public void Timer(int interval)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(interval);
            timer.Tick += new EventHandler(EntyRestriction);
            timer.Start();
        }

        public void EntyRestriction(object obj, EventArgs e)//событие вызывающееся таймером
        {
            GenerationRssItem(getRssData(SelectRss));
        }

        private void UppdataRss(object sender, RoutedEventArgs e)// передать ссылку на ленту из файла
        {
            GenerationRssItem(getRssData(SelectRss));
        }

        private void LaunchSelectedRss(object sender, EventArgs e)
        {
            SelectRss= SelectRssCB.SelectedItem.ToString();
            GenerationRssItem(getRssData(SelectRss));
        }

        private void DeletSelectRss(object sender, RoutedEventArgs e)
        {
            CurrentSettings.AddressRss.Remove(RssListCB.SelectedItem.ToString());
            RssListCB.ItemsSource = null;
            RssListCB.ItemsSource = CurrentSettings.AddressRss;
            RssListCB.Text = null;

            SelectRssCB.ItemsSource= CurrentSettings.AddressRss;

            MessageBox.Show("Адрес удален");
        }

        private void SavingSettingsOnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (FileStream stream = File.Open(@"SettingsFile.xml", FileMode.Truncate)) { }//очистка файла настроек
            CreateXmlFile(CurrentSettings);

        }

        private void SaveProxySetting(object sender, RoutedEventArgs e)
        {
            CurrentSettings.UserName = UserNameBox.Text;
            CurrentSettings.Password = UserPasBox.Password;
            CurrentSettings.AddressProxy = ProxyBox.Text;
            GenerationRssItem(getRssData(SelectRss));
        }
    }
}
