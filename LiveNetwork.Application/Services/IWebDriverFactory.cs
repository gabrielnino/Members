using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace LiveNetwork.Application.Services
{
    public interface IWebDriverFactory
    {
        IWebDriver Create(bool hide = false);
        IWebDriver Create(Action<ChromeOptions> configureOptions);
        ChromeOptions GetDefaultOptions(string downloadFolder);
    }
}
