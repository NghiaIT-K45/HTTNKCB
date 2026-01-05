package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.IntakePage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

/**
 * Negative/boundary test cases for the Intake (patient registration) page.
 */
public class IntakeNegativeMatrixTests extends BaseUiTest {

    @Test
    public void intake_emptyFullName_shouldShowValidationError() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));

        new IntakePage(driver)
                .open()
                .setFullName("")
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Họ tên là bắt buộc") || src.contains("Họ tên"),
                "Expected required full name validation on Intake form");

        logoutIfPossible();
    }

    @Test
    public void intake_fullNameWhitespace_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));

        new IntakePage(driver)
                .open()
                .setFullName("   ")
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .submitExpectingError();

        Assert.assertTrue(driver.getPageSource().contains("Họ tên bệnh nhân là bắt buộc"),
                "Whitespace full name should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void intake_dateOfBirthEmpty_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("receptionist");

        new IntakePage(driver)
                .open()
                .setFullName("BN Missing DOB " + TestData.uniqueCode(""))
                .clearDateOfBirth()
                .selectGender("Nam")
                .submitExpectingError();

        Assert.assertTrue(driver.getPageSource().contains("Ngày sinh không hợp lệ"),
                "Empty DOB should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void intake_fullNameTooLong_shouldShowValidationHint() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));
        String longName = "N".repeat(220); // > 200

        new IntakePage(driver)
                .open()
                .setFullName(longName)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tiếp nhận bệnh nhân"), "Should remain on Intake page");
        Assert.assertTrue(src.contains("200") || src.contains("maximum") || src.contains("StringLength"),
                "Expected some max-length validation hint (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void intake_identityNumberTooLong_shouldShowValidationHint() {
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(20));
        String longId = "9".repeat(60); // > 50

        new IntakePage(driver)
                .open()
                .setFullName("BN Long ID " + TestData.uniqueCode(""))
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(longId)
                .submitExpectingError();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tiếp nhận bệnh nhân"), "Should remain on Intake page");
        Assert.assertTrue(src.contains("50") || src.contains("maximum") || src.contains("StringLength"),
                "Expected IdentityNumber max-length validation hint");

        logoutIfPossible();
    }

    @Test
    public void intake_accessDenied_forNurseRole() {
        logoutIfPossible();
        loginAs("nurse");

        driver.get(Config.baseUrl() + "Intake/Register");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Nurse should not access Intake/Register");
        Assert.assertTrue(driver.getPageSource().contains("Bạn không có quyền"),
                "AccessDenied message should be shown");

        logoutIfPossible();
    }
}
