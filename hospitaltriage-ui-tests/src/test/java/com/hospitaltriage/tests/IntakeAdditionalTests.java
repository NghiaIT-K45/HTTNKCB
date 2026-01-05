package com.hospitaltriage.tests;

import com.hospitaltriage.pages.IntakePage;
import com.hospitaltriage.pages.IntakeResultPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

/**
 * Extra intake tests (validation + upsert logic variants).
 */
public class IntakeAdditionalTests extends BaseUiTest {

    @Test
    public void intake_requiredFullName_shouldShowValidation() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));

        IntakePage form = new IntakePage(driver).open();
        form.setFullName("")
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .submitExpectingError();

        Assert.assertTrue(driver.getPageSource().contains("Họ tên là bắt buộc")
                        || form.validationSummaryText().contains("Họ tên"),
                "Expected full name required validation");

        logoutIfPossible();
    }

    @Test
    public void intake_minimalRequiredFields_shouldSucceed() {
        logoutIfPossible();
        loginAs("receptionist");

        String name = "BN Minimal " + TestData.uniqueCode("");
        String dob = TestData.date(LocalDate.now().minusYears(22));

        IntakeResultPage result = new IntakePage(driver)
                .open()
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                // keep optional fields empty
                .submit();

        Assert.assertTrue(result.visitId() > 0, "VisitId should be present");
        Assert.assertTrue(result.queueNumber() > 0, "QueueNumber should be present");
        Assert.assertTrue(result.patientId() > 0, "PatientId should be present");

        logoutIfPossible();
    }

    @Test
    public void intake_upsert_byBasicInfo_whenIdentityNumberMissing_shouldReuseExistingPatient() {
        logoutIfPossible();
        loginAs("receptionist");

        String name = "BN UpsertBasic " + TestData.uniqueCode("");
        String dob = TestData.date(LocalDate.now().minusYears(28));
        String phone = TestData.phone();

        // 1st intake (no IdentityNumber)
        IntakeResultPage r1 = new IntakePage(driver)
                .open()
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setPhone(phone)
                .submit();

        Assert.assertTrue(r1.isNewPatient(), "First intake should create new patient");
        int patientId = r1.patientId();

        // 2nd intake with same name + dob + phone (still no IdentityNumber)
        r1.startNewIntake();
        IntakeResultPage r2 = new IntakePage(driver)
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setPhone(phone)
                .submit();

        Assert.assertTrue(r2.isExistingPatient(), "Second intake should reuse existing patient");
        Assert.assertEquals(r2.patientId(), patientId, "PatientId should be reused by basic info match");
        Assert.assertTrue(r2.queueNumber() > r1.queueNumber(), "Queue number should increase for new visit");

        logoutIfPossible();
    }
}
