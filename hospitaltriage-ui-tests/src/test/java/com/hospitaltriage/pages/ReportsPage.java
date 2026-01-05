package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

import java.util.List;

public final class ReportsPage extends BasePage {
    public ReportsPage(WebDriver driver) {
        super(driver);
    }

    public ReportsPage open() {
        driver.get(Config.baseUrl() + "Reports");
        return this;
    }

    public ReportsPage setFromDate(String yyyyMmDd) {
        clearInput(By.id("Filter_FromDate"));
        type(By.id("Filter_FromDate"), yyyyMmDd);
        return this;
    }

    public ReportsPage clearFromDate() {
        clearInput(By.id("Filter_FromDate"));
        return this;
    }

    public ReportsPage setToDate(String yyyyMmDd) {
        clearInput(By.id("Filter_ToDate"));
        type(By.id("Filter_ToDate"), yyyyMmDd);
        return this;
    }

    public ReportsPage clearToDate() {
        clearInput(By.id("Filter_ToDate"));
        return this;
    }

    public ReportsPage selectDepartment(String visibleTextOrNull) {
        if (visibleTextOrNull != null) {
            select(By.id("Filter_DepartmentId"), visibleTextOrNull);
        }
        return this;
    }

    public ReportsPage runReport() {
        click(By.xpath("//button[normalize-space()='Chạy báo cáo']"));
        return this;
    }

    public boolean hasResult() {
        return driver.findElements(By.xpath("//h4[normalize-space()='Kết quả']")).size() > 0;
    }

    public int visitsPerDayRowCount() {
        List<WebElement> rows = driver.findElements(By.cssSelector("table.table tbody tr"));
        return rows.size();
    }

    public String exportCsvHrefOrEmpty() {
        List<WebElement> links = driver.findElements(By.linkText("Export CSV"));
        if (links.isEmpty()) return "";
        return links.get(0).getAttribute("href");
    }
}
