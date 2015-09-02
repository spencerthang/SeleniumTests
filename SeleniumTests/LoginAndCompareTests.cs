using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(ExpectedConditions.ElementIsVisible(by));
            }
            return driver.FindElement(by);
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElements(by));
            }
            return driver.FindElements(by);
        }
    }

    public class GearsetConfigure
    {
        private readonly IWebDriver m_Browser;

        public GearsetConfigure(IWebDriver browser)
        {
            m_Browser = browser;
        }
    }

    public class GearsetHosted
    {
        private readonly IWebDriver m_Browser;

        public GearsetHosted(IWebDriver browser)
        {
            m_Browser = browser;

            browser.Navigate().GoToUrl("https://app.gearset.com");
        }

        public GearsetConfigure Login()
        {
            var loginForm = m_Browser.FindElement(By.Id("splashscreen-container-login-via-salesforce"), 30);
            loginForm.FindElement(By.Id("get-started")).Click();

            m_Browser.FindElement(By.Id("username"), 10).SendKeys("spencerthang@gmail.com");
            m_Browser.FindElement(By.Id("password"), 10).SendKeys("P@ssw0rd1");

            m_Browser.FindElement(By.Id("Login"), 10).Click();

            return new GearsetConfigure(m_Browser);
        }
    }

    public class LoginAndCompareTests
    {
        [Test]
        public void Foo()
        {
            var browser = new ChromeDriver(new ChromeOptions
                                           {
                                               LeaveBrowserRunning = false
                                           });


            var gearsetConfigure = new GearsetHosted(browser).Login();
        }
    }
}
