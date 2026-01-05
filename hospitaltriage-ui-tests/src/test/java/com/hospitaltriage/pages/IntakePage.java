package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

public final class IntakePage extends BasePage {
    public IntakePage(WebDriver driver) {
        super(driver);
    }

    public IntakePage open() {
        driver.get(Config.baseUrl() + "Intake/Register");
        return this;
    }

    public IntakePage setFullName(String name) {
        type(By.id("FullName"), name);
        return this;
    }

    public IntakePage setDateOfBirth(String yyyyMmDd) {
        clearInput(By.id("DateOfBirth"));
        type(By.id("DateOfBirth"), yyyyMmDd);
        return this;
    }

    public IntakePage clearDateOfBirth() {
        clearInput(By.id("DateOfBirth"));
        return this;
    }

    public IntakePage selectGender(String visibleText) {
        select(By.id("Gender"), visibleText);
        return this;
    }

    public IntakePage setIdentityNumber(String v) {
        type(By.id("IdentityNumber"), v);
        return this;
    }

    /**
     * Negative-testing helper: force-set value using JavaScript (bypasses any input constraints).
     */
    public IntakePage forceIdentityNumber(String v) {
        setValueByJs(By.id("IdentityNumber"), v);
        return this;
    }

    public IntakePage setPhone(String v) {
        type(By.id("Phone"), v);
        return this;
    }

    public IntakePage forcePhone(String v) {
        setValueByJs(By.id("Phone"), v);
        return this;
    }

    public IntakePage setInsuranceCode(String v) {
        type(By.id("InsuranceCode"), v);
        return this;
    }

    public IntakePage forceInsuranceCode(String v) {
        setValueByJs(By.id("InsuranceCode"), v);
        return this;
    }

    public IntakePage setAddress(String v) {
        type(By.id("Address"), v);
        return this;
    }

    public IntakePage forceAddress(String v) {
        setValueByJs(By.id("Address"), v);
        return this;
    }

    public IntakeResultPage submit() {
        click(By.xpath("//button[normalize-space()='Tiếp nhận']"));
        return new IntakeResultPage(driver);
    }

    /**
     * Click submit but stay on the same page (useful for validation/negative tests).
     */
    public IntakePage submitExpectingError() {
        click(By.xpath("//button[normalize-space()='Tiếp nhận']"));
        // Expect validation summary to appear (or at least remain on the form)
        return this;
    }

    public String validationSummaryText() {
        return collectErrorTexts();
    }
}
