package com.hospitaltriage.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public final class DepartmentFormPage extends BasePage {
    public DepartmentFormPage(WebDriver driver) {
        super(driver);
    }

    public DepartmentFormPage setCode(String code) {
        type(By.id("Code"), code);
        return this;
    }

    /**
     * Negative-testing helper: bypass any client-side maxlength/input constraints
     * by forcing the value using JavaScript.
     */
    public DepartmentFormPage forceCode(String code) {
        setValueByJs(By.id("Code"), code);
        return this;
    }

    public DepartmentFormPage setName(String name) {
        type(By.id("Name"), name);
        return this;
    }

    public DepartmentFormPage forceName(String name) {
        setValueByJs(By.id("Name"), name);
        return this;
    }

    public DepartmentFormPage setIsActive(boolean value) {
        setCheckbox(By.id("IsActive"), value);
        return this;
    }

    public DepartmentFormPage setIsGeneral(boolean value) {
        setCheckbox(By.id("IsGeneral"), value);
        return this;
    }

    private void setCheckbox(By locator, boolean value) {
        WebElement cb = $(locator);
        if (cb.isSelected() != value) {
            cb.click();
        }
    }

    public DepartmentsPage save() {
        // UI label is "Lưu".
        click(By.xpath("//button[@type='submit' and normalize-space()='Lưu']"));
        // On success redirects to Index (h2 'Khoa khám')
        try {
            $(By.xpath("//h2[normalize-space()='Khoa khám']"));
        } catch (Exception ignored) {
            // Stay on form if validation fails
        }
        return new DepartmentsPage(driver);
    }

    public String validationSummaryText() {
        return collectErrorTexts();
    }
}
