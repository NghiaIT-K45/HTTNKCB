package com.hospitaltriage.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Result page after successful intake.
 *
 * Matches Views/Intake/Result.cshtml in the ASP.NET MVC project.
 */
public final class IntakeResultPage extends BasePage {
    public IntakeResultPage(WebDriver driver) {
        super(driver);
        // Ensure page rendered
        $(By.cssSelector(".alert.alert-success"));
        $(By.xpath("//h5[normalize-space()='Thông tin lượt khám']"));
    }

    private String ddTextForDt(String dtLabel) {
        // <dt>Label</dt><dd>Value</dd>
        return $(By.xpath("//dt[normalize-space()='%s']/following-sibling::dd[1]".formatted(dtLabel)))
                .getText()
                .trim();
    }

    public int visitId() {
        return extractFirstInt(ddTextForDt("VisitId"));
    }

    public int queueNumber() {
        // Queue number is inside a badge
        String badge = $(By.xpath("//dt[normalize-space()='Số thứ tự']/following-sibling::dd[1]//span"))
                .getText()
                .trim();
        return Integer.parseInt(badge);
    }

    public int patientId() {
        return extractFirstInt(ddTextForDt("Patient"));
    }

    public boolean isNewPatient() {
        String line = ddTextForDt("Patient");
        return line.contains("(New)");
    }

    public boolean isExistingPatient() {
        String line = ddTextForDt("Patient");
        return line.contains("(Existing)");
    }

    public void startNewIntake() {
        click(By.linkText("Tiếp nhận tiếp"));
    }

    public void goToTriage() {
        click(By.linkText("Đi tới phân luồng (Nurse/Admin)"));
    }

    public void goToDashboard() {
        click(By.linkText("Dashboard"));
    }

    private static int extractFirstInt(String text) {
        Matcher m = Pattern.compile("(\\d+)").matcher(text);
        if (!m.find()) throw new IllegalStateException("Cannot parse int from: " + text);
        return Integer.parseInt(m.group(1));
    }
}
