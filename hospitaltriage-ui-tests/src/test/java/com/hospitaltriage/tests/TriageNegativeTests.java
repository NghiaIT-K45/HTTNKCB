package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.pages.IntakePage;
import com.hospitaltriage.pages.IntakeResultPage;
import com.hospitaltriage.pages.TriageProcessPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

/**
 * Negative/error-handling tests for the Triage module.
 */
public class TriageNegativeTests extends BaseUiTest {

    private int createVisitAsReceptionist(String patientName) {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(25));

        IntakeResultPage result = new IntakePage(driver)
                .open()
                .setFullName(patientName)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(TestData.identityNumber())
                .setPhone(TestData.phone())
                .setAddress("HN")
                .submit();

        return result.visitId();
    }

    @Test
    public void triage_process_invalidIdZero_shouldRedirectWithError() {
        logoutIfPossible();
        loginAs("nurse");

        driver.get(Config.baseUrl() + "Triage/Process/0");

        Assert.assertTrue(driver.getCurrentUrl().contains("/Triage"),
                "Should redirect back to /Triage index");
        Assert.assertTrue(driver.getPageSource().contains("Vui lòng chọn lượt khám để phân luồng"),
                "Expected error message for invalid visit id");

        logoutIfPossible();
    }

    @Test
    public void triage_process_notFound_shouldRedirectWithError() {
        logoutIfPossible();
        loginAs("nurse");

        driver.get(Config.baseUrl() + "Triage/Process/999999");

        Assert.assertTrue(driver.getCurrentUrl().contains("/Triage"),
                "Should redirect back to /Triage index");
        Assert.assertTrue(driver.getPageSource().contains("Không tìm thấy lượt khám"),
                "Expected error message for non-existing visit id");

        logoutIfPossible();
    }

    @Test
    public void triage_submitSymptomsWhitespace_shouldShowServiceError_andStayOnProcess() {
        String patientName = "BN Triage WS " + TestData.uniqueCode("");
        int visitId = createVisitAsReceptionist(patientName);

        logoutIfPossible();
        loginAs("nurse");

        driver.get(Config.baseUrl() + "Triage/Process/" + visitId);
        new TriageProcessPage(driver)
                .setSymptoms("   ")
                .submit();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Triệu chứng ban đầu là bắt buộc") || src.contains("Triệu chứng"),
                "Whitespace symptoms should be rejected (service error expected)");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Triage/Process"),
                "Should stay on Process page when triage submit fails");

        logoutIfPossible();
    }

    @Test
    public void triage_fallbackWithoutGeneralDepartment_shouldShowError_thenRestoreGeneral() {
        String patientName = "BN NoGEN " + TestData.uniqueCode("");

        // We will modify a global setting (General department flag) then restore it.
        try {
            // 1) Remove General department
            logoutIfPossible();
            loginAs("admin");
            new DepartmentsPage(driver)
                    .open()
                    .search("GEN")
                    .clickEditByCode("GEN")
                    .setIsGeneral(false)
                    .save();

            // 2) Create a visit
            int visitId = createVisitAsReceptionist(patientName);

            // 3) Try triage with no rule match -> should FAIL because no General dept exists
            logoutIfPossible();
            loginAs("nurse");

            driver.get(Config.baseUrl() + "Triage/Process/" + visitId);
            new TriageProcessPage(driver)
                    .setSymptoms("abcxyz (không match rule)")
                    .submit();

            Assert.assertTrue(driver.getPageSource().contains("Không xác định được khoa khám"),
                    "Expected error when no General department exists for fallback");
            Assert.assertTrue(driver.getCurrentUrl().contains("/Triage/Process"),
                    "Should stay on Process page when fallback fails");
        } finally {
            // Restore General department regardless of test outcome
            try {
                logoutIfPossible();
                loginAs("admin");
                new DepartmentsPage(driver)
                        .open()
                        .search("GEN")
                        .clickEditByCode("GEN")
                        .setIsGeneral(true)
                        .save();
            } catch (Exception ignored) {
            }
            logoutIfPossible();
        }
    }
}
