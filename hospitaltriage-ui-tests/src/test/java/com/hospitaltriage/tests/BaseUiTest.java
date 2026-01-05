package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.driver.DriverFactory;
import com.hospitaltriage.pages.HomePage;
import com.hospitaltriage.pages.LoginPage;
import com.hospitaltriage.pages.components.Navbar;
import org.openqa.selenium.OutputType;
import org.openqa.selenium.TakesScreenshot;
import org.openqa.selenium.WebDriver;
import org.testng.ITestResult;
import org.testng.annotations.AfterMethod;
import org.testng.annotations.AfterClass;
import org.testng.annotations.BeforeClass;

import java.io.File;
import java.nio.file.Files;
import java.nio.file.Path;

public abstract class BaseUiTest {
    protected WebDriver driver;

    /**
     * Expose WebDriver to TestNG listeners (e.g. for screenshot capture on failure).
     */
    public WebDriver getDriver() {
        return driver;
    }

    @BeforeClass(alwaysRun = true)
    public void beforeClass() {
        driver = DriverFactory.createDriver();
    }

    @AfterClass(alwaysRun = true)
    public void afterClass() {
        if (driver != null) {
            driver.quit();
        }
    }

    // Screenshot + artifact capture is handled by com.hospitaltriage.listeners.UiTestListener

    protected HomePage loginAs(String roleKey) {
        String email = Config.credential(roleKey, "email");
        String password = Config.credential(roleKey, "password");
        return new LoginPage(driver).open().login(email, password);
    }

    protected void logoutIfPossible() {
        HomePage home = new HomePage(driver);
        Navbar nav = home.navbar();
        if (nav.isLinkVisible("Login")) {
            return; // already logged out
        }
        try {
            nav.logout();
        } catch (Exception ignored) {
            // If logout isn't visible (e.g. we are on Login page already), ignore.
        }
    }
}
