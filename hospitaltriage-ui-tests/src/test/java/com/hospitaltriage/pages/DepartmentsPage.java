package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

import java.util.List;

public final class DepartmentsPage extends BasePage {
    public DepartmentsPage(WebDriver driver) {
        super(driver);
    }

    public DepartmentsPage open() {
        driver.get(Config.baseUrl() + "Departments");
        return this;
    }

    public DepartmentsPage search(String keyword) {
        type(By.name("search"), keyword);
        // Click submit button inside the GET filter form to be robust.
        // (UI label is currently "Tìm".)
        click(By.xpath("//form[@method='get']//button[@type='submit']"));
        return this;
    }

    public DepartmentFormPage clickCreate() {
        click(By.linkText("Tạo mới"));
        return new DepartmentFormPage(driver);
    }

    private WebElement rowByCode(String code) {
        String xpath = "//table//tr[td[normalize-space()='%s']]".formatted(code);
        return $(By.xpath(xpath));
    }

    public boolean hasDepartmentCode(String code) {
        return driver.findElements(By.xpath("//table//tr/td[normalize-space()='%s']".formatted(code))).size() > 0;
    }

    public DepartmentFormPage clickEditByCode(String code) {
        WebElement row = rowByCode(code);
        // UI label is "Sửa"; use href contains to be resilient.
        row.findElement(By.xpath(".//a[contains(@href,'/Departments/Edit') or contains(@href,'Departments/Edit')]")).click();
        return new DepartmentFormPage(driver);
    }

    public DepartmentsPage clickDeleteByCode(String code) {
        WebElement row = rowByCode(code);
        // UI label is "Vô hiệu hóa". Click the submit button inside the Delete form.
        click(row.findElement(By.xpath(".//form[contains(@action,'Delete')]//button[@type='submit']")));
        acceptConfirmIfPresent();
        // After redirect, wait for list
        $(By.xpath("//h2[normalize-space()='Khoa khám']"));
        return this;
    }

    public boolean rowHasBadge(String code, String badgeText) {
        WebElement row = rowByCode(code);
        List<WebElement> badges = row.findElements(By.cssSelector("span.badge"));
        return badges.stream().map(WebElement::getText).anyMatch(t -> t != null && t.contains(badgeText));
    }
}
