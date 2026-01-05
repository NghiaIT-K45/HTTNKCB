package com.hospitaltriage.pages.components;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public final class Navbar {
    private final WebDriver driver;
    private final WebDriverWait wait;

    public Navbar(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(10));
    }

    private WebElement find(By by) {
        return wait.until(ExpectedConditions.presenceOfElementLocated(by));
    }

    public boolean isLinkVisible(String linkText) {
        // Backward-compatible aliases for older test code.
        // The app UI uses Vietnamese labels: "Đăng nhập" / "Đăng xuất".
        if ("Login".equalsIgnoreCase(linkText)) {
            return driver.findElements(By.linkText("Đăng nhập")).stream().anyMatch(WebElement::isDisplayed)
                    || driver.findElements(By.linkText("Login")).stream().anyMatch(WebElement::isDisplayed);
        }
        if ("Logout".equalsIgnoreCase(linkText)) {
            return driver.findElements(By.xpath("//button[normalize-space()='Đăng xuất' or normalize-space()='Logout']")).stream()
                    .anyMatch(WebElement::isDisplayed);
        }
        return driver.findElements(By.linkText(linkText)).stream().anyMatch(WebElement::isDisplayed);
    }

    public void clickLink(String linkText) {
        wait.until(ExpectedConditions.elementToBeClickable(By.linkText(linkText))).click();
    }

    public void openCatalogDropdown() {
        // "Danh mục" dropdown toggle
        wait.until(ExpectedConditions.elementToBeClickable(By.linkText("Danh mục"))).click();
    }

    public void goToDepartments() {
        openCatalogDropdown();
        wait.until(ExpectedConditions.elementToBeClickable(By.linkText("Khoa khám"))).click();
    }

    public void goToDoctors() {
        openCatalogDropdown();
        wait.until(ExpectedConditions.elementToBeClickable(By.linkText("Bác sĩ"))).click();
    }

    public void logout() {
        // Logout is inside the avatar dropdown.
        // 1) open avatar dropdown
        By avatarToggle = By.cssSelector("ul.navbar-nav.ms-auto .nav-item.dropdown > a.nav-link.dropdown-toggle");
        wait.until(ExpectedConditions.elementToBeClickable(avatarToggle)).click();

        // 2) click logout
        By logoutBtn = By.xpath("//button[normalize-space()='Đăng xuất' or normalize-space()='Logout']");
        wait.until(ExpectedConditions.elementToBeClickable(logoutBtn)).click();

        // 3) ensure we see Login/Đăng nhập button afterwards
        wait.until(d -> d.findElements(By.linkText("Đăng nhập")).size() > 0 || d.findElements(By.linkText("Login")).size() > 0);
    }

    public void loginButtonClick() {
        // App label is "Đăng nhập".
        By loginLink = driver.findElements(By.linkText("Đăng nhập")).isEmpty()
                ? By.linkText("Login")
                : By.linkText("Đăng nhập");
        wait.until(ExpectedConditions.elementToBeClickable(loginLink)).click();
    }
}
