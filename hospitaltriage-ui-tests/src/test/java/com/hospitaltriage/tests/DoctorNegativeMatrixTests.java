package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.DoctorsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Doctor negative/boundary tests for validating input, foreign keys, and errors.
 */
public class DoctorNegativeMatrixTests extends BaseUiTest {

    @Test
    public void createDoctor_codeWhitespace_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("admin");

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode("   ")
                .setFullName("BS whitespace code")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Mã bác sĩ (Code) là bắt buộc"),
                "Whitespace code should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void createDoctor_fullNameWhitespace_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DWDR");

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName("   ")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Họ tên bác sĩ là bắt buộc"),
                "Whitespace full name should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void createDoctor_invalidDepartmentId_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("BADDEPT");

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName("BS Invalid Dept")
                .forceDepartmentId("999999")
                .setIsActive(true)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Khoa khám không tồn tại"),
                "Should show invalid department error from service layer");

        logoutIfPossible();
    }

    @Test
    public void createDoctor_fullNameTooLong_shouldStayOnForm_andShowValidationHint() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DRLEN");
        String longName = "A".repeat(220); // > 200

        new DoctorsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setFullName(longName)
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tạo bác sĩ"), "Should remain on Create Doctor page");
        Assert.assertTrue(src.contains("200") || src.contains("maximum") || src.contains("StringLength"),
                "Expected some max-length validation hint (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void editDoctor_invalidId_shouldRedirectToIndex_withErrorFlash() {
        logoutIfPossible();
        loginAs("admin");

        driver.get(Config.baseUrl() + "Doctors/Edit/999999");

        Assert.assertTrue(driver.getCurrentUrl().contains("/Doctors"), "Should redirect back to doctors index");
        Assert.assertTrue(driver.getPageSource().contains("Không tìm thấy bác sĩ")
                        || driver.getPageSource().contains("Id không hợp lệ"),
                "Expected an error flash message about invalid/non-existing doctor");

        logoutIfPossible();
    }

    @Test
    public void editDoctor_duplicateCode_shouldShowError() {
        logoutIfPossible();
        loginAs("admin");

        String code1 = TestData.uniqueCode("DRDUPA");
        String code2 = TestData.uniqueCode("DRDUPB");

        DoctorsPage list = new DoctorsPage(driver).open();
        list.clickCreate()
                .setCode(code1)
                .setFullName("BS DUP A")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        list.open().clickCreate()
                .setCode(code2)
                .setFullName("BS DUP B")
                .selectDepartmentByVisibleText("NOI - Khoa Nội")
                .setIsActive(true)
                .save();

        // Edit B -> set code to A
        list.open().filter(code2, null)
                .clickEditByCode(code2)
                .setCode(code1)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Mã bác sĩ đã tồn tại"),
                "Should show duplicate doctor code error when editing");

        // Cleanup-ish (deactivate both)
        list.open().filter(code1, null).clickDeleteByCode(code1);
        list.open().filter(code2, null).clickDeleteByCode(code2);

        logoutIfPossible();
    }
}
