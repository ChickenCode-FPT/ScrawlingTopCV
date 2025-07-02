using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using MongoDB.Driver;
using PuppeteerSharp; 
using Services.CrawlingService; 
using Services.Models;
using Services.MongoDBService;       

namespace Microservices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GemloginApiClient _gemloginApiClient;
        private readonly HttpClient _httpClient;
        //private Browser? _browser;
        private List<IBrowser> _openedBrowsers = new List<IBrowser>();
        private string UrlToStartCrawl1 = "https://vieclam24h.vn/viec-lam-ke-toan-o17.html?occupation_ids%5B%5D=17&page=1&sort_q=priority_max%2Cdesc";
       private string UrlToStartCrawl2 = "https://vieclam24h.vn/viec-lam-ke-toan-o17.html?occupation_ids%5B%5D=17&page=2&sort_q=priority_max%2Cdesc";
       private string UrlToStartCrawl3 = "https://vieclam24h.vn/viec-lam-ke-toan-o17.html?occupation_ids%5B%5D=17&page=3&sort_q=priority_max%2Cdesc";
       private string UrlToStartCrawl4 = "https://vieclam24h.vn/viec-lam-ke-toan-o17.html?occupation_ids%5B%5D=17&page=4&sort_q=priority_max%2Cdesc";
        private string UrlToStartCrawl5 = "https://vieclam24h.vn/viec-lam-ke-toan-o17.html?occupation_ids%5B%5D=17&page=5&sort_q=priority_max%2Cdesc";
        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _gemloginApiClient = new GemloginApiClient(_httpClient, "https://localhost:7039");
            //CBlistScrap.Items.Add("https://www.topcv.vn/viec-lam-it");
            //CBlistScrap.Items.Add(UrlToStartCrawl);
            CBlistScrap.Items.Add(UrlToStartCrawl1);
            CBlistScrap.Items.Add(UrlToStartCrawl2);
            CBlistScrap.Items.Add(UrlToStartCrawl3);
            CBlistScrap.Items.Add(UrlToStartCrawl4);
            CBlistScrap.Items.Add(UrlToStartCrawl5);
        }

        private async void StartScrapingButton_Click(object sender, RoutedEventArgs e)
        {
            StartScrapingButton.IsEnabled = false;
            LogMessage("Starting the scraping process...");
            StatusTextBlock.Text = "Running...";

            foreach (var browser in _openedBrowsers)
            {
                if (browser != null && browser.IsConnected)
                {
                    try
                    {
                        await browser.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error disposing previous browser: {ex.Message}");
                    }
                }
            }
            _openedBrowsers.Clear();



                //if (_browser != null && _browser.IsConnected)
                //{
                //    await _browser.DisposeAsync();
                //    _browser = null;
                //    LogMessage("Disconnected from previous browser instance.");
                //}
                try
                {
                    // Lấy ID profile cơ sở từ hộp văn bản
                    if (!int.TryParse(ProfileIdTextBox.Text, out int baseProfileId))
                    {
                        LogMessage("Error: Invalid Base Profile ID. Please enter a valid number.");
                        StatusTextBlock.Text = "Completed with errors.";
                        return;
                    }

                    var urlsToScrape = new List<string>
                    {
                          UrlToStartCrawl1,
                          UrlToStartCrawl2,
                          UrlToStartCrawl3,
                          UrlToStartCrawl4,
                          UrlToStartCrawl5
                     };

                int maxConcurrentProfiles = 5; 
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int visibleConcurrentDisplay = 2; 
                int desiredWindowWidth = 800;
                int desiredWindowHeight = 600;
                var scrapingTasks = new List<Task>();

                for (int i = 0; i < urlsToScrape.Count; i++)
                {
                    int currentProfileId = baseProfileId + i;
                    string currentUrl = urlsToScrape[i];

                    int windowXPosition = (i % visibleConcurrentDisplay) * desiredWindowWidth;
                    int windowYPosition = 0; 

        
                    scrapingTasks.Add(ScrapeWithDedicatedProfileAsync(
                        currentProfileId,
                        currentUrl,
                        desiredWindowWidth,
                        desiredWindowHeight,
                        windowXPosition,
                        windowYPosition
                    ));
                }

                await Task.WhenAll(scrapingTasks);

                    LogMessage("All specified URLs have been scraped, each with a dedicated Gemlogin profile.");
                    StatusTextBlock.Text = "Successfully completed.";
                }
                catch (Exception ex)
                {
                    LogMessage($"An error occurred during scraping: {ex.Message}");
                    StatusTextBlock.Text = "Completed with errors.";
                    LogMessage($"Stack Trace: {ex.StackTrace}");
                }
                finally
                {
                    StartScrapingButton.IsEnabled = true;
                    // Đóng tất cả các trình duyệt đã mở
                    foreach (var browser in _openedBrowsers)
                    {
                        if (browser != null && browser.IsConnected)
                        {
                            try
                            {
                                await browser.DisposeAsync();
                                LogMessage($"Closed browser instance for a profile.");
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Error closing browser: {ex.Message}");
                            }
                        }
                    }
                    _openedBrowsers.Clear(); 
                }
            
        }


                //LogMessage($"Requesting Gemlogin API to start profile {profileId}...");
                //var startProfileResponse = await _gemloginApiClient.StartProfileAsync(profileId, new StartProfileOptions());

                //if (startProfileResponse == null || !startProfileResponse.Success || string.IsNullOrEmpty(startProfileResponse.Data?.RemoteDebuggingAddress))
                //{
                //    LogMessage($"Error: Failed to launch Gemlogin profile {profileId}. Message: {startProfileResponse?.Message ?? "Unknown error."}");
                //    StatusTextBlock.Text = "Completed with errors.";
                //    return;
                //}

                //string remoteDebuggingAddress = startProfileResponse.Data.RemoteDebuggingAddress;
                //// PuppeteerSharp typically expects a URL format like http://host:port
                //string browserUrlForPuppeteer = $"http://{remoteDebuggingAddress}";

                //LogMessage($"Received Remote Debugging Address from Gemlogin: {remoteDebuggingAddress}");
                //LogMessage($"PuppeteerSharp will connect to: {browserUrlForPuppeteer}");

                //// --- 2. Connect PuppeteerSharp to the running browser instance directly ---
                //LogMessage("Connecting PuppeteerSharp to the Gemlogin browser...");
                //var connectOptions = new ConnectOptions
                //{
                //    BrowserURL = browserUrlForPuppeteer,
                //    DefaultViewport = null // Allow Gemlogin profile's viewport settings to be used
                //};

                //_browser = (Browser?)await Puppeteer.ConnectAsync(connectOptions);
                //LogMessage("PuppeteerSharp connected successfully to the Gemlogin browser.");
                //List<string> urlsToScrapeConcurrently = new List<string>();
                //foreach (var item in CBlistScrap.Items)
                //{
                //    if (item is string url)
                //    {
                //        urlsToScrapeConcurrently.Add(url);
                //    }
                //}
                //if (!urlsToScrapeConcurrently.Any())
                //{
                //    LogMessage("No URLs selected for scraping. Please ensure URLs are added or selected.");
                //    StatusTextBlock.Text = "Completed with no URLs to scrape.";
                //    return;
                //}
                //var scrapingTasks = urlsToScrapeConcurrently.Select(url => ScrapeSingleUrlAsync(url)).ToList();
                //await Task.WhenAll(scrapingTasks);

                //LogMessage("All selected URLs have been scraped concurrently.");
                //StatusTextBlock.Text = "Successfully completed.";




                // --- 3. Start scraping based on the selected URL from the ComboBox ---
                //if (CBlistScrap.SelectedItem == "https://www.topcv.vn/viec-lam-it" && CBlistScrap.SelectedItem != null) {


                //} else if (CBlistScrap.SelectedItem == UrlToStartCrawl1 || CBlistScrap.SelectedItem == UrlToStartCrawl2 && CBlistScrap.SelectedItem != null)
                //{
                //    if(CBlistScrap.SelectedItem == UrlToStartCrawl1)
                //    {
                //        UrlToStartCrawl = UrlToStartCrawl1;
                //    }
                //    else if (CBlistScrap.SelectedItem == UrlToStartCrawl2)
                //    {
                //        UrlToStartCrawl = UrlToStartCrawl2;
                //    }
                //    using (var page = await _browser.NewPageAsync())
                //    {
                //        LogMessage("Navigating to TopCV job listings page...");
                //        await page.GoToAsync(UrlToStartCrawl, new NavigationOptions { Timeout = 60000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                //        await Task.Delay(5000);

                //        LogMessage($"Current URL: {page.Url}");

                //        List<string> allJobDetailLinks = new List<string>();

                //        string jobItemContainerSelector = ".grid.grid-cols-1.gap-y-2.lg\\:gap-y-2\\.5";
                //        string fullLinkSelector = jobItemContainerSelector + " a";
                //        LogMessage($"Waiting for job listing container '{jobItemContainerSelector}' to appear...");
                //        try
                //        {

                //            await page.WaitForSelectorAsync(jobItemContainerSelector, new WaitForSelectorOptions { Timeout = 15000 });
                //            LogMessage("Job listing container found. Starting link collection.");
                //            var linkElementHandles = await page.QuerySelectorAllAsync(fullLinkSelector);
                //            if (jobItemContainerSelector.Any())
                //            {
                //                foreach (var linkHandle in linkElementHandles)
                //                {
                //                    var hrefProperty = await linkHandle.GetPropertyAsync("href");
                //                    string? hrefValue = await hrefProperty.JsonValueAsync<string>();
                //                    if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
                //                    {
                //                        allJobDetailLinks.Add(hrefValue);
                //                    }
                //                }
                //            }
                //            LogMessage($"Total job detail links collected: {allJobDetailLinks.Count}");
                //            await page.ScreenshotAsync("screenshot_job_listing_page.png");
                //            LogMessage("Screenshot of listing page saved.");
                //        }
                //        catch (Exception ex)
                //        {
                //            LogMessage($"Error collecting links from the listing page: {ex.Message}");
                //            LogMessage($"Current URL during error: {page.Url}");
                //        }

                //        LogMessage("\n--- Navigating to individual job detail links and printing HTML ---");
                //        int visitedLinkCounter = 0;
                //        foreach (string detailLink in allJobDetailLinks.Distinct().Take(20))
                //        {
                //            visitedLinkCounter++;
                //            LogMessage($"\nAccessing Link #{visitedLinkCounter}: {detailLink}");

                //using (var detailPage = await _browser.NewPageAsync())
                //{
                //    try
                //    {
                //        await detailPage.GoToAsync(detailLink, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded }, Timeout = 60000 });
                //        LogMessage($"  Navigated to: {detailPage.Url}");

                //        int delayMilliseconds = new Random().Next(3000, 7001);
                //        LogMessage($"  Waiting for {delayMilliseconds / 1000.0} seconds...");
                //        await Task.Delay(delayMilliseconds)

                //        var extractedSections = new Dictionary<string, string>();
                //        string mongoConnectionString = "mongodb+srv://lumconon0911:SWD-SWD@cluster.slolqwf.mongodb.net/";
                //        string mongoDatabaseName = "JobScraperTopCV";
                //        string mongoCollectionName = "JobPostingTopCV";
                //        var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName, mongoCollectionName);

                //        var jobValue = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.mb-5.relative > div > div > div > div.flex.sm_cv\\:flex-col.mt-4.sm_cv\\:mt-\\[12px\\].mb-8.sm_cv\\:mb-\\[12px\\].gap-10.sm_cv\\:gap-\\[12px\\].md\\:flex-wrap > div.flex.gap-10 > div:nth-child(1) > h2 > p.font-semibold.text-14.text-\\[\\#8B5CF6\\]");
                //        if (jobValue != null)
                //        {
                //            string jobValueText = await jobValue.EvaluateFunctionAsync<string>("el => el.innerText");
                //            extractedSections["Tien Luong"] = jobValueText.Trim();
                //            LogMessage($"  Job Value: {jobValueText}");
                //        }
                //        else
                //        {
                //            LogMessage("  Job Value section not found.");
                //        }
                //        var jobTitle = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.mb-5.relative > div > div > div > h1");
                //        if (jobTitle != null)
                //        {
                //            string jobTitleText = await jobTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                //            extractedSections["Tittle"] = jobTitleText.Trim();
                //            LogMessage($"  Job Value: {jobTitleText}");
                //        }
                //        else
                //        {
                //            LogMessage("  Job Title section not found.");
                //        }

                //        var jobDescriptionElement = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3)");


                //          var sectionTitle = await jobDescriptionElement.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3) > h2");
                //                var sectionContent = await jobDescriptionElement.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3) > div");
                //                if (sectionTitle != null && sectionContent != null)
                //                {
                //                    string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                //                    string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
                //                    extractedSections[titleText.Trim()] = sectionContentText.Trim();
                //                    LogMessage($"  Title Section: {titleText}");
                //                    LogMessage($"  Content Section: {sectionContentText}");
                //                }



                //        var sectionRequiement = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div.jsx-5b2773f86d2f74b.mb-4.md\\:mb-8 > div");
                //            if (sectionRequiement != null)
                //            {
                //                string sectionRequiementText = await sectionRequiement.EvaluateFunctionAsync<string>("el => el.innerText");
                //                extractedSections["Yêu cầu công việc"] = sectionRequiementText.Trim();
                //                LogMessage($"  Yêu cầu công việc: {sectionRequiementText}");
                //            }


                //        var sectionBenefitContent = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(5) > div");
                //        if (sectionBenefitContent != null)
                //        {
                //            string sectionBenefitContentText = await sectionBenefitContent.EvaluateFunctionAsync<string>("el => el.innerText");
                //            extractedSections["Quyền lợi"] = sectionBenefitContentText.Trim();
                //            LogMessage($"  Content Section: {sectionBenefitContentText}");
                //        }


                //        var sectionAddressContent = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(6) > div");

                //        if ( sectionAddressContent != null)
                //        {
                //            string sectionAddressContentText = await sectionAddressContent.EvaluateFunctionAsync<string>("el => el.innerText");
                //            extractedSections["Địa điểm làm việc"] = sectionAddressContentText.Trim();
                //            LogMessage($"  Content Section: {sectionAddressContentText}");
                //        }



                //        if (sectionTitle != null && sectionContent != null)
                //        {
                //            string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                //            string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
                //            extractedSections[titleText.Trim()] = sectionContentText.Trim();
                //            LogMessage($"  Title Section: {titleText}");
                //            LogMessage($"  Content Section: {sectionContentText}");
                //        }



                //        LogMessage($"\n--- Chi tiết công việc đã trích xuất từ {detailLink} ---");
                //            foreach (var entry in extractedSections)
                //            {
                //                LogMessage($"\nPhần: {entry.Key}");
                //                LogMessage($"Nội dung: {entry.Value}");
                //            }
                //            LogMessage($"--- Kết thúc chi tiết công việc từ {detailLink} ---");

                //            //Save to MongoDB
                //            if (extractedSections.Any())
                //            {
                //                await mongoService.SaveJobDetailsAsync(detailLink, extractedSections);
                //            }
                //            else
                //            {
                //                LogMessage($"  Không có dữ liệu trích xuất để lưu cho URL: {detailLink}");
                //            }

                //    }
                //    catch (Exception detailEx)
                //    {
                //        LogMessage($"  Error accessing detail link {detailLink}: {detailEx.Message}");
                //        LogMessage($"  Current URL after error: {detailPage.Url}");
                //    }
                //}
                //            }
                //        }
                //    }
                //    else
                //    {
                //        LogMessage("No valid URL selected for scraping.");
                //        StatusTextBlock.Text = "Completed with errors.";
                //        return;
                //    }


                //    LogMessage("Scraping process completed successfully!");
                //    StatusTextBlock.Text = "Completed.";

                //}
                //catch (Exception ex)
                //{
                //    LogMessage($"An error occurred during scraping: {ex.Message}");
                //    LogMessage($"Stack Trace: {ex.StackTrace}");
                //    StatusTextBlock.Text = "Completed with errors.";
                //}
                //finally
                //{

                //    if (_browser != null && _browser.IsConnected)
                //    {
                //        LogMessage("Disconnecting PuppeteerSharp.");
                //        await _browser.DisposeAsync(); 
                //        _browser = null;
                //    }

                //    StartScrapingButton.IsEnabled = true; 
                //}
             
            
        

        private async Task ScrapeWithDedicatedProfileAsync(int profileId,
    string urlToScrape,
    int windowWidth,    
    int windowHeight,   
    int windowX,       
    int windowY)        
        {
            IBrowser? browser = null; 
            try
            {
                
                LogMessage($"[Profile {profileId}] Requesting Gemlogin API to start profile {profileId} for URL: {urlToScrape}...");
                LogMessage($"[DEBUG] Profile {profileId} will start with Pos: {windowX},{windowY} Size: {windowWidth},{windowHeight}");
                var startProfileOptions = new StartProfileOptions
                {
                    AdditionalArgs = "--disable-features=TranslateUI,PasswordGeneration --disable-gpu",

                    WindowPosition = $"{windowX},{windowY}",

                    WindowSize = $"{windowWidth},{windowHeight}",

                    WindowScale = null 
                };
                var startProfileResponse = await _gemloginApiClient.StartProfileAsync(profileId, startProfileOptions);

                if (startProfileResponse == null || !startProfileResponse.Success || string.IsNullOrEmpty(startProfileResponse.Data?.RemoteDebuggingAddress))
                {
                    LogMessage($"[Profile {profileId}] Error: Failed to launch Gemlogin profile {profileId}. Message: {startProfileResponse?.Message ?? "Unknown error."}");
                    return;
                }

                string remoteDebuggingAddress = startProfileResponse.Data.RemoteDebuggingAddress;
                string browserUrlForPuppeteer = $"http://{remoteDebuggingAddress}";

                LogMessage($"[Profile {profileId}] Received Remote Debugging Address: {remoteDebuggingAddress}");
                LogMessage($"[Profile {profileId}] PuppeteerSharp will connect to: {browserUrlForPuppeteer}");

                LogMessage($"[Profile {profileId}] Connecting PuppeteerSharp to the Gemlogin browser...");
                var connectOptions = new ConnectOptions
                {
                    BrowserURL = browserUrlForPuppeteer,
                    DefaultViewport = null
                };

                browser = (Browser?)await Puppeteer.ConnectAsync(connectOptions);
                _openedBrowsers.Add(browser); 
                LogMessage($"[Profile {profileId}] PuppeteerSharp connected successfully.");

                await ScrapeSingleUrlAsync(browser, urlToScrape);

                LogMessage($"[Profile {profileId}] Scraping completed for URL: {urlToScrape}");
            }
            catch (Exception ex)
            {
                LogMessage($"[Profile {profileId}] An error occurred during scraping for {urlToScrape}: {ex.Message}");
                LogMessage($"[Profile {profileId}] Stack Trace: {ex.StackTrace}");
            }


        }


        private async Task ScrapeSingleUrlAsync(IBrowser browserInstance, string urlToScrape)
        {

            //Go to Page ang crawl link details
            using (var page = await browserInstance.NewPageAsync())
            {
                LogMessage("Navigating to TopCV job listings page...");
                await page.GoToAsync(urlToScrape, new NavigationOptions { Timeout = 60000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                await Task.Delay(5000);

                LogMessage($"Current URL: {page.Url}");

                List<string> allJobDetailLinks = new List<string>();

                string jobItemContainerSelector = ".grid.grid-cols-1.gap-y-2.lg\\:gap-y-2\\.5";
                string fullLinkSelector = jobItemContainerSelector + " a";
                LogMessage($"Waiting for job listing container '{jobItemContainerSelector}' to appear...");
                try
                {

                    await page.WaitForSelectorAsync(jobItemContainerSelector, new WaitForSelectorOptions { Timeout = 15000 });
                    LogMessage("Job listing container found. Starting link collection.");
                    var linkElementHandles = await page.QuerySelectorAllAsync(fullLinkSelector);
                    if (jobItemContainerSelector.Any())
                    {
                        foreach (var linkHandle in linkElementHandles)
                        {
                            var hrefProperty = await linkHandle.GetPropertyAsync("href");
                            string? hrefValue = await hrefProperty.JsonValueAsync<string>();
                            if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
                            {
                                allJobDetailLinks.Add(hrefValue);
                            }
                        }
                    }
                    LogMessage($"Total job detail links collected: {allJobDetailLinks.Count}");
                    await page.ScreenshotAsync("screenshot_job_listing_page.png");
                    LogMessage("Screenshot of listing page saved.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error collecting links from the listing page: {ex.Message}");
                    LogMessage($"Current URL during error: {page.Url}");
                }

                LogMessage("\n--- Navigating to individual job detail links and printing HTML ---");
                int visitedLinkCounter = 0;
                foreach (string detailLink in allJobDetailLinks.Distinct().Take(20))
                {
                    visitedLinkCounter++;
                    LogMessage($"\nAccessing Link #{visitedLinkCounter}: {detailLink}");
                    using (var detailPage = await browserInstance.NewPageAsync())
                    {
                        try
                        {
                            await detailPage.GoToAsync(detailLink, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded }, Timeout = 60000 });
                            LogMessage($"  Navigated to: {detailPage.Url}");

                            await Task.Delay(4000);
                            LogMessage($"  Waiting for {4000 / 1000.0} seconds...");

                            var extractedSections = new Dictionary<string, string>();
                            string mongoConnectionString = "mongodb+srv://lumconon0911:SWD-SWD@cluster.slolqwf.mongodb.net/";
                            string mongoDatabaseName = "JobScraperTopCV";
                            string mongoCollectionName = "JobPostingTopCV";
                            var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName, mongoCollectionName);

                            var jobValue = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.mb-5.relative > div > div > div > div.flex.sm_cv\\:flex-col.mt-4.sm_cv\\:mt-\\[12px\\].mb-8.sm_cv\\:mb-\\[12px\\].gap-10.sm_cv\\:gap-\\[12px\\].md\\:flex-wrap > div.flex.gap-10 > div:nth-child(1) > h2 > p.font-semibold.text-14.text-\\[\\#8B5CF6\\]");
                            if (jobValue != null)
                            {
                                string jobValueText = await jobValue.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections["Tien Luong"] = jobValueText.Trim();
                                LogMessage($"  Job Value: {jobValueText}");
                            }
                            else
                            {
                                LogMessage("  Job Value section not found.");
                            }
                            var jobTitle = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.mb-5.relative > div > div > div > h1");
                            if (jobTitle != null)
                            {
                                string jobTitleText = await jobTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections["Tittle"] = jobTitleText.Trim();
                                LogMessage($"  Job Value: {jobTitleText}");
                            }
                            else
                            {
                                LogMessage("  Job Title section not found.");
                            }

                            var jobDescriptionElement = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3)");


                            var sectionTitle = await jobDescriptionElement.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3) > h2");
                            var sectionContent = await jobDescriptionElement.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(3) > div");
                            if (sectionTitle != null && sectionContent != null)
                            {
                                string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                                string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections[titleText.Trim()] = sectionContentText.Trim();
                                LogMessage($"  Title Section: {titleText}");
                                LogMessage($"  Content Section: {sectionContentText}");
                            }



                            var sectionRequiement = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div.jsx-5b2773f86d2f74b.mb-4.md\\:mb-8 > div");
                            if (sectionRequiement != null)
                            {
                                string sectionRequiementText = await sectionRequiement.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections["Yêu cầu công việc"] = sectionRequiementText.Trim();
                                LogMessage($"  Yêu cầu công việc: {sectionRequiementText}");
                            }


                            var sectionBenefitContent = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(5) > div");
                            if (sectionBenefitContent != null)
                            {
                                string sectionBenefitContentText = await sectionBenefitContent.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections["Quyền lợi"] = sectionBenefitContentText.Trim();
                                LogMessage($"  Content Section: {sectionBenefitContentText}");
                            }


                            var sectionAddressContent = await detailPage.QuerySelectorAsync("#__next > div:nth-child(1) > main > div > div.flex.flex-col.lg\\:flex-row > div.w-full.lg\\:w-3\\/4.pb-4 > div.jsx-5b2773f86d2f74b.px-4.md\\:px-10.py-4.bg-white.shadow-sd-12.rounded-sm > div:nth-child(6) > div");

                            if (sectionAddressContent != null)
                            {
                                string sectionAddressContentText = await sectionAddressContent.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections["Địa điểm làm việc"] = sectionAddressContentText.Trim();
                                LogMessage($"  Content Section: {sectionAddressContentText}");
                            }



                            if (sectionTitle != null && sectionContent != null)
                            {
                                string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
                                string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
                                extractedSections[titleText.Trim()] = sectionContentText.Trim();
                                LogMessage($"  Title Section: {titleText}");
                                LogMessage($"  Content Section: {sectionContentText}");
                            }



                            LogMessage($"\n--- Chi tiết công việc đã trích xuất từ {detailLink} ---");
                            foreach (var entry in extractedSections)
                            {
                                LogMessage($"\nPhần: {entry.Key}");
                                LogMessage($"Nội dung: {entry.Value}");
                            }
                            LogMessage($"--- Kết thúc chi tiết công việc từ {detailLink} ---");

                            //Save to MongoDB
                            if (extractedSections.Any())
                            {
                                await mongoService.SaveJobDetailsAsync(detailLink, extractedSections);
                            }
                            else
                            {
                                LogMessage($"  Không có dữ liệu trích xuất để lưu cho URL: {detailLink}");
                            }

                        }
                        catch (Exception detailEx)
                        {
                            LogMessage($"  Error accessing detail link {detailLink}: {detailEx.Message}");
                            LogMessage($"  Current URL after error: {detailPage.Url}");
                        }

                    }

                }
            }

            //using (var page = await browserInstance.NewPageAsync())
            //{
            //    LogMessage("Navigating to TopCV job listings page...");
            //    await page.GoToAsync(urlToScrape, new NavigationOptions { Timeout = 60000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
            //    await Task.Delay(5000);

            //    LogMessage($"Current URL: {page.Url}");

            //    List<string> allJobDetailLinks = new List<string>();

            //    try
            //    {
            //        string jobItemContainerSelector = ".avatar";
            //        LogMessage($"Waiting for job listing container '{jobItemContainerSelector}' to appear...");
            //        await page.WaitForSelectorAsync(jobItemContainerSelector, new WaitForSelectorOptions { Timeout = 15000 });
            //        LogMessage("Job listing container found. Starting link collection.");

            //        var allJobItemElements = await page.QuerySelectorAllAsync(jobItemContainerSelector);
            //        LogMessage($"Found {allJobItemElements.Length} job blocks on the listing page.");

            //        foreach (var jobElementHandle in allJobItemElements)
            //        {
            //            string specificLinkSelectorInsideJob = "a";
            //            var linkHandles = await jobElementHandle.QuerySelectorAllAsync(specificLinkSelectorInsideJob);

            //            foreach (var linkHandle in linkHandles)
            //            {
            //                var hrefProperty = await linkHandle.GetPropertyAsync("href");
            //                string hrefValue = await hrefProperty.JsonValueAsync<string>();

            //                if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
            //                {
            //                    allJobDetailLinks.Add(hrefValue);
            //                }
            //            }
            //        }
            //        LogMessage($"Total job detail links collected: {allJobDetailLinks.Count}");
            //        await page.ScreenshotAsync("screenshot_job_listing_page.png");
            //        LogMessage("Screenshot of listing page saved.");
            //    }
            //    catch (Exception ex)
            //    {
            //        LogMessage($"Error collecting links from the listing page: {ex.Message}");
            //        LogMessage($"Current URL during error: {page.Url}");
            //    }

            //    LogMessage("\n--- Navigating to individual job detail links and printing HTML ---");
            //    int visitedLinkCounter = 0;
            //    foreach (string detailLink in allJobDetailLinks.Distinct())
            //    {
            //        visitedLinkCounter++;
            //        LogMessage($"\nAccessing Link #{visitedLinkCounter}: {detailLink}");

            //        using (var detailPage = await browserInstance.NewPageAsync())
            //        {
            //            try
            //            {
            //                await detailPage.GoToAsync(detailLink, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded }, Timeout = 60000 });
            //                LogMessage($"  Navigated to: {detailPage.Url}");

            //                int delayMilliseconds = new Random().Next(3000, 7001);
            //                LogMessage($"  Waiting for {delayMilliseconds / 1000.0} seconds...");
            //                await Task.Delay(delayMilliseconds);

            //                string detailPageContentSelector = ".job-detail";
            //                try
            //                {
            //                    await detailPage.WaitForSelectorAsync(detailPageContentSelector, new WaitForSelectorOptions { Timeout = 10000 });
            //                    LogMessage("  Detail page content loaded.");
            //                }
            //                catch
            //                {
            //                    LogMessage($"  Could not find selector '{detailPageContentSelector}' on the detail page. Page might not have loaded fully or been blocked.");
            //                }
            //                var extractedSections = new Dictionary<string, string>();
            //                string mongoConnectionString = "mongodb+srv://lumconon0911:SWD-SWD@cluster.slolqwf.mongodb.net/";
            //                string mongoDatabaseName = "JobScraperTopCV";
            //                string mongoCollectionName = "JobPostingTopCV";
            //                var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName, mongoCollectionName);

            //                var jobValue = await detailPage.QuerySelectorAsync(".job-detail__info--section-content-value");
            //                if (jobValue != null)
            //                {
            //                    string jobValueText = await jobValue.EvaluateFunctionAsync<string>("el => el.innerText");
            //                    extractedSections["Tien Luong"] = jobValueText.Trim();
            //                    LogMessage($"  Job Value: {jobValueText}");
            //                }
            //                else
            //                {
            //                    LogMessage("  Job Value section not found.");
            //                }
            //                var jobTitle = await detailPage.QuerySelectorAsync(".job-detail__info--title");
            //                if (jobTitle != null)
            //                {
            //                    string jobTitleText = await jobTitle.EvaluateFunctionAsync<string>("el => el.innerText");
            //                    extractedSections["Tittle"] = jobTitleText.Trim();

            //                }
            //                else
            //                {
            //                    LogMessage("  Job Title section not found.");
            //                }

            //                var jobDescriptionElement = await detailPage.QuerySelectorAllAsync(".job-description__item");
            //                if (jobDescriptionElement.Any())
            //                {
            //                    for (int i = 0; i <= Math.Min(jobDescriptionElement.Length, 3); i++)
            //                    {

            //                        var currentItemHandle = jobDescriptionElement[i];

            //                        var sectionTitle = await currentItemHandle.QuerySelectorAsync("h3");
            //                        var sectionContent = await currentItemHandle.QuerySelectorAsync(".job-description__item--content");
            //                        if (sectionTitle != null && sectionContent != null)
            //                        {
            //                            string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
            //                            string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
            //                            extractedSections[titleText.Trim()] = sectionContentText.Trim();
            //                            LogMessage($"  Title Section: {titleText}");
            //                            LogMessage($"  Content Section: {sectionContentText}");
            //                        }
            //                    }
            //                    LogMessage($"\n--- Chi tiết công việc đã trích xuất từ {detailLink} ---");
            //                    foreach (var entry in extractedSections)
            //                    {
            //                        LogMessage($"\nPhần: {entry.Key}");
            //                        LogMessage($"Nội dung: {entry.Value}");
            //                    }
            //                    LogMessage($"--- Kết thúc chi tiết công việc từ {detailLink} ---");

            //                    //Save to MongoDB
            //                    if (extractedSections.Any())
            //                    {
            //                        await mongoService.SaveJobDetailsAsync(detailLink, extractedSections);
            //                    }
            //                    else
            //                    {
            //                        LogMessage($"  Không có dữ liệu trích xuất để lưu cho URL: {detailLink}");
            //                    }
            //                }
            //                else
            //                {
            //                    LogMessage($"  Không tìm thấy phần tử '.job-description__item' nào trên trang {detailLink}.");
            //                }
            //            }
            //            catch (Exception detailEx)
            //            {
            //                LogMessage($"  Error accessing detail link {detailLink}: {detailEx.Message}");
            //                LogMessage($"  Current URL after error: {detailPage.Url}");
            //            }
            //        }
            //    }
            //}
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogTextBox.ScrollToEnd(); 
            });
        }

        //private async void TopCVCrawling()
        //{
        //    using (var page = await _browser.NewPageAsync())
        //    {
        //        LogMessage("Navigating to TopCV job listings page...");
        //        await page.GoToAsync("https://www.topcv.vn/viec-lam-it", new NavigationOptions { Timeout = 60000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
        //        await Task.Delay(5000);

        //        LogMessage($"Current URL: {page.Url}");

        //        List<string> allJobDetailLinks = new List<string>();

        //        try
        //        {
        //            string jobItemContainerSelector = ".avatar";
        //            LogMessage($"Waiting for job listing container '{jobItemContainerSelector}' to appear...");
        //            await page.WaitForSelectorAsync(jobItemContainerSelector, new WaitForSelectorOptions { Timeout = 15000 });
        //            LogMessage("Job listing container found. Starting link collection.");

        //            var allJobItemElements = await page.QuerySelectorAllAsync(jobItemContainerSelector);
        //            LogMessage($"Found {allJobItemElements.Length} job blocks on the listing page.");

        //            foreach (var jobElementHandle in allJobItemElements)
        //            {
        //                string specificLinkSelectorInsideJob = "a";
        //                var linkHandles = await jobElementHandle.QuerySelectorAllAsync(specificLinkSelectorInsideJob);

        //                foreach (var linkHandle in linkHandles)
        //                {
        //                    var hrefProperty = await linkHandle.GetPropertyAsync("href");
        //                    string hrefValue = await hrefProperty.JsonValueAsync<string>();

        //                    if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
        //                    {
        //                        allJobDetailLinks.Add(hrefValue);
        //                    }
        //                }
        //            }
        //            LogMessage($"Total job detail links collected: {allJobDetailLinks.Count}");
        //            await page.ScreenshotAsync("screenshot_job_listing_page.png");
        //            LogMessage("Screenshot of listing page saved.");
        //        }
        //        catch (Exception ex)
        //        {
        //            LogMessage($"Error collecting links from the listing page: {ex.Message}");
        //            LogMessage($"Current URL during error: {page.Url}");
        //        }

        //        LogMessage("\n--- Navigating to individual job detail links and printing HTML ---");
        //        int visitedLinkCounter = 0;
        //        foreach (string detailLink in allJobDetailLinks.Distinct().Take(20))
        //        {
        //            visitedLinkCounter++;
        //            LogMessage($"\nAccessing Link #{visitedLinkCounter}: {detailLink}");

        //            using (var detailPage = await _browser.NewPageAsync())
        //            {
        //                try
        //                {
        //                    await detailPage.GoToAsync(detailLink, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded }, Timeout = 60000 });
        //                    LogMessage($"  Navigated to: {detailPage.Url}");

        //                    int delayMilliseconds = new Random().Next(3000, 7001);
        //                    LogMessage($"  Waiting for {delayMilliseconds / 1000.0} seconds...");
        //                    await Task.Delay(delayMilliseconds);

        //                    string detailPageContentSelector = ".job-detail";
        //                    try
        //                    {
        //                        await detailPage.WaitForSelectorAsync(detailPageContentSelector, new WaitForSelectorOptions { Timeout = 10000 });
        //                        LogMessage("  Detail page content loaded.");
        //                    }
        //                    catch
        //                    {
        //                        LogMessage($"  Could not find selector '{detailPageContentSelector}' on the detail page. Page might not have loaded fully or been blocked.");
        //                    }
        //                    var extractedSections = new Dictionary<string, string>();
        //                    string mongoConnectionString = "mongodb+srv://lumconon0911:SWD-SWD@cluster.slolqwf.mongodb.net/";
        //                    string mongoDatabaseName = "JobScraperTopCV";
        //                    string mongoCollectionName = "JobPostingTopCV";
        //                    var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName, mongoCollectionName);

        //                    var jobValue = await detailPage.QuerySelectorAsync(".job-detail__info--section-content-value");
        //                    if (jobValue != null)
        //                    {
        //                        string jobValueText = await jobValue.EvaluateFunctionAsync<string>("el => el.innerText");
        //                        extractedSections["Tien Luong"] = jobValueText.Trim();
        //                        LogMessage($"  Job Value: {jobValueText}");
        //                    }
        //                    else
        //                    {
        //                        LogMessage("  Job Value section not found.");
        //                    }
        //                    var jobTitle = await detailPage.QuerySelectorAsync(".job-detail__info--title");
        //                    if (jobTitle != null)
        //                    {
        //                        string jobTitleText = await jobTitle.EvaluateFunctionAsync<string>("el => el.innerText");
        //                        extractedSections["Tittle"] = jobTitleText.Trim();

        //                    }
        //                    else
        //                    {
        //                        LogMessage("  Job Title section not found.");
        //                    }

        //                    var jobDescriptionElement = await detailPage.QuerySelectorAllAsync(".job-description__item");
        //                    if (jobDescriptionElement.Any())
        //                    {
        //                        for (int i = 0; i <= Math.Min(jobDescriptionElement.Length, 3); i++)
        //                        {

        //                            var currentItemHandle = jobDescriptionElement[i];

        //                            var sectionTitle = await currentItemHandle.QuerySelectorAsync("h3");
        //                            var sectionContent = await currentItemHandle.QuerySelectorAsync(".job-description__item--content");
        //                            if (sectionTitle != null && sectionContent != null)
        //                            {
        //                                string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                extractedSections[titleText.Trim()] = sectionContentText.Trim();
        //                                LogMessage($"  Title Section: {titleText}");
        //                                LogMessage($"  Content Section: {sectionContentText}");
        //                            }
        //                        }
        //                        LogMessage($"\n--- Chi tiết công việc đã trích xuất từ {detailLink} ---");
        //                        foreach (var entry in extractedSections)
        //                        {
        //                            LogMessage($"\nPhần: {entry.Key}");
        //                            LogMessage($"Nội dung: {entry.Value}");
        //                        }
        //                        LogMessage($"--- Kết thúc chi tiết công việc từ {detailLink} ---");

        //                        //Save to MongoDB
        //                        if (extractedSections.Any())
        //                        {
        //                            await mongoService.SaveJobDetailsAsync(detailLink, extractedSections);
        //                        }
        //                        else
        //                        {
        //                            LogMessage($"  Không có dữ liệu trích xuất để lưu cho URL: {detailLink}");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        LogMessage($"  Không tìm thấy phần tử '.job-description__item' nào trên trang {detailLink}.");
        //                    }
        //                }
        //                catch (Exception detailEx)
        //                {
        //                    LogMessage($"  Error accessing detail link {detailLink}: {detailEx.Message}");
        //                    LogMessage($"  Current URL after error: {detailPage.Url}");
        //                }
        //            }
        //        }
        //    }
        //}

        //private async void ViecLam24hCrawling()
        //{

        //    using (var page = await _browser.NewPageAsync())
        //    {
        //        LogMessage("Navigating to TopCV job listings page...");
        //        await page.GoToAsync("https://vieclam24h.vn/viec-lam-it-phan-mem-o8.html", new NavigationOptions { Timeout = 60000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
        //        await Task.Delay(5000);

        //        LogMessage($"Current URL: {page.Url}");

        //        List<string> allJobDetailLinks = new List<string>();

        //        string jobItemContainerSelector = ".grid grid-cols-1 gap-y-2 lg:gap-y-2.5";
        //        string fullLinkSelector = jobItemContainerSelector + " a";
        //        LogMessage($"Waiting for job listing container '{jobItemContainerSelector}' to appear...");
        //        try
        //        {

        //            await page.WaitForSelectorAsync(jobItemContainerSelector, new WaitForSelectorOptions { Timeout = 15000 });
        //            LogMessage("Job listing container found. Starting link collection.");
        //            var linkElementHandles = await page.QuerySelectorAllAsync(fullLinkSelector);
        //            if (jobItemContainerSelector.Any())
        //            {
        //                foreach (var linkHandle in linkElementHandles)
        //                {
        //                    var hrefProperty = await linkHandle.GetPropertyAsync("href");
        //                    string? hrefValue = await hrefProperty.JsonValueAsync<string>();
        //                    if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
        //                    {
        //                        allJobDetailLinks.Add(hrefValue);
        //                    }
        //                }
        //            }
        //            LogMessage($"Total job detail links collected: {allJobDetailLinks.Count}");
        //            await page.ScreenshotAsync("screenshot_job_listing_page.png");
        //            LogMessage("Screenshot of listing page saved.");
        //        }
        //        catch (Exception ex)
        //        {
        //            LogMessage($"Error collecting links from the listing page: {ex.Message}");
        //            LogMessage($"Current URL during error: {page.Url}");
        //        }

        //        LogMessage("\n--- Navigating to individual job detail links and printing HTML ---");
        //        int visitedLinkCounter = 0;
        //        foreach (string detailLink in allJobDetailLinks.Distinct().Take(20))
        //        {
        //            visitedLinkCounter++;
        //            LogMessage($"\nAccessing Link #{visitedLinkCounter}: {detailLink}");

        //            using (var detailPage = await _browser.NewPageAsync())
        //            {
        //                try
        //                {
        //                    await detailPage.GoToAsync(detailLink, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded }, Timeout = 60000 });
        //                    LogMessage($"  Navigated to: {detailPage.Url}");

        //                    int delayMilliseconds = new Random().Next(3000, 7001);
        //                    LogMessage($"  Waiting for {delayMilliseconds / 1000.0} seconds...");
        //                    await Task.Delay(delayMilliseconds);

        //                    string detailPageContentSelector = ".job-detail";
        //                    try
        //                    {
        //                        await detailPage.WaitForSelectorAsync(detailPageContentSelector, new WaitForSelectorOptions { Timeout = 10000 });
        //                        LogMessage("  Detail page content loaded.");
        //                    }
        //                    catch
        //                    {
        //                        LogMessage($"  Could not find selector '{detailPageContentSelector}' on the detail page. Page might not have loaded fully or been blocked.");
        //                    }
        //                    var extractedSections = new Dictionary<string, string>();
        //                    string mongoConnectionString = "mongodb+srv://lumconon0911:SWD-SWD@cluster.slolqwf.mongodb.net/";
        //                    string mongoDatabaseName = "JobScraperTopCV";
        //                    string mongoCollectionName = "JobPostingTopCV";
        //                    var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName, mongoCollectionName);

        //                    var jobValue = await detailPage.QuerySelectorAsync(".font-semibold text-14 text-[#8B5CF6]");
        //                    if (jobValue != null)
        //                    {
        //                        string jobValueText = await jobValue.EvaluateFunctionAsync<string>("el => el.innerText");
        //                        extractedSections["Tien Luong"] = jobValueText.Trim();
        //                        LogMessage($"  Job Value: {jobValueText}");
        //                    }
        //                    else
        //                    {
        //                        LogMessage("  Job Value section not found.");
        //                    }
        //                    var jobTitle = await detailPage.QuerySelectorAsync(".font-semibold text-18 md:text-24 leading-snug");
        //                    if (jobTitle != null)
        //                    {
        //                        string jobTitleText = await jobTitle.EvaluateFunctionAsync<string>("el => el.innerText");
        //                        extractedSections["Tittle"] = jobTitleText.Trim();

        //                    }
        //                    else
        //                    {
        //                        LogMessage("  Job Title section not found.");
        //                    }

        //                    var jobDescriptionElement = await detailPage.QuerySelectorAllAsync(".jsx-5b2773f86d2f74b");
        //                    if (jobDescriptionElement.Any())
        //                    {
        //                        for (int i = 0; i <= Math.Min(jobDescriptionElement.Length, 1); i++)
        //                        {

        //                            var currentItemHandle = jobDescriptionElement[i];

        //                            var sectionTitle = await currentItemHandle.QuerySelectorAsync("h2");
        //                            var sectionContent = await currentItemHandle.QuerySelectorAsync(".jsx-5b2773f86d2f74b mb-2 text-14 break-words text-se-neutral-80 text-description");
        //                            if (sectionTitle != null && sectionContent != null )
        //                            {
        //                                string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                extractedSections[titleText.Trim()] = sectionContentText.Trim();
        //                                LogMessage($"  Title Section: {titleText}");
        //                                LogMessage($"  Content Section: {sectionContentText}");
        //                            }
        //                        }
        //                        var sectionRequiement = await detailPage.QuerySelectorAsync(".jsx-5b2773f86d2f74b mb-4 md:mb-8");
        //                        if (sectionRequiement != null)
        //                        {
        //                            string sectionRequiementText = await sectionRequiement.EvaluateFunctionAsync<string>("el => el.innerText");
        //                            extractedSections["Yêu cầu công việc"] = sectionRequiementText.Trim();
        //                            LogMessage($"  Yêu cầu công việc: {sectionRequiementText}");
        //                        }

        //                        for (int i = 1; i <= Math.Min(jobDescriptionElement.Length, 3); i++)
        //                        {

        //                            var currentItemHandle = jobDescriptionElement[i];

        //                            var sectionTitle = await currentItemHandle.QuerySelectorAsync("h2");
        //                            var sectionContent = await currentItemHandle.QuerySelectorAsync(".jsx-5b2773f86d2f74b mb-2 text-14 break-words text-se-neutral-80 text-description");
        //                            if (sectionTitle != null && sectionContent != null)
        //                            {
        //                                string titleText = await sectionTitle.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                string sectionContentText = await sectionContent.EvaluateFunctionAsync<string>("el => el.innerText");
        //                                extractedSections[titleText.Trim()] = sectionContentText.Trim();
        //                                LogMessage($"  Title Section: {titleText}");
        //                                LogMessage($"  Content Section: {sectionContentText}");
        //                            }
        //                        }

        //                        LogMessage($"\n--- Chi tiết công việc đã trích xuất từ {detailLink} ---");
        //                        foreach (var entry in extractedSections)
        //                        {
        //                            LogMessage($"\nPhần: {entry.Key}");
        //                            LogMessage($"Nội dung: {entry.Value}");
        //                        }
        //                        LogMessage($"--- Kết thúc chi tiết công việc từ {detailLink} ---");

        //                        //Save to MongoDB
        //                        if (extractedSections.Any())
        //                        {
        //                            await mongoService.SaveJobDetailsAsync(detailLink, extractedSections);
        //                        }
        //                        else
        //                        {
        //                            LogMessage($"  Không có dữ liệu trích xuất để lưu cho URL: {detailLink}");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        LogMessage($"  Không tìm thấy phần tử '.job-description__item' nào trên trang {detailLink}.");
        //                    }
        //                }
        //                catch (Exception detailEx)
        //                {
        //                    LogMessage($"  Error accessing detail link {detailLink}: {detailEx.Message}");
        //                    LogMessage($"  Current URL after error: {detailPage.Url}");
        //                }
        //            }
        //        }
        //    }
        //}

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _httpClient.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CrawlingTopcvWindow crawlingTopcvWindow = new CrawlingTopcvWindow();
            crawlingTopcvWindow.Show();
            this.Close();
        }
    }
}