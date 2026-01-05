package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public final class DoctorsPage extends BasePage {
    public DoctorsPage(WebDriver driver) {
        super(driver);
    }

    public DoctorsPage open() {
        driver.get(Config.baseUrl() + "Doctors");
        return this;
    }

    public DoctorsPage filter(String keyword, String departmentVisibleTextOrNull) {
        if (keyword != null) type(By.name("search"), keyword);
        if (departmentVisibleTextOrNull != null) {
            select(By.name("departmentId"), departmentVisibleTextOrNull);
        }
        // Click submit button inside the GET filter form (UI label is currently "Tìm").
        click(By.xpath("//form[@method='get']//button[@type='submit']"));
        return this;
    }

    public DoctorFormPage clickCreate() {
        click(By.linkText("Tạo mới"));
        return new DoctorFormPage(driver);
    }

    private WebElement rowByCode(String code) {
        return $(By.xpath("//table//tr[td[normalize-space()='" + code + "']]"));
    }

    public boolean hasDoctorCode(String code) {
        return driver.findElements(By.xpath("//table//tr/td[normalize-space()='" + code + "']")).size() > 0;
    }

    public DoctorFormPage clickEditByCode(String code) {
        WebElement row = rowByCode(code);
        // UI label is "Sửa"; use href contains to be resilient.
        row.findElement(By.xpath(".//a[contains(@href,'/Doctors/Edit') or contains(@href,'Doctors/Edit')]"))
                .click();
        return new DoctorFormPage(driver);
    }

    public DoctorsPage clickDeleteByCode(String code) {
        WebElement row = rowByCode(code);
        // UI label is "Vô hiệu hóa". Click the submit button inside the Delete form.
        click(row.findElement(By.xpath(".//form[contains(@action,'Delete')]//button[@type='submit']")));
        acceptConfirmIfPresent();
        $(By.xpath("//h2[normalize-space()='Bác sĩ']"));
        return this;
    }

    /**
     * Convenience helper for asserting a badge in a row (e.g. Active/Inactive).
     */
    public boolean rowHasBadge(String code, String badgeText) {
        WebElement row = rowByCode(code);
        return row.findElements(By.cssSelector("span.badge")).stream()
                .map(WebElement::getText)
                .anyMatch(t -> t != null && t.contains(badgeText));
    }

    public String departmentNameInRow(String code) {
        WebElement row = rowByCode(code);
        // columns: Id | Code | FullName | DepartmentName | IsActive | Actions
        return row.findElements(By.tagName("td")).get(3).getText().trim();
    }
}
