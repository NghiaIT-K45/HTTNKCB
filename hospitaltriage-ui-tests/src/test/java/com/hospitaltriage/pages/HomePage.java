package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.components.Navbar;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

public final class HomePage extends BasePage {
    private final Navbar navbar;

    public HomePage(WebDriver driver) {
        super(driver);
        this.navbar = new Navbar(driver);
    }

    public HomePage open() {
        driver.get(Config.baseUrl());
        return this;
    }

    public Navbar navbar() {
        return navbar;
    }

    public String successAlert() {
        if (!isPresent(By.cssSelector(".alert.alert-success"))) return "";
        return $(By.cssSelector(".alert.alert-success")).getText().trim();
    }

    public String errorAlert() {
        if (!isPresent(By.cssSelector(".alert.alert-danger"))) return "";
        return $(By.cssSelector(".alert.alert-danger")).getText().trim();
    }

    public void goToIntakeFromHome() {
        click(By.linkText("Vào Tiếp nhận"));
    }

    public void goToTriageFromHome() {
        // Home page doesn't provide a direct triage link.
        // Use navbar for triage navigation.
        navbar.clickLink("Phân luồng");
    }

    public void goToDashboardFromHome() {
        // Home page card uses label "Dashboard".
        click(By.xpath("//a[normalize-space()='Dashboard']"));
    }
}
