package com.hospitaltriage.tests;

import com.hospitaltriage.pages.IntakePage;
import com.hospitaltriage.pages.IntakeResultPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.DataProvider;
import org.testng.annotations.Test;

import java.time.LocalDate;

/**
 * Extra "Create New" test cases for Intake (patient registration):
 *  - positive: minimal required data across allowed roles
 *  - positive: IdentityNumber trimming works and matches existing patient
 *  - negative: boundary length checks for Phone/Insurance/Address
 */
public class IntakeCreateExtendedTests extends BaseUiTest {

    @DataProvider(name = "intakeCreateRoles")
    public Object[][] intakeCreateRoles() {
        return new Object[][]{
                {"receptionist"},
                {"manager"},
                {"admin"}
        };
    }

    @Test(dataProvider = "intakeCreateRoles")
    public void intake_createNewPatient_minimalRequiredFields_shouldSucceed(String roleKey) {
        logoutIfPossible();
        loginAs(roleKey);

        String name = TestData.vietnamesePatientName() + " (" + roleKey + ")";
        String dob = TestData.date(LocalDate.now().minusYears(30));

        IntakeResultPage result = new IntakePage(driver)
                .open()
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                // leave optional fields empty
                .submit();

        Assert.assertTrue(driver.getPageSource().contains("Tiếp nhận thành công"),
                "Expected success flash after intake");
        Assert.assertTrue(result.patientId() > 0, "PatientId should be > 0");
        Assert.assertTrue(result.visitId() > 0, "VisitId should be > 0");
        Assert.assertTrue(result.queueNumber() > 0, "QueueNumber should be > 0");

        logoutIfPossible();
    }

    @Test
    public void intake_identityNumberWithSpaces_shouldTrim_andMatchExistingOnSecondIntake() {
        logoutIfPossible();
        loginAs("receptionist");

        String name = TestData.vietnamesePatientName();
        String dob = TestData.date(LocalDate.now().minusYears(25));
        String id = TestData.identityNumber();
        String rawId = "  " + id + "  ";

        IntakeResultPage r1 = new IntakePage(driver)
                .open()
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(rawId)
                .submit();

        Assert.assertTrue(r1.isNewPatient(), "First intake should create new patient");
        int p1 = r1.patientId();

        r1.startNewIntake();

        IntakeResultPage r2 = new IntakePage(driver)
                .setFullName(name)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(id) // trimmed
                .submit();

        Assert.assertTrue(r2.isExistingPatient(), "Second intake should match existing patient");
        Assert.assertEquals(r2.patientId(), p1, "PatientId should stay the same after trimmed identity match");
        Assert.assertTrue(r2.queueNumber() > r1.queueNumber(), "Queue number should increase on next visit");

        logoutIfPossible();
    }

    @Test
    public void intake_negative_phoneTooLong_shouldShowValidationHint() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));
        String longPhone = "1".repeat(40); // > 30

        new IntakePage(driver)
                .open()
                .setFullName("BN LongPhone " + TestData.uniqueCode(""))
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .forcePhone(longPhone)
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tiếp nhận bệnh nhân"), "Should remain on Intake page");
        Assert.assertTrue(src.contains("30") || src.contains("maximum") || src.contains("StringLength"),
                "Expected max-length validation hint for Phone (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void intake_negative_insuranceTooLong_shouldShowValidationHint() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));
        String longInsurance = "B".repeat(80); // > 50

        new IntakePage(driver)
                .open()
                .setFullName("BN LongIns " + TestData.uniqueCode(""))
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .forceInsuranceCode(longInsurance)
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tiếp nhận bệnh nhân"), "Should remain on Intake page");
        Assert.assertTrue(src.contains("50") || src.contains("maximum") || src.contains("StringLength"),
                "Expected max-length validation hint for InsuranceCode");

        logoutIfPossible();
    }

    @Test
    public void intake_negative_addressTooLong_shouldShowValidationHint() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));
        String longAddress = "A".repeat(600); // > 500

        new IntakePage(driver)
                .open()
                .setFullName("BN LongAddr " + TestData.uniqueCode(""))
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .forceAddress(longAddress)
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tiếp nhận bệnh nhân"), "Should remain on Intake page");
        Assert.assertTrue(src.contains("500") || src.contains("maximum") || src.contains("StringLength"),
                "Expected max-length validation hint for Address");

        logoutIfPossible();
    }
}
