package com.hospitaltriage.pages;

import com.hospitaltriage.config.Config;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public final class TriageListPage extends BasePage {
    public TriageListPage(WebDriver driver) {
        super(driver);
    }

    public TriageListPage open() {
        driver.get(Config.baseUrl() + "Triage");
        return this;
    }

    public TriageListPage filterDate(String yyyyMmDd) {
        // type=date sometimes keeps the old value; use robust clear
        clearInput(By.name("date"));
        type(By.name("date"), yyyyMmDd);
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

    public TriageProcessPage clickProcessByPatientName(String patientName) {
        WebElement row = rowByPatientName(patientName);
        row.findElement(By.linkText("Phân luồng")).click();
        return new TriageProcessPage(driver);
    }
}
