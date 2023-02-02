using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Clubmed;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ChromeDriver _driver;
    private string _baseUrl = "https://www.clubmed.co.il/r/val-thorens-sensations/y?departure_city=TLV";
    private string _token = "6010440690:AAFpOnZkhD_11OAcj80PaPTsBSv0kSGR84c";
    private readonly TelegramBotClient _botClient;
    private ChatId _chatId = new(-825328442);

    public Worker(ILogger<Worker> logger)
    {
        _botClient = new TelegramBotClient(_token);
        _logger = logger;
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
        _driver.Manage().Window.Maximize();
        _driver.Navigate().GoToUrl(_baseUrl);
        Thread.Sleep(TimeSpan.FromSeconds(3));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var price = DoWork();
                if (price < 2600)
                {
                    var message = await _botClient.SendTextMessageAsync(
                                                                        chatId: _chatId,
                                                                        text: "Price is: " + price,
                                                                        cancellationToken: stoppingToken);
                }
                else
                {
                    var message = await _botClient.SendTextMessageAsync(
                                                                        chatId: _chatId,
                                                                        text: $"No good price yet right now the price is {price}",
                                                                        cancellationToken: stoppingToken);
                }
            }
            catch (Exception e)
            {
                var message = await _botClient.SendTextMessageAsync(
                                                                    chatId: _chatId,
                                                                    text: "Error",
                                                                    cancellationToken: stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private int DoWork()
    {
        new WebDriverWait(_driver, TimeSpan.FromSeconds(60))
            .Until(ExpectedConditions
                       .ElementToBeClickable(By.XPath("//*[@id='calendar']/div/div[1]/div/div[1]/div/div/div[2]/button/div[1]")));
        var element = _driver.FindElement(By.XPath("//*[@id='calendar']/div/div[1]/div/div[1]/div/div/div[2]/button/div[1]"));
        _driver.ExecuteScript("arguments[0].scrollIntoView(true);", element);
        element.Click();
        var date =
            _driver.FindElement(By.XPath("/html/body/div[5]/div/div[1]/main/div[12]/div[2]/div/div/form/div[2]/div/div[3]/div/div/div/div/div[2]/div[4]/div[1]/div[3]/span[2]"));
        var elementText = date.Text;
        const char euroSign = (char)8364;
        var split = elementText.Split(euroSign);
        var price = split[1].Trim();
        price = price.Replace(",", "");
        var result = int.Parse(price);
        return result;
    }
}