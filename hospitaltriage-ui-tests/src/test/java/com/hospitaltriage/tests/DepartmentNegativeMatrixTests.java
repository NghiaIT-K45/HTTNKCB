package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.DepartmentsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Department negative/boundary tests to increase coverage.
 */
public class DepartmentNegativeMatrixTests extends BaseUiTest {

    @Test
    public void createDepartment_codeWhitespace_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("admin");

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode("   ")
                .setName("Khoa whitespace code")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Mã khoa (Code) là bắt buộc"),
                "Whitespace code should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void createDepartment_nameWhitespace_shouldShowServiceError() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DW");

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName("   ")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Tên khoa (Name) là bắt buộc"),
                "Whitespace name should be rejected by service layer");

        logoutIfPossible();
    }

    @Test
    public void createDepartment_nameTooLong_shouldStayOnForm_andShowValidationHint() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("DL");
        String longName = "N".repeat(220); // > 200

        new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName(longName)
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains("Tạo khoa"), "Should remain on Create page when validation fails");
        Assert.assertTrue(src.contains("200") || src.contains("maximum") || src.contains("StringLength"),
                "Expected some max-length validation hint (message may vary by locale)");

        logoutIfPossible();
    }

    @Test
    public void editDepartment_duplicateCode_shouldShowError() {
        logoutIfPossible();
        loginAs("admin");

        String code1 = TestData.uniqueCode("DUPA");
        String code2 = TestData.uniqueCode("DUPB");

        DepartmentsPage list = new DepartmentsPage(driver).open();
        list.clickCreate()
                .setCode(code1)
                .setName("Khoa DUP A")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        list.open().clickCreate()
                .setCode(code2)
                .setName("Khoa DUP B")
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        // Edit B -> set code to A
        list.open().search(code2)
                .clickEditByCode(code2)
                .setCode(code1)
                .save();

        Assert.assertTrue(driver.getPageSource().contains("Mã khoa đã tồn tại"),
                "Should show duplicate code error when editing");

        // Cleanup-ish
        list.open().search(code1).clickDeleteByCode(code1);
        list.open().search(code2).clickDeleteByCode(code2);

        logoutIfPossible();
    }

    @Test
    public void editDepartment_invalidId_shouldRedirectToIndex_withErrorFlash() {
        logoutIfPossible();
        loginAs("admin");

        driver.get(Config.baseUrl() + "Departments/Edit/999999");

        Assert.assertTrue(driver.getCurrentUrl().contains("/Departments"),
                "Should redirect back to departments index");
        Assert.assertTrue(driver.getPageSource().contains("Không tìm thấy khoa")
                        || driver.getPageSource().contains("Id không hợp lệ"),
                "Expected an error flash message about invalid/non-existing department");

        logoutIfPossible();
    }

    @Test
    public void department_nameXssLikeInput_shouldBeHtmlEncodedInList() {
        logoutIfPossible();
        loginAs("admin");

        String code = TestData.uniqueCode("XSS");
        String xss = "<script>alert(1)</script>";

        DepartmentsPage list = new DepartmentsPage(driver)
                .open()
                .clickCreate()
                .setCode(code)
                .setName(xss)
                .setIsActive(true)
                .setIsGeneral(false)
                .save();

        list.open().search(code);

        String src = driver.getPageSource();
        Assert.assertTrue(src.contains(code), "Created department should exist");
        Assert.assertFalse(src.contains("<script>alert(1)</script>"),
                "Raw <script> should not appear in HTML source (should be encoded)");
        Assert.assertTrue(src.contains("&lt;script&gt;") || src.contains("&lt;SCRIPT&gt;") || src.contains("script"),
                "Expected encoded/script text to be present (encoding depends on renderer)");

        // Cleanup-ish
        list.clickDeleteByCode(code);

        logoutIfPossible();
    }
}
