using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WouldISurviveTheTitanic
{

    public class ScoreData
    {
        public Dictionary<string, string> FeatureVector { get; set; }
        public Dictionary<string, string> GlobalParameters { get; set; }
    }

    public class ScoreRequest
    {
        public string Id { get; set; }
        public ScoreData Instance { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
       


        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            string pClass =TicketClass.SelectedValue.ToString();
            string sex =Gender.SelectedValue.ToString().ToLower();
            string age = Age.Text;
            string ss = (Convert.ToInt32(Siblings.Text) + (Convert.ToInt32(Spouse.IsChecked))).ToString();
            string pc = (Convert.ToInt32(Parents.Text) + (Convert.ToInt32(Children.Text))).ToString();
            string tFare = TicketFare.Text;
            string tPort =TicketPort.SelectedValue.ToString();
            string result = await InvokeRequestResponseService(pClass,sex,age,ss,pc, tFare,tPort);
            //display message   
            Windows.UI.Popups.MessageDialog message = new Windows.UI.Popups.MessageDialog(result);
            await message.ShowAsync();

        }



       static async Task<string> InvokeRequestResponseService(string passenger_class, string sex, string age, string sibling_spouse, string parent_child, string fare, string embarked)
        {
            using (var client = new HttpClient())
            {
                ScoreData scoreData = new ScoreData( )
                {
                    FeatureVector = new Dictionary<string, string>() 
                    {
                        { "Passenger_Class", passenger_class },
                        { "Sex", sex },
                        { "Age", age },
                        { "Sibling_Spouse",sibling_spouse },
                        { "Parent_Child", parent_child },
                        { "Fare", fare },
                        { "Embarked", embarked },
                    },
                    GlobalParameters = new Dictionary<string, string>() 
                    {
                    }
                };

                ScoreRequest scoreRequest = new ScoreRequest()
                {
                    Id = "score00001",
                    Instance = scoreData
                };

                const string apiKey = ""; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/54378d453f5e4741875b2cf3356155b3/services/c14025c2c095430ea2792e52a49f109f/score");
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    result = result.Replace("[", "").Replace("]", "").Replace('"',' ').Trim();
                    var resultList = result.Split(',');
                    Double prob =Convert.ToDouble (resultList[resultList.Length - 1].Trim());
                    if (prob > .90) result = "You should be golden!";
                    else if (prob > .6) result = "I think you might make it but grab a life preserver just in case!";
                    else if (prob < .6) result = "If you see an iceberg jump it might be your only chance!";
                    return result;
                }
                else
                {
                    return "Query Failed: Check your inputs and try again!";    
                }
            }
        }
    }
}

