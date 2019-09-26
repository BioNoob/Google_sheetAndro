using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.ComponentModel;
using System.Reflection;
using TableAndro;
using Xamarin.Forms;

namespace Google_sheetAndro.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            var assembly = Assembly.GetExecutingAssembly();
            GoogleCredential credential;
            using (var stream = assembly.GetManifestResourceStream("Google_sheetAndro.sicret_new.json"))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Googles.Scopes);
            }

            // Create Google Sheets API service.
            Googles.service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Googles.ApplicationName,
            });
            Googles.sheetInfo = Googles.service.Spreadsheets.Get(Googles.SpreadsheetId).Execute();
        }
    }
}