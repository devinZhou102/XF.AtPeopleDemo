using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AtPeopleDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ChatEntry_AtEvent(object sender, AtEventArgs e)
        {
            ChatEntry.InsertAtPerson(new AtPerson
            {
                Name = "Name" + (Number++),
                Uid = Guid.NewGuid(),
            }, e.Offset);
        }

        static int Number = 0;

        private void Button_Clicked(object sender, EventArgs e)
        {
            var content = new TextMsgContent
            {
                Content = ChatEntry.Text,
                AtPeople = ChatEntry.AtPeople?.ToList(),
            };
            LabelContent.Text = JsonConvert.SerializeObject(content);
        }


        private void Button2_Clicked(object sender, EventArgs e)
        {
            ChatEntry.Text = "";
            LabelContent.Text = "";
            Number = 0;
        }
    }

    public class TextMsgContent
    {
        public string Content { get; set; }

        public List<AtPerson> AtPeople { get; set; }

    }
}
