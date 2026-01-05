package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.support.ui.ExpectedConditions;

public final class LoginPage extends BasePage {
    public LoginPage(WebDriver driver) {
        super(driver);
    }

    public LoginPage open() {
        driver.get(Config.baseUrl() + "Identity/Account/Login");
        return this;
    }

    private By emailLocator() {
        // Default ASP.NET Core Identity scaffolding uses name="Input.Email" and id="Input_Email"
        if (isPresent(By.cssSelector("input[name='Input.Email']"))) return By.cssSelector("input[name='Input.Email']");
        if (isPresent(By.id("Input_Email"))) return By.id("Input_Email");
        // fallback
        return By.cssSelector("input[type='email']");
    }

    private By passwordLocator() {
        if (isPresent(By.cssSelector("input[name='Input.Password']"))) return By.cssSelector("input[name='Input.Password']");
        if (isPresent(By.id("Input_Password"))) return By.id("Input_Password");
        return By.cssSelector("input[type='password']");
    }

    public LoginPage typeEmail(String email) {
        type(emailLocator(), email);
        return this;
    }

    public LoginPage typePassword(String password) {
        type(passwordLocator(), password);
        return this;
    }

    public HomePage submit() {
        click(By.cssSelector("button[type='submit']"));
        // Wait for either:
        //  - navigation away from Login, OR
        //  - an error message shown on Login (invalid credentials / missing seeded user)
        wait.until(d -> !d.getCurrentUrl().contains("/Identity/Account/Login") || hasErrorMessage());

        if (driver.getCurrentUrl().contains("/Identity/Account/Login")) {
            throw new AssertionError("Login failed / stayed on Login page. Error: " + errorText());
        }

        return new HomePage(driver);
    }

    /**
     * Submit expecting to stay on Login page (invalid credentials / validation errors).
     */
    public LoginPage submitExpectingError() {
        click(By.cssSelector("button[type='submit']"));
        // Wait for either an error message or just ensure URL still contains login.
        wait.until(d -> d.getCurrentUrl().contains("/Identity/Account/Login"));
        return this;
    }

    public HomePage login(String email, String password) {
        return typeEmail(email).typePassword(password).submit();
    }

    public boolean hasErrorMessage() {
        // Identity uses validation-summary-errors or text-danger
        return driver.findElements(By.cssSelector(".validation-summary-errors, .text-danger")).stream()
                .map(WebElement::getText)
                .anyMatch(t -> t != null && !t.isBlank());
    }

    public String errorText() {
        return driver.findElements(By.cssSelector(".validation-summary-errors, .text-danger")).stream()
                .map(WebElement::getText)
                .filter(t -> t != null && !t.isBlank())
                .findFirst()
                .orElse("");
    }

    public String emailHtml5ValidationMessage() {
        try {
            WebElement el = driver.findElement(emailLocator());
            return (String) ((JavascriptExecutor) driver).executeScript("return arguments[0].validationMessage;", el);
        } catch (Exception e) {
            return "";
        }
    }
}
