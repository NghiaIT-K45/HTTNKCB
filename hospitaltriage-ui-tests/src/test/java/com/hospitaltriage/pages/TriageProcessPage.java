package com.hospitaltriage.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

public final class TriageProcessPage extends BasePage {
    public TriageProcessPage(WebDriver driver) {
        super(driver);
        $(By.xpath("//h2[contains(normalize-space(),'Phân luồng')]"));
    }

    public TriageProcessPage setSymptoms(String symptoms) {
        type(By.id("Symptoms"), symptoms);
        return this;
    }

    public TriageProcessPage selectDepartment(String visibleTextOrNull) {
        if (visibleTextOrNull != null) {
            select(By.id("DepartmentId"), visibleTextOrNull);
        }
        return this;
    }

    public TriageProcessPage selectDoctor(String visibleTextOrNull) {
        if (visibleTextOrNull != null) {
            select(By.id("DoctorId"), visibleTextOrNull);
        }
        return this;
    }

    public HomePage submit() {
        click(By.xpath("//button[normalize-space()='Xác nhận phân luồng']"));
        return new HomePage(driver);
    }

    public String validationSummaryText() {
        if (!isPresent(By.cssSelector(".text-danger"))) return "";
        return driver.findElement(By.cssSelector(".text-danger")).getText();
    }
}
