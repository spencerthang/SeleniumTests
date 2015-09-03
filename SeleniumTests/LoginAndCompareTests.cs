using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Threading;
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

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds, int count = 0)
        {
            if (timeoutInSeconds > 0)
            {
                var maxTime = DateTime.Now + TimeSpan.FromSeconds(timeoutInSeconds);
                while (DateTime.Now < maxTime)
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    var ret = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
                    if(ret.Count >= count)
                        return ret;
                }
            }
            return driver.FindElements(by);
        }
    }

    public class GearsetComparisonResults
    {
        private readonly IWebDriver m_Browser;

        public GearsetComparisonResults(IWebDriver browser)
        {
            m_Browser = browser;
        }

        public GearsetComparisonResults LoadResults(int expectedDifferences)
        {
            // Wait till at least 2 elements have loaded
            var checkboxes = m_Browser.FindElements(By.ClassName("ko-grid-selection-element"), 300, expectedDifferences);

            foreach (var checkbox in checkboxes)
            {
                checkbox.Click();
                Thread.Sleep(2000);
            }

            return this;
        }
    }

    public class GearsetConfigure
    {
        private readonly IWebDriver m_Browser;

        public GearsetConfigure(IWebDriver browser)
        {
            m_Browser = browser;
        }

        public GearsetComparisonResults Compare(string source, string target)
        {
            // Wait till all 4 elements have loaded via JS injection
            m_Browser.FindElements(By.Id("configure-auth-username"), 30, 4);

            // Fill via Javascript - Selenium has problems with Javascript-inserted DOM element visibility
            var js = m_Browser as IJavaScriptExecutor;
            js.ExecuteScript("$('.configure-oauth-widget #configure-auth-username').val('" + target + "');");
            js.ExecuteScript("$('.configure-oauth-widget #configure-auth-username:first').val('" + source + "');");
            js.ExecuteScript("$('.configure-oauth-widget #configure-auth-username').change();");

            var compareButton = m_Browser.FindElement(By.CssSelector("#compareico:enabled"), 10);
            compareButton.Click();

            return new GearsetComparisonResults(m_Browser);
        }
    }

    public class GearsetHosted
    {
        private readonly IWebDriver m_Browser;

        public GearsetHosted(IWebDriver browser)
        {
            m_Browser = browser;

            browser.Navigate().GoToUrl("http://localhost:1234");
        }

        public GearsetConfigure Login(string username, string password)
        {
            var loginForm = m_Browser.FindElement(By.Id("splashscreen-container-login-via-salesforce"), 30);
            loginForm.FindElement(By.Id("get-started")).Click();

            m_Browser.FindElement(By.Id("username"), 10).SendKeys(username);
            m_Browser.FindElement(By.Id("password"), 10).SendKeys(password);

            m_Browser.FindElement(By.Id("Login"), 10).Click();

            return new GearsetConfigure(m_Browser);
        }
    }

    public class LoginAndCompareTests
    {
        private const string c_SalesforceUsername = "spencerthang@gmail.com";
        private const string c_SalesforcePassword = "P@ssw0rd1";
        private const string c_SalesforceSource = "spencerthang@gmail.com";
        private const string c_SalesforceTarget = "spencer.thang@red-gate.com";
        private const int c_ExpectedDifferences = 10;

        [Test]
        public void Login()
        {
            var browser = new ChromeDriver(new ChromeOptions
                                           {
                                               LeaveBrowserRunning = false
                                           });


            new GearsetHosted(browser).Login(c_SalesforceUsername, c_SalesforcePassword);
        }

        [Test]
        public void LoginAndCompare()
        {
            var browser = new ChromeDriver(new ChromeOptions
            {
                LeaveBrowserRunning = false
            });


            var gearsetConfigure = new GearsetHosted(browser).Login(c_SalesforceUsername, c_SalesforcePassword);
            gearsetConfigure.Compare(c_SalesforceSource, c_SalesforceTarget);
        }

        [Test]
        public void LoginCompareAndTestResults()
        {
            var browser = new ChromeDriver(new ChromeOptions
            {
                LeaveBrowserRunning = false
            });


            var gearsetConfigure = new GearsetHosted(browser).Login(c_SalesforceUsername, c_SalesforcePassword);
            var gearsetCompareResults = gearsetConfigure.Compare(c_SalesforceSource, c_SalesforceTarget);
            gearsetCompareResults.LoadResults(c_ExpectedDifferences);
        }
    }
}
