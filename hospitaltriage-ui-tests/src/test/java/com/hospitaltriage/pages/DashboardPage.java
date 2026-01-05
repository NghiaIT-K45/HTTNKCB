package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

import java.util.List;

public final class DashboardPage extends BasePage {
    public DashboardPage(WebDriver driver) {
        super(driver);
    }

    public DashboardPage open() {
        driver.get(Config.baseUrl() + "Dashboard");
        return this;
    }

    public boolean canSelectDepartment() {
        return isPresent(By.cssSelector("select[name='departmentId']"));
    }

    public DashboardPage filterDepartment(String visibleText) {
        select(By.cssSelector("select[name='departmentId']"), visibleText);
        click(By.xpath("//button[normalize-space()='Lọc']"));
        return this;
    }

    private WebElement rowByPatientName(String patientName) {
        String xpath = "//table//tr[td[contains(normalize-space(),'%s')]]".formatted(patientName);
        return $(By.xpath(xpath));
    }

    public boolean hasPatient(String patientName) {
        return driver.findElements(By.xpath("//table//tr/td[contains(normalize-space(),'%s')]".formatted(patientName))).size() > 0;
    }

    public String departmentForPatient(String patientName) {
        WebElement row = rowByPatientName(patientName);
        List<WebElement> tds = row.findElements(By.tagName("td"));
        // columns in Views/Dashboard/Index.cshtml:
        // VisitId | Date | Queue | Patient | Department | Doctor | Status
        return tds.get(4).getText().trim();
    }

    public String doctorForPatient(String patientName) {
        WebElement row = rowByPatientName(patientName);
        List<WebElement> tds = row.findElements(By.tagName("td"));
        return tds.get(5).getText().trim();
    }

    public String statusForPatient(String patientName) {
        WebElement row = rowByPatientName(patientName);
        List<WebElement> tds = row.findElements(By.tagName("td"));
        return tds.get(6).getText().trim();
    }

    /**
     * Click "Bắt đầu khám" on the dashboard row (when status is WaitingDoctor) and accept the confirm.
     * After a successful status change, the visit is no longer in the WaitingList and the row disappears.
     */
    public DashboardPage startExaminationForPatient(String patientName) {
        WebElement row = rowByPatientName(patientName);
        WebElement btn = row.findElement(By.xpath(".//button[normalize-space()='Bắt đầu khám']"));
        click(btn);
        acceptConfirmIfPresent();
        // Wait for redirect/re-render
        wait.until(d -> d.getPageSource().contains("Cập nhật trạng thái") || !hasPatient(patientName));
        return this;
    }
}
