package com.hospitaltriage.driver;

import com.hospitaltriage.config.Config;
import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.edge.EdgeDriver;
import org.openqa.selenium.edge.EdgeOptions;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxOptions;

import java.time.Duration;

public final class DriverFactory {
    private DriverFactory() {
    }

    public static WebDriver createDriver() {
        String browser = Config.browser().toLowerCase();
        boolean headless = Config.headless();

        WebDriver driver;
        switch (browser) {
            case "firefox" -> {
                WebDriverManager.firefoxdriver().setup();
                FirefoxOptions options = new FirefoxOptions();
                if (headless) options.addArguments("-headless");
                // accept self-signed cert on localhost https
                options.setAcceptInsecureCerts(true);
                driver = new FirefoxDriver(options);
            }
            case "edge" -> {
                WebDriverManager.edgedriver().setup();
                EdgeOptions options = new EdgeOptions();
                if (headless) options.addArguments("--headless=new");
                options.setAcceptInsecureCerts(true);
                options.addArguments("--ignore-certificate-errors");
                options.addArguments("--allow-insecure-localhost");
                driver = new EdgeDriver(options);
            }
            case "chrome" -> {
                WebDriverManager.chromedriver().setup();
                ChromeOptions options = new ChromeOptions();
                if (headless) options.addArguments("--headless=new");
                options.setAcceptInsecureCerts(true);
                options.addArguments("--ignore-certificate-errors");
                options.addArguments("--allow-insecure-localhost");
                options.addArguments("--disable-gpu");
                options.addArguments("--window-size=1366,900");
                driver = new ChromeDriver(options);
            }
            default -> throw new IllegalArgumentException("Unsupported browser: " + browser);
        }

        driver.manage().timeouts().pageLoadTimeout(Duration.ofSeconds(30));
        driver.manage().timeouts().implicitlyWait(Duration.ofMillis(0)); // explicit waits only
        driver.manage().window().maximize();

        return driver;
    }
}
