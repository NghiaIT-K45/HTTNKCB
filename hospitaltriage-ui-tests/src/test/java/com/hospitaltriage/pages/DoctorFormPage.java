package com.hospitaltriage.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public final class DoctorFormPage extends BasePage {
    public DoctorFormPage(WebDriver driver) {
        super(driver);
    }

    public DoctorFormPage setCode(String code) {
        type(By.id("Code"), code);
        return this;
    }

    /**
     * Negative-testing helper: bypass any client-side maxlength/input constraints
     * by forcing the value using JavaScript.
     */
    public DoctorFormPage forceCode(String code) {
        setValueByJs(By.id("Code"), code);
        return this;
    }

    public DoctorFormPage setFullName(String name) {
        type(By.id("FullName"), name);
        return this;
    }

    public DoctorFormPage forceFullName(String name) {
        setValueByJs(By.id("FullName"), name);
        return this;
    }

    public DoctorFormPage selectDepartmentByVisibleText(String visibleText) {
        select(By.id("DepartmentId"), visibleText);
        return this;
    }

    /**
     * Negative testing helper: force an invalid DepartmentId value (not present in the dropdown).
     * This bypasses client-side "required" validation and lets the server return
     * "Khoa khám không tồn tại.".
     */
    public DoctorFormPage forceDepartmentId(String value) {
        setValueByJs(By.id("DepartmentId"), value);
        return this;
    }

    public DoctorFormPage setIsActive(boolean value) {
        WebElement cb = $(By.id("IsActive"));
        if (cb.isSelected() != value) cb.click();
        return this;
    }

    public DoctorsPage save() {
        // UI label is "Lưu".
        click(By.xpath("//button[@type='submit' and normalize-space()='Lưu']"));
        // On success controller redirects to Index (h2 'Bác sĩ')
        try {
            $(By.xpath("//h2[normalize-space()='Bác sĩ']"));
        } catch (Exception ignored) {
            // Stay on form if validation fails
        }
        return new DoctorsPage(driver);
    }

    public String validationSummaryText() {
        return collectErrorTexts();
    }
}
