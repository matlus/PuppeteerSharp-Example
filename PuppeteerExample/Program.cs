using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Threading.Tasks;

namespace PuppeteerExample
{
    class Program
    {
        private static NavigationOptions _navigationOptions = new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 } };
        private static object hyperlinkText;

        static async Task Main(string[] args)
        {
            /****************************************************************
              the default.htm file need to be moved to the same folder
              as the chrome.exe file. that will be downloaded to:
              PuppeteerExample\PuppeteerExample\bin\Debug\netcoreapp3.1\.local-chromium\Win64-706915\chrome-win
              The default.htm file is currently in the csproj folder
            *****************************************************************/
            ////const string url = "file:///default.htm";
            const string url = "https://matluspub.blob.core.windows.net/public/default.htm";
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                DefaultViewport = null
            });

            var delay = 100;
            var page = await browser.NewPageAsync();
            page.Request += Page_Request;
            page.Response += Page_Response;

            await page.GoToAsync(url);

            var firstNameSelector = "#firstName";
            await page.WaitForSelectorAsync(firstNameSelector);
            await TypeFieldValue(page, firstNameSelector, "MyFirstName", delay);

            var lastNameSelector = "#lastName";
            await TypeFieldValue(page, lastNameSelector, "MyLastName", delay);

            var usernameSelector = "#username";
            await TypeFieldValue(page, usernameSelector, "MyUsername", delay);

            var emailSelector = "#email";
            await TypeFieldValue(page, emailSelector, "myemail@somedomain.com", delay);

            var addressSelector = "#address";
            await TypeFieldValue(page, addressSelector, "6963 Gillis Way", delay);

            await SetDropdownValue(page, "country", "ISO 3166-2:US");

            await SetDropdownValue(page, "state", "VA");

            var zipSelector = "#zip";
            await TypeFieldValue(page, zipSelector, "22015", delay);

            /// Same as Billing checkbox
            await page.ClickAsync("#same-address");
            await page.Keyboard.PressAsync("Tab");

            /// Save Information checkbox
            await page.ClickAsync("#save-info");
            await page.Keyboard.PressAsync("Tab");

            /// PayPal radio button
            await page.ClickAsync("#paypal");
            await page.Keyboard.PressAsync("Tab");

            await ClickHyperlinkWithText(page, "Matlus");

            await MatlusWebsiteOperations(page);

            await page.ScreenshotAsync("Screenshot.jpg", new ScreenshotOptions { FullPage = true, Quality = 100 });
            await browser.CloseAsync();
        }

        private static async void Page_Response(object sender, ResponseCreatedEventArgs e)
        {
            Console.WriteLine(e.Response.Status);
        }

        private static void Page_Request(object sender, RequestEventArgs e)
        {
            Console.WriteLine(e.Request.ResourceType.ToString());
            Console.WriteLine(e.Request.Url);
        }

        private static async Task MatlusWebsiteOperations(Page page)
        {
            await ClickElementWithXPathAndWaitForXPath(page, "//a[text()='2']", "//a[text()='1']");
            ////await ClickLinkWithSelectorAndWaitForSelector(page, "a[data-pageno=\"2\"]", "a[data-pageno=\"1\"]");
            await ClickHyperlinkWithText(page, "A Generic RESTful CRUD HttpClient");
        }

        private static async Task TypeFieldValue(Page page, string fieldSelector, string value, int delay = 0)
        {
            await page.FocusAsync(fieldSelector);
            await page.TypeAsync(fieldSelector, value, new TypeOptions { Delay = delay });
            await page.Keyboard.PressAsync("Tab");
        }

        private static async Task SetDropdownValue(Page page, string dropdownId, string value)
        {
            var elementHandles = await page.XPathAsync($"//*[@id = \"{dropdownId}\"]/option[text() = \"{value}\"]");
            if (elementHandles.Length > 0)
            {
                var chosenOption = elementHandles[0];
                var jsHandle = await chosenOption.GetPropertyAsync("value");
                var choseOptionValue = await jsHandle.JsonValueAsync<string>();
                await page.FocusAsync($"#{dropdownId}");
                await page.SelectAsync($"#{dropdownId}", choseOptionValue);
            }
            else
            {
                await page.FocusAsync($"#{dropdownId}");
                await page.SelectAsync($"#{dropdownId}", value);
            }

            await page.Keyboard.PressAsync("Tab");
        }

        private static async Task ClickHyperlinkWithText(Page page, string hyperlinkText)
        {            
            var aElementsWithRestful = await page.XPathAsync($"//a[contains(text(), '{hyperlinkText}')]");
            if (aElementsWithRestful.Length == 1)
            {
                var navigationTask = page.WaitForNavigationAsync(_navigationOptions);
                var clickTask = aElementsWithRestful[0].ClickAsync();
                await Task.WhenAll(navigationTask, clickTask);
            }
            else
            {
                throw new Exception($"A hyperlink with the text: {hyperlinkText} was not found");
            }            
        }

        private static async Task ClickLinkWithSelectorAndWaitForSelector(Page page, string linkSelector, string waitForSelector)
        {
            await page.ClickAsync(linkSelector);
            await page.WaitForSelectorAsync($"{ waitForSelector}");
        }

        private static async Task ClickElementWithXPathAndWaitForXPath(Page page, string clickOnXpathExpression, string waitForXpathExpression)
        {
            var aElementsWithRestful = await page.XPathAsync(clickOnXpathExpression);
            if (aElementsWithRestful.Length == 1)
            {
                var navigationTask = page.WaitForXPathAsync(waitForXpathExpression);
                var clickTask = aElementsWithRestful[0].ClickAsync();
                await Task.WhenAll(navigationTask, clickTask);
            }
            else
            {
                throw new Exception($"A hyperlink with expression: {clickOnXpathExpression} was not found");
            }
        }
    }
}
